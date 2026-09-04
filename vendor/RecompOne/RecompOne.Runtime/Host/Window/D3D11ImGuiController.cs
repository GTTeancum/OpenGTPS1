using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace RecompOne.Runtime.Host.Window;

internal sealed class D3D11ImGuiController : IDisposable
{
    const string Shader = """
        cbuffer Projection : register(b0) { float4x4 Matrix; };
        Texture2D Texture : register(t0);
        SamplerState TextureSampler : register(s0);
        struct VSIn {
            float2 position : POSITION;
            float2 uv : TEXCOORD0;
            float4 color : COLOR0;
        };
        struct PSIn {
            float4 position : SV_Position;
            float2 uv : TEXCOORD0;
            float4 color : COLOR0;
        };
        PSIn VSMain(VSIn input) {
            PSIn output;
            output.position = mul(Matrix, float4(input.position, 0.0, 1.0));
            output.uv = input.uv;
            output.color = input.color;
            return output;
        }
        float4 PSMain(PSIn input) : SV_Target {
            return input.color * Texture.Sample(TextureSampler, input.uv);
        }
        """;

    readonly D3D11Renderer _renderer;
    readonly IWindow _window;
    readonly IInputContext _input;
    readonly IKeyboard? _keyboard;
    readonly IMouse? _mouse;
    readonly nint _context;
    D3D11Renderer.Texture? _fontTexture;
    ID3D11VertexShader? _vertexShader;
    ID3D11PixelShader? _pixelShader;
    ID3D11InputLayout? _inputLayout;
    ID3D11Buffer? _vertexBuffer;
    ID3D11Buffer? _indexBuffer;
    ID3D11Buffer? _constantBuffer;
    ID3D11SamplerState? _sampler;
    ID3D11BlendState? _blend;
    ID3D11RasterizerState? _rasterizer;
    ID3D11DepthStencilState? _depthStencil;
    int _vertexCapacity;
    int _indexCapacity;
    Vector2 _lastWheel;

    public D3D11ImGuiController(D3D11Renderer renderer,
        IWindow window, IInputContext input, Action configure)
    {
        _renderer = renderer;
        _window = window;
        _input = input;
        _keyboard = input.Keyboards.FirstOrDefault();
        _mouse = input.Mice.FirstOrDefault();
        _context = ImGui.CreateContext();
        ImGui.SetCurrentContext(_context);
        ImGui.StyleColorsDark();
        configure();
        if (_keyboard != null)
            _keyboard.KeyChar += OnKeyChar;
        CreateDeviceResources();
    }

    unsafe void CreateDeviceResources()
    {
        ReadOnlyMemory<byte> vs = Compiler.Compile(
            Shader, "VSMain", "imgui.hlsl", "vs_5_0",
            ShaderFlags.EnableStrictness | ShaderFlags.OptimizationLevel3);
        ReadOnlyMemory<byte> ps = Compiler.Compile(
            Shader, "PSMain", "imgui.hlsl", "ps_5_0",
            ShaderFlags.EnableStrictness | ShaderFlags.OptimizationLevel3);
        _vertexShader = _renderer.Device.CreateVertexShader(vs.Span);
        _pixelShader = _renderer.Device.CreatePixelShader(ps.Span);
        InputElementDescription[] elements =
        [
            new("POSITION", 0, Format.R32G32_Float, 0, 0),
            new("TEXCOORD", 0, Format.R32G32_Float, 8, 0),
            new("COLOR", 0, Format.R8G8B8A8_UNorm, 16, 0),
        ];
        _inputLayout = _renderer.Device.CreateInputLayout(elements, vs.Span);
        _constantBuffer = _renderer.Device.CreateBuffer(
            new BufferDescription(64, BindFlags.ConstantBuffer,
                ResourceUsage.Dynamic, CpuAccessFlags.Write));
        _sampler = _renderer.Device.CreateSamplerState(new SamplerDescription(
            Filter.MinMagMipLinear,
            TextureAddressMode.Wrap,
            TextureAddressMode.Wrap,
            TextureAddressMode.Wrap));
        _blend = _renderer.Device.CreateBlendState(
            BlendDescription.NonPremultiplied);
        _rasterizer = _renderer.Device.CreateRasterizerState(
            new RasterizerDescription(CullMode.None, FillMode.Solid)
            {
                ScissorEnable = true,
                DepthClipEnable = true,
            });
        _depthStencil = _renderer.Device.CreateDepthStencilState(
            DepthStencilDescription.None);

        ImGuiIOPtr io = ImGui.GetIO();
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.Fonts.AddFontDefault();
        io.Fonts.GetTexDataAsRGBA32(out byte* pixels,
            out int width, out int height, out int bytesPerPixel);
        _fontTexture = _renderer.CreateTexture(width, height);
        _renderer.Upload(_fontTexture, width, height,
            new ReadOnlySpan<byte>(pixels, width * height * bytesPerPixel));
        io.Fonts.SetTexID((nint)_fontTexture.Id);
        io.Fonts.ClearTexData();
    }

    void OnKeyChar(IKeyboard keyboard, char value)
    {
        ImGui.SetCurrentContext(_context);
        ImGui.GetIO().AddInputCharacter(value);
    }

    public void Update(float deltaSeconds)
    {
        ImGui.SetCurrentContext(_context);
        ImGuiIOPtr io = ImGui.GetIO();
        var fb = _window.FramebufferSize;
        var size = _window.Size;
        io.DisplaySize = new Vector2(Math.Max(1, size.X), Math.Max(1, size.Y));
        io.DisplayFramebufferScale = new Vector2(
            size.X > 0 ? fb.X / (float)size.X : 1f,
            size.Y > 0 ? fb.Y / (float)size.Y : 1f);
        io.DeltaTime = deltaSeconds > 0 ? deltaSeconds : 1f / 60f;

        if (_mouse != null)
        {
            io.AddMousePosEvent(_mouse.Position.X, _mouse.Position.Y);
            io.AddMouseButtonEvent(0, _mouse.IsButtonPressed(MouseButton.Left));
            io.AddMouseButtonEvent(1, _mouse.IsButtonPressed(MouseButton.Right));
            io.AddMouseButtonEvent(2, _mouse.IsButtonPressed(MouseButton.Middle));
            Vector2 wheel = _mouse.ScrollWheels.Count > 0
                ? new Vector2(_mouse.ScrollWheels[0].X, _mouse.ScrollWheels[0].Y)
                : Vector2.Zero;
            Vector2 wheelDelta = wheel - _lastWheel;
            if (wheelDelta != Vector2.Zero)
                io.AddMouseWheelEvent(wheelDelta.X, wheelDelta.Y);
            _lastWheel = wheel;
        }

        if (_keyboard != null)
        {
            io.AddKeyEvent(ImGuiKey.ModCtrl,
                _keyboard.IsKeyPressed(Key.ControlLeft) ||
                _keyboard.IsKeyPressed(Key.ControlRight));
            io.AddKeyEvent(ImGuiKey.ModShift,
                _keyboard.IsKeyPressed(Key.ShiftLeft) ||
                _keyboard.IsKeyPressed(Key.ShiftRight));
            io.AddKeyEvent(ImGuiKey.ModAlt,
                _keyboard.IsKeyPressed(Key.AltLeft) ||
                _keyboard.IsKeyPressed(Key.AltRight));
            io.AddKeyEvent(ImGuiKey.ModSuper,
                _keyboard.IsKeyPressed(Key.SuperLeft) ||
                _keyboard.IsKeyPressed(Key.SuperRight));
            foreach ((Key key, ImGuiKey imgui) in KeyMap)
                io.AddKeyEvent(imgui, _keyboard.IsKeyPressed(key));
        }
        ImGui.NewFrame();
    }

    static readonly (Key, ImGuiKey)[] KeyMap =
    [
        (Key.Tab, ImGuiKey.Tab),
        (Key.Left, ImGuiKey.LeftArrow),
        (Key.Right, ImGuiKey.RightArrow),
        (Key.Up, ImGuiKey.UpArrow),
        (Key.Down, ImGuiKey.DownArrow),
        (Key.PageUp, ImGuiKey.PageUp),
        (Key.PageDown, ImGuiKey.PageDown),
        (Key.Home, ImGuiKey.Home),
        (Key.End, ImGuiKey.End),
        (Key.Insert, ImGuiKey.Insert),
        (Key.Delete, ImGuiKey.Delete),
        (Key.Backspace, ImGuiKey.Backspace),
        (Key.Space, ImGuiKey.Space),
        (Key.Enter, ImGuiKey.Enter),
        (Key.Escape, ImGuiKey.Escape),
        (Key.A, ImGuiKey.A), (Key.C, ImGuiKey.C),
        (Key.V, ImGuiKey.V), (Key.X, ImGuiKey.X),
        (Key.Y, ImGuiKey.Y), (Key.Z, ImGuiKey.Z),
    ];

    unsafe void EnsureBuffers(int vertices, int indices)
    {
        if (_vertexBuffer == null || vertices > _vertexCapacity)
        {
            _vertexBuffer?.Dispose();
            _vertexCapacity = Math.Max(vertices + 5000, 10000);
            _vertexBuffer = _renderer.Device.CreateBuffer(
                new BufferDescription(
                    _vertexCapacity * Unsafe.SizeOf<ImDrawVert>(),
                    BindFlags.VertexBuffer,
                    ResourceUsage.Dynamic,
                    CpuAccessFlags.Write));
        }
        if (_indexBuffer == null || indices > _indexCapacity)
        {
            _indexBuffer?.Dispose();
            _indexCapacity = Math.Max(indices + 10000, 20000);
            _indexBuffer = _renderer.Device.CreateBuffer(
                new BufferDescription(
                    _indexCapacity * sizeof(ushort),
                    BindFlags.IndexBuffer,
                    ResourceUsage.Dynamic,
                    CpuAccessFlags.Write));
        }
    }

    public unsafe void Render()
    {
        ImGui.SetCurrentContext(_context);
        ImGui.Render();
        ImDrawDataPtr drawData = ImGui.GetDrawData();
        if (drawData.CmdListsCount == 0 ||
            drawData.DisplaySize.X <= 0 || drawData.DisplaySize.Y <= 0)
            return;

        EnsureBuffers(drawData.TotalVtxCount, drawData.TotalIdxCount);
        MappedSubresource vertexMap = _renderer.Context.Map(
            _vertexBuffer!, MapMode.WriteDiscard,
            Vortice.Direct3D11.MapFlags.None);
        MappedSubresource indexMap = _renderer.Context.Map(
            _indexBuffer!, MapMode.WriteDiscard,
            Vortice.Direct3D11.MapFlags.None);
        byte* vertexDestination = (byte*)vertexMap.DataPointer;
        byte* indexDestination = (byte*)indexMap.DataPointer;
        for (int listIndex = 0; listIndex < drawData.CmdListsCount; listIndex++)
        {
            ImDrawListPtr list = drawData.CmdLists[listIndex];
            int vertexBytes = list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
            int indexBytes = list.IdxBuffer.Size * sizeof(ushort);
            Buffer.MemoryCopy((void*)list.VtxBuffer.Data,
                vertexDestination, vertexBytes, vertexBytes);
            Buffer.MemoryCopy((void*)list.IdxBuffer.Data,
                indexDestination, indexBytes, indexBytes);
            vertexDestination += vertexBytes;
            indexDestination += indexBytes;
        }
        _renderer.Context.Unmap(_vertexBuffer!, 0);
        _renderer.Context.Unmap(_indexBuffer!, 0);

        float left = drawData.DisplayPos.X;
        float right = drawData.DisplayPos.X + drawData.DisplaySize.X;
        float top = drawData.DisplayPos.Y;
        float bottom = drawData.DisplayPos.Y + drawData.DisplaySize.Y;
        Matrix4x4 projection = new(
            2f / (right - left), 0, 0, 0,
            0, 2f / (top - bottom), 0, 0,
            0, 0, 0.5f, 0,
            (right + left) / (left - right),
            (top + bottom) / (bottom - top), 0.5f, 1f);
        MappedSubresource constantMap = _renderer.Context.Map(
            _constantBuffer!, MapMode.WriteDiscard,
            Vortice.Direct3D11.MapFlags.None);
        Unsafe.Write((void*)constantMap.DataPointer, projection);
        _renderer.Context.Unmap(_constantBuffer!, 0);

        _renderer.RestoreBackBuffer();
        _renderer.Context.IASetInputLayout(_inputLayout);
        _renderer.Context.IASetVertexBuffer(0, _vertexBuffer!,
            Unsafe.SizeOf<ImDrawVert>(), 0);
        _renderer.Context.IASetIndexBuffer(_indexBuffer!,
            Format.R16_UInt, 0);
        _renderer.Context.IASetPrimitiveTopology(
            PrimitiveTopology.TriangleList);
        _renderer.Context.VSSetShader(_vertexShader);
        _renderer.Context.VSSetConstantBuffer(0, _constantBuffer);
        _renderer.Context.PSSetShader(_pixelShader);
        _renderer.Context.PSSetSampler(0, _sampler);
        _renderer.Context.OMSetBlendState(_blend);
        _renderer.Context.OMSetDepthStencilState(_depthStencil);
        _renderer.Context.RSSetState(_rasterizer);

        Vector2 clipOffset = drawData.DisplayPos;
        Vector2 clipScale = drawData.FramebufferScale;
        int globalVertexOffset = 0;
        int globalIndexOffset = 0;
        for (int listIndex = 0; listIndex < drawData.CmdListsCount; listIndex++)
        {
            ImDrawListPtr list = drawData.CmdLists[listIndex];
            for (int commandIndex = 0;
                commandIndex < list.CmdBuffer.Size; commandIndex++)
            {
                ImDrawCmdPtr command = list.CmdBuffer[commandIndex];
                Vector4 clip = command.ClipRect;
                int x0 = (int)((clip.X - clipOffset.X) * clipScale.X);
                int y0 = (int)((clip.Y - clipOffset.Y) * clipScale.Y);
                int x1 = (int)((clip.Z - clipOffset.X) * clipScale.X);
                int y1 = (int)((clip.W - clipOffset.Y) * clipScale.Y);
                if (x1 <= x0 || y1 <= y0) continue;
                _renderer.Context.RSSetScissorRect(x0, y0, x1, y1);
                uint textureId = unchecked((uint)command.TextureId);
                D3D11Renderer.Texture? texture =
                    _renderer.ResolveTexture(textureId);
                if (texture == null) continue;
                _renderer.Context.PSSetShaderResource(0, texture.View);
                _renderer.Context.DrawIndexed(
                    (int)command.ElemCount,
                    globalIndexOffset + (int)command.IdxOffset,
                    globalVertexOffset + (int)command.VtxOffset);
            }
            globalIndexOffset += list.IdxBuffer.Size;
            globalVertexOffset += list.VtxBuffer.Size;
        }
        _renderer.Context.PSSetShaderResource(0, null!);
    }

    public void Dispose()
    {
        ImGui.SetCurrentContext(_context);
        if (_keyboard != null)
            _keyboard.KeyChar -= OnKeyChar;
        _renderer.DisposeTexture(_fontTexture);
        _depthStencil?.Dispose();
        _rasterizer?.Dispose();
        _blend?.Dispose();
        _sampler?.Dispose();
        _constantBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _vertexBuffer?.Dispose();
        _inputLayout?.Dispose();
        _pixelShader?.Dispose();
        _vertexShader?.Dispose();
        ImGui.DestroyContext(_context);
    }
}
