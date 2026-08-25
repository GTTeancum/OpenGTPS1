#include "opengt/world_draw_list.hpp"

#include <algorithm>
#include <cmath>
#include <limits>
#include <unordered_map>
#include <utility>
#include <new>

namespace opengt::render {
namespace {

std::uint32_t divide(std::uint32_t h, std::uint32_t depth) {
    if (h >= depth * 2U)
        return 0x1FFFF;
    int shift = 0;
    std::uint32_t value = depth;
    while ((value & 0x8000U) == 0U) {
        value <<= 1;
        ++shift;
    }
    std::uint64_t numerator = static_cast<std::uint64_t>(h) << shift;
    std::uint64_t denominator =
        static_cast<std::uint64_t>(depth) << shift;
    int index = static_cast<int>((denominator - 0x7FC0) >> 7);
    index = std::clamp(index, 0, 0x100);
    const int unr = std::clamp(
        (0x40000 / (index + 0x100) + 1) / 2 - 0x101,
        0,
        0xFF);
    std::uint64_t reciprocal = static_cast<std::uint64_t>(unr) + 0x101;
    denominator =
        (0x2000080ULL - denominator * reciprocal) >> 8;
    denominator =
        (0x0000080ULL + denominator * reciprocal) >> 8;
    const std::uint64_t result =
        (numerator * denominator + 0x8000) >> 16;
    return static_cast<std::uint32_t>(
        std::min<std::uint64_t>(result, 0x1FFFF));
}

bool same_material(
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
}

bool main_projection(
    const WorldCaptureHeader& header,
    const WorldCaptureTriangle& triangle
) {
    if (
        triangle.draw_offset_x != header.draw_offset_x ||
        triangle.draw_offset_y != header.draw_offset_y
    )
        return false;
    for (const auto& vertex : triangle.vertices) {
        if (
            vertex.projection_offset_x != header.projection_offset_x ||
            vertex.projection_offset_y != header.projection_offset_y ||
            vertex.projection_plane != header.projection_plane
        )
            return false;
    }
    return true;
}

bool displayed_screen_target(
    const WorldCaptureHeader& header,
    const WorldCaptureTriangle& triangle
) {
    if (
        triangle.draw_offset_x != header.draw_offset_x ||
        triangle.draw_offset_y != header.draw_offset_y
    )
        return false;
    const std::int32_t display_x1 =
        header.display_x + header.display_width - 1;
    const std::int32_t display_y1 =
        header.display_y + header.display_height - 1;
    if (
        triangle.clip_x1 < header.display_x ||
        triangle.clip_y1 < header.display_y ||
        triangle.clip_x0 > display_x1 ||
        triangle.clip_y0 > display_y1
    )
        return false;
    float minimum_x = triangle.vertices[0].screen_x;
    float maximum_x = minimum_x;
    float minimum_y = triangle.vertices[0].screen_y;
    float maximum_y = minimum_y;
    for (int index = 1; index < 3; ++index) {
        minimum_x = std::min(
            minimum_x, triangle.vertices[index].screen_x);
        maximum_x = std::max(
            maximum_x, triangle.vertices[index].screen_x);
        minimum_y = std::min(
            minimum_y, triangle.vertices[index].screen_y);
        maximum_y = std::max(
            maximum_y, triangle.vertices[index].screen_y);
    }
    return
        maximum_x >= header.display_x &&
        maximum_y >= header.display_y &&
        minimum_x <= display_x1 &&
        minimum_y <= display_y1;
}

bool valid_ps1_screen_polygon_span(
    const WorldCaptureTriangle& triangle
) {
    float minimum_x = triangle.vertices[0].screen_x;
    float maximum_x = minimum_x;
    float minimum_y = triangle.vertices[0].screen_y;
    float maximum_y = minimum_y;
    for (int index = 1; index < 3; ++index) {
        minimum_x = std::min(
            minimum_x, triangle.vertices[index].screen_x);
        maximum_x = std::max(
            maximum_x, triangle.vertices[index].screen_x);
        minimum_y = std::min(
            minimum_y, triangle.vertices[index].screen_y);
        maximum_y = std::max(
            maximum_y, triangle.vertices[index].screen_y);
    }
    // The PS1 rejects polygons whose projected extent exceeds the GPU's
    // 1023x511 validity limits. The compatibility rasterizers enforce the
    // same rule before drawing. Apply it to explicit screen primitives in
    // the native path as well: otherwise an offscreen billboard crossing the
    // signed 11-bit coordinate boundary can wrap one vertex from -1025 to
    // +1023 and become a full-screen streak.
    return
        maximum_x - minimum_x <= 1023.0F &&
        maximum_y - minimum_y <= 511.0F;
}

std::uint32_t material_index(
    const WorldCaptureTriangle& triangle,
    bool screen_space,
    std::vector<WorldMaterial>* materials
) {
    const WorldMaterial material{
        triangle.primitive_flags |
            (screen_space
                ? world_primitive_screen_space_flag
                : 0U),
        triangle.texture_page,
        triangle.clut,
        triangle.texture_mask_x,
        triangle.texture_mask_y,
        triangle.texture_offset_x,
        triangle.texture_offset_y,
        triangle.environment_flags,
    };
    for (std::size_t index = 0; index < materials->size(); ++index) {
        if (same_material((*materials)[index], material))
            return static_cast<std::uint32_t>(index);
    }
    materials->push_back(material);
    return static_cast<std::uint32_t>(materials->size() - 1);
}

void normal(WorldDrawCommand* command) {
    const auto& a = command->vertices[0];
    const auto& b = command->vertices[1];
    const auto& c = command->vertices[2];
    const float ab_x = b.world_x - a.world_x;
    const float ab_y = b.world_y - a.world_y;
    const float ab_z = b.world_z - a.world_z;
    const float ac_x = c.world_x - a.world_x;
    const float ac_y = c.world_y - a.world_y;
    const float ac_z = c.world_z - a.world_z;
    float x = ab_y * ac_z - ab_z * ac_y;
    float y = ab_z * ac_x - ab_x * ac_z;
    float z = ab_x * ac_y - ab_y * ac_x;
    const float length = std::sqrt(x * x + y * y + z * z);
    if (length > 0.000001F) {
        x /= length;
        y /= length;
        z /= length;
    } else {
        x = y = 0.0F;
        z = 1.0F;
    }
    command->face_normal_x = x;
    command->face_normal_y = y;
    command->face_normal_z = z;
}

} // namespace

Ps1ProjectedPoint project_ps1_vertex(
    const WorldCaptureVertex& vertex,
    std::int16_t draw_offset_x,
    std::int16_t draw_offset_y
) noexcept {
    const std::uint16_t depth = static_cast<std::uint16_t>(
        std::clamp(vertex.view_z, 0, 0xFFFF));
    const std::uint32_t quotient =
        divide(vertex.projection_plane, depth);
    const std::int32_t ir1 =
        std::clamp(vertex.view_x, -0x8000, 0x7FFF);
    const std::int32_t ir2 =
        std::clamp(vertex.view_y, -0x8000, 0x7FFF);
    const std::int64_t x =
        static_cast<std::int64_t>(quotient) * ir1 +
        vertex.projection_offset_x;
    const std::int64_t y =
        static_cast<std::int64_t>(quotient) * ir2 +
        vertex.projection_offset_y;
    return Ps1ProjectedPoint{
        std::clamp(
            static_cast<std::int32_t>(x >> 16),
            -0x400,
            0x3FF) + draw_offset_x,
        std::clamp(
            static_cast<std::int32_t>(y >> 16),
            -0x400,
            0x3FF) + draw_offset_y,
        depth,
    };
}

WorldDrawListResult build_world_draw_list(
    const WorldCaptureHeader& header,
    const WorldCaptureTriangle* triangles,
    std::size_t triangle_count,
    WorldDrawListOptions options,
    WorldDrawList* output
) noexcept {
    if (triangles == nullptr || output == nullptr)
        return WorldDrawListResult::invalid_argument;
    if (
        header.display_width <= 0 ||
        header.display_height <= 0 ||
        header.projection_plane == 0 ||
        header.camera_transform_id == 0
    )
        return WorldDrawListResult::invalid_camera;

    try {
        WorldDrawList result{};
        result.display_x = header.display_x;
        result.display_y = header.display_y;
        result.display_width = header.display_width;
        result.display_height = header.display_height;
        result.camera_transform_id = header.camera_transform_id;
        result.continuous_projection = options.continuous_projection;
        result.materials.reserve(256);
        result.commands.reserve(triangle_count);

        for (std::size_t index = 0; index < triangle_count; ++index) {
            const auto& triangle = triangles[index];
            bool complete = true;
            for (const auto& vertex : triangle.vertices)
                complete = complete && vertex.world_valid;
            const bool is_main =
                complete && main_projection(header, triangle);
            const bool screen_space =
                options.include_screen_space && !complete;
            if (
                screen_space &&
                !valid_ps1_screen_polygon_span(triangle)
            ) {
                ++result.rejected_oversized_screen_commands;
                continue;
            }
            if (
                screen_space &&
                !displayed_screen_target(header, triangle)
            ) {
                ++result.rejected_screen_target;
                if (triangle.object_kind == 1U)
                    ++result.rejected_screen_target_track;
                continue;
            }
            if (!complete) {
                ++result.rejected_incomplete;
                if (triangle.object_kind == 1U)
                    ++result.rejected_incomplete_track;
                else if (triangle.object_kind == 2U)
                    ++result.rejected_incomplete_vehicle;
            }
            if (!complete && !screen_space)
                continue;
            if (complete && !is_main) {
                ++result.secondary_commands;
                if (!options.include_secondary_views)
                    continue;
            }

            WorldDrawCommand command{};
            command.material_index =
                material_index(
                    triangle,
                    screen_space,
                    &result.materials);
            command.ordering_table_index =
                triangle.ordering_table_index;
            command.clip_x0 = triangle.clip_x0;
            command.clip_y0 = triangle.clip_y0;
            command.clip_x1 = triangle.clip_x1;
            command.clip_y1 = triangle.clip_y1;
            const bool screen_space_track =
                screen_space && triangle.object_kind == 1U;
            command.object_kind = screen_space_track
                ? triangle.object_kind
                : screen_space ? 0U : triangle.object_kind;
            command.object_id = screen_space_track
                ? triangle.object_id
                : screen_space ? 0U : triangle.object_id;
            command.model_pointer = screen_space_track
                ? triangle.model_pointer
                : screen_space ? 0U : triangle.model_pointer;
            command.transform_id = triangle.transform_id;
            for (int component = 0; component < 9; ++component)
                command.transform_rotation[component] =
                    triangle.transform_rotation[component];
            for (int component = 0; component < 3; ++component)
                command.transform_translation[component] =
                    triangle.transform_translation[component];
            command.exact_transform_valid =
                !screen_space && triangle.exact_transform_valid;
            command.source_command_index =
                static_cast<std::uint32_t>(index);
            // displayed_screen_target() already proved that an explicit 2D
            // primitive belongs to the active display. It cannot satisfy the
            // 3D-only main_projection() predicate because it deliberately has
            // no world provenance, so treating it as a secondary camera put
            // every HUD command on the centred 4:3 fallback path.
            command.channel = screen_space || is_main
                ? WorldViewChannel::main_view
                : WorldViewChannel::secondary_view;

            for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
                const auto& source = triangle.vertices[vertex_index];
                auto& destination = command.vertices[vertex_index];
                const Ps1ProjectedPoint projected = screen_space
                    ? Ps1ProjectedPoint{
                        static_cast<std::int32_t>(
                            std::lround(source.screen_x)),
                        static_cast<std::int32_t>(
                            std::lround(source.screen_y)),
                        1U}
                    : project_ps1_vertex(
                        source,
                        triangle.draw_offset_x,
                        triangle.draw_offset_y);
                float projected_x = static_cast<float>(projected.x);
                float projected_y = static_cast<float>(projected.y);
                if (
                    !screen_space &&
                    options.continuous_projection &&
                    source.view_z > 0
                ) {
                    // Enhanced rendering starts from the exact captured GTE
                    // view coordinates, not the PS1's integer SXY result.
                    // Adjacent sections can deliberately use different
                    // fixed-point transform scales; their ratios describe
                    // the same authored boundary, but independent integer
                    // projection can round its copies to neighboring pixels.
                    // Keeping the division continuous removes that engine
                    // quantization without adding geometry or expanding a
                    // triangle in screen space.
                    constexpr float fixed_scale = 1.0F / 65536.0F;
                    const float inverse_depth =
                        1.0F / static_cast<float>(source.view_z);
                    projected_x =
                        triangle.draw_offset_x +
                        source.projection_offset_x * fixed_scale +
                        source.projection_plane *
                            static_cast<float>(source.view_x) *
                            inverse_depth;
                    projected_y =
                        triangle.draw_offset_y +
                        source.projection_offset_y * fixed_scale +
                        source.projection_plane *
                            static_cast<float>(source.view_y) *
                            inverse_depth;
                }
                const float ndc_x =
                    ((projected_x - header.display_x) /
                        static_cast<float>(header.display_width)) *
                        2.0F - 1.0F;
                const float ndc_y =
                    1.0F -
                    ((projected_y - header.display_y) /
                        static_cast<float>(header.display_height)) *
                        2.0F;
                const float clip_w = screen_space
                    ? 1.0F
                    : std::max(
                        1.0F,
                        static_cast<float>(source.view_z));
                constexpr float near_plane = 16.0F;
                constexpr float far_plane = 1048576.0F;
                constexpr float depth_a =
                    far_plane / (far_plane - near_plane);
                constexpr float depth_b =
                    -near_plane * far_plane /
                    (far_plane - near_plane);
                destination.world_x = source.world_x;
                destination.world_y = source.world_y;
                destination.world_z = source.world_z;
                destination.view_x = static_cast<float>(source.view_x);
                destination.view_y = static_cast<float>(source.view_y);
                destination.view_z = static_cast<float>(source.view_z);
                destination.clip_x = ndc_x * clip_w;
                destination.clip_y = ndc_y * clip_w;
                destination.clip_z = screen_space
                    ? 0.5F
                    : depth_a * clip_w + depth_b;
                destination.clip_w = clip_w;
                destination.screen_x = projected_x;
                destination.screen_y = projected_y;
                destination.projection_offset_x =
                    static_cast<float>(source.projection_offset_x);
                destination.projection_offset_y =
                    static_cast<float>(source.projection_offset_y);
                destination.projection_plane =
                    static_cast<float>(source.projection_plane);
                destination.draw_offset_x =
                    static_cast<float>(triangle.draw_offset_x);
                destination.draw_offset_y =
                    static_cast<float>(triangle.draw_offset_y);
                destination.u = static_cast<float>(source.u);
                destination.v = static_cast<float>(source.v);
                destination.r = source.r;
                destination.g = source.g;
                destination.b = source.b;
                destination.model_x = source.model_x;
                destination.model_y = source.model_y;
                destination.model_z = source.model_z;
                destination.provenance_flags =
                    (source.source_vertex_identity != 0
                        ? world_vertex_source_identity_flag
                        : 0U) |
                    (source.screen_offset_anchor
                        ? world_vertex_screen_offset_anchor_flag
                        : 0U);
                destination.source_vertex_identity =
                    source.source_vertex_identity;
                destination.exact_view_x = source.view_x;
                destination.exact_view_y = source.view_y;
                destination.exact_view_z = source.view_z;
                destination.transform_id = source.transform_id;
                for (int component = 0; component < 9; ++component) {
                    destination.transform_rotation[component] =
                        source.transform_rotation[component];
                }
                for (int component = 0; component < 3; ++component) {
                    destination.transform_translation[component] =
                        source.transform_translation[component];
                }
                destination.exact_transform_valid =
                    source.exact_transform_valid;
                destination.authored_screen_x = projected.x;
                destination.authored_screen_y = projected.y;
            }
            normal(&command);
            result.commands.push_back(command);
            if (command.object_kind == 1)
                ++result.track_commands;
            else if (command.object_kind == 2)
                ++result.vehicle_commands;
            else
                ++result.unclassified_commands;
        }
        *output = std::move(result);
        return WorldDrawListResult::success;
    } catch (const std::bad_alloc&) {
        return WorldDrawListResult::allocation_failed;
    }
}

const char* world_draw_list_result_name(
    WorldDrawListResult result
) noexcept {
    switch (result) {
        case WorldDrawListResult::success: return "success";
        case WorldDrawListResult::invalid_argument:
            return "invalid_argument";
        case WorldDrawListResult::invalid_camera:
            return "invalid_camera";
        case WorldDrawListResult::allocation_failed:
            return "allocation_failed";
    }
    return "unknown";
}

} // namespace opengt::render
