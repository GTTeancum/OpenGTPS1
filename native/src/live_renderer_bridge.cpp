#include "opengt/live_renderer_bridge.h"

#include "opengt/world_capture.hpp"
#include "opengt/world_draw_list.hpp"
#include "opengt/world_gpu_renderer.hpp"
#include "opengt/world_topology.hpp"

#include <chrono>
#include <cstdint>
#include <new>
#include <vector>

namespace {

constexpr std::uint32_t api_version = 1;

struct LiveContext {
    std::vector<opengt::render::WorldCaptureTriangle> triangles;
    std::vector<std::uint16_t> vram;
    opengt::render::WorldDrawList draw_list;

    LiveContext() : vram(1024U * 512U) {}
};

void clear_stats(opengt_live_stats* stats) {
    if (stats == nullptr)
        return;
    const std::uint32_t size = stats->struct_size;
    *stats = {};
    stats->struct_size = size;
}

int32_t fail(opengt_live_stats* stats, std::uint32_t result) {
    if (stats != nullptr)
        stats->result = result;
    return -static_cast<std::int32_t>(result);
}

} // namespace

extern "C" {

void* opengt_live_create(void) {
    try {
        return new LiveContext();
    } catch (...) {
        return nullptr;
    }
}

void opengt_live_destroy(void* handle) {
    delete static_cast<LiveContext*>(handle);
}

int32_t opengt_live_render(
    void* handle,
    const std::uint8_t* capture_bytes,
    std::size_t capture_size,
    std::uint8_t* output_rgba,
    std::size_t output_capacity,
    const opengt_live_options* options,
    opengt_live_stats* stats
) {
    clear_stats(stats);
    if (
        handle == nullptr ||
        capture_bytes == nullptr ||
        output_rgba == nullptr ||
        options == nullptr ||
        stats == nullptr ||
        options->struct_size != sizeof(opengt_live_options) ||
        stats->struct_size != sizeof(opengt_live_stats)
    )
        return fail(stats, 1);
    try {
        auto* context = static_cast<LiveContext*>(handle);
        opengt::render::WorldCaptureHeader header{};
        opengt::render::WorldCaptureTriangle probe_triangle{};
        std::uint16_t probe_vram{};
        auto read_result =
            opengt::render::load_world_capture_memory(
                capture_bytes,
                capture_size,
                &header,
                &probe_triangle,
                0,
                &probe_vram,
                0);
        if (
            read_result !=
            opengt::render::WorldCaptureReadResult::capacity_too_small
        )
            return fail(
                stats,
                100U + static_cast<std::uint32_t>(read_result));
        context->triangles.resize(header.triangle_count);
        read_result = opengt::render::load_world_capture_memory(
            capture_bytes,
            capture_size,
            &header,
            context->triangles.data(),
            context->triangles.size(),
            context->vram.data(),
            context->vram.size());
        if (
            read_result != opengt::render::WorldCaptureReadResult::success
        )
            return fail(
                stats,
                100U + static_cast<std::uint32_t>(read_result));

        context->draw_list = {};
        const auto list_result =
            opengt::render::build_world_draw_list(
                header,
                context->triangles.data(),
                context->triangles.size(),
                // A live GT2 frame needs both authored projections: the main
                // race view and the rear-view mirror. Diagnostic standalone
                // renders may isolate the main camera, but excluding the
                // secondary channel here removes visible race content.
                opengt::render::WorldDrawListOptions{
                    true,
                    true,
                    (options->flags & OPENGT_LIVE_TOPOLOGY) != 0},
                &context->draw_list);
        if (
            list_result != opengt::render::WorldDrawListResult::success
        )
            return fail(
                stats,
                200U + static_cast<std::uint32_t>(list_result));

        opengt::render::WorldTopologyStats topology{};
        if ((options->flags & OPENGT_LIVE_TOPOLOGY) != 0) {
            const auto topology_result =
                opengt::render::apply_world_topology(
                    &context->draw_list,
                    opengt::render::WorldTopologyOptions{
                        true, true, true},
                    &topology);
            if (
                topology_result !=
                opengt::render::WorldTopologyResult::success
            )
                return fail(
                    stats,
                    300U +
                    static_cast<std::uint32_t>(topology_result));
        }

        const auto started = std::chrono::steady_clock::now();
        opengt::render::WorldGpuRenderStats render_stats{};
        const auto render_result =
            opengt::render::render_world_d3d11(
                context->draw_list,
                context->vram.data(),
                context->vram.size(),
                output_rgba,
                output_capacity,
                opengt::render::WorldGpuRenderOptions{
                    (options->flags & OPENGT_LIVE_WARP) != 0,
                    (options->flags & OPENGT_LIVE_DEPTH) != 0,
                    (options->flags & OPENGT_LIVE_DITHER) != 0,
                    (options->flags & OPENGT_LIVE_PERSPECTIVE) != 0,
                    options->output_scale,
                    options->clear_color_rgba8,
                },
                &render_stats);
        const auto finished = std::chrono::steady_clock::now();
        if (
            render_result != opengt::render::WorldGpuRenderResult::success
        )
            return fail(
                stats,
                400U + static_cast<std::uint32_t>(render_result));

        stats->result = 0;
        stats->capture_triangles = header.triangle_count;
        stats->output_commands =
            static_cast<std::uint32_t>(
                context->draw_list.commands.size());
        stats->draw_calls = render_stats.draw_calls;
        stats->transparent_draw_calls =
            render_stats.transparent_draw_calls;
        stats->topology_boundary_groups =
            topology.authored_boundary_groups;
        stats->topology_t_junctions = topology.exact_t_junctions;
        stats->topology_split_triangles =
            topology.emitted_split_triangles;
        stats->topology_coplanar_pairs =
            topology.coplanar_overlap_pairs;
        stats->topology_ownership_reorders =
            topology.ownership_reorders;
        stats->output_width =
            static_cast<std::uint32_t>(header.display_width) *
            options->output_scale;
        stats->output_height =
            static_cast<std::uint32_t>(header.display_height) *
            options->output_scale;
        stats->render_microseconds =
            static_cast<std::uint64_t>(
                std::chrono::duration_cast<std::chrono::microseconds>(
                    finished - started).count());
        stats->frame_index = header.frame_index;
        stats->input_poll = header.input_poll;
        return 0;
    } catch (const std::bad_alloc&) {
        return fail(stats, 2);
    } catch (...) {
        return fail(stats, 3);
    }
}

std::uint32_t opengt_live_api_version(void) {
    return api_version;
}

} // extern "C"
