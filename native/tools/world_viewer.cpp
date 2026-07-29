#include "opengt/image_writer.hpp"
#include "opengt/projected_reference_renderer.hpp"
#include "opengt/world_capture.hpp"
#include "opengt/world_draw_list.hpp"
#include "opengt/world_gpu_renderer.hpp"

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
            "[--scale <1-8>] [--oracle <oracle.png>] [--window]\n");
        return 2;
    }
    bool warp = false;
    bool depth = true;
    bool dither = false;
    bool window = false;
    std::uint32_t scale = 1;
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
        WorldDrawListOptions{false},
        &draw_list);
    if (list_result != WorldDrawListResult::success) {
        std::fprintf(
            stderr,
            "draw-list build failed: %s\n",
            world_draw_list_result_name(list_result));
        return 1;
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
        scale,
        0xFF402820U,
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

    const auto oracle_triangles = make_oracle_input(draw_list);
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
            0xFF402820U,
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
        draw_list,
        vram.data(),
        vram.size(),
        comparison_gpu.data(),
        comparison_gpu.size(),
        WorldGpuRenderOptions{
            true,
            false,
            dither,
            1,
            0xFF402820U,
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
