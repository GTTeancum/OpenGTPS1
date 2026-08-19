#pragma once

#include "opengt/world_draw_list.hpp"

#include <cstdint>

namespace opengt::render {

// This pass is deliberately source-topology driven. Every operation requires
// a non-zero authored vertex identity and exact integer GTE view coordinates.
// Continuous high-resolution projection may additionally close a subpixel
// seam between mutually nearest boundary vertices, but only after exact
// authored evidence proves that the two track sections are adjacent.
struct WorldTopologyOptions {
    bool join_authored_boundaries;
    bool split_exact_t_junctions;
    bool deterministic_coplanar_ownership;
    bool repair_projected_t_junctions = true;
    bool repair_offscreen_projected_t_junctions = true;
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
    std::uint32_t authored_projection_groups;
    std::uint32_t adjusted_projection_instances;
    std::uint32_t projected_seam_groups;
    std::uint32_t adjusted_seam_instances;
    std::uint32_t authored_raster_groups;
    std::uint32_t adjusted_authored_raster_instances;
    std::uint32_t projected_t_junctions;
    std::uint32_t adjusted_projected_t_junction_instances;
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
