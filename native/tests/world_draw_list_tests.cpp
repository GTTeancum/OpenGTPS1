#include "opengt/world_draw_list.hpp"

#include <cmath>
#include <cstdio>

namespace {

bool expect(bool value, const char* message) {
    if (!value)
        std::fprintf(stderr, "FAILED: %s\n", message);
    return value;
}

opengt::render::WorldCaptureVertex vertex(
    int view_x,
    int view_y,
    int view_z,
    float world_x,
    float world_y
) {
    opengt::render::WorldCaptureVertex result{};
    result.world_valid = true;
    result.view_x = view_x;
    result.view_y = view_y;
    result.view_z = view_z;
    result.world_x = world_x;
    result.world_y = world_y;
    result.world_z = 0.0F;
    result.projection_offset_x = 160 << 16;
    result.projection_offset_y = 120 << 16;
    result.projection_plane = 256;
    result.u = static_cast<std::int16_t>(world_x);
    result.v = static_cast<std::int16_t>(world_y);
    result.r = result.g = result.b = 128;
    return result;
}

} // namespace

int main() {
    using namespace opengt::render;
    WorldCaptureHeader header{};
    header.display_width = 320;
    header.display_height = 240;
    header.camera_transform_id = 1;
    header.projection_offset_x = 160 << 16;
    header.projection_offset_y = 120 << 16;
    header.projection_plane = 256;

    WorldCaptureTriangle triangles[2]{};
    triangles[0].primitive_flags = 1;
    triangles[0].texture_page = 7;
    triangles[0].clut = 12;
    triangles[0].object_kind = 1;
    triangles[0].vertices[0] = vertex(0, 0, 256, 0, 0);
    triangles[0].vertices[1] = vertex(100, 0, 256, 100, 0);
    triangles[0].vertices[2] = vertex(0, 100, 256, 0, 100);
    triangles[1] = triangles[0];
    for (auto& point : triangles[1].vertices)
        point.projection_plane = 120;

    const Ps1ProjectedPoint center =
        project_ps1_vertex(triangles[0].vertices[0], 0, 0);
    bool okay = expect(
        center.x == 160 && center.y == 120 && center.depth == 256,
        "exact center projection");

    WorldDrawList list{};
    okay &= expect(
        build_world_draw_list(
            header,
            triangles,
            2,
            WorldDrawListOptions{false, false, false},
            &list) == WorldDrawListResult::success,
        "build main draw list");
    okay &= expect(list.commands.size() == 1, "filter secondary view");
    okay &= expect(list.secondary_commands == 1, "count secondary view");
    okay &= expect(list.materials.size() == 1, "deduplicate material");
    okay &= expect(list.track_commands == 1, "count track commands");
    okay &= expect(
        std::fabs(list.commands[0].vertices[0].clip_x) < 0.001F &&
        std::fabs(list.commands[0].vertices[0].clip_y) < 0.001F,
        "center maps to clip origin");
    okay &= expect(
        std::fabs(list.commands[0].face_normal_z - 1.0F) < 0.001F,
        "derive face normal for future lighting");

    // GT2's raw course hook observes H one unit before the projected vehicle
    // packets in the same authored view. Treat that integer boundary as one
    // camera instead of splitting the course and cars into unrelated layers;
    // a larger difference remains a genuine secondary projection.
    WorldCaptureTriangle projection_variants[3]{
        triangles[0], triangles[0], triangles[0]};
    for (auto& point : projection_variants[1].vertices)
        point.projection_plane = 257;
    for (auto& point : projection_variants[2].vertices)
        point.projection_plane = 258;
    WorldDrawList projection_tolerance{};
    okay &= expect(
        build_world_draw_list(
            header,
            projection_variants,
            3,
            WorldDrawListOptions{false, false, true},
            &projection_tolerance) == WorldDrawListResult::success,
        "build one-unit main-projection tolerance fixture");
    okay &= expect(
        projection_tolerance.commands.size() == 2 &&
        projection_tolerance.secondary_commands == 1 &&
        projection_tolerance.commands[0].channel ==
            WorldViewChannel::main_view &&
        projection_tolerance.commands[1].channel ==
            WorldViewChannel::main_view,
        "keep one-unit projection boundary in the shared world view");

    // This is the transform-scale boundary from the captured 0:57 track
    // seam.  The two authored copies represent the same endpoint, but the
    // PS1's integer projection rounds their Y coordinates to 345 and 346.
    // Enhanced continuous projection must retain the subpixel relationship
    // instead of magnifying that one-pixel engine crack.
    WorldCaptureHeader seam_header = header;
    seam_header.display_y = 240;
    seam_header.projection_plane = 597;
    seam_header.draw_offset_y = 240;
    WorldCaptureTriangle seam_triangles[2]{};
    seam_triangles[0].object_kind = 1;
    seam_triangles[0].draw_offset_y = 240;
    seam_triangles[0].vertices[0] =
        vertex(891, -354, 15091, 0, 0);
    seam_triangles[0].vertices[1] =
        vertex(343, -203, 14909, 1, 0);
    seam_triangles[0].vertices[2] =
        vertex(584, -361, 15939, 0, 1);
    seam_triangles[1] = seam_triangles[0];
    seam_triangles[1].vertices[0] =
        vertex(1784, -706, 30184, 0, 0);
    seam_triangles[1].vertices[1] =
        vertex(687, -405, 29819, 1, 0);
    seam_triangles[1].vertices[2] =
        vertex(1202, -368, 28154, 0, 1);
    for (auto& triangle : seam_triangles) {
        for (auto& point : triangle.vertices)
            point.projection_plane = 597;
    }
    WorldDrawList seam_ps1{};
    WorldDrawList seam_enhanced{};
    okay &= expect(
        build_world_draw_list(
            seam_header,
            seam_triangles,
            2,
            WorldDrawListOptions{false, false, false},
            &seam_ps1) == WorldDrawListResult::success &&
        build_world_draw_list(
            seam_header,
            seam_triangles,
            2,
            WorldDrawListOptions{false, false, true},
            &seam_enhanced) == WorldDrawListResult::success,
        "build PS1 and continuous seam projections");
    okay &= expect(
        seam_ps1.commands.size() == 2 &&
        seam_enhanced.commands.size() == 2 &&
        seam_ps1.commands[0].vertices[0].screen_y == 345.0F &&
        seam_ps1.commands[1].vertices[0].screen_y == 346.0F,
        "reproduce one-pixel PS1 transform-scale seam");
    okay &= expect(
        std::fabs(
            seam_enhanced.commands[0].vertices[0].screen_y -
            seam_enhanced.commands[1].vertices[0].screen_y) < 0.05F,
        "preserve continuous subpixel boundary");

    // Complete-course residency intentionally submits authored geometry on
    // both sides of the camera. Signed homogeneous W lets D3D clip an edge at
    // the real near plane; clamping a rear vertex to W=1 turns it into a
    // screen-spanning polygon instead.
    WorldCaptureTriangle near_crossing = triangles[0];
    near_crossing.vertices[0] = vertex(64, 0, -256, 0, 0);
    near_crossing.vertices[1] = vertex(64, 0, 256, 100, 0);
    near_crossing.vertices[2] = vertex(0, 64, 256, 0, 100);
    WorldDrawList near_crossing_list{};
    okay &= expect(
        build_world_draw_list(
            header,
            &near_crossing,
            1,
            WorldDrawListOptions{false, false, true},
            &near_crossing_list) == WorldDrawListResult::success,
        "build camera-crossing course triangle");
    okay &= expect(
        near_crossing_list.commands.size() == 1 &&
        near_crossing_list.commands[0].vertices[0].clip_w == -256.0F &&
        near_crossing_list.commands[0].vertices[0].clip_z < 0.0F &&
        near_crossing_list.commands[0].vertices[1].clip_w == 256.0F &&
        near_crossing_list.commands[0].vertices[1].clip_z > 0.0F,
        "retain signed camera depth for homogeneous near clipping");
    okay &= expect(
        std::fabs(
            near_crossing_list.commands[0].vertices[0].clip_x -
            102.4F) < 0.01F,
        "project rear vertex linearly in homogeneous camera space");

    WorldDrawList live_list{};
    okay &= expect(
        build_world_draw_list(
            header,
            triangles,
            2,
            WorldDrawListOptions{true, false, false},
            &live_list) == WorldDrawListResult::success,
        "build live draw list with secondary projection");
    okay &= expect(
        live_list.commands.size() == 2 &&
        live_list.secondary_commands == 1 &&
        live_list.commands[1].channel == WorldViewChannel::secondary_view,
        "retain authored rear-view projection for live presentation");

    WorldCaptureTriangle screen_triangle{};
    screen_triangle.primitive_flags = 1;
    screen_triangle.clip_x0 = 0;
    screen_triangle.clip_y0 = 0;
    screen_triangle.clip_x1 = 319;
    screen_triangle.clip_y1 = 239;
    screen_triangle.vertices[0].screen_x = 16.0F;
    screen_triangle.vertices[0].screen_y = 144.0F;
    screen_triangle.vertices[1].screen_x = 112.0F;
    screen_triangle.vertices[1].screen_y = 144.0F;
    screen_triangle.vertices[2].screen_x = 16.0F;
    screen_triangle.vertices[2].screen_y = 240.0F;
    WorldDrawList screen_list{};
    okay &= expect(
        build_world_draw_list(
            header,
            &screen_triangle,
            1,
            WorldDrawListOptions{false, true, false},
            &screen_list) == WorldDrawListResult::success,
        "build screen-space draw list");
    okay &= expect(
        screen_list.commands.size() == 1,
        "retain displayed screen primitive");
    okay &= expect(
        screen_list.materials.size() == 1 &&
        (screen_list.materials[0].primitive_flags &
            world_primitive_screen_space_flag) != 0,
        "tag screen material for sprite texel sampling");
    okay &= expect(
        screen_list.commands[0].channel == WorldViewChannel::main_view,
        "classify displayed HUD primitive as part of the main view");

    // SSR11 street-light regression: the authored billboard is above the
    // camera, but one Y coordinate crosses the PS1 signed screen boundary
    // and wraps from about -1025 to +1023. The PS1 polygon-size rule rejects
    // this malformed half-quad; accepting it creates a bright vertical flash.
    WorldCaptureTriangle wrapped_light = screen_triangle;
    wrapped_light.primitive_flags = 0x0BU;
    wrapped_light.texture_page = 41;
    wrapped_light.clut = 32599;
    wrapped_light.vertices[0].screen_x = 270.0F;
    wrapped_light.vertices[0].screen_y = -991.0F;
    wrapped_light.vertices[1].screen_x = 188.0F;
    wrapped_light.vertices[1].screen_y = 1023.0F;
    wrapped_light.vertices[2].screen_x = 236.0F;
    wrapped_light.vertices[2].screen_y = -909.0F;
    WorldCaptureTriangle screen_triangles[2]{
        screen_triangle,
        wrapped_light,
    };
    WorldDrawList filtered_screen_list{};
    okay &= expect(
        build_world_draw_list(
            header,
            screen_triangles,
            2,
            WorldDrawListOptions{false, true, false},
            &filtered_screen_list) == WorldDrawListResult::success,
        "build screen list containing wrapped street light");
    okay &= expect(
        filtered_screen_list.commands.size() == 1 &&
        filtered_screen_list.rejected_oversized_screen_commands == 1,
        "reject wrapped SSR11 street-light triangle by PS1 span limits");


    if (!okay)
        return 1;
    std::puts("world draw-list tests passed");
    return 0;
}
