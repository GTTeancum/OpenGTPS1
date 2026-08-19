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
#include <limits>
#include <thread>
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
    const std::int16_t model_y = command.vertices[0].model_y;
    return
        command.vertices[1].model_y == model_y &&
        command.vertices[2].model_y == model_y;
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
};

struct DrawConstants {
    std::uint32_t base_command;
    std::uint32_t pass_kind;
    std::uint32_t dithering;
    std::uint32_t perspective_correct;
    std::uint32_t texture_smoothing;
    std::uint32_t footprint_coverage;
    // Constant buffers must stay a multiple of 16 bytes.
    std::uint32_t padding[2];
};

const char shader_source[] = R"(
Texture2D<uint> Vram : register(t0);

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
};

StructuredBuffer<MaterialData> Materials : register(t1);

cbuffer DrawConstants : register(b0) {
    uint BaseCommand;
    uint PassKind;
    uint Dithering;
    uint PerspectiveCorrect;
    uint TextureSmoothing;
    uint FootprintCoverage;
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
                (int)floor(uv.x + offset.x + 0.5),
                (int)floor(uv.y + offset.y + 0.5),
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
    float2 uv = PerspectiveCorrect != 0
        ? input.perspectiveUv
        : input.affineUv;
    float3 color = saturate(input.color.rgb);
    bool textureStp = false;
    if (textured) {
        int sampleU = screenSpace
            ? (int)floor(uv.x)
            : (int)floor(uv.x + 0.5);
        int sampleV = screenSpace
            ? (int)floor(uv.y)
            : (int)floor(uv.y + 0.5);
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
    ComPtr<ID3D11Buffer> vertex_buffer;
    UINT vertex_buffer_bytes;
    ComPtr<ID3D11Buffer> material_buffer;
    ComPtr<ID3D11ShaderResourceView> material_view;
    UINT material_buffer_bytes;
    bool material_buffer_initialized;
    std::array<ComPtr<ID3D11Buffer>, 3> constant_buffers;
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
    ComPtr<ID3D11Texture2D> vram_texture;
    ComPtr<ID3D11ShaderResourceView> vram_view;
    bool vram_texture_initialized;
    std::uint32_t output_width;
    std::uint32_t output_height;
};

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

bool ensure_frame_resources(
    BaseResources* resources,
    std::uint32_t output_width,
    std::uint32_t output_height,
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
        !resources->vertex_buffer ||
        resources->vertex_buffer_bytes < required_vertex_bytes
    ) {
        const UINT requested =
            static_cast<UINT>(required_vertex_bytes);
        const UINT doubled = resources->vertex_buffer_bytes <=
                (std::numeric_limits<UINT>::max)() / 2
            ? resources->vertex_buffer_bytes * 2
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
        resources->vertex_buffer = std::move(replacement);
        resources->vertex_buffer_bytes = capacity;
    }
    const std::size_t required_material_bytes =
        material_count * sizeof(GpuMaterial);
    if (
        required_material_bytes >
        (std::numeric_limits<UINT>::max)()
    )
        return false;
    if (
        !resources->material_buffer ||
        resources->material_buffer_bytes < required_material_bytes
    ) {
        const UINT requested = static_cast<UINT>(
            (std::max<std::size_t>)(1, material_count) *
            sizeof(GpuMaterial));
        const UINT doubled = resources->material_buffer_bytes <=
                (std::numeric_limits<UINT>::max)() / 2
            ? resources->material_buffer_bytes * 2
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
        resources->material_buffer = std::move(replacement);
        resources->material_view = std::move(replacement_view);
        resources->material_buffer_bytes = capacity;
        resources->material_buffer_initialized = false;
    }
    for (auto& constant_buffer : resources->constant_buffers) {
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
    if (!resources->vram_texture) {
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
                resources->vram_texture.GetAddressOf())) ||
            FAILED(device->CreateShaderResourceView(
                resources->vram_texture.Get(),
                nullptr,
                resources->vram_view.GetAddressOf()))
        )
            return false;
        resources->vram_texture_initialized = false;
    }
    if (
        resources->color_texture &&
        resources->output_width == output_width &&
        resources->output_height == output_height
    )
        return true;

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
    const std::uint32_t output_width =
        static_cast<std::uint32_t>(draw_list.display_width) *
        output_scale;
    const std::uint32_t output_height =
        static_cast<std::uint32_t>(draw_list.display_height) *
        output_scale;
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

    std::vector<SmoothWheel> smooth_wheels;
    try {
        smooth_wheels = build_smooth_wheels(draw_list);
    } catch (const std::bad_alloc&) {
        return WorldGpuRenderResult::resource_failed;
    }
    try {
        emit_vehicle_diagnostics(
            draw_list, smooth_wheels, options.synthetic_midpoint);
        emit_clip_rect_diagnostics(draw_list);
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
    ID3D11DeviceContext* context = base.context.Get();
    const std::size_t authored_vertex_count =
        draw_list.commands.size() * 3;
    const std::size_t vertex_count = std::max<std::size_t>(
        3,
        authored_vertex_count +
            smooth_wheels.size() * smooth_wheel_vertices);
    if (!ensure_frame_resources(
            &base,
            output_width,
            output_height,
            vertex_count,
            draw_list.commands.size() + smooth_wheels.size()))
        return WorldGpuRenderResult::resource_failed;
    const UINT staging_write = base.staging_write_index;
    const UINT staging_fill_count = base.staging_fill_count;
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
    const UINT async_staging_write = base.async_staging_write_index;
    if (
        options.asynchronous_readback &&
        base.async_staging_count >= base.async_staging_textures.size()
    )
        return WorldGpuRenderResult::resource_failed;
    if (
        !options.reuse_uploaded_vram ||
        !base.vram_texture_initialized
    ) {
        context->UpdateSubresource(
            base.vram_texture.Get(),
            0,
            nullptr,
            vram,
            1024U * sizeof(std::uint16_t),
            1024U * 512U * sizeof(std::uint16_t));
        base.vram_texture_initialized = true;
    }

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
    const UINT stride = sizeof(GpuVertex);
    const UINT offset = 0;
    ID3D11Buffer* raw_vertex_buffer = base.vertex_buffer.Get();
    context->IASetVertexBuffers(
        0, 1, &raw_vertex_buffer, &stride, &offset);
    context->VSSetShader(base.vertex_shader.Get(), nullptr, 0);
    context->PSSetShader(base.pixel_shader.Get(), nullptr, 0);
    ID3D11ShaderResourceView* shader_views[] = {
        base.vram_view.Get(),
        base.material_view.Get(),
    };
    context->PSSetShaderResources(
        0,
        static_cast<UINT>(std::size(shader_views)),
        shader_views);

    D3D11_MAPPED_SUBRESOURCE mapped_vertices{};
    if (FAILED(context->Map(
            base.vertex_buffer.Get(),
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
            const double ndc_x =
                ((projected_x - draw_list.display_x) /
                    draw_list.display_width) * 2.0 - 1.0;
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
    context->Unmap(base.vertex_buffer.Get(), 0);

    if (
        !options.reuse_uploaded_materials ||
        !base.material_buffer_initialized
    ) {
        D3D11_MAPPED_SUBRESOURCE mapped_materials{};
        if (FAILED(context->Map(
                base.material_buffer.Get(),
                0,
                D3D11_MAP_WRITE_DISCARD,
                0,
                &mapped_materials)))
            return WorldGpuRenderResult::render_failed;
        auto* gpu_materials =
            static_cast<GpuMaterial*>(mapped_materials.pData);
        for (std::size_t command_index = 0;
             command_index < draw_list.commands.size();
             ++command_index) {
            const auto& command = draw_list.commands[command_index];
            if (command.material_index >= draw_list.materials.size()) {
                context->Unmap(base.material_buffer.Get(), 0);
                return WorldGpuRenderResult::render_failed;
            }
            const auto& material =
                draw_list.materials[command.material_index];
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
            const bool opaque_track_surface =
                command.object_kind == 1U &&
                (material.primitive_flags & textured_flag) != 0 &&
                (material.primitive_flags & semi_transparent_flag) == 0 &&
                (material.primitive_flags &
                    world_primitive_screen_space_flag) == 0 &&
                std::llabs(normal_y) >= std::llabs(normal_x) &&
                std::llabs(normal_y) >= std::llabs(normal_z);
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
                    (smoothed_wheel_surface ? 4U : 0U),
            };
        }
        for (std::size_t wheel_index = 0;
             wheel_index < smooth_wheels.size();
             ++wheel_index) {
            gpu_materials[draw_list.commands.size() + wheel_index] =
                GpuMaterial{};
        }
        context->Unmap(base.material_buffer.Get(), 0);
        base.material_buffer_initialized = true;
    }

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
            {},
        };
        context->UpdateSubresource(
            base.constant_buffers[pass].Get(),
            0,
            nullptr,
            &constants,
            0,
            0);
    }

    std::uint32_t depth_object_kind =
        (std::numeric_limits<std::uint32_t>::max)();
    std::uint32_t depth_object_id =
        (std::numeric_limits<std::uint32_t>::max)();
    std::uint32_t depth_model_pointer =
        (std::numeric_limits<std::uint32_t>::max)();
    WorldViewChannel depth_channel = WorldViewChannel::main_view;
    bool depth_channel_initialized = false;
    std::int32_t depth_bucket =
        (std::numeric_limits<std::int32_t>::min)();
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
                        static_cast<std::int32_t>(output_scale)),
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
                        static_cast<std::int32_t>(output_scale)),
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
                base.constant_buffers[0].Get();
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

        const D3D11_RECT scissor{
            std::clamp(
                static_cast<LONG>(
                    (command.clip_x0 - draw_list.display_x) *
                    static_cast<std::int32_t>(output_scale)),
                0L,
                static_cast<LONG>(output_width)),
            std::clamp(
                static_cast<LONG>(
                    (command.clip_y0 - draw_list.display_y) *
                    static_cast<std::int32_t>(output_scale)),
                0L,
                static_cast<LONG>(output_height)),
            std::clamp(
                static_cast<LONG>(
                    (command.clip_x1 - draw_list.display_x + 1) *
                    static_cast<std::int32_t>(output_scale)),
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
            (
                command.object_kind != depth_object_kind ||
                !depth_channel_initialized ||
                command.channel != depth_channel ||
                (command.object_kind == 1U &&
                    command.ordering_table_index != depth_bucket) ||
                (command.object_kind != 1U &&
                    (command.object_id != depth_object_id ||
                        (command.object_kind != 2U &&
                            command.model_pointer != depth_model_pointer)))
            )
        ) {
            // Track sections in one OT layer are one coherent modern depth
            // surface. Resetting at every section/model made distant props
            // alternate ownership as section submission changed. Vehicle
            // models intentionally retain authored painter order instead:
            // wheel rims, tyre faces, and body cut-outs contain coplanar PS1
            // submeshes that a LESS_EQUAL depth surface can incorrectly mask.
            context->ClearDepthStencilView(
                base.depth_view.Get(),
                D3D11_CLEAR_DEPTH,
                1.0F,
                0);
            depth_object_kind = command.object_kind;
            depth_object_id = command.object_id;
            depth_model_pointer = command.model_pointer;
            depth_bucket = command.ordering_table_index;
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
                    base.constant_buffers[shader_pass].Get();
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

std::size_t pending_world_d3d11_readback_pairs(
    bool use_software_adapter
) noexcept {
    const auto& base = base_resources(use_software_adapter);
    return base.async_staging_count / 2U;
}

void reset_world_d3d11_readback(bool use_software_adapter) noexcept {
    auto& base = base_resources(use_software_adapter);
    base.staging_fill_count = 0;
    base.staging_write_index = 0;
    base.staging_warmup_output.clear();
    base.async_staging_read_index = 0;
    base.async_staging_write_index = 0;
    base.async_staging_count = 0;
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
