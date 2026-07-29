#pragma once

#include "opengt/projected_capture.hpp"

#include <cstddef>
#include <cstdint>

namespace opengt::render {

struct ProjectedRenderOptions {
    bool perspective_correct_textures;
    bool dithering;
    std::uint32_t clear_color_rgba8;
};

struct ProjectedRenderStats {
    std::uint32_t submitted_triangles;
    std::uint32_t rasterized_triangles;
    std::uint32_t perspective_corrected_triangles;
    std::uint64_t shaded_pixels;
    std::uint64_t transparent_pixels;
};

ProjectedRenderStats render_projected_capture(
    const ProjectedCaptureHeader& header,
    const ProjectedCaptureTriangle* triangles,
    std::size_t triangle_count,
    const std::uint16_t* vram,
    std::size_t vram_word_count,
    std::uint8_t* output_rgba,
    std::size_t output_size,
    ProjectedRenderOptions options
) noexcept;

} // namespace opengt::render
