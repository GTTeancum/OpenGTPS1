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
    header.frame_index = 123;
    header.input_poll = 456;
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
    for (auto& point : triangles[1].vertices) {
        point.projection_plane = 120;
        point.projection_offset_x = 128 << 16;
    }

    const Ps1ProjectedPoint center =
        project_ps1_vertex(triangles[0].vertices[0], 0, 0);
    bool okay = expect(
        center.x == 160 && center.y == 120 && center.depth == 256,
        "exact center projection");
    const WorldCaptureVertex fractional_source =
        vertex(1, -2, 3, 0, 0);
    const ContinuousProjectedPoint fractional =
        project_continuous_vertex(fractional_source, 7, 240);
    okay &= expect(
        std::fabs(fractional.x - (167.0F + 256.0F / 3.0F)) <
                0.0001F &&
            std::fabs(fractional.y - (360.0F - 512.0F / 3.0F)) <
                0.0001F,
        "continuous projection preserves offsets and fractional position");

    // The GTE exposes integer view coordinates, but GT2's exact matrix keeps
    // twelve fractional bits. A half-unit X translation must survive into the
    // modern projection instead of snapping this vertex back to the centre.
    WorldCaptureVertex fixed_transform_source =
        vertex(0, 0, 100, 0, 0);
    fixed_transform_source.model_x = 1;
    fixed_transform_source.transform_id = 7;
    fixed_transform_source.transform_rotation[0] = 2048;
    fixed_transform_source.transform_rotation[4] = 4096;
    fixed_transform_source.transform_rotation[8] = 4096;
    fixed_transform_source.transform_translation[2] = 100;
    fixed_transform_source.exact_transform_valid = true;
    const ContinuousProjectedPoint fixed_transform_projection =
        project_continuous_vertex(fixed_transform_source, 0, 0);
    okay &= expect(
        std::fabs(fixed_transform_projection.x - 161.28F) < 0.0001F &&
            std::fabs(fixed_transform_projection.y - 120.0F) < 0.0001F,
        "continuous projection preserves fixed-point transform fractions");

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
    okay &= expect(
        list.frame_index == 123 && list.input_poll == 456,
        "retain capture identity for renderer diagnostics");
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

    WorldCaptureTriangle fixed_transform_triangle = triangles[0];
    fixed_transform_triangle.transform_id = 7;
    fixed_transform_triangle.transform_rotation[0] = 2048;
    fixed_transform_triangle.transform_rotation[4] = 4096;
    fixed_transform_triangle.transform_rotation[8] = 4096;
    fixed_transform_triangle.transform_translation[2] = 100;
    fixed_transform_triangle.exact_transform_valid = true;
    for (int index = 0; index < 3; ++index) {
        auto& point = fixed_transform_triangle.vertices[index];
        point = fixed_transform_source;
        point.model_x = static_cast<std::int16_t>(1 + index);
        point.model_y = static_cast<std::int16_t>(index == 2 ? 1 : 0);
        point.view_x = point.model_x / 2;
        point.view_y = point.model_y;
    }
    WorldDrawList fixed_transform_list{};
    okay &= expect(
        build_world_draw_list(
            header,
            &fixed_transform_triangle,
            1,
            WorldDrawListOptions{false, false, true},
            &fixed_transform_list) == WorldDrawListResult::success &&
            fixed_transform_list.commands.size() == 1,
        "build exact fixed-point transform fixture");
    okay &= expect(
        std::fabs(
            fixed_transform_list.commands[0].vertices[0].view_x - 0.5F) <
                0.0001F &&
            std::fabs(
                fixed_transform_list.commands[0].vertices[0].screen_x -
                161.28F) < 0.0001F &&
            std::fabs(
                fixed_transform_list.commands[0].vertices[0].clip_w -
                100.0F) < 0.0001F &&
            std::fabs(
                fixed_transform_list.commands[0].vertices[0].clip_z -
                16.0F) < 0.0001F,
        "draw list uses fixed-point view reconstruction and reversed depth");

    // Integer PS1 SXY collapses the first and third vertices to the same
    // pixel. Continuous projection separates their X coordinates and would
    // otherwise invent a thin, screen-spanning sliver at enhanced resolution.
    // Keep the command and weld every occurrence of both endpoints so its
    // neighboring surfaces remain closed.
    WorldCaptureTriangle native_degenerate = triangles[0];
    native_degenerate.vertices[0] = vertex(1, 0, 4000, 0, 0);
    native_degenerate.vertices[1] = vertex(-250, -500, 4000, 0, 0);
    native_degenerate.vertices[2] = vertex(2, 0, 4000, 0, 0);
    WorldDrawList native_degenerate_list{};
    okay &= expect(
        build_world_draw_list(
            header,
            &native_degenerate,
            1,
            WorldDrawListOptions{false, false, true},
            &native_degenerate_list) == WorldDrawListResult::success &&
        native_degenerate_list.commands.size() == 1 &&
        native_degenerate_list.commands[0].vertices[0].screen_x ==
            native_degenerate_list.commands[0].vertices[2].screen_x &&
        native_degenerate_list.commands[0].vertices[0].screen_y ==
            native_degenerate_list.commands[0].vertices[2].screen_y,
        "weld authored zero-width PS1 micro-seams");

    // GT2 independently normalizes camera translation for each submitted
    // object before the GTE sees it. Raw SZ/view-Z therefore cannot be
    // compared across objects. This fixture deliberately reverses the raw-Z
    // ordering: the first object has raw Z=100 but exponent 1 (common Z=200),
    // while the second has raw Z=150 and exponent 0 (common Z=150).
    WorldCaptureTriangle normalized_depth[2]{triangles[0], triangles[0]};
    normalized_depth[0].depth_scale_exponent = 1;
    normalized_depth[0].depth_scale_valid = true;
    normalized_depth[0].vertices[0] = vertex(0, 0, 100, 0, 0);
    normalized_depth[0].vertices[1] = vertex(40, 0, 100, 40, 0);
    normalized_depth[0].vertices[2] = vertex(0, 40, 100, 0, 40);
    normalized_depth[1].depth_scale_exponent = 0;
    normalized_depth[1].depth_scale_valid = true;
    normalized_depth[1].vertices[0] = vertex(0, 0, 150, 0, 0);
    normalized_depth[1].vertices[1] = vertex(60, 0, 150, 60, 0);
    normalized_depth[1].vertices[2] = vertex(0, 60, 150, 0, 60);
    WorldDrawList normalized_depth_list{};
    okay &= expect(
        build_world_draw_list(
            header,
            normalized_depth,
            2,
            WorldDrawListOptions{false, false, true, true},
            &normalized_depth_list) == WorldDrawListResult::success &&
            normalized_depth_list.commands.size() == 2,
        "build required GT2-normalized depth fixture");
    if (normalized_depth_list.commands.size() == 2) {
        const auto& farther = normalized_depth_list.commands[0].vertices[1];
        const auto& nearer = normalized_depth_list.commands[1].vertices[1];
        okay &= expect(
            farther.view_z == 100.0F && nearer.view_z == 150.0F &&
                farther.clip_w == 200.0F && nearer.clip_w == 150.0F &&
                farther.clip_z / farther.clip_w <
                    nearer.clip_z / nearer.clip_w,
            "compare independently normalized objects in common GT2 depth");
        okay &= expect(
            std::fabs(
                farther.clip_x / farther.clip_w -
                nearer.clip_x / nearer.clip_w) < 0.0001F &&
                std::fabs(
                    farther.clip_y / farther.clip_w -
                    nearer.clip_y / nearer.clip_w) < 0.0001F,
            "depth normalization leaves perspective projection unchanged");
    }
    normalized_depth[0].depth_scale_valid = false;
    WorldDrawList missing_depth_scale_list{};
    okay &= expect(
        build_world_draw_list(
            header,
            normalized_depth,
            1,
            WorldDrawListOptions{false, false, true, true},
            &missing_depth_scale_list) ==
            WorldDrawListResult::invalid_depth_scale,
        "reject live world geometry without GT2 depth normalization");

    // The former Clubman test incorrectly treated reverse DMA ordinal 3390
    // as a metric bucket. A halo at Z 7,183,360 is in front of a surface at
    // Z 13,538,792, not behind it. Preserve actual depth and projected shape.
    WorldCaptureTriangle biased_billboard = triangles[0];
    biased_billboard.depth_scale_exponent = 9;
    biased_billboard.depth_scale_valid = true;
    biased_billboard.ordering_table_index = 3390;
    for (auto& point : biased_billboard.vertices) {
        point = vertex(0, 0, 14030, 0, 0);
        point.screen_offset_anchor = true;
    }
    biased_billboard.vertices[1].view_x = 1000;
    biased_billboard.vertices[2].view_y = 1000;
    WorldDrawList biased_billboard_list{};
    okay &= expect(
        build_world_draw_list(
            header,
            &biased_billboard,
            1,
            WorldDrawListOptions{false, false, true, true},
            &biased_billboard_list) == WorldDrawListResult::success &&
        biased_billboard_list.commands.size() == 1,
        "build authored track billboard depth fixture");
    if (biased_billboard_list.commands.size() == 1) {
        const auto& point = biased_billboard_list.commands[0].vertices[1];
        constexpr float authored_common_z = 14030.0F * 512.0F;
        okay &= expect(
            point.clip_w == authored_common_z &&
                16.0F / point.clip_w > 16.0F / 13538792.0F,
            "screen-offset halo uses anchor depth, not reverse DMA ordinal");
        okay &= expect(
            std::fabs(
                point.clip_x / point.clip_w -
                (point.screen_x / 160.0F - 1.0F)) < 0.0001F,
            "physical halo depth preserves projected screen position");
    }
    // SSR5 starting straight: near right-hand lamp halo versus far building.
    auto ssr5_flare = biased_billboard;
    ssr5_flare.depth_scale_exponent = 8;
    ssr5_flare.ordering_table_index = 4020;
    for (auto& point : ssr5_flare.vertices) point.view_z = 7888;
    WorldDrawList ssr5_flare_list{};
    okay &= expect(build_world_draw_list(header, &ssr5_flare, 1,
        WorldDrawListOptions{false, false, true, true}, &ssr5_flare_list) ==
            WorldDrawListResult::success &&
        ssr5_flare_list.commands.size() == 1 &&
        ssr5_flare_list.commands[0].vertices[0].clip_w == 2019328.0F &&
        ssr5_flare_list.commands[0].vertices[0].clip_z /
            ssr5_flare_list.commands[0].vertices[0].clip_w > 16.0F / 12567668.0F,
        "SSR5 streetlight halo remains in front of its distant building");
    WorldCaptureTriangle ordinary_track = biased_billboard;
    for (auto& point : ordinary_track.vertices)
        point.screen_offset_anchor = false;
    WorldDrawList ordinary_track_list{};
    okay &= expect(
        build_world_draw_list(
            header,
            &ordinary_track,
            1,
            WorldDrawListOptions{false, false, true, true},
            &ordinary_track_list) == WorldDrawListResult::success &&
        ordinary_track_list.commands.size() == 1 &&
        ordinary_track_list.commands[0].vertices[0].clip_w ==
            14030.0F * 512.0F,
        "keep ordinary track geometry on normalized camera depth");
    WorldCaptureTriangle resident_billboard = ordinary_track;
    resident_billboard.primitive_flags |=
        world_primitive_track_billboard_depth_flag;
    WorldDrawList resident_billboard_list{};
    okay &= expect(
        build_world_draw_list(
            header,
            &resident_billboard,
            1,
            WorldDrawListOptions{false, false, true, true},
            &resident_billboard_list) == WorldDrawListResult::success &&
        resident_billboard_list.commands.size() == 1 &&
        resident_billboard_list.commands[0].vertices[0].clip_w ==
            14030.0F * 512.0F,
        "legacy resident billboard tag must not turn DMA ordinal into depth");
    // Measured Midfield wall/tree overlap at poll 2859. The farther tree's
    // reverse traversal ordinal 168 previously became common Z 1,376,256,
    // incorrectly beating the wall at common Z approximately 4,049,442.
    resident_billboard.depth_scale_exponent = 11;
    resident_billboard.ordering_table_index = 168;
    resident_billboard.primitive_flags |= world_primitive_resident_course_flag;
    for (auto& point : resident_billboard.vertices)
        point.view_z = 16361;
    resident_billboard.vertices[1].view_z = 16367;
    resident_billboard.vertices[2].view_z = 16368;
    WorldDrawList midfield_billboard_list{};
    okay &= expect(
        build_world_draw_list(header, &resident_billboard, 1,
            WorldDrawListOptions{false, false, true, true},
            &midfield_billboard_list) == WorldDrawListResult::success &&
        midfield_billboard_list.commands.size() == 1,
        "build measured Midfield resident tree regression");
    if (midfield_billboard_list.commands.size() == 1) {
        const auto& points = midfield_billboard_list.commands[0].vertices;
        okay &= expect(
            points[0].clip_w == 16361.0F * 2048.0F &&
            points[1].clip_w == 16367.0F * 2048.0F &&
            points[2].clip_w == 16368.0F * 2048.0F &&
            points[0].clip_z / points[0].clip_w < 16.0F / 4049442.0F,
            "distant Midfield tree stays behind building using physical corner depth");
    }
    biased_billboard.ordering_table_index = 0;
    WorldDrawList zero_bucket_billboard_list{};
    okay &= expect(
        build_world_draw_list(
            header,
            &biased_billboard,
            1,
            WorldDrawListOptions{false, false, true, true},
            &zero_bucket_billboard_list) == WorldDrawListResult::success &&
        zero_bucket_billboard_list.commands.size() == 1 &&
        zero_bucket_billboard_list.commands[0].vertices[0].clip_w ==
            14030.0F * 512.0F,
        "keep zero-bucket track billboard on captured anchor depth");

    WorldCaptureTriangle background_triangle = triangles[0];
    background_triangle.object_kind = 3;
    WorldDrawList background_list{};
    okay &= expect(
        build_world_draw_list(
            header,
            &background_triangle,
            1,
            WorldDrawListOptions{false, false, true, true},
            &background_list) == WorldDrawListResult::success &&
        background_list.commands.size() == 1 &&
        background_list.background_commands == 1 &&
        background_list.unclassified_commands == 0 &&
        background_list.unclassified_world_commands == 0,
        "retain explicit authored background ownership");

    WorldCaptureTriangle unknown_world_triangle = triangles[0];
    unknown_world_triangle.object_kind = 0;
    WorldDrawList unknown_world_list{};
    okay &= expect(
        build_world_draw_list(
            header,
            &unknown_world_triangle,
            1,
            WorldDrawListOptions{false, false, true},
            &unknown_world_list) == WorldDrawListResult::success &&
        unknown_world_list.commands.size() == 1 &&
        unknown_world_list.unclassified_commands == 1 &&
        unknown_world_list.unclassified_world_commands == 1,
        "distinguish ownerless world geometry from explicit screen packets");

    // A per-submission focal-length change does not create a separate camera
    // or depth space. The primitive retains its own H during projection while
    // sharing coherent view-space depth with the rest of the target.
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
        "build same-target focal-length fixture");
    okay &= expect(
        projection_tolerance.commands.size() == 3 &&
        projection_tolerance.secondary_commands == 0 &&
        projection_tolerance.commands[0].channel ==
            WorldViewChannel::main_view &&
        projection_tolerance.commands[1].channel ==
            WorldViewChannel::main_view &&
        projection_tolerance.commands[2].channel ==
            WorldViewChannel::main_view,
        "keep all focal lengths in one target's shared world view");

    // Seattle's resident-course hook and vehicle packets can differ by two H
    // units normally and by hundreds while the replay camera zoom is being
    // updated. Both cases use the same projection centre and target, so both
    // must keep the same depth surface.
    WorldCaptureHeader seattle_projection_header = header;
    seattle_projection_header.projection_plane = 913;
    WorldCaptureTriangle seattle_projection_variants[3]{
        triangles[0], triangles[0], triangles[0]};
    for (auto& point : seattle_projection_variants[0].vertices)
        point.projection_plane = 913;
    for (auto& point : seattle_projection_variants[1].vertices)
        point.projection_plane = 915;
    for (auto& point : seattle_projection_variants[2].vertices)
        point.projection_plane = 160;
    WorldDrawList seattle_projection_tolerance{};
    okay &= expect(
        build_world_draw_list(
            seattle_projection_header,
            seattle_projection_variants,
            3,
            WorldDrawListOptions{false, false, true},
            &seattle_projection_tolerance) == WorldDrawListResult::success &&
        seattle_projection_tolerance.commands.size() == 3 &&
        seattle_projection_tolerance.secondary_commands == 0,
        "share depth across Seattle's complete focal-length update");

    // A complete auxiliary GT2 scene pass can share the main pass's drawing
    // target, projection centre, and H. Explicit authored pass ownership must
    // still keep its vehicles and course geometry out of the main view.
    WorldCaptureTriangle explicit_pass_variants[2]{
        triangles[0], triangles[0]};
    explicit_pass_variants[1].primitive_flags |=
        world_primitive_secondary_view_flag;
    WorldDrawList explicit_pass_main_only{};
    okay &= expect(
        build_world_draw_list(
            header,
            explicit_pass_variants,
            2,
            WorldDrawListOptions{false, false, true},
            &explicit_pass_main_only) == WorldDrawListResult::success &&
        explicit_pass_main_only.commands.size() == 1 &&
        explicit_pass_main_only.secondary_commands == 1 &&
        explicit_pass_main_only.commands[0].channel ==
            WorldViewChannel::main_view,
        "exclude explicitly tagged auxiliary pass from main view");
    WorldDrawList explicit_pass_all_views{};
    okay &= expect(
        build_world_draw_list(
            header,
            explicit_pass_variants,
            2,
            WorldDrawListOptions{true, false, true},
            &explicit_pass_all_views) == WorldDrawListResult::success &&
        explicit_pass_all_views.commands.size() == 2 &&
        explicit_pass_all_views.commands[1].channel ==
            WorldViewChannel::secondary_view,
        "retain explicitly tagged auxiliary pass for diagnostics");

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
        near_crossing_list.commands[0].vertices[0].clip_z == 16.0F &&
        near_crossing_list.commands[0].vertices[1].clip_w == 256.0F &&
        near_crossing_list.commands[0].vertices[1].clip_z == 16.0F &&
        near_crossing_list.commands[0].vertices[1].clip_z /
                near_crossing_list.commands[0].vertices[1].clip_w ==
            0.0625F,
        "retain signed W and reversed infinite homogeneous near clipping");
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
        screen_list.commands[0].channel == WorldViewChannel::main_view &&
        screen_list.unclassified_commands == 1 &&
        screen_list.unclassified_world_commands == 0,
        "classify displayed HUD without counting it as ownerless world geometry");

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
