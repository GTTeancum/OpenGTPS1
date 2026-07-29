#include "opengt/world_draw_list.hpp"

#include <algorithm>
#include <cmath>
#include <limits>
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

std::uint32_t material_index(
    const WorldCaptureTriangle& triangle,
    std::vector<WorldMaterial>* materials
) {
    const WorldMaterial material{
        triangle.primitive_flags,
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
        result.materials.reserve(256);
        result.commands.reserve(triangle_count);

        for (std::size_t index = 0; index < triangle_count; ++index) {
            const auto& triangle = triangles[index];
            bool complete = true;
            for (const auto& vertex : triangle.vertices)
                complete = complete && vertex.world_valid;
            if (!complete) {
                ++result.rejected_incomplete;
                continue;
            }
            const bool is_main = main_projection(header, triangle);
            if (!is_main) {
                ++result.secondary_commands;
                if (!options.include_secondary_views)
                    continue;
            }

            WorldDrawCommand command{};
            command.material_index =
                material_index(triangle, &result.materials);
            command.ordering_table_index =
                triangle.ordering_table_index;
            command.clip_x0 = triangle.clip_x0;
            command.clip_y0 = triangle.clip_y0;
            command.clip_x1 = triangle.clip_x1;
            command.clip_y1 = triangle.clip_y1;
            command.object_kind = triangle.object_kind;
            command.object_id = triangle.object_id;
            command.model_pointer = triangle.model_pointer;
            command.transform_id = triangle.transform_id;
            command.channel = is_main
                ? WorldViewChannel::main_view
                : WorldViewChannel::secondary_view;

            for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
                const auto& source = triangle.vertices[vertex_index];
                auto& destination = command.vertices[vertex_index];
                const Ps1ProjectedPoint projected = project_ps1_vertex(
                    source,
                    triangle.draw_offset_x,
                    triangle.draw_offset_y);
                const float ndc_x =
                    ((projected.x - header.display_x) /
                        static_cast<float>(header.display_width)) *
                        2.0F - 1.0F;
                const float ndc_y =
                    1.0F -
                    ((projected.y - header.display_y) /
                        static_cast<float>(header.display_height)) *
                        2.0F;
                const float clip_w =
                    std::max(1.0F, static_cast<float>(source.view_z));
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
                destination.clip_z =
                    depth_a * clip_w + depth_b;
                destination.clip_w = clip_w;
                destination.screen_x = static_cast<float>(projected.x);
                destination.screen_y = static_cast<float>(projected.y);
                destination.u = static_cast<float>(source.u);
                destination.v = static_cast<float>(source.v);
                destination.r = source.r;
                destination.g = source.g;
                destination.b = source.b;
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
