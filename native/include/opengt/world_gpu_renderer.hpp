#pragma once

#include "opengt/world_draw_list.hpp"

#include <cstddef>
#include <cstdint>

namespace opengt::render {

// Readback retains whole midpoint/actual pairs. Both the native staging ring
// and the bridge's metadata FIFO derive their sizes from this one invariant.
constexpr std::size_t world_gpu_readback_pair_delay = 2;
// The live renderer can continue submitting unique midpoint/actual pairs while
// a prior staging copy is delayed by GPU scheduling. Eight pairs cover more
// than a quarter second at the authored 30 Hz cadence without changing the
// normal two-pair presentation latency.
constexpr std::size_t world_gpu_async_readback_pair_capacity = 8;

struct WorldGpuRenderOptions {
    bool use_software_adapter;
    bool depth_buffer;
    bool dithering;
    bool perspective_correct;
    bool texture_smoothing;
    std::uint32_t output_scale;
    std::uint32_t clear_color_rgba8;
    // A generated midpoint can reuse the prior authored frame's immutable
    // texture/material inputs when the caller has proven exact compatibility.
    bool reuse_uploaded_vram = false;
    bool reuse_uploaded_materials = false;
    // Diagnostics only: identifies the generated half-step submitted before
    // an authored current frame. Rendering behavior must not depend on it.
    bool synthetic_midpoint = false;
    // Cadence-gap reset recovery can re-prime the readback ring without
    // forcing the first staged copy to block the worker thread.
    bool defer_initial_readback = false;
    // Submit this image to the live pair readback queue without synchronously
    // waiting for an older staging resource. Completed pairs are drained by
    // try_read_world_d3d11_pair in chronological order.
    bool asynchronous_readback = false;
};

struct WorldGpuRenderStats {
    std::uint32_t commands;
    std::uint32_t draw_calls;
    std::uint32_t transparent_draw_calls;
    std::uint32_t secondary_commands;
    // Commands dropped because their guest drawing area was empty.
    std::uint32_t skipped_empty_scissor_commands;
    bool software_adapter;
    bool output_valid;
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

enum class WorldGpuReadbackResult {
    success,
    not_ready,
    invalid_argument,
    device_failed,
    read_failed,
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

WorldGpuReadbackResult try_read_world_d3d11_pair(
    bool use_software_adapter,
    std::uint8_t* first_output_rgba,
    std::uint8_t* second_output_rgba,
    std::size_t output_size,
    bool wait_for_completion = false
) noexcept;

std::size_t pending_world_d3d11_readback_pairs(
    bool use_software_adapter
) noexcept;

// Begins a new temporal stream. The next render is read back synchronously;
// later renders resume the two-pair staging overlap.
void reset_world_d3d11_readback(bool use_software_adapter) noexcept;

const char* world_gpu_render_result_name(
    WorldGpuRenderResult result
) noexcept;

} // namespace opengt::render
