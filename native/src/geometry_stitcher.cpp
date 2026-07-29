#include "opengt/geometry_stitcher.hpp"

#include <algorithm>
#include <cmath>
#include <limits>

namespace opengt::render {
namespace {

bool anchor_less(const BoundaryAnchor& left, const BoundaryAnchor& right) {
    if (left.stitch_group != right.stitch_group) {
        return left.stitch_group < right.stitch_group;
    }
    if (left.priority != right.priority) {
        return left.priority < right.priority;
    }
    return left.vertex_index < right.vertex_index;
}

float distance_squared(const Vec3& left, const Vec3& right) {
    const float dx = left.x - right.x;
    const float dy = left.y - right.y;
    const float dz = left.z - right.z;
    return dx * dx + dy * dy + dz * dz;
}

bool same_position(const Vec3& left, const Vec3& right) {
    return left.x == right.x && left.y == right.y && left.z == right.z;
}

} // namespace

StitchStats stitch_mesh_boundaries(
    RenderVertex* vertices,
    std::size_t vertex_count,
    const BoundaryAnchor* anchors,
    std::size_t anchor_count,
    StitchWorkspace workspace,
    StitchOptions options
) noexcept {
    StitchStats stats{};
    stats.input_anchor_count = static_cast<std::uint32_t>(
        std::min(anchor_count, static_cast<std::size_t>(
            std::numeric_limits<std::uint32_t>::max()
        ))
    );

    if (
        vertices == nullptr ||
        anchors == nullptr ||
        vertex_count == 0 ||
        anchor_count == 0 ||
        workspace.anchor_scratch == nullptr ||
        workspace.vertex_index_scratch == nullptr ||
        !std::isfinite(options.maximum_anchor_distance) ||
        options.maximum_anchor_distance < 0.0F
    ) {
        return stats;
    }

    if (workspace.capacity < anchor_count) {
        stats.insufficient_workspace = true;
        return stats;
    }

    std::size_t valid_anchor_count = 0;
    for (std::size_t index = 0; index < anchor_count; ++index) {
        const BoundaryAnchor anchor = anchors[index];
        if (
            anchor.stitch_group == 0 ||
            anchor.vertex_index >= vertex_count
        ) {
            ++stats.invalid_anchor_count;
            continue;
        }
        workspace.anchor_scratch[valid_anchor_count++] = anchor;
    }
    std::sort(
        workspace.anchor_scratch,
        workspace.anchor_scratch + valid_anchor_count,
        anchor_less
    );

    const float maximum_distance_squared =
        options.maximum_anchor_distance * options.maximum_anchor_distance;

    std::size_t group_begin = 0;
    while (group_begin < valid_anchor_count) {
        std::size_t group_end = group_begin + 1;
        while (
            group_end < valid_anchor_count &&
            workspace.anchor_scratch[group_end].stitch_group ==
                workspace.anchor_scratch[group_begin].stitch_group
        ) {
            ++group_end;
        }

        // Duplicate anchor records do not turn a single vertex into a seam.
        std::size_t unique_vertex_count = 0;
        for (std::size_t index = group_begin; index < group_end; ++index) {
            const std::uint32_t vertex_index =
                workspace.anchor_scratch[index].vertex_index;
            const auto first = workspace.vertex_index_scratch;
            const auto last = first + unique_vertex_count;
            if (std::find(first, last, vertex_index) == last) {
                workspace.vertex_index_scratch[unique_vertex_count++] =
                    vertex_index;
            }
        }

        if (unique_vertex_count >= 2) {
            const std::uint32_t canonical_index =
                workspace.anchor_scratch[group_begin].vertex_index;
            const Vec3 canonical_position =
                vertices[canonical_index].position;

            bool valid_group = true;
            for (
                std::size_t index = 0;
                index < unique_vertex_count;
                ++index
            ) {
                const std::uint32_t vertex_index =
                    workspace.vertex_index_scratch[index];
                const Vec3 position = vertices[vertex_index].position;
                if (
                    !std::isfinite(position.x) ||
                    !std::isfinite(position.y) ||
                    !std::isfinite(position.z) ||
                    distance_squared(position, canonical_position) >
                        maximum_distance_squared
                ) {
                    valid_group = false;
                    break;
                }
            }

            if (!valid_group) {
                ++stats.rejected_group_count;
            } else {
                ++stats.stitched_group_count;
                for (
                    std::size_t index = 0;
                    index < unique_vertex_count;
                    ++index
                ) {
                    const std::uint32_t vertex_index =
                        workspace.vertex_index_scratch[index];
                    Vec3& position = vertices[vertex_index].position;
                    if (!same_position(position, canonical_position)) {
                        position = canonical_position;
                        ++stats.adjusted_vertex_count;
                    }
                }
            }
        }

        group_begin = group_end;
    }

    return stats;
}

} // namespace opengt::render
