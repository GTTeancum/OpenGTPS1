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
    float near_depth
) {
    using namespace opengt::render;
    WorldDrawList list{};
    list.display_width = 32;
    list.display_height = 32;
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
        is_clear(render_center(2U, true, 4U)),
        "preserve transparent texels on vehicle geometry");
    okay &= expect(
        is_clear(render_center(1U, false, 4U)),
        "preserve transparent texels on a vertical track surface");
    okay &= expect(
        is_clear(render_center(1U, true, 2U)),
        "preserve an authored track-texture cutout");
    okay &= expect(
        is_green(render_fractional_uv_center()),
        "use PS1 floor semantics for fractional world-texture visibility");
    okay &= expect(
        reset_isolates_async_readback_generation(),
        "isolate asynchronous pixels and queries across a temporal reset");
    okay &= expect(
        async_frames_preserve_mutable_inputs(),
        "preserve each asynchronous frame's mutable GPU inputs");
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
    const auto deep_affine = render_uv_island(false, 1.0F);
    const auto deep_perspective = render_uv_island(true, 1.0F);
    okay &= expect(
        !deep_affine.empty() && deep_affine == deep_perspective,
        "keep a deep GT2 UV island on one affine interpolation contract");
    const auto shallow_affine = render_uv_island(false, 4.0F);
    const auto shallow_perspective = render_uv_island(true, 4.0F);
    okay &= expect(
        !shallow_affine.empty() &&
            shallow_affine != shallow_perspective,
        "retain perspective correction on a coherent shallow UV island");
    if (!okay)
        return 1;
    std::puts("world GPU renderer tests passed");
    return 0;
}
