#pragma once

#include <cstdint>

namespace opengt::render {

bool write_rgba_png(
    const char* path,
    const std::uint8_t* rgba,
    std::uint32_t width,
    std::uint32_t height
) noexcept;

} // namespace opengt::render
