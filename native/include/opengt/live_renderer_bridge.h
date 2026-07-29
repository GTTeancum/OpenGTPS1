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
    uint64_t render_microseconds;
    uint64_t frame_index;
    int32_t input_poll;
    uint32_t reserved;
} opengt_live_stats;

OPENGT_LIVE_EXPORT void* opengt_live_create(void);

OPENGT_LIVE_EXPORT void opengt_live_destroy(void* handle);

OPENGT_LIVE_EXPORT int32_t opengt_live_render(
    void* handle,
    const uint8_t* capture_bytes,
    size_t capture_size,
    uint8_t* output_rgba,
    size_t output_capacity,
    const opengt_live_options* options,
    opengt_live_stats* stats);

OPENGT_LIVE_EXPORT uint32_t opengt_live_api_version(void);

#ifdef __cplusplus
}
#endif
