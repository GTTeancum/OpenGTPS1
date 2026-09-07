using System.Runtime.InteropServices;
using SharpGen.Runtime;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using static Vortice.Direct3D11.D3D11;
using static Vortice.DXGI.DXGI;

namespace RecompOne.Runtime.Host.Window;

/// <summary>
/// Owns the single shipping graphics device. All guest display textures,
/// native-world readbacks, presentation passes, and ImGui draw data are
/// submitted through this D3D11 context.
/// </summary>
internal sealed class D3D11Renderer : IDisposable
{
    internal sealed class Texture : IDisposable
    {
        public readonly uint Id;
        public ID3D11Texture2D Resource;
        public ID3D11ShaderResourceView View;
        public ID3D11RenderTargetView? Target;
        public int Width;
        public int Height;

        public Texture(uint id, ID3D11Texture2D resource,
            ID3D11ShaderResourceView view, ID3D11RenderTargetView? target,
            int width, int height)
        {
            Id = id;
            Resource = resource;
            View = view;
            Target = target;
            Width = width;
            Height = height;
        }

        public void Dispose()
        {
            Target?.Dispose();
            View.Dispose();
            Resource.Dispose();
        }
    }

    const string FullscreenShader = """
        Texture2D Source : register(t0);
        SamplerState SourceSampler : register(s0);

        struct VsOut { float4 position : SV_Position; float2 uv : TEXCOORD0; };
        VsOut VSMain(uint id : SV_VertexID)
        {
            VsOut o;
            float2 p = float2((id == 1 || id == 3) ? 1.0 : -1.0,
                              (id >= 2) ? -1.0 : 1.0);
            o.position = float4(p, 0.0, 1.0);
            o.uv = float2(p.x * 0.5 + 0.5, 0.5 - p.y * 0.5);
            return o;
        }
        float4 PSCopy(VsOut input) : SV_Target
        {
            return Source.Sample(SourceSampler, input.uv);
        }
        float Luma(float3 color)
        {
            return dot(color, float3(0.299, 0.587, 0.114));
        }
        float4 PSFxaa(VsOut input) : SV_Target
        {
            uint width, height;
            Source.GetDimensions(width, height);
            float2 inverseSize = 1.0 / float2(width, height);
            float3 nw = Source.Sample(SourceSampler, input.uv + float2(-1, -1) * inverseSize).rgb;
            float3 ne = Source.Sample(SourceSampler, input.uv + float2( 1, -1) * inverseSize).rgb;
            float3 sw = Source.Sample(SourceSampler, input.uv + float2(-1,  1) * inverseSize).rgb;
            float3 se = Source.Sample(SourceSampler, input.uv + float2( 1,  1) * inverseSize).rgb;
            float3 m = Source.Sample(SourceSampler, input.uv).rgb;
            float lnw = Luma(nw), lne = Luma(ne), lsw = Luma(sw), lse = Luma(se), lm = Luma(m);
            float minimum = min(lm, min(min(lnw, lne), min(lsw, lse)));
            float maximum = max(lm, max(max(lnw, lne), max(lsw, lse)));
            float2 direction = float2(-((lnw + lne) - (lsw + lse)),
                                       ((lnw + lsw) - (lne + lse)));
            float reduce = max((lnw + lne + lsw + lse) * (0.25 / 8.0), 1.0 / 128.0);
            direction = clamp(direction / (min(abs(direction.x), abs(direction.y)) + reduce), -8.0, 8.0) * inverseSize;
            float3 a = 0.5 * (Source.Sample(SourceSampler, input.uv + direction * (1.0 / 3.0 - 0.5)).rgb +
                              Source.Sample(SourceSampler, input.uv + direction * (2.0 / 3.0 - 0.5)).rgb);
            float3 b = a * 0.5 + 0.25 * (Source.Sample(SourceSampler, input.uv - direction * 0.5).rgb +
                                         Source.Sample(SourceSampler, input.uv + direction * 0.5).rgb);
            float lb = Luma(b);
            return float4((lb < minimum || lb > maximum) ? a : b, 1.0);
        }
        """;

    readonly IDXGIFactory2 _factory;
    readonly Dictionary<uint, Texture> _textures = [];
    uint _nextTextureId = 1;
    ID3D11Texture2D? _backBuffer;
    ID3D11RenderTargetView? _backBufferView;
    ID3D11VertexShader? _fullscreenVertexShader;
    ID3D11PixelShader? _copyPixelShader;
    ID3D11PixelShader? _fxaaPixelShader;
    ID3D11SamplerState? _linearSampler;
    ID3D11SamplerState? _pointSampler;
    ID3D11RasterizerState? _rasterizer;
    ID3D11BlendState? _opaqueBlend;
    int _backBufferWidth;
    int _backBufferHeight;

    public ID3D11Device Device { get; }
    public ID3D11DeviceContext Context { get; }
    public IDXGISwapChain1 SwapChain { get; }
    public int BackBufferWidth => _backBufferWidth;
    public int BackBufferHeight => _backBufferHeight;

    public D3D11Renderer(nint hwnd, int width, int height)
    {
        if (hwnd == 0)
            throw new InvalidOperationException("A Win32 window handle is required for D3D11 presentation.");

        FeatureLevel[] levels =
        [
            FeatureLevel.Level_11_1,
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_10_1,
            FeatureLevel.Level_10_0,
        ];
        Result result = D3D11CreateDevice(
            IntPtr.Zero,
            DriverType.Hardware,
            DeviceCreationFlags.BgraSupport,
            levels,
            out ID3D11Device device,
            out _,
            out ID3D11DeviceContext context);
        if (result.Failure)
        {
            result = D3D11CreateDevice(
                IntPtr.Zero,
                DriverType.Warp,
                DeviceCreationFlags.BgraSupport,
                levels,
                out device,
                out _,
                out context);
        }
        result.CheckError();
        Device = device;
        Context = context;
        using (ID3D11Multithread multithread =
            Context.QueryInterface<ID3D11Multithread>())
        {
            multithread.SetMultithreadProtected(true);
        }

        _factory = CreateDXGIFactory1<IDXGIFactory2>();
        var description = new SwapChainDescription1(
            Math.Max(1, width),
            Math.Max(1, height),
            Format.B8G8R8A8_UNorm,
            bufferCount: 2,
            swapEffect: SwapEffect.FlipDiscard);
        SwapChain = _factory.CreateSwapChainForHwnd(
            Device, hwnd, description);
        _factory.MakeWindowAssociation(hwnd, WindowAssociationFlags.IgnoreAltEnter);

        CreatePipeline();
        Resize(width, height);
        Console.WriteLine($"[Host] Direct3D 11 device={Device.FeatureLevel} presentation={_backBufferWidth}x{_backBufferHeight}");
    }

    void CreatePipeline()
    {
        ReadOnlyMemory<byte> vs = Compiler.Compile(
            FullscreenShader, "VSMain", "fullscreen.hlsl", "vs_5_0",
            ShaderFlags.EnableStrictness | ShaderFlags.OptimizationLevel3);
        ReadOnlyMemory<byte> ps = Compiler.Compile(
            FullscreenShader, "PSCopy", "fullscreen.hlsl", "ps_5_0",
            ShaderFlags.EnableStrictness | ShaderFlags.OptimizationLevel3);
        _fullscreenVertexShader = Device.CreateVertexShader(vs.Span);
        _copyPixelShader = Device.CreatePixelShader(ps.Span);
        ReadOnlyMemory<byte> fxaa = Compiler.Compile(
            FullscreenShader, "PSFxaa", "fullscreen.hlsl", "ps_5_0",
            ShaderFlags.EnableStrictness | ShaderFlags.OptimizationLevel3);
        _fxaaPixelShader = Device.CreatePixelShader(fxaa.Span);
        _linearSampler = Device.CreateSamplerState(new SamplerDescription(
            Filter.MinMagMipLinear,
            TextureAddressMode.Clamp,
            TextureAddressMode.Clamp,
            TextureAddressMode.Clamp));
        _pointSampler = Device.CreateSamplerState(new SamplerDescription(
            Filter.MinMagMipPoint,
            TextureAddressMode.Clamp,
            TextureAddressMode.Clamp,
            TextureAddressMode.Clamp));
        _rasterizer = Device.CreateRasterizerState(new RasterizerDescription(
            CullMode.None, FillMode.Solid)
        {
            ScissorEnable = false,
            DepthClipEnable = true,
        });
        _opaqueBlend = Device.CreateBlendState(BlendDescription.Opaque);
    }

    public void Resize(int width, int height)
    {
        width = Math.Max(1, width);
        height = Math.Max(1, height);
        if (_backBufferView != null && width == _backBufferWidth &&
            height == _backBufferHeight)
            return;

        Context.UnsetRenderTargets();
        _backBufferView?.Dispose();
        _backBuffer?.Dispose();
        _backBufferView = null;
        _backBuffer = null;
        if (_backBufferWidth != 0)
            SwapChain.ResizeBuffers(2, width, height,
                Format.B8G8R8A8_UNorm, SwapChainFlags.None).CheckError();
        _backBuffer = SwapChain.GetBuffer<ID3D11Texture2D>(0);
        _backBufferView = Device.CreateRenderTargetView(_backBuffer);
        _backBufferWidth = width;
        _backBufferHeight = height;
    }

    public void BeginFrame(int width, int height)
    {
        Resize(width, height);
        Context.OMSetRenderTargets(_backBufferView!);
        Context.RSSetViewport(new Viewport(0, 0, width, height));
        Context.ClearRenderTargetView(_backBufferView!,
            new Color4(0.08f, 0.08f, 0.08f, 1f));
    }

    public void EndFrame(bool present)
    {
        if (present)
            SwapChain.Present(0, PresentFlags.None).CheckError();
        else
            // Hidden/headless validation has no swap-chain Present to submit
            // the shared immediate-context queue. Flush once after both the
            // native command list and compositor copy are recorded so dynamic
            // rings cannot accumulate dozens of unsubmitted frames.
            Context.Flush();
    }

    public Texture CreateTexture(int width = 1, int height = 1,
        bool renderTarget = false, bool linear = false)
    {
        uint id = _nextTextureId++;
        var texture = CreateTextureResource(id, width, height, renderTarget);
        _textures.Add(id, texture);
        return texture;
    }

    Texture CreateTextureResource(uint id, int width, int height,
        bool renderTarget)
    {
        BindFlags bindings = BindFlags.ShaderResource;
        if (renderTarget) bindings |= BindFlags.RenderTarget;
        var description = new Texture2DDescription(
            Format.R8G8B8A8_UNorm,
            Math.Max(1, width),
            Math.Max(1, height),
            1, 1, bindings);
        ID3D11Texture2D resource;
        try
        {
            resource = Device.CreateTexture2D(description);
        }
        catch (SharpGenException)
        {
            Console.Error.WriteLine(
                $"[Host] D3D11 texture allocation failed size=" +
                $"{description.Width}x{description.Height} " +
                $"renderTarget={renderTarget} " +
                $"removedReason={Device.DeviceRemovedReason}");
            throw;
        }
        ID3D11ShaderResourceView view = Device.CreateShaderResourceView(resource);
        ID3D11RenderTargetView? target = renderTarget
            ? Device.CreateRenderTargetView(resource)
            : null;
        return new Texture(id, resource, view, target,
            Math.Max(1, width), Math.Max(1, height));
    }

    public void EnsureTexture(Texture texture, int width, int height,
        bool renderTarget = false)
    {
        width = Math.Max(1, width);
        height = Math.Max(1, height);
        if (texture.Width == width && texture.Height == height &&
            (texture.Target != null) == renderTarget)
            return;
        Texture replacement = CreateTextureResource(
            texture.Id, width, height, renderTarget);
        texture.Target?.Dispose();
        texture.View.Dispose();
        texture.Resource.Dispose();
        texture.Resource = replacement.Resource;
        texture.View = replacement.View;
        texture.Target = replacement.Target;
        texture.Width = width;
        texture.Height = height;
    }

    public unsafe void Upload(Texture texture, int width, int height,
        ReadOnlySpan<byte> rgba)
    {
        EnsureTexture(texture, width, height);
        if (rgba.Length < checked(width * height * 4))
            throw new ArgumentException("Texture upload is smaller than its dimensions.", nameof(rgba));
        Context.UpdateSubresource(rgba, texture.Resource,
            rowPitch: width * 4);
    }

    public void CopyNativeTexture(Texture destination, nint sourcePointer,
        int width, int height)
    {
        if (sourcePointer == 0)
            throw new ArgumentException(
                "Native texture pointer is null.", nameof(sourcePointer));
        EnsureTexture(destination, width, height);
        Marshal.AddRef(sourcePointer);
        using var source = new ID3D11Texture2D(sourcePointer);
        Context.CopyResource(destination.Resource, source);
    }

    public void UploadRegion(Texture texture, int x, int y,
        int width, int height, ReadOnlySpan<byte> rgba)
    {
        if (width <= 0 || height <= 0) return;
        if (rgba.Length < checked(width * height * 4))
            throw new ArgumentException("Region upload is smaller than its dimensions.", nameof(rgba));
        var region = new Box(x, y, 0, x + width, y + height, 1);
        Context.UpdateSubresource(rgba, texture.Resource,
            rowPitch: width * 4, region: region);
    }

    public void CopyRegion(Texture source, Texture destination,
        int sourceX, int sourceY, int destinationX, int destinationY,
        int width, int height)
    {
        if (width <= 0 || height <= 0) return;
        if (sourceX < 0 || sourceY < 0 || destinationX < 0 ||
            destinationY < 0 || sourceX + width > source.Width ||
            sourceY + height > source.Height ||
            destinationX + width > destination.Width ||
            destinationY + height > destination.Height)
            throw new InvalidOperationException(
                $"D3D11 texture copy is out of bounds: " +
                $"source={source.Id}:{source.Width}x{source.Height}@" +
                $"{sourceX},{sourceY} destination=" +
                $"{destination.Id}:{destination.Width}x{destination.Height}@" +
                $"{destinationX},{destinationY} size={width}x{height}");
        var sourceRegion = new Box(sourceX, sourceY, 0,
            sourceX + width, sourceY + height, 1);
        Context.CopySubresourceRegion(destination.Resource, 0,
            destinationX, destinationY, 0,
            source.Resource, 0, sourceRegion);
    }

    public Texture? ResolveTexture(uint id) =>
        _textures.GetValueOrDefault(id);

    public void DrawFullscreen(Texture source,
        ID3D11RenderTargetView target, int width, int height,
        bool linear, bool fxaa = false,
        int x = 0, int y = 0, int? drawWidth = null, int? drawHeight = null)
    {
        Context.OMSetRenderTargets(target);
        Context.OMSetBlendState(_opaqueBlend);
        Context.RSSetState(_rasterizer);
        Context.RSSetViewport(new Viewport(x, y,
            drawWidth ?? width, drawHeight ?? height));
        Context.IASetInputLayout(null);
        Context.IASetPrimitiveTopology(PrimitiveTopology.TriangleStrip);
        Context.VSSetShader(_fullscreenVertexShader);
        Context.PSSetShader(fxaa ? _fxaaPixelShader : _copyPixelShader);
        Context.PSSetSampler(0, linear ? _linearSampler : _pointSampler);
        Context.PSSetShaderResource(0, source.View);
        Context.Draw(4, 0);
        Context.PSSetShaderResource(0, null!);
    }

    public void Clear(Texture texture, Color4 color)
    {
        if (texture.Target == null)
            throw new InvalidOperationException("Texture is not a render target.");
        Context.ClearRenderTargetView(texture.Target, color);
    }

    public unsafe void Readback(Texture texture, Span<byte> rgba)
    {
        ReadbackRegion(texture, 0, 0, texture.Width, texture.Height, rgba);
    }

    public unsafe void ReadbackRegion(Texture texture, int x, int y,
        int width, int height, Span<byte> rgba)
    {
        if (width <= 0 || height <= 0) return;
        if (x < 0 || y < 0 || x + width > texture.Width ||
            y + height > texture.Height)
            throw new ArgumentOutOfRangeException(nameof(x),
                $"Readback region {width}x{height}@{x},{y} exceeds " +
                $"texture {texture.Width}x{texture.Height}.");
        int required = checked(width * height * 4);
        if (rgba.Length < required)
            throw new ArgumentException("Readback span is too small.", nameof(rgba));
        using ID3D11Texture2D staging = Device.CreateTexture2D(
            new Texture2DDescription(Format.R8G8B8A8_UNorm,
                width, height, 1, 1, BindFlags.None,
                ResourceUsage.Staging, CpuAccessFlags.Read));
        var sourceRegion = new Box(x, y, 0, x + width, y + height, 1);
        Context.CopySubresourceRegion(staging, 0, 0, 0, 0,
            texture.Resource, 0, sourceRegion);
        MappedSubresource mapped = Context.Map(staging, 0,
            MapMode.Read, Vortice.Direct3D11.MapFlags.None);
        try
        {
            fixed (byte* destinationBase = rgba)
            {
                byte* sourceBase = (byte*)mapped.DataPointer;
                int rowBytes = width * 4;
                for (int row = 0; row < height; row++)
                    Buffer.MemoryCopy(sourceBase + row * mapped.RowPitch,
                        destinationBase + row * rowBytes, rowBytes, rowBytes);
            }
        }
        finally
        {
            Context.Unmap(staging, 0);
        }
    }

    public void RestoreBackBuffer()
    {
        Context.OMSetRenderTargets(_backBufferView!);
        Context.RSSetViewport(new Viewport(
            0, 0, _backBufferWidth, _backBufferHeight));
    }

    public unsafe void CaptureBackBufferPpm(string path)
    {
        if (_backBuffer == null || _backBufferWidth <= 0 ||
            _backBufferHeight <= 0)
            throw new InvalidOperationException("The D3D11 backbuffer is unavailable.");
        using ID3D11Texture2D staging = Device.CreateTexture2D(
            new Texture2DDescription(Format.B8G8R8A8_UNorm,
                _backBufferWidth, _backBufferHeight, 1, 1, BindFlags.None,
                ResourceUsage.Staging, CpuAccessFlags.Read));
        Context.UnsetRenderTargets();
        Context.CopyResource(staging, _backBuffer);
        MappedSubresource mapped = Context.Map(staging, 0,
            MapMode.Read, Vortice.Direct3D11.MapFlags.None);
        try
        {
            using FileStream output = File.Create(path);
            output.Write(System.Text.Encoding.ASCII.GetBytes(
                $"P6\n{_backBufferWidth} {_backBufferHeight}\n255\n"));
            byte[] row = new byte[_backBufferWidth * 3];
            for (int y = 0; y < _backBufferHeight; y++)
            {
                byte* source = (byte*)mapped.DataPointer + y * mapped.RowPitch;
                for (int x = 0, destination = 0; x < _backBufferWidth;
                    x++, destination += 3)
                {
                    row[destination] = source[x * 4 + 2];
                    row[destination + 1] = source[x * 4 + 1];
                    row[destination + 2] = source[x * 4];
                }
                output.Write(row);
            }
        }
        finally
        {
            Context.Unmap(staging, 0);
            RestoreBackBuffer();
        }
    }

    public void DisposeTexture(Texture? texture)
    {
        if (texture == null || !_textures.Remove(texture.Id)) return;
        texture.Dispose();
    }

    public void Dispose()
    {
        Context.ClearState();
        Context.Flush();
        foreach (Texture texture in _textures.Values)
            texture.Dispose();
        _textures.Clear();
        _opaqueBlend?.Dispose();
        _rasterizer?.Dispose();
        _pointSampler?.Dispose();
        _linearSampler?.Dispose();
        _copyPixelShader?.Dispose();
        _fxaaPixelShader?.Dispose();
        _fullscreenVertexShader?.Dispose();
        _backBufferView?.Dispose();
        _backBuffer?.Dispose();
        SwapChain.Dispose();
        _factory.Dispose();
        Context.Dispose();
        // Releasing the final D3D11 device reference can block indefinitely
        // in some Windows display drivers after a hidden flip-model swapchain
        // has been torn down. This host terminates immediately after shutdown,
        // so let process teardown release that final COM reference after every
        // owned child object and the immediate context are already released.
    }
}
