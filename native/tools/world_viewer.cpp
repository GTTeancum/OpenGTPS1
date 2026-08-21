#include "opengt/image_writer.hpp"
#include "opengt/projected_reference_renderer.hpp"
#include "opengt/world_capture.hpp"
#include "opengt/world_draw_list.hpp"
#include "opengt/world_gpu_renderer.hpp"
#include "opengt/world_topology.hpp"

#include <algorithm>
#include <cmath>
#include <cstdlib>
#include <cstdio>
#include <cstring>
#include <vector>

#if defined(_WIN32)
#define NOMINMAX
#include <windows.h>
#endif

namespace {

std::uint64_t fnv1a64(
    const std::uint8_t* data,
    std::size_t size
) {
    std::uint64_t value = 14695981039346656037ULL;
    for (std::size_t index = 0; index < size; ++index) {
        value ^= data[index];
        value *= 1099511628211ULL;
    }
    return value;
}

std::vector<opengt::render::ProjectedCaptureTriangle> make_oracle_input(
    const opengt::render::WorldDrawList& draw_list
) {
    std::vector<opengt::render::ProjectedCaptureTriangle> result;
    result.reserve(draw_list.commands.size());
    for (const auto& command : draw_list.commands) {
        const auto& material =
            draw_list.materials[command.material_index];
        opengt::render::ProjectedCaptureTriangle triangle{};
        triangle.primitive_flags = material.primitive_flags;
        triangle.texture_page = material.texture_page;
        triangle.clut = material.clut;
        triangle.ordering_table_index =
            command.ordering_table_index;
        triangle.clip_x0 = command.clip_x0;
        triangle.clip_y0 = command.clip_y0;
        triangle.clip_x1 = command.clip_x1;
        triangle.clip_y1 = command.clip_y1;
        triangle.texture_mask_x = material.texture_mask_x;
        triangle.texture_mask_y = material.texture_mask_y;
        triangle.texture_offset_x = material.texture_offset_x;
        triangle.texture_offset_y = material.texture_offset_y;
        triangle.environment_flags = material.environment_flags;
        for (int index = 0; index < 3; ++index) {
            const auto& source = command.vertices[index];
            auto& destination = triangle.vertices[index];
            destination.x = source.screen_x;
            destination.y = source.screen_y;
            destination.z = source.view_z;
            destination.u =
                static_cast<std::int16_t>(source.u);
            destination.v =
                static_cast<std::int16_t>(source.v);
            destination.r = source.r;
            destination.g = source.g;
            destination.b = source.b;
            destination.has_gte_depth = true;
        }
        result.push_back(triangle);
    }
    return result;
}

struct Difference {
    double rms;
    int maximum;
    std::uint64_t changed_pixels;
};

Difference difference(
    const std::vector<std::uint8_t>& left,
    const std::vector<std::uint8_t>& right
) {
    double squared = 0.0;
    int maximum = 0;
    std::uint64_t changed_pixels = 0;
    for (std::size_t pixel = 0; pixel < left.size() / 4; ++pixel) {
        bool changed = false;
        for (int channel = 0; channel < 3; ++channel) {
            const int delta =
                static_cast<int>(left[pixel * 4 + channel]) -
                static_cast<int>(right[pixel * 4 + channel]);
            squared += static_cast<double>(delta) * delta;
            maximum = std::max(maximum, std::abs(delta));
            changed = changed || delta != 0;
        }
        if (changed)
            ++changed_pixels;
    }
    const std::size_t samples = left.size() / 4 * 3;
    return Difference{
        samples == 0 ? 0.0 : std::sqrt(squared / samples),
        maximum,
        changed_pixels,
    };
}

#if defined(_WIN32)

std::vector<std::uint8_t> window_bgra;
int window_width;
int window_height;

LRESULT CALLBACK window_procedure(
    HWND window,
    UINT message,
    WPARAM wparam,
    LPARAM lparam
) {
    if (message == WM_DESTROY) {
        PostQuitMessage(0);
        return 0;
    }
    if (message == WM_PAINT) {
        PAINTSTRUCT paint{};
        HDC device = BeginPaint(window, &paint);
        RECT client{};
        GetClientRect(window, &client);
        BITMAPINFO info{};
        info.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
        info.bmiHeader.biWidth = window_width;
        info.bmiHeader.biHeight = -window_height;
        info.bmiHeader.biPlanes = 1;
        info.bmiHeader.biBitCount = 32;
        info.bmiHeader.biCompression = BI_RGB;
        StretchDIBits(
            device,
            0,
            0,
            client.right,
            client.bottom,
            0,
            0,
            window_width,
            window_height,
            window_bgra.data(),
            &info,
            DIB_RGB_COLORS,
            SRCCOPY);
        EndPaint(window, &paint);
        return 0;
    }
    return DefWindowProcW(window, message, wparam, lparam);
}

bool show_window(
    const std::vector<std::uint8_t>& rgba,
    int width,
    int height
) {
    window_bgra.resize(rgba.size());
    for (std::size_t index = 0; index < rgba.size(); index += 4) {
        window_bgra[index] = rgba[index + 2];
        window_bgra[index + 1] = rgba[index + 1];
        window_bgra[index + 2] = rgba[index];
        window_bgra[index + 3] = rgba[index + 3];
    }
    window_width = width;
    window_height = height;
    const HINSTANCE instance = GetModuleHandleW(nullptr);
    const wchar_t class_name[] = L"OpenGTWorldViewer";
    WNDCLASSW window_class{};
    window_class.lpfnWndProc = window_procedure;
    window_class.hInstance = instance;
    window_class.hCursor =
        LoadCursorW(nullptr, MAKEINTRESOURCEW(32512));
    window_class.lpszClassName = class_name;
    if (
        RegisterClassW(&window_class) == 0 &&
        GetLastError() != ERROR_CLASS_ALREADY_EXISTS
    )
        return false;
    const int display_scale = width <= 640 ? 4 : 1;
    RECT rectangle{
        0, 0, width * display_scale, height * display_scale};
    AdjustWindowRect(&rectangle, WS_OVERLAPPEDWINDOW, FALSE);
    HWND window = CreateWindowExW(
        0,
        class_name,
        L"OpenGT modern world renderer",
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT,
        CW_USEDEFAULT,
        rectangle.right - rectangle.left,
        rectangle.bottom - rectangle.top,
        nullptr,
        nullptr,
        instance,
        nullptr);
    if (window == nullptr)
        return false;
    ShowWindow(window, SW_SHOW);
    MSG message{};
    while (GetMessageW(&message, nullptr, 0, 0) > 0) {
        TranslateMessage(&message);
        DispatchMessageW(&message);
    }
    return true;
}

#endif

} // namespace

int main(int argc, char** argv) {
    if (argc < 3) {
        std::fprintf(
            stderr,
            "usage: opengt_world_viewer <capture.ogtwcap> <gpu.png> "
            "[--warp] [--no-depth] [--dither] "
            "[--no-topology] [--include-secondary] "
            "[--include-screen-space] "
            "[--no-texture-smoothing] "
            "[--scale <1-8>] [--clear-color <RRGGBB>] "
            "[--inspect-pixel <x> <y>] "
            "[--oracle <oracle.png>] [--window]\n");
        return 2;
    }
    bool warp = false;
    bool depth = true;
    bool dither = false;
    bool window = false;
    bool topology = true;
    bool include_secondary = false;
    bool include_screen_space = false;
    bool texture_smoothing = true;
    std::uint32_t scale = 1;
    std::uint32_t clear_color = 0xFF402820U;
    int inspect_x = -1;
    int inspect_y = -1;
    const char* oracle_path = nullptr;
    for (int index = 3; index < argc; ++index) {
        if (std::strcmp(argv[index], "--warp") == 0)
            warp = true;
        else if (std::strcmp(argv[index], "--no-depth") == 0)
            depth = false;
        else if (std::strcmp(argv[index], "--dither") == 0)
            dither = true;
        else if (std::strcmp(argv[index], "--window") == 0)
            window = true;
        else if (std::strcmp(argv[index], "--no-topology") == 0)
            topology = false;
        else if (std::strcmp(argv[index], "--include-secondary") == 0)
            include_secondary = true;
        else if (std::strcmp(argv[index], "--include-screen-space") == 0)
            include_screen_space = true;
        else if (std::strcmp(argv[index], "--no-texture-smoothing") == 0)
            texture_smoothing = false;
        else if (
            std::strcmp(argv[index], "--clear-color") == 0 &&
            index + 1 < argc
        ) {
            char* end = nullptr;
            const unsigned long parsed =
                std::strtoul(argv[++index], &end, 16);
            if (end == argv[index] || *end != '\0' || parsed > 0xFFFFFFUL) {
                std::fprintf(stderr, "--clear-color must be RRGGBB hex\n");
                return 2;
            }
            clear_color =
                0xFF000000U |
                ((parsed & 0x0000FFUL) << 16) |
                (parsed & 0x00FF00UL) |
                ((parsed & 0xFF0000UL) >> 16);
        }
        else if (
            std::strcmp(argv[index], "--inspect-pixel") == 0 &&
            index + 2 < argc
        ) {
            inspect_x = static_cast<int>(
                std::strtol(argv[++index], nullptr, 10));
            inspect_y = static_cast<int>(
                std::strtol(argv[++index], nullptr, 10));
        }
        else if (
            std::strcmp(argv[index], "--scale") == 0 &&
            index + 1 < argc
        ) {
            const long parsed = std::strtol(argv[++index], nullptr, 10);
            if (parsed < 1 || parsed > 8) {
                std::fprintf(stderr, "--scale must be between 1 and 8\n");
                return 2;
            }
            scale = static_cast<std::uint32_t>(parsed);
        }
        else if (
            std::strcmp(argv[index], "--oracle") == 0 &&
            index + 1 < argc
        )
            oracle_path = argv[++index];
        else {
            std::fprintf(stderr, "unknown option: %s\n", argv[index]);
            return 2;
        }
    }

    using namespace opengt::render;
    WorldCaptureHeader header{};
    auto read_result =
        read_world_capture_header(argv[1], &header);
    if (read_result != WorldCaptureReadResult::success) {
        std::fprintf(
            stderr,
            "capture header failed: %s\n",
            world_capture_read_result_name(read_result));
        return 1;
    }
    std::vector<WorldCaptureTriangle> triangles(
        header.triangle_count);
    std::vector<std::uint16_t> vram(1024U * 512U);
    read_result = load_world_capture(
        argv[1],
        &header,
        triangles.data(),
        triangles.size(),
        vram.data(),
        vram.size());
    if (read_result != WorldCaptureReadResult::success) {
        std::fprintf(
            stderr,
            "capture load failed: %s\n",
            world_capture_read_result_name(read_result));
        return 1;
    }

    WorldDrawList draw_list{};
    const auto list_result = build_world_draw_list(
        header,
        triangles.data(),
        triangles.size(),
        WorldDrawListOptions{
            include_secondary,
            include_screen_space,
            scale > 1},
        &draw_list);
    if (list_result != WorldDrawListResult::success) {
        std::fprintf(
            stderr,
            "draw-list build failed: %s\n",
            world_draw_list_result_name(list_result));
        return 1;
    }
    const WorldDrawList compatibility_draw_list = draw_list;
    WorldTopologyStats topology_stats{};
    if (topology) {
        const auto topology_result = apply_world_topology(
            &draw_list,
            WorldTopologyOptions{true, true, true},
            &topology_stats);
        if (topology_result != WorldTopologyResult::success) {
            std::fprintf(
                stderr,
                "topology pass failed: %s\n",
                world_topology_result_name(topology_result));
            return 1;
        }
    }
    if (inspect_x >= 0 && inspect_y >= 0) {
        // Inspector coordinates are expressed in the written output image,
        // while captured primitives retain their absolute VRAM display origin.
        // D3D rasterization evaluates coverage and interpolants at the output
        // pixel centre, not at its upper-left boundary.
        const float x = header.display_x +
            (static_cast<float>(inspect_x) + 0.5F) / scale;
        const float y = header.display_y +
            (static_cast<float>(inspect_y) + 0.5F) / scale;
        const auto edge = [](const WorldDrawVertex& a,
                             const WorldDrawVertex& b,
                             float px,
                             float py) {
            return (px - a.screen_x) * (b.screen_y - a.screen_y) -
                (py - a.screen_y) * (b.screen_x - a.screen_x);
        };
        const auto vram_at = [&] (int px, int py) {
            return vram[
                static_cast<std::size_t>(py & 511) * 1024U +
                static_cast<std::size_t>(px & 1023)];
        };
        const auto texture_word = [&] (
            int raw_u,
            int raw_v,
            const WorldMaterial& material
        ) {
            int u =
                (raw_u & ~(material.texture_mask_x * 8)) |
                ((material.texture_offset_x &
                    material.texture_mask_x) * 8);
            int v =
                (raw_v & ~(material.texture_mask_y * 8)) |
                ((material.texture_offset_y &
                    material.texture_mask_y) * 8);
            u &= 255;
            v &= 255;
            const int page_x = (material.texture_page & 15) * 64;
            const int page_y =
                ((material.texture_page >> 4) & 1) * 256;
            const int mode = (material.texture_page >> 7) & 3;
            const int clut_x = (material.clut & 63) * 16;
            const int clut_y = (material.clut >> 6) & 511;
            if (mode == 0) {
                const std::uint16_t packed = vram_at(
                    page_x + (u >> 2), page_y + v);
                const int index =
                    (packed >> ((u & 3) * 4)) & 15;
                return vram_at(clut_x + index, clut_y);
            }
            if (mode == 1) {
                const std::uint16_t packed = vram_at(
                    page_x + (u >> 1), page_y + v);
                const int index =
                    (packed >> ((u & 1) * 8)) & 255;
                return vram_at(clut_x + index, clut_y);
            }
            return vram_at(page_x + u, page_y + v);
        };
        std::printf(
            "inspect output=(%d,%d) native=(%.3f,%.3f)\n",
            inspect_x, inspect_y, x, y);
        for (std::size_t command_index = 0;
             command_index < draw_list.commands.size();
             ++command_index) {
            const auto& command = draw_list.commands[command_index];
            float minimum_x = command.vertices[0].screen_x;
            float maximum_x = minimum_x;
            float minimum_y = command.vertices[0].screen_y;
            float maximum_y = minimum_y;
            for (int vertex = 1; vertex < 3; ++vertex) {
                minimum_x = std::min(
                    minimum_x, command.vertices[vertex].screen_x);
                maximum_x = std::max(
                    maximum_x, command.vertices[vertex].screen_x);
                minimum_y = std::min(
                    minimum_y, command.vertices[vertex].screen_y);
                maximum_y = std::max(
                    maximum_y, command.vertices[vertex].screen_y);
            }
            if (
                x < minimum_x - 2.0F || x > maximum_x + 2.0F ||
                y < minimum_y - 2.0F || y > maximum_y + 2.0F
            )
                continue;
            const float e0 = edge(
                command.vertices[0], command.vertices[1], x, y);
            const float e1 = edge(
                command.vertices[1], command.vertices[2], x, y);
            const float e2 = edge(
                command.vertices[2], command.vertices[0], x, y);
            const bool inside =
                (e0 >= 0.0F && e1 >= 0.0F && e2 >= 0.0F) ||
                (e0 <= 0.0F && e1 <= 0.0F && e2 <= 0.0F);
            const auto& material =
                draw_list.materials[command.material_index];
            std::printf(
                "  cmd=%zu inside=%s kind=%u object=%u model=%08x "
                "transform=%016llx ot=%d source=%u material=%u flags=%08x "
                "tpage=%04x clut=%04x "
                "window=(%d,%d,%d,%d) "
                "p=(%.3f,%.3f)(%.3f,%.3f)(%.3f,%.3f) "
                "uv=(%.3f,%.3f)(%.3f,%.3f)(%.3f,%.3f) "
                "model=(%d,%d,%d)(%d,%d,%d)(%d,%d,%d) "
                "view=(%d,%d,%d)(%d,%d,%d)(%d,%d,%d) "
                "edge=(%.3f,%.3f,%.3f)\n",
                command_index,
                inside ? "yes" : "no",
                command.object_kind,
                command.object_id,
                command.model_pointer,
                static_cast<unsigned long long>(command.transform_id),
                command.ordering_table_index,
                command.source_command_index,
                command.material_index,
                material.primitive_flags,
                material.texture_page,
                material.clut,
                material.texture_mask_x,
                material.texture_mask_y,
                material.texture_offset_x,
                material.texture_offset_y,
                command.vertices[0].screen_x,
                command.vertices[0].screen_y,
                command.vertices[1].screen_x,
                command.vertices[1].screen_y,
                command.vertices[2].screen_x,
                command.vertices[2].screen_y,
                command.vertices[0].u,
                command.vertices[0].v,
                command.vertices[1].u,
                command.vertices[1].v,
                command.vertices[2].u,
                command.vertices[2].v,
                command.vertices[0].model_x,
                command.vertices[0].model_y,
                command.vertices[0].model_z,
                command.vertices[1].model_x,
                command.vertices[1].model_y,
                command.vertices[1].model_z,
                command.vertices[2].model_x,
                command.vertices[2].model_y,
                command.vertices[2].model_z,
                command.vertices[0].exact_view_x,
                command.vertices[0].exact_view_y,
                command.vertices[0].exact_view_z,
                command.vertices[1].exact_view_x,
                command.vertices[1].exact_view_y,
                command.vertices[1].exact_view_z,
                command.vertices[2].exact_view_x,
                command.vertices[2].exact_view_y,
                command.vertices[2].exact_view_z,
                e0, e1, e2);
            if (inside && (material.primitive_flags & 1U) != 0) {
                const auto& a = command.vertices[0];
                const auto& b = command.vertices[1];
                const auto& c = command.vertices[2];
                const float denominator =
                    (b.screen_y - c.screen_y) *
                        (a.screen_x - c.screen_x) +
                    (c.screen_x - b.screen_x) *
                        (a.screen_y - c.screen_y);
                if (std::abs(denominator) > 0.000001F) {
                    const float lambda_a =
                        ((b.screen_y - c.screen_y) *
                            (x - c.screen_x) +
                        (c.screen_x - b.screen_x) *
                            (y - c.screen_y)) / denominator;
                    const float lambda_b =
                        ((c.screen_y - a.screen_y) *
                            (x - c.screen_x) +
                        (a.screen_x - c.screen_x) *
                            (y - c.screen_y)) / denominator;
                    const float lambda_c = 1.0F - lambda_a - lambda_b;
                    const float reciprocal_w =
                        lambda_a / a.clip_w +
                        lambda_b / b.clip_w +
                        lambda_c / c.clip_w;
                    const float u =
                        (lambda_a * a.u / a.clip_w +
                            lambda_b * b.u / b.clip_w +
                            lambda_c * c.u / c.clip_w) /
                        reciprocal_w;
                    const float v =
                        (lambda_a * a.v / a.clip_w +
                            lambda_b * b.v / b.clip_w +
                            lambda_c * c.v / c.clip_w) /
                        reciprocal_w;
                    const bool screen_space =
                        (material.primitive_flags &
                            world_primitive_screen_space_flag) != 0;
                    const int sample_u = static_cast<int>(std::floor(
                        u + (screen_space ? 0.0F : 0.5F)));
                    const int sample_v = static_cast<int>(std::floor(
                        v + (screen_space ? 0.0F : 0.5F)));
                    const std::int64_t ab_x =
                        static_cast<std::int64_t>(b.model_x) - a.model_x;
                    const std::int64_t ab_y =
                        static_cast<std::int64_t>(b.model_y) - a.model_y;
                    const std::int64_t ab_z =
                        static_cast<std::int64_t>(b.model_z) - a.model_z;
                    const std::int64_t ac_x =
                        static_cast<std::int64_t>(c.model_x) - a.model_x;
                    const std::int64_t ac_y =
                        static_cast<std::int64_t>(c.model_y) - a.model_y;
                    const std::int64_t ac_z =
                        static_cast<std::int64_t>(c.model_z) - a.model_z;
                    const std::int64_t normal_x = ab_y * ac_z - ab_z * ac_y;
                    const std::int64_t normal_y = ab_z * ac_x - ab_x * ac_z;
                    const std::int64_t normal_z = ab_x * ac_y - ab_y * ac_x;
                    const bool recover_opaque_track =
                        command.object_kind == 1U &&
                        (material.primitive_flags & 2U) == 0 &&
                        !screen_space &&
                        std::llabs(normal_y) >= std::llabs(normal_x) &&
                        std::llabs(normal_y) >= std::llabs(normal_z);
                    std::printf(
                        "    sample uv=(%.4f,%.4f) texel=(%d,%d) "
                        "word=%04x neighbors=%04x,%04x,%04x,%04x "
                        "opaqueTrackRecovery=%s normal=(%lld,%lld,%lld)\n",
                        u,
                        v,
                        sample_u,
                        sample_v,
                        texture_word(sample_u, sample_v, material),
                        texture_word(sample_u - 1, sample_v, material),
                        texture_word(sample_u + 1, sample_v, material),
                        texture_word(sample_u, sample_v - 1, material),
                        texture_word(sample_u, sample_v + 1, material),
                        recover_opaque_track ? "yes" : "no",
                        static_cast<long long>(normal_x),
                        static_cast<long long>(normal_y),
                        static_cast<long long>(normal_z));
                }
            }
        }
    }
    const std::uint32_t output_width =
        static_cast<std::uint32_t>(header.display_width) * scale;
    const std::uint32_t output_height =
        static_cast<std::uint32_t>(header.display_height) * scale;
    const std::size_t output_size =
        static_cast<std::size_t>(output_width) *
        output_height * 4;
    std::vector<std::uint8_t> gpu(output_size);
    WorldGpuRenderStats gpu_stats{};
    const WorldGpuRenderOptions options{
        warp,
        depth,
        dither,
        true,
        texture_smoothing,
        false,
        scale,
        clear_color,
    };
    const auto gpu_result = render_world_d3d11(
        draw_list,
        vram.data(),
        vram.size(),
        gpu.data(),
        gpu.size(),
        options,
        &gpu_stats);
    if (gpu_result != WorldGpuRenderResult::success) {
        std::fprintf(
            stderr,
            "GPU render failed: %s\n",
            world_gpu_render_result_name(gpu_result));
        return 1;
    }
    if (!write_rgba_png(
            argv[2],
            gpu.data(),
            output_width,
            output_height)) {
        std::fprintf(stderr, "cannot write %s\n", argv[2]);
        return 1;
    }

    const auto oracle_triangles =
        make_oracle_input(compatibility_draw_list);
    ProjectedCaptureHeader oracle_header{};
    oracle_header.frame_index = header.frame_index;
    oracle_header.input_poll = header.input_poll;
    oracle_header.display_x = header.display_x;
    oracle_header.display_y = header.display_y;
    oracle_header.display_width = header.display_width;
    oracle_header.display_height = header.display_height;
    oracle_header.triangle_count =
        static_cast<std::uint32_t>(oracle_triangles.size());
    oracle_header.vram_width = 1024;
    oracle_header.vram_height = 512;
    const std::size_t reference_size =
        static_cast<std::size_t>(header.display_width) *
        header.display_height * 4;
    std::vector<std::uint8_t> oracle(reference_size);
    const auto oracle_stats = render_projected_capture(
        oracle_header,
        oracle_triangles.data(),
        oracle_triangles.size(),
        vram.data(),
        vram.size(),
        oracle.data(),
        oracle.size(),
        ProjectedRenderOptions{
            true,
            dither,
            clear_color,
        });
    if (
        oracle_stats.submitted_triangles !=
        oracle_triangles.size()
    ) {
        std::fprintf(stderr, "compatibility oracle failed\n");
        return 1;
    }
    if (
        oracle_path != nullptr &&
        !write_rgba_png(
            oracle_path,
            oracle.data(),
            header.display_width,
            header.display_height)
    ) {
        std::fprintf(stderr, "cannot write %s\n", oracle_path);
        return 1;
    }

    std::vector<std::uint8_t> comparison_gpu(reference_size);
    WorldGpuRenderStats comparison_stats{};
    const auto comparison_result = render_world_d3d11(
        compatibility_draw_list,
        vram.data(),
        vram.size(),
        comparison_gpu.data(),
        comparison_gpu.size(),
        WorldGpuRenderOptions{
            true,
            false,
            dither,
            true,
            false,
            false,
            1,
            clear_color,
        },
        &comparison_stats);
    if (comparison_result != WorldGpuRenderResult::success) {
        std::fprintf(
            stderr,
            "comparison GPU render failed: %s\n",
            world_gpu_render_result_name(comparison_result));
        return 1;
    }
    const Difference comparison =
        difference(comparison_gpu, oracle);
    const double compatibility_psnr =
        comparison.rms == 0.0
            ? 999.0
            : 20.0 * std::log10(255.0 / comparison.rms);
    std::printf(
        "version=%u frame=%llu poll=%d adapter=%s resolution=%ux%u "
        "scale=%u depth=%s "
        "dither=%s commands=%u track=%u vehicles=%u unclassified=%u "
        "materials=%zu secondaryExcluded=%u "
        "topology=%s topologyInput=%u topologyOutput=%u "
        "topologyEligible=%u topologyMissingProvenance=%u "
        "topologySources=%u topologyPositionGroups=%u "
        "topologyBoundaryGroups=%u topologyAdjusted=%u "
        "topologyProjectionGroups=%u topologyProjectionAdjusted=%u "
        "topologySeamGroups=%u topologySeamAdjusted=%u "
        "topologyRasterGroups=%u topologyRasterAdjusted=%u "
        "topologyProjectedTJunctions=%u "
        "topologyProjectedTJunctionAdjusted=%u "
        "topologyBoundaryEdges=%u topologyManifoldEdges=%u "
        "topologyNonmanifoldEdges=%u topologyTJunctions=%u "
        "topologySplitSources=%u topologySplitTriangles=%u "
        "topologyCoplanarPairs=%u topologyMaterialPairs=%u "
        "topologyDuplicatePairs=%u topologyOwnershipGroups=%u "
        "topologyReorders=%u "
        "drawCalls=%u transparentDrawCalls=%u "
        "gpuHash=%016llx oracleHash=%016llx "
        "compatibilityRms=%.6f compatibilityPsnr=%.3f "
        "compatibilityMax=%d "
        "compatibilityChangedPixels=%llu output=%s oracle=%s\n",
        header.version,
        static_cast<unsigned long long>(header.frame_index),
        header.input_poll,
        warp ? "warp" : "hardware",
        output_width,
        output_height,
        scale,
        depth ? "on" : "off",
        dither ? "on" : "off",
        gpu_stats.commands,
        draw_list.track_commands,
        draw_list.vehicle_commands,
        draw_list.unclassified_commands,
        draw_list.materials.size(),
        draw_list.secondary_commands,
        topology ? "on" : "off",
        topology_stats.input_commands,
        topology_stats.output_commands,
        topology_stats.eligible_track_commands,
        topology_stats.skipped_without_provenance,
        topology_stats.unique_source_vertices,
        topology_stats.exact_position_groups,
        topology_stats.authored_boundary_groups,
        topology_stats.adjusted_vertex_instances,
        topology_stats.authored_projection_groups,
        topology_stats.adjusted_projection_instances,
        topology_stats.projected_seam_groups,
        topology_stats.adjusted_seam_instances,
        topology_stats.authored_raster_groups,
        topology_stats.adjusted_authored_raster_instances,
        topology_stats.projected_t_junctions,
        topology_stats.adjusted_projected_t_junction_instances,
        topology_stats.boundary_edges,
        topology_stats.manifold_edges,
        topology_stats.nonmanifold_edges,
        topology_stats.exact_t_junctions,
        topology_stats.split_source_triangles,
        topology_stats.emitted_split_triangles,
        topology_stats.coplanar_overlap_pairs,
        topology_stats.material_overlap_pairs,
        topology_stats.exact_duplicate_pairs,
        topology_stats.ownership_components,
        topology_stats.ownership_reorders,
        gpu_stats.draw_calls,
        gpu_stats.transparent_draw_calls,
        static_cast<unsigned long long>(
            fnv1a64(gpu.data(), gpu.size())),
        static_cast<unsigned long long>(
            fnv1a64(oracle.data(), oracle.size())),
        comparison.rms,
        compatibility_psnr,
        comparison.maximum,
        static_cast<unsigned long long>(
            comparison.changed_pixels),
        argv[2],
        oracle_path == nullptr ? "not-written" : oracle_path);
#if defined(_WIN32)
    if (window && !show_window(
            gpu,
            output_width,
            output_height)) {
        std::fprintf(stderr, "cannot open viewer window\n");
        return 1;
    }
#else
    if (window) {
        std::fprintf(stderr, "--window is only supported on Windows\n");
        return 1;
    }
#endif
    return 0;
}
