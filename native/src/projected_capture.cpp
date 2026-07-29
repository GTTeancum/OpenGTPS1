#include "opengt/projected_capture.hpp"

#include <cstdio>
#include <cstring>
#include <limits>

namespace opengt::render {
namespace {

constexpr std::uint32_t header_size = 80;

std::uint16_t u16(const std::uint8_t* bytes) {
    return static_cast<std::uint16_t>(
        bytes[0] | (static_cast<std::uint16_t>(bytes[1]) << 8)
    );
}

std::uint32_t u32(const std::uint8_t* bytes) {
    return
        static_cast<std::uint32_t>(bytes[0]) |
        (static_cast<std::uint32_t>(bytes[1]) << 8) |
        (static_cast<std::uint32_t>(bytes[2]) << 16) |
        (static_cast<std::uint32_t>(bytes[3]) << 24);
}

std::uint64_t u64(const std::uint8_t* bytes) {
    return
        static_cast<std::uint64_t>(u32(bytes)) |
        (static_cast<std::uint64_t>(u32(bytes + 4)) << 32);
}

std::int16_t i16(const std::uint8_t* bytes) {
    return static_cast<std::int16_t>(u16(bytes));
}

std::int32_t i32(const std::uint8_t* bytes) {
    return static_cast<std::int32_t>(u32(bytes));
}

float f32(const std::uint8_t* bytes) {
    const std::uint32_t bits = u32(bytes);
    float value;
    std::memcpy(&value, &bits, sizeof(value));
    return value;
}

CaptureReadResult parse_header(
    const std::uint8_t* bytes,
    ProjectedCaptureHeader* header
) {
    const char magic[8] = {'O', 'G', 'T', 'P', 'C', 'A', 'P', '\0'};
    if (std::memcmp(bytes, magic, sizeof(magic)) != 0)
        return CaptureReadResult::invalid_magic;
    if (u32(bytes + 8) != projected_capture_version)
        return CaptureReadResult::unsupported_version;
    if (
        u32(bytes + 12) != header_size ||
        u32(bytes + 48) != projected_capture_triangle_stride
    )
        return CaptureReadResult::invalid_layout;

    header->frame_index = u64(bytes + 16);
    header->input_poll = i32(bytes + 24);
    header->display_x = i32(bytes + 28);
    header->display_y = i32(bytes + 32);
    header->display_width = i32(bytes + 36);
    header->display_height = i32(bytes + 40);
    header->triangle_count = u32(bytes + 44);
    header->vram_width = u32(bytes + 52);
    header->vram_height = u32(bytes + 56);
    header->vram_offset = u64(bytes + 60);
    header->vram_size = u64(bytes + 68);
    header->flags = u32(bytes + 76);

    if (
        header->display_width <= 0 ||
        header->display_height <= 0 ||
        header->display_width > 4096 ||
        header->display_height > 4096 ||
        header->triangle_count > projected_capture_max_triangles ||
        header->vram_width != 1024 ||
        header->vram_height != 512 ||
        header->vram_size !=
            static_cast<std::uint64_t>(header->vram_width) *
            header->vram_height * 2
    )
        return CaptureReadResult::invalid_layout;

    const std::uint64_t triangle_end =
        header_size +
        static_cast<std::uint64_t>(header->triangle_count) *
        projected_capture_triangle_stride;
    if (header->vram_offset != triangle_end)
        return CaptureReadResult::invalid_layout;
    return CaptureReadResult::success;
}

CaptureReadResult read_header(
    std::FILE* file,
    ProjectedCaptureHeader* header
) {
    std::uint8_t bytes[header_size];
    if (std::fread(bytes, 1, sizeof(bytes), file) != sizeof(bytes))
        return CaptureReadResult::truncated;
    return parse_header(bytes, header);
}

void parse_vertex(
    const std::uint8_t* bytes,
    ProjectedCaptureVertex* vertex
) {
    vertex->x = f32(bytes);
    vertex->y = f32(bytes + 4);
    vertex->z = f32(bytes + 8);
    vertex->u = i16(bytes + 12);
    vertex->v = i16(bytes + 14);
    vertex->r = bytes[16];
    vertex->g = bytes[17];
    vertex->b = bytes[18];
    vertex->has_gte_depth = (bytes[19] & 1) != 0;
}

void parse_triangle(
    const std::uint8_t* bytes,
    ProjectedCaptureTriangle* triangle
) {
    triangle->primitive_flags = u32(bytes);
    triangle->texture_page = u16(bytes + 4);
    triangle->clut = u16(bytes + 6);
    triangle->ordering_table_index = i32(bytes + 8);
    triangle->clip_x0 = i16(bytes + 12);
    triangle->clip_y0 = i16(bytes + 14);
    triangle->clip_x1 = i16(bytes + 16);
    triangle->clip_y1 = i16(bytes + 18);
    triangle->texture_mask_x = i16(bytes + 20);
    triangle->texture_mask_y = i16(bytes + 22);
    triangle->texture_offset_x = i16(bytes + 24);
    triangle->texture_offset_y = i16(bytes + 26);
    triangle->environment_flags = u32(bytes + 28);
    parse_vertex(bytes + 32, &triangle->vertices[0]);
    parse_vertex(bytes + 52, &triangle->vertices[1]);
    parse_vertex(bytes + 72, &triangle->vertices[2]);
}

} // namespace

CaptureReadResult read_projected_capture_header(
    const char* path,
    ProjectedCaptureHeader* header
) noexcept {
    if (path == nullptr || header == nullptr)
        return CaptureReadResult::invalid_argument;
    std::FILE* file = std::fopen(path, "rb");
    if (file == nullptr)
        return CaptureReadResult::open_failed;
    const CaptureReadResult result = read_header(file, header);
    std::fclose(file);
    return result;
}

CaptureReadResult load_projected_capture(
    const char* path,
    ProjectedCaptureHeader* header,
    ProjectedCaptureTriangle* triangles,
    std::size_t triangle_capacity,
    std::uint16_t* vram,
    std::size_t vram_word_capacity
) noexcept {
    if (
        path == nullptr ||
        header == nullptr ||
        (triangles == nullptr && triangle_capacity != 0) ||
        vram == nullptr
    )
        return CaptureReadResult::invalid_argument;

    std::FILE* file = std::fopen(path, "rb");
    if (file == nullptr)
        return CaptureReadResult::open_failed;

    CaptureReadResult result = read_header(file, header);
    if (result != CaptureReadResult::success) {
        std::fclose(file);
        return result;
    }

    const std::size_t required_vram =
        static_cast<std::size_t>(header->vram_width) * header->vram_height;
    if (
        triangle_capacity < header->triangle_count ||
        vram_word_capacity < required_vram
    ) {
        std::fclose(file);
        return CaptureReadResult::capacity_too_small;
    }

    std::uint8_t record[projected_capture_triangle_stride];
    for (std::uint32_t index = 0; index < header->triangle_count; ++index) {
        if (std::fread(record, 1, sizeof(record), file) != sizeof(record)) {
            std::fclose(file);
            return CaptureReadResult::truncated;
        }
        parse_triangle(record, &triangles[index]);
    }

    if (
        header->vram_offset >
            static_cast<std::uint64_t>(std::numeric_limits<long>::max()) ||
        std::fseek(file, static_cast<long>(header->vram_offset), SEEK_SET) != 0
    ) {
        std::fclose(file);
        return CaptureReadResult::io_error;
    }
    for (std::size_t index = 0; index < required_vram; ++index) {
        std::uint8_t bytes[2];
        if (std::fread(bytes, 1, 2, file) != 2) {
            std::fclose(file);
            return CaptureReadResult::truncated;
        }
        vram[index] = u16(bytes);
    }

    std::fclose(file);
    return CaptureReadResult::success;
}

const char* capture_read_result_name(CaptureReadResult result) noexcept {
    switch (result) {
        case CaptureReadResult::success: return "success";
        case CaptureReadResult::invalid_argument: return "invalid_argument";
        case CaptureReadResult::open_failed: return "open_failed";
        case CaptureReadResult::truncated: return "truncated";
        case CaptureReadResult::invalid_magic: return "invalid_magic";
        case CaptureReadResult::unsupported_version:
            return "unsupported_version";
        case CaptureReadResult::invalid_layout: return "invalid_layout";
        case CaptureReadResult::capacity_too_small:
            return "capacity_too_small";
        case CaptureReadResult::io_error: return "io_error";
    }
    return "unknown";
}

} // namespace opengt::render
