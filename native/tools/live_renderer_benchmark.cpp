#include "opengt/live_renderer_bridge.h"

#include <algorithm>
#include <chrono>
#include <cmath>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <fstream>
#include <iterator>
#include <numeric>
#include <vector>

namespace {

double percentile_ms(
    std::vector<std::uint64_t> samples,
    double fraction
) {
    std::sort(samples.begin(), samples.end());
    const std::size_t index = std::min(
        samples.size() - 1,
        static_cast<std::size_t>(
            std::ceil(samples.size() * fraction) - 1));
    return samples[index] / 1000.0;
}

double average_ms(const std::vector<std::uint64_t>& samples) {
    return std::accumulate(
        samples.begin(), samples.end(), 0.0) /
        1000.0 / samples.size();
}

} // namespace

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
    constexpr int warmup_frames = 8;
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
        "captureBytes=%zu measuredFrames=%d warmupFrames=%d scale=%d "
        "api=%u gpuMetric=driver-submit-completion-readback\n",
        capture_bytes.size(),
        frames,
        warmup_frames,
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
            OPENGT_LIVE_PERSPECTIVE |
            OPENGT_LIVE_TEXTURE_SMOOTHING,
        static_cast<std::uint32_t>(scale),
        0xFF402820U,
    };
    std::vector<std::uint64_t> wall_samples;
    std::vector<std::uint64_t> gpu_driver_samples;
    std::vector<std::uint64_t> decode_samples;
    std::vector<std::uint64_t> draw_list_samples;
    std::vector<std::uint64_t> topology_samples;
    std::vector<std::uint64_t> pipeline_samples;
    wall_samples.reserve(frames);
    gpu_driver_samples.reserve(frames);
    decode_samples.reserve(frames);
    draw_list_samples.reserve(frames);
    topology_samples.reserve(frames);
    pipeline_samples.reserve(frames);
    const auto started = std::chrono::steady_clock::now();
    for (int index = -warmup_frames; index < frames; ++index) {
        opengt_live_stats stats{};
        stats.struct_size = sizeof(stats);
        const auto frame_started = std::chrono::steady_clock::now();
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
        const auto frame_finished = std::chrono::steady_clock::now();
        if (index < 0)
            continue;
        wall_samples.push_back(static_cast<std::uint64_t>(
            std::chrono::duration_cast<std::chrono::microseconds>(
                frame_finished - frame_started).count()));
        gpu_driver_samples.push_back(stats.render_microseconds);
        decode_samples.push_back(stats.decode_microseconds);
        draw_list_samples.push_back(stats.draw_list_microseconds);
        topology_samples.push_back(stats.topology_microseconds);
        pipeline_samples.push_back(stats.pipeline_microseconds);
        if (index == 0 || index + 1 == frames) {
            std::printf(
                "frame=%d commands=%u drawCalls=%u transparent=%u "
                "nativeMs=%.3f pipelineMs=%.3f size=%ux%u\n",
                index,
                stats.output_commands,
                stats.draw_calls,
                stats.transparent_draw_calls,
                stats.render_microseconds / 1000.0,
                stats.pipeline_microseconds / 1000.0,
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
        "frames=%d totalWallMs=%.3f "
        "wallMs=%.3f/%.3f/%.3f "
        "pipelineMs=%.3f/%.3f/%.3f "
        "gpuDriverMs=%.3f/%.3f/%.3f "
        "topologyMs=%.3f/%.3f/%.3f "
        "averagesMs=wall:%.3f,pipeline:%.3f,gpuDriver:%.3f,"
        "topology:%.3f,decode:%.3f,drawList:%.3f\n",
        frames,
        wall_ms,
        percentile_ms(wall_samples, 0.50),
        percentile_ms(wall_samples, 0.95),
        percentile_ms(wall_samples, 0.99),
        percentile_ms(pipeline_samples, 0.50),
        percentile_ms(pipeline_samples, 0.95),
        percentile_ms(pipeline_samples, 0.99),
        percentile_ms(gpu_driver_samples, 0.50),
        percentile_ms(gpu_driver_samples, 0.95),
        percentile_ms(gpu_driver_samples, 0.99),
        percentile_ms(topology_samples, 0.50),
        percentile_ms(topology_samples, 0.95),
        percentile_ms(topology_samples, 0.99),
        average_ms(wall_samples),
        average_ms(pipeline_samples),
        average_ms(gpu_driver_samples),
        average_ms(topology_samples),
        average_ms(decode_samples),
        average_ms(draw_list_samples));
    std::fflush(stdout);
    opengt_live_destroy(renderer);
    return 0;
}
