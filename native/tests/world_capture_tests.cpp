#include "opengt/world_capture.hpp"

#include <array>
#include <cmath>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <vector>

namespace {

void put_u16(std::uint8_t* out, std::uint16_t value) {
    out[0] = static_cast<std::uint8_t>(value);
    out[1] = static_cast<std::uint8_t>(value >> 8);
}

void put_u32(std::uint8_t* out, std::uint32_t value) {
    out[0] = static_cast<std::uint8_t>(value);
    out[1] = static_cast<std::uint8_t>(value >> 8);
    out[2] = static_cast<std::uint8_t>(value >> 16);
    out[3] = static_cast<std::uint8_t>(value >> 24);
}

void put_u64(std::uint8_t* out, std::uint64_t value) {
    put_u32(out, static_cast<std::uint32_t>(value));
    put_u32(out + 4, static_cast<std::uint32_t>(value >> 32));
}

void put_f32(std::uint8_t* out, float value) {
    std::uint32_t bits = 0;
    std::memcpy(&bits, &value, sizeof(bits));
    put_u32(out, bits);
}

void put_vertex_base(
    std::uint8_t* out,
    std::int16_t x,
    std::int16_t y,
    std::int16_t z,
    std::uint32_t source_pointer
) {
    put_f32(out, static_cast<float>(x));
    put_f32(out + 4, static_cast<float>(y));
    put_f32(out + 8, static_cast<float>(z));
    out[16] = out[17] = out[18] = 128;
    out[19] = 1;
    put_u16(out + 20, static_cast<std::uint16_t>(x));
    put_u16(out + 22, static_cast<std::uint16_t>(y));
    put_u16(out + 24, static_cast<std::uint16_t>(z));
    put_u32(out + 28, static_cast<std::uint32_t>(x + 10));
    put_u32(out + 32, static_cast<std::uint32_t>(y + 20));
    put_u32(out + 36, static_cast<std::uint32_t>(z + 30));
    put_u32(out + 40, 160U << 16);
    put_u32(out + 44, 120U << 16);
    put_u32(out + 48, 256);
    put_u32(out + 52, source_pointer);
}

void put_vertex(
    std::uint8_t* out,
    std::int16_t x,
    std::int16_t y,
    std::int16_t z,
    std::uint32_t source_pointer
) {
    put_vertex_base(out, x, y, z, source_pointer);
    put_u64(out + 56, 0x9000000000000000ULL | source_pointer);
    put_u16(out + 64, 4096);
    put_u16(out + 72, 4096);
    put_u16(out + 80, 4096);
    put_u32(out + 84, 10);
    put_u32(out + 88, 20);
    put_u32(out + 92, 30);
}

bool write_fixture(const char* path) {
    constexpr std::size_t prefix_size =
        opengt::render::world_capture_header_size +
        opengt::render::world_capture_triangle_stride;
    std::array<std::uint8_t, prefix_size> bytes{};
    std::memcpy(bytes.data(), "OGTWCAP\0", 8);
    put_u32(bytes.data() + 8, opengt::render::world_capture_version);
    put_u32(
        bytes.data() + 12,
        opengt::render::world_capture_header_size);
    put_u64(bytes.data() + 16, 7);
    put_u32(bytes.data() + 24, 1234);
    put_u32(bytes.data() + 36, 320);
    put_u32(bytes.data() + 40, 240);
    put_u32(bytes.data() + 44, 1);
    put_u32(
        bytes.data() + 48,
        opengt::render::world_capture_triangle_stride);
    put_u32(bytes.data() + 52, 1024);
    put_u32(bytes.data() + 56, 512);
    put_u64(bytes.data() + 60, opengt::render::world_capture_header_size);
    put_u64(bytes.data() + 68, prefix_size);
    put_u64(bytes.data() + 76, 1024U * 512U * 2U);
    put_u32(bytes.data() + 84, 1U << 2);
    put_u64(bytes.data() + 88, 0x1234);
    put_u16(bytes.data() + 96, 4096);
    put_u16(bytes.data() + 104, 4096);
    put_u16(bytes.data() + 112, 4096);
    put_u32(bytes.data() + 116, 10);
    put_u32(bytes.data() + 120, 20);
    put_u32(bytes.data() + 124, 30);
    put_u32(bytes.data() + 128, 160U << 16);
    put_u32(bytes.data() + 132, 120U << 16);
    put_u32(bytes.data() + 136, 256);

    std::uint8_t* triangle =
        bytes.data() + opengt::render::world_capture_header_size;
    put_u32(triangle, 1);
    put_u32(triangle + 32, 1);
    put_u32(triangle + 36, 42);
    put_u32(triangle + 40, 0x80123456);
    put_u16(triangle + 44, 3);
    put_u16(triangle + 46, 4);
    put_u64(triangle + 48, 0x1234);
    put_u16(triangle + 56, 4096);
    put_u16(triangle + 64, 4096);
    put_u16(triangle + 72, 4096);
    put_u32(triangle + 76, 10);
    put_u32(triangle + 80, 20);
    put_u32(triangle + 84, 30);
    put_vertex(triangle + 88, 1, 2, 3, 0x80001000);
    put_vertex(triangle + 184, 4, 5, 6, 0x80001014);
    put_vertex(triangle + 280, 7, 8, 9, 0x80001028);

    std::vector<std::uint16_t> vram(1024U * 512U);
    std::FILE* file = std::fopen(path, "wb");
    if (file == nullptr)
        return false;
    const bool okay =
        std::fwrite(bytes.data(), 1, bytes.size(), file) == bytes.size() &&
        std::fwrite(
            vram.data(), sizeof(std::uint16_t), vram.size(), file
        ) == vram.size();
    std::fclose(file);
    return okay;
}

bool write_v5_fixture(const char* path) {
    constexpr std::size_t prefix_size =
        opengt::render::world_capture_header_size +
        opengt::render::world_capture_v5_triangle_stride;
    std::array<std::uint8_t, prefix_size> bytes{};
    std::memcpy(bytes.data(), "OGTWCAP\0", 8);
    put_u32(bytes.data() + 8, 5);
    put_u32(
        bytes.data() + 12,
        opengt::render::world_capture_header_size);
    put_u64(bytes.data() + 16, 8);
    put_u32(bytes.data() + 24, 1236);
    put_u32(bytes.data() + 36, 320);
    put_u32(bytes.data() + 40, 240);
    put_u32(bytes.data() + 44, 1);
    put_u32(
        bytes.data() + 48,
        opengt::render::world_capture_v5_triangle_stride);
    put_u32(bytes.data() + 52, 1024);
    put_u32(bytes.data() + 56, 512);
    put_u64(bytes.data() + 60, opengt::render::world_capture_header_size);
    put_u64(bytes.data() + 68, prefix_size);
    put_u64(bytes.data() + 76, 1024U * 512U * 2U);
    put_u32(bytes.data() + 84, 1U << 2);
    put_u64(bytes.data() + 88, 0x1234);
    put_u16(bytes.data() + 96, 4096);
    put_u16(bytes.data() + 104, 4096);
    put_u16(bytes.data() + 112, 4096);
    put_u32(bytes.data() + 128, 160U << 16);
    put_u32(bytes.data() + 132, 120U << 16);
    put_u32(bytes.data() + 136, 256);

    std::uint8_t* triangle =
        bytes.data() + opengt::render::world_capture_header_size;
    put_u32(triangle, 1);
    put_u32(triangle + 32, 1);
    put_u32(triangle + 36, 43);
    put_u32(triangle + 40, 0x80123456);
    put_u64(triangle + 48, 0x5678);
    put_u16(triangle + 56, 4096);
    put_u16(triangle + 64, 4096);
    put_u16(triangle + 72, 4096);
    put_u32(triangle + 76, 40);
    put_u32(triangle + 80, 50);
    put_u32(triangle + 84, 60);
    put_vertex_base(triangle + 88, 1, 2, 3, 0x80002000);
    put_vertex_base(triangle + 144, 4, 5, 6, 0x80002014);
    put_vertex_base(triangle + 200, 7, 8, 9, 0x80002028);

    std::vector<std::uint16_t> vram(1024U * 512U);
    std::FILE* file = std::fopen(path, "wb");
    if (file == nullptr)
        return false;
    const bool okay =
        std::fwrite(bytes.data(), 1, bytes.size(), file) == bytes.size() &&
        std::fwrite(
            vram.data(), sizeof(std::uint16_t), vram.size(), file
        ) == vram.size();
    std::fclose(file);
    return okay;
}

bool expect(bool value, const char* message) {
    if (!value)
        std::fprintf(stderr, "FAILED: %s\n", message);
    return value;
}

} // namespace

int main() {
    const char* path = "opengt_world_capture_test.ogtwcap";
    const char* v5_path = "opengt_world_capture_v5_test.ogtwcap";
    bool okay = expect(write_fixture(path), "write fixture");
    opengt::render::WorldCaptureHeader header{};
    okay &= expect(
        opengt::render::read_world_capture_header(path, &header) ==
            opengt::render::WorldCaptureReadResult::success,
        "read header");
    okay &= expect(header.frame_index == 7, "frame index");
    okay &= expect(header.input_poll == 1234, "input poll");
    okay &= expect(header.triangle_count == 1, "triangle count");
    std::array<opengt::render::WorldCaptureTriangle, 1> triangles{};
    std::vector<std::uint16_t> vram(1024U * 512U);
    okay &= expect(
        opengt::render::load_world_capture(
            path, &header,
            triangles.data(), 0,
            vram.data(), vram.size()) ==
            opengt::render::WorldCaptureReadResult::capacity_too_small,
        "reject undersized triangle storage");
    okay &= expect(
        opengt::render::load_world_capture(
            path, &header,
            triangles.data(), triangles.size(),
            vram.data(), vram.size()) ==
            opengt::render::WorldCaptureReadResult::success,
        "load capture");
    const auto& vertex = triangles[0].vertices[0];
    okay &= expect(triangles[0].object_id == 42, "stable object identity");
    okay &= expect(
        triangles[0].draw_offset_x == 3 &&
        triangles[0].draw_offset_y == 4,
        "per-draw projection offset");
    okay &= expect(
        triangles[0].exact_transform_valid &&
        triangles[0].transform_rotation[0] == 4096 &&
        triangles[0].transform_rotation[4] == 4096 &&
        triangles[0].transform_rotation[8] == 4096 &&
        triangles[0].transform_translation[0] == 10 &&
        triangles[0].transform_translation[1] == 20 &&
        triangles[0].transform_translation[2] == 30,
        "retain exact object transform");
    okay &= expect(
        vertex.projection_offset_x == (160 << 16) &&
        vertex.projection_offset_y == (120 << 16) &&
        vertex.projection_plane == 256,
        "per-vertex projection state");
    okay &= expect(
        vertex.source_vertex_identity == 0x80001000,
        "source vertex provenance");
    okay &= expect(
        vertex.exact_transform_valid &&
        vertex.transform_id == 0x9000000080001000ULL &&
        vertex.transform_rotation[0] == 4096 &&
        vertex.transform_rotation[4] == 4096 &&
        vertex.transform_rotation[8] == 4096 &&
        vertex.transform_translation[0] == 10 &&
        vertex.transform_translation[1] == 20 &&
        vertex.transform_translation[2] == 30,
        "retain each vertex's exact transform");
    okay &= expect(
        std::fabs(vertex.world_x - 1.0F) < 0.001F &&
        std::fabs(vertex.world_y - 2.0F) < 0.001F &&
        std::fabs(vertex.world_z - 3.0F) < 0.001F,
        "recover world coordinates");

    okay &= expect(write_v5_fixture(v5_path), "write v5 fixture");
    std::array<opengt::render::WorldCaptureTriangle, 1> v5_triangles{};
    okay &= expect(
        opengt::render::load_world_capture(
            v5_path, &header,
            v5_triangles.data(), v5_triangles.size(),
            vram.data(), vram.size()) ==
            opengt::render::WorldCaptureReadResult::success,
        "load v5 capture");
    okay &= expect(
        header.version == 5 &&
        header.triangle_stride ==
            opengt::render::world_capture_v5_triangle_stride,
        "retain v5 layout");
    for (const auto& legacy_vertex : v5_triangles[0].vertices) {
        okay &= expect(
            legacy_vertex.exact_transform_valid &&
            legacy_vertex.transform_id == 0x5678 &&
            legacy_vertex.transform_rotation[0] == 4096 &&
            legacy_vertex.transform_rotation[4] == 4096 &&
            legacy_vertex.transform_rotation[8] == 4096 &&
            legacy_vertex.transform_translation[0] == 40 &&
            legacy_vertex.transform_translation[1] == 50 &&
            legacy_vertex.transform_translation[2] == 60,
            "propagate v5 command transform to each vertex");
    }

    {
        std::FILE* file = std::fopen(path, "r+b");
        std::uint8_t oversized_count[4];
        put_u32(
            oversized_count,
            opengt::render::world_capture_max_triangles + 1);
        const bool corrupted =
            file != nullptr &&
            std::fseek(file, 44, SEEK_SET) == 0 &&
            std::fwrite(
                oversized_count,
                1,
                sizeof(oversized_count),
                file) == sizeof(oversized_count);
        if (file != nullptr)
            std::fclose(file);
        okay &= expect(corrupted, "write oversized triangle count");
        okay &= expect(
            opengt::render::read_world_capture_header(path, &header) ==
                opengt::render::WorldCaptureReadResult::invalid_layout,
            "reject oversized triangle count");
    }

    std::remove(path);
    std::remove(v5_path);
    if (!okay)
        return 1;
    std::puts("world capture tests passed");
    return 0;
}
