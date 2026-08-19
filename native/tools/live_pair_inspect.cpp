#include "opengt/image_writer.hpp"
#include "opengt/live_renderer_bridge.h"

#include <algorithm>
#include <cstdint>
#include <cstdio>
#include <filesystem>
#include <fstream>
#include <iterator>
#include <string>
#include <vector>

int main(int argc, char** argv) {
    const bool metrics_only = argc >= 2 &&
        std::string(argv[1]) == "--metrics-only";
    const int output_argument = metrics_only ? 2 : 1;
    const int first_capture_argument = metrics_only ? 3 : 2;
    if (argc - first_capture_argument < 2) {
        std::fprintf(
            stderr,
            "usage: opengt_live_pair_inspect [--metrics-only] "
            "<output-directory> "
            "<capture-1> <capture-2> [...]\n");
        return 2;
    }
    const std::filesystem::path output_directory(argv[output_argument]);
    std::filesystem::create_directories(output_directory);
    void* renderer = opengt_live_create();
    if (renderer == nullptr)
        return 1;
    std::uint32_t flags =
        OPENGT_LIVE_DEPTH | OPENGT_LIVE_TOPOLOGY |
        OPENGT_LIVE_PERSPECTIVE | OPENGT_LIVE_TEXTURE_SMOOTHING;
    if (std::getenv("OPENGT_PAIR_DISABLE_TEXTURE_SMOOTHING") != nullptr)
        flags &= ~OPENGT_LIVE_TEXTURE_SMOOTHING;
    const opengt_live_options options{
        sizeof(opengt_live_options),
        flags,
        4,
        0xFF402820U,
    };
    std::vector<std::uint8_t> first(4096U * 4096U * 4U);
    std::vector<std::uint8_t> second(first.size());
    std::vector<std::uint64_t> pair_samples;
    std::uint32_t written = 0;
    std::uint32_t rendered_outputs = 0;
    std::uint32_t capture_count = 0;
    for (int argument = first_capture_argument; argument < argc; ++argument) {
        ++capture_count;
        std::ifstream stream(argv[argument], std::ios::binary);
        if (!stream) {
            std::fprintf(stderr, "cannot read %s\n", argv[argument]);
            opengt_live_destroy(renderer);
            return 1;
        }
        const std::vector<std::uint8_t> capture{
            std::istreambuf_iterator<char>(stream),
            std::istreambuf_iterator<char>()};
        opengt_live_stats first_stats{};
        first_stats.struct_size = sizeof(first_stats);
        opengt_live_stats second_stats{};
        second_stats.struct_size = sizeof(second_stats);
        opengt_live_interpolation_stats interpolation{};
        interpolation.struct_size = sizeof(interpolation);
        const int result = opengt_live_render_pair(
            renderer,
            capture.data(),
            capture.size(),
            first.data(),
            second.data(),
            first.size(),
            &options,
            &first_stats,
            &second_stats,
            &interpolation);
        if (result != 0) {
            std::fprintf(
                stderr,
                "pair failed capture=%s result=%d detail=%u\n",
                argv[argument],
                result,
                interpolation.result);
            opengt_live_destroy(renderer);
            return 1;
        }
        rendered_outputs += interpolation.output_count;
        if (interpolation.temporal_reset == 0)
            pair_samples.push_back(interpolation.pair_pipeline_microseconds);
        std::printf(
            "capture=%s outputs=%u reset=%u pairMs=%.3f interpMs=%.3f "
            "midMs=%.3f actualMs=%.3f matched=%u/%u worldEligible=%u "
            "currentTopoMs=%.3f firstTopoMs=%.3f secondTopoMs=%.3f "
            "groups=%u/%u/%u "
            "exactGroups=%u incoherentExactGroups=%u "
            "heldIncoherentVehicle=%u heldTrackVisibility=%u "
            "heldUnsafeTrack=%u firstReserved=%u secondReserved=%u "
            "uploadReuse=%u\n",
            argv[argument],
            interpolation.output_count,
            interpolation.temporal_reset,
            interpolation.pair_pipeline_microseconds / 1000.0,
            interpolation.interpolation_microseconds / 1000.0,
            interpolation.midpoint_render_microseconds / 1000.0,
            interpolation.actual_render_microseconds / 1000.0,
            interpolation.matched_commands,
            interpolation.previous_commands,
            interpolation.eligible_world_commands,
            interpolation.current_topology_microseconds / 1000.0,
            first_stats.topology_microseconds / 1000.0,
            second_stats.topology_microseconds / 1000.0,
            interpolation.matched_transform_groups,
            interpolation.previous_transform_groups,
            interpolation.current_transform_groups,
            interpolation.exact_rigid_transform_groups,
            interpolation.incoherent_exact_transform_groups,
            interpolation.held_incoherent_vehicle_commands,
            interpolation.held_track_visibility_commands,
            interpolation.held_unsafe_track_commands,
            first_stats.reserved,
            second_stats.reserved,
            interpolation.reserved & 1U);
        std::fflush(stdout);
        const auto write = [&](const std::vector<std::uint8_t>& pixels,
                               const opengt_live_stats& stats,
                               const char* suffix) {
            const auto path = output_directory /
                (std::string("frame-") +
                    (written < 9 ? "0" : "") +
                    std::to_string(++written) + suffix + ".png");
            return opengt::render::write_rgba_png(
                path.string().c_str(),
                pixels.data(),
                stats.output_width,
                stats.output_height);
        };
        if (
            !metrics_only &&
            interpolation.output_count >= 1 &&
            !write(
                first,
                first_stats,
                (first_stats.reserved & 1U) != 0
                    ? "-midpoint"
                    : "-actual")
        )
            return 1;
        if (
            !metrics_only &&
            interpolation.output_count >= 2 &&
            !write(
                second,
                second_stats,
                (second_stats.reserved & 1U) != 0
                    ? "-midpoint"
                    : "-actual")
        )
            return 1;
    }
    if (pair_samples.empty()) {
        std::fprintf(stderr, "no temporal pairs were produced\n");
        opengt_live_destroy(renderer);
        return 1;
    }
    std::sort(pair_samples.begin(), pair_samples.end());
    const auto percentile = [&](double fraction) {
        const std::size_t index = std::min(
            pair_samples.size() - 1,
            static_cast<std::size_t>(
                pair_samples.size() * fraction));
        return pair_samples[index] / 1000.0;
    };
    std::printf(
        "captures=%u temporalPairs=%zu outputs=%u images=%u "
        "p50PairMs=%.3f p95PairMs=%.3f "
        "p99PairMs=%.3f\n",
        capture_count,
        pair_samples.size(),
        rendered_outputs,
        written,
        percentile(0.50),
        percentile(0.95),
        percentile(0.99));
    std::fflush(stdout);
    opengt_live_destroy(renderer);
    return 0;
}
