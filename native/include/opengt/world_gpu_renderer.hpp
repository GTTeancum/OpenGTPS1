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
constexpr std::size_t world_gpu_async_readback_image_capacity =
    world_gpu_async_readback_pair_capacity * 2;

struct WorldGpuRenderOptions {
    bool use_software_adapter;
    bool depth_buffer;
    bool dithering;
    bool perspective_correct;
    bool texture_smoothing;
    bool high_resolution_textures;
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

struct WorldTextureUpload {
    std::uint64_t key;
    std::int32_t x;
    std::int32_t y;
    std::int32_t word_width;
    std::int32_t height;
};

// A car paint is identified by both its indexed bitmap upload and the exact
// 64x4 palette-bank upload containing the primitive's 16-word CLUT.
constexpr bool world_texture_upload_contains_clut(
    const WorldTextureUpload& upload,
    std::uint64_t expected_key,
    std::uint16_t clut
) noexcept {
    const std::int64_t clut_x = static_cast<std::int64_t>(clut & 63U) * 16;
    const std::int64_t clut_y = (clut >> 6U) & 511U;
    return
        upload.key == expected_key &&
        upload.word_width > 0 && upload.height > 0 &&
        clut_x >= upload.x && clut_y >= upload.y &&
        clut_x + 16 <= static_cast<std::int64_t>(upload.x) + upload.word_width &&
        clut_y + 1 <= static_cast<std::int64_t>(upload.y) + upload.height;
}

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

WorldGpuReadbackResult try_read_world_d3d11_image(
    bool use_software_adapter,
    std::uint8_t* output_rgba,
    std::size_t output_size,
    bool wait_for_completion = false
) noexcept;

std::size_t pending_world_d3d11_readback_pairs(
    bool use_software_adapter
) noexcept;

// Begins a new temporal stream. The next render is read back synchronously;
// later renders resume the two-pair staging overlap.
void reset_world_d3d11_readback(bool use_software_adapter) noexcept;

bool set_world_d3d11_texture_uploads(
    bool use_software_adapter,
    const WorldTextureUpload* uploads,
    std::size_t upload_count) noexcept;

const char* world_gpu_render_result_name(
    WorldGpuRenderResult result
) noexcept;

} // namespace opengt::render
