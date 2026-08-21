#include "opengt/live_renderer_bridge.h"

#include "opengt/world_capture.hpp"
#include "opengt/world_draw_list.hpp"
#include "opengt/world_gpu_renderer.hpp"
#include "opengt/world_interpolation.hpp"
#include "opengt/world_topology.hpp"

#include <chrono>
#include <array>
#include <cstdio>
#include <cstdlib>
#include <cstdint>
#include <cstring>
#include <new>
#include <utility>
#include <vector>

namespace {

constexpr std::uint32_t api_version = 6;
static_assert(sizeof(opengt_live_interpolation_stats) == 128);
using Clock = std::chrono::steady_clock;

struct BuiltFrame {
    opengt::render::WorldCaptureHeader header{};
    opengt::render::WorldTopologyStats topology{};
    std::uint64_t decode_microseconds{};
    std::uint64_t draw_list_microseconds{};
    std::uint64_t topology_microseconds{};
    Clock::time_point pipeline_started{};
};

struct LiveContext {
    std::vector<opengt::render::WorldCaptureTriangle> triangles;
    std::vector<std::uint16_t> vram;
    opengt::render::WorldDrawList draw_list;
    opengt::render::WorldInterpolationCache interpolation_cache;

    bool has_previous{};
    std::vector<std::uint16_t> previous_vram;
    opengt::render::WorldDrawList previous_draw_list;
    opengt::render::WorldInterpolationCache previous_interpolation_cache;
    opengt::render::WorldCaptureHeader previous_header{};
    opengt_live_stats previous_actual_stats{};
    opengt_live_options previous_options{};
    // Mirror the asynchronous D3D staging capacity exactly so pixels and frame
    // identity can never drift.
    std::array<
        opengt_live_stats,
        opengt::render::world_gpu_async_readback_pair_capacity>
        pending_midpoint_stats{};
    std::array<
        opengt_live_stats,
        opengt::render::world_gpu_async_readback_pair_capacity>
        pending_actual_stats{};
    std::uint32_t pending_output_pair_read{};
    std::uint32_t pending_output_pair_write{};
    std::uint32_t pending_output_pair_count{};
    bool readback_software_adapter{};
    std::array<
        opengt_live_stats,
        opengt::render::world_gpu_async_readback_image_capacity>
        pending_authored_stats{};
    std::uint32_t pending_authored_read{};
    std::uint32_t pending_authored_write{};
    std::uint32_t pending_authored_count{};

    LiveContext()
        : vram(1024U * 512U), previous_vram(1024U * 512U) {}
};

int32_t try_read_pending_authored(
    LiveContext* context,
    std::uint8_t* output,
    std::size_t output_capacity,
    opengt_live_stats* stats,
    bool wait_for_completion
) {
    if (context->pending_authored_count == 0)
        return 0;
    const auto result = opengt::render::try_read_world_d3d11_image(
        context->readback_software_adapter,
        output,
        output_capacity,
        wait_for_completion);
    if (result == opengt::render::WorldGpuReadbackResult::not_ready)
        return 0;
    if (result != opengt::render::WorldGpuReadbackResult::success)
        return -static_cast<std::int32_t>(
            700U + static_cast<std::uint32_t>(result));
    const std::uint32_t read = context->pending_authored_read;
    *stats = context->pending_authored_stats[read];
    context->pending_authored_read =
        (read + 1U) % context->pending_authored_stats.size();
    --context->pending_authored_count;
    return 1;
}

template<typename T>
void clear_struct(T* value) {
    if (value == nullptr)
        return;
    const std::uint32_t size = value->struct_size;
    *value = {};
    value->struct_size = size;
}

int32_t fail(opengt_live_stats* stats, std::uint32_t result) {
    if (stats != nullptr)
        stats->result = result;
    return -static_cast<std::int32_t>(result);
}

int32_t fail_pair(
    opengt_live_stats* first,
    opengt_live_stats* second,
    opengt_live_interpolation_stats* interpolation,
    std::uint32_t result
) {
    if (first != nullptr)
        first->result = result;
    if (second != nullptr)
        second->result = result;
    if (interpolation != nullptr)
        interpolation->result = result;
    return -static_cast<std::int32_t>(result);
}

int32_t try_read_pending_pair(
    LiveContext* context,
    std::uint8_t* first_output,
    std::uint8_t* second_output,
    std::size_t output_capacity,
    opengt_live_stats* first_stats,
    opengt_live_stats* second_stats,
    bool wait_for_completion
) {
    if (context->pending_output_pair_count == 0)
        return 0;
    const auto result = opengt::render::try_read_world_d3d11_pair(
        context->readback_software_adapter,
        first_output,
        second_output,
        output_capacity,
        wait_for_completion);
    if (result == opengt::render::WorldGpuReadbackResult::not_ready)
        return 0;
    if (result != opengt::render::WorldGpuReadbackResult::success)
        return -static_cast<std::int32_t>(
            700U + static_cast<std::uint32_t>(result));
    const std::uint32_t read = context->pending_output_pair_read;
    *first_stats = context->pending_midpoint_stats[read];
    *second_stats = context->pending_actual_stats[read];
    context->pending_output_pair_read =
        (read + 1U) % context->pending_midpoint_stats.size();
    --context->pending_output_pair_count;
    return 1;
}

std::uint64_t microseconds(Clock::duration duration) {
    return static_cast<std::uint64_t>(
        std::chrono::duration_cast<std::chrono::microseconds>(
            duration).count());
}

std::uint32_t build_frame(
    LiveContext* context,
    const std::uint8_t* capture_bytes,
    std::size_t capture_size,
    const opengt_live_options& options,
    BuiltFrame* built,
    bool apply_topology = true,
    bool repair_projected_topology = true
) {
    using namespace opengt::render;
    built->pipeline_started = Clock::now();
    WorldCaptureTriangle probe_triangle{};
    std::uint16_t probe_vram{};
    auto result = load_world_capture_memory(
        capture_bytes,
        capture_size,
        &built->header,
        &probe_triangle,
        0,
        &probe_vram,
        0);
    if (result != WorldCaptureReadResult::capacity_too_small)
        return 100U + static_cast<std::uint32_t>(result);
    context->triangles.resize(built->header.triangle_count);
    result = load_world_capture_memory(
        capture_bytes,
        capture_size,
        &built->header,
        context->triangles.data(),
        context->triangles.size(),
        context->vram.data(),
        context->vram.size());
    if (result != WorldCaptureReadResult::success)
        return 100U + static_cast<std::uint32_t>(result);
    const auto decoded = Clock::now();

    context->draw_list = {};
    context->interpolation_cache.reset();
    const auto list_result = build_world_draw_list(
        built->header,
        context->triangles.data(),
        context->triangles.size(),
        WorldDrawListOptions{
            true,
            true,
            (options.flags & OPENGT_LIVE_TOPOLOGY) != 0},
        &context->draw_list);
    if (list_result != WorldDrawListResult::success)
        return 200U + static_cast<std::uint32_t>(list_result);
    const auto draw_list_built = Clock::now();

    if (apply_topology && (options.flags & OPENGT_LIVE_TOPOLOGY) != 0) {
        const auto topology_result = apply_world_topology(
            &context->draw_list,
            WorldTopologyOptions{
                true,
                true,
                true,
                repair_projected_topology,
                // Topology is rebuilt independently every authored frame.
                // Repair the one-pixel raster-visible boundary; spending the
                // 192-pixel diagnostic margin cannot affect this frame's
                // visible seam and consumed most of the 59.94 Hz budget.
                false},
            &built->topology);
        if (topology_result != WorldTopologyResult::success)
            return 300U + static_cast<std::uint32_t>(topology_result);
    }
    const auto topology_finished = Clock::now();
    built->decode_microseconds = microseconds(
        decoded - built->pipeline_started);
    built->draw_list_microseconds = microseconds(
        draw_list_built - decoded);
    built->topology_microseconds = microseconds(
        topology_finished - draw_list_built);
    return 0;
}

opengt::render::WorldGpuRenderOptions gpu_options(
    const opengt_live_options& options
) {
    return opengt::render::WorldGpuRenderOptions{
        (options.flags & OPENGT_LIVE_WARP) != 0,
        (options.flags & OPENGT_LIVE_DEPTH) != 0,
        (options.flags & OPENGT_LIVE_DITHER) != 0,
        (options.flags & OPENGT_LIVE_PERSPECTIVE) != 0,
        (options.flags & OPENGT_LIVE_TEXTURE_SMOOTHING) != 0,
        (options.flags & OPENGT_LIVE_HIGH_RESOLUTION_TEXTURES) != 0,
        options.output_scale,
        options.clear_color_rgba8,
    };
}

void fill_stats(
    const BuiltFrame& built,
    const opengt::render::WorldDrawList& draw_list,
    const opengt::render::WorldGpuRenderStats& render,
    std::uint64_t render_microseconds,
    std::uint64_t pipeline_microseconds,
    std::uint32_t output_scale,
    opengt_live_stats* stats
) {
    stats->result = 0;
    stats->capture_triangles = built.header.triangle_count;
    stats->output_commands = static_cast<std::uint32_t>(
        draw_list.commands.size());
    stats->draw_calls = render.draw_calls;
    stats->transparent_draw_calls = render.transparent_draw_calls;
    stats->topology_boundary_groups =
        built.topology.authored_boundary_groups;
    stats->topology_t_junctions =
        built.topology.exact_t_junctions +
        built.topology.projected_t_junctions;
    stats->topology_split_triangles =
        built.topology.emitted_split_triangles;
    stats->topology_coplanar_pairs =
        built.topology.coplanar_overlap_pairs;
    stats->topology_ownership_reorders =
        built.topology.ownership_reorders;
    stats->output_width =
        static_cast<std::uint32_t>(draw_list.display_width) * output_scale;
    stats->output_height =
        static_cast<std::uint32_t>(draw_list.display_height) * output_scale;
    stats->render_microseconds = render_microseconds;
    stats->frame_index = built.header.frame_index;
    stats->input_poll = built.header.input_poll;
    stats->decode_microseconds = built.decode_microseconds;
    stats->draw_list_microseconds = built.draw_list_microseconds;
    stats->topology_microseconds = built.topology_microseconds;
    stats->pipeline_microseconds = pipeline_microseconds;
}

std::uint32_t render_frame(
    const BuiltFrame& built,
    const opengt::render::WorldDrawList& draw_list,
    const std::uint16_t* vram,
    std::size_t vram_size,
    std::uint8_t* output,
    std::size_t output_capacity,
    const opengt_live_options& options,
    opengt_live_stats* stats,
    bool reuse_uploaded_vram = false,
    bool reuse_uploaded_materials = false,
    bool synthetic_midpoint = false,
    bool defer_initial_readback = false,
    bool asynchronous_readback = false,
    bool* output_valid = nullptr
) {
    const auto gpu_started = Clock::now();
    opengt::render::WorldGpuRenderStats render{};
    auto render_options = gpu_options(options);
    render_options.reuse_uploaded_vram = reuse_uploaded_vram;
    render_options.reuse_uploaded_materials = reuse_uploaded_materials;
    render_options.synthetic_midpoint = synthetic_midpoint;
    render_options.defer_initial_readback = defer_initial_readback;
    render_options.asynchronous_readback = asynchronous_readback;
    const auto result = opengt::render::render_world_d3d11(
        draw_list,
        vram,
        vram_size,
        output,
        output_capacity,
        render_options,
        &render);
    const auto gpu_finished = Clock::now();
    if (result != opengt::render::WorldGpuRenderResult::success)
        return 400U + static_cast<std::uint32_t>(result);
    if (output_valid != nullptr)
        *output_valid = render.output_valid;
    fill_stats(
        built,
        draw_list,
        render,
        microseconds(gpu_finished - gpu_started),
        microseconds(gpu_finished - built.pipeline_started),
        options.output_scale,
        stats);
    return 0;
}

void fill_deferred_reset_stats(
    const BuiltFrame& current,
    const opengt::render::WorldDrawList& draw_list,
    const opengt_live_options& options,
    Clock::time_point pair_started,
    opengt_live_stats* stats
) {
    opengt::render::WorldGpuRenderStats render{};
    render.commands = static_cast<std::uint32_t>(draw_list.commands.size());
    render.secondary_commands = draw_list.secondary_commands;
    fill_stats(
        current,
        draw_list,
        render,
        0,
        microseconds(Clock::now() - pair_started),
        options.output_scale,
        stats);
}

bool midpoint_uploads_match_previous(
    const opengt::render::WorldDrawList& previous,
    const opengt::render::WorldDrawList& midpoint
) {
    using opengt::render::WorldMaterial;
    if (previous.commands.size() != midpoint.commands.size())
        return false;

    const auto same_material = [](
        const WorldMaterial& left,
        const WorldMaterial& right
    ) {
        return
            left.primitive_flags == right.primitive_flags &&
            left.texture_page == right.texture_page &&
            left.clut == right.clut &&
            left.texture_mask_x == right.texture_mask_x &&
            left.texture_mask_y == right.texture_mask_y &&
            left.texture_offset_x == right.texture_offset_x &&
            left.texture_offset_y == right.texture_offset_y &&
            left.environment_flags == right.environment_flags;
    };

    for (std::size_t index = 0; index < previous.commands.size(); ++index) {
        const auto& left = previous.commands[index];
        const auto& right = midpoint.commands[index];
        if (
            left.material_index >= previous.materials.size() ||
            right.material_index >= midpoint.materials.size() ||
            left.object_kind != right.object_kind ||
            !same_material(
                previous.materials[left.material_index],
                midpoint.materials[right.material_index])
        )
            return false;
        for (int vertex = 0; vertex < 3; ++vertex) {
            if (
                left.vertices[vertex].model_x !=
                    right.vertices[vertex].model_x ||
                left.vertices[vertex].model_y !=
                    right.vertices[vertex].model_y ||
                left.vertices[vertex].model_z !=
                    right.vertices[vertex].model_z
            )
                return false;
        }
    }
    return true;
}

const char* temporal_stream_reset_reason(
    const LiveContext& context,
    const BuiltFrame& current,
    const opengt_live_options& options
) {
    if (!context.has_previous)
        return "no_previous";
    const auto& previous = context.previous_header;
    const std::uint64_t frame_delta =
        current.header.frame_index > previous.frame_index
            ? current.header.frame_index - previous.frame_index
            : 0;
    const std::int64_t poll_delta =
        static_cast<std::int64_t>(current.header.input_poll) -
        previous.input_poll;
    if (frame_delta == 0)
        return "frame_not_advancing";
    if (frame_delta > 4)
        return "frame_gap";
    if (poll_delta <= 0)
        return "poll_not_advancing";
    if (poll_delta > 4)
        return "poll_gap";
    if (
        current.header.display_width != previous.display_width ||
        current.header.display_height != previous.display_height)
        return "display_size";
    if (options.flags != context.previous_options.flags)
        return "options_flags";
    if (options.output_scale != context.previous_options.output_scale)
        return "output_scale";
    if (
        options.clear_color_rgba8 !=
        context.previous_options.clear_color_rgba8)
        return "clear_color";
    return nullptr;
}

void log_temporal_reset(
    const LiveContext& context,
    const BuiltFrame& current,
    const opengt_live_options& options,
    const char* reason
) {
    if (std::getenv("OPENGT_TEMPORAL_STREAM_DIAGNOSTICS") == nullptr)
        return;
    const auto& previous = context.previous_header;
    const std::uint64_t frame_delta =
        context.has_previous && current.header.frame_index > previous.frame_index
            ? current.header.frame_index - previous.frame_index
            : 0;
    const std::int64_t poll_delta = context.has_previous
        ? static_cast<std::int64_t>(current.header.input_poll) -
            previous.input_poll
        : 0;
    std::fprintf(
        stderr,
        "[Native-Temporal] reset reason=%s frame=%llu prevFrame=%llu "
        "frameDelta=%llu poll=%d prevPoll=%d pollDelta=%lld "
        "size=%dx%d prevSize=%dx%d flags=%u prevFlags=%u scale=%u "
        "prevScale=%u clear=%08x prevClear=%08x camera=%llu prevCamera=%llu "
        "commands=%zu prevCommands=%zu track=%u prevTrack=%u vehicle=%u "
        "prevVehicle=%u trackScope=%u triangles=%u\n",
        reason,
        static_cast<unsigned long long>(current.header.frame_index),
        static_cast<unsigned long long>(previous.frame_index),
        static_cast<unsigned long long>(frame_delta),
        current.header.input_poll,
        previous.input_poll,
        static_cast<long long>(poll_delta),
        current.header.display_width,
        current.header.display_height,
        previous.display_width,
        previous.display_height,
        options.flags,
        context.previous_options.flags,
        options.output_scale,
        context.previous_options.output_scale,
        options.clear_color_rgba8,
        context.previous_options.clear_color_rgba8,
        static_cast<unsigned long long>(current.header.camera_transform_id),
        static_cast<unsigned long long>(previous.camera_transform_id),
        context.draw_list.commands.size(),
        context.previous_draw_list.commands.size(),
        context.draw_list.track_commands,
        context.previous_draw_list.track_commands,
        context.draw_list.vehicle_commands,
        context.previous_draw_list.vehicle_commands,
        context.draw_list.track_commands != 0 ? 1U : 0U,
        current.header.triangle_count);
}

void cache_current(
    LiveContext* context,
    const BuiltFrame& current,
    const opengt_live_options& options,
    const opengt_live_stats& actual_stats
) {
    context->previous_draw_list = std::move(context->draw_list);
    context->previous_interpolation_cache =
        std::move(context->interpolation_cache);
    context->previous_vram.swap(context->vram);
    context->previous_header = current.header;
    context->previous_actual_stats = actual_stats;
    context->previous_options = options;
    context->has_previous = true;
}

bool defer_temporal_reset_readback(
    const char* reason,
    bool current_has_track
) {
    if (current_has_track)
        return false;
    return
        std::strcmp(reason, "frame_gap") == 0 ||
        std::strcmp(reason, "poll_gap") == 0;
}

bool recover_temporal_gap_by_interpolation(
    const char* reason,
    const LiveContext& context,
    const BuiltFrame& current,
    bool current_has_track,
    const opengt_live_options& options
) {
    if (
        std::strcmp(reason, "frame_gap") != 0 &&
        std::strcmp(reason, "poll_gap") != 0)
        return false;
    if (
        !current_has_track ||
        context.previous_draw_list.track_commands == 0)
        return false;
    const auto& previous = context.previous_header;
    if (
        current.header.display_width != previous.display_width ||
        current.header.display_height != previous.display_height)
        return false;
    return
        options.flags == context.previous_options.flags &&
        options.output_scale == context.previous_options.output_scale &&
        options.clear_color_rgba8 ==
            context.previous_options.clear_color_rgba8;
}

} // namespace

extern "C" {

void* opengt_live_create(void) {
    try {
        opengt::render::reset_world_d3d11_readback(false);
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
    clear_struct(stats);
    if (
        handle == nullptr || capture_bytes == nullptr ||
        output_rgba == nullptr || options == nullptr || stats == nullptr ||
        options->struct_size != sizeof(opengt_live_options) ||
        stats->struct_size != sizeof(opengt_live_stats)
    )
        return fail(stats, 1);
    try {
        auto* context = static_cast<LiveContext*>(handle);
        BuiltFrame built{};
        std::uint32_t result = build_frame(
            context,
            capture_bytes,
            capture_size,
            *options,
            &built);
        if (result != 0)
            return fail(stats, result);
        const bool realtime_readback =
            (options->flags & OPENGT_LIVE_REALTIME_READBACK) != 0;
        if (!realtime_readback) {
            context->has_previous = false;
            result = render_frame(
                built,
                context->draw_list,
                context->vram.data(),
                context->vram.size(),
                output_rgba,
                output_capacity,
                *options,
                stats);
            return result == 0 ? 0 : fail(stats, result);
        }

        const char* reset_reason = temporal_stream_reset_reason(
            *context, built, *options);
        const bool software_adapter =
            (options->flags & OPENGT_LIVE_WARP) != 0;
        if (reset_reason != nullptr) {
            log_temporal_reset(*context, built, *options, reset_reason);
            opengt::render::reset_world_d3d11_readback(software_adapter);
            context->pending_authored_read = 0;
            context->pending_authored_write = 0;
            context->pending_authored_count = 0;
        }
        context->readback_software_adapter = software_adapter;

        opengt_live_stats submitted{};
        submitted.struct_size = sizeof(submitted);
        result = render_frame(
            built,
            context->draw_list,
            context->vram.data(),
            context->vram.size(),
            output_rgba,
            output_capacity,
            *options,
            &submitted,
            false,
            false,
            false,
            false,
            true);
        if (result != 0)
            return fail(stats, result);
        if (
            context->pending_authored_count >=
            context->pending_authored_stats.size()
        )
            return fail(stats, 704U);
        const std::uint32_t write = context->pending_authored_write;
        context->pending_authored_stats[write] = submitted;
        context->pending_authored_write =
            (write + 1U) % context->pending_authored_stats.size();
        ++context->pending_authored_count;

        const bool readback_primed =
            context->pending_authored_count >=
            opengt::render::world_gpu_readback_pair_delay * 2U;
        const int32_t drained = try_read_pending_authored(
            context,
            output_rgba,
            output_capacity,
            stats,
            readback_primed);
        if (drained < 0)
            return fail(stats, static_cast<std::uint32_t>(-drained));
        if (drained == 0) {
            *stats = submitted;
            stats->reserved |= OPENGT_LIVE_STATS_NO_OUTPUT;
        }
        cache_current(context, built, *options, submitted);
        return 0;
    } catch (const std::bad_alloc&) {
        return fail(stats, 2);
    } catch (...) {
        return fail(stats, 3);
    }
}

int32_t opengt_live_render_pair(
    void* handle,
    const std::uint8_t* capture_bytes,
    std::size_t capture_size,
    std::uint8_t* output_first_rgba,
    std::uint8_t* output_second_rgba,
    std::size_t output_capacity,
    const opengt_live_options* options,
    opengt_live_stats* first_stats,
    opengt_live_stats* second_stats,
    opengt_live_interpolation_stats* interpolation_stats
) {
    clear_struct(first_stats);
    clear_struct(second_stats);
    clear_struct(interpolation_stats);
    if (
        handle == nullptr || capture_bytes == nullptr ||
        output_first_rgba == nullptr || output_second_rgba == nullptr ||
        options == nullptr || first_stats == nullptr ||
        second_stats == nullptr || interpolation_stats == nullptr ||
        options->struct_size != sizeof(opengt_live_options) ||
        first_stats->struct_size != sizeof(opengt_live_stats) ||
        second_stats->struct_size != sizeof(opengt_live_stats) ||
        interpolation_stats->struct_size !=
            sizeof(opengt_live_interpolation_stats)
    )
        return fail_pair(
            first_stats, second_stats, interpolation_stats, 1);
    try {
        auto* context = static_cast<LiveContext*>(handle);
        const auto pair_started = Clock::now();
        BuiltFrame current{};
        std::uint32_t result = build_frame(
            context,
            capture_bytes,
            capture_size,
            *options,
            &current,
            false,
            false);
        if (result != 0)
            return fail_pair(
                first_stats, second_stats, interpolation_stats, result);
        // Pair rendering intentionally builds topology on the interpolated
        // midpoint rather than this raw authored list. Use draw-list
        // provenance for track scope/reset decisions; current.topology is
        // therefore zero here by construction.
        const bool current_has_track =
            context->draw_list.track_commands != 0;

        const char* reset_reason = temporal_stream_reset_reason(
            *context, current, *options);
        if (
            reset_reason != nullptr &&
            recover_temporal_gap_by_interpolation(
                reset_reason,
                *context,
                current,
                current_has_track,
                *options))
            reset_reason = nullptr;
        if (reset_reason != nullptr) {
            log_temporal_reset(*context, current, *options, reset_reason);
            interpolation_stats->temporal_reset = 1;
            opengt::render::reset_world_d3d11_readback(
                (options->flags & OPENGT_LIVE_WARP) != 0);
            context->readback_software_adapter =
                (options->flags & OPENGT_LIVE_WARP) != 0;
            context->pending_output_pair_read = 0;
            context->pending_output_pair_write = 0;
            context->pending_output_pair_count = 0;
            if (defer_temporal_reset_readback(
                    reset_reason, current_has_track)) {
                opengt_live_stats actual{};
                actual.struct_size = sizeof(actual);
                fill_deferred_reset_stats(
                    current,
                    context->draw_list,
                    *options,
                    pair_started,
                    &actual);
                interpolation_stats->current_topology_microseconds =
                    current.topology_microseconds;
                if (current_has_track)
                    interpolation_stats->reserved |= 2U;
                interpolation_stats->pair_pipeline_microseconds =
                    microseconds(Clock::now() - pair_started);
                *first_stats = actual;
                interpolation_stats->output_count = 0;
                cache_current(context, current, *options, actual);
                return 0;
            }
            opengt_live_stats actual{};
            actual.struct_size = sizeof(actual);
            result = render_frame(
                current,
                context->draw_list,
                context->vram.data(),
                context->vram.size(),
                output_first_rgba,
                output_capacity,
                *options,
                &actual);
            if (result != 0)
                return fail_pair(
                    first_stats,
                    second_stats,
                    interpolation_stats,
                    result);
            actual.reserved |= 4U;
            interpolation_stats->actual_render_microseconds =
                actual.render_microseconds;
            interpolation_stats->current_topology_microseconds =
                current.topology_microseconds;
            if (current_has_track)
                interpolation_stats->reserved |= 2U;
            interpolation_stats->pair_pipeline_microseconds =
                microseconds(Clock::now() - pair_started);
            // A temporal discontinuity cannot be interpolated, but the D3D11
            // reset path reads this authored image synchronously. Publish it
            // immediately instead of creating a blank modern-world interval.
            *first_stats = actual;
            interpolation_stats->output_count = 1;
            cache_current(context, current, *options, actual);
            return 0;
        }

        const auto interpolation_started = Clock::now();
        opengt::render::WorldDrawList midpoint{};
        opengt::render::WorldInterpolationStats interpolation{};
        const auto interpolation_result =
            opengt::render::interpolate_world_draw_lists_cached(
                context->previous_draw_list,
                context->draw_list,
                &context->previous_interpolation_cache,
                &context->interpolation_cache,
                0.5F,
                &midpoint,
                &interpolation);
        const auto interpolation_finished = Clock::now();
        if (
            interpolation_result !=
            opengt::render::WorldInterpolationResult::success
        )
            return fail_pair(
                first_stats,
                second_stats,
                interpolation_stats,
                500U + static_cast<std::uint32_t>(interpolation_result));

        BuiltFrame midpoint_built{};
        midpoint_built.header = context->previous_header;
        midpoint_built.header.frame_index =
            context->previous_header.frame_index + 1;
        midpoint_built.header.input_poll =
            context->previous_header.input_poll + 1;
        midpoint_built.pipeline_started = interpolation_started;
        if ((options->flags & OPENGT_LIVE_TOPOLOGY) != 0) {
            const auto midpoint_topology_started = Clock::now();
            const auto topology_result = opengt::render::apply_world_topology(
                &midpoint,
                opengt::render::WorldTopologyOptions{
                    true,
                    true,
                    true,
                    true,
                    false},
                &midpoint_built.topology);
            const auto midpoint_topology_finished = Clock::now();
            midpoint_built.topology_microseconds = microseconds(
                midpoint_topology_finished - midpoint_topology_started);
            if (topology_result !=
                opengt::render::WorldTopologyResult::success) {
                return fail_pair(
                    first_stats,
                    second_stats,
                    interpolation_stats,
                    300U + static_cast<std::uint32_t>(topology_result));
            }
        }
        opengt_live_stats midpoint_stats{};
        midpoint_stats.struct_size = sizeof(midpoint_stats);
        const bool software_adapter =
            (options->flags & OPENGT_LIVE_WARP) != 0;
        context->readback_software_adapter = software_adapter;
        const std::size_t readback_limit =
            (options->flags & OPENGT_LIVE_REALTIME_READBACK) != 0
                ? context->pending_midpoint_stats.size()
                : opengt::render::world_gpu_readback_pair_delay;
        const bool readback_full =
            context->pending_output_pair_count >= readback_limit;
        const int32_t drained = try_read_pending_pair(
            context,
            output_first_rgba,
            output_second_rgba,
            output_capacity,
            first_stats,
            second_stats,
            readback_full);
        if (drained < 0)
            return fail_pair(
                first_stats,
                second_stats,
                interpolation_stats,
                static_cast<std::uint32_t>(-drained));
        interpolation_stats->output_count = drained == 1 ? 2U : 0U;
        const bool reuse_midpoint_uploads =
            midpoint_uploads_match_previous(
                context->previous_draw_list,
                midpoint);
        result = render_frame(
            midpoint_built,
            midpoint,
            context->previous_vram.data(),
            context->previous_vram.size(),
            output_first_rgba,
            output_capacity,
            *options,
            &midpoint_stats,
            reuse_midpoint_uploads,
            reuse_midpoint_uploads,
            true,
            false,
            true);
        if (result != 0)
            return fail_pair(
                first_stats, second_stats, interpolation_stats, result);

        opengt_live_stats actual{};
        actual.struct_size = sizeof(actual);
        result = render_frame(
            current,
            context->draw_list,
            context->vram.data(),
            context->vram.size(),
            output_second_rgba,
            output_capacity,
            *options,
            &actual,
            false,
            false,
            false,
            false,
            true);
        if (result != 0)
            return fail_pair(
                first_stats, second_stats, interpolation_stats, result);

        midpoint_stats.reserved |= 1U;
        const std::uint64_t pair_pipeline_microseconds =
            microseconds(Clock::now() - pair_started);
        const std::uint32_t write = context->pending_output_pair_write;
        context->pending_midpoint_stats[write] = midpoint_stats;
        context->pending_actual_stats[write] = actual;
        context->pending_output_pair_write =
            (write + 1U) % context->pending_midpoint_stats.size();
        ++context->pending_output_pair_count;
        interpolation_stats->result = 0;
        interpolation_stats->previous_commands =
            interpolation.previous_commands;
        interpolation_stats->current_commands =
            interpolation.current_commands;
        interpolation_stats->eligible_world_commands =
            interpolation.eligible_world_commands;
        interpolation_stats->matched_commands =
            interpolation.matched_commands;
        interpolation_stats->matched_track_commands =
            interpolation.matched_track_commands;
        interpolation_stats->matched_vehicle_commands =
            interpolation.matched_vehicle_commands;
        interpolation_stats->held_screen_commands =
            interpolation.held_screen_commands;
        interpolation_stats->held_unmatched_commands =
            interpolation.held_unmatched_commands;
        interpolation_stats->previous_transform_groups =
            interpolation.previous_transform_groups;
        interpolation_stats->current_transform_groups =
            interpolation.current_transform_groups;
        interpolation_stats->matched_transform_groups =
            interpolation.matched_transform_groups;
        if (reuse_midpoint_uploads)
            interpolation_stats->reserved |= 1U;
        if (current_has_track)
            interpolation_stats->reserved |= 2U;
        interpolation_stats->exact_rigid_transform_groups =
            interpolation.exact_rigid_transform_groups;
        interpolation_stats->incoherent_exact_transform_groups =
            interpolation.incoherent_exact_transform_groups;
        interpolation_stats->held_incoherent_vehicle_commands =
            interpolation.held_incoherent_vehicle_commands;
        interpolation_stats->held_track_visibility_commands =
            interpolation.held_track_visibility_commands;
        interpolation_stats->held_unsafe_track_commands =
            interpolation.held_unsafe_track_commands;
        interpolation_stats->interpolation_microseconds =
            microseconds(interpolation_finished - interpolation_started);
        interpolation_stats->midpoint_render_microseconds =
            midpoint_stats.render_microseconds;
        interpolation_stats->actual_render_microseconds =
            actual.render_microseconds;
        interpolation_stats->current_topology_microseconds =
            midpoint_built.topology_microseconds;
        interpolation_stats->pair_pipeline_microseconds =
            pair_pipeline_microseconds;
        cache_current(context, current, *options, actual);
        return 0;
    } catch (const std::bad_alloc&) {
        return fail_pair(
            first_stats, second_stats, interpolation_stats, 2);
    } catch (...) {
        return fail_pair(
            first_stats, second_stats, interpolation_stats, 3);
    }
}

int32_t opengt_live_try_read_pair(
    void* handle,
    std::uint8_t* output_first_rgba,
    std::uint8_t* output_second_rgba,
    std::size_t output_capacity,
    opengt_live_stats* first_stats,
    opengt_live_stats* second_stats
) {
    clear_struct(first_stats);
    clear_struct(second_stats);
    if (
        handle == nullptr || output_first_rgba == nullptr ||
        output_second_rgba == nullptr || first_stats == nullptr ||
        second_stats == nullptr ||
        first_stats->struct_size != sizeof(opengt_live_stats) ||
        second_stats->struct_size != sizeof(opengt_live_stats)
    )
        return -1;
    try {
        const int32_t result = try_read_pending_pair(
            static_cast<LiveContext*>(handle),
            output_first_rgba,
            output_second_rgba,
            output_capacity,
            first_stats,
            second_stats,
            false);
        if (result < 0) {
            first_stats->result = static_cast<std::uint32_t>(-result);
            second_stats->result = first_stats->result;
        }
        return result;
    } catch (const std::bad_alloc&) {
        first_stats->result = 2;
        second_stats->result = 2;
        return -2;
    } catch (...) {
        first_stats->result = 3;
        second_stats->result = 3;
        return -3;
    }
}

int32_t opengt_live_set_texture_uploads(
    void* handle,
    int32_t software_adapter,
    const opengt_live_texture_upload* uploads,
    size_t upload_count
) {
    if (
        handle == nullptr ||
        (upload_count != 0 && uploads == nullptr) ||
        upload_count > 65536
    )
        return -1;
    static_assert(
        sizeof(opengt_live_texture_upload) ==
        sizeof(opengt::render::WorldTextureUpload));
    return opengt::render::set_world_d3d11_texture_uploads(
        software_adapter != 0,
        reinterpret_cast<const opengt::render::WorldTextureUpload*>(uploads),
        upload_count) ? 0 : -2;
}

std::uint32_t opengt_live_api_version(void) {
    return api_version;
}

} // extern "C"
