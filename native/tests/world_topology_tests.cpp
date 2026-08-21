#include "opengt/world_topology.hpp"

#include <cmath>
#include <cstdio>

namespace {

bool expect(bool value, const char* message) {
    if (!value)
        std::fprintf(stderr, "FAILED: %s\n", message);
    return value;
}

opengt::render::WorldDrawVertex vertex(
    std::int16_t x,
    std::int16_t y,
    std::int16_t z,
    std::uint32_t pointer,
    float geometric_bias = 0.0F
) {
    opengt::render::WorldDrawVertex result{};
    result.model_x = x;
    result.model_y = y;
    result.model_z = z;
    result.exact_view_x = x;
    result.exact_view_y = y;
    result.exact_view_z = z;
    result.source_vertex_identity = pointer;
    result.provenance_flags = 1;
    result.world_x = x + geometric_bias;
    result.world_y = y;
    result.world_z = z;
    result.view_x = x + geometric_bias;
    result.view_y = y;
    result.view_z = 256.0F;
    result.clip_x = x + geometric_bias;
    result.clip_y = y;
    result.clip_z = 0.5F;
    result.clip_w = 256.0F;
    result.screen_x = x + geometric_bias;
    result.screen_y = y;
    result.authored_screen_x = x;
    result.authored_screen_y = y;
    result.u = static_cast<float>(x);
    result.v = static_cast<float>(y);
    result.r = result.g = result.b = 128;
    return result;
}

opengt::render::WorldDrawCommand triangle(
    opengt::render::WorldDrawVertex a,
    opengt::render::WorldDrawVertex b,
    opengt::render::WorldDrawVertex c,
    std::uint32_t model,
    std::uint32_t source_index,
    std::uint32_t material = 0
) {
    opengt::render::WorldDrawCommand result{};
    result.vertices[0] = a;
    result.vertices[1] = b;
    result.vertices[2] = c;
    result.material_index = material;
    result.object_kind = 1;
    result.object_id = source_index + 1;
    result.model_pointer = model;
    result.source_command_index = source_index;
    result.channel = opengt::render::WorldViewChannel::main_view;
    return result;
}

} // namespace

int main() {
    using namespace opengt::render;
    WorldDrawList list{};
    list.materials.resize(2);
    list.commands.push_back(triangle(
        vertex(0, 0, 0, 0x1000),
        vertex(10, 0, 0, 0x1014),
        vertex(0, 10, 0, 0x1028),
        0x80001000,
        0));
    list.commands.push_back(triangle(
        vertex(5, 0, 0, 0x2000),
        vertex(5, -5, 0, 0x2014),
        vertex(10, -5, 0, 0x2028),
        0x80002000,
        1));
    // An authored boundary copy with deliberately different recovered float
    // geometry proves that exact integer provenance, not distance, drives the
    // canonical join.
    auto sector_boundary = vertex(0, 0, 0, 0x3000, 0.25F);
    sector_boundary.model_x = 4096;
    list.commands.push_back(triangle(
        sector_boundary,
        vertex(-5, 0, 0, 0x3014),
        vertex(0, -5, 0, 0x3028),
        0x80003000,
        2));
    // Exact coplanar duplicates away from the T-junction fixture exercise
    // ownership without turning the long edge into a manifold edge.
    list.commands.push_back(triangle(
        vertex(20, 0, 0, 0x4000),
        vertex(30, 0, 0, 0x4014),
        vertex(20, 10, 0, 0x4028),
        0x80004000,
        4));
    list.commands.push_back(triangle(
        vertex(20, 0, 0, 0x5000),
        vertex(30, 0, 0, 0x5014),
        vertex(20, 10, 0, 0x5028),
        0x80005000,
        3));
    list.commands.push_back(triangle(
        vertex(20, 0, 0, 0x6000),
        vertex(30, 0, 0, 0x6014),
        vertex(20, 10, 0, 0x6028),
        0x80006000,
        5,
        1));
    list.track_commands =
        static_cast<std::uint32_t>(list.commands.size());

    WorldTopologyStats stats{};
    bool okay = expect(
        apply_world_topology(
            &list,
            WorldTopologyOptions{true, true, true},
            &stats) == WorldTopologyResult::success,
        "apply topology");
    okay &= expect(
        stats.authored_boundary_groups >= 1,
        "identify authored exact-position copies");
    okay &= expect(
        stats.adjusted_vertex_instances >= 1,
        "canonicalize copied boundary geometry");
    okay &= expect(
        stats.exact_t_junctions >= 1,
        "find exact T-junction");
    okay &= expect(
        stats.split_source_triangles >= 1 &&
        stats.output_commands > stats.input_commands,
        "split T-junction source triangle");
    okay &= expect(
        stats.exact_duplicate_pairs >= 1 &&
        stats.ownership_components >= 1 &&
        stats.ownership_reorders == 2,
        "establish deterministic coplanar ownership");
    std::size_t source_three = list.commands.size();
    std::size_t source_four = list.commands.size();
    std::size_t different_material = list.commands.size();
    for (std::size_t index = 0; index < list.commands.size(); ++index) {
        const auto& command = list.commands[index];
        if (command.source_command_index == 3)
            source_three = index;
        else if (command.source_command_index == 4)
            source_four = index;
        else if (
            command.source_command_index == 5 &&
            command.material_index == 1
        )
            different_material = index;
    }
    okay &= expect(
        stats.material_overlap_pairs >= 1 &&
        source_three < source_four &&
        different_material < list.commands.size(),
        "reorder same-material ownership but preserve material seam");
    bool has_split_vertex = false;
    for (const auto& command : list.commands) {
        for (const auto& point : command.vertices) {
            if (point.model_x == 5 && point.model_y == 0) {
                has_split_vertex = true;
                okay &= expect(
                    std::fabs(point.u - 5.0F) < 0.001F,
                    "interpolate edge UV at exact split");
            }
        }
    }
    okay &= expect(has_split_vertex, "emit exact split vertex");
    okay &= expect(
        list.track_commands == list.commands.size(),
        "refresh draw-list category counts");

    // More than four copies exercise the inline occurrence bucket's overflow
    // while preserving canonical authored-boundary selection and iteration.
    WorldDrawList occurrence_overflow_list{};
    for (std::uint32_t index = 0; index < 5; ++index) {
        occurrence_overflow_list.commands.push_back(triangle(
            vertex(
                40,
                40,
                0,
                0x7000 + index * 0x100,
                static_cast<float>(index) * 0.125F),
            vertex(
                static_cast<std::int16_t>(50 + index * 2),
                40,
                0,
                0x7014 + index * 0x100),
            vertex(
                40,
                static_cast<std::int16_t>(50 + index * 2),
                0,
                0x7028 + index * 0x100),
            0x81000000 + index * 0x1000,
            100 + index));
    }
    occurrence_overflow_list.track_commands = 5;
    WorldTopologyStats occurrence_overflow_stats{};
    okay &= expect(
        apply_world_topology(
            &occurrence_overflow_list,
            WorldTopologyOptions{true, false, false},
            &occurrence_overflow_stats) == WorldTopologyResult::success,
        "apply occurrence overflow topology");
    okay &= expect(
        occurrence_overflow_stats.exact_position_groups >= 1 &&
        occurrence_overflow_stats.authored_boundary_groups >= 1 &&
        occurrence_overflow_stats.adjusted_vertex_instances == 4,
        "canonicalize all five inline and overflow occurrences");

    // Adjacent track sections can submit the same authored boundary through
    // differently scaled GTE transforms. Continuous projection preserves the
    // subpixel result, and topology must then give both copies one exact
    // projected endpoint without moving unrelated geometry.
    WorldDrawList projection_list{};
    projection_list.display_width = 320;
    projection_list.display_height = 240;
    projection_list.continuous_projection = true;
    auto projection_left = triangle(
        vertex(100, 0, 0, 0x7000),
        vertex(100, 20, 0, 0x7014),
        vertex(80, 0, 0, 0x7028),
        0x80007000,
        10);
    auto projection_right = triangle(
        vertex(100, 0, 0, 0x8000),
        vertex(100, 20, 0, 0x8014),
        vertex(120, 20, 0, 0x8028),
        0x80008000,
        11);
    projection_left.vertices[0].screen_x = 100.20F;
    projection_left.vertices[1].screen_x = 100.20F;
    projection_right.vertices[0].screen_x = 100.80F;
    projection_right.vertices[1].screen_x = 100.80F;
    projection_list.commands.push_back(projection_left);
    projection_list.commands.push_back(projection_right);
    projection_list.track_commands = 2;
    WorldTopologyStats projection_stats{};
    okay &= expect(
        apply_world_topology(
            &projection_list,
            WorldTopologyOptions{false, false, false},
            &projection_stats) == WorldTopologyResult::success,
        "apply authored projection join");
    okay &= expect(
        projection_stats.authored_projection_groups == 2 &&
        projection_stats.adjusted_projection_instances == 2,
        "identify and join the two exact authored boundary endpoints");
    okay &= expect(
        projection_list.commands[0].vertices[0].screen_x ==
            projection_list.commands[1].vertices[0].screen_x &&
        projection_list.commands[0].vertices[1].screen_x ==
            projection_list.commands[1].vertices[1].screen_x,
        "give adjacent authored copies exact projected positions");
    okay &= expect(
        projection_list.commands[0].vertices[2].screen_x == 80.0F &&
        projection_list.commands[1].vertices[2].screen_x == 120.0F,
        "leave non-boundary vertices unchanged");

    // Two proven-adjacent sections may tessellate a shared visible edge with
    // different intermediate 3D vertices. At enhanced resolution their
    // subpixel projection difference exposes the background, so mutually
    // nearest boundary endpoints in the same material layer are joined.
    WorldDrawList seam_list{};
    seam_list.display_width = 320;
    seam_list.display_height = 240;
    seam_list.continuous_projection = true;
    auto seam_left = triangle(
        vertex(100, 0, 0, 0x9000),
        vertex(100, 20, 0, 0x9014),
        vertex(80, 10, 0, 0x9028),
        0x80009000,
        20);
    auto seam_right = triangle(
        vertex(100, 0, 0, 0xA000),
        vertex(100, 20, 0, 0xA014),
        vertex(120, 10, 0, 0xA028),
        0x8000A000,
        21);
    seam_left.vertices[2].screen_x = 90.20F;
    seam_right.vertices[2].screen_x = 90.70F;
    seam_list.commands.push_back(seam_left);
    seam_list.commands.push_back(seam_right);
    seam_list.track_commands = 2;
    WorldTopologyStats seam_stats{};
    okay &= expect(
        apply_world_topology(
            &seam_list,
            WorldTopologyOptions{false, false, false},
            &seam_stats) == WorldTopologyResult::success,
        "apply projected seam join");
    okay &= expect(
        seam_stats.projected_seam_groups == 1 &&
        seam_stats.adjusted_seam_instances == 1 &&
        seam_list.commands[0].vertices[2].screen_x ==
            seam_list.commands[1].vertices[2].screen_x,
        "join a proven subpixel boundary seam");

    // Integer GTE rounding can leave an intermediate vertex slightly off a
    // neighboring triangle's long edge in exact view space. It is still the
    // same authored track object/material/layer, and continuous projection
    // should snap only that vertex's subpixel screen position to the edge.
    WorldDrawList projected_t_list{};
    projected_t_list.display_width = 320;
    projected_t_list.display_height = 240;
    projected_t_list.continuous_projection = true;
    auto projected_t_edge = triangle(
        vertex(0, 0, 0, 0xB000),
        vertex(10, 0, 1, 0xB014),
        vertex(0, 10, 0, 0xB028),
        0x8000B000,
        30);
    auto projected_t_point = triangle(
        vertex(5, 0, 0, 0xC000),
        vertex(5, -5, 0, 0xC014),
        vertex(10, -5, 0, 0xC028),
        0x8000B000,
        31);
    projected_t_edge.object_id = 99;
    projected_t_point.object_id = 99;
    projected_t_point.vertices[0].screen_y = 0.10F;
    projected_t_list.commands.push_back(projected_t_edge);
    projected_t_list.commands.push_back(projected_t_point);
    projected_t_list.track_commands = 2;
    WorldTopologyStats projected_t_stats{};
    okay &= expect(
        apply_world_topology(
            &projected_t_list,
            WorldTopologyOptions{false, false, false},
            &projected_t_stats) == WorldTopologyResult::success,
        "apply projected T-junction join");
    okay &= expect(
        projected_t_stats.projected_t_junctions >= 1 &&
        projected_t_stats.adjusted_projected_t_junction_instances >= 1 &&
        std::fabs(
            projected_t_list.commands[1].vertices[0].screen_y) < 0.001F,
        "close a bounded projected vertex-to-edge T-junction");

    // A GT2 road LOD can describe the same original raster edge at a
    // different view-space scale. Preserve the authored integer SXY as proof
    // that it was a continuous 320x240 boundary, then remove only the
    // sub-half-pixel divergence introduced by continuous reprojection.
    WorldDrawList lod_t_list{};
    lod_t_list.display_width = 320;
    lod_t_list.display_height = 240;
    lod_t_list.continuous_projection = true;
    auto lod_edge = triangle(
        vertex(-418, 197, 1158, 0xD000),
        vertex(-100, 212, 1155, 0xD014),
        vertex(-365, 199, 1981, 0xD028),
        0x8000D000,
        40);
    auto lod_point = triangle(
        vertex(-2069, 1641, 9252, 0xE000),
        // Y is nine guest units from the exact 8x endpoint. This is the
        // measured GT2 road residual that must still prove the LOD relation.
        vertex(-3340, 1585, 9264, 0xE014),
        vertex(-798, 1702, 9240, 0xE028),
        0x8000D000,
        41);
    lod_edge.object_id = 100;
    lod_point.object_id = 100;
    lod_edge.transform_id = 0x1234;
    lod_point.transform_id = 0x1234;
    lod_edge.vertices[0].screen_x = 82.031F;
    lod_edge.vertices[0].screen_y = 396.746F;
    lod_edge.vertices[0].authored_screen_x = 82;
    lod_edge.vertices[0].authored_screen_y = 396;
    lod_edge.vertices[1].screen_x = 141.299F;
    lod_edge.vertices[1].screen_y = 399.647F;
    lod_edge.vertices[1].authored_screen_x = 141;
    lod_edge.vertices[1].authored_screen_y = 399;
    lod_point.vertices[0].screen_x = 111.697F;
    lod_point.vertices[0].screen_y = 398.311F;
    lod_point.vertices[0].authored_screen_x = 111;
    lod_point.vertices[0].authored_screen_y = 398;
    // This endpoint reproduces the measured live road seam divergence:
    // roughly 0.60 native px diagonally from the matching lower-LOD endpoint.
    // It is still safely bounded by the exact 8x view-space relationship and
    // identical authored SXY proof.
    lod_point.vertices[1].screen_x = 82.356F;
    lod_point.vertices[1].screen_y = 397.244F;
    lod_point.vertices[1].authored_screen_x = 82;
    lod_point.vertices[1].authored_screen_y = 396;
    lod_point.vertices[2].screen_x = 141.345F;
    lod_point.vertices[2].screen_y = 399.787F;
    lod_point.vertices[2].authored_screen_x = 141;
    lod_point.vertices[2].authored_screen_y = 399;
    lod_t_list.commands.push_back(lod_edge);
    lod_t_list.commands.push_back(lod_point);
    lod_t_list.track_commands = 2;
    WorldTopologyStats lod_t_stats{};
    okay &= expect(
        apply_world_topology(
            &lod_t_list,
            WorldTopologyOptions{false, false, false},
            &lod_t_stats) == WorldTopologyResult::success,
        "apply projected road LOD join");
    const float lod_dx =
        lod_t_list.commands[0].vertices[1].screen_x -
        lod_t_list.commands[0].vertices[0].screen_x;
    const float lod_dy =
        lod_t_list.commands[0].vertices[1].screen_y -
        lod_t_list.commands[0].vertices[0].screen_y;
    const float lod_px =
        lod_t_list.commands[1].vertices[0].screen_x -
        lod_t_list.commands[0].vertices[0].screen_x;
    const float lod_py =
        lod_t_list.commands[1].vertices[0].screen_y -
        lod_t_list.commands[0].vertices[0].screen_y;
    okay &= expect(
        lod_t_stats.projected_t_junctions >= 1 &&
        lod_t_stats.authored_raster_groups >= 2 &&
        lod_t_stats.adjusted_authored_raster_instances >= 2 &&
        std::fabs(lod_px * lod_dy - lod_py * lod_dx) < 0.001F,
        "close an authored-raster-proven road LOD boundary");

    // At the bottom of Red Rock's 0:18 frame, the native raster keeps two
    // road LOD strips watertight on adjacent authored scanlines. Continuous
    // projection separates them by 0.40 native pixels, enough to expose one
    // high-resolution row even though the 4x view-space relation is exact.
    WorldDrawList scanline_lod_list{};
    scanline_lod_list.display_y = 240;
    scanline_lod_list.display_width = 320;
    scanline_lod_list.display_height = 240;
    scanline_lod_list.continuous_projection = true;
    scanline_lod_list.materials.resize(2);
    scanline_lod_list.materials[0].primitive_flags = 1;
    scanline_lod_list.materials[1].primitive_flags = 1;
    auto scanline_lod_edge = triangle(
        vertex(110, 220, 463, 0xE100),
        vertex(148, 221, 464, 0xE114),
        vertex(144, 220, 626, 0xE128),
        0x800C0304,
        42,
        1);
    auto scanline_lod_point = triangle(
        vertex(445, 884, 1853, 0xE200),
        vertex(453, 882, 1527, 0xE214),
        vertex(520, 884, 1855, 0xE228),
        0x800C0304,
        43);
    scanline_lod_edge.object_id = 21;
    scanline_lod_point.object_id = 21;
    scanline_lod_edge.transform_id = 0x5678;
    scanline_lod_point.transform_id = 0x1234;
    scanline_lod_edge.vertices[0].screen_x = 211.317F;
    scanline_lod_edge.vertices[0].screen_y = 462.635F;
    scanline_lod_edge.vertices[0].authored_screen_x = 211;
    scanline_lod_edge.vertices[0].authored_screen_y = 462;
    scanline_lod_edge.vertices[1].screen_x = 228.897F;
    scanline_lod_edge.vertices[1].screen_y = 462.879F;
    scanline_lod_edge.vertices[1].authored_screen_x = 228;
    scanline_lod_edge.vertices[1].authored_screen_y = 462;
    scanline_lod_point.vertices[0].screen_x = 211.873F;
    scanline_lod_point.vertices[0].screen_y = 463.046F;
    scanline_lod_point.vertices[0].authored_screen_x = 211;
    scanline_lod_point.vertices[0].authored_screen_y = 463;
    scanline_lod_list.commands.push_back(scanline_lod_edge);
    scanline_lod_list.commands.push_back(scanline_lod_point);
    scanline_lod_list.track_commands = 2;
    WorldTopologyStats scanline_lod_stats{};
    const auto scanline_lod_result = apply_world_topology(
            &scanline_lod_list,
            WorldTopologyOptions{false, false, false},
            &scanline_lod_stats);
    okay &= expect(
        scanline_lod_result == WorldTopologyResult::success &&
        scanline_lod_stats.projected_t_junctions >= 1 &&
        std::fabs(
            scanline_lod_list.commands[1].vertices[0].screen_y -
            scanline_lod_list.commands[0].vertices[0].screen_y) < 0.02F,
        "close an adjacent-scanline road LOD boundary");

    // Red Rock turn 1 submits a near road strip and its containing far strip
    // through different transforms and texture pages. Both boundary edges
    // occupy the same integer SXY line in the PS1 raster, but their continuous
    // projections miss by 0.03 native pixels and expose a clear-color row.
    WorldDrawList authored_overlap_list{};
    authored_overlap_list.display_width = 320;
    authored_overlap_list.display_height = 240;
    authored_overlap_list.continuous_projection = true;
    authored_overlap_list.materials.resize(2);
    authored_overlap_list.materials[0].primitive_flags = 1;
    authored_overlap_list.materials[1].primitive_flags = 1;
    auto overlap_outer = triangle(
        vertex(0, 0, 0, 0xF000),
        vertex(100, 0, 0, 0xF014),
        vertex(0, 0, 100, 0xF028),
        0x800C0304,
        50,
        0);
    auto overlap_inner = triangle(
        vertex(40, 0, 200, 0x10000),
        vertex(60, 0, 200, 0x10014),
        vertex(40, 0, 300, 0x10028),
        0x800C0304,
        51,
        1);
    overlap_outer.object_id = 21;
    overlap_inner.object_id = 21;
    overlap_outer.transform_id = 0x1111;
    overlap_inner.transform_id = 0x2222;
    overlap_outer.vertices[0].authored_screen_x = 40;
    overlap_outer.vertices[0].authored_screen_y = 387;
    overlap_outer.vertices[0].screen_x = 40.606F;
    overlap_outer.vertices[0].screen_y = 387.273F;
    overlap_outer.vertices[1].authored_screen_x = 113;
    overlap_outer.vertices[1].authored_screen_y = 387;
    overlap_outer.vertices[1].screen_x = 113.817F;
    overlap_outer.vertices[1].screen_y = 387.180F;
    overlap_outer.vertices[2].authored_screen_x = 108;
    overlap_outer.vertices[2].authored_screen_y = 371;
    overlap_outer.vertices[2].screen_x = 108.290F;
    overlap_outer.vertices[2].screen_y = 371.376F;
    overlap_inner.vertices[0].authored_screen_x = 77;
    overlap_inner.vertices[0].authored_screen_y = 387;
    overlap_inner.vertices[0].screen_x = 77.355F;
    overlap_inner.vertices[0].screen_y = 387.257F;
    overlap_inner.vertices[1].authored_screen_x = 59;
    overlap_inner.vertices[1].authored_screen_y = 387;
    overlap_inner.vertices[1].screen_x = 59.017F;
    overlap_inner.vertices[1].screen_y = 387.265F;
    overlap_inner.vertices[2].authored_screen_x = 33;
    overlap_inner.vertices[2].authored_screen_y = 402;
    overlap_inner.vertices[2].screen_x = 33.257F;
    overlap_inner.vertices[2].screen_y = 402.137F;
    authored_overlap_list.commands.push_back(overlap_outer);
    authored_overlap_list.commands.push_back(overlap_inner);
    authored_overlap_list.track_commands = 2;
    auto rejected_overlap_list = authored_overlap_list;
    rejected_overlap_list.commands[1].model_pointer = 0x800C0404;
    WorldTopologyStats authored_overlap_stats{};
    okay &= expect(
        apply_world_topology(
            &authored_overlap_list,
            WorldTopologyOptions{false, false, false},
            &authored_overlap_stats) == WorldTopologyResult::success,
        "apply authored overlap seam join");
    const auto& joined_outer = authored_overlap_list.commands[0];
    const auto& joined_inner = authored_overlap_list.commands[1];
    const float overlap_dx =
        joined_outer.vertices[1].screen_x -
        joined_outer.vertices[0].screen_x;
    const float overlap_dy =
        joined_outer.vertices[1].screen_y -
        joined_outer.vertices[0].screen_y;
    const auto overlap_distance = [&] (const WorldDrawVertex& point) {
        return std::fabs(
            (point.screen_x - joined_outer.vertices[0].screen_x) *
                overlap_dy -
            (point.screen_y - joined_outer.vertices[0].screen_y) *
                overlap_dx);
    };
    okay &= expect(
        authored_overlap_stats.authored_overlap_seam_groups == 1 &&
        authored_overlap_stats.adjusted_authored_overlap_instances == 2 &&
        overlap_distance(joined_inner.vertices[0]) < 0.01F &&
        overlap_distance(joined_inner.vertices[1]) < 0.01F,
        "close a cross-transform/material authored raster seam");
    auto same_layer_overlap_list = authored_overlap_list;
    same_layer_overlap_list.commands[1].transform_id =
        same_layer_overlap_list.commands[0].transform_id;
    same_layer_overlap_list.commands[1].material_index =
        same_layer_overlap_list.commands[0].material_index;
    same_layer_overlap_list.commands[1].vertices[0].screen_y += 0.08F;
    same_layer_overlap_list.commands[1].vertices[1].screen_y += 0.08F;
    WorldTopologyStats same_layer_overlap_stats{};
    okay &= expect(
        apply_world_topology(
            &same_layer_overlap_list,
            WorldTopologyOptions{false, false, false},
            &same_layer_overlap_stats) == WorldTopologyResult::success &&
        same_layer_overlap_stats.authored_overlap_seam_groups == 1 &&
        same_layer_overlap_stats.adjusted_authored_overlap_instances == 2,
        "close a same-layer authored raster seam");
    WorldTopologyStats rejected_overlap_stats{};
    okay &= expect(
        apply_world_topology(
            &rejected_overlap_list,
            WorldTopologyOptions{false, false, false},
            &rejected_overlap_stats) == WorldTopologyResult::success &&
        rejected_overlap_stats.authored_overlap_seam_groups == 0 &&
        rejected_overlap_list.commands[1].vertices[0].screen_y ==
            overlap_inner.vertices[0].screen_y,
        "do not join a matching raster line from a different model");

    if (!okay)
        return 1;
    std::puts("world topology tests passed");
    return 0;
}
