#pragma once

#include "opengt/world_capture.hpp"

#include <cstddef>
#include <cstdint>
#include <vector>

namespace opengt::render {

// Internal material bit added by draw-list construction. Captured PS1
// primitive bits occupy only the low nibble.
constexpr std::uint32_t world_primitive_screen_space_flag = 1U << 31;
// Internal-only material tag for narrow triangles emitted to preserve a
// proven authored edge through a synthetic temporal sample. These stitches
// must not merge otherwise independent authored topology groups; they still
// use the fixed perspective-correct 3D projection contract.
constexpr std::uint32_t world_primitive_temporal_seam_flag = 1U << 30;
// GT2 resolves coplanar course artwork by authored submission order because
// the PS1 GPU has no depth buffer. The resident-course decoder assigns this
// bounded layer to positive-area detail surfaces which are materially smaller
// than their support (lane markings, arrows, grid boxes, and similar road
// artwork). GT2 authors one class exactly coplanar and other classes one or two
// model units from road support. A separate typed replacement flag below marks
// later-authored detailed course surfaces which supersede an earlier coarse
// plane. The modern renderer resolves only these proven geometric relations;
// it is not a track, texture, address, or screen-coordinate exception.
constexpr std::uint32_t world_primitive_track_overlay_layer_shift = 24;
constexpr std::uint32_t world_primitive_track_overlay_layer_mask =
    0x1FU << world_primitive_track_overlay_layer_shift;
// The resident-course classifier records the exact earlier primitive which
// authored road artwork overlays. The renderer tags only visible pixels from
// those supports, allowing the overlay to win that typed relationship without
// bypassing walls, vehicles, or other nearer geometry.
constexpr std::uint32_t world_primitive_track_overlay_support_flag = 1U << 6;
// GT2 has no depth buffer, so a later detailed road packet replaces an earlier
// coarse opaque course plane wherever the two overlap. Modern depth can invert
// that authored ownership when the coarse plane is a few model units nearer to
// the camera. Resident decoding tags that relationship explicitly; the GPU
// then lets the replacement win only pixels whose nearest opaque owner is one
// of its classified supports, preserving walls and vehicles in front.
constexpr std::uint32_t world_primitive_track_replacement_flag = 1U << 7;
// Legacy capture tag identifying resident billboard quads. It is NOT a depth
// override: their DMA traversal ordinal is not a metric distance. Keep the bit
// readable for existing captures, but use normalized model-space corner depth.
constexpr std::uint32_t world_primitive_track_billboard_depth_flag = 1U << 8;
// Captured directly from GT2's authored pre-projection course data. Its
// shared model vertices are already continuous; topology inference intended
// for guest-projected packets must not rebuild or split it.
constexpr std::uint32_t world_primitive_resident_course_flag = 1U << 4;
// Managed capture tags geometry emitted by GT2's complete auxiliary camera
// submission. This is explicit authored pass ownership; it must not be folded
// into the main view merely because both passes share a render target or lens.
constexpr std::uint32_t world_primitive_secondary_view_flag = 1U << 5;
constexpr std::uint16_t world_vertex_source_identity_flag = 1U << 0;
constexpr std::uint16_t world_vertex_screen_offset_anchor_flag = 1U << 1;

enum class WorldViewChannel : std::uint8_t {
    main_view,
    secondary_view,
};

struct Ps1ProjectedPoint {
    std::int32_t x;
    std::int32_t y;
    std::uint16_t depth;
};

struct ContinuousProjectedPoint {
    float x;
    float y;
};

struct WorldMaterial {
    std::uint32_t primitive_flags;
    std::uint16_t texture_page;
    std::uint16_t clut;
    std::int16_t texture_mask_x;
    std::int16_t texture_mask_y;
    std::int16_t texture_offset_x;
    std::int16_t texture_offset_y;
    std::uint32_t environment_flags;
};

struct WorldDrawVertex {
    float world_x;
    float world_y;
    float world_z;
    float view_x;
    float view_y;
    float view_z;
    float clip_x;
    float clip_y;
    float clip_z;
    float clip_w;
    float screen_x;
    float screen_y;
    // Retain the authored continuous-projection state so temporal samples
    // can reproject a shape-preserving 3D transform.  Interpolating SXY
    // directly is not sufficient for rotating rigid parts such as wheels.
    float projection_offset_x;
    float projection_offset_y;
    float projection_plane;
    float draw_offset_x;
    float draw_offset_y;
    float u;
    float v;
    std::uint8_t r;
    std::uint8_t g;
    std::uint8_t b;
    std::uint8_t unused;
    std::int16_t model_x;
    std::int16_t model_y;
    std::int16_t model_z;
    std::uint16_t provenance_flags;
    std::uint32_t source_vertex_identity;
    std::int32_t exact_view_x;
    std::int32_t exact_view_y;
    std::int32_t exact_view_z;
    std::uint64_t transform_id;
    std::int16_t transform_rotation[9];
    std::int32_t transform_translation[3];
    bool exact_transform_valid;
    // Integer SXY emitted by the authored PS1 projection. The modern path
    // keeps this only as topology evidence while rendering with continuous
    // subpixel screen coordinates above.
    std::int32_t authored_screen_x;
    std::int32_t authored_screen_y;
};

struct WorldDrawCommand {
    WorldDrawVertex vertices[3];
    float face_normal_x;
    float face_normal_y;
    float face_normal_z;
    std::uint32_t material_index;
    std::int32_t ordering_table_index;
    std::int16_t clip_x0;
    std::int16_t clip_y0;
    std::int16_t clip_x1;
    std::int16_t clip_y1;
    std::uint32_t object_kind;
    std::uint32_t object_id;
    std::uint32_t model_pointer;
    std::uint64_t transform_id;
    std::int16_t transform_rotation[9];
    std::int32_t transform_translation[3];
    bool exact_transform_valid;
    std::uint32_t source_command_index;
    WorldViewChannel channel;
};

struct WorldDrawList {
    std::int32_t display_x;
    std::int32_t display_y;
    std::int32_t display_width;
    std::int32_t display_height;
    std::uint64_t frame_index;
    std::int32_t input_poll;
    std::uint64_t camera_transform_id;
    bool continuous_projection;
    std::vector<WorldMaterial> materials;
    std::vector<WorldDrawCommand> commands;
    std::uint32_t rejected_incomplete;
    // Breakdown of what the capture could not carry as 3D world
    // geometry, and what the screen-space fallback then dropped.
    std::uint32_t rejected_incomplete_track;
    std::uint32_t rejected_incomplete_vehicle;
    std::uint32_t rejected_screen_target;
    std::uint32_t rejected_screen_target_track;
    std::uint32_t rejected_oversized_screen_commands;
    std::uint32_t secondary_commands;
    std::uint32_t track_commands;
    std::uint32_t vehicle_commands;
    std::uint32_t background_commands;
    // object_kind 0 includes both explicit 2D screen packets and world
    // packets whose authored owner has not been reconstructed. Keep the
    // historical aggregate above/below for HUD sizing, but count the latter
    // separately so a complete live run can prove that no 3D packet remained
    // outside the modern layer contract.
    std::uint32_t unclassified_commands;
    std::uint32_t unclassified_world_commands;
};

struct WorldDrawListOptions {
    bool include_secondary_views;
    bool include_screen_space;
    bool continuous_projection;
    // The shipping live path must never compare raw GT2 camera Z values from
    // independently normalized objects. Development readers may leave this
    // false so historical capture files remain inspectable.
    bool require_world_depth_scale{};
};

enum class WorldDrawListResult {
    success,
    invalid_argument,
    invalid_camera,
    invalid_depth_scale,
    allocation_failed,
};

Ps1ProjectedPoint project_ps1_vertex(
    const WorldCaptureVertex& vertex,
    std::int16_t draw_offset_x,
    std::int16_t draw_offset_y
) noexcept;

ContinuousProjectedPoint project_continuous_vertex(
    const WorldCaptureVertex& vertex,
    std::int16_t draw_offset_x,
    std::int16_t draw_offset_y
) noexcept;

WorldDrawListResult build_world_draw_list(
    const WorldCaptureHeader& header,
    const WorldCaptureTriangle* triangles,
    std::size_t triangle_count,
    WorldDrawListOptions options,
    WorldDrawList* output
) noexcept;

const char* world_draw_list_result_name(
    WorldDrawListResult result
) noexcept;

} // namespace opengt::render
