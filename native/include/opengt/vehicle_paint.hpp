#pragma once

#include "opengt/world_draw_list.hpp"
#include <array>
#include <string>
#include <unordered_map>
#include <vector>

namespace opengt::render {

using PaintFaceKey = std::array<std::int16_t, 9>;
struct PaintFaceHash {
    std::size_t operator()(const PaintFaceKey& key) const noexcept;
};
struct PaintCorner {
    std::int16_t position[3]{};
    float normal[3]{};
    float uv[2]{};
};
struct PaintFace { PaintCorner corners[3]; };
struct VehiclePaintPack {
    std::uint64_t bitmap_key{};
    std::uint64_t alternate_bitmap_key{};
    std::uint32_t width{}, height{};
    std::vector<std::uint8_t> mask;
    std::unordered_map<PaintFaceKey, PaintFace, PaintFaceHash> faces;
};
// Matches the optional vertex-shader structured buffer, not the guest ABI.
struct PaintGpuVertex {
    float normal[3]{}, mode{};
    float view_position[3]{}, intensity{};
    float uv[2]{}, padding[2]{};
    float light_direction[3]{}, padding2{};
};
static_assert(sizeof(PaintGpuVertex) == 64);
struct VehiclePaintFrame {
    std::vector<std::uint32_t> vertex_indices;
    std::vector<PaintGpuVertex> vertices;
    std::uint32_t matched_vehicles{}, reflection_triangles{};
};

PaintFaceKey paint_face_key(const PaintFace& face) noexcept;
bool load_vehicle_paint_pack(const char* path, VehiclePaintPack* pack,
    std::string* error) noexcept;
std::uint64_t vehicle_paint_bitmap_key(const std::uint16_t* vram,
    std::uint16_t page, std::uint32_t width, std::uint32_t height) noexcept;
VehiclePaintFrame build_vehicle_paint_frame(const WorldDrawList& list,
    const std::uint16_t* vram, const VehiclePaintPack& pack);

} // namespace opengt::render
