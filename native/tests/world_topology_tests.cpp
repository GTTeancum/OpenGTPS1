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

    if (!okay)
        return 1;
    std::puts("world topology tests passed");
    return 0;
}
