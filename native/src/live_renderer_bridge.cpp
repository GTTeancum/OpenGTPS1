#include "opengt/live_renderer_bridge.h"

#include "opengt/world_capture.hpp"
#include "opengt/world_draw_list.hpp"
#include "opengt/world_gpu_renderer.hpp"
#if defined(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT)
#include "opengt/world_interpolation.hpp"
#endif
#include "opengt/world_topology.hpp"

#include <algorithm>
#include <chrono>
#include <array>
#include <cmath>
#include <cstdio>
#include <cstdlib>
#include <cstdint>
#include <cstring>
#include <new>
#include <numeric>
#include <unordered_map>
#include <utility>
#include <vector>

namespace {

// Version 11 adds same-device D3D11 texture output. Keep this version
// synchronized with LiveWorldRenderer so a stale DLL cannot reinterpret the
// extended options/stats layouts.
constexpr std::uint32_t api_version = 11;
constexpr std::uint64_t resident_mesh_magic = 0x314853454D54474FULL;
constexpr std::uint32_t resident_mesh_version = 2;
constexpr std::size_t resident_mesh_header_size = 32;
constexpr std::size_t resident_vertex_stride = 12;
constexpr std::size_t resident_primitive_stride = 80;
constexpr std::size_t resident_instance_stride = 104;
constexpr std::uint32_t resident_primitive_quad = 1U << 0;
constexpr std::uint32_t resident_primitive_one_sided = 1U << 1;
constexpr std::uint32_t resident_primitive_textured = 1U << 2;
constexpr std::uint32_t resident_primitive_semi_transparent = 1U << 3;
constexpr std::uint32_t resident_primitive_raw_texture = 1U << 4;
constexpr std::uint32_t resident_primitive_gouraud = 1U << 5;
constexpr std::uint32_t resident_primitive_primary_path = 1U << 6;
constexpr std::uint32_t resident_primitive_local_coordinates = 1U << 7;

constexpr std::array<int, 3> resident_quad_packet_corners(
    bool primary,
    int triangle
) noexcept {
    if (primary)
        return triangle == 0
            ? std::array<int, 3>{3, 1, 0}
            : std::array<int, 3>{1, 2, 3};
    return triangle == 0
        ? std::array<int, 3>{0, 1, 3}
        : std::array<int, 3>{1, 3, 2};
}

static_assert(resident_quad_packet_corners(true, 0)[0] == 3);
static_assert(resident_quad_packet_corners(true, 0)[1] == 1);
static_assert(resident_quad_packet_corners(true, 0)[2] == 0);
static_assert(resident_quad_packet_corners(true, 1)[0] == 1);
static_assert(resident_quad_packet_corners(true, 1)[1] == 2);
static_assert(resident_quad_packet_corners(true, 1)[2] == 3);
static_assert(resident_quad_packet_corners(false, 0)[0] == 0);
static_assert(resident_quad_packet_corners(false, 0)[1] == 1);
static_assert(resident_quad_packet_corners(false, 0)[2] == 3);
static_assert(resident_quad_packet_corners(false, 1)[0] == 1);
static_assert(resident_quad_packet_corners(false, 1)[1] == 3);
static_assert(resident_quad_packet_corners(false, 1)[2] == 2);
static_assert(sizeof(opengt_live_options) == 28);
static_assert(sizeof(opengt_live_stats) == 136);
#if defined(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT)
static_assert(sizeof(opengt_live_interpolation_stats) == 128);
#endif
using Clock = std::chrono::steady_clock;

bool output_fingerprint_enabled() noexcept {
    static const bool enabled = [] {
        const char* value = std::getenv(
            "RECOMPONE_AUDIT_NATIVE_WORLD_OUTPUT_HASH");
        return value != nullptr && std::strcmp(value, "1") == 0;
    }();
    return enabled;
}

bool world_fingerprint_enabled() noexcept {
    static const bool enabled = [] {
        const char* value = std::getenv(
            "RECOMPONE_AUDIT_NATIVE_WORLD_HASH");
        return value != nullptr && std::strcmp(value, "1") == 0;
    }();
    return enabled;
}

bool resident_equivalence_audit_enabled() noexcept {
    static const bool enabled = [] {
        const char* value = std::getenv(
            "RECOMPONE_AUDIT_NATIVE_RESIDENT_EQUIVALENCE");
        return value != nullptr && std::strcmp(value, "1") == 0;
    }();
    return enabled;
}

bool resident_lod_diagnostics_enabled() noexcept {
    static const bool enabled = [] {
        const char* value = std::getenv(
            "OPENGT_RENDER_RESIDENT_LOD_DIAGNOSTICS");
        return value != nullptr && std::strcmp(value, "1") == 0;
    }();
    return enabled;
}

std::uint64_t fingerprint_output(
    const std::uint8_t* bytes,
    std::size_t size
) noexcept {
    constexpr std::uint64_t offset = 14695981039346656037ULL;
    constexpr std::uint64_t prime = 1099511628211ULL;
    std::uint64_t hash = offset;
    for (std::size_t index = 0; index < size; ++index) {
        hash ^= bytes[index];
        hash *= prime;
    }
    return hash == 0 ? 1ULL : hash;
}

void fingerprint_add(
    std::uint64_t* hash,
    std::uint64_t value,
    int bytes = 8
) noexcept {
    constexpr std::uint64_t prime = 1099511628211ULL;
    for (int index = 0; index < bytes; ++index) {
        *hash ^= static_cast<std::uint8_t>(value >> (index * 8));
        *hash *= prime;
    }
}

void fingerprint_add_float(
    std::uint64_t* hash,
    float value
) noexcept {
    std::uint32_t bits{};
    static_assert(sizeof(bits) == sizeof(value));
    std::memcpy(&bits, &value, sizeof(bits));
    fingerprint_add(hash, bits, 4);
}

void fingerprint_add_material(
    std::uint64_t* hash,
    const opengt::render::WorldMaterial& material
) noexcept {
    fingerprint_add(hash, material.primitive_flags, 4);
    fingerprint_add(hash, material.texture_page, 2);
    fingerprint_add(hash, material.clut, 2);
    fingerprint_add(
        hash, static_cast<std::uint16_t>(material.texture_mask_x), 2);
    fingerprint_add(
        hash, static_cast<std::uint16_t>(material.texture_mask_y), 2);
    fingerprint_add(
        hash, static_cast<std::uint16_t>(material.texture_offset_x), 2);
    fingerprint_add(
        hash, static_cast<std::uint16_t>(material.texture_offset_y), 2);
    fingerprint_add(hash, material.environment_flags, 4);
}

void fingerprint_add_vertex(
    std::uint64_t* hash,
    const opengt::render::WorldDrawVertex& vertex
) noexcept {
    fingerprint_add_float(hash, vertex.world_x);
    fingerprint_add_float(hash, vertex.world_y);
    fingerprint_add_float(hash, vertex.world_z);
    fingerprint_add_float(hash, vertex.view_x);
    fingerprint_add_float(hash, vertex.view_y);
    fingerprint_add_float(hash, vertex.view_z);
    fingerprint_add_float(hash, vertex.clip_x);
    fingerprint_add_float(hash, vertex.clip_y);
    fingerprint_add_float(hash, vertex.clip_z);
    fingerprint_add_float(hash, vertex.clip_w);
    fingerprint_add_float(hash, vertex.screen_x);
    fingerprint_add_float(hash, vertex.screen_y);
    fingerprint_add_float(hash, vertex.projection_offset_x);
    fingerprint_add_float(hash, vertex.projection_offset_y);
    fingerprint_add_float(hash, vertex.projection_plane);
    fingerprint_add_float(hash, vertex.draw_offset_x);
    fingerprint_add_float(hash, vertex.draw_offset_y);
    fingerprint_add_float(hash, vertex.u);
    fingerprint_add_float(hash, vertex.v);
    fingerprint_add(hash, vertex.r, 1);
    fingerprint_add(hash, vertex.g, 1);
    fingerprint_add(hash, vertex.b, 1);
    fingerprint_add(hash, static_cast<std::uint16_t>(vertex.model_x), 2);
    fingerprint_add(hash, static_cast<std::uint16_t>(vertex.model_y), 2);
    fingerprint_add(hash, static_cast<std::uint16_t>(vertex.model_z), 2);
    fingerprint_add(hash, vertex.provenance_flags, 2);
    fingerprint_add(hash, vertex.source_vertex_identity, 4);
    fingerprint_add(
        hash, static_cast<std::uint32_t>(vertex.exact_view_x), 4);
    fingerprint_add(
        hash, static_cast<std::uint32_t>(vertex.exact_view_y), 4);
    fingerprint_add(
        hash, static_cast<std::uint32_t>(vertex.exact_view_z), 4);
    fingerprint_add(hash, vertex.transform_id);
    for (int index = 0; index < 9; ++index) {
        fingerprint_add(
            hash,
            static_cast<std::uint16_t>(vertex.transform_rotation[index]),
            2);
    }
    for (int index = 0; index < 3; ++index) {
        fingerprint_add(
            hash,
            static_cast<std::uint32_t>(vertex.transform_translation[index]),
            4);
    }
    fingerprint_add(hash, vertex.exact_transform_valid ? 1U : 0U, 1);
    fingerprint_add(
        hash, static_cast<std::uint32_t>(vertex.authored_screen_x), 4);
    fingerprint_add(
        hash, static_cast<std::uint32_t>(vertex.authored_screen_y), 4);
}

std::uint64_t fingerprint_world_draw_list(
    const opengt::render::WorldDrawList& draw_list
) noexcept {
    constexpr std::uint64_t offset = 14695981039346656037ULL;
    std::uint64_t hash = offset;
    fingerprint_add(&hash, draw_list.materials.size());
    for (const auto& material : draw_list.materials)
        fingerprint_add_material(&hash, material);
    fingerprint_add(&hash, draw_list.commands.size());
    for (const auto& command : draw_list.commands) {
        for (const auto& vertex : command.vertices)
            fingerprint_add_vertex(&hash, vertex);
        fingerprint_add_float(&hash, command.face_normal_x);
        fingerprint_add_float(&hash, command.face_normal_y);
        fingerprint_add_float(&hash, command.face_normal_z);
        fingerprint_add(&hash, command.material_index, 4);
        fingerprint_add(
            &hash,
            static_cast<std::uint32_t>(command.ordering_table_index),
            4);
        fingerprint_add(
            &hash, static_cast<std::uint16_t>(command.clip_x0), 2);
        fingerprint_add(
            &hash, static_cast<std::uint16_t>(command.clip_y0), 2);
        fingerprint_add(
            &hash, static_cast<std::uint16_t>(command.clip_x1), 2);
        fingerprint_add(
            &hash, static_cast<std::uint16_t>(command.clip_y1), 2);
        fingerprint_add(&hash, command.object_kind, 4);
        fingerprint_add(&hash, command.object_id, 4);
        fingerprint_add(&hash, command.model_pointer, 4);
        fingerprint_add(&hash, command.transform_id);
        for (int index = 0; index < 9; ++index) {
            fingerprint_add(
                &hash,
                static_cast<std::uint16_t>(
                    command.transform_rotation[index]),
                2);
        }
        for (int index = 0; index < 3; ++index) {
            fingerprint_add(
                &hash,
                static_cast<std::uint32_t>(
                    command.transform_translation[index]),
                4);
        }
        fingerprint_add(
            &hash, command.exact_transform_valid ? 1U : 0U, 1);
        fingerprint_add(
            &hash, static_cast<std::uint32_t>(command.channel), 1);
    }
    return hash == 0 ? 1ULL : hash;
}

std::uint64_t fingerprint_track_triangle(
    const opengt::render::WorldCaptureTriangle& triangle,
    bool include_material = true,
    bool include_world = true
) noexcept {
    constexpr std::uint64_t offset = 14695981039346656037ULL;
    std::uint64_t hash = offset;
    if (include_material) {
        // Resident decoding adds renderer-only relationship annotations after
        // reproducing the guest packet. They intentionally differ from the
        // managed diagnostic expansion and are not PS1 material state. Keep
        // every captured material bit in the equivalence oracle while
        // excluding only those typed road-overlay annotations.
        constexpr std::uint32_t resident_annotation_mask =
            opengt::render::world_primitive_track_overlay_layer_mask |
            opengt::render::world_primitive_track_overlay_support_flag |
            opengt::render::world_primitive_track_replacement_flag;
        fingerprint_add(
            &hash,
            triangle.primitive_flags & ~resident_annotation_mask,
            4);
        fingerprint_add(&hash, triangle.texture_page, 2);
        fingerprint_add(&hash, triangle.clut, 2);
        fingerprint_add(
            &hash,
            static_cast<std::uint32_t>(triangle.ordering_table_index),
            4);
        fingerprint_add(
            &hash, static_cast<std::uint16_t>(triangle.clip_x0), 2);
        fingerprint_add(
            &hash, static_cast<std::uint16_t>(triangle.clip_y0), 2);
        fingerprint_add(
            &hash, static_cast<std::uint16_t>(triangle.clip_x1), 2);
        fingerprint_add(
            &hash, static_cast<std::uint16_t>(triangle.clip_y1), 2);
        fingerprint_add(
            &hash, static_cast<std::uint16_t>(triangle.texture_mask_x), 2);
        fingerprint_add(
            &hash, static_cast<std::uint16_t>(triangle.texture_mask_y), 2);
        fingerprint_add(
            &hash, static_cast<std::uint16_t>(triangle.texture_offset_x), 2);
        fingerprint_add(
            &hash, static_cast<std::uint16_t>(triangle.texture_offset_y), 2);
        fingerprint_add(&hash, triangle.environment_flags, 4);
    }
    fingerprint_add(&hash, triangle.object_kind, 4);
    fingerprint_add(&hash, triangle.object_id, 4);
    fingerprint_add(&hash, triangle.model_pointer, 4);
    fingerprint_add(
        &hash, static_cast<std::uint16_t>(triangle.draw_offset_x), 2);
    fingerprint_add(
        &hash, static_cast<std::uint16_t>(triangle.draw_offset_y), 2);
    fingerprint_add(&hash, triangle.transform_id);
    for (int index = 0; index < 9; ++index) {
        fingerprint_add(
            &hash,
            static_cast<std::uint16_t>(triangle.transform_rotation[index]),
            2);
    }
    for (int index = 0; index < 3; ++index) {
        fingerprint_add(
            &hash,
            static_cast<std::uint32_t>(triangle.transform_translation[index]),
            4);
    }
    fingerprint_add(&hash, triangle.exact_transform_valid ? 1U : 0U, 1);
    fingerprint_add(
        &hash,
        static_cast<std::uint32_t>(triangle.depth_scale_exponent),
        4);
    fingerprint_add(&hash, triangle.depth_scale_valid ? 1U : 0U, 1);
    for (const auto& vertex : triangle.vertices) {
        if (include_material) {
            fingerprint_add(
                &hash, static_cast<std::uint16_t>(vertex.u), 2);
            fingerprint_add(
                &hash, static_cast<std::uint16_t>(vertex.v), 2);
            fingerprint_add(&hash, vertex.r, 1);
            fingerprint_add(&hash, vertex.g, 1);
            fingerprint_add(&hash, vertex.b, 1);
        }
        fingerprint_add(&hash, vertex.world_valid ? 1U : 0U, 1);
        fingerprint_add(
            &hash, vertex.screen_offset_anchor ? 1U : 0U, 1);
        fingerprint_add(
            &hash, static_cast<std::uint16_t>(vertex.model_x), 2);
        fingerprint_add(
            &hash, static_cast<std::uint16_t>(vertex.model_y), 2);
        fingerprint_add(
            &hash, static_cast<std::uint16_t>(vertex.model_z), 2);
        fingerprint_add(
            &hash, static_cast<std::uint32_t>(vertex.view_x), 4);
        fingerprint_add(
            &hash, static_cast<std::uint32_t>(vertex.view_y), 4);
        fingerprint_add(
            &hash, static_cast<std::uint32_t>(vertex.view_z), 4);
        fingerprint_add(
            &hash,
            static_cast<std::uint32_t>(vertex.projection_offset_x),
            4);
        fingerprint_add(
            &hash,
            static_cast<std::uint32_t>(vertex.projection_offset_y),
            4);
        fingerprint_add(&hash, vertex.projection_plane, 4);
        fingerprint_add(&hash, vertex.source_vertex_identity, 4);
        fingerprint_add(&hash, vertex.transform_id);
        for (int index = 0; index < 9; ++index) {
            fingerprint_add(
                &hash,
                static_cast<std::uint16_t>(
                    vertex.transform_rotation[index]),
                2);
        }
        for (int index = 0; index < 3; ++index) {
            fingerprint_add(
                &hash,
                static_cast<std::uint32_t>(
                    vertex.transform_translation[index]),
                4);
        }
        fingerprint_add(
            &hash, vertex.exact_transform_valid ? 1U : 0U, 1);
        if (include_world) {
            fingerprint_add_float(&hash, vertex.world_x);
            fingerprint_add_float(&hash, vertex.world_y);
            fingerprint_add_float(&hash, vertex.world_z);
        }
    }
    return hash == 0 ? 1ULL : hash;
}

std::uint16_t resident_u16(const std::uint8_t* bytes) noexcept {
    return static_cast<std::uint16_t>(
        bytes[0] | (static_cast<std::uint16_t>(bytes[1]) << 8));
}

std::int16_t resident_i16(const std::uint8_t* bytes) noexcept {
    return static_cast<std::int16_t>(resident_u16(bytes));
}

std::uint32_t resident_u32(const std::uint8_t* bytes) noexcept {
    return
        static_cast<std::uint32_t>(bytes[0]) |
        (static_cast<std::uint32_t>(bytes[1]) << 8) |
        (static_cast<std::uint32_t>(bytes[2]) << 16) |
        (static_cast<std::uint32_t>(bytes[3]) << 24);
}

std::int32_t resident_i32(const std::uint8_t* bytes) noexcept {
    return static_cast<std::int32_t>(resident_u32(bytes));
}

std::uint64_t resident_u64(const std::uint8_t* bytes) noexcept {
    return
        static_cast<std::uint64_t>(resident_u32(bytes)) |
        (static_cast<std::uint64_t>(resident_u32(bytes + 4)) << 32);
}

struct ResidentVertex {
    std::int16_t x{};
    std::int16_t y{};
    std::int16_t z{};
    std::uint32_t source_identity{};
};

struct ResidentMaterial {
    std::uint16_t texture_page{};
    std::uint16_t clut{};
    std::uint16_t uv[4]{};
    std::uint32_t color[4]{};
};

struct ResidentPrimitive {
    std::uint32_t source_address{};
    std::uint32_t flags{};
    std::uint16_t stream{};
    std::uint16_t lod_threshold{};
    std::uint16_t indices[4]{};
    ResidentMaterial near_material{};
    ResidentMaterial distant_material{};
    std::uint8_t overlay_layer{};
    bool overlay_support{};
    bool replacement_surface{};
};

struct ResidentMesh {
    std::uint64_t key{};
    std::vector<ResidentVertex> vertices;
    std::vector<ResidentPrimitive> primitives;
    std::uint32_t overlay_pairs{};
    std::uint32_t overlay_primitives{};
    std::uint32_t untextured_overlay_primitives{};
    std::uint32_t replacement_primitives{};
    std::uint8_t maximum_overlay_layer{};
};

struct ResidentPlane {
    std::int64_t x{};
    std::int64_t y{};
    std::int64_t z{};
    ResidentVertex origin{};
    int dropped_axis{};
};

struct ResidentPoint2 {
    double x{};
    double y{};
};

struct ResidentPlaneKey {
    std::int64_t x{};
    std::int64_t y{};
    std::int64_t z{};
    std::int64_t d{};

    bool operator==(const ResidentPlaneKey& other) const noexcept {
        return
            x == other.x && y == other.y &&
            z == other.z && d == other.d;
    }
};

struct ResidentPlaneKeyHash {
    std::size_t operator()(const ResidentPlaneKey& key) const noexcept {
        std::size_t result = 0;
        const auto combine = [&result](std::int64_t value) {
            const std::size_t hashed = std::hash<std::int64_t>{}(value);
            result ^= hashed + 0x9e3779b9U + (result << 6) + (result >> 2);
        };
        combine(key.x);
        combine(key.y);
        combine(key.z);
        combine(key.d);
        return result;
    }
};

struct ResidentOverlayCandidate {
    ResidentPlaneKey plane{};
    double normal_x{};
    double normal_y{};
    double normal_z{};
    double plane_offset{};
    int dropped_axis{};
    std::int32_t minimum_x{};
    std::int32_t minimum_y{};
    std::int32_t maximum_x{};
    std::int32_t maximum_y{};
    bool valid{};
};

ResidentPoint2 resident_project(
    const ResidentVertex& point,
    int dropped_axis
) noexcept;

bool resident_opaque(
    const ResidentPrimitive& primitive
) noexcept {
    return (primitive.flags & resident_primitive_semi_transparent) == 0;
}

int resident_primitive_vertex_count(
    const ResidentPrimitive& primitive
) noexcept {
    return (primitive.flags & resident_primitive_quad) != 0 ? 4 : 3;
}

bool resident_primitive_plane(
    const ResidentMesh& mesh,
    const ResidentPrimitive& primitive,
    ResidentPlane* result
) noexcept {
    if (result == nullptr)
        return false;
    const int count = resident_primitive_vertex_count(primitive);
    for (int a = 0; a < count; ++a) {
        const auto& origin = mesh.vertices[primitive.indices[a]];
        for (int b = a + 1; b < count; ++b) {
            const auto& second = mesh.vertices[primitive.indices[b]];
            const std::int64_t ab_x =
                static_cast<std::int64_t>(second.x) - origin.x;
            const std::int64_t ab_y =
                static_cast<std::int64_t>(second.y) - origin.y;
            const std::int64_t ab_z =
                static_cast<std::int64_t>(second.z) - origin.z;
            for (int c = b + 1; c < count; ++c) {
                const auto& third = mesh.vertices[primitive.indices[c]];
                const std::int64_t ac_x =
                    static_cast<std::int64_t>(third.x) - origin.x;
                const std::int64_t ac_y =
                    static_cast<std::int64_t>(third.y) - origin.y;
                const std::int64_t ac_z =
                    static_cast<std::int64_t>(third.z) - origin.z;
                const std::int64_t normal_x = ab_y * ac_z - ab_z * ac_y;
                const std::int64_t normal_y = ab_z * ac_x - ab_x * ac_z;
                const std::int64_t normal_z = ab_x * ac_y - ab_y * ac_x;
                if (normal_x == 0 && normal_y == 0 && normal_z == 0)
                    continue;
                result->x = normal_x;
                result->y = normal_y;
                result->z = normal_z;
                result->origin = origin;
                const std::int64_t abs_x = std::llabs(normal_x);
                const std::int64_t abs_y = std::llabs(normal_y);
                const std::int64_t abs_z = std::llabs(normal_z);
                result->dropped_axis =
                    abs_x >= abs_y && abs_x >= abs_z
                        ? 0
                        : abs_y >= abs_z ? 1 : 2;
                for (int corner = 0; corner < count; ++corner) {
                    const auto& point =
                        mesh.vertices[primitive.indices[corner]];
                    const std::int64_t distance =
                        normal_x * (point.x - origin.x) +
                        normal_y * (point.y - origin.y) +
                        normal_z * (point.z - origin.z);
                    if (distance != 0)
                        return false;
                }
                return true;
            }
        }
    }
    return false;
}

ResidentPlaneKey resident_normalized_plane(
    const ResidentPlane& plane
) noexcept {
    ResidentPlaneKey key{
        plane.x,
        plane.y,
        plane.z,
        -(
            plane.x * plane.origin.x +
            plane.y * plane.origin.y +
            plane.z * plane.origin.z),
    };
    std::int64_t divisor = 0;
    divisor = std::gcd(divisor, std::llabs(key.x));
    divisor = std::gcd(divisor, std::llabs(key.y));
    divisor = std::gcd(divisor, std::llabs(key.z));
    divisor = std::gcd(divisor, std::llabs(key.d));
    if (divisor > 1) {
        key.x /= divisor;
        key.y /= divisor;
        key.z /= divisor;
        key.d /= divisor;
    }
    const std::int64_t first =
        key.x != 0 ? key.x : key.y != 0 ? key.y : key.z;
    if (first < 0) {
        key.x = -key.x;
        key.y = -key.y;
        key.z = -key.z;
        key.d = -key.d;
    }
    return key;
}

ResidentOverlayCandidate resident_overlay_candidate_from_plane(
    const ResidentMesh& mesh,
    const ResidentPrimitive& primitive,
    const ResidentPlane& plane,
    const int* corners,
    int corner_count
) noexcept {
    ResidentOverlayCandidate result{};
    if (!resident_opaque(primitive) || corners == nullptr || corner_count < 3)
        return result;
    result.plane = resident_normalized_plane(plane);
    const double normal_length = std::sqrt(
        static_cast<double>(plane.x) * plane.x +
        static_cast<double>(plane.y) * plane.y +
        static_cast<double>(plane.z) * plane.z);
    if (!std::isfinite(normal_length) || normal_length <= 0.0)
        return result;
    double direction = 1.0;
    const std::int64_t first_normal_component =
        plane.x != 0 ? plane.x : plane.y != 0 ? plane.y : plane.z;
    if (first_normal_component < 0)
        direction = -1.0;
    result.normal_x = direction * plane.x / normal_length;
    result.normal_y = direction * plane.y / normal_length;
    result.normal_z = direction * plane.z / normal_length;
    result.plane_offset = -(
        result.normal_x * plane.origin.x +
        result.normal_y * plane.origin.y +
        result.normal_z * plane.origin.z);
    result.dropped_axis = plane.dropped_axis;
    const auto first = resident_project(
        mesh.vertices[primitive.indices[corners[0]]], plane.dropped_axis);
    result.minimum_x = result.maximum_x = static_cast<std::int32_t>(first.x);
    result.minimum_y = result.maximum_y = static_cast<std::int32_t>(first.y);
    for (int corner = 1; corner < corner_count; ++corner) {
        const auto point = resident_project(
            mesh.vertices[primitive.indices[corners[corner]]],
            plane.dropped_axis);
        const auto x = static_cast<std::int32_t>(point.x);
        const auto y = static_cast<std::int32_t>(point.y);
        result.minimum_x = (std::min)(result.minimum_x, x);
        result.minimum_y = (std::min)(result.minimum_y, y);
        result.maximum_x = (std::max)(result.maximum_x, x);
        result.maximum_y = (std::max)(result.maximum_y, y);
    }
    result.valid = true;
    return result;
}

ResidentOverlayCandidate resident_overlay_candidate(
    const ResidentMesh& mesh,
    const ResidentPrimitive& primitive
) noexcept {
    if (!resident_opaque(primitive))
        return {};
    ResidentPlane plane{};
    if (!resident_primitive_plane(mesh, primitive, &plane))
        return {};
    constexpr int corners[] = {0, 1, 2, 3};
    return resident_overlay_candidate_from_plane(
        mesh,
        primitive,
        plane,
        corners,
        resident_primitive_vertex_count(primitive));
}

bool resident_road_overlay_candidate(
    const ResidentOverlayCandidate& candidate
) noexcept {
    // The priority relationship is specifically between artwork and its
    // drivable road support.  Coplanar scenery uses the same authored data
    // pattern (small fence, billboard, cliff, and treeline pieces over a
    // larger vertical surface), but promoting those pieces into the road
    // stencil lets them punch through asphalt wherever their projections
    // overlap.  GT2 course model space is Z-up; requiring Z to be the
    // dominant plane normal admits banked and sloped ground while excluding
    // upright scenery without relying on a track, address, stream, material,
    // or texture exception.
    return candidate.valid && candidate.dropped_axis == 2;
}

bool resident_overlay_bounds_positive_overlap(
    const ResidentOverlayCandidate& left,
    const ResidentOverlayCandidate& right
) noexcept {
    return
        (std::min)(left.maximum_x, right.maximum_x) >
            (std::max)(left.minimum_x, right.minimum_x) &&
        (std::min)(left.maximum_y, right.maximum_y) >
            (std::max)(left.minimum_y, right.minimum_y);
}

ResidentPoint2 resident_project(
    const ResidentVertex& point,
    int dropped_axis
) noexcept {
    if (dropped_axis == 0)
        return {static_cast<double>(point.y), static_cast<double>(point.z)};
    if (dropped_axis == 1)
        return {static_cast<double>(point.x), static_cast<double>(point.z)};
    return {static_cast<double>(point.x), static_cast<double>(point.y)};
}

double resident_cross(
    const ResidentPoint2& a,
    const ResidentPoint2& b,
    const ResidentPoint2& point
) noexcept {
    return
        (b.x - a.x) * (point.y - a.y) -
        (b.y - a.y) * (point.x - a.x);
}

double resident_area_twice(
    const std::vector<ResidentPoint2>& polygon
) noexcept {
    double area = 0.0;
    for (std::size_t index = 0; index < polygon.size(); ++index) {
        const auto& a = polygon[index];
        const auto& b = polygon[(index + 1) % polygon.size()];
        area += a.x * b.y - a.y * b.x;
    }
    return area;
}

std::vector<ResidentPoint2> resident_triangle_overlap_polygon(
    std::array<ResidentPoint2, 3> subject,
    std::array<ResidentPoint2, 3> clip
) {
    std::vector<ResidentPoint2> polygon(subject.begin(), subject.end());
    if (resident_area_twice(polygon) < 0.0)
        std::swap(polygon[1], polygon[2]);
    std::vector<ResidentPoint2> clip_polygon(clip.begin(), clip.end());
    if (resident_area_twice(clip_polygon) < 0.0)
        std::swap(clip_polygon[1], clip_polygon[2]);
    for (int edge = 0; edge < 3 && !polygon.empty(); ++edge) {
        const ResidentPoint2 edge_a = clip_polygon[edge];
        const ResidentPoint2 edge_b = clip_polygon[(edge + 1) % 3];
        std::vector<ResidentPoint2> output;
        output.reserve(polygon.size() + 1);
        ResidentPoint2 previous = polygon.back();
        double previous_distance = resident_cross(edge_a, edge_b, previous);
        bool previous_inside = previous_distance >= 0.0;
        for (const auto& current : polygon) {
            const double current_distance =
                resident_cross(edge_a, edge_b, current);
            const bool current_inside = current_distance >= 0.0;
            if (current_inside != previous_inside) {
                const double denominator =
                    previous_distance - current_distance;
                if (denominator != 0.0) {
                    const double amount = previous_distance / denominator;
                    output.push_back(ResidentPoint2{
                        previous.x + (current.x - previous.x) * amount,
                        previous.y + (current.y - previous.y) * amount,
                    });
                }
            }
            if (current_inside)
                output.push_back(current);
            previous = current;
            previous_distance = current_distance;
            previous_inside = current_inside;
        }
        polygon = std::move(output);
    }
    if (polygon.size() < 3 ||
        std::abs(resident_area_twice(polygon)) <= 1.0e-6)
        polygon.clear();
    return polygon;
}

std::array<int, 3> resident_primitive_triangle_corners(
    const ResidentPrimitive& primitive,
    int triangle
) noexcept {
    if ((primitive.flags & resident_primitive_quad) == 0)
        return {0, 1, 2};
    return resident_quad_packet_corners(
        (primitive.flags & resident_primitive_primary_path) != 0,
        triangle);
}

ResidentOverlayCandidate resident_triangle_overlay_candidate(
    const ResidentMesh& mesh,
    const ResidentPrimitive& primitive,
    int triangle
) noexcept {
    if (!resident_opaque(primitive))
        return {};
    const auto corners = resident_primitive_triangle_corners(
        primitive, triangle);
    const auto& origin = mesh.vertices[primitive.indices[corners[0]]];
    const auto& second = mesh.vertices[primitive.indices[corners[1]]];
    const auto& third = mesh.vertices[primitive.indices[corners[2]]];
    const std::int64_t ab_x =
        static_cast<std::int64_t>(second.x) - origin.x;
    const std::int64_t ab_y =
        static_cast<std::int64_t>(second.y) - origin.y;
    const std::int64_t ab_z =
        static_cast<std::int64_t>(second.z) - origin.z;
    const std::int64_t ac_x =
        static_cast<std::int64_t>(third.x) - origin.x;
    const std::int64_t ac_y =
        static_cast<std::int64_t>(third.y) - origin.y;
    const std::int64_t ac_z =
        static_cast<std::int64_t>(third.z) - origin.z;
    ResidentPlane plane{};
    plane.x = ab_y * ac_z - ab_z * ac_y;
    plane.y = ab_z * ac_x - ab_x * ac_z;
    plane.z = ab_x * ac_y - ab_y * ac_x;
    if (plane.x == 0 && plane.y == 0 && plane.z == 0)
        return {};
    plane.origin = origin;
    const std::int64_t abs_x = std::llabs(plane.x);
    const std::int64_t abs_y = std::llabs(plane.y);
    const std::int64_t abs_z = std::llabs(plane.z);
    plane.dropped_axis =
        abs_x >= abs_y && abs_x >= abs_z
            ? 0
            : abs_y >= abs_z ? 1 : 2;
    return resident_overlay_candidate_from_plane(
        mesh,
        primitive,
        plane,
        corners.data(),
        static_cast<int>(corners.size()));
}

double resident_primitive_projected_area_twice(
    const ResidentMesh& mesh,
    const ResidentPrimitive& primitive,
    int dropped_axis
) noexcept {
    const int triangle_count =
        (primitive.flags & resident_primitive_quad) != 0 ? 2 : 1;
    double area = 0.0;
    for (int triangle = 0; triangle < triangle_count; ++triangle) {
        const auto corners = resident_primitive_triangle_corners(
            primitive, triangle);
        const auto a = resident_project(
            mesh.vertices[primitive.indices[corners[0]]], dropped_axis);
        const auto b = resident_project(
            mesh.vertices[primitive.indices[corners[1]]], dropped_axis);
        const auto c = resident_project(
            mesh.vertices[primitive.indices[corners[2]]], dropped_axis);
        area += std::abs(resident_cross(a, b, c));
    }
    return area;
}

bool resident_overlay_is_smaller_than_support(
    double overlay_area_twice,
    double support_area_twice
) noexcept {
    // Authored road paint is a bounded detail surface over a larger course
    // surface. Exact-coplanar road/LOD replacements can arrive later too, but
    // they cover a comparable or larger area and must retain physical depth.
    // Seattle's authored data has a clean boundary: detail relationships end
    // below 0.20 while replacement relationships begin above 0.28.
    return overlay_area_twice > 0.0 && support_area_twice > 0.0 &&
        overlay_area_twice <= support_area_twice * 0.25;
}

bool resident_elevated_overlay_relation(
    const ResidentOverlayCandidate& support,
    const ResidentOverlayCandidate& overlay
) noexcept {
    const double parallel =
        support.normal_x * overlay.normal_x +
        support.normal_y * overlay.normal_y +
        support.normal_z * overlay.normal_z;
    if (parallel < 0.999999)
        return false;
    const double separation = std::abs(
        support.plane_offset - overlay.plane_offset);
    // Seattle's authored road-detail surfaces use integer offsets of one or
    // two model units above their support. Both classes share the same
    // later-authored, bounded, positively overlapping geometry signature.
    // Allow the measured fixed-point normalization tolerance, but do not turn
    // this into a general proximity rule: three-unit and comparable-area
    // relationships remain ordinary physical geometry.
    return separation > 0.0 && separation <= 2.01;
}

bool resident_authored_replacement_relation(
    const ResidentMesh& mesh,
    const ResidentPrimitive& earlier,
    double earlier_area_twice,
    const ResidentPrimitive& later,
    double later_area_twice
) {
    // This is the complementary authored-course relationship to road paint.
    // Some sectors first submit small untextured Gouraud pieces of a coarse
    // ground plane, four model units above the detailed textured road that is
    // submitted later.  The PS1's painter ordering makes the detailed road
    // own their overlap; a modern depth buffer otherwise exposes the coarse
    // pieces until the camera passes them.
    //
    // Keep the rule typed and bounded: later textured over earlier untextured,
    // locally parallel and 3-6 model units apart, with the later primitive at
    // least four times the projected area. GT2's road quads are commonly a
    // few fixed-point units non-planar, so compare the exact triangles that
    // the renderer emits and measure separation at their actual overlap.
    // Exact/1-2-unit small surfaces remain in the order-independent
    // road-artwork classifier above.
    if (
        (earlier.flags & resident_primitive_textured) != 0 ||
        (later.flags & resident_primitive_textured) == 0 ||
        earlier_area_twice <= 0.0 || later_area_twice <= 0.0 ||
        later_area_twice < earlier_area_twice * 4.0
    ) return false;
    const int earlier_triangles =
        (earlier.flags & resident_primitive_quad) != 0 ? 2 : 1;
    const int later_triangles =
        (later.flags & resident_primitive_quad) != 0 ? 2 : 1;
    for (int earlier_triangle = 0;
         earlier_triangle < earlier_triangles;
         ++earlier_triangle) {
        const auto earlier_candidate = resident_triangle_overlay_candidate(
            mesh, earlier, earlier_triangle);
        if (!earlier_candidate.valid || earlier_candidate.dropped_axis != 2 ||
            std::abs(earlier_candidate.normal_z) < 0.95)
            continue;
        const auto earlier_corners = resident_primitive_triangle_corners(
            earlier, earlier_triangle);
        std::array<ResidentPoint2, 3> earlier_points{};
        for (int corner = 0; corner < 3; ++corner) {
            earlier_points[corner] = resident_project(
                mesh.vertices[earlier.indices[earlier_corners[corner]]], 2);
        }
        for (int later_triangle = 0;
             later_triangle < later_triangles;
             ++later_triangle) {
            const auto later_candidate = resident_triangle_overlay_candidate(
                mesh, later, later_triangle);
            if (!later_candidate.valid || later_candidate.dropped_axis != 2 ||
                std::abs(later_candidate.normal_z) < 0.95 ||
                !resident_overlay_bounds_positive_overlap(
                    earlier_candidate, later_candidate))
                continue;
            const double parallel =
                earlier_candidate.normal_x * later_candidate.normal_x +
                earlier_candidate.normal_y * later_candidate.normal_y +
                earlier_candidate.normal_z * later_candidate.normal_z;
            if (parallel < 0.9995)
                continue;
            const auto later_corners = resident_primitive_triangle_corners(
                later, later_triangle);
            std::array<ResidentPoint2, 3> later_points{};
            for (int corner = 0; corner < 3; ++corner) {
                later_points[corner] = resident_project(
                    mesh.vertices[later.indices[later_corners[corner]]], 2);
            }
            const auto overlap = resident_triangle_overlap_polygon(
                earlier_points, later_points);
            if (overlap.empty())
                continue;
            ResidentPoint2 overlap_center{};
            for (const auto& point : overlap) {
                overlap_center.x += point.x;
                overlap_center.y += point.y;
            }
            overlap_center.x /= static_cast<double>(overlap.size());
            overlap_center.y /= static_cast<double>(overlap.size());
            const double earlier_height = -(
                earlier_candidate.normal_x * overlap_center.x +
                earlier_candidate.normal_y * overlap_center.y +
                earlier_candidate.plane_offset) /
                earlier_candidate.normal_z;
            const double later_height = -(
                later_candidate.normal_x * overlap_center.x +
                later_candidate.normal_y * overlap_center.y +
                later_candidate.plane_offset) /
                later_candidate.normal_z;
            const double separation = std::abs(
                earlier_height - later_height);
            if (separation > 2.01 && separation <= 6.01)
                return true;
        }
    }
    return false;
}

bool resident_primitives_positive_overlap(
    const ResidentMesh& mesh,
    const ResidentPrimitive& left,
    const ResidentPrimitive& right,
    int dropped_axis
) {
    double overlap_area_twice = 0.0;
    const int left_triangles =
        (left.flags & resident_primitive_quad) != 0 ? 2 : 1;
    const int right_triangles =
        (right.flags & resident_primitive_quad) != 0 ? 2 : 1;
    for (int left_triangle = 0;
         left_triangle < left_triangles;
         ++left_triangle) {
        const auto left_corners = resident_primitive_triangle_corners(
            left, left_triangle);
        std::array<ResidentPoint2, 3> left_points{};
        for (int vertex = 0; vertex < 3; ++vertex) {
            left_points[vertex] = resident_project(
                mesh.vertices[left.indices[left_corners[vertex]]],
                dropped_axis);
        }
        for (int right_triangle = 0;
             right_triangle < right_triangles;
             ++right_triangle) {
            const auto right_corners = resident_primitive_triangle_corners(
                right, right_triangle);
            std::array<ResidentPoint2, 3> right_points{};
            for (int vertex = 0; vertex < 3; ++vertex) {
                right_points[vertex] = resident_project(
                    mesh.vertices[right.indices[right_corners[vertex]]],
                    dropped_axis);
            }
            const auto overlap = resident_triangle_overlap_polygon(
                left_points, right_points);
            if (!overlap.empty())
                overlap_area_twice += std::abs(
                    resident_area_twice(overlap));
        }
    }
    const double left_area_twice = resident_primitive_projected_area_twice(
        mesh, left, dropped_axis);
    const double right_area_twice = resident_primitive_projected_area_twice(
        mesh, right, dropped_axis);
    const double smaller_area_twice = (std::min)(
        left_area_twice, right_area_twice);
    // Adjacent fixed-point course sectors can miss exact collinearity by one
    // half-unit and leave a numerical overlap sliver.  That boundary contact
    // is not a road-artwork relationship: promoting the smaller full road
    // sector into the stencil-only overlay pass cuts a rectangular hole when
    // its adjacent sector no longer owns the same screen pixels.  Authored
    // Some untextured authored paint ends at a resident-mesh boundary and can
    // only be related to adjacent textured asphalt by a narrow overlap.  The
    // Grand Valley start-box segment at 0x80122F34 is the limiting known
    // valid case.  Same-material road sectors do not have that artwork/support
    // relationship: keep the stricter threshold for them so the textured
    // bridge-sector sliver at 0x800FC5FC remains rejected.
    const bool mixed_texturing =
        ((left.flags & resident_primitive_textured) != 0) !=
        ((right.flags & resident_primitive_textured) != 0);
    if (mixed_texturing)
        return overlap_area_twice > 0.0;
    return smaller_area_twice > 0.0 &&
        overlap_area_twice > smaller_area_twice / 1024.0;
}

void classify_resident_track_overlays(ResidentMesh* mesh) {
    if (mesh == nullptr)
        return;
    mesh->overlay_pairs = 0;
    mesh->overlay_primitives = 0;
    mesh->untextured_overlay_primitives = 0;
    mesh->replacement_primitives = 0;
    mesh->maximum_overlay_layer = 0;
    for (auto& primitive : mesh->primitives) {
        primitive.overlay_layer = 0;
        primitive.overlay_support = false;
        primitive.replacement_surface = false;
    }
    // The road relationship is defined in GT2's world-aligned course space
    // (Z-up), not arbitrary object-local space. Auxiliary scenery is rotated
    // per instance: Trial Mountain's upright sign panels lie in local Z=-2.
    // Treating those panels as ground artwork removes their middle section
    // in the road-stencil pass. Keep local-space meshes at physical depth;
    // projection path/face winding does not identify their coordinate frame.
    if (std::any_of(mesh->primitives.begin(), mesh->primitives.end(),
            [](const ResidentPrimitive& primitive) {
                return (primitive.flags &
                    resident_primitive_local_coordinates) != 0;
            }))
        return;
    std::vector<ResidentOverlayCandidate> candidates;
    candidates.reserve(mesh->primitives.size());
    for (const auto& primitive : mesh->primitives) {
        ResidentOverlayCandidate candidate =
            resident_overlay_candidate(*mesh, primitive);
        if (!resident_road_overlay_candidate(candidate))
            candidate.valid = false;
        candidates.push_back(candidate);
    }
    std::vector<double> projected_areas_twice;
    projected_areas_twice.reserve(mesh->primitives.size());
    for (std::size_t index = 0; index < mesh->primitives.size(); ++index) {
        projected_areas_twice.push_back(candidates[index].valid
            ? resident_primitive_projected_area_twice(
                *mesh,
                mesh->primitives[index],
                candidates[index].dropped_axis)
            : 0.0);
    }
    std::vector<double> ground_projected_areas_twice;
    ground_projected_areas_twice.reserve(mesh->primitives.size());
    for (const auto& primitive : mesh->primitives) {
        ground_projected_areas_twice.push_back(
            resident_primitive_projected_area_twice(*mesh, primitive, 2));
    }
    // Road artwork is identified by its geometric relationship to a larger
    // road surface, not by packet stream or source order. Seattle contains
    // both conventions: textured detail is commonly authored after its road,
    // while the untextured white rectangles after the first hairpin are
    // authored before the textured asphalt that supports them. Treat the
    // smaller member as the overlay in either ordering.
    std::vector<std::vector<std::size_t>> supports_by_overlay(
        mesh->primitives.size());
    std::vector<std::vector<std::size_t>> supports_by_replacement(
        mesh->primitives.size());
    for (std::size_t left = 0; left < mesh->primitives.size(); ++left) {
        for (std::size_t right = left + 1;
             right < mesh->primitives.size();
             ++right) {
            // Authored replacement is intentionally directional: the later
            // detailed primitive replaces the earlier coarse course plane.
            // It is evaluated from the renderer's actual triangles before
            // the exact-quad road-artwork path below, because fixed-point
            // road quads are often slightly non-planar.
            if (resident_authored_replacement_relation(
                    *mesh,
                    mesh->primitives[left],
                    ground_projected_areas_twice[left],
                    mesh->primitives[right],
                    ground_projected_areas_twice[right])) {
                mesh->primitives[left].overlay_support = true;
                supports_by_replacement[right].push_back(left);
                ++mesh->overlay_pairs;
                continue;
            }

            if (!candidates[left].valid || !candidates[right].valid)
                continue;

            const bool bounds_overlap =
                resident_overlay_bounds_positive_overlap(
                    candidates[left], candidates[right]);
            if (!bounds_overlap)
                continue;
            const bool positive_overlap =
                resident_primitives_positive_overlap(
                    *mesh,
                    mesh->primitives[left],
                    mesh->primitives[right],
                    candidates[right].dropped_axis);
            if (!positive_overlap)
                continue;

            std::size_t overlay = left;
            std::size_t support = right;
            if (!resident_overlay_is_smaller_than_support(
                    projected_areas_twice[overlay],
                    projected_areas_twice[support])) {
                overlay = right;
                support = left;
                if (!resident_overlay_is_smaller_than_support(
                        projected_areas_twice[overlay],
                        projected_areas_twice[support]))
                    continue;
            }

            const auto& overlay_candidate = candidates[overlay];
            const auto& support_candidate = candidates[support];
            const bool coplanar =
                overlay_candidate.plane == support_candidate.plane;
            if (!coplanar && !resident_elevated_overlay_relation(
                    support_candidate, overlay_candidate))
                continue;
            mesh->primitives[support].overlay_support = true;
            supports_by_overlay[overlay].push_back(support);
            ++mesh->overlay_pairs;
        }
    }

    // Every edge points from a surface to one at least four times larger, so
    // descending projected area is a deterministic topological order. This
    // retains nested artwork layers without relying on authored packet order.
    std::vector<std::size_t> area_order(mesh->primitives.size());
    std::iota(area_order.begin(), area_order.end(), 0U);
    std::stable_sort(
        area_order.begin(),
        area_order.end(),
        [&projected_areas_twice](std::size_t left, std::size_t right) {
            return projected_areas_twice[left] > projected_areas_twice[right];
        });
    for (const std::size_t overlay : area_order) {
        std::uint8_t layer = 0;
        for (const std::size_t support : supports_by_overlay[overlay]) {
            layer = (std::max)(
                layer,
                static_cast<std::uint8_t>((std::min)(
                    static_cast<unsigned>(
                        mesh->primitives[support].overlay_layer) + 1U,
                    31U)));
        }
        if (layer == 0)
            continue;
        mesh->primitives[overlay].overlay_layer = layer;
    }

    // Replacement edges always point from an earlier primitive to a later
    // one, so authored order is a deterministic topological order even when
    // a later detailed surface itself supports a subsequent replacement.
    for (std::size_t replacement = 0;
         replacement < mesh->primitives.size();
         ++replacement) {
        std::uint8_t layer = 0;
        for (const std::size_t support :
             supports_by_replacement[replacement]) {
            layer = (std::max)(
                layer,
                static_cast<std::uint8_t>((std::min)(
                    static_cast<unsigned>(
                        mesh->primitives[support].overlay_layer) + 1U,
                    31U)));
        }
        if (layer == 0)
            continue;
        auto& primitive = mesh->primitives[replacement];
        primitive.overlay_layer = (std::max)(
            primitive.overlay_layer, layer);
        primitive.replacement_surface = true;
    }

    for (const auto& primitive : mesh->primitives) {
        if (primitive.overlay_layer == 0)
            continue;
        ++mesh->overlay_primitives;
        if ((primitive.flags & resident_primitive_textured) == 0)
            ++mesh->untextured_overlay_primitives;
        if (primitive.replacement_surface)
            ++mesh->replacement_primitives;
        mesh->maximum_overlay_layer = (std::max)(
            mesh->maximum_overlay_layer, primitive.overlay_layer);
    }
}

void trace_resident_near_overlay_candidates(const ResidentMesh& mesh) {
    if (std::getenv("OPENGT_TRACE_RESIDENT_NEAR_OVERLAYS") == nullptr)
        return;
    double minimum_parallel = 0.999999;
    if (const char* configured = std::getenv(
            "OPENGT_TRACE_RESIDENT_NEAR_OVERLAY_MIN_PARALLEL")) {
        const double parsed = std::strtod(configured, nullptr);
        if (std::isfinite(parsed) && parsed >= -1.0 && parsed <= 1.0)
            minimum_parallel = parsed;
    }
    double maximum_separation = 64.0;
    if (const char* configured = std::getenv(
            "OPENGT_TRACE_RESIDENT_NEAR_OVERLAY_MAX_SEPARATION")) {
        const double parsed = std::strtod(configured, nullptr);
        if (std::isfinite(parsed) && parsed >= 0.0)
            maximum_separation = parsed;
    }
    std::vector<ResidentOverlayCandidate> candidates;
    candidates.reserve(mesh.primitives.size());
    for (const auto& primitive : mesh.primitives)
        candidates.push_back(resident_overlay_candidate(mesh, primitive));
    std::size_t traced_pairs = 0;
    for (std::size_t later = 0; later < mesh.primitives.size(); ++later) {
        const auto& later_candidate = candidates[later];
        if (!later_candidate.valid)
            continue;
        for (std::size_t earlier = 0; earlier < later; ++earlier) {
            const auto& earlier_candidate = candidates[earlier];
            if (!earlier_candidate.valid)
                continue;
            const double parallel =
                earlier_candidate.normal_x * later_candidate.normal_x +
                earlier_candidate.normal_y * later_candidate.normal_y +
                earlier_candidate.normal_z * later_candidate.normal_z;
            if (parallel < minimum_parallel)
                continue;
            const double separation = std::abs(
                earlier_candidate.plane_offset -
                later_candidate.plane_offset);
            if (separation > maximum_separation ||
                !resident_overlay_bounds_positive_overlap(
                    earlier_candidate, later_candidate) ||
                !resident_primitives_positive_overlap(
                    mesh,
                    mesh.primitives[earlier],
                    mesh.primitives[later],
                    later_candidate.dropped_axis))
                continue;
            const auto& earlier_primitive = mesh.primitives[earlier];
            const auto& later_primitive = mesh.primitives[later];
            const double earlier_area =
                resident_primitive_projected_area_twice(
                    mesh,
                    earlier_primitive,
                    later_candidate.dropped_axis);
            const double later_area =
                resident_primitive_projected_area_twice(
                    mesh,
                    later_primitive,
                    later_candidate.dropped_axis);
            std::fprintf(
                stderr,
                "[Native-Resident-Near-Overlay] mesh=%016llx "
                "earlier=%zu/%08x later=%zu/%08x separation=%.6f "
                "parallel=%.9f exact=%u area2=%.3f/%.3f ratio=%.6f "
                "layers=%u/%u streams=%u/%u flags=%08x/%08x "
                "nearMaterial=%04x,%04x/%04x,%04x\n",
                static_cast<unsigned long long>(mesh.key),
                earlier,
                earlier_primitive.source_address,
                later,
                later_primitive.source_address,
                separation,
                parallel,
                earlier_candidate.plane == later_candidate.plane ? 1U : 0U,
                earlier_area,
                later_area,
                earlier_area > 0.0 ? later_area / earlier_area : 0.0,
                static_cast<unsigned>(earlier_primitive.overlay_layer),
                static_cast<unsigned>(later_primitive.overlay_layer),
                static_cast<unsigned>(earlier_primitive.stream),
                static_cast<unsigned>(later_primitive.stream),
                earlier_primitive.flags,
                later_primitive.flags,
                earlier_primitive.near_material.texture_page,
                earlier_primitive.near_material.clut,
                later_primitive.near_material.texture_page,
                later_primitive.near_material.clut);
            ++traced_pairs;
        }
    }
    if (traced_pairs != 0) {
        std::fprintf(
            stderr,
            "[Native-Resident-Near-Overlay-Summary] mesh=%016llx "
            "pairs=%zu minimumParallel=%.9f maximumSeparation=%.3f\n",
            static_cast<unsigned long long>(mesh.key),
            traced_pairs,
            minimum_parallel,
            maximum_separation);
    }
}

void trace_resident_primitive_address_range(const ResidentMesh& mesh) {
    const char* configured_minimum = std::getenv(
        "OPENGT_TRACE_RESIDENT_PRIMITIVE_ADDRESS_MIN");
    const char* configured_maximum = std::getenv(
        "OPENGT_TRACE_RESIDENT_PRIMITIVE_ADDRESS_MAX");
    if (configured_minimum == nullptr || configured_maximum == nullptr)
        return;
    const auto minimum = static_cast<std::uint32_t>(
        std::strtoul(configured_minimum, nullptr, 0));
    const auto maximum = static_cast<std::uint32_t>(
        std::strtoul(configured_maximum, nullptr, 0));
    if (minimum > maximum)
        return;
    for (std::size_t index = 0; index < mesh.primitives.size(); ++index) {
        const auto& primitive = mesh.primitives[index];
        if (primitive.source_address < minimum ||
            primitive.source_address > maximum)
            continue;
        const ResidentOverlayCandidate candidate =
            resident_overlay_candidate(mesh, primitive);
        std::fprintf(
            stderr,
            "[Native-Resident-Primitive-Geometry] mesh=%016llx "
            "primitive=%zu source=%08x flags=%08x stream=%u "
            "layer=%u support=%u replacement=%u validPlane=%u "
            "normal=%.9f,%.9f,%.9f offset=%.9f dropped=%d "
            "vertices=",
            static_cast<unsigned long long>(mesh.key),
            index,
            primitive.source_address,
            primitive.flags,
            static_cast<unsigned>(primitive.stream),
            static_cast<unsigned>(primitive.overlay_layer),
            primitive.overlay_support ? 1U : 0U,
            primitive.replacement_surface ? 1U : 0U,
            candidate.valid ? 1U : 0U,
            candidate.normal_x,
            candidate.normal_y,
            candidate.normal_z,
            candidate.plane_offset,
            candidate.dropped_axis);
        const int vertex_count = resident_primitive_vertex_count(primitive);
        for (int corner = 0; corner < vertex_count; ++corner) {
            const auto& vertex = mesh.vertices[primitive.indices[corner]];
            std::fprintf(
                stderr,
                "%s%d,%d,%d",
                corner == 0 ? "" : ";",
                static_cast<int>(vertex.x),
                static_cast<int>(vertex.y),
                static_cast<int>(vertex.z));
        }
        std::fprintf(stderr, "\n");
    }
}

struct ResidentInstance {
    std::uint64_t mesh_key{};
    std::uint32_t object_id{};
    std::uint32_t model_pointer{};
    std::uint64_t transform_id{};
    std::int16_t rotation[9]{};
    std::int32_t translation[3]{};
    std::int32_t projection_offset_x{};
    std::int32_t projection_offset_y{};
    std::uint32_t projection_plane{};
    std::int16_t clip_x0{};
    std::int16_t clip_y0{};
    std::int16_t clip_x1{};
    std::int16_t clip_y1{};
    std::int16_t texture_mask_x{};
    std::int16_t texture_mask_y{};
    std::int16_t texture_offset_x{};
    std::int16_t texture_offset_y{};
    std::int16_t draw_offset_x{};
    std::int16_t draw_offset_y{};
    std::uint32_t environment_flags{};
    std::uint16_t default_texture_page{};
    std::uint16_t capture_insert_index{};
    std::int32_t depth_scale_exponent{};
    bool depth_scale_valid{};
};

bool parse_resident_material(
    const std::uint8_t* bytes,
    ResidentMaterial* material
) noexcept {
    if (bytes == nullptr || material == nullptr)
        return false;
    material->texture_page = resident_u16(bytes);
    material->clut = resident_u16(bytes + 2);
    for (int index = 0; index < 4; ++index) {
        material->uv[index] = resident_u16(bytes + 4 + index * 2);
        material->color[index] = resident_u32(bytes + 12 + index * 4);
    }
    return true;
}

bool parse_resident_mesh(
    const std::uint8_t* bytes,
    std::size_t size,
    ResidentMesh* mesh
) {
    if (
        bytes == nullptr || mesh == nullptr ||
        size < resident_mesh_header_size ||
        resident_u64(bytes) != resident_mesh_magic ||
        resident_u32(bytes + 8) != resident_mesh_version ||
        resident_u32(bytes + 12) != resident_mesh_header_size
    ) return false;
    const std::uint32_t vertex_count = resident_u32(bytes + 24);
    const std::uint32_t primitive_count = resident_u32(bytes + 28);
    if (vertex_count == 0 || vertex_count > 1024 || primitive_count > 65535)
        return false;
    const std::uint64_t vertex_bytes =
        static_cast<std::uint64_t>(vertex_count) * resident_vertex_stride;
    const std::uint64_t primitive_bytes =
        static_cast<std::uint64_t>(primitive_count) *
        resident_primitive_stride;
    const std::uint64_t required =
        resident_mesh_header_size + vertex_bytes + primitive_bytes;
    if (required != size)
        return false;

    ResidentMesh parsed{};
    parsed.key = resident_u64(bytes + 16);
    if (parsed.key == 0)
        return false;
    parsed.vertices.resize(vertex_count);
    const std::uint8_t* cursor = bytes + resident_mesh_header_size;
    for (std::uint32_t index = 0; index < vertex_count; ++index) {
        auto& vertex = parsed.vertices[index];
        vertex.x = resident_i16(cursor);
        vertex.y = resident_i16(cursor + 2);
        vertex.z = resident_i16(cursor + 4);
        vertex.source_identity = resident_u32(cursor + 8);
        if (vertex.source_identity == 0)
            return false;
        cursor += resident_vertex_stride;
    }
    parsed.primitives.resize(primitive_count);
    for (std::uint32_t index = 0; index < primitive_count; ++index) {
        auto& primitive = parsed.primitives[index];
        primitive.source_address = resident_u32(cursor);
        primitive.flags = resident_u32(cursor + 4);
        primitive.stream = resident_u16(cursor + 8);
        primitive.lod_threshold = resident_u16(cursor + 10);
        const int vertex_total =
            (primitive.flags & resident_primitive_quad) != 0 ? 4 : 3;
        for (int corner = 0; corner < 4; ++corner) {
            primitive.indices[corner] = resident_u16(cursor + 12 + corner * 2);
            if (corner < vertex_total && primitive.indices[corner] >= vertex_count)
                return false;
        }
        if (!parse_resident_material(cursor + 24, &primitive.near_material) ||
            !parse_resident_material(cursor + 52, &primitive.distant_material))
            return false;
        cursor += resident_primitive_stride;
    }
    classify_resident_track_overlays(&parsed);
    *mesh = std::move(parsed);
    return true;
}

bool parse_resident_instances(
    const std::uint8_t* bytes,
    std::size_t count,
    std::vector<ResidentInstance>* instances
) {
    if (instances == nullptr || count > 4096 || (count != 0 && bytes == nullptr))
        return false;
    instances->resize(count);
    for (std::size_t index = 0; index < count; ++index) {
        const std::uint8_t* source = bytes + index * resident_instance_stride;
        auto& instance = (*instances)[index];
        instance.mesh_key = resident_u64(source);
        instance.object_id = resident_u32(source + 8);
        instance.model_pointer = resident_u32(source + 12);
        instance.transform_id = resident_u64(source + 16);
        for (int item = 0; item < 9; ++item)
            instance.rotation[item] = resident_i16(source + 24 + item * 2);
        for (int item = 0; item < 3; ++item)
            instance.translation[item] = resident_i32(source + 44 + item * 4);
        instance.projection_offset_x = resident_i32(source + 56);
        instance.projection_offset_y = resident_i32(source + 60);
        instance.projection_plane = resident_u32(source + 64);
        instance.clip_x0 = resident_i16(source + 68);
        instance.clip_y0 = resident_i16(source + 70);
        instance.clip_x1 = resident_i16(source + 72);
        instance.clip_y1 = resident_i16(source + 74);
        instance.texture_mask_x = resident_i16(source + 76);
        instance.texture_mask_y = resident_i16(source + 78);
        instance.texture_offset_x = resident_i16(source + 80);
        instance.texture_offset_y = resident_i16(source + 82);
        instance.draw_offset_x = resident_i16(source + 84);
        instance.draw_offset_y = resident_i16(source + 86);
        instance.environment_flags = resident_u32(source + 88);
        instance.default_texture_page = resident_u16(source + 92);
        instance.capture_insert_index = resident_u16(source + 94);
        instance.depth_scale_exponent = resident_i32(source + 96);
        instance.depth_scale_valid = (resident_u32(source + 100) & 1U) != 0;
        if (
            instance.mesh_key == 0 || instance.transform_id == 0 ||
            instance.projection_plane == 0 ||
            instance.clip_x1 < instance.clip_x0 ||
            instance.clip_y1 < instance.clip_y0
        ) return false;
    }
    return true;
}

struct BuiltFrame {
    opengt::render::WorldCaptureHeader header{};
    opengt::render::WorldTopologyStats topology{};
    std::uint64_t decode_microseconds{};
    std::uint64_t draw_list_microseconds{};
    std::uint64_t topology_microseconds{};
    std::uint64_t world_fingerprint{};
    Clock::time_point pipeline_started{};
};

struct LiveContext {
    std::vector<opengt::render::WorldCaptureTriangle> triangles;
    // Reusable merge storage retains authored capture/resident insertion
    // order without allocating another multi-megabyte triangle buffer on
    // every 60 Hz frame.
    std::vector<opengt::render::WorldCaptureTriangle>
        resident_interleaved_triangles;
    std::vector<std::uint16_t> vram;
    opengt::render::WorldDrawList draw_list;
    std::unordered_map<std::uint64_t, ResidentMesh> resident_meshes;
    std::vector<ResidentInstance> resident_instances;
    std::vector<std::size_t> resident_instance_triangle_counts;
#if defined(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT)
    opengt::render::WorldInterpolationCache interpolation_cache;
#endif

    bool has_previous{};
#if defined(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT)
    std::vector<std::uint16_t> previous_vram;
#endif
    opengt::render::WorldDrawList previous_draw_list;
#if defined(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT)
    opengt::render::WorldInterpolationCache previous_interpolation_cache;
#endif
    opengt::render::WorldCaptureHeader previous_header{};
#if defined(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT)
    opengt_live_stats previous_actual_stats{};
#endif
    opengt_live_options previous_options{};
#if defined(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT)
    // Mirror the asynchronous D3D staging capacity exactly so pixels and frame
    // identity can never drift.
    std::array<
        opengt_live_stats,
        opengt::render::world_gpu_async_readback_pair_capacity>
        pending_midpoint_stats{};
    std::array<
        opengt_live_stats,
        opengt::render::world_gpu_async_readback_pair_capacity>
        pending_actual_stats{};
    std::uint32_t pending_output_pair_read{};
    std::uint32_t pending_output_pair_write{};
    std::uint32_t pending_output_pair_count{};
#endif
    bool readback_software_adapter{};
    std::array<
        opengt_live_stats,
        opengt::render::world_gpu_async_readback_image_capacity>
        pending_authored_stats{};
    std::uint32_t pending_authored_read{};
    std::uint32_t pending_authored_write{};
    std::uint32_t pending_authored_count{};
    std::uint64_t classification_frames{};
    std::uint64_t classified_world_commands{};
    std::uint64_t unclassified_world_commands{};
    std::uint64_t frames_with_unclassified_world{};
    std::uint32_t maximum_unclassified_world_commands{};
    std::uint64_t resident_audit_frames{};
    std::uint64_t resident_audit_triangles{};
    std::uint64_t resident_audit_matched{};
    std::uint64_t resident_audit_unmatched{};
    std::uint64_t resident_audit_ordered{};
    std::uint64_t resident_audit_order_mismatched{};
    std::uint64_t resident_audit_material_order_mismatched{};
    std::uint32_t resident_audit_mismatch_traces{};
    std::uint64_t resident_lod_frames{};
    std::uint64_t resident_lod_textured_evaluations{};
    std::uint64_t resident_lod_authored_near{};
    std::uint64_t resident_lod_authored_distant{};
    std::uint64_t resident_lod_authored_switches{};
    std::uint32_t resident_lod_switch_traces{};
    std::unordered_map<std::uint64_t, bool> resident_lod_previous;

    LiveContext()
        : vram(1024U * 512U)
#if defined(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT)
        , previous_vram(1024U * 512U)
#endif
    {}
};

int32_t try_read_pending_authored(
    LiveContext* context,
    std::uint8_t* output,
    std::size_t output_capacity,
    opengt_live_stats* stats,
    bool wait_for_completion
) {
    if (context->pending_authored_count == 0)
        return 0;
    const auto result = opengt::render::try_read_world_d3d11_image(
        context->readback_software_adapter,
        output,
        output_capacity,
        wait_for_completion);
    if (result == opengt::render::WorldGpuReadbackResult::not_ready)
        return 0;
    if (result != opengt::render::WorldGpuReadbackResult::success)
        return -static_cast<std::int32_t>(
            700U + static_cast<std::uint32_t>(result));
    const std::uint32_t read = context->pending_authored_read;
    *stats = context->pending_authored_stats[read];
    if (output_fingerprint_enabled()) {
        const std::uint64_t output_size =
            static_cast<std::uint64_t>(stats->output_width) *
            stats->output_height * 4U;
        if (output_size > output_capacity)
            return -706;
        stats->output_fingerprint = fingerprint_output(
            output,
            static_cast<std::size_t>(output_size));
    }
    context->pending_authored_read =
        (read + 1U) % context->pending_authored_stats.size();
    --context->pending_authored_count;
    return 1;
}

template<typename T>
void clear_struct(T* value) {
    if (value == nullptr)
        return;
    const std::uint32_t size = value->struct_size;
    *value = {};
    value->struct_size = size;
}

int32_t fail(opengt_live_stats* stats, std::uint32_t result) {
    if (stats != nullptr)
        stats->result = result;
    return -static_cast<std::int32_t>(result);
}

#if defined(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT)
int32_t fail_pair(
    opengt_live_stats* first,
    opengt_live_stats* second,
    opengt_live_interpolation_stats* interpolation,
    std::uint32_t result
) {
    if (first != nullptr)
        first->result = result;
    if (second != nullptr)
        second->result = result;
    if (interpolation != nullptr)
        interpolation->result = result;
    return -static_cast<std::int32_t>(result);
}

int32_t try_read_pending_pair(
    LiveContext* context,
    std::uint8_t* first_output,
    std::uint8_t* second_output,
    std::size_t output_capacity,
    opengt_live_stats* first_stats,
    opengt_live_stats* second_stats,
    bool wait_for_completion
) {
    if (context->pending_output_pair_count == 0)
        return 0;
    const auto result = opengt::render::try_read_world_d3d11_pair(
        context->readback_software_adapter,
        first_output,
        second_output,
        output_capacity,
        wait_for_completion);
    if (result == opengt::render::WorldGpuReadbackResult::not_ready)
        return 0;
    if (result != opengt::render::WorldGpuReadbackResult::success)
        return -static_cast<std::int32_t>(
            700U + static_cast<std::uint32_t>(result));
    const std::uint32_t read = context->pending_output_pair_read;
    *first_stats = context->pending_midpoint_stats[read];
    *second_stats = context->pending_actual_stats[read];
    context->pending_output_pair_read =
        (read + 1U) % context->pending_midpoint_stats.size();
    --context->pending_output_pair_count;
    return 1;
}
#endif

std::uint64_t microseconds(Clock::duration duration) {
    return static_cast<std::uint64_t>(
        std::chrono::duration_cast<std::chrono::microseconds>(
            duration).count());
}

struct ResidentViewVertex {
    const ResidentVertex* source{};
    std::int64_t x_fixed{};
    std::int64_t y_fixed{};
    std::int64_t z_fixed{};
    std::int32_t x{};
    std::int32_t y{};
    std::int32_t z{};
    opengt::render::WorldCaptureVertex capture{};
};

std::uint32_t resident_divide(
    std::uint32_t h,
    std::uint32_t depth
) noexcept {
    if (h >= depth * 2U)
        return 0x1FFFF;
    int shift = 0;
    std::uint32_t value = depth;
    while ((value & 0x8000U) == 0U) {
        value <<= 1;
        ++shift;
    }
    std::uint64_t numerator = static_cast<std::uint64_t>(h) << shift;
    std::uint64_t denominator =
        static_cast<std::uint64_t>(depth) << shift;
    int index = static_cast<int>((denominator - 0x7FC0) >> 7);
    index = std::clamp(index, 0, 0x100);
    const int unr = std::clamp(
        (0x40000 / (index + 0x100) + 1) / 2 - 0x101,
        0,
        0xFF);
    std::uint64_t reciprocal = static_cast<std::uint64_t>(unr) + 0x101;
    denominator = (0x2000080ULL - denominator * reciprocal) >> 8;
    denominator = (0x0000080ULL + denominator * reciprocal) >> 8;
    const std::uint64_t result =
        (numerator * denominator + 0x8000) >> 16;
    return static_cast<std::uint32_t>(
        std::min<std::uint64_t>(result, 0x1FFFF));
}

std::array<std::int32_t, 2> resident_gte_screen(
    const ResidentViewVertex& vertex,
    const ResidentInstance& instance
) noexcept {
    const std::uint16_t depth = static_cast<std::uint16_t>(
        std::clamp(vertex.z, 0, 0xFFFF));
    const std::uint32_t quotient =
        resident_divide(instance.projection_plane, depth);
    const std::int32_t ir1 = std::clamp(vertex.x, -0x8000, 0x7FFF);
    const std::int32_t ir2 = std::clamp(vertex.y, -0x8000, 0x7FFF);
    const std::int64_t projected_x =
        static_cast<std::int64_t>(quotient) * ir1 +
        instance.projection_offset_x;
    const std::int64_t projected_y =
        static_cast<std::int64_t>(quotient) * ir2 +
        instance.projection_offset_y;
    return {
        static_cast<std::int32_t>(std::clamp<std::int64_t>(
            projected_x >> 16, -0x400, 0x3FF)),
        static_cast<std::int32_t>(std::clamp<std::int64_t>(
            projected_y >> 16, -0x400, 0x3FF)),
    };
}

std::int64_t resident_nclip(
    const ResidentViewVertex& a,
    const ResidentViewVertex& b,
    const ResidentViewVertex& c,
    const ResidentInstance& instance
) noexcept {
    const auto pa = resident_gte_screen(a, instance);
    const auto pb = resident_gte_screen(b, instance);
    const auto pc = resident_gte_screen(c, instance);
    return
        static_cast<std::int64_t>(pa[0]) * pb[1] +
        static_cast<std::int64_t>(pb[0]) * pc[1] +
        static_cast<std::int64_t>(pc[0]) * pa[1] -
        static_cast<std::int64_t>(pa[0]) * pc[1] -
        static_cast<std::int64_t>(pb[0]) * pa[1] -
        static_cast<std::int64_t>(pc[0]) * pb[1];
}

double resident_determinant(
    const ResidentViewVertex& a,
    const ResidentViewVertex& b,
    const ResidentViewVertex& c
) noexcept {
    return
        static_cast<double>(a.x_fixed) *
            (static_cast<double>(b.y_fixed) * c.z_fixed -
             static_cast<double>(b.z_fixed) * c.y_fixed) -
        static_cast<double>(a.y_fixed) *
            (static_cast<double>(b.x_fixed) * c.z_fixed -
             static_cast<double>(b.z_fixed) * c.x_fixed) +
        static_cast<double>(a.z_fixed) *
            (static_cast<double>(b.x_fixed) * c.y_fixed -
             static_cast<double>(b.y_fixed) * c.x_fixed);
}

struct ResidentClipPoint {
    double x{};
    double y{};
    double z{};
};

bool resident_near_clipped_accepted(
    const ResidentViewVertex& a,
    const ResidentViewVertex& b,
    const ResidentViewVertex& c,
    bool one_sided,
    bool accept_positive
) noexcept {
    constexpr double near_fixed = 16.0 * 4096.0;
    std::array<ResidentClipPoint, 4> input{{
        {static_cast<double>(a.x_fixed), static_cast<double>(a.y_fixed),
            static_cast<double>(a.z_fixed)},
        {static_cast<double>(b.x_fixed), static_cast<double>(b.y_fixed),
            static_cast<double>(b.z_fixed)},
        {static_cast<double>(c.x_fixed), static_cast<double>(c.y_fixed),
            static_cast<double>(c.z_fixed)},
        {},
    }};
    std::array<ResidentClipPoint, 4> output{};
    int input_count = 3;
    int output_count = 0;
    ResidentClipPoint previous = input[input_count - 1];
    bool previous_inside = previous.z >= near_fixed;
    for (int index = 0; index < input_count; ++index) {
        const ResidentClipPoint current = input[index];
        const bool current_inside = current.z >= near_fixed;
        if (current_inside != previous_inside) {
            const double t =
                (near_fixed - previous.z) / (current.z - previous.z);
            output[output_count++] = {
                previous.x + (current.x - previous.x) * t,
                previous.y + (current.y - previous.y) * t,
                near_fixed,
            };
        }
        if (current_inside)
            output[output_count++] = current;
        previous = current;
        previous_inside = current_inside;
    }
    if (output_count < 3)
        return false;
    for (int index = 1; index + 1 < output_count; ++index) {
        const ResidentClipPoint& p0 = output[0];
        const ResidentClipPoint& p1 = output[index];
        const ResidentClipPoint& p2 = output[index + 1];
        const double determinant =
            p0.x * (p1.y * p2.z - p1.z * p2.y) -
            p0.y * (p1.x * p2.z - p1.z * p2.x) +
            p0.z * (p1.x * p2.y - p1.y * p2.x);
        if (
            one_sided
                ? accept_positive ? determinant > 0.0 : determinant < 0.0
                : determinant != 0.0
        ) return true;
    }
    return false;
}

bool resident_facing_accepted(
    const ResidentViewVertex& a,
    const ResidentViewVertex& b,
    const ResidentViewVertex& c,
    bool one_sided,
    bool accept_positive
) noexcept {
    const double determinant = resident_determinant(a, b, c);
    const bool preclip = one_sided
        ? accept_positive ? determinant > 0.0 : determinant < 0.0
        : determinant != 0.0;
    const bool has_front =
        a.z_fixed >= 16LL * 4096 ||
        b.z_fixed >= 16LL * 4096 ||
        c.z_fixed >= 16LL * 4096;
    const bool has_behind =
        a.z_fixed < 16LL * 4096 ||
        b.z_fixed < 16LL * 4096 ||
        c.z_fixed < 16LL * 4096;
    return has_front && has_behind
        ? resident_near_clipped_accepted(
            a, b, c, one_sided, accept_positive)
        : preclip;
}

std::uint32_t resident_material_coverage(
    const ResidentPrimitive& primitive,
    const std::vector<ResidentViewVertex>& view,
    const ResidentInstance& instance
) noexcept {
    const auto& i = primitive.indices;
    if ((primitive.flags & resident_primitive_quad) == 0) {
        return static_cast<std::uint32_t>(resident_nclip(
            view[i[0]], view[i[2]], view[i[1]], instance));
    }
    // Match the guest GTE FIFO exactly. The first NCLIP is cyclically
    // equivalent to authored 0,1,2. Pushing corner 3 produces 2,0,3 for the
    // second NCLIP; GT2 compares abs(second - first) with the LOD threshold.
    const std::int32_t first = static_cast<std::int32_t>(resident_nclip(
        view[i[0]], view[i[1]], view[i[2]], instance));
    const std::int32_t second = static_cast<std::int32_t>(resident_nclip(
        view[i[2]], view[i[0]], view[i[3]], instance));
    std::uint32_t combined =
        static_cast<std::uint32_t>(second) -
        static_cast<std::uint32_t>(first);
    if (static_cast<std::int32_t>(combined) < 0)
        combined = 0U - combined;
    return combined;
}

void resident_calculate_world(
    const opengt::render::WorldCaptureHeader& header,
    opengt::render::WorldCaptureVertex* vertex
) noexcept {
    const float x = static_cast<float>(
        vertex->view_x - header.camera_translation[0]);
    const float y = static_cast<float>(
        vertex->view_y - header.camera_translation[1]);
    const float z = static_cast<float>(
        vertex->view_z - header.camera_translation[2]);
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

void resident_fill_common_vertex(
    const opengt::render::WorldCaptureHeader& header,
    const ResidentViewVertex& view,
    const ResidentInstance& instance,
    opengt::render::WorldCaptureVertex* destination
) noexcept {
    *destination = {};
    destination->world_valid = true;
    destination->screen_offset_anchor = false;
    destination->model_x = view.source->x;
    destination->model_y = view.source->y;
    destination->model_z = view.source->z;
    destination->view_x = view.x;
    destination->view_y = view.y;
    destination->view_z = view.z;
    destination->screen_z = static_cast<float>(view.z);
    destination->projection_offset_x = instance.projection_offset_x;
    destination->projection_offset_y = instance.projection_offset_y;
    destination->projection_plane = instance.projection_plane;
    destination->source_vertex_identity = view.source->source_identity;
    destination->transform_id = instance.transform_id;
    for (int index = 0; index < 9; ++index)
        destination->transform_rotation[index] = instance.rotation[index];
    for (int index = 0; index < 3; ++index)
        destination->transform_translation[index] = instance.translation[index];
    destination->exact_transform_valid = true;
    resident_calculate_world(header, destination);
}

void resident_fill_vertex(
    const ResidentViewVertex& view,
    const ResidentMaterial& material,
    int material_corner,
    int uv_corner,
    opengt::render::WorldCaptureVertex* destination
) noexcept {
    *destination = view.capture;
    destination->u = static_cast<std::int16_t>(
        material.uv[uv_corner] & 0xFFU);
    destination->v = static_cast<std::int16_t>(
        material.uv[uv_corner] >> 8);
    const std::uint32_t color = material.color[material_corner];
    destination->r = static_cast<std::uint8_t>(color);
    destination->g = static_cast<std::uint8_t>(color >> 8);
    destination->b = static_cast<std::uint8_t>(color >> 16);
}

void resident_emit_capture_triangle(
    const ResidentPrimitive& primitive,
    const ResidentMaterial& material,
    const ResidentInstance& instance,
    const opengt::render::WorldCaptureVertex& a,
    const opengt::render::WorldCaptureVertex& b,
    const opengt::render::WorldCaptureVertex& c,
    std::vector<opengt::render::WorldCaptureTriangle>* triangles
) {
    using namespace opengt::render;
    WorldCaptureTriangle triangle{};
    triangle.primitive_flags = world_primitive_resident_course_flag;
    triangle.primitive_flags |=
        (static_cast<std::uint32_t>(primitive.overlay_layer) <<
            world_primitive_track_overlay_layer_shift) &
        world_primitive_track_overlay_layer_mask;
    if (primitive.overlay_support)
        triangle.primitive_flags |=
            world_primitive_track_overlay_support_flag;
    if (primitive.replacement_surface)
        triangle.primitive_flags |=
            world_primitive_track_replacement_flag;
    if ((primitive.flags & resident_primitive_textured) != 0)
        triangle.primitive_flags |= 1U << 0;
    if ((primitive.flags & resident_primitive_semi_transparent) != 0)
        triangle.primitive_flags |= 1U << 1;
    if ((primitive.flags & resident_primitive_raw_texture) != 0)
        triangle.primitive_flags |= 1U << 2;
    if ((primitive.flags & resident_primitive_gouraud) != 0)
        triangle.primitive_flags |= 1U << 3;
    triangle.texture_page = material.texture_page;
    triangle.clut = material.clut;
    triangle.ordering_table_index = 0;
    triangle.clip_x0 = instance.clip_x0;
    triangle.clip_y0 = instance.clip_y0;
    triangle.clip_x1 = instance.clip_x1;
    triangle.clip_y1 = instance.clip_y1;
    triangle.texture_mask_x = instance.texture_mask_x;
    triangle.texture_mask_y = instance.texture_mask_y;
    triangle.texture_offset_x = instance.texture_offset_x;
    triangle.texture_offset_y = instance.texture_offset_y;
    triangle.environment_flags = instance.environment_flags;
    triangle.object_kind = 1;
    triangle.object_id = instance.object_id;
    triangle.model_pointer = instance.model_pointer;
    triangle.draw_offset_x = instance.draw_offset_x;
    triangle.draw_offset_y = instance.draw_offset_y;
    triangle.transform_id = instance.transform_id;
    for (int index = 0; index < 9; ++index)
        triangle.transform_rotation[index] = instance.rotation[index];
    for (int index = 0; index < 3; ++index)
        triangle.transform_translation[index] = instance.translation[index];
    triangle.exact_transform_valid =
        a.exact_transform_valid &&
        b.exact_transform_valid &&
        c.exact_transform_valid;
    triangle.depth_scale_exponent = instance.depth_scale_exponent;
    triangle.depth_scale_valid = instance.depth_scale_valid;
    triangle.vertices[0] = a;
    triangle.vertices[1] = b;
    triangle.vertices[2] = c;
    triangles->push_back(triangle);
}

ResidentViewVertex resident_emission_vertex(
    const ResidentViewVertex& view,
    const ResidentMaterial& material,
    int material_corner,
    int uv_corner
) noexcept {
    ResidentViewVertex result = view;
    resident_fill_vertex(
        view,
        material,
        material_corner,
        uv_corner,
        &result.capture);
    return result;
}

std::size_t resident_emit_triangle(
    const opengt::render::WorldCaptureHeader& header,
    const ResidentPrimitive& primitive,
    const ResidentMaterial& material,
    const ResidentInstance& instance,
    const ResidentViewVertex& a,
    const ResidentViewVertex& b,
    const ResidentViewVertex& c,
    int material_a,
    int material_b,
    int material_c,
    int uv_a,
    int uv_b,
    int uv_c,
    std::vector<opengt::render::WorldCaptureTriangle>* triangles
) {
    const ResidentViewVertex emission_a = resident_emission_vertex(
        a, material, material_a, uv_a);
    const ResidentViewVertex emission_b = resident_emission_vertex(
        b, material, material_b, uv_b);
    const ResidentViewVertex emission_c = resident_emission_vertex(
        c, material, material_c, uv_c);
    resident_emit_capture_triangle(
        primitive,
        material,
        instance,
        emission_a.capture,
        emission_b.capture,
        emission_c.capture,
        triangles);
    return 1;
}

std::uint32_t append_resident_course(
    LiveContext* context,
    const opengt::render::WorldCaptureHeader& header
) {
    std::uint32_t traced_model = 0;
    if (const char* configured = std::getenv(
            "OPENGT_TRACE_RESIDENT_MODEL")) {
        traced_model = static_cast<std::uint32_t>(
            std::strtoul(configured, nullptr, 0));
    }
    std::int32_t traced_model_poll = -1;
    if (const char* configured = std::getenv(
            "OPENGT_TRACE_RESIDENT_MODEL_POLL")) {
        traced_model_poll = static_cast<std::int32_t>(
            std::strtol(configured, nullptr, 10));
    }
    const bool trace_model_primitives = [] {
        const char* configured = std::getenv(
            "OPENGT_TRACE_RESIDENT_MODEL_PRIMITIVES");
        return configured != nullptr && std::strcmp(configured, "1") == 0;
    }();
    std::uint32_t traced_primitive = 0;
    if (const char* configured = std::getenv(
            "OPENGT_TRACE_RESIDENT_PRIMITIVE")) {
        traced_primitive = static_cast<std::uint32_t>(
            std::strtoul(configured, nullptr, 0));
    }
    std::int32_t traced_poll = -1;
    if (const char* configured = std::getenv(
            "OPENGT_TRACE_RESIDENT_PRIMITIVE_POLL")) {
        traced_poll = static_cast<std::int32_t>(
            std::strtol(configured, nullptr, 10));
    }
    std::size_t possible_triangles = 0;
    for (const auto& instance : context->resident_instances) {
        const auto found = context->resident_meshes.find(instance.mesh_key);
        if (found == context->resident_meshes.end())
            return 1;
        for (const auto& primitive : found->second.primitives) {
            possible_triangles +=
                (primitive.flags & resident_primitive_quad) != 0 ? 2 : 1;
        }
    }
    context->triangles.reserve(context->triangles.size() + possible_triangles);
    context->resident_instance_triangle_counts.clear();
    context->resident_instance_triangle_counts.reserve(
        context->resident_instances.size());
    if (!context->resident_instances.empty())
        ++context->resident_lod_frames;
    // The largest registered mesh is bounded to 1,024 vertices. Preserve the
    // transform scratch allocation across frames instead of rebuilding it for
    // every resident-course expansion.
    static thread_local std::vector<ResidentViewVertex> view;
    for (const auto& instance : context->resident_instances) {
        const std::size_t instance_triangle_start =
            context->triangles.size();
        const ResidentMesh& mesh = context->resident_meshes.at(instance.mesh_key);
        if (
            traced_model != 0 && instance.model_pointer == traced_model &&
            (traced_model_poll < 0 || header.input_poll == traced_model_poll)
        ) {
            std::fprintf(
                stderr,
                "[Native-Resident-Model] frame=%llu poll=%u object=%08x "
                "model=%08x mesh=%016llx primitives=%zu overlays=%u "
                "untexturedOverlays=%u replacements=%u clip=%d,%d..%d,%d "
                "t=%d,%d,%d depthScale=%d/%u r=%d,%d,%d/%d,%d,%d/%d,%d,%d\n",
                static_cast<unsigned long long>(header.frame_index),
                header.input_poll,
                instance.object_id,
                instance.model_pointer,
                static_cast<unsigned long long>(instance.mesh_key),
                mesh.primitives.size(),
                mesh.overlay_primitives,
                mesh.untextured_overlay_primitives,
                mesh.replacement_primitives,
                instance.clip_x0,
                instance.clip_y0,
                instance.clip_x1,
                instance.clip_y1,
                instance.translation[0],
                instance.translation[1],
                instance.translation[2],
                instance.depth_scale_exponent,
                instance.depth_scale_valid ? 1U : 0U,
                instance.rotation[0],
                instance.rotation[1],
                instance.rotation[2],
                instance.rotation[3],
                instance.rotation[4],
                instance.rotation[5],
                instance.rotation[6],
                instance.rotation[7],
                instance.rotation[8]);
        }
        view.resize(mesh.vertices.size());
        for (std::size_t index = 0; index < mesh.vertices.size(); ++index) {
            const auto& source = mesh.vertices[index];
            auto& destination = view[index];
            destination.source = &source;
            destination.x_fixed =
                (static_cast<std::int64_t>(instance.translation[0]) << 12) +
                static_cast<std::int64_t>(instance.rotation[0]) * source.x +
                static_cast<std::int64_t>(instance.rotation[1]) * source.y +
                static_cast<std::int64_t>(instance.rotation[2]) * source.z;
            destination.y_fixed =
                (static_cast<std::int64_t>(instance.translation[1]) << 12) +
                static_cast<std::int64_t>(instance.rotation[3]) * source.x +
                static_cast<std::int64_t>(instance.rotation[4]) * source.y +
                static_cast<std::int64_t>(instance.rotation[5]) * source.z;
            destination.z_fixed =
                (static_cast<std::int64_t>(instance.translation[2]) << 12) +
                static_cast<std::int64_t>(instance.rotation[6]) * source.x +
                static_cast<std::int64_t>(instance.rotation[7]) * source.y +
                static_cast<std::int64_t>(instance.rotation[8]) * source.z;
            destination.x = static_cast<std::int32_t>(destination.x_fixed >> 12);
            destination.y = static_cast<std::int32_t>(destination.y_fixed >> 12);
            destination.z = static_cast<std::int32_t>(destination.z_fixed >> 12);
            resident_fill_common_vertex(
                header, destination, instance, &destination.capture);
        }

        const bool trace_this_instance =
            traced_model != 0 && instance.model_pointer == traced_model &&
            (traced_model_poll < 0 || header.input_poll == traced_model_poll);
        std::size_t traced_flare_primitives = 0;
        std::size_t traced_flare_triangles = 0;
        for (const auto& primitive : mesh.primitives) {
            const auto& i = primitive.indices;
            const bool one_sided =
                (primitive.flags & resident_primitive_one_sided) != 0;
            const std::uint32_t coverage = resident_material_coverage(
                primitive, view, instance);
            const bool authored_distant =
                primitive.lod_threshold >= coverage;
            const bool textured =
                (primitive.flags & resident_primitive_textured) != 0;
            ResidentMaterial material = authored_distant
                ? primitive.distant_material
                : primitive.near_material;
            if (textured) {
                ++context->resident_lod_textured_evaluations;
                if (authored_distant) {
                    ++context->resident_lod_authored_distant;
                } else {
                    ++context->resident_lod_authored_near;
                }

                if (resident_lod_diagnostics_enabled()) {
                    const std::uint64_t key =
                        (static_cast<std::uint64_t>(instance.object_id) << 32) |
                        primitive.source_address;
                    const auto inserted = context->resident_lod_previous.emplace(
                        key, authored_distant);
                    if (!inserted.second &&
                        inserted.first->second != authored_distant) {
                        ++context->resident_lod_authored_switches;
                        if (context->resident_lod_switch_traces < 64) {
                            std::fprintf(
                                stderr,
                                "[Native-Resident-LOD-Switch] frame=%llu "
                                "poll=%d object=%u primitive=%08x "
                                "threshold=%u coverage=%u authored=%s "
                                "rendered=%s\n",
                                static_cast<unsigned long long>(
                                    header.frame_index),
                                header.input_poll,
                                instance.object_id,
                                primitive.source_address,
                                static_cast<unsigned>(
                                    primitive.lod_threshold),
                                coverage,
                                authored_distant ? "far" : "near",
                                authored_distant ? "far" : "near");
                            ++context->resident_lod_switch_traces;
                        }
                        inserted.first->second = authored_distant;
                    }
                }
            }
            const bool traced_flare = trace_this_instance && textured &&
                material.texture_page == 0x0029U &&
                material.clut == 0x7F57U;
            traced_flare_primitives += traced_flare ? 1U : 0U;
            if (
                (trace_this_instance && trace_model_primitives) ||
                (traced_primitive != 0 &&
                    primitive.source_address == traced_primitive &&
                    (traced_poll < 0 || header.input_poll == traced_poll))
            ) {
                std::fprintf(
                    stderr,
                    "[Native-Resident-Primitive] frame=%llu poll=%d "
                    "object=%u model=%08x primitive=%08x stream=%u "
                    "flags=%08x indices=%u/%u/%u/%u "
                    "overlay=%u/%u threshold=%u coverage=%u "
                    "authored=%s "
                    "rendered=%s "
                    "depthExponent=%d view="
                    "%d,%d,%d/%d,%d,%d/%d,%d,%d/%d,%d,%d "
                    "near=page:%04x,clut:%04x,uv:%04x/%04x/%04x/%04x "
                    "far=page:%04x,clut:%04x,uv:%04x/%04x/%04x/%04x "
                    "nearRgb=%08x/%08x/%08x/%08x "
                    "farRgb=%08x/%08x/%08x/%08x\n",
                    static_cast<unsigned long long>(header.frame_index),
                    header.input_poll,
                    instance.object_id,
                    instance.model_pointer,
                    primitive.source_address,
                    static_cast<unsigned>(primitive.stream),
                    primitive.flags,
                    static_cast<unsigned>(i[0]),
                    static_cast<unsigned>(i[1]),
                    static_cast<unsigned>(i[2]),
                    static_cast<unsigned>(i[3]),
                    static_cast<unsigned>(primitive.overlay_layer),
                    primitive.overlay_support ? 1U : 0U,
                    static_cast<unsigned>(primitive.lod_threshold),
                    coverage,
                    authored_distant ? "far" : "near",
                    authored_distant ? "far" : "near",
                    instance.depth_scale_exponent,
                    view[i[0]].x, view[i[0]].y, view[i[0]].z,
                    view[i[1]].x, view[i[1]].y, view[i[1]].z,
                    view[i[2]].x, view[i[2]].y, view[i[2]].z,
                    view[i[3]].x, view[i[3]].y, view[i[3]].z,
                    primitive.near_material.texture_page,
                    primitive.near_material.clut,
                    primitive.near_material.uv[0],
                    primitive.near_material.uv[1],
                    primitive.near_material.uv[2],
                    primitive.near_material.uv[3],
                    primitive.distant_material.texture_page,
                    primitive.distant_material.clut,
                    primitive.distant_material.uv[0],
                    primitive.distant_material.uv[1],
                    primitive.distant_material.uv[2],
                    primitive.distant_material.uv[3],
                    primitive.near_material.color[0],
                    primitive.near_material.color[1],
                    primitive.near_material.color[2],
                    primitive.near_material.color[3],
                    primitive.distant_material.color[0],
                    primitive.distant_material.color[1],
                    primitive.distant_material.color[2],
                    primitive.distant_material.color[3]);
            }
            if ((primitive.flags & resident_primitive_textured) == 0)
                material.texture_page = instance.default_texture_page;
            if ((primitive.flags & resident_primitive_quad) == 0) {
                if (!resident_facing_accepted(
                        view[i[0]], view[i[1]], view[i[2]],
                        one_sided, false))
                    continue;
                resident_emit_triangle(
                    header, primitive, material, instance,
                    view[i[0]], view[i[1]], view[i[2]],
                    0, 1, 2,
                    0, 1, 2,
                    &context->triangles);
                traced_flare_triangles += traced_flare ? 1U : 0U;
                continue;
            }

            const bool primary =
                (primitive.flags & resident_primitive_primary_path) != 0;
            const auto first_corners =
                resident_quad_packet_corners(primary, 0);
            const auto second_corners =
                resident_quad_packet_corners(primary, 1);
            const bool first_accepted = resident_facing_accepted(
                view[i[first_corners[0]]],
                view[i[first_corners[1]]],
                view[i[first_corners[2]]],
                one_sided, primary);
            const bool second_accepted = resident_facing_accepted(
                view[i[second_corners[0]]],
                view[i[second_corners[1]]],
                view[i[second_corners[2]]],
                one_sided, !primary);
            // GT2 submits one GPU quad after either of its two NCLIP tests
            // accepts the authored primitive.  Keep both modern triangles in
            // that case.  Culling the halves independently creates triangular
            // holes in railings, foliage, and road surfaces; D3D homogeneous
            // clipping and perspective interpolation already solve the affine
            // and near-plane artifacts that motivated that older workaround.
            const bool quad_accepted = first_accepted || second_accepted;
            if (quad_accepted) {
                resident_emit_triangle(
                    header, primitive, material, instance,
                    view[i[first_corners[0]]],
                    view[i[first_corners[1]]],
                    view[i[first_corners[2]]],
                    first_corners[0], first_corners[1], first_corners[2],
                    first_corners[0], first_corners[1], first_corners[2],
                    &context->triangles);
                traced_flare_triangles += traced_flare ? 1U : 0U;
            }
            if (quad_accepted) {
                resident_emit_triangle(
                    header, primitive, material, instance,
                    view[i[second_corners[0]]],
                    view[i[second_corners[1]]],
                    view[i[second_corners[2]]],
                    second_corners[0], second_corners[1], second_corners[2],
                    second_corners[0], second_corners[1], second_corners[2],
                    &context->triangles);
                traced_flare_triangles += traced_flare ? 1U : 0U;
            }
        }
        if (trace_this_instance) {
            std::fprintf(
                stderr,
                "[Native-Resident-Model-Result] frame=%llu poll=%u "
                "object=%08x model=%08x flarePrimitives=%zu "
                "flareTriangles=%zu emittedTriangles=%zu\n",
                static_cast<unsigned long long>(header.frame_index),
                header.input_poll,
                instance.object_id,
                instance.model_pointer,
                traced_flare_primitives,
                traced_flare_triangles,
                context->triangles.size() - instance_triangle_start);
        }
        context->resident_instance_triangle_counts.push_back(
            context->triangles.size() - instance_triangle_start);
    }
    return 0;
}

bool interleave_resident_course(
    LiveContext* context,
    std::size_t captured_triangle_count
) {
    if (context->resident_instances.empty())
        return true;
    if (
        context->resident_instance_triangle_counts.size() !=
        context->resident_instances.size()
    ) return false;

    auto& merged = context->resident_interleaved_triangles;
    merged.clear();
    merged.reserve(context->triangles.size());
    std::size_t capture_cursor = 0;
    std::size_t resident_cursor = captured_triangle_count;
    for (std::size_t index = 0;
         index < context->resident_instances.size();
         ++index) {
        const std::size_t insert =
            context->resident_instances[index].capture_insert_index;
        const std::size_t resident_count =
            context->resident_instance_triangle_counts[index];
        if (
            insert < capture_cursor || insert > captured_triangle_count ||
            resident_cursor + resident_count > context->triangles.size()
        ) return false;
        merged.insert(
            merged.end(),
            context->triangles.begin() + capture_cursor,
            context->triangles.begin() + insert);
        merged.insert(
            merged.end(),
            context->triangles.begin() + resident_cursor,
            context->triangles.begin() + resident_cursor + resident_count);
        capture_cursor = insert;
        resident_cursor += resident_count;
    }
    if (resident_cursor != context->triangles.size())
        return false;
    merged.insert(
        merged.end(),
        context->triangles.begin() + capture_cursor,
        context->triangles.begin() + captured_triangle_count);
    if (merged.size() != context->triangles.size())
        return false;
    context->triangles.swap(merged);
    return true;
}

std::uint32_t build_frame(
    LiveContext* context,
    const std::uint8_t* capture_bytes,
    std::size_t capture_size,
    const opengt_live_options& options,
    BuiltFrame* built,
    bool apply_topology = true,
    bool repair_projected_topology = true
) {
    using namespace opengt::render;
    built->pipeline_started = Clock::now();
    WorldCaptureTriangle probe_triangle{};
    std::uint16_t probe_vram{};
    auto result = load_world_capture_memory(
        capture_bytes,
        capture_size,
        &built->header,
        &probe_triangle,
        0,
        &probe_vram,
        0);
    if (result != WorldCaptureReadResult::capacity_too_small)
        return 100U + static_cast<std::uint32_t>(result);
    context->triangles.resize(built->header.triangle_count);
    result = load_world_capture_memory(
        capture_bytes,
        capture_size,
        &built->header,
        context->triangles.data(),
        context->triangles.size(),
        context->vram.data(),
        context->vram.size());
    if (result != WorldCaptureReadResult::success)
        return 100U + static_cast<std::uint32_t>(result);
    const std::size_t expanded_triangle_count = context->triangles.size();
    const std::uint32_t resident_result = append_resident_course(
        context,
        built->header);
    if (resident_result != 0)
        return 180U + resident_result;
    if (
        resident_equivalence_audit_enabled() &&
        context->triangles.size() != expanded_triangle_count
    ) {
        std::unordered_map<std::uint64_t, std::uint32_t> expanded_hashes;
        std::unordered_map<std::uint64_t, std::uint32_t>
            expanded_geometry_hashes;
        std::unordered_map<
            std::uint64_t,
            const WorldCaptureTriangle*> expanded_geometry_examples;
        std::unordered_map<std::uint64_t, std::uint32_t>
            expanded_core_hashes;
        for (std::size_t index = 0; index < expanded_triangle_count; ++index) {
            const auto& triangle = context->triangles[index];
            if (
                triangle.object_kind == 1U &&
                (triangle.primitive_flags &
                    world_primitive_resident_course_flag) != 0
            ) {
                ++expanded_hashes[fingerprint_track_triangle(triangle)];
                const auto geometry_hash = fingerprint_track_triangle(
                    triangle, false, false);
                ++expanded_geometry_hashes[geometry_hash];
                expanded_geometry_examples.try_emplace(
                    geometry_hash,
                    &triangle);
                ++expanded_core_hashes[
                    fingerprint_track_triangle(triangle, true, false)];
            }
        }
        std::uint32_t matched{};
        std::uint32_t unmatched{};
        std::uint32_t geometry_matched{};
        std::uint32_t core_matched{};
        for (
            std::size_t index = expanded_triangle_count;
            index < context->triangles.size();
            ++index
        ) {
            const auto hash = fingerprint_track_triangle(
                context->triangles[index]);
            const auto found = expanded_hashes.find(hash);
            if (found != expanded_hashes.end() && found->second != 0) {
                --found->second;
                ++matched;
            } else {
                ++unmatched;
                if (context->resident_audit_mismatch_traces < 8) {
                    const auto geometry_hash = fingerprint_track_triangle(
                        context->triangles[index], false, false);
                    const auto example = expanded_geometry_examples.find(
                        geometry_hash);
                    if (example != expanded_geometry_examples.end()) {
                        const auto& resident = context->triangles[index];
                        const auto& expanded = *example->second;
                        ++context->resident_audit_mismatch_traces;
                        std::fprintf(
                            stderr,
                            "[Native-Resident-Mismatch] n=%u frame=%llu "
                            "object=%u model=%08X flags=%08X/%08X "
                            "page=%04X/%04X clut=%04X/%04X "
                            "env=%08X/%08X "
                            "depthScale=%d/%u,%d/%u "
                            "uv=%d,%d;%d,%d;%d,%d/"
                            "%d,%d;%d,%d;%d,%d "
                            "rgb=%u,%u,%u;%u,%u,%u;%u,%u,%u/"
                            "%u,%u,%u;%u,%u,%u;%u,%u,%u\n",
                            context->resident_audit_mismatch_traces,
                            static_cast<unsigned long long>(
                                built->header.frame_index),
                            resident.object_id,
                            resident.model_pointer,
                            resident.primitive_flags,
                            expanded.primitive_flags,
                            resident.texture_page,
                            expanded.texture_page,
                            resident.clut,
                            expanded.clut,
                            resident.environment_flags,
                            expanded.environment_flags,
                            resident.depth_scale_exponent,
                            resident.depth_scale_valid ? 1U : 0U,
                            expanded.depth_scale_exponent,
                            expanded.depth_scale_valid ? 1U : 0U,
                            resident.vertices[0].u,
                            resident.vertices[0].v,
                            resident.vertices[1].u,
                            resident.vertices[1].v,
                            resident.vertices[2].u,
                            resident.vertices[2].v,
                            expanded.vertices[0].u,
                            expanded.vertices[0].v,
                            expanded.vertices[1].u,
                            expanded.vertices[1].v,
                            expanded.vertices[2].u,
                            expanded.vertices[2].v,
                            resident.vertices[0].r,
                            resident.vertices[0].g,
                            resident.vertices[0].b,
                            resident.vertices[1].r,
                            resident.vertices[1].g,
                            resident.vertices[1].b,
                            resident.vertices[2].r,
                            resident.vertices[2].g,
                            resident.vertices[2].b,
                            expanded.vertices[0].r,
                            expanded.vertices[0].g,
                            expanded.vertices[0].b,
                            expanded.vertices[1].r,
                            expanded.vertices[1].g,
                            expanded.vertices[1].b,
                            expanded.vertices[2].r,
                            expanded.vertices[2].g,
                            expanded.vertices[2].b);
                    }
                }
            }
            const auto geometry_hash = fingerprint_track_triangle(
                context->triangles[index], false, false);
            const auto geometry_found = expanded_geometry_hashes.find(
                geometry_hash);
            if (
                geometry_found != expanded_geometry_hashes.end() &&
                geometry_found->second != 0
            ) {
                --geometry_found->second;
                ++geometry_matched;
            }
            const auto core_hash = fingerprint_track_triangle(
                context->triangles[index], true, false);
            const auto core_found = expanded_core_hashes.find(core_hash);
            if (
                core_found != expanded_core_hashes.end() &&
                core_found->second != 0
            ) {
                --core_found->second;
                ++core_matched;
            }
        }
        std::uint32_t ordered{};
        std::uint32_t order_mismatched{};
        std::uint32_t material_order_mismatched{};
        std::size_t resident_cursor = expanded_triangle_count;
        for (std::size_t instance_index = 0;
             instance_index < context->resident_instances.size();
             ++instance_index) {
            const std::size_t insert = context->resident_instances[
                instance_index].capture_insert_index;
            const std::size_t count = context->resident_instance_triangle_counts[
                instance_index];
            if (
                insert + count > expanded_triangle_count ||
                resident_cursor + count > context->triangles.size()
            ) {
                order_mismatched += static_cast<std::uint32_t>(count);
                resident_cursor += count;
                continue;
            }
            for (std::size_t triangle_index = 0;
                 triangle_index < count;
                 ++triangle_index) {
                const auto expanded_hash = fingerprint_track_triangle(
                    context->triangles[insert + triangle_index]);
                const auto resident_hash = fingerprint_track_triangle(
                    context->triangles[resident_cursor + triangle_index]);
                if (expanded_hash != resident_hash)
                    ++material_order_mismatched;
                const auto expanded_geometry_hash =
                    fingerprint_track_triangle(
                        context->triangles[insert + triangle_index],
                        false,
                        false);
                const auto resident_geometry_hash =
                    fingerprint_track_triangle(
                        context->triangles[resident_cursor + triangle_index],
                        false,
                        false);
                if (expanded_geometry_hash == resident_geometry_hash)
                    ++ordered;
                else
                    ++order_mismatched;
            }
            resident_cursor += count;
        }
        if (resident_cursor != context->triangles.size())
            ++order_mismatched;
        const std::uint32_t resident_triangles = matched + unmatched;
        ++context->resident_audit_frames;
        context->resident_audit_triangles += resident_triangles;
        context->resident_audit_matched += matched;
        context->resident_audit_unmatched += unmatched;
        context->resident_audit_ordered += ordered;
        context->resident_audit_order_mismatched += order_mismatched;
        context->resident_audit_material_order_mismatched +=
            material_order_mismatched;
        std::fprintf(
            stderr,
            "[Native-Resident-Audit] frame=%llu poll=%d "
            "expanded=%zu resident=%u geometry=%u core=%u "
            "matched=%u unmatched=%u geometryOrdered=%u "
            "geometryOrderMismatch=%u materialOrderMismatch=%u\n",
            static_cast<unsigned long long>(built->header.frame_index),
            built->header.input_poll,
            expanded_triangle_count,
            resident_triangles,
            geometry_matched,
            core_matched,
            matched,
            unmatched,
            ordered,
            order_mismatched,
            material_order_mismatched);
        // Renderer-only road-overlay annotations are normalized by the
        // fingerprint. Everything else -- geometry, order, material, UV, and
        // color -- must be byte-equivalent to the managed packet expansion.
        if (order_mismatched != 0 || material_order_mismatched != 0)
            return 183U;
        context->triangles.resize(expanded_triangle_count);
    } else if (!interleave_resident_course(
                   context,
                   expanded_triangle_count)) {
        return 182U;
    }
    built->header.triangle_count = static_cast<std::uint32_t>(
        context->triangles.size());
    const auto decoded = Clock::now();

#if defined(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT)
    context->interpolation_cache.reset();
#endif
    const auto list_result = build_world_draw_list(
        built->header,
        context->triangles.data(),
        context->triangles.size(),
        WorldDrawListOptions{
            true,
            true,
            (options.flags & OPENGT_LIVE_TOPOLOGY) != 0,
            true},
        &context->draw_list);
    if (list_result != WorldDrawListResult::success)
        return 200U + static_cast<std::uint32_t>(list_result);
    const auto draw_list_built = Clock::now();

    if (apply_topology && (options.flags & OPENGT_LIVE_TOPOLOGY) != 0) {
        const auto topology_result = apply_world_topology(
            &context->draw_list,
            WorldTopologyOptions{
                true,
                true,
                true,
                repair_projected_topology,
                // Topology is rebuilt independently every authored frame.
                // Repair the one-pixel raster-visible boundary; spending the
                // 192-pixel diagnostic margin cannot affect this frame's
                // visible seam and consumed most of the 59.94 Hz budget.
                false},
            &built->topology);
        if (topology_result != WorldTopologyResult::success)
            return 300U + static_cast<std::uint32_t>(topology_result);
    }
    const auto topology_finished = Clock::now();
    if (world_fingerprint_enabled()) {
        built->world_fingerprint = fingerprint_world_draw_list(
            context->draw_list);
    }
    built->decode_microseconds = microseconds(
        decoded - built->pipeline_started);
    built->draw_list_microseconds = microseconds(
        draw_list_built - decoded);
    built->topology_microseconds = microseconds(
        topology_finished - draw_list_built);
    return 0;
}

opengt::render::WorldGpuRenderOptions gpu_options(
    const opengt_live_options& options
) {
    auto result = opengt::render::WorldGpuRenderOptions{
        (options.flags & OPENGT_LIVE_WARP) != 0,
        (options.flags & OPENGT_LIVE_DEPTH) != 0,
        (options.flags & OPENGT_LIVE_DITHER) != 0,
        (options.flags & OPENGT_LIVE_PERSPECTIVE) != 0,
        (options.flags & OPENGT_LIVE_TEXTURE_SMOOTHING) != 0,
        (options.flags & OPENGT_LIVE_HIGH_RESOLUTION_TEXTURES) != 0,
        options.output_scale,
        options.clear_color_rgba8,
    };
    result.target_aspect_width = options.target_aspect_width;
    result.target_aspect_height = options.target_aspect_height;
    result.direct_gpu_output =
        (options.flags & OPENGT_LIVE_DIRECT_GPU_OUTPUT) != 0;
    result.direct_output_slot = options.direct_output_slot;
    return result;
}

void fill_stats(
    const BuiltFrame& built,
    const opengt::render::WorldDrawList& draw_list,
    const opengt::render::WorldGpuRenderStats& render,
    std::uint64_t render_microseconds,
    std::uint64_t pipeline_microseconds,
    const opengt::render::WorldGpuRenderOptions& options,
    opengt_live_stats* stats
) {
    stats->result = 0;
    stats->capture_triangles = built.header.triangle_count;
    stats->output_commands = static_cast<std::uint32_t>(
        draw_list.commands.size());
    stats->draw_calls = render.draw_calls;
    stats->transparent_draw_calls = render.transparent_draw_calls;
    stats->topology_boundary_groups =
        built.topology.authored_boundary_groups;
    stats->topology_t_junctions =
        built.topology.exact_t_junctions +
        built.topology.projected_t_junctions;
    stats->topology_split_triangles =
        built.topology.emitted_split_triangles;
    stats->topology_coplanar_pairs =
        built.topology.coplanar_overlap_pairs;
    stats->topology_ownership_reorders =
        built.topology.ownership_reorders;
    stats->output_width =
        opengt::render::world_gpu_target_display_width(draw_list, options) *
        options.output_scale;
    stats->output_height =
        static_cast<std::uint32_t>(draw_list.display_height) *
        options.output_scale;
    stats->render_microseconds = render_microseconds;
    stats->frame_index = built.header.frame_index;
    stats->input_poll = built.header.input_poll;
    stats->decode_microseconds = built.decode_microseconds;
    stats->draw_list_microseconds = built.draw_list_microseconds;
    stats->topology_microseconds = built.topology_microseconds;
    stats->pipeline_microseconds = pipeline_microseconds;
    stats->world_fingerprint = built.world_fingerprint;
    stats->output_texture = reinterpret_cast<std::uintptr_t>(
        render.output_texture);
}

std::uint32_t render_frame(
    const BuiltFrame& built,
    const opengt::render::WorldDrawList& draw_list,
    const std::uint16_t* vram,
    std::size_t vram_size,
    std::uint8_t* output,
    std::size_t output_capacity,
    const opengt_live_options& options,
    opengt_live_stats* stats,
    bool reuse_uploaded_vram = false,
    bool reuse_uploaded_materials = false,
    bool synthetic_midpoint = false,
    bool defer_initial_readback = false,
    bool asynchronous_readback = false,
    bool* output_valid = nullptr
) {
    const auto gpu_started = Clock::now();
    opengt::render::WorldGpuRenderStats render{};
    auto render_options = gpu_options(options);
    render_options.reuse_uploaded_vram = reuse_uploaded_vram;
    render_options.reuse_uploaded_materials = reuse_uploaded_materials;
    render_options.synthetic_midpoint = synthetic_midpoint;
    render_options.defer_initial_readback = defer_initial_readback;
    render_options.asynchronous_readback = asynchronous_readback;
    const auto result = opengt::render::render_world_d3d11(
        draw_list,
        vram,
        vram_size,
        output,
        output_capacity,
        render_options,
        &render);
    const auto gpu_finished = Clock::now();
    if (result != opengt::render::WorldGpuRenderResult::success)
        return 400U + static_cast<std::uint32_t>(result);
    if (output_valid != nullptr)
        *output_valid = render.output_valid;
    fill_stats(
        built,
        draw_list,
        render,
        microseconds(gpu_finished - gpu_started),
        microseconds(gpu_finished - built.pipeline_started),
        render_options,
        stats);
    if (
        output_fingerprint_enabled() && !asynchronous_readback &&
        render.output_valid
    ) {
        const std::uint64_t output_size =
            static_cast<std::uint64_t>(stats->output_width) *
            stats->output_height * 4U;
        if (output_size > output_capacity)
            return 706U;
        stats->output_fingerprint = fingerprint_output(
            output,
            static_cast<std::size_t>(output_size));
    }
    return 0;
}

#if defined(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT)
void fill_deferred_reset_stats(
    const BuiltFrame& current,
    const opengt::render::WorldDrawList& draw_list,
    const opengt_live_options& options,
    Clock::time_point pair_started,
    opengt_live_stats* stats
) {
    opengt::render::WorldGpuRenderStats render{};
    render.commands = static_cast<std::uint32_t>(draw_list.commands.size());
    render.secondary_commands = draw_list.secondary_commands;
    fill_stats(
        current,
        draw_list,
        render,
        0,
        microseconds(Clock::now() - pair_started),
        gpu_options(options),
        stats);
}

bool midpoint_uploads_match_previous(
    const opengt::render::WorldDrawList& previous,
    const opengt::render::WorldDrawList& midpoint
) {
    using opengt::render::WorldMaterial;
    if (previous.commands.size() != midpoint.commands.size())
        return false;

    const auto same_material = [](
        const WorldMaterial& left,
        const WorldMaterial& right
    ) {
        return
            left.primitive_flags == right.primitive_flags &&
            left.texture_page == right.texture_page &&
            left.clut == right.clut &&
            left.texture_mask_x == right.texture_mask_x &&
            left.texture_mask_y == right.texture_mask_y &&
            left.texture_offset_x == right.texture_offset_x &&
            left.texture_offset_y == right.texture_offset_y &&
            left.environment_flags == right.environment_flags;
    };

    for (std::size_t index = 0; index < previous.commands.size(); ++index) {
        const auto& left = previous.commands[index];
        const auto& right = midpoint.commands[index];
        if (
            left.material_index >= previous.materials.size() ||
            right.material_index >= midpoint.materials.size() ||
            left.object_kind != right.object_kind ||
            !same_material(
                previous.materials[left.material_index],
                midpoint.materials[right.material_index])
        )
            return false;
        for (int vertex = 0; vertex < 3; ++vertex) {
            if (
                left.vertices[vertex].model_x !=
                    right.vertices[vertex].model_x ||
                left.vertices[vertex].model_y !=
                    right.vertices[vertex].model_y ||
                left.vertices[vertex].model_z !=
                    right.vertices[vertex].model_z
            )
                return false;
        }
    }
    return true;
}
#endif

const char* temporal_stream_reset_reason(
    const LiveContext& context,
    const BuiltFrame& current,
    const opengt_live_options& options
) {
    if (!context.has_previous)
        return "no_previous";
    const auto& previous = context.previous_header;
    const std::uint64_t frame_delta =
        current.header.frame_index > previous.frame_index
            ? current.header.frame_index - previous.frame_index
            : 0;
    const std::int64_t poll_delta =
        static_cast<std::int64_t>(current.header.input_poll) -
        previous.input_poll;
    if (frame_delta == 0)
        return "frame_not_advancing";
    if (frame_delta > 4)
        return "frame_gap";
    if (poll_delta <= 0)
        return "poll_not_advancing";
    if (poll_delta > 4)
        return "poll_gap";
    if (
        current.header.display_width != previous.display_width ||
        current.header.display_height != previous.display_height)
        return "display_size";
    if (options.flags != context.previous_options.flags)
        return "options_flags";
    if (options.output_scale != context.previous_options.output_scale)
        return "output_scale";
    if (
        options.clear_color_rgba8 !=
        context.previous_options.clear_color_rgba8)
        return "clear_color";
    return nullptr;
}

void log_temporal_reset(
    const LiveContext& context,
    const BuiltFrame& current,
    const opengt_live_options& options,
    const char* reason
) {
    if (std::getenv("OPENGT_TEMPORAL_STREAM_DIAGNOSTICS") == nullptr)
        return;
    const auto& previous = context.previous_header;
    const std::uint64_t frame_delta =
        context.has_previous && current.header.frame_index > previous.frame_index
            ? current.header.frame_index - previous.frame_index
            : 0;
    const std::int64_t poll_delta = context.has_previous
        ? static_cast<std::int64_t>(current.header.input_poll) -
            previous.input_poll
        : 0;
    std::fprintf(
        stderr,
        "[Native-Temporal] reset reason=%s frame=%llu prevFrame=%llu "
        "frameDelta=%llu poll=%d prevPoll=%d pollDelta=%lld "
        "size=%dx%d prevSize=%dx%d flags=%u prevFlags=%u scale=%u "
        "prevScale=%u clear=%08x prevClear=%08x camera=%llu prevCamera=%llu "
        "commands=%zu prevCommands=%zu track=%u prevTrack=%u vehicle=%u "
        "prevVehicle=%u trackScope=%u triangles=%u\n",
        reason,
        static_cast<unsigned long long>(current.header.frame_index),
        static_cast<unsigned long long>(previous.frame_index),
        static_cast<unsigned long long>(frame_delta),
        current.header.input_poll,
        previous.input_poll,
        static_cast<long long>(poll_delta),
        current.header.display_width,
        current.header.display_height,
        previous.display_width,
        previous.display_height,
        options.flags,
        context.previous_options.flags,
        options.output_scale,
        context.previous_options.output_scale,
        options.clear_color_rgba8,
        context.previous_options.clear_color_rgba8,
        static_cast<unsigned long long>(current.header.camera_transform_id),
        static_cast<unsigned long long>(previous.camera_transform_id),
        context.draw_list.commands.size(),
        context.previous_draw_list.commands.size(),
        context.draw_list.track_commands,
        context.previous_draw_list.track_commands,
        context.draw_list.vehicle_commands,
        context.previous_draw_list.vehicle_commands,
        context.draw_list.track_commands != 0 ? 1U : 0U,
        current.header.triangle_count);
}

void cache_current(
    LiveContext* context,
    const BuiltFrame& current,
    const opengt_live_options& options,
    const opengt_live_stats& actual_stats
) {
    // Keep two command/material buffers alive and alternate them. The previous
    // frame remains available to development-only temporal diagnostics while
    // the buffer it replaced becomes the next frame's allocation-free target.
    std::swap(context->previous_draw_list, context->draw_list);
#if defined(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT)
    context->previous_interpolation_cache =
        std::move(context->interpolation_cache);
    context->previous_vram.swap(context->vram);
#endif
    context->previous_header = current.header;
#if defined(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT)
    context->previous_actual_stats = actual_stats;
#else
    (void)actual_stats;
#endif
    context->previous_options = options;
    context->has_previous = true;
}

#if defined(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT)
bool defer_temporal_reset_readback(
    const char* reason,
    bool current_has_track
) {
    if (current_has_track)
        return false;
    return
        std::strcmp(reason, "frame_gap") == 0 ||
        std::strcmp(reason, "poll_gap") == 0;
}

bool recover_temporal_gap_by_interpolation(
    const char* reason,
    const LiveContext& context,
    const BuiltFrame& current,
    bool current_has_track,
    const opengt_live_options& options
) {
    if (
        std::strcmp(reason, "frame_gap") != 0 &&
        std::strcmp(reason, "poll_gap") != 0)
        return false;
    if (
        !current_has_track ||
        context.previous_draw_list.track_commands == 0)
        return false;
    const auto& previous = context.previous_header;
    if (
        current.header.display_width != previous.display_width ||
        current.header.display_height != previous.display_height)
        return false;
    return
        options.flags == context.previous_options.flags &&
        options.output_scale == context.previous_options.output_scale &&
        options.clear_color_rgba8 ==
            context.previous_options.clear_color_rgba8;
}
#endif

} // namespace

extern "C" {

void* opengt_live_create(void) {
    try {
        return new LiveContext();
    } catch (...) {
        return nullptr;
    }
}

void opengt_live_destroy(void* handle) {
    auto* context = static_cast<LiveContext*>(handle);
    opengt::render::drain_world_d3d11_direct_output();
    if (context != nullptr) {
        std::fprintf(
            stderr,
            "[Native-World-Classification] frames=%llu "
            "classifiedWorldCommands=%llu unclassifiedWorldCommands=%llu "
            "framesWithUnclassifiedWorld=%llu maximumUnclassifiedWorld=%u\n",
            static_cast<unsigned long long>(context->classification_frames),
            static_cast<unsigned long long>(
                context->classified_world_commands),
            static_cast<unsigned long long>(
                context->unclassified_world_commands),
            static_cast<unsigned long long>(
                context->frames_with_unclassified_world),
            context->maximum_unclassified_world_commands);
        if (context->resident_audit_frames != 0) {
            std::fprintf(
                stderr,
                "[Native-Resident-Audit-Summary] frames=%llu "
                "triangles=%llu matched=%llu unmatched=%llu "
                "geometryOrdered=%llu geometryOrderMismatch=%llu "
                "materialOrderMismatch=%llu\n",
                static_cast<unsigned long long>(
                    context->resident_audit_frames),
                static_cast<unsigned long long>(
                    context->resident_audit_triangles),
                static_cast<unsigned long long>(
                    context->resident_audit_matched),
                static_cast<unsigned long long>(
                    context->resident_audit_unmatched),
                static_cast<unsigned long long>(
                    context->resident_audit_ordered),
                static_cast<unsigned long long>(
                    context->resident_audit_order_mismatched),
                static_cast<unsigned long long>(
                    context->resident_audit_material_order_mismatched));
        }
        if (context->resident_lod_frames != 0) {
            std::fprintf(
                stderr,
                "[Native-Resident-LOD-Summary] frames=%llu "
                "texturedEvaluations=%llu authoredNear=%llu "
                "authoredFar=%llu authoredSwitches=%llu renderedFar=%llu\n",
                static_cast<unsigned long long>(
                    context->resident_lod_frames),
                static_cast<unsigned long long>(
                    context->resident_lod_textured_evaluations),
                static_cast<unsigned long long>(
                    context->resident_lod_authored_near),
                static_cast<unsigned long long>(
                    context->resident_lod_authored_distant),
                static_cast<unsigned long long>(
                    context->resident_lod_authored_switches),
                static_cast<unsigned long long>(
                    context->resident_lod_authored_distant));
        }
    }
    delete context;
}

int32_t opengt_live_set_presentation_device(
    void* handle,
    void* d3d11_device
) {
    if (handle == nullptr || d3d11_device == nullptr)
        return -1;
    return opengt::render::set_world_d3d11_presentation_device(
        d3d11_device) ? 0 : -2;
}

int32_t opengt_live_render(
    void* handle,
    const std::uint8_t* capture_bytes,
    std::size_t capture_size,
    std::uint8_t* output_rgba,
    std::size_t output_capacity,
    const opengt_live_options* options,
    opengt_live_stats* stats
) {
    clear_struct(stats);
    if (
        handle == nullptr || capture_bytes == nullptr ||
        output_rgba == nullptr || options == nullptr || stats == nullptr ||
        options->struct_size != sizeof(opengt_live_options) ||
        stats->struct_size != sizeof(opengt_live_stats)
    )
        return fail(stats, 1);
    try {
        auto* context = static_cast<LiveContext*>(handle);
        BuiltFrame built{};
        std::uint32_t result = build_frame(
            context,
            capture_bytes,
            capture_size,
            *options,
            &built);
        if (result != 0)
            return fail(stats, result);
        ++context->classification_frames;
        context->classified_world_commands +=
            static_cast<std::uint64_t>(context->draw_list.track_commands) +
            context->draw_list.vehicle_commands +
            context->draw_list.background_commands;
        context->unclassified_world_commands +=
            context->draw_list.unclassified_world_commands;
        if (context->draw_list.unclassified_world_commands != 0) {
            ++context->frames_with_unclassified_world;
            context->maximum_unclassified_world_commands = (std::max)(
                context->maximum_unclassified_world_commands,
                context->draw_list.unclassified_world_commands);
        }
        const bool direct_gpu_output =
            (options->flags & OPENGT_LIVE_DIRECT_GPU_OUTPUT) != 0;
        if (direct_gpu_output) {
            context->has_previous = false;
            result = render_frame(
                built,
                context->draw_list,
                context->vram.data(),
                context->vram.size(),
                output_rgba,
                output_capacity,
                *options,
                stats);
            return result == 0 ? 0 : fail(stats, result);
        }
        const bool realtime_readback =
            (options->flags & OPENGT_LIVE_REALTIME_READBACK) != 0;
        if (!realtime_readback) {
            context->has_previous = false;
            result = render_frame(
                built,
                context->draw_list,
                context->vram.data(),
                context->vram.size(),
                output_rgba,
                output_capacity,
                *options,
                stats);
            return result == 0 ? 0 : fail(stats, result);
        }

        const char* reset_reason = temporal_stream_reset_reason(
            *context, built, *options);
        const bool software_adapter =
            (options->flags & OPENGT_LIVE_WARP) != 0;
        if (reset_reason != nullptr) {
            log_temporal_reset(*context, built, *options, reset_reason);
            opengt::render::reset_world_d3d11_readback(software_adapter);
            context->pending_authored_read = 0;
            context->pending_authored_write = 0;
            context->pending_authored_count = 0;
        }
        context->readback_software_adapter = software_adapter;

        opengt_live_stats submitted{};
        submitted.struct_size = sizeof(submitted);
        result = render_frame(
            built,
            context->draw_list,
            context->vram.data(),
            context->vram.size(),
            output_rgba,
            output_capacity,
            *options,
            &submitted,
            false,
            false,
            false,
            false,
            true);
        if (result != 0)
            return fail(stats, result);
        if (
            context->pending_authored_count >=
            context->pending_authored_stats.size()
        )
            return fail(stats, 704U);
        const std::uint32_t write = context->pending_authored_write;
        context->pending_authored_stats[write] = submitted;
        context->pending_authored_write =
            (write + 1U) % context->pending_authored_stats.size();
        ++context->pending_authored_count;

        // Managed presentation deliberately waits for the first modern-world
        // image before it asserts world ownership. A nonblocking probe here
        // can miss a slow one-time shader/resource initialization, while the
        // emulation thread is simultaneously waiting and therefore cannot
        // submit the next image that would advance this queue. Complete only
        // that first authored copy synchronously. Every later submission keeps
        // the bounded asynchronous staging contract; a full queue also waits
        // rather than discarding an authored frame.
        const bool initial_authored_seed =
            !context->has_previous &&
            context->pending_authored_count == 1U;
        const bool readback_must_complete =
            initial_authored_seed ||
            context->pending_authored_count >=
            opengt::render::world_gpu_realtime_readback_image_delay;
        const int32_t drained = try_read_pending_authored(
            context,
            output_rgba,
            output_capacity,
            stats,
            readback_must_complete);
        if (drained < 0)
            return fail(stats, static_cast<std::uint32_t>(-drained));
        if (drained == 0) {
            *stats = submitted;
            stats->reserved |= OPENGT_LIVE_STATS_NO_OUTPUT;
        }
        cache_current(context, built, *options, submitted);
        return 0;
    } catch (const std::bad_alloc&) {
        return fail(stats, 2);
    } catch (...) {
        return fail(stats, 3);
    }
}

#if defined(OPENGT_SYNTHETIC_FRAME_DEV_SUPPORT)
int32_t opengt_live_render_pair(
    void* handle,
    const std::uint8_t* capture_bytes,
    std::size_t capture_size,
    std::uint8_t* output_first_rgba,
    std::uint8_t* output_second_rgba,
    std::size_t output_capacity,
    const opengt_live_options* options,
    opengt_live_stats* first_stats,
    opengt_live_stats* second_stats,
    opengt_live_interpolation_stats* interpolation_stats
) {
    clear_struct(first_stats);
    clear_struct(second_stats);
    clear_struct(interpolation_stats);
    if (
        handle == nullptr || capture_bytes == nullptr ||
        output_first_rgba == nullptr || output_second_rgba == nullptr ||
        options == nullptr || first_stats == nullptr ||
        second_stats == nullptr || interpolation_stats == nullptr ||
        options->struct_size != sizeof(opengt_live_options) ||
        first_stats->struct_size != sizeof(opengt_live_stats) ||
        second_stats->struct_size != sizeof(opengt_live_stats) ||
        interpolation_stats->struct_size !=
            sizeof(opengt_live_interpolation_stats)
    )
        return fail_pair(
            first_stats, second_stats, interpolation_stats, 1);
    try {
        auto* context = static_cast<LiveContext*>(handle);
        const auto pair_started = Clock::now();
        BuiltFrame current{};
        std::uint32_t result = build_frame(
            context,
            capture_bytes,
            capture_size,
            *options,
            &current,
            false,
            false);
        if (result != 0)
            return fail_pair(
                first_stats, second_stats, interpolation_stats, result);
        // Pair rendering intentionally builds topology on the interpolated
        // midpoint rather than this raw authored list. Use draw-list
        // provenance for track scope/reset decisions; current.topology is
        // therefore zero here by construction.
        const bool current_has_track =
            context->draw_list.track_commands != 0;

        const char* reset_reason = temporal_stream_reset_reason(
            *context, current, *options);
        if (
            reset_reason != nullptr &&
            recover_temporal_gap_by_interpolation(
                reset_reason,
                *context,
                current,
                current_has_track,
                *options))
            reset_reason = nullptr;
        if (reset_reason != nullptr) {
            log_temporal_reset(*context, current, *options, reset_reason);
            interpolation_stats->temporal_reset = 1;
            opengt::render::reset_world_d3d11_readback(
                (options->flags & OPENGT_LIVE_WARP) != 0);
            context->readback_software_adapter =
                (options->flags & OPENGT_LIVE_WARP) != 0;
            context->pending_output_pair_read = 0;
            context->pending_output_pair_write = 0;
            context->pending_output_pair_count = 0;
            if (defer_temporal_reset_readback(
                    reset_reason, current_has_track)) {
                opengt_live_stats actual{};
                actual.struct_size = sizeof(actual);
                fill_deferred_reset_stats(
                    current,
                    context->draw_list,
                    *options,
                    pair_started,
                    &actual);
                interpolation_stats->current_topology_microseconds =
                    current.topology_microseconds;
                if (current_has_track)
                    interpolation_stats->reserved |= 2U;
                interpolation_stats->pair_pipeline_microseconds =
                    microseconds(Clock::now() - pair_started);
                *first_stats = actual;
                interpolation_stats->output_count = 0;
                cache_current(context, current, *options, actual);
                return 0;
            }
            opengt_live_stats actual{};
            actual.struct_size = sizeof(actual);
            result = render_frame(
                current,
                context->draw_list,
                context->vram.data(),
                context->vram.size(),
                output_first_rgba,
                output_capacity,
                *options,
                &actual);
            if (result != 0)
                return fail_pair(
                    first_stats,
                    second_stats,
                    interpolation_stats,
                    result);
            actual.reserved |= 4U;
            interpolation_stats->actual_render_microseconds =
                actual.render_microseconds;
            interpolation_stats->current_topology_microseconds =
                current.topology_microseconds;
            if (current_has_track)
                interpolation_stats->reserved |= 2U;
            interpolation_stats->pair_pipeline_microseconds =
                microseconds(Clock::now() - pair_started);
            // A temporal discontinuity cannot be interpolated, but the D3D11
            // reset path reads this authored image synchronously. Publish it
            // immediately instead of creating a blank modern-world interval.
            *first_stats = actual;
            interpolation_stats->output_count = 1;
            cache_current(context, current, *options, actual);
            return 0;
        }

        const auto interpolation_started = Clock::now();
        opengt::render::WorldDrawList midpoint{};
        opengt::render::WorldInterpolationStats interpolation{};
        const auto interpolation_result =
            opengt::render::interpolate_world_draw_lists_cached(
                context->previous_draw_list,
                context->draw_list,
                &context->previous_interpolation_cache,
                &context->interpolation_cache,
                0.5F,
                &midpoint,
                &interpolation);
        const auto interpolation_finished = Clock::now();
        if (
            interpolation_result !=
            opengt::render::WorldInterpolationResult::success
        )
            return fail_pair(
                first_stats,
                second_stats,
                interpolation_stats,
                500U + static_cast<std::uint32_t>(interpolation_result));

        BuiltFrame midpoint_built{};
        midpoint_built.header = context->previous_header;
        midpoint_built.header.frame_index =
            context->previous_header.frame_index + 1;
        midpoint_built.header.input_poll =
            context->previous_header.input_poll + 1;
        midpoint_built.pipeline_started = interpolation_started;
        if ((options->flags & OPENGT_LIVE_TOPOLOGY) != 0) {
            const auto midpoint_topology_started = Clock::now();
            const auto topology_result = opengt::render::apply_world_topology(
                &midpoint,
                opengt::render::WorldTopologyOptions{
                    true,
                    true,
                    true,
                    true,
                    false},
                &midpoint_built.topology);
            const auto midpoint_topology_finished = Clock::now();
            midpoint_built.topology_microseconds = microseconds(
                midpoint_topology_finished - midpoint_topology_started);
            if (topology_result !=
                opengt::render::WorldTopologyResult::success) {
                return fail_pair(
                    first_stats,
                    second_stats,
                    interpolation_stats,
                    300U + static_cast<std::uint32_t>(topology_result));
            }
        }
        opengt_live_stats midpoint_stats{};
        midpoint_stats.struct_size = sizeof(midpoint_stats);
        const bool software_adapter =
            (options->flags & OPENGT_LIVE_WARP) != 0;
        context->readback_software_adapter = software_adapter;
        const std::size_t readback_limit =
            (options->flags & OPENGT_LIVE_REALTIME_READBACK) != 0
                ? context->pending_midpoint_stats.size()
                : opengt::render::world_gpu_readback_pair_delay;
        const bool readback_full =
            context->pending_output_pair_count >= readback_limit;
        const int32_t drained = try_read_pending_pair(
            context,
            output_first_rgba,
            output_second_rgba,
            output_capacity,
            first_stats,
            second_stats,
            readback_full);
        if (drained < 0)
            return fail_pair(
                first_stats,
                second_stats,
                interpolation_stats,
                static_cast<std::uint32_t>(-drained));
        interpolation_stats->output_count = drained == 1 ? 2U : 0U;
        const bool reuse_midpoint_uploads =
            midpoint_uploads_match_previous(
                context->previous_draw_list,
                midpoint);
        result = render_frame(
            midpoint_built,
            midpoint,
            context->previous_vram.data(),
            context->previous_vram.size(),
            output_first_rgba,
            output_capacity,
            *options,
            &midpoint_stats,
            reuse_midpoint_uploads,
            reuse_midpoint_uploads,
            true,
            false,
            true);
        if (result != 0)
            return fail_pair(
                first_stats, second_stats, interpolation_stats, result);

        opengt_live_stats actual{};
        actual.struct_size = sizeof(actual);
        result = render_frame(
            current,
            context->draw_list,
            context->vram.data(),
            context->vram.size(),
            output_second_rgba,
            output_capacity,
            *options,
            &actual,
            false,
            false,
            false,
            false,
            true);
        if (result != 0)
            return fail_pair(
                first_stats, second_stats, interpolation_stats, result);

        midpoint_stats.reserved |= 1U;
        const std::uint64_t pair_pipeline_microseconds =
            microseconds(Clock::now() - pair_started);
        const std::uint32_t write = context->pending_output_pair_write;
        context->pending_midpoint_stats[write] = midpoint_stats;
        context->pending_actual_stats[write] = actual;
        context->pending_output_pair_write =
            (write + 1U) % context->pending_midpoint_stats.size();
        ++context->pending_output_pair_count;
        interpolation_stats->result = 0;
        interpolation_stats->previous_commands =
            interpolation.previous_commands;
        interpolation_stats->current_commands =
            interpolation.current_commands;
        interpolation_stats->eligible_world_commands =
            interpolation.eligible_world_commands;
        interpolation_stats->matched_commands =
            interpolation.matched_commands;
        interpolation_stats->matched_track_commands =
            interpolation.matched_track_commands;
        interpolation_stats->matched_vehicle_commands =
            interpolation.matched_vehicle_commands;
        interpolation_stats->held_screen_commands =
            interpolation.held_screen_commands;
        interpolation_stats->held_unmatched_commands =
            interpolation.held_unmatched_commands;
        interpolation_stats->previous_transform_groups =
            interpolation.previous_transform_groups;
        interpolation_stats->current_transform_groups =
            interpolation.current_transform_groups;
        interpolation_stats->matched_transform_groups =
            interpolation.matched_transform_groups;
        if (reuse_midpoint_uploads)
            interpolation_stats->reserved |= 1U;
        if (current_has_track)
            interpolation_stats->reserved |= 2U;
        interpolation_stats->exact_rigid_transform_groups =
            interpolation.exact_rigid_transform_groups;
        interpolation_stats->incoherent_exact_transform_groups =
            interpolation.incoherent_exact_transform_groups;
        interpolation_stats->held_incoherent_vehicle_commands =
            interpolation.held_incoherent_vehicle_commands;
        interpolation_stats->held_track_visibility_commands =
            interpolation.held_track_visibility_commands;
        interpolation_stats->held_unsafe_track_commands =
            interpolation.held_unsafe_track_commands;
        interpolation_stats->interpolation_microseconds =
            microseconds(interpolation_finished - interpolation_started);
        interpolation_stats->midpoint_render_microseconds =
            midpoint_stats.render_microseconds;
        interpolation_stats->actual_render_microseconds =
            actual.render_microseconds;
        interpolation_stats->current_topology_microseconds =
            midpoint_built.topology_microseconds;
        interpolation_stats->pair_pipeline_microseconds =
            pair_pipeline_microseconds;
        cache_current(context, current, *options, actual);
        return 0;
    } catch (const std::bad_alloc&) {
        return fail_pair(
            first_stats, second_stats, interpolation_stats, 2);
    } catch (...) {
        return fail_pair(
            first_stats, second_stats, interpolation_stats, 3);
    }
}

int32_t opengt_live_try_read_pair(
    void* handle,
    std::uint8_t* output_first_rgba,
    std::uint8_t* output_second_rgba,
    std::size_t output_capacity,
    opengt_live_stats* first_stats,
    opengt_live_stats* second_stats
) {
    clear_struct(first_stats);
    clear_struct(second_stats);
    if (
        handle == nullptr || output_first_rgba == nullptr ||
        output_second_rgba == nullptr || first_stats == nullptr ||
        second_stats == nullptr ||
        first_stats->struct_size != sizeof(opengt_live_stats) ||
        second_stats->struct_size != sizeof(opengt_live_stats)
    )
        return -1;
    try {
        const int32_t result = try_read_pending_pair(
            static_cast<LiveContext*>(handle),
            output_first_rgba,
            output_second_rgba,
            output_capacity,
            first_stats,
            second_stats,
            false);
        if (result < 0) {
            first_stats->result = static_cast<std::uint32_t>(-result);
            second_stats->result = first_stats->result;
        }
        return result;
    } catch (const std::bad_alloc&) {
        first_stats->result = 2;
        second_stats->result = 2;
        return -2;
    } catch (...) {
        first_stats->result = 3;
        second_stats->result = 3;
        return -3;
    }
}
#endif

int32_t opengt_live_set_texture_uploads(
    void* handle,
    int32_t software_adapter,
    const opengt_live_texture_upload* uploads,
    size_t upload_count
) {
    if (
        handle == nullptr ||
        (upload_count != 0 && uploads == nullptr) ||
        upload_count > 65536
    )
        return -1;
    static_assert(
        sizeof(opengt_live_texture_upload) ==
        sizeof(opengt::render::WorldTextureUpload));
    return opengt::render::set_world_d3d11_texture_uploads(
        software_adapter != 0,
        reinterpret_cast<const opengt::render::WorldTextureUpload*>(uploads),
        upload_count) ? 0 : -2;
}

int32_t opengt_live_register_resident_mesh(
    void* handle,
    const std::uint8_t* definition_bytes,
    std::size_t definition_size
) {
    if (handle == nullptr || definition_bytes == nullptr)
        return -1;
    try {
        ResidentMesh mesh{};
        if (!parse_resident_mesh(definition_bytes, definition_size, &mesh))
            return -2;
        trace_resident_near_overlay_candidates(mesh);
        trace_resident_primitive_address_range(mesh);
        auto* context = static_cast<LiveContext*>(handle);
        const std::uint64_t key = mesh.key;
        if (
            mesh.overlay_primitives != 0 &&
            std::getenv("OPENGT_TRACE_RESIDENT_OVERLAYS") != nullptr
        ) {
            std::vector<ResidentOverlayCandidate> overlay_candidates;
            overlay_candidates.reserve(mesh.primitives.size());
            for (const auto& primitive : mesh.primitives) {
                overlay_candidates.push_back(
                    resident_overlay_candidate(mesh, primitive));
            }
            std::fprintf(
                stderr,
                "[Native-Resident-Overlays] mesh=%016llx "
                "primitives=%zu overlapPairs=%u overlays=%u "
                "untexturedOverlays=%u replacements=%u maxLayer=%u\n",
                static_cast<unsigned long long>(mesh.key),
                mesh.primitives.size(),
                mesh.overlay_pairs,
                mesh.overlay_primitives,
                mesh.untextured_overlay_primitives,
                mesh.replacement_primitives,
                static_cast<unsigned>(mesh.maximum_overlay_layer));
            for (std::size_t primitive_index = 0;
                 primitive_index < mesh.primitives.size();
                 ++primitive_index) {
                const auto& primitive = mesh.primitives[primitive_index];
                if (primitive.overlay_layer == 0)
                    continue;
                const int vertex_count =
                    resident_primitive_vertex_count(primitive);
                std::fprintf(
                    stderr,
                    "[Native-Resident-Overlay-Primitive] mesh=%016llx "
                    "primitive=%zu source=%08x flags=%08x stream=%u "
                    "layer=%u replacement=%u color=%08x "
                    "material=%04x,%04x area2=%.3f "
                    "vertices=",
                    static_cast<unsigned long long>(mesh.key),
                    primitive_index,
                    primitive.source_address,
                    primitive.flags,
                    static_cast<unsigned>(primitive.stream),
                    static_cast<unsigned>(primitive.overlay_layer),
                    primitive.replacement_surface ? 1U : 0U,
                    primitive.near_material.color[0],
                    primitive.near_material.texture_page,
                    primitive.near_material.clut,
                    resident_primitive_projected_area_twice(
                        mesh,
                        primitive,
                        overlay_candidates[primitive_index].dropped_axis));
                for (int corner = 0; corner < vertex_count; ++corner) {
                    const auto& vertex =
                        mesh.vertices[primitive.indices[corner]];
                    std::fprintf(
                        stderr,
                        "%s%d,%d,%d",
                        corner == 0 ? "" : ";",
                        static_cast<int>(vertex.x),
                        static_cast<int>(vertex.y),
                        static_cast<int>(vertex.z));
                }
                std::fprintf(stderr, "\n");
                for (std::size_t earlier = 0;
                     earlier < primitive_index;
                     ++earlier) {
                    if (!overlay_candidates[earlier].valid ||
                        !(overlay_candidates[earlier].plane ==
                            overlay_candidates[primitive_index].plane) ||
                        !resident_overlay_bounds_positive_overlap(
                            overlay_candidates[earlier],
                            overlay_candidates[primitive_index]) ||
                        !resident_primitives_positive_overlap(
                            mesh,
                            mesh.primitives[earlier],
                            primitive,
                            overlay_candidates[primitive_index].dropped_axis))
                        continue;
                    const auto& support = mesh.primitives[earlier];
                    const double overlay_area =
                        resident_primitive_projected_area_twice(
                            mesh,
                            primitive,
                            overlay_candidates[primitive_index].dropped_axis);
                    const double support_area =
                        resident_primitive_projected_area_twice(
                            mesh,
                            support,
                            overlay_candidates[primitive_index].dropped_axis);
                    std::fprintf(
                        stderr,
                        "[Native-Resident-Overlay-Pair] mesh=%016llx "
                        "earlier=%zu/%08x later=%zu/%08x "
                        "area2=%.3f/%.3f ratio=%.6f "
                        "flags=%08x/%08x materials=%04x,%04x/%04x,%04x\n",
                        static_cast<unsigned long long>(mesh.key),
                        earlier,
                        support.source_address,
                        primitive_index,
                        primitive.source_address,
                        support_area,
                        overlay_area,
                        support_area > 0.0 ? overlay_area / support_area : 0.0,
                        support.flags,
                        primitive.flags,
                        support.near_material.texture_page,
                        support.near_material.clut,
                        primitive.near_material.texture_page,
                        primitive.near_material.clut);
                }
            }
        }
        context->resident_meshes.insert_or_assign(key, std::move(mesh));
        return 0;
    } catch (const std::bad_alloc&) {
        return -3;
    } catch (...) {
        return -4;
    }
}

int32_t opengt_live_set_resident_instances(
    void* handle,
    const std::uint8_t* instance_bytes,
    std::size_t instance_count
) {
    if (handle == nullptr)
        return -1;
    try {
        auto* context = static_cast<LiveContext*>(handle);
        return parse_resident_instances(
            instance_bytes,
            instance_count,
            &context->resident_instances) ? 0 : -2;
    } catch (const std::bad_alloc&) {
        return -3;
    } catch (...) {
        return -4;
    }
}

std::uint32_t opengt_live_api_version(void) {
    return api_version;
}

} // extern "C"
