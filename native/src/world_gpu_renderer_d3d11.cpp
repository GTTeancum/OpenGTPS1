#include "opengt/world_gpu_renderer.hpp"

#if defined(_WIN32)

#include <d3d11.h>
#include <d3dcompiler.h>
#include <wrl/client.h>

#include <algorithm>
#include <array>
#include <chrono>
#include <cmath>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <filesystem>
#include <fstream>
#include <iterator>
#include <limits>
#include <optional>
#include <regex>
#include <string>
#include <thread>
#include <unordered_map>
#include <unordered_set>
#include <utility>
#include <vector>

namespace opengt::render {
namespace {

using Microsoft::WRL::ComPtr;

constexpr std::uint32_t textured_flag = 1U << 0;
constexpr std::uint32_t semi_transparent_flag = 1U << 1;
constexpr std::uint32_t set_mask_flag = 1U << 0;
constexpr std::uint32_t check_mask_flag = 1U << 1;

bool soft_vehicle_shadow(
    const WorldDrawCommand& command,
    const WorldMaterial& material
) noexcept {
    if (
        command.object_kind != 2U ||
        (material.primitive_flags & textured_flag) != 0 ||
        (material.primitive_flags & semi_transparent_flag) == 0 ||
        ((material.texture_page >> 5) & 3U) != 2U
    )
        return false;
    // This is GT2's dedicated untextured reverse-subtract vehicle-shadow
    // contract. Clipped/interpolated vertices can lack model provenance and
    // retain unrelated view-space coordinates, so geometry is not a safe
    // secondary discriminator for this otherwise unique signature.
    return true;
}

bool vehicle_wheel_tread(
    const WorldDrawCommand& command,
    const WorldMaterial& material
) noexcept {
    if (
        command.object_kind != 2U ||
        (material.primitive_flags &
            (textured_flag | semi_transparent_flag)) != 0
    )
        return false;

    // GT2 wheel meshes are centred cylinders in model space. Their tread
    // triangles bridge the two narrow axle planes while every vertex stays
    // on the same much-larger radial ring. Detect that shape rather than a
    // particular car, model pointer, or capture-local material index.
    std::int32_t minimum_z = command.vertices[0].model_z;
    std::int32_t maximum_z = minimum_z;
    std::int64_t minimum_radius_squared =
        (std::numeric_limits<std::int64_t>::max)();
    std::int64_t maximum_radius_squared = 0;
    std::int32_t maximum_abs_z = 0;
    for (const auto& vertex : command.vertices) {
        minimum_z = (std::min)(
            minimum_z, static_cast<std::int32_t>(vertex.model_z));
        maximum_z = (std::max)(
            maximum_z, static_cast<std::int32_t>(vertex.model_z));
        maximum_abs_z = (std::max)(
            maximum_abs_z,
            std::abs(static_cast<std::int32_t>(vertex.model_z)));
        const std::int64_t x = vertex.model_x;
        const std::int64_t y = vertex.model_y;
        const std::int64_t radius_squared = x * x + y * y;
        minimum_radius_squared = (std::min)(
            minimum_radius_squared, radius_squared);
        maximum_radius_squared = (std::max)(
            maximum_radius_squared, radius_squared);
    }
    if (
        minimum_z >= 0 || maximum_z <= 0 ||
        maximum_abs_z == 0 || minimum_radius_squared < 256 * 256
    )
        return false;
    const std::int64_t maximum_abs_z_squared =
        static_cast<std::int64_t>(maximum_abs_z) * maximum_abs_z;
    return
        minimum_radius_squared > maximum_abs_z_squared * 4 &&
        maximum_radius_squared * 4 <= minimum_radius_squared * 5;
}

struct SmoothWheel {
    std::uint32_t object_id;
    std::uint32_t model_pointer;
    std::uint64_t transform_id;
    WorldViewChannel channel;
    std::size_t insertion_command;
    // Drawing-area union of the authored commands this shell replaces. The
    // shell is a substitute for that geometry, so it must inherit the guest
    // clip rectangle: rear-view mirror passes submit vehicles through a
    // narrow drawing area and rely on it to cut the oversized near-camera
    // geometry away.
    std::int32_t clip_x0;
    std::int32_t clip_y0;
    std::int32_t clip_x1;
    std::int32_t clip_y1;
    double model_to_view[3][3];
    double model_centroid[3];
    double view_centroid[3];
    double radius;
    double half_width;
    double maximum_fit_error;
    // Projected extent of the generated shell in PS1 display coordinates,
    // filled while the shell vertices are written. Compared against the
    // clip rectangle by the wheel diagnostics.
    double shell_minimum_x;
    double shell_minimum_y;
    double shell_maximum_x;
    double shell_maximum_y;
    bool exact_transform;
};

struct VehicleDiagnosticGroup {
    std::uint32_t object_id{};
    std::uint32_t model_pointer{};
    std::uint64_t transform_id{};
    WorldViewChannel channel{};
    std::size_t commands{};
    std::size_t vertices_in_display{};
    std::size_t vertices_in_front{};
    std::size_t first_command{(std::numeric_limits<std::size_t>::max)()};
    std::size_t last_command{};
    float minimum_screen_x{(std::numeric_limits<float>::max)()};
    float minimum_screen_y{(std::numeric_limits<float>::max)()};
    float maximum_screen_x{(std::numeric_limits<float>::lowest)()};
    float maximum_screen_y{(std::numeric_limits<float>::lowest)()};
    float minimum_view_z{(std::numeric_limits<float>::max)()};
    float maximum_view_z{(std::numeric_limits<float>::lowest)()};
    float minimum_clip_w{(std::numeric_limits<float>::max)()};
    float maximum_clip_w{(std::numeric_limits<float>::lowest)()};
    std::int32_t minimum_clip_x0{(std::numeric_limits<std::int32_t>::max)()};
    std::int32_t minimum_clip_y0{(std::numeric_limits<std::int32_t>::max)()};
    std::int32_t maximum_clip_x1{(std::numeric_limits<std::int32_t>::min)()};
    std::int32_t maximum_clip_y1{(std::numeric_limits<std::int32_t>::min)()};
    std::size_t commands_with_visible_scissor{};
    bool exact_transform_valid{};
};

bool same_vehicle_diagnostic_group(
    const VehicleDiagnosticGroup& group,
    const WorldDrawCommand& command
) noexcept {
    return
        command.object_kind == 2U &&
        group.object_id == command.object_id &&
        group.model_pointer == command.model_pointer &&
        group.transform_id == command.transform_id &&
        group.channel == command.channel;
}

void emit_vehicle_diagnostics(
    const WorldDrawList& draw_list,
    const std::vector<SmoothWheel>& smooth_wheels,
    bool synthetic_midpoint
) {
    if (std::getenv("OPENGT_RENDER_VEHICLE_DIAGNOSTICS") == nullptr)
        return;
    if (
        std::getenv("OPENGT_RENDER_VEHICLE_DIAGNOSTICS_RACE_ONLY") !=
            nullptr &&
        draw_list.vehicle_commands < 1000U
    )
        return;
    static thread_local std::uint64_t diagnostic_sample = 0;
    std::uint64_t interval = 120;
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_VEHICLE_DIAGNOSTICS_INTERVAL")) {
        const unsigned long parsed = std::strtoul(configured, nullptr, 10);
        if (parsed != 0)
            interval = parsed;
    }
    if ((diagnostic_sample++ % interval) != 0)
        return;

    std::vector<VehicleDiagnosticGroup> groups;
    groups.reserve(32);
    const float display_x0 = static_cast<float>(draw_list.display_x);
    const float display_y0 = static_cast<float>(draw_list.display_y);
    const float display_x1 = display_x0 + draw_list.display_width;
    const float display_y1 = display_y0 + draw_list.display_height;
    for (std::size_t command_index = 0;
         command_index < draw_list.commands.size();
         ++command_index) {
        const auto& command = draw_list.commands[command_index];
        if (command.object_kind != 2U)
            continue;
        auto found = std::find_if(
            groups.begin(), groups.end(),
            [&] (const VehicleDiagnosticGroup& group) {
                return same_vehicle_diagnostic_group(group, command);
            });
        if (found == groups.end()) {
            groups.push_back(VehicleDiagnosticGroup{});
            found = groups.end() - 1;
            found->object_id = command.object_id;
            found->model_pointer = command.model_pointer;
            found->transform_id = command.transform_id;
            found->channel = command.channel;
            found->exact_transform_valid = command.exact_transform_valid;
        } else {
            found->exact_transform_valid =
                found->exact_transform_valid &&
                command.exact_transform_valid;
        }
        ++found->commands;
        found->minimum_clip_x0 = (std::min)(
            found->minimum_clip_x0,
            static_cast<std::int32_t>(command.clip_x0));
        found->minimum_clip_y0 = (std::min)(
            found->minimum_clip_y0,
            static_cast<std::int32_t>(command.clip_y0));
        found->maximum_clip_x1 = (std::max)(
            found->maximum_clip_x1,
            static_cast<std::int32_t>(command.clip_x1));
        found->maximum_clip_y1 = (std::max)(
            found->maximum_clip_y1,
            static_cast<std::int32_t>(command.clip_y1));
        if (
            command.clip_x1 >= draw_list.display_x &&
            command.clip_y1 >= draw_list.display_y &&
            command.clip_x0 < draw_list.display_x + draw_list.display_width &&
            command.clip_y0 < draw_list.display_y + draw_list.display_height
        )
            ++found->commands_with_visible_scissor;
        found->first_command = (std::min)(
            found->first_command, command_index);
        found->last_command = (std::max)(
            found->last_command, command_index);
        for (const auto& vertex : command.vertices) {
            found->minimum_screen_x = (std::min)(
                found->minimum_screen_x, vertex.screen_x);
            found->minimum_screen_y = (std::min)(
                found->minimum_screen_y, vertex.screen_y);
            found->maximum_screen_x = (std::max)(
                found->maximum_screen_x, vertex.screen_x);
            found->maximum_screen_y = (std::max)(
                found->maximum_screen_y, vertex.screen_y);
            found->minimum_view_z = (std::min)(
                found->minimum_view_z, vertex.view_z);
            found->maximum_view_z = (std::max)(
                found->maximum_view_z, vertex.view_z);
            found->minimum_clip_w = (std::min)(
                found->minimum_clip_w, vertex.clip_w);
            found->maximum_clip_w = (std::max)(
                found->maximum_clip_w, vertex.clip_w);
            if (
                vertex.screen_x >= display_x0 &&
                vertex.screen_x < display_x1 &&
                vertex.screen_y >= display_y0 &&
                vertex.screen_y < display_y1
            )
                ++found->vertices_in_display;
            if (vertex.view_z >= 16.0F)
                ++found->vertices_in_front;
        }
    }

    std::fprintf(
        stderr,
        "[Render-Vehicle-Groups] sample=%s groups=%zu commands=%u\n",
        synthetic_midpoint ? "midpoint" : "actual",
        groups.size(),
        draw_list.vehicle_commands);
    for (const auto& group : groups) {
        const bool wheel = std::any_of(
            smooth_wheels.begin(), smooth_wheels.end(),
            [&] (const SmoothWheel& candidate) {
                return
                    candidate.object_id == group.object_id &&
                    candidate.model_pointer == group.model_pointer &&
                    candidate.transform_id == group.transform_id &&
                    candidate.channel == group.channel;
            });
        std::fprintf(
            stderr,
            "[Render-Vehicle-Group] sample=%s object=%u model=%08x "
            "transform=%016llx channel=%u role=%s commands=%zu "
            "range=%zu-%zu exact=%u visible=%zu/%zu front=%zu/%zu "
            "screen=%.1f,%.1f..%.1f,%.1f viewZ=%.1f..%.1f "
            "clipW=%.1f..%.1f scissor=%d,%d..%d,%d visibleScissor=%zu/%zu "
            "display=%d,%d..%d,%d\n",
            synthetic_midpoint ? "midpoint" : "actual",
            group.object_id,
            group.model_pointer,
            static_cast<unsigned long long>(group.transform_id),
            static_cast<unsigned>(group.channel),
            wheel ? "wheel" : "body/part",
            group.commands,
            group.first_command,
            group.last_command,
            group.exact_transform_valid ? 1U : 0U,
            group.vertices_in_display,
            group.commands * 3,
            group.vertices_in_front,
            group.commands * 3,
            group.minimum_screen_x,
            group.minimum_screen_y,
            group.maximum_screen_x,
            group.maximum_screen_y,
            group.minimum_view_z,
            group.maximum_view_z,
            group.minimum_clip_w,
            group.maximum_clip_w,
            group.minimum_clip_x0,
            group.minimum_clip_y0,
            group.maximum_clip_x1,
            group.maximum_clip_y1,
            group.commands_with_visible_scissor,
            group.commands,
            draw_list.display_x,
            draw_list.display_y,
            draw_list.display_x + draw_list.display_width,
            draw_list.display_y + draw_list.display_height);
    }
}

// D3D scissors every batch to its command's guest drawing area, so geometry
// cannot physically paint outside the rectangle it carries. When content still
// appears beside the rear-view mirror, the question is which rectangles the
// frame actually contains: a mirror-pass command carrying the full display
// area would escape legitimately as far as the scissor is concerned. Report
// the distinct rectangles per channel so that is a measurement, not a guess.
void emit_clip_rect_diagnostics(const WorldDrawList& draw_list) {
    if (std::getenv("OPENGT_RENDER_CLIP_DIAGNOSTICS") == nullptr)
        return;
    static thread_local std::uint64_t clip_sample = 0;
    std::uint64_t interval = 120;
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_CLIP_DIAGNOSTICS_INTERVAL")) {
        const unsigned long parsed = std::strtoul(configured, nullptr, 10);
        if (parsed != 0)
            interval = parsed;
    }
    if ((clip_sample++ % interval) != 0)
        return;
    struct ClipGroup {
        WorldViewChannel channel{};
        std::int32_t x0{};
        std::int32_t y0{};
        std::int32_t x1{};
        std::int32_t y1{};
        std::size_t commands{};
        std::array<std::size_t, 3> kinds{};
    };
    std::vector<ClipGroup> groups;
    groups.reserve(16);
    for (const auto& command : draw_list.commands) {
        auto found = std::find_if(
            groups.begin(), groups.end(),
            [&] (const ClipGroup& group) {
                return
                    group.channel == command.channel &&
                    group.x0 == command.clip_x0 &&
                    group.y0 == command.clip_y0 &&
                    group.x1 == command.clip_x1 &&
                    group.y1 == command.clip_y1;
            });
        if (found == groups.end()) {
            groups.push_back(ClipGroup{
                command.channel,
                command.clip_x0,
                command.clip_y0,
                command.clip_x1,
                command.clip_y1,
                0,
                {}});
            found = groups.end() - 1;
        }
        ++found->commands;
        ++found->kinds[command.object_kind <= 2U ? command.object_kind : 0U];
    }
    std::fprintf(
        stderr,
        "[Render-Clip-Rects] rects=%zu commands=%zu rejectedIncomplete=%u "
        "rejectedOversized=%u incTrack=%u incVehicle=%u screenTargetDrop=%u/%u secondary=%u display=%d,%d..%d,%d\n",
        groups.size(),
        draw_list.commands.size(),
        draw_list.rejected_incomplete,
        draw_list.rejected_oversized_screen_commands,
        draw_list.rejected_incomplete_track,
        draw_list.rejected_incomplete_vehicle,
        draw_list.rejected_screen_target_track,
        draw_list.rejected_screen_target,
        draw_list.secondary_commands,
        draw_list.display_x,
        draw_list.display_y,
        draw_list.display_x + draw_list.display_width,
        draw_list.display_y + draw_list.display_height);
    for (const auto& group : groups) {
        std::fprintf(
            stderr,
            "[Render-Clip-Rect] channel=%u clip=%d,%d..%d,%d commands=%zu "
            "screen=%zu track=%zu vehicle=%zu\n",
            static_cast<unsigned>(group.channel),
            group.x0,
            group.y0,
            group.x1,
            group.y1,
            group.commands,
            group.kinds[0],
            group.kinds[1],
            group.kinds[2]);
    }
}

// Projection failures are often intermittent because a single vertex crosses
// a clip plane or a large authored triangle changes its raster footprint by a
// fraction of a pixel. Keep this opt-in: at interval 1 it deliberately emits
// enough provenance to correlate a visible frame with the exact command.
void emit_primitive_diagnostics(
    const WorldDrawList& draw_list,
    const WorldGpuRenderOptions& options
) {
    if (std::getenv("OPENGT_RENDER_PRIMITIVE_DIAGNOSTICS") == nullptr)
        return;
    static thread_local std::uint64_t primitive_sample = 0;
    std::uint64_t interval = 120;
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_PRIMITIVE_DIAGNOSTICS_INTERVAL")) {
        const unsigned long parsed = std::strtoul(configured, nullptr, 10);
        if (parsed != 0)
            interval = parsed;
    }
    if ((primitive_sample++ % interval) != 0)
        return;

    constexpr float d3d_near_plane = 16.0F;
    std::size_t world_commands = 0;
    std::size_t screen_commands = 0;
    std::size_t camera_crossings = 0;
    std::size_t near_crossings = 0;
    std::size_t behind_near = 0;
    std::size_t viewport_crossings = 0;
    std::size_t oversized_spans = 0;
    std::size_t nonfinite_commands = 0;
    std::size_t depth_track_commands = 0;
    float maximum_projection_delta = 0.0F;
    float minimum_view_z = (std::numeric_limits<float>::max)();
    float maximum_view_z = (std::numeric_limits<float>::lowest)();
    std::vector<std::size_t> suspicious;
    suspicious.reserve(32);

    const float display_x0 = static_cast<float>(draw_list.display_x);
    const float display_y0 = static_cast<float>(draw_list.display_y);
    const float display_x1 = display_x0 + draw_list.display_width;
    const float display_y1 = display_y0 + draw_list.display_height;
    for (std::size_t index = 0; index < draw_list.commands.size(); ++index) {
        const auto& command = draw_list.commands[index];
        const auto& material = draw_list.materials[command.material_index];
        if ((material.primitive_flags & world_primitive_screen_space_flag) != 0) {
            ++screen_commands;
            continue;
        }
        ++world_commands;
        if (options.depth_buffer && command.object_kind == 1U)
            ++depth_track_commands;
        float min_x = command.vertices[0].screen_x;
        float max_x = min_x;
        float min_y = command.vertices[0].screen_y;
        float max_y = min_y;
        float min_z = command.vertices[0].view_z;
        float max_z = min_z;
        bool finite = true;
        for (const auto& vertex : command.vertices) {
            min_x = (std::min)(min_x, vertex.screen_x);
            max_x = (std::max)(max_x, vertex.screen_x);
            min_y = (std::min)(min_y, vertex.screen_y);
            max_y = (std::max)(max_y, vertex.screen_y);
            min_z = (std::min)(min_z, vertex.view_z);
            max_z = (std::max)(max_z, vertex.view_z);
            finite = finite && std::isfinite(vertex.screen_x) &&
                std::isfinite(vertex.screen_y) &&
                std::isfinite(vertex.clip_x) &&
                std::isfinite(vertex.clip_y) &&
                std::isfinite(vertex.clip_z) &&
                std::isfinite(vertex.clip_w);
            maximum_projection_delta = (std::max)(
                maximum_projection_delta,
                (std::max)(
                    std::abs(vertex.screen_x - vertex.authored_screen_x),
                    std::abs(vertex.screen_y - vertex.authored_screen_y)));
        }
        minimum_view_z = (std::min)(minimum_view_z, min_z);
        maximum_view_z = (std::max)(maximum_view_z, max_z);
        const bool crosses_camera = min_z <= 0.0F && max_z > 0.0F;
        const bool crosses_near =
            min_z < d3d_near_plane && max_z >= d3d_near_plane;
        const bool entirely_behind_near = max_z < d3d_near_plane;
        const bool crosses_viewport =
            (min_x < display_x0 && max_x >= display_x0) ||
            (min_x < display_x1 && max_x >= display_x1) ||
            (min_y < display_y0 && max_y >= display_y0) ||
            (min_y < display_y1 && max_y >= display_y1);
        const bool oversized =
            max_x - min_x > draw_list.display_width * 4.0F ||
            max_y - min_y > draw_list.display_height * 4.0F;
        camera_crossings += crosses_camera ? 1U : 0U;
        near_crossings += crosses_near ? 1U : 0U;
        behind_near += entirely_behind_near ? 1U : 0U;
        viewport_crossings += crosses_viewport ? 1U : 0U;
        oversized_spans += oversized ? 1U : 0U;
        nonfinite_commands += finite ? 0U : 1U;
        if (
            suspicious.size() < 32 &&
            (crosses_camera || crosses_near || entirely_behind_near ||
                oversized || !finite)
        )
            suspicious.push_back(index);
    }

    if (world_commands == 0) {
        minimum_view_z = 0.0F;
        maximum_view_z = 0.0F;
    }
    std::fprintf(
        stderr,
        "[Render-Primitives] frameSample=%llu authored=%s "
        "projection=%s textureProjection=%s depth=%s topologyCommands=%zu "
        "world=%zu screen=%zu depthTrack=%zu viewZ=%.3f..%.3f "
        "cameraCross=%zu nearCross=%zu behindNear=%zu viewportCross=%zu "
        "oversized=%zu nonfinite=%zu maxAuthoredDelta=%.3f "
        "display=%d,%d..%d,%d\n",
        static_cast<unsigned long long>(primitive_sample - 1),
        options.synthetic_midpoint ? "midpoint" : "actual",
        draw_list.continuous_projection ? "continuous" : "authored-sxy",
        options.perspective_correct ? "perspective" : "affine",
        options.depth_buffer ? "on" : "off",
        draw_list.commands.size(),
        world_commands,
        screen_commands,
        depth_track_commands,
        minimum_view_z,
        maximum_view_z,
        camera_crossings,
        near_crossings,
        behind_near,
        viewport_crossings,
        oversized_spans,
        nonfinite_commands,
        maximum_projection_delta,
        draw_list.display_x,
        draw_list.display_y,
        draw_list.display_x + draw_list.display_width,
        draw_list.display_y + draw_list.display_height);

    if (std::getenv("OPENGT_RENDER_PRIMITIVE_DIAGNOSTICS_VERBOSE") == nullptr)
        return;
    for (const std::size_t index : suspicious) {
        const auto& command = draw_list.commands[index];
        std::fprintf(
            stderr,
            "[Render-Primitive] cmd=%zu kind=%u object=%u model=%08x "
            "transform=%016llx ot=%d source=%u clip=%d,%d..%d,%d "
            "screen=(%.3f,%.3f)(%.3f,%.3f)(%.3f,%.3f) "
            "authored=(%d,%d)(%d,%d)(%d,%d) "
            "viewZ=(%.3f,%.3f,%.3f) clipW=(%.3f,%.3f,%.3f)\n",
            index,
            command.object_kind,
            command.object_id,
            command.model_pointer,
            static_cast<unsigned long long>(command.transform_id),
            command.ordering_table_index,
            command.source_command_index,
            command.clip_x0,
            command.clip_y0,
            command.clip_x1,
            command.clip_y1,
            command.vertices[0].screen_x,
            command.vertices[0].screen_y,
            command.vertices[1].screen_x,
            command.vertices[1].screen_y,
            command.vertices[2].screen_x,
            command.vertices[2].screen_y,
            command.vertices[0].authored_screen_x,
            command.vertices[0].authored_screen_y,
            command.vertices[1].authored_screen_x,
            command.vertices[1].authored_screen_y,
            command.vertices[2].authored_screen_x,
            command.vertices[2].authored_screen_y,
            command.vertices[0].view_z,
            command.vertices[1].view_z,
            command.vertices[2].view_z,
            command.vertices[0].clip_w,
            command.vertices[1].clip_w,
            command.vertices[2].clip_w);
    }
}

bool same_wheel_group(
    const SmoothWheel& wheel,
    const WorldDrawCommand& command
) noexcept {
    return
        command.object_kind == 2U &&
        wheel.object_id == command.object_id &&
        wheel.model_pointer == command.model_pointer &&
        wheel.transform_id == command.transform_id &&
        wheel.channel == command.channel;
}

bool vehicle_wheel_sidewall_ring(
    const WorldDrawCommand& command,
    const WorldMaterial& material,
    const SmoothWheel& wheel
) noexcept {
    if (
        !same_wheel_group(wheel, command) ||
        (material.primitive_flags &
            (textured_flag | semi_transparent_flag)) != 0
    )
        return false;
    const std::int16_t model_z = command.vertices[0].model_z;
    double minimum_radius_squared =
        (std::numeric_limits<double>::max)();
    double maximum_radius_squared = 0.0;
    for (const auto& vertex : command.vertices) {
        if (vertex.model_z != model_z)
            return false;
        const double x = vertex.model_x;
        const double y = vertex.model_y;
        const double radius_squared = x * x + y * y;
        minimum_radius_squared = (std::min)(
            minimum_radius_squared, radius_squared);
        maximum_radius_squared = (std::max)(
            maximum_radius_squared, radius_squared);
    }
    const double tire_radius_squared = wheel.radius * wheel.radius;
    // The outer sidewall annulus sits between roughly 0.68R and R.  Its
    // twelve-sided outline rolls with the wheel and exposes alternating
    // grey/black wedges at the contact patch.  Inner rim/spoke geometry is
    // deliberately excluded and remains authored GT2 content.
    return
        minimum_radius_squared >= tire_radius_squared * 0.36 &&
        maximum_radius_squared >= tire_radius_squared * 0.80;
}

bool solve_three_by_three(
    const double source[3][3],
    const double right[3],
    double output[3]
) noexcept {
    double rows[3][4]{};
    for (int row = 0; row < 3; ++row) {
        for (int column = 0; column < 3; ++column)
            rows[row][column] = source[row][column];
        rows[row][3] = right[row];
    }
    for (int pivot = 0; pivot < 3; ++pivot) {
        int largest = pivot;
        for (int row = pivot + 1; row < 3; ++row) {
            if (std::fabs(rows[row][pivot]) >
                std::fabs(rows[largest][pivot]))
                largest = row;
        }
        if (std::fabs(rows[largest][pivot]) <= 1.0e-9)
            return false;
        if (largest != pivot)
            for (int column = pivot; column < 4; ++column)
                std::swap(rows[pivot][column], rows[largest][column]);
        const double inverse = 1.0 / rows[pivot][pivot];
        for (int column = pivot; column < 4; ++column)
            rows[pivot][column] *= inverse;
        for (int row = 0; row < 3; ++row) {
            if (row == pivot)
                continue;
            const double factor = rows[row][pivot];
            for (int column = pivot; column < 4; ++column)
                rows[row][column] -= factor * rows[pivot][column];
        }
    }
    for (int row = 0; row < 3; ++row)
        output[row] = rows[row][3];
    return true;
}

std::vector<SmoothWheel> build_smooth_wheels(
    const WorldDrawList& draw_list
) {
    std::vector<SmoothWheel> wheels;
    for (std::size_t command_index = 0;
         command_index < draw_list.commands.size();
         ++command_index) {
        const auto& command = draw_list.commands[command_index];
        if (command.material_index >= draw_list.materials.size() ||
            !vehicle_wheel_tread(
                command, draw_list.materials[command.material_index]))
            continue;
        auto found = std::find_if(
            wheels.begin(), wheels.end(),
            [&] (const SmoothWheel& wheel) {
                return same_wheel_group(wheel, command);
            });
        if (found == wheels.end()) {
            SmoothWheel wheel{};
            wheel.object_id = command.object_id;
            wheel.model_pointer = command.model_pointer;
            wheel.transform_id = command.transform_id;
            wheel.channel = command.channel;
            wheel.insertion_command = command_index;
            wheel.clip_x0 = (std::numeric_limits<std::int32_t>::max)();
            wheel.clip_y0 = (std::numeric_limits<std::int32_t>::max)();
            wheel.clip_x1 = (std::numeric_limits<std::int32_t>::min)();
            wheel.clip_y1 = (std::numeric_limits<std::int32_t>::min)();
            wheel.shell_minimum_x = (std::numeric_limits<double>::max)();
            wheel.shell_minimum_y = (std::numeric_limits<double>::max)();
            wheel.shell_maximum_x =
                (std::numeric_limits<double>::lowest)();
            wheel.shell_maximum_y =
                (std::numeric_limits<double>::lowest)();
            wheel.exact_transform = command.exact_transform_valid;
            wheels.push_back(wheel);
        }
    }

    struct Point {
        double model[3];
        double view[3];
    };
    std::vector<std::vector<Point>> wheel_points(wheels.size());
    std::vector<double> maximum_radius_squared(wheels.size(), 0.0);
    std::vector<double> maximum_abs_z(wheels.size(), 0.0);
    for (auto& wheel : wheels)
        wheel.insertion_command = draw_list.commands.size();
    // Gather every fitted wheel in one command-list pass.  The earlier
    // implementation rescanned the complete 4-5k-command frame once per
    // wheel, wasting the cadence margin on a 20-wheel field.
    for (std::size_t command_index = 0;
         command_index < draw_list.commands.size();
         ++command_index) {
        const auto& command = draw_list.commands[command_index];
        const auto found = std::find_if(
            wheels.begin(), wheels.end(),
            [&] (const SmoothWheel& wheel) {
                return same_wheel_group(wheel, command);
            });
        if (found == wheels.end())
            continue;
        const std::size_t wheel_index = static_cast<std::size_t>(
            found - wheels.begin());
        found->insertion_command = (std::min)(
            found->insertion_command, command_index);
        found->clip_x0 = (std::min)(
            found->clip_x0, static_cast<std::int32_t>(command.clip_x0));
        found->clip_y0 = (std::min)(
            found->clip_y0, static_cast<std::int32_t>(command.clip_y0));
        found->clip_x1 = (std::max)(
            found->clip_x1, static_cast<std::int32_t>(command.clip_x1));
        found->clip_y1 = (std::max)(
            found->clip_y1, static_cast<std::int32_t>(command.clip_y1));
        const bool tread =
            command.material_index < draw_list.materials.size() &&
            vehicle_wheel_tread(
                command, draw_list.materials[command.material_index]);
        auto& points = wheel_points[wheel_index];
        for (const auto& vertex : command.vertices) {
            const auto duplicate = std::find_if(
                points.begin(), points.end(),
                [&] (const Point& point) {
                    return
                        point.model[0] == vertex.model_x &&
                        point.model[1] == vertex.model_y &&
                        point.model[2] == vertex.model_z;
                });
            if (duplicate == points.end()) {
                points.push_back(Point{
                    {
                        static_cast<double>(vertex.model_x),
                        static_cast<double>(vertex.model_y),
                        static_cast<double>(vertex.model_z),
                    },
                    {
                        static_cast<double>(vertex.view_x),
                        static_cast<double>(vertex.view_y),
                        static_cast<double>(vertex.view_z),
                    },
                });
            }
            if (tread) {
                const double x = vertex.model_x;
                const double y = vertex.model_y;
                maximum_radius_squared[wheel_index] = (std::max)(
                    maximum_radius_squared[wheel_index], x * x + y * y);
                maximum_abs_z[wheel_index] = (std::max)(
                    maximum_abs_z[wheel_index],
                    std::fabs(static_cast<double>(vertex.model_z)));
            }
        }
    }

    for (std::size_t wheel_index = 0;
         wheel_index < wheels.size();
         ++wheel_index) {
        auto& wheel = wheels[wheel_index];
        const auto& points = wheel_points[wheel_index];
        if (points.size() < 4 ||
            maximum_radius_squared[wheel_index] <= 0.0 ||
            maximum_abs_z[wheel_index] <= 0.0) {
            wheel.radius = 0.0;
            continue;
        }
        for (const auto& point : points) {
            for (int axis = 0; axis < 3; ++axis) {
                wheel.model_centroid[axis] += point.model[axis];
                wheel.view_centroid[axis] += point.view[axis];
            }
        }
        const double inverse_count = 1.0 / points.size();
        for (int axis = 0; axis < 3; ++axis) {
            wheel.model_centroid[axis] *= inverse_count;
            wheel.view_centroid[axis] *= inverse_count;
        }
        const auto& transform_command =
            draw_list.commands[wheel.insertion_command];
        if (transform_command.exact_transform_valid) {
            wheel.exact_transform = true;
            constexpr double fixed_scale = 1.0 / 4096.0;
            for (int view_axis = 0; view_axis < 3; ++view_axis) {
                wheel.view_centroid[view_axis] =
                    transform_command.transform_translation[view_axis];
                for (int model_axis = 0; model_axis < 3; ++model_axis) {
                    wheel.model_to_view[view_axis][model_axis] =
                        transform_command.transform_rotation[
                            view_axis * 3 + model_axis] * fixed_scale;
                    wheel.view_centroid[view_axis] +=
                        wheel.model_to_view[view_axis][model_axis] *
                        wheel.model_centroid[model_axis];
                }
            }
            wheel.radius = std::sqrt(
                maximum_radius_squared[wheel_index]);
            wheel.half_width = maximum_abs_z[wheel_index];
            // Keep a measured consistency check around the authoritative
            // guest transform. A large residual means capture attribution is
            // wrong; it must not be hidden by constructing a fitted shell.
            for (const auto& point : points) {
                double error_squared = 0.0;
                for (int view_axis = 0; view_axis < 3; ++view_axis) {
                    double predicted =
                        transform_command.transform_translation[view_axis];
                    for (int model_axis = 0;
                         model_axis < 3;
                         ++model_axis) {
                        predicted +=
                            wheel.model_to_view[view_axis][model_axis] *
                            point.model[model_axis];
                    }
                    const double error =
                        predicted - point.view[view_axis];
                    error_squared += error * error;
                }
                wheel.maximum_fit_error = (std::max)(
                    wheel.maximum_fit_error,
                    std::sqrt(error_squared));
            }
            continue;
        }
        double covariance[3][3]{};
        double right[3][3]{};
        for (const auto& point : points) {
            double model[3]{};
            double view[3]{};
            for (int axis = 0; axis < 3; ++axis) {
                model[axis] =
                    point.model[axis] - wheel.model_centroid[axis];
                view[axis] =
                    point.view[axis] - wheel.view_centroid[axis];
            }
            for (int row = 0; row < 3; ++row) {
                for (int column = 0; column < 3; ++column) {
                    covariance[row][column] +=
                        model[row] * model[column];
                    right[column][row] += model[row] * view[column];
                }
            }
        }
        bool valid = true;
        for (int view_axis = 0; view_axis < 3; ++view_axis) {
            valid = valid && solve_three_by_three(
                covariance,
                right[view_axis],
                wheel.model_to_view[view_axis]);
        }
        wheel.radius = valid
            ? std::sqrt(maximum_radius_squared[wheel_index])
            : 0.0;
        wheel.half_width = maximum_abs_z[wheel_index];
        if (valid) {
            for (const auto& point : points) {
                double error_squared = 0.0;
                for (int view_axis = 0; view_axis < 3; ++view_axis) {
                    double predicted = wheel.view_centroid[view_axis];
                    for (int model_axis = 0;
                         model_axis < 3;
                         ++model_axis) {
                        predicted +=
                            wheel.model_to_view[view_axis][model_axis] *
                            (point.model[model_axis] -
                                wheel.model_centroid[model_axis]);
                    }
                    const double error =
                        predicted - point.view[view_axis];
                    error_squared += error * error;
                }
                wheel.maximum_fit_error = (std::max)(
                    wheel.maximum_fit_error,
                    std::sqrt(error_squared));
            }
        }
    }
    wheels.erase(
        std::remove_if(
            wheels.begin(), wheels.end(),
            [] (const SmoothWheel& wheel) {
                return
                    !std::isfinite(wheel.radius) || wheel.radius <= 0.0 ||
                    !std::isfinite(wheel.maximum_fit_error) ||
                    wheel.maximum_fit_error > 64.0;
            }),
        wheels.end());
    return wheels;
}

bool batch_compatible(
    const WorldDrawCommand& left,
    const WorldMaterial& left_material,
    const WorldDrawCommand& right,
    const WorldMaterial& right_material
) {
    const bool left_screen_space =
        (left_material.primitive_flags &
            world_primitive_screen_space_flag) != 0;
    const bool right_screen_space =
        (right_material.primitive_flags &
            world_primitive_screen_space_flag) != 0;
    const bool left_transparent =
        (left_material.primitive_flags & semi_transparent_flag) != 0;
    const bool right_transparent =
        (right_material.primitive_flags & semi_transparent_flag) != 0;
    const bool left_textured =
        (left_material.primitive_flags & textured_flag) != 0;
    const bool right_textured =
        (right_material.primitive_flags & textured_flag) != 0;
    return
        left_transparent == right_transparent &&
        (!left_transparent ||
            (left_textured == right_textured &&
                (left_material.texture_page & 0x60U) ==
                    (right_material.texture_page & 0x60U))) &&
        (left_material.environment_flags &
            (set_mask_flag | check_mask_flag)) ==
            (right_material.environment_flags &
                (set_mask_flag | check_mask_flag)) &&
        left.clip_x0 == right.clip_x0 &&
        left.clip_y0 == right.clip_y0 &&
        left.clip_x1 == right.clip_x1 &&
        left.clip_y1 == right.clip_y1 &&
        left.channel == right.channel &&
        left_screen_space == right_screen_space &&
        left.object_kind == right.object_kind &&
        left.ordering_table_index == right.ordering_table_index &&
        ((!left_transparent &&
                (left.object_kind == 1U ||
                    (left.object_kind == 2U &&
                        left.object_id == right.object_id))) ||
            (left.object_id == right.object_id &&
                (left.object_kind == 2U
                    ? left.transform_id == right.transform_id
                    : left.model_pointer == right.model_pointer)));
}

struct GpuVertex {
    float position[4];
    float uv[2];
    float color[4];
    std::uint32_t command_index;
};

constexpr std::uint32_t replacement_mode_rgb = 0;
constexpr std::uint32_t replacement_mode_palette_detail = 1;
constexpr std::uint32_t perspective_uv_eligible_flag = 1U << 3U;
constexpr float maximum_gt2_perspective_depth_ratio = 8.0F;

bool is_opaque_track_surface(
    const WorldDrawCommand& command,
    const WorldMaterial& material
) noexcept {
    const auto& a = command.vertices[0];
    const auto& b = command.vertices[1];
    const auto& c = command.vertices[2];
    const std::int64_t ab_x =
        static_cast<std::int64_t>(b.model_x) - a.model_x;
    const std::int64_t ab_y =
        static_cast<std::int64_t>(b.model_y) - a.model_y;
    const std::int64_t ab_z =
        static_cast<std::int64_t>(b.model_z) - a.model_z;
    const std::int64_t ac_x =
        static_cast<std::int64_t>(c.model_x) - a.model_x;
    const std::int64_t ac_y =
        static_cast<std::int64_t>(c.model_y) - a.model_y;
    const std::int64_t ac_z =
        static_cast<std::int64_t>(c.model_z) - a.model_z;
    const std::int64_t normal_x = ab_y * ac_z - ab_z * ac_y;
    const std::int64_t normal_y = ab_z * ac_x - ab_x * ac_z;
    const std::int64_t normal_z = ab_x * ac_y - ab_y * ac_x;
    return command.object_kind == 1U &&
        (material.primitive_flags & textured_flag) != 0 &&
        (material.primitive_flags & semi_transparent_flag) == 0 &&
        (material.primitive_flags & world_primitive_screen_space_flag) == 0 &&
        std::llabs(normal_y) >= std::llabs(normal_x) &&
        std::llabs(normal_y) >= std::llabs(normal_z);
}

bool perspective_uv_eligible(
    const WorldDrawCommand& command,
    const WorldMaterial& material
) {
    if ((material.primitive_flags & textured_flag) == 0 ||
        (material.primitive_flags &
            (world_primitive_screen_space_flag |
                world_primitive_temporal_seam_flag)) != 0)
        return false;
    float minimum = (std::numeric_limits<float>::max)();
    float maximum = 0.0F;
    for (const auto& vertex : command.vertices) {
        if (!std::isfinite(vertex.clip_w) || vertex.clip_w <= 0.0F)
            return false;
        minimum = (std::min)(minimum, vertex.clip_w);
        maximum = (std::max)(maximum, vertex.clip_w);
    }
    // GT2 subdivides and assigns UVs for the PS1's affine rasterizer. Across
    // very deep track strips those coordinates are deliberately close to
    // screen-linear; treating them as an unmodified projective parameterization
    // double-corrects the strip and expands a one-texel atlas edge into a large
    // polygon. The original RecompOne perspective path used the same bounded
    // depth contract. Preserve perspective correction on coherent local
    // surfaces while the GT-specific UV-island reconstruction is developed.
    return maximum / minimum <= maximum_gt2_perspective_depth_ratio;
}

struct PerspectiveUvVertexKey {
    std::int32_t view_x{};
    std::int32_t view_y{};
    std::int32_t view_z{};
    std::int32_t u{};
    std::int32_t v{};

    bool operator==(const PerspectiveUvVertexKey& other) const noexcept {
        return view_x == other.view_x &&
            view_y == other.view_y &&
            view_z == other.view_z &&
            u == other.u &&
            v == other.v;
    }

    bool operator<(const PerspectiveUvVertexKey& other) const noexcept {
        if (view_x != other.view_x) return view_x < other.view_x;
        if (view_y != other.view_y) return view_y < other.view_y;
        if (view_z != other.view_z) return view_z < other.view_z;
        if (u != other.u) return u < other.u;
        return v < other.v;
    }
};

struct PerspectiveUvEdgeKey {
    std::uint32_t continuity_kind{};
    std::uint32_t object_kind{};
    std::uint32_t object_id{};
    std::uint32_t model_pointer{};
    std::uint32_t primitive_flags{};
    std::uint32_t texture_page{};
    std::uint32_t clut{};
    std::int32_t texture_mask_x{};
    std::int32_t texture_mask_y{};
    std::int32_t texture_offset_x{};
    std::int32_t texture_offset_y{};
    PerspectiveUvVertexKey first{};
    PerspectiveUvVertexKey second{};

    bool operator==(const PerspectiveUvEdgeKey& other) const noexcept {
        return continuity_kind == other.continuity_kind &&
            object_kind == other.object_kind &&
            object_id == other.object_id &&
            model_pointer == other.model_pointer &&
            primitive_flags == other.primitive_flags &&
            texture_page == other.texture_page &&
            clut == other.clut &&
            texture_mask_x == other.texture_mask_x &&
            texture_mask_y == other.texture_mask_y &&
            texture_offset_x == other.texture_offset_x &&
            texture_offset_y == other.texture_offset_y &&
            first == other.first &&
            second == other.second;
    }
};

struct PerspectiveUvEdgeHash {
    std::size_t operator()(const PerspectiveUvEdgeKey& key) const noexcept {
        std::size_t result = 1469598103934665603ULL;
        const auto mix = [&result](std::uint64_t value) {
            result ^= static_cast<std::size_t>(value);
            result *= 1099511628211ULL;
        };
        mix(key.continuity_kind);
        mix(key.object_kind);
        mix(key.object_id);
        mix(key.model_pointer);
        mix(key.primitive_flags);
        mix(key.texture_page);
        mix(key.clut);
        mix(static_cast<std::uint32_t>(key.texture_mask_x));
        mix(static_cast<std::uint32_t>(key.texture_mask_y));
        mix(static_cast<std::uint32_t>(key.texture_offset_x));
        mix(static_cast<std::uint32_t>(key.texture_offset_y));
        for (const auto& vertex : {key.first, key.second}) {
            mix(static_cast<std::uint32_t>(vertex.view_x));
            mix(static_cast<std::uint32_t>(vertex.view_y));
            mix(static_cast<std::uint32_t>(vertex.view_z));
            mix(static_cast<std::uint32_t>(vertex.u));
            mix(static_cast<std::uint32_t>(vertex.v));
        }
        return result;
    }
};

std::vector<std::uint8_t> perspective_uv_island_eligibility(
    const WorldDrawList& draw_list
) {
    const std::size_t count = draw_list.commands.size();
    std::vector<std::size_t> parent(count);
    std::vector<std::uint8_t> rank(count, 0);
    std::vector<std::uint8_t> eligible(count, 0);
    std::size_t individually_eligible = 0;
    for (std::size_t index = 0; index < count; ++index) {
        parent[index] = index;
        const auto& command = draw_list.commands[index];
        if (command.material_index < draw_list.materials.size()) {
            eligible[index] = perspective_uv_eligible(
                command,
                draw_list.materials[command.material_index])
                ? 1U
                : 0U;
            individually_eligible += eligible[index] != 0 ? 1U : 0U;
        }
    }
    const auto find_root = [&parent](std::size_t value) {
        std::size_t root = value;
        while (parent[root] != root)
            root = parent[root];
        while (parent[value] != value) {
            const std::size_t next = parent[value];
            parent[value] = root;
            value = next;
        }
        return root;
    };
    const auto unite = [&parent, &rank, &find_root](
        std::size_t left,
        std::size_t right
    ) {
        left = find_root(left);
        right = find_root(right);
        if (left == right)
            return;
        if (rank[left] < rank[right])
            std::swap(left, right);
        parent[right] = left;
        if (rank[left] == rank[right])
            ++rank[left];
    };
    const auto vertex_key = [](const WorldDrawVertex& vertex) {
        return PerspectiveUvVertexKey{
            vertex.exact_view_x,
            vertex.exact_view_y,
            vertex.exact_view_z,
            static_cast<std::int32_t>(std::lround(vertex.u * 1024.0F)),
            static_cast<std::int32_t>(std::lround(vertex.v * 1024.0F)),
        };
    };
    std::unordered_map<
        PerspectiveUvEdgeKey,
        std::size_t,
        PerspectiveUvEdgeHash> edges;
    edges.reserve(count * 3);
    for (std::size_t command_index = 0;
         command_index < count;
         ++command_index) {
        const auto& command = draw_list.commands[command_index];
        if (command.material_index >= draw_list.materials.size())
            continue;
        const auto& material =
            draw_list.materials[command.material_index];
        if ((material.primitive_flags & textured_flag) == 0 ||
            (material.primitive_flags &
                (world_primitive_screen_space_flag |
                    world_primitive_temporal_seam_flag)) != 0)
            continue;
        for (int edge_index = 0; edge_index < 3; ++edge_index) {
            const auto& first_vertex = command.vertices[edge_index];
            const auto& second_vertex =
                command.vertices[(edge_index + 1) % 3];
            if (!first_vertex.exact_transform_valid ||
                !second_vertex.exact_transform_valid)
                continue;
            auto first = vertex_key(first_vertex);
            auto second = vertex_key(second_vertex);
            if (second < first)
                std::swap(first, second);
            const PerspectiveUvEdgeKey key{
                0U,
                command.object_kind,
                command.object_id,
                command.model_pointer,
                material.primitive_flags,
                material.texture_page,
                material.clut,
                material.texture_mask_x,
                material.texture_mask_y,
                material.texture_offset_x,
                material.texture_offset_y,
                first,
                second,
            };
            const auto [found, inserted] = edges.emplace(
                key, command_index);
            if (!inserted)
                unite(found->second, command_index);

            // Road and terrain UV islands meet at deliberate atlas seams.
            // Even though their UV endpoints differ, changing interpolation
            // mode on that shared geometric edge introduces a new crack that
            // GT2 never authored. Join horizontal opaque track surfaces by
            // their exact view-space edge so one continuous ground surface
            // receives one projection contract across all of its UV islands.
            if (is_opaque_track_surface(command, material)) {
                first.u = first.v = 0;
                second.u = second.v = 0;
                const PerspectiveUvEdgeKey surface_key{
                    1U,
                    command.object_kind,
                    command.object_id,
                    command.model_pointer,
                    0U,
                    0U,
                    0U,
                    0,
                    0,
                    0,
                    0,
                    first,
                    second,
                };
                const auto [surface_found, surface_inserted] =
                    edges.emplace(surface_key, command_index);
                if (!surface_inserted)
                    unite(surface_found->second, command_index);
            }
        }
    }
    std::vector<std::uint8_t> island_eligible(count, 1U);
    for (std::size_t index = 0; index < count; ++index) {
        const std::size_t root = find_root(index);
        island_eligible[root] = static_cast<std::uint8_t>(
            island_eligible[root] != 0 && eligible[index] != 0);
    }
    for (std::size_t index = 0; index < count; ++index)
        eligible[index] = island_eligible[find_root(index)];
    if (std::getenv("OPENGT_RENDER_UV_DIAGNOSTICS") != nullptr) {
        std::size_t textured_commands = 0;
        std::size_t island_eligible_count = 0;
        std::size_t track_fallback = 0;
        std::size_t vehicle_fallback = 0;
        std::size_t other_fallback = 0;
        std::vector<std::size_t> island_sizes(count, 0);
        for (std::size_t index = 0; index < count; ++index) {
            const auto& command = draw_list.commands[index];
            if (command.material_index >= draw_list.materials.size())
                continue;
            const auto& material =
                draw_list.materials[command.material_index];
            if ((material.primitive_flags & textured_flag) == 0 ||
                (material.primitive_flags &
                    world_primitive_screen_space_flag) != 0)
                continue;
            ++textured_commands;
            island_eligible_count += eligible[index] != 0 ? 1U : 0U;
            if (eligible[index] == 0) {
                const auto kind = command.object_kind;
                if (kind == 1U) ++track_fallback;
                else if (kind == 2U) ++vehicle_fallback;
                else ++other_fallback;
            }
            ++island_sizes[find_root(index)];
        }
        const std::size_t largest_island = island_sizes.empty()
            ? 0
            : *std::max_element(island_sizes.begin(), island_sizes.end());
        std::fprintf(
            stderr,
            "[Render-UV] commands=%zu textured=%zu "
            "individualPerspective=%zu "
            "islandPerspective=%zu fallback=%zu trackFallback=%zu "
            "vehicleFallback=%zu otherFallback=%zu edgeKeys=%zu "
            "largestIsland=%zu maxDepthRatio=%.1f\n",
            count,
            textured_commands,
            individually_eligible,
            island_eligible_count,
            textured_commands - island_eligible_count,
            track_fallback,
            vehicle_fallback,
            other_fallback,
            edges.size(),
            largest_island,
            maximum_gt2_perspective_depth_ratio);
    }
    return eligible;
}

struct GpuMaterial {
    std::uint32_t primitive_flags;
    std::uint32_t texture_page;
    std::uint32_t clut;
    std::uint32_t environment_flags;
    std::int32_t texture_mask_x;
    std::int32_t texture_mask_y;
    std::int32_t texture_offset_x;
    std::int32_t texture_offset_y;
    std::uint32_t coverage_flags;
    std::int32_t source_min_u;
    std::int32_t source_min_v;
    std::uint32_t source_width;
    std::uint32_t source_height;
    std::uint32_t replacement_x;
    std::uint32_t replacement_y;
    std::uint32_t replacement_width;
    std::uint32_t replacement_height;
    std::uint32_t replacement_mode;
    float replacement_scale_r;
    float replacement_scale_g;
    float replacement_scale_b;
    float replacement_bias_r;
    float replacement_bias_g;
    float replacement_bias_b;
};

struct DrawConstants {
    std::uint32_t base_command;
    std::uint32_t pass_kind;
    std::uint32_t dithering;
    std::uint32_t perspective_correct;
    std::uint32_t texture_smoothing;
    std::uint32_t footprint_coverage;
    std::uint32_t replacement_atlas_width;
    std::uint32_t replacement_atlas_height;
};

const char shader_source[] = R"(
Texture2D<uint> Vram : register(t0);
Texture2D<float4> ReplacementAtlas : register(t2);
SamplerState ReplacementSampler : register(s0);

struct MaterialData {
    uint primitiveFlags;
    uint texturePage;
    uint clut;
    uint environmentFlags;
    int textureMaskX;
    int textureMaskY;
    int textureOffsetX;
    int textureOffsetY;
    uint coverageFlags;
    int sourceMinU;
    int sourceMinV;
    uint sourceWidth;
    uint sourceHeight;
    uint replacementX;
    uint replacementY;
    uint replacementWidth;
    uint replacementHeight;
    uint replacementMode;
    float replacementScaleR;
    float replacementScaleG;
    float replacementScaleB;
    float replacementBiasR;
    float replacementBiasG;
    float replacementBiasB;
};

StructuredBuffer<MaterialData> Materials : register(t1);

cbuffer DrawConstants : register(b0) {
    uint BaseCommand;
    uint PassKind;
    uint Dithering;
    uint PerspectiveCorrect;
    uint TextureSmoothing;
    uint FootprintCoverage;
    uint ReplacementAtlasWidth;
    uint ReplacementAtlasHeight;
};

struct VsInput {
    float4 position : POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR0;
    uint commandIndex : TEXCOORD2;
};

struct VsOutput {
    float4 position : SV_POSITION;
    float2 perspectiveUv : TEXCOORD0;
    noperspective float2 affineUv : TEXCOORD1;
    noperspective float4 color : COLOR0;
    nointerpolation uint commandIndex : TEXCOORD2;
};

struct PsOutput {
    float4 color : SV_Target0;
    float4 blendFactors : SV_Target1;
};

VsOutput VSMain(VsInput input) {
    VsOutput output;
    output.position = input.position;
    output.perspectiveUv = input.uv;
    output.affineUv = input.uv;
    output.color = input.color;
    output.commandIndex = input.commandIndex;
    return output;
}

uint VramAt(int x, int y) {
    return Vram.Load(int3(x & 1023, y & 511, 0));
}

uint TextureWord(int rawU, int rawV, MaterialData material) {
    int u =
        (rawU & ~(material.textureMaskX * 8)) |
        ((material.textureOffsetX & material.textureMaskX) * 8);
    int v =
        (rawV & ~(material.textureMaskY * 8)) |
        ((material.textureOffsetY & material.textureMaskY) * 8);
    u &= 255;
    v &= 255;
    int pageX = (material.texturePage & 15) * 64;
    int pageY = ((material.texturePage >> 4) & 1) * 256;
    int mode = (material.texturePage >> 7) & 3;
    int clutX = (material.clut & 63) * 16;
    int clutY = (material.clut >> 6) & 511;
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

float3 TextureColor(uint word) {
    return float3(
        Expand5(word & 31),
        Expand5((word >> 5) & 31),
        Expand5((word >> 10) & 31));
}

struct FootprintSample {
    float3 color;
    uint word;
    bool any;
};

// A pixel that covers more than one source texel has to decide transparency
// from everything it covers. GT2 keys transparency on word==0, so a single
// point sample of a minified texture drops whatever fraction of pixels happen
// to land on the key. Vertical scenery - signage, barriers, grandstands,
// buildings - has no equivalent of the opaque-road texel repair, so at
// distance it lost most of its pixels and read as invisible while the road
// behind it kept drawing. Averaging the covered texels restores the coverage
// a minifying sampler is supposed to produce, and only a footprint that is
// transparent everywhere still discards, which preserves authored cutouts.
FootprintSample SampleFootprint(
    float2 uv,
    float2 footprint,
    MaterialData material
) {
    FootprintSample result;
    result.color = 0.0;
    result.word = 0;
    result.any = false;
    // Three stratified taps per axis bound the cost at nine loads however
    // small the surface becomes on screen.
    float2 stride = footprint / 3.0;
    float coverage = 0.0;
    float nearest = 1.0e9;
    [unroll]
    for (int offsetV = -1; offsetV <= 1; ++offsetV) {
        [unroll]
        for (int offsetU = -1; offsetU <= 1; ++offsetU) {
            float2 offset = float2(offsetU, offsetV) * stride;
            uint word = TextureWord(
                (int)floor(uv.x + offset.x + 0.0001),
                (int)floor(uv.y + offset.y + 0.0001),
                material);
            if (word == 0)
                continue;
            result.color += TextureColor(word);
            coverage += 1.0;
            float span = abs(offset.x) + abs(offset.y);
            if (span < nearest) {
                nearest = span;
                result.word = word;
            }
        }
    }
    if (coverage > 0.0) {
        result.color /= coverage;
        result.any = true;
    }
    return result;
}

float3 FilteredTextureColor(
    float2 uv,
    uint centerWord,
    MaterialData material
) {
    int2 base = int2(floor(uv));
    float2 blend = frac(uv);
    uint words[4] = {
        TextureWord(base.x, base.y, material),
        TextureWord(base.x + 1, base.y, material),
        TextureWord(base.x, base.y + 1, material),
        TextureWord(base.x + 1, base.y + 1, material)
    };
    float weights[4] = {
        (1.0 - blend.x) * (1.0 - blend.y),
        blend.x * (1.0 - blend.y),
        (1.0 - blend.x) * blend.y,
        blend.x * blend.y
    };
    float3 color = 0.0;
    float coverage = 0.0;
    [unroll]
    for (int index = 0; index < 4; ++index) {
        // Transparent palette entries must not bleed dark fringes into the
        // opaque part of a billboard or car decal. Preserve the center
        // texel's alpha/STP decision and renormalize only visible neighbors.
        if (words[index] != 0) {
            color += TextureColor(words[index]) * weights[index];
            coverage += weights[index];
        }
    }
    return coverage > 0.0001
        ? color / coverage
        : TextureColor(centerWord);
}

float4 ReplacementTextureSample(float2 uv, MaterialData material) {
    int2 integerUv = int2(floor(uv));
    float2 mappedUv = float2(
        (integerUv.x & ~(material.textureMaskX * 8)) |
            ((material.textureOffsetX & material.textureMaskX) * 8),
        (integerUv.y & ~(material.textureMaskY * 8)) |
            ((material.textureOffsetY & material.textureMaskY) * 8)) +
        frac(uv);
    float2 sourceSize = float2(
        max(material.sourceWidth, 1U),
        max(material.sourceHeight, 1U));
    float2 local =
        (mappedUv - float2(material.sourceMinU, material.sourceMinV) + 0.5) /
        sourceSize;
    float2 outputSize = float2(
        max(material.replacementWidth, 1U),
        max(material.replacementHeight, 1U));
    float2 halfTexel = 0.5 / outputSize;
    local = clamp(local, halfTexel, 1.0 - halfTexel);
    float2 minimumPixel = float2(
        material.replacementX, material.replacementY) + 0.5;
    float2 maximumPixel = float2(
        material.replacementX + material.replacementWidth,
        material.replacementY + material.replacementHeight) - 0.5;
    float2 atlasPixel = clamp(
        float2(material.replacementX, material.replacementY) +
            local * outputSize,
        minimumPixel,
        maximumPixel);
    float2 atlasSize = float2(
        max(ReplacementAtlasWidth, 1U),
        max(ReplacementAtlasHeight, 1U));
    return ReplacementAtlas.SampleLevel(
        ReplacementSampler, atlasPixel / atlasSize, 0);
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

PsOutput PSMain(VsOutput input) {
    MaterialData material = Materials[input.commandIndex];
    bool textured = (material.primitiveFlags & 1) != 0;
    bool semitransparent = (material.primitiveFlags & 2) != 0;
    bool rawTexture = (material.primitiveFlags & 4) != 0;
    bool screenSpace =
        (material.primitiveFlags & 0x80000000) != 0;
    bool perspectiveEligible =
        (material.coverageFlags & 8) != 0;
    float2 uv = PerspectiveCorrect != 0 && perspectiveEligible
        ? input.perspectiveUv
        : input.affineUv;
    float3 color = saturate(input.color.rgb);
    bool textureStp = false;
    if (textured) {
        // PS1 UV interpolation assigns the complete [N,N+1) interval to
        // texel N. Keep the world alpha/STP and footprint decisions aligned
        // with that contract, the bilinear color base, and replacement UVs.
        // Rounding here shifted only the visibility test by half a texel, so
        // scenery could be discarded while its color came from its neighbor.
        int sampleU = (int)floor(uv.x + 0.0001);
        int sampleV = (int)floor(uv.y + 0.0001);
        // Screen-space HUD material keeps exact PS1 texel selection. Only a
        // world surface small enough that one pixel spans several texels
        // takes the coverage path.
        float2 footprint = max(abs(ddx(uv)), abs(ddy(uv)));
        bool minified =
            FootprintCoverage != 0 &&
            !screenSpace && max(footprint.x, footprint.y) > 1.0;
        uint word = TextureWord(sampleU, sampleV, material);
        float3 recoveredColor = 0.0;
        bool coveredTexel = false;
        if (minified) {
            FootprintSample covered = SampleFootprint(
                uv, footprint, material);
            if (!covered.any)
                discard;
            word = covered.word;
            recoveredColor = covered.color;
            coveredTexel = true;
        } else if (word == 0) {
            if ((material.coverageFlags & 1) == 0)
                discard;
            uint neighbors[4] = {
                TextureWord(sampleU - 1, sampleV, material),
                TextureWord(sampleU + 1, sampleV, material),
                TextureWord(sampleU, sampleV - 1, material),
                TextureWord(sampleU, sampleV + 1, material)
            };
            uint visibleNeighbors = 0;
            [unroll]
            for (int neighbor = 0; neighbor < 4; ++neighbor) {
                if (neighbors[neighbor] != 0) {
                    recoveredColor += TextureColor(neighbors[neighbor]);
                    ++visibleNeighbors;
                }
            }
            // Fill only an isolated transparent source texel. Two or more
            // transparent cardinal neighbors identify an authored cutout or
            // texture edge and retain normal PS1 alpha-test behavior.
            if (visibleNeighbors < 3)
                discard;
            recoveredColor /= visibleNeighbors;
        }
        bool stp = (word & 0x8000) != 0;
        textureStp = stp;
        if (semitransparent) {
            if (PassKind == 0 && stp)
                discard;
            if (PassKind == 1 && !stp)
                discard;
        }
        // HUD and text retain exact PS1 texel edges. Perspective-correct 3D
        // materials use decoded bilinear filtering even for indexed CLUT
        // textures, avoiding nearest-neighbor blocks at the 4x output scale.
        float3 texel = coveredTexel || word == 0
            ? recoveredColor
            : screenSpace || TextureSmoothing == 0
            ? TextureColor(word)
            : FilteredTextureColor(uv, word, material);
        if (!screenSpace && material.replacementWidth != 0) {
            float4 replacement = ReplacementTextureSample(uv, material);
            if (material.replacementMode == 1) {
                // Cars retain GT2's native indexed bitmap -> selected live
                // material CLUT path above. The neural asset contributes only
                // paint-neutral 4x detail: R is neural luminance and G is the
                // matching bilinear source luminance. Their bounded ratio
                // cannot bake a paint choice or track-light palette into RGB.
                float detailRatio = clamp(
                    (replacement.r + 0.0625) /
                        (replacement.g + 0.0625),
                    0.5,
                    2.0);
                texel = saturate(texel * detailRatio);
            } else {
                texel = saturate(
                    replacement.rgb * float3(
                        material.replacementScaleR,
                        material.replacementScaleG,
                        material.replacementScaleB) +
                    float3(
                        material.replacementBiasR,
                        material.replacementBiasG,
                        material.replacementBiasB));
            }
        }
        color = rawTexture
            ? texel
            : saturate(texel * input.color.rgb * 2.0);
    }
    if (
        Dithering != 0 &&
        (material.environmentFlags & 4) != 0 &&
        (!textured || !rawTexture)
    )
        color = Quantize(color, int2(input.position.xy));
    // GT2's car shadow is an untextured reverse-subtract polygon. Directly
    // scaling the PS1 5-bit subtraction to modern color space clamps dark
    // asphalt to opaque black, making the polygon look like a wheel texture.
    // Marked shadows instead supply a low-opacity black contact layer. The
    // source mesh is too coarse and irregular to feather without exposing its
    // individual triangles, so keep the whole footprint subtle enough that
    // its boundary cannot read as wheel or underbody geometry.
    bool vehicleShadow = (material.coverageFlags & 2) != 0;
    bool vehicleWheelTread = (material.coverageFlags & 4) != 0;
    if (vehicleShadow)
        color = 0.0;
    else if (vehicleWheelTread)
        discard;
    PsOutput output;
    output.color = float4(color, vehicleShadow ? input.color.a : 1.0);
    float sourceFactor = 1.0;
    float destinationFactor = 0.0;
    if (PassKind == 2 && semitransparent && textureStp) {
        uint blendMode = (material.texturePage >> 5) & 3;
        sourceFactor = blendMode == 0 ? 0.5 :
            blendMode == 3 ? 0.25 : 1.0;
        destinationFactor = blendMode == 0 ? 0.5 : 1.0;
    }
    output.blendFactors = float4(
        destinationFactor,
        destinationFactor,
        destinationFactor,
        sourceFactor);
    return output;
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
        target.SrcBlend = mode == 4
            ? D3D11_BLEND_SRC1_ALPHA :
            mode == 5 ? D3D11_BLEND_SRC_ALPHA :
            mode == 0 ? D3D11_BLEND_BLEND_FACTOR :
            mode == 3 ? D3D11_BLEND_BLEND_FACTOR :
            D3D11_BLEND_ONE;
        target.DestBlend = mode == 4
            ? D3D11_BLEND_SRC1_COLOR :
            mode == 5 ? D3D11_BLEND_INV_SRC_ALPHA :
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

struct ReplacementRect {
    std::uint32_t x{};
    std::uint32_t y{};
    std::uint32_t width{};
    std::uint32_t height{};

    explicit operator bool() const noexcept {
        return width != 0 && height != 0;
    }
};

struct ReplacementSignature {
    std::uint16_t texture_page{};
    std::uint16_t clut{};
    std::int16_t mask_x{};
    std::int16_t mask_y{};
    std::int16_t offset_x{};
    std::int16_t offset_y{};
    std::int16_t minimum_u{};
    std::int16_t minimum_v{};
    std::int16_t maximum_u{};
    std::int16_t maximum_v{};

    bool operator==(const ReplacementSignature& right) const noexcept {
        return std::memcmp(this, &right, sizeof(*this)) == 0;
    }
};

struct ReplacementSignatureHash {
    std::size_t operator()(const ReplacementSignature& value) const noexcept {
        const auto* bytes = reinterpret_cast<const std::uint8_t*>(&value);
        std::uint64_t hash = 14695981039346656037ULL;
        for (std::size_t index = 0; index < sizeof(value); ++index) {
            hash ^= bytes[index];
            hash *= 1099511628211ULL;
        }
        return static_cast<std::size_t>(hash);
    }
};

struct ReplacementResolution {
    ReplacementRect rect{};
    std::int32_t minimum_u{};
    std::int32_t minimum_v{};
    std::uint32_t source_width{};
    std::uint32_t source_height{};
    std::uint64_t key{};
    std::uint64_t palette_key{};
    std::uint32_t mode{replacement_mode_rgb};
    float color_scale[3]{1.0F, 1.0F, 1.0F};
    float color_bias[3]{};
};

struct ReplacementEntry {
    ReplacementRect rect{};
    std::uint32_t source_width{};
    std::uint32_t source_height{};
    std::uint32_t pixel_mode{};
    std::uint64_t palette_key{};
    std::uint32_t mode{replacement_mode_rgb};
    bool color_fit{true};
    std::vector<std::uint8_t> canonical_rgb;
};

struct ReplacementIdentity {
    std::uint64_t key{};
    std::uint64_t palette_key{};

    bool operator==(const ReplacementIdentity& right) const noexcept {
        return key == right.key && palette_key == right.palette_key;
    }
};

struct ReplacementIdentityHash {
    std::size_t operator()(const ReplacementIdentity& value) const noexcept {
        std::uint64_t hash = 14695981039346656037ULL;
        for (const std::uint64_t part : {value.key, value.palette_key}) {
            for (int byte = 0; byte < 8; ++byte) {
                hash ^= static_cast<std::uint8_t>(part >> (byte * 8));
                hash *= 1099511628211ULL;
            }
        }
        return static_cast<std::size_t>(hash);
    }
};

struct CachedReplacementResolution {
    std::uint64_t texture_revision{};
    ReplacementResolution resolution{};
};

enum class HudHorizontalAnchor : std::int8_t {
    left = -1,
    center = 0,
    right = 1,
};

struct HudHorizontalPlacement {
    HudHorizontalAnchor anchor{HudHorizontalAnchor::center};
    // Authored distance from the selected guest edge. The renderer scales
    // this margin with the target width while leaving the HUD artwork itself
    // at its authored size.
    float edge_margin{};
};

struct HudBounds {
    float minimum_x{};
    float minimum_y{};
    float maximum_x{};
    float maximum_y{};
    bool eligible{};
    bool connectable{};
};

std::vector<HudHorizontalPlacement> build_hud_horizontal_placements(
    const WorldDrawList& draw_list
) {
    std::vector<HudHorizontalPlacement> placements(
        draw_list.commands.size());
    std::vector<HudBounds> bounds(draw_list.commands.size());
    std::vector<std::size_t> parents(draw_list.commands.size());
    for (std::size_t index = 0; index < parents.size(); ++index)
        parents[index] = index;

    const auto find_root = [&parents] (std::size_t index) {
        std::size_t root = index;
        while (parents[root] != root)
            root = parents[root];
        while (parents[index] != index) {
            const std::size_t next = parents[index];
            parents[index] = root;
            index = next;
        }
        return root;
    };
    const auto join = [&parents, &find_root] (
        std::size_t left,
        std::size_t right
    ) {
        const std::size_t left_root = find_root(left);
        const std::size_t right_root = find_root(right);
        if (left_root != right_root)
            parents[right_root] = left_root;
    };

    std::vector<std::size_t> hud_commands;
    hud_commands.reserve(draw_list.unclassified_commands);
    for (std::size_t command_index = 0;
         command_index < draw_list.commands.size();
         ++command_index) {
        const auto& command = draw_list.commands[command_index];
        if (command.material_index >= draw_list.materials.size())
            continue;
        const auto& material = draw_list.materials[command.material_index];
        if (
            (material.primitive_flags &
                world_primitive_screen_space_flag) == 0 ||
            command.channel != WorldViewChannel::main_view
        )
            continue;
        auto& item = bounds[command_index];
        item.minimum_x = item.maximum_x = command.vertices[0].screen_x;
        item.minimum_y = item.maximum_y = command.vertices[0].screen_y;
        for (int vertex_index = 1; vertex_index < 3; ++vertex_index) {
            const auto& vertex = command.vertices[vertex_index];
            item.minimum_x = (std::min)(item.minimum_x, vertex.screen_x);
            item.minimum_y = (std::min)(item.minimum_y, vertex.screen_y);
            item.maximum_x = (std::max)(item.maximum_x, vertex.screen_x);
            item.maximum_y = (std::max)(item.maximum_y, vertex.screen_y);
        }
        item.eligible = true;
        item.connectable =
            item.maximum_x - item.minimum_x <=
                static_cast<float>(draw_list.display_width) * 0.75F &&
            item.maximum_y - item.minimum_y <=
                static_cast<float>(draw_list.display_height) * 0.75F;
        hud_commands.push_back(command_index);
    }

    // A PS1 sprite or font run arrives as several independently triangulated
    // commands.  Anchor the connected run as one HUD element so the two
    // halves of a quad, or the final glyph in a left-side label, cannot choose
    // different widescreen margins.  The two-pixel tolerance bridges the
    // authored spacing between adjacent font glyphs without joining separate
    // HUD panels.
    constexpr float connection_gap = 2.0F;
    for (std::size_t left_index = 0;
         left_index < hud_commands.size();
         ++left_index) {
        const std::size_t left_command = hud_commands[left_index];
        const auto& left = bounds[left_command];
        if (!left.connectable)
            continue;
        for (std::size_t right_index = left_index + 1;
             right_index < hud_commands.size();
             ++right_index) {
            const std::size_t right_command = hud_commands[right_index];
            const auto& right = bounds[right_command];
            if (!right.connectable)
                continue;
            const bool separated_x =
                left.maximum_x + connection_gap < right.minimum_x ||
                right.maximum_x + connection_gap < left.minimum_x;
            const bool separated_y =
                left.maximum_y + connection_gap < right.minimum_y ||
                right.maximum_y + connection_gap < left.minimum_y;
            if (!separated_x && !separated_y)
                join(left_command, right_command);
        }
    }

    std::vector<HudBounds> component_bounds(draw_list.commands.size());
    for (const std::size_t command_index : hud_commands) {
        const std::size_t root = find_root(command_index);
        const auto& source = bounds[command_index];
        auto& component = component_bounds[root];
        if (!component.eligible) {
            component = source;
        } else {
            component.minimum_x = (std::min)(
                component.minimum_x, source.minimum_x);
            component.minimum_y = (std::min)(
                component.minimum_y, source.minimum_y);
            component.maximum_x = (std::max)(
                component.maximum_x, source.maximum_x);
            component.maximum_y = (std::max)(
                component.maximum_y, source.maximum_y);
        }
    }

    const float display_x = static_cast<float>(draw_list.display_x);
    const float display_width = static_cast<float>(draw_list.display_width);
    const float left_limit = display_x + display_width * 0.4F;
    const float right_limit = display_x + display_width * 0.6F;
    for (const std::size_t command_index : hud_commands) {
        const auto& component = component_bounds[find_root(command_index)];
        const float center_x =
            (component.minimum_x + component.maximum_x) * 0.5F;
        auto& placement = placements[command_index];
        placement.anchor = center_x < left_limit
            ? HudHorizontalAnchor::left
            : center_x > right_limit
            ? HudHorizontalAnchor::right
            : HudHorizontalAnchor::center;
        if (placement.anchor == HudHorizontalAnchor::left) {
            placement.edge_margin = (std::max)(
                0.0F, component.minimum_x - display_x);
        } else if (placement.anchor == HudHorizontalAnchor::right) {
            placement.edge_margin = (std::max)(
                0.0F,
                display_x + display_width - component.maximum_x);
        }
    }
    if (!hud_commands.empty()) {
        static thread_local bool emitted = false;
        if (!emitted) {
            emitted = true;
            std::array<std::size_t, 3> anchor_counts{};
            std::size_t component_count = 0;
            for (const std::size_t command_index : hud_commands) {
                const auto anchor = placements[command_index].anchor;
                ++anchor_counts[static_cast<std::size_t>(
                    static_cast<std::int8_t>(anchor) + 1)];
                if (find_root(command_index) == command_index)
                    ++component_count;
            }
            std::fprintf(
                stderr,
                "[Render-HUD] commands=%zu components=%zu "
                "anchors=%zu/%zu/%zu guest=%dx%d "
                "policy=relative-edge-groups\n",
                hud_commands.size(),
                component_count,
                anchor_counts[0],
                anchor_counts[1],
                anchor_counts[2],
                draw_list.display_width,
                draw_list.display_height);
        }
    }
    return placements;
}

struct FrameInputResources {
    ComPtr<ID3D11Buffer> vertex_buffer;
    UINT vertex_buffer_bytes{};
    ComPtr<ID3D11Buffer> material_buffer;
    ComPtr<ID3D11ShaderResourceView> material_view;
    UINT material_buffer_bytes{};
    std::array<ComPtr<ID3D11Buffer>, 3> constant_buffers;
    ComPtr<ID3D11Texture2D> vram_texture;
    ComPtr<ID3D11ShaderResourceView> vram_view;
};

struct BaseResources {
    bool ready;
    bool software_adapter;
    ComPtr<ID3D11Device> device;
    ComPtr<ID3D11DeviceContext> context;
    ComPtr<ID3D11VertexShader> vertex_shader;
    ComPtr<ID3D11PixelShader> pixel_shader;
    ComPtr<ID3D11InputLayout> input_layout;
    ComPtr<ID3D11RasterizerState> rasterizer;
    std::array<ComPtr<ID3D11BlendState>, 7> blend_states;
    ComPtr<ID3D11DepthStencilState> depth_states[2][2][2][2];
    // Mutable inputs can follow the full sixteen-slot asynchronous readback
    // cadence. The low-latency authored path keeps four slots hot; a deeper
    // queue expands to the staging slot's unique resource set. In either case,
    // a slot cannot be uploaded again until its image has been drained.
    std::array<
        FrameInputResources,
        world_gpu_async_readback_image_capacity> frame_inputs;
    ComPtr<ID3D11Texture2D> color_texture;
    ComPtr<ID3D11RenderTargetView> color_view;
    ComPtr<ID3D11Texture2D> depth_texture;
    ComPtr<ID3D11DepthStencilView> depth_view;
    // Four slots retain two complete midpoint/actual pairs. The renderer can
    // therefore absorb a GPU tail longer than one 30 Hz guest interval without
    // synchronizing the producer thread at Map. Keeping an even slot count is
    // essential: output is consumed in midpoint/actual pairs, so odd-sized
    // rings would eventually combine images from different authored states.
    std::array<
        ComPtr<ID3D11Texture2D>,
        world_gpu_readback_pair_delay * 2> staging_textures;
    std::array<
        ComPtr<ID3D11Query>,
        world_gpu_readback_pair_delay * 2> staging_completion_queries;
    std::array<
        ComPtr<ID3D11Texture2D>,
        world_gpu_async_readback_pair_capacity * 2>
        async_staging_textures;
    std::array<
        ComPtr<ID3D11Query>,
        world_gpu_async_readback_pair_capacity * 2>
        async_completion_queries;
    UINT staging_write_index;
    UINT staging_fill_count;
    UINT async_staging_read_index;
    UINT async_staging_write_index;
    UINT async_staging_count;
    std::vector<std::uint8_t> staging_warmup_output;
    bool replacement_pack_attempted;
    ComPtr<ID3D11Texture2D> replacement_texture;
    ComPtr<ID3D11ShaderResourceView> replacement_view;
    ComPtr<ID3D11SamplerState> replacement_sampler;
    std::unordered_map<
        std::uint64_t,
        std::vector<ReplacementEntry>> replacement_entries;
    std::vector<WorldTextureUpload> replacement_uploads;
    std::unordered_set<
        ReplacementIdentity,
        ReplacementIdentityHash> replacement_hit_keys;
    std::unordered_map<
        ReplacementSignature,
        CachedReplacementResolution,
        ReplacementSignatureHash> replacement_resolution_cache;
    std::vector<std::uint16_t> replacement_vram_shadow;
    std::array<std::uint64_t, 16 * 8> replacement_block_revisions{};
    std::uint64_t replacement_revision{1};
    std::unordered_set<std::uint64_t> dumped_replacement_keys;
    std::filesystem::path replacement_dump_directory;
    std::uint32_t replacement_width;
    std::uint32_t replacement_height;
    std::uint64_t replacement_resolves;
    std::uint64_t replacement_hits;
    std::size_t replacement_entry_count;
    std::uint32_t output_width;
    std::uint32_t output_height;
};

struct LooseReplacementImage {
    std::string name;
    std::uint32_t width{};
    std::uint32_t height{};
    std::vector<std::uint8_t> rgba;
    std::uint32_t packed_x{};
    std::uint32_t packed_y{};
};

struct PendingReplacementEntry {
    std::uint64_t key{};
    std::uint64_t palette_key{};
    std::string image;
    std::int32_t x{};
    std::int32_t y{};
    std::int32_t width{};
    std::int32_t height{};
    std::int32_t source_width{};
    std::int32_t source_height{};
    std::int32_t pixel_mode{};
    std::uint32_t mode{replacement_mode_rgb};
    bool color_fit{true};
};

std::uint32_t read_little_u32(
    const std::vector<std::uint8_t>& bytes,
    std::size_t offset
) {
    if (offset + 4 > bytes.size())
        return 0;
    return
        static_cast<std::uint32_t>(bytes[offset]) |
        static_cast<std::uint32_t>(bytes[offset + 1]) << 8 |
        static_cast<std::uint32_t>(bytes[offset + 2]) << 16 |
        static_cast<std::uint32_t>(bytes[offset + 3]) << 24;
}

std::uint8_t dds_channel(
    std::uint32_t pixel,
    std::uint32_t mask,
    std::uint8_t absent
) {
    if (mask == 0)
        return absent;
    unsigned shift = 0;
    while (((mask >> shift) & 1U) == 0U)
        ++shift;
    const std::uint32_t maximum = mask >> shift;
    const std::uint32_t value = (pixel & mask) >> shift;
    return static_cast<std::uint8_t>(
        (value * 255U + maximum / 2U) / maximum);
}

std::optional<LooseReplacementImage> read_loose_dds(
    const std::filesystem::path& path,
    const std::string& name
) {
    std::ifstream stream(path, std::ios::binary);
    if (!stream)
        return std::nullopt;
    std::vector<std::uint8_t> bytes{
        std::istreambuf_iterator<char>(stream),
        std::istreambuf_iterator<char>()};
    if (
        bytes.size() < 128 ||
        read_little_u32(bytes, 0) != 0x20534444U ||
        read_little_u32(bytes, 4) != 124U ||
        read_little_u32(bytes, 76) != 32U
    )
        return std::nullopt;
    const std::uint32_t height = read_little_u32(bytes, 12);
    const std::uint32_t width = read_little_u32(bytes, 16);
    const std::uint32_t pitch = read_little_u32(bytes, 20);
    const std::uint32_t four_cc = read_little_u32(bytes, 84);
    const std::uint32_t bits = read_little_u32(bytes, 88);
    const std::uint32_t red_mask = read_little_u32(bytes, 92);
    const std::uint32_t green_mask = read_little_u32(bytes, 96);
    const std::uint32_t blue_mask = read_little_u32(bytes, 100);
    const std::uint32_t alpha_mask = read_little_u32(bytes, 104);
    if (
        width == 0 || height == 0 || width > 4096 || height > 4096 ||
        four_cc != 0 || bits != 32 || pitch < width * 4 ||
        bytes.size() < 128ULL + static_cast<std::uint64_t>(pitch) * height
    )
        return std::nullopt;
    LooseReplacementImage image{name, width, height};
    image.rgba.resize(static_cast<std::size_t>(width) * height * 4);
    for (std::uint32_t y = 0; y < height; ++y) {
        for (std::uint32_t x = 0; x < width; ++x) {
            const std::uint32_t pixel = read_little_u32(
                bytes, 128ULL + static_cast<std::size_t>(y) * pitch + x * 4);
            const std::size_t output =
                (static_cast<std::size_t>(y) * width + x) * 4;
            image.rgba[output] = dds_channel(pixel, red_mask, 0);
            image.rgba[output + 1] = dds_channel(pixel, green_mask, 0);
            image.rgba[output + 2] = dds_channel(pixel, blue_mask, 0);
            image.rgba[output + 3] = dds_channel(pixel, alpha_mask, 255);
        }
    }
    return image;
}

std::optional<std::string> json_string_field(
    const std::string& object,
    const char* key
) {
    const std::regex expression{
        std::string{"\""} + key + "\"\\s*:\\s*\"([^\"]*)\""};
    std::smatch match;
    if (!std::regex_search(object, match, expression))
        return std::nullopt;
    return match[1].str();
}

std::optional<std::int32_t> json_integer_field(
    const std::string& object,
    const char* key
) {
    const std::regex expression{
        std::string{"\""} + key + "\"\\s*:\\s*(-?[0-9]+)"};
    std::smatch match;
    if (!std::regex_search(object, match, expression))
        return std::nullopt;
    try {
        return static_cast<std::int32_t>(std::stoll(match[1].str()));
    } catch (...) {
        return std::nullopt;
    }
}

std::vector<std::string> json_entry_objects(const std::string& document) {
    std::vector<std::string> result;
    const std::size_t entries = document.find("\"entries\"");
    const std::size_t begin = entries == std::string::npos
        ? std::string::npos
        : document.find('[', entries);
    if (begin == std::string::npos)
        return result;
    bool quoted = false;
    bool escaped = false;
    int object_depth = 0;
    std::size_t object_begin = std::string::npos;
    for (std::size_t index = begin + 1; index < document.size(); ++index) {
        const char value = document[index];
        if (quoted) {
            if (escaped)
                escaped = false;
            else if (value == '\\')
                escaped = true;
            else if (value == '"')
                quoted = false;
            continue;
        }
        if (value == '"') {
            quoted = true;
            continue;
        }
        if (value == '{') {
            if (object_depth++ == 0)
                object_begin = index;
        } else if (value == '}') {
            if (object_depth <= 0)
                return {};
            if (--object_depth == 0 && object_begin != std::string::npos)
                result.push_back(document.substr(
                    object_begin, index - object_begin + 1));
        } else if (value == ']' && object_depth == 0) {
            return result;
        }
    }
    return {};
}

bool path_is_below(
    const std::filesystem::path& root,
    const std::filesystem::path& candidate
) {
    auto root_part = root.begin();
    auto candidate_part = candidate.begin();
    while (root_part != root.end()) {
        if (
            candidate_part == candidate.end() ||
            root_part->wstring() != candidate_part->wstring()
        )
            return false;
        ++root_part;
        ++candidate_part;
    }
    return true;
}

bool layout_replacement_images(
    std::vector<LooseReplacementImage>* images,
    std::uint32_t size,
    std::uint32_t padding
) {
    std::vector<std::size_t> order(images->size());
    for (std::size_t index = 0; index < order.size(); ++index)
        order[index] = index;
    std::sort(order.begin(), order.end(), [&] (std::size_t left, std::size_t right) {
        const auto& a = (*images)[left];
        const auto& b = (*images)[right];
        if (a.height != b.height)
            return a.height > b.height;
        if (a.width != b.width)
            return a.width > b.width;
        return a.name < b.name;
    });
    std::uint32_t x = 0;
    std::uint32_t y = 0;
    std::uint32_t row_height = 0;
    for (const std::size_t index : order) {
        auto& image = (*images)[index];
        const std::uint32_t packed_width = image.width + padding * 2;
        const std::uint32_t packed_height = image.height + padding * 2;
        if (packed_width > size || packed_height > size)
            return false;
        if (x + packed_width > size) {
            x = 0;
            y += row_height;
            row_height = 0;
        }
        if (y + packed_height > size)
            return false;
        image.packed_x = x + padding;
        image.packed_y = y + padding;
        x += packed_width;
        row_height = (std::max)(row_height, packed_height);
    }
    return true;
}

void configure_replacement_pack(BaseResources* resources) noexcept {
    if (resources->replacement_pack_attempted)
        return;
    resources->replacement_pack_attempted = true;
    try {
        std::filesystem::path directory;
        if (const char* override_directory =
                std::getenv("OPENGT_TEXTURE_PACK_DIR")) {
            if (*override_directory != '\0')
                directory = override_directory;
        }
        if (directory.empty())
            directory = std::filesystem::current_path() /
                "mods" / "enhanced_textures_4x";
        directory = std::filesystem::absolute(directory).lexically_normal();
        const std::filesystem::path manifest_path =
            directory / "manifest.json";
        std::ifstream manifest_stream(manifest_path, std::ios::binary);
        if (!manifest_stream) {
            std::fprintf(
                stderr,
                "[TexturePack] no external DDS pack at %s\n",
                directory.string().c_str());
            return;
        }
        const std::string manifest{
            std::istreambuf_iterator<char>(manifest_stream),
            std::istreambuf_iterator<char>()};
        const auto format = json_integer_field(manifest, "format");
        if (!format || (*format != 5 && *format != 6 && *format != 7))
            throw std::runtime_error(
                "manifest format must be individual-upload DDS format 5, 6, or 7");
        const std::int32_t manifest_scale =
            json_integer_field(manifest, "scale").value_or(2);
        if (manifest_scale <= 0)
            throw std::runtime_error("manifest scale must be positive");

        std::vector<PendingReplacementEntry> pending;
        for (const std::string& object : json_entry_objects(manifest)) {
            const auto key_text = json_string_field(object, "key");
            const auto image = json_string_field(object, "image");
            const auto x = json_integer_field(object, "x");
            const auto y = json_integer_field(object, "y");
            const auto width = json_integer_field(object, "width");
            const auto height = json_integer_field(object, "height");
            const auto pixel_mode = json_integer_field(object, "pixelMode");
            const auto palette_key_text =
                json_string_field(object, "paletteKey");
            const auto replacement_mode_text =
                json_string_field(object, "replacementMode");
            if (
                !key_text || !image || !x || !y || !width || !height ||
                !pixel_mode || (*pixel_mode != 0 && *pixel_mode != 1)
            )
                throw std::runtime_error("manifest entry is incomplete");
            std::size_t parsed = 0;
            const std::uint64_t key = std::stoull(*key_text, &parsed, 16);
            if (parsed != key_text->size())
                throw std::runtime_error("manifest key is not hexadecimal");
            std::uint64_t palette_key = 0;
            if (palette_key_text) {
                parsed = 0;
                palette_key = std::stoull(*palette_key_text, &parsed, 16);
                if (parsed != palette_key_text->size() || palette_key == 0)
                    throw std::runtime_error(
                        "manifest palette key is not nonzero hexadecimal");
            }
            const std::int32_t color_fit = json_integer_field(
                object, "colorFit").value_or(palette_key == 0 ? 1 : 0);
            if (color_fit != 0 && color_fit != 1)
                throw std::runtime_error(
                    "manifest colorFit must be zero or one");
            std::uint32_t replacement_mode = replacement_mode_rgb;
            if (replacement_mode_text) {
                if (*replacement_mode_text == "rgb")
                    replacement_mode = replacement_mode_rgb;
                else if (*replacement_mode_text == "paletteDetail")
                    replacement_mode = replacement_mode_palette_detail;
                else
                    throw std::runtime_error(
                        "manifest replacementMode is invalid");
            } else if (*format == 7) {
                throw std::runtime_error(
                    "format 7 manifest entry has no replacementMode");
            }
            if (
                replacement_mode == replacement_mode_palette_detail &&
                (palette_key != 0 || color_fit != 0)
            )
                throw std::runtime_error(
                    "paletteDetail must use the live GT2 palette");
            pending.push_back(PendingReplacementEntry{
                key, palette_key, *image, *x, *y, *width, *height,
                json_integer_field(object, "sourceWidth")
                    .value_or(*width / manifest_scale),
                json_integer_field(object, "sourceHeight")
                    .value_or(*height / manifest_scale),
                *pixel_mode, replacement_mode, color_fit != 0});
        }
        if (pending.empty())
            throw std::runtime_error("manifest has no texture entries");

        std::vector<LooseReplacementImage> images;
        std::unordered_map<std::string, std::size_t> image_indices;
        for (const auto& entry : pending) {
            if (image_indices.find(entry.image) != image_indices.end())
                continue;
            std::filesystem::path relative{entry.image};
            if (relative.is_absolute())
                throw std::runtime_error("manifest image path is absolute");
            const std::filesystem::path path =
                std::filesystem::absolute(directory / relative)
                    .lexically_normal();
            if (!path_is_below(directory, path))
                throw std::runtime_error("manifest image escapes pack directory");
            auto image = read_loose_dds(path, entry.image);
            if (!image)
                throw std::runtime_error(
                    "invalid uncompressed RGBA DDS: " + entry.image);
            image_indices.emplace(entry.image, images.size());
            images.push_back(std::move(*image));
        }

        constexpr std::uint32_t padding = 2;
        std::uint32_t atlas_size = 1024;
        while (
            atlas_size <= 16384 &&
            !layout_replacement_images(&images, atlas_size, padding)
        )
            atlas_size *= 2;
        if (atlas_size > 16384)
            throw std::runtime_error("replacement images exceed 16384 atlas");
        std::vector<std::uint8_t> atlas(
            static_cast<std::size_t>(atlas_size) * atlas_size * 4);
        for (const auto& image : images) {
            for (std::int32_t y = -static_cast<std::int32_t>(padding);
                 y < static_cast<std::int32_t>(image.height + padding);
                 ++y) {
                for (std::int32_t x = -static_cast<std::int32_t>(padding);
                     x < static_cast<std::int32_t>(image.width + padding);
                     ++x) {
                    const std::uint32_t source_x = static_cast<std::uint32_t>(
                        std::clamp(x, 0, static_cast<std::int32_t>(image.width) - 1));
                    const std::uint32_t source_y = static_cast<std::uint32_t>(
                        std::clamp(y, 0, static_cast<std::int32_t>(image.height) - 1));
                    const std::size_t source =
                        (static_cast<std::size_t>(source_y) * image.width +
                            source_x) * 4;
                    const std::size_t target =
                        (static_cast<std::size_t>(
                            static_cast<std::int32_t>(image.packed_y) + y) *
                            atlas_size +
                            static_cast<std::int32_t>(image.packed_x) + x) * 4;
                    std::memcpy(atlas.data() + target,
                        image.rgba.data() + source, 4);
                }
            }
        }
        for (const auto& entry : pending) {
            const auto& image = images[image_indices.at(entry.image)];
            if (
                entry.x < 0 || entry.y < 0 ||
                entry.width <= 0 || entry.height <= 0 ||
                static_cast<std::uint64_t>(entry.x) + entry.width > image.width ||
                static_cast<std::uint64_t>(entry.y) + entry.height > image.height
            )
                throw std::runtime_error("manifest crop is outside its DDS");
            if (
                entry.source_width <= 0 || entry.source_height <= 0 ||
                entry.width % entry.source_width != 0 ||
                entry.height % entry.source_height != 0 ||
                entry.width / entry.source_width !=
                    entry.height / entry.source_height
            )
                throw std::runtime_error(
                    "manifest replacement has a non-integer source scale");
            ReplacementEntry replacement{};
            replacement.rect = ReplacementRect{
                image.packed_x + static_cast<std::uint32_t>(entry.x),
                image.packed_y + static_cast<std::uint32_t>(entry.y),
                static_cast<std::uint32_t>(entry.width),
                static_cast<std::uint32_t>(entry.height)};
            replacement.source_width =
                static_cast<std::uint32_t>(entry.source_width);
            replacement.source_height =
                static_cast<std::uint32_t>(entry.source_height);
            replacement.pixel_mode =
                static_cast<std::uint32_t>(entry.pixel_mode);
            replacement.palette_key = entry.palette_key;
            replacement.mode = entry.mode;
            replacement.color_fit = entry.color_fit;
            const std::uint32_t source_scale = static_cast<std::uint32_t>(
                entry.width / entry.source_width);
            if (replacement.color_fit) {
                replacement.canonical_rgb.resize(
                    static_cast<std::size_t>(replacement.source_width) *
                    replacement.source_height * 3);
                for (std::uint32_t source_y = 0;
                     source_y < replacement.source_height;
                     ++source_y) {
                    for (std::uint32_t source_x = 0;
                         source_x < replacement.source_width;
                         ++source_x) {
                        std::uint32_t sums[3]{};
                        for (std::uint32_t dy = 0; dy < source_scale; ++dy) {
                            for (std::uint32_t dx = 0; dx < source_scale; ++dx) {
                                const std::size_t pixel = (
                                    static_cast<std::size_t>(
                                        entry.y + source_y * source_scale + dy) *
                                        image.width +
                                    entry.x + source_x * source_scale + dx) * 4;
                                for (int channel = 0; channel < 3; ++channel)
                                    sums[channel] += image.rgba[pixel + channel];
                            }
                        }
                        const std::size_t output = (
                            static_cast<std::size_t>(source_y) *
                                replacement.source_width + source_x) * 3;
                        const std::uint32_t samples = source_scale * source_scale;
                        for (int channel = 0; channel < 3; ++channel)
                            replacement.canonical_rgb[output + channel] =
                                static_cast<std::uint8_t>(
                                    (sums[channel] + samples / 2) / samples);
                    }
                }
            }
            auto& variants = resources->replacement_entries[entry.key];
            if (std::any_of(
                    variants.begin(), variants.end(),
                    [&] (const ReplacementEntry& existing) {
                        return existing.palette_key == entry.palette_key;
                    }))
                throw std::runtime_error(
                    "manifest has a duplicate bitmap/palette identity");
            variants.push_back(std::move(replacement));
            ++resources->replacement_entry_count;
        }

        D3D11_TEXTURE2D_DESC texture_description{};
        texture_description.Width = atlas_size;
        texture_description.Height = atlas_size;
        texture_description.MipLevels = 1;
        texture_description.ArraySize = 1;
        texture_description.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
        texture_description.SampleDesc.Count = 1;
        texture_description.Usage = D3D11_USAGE_IMMUTABLE;
        texture_description.BindFlags = D3D11_BIND_SHADER_RESOURCE;
        const D3D11_SUBRESOURCE_DATA texture_data{
            atlas.data(), atlas_size * 4, 0};
        if (
            FAILED(resources->device->CreateTexture2D(
                &texture_description,
                &texture_data,
                resources->replacement_texture.GetAddressOf())) ||
            FAILED(resources->device->CreateShaderResourceView(
                resources->replacement_texture.Get(),
                nullptr,
                resources->replacement_view.GetAddressOf()))
        )
            throw std::runtime_error("could not upload replacement atlas");
        D3D11_SAMPLER_DESC sampler_description{};
        sampler_description.Filter = D3D11_FILTER_MIN_MAG_LINEAR_MIP_POINT;
        sampler_description.AddressU = D3D11_TEXTURE_ADDRESS_CLAMP;
        sampler_description.AddressV = D3D11_TEXTURE_ADDRESS_CLAMP;
        sampler_description.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
        sampler_description.MaxLOD = D3D11_FLOAT32_MAX;
        if (FAILED(resources->device->CreateSamplerState(
                &sampler_description,
                resources->replacement_sampler.GetAddressOf())))
            throw std::runtime_error("could not create replacement sampler");
        resources->replacement_width = atlas_size;
        resources->replacement_height = atlas_size;
        resources->replacement_hit_keys.clear();
        resources->replacement_resolves = 0;
        resources->replacement_hits = 0;
        std::fprintf(
            stderr,
            "[TexturePack] loaded %zu individual DDS assets / %zu uploads from %s "
            "gpu-cache=%ux%u\n",
            images.size(),
            resources->replacement_entry_count,
            directory.string().c_str(),
            atlas_size,
            atlas_size);
    } catch (const std::exception& error) {
        resources->replacement_texture.Reset();
        resources->replacement_view.Reset();
        resources->replacement_sampler.Reset();
        resources->replacement_entries.clear();
        resources->replacement_entry_count = 0;
        resources->replacement_width = 0;
        resources->replacement_height = 0;
        std::fprintf(stderr, "[TexturePack] rejected pack: %s\n", error.what());
    }
}

std::uint8_t expand_ps1_five(std::uint16_t value) {
    return static_cast<std::uint8_t>((value << 3) | (value >> 2));
}

std::uint16_t read_vram_word(
    const std::uint16_t* vram,
    std::int32_t x,
    std::int32_t y
) {
    return vram[
        static_cast<std::size_t>(y & 511) * 1024 +
        static_cast<std::size_t>(x & 1023)];
}

std::uint16_t material_texture_word_impl(
    const std::uint16_t* vram,
    const WorldMaterial& material,
    std::int32_t raw_u,
    std::int32_t raw_v,
    bool apply_texture_window,
    std::uint8_t* index_output = nullptr
) {
    const std::int32_t u = apply_texture_window
        ? ((raw_u & ~(material.texture_mask_x * 8)) |
            ((material.texture_offset_x & material.texture_mask_x) * 8)) & 255
        : raw_u & 255;
    const std::int32_t v = apply_texture_window
        ? ((raw_v & ~(material.texture_mask_y * 8)) |
            ((material.texture_offset_y & material.texture_mask_y) * 8)) & 255
        : raw_v & 255;
    const std::int32_t page_x = (material.texture_page & 15) * 64;
    const std::int32_t page_y = ((material.texture_page >> 4) & 1) * 256;
    const std::int32_t mode = (material.texture_page >> 7) & 3;
    const std::int32_t clut_x = (material.clut & 63) * 16;
    const std::int32_t clut_y = (material.clut >> 6) & 511;
    if (mode == 0) {
        const std::uint16_t packed =
            read_vram_word(vram, page_x + (u >> 2), page_y + v);
        const std::uint8_t index = static_cast<std::uint8_t>(
            (packed >> ((u & 3) * 4)) & 15);
        if (index_output != nullptr)
            *index_output = index;
        return read_vram_word(vram, clut_x + index, clut_y);
    }
    if (mode == 1) {
        const std::uint16_t packed =
            read_vram_word(vram, page_x + (u >> 1), page_y + v);
        const std::uint8_t index = static_cast<std::uint8_t>(
            (packed >> ((u & 1) * 8)) & 255);
        if (index_output != nullptr)
            *index_output = index;
        return read_vram_word(vram, clut_x + index, clut_y);
    }
    return read_vram_word(vram, page_x + u, page_y + v);
}

std::uint16_t material_texture_word(
    const std::uint16_t* vram,
    const WorldMaterial& material,
    std::int32_t raw_u,
    std::int32_t raw_v,
    std::uint8_t* index_output = nullptr
) {
    return material_texture_word_impl(
        vram, material, raw_u, raw_v, true, index_output);
}

std::uint16_t raw_material_texture_word(
    const std::uint16_t* vram,
    const WorldMaterial& material,
    std::int32_t raw_u,
    std::int32_t raw_v,
    std::uint8_t* index_output = nullptr
) {
    return material_texture_word_impl(
        vram, material, raw_u, raw_v, false, index_output);
}

void update_replacement_vram_revisions(
    BaseResources* resources,
    const std::uint16_t* vram
) {
    constexpr std::size_t word_count = 1024U * 512U;
    if (resources->replacement_vram_shadow.size() != word_count) {
        resources->replacement_vram_shadow.assign(vram, vram + word_count);
        const std::uint64_t revision = ++resources->replacement_revision;
        resources->replacement_block_revisions.fill(revision);
        resources->replacement_resolution_cache.clear();
        return;
    }
    for (std::int32_t block_y = 0; block_y < 8; ++block_y) {
        for (std::int32_t block_x = 0; block_x < 16; ++block_x) {
            bool changed = false;
            for (std::int32_t row = 0; row < 64 && !changed; ++row) {
                const std::size_t offset =
                    static_cast<std::size_t>(block_y * 64 + row) * 1024 +
                    block_x * 64;
                changed = std::memcmp(
                    resources->replacement_vram_shadow.data() + offset,
                    vram + offset,
                    64 * sizeof(std::uint16_t)) != 0;
            }
            if (!changed)
                continue;
            const std::uint64_t revision = ++resources->replacement_revision;
            resources->replacement_block_revisions[
                block_y * 16 + block_x] = revision;
            for (std::int32_t row = 0; row < 64; ++row) {
                const std::size_t offset =
                    static_cast<std::size_t>(block_y * 64 + row) * 1024 +
                    block_x * 64;
                std::memcpy(
                    resources->replacement_vram_shadow.data() + offset,
                    vram + offset,
                    64 * sizeof(std::uint16_t));
            }
        }
    }
}

std::uint64_t replacement_texture_revision(
    const BaseResources& resources,
    const WorldMaterial& material
) {
    const std::int32_t mode = (material.texture_page >> 7) & 3;
    const std::int32_t page_x = (material.texture_page & 15) * 64;
    const std::int32_t page_y = ((material.texture_page >> 4) & 1) * 256;
    const std::int32_t word_width = mode == 0 ? 64 : mode == 1 ? 128 : 256;
    std::uint64_t revision = 0;
    for (std::int32_t block_y = page_y >> 6;
         block_y <= (page_y + 255) >> 6;
         ++block_y) {
        for (std::int32_t block_x = page_x >> 6;
             block_x <= (page_x + word_width - 1) >> 6;
             ++block_x) {
            revision = (std::max)(
                revision,
                resources.replacement_block_revisions[
                    (block_y & 7) * 16 + (block_x & 15)]);
        }
    }
    return revision;
}

void fit_replacement_palette(
    ReplacementResolution* resolution,
    const ReplacementEntry& entry,
    const WorldMaterial& material,
    const std::uint16_t* vram
) {
    if (
        entry.source_width != resolution->source_width ||
        entry.source_height != resolution->source_height ||
        entry.canonical_rgb.size() !=
            static_cast<std::size_t>(entry.source_width) *
                entry.source_height * 3
    )
        return;
    const std::size_t pixels =
        static_cast<std::size_t>(entry.source_width) * entry.source_height;
    const std::size_t stride = (std::max<std::size_t>)(1, pixels / 64);
    double sx[3]{}, sy[3]{}, sxx[3]{}, sxy[3]{};
    std::uint32_t count = 0;
    for (std::size_t pixel = 0; pixel < pixels; pixel += stride) {
        const std::int32_t raw_u = resolution->minimum_u +
            static_cast<std::int32_t>(pixel % entry.source_width);
        const std::int32_t raw_v = resolution->minimum_v +
            static_cast<std::int32_t>(pixel / entry.source_width);
        const std::uint16_t live = raw_material_texture_word(
            vram, material, raw_u, raw_v);
        if (live == 0)
            continue;
        const double source[3]{
            entry.canonical_rgb[pixel * 3] / 255.0,
            entry.canonical_rgb[pixel * 3 + 1] / 255.0,
            entry.canonical_rgb[pixel * 3 + 2] / 255.0};
        const double target[3]{
            expand_ps1_five(live & 31) / 255.0,
            expand_ps1_five((live >> 5) & 31) / 255.0,
            expand_ps1_five((live >> 10) & 31) / 255.0};
        for (int channel = 0; channel < 3; ++channel) {
            sx[channel] += source[channel];
            sy[channel] += target[channel];
            sxx[channel] += source[channel] * source[channel];
            sxy[channel] += source[channel] * target[channel];
        }
        ++count;
    }
    if (count == 0)
        return;
    for (int channel = 0; channel < 3; ++channel) {
        const double denominator = count * sxx[channel] -
            sx[channel] * sx[channel];
        double scale = 1.0;
        double bias = (sy[channel] - sx[channel]) / count;
        if (std::abs(denominator) >= 1.0e-9) {
            scale = (count * sxy[channel] -
                sx[channel] * sy[channel]) / denominator;
            bias = (sy[channel] - scale * sx[channel]) / count;
        }
        resolution->color_scale[channel] = static_cast<float>(
            std::clamp(scale, 0.0, 4.0));
        resolution->color_bias[channel] = static_cast<float>(
            std::clamp(bias, -1.0, 1.0));
    }
}

ReplacementResolution resolve_texture_replacement(
    BaseResources* resources,
    const WorldDrawCommand& command,
    const WorldMaterial& material,
    const std::uint16_t* vram,
    bool enabled,
    std::unordered_map<
        ReplacementSignature,
        ReplacementResolution,
        ReplacementSignatureHash>* cache
) {
    ReplacementResolution result{};
    if (
        (material.primitive_flags & textured_flag) == 0 ||
        (material.primitive_flags & world_primitive_screen_space_flag) != 0
    )
        return result;
    const float minimum_u_float = (std::min)({
        command.vertices[0].u,
        command.vertices[1].u,
        command.vertices[2].u});
    const float minimum_v_float = (std::min)({
        command.vertices[0].v,
        command.vertices[1].v,
        command.vertices[2].v});
    const float maximum_u_float = (std::max)({
        command.vertices[0].u,
        command.vertices[1].u,
        command.vertices[2].u});
    const float maximum_v_float = (std::max)({
        command.vertices[0].v,
        command.vertices[1].v,
        command.vertices[2].v});
    const std::int32_t authored_minimum_u =
        static_cast<std::int32_t>(std::floor(minimum_u_float));
    const std::int32_t authored_minimum_v =
        static_cast<std::int32_t>(std::floor(minimum_v_float));
    const std::int32_t authored_maximum_u =
        static_cast<std::int32_t>(std::ceil(maximum_u_float));
    const std::int32_t authored_maximum_v =
        static_cast<std::int32_t>(std::ceil(maximum_v_float));
    const std::int32_t mode = (material.texture_page >> 7) & 3;
    if (
        authored_minimum_u < 0 || authored_minimum_v < 0 ||
        authored_maximum_u > 255 || authored_maximum_v > 255 ||
        authored_minimum_u > authored_maximum_u ||
        authored_minimum_v > authored_maximum_v ||
        // Direct-color pages commonly contain live framebuffer effects.
        // Upscale stable authored 4/8-bit texture assets first; never turn
        // dynamic render targets into an unbounded replacement inventory.
        mode >= 2
    )
        return result;
    ReplacementSignature signature{};
    signature.texture_page = material.texture_page;
    signature.clut = material.clut;
    signature.mask_x = material.texture_mask_x;
    signature.mask_y = material.texture_mask_y;
    signature.offset_x = material.texture_offset_x;
    signature.offset_y = material.texture_offset_y;
    signature.minimum_u = static_cast<std::int16_t>(authored_minimum_u);
    signature.minimum_v = static_cast<std::int16_t>(authored_minimum_v);
    signature.maximum_u = static_cast<std::int16_t>(authored_maximum_u);
    signature.maximum_v = static_cast<std::int16_t>(authored_maximum_v);
    if (const auto found = cache->find(signature); found != cache->end())
        return found->second;
    const auto map_coordinate = [] (
        float coordinate,
        std::int32_t mask,
        std::int32_t offset
    ) {
        const std::int32_t integer =
            static_cast<std::int32_t>(std::floor(coordinate));
        return static_cast<float>(
            ((integer & ~(mask * 8)) | ((offset & mask) * 8)) & 255) +
            (coordinate - std::floor(coordinate));
    };
    float mapped_minimum_u = 256.0F;
    float mapped_minimum_v = 256.0F;
    float mapped_maximum_u = 0.0F;
    float mapped_maximum_v = 0.0F;
    for (const auto& vertex : command.vertices) {
        const float u = map_coordinate(
            vertex.u, material.texture_mask_x, material.texture_offset_x);
        const float v = map_coordinate(
            vertex.v, material.texture_mask_y, material.texture_offset_y);
        mapped_minimum_u = (std::min)(mapped_minimum_u, u);
        mapped_minimum_v = (std::min)(mapped_minimum_v, v);
        mapped_maximum_u = (std::max)(mapped_maximum_u, u);
        mapped_maximum_v = (std::max)(mapped_maximum_v, v);
    }
    const std::int32_t page_x = (material.texture_page & 15) * 64;
    const std::int32_t page_y =
        ((material.texture_page >> 4) & 1) * 256;
    const std::int32_t pixels_per_word = mode == 0 ? 4 : 2;
    std::uint64_t smallest_area =
        (std::numeric_limits<std::uint64_t>::max)();
    const ReplacementEntry* selected = nullptr;
    bool selected_exact_palette = false;
    for (const auto& upload : resources->replacement_uploads) {
        const auto entries = resources->replacement_entries.find(upload.key);
        if (
            entries == resources->replacement_entries.end() ||
            upload.word_width <= 0 || upload.height <= 0
        )
            continue;
        const std::int32_t minimum_u =
            (upload.x - page_x) * pixels_per_word;
        const std::int32_t minimum_v = upload.y - page_y;
        const std::int32_t width = upload.word_width * pixels_per_word;
        const std::int32_t height = upload.height;
        if (
            minimum_u < 0 || minimum_v < 0 ||
            minimum_u + width > 256 || minimum_v + height > 256 ||
            mapped_minimum_u < minimum_u ||
            mapped_minimum_v < minimum_v ||
            mapped_maximum_u >= minimum_u + width ||
            mapped_maximum_v >= minimum_v + height
        )
            continue;
        const std::uint64_t area =
            static_cast<std::uint64_t>(width) * height;
        for (const ReplacementEntry& entry : entries->second) {
            if (
                entry.pixel_mode != static_cast<std::uint32_t>(mode) ||
                entry.source_width != static_cast<std::uint32_t>(width) ||
                entry.source_height != static_cast<std::uint32_t>(height)
            )
                continue;
            const bool exact_palette = entry.palette_key != 0;
            if (
                exact_palette &&
                std::none_of(
                    resources->replacement_uploads.begin(),
                    resources->replacement_uploads.end(),
                    [&] (const WorldTextureUpload& palette_upload) {
                        return world_texture_upload_contains_clut(
                            palette_upload, entry.palette_key, material.clut);
                    })
            )
                continue;
            if (
                selected != nullptr &&
                ((selected_exact_palette && !exact_palette) ||
                 (selected_exact_palette == exact_palette &&
                    area >= smallest_area))
            )
                continue;
            smallest_area = area;
            selected = &entry;
            selected_exact_palette = exact_palette;
            result.minimum_u = minimum_u;
            result.minimum_v = minimum_v;
            result.source_width = static_cast<std::uint32_t>(width);
            result.source_height = static_cast<std::uint32_t>(height);
            result.key = upload.key;
            result.palette_key = entry.palette_key;
            result.mode = entry.mode;
            result.rect = entry.rect;
        }
    }
    ++resources->replacement_resolves;
    if (enabled && selected != nullptr) {
        if (
            selected->mode == replacement_mode_rgb &&
            selected->color_fit
        )
            fit_replacement_palette(&result, *selected, material, vram);
        ++resources->replacement_hits;
        const auto [_, first_hit] = resources->replacement_hit_keys.insert(
            ReplacementIdentity{result.key, result.palette_key});
        if (
            first_hit &&
            result.mode == replacement_mode_palette_detail
        ) {
            std::fprintf(
                stderr,
                "[TexturePack] matched palette-native car bitmap=%016llx\n",
                static_cast<unsigned long long>(result.key));
        } else if (first_hit && result.palette_key != 0) {
            std::fprintf(
                stderr,
                "[TexturePack] matched exact car paint bitmap=%016llx "
                "palette=%016llx\n",
                static_cast<unsigned long long>(result.key),
                static_cast<unsigned long long>(result.palette_key));
        }
    } else if (!enabled) {
        result.rect = {};
    }
    if ((resources->replacement_resolves & 0xFFFFFULL) == 0) {
        std::fprintf(
            stderr,
            "[TexturePack] individual-asset hits=%llu/%llu (%.1f%%) "
            "matched-assets=%zu/%zu active-uploads=%zu\n",
            static_cast<unsigned long long>(resources->replacement_hits),
            static_cast<unsigned long long>(resources->replacement_resolves),
            100.0 * resources->replacement_hits /
            (std::max<std::uint64_t>)(1, resources->replacement_resolves),
            resources->replacement_hit_keys.size(),
            resources->replacement_entry_count,
            resources->replacement_uploads.size());
    }
    cache->emplace(signature, result);
    return result;
}

bool initialize_base(
    BaseResources* resources,
    bool software_adapter
) {
    if (
        resources->ready &&
        resources->software_adapter == software_adapter
    )
        return true;
    *resources = {};
    resources->software_adapter = software_adapter;
    D3D_FEATURE_LEVEL feature_level{};
    const D3D_DRIVER_TYPE driver = software_adapter
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
            resources->device.GetAddressOf(),
            &feature_level,
             resources->context.GetAddressOf())))
        return false;
    ComPtr<ID3DBlob> vertex_blob;
    ComPtr<ID3DBlob> pixel_blob;
    if (
        !compile_shader("VSMain", "vs_4_0", &vertex_blob) ||
        !compile_shader("PSMain", "ps_4_0", &pixel_blob)
    )
        return false;
    if (
        FAILED(resources->device->CreateVertexShader(
            vertex_blob->GetBufferPointer(),
            vertex_blob->GetBufferSize(),
            nullptr,
            resources->vertex_shader.GetAddressOf())) ||
        FAILED(resources->device->CreatePixelShader(
            pixel_blob->GetBufferPointer(),
            pixel_blob->GetBufferSize(),
            nullptr,
            resources->pixel_shader.GetAddressOf()))
    )
        return false;
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
        {
            "TEXCOORD", 2, DXGI_FORMAT_R32_UINT,
            0, 40, D3D11_INPUT_PER_VERTEX_DATA, 0,
        },
    };
    if (FAILED(resources->device->CreateInputLayout(
            elements,
            static_cast<UINT>(std::size(elements)),
            vertex_blob->GetBufferPointer(),
            vertex_blob->GetBufferSize(),
            resources->input_layout.GetAddressOf())))
        return false;
    D3D11_RASTERIZER_DESC rasterizer_description{};
    rasterizer_description.FillMode = D3D11_FILL_SOLID;
    rasterizer_description.CullMode = D3D11_CULL_NONE;
    rasterizer_description.ScissorEnable = TRUE;
    rasterizer_description.DepthClipEnable = TRUE;
    if (FAILED(resources->device->CreateRasterizerState(
            &rasterizer_description,
            resources->rasterizer.GetAddressOf())))
        return false;
    resources->blend_states[0] =
        blend_state(resources->device.Get(), -1);
    for (int mode = 0; mode < 4; ++mode)
        resources->blend_states[mode + 1] =
            blend_state(resources->device.Get(), mode);
    resources->blend_states[5] =
        blend_state(resources->device.Get(), 4);
    resources->blend_states[6] =
        blend_state(resources->device.Get(), 5);
    for (const auto& state : resources->blend_states)
        if (!state)
            return false;
    for (int enabled = 0; enabled < 2; ++enabled) {
        for (int transparent = 0; transparent < 2; ++transparent) {
            for (int check = 0; check < 2; ++check) {
                for (int set = 0; set < 2; ++set) {
                    auto& state =
                        resources->depth_states
                            [enabled][transparent][check][set];
                    state = depth_state(
                        resources->device.Get(),
                        transparent == 0,
                        check != 0,
                        set != 0,
                        enabled != 0);
                    if (!state)
                        return false;
                }
            }
        }
    }
    resources->ready = true;
    return true;
}

BaseResources& base_resources(bool software_adapter) {
    static thread_local BaseResources resources;
    initialize_base(&resources, software_adapter);
    return resources;
}

void release_readback_resources(BaseResources* resources) {
    resources->color_view.Reset();
    resources->color_texture.Reset();
    resources->depth_view.Reset();
    resources->depth_texture.Reset();
    for (auto& staging : resources->staging_textures)
        staging.Reset();
    for (auto& query : resources->staging_completion_queries)
        query.Reset();
    for (auto& staging : resources->async_staging_textures)
        staging.Reset();
    for (auto& query : resources->async_completion_queries)
        query.Reset();
    resources->staging_write_index = 0;
    resources->staging_fill_count = 0;
    resources->async_staging_read_index = 0;
    resources->async_staging_write_index = 0;
    resources->async_staging_count = 0;
    resources->staging_warmup_output.clear();
    resources->output_width = 0;
    resources->output_height = 0;
}

void release_mutable_frame_resources(FrameInputResources* frame) {
    frame->vertex_buffer.Reset();
    frame->vertex_buffer_bytes = 0;
    frame->material_view.Reset();
    frame->material_buffer.Reset();
    frame->material_buffer_bytes = 0;
    for (auto& constant_buffer : frame->constant_buffers)
        constant_buffer.Reset();
    frame->vram_view.Reset();
    frame->vram_texture.Reset();
}

void release_mutable_frame_resources(BaseResources* resources) {
    for (auto& frame : resources->frame_inputs)
        release_mutable_frame_resources(&frame);
}

void release_frame_generation(BaseResources* resources) {
    if (resources->context != nullptr) {
        resources->context->ClearState();
        resources->context->Flush();
    }
    release_readback_resources(resources);
    release_mutable_frame_resources(resources);
}

bool ensure_mutable_frame_resources(
    BaseResources* resources,
    FrameInputResources* frame,
    std::size_t vertex_count,
    std::size_t material_count
) {
    ID3D11Device* device = resources->device.Get();
    const std::size_t required_vertex_bytes =
        vertex_count * sizeof(GpuVertex);
    if (
        required_vertex_bytes >
        (std::numeric_limits<UINT>::max)()
    )
        return false;
    if (
        !frame->vertex_buffer ||
        frame->vertex_buffer_bytes < required_vertex_bytes
    ) {
        const UINT requested =
            static_cast<UINT>(required_vertex_bytes);
        const UINT doubled = frame->vertex_buffer_bytes <=
                (std::numeric_limits<UINT>::max)() / 2
            ? frame->vertex_buffer_bytes * 2
            : (std::numeric_limits<UINT>::max)();
        const UINT capacity = (std::max)(requested, doubled);
        D3D11_BUFFER_DESC description{};
        description.ByteWidth = capacity;
        description.Usage = D3D11_USAGE_DYNAMIC;
        description.BindFlags = D3D11_BIND_VERTEX_BUFFER;
        description.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
        ComPtr<ID3D11Buffer> replacement;
        if (FAILED(device->CreateBuffer(
                &description,
                nullptr,
                replacement.GetAddressOf())))
            return false;
        frame->vertex_buffer = std::move(replacement);
        frame->vertex_buffer_bytes = capacity;
    }
    const std::size_t required_material_bytes =
        material_count * sizeof(GpuMaterial);
    if (
        required_material_bytes >
        (std::numeric_limits<UINT>::max)()
    )
        return false;
    if (
        !frame->material_buffer ||
        frame->material_buffer_bytes < required_material_bytes
    ) {
        const UINT requested = static_cast<UINT>(
            (std::max<std::size_t>)(1, material_count) *
            sizeof(GpuMaterial));
        const UINT doubled = frame->material_buffer_bytes <=
                (std::numeric_limits<UINT>::max)() / 2
            ? frame->material_buffer_bytes * 2
            : (std::numeric_limits<UINT>::max)();
        UINT capacity = (std::max)(requested, doubled);
        capacity -= capacity % sizeof(GpuMaterial);
        if (capacity < requested)
            capacity = requested;
        D3D11_BUFFER_DESC description{};
        description.ByteWidth = capacity;
        description.Usage = D3D11_USAGE_DYNAMIC;
        description.BindFlags = D3D11_BIND_SHADER_RESOURCE;
        description.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
        description.MiscFlags =
            D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
        description.StructureByteStride = sizeof(GpuMaterial);
        ComPtr<ID3D11Buffer> replacement;
        if (FAILED(device->CreateBuffer(
                &description,
                nullptr,
                replacement.GetAddressOf())))
            return false;
        D3D11_SHADER_RESOURCE_VIEW_DESC view_description{};
        view_description.Format = DXGI_FORMAT_UNKNOWN;
        view_description.ViewDimension =
            D3D11_SRV_DIMENSION_BUFFER;
        view_description.Buffer.FirstElement = 0;
        view_description.Buffer.NumElements =
            capacity / sizeof(GpuMaterial);
        ComPtr<ID3D11ShaderResourceView> replacement_view;
        if (FAILED(device->CreateShaderResourceView(
                replacement.Get(),
                &view_description,
                replacement_view.GetAddressOf())))
            return false;
        frame->material_buffer = std::move(replacement);
        frame->material_view = std::move(replacement_view);
        frame->material_buffer_bytes = capacity;
    }
    for (auto& constant_buffer : frame->constant_buffers) {
        if (!constant_buffer) {
            D3D11_BUFFER_DESC description{};
            description.ByteWidth = sizeof(DrawConstants);
            description.Usage = D3D11_USAGE_DEFAULT;
            description.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
            if (FAILED(device->CreateBuffer(
                    &description,
                    nullptr,
                    constant_buffer.GetAddressOf())))
                return false;
        }
    }
    if (!frame->vram_texture) {
        D3D11_TEXTURE2D_DESC description{};
        description.Width = 1024;
        description.Height = 512;
        description.MipLevels = 1;
        description.ArraySize = 1;
        description.Format = DXGI_FORMAT_R16_UINT;
        description.SampleDesc.Count = 1;
        description.Usage = D3D11_USAGE_DEFAULT;
        description.BindFlags = D3D11_BIND_SHADER_RESOURCE;
        if (
            FAILED(device->CreateTexture2D(
                &description,
                nullptr,
                frame->vram_texture.GetAddressOf())) ||
            FAILED(device->CreateShaderResourceView(
                frame->vram_texture.Get(),
                nullptr,
                frame->vram_view.GetAddressOf()))
        )
            return false;
    }
    return true;
}

bool ensure_output_resources(
    BaseResources* resources,
    std::uint32_t output_width,
    std::uint32_t output_height
) {
    ID3D11Device* device = resources->device.Get();
    if (
        resources->color_texture &&
        resources->output_width == output_width &&
        resources->output_height == output_height
    )
        return true;

    // Output-size changes also rewind the staging indices. Detach the matching
    // mutable inputs so an unfinished prior-size frame cannot alias slot zero
    // in the new generation.
    release_frame_generation(resources);
    D3D11_TEXTURE2D_DESC color_description{};
    color_description.Width = output_width;
    color_description.Height = output_height;
    color_description.MipLevels = 1;
    color_description.ArraySize = 1;
    color_description.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    color_description.SampleDesc.Count = 1;
    color_description.Usage = D3D11_USAGE_DEFAULT;
    color_description.BindFlags = D3D11_BIND_RENDER_TARGET;
    if (
        FAILED(device->CreateTexture2D(
            &color_description,
            nullptr,
            resources->color_texture.GetAddressOf())) ||
        FAILED(device->CreateRenderTargetView(
            resources->color_texture.Get(),
            nullptr,
            resources->color_view.GetAddressOf()))
    )
        return false;

    D3D11_TEXTURE2D_DESC depth_description = color_description;
    depth_description.Format = DXGI_FORMAT_D24_UNORM_S8_UINT;
    depth_description.BindFlags = D3D11_BIND_DEPTH_STENCIL;
    if (
        FAILED(device->CreateTexture2D(
            &depth_description,
            nullptr,
            resources->depth_texture.GetAddressOf())) ||
        FAILED(device->CreateDepthStencilView(
            resources->depth_texture.Get(),
            nullptr,
            resources->depth_view.GetAddressOf()))
    )
        return false;

    D3D11_TEXTURE2D_DESC staging_description = color_description;
    staging_description.Usage = D3D11_USAGE_STAGING;
    staging_description.BindFlags = 0;
    staging_description.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
    D3D11_QUERY_DESC completion_description{};
    completion_description.Query = D3D11_QUERY_EVENT;
    for (std::size_t index = 0;
         index < resources->staging_textures.size();
         ++index) {
        auto& staging = resources->staging_textures[index];
        auto& completion = resources->staging_completion_queries[index];
        if (FAILED(device->CreateTexture2D(
                &staging_description,
                nullptr,
                staging.GetAddressOf())) ||
            FAILED(device->CreateQuery(
                &completion_description,
                completion.GetAddressOf())))
            return false;
    }
    for (std::size_t index = 0;
         index < resources->async_staging_textures.size();
         ++index) {
        auto& staging = resources->async_staging_textures[index];
        auto& completion = resources->async_completion_queries[index];
        if (FAILED(device->CreateTexture2D(
                &staging_description,
                nullptr,
                staging.GetAddressOf())) ||
            FAILED(device->CreateQuery(
                &completion_description,
                completion.GetAddressOf())))
            return false;
    }
    resources->output_width = output_width;
    resources->output_height = output_height;
    return true;
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
    const std::uint32_t target_display_width =
        world_gpu_target_display_width(draw_list, options);
    if (target_display_width == 0 || target_display_width > 8192U)
        return WorldGpuRenderResult::invalid_argument;
    const std::uint32_t output_width =
        target_display_width * output_scale;
    const std::uint32_t output_height =
        static_cast<std::uint32_t>(draw_list.display_height) *
        output_scale;
    const float horizontal_projection_scale =
        static_cast<float>(draw_list.display_width) /
        static_cast<float>(target_display_width);
    const std::int32_t horizontal_margin = static_cast<std::int32_t>(
        (output_width -
            static_cast<std::uint32_t>(draw_list.display_width) *
                output_scale) /
        2U);
    const std::int32_t horizontal_extra = static_cast<std::int32_t>(
        output_width -
        static_cast<std::uint32_t>(draw_list.display_width) * output_scale);
    const auto hud_output_offset = [
        horizontal_extra,
        &draw_list
    ] (const HudHorizontalPlacement& placement)
    {
        const float proportional_margin =
            draw_list.display_width > 0
            ? placement.edge_margin * horizontal_extra /
                static_cast<float>(draw_list.display_width)
            : 0.0F;
        switch (placement.anchor) {
            case HudHorizontalAnchor::left:
                return proportional_margin;
            case HudHorizontalAnchor::right:
                return static_cast<float>(horizontal_extra) -
                    proportional_margin;
            case HudHorizontalAnchor::center:
            default:
                return static_cast<float>(horizontal_extra) * 0.5F;
        }
    };
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

    // GT2 already submits complete tire and sidewall geometry with an exact
    // per-wheel guest transform.  Reconstructing a second cylindrical shell
    // from those commands can cross the camera plane during replay close-ups
    // and turn one tire into a detached, screen-filling object.  Keep the old
    // reconstruction available for focused diagnostics, but render the
    // authored wheel mesh in normal builds.
    std::vector<SmoothWheel> smooth_wheels;
    std::vector<HudHorizontalPlacement> hud_horizontal_placements;
    if (const char* enabled = std::getenv("OPENGT_RENDER_SMOOTH_WHEELS");
        enabled != nullptr && std::strcmp(enabled, "1") == 0) {
        try {
            smooth_wheels = build_smooth_wheels(draw_list);
        } catch (const std::bad_alloc&) {
            return WorldGpuRenderResult::resource_failed;
        }
    }
    try {
        hud_horizontal_placements =
            build_hud_horizontal_placements(draw_list);
        emit_vehicle_diagnostics(
            draw_list, smooth_wheels, options.synthetic_midpoint);
        emit_clip_rect_diagnostics(draw_list);
        emit_primitive_diagnostics(draw_list, options);
    } catch (const std::bad_alloc&) {
        return WorldGpuRenderResult::resource_failed;
    }
    static thread_local std::uint64_t wheel_diagnostic_frame = 0;
    std::uint64_t wheel_diagnostic_interval = 120;
    if (const char* configured_interval = std::getenv(
            "OPENGT_RENDER_WHEEL_DIAGNOSTICS_INTERVAL")) {
        const unsigned long parsed = std::strtoul(
            configured_interval, nullptr, 10);
        if (parsed != 0)
            wheel_diagnostic_interval = parsed;
    }
    const bool emit_wheel_diagnostics =
        std::getenv("OPENGT_RENDER_WHEEL_DIAGNOSTICS") != nullptr &&
        (std::getenv("OPENGT_RENDER_WHEEL_DIAGNOSTICS_RACE_ONLY") ==
            nullptr || draw_list.vehicle_commands >= 1000U) &&
        (wheel_diagnostic_frame++ % wheel_diagnostic_interval) == 0;
    if (emit_wheel_diagnostics) {
        double worst_fit = 0.0;
        std::size_t exact_shells = 0;
        std::size_t vehicle_commands = 0;
        std::vector<std::uint64_t> vehicle_groups;
        vehicle_groups.reserve(32);
        for (const auto& command : draw_list.commands) {
            if (command.object_kind != 2U)
                continue;
            ++vehicle_commands;
            const std::uint64_t group =
                (static_cast<std::uint64_t>(command.object_id) << 32U) ^
                command.transform_id;
            if (std::find(
                    vehicle_groups.begin(), vehicle_groups.end(), group) ==
                vehicle_groups.end())
                vehicle_groups.push_back(group);
        }
        for (const auto& wheel : smooth_wheels)
        {
            worst_fit = (std::max)(worst_fit, wheel.maximum_fit_error);
            exact_shells += wheel.exact_transform ? 1U : 0U;
        }
        std::fprintf(
            stderr,
            "[Render-Wheels] sample=%s shells=%zu exact=%zu fitted=%zu "
            "worstResidual=%.3f commands=%zu vehicleCommands=%zu "
            "vehicleGroups=%zu\n",
            options.synthetic_midpoint ? "midpoint" : "actual",
            smooth_wheels.size(),
            exact_shells,
            smooth_wheels.size() - exact_shells,
            worst_fit,
            draw_list.commands.size(),
            vehicle_commands,
            vehicle_groups.size());
        static thread_local bool printed_wheel_detail = false;
        if (!printed_wheel_detail && !smooth_wheels.empty()) {
            printed_wheel_detail = true;
            const SmoothWheel* largest_wheel = nullptr;
            double largest_area = -1.0;
            for (const auto& wheel : smooth_wheels) {
                double minimum_x =
                    (std::numeric_limits<double>::max)();
                double minimum_y = minimum_x;
                double maximum_x =
                    (std::numeric_limits<double>::lowest)();
                double maximum_y = maximum_x;
                for (const auto& command : draw_list.commands) {
                    if (!same_wheel_group(wheel, command))
                        continue;
                    for (const auto& vertex : command.vertices) {
                        minimum_x = (std::min)(
                            minimum_x,
                            static_cast<double>(vertex.screen_x));
                        minimum_y = (std::min)(
                            minimum_y,
                            static_cast<double>(vertex.screen_y));
                        maximum_x = (std::max)(
                            maximum_x,
                            static_cast<double>(vertex.screen_x));
                        maximum_y = (std::max)(
                            maximum_y,
                            static_cast<double>(vertex.screen_y));
                    }
                }
                const double area =
                    (maximum_x - minimum_x) * (maximum_y - minimum_y);
                if (area > largest_area) {
                    largest_area = area;
                    largest_wheel = &wheel;
                }
            }
            if (largest_wheel != nullptr) {
                std::fprintf(
                    stderr,
                    "[Render-Wheel-Detail] object=%u model=%08x "
                    "transform=%016llx source=%s area=%.2f radius=%.2f "
                    "width=%.2f residual=%.3f\n",
                    largest_wheel->object_id,
                    largest_wheel->model_pointer,
                    static_cast<unsigned long long>(
                        largest_wheel->transform_id),
                    largest_wheel->exact_transform ? "guest-rt" : "fit",
                    largest_area,
                    largest_wheel->radius,
                    largest_wheel->half_width,
                    largest_wheel->maximum_fit_error);
                for (std::size_t command_index = 0;
                     command_index < draw_list.commands.size();
                     ++command_index) {
                    const auto& command =
                        draw_list.commands[command_index];
                    if (!same_wheel_group(*largest_wheel, command) ||
                        command.material_index >=
                            draw_list.materials.size())
                        continue;
                    const auto& material =
                        draw_list.materials[command.material_index];
                    if ((material.primitive_flags &
                            (textured_flag | semi_transparent_flag)) != 0)
                        continue;
                    std::fprintf(
                        stderr,
                        "[Render-Wheel-Command] index=%zu tread=%u "
                        "rgb=%u/%u/%u model=(%d,%d,%d)(%d,%d,%d)"
                        "(%d,%d,%d)\n",
                        command_index,
                        vehicle_wheel_tread(command, material) ? 1U : 0U,
                        command.vertices[0].r,
                        command.vertices[0].g,
                        command.vertices[0].b,
                        command.vertices[0].model_x,
                        command.vertices[0].model_y,
                        command.vertices[0].model_z,
                        command.vertices[1].model_x,
                        command.vertices[1].model_y,
                        command.vertices[1].model_z,
                        command.vertices[2].model_x,
                        command.vertices[2].model_y,
                        command.vertices[2].model_z);
                }
            }
        }
    }
    constexpr std::size_t smooth_wheel_segments = 32;
    constexpr std::size_t smooth_wheel_vertices =
        // Six vertices for the cylindrical tread and three for each of the
        // two circular sidewall faces.  The authored textured rim is drawn
        // afterward, so this shell only supplies a stable tire silhouette.
        smooth_wheel_segments * 12;

    auto& base = base_resources(options.use_software_adapter);
    if (!base.ready)
        return WorldGpuRenderResult::device_failed;
    configure_replacement_pack(&base);
    ID3D11DeviceContext* context = base.context.Get();
    const std::size_t authored_vertex_count =
        draw_list.commands.size() * 3;
    const std::vector<std::uint8_t> perspective_uv_eligibility =
        options.perspective_correct
            ? perspective_uv_island_eligibility(draw_list)
            : std::vector<std::uint8_t>{};
    const std::size_t vertex_count = std::max<std::size_t>(
        3,
        authored_vertex_count +
            smooth_wheels.size() * smooth_wheel_vertices);
    if (!ensure_output_resources(&base, output_width, output_height))
        return WorldGpuRenderResult::resource_failed;
    const UINT staging_write = base.staging_write_index;
    const UINT staging_fill_count = base.staging_fill_count;
    const UINT async_staging_write = base.async_staging_write_index;
    const UINT frame_input_index = options.asynchronous_readback
        ? (base.async_staging_count < world_gpu_readback_pair_delay * 2U
            ? async_staging_write %
                static_cast<UINT>(world_gpu_readback_pair_delay * 2U)
            : async_staging_write)
        : staging_write;
    auto& frame = base.frame_inputs[frame_input_index];
    if (!ensure_mutable_frame_resources(
            &base,
            &frame,
            vertex_count,
            draw_list.commands.size() + smooth_wheels.size()))
        return WorldGpuRenderResult::resource_failed;
    using PhaseClock = std::chrono::steady_clock;
    const auto render_started = PhaseClock::now();
    std::uint64_t readback_microseconds = 0;
    std::uint64_t completion_wait_microseconds = 0;
    std::uint64_t map_wait_microseconds = 0;
    std::uint64_t copy_microseconds = 0;
    bool output_valid = false;
    const std::size_t row_size =
        static_cast<std::size_t>(output_width) * 4;
    const auto read_staging = [&] (UINT index) {
        const auto read_started = PhaseClock::now();
        const auto completion_started = PhaseClock::now();
        BOOL complete = FALSE;
        std::uint32_t polls = 0;
        while (true) {
            const HRESULT completion_result = context->GetData(
                base.staging_completion_queries[index].Get(),
                &complete,
                sizeof(complete),
                D3D11_ASYNC_GETDATA_DONOTFLUSH);
            if (completion_result == S_OK && complete)
                break;
            if (FAILED(completion_result))
                return false;
            // Completed copies normally pass the first probe because every
            // slot is four submissions old. On a genuine scheduler/GPU tail,
            // yield instead of entering the driver's unbounded blocking Map.
            if (++polls < 64)
                std::this_thread::yield();
            else
                std::this_thread::sleep_for(
                    std::chrono::microseconds(100));
        }
        completion_wait_microseconds += static_cast<std::uint64_t>(
            std::chrono::duration_cast<std::chrono::microseconds>(
                PhaseClock::now() - completion_started).count());

        const auto map_started = PhaseClock::now();
        D3D11_MAPPED_SUBRESOURCE mapped{};
        while (true) {
            const HRESULT map_result = context->Map(
                base.staging_textures[index].Get(),
                0,
                D3D11_MAP_READ,
                D3D11_MAP_FLAG_DO_NOT_WAIT,
                &mapped);
            if (SUCCEEDED(map_result))
                break;
            if (map_result != DXGI_ERROR_WAS_STILL_DRAWING)
                return false;
            std::this_thread::yield();
        }
        map_wait_microseconds += static_cast<std::uint64_t>(
            std::chrono::duration_cast<std::chrono::microseconds>(
                PhaseClock::now() - map_started).count());
        const auto copy_started = PhaseClock::now();
        for (std::uint32_t y = 0; y < output_height; ++y) {
            std::memcpy(
                output_rgba + static_cast<std::size_t>(y) * row_size,
                static_cast<const std::uint8_t*>(mapped.pData) +
                    static_cast<std::size_t>(y) * mapped.RowPitch,
                row_size);
        }
        context->Unmap(base.staging_textures[index].Get(), 0);
        copy_microseconds += static_cast<std::uint64_t>(
            std::chrono::duration_cast<std::chrono::microseconds>(
                PhaseClock::now() - copy_started).count());
        readback_microseconds += static_cast<std::uint64_t>(
            std::chrono::duration_cast<std::chrono::microseconds>(
                PhaseClock::now() - read_started).count());
        output_valid = true;
        return true;
    };
    // Once all four slots are populated, read the slot this call will
    // overwrite. It was submitted four render calls ago (the corresponding
    // image from two authored pairs earlier), giving the GPU two complete
    // 30 Hz guest intervals to finish. The even ring size preserves exact
    // midpoint/actual pairing while moving the blocking Map out of the
    // observed 39-49 ms GPU tail.
    if (
        !options.asynchronous_readback &&
        staging_fill_count >= base.staging_textures.size()
    ) {
        if (!read_staging(staging_write))
            return WorldGpuRenderResult::render_failed;
    } else if (
        !options.asynchronous_readback &&
        !base.staging_warmup_output.empty()
    ) {
        // During the single-call fill interval, repeat the reset image instead
        // of reading an uninitialized staging resource. The pair bridge marks
        // this startup output as a repeat and replaces it on the next pair.
        std::memcpy(
            output_rgba,
            base.staging_warmup_output.data(),
            required_output);
        output_valid = true;
    }
    if (
        options.asynchronous_readback &&
        base.async_staging_count >= base.async_staging_textures.size()
    )
        return WorldGpuRenderResult::resource_failed;
    // Each in-flight image owns this VRAM texture. Always populate the selected
    // slot: an immediate-previous-frame reuse hint cannot apply to a slot last
    // used up to sixteen submissions ago.
    context->UpdateSubresource(
        frame.vram_texture.Get(),
        0,
        nullptr,
        vram,
        1024U * sizeof(std::uint16_t),
        1024U * 512U * sizeof(std::uint16_t));

    const float clear[] = {
        (options.clear_color_rgba8 & 0xFF) / 255.0F,
        ((options.clear_color_rgba8 >> 8) & 0xFF) / 255.0F,
        ((options.clear_color_rgba8 >> 16) & 0xFF) / 255.0F,
        ((options.clear_color_rgba8 >> 24) & 0xFF) / 255.0F,
    };
    context->ClearRenderTargetView(base.color_view.Get(), clear);
    context->ClearDepthStencilView(
        base.depth_view.Get(),
        D3D11_CLEAR_DEPTH | D3D11_CLEAR_STENCIL,
        1.0F,
        0);
    ID3D11RenderTargetView* render_target = base.color_view.Get();
    context->OMSetRenderTargets(1, &render_target, base.depth_view.Get());
    const D3D11_VIEWPORT viewport{
        0.0F,
        0.0F,
        static_cast<float>(output_width),
        static_cast<float>(output_height),
        0.0F,
        1.0F,
    };
    context->RSSetViewports(1, &viewport);
    context->RSSetState(base.rasterizer.Get());
    context->IASetInputLayout(base.input_layout.Get());
    context->IASetPrimitiveTopology(
        D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

    D3D11_MAPPED_SUBRESOURCE mapped_vertices{};
    if (FAILED(context->Map(
            frame.vertex_buffer.Get(),
            0,
            D3D11_MAP_WRITE_DISCARD,
            0,
            &mapped_vertices)))
        return WorldGpuRenderResult::render_failed;
    auto* gpu_vertices =
        static_cast<GpuVertex*>(mapped_vertices.pData);
    for (std::size_t command_index = 0;
         command_index < draw_list.commands.size();
         ++command_index) {
        const auto& command = draw_list.commands[command_index];
        const auto& material = draw_list.materials[command.material_index];
        const bool screen_space =
            (material.primitive_flags &
                world_primitive_screen_space_flag) != 0;
        const auto& hud_placement =
            hud_horizontal_placements[command_index];
        const float hud_native_offset = screen_space
            ? hud_output_offset(hud_placement) /
                static_cast<float>(output_scale)
            : static_cast<float>(horizontal_margin) /
                static_cast<float>(output_scale);
        const float centered_native_offset =
            static_cast<float>(target_display_width -
                static_cast<std::uint32_t>(draw_list.display_width)) * 0.5F;
        const float hud_ndc_shift = screen_space
            ? 2.0F * (hud_native_offset - centered_native_offset) /
                static_cast<float>(target_display_width)
            : 0.0F;
        const bool vehicle_shadow =
            soft_vehicle_shadow(command, material);
        for (int index = 0; index < 3; ++index) {
            const auto& source = command.vertices[index];
            const float alpha = vehicle_shadow ? 0.06F : 1.0F;
            gpu_vertices[command_index * 3 + index] = GpuVertex{
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
                    alpha,
                },
                static_cast<std::uint32_t>(command_index),
            };
            gpu_vertices[command_index * 3 + index].position[0] *=
                horizontal_projection_scale;
            gpu_vertices[command_index * 3 + index].position[0] +=
                source.clip_w * hud_ndc_shift;
        }
    }
    constexpr double two_pi = 6.283185307179586476925286766559;
    for (std::size_t wheel_index = 0;
         wheel_index < smooth_wheels.size();
         ++wheel_index) {
        auto& wheel = smooth_wheels[wheel_index];
        const auto& sample = draw_list.commands[
            wheel.insertion_command].vertices[0];
        const std::uint32_t material_slot = static_cast<std::uint32_t>(
            draw_list.commands.size() + wheel_index);
        const auto make_vertex = [&] (
            double model_x,
            double model_y,
            double model_z
        ) {
            const double model[3] = {
                model_x - wheel.model_centroid[0],
                model_y - wheel.model_centroid[1],
                model_z - wheel.model_centroid[2],
            };
            double view[3] = {
                wheel.view_centroid[0],
                wheel.view_centroid[1],
                wheel.view_centroid[2],
            };
            for (int view_axis = 0; view_axis < 3; ++view_axis)
                for (int model_axis = 0; model_axis < 3; ++model_axis)
                    view[view_axis] +=
                        wheel.model_to_view[view_axis][model_axis] *
                        model[model_axis];
            const double view_z = (std::max)(1.0, view[2]);
            const double projected_x =
                sample.draw_offset_x +
                sample.projection_offset_x / 65536.0 +
                sample.projection_plane * view[0] / view_z;
            const double projected_y =
                sample.draw_offset_y +
                sample.projection_offset_y / 65536.0 +
                sample.projection_plane * view[1] / view_z;
            wheel.shell_minimum_x = (std::min)(
                wheel.shell_minimum_x, projected_x);
            wheel.shell_minimum_y = (std::min)(
                wheel.shell_minimum_y, projected_y);
            wheel.shell_maximum_x = (std::max)(
                wheel.shell_maximum_x, projected_x);
            wheel.shell_maximum_y = (std::max)(
                wheel.shell_maximum_y, projected_y);
            const double guest_ndc_x =
                ((projected_x - draw_list.display_x) /
                    draw_list.display_width) * 2.0 - 1.0;
            const double ndc_x = guest_ndc_x *
                draw_list.display_width / target_display_width;
            const double ndc_y =
                1.0 - ((projected_y - draw_list.display_y) /
                    draw_list.display_height) * 2.0;
            constexpr double near_plane = 16.0;
            constexpr double far_plane = 1048576.0;
            constexpr double depth_a =
                far_plane / (far_plane - near_plane);
            constexpr double depth_b =
                -near_plane * far_plane /
                (far_plane - near_plane);
            return GpuVertex{
                {
                    static_cast<float>(ndc_x * view_z),
                    static_cast<float>(ndc_y * view_z),
                    static_cast<float>(depth_a * view_z + depth_b),
                    static_cast<float>(view_z),
                },
                {0.0F, 0.0F},
                {0.015F, 0.015F, 0.015F, 1.0F},
                material_slot,
            };
        };
        std::size_t output_index = authored_vertex_count +
            wheel_index * smooth_wheel_vertices;
        for (std::size_t segment = 0;
             segment < smooth_wheel_segments;
             ++segment) {
            const double angle0 =
                two_pi * segment / smooth_wheel_segments;
            const double angle1 =
                two_pi * (segment + 1) / smooth_wheel_segments;
            const double x0 = wheel.radius * std::cos(angle0);
            const double y0 = wheel.radius * std::sin(angle0);
            const double x1 = wheel.radius * std::cos(angle1);
            const double y1 = wheel.radius * std::sin(angle1);
            const GpuVertex a = make_vertex(
                x0, y0, -wheel.half_width);
            const GpuVertex b = make_vertex(
                x0, y0, wheel.half_width);
            const GpuVertex c = make_vertex(
                x1, y1, wheel.half_width);
            const GpuVertex d = make_vertex(
                x1, y1, -wheel.half_width);
            gpu_vertices[output_index++] = a;
            gpu_vertices[output_index++] = b;
            gpu_vertices[output_index++] = c;
            gpu_vertices[output_index++] = a;
            gpu_vertices[output_index++] = c;
            gpu_vertices[output_index++] = d;

            const GpuVertex negative_center = make_vertex(
                0.0, 0.0, -wheel.half_width);
            const GpuVertex positive_center = make_vertex(
                0.0, 0.0, wheel.half_width);
            gpu_vertices[output_index++] = negative_center;
            gpu_vertices[output_index++] = d;
            gpu_vertices[output_index++] = a;
            gpu_vertices[output_index++] = positive_center;
            gpu_vertices[output_index++] = b;
            gpu_vertices[output_index++] = c;
        }
    }
    if (emit_wheel_diagnostics) {
        for (const auto& wheel : smooth_wheels) {
            if (wheel.shell_maximum_x < wheel.shell_minimum_x)
                continue;
            const double spill_left =
                wheel.clip_x0 - wheel.shell_minimum_x;
            const double spill_top =
                wheel.clip_y0 - wheel.shell_minimum_y;
            const double spill_right =
                wheel.shell_maximum_x - (wheel.clip_x1 + 1);
            const double spill_bottom =
                wheel.shell_maximum_y - (wheel.clip_y1 + 1);
            const double spill = (std::max)(
                (std::max)(spill_left, spill_top),
                (std::max)(spill_right, spill_bottom));
            if (spill <= 0.0)
                continue;
            std::fprintf(
                stderr,
                "[Render-Wheel-Clip] sample=%s object=%u model=%08x "
                "transform=%016llx channel=%u shell=%.1f,%.1f..%.1f,%.1f "
                "clip=%d,%d..%d,%d spill=%.1f display=%d,%d..%d,%d\n",
                options.synthetic_midpoint ? "midpoint" : "actual",
                wheel.object_id,
                wheel.model_pointer,
                static_cast<unsigned long long>(wheel.transform_id),
                static_cast<unsigned>(wheel.channel),
                wheel.shell_minimum_x,
                wheel.shell_minimum_y,
                wheel.shell_maximum_x,
                wheel.shell_maximum_y,
                wheel.clip_x0,
                wheel.clip_y0,
                wheel.clip_x1,
                wheel.clip_y1,
                spill,
                draw_list.display_x,
                draw_list.display_y,
                draw_list.display_x + draw_list.display_width,
                draw_list.display_y + draw_list.display_height);
        }
    }
    context->Unmap(frame.vertex_buffer.Get(), 0);

    {
        D3D11_MAPPED_SUBRESOURCE mapped_materials{};
        if (FAILED(context->Map(
                frame.material_buffer.Get(),
                0,
                D3D11_MAP_WRITE_DISCARD,
                0,
                &mapped_materials)))
            return WorldGpuRenderResult::render_failed;
        auto* gpu_materials =
            static_cast<GpuMaterial*>(mapped_materials.pData);
        std::unordered_map<
            ReplacementSignature,
            ReplacementResolution,
            ReplacementSignatureHash> replacement_cache;
        const bool inspect_replacements =
            options.high_resolution_textures &&
            !base.replacement_entries.empty() &&
            !base.replacement_uploads.empty();
        for (std::size_t command_index = 0;
             command_index < draw_list.commands.size();
             ++command_index) {
            const auto& command = draw_list.commands[command_index];
            if (command.material_index >= draw_list.materials.size()) {
                context->Unmap(frame.material_buffer.Get(), 0);
                return WorldGpuRenderResult::render_failed;
            }
            const auto& material =
                draw_list.materials[command.material_index];
            const bool opaque_track_surface =
                is_opaque_track_surface(command, material);
            const bool vehicle_shadow =
                soft_vehicle_shadow(command, material);
            const bool wheel_tread =
                vehicle_wheel_tread(command, material);
            const auto smooth_wheel = std::find_if(
                smooth_wheels.begin(), smooth_wheels.end(),
                [&] (const SmoothWheel& wheel) {
                    return same_wheel_group(wheel, command);
                });
            const bool smoothed_wheel_surface =
                smooth_wheel != smooth_wheels.end() &&
                (wheel_tread || vehicle_wheel_sidewall_ring(
                    command, material, *smooth_wheel));
            const bool perspective_eligible =
                options.perspective_correct &&
                perspective_uv_eligibility[command_index] != 0;
            const ReplacementResolution replacement = inspect_replacements
                ? resolve_texture_replacement(
                    &base,
                    command,
                    material,
                    vram,
                    options.high_resolution_textures,
                    &replacement_cache)
                : ReplacementResolution{};
            gpu_materials[command_index] = GpuMaterial{
                material.primitive_flags,
                material.texture_page,
                material.clut,
                material.environment_flags,
                material.texture_mask_x,
                material.texture_mask_y,
                material.texture_offset_x,
                material.texture_offset_y,
                    (opaque_track_surface ? 1U : 0U) |
                    (vehicle_shadow ? 2U : 0U) |
                    (smoothed_wheel_surface ? 4U : 0U) |
                    (perspective_eligible
                        ? perspective_uv_eligible_flag
                        : 0U),
                replacement.minimum_u,
                replacement.minimum_v,
                replacement.source_width,
                replacement.source_height,
                replacement.rect.x,
                replacement.rect.y,
                replacement.rect.width,
                replacement.rect.height,
                replacement.mode,
                replacement.color_scale[0],
                replacement.color_scale[1],
                replacement.color_scale[2],
                replacement.color_bias[0],
                replacement.color_bias[1],
                replacement.color_bias[2],
            };
        }
        for (std::size_t wheel_index = 0;
             wheel_index < smooth_wheels.size();
             ++wheel_index) {
            gpu_materials[draw_list.commands.size() + wheel_index] =
                GpuMaterial{};
        }
        context->Unmap(frame.material_buffer.Get(), 0);
    }

    // Bind only after both dynamic buffers have been populated. This keeps the
    // D3D resource/allocation selected by WRITE_DISCARD identical to the one
    // observed by every draw in this submission.
    const UINT stride = sizeof(GpuVertex);
    const UINT offset = 0;
    ID3D11Buffer* raw_vertex_buffer = frame.vertex_buffer.Get();
    context->IASetVertexBuffers(
        0, 1, &raw_vertex_buffer, &stride, &offset);
    context->VSSetShader(base.vertex_shader.Get(), nullptr, 0);
    context->PSSetShader(base.pixel_shader.Get(), nullptr, 0);
    ID3D11ShaderResourceView* shader_views[] = {
        frame.vram_view.Get(),
        frame.material_view.Get(),
        options.high_resolution_textures
            ? base.replacement_view.Get()
            : nullptr,
    };
    context->PSSetShaderResources(
        0,
        static_cast<UINT>(std::size(shader_views)),
        shader_views);
    ID3D11SamplerState* replacement_sampler =
        base.replacement_sampler.Get();
    context->PSSetSamplers(0, 1, &replacement_sampler);

    for (std::uint32_t pass = 0; pass < 3; ++pass) {
        const DrawConstants constants{
            0,
            pass,
            options.dithering ? 1U : 0U,
            options.perspective_correct ? 1U : 0U,
            options.texture_smoothing ? 1U : 0U,
            // Off only for rendering an A/B proof of the coverage path from
            // identical captured geometry.
            std::getenv("OPENGT_DISABLE_FOOTPRINT_COVERAGE") != nullptr
                ? 0U
                : 1U,
            options.high_resolution_textures
                ? base.replacement_width
                : 0U,
            options.high_resolution_textures
                ? base.replacement_height
                : 0U,
        };
        context->UpdateSubresource(
            frame.constant_buffers[pass].Get(),
            0,
            nullptr,
            &constants,
            0,
            0);
    }

    WorldViewChannel depth_channel = WorldViewChannel::main_view;
    bool depth_channel_initialized = false;
    int bound_pass = -1;
    bool has_bound_scissor = false;
    D3D11_RECT bound_scissor{};
    ID3D11BlendState* bound_blend_state = nullptr;
    float bound_blend_factor = -1.0F;
    ID3D11DepthStencilState* bound_depth_state = nullptr;
    const bool diagnose_batches =
        std::getenv("OPENGT_RENDER_BATCH_DIAGNOSTICS") != nullptr;
    std::array<std::uint64_t, 3> diagnostic_batches{};
    std::array<std::uint64_t, 3> diagnostic_draws{};
    std::array<std::uint64_t, 3> diagnostic_commands{};
    std::array<std::uint64_t, 3> diagnostic_transparent_draws{};
    std::array<std::uint64_t, 4> diagnostic_blend_batches{};
    std::array<std::size_t, 3> diagnostic_max_batch{};
    for (std::size_t command_index = 0;
         command_index < draw_list.commands.size();) {
        for (std::size_t wheel_index = 0;
             wheel_index < smooth_wheels.size();
             ++wheel_index) {
            const auto& wheel = smooth_wheels[wheel_index];
            if (wheel.insertion_command != command_index)
                continue;
            // The shell stands in for authored wheel triangles, so it obeys
            // the same guest drawing area those triangles were submitted
            // under. Drawing it against the full render target let rear-view
            // mirror wheels - projected huge because the mirror camera sits
            // metres from the car behind - paint across the whole frame as
            // black discs.
            const D3D11_RECT wheel_scissor{
                std::clamp(
                    static_cast<LONG>(
                        (wheel.clip_x0 - draw_list.display_x) *
                        static_cast<std::int32_t>(output_scale) +
                        horizontal_margin),
                    0L,
                    static_cast<LONG>(output_width)),
                std::clamp(
                    static_cast<LONG>(
                        (wheel.clip_y0 - draw_list.display_y) *
                        static_cast<std::int32_t>(output_scale)),
                    0L,
                    static_cast<LONG>(output_height)),
                std::clamp(
                    static_cast<LONG>(
                        (wheel.clip_x1 - draw_list.display_x + 1) *
                        static_cast<std::int32_t>(output_scale) +
                        horizontal_margin),
                    0L,
                    static_cast<LONG>(output_width)),
                std::clamp(
                    static_cast<LONG>(
                        (wheel.clip_y1 - draw_list.display_y + 1) *
                        static_cast<std::int32_t>(output_scale)),
                    0L,
                    static_cast<LONG>(output_height)),
            };
            if (wheel_scissor.left >= wheel_scissor.right ||
                wheel_scissor.top >= wheel_scissor.bottom)
                continue;
            context->RSSetScissorRects(1, &wheel_scissor);
            has_bound_scissor = false;
            ID3D11Buffer* raw_constant_buffer =
                frame.constant_buffers[0].Get();
            context->PSSetConstantBuffers(0, 1, &raw_constant_buffer);
            bound_pass = 0;
            const float blend_factor[4] = {1.0F, 1.0F, 1.0F, 1.0F};
            context->OMSetBlendState(
                base.blend_states[0].Get(),
                blend_factor,
                0xFFFFFFFFU);
            bound_blend_state = base.blend_states[0].Get();
            bound_blend_factor = 1.0F;
            ID3D11DepthStencilState* depth_state =
                base.depth_states[0][0][0][0].Get();
            context->OMSetDepthStencilState(depth_state, 1);
            bound_depth_state = depth_state;
            context->Draw(
                static_cast<UINT>(smooth_wheel_vertices),
                static_cast<UINT>(
                    authored_vertex_count +
                    wheel_index * smooth_wheel_vertices));
            ++stats->draw_calls;
        }
        const auto& command = draw_list.commands[command_index];
        if (command.material_index >= draw_list.materials.size())
            return WorldGpuRenderResult::render_failed;
        const auto& material =
            draw_list.materials[command.material_index];
        const bool textured =
            (material.primitive_flags & textured_flag) != 0;
        const bool semitransparent =
            (material.primitive_flags & semi_transparent_flag) != 0;
        const bool screen_space =
            (material.primitive_flags &
                world_primitive_screen_space_flag) != 0;
        const int blend_mode =
            (material.texture_page >> 5) & 3;
        const bool vehicle_shadow =
            soft_vehicle_shadow(command, material);
        const bool combined_semitransparent =
            textured && semitransparent && blend_mode != 2;
        const int pass_count =
            textured && semitransparent && !combined_semitransparent ? 2 : 1;
        std::size_t batch_commands = 1;
        while (
            command_index + batch_commands <
                draw_list.commands.size()
        ) {
            const std::size_t next_index =
                command_index + batch_commands;
            const bool inserts_wheel = std::any_of(
                smooth_wheels.begin(), smooth_wheels.end(),
                [&] (const SmoothWheel& wheel) {
                    return wheel.insertion_command == next_index;
                });
            if (inserts_wheel)
                break;
            if (
                hud_horizontal_placements[next_index].anchor !=
                    hud_horizontal_placements[command_index].anchor ||
                hud_horizontal_placements[next_index].edge_margin !=
                    hud_horizontal_placements[command_index].edge_margin
            )
                break;
            const auto& next_command = draw_list.commands[
                next_index];
            if (next_command.material_index >=
                draw_list.materials.size())
                return WorldGpuRenderResult::render_failed;
            const auto& next_material = draw_list.materials[
                next_command.material_index];
            if (!batch_compatible(
                    command,
                    material,
                    next_command,
                    next_material))
                break;
            ++batch_commands;
        }
        const std::size_t diagnostic_kind =
            command.object_kind <= 2U ? command.object_kind : 0U;
        if (diagnose_batches) {
            ++diagnostic_batches[diagnostic_kind];
            diagnostic_draws[diagnostic_kind] += pass_count;
            diagnostic_commands[diagnostic_kind] += batch_commands;
            diagnostic_transparent_draws[diagnostic_kind] +=
                semitransparent ? pass_count : 0;
            if (semitransparent)
                ++diagnostic_blend_batches[
                    (material.texture_page >> 5) & 3U];
            diagnostic_max_batch[diagnostic_kind] =
                (std::max)(
                    diagnostic_max_batch[diagnostic_kind],
                    batch_commands);
        }

        const bool full_main_world_scissor =
            !screen_space &&
            command.channel == WorldViewChannel::main_view &&
            command.clip_x0 <= draw_list.display_x &&
            command.clip_x1 >=
                draw_list.display_x + draw_list.display_width - 1;
        const std::int32_t command_horizontal_offset = screen_space
            ? static_cast<std::int32_t>(std::lround(
                hud_output_offset(
                    hud_horizontal_placements[command_index])))
            : horizontal_margin;
        const D3D11_RECT scissor{
            full_main_world_scissor
                ? 0L
                : std::clamp(
                    static_cast<LONG>(
                        (command.clip_x0 - draw_list.display_x) *
                        static_cast<std::int32_t>(output_scale) +
                        command_horizontal_offset),
                    0L,
                    static_cast<LONG>(output_width)),
            std::clamp(
                static_cast<LONG>(
                    (command.clip_y0 - draw_list.display_y) *
                    static_cast<std::int32_t>(output_scale)),
                0L,
                static_cast<LONG>(output_height)),
            full_main_world_scissor
                ? static_cast<LONG>(output_width)
                : std::clamp(
                    static_cast<LONG>(
                        (command.clip_x1 - draw_list.display_x + 1) *
                        static_cast<std::int32_t>(output_scale) +
                        command_horizontal_offset),
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
            scissor.top >= scissor.bottom) {
            // An empty guest drawing area removes every command in the batch.
            // Counted so a vanished surface can be attributed here rather
            // than guessed at.
            stats->skipped_empty_scissor_commands +=
                static_cast<std::uint32_t>(batch_commands);
            command_index += batch_commands;
            continue;
        }
        if (
            !has_bound_scissor ||
            scissor.left != bound_scissor.left ||
            scissor.top != bound_scissor.top ||
            scissor.right != bound_scissor.right ||
            scissor.bottom != bound_scissor.bottom
        ) {
            context->RSSetScissorRects(1, &scissor);
            bound_scissor = scissor;
            has_bound_scissor = true;
        }
        const bool uses_modern_depth =
            command.object_kind == 1U && !screen_space;
        if (
            options.depth_buffer &&
            uses_modern_depth &&
            depth_channel_initialized &&
            command.channel != depth_channel
        ) {
            // A view owns one coherent depth surface for its complete world
            // pass. GT2 ordering-table buckets are submission-order hints,
            // not independent depth spaces: clearing between them lets a
            // later distant road section overwrite nearer geometry. A
            // secondary camera is a new view and therefore starts clean.
            context->ClearDepthStencilView(
                base.depth_view.Get(),
                D3D11_CLEAR_DEPTH,
                1.0F,
                0);
        }
        if (uses_modern_depth) {
            depth_channel = command.channel;
            depth_channel_initialized = true;
        }
        for (int pass = 0; pass < pass_count; ++pass) {
            const int shader_pass =
                combined_semitransparent ? 2 : pass;
            const bool blended = semitransparent &&
                (combined_semitransparent || !textured || pass == 1);
            if (bound_pass != shader_pass) {
                ID3D11Buffer* raw_constant_buffer =
                    frame.constant_buffers[shader_pass].Get();
                context->PSSetConstantBuffers(
                    0, 1, &raw_constant_buffer);
                bound_pass = shader_pass;
            }
            const float factor = blend_mode == 0 ? 0.5F :
                blend_mode == 3 ? 0.25F :
                1.0F;
            const float effective_factor = blended ? factor : 1.0F;
            const float blend_factor[4] = {
                effective_factor,
                effective_factor,
                effective_factor,
                effective_factor,
            };
            ID3D11BlendState* blend_state = vehicle_shadow
                ? base.blend_states[6].Get()
                : combined_semitransparent
                ? base.blend_states[5].Get()
                : blended
                ? base.blend_states[blend_mode + 1].Get()
                : base.blend_states[0].Get();
            if (
                blend_state != bound_blend_state ||
                effective_factor != bound_blend_factor
            ) {
                context->OMSetBlendState(
                    blend_state,
                    blend_factor,
                    0xFFFFFFFFU);
                bound_blend_state = blend_state;
                bound_blend_factor = effective_factor;
            }
            const bool check_mask =
                (material.environment_flags & check_mask_flag) != 0;
            const bool set_mask =
                (material.environment_flags & set_mask_flag) != 0;
            // Geometry outside track sections retains the PS1 ordering-table
            // contract. In particular, vehicle wheel layers are authored as
            // painter-ordered coplanar meshes; depth-testing those layers can
            // leave an opaque tyre polygon covering the outside wheel face.
            const bool use_depth =
                options.depth_buffer && uses_modern_depth;
            ID3D11DepthStencilState* depth_state =
                base.depth_states[use_depth ? 1 : 0]
                    [blended && !combined_semitransparent ? 1 : 0]
                    [check_mask ? 1 : 0]
                    [set_mask ? 1 : 0].Get();
            if (depth_state != bound_depth_state) {
                context->OMSetDepthStencilState(depth_state, 1);
                bound_depth_state = depth_state;
            }
            context->Draw(
                static_cast<UINT>(batch_commands * 3),
                static_cast<UINT>(command_index * 3));
            ++stats->draw_calls;
            if (blended)
                ++stats->transparent_draw_calls;
        }
        command_index += batch_commands;
    }

    if (diagnose_batches) {
        std::fprintf(
            stderr,
            "[Render-Batches] unknown=%llu/%llu/%llu/max%zu "
            "track=%llu/%llu/%llu/max%zu "
            "vehicle=%llu/%llu/%llu/max%zu transparent=%llu/%llu/%llu "
            "blendModes=%llu/%llu/%llu/%llu\n",
            static_cast<unsigned long long>(diagnostic_commands[0]),
            static_cast<unsigned long long>(diagnostic_batches[0]),
            static_cast<unsigned long long>(diagnostic_draws[0]),
            diagnostic_max_batch[0],
            static_cast<unsigned long long>(diagnostic_commands[1]),
            static_cast<unsigned long long>(diagnostic_batches[1]),
            static_cast<unsigned long long>(diagnostic_draws[1]),
            diagnostic_max_batch[1],
            static_cast<unsigned long long>(diagnostic_commands[2]),
            static_cast<unsigned long long>(diagnostic_batches[2]),
            static_cast<unsigned long long>(diagnostic_draws[2]),
            diagnostic_max_batch[2],
            static_cast<unsigned long long>(
                diagnostic_transparent_draws[0]),
            static_cast<unsigned long long>(
                diagnostic_transparent_draws[1]),
            static_cast<unsigned long long>(
                diagnostic_transparent_draws[2]),
            static_cast<unsigned long long>(diagnostic_blend_batches[0]),
            static_cast<unsigned long long>(diagnostic_blend_batches[1]),
            static_cast<unsigned long long>(diagnostic_blend_batches[2]),
            static_cast<unsigned long long>(diagnostic_blend_batches[3]));
    }

    if (options.asynchronous_readback) {
        context->CopyResource(
            base.async_staging_textures[async_staging_write].Get(),
            base.color_texture.Get());
        context->End(
            base.async_completion_queries[async_staging_write].Get());
        base.async_staging_write_index =
            (async_staging_write + 1U) %
            static_cast<UINT>(base.async_staging_textures.size());
        ++base.async_staging_count;
    } else {
        // Keep two complete midpoint/actual pairs behind the GPU. Each slot is
        // read before it is overwritten, preserving chronological output while
        // allowing up to two authored intervals for asynchronous completion.
        context->CopyResource(
            base.staging_textures[staging_write].Get(),
            base.color_texture.Get());
        context->End(base.staging_completion_queries[staging_write].Get());
    }
    // The immediate context may otherwise retain several CopyResource calls
    // in its driver command buffer until the later staging Map forces a
    // flush. Submit now so the GPU performs this copy during the intentional
    // two-pair readback delay; Flush does not wait for completion.
    context->Flush();
    if (
        !options.asynchronous_readback &&
        staging_fill_count == 0 &&
        !options.defer_initial_readback
    ) {
        if (!read_staging(staging_write))
            return WorldGpuRenderResult::render_failed;
        base.staging_warmup_output.assign(
            output_rgba,
            output_rgba + required_output);
    }
    if (!options.asynchronous_readback) {
        base.staging_fill_count = std::min<UINT>(
            static_cast<UINT>(base.staging_textures.size()),
            staging_fill_count + 1U);
        base.staging_write_index =
            (staging_write + 1U) %
            static_cast<UINT>(base.staging_textures.size());
    }
    const auto render_finished = PhaseClock::now();
    const auto total_microseconds = static_cast<std::uint64_t>(
        std::chrono::duration_cast<std::chrono::microseconds>(
            render_finished - render_started).count());
    if (std::getenv("OPENGT_RENDER_PHASE_DIAGNOSTICS") != nullptr) {
        const auto submit_microseconds =
            total_microseconds - readback_microseconds;
        std::fprintf(
            stderr,
            "[Render-Phases] totalMs=%.3f readbackMs=%.3f submitMs=%.3f "
            "completionMs=%.3f mapMs=%.3f copyMs=%.3f "
            "slot=%u fill=%u commands=%u draws=%u\n",
            total_microseconds / 1000.0,
            readback_microseconds / 1000.0,
            submit_microseconds / 1000.0,
            completion_wait_microseconds / 1000.0,
            map_wait_microseconds / 1000.0,
            copy_microseconds / 1000.0,
            staging_write,
            staging_fill_count,
            stats->commands,
            stats->draw_calls);
    }
    stats->output_valid = output_valid;
    return WorldGpuRenderResult::success;
}

WorldGpuReadbackResult try_read_world_d3d11_pair(
    bool use_software_adapter,
    std::uint8_t* first_output_rgba,
    std::uint8_t* second_output_rgba,
    std::size_t output_size,
    bool wait_for_completion
) noexcept {
    if (first_output_rgba == nullptr || second_output_rgba == nullptr)
        return WorldGpuReadbackResult::invalid_argument;
    auto& base = base_resources(use_software_adapter);
    if (!base.ready || base.context == nullptr)
        return WorldGpuReadbackResult::device_failed;
    if (base.async_staging_count < 2)
        return WorldGpuReadbackResult::not_ready;
    const std::size_t required_output =
        static_cast<std::size_t>(base.output_width) *
        static_cast<std::size_t>(base.output_height) * 4U;
    if (required_output == 0 || output_size < required_output)
        return WorldGpuReadbackResult::invalid_argument;

    ID3D11DeviceContext* context = base.context.Get();
    const UINT first_index = base.async_staging_read_index;
    const UINT second_index =
        (first_index + 1U) %
        static_cast<UINT>(base.async_staging_textures.size());
    const auto completed = [&] (UINT index) {
        while (true) {
            BOOL complete = FALSE;
            const HRESULT result = context->GetData(
                base.async_completion_queries[index].Get(),
                &complete,
                sizeof(complete),
                D3D11_ASYNC_GETDATA_DONOTFLUSH);
            if (result == S_OK && complete)
                return WorldGpuReadbackResult::success;
            if (FAILED(result))
                return WorldGpuReadbackResult::read_failed;
            if (!wait_for_completion)
                return WorldGpuReadbackResult::not_ready;
            std::this_thread::sleep_for(std::chrono::microseconds(100));
        }
    };
    WorldGpuReadbackResult result = completed(first_index);
    if (result != WorldGpuReadbackResult::success)
        return result;
    result = completed(second_index);
    if (result != WorldGpuReadbackResult::success)
        return result;

    const std::size_t row_size =
        static_cast<std::size_t>(base.output_width) * 4U;
    const auto read = [&] (UINT index, std::uint8_t* destination) {
        while (true) {
            D3D11_MAPPED_SUBRESOURCE mapped{};
            const HRESULT map_result = context->Map(
                base.async_staging_textures[index].Get(),
                0,
                D3D11_MAP_READ,
                D3D11_MAP_FLAG_DO_NOT_WAIT,
                &mapped);
            if (SUCCEEDED(map_result)) {
                for (std::uint32_t y = 0; y < base.output_height; ++y) {
                    std::memcpy(
                        destination + static_cast<std::size_t>(y) * row_size,
                        static_cast<const std::uint8_t*>(mapped.pData) +
                            static_cast<std::size_t>(y) * mapped.RowPitch,
                        row_size);
                }
                context->Unmap(base.async_staging_textures[index].Get(), 0);
                return WorldGpuReadbackResult::success;
            }
            if (map_result != DXGI_ERROR_WAS_STILL_DRAWING)
                return WorldGpuReadbackResult::read_failed;
            if (!wait_for_completion)
                return WorldGpuReadbackResult::not_ready;
            std::this_thread::yield();
        }
    };
    result = read(first_index, first_output_rgba);
    if (result != WorldGpuReadbackResult::success)
        return result;
    result = read(second_index, second_output_rgba);
    if (result != WorldGpuReadbackResult::success)
        return result;

    base.async_staging_read_index =
        (second_index + 1U) %
        static_cast<UINT>(base.async_staging_textures.size());
    base.async_staging_count -= 2U;
    return WorldGpuReadbackResult::success;
}

WorldGpuReadbackResult try_read_world_d3d11_image(
    bool use_software_adapter,
    std::uint8_t* output_rgba,
    std::size_t output_size,
    bool wait_for_completion
) noexcept {
    if (output_rgba == nullptr)
        return WorldGpuReadbackResult::invalid_argument;
    auto& base = base_resources(use_software_adapter);
    if (!base.ready || base.context == nullptr)
        return WorldGpuReadbackResult::device_failed;
    if (base.async_staging_count == 0)
        return WorldGpuReadbackResult::not_ready;
    const std::size_t required_output =
        static_cast<std::size_t>(base.output_width) *
        static_cast<std::size_t>(base.output_height) * 4U;
    if (required_output == 0 || output_size < required_output)
        return WorldGpuReadbackResult::invalid_argument;

    ID3D11DeviceContext* context = base.context.Get();
    const UINT index = base.async_staging_read_index;
    while (true) {
        BOOL complete = FALSE;
        const HRESULT result = context->GetData(
            base.async_completion_queries[index].Get(),
            &complete,
            sizeof(complete),
            D3D11_ASYNC_GETDATA_DONOTFLUSH);
        if (result == S_OK && complete)
            break;
        if (FAILED(result))
            return WorldGpuReadbackResult::read_failed;
        if (!wait_for_completion)
            return WorldGpuReadbackResult::not_ready;
        std::this_thread::sleep_for(std::chrono::microseconds(100));
    }

    const std::size_t row_size =
        static_cast<std::size_t>(base.output_width) * 4U;
    while (true) {
        D3D11_MAPPED_SUBRESOURCE mapped{};
        const HRESULT result = context->Map(
            base.async_staging_textures[index].Get(),
            0,
            D3D11_MAP_READ,
            D3D11_MAP_FLAG_DO_NOT_WAIT,
            &mapped);
        if (SUCCEEDED(result)) {
            for (std::uint32_t y = 0; y < base.output_height; ++y) {
                std::memcpy(
                    output_rgba + static_cast<std::size_t>(y) * row_size,
                    static_cast<const std::uint8_t*>(mapped.pData) +
                        static_cast<std::size_t>(y) * mapped.RowPitch,
                    row_size);
            }
            context->Unmap(base.async_staging_textures[index].Get(), 0);
            break;
        }
        if (result != DXGI_ERROR_WAS_STILL_DRAWING)
            return WorldGpuReadbackResult::read_failed;
        if (!wait_for_completion)
            return WorldGpuReadbackResult::not_ready;
        std::this_thread::yield();
    }

    base.async_staging_read_index =
        (index + 1U) %
        static_cast<UINT>(base.async_staging_textures.size());
    --base.async_staging_count;
    return WorldGpuReadbackResult::success;
}

std::size_t pending_world_d3d11_readback_pairs(
    bool use_software_adapter
) noexcept {
    const auto& base = base_resources(use_software_adapter);
    return base.async_staging_count / 2U;
}

void reset_world_d3d11_readback(bool use_software_adapter) noexcept {
    auto& base = base_resources(use_software_adapter);
    // A reset can follow a guest-frame gap while copies from the prior stream
    // are still executing. Merely rewinding the ring indices reuses the same
    // staging textures and event queries before that work has completed. Under
    // a long GPU tail, GetData can then observe the prior query generation while
    // the slot already contains a later frame, pairing stale identity with
    // malformed pixels. Detach every mutable resource instead. Commands already
    // submitted retain their own COM references, while the next render creates
    // an independent generation that cannot alias the abandoned work.
    release_frame_generation(&base);
}

bool set_world_d3d11_texture_uploads(
    bool use_software_adapter,
    const WorldTextureUpload* uploads,
    std::size_t upload_count
) noexcept {
    if (upload_count != 0 && uploads == nullptr)
        return false;
    try {
        auto& base = base_resources(use_software_adapter);
        if (upload_count == 0)
            base.replacement_uploads.clear();
        else
            base.replacement_uploads.assign(uploads, uploads + upload_count);
        base.replacement_resolution_cache.clear();
        return base.ready;
    } catch (...) {
        return false;
    }
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

void reset_world_d3d11_readback(bool) noexcept {}

bool set_world_d3d11_texture_uploads(
    bool,
    const WorldTextureUpload*,
    std::size_t
) noexcept {
    return false;
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
