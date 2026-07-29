#include "opengt/projected_reference_renderer.hpp"

#include <algorithm>
#include <cmath>

namespace opengt::render {
namespace {

constexpr std::uint32_t textured_flag = 1U << 0;
constexpr std::uint32_t semi_transparent_flag = 1U << 1;
constexpr std::uint32_t raw_texture_flag = 1U << 2;
constexpr std::uint32_t set_mask_flag = 1U << 0;
constexpr std::uint32_t check_mask_flag = 1U << 1;
constexpr std::uint32_t dither_flag = 1U << 2;

struct Color {
    int r;
    int g;
    int b;
    bool mask;
};

float edge(
    const ProjectedCaptureVertex& a,
    const ProjectedCaptureVertex& b,
    float x,
    float y
) {
    return
        (x - a.x) * (b.y - a.y) -
        (y - a.y) * (b.x - a.x);
}

int expand5(int value) {
    return (value << 3) | (value >> 2);
}

Color decode_color(std::uint16_t word) {
    return Color{
        expand5(word & 31),
        expand5((word >> 5) & 31),
        expand5((word >> 10) & 31),
        (word & 0x8000U) != 0,
    };
}

std::uint16_t vram_at(
    const std::uint16_t* vram,
    int x,
    int y
) {
    x &= 1023;
    y &= 511;
    return vram[y * 1024 + x];
}

std::uint16_t texture_word(
    const ProjectedCaptureTriangle& triangle,
    const std::uint16_t* vram,
    int raw_u,
    int raw_v
) {
    int u =
        (raw_u & ~(triangle.texture_mask_x * 8)) |
        ((triangle.texture_offset_x & triangle.texture_mask_x) * 8);
    int v =
        (raw_v & ~(triangle.texture_mask_y * 8)) |
        ((triangle.texture_offset_y & triangle.texture_mask_y) * 8);
    u &= 255;
    v &= 255;

    const int page_x = (triangle.texture_page & 0x0FU) * 64;
    const int page_y = ((triangle.texture_page >> 4) & 1U) * 256;
    const int mode = (triangle.texture_page >> 7) & 3U;
    const int clut_x = (triangle.clut & 0x3FU) * 16;
    const int clut_y = (triangle.clut >> 6) & 0x1FFU;

    if (mode == 0) {
        const std::uint16_t packed =
            vram_at(vram, page_x + (u >> 2), page_y + v);
        const int index = (packed >> ((u & 3) * 4)) & 15;
        return vram_at(vram, clut_x + index, clut_y);
    }
    if (mode == 1) {
        const std::uint16_t packed =
            vram_at(vram, page_x + (u >> 1), page_y + v);
        const int index = (packed >> ((u & 1) * 8)) & 255;
        return vram_at(vram, clut_x + index, clut_y);
    }
    return vram_at(vram, page_x + u, page_y + v);
}

int quantize(int value, int x, int y, bool dither) {
    static constexpr int matrix[16] = {
        -4, 0, -3, 1,
         2, -2, 3, -1,
        -3, 1, -4, 0,
         3, -1, 2, -2,
    };
    if (dither)
        value += matrix[(y & 3) * 4 + (x & 3)];
    value = std::clamp(value, 0, 255);
    return expand5(std::min(value >> 3, 31));
}

void blend_pixel(
    std::uint8_t* pixel,
    const Color& source,
    bool blend,
    int blend_mode,
    bool set_mask
) {
    int r = source.r;
    int g = source.g;
    int b = source.b;
    if (blend) {
        const int dr = pixel[0];
        const int dg = pixel[1];
        const int db = pixel[2];
        switch (blend_mode) {
            case 0:
                r = (dr + r) / 2;
                g = (dg + g) / 2;
                b = (db + b) / 2;
                break;
            case 1:
                r = std::min(255, dr + r);
                g = std::min(255, dg + g);
                b = std::min(255, db + b);
                break;
            case 2:
                r = std::max(0, dr - r);
                g = std::max(0, dg - g);
                b = std::max(0, db - b);
                break;
            default:
                r = std::min(255, dr + r / 4);
                g = std::min(255, dg + g / 4);
                b = std::min(255, db + b / 4);
                break;
        }
    }
    pixel[0] = static_cast<std::uint8_t>(r);
    pixel[1] = static_cast<std::uint8_t>(g);
    pixel[2] = static_cast<std::uint8_t>(b);
    // Preserve the PS1 mask bit without making ordinary pixels transparent
    // when this buffer is saved as a conventional RGBA image.
    pixel[3] = (source.mask || set_mask) ? 255 : 254;
}

} // namespace

ProjectedRenderStats render_projected_capture(
    const ProjectedCaptureHeader& header,
    const ProjectedCaptureTriangle* triangles,
    std::size_t triangle_count,
    const std::uint16_t* vram,
    std::size_t vram_word_count,
    std::uint8_t* output_rgba,
    std::size_t output_size,
    ProjectedRenderOptions options
) noexcept {
    ProjectedRenderStats stats{};
    if (
        triangles == nullptr ||
        vram == nullptr ||
        output_rgba == nullptr ||
        header.display_width <= 0 ||
        header.display_height <= 0 ||
        vram_word_count < 1024U * 512U
    )
        return stats;

    const std::size_t required_output =
        static_cast<std::size_t>(header.display_width) *
        header.display_height * 4;
    if (output_size < required_output)
        return stats;

    const std::uint8_t clear_r =
        static_cast<std::uint8_t>(options.clear_color_rgba8);
    const std::uint8_t clear_g =
        static_cast<std::uint8_t>(options.clear_color_rgba8 >> 8);
    const std::uint8_t clear_b =
        static_cast<std::uint8_t>(options.clear_color_rgba8 >> 16);
    const std::uint8_t clear_a =
        static_cast<std::uint8_t>(options.clear_color_rgba8 >> 24);
    for (std::size_t index = 0; index < required_output; index += 4) {
        output_rgba[index] = clear_r;
        output_rgba[index + 1] = clear_g;
        output_rgba[index + 2] = clear_b;
        output_rgba[index + 3] = clear_a;
    }

    stats.submitted_triangles = static_cast<std::uint32_t>(
        std::min(
            triangle_count,
            static_cast<std::size_t>(0xFFFFFFFFU)
        )
    );

    const int display_x1 =
        header.display_x + header.display_width - 1;
    const int display_y1 =
        header.display_y + header.display_height - 1;

    for (std::size_t index = 0; index < triangle_count; ++index) {
        const ProjectedCaptureTriangle& triangle = triangles[index];
        const auto& a = triangle.vertices[0];
        const auto& b = triangle.vertices[1];
        const auto& c = triangle.vertices[2];
        const float area = edge(a, b, c.x, c.y);
        if (!std::isfinite(area) || std::fabs(area) < 0.0001F)
            continue;

        int min_x = static_cast<int>(
            std::floor(std::min({a.x, b.x, c.x}))
        );
        int max_x = static_cast<int>(
            std::ceil(std::max({a.x, b.x, c.x}))
        );
        int min_y = static_cast<int>(
            std::floor(std::min({a.y, b.y, c.y}))
        );
        int max_y = static_cast<int>(
            std::ceil(std::max({a.y, b.y, c.y}))
        );
        min_x = std::max({
            min_x,
            static_cast<int>(triangle.clip_x0),
            header.display_x
        });
        max_x = std::min({
            max_x,
            static_cast<int>(triangle.clip_x1),
            display_x1
        });
        min_y = std::max({
            min_y,
            static_cast<int>(triangle.clip_y0),
            header.display_y
        });
        max_y = std::min({
            max_y,
            static_cast<int>(triangle.clip_y1),
            display_y1
        });
        if (min_x > max_x || min_y > max_y)
            continue;

        ++stats.rasterized_triangles;
        const bool textured =
            (triangle.primitive_flags & textured_flag) != 0;
        const bool semitransparent =
            (triangle.primitive_flags & semi_transparent_flag) != 0;
        const bool raw_texture =
            (triangle.primitive_flags & raw_texture_flag) != 0;
        const bool set_mask =
            (triangle.environment_flags & set_mask_flag) != 0;
        const bool check_mask =
            (triangle.environment_flags & check_mask_flag) != 0;
        const bool dither =
            options.dithering &&
            (triangle.environment_flags & dither_flag) != 0 &&
            (!textured || !raw_texture);
        const int blend_mode = (triangle.texture_page >> 5) & 3;

        float minimum_z = std::min({a.z, b.z, c.z});
        float maximum_z = std::max({a.z, b.z, c.z});
        const bool perspective =
            options.perspective_correct_textures &&
            textured &&
            a.has_gte_depth &&
            b.has_gte_depth &&
            c.has_gte_depth &&
            minimum_z > 0.0F &&
            std::isfinite(maximum_z);
        if (perspective)
            ++stats.perspective_corrected_triangles;

        for (int y = min_y; y <= max_y; ++y) {
            for (int x = min_x; x <= max_x; ++x) {
                const float px = static_cast<float>(x) + 0.5F;
                const float py = static_cast<float>(y) + 0.5F;
                const float wa = edge(b, c, px, py) / area;
                const float wb = edge(c, a, px, py) / area;
                const float wc = edge(a, b, px, py) / area;
                if (wa < 0.0F || wb < 0.0F || wc < 0.0F)
                    continue;

                const std::size_t output_index =
                    (static_cast<std::size_t>(y - header.display_y) *
                        header.display_width +
                     static_cast<std::size_t>(x - header.display_x)) * 4;
                std::uint8_t* destination = output_rgba + output_index;
                if (check_mask && destination[3] == 255)
                    continue;

                int vertex_r = static_cast<int>(
                    wa * a.r + wb * b.r + wc * c.r + 0.5F
                );
                int vertex_g = static_cast<int>(
                    wa * a.g + wb * b.g + wc * c.g + 0.5F
                );
                int vertex_b = static_cast<int>(
                    wa * a.b + wb * b.b + wc * c.b + 0.5F
                );

                Color source{vertex_r, vertex_g, vertex_b, false};
                bool blend = semitransparent;
                if (textured) {
                    float u;
                    float v;
                    if (perspective) {
                        const float qa = 1.0F / std::max(1.0F, a.z);
                        const float qb = 1.0F / std::max(1.0F, b.z);
                        const float qc = 1.0F / std::max(1.0F, c.z);
                        const float denominator =
                            wa * qa + wb * qb + wc * qc;
                        u = (
                            wa * a.u * qa +
                            wb * b.u * qb +
                            wc * c.u * qc
                        ) / denominator;
                        v = (
                            wa * a.v * qa +
                            wb * b.v * qb +
                            wc * c.v * qc
                        ) / denominator;
                    } else {
                        u = wa * a.u + wb * b.u + wc * c.u;
                        v = wa * a.v + wb * b.v + wc * c.v;
                    }

                    const std::uint16_t texel = texture_word(
                        triangle,
                        vram,
                        static_cast<int>(std::floor(u + 0.0001F)),
                        static_cast<int>(std::floor(v + 0.0001F))
                    );
                    if (texel == 0) {
                        ++stats.transparent_pixels;
                        continue;
                    }
                    source = decode_color(texel);
                    if (!raw_texture) {
                        source.r = std::min(
                            255,
                            source.r * vertex_r / 128
                        );
                        source.g = std::min(
                            255,
                            source.g * vertex_g / 128
                        );
                        source.b = std::min(
                            255,
                            source.b * vertex_b / 128
                        );
                    }
                    blend = semitransparent && source.mask;
                }

                source.r = quantize(source.r, x, y, dither);
                source.g = quantize(source.g, x, y, dither);
                source.b = quantize(source.b, x, y, dither);
                blend_pixel(
                    destination,
                    source,
                    blend,
                    blend_mode,
                    set_mask
                );
                ++stats.shaded_pixels;
            }
        }
    }
    return stats;
}

} // namespace opengt::render
