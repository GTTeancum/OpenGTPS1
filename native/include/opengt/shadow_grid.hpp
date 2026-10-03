#pragma once
#include "opengt/lighting.hpp"

namespace opengt::render::lighting {
// The primary GTE camera contains quantized scale/shear as well as rotation.
// These are affine DUAL axes, not normalized camera direction vectors. Double
// precision is used only on CPU so large course origins cancel before upload.
struct ShadowCameraTransform {
    bool valid{};
    double inverse[3][3]{}; // canonical/script world vector from common view
    double origin[3]{};    // canonical/script world camera origin
    Vec3 vector_to_world(Vec3) const noexcept;
    Vec3 world_axis_in_view(Vec3) const noexcept;
    double origin_on_axis(Vec3) const noexcept;
};
ShadowCameraTransform shadow_camera_transform(const WorldDrawList&) noexcept;
// Stable XY rows in a camera-relative shader coordinate system. Returns a
// finite affine row; only the world texel phase is rounded, never vertices.
std::array<float,4> snapped_shadow_row(const ShadowCameraTransform&, Vec3 world_axis,
    Vec3 view_center, float radius, unsigned resolution) noexcept;
} // namespace opengt::render::lighting
