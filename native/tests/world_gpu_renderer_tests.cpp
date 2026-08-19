#include "opengt/world_gpu_renderer.hpp"

#include <array>
#include <cmath>
#include <cstdint>
#include <cstdio>
#include <vector>

namespace {

constexpr std::uint32_t clear_rgba = 0xFF0000FFU;
constexpr std::uint16_t green_555 = 31U << 5U;

bool expect(bool value, const char* message) {
    if (!value)
        std::fprintf(stderr, "FAILED: %s\n", message);
    return value;
}

opengt::render::WorldDrawVertex vertex(
    float clip_x,
    float clip_y,
    std::int16_t model_x,
    std::int16_t model_y,
    std::int16_t model_z
) {
    opengt::render::WorldDrawVertex result{};
    result.clip_x = clip_x;
    result.clip_y = clip_y;
    result.clip_z = 0.5F;
    result.clip_w = 1.0F;
    result.u = 10.0F;
    result.v = 10.0F;
    result.r = result.g = result.b = 128;
    result.model_x = model_x;
    result.model_y = model_y;
    result.model_z = model_z;
    return result;
}

opengt::render::WorldDrawList draw_list(
    std::uint32_t object_kind,
    bool horizontal
) {
    using namespace opengt::render;
    WorldDrawList result{};
    result.display_width = 16;
    result.display_height = 16;
    result.materials.push_back(WorldMaterial{
        1U | 4U,
        2U << 7U,
        0,
        0,
        0,
        0,
        0,
        0,
    });
    WorldDrawCommand command{};
    command.vertices[0] = vertex(
        -1.0F, -1.0F, 0, 0, 0);
    command.vertices[1] = horizontal
        ? vertex(0.0F, 1.0F, 0, 0, 1)
        : vertex(0.0F, 1.0F, 0, 1, 0);
    command.vertices[2] = vertex(
        1.0F, -1.0F, 1, 0, 0);
    command.material_index = 0;
    command.clip_x0 = 0;
    command.clip_y0 = 0;
    command.clip_x1 = 15;
    command.clip_y1 = 15;
    command.object_kind = object_kind;
    command.object_id = 1;
    command.model_pointer = 0x80001000U;
    command.channel = WorldViewChannel::main_view;
    result.commands.push_back(command);
    result.track_commands = object_kind == 1U ? 1U : 0U;
    result.vehicle_commands = object_kind == 2U ? 1U : 0U;
    return result;
}

std::array<std::uint8_t, 4> render_center(
    std::uint32_t object_kind,
    bool horizontal,
    std::uint32_t visible_neighbors
) {
    using namespace opengt::render;
    std::vector<std::uint16_t> vram(1024U * 512U);
    constexpr std::array<std::array<int, 2>, 4> offsets{{
        {{-1, 0}},
        {{1, 0}},
        {{0, -1}},
        {{0, 1}},
    }};
    for (std::uint32_t index = 0; index < visible_neighbors; ++index) {
        const int x = 10 + offsets[index][0];
        const int y = 10 + offsets[index][1];
        vram[static_cast<std::size_t>(y) * 1024U + x] = green_555;
    }
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    WorldGpuRenderStats stats{};
    const auto list = draw_list(object_kind, horizontal);
    reset_world_d3d11_readback(false);
    const auto result = render_world_d3d11(
        list,
        vram.data(),
        vram.size(),
        output.data(),
        output.size(),
        WorldGpuRenderOptions{
            false,
            true,
            false,
            true,
            false,
            1,
            clear_rgba,
        },
        &stats);
    if (result != WorldGpuRenderResult::success || !stats.output_valid) {
        std::fprintf(
            stderr,
            "FAILED: D3D render result=%s outputValid=%u\n",
            world_gpu_render_result_name(result),
            stats.output_valid ? 1U : 0U);
        return {};
    }
    const std::size_t center = (8U * 16U + 8U) * 4U;
    return {
        output[center],
        output[center + 1],
        output[center + 2],
        output[center + 3],
    };
}

bool is_green(const std::array<std::uint8_t, 4>& pixel) {
    return pixel[0] < 16U && pixel[1] > 240U && pixel[2] < 16U;
}

bool is_clear(const std::array<std::uint8_t, 4>& pixel) {
    return pixel[0] > 240U && pixel[1] < 16U && pixel[2] < 16U;
}

// A GT2 rear-view mirror pass submits vehicles through a narrow guest drawing
// area. Build one wheel group whose replacement shell projects far wider than
// that rectangle, then prove the renderer honours the guest clip.
opengt::render::WorldDrawList mirror_wheel_draw_list(
    std::int16_t clip_x1
) {
    using namespace opengt::render;
    constexpr double pi = 3.14159265358979323846;
    constexpr double radius = 300.0;
    constexpr std::int16_t half_width = 60;
    constexpr float projection_plane = 32.0F;
    constexpr float draw_offset = 16.0F;
    constexpr std::int32_t translation_z = 900;
    constexpr int segments = 12;

    WorldDrawList result{};
    result.display_width = 32;
    result.display_height = 32;
    result.camera_transform_id = 1;
    result.materials.push_back(WorldMaterial{});
    const auto tread_vertex = [&] (
        std::int16_t model_x,
        std::int16_t model_y,
        std::int16_t model_z
    ) {
        WorldDrawVertex output{};
        output.model_x = model_x;
        output.model_y = model_y;
        output.model_z = model_z;
        output.view_x = static_cast<float>(model_x);
        output.view_y = static_cast<float>(model_y);
        output.view_z = static_cast<float>(model_z + translation_z);
        output.projection_plane = projection_plane;
        output.draw_offset_x = draw_offset;
        output.draw_offset_y = draw_offset;
        output.screen_x = draw_offset +
            projection_plane * output.view_x / output.view_z;
        output.screen_y = draw_offset +
            projection_plane * output.view_y / output.view_z;
        output.clip_x = output.screen_x / 16.0F - 1.0F;
        output.clip_y = 1.0F - output.screen_y / 16.0F;
        output.clip_z = 0.5F;
        output.clip_w = 1.0F;
        output.r = output.g = output.b = 128;
        return output;
    };
    for (int segment = 0; segment < segments; ++segment) {
        const double angle0 = 2.0 * pi * segment / segments;
        const double angle1 = 2.0 * pi * (segment + 1) / segments;
        const std::int16_t x0 = static_cast<std::int16_t>(
            std::lround(radius * std::cos(angle0)));
        const std::int16_t y0 = static_cast<std::int16_t>(
            std::lround(radius * std::sin(angle0)));
        const std::int16_t x1 = static_cast<std::int16_t>(
            std::lround(radius * std::cos(angle1)));
        const std::int16_t y1 = static_cast<std::int16_t>(
            std::lround(radius * std::sin(angle1)));
        const std::array<std::array<std::int16_t, 3>, 6> corners{{
            {{x0, y0, -half_width}},
            {{x0, y0, half_width}},
            {{x1, y1, half_width}},
            {{x0, y0, -half_width}},
            {{x1, y1, half_width}},
            {{x1, y1, -half_width}},
        }};
        for (int triangle = 0; triangle < 2; ++triangle) {
            WorldDrawCommand command{};
            for (int index = 0; index < 3; ++index) {
                const auto& corner = corners[triangle * 3 + index];
                command.vertices[index] = tread_vertex(
                    corner[0], corner[1], corner[2]);
            }
            command.material_index = 0;
            command.clip_x0 = 0;
            command.clip_y0 = 0;
            command.clip_x1 = clip_x1;
            command.clip_y1 = 31;
            command.object_kind = 2U;
            command.object_id = 7U;
            command.model_pointer = 0x80002000U;
            command.transform_id = 0x1234U;
            command.exact_transform_valid = true;
            command.transform_rotation[0] = 4096;
            command.transform_rotation[4] = 4096;
            command.transform_rotation[8] = 4096;
            command.transform_translation[2] = translation_z;
            command.channel = WorldViewChannel::secondary_view;
            result.commands.push_back(command);
        }
    }
    result.vehicle_commands = static_cast<std::uint32_t>(
        result.commands.size());
    return result;
}

// Returns shell coverage counts as {inside the clip rect, outside it}.
std::array<int, 2> mirror_shell_coverage(std::int16_t clip_x1) {
    using namespace opengt::render;
    std::vector<std::uint16_t> vram(1024U * 512U);
    std::vector<std::uint8_t> output(32U * 32U * 4U);
    WorldGpuRenderStats stats{};
    const auto list = mirror_wheel_draw_list(clip_x1);
    reset_world_d3d11_readback(false);
    const auto result = render_world_d3d11(
        list,
        vram.data(),
        vram.size(),
        output.data(),
        output.size(),
        WorldGpuRenderOptions{
            false,
            true,
            false,
            true,
            false,
            1,
            clear_rgba,
        },
        &stats);
    if (result != WorldGpuRenderResult::success || !stats.output_valid) {
        std::fprintf(
            stderr,
            "FAILED: mirror wheel render result=%s outputValid=%u\n",
            world_gpu_render_result_name(result),
            stats.output_valid ? 1U : 0U);
        return {{-1, -1}};
    }
    std::array<int, 2> coverage{{0, 0}};
    for (int y = 0; y < 32; ++y) {
        for (int x = 0; x < 32; ++x) {
            const std::size_t offset =
                (static_cast<std::size_t>(y) * 32U + x) * 4U;
            const bool shell =
                output[offset] < 16U &&
                output[offset + 1] < 16U &&
                output[offset + 2] < 16U;
            if (!shell)
                continue;
            ++coverage[x <= clip_x1 ? 0 : 1];
        }
    }
    return coverage;
}

} // namespace

int main() {
    bool okay = true;
    okay &= expect(
        is_green(render_center(1U, true, 4U)),
        "repair an isolated transparent texel on an opaque track surface");
    okay &= expect(
        is_clear(render_center(2U, true, 4U)),
        "preserve transparent texels on vehicle geometry");
    okay &= expect(
        is_clear(render_center(1U, false, 4U)),
        "preserve transparent texels on a vertical track surface");
    okay &= expect(
        is_clear(render_center(1U, true, 2U)),
        "preserve an authored track-texture cutout");
    const auto clipped = mirror_shell_coverage(15);
    okay &= expect(
        clipped[0] > 0,
        "draw a mirror wheel shell inside the guest drawing area");
    okay &= expect(
        clipped[1] == 0,
        "clip a mirror wheel shell to the guest drawing area");
    const auto unclipped = mirror_shell_coverage(31);
    okay &= expect(
        unclipped[0] > clipped[0],
        "keep the full shell when the drawing area covers the display");
    if (!okay)
        return 1;
    std::puts("world GPU renderer tests passed");
    return 0;
}
