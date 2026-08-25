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

int main(int argc, char** argv) {
    if (argc < 2 || argc > 5) {
        std::fprintf(
            stderr,
            "usage: opengt_world_capture_inspect <capture.ogtwcap> "
            "[output.obj] [topology.csv] [--include-screen-space]\n");
        return 2;
    }
    const bool include_screen_space =
        argc == 5 &&
        std::strcmp(argv[4], "--include-screen-space") == 0;
    if (argc == 5 && !include_screen_space) {
        std::fprintf(stderr, "unknown option: %s\n", argv[4]);
        return 2;
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
            triangle.draw_offset_x == header.draw_offset_x &&
            triangle.draw_offset_y == header.draw_offset_y;
        for (const auto& vertex : triangle.vertices) {
            main_projection =
                main_projection &&
                vertex.projection_offset_x ==
                    header.projection_offset_x &&
                vertex.projection_offset_y ==
                    header.projection_offset_y &&
                vertex.projection_plane == header.projection_plane;
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
                const auto projected =
                    opengt::render::project_ps1_vertex(
                        vertex,
                        triangle.draw_offset_x,
                        triangle.draw_offset_y);
                const double projected_x = projected.x;
                const double projected_y = projected.y;
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
            false, include_screen_space, false},
        &draw_list);
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
            "view_x,view_y,view_z,screen_x,screen_y,u,v,r,g,b,"
            "source_identity\n");
        for (std::size_t command_index = 0;
             command_index < draw_list.commands.size();
             ++command_index) {
            const auto& command = draw_list.commands[command_index];
            const auto& material =
                draw_list.materials[command.material_index];
            for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
                const auto& vertex = command.vertices[vertex_index];
                std::fprintf(
                    csv,
                    "%zu,%u,%u,%u,%u,%u,%llu,%u,%u,%u,%u,%d,%d,%d,%d,%d,"
                    "%.6f,%.6f,%.6f,%.0f,%.0f,%.0f,%.6f,%.6f,"
                    "%.6f,%.6f,%u,%u,%u,%u\n",
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
                    vertex.u,
                    vertex.v,
                    vertex.r,
                    vertex.g,
                    vertex.b,
                    vertex.source_vertex_identity);
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
        "version=%u frame=%llu poll=%d triangles=%u validVertices=%llu "
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
