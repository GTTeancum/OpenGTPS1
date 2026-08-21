#include "opengt/projected_capture.hpp"
#include "opengt/projected_reference_renderer.hpp"

#include <algorithm>
#include <array>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <vector>

namespace {

struct QoiPixel {
    std::uint8_t r;
    std::uint8_t g;
    std::uint8_t b;
    std::uint8_t a;
};

bool same(const QoiPixel& left, const QoiPixel& right) {
    return
        left.r == right.r &&
        left.g == right.g &&
        left.b == right.b &&
        left.a == right.a;
}

std::uint8_t hash(const QoiPixel& pixel) {
    return static_cast<std::uint8_t>(
        (pixel.r * 3 + pixel.g * 5 + pixel.b * 7 + pixel.a * 11) % 64
    );
}

void write_u32_be(std::FILE* file, std::uint32_t value) {
    const std::uint8_t bytes[4] = {
        static_cast<std::uint8_t>(value >> 24),
        static_cast<std::uint8_t>(value >> 16),
        static_cast<std::uint8_t>(value >> 8),
        static_cast<std::uint8_t>(value),
    };
    std::fwrite(bytes, 1, 4, file);
}

bool write_qoi(
    const char* path,
    const std::uint8_t* rgba,
    std::uint32_t width,
    std::uint32_t height
) {
    std::FILE* file = std::fopen(path, "wb");
    if (file == nullptr)
        return false;

    std::fwrite("qoif", 1, 4, file);
    write_u32_be(file, width);
    write_u32_be(file, height);
    std::fputc(4, file);
    std::fputc(0, file);

    std::array<QoiPixel, 64> index{};
    QoiPixel previous{0, 0, 0, 255};
    int run = 0;
    const std::size_t count =
        static_cast<std::size_t>(width) * height;

    for (std::size_t i = 0; i < count; ++i) {
        const QoiPixel pixel{
            rgba[i * 4],
            rgba[i * 4 + 1],
            rgba[i * 4 + 2],
            rgba[i * 4 + 3],
        };
        if (same(pixel, previous)) {
            ++run;
            if (run == 62 || i + 1 == count) {
                std::fputc(0xC0 | (run - 1), file);
                run = 0;
            }
            continue;
        }
        if (run > 0) {
            std::fputc(0xC0 | (run - 1), file);
            run = 0;
        }

        const std::uint8_t slot = hash(pixel);
        if (same(index[slot], pixel)) {
            std::fputc(slot, file);
        } else {
            index[slot] = pixel;
            if (pixel.a == previous.a) {
                const int dr = pixel.r - previous.r;
                const int dg = pixel.g - previous.g;
                const int db = pixel.b - previous.b;
                if (
                    dr >= -2 && dr <= 1 &&
                    dg >= -2 && dg <= 1 &&
                    db >= -2 && db <= 1
                ) {
                    std::fputc(
                        0x40 |
                        ((dr + 2) << 4) |
                        ((dg + 2) << 2) |
                        (db + 2),
                        file
                    );
                } else {
                    const int dr_dg = dr - dg;
                    const int db_dg = db - dg;
                    if (
                        dg >= -32 && dg <= 31 &&
                        dr_dg >= -8 && dr_dg <= 7 &&
                        db_dg >= -8 && db_dg <= 7
                    ) {
                        std::fputc(0x80 | (dg + 32), file);
                        std::fputc(
                            ((dr_dg + 8) << 4) | (db_dg + 8),
                            file
                        );
                    } else {
                        std::fputc(0xFE, file);
                        std::fputc(pixel.r, file);
                        std::fputc(pixel.g, file);
                        std::fputc(pixel.b, file);
                    }
                }
            } else {
                std::fputc(0xFF, file);
                std::fputc(pixel.r, file);
                std::fputc(pixel.g, file);
                std::fputc(pixel.b, file);
                std::fputc(pixel.a, file);
            }
        }
        previous = pixel;
    }
    const std::uint8_t end[8] = {0, 0, 0, 0, 0, 0, 0, 1};
    std::fwrite(end, 1, sizeof(end), file);
    const bool okay = std::ferror(file) == 0;
    std::fclose(file);
    return okay;
}

std::uint32_t crc32(
    const std::uint8_t* data,
    std::size_t size,
    std::uint32_t crc = 0
) {
    crc ^= 0xFFFFFFFFU;
    for (std::size_t index = 0; index < size; ++index) {
        crc ^= data[index];
        for (int bit = 0; bit < 8; ++bit)
            crc = (crc >> 1) ^ (
                0xEDB88320U & (0U - (crc & 1U))
            );
    }
    return crc ^ 0xFFFFFFFFU;
}

std::uint32_t adler32(
    const std::uint8_t* data,
    std::size_t size
) {
    std::uint32_t a = 1;
    std::uint32_t b = 0;
    for (std::size_t index = 0; index < size; ++index) {
        a = (a + data[index]) % 65521U;
        b = (b + a) % 65521U;
    }
    return (b << 16) | a;
}

void append_u32_be(
    std::vector<std::uint8_t>& output,
    std::uint32_t value
) {
    output.push_back(static_cast<std::uint8_t>(value >> 24));
    output.push_back(static_cast<std::uint8_t>(value >> 16));
    output.push_back(static_cast<std::uint8_t>(value >> 8));
    output.push_back(static_cast<std::uint8_t>(value));
}

bool write_png_chunk(
    std::FILE* file,
    const char type[4],
    const std::uint8_t* payload,
    std::size_t size
) {
    if (size > 0xFFFFFFFFU)
        return false;
    write_u32_be(file, static_cast<std::uint32_t>(size));
    std::fwrite(type, 1, 4, file);
    if (size != 0)
        std::fwrite(payload, 1, size, file);
    std::vector<std::uint8_t> crc_input;
    crc_input.reserve(4 + size);
    crc_input.insert(crc_input.end(), type, type + 4);
    if (size != 0)
        crc_input.insert(crc_input.end(), payload, payload + size);
    write_u32_be(file, crc32(crc_input.data(), crc_input.size()));
    return std::ferror(file) == 0;
}

bool write_png(
    const char* path,
    const std::uint8_t* rgba,
    std::uint32_t width,
    std::uint32_t height
) {
    const std::size_t stride = static_cast<std::size_t>(width) * 4;
    std::vector<std::uint8_t> scanlines(
        (stride + 1) * static_cast<std::size_t>(height)
    );
    for (std::uint32_t y = 0; y < height; ++y) {
        const std::size_t destination =
            static_cast<std::size_t>(y) * (stride + 1);
        scanlines[destination] = 0;
        std::memcpy(
            scanlines.data() + destination + 1,
            rgba + static_cast<std::size_t>(y) * stride,
            stride
        );
    }

    // A standards-compliant zlib stream using bounded DEFLATE stored blocks.
    // QOI remains the compact native output; PNG exists for ubiquitous review.
    std::vector<std::uint8_t> zlib;
    zlib.reserve(scanlines.size() + scanlines.size() / 65535 * 5 + 8);
    zlib.push_back(0x78);
    zlib.push_back(0x01);
    std::size_t cursor = 0;
    while (cursor < scanlines.size()) {
        const std::size_t remaining = scanlines.size() - cursor;
        const std::uint16_t length = static_cast<std::uint16_t>(
            std::min<std::size_t>(remaining, 65535)
        );
        const bool final_block = cursor + length == scanlines.size();
        zlib.push_back(final_block ? 1 : 0);
        zlib.push_back(static_cast<std::uint8_t>(length));
        zlib.push_back(static_cast<std::uint8_t>(length >> 8));
        const std::uint16_t inverse =
            static_cast<std::uint16_t>(~length);
        zlib.push_back(static_cast<std::uint8_t>(inverse));
        zlib.push_back(static_cast<std::uint8_t>(inverse >> 8));
        zlib.insert(
            zlib.end(),
            scanlines.begin() + static_cast<std::ptrdiff_t>(cursor),
            scanlines.begin() +
                static_cast<std::ptrdiff_t>(cursor + length)
        );
        cursor += length;
    }
    append_u32_be(zlib, adler32(scanlines.data(), scanlines.size()));

    std::FILE* file = std::fopen(path, "wb");
    if (file == nullptr)
        return false;
    const std::uint8_t signature[8] = {
        137, 80, 78, 71, 13, 10, 26, 10
    };
    std::fwrite(signature, 1, sizeof(signature), file);
    std::vector<std::uint8_t> ihdr;
    append_u32_be(ihdr, width);
    append_u32_be(ihdr, height);
    ihdr.insert(ihdr.end(), {8, 6, 0, 0, 0});
    bool okay =
        write_png_chunk(file, "IHDR", ihdr.data(), ihdr.size()) &&
        write_png_chunk(file, "IDAT", zlib.data(), zlib.size()) &&
        write_png_chunk(file, "IEND", nullptr, 0);
    okay = okay && std::ferror(file) == 0;
    std::fclose(file);
    return okay;
}

bool ends_with(const char* value, const char* suffix) {
    const std::size_t value_length = std::strlen(value);
    const std::size_t suffix_length = std::strlen(suffix);
    return
        value_length >= suffix_length &&
        std::strcmp(
            value + value_length - suffix_length,
            suffix
        ) == 0;
}

std::uint64_t fnv1a64(
    const std::uint8_t* data,
    std::size_t size
) {
    std::uint64_t value = 14695981039346656037ULL;
    for (std::size_t index = 0; index < size; ++index) {
        value ^= data[index];
        value *= 1099511628211ULL;
    }
    return value;
}

void copy_vram_reference(
    const opengt::render::ProjectedCaptureHeader& header,
    const std::uint16_t* vram,
    std::uint8_t* output
) {
    for (int y = 0; y < header.display_height; ++y) {
        for (int x = 0; x < header.display_width; ++x) {
            const int source_x = (header.display_x + x) & 1023;
            const int source_y = (header.display_y + y) & 511;
            const std::uint16_t word = vram[source_y * 1024 + source_x];
            const std::size_t destination =
                (static_cast<std::size_t>(y) * header.display_width + x) * 4;
            output[destination] = static_cast<std::uint8_t>(
                ((word & 31U) << 3) | ((word & 31U) >> 2)
            );
            output[destination + 1] = static_cast<std::uint8_t>(
                (((word >> 5) & 31U) << 3) |
                (((word >> 5) & 31U) >> 2)
            );
            output[destination + 2] = static_cast<std::uint8_t>(
                (((word >> 10) & 31U) << 3) |
                (((word >> 10) & 31U) >> 2)
            );
            output[destination + 3] = 255;
        }
    }
}

} // namespace

int main(int argc, char** argv) {
    if (argc < 3 || argc > 4) {
        std::fprintf(
            stderr,
            "usage: opengt_capture_render <capture.ogtcap> "
            "<output.qoi|output.png> "
            "[--affine|--vram]\n"
        );
        return 2;
    }
    const bool affine =
        argc == 4 && std::strcmp(argv[3], "--affine") == 0;
    const bool vram_reference =
        argc == 4 && std::strcmp(argv[3], "--vram") == 0;
    if (argc == 4 && !affine && !vram_reference) {
        std::fprintf(stderr, "unknown render mode: %s\n", argv[3]);
        return 2;
    }
    const bool perspective = !affine && !vram_reference;

    opengt::render::ProjectedCaptureHeader header{};
    auto result = opengt::render::read_projected_capture_header(
        argv[1],
        &header
    );
    if (result != opengt::render::CaptureReadResult::success) {
        std::fprintf(
            stderr,
            "capture header failed: %s\n",
            opengt::render::capture_read_result_name(result)
        );
        return 1;
    }

    std::vector<opengt::render::ProjectedCaptureTriangle> triangles(
        header.triangle_count
    );
    std::vector<std::uint16_t> vram(
        static_cast<std::size_t>(header.vram_width) * header.vram_height
    );
    result = opengt::render::load_projected_capture(
        argv[1],
        &header,
        triangles.data(),
        triangles.size(),
        vram.data(),
        vram.size()
    );
    if (result != opengt::render::CaptureReadResult::success) {
        std::fprintf(
            stderr,
            "capture load failed: %s\n",
            opengt::render::capture_read_result_name(result)
        );
        return 1;
    }

    std::vector<std::uint8_t> output(
        static_cast<std::size_t>(header.display_width) *
        header.display_height * 4
    );
    opengt::render::ProjectedRenderStats stats{};
    if (vram_reference) {
        if ((header.flags & (1U << 1)) != 0) {
            std::fprintf(
                stderr,
                "24-bit VRAM reference extraction is not supported\n"
            );
            return 1;
        }
        copy_vram_reference(header, vram.data(), output.data());
    } else {
        stats = opengt::render::render_projected_capture(
            header,
            triangles.data(),
            triangles.size(),
            vram.data(),
            vram.size(),
            output.data(),
            output.size(),
            opengt::render::ProjectedRenderOptions{
                perspective,
                true,
                0xFE000000U,
            }
        );
    }
    const bool image_written = ends_with(argv[2], ".png")
        ? write_png(
            argv[2],
            output.data(),
            static_cast<std::uint32_t>(header.display_width),
            static_cast<std::uint32_t>(header.display_height))
        : write_qoi(
            argv[2],
            output.data(),
            static_cast<std::uint32_t>(header.display_width),
            static_cast<std::uint32_t>(header.display_height));
    if (!image_written) {
        std::fprintf(stderr, "failed to write %s\n", argv[2]);
        return 1;
    }

    std::printf(
        "frame=%llu poll=%d display=%dx%d triangles=%u "
        "rasterized=%u perspectiveTriangles=%u pixels=%llu "
        "transparent=%llu projection=%s rgbaHash=%016llx output=%s\n",
        static_cast<unsigned long long>(header.frame_index),
        header.input_poll,
        header.display_width,
        header.display_height,
        vram_reference ? header.triangle_count : stats.submitted_triangles,
        stats.rasterized_triangles,
        stats.perspective_corrected_triangles,
        static_cast<unsigned long long>(stats.shaded_pixels),
        static_cast<unsigned long long>(stats.transparent_pixels),
        vram_reference
            ? "vram-reference"
            : perspective ? "perspective" : "affine",
        static_cast<unsigned long long>(
            fnv1a64(output.data(), output.size())
        ),
        argv[2]
    );
    return 0;
}
