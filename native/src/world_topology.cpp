#include "opengt/world_topology.hpp"

#include <algorithm>
#include <array>
#include <cmath>
#include <cstdint>
#include <map>
#include <numeric>
#include <set>
#include <tuple>
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

struct Edge {
    Position a;
    Position b;

    Edge(Position left, Position right)
        : a(std::min(left, right)), b(std::max(left, right)) {}

    bool operator<(const Edge& other) const {
        return std::tie(a, b) < std::tie(other.a, other.b);
    }
};

struct Occurrence {
    std::size_t command;
    int vertex;
};

struct EdgeOccurrence {
    std::size_t command;
    int edge;
};

struct Plane {
    std::int64_t x;
    std::int64_t y;
    std::int64_t z;
    std::int64_t d;

    bool operator<(const Plane& other) const {
        return std::tie(x, y, z, d) <
            std::tie(other.x, other.y, other.z, other.d);
    }
};

struct Point2 {
    std::int64_t x;
    std::int64_t y;
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

bool eligible(const WorldDrawCommand& command) {
    if (
        command.object_kind != 1 ||
        command.channel != WorldViewChannel::main_view
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
    std::vector<std::size_t> parent;

    explicit DisjointSet(std::size_t count) : parent(count) {
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
        WorldTopologyStats stats{};
        stats.input_commands =
            static_cast<std::uint32_t>(draw_list->commands.size());

        std::map<Position, std::vector<Occurrence>> positions;
        using ModelOccurrences =
            std::map<Position, std::vector<Occurrence>>;
        std::map<std::uint32_t, ModelOccurrences> object_models;
        std::map<
            std::uint32_t,
            std::map<Position, std::set<Position>>> object_view_models;
        std::set<std::pair<std::uint32_t, std::uint32_t>> sources;
        std::vector<bool> command_eligible(
            draw_list->commands.size(), false);
        for (std::size_t command_index = 0;
             command_index < draw_list->commands.size();
             ++command_index) {
            const auto& command = draw_list->commands[command_index];
            if (!eligible(command)) {
                if (
                    command.object_kind == 1 &&
                    command.channel == WorldViewChannel::main_view
                )
                    ++stats.skipped_without_provenance;
                continue;
            }
            command_eligible[command_index] = true;
            ++stats.eligible_track_commands;
            for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
                const auto& vertex = command.vertices[vertex_index];
                positions[position(vertex)].push_back(
                    Occurrence{command_index, vertex_index});
                const Occurrence occurrence{
                    command_index, vertex_index};
                const Position model = model_position(vertex);
                object_models[command.object_id][model].push_back(
                    occurrence);
                object_view_models[command.object_id][position(vertex)]
                    .insert(model);
                sources.emplace(
                    command.model_pointer,
                    vertex.source_vertex_identity);
            }
        }
        stats.unique_source_vertices =
            static_cast<std::uint32_t>(sources.size());

        std::map<Position, Occurrence> canonical_occurrences;
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
            std::set<std::pair<std::uint32_t, std::uint32_t>>
                authored_sources;
            for (const auto& occurrence : occurrences) {
                const auto& command =
                    draw_list->commands[occurrence.command];
                const auto& vertex =
                    command.vertices[occurrence.vertex];
                authored_sources.emplace(
                    command.model_pointer,
                    vertex.source_vertex_identity);
            }
            if (authored_sources.size() < 2)
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
                std::set<std::pair<Position, Position>> demonstrated;
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
                std::map<Position, std::uint32_t> translations;
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
                if (best->second < 2)
                    continue;
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
                    std::vector<Occurrence> component = left.second;
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
            }
        }

        std::map<Edge, std::vector<EdgeOccurrence>> edges;
        for (std::size_t command_index = 0;
             command_index < draw_list->commands.size();
             ++command_index) {
            if (!command_eligible[command_index])
                continue;
            const auto& command = draw_list->commands[command_index];
            for (int edge_index = 0; edge_index < 3; ++edge_index) {
                edges[Edge{
                    position(command.vertices[edge_index]),
                    position(command.vertices[(edge_index + 1) % 3]),
                }].push_back(EdgeOccurrence{command_index, edge_index});
            }
        }
        std::vector<std::pair<Edge, EdgeOccurrence>> boundaries;
        for (const auto& entry : edges) {
            if (entry.second.size() == 1) {
                ++stats.boundary_edges;
                boundaries.emplace_back(entry.first, entry.second.front());
            } else if (entry.second.size() == 2) {
                ++stats.manifold_edges;
            } else {
                ++stats.nonmanifold_edges;
            }
        }

        std::array<std::vector<Position>, 3> sorted_boundary_points;
        {
            std::set<Position> unique;
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
        std::map<std::size_t, EdgeSplits> command_splits;
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
                const auto& split_edges = found->second;
                std::vector<WorldDrawVertex> polygon;
                for (int edge_index = 0; edge_index < 3; ++edge_index) {
                    const auto& a = source.vertices[edge_index];
                    const auto& b =
                        source.vertices[(edge_index + 1) % 3];
                    polygon.push_back(a);
                    auto splits = split_edges[edge_index];
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

        if (options.deterministic_coplanar_ownership) {
            std::map<Plane, std::vector<std::size_t>> planes;
            for (std::size_t index = 0;
                 index < draw_list->commands.size();
                 ++index) {
                const auto& command = draw_list->commands[index];
                if (!eligible(command))
                    continue;
                const auto& material =
                    draw_list->materials[command.material_index];
                if ((material.primitive_flags & (1U << 1)) != 0)
                    continue;
                Plane key{};
                if (plane(command, &key))
                    planes[key].push_back(index);
            }
            DisjointSet sets(draw_list->commands.size());
            for (const auto& plane_group : planes) {
                const auto& indices = plane_group.second;
                for (std::size_t i = 0; i < indices.size(); ++i) {
                    for (std::size_t j = i + 1;
                         j < indices.size();
                         ++j) {
                        const auto& left =
                            draw_list->commands[indices[i]];
                        const auto& right =
                            draw_list->commands[indices[j]];
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
                        sets.join(indices[i], indices[j]);
                    }
                }
            }
            std::map<std::size_t, std::vector<std::size_t>> components;
            for (std::size_t index = 0;
                 index < draw_list->commands.size();
                 ++index)
                components[sets.find(index)].push_back(index);
            for (auto& entry : components) {
                auto& slots = entry.second;
                if (slots.size() < 2)
                    continue;
                ++stats.ownership_components;
                std::sort(slots.begin(), slots.end());
                std::vector<WorldDrawCommand> ordered;
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

        stats.output_commands =
            static_cast<std::uint32_t>(draw_list->commands.size());
        draw_list->track_commands = 0;
        draw_list->vehicle_commands = 0;
        draw_list->unclassified_commands = 0;
        for (const auto& command : draw_list->commands) {
            if (command.object_kind == 1)
                ++draw_list->track_commands;
            else if (command.object_kind == 2)
                ++draw_list->vehicle_commands;
            else
                ++draw_list->unclassified_commands;
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
