#include "opengt/world_capture.hpp"
#include "opengt/world_draw_list.hpp"
#include "opengt/world_topology.hpp"

#include <algorithm>
#include <cmath>
#include <cstdio>
#include <cstring>
#include <limits>
#include <set>
#include <tuple>
#include <vector>

namespace {

struct DepthAuditSurface {
    double view_z{std::numeric_limits<double>::infinity()};
    std::uint32_t object_id{};
    std::uint32_t model_pointer{};
    std::uint32_t material_index{};
    std::uint32_t command_index{};
};

struct DepthAuditPixel {
    DepthAuditSurface nearest{};
    DepthAuditSurface second{};
};

std::uint32_t quantize_standard_d24(double view_z) {
    constexpr double near_plane = 16.0;
    constexpr double far_plane = 1048576.0;
    constexpr double maximum_d24 = 16777215.0;
    const double depth =
        far_plane / (far_plane - near_plane) -
        near_plane * far_plane /
            ((far_plane - near_plane) * view_z);
    return static_cast<std::uint32_t>(std::llround(
        std::clamp(depth, 0.0, 1.0) * maximum_d24));
}

float reversed_infinite_d32(double view_z) {
    constexpr double near_plane = 16.0;
    return static_cast<float>(near_plane / view_z);
}

void insert_depth_surface(
    DepthAuditPixel* pixel,
    const DepthAuditSurface& candidate
) {
    constexpr double distinct_epsilon = 1.0e-4;
    if (candidate.view_z < pixel->nearest.view_z - distinct_epsilon) {
        pixel->second = pixel->nearest;
        pixel->nearest = candidate;
    } else if (
        std::abs(candidate.view_z - pixel->nearest.view_z) >
            distinct_epsilon &&
        candidate.view_z < pixel->second.view_z
    ) {
        pixel->second = candidate;
    }
}

void audit_depth_precision(const opengt::render::WorldDrawList& draw_list) {
    constexpr double near_plane = 16.0;
    constexpr std::uint32_t semi_transparent_flag = 1U << 1;
    const int target_height = draw_list.display_height;
    const int target_width =
        (target_height * 16 + 9 - 1) / 9;
    const double target_x0 =
        draw_list.display_x + draw_list.display_width * 0.5 -
        target_width * 0.5;
    const double target_y0 = draw_list.display_y;
    std::vector<DepthAuditPixel> pixels(
        static_cast<std::size_t>(target_width) * target_height);
    std::uint64_t rasterized_commands = 0;
    std::uint64_t rasterized_samples = 0;

    const auto edge = [](
        double ax, double ay,
        double bx, double by,
        double px, double py
    ) {
        return (px - ax) * (by - ay) -
            (py - ay) * (bx - ax);
    };

    for (std::size_t command_index = 0;
         command_index < draw_list.commands.size();
         ++command_index) {
        const auto& command = draw_list.commands[command_index];
        if (
            command.object_kind != 1U ||
            command.channel != opengt::render::WorldViewChannel::main_view ||
            command.material_index >= draw_list.materials.size()
        ) {
            continue;
        }
        const auto& material = draw_list.materials[command.material_index];
        if (
            (material.primitive_flags &
                (opengt::render::world_primitive_screen_space_flag |
                    semi_transparent_flag)) != 0
        ) {
            continue;
        }
        bool entirely_in_front = true;
        double x[3]{};
        double y[3]{};
        double inverse_z[3]{};
        for (int vertex = 0; vertex < 3; ++vertex) {
            const auto& source = command.vertices[vertex];
            entirely_in_front =
                entirely_in_front && source.view_z >= near_plane;
            x[vertex] = source.screen_x - target_x0;
            y[vertex] = source.screen_y - target_y0;
            inverse_z[vertex] = 1.0 / source.view_z;
        }
        if (!entirely_in_front)
            continue;
        const double signed_area = edge(
            x[0], y[0], x[1], y[1], x[2], y[2]);
        if (std::abs(signed_area) <= 1.0e-12)
            continue;
        const int minimum_x = std::clamp(
            static_cast<int>(std::floor(
                std::min({x[0], x[1], x[2]}))),
            0,
            target_width - 1);
        const int maximum_x = std::clamp(
            static_cast<int>(std::floor(
                std::max({x[0], x[1], x[2]}))),
            0,
            target_width - 1);
        const int minimum_y = std::clamp(
            static_cast<int>(std::floor(
                std::min({y[0], y[1], y[2]}))),
            0,
            target_height - 1);
        const int maximum_y = std::clamp(
            static_cast<int>(std::floor(
                std::max({y[0], y[1], y[2]}))),
            0,
            target_height - 1);
        if (minimum_x > maximum_x || minimum_y > maximum_y)
            continue;
        ++rasterized_commands;
        for (int pixel_y = minimum_y; pixel_y <= maximum_y; ++pixel_y) {
            for (int pixel_x = minimum_x; pixel_x <= maximum_x; ++pixel_x) {
                const double sample_x = pixel_x + 0.5;
                const double sample_y = pixel_y + 0.5;
                const double w0 = edge(
                    x[1], y[1], x[2], y[2], sample_x, sample_y);
                const double w1 = edge(
                    x[2], y[2], x[0], y[0], sample_x, sample_y);
                const double w2 = edge(
                    x[0], y[0], x[1], y[1], sample_x, sample_y);
                // Exclude exact shared edges from this precision audit. D3D's
                // top-left fill rule gives them to only one triangle, while a
                // symmetric software test would otherwise report false
                // overlaps between adjacent road faces.
                constexpr double boundary_epsilon = 1.0e-9;
                const bool positive =
                    w0 > boundary_epsilon &&
                    w1 > boundary_epsilon &&
                    w2 > boundary_epsilon;
                const bool negative =
                    w0 < -boundary_epsilon &&
                    w1 < -boundary_epsilon &&
                    w2 < -boundary_epsilon;
                if (!positive && !negative)
                    continue;
                const double reciprocal_depth =
                    (w0 * inverse_z[0] +
                        w1 * inverse_z[1] +
                        w2 * inverse_z[2]) /
                    signed_area;
                if (!(reciprocal_depth > 0.0) ||
                    !std::isfinite(reciprocal_depth))
                    continue;
                const double view_z = 1.0 / reciprocal_depth;
                if (view_z < near_plane || !std::isfinite(view_z))
                    continue;
                insert_depth_surface(
                    &pixels[static_cast<std::size_t>(pixel_y) *
                        target_width + pixel_x],
                    DepthAuditSurface{
                        view_z,
                        command.object_id,
                        command.model_pointer,
                        command.material_index,
                        static_cast<std::uint32_t>(command_index)});
                ++rasterized_samples;
            }
        }
    }

    std::uint64_t covered_pixels = 0;
    std::uint64_t overlap_pixels = 0;
    std::uint64_t standard_d24_collisions = 0;
    std::uint64_t reversed_d32_collisions = 0;
    std::uint64_t horizon_d24_collisions = 0;
    double maximum_collision_separation = 0.0;
    int collision_x0 = target_width;
    int collision_y0 = target_height;
    int collision_x1 = -1;
    int collision_y1 = -1;
    DepthAuditSurface maximum_nearest{};
    DepthAuditSurface maximum_second{};
    int maximum_x = -1;
    int maximum_y = -1;
    for (int y = 0; y < target_height; ++y) {
        for (int x = 0; x < target_width; ++x) {
            const auto& pixel = pixels[static_cast<std::size_t>(y) *
                target_width + x];
            if (std::isfinite(pixel.nearest.view_z))
                ++covered_pixels;
            if (!std::isfinite(pixel.second.view_z))
                continue;
            ++overlap_pixels;
            const bool standard_collision =
                quantize_standard_d24(pixel.nearest.view_z) ==
                quantize_standard_d24(pixel.second.view_z);
            const bool reversed_collision =
                reversed_infinite_d32(pixel.nearest.view_z) ==
                reversed_infinite_d32(pixel.second.view_z);
            reversed_d32_collisions += reversed_collision ? 1U : 0U;
            if (!standard_collision)
                continue;
            ++standard_d24_collisions;
            horizon_d24_collisions += y < target_height / 2 ? 1U : 0U;
            collision_x0 = std::min(collision_x0, x);
            collision_y0 = std::min(collision_y0, y);
            collision_x1 = std::max(collision_x1, x);
            collision_y1 = std::max(collision_y1, y);
            const double separation =
                pixel.second.view_z - pixel.nearest.view_z;
            if (separation > maximum_collision_separation) {
                maximum_collision_separation = separation;
                maximum_nearest = pixel.nearest;
                maximum_second = pixel.second;
                maximum_x = x;
                maximum_y = y;
            }
        }
    }
    std::printf(
        "depthAudit=16:9@%dx%d opaqueTrackCommands=%llu "
        "rasterSamples=%llu coveredPixels=%llu overlapPixels=%llu "
        "standardD24Collisions=%llu horizonD24Collisions=%llu "
        "reversedInfiniteD32Collisions=%llu collisionBounds=%d,%d..%d,%d "
        "maximumCollisionSeparation=%.9f maximumCollisionPixel=%d,%d "
        "nearest=z%.9f/object%u/model%08x/material%u/command%u "
        "second=z%.9f/object%u/model%08x/material%u/command%u\n",
        target_width,
        target_height,
        static_cast<unsigned long long>(rasterized_commands),
        static_cast<unsigned long long>(rasterized_samples),
        static_cast<unsigned long long>(covered_pixels),
        static_cast<unsigned long long>(overlap_pixels),
        static_cast<unsigned long long>(standard_d24_collisions),
        static_cast<unsigned long long>(horizon_d24_collisions),
        static_cast<unsigned long long>(reversed_d32_collisions),
        collision_x0,
        collision_y0,
        collision_x1,
        collision_y1,
        maximum_collision_separation,
        maximum_x,
        maximum_y,
        maximum_nearest.view_z,
        maximum_nearest.object_id,
        maximum_nearest.model_pointer,
        maximum_nearest.material_index,
        maximum_nearest.command_index,
        maximum_second.view_z,
        maximum_second.object_id,
        maximum_second.model_pointer,
        maximum_second.material_index,
        maximum_second.command_index);
}

} // namespace

int main(int argc, char** argv) {
    if (argc < 2 || argc > 8) {
        std::fprintf(
            stderr,
            "usage: opengt_world_capture_inspect <capture.ogtwcap> "
            "[output.obj] [topology.csv] [--include-secondary] "
            "[--include-screen-space] [--continuous-projection] "
            "[--depth-audit]\n");
        return 2;
    }
    bool include_secondary = false;
    bool include_screen_space = false;
    bool continuous_projection = false;
    bool depth_audit = false;
    for (int index = 4; index < argc; ++index) {
        if (std::strcmp(argv[index], "--include-secondary") == 0)
            include_secondary = true;
        else if (std::strcmp(argv[index], "--include-screen-space") == 0)
            include_screen_space = true;
        else if (std::strcmp(argv[index], "--continuous-projection") == 0)
            continuous_projection = true;
        else if (std::strcmp(argv[index], "--depth-audit") == 0)
            depth_audit = true;
        else {
            std::fprintf(stderr, "unknown option: %s\n", argv[index]);
            return 2;
        }
    }
    opengt::render::WorldCaptureHeader header{};
    auto result = opengt::render::read_world_capture_header(argv[1], &header);
    if (result != opengt::render::WorldCaptureReadResult::success) {
        std::fprintf(
            stderr,
            "header failed: %s\n",
            opengt::render::world_capture_read_result_name(result));
        return 1;
    }
    std::vector<opengt::render::WorldCaptureTriangle> triangles(
        header.triangle_count);
    std::vector<std::uint16_t> vram(1024U * 512U);
    result = opengt::render::load_world_capture(
        argv[1], &header,
        triangles.data(), triangles.size(),
        vram.data(), vram.size());
    if (result != opengt::render::WorldCaptureReadResult::success) {
        std::fprintf(
            stderr,
            "load failed: %s\n",
            opengt::render::world_capture_read_result_name(result));
        return 1;
    }

    std::set<std::pair<std::uint32_t, std::uint32_t>> objects;
    std::set<std::tuple<std::uint32_t, std::uint32_t, std::uint32_t>> models;
    std::set<std::uint32_t> track_objects;
    std::set<std::uint32_t> vehicle_objects;
    std::set<std::tuple<std::uint32_t, std::uint16_t, std::uint16_t>> materials;
    std::set<std::uint64_t> transforms;
    std::uint64_t valid_vertices = 0;
    std::uint64_t source_vertices = 0;
    std::uint64_t track_source_vertices = 0;
    std::uint64_t track_missing_source_vertices = 0;
    std::set<std::pair<std::uint32_t, std::uint32_t>>
        unique_track_sources;
    std::uint64_t track_triangles = 0;
    std::uint64_t vehicle_triangles = 0;
    float minimum[3] = {
        std::numeric_limits<float>::infinity(),
        std::numeric_limits<float>::infinity(),
        std::numeric_limits<float>::infinity()};
    float maximum[3] = {
        -std::numeric_limits<float>::infinity(),
        -std::numeric_limits<float>::infinity(),
        -std::numeric_limits<float>::infinity()};
    double squared_error = 0.0;
    double screen_squared_error = 0.0;
    double screen_max_error = 0.0;
    std::uint64_t screen_samples = 0;
    std::uint64_t screen_error_over_one = 0;
    std::uint64_t screen_error_over_two = 0;
    std::uint64_t screen_error_over_ten = 0;
    std::uint64_t track_screen_error_over_two = 0;
    std::uint64_t vehicle_screen_error_over_two = 0;
    std::uint64_t main_projection_triangles = 0;
    std::uint64_t secondary_projection_triangles = 0;
    std::FILE* obj = argc >= 3 ? std::fopen(argv[2], "wb") : nullptr;
    if (argc >= 3 && obj == nullptr) {
        std::fprintf(stderr, "cannot write %s\n", argv[2]);
        return 1;
    }
    std::uint64_t obj_vertex = 1;
    for (const auto& triangle : triangles) {
        bool main_projection =
            (triangle.primitive_flags &
                opengt::render::world_primitive_secondary_view_flag) == 0 &&
            triangle.draw_offset_x == header.draw_offset_x &&
            triangle.draw_offset_y == header.draw_offset_y;
        for (const auto& vertex : triangle.vertices) {
            main_projection =
                main_projection &&
                vertex.projection_offset_x ==
                    header.projection_offset_x &&
                vertex.projection_offset_y ==
                    header.projection_offset_y;
        }
        if (main_projection)
            ++main_projection_triangles;
        else
            ++secondary_projection_triangles;
        objects.emplace(triangle.object_kind, triangle.object_id);
        models.emplace(
            triangle.object_kind,
            triangle.object_id,
            triangle.model_pointer);
        materials.emplace(
            triangle.primitive_flags,
            triangle.texture_page,
            triangle.clut);
        transforms.insert(triangle.transform_id);
        if (triangle.object_kind == 1) {
            ++track_triangles;
            track_objects.insert(triangle.object_id);
        }
        if (triangle.object_kind == 2) {
            ++vehicle_triangles;
            vehicle_objects.insert(triangle.object_id);
        }
        bool complete = true;
        for (const auto& vertex : triangle.vertices) {
            if (vertex.source_vertex_identity != 0) {
                ++source_vertices;
                if (triangle.object_kind == 1) {
                    ++track_source_vertices;
                    unique_track_sources.emplace(
                        triangle.model_pointer,
                        vertex.source_vertex_identity);
                }
            } else if (triangle.object_kind == 1) {
                ++track_missing_source_vertices;
            }
            if (!vertex.world_valid) {
                complete = false;
                continue;
            }
            ++valid_vertices;
            const float world[3] = {
                vertex.world_x, vertex.world_y, vertex.world_z};
            for (int axis = 0; axis < 3; ++axis) {
                minimum[axis] = std::min(minimum[axis], world[axis]);
                maximum[axis] = std::max(maximum[axis], world[axis]);
            }
            const std::int32_t view[3] = {
                vertex.view_x, vertex.view_y, vertex.view_z};
            for (int row = 0; row < 3; ++row) {
                double projected = header.camera_translation[row];
                for (int column = 0; column < 3; ++column)
                    projected +=
                        header.camera_rotation[row * 3 + column] *
                        world[column] / 4096.0;
                const double difference = projected - view[row];
                squared_error += difference * difference;
            }
            if (vertex.projection_plane != 0 && vertex.view_z > 0) {
                double projected_x = 0.0;
                double projected_y = 0.0;
                if (continuous_projection) {
                    const auto projected =
                        opengt::render::project_continuous_vertex(
                            vertex,
                            triangle.draw_offset_x,
                            triangle.draw_offset_y);
                    projected_x = projected.x;
                    projected_y = projected.y;
                } else {
                    const auto projected =
                        opengt::render::project_ps1_vertex(
                            vertex,
                            triangle.draw_offset_x,
                            triangle.draw_offset_y);
                    projected_x = projected.x;
                    projected_y = projected.y;
                }
                const double difference_x =
                    projected_x - vertex.screen_x;
                const double difference_y =
                    projected_y - vertex.screen_y;
                const double error =
                    std::sqrt(
                        difference_x * difference_x +
                        difference_y * difference_y);
                screen_squared_error +=
                    difference_x * difference_x +
                    difference_y * difference_y;
                screen_max_error = std::max(screen_max_error, error);
                if (error > 1.0) ++screen_error_over_one;
                if (error > 2.0) {
                    ++screen_error_over_two;
                    if (triangle.object_kind == 1)
                        ++track_screen_error_over_two;
                    if (triangle.object_kind == 2)
                        ++vehicle_screen_error_over_two;
                }
                if (error > 10.0) ++screen_error_over_ten;
                ++screen_samples;
            }
        }
        if (obj != nullptr && complete) {
            std::fprintf(
                obj,
                "o kind_%u_id_%u_model_%08x\n",
                triangle.object_kind,
                triangle.object_id,
                triangle.model_pointer);
            for (const auto& vertex : triangle.vertices)
                std::fprintf(
                    obj,
                    "v %.6f %.6f %.6f %.6f %.6f %.6f\n",
                    vertex.world_x, vertex.world_y, vertex.world_z,
                    vertex.r / 255.0, vertex.g / 255.0, vertex.b / 255.0);
            std::fprintf(
                obj,
                "f %llu %llu %llu\n",
                static_cast<unsigned long long>(obj_vertex),
                static_cast<unsigned long long>(obj_vertex + 1),
                static_cast<unsigned long long>(obj_vertex + 2));
            obj_vertex += 3;
        }
    }
    if (obj != nullptr)
        std::fclose(obj);
    const double rms = valid_vertices == 0
        ? 0.0
        : std::sqrt(squared_error / (valid_vertices * 3));
    const double screen_rms = screen_samples == 0
        ? 0.0
        : std::sqrt(screen_squared_error / (screen_samples * 2));
    opengt::render::WorldDrawList draw_list{};
    opengt::render::WorldTopologyStats topology{};
    const auto draw_result = opengt::render::build_world_draw_list(
        header,
        triangles.data(),
        triangles.size(),
        opengt::render::WorldDrawListOptions{
            include_secondary, include_screen_space, continuous_projection},
        &draw_list);
    if (
        depth_audit &&
        draw_result == opengt::render::WorldDrawListResult::success
    ) {
        audit_depth_precision(draw_list);
    }
    if (argc >= 4 && draw_result ==
            opengt::render::WorldDrawListResult::success) {
        std::FILE* csv = std::fopen(argv[3], "wb");
        if (csv == nullptr) {
            std::fprintf(stderr, "cannot write %s\n", argv[3]);
            return 1;
        }
        std::fprintf(
            csv,
            "command,source_command,object_kind,object_id,model_pointer,channel,"
            "transform_id,material,primitive_flags,texture_page,clut,ordering_table,"
            "vertex,model_x,model_y,model_z,world_x,world_y,world_z,"
            "view_x,view_y,view_z,screen_x,screen_y,"
            "source_screen_x,source_screen_y,authored_screen_x,authored_screen_y,"
            "clip_x,clip_y,clip_z,clip_w,projection_offset_x,"
            "projection_offset_y,projection_plane,u,v,r,g,b,"
            "source_identity,provenance_flags\n");
        for (std::size_t command_index = 0;
             command_index < draw_list.commands.size();
             ++command_index) {
            const auto& command = draw_list.commands[command_index];
            const auto& material =
                draw_list.materials[command.material_index];
            for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
                const auto& vertex = command.vertices[vertex_index];
                const auto* source_vertex =
                    command.source_command_index < triangles.size()
                    ? &triangles[command.source_command_index]
                        .vertices[vertex_index]
                    : nullptr;
                std::fprintf(
                    csv,
                    "%zu,%u,%u,%u,%u,%u,%llu,%u,%u,%u,%u,%d,%d,%d,%d,%d,"
                    "%.6f,%.6f,%.6f,%.0f,%.0f,%.0f,%.6f,%.6f,"
                    "%.6f,%.6f,%d,%d,"
                    "%.6f,%.6f,%.6f,%.6f,%.6f,%.6f,%.6f,"
                    "%.6f,%.6f,%u,%u,%u,%u,%u\n",
                    command_index,
                    command.source_command_index,
                    command.object_kind,
                    command.object_id,
                    command.model_pointer,
                    static_cast<unsigned>(command.channel),
                    static_cast<unsigned long long>(command.transform_id),
                    command.material_index,
                    material.primitive_flags,
                    material.texture_page,
                    material.clut,
                    command.ordering_table_index,
                    vertex_index,
                    vertex.model_x,
                    vertex.model_y,
                    vertex.model_z,
                    vertex.world_x,
                    vertex.world_y,
                    vertex.world_z,
                    vertex.view_x,
                    vertex.view_y,
                    vertex.view_z,
                    vertex.screen_x,
                    vertex.screen_y,
                    source_vertex != nullptr
                        ? source_vertex->screen_x
                        : std::numeric_limits<float>::quiet_NaN(),
                    source_vertex != nullptr
                        ? source_vertex->screen_y
                        : std::numeric_limits<float>::quiet_NaN(),
                    vertex.authored_screen_x,
                    vertex.authored_screen_y,
                    vertex.clip_x,
                    vertex.clip_y,
                    vertex.clip_z,
                    vertex.clip_w,
                    vertex.projection_offset_x,
                    vertex.projection_offset_y,
                    vertex.projection_plane,
                    vertex.u,
                    vertex.v,
                    vertex.r,
                    vertex.g,
                    vertex.b,
                    vertex.source_vertex_identity,
                    static_cast<unsigned>(vertex.provenance_flags));
            }
        }
        std::fclose(csv);
    }
    const auto topology_result =
        draw_result == opengt::render::WorldDrawListResult::success
            ? opengt::render::apply_world_topology(
                &draw_list,
                opengt::render::WorldTopologyOptions{true, true, true},
                &topology)
            : opengt::render::WorldTopologyResult::invalid_argument;
    std::printf(
        "version=%u frame=%llu poll=%d "
        "display=%d,%d/%dx%d drawOffset=%d,%d "
        "projectionOffset=%d,%d projectionPlane=%u "
        "continuousProjection=%s triangles=%u validVertices=%llu "
        "sourceVertices=%llu trackSourceVertices=%llu "
        "trackMissingSourceVertices=%llu uniqueTrackSources=%zu "
        "trackTriangles=%llu vehicleTriangles=%llu trackObjects=%zu "
        "vehicleObjects=%zu objects=%zu models=%zu materials=%zu "
        "transforms=%zu cameraTransform=%016llx "
        "mainProjectionTriangles=%llu secondaryProjectionTriangles=%llu "
        "worldMin=%.3f,%.3f,%.3f worldMax=%.3f,%.3f,%.3f "
        "viewRoundTripRms=%.6f screenProjectionRms=%.6f "
        "screenProjectionMax=%.6f screenSamples=%llu "
        "screenErrorOver1=%llu screenErrorOver2=%llu "
        "screenErrorOver10=%llu trackErrorOver2=%llu "
        "vehicleErrorOver2=%llu "
        "topologyResult=%s topologyInput=%u topologyOutput=%u "
        "topologyEligible=%u topologyMissingProvenance=%u "
        "topologyPositionGroups=%u topologyBoundaryGroups=%u "
        "topologyAdjusted=%u topologyProjectionGroups=%u "
        "topologyProjectionAdjusted=%u topologySeamGroups=%u "
        "topologySeamAdjusted=%u topologyProjectedTJunctions=%u "
        "topologyRasterGroups=%u topologyRasterAdjusted=%u "
        "topologyOverlapSeamGroups=%u topologyOverlapSeamAdjusted=%u "
        "topologyProjectedTJunctionAdjusted=%u topologyBoundaryEdges=%u "
        "topologyManifoldEdges=%u topologyNonmanifoldEdges=%u "
        "topologyTJunctions=%u topologySplitSources=%u "
        "topologySplitTriangles=%u topologyCoplanarPairs=%u "
        "topologyMaterialPairs=%u topologyDuplicatePairs=%u "
        "topologyOwnershipGroups=%u topologyReorders=%u obj=%s\n",
        header.version,
        static_cast<unsigned long long>(header.frame_index),
        header.input_poll,
        header.display_x,
        header.display_y,
        header.display_width,
        header.display_height,
        header.draw_offset_x,
        header.draw_offset_y,
        header.projection_offset_x,
        header.projection_offset_y,
        header.projection_plane,
        continuous_projection ? "yes" : "no",
        header.triangle_count,
        static_cast<unsigned long long>(valid_vertices),
        static_cast<unsigned long long>(source_vertices),
        static_cast<unsigned long long>(track_source_vertices),
        static_cast<unsigned long long>(track_missing_source_vertices),
        unique_track_sources.size(),
        static_cast<unsigned long long>(track_triangles),
        static_cast<unsigned long long>(vehicle_triangles),
        track_objects.size(),
        vehicle_objects.size(),
        objects.size(),
        models.size(),
        materials.size(),
        transforms.size(),
        static_cast<unsigned long long>(header.camera_transform_id),
        static_cast<unsigned long long>(main_projection_triangles),
        static_cast<unsigned long long>(secondary_projection_triangles),
        minimum[0], minimum[1], minimum[2],
        maximum[0], maximum[1], maximum[2],
        rms,
        screen_rms,
        screen_max_error,
        static_cast<unsigned long long>(screen_samples),
        static_cast<unsigned long long>(screen_error_over_one),
        static_cast<unsigned long long>(screen_error_over_two),
        static_cast<unsigned long long>(screen_error_over_ten),
        static_cast<unsigned long long>(track_screen_error_over_two),
        static_cast<unsigned long long>(vehicle_screen_error_over_two),
        opengt::render::world_topology_result_name(topology_result),
        topology.input_commands,
        topology.output_commands,
        topology.eligible_track_commands,
        topology.skipped_without_provenance,
        topology.exact_position_groups,
        topology.authored_boundary_groups,
        topology.adjusted_vertex_instances,
        topology.authored_projection_groups,
        topology.adjusted_projection_instances,
        topology.projected_seam_groups,
        topology.adjusted_seam_instances,
        topology.projected_t_junctions,
        topology.authored_raster_groups,
        topology.adjusted_authored_raster_instances,
        topology.authored_overlap_seam_groups,
        topology.adjusted_authored_overlap_instances,
        topology.adjusted_projected_t_junction_instances,
        topology.boundary_edges,
        topology.manifold_edges,
        topology.nonmanifold_edges,
        topology.exact_t_junctions,
        topology.split_source_triangles,
        topology.emitted_split_triangles,
        topology.coplanar_overlap_pairs,
        topology.material_overlap_pairs,
        topology.exact_duplicate_pairs,
        topology.ownership_components,
        topology.ownership_reorders,
        argc >= 3 ? argv[2] : "none");
    return valid_vertices == static_cast<std::uint64_t>(
        header.triangle_count) * 3 ? 0 : 1;
}
