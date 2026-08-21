#include "opengt/world_interpolation.hpp"

#include <array>
#include <cmath>
#include <cstdio>
#include <utility>

namespace {

bool expect(bool value, const char* message) {
    if (!value)
        std::fprintf(stderr, "FAILED: %s\n", message);
    return value;
}

opengt::render::WorldDrawVertex vertex(
    float screen_x,
    float screen_y,
    std::uint32_t identity,
    std::int16_t model_x
) {
    opengt::render::WorldDrawVertex result{};
    result.screen_x = screen_x;
    result.screen_y = screen_y;
    result.clip_w = 100.0F;
    result.clip_z = 50.0F;
    result.source_vertex_identity = identity;
    result.provenance_flags = 1;
    result.model_x = model_x;
    result.r = result.g = result.b = 128;
    return result;
}

opengt::render::WorldDrawCommand command(
    std::uint32_t kind,
    std::uint32_t object,
    float offset,
    std::uint32_t source = 0
) {
    opengt::render::WorldDrawCommand result{};
    result.vertices[0] = vertex(offset, 20.0F, 0x1000, 0);
    result.vertices[1] = vertex(offset + 10.0F, 20.0F, 0x1010, 10);
    result.vertices[2] = vertex(offset, 30.0F, 0x1020, 20);
    result.object_kind = kind;
    result.object_id = object;
    result.model_pointer = 0x80001000;
    result.material_index = 0;
    result.source_command_index = source;
    result.channel = opengt::render::WorldViewChannel::main_view;
    return result;
}

opengt::render::WorldDrawList list() {
    opengt::render::WorldDrawList result{};
    result.display_width = 320;
    result.display_height = 240;
    result.materials.resize(1);
    return result;
}

opengt::render::WorldDrawVertex rigid_vertex(
    std::int16_t model_x,
    std::int16_t model_y,
    float view_x,
    float view_y
) {
    auto result = vertex(0.0F, 0.0F, 0x2000U +
        static_cast<std::uint16_t>(model_x) * 31U +
        static_cast<std::uint16_t>(model_y), model_x);
    result.model_y = model_y;
    result.view_x = view_x;
    result.view_y = view_y;
    result.view_z = 1000.0F;
    result.exact_view_x = static_cast<std::int32_t>(std::lround(view_x));
    result.exact_view_y = static_cast<std::int32_t>(std::lround(view_y));
    result.exact_view_z = 1000;
    result.projection_offset_x = 160.0F * 65536.0F;
    result.projection_offset_y = 120.0F * 65536.0F;
    result.projection_plane = 256.0F;
    result.screen_x = 160.0F + 256.0F * view_x / 1000.0F;
    result.screen_y = 120.0F + 256.0F * view_y / 1000.0F;
    result.clip_w = 1000.0F;
    result.clip_z = 984.0F;
    return result;
}

opengt::render::WorldDrawCommand rigid_command(
    const opengt::render::WorldDrawVertex& a,
    const opengt::render::WorldDrawVertex& b,
    const opengt::render::WorldDrawVertex& c,
    std::uint64_t transform,
    std::uint32_t source
) {
    opengt::render::WorldDrawCommand result{};
    result.vertices[0] = a;
    result.vertices[1] = b;
    result.vertices[2] = c;
    result.object_kind = 2;
    result.object_id = 17;
    result.model_pointer = 0x80002000;
    result.transform_id = transform;
    result.material_index = 0;
    result.source_command_index = source;
    result.channel = opengt::render::WorldViewChannel::main_view;
    return result;
}

void set_exact_transform(
    opengt::render::WorldDrawCommand* command,
    const std::int16_t rotation[9],
    std::int32_t translate_x,
    std::int32_t translate_y,
    std::int32_t translate_z
) {
    command->exact_transform_valid = true;
    for (int component = 0; component < 9; ++component)
        command->transform_rotation[component] = rotation[component];
    command->transform_translation[0] = translate_x;
    command->transform_translation[1] = translate_y;
    command->transform_translation[2] = translate_z;
}

void set_vertex_exact_transform(
    opengt::render::WorldDrawVertex* vertex,
    std::uint64_t transform_id,
    const std::int16_t rotation[9],
    std::int32_t translate_x,
    std::int32_t translate_y,
    std::int32_t translate_z
) {
    vertex->transform_id = transform_id;
    vertex->exact_transform_valid = true;
    for (int component = 0; component < 9; ++component)
        vertex->transform_rotation[component] = rotation[component];
    vertex->transform_translation[0] = translate_x;
    vertex->transform_translation[1] = translate_y;
    vertex->transform_translation[2] = translate_z;
    const float model_x = vertex->model_x;
    const float model_y = vertex->model_y;
    const float model_z = vertex->model_z;
    vertex->view_x =
        (rotation[0] * model_x + rotation[1] * model_y +
            rotation[2] * model_z) / 4096.0F + translate_x;
    vertex->view_y =
        (rotation[3] * model_x + rotation[4] * model_y +
            rotation[5] * model_z) / 4096.0F + translate_y;
    vertex->view_z =
        (rotation[6] * model_x + rotation[7] * model_y +
            rotation[8] * model_z) / 4096.0F + translate_z;
    vertex->exact_view_x = static_cast<std::int32_t>(
        std::lround(vertex->view_x));
    vertex->exact_view_y = static_cast<std::int32_t>(
        std::lround(vertex->view_y));
    vertex->exact_view_z = static_cast<std::int32_t>(
        std::lround(vertex->view_z));
    vertex->screen_x = 160.0F +
        256.0F * vertex->view_x / vertex->view_z;
    vertex->screen_y = 120.0F +
        256.0F * vertex->view_y / vertex->view_z;
    vertex->clip_w = vertex->view_z;
    vertex->clip_z = vertex->view_z - 16.0F;
}

} // namespace

int main() {
    using namespace opengt::render;
    bool okay = true;
    auto previous = list();
    previous.commands.push_back(command(2, 7, 10.0F));
    previous.vehicle_commands = 1;
    auto current = list();
    // Reorder triangle vertices and change the capture command index. Neither
    // is a temporal identity; authored vertex provenance is.
    auto moved = command(2, 7, 14.0F, 99);
    std::swap(moved.vertices[0], moved.vertices[2]);
    current.commands.push_back(moved);
    current.vehicle_commands = 1;

    WorldDrawList midpoint{};
    WorldInterpolationStats stats{};
    okay &= expect(
        interpolate_world_draw_lists(
            previous, current, 0.5F, &midpoint, &stats) ==
            WorldInterpolationResult::success,
        "interpolate provenance-matched geometry");
    okay &= expect(
        stats.matched_commands == 1 &&
        stats.matched_vehicle_commands == 1,
        "report one matched vehicle command");
    okay &= expect(
        std::fabs(midpoint.commands[0].vertices[0].screen_x - 12.0F) <
            0.001F,
        "move the original silhouette to the geometric midpoint");
    okay &= expect(
        midpoint.commands.size() == previous.commands.size(),
        "do not duplicate the moving command");

    auto appearing = current;
    appearing.commands.push_back(command(2, 8, 100.0F));
    okay &= expect(
        interpolate_world_draw_lists(
            previous, appearing, 0.5F, &midpoint, &stats) ==
            WorldInterpolationResult::success &&
        midpoint.commands.size() == 1,
        "hold visibility atomically until the current authored state");

    auto vanished = list();
    okay &= expect(
        interpolate_world_draw_lists(
            previous, vanished, 0.5F, &midpoint, &stats) ==
            WorldInterpolationResult::success &&
        midpoint.commands.size() == 1 &&
        stats.held_unmatched_commands == 1,
        "retain a disappearing command for the temporal midpoint");

    auto unclassified_previous = list();
    auto unclassified_flare = command(0, 0, 10.0F);
    unclassified_flare.model_pointer = 0;
    unclassified_flare.transform_id = 0x1234;
    unclassified_previous.commands.push_back(unclassified_flare);
    auto unclassified_current = list();
    auto moved_unclassified_flare = command(0, 0, 14.0F);
    moved_unclassified_flare.model_pointer = 0;
    moved_unclassified_flare.transform_id = 0x5678;
    unclassified_current.commands.push_back(moved_unclassified_flare);
    okay &= expect(
        interpolate_world_draw_lists(
            unclassified_previous,
            unclassified_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.matched_commands == 1 &&
        stats.held_screen_commands == 0 &&
        std::fabs(midpoint.commands[0].vertices[0].screen_x - 12.0F) <
            0.001F,
        "interpolate transformed unclassified world effects");

    auto projected_previous = list();
    projected_previous.commands.push_back(
        rigid_command(
            rigid_vertex(0, 0, 100.0F, -100.0F),
            rigid_vertex(100, 0, 140.0F, -100.0F),
            rigid_vertex(0, 100, 100.0F, -60.0F),
            0,
            26));
    projected_previous.commands[0].object_kind = 0;
    projected_previous.commands[0].object_id = 0;
    projected_previous.commands[0].model_pointer = 0;
    projected_previous.commands[0].transform_id = 0;
    auto projected_current = list();
    projected_current.commands.push_back(
        rigid_command(
            rigid_vertex(0, 0, 100.0F, -90.0F),
            rigid_vertex(100, 0, 140.0F, -90.0F),
            rigid_vertex(0, 100, 100.0F, -50.0F),
            0,
            26));
    projected_current.commands[0].object_kind = 0;
    projected_current.commands[0].object_id = 0;
    projected_current.commands[0].model_pointer = 0;
    projected_current.commands[0].transform_id = 0;
    okay &= expect(
        interpolate_world_draw_lists(
            projected_previous,
            projected_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.matched_commands == 1 &&
        stats.held_unmatched_commands == 0 &&
        std::fabs(midpoint.commands[0].vertices[0].screen_y - 95.68F) <
            0.001F,
        "interpolate no-transform projected world fragments");

    auto hud_previous = list();
    hud_previous.materials[0].primitive_flags =
        world_primitive_screen_space_flag;
    hud_previous.commands.push_back(command(0, 0, 40.0F));
    auto hud_current = hud_previous;
    hud_current.commands[0].vertices[0].screen_x = 80.0F;
    okay &= expect(
        interpolate_world_draw_lists(
            hud_previous, hud_current, 0.5F, &midpoint, &stats) ==
            WorldInterpolationResult::success &&
        midpoint.commands[0].vertices[0].screen_x == 40.0F &&
        stats.held_screen_commands == 1,
        "hold HUD pixels instead of cross-fading or morphing them");

    auto background_previous = list();
    background_previous.materials[0].primitive_flags =
        world_primitive_screen_space_flag;
    background_previous.commands.push_back(command(0, 0, 0.0F));
    background_previous.commands[0].vertices[0].screen_x = 0.0F;
    background_previous.commands[0].vertices[0].screen_y = 70.0F;
    background_previous.commands[0].vertices[1].screen_x = 0.0F;
    background_previous.commands[0].vertices[1].screen_y = 240.0F;
    background_previous.commands[0].vertices[2].screen_x = 320.0F;
    background_previous.commands[0].vertices[2].screen_y = 70.0F;
    background_previous.commands[0].clip_x0 = 0;
    background_previous.commands[0].clip_y0 = 0;
    background_previous.commands[0].clip_x1 = 319;
    background_previous.commands[0].clip_y1 = 239;
    auto background_current = background_previous;
    background_current.commands[0].vertices[0].screen_y = 76.0F;
    background_current.commands[0].vertices[2].screen_y = 76.0F;
    okay &= expect(
        interpolate_world_draw_lists(
            background_previous,
            background_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        std::fabs(midpoint.commands[0].vertices[0].screen_y - 73.0F) <
            0.001F &&
        stats.matched_commands == 1 &&
        stats.held_screen_commands == 0,
        "interpolate large untextured screen-space background fills");

    auto screen_effect_previous = list();
    screen_effect_previous.materials[0].primitive_flags =
        world_primitive_screen_space_flag | 1U;
    screen_effect_previous.commands.push_back(command(0, 0, 40.0F));
    screen_effect_previous.commands[0].model_pointer = 0;
    auto screen_effect_current = screen_effect_previous;
    for (auto& screen_vertex : screen_effect_current.commands[0].vertices)
        screen_vertex.screen_x += 8.0F;
    okay &= expect(
        interpolate_world_draw_lists(
            screen_effect_previous,
            screen_effect_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        std::fabs(midpoint.commands[0].vertices[0].screen_x - 44.0F) <
            0.001F &&
        stats.matched_commands == 1 &&
        stats.held_screen_commands == 0,
        "interpolate matched textured screen-space world effects");

    auto wrong_viewport = current;
    wrong_viewport.display_width = 640;
    okay &= expect(
        interpolate_world_draw_lists(
            previous, wrong_viewport, 0.5F, &midpoint, &stats) ==
            WorldInterpolationResult::incompatible_viewport,
        "reject a transition between incompatible viewports");

    // A 120-degree authored wheel step is deliberately beyond the range
    // where independent vertex lerp can preserve a circular mesh: its
    // midpoint contracts to half size.  Temporal interpolation must recover
    // the rigid transform and retain the full diagonal.
    auto rotating_previous = list();
    const auto pa = rigid_vertex(-100, -100, -100.0F, -100.0F);
    const auto pb = rigid_vertex(100, -100, 100.0F, -100.0F);
    const auto pc = rigid_vertex(100, 100, 100.0F, 100.0F);
    const auto pd = rigid_vertex(-100, 100, -100.0F, 100.0F);
    rotating_previous.commands.push_back(
        rigid_command(pa, pb, pc, 0x1111, 0));
    rotating_previous.commands.push_back(
        rigid_command(pa, pc, pd, 0x1111, 1));
    rotating_previous.vehicle_commands = 2;
    auto rotating_current = list();
    constexpr float cosine120 = -0.5F;
    constexpr float sine120 = 0.8660254038F;
    const auto rotate120 = [&](std::int16_t x, std::int16_t y) {
        return rigid_vertex(
            x,
            y,
            20.0F + cosine120 * x - sine120 * y,
            -10.0F + sine120 * x + cosine120 * y);
    };
    const auto ca = rotate120(-100, -100);
    const auto cb = rotate120(100, -100);
    const auto cc = rotate120(100, 100);
    const auto cd = rotate120(-100, 100);
    rotating_current.commands.push_back(
        rigid_command(ca, cb, cc, 0x2222, 0));
    rotating_current.commands.push_back(
        rigid_command(ca, cc, cd, 0x2222, 1));
    rotating_current.vehicle_commands = 2;
    okay &= expect(
        interpolate_world_draw_lists(
            rotating_previous,
            rotating_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success,
        "interpolate a rotating rigid vehicle group");
    const auto& midpoint_a = midpoint.commands[0].vertices[0];
    const auto& midpoint_c = midpoint.commands[0].vertices[2];
    const float regular_midpoint_screen_y = midpoint_a.screen_y;
    const float midpoint_diagonal = std::hypot(
        midpoint_c.view_x - midpoint_a.view_x,
        midpoint_c.view_y - midpoint_a.view_y);
    okay &= expect(
        std::fabs(midpoint_diagonal - std::sqrt(80000.0F)) < 0.1F,
        "preserve wheel dimensions through a large rotational midpoint");
    okay &= expect(
        std::fabs(
            (midpoint_c.view_x - midpoint_a.view_x) + 73.2051F) < 0.1F &&
        std::fabs(
            (midpoint_c.view_y - midpoint_a.view_y) - 273.2051F) < 0.1F,
        "sample a genuine half-angle wheel pose without duplicating a frame");

    // Production v5 captures carry the exact GTE object matrix. Coherent
    // endpoint vertices must use that transform directly instead of a second
    // least-squares reconstruction of the submitted triangles.
    auto exact_previous = rotating_previous;
    auto exact_current = list();
    constexpr std::int16_t identity_rotation[9] = {
        4096, 0, 0,
        0, 4096, 0,
        0, 0, 4096,
    };
    constexpr std::int16_t quarter_turn_rotation[9] = {
        0, -4096, 0,
        4096, 0, 0,
        0, 0, 4096,
    };
    for (auto& exact_command : exact_previous.commands)
        set_exact_transform(
            &exact_command, identity_rotation, 0, 0, 1000);
    const auto quarter_turn = [&](std::int16_t x, std::int16_t y) {
        return rigid_vertex(x, y, 20.0F - y, -10.0F + x);
    };
    const auto exact_ca = quarter_turn(-100, -100);
    const auto exact_cb = quarter_turn(100, -100);
    const auto exact_cc = quarter_turn(100, 100);
    const auto exact_cd = quarter_turn(-100, 100);
    exact_current.commands.push_back(
        rigid_command(exact_ca, exact_cb, exact_cc, 0x2222, 0));
    exact_current.commands.push_back(
        rigid_command(exact_ca, exact_cc, exact_cd, 0x2222, 1));
    exact_current.vehicle_commands = 2;
    for (auto& exact_command : exact_current.commands) {
        set_exact_transform(
            &exact_command, quarter_turn_rotation, 20, -10, 1000);
    }
    okay &= expect(
        interpolate_world_draw_lists(
            exact_previous,
            exact_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success,
        "interpolate exact guest wheel transforms");
    const auto& exact_midpoint_a = midpoint.commands[0].vertices[0];
    okay &= expect(
        std::fabs(exact_midpoint_a.view_x - 10.0F) < 0.2F &&
        std::fabs(exact_midpoint_a.view_y + 146.4214F) < 0.2F &&
        std::fabs(exact_midpoint_a.view_z - 1000.0F) < 0.2F,
        "derive the wheel midpoint from guest RT instead of vertex fitting");
    okay &= expect(
        midpoint.commands[0].exact_transform_valid &&
        std::abs(midpoint.commands[0].transform_rotation[0] - 2896) <= 1 &&
        std::abs(midpoint.commands[0].transform_rotation[1] + 2896) <= 1 &&
        std::abs(midpoint.commands[0].transform_rotation[3] - 2896) <= 1 &&
        midpoint.commands[0].transform_translation[0] == 10 &&
        midpoint.commands[0].transform_translation[1] == -5 &&
        midpoint.commands[0].transform_translation[2] == 1000,
        "publish the sampled guest RT with midpoint wheel commands");

    // Retail GT2 wheel matrices include model scale in the captured GTE
    // transform. A unit-quaternion conversion used to discard this 1/16
    // scale and turn every synthetic wheel into a screen-sized polygon.
    auto scaled_previous = list();
    auto scaled_current = list();
    constexpr std::int16_t scaled_identity[9] = {
        256, 0, 0,
        0, 256, 0,
        0, 0, 256,
    };
    constexpr std::int16_t scaled_quarter_turn[9] = {
        0, -256, 0,
        256, 0, 0,
        0, 0, 256,
    };
    const auto scaled_start = [&](std::int16_t x, std::int16_t y) {
        return rigid_vertex(x, y, x / 16.0F, y / 16.0F);
    };
    const auto scaled_end = [&](std::int16_t x, std::int16_t y) {
        return rigid_vertex(
            x, y, 20.0F - y / 16.0F, -10.0F + x / 16.0F);
    };
    const auto scaled_pa = scaled_start(-100, -100);
    const auto scaled_pb = scaled_start(100, -100);
    const auto scaled_pc = scaled_start(100, 100);
    const auto scaled_pd = scaled_start(-100, 100);
    scaled_previous.commands.push_back(
        rigid_command(scaled_pa, scaled_pb, scaled_pc, 0x1111, 0));
    scaled_previous.commands.push_back(
        rigid_command(scaled_pa, scaled_pc, scaled_pd, 0x1111, 1));
    scaled_previous.vehicle_commands = 2;
    const auto scaled_ca = scaled_end(-100, -100);
    const auto scaled_cb = scaled_end(100, -100);
    const auto scaled_cc = scaled_end(100, 100);
    const auto scaled_cd = scaled_end(-100, 100);
    scaled_current.commands.push_back(
        rigid_command(scaled_ca, scaled_cb, scaled_cc, 0x2222, 0));
    scaled_current.commands.push_back(
        rigid_command(scaled_ca, scaled_cc, scaled_cd, 0x2222, 1));
    scaled_current.vehicle_commands = 2;
    for (auto& scaled_command : scaled_previous.commands)
        set_exact_transform(
            &scaled_command, scaled_identity, 0, 0, 1000);
    for (auto& scaled_command : scaled_current.commands)
        set_exact_transform(
            &scaled_command, scaled_quarter_turn, 20, -10, 1000);
    okay &= expect(
        interpolate_world_draw_lists(
            scaled_previous,
            scaled_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success,
        "interpolate scaled guest wheel transforms");
    const auto& scaled_midpoint_a = midpoint.commands[0].vertices[0];
    const auto& scaled_midpoint_c = midpoint.commands[0].vertices[2];
    const float scaled_midpoint_diagonal = std::hypot(
        scaled_midpoint_c.view_x - scaled_midpoint_a.view_x,
        scaled_midpoint_c.view_y - scaled_midpoint_a.view_y);
    okay &= expect(
        std::fabs(scaled_midpoint_diagonal -
            std::sqrt(80000.0F) / 16.0F) < 0.1F &&
        std::fabs(scaled_midpoint_a.view_x - 10.0F) < 0.2F &&
        std::fabs(scaled_midpoint_a.view_y + 13.8388F) < 0.2F,
        "preserve captured wheel scale at the rotational midpoint");
    okay &= expect(
        std::abs(midpoint.commands[0].transform_rotation[0] - 181) <= 1 &&
        std::abs(midpoint.commands[0].transform_rotation[1] + 181) <= 1 &&
        std::abs(midpoint.commands[0].transform_rotation[3] - 181) <= 1,
        "publish the scaled midpoint matrix without unit expansion");

    // Near-plane clipping can leave a tiny vehicle fragment carrying a
    // copied exact-transform flag even though its generated model coordinates
    // no longer reproduce its captured view coordinates. The SSR11 white-car
    // failure used an identity transform on such a fragment, expanding clean
    // endpoint pixels into a screen-sized wheel wedge at the midpoint.
    auto clipped_fragment_previous = list();
    const auto fragment_pa = rigid_vertex(10000, 10000, 100.0F, 40.0F);
    const auto fragment_pb = rigid_vertex(10100, 10000, 110.0F, 40.0F);
    const auto fragment_pc = rigid_vertex(10100, 10100, 110.0F, 50.0F);
    const auto fragment_pd = rigid_vertex(10000, 10100, 100.0F, 50.0F);
    clipped_fragment_previous.commands.push_back(
        rigid_command(fragment_pa, fragment_pb, fragment_pc, 0x3333, 0));
    clipped_fragment_previous.commands.push_back(
        rigid_command(fragment_pa, fragment_pc, fragment_pd, 0x3333, 1));
    clipped_fragment_previous.vehicle_commands = 2;
    auto clipped_fragment_current = list();
    const auto fragment_current = [&](std::int16_t x, std::int16_t y) {
        return rigid_vertex(
            x,
            y,
            102.0F + (x - 10000) * 0.1F,
            41.0F + (y - 10000) * 0.1F);
    };
    const auto fragment_ca = fragment_current(10000, 10000);
    const auto fragment_cb = fragment_current(10100, 10000);
    const auto fragment_cc = fragment_current(10100, 10100);
    const auto fragment_cd = fragment_current(10000, 10100);
    clipped_fragment_current.commands.push_back(
        rigid_command(fragment_ca, fragment_cb, fragment_cc, 0x3333, 0));
    clipped_fragment_current.commands.push_back(
        rigid_command(fragment_ca, fragment_cc, fragment_cd, 0x3333, 1));
    clipped_fragment_current.vehicle_commands = 2;
    for (auto& fragment_command : clipped_fragment_previous.commands)
        set_exact_transform(
            &fragment_command, identity_rotation, 0, 0, 0);
    for (auto& fragment_command : clipped_fragment_current.commands)
        set_exact_transform(
            &fragment_command, identity_rotation, 0, 0, 0);
    okay &= expect(
        interpolate_world_draw_lists(
            clipped_fragment_previous,
            clipped_fragment_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.incoherent_exact_transform_groups == 1 &&
        stats.held_incoherent_vehicle_commands == 2,
        "interpolate a clipped vehicle fragment with an incoherent matrix");
    bool fragment_held =
        midpoint.commands.size() == clipped_fragment_previous.commands.size();
    for (std::size_t command_index = 0;
         command_index < midpoint.commands.size();
         ++command_index) {
        const auto& midpoint_command = midpoint.commands[command_index];
        const auto& previous_command =
            clipped_fragment_previous.commands[command_index];
        fragment_held = fragment_held &&
            midpoint_command.exact_transform_valid;
        for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
            fragment_held = fragment_held &&
                midpoint_command.vertices[vertex_index].screen_x ==
                    previous_command.vertices[vertex_index].screen_x &&
                midpoint_command.vertices[vertex_index].screen_y ==
                    previous_command.vertices[vertex_index].screen_y;
        }
    }
    okay &= expect(
        fragment_held,
        "reject an incoherent exact transform and hold the fragment intact");

    auto atomic_vehicle_previous = exact_previous;
    atomic_vehicle_previous.commands.insert(
        atomic_vehicle_previous.commands.end(),
        clipped_fragment_previous.commands.begin(),
        clipped_fragment_previous.commands.end());
    atomic_vehicle_previous.vehicle_commands = 4;
    auto atomic_vehicle_current = exact_current;
    atomic_vehicle_current.commands.insert(
        atomic_vehicle_current.commands.end(),
        clipped_fragment_current.commands.begin(),
        clipped_fragment_current.commands.end());
    atomic_vehicle_current.vehicle_commands = 4;
    okay &= expect(
        interpolate_world_draw_lists(
            atomic_vehicle_previous,
            atomic_vehicle_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.incoherent_exact_transform_groups == 1 &&
        stats.held_incoherent_vehicle_commands == 2 &&
        stats.held_atomic_vehicle_commands == 2 &&
        stats.matched_vehicle_commands == 0 &&
        stats.held_unmatched_commands == 4,
        "hold every sibling group when one vehicle group is incoherent");
    bool atomic_vehicle_held =
        midpoint.commands.size() == atomic_vehicle_previous.commands.size();
    for (std::size_t command_index = 0;
         command_index < midpoint.commands.size();
         ++command_index) {
        for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
            atomic_vehicle_held = atomic_vehicle_held &&
                midpoint.commands[command_index].vertices[vertex_index]
                        .screen_x ==
                    atomic_vehicle_previous.commands[command_index]
                        .vertices[vertex_index].screen_x &&
                midpoint.commands[command_index].vertices[vertex_index]
                        .screen_y ==
                    atomic_vehicle_previous.commands[command_index]
                        .vertices[vertex_index].screen_y;
        }
    }
    okay &= expect(
        atomic_vehicle_held,
        "restore a mixed vehicle to one coherent authored pose");

    auto clipped_current = rotating_current;
    clipped_current.commands.pop_back();
    clipped_current.vehicle_commands = 1;
    okay &= expect(
        interpolate_world_draw_lists(
            rotating_previous,
            clipped_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.matched_vehicle_commands == 2 &&
        stats.held_unmatched_commands == 0,
        "move every triangle in a valid rigid group through clipping churn");
    const auto& clipped_midpoint_a = midpoint.commands[1].vertices[0];
    const auto& clipped_matched_a = midpoint.commands[0].vertices[0];
    okay &= expect(
        std::fabs(
            clipped_midpoint_a.view_x - clipped_matched_a.view_x) < 0.1F &&
        std::fabs(
            clipped_midpoint_a.view_y - clipped_matched_a.view_y) < 0.1F,
        "keep unmatched rigid-group vertices coherent with matched geometry");

    // Track visibility changes at 30 Hz even when the camera transform is
    // continuous. Hold the complete connected section when any triangle is
    // clipped; moving only the surviving triangles tears its road markings,
    // while translating the section can tear its boundary with its neighbors.
    auto track_previous = rotating_previous;
    auto track_current = rotating_current;
    track_current.commands.pop_back();
    for (auto& track_command : track_previous.commands)
        track_command.object_kind = 1;
    for (auto& track_command : track_current.commands)
        track_command.object_kind = 1;
    track_previous.vehicle_commands = 0;
    track_previous.track_commands = 2;
    track_current.vehicle_commands = 0;
    track_current.track_commands = 1;
    okay &= expect(
        interpolate_world_draw_lists(
            track_previous,
            track_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.matched_track_commands == 0 &&
        stats.held_unmatched_commands == 2 &&
        stats.held_track_visibility_commands == 2,
        "hold a track section atomically through visibility churn");
    okay &= expect(
        midpoint.commands[0].vertices[0].screen_x ==
            track_previous.commands[0].vertices[0].screen_x &&
        midpoint.commands[1].vertices[0].screen_x ==
            track_previous.commands[1].vertices[0].screen_x,
        "preserve every triangle in a churning track section");

    // A coherent captured camera matrix can move an entire track section,
    // including a triangle that leaves the authored visibility list. This
    // avoids duplicating the road/prop pose at the synthetic 60 Hz midpoint.
    auto exact_track_previous = exact_previous;
    auto exact_track_current = exact_current;
    exact_track_current.commands.pop_back();
    for (auto& exact_track_command : exact_track_previous.commands)
        exact_track_command.object_kind = 1;
    for (auto& exact_track_command : exact_track_current.commands)
        exact_track_command.object_kind = 1;
    exact_track_previous.vehicle_commands = 0;
    exact_track_previous.track_commands = 2;
    exact_track_current.vehicle_commands = 0;
    exact_track_current.track_commands = 1;
    okay &= expect(
        interpolate_world_draw_lists(
            exact_track_previous,
            exact_track_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.exact_rigid_transform_groups == 1 &&
        stats.matched_track_commands == 2 &&
        stats.held_track_visibility_commands == 0 &&
        stats.held_unsafe_track_commands == 0,
        "move a churning track section with its coherent guest transform");
    okay &= expect(
        std::fabs(midpoint.commands[1].vertices[2].view_x + 131.4214F) <
            0.2F &&
        std::fabs(midpoint.commands[1].vertices[2].view_y + 5.0F) < 0.2F,
        "give the visibility-churn triangle a unique rigid midpoint pose");

    // Some track connector packets combine vertices emitted under different
    // GTE transforms. A command-level matrix describes only its first vertex;
    // v6 retains each vertex matrix so the complete connected section can move
    // through visibility churn without fitting, tearing, or a 30 Hz hold.
    auto mixed_transform_previous = list();
    auto mixed_transform_current = list();
    auto mixed_a = rigid_vertex(0, 0, 0.0F, 0.0F);
    auto mixed_b = rigid_vertex(100, 0, 100.0F, 0.0F);
    auto mixed_c = rigid_vertex(0, 100, 0.0F, 100.0F);
    auto mixed_d = rigid_vertex(100, 100, 100.0F, 100.0F);
    auto mixed_current_a = mixed_a;
    auto mixed_current_b = mixed_b;
    auto mixed_current_c = mixed_c;
    auto mixed_current_d = mixed_d;
    set_vertex_exact_transform(
        &mixed_a, 0xA1, identity_rotation, 0, 0, 1000);
    set_vertex_exact_transform(
        &mixed_b, 0xA1, identity_rotation, 0, 0, 1000);
    set_vertex_exact_transform(
        &mixed_c, 0xB1, identity_rotation, 200, 0, 1000);
    set_vertex_exact_transform(
        &mixed_d, 0xB1, identity_rotation, 200, 0, 1000);
    set_vertex_exact_transform(
        &mixed_current_a, 0xA2, identity_rotation, 20, 0, 1000);
    set_vertex_exact_transform(
        &mixed_current_b, 0xA2, identity_rotation, 20, 0, 1000);
    set_vertex_exact_transform(
        &mixed_current_c, 0xB2, identity_rotation, 240, 20, 1000);
    set_vertex_exact_transform(
        &mixed_current_d, 0xB2, identity_rotation, 240, 20, 1000);
    mixed_transform_previous.commands.push_back(
        rigid_command(mixed_a, mixed_b, mixed_c, 0xA1, 0));
    mixed_transform_previous.commands.push_back(
        rigid_command(mixed_b, mixed_d, mixed_c, 0xA1, 1));
    mixed_transform_current.commands.push_back(
        rigid_command(
            mixed_current_a,
            mixed_current_b,
            mixed_current_c,
            0xA2,
            0));
    for (auto& mixed_command : mixed_transform_previous.commands) {
        mixed_command.object_kind = 1;
        set_exact_transform(
            &mixed_command, identity_rotation, 0, 0, 1000);
    }
    for (auto& mixed_command : mixed_transform_current.commands) {
        mixed_command.object_kind = 1;
        set_exact_transform(
            &mixed_command, identity_rotation, 20, 0, 1000);
    }
    mixed_transform_previous.track_commands = 2;
    mixed_transform_current.track_commands = 1;
    okay &= expect(
        interpolate_world_draw_lists(
            mixed_transform_previous,
            mixed_transform_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.incoherent_exact_transform_groups == 1 &&
        stats.exact_rigid_transform_groups == 1 &&
        stats.matched_track_commands == 2 &&
        stats.held_track_visibility_commands == 0 &&
        stats.held_unsafe_track_commands == 0,
        "move a mixed-transform connector through visibility churn");
    okay &= expect(
        std::fabs(midpoint.commands[1].vertices[1].view_x - 320.0F) <
            0.1F &&
        std::fabs(midpoint.commands[1].vertices[1].view_y - 110.0F) <
            0.1F &&
        !midpoint.commands[1].exact_transform_valid,
        "apply the correct per-vertex midpoint transform to the connector");

    auto unsafe_mixed_previous = mixed_transform_previous;
    auto unsafe_mixed_current = mixed_transform_current;
    for (auto& mixed_command : unsafe_mixed_previous.commands) {
        for (auto& mixed_vertex : mixed_command.vertices) {
            if (mixed_vertex.transform_id == 0xB1) {
                set_vertex_exact_transform(
                    &mixed_vertex,
                    0xB1,
                    identity_rotation,
                    200,
                    0,
                    80);
            }
        }
    }
    for (auto& mixed_command : unsafe_mixed_current.commands) {
        for (auto& mixed_vertex : mixed_command.vertices) {
            if (mixed_vertex.transform_id == 0xB2) {
                set_vertex_exact_transform(
                    &mixed_vertex,
                    0xB2,
                    identity_rotation,
                    240,
                    20,
                    20);
            }
        }
    }
    const auto unsafe_mixed_result = interpolate_world_draw_lists(
            unsafe_mixed_previous,
            unsafe_mixed_current,
            0.5F,
            &midpoint,
            &stats);
    okay &= expect(
        unsafe_mixed_result == WorldInterpolationResult::success &&
        stats.matched_track_commands == 0 &&
        stats.held_unmatched_commands == 2 &&
        stats.held_track_visibility_commands == 0 &&
        stats.held_unsafe_track_commands == 2,
        "hold an unsafe mixed-transform connector atomically");
    okay &= expect(
        midpoint.commands[0].vertices[0].screen_x ==
            unsafe_mixed_previous.commands[0].vertices[0].screen_x &&
        midpoint.commands[1].vertices[1].screen_y ==
            unsafe_mixed_previous.commands[1].vertices[1].screen_y,
        "keep every mixed-transform neighbor authored after unsafe preflight");

    auto view_delta_previous = list();
    auto view_delta_current = list();
    auto delta_a = rigid_vertex(0, 0, 0.0F, 0.0F);
    auto delta_b = rigid_vertex(100, 0, 100.0F, 0.0F);
    auto delta_c = rigid_vertex(0, 100, 0.0F, 100.0F);
    auto delta_current_a = delta_a;
    auto delta_current_b = delta_b;
    auto delta_current_c = delta_c;
    set_vertex_exact_transform(
        &delta_a, 0xD1, identity_rotation, 0, 0, 1000);
    set_vertex_exact_transform(
        &delta_b, 0xD1, identity_rotation, 0, 0, 1000);
    set_vertex_exact_transform(
        &delta_c, 0xD1, identity_rotation, 0, 0, 1000);
    set_vertex_exact_transform(
        &delta_current_a, 0xD2, identity_rotation, 20, 10, 960);
    set_vertex_exact_transform(
        &delta_current_b, 0xD2, identity_rotation, 20, 10, 960);
    set_vertex_exact_transform(
        &delta_current_c, 0xD2, identity_rotation, 20, 10, 960);
    auto pretransformed_a = rigid_vertex(300, 200, 300.0F, 200.0F);
    auto pretransformed_b = rigid_vertex(400, 200, 400.0F, 200.0F);
    auto pretransformed_c = rigid_vertex(300, 300, 300.0F, 300.0F);
    for (auto* pretransformed : {
             &pretransformed_a,
             &pretransformed_b,
             &pretransformed_c}) {
        pretransformed->model_z = 1000;
        set_vertex_exact_transform(
            pretransformed,
            0xE1,
            identity_rotation,
            0,
            0,
            0);
    }
    view_delta_previous.commands.push_back(
        rigid_command(delta_a, delta_b, delta_c, 0xD1, 0));
    view_delta_previous.commands.push_back(rigid_command(
        pretransformed_a,
        pretransformed_b,
        pretransformed_c,
        0xD1,
        1));
    view_delta_current.commands.push_back(rigid_command(
        delta_current_a,
        delta_current_b,
        delta_current_c,
        0xD2,
        0));
    for (auto& delta_command : view_delta_previous.commands) {
        delta_command.object_kind = 1;
        set_exact_transform(
            &delta_command, identity_rotation, 0, 0, 1000);
    }
    for (auto& delta_command : view_delta_current.commands) {
        delta_command.object_kind = 1;
        set_exact_transform(
            &delta_command, identity_rotation, 20, 10, 960);
    }
    view_delta_previous.track_commands = 2;
    view_delta_current.track_commands = 1;
    okay &= expect(
        interpolate_world_draw_lists(
            view_delta_previous,
            view_delta_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.vertex_exact_track_groups == 1 &&
        stats.matched_track_commands == 2 &&
        stats.held_track_visibility_commands == 0 &&
        stats.missing_track_vertex_candidates == 0,
        "move a pretransformed connector with its authored scale-band delta");
    okay &= expect(
        std::fabs(midpoint.commands[1].vertices[0].view_x - 310.0F) <
            0.1F &&
        std::fabs(midpoint.commands[1].vertices[0].view_y - 205.0F) <
            0.1F &&
        std::fabs(midpoint.commands[1].vertices[0].view_z - 980.0F) <
            0.1F,
        "give an unmatched pretransformed vertex unique camera motion");

    auto screen_track_previous = view_delta_previous;
    auto screen_track_current = view_delta_current;
    screen_track_previous.materials[0].primitive_flags =
        opengt::render::world_primitive_screen_space_flag | 1U;
    screen_track_current.materials[0].primitive_flags =
        opengt::render::world_primitive_screen_space_flag | 1U;
    okay &= expect(
        interpolate_world_draw_lists(
            screen_track_previous,
            screen_track_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.matched_track_commands == 2 &&
        stats.held_screen_commands == 0 &&
        std::fabs(midpoint.commands[1].vertices[0].view_x - 310.0F) <
            0.1F,
        "move clipped screen-space track fallback with the camera delta");

    // Converted track objects can use a different fixed-point scale from
    // pretransformed camera vertices, and the secondary view can move in the
    // opposite direction. Keep those delta populations isolated by channel
    // and normalize translation back to the unmatched vertex's scale band.
    const std::int16_t double_scale_rotation[9]{
        8192, 0, 0,
        0, 8192, 0,
        0, 0, 8192,
    };
    auto scaled_channel_previous = view_delta_previous;
    auto scaled_channel_current = view_delta_current;
    for (auto& scaled_vertex :
         scaled_channel_previous.commands[0].vertices) {
        set_vertex_exact_transform(
            &scaled_vertex,
            0xD1,
            double_scale_rotation,
            0,
            0,
            1000);
    }
    set_exact_transform(
        &scaled_channel_previous.commands[0],
        double_scale_rotation,
        0,
        0,
        1000);
    for (auto& scaled_vertex :
         scaled_channel_current.commands[0].vertices) {
        set_vertex_exact_transform(
            &scaled_vertex,
            0xD2,
            double_scale_rotation,
            40,
            20,
            920);
    }
    set_exact_transform(
        &scaled_channel_current.commands[0],
        double_scale_rotation,
        40,
        20,
        920);

    auto secondary_previous_anchor =
        scaled_channel_previous.commands[0];
    auto secondary_previous_unmatched =
        scaled_channel_previous.commands[1];
    auto secondary_current_anchor = scaled_channel_current.commands[0];
    for (auto* secondary_command : {
             &secondary_previous_anchor,
             &secondary_previous_unmatched,
             &secondary_current_anchor}) {
        secondary_command->object_id = 18;
        secondary_command->model_pointer = 0x80003000;
        secondary_command->channel = WorldViewChannel::secondary_view;
    }
    for (auto& scaled_vertex : secondary_current_anchor.vertices) {
        set_vertex_exact_transform(
            &scaled_vertex,
            0xD2,
            double_scale_rotation,
            -40,
            -20,
            1080);
    }
    set_exact_transform(
        &secondary_current_anchor,
        double_scale_rotation,
        -40,
        -20,
        1080);
    scaled_channel_previous.commands.push_back(
        secondary_previous_anchor);
    scaled_channel_previous.commands.push_back(
        secondary_previous_unmatched);
    scaled_channel_current.commands.push_back(secondary_current_anchor);
    scaled_channel_previous.track_commands = 4;
    scaled_channel_current.track_commands = 2;
    okay &= expect(
        interpolate_world_draw_lists(
            scaled_channel_previous,
            scaled_channel_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.vertex_exact_track_groups == 2 &&
        stats.matched_track_commands == 4 &&
        stats.held_track_visibility_commands == 0 &&
        stats.missing_track_vertex_candidates == 0,
        "reconstruct converted visibility candidates per view channel");
    okay &= expect(
        std::fabs(midpoint.commands[1].vertices[0].view_x - 310.0F) <
            0.1F &&
        std::fabs(midpoint.commands[1].vertices[0].view_z - 980.0F) <
            0.1F &&
        std::fabs(midpoint.commands[3].vertices[0].view_x - 290.0F) <
            0.1F &&
        std::fabs(midpoint.commands[3].vertices[0].view_z - 1020.0F) <
            0.1F,
        "scale opposing channel deltas into the authored camera band");

    // Exact endpoint matrices are not sufficient on their own: a coherent
    // section can cross the projection's protected near depth between them.
    // Reject the complete connected group if any midpoint triangle is unsafe.
    auto near_exact_previous = exact_track_previous;
    auto near_exact_current = exact_track_current;
    const auto set_exact_depth = [](
        WorldDrawCommand* exact_command,
        const std::int16_t rotation[9],
        std::int32_t translate_x,
        std::int32_t translate_y,
        std::int32_t translate_z
    ) {
        set_exact_transform(
            exact_command,
            rotation,
            translate_x,
            translate_y,
            translate_z);
        for (auto& exact_vertex : exact_command->vertices) {
            const float model_x = exact_vertex.model_x;
            const float model_y = exact_vertex.model_y;
            const float model_z = exact_vertex.model_z;
            exact_vertex.view_x =
                (rotation[0] * model_x + rotation[1] * model_y +
                    rotation[2] * model_z) /
                    4096.0F +
                translate_x;
            exact_vertex.view_y =
                (rotation[3] * model_x + rotation[4] * model_y +
                    rotation[5] * model_z) /
                    4096.0F +
                translate_y;
            exact_vertex.view_z =
                (rotation[6] * model_x + rotation[7] * model_y +
                    rotation[8] * model_z) /
                    4096.0F +
                translate_z;
            exact_vertex.exact_view_x = static_cast<std::int32_t>(
                std::lround(exact_vertex.view_x));
            exact_vertex.exact_view_y = static_cast<std::int32_t>(
                std::lround(exact_vertex.view_y));
            exact_vertex.exact_view_z = translate_z;
            exact_vertex.screen_x = 160.0F +
                256.0F * exact_vertex.view_x / exact_vertex.view_z;
            exact_vertex.screen_y = 120.0F +
                256.0F * exact_vertex.view_y / exact_vertex.view_z;
            exact_vertex.clip_w = exact_vertex.view_z;
            exact_vertex.clip_z = exact_vertex.view_z - 16.0F;
        }
    };

    auto oversized_previous = exact_track_previous;
    auto oversized_current = exact_track_current;
    for (auto* oversized_list : {&oversized_previous, &oversized_current}) {
        for (auto& oversized_command : oversized_list->commands) {
            for (auto& oversized_vertex : oversized_command.vertices) {
                oversized_vertex.model_x = static_cast<std::int16_t>(
                    oversized_vertex.model_x * 16);
                oversized_vertex.model_y = static_cast<std::int16_t>(
                    oversized_vertex.model_y * 16);
            }
        }
    }
    for (auto& oversized_command : oversized_previous.commands) {
        set_exact_depth(
            &oversized_command, identity_rotation, 0, 0, 1000);
    }
    for (auto& oversized_command : oversized_current.commands) {
        set_exact_depth(
            &oversized_command, identity_rotation, 20, 10, 960);
    }
    okay &= expect(
        interpolate_world_draw_lists(
            oversized_previous,
            oversized_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.matched_track_commands == 2 &&
        stats.held_unsafe_track_commands == 0,
        "move a modestly growing authored track polygon beyond the viewport");

    // A finite exact visibility candidate can move farther than one display
    // width while crossing the viewport edge. D3D clips that triangle; holding
    // the connected group instead exposes the clear buffer beside the road.
    auto shifted_visibility_previous = view_delta_previous;
    auto shifted_visibility_current = view_delta_current;
    for (auto& shifted_vertex :
         shifted_visibility_previous.commands[1].vertices) {
        shifted_vertex.model_z = 100;
        set_vertex_exact_transform(
            &shifted_vertex,
            0xE1,
            identity_rotation,
            0,
            0,
            0);
    }
    set_exact_depth(
        &shifted_visibility_current.commands[0],
        identity_rotation,
        20,
        10,
        940);
    for (auto& shifted_vertex :
         shifted_visibility_current.commands[0].vertices) {
        set_vertex_exact_transform(
            &shifted_vertex,
            0xD2,
            identity_rotation,
            20,
            10,
            940);
    }
    okay &= expect(
        interpolate_world_draw_lists(
            shifted_visibility_previous,
            shifted_visibility_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.vertex_exact_track_groups == 1 &&
        stats.matched_track_commands == 2 &&
        stats.held_track_visibility_commands == 0 &&
        stats.held_unsafe_track_commands == 0,
        "accept a finite exact visibility candidate crossing the viewport");
    okay &= expect(
        std::fabs(
            midpoint.commands[1].vertices[0].screen_x -
            shifted_visibility_previous.commands[1].vertices[0].screen_x) >
            shifted_visibility_previous.display_width &&
        midpoint.commands[1].vertices[0].view_z > 64.0F,
        "retain the large finite visibility-candidate motion for clipping");

    auto expanding_previous = exact_track_previous;
    auto expanding_current = exact_track_current;
    for (auto* expanding_list : {&expanding_previous, &expanding_current}) {
        for (auto& expanding_command : expanding_list->commands) {
            for (auto& expanding_vertex : expanding_command.vertices) {
                expanding_vertex.model_x = static_cast<std::int16_t>(
                    expanding_vertex.model_x * 10);
                expanding_vertex.model_y = static_cast<std::int16_t>(
                    expanding_vertex.model_y * 10);
            }
        }
    }
    for (auto& expanding_command : expanding_previous.commands) {
        set_exact_depth(
            &expanding_command, identity_rotation, 0, 0, 1000);
    }
    for (auto& expanding_command : expanding_current.commands) {
        set_exact_depth(
            &expanding_command, identity_rotation, 0, 0, 560);
    }
    okay &= expect(
        interpolate_world_draw_lists(
            expanding_previous,
            expanding_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.matched_track_commands == 0 &&
        stats.held_unsafe_track_commands == 2 &&
        stats.held_unsafe_track_span_commands == 2,
        "hold a newly overexpanded track midpoint atomically");

    for (auto& near_command : near_exact_previous.commands) {
        set_exact_depth(
            &near_command, identity_rotation, 0, 0, 80);
    }
    for (auto& near_command : near_exact_current.commands) {
        set_exact_depth(
            &near_command, quarter_turn_rotation, 20, -10, 20);
    }
    okay &= expect(
        interpolate_world_draw_lists(
            near_exact_previous,
            near_exact_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.exact_rigid_transform_groups == 1 &&
        stats.matched_track_commands == 0 &&
        stats.held_unmatched_commands == 2 &&
        stats.held_track_visibility_commands == 0 &&
        stats.held_unsafe_track_commands == 2,
        "reject an exact track midpoint inside the protected near depth");
    okay &= expect(
        midpoint.commands[0].vertices[0].screen_x ==
            near_exact_previous.commands[0].vertices[0].screen_x &&
        midpoint.commands[1].vertices[2].screen_y ==
            near_exact_previous.commands[1].vertices[2].screen_y,
        "roll unsafe exact track triangles back to their authored pose");

    auto mixed_depth_previous = exact_previous;
    auto mixed_depth_current = exact_current;
    for (auto* mixed_list : {&mixed_depth_previous, &mixed_depth_current}) {
        for (auto& mixed_command : mixed_list->commands)
            mixed_command.object_kind = 1;
        mixed_list->vehicle_commands = 0;
        mixed_list->track_commands = 2;
        for (auto& far_vertex : mixed_list->commands[1].vertices)
            far_vertex.model_z = 1000;
    }
    for (auto& mixed_command : mixed_depth_previous.commands) {
        set_exact_depth(
            &mixed_command, identity_rotation, 0, 0, 80);
    }
    for (auto& mixed_command : mixed_depth_current.commands) {
        set_exact_depth(
            &mixed_command, quarter_turn_rotation, 20, -10, 20);
    }
    okay &= expect(
        interpolate_world_draw_lists(
            mixed_depth_previous,
            mixed_depth_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.matched_track_commands == 0 &&
        stats.held_unmatched_commands == 2 &&
        stats.held_unsafe_track_commands == 2,
        "hold a complete exact track group when only one triangle is unsafe");
    okay &= expect(
        midpoint.commands[1].vertices[0].view_z ==
            mixed_depth_previous.commands[1].vertices[0].view_z,
        "keep a safe neighbor authored when its exact track group is unsafe");

    // The same atomic hold prevents a fitted camera transform from expanding
    // a clipped near-plane triangle into a screen-sized polygon.
    auto explosive_previous = track_previous;
    auto explosive_current = track_current;
    for (auto& vertex : explosive_previous.commands[1].vertices) {
        vertex.view_z = 20.0F;
        vertex.exact_view_z = 20;
        vertex.screen_x = 160.0F +
            256.0F * vertex.view_x / vertex.view_z;
        vertex.screen_y = 120.0F +
            256.0F * vertex.view_y / vertex.view_z;
    }
    okay &= expect(
        interpolate_world_draw_lists(
            explosive_previous,
            explosive_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.matched_track_commands == 0 &&
        stats.held_unmatched_commands == 2,
        "hold a near-plane track section without reprojecting it");
    okay &= expect(
        midpoint.commands[1].vertices[0].screen_x ==
            explosive_previous.commands[1].vertices[0].screen_x &&
        midpoint.commands[1].vertices[1].screen_x ==
            explosive_previous.commands[1].vertices[1].screen_x,
        "prevent a clipped track triangle from exploding across the frame");

    auto anchored_previous = list();
    auto anchored_current = list();
    auto make_anchored = [](float offset) {
        auto result = command(1, 29, offset);
        for (auto& anchored_vertex : result.vertices) {
            anchored_vertex.model_x = 100;
            anchored_vertex.model_y = 200;
            anchored_vertex.model_z = 300;
            anchored_vertex.provenance_flags =
                world_vertex_source_identity_flag |
                world_vertex_screen_offset_anchor_flag;
        }
        return result;
    };
    anchored_previous.commands.push_back(make_anchored(10.0F));
    anchored_previous.commands.push_back(make_anchored(100.0F));
    anchored_previous.track_commands = 2;
    anchored_current.commands.push_back(make_anchored(104.0F));
    anchored_current.commands.push_back(make_anchored(14.0F));
    anchored_current.track_commands = 2;
    okay &= expect(
        interpolate_world_draw_lists(
            anchored_previous,
            anchored_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.matched_track_commands == 2,
        "match reordered screen-offset anchors as authored commands");
    okay &= expect(
        std::fabs(midpoint.commands[0].vertices[0].screen_x - 12.0F) < 0.01F &&
        std::fabs(midpoint.commands[1].vertices[0].screen_x - 102.0F) < 0.01F,
        "interpolate each billboard against its nearest matching anchor");

    // Two GT triangles can share the same authored SXY edge while separate
    // transform groups advance their continuous midpoint projections by a
    // fraction of a pixel. Preserve the original triangles and cover only
    // that proven temporal strip with an internal affine seam primitive.
    auto temporal_seam_previous = list();
    auto temporal_seam_current = list();
    const auto make_temporal_seam_command = [] (
        std::uint32_t source,
        bool reverse_edge,
        float edge_y
    ) {
        auto result = command(1, 31, 0.0F, source);
        result.ordering_table_index = 5;
        const std::array<std::pair<float, float>, 3> points = reverse_edge
            ? std::array<std::pair<float, float>, 3>{
                  std::pair{80.0F, edge_y},
                  std::pair{40.0F, edge_y},
                  std::pair{80.0F, 10.0F},
              }
            : std::array<std::pair<float, float>, 3>{
                  std::pair{40.0F, edge_y},
                  std::pair{80.0F, edge_y},
                  std::pair{40.0F, 70.0F},
              };
        const std::array<std::pair<std::int32_t, std::int32_t>, 3>
            authored = reverse_edge
                ? std::array<std::pair<std::int32_t, std::int32_t>, 3>{
                      std::pair{80, 40},
                      std::pair{40, 40},
                      std::pair{80, 10},
                  }
                : std::array<std::pair<std::int32_t, std::int32_t>, 3>{
                      std::pair{40, 40},
                      std::pair{80, 40},
                      std::pair{40, 70},
                  };
        for (std::size_t index = 0; index < 3; ++index) {
            auto& seam_vertex = result.vertices[index];
            seam_vertex.screen_x = points[index].first;
            seam_vertex.screen_y = points[index].second;
            seam_vertex.authored_screen_x = authored[index].first;
            seam_vertex.authored_screen_y = authored[index].second;
            seam_vertex.source_vertex_identity =
                source * 16U + static_cast<std::uint32_t>(index);
            seam_vertex.provenance_flags =
                world_vertex_source_identity_flag |
                world_vertex_screen_offset_anchor_flag;
        }
        return result;
    };
    temporal_seam_previous.commands.push_back(
        make_temporal_seam_command(100, false, 40.0F));
    temporal_seam_previous.commands.push_back(
        make_temporal_seam_command(101, true, 40.0F));
    temporal_seam_previous.track_commands = 2;
    temporal_seam_current.commands.push_back(
        make_temporal_seam_command(100, false, 40.4F));
    temporal_seam_current.commands.push_back(
        make_temporal_seam_command(101, true, 40.0F));
    temporal_seam_current.track_commands = 2;
    okay &= expect(
        interpolate_world_draw_lists(
            temporal_seam_previous,
            temporal_seam_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        stats.temporal_track_seam_triangles == 2 &&
        midpoint.commands.size() == 4,
        "stitch a topology-proven divergent authored track edge");
    okay &= expect(
        std::fabs(midpoint.commands[0].vertices[0].screen_y - 40.2F) <
                0.001F &&
        std::fabs(midpoint.commands[1].vertices[0].screen_y - 40.0F) <
                0.001F,
        "leave the surrounding track triangles at their true midpoints");
    okay &= expect(
        midpoint.commands[2].material_index < midpoint.materials.size() &&
        midpoint.commands[3].material_index < midpoint.materials.size() &&
        (midpoint.materials[
             midpoint.commands[2].material_index].primitive_flags &
            world_primitive_temporal_seam_flag) != 0 &&
        (midpoint.materials[
             midpoint.commands[3].material_index].primitive_flags &
            world_primitive_temporal_seam_flag) != 0,
        "tag temporal seams so the modern renderer keeps affine fallback");

    auto field_previous = rotating_previous;
    field_previous.display_y = 240;
    for (auto& field_command : field_previous.commands) {
        for (auto& field_vertex : field_command.vertices) {
            field_vertex.screen_y += 240.0F;
            field_vertex.draw_offset_y = 240.0F;
        }
    }
    auto field_current = rotating_current;
    field_current.display_y = 0;
    okay &= expect(
        interpolate_world_draw_lists(
            field_previous,
            field_current,
            0.5F,
            &midpoint,
            &stats) == WorldInterpolationResult::success &&
        std::fabs(
            midpoint.commands[0].vertices[0].screen_y -
            midpoint.display_y - regular_midpoint_screen_y) < 0.01F,
        "normalize alternating field origins before reprojecting motion");

    auto cached_previous = rotating_previous;
    auto cached_current = rotating_current;
    WorldInterpolationCache previous_cache;
    WorldInterpolationCache current_cache;
    WorldDrawList cached_midpoint{};
    WorldInterpolationStats cached_stats{};
    okay &= expect(
        interpolate_world_draw_lists_cached(
            cached_previous,
            cached_current,
            &previous_cache,
            &current_cache,
            0.5F,
            &cached_midpoint,
            &cached_stats) == WorldInterpolationResult::success &&
        cached_stats.previous_group_cache_hit == 0 &&
        cached_stats.current_group_cache_hit == 0,
        "build reusable interpolation indexes on their first pair");

    cached_previous = std::move(cached_current);
    previous_cache = std::move(current_cache);
    cached_current = rotating_current;
    for (auto& cached_command : cached_current.commands) {
        for (auto& cached_vertex : cached_command.vertices)
            cached_vertex.screen_x += 1.0F;
    }
    okay &= expect(
        interpolate_world_draw_lists_cached(
            cached_previous,
            cached_current,
            &previous_cache,
            &current_cache,
            0.5F,
            &cached_midpoint,
            &cached_stats) == WorldInterpolationResult::success &&
        cached_stats.previous_group_cache_hit == 1 &&
        cached_stats.current_group_cache_hit == 0,
        "reuse a current-frame index after its draw list becomes previous");
    WorldDrawList uncached_midpoint{};
    WorldInterpolationStats uncached_stats{};
    okay &= expect(
        interpolate_world_draw_lists(
            cached_previous,
            cached_current,
            0.5F,
            &uncached_midpoint,
            &uncached_stats) == WorldInterpolationResult::success &&
        cached_midpoint.commands.size() == uncached_midpoint.commands.size() &&
        std::fabs(
            cached_midpoint.commands[0].vertices[0].screen_x -
            uncached_midpoint.commands[0].vertices[0].screen_x) < 0.001F,
        "preserve interpolation output when reusing a moved draw-list index");

    if (!okay)
        return 1;
    std::printf("world interpolation tests passed\n");
    return 0;
}
