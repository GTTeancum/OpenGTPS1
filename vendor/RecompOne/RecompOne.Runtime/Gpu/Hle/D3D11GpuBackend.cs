using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RecompOne.Runtime.Config;
using RecompOne.Runtime.Host.Window;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace RecompOne.Runtime.Hle;

/// <summary>
/// D3D11 implementation of the PS1 authored-command compositor. The native
/// renderer remains the sole owner of provenance-backed world geometry.
/// </summary>
internal sealed class D3D11GpuBackend : IGpuBackend, IDisposable
{
    sealed class DisplayTarget
    {
        public required D3D11Renderer.Texture Texture;
        public int X, Y, W, H, Margin;
        public bool Dirty;
        public long Stamp, LastDrawFrame;
        public int WideWidth => W + Margin * 2;
        public bool Covers(int x0, int y0, int x1, int y1) =>
            x0 <= X && x1 >= X + W - 1 &&
            y0 <= Y && y1 >= Y + H - 1;
        public bool Intersects(int x, int y, int width, int height) =>
            x < X + W && X < x + width &&
            y < Y + H && Y < y + height;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Vertex
    {
        public float X, Y;
        public uint Color;
        public int Clut, Texpage;
        public float U, V, PerspectiveW;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Constants
    {
        public Vector4 Geometry;
        public int TwAndX, TwAndY, TwOrX, TwOrY;
        public Vector4 Blend;
        public Vector4 OpaqueBlend;
        public float SetMask;
        public int CheckMask, Scale, TextureSmoothing;
    }

    const int Scale = 4;
    const int Width = VramShadow.Width * Scale;
    const int Height = VramShadow.Height * Scale;
    const int MaxVertices = 0x40000;
    // A single dynamic buffer forces the D3D11 driver to rename an 8 MiB
    // allocation while the GPU is still consuming its tail. Rotate whole
    // resources when the append cursor wraps so each buffer has multiple
    // seconds to retire before it is reused. A resource is discarded only on
    // its first map; later laps use NO_OVERWRITE after two complete resources
    // have retired, avoiding another large driver allocation/rename.
    const int VertexBufferCount = 3;
    static readonly bool TracePerformance =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_PERFORMANCE") == "1";

    const string Shader = """
        cbuffer State : register(b0) {
            float4 Geometry;
            int4 TextureWindow;
            float4 BlendParameters;
            float4 OpaqueBlendParameters;
            float SetMask;
            int CheckMask;
            int Scale;
            int TextureSmoothing;
        };
        Texture2D<float4> Vram : register(t0);
        Texture2D<float4> Destination : register(t1);

        struct VSIn {
            float2 position : POSITION;
            float4 color : COLOR0;
            int clut : CLUT;
            int texpage : TEXPAGE;
            float2 uv : TEXCOORD0;
            float perspectiveW : PERSPECTIVE;
        };
        struct PSIn {
            float4 position : SV_Position;
            noperspective float4 color : COLOR0;
            float2 uv : TEXCOORD0;
            nointerpolation int2 clutBase : TEXCOORD1;
            nointerpolation int2 pageBase : TEXCOORD2;
            nointerpolation int texMode : TEXCOORD3;
            nointerpolation int dither : TEXCOORD4;
            nointerpolation int smooth : TEXCOORD5;
        };
        PSIn VSMain(VSIn input) {
            PSIn output;
            float2 position = input.position + Geometry.xy;
            float2 p = float2(position.x * Geometry.z - 1.0,
                              1.0 - position.y * Geometry.w);
            float w = max(input.perspectiveW, 1.0);
            output.position = float4(p * w, 0.0, w);
            output.color = input.color;
            output.uv = input.uv;
            output.dither = (input.texpage >> 10) & 1;
            output.smooth = (input.texpage >> 11) & 1;
            if ((input.texpage & 0x8000) != 0) {
                output.texMode = 4;
                output.clutBase = 0;
                output.pageBase = 0;
            } else {
                output.texMode = (input.texpage >> 7) & 3;
                output.pageBase = int2((input.texpage & 15) * 64,
                    ((input.texpage >> 4) & 1) * 256);
                output.clutBase = int2((input.clut & 63) * 16,
                    (input.clut >> 6) & 511);
            }
            return output;
        }

        int U5(float value) { return (int)floor(value * 31.0 + 0.5); }
        float4 Fetch(int2 coordinate) {
            coordinate &= int2(1023, 511);
            return Vram.Load(int3(coordinate * Scale, 0));
        }
        int Fetch16(int2 coordinate) {
            float4 pixel = Fetch(coordinate);
            return U5(pixel.r) | (U5(pixel.g) << 5) |
                (U5(pixel.b) << 10) | ((int)ceil(pixel.a) << 15);
        }
        int2 ApplyWindow(int2 uv) {
            return ((uv & TextureWindow.xy) | TextureWindow.zw) & 255;
        }
        float4 TextureTexel(PSIn input, int2 uv) {
            uv = ApplyWindow(uv);
            if (input.texMode == 0) {
                int packed = Fetch16(int2(input.pageBase.x + (uv.x >> 2), input.pageBase.y + uv.y));
                int index = (packed >> ((uv.x & 3) << 2)) & 15;
                return Fetch(int2(input.clutBase.x + index, input.clutBase.y));
            }
            if (input.texMode == 1) {
                int packed = Fetch16(int2(input.pageBase.x + (uv.x >> 1), input.pageBase.y + uv.y));
                int index = (packed >> ((uv.x & 1) << 3)) & 255;
                return Fetch(int2(input.clutBase.x + index, input.clutBase.y));
            }
            return Fetch(input.pageBase + uv);
        }
        bool TransparentBlack(float4 pixel) {
            return all(pixel.rgb == 0.0) && pixel.a < 0.5;
        }
        float3 CubicHermite(float3 a, float3 b, float3 c, float3 d, float t) {
            float3 p = (d - c) - (a - b);
            float3 q = (a - b) - p;
            float3 r = c - a;
            return ((p * t + q) * t + r) * t + b;
        }
        float4 SmoothedTexture(PSIn input, float2 uv, float4 nearestTexel) {
            float2 p = uv - 0.5;
            int2 uv0 = (int2)floor(p);
            float2 fraction = frac(p);
            float3 rows[4];
            [unroll]
            for (int y = 0; y < 4; y++) {
                float4 s0 = TextureTexel(input, uv0 + int2(-1, y - 1));
                float4 s1 = TextureTexel(input, uv0 + int2( 0, y - 1));
                float4 s2 = TextureTexel(input, uv0 + int2( 1, y - 1));
                float4 s3 = TextureTexel(input, uv0 + int2( 2, y - 1));
                if (TransparentBlack(s0)) s0.rgb = nearestTexel.rgb;
                if (TransparentBlack(s1)) s1.rgb = nearestTexel.rgb;
                if (TransparentBlack(s2)) s2.rgb = nearestTexel.rgb;
                if (TransparentBlack(s3)) s3.rgb = nearestTexel.rgb;
                rows[y] = CubicHermite(s0.rgb, s1.rgb, s2.rgb, s3.rgb,
                    fraction.x);
            }
            return float4(saturate(CubicHermite(rows[0], rows[1], rows[2],
                rows[3], fraction.y)), nearestTexel.a);
        }
        static const int DitherTable[16] = {
            -4, 0, -3, 1, 2, -2, 3, -1,
            -3, 1, -4, 0, 3, -1, 2, -2
        };
        float3 Quantize5(int3 color, int dither, float2 position) {
            if (dither != 0) {
                int2 p = (int2)floor(position / (float)Scale);
                color = clamp(color + DitherTable[(p.y & 3) * 4 + (p.x & 3)], 0, 255);
            }
            return min(color >> 3, 31) / 31.0;
        }
        struct PSOutput {
            float4 color : SV_Target0;
            float4 blend : SV_Target1;
        };
        PSOutput PSMain(PSIn input) {
            int2 pixelPosition = (int2)input.position.xy;
            if (CheckMask != 0 &&
                Destination.Load(int3(pixelPosition, 0)).a >= 0.5) discard;

            float4 output;
            bool blendPixel;
            if (input.texMode == 4) {
                output = float4(Quantize5((int3)(input.color.rgb * 255.0 + 0.5),
                    input.dither, input.position.xy), SetMask);
                blendPixel = true;
            } else {
                int rawU = ddx(input.uv.x) < 0.0 ? (int)ceil(input.uv.x - 0.0001) : (int)floor(input.uv.x + 0.0001);
                int rawV = ddy(input.uv.y) < 0.0 ? (int)ceil(input.uv.y - 0.0001) : (int)floor(input.uv.y + 0.0001);
                float4 nearestTexel = TextureTexel(input, int2(rawU, rawV));
                if (TransparentBlack(nearestTexel)) discard;
                float4 texel = TextureSmoothing != 0 && input.smooth != 0
                    ? SmoothedTexture(input, input.uv, nearestTexel)
                    : nearestTexel;
                int3 texture8 = ((int3)(texel.rgb * 31.0 + 0.5)) << 3;
                int3 color8 = (texture8 * (int3)(input.color.rgb * 255.0 + 0.5)) >> 7;
                output = float4(Quantize5(color8, input.dither, input.position.xy),
                    max(texel.a, SetMask));
                blendPixel = nearestTexel.a >= 0.5;
            }
            PSOutput result;
            result.color = output;
            result.blend = blendPixel
                ? BlendParameters
                : OpaqueBlendParameters;
            return result;
        }
        """;

    readonly D3D11Renderer _renderer;
    readonly D3D11Renderer.Texture _vram;
    readonly D3D11Renderer.Texture _textureVram;
    readonly D3D11Renderer.Texture _snapshot;
    readonly D3D11Renderer.Texture _destinationSnapshot;
    readonly D3D11Renderer.Texture _present;
    readonly DisplayTarget?[] _displayTargets = new DisplayTarget?[2];
    readonly Vertex[] _vertices = new Vertex[MaxVertices];
    ID3D11VertexShader? _vertexShader;
    ID3D11PixelShader? _pixelShader;
    ID3D11InputLayout? _inputLayout;
    readonly ID3D11Buffer?[] _vertexBuffers =
        new ID3D11Buffer?[VertexBufferCount];
    readonly bool[] _vertexBufferPrimed = new bool[VertexBufferCount];
    ID3D11Buffer? _constantBuffer;
    ID3D11RasterizerState? _rasterizer;
    ID3D11BlendState? _opaqueBlend;
    ID3D11BlendState? _dualSourceAddBlend;
    ID3D11BlendState? _dualSourceReverseSubtractBlend;
    ID3D11DepthStencilState? _depthStencil;
    HleDrawEnv _environment;
    int _vertexCount;
    int _vertexBufferIndex;
    int _vertexBufferCursor;
    bool _transparent;
    int _blendMode, _setMask, _checkMask;
    int _twAndX, _twAndY, _twOrX, _twOrY;
    int _clipX0, _clipY0, _clipX1 = 1023, _clipY1 = 511;
    DisplayTarget? _batchTarget;
    long _targetStamp;
    long _frame;
    int _batchX0 = VramShadow.Width;
    int _batchY0 = VramShadow.Height;
    int _batchX1 = -1;
    int _batchY1 = -1;
    byte[] _upload = [];
    byte[] _readback = [];
    ushort[] _read16 = [];

    public bool Ready { get; private set; }

    public D3D11GpuBackend(D3D11Renderer renderer)
    {
        _renderer = renderer;
        _vram = renderer.CreateTexture(Width, Height, renderTarget: true);
        _textureVram = renderer.CreateTexture(Width, Height, renderTarget: true);
        _snapshot = renderer.CreateTexture(Width, Height, renderTarget: true);
        _destinationSnapshot = renderer.CreateTexture(1, 1, renderTarget: true);
        _present = renderer.CreateTexture(1, 1, renderTarget: true);
    }

    public void Initialize()
    {
        ReadOnlyMemory<byte> vs = Compiler.Compile(Shader, "VSMain",
            "ps1-compositor.hlsl", "vs_5_0",
            ShaderFlags.EnableStrictness | ShaderFlags.OptimizationLevel3);
        ReadOnlyMemory<byte> ps = Compiler.Compile(Shader, "PSMain",
            "ps1-compositor.hlsl", "ps_5_0",
            ShaderFlags.EnableStrictness | ShaderFlags.OptimizationLevel3);
        _vertexShader = _renderer.Device.CreateVertexShader(vs.Span);
        _pixelShader = _renderer.Device.CreatePixelShader(ps.Span);
        InputElementDescription[] elements =
        [
            new("POSITION", 0, Format.R32G32_Float, 0, 0),
            new("COLOR", 0, Format.R8G8B8A8_UNorm, 8, 0),
            new("CLUT", 0, Format.R32_SInt, 12, 0),
            new("TEXPAGE", 0, Format.R32_SInt, 16, 0),
            new("TEXCOORD", 0, Format.R32G32_Float, 20, 0),
            new("PERSPECTIVE", 0, Format.R32_Float, 28, 0),
        ];
        _inputLayout = _renderer.Device.CreateInputLayout(elements, vs.Span);
        for (int i = 0; i < _vertexBuffers.Length; i++)
            _vertexBuffers[i] = _renderer.Device.CreateBuffer(
                new BufferDescription(
                    MaxVertices * Unsafe.SizeOf<Vertex>(),
                    BindFlags.VertexBuffer,
                    ResourceUsage.Dynamic,
                    CpuAccessFlags.Write));
        _constantBuffer = _renderer.Device.CreateBuffer(new BufferDescription(
            Unsafe.SizeOf<Constants>(), BindFlags.ConstantBuffer,
            ResourceUsage.Dynamic, CpuAccessFlags.Write));
        _rasterizer = _renderer.Device.CreateRasterizerState(
            new RasterizerDescription(CullMode.None, FillMode.Solid)
            { ScissorEnable = true, DepthClipEnable = true });
        _opaqueBlend = _renderer.Device.CreateBlendState(BlendDescription.Opaque);
        _dualSourceAddBlend = _renderer.Device.CreateBlendState(
            CreateDualSourceBlend(BlendOperation.Add));
        _dualSourceReverseSubtractBlend = _renderer.Device.CreateBlendState(
            CreateDualSourceBlend(BlendOperation.ReverseSubtract));
        _depthStencil = _renderer.Device.CreateDepthStencilState(DepthStencilDescription.None);
        _renderer.Clear(_vram, new Color4(0, 0, 0, 0));
        _renderer.Clear(_textureVram, new Color4(0, 0, 0, 0));
        Ready = true;
        if (Environment.GetEnvironmentVariable(
                "RECOMPONE_D3D_COMPOSITOR_SELF_TEST") == "1")
            RunSelfTest();
        Console.WriteLine($"[Host] authored 2D compositor=D3D11 internal={Width}x{Height}");
    }

    static BlendDescription CreateDualSourceBlend(BlendOperation operation)
    {
        var description = new BlendDescription
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = false,
        };
        description.RenderTarget[0] = new RenderTargetBlendDescription
        {
            BlendEnable = true,
            SourceBlend = Blend.Source1Color,
            DestinationBlend = Blend.Source1Alpha,
            BlendOperation = operation,
            SourceBlendAlpha = Blend.One,
            DestinationBlendAlpha = Blend.Zero,
            BlendOperationAlpha = BlendOperation.Add,
            RenderTargetWriteMask = ColorWriteEnable.All,
        };
        return description;
    }

    void RunSelfTest()
    {
        HleDrawEnv previous = _environment;
        _environment = new HleDrawEnv
        {
            ClipX0 = 0,
            ClipY0 = 0,
            ClipX1 = VramShadow.Width - 1,
            ClipY1 = VramShadow.Height - 1,
        };
        DrawRect(new HleRect
        {
            X = 0,
            Y = 0,
            W = 4,
            H = 4,
            R = 255,
        }, new PrimFlags());
        Span<ushort> result = stackalloc ushort[1];
        ReadVram(0, 0, 1, 1, result);
        _renderer.Clear(_vram, new Color4(0, 0, 0, 0));
        _renderer.Clear(_textureVram, new Color4(0, 0, 0, 0));
        _batchX0 = VramShadow.Width;
        _batchY0 = VramShadow.Height;
        _batchX1 = -1;
        _batchY1 = -1;
        _environment = previous;
        if ((result[0] & 0x7FFF) != 0x001F)
            throw new InvalidOperationException(
                $"D3D11 authored compositor self-test failed: " +
                $"expected=0x001F actual=0x{result[0]:X4}");
        Console.WriteLine("[Host] authored 2D compositor self-test=passed");
    }

    public void SetDrawEnv(in HleDrawEnv environment) => _environment = environment;

    const int FramebufferSlackWidth = 64;
    const int FramebufferSlackHeight = 32;

    DisplayTarget? ClassifyTarget()
    {
        int clipWidth = _environment.ClipX1 - _environment.ClipX0 + 1;
        int clipHeight = _environment.ClipY1 - _environment.ClipY0 + 1;
        if (clipWidth <= 0 || clipHeight <= 0) return null;
        long bestStamp = -1;
        int x = 0, y = 0, width = 0, height = 0;
        for (int i = 0; i < GpuHle.RectCount; i++)
        {
            var display = GpuHle.GetRect(i);
            if (!display.Valid || display.W <= 0 || display.H <= 0 ||
                display.Stamp <= bestStamp)
                continue;
            bool clipInside = _environment.ClipX0 >= display.X &&
                _environment.ClipX0 + clipWidth <= display.X + display.W &&
                _environment.ClipY0 >= display.Y &&
                _environment.ClipY0 + clipHeight <= display.Y + display.H;
            bool clipIsFramebuffer = _environment.ClipX0 <= display.X &&
                _environment.ClipX0 + clipWidth >= display.X + display.W &&
                _environment.ClipY0 <= display.Y &&
                _environment.ClipY0 + clipHeight >= display.Y + display.H &&
                clipWidth - display.W <= FramebufferSlackWidth &&
                clipHeight - display.H <= FramebufferSlackHeight;
            if (!clipInside && !clipIsFramebuffer) continue;
            bestStamp = display.Stamp;
            x = clipInside ? display.X : _environment.ClipX0;
            y = clipInside ? display.Y : _environment.ClipY0;
            width = clipInside ? display.W : clipWidth;
            height = clipInside ? display.H : clipHeight;
        }
        return bestStamp < 0 ? null : GetOrCreateTarget(x, y, width, height);
    }

    DisplayTarget GetOrCreateTarget(int x, int y, int width, int height)
    {
        int slot = -1;
        for (int i = 0; i < _displayTargets.Length; i++)
        {
            DisplayTarget? target = _displayTargets[i];
            if (target == null || target.X != x || target.Y != y) continue;
            bool sameWidth = target.W == width;
            bool compatibleHeight = target.H >= height &&
                target.H - height <= FramebufferSlackHeight;
            if (sameWidth && compatibleHeight &&
                target.Margin == GpuHle.WideMargin(target.W))
            {
                target.Stamp = ++_targetStamp;
                return target;
            }
            slot = i;
            break;
        }
        if (slot < 0)
        {
            slot = 0;
            for (int i = 1; i < _displayTargets.Length; i++)
                if (_displayTargets[i] == null ||
                    _displayTargets[slot] != null &&
                    _displayTargets[i]!.Stamp < _displayTargets[slot]!.Stamp)
                {
                    slot = i;
                    break;
                }
        }
        if (_displayTargets[slot] is { } old)
        {
            // ClassifyTarget runs before Begin can compare the next batch.
            // Drain vertices that still reference this target before replacing
            // its texture, otherwise Flush would submit through disposed views.
            if (ReferenceEquals(_batchTarget, old) && _vertexCount > 0)
                Flush();
            if (old.Dirty) Writeback(old);
            _renderer.DisposeTexture(old.Texture);
        }
        int margin = GpuHle.WideMargin(width);
        var fresh = new DisplayTarget
        {
            Texture = _renderer.CreateTexture(
                (width + margin * 2) * Scale, height * Scale,
                renderTarget: true),
            X = x, Y = y, W = width, H = height, Margin = margin,
            Stamp = ++_targetStamp, LastDrawFrame = _frame,
        };
        _displayTargets[slot] = fresh;
        SyncTargetFromVram(fresh, x, y, width, height);
        return fresh;
    }

    bool Matches(in PrimFlags flags, DisplayTarget? target)
    {
        int andX = ~(_environment.TwMaskX * 8) & 255;
        int andY = ~(_environment.TwMaskY * 8) & 255;
        int orX = (_environment.TwOffX & _environment.TwMaskX) * 8;
        int orY = (_environment.TwOffY & _environment.TwMaskY) * 8;
        return _batchTarget == target &&
            _transparent == flags.SemiTrans && _blendMode == flags.BlendMode &&
            _setMask == (_environment.SetMask ? 1 : 0) &&
            _checkMask == (_environment.CheckMask ? 1 : 0) &&
            _twAndX == andX && _twAndY == andY && _twOrX == orX && _twOrY == orY &&
            _clipX0 == _environment.ClipX0 && _clipY0 == _environment.ClipY0 &&
            _clipX1 == _environment.ClipX1 && _clipY1 == _environment.ClipY1;
    }

    void Begin(in PrimFlags flags, int needed)
    {
        DisplayTarget? target = ClassifyTarget();
        if (_vertexCount > 0 && !Matches(flags, target)) Flush();
        if (_vertexCount + needed > MaxVertices) Flush();
        CheckTextureFeedback(in flags);
        _batchTarget = target;
        _transparent = flags.SemiTrans;
        _blendMode = flags.BlendMode;
        _setMask = _environment.SetMask ? 1 : 0;
        _checkMask = _environment.CheckMask ? 1 : 0;
        _twAndX = ~(_environment.TwMaskX * 8) & 255;
        _twAndY = ~(_environment.TwMaskY * 8) & 255;
        _twOrX = (_environment.TwOffX & _environment.TwMaskX) * 8;
        _twOrY = (_environment.TwOffY & _environment.TwMaskY) * 8;
        _clipX0 = _environment.ClipX0; _clipY0 = _environment.ClipY0;
        _clipX1 = _environment.ClipX1; _clipY1 = _environment.ClipY1;
    }

    void CheckTextureFeedback(in PrimFlags flags)
    {
        if (!flags.Textured) return;
        int pageX = (flags.TPage & 15) * 64;
        int pageY = ((flags.TPage >> 4) & 1) * 256;
        int mode = (flags.TPage >> 7) & 3;
        int pageWidth = mode switch { 0 => 64, 1 => 128, _ => 256 };
        WritebackFeedbackRegion(pageX, pageY, pageWidth, 256);
        if (mode < 2)
        {
            int clutX = (flags.Clut & 63) * 16;
            int clutY = (flags.Clut >> 6) & 511;
            WritebackFeedbackRegion(
                clutX, clutY, mode == 0 ? 16 : 256, 1);
        }
    }

    void WritebackFeedbackRegion(int x, int y, int width, int height)
    {
        if (_vertexCount > 0 && _batchTarget != null &&
            _batchTarget.Intersects(x, y, width, height))
            Flush();
        foreach (DisplayTarget? target in _displayTargets)
            if (target is { Dirty: true } &&
                target.Intersects(x, y, width, height))
            {
                Flush();
                Writeback(target);
            }
    }

    void IncludeDirty(float x0, float y0, float x1, float y1)
    {
        _batchX0 = Math.Min(_batchX0, Math.Clamp(
            (int)MathF.Floor(MathF.Min(x0, x1)), 0, VramShadow.Width - 1));
        _batchY0 = Math.Min(_batchY0, Math.Clamp(
            (int)MathF.Floor(MathF.Min(y0, y1)), 0, VramShadow.Height - 1));
        _batchX1 = Math.Max(_batchX1, Math.Clamp(
            (int)MathF.Ceiling(MathF.Max(x0, x1)), 0, VramShadow.Width - 1));
        _batchY1 = Math.Max(_batchY1, Math.Clamp(
            (int)MathF.Ceiling(MathF.Max(y0, y1)), 0, VramShadow.Height - 1));
    }

    Vertex MakeVertex(in HleVertex vertex, in PrimFlags flags,
        bool dither, bool perspective, bool ui = false)
    {
        uint color = flags.Textured && flags.RawTexture
            ? 0xFF808080u
            : (uint)(vertex.R | vertex.G << 8 | vertex.B << 16 | 0xFF << 24);
        int texpage = flags.Textured ? flags.TPage & 0x1FF : 0x8000;
        if (dither) texpage |= 0x400;
        if (ConfigManager.View.TextureSmoothing && flags.Textured &&
            (!flags.RawTexture || !ui))
            texpage |= 0x800;
        return new Vertex
        {
            X = vertex.X, Y = vertex.Y, Color = color,
            Clut = flags.Clut & 0x7FFF, Texpage = texpage,
            U = vertex.U, V = vertex.V,
            PerspectiveW = perspective ? MathF.Max(1, vertex.Z) : 1,
        };
    }

    public void DrawTri(in HleVertex a, in HleVertex b,
        in HleVertex c, in PrimFlags flags)
    {
        bool dither = _environment.Dither &&
            (flags.Gouraud || flags.Textured && !flags.RawTexture);
        bool coherentDepth = flags.Textured && a.HasGteZ && b.HasGteZ && c.HasGteZ &&
            MathF.Min(a.Z, MathF.Min(b.Z, c.Z)) > 0;
        bool perspective = ConfigManager.View.PerspectiveCorrectTextures && coherentDepth;
        Begin(flags, 3);
        _vertices[_vertexCount++] = MakeVertex(a, flags, dither, perspective);
        _vertices[_vertexCount++] = MakeVertex(b, flags, dither, perspective);
        _vertices[_vertexCount++] = MakeVertex(c, flags, dither, perspective);
        IncludeDirty(MathF.Min(a.X, MathF.Min(b.X, c.X)),
            MathF.Min(a.Y, MathF.Min(b.Y, c.Y)),
            MathF.Max(a.X, MathF.Max(b.X, c.X)),
            MathF.Max(a.Y, MathF.Max(b.Y, c.Y)));
    }

    public void DrawRect(in HleRect rectangle, in PrimFlags flags)
    {
        Begin(flags, 6);
        var a = new HleVertex { X = rectangle.X, Y = rectangle.Y, R = rectangle.R, G = rectangle.G, B = rectangle.B, U = rectangle.U, V = rectangle.V };
        var b = a; b.X += rectangle.W; b.U += (short)rectangle.W;
        var c = a; c.Y += rectangle.H; c.V += (short)rectangle.H;
        var d = b; d.Y += rectangle.H; d.V += (short)rectangle.H;
        _vertices[_vertexCount++] = MakeVertex(a, flags, false, false, true);
        _vertices[_vertexCount++] = MakeVertex(b, flags, false, false, true);
        _vertices[_vertexCount++] = MakeVertex(c, flags, false, false, true);
        _vertices[_vertexCount++] = MakeVertex(b, flags, false, false, true);
        _vertices[_vertexCount++] = MakeVertex(d, flags, false, false, true);
        _vertices[_vertexCount++] = MakeVertex(c, flags, false, false, true);
        IncludeDirty(rectangle.X, rectangle.Y,
            rectangle.X + rectangle.W, rectangle.Y + rectangle.H);
    }

    public void DrawLine(in HleVertex a, in HleVertex b, in PrimFlags flags)
    {
        Begin(flags, 6);
        float x1 = a.X, y1 = a.Y, x2 = b.X, y2 = b.Y;
        float dx = x2 - x1, dy = y2 - y1;
        float xo, yo;
        if (dx == 0 && dy == 0) { xo = yo = 1; }
        else if (Math.Abs(dx) > Math.Abs(dy)) { xo = 0; yo = 1; if (dx > 0) x2++; else x1++; }
        else { xo = 1; yo = 0; if (dy > 0) y2++; else y1++; }
        AddLineVertex(x1, y1, a, flags); AddLineVertex(x2, y2, b, flags);
        AddLineVertex(x2 + xo, y2 + yo, b, flags);
        AddLineVertex(x2 + xo, y2 + yo, b, flags);
        AddLineVertex(x1 + xo, y1 + yo, a, flags); AddLineVertex(x1, y1, a, flags);
        IncludeDirty(MathF.Min(x1, x2), MathF.Min(y1, y2),
            MathF.Max(x1 + xo, x2 + xo), MathF.Max(y1 + yo, y2 + yo));
    }

    void AddLineVertex(float x, float y, in HleVertex source, in PrimFlags flags)
    {
        var vertex = source; vertex.X = x; vertex.Y = y;
        _vertices[_vertexCount++] = MakeVertex(vertex, flags, _environment.Dither, false);
    }

    public unsafe void Flush()
    {
        if (_vertexCount == 0) return;
        long started = TracePerformance ? Stopwatch.GetTimestamp() : 0;
        var context = _renderer.Context;
        DisplayTarget? target = _batchTarget;
        D3D11Renderer.Texture destination = target?.Texture ?? _vram;
        context.UnsetRenderTargets();
        context.PSSetShaderResource(0, null!);
        context.PSSetShaderResource(1, null!);
        if (_checkMask != 0)
        {
            _renderer.EnsureTexture(_destinationSnapshot,
                destination.Width, destination.Height, renderTarget: true);
            context.CopyResource(
                _destinationSnapshot.Resource, destination.Resource);
        }
        context.OMSetRenderTargets(destination.Target!);
        context.OMSetBlendState(
            _transparent ? _dualSourceAddBlend : _opaqueBlend);
        context.OMSetDepthStencilState(_depthStencil);
        context.RSSetState(_rasterizer);
        context.RSSetViewport(new Viewport(
            0, 0, destination.Width, destination.Height));
        int scissorX0 = _clipX0;
        int scissorY0 = _clipY0;
        int scissorX1 = _clipX1 + 1;
        int scissorY1 = _clipY1 + 1;
        if (target != null)
        {
            scissorX0 = _clipX0 - target.X + target.Margin;
            scissorY0 = _clipY0 - target.Y;
            scissorX1 = _clipX1 - target.X + target.Margin + 1;
            scissorY1 = _clipY1 - target.Y + 1;
            if (target.Margin > 0 && _clipX0 <= target.X &&
                _clipX1 >= target.X + target.W - 1)
            {
                scissorX0 = 0;
                scissorX1 = target.WideWidth;
            }
        }
        context.RSSetScissorRect(
            Math.Clamp(scissorX0 * Scale, 0, destination.Width),
            Math.Clamp(scissorY0 * Scale, 0, destination.Height),
            Math.Clamp(scissorX1 * Scale, 0, destination.Width),
            Math.Clamp(scissorY1 * Scale, 0, destination.Height));
        int vertexStride = Unsafe.SizeOf<Vertex>();
        if (_vertexBufferCursor + _vertexCount > MaxVertices)
        {
            _vertexBufferCursor = 0;
            _vertexBufferIndex =
                (_vertexBufferIndex + 1) % _vertexBuffers.Length;
        }
        ID3D11Buffer vertexBuffer = _vertexBuffers[_vertexBufferIndex]!;
        int vertexBufferOffset = _vertexBufferCursor * vertexStride;
        MapMode vertexMapMode = _vertexBufferCursor == 0 &&
            !_vertexBufferPrimed[_vertexBufferIndex]
            ? MapMode.WriteDiscard
            : MapMode.WriteNoOverwrite;
        long beforeVertexMap = TracePerformance ? Stopwatch.GetTimestamp() : 0;
        MappedSubresource vertexMap = context.Map(
            vertexBuffer,
            vertexMapMode,
            Vortice.Direct3D11.MapFlags.None);
        _vertexBufferPrimed[_vertexBufferIndex] = true;
        long afterVertexMap = TracePerformance ? Stopwatch.GetTimestamp() : 0;
        fixed (Vertex* source = _vertices)
            Buffer.MemoryCopy(
                source,
                (void*)(vertexMap.DataPointer + vertexBufferOffset),
                (MaxVertices - _vertexBufferCursor) * vertexStride,
                _vertexCount * vertexStride);
        context.Unmap(vertexBuffer);
        long afterVertexUpload = TracePerformance ? Stopwatch.GetTimestamp() : 0;
        float sourceFactor = _blendMode switch
        {
            0 => 0.5f,
            2 => 0f,
            3 => 0.25f,
            _ => 1f,
        };
        float destinationFactor = _blendMode == 0 ? 0.5f : 1f;
        var constants = new Constants
        {
            Geometry = target == null
                ? new Vector4(0, 0,
                    2f / VramShadow.Width, 2f / VramShadow.Height)
                : new Vector4(target.Margin - target.X, -target.Y,
                    2f / target.WideWidth, 2f / target.H),
            TwAndX = _twAndX, TwAndY = _twAndY,
            TwOrX = _twOrX, TwOrY = _twOrY,
            Blend = new Vector4(sourceFactor, sourceFactor, sourceFactor,
                destinationFactor),
            OpaqueBlend = new Vector4(1, 1, 1, 0),
            SetMask = _setMask,
            CheckMask = _checkMask,
            Scale = Scale,
            TextureSmoothing = ConfigManager.View.TextureSmoothing ? 1 : 0,
        };
        UploadConstants(context, in constants);
        long afterConstants = TracePerformance ? Stopwatch.GetTimestamp() : 0;
        context.IASetInputLayout(_inputLayout);
        context.IASetVertexBuffer(
            0, vertexBuffer, vertexStride, vertexBufferOffset);
        context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        context.VSSetShader(_vertexShader);
        context.VSSetConstantBuffer(0, _constantBuffer);
        context.PSSetShader(_pixelShader);
        context.PSSetConstantBuffer(0, _constantBuffer);
        context.PSSetShaderResource(0, _textureVram.View);
        if (_checkMask != 0)
            context.PSSetShaderResource(1, _destinationSnapshot.View);
        context.Draw(_vertexCount, 0);
        if (_transparent && _blendMode == 2)
        {
            constants.Blend = new Vector4(1, 1, 1, 1);
            constants.OpaqueBlend = new Vector4(0, 0, 0, 1);
            UploadConstants(context, in constants);
            context.OMSetBlendState(_dualSourceReverseSubtractBlend);
            context.Draw(_vertexCount, 0);
        }
        long afterDraw = TracePerformance ? Stopwatch.GetTimestamp() : 0;
        context.PSSetShaderResource(0, null!);
        context.PSSetShaderResource(1, null!);
        context.UnsetRenderTargets();
        int dirtyX0 = Math.Max(_batchX0, _clipX0);
        int dirtyY0 = Math.Max(_batchY0, _clipY0);
        int dirtyX1 = Math.Min(_batchX1, _clipX1);
        int dirtyY1 = Math.Min(_batchY1, _clipY1);
        if (target != null)
        {
            target.Dirty = true;
            target.LastDrawFrame = _frame;
        }
        else if (dirtyX0 <= dirtyX1 && dirtyY0 <= dirtyY1)
            _renderer.CopyRegion(_vram, _textureVram,
                dirtyX0 * Scale, dirtyY0 * Scale,
                dirtyX0 * Scale, dirtyY0 * Scale,
                (dirtyX1 - dirtyX0 + 1) * Scale,
                (dirtyY1 - dirtyY0 + 1) * Scale);
        long afterCopy = TracePerformance ? Stopwatch.GetTimestamp() : 0;
        if (TracePerformance &&
            Stopwatch.GetElapsedTime(started, afterCopy).TotalMilliseconds >= 40.0)
        {
            Console.Error.WriteLine(
                $"[GPU-Long-Flush] poll={Host.InputManager.CurrentPoll} " +
                $"vertices={_vertexCount} buffer={_vertexBufferIndex} " +
                $"cursor={_vertexBufferCursor} " +
                $"mapMode={vertexMapMode} target={destination.Width}x{destination.Height} " +
                $"setupMs={Stopwatch.GetElapsedTime(started, beforeVertexMap).TotalMilliseconds:F3} " +
                $"mapMs={Stopwatch.GetElapsedTime(beforeVertexMap, afterVertexMap).TotalMilliseconds:F3} " +
                $"uploadMs={Stopwatch.GetElapsedTime(afterVertexMap, afterVertexUpload).TotalMilliseconds:F3} " +
                $"constantsMs={Stopwatch.GetElapsedTime(afterVertexUpload, afterConstants).TotalMilliseconds:F3} " +
                $"drawMs={Stopwatch.GetElapsedTime(afterConstants, afterDraw).TotalMilliseconds:F3} " +
                $"copyMs={Stopwatch.GetElapsedTime(afterDraw, afterCopy).TotalMilliseconds:F3}");
        }
        _batchX0 = VramShadow.Width;
        _batchY0 = VramShadow.Height;
        _batchX1 = -1;
        _batchY1 = -1;
        _vertexBufferCursor += _vertexCount;
        _vertexCount = 0;
    }

    unsafe void UploadConstants(ID3D11DeviceContext context,
        in Constants constants)
    {
        MappedSubresource mapped = context.Map(_constantBuffer!,
            MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None);
        *(Constants*)mapped.DataPointer = constants;
        context.Unmap(_constantBuffer!);
    }

    void Writeback(DisplayTarget target)
    {
        // A 480-line interlaced display target can extend beyond the 512-line
        // physical VRAM when its field starts in the lower half.  Keep the
        // full virtual target for composition, but only synchronize the part
        // that has physical VRAM backing.
        int x0 = Math.Max(0, target.X);
        int y0 = Math.Max(0, target.Y);
        int x1 = Math.Min(VramShadow.Width, target.X + target.W);
        int y1 = Math.Min(VramShadow.Height, target.Y + target.H);
        if (x0 >= x1 || y0 >= y1)
        {
            target.Dirty = false;
            return;
        }
        int width = (x1 - x0) * Scale;
        int height = (y1 - y0) * Scale;
        int sourceX = (target.Margin + x0 - target.X) * Scale;
        int sourceY = (y0 - target.Y) * Scale;
        int destinationX = x0 * Scale;
        int destinationY = y0 * Scale;
        _renderer.CopyRegion(target.Texture, _vram,
            sourceX, sourceY, destinationX, destinationY, width, height);
        _renderer.CopyRegion(target.Texture, _textureVram,
            sourceX, sourceY, destinationX, destinationY, width, height);
        target.Dirty = false;
    }

    void WritebackDirtyIntersecting(int x, int y, int width, int height)
    {
        foreach (DisplayTarget? target in _displayTargets)
            if (target is { Dirty: true } &&
                target.Intersects(x, y, width, height))
                Writeback(target);
    }

    void SyncTargetFromVram(DisplayTarget target,
        int x, int y, int width, int height)
    {
        int x0 = Math.Max(0, Math.Max(x, target.X));
        int y0 = Math.Max(0, Math.Max(y, target.Y));
        int x1 = Math.Min(VramShadow.Width,
            Math.Min(x + width, target.X + target.W));
        int y1 = Math.Min(VramShadow.Height,
            Math.Min(y + height, target.Y + target.H));
        if (x0 >= x1 || y0 >= y1) return;
        _renderer.CopyRegion(_vram, target.Texture,
            x0 * Scale, y0 * Scale,
            (x0 - target.X + target.Margin) * Scale,
            (y0 - target.Y) * Scale,
            (x1 - x0) * Scale, (y1 - y0) * Scale);
    }

    void SyncTargetsFromVram(int x, int y, int width, int height)
    {
        foreach (DisplayTarget? target in _displayTargets)
            if (target != null && target.Intersects(x, y, width, height))
                SyncTargetFromVram(target, x, y, width, height);
    }

    public void FillRect(int x, int y, int width, int height, ushort color15)
    {
        Flush();
        UploadSolid(x, y, width, height, color15);
    }

    void UploadSolid(int x, int y, int width, int height, ushort color15)
    {
        int sw = Math.Max(0, width * Scale), sh = Math.Max(0, height * Scale);
        if (sw == 0 || sh == 0) return;
        int bytes = sw * sh * 4;
        if (_upload.Length < bytes) _upload = new byte[bytes];
        byte r = Expand5(color15 & 31), g = Expand5((color15 >> 5) & 31), b = Expand5((color15 >> 10) & 31);
        byte a = (color15 & 0x8000) != 0 ? (byte)255 : (byte)0;
        for (int i = 0; i < bytes; i += 4) { _upload[i] = r; _upload[i + 1] = g; _upload[i + 2] = b; _upload[i + 3] = a; }
        _renderer.UploadRegion(_vram, x * Scale, y * Scale, sw, sh, _upload.AsSpan(0, bytes));
        _renderer.UploadRegion(_textureVram, x * Scale, y * Scale,
            sw, sh, _upload.AsSpan(0, bytes));
        foreach (DisplayTarget? target in _displayTargets)
        {
            if (target == null || !target.Intersects(x, y, width, height))
                continue;
            if (target.Covers(x, y, x + width - 1, y + height - 1))
            {
                _renderer.Clear(target.Texture,
                    new Color4(r / 255f, g / 255f, b / 255f, a / 255f));
                target.Dirty = false;
                target.LastDrawFrame = _frame;
            }
            else
                SyncTargetFromVram(target, x, y, width, height);
        }
    }

    public void WriteVram(int x, int y, int width, int height, ReadOnlySpan<ushort> pixels)
    {
        Flush();
        int sw = width * Scale, sh = height * Scale, bytes = sw * sh * 4;
        if (_upload.Length < bytes) _upload = new byte[bytes];
        for (int py = 0; py < height; py++)
        for (int sy = 0; sy < Scale; sy++)
        for (int px = 0; px < width; px++)
        {
            ushort value = pixels[py * width + px];
            int row = (py * Scale + sy) * sw * 4;
            for (int sx = 0; sx < Scale; sx++)
            {
                int offset = row + (px * Scale + sx) * 4;
                _upload[offset] = Expand5(value & 31);
                _upload[offset + 1] = Expand5((value >> 5) & 31);
                _upload[offset + 2] = Expand5((value >> 10) & 31);
                _upload[offset + 3] = (value & 0x8000) != 0 ? (byte)255 : (byte)0;
            }
        }
        _renderer.UploadRegion(_vram, x * Scale, y * Scale, sw, sh, _upload.AsSpan(0, bytes));
        _renderer.UploadRegion(_textureVram, x * Scale, y * Scale,
            sw, sh, _upload.AsSpan(0, bytes));
        SyncTargetsFromVram(x, y, width, height);
    }

    public void CopyVram(int sx, int sy, int dx, int dy, int width, int height)
    {
        Flush();
        WritebackDirtyIntersecting(sx, sy, width, height);
        int sw = width * Scale, sh = height * Scale;
        _renderer.CopyRegion(_vram, _snapshot, sx * Scale, sy * Scale, 0, 0, sw, sh);
        _renderer.CopyRegion(_snapshot, _vram, 0, 0, dx * Scale, dy * Scale, sw, sh);
        _renderer.CopyRegion(_snapshot, _textureVram, 0, 0,
            dx * Scale, dy * Scale, sw, sh);
        SyncTargetsFromVram(dx, dy, width, height);
    }

    public void ReadVram(int x, int y, int width, int height, Span<ushort> pixels)
    {
        if (width <= 0 || height <= 0 || pixels.IsEmpty) return;
        Flush();
        WritebackDirtyIntersecting(x, y, width, height);
        int rows = Math.Min(height, pixels.Length / Math.Max(1, width));
        int destinationRow = 0;
        while (destinationRow < rows)
        {
            int sourceY = (y + destinationRow) & 511;
            int chunkRows = Math.Min(rows - destinationRow, 512 - sourceY);
            int destinationColumn = 0;
            while (destinationColumn < width)
            {
                int sourceX = (x + destinationColumn) & 1023;
                int chunkColumns = Math.Min(width - destinationColumn,
                    1024 - sourceX);
                int scaledWidth = chunkColumns * Scale;
                int scaledHeight = chunkRows * Scale;
                int required = checked(scaledWidth * scaledHeight * 4);
                if (_readback.Length < required)
                    _readback = new byte[required];
                _renderer.ReadbackRegion(_vram,
                    sourceX * Scale, sourceY * Scale,
                    scaledWidth, scaledHeight,
                    _readback.AsSpan(0, required));
                for (int row = 0; row < chunkRows; row++)
                for (int column = 0; column < chunkColumns; column++)
                {
                    int offset = ((row * Scale) * scaledWidth +
                        column * Scale) * 4;
                    int mask = _readback[offset + 3] >= 128 ? 0x8000 : 0;
                    int destination = (destinationRow + row) * width +
                        destinationColumn + column;
                    pixels[destination] = (ushort)((_readback[offset] >> 3) |
                        ((_readback[offset + 1] >> 3) << 5) |
                        ((_readback[offset + 2] >> 3) << 10) | mask);
                }
                destinationColumn += chunkColumns;
            }
            destinationRow += chunkRows;
        }
    }

    public void Present(in HleDispEnv display) =>
        PresentDisplay(display.X, display.Y, display.W, display.H, display.Rgb24);

    public (D3D11Renderer.Texture texture, int width, int height, float aspect)
        PresentDisplay(int x, int y, int width, int height, bool rgb24 = false)
    {
        _frame++;
        Flush();
        foreach (DisplayTarget? target in _displayTargets)
            if (target is { Dirty: true }) Writeback(target);
        if (!rgb24)
        {
            DisplayTarget? source = null;
            foreach (DisplayTarget? target in _displayTargets)
            {
                if (target == null || _frame - target.LastDrawFrame > 4 ||
                    x < target.X || y < target.Y ||
                    x + width > target.X + target.W ||
                    y + height > target.Y + target.H)
                    continue;
                if (source == null ||
                    target.LastDrawFrame > source.LastDrawFrame)
                    source = target;
            }
            int wideWidth = source == null
                ? width
                : width + source.Margin * 2;
            int outputWidth = wideWidth * Scale;
            int outputHeight = height * Scale;
            _renderer.EnsureTexture(_present, outputWidth, outputHeight, renderTarget: true);
            _renderer.CopyRegion(source?.Texture ?? _vram, _present,
                (source == null ? x : x - source.X) * Scale,
                (source == null ? y : y - source.Y) * Scale,
                0, 0, outputWidth, outputHeight);
            float aspect = source is { Margin: > 0 }
                ? GpuHle.WideAspect
                : GpuHle.OutputAspect;
            return (_present, outputWidth, outputHeight, aspect);
        }

        int total = VramShadow.Width * VramShadow.Height;
        if (_read16.Length < total) _read16 = new ushort[total];
        ReadVram(0, 0, VramShadow.Width, VramShadow.Height, _read16);
        int bytes = width * height * 4;
        if (_upload.Length < bytes) _upload = new byte[bytes];
        for (int py = 0; py < height; py++)
        for (int px = 0; px < width; px++)
        {
            int baseByte = ((y + py) * VramShadow.Width + x) * 2 + px * 3;
            int destination = (py * width + px) * 4;
            _upload[destination] = VramByte(_read16, baseByte);
            _upload[destination + 1] = VramByte(_read16, baseByte + 1);
            _upload[destination + 2] = VramByte(_read16, baseByte + 2);
            _upload[destination + 3] = 255;
        }
        _renderer.Upload(_present, width, height, _upload.AsSpan(0, bytes));
        return (_present, width, height, GpuHle.OutputAspect);
    }

    static byte VramByte(ushort[] words, int byteOffset)
    {
        ushort word = words[(byteOffset >> 1) & (words.Length - 1)];
        return (byte)(((byteOffset & 1) == 0) ? word : word >> 8);
    }

    static byte Expand5(int value) => (byte)((value << 3) | (value >> 2));

    public void Dispose()
    {
        Ready = false;
        foreach (DisplayTarget? target in _displayTargets)
            if (target != null) _renderer.DisposeTexture(target.Texture);
        _depthStencil?.Dispose();
        _dualSourceReverseSubtractBlend?.Dispose();
        _dualSourceAddBlend?.Dispose();
        _opaqueBlend?.Dispose();
        _rasterizer?.Dispose();
        _constantBuffer?.Dispose();
        foreach (ID3D11Buffer? vertexBuffer in _vertexBuffers)
            vertexBuffer?.Dispose();
        _inputLayout?.Dispose();
        _pixelShader?.Dispose();
        _vertexShader?.Dispose();
        _renderer.DisposeTexture(_present);
        _renderer.DisposeTexture(_destinationSnapshot);
        _renderer.DisposeTexture(_snapshot);
        _renderer.DisposeTexture(_textureVram);
        _renderer.DisposeTexture(_vram);
    }
}
