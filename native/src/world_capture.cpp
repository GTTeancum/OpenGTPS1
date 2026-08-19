#include "opengt/world_capture.hpp"

#include <cstdio>
#include <cstring>
#include <limits>

namespace opengt::render {
namespace {

std::uint16_t u16(const std::uint8_t* bytes) {
    return static_cast<std::uint16_t>(
        bytes[0] | (static_cast<std::uint16_t>(bytes[1]) << 8)
    );
}

std::uint32_t u32(const std::uint8_t* bytes) {
    return static_cast<std::uint32_t>(bytes[0]) |
        (static_cast<std::uint32_t>(bytes[1]) << 8) |
        (static_cast<std::uint32_t>(bytes[2]) << 16) |
        (static_cast<std::uint32_t>(bytes[3]) << 24);
}

std::uint64_t u64(const std::uint8_t* bytes) {
    return static_cast<std::uint64_t>(u32(bytes)) |
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

WorldCaptureReadResult parse_header(
    const std::uint8_t* bytes,
    std::uint32_t byte_count,
    WorldCaptureHeader* header
) {
    const char magic[8] = {'O', 'G', 'T', 'W', 'C', 'A', 'P', '\0'};
    if (std::memcmp(bytes, magic, 8) != 0)
        return WorldCaptureReadResult::invalid_magic;
    const std::uint32_t version = u32(bytes + 8);
    const std::uint32_t header_size = u32(bytes + 12);
    if (version < 1 || version > world_capture_version)
        return WorldCaptureReadResult::unsupported_version;
    const std::uint32_t expected_header_size =
        version == 1
            ? world_capture_v1_header_size
            : world_capture_header_size;
    const std::uint32_t expected_triangle_stride =
        version < 3
            ? world_capture_legacy_triangle_stride
            : version == 3
                ? world_capture_v3_triangle_stride
                : version == 4
                    ? world_capture_v4_triangle_stride
                    : version == 5
                        ? world_capture_v5_triangle_stride
                        : world_capture_triangle_stride;
    if (header_size != expected_header_size ||
        byte_count != expected_header_size ||
        u32(bytes + 48) != expected_triangle_stride)
        return WorldCaptureReadResult::invalid_layout;
    header->version = version;
    header->header_size = header_size;
    header->frame_index = u64(bytes + 16);
    header->input_poll = i32(bytes + 24);
    header->display_x = i32(bytes + 28);
    header->display_y = i32(bytes + 32);
    header->display_width = i32(bytes + 36);
    header->display_height = i32(bytes + 40);
    header->triangle_count = u32(bytes + 44);
    header->triangle_stride = u32(bytes + 48);
    header->vram_width = u32(bytes + 52);
    header->vram_height = u32(bytes + 56);
    header->triangle_offset = u64(bytes + 60);
    header->vram_offset = u64(bytes + 68);
    header->vram_size = u64(bytes + 76);
    header->flags = u32(bytes + 84);
    header->camera_transform_id = u64(bytes + 88);
    for (int index = 0; index < 9; ++index)
        header->camera_rotation[index] = i16(bytes + 96 + index * 2);
    for (int index = 0; index < 3; ++index)
        header->camera_translation[index] = i32(bytes + 116 + index * 4);
    header->projection_offset_x = 0;
    header->projection_offset_y = 0;
    header->projection_plane = 0;
    header->draw_offset_x = 0;
    header->draw_offset_y = 0;
    if (version >= 2) {
        header->projection_offset_x = i32(bytes + 128);
        header->projection_offset_y = i32(bytes + 132);
        header->projection_plane = u32(bytes + 136);
        header->draw_offset_x = i32(bytes + 140);
        header->draw_offset_y = i32(bytes + 144);
    }

    const std::uint64_t triangle_end =
        header->header_size +
        static_cast<std::uint64_t>(header->triangle_count) *
        header->triangle_stride;
    if (header->display_width <= 0 ||
        header->display_height <= 0 ||
        header->display_width > 4096 ||
        header->display_height > 4096 ||
        header->triangle_count > world_capture_max_triangles ||
        header->vram_width != 1024 ||
        header->vram_height != 512 ||
        header->triangle_offset != header->header_size ||
        header->vram_offset != triangle_end ||
        header->vram_size != 1024U * 512U * 2U ||
        header->camera_transform_id == 0 ||
        (version >= 2 &&
            (((header->flags & (1U << 2)) == 0) ||
             header->projection_plane == 0)))
        return WorldCaptureReadResult::invalid_layout;
    return WorldCaptureReadResult::success;
}

WorldCaptureReadResult read_header(
    std::FILE* file,
    WorldCaptureHeader* header
) {
    std::uint8_t bytes[world_capture_header_size]{};
    constexpr std::size_t preamble_size = 16;
    if (std::fread(bytes, 1, preamble_size, file) != preamble_size)
        return WorldCaptureReadResult::truncated;
    const std::uint32_t version = u32(bytes + 8);
    const std::uint32_t header_size = u32(bytes + 12);
    const std::uint32_t expected_header_size =
        version == 1
            ? world_capture_v1_header_size
            : version >= 2 && version <= world_capture_version
                ? world_capture_header_size
                : 0;
    if (expected_header_size == 0)
        return WorldCaptureReadResult::unsupported_version;
    if (header_size != expected_header_size)
        return WorldCaptureReadResult::invalid_layout;
    const std::size_t remaining = header_size - preamble_size;
    if (std::fread(
            bytes + preamble_size,
            1,
            remaining,
            file) != remaining)
        return WorldCaptureReadResult::truncated;
    return parse_header(bytes, header_size, header);
}

void calculate_world(
    const WorldCaptureHeader& header,
    WorldCaptureVertex* vertex
) {
    const float x =
        static_cast<float>(vertex->view_x - header.camera_translation[0]);
    const float y =
        static_cast<float>(vertex->view_y - header.camera_translation[1]);
    const float z =
        static_cast<float>(vertex->view_z - header.camera_translation[2]);
    constexpr float scale = 1.0F / 4096.0F;
    vertex->world_x = scale * (
        header.camera_rotation[0] * x +
        header.camera_rotation[3] * y +
        header.camera_rotation[6] * z);
    vertex->world_y = scale * (
        header.camera_rotation[1] * x +
        header.camera_rotation[4] * y +
        header.camera_rotation[7] * z);
    vertex->world_z = scale * (
        header.camera_rotation[2] * x +
        header.camera_rotation[5] * y +
        header.camera_rotation[8] * z);
}

void parse_vertex(
    const std::uint8_t* bytes,
    const WorldCaptureHeader& header,
    WorldCaptureVertex* vertex
) {
    vertex->screen_x = f32(bytes);
    vertex->screen_y = f32(bytes + 4);
    vertex->screen_z = f32(bytes + 8);
    vertex->u = i16(bytes + 12);
    vertex->v = i16(bytes + 14);
    vertex->r = bytes[16];
    vertex->g = bytes[17];
    vertex->b = bytes[18];
    vertex->world_valid = (bytes[19] & 1U) != 0;
    vertex->screen_offset_anchor = (bytes[19] & 2U) != 0;
    vertex->model_x = i16(bytes + 20);
    vertex->model_y = i16(bytes + 22);
    vertex->model_z = i16(bytes + 24);
    vertex->view_x = i32(bytes + 28);
    vertex->view_y = i32(bytes + 32);
    vertex->view_z = i32(bytes + 36);
    vertex->projection_offset_x = header.projection_offset_x;
    vertex->projection_offset_y = header.projection_offset_y;
    vertex->projection_plane = header.projection_plane;
    vertex->source_vertex_identity = 0;
    if (header.version >= 3) {
        vertex->projection_offset_x = i32(bytes + 40);
        vertex->projection_offset_y = i32(bytes + 44);
        vertex->projection_plane = u32(bytes + 48);
    }
    if (header.version >= 4)
        vertex->source_vertex_identity = u32(bytes + 52);
    vertex->transform_id = 0;
    for (int index = 0; index < 9; ++index)
        vertex->transform_rotation[index] = 0;
    for (auto& component : vertex->transform_translation)
        component = 0;
    vertex->exact_transform_valid = false;
    if (header.version >= 6) {
        vertex->transform_id = u64(bytes + 56);
        for (int index = 0; index < 9; ++index)
            vertex->transform_rotation[index] = i16(bytes + 64 + index * 2);
        for (int index = 0; index < 3; ++index) {
            vertex->transform_translation[index] =
                i32(bytes + 84 + index * 4);
        }
        vertex->exact_transform_valid =
            vertex->world_valid &&
            !vertex->screen_offset_anchor &&
            vertex->transform_id != 0;
    }
    if (vertex->world_valid)
        calculate_world(header, vertex);
    else
        vertex->world_x = vertex->world_y = vertex->world_z = 0.0F;
}

void parse_triangle(
    const std::uint8_t* bytes,
    const WorldCaptureHeader& header,
    WorldCaptureTriangle* triangle
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
    triangle->object_kind = u32(bytes + 32);
    triangle->object_id = u32(bytes + 36);
    triangle->model_pointer = u32(bytes + 40);
    triangle->draw_offset_x =
        static_cast<std::int16_t>(header.draw_offset_x);
    triangle->draw_offset_y =
        static_cast<std::int16_t>(header.draw_offset_y);
    if (header.version >= 3) {
        triangle->draw_offset_x = i16(bytes + 44);
        triangle->draw_offset_y = i16(bytes + 46);
    }
    triangle->transform_id = u64(bytes + 48);
    triangle->exact_transform_valid = header.version >= 5;
    if (triangle->exact_transform_valid) {
        for (int index = 0; index < 9; ++index)
            triangle->transform_rotation[index] =
                i16(bytes + 56 + index * 2);
        for (int index = 0; index < 3; ++index)
            triangle->transform_translation[index] =
                i32(bytes + 76 + index * 4);
    } else {
        for (int index = 0; index < 9; ++index)
            triangle->transform_rotation[index] =
                index == 0 || index == 4 || index == 8 ? 4096 : 0;
        for (auto& component : triangle->transform_translation)
            component = 0;
    }
    const std::size_t vertex_stride = header.version >= 6
        ? 96
        : header.version >= 4 ? 56 : header.version >= 3 ? 52 : 40;
    const std::size_t vertices_offset = header.version >= 5 ? 88 : 56;
    parse_vertex(
        bytes + vertices_offset,
        header,
        &triangle->vertices[0]);
    parse_vertex(
        bytes + vertices_offset + vertex_stride,
        header,
        &triangle->vertices[1]);
    parse_vertex(
        bytes + vertices_offset + vertex_stride * 2,
        header,
        &triangle->vertices[2]);
    if (header.version < 6) {
        for (auto& vertex : triangle->vertices) {
            vertex.transform_id = triangle->transform_id;
            for (int index = 0; index < 9; ++index) {
                vertex.transform_rotation[index] =
                    triangle->transform_rotation[index];
            }
            for (int index = 0; index < 3; ++index) {
                vertex.transform_translation[index] =
                    triangle->transform_translation[index];
            }
            vertex.exact_transform_valid =
                vertex.world_valid && triangle->exact_transform_valid;
        }
    }
}

} // namespace

WorldCaptureReadResult read_world_capture_header(
    const char* path,
    WorldCaptureHeader* header
) noexcept {
    if (path == nullptr || header == nullptr)
        return WorldCaptureReadResult::invalid_argument;
    std::FILE* file = std::fopen(path, "rb");
    if (file == nullptr)
        return WorldCaptureReadResult::open_failed;
    const auto result = read_header(file, header);
    std::fclose(file);
    return result;
}

WorldCaptureReadResult load_world_capture(
    const char* path,
    WorldCaptureHeader* header,
    WorldCaptureTriangle* triangles,
    std::size_t triangle_capacity,
    std::uint16_t* vram,
    std::size_t vram_word_capacity
) noexcept {
    if (path == nullptr || header == nullptr || triangles == nullptr ||
        vram == nullptr)
        return WorldCaptureReadResult::invalid_argument;
    std::FILE* file = std::fopen(path, "rb");
    if (file == nullptr)
        return WorldCaptureReadResult::open_failed;
    auto result = read_header(file, header);
    if (result != WorldCaptureReadResult::success) {
        std::fclose(file);
        return result;
    }
    constexpr std::size_t required_vram = 1024U * 512U;
    if (triangle_capacity < header->triangle_count ||
        vram_word_capacity < required_vram) {
        std::fclose(file);
        return WorldCaptureReadResult::capacity_too_small;
    }
    std::uint8_t record[world_capture_triangle_stride];
    for (std::uint32_t index = 0; index < header->triangle_count; ++index) {
        if (std::fread(
                record,
                1,
                header->triangle_stride,
                file) != header->triangle_stride) {
            std::fclose(file);
            return WorldCaptureReadResult::truncated;
        }
        parse_triangle(record, *header, &triangles[index]);
    }
    if (header->vram_offset >
            static_cast<std::uint64_t>(std::numeric_limits<long>::max()) ||
        std::fseek(
            file,
            static_cast<long>(header->vram_offset),
            SEEK_SET) != 0) {
        std::fclose(file);
        return WorldCaptureReadResult::io_error;
    }
    for (std::size_t index = 0; index < required_vram; ++index) {
        std::uint8_t bytes[2];
        if (std::fread(bytes, 1, 2, file) != 2) {
            std::fclose(file);
            return WorldCaptureReadResult::truncated;
        }
        vram[index] = u16(bytes);
    }
    std::fclose(file);
    return WorldCaptureReadResult::success;
}

WorldCaptureReadResult load_world_capture_memory(
    const std::uint8_t* bytes,
    std::size_t byte_count,
    WorldCaptureHeader* header,
    WorldCaptureTriangle* triangles,
    std::size_t triangle_capacity,
    std::uint16_t* vram,
    std::size_t vram_word_capacity
) noexcept {
    if (
        bytes == nullptr ||
        header == nullptr ||
        triangles == nullptr ||
        vram == nullptr
    )
        return WorldCaptureReadResult::invalid_argument;
    if (byte_count < world_capture_v1_header_size)
        return WorldCaptureReadResult::truncated;
    const std::uint32_t header_size = u32(bytes + 12);
    if (
        header_size != world_capture_v1_header_size &&
        header_size != world_capture_header_size
    )
        return WorldCaptureReadResult::invalid_layout;
    if (byte_count < header_size)
        return WorldCaptureReadResult::truncated;
    auto result = parse_header(bytes, header_size, header);
    if (result != WorldCaptureReadResult::success)
        return result;
    constexpr std::size_t required_vram = 1024U * 512U;
    if (
        triangle_capacity < header->triangle_count ||
        vram_word_capacity < required_vram
    )
        return WorldCaptureReadResult::capacity_too_small;
    if (
        header->vram_offset > byte_count ||
        header->vram_size > byte_count - header->vram_offset
    )
        return WorldCaptureReadResult::truncated;
    for (std::uint32_t index = 0;
         index < header->triangle_count;
         ++index) {
        const std::uint64_t offset =
            header->triangle_offset +
            static_cast<std::uint64_t>(index) *
            header->triangle_stride;
        if (
            offset > byte_count ||
            header->triangle_stride > byte_count - offset
        )
            return WorldCaptureReadResult::truncated;
        parse_triangle(
            bytes + static_cast<std::size_t>(offset),
            *header,
            &triangles[index]);
    }
    const auto* vram_bytes =
        bytes + static_cast<std::size_t>(header->vram_offset);
    for (std::size_t index = 0; index < required_vram; ++index)
        vram[index] = u16(vram_bytes + index * 2);
    return WorldCaptureReadResult::success;
}

const char* world_capture_read_result_name(
    WorldCaptureReadResult result
) noexcept {
    switch (result) {
        case WorldCaptureReadResult::success: return "success";
        case WorldCaptureReadResult::invalid_argument:
            return "invalid_argument";
        case WorldCaptureReadResult::open_failed: return "open_failed";
        case WorldCaptureReadResult::truncated: return "truncated";
        case WorldCaptureReadResult::invalid_magic: return "invalid_magic";
        case WorldCaptureReadResult::unsupported_version:
            return "unsupported_version";
        case WorldCaptureReadResult::invalid_layout:
            return "invalid_layout";
        case WorldCaptureReadResult::capacity_too_small:
            return "capacity_too_small";
        case WorldCaptureReadResult::io_error: return "io_error";
    }
    return "unknown";
}

} // namespace opengt::render
