#include "opengt/world_gpu_renderer.hpp"

#if defined(_WIN32)

#include <d3d11.h>
#include <d3dcompiler.h>
#include <wrl/client.h>

#include <algorithm>
#include <array>
#include <cstdio>
#include <cstring>
#include <limits>

namespace opengt::render {
namespace {

using Microsoft::WRL::ComPtr;

constexpr std::uint32_t textured_flag = 1U << 0;
constexpr std::uint32_t semi_transparent_flag = 1U << 1;
constexpr std::uint32_t set_mask_flag = 1U << 0;
constexpr std::uint32_t check_mask_flag = 1U << 1;

struct GpuVertex {
    float position[4];
    float uv[2];
    float color[4];
};

struct alignas(16) DrawConstants {
    std::uint32_t primitive_flags;
    std::uint32_t texture_page;
    std::uint32_t clut;
    std::uint32_t environment_flags;
    std::int32_t texture_mask_x;
    std::int32_t texture_mask_y;
    std::int32_t texture_offset_x;
    std::int32_t texture_offset_y;
    std::uint32_t pass_kind;
    std::uint32_t dithering;
    std::uint32_t unused[2];
};

const char shader_source[] = R"(
Texture2D<uint> Vram : register(t0);

cbuffer DrawConstants : register(b0) {
    uint PrimitiveFlags;
    uint TexturePage;
    uint Clut;
    uint EnvironmentFlags;
    int TextureMaskX;
    int TextureMaskY;
    int TextureOffsetX;
    int TextureOffsetY;
    uint PassKind;
    uint Dithering;
    uint2 Unused;
};

struct VsInput {
    float4 position : POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR0;
};

struct VsOutput {
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD0;
    noperspective float4 color : COLOR0;
};

VsOutput VSMain(VsInput input) {
    VsOutput output;
    output.position = input.position;
    output.uv = input.uv;
    output.color = input.color;
    return output;
}

uint VramAt(int x, int y) {
    return Vram.Load(int3(x & 1023, y & 511, 0));
}

uint TextureWord(int rawU, int rawV) {
    int u =
        (rawU & ~(TextureMaskX * 8)) |
        ((TextureOffsetX & TextureMaskX) * 8);
    int v =
        (rawV & ~(TextureMaskY * 8)) |
        ((TextureOffsetY & TextureMaskY) * 8);
    u &= 255;
    v &= 255;
    int pageX = (TexturePage & 15) * 64;
    int pageY = ((TexturePage >> 4) & 1) * 256;
    int mode = (TexturePage >> 7) & 3;
    int clutX = (Clut & 63) * 16;
    int clutY = (Clut >> 6) & 511;
    if (mode == 0) {
        uint packed = VramAt(pageX + (u >> 2), pageY + v);
        uint index = (packed >> ((u & 3) * 4)) & 15;
        return VramAt(clutX + index, clutY);
    }
    if (mode == 1) {
        uint packed = VramAt(pageX + (u >> 1), pageY + v);
        uint index = (packed >> ((u & 1) * 8)) & 255;
        return VramAt(clutX + index, clutY);
    }
    return VramAt(pageX + u, pageY + v);
}

float Expand5(uint value) {
    return ((value << 3) | (value >> 2)) / 255.0;
}

float3 Quantize(float3 color, int2 pixel) {
    static const int ditherMatrix[16] = {
        -4, 0, -3, 1,
         2, -2, 3, -1,
        -3, 1, -4, 0,
         3, -1, 2, -2
    };
    int adjustment =
        ditherMatrix[(pixel.y & 3) * 4 + (pixel.x & 3)];
    int3 value = clamp(
        int3(color * 255.0 + 0.5) + adjustment,
        0,
        255);
    int3 five = min(value >> 3, 31);
    return ((five << 3) | (five >> 2)) / 255.0;
}

float4 PSMain(VsOutput input) : SV_TARGET {
    bool textured = (PrimitiveFlags & 1) != 0;
    bool semitransparent = (PrimitiveFlags & 2) != 0;
    bool rawTexture = (PrimitiveFlags & 4) != 0;
    float3 color = saturate(input.color.rgb);
    if (textured) {
        uint word = TextureWord(
            (int)floor(input.uv.x + 0.5),
            (int)floor(input.uv.y + 0.5));
        if (word == 0)
            discard;
        bool stp = (word & 0x8000) != 0;
        if (semitransparent) {
            if (PassKind == 0 && stp)
                discard;
            if (PassKind == 1 && !stp)
                discard;
        }
        float3 texel = float3(
            Expand5(word & 31),
            Expand5((word >> 5) & 31),
            Expand5((word >> 10) & 31));
        color = rawTexture
            ? texel
            : saturate(texel * input.color.rgb * 2.0);
    }
    if (
        Dithering != 0 &&
        (EnvironmentFlags & 4) != 0 &&
        (!textured || !rawTexture)
    )
        color = Quantize(color, int2(input.position.xy));
    return float4(color, 1.0);
}
)";

bool compile_shader(
    const char* entry,
    const char* target,
    ComPtr<ID3DBlob>* blob
) {
    ComPtr<ID3DBlob> errors;
    const HRESULT result = D3DCompile(
        shader_source,
        sizeof(shader_source) - 1,
        "OpenGT world shader",
        nullptr,
        nullptr,
        entry,
        target,
        D3DCOMPILE_ENABLE_STRICTNESS | D3DCOMPILE_OPTIMIZATION_LEVEL3,
        0,
        blob->ReleaseAndGetAddressOf(),
        errors.GetAddressOf());
    if (FAILED(result) && errors != nullptr) {
        std::fprintf(
            stderr,
            "[World-GPU] shader %s/%s failed: %.*s\n",
            entry,
            target,
            static_cast<int>(errors->GetBufferSize()),
            static_cast<const char*>(errors->GetBufferPointer()));
    }
    return SUCCEEDED(result);
}

ComPtr<ID3D11BlendState> blend_state(
    ID3D11Device* device,
    int mode
) {
    D3D11_BLEND_DESC description{};
    auto& target = description.RenderTarget[0];
    target.RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;
    if (mode >= 0) {
        target.BlendEnable = TRUE;
        target.BlendOp = mode == 2
            ? D3D11_BLEND_OP_REV_SUBTRACT
            : D3D11_BLEND_OP_ADD;
        target.SrcBlend =
            mode == 0 ? D3D11_BLEND_BLEND_FACTOR :
            mode == 3 ? D3D11_BLEND_BLEND_FACTOR :
            D3D11_BLEND_ONE;
        target.DestBlend =
            mode == 0 ? D3D11_BLEND_BLEND_FACTOR :
            D3D11_BLEND_ONE;
        target.BlendOpAlpha = D3D11_BLEND_OP_ADD;
        target.SrcBlendAlpha = D3D11_BLEND_ONE;
        target.DestBlendAlpha = D3D11_BLEND_ZERO;
    }
    ComPtr<ID3D11BlendState> result;
    if (FAILED(device->CreateBlendState(
            &description,
            result.GetAddressOf())))
        result.Reset();
    return result;
}

ComPtr<ID3D11DepthStencilState> depth_state(
    ID3D11Device* device,
    bool write_depth,
    bool check_mask,
    bool set_mask,
    bool depth_enabled
) {
    D3D11_DEPTH_STENCIL_DESC description{};
    description.DepthEnable = depth_enabled ? TRUE : FALSE;
    description.DepthWriteMask = write_depth && depth_enabled
        ? D3D11_DEPTH_WRITE_MASK_ALL
        : D3D11_DEPTH_WRITE_MASK_ZERO;
    description.DepthFunc = D3D11_COMPARISON_LESS_EQUAL;
    description.StencilEnable = check_mask || set_mask;
    description.StencilReadMask = 1;
    description.StencilWriteMask = set_mask ? 1 : 0;
    description.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
    description.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;
    description.FrontFace.StencilPassOp = set_mask
        ? D3D11_STENCIL_OP_REPLACE
        : D3D11_STENCIL_OP_KEEP;
    description.FrontFace.StencilFunc = check_mask
        ? D3D11_COMPARISON_NOT_EQUAL
        : D3D11_COMPARISON_ALWAYS;
    description.BackFace = description.FrontFace;
    ComPtr<ID3D11DepthStencilState> result;
    if (FAILED(device->CreateDepthStencilState(
            &description,
            result.GetAddressOf())))
        result.Reset();
    return result;
}

} // namespace

WorldGpuRenderResult render_world_d3d11(
    const WorldDrawList& draw_list,
    const std::uint16_t* vram,
    std::size_t vram_word_count,
    std::uint8_t* output_rgba,
    std::size_t output_size,
    WorldGpuRenderOptions options,
    WorldGpuRenderStats* stats
) noexcept {
    const std::uint32_t output_scale = options.output_scale;
    if (
        draw_list.display_width <= 0 ||
        draw_list.display_height <= 0 ||
        output_scale == 0 ||
        output_scale > 8
    )
        return WorldGpuRenderResult::invalid_argument;
    const std::uint32_t output_width =
        static_cast<std::uint32_t>(draw_list.display_width) *
        output_scale;
    const std::uint32_t output_height =
        static_cast<std::uint32_t>(draw_list.display_height) *
        output_scale;
    const std::size_t required_output =
        static_cast<std::size_t>(output_width) *
        output_height * 4;
    if (
        vram == nullptr ||
        output_rgba == nullptr ||
        stats == nullptr ||
        vram_word_count < 1024U * 512U ||
        output_size < required_output
    )
        return WorldGpuRenderResult::invalid_argument;
    *stats = {};
    stats->software_adapter = options.use_software_adapter;
    stats->commands =
        static_cast<std::uint32_t>(draw_list.commands.size());
    stats->secondary_commands = draw_list.secondary_commands;

    ComPtr<ID3D11Device> device;
    ComPtr<ID3D11DeviceContext> context;
    D3D_FEATURE_LEVEL feature_level{};
    const D3D_DRIVER_TYPE driver = options.use_software_adapter
        ? D3D_DRIVER_TYPE_WARP
        : D3D_DRIVER_TYPE_HARDWARE;
    if (FAILED(D3D11CreateDevice(
            nullptr,
            driver,
            nullptr,
            D3D11_CREATE_DEVICE_BGRA_SUPPORT,
            nullptr,
            0,
            D3D11_SDK_VERSION,
            device.GetAddressOf(),
            &feature_level,
            context.GetAddressOf())))
        return WorldGpuRenderResult::device_failed;

    ComPtr<ID3DBlob> vertex_blob;
    ComPtr<ID3DBlob> pixel_blob;
    if (
        !compile_shader("VSMain", "vs_4_0", &vertex_blob) ||
        !compile_shader("PSMain", "ps_4_0", &pixel_blob)
    )
        return WorldGpuRenderResult::shader_failed;
    ComPtr<ID3D11VertexShader> vertex_shader;
    ComPtr<ID3D11PixelShader> pixel_shader;
    if (
        FAILED(device->CreateVertexShader(
            vertex_blob->GetBufferPointer(),
            vertex_blob->GetBufferSize(),
            nullptr,
            vertex_shader.GetAddressOf())) ||
        FAILED(device->CreatePixelShader(
            pixel_blob->GetBufferPointer(),
            pixel_blob->GetBufferSize(),
            nullptr,
            pixel_shader.GetAddressOf()))
    )
        return WorldGpuRenderResult::shader_failed;

    const D3D11_INPUT_ELEMENT_DESC elements[] = {
        {
            "POSITION", 0, DXGI_FORMAT_R32G32B32A32_FLOAT,
            0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0,
        },
        {
            "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT,
            0, 16, D3D11_INPUT_PER_VERTEX_DATA, 0,
        },
        {
            "COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT,
            0, 24, D3D11_INPUT_PER_VERTEX_DATA, 0,
        },
    };
    ComPtr<ID3D11InputLayout> input_layout;
    if (FAILED(device->CreateInputLayout(
            elements,
            static_cast<UINT>(std::size(elements)),
            vertex_blob->GetBufferPointer(),
            vertex_blob->GetBufferSize(),
            input_layout.GetAddressOf())))
        return WorldGpuRenderResult::resource_failed;

    D3D11_BUFFER_DESC vertex_buffer_description{};
    vertex_buffer_description.ByteWidth = sizeof(GpuVertex) * 3;
    vertex_buffer_description.Usage = D3D11_USAGE_DYNAMIC;
    vertex_buffer_description.BindFlags = D3D11_BIND_VERTEX_BUFFER;
    vertex_buffer_description.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
    ComPtr<ID3D11Buffer> vertex_buffer;
    if (FAILED(device->CreateBuffer(
            &vertex_buffer_description,
            nullptr,
            vertex_buffer.GetAddressOf())))
        return WorldGpuRenderResult::resource_failed;

    D3D11_BUFFER_DESC constant_buffer_description{};
    constant_buffer_description.ByteWidth = sizeof(DrawConstants);
    constant_buffer_description.Usage = D3D11_USAGE_DEFAULT;
    constant_buffer_description.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
    ComPtr<ID3D11Buffer> constant_buffer;
    if (FAILED(device->CreateBuffer(
            &constant_buffer_description,
            nullptr,
            constant_buffer.GetAddressOf())))
        return WorldGpuRenderResult::resource_failed;

    D3D11_TEXTURE2D_DESC color_description{};
    color_description.Width = output_width;
    color_description.Height = output_height;
    color_description.MipLevels = 1;
    color_description.ArraySize = 1;
    color_description.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    color_description.SampleDesc.Count = 1;
    color_description.Usage = D3D11_USAGE_DEFAULT;
    color_description.BindFlags = D3D11_BIND_RENDER_TARGET;
    ComPtr<ID3D11Texture2D> color_texture;
    ComPtr<ID3D11RenderTargetView> color_view;
    if (
        FAILED(device->CreateTexture2D(
            &color_description,
            nullptr,
            color_texture.GetAddressOf())) ||
        FAILED(device->CreateRenderTargetView(
            color_texture.Get(),
            nullptr,
            color_view.GetAddressOf()))
    )
        return WorldGpuRenderResult::resource_failed;

    D3D11_TEXTURE2D_DESC depth_description = color_description;
    depth_description.Format = DXGI_FORMAT_D24_UNORM_S8_UINT;
    depth_description.BindFlags = D3D11_BIND_DEPTH_STENCIL;
    ComPtr<ID3D11Texture2D> depth_texture;
    ComPtr<ID3D11DepthStencilView> depth_view;
    if (
        FAILED(device->CreateTexture2D(
            &depth_description,
            nullptr,
            depth_texture.GetAddressOf())) ||
        FAILED(device->CreateDepthStencilView(
            depth_texture.Get(),
            nullptr,
            depth_view.GetAddressOf()))
    )
        return WorldGpuRenderResult::resource_failed;

    D3D11_TEXTURE2D_DESC vram_description{};
    vram_description.Width = 1024;
    vram_description.Height = 512;
    vram_description.MipLevels = 1;
    vram_description.ArraySize = 1;
    vram_description.Format = DXGI_FORMAT_R16_UINT;
    vram_description.SampleDesc.Count = 1;
    vram_description.Usage = D3D11_USAGE_IMMUTABLE;
    vram_description.BindFlags = D3D11_BIND_SHADER_RESOURCE;
    const D3D11_SUBRESOURCE_DATA vram_data{
        vram,
        1024U * sizeof(std::uint16_t),
        1024U * 512U * sizeof(std::uint16_t),
    };
    ComPtr<ID3D11Texture2D> vram_texture;
    ComPtr<ID3D11ShaderResourceView> vram_view;
    if (
        FAILED(device->CreateTexture2D(
            &vram_description,
            &vram_data,
            vram_texture.GetAddressOf())) ||
        FAILED(device->CreateShaderResourceView(
            vram_texture.Get(),
            nullptr,
            vram_view.GetAddressOf()))
    )
        return WorldGpuRenderResult::resource_failed;

    D3D11_RASTERIZER_DESC rasterizer_description{};
    rasterizer_description.FillMode = D3D11_FILL_SOLID;
    rasterizer_description.CullMode = D3D11_CULL_NONE;
    rasterizer_description.ScissorEnable = TRUE;
    rasterizer_description.DepthClipEnable = TRUE;
    ComPtr<ID3D11RasterizerState> rasterizer;
    if (FAILED(device->CreateRasterizerState(
            &rasterizer_description,
            rasterizer.GetAddressOf())))
        return WorldGpuRenderResult::resource_failed;

    std::array<ComPtr<ID3D11BlendState>, 5> blend_states;
    blend_states[0] = blend_state(device.Get(), -1);
    for (int mode = 0; mode < 4; ++mode)
        blend_states[mode + 1] = blend_state(device.Get(), mode);
    for (const auto& state : blend_states) {
        if (!state)
            return WorldGpuRenderResult::resource_failed;
    }

    ComPtr<ID3D11DepthStencilState> depth_states[2][2][2][2];
    for (int enabled = 0; enabled < 2; ++enabled) {
        for (int transparent = 0; transparent < 2; ++transparent) {
            for (int check = 0; check < 2; ++check) {
                for (int set = 0; set < 2; ++set) {
                    depth_states[enabled][transparent][check][set] =
                        depth_state(
                            device.Get(),
                            transparent == 0,
                            check != 0,
                            set != 0,
                            enabled != 0);
                    if (!depth_states[enabled][transparent][check][set])
                        return WorldGpuRenderResult::resource_failed;
                }
            }
        }
    }

    const float clear[] = {
        (options.clear_color_rgba8 & 0xFF) / 255.0F,
        ((options.clear_color_rgba8 >> 8) & 0xFF) / 255.0F,
        ((options.clear_color_rgba8 >> 16) & 0xFF) / 255.0F,
        ((options.clear_color_rgba8 >> 24) & 0xFF) / 255.0F,
    };
    context->ClearRenderTargetView(color_view.Get(), clear);
    context->ClearDepthStencilView(
        depth_view.Get(),
        D3D11_CLEAR_DEPTH | D3D11_CLEAR_STENCIL,
        1.0F,
        0);
    ID3D11RenderTargetView* render_target = color_view.Get();
    context->OMSetRenderTargets(1, &render_target, depth_view.Get());
    const D3D11_VIEWPORT viewport{
        0.0F,
        0.0F,
        static_cast<float>(output_width),
        static_cast<float>(output_height),
        0.0F,
        1.0F,
    };
    context->RSSetViewports(1, &viewport);
    context->RSSetState(rasterizer.Get());
    context->IASetInputLayout(input_layout.Get());
    context->IASetPrimitiveTopology(
        D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
    const UINT stride = sizeof(GpuVertex);
    const UINT offset = 0;
    ID3D11Buffer* raw_vertex_buffer = vertex_buffer.Get();
    context->IASetVertexBuffers(
        0, 1, &raw_vertex_buffer, &stride, &offset);
    context->VSSetShader(vertex_shader.Get(), nullptr, 0);
    context->PSSetShader(pixel_shader.Get(), nullptr, 0);
    ID3D11Buffer* raw_constant_buffer = constant_buffer.Get();
    context->PSSetConstantBuffers(0, 1, &raw_constant_buffer);
    ID3D11ShaderResourceView* raw_vram_view = vram_view.Get();
    context->PSSetShaderResources(0, 1, &raw_vram_view);

    std::uint32_t depth_object_kind =
        (std::numeric_limits<std::uint32_t>::max)();
    std::uint32_t depth_object_id =
        (std::numeric_limits<std::uint32_t>::max)();
    std::uint32_t depth_model_pointer =
        (std::numeric_limits<std::uint32_t>::max)();
    std::int32_t depth_bucket =
        (std::numeric_limits<std::int32_t>::min)();
    for (const auto& command : draw_list.commands) {
        if (command.material_index >= draw_list.materials.size())
            return WorldGpuRenderResult::render_failed;
        const auto& material =
            draw_list.materials[command.material_index];
        const bool textured =
            (material.primitive_flags & textured_flag) != 0;
        const bool semitransparent =
            (material.primitive_flags & semi_transparent_flag) != 0;
        const int pass_count =
            textured && semitransparent ? 2 : 1;

        D3D11_MAPPED_SUBRESOURCE mapped{};
        if (FAILED(context->Map(
                vertex_buffer.Get(),
                0,
                D3D11_MAP_WRITE_DISCARD,
                0,
                &mapped)))
            return WorldGpuRenderResult::render_failed;
        auto* gpu_vertices = static_cast<GpuVertex*>(mapped.pData);
        for (int index = 0; index < 3; ++index) {
            const auto& source = command.vertices[index];
            gpu_vertices[index] = GpuVertex{
                {
                    source.clip_x,
                    source.clip_y,
                    source.clip_z,
                    source.clip_w,
                },
                {source.u, source.v},
                {
                    source.r / 255.0F,
                    source.g / 255.0F,
                    source.b / 255.0F,
                    1.0F,
                },
            };
        }
        context->Unmap(vertex_buffer.Get(), 0);

        const D3D11_RECT scissor{
            std::clamp(
                static_cast<LONG>(
                    (command.clip_x0 - draw_list.display_x) *
                    static_cast<std::int32_t>(output_scale)),
                0L,
                static_cast<LONG>(output_width)),
            std::clamp(
                static_cast<LONG>(
                    (command.clip_y0 - draw_list.display_y) *
                    static_cast<std::int32_t>(output_scale)),
                0L,
                static_cast<LONG>(output_height)),
            std::clamp(
                static_cast<LONG>(
                    (command.clip_x1 - draw_list.display_x + 1) *
                    static_cast<std::int32_t>(output_scale)),
                0L,
                static_cast<LONG>(output_width)),
            std::clamp(
                static_cast<LONG>(
                    (command.clip_y1 - draw_list.display_y + 1) *
                    static_cast<std::int32_t>(output_scale)),
                0L,
                static_cast<LONG>(output_height)),
        };
        if (scissor.left >= scissor.right ||
            scissor.top >= scissor.bottom)
            continue;
        context->RSSetScissorRects(1, &scissor);
        const bool identified_world = command.object_kind != 0;
        if (
            options.depth_buffer &&
            identified_world &&
            (
                command.object_kind != depth_object_kind ||
                command.object_id != depth_object_id ||
                command.model_pointer != depth_model_pointer ||
                command.ordering_table_index != depth_bucket
            )
        ) {
            // PS1 ordering-table and object boundaries are visibility layers,
            // not merely sorting hints. Reset depth between them so a modern
            // Z buffer resolves each coherent mesh without overturning GT2's
            // intentional inter-model order (sky, scenery, road, vehicles).
            context->ClearDepthStencilView(
                depth_view.Get(),
                D3D11_CLEAR_DEPTH,
                1.0F,
                0);
            depth_object_kind = command.object_kind;
            depth_object_id = command.object_id;
            depth_model_pointer = command.model_pointer;
            depth_bucket = command.ordering_table_index;
        }

        for (int pass = 0; pass < pass_count; ++pass) {
            const bool blended = semitransparent &&
                (!textured || pass == 1);
            const DrawConstants constants{
                material.primitive_flags,
                material.texture_page,
                material.clut,
                material.environment_flags,
                material.texture_mask_x,
                material.texture_mask_y,
                material.texture_offset_x,
                material.texture_offset_y,
                static_cast<std::uint32_t>(pass),
                options.dithering ? 1U : 0U,
                {0, 0},
            };
            context->UpdateSubresource(
                constant_buffer.Get(),
                0,
                nullptr,
                &constants,
                0,
                0);
            const int blend_mode =
                (material.texture_page >> 5) & 3;
            const float factor =
                blend_mode == 0 ? 0.5F :
                blend_mode == 3 ? 0.25F :
                1.0F;
            const float blend_factor[4] = {
                factor, factor, factor, factor,
            };
            context->OMSetBlendState(
                blended
                    ? blend_states[blend_mode + 1].Get()
                    : blend_states[0].Get(),
                blend_factor,
                0xFFFFFFFFU);
            const bool check_mask =
                (material.environment_flags & check_mask_flag) != 0;
            const bool set_mask =
                (material.environment_flags & set_mask_flag) != 0;
            // Geometry outside an identified world object retains the PS1
            // ordering-table contract. Depth applies to track and vehicle
            // objects, where it can resolve inter-model visibility without
            // allowing legacy sky/background layers to occlude the scene.
            const bool use_depth =
                options.depth_buffer && identified_world;
            context->OMSetDepthStencilState(
                depth_states[use_depth ? 1 : 0]
                    [blended ? 1 : 0]
                    [check_mask ? 1 : 0]
                    [set_mask ? 1 : 0].Get(),
                1);
            context->Draw(3, 0);
            ++stats->draw_calls;
            if (blended)
                ++stats->transparent_draw_calls;
        }
    }

    D3D11_TEXTURE2D_DESC staging_description = color_description;
    staging_description.Usage = D3D11_USAGE_STAGING;
    staging_description.BindFlags = 0;
    staging_description.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
    ComPtr<ID3D11Texture2D> staging;
    if (FAILED(device->CreateTexture2D(
            &staging_description,
            nullptr,
            staging.GetAddressOf())))
        return WorldGpuRenderResult::resource_failed;
    context->CopyResource(staging.Get(), color_texture.Get());
    D3D11_MAPPED_SUBRESOURCE mapped{};
    if (FAILED(context->Map(
            staging.Get(),
            0,
            D3D11_MAP_READ,
            0,
            &mapped)))
        return WorldGpuRenderResult::render_failed;
    const std::size_t row_size =
        static_cast<std::size_t>(output_width) * 4;
    for (std::uint32_t y = 0; y < output_height; ++y) {
        std::memcpy(
            output_rgba + static_cast<std::size_t>(y) * row_size,
            static_cast<const std::uint8_t*>(mapped.pData) +
                static_cast<std::size_t>(y) * mapped.RowPitch,
            row_size);
    }
    context->Unmap(staging.Get(), 0);
    return WorldGpuRenderResult::success;
}

} // namespace opengt::render

#else

namespace opengt::render {

WorldGpuRenderResult render_world_d3d11(
    const WorldDrawList&,
    const std::uint16_t*,
    std::size_t,
    std::uint8_t*,
    std::size_t,
    WorldGpuRenderOptions,
    WorldGpuRenderStats*
) noexcept {
    return WorldGpuRenderResult::unsupported_platform;
}

} // namespace opengt::render

#endif

namespace opengt::render {

const char* world_gpu_render_result_name(
    WorldGpuRenderResult result
) noexcept {
    switch (result) {
        case WorldGpuRenderResult::success: return "success";
        case WorldGpuRenderResult::invalid_argument:
            return "invalid_argument";
        case WorldGpuRenderResult::unsupported_platform:
            return "unsupported_platform";
        case WorldGpuRenderResult::device_failed:
            return "device_failed";
        case WorldGpuRenderResult::shader_failed:
            return "shader_failed";
        case WorldGpuRenderResult::resource_failed:
            return "resource_failed";
        case WorldGpuRenderResult::render_failed:
            return "render_failed";
    }
    return "unknown";
}

} // namespace opengt::render
