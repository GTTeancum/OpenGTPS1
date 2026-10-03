#pragma once

#include <cstddef>
#include <cstdint>

namespace opengt::render {

// Version-6 optional flag: header camera consumes GT2 primary-mesh X/Y/Z-up
// coordinates, rather than canonical X/-Z/Y. No struct or C ABI layout change.
constexpr std::uint32_t world_capture_primary_camera_axes_flag = 1U << 3;
// L08: reserved v6 bytes carry an independently verified primary-world anchor.
constexpr std::uint32_t world_capture_world_anchor_flag = 1U << 4;
constexpr std::uint32_t world_capture_version = 6;
constexpr std::uint32_t world_capture_header_size = 160;
constexpr std::uint32_t world_capture_v1_header_size = 128;
constexpr std::uint32_t world_capture_legacy_triangle_stride = 176;
constexpr std::uint32_t world_capture_v3_triangle_stride = 212;
constexpr std::uint32_t world_capture_v4_triangle_stride = 224;
constexpr std::uint32_t world_capture_v5_triangle_stride = 256;
constexpr std::uint32_t world_capture_triangle_stride = 384;
constexpr std::uint32_t world_capture_max_triangles = 262144;

struct WorldCaptureHeader {
    std::uint32_t version;
    std::uint32_t header_size;
    std::uint64_t frame_index;
    std::int32_t input_poll;
    std::int32_t display_x;
    std::int32_t display_y;
    std::int32_t display_width;
    std::int32_t display_height;
    std::uint32_t triangle_count;
    std::uint32_t triangle_stride;
    std::uint32_t vram_width;
    std::uint32_t vram_height;
    std::uint64_t triangle_offset;
    std::uint64_t vram_offset;
    std::uint64_t vram_size;
    std::uint32_t flags;
    std::uint64_t camera_transform_id;
    std::int16_t camera_rotation[9];
    std::int32_t camera_translation[3];
    std::int32_t projection_offset_x;
    std::int32_t projection_offset_y;
    std::uint32_t projection_plane;
    std::int32_t draw_offset_x;
    std::int32_t draw_offset_y;
    std::int16_t camera_depth_scale_exponent{};
    std::int32_t camera_world_offset[3]{}; // negative source camera; primary axes, raw common units
};

struct WorldCaptureVertex {
    float screen_x;
    float screen_y;
    float screen_z;
    std::int16_t u;
    std::int16_t v;
    std::uint8_t r;
    std::uint8_t g;
    std::uint8_t b;
    bool world_valid;
    bool screen_offset_anchor;
    std::int16_t model_x;
    std::int16_t model_y;
    std::int16_t model_z;
    std::int32_t view_x;
    std::int32_t view_y;
    std::int32_t view_z;
    std::int32_t projection_offset_x;
    std::int32_t projection_offset_y;
    std::uint32_t projection_plane;
    std::uint32_t source_vertex_identity;
    std::uint64_t transform_id;
    std::int16_t transform_rotation[9];
    std::int32_t transform_translation[3];
    bool exact_transform_valid;
    float world_x;
    float world_y;
    float world_z;
};

struct WorldCaptureTriangle {
    std::uint32_t primitive_flags;
    std::uint16_t texture_page;
    std::uint16_t clut;
    std::int32_t ordering_table_index;
    std::int16_t clip_x0;
    std::int16_t clip_y0;
    std::int16_t clip_x1;
    std::int16_t clip_y1;
    std::int16_t texture_mask_x;
    std::int16_t texture_mask_y;
    std::int16_t texture_offset_x;
    std::int16_t texture_offset_y;
    std::uint32_t environment_flags;
    std::uint32_t object_kind;
    std::uint32_t object_id;
    std::uint32_t model_pointer;
    std::int16_t draw_offset_x;
    std::int16_t draw_offset_y;
    std::uint64_t transform_id;
    std::int16_t transform_rotation[9];
    std::int32_t transform_translation[3];
    bool exact_transform_valid;
    std::int32_t depth_scale_exponent;
    bool depth_scale_valid;
    WorldCaptureVertex vertices[3];
    // Live resident-course provenance. These fields are intentionally outside
    // the serialized v6 triangle contract: file captures decode them as zero,
    // while the in-process resident path can carry an exact source primitive.
    std::uint64_t source_mesh_key{};
    std::uint32_t source_primitive_address{};
    // Path-neutral fingerprint of the complete immutable resident definition.
    // This is live-only diagnostic provenance; serialized captures decode zero.
    std::uint64_t resident_content_key{};
};

enum class WorldCaptureReadResult {
    success,
    invalid_argument,
    open_failed,
    truncated,
    invalid_magic,
    unsupported_version,
    invalid_layout,
    capacity_too_small,
    io_error,
};

WorldCaptureReadResult read_world_capture_header(
    const char* path,
    WorldCaptureHeader* header
) noexcept;

WorldCaptureReadResult load_world_capture(
    const char* path,
    WorldCaptureHeader* header,
    WorldCaptureTriangle* triangles,
    std::size_t triangle_capacity,
    std::uint16_t* vram,
    std::size_t vram_word_capacity
) noexcept;

WorldCaptureReadResult load_world_capture_memory(
    const std::uint8_t* bytes,
    std::size_t byte_count,
    WorldCaptureHeader* header,
    WorldCaptureTriangle* triangles,
    std::size_t triangle_capacity,
    std::uint16_t* vram,
    std::size_t vram_word_capacity
) noexcept;

const char* world_capture_read_result_name(
    WorldCaptureReadResult result
) noexcept;

} // namespace opengt::render
