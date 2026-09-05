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
#include <map>
#include <numeric>
#include <optional>
#include <regex>
#include <set>
#include <string>
#include <thread>
#include <tuple>
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
std::uint32_t track_overlay_layer(std::uint32_t primitive_flags) noexcept {
    return
        (primitive_flags & world_primitive_track_overlay_layer_mask) >>
        world_primitive_track_overlay_layer_shift;
}

enum class WorldRenderLayer : std::size_t {
    background = 0,
    track = 1,
    vehicle = 2,
    unclassified = 3,
    screen = 4,
};

constexpr std::size_t world_render_layer_count = 5;

WorldRenderLayer world_render_layer(
    const WorldDrawCommand& command,
    const WorldMaterial& material
) noexcept {
    if ((material.primitive_flags & world_primitive_screen_space_flag) != 0)
        return WorldRenderLayer::screen;
    if (command.object_kind == 1U)
        return WorldRenderLayer::track;
    if (command.object_kind == 2U)
        return WorldRenderLayer::vehicle;
    if (command.object_kind == 3U)
        return WorldRenderLayer::background;
    return WorldRenderLayer::unclassified;
}

bool uses_modern_world_depth(
    const WorldDrawCommand& command,
    const WorldMaterial& material
) noexcept {
    const WorldRenderLayer layer = world_render_layer(command, material);
    // Exact generated-code provenance identifies GT2's authored backdrop.
    // Seattle can submit that camera-centred mesh after vehicles, with finite
    // view Z which overlaps distant road. It paints the color target without
    // reserving physical world depth and is explicitly scheduled before the
    // physical-world phase below. Unknown 3D packets remain a distinct
    // diagnostic layer and are never silently reclassified as sky/effects.
    // Explicit screen packets are the final HUD and presentation layer. Only
    // course and vehicle geometry owns the coherent modern depth surface.
    return
        layer == WorldRenderLayer::track ||
        layer == WorldRenderLayer::vehicle;
}

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
    std::int32_t minimum_authored_x{(std::numeric_limits<std::int32_t>::max)()};
    std::int32_t minimum_authored_y{(std::numeric_limits<std::int32_t>::max)()};
    std::int32_t maximum_authored_x{(std::numeric_limits<std::int32_t>::min)()};
    std::int32_t maximum_authored_y{(std::numeric_limits<std::int32_t>::min)()};
    std::int16_t minimum_model_x{(std::numeric_limits<std::int16_t>::max)()};
    std::int16_t minimum_model_y{(std::numeric_limits<std::int16_t>::max)()};
    std::int16_t minimum_model_z{(std::numeric_limits<std::int16_t>::max)()};
    std::int16_t maximum_model_x{(std::numeric_limits<std::int16_t>::min)()};
    std::int16_t maximum_model_y{(std::numeric_limits<std::int16_t>::min)()};
    std::int16_t maximum_model_z{(std::numeric_limits<std::int16_t>::min)()};
    float maximum_view_residual{};
    float maximum_projection_residual{};
    std::int32_t minimum_clip_x0{(std::numeric_limits<std::int32_t>::max)()};
    std::int32_t minimum_clip_y0{(std::numeric_limits<std::int32_t>::max)()};
    std::int32_t maximum_clip_x1{(std::numeric_limits<std::int32_t>::min)()};
    std::int32_t maximum_clip_y1{(std::numeric_limits<std::int32_t>::min)()};
    std::size_t commands_with_visible_scissor{};
    std::size_t textured_commands{};
    std::size_t semi_transparent_commands{};
    std::int16_t transform_rotation[9]{};
    std::int32_t transform_translation[3]{};
    bool exact_transform_valid{};
    bool authored_shadow{};
    bool authored_wheel{};
};

struct VehicleDiagnosticGeometry {
    std::array<std::int16_t, 9> model{};
    std::size_t opaque_textured{};
    std::size_t semi_textured{};
    std::size_t other{};
};

struct VehicleDiagnosticMaterial {
    std::uint32_t primitive_flags{};
    std::uint16_t texture_page{};
    std::uint16_t clut{};
    std::size_t commands{};
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

using VehicleModelTriangle =
    std::array<std::array<std::int16_t, 3>, 3>;

VehicleModelTriangle vehicle_model_triangle(
    const WorldDrawCommand& command
) noexcept {
    VehicleModelTriangle triangle{};
    for (std::size_t index = 0; index < triangle.size(); ++index) {
        triangle[index] = {
            command.vertices[index].model_x,
            command.vertices[index].model_y,
            command.vertices[index].model_z,
        };
    }
    std::sort(triangle.begin(), triangle.end());
    return triangle;
}

struct VehicleReflectionGroup {
    std::uint32_t object_id{};
    std::uint32_t model_pointer{};
    std::uint64_t transform_id{};
    WorldViewChannel channel{};
    std::uint32_t material_index{};
    std::size_t commands{};
    std::size_t paired_commands{};
};

bool same_vehicle_reflection_group(
    const VehicleReflectionGroup& group,
    const WorldDrawCommand& command
) noexcept {
    return
        command.object_kind == 2U &&
        group.object_id == command.object_id &&
        group.model_pointer == command.model_pointer &&
        group.transform_id == command.transform_id &&
        group.channel == command.channel;
}

struct VehicleReflectionEligibility {
    std::vector<std::uint8_t> details;
    std::vector<std::uint8_t> supports;
};

VehicleReflectionEligibility vehicle_reflection_eligibility(
    const WorldDrawList& draw_list
) {
    VehicleReflectionEligibility result{
        std::vector<std::uint8_t>(draw_list.commands.size()),
        std::vector<std::uint8_t>(draw_list.commands.size()),
    };
    using BodyKey = std::tuple<std::uint32_t, std::uint32_t, std::uint64_t,
        WorldViewChannel, VehicleModelTriangle>;
    const auto body_key = [] (const WorldDrawCommand& command) {
        return BodyKey{command.object_id, command.model_pointer,
            command.transform_id, command.channel, vehicle_model_triangle(command)};
    };
    // Index once: close-up subdivision must not turn each reflection lookup
    // into another complete body scan (and repeated triangle sorting).
    std::set<BodyKey> opaque_triangles;
    for (const auto& command : draw_list.commands) {
        if (command.object_kind != 2U || command.material_index >= draw_list.materials.size())
            continue;
        const auto flags = draw_list.materials[command.material_index].primitive_flags;
        if ((flags & textured_flag) != 0 && (flags & semi_transparent_flag) == 0)
            opaque_triangles.insert(body_key(command));
    }
    std::vector<VehicleReflectionGroup> groups;
    for (std::size_t command_index = 0;
         command_index < draw_list.commands.size();
         ++command_index) {
        const auto& command = draw_list.commands[command_index];
        if (
            command.object_kind != 2U ||
            command.material_index >= draw_list.materials.size()
        )
            continue;
        const auto& material = draw_list.materials[command.material_index];
        const bool textured =
            (material.primitive_flags & textured_flag) != 0;
        const bool semitransparent =
            (material.primitive_flags & semi_transparent_flag) != 0;
        const bool raw_texture =
            (material.primitive_flags & 4U) != 0;
        const std::uint32_t blend_mode =
            (material.texture_page >> 5U) & 3U;
        if (
            !textured || !semitransparent || raw_texture || blend_mode != 1U
        )
            continue;

        auto group = std::find_if(
            groups.begin(), groups.end(),
            [&] (const VehicleReflectionGroup& candidate) {
                return
                    same_vehicle_reflection_group(candidate, command) &&
                    candidate.material_index == command.material_index;
            });
        if (group == groups.end()) {
            groups.push_back(VehicleReflectionGroup{
                command.object_id,
                command.model_pointer,
                command.transform_id,
                command.channel,
                command.material_index,
            });
            group = groups.end() - 1;
        }
        ++group->commands;
        const bool paired = opaque_triangles.find(body_key(command)) != opaque_triangles.end();
        if (paired)
            ++group->paired_commands;
    }

    groups.erase(
        std::remove_if(
            groups.begin(), groups.end(),
            [] (const VehicleReflectionGroup& group) {
                // Broad stock environment layers duplicate much of the opaque
                // body mesh. Small additive lamps and trim do not. Requiring
                // several exact source-triangle pairs makes the distinction
                // from geometry/provenance rather than a car-specific texture
                // page, palette, model pointer, or command range.
                // Near-camera adaptive subdivision expands only the base
                // mesh. Its exact-pair percentage therefore changes with the
                // camera even though this is still the same material. Keep
                // the positive geometric evidence; do not gate it on a ratio
                // of the current tessellations (23/148 on the close roof).
                return group.paired_commands < 8U;
            }),
        groups.end());

    for (std::size_t command_index = 0;
         command_index < draw_list.commands.size();
         ++command_index) {
        const auto& command = draw_list.commands[command_index];
        if (command.material_index >= draw_list.materials.size())
            continue;
        const bool reflection_group = std::any_of(
            groups.begin(), groups.end(),
            [&] (const VehicleReflectionGroup& candidate) {
                return same_vehicle_reflection_group(candidate, command);
            });
        if (!reflection_group)
            continue;
        const bool reflection_material = std::any_of(
            groups.begin(), groups.end(),
            [&] (const VehicleReflectionGroup& candidate) {
                return
                    same_vehicle_reflection_group(candidate, command) &&
                    candidate.material_index == command.material_index;
            });
        const auto& material = draw_list.materials[command.material_index];
        const bool textured =
            (material.primitive_flags & textured_flag) != 0;
        const bool semitransparent =
            (material.primitive_flags & semi_transparent_flag) != 0;
        result.details[command_index] =
            reflection_material && textured && semitransparent
            ? 1U : 0U;
        result.supports[command_index] =
            textured && !semitransparent ? 1U : 0U;
    }
    return result;
}

// GT2 subdivides an opaque quad bilinearly near the camera, but can keep its
// environment quad as two flat triangles. Transfer that quad's UV/color field
// to the authored body triangles only when they demonstrably tile the SAME
// bilinear patch. No depth tolerance or replacement body geometry is needed.
std::optional<WorldDrawList> conform_vehicle_reflection_quads(
    const WorldDrawList& source,
    const VehicleReflectionEligibility& eligibility,
    VehicleReflectionEligibility* output_eligibility
) {
    std::vector<std::vector<WorldDrawCommand>> replacements(source.commands.size());
    std::vector<bool> removed(source.commands.size());
    bool changed = false;
    const auto position = [] (const WorldDrawVertex& vertex) {
        return std::array<double, 3>{
            static_cast<double>(vertex.model_x),
            static_cast<double>(vertex.model_y),
            static_cast<double>(vertex.model_z)};
    };
    for (std::size_t index = 0; index + 1 < source.commands.size(); ++index) {
        if (!eligibility.details[index] || !eligibility.details[index+1] || removed[index])
            continue;
        const auto& first = source.commands[index];
        const auto& second = source.commands[index+1];
        if (!first.exact_transform_valid || !second.exact_transform_valid ||
            first.material_index != second.material_index ||
            first.object_id != second.object_id || first.model_pointer != second.model_pointer ||
            first.transform_id != second.transform_id || first.channel != second.channel ||
            first.ordering_table_index != second.ordering_table_index)
            continue;
        std::array<int, 2> shared{};
        int shared_count = 0, unique_first = -1, unique_second = -1;
        bool seam = false;
        for (int a = 0; a < 3; ++a) {
            int match = -1;
            for (int b = 0; b < 3; ++b)
                if (position(first.vertices[a]) == position(second.vertices[b])) {
                    match = b;
                    const auto& left = first.vertices[a];
                    const auto& right = second.vertices[b];
                    seam |= left.u != right.u || left.v != right.v ||
                        left.r != right.r || left.g != right.g || left.b != right.b;
                }
            if (match < 0)
                unique_first = a;
            else if (shared_count < 2)
                shared[shared_count++] = a;
            else
                seam = true;
        }
        for (int b = 0; b < 3; ++b) {
            bool found = false;
            for (const auto& vertex : first.vertices)
                found |= position(vertex) == position(second.vertices[b]);
            if (!found)
                unique_second = b;
        }
        if (seam || shared_count != 2 || unique_first < 0 || unique_second < 0)
            continue;
        const std::array<WorldDrawVertex, 4> corners{
            first.vertices[unique_first], first.vertices[shared[0]],
            first.vertices[shared[1]], second.vertices[unique_second]};
        std::array<std::array<double, 3>, 4> p{};
        for (int corner = 0; corner < 4; ++corner)
            p[corner] = position(corners[corner]);
        const std::array<double, 3> edge_u{
            p[1][0]-p[0][0], p[1][1]-p[0][1], p[1][2]-p[0][2]};
        const std::array<double, 3> edge_v{
            p[2][0]-p[0][0], p[2][1]-p[0][1], p[2][2]-p[0][2]};
        const std::array<double, 3> normal{
            edge_u[1]*edge_v[2]-edge_u[2]*edge_v[1],
            edge_u[2]*edge_v[0]-edge_u[0]*edge_v[2],
            edge_u[0]*edge_v[1]-edge_u[1]*edge_v[0]};
        const double nonplanarity = normal[0]*(p[3][0]-p[0][0]) +
            normal[1]*(p[3][1]-p[0][1]) + normal[2]*(p[3][2]-p[0][2]);
        // Flat patches already share an exact depth plane. Preserve their
        // authored triangle UV interpolation and avoid needless subdivision.
        if (nonplanarity == 0.0)
            continue;
        std::array<double, 3> minimum = p[0], maximum = p[0];
        for (const auto& point : p)
            for (int axis = 0; axis < 3; ++axis) {
                minimum[axis] = (std::min)(minimum[axis], point[axis]);
                maximum[axis] = (std::max)(maximum[axis], point[axis]);
            }
        const auto coordinates = [&] (const WorldDrawVertex& vertex,
                                      std::array<double, 2>* uv) {
            const auto target = position(vertex);
            for (int axis = 0; axis < 3; ++axis)
                if (target[axis] < minimum[axis]-2.0 || target[axis] > maximum[axis]+2.0)
                    return false;
            double u = 0.5, v = 0.5;
            for (int iteration = 0; iteration < 8; ++iteration) {
                double uu = 0, vv = 0, uv_product = 0, ur = 0, vr = 0;
                for (int axis = 0; axis < 3; ++axis) {
                    const double twist = p[3][axis]-p[1][axis]-p[2][axis]+p[0][axis];
                    const double du = p[1][axis]-p[0][axis]+v*twist;
                    const double dv = p[2][axis]-p[0][axis]+u*twist;
                    const double residual = target[axis] -
                        (p[0][axis]+u*(p[1][axis]-p[0][axis])+v*(p[2][axis]-p[0][axis])+u*v*twist);
                    uu += du*du; vv += dv*dv; uv_product += du*dv;
                    ur += du*residual; vr += dv*residual;
                }
                const double determinant = uu*vv-uv_product*uv_product;
                if (determinant < 1.0e-8)
                    return false;
                u += (ur*vv-vr*uv_product)/determinant;
                v += (vr*uu-ur*uv_product)/determinant;
            }
            if (!std::isfinite(u) || !std::isfinite(v) ||
                u < -0.001 || u > 1.001 || v < -0.001 || v > 1.001)
                return false;
            u = std::clamp(u, 0.0, 1.0); v = std::clamp(v, 0.0, 1.0);
            for (int axis = 0; axis < 3; ++axis) {
                const double reconstructed = (1-u)*(1-v)*p[0][axis] +
                    u*(1-v)*p[1][axis] + (1-u)*v*p[2][axis] + u*v*p[3][axis];
                // Authored subdivided coordinates are signed integer model
                // units; allow only their truncation/rounding envelope.
                if (std::abs(reconstructed-target[axis]) > 2.0)
                    return false;
            }
            *uv = {u, v};
            return true;
        };
        double covered_area = 0.0;
        std::vector<WorldDrawCommand> conformed;
        for (std::size_t body_index = 0; body_index < source.commands.size(); ++body_index) {
            if (!eligibility.supports[body_index])
                continue;
            const auto& body = source.commands[body_index];
            if (!body.exact_transform_valid || body.object_id != first.object_id ||
                body.model_pointer != first.model_pointer || body.transform_id != first.transform_id ||
                body.channel != first.channel || body.clip_x0 != first.clip_x0 ||
                body.clip_y0 != first.clip_y0 || body.clip_x1 != first.clip_x1 || body.clip_y1 != first.clip_y1)
                continue;
            std::array<std::array<double, 2>, 3> uv{};
            if (!coordinates(body.vertices[0], &uv[0]) ||
                !coordinates(body.vertices[1], &uv[1]) || !coordinates(body.vertices[2], &uv[2]))
                continue;
            const double area = std::abs((uv[1][0]-uv[0][0])*(uv[2][1]-uv[0][1]) -
                (uv[1][1]-uv[0][1])*(uv[2][0]-uv[0][0]))*0.5;
            if (area < 1.0e-8)
                continue;
            auto detail = body;
            detail.material_index = first.material_index;
            detail.source_command_index = first.source_command_index;
            detail.ordering_table_index = first.ordering_table_index;
            for (int corner = 0; corner < 3; ++corner) {
                const double u = uv[corner][0], v = uv[corner][1];
                const std::array<double, 4> weights{(1-u)*(1-v), u*(1-v), (1-u)*v, u*v};
                double tex_u = 0, tex_v = 0, red = 0, green = 0, blue = 0;
                for (int sample = 0; sample < 4; ++sample) {
                    tex_u += weights[sample]*corners[sample].u;
                    tex_v += weights[sample]*corners[sample].v;
                    red += weights[sample]*corners[sample].r;
                    green += weights[sample]*corners[sample].g;
                    blue += weights[sample]*corners[sample].b;
                }
                auto& vertex = detail.vertices[corner];
                vertex.u = static_cast<float>(tex_u); vertex.v = static_cast<float>(tex_v);
                vertex.r = static_cast<std::uint8_t>(std::clamp(std::round(red), 0.0, 255.0));
                vertex.g = static_cast<std::uint8_t>(std::clamp(std::round(green), 0.0, 255.0));
                vertex.b = static_cast<std::uint8_t>(std::clamp(std::round(blue), 0.0, 255.0));
            }
            covered_area += area;
            conformed.push_back(detail);
        }
        // Partial coverage or overlapping body layers are not evidence that
        // this mesh is a subdivision of the entire reflection patch.
        if (conformed.size() <= 2 || std::abs(covered_area-1.0) > 0.01)
            continue;
        replacements[index] = std::move(conformed);
        removed[index+1] = true;
        changed = true;
        ++index;
    }
    if (!changed)
        return std::nullopt;
    WorldDrawList result = source;
    result.commands.clear();
    for (std::size_t index = 0; index < source.commands.size(); ++index) {
        if (removed[index])
            continue;
        if (replacements[index].empty())
        {
            result.commands.push_back(source.commands[index]);
            output_eligibility->details.push_back(eligibility.details[index]);
            output_eligibility->supports.push_back(eligibility.supports[index]);
        }
        else
        {
            result.commands.insert(result.commands.end(), replacements[index].begin(), replacements[index].end());
            output_eligibility->details.insert(output_eligibility->details.end(), replacements[index].size(), 1U);
            output_eligibility->supports.insert(output_eligibility->supports.end(), replacements[index].size(), 0U);
        }
    }
    result.vehicle_commands += static_cast<std::uint32_t>(result.commands.size()-source.commands.size());
    return result;
}

// Rasterizers quantize triangle XY independently. Even an exact shared model
// plane can consequently produce different interpolated depth on its large
// support and small detail triangles. Evaluate a shared plane at pixel centers
// instead of biasing the detail toward the camera.
std::vector<std::array<float, 4>> shared_surface_depth_planes(
    const WorldDrawList& draw_list,
    const VehicleReflectionEligibility& reflections,
    float horizontal_scale,
    std::uint32_t width,
    std::uint32_t height
) {
    using Key = std::tuple<std::uint32_t, std::uint32_t, std::uint32_t,
        std::uint64_t, WorldViewChannel, std::array<std::int64_t, 4>>;
    struct Group {
        std::vector<std::size_t> commands;
        std::array<double, 3> plane{};
        double area{};
        bool detail{};
    };
    std::map<Key, Group> groups;
    std::vector<std::array<float, 4>> result(draw_list.commands.size());
    for (std::size_t index = 0; index < draw_list.commands.size(); ++index) {
        const auto& command = draw_list.commands[index];
        if ((command.object_kind != 1U && command.object_kind != 2U) ||
            !command.exact_transform_valid ||
            command.material_index >= draw_list.materials.size())
            continue;
        const auto& material = draw_list.materials[command.material_index];
        if ((material.primitive_flags & world_primitive_screen_space_flag) != 0)
            continue;
        const auto& a = command.vertices[0];
        const auto& b = command.vertices[1];
        const auto& c = command.vertices[2];
        const std::array<std::int64_t, 3> u{
            b.model_x - a.model_x, b.model_y - a.model_y, b.model_z - a.model_z};
        const std::array<std::int64_t, 3> v{
            c.model_x - a.model_x, c.model_y - a.model_y, c.model_z - a.model_z};
        std::array<std::int64_t, 4> model_plane{
            u[1]*v[2] - u[2]*v[1], u[2]*v[0] - u[0]*v[2],
            u[0]*v[1] - u[1]*v[0], 0};
        const auto divisor = std::gcd(std::gcd(model_plane[0], model_plane[1]), model_plane[2]);
        if (divisor == 0)
            continue;
        for (std::size_t axis = 0; axis < 3; ++axis)
            model_plane[axis] /= divisor;
        for (std::size_t axis = 0; axis < 3; ++axis) {
            if (model_plane[axis] == 0)
                continue;
            if (model_plane[axis] < 0)
                for (auto& value : model_plane) value = -value;
            break;
        }
        model_plane[3] = -(model_plane[0]*a.model_x +
            model_plane[1]*a.model_y + model_plane[2]*a.model_z);
        std::array<std::array<double, 3>, 3> projected{};
        bool valid = true;
        for (std::size_t corner = 0; corner < 3; ++corner) {
            const auto& point = command.vertices[corner];
            if (!std::isfinite(point.clip_w) || std::abs(point.clip_w) < 1.0e-10) {
                valid = false;
                break;
            }
            projected[corner] = {
                (static_cast<double>(point.clip_x * horizontal_scale) / point.clip_w * 0.5 + 0.5) * width,
                (-static_cast<double>(point.clip_y) / point.clip_w * 0.5 + 0.5) * height,
                static_cast<double>(point.clip_z) / point.clip_w};
        }
        if (!valid)
            continue;
        const auto& p = projected[0];
        const auto& q = projected[1];
        const auto& r = projected[2];
        const double determinant = (q[0]-p[0])*(r[1]-p[1]) - (r[0]-p[0])*(q[1]-p[1]);
        if (!std::isfinite(determinant) || std::abs(determinant) < 1.0e-6)
            continue;
        auto& group = groups[Key{command.object_kind, command.object_id,
            command.model_pointer, command.transform_id, command.channel, model_plane}];
        group.commands.push_back(index);
        group.detail |= command.object_kind == 1U
            ? track_overlay_layer(material.primitive_flags) != 0
            : index < reflections.details.size() && reflections.details[index] != 0;
        if (std::abs(determinant) > group.area) {
            group.area = std::abs(determinant);
            const double dx = ((q[2]-p[2])*(r[1]-p[1]) - (r[2]-p[2])*(q[1]-p[1])) / determinant;
            const double dy = ((q[0]-p[0])*(r[2]-p[2]) - (r[0]-p[0])*(q[2]-p[2])) / determinant;
            group.plane = {dx, dy, p[2] - dx*p[0] - dy*p[1]};
        }
    }
    for (const auto& [key, group] : groups) {
        if (!group.detail || group.commands.size() < 2)
            continue;
        // Reject inconsistent projection/provenance rather than flattening
        // genuinely separated geometry onto an inferred shared surface.
        bool consistent = true;
        for (const auto index : group.commands) {
            for (const auto& point : draw_list.commands[index].vertices) {
                const double x = (static_cast<double>(point.clip_x * horizontal_scale) / point.clip_w * 0.5 + 0.5) * width;
                const double y = (-static_cast<double>(point.clip_y) / point.clip_w * 0.5 + 0.5) * height;
                const double z = static_cast<double>(point.clip_z) / point.clip_w;
                const double evaluated = group.plane[0]*x + group.plane[1]*y + group.plane[2];
                if (!std::isfinite(evaluated) || std::abs(evaluated-z) > std::abs(z)*1.0e-5)
                    consistent = false;
            }
        }
        if (!consistent)
            continue;
        for (const auto index : group.commands)
            result[index] = {static_cast<float>(group.plane[0]),
                static_cast<float>(group.plane[1]), static_cast<float>(group.plane[2]), 1.0F};
    }
    return result;
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
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_VEHICLE_DIAGNOSTICS_START_POLL")) {
        const long start_poll = std::strtol(configured, nullptr, 10);
        if (draw_list.input_poll < start_poll)
            return;
    }
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_VEHICLE_DIAGNOSTICS_END_POLL")) {
        const long end_poll = std::strtol(configured, nullptr, 10);
        if (draw_list.input_poll > end_poll)
            return;
    }
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
    bool filter_object = false;
    std::uint32_t requested_object = 0;
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_VEHICLE_DIAGNOSTICS_OBJECT")) {
        requested_object = static_cast<std::uint32_t>(
            std::strtoul(configured, nullptr, 0));
        filter_object = true;
    }
    for (std::size_t command_index = 0;
         command_index < draw_list.commands.size();
         ++command_index) {
        const auto& command = draw_list.commands[command_index];
        if (command.object_kind != 2U ||
            (filter_object && command.object_id != requested_object))
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
            for (int component = 0; component < 9; ++component)
                found->transform_rotation[component] =
                    command.transform_rotation[component];
            for (int component = 0; component < 3; ++component)
                found->transform_translation[component] =
                    command.transform_translation[component];
        } else {
            found->exact_transform_valid =
                found->exact_transform_valid &&
                command.exact_transform_valid;
        }
        if (command.material_index < draw_list.materials.size()) {
            const auto& material =
                draw_list.materials[command.material_index];
            found->textured_commands +=
                (material.primitive_flags & textured_flag) != 0 ? 1U : 0U;
            found->semi_transparent_commands +=
                (material.primitive_flags & semi_transparent_flag) != 0
                    ? 1U
                    : 0U;
            found->authored_shadow = found->authored_shadow ||
                soft_vehicle_shadow(command, material);
            found->authored_wheel = found->authored_wheel ||
                vehicle_wheel_tread(command, material);
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
            found->minimum_authored_x = (std::min)(
                found->minimum_authored_x, vertex.authored_screen_x);
            found->minimum_authored_y = (std::min)(
                found->minimum_authored_y, vertex.authored_screen_y);
            found->maximum_authored_x = (std::max)(
                found->maximum_authored_x, vertex.authored_screen_x);
            found->maximum_authored_y = (std::max)(
                found->maximum_authored_y, vertex.authored_screen_y);
            found->minimum_model_x = (std::min)(
                found->minimum_model_x, vertex.model_x);
            found->minimum_model_y = (std::min)(
                found->minimum_model_y, vertex.model_y);
            found->minimum_model_z = (std::min)(
                found->minimum_model_z, vertex.model_z);
            found->maximum_model_x = (std::max)(
                found->maximum_model_x, vertex.model_x);
            found->maximum_model_y = (std::max)(
                found->maximum_model_y, vertex.model_y);
            found->maximum_model_z = (std::max)(
                found->maximum_model_z, vertex.model_z);
            found->maximum_view_residual = (std::max)(
                found->maximum_view_residual,
                (std::max)({
                    std::abs(vertex.view_x - vertex.exact_view_x),
                    std::abs(vertex.view_y - vertex.exact_view_y),
                    std::abs(vertex.view_z - vertex.exact_view_z)}));
            found->maximum_projection_residual = (std::max)(
                found->maximum_projection_residual,
                (std::max)(
                    std::abs(vertex.screen_x - vertex.authored_screen_x),
                    std::abs(vertex.screen_y - vertex.authored_screen_y)));
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
        const bool diagnostic_shell = std::any_of(
            smooth_wheels.begin(), smooth_wheels.end(),
            [&] (const SmoothWheel& candidate) {
                return
                    candidate.object_id == group.object_id &&
                    candidate.model_pointer == group.model_pointer &&
                    candidate.transform_id == group.transform_id &&
                    candidate.channel == group.channel;
            });
        std::vector<VehicleDiagnosticGeometry> geometries;
        std::vector<VehicleDiagnosticMaterial> materials;
        geometries.reserve(group.commands);
        materials.reserve(16);
        for (const auto& command : draw_list.commands) {
            if (!same_vehicle_diagnostic_group(group, command) ||
                command.material_index >= draw_list.materials.size())
                continue;
            const auto& material =
                draw_list.materials[command.material_index];
            std::array<std::int16_t, 9> model{};
            for (int vertex = 0; vertex < 3; ++vertex) {
                model[vertex * 3] = command.vertices[vertex].model_x;
                model[vertex * 3 + 1] = command.vertices[vertex].model_y;
                model[vertex * 3 + 2] = command.vertices[vertex].model_z;
            }
            auto geometry = std::find_if(
                geometries.begin(), geometries.end(),
                [&] (const VehicleDiagnosticGeometry& candidate) {
                    return candidate.model == model;
                });
            if (geometry == geometries.end()) {
                geometries.push_back(VehicleDiagnosticGeometry{});
                geometry = geometries.end() - 1;
                geometry->model = model;
            }
            const bool textured =
                (material.primitive_flags & textured_flag) != 0;
            const bool semi =
                (material.primitive_flags & semi_transparent_flag) != 0;
            if (textured && semi)
                ++geometry->semi_textured;
            else if (textured)
                ++geometry->opaque_textured;
            else
                ++geometry->other;

            auto material_group = std::find_if(
                materials.begin(), materials.end(),
                [&] (const VehicleDiagnosticMaterial& candidate) {
                    return
                        candidate.primitive_flags == material.primitive_flags &&
                        candidate.texture_page == material.texture_page &&
                        candidate.clut == material.clut;
                });
            if (material_group == materials.end()) {
                materials.push_back(VehicleDiagnosticMaterial{
                    material.primitive_flags,
                    material.texture_page,
                    material.clut,
                    0});
                material_group = materials.end() - 1;
            }
            ++material_group->commands;
        }
        std::size_t paired_textured_geometry = 0;
        std::size_t opaque_only_geometry = 0;
        std::size_t semi_only_geometry = 0;
        std::size_t other_geometry = 0;
        for (const auto& geometry : geometries) {
            if (geometry.opaque_textured != 0 &&
                geometry.semi_textured != 0)
                ++paired_textured_geometry;
            else if (geometry.opaque_textured != 0)
                ++opaque_only_geometry;
            else if (geometry.semi_textured != 0)
                ++semi_only_geometry;
            else
                ++other_geometry;
        }
        std::fprintf(
            stderr,
            "[Render-Vehicle-Group] sample=%s object=%u model=%08x "
            "transform=%016llx channel=%u role=%s commands=%zu "
            "textured=%zu/%zu semi=%zu/%zu "
            "range=%zu-%zu exact=%u visible=%zu/%zu front=%zu/%zu "
            "screen=%.1f,%.1f..%.1f,%.1f viewZ=%.1f..%.1f "
            "authored=%d,%d..%d,%d model=%d,%d,%d..%d,%d,%d "
            "residual=view:%.3f,projection:%.3f "
            "clipW=%.1f..%.1f scissor=%d,%d..%d,%d visibleScissor=%zu/%zu "
            "display=%d,%d..%d,%d rt=[%d,%d,%d;%d,%d,%d;%d,%d,%d] "
            "t=[%d,%d,%d] geometry=%zu paired=%zu opaqueOnly=%zu "
            "semiOnly=%zu other=%zu\n",
            synthetic_midpoint ? "midpoint" : "actual",
            group.object_id,
            group.model_pointer,
            static_cast<unsigned long long>(group.transform_id),
            static_cast<unsigned>(group.channel),
            group.authored_shadow
                ? "authored-shadow"
                : group.authored_wheel
                    ? "authored-wheel"
                    : diagnostic_shell ? "diagnostic-wheel" : "body/part",
            group.commands,
            group.textured_commands,
            group.commands,
            group.semi_transparent_commands,
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
            group.minimum_authored_x,
            group.minimum_authored_y,
            group.maximum_authored_x,
            group.maximum_authored_y,
            group.minimum_model_x,
            group.minimum_model_y,
            group.minimum_model_z,
            group.maximum_model_x,
            group.maximum_model_y,
            group.maximum_model_z,
            group.maximum_view_residual,
            group.maximum_projection_residual,
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
            draw_list.display_y + draw_list.display_height,
            group.transform_rotation[0],
            group.transform_rotation[1],
            group.transform_rotation[2],
            group.transform_rotation[3],
            group.transform_rotation[4],
            group.transform_rotation[5],
            group.transform_rotation[6],
            group.transform_rotation[7],
            group.transform_rotation[8],
            group.transform_translation[0],
            group.transform_translation[1],
            group.transform_translation[2],
            geometries.size(),
            paired_textured_geometry,
            opaque_only_geometry,
            semi_only_geometry,
            other_geometry);
        for (const auto& material : materials) {
            std::fprintf(
                stderr,
                "[Render-Vehicle-Material] sample=%s object=%u "
                "model=%08x transform=%016llx flags=%08x tpage=%04x "
                "clut=%04x commands=%zu\n",
                synthetic_midpoint ? "midpoint" : "actual",
                group.object_id,
                group.model_pointer,
                static_cast<unsigned long long>(group.transform_id),
                material.primitive_flags,
                material.texture_page,
                material.clut,
                material.commands);
        }
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
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_CLIP_DIAGNOSTICS_START_POLL")) {
        const long start_poll = std::strtol(configured, nullptr, 10);
        if (draw_list.input_poll < start_poll)
            return;
    }
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_CLIP_DIAGNOSTICS_END_POLL")) {
        const long end_poll = std::strtol(configured, nullptr, 10);
        if (draw_list.input_poll > end_poll)
            return;
    }
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
        std::array<std::size_t, 4> kinds{};
        float minimum_projection_plane{
            std::numeric_limits<float>::infinity()};
        float maximum_projection_plane{
            -std::numeric_limits<float>::infinity()};
        float minimum_projection_offset_x{
            std::numeric_limits<float>::infinity()};
        float maximum_projection_offset_x{
            -std::numeric_limits<float>::infinity()};
        float minimum_projection_offset_y{
            std::numeric_limits<float>::infinity()};
        float maximum_projection_offset_y{
            -std::numeric_limits<float>::infinity()};
        float minimum_draw_offset_x{
            std::numeric_limits<float>::infinity()};
        float maximum_draw_offset_x{
            -std::numeric_limits<float>::infinity()};
        float minimum_draw_offset_y{
            std::numeric_limits<float>::infinity()};
        float maximum_draw_offset_y{
            -std::numeric_limits<float>::infinity()};
        std::size_t exact_transform_commands{};
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
        ++found->kinds[command.object_kind <= 3U ? command.object_kind : 0U];
        found->exact_transform_commands +=
            command.exact_transform_valid ? 1U : 0U;
        for (const auto& vertex : command.vertices) {
            found->minimum_projection_plane = (std::min)(
                found->minimum_projection_plane,
                vertex.projection_plane);
            found->maximum_projection_plane = (std::max)(
                found->maximum_projection_plane,
                vertex.projection_plane);
            found->minimum_projection_offset_x = (std::min)(
                found->minimum_projection_offset_x,
                vertex.projection_offset_x);
            found->maximum_projection_offset_x = (std::max)(
                found->maximum_projection_offset_x,
                vertex.projection_offset_x);
            found->minimum_projection_offset_y = (std::min)(
                found->minimum_projection_offset_y,
                vertex.projection_offset_y);
            found->maximum_projection_offset_y = (std::max)(
                found->maximum_projection_offset_y,
                vertex.projection_offset_y);
            found->minimum_draw_offset_x = (std::min)(
                found->minimum_draw_offset_x,
                vertex.draw_offset_x);
            found->maximum_draw_offset_x = (std::max)(
                found->maximum_draw_offset_x,
                vertex.draw_offset_x);
            found->minimum_draw_offset_y = (std::min)(
                found->minimum_draw_offset_y,
                vertex.draw_offset_y);
            found->maximum_draw_offset_y = (std::max)(
                found->maximum_draw_offset_y,
                vertex.draw_offset_y);
        }
    }
    std::fprintf(
        stderr,
        "[Render-Clip-Rects] frame=%llu poll=%d rects=%zu commands=%zu "
        "rejectedIncomplete=%u "
        "rejectedOversized=%u incTrack=%u incVehicle=%u "
        "screenTargetDrop=%u/%u secondary=%u display=%d,%d..%d,%d\n",
        static_cast<unsigned long long>(draw_list.frame_index),
        draw_list.input_poll,
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
            "unclassified=%zu track=%zu vehicle=%zu background=%zu "
            "exact=%zu/%zu "
            "projectionH=%.0f..%.0f projectionOffset=%.0f..%.0f,"
            "%.0f..%.0f drawOffset=%.0f..%.0f,%.0f..%.0f\n",
            static_cast<unsigned>(group.channel),
            group.x0,
            group.y0,
            group.x1,
            group.y1,
            group.commands,
            group.kinds[0],
            group.kinds[1],
            group.kinds[2],
            group.kinds[3],
            group.exact_transform_commands,
            group.commands,
            group.minimum_projection_plane,
            group.maximum_projection_plane,
            group.minimum_projection_offset_x,
            group.maximum_projection_offset_x,
            group.minimum_projection_offset_y,
            group.maximum_projection_offset_y,
            group.minimum_draw_offset_x,
            group.maximum_draw_offset_x,
            group.minimum_draw_offset_y,
            group.maximum_draw_offset_y);
    }
}

// Projection failures are often intermittent because a single vertex crosses
// a clip plane or a large authored triangle changes its raster footprint by a
// fraction of a pixel. Keep this opt-in: at interval 1 it deliberately emits
// enough provenance to correlate a visible frame with the exact command.
struct DiagnosticClipVertex {
    double x{};
    double y{};
    double z{};
    double w{};
    // D3D clips vertex attributes in homogeneous space before applying the
    // ordinary perspective interpolation used by the pixel shader. Retaining
    // these values lets pixel provenance reproduce a near-plane crossing
    // triangle instead of silently dropping it from the ownership report.
    double u{};
    double v{};
    double r{};
    double g{};
    double b{};
};

struct DiagnosticClipResult {
    std::array<DiagnosticClipVertex, 16> vertices{};
    std::size_t vertex_count{};
    bool finite{true};
    bool bounded{true};
    bool clipped{};
    double minimum_ndc_x{1.0};
    double minimum_ndc_y{1.0};
    double maximum_ndc_x{-1.0};
    double maximum_ndc_y{-1.0};
    double ndc_area{};
};

double diagnostic_clip_distance(
    const DiagnosticClipVertex& vertex,
    std::size_t plane
) noexcept {
    switch (plane) {
        case 0: return vertex.x + vertex.w;
        case 1: return vertex.w - vertex.x;
        case 2: return vertex.y + vertex.w;
        case 3: return vertex.w - vertex.y;
        case 4: return vertex.z;
        case 5: return vertex.w - vertex.z;
        default: return -1.0;
    }
}

DiagnosticClipVertex diagnostic_clip_lerp(
    const DiagnosticClipVertex& first,
    const DiagnosticClipVertex& second,
    double amount
) noexcept {
    return {
        first.x + (second.x - first.x) * amount,
        first.y + (second.y - first.y) * amount,
        first.z + (second.z - first.z) * amount,
        first.w + (second.w - first.w) * amount,
        first.u + (second.u - first.u) * amount,
        first.v + (second.v - first.v) * amount,
        first.r + (second.r - first.r) * amount,
        first.g + (second.g - first.g) * amount,
        first.b + (second.b - first.b) * amount,
    };
}

DiagnosticClipResult diagnostic_homogeneous_clip(
    const WorldDrawCommand& command,
    double horizontal_projection_scale
) noexcept {
    // Mirror D3D's six homogeneous clip inequalities after the renderer's
    // Hor+ X adjustment. A triangle can grow by at most one vertex per plane;
    // sixteen slots therefore leave comfortable room without per-command
    // allocation in a verbose whole-course trace.
    std::array<DiagnosticClipVertex, 16> first{};
    std::array<DiagnosticClipVertex, 16> second{};
    std::size_t count = 3;
    for (std::size_t index = 0; index < count; ++index) {
        const auto& source = command.vertices[index];
        first[index] = {
            source.clip_x * horizontal_projection_scale,
            source.clip_y,
            source.clip_z,
            source.clip_w,
            source.u,
            source.v,
            static_cast<double>(source.r),
            static_cast<double>(source.g),
            static_cast<double>(source.b),
        };
    }

    DiagnosticClipResult result{};
    for (std::size_t index = 0; index < count; ++index) {
        const auto& vertex = first[index];
        if (!std::isfinite(vertex.x) || !std::isfinite(vertex.y) ||
            !std::isfinite(vertex.z) || !std::isfinite(vertex.w)) {
            result.finite = false;
            return result;
        }
    }

    auto* input = &first;
    auto* output = &second;
    for (std::size_t plane = 0; plane < 6 && count != 0; ++plane) {
        std::size_t output_count = 0;
        DiagnosticClipVertex previous = (*input)[count - 1];
        double previous_distance = diagnostic_clip_distance(previous, plane);
        bool previous_inside = previous_distance >= 0.0;
        for (std::size_t index = 0; index < count; ++index) {
            const DiagnosticClipVertex current = (*input)[index];
            const double current_distance =
                diagnostic_clip_distance(current, plane);
            const bool current_inside = current_distance >= 0.0;
            if (previous_inside != current_inside) {
                result.clipped = true;
                const double denominator =
                    previous_distance - current_distance;
                if (!std::isfinite(denominator) || denominator == 0.0 ||
                    output_count >= output->size()) {
                    result.finite = false;
                    return result;
                }
                const double amount = previous_distance / denominator;
                (*output)[output_count++] = diagnostic_clip_lerp(
                    previous, current, amount);
            }
            if (!current_inside)
                result.clipped = true;
            if (current_inside) {
                if (output_count >= output->size()) {
                    result.finite = false;
                    return result;
                }
                (*output)[output_count++] = current;
            }
            previous = current;
            previous_distance = current_distance;
            previous_inside = current_inside;
        }
        count = output_count;
        std::swap(input, output);
    }

    result.vertex_count = count;
    for (std::size_t index = 0; index < count; ++index)
        result.vertices[index] = (*input)[index];
    if (count < 3)
        return result;
    constexpr double bounds_epsilon = 1.0e-6;
    for (std::size_t index = 0; index < count; ++index) {
        const auto& vertex = (*input)[index];
        if (!std::isfinite(vertex.w) || vertex.w <= 0.0) {
            result.finite = false;
            return result;
        }
        const double ndc_x = vertex.x / vertex.w;
        const double ndc_y = vertex.y / vertex.w;
        if (!std::isfinite(ndc_x) || !std::isfinite(ndc_y)) {
            result.finite = false;
            return result;
        }
        result.minimum_ndc_x = (std::min)(result.minimum_ndc_x, ndc_x);
        result.minimum_ndc_y = (std::min)(result.minimum_ndc_y, ndc_y);
        result.maximum_ndc_x = (std::max)(result.maximum_ndc_x, ndc_x);
        result.maximum_ndc_y = (std::max)(result.maximum_ndc_y, ndc_y);
        result.bounded = result.bounded &&
            ndc_x >= -1.0 - bounds_epsilon &&
            ndc_x <= 1.0 + bounds_epsilon &&
            ndc_y >= -1.0 - bounds_epsilon &&
            ndc_y <= 1.0 + bounds_epsilon;
        const auto& next = (*input)[(index + 1) % count];
        if (!std::isfinite(next.w) || next.w <= 0.0) {
            result.finite = false;
            return result;
        }
        result.ndc_area += ndc_x * (next.y / next.w) -
            (next.x / next.w) * ndc_y;
    }
    result.ndc_area = std::fabs(result.ndc_area) * 0.5;
    return result;
}

void emit_primitive_diagnostics(
    const WorldDrawList& draw_list,
    const WorldGpuRenderOptions& options
) {
    if (std::getenv("OPENGT_RENDER_PRIMITIVE_DIAGNOSTICS") == nullptr)
        return;
    if (
        options.synthetic_midpoint &&
        std::getenv("OPENGT_RENDER_PRIMITIVE_DIAGNOSTICS_ALL_SAMPLES") ==
            nullptr
    )
        return;
    if (
        std::getenv("OPENGT_RENDER_PRIMITIVE_DIAGNOSTICS_RACE_ONLY") !=
            nullptr &&
        draw_list.track_commands < 1000U
    )
        return;
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_PRIMITIVE_DIAGNOSTICS_START_POLL")) {
        const long start_poll = std::strtol(configured, nullptr, 10);
        if (draw_list.input_poll < start_poll)
            return;
    }
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_PRIMITIVE_DIAGNOSTICS_END_POLL")) {
        const long end_poll = std::strtol(configured, nullptr, 10);
        if (draw_list.input_poll > end_poll)
            return;
    }
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
    std::size_t target_intersections = 0;
    std::size_t native_intersections = 0;
    std::size_t hor_plus_only = 0;
    std::size_t target_camera_crossings = 0;
    std::size_t target_near_crossings = 0;
    std::size_t oversized_spans = 0;
    std::size_t target_oversized_spans = 0;
    std::size_t nonfinite_commands = 0;
    std::size_t post_clip_visible = 0;
    std::size_t post_clip_rejected = 0;
    std::size_t post_clip_nonfinite = 0;
    std::size_t post_clip_unbounded = 0;
    std::size_t post_clip_degenerate = 0;
    std::size_t preclip_false_positive = 0;
    std::size_t preclip_false_negative = 0;
    std::size_t maximum_post_clip_vertices = 0;
    double maximum_post_clip_area_pixels = 0.0;
    std::size_t depth_track_commands = 0;
    float maximum_projection_delta = 0.0F;
    float minimum_view_z = (std::numeric_limits<float>::max)();
    float maximum_view_z = (std::numeric_limits<float>::lowest)();
    std::vector<std::size_t> suspicious;
    suspicious.reserve(64);

    const float native_x0 = static_cast<float>(draw_list.display_x);
    const float native_y0 = static_cast<float>(draw_list.display_y);
    const float native_y1 = native_y0 + draw_list.display_height;
    const float target_width = static_cast<float>(
        world_gpu_target_display_width(draw_list, options));
    const float native_center_x =
        native_x0 + draw_list.display_width * 0.5F;
    // World clip X is scaled by native_width / target_width before D3D
    // clipping. Express the real Hor+ frustum back in captured screen units
    // so edge diagnostics do not accidentally audit only the central 4:3
    // viewport.
    const float target_x0 = native_center_x - target_width * 0.5F;
    const float target_x1 = native_center_x + target_width * 0.5F;
    const double horizontal_projection_scale =
        static_cast<double>(draw_list.display_width) / target_width;
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
            (min_x < target_x0 && max_x >= target_x0) ||
            (min_x < target_x1 && max_x >= target_x1) ||
            (min_y < native_y0 && max_y >= native_y0) ||
            (min_y < native_y1 && max_y >= native_y1);
        const bool in_front = max_z >= d3d_near_plane;
        const bool target_intersection =
            in_front &&
            max_x >= target_x0 && min_x <= target_x1 &&
            max_y >= native_y0 && min_y <= native_y1;
        const bool native_intersection =
            in_front &&
            max_x >= native_x0 &&
            min_x <= native_x0 + draw_list.display_width &&
            max_y >= native_y0 && min_y <= native_y1;
        const bool oversized =
            max_x - min_x > draw_list.display_width * 4.0F ||
            max_y - min_y > draw_list.display_height * 4.0F;
        const DiagnosticClipResult post_clip = diagnostic_homogeneous_clip(
            command, horizontal_projection_scale);
        const bool post_clip_visible_command =
            post_clip.finite && post_clip.vertex_count >= 3;
        post_clip_visible += post_clip_visible_command ? 1U : 0U;
        post_clip_rejected +=
            post_clip.finite && post_clip.vertex_count < 3 ? 1U : 0U;
        post_clip_nonfinite += post_clip.finite ? 0U : 1U;
        post_clip_unbounded +=
            post_clip_visible_command && !post_clip.bounded ? 1U : 0U;
        post_clip_degenerate +=
            post_clip_visible_command && post_clip.ndc_area <= 1.0e-12
                ? 1U
                : 0U;
        preclip_false_positive +=
            target_intersection && !post_clip_visible_command ? 1U : 0U;
        preclip_false_negative +=
            !target_intersection && post_clip_visible_command ? 1U : 0U;
        maximum_post_clip_vertices = (std::max)(
            maximum_post_clip_vertices, post_clip.vertex_count);
        maximum_post_clip_area_pixels = (std::max)(
            maximum_post_clip_area_pixels,
            post_clip.ndc_area * target_width * draw_list.display_height /
                4.0);
        camera_crossings += crosses_camera ? 1U : 0U;
        near_crossings += crosses_near ? 1U : 0U;
        behind_near += entirely_behind_near ? 1U : 0U;
        viewport_crossings += crosses_viewport ? 1U : 0U;
        target_intersections += target_intersection ? 1U : 0U;
        native_intersections += native_intersection ? 1U : 0U;
        hor_plus_only +=
            target_intersection && !native_intersection ? 1U : 0U;
        target_camera_crossings +=
            target_intersection && crosses_camera ? 1U : 0U;
        target_near_crossings +=
            target_intersection && crosses_near ? 1U : 0U;
        oversized_spans += oversized ? 1U : 0U;
        target_oversized_spans +=
            target_intersection && oversized ? 1U : 0U;
        nonfinite_commands += finite ? 0U : 1U;
        if (
            suspicious.size() < 64 &&
            ((target_intersection &&
                (crosses_camera || crosses_near || oversized)) || !finite)
        )
            suspicious.push_back(index);
    }

    if (world_commands == 0) {
        minimum_view_z = 0.0F;
        maximum_view_z = 0.0F;
    }
    std::fprintf(
        stderr,
        "[Render-Primitives] frameSample=%llu frame=%llu poll=%d authored=%s "
        "projection=%s textureProjection=%s depth=%s topologyCommands=%zu "
        "world=%zu screen=%zu depthTrack=%zu viewZ=%.3f..%.3f "
        "cameraCross=%zu/%zu nearCross=%zu/%zu behindNear=%zu "
        "viewportCross=%zu intersections=%zu/%zu horPlusOnly=%zu "
        "oversized=%zu/%zu nonfinite=%zu maxAuthoredDelta=%.3f "
        "postClip=%zu/%zu nonfinite=%zu unbounded=%zu degenerate=%zu "
        "preclipMismatch=%zu/%zu maxVertices=%zu maxAreaPx=%.1f "
        "nativeDisplay=%d,%d..%d,%d targetDisplay=%.1f,%.1f..%.1f,%.1f\n",
        static_cast<unsigned long long>(primitive_sample - 1),
        static_cast<unsigned long long>(draw_list.frame_index),
        draw_list.input_poll,
        options.synthetic_midpoint ? "midpoint" : "actual",
        draw_list.continuous_projection ? "continuous" : "authored-sxy",
        "perspective-fixed",
        options.depth_buffer ? "on" : "off",
        draw_list.commands.size(),
        world_commands,
        screen_commands,
        depth_track_commands,
        minimum_view_z,
        maximum_view_z,
        camera_crossings,
        target_camera_crossings,
        near_crossings,
        target_near_crossings,
        behind_near,
        viewport_crossings,
        target_intersections,
        native_intersections,
        hor_plus_only,
        oversized_spans,
        target_oversized_spans,
        nonfinite_commands,
        maximum_projection_delta,
        post_clip_visible,
        post_clip_rejected,
        post_clip_nonfinite,
        post_clip_unbounded,
        post_clip_degenerate,
        preclip_false_positive,
        preclip_false_negative,
        maximum_post_clip_vertices,
        maximum_post_clip_area_pixels,
        draw_list.display_x,
        draw_list.display_y,
        draw_list.display_x + draw_list.display_width,
        draw_list.display_y + draw_list.display_height,
        target_x0,
        native_y0,
        target_x1,
        native_y1);

    if (std::getenv("OPENGT_RENDER_PRIMITIVE_DIAGNOSTICS_VERBOSE") == nullptr)
        return;
    for (const std::size_t index : suspicious) {
        const auto& command = draw_list.commands[index];
        float minimum_x = command.vertices[0].screen_x;
        float maximum_x = minimum_x;
        float minimum_y = command.vertices[0].screen_y;
        float maximum_y = minimum_y;
        float minimum_z = command.vertices[0].view_z;
        float maximum_z = minimum_z;
        bool finite = true;
        for (const auto& vertex : command.vertices) {
            minimum_x = (std::min)(minimum_x, vertex.screen_x);
            maximum_x = (std::max)(maximum_x, vertex.screen_x);
            minimum_y = (std::min)(minimum_y, vertex.screen_y);
            maximum_y = (std::max)(maximum_y, vertex.screen_y);
            minimum_z = (std::min)(minimum_z, vertex.view_z);
            maximum_z = (std::max)(maximum_z, vertex.view_z);
            finite = finite && std::isfinite(vertex.screen_x) &&
                std::isfinite(vertex.screen_y) &&
                std::isfinite(vertex.clip_x) &&
                std::isfinite(vertex.clip_y) &&
                std::isfinite(vertex.clip_z) &&
                std::isfinite(vertex.clip_w);
        }
        const bool target_intersection =
            maximum_z >= d3d_near_plane &&
            maximum_x >= target_x0 && minimum_x <= target_x1 &&
            maximum_y >= native_y0 && minimum_y <= native_y1;
        const bool crosses_camera =
            minimum_z <= 0.0F && maximum_z > 0.0F;
        const bool crosses_near =
            minimum_z < d3d_near_plane && maximum_z >= d3d_near_plane;
        const bool oversized =
            maximum_x - minimum_x > draw_list.display_width * 4.0F ||
            maximum_y - minimum_y > draw_list.display_height * 4.0F;
        const DiagnosticClipResult post_clip = diagnostic_homogeneous_clip(
            command, horizontal_projection_scale);
        const double post_clip_area_pixels =
            post_clip.ndc_area * target_width * draw_list.display_height /
                4.0;
        std::fprintf(
            stderr,
            "[Render-Primitive] frame=%llu poll=%d cmd=%zu kind=%u "
            "object=%u model=%08x "
            "transform=%016llx ot=%d source=%u clip=%d,%d..%d,%d "
            "reason=target:%u,camera:%u,near:%u,oversized:%u,nonfinite:%u "
            "screen=(%.3f,%.3f)(%.3f,%.3f)(%.3f,%.3f) "
            "authored=(%d,%d)(%d,%d)(%d,%d) "
            "viewZ=(%.3f,%.3f,%.3f) clipW=(%.3f,%.3f,%.3f) "
            "postClip=vertices:%zu,finite:%u,bounded:%u,areaPx:%.3f,"
            "ndc:(%.6f,%.6f)..(%.6f,%.6f)\n",
            static_cast<unsigned long long>(draw_list.frame_index),
            draw_list.input_poll,
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
            target_intersection ? 1U : 0U,
            crosses_camera ? 1U : 0U,
            crosses_near ? 1U : 0U,
            oversized ? 1U : 0U,
            finite ? 0U : 1U,
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
            command.vertices[2].clip_w,
            post_clip.vertex_count,
            post_clip.finite ? 1U : 0U,
            post_clip.bounded ? 1U : 0U,
            post_clip_area_pixels,
            post_clip.minimum_ndc_x,
            post_clip.minimum_ndc_y,
            post_clip.maximum_ndc_x,
            post_clip.maximum_ndc_y);
    }
}

struct ResidentEdgePosition {
    std::int32_t x{};
    std::int32_t y{};
    std::int32_t z{};

    bool operator==(const ResidentEdgePosition& other) const noexcept {
        return x == other.x && y == other.y && z == other.z;
    }

    bool operator<(const ResidentEdgePosition& other) const noexcept {
        if (x != other.x) return x < other.x;
        if (y != other.y) return y < other.y;
        return z < other.z;
    }
};

struct ResidentEdgeKey {
    ResidentEdgePosition first{};
    ResidentEdgePosition second{};

    bool operator==(const ResidentEdgeKey& other) const noexcept {
        return first == other.first && second == other.second;
    }
};

struct ResidentEdgeKeyHash {
    std::size_t operator()(const ResidentEdgeKey& key) const noexcept {
        std::size_t result = 1469598103934665603ULL;
        const auto mix = [&result](std::uint32_t value) {
            result ^= static_cast<std::size_t>(value);
            result *= 1099511628211ULL;
        };
        for (const auto& position : {key.first, key.second}) {
            mix(static_cast<std::uint32_t>(position.x));
            mix(static_cast<std::uint32_t>(position.y));
            mix(static_cast<std::uint32_t>(position.z));
        }
        return result;
    }
};

struct ResidentEdgeOccurrence {
    std::size_t command{};
    int edge{};
    std::uint32_t object_id{};
    std::uint32_t model_pointer{};
    std::uint32_t material_index{};
    std::uint64_t transform_id{};
    float first_x{};
    float first_y{};
    float second_x{};
    float second_y{};
    bool target_relevant{};
};

struct ResidentEdgeMismatch {
    ResidentEdgeKey key{};
    ResidentEdgeOccurrence first{};
    ResidentEdgeOccurrence second{};
    float gap{};
};

// The resident decoder reconstructs continuous view coordinates from GT2's
// exact fixed-point transform, while exact_view_* retains the integer GTE
// result. Two authored boundary copies can therefore have the same GTE edge
// but slightly different continuous endpoints. D3D rasterization is watertight
// only when both triangles submit byte-identical clip positions. This audit is
// read-only: it proves whether a visible seam is possible before any topology
// correction is allowed to move resident geometry.
void emit_resident_edge_diagnostics(
    const WorldDrawList& draw_list,
    const WorldGpuRenderOptions& options
) {
    if (std::getenv("OPENGT_RENDER_RESIDENT_EDGE_DIAGNOSTICS") == nullptr)
        return;
    if (options.synthetic_midpoint)
        return;
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_RESIDENT_EDGE_DIAGNOSTICS_START_POLL")) {
        const long start_poll = std::strtol(configured, nullptr, 10);
        if (draw_list.input_poll < start_poll)
            return;
    }
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_RESIDENT_EDGE_DIAGNOSTICS_END_POLL")) {
        const long end_poll = std::strtol(configured, nullptr, 10);
        if (draw_list.input_poll > end_poll)
            return;
    }
    static thread_local std::uint64_t sample_index = 0;
    std::uint64_t interval = 120;
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_RESIDENT_EDGE_DIAGNOSTICS_INTERVAL")) {
        const unsigned long parsed = std::strtoul(configured, nullptr, 10);
        if (parsed != 0)
            interval = parsed;
    }
    if ((sample_index++ % interval) != 0)
        return;

    const float native_y0 = static_cast<float>(draw_list.display_y);
    const float native_y1 = native_y0 + draw_list.display_height;
    const float target_width = static_cast<float>(
        world_gpu_target_display_width(draw_list, options));
    const float center_x =
        draw_list.display_x + draw_list.display_width * 0.5F;
    const float target_x0 = center_x - target_width * 0.5F;
    const float target_x1 = center_x + target_width * 0.5F;
    std::unordered_map<
        ResidentEdgeKey,
        std::vector<ResidentEdgeOccurrence>,
        ResidentEdgeKeyHash> edges;
    edges.reserve(draw_list.track_commands * 2U);
    std::size_t resident_commands = 0;
    std::size_t resident_edges = 0;
    for (std::size_t command_index = 0;
         command_index < draw_list.commands.size();
         ++command_index) {
        const auto& command = draw_list.commands[command_index];
        if (command.object_kind != 1U ||
            command.channel != WorldViewChannel::main_view ||
            command.material_index >= draw_list.materials.size())
            continue;
        const auto& material = draw_list.materials[command.material_index];
        if ((material.primitive_flags &
                world_primitive_resident_course_flag) == 0 ||
            (material.primitive_flags &
                world_primitive_screen_space_flag) != 0)
            continue;
        if (!command.exact_transform_valid)
            continue;
        ++resident_commands;
        for (int edge_index = 0; edge_index < 3; ++edge_index) {
            const auto& source_a = command.vertices[edge_index];
            const auto& source_b =
                command.vertices[(edge_index + 1) % 3];
            if (!source_a.exact_transform_valid ||
                !source_b.exact_transform_valid)
                continue;
            ResidentEdgePosition a{
                source_a.exact_view_x,
                source_a.exact_view_y,
                source_a.exact_view_z};
            ResidentEdgePosition b{
                source_b.exact_view_x,
                source_b.exact_view_y,
                source_b.exact_view_z};
            float ax = source_a.screen_x;
            float ay = source_a.screen_y;
            float bx = source_b.screen_x;
            float by = source_b.screen_y;
            if (b < a) {
                std::swap(a, b);
                std::swap(ax, bx);
                std::swap(ay, by);
            }
            const float minimum_x = (std::min)(ax, bx);
            const float maximum_x = (std::max)(ax, bx);
            const float minimum_y = (std::min)(ay, by);
            const float maximum_y = (std::max)(ay, by);
            const bool target_relevant =
                (source_a.view_z >= 16.0F || source_b.view_z >= 16.0F) &&
                maximum_x >= target_x0 && minimum_x <= target_x1 &&
                maximum_y >= native_y0 && minimum_y <= native_y1;
            edges[ResidentEdgeKey{a, b}].push_back(
                ResidentEdgeOccurrence{
                    command_index,
                    edge_index,
                    command.object_id,
                    command.model_pointer,
                    command.material_index,
                    command.transform_id,
                    ax,
                    ay,
                    bx,
                    by,
                    target_relevant});
            ++resident_edges;
        }
    }

    constexpr float mismatch_epsilon = 1.0e-5F;
    std::size_t shared_edges = 0;
    std::size_t target_shared_edges = 0;
    std::size_t mismatched_edges = 0;
    std::size_t target_mismatched_edges = 0;
    std::size_t target_quarter_pixel_edges = 0;
    std::size_t target_same_material_mismatches = 0;
    std::size_t target_cross_object_mismatches = 0;
    float maximum_gap = 0.0F;
    float maximum_target_gap = 0.0F;
    std::vector<ResidentEdgeMismatch> details;
    details.reserve(64);
    for (const auto& entry : edges) {
        const auto& occurrences = entry.second;
        if (occurrences.size() < 2)
            continue;
        ++shared_edges;
        bool target_shared = false;
        bool mismatched = false;
        bool target_mismatched = false;
        float edge_maximum_gap = 0.0F;
        ResidentEdgeMismatch maximum{};
        for (std::size_t left = 0; left < occurrences.size(); ++left) {
            target_shared = target_shared || occurrences[left].target_relevant;
            for (std::size_t right = left + 1;
                 right < occurrences.size();
                 ++right) {
                const float first_dx =
                    occurrences[left].first_x - occurrences[right].first_x;
                const float first_dy =
                    occurrences[left].first_y - occurrences[right].first_y;
                const float second_dx =
                    occurrences[left].second_x - occurrences[right].second_x;
                const float second_dy =
                    occurrences[left].second_y - occurrences[right].second_y;
                const float gap = (std::max)(
                    std::hypot(first_dx, first_dy),
                    std::hypot(second_dx, second_dy));
                maximum_gap = (std::max)(maximum_gap, gap);
                if (gap <= mismatch_epsilon)
                    continue;
                mismatched = true;
                const bool pair_target =
                    occurrences[left].target_relevant ||
                    occurrences[right].target_relevant;
                if (pair_target) {
                    target_mismatched = true;
                    maximum_target_gap = (std::max)(maximum_target_gap, gap);
                }
                if (gap > edge_maximum_gap) {
                    edge_maximum_gap = gap;
                    maximum = ResidentEdgeMismatch{
                        entry.first,
                        occurrences[left],
                        occurrences[right],
                        gap};
                }
            }
        }
        target_shared_edges += target_shared ? 1U : 0U;
        mismatched_edges += mismatched ? 1U : 0U;
        target_mismatched_edges += target_mismatched ? 1U : 0U;
        if (!target_mismatched)
            continue;
        target_quarter_pixel_edges += edge_maximum_gap >= 0.25F ? 1U : 0U;
        target_same_material_mismatches +=
            maximum.first.material_index == maximum.second.material_index
                ? 1U
                : 0U;
        target_cross_object_mismatches +=
            maximum.first.object_id != maximum.second.object_id ? 1U : 0U;
        if (details.size() < 64)
            details.push_back(maximum);
    }
    std::sort(
        details.begin(), details.end(),
        [](const ResidentEdgeMismatch& left,
           const ResidentEdgeMismatch& right) {
            if (left.gap != right.gap)
                return left.gap > right.gap;
            if (left.first.command != right.first.command)
                return left.first.command < right.first.command;
            return left.second.command < right.second.command;
        });
    std::fprintf(
        stderr,
        "[Render-Resident-Edges] sample=%llu frame=%llu poll=%d "
        "commands=%zu edges=%zu unique=%zu shared=%zu targetShared=%zu "
        "mismatched=%zu targetMismatched=%zu quarterPixel=%zu "
        "sameMaterial=%zu crossObject=%zu maximum=%.6f targetMaximum=%.6f\n",
        static_cast<unsigned long long>(sample_index - 1),
        static_cast<unsigned long long>(draw_list.frame_index),
        draw_list.input_poll,
        resident_commands,
        resident_edges,
        edges.size(),
        shared_edges,
        target_shared_edges,
        mismatched_edges,
        target_mismatched_edges,
        target_quarter_pixel_edges,
        target_same_material_mismatches,
        target_cross_object_mismatches,
        maximum_gap,
        maximum_target_gap);
    if (std::getenv(
            "OPENGT_RENDER_RESIDENT_EDGE_DIAGNOSTICS_VERBOSE") == nullptr)
        return;
    for (std::size_t index = 0; index < details.size() && index < 16; ++index) {
        const auto& detail = details[index];
        std::fprintf(
            stderr,
            "[Render-Resident-Edge] frame=%llu poll=%d rank=%zu gap=%.6f "
            "exact=(%d,%d,%d)-(%d,%d,%d) "
            "first=cmd%zu/edge%d/object%u/model%08x/material%u/transform%016llx/"
            "screen(%.6f,%.6f)-(%.6f,%.6f) "
            "second=cmd%zu/edge%d/object%u/model%08x/material%u/transform%016llx/"
            "screen(%.6f,%.6f)-(%.6f,%.6f)\n",
            static_cast<unsigned long long>(draw_list.frame_index),
            draw_list.input_poll,
            index,
            detail.gap,
            detail.key.first.x,
            detail.key.first.y,
            detail.key.first.z,
            detail.key.second.x,
            detail.key.second.y,
            detail.key.second.z,
            detail.first.command,
            detail.first.edge,
            detail.first.object_id,
            detail.first.model_pointer,
            detail.first.material_index,
            static_cast<unsigned long long>(detail.first.transform_id),
            detail.first.first_x,
            detail.first.first_y,
            detail.first.second_x,
            detail.first.second_y,
            detail.second.command,
            detail.second.edge,
            detail.second.object_id,
            detail.second.model_pointer,
            detail.second.material_index,
            static_cast<unsigned long long>(detail.second.transform_id),
            detail.second.first_x,
            detail.second.first_y,
            detail.second.second_x,
            detail.second.second_y);
    }
}

struct SceneDiagnosticGroup {
    std::uint32_t object_kind{};
    std::uint32_t object_id{};
    std::uint32_t model_pointer{};
    WorldViewChannel channel{};
    std::size_t commands{};
    std::size_t textured_commands{};
    std::size_t transparent_commands{};
    std::size_t resident_course_commands{};
    std::size_t native_intersections{};
    std::size_t target_intersections{};
    std::size_t hor_plus_only{};
    std::size_t left_margin_intersections{};
    std::size_t right_margin_intersections{};
    std::size_t clip_crossings{};
    std::size_t target_clip_crossings{};
    std::array<std::size_t, 6> target_clip_plane_crossings{};
    std::array<std::size_t, 6> hor_plus_clip_plane_crossings{};
    std::size_t trivial_clip_rejects{};
    std::size_t target_trivial_clip_rejects{};
    std::size_t near_crossings{};
    std::size_t target_near_crossings{};
    std::size_t behind_near{};
    std::size_t nonfinite_commands{};
    std::size_t full_display_scissors{};
    std::size_t post_clip_visible{};
    std::size_t post_clip_rejected{};
    std::size_t post_clip_nonfinite{};
    std::size_t post_clip_changed_vertex_count{};
    double post_clip_ndc_area{};
    std::size_t target_post_clip_visible{};
    std::size_t target_post_clip_rejected{};
    std::size_t target_post_clip_nonfinite{};
    std::uint64_t geometry_signature{1469598103934665603ULL};
    float minimum_screen_x{(std::numeric_limits<float>::max)()};
    float minimum_screen_y{(std::numeric_limits<float>::max)()};
    float maximum_screen_x{(std::numeric_limits<float>::lowest)()};
    float maximum_screen_y{(std::numeric_limits<float>::lowest)()};
    float minimum_view_z{(std::numeric_limits<float>::max)()};
    float maximum_view_z{(std::numeric_limits<float>::lowest)()};
    float minimum_projection_plane{(std::numeric_limits<float>::max)()};
    float maximum_projection_plane{(std::numeric_limits<float>::lowest)()};
    float minimum_projection_offset_x{(std::numeric_limits<float>::max)()};
    float maximum_projection_offset_x{(std::numeric_limits<float>::lowest)()};
    float minimum_projection_offset_y{(std::numeric_limits<float>::max)()};
    float maximum_projection_offset_y{(std::numeric_limits<float>::lowest)()};
};

struct SceneDiagnosticGroupKey {
    std::uint32_t object_kind{};
    std::uint32_t object_id{};
    std::uint32_t model_pointer{};
    WorldViewChannel channel{};

    bool operator==(const SceneDiagnosticGroupKey& other) const noexcept
    {
        return object_kind == other.object_kind &&
            object_id == other.object_id &&
            model_pointer == other.model_pointer &&
            channel == other.channel;
    }
};

struct SceneDiagnosticGroupKeyHash {
    std::size_t operator()(
        const SceneDiagnosticGroupKey& key) const noexcept
    {
        std::size_t hash = key.object_kind;
        const auto mix = [&hash] (std::uint32_t value) {
            hash ^= static_cast<std::size_t>(value) +
                0x9E3779B9U + (hash << 6U) + (hash >> 2U);
        };
        mix(key.object_id);
        mix(key.model_pointer);
        mix(static_cast<std::uint32_t>(key.channel));
        return hash;
    }
};

struct SceneUnownedDiagnosticGroup {
    std::uint64_t transform_id{};
    std::uint32_t primitive_flags{};
    std::uint16_t texture_page{};
    std::uint16_t clut{};
    std::int16_t clip_x0{};
    std::int16_t clip_y0{};
    std::int16_t clip_x1{};
    std::int16_t clip_y1{};
    WorldViewChannel channel{};
    bool exact_transform_valid{};
    std::int32_t translation_x{};
    std::int32_t translation_y{};
    std::int32_t translation_z{};
    std::size_t commands{};
    std::size_t transparent_commands{};
    std::uint32_t minimum_source_command{
        (std::numeric_limits<std::uint32_t>::max)()};
    std::uint32_t maximum_source_command{};
    float minimum_model_x{(std::numeric_limits<float>::max)()};
    float minimum_model_y{(std::numeric_limits<float>::max)()};
    float minimum_model_z{(std::numeric_limits<float>::max)()};
    float maximum_model_x{(std::numeric_limits<float>::lowest)()};
    float maximum_model_y{(std::numeric_limits<float>::lowest)()};
    float maximum_model_z{(std::numeric_limits<float>::lowest)()};
    float minimum_screen_x{(std::numeric_limits<float>::max)()};
    float minimum_screen_y{(std::numeric_limits<float>::max)()};
    float maximum_screen_x{(std::numeric_limits<float>::lowest)()};
    float maximum_screen_y{(std::numeric_limits<float>::lowest)()};
    float minimum_view_z{(std::numeric_limits<float>::max)()};
    float maximum_view_z{(std::numeric_limits<float>::lowest)()};
    float minimum_projection_plane{(std::numeric_limits<float>::max)()};
    float maximum_projection_plane{(std::numeric_limits<float>::lowest)()};
};

bool same_scene_unowned_diagnostic_group(
    const SceneUnownedDiagnosticGroup& group,
    const WorldDrawCommand& command,
    const WorldMaterial& material
) noexcept {
    return
        group.transform_id == command.transform_id &&
        group.primitive_flags == material.primitive_flags &&
        group.texture_page == material.texture_page &&
        group.clut == material.clut &&
        group.clip_x0 == command.clip_x0 &&
        group.clip_y0 == command.clip_y0 &&
        group.clip_x1 == command.clip_x1 &&
        group.clip_y1 == command.clip_y1 &&
        group.channel == command.channel &&
        group.exact_transform_valid == command.exact_transform_valid;
}

bool same_scene_diagnostic_group(
    const SceneDiagnosticGroup& group,
    const WorldDrawCommand& command
) noexcept {
    return
        group.object_kind == command.object_kind &&
        group.object_id == command.object_id &&
        group.model_pointer == command.model_pointer &&
        group.channel == command.channel;
}

bool same_scene_diagnostic_group(
    const SceneDiagnosticGroup& left,
    const SceneDiagnosticGroup& right
) noexcept {
    return
        left.object_kind == right.object_kind &&
        left.object_id == right.object_id &&
        left.model_pointer == right.model_pointer &&
        left.channel == right.channel;
}

const char* scene_diagnostic_kind_name(std::uint32_t kind) noexcept {
    switch (kind) {
        case 0U: return "unclassified";
        case 1U: return "track";
        case 2U: return "vehicle";
        case 3U: return "background";
        default: return "invalid";
    }
}

const char* scene_diagnostic_channel_name(WorldViewChannel channel) noexcept {
    return channel == WorldViewChannel::main_view ? "main" : "secondary";
}

void emit_scene_diagnostics(
    const WorldDrawList& draw_list,
    const WorldGpuRenderOptions& options
) {
    const bool snapshot_diagnostics =
        std::getenv("OPENGT_RENDER_SCENE_DIAGNOSTICS") != nullptr;
    const bool scene_continuity_audit =
        std::getenv("OPENGT_RENDER_SCENE_CONTINUITY_AUDIT") != nullptr;
    const bool vehicle_boundary_audit =
        std::getenv("OPENGT_RENDER_VEHICLE_BOUNDARY_AUDIT") != nullptr;
    const bool continuity_audit =
        scene_continuity_audit || vehicle_boundary_audit;
    if (!snapshot_diagnostics && !continuity_audit)
        return;
    if (
        options.synthetic_midpoint &&
        std::getenv("OPENGT_RENDER_SCENE_DIAGNOSTICS_ALL_SAMPLES") ==
            nullptr
    )
        return;
    if (
        std::getenv("OPENGT_RENDER_SCENE_DIAGNOSTICS_RACE_ONLY") != nullptr &&
        draw_list.track_commands < 1000U
    )
        return;
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_SCENE_DIAGNOSTICS_START_POLL")) {
        const long start_poll = std::strtol(configured, nullptr, 10);
        if (draw_list.input_poll < start_poll)
            return;
    }
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_SCENE_DIAGNOSTICS_END_POLL")) {
        const long end_poll = std::strtol(configured, nullptr, 10);
        if (draw_list.input_poll > end_poll)
            return;
    }

    static thread_local std::uint64_t diagnostic_sample = 0;
    std::uint64_t interval = 120;
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_SCENE_DIAGNOSTICS_INTERVAL")) {
        const unsigned long parsed = std::strtoul(configured, nullptr, 10);
        if (parsed != 0)
            interval = parsed;
    }
    const std::uint64_t sample_index = diagnostic_sample++;
    const bool emit_snapshot =
        snapshot_diagnostics && (sample_index % interval) == 0;
    if (!emit_snapshot && !continuity_audit)
        return;

    constexpr float near_plane = 16.0F;
    const float native_x0 = static_cast<float>(draw_list.display_x);
    const float native_y0 = static_cast<float>(draw_list.display_y);
    const float native_x1 = native_x0 + draw_list.display_width;
    const float native_y1 = native_y0 + draw_list.display_height;
    const std::uint32_t target_display_width =
        world_gpu_target_display_width(draw_list, options);
    const float target_width = static_cast<float>(target_display_width);
    const float native_center_x =
        native_x0 + draw_list.display_width * 0.5F;
    const float target_x0 = native_center_x - target_width * 0.5F;
    const float target_x1 = native_center_x + target_width * 0.5F;
    const float horizontal_projection_scale =
        static_cast<float>(draw_list.display_width) / target_width;

    bool continuity_object_filter = false;
    std::uint32_t continuity_object = 0;
    if (continuity_audit) {
        if (const char* configured = std::getenv(
                "OPENGT_RENDER_SCENE_CONTINUITY_OBJECT")) {
            continuity_object = static_cast<std::uint32_t>(
                std::strtoul(configured, nullptr, 0));
            continuity_object_filter = true;
        }
    }

    std::vector<SceneDiagnosticGroup> groups;
    groups.reserve(192);
    std::unordered_map<
        SceneDiagnosticGroupKey,
        std::size_t,
        SceneDiagnosticGroupKeyHash> group_indices;
    group_indices.reserve(256);
    std::size_t screen_commands = 0;
    for (const auto& command : draw_list.commands) {
        if (continuity_object_filter &&
            command.object_id != continuity_object)
            continue;
        if (
            vehicle_boundary_audit && !scene_continuity_audit &&
            command.object_kind != 2U
        )
            continue;
        const auto& material = draw_list.materials[command.material_index];
        if ((material.primitive_flags & world_primitive_screen_space_flag) != 0) {
            ++screen_commands;
            continue;
        }

        const SceneDiagnosticGroupKey key{
            command.object_kind,
            command.object_id,
            command.model_pointer,
            command.channel,
        };
        const auto [found, inserted] = group_indices.try_emplace(
            key, groups.size());
        if (inserted) {
            groups.push_back(SceneDiagnosticGroup{});
            auto& group = groups.back();
            group.object_kind = command.object_kind;
            group.object_id = command.object_id;
            group.model_pointer = command.model_pointer;
            group.channel = command.channel;
        }
        auto& group = groups[found->second];
        ++group.commands;

        DiagnosticClipResult vehicle_post_clip{};
        bool vehicle_post_clip_evaluated = false;
        if (vehicle_boundary_audit && command.object_kind == 2U) {
            const auto signature_add = [&group] (std::uint32_t value) {
                group.geometry_signature ^= value;
                group.geometry_signature *= 1099511628211ULL;
            };
            signature_add(command.model_pointer);
            signature_add(material.primitive_flags);
            signature_add(material.texture_page);
            signature_add(material.clut);
            for (const auto& vertex : command.vertices) {
                signature_add(static_cast<std::uint16_t>(vertex.model_x));
                signature_add(static_cast<std::uint16_t>(vertex.model_y));
                signature_add(static_cast<std::uint16_t>(vertex.model_z));
            }
            vehicle_post_clip = diagnostic_homogeneous_clip(
                command, horizontal_projection_scale);
            vehicle_post_clip_evaluated = true;
            const bool post_clip_visible_command =
                vehicle_post_clip.finite &&
                vehicle_post_clip.vertex_count >= 3U;
            group.post_clip_visible +=
                post_clip_visible_command ? 1U : 0U;
            group.post_clip_rejected +=
                vehicle_post_clip.finite &&
                vehicle_post_clip.vertex_count < 3U ? 1U : 0U;
            group.post_clip_nonfinite +=
                vehicle_post_clip.finite ? 0U : 1U;
            group.post_clip_changed_vertex_count +=
                post_clip_visible_command &&
                    vehicle_post_clip.vertex_count != 3U
                    ? 1U
                    : 0U;
            group.post_clip_ndc_area += post_clip_visible_command
                ? vehicle_post_clip.ndc_area
                : 0.0;
        }

        if (continuity_audit && !emit_snapshot) {
            float minimum_x = command.vertices[0].screen_x;
            float maximum_x = minimum_x;
            float minimum_y = command.vertices[0].screen_y;
            float maximum_y = minimum_y;
            float minimum_z = command.vertices[0].view_z;
            float maximum_z = minimum_z;
            for (const auto& vertex : command.vertices) {
                minimum_x = (std::min)(minimum_x, vertex.screen_x);
                maximum_x = (std::max)(maximum_x, vertex.screen_x);
                minimum_y = (std::min)(minimum_y, vertex.screen_y);
                maximum_y = (std::max)(maximum_y, vertex.screen_y);
                minimum_z = (std::min)(minimum_z, vertex.view_z);
                maximum_z = (std::max)(maximum_z, vertex.view_z);
            }
            // PS1 double buffering alternates the active drawing page between
            // y=0 and y=240.  Report coordinates relative to that frame's
            // displayed page so a stationary object does not appear to jump
            // 240 lines in a cross-frame continuity trace.
            group.minimum_screen_x = (std::min)(
                group.minimum_screen_x, minimum_x - native_x0);
            group.minimum_screen_y = (std::min)(
                group.minimum_screen_y, minimum_y - native_y0);
            group.maximum_screen_x = (std::max)(
                group.maximum_screen_x, maximum_x - native_x0);
            group.maximum_screen_y = (std::max)(
                group.maximum_screen_y, maximum_y - native_y0);
            group.minimum_view_z = (std::min)(
                group.minimum_view_z, minimum_z);
            group.maximum_view_z = (std::max)(
                group.maximum_view_z, maximum_z);
            const bool in_front = maximum_z >= near_plane;
            const bool native_intersection =
                in_front &&
                maximum_x >= native_x0 && minimum_x <= native_x1 &&
                maximum_y >= native_y0 && minimum_y <= native_y1;
            const bool target_intersection =
                in_front &&
                maximum_x >= target_x0 && minimum_x <= target_x1 &&
                maximum_y >= native_y0 && minimum_y <= native_y1;
            group.native_intersections +=
                native_intersection ? 1U : 0U;
            group.target_intersections +=
                target_intersection ? 1U : 0U;
            if (target_intersection && vehicle_post_clip_evaluated) {
                group.target_post_clip_visible +=
                    vehicle_post_clip.finite &&
                        vehicle_post_clip.vertex_count >= 3U ? 1U : 0U;
                group.target_post_clip_rejected +=
                    vehicle_post_clip.finite &&
                        vehicle_post_clip.vertex_count < 3U ? 1U : 0U;
                group.target_post_clip_nonfinite +=
                    vehicle_post_clip.finite ? 0U : 1U;
            }
            group.full_display_scissors +=
                command.clip_x0 <= draw_list.display_x &&
                command.clip_y0 <= draw_list.display_y &&
                command.clip_x1 >=
                    draw_list.display_x + draw_list.display_width - 1 &&
                command.clip_y1 >=
                    draw_list.display_y + draw_list.display_height - 1
                    ? 1U
                    : 0U;
            continue;
        }

        group.textured_commands +=
            (material.primitive_flags & textured_flag) != 0 ? 1U : 0U;
        group.transparent_commands +=
            (material.primitive_flags & semi_transparent_flag) != 0 ? 1U : 0U;
        group.resident_course_commands +=
            (material.primitive_flags &
                world_primitive_resident_course_flag) != 0
                ? 1U
                : 0U;

        float minimum_x = command.vertices[0].screen_x;
        float maximum_x = minimum_x;
        float minimum_y = command.vertices[0].screen_y;
        float maximum_y = minimum_y;
        float minimum_z = command.vertices[0].view_z;
        float maximum_z = minimum_z;
        bool finite = true;
        std::array<bool, 6> all_outside{
            true, true, true, true, true, true};
        std::array<bool, 6> any_outside{};
        std::array<bool, 6> any_inside{};
        for (const auto& vertex : command.vertices) {
            minimum_x = (std::min)(minimum_x, vertex.screen_x);
            maximum_x = (std::max)(maximum_x, vertex.screen_x);
            minimum_y = (std::min)(minimum_y, vertex.screen_y);
            maximum_y = (std::max)(maximum_y, vertex.screen_y);
            minimum_z = (std::min)(minimum_z, vertex.view_z);
            maximum_z = (std::max)(maximum_z, vertex.view_z);
            group.minimum_projection_plane = (std::min)(
                group.minimum_projection_plane, vertex.projection_plane);
            group.maximum_projection_plane = (std::max)(
                group.maximum_projection_plane, vertex.projection_plane);
            group.minimum_projection_offset_x = (std::min)(
                group.minimum_projection_offset_x,
                vertex.projection_offset_x);
            group.maximum_projection_offset_x = (std::max)(
                group.maximum_projection_offset_x,
                vertex.projection_offset_x);
            group.minimum_projection_offset_y = (std::min)(
                group.minimum_projection_offset_y,
                vertex.projection_offset_y);
            group.maximum_projection_offset_y = (std::max)(
                group.maximum_projection_offset_y,
                vertex.projection_offset_y);
            finite = finite &&
                std::isfinite(vertex.screen_x) &&
                std::isfinite(vertex.screen_y) &&
                std::isfinite(vertex.view_z) &&
                std::isfinite(vertex.clip_x) &&
                std::isfinite(vertex.clip_y) &&
                std::isfinite(vertex.clip_z) &&
                std::isfinite(vertex.clip_w);

            const float clip_x =
                vertex.clip_x * horizontal_projection_scale;
            const std::array<bool, 6> outside{
                clip_x < -vertex.clip_w,
                clip_x > vertex.clip_w,
                vertex.clip_y < -vertex.clip_w,
                vertex.clip_y > vertex.clip_w,
                vertex.clip_z < 0.0F,
                vertex.clip_z > vertex.clip_w,
            };
            for (std::size_t plane = 0; plane < outside.size(); ++plane) {
                all_outside[plane] = all_outside[plane] && outside[plane];
                any_outside[plane] = any_outside[plane] || outside[plane];
                any_inside[plane] = any_inside[plane] || !outside[plane];
            }
        }
        group.minimum_screen_x = (std::min)(
            group.minimum_screen_x, minimum_x - native_x0);
        group.minimum_screen_y = (std::min)(
            group.minimum_screen_y, minimum_y - native_y0);
        group.maximum_screen_x = (std::max)(
            group.maximum_screen_x, maximum_x - native_x0);
        group.maximum_screen_y = (std::max)(
            group.maximum_screen_y, maximum_y - native_y0);
        group.minimum_view_z = (std::min)(
            group.minimum_view_z, minimum_z);
        group.maximum_view_z = (std::max)(
            group.maximum_view_z, maximum_z);

        const bool in_front = maximum_z >= near_plane;
        const bool native_intersection =
            in_front &&
            maximum_x >= native_x0 && minimum_x <= native_x1 &&
            maximum_y >= native_y0 && minimum_y <= native_y1;
        const bool target_intersection =
            in_front &&
            maximum_x >= target_x0 && minimum_x <= target_x1 &&
            maximum_y >= native_y0 && minimum_y <= native_y1;
        group.native_intersections += native_intersection ? 1U : 0U;
        group.target_intersections += target_intersection ? 1U : 0U;
        if (target_intersection && vehicle_post_clip_evaluated) {
            group.target_post_clip_visible +=
                vehicle_post_clip.finite &&
                    vehicle_post_clip.vertex_count >= 3U ? 1U : 0U;
            group.target_post_clip_rejected +=
                vehicle_post_clip.finite &&
                    vehicle_post_clip.vertex_count < 3U ? 1U : 0U;
            group.target_post_clip_nonfinite +=
                vehicle_post_clip.finite ? 0U : 1U;
        }
        group.hor_plus_only +=
            target_intersection && !native_intersection ? 1U : 0U;
        group.left_margin_intersections +=
            target_intersection && minimum_x < native_x0 ? 1U : 0U;
        group.right_margin_intersections +=
            target_intersection && maximum_x > native_x1 ? 1U : 0U;
        const bool near_crossing =
            minimum_z < near_plane && maximum_z >= near_plane;
        group.near_crossings += near_crossing ? 1U : 0U;
        group.target_near_crossings +=
            near_crossing && target_intersection ? 1U : 0U;
        group.behind_near += maximum_z < near_plane ? 1U : 0U;
        group.nonfinite_commands += finite ? 0U : 1U;
        const bool trivial_clip_reject = std::any_of(
            all_outside.begin(), all_outside.end(),
            [] (bool value) { return value; });
        bool clip_crossing = false;
        for (std::size_t plane = 0; plane < any_outside.size(); ++plane) {
            const bool crosses_plane =
                any_outside[plane] && any_inside[plane];
            clip_crossing = clip_crossing || crosses_plane;
            group.target_clip_plane_crossings[plane] +=
                crosses_plane && target_intersection ? 1U : 0U;
            group.hor_plus_clip_plane_crossings[plane] +=
                crosses_plane && target_intersection && !native_intersection
                    ? 1U
                    : 0U;
        }
        group.trivial_clip_rejects += trivial_clip_reject ? 1U : 0U;
        group.target_trivial_clip_rejects +=
            trivial_clip_reject && target_intersection ? 1U : 0U;
        group.clip_crossings += clip_crossing ? 1U : 0U;
        group.target_clip_crossings +=
            clip_crossing && target_intersection ? 1U : 0U;
        group.full_display_scissors +=
            command.clip_x0 <= draw_list.display_x &&
            command.clip_y0 <= draw_list.display_y &&
            command.clip_x1 >=
                draw_list.display_x + draw_list.display_width - 1 &&
            command.clip_y1 >=
                draw_list.display_y + draw_list.display_height - 1
                ? 1U
                : 0U;
    }

    static thread_local std::vector<SceneDiagnosticGroup> previous_groups;
    static thread_local bool continuity_has_baseline = false;
    const bool continuity_baseline = !continuity_has_baseline;
    std::size_t added_groups = 0;
    std::size_t removed_groups = 0;
    std::size_t changed_track_groups = 0;
    std::size_t target_visible_added_groups = 0;
    std::size_t target_visible_removed_groups = 0;
    if (!continuity_baseline) {
        for (const auto& previous : previous_groups) {
            const auto current = std::find_if(
                groups.begin(), groups.end(),
                [&] (const SceneDiagnosticGroup& candidate) {
                    return same_scene_diagnostic_group(previous, candidate);
                });
            if (current == groups.end()) {
                ++removed_groups;
                target_visible_removed_groups +=
                    previous.target_intersections != 0 ? 1U : 0U;
            } else if (
                previous.object_kind == 1U &&
                (previous.resident_course_commands != 0 ||
                    current->resident_course_commands != 0) &&
                previous.commands != current->commands
            )
                ++changed_track_groups;
        }
        for (const auto& current : groups) {
            const auto previous = std::find_if(
                previous_groups.begin(), previous_groups.end(),
                [&] (const SceneDiagnosticGroup& candidate) {
                    return same_scene_diagnostic_group(current, candidate);
                });
            if (previous == previous_groups.end()) {
                ++added_groups;
                target_visible_added_groups +=
                    current.target_intersections != 0 ? 1U : 0U;
            }
        }
    }

    static thread_local std::uint64_t continuity_samples = 0;
    static thread_local std::uint64_t continuity_target_added = 0;
    static thread_local std::uint64_t continuity_target_removed = 0;
    static thread_local std::uint64_t continuity_previous_frame = 0;
    static thread_local std::uint64_t continuity_frame_gaps = 0;
    if (continuity_audit) {
        if (continuity_samples != 0 &&
            draw_list.frame_index > continuity_previous_frame + 1U)
            continuity_frame_gaps +=
                draw_list.frame_index - continuity_previous_frame - 1U;
        continuity_previous_frame = draw_list.frame_index;
        ++continuity_samples;
        continuity_target_added += target_visible_added_groups;
        continuity_target_removed += target_visible_removed_groups;
        if (continuity_samples == 1 ||
            (continuity_samples %
                (continuity_object_filter ? 100U : 600U)) == 0 ||
            target_visible_added_groups != 0 ||
            target_visible_removed_groups != 0) {
            std::fprintf(
                stderr,
                "[Render-Scene-Continuity] sample=%llu frame=%llu poll=%d "
                "groups=%zu targetAdded=%zu targetRemoved=%zu "
                "totalAdded=%llu totalRemoved=%llu frameGaps=%llu"
                " focusObject=%u\n",
                static_cast<unsigned long long>(continuity_samples),
                static_cast<unsigned long long>(draw_list.frame_index),
                draw_list.input_poll,
                groups.size(),
                target_visible_added_groups,
                target_visible_removed_groups,
                static_cast<unsigned long long>(continuity_target_added),
                static_cast<unsigned long long>(continuity_target_removed),
                static_cast<unsigned long long>(continuity_frame_gaps),
                continuity_object_filter ? continuity_object : 0U);
        }
    }

    const auto vehicle_at_legacy_boundary = [&draw_list] (
        const SceneDiagnosticGroup& group
    ) noexcept {
        return
            group.object_kind == 2U &&
            group.channel == WorldViewChannel::main_view &&
            group.target_intersections != 0U &&
            (group.minimum_screen_x < 0.0F ||
                group.maximum_screen_x >
                    static_cast<float>(draw_list.display_width));
    };
    if (vehicle_boundary_audit) {
        for (const auto& current : groups) {
            if (!vehicle_at_legacy_boundary(current))
                continue;
            const bool left = current.minimum_screen_x < 0.0F;
            const bool right = current.maximum_screen_x >
                static_cast<float>(draw_list.display_width);
            std::fprintf(
                stderr,
                "[Render-Vehicle-Boundary] sample=%llu frame=%llu poll=%d "
                "object=%u model=%08x edge=%s commands=%zu "
                "native=%zu target=%zu "
                "postClip=%zu/%zu/%zu changedVertices=%zu "
                "targetPostClip=%zu/%zu/%zu "
                "ndcArea=%.9f geometry=%016llx fullScissor=%zu/%zu "
                "screen=%.2f,%.2f..%.2f,%.2f viewZ=%.2f..%.2f\n",
                static_cast<unsigned long long>(sample_index),
                static_cast<unsigned long long>(draw_list.frame_index),
                draw_list.input_poll,
                current.object_id,
                current.model_pointer,
                left && right ? "both" : left ? "left" : "right",
                current.commands,
                current.native_intersections,
                current.target_intersections,
                current.post_clip_visible,
                current.post_clip_rejected,
                current.post_clip_nonfinite,
                current.post_clip_changed_vertex_count,
                current.target_post_clip_visible,
                current.target_post_clip_rejected,
                current.target_post_clip_nonfinite,
                current.post_clip_ndc_area,
                static_cast<unsigned long long>(current.geometry_signature),
                current.full_display_scissors,
                current.commands,
                current.minimum_screen_x,
                current.minimum_screen_y,
                current.maximum_screen_x,
                current.maximum_screen_y,
                current.minimum_view_z,
                current.maximum_view_z);
        }

        if (!continuity_baseline) {
            for (const auto& previous : previous_groups) {
                const auto current = std::find_if(
                    groups.begin(), groups.end(),
                    [&] (const SceneDiagnosticGroup& candidate) {
                        return same_scene_diagnostic_group(
                            previous, candidate);
                    });
                const bool previous_boundary =
                    vehicle_at_legacy_boundary(previous);
                const bool current_boundary =
                    current != groups.end() &&
                    vehicle_at_legacy_boundary(*current);
                if (!previous_boundary && !current_boundary)
                    continue;
                const char* event = nullptr;
                if (current == groups.end())
                    event = "missing";
                else if (previous.commands != current->commands)
                    event = "command-count";
                else if (
                    previous.post_clip_visible !=
                        current->post_clip_visible ||
                    previous.post_clip_rejected !=
                        current->post_clip_rejected ||
                    previous.post_clip_nonfinite !=
                        current->post_clip_nonfinite ||
                    previous.post_clip_changed_vertex_count !=
                        current->post_clip_changed_vertex_count
                )
                    event = "post-clip-count";
                else if (
                    previous.geometry_signature !=
                        current->geometry_signature
                )
                    event = "geometry-set";
                else if (
                    previous.full_display_scissors !=
                        current->full_display_scissors
                )
                    event = "scissor-count";
                if (event == nullptr)
                    continue;
                std::fprintf(
                    stderr,
                    "[Render-Vehicle-Boundary-Transition] "
                    "sample=%llu frame=%llu poll=%d event=%s "
                    "object=%u model=%08x commands=%zu->%zu "
                    "postClip=%zu/%zu/%zu->%zu/%zu/%zu "
                    "changedVertices=%zu->%zu geometry=%016llx->%016llx "
                    "fullScissor=%zu->%zu "
                    "previousScreen=%.2f,%.2f..%.2f,%.2f "
                    "currentScreen=%.2f,%.2f..%.2f,%.2f\n",
                    static_cast<unsigned long long>(sample_index),
                    static_cast<unsigned long long>(draw_list.frame_index),
                    draw_list.input_poll,
                    event,
                    previous.object_id,
                    previous.model_pointer,
                    previous.commands,
                    current == groups.end() ? 0U : current->commands,
                    previous.post_clip_visible,
                    previous.post_clip_rejected,
                    previous.post_clip_nonfinite,
                    current == groups.end()
                        ? 0U : current->post_clip_visible,
                    current == groups.end()
                        ? 0U : current->post_clip_rejected,
                    current == groups.end()
                        ? 0U : current->post_clip_nonfinite,
                    previous.post_clip_changed_vertex_count,
                    current == groups.end()
                        ? 0U : current->post_clip_changed_vertex_count,
                    static_cast<unsigned long long>(
                        previous.geometry_signature),
                    static_cast<unsigned long long>(
                        current == groups.end()
                            ? 0ULL : current->geometry_signature),
                    previous.full_display_scissors,
                    current == groups.end()
                        ? 0U : current->full_display_scissors,
                    previous.minimum_screen_x,
                    previous.minimum_screen_y,
                    previous.maximum_screen_x,
                    previous.maximum_screen_y,
                    current == groups.end()
                        ? 0.0F : current->minimum_screen_x,
                    current == groups.end()
                        ? 0.0F : current->minimum_screen_y,
                    current == groups.end()
                        ? 0.0F : current->maximum_screen_x,
                    current == groups.end()
                        ? 0.0F : current->maximum_screen_y);
            }
            for (const auto& current : groups) {
                if (!vehicle_at_legacy_boundary(current))
                    continue;
                const auto previous = std::find_if(
                    previous_groups.begin(), previous_groups.end(),
                    [&] (const SceneDiagnosticGroup& candidate) {
                        return same_scene_diagnostic_group(
                            current, candidate);
                    });
                if (previous != previous_groups.end())
                    continue;
                std::fprintf(
                    stderr,
                    "[Render-Vehicle-Boundary-Transition] "
                    "sample=%llu frame=%llu poll=%d event=added "
                    "object=%u model=%08x commands=0->%zu "
                    "postClip=0/0/0->%zu/%zu/%zu "
                    "changedVertices=0->%zu geometry=0000000000000000->%016llx "
                    "fullScissor=0->%zu "
                    "previousScreen=0.00,0.00..0.00,0.00 "
                    "currentScreen=%.2f,%.2f..%.2f,%.2f\n",
                    static_cast<unsigned long long>(sample_index),
                    static_cast<unsigned long long>(draw_list.frame_index),
                    draw_list.input_poll,
                    current.object_id,
                    current.model_pointer,
                    current.commands,
                    current.post_clip_visible,
                    current.post_clip_rejected,
                    current.post_clip_nonfinite,
                    current.post_clip_changed_vertex_count,
                    static_cast<unsigned long long>(
                        current.geometry_signature),
                    current.full_display_scissors,
                    current.minimum_screen_x,
                    current.minimum_screen_y,
                    current.maximum_screen_x,
                    current.maximum_screen_y);
            }
        }
    }

    std::size_t resident_track_groups = 0;
    std::size_t resident_track_commands = 0;
    for (const auto& group : groups)
        if (group.object_kind == 1U && group.resident_course_commands != 0) {
            ++resident_track_groups;
            resident_track_commands += group.resident_course_commands;
        }
    if (emit_snapshot) {
    std::fprintf(
        stderr,
        "[Render-Scene] sample=%llu frame=%llu poll=%d source=%s "
        "camera=%016llx groups=%zu residentTrack=%zu/%zu "
        "commands=%zu screen=%zu "
        "secondaryExcluded=%u continuity=%s added=%zu removed=%zu "
        "trackCommandChanges=%zu culling=none "
        "native=%d,%d..%d,%d target=%.1f,%.1f..%.1f,%.1f\n",
        static_cast<unsigned long long>(sample_index),
        static_cast<unsigned long long>(draw_list.frame_index),
        draw_list.input_poll,
        options.synthetic_midpoint ? "midpoint" : "actual",
        static_cast<unsigned long long>(draw_list.camera_transform_id),
        groups.size(),
        resident_track_groups,
        resident_track_commands,
        draw_list.commands.size(),
        screen_commands,
        draw_list.secondary_commands,
        continuity_baseline ? "baseline" : "compared",
        added_groups,
        removed_groups,
        changed_track_groups,
        draw_list.display_x,
        draw_list.display_y,
        draw_list.display_x + draw_list.display_width,
        draw_list.display_y + draw_list.display_height,
        target_x0,
        native_y0,
        target_x1,
        native_y1);

    for (std::uint32_t kind = 0; kind <= 3U; ++kind) {
        for (std::uint32_t channel_index = 0; channel_index < 2U;
             ++channel_index) {
            const auto channel = channel_index == 0
                ? WorldViewChannel::main_view
                : WorldViewChannel::secondary_view;
            std::size_t class_groups = 0;
            std::size_t commands = 0;
            std::size_t textured = 0;
            std::size_t transparent = 0;
            std::size_t resident = 0;
            std::size_t native_intersections = 0;
            std::size_t target_intersections = 0;
            std::size_t hor_plus_only = 0;
            std::size_t left_margin = 0;
            std::size_t right_margin = 0;
            std::size_t clip_crossings = 0;
            std::size_t target_clip_crossings = 0;
            std::array<std::size_t, 6> target_clip_planes{};
            std::array<std::size_t, 6> hor_plus_clip_planes{};
            std::size_t trivial_rejects = 0;
            std::size_t target_trivial_rejects = 0;
            std::size_t near_crossings = 0;
            std::size_t target_near_crossings = 0;
            std::size_t behind_near = 0;
            std::size_t nonfinite = 0;
            std::size_t full_scissors = 0;
            float minimum_view_z = (std::numeric_limits<float>::max)();
            float maximum_view_z = (std::numeric_limits<float>::lowest)();
            float minimum_h = (std::numeric_limits<float>::max)();
            float maximum_h = (std::numeric_limits<float>::lowest)();
            float minimum_ofx = (std::numeric_limits<float>::max)();
            float maximum_ofx = (std::numeric_limits<float>::lowest)();
            float minimum_ofy = (std::numeric_limits<float>::max)();
            float maximum_ofy = (std::numeric_limits<float>::lowest)();
            for (const auto& group : groups) {
                if (
                    group.object_kind != kind ||
                    group.channel != channel
                )
                    continue;
                ++class_groups;
                commands += group.commands;
                textured += group.textured_commands;
                transparent += group.transparent_commands;
                resident += group.resident_course_commands;
                native_intersections += group.native_intersections;
                target_intersections += group.target_intersections;
                hor_plus_only += group.hor_plus_only;
                left_margin += group.left_margin_intersections;
                right_margin += group.right_margin_intersections;
                clip_crossings += group.clip_crossings;
                target_clip_crossings += group.target_clip_crossings;
                for (std::size_t plane = 0;
                     plane < target_clip_planes.size();
                     ++plane) {
                    target_clip_planes[plane] +=
                        group.target_clip_plane_crossings[plane];
                    hor_plus_clip_planes[plane] +=
                        group.hor_plus_clip_plane_crossings[plane];
                }
                trivial_rejects += group.trivial_clip_rejects;
                target_trivial_rejects +=
                    group.target_trivial_clip_rejects;
                near_crossings += group.near_crossings;
                target_near_crossings += group.target_near_crossings;
                behind_near += group.behind_near;
                nonfinite += group.nonfinite_commands;
                full_scissors += group.full_display_scissors;
                minimum_view_z = (std::min)(
                    minimum_view_z, group.minimum_view_z);
                maximum_view_z = (std::max)(
                    maximum_view_z, group.maximum_view_z);
                minimum_h = (std::min)(
                    minimum_h, group.minimum_projection_plane);
                maximum_h = (std::max)(
                    maximum_h, group.maximum_projection_plane);
                minimum_ofx = (std::min)(
                    minimum_ofx, group.minimum_projection_offset_x);
                maximum_ofx = (std::max)(
                    maximum_ofx, group.maximum_projection_offset_x);
                minimum_ofy = (std::min)(
                    minimum_ofy, group.minimum_projection_offset_y);
                maximum_ofy = (std::max)(
                    maximum_ofy, group.maximum_projection_offset_y);
            }
            if (commands == 0)
                continue;
            std::fprintf(
                stderr,
                "[Render-Scene-Class] sample=%llu channel=%s kind=%s "
                "groups=%zu commands=%zu resident=%zu textured=%zu "
                "transparent=%zu "
                "target=%zu native=%zu horPlusOnly=%zu margins=%zu/%zu "
                "clipCross=%zu/%zu trivialReject=%zu/%zu "
                "nearCross=%zu/%zu "
                "behindNear=%zu nonfinite=%zu fullScissor=%zu/%zu "
                "targetClipPlanes=%zu/%zu/%zu/%zu/%zu/%zu "
                "horPlusClipPlanes=%zu/%zu/%zu/%zu/%zu/%zu "
                "viewZ=%.1f..%.1f H=%.1f..%.1f "
                "projectionOffset=(%.1f..%.1f,%.1f..%.1f)\n",
                static_cast<unsigned long long>(sample_index),
                scene_diagnostic_channel_name(channel),
                scene_diagnostic_kind_name(kind),
                class_groups,
                commands,
                resident,
                textured,
                transparent,
                target_intersections,
                native_intersections,
                hor_plus_only,
                left_margin,
                right_margin,
                clip_crossings,
                target_clip_crossings,
                trivial_rejects,
                target_trivial_rejects,
                near_crossings,
                target_near_crossings,
                behind_near,
                nonfinite,
                full_scissors,
                commands,
                target_clip_planes[0],
                target_clip_planes[1],
                target_clip_planes[2],
                target_clip_planes[3],
                target_clip_planes[4],
                target_clip_planes[5],
                hor_plus_clip_planes[0],
                hor_plus_clip_planes[1],
                hor_plus_clip_planes[2],
                hor_plus_clip_planes[3],
                hor_plus_clip_planes[4],
                hor_plus_clip_planes[5],
                minimum_view_z,
                maximum_view_z,
                minimum_h,
                maximum_h,
                minimum_ofx / 65536.0F,
                maximum_ofx / 65536.0F,
                minimum_ofy / 65536.0F,
                maximum_ofy / 65536.0F);
        }
    }
    }

    if (std::getenv("OPENGT_RENDER_SCENE_DIAGNOSTICS_VERBOSE") != nullptr) {
        std::vector<SceneUnownedDiagnosticGroup> unowned_groups;
        unowned_groups.reserve(16);
        for (const auto& command : draw_list.commands) {
            const auto& material = draw_list.materials[command.material_index];
            if (
                command.object_kind != 0U ||
                (material.primitive_flags &
                    world_primitive_screen_space_flag) != 0
            ) {
                continue;
            }
            auto found = std::find_if(
                unowned_groups.begin(), unowned_groups.end(),
                [&] (const SceneUnownedDiagnosticGroup& group) {
                    return same_scene_unowned_diagnostic_group(
                        group, command, material);
                });
            if (found == unowned_groups.end()) {
                unowned_groups.push_back(SceneUnownedDiagnosticGroup{});
                found = unowned_groups.end() - 1;
                found->transform_id = command.transform_id;
                found->primitive_flags = material.primitive_flags;
                found->texture_page = material.texture_page;
                found->clut = material.clut;
                found->clip_x0 = command.clip_x0;
                found->clip_y0 = command.clip_y0;
                found->clip_x1 = command.clip_x1;
                found->clip_y1 = command.clip_y1;
                found->channel = command.channel;
                found->exact_transform_valid =
                    command.exact_transform_valid;
                found->translation_x = command.transform_translation[0];
                found->translation_y = command.transform_translation[1];
                found->translation_z = command.transform_translation[2];
            }
            auto& group = *found;
            ++group.commands;
            group.transparent_commands +=
                (material.primitive_flags & semi_transparent_flag) != 0
                    ? 1U
                    : 0U;
            group.minimum_source_command = (std::min)(
                group.minimum_source_command,
                command.source_command_index);
            group.maximum_source_command = (std::max)(
                group.maximum_source_command,
                command.source_command_index);
            for (const auto& vertex : command.vertices) {
                group.minimum_model_x = (std::min)(
                    group.minimum_model_x,
                    static_cast<float>(vertex.model_x));
                group.minimum_model_y = (std::min)(
                    group.minimum_model_y,
                    static_cast<float>(vertex.model_y));
                group.minimum_model_z = (std::min)(
                    group.minimum_model_z,
                    static_cast<float>(vertex.model_z));
                group.maximum_model_x = (std::max)(
                    group.maximum_model_x,
                    static_cast<float>(vertex.model_x));
                group.maximum_model_y = (std::max)(
                    group.maximum_model_y,
                    static_cast<float>(vertex.model_y));
                group.maximum_model_z = (std::max)(
                    group.maximum_model_z,
                    static_cast<float>(vertex.model_z));
                group.minimum_screen_x = (std::min)(
                    group.minimum_screen_x, vertex.screen_x);
                group.minimum_screen_y = (std::min)(
                    group.minimum_screen_y, vertex.screen_y);
                group.maximum_screen_x = (std::max)(
                    group.maximum_screen_x, vertex.screen_x);
                group.maximum_screen_y = (std::max)(
                    group.maximum_screen_y, vertex.screen_y);
                group.minimum_view_z = (std::min)(
                    group.minimum_view_z, vertex.view_z);
                group.maximum_view_z = (std::max)(
                    group.maximum_view_z, vertex.view_z);
                group.minimum_projection_plane = (std::min)(
                    group.minimum_projection_plane,
                    vertex.projection_plane);
                group.maximum_projection_plane = (std::max)(
                    group.maximum_projection_plane,
                    vertex.projection_plane);
            }
        }
        for (std::size_t group_index = 0;
             group_index < unowned_groups.size() && group_index < 64;
             ++group_index) {
            const auto& group = unowned_groups[group_index];
            std::fprintf(
                stderr,
                "[Render-Scene-Unowned] sample=%llu frame=%llu poll=%d "
                "group=%zu/%zu channel=%s transform=%016llx exact=%d "
                "translation=%d,%d,%d flags=%08x tpage=%04x clut=%04x "
                "scissor=%d,%d..%d,%d commands=%zu transparent=%zu "
                "source=%u..%u model=%.0f,%.0f,%.0f..%.0f,%.0f,%.0f "
                "screen=%.1f,%.1f..%.1f,%.1f viewZ=%.1f..%.1f "
                "H=%.1f..%.1f\n",
                static_cast<unsigned long long>(sample_index),
                static_cast<unsigned long long>(draw_list.frame_index),
                draw_list.input_poll,
                group_index,
                unowned_groups.size(),
                scene_diagnostic_channel_name(group.channel),
                static_cast<unsigned long long>(group.transform_id),
                group.exact_transform_valid ? 1 : 0,
                group.translation_x,
                group.translation_y,
                group.translation_z,
                group.primitive_flags,
                group.texture_page,
                group.clut,
                group.clip_x0,
                group.clip_y0,
                group.clip_x1,
                group.clip_y1,
                group.commands,
                group.transparent_commands,
                group.minimum_source_command,
                group.maximum_source_command,
                group.minimum_model_x,
                group.minimum_model_y,
                group.minimum_model_z,
                group.maximum_model_x,
                group.maximum_model_y,
                group.maximum_model_z,
                group.minimum_screen_x,
                group.minimum_screen_y,
                group.maximum_screen_x,
                group.maximum_screen_y,
                group.minimum_view_z,
                group.maximum_view_z,
                group.minimum_projection_plane,
                group.maximum_projection_plane);
        }
        std::size_t margin_details = 0;
        for (const auto& group : groups) {
            if (
                group.object_kind != 1U ||
                group.hor_plus_only == 0 ||
                margin_details++ >= 64
            )
                continue;
            std::fprintf(
                stderr,
                "[Render-Scene-Margin-Group] sample=%llu frame=%llu poll=%d "
                "channel=%s object=%u model=%08x commands=%zu "
                "target=%zu native=%zu horPlusOnly=%zu margins=%zu/%zu "
                "targetClipPlanes=%zu/%zu/%zu/%zu/%zu/%zu "
                "horPlusClipPlanes=%zu/%zu/%zu/%zu/%zu/%zu "
                "screen=%.1f,%.1f..%.1f,%.1f viewZ=%.1f..%.1f\n",
                static_cast<unsigned long long>(sample_index),
                static_cast<unsigned long long>(draw_list.frame_index),
                draw_list.input_poll,
                scene_diagnostic_channel_name(group.channel),
                group.object_id,
                group.model_pointer,
                group.commands,
                group.target_intersections,
                group.native_intersections,
                group.hor_plus_only,
                group.left_margin_intersections,
                group.right_margin_intersections,
                group.target_clip_plane_crossings[0],
                group.target_clip_plane_crossings[1],
                group.target_clip_plane_crossings[2],
                group.target_clip_plane_crossings[3],
                group.target_clip_plane_crossings[4],
                group.target_clip_plane_crossings[5],
                group.hor_plus_clip_plane_crossings[0],
                group.hor_plus_clip_plane_crossings[1],
                group.hor_plus_clip_plane_crossings[2],
                group.hor_plus_clip_plane_crossings[3],
                group.hor_plus_clip_plane_crossings[4],
                group.hor_plus_clip_plane_crossings[5],
                group.minimum_screen_x,
                group.minimum_screen_y,
                group.maximum_screen_x,
                group.maximum_screen_y,
                group.minimum_view_z,
                group.maximum_view_z);
        }
    }

    // Object-level continuity is useful on its own when a resident prop
    // flickers.  Keep it independently selectable: the broader verbose mode
    // also emits every Hor+ margin group and can bury a one-frame transition
    // in tens of thousands of unrelated records during a long replay.
    if (
        !continuity_baseline &&
        (std::getenv("OPENGT_RENDER_SCENE_DIAGNOSTICS_VERBOSE") != nullptr ||
            std::getenv(
                "OPENGT_RENDER_SCENE_TRANSITION_DIAGNOSTICS") != nullptr ||
            continuity_audit)
    ) {
        std::size_t details = 0;
        for (const auto& previous : previous_groups) {
            const auto current = std::find_if(
                groups.begin(), groups.end(),
                [&] (const SceneDiagnosticGroup& candidate) {
                    return same_scene_diagnostic_group(previous, candidate);
                });
            const char* event = nullptr;
            std::size_t current_commands = 0;
            if (current == groups.end()) {
                if (continuity_audit &&
                    previous.target_intersections == 0)
                    continue;
                event = "missing";
            } else if (
                !continuity_audit &&
                previous.object_kind == 1U &&
                (previous.resident_course_commands != 0 ||
                    current->resident_course_commands != 0) &&
                previous.commands != current->commands
            ) {
                event = "track-command-count";
                current_commands = current->commands;
            }
            if (event == nullptr || details++ >= 64)
                continue;
            std::fprintf(
                stderr,
                "[Render-Scene-Transition] sample=%llu frame=%llu poll=%d "
                "camera=%016llx event=%s "
                "channel=%s kind=%s object=%u model=%08x commands=%zu->%zu "
                "previousTarget=%zu previousNative=%zu "
                "previousScreen=%.1f,%.1f..%.1f,%.1f "
                "previousViewZ=%.1f..%.1f\n",
                static_cast<unsigned long long>(sample_index),
                static_cast<unsigned long long>(draw_list.frame_index),
                draw_list.input_poll,
                static_cast<unsigned long long>(draw_list.camera_transform_id),
                event,
                scene_diagnostic_channel_name(previous.channel),
                scene_diagnostic_kind_name(previous.object_kind),
                previous.object_id,
                previous.model_pointer,
                previous.commands,
                current_commands,
                previous.target_intersections,
                previous.native_intersections,
                previous.minimum_screen_x,
                previous.minimum_screen_y,
                previous.maximum_screen_x,
                previous.maximum_screen_y,
                previous.minimum_view_z,
                previous.maximum_view_z);
        }
        for (const auto& current : groups) {
            const auto previous = std::find_if(
                previous_groups.begin(), previous_groups.end(),
                [&] (const SceneDiagnosticGroup& candidate) {
                    return same_scene_diagnostic_group(current, candidate);
                });
            if (previous != previous_groups.end() ||
                (continuity_audit && current.target_intersections == 0) ||
                details++ >= 64)
                continue;
            std::fprintf(
                stderr,
                "[Render-Scene-Transition] sample=%llu frame=%llu poll=%d "
                "camera=%016llx event=added "
                "channel=%s kind=%s object=%u model=%08x commands=0->%zu "
                "currentTarget=%zu currentNative=%zu "
                "currentScreen=%.1f,%.1f..%.1f,%.1f "
                "currentViewZ=%.1f..%.1f\n",
                static_cast<unsigned long long>(sample_index),
                static_cast<unsigned long long>(draw_list.frame_index),
                draw_list.input_poll,
                static_cast<unsigned long long>(draw_list.camera_transform_id),
                scene_diagnostic_channel_name(current.channel),
                scene_diagnostic_kind_name(current.object_kind),
                current.object_id,
                current.model_pointer,
                current.commands,
                current.target_intersections,
                current.native_intersections,
                current.minimum_screen_x,
                current.minimum_screen_y,
                current.maximum_screen_x,
                current.maximum_screen_y,
                current.minimum_view_z,
                current.maximum_view_z);
        }
    }
    previous_groups = std::move(groups);
    continuity_has_baseline = true;
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
    const WorldMaterial& right_material,
    bool left_alpha_tested_cutout,
    bool right_alpha_tested_cutout,
    bool left_road_support,
    bool right_road_support,
    bool left_vehicle_reflection_support,
    bool right_vehicle_reflection_support
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
        track_overlay_layer(left_material.primitive_flags) ==
            track_overlay_layer(right_material.primitive_flags) &&
        (left_material.primitive_flags &
            world_primitive_track_overlay_support_flag) ==
            (right_material.primitive_flags &
                world_primitive_track_overlay_support_flag) &&
        (left_material.primitive_flags &
            world_primitive_track_replacement_flag) ==
            (right_material.primitive_flags &
                world_primitive_track_replacement_flag) &&
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
        left_alpha_tested_cutout == right_alpha_tested_cutout &&
        left_road_support == right_road_support &&
        left_vehicle_reflection_support ==
            right_vehicle_reflection_support &&
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

std::array<double, 3> track_surface_face_normal(
    const WorldDrawCommand& command
) noexcept {
    const auto& a = command.vertices[0];
    const auto& b = command.vertices[1];
    const auto& c = command.vertices[2];
    const auto face_normal = [] (
        double ax,
        double ay,
        double az,
        double bx,
        double by,
        double bz,
        double cx,
        double cy,
        double cz
    ) noexcept {
        const double ab_x = bx - ax;
        const double ab_y = by - ay;
        const double ab_z = bz - az;
        const double ac_x = cx - ax;
        const double ac_y = cy - ay;
        const double ac_z = cz - az;
        return std::array<double, 3>{
            ab_y * ac_z - ab_z * ac_y,
            ab_z * ac_x - ab_x * ac_z,
            ab_x * ac_y - ab_y * ac_x,
        };
    };
    auto normal = face_normal(
        a.world_x, a.world_y, a.world_z,
        b.world_x, b.world_y, b.world_z,
        c.world_x, c.world_y, c.world_z);
    const bool finite_world_normal =
        std::isfinite(normal[0]) &&
        std::isfinite(normal[1]) &&
        std::isfinite(normal[2]);
    const double world_normal_extent = finite_world_normal
        ? (std::max)({
            std::abs(normal[0]),
            std::abs(normal[1]),
            std::abs(normal[2])})
        : 0.0;
    if (world_normal_extent <= 1.0e-9) {
        // Unit tests and deliberately synthetic draw lists may supply only
        // authored model coordinates. Real course commands carry transformed
        // world coordinates and must never be classified in local object
        // space: scenery models can rotate a locally horizontal primitive
        // into an upright treeline or fence.
        normal = face_normal(
            a.model_x, a.model_y, a.model_z,
            b.model_x, b.model_y, b.model_z,
            c.model_x, c.model_y, c.model_z);
    }
    return normal;
}

bool opaque_track_material_candidate(
    const WorldDrawCommand& command,
    const WorldMaterial& material
) noexcept {
    return command.object_kind == 1U &&
        (material.primitive_flags & textured_flag) != 0 &&
        (material.primitive_flags & semi_transparent_flag) == 0 &&
        (material.primitive_flags & world_primitive_screen_space_flag) == 0;
}

bool is_opaque_track_surface(
    const WorldDrawCommand& command,
    const WorldMaterial& material
) noexcept {
    const auto normal = track_surface_face_normal(command);
    // GT2's transformed course coordinates are Z-up. An XY ground plane has
    // a +/-Z world normal. World-space classification keeps rotated foliage
    // and fences in the keyed-cutout path while ordinary asphalt remains a
    // solid recovery surface.
    return opaque_track_material_candidate(command, material) &&
        std::abs(normal[2]) >= std::abs(normal[0]) &&
        std::abs(normal[2]) >= std::abs(normal[1]);
}

struct TrackSurfaceVertexKey {
    std::int16_t x{};
    std::int16_t y{};
    std::int16_t z{};

    bool operator==(const TrackSurfaceVertexKey& other) const noexcept {
        return x == other.x && y == other.y && z == other.z;
    }

    bool operator<(const TrackSurfaceVertexKey& other) const noexcept {
        if (x != other.x) return x < other.x;
        if (y != other.y) return y < other.y;
        return z < other.z;
    }
};

struct TrackSurfaceEdgeKey {
    std::uint32_t object_id{};
    std::uint32_t model_pointer{};
    std::uint64_t transform_id{};
    WorldViewChannel channel{};
    TrackSurfaceVertexKey first{};
    TrackSurfaceVertexKey second{};

    bool operator==(const TrackSurfaceEdgeKey& other) const noexcept {
        return object_id == other.object_id &&
            model_pointer == other.model_pointer &&
            transform_id == other.transform_id &&
            channel == other.channel &&
            first == other.first && second == other.second;
    }
};

struct TrackSurfaceEdgeHash {
    std::size_t operator()(const TrackSurfaceEdgeKey& key) const noexcept {
        std::size_t result = 1469598103934665603ULL;
        const auto mix = [&result](std::uint64_t value) {
            result ^= static_cast<std::size_t>(value);
            result *= 1099511628211ULL;
        };
        mix(key.object_id);
        mix(key.model_pointer);
        mix(key.transform_id);
        mix(static_cast<std::uint8_t>(key.channel));
        for (const auto& vertex : {key.first, key.second}) {
            mix(static_cast<std::uint16_t>(vertex.x));
            mix(static_cast<std::uint16_t>(vertex.y));
            mix(static_cast<std::uint16_t>(vertex.z));
        }
        return result;
    }
};

bool smoothly_connected_track_surfaces(
    const WorldDrawCommand& left,
    const WorldDrawCommand& right
) noexcept {
    const auto left_normal = track_surface_face_normal(left);
    const auto right_normal = track_surface_face_normal(right);
    const double left_length_squared =
        left_normal[0] * left_normal[0] +
        left_normal[1] * left_normal[1] +
        left_normal[2] * left_normal[2];
    const double right_length_squared =
        right_normal[0] * right_normal[0] +
        right_normal[1] * right_normal[1] +
        right_normal[2] * right_normal[2];
    if (left_length_squared <= 1.0e-18 ||
        right_length_squared <= 1.0e-18)
        return false;
    const double dot =
        left_normal[0] * right_normal[0] +
        left_normal[1] * right_normal[1] +
        left_normal[2] * right_normal[2];
    // Winding can reverse between neighboring GT2 packets.  Treat faces as
    // one continuous authored surface when their unsigned dihedral angle is
    // at most sixty degrees.  Hard folds remain independent, so a fence or
    // foliage card attached to terrain cannot inherit solid-ground behavior.
    return dot * dot >=
        0.25 * left_length_squared * right_length_squared;
}

std::vector<std::uint8_t> opaque_track_surface_eligibility(
    const WorldDrawList& draw_list
) {
    const std::size_t count = draw_list.commands.size();
    std::vector<std::size_t> parent(count);
    std::vector<std::uint8_t> rank(count, 0U);
    std::vector<std::uint8_t> eligible(count, 0U);
    std::vector<std::uint8_t> resident_candidate(count, 0U);
    for (std::size_t index = 0; index < count; ++index) {
        parent[index] = index;
        const auto& command = draw_list.commands[index];
        if (command.material_index >= draw_list.materials.size())
            continue;
        const auto& material = draw_list.materials[command.material_index];
        eligible[index] = is_opaque_track_surface(command, material)
            ? 1U
            : 0U;
        resident_candidate[index] =
            opaque_track_material_candidate(command, material) &&
            (material.primitive_flags &
                world_primitive_resident_course_flag) != 0
            ? 1U
            : 0U;
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
        return TrackSurfaceVertexKey{
            vertex.model_x,
            vertex.model_y,
            vertex.model_z,
        };
    };
    std::unordered_multimap<
        TrackSurfaceEdgeKey,
        std::size_t,
        TrackSurfaceEdgeHash> edges;
    edges.reserve(count * 2U);
    for (std::size_t command_index = 0;
         command_index < count;
         ++command_index) {
        if (resident_candidate[command_index] == 0)
            continue;
        const auto& command = draw_list.commands[command_index];
        for (int edge_index = 0; edge_index < 3; ++edge_index) {
            auto first = vertex_key(command.vertices[edge_index]);
            auto second = vertex_key(
                command.vertices[(edge_index + 1) % 3]);
            if (second < first)
                std::swap(first, second);
            const TrackSurfaceEdgeKey key{
                command.object_id,
                command.model_pointer,
                command.transform_id,
                command.channel,
                first,
                second,
            };
            const auto matching = edges.equal_range(key);
            for (auto found = matching.first;
                 found != matching.second;
                 ++found) {
                if (smoothly_connected_track_surfaces(
                        draw_list.commands[found->second], command))
                    unite(found->second, command_index);
            }
            edges.emplace(key, command_index);
        }
    }
    std::vector<std::uint8_t> component_has_ground(count, 0U);
    for (std::size_t index = 0; index < count; ++index) {
        if (eligible[index] != 0)
            component_has_ground[find_root(index)] = 1U;
    }
    for (std::size_t index = 0; index < count; ++index) {
        if (resident_candidate[index] != 0 &&
            component_has_ground[find_root(index)] != 0)
            eligible[index] = 1U;
    }
    return eligible;
}

bool alpha_tested_track_cutout_candidate(
    const WorldDrawCommand& command,
    const WorldMaterial& material,
    bool opaque_track_surface
) noexcept {
    return opaque_track_material_candidate(command, material) &&
        !opaque_track_surface;
}

bool perspective_uv_eligible(
    const WorldDrawCommand& command,
    const WorldMaterial& material
) {
    if ((material.primitive_flags & textured_flag) == 0 ||
        (material.primitive_flags & world_primitive_screen_space_flag) != 0)
        return false;

    // Every remaining textured 3D command carries either GT2's authored
    // pre-projection course surface or reconstructed model/view/clip state.
    // Hardware interpolation is therefore the sole normal world-texture
    // contract.
    (void)command;
    return true;
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
    // Resident pre-projection course surfaces already have one unambiguous
    // perspective contract and never need the legacy packet-island search.
    // Reserve for only the remaining packet geometry; the conservative count
    // avoids a large per-frame table allocation on a fully resident course.
    edges.reserve(count);
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
        if ((material.primitive_flags &
                world_primitive_resident_course_flag) != 0)
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
    static thread_local bool emitted_shipping_uv_contract = false;
    const bool shipping_uv_contract =
        !emitted_shipping_uv_contract &&
        draw_list.track_commands >= 1000U;
    if (
        std::getenv("OPENGT_RENDER_UV_DIAGNOSTICS") != nullptr ||
        shipping_uv_contract
    ) {
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
            "islandPerspective=%zu worldFallback=%zu "
            "trackFallback=%zu vehicleFallback=%zu otherFallback=%zu "
            "edgeKeys=%zu largestIsland=%zu projection=fixed-modern\n",
            count,
            textured_commands,
            individually_eligible,
            island_eligible_count,
            textured_commands - island_eligible_count,
            track_fallback,
            vehicle_fallback,
            other_fallback,
            edges.size(),
            largest_island);
        if (shipping_uv_contract)
            emitted_shipping_uv_contract = true;
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
    std::array<float, 4> depth_plane;
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

constexpr std::size_t maximum_screen_arcs = 32;

struct alignas(16) GpuScreenArc {
    float center_x;
    float center_y;
    float radius_x;
    float radius_y;
    std::uint32_t orientation;
    float padding[3];
};

struct alignas(16) ScreenArcConstants {
    std::uint32_t count;
    float padding[3];
    GpuScreenArc arcs[maximum_screen_arcs];
};

static_assert(sizeof(GpuScreenArc) == 32);
static_assert(sizeof(ScreenArcConstants) % 16 == 0);

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
    float4 depthPlane;
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
    float depth : SV_Depth;
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

)" R"(

struct FootprintSample {
    float3 color;
    float coverage;
    uint word;
    bool any;
};

float3 FilteredTextureColor(
    float2 uv,
    uint centerWord,
    MaterialData material);

// A pixel that covers more than one source texel has to decide transparency
// from everything it covers. GT2 keys transparency on word==0, so a single
// point sample of a minified texture drops whatever fraction of pixels happen
// to land on the key. A fixed 3x3 grid still undersampled Seattle road spans
// covering 8..31 texels in one direction and could hit nine transparent words
// despite substantial opaque coverage between them. Follow the oriented
// derivatives with a bounded anisotropic grid instead: up to sixteen taps on
// the major axis and four on the minor axis. This is the indexed-VRAM
// equivalent of modern anisotropic minification; it preserves dynamic CLUTs
// and only discards a footprint whose sampled source coverage is all zero.
FootprintSample SampleFootprint(
    float2 uv,
    float2 uvDx,
    float2 uvDy,
    MaterialData material
) {
    FootprintSample result;
    result.color = 0.0;
    result.coverage = 0.0;
    result.word = 0;
    result.any = false;
    float coverage = 0.0;
    float nearest = 1.0e9;
    float dxLengthSquared = dot(uvDx, uvDx);
    float dyLengthSquared = dot(uvDy, uvDy);
    float2 major = dxLengthSquared >= dyLengthSquared ? uvDx : uvDy;
    float2 minor = dxLengthSquared >= dyLengthSquared ? uvDy : uvDx;
    float majorLength = length(major);
    float minorLength = length(minor);
    bool compactFootprint = majorLength < 2.0 && minorLength < 2.0;
    int majorTaps = clamp((int)ceil(majorLength), 3, 16);
    // Retain three transverse samples even for a sub-texel minor axis. This
    // supplies the coverage component a bilinear anisotropic tap would have;
    // one point at the footprint centre can sit on the transparent side of a
    // texel boundary while the same pixel still covers its opaque neighbor.
    int minorTaps = clamp((int)ceil(minorLength), 3, 4);
    [loop]
    for (int majorIndex = 0; majorIndex < majorTaps; ++majorIndex) {
        float majorAmount =
            ((majorIndex + 0.5) / majorTaps) - 0.5;
        [loop]
        for (int minorIndex = 0; minorIndex < minorTaps; ++minorIndex) {
            float minorAmount =
                ((minorIndex + 0.5) / minorTaps) - 0.5;
            // ddx and ddy span the actual pixel footprint in texture space.
            // Sampling their oriented parallelogram avoids a broad
            // axis-aligned box that would blur oblique road tiles.
            float2 offset =
                majorAmount * major + minorAmount * minor;
            float2 sampleUv = uv + offset;
            uint word = TextureWord(
                (int)floor(sampleUv.x + 0.0001),
                (int)floor(sampleUv.y + 0.0001),
                material);
            if (word == 0)
                continue;
            // A footprint sample is itself continuous in texture space.
            // Filtering nearest texels here made the renderer jump from
            // bilinear sampling to a phase-dependent point grid as soon as
            // the footprint crossed one texel, so thin arrows and road marks
            // appeared to rotate or swap detail with distance.
            result.color += compactFootprint
                ? FilteredTextureColor(sampleUv, word, material)
                : TextureColor(word);
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
        result.coverage = coverage / (majorTaps * minorTaps);
        result.any = true;
    }
    return result;
}

)" R"(

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

PsOutput ShadePixel(VsOutput input, bool preserveCutoutCoverage) {
    MaterialData material = Materials[input.commandIndex];
    bool textured = (material.primitiveFlags & 1) != 0;
    bool semitransparent = (material.primitiveFlags & 2) != 0;
    bool rawTexture = (material.primitiveFlags & 4) != 0;
    bool screenSpace =
        (material.primitiveFlags & 0x80000000) != 0;
    bool perspectiveEligible =
        (material.coverageFlags & 8) != 0;
    float2 uv = perspectiveEligible
        ? input.perspectiveUv
        : input.affineUv;
    float3 color = saturate(input.color.rgb);
    bool textureStp = false;
    float cutoutCoverage = 1.0;
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
        float2 uvDx = ddx(uv);
        float2 uvDy = ddy(uv);
        float footprint = max(
            max(abs(uvDx.x), abs(uvDx.y)),
            max(abs(uvDy.x), abs(uvDy.y)));
        bool minified =
            FootprintCoverage != 0 &&
            !screenSpace && footprint > 1.0;
        uint word = TextureWord(sampleU, sampleV, material);
        uint centerWord = word;
        float3 recoveredColor = 0.0;
        bool coveredTexel = false;
        if (minified) {
            FootprintSample covered = SampleFootprint(
                uv, uvDx, uvDy, material);
            if (!covered.any)
                discard;
            // Blend continuously out of the ordinary point-visibility result
            // over the first minification octave.  This applies to both the
            // recovered color and keyed-alpha coverage, so crossing a one-
            // texel footprint cannot make a fence or leaf suddenly appear.
            float areaWeight = saturate(footprint - 1.0);
            // Horizontal opaque course surfaces use the coverage sampler to
            // recover thin road artwork and must remain solid.  Other opaque
            // world materials with keyed transparency are cutouts: preserve
            // their fractional footprint coverage instead of promoting a
            // visible source texel to a fully opaque distant fence or tree.
            // Returning continuous alpha avoids the detached full-opacity
            // pixels created by the retired ordered screen-door test.
            [branch]
            if (
                preserveCutoutCoverage &&
                !semitransparent &&
                (material.coverageFlags & 1) == 0
            ) {
                float pointCoverage = centerWord != 0 ? 1.0 : 0.0;
                cutoutCoverage = lerp(
                    pointCoverage, covered.coverage, areaWeight);
                if (cutoutCoverage <= 0.0001)
                    discard;
            }
            word = covered.word;
            recoveredColor = covered.color;
            if (centerWord != 0 && areaWeight < 1.0) {
                recoveredColor = lerp(
                    FilteredTextureColor(uv, centerWord, material),
                    recoveredColor,
                    areaWeight);
            }
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
    precise float planeDepth = material.depthPlane.x * input.position.x +
        material.depthPlane.y * input.position.y + material.depthPlane.z;
    output.depth = material.depthPlane.w != 0.0 ? saturate(planeDepth) : input.position.z;
    output.color = float4(
        color,
        vehicleShadow ? input.color.a : cutoutCoverage);
    float sourceFactor = 1.0;
    float destinationFactor = 0.0;
    if (PassKind == 2 && semitransparent && textureStp) {
        uint blendMode = (material.texturePage >> 5) & 3;
        sourceFactor = blendMode == 0 ? 0.5 :
            blendMode == 3 ? 0.25 : 1.0;
        destinationFactor = blendMode == 0 ? 0.5 : 1.0;
        // Preserve the authored blend strength, including vehicle reflections.
        // Surface alignment and depth resolve their visibility independently.
    }
    output.blendFactors = float4(
        destinationFactor,
        destinationFactor,
        destinationFactor,
        sourceFactor);
    return output;
}

PsOutput PSMain(VsOutput input) {
    return ShadePixel(input, false);
}

PsOutput PSMainCutoutDepth(VsOutput input) {
    PsOutput output = ShadePixel(input, true);
    if (output.color.a < 0.5)
        discard;
    return output;
}

PsOutput PSMainCutoutFringe(VsOutput input) {
    PsOutput output = ShadePixel(input, true);
    if (output.color.a >= 0.5)
        discard;
    return output;
}

PsOutput PSMainRoadOverlay(VsOutput input) {
    // GT2 authors road paint as separate geometry. Its priority is resolved
    // by the typed road-owner stencil contract, not a numeric depth bias:
    // resident model units are transformed at track-dependent scales, so a
    // fixed view-space offset cannot guarantee the authored relationship.
    return ShadePixel(input, false);
}

PsOutput PSMainRoadOverlayTint(VsOutput input) {
    PsOutput shaded = ShadePixel(input, false);
    PsOutput output;
    output.color = float4(0.0, 1.0, 1.0, 1.0);
    output.blendFactors = shaded.blendFactors;
    output.depth = shaded.depth;
    return output;
}

PsOutput PSMainCommandId(VsOutput input) {
    PsOutput output = ShadePixel(input, false);
    uint encoded = input.commandIndex + 1;
    output.color = float4(
        float(encoded & 255) / 255.0,
        float((encoded >> 8) & 255) / 255.0,
        float((encoded >> 16) & 255) / 255.0,
        1.0);
    return output;
}

float4 PSMainScreenGridMask(VsOutput input) : SV_Target0 {
    MaterialData material = Materials[input.commandIndex];
    if ((material.primitiveFlags & 1) != 0) {
        bool perspectiveEligible = (material.coverageFlags & 8) != 0;
        float2 uv = perspectiveEligible
            ? input.perspectiveUv
            : input.affineUv;
        int sampleU = (int)floor(uv.x + 0.0001);
        int sampleV = (int)floor(uv.y + 0.0001);
        if (TextureWord(sampleU, sampleV, material) == 0)
            discard;
    }
    return 1.0;
}

)";

const char screen_grid_shader_source[] = R"(
Texture2D<float4> ScreenGridSource : register(t3);
Texture2D<float> ScreenGridMask : register(t4);
Texture2D<float4> ScreenGridWorld : register(t5);

struct ScreenArc {
    float2 center;
    float2 radius;
    uint orientation;
    float3 padding;
};

cbuffer ScreenArcs : register(b1) {
    uint ScreenArcCount;
    float3 ScreenArcPadding;
    ScreenArc ScreenArcData[32];
};

struct ScreenGridVertex {
    float4 position : SV_Position;
};

ScreenGridVertex VSMainScreenGrid(uint vertexId : SV_VertexID) {
    float2 positions[3] = {
        float2(-1.0, -1.0),
        float2(-1.0,  3.0),
        float2( 3.0, -1.0)
    };
    ScreenGridVertex output;
    output.position = float4(positions[vertexId], 0.0, 1.0);
    return output;
}

float4 PSMainScreenGrid(ScreenGridVertex input) : SV_Target0 {
    uint outputWidth;
    uint outputHeight;
    uint maskWidth;
    uint maskHeight;
    ScreenGridSource.GetDimensions(outputWidth, outputHeight);
    ScreenGridMask.GetDimensions(maskWidth, maskHeight);
    int2 outputPixel = int2(input.position.xy);
    int scale = max(1, (int)(outputHeight / max(maskHeight, 1U)));
    float2 nativePosition =
        (float2(outputPixel) + 0.5) / (float)scale - 0.5;
    int2 nativeBase = int2(floor(nativePosition));
    float2 nativeBlend = frac(nativePosition);
    int2 maximumNative = int2((int)maskWidth - 1, (int)maskHeight - 1);
    int2 nativePixels[4] = {
        clamp(nativeBase, int2(0, 0), maximumNative),
        clamp(nativeBase + int2(1, 0), int2(0, 0), maximumNative),
        clamp(nativeBase + int2(0, 1), int2(0, 0), maximumNative),
        clamp(nativeBase + int2(1, 1), int2(0, 0), maximumNative)
    };
    float weights[4] = {
        (1.0 - nativeBlend.x) * (1.0 - nativeBlend.y),
        nativeBlend.x * (1.0 - nativeBlend.y),
        (1.0 - nativeBlend.x) * nativeBlend.y,
        nativeBlend.x * nativeBlend.y
    };
    float coverage = 0.0;
    float4 authored = 0.0;
    int2 maximumOutput = int2((int)outputWidth - 1, (int)outputHeight - 1);
    [unroll]
    for (int index = 0; index < 4; ++index) {
        coverage += ScreenGridMask.Load(
            int3(nativePixels[index], 0)) * weights[index];
        int2 nativeSample = min(
            nativePixels[index] * scale + scale / 2,
            maximumOutput);
        authored += ScreenGridSource.Load(
            int3(nativeSample, 0)) * weights[index];
    }
    float2 pixelCenter = float2(outputPixel) + 0.5;
    bool analyticArc = false;
    [loop]
    for (uint index = 0; index < ScreenArcCount; ++index) {
        ScreenArc arc = ScreenArcData[index];
        float2 delta = pixelCenter - arc.center;
        bool inHalf = arc.orientation == 0 ? delta.x <= 0.0 :
            arc.orientation == 1 ? delta.x >= 0.0 :
            arc.orientation == 2 ? delta.y <= 0.0 : delta.y >= 0.0;
        bool inBounds = arc.orientation < 2
            ? abs(delta.y) <= arc.radius.y + 1.5 &&
                abs(delta.x) <= arc.radius.x + 1.5
            : abs(delta.x) <= arc.radius.x + 1.5 &&
                abs(delta.y) <= arc.radius.y + 1.5;
        if (inHalf && inBounds) {
            analyticArc = true;
            float normalizedRadius = length(delta / arc.radius);
            float signedPixels = (1.0 - normalizedRadius) *
                min(arc.radius.x, arc.radius.y);
            coverage = saturate(signedPixels + 0.5);
            float inwardScale = min(
                1.0,
                0.80 / max(normalizedRadius, 0.001));
            int2 inwardPixel = clamp(
                int2(arc.center + delta * inwardScale),
                int2(0, 0),
                maximumOutput);
            authored = ScreenGridSource.Load(int3(inwardPixel, 0));
        }
    }
    float4 world = ScreenGridWorld.Load(int3(outputPixel, 0));
    if (coverage >= 0.999)
        return analyticArc
            ? authored
            : ScreenGridSource.Load(int3(outputPixel, 0));
    return lerp(world, authored, saturate(coverage));
}
)";

bool compile_shader_source(
    const char* source,
    std::size_t source_size,
    const char* source_name,
    const char* entry,
    const char* target,
    ComPtr<ID3DBlob>* blob
) {
    ComPtr<ID3DBlob> errors;
    const HRESULT result = D3DCompile(
        source,
        source_size,
        source_name,
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

bool compile_shader(
    const char* entry,
    const char* target,
    ComPtr<ID3DBlob>* blob
) {
    return compile_shader_source(
        shader_source,
        sizeof(shader_source) - 1,
        "OpenGT world shader",
        entry,
        target,
        blob);
}

bool compile_screen_grid_shader(
    const char* entry,
    const char* target,
    ComPtr<ID3DBlob>* blob
) {
    return compile_shader_source(
        screen_grid_shader_source,
        sizeof(screen_grid_shader_source) - 1,
        "OpenGT screen-grid shader",
        entry,
        target,
        blob);
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
    // World vertices use reversed infinite depth (near/Z after divide).
    // Greater values are physically nearer; equality retains deterministic
    // authored ownership for exactly coplanar submissions.
    description.DepthFunc = D3D11_COMPARISON_GREATER_EQUAL;
    const bool writes_depth_owner = write_depth && depth_enabled;
    description.StencilEnable = check_mask || set_mask || writes_depth_owner;
    description.StencilReadMask = 1;
    // Bit zero retains the PS1 mask-bit contract. Bit one records whether the
    // nearest opaque world pixel belongs to a classified road support; bit two
    // records an opaque vehicle body that owns a stock reflection-detail pass.
    description.StencilWriteMask =
        (set_mask ? 1 : 0) | (writes_depth_owner ? 6 : 0);
    description.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
    description.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;
    description.FrontFace.StencilPassOp = set_mask || writes_depth_owner
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

ComPtr<ID3D11DepthStencilState> road_overlay_depth_state(
    ID3D11Device* device,
    bool check_mask
) {
    D3D11_DEPTH_STENCIL_DESC description{};
    // Typed artwork may replace only a visible road-support pixel at the same
    // or a nearer physical depth. The depth bound prevents a misclassified
    // distant polygon from punching through nearer scenery or vehicles.
    description.DepthEnable = TRUE;
    description.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ZERO;
    description.DepthFunc = D3D11_COMPARISON_GREATER_EQUAL;
    description.StencilEnable = TRUE;
    description.StencilReadMask = check_mask ? 3 : 2;
    description.StencilWriteMask = 0;
    description.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
    description.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;
    description.FrontFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;
    description.FrontFace.StencilFunc = D3D11_COMPARISON_EQUAL;
    description.BackFace = description.FrontFace;
    ComPtr<ID3D11DepthStencilState> result;
    if (FAILED(device->CreateDepthStencilState(
            &description,
            result.GetAddressOf())))
        result.Reset();
    return result;
}

ComPtr<ID3D11DepthStencilState> vehicle_reflection_depth_state(
    ID3D11Device* device,
    bool check_mask
) {
    D3D11_DEPTH_STENCIL_DESC description{};
    description.DepthEnable = TRUE;
    description.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ZERO;
    description.DepthFunc = D3D11_COMPARISON_GREATER_EQUAL;
    description.StencilEnable = TRUE;
    description.StencilReadMask = check_mask ? 5 : 4;
    description.StencilWriteMask = 0;
    description.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
    description.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;
    description.FrontFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;
    description.FrontFace.StencilFunc = D3D11_COMPARISON_EQUAL;
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

struct AuthoredScreenArc {
    float center_x{};
    float center_y{};
    float radius_x{};
    float radius_y{};
    std::uint32_t orientation{};
    std::size_t command_index{};
};

std::vector<AuthoredScreenArc> detect_authored_screen_arcs(
    const WorldDrawList& draw_list
) {
    struct FanGroup {
        float center_x{};
        float center_y{};
        std::size_t command_index{};
        std::vector<std::array<float, 2>> contour_points;
    };
    std::unordered_map<std::uint64_t, FanGroup> fan_groups;
    const auto center_key = [] (float x, float y) {
        const std::int32_t quantized_x = static_cast<std::int32_t>(
            std::lround(x * 16.0F));
        const std::int32_t quantized_y = static_cast<std::int32_t>(
            std::lround(y * 16.0F));
        return
            static_cast<std::uint64_t>(
                static_cast<std::uint32_t>(quantized_x)) << 32 |
            static_cast<std::uint32_t>(quantized_y);
    };
    for (std::size_t command_index = 0;
         command_index < draw_list.commands.size();
         ++command_index) {
        const auto& command = draw_list.commands[command_index];
        if (command.material_index >= draw_list.materials.size())
            continue;
        const auto& material = draw_list.materials[command.material_index];
        if (
            command.channel != WorldViewChannel::main_view ||
            (material.primitive_flags &
                world_primitive_screen_space_flag) == 0 ||
            (material.primitive_flags & textured_flag) != 0
        )
            continue;
        for (int center_index = 0; center_index < 3; ++center_index) {
            const int first_index = (center_index + 1) % 3;
            const int second_index = (center_index + 2) % 3;
            const auto& center = command.vertices[center_index];
            const auto& first = command.vertices[first_index];
            const auto& second = command.vertices[second_index];
            const float first_x = first.screen_x - center.screen_x;
            const float first_y = first.screen_y - center.screen_y;
            const float second_x = second.screen_x - center.screen_x;
            const float second_y = second.screen_y - center.screen_y;
            const float first_radius = std::hypot(first_x, first_y);
            const float second_radius = std::hypot(second_x, second_y);
            const float chord = std::hypot(
                first.screen_x - second.screen_x,
                first.screen_y - second.screen_y);
            if (
                !std::isfinite(first_radius) ||
                !std::isfinite(second_radius) ||
                first_radius < 3.0F ||
                second_radius < 3.0F ||
                first_radius > 64.0F ||
                second_radius > 64.0F ||
                (std::max)(first_radius, second_radius) >
                    (std::min)(first_radius, second_radius) * 1.20F ||
                chord >= (std::min)(first_radius, second_radius) * 0.85F
            )
                continue;
            const float radial_cosine =
                (first_x * second_x + first_y * second_y) /
                (first_radius * second_radius);
            if (radial_cosine < 0.45F)
                continue;
            const std::uint64_t key = center_key(
                center.screen_x, center.screen_y);
            auto [iterator, inserted] = fan_groups.try_emplace(key);
            auto& group = iterator->second;
            if (inserted) {
                group.center_x = center.screen_x;
                group.center_y = center.screen_y;
                group.command_index = command_index;
            }
            group.contour_points.push_back(
                {first.screen_x, first.screen_y});
            group.contour_points.push_back(
                {second.screen_x, second.screen_y});
        }
    }

    std::vector<AuthoredScreenArc> arcs;
    arcs.reserve((std::min)(fan_groups.size(), maximum_screen_arcs));
    for (auto& [_, group] : fan_groups) {
        std::vector<std::array<float, 2>> unique_points;
        for (const auto& point : group.contour_points) {
            const bool duplicate = std::any_of(
                unique_points.begin(),
                unique_points.end(),
                [&point] (const std::array<float, 2>& existing) {
                    return
                        std::abs(point[0] - existing[0]) <= 0.125F &&
                        std::abs(point[1] - existing[1]) <= 0.125F;
                });
            if (!duplicate)
                unique_points.push_back(point);
        }
        if (unique_points.size() < 5)
            continue;
        float minimum_x = 0.0F;
        float maximum_x = 0.0F;
        float minimum_y = 0.0F;
        float maximum_y = 0.0F;
        for (const auto& point : unique_points) {
            const float x = point[0] - group.center_x;
            const float y = point[1] - group.center_y;
            minimum_x = (std::min)(minimum_x, x);
            maximum_x = (std::max)(maximum_x, x);
            minimum_y = (std::min)(minimum_y, y);
            maximum_y = (std::max)(maximum_y, y);
        }
        const float radius_x = (std::max)(-minimum_x, maximum_x);
        const float radius_y = (std::max)(-minimum_y, maximum_y);
        if (
            radius_x < 3.0F || radius_y < 3.0F ||
            radius_x > 64.0F || radius_y > 64.0F ||
            radius_x > radius_y * 4.0F ||
            radius_y > radius_x * 4.0F
        )
            continue;
        float radial_error = 0.0F;
        float worst_radial_error = 0.0F;
        for (const auto& point : unique_points) {
            const float normalized_x =
                (point[0] - group.center_x) / radius_x;
            const float normalized_y =
                (point[1] - group.center_y) / radius_y;
            const float error = std::abs(
                std::hypot(normalized_x, normalized_y) - 1.0F);
            radial_error += error;
            worst_radial_error = (std::max)(worst_radial_error, error);
        }
        radial_error /= static_cast<float>(unique_points.size());
        if (radial_error > 0.14F || worst_radial_error > 0.30F)
            continue;
        const bool spans_x =
            minimum_x <= -radius_x * 0.65F &&
            maximum_x >= radius_x * 0.65F;
        const bool spans_y =
            minimum_y <= -radius_y * 0.65F &&
            maximum_y >= radius_y * 0.65F;
        const float x_seam_tolerance = radius_x * 0.20F + 0.25F;
        const float y_seam_tolerance = radius_y * 0.20F + 0.25F;
        std::optional<std::uint32_t> orientation;
        if (spans_y && maximum_x <= x_seam_tolerance)
            orientation = 0U;
        else if (spans_y && minimum_x >= -x_seam_tolerance)
            orientation = 1U;
        else if (spans_x && maximum_y <= y_seam_tolerance)
            orientation = 2U;
        else if (spans_x && minimum_y >= -y_seam_tolerance)
            orientation = 3U;
        if (!orientation.has_value())
            continue;
        arcs.push_back(AuthoredScreenArc{
            group.center_x,
            group.center_y,
            radius_x,
            radius_y,
            *orientation,
            group.command_index,
        });
        if (arcs.size() == maximum_screen_arcs)
            break;
    }
    std::sort(
        arcs.begin(),
        arcs.end(),
        [] (const AuthoredScreenArc& left, const AuthoredScreenArc& right) {
            if (left.center_y != right.center_y)
                return left.center_y < right.center_y;
            if (left.center_x != right.center_x)
                return left.center_x < right.center_x;
            return left.orientation < right.orientation;
        });
    return arcs;
}

struct FrameInputResources {
    ComPtr<ID3D11Buffer> vertex_buffer;
    UINT vertex_buffer_bytes{};
    ComPtr<ID3D11Buffer> material_buffer;
    ComPtr<ID3D11ShaderResourceView> material_view;
    UINT material_buffer_bytes{};
    std::array<ComPtr<ID3D11Buffer>, 3> constant_buffers;
    ComPtr<ID3D11Buffer> screen_arc_buffer;
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
    ComPtr<ID3D11PixelShader> cutout_depth_pixel_shader;
    ComPtr<ID3D11PixelShader> cutout_fringe_pixel_shader;
    ComPtr<ID3D11PixelShader> road_overlay_pixel_shader;
    ComPtr<ID3D11PixelShader> road_overlay_tint_pixel_shader;
    ComPtr<ID3D11PixelShader> command_id_pixel_shader;
    ComPtr<ID3D11PixelShader> screen_grid_mask_pixel_shader;
    ComPtr<ID3D11VertexShader> screen_grid_vertex_shader;
    ComPtr<ID3D11PixelShader> screen_grid_pixel_shader;
    ComPtr<ID3D11InputLayout> input_layout;
    ComPtr<ID3D11RasterizerState> rasterizer;
    std::array<ComPtr<ID3D11BlendState>, 7> blend_states;
    ComPtr<ID3D11DepthStencilState> depth_states[2][2][2][2];
    ComPtr<ID3D11DepthStencilState> road_overlay_depth_states[2];
    ComPtr<ID3D11DepthStencilState> vehicle_reflection_depth_states[2];
    // Mutable inputs can follow the full sixteen-slot asynchronous readback
    // cadence. The low-latency authored path keeps four slots hot; a deeper
    // queue expands to the staging slot's unique resource set. In either case,
    // a slot cannot be uploaded again until its image has been drained.
    std::array<
        FrameInputResources,
        world_gpu_async_readback_image_capacity> frame_inputs;
    ComPtr<ID3D11Texture2D> color_texture;
    ComPtr<ID3D11RenderTargetView> color_view;
    ComPtr<ID3D11ShaderResourceView> color_shader_view;
    ComPtr<ID3D11Texture2D> screen_grid_texture;
    ComPtr<ID3D11RenderTargetView> screen_grid_view;
    ComPtr<ID3D11Texture2D> screen_grid_world_texture;
    ComPtr<ID3D11ShaderResourceView> screen_grid_world_shader_view;
    ComPtr<ID3D11Texture2D> screen_grid_mask_texture;
    ComPtr<ID3D11RenderTargetView> screen_grid_mask_view;
    ComPtr<ID3D11ShaderResourceView> screen_grid_mask_shader_view;
    ComPtr<ID3D11Texture2D> depth_texture;
    ComPtr<ID3D11DepthStencilView> depth_view;
    // Four slots retain four chronological development images, allowing a GPU
    // tail to complete before a blocking Map. The shipping authored path uses
    // the separate sixteen-image asynchronous ring below; the even four-slot
    // layout remains for development-only pair-oracle compatibility.
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
    std::uint32_t screen_grid_width;
    std::uint32_t screen_grid_height;
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

// The pixel shader makes its alpha decision after perspective interpolation,
// including the bounded anisotropic minification footprint. Geometry and
// command-count audits cannot see a road pixel discarded by that stage, so
// reproduce the exact centroid derivatives and texture-word taps on the CPU
// when requested. This is deliberately read-only and bounded to an explicit
// poll window.
void emit_texture_coverage_diagnostics(
    const WorldDrawList& draw_list,
    const std::uint16_t* vram,
    std::size_t vram_word_count,
    const WorldGpuRenderOptions& options,
    const std::vector<std::uint8_t>& opaque_track_surfaces
) {
    if (std::getenv("OPENGT_RENDER_TEXTURE_COVERAGE_DIAGNOSTICS") == nullptr ||
        vram == nullptr || vram_word_count < 1024U * 512U ||
        options.synthetic_midpoint)
        return;
    std::int32_t start_poll = 0;
    std::int32_t end_poll = (std::numeric_limits<std::int32_t>::max)();
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_TEXTURE_COVERAGE_DIAGNOSTICS_START_POLL")) {
        start_poll = static_cast<std::int32_t>(std::strtol(
            configured, nullptr, 10));
    }
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_TEXTURE_COVERAGE_DIAGNOSTICS_END_POLL")) {
        end_poll = static_cast<std::int32_t>(std::strtol(
            configured, nullptr, 10));
    }
    if (draw_list.input_poll < start_poll || draw_list.input_poll > end_poll)
        return;
    static thread_local std::uint64_t sample = 0;
    std::uint64_t interval = 1;
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_TEXTURE_COVERAGE_DIAGNOSTICS_INTERVAL")) {
        const unsigned long parsed = std::strtoul(configured, nullptr, 10);
        if (parsed != 0)
            interval = parsed;
    }
    if ((sample++ % interval) != 0)
        return;

    const std::uint32_t output_scale = (std::max)(options.output_scale, 1U);
    const std::uint32_t target_width =
        world_gpu_target_display_width(draw_list, options);
    const double output_width =
        static_cast<double>(target_width) * output_scale;
    const double output_height =
        static_cast<double>(draw_list.display_height) * output_scale;
    const double horizontal_scale =
        static_cast<double>(draw_list.display_width) / target_width;
    std::size_t opaque_target = 0;
    std::size_t minified = 0;
    std::size_t center_zero = 0;
    std::size_t partial_zero_footprints = 0;
    std::size_t all_zero_footprints = 0;
    std::size_t sparse_footprint_misses = 0;
    std::size_t densely_transparent_footprints = 0;
    std::size_t maximum_dense_grid = 0;
    std::size_t maximum_filter_samples = 0;
    double maximum_footprint = 0.0;
    std::size_t verbose_count = 0;
    const bool verbose = std::getenv(
        "OPENGT_RENDER_TEXTURE_COVERAGE_DIAGNOSTICS_VERBOSE") != nullptr;

    for (std::size_t command_index = 0;
         command_index < draw_list.commands.size();
         ++command_index) {
        const auto& command = draw_list.commands[command_index];
        if (command.material_index >= draw_list.materials.size())
            continue;
        const auto& material = draw_list.materials[command.material_index];
        if (command_index >= opaque_track_surfaces.size() ||
            opaque_track_surfaces[command_index] == 0)
            continue;
        bool in_front = true;
        std::array<double, 3> screen_x{};
        std::array<double, 3> screen_y{};
        std::array<double, 3> reciprocal_w{};
        double minimum_x = (std::numeric_limits<double>::max)();
        double maximum_x = (std::numeric_limits<double>::lowest)();
        double minimum_y = (std::numeric_limits<double>::max)();
        double maximum_y = (std::numeric_limits<double>::lowest)();
        for (std::size_t vertex = 0; vertex < 3; ++vertex) {
            const auto& source = command.vertices[vertex];
            if (!std::isfinite(source.clip_w) || source.clip_w < 16.0F) {
                in_front = false;
                break;
            }
            reciprocal_w[vertex] = 1.0 / source.clip_w;
            screen_x[vertex] =
                (source.clip_x * horizontal_scale * reciprocal_w[vertex] *
                    0.5 + 0.5) * output_width;
            screen_y[vertex] =
                (-source.clip_y * reciprocal_w[vertex] * 0.5 + 0.5) *
                    output_height;
            minimum_x = (std::min)(minimum_x, screen_x[vertex]);
            maximum_x = (std::max)(maximum_x, screen_x[vertex]);
            minimum_y = (std::min)(minimum_y, screen_y[vertex]);
            maximum_y = (std::max)(maximum_y, screen_y[vertex]);
        }
        if (!in_front || maximum_x < 0.0 || minimum_x >= output_width ||
            maximum_y < 0.0 || minimum_y >= output_height)
            continue;
        ++opaque_target;

        const double denominator =
            (screen_y[1] - screen_y[2]) *
                (screen_x[0] - screen_x[2]) +
            (screen_x[2] - screen_x[1]) *
                (screen_y[0] - screen_y[2]);
        if (!std::isfinite(denominator) || std::abs(denominator) < 1.0e-12)
            continue;
        const std::array<double, 3> lambda_dx{{
            (screen_y[1] - screen_y[2]) / denominator,
            (screen_y[2] - screen_y[0]) / denominator,
            (screen_y[0] - screen_y[1]) / denominator,
        }};
        const std::array<double, 3> lambda_dy{{
            (screen_x[2] - screen_x[1]) / denominator,
            (screen_x[0] - screen_x[2]) / denominator,
            (screen_x[1] - screen_x[0]) / denominator,
        }};
        double q = 0.0;
        double numerator_u = 0.0;
        double numerator_v = 0.0;
        double q_dx = 0.0;
        double q_dy = 0.0;
        double numerator_u_dx = 0.0;
        double numerator_u_dy = 0.0;
        double numerator_v_dx = 0.0;
        double numerator_v_dy = 0.0;
        for (std::size_t vertex = 0; vertex < 3; ++vertex) {
            const double weighted_u =
                command.vertices[vertex].u * reciprocal_w[vertex];
            const double weighted_v =
                command.vertices[vertex].v * reciprocal_w[vertex];
            q += reciprocal_w[vertex] / 3.0;
            numerator_u += weighted_u / 3.0;
            numerator_v += weighted_v / 3.0;
            q_dx += lambda_dx[vertex] * reciprocal_w[vertex];
            q_dy += lambda_dy[vertex] * reciprocal_w[vertex];
            numerator_u_dx += lambda_dx[vertex] * weighted_u;
            numerator_u_dy += lambda_dy[vertex] * weighted_u;
            numerator_v_dx += lambda_dx[vertex] * weighted_v;
            numerator_v_dy += lambda_dy[vertex] * weighted_v;
        }
        if (!std::isfinite(q) || std::abs(q) < 1.0e-18)
            continue;
        const double q_squared = q * q;
        const double u = numerator_u / q;
        const double v = numerator_v / q;
        const double du_dx =
            (numerator_u_dx * q - numerator_u * q_dx) / q_squared;
        const double du_dy =
            (numerator_u_dy * q - numerator_u * q_dy) / q_squared;
        const double dv_dx =
            (numerator_v_dx * q - numerator_v * q_dx) / q_squared;
        const double dv_dy =
            (numerator_v_dy * q - numerator_v * q_dy) / q_squared;
        const double footprint_u = (std::max)(std::abs(du_dx), std::abs(du_dy));
        const double footprint_v = (std::max)(std::abs(dv_dx), std::abs(dv_dy));
        const double footprint = (std::max)(footprint_u, footprint_v);
        maximum_footprint = (std::max)(maximum_footprint, footprint);
        if (!(footprint > 1.0))
            continue;
        ++minified;
        const bool zero_center = material_texture_word(
            vram,
            material,
            static_cast<std::int32_t>(std::floor(u + 0.0001)),
            static_cast<std::int32_t>(std::floor(v + 0.0001))) == 0;
        center_zero += zero_center ? 1U : 0U;
        const double dx_length_squared = du_dx * du_dx + dv_dx * dv_dx;
        const double dy_length_squared = du_dy * du_dy + dv_dy * dv_dy;
        const bool dx_major = dx_length_squared >= dy_length_squared;
        const double major_u = dx_major ? du_dx : du_dy;
        const double major_v = dx_major ? dv_dx : dv_dy;
        const double minor_u = dx_major ? du_dy : du_dx;
        const double minor_v = dx_major ? dv_dy : dv_dx;
        const std::size_t major_taps = static_cast<std::size_t>(std::clamp(
            std::ceil(std::sqrt((std::max)(
                dx_length_squared, dy_length_squared))),
            3.0,
            16.0));
        const std::size_t minor_taps = static_cast<std::size_t>(std::clamp(
            std::ceil(std::sqrt((std::min)(
                dx_length_squared, dy_length_squared))),
            3.0,
            4.0));
        const std::size_t filter_samples = major_taps * minor_taps;
        maximum_filter_samples = (std::max)(
            maximum_filter_samples, filter_samples);
        std::size_t zero_taps = 0;
        for (std::size_t major_index = 0;
             major_index < major_taps;
             ++major_index) {
            const double major_amount =
                (major_index + 0.5) / major_taps - 0.5;
            for (std::size_t minor_index = 0;
                 minor_index < minor_taps;
                 ++minor_index) {
                const double minor_amount =
                    (minor_index + 0.5) / minor_taps - 0.5;
                const double sample_u =
                    u + major_amount * major_u + minor_amount * minor_u;
                const double sample_v =
                    v + major_amount * major_v + minor_amount * minor_v;
                zero_taps += material_texture_word(
                    vram,
                    material,
                    static_cast<std::int32_t>(std::floor(sample_u + 0.0001)),
                    static_cast<std::int32_t>(std::floor(sample_v + 0.0001))) == 0
                    ? 1U
                    : 0U;
            }
        }
        partial_zero_footprints += zero_taps != 0 ? 1U : 0U;
        all_zero_footprints += zero_taps == filter_samples ? 1U : 0U;
        std::size_t dense_nonzero = 0;
        std::size_t dense_grid = 0;
        if (zero_taps == filter_samples) {
            // Audit a denser reference grid only for a bounded-filter miss:
            // one sample per source texel along the longest derivative
            // (capped for a pathological sliver) distinguishes truly
            // transparent artwork from remaining undersampling.
            dense_grid = static_cast<std::size_t>(std::clamp(
                std::ceil(std::sqrt((std::max)(
                    dx_length_squared, dy_length_squared))),
                3.0,
                65.0));
            maximum_dense_grid = (std::max)(maximum_dense_grid, dense_grid);
            for (std::size_t sample_y = 0;
                 sample_y < dense_grid;
                 ++sample_y) {
                const double along_y =
                    (sample_y + 0.5) / dense_grid - 0.5;
                for (std::size_t sample_x = 0;
                     sample_x < dense_grid;
                     ++sample_x) {
                    const double along_x =
                        (sample_x + 0.5) / dense_grid - 0.5;
                    const double sample_u =
                        u + along_x * du_dx + along_y * du_dy;
                    const double sample_v =
                        v + along_x * dv_dx + along_y * dv_dy;
                    dense_nonzero += material_texture_word(
                        vram,
                        material,
                        static_cast<std::int32_t>(
                            std::floor(sample_u + 0.0001)),
                        static_cast<std::int32_t>(
                            std::floor(sample_v + 0.0001))) != 0
                        ? 1U
                        : 0U;
                }
            }
            sparse_footprint_misses += dense_nonzero != 0 ? 1U : 0U;
            densely_transparent_footprints += dense_nonzero == 0 ? 1U : 0U;
        }
        if (verbose && zero_taps != 0 && verbose_count++ < 128) {
            std::fprintf(
                stderr,
                "[Render-Texture-Coverage-Command] frame=%llu poll=%d "
                "command=%zu source=%u object=%u model=%08x ot=%d "
                "screen=%.1f,%.1f..%.1f,%.1f uv=%.3f,%.3f "
                "footprint=%.3f,%.3f zero=%zu/%zu centerZero=%u "
                "filter=%zux%zu "
                "dense=%zu/%zu "
                "tpage=%04x clut=%04x z=%.1f/%.1f/%.1f\n",
                static_cast<unsigned long long>(draw_list.frame_index),
                draw_list.input_poll,
                command_index,
                command.source_command_index,
                command.object_id,
                command.model_pointer,
                command.ordering_table_index,
                minimum_x,
                minimum_y,
                maximum_x,
                maximum_y,
                u,
                v,
                footprint_u,
                footprint_v,
                zero_taps,
                filter_samples,
                zero_center ? 1U : 0U,
                major_taps,
                minor_taps,
                dense_nonzero,
                dense_grid * dense_grid,
                material.texture_page,
                material.clut,
                command.vertices[0].view_z,
                command.vertices[1].view_z,
                command.vertices[2].view_z);
        }
    }
    std::fprintf(
        stderr,
        "[Render-Texture-Coverage] frame=%llu poll=%d "
        "opaqueTarget=%zu minified=%zu centerZero=%zu "
        "partialZero=%zu allZero=%zu sparseMiss=%zu denseTransparent=%zu "
        "maxFilterSamples=%zu maxDenseGrid=%zu maxFootprint=%.3f\n",
        static_cast<unsigned long long>(draw_list.frame_index),
        draw_list.input_poll,
        opaque_target,
        minified,
        center_zero,
        partial_zero_footprints,
        all_zero_footprints,
        sparse_footprint_misses,
        densely_transparent_footprints,
        maximum_filter_samples,
        maximum_dense_grid,
        maximum_footprint);
}

// A distant billboard that appears rotated can be caused by at least four
// independent inputs: different UVs/materials, mutable VRAM contents, a
// projection-orientation change, or ordinary foreground occlusion. Preserve
// the first three as a compact per-command time series; pixel provenance can
// then be enabled only for the exact screen point where ownership is in doubt.
// This diagnostic is deliberately data-only and bounded by explicit filters.
void emit_texture_instance_diagnostics(
    const WorldDrawList& draw_list,
    const std::uint16_t* vram,
    std::size_t vram_word_count,
    const WorldGpuRenderOptions& options
) {
    if (std::getenv("OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS") == nullptr ||
        vram == nullptr || vram_word_count < 1024U * 512U ||
        options.synthetic_midpoint)
        return;

    std::int32_t start_poll = 0;
    std::int32_t end_poll = (std::numeric_limits<std::int32_t>::max)();
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_START_POLL")) {
        start_poll = static_cast<std::int32_t>(std::strtol(
            configured, nullptr, 10));
    }
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_END_POLL")) {
        end_poll = static_cast<std::int32_t>(std::strtol(
            configured, nullptr, 10));
    }
    if (draw_list.input_poll < start_poll || draw_list.input_poll > end_poll)
        return;

    static thread_local std::uint64_t sample = 0;
    std::uint64_t interval = 1;
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_INTERVAL")) {
        const unsigned long parsed = std::strtoul(configured, nullptr, 10);
        if (parsed != 0)
            interval = parsed;
    }
    if ((sample++ % interval) != 0)
        return;

    const auto parse_optional_u32 = [] (
        const char* name,
        std::uint32_t* value
    ) {
        const char* configured = std::getenv(name);
        if (configured == nullptr || configured[0] == '\0')
            return false;
        char* end = nullptr;
        const unsigned long parsed = std::strtoul(configured, &end, 0);
        if (end == configured || *end != '\0')
            return false;
        *value = static_cast<std::uint32_t>(parsed);
        return true;
    };
    std::uint32_t requested_object = 0;
    std::uint32_t requested_model = 0;
    std::uint32_t requested_tpage = 0;
    std::uint32_t requested_clut = 0;
    const bool filter_object = parse_optional_u32(
        "OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_OBJECT",
        &requested_object);
    const bool filter_model = parse_optional_u32(
        "OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_MODEL",
        &requested_model);
    const bool filter_tpage = parse_optional_u32(
        "OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_TPAGE",
        &requested_tpage);
    const bool filter_clut = parse_optional_u32(
        "OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_CLUT",
        &requested_clut);
    const bool overlay_only = std::getenv(
        "OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_OVERLAY_ONLY") != nullptr;

    const std::uint32_t output_scale = (std::max)(options.output_scale, 1U);
    const std::uint32_t target_width =
        world_gpu_target_display_width(draw_list, options);
    const double output_width =
        static_cast<double>(target_width) * output_scale;
    const double output_height =
        static_cast<double>(draw_list.display_height) * output_scale;
    const double horizontal_scale =
        static_cast<double>(draw_list.display_width) / target_width;
    std::size_t matched = 0;

    for (std::size_t command_index = 0;
         command_index < draw_list.commands.size();
         ++command_index) {
        const auto& command = draw_list.commands[command_index];
        if (command.material_index >= draw_list.materials.size())
            continue;
        const auto& material = draw_list.materials[command.material_index];
        if ((material.primitive_flags & textured_flag) == 0 ||
            (material.primitive_flags & world_primitive_screen_space_flag) != 0 ||
            (overlay_only && track_overlay_layer(
                material.primitive_flags) == 0) ||
            (filter_object && command.object_id != requested_object) ||
            (filter_model && command.model_pointer != requested_model) ||
            (filter_tpage && material.texture_page != requested_tpage) ||
            (filter_clut && material.clut != requested_clut))
            continue;

        const DiagnosticClipResult clipped =
            diagnostic_homogeneous_clip(command, horizontal_scale);
        if (!clipped.finite || clipped.vertex_count < 3)
            continue;
        double minimum_x = (std::numeric_limits<double>::max)();
        double minimum_y = (std::numeric_limits<double>::max)();
        double maximum_x = (std::numeric_limits<double>::lowest)();
        double maximum_y = (std::numeric_limits<double>::lowest)();
        for (std::size_t vertex = 0; vertex < clipped.vertex_count; ++vertex) {
            const auto& source = clipped.vertices[vertex];
            const double reciprocal_w = 1.0 / source.w;
            const double x =
                (source.x * reciprocal_w * 0.5 + 0.5) * output_width;
            const double y =
                (-source.y * reciprocal_w * 0.5 + 0.5) * output_height;
            minimum_x = (std::min)(minimum_x, x);
            minimum_y = (std::min)(minimum_y, y);
            maximum_x = (std::max)(maximum_x, x);
            maximum_y = (std::max)(maximum_y, y);
        }
        const bool on_screen = maximum_x >= 0.0 && minimum_x < output_width &&
            maximum_y >= 0.0 && minimum_y < output_height;

        std::array<double, 3> screen_x{};
        std::array<double, 3> screen_y{};
        bool original_projectable = true;
        double minimum_view_z = (std::numeric_limits<double>::max)();
        double maximum_view_z = (std::numeric_limits<double>::lowest)();
        for (std::size_t vertex = 0; vertex < 3; ++vertex) {
            const auto& source = command.vertices[vertex];
            minimum_view_z = (std::min)(
                minimum_view_z, static_cast<double>(source.view_z));
            maximum_view_z = (std::max)(
                maximum_view_z, static_cast<double>(source.view_z));
            if (!std::isfinite(source.clip_w) || source.clip_w <= 0.0F) {
                original_projectable = false;
                continue;
            }
            const double reciprocal_w = 1.0 / source.clip_w;
            screen_x[vertex] =
                (source.clip_x * horizontal_scale * reciprocal_w * 0.5 + 0.5) *
                output_width;
            screen_y[vertex] =
                (-source.clip_y * reciprocal_w * 0.5 + 0.5) * output_height;
        }
        const double screen_determinant = original_projectable
            ? (screen_x[1] - screen_x[0]) * (screen_y[2] - screen_y[0]) -
                (screen_y[1] - screen_y[0]) * (screen_x[2] - screen_x[0])
            : 0.0;
        const double uv_determinant =
            (command.vertices[1].u - command.vertices[0].u) *
                (command.vertices[2].v - command.vertices[0].v) -
            (command.vertices[1].v - command.vertices[0].v) *
                (command.vertices[2].u - command.vertices[0].u);
        const int orientation = std::abs(screen_determinant) <= 1.0e-12 ||
                std::abs(uv_determinant) <= 1.0e-12
            ? 0
            : (screen_determinant * uv_determinant < 0.0 ? -1 : 1);

        const std::int32_t minimum_u = static_cast<std::int32_t>(std::floor(
            (std::min)({command.vertices[0].u, command.vertices[1].u,
                command.vertices[2].u})));
        const std::int32_t maximum_u = static_cast<std::int32_t>(std::ceil(
            (std::max)({command.vertices[0].u, command.vertices[1].u,
                command.vertices[2].u})));
        const std::int32_t minimum_v = static_cast<std::int32_t>(std::floor(
            (std::min)({command.vertices[0].v, command.vertices[1].v,
                command.vertices[2].v})));
        const std::int32_t maximum_v = static_cast<std::int32_t>(std::ceil(
            (std::max)({command.vertices[0].v, command.vertices[1].v,
                command.vertices[2].v})));
        std::uint64_t texture_hash = 1469598103934665603ULL;
        const std::int32_t span_u = (std::min)(maximum_u - minimum_u + 1, 256);
        const std::int32_t span_v = (std::min)(maximum_v - minimum_v + 1, 256);
        for (std::int32_t y = 0; y < span_v; ++y) {
            for (std::int32_t x = 0; x < span_u; ++x) {
                const std::uint16_t word = material_texture_word(
                    vram, material, minimum_u + x, minimum_v + y);
                texture_hash ^= word;
                texture_hash *= 1099511628211ULL;
            }
        }
        const std::array<std::uint16_t, 3> corner_words{{
            material_texture_word(vram, material,
                static_cast<std::int32_t>(std::floor(command.vertices[0].u)),
                static_cast<std::int32_t>(std::floor(command.vertices[0].v))),
            material_texture_word(vram, material,
                static_cast<std::int32_t>(std::floor(command.vertices[1].u)),
                static_cast<std::int32_t>(std::floor(command.vertices[1].v))),
            material_texture_word(vram, material,
                static_cast<std::int32_t>(std::floor(command.vertices[2].u)),
                static_cast<std::int32_t>(std::floor(command.vertices[2].v))),
        }};

        std::fprintf(
            stderr,
            "[Render-Texture-Instance] frame=%llu poll=%d command=%zu "
            "source=%u kind=%u object=%u model=%08x transform=%016llx "
            "channel=%u tpage=%04x clut=%04x flags=%08x "
            "screen=%.2f,%.2f..%.2f,%.2f onScreen=%u clipped=%u/%zu "
            "viewZ=%.3f..%.3f uv=%u,%u/%u,%u/%u,%u "
            "rgb=%u,%u,%u/%u,%u,%u/%u,%u,%u "
            "screenDet=%.6f uvDet=%.6f orientation=%d "
            "textureRect=%d,%d..%d,%d textureHash=%016llx "
            "cornerWords=%04x/%04x/%04x window=%d,%d/%d,%d\n",
            static_cast<unsigned long long>(draw_list.frame_index),
            draw_list.input_poll,
            command_index,
            command.source_command_index,
            command.object_kind,
            command.object_id,
            command.model_pointer,
            static_cast<unsigned long long>(command.transform_id),
            static_cast<unsigned>(command.channel),
            material.texture_page,
            material.clut,
            material.primitive_flags,
            minimum_x,
            minimum_y,
            maximum_x,
            maximum_y,
            on_screen ? 1U : 0U,
            clipped.vertex_count != 3 ? 1U : 0U,
            clipped.vertex_count,
            minimum_view_z,
            maximum_view_z,
            static_cast<unsigned>(command.vertices[0].u),
            static_cast<unsigned>(command.vertices[0].v),
            static_cast<unsigned>(command.vertices[1].u),
            static_cast<unsigned>(command.vertices[1].v),
            static_cast<unsigned>(command.vertices[2].u),
            static_cast<unsigned>(command.vertices[2].v),
            static_cast<unsigned>(command.vertices[0].r),
            static_cast<unsigned>(command.vertices[0].g),
            static_cast<unsigned>(command.vertices[0].b),
            static_cast<unsigned>(command.vertices[1].r),
            static_cast<unsigned>(command.vertices[1].g),
            static_cast<unsigned>(command.vertices[1].b),
            static_cast<unsigned>(command.vertices[2].r),
            static_cast<unsigned>(command.vertices[2].g),
            static_cast<unsigned>(command.vertices[2].b),
            screen_determinant,
            uv_determinant,
            orientation,
            minimum_u,
            minimum_v,
            maximum_u,
            maximum_v,
            static_cast<unsigned long long>(texture_hash),
            corner_words[0],
            corner_words[1],
            corner_words[2],
            material.texture_mask_x,
            material.texture_mask_y,
            material.texture_offset_x,
            material.texture_offset_y);
        ++matched;
    }
    std::fprintf(
        stderr,
        "[Render-Texture-Instance-Summary] frame=%llu poll=%d matched=%zu\n",
        static_cast<unsigned long long>(draw_list.frame_index),
        draw_list.input_poll,
        matched);
}

struct PixelProvenanceCandidate {
    std::size_t command_index{};
    double depth{};
    double view_z{};
    double u{};
    double v{};
    double footprint_u{};
    double footprint_v{};
    double minimum_x{};
    double minimum_y{};
    double maximum_x{};
    double maximum_y{};
    std::uint16_t word{};
    std::size_t zero_taps{};
    std::size_t filter_taps{1};
    double color_r{};
    double color_g{};
    double color_b{};
    double sample_r{};
    double sample_g{};
    double sample_b{};
    double output_r{};
    double output_g{};
    double output_b{};
    bool discarded{};
    bool homogeneous_clipped{};
    std::size_t clipped_vertex_count{};
};

void emit_pixel_provenance_diagnostics(
    const WorldDrawList& draw_list,
    const std::uint16_t* vram,
    std::size_t vram_word_count,
    const WorldGpuRenderOptions& options,
    const std::vector<std::uint8_t>& opaque_track_surfaces
) {
    const char* configured_points = std::getenv(
        "OPENGT_RENDER_PIXEL_PROVENANCE_POINTS");
    if (configured_points == nullptr || configured_points[0] == '\0' ||
        vram == nullptr || vram_word_count < 1024U * 512U ||
        options.synthetic_midpoint)
        return;
    std::int32_t start_poll = 0;
    std::int32_t end_poll = (std::numeric_limits<std::int32_t>::max)();
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_PIXEL_PROVENANCE_START_POLL")) {
        start_poll = static_cast<std::int32_t>(std::strtol(
            configured, nullptr, 10));
    }
    if (const char* configured = std::getenv(
            "OPENGT_RENDER_PIXEL_PROVENANCE_END_POLL")) {
        end_poll = static_cast<std::int32_t>(std::strtol(
            configured, nullptr, 10));
    }
    if (draw_list.input_poll < start_poll || draw_list.input_poll > end_poll)
        return;

    std::vector<std::array<double, 2>> points;
    const char* cursor = configured_points;
    while (*cursor != '\0' && points.size() < 32) {
        char* after_x = nullptr;
        const double x = std::strtod(cursor, &after_x);
        if (after_x == cursor || *after_x != ',')
            break;
        char* after_y = nullptr;
        const double y = std::strtod(after_x + 1, &after_y);
        if (after_y == after_x + 1 || !std::isfinite(x) || !std::isfinite(y))
            break;
        points.push_back({x, y});
        cursor = after_y;
        if (*cursor == ';')
            ++cursor;
        else if (*cursor != '\0')
            break;
    }
    if (points.empty())
        return;

    const std::uint32_t output_scale = (std::max)(options.output_scale, 1U);
    const std::uint32_t target_width =
        world_gpu_target_display_width(draw_list, options);
    const double output_width =
        static_cast<double>(target_width) * output_scale;
    const double output_height =
        static_cast<double>(draw_list.display_height) * output_scale;
    const double horizontal_scale =
        static_cast<double>(draw_list.display_width) / target_width;
    const double horizontal_margin =
        (output_width -
            static_cast<double>(draw_list.display_width) * output_scale) * 0.5;

    for (std::size_t point_index = 0; point_index < points.size(); ++point_index) {
        const double point_x = points[point_index][0];
        const double point_y = points[point_index][1];
        std::vector<PixelProvenanceCandidate> candidates;
        for (std::size_t command_index = 0;
             command_index < draw_list.commands.size();
             ++command_index) {
            const auto& command = draw_list.commands[command_index];
            if (command.material_index >= draw_list.materials.size())
                continue;
            const auto& material = draw_list.materials[command.material_index];
            const DiagnosticClipResult post_clip =
                diagnostic_homogeneous_clip(command, horizontal_scale);
            if (!post_clip.finite || post_clip.vertex_count < 3)
                continue;
            std::array<double, 16> polygon_screen_x{};
            std::array<double, 16> polygon_screen_y{};
            double minimum_x = (std::numeric_limits<double>::max)();
            double minimum_y = (std::numeric_limits<double>::max)();
            double maximum_x = (std::numeric_limits<double>::lowest)();
            double maximum_y = (std::numeric_limits<double>::lowest)();
            for (std::size_t vertex = 0;
                 vertex < post_clip.vertex_count;
                 ++vertex) {
                const auto& source = post_clip.vertices[vertex];
                const double reciprocal_w = 1.0 / source.w;
                polygon_screen_x[vertex] =
                    (source.x * reciprocal_w * 0.5 + 0.5) * output_width;
                polygon_screen_y[vertex] =
                    (-source.y * reciprocal_w * 0.5 + 0.5) * output_height;
                minimum_x = (std::min)(
                    minimum_x, polygon_screen_x[vertex]);
                minimum_y = (std::min)(
                    minimum_y, polygon_screen_y[vertex]);
                maximum_x = (std::max)(
                    maximum_x, polygon_screen_x[vertex]);
                maximum_y = (std::max)(
                    maximum_y, polygon_screen_y[vertex]);
            }
            if (point_x < minimum_x || point_x > maximum_x ||
                point_y < minimum_y || point_y > maximum_y)
                continue;

            // Match the command's actual D3D scissor. In Hor+ the full main
            // world may use the widened target, while a partial authored draw
            // area remains centred in the original 4:3 viewport.
            const bool full_main_world_scissor =
                command.channel == WorldViewChannel::main_view &&
                command.clip_x0 <= draw_list.display_x &&
                command.clip_x1 >=
                    draw_list.display_x + draw_list.display_width - 1;
            const double scissor_left = full_main_world_scissor
                ? 0.0
                : std::clamp(
                    static_cast<double>(
                        (command.clip_x0 - draw_list.display_x) *
                            static_cast<std::int32_t>(output_scale)) +
                        horizontal_margin,
                    0.0,
                    output_width);
            const double scissor_top = std::clamp(
                static_cast<double>(
                    (command.clip_y0 - draw_list.display_y) *
                        static_cast<std::int32_t>(output_scale)),
                0.0,
                output_height);
            const double scissor_right = full_main_world_scissor
                ? output_width
                : std::clamp(
                    static_cast<double>(
                        (command.clip_x1 - draw_list.display_x + 1) *
                            static_cast<std::int32_t>(output_scale)) +
                        horizontal_margin,
                    0.0,
                    output_width);
            const double scissor_bottom = std::clamp(
                static_cast<double>(
                    (command.clip_y1 - draw_list.display_y + 1) *
                        static_cast<std::int32_t>(output_scale)),
                0.0,
                output_height);
            if (point_x < scissor_left || point_x >= scissor_right ||
                point_y < scissor_top || point_y >= scissor_bottom)
                continue;

            // A clipped triangle is a convex polygon. D3D rasterizes the same
            // fan, so find the fan triangle containing this output pixel and
            // use its post-clip vertices for depth and attribute interpolation.
            std::array<DiagnosticClipVertex, 3> raster_vertices{};
            std::array<double, 3> screen_x{};
            std::array<double, 3> screen_y{};
            std::array<double, 3> weights{};
            double denominator = 0.0;
            bool contains_point = false;
            for (std::size_t fan = 1;
                 fan + 1 < post_clip.vertex_count;
                 ++fan) {
                const std::array<std::size_t, 3> indices{{0, fan, fan + 1}};
                for (std::size_t vertex = 0; vertex < 3; ++vertex) {
                    raster_vertices[vertex] = post_clip.vertices[indices[vertex]];
                    screen_x[vertex] = polygon_screen_x[indices[vertex]];
                    screen_y[vertex] = polygon_screen_y[indices[vertex]];
                }
                denominator =
                    (screen_y[1] - screen_y[2]) *
                        (screen_x[0] - screen_x[2]) +
                    (screen_x[2] - screen_x[1]) *
                        (screen_y[0] - screen_y[2]);
                if (!std::isfinite(denominator) ||
                    std::abs(denominator) < 1.0e-12)
                    continue;
                weights[0] =
                    ((screen_y[1] - screen_y[2]) *
                            (point_x - screen_x[2]) +
                        (screen_x[2] - screen_x[1]) *
                            (point_y - screen_y[2])) / denominator;
                weights[1] =
                    ((screen_y[2] - screen_y[0]) *
                            (point_x - screen_x[2]) +
                        (screen_x[0] - screen_x[2]) *
                            (point_y - screen_y[2])) / denominator;
                weights[2] = 1.0 - weights[0] - weights[1];
                if (weights[0] >= -1.0e-8 &&
                    weights[1] >= -1.0e-8 &&
                    weights[2] >= -1.0e-8) {
                    contains_point = true;
                    break;
                }
            }
            if (!contains_point)
                continue;
            const std::array<double, 3> lambda_dx{{
                (screen_y[1] - screen_y[2]) / denominator,
                (screen_y[2] - screen_y[0]) / denominator,
                (screen_y[0] - screen_y[1]) / denominator,
            }};
            const std::array<double, 3> lambda_dy{{
                (screen_x[2] - screen_x[1]) / denominator,
                (screen_x[0] - screen_x[2]) / denominator,
                (screen_x[1] - screen_x[0]) / denominator,
            }};
            double q = 0.0;
            double numerator_u = 0.0;
            double numerator_v = 0.0;
            double q_dx = 0.0;
            double q_dy = 0.0;
            double numerator_u_dx = 0.0;
            double numerator_u_dy = 0.0;
            double numerator_v_dx = 0.0;
            double numerator_v_dy = 0.0;
            double depth = 0.0;
            for (std::size_t vertex = 0; vertex < 3; ++vertex) {
                const double reciprocal_w = 1.0 / raster_vertices[vertex].w;
                const double weighted_u =
                    raster_vertices[vertex].u * reciprocal_w;
                const double weighted_v =
                    raster_vertices[vertex].v * reciprocal_w;
                q += weights[vertex] * reciprocal_w;
                numerator_u += weights[vertex] * weighted_u;
                numerator_v += weights[vertex] * weighted_v;
                q_dx += lambda_dx[vertex] * reciprocal_w;
                q_dy += lambda_dy[vertex] * reciprocal_w;
                numerator_u_dx += lambda_dx[vertex] * weighted_u;
                numerator_u_dy += lambda_dy[vertex] * weighted_u;
                numerator_v_dx += lambda_dx[vertex] * weighted_v;
                numerator_v_dy += lambda_dy[vertex] * weighted_v;
                depth += weights[vertex] *
                    raster_vertices[vertex].z * reciprocal_w;
            }
            if (!std::isfinite(q) || q <= 0.0)
                continue;
            const double q_squared = q * q;
            const double u = numerator_u / q;
            const double v = numerator_v / q;
            const double du_dx =
                (numerator_u_dx * q - numerator_u * q_dx) / q_squared;
            const double du_dy =
                (numerator_u_dy * q - numerator_u * q_dy) / q_squared;
            const double dv_dx =
                (numerator_v_dx * q - numerator_v * q_dx) / q_squared;
            const double dv_dy =
                (numerator_v_dy * q - numerator_v * q_dy) / q_squared;
            const double footprint_u =
                (std::max)(std::abs(du_dx), std::abs(du_dy));
            const double footprint_v =
                (std::max)(std::abs(dv_dx), std::abs(dv_dy));
            const bool textured =
                (material.primitive_flags & textured_flag) != 0;
            double color_r = 0.0;
            double color_g = 0.0;
            double color_b = 0.0;
            for (std::size_t vertex = 0; vertex < 3; ++vertex) {
                const double reciprocal_w = 1.0 / raster_vertices[vertex].w;
                color_r += weights[vertex] *
                    raster_vertices[vertex].r * reciprocal_w;
                color_g += weights[vertex] *
                    raster_vertices[vertex].g * reciprocal_w;
                color_b += weights[vertex] *
                    raster_vertices[vertex].b * reciprocal_w;
            }
            color_r /= q;
            color_g /= q;
            color_b /= q;
            std::uint16_t word = 1;
            std::size_t zero_taps = 0;
            std::size_t filter_taps = 1;
            bool discarded = false;
            double sample_r = 255.0;
            double sample_g = 255.0;
            double sample_b = 255.0;
            if (textured) {
                word = material_texture_word(
                    vram,
                    material,
                    static_cast<std::int32_t>(std::floor(u + 0.0001)),
                    static_cast<std::int32_t>(std::floor(v + 0.0001)));
                const auto bilinear_color = [&] (
                    double sample_u,
                    double sample_v,
                    std::uint16_t fallback_word
                ) {
                    std::array<double, 3> result{};
                    const std::int32_t base_u = static_cast<std::int32_t>(
                        std::floor(sample_u));
                    const std::int32_t base_v = static_cast<std::int32_t>(
                        std::floor(sample_v));
                    const double blend_u = sample_u - std::floor(sample_u);
                    const double blend_v = sample_v - std::floor(sample_v);
                    const std::array<std::array<std::int32_t, 2>, 4> offsets{{
                        {{0, 0}}, {{1, 0}}, {{0, 1}}, {{1, 1}},
                    }};
                    const std::array<double, 4> weights{{
                        (1.0 - blend_u) * (1.0 - blend_v),
                        blend_u * (1.0 - blend_v),
                        (1.0 - blend_u) * blend_v,
                        blend_u * blend_v,
                    }};
                    double coverage = 0.0;
                    for (std::size_t tap = 0; tap < offsets.size(); ++tap) {
                        const std::uint16_t sampled = material_texture_word(
                            vram,
                            material,
                            base_u + offsets[tap][0],
                            base_v + offsets[tap][1]);
                        if (sampled == 0)
                            continue;
                        result[0] += expand_ps1_five(sampled & 31U) *
                            weights[tap];
                        result[1] += expand_ps1_five(
                            (sampled >> 5U) & 31U) * weights[tap];
                        result[2] += expand_ps1_five(
                            (sampled >> 10U) & 31U) * weights[tap];
                        coverage += weights[tap];
                    }
                    if (coverage > 0.0001) {
                        for (double& channel : result)
                            channel /= coverage;
                    } else {
                        result = {
                            static_cast<double>(expand_ps1_five(
                                fallback_word & 31U)),
                            static_cast<double>(expand_ps1_five(
                                (fallback_word >> 5U) & 31U)),
                            static_cast<double>(expand_ps1_five(
                                (fallback_word >> 10U) & 31U)),
                        };
                    }
                    return result;
                };
                if ((std::max)(footprint_u, footprint_v) > 1.0) {
                    sample_r = 0.0;
                    sample_g = 0.0;
                    sample_b = 0.0;
                    std::size_t covered_taps = 0;
                    const double dx_length_squared =
                        du_dx * du_dx + dv_dx * dv_dx;
                    const double dy_length_squared =
                        du_dy * du_dy + dv_dy * dv_dy;
                    const bool dx_major =
                        dx_length_squared >= dy_length_squared;
                    const double major_u = dx_major ? du_dx : du_dy;
                    const double major_v = dx_major ? dv_dx : dv_dy;
                    const double minor_u = dx_major ? du_dy : du_dx;
                    const double minor_v = dx_major ? dv_dy : dv_dx;
                    const double major_length = std::sqrt((std::max)(
                        dx_length_squared,
                        dy_length_squared));
                    const double minor_length = std::sqrt((std::min)(
                        dx_length_squared,
                        dy_length_squared));
                    const bool compact_footprint =
                        major_length < 2.0 && minor_length < 2.0;
                    const std::size_t major_taps =
                        static_cast<std::size_t>(std::clamp(
                            std::ceil(major_length),
                            3.0,
                            16.0));
                    const std::size_t minor_taps =
                        static_cast<std::size_t>(std::clamp(
                            std::ceil(minor_length),
                            3.0,
                            4.0));
                    filter_taps = major_taps * minor_taps;
                    for (std::size_t major_index = 0;
                         major_index < major_taps;
                         ++major_index) {
                        const double major_amount =
                            (major_index + 0.5) / major_taps - 0.5;
                        for (std::size_t minor_index = 0;
                             minor_index < minor_taps;
                             ++minor_index) {
                            const double minor_amount =
                                (minor_index + 0.5) / minor_taps - 0.5;
                            const double sample_u =
                                u + major_amount * major_u +
                                minor_amount * minor_u;
                            const double sample_v =
                                v + major_amount * major_v +
                                minor_amount * minor_v;
                            const std::uint16_t sampled_word = material_texture_word(
                                vram,
                                material,
                                static_cast<std::int32_t>(
                                    std::floor(sample_u + 0.0001)),
                                static_cast<std::int32_t>(
                                    std::floor(sample_v + 0.0001)));
                            if (sampled_word == 0) {
                                ++zero_taps;
                                continue;
                            }
                            if (compact_footprint) {
                                const auto filtered = bilinear_color(
                                    sample_u, sample_v, sampled_word);
                                sample_r += filtered[0];
                                sample_g += filtered[1];
                                sample_b += filtered[2];
                            } else {
                                sample_r += expand_ps1_five(
                                    sampled_word & 31U);
                                sample_g += expand_ps1_five(
                                    (sampled_word >> 5U) & 31U);
                                sample_b += expand_ps1_five(
                                    (sampled_word >> 10U) & 31U);
                            }
                            ++covered_taps;
                        }
                    }
                    discarded = zero_taps == filter_taps;
                    if (covered_taps != 0) {
                        sample_r /= covered_taps;
                        sample_g /= covered_taps;
                        sample_b /= covered_taps;
                        const double area_weight = std::clamp(
                            (std::max)(footprint_u, footprint_v) - 1.0,
                            0.0,
                            1.0);
                        if (word != 0 && area_weight < 1.0) {
                            const auto center = bilinear_color(u, v, word);
                            sample_r = center[0] * (1.0 - area_weight) +
                                sample_r * area_weight;
                            sample_g = center[1] * (1.0 - area_weight) +
                                sample_g * area_weight;
                            sample_b = center[2] * (1.0 - area_weight) +
                                sample_b * area_weight;
                        }
                    }
                } else if (word == 0) {
                    sample_r = 0.0;
                    sample_g = 0.0;
                    sample_b = 0.0;
                    std::size_t visible_neighbors = 0;
                    const std::int32_t base_u = static_cast<std::int32_t>(
                        std::floor(u + 0.0001));
                    const std::int32_t base_v = static_cast<std::int32_t>(
                        std::floor(v + 0.0001));
                    const std::array<std::array<std::int32_t, 2>, 4> offsets{{
                        {{-1, 0}}, {{1, 0}}, {{0, -1}}, {{0, 1}},
                    }};
                    for (const auto& offset : offsets) {
                        const std::uint16_t sampled_word = material_texture_word(
                            vram,
                            material,
                            base_u + offset[0],
                            base_v + offset[1]);
                        if (sampled_word == 0)
                            continue;
                        sample_r += expand_ps1_five(sampled_word & 31U);
                        sample_g += expand_ps1_five(
                            (sampled_word >> 5U) & 31U);
                        sample_b += expand_ps1_five(
                            (sampled_word >> 10U) & 31U);
                        ++visible_neighbors;
                    }
                    discarded =
                        command_index >= opaque_track_surfaces.size() ||
                        opaque_track_surfaces[command_index] == 0 ||
                        visible_neighbors < 3;
                    if (visible_neighbors != 0) {
                        sample_r /= visible_neighbors;
                        sample_g /= visible_neighbors;
                        sample_b /= visible_neighbors;
                    }
                } else if (options.texture_smoothing) {
                    sample_r = 0.0;
                    sample_g = 0.0;
                    sample_b = 0.0;
                    double covered_weight = 0.0;
                    const std::int32_t base_u = static_cast<std::int32_t>(
                        std::floor(u));
                    const std::int32_t base_v = static_cast<std::int32_t>(
                        std::floor(v));
                    const double blend_u = u - std::floor(u);
                    const double blend_v = v - std::floor(v);
                    const std::array<std::array<std::int32_t, 2>, 4> offsets{{
                        {{0, 0}}, {{1, 0}}, {{0, 1}}, {{1, 1}},
                    }};
                    const std::array<double, 4> sample_weights{{
                        (1.0 - blend_u) * (1.0 - blend_v),
                        blend_u * (1.0 - blend_v),
                        (1.0 - blend_u) * blend_v,
                        blend_u * blend_v,
                    }};
                    for (std::size_t tap = 0; tap < offsets.size(); ++tap) {
                        const std::uint16_t sampled_word = material_texture_word(
                            vram,
                            material,
                            base_u + offsets[tap][0],
                            base_v + offsets[tap][1]);
                        if (sampled_word == 0)
                            continue;
                        sample_r += expand_ps1_five(sampled_word & 31U) *
                            sample_weights[tap];
                        sample_g += expand_ps1_five(
                            (sampled_word >> 5U) & 31U) * sample_weights[tap];
                        sample_b += expand_ps1_five(
                            (sampled_word >> 10U) & 31U) * sample_weights[tap];
                        covered_weight += sample_weights[tap];
                    }
                    if (covered_weight > 0.0001) {
                        sample_r /= covered_weight;
                        sample_g /= covered_weight;
                        sample_b /= covered_weight;
                    } else {
                        sample_r = expand_ps1_five(word & 31U);
                        sample_g = expand_ps1_five((word >> 5U) & 31U);
                        sample_b = expand_ps1_five((word >> 10U) & 31U);
                    }
                } else {
                    sample_r = expand_ps1_five(word & 31U);
                    sample_g = expand_ps1_five((word >> 5U) & 31U);
                    sample_b = expand_ps1_five((word >> 10U) & 31U);
                }
            }
            const bool raw_texture =
                (material.primitive_flags & 4U) != 0;
            double output_r = textured
                ? raw_texture
                    ? sample_r
                    : std::clamp(sample_r * color_r * 2.0 / 255.0, 0.0, 255.0)
                : color_r;
            double output_g = textured
                ? raw_texture
                    ? sample_g
                    : std::clamp(sample_g * color_g * 2.0 / 255.0, 0.0, 255.0)
                : color_g;
            double output_b = textured
                ? raw_texture
                    ? sample_b
                    : std::clamp(sample_b * color_b * 2.0 / 255.0, 0.0, 255.0)
                : color_b;
            if (options.dithering &&
                (material.environment_flags & 4U) != 0 &&
                (!textured || !raw_texture)) {
                static constexpr std::array<std::int32_t, 16> dither{{
                    -4, 0, -3, 1,
                     2, -2, 3, -1,
                    -3, 1, -4, 0,
                     3, -1, 2, -2,
                }};
                const std::int32_t pixel_x = static_cast<std::int32_t>(
                    std::floor(point_x));
                const std::int32_t pixel_y = static_cast<std::int32_t>(
                    std::floor(point_y));
                const std::int32_t adjustment = dither[
                    static_cast<std::size_t>((pixel_y & 3) * 4 +
                        (pixel_x & 3))];
                const auto quantize = [adjustment] (double value) {
                    const std::int32_t adjusted = std::clamp(
                        static_cast<std::int32_t>(value + 0.5) + adjustment,
                        0,
                        255);
                    return static_cast<double>(expand_ps1_five(
                        static_cast<std::uint16_t>(
                            (std::min)(adjusted >> 3, 31))));
                };
                output_r = quantize(output_r);
                output_g = quantize(output_g);
                output_b = quantize(output_b);
            }
            candidates.push_back(PixelProvenanceCandidate{
                command_index,
                depth,
                1.0 / q,
                u,
                v,
                footprint_u,
                footprint_v,
                minimum_x,
                minimum_y,
                maximum_x,
                maximum_y,
                word,
                zero_taps,
                filter_taps,
                color_r,
                color_g,
                color_b,
                sample_r,
                sample_g,
                sample_b,
                output_r,
                output_g,
                output_b,
                discarded,
                post_clip.clipped,
                post_clip.vertex_count,
            });
        }
        std::sort(
            candidates.begin(),
            candidates.end(),
            [] (const auto& left, const auto& right) {
                if (left.depth != right.depth)
                    return left.depth > right.depth;
                return left.command_index > right.command_index;
            });
        // Track and vehicle commands share the modern reversed-depth buffer.
        // Background commands are deliberately depthless, so sorting them by
        // their diagnostic view depth can never identify the owning fragment.
        auto visible = std::find_if(
            candidates.begin(), candidates.end(),
            [&draw_list] (const auto& candidate) {
                const auto& command = draw_list.commands[candidate.command_index];
                return !candidate.discarded &&
                    (command.object_kind == 1U || command.object_kind == 2U);
            });
        if (visible == candidates.end()) {
            visible = std::find_if(
                candidates.begin(), candidates.end(),
                [] (const auto& candidate) { return !candidate.discarded; });
        }
        std::fprintf(
            stderr,
            "[Render-Pixel-Provenance] frame=%llu poll=%d point=%zu/%.1f,%.1f "
            "candidates=%zu visible=%s%zu\n",
            static_cast<unsigned long long>(draw_list.frame_index),
            draw_list.input_poll,
            point_index,
            point_x,
            point_y,
            candidates.size(),
            visible == candidates.end() ? "none/" : "command/",
            visible == candidates.end() ? 0U : visible->command_index);
        const std::size_t report_count = (std::min)(
            candidates.size(), static_cast<std::size_t>(32));
        for (std::size_t rank = 0; rank < report_count; ++rank) {
            const auto& candidate = candidates[rank];
            const auto& command = draw_list.commands[candidate.command_index];
            const auto& material = draw_list.materials[command.material_index];
            std::fprintf(
                stderr,
                "[Render-Pixel-Candidate] point=%zu rank=%zu command=%zu "
                "source=%u kind=%u object=%u model=%08x ot=%d overlay=%u "
                "screen=%u "
                "depth=%.9f viewZ=%.3f discarded=%u clipped=%u/%zu "
                "textured=%u semi=%u "
                "word=%04x zero=%zu/%zu uv=%.3f,%.3f footprint=%.3f,%.3f "
                "color=%.1f,%.1f,%.1f sample=%.1f,%.1f,%.1f "
                "output=%.1f,%.1f,%.1f raw=%u flags=%08x env=%08x "
                "surface=%u cutout=%u face=%.4f,%.4f,%.4f "
                "screen=%.1f,%.1f..%.1f,%.1f tpage=%04x clut=%04x "
                "window=%d,%d/%d,%d "
                "transform=%016llx exact=%u t=%d,%d,%d "
                "uvCorners=%u,%u/%u,%u/%u,%u "
                "model=%d,%d,%d/%d,%d,%d/%d,%d,%d "
                "world=%.3f,%.3f,%.3f/%.3f,%.3f,%.3f/"
                    "%.3f,%.3f,%.3f "
                "view=%.3f,%.3f,%.3f/%.3f,%.3f,%.3f/"
                    "%.3f,%.3f,%.3f "
                "rgb=%u,%u,%u/%u,%u,%u/%u,%u,%u\n",
                point_index,
                rank,
                candidate.command_index,
                command.source_command_index,
                command.object_kind,
                command.object_id,
                command.model_pointer,
                command.ordering_table_index,
                track_overlay_layer(material.primitive_flags),
                (material.primitive_flags &
                    world_primitive_screen_space_flag) != 0 ? 1U : 0U,
                candidate.depth,
                candidate.view_z,
                candidate.discarded ? 1U : 0U,
                candidate.homogeneous_clipped ? 1U : 0U,
                candidate.clipped_vertex_count,
                (material.primitive_flags & textured_flag) != 0 ? 1U : 0U,
                (material.primitive_flags & semi_transparent_flag) != 0 ? 1U : 0U,
                candidate.word,
                candidate.zero_taps,
                candidate.filter_taps,
                candidate.u,
                candidate.v,
                candidate.footprint_u,
                candidate.footprint_v,
                candidate.color_r,
                candidate.color_g,
                candidate.color_b,
                candidate.sample_r,
                candidate.sample_g,
                candidate.sample_b,
                candidate.output_r,
                candidate.output_g,
                candidate.output_b,
                (material.primitive_flags & 4U) != 0 ? 1U : 0U,
                material.primitive_flags,
                material.environment_flags,
                candidate.command_index < opaque_track_surfaces.size() &&
                    opaque_track_surfaces[candidate.command_index] != 0
                    ? 1U
                    : 0U,
                alpha_tested_track_cutout_candidate(
                    command,
                    material,
                    candidate.command_index < opaque_track_surfaces.size() &&
                        opaque_track_surfaces[candidate.command_index] != 0)
                    ? 1U : 0U,
                command.face_normal_x,
                command.face_normal_y,
                command.face_normal_z,
                candidate.minimum_x,
                candidate.minimum_y,
                candidate.maximum_x,
                candidate.maximum_y,
                material.texture_page,
                material.clut,
                material.texture_mask_x,
                material.texture_mask_y,
                material.texture_offset_x,
                material.texture_offset_y,
                static_cast<unsigned long long>(command.transform_id),
                command.exact_transform_valid ? 1U : 0U,
                command.transform_translation[0],
                command.transform_translation[1],
                command.transform_translation[2],
                static_cast<unsigned>(command.vertices[0].u),
                static_cast<unsigned>(command.vertices[0].v),
                static_cast<unsigned>(command.vertices[1].u),
                static_cast<unsigned>(command.vertices[1].v),
                static_cast<unsigned>(command.vertices[2].u),
                static_cast<unsigned>(command.vertices[2].v),
                command.vertices[0].model_x,
                command.vertices[0].model_y,
                command.vertices[0].model_z,
                command.vertices[1].model_x,
                command.vertices[1].model_y,
                command.vertices[1].model_z,
                command.vertices[2].model_x,
                command.vertices[2].model_y,
                command.vertices[2].model_z,
                command.vertices[0].world_x,
                command.vertices[0].world_y,
                command.vertices[0].world_z,
                command.vertices[1].world_x,
                command.vertices[1].world_y,
                command.vertices[1].world_z,
                command.vertices[2].world_x,
                command.vertices[2].world_y,
                command.vertices[2].world_z,
                command.vertices[0].view_x,
                command.vertices[0].view_y,
                command.vertices[0].view_z,
                command.vertices[1].view_x,
                command.vertices[1].view_y,
                command.vertices[1].view_z,
                command.vertices[2].view_x,
                command.vertices[2].view_y,
                command.vertices[2].view_z,
                static_cast<unsigned>(command.vertices[0].r),
                static_cast<unsigned>(command.vertices[0].g),
                static_cast<unsigned>(command.vertices[0].b),
                static_cast<unsigned>(command.vertices[1].r),
                static_cast<unsigned>(command.vertices[1].g),
                static_cast<unsigned>(command.vertices[1].b),
                static_cast<unsigned>(command.vertices[2].r),
                static_cast<unsigned>(command.vertices[2].g),
                static_cast<unsigned>(command.vertices[2].b));
        }
        if (visible != candidates.end()) {
            const std::size_t owner_index = visible->command_index;
            const auto& owner = draw_list.commands[owner_index];
            const auto& owner_material =
                draw_list.materials[owner.material_index];
            std::size_t sibling_rank = 0;
            const std::size_t first_command = owner_index > 8
                ? owner_index - 8
                : 0;
            const std::size_t last_command = (std::min)(
                draw_list.commands.size(), owner_index + 9);
            for (std::size_t command_index = first_command;
                 command_index < last_command;
                 ++command_index) {
                const auto& sibling = draw_list.commands[command_index];
                const auto& sibling_material =
                    draw_list.materials[sibling.material_index];
                if (
                    sibling.object_id != owner.object_id ||
                    sibling.model_pointer != owner.model_pointer ||
                    sibling.transform_id != owner.transform_id ||
                    sibling_material.texture_page !=
                        owner_material.texture_page ||
                    sibling_material.clut != owner_material.clut
                )
                    continue;
                std::fprintf(
                    stderr,
                    "[Render-Pixel-Sibling] point=%zu rank=%zu command=%zu "
                    "source=%u object=%u model=%08x "
                    "screen=(%.3f,%.3f)(%.3f,%.3f)(%.3f,%.3f) "
                    "uvCorners=%u,%u/%u,%u/%u,%u "
                    "viewZ=%.3f/%.3f/%.3f flags=%08x\n",
                    point_index,
                    sibling_rank++,
                    command_index,
                    sibling.source_command_index,
                    sibling.object_id,
                    sibling.model_pointer,
                    sibling.vertices[0].screen_x,
                    sibling.vertices[0].screen_y,
                    sibling.vertices[1].screen_x,
                    sibling.vertices[1].screen_y,
                    sibling.vertices[2].screen_x,
                    sibling.vertices[2].screen_y,
                    static_cast<unsigned>(sibling.vertices[0].u),
                    static_cast<unsigned>(sibling.vertices[0].v),
                    static_cast<unsigned>(sibling.vertices[1].u),
                    static_cast<unsigned>(sibling.vertices[1].v),
                    static_cast<unsigned>(sibling.vertices[2].u),
                    static_cast<unsigned>(sibling.vertices[2].v),
                    sibling.vertices[0].view_z,
                    sibling.vertices[1].view_z,
                    sibling.vertices[2].view_z,
                    sibling_material.primitive_flags);
            }
        }
    }
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
    ComPtr<ID3DBlob> cutout_depth_pixel_blob;
    ComPtr<ID3DBlob> cutout_fringe_pixel_blob;
    ComPtr<ID3DBlob> road_overlay_pixel_blob;
    ComPtr<ID3DBlob> road_overlay_tint_pixel_blob;
    ComPtr<ID3DBlob> command_id_pixel_blob;
    ComPtr<ID3DBlob> screen_grid_mask_pixel_blob;
    ComPtr<ID3DBlob> screen_grid_vertex_blob;
    ComPtr<ID3DBlob> screen_grid_pixel_blob;
    if (
        !compile_shader("VSMain", "vs_4_0", &vertex_blob) ||
        !compile_shader("PSMain", "ps_4_0", &pixel_blob) ||
        !compile_shader(
            "PSMainCutoutDepth", "ps_4_0",
            &cutout_depth_pixel_blob) ||
        !compile_shader(
            "PSMainCutoutFringe", "ps_4_0",
            &cutout_fringe_pixel_blob) ||
        !compile_shader(
            "PSMainRoadOverlay", "ps_4_0", &road_overlay_pixel_blob) ||
        !compile_shader(
            "PSMainRoadOverlayTint", "ps_4_0",
            &road_overlay_tint_pixel_blob) ||
        !compile_shader(
            "PSMainCommandId", "ps_4_0", &command_id_pixel_blob)
        || !compile_shader(
            "PSMainScreenGridMask", "ps_4_0",
            &screen_grid_mask_pixel_blob)
        || !compile_screen_grid_shader(
            "VSMainScreenGrid", "vs_4_0", &screen_grid_vertex_blob)
        || !compile_screen_grid_shader(
            "PSMainScreenGrid", "ps_4_0", &screen_grid_pixel_blob)
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
            resources->pixel_shader.GetAddressOf())) ||
        FAILED(resources->device->CreatePixelShader(
            cutout_depth_pixel_blob->GetBufferPointer(),
            cutout_depth_pixel_blob->GetBufferSize(),
            nullptr,
            resources->cutout_depth_pixel_shader.GetAddressOf())) ||
        FAILED(resources->device->CreatePixelShader(
            cutout_fringe_pixel_blob->GetBufferPointer(),
            cutout_fringe_pixel_blob->GetBufferSize(),
            nullptr,
            resources->cutout_fringe_pixel_shader.GetAddressOf())) ||
        FAILED(resources->device->CreatePixelShader(
            road_overlay_pixel_blob->GetBufferPointer(),
            road_overlay_pixel_blob->GetBufferSize(),
            nullptr,
            resources->road_overlay_pixel_shader.GetAddressOf())) ||
        FAILED(resources->device->CreatePixelShader(
            road_overlay_tint_pixel_blob->GetBufferPointer(),
            road_overlay_tint_pixel_blob->GetBufferSize(),
            nullptr,
            resources->road_overlay_tint_pixel_shader.GetAddressOf())) ||
        FAILED(resources->device->CreatePixelShader(
            command_id_pixel_blob->GetBufferPointer(),
            command_id_pixel_blob->GetBufferSize(),
            nullptr,
            resources->command_id_pixel_shader.GetAddressOf())) ||
        FAILED(resources->device->CreatePixelShader(
            screen_grid_mask_pixel_blob->GetBufferPointer(),
            screen_grid_mask_pixel_blob->GetBufferSize(),
            nullptr,
            resources->screen_grid_mask_pixel_shader.GetAddressOf())) ||
        FAILED(resources->device->CreateVertexShader(
            screen_grid_vertex_blob->GetBufferPointer(),
            screen_grid_vertex_blob->GetBufferSize(),
            nullptr,
            resources->screen_grid_vertex_shader.GetAddressOf())) ||
        FAILED(resources->device->CreatePixelShader(
            screen_grid_pixel_blob->GetBufferPointer(),
            screen_grid_pixel_blob->GetBufferSize(),
            nullptr,
            resources->screen_grid_pixel_shader.GetAddressOf()))
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
                    auto& state = resources->depth_states
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
    for (int check = 0; check < 2; ++check) {
        resources->road_overlay_depth_states[check] =
            road_overlay_depth_state(resources->device.Get(), check != 0);
        resources->vehicle_reflection_depth_states[check] =
            vehicle_reflection_depth_state(
                resources->device.Get(), check != 0);
        if (
            !resources->road_overlay_depth_states[check] ||
            !resources->vehicle_reflection_depth_states[check]
        )
            return false;
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
    resources->color_shader_view.Reset();
    resources->color_texture.Reset();
    resources->screen_grid_view.Reset();
    resources->screen_grid_texture.Reset();
    resources->screen_grid_world_shader_view.Reset();
    resources->screen_grid_world_texture.Reset();
    resources->screen_grid_mask_shader_view.Reset();
    resources->screen_grid_mask_view.Reset();
    resources->screen_grid_mask_texture.Reset();
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
    resources->screen_grid_width = 0;
    resources->screen_grid_height = 0;
}

void release_mutable_frame_resources(FrameInputResources* frame) {
    frame->vertex_buffer.Reset();
    frame->vertex_buffer_bytes = 0;
    frame->material_view.Reset();
    frame->material_buffer.Reset();
    frame->material_buffer_bytes = 0;
    for (auto& constant_buffer : frame->constant_buffers)
        constant_buffer.Reset();
    frame->screen_arc_buffer.Reset();
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
    if (!frame->screen_arc_buffer) {
        D3D11_BUFFER_DESC description{};
        description.ByteWidth = sizeof(ScreenArcConstants);
        description.Usage = D3D11_USAGE_DEFAULT;
        description.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
        if (FAILED(device->CreateBuffer(
                &description,
                nullptr,
                frame->screen_arc_buffer.GetAddressOf())))
            return false;
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
    std::uint32_t output_height,
    std::uint32_t screen_grid_width,
    std::uint32_t screen_grid_height
) {
    ID3D11Device* device = resources->device.Get();
    if (
        resources->color_texture &&
        resources->output_width == output_width &&
        resources->output_height == output_height &&
        resources->screen_grid_width == screen_grid_width &&
        resources->screen_grid_height == screen_grid_height
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
    color_description.BindFlags =
        D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE;
    if (
        FAILED(device->CreateTexture2D(
            &color_description,
            nullptr,
            resources->color_texture.GetAddressOf())) ||
        FAILED(device->CreateRenderTargetView(
            resources->color_texture.Get(),
            nullptr,
            resources->color_view.GetAddressOf())) ||
        FAILED(device->CreateShaderResourceView(
            resources->color_texture.Get(),
            nullptr,
            resources->color_shader_view.GetAddressOf()))
    )
        return false;

    if (
        FAILED(device->CreateTexture2D(
            &color_description,
            nullptr,
            resources->screen_grid_texture.GetAddressOf())) ||
        FAILED(device->CreateRenderTargetView(
            resources->screen_grid_texture.Get(),
            nullptr,
            resources->screen_grid_view.GetAddressOf()))
    )
        return false;

    if (
        FAILED(device->CreateTexture2D(
            &color_description,
            nullptr,
            resources->screen_grid_world_texture.GetAddressOf())) ||
        FAILED(device->CreateShaderResourceView(
            resources->screen_grid_world_texture.Get(),
            nullptr,
            resources->screen_grid_world_shader_view.GetAddressOf()))
    )
        return false;

    D3D11_TEXTURE2D_DESC mask_description{};
    mask_description.Width = screen_grid_width;
    mask_description.Height = screen_grid_height;
    mask_description.MipLevels = 1;
    mask_description.ArraySize = 1;
    mask_description.Format = DXGI_FORMAT_R8_UNORM;
    mask_description.SampleDesc.Count = 1;
    mask_description.Usage = D3D11_USAGE_DEFAULT;
    mask_description.BindFlags =
        D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE;
    if (
        FAILED(device->CreateTexture2D(
            &mask_description,
            nullptr,
            resources->screen_grid_mask_texture.GetAddressOf())) ||
        FAILED(device->CreateRenderTargetView(
            resources->screen_grid_mask_texture.Get(),
            nullptr,
            resources->screen_grid_mask_view.GetAddressOf())) ||
        FAILED(device->CreateShaderResourceView(
            resources->screen_grid_mask_texture.Get(),
            nullptr,
            resources->screen_grid_mask_shader_view.GetAddressOf()))
    )
        return false;

    D3D11_TEXTURE2D_DESC depth_description = color_description;
    // Extending GT2's whole course to the horizon makes conventional D24
    // collapse distinct road layers to one value.  Reversed infinite depth
    // needs floating-point storage to preserve its precision advantage; the
    // stencil component retains the PS1 mask-bit contract.
    depth_description.Format = DXGI_FORMAT_D32_FLOAT_S8X24_UINT;
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
    resources->screen_grid_width = screen_grid_width;
    resources->screen_grid_height = screen_grid_height;
    return true;
}

} // namespace

WorldGpuRenderResult render_world_d3d11(
    const WorldDrawList& source_draw_list,
    const std::uint16_t* vram,
    std::size_t vram_word_count,
    std::uint8_t* output_rgba,
    std::size_t output_size,
    WorldGpuRenderOptions options,
    WorldGpuRenderStats* stats
) noexcept {
    std::optional<WorldDrawList> conformed_draw_list;
    VehicleReflectionEligibility source_reflections;
    VehicleReflectionEligibility vehicle_reflections;
    try {
        source_reflections = vehicle_reflection_eligibility(source_draw_list);
        conformed_draw_list = conform_vehicle_reflection_quads(
            source_draw_list, source_reflections, &vehicle_reflections);
        if (!conformed_draw_list)
            vehicle_reflections = std::move(source_reflections);
    } catch (const std::bad_alloc&) {
        return WorldGpuRenderResult::resource_failed;
    }
    const WorldDrawList& draw_list = conformed_draw_list ? *conformed_draw_list : source_draw_list;
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
    std::vector<AuthoredScreenArc> authored_screen_arcs;
    std::vector<std::uint8_t> opaque_track_surfaces;
    std::vector<std::array<float, 4>> surface_depth_planes;
    if (const char* enabled = std::getenv("OPENGT_RENDER_SMOOTH_WHEELS");
        enabled != nullptr && std::strcmp(enabled, "1") == 0) {
        try {
            smooth_wheels = build_smooth_wheels(draw_list);
        } catch (const std::bad_alloc&) {
            return WorldGpuRenderResult::resource_failed;
        }
    }
    try {
        opaque_track_surfaces =
            opaque_track_surface_eligibility(draw_list);
        surface_depth_planes = shared_surface_depth_planes(draw_list,
            vehicle_reflections, horizontal_projection_scale, output_width, output_height);
        hud_horizontal_placements =
            build_hud_horizontal_placements(draw_list);
        authored_screen_arcs = detect_authored_screen_arcs(draw_list);
        emit_vehicle_diagnostics(
            draw_list, smooth_wheels, options.synthetic_midpoint);
        emit_clip_rect_diagnostics(draw_list);
        emit_primitive_diagnostics(draw_list, options);
        emit_resident_edge_diagnostics(draw_list, options);
        emit_scene_diagnostics(draw_list, options);
        emit_texture_coverage_diagnostics(
            draw_list,
            vram,
            vram_word_count,
            options,
            opaque_track_surfaces);
        emit_texture_instance_diagnostics(
            draw_list, vram, vram_word_count, options);
        emit_pixel_provenance_diagnostics(
            draw_list,
            vram,
            vram_word_count,
            options,
            opaque_track_surfaces);
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
    // Perspective-correct 3D projection is fixed renderer behavior rather
    // than a quality option. The retained ABI flag is ignored here so an old
    // caller cannot silently reactivate affine world rendering.
    const std::vector<std::uint8_t> perspective_uv_eligibility =
        perspective_uv_island_eligibility(draw_list);
    const std::size_t vertex_count = std::max<std::size_t>(
        3,
        authored_vertex_count +
            smooth_wheels.size() * smooth_wheel_vertices);
    if (!ensure_output_resources(
            &base,
            output_width,
            output_height,
            target_display_width,
            static_cast<std::uint32_t>(draw_list.display_height)))
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
    // Once all four development slots are populated, read the slot this call
    // will overwrite. It was submitted four render calls ago, moving the
    // blocking Map out of the producer's immediate submission path.
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
        0.0F,
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
            return GpuVertex{
                {
                    static_cast<float>(ndc_x * view_z),
                    static_cast<float>(ndc_y * view_z),
                    static_cast<float>(near_plane),
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
                command_index < opaque_track_surfaces.size() &&
                opaque_track_surfaces[command_index] != 0;
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
                surface_depth_planes[command_index],
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
    ID3D11PixelShader* bound_pixel_shader = base.pixel_shader.Get();
    ID3D11ShaderResourceView* shader_views[] = {
        frame.vram_view.Get(),
        frame.material_view.Get(),
        options.high_resolution_textures
            ? base.replacement_view.Get()
            : nullptr,
        nullptr,
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
            1U,
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
    ScreenArcConstants screen_arc_constants{};
    screen_arc_constants.count = static_cast<std::uint32_t>(
        (std::min)(authored_screen_arcs.size(), maximum_screen_arcs));
    for (std::size_t index = 0;
         index < screen_arc_constants.count;
         ++index) {
        const auto& source = authored_screen_arcs[index];
        const auto& placement =
            hud_horizontal_placements[source.command_index];
        auto& destination = screen_arc_constants.arcs[index];
        destination.center_x =
            (source.center_x - draw_list.display_x) * output_scale +
            hud_output_offset(placement);
        destination.center_y =
            (source.center_y - draw_list.display_y) * output_scale;
        destination.radius_x = source.radius_x * output_scale;
        destination.radius_y = source.radius_y * output_scale;
        destination.orientation = source.orientation;
    }
    context->UpdateSubresource(
        frame.screen_arc_buffer.Get(),
        0,
        nullptr,
        &screen_arc_constants,
        0,
        0);
    static thread_local std::size_t reported_screen_arc_count =
        (std::numeric_limits<std::size_t>::max)();
    if (reported_screen_arc_count != authored_screen_arcs.size()) {
        reported_screen_arc_count = authored_screen_arcs.size();
        std::fprintf(
            stderr,
            "[Render-Screen-Arcs] count=%zu policy=authored-radial-fan\n",
            authored_screen_arcs.size());
    }

    WorldViewChannel depth_channel = WorldViewChannel::main_view;
    bool depth_channel_initialized = false;
    int bound_pass = -1;
    bool has_bound_scissor = false;
    D3D11_RECT bound_scissor{};
    ID3D11BlendState* bound_blend_state = nullptr;
    float bound_blend_factor = -1.0F;
    ID3D11DepthStencilState* bound_depth_state = nullptr;
    UINT bound_stencil_reference = (std::numeric_limits<UINT>::max)();
    static thread_local bool emitted_shipping_depth_contract = false;
    static thread_local bool emitted_shipping_layer_contract = false;
    const bool shipping_layer_contract =
        !emitted_shipping_layer_contract &&
        draw_list.track_commands >= 1000U &&
        draw_list.vehicle_commands != 0U;
    if (
        !emitted_shipping_depth_contract &&
        draw_list.track_commands >= 1000U &&
        draw_list.vehicle_commands != 0U
    ) {
        std::fprintf(
            stderr,
            "[Render-Depth] projection=reversed-infinite near=16 "
            "format=D32_FLOAT_S8X24_UINT compare=GREATER_EQUAL "
            "clear=0 stencilBits=8\n");
        emitted_shipping_depth_contract = true;
    }
    const bool diagnose_batches =
        std::getenv("OPENGT_RENDER_BATCH_DIAGNOSTICS") != nullptr ||
        shipping_layer_contract;
    std::array<std::uint64_t, world_render_layer_count> diagnostic_batches{};
    std::array<std::uint64_t, world_render_layer_count> diagnostic_draws{};
    std::array<std::uint64_t, world_render_layer_count> diagnostic_commands{};
    std::array<std::uint64_t, world_render_layer_count>
        diagnostic_transparent_draws{};
    std::array<std::uint64_t, 4> diagnostic_blend_batches{};
    std::array<std::size_t, world_render_layer_count> diagnostic_max_batch{};
    // GT2's projected backdrop can occur after vehicle commands in the source
    // ordering table. Because it intentionally paints color without owning
    // physical depth, drawing it late overwrites a vehicle's color while
    // leaving that vehicle's invisible depth behind to reject later road.
    // Road artwork is composited explicitly after opaque world depth while
    // retaining that physical depth test. Transparent effects follow so they
    // retain ordinary foreground ownership.
    const auto command_in_render_phase = [horizontal_projection_scale] (
        const WorldDrawCommand& command,
        const WorldMaterial& material,
        int phase
    ) noexcept {
        if ((material.primitive_flags &
                world_primitive_screen_space_flag) != 0)
            return phase == 4;
        // The resident course can retain GTE packets after projection has
        // saturated their authored SXY values. D3D clips ordinary intersecting
        // triangles correctly, but submitting a primitive whose three
        // vertices are outside the same homogeneous clip plane can produce a
        // thin reflected raster when W changes sign. Reject only the standard
        // mathematically empty cases before batching. Hor+ scales clip X in
        // the vertex buffer, so use that exact transformed coordinate here.
        const bool invalid = std::any_of(
            command.vertices,
            command.vertices + 3,
            [] (const WorldDrawVertex& vertex) {
                return !std::isfinite(vertex.clip_x) ||
                    !std::isfinite(vertex.clip_y) ||
                    !std::isfinite(vertex.clip_z) ||
                    !std::isfinite(vertex.clip_w);
            });
        const auto all_outside = [&] (const auto& outside) {
            return std::all_of(
                command.vertices,
                command.vertices + 3,
                outside);
        };
        const bool empty_homogeneous_intersection =
            all_outside([&] (const WorldDrawVertex& vertex) {
                return vertex.clip_x * horizontal_projection_scale <
                    -vertex.clip_w;
            }) ||
            all_outside([&] (const WorldDrawVertex& vertex) {
                return vertex.clip_x * horizontal_projection_scale >
                    vertex.clip_w;
            }) ||
            all_outside([] (const WorldDrawVertex& vertex) {
                return vertex.clip_y < -vertex.clip_w;
            }) ||
            all_outside([] (const WorldDrawVertex& vertex) {
                return vertex.clip_y > vertex.clip_w;
            }) ||
            all_outside([] (const WorldDrawVertex& vertex) {
                return vertex.clip_z < 0.0F;
            }) ||
            all_outside([] (const WorldDrawVertex& vertex) {
                return vertex.clip_z > vertex.clip_w;
            });
        if (invalid || empty_homogeneous_intersection)
            return false;
        if (command.object_kind == 3U)
            return phase == 0;
        const bool replacement = command.object_kind == 1U &&
            (material.primitive_flags &
                world_primitive_track_replacement_flag) != 0;
        if (replacement) {
            // A detailed replacement is still the ordinary road everywhere
            // outside its coarse support. Draw it once with physical depth,
            // then redraw only the typed overlap in the priority phase.
            return phase == 1 || phase == 2;
        }
        const bool overlay = command.object_kind == 1U &&
            track_overlay_layer(material.primitive_flags) != 0;
        if (overlay)
            return phase == 2;
        const bool semitransparent =
            (material.primitive_flags & semi_transparent_flag) != 0;
        if (semitransparent)
            return phase == 3;
        return phase == 1;
    };
    enum class DebugRenderLayerFilter {
        all,
        background,
        track,
        vehicle,
        unclassified,
        screen,
    };
    const auto debug_render_layer_filter = [] () noexcept {
        const char* configured = std::getenv(
            "OPENGT_DEBUG_RENDER_LAYER");
        if (configured == nullptr || configured[0] == '\0')
            return DebugRenderLayerFilter::all;
        if (std::strcmp(configured, "background") == 0)
            return DebugRenderLayerFilter::background;
        if (std::strcmp(configured, "track") == 0)
            return DebugRenderLayerFilter::track;
        if (std::strcmp(configured, "vehicle") == 0)
            return DebugRenderLayerFilter::vehicle;
        if (std::strcmp(configured, "unclassified") == 0)
            return DebugRenderLayerFilter::unclassified;
        if (std::strcmp(configured, "screen") == 0)
            return DebugRenderLayerFilter::screen;
        return DebugRenderLayerFilter::all;
    }();
    const auto debug_layer_selected = [debug_render_layer_filter] (
        WorldRenderLayer layer
    ) noexcept {
        if (debug_render_layer_filter == DebugRenderLayerFilter::all)
            return true;
        return static_cast<std::size_t>(debug_render_layer_filter) - 1U ==
            static_cast<std::size_t>(layer);
    };
    const char* debug_track_source = std::getenv(
        "OPENGT_DEBUG_TRACK_SOURCE");
    const bool debug_track_overlays_only = std::getenv(
        "OPENGT_DEBUG_TRACK_OVERLAYS_ONLY") != nullptr;
    const bool debug_road_overlay_tint = std::getenv(
        "OPENGT_DEBUG_ROAD_OVERLAY_TINT") != nullptr;
    const bool debug_road_overlay_no_depth = std::getenv(
        "OPENGT_DEBUG_ROAD_OVERLAY_NO_DEPTH") != nullptr;
    const bool debug_command_id = std::getenv(
        "OPENGT_DEBUG_COMMAND_ID") != nullptr;
    const auto debug_optional_u32 = [] (
        const char* name
    ) noexcept -> std::optional<std::uint32_t> {
        const char* configured = std::getenv(name);
        if (configured == nullptr || configured[0] == '\0')
            return std::nullopt;
        char* end = nullptr;
        const unsigned long parsed = std::strtoul(configured, &end, 0);
        if (end == configured || *end != '\0')
            return std::nullopt;
        return static_cast<std::uint32_t>(parsed);
    };
    const auto debug_model = debug_optional_u32("OPENGT_DEBUG_MODEL");
    const auto debug_object = debug_optional_u32("OPENGT_DEBUG_OBJECT");
    const auto debug_command_id_log_poll = debug_optional_u32(
        "OPENGT_DEBUG_COMMAND_ID_LOG_POLL");
    const auto debug_identity_selected = [debug_model, debug_object] (
        const WorldDrawCommand& command
    ) noexcept {
        return (!debug_model || command.model_pointer == *debug_model) &&
            (!debug_object || command.object_id == *debug_object);
    };
    const auto debug_command_bound = [] (
        const char* name,
        std::size_t fallback
    ) noexcept {
        const char* configured = std::getenv(name);
        if (configured == nullptr || configured[0] == '\0')
            return fallback;
        return static_cast<std::size_t>(std::strtoull(
            configured, nullptr, 10));
    };
    const std::size_t debug_command_start = debug_command_bound(
        "OPENGT_DEBUG_COMMAND_START", 0U);
    const std::size_t debug_command_end = debug_command_bound(
        "OPENGT_DEBUG_COMMAND_END",
        (std::numeric_limits<std::size_t>::max)());
    const auto debug_command_selected = [
        debug_command_start,
        debug_command_end
    ] (std::size_t index) noexcept {
        return index >= debug_command_start && index <= debug_command_end;
    };
    const auto debug_track_source_selected = [debug_track_source] (
        const WorldDrawCommand& command,
        const WorldMaterial& material
    ) noexcept {
        if (debug_track_source == nullptr || command.object_kind != 1U)
            return true;
        const bool resident = (material.primitive_flags &
            world_primitive_resident_course_flag) != 0;
        if (std::strcmp(debug_track_source, "resident") == 0)
            return resident;
        if (std::strcmp(debug_track_source, "projected") == 0)
            return !resident;
        return true;
    };
    if (debug_command_id &&
        std::getenv("OPENGT_DEBUG_COMMAND_ID_LOG") != nullptr &&
        (!debug_command_id_log_poll ||
            draw_list.input_poll ==
                static_cast<std::int32_t>(*debug_command_id_log_poll))) {
        for (std::size_t command_index = 0;
             command_index < draw_list.commands.size();
             ++command_index) {
            const auto& command = draw_list.commands[command_index];
            if (command.material_index >= draw_list.materials.size())
                continue;
            const auto& material = draw_list.materials[command.material_index];
            if (!debug_command_selected(command_index) ||
                !debug_identity_selected(command) ||
                !debug_layer_selected(world_render_layer(command, material)) ||
                !debug_track_source_selected(command, material))
                continue;
            std::fprintf(
                stderr,
                "[Render-Command-Id] frame=%llu poll=%d command=%zu "
                "source=%u kind=%u object=%08x model=%08x flags=%08x "
                "env=%08x tpage=%04x clut=%04x transform=%016llx "
                "channel=%u ot=%d clip=%d,%d..%d,%d\n",
                static_cast<unsigned long long>(draw_list.frame_index),
                draw_list.input_poll,
                command_index,
                command.source_command_index,
                command.object_kind,
                command.object_id,
                command.model_pointer,
                material.primitive_flags,
                material.environment_flags,
                material.texture_page,
                material.clut,
                static_cast<unsigned long long>(command.transform_id),
                static_cast<unsigned>(command.channel),
                command.ordering_table_index,
                command.clip_x0,
                command.clip_y0,
                command.clip_x1,
                command.clip_y1);
            for (std::size_t vertex_index = 0;
                 vertex_index < 3U;
                 ++vertex_index) {
                const auto& vertex = command.vertices[vertex_index];
                std::fprintf(
                    stderr,
                    "[Render-Command-Vertex] command=%zu vertex=%zu "
                    "screen=(%.9f,%.9f) authored=(%d,%d) "
                    "model=(%d,%d,%d) world=(%.9f,%.9f,%.9f) "
                    "view=(%.9f,%.9f,%.9f) exactView=(%d,%d,%d) "
                    "clip=(%.9f,%.9f,%.9f,%.9f)\n",
                    command_index,
                    vertex_index,
                    vertex.screen_x,
                    vertex.screen_y,
                    vertex.authored_screen_x,
                    vertex.authored_screen_y,
                    vertex.model_x,
                    vertex.model_y,
                    vertex.model_z,
                    vertex.world_x,
                    vertex.world_y,
                    vertex.world_z,
                    vertex.view_x,
                    vertex.view_y,
                    vertex.view_z,
                    vertex.exact_view_x,
                    vertex.exact_view_y,
                    vertex.exact_view_z,
                    vertex.clip_x,
                    vertex.clip_y,
                    vertex.clip_z,
                    vertex.clip_w);
            }
            const auto post_clip = diagnostic_homogeneous_clip(
                command,
                horizontal_projection_scale);
            std::fprintf(
                stderr,
                "[Render-Command-PostClip] command=%zu finite=%d "
                "bounded=%d clipped=%d vertices=%zu target=%ux%u "
                "hscale=%.9f\n",
                command_index,
                post_clip.finite ? 1 : 0,
                post_clip.bounded ? 1 : 0,
                post_clip.clipped ? 1 : 0,
                post_clip.vertex_count,
                output_width,
                output_height,
                horizontal_projection_scale);
            for (std::size_t vertex_index = 0;
                 vertex_index < post_clip.vertex_count;
                 ++vertex_index) {
                const auto& vertex = post_clip.vertices[vertex_index];
                const double reciprocal_w = 1.0 / vertex.w;
                const double output_x =
                    (vertex.x * reciprocal_w * 0.5 + 0.5) *
                    output_width;
                const double output_y =
                    (-vertex.y * reciprocal_w * 0.5 + 0.5) *
                    output_height;
                std::fprintf(
                    stderr,
                    "[Render-Command-PostClip-Vertex] command=%zu "
                    "vertex=%zu clip=(%.9f,%.9f,%.9f,%.9f) "
                    "output=(%.9f,%.9f)\n",
                    command_index,
                    vertex_index,
                    vertex.x,
                    vertex.y,
                    vertex.z,
                    vertex.w,
                    output_x,
                    output_y);
            }
        }
    }
    if (debug_render_layer_filter != DebugRenderLayerFilter::all) {
        static thread_local DebugRenderLayerFilter reported_filter =
            DebugRenderLayerFilter::all;
        if (reported_filter != debug_render_layer_filter) {
            std::fprintf(
                stderr,
                "[Render-Debug-Layer] filter=%s\n",
                std::getenv("OPENGT_DEBUG_RENDER_LAYER"));
            reported_filter = debug_render_layer_filter;
        }
    }
    if (debug_track_source != nullptr) {
        static thread_local std::string reported_track_source{};
        if (reported_track_source != debug_track_source) {
            std::fprintf(
                stderr,
                "[Render-Debug-Track-Source] filter=%s\n",
                debug_track_source);
            reported_track_source = debug_track_source;
        }
    }
    for (int render_phase = 0; render_phase < 5; ++render_phase) {
    if (render_phase == 4 && output_scale > 1) {
        context->OMSetRenderTargets(0, nullptr, nullptr);
        context->CopyResource(
            base.screen_grid_world_texture.Get(),
            base.color_texture.Get());
    }
    ID3D11RenderTargetView* phase_target = base.color_view.Get();
    context->OMSetRenderTargets(1, &phase_target, base.depth_view.Get());
    ID3D11PixelShader* phase_pixel_shader =
        debug_command_id
        ? base.command_id_pixel_shader.Get()
        : render_phase == 2 && debug_road_overlay_tint
        ? base.road_overlay_tint_pixel_shader.Get()
        : render_phase == 2
        ? base.road_overlay_pixel_shader.Get()
        : base.pixel_shader.Get();
    if (phase_pixel_shader != bound_pixel_shader) {
        context->PSSetShader(phase_pixel_shader, nullptr, 0);
        bound_pixel_shader = phase_pixel_shader;
    }
    bound_depth_state = nullptr;
    bound_blend_state = nullptr;
    has_bound_scissor = false;
    for (std::size_t command_index = 0;
         command_index < draw_list.commands.size();) {
        for (std::size_t wheel_index = 0;
             wheel_index < smooth_wheels.size();
             ++wheel_index) {
            if (render_phase != 1)
                break;
            if (!debug_layer_selected(WorldRenderLayer::vehicle))
                break;
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
            if (bound_pixel_shader != base.pixel_shader.Get()) {
                context->PSSetShader(base.pixel_shader.Get(), nullptr, 0);
                bound_pixel_shader = base.pixel_shader.Get();
            }
            const float blend_factor[4] = {1.0F, 1.0F, 1.0F, 1.0F};
            context->OMSetBlendState(
                base.blend_states[0].Get(),
                blend_factor,
                0xFFFFFFFFU);
            bound_blend_state = base.blend_states[0].Get();
            bound_blend_factor = 1.0F;
            ID3D11DepthStencilState* depth_state =
                base.depth_states[0][0][0][0].Get();
            context->OMSetDepthStencilState(depth_state, 0);
            bound_depth_state = depth_state;
            bound_stencil_reference = 0;
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
        const bool command_opaque_track_surface =
            command_index < opaque_track_surfaces.size() &&
            opaque_track_surfaces[command_index] != 0;
        const bool command_alpha_tested_cutout =
            alpha_tested_track_cutout_candidate(
                command,
                material,
                command_opaque_track_surface);
        const bool command_road_support =
            command.object_kind == 1U &&
            ((material.primitive_flags &
                world_primitive_track_overlay_support_flag) != 0 ||
                command_opaque_track_surface);
        const bool command_vehicle_reflection_support =
            command_index < vehicle_reflections.supports.size() &&
            vehicle_reflections.supports[command_index] != 0;
        const bool command_vehicle_reflection_detail =
            command_index < vehicle_reflections.details.size() &&
            vehicle_reflections.details[command_index] != 0;
        if (
            !debug_command_selected(command_index) ||
            !debug_identity_selected(command) ||
            !debug_layer_selected(world_render_layer(command, material)) ||
            !debug_track_source_selected(command, material) ||
            (debug_track_overlays_only && command.object_kind == 1U &&
                track_overlay_layer(material.primitive_flags) == 0)
        ) {
            ++command_index;
            continue;
        }
        if (!command_in_render_phase(command, material, render_phase)) {
            ++command_index;
            continue;
        }
        const int blend_mode =
            (material.texture_page >> 5) & 3;
        const bool vehicle_shadow =
            soft_vehicle_shadow(command, material);
        const bool combined_semitransparent =
            textured && semitransparent && blend_mode != 2;
        const bool alpha_tested_cutout_coverage =
            !debug_command_id &&
                render_phase == 1 &&
                command_alpha_tested_cutout;
        const int pass_count = alpha_tested_cutout_coverage
            ? 2
            : textured && semitransparent && !combined_semitransparent
            ? 2
            : 1;
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
            const bool next_opaque_track_surface =
                next_index < opaque_track_surfaces.size() &&
                opaque_track_surfaces[next_index] != 0;
            if (
                !debug_command_selected(command_index + batch_commands) ||
                !debug_identity_selected(next_command) ||
                !debug_layer_selected(
                    world_render_layer(next_command, next_material)) ||
                !debug_track_source_selected(next_command, next_material) ||
                (debug_track_overlays_only && next_command.object_kind == 1U &&
                    track_overlay_layer(next_material.primitive_flags) == 0)
            )
                break;
            if (!command_in_render_phase(
                    next_command, next_material, render_phase))
                break;
            if (!batch_compatible(
                    command,
                    material,
                    next_command,
                    next_material,
                    command_alpha_tested_cutout,
                    alpha_tested_track_cutout_candidate(
                        next_command,
                        next_material,
                        next_opaque_track_surface),
                    command_road_support,
                    next_command.object_kind == 1U &&
                        ((next_material.primitive_flags &
                            world_primitive_track_overlay_support_flag) != 0 ||
                            next_opaque_track_surface),
                    command_vehicle_reflection_support,
                    next_index < vehicle_reflections.supports.size() &&
                        vehicle_reflections.supports[next_index] != 0))
                break;
            ++batch_commands;
        }
        const std::size_t diagnostic_kind = static_cast<std::size_t>(
            world_render_layer(command, material));
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
            uses_modern_world_depth(command, material);
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
                D3D11_CLEAR_DEPTH | D3D11_CLEAR_STENCIL,
                0.0F,
                0);
        }
        if (uses_modern_depth) {
            depth_channel = command.channel;
            depth_channel_initialized = true;
        }
        for (int pass = 0; pass < pass_count; ++pass) {
            const int shader_pass = alpha_tested_cutout_coverage
                ? 0
                : combined_semitransparent ? 2 : pass;
            const bool blended = semitransparent &&
                (combined_semitransparent || !textured || pass == 1);
            ID3D11PixelShader* batch_pixel_shader =
                alpha_tested_cutout_coverage
                ? pass == 0
                    ? base.cutout_depth_pixel_shader.Get()
                    : base.cutout_fringe_pixel_shader.Get()
                : phase_pixel_shader;
            if (batch_pixel_shader != bound_pixel_shader) {
                context->PSSetShader(batch_pixel_shader, nullptr, 0);
                bound_pixel_shader = batch_pixel_shader;
            }
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
                : alpha_tested_cutout_coverage
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
            // Course and vehicles share one modern depth surface. Reversed
            // GREATER_EQUAL retains later coplanar vehicle detail while
            // allowing a car
            // authored before the resident course to protect its nearer
            // pixels from that later course submission.
            // Road paint, lane markers, arrows, grids, and other typed course
            // artwork are separate polygons in GT2, not baked road texture.
            // Their priority is therefore a class relationship: the opaque
            // pass records the nearest road owner, then the dedicated overlay
            // phase requires that owner and a passing physical depth test.
            const bool use_depth =
                !debug_road_overlay_no_depth &&
                options.depth_buffer && uses_modern_depth;
            const bool road_support =
                render_phase == 1 && command_road_support;
            const bool track_overlay =
                render_phase == 2 && command.object_kind == 1U &&
                track_overlay_layer(material.primitive_flags) != 0;
            const bool vehicle_reflection_detail =
                render_phase == 3 && command_vehicle_reflection_detail;
            ID3D11DepthStencilState* depth_state = nullptr;
            UINT stencil_reference = 0;
            if (track_overlay && use_depth) {
                depth_state = base.road_overlay_depth_states
                    [check_mask ? 1 : 0].Get();
                // Road overlays require an owned road pixel (bit 1 set). A
                // check-mask command also requires the PS1 mask bit (bit 0)
                // to stay clear, so both cases compare against 0b10.
                stencil_reference = 2U;
            } else if (vehicle_reflection_detail && use_depth) {
                depth_state = base.vehicle_reflection_depth_states
                    [check_mask ? 1 : 0].Get();
                // Stock environment detail may affect only a visible opaque
                // body owner (bit 2 set). Physical depth remains active, and
                // a check-mask command additionally requires bit 0 clear.
                stencil_reference = 4U;
            } else {
                depth_state = base.depth_states[use_depth ? 1 : 0]
                    [(blended && !combined_semitransparent) ||
                        (alpha_tested_cutout_coverage && pass == 1)
                        ? 1
                        : 0]
                    [check_mask ? 1 : 0]
                    [set_mask ? 1 : 0].Get();
                stencil_reference =
                    ((check_mask || set_mask) ? 1U : 0U) |
                    (road_support ? 2U : 0U) |
                    (render_phase == 1 &&
                            command_vehicle_reflection_support
                        ? 4U : 0U);
            }
            if (depth_state != bound_depth_state ||
                stencil_reference != bound_stencil_reference) {
                context->OMSetDepthStencilState(
                    depth_state, stencil_reference);
                bound_depth_state = depth_state;
                bound_stencil_reference = stencil_reference;
            }
            context->Draw(
                static_cast<UINT>(batch_commands * 3),
                static_cast<UINT>(command_index * 3));
            ++stats->draw_calls;
            if (blended || alpha_tested_cutout_coverage)
                ++stats->transparent_draw_calls;
        }
        command_index += batch_commands;
    }
    }

    ID3D11Texture2D* resolved_color_texture = base.color_texture.Get();
    const bool has_screen_commands = std::any_of(
        draw_list.commands.begin(),
        draw_list.commands.end(),
        [&draw_list] (const WorldDrawCommand& command) {
            return command.material_index < draw_list.materials.size() &&
                (draw_list.materials[command.material_index].primitive_flags &
                    world_primitive_screen_space_flag) != 0;
        });
    if (output_scale > 1 && has_screen_commands && !debug_command_id) {
        // GT2 authored its HUD and menus on the 320x240 GPU coverage grid.
        // Rasterizing those same polygon vertices directly at 4x exposes the
        // low-sided construction of shapes that were rounded by the original
        // pixel coverage. Build a one-bit coverage layer at the authored grid,
        // then resolve only covered output blocks from their native-pixel
        // centre. This preserves every submitted vertex, primitive, ordering
        // decision, and component offset; it does not recognize or rebuild a
        // particular menu shape.
        const float mask_clear[] = {0.0F, 0.0F, 0.0F, 0.0F};
        context->ClearRenderTargetView(
            base.screen_grid_mask_view.Get(), mask_clear);
        ID3D11RenderTargetView* mask_target =
            base.screen_grid_mask_view.Get();
        context->OMSetRenderTargets(1, &mask_target, nullptr);
        const D3D11_VIEWPORT mask_viewport{
            0.0F,
            0.0F,
            static_cast<float>(target_display_width),
            static_cast<float>(draw_list.display_height),
            0.0F,
            1.0F,
        };
        context->RSSetViewports(1, &mask_viewport);
        context->RSSetState(base.rasterizer.Get());
        context->IASetInputLayout(base.input_layout.Get());
        context->IASetPrimitiveTopology(
            D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
        context->IASetVertexBuffers(
            0, 1, &raw_vertex_buffer, &stride, &offset);
        context->VSSetShader(base.vertex_shader.Get(), nullptr, 0);
        context->PSSetShader(
            base.screen_grid_mask_pixel_shader.Get(), nullptr, 0);
        context->OMSetBlendState(
            base.blend_states[0].Get(), nullptr, 0xFFFFFFFFU);
        context->OMSetDepthStencilState(
            base.depth_states[0][0][0][0].Get(), 0);
        for (std::size_t command_index = 0;
             command_index < draw_list.commands.size();
             ++command_index) {
            const auto& command = draw_list.commands[command_index];
            if (command.material_index >= draw_list.materials.size())
                continue;
            const auto& material =
                draw_list.materials[command.material_index];
            if ((material.primitive_flags &
                    world_primitive_screen_space_flag) == 0 ||
                !debug_command_selected(command_index) ||
                !debug_identity_selected(command) ||
                !debug_layer_selected(
                    world_render_layer(command, material)) ||
                !debug_track_source_selected(command, material))
                continue;
            const std::int32_t native_horizontal_offset =
                static_cast<std::int32_t>(std::lround(
                    hud_output_offset(
                        hud_horizontal_placements[command_index]) /
                    static_cast<float>(output_scale)));
            const D3D11_RECT mask_scissor{
                std::clamp(
                    static_cast<LONG>(
                        command.clip_x0 - draw_list.display_x +
                        native_horizontal_offset),
                    0L,
                    static_cast<LONG>(target_display_width)),
                std::clamp(
                    static_cast<LONG>(
                        command.clip_y0 - draw_list.display_y),
                    0L,
                    static_cast<LONG>(draw_list.display_height)),
                std::clamp(
                    static_cast<LONG>(
                        command.clip_x1 - draw_list.display_x + 1 +
                        native_horizontal_offset),
                    0L,
                    static_cast<LONG>(target_display_width)),
                std::clamp(
                    static_cast<LONG>(
                        command.clip_y1 - draw_list.display_y + 1),
                    0L,
                    static_cast<LONG>(draw_list.display_height)),
            };
            context->RSSetScissorRects(1, &mask_scissor);
            context->Draw(
                3,
                static_cast<UINT>(command_index * 3));
        }

        ID3D11RenderTargetView* grid_target =
            base.screen_grid_view.Get();
        context->OMSetRenderTargets(1, &grid_target, nullptr);
        context->RSSetViewports(1, &viewport);
        const D3D11_RECT full_scissor{
            0L,
            0L,
            static_cast<LONG>(output_width),
            static_cast<LONG>(output_height),
        };
        context->RSSetScissorRects(1, &full_scissor);
        context->IASetInputLayout(nullptr);
        ID3D11Buffer* no_vertex_buffer = nullptr;
        const UINT no_stride = 0;
        const UINT no_offset = 0;
        context->IASetVertexBuffers(
            0, 1, &no_vertex_buffer, &no_stride, &no_offset);
        context->VSSetShader(
            base.screen_grid_vertex_shader.Get(), nullptr, 0);
        context->PSSetShader(
            base.screen_grid_pixel_shader.Get(), nullptr, 0);
        ID3D11Buffer* screen_arc_buffer = frame.screen_arc_buffer.Get();
        context->PSSetConstantBuffers(1, 1, &screen_arc_buffer);
        ID3D11ShaderResourceView* grid_views[] = {
            base.color_shader_view.Get(),
            base.screen_grid_mask_shader_view.Get(),
            base.screen_grid_world_shader_view.Get(),
        };
        context->PSSetShaderResources(3, 3, grid_views);
        context->OMSetBlendState(
            base.blend_states[0].Get(), nullptr, 0xFFFFFFFFU);
        context->OMSetDepthStencilState(
            base.depth_states[0][0][0][0].Get(), 0);
        context->Draw(3, 0);
        ID3D11ShaderResourceView* empty_grid_views[] = {
            nullptr,
            nullptr,
            nullptr,
        };
        context->PSSetShaderResources(3, 3, empty_grid_views);
        resolved_color_texture = base.screen_grid_texture.Get();
    }

    if (diagnose_batches) {
        std::fprintf(
            stderr,
            "[Render-Batches] background=%llu/%llu/%llu/max%zu "
            "track=%llu/%llu/%llu/max%zu "
            "vehicle=%llu/%llu/%llu/max%zu "
            "unclassified=%llu/%llu/%llu/max%zu "
            "screen=%llu/%llu/%llu/max%zu "
            "transparent=%llu/%llu/%llu/%llu/%llu "
            "depth=track+vehicle blendModes=%llu/%llu/%llu/%llu\n",
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
            static_cast<unsigned long long>(diagnostic_commands[3]),
            static_cast<unsigned long long>(diagnostic_batches[3]),
            static_cast<unsigned long long>(diagnostic_draws[3]),
            diagnostic_max_batch[3],
            static_cast<unsigned long long>(diagnostic_commands[4]),
            static_cast<unsigned long long>(diagnostic_batches[4]),
            static_cast<unsigned long long>(diagnostic_draws[4]),
            diagnostic_max_batch[4],
            static_cast<unsigned long long>(
                diagnostic_transparent_draws[0]),
            static_cast<unsigned long long>(
                diagnostic_transparent_draws[1]),
            static_cast<unsigned long long>(
                diagnostic_transparent_draws[2]),
            static_cast<unsigned long long>(
                diagnostic_transparent_draws[3]),
            static_cast<unsigned long long>(
                diagnostic_transparent_draws[4]),
            static_cast<unsigned long long>(diagnostic_blend_batches[0]),
            static_cast<unsigned long long>(diagnostic_blend_batches[1]),
            static_cast<unsigned long long>(diagnostic_blend_batches[2]),
            static_cast<unsigned long long>(diagnostic_blend_batches[3]));
        if (shipping_layer_contract)
            emitted_shipping_layer_contract = true;
    }

    if (options.asynchronous_readback) {
        context->CopyResource(
            base.async_staging_textures[async_staging_write].Get(),
            resolved_color_texture);
        context->End(
            base.async_completion_queries[async_staging_write].Get());
        base.async_staging_write_index =
            (async_staging_write + 1U) %
            static_cast<UINT>(base.async_staging_textures.size());
        ++base.async_staging_count;
    } else {
        // Keep four chronological development images behind the GPU. Each slot
        // is read before overwrite, preserving order while allowing earlier
        // copies time to complete.
        context->CopyResource(
            base.staging_textures[staging_write].Get(),
            resolved_color_texture);
        context->End(base.staging_completion_queries[staging_write].Get());
    }
    // The immediate context may otherwise retain several CopyResource calls
    // in its driver command buffer until the later staging Map forces a
    // flush. Submit now so the GPU performs this copy during the intentional
    // four-image development readback delay; Flush does not wait for completion.
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
