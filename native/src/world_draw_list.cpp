#include "opengt/world_draw_list.hpp"

#include <algorithm>
#include <cmath>
#include <cstdio>
#include <limits>
#include <unordered_map>
#include <unordered_set>
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

struct WorldMaterialHash {
    std::size_t operator()(const WorldMaterial& material) const noexcept {
        std::size_t hash = static_cast<std::size_t>(
            0x9E3779B97F4A7C15ULL);
        const auto mix = [&hash] (std::uint64_t value) {
            hash ^= static_cast<std::size_t>(
                value + 0x9E3779B97F4A7C15ULL +
                (static_cast<std::uint64_t>(hash) << 6U) +
                (static_cast<std::uint64_t>(hash) >> 2U));
        };
        mix(material.primitive_flags);
        mix(material.texture_page);
        mix(material.clut);
        mix(static_cast<std::uint16_t>(material.texture_mask_x));
        mix(static_cast<std::uint16_t>(material.texture_mask_y));
        mix(static_cast<std::uint16_t>(material.texture_offset_x));
        mix(static_cast<std::uint16_t>(material.texture_offset_y));
        mix(material.environment_flags);
        return hash;
    }
};

struct WorldMaterialEqual {
    bool operator()(
        const WorldMaterial& left,
        const WorldMaterial& right
    ) const noexcept {
        return same_material(left, right);
    }
};

using WorldMaterialIndices = std::unordered_map<
    WorldMaterial,
    std::uint32_t,
    WorldMaterialHash,
    WorldMaterialEqual>;

bool main_projection(
    const WorldCaptureHeader& header,
    const WorldCaptureTriangle& triangle
) {
    if ((triangle.primitive_flags &
            world_primitive_secondary_view_flag) != 0)
        return false;
    if (
        triangle.draw_offset_x != header.draw_offset_x ||
        triangle.draw_offset_y != header.draw_offset_y
    )
        return false;
    for (const auto& vertex : triangle.vertices) {
        // GTE H is a per-submission lens value, not a view/depth-space
        // identity. Seattle's replay camera changes H while building a frame:
        // the resident course receives the new value and vehicles retain the
        // prior value. Both still use the same projection centre, drawing
        // target, and view-space Z. Splitting them into separate channels
        // clears coherent depth and lets the later road overwrite cars.
        // Preserve each primitive's authored H for projection, but identify
        // an independent view only by its projection centre or drawing target.
        if (
            vertex.projection_offset_x != header.projection_offset_x ||
            vertex.projection_offset_y != header.projection_offset_y
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
    std::vector<WorldMaterial>* materials,
    WorldMaterialIndices* indices
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
    const auto found = indices->find(material);
    if (found != indices->end())
        return found->second;
    materials->push_back(material);
    const std::uint32_t index = static_cast<std::uint32_t>(
        materials->size() - 1);
    indices->emplace(material, index);
    return index;
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

struct ContinuousViewPoint {
    double x;
    double y;
    double z;
};

ContinuousViewPoint continuous_view_point(
    const WorldCaptureVertex& vertex
) noexcept {
    if (!vertex.exact_transform_valid) {
        return ContinuousViewPoint{
            static_cast<double>(vertex.view_x),
            static_cast<double>(vertex.view_y),
            static_cast<double>(vertex.view_z),
        };
    }

    constexpr double fixed_scale = 1.0 / 4096.0;
    const double model[3]{
        static_cast<double>(vertex.model_x),
        static_cast<double>(vertex.model_y),
        static_cast<double>(vertex.model_z),
    };
    ContinuousViewPoint result{};
    double* components[3]{&result.x, &result.y, &result.z};
    for (int view_axis = 0; view_axis < 3; ++view_axis) {
        double value = vertex.transform_translation[view_axis];
        for (int model_axis = 0; model_axis < 3; ++model_axis) {
            value +=
                vertex.transform_rotation[view_axis * 3 + model_axis] *
                model[model_axis] * fixed_scale;
        }
        *components[view_axis] = value;
    }
    return result;
}

ContinuousProjectedPoint project_continuous_view(
    const WorldCaptureVertex& vertex,
    const ContinuousViewPoint& view,
    std::int16_t draw_offset_x,
    std::int16_t draw_offset_y
) noexcept {
    constexpr double projection_fixed_scale = 1.0 / 65536.0;
    const double center_x =
        draw_offset_x +
        vertex.projection_offset_x * projection_fixed_scale;
    const double center_y =
        draw_offset_y +
        vertex.projection_offset_y * projection_fixed_scale;
    if (view.z == 0.0) {
        return ContinuousProjectedPoint{
            static_cast<float>(center_x),
            static_cast<float>(center_y),
        };
    }
    const double scale = vertex.projection_plane / view.z;
    return ContinuousProjectedPoint{
        static_cast<float>(center_x + view.x * scale),
        static_cast<float>(center_y + view.y * scale),
    };
}

bool valid_authored_micro_seam_target(const WorldDrawVertex& vertex) noexcept {
    // Saturated GTE division, IR or SXY values are not geometric projections.
    // In particular, rebuilding X/Y from a behind-camera SXY reverses the
    // homogeneous ray and can bend a road triangle across the sky. Preserve
    // continuous clip coordinates until the GPU clips these vertices.
    return
        vertex.exact_view_z > vertex.projection_plane * 0.5F &&
        vertex.exact_view_z <= 0xFFFF &&
        vertex.exact_view_x >= -0x8000 && vertex.exact_view_x <= 0x7FFF &&
        vertex.exact_view_y >= -0x8000 && vertex.exact_view_y <= 0x7FFF &&
        vertex.authored_screen_x > -0x400 + vertex.draw_offset_x &&
        vertex.authored_screen_x < 0x3FF + vertex.draw_offset_x &&
        vertex.authored_screen_y > -0x400 + vertex.draw_offset_y &&
        vertex.authored_screen_y < 0x3FF + vertex.draw_offset_y;
}

bool collapsed_authored_micro_edge(
    const WorldDrawVertex& a,
    const WorldDrawVertex& b
) noexcept {
    if (
        !valid_authored_micro_seam_target(a) ||
        !valid_authored_micro_seam_target(b) ||
        a.authored_screen_x != b.authored_screen_x ||
        a.authored_screen_y != b.authored_screen_y
    )
        return false;
    const std::int64_t dx =
        static_cast<std::int64_t>(a.exact_view_x) - b.exact_view_x;
    const std::int64_t dy =
        static_cast<std::int64_t>(a.exact_view_y) - b.exact_view_y;
    const std::int64_t dz =
        static_cast<std::int64_t>(a.exact_view_z) - b.exact_view_z;
    return
        dx >= -1 && dx <= 1 &&
        dy >= -1 && dy <= 1 &&
        dz >= -1 && dz <= 1;
}

struct AuthoredMicroSeamVertexKey {
    std::uint32_t object_kind;
    std::uint32_t object_id;
    std::uint32_t model_pointer;
    std::uint64_t transform_id;
    std::int64_t view_x;
    std::int64_t view_y;
    std::int64_t view_z;
    std::int32_t projection_x;
    std::int32_t projection_y;
    std::int32_t projection_plane;
    std::int32_t draw_x;
    std::int32_t draw_y;
    WorldViewChannel channel;
    bool fixed_position;

    bool operator==(
        const AuthoredMicroSeamVertexKey& other
    ) const noexcept {
        return
            object_kind == other.object_kind &&
            object_id == other.object_id &&
            model_pointer == other.model_pointer &&
            transform_id == other.transform_id &&
            view_x == other.view_x &&
            view_y == other.view_y &&
            view_z == other.view_z &&
            projection_x == other.projection_x &&
            projection_y == other.projection_y &&
            projection_plane == other.projection_plane &&
            draw_x == other.draw_x && draw_y == other.draw_y &&
            channel == other.channel && fixed_position == other.fixed_position;
    }
};

struct AuthoredMicroSeamVertexKeyHash {
    std::size_t operator()(
        const AuthoredMicroSeamVertexKey& key
    ) const noexcept {
        std::size_t hash = 0x9E3779B97F4A7C15ULL;
        const auto mix = [&hash] (std::uint64_t value) {
            hash ^= static_cast<std::size_t>(
                value + 0x9E3779B97F4A7C15ULL +
                (static_cast<std::uint64_t>(hash) << 6U) +
                (static_cast<std::uint64_t>(hash) >> 2U));
        };
        mix(key.object_kind);
        mix(key.object_id);
        mix(key.model_pointer);
        mix(key.transform_id);
        mix(static_cast<std::uint64_t>(key.view_x));
        mix(static_cast<std::uint64_t>(key.view_y));
        mix(static_cast<std::uint64_t>(key.view_z));
        mix(static_cast<std::uint32_t>(key.projection_x));
        mix(static_cast<std::uint32_t>(key.projection_y));
        mix(static_cast<std::uint32_t>(key.projection_plane));
        mix(static_cast<std::uint32_t>(key.draw_x));
        mix(static_cast<std::uint32_t>(key.draw_y));
        mix(static_cast<unsigned>(key.channel));
        mix(key.fixed_position);
        return hash;
    }
};

AuthoredMicroSeamVertexKey authored_micro_seam_key(
    const WorldDrawCommand& command,
    const WorldDrawVertex& vertex
) noexcept {
    AuthoredMicroSeamVertexKey key{
        command.object_kind,
        command.object_id,
        command.model_pointer,
        command.transform_id,
        vertex.exact_view_x,
        vertex.exact_view_y,
        vertex.exact_view_z,
        static_cast<std::int32_t>(vertex.projection_offset_x),
        static_cast<std::int32_t>(vertex.projection_offset_y),
        static_cast<std::int32_t>(vertex.projection_plane),
        static_cast<std::int32_t>(vertex.draw_offset_x),
        static_cast<std::int32_t>(vertex.draw_offset_y),
        command.channel,
        false,
    };
    if (command.object_kind == 1 && vertex.exact_transform_valid &&
        vertex.transform_id != 0) {
        // Course chunks duplicate shared endpoints under different object and
        // model IDs. Snapping only one copy tears an otherwise exact edge.
        // Share the existing micro-seam adjustment only at identical fixed-
        // point positions, transforms and projections, never nearby vertices
        // which merely round to the same integer GTE coordinate.
        key.object_id = 0;
        key.model_pointer = 0;
        key.transform_id = vertex.transform_id;
        key.fixed_position = true;
        const std::int64_t model[3]{
            vertex.model_x, vertex.model_y, vertex.model_z};
        std::int64_t* coordinates[3]{&key.view_x, &key.view_y, &key.view_z};
        for (int row = 0; row < 3; ++row) {
            *coordinates[row] =
                static_cast<std::int64_t>(vertex.transform_translation[row]) * 4096;
            for (int column = 0; column < 3; ++column)
                *coordinates[row] +=
                    vertex.transform_rotation[row * 3 + column] * model[column];
        }
    }
    return key;
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

ContinuousProjectedPoint project_continuous_vertex(
    const WorldCaptureVertex& vertex,
    std::int16_t draw_offset_x,
    std::int16_t draw_offset_y
) noexcept {
    const ContinuousViewPoint view = continuous_view_point(vertex);
    return project_continuous_view(
        vertex,
        view,
        draw_offset_x,
        draw_offset_y);
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
        // The live bridge alternates two draw lists so the preceding authored
        // frame remains available while the current frame is built. Reuse the
        // returned buffers instead of allocating and freeing tens of thousands
        // of commands every 60 Hz tick.
        WorldDrawList result{};
        result.materials = std::move(output->materials);
        result.commands = std::move(output->commands);
        result.materials.clear();
        result.commands.clear();
        result.display_x = header.display_x;
        result.display_y = header.display_y;
        result.display_width = header.display_width;
        result.display_height = header.display_height;
        result.frame_index = header.frame_index;
        result.input_poll = header.input_poll;
        result.camera_transform_id = header.camera_transform_id;
        result.continuous_projection = options.continuous_projection;
        result.materials.reserve(256);
        result.commands.reserve(triangle_count);
        WorldMaterialIndices material_indices;
        material_indices.reserve(256);
        std::unordered_set<
            AuthoredMicroSeamVertexKey,
            AuthoredMicroSeamVertexKeyHash> authored_micro_seam_vertices;
        authored_micro_seam_vertices.reserve(64);

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
                options.require_world_depth_scale && complete &&
                (triangle.object_kind == 1U || triangle.object_kind == 2U) &&
                (!triangle.depth_scale_valid ||
                    triangle.depth_scale_exponent < -16 ||
                    triangle.depth_scale_exponent > 16)
            ) {
                std::fprintf(
                    stderr,
                    "[Native-Depth-Scale-Failure] frame=%llu poll=%d "
                    "triangle=%zu kind=%u object=%08x model=%08x "
                    "flags=%08x valid=%u exponent=%d transform=%016llx "
                    "viewZ=%d/%d/%d\n",
                    static_cast<unsigned long long>(header.frame_index),
                    header.input_poll,
                    index,
                    triangle.object_kind,
                    triangle.object_id,
                    triangle.model_pointer,
                    triangle.primitive_flags,
                    triangle.depth_scale_valid ? 1U : 0U,
                    triangle.depth_scale_exponent,
                    static_cast<unsigned long long>(triangle.transform_id),
                    triangle.vertices[0].view_z,
                    triangle.vertices[1].view_z,
                    triangle.vertices[2].view_z);
                return WorldDrawListResult::invalid_depth_scale;
            }
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
                    &result.materials,
                    &material_indices);
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
                ContinuousViewPoint view{
                    static_cast<double>(source.view_x),
                    static_cast<double>(source.view_y),
                    static_cast<double>(source.view_z),
                };
                if (!screen_space && options.continuous_projection)
                    view = continuous_view_point(source);
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
                    view.z > 0.0
                ) {
                    // Reconstruct camera space from the captured model point
                    // and exact 12-bit GT2 transform before dividing. Using
                    // the GTE's integer IR/SZ outputs here independently
                    // truncated every corner and made otherwise rigid edges
                    // jitter at sub-view-unit motion. PS1 SXY remains above as
                    // the authored oracle; it does not drive modern geometry.
                    const auto continuous = project_continuous_view(
                        source,
                        view,
                        triangle.draw_offset_x,
                        triangle.draw_offset_y);
                    projected_x = continuous.x;
                    projected_y = continuous.y;
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
                const float depth_scale =
                    !screen_space && triangle.depth_scale_valid
                        ? std::ldexp(
                            1.0F,
                            triangle.depth_scale_exponent)
                        : 1.0F;
                const float raw_clip_w = screen_space
                    ? 1.0F
                    : static_cast<float>(view.z);
                // DMA records (head - entry) / 4, a reverse traversal ordinal,
                // not metric Z. All world primitives retain normalized camera
                // depth: model-space corners for resident foliage, and anchor
                // Z for screen-offset light halos. Using ordinal * 8192 put
                // SSR5's near lamp halo behind its distant buildings. Neither
                // the legacy resident-billboard tag nor OT order overrides Z.
                const float clip_w = screen_space
                    ? 1.0F
                    : raw_clip_w * depth_scale;
                constexpr float near_plane = 16.0F;
                destination.world_x = source.world_x;
                destination.world_y = source.world_y;
                destination.world_z = source.world_z;
                destination.view_x = static_cast<float>(view.x);
                destination.view_y = static_cast<float>(view.y);
                destination.view_z = static_cast<float>(view.z);
                if (
                    !screen_space && options.continuous_projection
                ) {
                    // Build homogeneous coordinates directly from GT2's
                    // captured camera-space vertex. This remains linear on
                    // edges that cross the camera or near plane. Projecting
                    // first and clamping W positive made behind-camera course
                    // geometry survive clipping as giant screen-spanning
                    // polygons.
                    constexpr float fixed_scale = 1.0F / 65536.0F;
                    const float projection_center_x =
                        triangle.draw_offset_x +
                        source.projection_offset_x * fixed_scale;
                    const float projection_center_y =
                        triangle.draw_offset_y +
                        source.projection_offset_y * fixed_scale;
                    const float ndc_center_x =
                        ((projection_center_x - header.display_x) /
                            static_cast<float>(header.display_width)) *
                            2.0F - 1.0F;
                    const float ndc_center_y =
                        1.0F -
                        ((projection_center_y - header.display_y) /
                            static_cast<float>(header.display_height)) *
                            2.0F;
                    destination.clip_x = depth_scale * (
                        ndc_center_x * raw_clip_w +
                        2.0F * source.projection_plane *
                            static_cast<float>(view.x) /
                            static_cast<float>(header.display_width));
                    destination.clip_y = depth_scale * (
                        ndc_center_y * raw_clip_w -
                        2.0F * source.projection_plane *
                            static_cast<float>(view.y) /
                            static_cast<float>(header.display_height));
                } else {
                    destination.clip_x = ndc_x * clip_w;
                    destination.clip_y = ndc_y * clip_w;
                }
                destination.clip_z = screen_space
                    ? 0.5F
                    // Reversed infinite projection: after homogeneous divide,
                    // depth is near/Z.  D3D's 0 <= Zclip <= Wclip rule still
                    // clips exactly at the authored modern near plane, while
                    // a D32_FLOAT surface retains useful precision across the
                    // complete resident course without imposing a far plane.
                    : near_plane;
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
            // Record exact sub-view-unit seams which the authored PS1 SXY
            // collapsed to one point. A later pass welds every occurrence of
            // both endpoints; dropping only the thin joining faces would open
            // a crack between their neighboring surfaces.
            constexpr int edge_vertices[3][2] = {
                {0, 1}, {1, 2}, {2, 0},
            };
            for (const auto& edge : edge_vertices) {
                const auto& a = command.vertices[edge[0]];
                const auto& b = command.vertices[edge[1]];
                if (!collapsed_authored_micro_edge(a, b))
                    continue;
                authored_micro_seam_vertices.insert(
                    authored_micro_seam_key(command, a));
                authored_micro_seam_vertices.insert(
                    authored_micro_seam_key(command, b));
            }
            normal(&command);
            result.commands.push_back(command);
            if (command.object_kind == 1)
                ++result.track_commands;
            else if (command.object_kind == 2)
                ++result.vehicle_commands;
            else if (command.object_kind == 3)
                ++result.background_commands;
            else {
                ++result.unclassified_commands;
                if (!screen_space)
                    ++result.unclassified_world_commands;
            }
        }
        // Continuous projection normally preserves fractional movement and
        // rigid shape. For an authored micro-seam, however, separating the
        // endpoints invents raster area which the PS1 never had. Snap every
        // shared occurrence back to its authored SXY and rebuild homogeneous
        // X/Y at its existing depth. This is topology/provenance based and is
        // independent of track, material, texture, address, or screen region.
        for (auto& command : result.commands) {
            for (auto& vertex : command.vertices) {
                if (
                    !valid_authored_micro_seam_target(vertex) ||
                    authored_micro_seam_vertices.find(
                        authored_micro_seam_key(command, vertex)) ==
                    authored_micro_seam_vertices.end()
                )
                    continue;
                vertex.screen_x =
                    static_cast<float>(vertex.authored_screen_x);
                vertex.screen_y =
                    static_cast<float>(vertex.authored_screen_y);
                const float ndc_x =
                    ((vertex.screen_x - header.display_x) /
                        static_cast<float>(header.display_width)) *
                        2.0F - 1.0F;
                const float ndc_y =
                    1.0F -
                    ((vertex.screen_y - header.display_y) /
                        static_cast<float>(header.display_height)) *
                        2.0F;
                vertex.clip_x = ndc_x * vertex.clip_w;
                vertex.clip_y = ndc_y * vertex.clip_w;
            }
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
        case WorldDrawListResult::invalid_depth_scale:
            return "invalid_depth_scale";
        case WorldDrawListResult::allocation_failed:
            return "allocation_failed";
    }
    return "unknown";
}

} // namespace opengt::render
