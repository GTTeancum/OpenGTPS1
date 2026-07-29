#include "opengt/live_renderer_bridge.h"

#include <algorithm>
#include <chrono>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <fstream>
#include <iterator>
#include <vector>

int main(int argc, char** argv) {
    if (argc < 2 || argc > 4) {
        std::fprintf(
            stderr,
            "usage: opengt_live_benchmark <capture.ogtwcap> "
            "[frames] [scale]\n");
        return 2;
    }
    const int frames = argc >= 3
        ? std::max(1, std::atoi(argv[2]))
        : 30;
    const int scale = argc == 4
        ? std::clamp(std::atoi(argv[3]), 1, 8)
        : 1;
    std::ifstream stream(argv[1], std::ios::binary);
    if (!stream) {
        std::fprintf(stderr, "cannot read %s\n", argv[1]);
        return 1;
    }
    std::vector<std::uint8_t> capture_bytes{
        std::istreambuf_iterator<char>(stream),
        std::istreambuf_iterator<char>()};
    std::vector<std::uint8_t> output(4096U * 4096U * 4U);
    std::printf(
        "captureBytes=%zu requestedFrames=%d scale=%d api=%u\n",
        capture_bytes.size(),
        frames,
        scale,
        opengt_live_api_version());
    std::fflush(stdout);
    void* renderer = opengt_live_create();
    if (renderer == nullptr) {
        std::fprintf(stderr, "cannot create renderer\n");
        return 1;
    }
    const opengt_live_options options{
        sizeof(opengt_live_options),
        OPENGT_LIVE_DEPTH |
            OPENGT_LIVE_TOPOLOGY |
            OPENGT_LIVE_PERSPECTIVE,
        static_cast<std::uint32_t>(scale),
        0xFF402820U,
    };
    std::uint64_t total_native_us = 0;
    const auto started = std::chrono::steady_clock::now();
    for (int index = 0; index < frames; ++index) {
        opengt_live_stats stats{};
        stats.struct_size = sizeof(stats);
        const int result = opengt_live_render(
            renderer,
            capture_bytes.data(),
            capture_bytes.size(),
            output.data(),
            output.size(),
            &options,
            &stats);
        if (result != 0) {
            std::fprintf(
                stderr,
                "frame %d failed result=%d detail=%u\n",
                index,
                result,
                stats.result);
            opengt_live_destroy(renderer);
            return 1;
        }
        total_native_us += stats.render_microseconds;
        if (index == 0 || index + 1 == frames) {
            std::printf(
                "frame=%d commands=%u drawCalls=%u "
                "nativeMs=%.3f size=%ux%u\n",
                index,
                stats.output_commands,
                stats.draw_calls,
                stats.render_microseconds / 1000.0,
                stats.output_width,
                stats.output_height);
            std::fflush(stdout);
        }
    }
    const auto finished = std::chrono::steady_clock::now();
    const double wall_ms =
        std::chrono::duration<double, std::milli>(
            finished - started).count();
    std::printf(
        "frames=%d wallMs=%.3f averageWallMs=%.3f "
        "averageNativeMs=%.3f\n",
        frames,
        wall_ms,
        wall_ms / frames,
        total_native_us / 1000.0 / frames);
    std::fflush(stdout);
    opengt_live_destroy(renderer);
    return 0;
}
