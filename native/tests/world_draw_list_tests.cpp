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

    if (!okay)
        return 1;
    std::puts("world draw-list tests passed");
    return 0;
}
