#pragma once

#include <cstddef>
#include <cstdint>

namespace opengt::render {

constexpr std::uint32_t projected_capture_version = 1;
constexpr std::uint32_t projected_capture_triangle_stride = 96;
constexpr std::uint32_t projected_capture_max_triangles = 262144;

struct ProjectedCaptureHeader {
    std::uint64_t frame_index;
    std::int32_t input_poll;
    std::int32_t display_x;
    std::int32_t display_y;
    std::int32_t display_width;
    std::int32_t display_height;
    std::uint32_t triangle_count;
    std::uint32_t vram_width;
    std::uint32_t vram_height;
    std::uint64_t vram_offset;
    std::uint64_t vram_size;
    std::uint32_t flags;
};

struct ProjectedCaptureVertex {
    float x;
    float y;
    float z;
    std::int16_t u;
    std::int16_t v;
    std::uint8_t r;
    std::uint8_t g;
    std::uint8_t b;
    bool has_gte_depth;
};

struct ProjectedCaptureTriangle {
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
    ProjectedCaptureVertex vertices[3];
};

enum class CaptureReadResult {
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

CaptureReadResult read_projected_capture_header(
    const char* path,
    ProjectedCaptureHeader* header
) noexcept;

CaptureReadResult load_projected_capture(
    const char* path,
    ProjectedCaptureHeader* header,
    ProjectedCaptureTriangle* triangles,
    std::size_t triangle_capacity,
    std::uint16_t* vram,
    std::size_t vram_word_capacity
) noexcept;

const char* capture_read_result_name(CaptureReadResult result) noexcept;

} // namespace opengt::render
