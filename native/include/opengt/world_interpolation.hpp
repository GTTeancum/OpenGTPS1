#pragma once

#include "opengt/world_draw_list.hpp"

#include <cstdint>

namespace opengt::render {

struct WorldInterpolationStats {
    std::uint32_t previous_commands;
    std::uint32_t current_commands;
    std::uint32_t eligible_world_commands;
    std::uint32_t matched_commands;
    std::uint32_t matched_track_commands;
    std::uint32_t matched_vehicle_commands;
    std::uint32_t held_screen_commands;
    std::uint32_t held_unmatched_commands;
    std::uint32_t previous_transform_groups;
    std::uint32_t current_transform_groups;
    std::uint32_t matched_transform_groups;
    std::uint32_t exact_rigid_transform_groups;
    std::uint32_t incoherent_exact_transform_groups;
    std::uint32_t held_incoherent_vehicle_commands;
    // Vehicle commands restored because a sibling group in the same car
    // could not advance coherently.
    std::uint32_t held_atomic_vehicle_commands;
    std::uint32_t held_track_visibility_commands;
    std::uint32_t held_unsafe_track_commands;
    std::uint32_t vertex_exact_track_groups;
    std::uint32_t unavailable_vertex_exact_track_groups;
    std::uint32_t ambiguous_track_vertex_mappings;
    std::uint32_t missing_track_vertex_candidates;
    std::uint64_t first_missing_track_transform_id;
    std::uint32_t first_missing_track_source_identity;
    std::uint32_t held_unsafe_track_nonfinite_commands;
    std::uint32_t held_unsafe_track_near_depth_commands;
    std::uint32_t held_unsafe_track_shift_commands;
    std::uint32_t held_unsafe_track_span_commands;
    std::uint32_t first_unsafe_track_command;
    float first_unsafe_track_previous_span_x;
    float first_unsafe_track_previous_span_y;
    float first_unsafe_track_midpoint_span_x;
    float first_unsafe_track_midpoint_span_y;
    float first_unsafe_track_maximum_shift;
    float first_unsafe_track_minimum_depth;
    std::uint32_t previous_group_cache_hit;
    std::uint32_t current_group_cache_hit;
    // Proven authored joins carried through the synthetic midpoint so track
    // sectors cannot advance their shared endpoints independently.
    std::uint32_t temporal_track_weld_groups;
    std::uint32_t adjusted_temporal_track_weld_vertices;
    std::uint32_t temporal_track_seam_triangles;
};

enum class WorldInterpolationResult {
    success,
    invalid_argument,
    incompatible_viewport,
    allocation_failed,
};

// Retains the transform/vertex index for one immutable draw list. Moving the
// draw list preserves this cache because std::vector move ownership also
// preserves its command storage. Call reset before modifying a cached list in
// place.
class WorldInterpolationCache {
public:
    WorldInterpolationCache() noexcept = default;
    ~WorldInterpolationCache();

    WorldInterpolationCache(const WorldInterpolationCache&) = delete;
    WorldInterpolationCache& operator=(
        const WorldInterpolationCache&) = delete;
    WorldInterpolationCache(WorldInterpolationCache&& other) noexcept;
    WorldInterpolationCache& operator=(
        WorldInterpolationCache&& other) noexcept;

    void reset() noexcept;

private:
    void* storage_{};

    friend WorldInterpolationResult interpolate_world_draw_lists_cached(
        const WorldDrawList& previous,
        const WorldDrawList& current,
        WorldInterpolationCache* previous_cache,
        WorldInterpolationCache* current_cache,
        float alpha,
        WorldDrawList* output,
        WorldInterpolationStats* stats
    ) noexcept;
};

// Builds the temporal sample between two complete authored states. The
// previous state supplies command membership, materials, HUD, and visibility;
// only provenance-matched world geometry moves. This intentionally prevents a
// disappearing object from becoming translucent and prevents two independent
// car silhouettes from being blended into one presentation frame.
WorldInterpolationResult interpolate_world_draw_lists(
    const WorldDrawList& previous,
    const WorldDrawList& current,
    float alpha,
    WorldDrawList* output,
    WorldInterpolationStats* stats
) noexcept;

WorldInterpolationResult interpolate_world_draw_lists_cached(
    const WorldDrawList& previous,
    const WorldDrawList& current,
    WorldInterpolationCache* previous_cache,
    WorldInterpolationCache* current_cache,
    float alpha,
    WorldDrawList* output,
    WorldInterpolationStats* stats
) noexcept;

const char* world_interpolation_result_name(
    WorldInterpolationResult result
) noexcept;

} // namespace opengt::render
