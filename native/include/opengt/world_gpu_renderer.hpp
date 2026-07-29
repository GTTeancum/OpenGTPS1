#pragma once

#include "opengt/world_draw_list.hpp"

#include <cstddef>
#include <cstdint>

namespace opengt::render {

struct WorldGpuRenderOptions {
    bool use_software_adapter;
    bool depth_buffer;
    bool dithering;
    std::uint32_t output_scale;
    std::uint32_t clear_color_rgba8;
};

struct WorldGpuRenderStats {
    std::uint32_t commands;
    std::uint32_t draw_calls;
    std::uint32_t transparent_draw_calls;
    std::uint32_t secondary_commands;
    bool software_adapter;
};

enum class WorldGpuRenderResult {
    success,
    invalid_argument,
    unsupported_platform,
    device_failed,
    shader_failed,
    resource_failed,
    render_failed,
};

WorldGpuRenderResult render_world_d3d11(
    const WorldDrawList& draw_list,
    const std::uint16_t* vram,
    std::size_t vram_word_count,
    std::uint8_t* output_rgba,
    std::size_t output_size,
    WorldGpuRenderOptions options,
    WorldGpuRenderStats* stats
) noexcept;

const char* world_gpu_render_result_name(
    WorldGpuRenderResult result
) noexcept;

} // namespace opengt::render
