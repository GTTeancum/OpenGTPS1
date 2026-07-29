#include "opengt/world_capture.hpp"

#include <algorithm>
#include <cmath>
#include <cstdio>
#include <limits>
#include <set>
#include <tuple>
#include <vector>

int main(int argc, char** argv) {
    if (argc < 2 || argc > 3) {
        std::fprintf(
            stderr,
            "usage: opengt_world_capture_inspect <capture.ogtwcap> "
            "[output.obj]\n");
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
    std::FILE* obj = argc == 3 ? std::fopen(argv[2], "wb") : nullptr;
    if (argc == 3 && obj == nullptr) {
        std::fprintf(stderr, "cannot write %s\n", argv[2]);
        return 1;
    }
    std::uint64_t obj_vertex = 1;
    for (const auto& triangle : triangles) {
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
    std::printf(
        "frame=%llu poll=%d triangles=%u validVertices=%llu "
        "trackTriangles=%llu vehicleTriangles=%llu trackObjects=%zu "
        "vehicleObjects=%zu objects=%zu models=%zu materials=%zu "
        "transforms=%zu cameraTransform=%016llx "
        "worldMin=%.3f,%.3f,%.3f worldMax=%.3f,%.3f,%.3f "
        "viewRoundTripRms=%.6f obj=%s\n",
        static_cast<unsigned long long>(header.frame_index),
        header.input_poll,
        header.triangle_count,
        static_cast<unsigned long long>(valid_vertices),
        static_cast<unsigned long long>(track_triangles),
        static_cast<unsigned long long>(vehicle_triangles),
        track_objects.size(),
        vehicle_objects.size(),
        objects.size(),
        models.size(),
        materials.size(),
        transforms.size(),
        static_cast<unsigned long long>(header.camera_transform_id),
        minimum[0], minimum[1], minimum[2],
        maximum[0], maximum[1], maximum[2],
        rms,
        argc == 3 ? argv[2] : "none");
    return valid_vertices == static_cast<std::uint64_t>(
        header.triangle_count) * 3 ? 0 : 1;
}
