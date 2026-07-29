#pragma once

#include "opengt/render_scene.hpp"

#include <cstddef>
#include <cstdint>

namespace opengt::render {

// A stitch group is supplied by the GT2 scene decoder after it has established
// that vertices describe the same authored boundary point. No proximity search
// is performed: nearby but unrelated geometry is never joined heuristically.
struct BoundaryAnchor {
    std::uint32_t vertex_index;
    std::uint32_t stitch_group;
    std::uint16_t priority;
    std::uint16_t reserved;
};

struct StitchOptions {
    // A corruption guard, not a matching tolerance. Explicitly linked anchors
    // farther apart than this are rejected instead of damaging the mesh.
    float maximum_anchor_distance;
};

struct StitchWorkspace {
    BoundaryAnchor* anchor_scratch;
    std::uint32_t* vertex_index_scratch;
    std::size_t capacity;
};

struct StitchStats {
    std::uint32_t input_anchor_count;
    std::uint32_t stitched_group_count;
    std::uint32_t adjusted_vertex_count;
    std::uint32_t rejected_group_count;
    std::uint32_t invalid_anchor_count;
    bool insufficient_workspace;
};

// Resolves explicitly identified model boundaries in world/model space.
// The highest-priority anchor (lowest numeric priority, then lowest vertex
// index) supplies the exact canonical position. UVs, colors, normals,
// materials, collision data, and draw order are untouched.
StitchStats stitch_mesh_boundaries(
    RenderVertex* vertices,
    std::size_t vertex_count,
    const BoundaryAnchor* anchors,
    std::size_t anchor_count,
    StitchWorkspace workspace,
    StitchOptions options
) noexcept;

} // namespace opengt::render
