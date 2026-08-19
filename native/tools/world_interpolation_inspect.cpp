#include "opengt/image_writer.hpp"
#include "opengt/world_capture.hpp"
#include "opengt/world_draw_list.hpp"
#include "opengt/world_gpu_renderer.hpp"
#include "opengt/world_interpolation.hpp"
#include "opengt/world_topology.hpp"

#include <cstdint>
#include <cstring>
#include <cstdio>
#include <cmath>
#include <cstdlib>
#include <map>
#include <limits>
#include <set>
#include <string>
#include <tuple>
#include <vector>

namespace {

struct Frame {
    opengt::render::WorldCaptureHeader header{};
    std::vector<opengt::render::WorldCaptureTriangle> triangles;
    std::vector<std::uint16_t> vram =
        std::vector<std::uint16_t>(1024U * 512U);
    opengt::render::WorldDrawList captured_draw_list;
    opengt::render::WorldDrawList draw_list;
};

bool load(const char* path, Frame* frame) {
    using namespace opengt::render;
    auto result = read_world_capture_header(path, &frame->header);
    if (result != WorldCaptureReadResult::success)
        return false;
    frame->triangles.resize(frame->header.triangle_count);
    result = load_world_capture(
        path,
        &frame->header,
        frame->triangles.data(),
        frame->triangles.size(),
        frame->vram.data(),
        frame->vram.size());
    if (result != WorldCaptureReadResult::success)
        return false;
    if (
        build_world_draw_list(
            frame->header,
            frame->triangles.data(),
            frame->triangles.size(),
            WorldDrawListOptions{true, true, true},
            &frame->draw_list) != WorldDrawListResult::success
    )
        return false;
    frame->captured_draw_list = frame->draw_list;
    WorldTopologyStats stats{};
    return
        apply_world_topology(
            &frame->draw_list,
            WorldTopologyOptions{true, true, true},
            &stats) == WorldTopologyResult::success;
}

void print_transform_groups(
    const char* label,
    const opengt::render::WorldDrawList& list,
    std::uint32_t object_kind,
    const char* kind_name
) {
    struct Group {
        std::uint32_t commands{};
        double x{};
        double y{};
        float minimum_x{(std::numeric_limits<float>::max)()};
        float minimum_y{(std::numeric_limits<float>::max)()};
        float maximum_x{(std::numeric_limits<float>::lowest)()};
        float maximum_y{(std::numeric_limits<float>::lowest)()};
        const opengt::render::WorldDrawCommand* command_sample{};
        const opengt::render::WorldDrawVertex* sample{};
        double exact_residual_squared{};
        double exact_residual_maximum{};
        std::uint32_t exact_residual_vertices{};
        std::uint32_t coherent_vertices{};
        std::uint32_t coherent_commands{};
        std::uint32_t mixed_commands{};
        double vertex_residual_squared{};
        double vertex_residual_maximum{};
        std::uint32_t vertex_transform_vertices{};
        std::uint32_t coherent_vertex_transforms{};
    };
    using Key = std::tuple<
        std::uint32_t,
        std::uint32_t,
        std::uint64_t,
        opengt::render::WorldViewChannel>;
    std::map<Key, Group> groups;
    for (const auto& command : list.commands) {
        if (command.object_kind != object_kind)
            continue;
        auto& group = groups[Key{
            command.object_id,
            command.model_pointer,
            command.transform_id,
            command.channel}];
        if (group.command_sample == nullptr)
            group.command_sample = &command;
        ++group.commands;
        std::uint32_t command_coherent_vertices = 0;
        for (const auto& vertex : command.vertices) {
            if (group.sample == nullptr)
                group.sample = &vertex;
            group.x += vertex.screen_x;
            group.y += vertex.screen_y - list.display_y;
            group.minimum_x = std::min(group.minimum_x, vertex.screen_x);
            group.minimum_y = std::min(
                group.minimum_y,
                vertex.screen_y - list.display_y);
            group.maximum_x = std::max(group.maximum_x, vertex.screen_x);
            group.maximum_y = std::max(
                group.maximum_y,
                vertex.screen_y - list.display_y);
            if (command.exact_transform_valid) {
                const double model[3] = {
                    static_cast<double>(vertex.model_x),
                    static_cast<double>(vertex.model_y),
                    static_cast<double>(vertex.model_z),
                };
                double predicted[3]{};
                for (int row = 0; row < 3; ++row) {
                    predicted[row] = command.transform_translation[row];
                    for (int column = 0; column < 3; ++column) {
                        predicted[row] +=
                            command.transform_rotation[row * 3 + column] *
                            model[column] / 4096.0;
                    }
                }
                const double dx = predicted[0] - vertex.view_x;
                const double dy = predicted[1] - vertex.view_y;
                const double dz = predicted[2] - vertex.view_z;
                const double residual = std::sqrt(
                    dx * dx + dy * dy + dz * dz);
                group.exact_residual_squared += residual * residual;
                group.exact_residual_maximum = std::max(
                    group.exact_residual_maximum, residual);
                if (residual <= 4.0) {
                    ++group.coherent_vertices;
                    ++command_coherent_vertices;
                }
                ++group.exact_residual_vertices;
            }
            if (vertex.exact_transform_valid) {
                const double model[3] = {
                    static_cast<double>(vertex.model_x),
                    static_cast<double>(vertex.model_y),
                    static_cast<double>(vertex.model_z),
                };
                double predicted[3]{};
                for (int row = 0; row < 3; ++row) {
                    predicted[row] = vertex.transform_translation[row];
                    for (int column = 0; column < 3; ++column) {
                        predicted[row] +=
                            vertex.transform_rotation[row * 3 + column] *
                            model[column] / 4096.0;
                    }
                }
                const double dx = predicted[0] - vertex.view_x;
                const double dy = predicted[1] - vertex.view_y;
                const double dz = predicted[2] - vertex.view_z;
                const double residual = std::sqrt(
                    dx * dx + dy * dy + dz * dz);
                group.vertex_residual_squared += residual * residual;
                group.vertex_residual_maximum = std::max(
                    group.vertex_residual_maximum, residual);
                if (residual <= 4.0)
                    ++group.coherent_vertex_transforms;
                ++group.vertex_transform_vertices;
            }
        }
        if (command_coherent_vertices == 3)
            ++group.coherent_commands;
        else if (command_coherent_vertices != 0)
            ++group.mixed_commands;
    }
    for (const auto& entry : groups) {
        const auto& [object, model, transform, channel] = entry.first;
        const auto& group = entry.second;
        const double divisor = group.commands * 3.0;
        std::printf(
            "%s %s object=%u model=%08x transform=%016llx "
            "channel=%u commands=%u centroid=(%.2f,%.2f)\n",
            label,
            kind_name,
            object,
            model,
            static_cast<unsigned long long>(transform),
            static_cast<unsigned>(channel),
            group.commands,
            group.x / divisor,
            group.y / divisor);
        std::printf(
            "%s bounds object=%u model=%08x transform=%016llx "
            "rect=(%.2f,%.2f)-(%.2f,%.2f)\n",
            label,
            object,
            model,
            static_cast<unsigned long long>(transform),
            group.minimum_x,
            group.minimum_y,
            group.maximum_x,
            group.maximum_y);
        const auto& sample = *group.sample;
        const float reconstructed_x =
            sample.draw_offset_x + sample.projection_offset_x / 65536.0F +
            sample.projection_plane * sample.view_x / sample.view_z;
        const float reconstructed_y =
            sample.draw_offset_y + sample.projection_offset_y / 65536.0F +
            sample.projection_plane * sample.view_y / sample.view_z;
        std::printf(
            "%s projection model=%08x sample=(%.2f,%.2f) "
            "reconstructed=(%.2f,%.2f) plane=%.2f offset=(%.2f,%.2f) "
            "draw=(%.2f,%.2f) view=(%.2f,%.2f,%.2f)\n",
            label,
            model,
            sample.screen_x,
            sample.screen_y - list.display_y,
            reconstructed_x,
            reconstructed_y - list.display_y,
            sample.projection_plane,
            sample.projection_offset_x,
            sample.projection_offset_y,
            sample.draw_offset_x,
            sample.draw_offset_y,
            sample.view_x,
            sample.view_y,
            sample.view_z);
        const auto& command = *group.command_sample;
        std::printf(
            "%s exact model=%08x valid=%u "
            "R=[%d,%d,%d;%d,%d,%d;%d,%d,%d] T=(%d,%d,%d) "
            "residualRms=%.3f residualMax=%.3f vertices=%u coherent=%u "
            "coherentCommands=%u mixedCommands=%u "
            "vertexResidualRms=%.3f vertexResidualMax=%.3f "
            "vertexTransforms=%u coherentVertexTransforms=%u\n",
            label,
            model,
            command.exact_transform_valid ? 1U : 0U,
            command.transform_rotation[0],
            command.transform_rotation[1],
            command.transform_rotation[2],
            command.transform_rotation[3],
            command.transform_rotation[4],
            command.transform_rotation[5],
            command.transform_rotation[6],
            command.transform_rotation[7],
            command.transform_rotation[8],
            command.transform_translation[0],
            command.transform_translation[1],
            command.transform_translation[2],
            group.exact_residual_vertices == 0 ? 0.0 : std::sqrt(
                group.exact_residual_squared /
                group.exact_residual_vertices),
            group.exact_residual_maximum,
            group.exact_residual_vertices,
            group.coherent_vertices,
            group.coherent_commands,
            group.mixed_commands,
            group.vertex_transform_vertices == 0 ? 0.0 : std::sqrt(
                group.vertex_residual_squared /
                group.vertex_transform_vertices),
            group.vertex_residual_maximum,
            group.vertex_transform_vertices,
            group.coherent_vertex_transforms);
    }
}

void print_vertex_transform_census(
    const char* label,
    const opengt::render::WorldDrawList& list
) {
    struct Group {
        const opengt::render::WorldDrawVertex* sample{};
        std::uint32_t vertices{};
        std::set<std::uint32_t> objects;
        std::set<std::uint32_t> models;
    };
    std::map<std::uint64_t, Group> groups;
    for (const auto& command : list.commands) {
        if (command.object_kind != 1)
            continue;
        for (const auto& vertex : command.vertices) {
            if (!vertex.exact_transform_valid || vertex.transform_id == 0)
                continue;
            auto& group = groups[vertex.transform_id];
            if (group.sample == nullptr)
                group.sample = &vertex;
            ++group.vertices;
            group.objects.insert(command.object_id);
            group.models.insert(command.model_pointer);
        }
    }
    for (const auto& entry : groups) {
        const auto& sample = *entry.second.sample;
        std::printf(
            "%s vertexTransform=%016llx vertices=%u objects=%zu models=%zu "
            "R=[%d,%d,%d;%d,%d,%d;%d,%d,%d] T=(%d,%d,%d)\n",
            label,
            static_cast<unsigned long long>(entry.first),
            entry.second.vertices,
            entry.second.objects.size(),
            entry.second.models.size(),
            sample.transform_rotation[0],
            sample.transform_rotation[1],
            sample.transform_rotation[2],
            sample.transform_rotation[3],
            sample.transform_rotation[4],
            sample.transform_rotation[5],
            sample.transform_rotation[6],
            sample.transform_rotation[7],
            sample.transform_rotation[8],
            sample.transform_translation[0],
            sample.transform_translation[1],
            sample.transform_translation[2]);
    }
}

void print_suspicious_vehicle_triangles(
    const char* label,
    const opengt::render::WorldDrawList& list
) {
    for (std::size_t command_index = 0;
         command_index < list.commands.size();
         ++command_index) {
        const auto& command = list.commands[command_index];
        if (command.object_kind != 2)
            continue;
        float minimum_x = (std::numeric_limits<float>::max)();
        float minimum_y = (std::numeric_limits<float>::max)();
        float maximum_x = (std::numeric_limits<float>::lowest)();
        float maximum_y = (std::numeric_limits<float>::lowest)();
        for (const auto& vertex : command.vertices) {
            minimum_x = std::min(minimum_x, vertex.screen_x);
            minimum_y = std::min(minimum_y, vertex.screen_y - list.display_y);
            maximum_x = std::max(maximum_x, vertex.screen_x);
            maximum_y = std::max(maximum_y, vertex.screen_y - list.display_y);
        }
        const float width = maximum_x - minimum_x;
        const float height = maximum_y - minimum_y;
        if (
            minimum_y < 130.0F || minimum_y > 160.0F ||
            minimum_x < 140.0F || minimum_x > 225.0F ||
            width < 3.0F || height < 2.0F
        )
            continue;
        std::printf(
            "%s suspicious command=%zu object=%u model=%08x "
            "transform=%016llx rect=(%.2f,%.2f)-(%.2f,%.2f) "
            "size=(%.2f,%.2f)\n",
            label,
            command_index,
            command.object_id,
            command.model_pointer,
            static_cast<unsigned long long>(command.transform_id),
            minimum_x,
            minimum_y,
            maximum_x,
            maximum_y,
            width,
            height);
        for (int vertex = 0; vertex < 3; ++vertex) {
            const auto& value = command.vertices[vertex];
            std::printf(
                "  v%d model=(%d,%d,%d) screen=(%.2f,%.2f) "
                "view=(%.2f,%.2f,%.2f) uv=(%.2f,%.2f)\n",
                vertex,
                value.model_x,
                value.model_y,
                value.model_z,
                value.screen_x,
                value.screen_y - list.display_y,
                value.view_x,
                value.view_y,
                value.view_z,
                value.u,
                value.v);
        }
    }
}

void print_exploded_midpoint_triangles(
    const opengt::render::WorldDrawList& previous,
    const opengt::render::WorldDrawList& midpoint
) {
    const float width_limit =
        static_cast<float>(midpoint.display_width) * 1.25F;
    const float height_limit =
        static_cast<float>(midpoint.display_height) * 1.25F;
    std::size_t printed = 0;
    for (std::size_t command_index = 0;
         command_index < midpoint.commands.size();
         ++command_index) {
        const auto& command = midpoint.commands[command_index];
        if (command.object_kind == 0)
            continue;
        float minimum_x = (std::numeric_limits<float>::max)();
        float minimum_y = (std::numeric_limits<float>::max)();
        float maximum_x = (std::numeric_limits<float>::lowest)();
        float maximum_y = (std::numeric_limits<float>::lowest)();
        for (const auto& vertex : command.vertices) {
            minimum_x = std::min(minimum_x, vertex.screen_x);
            minimum_y = std::min(minimum_y, vertex.screen_y);
            maximum_x = std::max(maximum_x, vertex.screen_x);
            maximum_y = std::max(maximum_y, vertex.screen_y);
        }
        const float width = maximum_x - minimum_x;
        const float height = maximum_y - minimum_y;
        if (width <= width_limit && height <= height_limit)
            continue;
        const auto& prior = previous.commands[command_index];
        std::printf(
            "exploded command=%zu kind=%u object=%u model=%08x "
            "transform=%016llx channel=%u midpointRect=(%.2f,%.2f)-"
            "(%.2f,%.2f) size=(%.2f,%.2f)\n",
            command_index,
            command.object_kind,
            command.object_id,
            command.model_pointer,
            static_cast<unsigned long long>(command.transform_id),
            static_cast<unsigned>(command.channel),
            minimum_x,
            minimum_y - midpoint.display_y,
            maximum_x,
            maximum_y - midpoint.display_y,
            width,
            height);
        for (int vertex = 0; vertex < 3; ++vertex) {
            const auto& before = prior.vertices[vertex];
            const auto& value = command.vertices[vertex];
            std::printf(
                "  v%d model=(%d,%d,%d) previousScreen=(%.2f,%.2f) "
                "midpointScreen=(%.2f,%.2f) previousViewZ=%.2f "
                "midpointViewZ=%.2f\n",
                vertex,
                value.model_x,
                value.model_y,
                value.model_z,
                before.screen_x,
                before.screen_y - previous.display_y,
                value.screen_x,
                value.screen_y - midpoint.display_y,
                before.view_z,
                value.view_z);
        }
        if (++printed == 100) {
            std::printf("exploded output truncated after 100 commands\n");
            break;
        }
    }
    std::printf("explodedCommands=%zu\n", printed);
}

float edge(
    const opengt::render::WorldDrawVertex& a,
    const opengt::render::WorldDrawVertex& b,
    float x,
    float y
) {
    return
        (b.screen_x - a.screen_x) * (y - a.screen_y) -
        (b.screen_y - a.screen_y) * (x - a.screen_x);
}

bool contains_point(
    const opengt::render::WorldDrawCommand& command,
    float x,
    float y
) {
    const float e0 = edge(command.vertices[0], command.vertices[1], x, y);
    const float e1 = edge(command.vertices[1], command.vertices[2], x, y);
    const float e2 = edge(command.vertices[2], command.vertices[0], x, y);
    return
        (e0 >= 0.0F && e1 >= 0.0F && e2 >= 0.0F) ||
        (e0 <= 0.0F && e1 <= 0.0F && e2 <= 0.0F);
}

void print_hit_commands(
    const opengt::render::WorldDrawList& list,
    float x,
    float y
) {
    std::size_t hits = 0;
    for (std::size_t command_index = 0;
         command_index < list.commands.size();
         ++command_index) {
        const auto& command = list.commands[command_index];
        float minimum_x = (std::numeric_limits<float>::max)();
        float minimum_y = (std::numeric_limits<float>::max)();
        float maximum_x = (std::numeric_limits<float>::lowest)();
        float maximum_y = (std::numeric_limits<float>::lowest)();
        for (const auto& vertex : command.vertices) {
            minimum_x = std::min(minimum_x, vertex.screen_x);
            minimum_y = std::min(minimum_y, vertex.screen_y);
            maximum_x = std::max(maximum_x, vertex.screen_x);
            maximum_y = std::max(maximum_y, vertex.screen_y);
        }
        if (
            x < minimum_x - 1.0F || x > maximum_x + 1.0F ||
            y < minimum_y - 1.0F || y > maximum_y + 1.0F ||
            !contains_point(command, x, y)
        ) {
            continue;
        }
        const auto& material = list.materials[command.material_index];
        const bool screen_space =
            (material.primitive_flags &
                opengt::render::world_primitive_screen_space_flag) != 0;
        std::printf(
            "hit command=%zu kind=%u object=%u model=%08x source=%u "
            "transform=%016llx exact=%u ot=%d channel=%u material=%u "
            "flags=%08x tpage=%04x clut=%04x env=%08x screenSpace=%u "
            "rect=(%.3f,%.3f)-(%.3f,%.3f)\n",
            command_index,
            command.object_kind,
            command.object_id,
            command.model_pointer,
            command.source_command_index,
            static_cast<unsigned long long>(command.transform_id),
            command.exact_transform_valid ? 1U : 0U,
            command.ordering_table_index,
            static_cast<unsigned>(command.channel),
            command.material_index,
            material.primitive_flags,
            material.texture_page,
            material.clut,
            material.environment_flags,
            screen_space ? 1U : 0U,
            minimum_x,
            minimum_y - list.display_y,
            maximum_x,
            maximum_y - list.display_y);
        for (int vertex = 0; vertex < 3; ++vertex) {
            const auto& value = command.vertices[vertex];
            std::printf(
                "  v%d screen=(%.3f,%.3f) view=(%.3f,%.3f,%.3f) "
                "model=(%d,%d,%d) uv=(%.3f,%.3f) src=%08x prov=%04x\n",
                vertex,
                value.screen_x,
                value.screen_y - list.display_y,
                value.view_x,
                value.view_y,
                value.view_z,
                value.model_x,
                value.model_y,
                value.model_z,
                value.u,
                value.v,
                value.source_vertex_identity,
                value.provenance_flags);
        }
        ++hits;
    }
    std::printf(
        "hitPoint=(%.3f,%.3f) hits=%zu\n",
        x,
        y - list.display_y,
        hits);
}

} // namespace

int main(int argc, char** argv) {
    if (argc < 3 || argc > 13) {
        std::fprintf(
            stderr,
            "usage: opengt_world_interpolation_inspect "
            "<previous.ogtwcap> <current.ogtwcap> "
            "[midpoint.png] [--no-depth] [--model hex] "
            "[--command-range first last] [--summary-only] "
            "[--repeat count] [--cached-index]\n");
        return 2;
    }
    const char* output_path = nullptr;
    bool depth = true;
    bool summary_only = false;
    bool cached_index = false;
    bool topology_midpoint = false;
    std::uint32_t repeat_count = 1;
    std::uint32_t isolated_model = 0;
    std::size_t first_command = 0;
    std::size_t last_command = (std::numeric_limits<std::size_t>::max)();
    for (int argument = 3; argument < argc; ++argument) {
        if (std::strcmp(argv[argument], "--no-depth") == 0)
            depth = false;
        else if (std::strcmp(argv[argument], "--summary-only") == 0)
            summary_only = true;
        else if (std::strcmp(argv[argument], "--cached-index") == 0)
            cached_index = true;
        else if (std::strcmp(argv[argument], "--topology-midpoint") == 0)
            topology_midpoint = true;
        else if (
            std::strcmp(argv[argument], "--repeat") == 0 &&
            argument + 1 < argc
        ) {
            repeat_count = static_cast<std::uint32_t>(
                std::strtoul(argv[++argument], nullptr, 10));
            if (repeat_count == 0 || repeat_count > 10000) {
                std::fprintf(stderr, "invalid repeat count\n");
                return 2;
            }
        }
        else if (
            std::strcmp(argv[argument], "--model") == 0 &&
            argument + 1 < argc
        ) {
            isolated_model = static_cast<std::uint32_t>(
                std::strtoul(argv[++argument], nullptr, 16));
        }
        else if (
            std::strcmp(argv[argument], "--command-range") == 0 &&
            argument + 2 < argc
        ) {
            first_command = static_cast<std::size_t>(
                std::strtoull(argv[++argument], nullptr, 10));
            last_command = static_cast<std::size_t>(
                std::strtoull(argv[++argument], nullptr, 10));
            if (last_command < first_command) {
                std::fprintf(stderr, "invalid command range\n");
                return 2;
            }
        }
        else if (output_path == nullptr)
            output_path = argv[argument];
        else {
            std::fprintf(stderr, "unexpected argument: %s\n", argv[argument]);
            return 2;
        }
    }
    Frame previous;
    Frame current;
    if (!load(argv[1], &previous) || !load(argv[2], &current)) {
        std::fprintf(stderr, "capture build/topology failed\n");
        return 1;
    }
    opengt::render::WorldDrawList midpoint{};
    opengt::render::WorldInterpolationStats stats{};
    opengt::render::WorldInterpolationCache previous_cache;
    opengt::render::WorldInterpolationCache current_cache;
    auto result = opengt::render::WorldInterpolationResult::success;
    for (std::uint32_t iteration = 0;
         iteration < repeat_count;
         ++iteration) {
        if (cached_index) {
            // Production reuses only the previous index. The current frame is
            // new on every pair and must still be indexed once.
            current_cache.reset();
            result = opengt::render::interpolate_world_draw_lists_cached(
                previous.draw_list,
                current.draw_list,
                &previous_cache,
                &current_cache,
                0.5F,
                &midpoint,
                &stats);
        } else {
            result = opengt::render::interpolate_world_draw_lists(
                previous.draw_list,
                current.draw_list,
                0.5F,
                &midpoint,
                &stats);
        }
        if (result != opengt::render::WorldInterpolationResult::success)
            break;
    }
    if (result != opengt::render::WorldInterpolationResult::success) {
        std::fprintf(
            stderr,
            "interpolation failed: %s previousViewport=(%d,%d %dx%d) "
            "currentViewport=(%d,%d %dx%d)\n",
            opengt::render::world_interpolation_result_name(result),
            previous.draw_list.display_x,
            previous.draw_list.display_y,
            previous.draw_list.display_width,
            previous.draw_list.display_height,
            current.draw_list.display_x,
            current.draw_list.display_y,
            current.draw_list.display_width,
            current.draw_list.display_height);
        return 1;
    }
    const double match_rate = stats.previous_commands == 0
        ? 0.0
        : stats.matched_commands * 100.0 /
            stats.previous_commands;
    std::printf(
        "previousFrame=%llu currentFrame=%llu previous=%u current=%u "
        "worldEligible=%u matched=%u/%u matchRate=%.3f%% "
        "track=%u vehicle=%u "
        "heldScreen=%u heldUnmatched=%u groups=%u/%u matchedGroups=%u "
        "exactRigidGroups=%u incoherentExactGroups=%u "
        "heldIncoherentVehicle=%u heldTrackVisibility=%u "
        "heldUnsafeTrack=%u vertexExactTrackGroups=%u "
        "unavailableVertexExactTrackGroups=%u "
        "ambiguousTrackVertexMappings=%u "
        "missingTrackVertexCandidates=%u "
        "firstMissingTrackTransform=%016llx "
        "firstMissingTrackSource=%08x "
        "unsafeNonfinite=%u unsafeNearDepth=%u "
        "unsafeShift=%u unsafeSpan=%u "
        "firstUnsafeCommand=%u previousSpan=%.3f,%.3f "
        "midpointSpan=%.3f,%.3f maxShift=%.3f minDepth=%.3f "
        "cache=%u/%u repeats=%u\n",
        static_cast<unsigned long long>(previous.header.frame_index),
        static_cast<unsigned long long>(current.header.frame_index),
        stats.previous_commands,
        stats.current_commands,
        stats.eligible_world_commands,
        stats.matched_commands,
        stats.previous_commands,
        match_rate,
        stats.matched_track_commands,
        stats.matched_vehicle_commands,
        stats.held_screen_commands,
        stats.held_unmatched_commands,
        stats.previous_transform_groups,
        stats.current_transform_groups,
        stats.matched_transform_groups,
        stats.exact_rigid_transform_groups,
        stats.incoherent_exact_transform_groups,
        stats.held_incoherent_vehicle_commands,
        stats.held_track_visibility_commands,
        stats.held_unsafe_track_commands,
        stats.vertex_exact_track_groups,
        stats.unavailable_vertex_exact_track_groups,
        stats.ambiguous_track_vertex_mappings,
        stats.missing_track_vertex_candidates,
        static_cast<unsigned long long>(
            stats.first_missing_track_transform_id),
        stats.first_missing_track_source_identity,
        stats.held_unsafe_track_nonfinite_commands,
        stats.held_unsafe_track_near_depth_commands,
        stats.held_unsafe_track_shift_commands,
        stats.held_unsafe_track_span_commands,
        stats.first_unsafe_track_command,
        stats.first_unsafe_track_previous_span_x,
        stats.first_unsafe_track_previous_span_y,
        stats.first_unsafe_track_midpoint_span_x,
        stats.first_unsafe_track_midpoint_span_y,
        stats.first_unsafe_track_maximum_shift,
        stats.first_unsafe_track_minimum_depth,
        stats.previous_group_cache_hit,
        stats.current_group_cache_hit,
        repeat_count);
    if (topology_midpoint) {
        opengt::render::WorldTopologyStats midpoint_topology{};
        if (
            opengt::render::apply_world_topology(
                &midpoint,
                opengt::render::WorldTopologyOptions{true, true, true},
                &midpoint_topology) !=
            opengt::render::WorldTopologyResult::success
        ) {
            std::fprintf(stderr, "midpoint topology failed\n");
            return 1;
        }
        std::printf(
            "midpointTopology input=%u output=%u eligible=%u "
            "boundary=%u adjusted=%u projection=%u/%u "
            "seam=%u/%u raster=%u/%u projectedTJ=%u/%u "
            "tJunctions=%u coplanar=%u reorders=%u\n",
            midpoint_topology.input_commands,
            midpoint_topology.output_commands,
            midpoint_topology.eligible_track_commands,
            midpoint_topology.authored_boundary_groups,
            midpoint_topology.adjusted_vertex_instances,
            midpoint_topology.authored_projection_groups,
            midpoint_topology.adjusted_projection_instances,
            midpoint_topology.projected_seam_groups,
            midpoint_topology.adjusted_seam_instances,
            midpoint_topology.authored_raster_groups,
            midpoint_topology.adjusted_authored_raster_instances,
            midpoint_topology.projected_t_junctions,
            midpoint_topology.adjusted_projected_t_junction_instances,
            midpoint_topology.exact_t_junctions +
                midpoint_topology.projected_t_junctions,
            midpoint_topology.coplanar_overlap_pairs,
            midpoint_topology.ownership_reorders);
    }
    if (const char* point = std::getenv("OPENGT_INSPECT_POINT")) {
        std::string value(point);
        const auto comma = value.find(',');
        if (comma != std::string::npos) {
            const float x = std::strtof(value.c_str(), nullptr);
            const float y = std::strtof(value.c_str() + comma + 1, nullptr) +
                static_cast<float>(midpoint.display_y);
            print_hit_commands(midpoint, x, y);
        }
    }
    if (output_path == nullptr && !summary_only) {
        print_transform_groups(
            "previous-captured", previous.captured_draw_list, 1, "track");
        print_transform_groups(
            "current-captured", current.captured_draw_list, 1, "track");
        print_vertex_transform_census(
            "previous-captured", previous.captured_draw_list);
        print_vertex_transform_census(
            "current-captured", current.captured_draw_list);
        print_transform_groups("previous", previous.draw_list, 1, "track");
        print_transform_groups("current", current.draw_list, 1, "track");
        print_transform_groups("midpoint", midpoint, 1, "track");
        print_transform_groups("previous", previous.draw_list, 2, "vehicle");
        print_transform_groups("current", current.draw_list, 2, "vehicle");
        print_transform_groups("midpoint", midpoint, 2, "vehicle");
        print_suspicious_vehicle_triangles("previous", previous.draw_list);
        print_suspicious_vehicle_triangles("current", current.draw_list);
        print_suspicious_vehicle_triangles("midpoint", midpoint);
        print_exploded_midpoint_triangles(previous.draw_list, midpoint);
    }

    if (output_path != nullptr) {
        if (
            isolated_model != 0 ||
            last_command != (std::numeric_limits<std::size_t>::max)()
        ) {
            std::size_t command_index = 0;
            midpoint.commands.erase(
                std::remove_if(
                    midpoint.commands.begin(),
                    midpoint.commands.end(),
                    [&](const auto& command) {
                        const bool remove =
                            (isolated_model != 0 &&
                                command.model_pointer != isolated_model) ||
                            command_index < first_command ||
                            command_index > last_command;
                        ++command_index;
                        return remove;
                    }),
                midpoint.commands.end());
        }
        const std::uint32_t scale = 4;
        const std::uint32_t width =
            static_cast<std::uint32_t>(midpoint.display_width) * scale;
        const std::uint32_t height =
            static_cast<std::uint32_t>(midpoint.display_height) * scale;
        std::vector<std::uint8_t> output(
            static_cast<std::size_t>(width) * height * 4U);
        opengt::render::WorldGpuRenderStats render_stats{};
        const auto render_result = opengt::render::render_world_d3d11(
            midpoint,
            previous.vram.data(),
            previous.vram.size(),
            output.data(),
            output.size(),
            opengt::render::WorldGpuRenderOptions{
                false, depth, false, true, true, scale, 0xFF402820U},
            &render_stats);
        if (
            render_result !=
                opengt::render::WorldGpuRenderResult::success ||
            !opengt::render::write_rgba_png(
                output_path, output.data(), width, height)
        ) {
            std::fprintf(stderr, "midpoint render/write failed\n");
            return 1;
        }
        std::printf(
            "output=%s drawCalls=%u transparent=%u\n",
            output_path,
            render_stats.draw_calls,
            render_stats.transparent_draw_calls);
    }
    return 0;
}
