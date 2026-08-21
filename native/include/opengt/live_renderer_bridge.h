#pragma once

#include <stddef.h>
#include <stdint.h>

#if defined(_WIN32)
#define OPENGT_LIVE_EXPORT __declspec(dllexport)
#else
#define OPENGT_LIVE_EXPORT
#endif

#ifdef __cplusplus
extern "C" {
#endif

enum {
    OPENGT_LIVE_DEPTH = 1u << 0,
    OPENGT_LIVE_DITHER = 1u << 1,
    OPENGT_LIVE_TOPOLOGY = 1u << 2,
    OPENGT_LIVE_PERSPECTIVE = 1u << 3,
    OPENGT_LIVE_WARP = 1u << 4,
    OPENGT_LIVE_TEXTURE_SMOOTHING = 1u << 5,
    OPENGT_LIVE_REALTIME_READBACK = 1u << 6,
    OPENGT_LIVE_HIGH_RESOLUTION_TEXTURES = 1u << 7,
};

typedef struct opengt_live_options {
    uint32_t struct_size;
    uint32_t flags;
    uint32_t output_scale;
    uint32_t clear_color_rgba8;
} opengt_live_options;

typedef struct opengt_live_stats {
    uint32_t struct_size;
    uint32_t result;
    uint32_t capture_triangles;
    uint32_t output_commands;
    uint32_t draw_calls;
    uint32_t transparent_draw_calls;
    uint32_t topology_boundary_groups;
    uint32_t topology_t_junctions;
    uint32_t topology_split_triangles;
    uint32_t topology_coplanar_pairs;
    uint32_t topology_ownership_reorders;
    uint32_t output_width;
    uint32_t output_height;
    // CPU elapsed time inside render_world_d3d11. The synchronous API waits
    // through completion/readback; live asynchronous submissions do not.
    uint64_t render_microseconds;
    uint64_t frame_index;
    int32_t input_poll;
    uint32_t reserved;
    uint64_t decode_microseconds;
    uint64_t draw_list_microseconds;
    uint64_t topology_microseconds;
    uint64_t pipeline_microseconds;
} opengt_live_stats;

enum {
    // No pixel buffer was produced by this real-time submission yet.  The
    // asynchronous readback queue retains the authored frame and publishes it
    // later with the matching metadata.
    OPENGT_LIVE_STATS_NO_OUTPUT = 1u << 3,
};

typedef struct opengt_live_interpolation_stats {
    uint32_t struct_size;
    uint32_t result;
    uint32_t output_count;
    uint32_t previous_commands;
    uint32_t current_commands;
    uint32_t eligible_world_commands;
    uint32_t matched_commands;
    uint32_t matched_track_commands;
    uint32_t matched_vehicle_commands;
    uint32_t held_screen_commands;
    uint32_t held_unmatched_commands;
    uint32_t previous_transform_groups;
    uint32_t current_transform_groups;
    uint32_t matched_transform_groups;
    uint32_t temporal_reset;
    // Bit 0 records that the midpoint reused exactly compatible prior-frame
    // VRAM/material uploads. Bit 1 records that the current authored draw
    // list contained track commands, so performance telemetry can exclude
    // menus and Results screens from its aligned track-world window.
    uint32_t reserved;
    uint32_t exact_rigid_transform_groups;
    uint32_t incoherent_exact_transform_groups;
    uint32_t held_incoherent_vehicle_commands;
    uint32_t held_track_visibility_commands;
    uint32_t held_unsafe_track_commands;
    uint64_t interpolation_microseconds;
    uint64_t pair_pipeline_microseconds;
    uint64_t midpoint_render_microseconds;
    uint64_t actual_render_microseconds;
    // Topology is built once for the interpolated midpoint. Expose that
    // pair-scoped cost here so live telemetry can keep pipeline, submit, and
    // topology percentile samples aligned.
    uint64_t current_topology_microseconds;
} opengt_live_interpolation_stats;

typedef struct opengt_live_texture_upload {
    uint64_t key;
    int32_t x;
    int32_t y;
    int32_t word_width;
    int32_t height;
} opengt_live_texture_upload;

OPENGT_LIVE_EXPORT void* opengt_live_create(void);

OPENGT_LIVE_EXPORT void opengt_live_destroy(void* handle);

// With OPENGT_LIVE_REALTIME_READBACK, this submits the authored frame to the
// asynchronous image queue.  stats.reserved carries
// OPENGT_LIVE_STATS_NO_OUTPUT while the initial queue is priming; afterward
// the returned pixels and metadata always identify the same authored frame.
OPENGT_LIVE_EXPORT int32_t opengt_live_render(
    void* handle,
    const uint8_t* capture_bytes,
    size_t capture_size,
    uint8_t* output_rgba,
    size_t output_capacity,
    const opengt_live_options* options,
    opengt_live_stats* stats);

// Submits one midpoint/actual pair and drains the oldest completed pair without
// blocking. output_count is one for an immediate temporal reset, zero when no
// prior pair is ready, and two for a completed chronological pair. Pixel data
// and frame metadata always come from the same staging slots. stats.reserved
// bit 0 marks a synthesized midpoint and bit 2 marks a one-output temporal
// reset boundary that may not be reused as the next 60 Hz native image.
OPENGT_LIVE_EXPORT int32_t opengt_live_render_pair(
    void* handle,
    const uint8_t* capture_bytes,
    size_t capture_size,
    uint8_t* output_first_rgba,
    uint8_t* output_second_rgba,
    size_t output_capacity,
    const opengt_live_options* options,
    opengt_live_stats* first_stats,
    opengt_live_stats* second_stats,
    opengt_live_interpolation_stats* interpolation_stats);

// Drains one additional completed midpoint/actual pair without submitting a
// new authored capture. Returns 1 when a pair was written, 0 when the oldest
// pair is still in flight, and a negative result on error.
OPENGT_LIVE_EXPORT int32_t opengt_live_try_read_pair(
    void* handle,
    uint8_t* output_first_rgba,
    uint8_t* output_second_rgba,
    size_t output_capacity,
    opengt_live_stats* first_stats,
    opengt_live_stats* second_stats);

OPENGT_LIVE_EXPORT int32_t opengt_live_set_texture_uploads(
    void* handle,
    int32_t software_adapter,
    const opengt_live_texture_upload* uploads,
    size_t upload_count);

OPENGT_LIVE_EXPORT uint32_t opengt_live_api_version(void);

#ifdef __cplusplus
}
#endif
