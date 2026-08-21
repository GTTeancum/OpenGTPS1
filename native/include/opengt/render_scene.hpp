#pragma once

#include <cstddef>
#include <cstdint>
#include <type_traits>

namespace opengt::render {

struct Vec2 {
    float x;
    float y;
};

struct Vec3 {
    float x;
    float y;
    float z;
};

struct Mat4 {
    // Column-major so the same frame data can feed OpenGL and the NV2A
    // transform path without a platform-specific scene representation.
    float elements[16];
};

struct RenderVertex {
    Vec3 position;
    Vec3 normal;
    Vec2 texcoord;
    std::uint32_t color_rgba8;
};

enum class BlendMode : std::uint8_t {
    opaque,
    alpha,
    additive,
    subtractive,
    quarter_additive,
};

enum class TextureFilter : std::uint8_t {
    nearest,
    bilinear,
};

struct Material {
    std::uint32_t texture_id;
    std::uint32_t flags;
    BlendMode blend_mode;
    TextureFilter texture_filter;
    std::uint16_t reserved;
    float alpha_cutoff;
    float specular_strength;
};

struct MeshView {
    const RenderVertex* vertices;
    std::uint32_t vertex_count;
    const std::uint32_t* indices;
    std::uint32_t index_count;
};

struct DrawItem {
    MeshView mesh;
    Mat4 model_matrix;
    std::uint32_t material_index;
    std::uint32_t sort_key;
};

struct Camera {
    Mat4 view_matrix;
    Mat4 projection_matrix;
    Vec3 world_position;
    float near_plane;
    float far_plane;
};

struct FrameView {
    Camera camera;
    const Material* materials;
    std::uint32_t material_count;
    const DrawItem* draws;
    std::uint32_t draw_count;
};

struct FrameOptions {
    bool perspective_correct_textures;
    bool dithering;
    bool force_highest_lod;
    bool extended_draw_distance;
};

class IRenderBackend {
public:
    virtual ~IRenderBackend() = default;

    virtual bool initialize() noexcept = 0;
    virtual bool render(
        const FrameView& frame,
        const FrameOptions& options
    ) noexcept = 0;
    virtual void shutdown() noexcept = 0;
};

static_assert(std::is_standard_layout_v<RenderVertex>);
static_assert(std::is_standard_layout_v<Material>);
static_assert(sizeof(RenderVertex) == 36);

} // namespace opengt::render
