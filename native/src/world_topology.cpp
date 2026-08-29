#include "opengt/world_topology.hpp"

#include <algorithm>
#include <array>
#include <chrono>
#include <cmath>
#include <cstddef>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <iterator>
#include <map>
#include <memory_resource>
#include <numeric>
#include <set>
#include <tuple>
#include <unordered_map>
#include <unordered_set>
#include <utility>
#include <vector>

namespace opengt::render {
namespace {

struct Position {
    std::int32_t x;
    std::int32_t y;
    std::int32_t z;

    bool operator<(const Position& other) const {
        return std::tie(x, y, z) <
            std::tie(other.x, other.y, other.z);
    }

    bool operator==(const Position& other) const {
        return x == other.x && y == other.y && z == other.z;
    }
};

void hash_combine(std::size_t* seed, std::uint64_t value) {
    *seed ^= std::hash<std::uint64_t>{}(value) +
        0x9E3779B97F4A7C15ULL + (*seed << 6) + (*seed >> 2);
}

struct PositionHash {
    std::size_t operator()(const Position& value) const noexcept {
        std::size_t seed = 0;
        hash_combine(&seed, static_cast<std::uint32_t>(value.x));
        hash_combine(&seed, static_cast<std::uint32_t>(value.y));
        hash_combine(&seed, static_cast<std::uint32_t>(value.z));
        return seed;
    }
};

struct Edge {
    Position a;
    Position b;

    Edge(Position left, Position right)
        : a(std::min(left, right)), b(std::max(left, right)) {}

    bool operator<(const Edge& other) const {
        return std::tie(a, b) < std::tie(other.a, other.b);
    }

    bool operator==(const Edge& other) const {
        return a == other.a && b == other.b;
    }
};

struct EdgeHash {
    std::size_t operator()(const Edge& value) const noexcept {
        std::size_t seed = PositionHash{}(value.a);
        hash_combine(&seed, PositionHash{}(value.b));
        return seed;
    }
};

struct Occurrence {
    std::size_t command;
    int vertex;
};

struct OccurrenceList {
    static constexpr std::size_t inline_capacity = 4;

    struct const_iterator {
        using iterator_category = std::forward_iterator_tag;
        using value_type = Occurrence;
        using difference_type = std::ptrdiff_t;
        using pointer = const Occurrence*;
        using reference = const Occurrence&;

        const OccurrenceList* owner{};
        std::size_t index{};

        reference operator*() const {
            return index < owner->inline_size
                ? owner->inline_items[index]
                : owner->overflow[index - owner->inline_size];
        }

        pointer operator->() const { return &**this; }

        const_iterator& operator++() {
            ++index;
            return *this;
        }

        const_iterator operator++(int) {
            const_iterator copy = *this;
            ++*this;
            return copy;
        }

        bool operator==(const const_iterator& other) const {
            return owner == other.owner && index == other.index;
        }

        bool operator!=(const const_iterator& other) const {
            return !(*this == other);
        }
    };

    std::array<Occurrence, inline_capacity> inline_items{};
    std::size_t inline_size{};
    std::vector<Occurrence> overflow;

    void push_back(Occurrence occurrence) {
        if (inline_size < inline_capacity)
            inline_items[inline_size++] = occurrence;
        else
            overflow.push_back(occurrence);
    }

    std::size_t size() const { return inline_size + overflow.size(); }
    bool empty() const { return size() == 0; }
    const Occurrence& front() const { return inline_items.front(); }
    const_iterator begin() const { return const_iterator{this, 0}; }
    const_iterator end() const { return const_iterator{this, size()}; }
};

struct EdgeOccurrence {
    std::size_t command;
    int edge;
};

bool material_compatible(
    const WorldDrawList& list,
    const OccurrenceList& left,
    const OccurrenceList& right
) {
    for (const auto& a : left) {
        const auto& left_command = list.commands[a.command];
        const bool left_has_material =
            left_command.material_index < list.materials.size();
        const auto& left_material =
            left_has_material
                ? &list.materials[left_command.material_index]
                : nullptr;
        for (const auto& b : right) {
            const auto& right_command = list.commands[b.command];
            if (
                left_command.ordering_table_index !=
                right_command.ordering_table_index
            )
                continue;
            if (!left_has_material) {
                if (left_command.material_index == right_command.material_index)
                    return true;
                continue;
            }
            if (right_command.material_index >= list.materials.size())
                continue;
            const auto& right_material =
                list.materials[right_command.material_index];
            if (
                left_material->primitive_flags ==
                    right_material.primitive_flags
            )
                return true;
        }
    }
    return false;
}

struct Plane {
    std::int64_t x;
    std::int64_t y;
    std::int64_t z;
    std::int64_t d;

    bool operator<(const Plane& other) const {
        return std::tie(x, y, z, d) <
            std::tie(other.x, other.y, other.z, other.d);
    }


    bool operator==(const Plane& other) const {
        return x == other.x && y == other.y &&
            z == other.z && d == other.d;
    }
};

struct PlaneHash {
    std::size_t operator()(const Plane& value) const noexcept {
        std::size_t seed = 0;
        hash_combine(&seed, static_cast<std::uint64_t>(value.x));
        hash_combine(&seed, static_cast<std::uint64_t>(value.y));
        hash_combine(&seed, static_cast<std::uint64_t>(value.z));
        hash_combine(&seed, static_cast<std::uint64_t>(value.d));
        return seed;
    }
};

struct Point2 {
    std::int64_t x;
    std::int64_t y;
};

struct RasterLayerKey {
    std::uint32_t object_id;
    std::uint32_t model_pointer;
    std::uint64_t transform_id;
    std::uint32_t material_index;
    std::int32_t ordering_table_index;
    std::int32_t authored_x;
    std::int32_t authored_y;

    bool operator==(const RasterLayerKey& other) const noexcept {
        return
            object_id == other.object_id &&
            model_pointer == other.model_pointer &&
            transform_id == other.transform_id &&
            material_index == other.material_index &&
            ordering_table_index == other.ordering_table_index &&
            authored_x == other.authored_x &&
            authored_y == other.authored_y;
    }
};

struct RasterLayerKeyHash {
    std::size_t operator()(const RasterLayerKey& key) const noexcept {
        std::size_t seed = 0;
        hash_combine(&seed, key.object_id);
        hash_combine(&seed, key.model_pointer);
        hash_combine(&seed, key.transform_id);
        hash_combine(&seed, key.material_index);
        hash_combine(&seed, static_cast<std::uint32_t>(
            key.ordering_table_index));
        hash_combine(&seed, static_cast<std::uint32_t>(key.authored_x));
        hash_combine(&seed, static_cast<std::uint32_t>(key.authored_y));
        return seed;
    }
};

struct LodLayerKey {
    std::uint32_t object_id;
    std::uint32_t model_pointer;
    std::uint64_t transform_id;
    std::uint32_t material_index;
    std::int32_t ordering_table_index;

    bool operator==(const LodLayerKey& other) const noexcept {
        return
            object_id == other.object_id &&
            model_pointer == other.model_pointer &&
            transform_id == other.transform_id &&
            material_index == other.material_index &&
            ordering_table_index == other.ordering_table_index;
    }
};

struct LodLayerKeyHash {
    std::size_t operator()(const LodLayerKey& key) const noexcept {
        std::size_t seed = 0;
        hash_combine(&seed, key.object_id);
        hash_combine(&seed, key.model_pointer);
        hash_combine(&seed, key.transform_id);
        hash_combine(&seed, key.material_index);
        hash_combine(&seed, static_cast<std::uint32_t>(
            key.ordering_table_index));
        return seed;
    }
};

struct AuthoredPixelHash {
    std::size_t operator()(
        const std::pair<std::int32_t, std::int32_t>& key
    ) const noexcept {
        std::size_t seed = 0;
        hash_combine(&seed, static_cast<std::uint32_t>(key.first));
        hash_combine(&seed, static_cast<std::uint32_t>(key.second));
        return seed;
    }
};

struct AuthoredRasterLineKey {
    std::uint32_t object_id;
    std::uint32_t model_pointer;
    std::int32_t ordering_table_index;
    std::int32_t direction_x;
    std::int32_t direction_y;
    std::int64_t offset;

    bool operator==(const AuthoredRasterLineKey& other) const noexcept {
        return
            object_id == other.object_id &&
            model_pointer == other.model_pointer &&
            ordering_table_index == other.ordering_table_index &&
            direction_x == other.direction_x &&
            direction_y == other.direction_y &&
            offset == other.offset;
    }
};

struct AuthoredRasterLineKeyHash {
    std::size_t operator()(
        const AuthoredRasterLineKey& key
    ) const noexcept {
        std::size_t seed = 0;
        hash_combine(&seed, key.object_id);
        hash_combine(&seed, key.model_pointer);
        hash_combine(&seed, static_cast<std::uint32_t>(
            key.ordering_table_index));
        hash_combine(&seed, static_cast<std::uint32_t>(key.direction_x));
        hash_combine(&seed, static_cast<std::uint32_t>(key.direction_y));
        hash_combine(&seed, static_cast<std::uint64_t>(key.offset));
        return seed;
    }
};

struct AuthoredOverlapEdge {
    std::size_t command;
    int edge;
    std::int64_t minimum_coordinate;
    std::int64_t maximum_coordinate;
};

Position position(const WorldDrawVertex& vertex) {
    // GTE view coordinates are exact transformed integers. Unlike GT2's
    // sector-local model coordinates, they place neighboring 4096-unit track
    // sectors in one common frame, so authored boundary copies compare equal
    // without a tolerance or screen-space inference.
    return Position{
        vertex.exact_view_x,
        vertex.exact_view_y,
        vertex.exact_view_z,
    };
}

Position model_position(const WorldDrawVertex& vertex) {
    return Position{
        vertex.model_x,
        vertex.model_y,
        vertex.model_z,
    };
}

bool screen_space_command(
    const WorldDrawList& list,
    const WorldDrawCommand& command
) {
    return
        command.material_index < list.materials.size() &&
        (list.materials[command.material_index].primitive_flags &
            world_primitive_screen_space_flag) != 0;
}

bool resident_course_command(
    const WorldDrawList& list,
    const WorldDrawCommand& command
) {
    return
        command.material_index < list.materials.size() &&
        (list.materials[command.material_index].primitive_flags &
            world_primitive_resident_course_flag) != 0;
}

bool eligible(
    const WorldDrawList& list,
    const WorldDrawCommand& command
) {
    if (
        command.object_kind != 1 ||
        command.channel != WorldViewChannel::main_view ||
        screen_space_command(list, command) ||
        resident_course_command(list, command)
    )
        return false;
    for (const auto& vertex : command.vertices) {
        if (
            (vertex.provenance_flags & 1U) == 0 ||
            vertex.source_vertex_identity == 0
        )
            return false;
    }
    return true;
}

bool opaque_track_surface(
    const WorldDrawList& list,
    const WorldDrawCommand& command
) {
    if (command.material_index >= list.materials.size())
        return false;
    const auto& material = list.materials[command.material_index];
    if (
        (material.primitive_flags & 1U) == 0 ||
        (material.primitive_flags & 2U) != 0 ||
        (material.primitive_flags & world_primitive_screen_space_flag) != 0
    )
        return false;
    const auto& a = command.vertices[0];
    const auto& b = command.vertices[1];
    const auto& c = command.vertices[2];
    const std::int64_t ab_x =
        static_cast<std::int64_t>(b.model_x) - a.model_x;
    const std::int64_t ab_y =
        static_cast<std::int64_t>(b.model_y) - a.model_y;
    const std::int64_t ab_z =
        static_cast<std::int64_t>(b.model_z) - a.model_z;
    const std::int64_t ac_x =
        static_cast<std::int64_t>(c.model_x) - a.model_x;
    const std::int64_t ac_y =
        static_cast<std::int64_t>(c.model_y) - a.model_y;
    const std::int64_t ac_z =
        static_cast<std::int64_t>(c.model_z) - a.model_z;
    const std::int64_t normal_x = ab_y * ac_z - ab_z * ac_y;
    const std::int64_t normal_y = ab_z * ac_x - ab_x * ac_z;
    const std::int64_t normal_z = ab_x * ac_y - ab_y * ac_x;
    return
        std::llabs(normal_y) >= std::llabs(normal_x) &&
        std::llabs(normal_y) >= std::llabs(normal_z);
}

auto occurrence_key(
    const WorldDrawList& list,
    const Occurrence& occurrence
) {
    const auto& command = list.commands[occurrence.command];
    const auto& vertex = command.vertices[occurrence.vertex];
    return std::make_tuple(
        command.model_pointer,
        vertex.source_vertex_identity,
        command.object_id,
        command.source_command_index,
        occurrence.vertex);
}

void copy_geometric_fields(
    WorldDrawVertex* destination,
    const WorldDrawVertex& source
) {
    destination->world_x = source.world_x;
    destination->world_y = source.world_y;
    destination->world_z = source.world_z;
    destination->view_x = source.view_x;
    destination->view_y = source.view_y;
    destination->view_z = source.view_z;
    destination->clip_x = source.clip_x;
    destination->clip_y = source.clip_y;
    destination->clip_z = source.clip_z;
    destination->clip_w = source.clip_w;
    destination->screen_x = source.screen_x;
    destination->screen_y = source.screen_y;
}

void copy_projected_position(
    WorldDrawVertex* destination,
    const WorldDrawVertex& source,
    const WorldDrawList& list
) {
    destination->screen_x = source.screen_x;
    destination->screen_y = source.screen_y;
    const float ndc_x =
        ((source.screen_x - list.display_x) /
            static_cast<float>(list.display_width)) *
            2.0F - 1.0F;
    const float ndc_y =
        1.0F -
        ((source.screen_y - list.display_y) /
            static_cast<float>(list.display_height)) *
            2.0F;
    destination->clip_x = ndc_x * destination->clip_w;
    destination->clip_y = ndc_y * destination->clip_w;
}

void set_projected_position(
    WorldDrawVertex* destination,
    float screen_x,
    float screen_y,
    const WorldDrawList& list
) {
    destination->screen_x = screen_x;
    destination->screen_y = screen_y;
    const float ndc_x =
        ((screen_x - list.display_x) /
            static_cast<float>(list.display_width)) *
            2.0F - 1.0F;
    const float ndc_y =
        1.0F -
        ((screen_y - list.display_y) /
            static_cast<float>(list.display_height)) *
            2.0F;
    destination->clip_x = ndc_x * destination->clip_w;
    destination->clip_y = ndc_y * destination->clip_w;
}

std::int64_t absolute(std::int64_t value) {
    return value < 0 ? -value : value;
}

int dominant_axis(const Position& a, const Position& b) {
    const std::int64_t distance[3] = {
        absolute(static_cast<std::int64_t>(b.x) - a.x),
        absolute(static_cast<std::int64_t>(b.y) - a.y),
        absolute(static_cast<std::int64_t>(b.z) - a.z),
    };
    return distance[1] > distance[0]
        ? (distance[2] > distance[1] ? 2 : 1)
        : (distance[2] > distance[0] ? 2 : 0);
}

std::int32_t coordinate(const Position& point, int axis) {
    return axis == 0 ? point.x : axis == 1 ? point.y : point.z;
}

bool strictly_on_segment(
    const Position& point,
    const Position& a,
    const Position& b
) {
    if (point == a || point == b)
        return false;
    const std::int64_t dx =
        static_cast<std::int64_t>(b.x) - a.x;
    const std::int64_t dy =
        static_cast<std::int64_t>(b.y) - a.y;
    const std::int64_t dz =
        static_cast<std::int64_t>(b.z) - a.z;
    const std::int64_t px =
        static_cast<std::int64_t>(point.x) - a.x;
    const std::int64_t py =
        static_cast<std::int64_t>(point.y) - a.y;
    const std::int64_t pz =
        static_cast<std::int64_t>(point.z) - a.z;
    if (
        dy * pz - dz * py != 0 ||
        dz * px - dx * pz != 0 ||
        dx * py - dy * px != 0
    )
        return false;
    const std::int64_t dot = px * dx + py * dy + pz * dz;
    const std::int64_t length = dx * dx + dy * dy + dz * dz;
    return dot > 0 && dot < length;
}

void recalculate_normal(WorldDrawCommand* command) {
    const auto& a = command->vertices[0];
    const auto& b = command->vertices[1];
    const auto& c = command->vertices[2];
    const float ab_x = b.world_x - a.world_x;
    const float ab_y = b.world_y - a.world_y;
    const float ab_z = b.world_z - a.world_z;
    const float ac_x = c.world_x - a.world_x;
    const float ac_y = c.world_y - a.world_y;
    const float ac_z = c.world_z - a.world_z;
    float x = ab_y * ac_z - ab_z * ac_y;
    float y = ab_z * ac_x - ab_x * ac_z;
    float z = ab_x * ac_y - ab_y * ac_x;
    const float length = std::sqrt(x * x + y * y + z * z);
    if (length > 0.000001F) {
        x /= length;
        y /= length;
        z /= length;
    } else {
        x = y = 0.0F;
        z = 1.0F;
    }
    command->face_normal_x = x;
    command->face_normal_y = y;
    command->face_normal_z = z;
}

WorldDrawVertex edge_vertex(
    const WorldDrawVertex& a,
    const WorldDrawVertex& b,
    const WorldDrawVertex& canonical,
    const Position& point
) {
    WorldDrawVertex result = canonical;
    const Position pa = position(a);
    const Position pb = position(b);
    const int axis = dominant_axis(pa, pb);
    const long double numerator =
        static_cast<long double>(
            coordinate(point, axis) - coordinate(pa, axis));
    const long double denominator =
        static_cast<long double>(
            coordinate(pb, axis) - coordinate(pa, axis));
    const long double t = numerator / denominator;
    const auto blend = [t](float left, float right) {
        return static_cast<float>(
            static_cast<long double>(left) +
            (static_cast<long double>(right) - left) * t);
    };
    result.u = blend(a.u, b.u);
    result.v = blend(a.v, b.v);
    result.r = static_cast<std::uint8_t>(std::clamp(
        std::lround(blend(
            static_cast<float>(a.r),
            static_cast<float>(b.r))),
        0L,
        255L));
    result.g = static_cast<std::uint8_t>(std::clamp(
        std::lround(blend(
            static_cast<float>(a.g),
            static_cast<float>(b.g))),
        0L,
        255L));
    result.b = static_cast<std::uint8_t>(std::clamp(
        std::lround(blend(
            static_cast<float>(a.b),
            static_cast<float>(b.b))),
        0L,
        255L));
    // The canonical vertex supplies the welded position and the identity that
    // lets a later frame match this split, but it belongs to the neighbouring
    // command and therefore carries that model's coordinates and object
    // matrix. Leaving them in place put a foreign model coordinate under this
    // command's transform, so the group could no longer reproduce its own
    // captured view positions and was rejected as incoherent. Interpolate the
    // model coordinate along the edge being split and keep this triangle's
    // own matrix; the residual is then only the sub-unit weld itself.
    result.model_x = static_cast<std::int16_t>(std::lround(blend(
        static_cast<float>(a.model_x), static_cast<float>(b.model_x))));
    result.model_y = static_cast<std::int16_t>(std::lround(blend(
        static_cast<float>(a.model_y), static_cast<float>(b.model_y))));
    result.model_z = static_cast<std::int16_t>(std::lround(blend(
        static_cast<float>(a.model_z), static_cast<float>(b.model_z))));
    bool shared_transform =
        a.exact_transform_valid &&
        b.exact_transform_valid &&
        a.transform_id == b.transform_id;
    for (int component = 0; component < 9; ++component) {
        shared_transform = shared_transform &&
            a.transform_rotation[component] == b.transform_rotation[component];
    }
    for (int component = 0; component < 3; ++component) {
        shared_transform = shared_transform &&
            a.transform_translation[component] ==
                b.transform_translation[component];
    }
    result.transform_id = a.transform_id;
    for (int component = 0; component < 9; ++component)
        result.transform_rotation[component] = a.transform_rotation[component];
    for (int component = 0; component < 3; ++component) {
        result.transform_translation[component] =
            a.transform_translation[component];
    }
    result.exact_transform_valid = shared_transform;
    result.exact_view_x = point.x;
    result.exact_view_y = point.y;
    result.exact_view_z = point.z;
    return result;
}

std::int64_t orient(
    const Point2& a,
    const Point2& b,
    const Point2& c
) {
    return
        (b.x - a.x) * (c.y - a.y) -
        (b.y - a.y) * (c.x - a.x);
}

Point2 project(const Position& point, int dropped_axis) {
    if (dropped_axis == 0)
        return Point2{point.y, point.z};
    if (dropped_axis == 1)
        return Point2{point.x, point.z};
    return Point2{point.x, point.y};
}

std::vector<std::array<int, 3>> triangulate_polygon(
    const std::vector<WorldDrawVertex>& polygon,
    const WorldDrawCommand& source
) {
    std::vector<std::array<int, 3>> result;
    if (polygon.size() < 3)
        return result;
    const Position a = position(source.vertices[0]);
    const Position b = position(source.vertices[1]);
    const Position c = position(source.vertices[2]);
    const std::int64_t nx =
        static_cast<std::int64_t>(b.y - a.y) * (c.z - a.z) -
        static_cast<std::int64_t>(b.z - a.z) * (c.y - a.y);
    const std::int64_t ny =
        static_cast<std::int64_t>(b.z - a.z) * (c.x - a.x) -
        static_cast<std::int64_t>(b.x - a.x) * (c.z - a.z);
    const std::int64_t nz =
        static_cast<std::int64_t>(b.x - a.x) * (c.y - a.y) -
        static_cast<std::int64_t>(b.y - a.y) * (c.x - a.x);
    const int dropped =
        absolute(nx) >= absolute(ny) && absolute(nx) >= absolute(nz)
            ? 0
            : absolute(ny) >= absolute(nz) ? 1 : 2;
    std::vector<Point2> points;
    points.reserve(polygon.size());
    for (const auto& vertex : polygon)
        points.push_back(project(position(vertex), dropped));
    std::int64_t signed_area = 0;
    for (std::size_t i = 0; i < points.size(); ++i) {
        const Point2& p = points[i];
        const Point2& q = points[(i + 1) % points.size()];
        signed_area += p.x * q.y - p.y * q.x;
    }
    const int winding = signed_area >= 0 ? 1 : -1;
    std::vector<int> remaining(points.size());
    std::iota(remaining.begin(), remaining.end(), 0);
    while (remaining.size() > 3) {
        bool clipped = false;
        for (std::size_t i = 0; i < remaining.size(); ++i) {
            const int previous =
                remaining[(i + remaining.size() - 1) % remaining.size()];
            const int current = remaining[i];
            const int next = remaining[(i + 1) % remaining.size()];
            const std::int64_t corner =
                orient(points[previous], points[current], points[next]);
            if ((winding > 0 && corner <= 0) ||
                (winding < 0 && corner >= 0))
                continue;
            bool contains = false;
            for (int candidate : remaining) {
                if (
                    candidate == previous ||
                    candidate == current ||
                    candidate == next
                )
                    continue;
                const std::int64_t o0 =
                    orient(points[previous], points[current],
                           points[candidate]);
                const std::int64_t o1 =
                    orient(points[current], points[next],
                           points[candidate]);
                const std::int64_t o2 =
                    orient(points[next], points[previous],
                           points[candidate]);
                const bool inside = winding > 0
                    ? o0 >= 0 && o1 >= 0 && o2 >= 0
                    : o0 <= 0 && o1 <= 0 && o2 <= 0;
                if (inside) {
                    contains = true;
                    break;
                }
            }
            if (contains)
                continue;
            result.push_back({previous, current, next});
            remaining.erase(remaining.begin() +
                static_cast<std::ptrdiff_t>(i));
            clipped = true;
            break;
        }
        if (!clipped) {
            result.clear();
            return result;
        }
    }
    if (remaining.size() == 3 &&
        orient(
            points[remaining[0]],
            points[remaining[1]],
            points[remaining[2]]) != 0)
        result.push_back({
            remaining[0], remaining[1], remaining[2]});
    return result;
}

std::int64_t gcd3(
    std::int64_t a,
    std::int64_t b,
    std::int64_t c
) {
    return std::gcd(std::gcd(absolute(a), absolute(b)), absolute(c));
}

bool plane(const WorldDrawCommand& command, Plane* output) {
    const Position a = position(command.vertices[0]);
    const Position b = position(command.vertices[1]);
    const Position c = position(command.vertices[2]);
    std::int64_t x =
        static_cast<std::int64_t>(b.y - a.y) * (c.z - a.z) -
        static_cast<std::int64_t>(b.z - a.z) * (c.y - a.y);
    std::int64_t y =
        static_cast<std::int64_t>(b.z - a.z) * (c.x - a.x) -
        static_cast<std::int64_t>(b.x - a.x) * (c.z - a.z);
    std::int64_t z =
        static_cast<std::int64_t>(b.x - a.x) * (c.y - a.y) -
        static_cast<std::int64_t>(b.y - a.y) * (c.x - a.x);
    const std::int64_t divisor = gcd3(x, y, z);
    if (divisor == 0)
        return false;
    x /= divisor;
    y /= divisor;
    z /= divisor;
    if (x < 0 || (x == 0 && y < 0) ||
        (x == 0 && y == 0 && z < 0)) {
        x = -x;
        y = -y;
        z = -z;
    }
    *output = Plane{
        x, y, z,
        -(x * a.x + y * a.y + z * a.z),
    };
    return true;
}

bool same_geometry(
    const WorldDrawCommand& left,
    const WorldDrawCommand& right
) {
    std::array<Position, 3> a{
        position(left.vertices[0]),
        position(left.vertices[1]),
        position(left.vertices[2]),
    };
    std::array<Position, 3> b{
        position(right.vertices[0]),
        position(right.vertices[1]),
        position(right.vertices[2]),
    };
    std::sort(a.begin(), a.end());
    std::sort(b.begin(), b.end());
    return a == b;
}

bool strict_inside(
    const Point2& point,
    const std::array<Point2, 3>& triangle
) {
    const std::int64_t a =
        orient(triangle[0], triangle[1], point);
    const std::int64_t b =
        orient(triangle[1], triangle[2], point);
    const std::int64_t c =
        orient(triangle[2], triangle[0], point);
    return
        (a > 0 && b > 0 && c > 0) ||
        (a < 0 && b < 0 && c < 0);
}

bool proper_intersection(
    const Point2& a,
    const Point2& b,
    const Point2& c,
    const Point2& d
) {
    const std::int64_t ab_c = orient(a, b, c);
    const std::int64_t ab_d = orient(a, b, d);
    const std::int64_t cd_a = orient(c, d, a);
    const std::int64_t cd_b = orient(c, d, b);
    return
        ((ab_c < 0 && ab_d > 0) || (ab_c > 0 && ab_d < 0)) &&
        ((cd_a < 0 && cd_b > 0) || (cd_a > 0 && cd_b < 0));
}

bool positive_overlap(
    const WorldDrawCommand& left,
    const WorldDrawCommand& right,
    const Plane& common_plane
) {
    if (same_geometry(left, right))
        return true;
    const int dropped =
        absolute(common_plane.x) >= absolute(common_plane.y) &&
        absolute(common_plane.x) >= absolute(common_plane.z)
            ? 0
            : absolute(common_plane.y) >= absolute(common_plane.z) ? 1 : 2;
    std::array<Point2, 3> a{};
    std::array<Point2, 3> b{};
    for (int index = 0; index < 3; ++index) {
        a[index] = project(position(left.vertices[index]), dropped);
        b[index] = project(position(right.vertices[index]), dropped);
    }
    for (const auto& point : a)
        if (strict_inside(point, b))
            return true;
    for (const auto& point : b)
        if (strict_inside(point, a))
            return true;
    for (int i = 0; i < 3; ++i)
        for (int j = 0; j < 3; ++j)
            if (proper_intersection(
                    a[i], a[(i + 1) % 3],
                    b[j], b[(j + 1) % 3]))
                return true;
    return false;
}

bool same_material(
    const WorldDrawCommand& left,
    const WorldDrawCommand& right
) {
    return left.material_index == right.material_index;
}

auto ownership_key(const WorldDrawCommand& command) {
    std::array<std::uint32_t, 3> pointers{
        command.vertices[0].source_vertex_identity,
        command.vertices[1].source_vertex_identity,
        command.vertices[2].source_vertex_identity,
    };
    std::sort(pointers.begin(), pointers.end());
    return std::make_tuple(
        command.object_id,
        command.model_pointer,
        pointers,
        command.source_command_index);
}

struct DisjointSet {
    std::pmr::vector<std::size_t> parent;

    explicit DisjointSet(
        std::size_t count,
        std::pmr::memory_resource* resource
    ) : parent(
        count,
        std::pmr::polymorphic_allocator<std::size_t>{resource}) {
        std::iota(parent.begin(), parent.end(), 0);
    }

    std::size_t find(std::size_t item) {
        while (parent[item] != item) {
            parent[item] = parent[parent[item]];
            item = parent[item];
        }
        return item;
    }

    void join(std::size_t left, std::size_t right) {
        left = find(left);
        right = find(right);
        if (left != right)
            parent[right] = left;
    }
};

} // namespace

WorldTopologyResult apply_world_topology(
    WorldDrawList* draw_list,
    WorldTopologyOptions options,
    WorldTopologyStats* output_stats
) noexcept {
    if (draw_list == nullptr || output_stats == nullptr)
        return WorldTopologyResult::invalid_argument;
    try {
        using TopologyClock = std::chrono::steady_clock;
        const auto topology_started = TopologyClock::now();
        auto setup_finished = topology_started;
        auto seams_finished = topology_started;
        auto raster_finished = topology_started;
        auto lod_finished = topology_started;
        auto projected_finished = topology_started;
        auto exact_finished = topology_started;
        auto ownership_finished = topology_started;
        WorldTopologyStats stats{};
        // Diagnostics are process-wide switches. Read them once: querying the
        // CRT environment in every edge/candidate iteration dominated this
        // pass on full-distance race packets.
        const bool adjacent_diagnostics =
            std::getenv("OPENGT_TOPOLOGY_ADJACENT_DIAGNOSTICS") != nullptr;
        const bool projected_pair_diagnostics =
            std::getenv(
                "OPENGT_TOPOLOGY_PROJECTED_PAIR_DIAGNOSTICS") != nullptr;
        // Topology rebuilds several thousand short-lived lookup nodes for each
        // authored field. A frame-local arena preserves the exact containers
        // and iteration semantics while removing general-heap allocation and
        // deallocation from the 59.94 Hz path.
        // Dense GT2 fields retain the setup indexes while the raster, LOD,
        // projected-edge and exact-boundary indexes are built. Four MiB was
        // enough for light scenes but forced the later phases of a crowded
        // race through monotonic_buffer_resource's general-heap fallback.
        // Keep the full authored-field working set in thread-local scratch.
        static thread_local std::array<std::byte, 32U * 1024U * 1024U>
            topology_arena_storage{};
        std::pmr::monotonic_buffer_resource topology_arena(
            topology_arena_storage.data(), topology_arena_storage.size());
        stats.input_commands =
            static_cast<std::uint32_t>(draw_list->commands.size());

        using PositionOccurrences = std::pmr::unordered_map<
            Position,
            OccurrenceList,
            PositionHash>;
        PositionOccurrences positions{&topology_arena};
        using ModelOccurrences =
            PositionOccurrences;
        std::pmr::unordered_map<std::uint32_t, ModelOccurrences>
            object_models{&topology_arena};
        using PositionModels = std::pmr::unordered_map<
            Position,
            std::pmr::unordered_set<Position, PositionHash>,
            PositionHash>;
        std::pmr::unordered_map<
            std::uint32_t,
            PositionModels> object_view_models{&topology_arena};
        std::pmr::unordered_map<
            std::uint32_t,
            PositionOccurrences> object_view_occurrences{&topology_arena};
        std::pmr::unordered_map<
            std::uint32_t,
            std::pmr::unordered_map<Edge, std::uint32_t, EdgeHash>>
            object_edge_counts{&topology_arena};
        std::pmr::unordered_map<std::uint32_t, std::size_t>
            object_command_counts{&topology_arena};
        std::pmr::unordered_set<std::uint64_t> sources{&topology_arena};
        object_models.reserve(64);
        object_view_models.reserve(64);
        object_view_occurrences.reserve(64);
        object_edge_counts.reserve(64);
        object_command_counts.reserve(64);
        std::pmr::vector<std::size_t> eligible_commands{&topology_arena};
        eligible_commands.reserve(draw_list->track_commands);
        for (std::size_t command_index = 0;
             command_index < draw_list->commands.size();
             ++command_index) {
            const auto& command = draw_list->commands[command_index];
            if (eligible(*draw_list, command)) {
                eligible_commands.push_back(command_index);
                ++stats.eligible_track_commands;
                ++object_command_counts[command.object_id];
            } else if (
                command.object_kind == 1 &&
                command.channel == WorldViewChannel::main_view &&
                !resident_course_command(*draw_list, command)
            ) {
                ++stats.skipped_without_provenance;
            }
        }
        const std::size_t eligible_vertex_capacity =
            static_cast<std::size_t>(stats.eligible_track_commands) * 3U;
        positions.reserve(eligible_vertex_capacity);
        sources.reserve(eligible_vertex_capacity);
        for (const std::size_t command_index : eligible_commands) {
            const auto& command = draw_list->commands[command_index];
            const std::size_t object_vertex_capacity =
                object_command_counts.at(command.object_id) * 3U;
            auto [models_entry, models_inserted] =
                object_models.try_emplace(command.object_id);
            auto [view_models_entry, view_models_inserted] =
                object_view_models.try_emplace(command.object_id);
            auto [view_occurrences_entry, view_occurrences_inserted] =
                object_view_occurrences.try_emplace(command.object_id);
            auto [edge_counts_entry, edge_counts_inserted] =
                object_edge_counts.try_emplace(command.object_id);
            if (models_inserted)
                models_entry->second.reserve(object_vertex_capacity);
            if (view_models_inserted)
                view_models_entry->second.reserve(object_vertex_capacity);
            if (view_occurrences_inserted)
                view_occurrences_entry->second.reserve(object_vertex_capacity);
            if (edge_counts_inserted)
                edge_counts_entry->second.reserve(object_vertex_capacity);
            auto& models = models_entry->second;
            auto& view_models = view_models_entry->second;
            auto& view_occurrences = view_occurrences_entry->second;
            auto& edge_counts = edge_counts_entry->second;
            std::array<Position, 3> views{};
            for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
                const auto& vertex = command.vertices[vertex_index];
                const Position view = position(vertex);
                views[vertex_index] = view;
                const Occurrence occurrence{
                    command_index, vertex_index};
                positions[view].push_back(occurrence);
                const Position model = model_position(vertex);
                models[model].push_back(occurrence);
                view_models[view].insert(model);
                view_occurrences[view].push_back(occurrence);
                sources.emplace(
                    (static_cast<std::uint64_t>(command.model_pointer) << 32) |
                    vertex.source_vertex_identity);
            }
            for (int edge_index = 0; edge_index < 3; ++edge_index) {
                ++edge_counts[Edge{
                    views[edge_index],
                    views[(edge_index + 1) % 3],
                }];
            }
        }
        stats.unique_source_vertices =
            static_cast<std::uint32_t>(sources.size());

        std::pmr::unordered_map<
            std::uint32_t,
            std::pmr::unordered_set<Position, PositionHash>>
            object_boundary_positions{&topology_arena};
        object_boundary_positions.reserve(object_edge_counts.size());
        for (const auto& object : object_edge_counts) {
            auto& boundary = object_boundary_positions[object.first];
            for (const auto& edge : object.second) {
                if (edge.second == 1) {
                    boundary.insert(edge.first.a);
                    boundary.insert(edge.first.b);
                }
            }
        }
        std::pmr::set<std::pair<std::uint32_t, std::uint32_t>>
            proven_adjacent_objects{&topology_arena};
        const auto adjacent_key = [] (
            std::uint32_t left,
            std::uint32_t right
        ) {
            return left < right
                ? std::make_pair(left, right)
                : std::make_pair(right, left);
        };

        std::pmr::unordered_map<Position, Occurrence, PositionHash>
            canonical_occurrences{&topology_arena};
        canonical_occurrences.reserve(positions.size());
        for (auto& entry : positions) {
            auto& occurrences = entry.second;
            const auto canonical = *std::min_element(
                occurrences.begin(), occurrences.end(),
                [&](const Occurrence& left, const Occurrence& right) {
                    return occurrence_key(*draw_list, left) <
                        occurrence_key(*draw_list, right);
                });
            canonical_occurrences.emplace(entry.first, canonical);
            if (occurrences.size() < 2)
                continue;
            ++stats.exact_position_groups;
            bool has_authored_source = false;
            bool has_distinct_authored_sources = false;
            std::uint64_t first_authored_source = 0;
            for (const auto& occurrence : occurrences) {
                const auto& command =
                    draw_list->commands[occurrence.command];
                const auto& vertex =
                    command.vertices[occurrence.vertex];
                const std::uint64_t authored_source =
                    (static_cast<std::uint64_t>(command.model_pointer) << 32) |
                    vertex.source_vertex_identity;
                if (!has_authored_source) {
                    first_authored_source = authored_source;
                    has_authored_source = true;
                } else if (authored_source != first_authored_source) {
                    has_distinct_authored_sources = true;
                    break;
                }
            }
            if (!has_distinct_authored_sources)
                continue;
            ++stats.authored_boundary_groups;
            if (!options.join_authored_boundaries)
                continue;
            const auto canonical_vertex =
                draw_list->commands[canonical.command]
                    .vertices[canonical.vertex];
            for (const auto& occurrence : occurrences) {
                auto& vertex =
                    draw_list->commands[occurrence.command]
                        .vertices[occurrence.vertex];
                const bool changed =
                    vertex.world_x != canonical_vertex.world_x ||
                    vertex.world_y != canonical_vertex.world_y ||
                    vertex.world_z != canonical_vertex.world_z ||
                    vertex.clip_x != canonical_vertex.clip_x ||
                    vertex.clip_y != canonical_vertex.clip_y ||
                    vertex.clip_w != canonical_vertex.clip_w;
                copy_geometric_fields(&vertex, canonical_vertex);
                if (changed)
                    ++stats.adjusted_vertex_instances;
            }
        }
        setup_finished = TopologyClock::now();

        std::uint64_t projected_edge_reference_count = 0;
        std::uint64_t projected_candidate_points = 0;
        std::uint64_t projected_candidate_edge_tests = 0;
        std::uint64_t projected_visible_candidate_points = 0;
        std::uint64_t projected_offscreen_candidate_points = 0;

        if (draw_list->continuous_projection) {
            // Determine the exact 4096-unit coordinate-cell translation
            // between each pair of consecutive track sections.  A candidate
            // translation must be demonstrated by at least two distinct
            // authored vertices (an edge), either through identical raw model
            // coordinates or through vertices that already coincide in exact
            // GTE view space.  No proximity or screen-space threshold is used.
            for (auto left_object = object_models.begin();
                 left_object != object_models.end();
                 ++left_object) {
                const auto right_object =
                    object_models.find(left_object->first + 1U);
                if (right_object == object_models.end())
                    continue;
                std::pmr::set<std::pair<Position, Position>> demonstrated{
                    &topology_arena};
                for (const auto& left : left_object->second) {
                    if (right_object->second.find(left.first) !=
                        right_object->second.end())
                        demonstrated.emplace(left.first, left.first);
                }
                const auto& left_views =
                    object_view_models[left_object->first];
                const auto& right_views =
                    object_view_models[right_object->first];
                for (const auto& left : left_views) {
                    const auto right = right_views.find(left.first);
                    if (right == right_views.end())
                        continue;
                    for (const Position& left_model : left.second)
                        for (const Position& right_model : right->second)
                            demonstrated.emplace(
                                left_model, right_model);
                }
                std::pmr::map<Position, std::uint32_t> translations{
                    &topology_arena};
                for (const auto& pair : demonstrated) {
                    const Position delta{
                        pair.second.x - pair.first.x,
                        pair.second.y - pair.first.y,
                        pair.second.z - pair.first.z,
                    };
                    if (
                        delta.x % 4096 == 0 &&
                        delta.y % 4096 == 0 &&
                        delta.z % 4096 == 0
                    )
                        ++translations[delta];
                }
                if (translations.empty())
                    continue;
                const auto best = std::max_element(
                    translations.begin(), translations.end(),
                    [](const auto& left, const auto& right) {
                        if (left.second != right.second)
                            return left.second < right.second;
                        return right.first < left.first;
                    });
                if (adjacent_diagnostics) {
                    std::fprintf(
                        stderr,
                        "[Topology-Adjacent] left=%u right=%u "
                        "candidates=%zu best=%u delta=(%d,%d,%d)\n",
                        left_object->first,
                        right_object->first,
                        translations.size(),
                        best->second,
                        best->first.x,
                        best->first.y,
                        best->first.z);
                }
                if (best->second < 2)
                    continue;
                proven_adjacent_objects.insert(adjacent_key(
                    left_object->first,
                    right_object->first));
                const Position delta = best->first;
                for (const auto& left : left_object->second) {
                    const Position target{
                        left.first.x + delta.x,
                        left.first.y + delta.y,
                        left.first.z + delta.z,
                    };
                    const auto right =
                        right_object->second.find(target);
                    if (right == right_object->second.end())
                        continue;
                    std::pmr::vector<Occurrence> component{&topology_arena};
                    component.reserve(
                        left.second.size() + right->second.size());
                    component.insert(
                        component.end(),
                        left.second.begin(),
                        left.second.end());
                    component.insert(
                        component.end(),
                        right->second.begin(),
                        right->second.end());
                    ++stats.authored_projection_groups;
                    const auto canonical = *std::min_element(
                        component.begin(), component.end(),
                        [&](const Occurrence& a, const Occurrence& b) {
                            return occurrence_key(*draw_list, a) <
                                occurrence_key(*draw_list, b);
                        });
                    const auto canonical_vertex =
                        draw_list->commands[canonical.command]
                            .vertices[canonical.vertex];
                    for (const auto& occurrence : component) {
                        auto& vertex =
                            draw_list->commands[occurrence.command]
                                .vertices[occurrence.vertex];
                        const bool changed =
                            vertex.screen_x != canonical_vertex.screen_x ||
                            vertex.screen_y != canonical_vertex.screen_y;
                        copy_projected_position(
                            &vertex, canonical_vertex, *draw_list);
                        if (changed)
                            ++stats.adjusted_projection_instances;
                    }
                }

                // Some neighboring GT2 track chunks terminate the same
                // visible seam with different tessellation. Their exact
                // authored anchors prove the section relationship above,
                // but the intermediate vertices are not copies in 3D and
                // can differ by a fraction of one native pixel after smooth
                // projection. At 4x that exposes the old background layer.
                // Join only mutually-nearest boundary vertices of the same
                // material/OT layer, and never search beyond 3/4 native px.
                const auto left_boundary_it =
                    object_boundary_positions.find(left_object->first);
                const auto right_boundary_it =
                    object_boundary_positions.find(right_object->first);
                if (
                    left_boundary_it == object_boundary_positions.end() ||
                    right_boundary_it == object_boundary_positions.end()
                )
                    continue;
                const auto& left_occurrences =
                    object_view_occurrences[left_object->first];
                const auto& right_occurrences =
                    object_view_occurrences[right_object->first];
                constexpr float maximum_seam_distance_squared =
                    0.75F * 0.75F;
                using PositionMatch = std::pmr::unordered_map<
                    Position, Position, PositionHash>;
                PositionMatch left_to_right{&topology_arena};
                PositionMatch right_to_left{&topology_arena};
                left_to_right.reserve(left_boundary_it->second.size());
                right_to_left.reserve(right_boundary_it->second.size());
                const auto find_matches = [&] (
                    const auto& source_boundary,
                    const auto& target_boundary,
                    const auto& source_groups,
                    const auto& target_groups,
                    PositionMatch* matches
                ) {
                    constexpr int seam_cell_size = 2;
                    std::pmr::unordered_map<
                        std::uint64_t,
                        std::pmr::vector<Position>> target_bins{
                            &topology_arena};
                    target_bins.reserve(target_boundary.size());
                    const auto cell_key = [] (int x, int y) {
                        return
                            (static_cast<std::uint64_t>(
                                static_cast<std::uint32_t>(x)) << 32) |
                            static_cast<std::uint32_t>(y);
                    };
                    for (const Position& target : target_boundary) {
                        const auto target_group = target_groups.find(target);
                        if (target_group == target_groups.end())
                            continue;
                        const auto& target_vertex =
                            draw_list->commands[
                                target_group->second.front().command]
                                .vertices[
                                    target_group->second.front().vertex];
                        const int cell_x = static_cast<int>(std::floor(
                            target_vertex.screen_x / seam_cell_size));
                        const int cell_y = static_cast<int>(std::floor(
                            target_vertex.screen_y / seam_cell_size));
                        target_bins[cell_key(cell_x, cell_y)].push_back(target);
                    }
                    for (const Position& source : source_boundary) {
                        const auto source_group = source_groups.find(source);
                        if (source_group == source_groups.end() ||
                            target_groups.find(source) != target_groups.end())
                            continue;
                        const auto& source_vertex =
                            draw_list->commands[
                                source_group->second.front().command]
                                .vertices[
                                    source_group->second.front().vertex];
                        bool found = false;
                        Position nearest{};
                        float nearest_distance =
                            maximum_seam_distance_squared;
                        const int source_cell_x = static_cast<int>(std::floor(
                            source_vertex.screen_x / seam_cell_size));
                        const int source_cell_y = static_cast<int>(std::floor(
                            source_vertex.screen_y / seam_cell_size));
                        for (int cell_y = source_cell_y - 1;
                             cell_y <= source_cell_y + 1;
                             ++cell_y) {
                            for (int cell_x = source_cell_x - 1;
                                 cell_x <= source_cell_x + 1;
                                 ++cell_x) {
                                const auto bin = target_bins.find(
                                    cell_key(cell_x, cell_y));
                                if (bin == target_bins.end())
                                    continue;
                                for (const Position& target : bin->second) {
                                    if (source_groups.find(target) !=
                                        source_groups.end())
                                        continue;
                                    const auto target_group =
                                        target_groups.find(target);
                                    if (
                                        target_group == target_groups.end() ||
                                        !material_compatible(
                                            *draw_list,
                                            source_group->second,
                                            target_group->second)
                                    )
                                        continue;
                                    const auto& target_vertex =
                                        draw_list->commands[
                                            target_group->second.front().command]
                                            .vertices[
                                                target_group->second.front().vertex];
                                    const float dx =
                                        source_vertex.screen_x -
                                        target_vertex.screen_x;
                                    const float dy =
                                        source_vertex.screen_y -
                                        target_vertex.screen_y;
                                    const float distance = dx * dx + dy * dy;
                                    if (
                                        distance < nearest_distance ||
                                        (distance == nearest_distance &&
                                            (!found || target < nearest))
                                    ) {
                                        found = true;
                                        nearest = target;
                                        nearest_distance = distance;
                                    }
                                }
                            }
                        }
                        if (found)
                            matches->emplace(source, nearest);
                    }
                };
                find_matches(
                    left_boundary_it->second,
                    right_boundary_it->second,
                    left_occurrences,
                    right_occurrences,
                    &left_to_right);
                find_matches(
                    right_boundary_it->second,
                    left_boundary_it->second,
                    right_occurrences,
                    left_occurrences,
                    &right_to_left);
                for (const auto& match : left_to_right) {
                    const auto reverse = right_to_left.find(match.second);
                    if (
                        reverse == right_to_left.end() ||
                        !(reverse->second == match.first)
                    )
                        continue;
                    const auto left_group =
                        left_occurrences.find(match.first);
                    const auto right_group =
                        right_occurrences.find(match.second);
                    if (
                        left_group == left_occurrences.end() ||
                        right_group == right_occurrences.end()
                    )
                        continue;
                    std::pmr::vector<Occurrence> component{&topology_arena};
                    component.reserve(
                        left_group->second.size() +
                        right_group->second.size());
                    component.insert(
                        component.end(),
                        left_group->second.begin(),
                        left_group->second.end());
                    component.insert(
                        component.end(),
                        right_group->second.begin(),
                        right_group->second.end());
                    const auto canonical = *std::min_element(
                        component.begin(), component.end(),
                        [&](const Occurrence& a, const Occurrence& b) {
                            return occurrence_key(*draw_list, a) <
                                occurrence_key(*draw_list, b);
                        });
                    const auto canonical_vertex =
                        draw_list->commands[canonical.command]
                            .vertices[canonical.vertex];
                    ++stats.projected_seam_groups;
                    for (const auto& occurrence : component) {
                        auto& vertex =
                            draw_list->commands[occurrence.command]
                                .vertices[occurrence.vertex];
                        const bool changed =
                            vertex.screen_x != canonical_vertex.screen_x ||
                            vertex.screen_y != canonical_vertex.screen_y;
                        copy_projected_position(
                            &vertex, canonical_vertex, *draw_list);
                        if (changed)
                            ++stats.adjusted_seam_instances;
                    }
                }
            }
            seams_finished = TopologyClock::now();

            // Road LOD endpoints can use different view-space scales while
            // landing on the same authored SXY pixel. Preserve that exact
            // raster identity, but join only copies in the same object,
            // model, transform, material and OT layer whose modern projected
            // positions remain within one half native pixel. The scale proof
            // is exact and restrictive; the wider bound is needed for GT2
            // road endpoints whose two 8x LOD projections differ by roughly
            // 0.27 native px, which otherwise exposes a clear-color sliver at
            // 4x output resolution.
            std::pmr::unordered_map<
                std::uint32_t,
                std::pmr::unordered_set<Position, PositionHash>>
                raster_joined_positions{&topology_arena};
            std::pmr::unordered_map<
                RasterLayerKey,
                std::pmr::vector<Occurrence>,
                RasterLayerKeyHash> raster_groups{&topology_arena};
            raster_groups.reserve(stats.eligible_track_commands * 3U);
            for (const std::size_t command_index : eligible_commands) {
                const auto& command = draw_list->commands[command_index];
                for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
                    const auto& vertex = command.vertices[vertex_index];
                    raster_groups[RasterLayerKey{
                        command.object_id,
                        command.model_pointer,
                        command.transform_id,
                        command.material_index,
                        command.ordering_table_index,
                        vertex.authored_screen_x,
                        vertex.authored_screen_y,
                    }].push_back(Occurrence{command_index, vertex_index});
                }
            }
            constexpr float maximum_raster_join_distance_squared =
                0.625F * 0.625F;
            for (const auto& group : raster_groups) {
                if (group.second.size() < 2)
                    continue;
                std::pmr::unordered_set<Position, PositionHash>
                    view_positions{&topology_arena};
                view_positions.reserve(group.second.size());
                for (const auto& occurrence : group.second) {
                    view_positions.insert(position(
                        draw_list->commands[occurrence.command]
                            .vertices[occurrence.vertex]));
                }
                if (view_positions.size() < 2)
                    continue;
                // Same authored pixel alone is not enough; unrelated road
                // surfaces can cross that pixel. A genuine GT2 LOD copy uses
                // the same transform and power-of-two-scaled integer view
                // coordinates (the observed road bridge is 8x), with only a
                // few guest units of GTE rounding error.
                const Position reference = *std::min_element(
                    view_positions.begin(), view_positions.end(),
                    [](const Position& left, const Position& right) {
                        return std::abs(left.z) < std::abs(right.z);
                    });
                bool scale_proven = reference.z != 0;
                constexpr std::array<std::int32_t, 4> lod_scales{
                    2, 4, 8, 16};
                for (const Position& candidate : view_positions) {
                    if (candidate == reference)
                        continue;
                    bool matches_scale = false;
                    for (const std::int32_t scale : lod_scales) {
                        const auto near_scaled = [](std::int32_t value,
                                                    std::int32_t base,
                                                    std::int32_t factor) {
                            // The exact authored-pixel LOD pair at the live
                            // road seam has an eight-times scale with a
                            // nine-unit Y residual from integer GTE rounding.
                            // Scale the tight allowance with the proven LOD
                            // factor instead of rejecting that endpoint by a
                            // single source-space unit.
                            const std::int64_t tolerance = factor + 4;
                            return std::llabs(
                                static_cast<std::int64_t>(value) -
                                static_cast<std::int64_t>(base) * factor) <=
                                tolerance;
                        };
                        if (
                            near_scaled(candidate.x, reference.x, scale) &&
                            near_scaled(candidate.y, reference.y, scale) &&
                            near_scaled(candidate.z, reference.z, scale)
                        ) {
                            matches_scale = true;
                            break;
                        }
                    }
                    if (!matches_scale) {
                        scale_proven = false;
                        break;
                    }
                }
                if (!scale_proven)
                    continue;
                const Occurrence canonical = *std::min_element(
                    group.second.begin(), group.second.end(),
                    [&](const Occurrence& left, const Occurrence& right) {
                        return occurrence_key(*draw_list, left) <
                            occurrence_key(*draw_list, right);
                    });
                const auto canonical_vertex =
                    draw_list->commands[canonical.command]
                        .vertices[canonical.vertex];
                bool bounded = true;
                for (const auto& occurrence : group.second) {
                    const auto& vertex =
                        draw_list->commands[occurrence.command]
                            .vertices[occurrence.vertex];
                    const float dx =
                        vertex.screen_x - canonical_vertex.screen_x;
                    const float dy =
                        vertex.screen_y - canonical_vertex.screen_y;
                    if (dx * dx + dy * dy >
                        maximum_raster_join_distance_squared) {
                        bounded = false;
                        break;
                    }
                }
                if (!bounded)
                    continue;
                ++stats.authored_raster_groups;
                const auto& canonical_command =
                    draw_list->commands[canonical.command];
                for (const auto& seed : group.second) {
                    const auto& seed_command =
                        draw_list->commands[seed.command];
                    const Position seed_position = position(
                        seed_command.vertices[seed.vertex]);
                    raster_joined_positions[seed_command.object_id].insert(
                        seed_position);
                    const auto object_it = object_view_occurrences.find(
                        seed_command.object_id);
                    if (object_it == object_view_occurrences.end())
                        continue;
                    const auto exact_group =
                        object_it->second.find(seed_position);
                    if (exact_group == object_it->second.end())
                        continue;
                    // Once a same-material LOD copy proves the target, move
                    // every material occurrence of that exact geometric
                    // vertex. Otherwise the road may close while an adjacent
                    // shoulder triangle tears away from it.
                    for (const auto& occurrence : exact_group->second) {
                        auto& command =
                            draw_list->commands[occurrence.command];
                        if (
                            command.model_pointer !=
                                canonical_command.model_pointer ||
                            command.transform_id !=
                                canonical_command.transform_id ||
                            command.ordering_table_index !=
                                canonical_command.ordering_table_index
                        )
                            continue;
                        auto& vertex = command.vertices[occurrence.vertex];
                        const bool changed =
                            vertex.screen_x != canonical_vertex.screen_x ||
                            vertex.screen_y != canonical_vertex.screen_y;
                        copy_projected_position(
                            &vertex, canonical_vertex, *draw_list);
                        if (changed)
                            ++stats.adjusted_authored_raster_instances;
                    }
                }
            }
            raster_finished = TopologyClock::now();

            // Some 8x road-LOD endpoints either quantize to adjacent authored
            // SXY pixels or share an exact authored pixel bucket with an
            // unrelated surface. The all-or-nothing raster grouping above
            // cannot safely join either case. Evaluate individual,
            // mutually-nearest vertices in the same complete render layer
            // when all three view coordinates demonstrate a 2/4/8/16x
            // relationship, authored SXY differs by at most one, and
            // continuous projection differs by no more than half a native
            // pixel. Do not require a globally open boundary: one side of a
            // LOD seam can be manifold inside its own tessellated strip. This
            // closes the endpoint first; the projected T-junction pass below
            // can then place differently-tessellated intermediate vertices
            // on the edge.
            struct LodBoundaryVertex {
                Position view;
                Occurrence occurrence;
                std::int32_t authored_x;
                std::int32_t authored_y;
            };
            std::pmr::unordered_map<
                LodLayerKey,
                std::pmr::unordered_map<
                    Position,
                    LodBoundaryVertex,
                    PositionHash>,
                LodLayerKeyHash> lod_layers{&topology_arena};
            lod_layers.reserve(object_edge_counts.size() * 4U);
            for (const std::size_t command_index : eligible_commands) {
                const auto& command = draw_list->commands[command_index];
                const LodLayerKey layer{
                    command.object_id,
                    command.model_pointer,
                    command.transform_id,
                    command.material_index,
                    command.ordering_table_index,
                };
                for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
                    const auto& vertex = command.vertices[vertex_index];
                    const Position view = position(vertex);
                    const Occurrence occurrence{
                        command_index, vertex_index};
                    auto& vertices = lod_layers[layer];
                    const auto existing = vertices.find(view);
                    if (
                        existing == vertices.end() ||
                        occurrence_key(*draw_list, occurrence) <
                            occurrence_key(
                                *draw_list,
                                existing->second.occurrence)
                    ) {
                        vertices[view] = LodBoundaryVertex{
                            view,
                            occurrence,
                            vertex.authored_screen_x,
                            vertex.authored_screen_y,
                        };
                    }
                }
            }
            constexpr std::array<std::int32_t, 4> lod_scales{
                2, 4, 8, 16};
            const auto scaled_view_match = [&] (
                const Position& left,
                const Position& right
            ) {
                const auto near_scaled = [](
                    std::int32_t value,
                    std::int32_t base,
                    std::int32_t factor
                ) {
                    // GTE integer transforms can accumulate slightly more
                    // than one source-space unit when the same endpoint is
                    // authored at a higher LOD scale. At 8x, GT2 contains a
                    // proven road pair with a nine-unit residual. Keep the
                    // allowance proportional and still far below one vertex
                    // interval in the higher-resolution mesh.
                    const std::int64_t tolerance = factor + 4;
                    return std::llabs(
                        static_cast<std::int64_t>(value) -
                        static_cast<std::int64_t>(base) * factor) <=
                        tolerance;
                };
                for (const std::int32_t factor : lod_scales) {
                    const bool right_from_left =
                        near_scaled(right.x, left.x, factor) &&
                        near_scaled(right.y, left.y, factor) &&
                        near_scaled(right.z, left.z, factor);
                    const bool left_from_right =
                        near_scaled(left.x, right.x, factor) &&
                        near_scaled(left.y, right.y, factor) &&
                        near_scaled(left.z, right.z, factor);
                    if (right_from_left || left_from_right)
                        return true;
                }
                return false;
            };
            for (const auto& layer : lod_layers) {
                std::pmr::vector<LodBoundaryVertex> vertices{
                    &topology_arena};
                vertices.reserve(layer.second.size());
                for (const auto& entry : layer.second)
                    vertices.push_back(entry.second);
                const std::size_t no_match = vertices.size();
                std::pmr::vector<std::size_t> nearest(
                    vertices.size(), no_match, &topology_arena);
                std::pmr::vector<float> nearest_distance(
                    vertices.size(),
                    maximum_raster_join_distance_squared,
                    &topology_arena);
                std::pmr::unordered_map<
                    std::pair<std::int32_t, std::int32_t>,
                    std::pmr::vector<std::size_t>,
                    AuthoredPixelHash> authored_bins{&topology_arena};
                authored_bins.reserve(vertices.size());
                for (std::size_t index = 0; index < vertices.size(); ++index) {
                    authored_bins[{
                        vertices[index].authored_x,
                        vertices[index].authored_y,
                    }].push_back(index);
                }
                for (std::size_t left = 0; left < vertices.size(); ++left) {
                    const auto& left_vertex = draw_list->commands[
                        vertices[left].occurrence.command].vertices[
                            vertices[left].occurrence.vertex];
                    for (std::int32_t authored_dy = -1;
                         authored_dy <= 1;
                         ++authored_dy) {
                        for (std::int32_t authored_dx = -1;
                             authored_dx <= 1;
                             ++authored_dx) {
                            const auto bin = authored_bins.find({
                                vertices[left].authored_x + authored_dx,
                                vertices[left].authored_y + authored_dy,
                            });
                            if (bin == authored_bins.end())
                                continue;
                            for (const std::size_t right : bin->second) {
                                if (right <= left ||
                                    !scaled_view_match(
                                        vertices[left].view,
                                        vertices[right].view))
                                    continue;
                                const auto& right_vertex = draw_list->commands[
                                    vertices[right].occurrence.command].vertices[
                                        vertices[right].occurrence.vertex];
                                const float dx =
                                    left_vertex.screen_x - right_vertex.screen_x;
                                const float dy =
                                    left_vertex.screen_y - right_vertex.screen_y;
                                const float distance = dx * dx + dy * dy;
                                if (distance >
                                    maximum_raster_join_distance_squared)
                                    continue;
                                const auto improve = [&] (
                                    std::size_t source,
                                    std::size_t target
                                ) {
                                    if (
                                        distance < nearest_distance[source] ||
                                        (distance == nearest_distance[source] &&
                                            (nearest[source] == no_match ||
                                                vertices[target].view <
                                                    vertices[nearest[source]].view))
                                    ) {
                                        nearest[source] = target;
                                        nearest_distance[source] = distance;
                                    }
                                };
                                improve(left, right);
                                improve(right, left);
                            }
                        }
                    }
                }
                for (std::size_t left = 0; left < vertices.size(); ++left) {
                    const std::size_t right = nearest[left];
                    if (
                        right == no_match || right <= left ||
                        nearest[right] != left
                    )
                        continue;
                    const auto canonical =
                        occurrence_key(
                            *draw_list,
                            vertices[left].occurrence) <
                            occurrence_key(
                                *draw_list,
                                vertices[right].occurrence)
                        ? vertices[left].occurrence
                        : vertices[right].occurrence;
                    const auto canonical_vertex = draw_list->commands[
                        canonical.command].vertices[canonical.vertex];
                    const auto& canonical_command =
                        draw_list->commands[canonical.command];
                    ++stats.authored_raster_groups;
                    for (const std::size_t endpoint : {left, right}) {
                        raster_joined_positions[canonical_command.object_id]
                            .insert(vertices[endpoint].view);
                        const auto object_it = object_view_occurrences.find(
                            canonical_command.object_id);
                        if (object_it == object_view_occurrences.end())
                            continue;
                        const auto copies = object_it->second.find(
                            vertices[endpoint].view);
                        if (copies == object_it->second.end())
                            continue;
                        for (const auto& occurrence : copies->second) {
                            auto& command =
                                draw_list->commands[occurrence.command];
                            if (
                                command.model_pointer !=
                                    canonical_command.model_pointer ||
                                command.transform_id !=
                                    canonical_command.transform_id ||
                                command.ordering_table_index !=
                                    canonical_command.ordering_table_index
                            )
                                continue;
                            auto& vertex =
                                command.vertices[occurrence.vertex];
                            const bool changed =
                                vertex.screen_x != canonical_vertex.screen_x ||
                                vertex.screen_y != canonical_vertex.screen_y;
                            copy_projected_position(
                                &vertex,
                                canonical_vertex,
                                *draw_list);
                            if (changed)
                                ++stats.adjusted_authored_raster_instances;
                        }
                    }
                }
            }
            lod_finished = TopologyClock::now();

            // GT2 also joins some near/far road strips only in the original
            // integer SXY raster. The strips can belong to the same track
            // object and model while using different GTE transforms and
            // texture pages, so neither exact 3D topology nor a
            // same-material LOD key can identify the join. Continuous
            // projection then leaves a subpixel sliver even though the two
            // authored boundary edges occupy the same raster line.
            //
            // Close only the exact case the PS1 raster proves: opaque road
            // boundary edges in the same object/model/OT layer, exactly
            // collinear in authored SXY, with one authored interval wholly
            // contained by the other. Snap the contained edge orthogonally
            // to the longer projected edge only when both endpoints are at
            // most one eighth of a native pixel away. The rule contains no
            // output-resolution or aspect-ratio coordinates.
            using AuthoredOverlapEdges = std::pmr::unordered_map<
                AuthoredRasterLineKey,
                std::pmr::vector<AuthoredOverlapEdge>,
                AuthoredRasterLineKeyHash>;
            AuthoredOverlapEdges authored_overlap_edges{&topology_arena};
            authored_overlap_edges.reserve(
                stats.eligible_track_commands * 2U);
            for (const std::size_t command_index : eligible_commands) {
                const auto& command = draw_list->commands[command_index];
                if (!opaque_track_surface(*draw_list, command))
                    continue;
                const auto counts = object_edge_counts.find(
                    command.object_id);
                if (counts == object_edge_counts.end())
                    continue;
                for (int edge_index = 0; edge_index < 3; ++edge_index) {
                    const int next = (edge_index + 1) % 3;
                    const auto count = counts->second.find(Edge{
                        position(command.vertices[edge_index]),
                        position(command.vertices[next])});
                    if (
                        count == counts->second.end() ||
                        count->second != 1U
                    )
                        continue;
                    const auto& a = command.vertices[edge_index];
                    const auto& b = command.vertices[next];
                    std::int32_t dx =
                        b.authored_screen_x - a.authored_screen_x;
                    std::int32_t dy =
                        b.authored_screen_y - a.authored_screen_y;
                    const std::int32_t divisor = std::gcd(
                        static_cast<std::int32_t>(std::abs(dx)),
                        static_cast<std::int32_t>(std::abs(dy)));
                    if (divisor == 0)
                        continue;
                    dx /= divisor;
                    dy /= divisor;
                    if (dx < 0 || (dx == 0 && dy < 0)) {
                        dx = -dx;
                        dy = -dy;
                    }
                    const std::int64_t offset =
                        static_cast<std::int64_t>(dx) *
                            a.authored_screen_y -
                        static_cast<std::int64_t>(dy) *
                            a.authored_screen_x;
                    const auto coordinate = [dx, dy] (
                        const WorldDrawVertex& vertex
                    ) {
                        return
                            static_cast<std::int64_t>(dx) *
                                vertex.authored_screen_x +
                            static_cast<std::int64_t>(dy) *
                                vertex.authored_screen_y;
                    };
                    const std::int64_t coordinate_a = coordinate(a);
                    const std::int64_t coordinate_b = coordinate(b);
                    authored_overlap_edges[AuthoredRasterLineKey{
                        command.object_id,
                        command.model_pointer,
                        command.ordering_table_index,
                        dx,
                        dy,
                        offset,
                    }].push_back(AuthoredOverlapEdge{
                        command_index,
                        edge_index,
                        std::min(coordinate_a, coordinate_b),
                        std::max(coordinate_a, coordinate_b),
                    });
                }
            }
            struct AuthoredOverlapCandidate {
                AuthoredOverlapEdge inner;
                AuthoredOverlapEdge outer;
                std::int64_t outer_span;
                float maximum_distance_squared;
            };
            std::pmr::unordered_map<
                std::size_t,
                AuthoredOverlapCandidate> authored_overlap_candidates{
                    &topology_arena};
            constexpr float maximum_authored_overlap_distance = 0.125F;
            constexpr float maximum_authored_overlap_distance_squared =
                maximum_authored_overlap_distance *
                maximum_authored_overlap_distance;
            for (const auto& group : authored_overlap_edges) {
                const auto& edges = group.second;
                for (std::size_t left = 0; left < edges.size(); ++left) {
                    for (std::size_t right = left + 1;
                         right < edges.size();
                         ++right) {
                        if (edges[left].command == edges[right].command)
                            continue;
                        const std::int64_t left_span =
                            edges[left].maximum_coordinate -
                            edges[left].minimum_coordinate;
                        const std::int64_t right_span =
                            edges[right].maximum_coordinate -
                            edges[right].minimum_coordinate;
                        if (left_span == right_span)
                            continue;
                        const AuthoredOverlapEdge& outer =
                            left_span > right_span ? edges[left] : edges[right];
                        const AuthoredOverlapEdge& inner =
                            left_span > right_span ? edges[right] : edges[left];
                        if (
                            inner.minimum_coordinate <
                                outer.minimum_coordinate ||
                            inner.maximum_coordinate >
                                outer.maximum_coordinate
                        )
                            continue;
                        const auto& outer_command =
                            draw_list->commands[outer.command];
                        const auto& inner_command =
                            draw_list->commands[inner.command];
                        const auto& outer_a =
                            outer_command.vertices[outer.edge];
                        const auto& outer_b =
                            outer_command.vertices[(outer.edge + 1) % 3];
                        const float outer_dx =
                            outer_b.screen_x - outer_a.screen_x;
                        const float outer_dy =
                            outer_b.screen_y - outer_a.screen_y;
                        const float outer_length_squared =
                            outer_dx * outer_dx + outer_dy * outer_dy;
                        if (outer_length_squared < 0.000001F)
                            continue;
                        float maximum_distance_squared = 0.0F;
                        bool bounded = true;
                        for (const int vertex_index : {
                                 inner.edge, (inner.edge + 1) % 3}) {
                            const auto& vertex =
                                inner_command.vertices[vertex_index];
                            const float t =
                                ((vertex.screen_x - outer_a.screen_x) *
                                    outer_dx +
                                 (vertex.screen_y - outer_a.screen_y) *
                                    outer_dy) /
                                outer_length_squared;
                            if (t < 0.0F || t > 1.0F) {
                                bounded = false;
                                break;
                            }
                            const float target_x =
                                outer_a.screen_x + t * outer_dx;
                            const float target_y =
                                outer_a.screen_y + t * outer_dy;
                            const float distance_x =
                                vertex.screen_x - target_x;
                            const float distance_y =
                                vertex.screen_y - target_y;
                            maximum_distance_squared = std::max(
                                maximum_distance_squared,
                                distance_x * distance_x +
                                    distance_y * distance_y);
                        }
                        if (
                            !bounded ||
                            maximum_distance_squared >
                                maximum_authored_overlap_distance_squared
                        )
                            continue;
                        const std::size_t candidate_key =
                            inner.command * 3U +
                            static_cast<std::size_t>(inner.edge);
                        const AuthoredOverlapCandidate candidate{
                            inner,
                            outer,
                            std::max(left_span, right_span),
                            maximum_distance_squared,
                        };
                        const auto existing =
                            authored_overlap_candidates.find(candidate_key);
                        if (
                            existing == authored_overlap_candidates.end() ||
                            candidate.maximum_distance_squared <
                                existing->second.maximum_distance_squared ||
                            (candidate.maximum_distance_squared ==
                                    existing->second.maximum_distance_squared &&
                                candidate.outer_span >
                                    existing->second.outer_span)
                        )
                            authored_overlap_candidates[candidate_key] =
                                candidate;
                    }
                }
            }
            std::pmr::vector<AuthoredOverlapCandidate>
                ordered_authored_overlap_candidates{&topology_arena};
            ordered_authored_overlap_candidates.reserve(
                authored_overlap_candidates.size());
            for (const auto& candidate : authored_overlap_candidates)
                ordered_authored_overlap_candidates.push_back(
                    candidate.second);
            std::sort(
                ordered_authored_overlap_candidates.begin(),
                ordered_authored_overlap_candidates.end(),
                [] (const auto& left, const auto& right) {
                    if (left.outer_span != right.outer_span)
                        return left.outer_span > right.outer_span;
                    return std::tie(
                        left.outer.command,
                        left.outer.edge,
                        left.inner.command,
                        left.inner.edge) <
                        std::tie(
                            right.outer.command,
                            right.outer.edge,
                            right.inner.command,
                            right.inner.edge);
                });
            for (const auto& candidate :
                 ordered_authored_overlap_candidates) {
                const auto& outer_command =
                    draw_list->commands[candidate.outer.command];
                const auto& inner_command =
                    draw_list->commands[candidate.inner.command];
                const auto& outer_a =
                    outer_command.vertices[candidate.outer.edge];
                const auto& outer_b =
                    outer_command.vertices[(candidate.outer.edge + 1) % 3];
                const float outer_dx =
                    outer_b.screen_x - outer_a.screen_x;
                const float outer_dy =
                    outer_b.screen_y - outer_a.screen_y;
                const float outer_length_squared =
                    outer_dx * outer_dx + outer_dy * outer_dy;
                if (outer_length_squared < 0.000001F)
                    continue;
                ++stats.authored_overlap_seam_groups;
                for (const int vertex_index : {
                         candidate.inner.edge,
                         (candidate.inner.edge + 1) % 3}) {
                    const auto& seed = inner_command.vertices[vertex_index];
                    const float t =
                        ((seed.screen_x - outer_a.screen_x) * outer_dx +
                         (seed.screen_y - outer_a.screen_y) * outer_dy) /
                        outer_length_squared;
                    const float target_x =
                        outer_a.screen_x + t * outer_dx;
                    const float target_y =
                        outer_a.screen_y + t * outer_dy;
                    const Position seed_position = position(seed);
                    if (
                        inner_command.transform_id !=
                            outer_command.transform_id ||
                        inner_command.material_index !=
                            outer_command.material_index
                    ) {
                        raster_joined_positions[inner_command.object_id]
                            .insert(seed_position);
                    }
                    const auto object_it = object_view_occurrences.find(
                        inner_command.object_id);
                    if (object_it == object_view_occurrences.end())
                        continue;
                    const auto copies = object_it->second.find(seed_position);
                    if (copies == object_it->second.end())
                        continue;
                    for (const auto& occurrence : copies->second) {
                        auto& command =
                            draw_list->commands[occurrence.command];
                        if (
                            command.model_pointer !=
                                inner_command.model_pointer ||
                            command.transform_id !=
                                inner_command.transform_id ||
                            command.ordering_table_index !=
                                inner_command.ordering_table_index
                        )
                            continue;
                        auto& vertex = command.vertices[occurrence.vertex];
                        const bool changed =
                            vertex.screen_x != target_x ||
                            vertex.screen_y != target_y;
                        set_projected_position(
                            &vertex, target_x, target_y, *draw_list);
                        if (changed)
                            ++stats.adjusted_authored_overlap_instances;
                    }
                }
            }

            if (options.repair_projected_t_junctions) {
            // GT2's integer GTE transforms can place an authored intermediate
            // road vertex a fraction of a guest unit away from the neighboring
            // triangle's long edge. The 320x240 raster hides that T-junction,
            // while continuous projection exposes isolated background samples.
            // Same-surface joins require matching track provenance, a point no
            // more than one guest unit from the strict interior of a boundary
            // edge, and at most a quarter-pixel projected displacement. Road
            // LOD joins may cross transforms/materials only when the authored
            // raster and an exact power-of-two view-space scale prove them.
            struct ProjectedBoundaryEdge {
                std::size_t command;
                int edge;
                Position view_a;
                Position view_b;
                float screen_ax;
                float screen_ay;
                float screen_bx;
                float screen_by;
                std::uint32_t model_pointer;
                std::uint64_t transform_id;
                std::uint32_t material_index;
                std::int32_t ordering_table_index;
                std::uint32_t object_id;
                bool adjacent_copy;
            };
            std::pmr::unordered_map<
                std::uint32_t,
                std::pmr::vector<ProjectedBoundaryEdge>> projected_edges{
                    &topology_arena};
            projected_edges.reserve(object_edge_counts.size());
            for (const std::size_t command_index : eligible_commands) {
                const auto& command = draw_list->commands[command_index];
                const auto counts = object_edge_counts.find(
                    command.object_id);
                if (counts == object_edge_counts.end())
                    continue;
                for (int edge_index = 0; edge_index < 3; ++edge_index) {
                    const int next = (edge_index + 1) % 3;
                    const Position a = position(
                        command.vertices[edge_index]);
                    const Position b = position(command.vertices[next]);
                    const auto count = counts->second.find(Edge{a, b});
                    const bool boundary_edge =
                        count != counts->second.end() && count->second == 1;
                    if (!boundary_edge)
                        continue;
                    const bool has_previous_neighbor =
                        command.object_id > 0 &&
                        proven_adjacent_objects.find(adjacent_key(
                            command.object_id - 1U,
                            command.object_id)) !=
                            proven_adjacent_objects.end();
                    const bool has_next_neighbor =
                        proven_adjacent_objects.find(adjacent_key(
                            command.object_id,
                            command.object_id + 1U)) !=
                            proven_adjacent_objects.end();
                    const ProjectedBoundaryEdge edge{
                        command_index,
                        edge_index,
                        a,
                        b,
                        command.vertices[edge_index].screen_x,
                        command.vertices[edge_index].screen_y,
                        command.vertices[next].screen_x,
                        command.vertices[next].screen_y,
                        command.model_pointer,
                        command.transform_id,
                        command.material_index,
                        command.ordering_table_index,
                        command.object_id,
                        false,
                    };
                    if (boundary_edge)
                        projected_edges[command.object_id].push_back(edge);
                    if (has_previous_neighbor) {
                        auto adjacent_edge = edge;
                        adjacent_edge.adjacent_copy = true;
                        projected_edges[command.object_id - 1U].push_back(
                            adjacent_edge);
                    }
                    if (has_next_neighbor) {
                        auto adjacent_edge = edge;
                        adjacent_edge.adjacent_copy = true;
                        projected_edges[command.object_id + 1U].push_back(
                            adjacent_edge);
                    }
                    if (projected_pair_diagnostics && command_index == 870) {
                        std::fprintf(
                            stderr,
                            "[Topology-Projected-EdgeBuild] command=%zu "
                            "edge=%d object=%u boundary=%u prev=%u next=%u "
                            "screen=(%.3f,%.3f)-(%.3f,%.3f)\n",
                            command_index,
                            edge_index,
                            command.object_id,
                            boundary_edge ? 1U : 0U,
                            has_previous_neighbor ? 1U : 0U,
                            has_next_neighbor ? 1U : 0U,
                            edge.screen_ax,
                            edge.screen_ay - draw_list->display_y,
                            edge.screen_bx,
                            edge.screen_by - draw_list->display_y);
                    }
                }
            }
            // The proof below used to compare every object vertex with every
            // boundary edge in that object. Full-distance scenes can contain
            // thousands of each, making the topology pass quadratic. Index
            // visible edge bounding boxes into 8x8 native-pixel cells. Any
            // edge within the accepted quarter-pixel distance must intersect
            // the candidate's cell after the one-pixel expansion, so this
            // changes search cost without changing eligible joins.
            constexpr int projected_edge_cell_size = 8;
            using ProjectedEdgeBins = std::pmr::unordered_map<
                std::uint64_t,
                std::pmr::vector<std::size_t>>;
            std::pmr::unordered_map<std::uint32_t, ProjectedEdgeBins>
                projected_edge_bins{&topology_arena};
            projected_edge_bins.reserve(projected_edges.size());
            const auto projected_cell_key = [] (int x, int y) {
                return
                    (static_cast<std::uint64_t>(
                        static_cast<std::uint32_t>(x)) << 32) |
                    static_cast<std::uint32_t>(y);
            };
            const float visible_min_x =
                static_cast<float>(draw_list->display_x) - 1.0F;
            const float visible_min_y =
                static_cast<float>(draw_list->display_y) - 1.0F;
            const float visible_max_x =
                static_cast<float>(
                    draw_list->display_x + draw_list->display_width) + 1.0F;
            const float visible_max_y =
                static_cast<float>(
                    draw_list->display_y + draw_list->display_height) + 1.0F;
            const float projected_edge_bin_margin =
                options.repair_offscreen_projected_t_junctions
                    ? 192.0F
                    : 1.0F;
            const float binned_min_x =
                static_cast<float>(draw_list->display_x) -
                projected_edge_bin_margin;
            const float binned_min_y =
                static_cast<float>(draw_list->display_y) -
                projected_edge_bin_margin;
            const float binned_max_x =
                static_cast<float>(
                    draw_list->display_x + draw_list->display_width) +
                projected_edge_bin_margin;
            const float binned_max_y =
                static_cast<float>(
                    draw_list->display_y + draw_list->display_height) +
                projected_edge_bin_margin;
            for (const auto& object : projected_edges) {
                auto& bins = projected_edge_bins[object.first];
                bins.reserve(object.second.size() * 2);
                for (std::size_t edge_index = 0;
                     edge_index < object.second.size();
                     ++edge_index) {
                    const auto& edge = object.second[edge_index];
                    float minimum_x = (std::max)(
                        binned_min_x,
                        (std::min)(edge.screen_ax, edge.screen_bx) - 1.0F);
                    float maximum_x = (std::min)(
                        binned_max_x,
                        (std::max)(edge.screen_ax, edge.screen_bx) + 1.0F);
                    float minimum_y = (std::max)(
                        binned_min_y,
                        (std::min)(edge.screen_ay, edge.screen_by) - 1.0F);
                    float maximum_y = (std::min)(
                        binned_max_y,
                        (std::max)(edge.screen_ay, edge.screen_by) + 1.0F);
                    if (minimum_x > maximum_x || minimum_y > maximum_y)
                        continue;
                    const int cell_x0 = static_cast<int>(std::floor(
                        minimum_x / projected_edge_cell_size));
                    const int cell_x1 = static_cast<int>(std::floor(
                        maximum_x / projected_edge_cell_size));
                    const int cell_y0 = static_cast<int>(std::floor(
                        minimum_y / projected_edge_cell_size));
                    const int cell_y1 = static_cast<int>(std::floor(
                        maximum_y / projected_edge_cell_size));
                    for (int cell_y = cell_y0; cell_y <= cell_y1; ++cell_y)
                        for (int cell_x = cell_x0;
                             cell_x <= cell_x1;
                            ++cell_x)
                            bins[projected_cell_key(cell_x, cell_y)]
                                .push_back(edge_index);
                    projected_edge_reference_count +=
                        static_cast<std::uint64_t>(
                            (cell_x1 - cell_x0 + 1) *
                            (cell_y1 - cell_y0 + 1));
                }
            }
            constexpr double maximum_view_distance_squared = 1.0;
            constexpr double maximum_authored_distance_squared =
                1.0 * 1.0;
            constexpr double maximum_screen_distance_squared =
                0.25 * 0.25;
            constexpr double maximum_raster_lod_screen_distance_squared =
                0.5 * 0.5;
            constexpr double maximum_adjacent_screen_distance_squared =
                0.75 * 0.75;
            // The edge must be an exact topology boundary. The candidate
            // vertex may also participate in interior/degenerate connector
            // triangles, as GT2 uses those to bridge road LOD strips, so
            // consider every proven vertex occurrence in the same object.
            for (const auto& object : object_view_occurrences) {
                const auto edges_it = projected_edges.find(object.first);
                const auto bins_it = projected_edge_bins.find(object.first);
                if (
                    edges_it == projected_edges.end() ||
                    bins_it == projected_edge_bins.end()
                )
                    continue;
                const auto joined_it =
                    raster_joined_positions.find(object.first);
                const auto boundary_it =
                    object_boundary_positions.find(object.first);
                for (const auto& point_entry : object.second) {
                    const Position& point = point_entry.first;
                    const auto& point_occurrences = point_entry.second;
                    if (point_occurrences.empty())
                        continue;
                    if (
                        joined_it != raster_joined_positions.end() &&
                        joined_it->second.find(point) !=
                            joined_it->second.end()
                    )
                        continue;
                    const Occurrence representative =
                        point_occurrences.front();
                    const auto& representative_command =
                        draw_list->commands[representative.command];
                    const auto& representative_vertex =
                        representative_command.vertices[representative.vertex];
                    const bool candidate_is_visible =
                        representative_vertex.screen_x >= visible_min_x &&
                        representative_vertex.screen_x <= visible_max_x &&
                        representative_vertex.screen_y >= visible_min_y &&
                        representative_vertex.screen_y <= visible_max_y;
                    const int candidate_cell_x = static_cast<int>(std::floor(
                        representative_vertex.screen_x /
                        projected_edge_cell_size));
                    const int candidate_cell_y = static_cast<int>(std::floor(
                        representative_vertex.screen_y /
                        projected_edge_cell_size));
                    const auto candidates = bins_it->second.find(
                        projected_cell_key(
                            candidate_cell_x, candidate_cell_y));
                    if (candidates == bins_it->second.end())
                        continue;
                    ++projected_candidate_points;
                    if (candidate_is_visible)
                        ++projected_visible_candidate_points;
                    else
                        ++projected_offscreen_candidate_points;
                    const ProjectedBoundaryEdge* best = nullptr;
                    double best_screen_distance =
                        maximum_adjacent_screen_distance_squared;
                    float best_x = representative_vertex.screen_x;
                    float best_y = representative_vertex.screen_y;
                    bool best_raster_lod_join = false;
                    const bool candidate_is_boundary =
                        boundary_it != object_boundary_positions.end() &&
                        boundary_it->second.find(point) !=
                            boundary_it->second.end();
                    const std::size_t candidate_count =
                        candidates->second.size();
                    projected_candidate_edge_tests += candidate_count;
                    if (
                        projected_pair_diagnostics &&
                        representative.command == 942
                    ) {
                            std::fprintf(
                                stderr,
                                "[Topology-Projected-Rep] rep=%zu object=%u "
                            "visible=%u cell=%d,%d candidates=%zu\n",
                            representative.command,
                            representative_command.object_id,
                            candidate_is_visible ? 1U : 0U,
                            candidate_cell_x,
                            candidate_cell_y,
                            candidate_count);
                        const std::size_t print_count =
                            std::min<std::size_t>(candidate_count, 24);
                        for (std::size_t printed = 0;
                             printed < print_count;
                            ++printed) {
                            const std::size_t edge_index =
                                candidates->second[printed];
                            const auto& debug_edge =
                                edges_it->second[edge_index];
                            std::fprintf(
                                stderr,
                                "  edge[%zu] command=%zu object=%u "
                                "screen=(%.3f,%.3f)-(%.3f,%.3f)\n",
                                printed,
                                debug_edge.command,
                                debug_edge.object_id,
                                debug_edge.screen_ax,
                                debug_edge.screen_ay - draw_list->display_y,
                                debug_edge.screen_bx,
                                debug_edge.screen_by - draw_list->display_y);
                        }
                    }
                    for (std::size_t candidate_index = 0;
                         candidate_index < candidate_count;
                         ++candidate_index) {
                        const std::size_t edge_index =
                            candidates->second[candidate_index];
                        const auto& edge = edges_it->second[edge_index];
                        if (!candidate_is_visible && edge.adjacent_copy)
                            continue;
                        const bool diagnose_pair =
                            projected_pair_diagnostics &&
                            representative.command == 942 &&
                            edge.command == 870;
                        if (
                            edge.command == representative.command ||
                            point == edge.view_a || point == edge.view_b
                        )
                            continue;
                        const auto& edge_command =
                            draw_list->commands[edge.command];
                        const bool same_surface =
                            edge.object_id == representative_command.object_id &&
                            edge.model_pointer ==
                                representative_command.model_pointer &&
                            edge.transform_id ==
                                representative_command.transform_id &&
                            edge.material_index ==
                                representative_command.material_index &&
                            edge.ordering_table_index ==
                                representative_command.ordering_table_index;
                        const bool same_lod_layer =
                            edge.object_id ==
                                representative_command.object_id &&
                            edge.model_pointer ==
                                representative_command.model_pointer &&
                            edge.ordering_table_index ==
                                representative_command.ordering_table_index &&
                            opaque_track_surface(
                                *draw_list, edge_command) &&
                            opaque_track_surface(
                                *draw_list, representative_command);
                        bool same_render_layer =
                            edge.ordering_table_index ==
                                representative_command.ordering_table_index;
                        if (
                            same_render_layer &&
                            edge.material_index < draw_list->materials.size() &&
                            representative_command.material_index <
                                draw_list->materials.size()
                        ) {
                            same_render_layer =
                                draw_list->materials[
                                    edge.material_index].primitive_flags ==
                                draw_list->materials[
                                    representative_command
                                        .material_index].primitive_flags;
                        } else if (same_render_layer) {
                            same_render_layer =
                                edge.material_index ==
                                representative_command.material_index;
                        }
                        const bool adjacent_surface =
                            edge.object_id != representative_command.object_id &&
                            same_render_layer &&
                            proven_adjacent_objects.find(adjacent_key(
                                edge.object_id,
                                representative_command.object_id)) !=
                                proven_adjacent_objects.end();
                        if (diagnose_pair) {
                            std::fprintf(
                                stderr,
                                "[Topology-Projected-Pair] rep=%zu edge=%zu "
                                "sameSurface=%u sameLayer=%u adjacent=%u "
                                "edgeObj=%u repObj=%u edgeMat=%u repMat=%u "
                                "edgeOt=%d repOt=%d candidateBoundary=%u\n",
                                representative.command,
                                edge.command,
                                same_surface ? 1U : 0U,
                                same_render_layer ? 1U : 0U,
                                adjacent_surface ? 1U : 0U,
                                edge.object_id,
                                representative_command.object_id,
                                edge.material_index,
                                representative_command.material_index,
                                edge.ordering_table_index,
                                representative_command.ordering_table_index,
                                candidate_is_boundary ? 1U : 0U);
                        }
                        if (
                            !same_surface &&
                            !same_lod_layer &&
                            !adjacent_surface
                        )
                            continue;
                        const double dx =
                            static_cast<double>(edge.view_b.x) - edge.view_a.x;
                        const double dy =
                            static_cast<double>(edge.view_b.y) - edge.view_a.y;
                        const double dz =
                            static_cast<double>(edge.view_b.z) - edge.view_a.z;
                        const double length_squared =
                            dx * dx + dy * dy + dz * dz;
                        if (length_squared <= 0.0)
                            continue;
                        const double px =
                            static_cast<double>(point.x) - edge.view_a.x;
                        const double py =
                            static_cast<double>(point.y) - edge.view_a.y;
                        const double pz =
                            static_cast<double>(point.z) - edge.view_a.z;
                        const double view_t =
                            (px * dx + py * dy + pz * dz) /
                            length_squared;
                        const double view_error_x = px - view_t * dx;
                        const double view_error_y = py - view_t * dy;
                        const double view_error_z = pz - view_t * dz;
                        const double view_distance_squared =
                            view_error_x * view_error_x +
                            view_error_y * view_error_y +
                            view_error_z * view_error_z;
                        // Most joins are proven in exact integer GTE view
                        // space. GT2's road LOD strips are the exception: two
                        // transforms/materials can submit the same boundary at
                        // different power-of-two fixed-point scales. In that
                        // case the authored integer SXY raster must also place
                        // the candidate within one native pixel of the edge.
                        const bool view_space_join =
                            view_t > 0.0 && view_t < 1.0 &&
                            view_distance_squared <=
                                maximum_view_distance_squared;
                        if (
                            !adjacent_surface &&
                            view_space_join &&
                            !candidate_is_boundary
                        )
                            continue;
                        bool raster_lod_join = false;
                        if (
                            !adjacent_surface &&
                            (!view_space_join || !same_surface)
                        ) {
                            const auto& authored_a =
                                edge_command.vertices[edge.edge];
                            const auto& authored_b =
                                edge_command.vertices[(edge.edge + 1) % 3];
                            const double authored_dx =
                                static_cast<double>(
                                    authored_b.authored_screen_x) -
                                authored_a.authored_screen_x;
                            const double authored_dy =
                                static_cast<double>(
                                    authored_b.authored_screen_y) -
                                authored_a.authored_screen_y;
                            const double authored_length_squared =
                                authored_dx * authored_dx +
                                authored_dy * authored_dy;
                            if (authored_length_squared <= 0.0)
                                continue;
                            const double authored_px =
                                static_cast<double>(
                                    representative_vertex.authored_screen_x) -
                                authored_a.authored_screen_x;
                            const double authored_py =
                                static_cast<double>(
                                    representative_vertex.authored_screen_y) -
                                authored_a.authored_screen_y;
                            const double authored_t =
                                (authored_px * authored_dx +
                                    authored_py * authored_dy) /
                                authored_length_squared;
                            if (authored_t < 0.0 || authored_t > 1.0)
                                continue;
                            const double authored_error_x =
                                authored_px - authored_t * authored_dx;
                            const double authored_error_y =
                                authored_py - authored_t * authored_dy;
                            const double authored_distance_squared =
                                authored_error_x * authored_error_x +
                                authored_error_y * authored_error_y;
                            if (authored_distance_squared >
                                maximum_authored_distance_squared)
                                continue;
                            // Interior connector vertices are accepted only
                            // when their exact view position is a proven
                            // power-of-two LOD-scale copy of the corresponding
                            // point on the boundary edge.
                            bool scale_proven = false;
                            constexpr std::array<int, 4> scales{
                                2, 4, 8, 16};
                            for (const int scale : scales) {
                                const auto near_edge = [&] (
                                    double candidate_x,
                                    double candidate_y,
                                    double candidate_z
                                ) {
                                    const double candidate_px =
                                        candidate_x - edge.view_a.x;
                                    const double candidate_py =
                                        candidate_y - edge.view_a.y;
                                    const double candidate_pz =
                                        candidate_z - edge.view_a.z;
                                    const double candidate_t =
                                        (candidate_px * dx +
                                            candidate_py * dy +
                                            candidate_pz * dz) /
                                        length_squared;
                                    if (candidate_t <= 0.0 ||
                                        candidate_t >= 1.0)
                                        return false;
                                    const double error_x = candidate_px -
                                        candidate_t * dx;
                                    const double error_y = candidate_py -
                                        candidate_t * dy;
                                    const double error_z = candidate_pz -
                                        candidate_t * dz;
                                    return error_x * error_x +
                                        error_y * error_y +
                                        error_z * error_z <= 4.0;
                                };
                                if (
                                    near_edge(
                                        static_cast<double>(point.x) / scale,
                                        static_cast<double>(point.y) / scale,
                                        static_cast<double>(point.z) / scale) ||
                                    near_edge(
                                        static_cast<double>(point.x) * scale,
                                        static_cast<double>(point.y) * scale,
                                        static_cast<double>(point.z) * scale)
                                ) {
                                    scale_proven = true;
                                    break;
                                }
                            }
                            if (!scale_proven)
                                continue;
                            raster_lod_join = true;
                        }
                        const double screen_dx =
                            edge.screen_bx - edge.screen_ax;
                        const double screen_dy =
                            edge.screen_by - edge.screen_ay;
                        const double screen_length_squared =
                            screen_dx * screen_dx + screen_dy * screen_dy;
                        if (screen_length_squared <= 0.0)
                            continue;
                        const double screen_px =
                            representative_vertex.screen_x - edge.screen_ax;
                        const double screen_py =
                            representative_vertex.screen_y - edge.screen_ay;
                        const double screen_t =
                            (screen_px * screen_dx + screen_py * screen_dy) /
                            screen_length_squared;
                        if (screen_t <= 0.0 || screen_t >= 1.0)
                            continue;
                        const float target_x = static_cast<float>(
                            edge.screen_ax + screen_t * screen_dx);
                        const float target_y = static_cast<float>(
                            edge.screen_ay + screen_t * screen_dy);
                        const double screen_error_x =
                            representative_vertex.screen_x - target_x;
                        const double screen_error_y =
                            representative_vertex.screen_y - target_y;
                        const double screen_distance_squared =
                            screen_error_x * screen_error_x +
                            screen_error_y * screen_error_y;
                        const double maximum_allowed_screen_distance_squared =
                            adjacent_surface
                                ? maximum_adjacent_screen_distance_squared
                                : raster_lod_join
                                    ? maximum_raster_lod_screen_distance_squared
                                    : maximum_screen_distance_squared;
                        if (
                            screen_distance_squared >
                                maximum_allowed_screen_distance_squared ||
                            (best != nullptr &&
                                screen_distance_squared >=
                                    best_screen_distance)
                        )
                            continue;
                        best = &edge;
                        best_screen_distance = screen_distance_squared;
                        best_x = target_x;
                        best_y = target_y;
                        best_raster_lod_join = raster_lod_join;
                    }
                    if (best == nullptr)
                        continue;
                    bool adjusted = false;
                    for (const Occurrence& occurrence : point_occurrences) {
                        auto& command =
                            draw_list->commands[occurrence.command];
                        auto& vertex = command.vertices[occurrence.vertex];
                        // A GT packet can mix a locally transformed vertex
                        // with vertices that are already in view space.  The
                        // command-level transform then describes only one
                        // vertex and is not a valid weld boundary.  Exact
                        // view-space copies under the same projection must
                        // receive the same T-junction correction or the
                        // modern subpixel raster opens a dotted seam between
                        // otherwise adjacent triangles.
                        const bool shared_exact_projection =
                            command.object_id ==
                                representative_command.object_id &&
                            command.model_pointer ==
                                representative_command.model_pointer &&
                            command.ordering_table_index ==
                                representative_command
                                    .ordering_table_index &&
                            vertex.projection_plane ==
                                representative_vertex.projection_plane &&
                            vertex.projection_offset_x ==
                                representative_vertex.projection_offset_x &&
                            vertex.projection_offset_y ==
                                representative_vertex.projection_offset_y &&
                            vertex.draw_offset_x ==
                                representative_vertex.draw_offset_x &&
                            vertex.draw_offset_y ==
                                representative_vertex.draw_offset_y;
                        if (
                            command.ordering_table_index !=
                                representative_command
                                    .ordering_table_index ||
                            (
                                !shared_exact_projection &&
                                (
                                    command.model_pointer !=
                                        best->model_pointer ||
                                    command.transform_id !=
                                        best->transform_id ||
                                    command.ordering_table_index !=
                                        best->ordering_table_index
                                ) &&
                                !(
                                    best_raster_lod_join &&
                                    command.object_id ==
                                        representative_command.object_id &&
                                    command.model_pointer ==
                                        representative_command.model_pointer &&
                                    command.transform_id ==
                                        representative_command.transform_id
                                ) &&
                                !(
                                    command.object_id ==
                                        representative_command.object_id &&
                                    proven_adjacent_objects.find(adjacent_key(
                                        best->object_id,
                                        command.object_id)) !=
                                        proven_adjacent_objects.end()
                                )
                            )
                        )
                            continue;
                        if (
                            vertex.screen_x == best_x &&
                            vertex.screen_y == best_y
                        )
                            continue;
                        set_projected_position(
                            &vertex, best_x, best_y, *draw_list);
                        ++stats.adjusted_projected_t_junction_instances;
                        adjusted = true;
                    }
                    if (adjusted)
                        ++stats.projected_t_junctions;
                }
            }
            }
            projected_finished = TopologyClock::now();
        } else {
            seams_finished = setup_finished;
            raster_finished = setup_finished;
            lod_finished = setup_finished;
            projected_finished = setup_finished;
        }

        struct EdgeOccurrenceBucket {
            EdgeOccurrence first{};
            std::uint32_t count = 0;
        };
        std::pmr::unordered_map<Edge, EdgeOccurrenceBucket, EdgeHash> edges{
            &topology_arena};
        edges.reserve(
            static_cast<std::size_t>(stats.eligible_track_commands) * 3U);
        for (const std::size_t command_index : eligible_commands) {
            const auto& command = draw_list->commands[command_index];
            for (int edge_index = 0; edge_index < 3; ++edge_index) {
                const Edge edge{
                    position(command.vertices[edge_index]),
                    position(command.vertices[(edge_index + 1) % 3]),
                };
                auto [entry, inserted] = edges.try_emplace(edge);
                if (inserted)
                    entry->second.first =
                        EdgeOccurrence{command_index, edge_index};
                ++entry->second.count;
            }
        }
        std::pmr::vector<std::pair<Edge, EdgeOccurrence>> boundaries{
            &topology_arena};
        boundaries.reserve(edges.size());
        for (const auto& entry : edges) {
            if (entry.second.count == 1) {
                ++stats.boundary_edges;
                boundaries.emplace_back(entry.first, entry.second.first);
            } else if (entry.second.count == 2) {
                ++stats.manifold_edges;
            } else {
                ++stats.nonmanifold_edges;
            }
        }

        std::array<std::pmr::vector<Position>, 3> sorted_boundary_points{
            std::pmr::vector<Position>{&topology_arena},
            std::pmr::vector<Position>{&topology_arena},
            std::pmr::vector<Position>{&topology_arena},
        };
        {
            std::pmr::unordered_set<Position, PositionHash> unique{
                &topology_arena};
            unique.reserve(boundaries.size() * 2);
            for (const auto& boundary : boundaries) {
                unique.insert(boundary.first.a);
                unique.insert(boundary.first.b);
            }
            for (int axis = 0; axis < 3; ++axis) {
                sorted_boundary_points[axis].assign(
                    unique.begin(), unique.end());
                std::sort(
                    sorted_boundary_points[axis].begin(),
                    sorted_boundary_points[axis].end(),
                    [axis](const Position& left, const Position& right) {
                        const auto left_key = std::make_tuple(
                            coordinate(left, axis), left.x, left.y, left.z);
                        const auto right_key = std::make_tuple(
                            coordinate(right, axis), right.x, right.y, right.z);
                        return left_key < right_key;
                    });
            }
        }

        using EdgeSplits = std::array<std::vector<Position>, 3>;
        std::pmr::unordered_map<std::size_t, EdgeSplits> command_splits{
            &topology_arena};
        command_splits.reserve(boundaries.size());
        if (options.split_exact_t_junctions) {
            for (const auto& boundary : boundaries) {
                const Position& a = boundary.first.a;
                const Position& b = boundary.first.b;
                const int axis = dominant_axis(a, b);
                const auto& candidates = sorted_boundary_points[axis];
                const std::int32_t minimum =
                    std::min(coordinate(a, axis), coordinate(b, axis));
                const std::int32_t maximum =
                    std::max(coordinate(a, axis), coordinate(b, axis));
                auto begin = std::lower_bound(
                    candidates.begin(), candidates.end(), minimum,
                    [axis](const Position& point, std::int32_t value) {
                        return coordinate(point, axis) < value;
                    });
                for (auto candidate = begin;
                     candidate != candidates.end() &&
                     coordinate(*candidate, axis) <= maximum;
                     ++candidate) {
                    if (!strictly_on_segment(*candidate, a, b))
                        continue;
                    auto& splits =
                        command_splits[boundary.second.command]
                            [boundary.second.edge];
                    if (std::find(
                            splits.begin(), splits.end(), *candidate) ==
                        splits.end()) {
                        splits.push_back(*candidate);
                        ++stats.exact_t_junctions;
                    }
                }
            }
        }

        if (!command_splits.empty()) {
            std::vector<WorldDrawCommand> rebuilt;
            rebuilt.reserve(
                draw_list->commands.size() + stats.exact_t_junctions);
            for (std::size_t command_index = 0;
                 command_index < draw_list->commands.size();
                 ++command_index) {
                const auto found = command_splits.find(command_index);
                if (found == command_splits.end()) {
                    rebuilt.push_back(draw_list->commands[command_index]);
                    continue;
                }
                const auto& source = draw_list->commands[command_index];
                auto& split_edges = found->second;
                std::size_t polygon_capacity = 3;
                for (const auto& edge_splits : split_edges)
                    polygon_capacity += edge_splits.size();
                std::vector<WorldDrawVertex> polygon;
                polygon.reserve(polygon_capacity);
                for (int edge_index = 0; edge_index < 3; ++edge_index) {
                    const auto& a = source.vertices[edge_index];
                    const auto& b =
                        source.vertices[(edge_index + 1) % 3];
                    polygon.push_back(a);
                    auto& splits = split_edges[edge_index];
                    const Position pa = position(a);
                    const Position pb = position(b);
                    const int axis = dominant_axis(pa, pb);
                    const bool ascending =
                        coordinate(pa, axis) < coordinate(pb, axis);
                    std::sort(
                        splits.begin(), splits.end(),
                        [axis, ascending](
                            const Position& left,
                            const Position& right) {
                            return ascending
                                ? coordinate(left, axis) <
                                    coordinate(right, axis)
                                : coordinate(left, axis) >
                                    coordinate(right, axis);
                        });
                    for (const auto& split : splits) {
                        const auto canonical =
                            canonical_occurrences.find(split);
                        if (canonical == canonical_occurrences.end())
                            continue;
                        const auto occurrence = canonical->second;
                        const auto& canonical_vertex =
                            draw_list->commands[occurrence.command]
                                .vertices[occurrence.vertex];
                        polygon.push_back(edge_vertex(
                            a, b, canonical_vertex, split));
                    }
                }
                const auto triangles =
                    triangulate_polygon(polygon, source);
                if (triangles.empty()) {
                    rebuilt.push_back(source);
                    continue;
                }
                ++stats.split_source_triangles;
                stats.emitted_split_triangles +=
                    static_cast<std::uint32_t>(triangles.size());
                for (const auto& indices : triangles) {
                    WorldDrawCommand command = source;
                    for (int index = 0; index < 3; ++index)
                        command.vertices[index] = polygon[indices[index]];
                    recalculate_normal(&command);
                    rebuilt.push_back(command);
                }
            }
            draw_list->commands = std::move(rebuilt);
        }
        exact_finished = TopologyClock::now();

        if (options.deterministic_coplanar_ownership) {
            std::pmr::unordered_map<
                Plane,
                std::pmr::vector<std::size_t>,
                PlaneHash> planes{&topology_arena};
            planes.reserve(draw_list->commands.size());
            for (std::size_t index = 0;
                 index < draw_list->commands.size();
                 ++index) {
                const auto& command = draw_list->commands[index];
                if (!eligible(*draw_list, command))
                    continue;
                const auto& material =
                    draw_list->materials[command.material_index];
                if ((material.primitive_flags & (1U << 1)) != 0)
                    continue;
                Plane key{};
                if (plane(command, &key))
                    planes[key].push_back(index);
            }
            DisjointSet sets(
                draw_list->commands.size(), &topology_arena);
            std::pmr::unordered_set<std::size_t> ownership_members{
                &topology_arena};
            ownership_members.reserve(draw_list->commands.size() / 4);
            for (const auto& plane_group : planes) {
                const auto& indices = plane_group.second;
                struct ProjectedBounds {
                    std::size_t index;
                    std::int64_t minimum_x;
                    std::int64_t maximum_x;
                    std::int64_t minimum_y;
                    std::int64_t maximum_y;
                };
                const int dropped =
                    absolute(plane_group.first.x) >=
                        absolute(plane_group.first.y) &&
                    absolute(plane_group.first.x) >=
                        absolute(plane_group.first.z)
                        ? 0
                        : absolute(plane_group.first.y) >=
                            absolute(plane_group.first.z) ? 1 : 2;
                std::pmr::vector<ProjectedBounds> bounds{&topology_arena};
                bounds.reserve(indices.size());
                for (const std::size_t index : indices) {
                    const auto& command = draw_list->commands[index];
                    const Point2 first = project(
                        position(command.vertices[0]), dropped);
                    ProjectedBounds item{
                        index, first.x, first.x, first.y, first.y};
                    for (int vertex_index = 1;
                         vertex_index < 3;
                         ++vertex_index) {
                        const Point2 point = project(
                            position(command.vertices[vertex_index]), dropped);
                        item.minimum_x = (std::min)(item.minimum_x, point.x);
                        item.maximum_x = (std::max)(item.maximum_x, point.x);
                        item.minimum_y = (std::min)(item.minimum_y, point.y);
                        item.maximum_y = (std::max)(item.maximum_y, point.y);
                    }
                    bounds.push_back(item);
                }
                std::sort(
                    bounds.begin(), bounds.end(),
                    [](const ProjectedBounds& left,
                       const ProjectedBounds& right) {
                        return std::tie(left.minimum_x, left.index) <
                            std::tie(right.minimum_x, right.index);
                    });
                for (std::size_t i = 0; i < bounds.size(); ++i) {
                    for (std::size_t j = i + 1;
                         j < bounds.size();
                         ++j) {
                        if (bounds[j].minimum_x > bounds[i].maximum_x)
                            break;
                        if (
                            bounds[j].minimum_y > bounds[i].maximum_y ||
                            bounds[i].minimum_y > bounds[j].maximum_y
                        )
                            continue;
                        const auto& left =
                            draw_list->commands[bounds[i].index];
                        const auto& right =
                            draw_list->commands[bounds[j].index];
                        if (!positive_overlap(
                                left, right, plane_group.first))
                            continue;
                        ++stats.coplanar_overlap_pairs;
                        if (same_geometry(left, right))
                            ++stats.exact_duplicate_pairs;
                        if (!same_material(left, right)) {
                            ++stats.material_overlap_pairs;
                            continue;
                        }
                        ownership_members.insert(bounds[i].index);
                        ownership_members.insert(bounds[j].index);
                        sets.join(bounds[i].index, bounds[j].index);
                    }
                }
            }
            std::pmr::unordered_map<
                std::size_t,
                std::pmr::vector<std::size_t>> components{&topology_arena};
            components.reserve(draw_list->commands.size());
            for (const std::size_t index : ownership_members)
                components[sets.find(index)].push_back(index);
            for (auto& entry : components) {
                auto& slots = entry.second;
                if (slots.size() < 2)
                    continue;
                ++stats.ownership_components;
                std::sort(slots.begin(), slots.end());
                std::pmr::vector<WorldDrawCommand> ordered{&topology_arena};
                ordered.reserve(slots.size());
                for (const auto slot : slots)
                    ordered.push_back(draw_list->commands[slot]);
                std::stable_sort(
                    ordered.begin(), ordered.end(),
                    [](const WorldDrawCommand& left,
                       const WorldDrawCommand& right) {
                        return ownership_key(left) < ownership_key(right);
                    });
                for (std::size_t index = 0; index < slots.size(); ++index) {
                    if (
                        draw_list->commands[slots[index]]
                            .source_command_index !=
                        ordered[index].source_command_index
                    )
                        ++stats.ownership_reorders;
                    draw_list->commands[slots[index]] =
                        std::move(ordered[index]);
                }
            }
        }
        ownership_finished = TopologyClock::now();

        stats.output_commands =
            static_cast<std::uint32_t>(draw_list->commands.size());
        draw_list->track_commands = 0;
        draw_list->vehicle_commands = 0;
        draw_list->background_commands = 0;
        draw_list->unclassified_commands = 0;
        draw_list->unclassified_world_commands = 0;
        for (const auto& command : draw_list->commands) {
            if (command.object_kind == 1)
                ++draw_list->track_commands;
            else if (command.object_kind == 2)
                ++draw_list->vehicle_commands;
            else if (command.object_kind == 3)
                ++draw_list->background_commands;
            else {
                ++draw_list->unclassified_commands;
                if (
                    command.material_index < draw_list->materials.size() &&
                    (draw_list->materials[command.material_index]
                         .primitive_flags &
                        world_primitive_screen_space_flag) == 0
                ) {
                    ++draw_list->unclassified_world_commands;
                }
            }
        }
        if (std::getenv("OPENGT_TOPOLOGY_PHASE_DIAGNOSTICS") != nullptr) {
            const auto milliseconds = [] (TopologyClock::duration duration) {
                return std::chrono::duration<double, std::milli>(
                    duration).count();
            };
            std::fprintf(
                stderr,
                "[Topology-Phases] totalMs=%.3f setupMs=%.3f "
                "seamsMs=%.3f rasterMs=%.3f lodMs=%.3f "
                "projectedMs=%.3f exactMs=%.3f ownershipMs=%.3f "
                "commands=%u eligible=%u\n",
                milliseconds(ownership_finished - topology_started),
                milliseconds(setup_finished - topology_started),
                milliseconds(seams_finished - setup_finished),
                milliseconds(raster_finished - seams_finished),
                milliseconds(lod_finished - raster_finished),
                milliseconds(projected_finished - lod_finished),
                milliseconds(exact_finished - projected_finished),
                milliseconds(ownership_finished - exact_finished),
                stats.input_commands,
                stats.eligible_track_commands);
            std::fprintf(
                stderr,
                "[Topology-Projected-Counters] edges=%u edgeRefs=%llu "
                "candidatePoints=%llu visible=%llu offscreen=%llu "
                "candidateEdges=%llu projectedTJ=%u/%u\n",
                stats.boundary_edges,
                static_cast<unsigned long long>(
                    projected_edge_reference_count),
                static_cast<unsigned long long>(
                    projected_candidate_points),
                static_cast<unsigned long long>(
                    projected_visible_candidate_points),
                static_cast<unsigned long long>(
                    projected_offscreen_candidate_points),
                static_cast<unsigned long long>(
                    projected_candidate_edge_tests),
                stats.projected_t_junctions,
                stats.adjusted_projected_t_junction_instances);
        }
        *output_stats = stats;
        return WorldTopologyResult::success;
    } catch (const std::bad_alloc&) {
        return WorldTopologyResult::allocation_failed;
    }
}

const char* world_topology_result_name(
    WorldTopologyResult result
) noexcept {
    switch (result) {
        case WorldTopologyResult::success: return "success";
        case WorldTopologyResult::invalid_argument:
            return "invalid_argument";
        case WorldTopologyResult::allocation_failed:
            return "allocation_failed";
    }
    return "unknown";
}

} // namespace opengt::render
