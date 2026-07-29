#include "opengt/image_writer.hpp"

#include <algorithm>
#include <cstdio>
#include <cstring>
#include <vector>

namespace opengt::render {
namespace {

void write_u32_be(std::FILE* file, std::uint32_t value) {
    const std::uint8_t bytes[4] = {
        static_cast<std::uint8_t>(value >> 24),
        static_cast<std::uint8_t>(value >> 16),
        static_cast<std::uint8_t>(value >> 8),
        static_cast<std::uint8_t>(value),
    };
    std::fwrite(bytes, 1, 4, file);
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
                0xEDB88320U & (0U - (crc & 1U)));
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

bool write_chunk(
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

} // namespace

bool write_rgba_png(
    const char* path,
    const std::uint8_t* rgba,
    std::uint32_t width,
    std::uint32_t height
) noexcept {
    if (path == nullptr || rgba == nullptr || width == 0 || height == 0)
        return false;
    try {
        const std::size_t stride = static_cast<std::size_t>(width) * 4;
        std::vector<std::uint8_t> scanlines(
            (stride + 1) * static_cast<std::size_t>(height));
        for (std::uint32_t y = 0; y < height; ++y) {
            const std::size_t destination =
                static_cast<std::size_t>(y) * (stride + 1);
            scanlines[destination] = 0;
            std::memcpy(
                scanlines.data() + destination + 1,
                rgba + static_cast<std::size_t>(y) * stride,
                stride);
        }

        std::vector<std::uint8_t> zlib;
        zlib.reserve(
            scanlines.size() + scanlines.size() / 65535 * 5 + 8);
        zlib.push_back(0x78);
        zlib.push_back(0x01);
        std::size_t cursor = 0;
        while (cursor < scanlines.size()) {
            const std::size_t remaining = scanlines.size() - cursor;
            const std::uint16_t length =
                static_cast<std::uint16_t>(
                    std::min<std::size_t>(remaining, 65535));
            const bool final_block =
                cursor + length == scanlines.size();
            zlib.push_back(final_block ? 1 : 0);
            zlib.push_back(static_cast<std::uint8_t>(length));
            zlib.push_back(static_cast<std::uint8_t>(length >> 8));
            const std::uint16_t inverse =
                static_cast<std::uint16_t>(~length);
            zlib.push_back(static_cast<std::uint8_t>(inverse));
            zlib.push_back(static_cast<std::uint8_t>(inverse >> 8));
            zlib.insert(
                zlib.end(),
                scanlines.begin() +
                    static_cast<std::ptrdiff_t>(cursor),
                scanlines.begin() +
                    static_cast<std::ptrdiff_t>(cursor + length));
            cursor += length;
        }
        append_u32_be(
            zlib,
            adler32(scanlines.data(), scanlines.size()));

        std::FILE* file = std::fopen(path, "wb");
        if (file == nullptr)
            return false;
        const std::uint8_t signature[8] = {
            137, 80, 78, 71, 13, 10, 26, 10,
        };
        std::fwrite(signature, 1, sizeof(signature), file);
        std::vector<std::uint8_t> ihdr;
        append_u32_be(ihdr, width);
        append_u32_be(ihdr, height);
        ihdr.insert(ihdr.end(), {8, 6, 0, 0, 0});
        bool okay =
            write_chunk(
                file, "IHDR", ihdr.data(), ihdr.size()) &&
            write_chunk(
                file, "IDAT", zlib.data(), zlib.size()) &&
            write_chunk(file, "IEND", nullptr, 0);
        okay = okay && std::ferror(file) == 0;
        std::fclose(file);
        return okay;
    } catch (...) {
        return false;
    }
}

} // namespace opengt::render
