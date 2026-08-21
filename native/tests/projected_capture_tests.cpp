#include "opengt/projected_capture.hpp"
#include "opengt/projected_reference_renderer.hpp"

#include <array>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <vector>

namespace {

constexpr std::size_t header_size = 80;
constexpr std::size_t record_size = 96;

void put_u16(std::uint8_t* destination, std::uint16_t value) {
    destination[0] = static_cast<std::uint8_t>(value);
    destination[1] = static_cast<std::uint8_t>(value >> 8);
}

void put_u32(std::uint8_t* destination, std::uint32_t value) {
    destination[0] = static_cast<std::uint8_t>(value);
    destination[1] = static_cast<std::uint8_t>(value >> 8);
    destination[2] = static_cast<std::uint8_t>(value >> 16);
    destination[3] = static_cast<std::uint8_t>(value >> 24);
}

void put_u64(std::uint8_t* destination, std::uint64_t value) {
    put_u32(destination, static_cast<std::uint32_t>(value));
    put_u32(destination + 4, static_cast<std::uint32_t>(value >> 32));
}

void put_f32(std::uint8_t* destination, float value) {
    std::uint32_t bits = 0;
    static_assert(sizeof(bits) == sizeof(value));
    std::memcpy(&bits, &value, sizeof(bits));
    put_u32(destination, bits);
}

void put_vertex(
    std::uint8_t* destination,
    float x,
    float y,
    float z,
    std::int16_t u,
    std::int16_t v
) {
    put_f32(destination, x);
    put_f32(destination + 4, y);
    put_f32(destination + 8, z);
    put_u16(destination + 12, static_cast<std::uint16_t>(u));
    put_u16(destination + 14, static_cast<std::uint16_t>(v));
    destination[16] = 128;
    destination[17] = 128;
    destination[18] = 128;
    destination[19] = 1;
}

bool write_fixture(const char* path) {
    constexpr std::size_t vram_words = 1024U * 512U;
    std::array<std::uint8_t, header_size + record_size> prefix{};
    std::memcpy(prefix.data(), "OGTPCAP\0", 8);
    put_u32(prefix.data() + 8, opengt::render::projected_capture_version);
    put_u32(prefix.data() + 12, static_cast<std::uint32_t>(header_size));
    put_u64(prefix.data() + 16, 42);
    put_u32(prefix.data() + 24, 9001);
    put_u32(prefix.data() + 28, 0);
    put_u32(prefix.data() + 32, 0);
    put_u32(prefix.data() + 36, 8);
    put_u32(prefix.data() + 40, 8);
    put_u32(prefix.data() + 44, 1);
    put_u32(prefix.data() + 48, static_cast<std::uint32_t>(record_size));
    put_u32(prefix.data() + 52, 1024);
    put_u32(prefix.data() + 56, 512);
    put_u64(prefix.data() + 60, header_size + record_size);
    put_u64(prefix.data() + 68, vram_words * 2);

    std::uint8_t* record = prefix.data() + header_size;
    put_u32(record, 1); // textured
    put_u16(record + 4, 2U << 7); // 15-bit texture page at (0, 0)
    put_u16(record + 6, 0);
    put_u32(record + 8, 7);
    put_u16(record + 12, 0);
    put_u16(record + 14, 0);
    put_u16(record + 16, 7);
    put_u16(record + 18, 7);
    put_vertex(record + 32, 0.5F, 0.5F, 100.0F, 0, 0);
    put_vertex(record + 52, 7.5F, 0.5F, 200.0F, 31, 0);
    put_vertex(record + 72, 0.5F, 7.5F, 400.0F, 0, 31);

    std::vector<std::uint16_t> vram(vram_words);
    for (int y = 0; y < 32; ++y) {
        for (int x = 0; x < 32; ++x) {
            const std::uint16_t r = static_cast<std::uint16_t>(
                1 + x * 30 / 31
            );
            const std::uint16_t g = static_cast<std::uint16_t>(
                1 + y * 30 / 31
            );
            vram[static_cast<std::size_t>(y) * 1024 + x] =
                static_cast<std::uint16_t>(r | (g << 5) | (1U << 10));
        }
    }

    std::FILE* file = std::fopen(path, "wb");
    if (file == nullptr)
        return false;
    const bool okay =
        std::fwrite(prefix.data(), 1, prefix.size(), file) == prefix.size() &&
        std::fwrite(
            vram.data(),
            sizeof(std::uint16_t),
            vram.size(),
            file
        ) == vram.size() &&
        std::ferror(file) == 0;
    std::fclose(file);
    return okay;
}

bool expect(bool condition, const char* message) {
    if (!condition)
        std::fprintf(stderr, "FAILED: %s\n", message);
    return condition;
}

} // namespace

int main() {
    const char* path = "opengt_projected_capture_test.ogtcap";
    if (!expect(write_fixture(path), "write fixture"))
        return 1;

    opengt::render::ProjectedCaptureHeader header{};
    bool okay = expect(
        opengt::render::read_projected_capture_header(path, &header) ==
            opengt::render::CaptureReadResult::success,
        "read header"
    );
    okay &= expect(header.frame_index == 42, "frame index");
    okay &= expect(header.input_poll == 9001, "input poll");
    okay &= expect(header.triangle_count == 1, "triangle count");

    std::array<opengt::render::ProjectedCaptureTriangle, 1> triangles{};
    std::vector<std::uint16_t> vram(1024U * 512U);
    okay &= expect(
        opengt::render::load_projected_capture(
            path,
            &header,
            triangles.data(),
            0,
            vram.data(),
            vram.size()
        ) == opengt::render::CaptureReadResult::capacity_too_small,
        "reject undersized triangle storage"
    );
    okay &= expect(
        opengt::render::load_projected_capture(
            path,
            &header,
            triangles.data(),
            triangles.size(),
            vram.data(),
            vram.size()
        ) == opengt::render::CaptureReadResult::success,
        "load capture"
    );

    std::array<std::uint8_t, 8 * 8 * 4> perspective{};
    std::array<std::uint8_t, 8 * 8 * 4> affine{};
    const auto perspective_stats =
        opengt::render::render_projected_capture(
            header,
            triangles.data(),
            triangles.size(),
            vram.data(),
            vram.size(),
            perspective.data(),
            perspective.size(),
            {true, false, 0xFE000000U}
        );
    const auto affine_stats =
        opengt::render::render_projected_capture(
            header,
            triangles.data(),
            triangles.size(),
            vram.data(),
            vram.size(),
            affine.data(),
            affine.size(),
            {false, false, 0xFE000000U}
        );
    okay &= expect(
        perspective_stats.rasterized_triangles == 1,
        "rasterize captured triangle"
    );
    okay &= expect(
        perspective_stats.perspective_corrected_triangles == 1,
        "select perspective interpolation"
    );
    okay &= expect(
        affine_stats.perspective_corrected_triangles == 0,
        "select affine interpolation"
    );
    okay &= expect(
        perspective_stats.shaded_pixels > 0,
        "shade pixels"
    );
    okay &= expect(
        perspective != affine,
        "perspective output differs from affine output"
    );

    {
        std::FILE* file = std::fopen(path, "r+b");
        std::uint8_t oversized_count[4];
        put_u32(
            oversized_count,
            opengt::render::projected_capture_max_triangles + 1
        );
        const bool corrupted =
            file != nullptr &&
            std::fseek(file, 44, SEEK_SET) == 0 &&
            std::fwrite(
                oversized_count,
                1,
                sizeof(oversized_count),
                file
            ) == sizeof(oversized_count);
        if (file != nullptr)
            std::fclose(file);
        okay &= expect(corrupted, "write oversized triangle count");
        okay &= expect(
            opengt::render::read_projected_capture_header(path, &header) ==
                opengt::render::CaptureReadResult::invalid_layout,
            "reject oversized triangle count"
        );
    }

    std::remove(path);
    if (!okay)
        return 1;
    std::puts("projected capture tests passed");
    return 0;
}
