#include "opengt/world_gpu_renderer.hpp"

#include <array>
#include <cmath>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
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
        ? vertex(0.0F, 1.0F, 0, 1, 0)
        : vertex(0.0F, 1.0F, 0, 0, 1);
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
    result.background_commands = object_kind == 3U ? 1U : 0U;
    result.unclassified_commands =
        object_kind != 1U && object_kind != 2U && object_kind != 3U
            ? 1U
            : 0U;
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

bool resident_billboard_uses_metric_depth(bool tree_first, bool tree_nearer,
    bool screen_offset_flare = false) {
    using namespace opengt::render;
    WorldCaptureHeader header{};
    header.camera_transform_id = 1;
    header.display_width = header.display_height = 16;
    header.projection_offset_x = header.projection_offset_y = 8 << 16;
    header.projection_plane = 8;
    std::array<WorldCaptureTriangle, 2> triangles{};
    for (int object = 0; object < 2; ++object) {
        const bool tree = object == 1;
        auto& triangle = triangles[tree_first ? 1 - object : object];
        triangle.object_kind = 1;
        triangle.object_id = object + 1;
        triangle.clip_x1 = triangle.clip_y1 = 15;
        triangle.depth_scale_valid = true;
        triangle.depth_scale_exponent = tree ? 11 : 8;
        triangle.primitive_flags = tree && screen_offset_flare
            ? 2U : world_primitive_resident_course_flag;
        if (tree) {
            if (!screen_offset_flare)
                triangle.primitive_flags |= world_primitive_track_billboard_depth_flag;
            triangle.ordering_table_index = 168;
        }
        const int z = tree ? (tree_nearer ? 1000 : 16364) : 15818;
        constexpr int xy[3][2]{{-1, 1}, {0, -1}, {1, 1}};
        for (int index = 0; index < 3; ++index) {
            auto& point = triangle.vertices[index];
            point.world_valid = true;
            point.screen_offset_anchor = tree && screen_offset_flare;
            point.view_x = xy[index][0] * z;
            point.view_y = xy[index][1] * z;
            point.view_z = z;
            point.model_x = static_cast<std::int16_t>(point.view_x);
            point.model_y = static_cast<std::int16_t>(point.view_y);
            point.model_z = static_cast<std::int16_t>(z);
            point.world_x = static_cast<float>(point.view_x);
            point.world_y = static_cast<float>(point.view_y);
            point.world_z = static_cast<float>(z);
            point.projection_offset_x = point.projection_offset_y = 8 << 16;
            point.projection_plane = 8;
            point.r = tree ? 0 : 192;
            point.g = tree ? 192 : 0;
        }
    }
    WorldDrawList list{};
    if (build_world_draw_list(header, triangles.data(), triangles.size(),
            WorldDrawListOptions{false, false, true, true}, &list) !=
        WorldDrawListResult::success || list.commands.size() != 2) {
        return false;
    }
    std::vector<std::uint16_t> vram(1024U * 512U);
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    WorldGpuRenderStats stats{};
    reset_world_d3d11_readback(false);
    if (render_world_d3d11(list, vram.data(), vram.size(), output.data(), output.size(),
            WorldGpuRenderOptions{false, true, false, true, false, false, 1, clear_rgba},
            &stats) != WorldGpuRenderResult::success || !stats.output_valid)
        return false;
    const std::size_t center = (8U * 16U + 8U) * 4U;
    return tree_nearer
        ? (screen_offset_flare
            ? output[center] < 120 && output[center + 1] > 80
            : output[center] < 16 && output[center + 1] > 150)
        : output[center] > 150 && output[center + 1] < 16;
}

bool fully_behind_world_triangle_is_rejected_before_submit() {
    using namespace opengt::render;
    WorldDrawList list = draw_list(1U, true);
    for (auto& vertex : list.commands[0].vertices) {
        vertex.clip_z = 16.0F;
        vertex.clip_w = -32.0F;
    }
    std::vector<std::uint16_t> vram(1024U * 512U, green_555);
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    WorldGpuRenderStats stats{};
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
            false,
            1,
            clear_rgba,
        },
        &stats);
    return result == WorldGpuRenderResult::success &&
        stats.output_valid && stats.draw_calls == 0;
}

bool fully_outside_world_triangle_is_rejected_before_submit() {
    using namespace opengt::render;
    WorldDrawList list = draw_list(1U, true);
    // One point is in front of the camera and two are behind it, but every
    // point remains outside the right homogeneous clip plane. This is the
    // camera-straddling empty primitive that must not become a reflected
    // screen sliver on the D3D rasterizer.
    list.commands[0].vertices[0].clip_x = 4.0F;
    list.commands[0].vertices[0].clip_z = 0.5F;
    list.commands[0].vertices[0].clip_w = 1.0F;
    for (std::size_t index = 1; index < 3; ++index) {
        list.commands[0].vertices[index].clip_x = 4.0F;
        list.commands[0].vertices[index].clip_z = 0.5F;
        list.commands[0].vertices[index].clip_w = -1.0F;
    }
    std::vector<std::uint16_t> vram(1024U * 512U, green_555);
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    WorldGpuRenderStats stats{};
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
            false,
            1,
            clear_rgba,
        },
        &stats);
    return result == WorldGpuRenderResult::success &&
        stats.output_valid && stats.draw_calls == 0;
}

std::array<std::uint8_t, 4> render_connected_terrain_zero_texel(
    bool smooth_connection
) {
    using namespace opengt::render;
    WorldDrawList list{};
    list.display_width = 16;
    list.display_height = 16;
    list.materials.push_back(WorldMaterial{
        1U | 4U | world_primitive_resident_course_flag,
        2U << 7U,
        0,
        0,
        0,
        0,
        0,
        0,
    });
    const auto point = [] (
        float clip_x,
        float clip_y,
        std::int16_t model_x,
        std::int16_t model_y,
        std::int16_t model_z
    ) {
        WorldDrawVertex result = vertex(
            clip_x, clip_y, model_x, model_y, model_z);
        result.world_x = static_cast<float>(model_x);
        result.world_y = static_cast<float>(model_y);
        result.world_z = static_cast<float>(model_z);
        return result;
    };
    const auto append = [&list] (
        const std::array<WorldDrawVertex, 3>& points
    ) {
        WorldDrawCommand command{};
        for (std::size_t index = 0; index < points.size(); ++index)
            command.vertices[index] = points[index];
        command.material_index = 0;
        command.clip_x0 = command.clip_y0 = 0;
        command.clip_x1 = command.clip_y1 = 15;
        command.object_kind = 1;
        command.object_id = 7;
        command.model_pointer = 0x80002000U;
        command.transform_id = 0x1122334455667788ULL;
        command.channel = WorldViewChannel::main_view;
        list.commands.push_back(command);
    };
    // The first triangle is an ordinary ground seed. The second shares its
    // exact authored edge but is steep enough that normal-only classification
    // would mistake it for a keyed fence. A smooth fold is one continuous
    // terrain island; a ninety-degree fold remains an independent cutout.
    const WorldDrawVertex a = point(-1.0F, -1.0F, 0, 100, 0);
    const WorldDrawVertex b = point(-1.0F, 1.0F, 0, 0, 0);
    const WorldDrawVertex c = point(1.0F, -1.0F, 100, 0, 0);
    const WorldDrawVertex d = smooth_connection
        ? point(1.0F, 1.0F, 0, 60, 80)
        : point(1.0F, 1.0F, 0, 0, 100);
    append({a, b, c});
    append({c, b, d});
    list.track_commands = 2;

    std::vector<std::uint16_t> vram(1024U * 512U);
    constexpr std::array<std::array<int, 2>, 4> offsets{{
        {{-1, 0}}, {{1, 0}}, {{0, -1}}, {{0, 1}},
    }};
    for (const auto& offset : offsets) {
        vram[static_cast<std::size_t>(10 + offset[1]) * 1024U +
            10 + offset[0]] = green_555;
    }
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    WorldGpuRenderStats stats{};
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
            false,
            1,
            clear_rgba,
        },
        &stats);
    if (result != WorldGpuRenderResult::success || !stats.output_valid)
        return {};
    const std::size_t sample = (4U * 16U + 12U) * 4U;
    return {
        output[sample],
        output[sample + 1],
        output[sample + 2],
        output[sample + 3],
    };
}

bool horizontal_plus_reveals_world_outside_guest_edge() {
    using namespace opengt::render;
    WorldDrawList list{};
    list.display_width = 16;
    list.display_height = 16;
    list.materials.push_back(WorldMaterial{});
    WorldDrawCommand command{};
    command.vertices[0] = vertex(1.05F, -0.5F, 0, 0, 0);
    command.vertices[1] = vertex(1.55F, -0.5F, 1, 0, 0);
    command.vertices[2] = vertex(1.05F, 0.5F, 0, 1, 0);
    command.material_index = 0;
    command.clip_x0 = command.clip_y0 = 0;
    command.clip_x1 = command.clip_y1 = 15;
    command.object_kind = 1;
    command.object_id = 1;
    command.channel = WorldViewChannel::main_view;
    list.commands.push_back(command);
    list.track_commands = 1;

    WorldGpuRenderOptions options{
        false,
        true,
        false,
        false,
        false,
        false,
        1,
        clear_rgba,
    };
    options.target_aspect_width = 16;
    options.target_aspect_height = 9;
    const std::uint32_t width = world_gpu_target_display_width(list, options);
    std::vector<std::uint16_t> vram(1024U * 512U);
    std::vector<std::uint8_t> output(
        static_cast<std::size_t>(width) * 16U * 4U);
    WorldGpuRenderStats stats{};
    reset_world_d3d11_readback(false);
    if (render_world_d3d11(
            list,
            vram.data(),
            vram.size(),
            output.data(),
            output.size(),
            options,
            &stats) != WorldGpuRenderResult::success ||
        !stats.output_valid)
        return false;

    // The original 16-pixel view is centred in the 29-pixel Hor+ target and
    // ends before x=23. This triangle begins beyond the original right plane.
    for (std::uint32_t y = 0; y < 16; ++y) {
        for (std::uint32_t x = 23; x < width; ++x) {
            const std::size_t offset =
                (static_cast<std::size_t>(y) * width + x) * 4U;
            if (output[offset] < 240U || output[offset + 1] > 16U ||
                output[offset + 2] > 16U)
                return true;
        }
    }
    return false;
}

bool horizontal_plus_anchors_hud_groups_to_margins(
    std::uint32_t aspect_width,
    std::uint32_t aspect_height
) {
    using namespace opengt::render;
    constexpr std::int32_t guest_width = 16;
    constexpr std::int32_t guest_height = 16;
    WorldDrawList list{};
    list.display_width = guest_width;
    list.display_height = guest_height;
    list.materials.push_back(WorldMaterial{
        world_primitive_screen_space_flag,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
    });
    const auto screen_vertex = [=] (
        float x,
        float y,
        std::uint8_t red,
        std::uint8_t green,
        std::uint8_t blue
    ) {
        WorldDrawVertex result{};
        result.clip_x = x / guest_width * 2.0F - 1.0F;
        result.clip_y = 1.0F - y / guest_height * 2.0F;
        result.clip_z = 0.5F;
        result.clip_w = 1.0F;
        result.screen_x = x;
        result.screen_y = y;
        result.r = red;
        result.g = green;
        result.b = blue;
        return result;
    };
    const auto append_rectangle = [&] (
        float x0,
        float y0,
        float x1,
        float y1,
        std::uint8_t red,
        std::uint8_t green,
        std::uint8_t blue
    ) {
        WorldDrawCommand first{};
        first.vertices[0] = screen_vertex(
            x0, y0, red, green, blue);
        first.vertices[1] = screen_vertex(
            x1, y0, red, green, blue);
        first.vertices[2] = screen_vertex(
            x0, y1, red, green, blue);
        first.material_index = 0;
        first.clip_x0 = first.clip_y0 = 0;
        first.clip_x1 = guest_width - 1;
        first.clip_y1 = guest_height - 1;
        first.channel = WorldViewChannel::main_view;
        list.commands.push_back(first);
        WorldDrawCommand second = first;
        second.vertices[0] = screen_vertex(
            x1, y0, red, green, blue);
        second.vertices[1] = screen_vertex(
            x1, y1, red, green, blue);
        second.vertices[2] = screen_vertex(
            x0, y1, red, green, blue);
        list.commands.push_back(second);
    };

    // Each edge group has a narrow member whose own centre falls in the
    // centre band. Connected-component anchoring must keep that member with
    // the rest of its HUD group rather than tearing the sprite/font run.
    append_rectangle(1.0F, 2.0F, 5.0F, 4.0F, 0, 255, 0);
    append_rectangle(5.0F, 2.0F, 8.0F, 4.0F, 0, 255, 0);
    append_rectangle(7.0F, 7.0F, 9.0F, 9.0F, 0, 0, 255);
    append_rectangle(8.0F, 12.0F, 11.0F, 14.0F, 255, 255, 255);
    append_rectangle(11.0F, 12.0F, 15.0F, 14.0F, 255, 255, 255);
    list.unclassified_commands = static_cast<std::uint32_t>(
        list.commands.size());

    WorldGpuRenderOptions options{
        false,
        true,
        false,
        false,
        false,
        false,
        1,
        clear_rgba,
    };
    options.target_aspect_width = aspect_width;
    options.target_aspect_height = aspect_height;
    const std::uint32_t output_width =
        world_gpu_target_display_width(list, options);
    std::vector<std::uint16_t> vram(1024U * 512U);
    std::vector<std::uint8_t> output(
        static_cast<std::size_t>(output_width) * guest_height * 4U);
    WorldGpuRenderStats stats{};
    reset_world_d3d11_readback(false);
    if (render_world_d3d11(
            list,
            vram.data(),
            vram.size(),
            output.data(),
            output.size(),
            options,
            &stats) != WorldGpuRenderResult::success ||
        !stats.output_valid)
        return false;

    int green_minimum = static_cast<int>(output_width);
    int green_maximum = -1;
    int blue_minimum = static_cast<int>(output_width);
    int blue_maximum = -1;
    int white_minimum = static_cast<int>(output_width);
    int white_maximum = -1;
    for (std::uint32_t y = 0; y < guest_height; ++y) {
        for (std::uint32_t x = 0; x < output_width; ++x) {
            const std::size_t offset =
                (static_cast<std::size_t>(y) * output_width + x) * 4U;
            const std::uint8_t red = output[offset];
            const std::uint8_t green = output[offset + 1];
            const std::uint8_t blue = output[offset + 2];
            if (green > 240U && red < 16U && blue < 16U) {
                green_minimum = (std::min)(
                    green_minimum, static_cast<int>(x));
                green_maximum = (std::max)(green_maximum, static_cast<int>(x));
            }
            if (blue > 240U && red < 16U && green < 16U) {
                blue_minimum = (std::min)(
                    blue_minimum, static_cast<int>(x));
                blue_maximum = (std::max)(
                    blue_maximum, static_cast<int>(x));
            }
            if (red > 240U && green > 240U && blue > 240U) {
                white_minimum = (std::min)(
                    white_minimum, static_cast<int>(x));
                white_maximum = (std::max)(
                    white_maximum, static_cast<int>(x));
            }
        }
    }
    const float proportional_margin =
        static_cast<float>(output_width) / guest_width;
    const float expected_left_minimum = proportional_margin;
    const float expected_left_maximum = proportional_margin + 7.0F;
    const float expected_right_minimum =
        static_cast<float>(output_width) - proportional_margin - 7.0F;
    const float expected_right_maximum =
        static_cast<float>(output_width) - proportional_margin;
    const float blue_center =
        static_cast<float>(blue_minimum + blue_maximum) * 0.5F;
    return
        output_width ==
            (guest_height * aspect_width + aspect_height - 1U) /
                aspect_height &&
        std::fabs(green_minimum - expected_left_minimum) <= 2.0F &&
        std::fabs(green_maximum - expected_left_maximum) <= 2.0F &&
        std::fabs(white_minimum - expected_right_minimum) <= 2.0F &&
        std::fabs(white_maximum - expected_right_maximum) <= 2.0F &&
        std::fabs(blue_center - output_width * 0.5F) <= 1.5F &&
        green_minimum <= green_maximum &&
        green_maximum - green_minimum >= 5 &&
        green_maximum - green_minimum <= 8 &&
        white_minimum <= white_maximum &&
        white_maximum - white_minimum >= 5 &&
        white_maximum - white_minimum <= 8;
}

bool track_depth_survives_ordering_table_boundaries() {
    using namespace opengt::render;
    WorldDrawList list{};
    list.display_width = 16;
    list.display_height = 16;
    list.materials.push_back(WorldMaterial{});
    const auto append = [&list](
        float depth,
        std::int32_t ordering_table_index,
        std::uint8_t red,
        std::uint8_t green,
        std::uint8_t blue
    ) {
        WorldDrawCommand command{};
        command.vertices[0] = vertex(-1.0F, -1.0F, 0, 0, 0);
        command.vertices[1] = vertex(0.0F, 1.0F, 0, 1, 0);
        command.vertices[2] = vertex(1.0F, -1.0F, 1, 0, 0);
        for (auto& point : command.vertices) {
            point.clip_z = depth;
            point.clip_w = 1.0F;
            point.r = red;
            point.g = green;
            point.b = blue;
        }
        command.material_index = 0;
        command.ordering_table_index = ordering_table_index;
        command.clip_x0 = command.clip_y0 = 0;
        command.clip_x1 = command.clip_y1 = 15;
        command.object_kind = 1;
        command.object_id = static_cast<std::uint32_t>(
            ordering_table_index + 1);
        command.model_pointer = 0x80004000U +
            static_cast<std::uint32_t>(ordering_table_index) * 4U;
        command.channel = WorldViewChannel::main_view;
        list.commands.push_back(command);
    };
    // Submit the near green road first and a far blue road in another GT2 OT
    // bucket second. These are the worst distinct view depths measured in the
    // direct Seattle turn-one capture: conventional D24 maps both to the same
    // value, while reversed D32 keeps them ordered. A coherent modern depth
    // surface must retain the near one.
    append(16.0F / 5465.502323639F, 0, 0, 128, 0);
    append(16.0F / 5465.515567936F, 1, 0, 0, 128);
    list.track_commands = 2;

    std::vector<std::uint16_t> vram(1024U * 512U);
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    WorldGpuRenderStats stats{};
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
            false,
            false,
            false,
            1,
            clear_rgba,
        },
        &stats);
    if (result != WorldGpuRenderResult::success || !stats.output_valid)
        return false;
    const std::size_t center = (8U * 16U + 8U) * 4U;
    return output[center] < 16U &&
        output[center + 1] > 100U &&
        output[center + 1] > output[center + 2] * 4U;
}

std::array<std::uint8_t, 3> render_authored_track_overlay_depth_case(
    bool add_nearer_occluder,
    bool add_coplanar_road_sibling = false,
    bool mark_support = true,
    std::uint8_t overlay_red = 192,
    std::uint8_t overlay_green = 160,
    std::uint8_t overlay_blue = 0,
    bool replacement_surface = false,
    float overlay_view_z = 1002.0F,
    bool append_support_command = true,
    bool support_is_drivable_road = false,
    bool support_uses_different_group = false,
    bool support_sets_mask = false,
    bool overlay_checks_mask = false
) {
    using namespace opengt::render;
    WorldDrawList list{};
    list.display_width = 16;
    list.display_height = 16;
    WorldMaterial support_material{};
    if (mark_support) {
        support_material.primitive_flags |=
            world_primitive_track_overlay_support_flag;
    }
    if (support_is_drivable_road)
        support_material.primitive_flags |= 1U;
    if (support_is_drivable_road)
        support_material.texture_page = 2U << 7U;
    if (support_sets_mask)
        support_material.environment_flags |= 1U;
    list.materials.push_back(support_material);
    WorldMaterial overlay_material{};
    overlay_material.primitive_flags =
        1U << world_primitive_track_overlay_layer_shift;
    if (replacement_surface) {
        overlay_material.primitive_flags |=
            world_primitive_track_replacement_flag;
    }
    if (overlay_checks_mask)
        overlay_material.environment_flags |= 2U;
    list.materials.push_back(overlay_material);
    WorldMaterial road_sibling_material{};
    road_sibling_material.primitive_flags = 1U;
    road_sibling_material.texture_page = 2U << 7U;
    list.materials.push_back(road_sibling_material);
    list.materials.push_back(WorldMaterial{});
    const auto append = [&list](
        std::uint32_t material_index,
        float view_z,
        std::uint8_t red,
        std::uint8_t green,
        std::uint8_t blue,
        std::uint32_t object_id = 1U
    ) {
        WorldDrawCommand command{};
        command.vertices[0] = vertex(-view_z, -view_z, 0, 0, 0);
        command.vertices[1] = vertex(0.0F, view_z, 0, 1, 0);
        command.vertices[2] = vertex(view_z, -view_z, 1, 0, 0);
        for (auto& point : command.vertices) {
            point.clip_z = 16.0F;
            point.clip_w = view_z;
            point.r = red;
            point.g = green;
            point.b = blue;
        }
        command.material_index = material_index;
        command.clip_x0 = command.clip_y0 = 0;
        command.clip_x1 = command.clip_y1 = 15;
        command.object_kind = 1;
        command.object_id = object_id;
        command.model_pointer = 0x80004000U;
        command.channel = WorldViewChannel::main_view;
        list.commands.push_back(command);
    };
    // Typed road artwork receives deterministic priority only when it is
    // physically coplanar with its road support. A farther classified polygon
    // must remain behind nearer geometry.
    if (append_support_command)
        append(
            0, 1000.0F, 0, 128, 0,
            support_uses_different_group ? 2U : 1U);
    // GT2 can submit another road triangle at the same final depth after the
    // exact support. It is still road, so it must not erase ownership and make
    // the marking alternate with draw order.
    if (add_coplanar_road_sibling)
        append(2, 1000.0F, 0, 96, 0);
    append(
        1,
        overlay_view_z,
        overlay_red,
        overlay_green,
        overlay_blue);
    // Real geometry at view Z=997 must still occlude it. This guards the
    // priority pass against turning into an unbounded draw-order override.
    if (add_nearer_occluder)
        append(3, 997.0F, 0, 0, 192);
    list.track_commands = list.commands.size();

    std::vector<std::uint16_t> vram(1024U * 512U);
    vram[10U * 1024U + 10U] = green_555;
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    WorldGpuRenderStats stats{};
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
            false,
            false,
            false,
            1,
            clear_rgba,
        },
        &stats);
    if (result != WorldGpuRenderResult::success || !stats.output_valid)
        return {};
    const std::size_t center = (8U * 16U + 8U) * 4U;
    return {
        output[center],
        output[center + 1],
        output[center + 2],
    };
}

bool authored_track_overlay_wins_coplanar_depth() {
    const auto color = render_authored_track_overlay_depth_case(
        false, false, true, 192, 160, 0, false, 1000.0F);
    return color[0] > 150U && color[1] > 120U && color[2] < 16U;
}

bool surface_detail_stays_visible_during_camera_motion(bool vehicle = false, bool occluded = false) {
    using namespace opengt::render;
    // A small marking and its large road triangle share one authored plane,
    // but not raster vertices. Move their perspective projection through 32
    // subpixel positions, including opposite diagonals/winding. A constant-Z
    // pixel test cannot exercise independent raster depth interpolation.
    for (int frame = 0; frame < 32; ++frame) {
        WorldDrawList list{};
        list.display_width = list.display_height = 64;
        WorldMaterial support{};
        support.primitive_flags = world_primitive_track_overlay_support_flag;
        WorldMaterial detail{};
        detail.primitive_flags = 1U << world_primitive_track_overlay_layer_shift;
        if (vehicle) {
            support.primitive_flags = 1U | 4U;
            support.texture_page = 2U << 7U;
            detail.primitive_flags = 1U | 2U;
            detail.texture_page = (2U << 7U) | (1U << 5U);
        }
        list.materials = {support, detail};
        for (int layer = 0; layer < 2; ++layer) {
            const std::int16_t extent = layer == 0 ? 100 : 30;
            WorldDrawCommand command{};
            command.material_index = layer;
            command.object_kind = vehicle ? 2U : 1U;
            command.object_id = 1;
            command.model_pointer = 0x80004000U;
            command.transform_id = static_cast<std::uint64_t>(frame + 1);
            command.exact_transform_valid = true;
            command.clip_x1 = command.clip_y1 = 63;
            const std::array<std::array<std::int16_t, 2>, 3> model{{
                {{static_cast<std::int16_t>(-extent), static_cast<std::int16_t>(-extent)}},
                {{0, extent}}, {{extent, static_cast<std::int16_t>(-extent)}}}};
            for (int corner = 0; corner < 3; ++corner) {
                const auto& p = model[(frame % 2 && layer == 1) ? 2-corner : corner];
                auto& point = command.vertices[corner];
                point = vertex(p[0]*10.0F + frame*0.137F,
                    p[1]*10.0F + frame*0.173F, p[0], p[1], 0);
                point.clip_w = 1000.0F + p[1]*3.0F + p[0]*0.021F + frame*0.193F;
                point.clip_z = 16.0F;
                point.r = layer == 1 ? 192 : 0;
                point.g = layer == 1 ? 160 : 128;
                point.b = 0;
                if (vehicle) {
                    point.r = point.g = point.b = 128;
                    point.u = point.v = layer == 1 ? 20.0F : 10.0F;
                }
            }
            list.commands.push_back(command);
        }
        list.track_commands = 2;
        if (vehicle) {
            // Eight exact body/detail pairs establish a broad environment
            // material; a smaller, differently tessellated visible detail
            // triangle exercises its shared-plane depth in the center.
            for (int pair = 0; pair < 8; ++pair) {
                auto body = list.commands[0];
                for (auto& point : body.vertices) {
                    point.model_x += static_cast<std::int16_t>(pair + 1);
                    point.clip_x += (pair + 1)*10.0F;
                    point.clip_w += (pair + 1)*0.021F;
                }
                auto reflection = body;
                reflection.material_index = 1;
                for (auto& point : reflection.vertices)
                    point.u = point.v = 30.0F;
                list.commands.push_back(body);
                list.commands.push_back(reflection);
            }
            list.vehicle_commands = static_cast<std::uint32_t>(list.commands.size());
            list.track_commands = 0;
        }
        if (occluded) {
            list.materials.push_back(WorldMaterial{});
            auto foreground = list.commands[0];
            foreground.material_index = 2;
            foreground.object_kind = 1;
            foreground.object_id = 2;
            foreground.model_pointer = 0x80005000U;
            for (auto& point : foreground.vertices) {
                point.clip_z *= 1.01F;
                point.r = point.g = 0;
                point.b = 255;
            }
            list.commands.push_back(foreground);
            ++list.track_commands;
        }
        std::vector<std::uint16_t> vram(1024U*512U);
        vram[10U*1024U+10U] = 0x4210U;
        vram[20U*1024U+20U] = 0xA108U;
        std::vector<std::uint8_t> output(64U*64U*4U);
        WorldGpuRenderStats stats{};
        reset_world_d3d11_readback(false);
        const auto result = render_world_d3d11(list, vram.data(), vram.size(),
            output.data(), output.size(), WorldGpuRenderOptions{
                false, true, false, false, false, false, 1, clear_rgba}, &stats);
        if (result != WorldGpuRenderResult::success || !stats.output_valid)
            return false;
        // Interior pixels are safely inside both triangles throughout motion.
        for (std::size_t y = 30; y <= 35; ++y)
            for (std::size_t x = 30; x <= 33; ++x) {
                const auto pixel = (y*64U+x)*4U;
                const bool okay = occluded
                    ? output[pixel] < 16U && output[pixel+2] > 240U
                    : vehicle
                    ? output[pixel] >= 194U && output[pixel] <= 202U
                    : output[pixel] >= 150U && output[pixel+1] >= 120U;
                if (!okay)
                    return false;
            }
    }
    return true;
}

bool mask_checked_track_overlay_obeys_ps1_mask() {
    const auto unmasked = render_authored_track_overlay_depth_case(
        false, false, true, 192, 160, 0, false, 1000.0F,
        true, false, false, false, true);
    const auto masked = render_authored_track_overlay_depth_case(
        false, false, true, 192, 160, 0, false, 1000.0F,
        true, false, false, true, true);
    return
        unmasked[0] > 150U && unmasked[1] > 120U && unmasked[2] < 16U &&
        masked[0] < 16U && masked[1] > 90U && masked[2] < 16U;
}

bool white_track_overlay_uses_same_priority_contract() {
    const auto color = render_authored_track_overlay_depth_case(
        false, false, true, 192, 192, 192, false, 1000.0F);
    return color[0] > 150U && color[1] > 150U && color[2] > 150U;
}

bool classified_track_overlay_does_not_require_exact_support_mask() {
    const auto color = render_authored_track_overlay_depth_case(
        false, false, false, 192, 160, 0, false, 1000.0F, true, true);
    return color[0] > 150U && color[1] > 120U && color[2] < 16U;
}

bool authored_track_overlay_obeys_physical_depth() {
    // Classification cannot authorize a distant polygon to punch through its
    // nearer support, regardless of resident transform scale.
    const auto color = render_authored_track_overlay_depth_case(
        false, false, true, 192, 160, 0, false, 1032.0F);
    return color[0] < 16U && color[1] > 90U && color[2] < 16U;
}

bool authored_track_overlay_rejects_unrelated_horizontal_group() {
    const auto color = render_authored_track_overlay_depth_case(
        false, false, false, 192, 160, 0, false, 1032.0F, true, true, true);
    return color[0] < 16U && color[1] > 90U && color[2] < 16U;
}

bool authored_track_overlay_stays_behind_nearer_geometry() {
    const auto color = render_authored_track_overlay_depth_case(true);
    return color[0] < 16U && color[1] < 16U && color[2] > 150U;
}

bool connected_upright_wall_does_not_inherit_road_overlay_support() {
    using namespace opengt::render;
    WorldDrawList list{};
    list.display_width = 16;
    list.display_height = 16;

    WorldMaterial course_material{};
    course_material.primitive_flags =
        1U | world_primitive_resident_course_flag;
    course_material.texture_page = 2U << 7U;
    list.materials.push_back(course_material);
    WorldMaterial overlay_material{};
    overlay_material.primitive_flags =
        1U << world_primitive_track_overlay_layer_shift;
    list.materials.push_back(overlay_material);

    const auto append = [&list] (
        std::uint32_t material_index,
        float view_z,
        std::uint8_t red,
        std::uint8_t green,
        std::uint8_t blue,
        const std::array<std::array<std::int16_t, 3>, 3>& model
    ) {
        WorldDrawCommand command{};
        command.vertices[0] = vertex(-view_z, -view_z, 0, 0, 0);
        command.vertices[1] = vertex(0.0F, view_z, 0, 1, 0);
        command.vertices[2] = vertex(view_z, -view_z, 1, 0, 0);
        for (std::size_t index = 0; index < 3; ++index) {
            auto& point = command.vertices[index];
            point.clip_z = 16.0F;
            point.clip_w = view_z;
            point.r = red;
            point.g = green;
            point.b = blue;
            point.model_x = model[index][0];
            point.model_y = model[index][1];
            point.model_z = model[index][2];
            point.world_x = static_cast<float>(model[index][0]);
            point.world_y = static_cast<float>(model[index][1]);
            point.world_z = static_cast<float>(model[index][2]);
        }
        command.material_index = material_index;
        command.clip_x0 = command.clip_y0 = 0;
        command.clip_x1 = command.clip_y1 = 15;
        command.object_kind = 1;
        command.object_id = 7;
        command.model_pointer = 0x80007000U;
        command.transform_id = 0x123456789ABCDEF0ULL;
        command.channel = WorldViewChannel::main_view;
        list.commands.push_back(command);
    };

    // These three faces share exact model edges. Their normals progress from
    // ground-facing through a smooth diagonal to upright, so the opacity graph
    // intentionally classifies the wall as solid. The wall is nearer than the
    // road overlay and must clear—not inherit—the road-owner stencil.
    append(0, 1000.0F, 0, 128, 0, {{{0, 0, 0}, {1, 0, 0}, {0, 1, 0}}});
    append(0, 999.0F, 0, 128, 0, {{{1, 0, 0}, {0, 1, 0}, {0, 0, 1}}});
    append(0, 997.0F, 0, 0, 192, {{{0, 1, 0}, {0, 0, 1}, {0, 1, 1}}});
    append(1, 1002.0F, 192, 160, 0, {{{0, 0, 0}, {1, 0, 0}, {0, 1, 0}}});
    list.track_commands = list.commands.size();

    std::vector<std::uint16_t> vram(1024U * 512U, 0x7FFFU);
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    WorldGpuRenderStats stats{};
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
            false,
            false,
            false,
            1,
            clear_rgba,
        },
        &stats);
    if (result != WorldGpuRenderResult::success || !stats.output_valid)
        return false;
    const std::size_t center = (8U * 16U + 8U) * 4U;
    return output[center] < 16U &&
        output[center + 1] < 16U &&
        output[center + 2] > 150U;
}

bool coplanar_road_sibling_cannot_erase_overlay_ownership() {
    const auto color = render_authored_track_overlay_depth_case(
        false, true, true, 192, 160, 0, false, 1000.0F);
    return color[0] > 150U && color[1] > 120U && color[2] < 16U;
}

bool authored_track_replacement_respects_typed_support_depth() {
    // A classified replacement is still bounded by physical depth. Typed
    // support must not turn it into an unbounded screen-space overwrite.
    const auto color = render_authored_track_overlay_depth_case(
        false, false, true, 192, 160, 0, true, 1400.0F);
    return color[0] < 16U && color[1] > 90U && color[2] < 16U;
}

bool authored_track_replacement_requires_typed_support() {
    const auto color = render_authored_track_overlay_depth_case(
        false, false, false, 192, 160, 0, true, 1400.0F);
    return color[0] < 16U && color[1] > 90U && color[2] < 16U;
}

bool authored_track_replacement_retains_ordinary_surface() {
    // The priority redraw is additional. The full detailed road must also
    // participate in the ordinary depth pass where no coarse support exists.
    const auto color = render_authored_track_overlay_depth_case(
        false, false, false, 192, 160, 0, true, 1400.0F, false);
    return color[0] > 150U && color[1] > 120U && color[2] < 16U;
}

bool authored_track_replacement_stays_behind_nearer_geometry() {
    const auto color = render_authored_track_overlay_depth_case(
        true, false, true, 192, 160, 0, true, 1400.0F);
    return color[0] < 16U && color[1] < 16U && color[2] > 150U;
}

bool authored_track_overlay_stays_clipped_beyond_near_plane() {
    using namespace opengt::render;
    WorldDrawList list{};
    list.display_width = 16;
    list.display_height = 16;
    WorldMaterial overlay_material{};
    overlay_material.primitive_flags =
        1U << world_primitive_track_overlay_layer_shift;
    list.materials.push_back(overlay_material);

    WorldDrawCommand command{};
    command.vertices[0] = vertex(-1.0F, -1.0F, 0, 0, 0);
    command.vertices[1] = vertex(0.0F, 1.0F, 0, 1, 0);
    command.vertices[2] = vertex(1.0F, -1.0F, 1, 0, 0);
    for (auto& point : command.vertices) {
        point.clip_z = 2.0F;
        point.clip_w = 1.0F;
        point.r = 192;
        point.g = 160;
        point.b = 0;
    }
    command.material_index = 0;
    command.clip_x0 = command.clip_y0 = 0;
    command.clip_x1 = command.clip_y1 = 15;
    command.object_kind = 1;
    command.object_id = 1;
    command.model_pointer = 0x80004000U;
    command.channel = WorldViewChannel::main_view;
    list.commands.push_back(command);
    list.track_commands = 1;

    std::vector<std::uint16_t> vram(1024U * 512U);
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    WorldGpuRenderStats stats{};
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
            false,
            false,
            false,
            1,
            clear_rgba,
        },
        &stats);
    if (result != WorldGpuRenderResult::success || !stats.output_valid)
        return false;
    const std::size_t center = (8U * 16U + 8U) * 4U;
    return output[center] > 240U &&
        output[center + 1] < 16U &&
        output[center + 2] < 16U;
}

bool background_layer_does_not_occlude_world_depth() {
    using namespace opengt::render;
    WorldDrawList list{};
    list.display_width = 16;
    list.display_height = 16;
    list.materials.push_back(WorldMaterial{});
    const auto append = [&list](
        std::uint32_t object_kind,
        float depth,
        std::uint8_t red,
        std::uint8_t green,
        std::uint8_t blue
    ) {
        WorldDrawCommand command{};
        command.vertices[0] = vertex(-1.0F, -1.0F, 0, 0, 0);
        command.vertices[1] = vertex(0.0F, 1.0F, 0, 1, 0);
        command.vertices[2] = vertex(1.0F, -1.0F, 1, 0, 0);
        for (auto& point : command.vertices) {
            point.clip_z = depth;
            point.clip_w = 1.0F;
            point.r = red;
            point.g = green;
            point.b = blue;
        }
        command.material_index = 0;
        command.clip_x0 = command.clip_y0 = 0;
        command.clip_x1 = command.clip_y1 = 15;
        command.object_kind = object_kind;
        command.object_id = object_kind;
        command.channel = WorldViewChannel::main_view;
        list.commands.push_back(command);
    };
    // Reproduce Seattle's observed ordering: a visible vehicle is submitted,
    // then the projected background paints color without physical depth, then
    // a farther road surface is submitted. The background must be scheduled
    // first; otherwise it erases the vehicle color while the invisible vehicle
    // depth remains and rejects the road, leaving a background-colored hole.
    append(2U, 0.75F, 0, 128, 0);
    append(3U, 0.95F, 128, 0, 0);
    append(1U, 0.25F, 0, 0, 128);
    list.vehicle_commands = 1;
    list.background_commands = 1;
    list.track_commands = 1;

    std::vector<std::uint16_t> vram(1024U * 512U);
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    WorldGpuRenderStats stats{};
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
            false,
            false,
            false,
            1,
            clear_rgba,
        },
        &stats);
    if (result != WorldGpuRenderResult::success || !stats.output_valid)
        return false;
    const std::size_t center = (8U * 16U + 8U) * 4U;
    return output[center] < 16U &&
        output[center + 1] > 100U &&
        output[center + 1] > output[center] * 4U;
}

std::array<std::uint8_t, 4> render_fractional_uv_center() {
    using namespace opengt::render;
    std::vector<std::uint16_t> vram(1024U * 512U);
    vram[10U * 1024U + 10U] = green_555;
    auto list = draw_list(2U, true);
    for (auto& point : list.commands[0].vertices) {
        point.u = 10.75F;
        point.v = 10.75F;
    }
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    WorldGpuRenderStats stats{};
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
            false,
            1,
            clear_rgba,
        },
        &stats);
    if (result != WorldGpuRenderResult::success || !stats.output_valid)
        return {};
    const std::size_t center = (8U * 16U + 8U) * 4U;
    return {
        output[center],
        output[center + 1],
        output[center + 2],
        output[center + 3],
    };
}

std::array<std::uint8_t, 4> render_minified_sparse_center(
    bool visible_source_texel
) {
    using namespace opengt::render;
    WorldDrawList list{};
    list.display_width = 16;
    list.display_height = 16;
    list.materials.push_back(WorldMaterial{
        1U | 4U,
        2U << 7U,
        0,
        0,
        0,
        0,
        0,
        0,
    });
    const auto point = [] (
        float clip_x,
        float clip_y,
        float u,
        std::int16_t model_x,
        std::int16_t model_y
    ) {
        WorldDrawVertex result{};
        result.clip_x = clip_x;
        result.clip_y = clip_y;
        result.clip_z = 0.5F;
        result.clip_w = 1.0F;
        result.u = u;
        result.v = 10.0F;
        result.r = result.g = result.b = 128;
        result.model_x = model_x;
        result.model_y = model_y;
        result.model_z = 0;
        return result;
    };
    WorldDrawCommand command{};
    // One oversized triangle covers the target. U advances sixteen source
    // texels per output pixel. At the center, the retired 3x3 filter sampled
    // only U=130/136/141; the authored visible texel at U=132 lies inside the
    // same pixel footprint and must be recovered by adaptive anisotropy.
    command.vertices[0] = point(-1.0F, -1.0F, 0.0F, 0, 0);
    command.vertices[1] = point(3.0F, -1.0F, 512.0F, 1, 0);
    command.vertices[2] = point(-1.0F, 3.0F, 0.0F, 0, 1);
    command.material_index = 0;
    command.clip_x0 = command.clip_y0 = 0;
    command.clip_x1 = command.clip_y1 = 15;
    command.object_kind = 1;
    command.object_id = 1;
    command.model_pointer = 0x80001000U;
    command.channel = WorldViewChannel::main_view;
    list.commands.push_back(command);
    list.track_commands = 1;

    std::vector<std::uint16_t> vram(1024U * 512U);
    if (visible_source_texel)
        vram[10U * 1024U + 132U] = green_555;
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    WorldGpuRenderStats stats{};
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
            false,
            1,
            clear_rgba,
        },
        &stats);
    if (result != WorldGpuRenderResult::success || !stats.output_valid)
        return {};
    const std::size_t center = (8U * 16U + 8U) * 4U;
    return {
        output[center],
        output[center + 1],
        output[center + 2],
        output[center + 3],
    };
}

std::array<std::uint32_t, 3> render_minified_cutout_coverage(
    float source_u_span = 512.0F,
    bool later_opaque_surface = false,
    bool solid_texture = false
) {
    using namespace opengt::render;
    WorldDrawList list{};
    list.display_width = 16;
    list.display_height = 16;
    list.materials.push_back(WorldMaterial{
        1U | 4U,
        2U << 7U,
        0,
        0,
        0,
        0,
        0,
        0,
    });
    const auto point = [] (
        float clip_x,
        float clip_y,
        float u,
        std::int16_t model_x,
        std::int16_t model_y,
        float world_y,
        float world_z
    ) {
        WorldDrawVertex result{};
        result.clip_x = clip_x;
        result.clip_y = clip_y;
        result.clip_z = 0.5F;
        result.clip_w = 1.0F;
        result.u = u;
        result.v = 10.0F;
        result.r = result.g = result.b = 128;
        result.model_x = model_x;
        result.model_y = model_y;
        result.model_z = 0;
        result.world_x = 0.0F;
        result.world_y = world_y;
        result.world_z = world_z;
        return result;
    };
    WorldDrawCommand command{};
    // This vertical track triangle represents an alpha-tested fence.  U spans
    // sixteen source texels per output pixel and only one texel in eight is
    // opaque.  A coverage-preserving minifier must therefore retain some wire
    // pixels and some background pixels; promoting any non-zero sample to
    // opaque would turn the entire target green.
    // The model-space triangle is horizontal, but the object transform makes
    // it an upright YZ plane. This is how distant course foliage exposed the
    // old local-normal classifier: it was incorrectly treated like asphalt.
    command.vertices[0] = point(
        -1.0F, -1.0F, 0.0F, 0, 0, 0.0F, 0.0F);
    command.vertices[1] = point(
        3.0F, -1.0F, source_u_span, 1, 0, 1.0F, 0.0F);
    command.vertices[2] = point(
        -1.0F, 3.0F, 0.0F, 0, 1, 0.0F, 1.0F);
    command.material_index = 0;
    command.clip_x0 = command.clip_y0 = 0;
    command.clip_x1 = command.clip_y1 = 15;
    command.object_kind = 1;
    command.object_id = 1;
    command.model_pointer = 0x80001000U;
    command.channel = WorldViewChannel::main_view;
    list.commands.push_back(command);
    list.track_commands = 1;

    if (later_opaque_surface) {
        // A farther opaque course object can occur later in GT2's ordering
        // table. Faint filtered foliage must not reserve a fully opaque depth
        // sample and leave a hole in that object. Solid foliage must still
        // occlude it; disabling foliage depth wholesale is not a valid fix.
        auto farther = command;
        farther.material_index = 1;
        farther.object_id = 2;
        farther.model_pointer = 0x80002000U;
        for (auto& point : farther.vertices) {
            point.clip_z = 0.25F;
            point.r = point.g = 0;
            point.b = 255;
        }
        list.materials.push_back(WorldMaterial{});
        list.commands.push_back(farther);
        ++list.track_commands;
    }

    std::vector<std::uint16_t> vram(1024U * 512U);
    for (std::size_t u = 0; u < 256; u += solid_texture ? 1 : 8)
        vram[10U * 1024U + u] = green_555;
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    WorldGpuRenderStats stats{};
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
            false,
            1,
            clear_rgba,
        },
        &stats);
    if (result != WorldGpuRenderResult::success || !stats.output_valid)
        return {};

    // Counts are fully foreground, fully background, and fractionally
    // covered. A screen-door implementation produces only the first two;
    // continuous cutout coverage must produce blended red/green pixels.
    std::array<std::uint32_t, 3> counts{};
    for (std::uint32_t y = 4; y < 12; ++y) {
        for (std::uint32_t x = 4; x < 12; ++x) {
            const std::size_t offset = (y * 16U + x) * 4U;
            const std::array<std::uint8_t, 4> pixel{{
                output[offset],
                output[offset + 1],
                output[offset + 2],
                output[offset + 3],
            }};
            if (is_green(pixel))
                ++counts[0];
            else if (is_clear(pixel) ||
                (later_opaque_surface && pixel[0] < 16U &&
                    pixel[1] < 16U && pixel[2] > 240U))
                ++counts[1];
            else if (
                pixel[0] > 16U && pixel[1] > 16U && pixel[2] < 16U
            )
                ++counts[2];
        }
    }
    return counts;
}

bool reset_isolates_async_readback_generation() {
    using namespace opengt::render;
    constexpr std::uint32_t blue_clear_rgba = 0xFFFF0000U;
    std::vector<std::uint16_t> vram(1024U * 512U);
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    const auto list = draw_list(1U, true);
    const auto submit = [&] (std::uint32_t clear) {
        WorldGpuRenderStats stats{};
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
                false,
                1,
                clear,
                false,
                false,
                false,
                false,
                true,
            },
            &stats);
        return result == WorldGpuRenderResult::success &&
            !stats.output_valid;
    };

    reset_world_d3d11_readback(false);
    for (std::size_t index = 0;
         index < world_gpu_readback_pair_delay * 2;
         ++index) {
        if (!submit(clear_rgba))
            return false;
    }
    if (pending_world_d3d11_readback_pairs(false) == 0)
        return false;

    // Abandon the red generation without draining it, exactly as a live guest
    // cadence gap does while the GPU is under load.
    reset_world_d3d11_readback(false);
    if (pending_world_d3d11_readback_pairs(false) != 0)
        return false;
    for (std::size_t index = 0;
         index < world_gpu_readback_pair_delay * 2;
         ++index) {
        if (!submit(blue_clear_rgba))
            return false;
    }
    for (std::size_t index = 0;
         index < world_gpu_readback_pair_delay * 2;
         ++index) {
        const auto result = try_read_world_d3d11_image(
            false,
            output.data(),
            output.size(),
            true);
        if (result != WorldGpuReadbackResult::success)
            return false;
        // The upper-left pixel is outside the test triangle and therefore
        // identifies which clear-color generation supplied the image.
        if (output[0] > 16U || output[1] > 16U || output[2] < 240U)
            return false;
    }
    return true;
}

bool async_frames_preserve_mutable_inputs() {
    using namespace opengt::render;
    constexpr std::array<std::uint16_t, 3> words{{
        31U,
        31U << 5U,
        31U << 10U,
    }};
    std::vector<std::uint16_t> vram(1024U * 512U);
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    const auto list = draw_list(2U, true);
    reset_world_d3d11_readback(false);

    for (std::size_t index = 0;
         index < world_gpu_async_readback_image_capacity;
         ++index) {
        vram[10U * 1024U + 10U] = words[index % words.size()];
        WorldGpuRenderStats stats{};
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
                false,
                1,
                clear_rgba,
                false,
                false,
                false,
                false,
                true,
            },
            &stats);
        if (result != WorldGpuRenderResult::success || stats.output_valid)
            return false;
    }

    constexpr std::size_t center = (8U * 16U + 8U) * 4U;
    for (std::size_t index = 0;
         index < world_gpu_async_readback_image_capacity;
         ++index) {
        if (try_read_world_d3d11_image(
                false,
                output.data(),
                output.size(),
                true) != WorldGpuReadbackResult::success)
            return false;
        const std::size_t expected_channel = index % words.size();
        for (std::size_t channel = 0; channel < 3; ++channel) {
            const std::uint8_t value = output[center + channel];
            if (channel == expected_channel ? value < 240U : value > 16U)
                return false;
        }
    }
    return true;
}

std::array<std::uint8_t, 4> render_vehicle_reflection_center(
    std::size_t paired_commands,
    bool base_visible = true,
    bool reflection_visible = true,
    bool nearer_track = false,
    std::size_t unpaired_reflections = 0
) {
    using namespace opengt::render;
    WorldDrawList list{};
    list.display_width = 16;
    list.display_height = 16;
    list.materials.push_back(WorldMaterial{
        1U | 4U,
        2U << 7U,
        0,
        0,
        0,
        0,
        0,
        0,
    });
    list.materials.push_back(WorldMaterial{
        1U | 2U,
        static_cast<std::uint16_t>((2U << 7U) | (1U << 5U)),
        0,
        0,
        0,
        0,
        0,
        0,
    });
    list.materials.push_back(WorldMaterial{});

    const auto append = [&list] (
        std::uint32_t material_index,
        std::int16_t model_offset,
        float depth,
        float uv,
        std::uint32_t object_kind = 2U
    ) {
        WorldDrawCommand command{};
        command.vertices[0] = vertex(
            -1.0F, -1.0F, model_offset, 0, 0);
        command.vertices[1] = vertex(
            0.0F, 1.0F, model_offset, 1, 0);
        command.vertices[2] = vertex(
            1.0F, -1.0F, model_offset, 0, 1);
        for (auto& point : command.vertices) {
            point.clip_z = depth;
            point.clip_w = 1.0F;
            point.u = point.v = uv;
            point.r = point.g = point.b = 128;
        }
        command.material_index = material_index;
        command.clip_x0 = command.clip_y0 = 0;
        command.clip_x1 = command.clip_y1 = 15;
        command.object_kind = object_kind;
        command.object_id = object_kind == 2U ? 1U : 2U;
        command.model_pointer = object_kind == 2U
            ? 0x80008000U
            : 0x80009000U;
        command.transform_id = object_kind == 2U
            ? 0x123456789ABCDEF0ULL
            : 0x0FEDCBA987654321ULL;
        command.channel = WorldViewChannel::main_view;
        list.commands.push_back(command);
    };

    for (std::size_t index = 0; index < paired_commands; ++index)
        append(0, static_cast<std::int16_t>(index * 4), 0.5F, 10.0F);
    if (nearer_track) {
        append(2, 100, 0.75F, 0.0F, 1U);
        for (auto& point : list.commands.back().vertices) {
            point.r = point.g = 0;
            point.b = 255;
        }
    }
    for (std::size_t index = 0; index < paired_commands; ++index) {
        append(
            1,
            static_cast<std::int16_t>(index * 4),
            0.5F,
            index == 0 ? 20.0F : 30.0F + static_cast<float>(index));
    }
    for (std::size_t index = 0; index < unpaired_reflections; ++index)
        append(1, static_cast<std::int16_t>(1000 + index*4), 0.5F, 100.0F);
    list.vehicle_commands = static_cast<std::uint32_t>(
        paired_commands * 2U + unpaired_reflections);
    list.track_commands = nearer_track ? 1U : 0U;

    std::vector<std::uint16_t> vram(1024U * 512U);
    if (base_visible)
        vram[10U * 1024U + 10U] = 0x4210U;
    if (reflection_visible)
        vram[20U * 1024U + 20U] = 0xA108U;
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    WorldGpuRenderStats stats{};
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
            false,
            false,
            false,
            1,
            clear_rgba,
        },
        &stats);
    if (result != WorldGpuRenderResult::success || !stats.output_valid)
        return {};
    const std::size_t center = (8U * 16U + 8U) * 4U;
    return {
        output[center],
        output[center + 1],
        output[center + 2],
        output[center + 3],
    };
}

bool broad_vehicle_reflection_keeps_authored_strength() {
    const auto pixel = render_vehicle_reflection_center(8U);
    const auto ordinary_additive = render_vehicle_reflection_center(1U);
    return pixel == ordinary_additive &&
        pixel[0] >= 194U && pixel[0] <= 202U &&
        pixel[1] >= 194U && pixel[1] <= 202U &&
        pixel[2] >= 194U && pixel[2] <= 202U;
}

bool curved_body_reflection_follows_authored_subdivision(bool occluded = false) {
    using namespace opengt::render;
    WorldDrawList list{};
    list.display_width = list.display_height = 64;
    WorldMaterial body_material{}, reflection_material{};
    body_material.primitive_flags = 1U | 4U;
    body_material.texture_page = 2U << 7U;
    reflection_material.primitive_flags = 1U | 2U;
    reflection_material.texture_page = (2U << 7U) | (1U << 5U);
    list.materials = {body_material, reflection_material, WorldMaterial{}};
    const auto point = [] (int x, int y, bool reflection) {
        const auto z = static_cast<std::int16_t>(-(x+100)*(y+100)/500);
        auto result = vertex(x*5.0F, y*5.0F,
            static_cast<std::int16_t>(x), static_cast<std::int16_t>(y), z);
        result.clip_z = 16.0F;
        result.clip_w = 1000.0F + z;
        result.u = result.v = reflection ? 20.0F : 10.0F;
        return result;
    };
    const auto append = [&] (WorldDrawVertex a, WorldDrawVertex b,
                            WorldDrawVertex c, bool reflection) {
        WorldDrawCommand command{};
        command.vertices[0] = a; command.vertices[1] = b; command.vertices[2] = c;
        command.material_index = reflection ? 1U : 0U;
        command.object_kind = 2;
        command.object_id = 1;
        command.model_pointer = 0x80004000U;
        command.transform_id = 1;
        command.exact_transform_valid = true;
        command.clip_x1 = command.clip_y1 = 63;
        list.commands.push_back(command);
    };
    for (int y = -100; y < 100; y += 50)
        for (int x = -100; x < 100; x += 50) {
            append(point(x,y,false), point(x+50,y,false), point(x,y+50,false), false);
            append(point(x+50,y,false), point(x,y+50,false), point(x+50,y+50,false), false);
        }
    // The broad environment material has exact pairs elsewhere on the body;
    // its visible roof is the mismatched coarse nonplanar quad under test.
    for (int pair = 0; pair < 8; ++pair) {
        auto a = point(-100,-100,false), b = point(100,-100,false), c = point(-100,100,false);
        for (auto* vertex : {&a,&b,&c}) {
            vertex->model_x += static_cast<std::int16_t>(1000 + pair*300);
            vertex->clip_x += 10000.0F + pair*3000.0F;
        }
        append(a,b,c,false);
        for (auto* vertex : {&a,&b,&c}) vertex->u = vertex->v = 20.0F;
        append(a,b,c,true);
    }
    append(point(-100,-100,true),point(100,-100,true),point(-100,100,true),true);
    append(point(100,-100,true),point(-100,100,true),point(100,100,true),true);
    list.vehicle_commands = static_cast<std::uint32_t>(list.commands.size());
    if (occluded) {
        WorldDrawCommand foreground = list.commands.front();
        foreground.material_index = 2;
        foreground.object_id = 2;
        foreground.object_kind = 1;
        foreground.vertices[0] = vertex(-1.0F,-1.0F,0,0,0);
        foreground.vertices[1] = vertex(0.0F,1.0F,0,1,0);
        foreground.vertices[2] = vertex(1.0F,-1.0F,1,0,0);
        for (auto& vertex : foreground.vertices) {
            vertex.r = vertex.g = 0;
            vertex.b = 255;
        }
        list.commands.push_back(foreground);
        list.track_commands = 1;
    }
    std::vector<std::uint16_t> vram(1024U*512U);
    vram[10U*1024U+10U] = 0x4210U;
    vram[20U*1024U+20U] = 0xA108U;
    std::vector<std::uint8_t> output(64U*64U*4U);
    WorldGpuRenderStats stats{};
    reset_world_d3d11_readback(false);
    const auto result = render_world_d3d11(list, vram.data(), vram.size(),
        output.data(), output.size(), WorldGpuRenderOptions{
            false,true,false,false,false,false,1,clear_rgba}, &stats);
    if (result != WorldGpuRenderResult::success || !stats.output_valid)
        return false;
    for (std::size_t y = 25; y < 39; ++y)
        for (std::size_t x = 25; x < 39; ++x) {
            const auto pixel = (y*64U+x)*4U;
            if (occluded ? output[pixel] > 16U || output[pixel+2] < 240U
                         : output[pixel] < 194U || output[pixel] > 202U)
                return false;
        }
    return true;
}

bool small_additive_vehicle_detail_keeps_ps1_blend() {
    const auto pixel = render_vehicle_reflection_center(1U);
    return pixel[0] >= 194U && pixel[1] >= 194U && pixel[2] >= 194U;
}

bool vehicle_reflection_strength_survives_tessellation_changes() {
    const auto reference = render_vehicle_reflection_center(8U);
    // More unpaired coarse reflection triangles must not change the strength
    // of an existing body/detail pair as base tessellation changes nearby.
    for (const auto count : {8U, 9U, 40U, 125U})
        if (render_vehicle_reflection_center(8U, true, true, false, count) != reference)
            return false;
    return true;
}

bool vehicle_reflection_requires_visible_base_owner() {
    return is_clear(render_vehicle_reflection_center(8U, false));
}

bool vehicle_reflection_preserves_zero_texel_transparency() {
    const auto pixel = render_vehicle_reflection_center(8U, true, false);
    return
        pixel[0] >= 128U && pixel[0] <= 136U &&
        pixel[1] >= 128U && pixel[1] <= 136U &&
        pixel[2] >= 128U && pixel[2] <= 136U;
}

bool nearer_track_occludes_vehicle_reflection() {
    const auto pixel = render_vehicle_reflection_center(
        8U, true, true, true);
    return pixel[0] < 16U && pixel[1] < 16U && pixel[2] > 240U;
}

std::array<std::uint8_t, 4> render_vehicle_shadow_center(
    bool prepend_clipped_shadow
) {
    using namespace opengt::render;
    WorldDrawList list{};
    list.display_width = 16;
    list.display_height = 16;
    list.materials.push_back(WorldMaterial{
        0xAU,
        2U << 5U,
        0,
        0,
        0,
        0,
        0,
        0,
    });

    WorldDrawCommand shadow{};
    shadow.vertices[0] = vertex(-1.0F, -1.0F, -1747, 0, 2472);
    shadow.vertices[1] = vertex(-1.0F, 1.0F, -1726, 1051, -43);
    // Clipped/interpolated GT2 shadow vertices can all lack model provenance
    // and retain unrelated view-space coordinates.
    shadow.vertices[2] = vertex(1.0F, 0.0F, 4556, 1732, 5593);
    shadow.material_index = 0;
    shadow.clip_x0 = 0;
    shadow.clip_y0 = 0;
    shadow.clip_x1 = 15;
    shadow.clip_y1 = 15;
    shadow.object_kind = 2;
    shadow.object_id = 1;
    shadow.model_pointer = 0x80165118U;
    shadow.transform_id = 0xC3D496D5101E901DULL;
    shadow.channel = WorldViewChannel::main_view;

    if (prepend_clipped_shadow) {
        WorldDrawCommand clipped_shadow = shadow;
        clipped_shadow.vertices[0] = vertex(-1.0F, -1.0F, 0, 0, 0);
        clipped_shadow.vertices[1] = vertex(-0.75F, -0.5F, 0, 1, 0);
        clipped_shadow.vertices[2] = vertex(-0.5F, -1.0F, 0, 2, 0);
        list.commands.push_back(clipped_shadow);
    }
    list.commands.push_back(shadow);
    list.vehicle_commands = static_cast<std::uint32_t>(
        list.commands.size());

    std::vector<std::uint16_t> vram(1024U * 512U);
    std::vector<std::uint8_t> output(16U * 16U * 4U);
    WorldGpuRenderStats stats{};
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
            false,
            1,
            clear_rgba,
        },
        &stats);
    if (result != WorldGpuRenderResult::success || !stats.output_valid) {
        std::fprintf(
            stderr,
            "FAILED: vehicle shadow render result=%s outputValid=%u\n",
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

bool is_soft_shadow(const std::array<std::uint8_t, 4>& pixel) {
    return
        pixel[0] >= 230U && pixel[0] <= 245U &&
        pixel[1] < 16U && pixel[2] < 16U;
}

std::vector<std::uint8_t> render_uv_island(
    bool perspective,
    float near_depth,
    bool resident_course = false
) {
    using namespace opengt::render;
    WorldDrawList list{};
    list.display_width = 32;
    list.display_height = 32;
    list.materials.push_back(WorldMaterial{
        1U | 4U |
            (resident_course
                ? world_primitive_resident_course_flag
                : 0U),
        2U << 7U,
        0,
        0,
        0,
        0,
        0,
        0,
    });
    const auto point = [](
        float ndc_x,
        float ndc_y,
        float depth,
        float u,
        float v,
        std::int32_t identity_x,
        std::int32_t identity_y
    ) {
        WorldDrawVertex result{};
        result.clip_x = ndc_x * depth;
        result.clip_y = ndc_y * depth;
        result.clip_z = 0.5F * depth;
        result.clip_w = depth;
        result.u = u;
        result.v = v;
        result.r = result.g = result.b = 128;
        result.exact_view_x = identity_x;
        result.exact_view_y = identity_y;
        result.exact_view_z = static_cast<std::int32_t>(depth);
        result.exact_transform_valid = true;
        return result;
    };
    const auto a = point(-0.9F, -0.9F, near_depth, 4, 4, 0, 0);
    const auto b = point(0.9F, -0.9F, 16, 20, 4, 16, 0);
    const auto c = point(-0.9F, 0.9F, 16, 4, 20, 0, 16);
    const auto d = point(0.9F, 0.9F, 4, 20, 20, 16, 16);
    const auto append = [&list](
        const WorldDrawVertex& first,
        const WorldDrawVertex& second,
        const WorldDrawVertex& third
    ) {
        WorldDrawCommand command{};
        command.vertices[0] = first;
        command.vertices[1] = second;
        command.vertices[2] = third;
        command.material_index = 0;
        command.clip_x0 = command.clip_y0 = 0;
        command.clip_x1 = command.clip_y1 = 31;
        command.object_kind = 1;
        command.object_id = 9;
        command.model_pointer = 0x80003000U;
        command.channel = WorldViewChannel::main_view;
        list.commands.push_back(command);
    };
    append(a, b, c);
    append(b, d, c);
    list.track_commands = 2;

    std::vector<std::uint16_t> vram(1024U * 512U);
    for (std::uint32_t y = 1; y < 32; ++y) {
        for (std::uint32_t x = 1; x < 32; ++x) {
            vram[y * 1024U + x] = static_cast<std::uint16_t>(
                (x & 31U) |
                ((y & 31U) << 5U) |
                (((x + y) & 31U) << 10U));
        }
    }
    std::vector<std::uint8_t> output(32U * 32U * 4U);
    WorldGpuRenderStats stats{};
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
            perspective,
            false,
            false,
            1,
            clear_rgba,
        },
        &stats);
    if (result != WorldGpuRenderResult::success || !stats.output_valid)
        return {};
    return output;
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
std::array<int, 2> mirror_shell_coverage(
    std::int16_t clip_x1,
    bool enable_reconstructed_shell
) {
    using namespace opengt::render;
#if defined(_WIN32)
    _putenv_s(
        "OPENGT_RENDER_SMOOTH_WHEELS",
        enable_reconstructed_shell ? "1" : "");
#endif
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
    using opengt::render::WorldTextureUpload;
    using opengt::render::world_texture_upload_contains_clut;
    bool okay = true;
    opengt::render::WorldDrawList aspect_list{};
    aspect_list.display_width = 320;
    aspect_list.display_height = 240;
    opengt::render::WorldGpuRenderOptions aspect_options{};
    aspect_options.target_aspect_width = 16;
    aspect_options.target_aspect_height = 9;
    okay &= expect(
        opengt::render::world_gpu_target_display_width(
            aspect_list, aspect_options) == 427,
        "derive a horizontal-plus width while preserving vertical resolution");
    aspect_options.target_aspect_width = 21;
    aspect_options.target_aspect_height = 9;
    okay &= expect(
        opengt::render::world_gpu_target_display_width(
            aspect_list, aspect_options) == 560,
        "derive a 21:9 Hor+ width without changing vertical resolution");
    aspect_options.target_aspect_width = 32;
    aspect_options.target_aspect_height = 9;
    okay &= expect(
        opengt::render::world_gpu_target_display_width(
            aspect_list, aspect_options) == 854,
        "derive a 32:9 Hor+ width without changing vertical resolution");
    constexpr std::uint64_t paint_key = 0x123456789ABCDEF0ULL;
    constexpr WorldTextureUpload paint_bank{
        paint_key, 448, 480, 64, 4};
    constexpr std::uint16_t contained_clut =
        static_cast<std::uint16_t>((481U << 6U) | (29U));
    okay &= expect(
        world_texture_upload_contains_clut(
            paint_bank, paint_key, contained_clut),
        "match a car paint only when its exact palette upload contains the CLUT");
    okay &= expect(
        !world_texture_upload_contains_clut(
            paint_bank, paint_key ^ 1U, contained_clut),
        "reject a different car paint palette key");
    constexpr std::uint16_t outside_clut =
        static_cast<std::uint16_t>((481U << 6U) | (27U));
    okay &= expect(
        !world_texture_upload_contains_clut(
            paint_bank, paint_key, outside_clut),
        "reject an exact palette upload that does not contain the primitive CLUT");
    okay &= expect(
        is_green(render_center(1U, true, 4U)),
        "repair an isolated transparent texel on an opaque track surface");
    okay &= expect(
        fully_behind_world_triangle_is_rejected_before_submit(),
        "reject a fully behind-camera world triangle before GPU batching");
    okay &= expect(
        fully_outside_world_triangle_is_rejected_before_submit(),
        "reject a camera-straddling triangle outside one clip plane");
    okay &= expect(
        is_clear(render_center(2U, true, 4U)),
        "preserve transparent texels on vehicle geometry");
    okay &= expect(
        is_clear(render_center(1U, false, 4U)),
        "preserve transparent texels on a vertical track surface");
    okay &= expect(
        is_green(render_connected_terrain_zero_texel(true)),
        "extend solid recovery across a smooth connected terrain fold");
    okay &= expect(
        is_clear(render_connected_terrain_zero_texel(false)),
        "keep a hard-attached course cutout transparent");
    okay &= expect(
        is_clear(render_center(1U, true, 2U)),
        "preserve an authored track-texture cutout");
    okay &= expect(
        is_green(render_fractional_uv_center()),
        "use PS1 floor semantics for fractional world-texture visibility");
    okay &= expect(
        is_green(render_minified_sparse_center(true)),
        "recover visible indexed texels across an anisotropic road footprint");
    okay &= expect(
        is_clear(render_minified_sparse_center(false)),
        "preserve an all-transparent anisotropic footprint");
    for (const float source_u_span : {64.0F, 128.0F, 256.0F, 512.0F}) {
        const auto cutout_coverage =
            render_minified_cutout_coverage(source_u_span);
        okay &= expect(
            cutout_coverage[2] > 0 &&
                cutout_coverage[0] + cutout_coverage[1] +
                    cutout_coverage[2] == 64,
            "blend partial alpha-tested coverage without a screen-door "
            "pattern throughout a course cutout distance sweep");
    }
    okay &= expect(
        render_minified_cutout_coverage(512.0F, true, false)[1] == 64,
        "faint foliage coverage must not punch depth holes in later terrain");
    okay &= expect(
        render_minified_cutout_coverage(512.0F, true, true)[0] == 64,
        "solid foliage must retain depth ownership over farther terrain");
    for (const bool tree_first : {false, true}) {
        for (const bool tree_nearer : {false, true}) {
            okay &= expect(resident_billboard_uses_metric_depth(tree_first, tree_nearer),
                "resident tree/wall occlusion follows normalized Z in either draw order");
            okay &= expect(resident_billboard_uses_metric_depth(tree_first, tree_nearer, true),
                "screen-offset flare blends over farther walls but stays behind nearer walls in either draw order");
        }
    }
    okay &= expect(
        horizontal_plus_reveals_world_outside_guest_edge(),
        "reveal main-world geometry beyond the original horizontal plane");
    okay &= expect(
        horizontal_plus_anchors_hud_groups_to_margins(16, 9),
        "anchor connected HUD groups to 16:9 margins without stretching");
    okay &= expect(
        horizontal_plus_anchors_hud_groups_to_margins(21, 9),
        "anchor connected HUD groups to 21:9 margins without stretching");
    okay &= expect(
        horizontal_plus_anchors_hud_groups_to_margins(32, 9),
        "anchor connected HUD groups to 32:9 margins without stretching");
    okay &= expect(
        track_depth_survives_ordering_table_boundaries(),
        "keep one track depth surface across GT2 ordering-table buckets");
    okay &= expect(
        authored_track_overlay_wins_coplanar_depth(),
        "give classified yellow road artwork priority over its support");
    okay &= expect(
        surface_detail_stays_visible_during_camera_motion(),
        "retain every interior road-marking pixel through perspective camera motion");
    okay &= expect(
        surface_detail_stays_visible_during_camera_motion(false, true),
        "keep nearer geometry ahead of shared-plane road artwork during motion");
    okay &= expect(
        surface_detail_stays_visible_during_camera_motion(true),
        "retain the vehicle reflection contribution through perspective camera motion");
    okay &= expect(
        surface_detail_stays_visible_during_camera_motion(true, true),
        "keep nearer geometry ahead of shared-plane vehicle detail during motion");
    okay &= expect(
        mask_checked_track_overlay_obeys_ps1_mask(),
        "preserve PS1 mask rejection for typed road artwork");
    okay &= expect(
        white_track_overlay_uses_same_priority_contract(),
        "give classified white road artwork the same support priority");
    okay &= expect(
        classified_track_overlay_does_not_require_exact_support_mask(),
        "apply typed road-artwork priority through its drivable road group");
    okay &= expect(
        authored_track_overlay_obeys_physical_depth(),
        "bound typed road artwork by physical depth at every transform scale");
    okay &= expect(
        authored_track_overlay_rejects_unrelated_horizontal_group(),
        "keep road artwork behind unrelated horizontal course geometry");
    okay &= expect(
        authored_track_overlay_stays_behind_nearer_geometry(),
        "keep bounded road-overlay priority behind genuinely nearer geometry");
    okay &= expect(
        connected_upright_wall_does_not_inherit_road_overlay_support(),
        "keep road overlays behind connected upright course walls");
    okay &= expect(
        coplanar_road_sibling_cannot_erase_overlay_ownership(),
        "keep road-artwork ownership through later coplanar road triangles");
    okay &= expect(
        authored_track_replacement_respects_typed_support_depth(),
        "keep classified replacement surfaces bounded by physical depth");
    okay &= expect(
        authored_track_replacement_requires_typed_support(),
        "reject course replacement outside its classified support");
    okay &= expect(
        authored_track_replacement_retains_ordinary_surface(),
        "retain replacement road outside its coarse support");
    okay &= expect(
        authored_track_replacement_stays_behind_nearer_geometry(),
        "keep course replacement behind nearer ordinary geometry");
    okay &= expect(
        authored_track_overlay_stays_clipped_beyond_near_plane(),
        "leave near-plane clipping to homogeneous hardware clipping");
    okay &= expect(
        background_layer_does_not_occlude_world_depth(),
        "schedule projected background before coherent physical world depth");
    okay &= expect(
        reset_isolates_async_readback_generation(),
        "isolate asynchronous pixels and queries across a temporal reset");
    okay &= expect(
        async_frames_preserve_mutable_inputs(),
        "preserve each asynchronous frame's mutable GPU inputs");
    okay &= expect(
        broad_vehicle_reflection_keeps_authored_strength(),
        "preserve authored additive strength for broad vehicle reflections");
    okay &= expect(
        small_additive_vehicle_detail_keeps_ps1_blend(),
        "preserve ordinary PS1 blending for small additive vehicle detail");
    okay &= expect(
        vehicle_reflection_strength_survives_tessellation_changes(),
        "retain reflection strength when camera-dependent tessellation changes exact-pair ratios");
    okay &= expect(curved_body_reflection_follows_authored_subdivision(),
        "keep reflection on every interior pixel of a subdivided nonplanar body quad");
    okay &= expect(curved_body_reflection_follows_authored_subdivision(true),
        "retain foreground occlusion when reflection follows subdivided body geometry");
    okay &= expect(
        vehicle_reflection_requires_visible_base_owner(),
        "require an opaque body owner beneath stock vehicle reflection");
    okay &= expect(
        vehicle_reflection_preserves_zero_texel_transparency(),
        "preserve zero-texel transparency in stock vehicle reflection");
    okay &= expect(
        nearer_track_occludes_vehicle_reflection(),
        "keep stock vehicle reflection behind genuinely nearer track geometry");
    okay &= expect(
        is_soft_shadow(render_vehicle_shadow_center(false)),
        "recognize a vehicle shadow with no model-space-flat edge");
    okay &= expect(
        is_soft_shadow(render_vehicle_shadow_center(true)),
        "preserve soft shadows when clipped shadow triangles are batched");
    const auto authored = mirror_shell_coverage(31, false);
    okay &= expect(
        authored[0] == 0 && authored[1] == 0,
        "render GT2 authored wheel geometry by default");
    const auto clipped = mirror_shell_coverage(15, true);
    okay &= expect(
        clipped[0] > 0,
        "draw a mirror wheel shell inside the guest drawing area");
    okay &= expect(
        clipped[1] == 0,
        "clip a mirror wheel shell to the guest drawing area");
    const auto unclipped = mirror_shell_coverage(31, true);
    okay &= expect(
        unclipped[0] > clipped[0],
        "keep the full shell when the drawing area covers the display");
    const auto deep_legacy_request = render_uv_island(false, 1.0F);
    const auto deep_modern_request = render_uv_island(true, 1.0F);
    okay &= expect(
        !deep_legacy_request.empty() &&
            deep_legacy_request == deep_modern_request,
        "ignore legacy affine requests for deep 3D UV islands");
    const auto resident_deep_perspective =
        render_uv_island(true, 1.0F, true);
    okay &= expect(
        !resident_deep_perspective.empty() &&
            resident_deep_perspective == deep_modern_request,
        "use one perspective contract for reconstructed and resident 3D UVs");
    const auto crossing_legacy_request =
        render_uv_island(false, -4.0F, true);
    const auto crossing_modern_request =
        render_uv_island(true, -4.0F, true);
    okay &= expect(
        !crossing_legacy_request.empty() &&
            crossing_legacy_request == crossing_modern_request,
        "keep perspective correction fixed through homogeneous near clipping");
    const auto shallow_legacy_request = render_uv_island(false, 4.0F);
    const auto shallow_modern_request = render_uv_island(true, 4.0F);
    okay &= expect(
        !shallow_legacy_request.empty() &&
            shallow_legacy_request == shallow_modern_request,
        "ignore legacy affine requests for shallow 3D UV islands");
    if (!okay)
        return 1;
    std::puts("world GPU renderer tests passed");
    return 0;
}
