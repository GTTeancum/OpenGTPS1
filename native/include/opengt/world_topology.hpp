#pragma once

#include "opengt/world_draw_list.hpp"

#include <cstdint>

namespace opengt::render {

// This pass is deliberately source-topology driven. Every operation requires
// a non-zero authored vertex identity and exact integer GTE view coordinates.
// There is no distance tolerance, screen-space expansion, or nearest-neighbor
// search in this API-neutral C++17 layer.
struct WorldTopologyOptions {
    bool join_authored_boundaries;
    bool split_exact_t_junctions;
    bool deterministic_coplanar_ownership;
};

struct WorldTopologyStats {
    std::uint32_t input_commands;
    std::uint32_t output_commands;
    std::uint32_t eligible_track_commands;
    std::uint32_t skipped_without_provenance;
    std::uint32_t unique_source_vertices;
    std::uint32_t exact_position_groups;
    std::uint32_t authored_boundary_groups;
    std::uint32_t adjusted_vertex_instances;
    std::uint32_t boundary_edges;
    std::uint32_t manifold_edges;
    std::uint32_t nonmanifold_edges;
    std::uint32_t exact_t_junctions;
    std::uint32_t split_source_triangles;
    std::uint32_t emitted_split_triangles;
    std::uint32_t coplanar_overlap_pairs;
    std::uint32_t material_overlap_pairs;
    std::uint32_t exact_duplicate_pairs;
    std::uint32_t ownership_components;
    std::uint32_t ownership_reorders;
};

enum class WorldTopologyResult {
    success,
    invalid_argument,
    allocation_failed,
};

WorldTopologyResult apply_world_topology(
    WorldDrawList* draw_list,
    WorldTopologyOptions options,
    WorldTopologyStats* stats
) noexcept;

const char* world_topology_result_name(
    WorldTopologyResult result
) noexcept;

} // namespace opengt::render
