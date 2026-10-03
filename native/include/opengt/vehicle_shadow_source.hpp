#pragma once
#include "opengt/world_draw_list.hpp"
#include <cstdint>
#include <vector>

namespace opengt::render {
constexpr std::uint64_t vehicle_shadow_magic = 0x313052414354474FULL;
struct VehicleShadowFace {
    WorldMaterial material{};
    struct Corner { std::int16_t x{}, y{}, z{}; std::uint8_t u{}, v{}; } vertices[3];
};
struct VehicleShadowSource {
    std::uint32_t model{}, lod{}, object{}, generation{};
    std::int32_t poll{};
    std::vector<VehicleShadowFace> faces;
};
bool parse_vehicle_shadow_source(const std::uint8_t*, std::size_t, VehicleShadowSource*) noexcept;
struct VehicleShadowStats {std::uint32_t definitions{}, matched{}, missing{}, added{};};
// No cached transforms. Every source mesh must match multiple original triangles
// in THIS authored frame, with the same exact body pose and source material.
VehicleShadowStats append_vehicle_shadow_sources(WorldDrawList&, const std::vector<VehicleShadowSource>&);
}
