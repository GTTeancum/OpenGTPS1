// Included in the renderer's anonymous namespace after D3D/WRL declarations.
struct LightingFrameGpu {
    ComPtr<ID3D11Buffer> constants;
    ComPtr<ID3D11Buffer> vertices;
    UINT vertex_bytes{};
};
struct LightingBaseGpu {
    ComPtr<ID3D11Texture2D> depth;
    ComPtr<ID3D11ShaderResourceView> view;
    std::array<ComPtr<ID3D11DepthStencilView>,2> slices;
    std::array<ComPtr<ID3D11VertexShader>,2> vertex_shaders;
    ComPtr<ID3D11PixelShader> pixel_shader;
    ComPtr<ID3D11InputLayout> layout;
    ComPtr<ID3D11RasterizerState> rasterizer;
    ComPtr<ID3D11DepthStencilState> depth_state;
    ComPtr<ID3D11SamplerState> sampler;
    unsigned resolution{};
    bool failed{};
};
