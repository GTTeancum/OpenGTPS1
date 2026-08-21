#include "opengt/world_interpolation.hpp"

#include <algorithm>
#include <array>
#include <chrono>
#include <cmath>
#include <cstddef>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <limits>
#include <tuple>
#include <unordered_map>
#include <utility>
#include <vector>

namespace opengt::render {
namespace {

struct VertexKey {
    std::int16_t model_x;
    std::int16_t model_y;
    std::int16_t model_z;

    bool operator==(const VertexKey& other) const noexcept {
        return
            model_x == other.model_x &&
            model_y == other.model_y &&
            model_z == other.model_z;
    }
};

struct VertexKeyHash {
    std::size_t operator()(const VertexKey& key) const noexcept {
        std::size_t result = 14695981039346656037ULL;
        const auto mix = [&result](std::uint16_t value) {
            result ^= value;
            result *= 1099511628211ULL;
        };
        mix(static_cast<std::uint16_t>(key.model_x));
        mix(static_cast<std::uint16_t>(key.model_y));
        mix(static_cast<std::uint16_t>(key.model_z));
        return result;
    }
};

struct TemporalVertexKey {
    VertexKey model;
    std::uint32_t source_vertex_identity;

    bool operator==(const TemporalVertexKey& other) const noexcept {
        return
            model == other.model &&
            source_vertex_identity == other.source_vertex_identity;
    }
};

struct TemporalVertexKeyHash {
    std::size_t operator()(const TemporalVertexKey& key) const noexcept {
        std::size_t result = VertexKeyHash{}(key.model);
        result ^= static_cast<std::size_t>(key.source_vertex_identity) +
            0x9E3779B97F4A7C15ULL + (result << 6U) + (result >> 2U);
        return result;
    }
};

struct GroupCategory {
    std::uint32_t object_kind;
    std::uint32_t object_id;
    std::uint32_t model_pointer;
    WorldViewChannel channel;

    bool operator==(const GroupCategory& other) const noexcept {
        return
            object_kind == other.object_kind &&
            object_id == other.object_id &&
            model_pointer == other.model_pointer &&
            channel == other.channel;
    }
};

struct GroupCategoryHash {
    std::size_t operator()(const GroupCategory& key) const noexcept {
        std::size_t result = key.object_kind;
        result = result * 16777619U ^ key.object_id;
        result = result * 16777619U ^ key.model_pointer;
        result = result * 16777619U ^
            static_cast<std::uint8_t>(key.channel);
        return result;
    }
};

struct GroupIdentity {
    GroupCategory category;
    std::uint64_t transform_id;

    bool operator==(const GroupIdentity& other) const noexcept {
        return
            category == other.category &&
            transform_id == other.transform_id;
    }
};

struct GroupIdentityHash {
    std::size_t operator()(const GroupIdentity& key) const noexcept {
        std::size_t result = GroupCategoryHash{}(key.category);
        result ^= static_cast<std::size_t>(key.transform_id) +
            0x9E3779B97F4A7C15ULL + (result << 6U) + (result >> 2U);
        return result;
    }
};

struct ScreenCommandKey {
    std::uint32_t primitive_flags;
    std::uint16_t texture_page;
    std::uint16_t clut;
    std::int16_t texture_mask_x;
    std::int16_t texture_mask_y;
    std::int16_t texture_offset_x;
    std::int16_t texture_offset_y;
    std::uint32_t environment_flags;
    std::int32_t ordering_table_index;
    WorldViewChannel channel;
    std::int32_t u[3];
    std::int32_t v[3];
    std::uint8_t r[3];
    std::uint8_t g[3];
    std::uint8_t b[3];

    bool operator==(const ScreenCommandKey& other) const noexcept {
        if (
            primitive_flags != other.primitive_flags ||
            texture_page != other.texture_page ||
            clut != other.clut ||
            texture_mask_x != other.texture_mask_x ||
            texture_mask_y != other.texture_mask_y ||
            texture_offset_x != other.texture_offset_x ||
            texture_offset_y != other.texture_offset_y ||
            environment_flags != other.environment_flags ||
            ordering_table_index != other.ordering_table_index ||
            channel != other.channel
        )
            return false;
        for (int index = 0; index < 3; ++index) {
            if (
                u[index] != other.u[index] ||
                v[index] != other.v[index] ||
                r[index] != other.r[index] ||
                g[index] != other.g[index] ||
                b[index] != other.b[index]
            )
                return false;
        }
        return true;
    }
};

struct ScreenCommandKeyHash {
    std::size_t operator()(const ScreenCommandKey& key) const noexcept {
        std::size_t result = key.primitive_flags;
        const auto mix = [&result](std::uint64_t value) {
            result ^= static_cast<std::size_t>(value) +
                0x9E3779B97F4A7C15ULL + (result << 6U) + (result >> 2U);
        };
        mix(key.texture_page);
        mix(key.clut);
        mix(static_cast<std::uint16_t>(key.texture_mask_x));
        mix(static_cast<std::uint16_t>(key.texture_mask_y));
        mix(static_cast<std::uint16_t>(key.texture_offset_x));
        mix(static_cast<std::uint16_t>(key.texture_offset_y));
        mix(key.environment_flags);
        mix(static_cast<std::uint32_t>(key.ordering_table_index));
        mix(static_cast<std::uint8_t>(key.channel));
        for (int index = 0; index < 3; ++index) {
            mix(static_cast<std::uint32_t>(key.u[index]));
            mix(static_cast<std::uint32_t>(key.v[index]));
            mix(key.r[index]);
            mix(key.g[index]);
            mix(key.b[index]);
        }
        return result;
    }
};

using ScreenCommandIndex = std::unordered_map<
    ScreenCommandKey,
    std::vector<std::size_t>,
    ScreenCommandKeyHash>;

struct ProjectedCommandKey {
    std::uint32_t source_command_index;
    ScreenCommandKey material;

    bool operator==(const ProjectedCommandKey& other) const noexcept {
        return
            source_command_index == other.source_command_index &&
            material == other.material;
    }
};

struct ProjectedCommandKeyHash {
    std::size_t operator()(const ProjectedCommandKey& key) const noexcept {
        std::size_t result = static_cast<std::size_t>(
            key.source_command_index);
        result ^= ScreenCommandKeyHash{}(key.material) +
            0x9E3779B97F4A7C15ULL + (result << 6U) + (result >> 2U);
        return result;
    }
};

using ProjectedCommandIndex = std::unordered_map<
    ProjectedCommandKey,
    std::vector<std::size_t>,
    ProjectedCommandKeyHash>;

struct TransformGroup {
    struct TemporalVertexBucket {
        const WorldDrawVertex* first{};
        std::vector<const WorldDrawVertex*> overflow;

        void add(const WorldDrawVertex* vertex) {
            if (first == nullptr)
                first = vertex;
            else
                overflow.push_back(vertex);
        }
    };

    GroupIdentity identity{};
    std::vector<std::size_t> commands;
    std::unordered_map<
        VertexKey,
        const WorldDrawVertex*,
        VertexKeyHash> vertices;
    std::unordered_map<
        TemporalVertexKey,
        TemporalVertexBucket,
        TemporalVertexKeyHash> temporal_vertices;
    double centroid_x{};
    double centroid_y{};
    std::size_t colliding_vertex_keys{};
    bool exact_transform_valid{};
    std::int16_t transform_rotation[9]{};
    std::int32_t transform_translation[3]{};
};

bool screen_space_command(
    const WorldDrawList& list,
    const WorldDrawCommand& command
) noexcept {
    if (command.material_index >= list.materials.size())
        return false;
    return
        (list.materials[command.material_index].primitive_flags &
            world_primitive_screen_space_flag) != 0;
}

bool textured_screen_space_command(
    const WorldDrawList& list,
    const WorldDrawCommand& command
) noexcept {
    if (
        !screen_space_command(list, command) ||
        command.material_index >= list.materials.size()
    )
        return false;
    constexpr std::uint32_t textured_flag = 1U << 0;
    return
        (list.materials[command.material_index].primitive_flags &
            textured_flag) != 0;
}

bool large_untextured_screen_space_command(
    const WorldDrawList& list,
    const WorldDrawCommand& command
) noexcept {
    if (
        !screen_space_command(list, command) ||
        command.material_index >= list.materials.size()
    )
        return false;
    constexpr std::uint32_t textured_flag = 1U << 0;
    if ((list.materials[command.material_index].primitive_flags &
            textured_flag) != 0)
        return false;

    float minimum_x = command.vertices[0].screen_x;
    float maximum_x = minimum_x;
    float minimum_y = command.vertices[0].screen_y;
    float maximum_y = minimum_y;
    for (const auto& vertex : command.vertices) {
        minimum_x = std::min(minimum_x, vertex.screen_x);
        maximum_x = std::max(maximum_x, vertex.screen_x);
        minimum_y = std::min(minimum_y, vertex.screen_y);
        maximum_y = std::max(maximum_y, vertex.screen_y);
    }

    const float width = maximum_x - minimum_x;
    const float height = maximum_y - minimum_y;
    if (
        width < static_cast<float>(list.display_width) * 0.75F ||
        height < static_cast<float>(list.display_height) * 0.10F
    )
        return false;
    return width * height >=
        static_cast<float>(list.display_width * list.display_height) * 0.125F;
}

bool interpolated_screen_space_command(
    const WorldDrawList& list,
    const WorldDrawCommand& command
) noexcept {
    return
        textured_screen_space_command(list, command) ||
        large_untextured_screen_space_command(list, command);
}

bool unowned_projected_command(
    const WorldDrawList& list,
    const WorldDrawCommand& command
) noexcept {
    if (
        screen_space_command(list, command) ||
        command.object_kind != 0 ||
        command.object_id != 0 ||
        command.model_pointer != 0 ||
        command.transform_id != 0
    )
        return false;
    for (const auto& vertex : command.vertices) {
        if (
            !std::isfinite(vertex.view_x) ||
            !std::isfinite(vertex.view_y) ||
            !std::isfinite(vertex.view_z) ||
            !std::isfinite(vertex.projection_plane) ||
            vertex.view_z <= 0.0F ||
            vertex.projection_plane <= 0.0F
        )
            return false;
    }
    return true;
}

bool screen_offset_anchor_command(
    const WorldDrawCommand& command
) noexcept {
    for (const auto& vertex : command.vertices) {
        if ((vertex.provenance_flags &
                world_vertex_screen_offset_anchor_flag) == 0)
            return false;
    }
    return true;
}

ScreenCommandKey screen_command_key(
    const WorldDrawList& list,
    const WorldDrawCommand& command
) noexcept {
    const WorldMaterial* material = command.material_index < list.materials.size()
        ? &list.materials[command.material_index]
        : nullptr;
    ScreenCommandKey key{};
    if (material != nullptr) {
        key.primitive_flags =
            material->primitive_flags & ~world_primitive_screen_space_flag;
        key.texture_page = material->texture_page;
        key.clut = material->clut;
        key.texture_mask_x = material->texture_mask_x;
        key.texture_mask_y = material->texture_mask_y;
        key.texture_offset_x = material->texture_offset_x;
        key.texture_offset_y = material->texture_offset_y;
        key.environment_flags = material->environment_flags;
    }
    key.ordering_table_index = command.ordering_table_index;
    key.channel = command.channel;
    for (int index = 0; index < 3; ++index) {
        key.u[index] = static_cast<std::int32_t>(
            std::lround(command.vertices[index].u));
        key.v[index] = static_cast<std::int32_t>(
            std::lround(command.vertices[index].v));
        key.r[index] = command.vertices[index].r;
        key.g[index] = command.vertices[index].g;
        key.b[index] = command.vertices[index].b;
    }
    return key;
}

ScreenCommandIndex build_screen_command_index(
    const WorldDrawList& list
) {
    ScreenCommandIndex result;
    result.reserve(list.commands.size());
    for (std::size_t index = 0; index < list.commands.size(); ++index) {
        const auto& command = list.commands[index];
        if (!interpolated_screen_space_command(list, command))
            continue;
        result[screen_command_key(list, command)].push_back(index);
    }
    return result;
}

ProjectedCommandIndex build_projected_command_index(
    const WorldDrawList& list
) {
    ProjectedCommandIndex result;
    result.reserve(list.commands.size());
    for (std::size_t index = 0; index < list.commands.size(); ++index) {
        const auto& command = list.commands[index];
        if (!unowned_projected_command(list, command))
            continue;
        result[ProjectedCommandKey{
            command.source_command_index,
            screen_command_key(list, command),
        }].push_back(index);
    }
    return result;
}

double screen_centroid_distance_squared(
    const WorldDrawCommand& left,
    const WorldDrawCommand& right
) noexcept {
    double left_x = 0.0;
    double left_y = 0.0;
    double right_x = 0.0;
    double right_y = 0.0;
    for (int index = 0; index < 3; ++index) {
        left_x += left.vertices[index].screen_x;
        left_y += left.vertices[index].screen_y;
        right_x += right.vertices[index].screen_x;
        right_y += right.vertices[index].screen_y;
    }
    const double dx = left_x / 3.0 - right_x / 3.0;
    const double dy = left_y / 3.0 - right_y / 3.0;
    return dx * dx + dy * dy;
}

void refresh_screen_command_projection(
    const WorldDrawList& list,
    WorldDrawCommand* command
) noexcept {
    float minimum_x = command->vertices[0].screen_x;
    float maximum_x = minimum_x;
    float minimum_y = command->vertices[0].screen_y;
    float maximum_y = minimum_y;
    for (auto& vertex : command->vertices) {
        const float clip_w = 1.0F;
        vertex.clip_w = clip_w;
        vertex.clip_z = 0.5F;
        vertex.clip_x =
            ((vertex.screen_x - list.display_x) /
                static_cast<float>(list.display_width)) *
            2.0F - 1.0F;
        vertex.clip_y =
            1.0F -
            ((vertex.screen_y - list.display_y) /
                static_cast<float>(list.display_height)) *
            2.0F;
        minimum_x = std::min(minimum_x, vertex.screen_x);
        maximum_x = std::max(maximum_x, vertex.screen_x);
        minimum_y = std::min(minimum_y, vertex.screen_y);
        maximum_y = std::max(maximum_y, vertex.screen_y);
    }
    command->clip_x0 = static_cast<std::int16_t>(std::floor(minimum_x));
    command->clip_y0 = static_cast<std::int16_t>(std::floor(minimum_y));
    command->clip_x1 = static_cast<std::int16_t>(std::ceil(maximum_x));
    command->clip_y1 = static_cast<std::int16_t>(std::ceil(maximum_y));
}

bool interpolate_screen_command(
    const WorldDrawList& previous,
    const WorldDrawList& current,
    const WorldDrawCommand& previous_command,
    const ScreenCommandIndex& current_screen_commands,
    std::vector<bool>* current_screen_used,
    float alpha,
    WorldDrawCommand* output
) noexcept {
    if (
        current_screen_used == nullptr ||
        output == nullptr ||
        !interpolated_screen_space_command(previous, previous_command)
    )
        return false;
    const auto found = current_screen_commands.find(
        screen_command_key(previous, previous_command));
    if (found == current_screen_commands.end())
        return false;

    constexpr double maximum_centroid_motion_squared = 96.0 * 96.0;
    std::size_t best = current.commands.size();
    double best_cost = maximum_centroid_motion_squared;
    for (const std::size_t current_index : found->second) {
        if (
            current_index >= current.commands.size() ||
            (*current_screen_used)[current_index]
        )
            continue;
        const double cost = screen_centroid_distance_squared(
            previous_command,
            current.commands[current_index]);
        if (cost < best_cost) {
            best = current_index;
            best_cost = cost;
        }
    }
    if (best >= current.commands.size())
        return false;

    const auto& current_command = current.commands[best];
    for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
        output->vertices[vertex_index].screen_x =
            previous_command.vertices[vertex_index].screen_x +
            (
                current_command.vertices[vertex_index].screen_x -
                previous_command.vertices[vertex_index].screen_x
            ) * alpha;
        output->vertices[vertex_index].screen_y =
            previous_command.vertices[vertex_index].screen_y +
            (
                current_command.vertices[vertex_index].screen_y -
                previous_command.vertices[vertex_index].screen_y
            ) * alpha;
    }
    refresh_screen_command_projection(previous, output);
    (*current_screen_used)[best] = true;
    return true;
}

VertexKey vertex_key(const WorldDrawVertex& vertex) noexcept {
    return VertexKey{vertex.model_x, vertex.model_y, vertex.model_z};
}

TemporalVertexKey temporal_vertex_key(
    const WorldDrawVertex& vertex
) noexcept {
    return TemporalVertexKey{
        vertex_key(vertex),
        vertex.source_vertex_identity,
    };
}

GroupIdentity group_identity(const WorldDrawCommand& command) noexcept {
    return GroupIdentity{
        GroupCategory{
            command.object_kind,
            command.object_id,
            command.model_pointer,
            command.channel,
        },
        command.transform_id,
    };
}

std::vector<TransformGroup> build_groups(const WorldDrawList& list) {
    std::vector<TransformGroup> groups;
    groups.reserve(64);
    std::unordered_map<GroupIdentity, std::size_t, GroupIdentityHash>
        indices;
    indices.reserve(64);
    for (std::size_t command_index = 0;
         command_index < list.commands.size();
         ++command_index) {
        const auto& command = list.commands[command_index];
        if (screen_offset_anchor_command(command))
            continue;
        if (
            command.object_kind == 0 &&
            command.model_pointer == 0 &&
            command.transform_id == 0
        )
            continue;
        const GroupIdentity identity = group_identity(command);
        const auto inserted = indices.emplace(identity, groups.size());
        if (inserted.second) {
            groups.push_back(TransformGroup{});
        }
        auto& group = groups[inserted.first->second];
        if (inserted.second) {
            group.identity = identity;
            group.commands.reserve(64);
            group.vertices.reserve(192);
            group.temporal_vertices.reserve(192);
            group.exact_transform_valid =
                command.exact_transform_valid;
            for (int component = 0; component < 9; ++component)
                group.transform_rotation[component] =
                    command.transform_rotation[component];
            for (int component = 0; component < 3; ++component)
                group.transform_translation[component] =
                    command.transform_translation[component];
        } else if (group.exact_transform_valid) {
            bool same_transform = command.exact_transform_valid;
            for (int component = 0; component < 9; ++component)
                same_transform = same_transform &&
                    group.transform_rotation[component] ==
                        command.transform_rotation[component];
            for (int component = 0; component < 3; ++component)
                same_transform = same_transform &&
                    group.transform_translation[component] ==
                        command.transform_translation[component];
            group.exact_transform_valid = same_transform;
        }
        group.commands.push_back(command_index);
        for (const auto& vertex : command.vertices) {
            // emplace keeps the first vertex for a key and drops the rest, so
            // a colliding key silently makes a later frame match the wrong
            // physical vertex. Count collisions that actually disagree about
            // where the vertex is; identical duplicates are harmless.
            const auto inserted =
                group.vertices.emplace(vertex_key(vertex), &vertex);
            if (!inserted.second) {
                const auto& kept = *inserted.first->second;
                if (kept.view_x != vertex.view_x ||
                    kept.view_y != vertex.view_y ||
                    kept.view_z != vertex.view_z)
                    ++group.colliding_vertex_keys;
            }
            group.temporal_vertices[temporal_vertex_key(vertex)].add(&vertex);
            group.centroid_x += vertex.screen_x - list.display_x;
            group.centroid_y += vertex.screen_y - list.display_y;
        }
    }
    for (auto& group : groups) {
        const double count = group.commands.size() * 3.0;
        group.centroid_x /= count;
        group.centroid_y /= count;
    }
    return groups;
}

struct InterpolationCacheStorage {
    const WorldDrawCommand* commands{};
    std::size_t command_count{};
    std::vector<TransformGroup> groups;

    bool matches(const WorldDrawList& list) const noexcept {
        return
            commands == list.commands.data() &&
            command_count == list.commands.size();
    }
};

TransformGroup merge_category_vertices(
    const std::vector<TransformGroup>& groups,
    const GroupCategory& category
) {
    TransformGroup merged{};
    merged.identity.category = category;
    std::size_t vertex_count = 0;
    std::size_t temporal_vertex_count = 0;
    for (const auto& group : groups) {
        if (!(group.identity.category == category))
            continue;
        vertex_count += group.vertices.size();
        temporal_vertex_count += group.temporal_vertices.size();
    }
    merged.vertices.reserve(vertex_count);
    merged.temporal_vertices.reserve(temporal_vertex_count);
    for (const auto& group : groups) {
        if (!(group.identity.category == category))
            continue;
        for (const auto& entry : group.vertices)
            merged.vertices.emplace(entry.first, entry.second);
        for (const auto& entry : group.temporal_vertices) {
            auto& destination = merged.temporal_vertices[entry.first];
            destination.add(entry.second.first);
            for (const WorldDrawVertex* vertex : entry.second.overflow)
                destination.add(vertex);
        }
    }
    return merged;
}

using MergedCategoryCache = std::unordered_map<
    GroupCategory,
    TransformGroup,
    GroupCategoryHash>;

const TransformGroup& merged_category_vertices(
    const std::vector<TransformGroup>& groups,
    const GroupCategory& category,
    MergedCategoryCache* cache
) {
    auto found = cache->find(category);
    if (found != cache->end())
        return found->second;
    TransformGroup merged = merge_category_vertices(groups, category);
    const auto inserted = cache->emplace(category, std::move(merged));
    return inserted.first->second;
}

std::vector<std::size_t> match_groups(
    const std::vector<TransformGroup>& previous,
    const std::vector<TransformGroup>& current
) {
    const std::size_t unmatched = current.size();
    std::vector<std::size_t> matches(previous.size(), unmatched);
    std::vector<bool> current_used(current.size(), false);
    std::vector<std::size_t> order(previous.size());
    for (std::size_t index = 0; index < order.size(); ++index)
        order[index] = index;
    // Pair the high-detail body/section group before identical low-detail
    // wheel groups. The latter are then paired by screen centroid.
    std::stable_sort(
        order.begin(),
        order.end(),
        [&](std::size_t left, std::size_t right) {
            return previous[left].commands.size() >
                previous[right].commands.size();
        });
    for (const std::size_t previous_index : order) {
        const auto& source = previous[previous_index];
        std::size_t best = unmatched;
        double best_cost = (std::numeric_limits<double>::max)();
        for (std::size_t current_index = 0;
             current_index < current.size();
             ++current_index) {
            if (
                current_used[current_index] ||
                !(current[current_index].identity.category ==
                    source.identity.category)
            )
                continue;
            const auto& candidate = current[current_index];
            const double command_delta = std::fabs(
                static_cast<double>(source.commands.size()) -
                static_cast<double>(candidate.commands.size()));
            const double dx =
                source.centroid_x - candidate.centroid_x;
            const double dy =
                source.centroid_y - candidate.centroid_y;
            // Command cardinality distinguishes a car body from its wheels
            // and shadow. Centroid distance then distinguishes the four
            // instances of the same wheel model.
            const double cost =
                command_delta * 1000000.0 + dx * dx + dy * dy;
            if (cost < best_cost) {
                best = current_index;
                best_cost = cost;
            }
        }
        if (best != unmatched) {
            matches[previous_index] = best;
            current_used[best] = true;
        }
    }
    return matches;
}

float interpolate(float a, float b, float alpha) noexcept {
    return a + (b - a) * alpha;
}

struct Vector3 {
    double x{};
    double y{};
    double z{};
};

struct Quaternion {
    double w{1.0};
    double x{};
    double y{};
    double z{};
};

struct Matrix3 {
    double value[9]{};
};

struct RigidTransform {
    bool valid{};
    bool exact{};
    bool hold{};
    Vector3 previous_centroid{};
    Vector3 current_centroid{};
    Quaternion rotation{};
    Quaternion previous_rotation{};
    Quaternion sample_rotation{};
    Vector3 previous_translation{};
    Vector3 current_translation{};
    Vector3 sample_translation{};
    Matrix3 previous_stretch{};
    Matrix3 current_stretch{};
    Matrix3 sample_matrix{};
    double scale{1.0};
    double sample_scale{1.0};
    std::size_t point_count{};
    double radius{};
    double rms{};
};

Vector3 view_position(const WorldDrawVertex& vertex) noexcept {
    return Vector3{vertex.view_x, vertex.view_y, vertex.view_z};
}

Vector3 subtract(const Vector3& left, const Vector3& right) noexcept {
    return Vector3{
        left.x - right.x,
        left.y - right.y,
        left.z - right.z,
    };
}

Vector3 rotate(const Quaternion& q, const Vector3& value) noexcept {
    // Expanded q * v * conjugate(q).  The fit always supplies a normalized
    // quaternion, so this preserves the model's dimensions exactly.
    const double tx = 2.0 * (q.y * value.z - q.z * value.y);
    const double ty = 2.0 * (q.z * value.x - q.x * value.z);
    const double tz = 2.0 * (q.x * value.y - q.y * value.x);
    return Vector3{
        value.x + q.w * tx + (q.y * tz - q.z * ty),
        value.y + q.w * ty + (q.z * tx - q.x * tz),
        value.z + q.w * tz + (q.x * ty - q.y * tx),
    };
}

Quaternion normalized(Quaternion value) noexcept {
    const double length = std::sqrt(
        value.w * value.w + value.x * value.x +
        value.y * value.y + value.z * value.z);
    if (length <= 1.0e-12 || !std::isfinite(length))
        return Quaternion{};
    value.w /= length;
    value.x /= length;
    value.y /= length;
    value.z /= length;
    return value;
}

Quaternion multiply(
    const Quaternion& left,
    const Quaternion& right
) noexcept {
    return normalized(Quaternion{
        left.w * right.w - left.x * right.x -
            left.y * right.y - left.z * right.z,
        left.w * right.x + left.x * right.w +
            left.y * right.z - left.z * right.y,
        left.w * right.y - left.x * right.z +
            left.y * right.w + left.z * right.x,
        left.w * right.z + left.x * right.y -
            left.y * right.x + left.z * right.w,
    });
}

Quaternion conjugate(const Quaternion& value) noexcept {
    return Quaternion{value.w, -value.x, -value.y, -value.z};
}

Matrix3 rotation_matrix(const Quaternion& input) noexcept {
    const Quaternion q = normalized(input);
    const double xx = q.x * q.x;
    const double yy = q.y * q.y;
    const double zz = q.z * q.z;
    const double xy = q.x * q.y;
    const double xz = q.x * q.z;
    const double yz = q.y * q.z;
    const double wx = q.w * q.x;
    const double wy = q.w * q.y;
    const double wz = q.w * q.z;
    return Matrix3{{
        1.0 - 2.0 * (yy + zz), 2.0 * (xy - wz),
            2.0 * (xz + wy),
        2.0 * (xy + wz), 1.0 - 2.0 * (xx + zz),
            2.0 * (yz - wx),
        2.0 * (xz - wy), 2.0 * (yz + wx),
            1.0 - 2.0 * (xx + yy),
    }};
}

Matrix3 fixed_matrix(const std::int16_t rotation[9]) noexcept {
    constexpr double fixed_scale = 1.0 / 4096.0;
    Matrix3 result{};
    for (int component = 0; component < 9; ++component)
        result.value[component] = rotation[component] * fixed_scale;
    return result;
}

Vector3 matrix_column(const Matrix3& matrix, int column) noexcept {
    return Vector3{
        matrix.value[column],
        matrix.value[3 + column],
        matrix.value[6 + column],
    };
}

double dot(const Vector3& left, const Vector3& right) noexcept {
    return left.x * right.x + left.y * right.y + left.z * right.z;
}

Vector3 cross(const Vector3& left, const Vector3& right) noexcept {
    return Vector3{
        left.y * right.z - left.z * right.y,
        left.z * right.x - left.x * right.z,
        left.x * right.y - left.y * right.x,
    };
}

Vector3 normalized_vector(Vector3 value, Vector3 fallback) noexcept {
    const double length = std::sqrt(dot(value, value));
    if (!std::isfinite(length) || length <= 1.0e-12)
        return fallback;
    value.x /= length;
    value.y /= length;
    value.z /= length;
    return value;
}

Matrix3 nearest_rotation(const Matrix3& matrix) noexcept {
    Vector3 x = normalized_vector(
        matrix_column(matrix, 0), Vector3{1.0, 0.0, 0.0});
    Vector3 y = matrix_column(matrix, 1);
    const double along_x = dot(y, x);
    y = normalized_vector(
        Vector3{
            y.x - along_x * x.x,
            y.y - along_x * x.y,
            y.z - along_x * x.z,
        },
        Vector3{0.0, 1.0, 0.0});
    Vector3 z = normalized_vector(
        cross(x, y), Vector3{0.0, 0.0, 1.0});
    if (dot(z, matrix_column(matrix, 2)) < 0.0) {
        z.x = -z.x;
        z.y = -z.y;
        z.z = -z.z;
    }
    y = normalized_vector(cross(z, x), y);
    return Matrix3{{
        x.x, y.x, z.x,
        x.y, y.y, z.y,
        x.z, y.z, z.z,
    }};
}

Matrix3 transpose(const Matrix3& matrix) noexcept {
    return Matrix3{{
        matrix.value[0], matrix.value[3], matrix.value[6],
        matrix.value[1], matrix.value[4], matrix.value[7],
        matrix.value[2], matrix.value[5], matrix.value[8],
    }};
}

Matrix3 multiply(const Matrix3& left, const Matrix3& right) noexcept {
    Matrix3 result{};
    for (int row = 0; row < 3; ++row) {
        for (int column = 0; column < 3; ++column) {
            for (int inner = 0; inner < 3; ++inner) {
                result.value[row * 3 + column] +=
                    left.value[row * 3 + inner] *
                    right.value[inner * 3 + column];
            }
        }
    }
    return result;
}

double determinant(const Matrix3& matrix) noexcept {
    return
        matrix.value[0] * (
            matrix.value[4] * matrix.value[8] -
            matrix.value[5] * matrix.value[7]) -
        matrix.value[1] * (
            matrix.value[3] * matrix.value[8] -
            matrix.value[5] * matrix.value[6]) +
        matrix.value[2] * (
            matrix.value[3] * matrix.value[7] -
            matrix.value[4] * matrix.value[6]);
}

bool inverse(const Matrix3& matrix, Matrix3* output) noexcept {
    if (output == nullptr)
        return false;
    const double det = determinant(matrix);
    if (!std::isfinite(det) || std::fabs(det) <= 1.0e-9)
        return false;
    const double scale = 1.0 / det;
    *output = Matrix3{{
        (matrix.value[4] * matrix.value[8] -
            matrix.value[5] * matrix.value[7]) * scale,
        (matrix.value[2] * matrix.value[7] -
            matrix.value[1] * matrix.value[8]) * scale,
        (matrix.value[1] * matrix.value[5] -
            matrix.value[2] * matrix.value[4]) * scale,
        (matrix.value[5] * matrix.value[6] -
            matrix.value[3] * matrix.value[8]) * scale,
        (matrix.value[0] * matrix.value[8] -
            matrix.value[2] * matrix.value[6]) * scale,
        (matrix.value[2] * matrix.value[3] -
            matrix.value[0] * matrix.value[5]) * scale,
        (matrix.value[3] * matrix.value[7] -
            matrix.value[4] * matrix.value[6]) * scale,
        (matrix.value[1] * matrix.value[6] -
            matrix.value[0] * matrix.value[7]) * scale,
        (matrix.value[0] * matrix.value[4] -
            matrix.value[1] * matrix.value[3]) * scale,
    }};
    return true;
}

Vector3 multiply(const Matrix3& matrix, const Vector3& vector) noexcept {
    return Vector3{
        matrix.value[0] * vector.x + matrix.value[1] * vector.y +
            matrix.value[2] * vector.z,
        matrix.value[3] * vector.x + matrix.value[4] * vector.y +
            matrix.value[5] * vector.z,
        matrix.value[6] * vector.x + matrix.value[7] * vector.y +
            matrix.value[8] * vector.z,
    };
}

Matrix3 interpolate(
    const Matrix3& previous,
    const Matrix3& current,
    float alpha
) noexcept {
    Matrix3 result{};
    for (int component = 0; component < 9; ++component) {
        result.value[component] = previous.value[component] +
            (current.value[component] - previous.value[component]) * alpha;
    }
    return result;
}

Quaternion quaternion_from_matrix(const Matrix3& matrix) noexcept {
    Quaternion result{};
    const double m00 = matrix.value[0];
    const double m01 = matrix.value[1];
    const double m02 = matrix.value[2];
    const double m10 = matrix.value[3];
    const double m11 = matrix.value[4];
    const double m12 = matrix.value[5];
    const double m20 = matrix.value[6];
    const double m21 = matrix.value[7];
    const double m22 = matrix.value[8];
    const double trace = m00 + m11 + m22;
    if (trace > 0.0) {
        const double scale = std::sqrt(trace + 1.0) * 2.0;
        result.w = 0.25 * scale;
        result.x = (m21 - m12) / scale;
        result.y = (m02 - m20) / scale;
        result.z = (m10 - m01) / scale;
    } else if (m00 > m11 && m00 > m22) {
        const double scale = std::sqrt(
            std::max(0.0, 1.0 + m00 - m11 - m22)) * 2.0;
        if (scale <= 1.0e-12)
            return Quaternion{};
        result.w = (m21 - m12) / scale;
        result.x = 0.25 * scale;
        result.y = (m01 + m10) / scale;
        result.z = (m02 + m20) / scale;
    } else if (m11 > m22) {
        const double scale = std::sqrt(
            std::max(0.0, 1.0 + m11 - m00 - m22)) * 2.0;
        if (scale <= 1.0e-12)
            return Quaternion{};
        result.w = (m02 - m20) / scale;
        result.x = (m01 + m10) / scale;
        result.y = 0.25 * scale;
        result.z = (m12 + m21) / scale;
    } else {
        const double scale = std::sqrt(
            std::max(0.0, 1.0 + m22 - m00 - m11)) * 2.0;
        if (scale <= 1.0e-12)
            return Quaternion{};
        result.w = (m10 - m01) / scale;
        result.x = (m02 + m20) / scale;
        result.y = (m12 + m21) / scale;
        result.z = 0.25 * scale;
    }
    return normalized(result);
}

Quaternion quaternion_from_rotation(
    const std::int16_t rotation[9]
) noexcept {
    return quaternion_from_matrix(nearest_rotation(fixed_matrix(rotation)));
}

Quaternion fractional_rotation(Quaternion value, float alpha) noexcept {
    value = normalized(value);
    // q and -q encode the same endpoint.  Select the representation reached
    // from identity without an unnecessary full revolution.
    if (value.w < 0.0) {
        value.w = -value.w;
        value.x = -value.x;
        value.y = -value.y;
        value.z = -value.z;
    }
    const double half_angle = std::acos(std::clamp(value.w, -1.0, 1.0));
    const double sine = std::sin(half_angle);
    if (std::fabs(sine) <= 1.0e-8)
        return normalized(Quaternion{
            interpolate(1.0F, static_cast<float>(value.w), alpha),
            value.x * alpha,
            value.y * alpha,
            value.z * alpha,
        });
    const double scale = std::sin(half_angle * alpha) / sine;
    return normalized(Quaternion{
        std::cos(half_angle * alpha),
        value.x * scale,
        value.y * scale,
        value.z * scale,
    });
}

void store_exact_sample_transform(
    const RigidTransform& transform,
    WorldDrawCommand* command
) noexcept {
    if (!transform.exact || command == nullptr)
        return;
    for (int component = 0; component < 9; ++component) {
        const long value = std::lround(
            transform.sample_matrix.value[component] * 4096.0);
        command->transform_rotation[component] =
            static_cast<std::int16_t>(std::clamp(
                value,
                static_cast<long>((std::numeric_limits<std::int16_t>::min)()),
                static_cast<long>((std::numeric_limits<std::int16_t>::max)())));
    }
    command->transform_translation[0] = static_cast<std::int32_t>(
        std::lround(transform.sample_translation.x));
    command->transform_translation[1] = static_cast<std::int32_t>(
        std::lround(transform.sample_translation.y));
    command->transform_translation[2] = static_cast<std::int32_t>(
        std::lround(transform.sample_translation.z));
    command->exact_transform_valid = true;
}

void store_exact_sample_transform(
    const RigidTransform& transform,
    WorldDrawVertex* vertex
) noexcept {
    if (!transform.exact || vertex == nullptr)
        return;
    for (int component = 0; component < 9; ++component) {
        const long value = std::lround(
            transform.sample_matrix.value[component] * 4096.0);
        vertex->transform_rotation[component] =
            static_cast<std::int16_t>(std::clamp(
                value,
                static_cast<long>((std::numeric_limits<std::int16_t>::min)()),
                static_cast<long>((std::numeric_limits<std::int16_t>::max)())));
    }
    vertex->transform_translation[0] = static_cast<std::int32_t>(
        std::lround(transform.sample_translation.x));
    vertex->transform_translation[1] = static_cast<std::int32_t>(
        std::lround(transform.sample_translation.y));
    vertex->transform_translation[2] = static_cast<std::int32_t>(
        std::lround(transform.sample_translation.z));
    vertex->exact_transform_valid = true;
}

bool exact_transform_is_coherent(
    const WorldDrawVertex& vertex
) noexcept {
    if (!vertex.exact_transform_valid || vertex.transform_id == 0)
        return false;
    const Matrix3 matrix = fixed_matrix(vertex.transform_rotation);
    const Vector3 predicted = multiply(
        matrix,
        Vector3{
            static_cast<double>(vertex.model_x),
            static_cast<double>(vertex.model_y),
            static_cast<double>(vertex.model_z),
        });
    const double dx = predicted.x + vertex.transform_translation[0] -
        vertex.view_x;
    const double dy = predicted.y + vertex.transform_translation[1] -
        vertex.view_y;
    const double dz = predicted.z + vertex.transform_translation[2] -
        vertex.view_z;
    constexpr double maximum_residual_squared = 4.0 * 4.0;
    const double residual_squared = dx * dx + dy * dy + dz * dz;
    return
        std::isfinite(residual_squared) &&
        residual_squared <= maximum_residual_squared;
}

bool same_exact_transform(
    const WorldDrawVertex& left,
    const WorldDrawVertex& right
) noexcept {
    if (
        !left.exact_transform_valid ||
        !right.exact_transform_valid
    ) {
        return false;
    }
    for (int component = 0; component < 9; ++component) {
        if (left.transform_rotation[component] !=
            right.transform_rotation[component]) {
            return false;
        }
    }
    for (int component = 0; component < 3; ++component) {
        if (left.transform_translation[component] !=
            right.transform_translation[component]) {
            return false;
        }
    }
    return true;
}

bool exact_transform_is_coherent(const TransformGroup& group) noexcept {
    if (!group.exact_transform_valid || group.vertices.empty())
        return false;
    const Matrix3 matrix = fixed_matrix(group.transform_rotation);
    const Vector3 translation{
        static_cast<double>(group.transform_translation[0]),
        static_cast<double>(group.transform_translation[1]),
        static_cast<double>(group.transform_translation[2]),
    };
    // Across 1,890 groups from the SSR11 failure trace, genuine captured GTE
    // transforms had a maximum endpoint residual of 1.701 units. Visibility-
    // clipped fallback fragments formed a separate 8,918+ unit population:
    // their integer model coordinates no longer belong to the copied matrix.
    // Leave headroom for GTE integer rounding, but never apply such an
    // incoherent matrix to model coordinates during a synthetic midpoint.
    constexpr double maximum_residual = 4.0;
    constexpr double maximum_residual_squared =
        maximum_residual * maximum_residual;
    for (const auto& entry : group.vertices) {
        const auto& vertex = *entry.second;
        const Vector3 predicted = multiply(
            matrix,
            Vector3{
                static_cast<double>(vertex.model_x),
                static_cast<double>(vertex.model_y),
                static_cast<double>(vertex.model_z),
            });
        const double dx = predicted.x + translation.x - vertex.view_x;
        const double dy = predicted.y + translation.y - vertex.view_y;
        const double dz = predicted.z + translation.z - vertex.view_z;
        const double residual_squared = dx * dx + dy * dy + dz * dz;
        if (
            !std::isfinite(residual_squared) ||
            residual_squared > maximum_residual_squared
        )
            return false;
    }
    return true;
}

RigidTransform exact_rigid_transform(
    const std::int16_t previous_rotation[9],
    const std::int32_t previous_translation[3],
    const std::int16_t current_rotation[9],
    const std::int32_t current_translation[3]
) noexcept {
    RigidTransform result{};
    result.valid = true;
    result.exact = true;
    const Matrix3 previous_matrix = fixed_matrix(previous_rotation);
    const Matrix3 current_matrix = fixed_matrix(current_rotation);
    const Matrix3 previous_rotation_matrix = nearest_rotation(
        previous_matrix);
    const Matrix3 current_rotation_matrix = nearest_rotation(current_matrix);
    result.previous_rotation = quaternion_from_matrix(
        previous_rotation_matrix);
    const Quaternion current_rotation_quaternion = quaternion_from_matrix(
        current_rotation_matrix);
    result.rotation = multiply(
        current_rotation_quaternion,
        conjugate(result.previous_rotation));
    result.previous_stretch = multiply(
        transpose(previous_rotation_matrix), previous_matrix);
    result.current_stretch = multiply(
        transpose(current_rotation_matrix), current_matrix);
    result.previous_translation = Vector3{
        static_cast<double>(previous_translation[0]),
        static_cast<double>(previous_translation[1]),
        static_cast<double>(previous_translation[2]),
    };
    result.current_translation = Vector3{
        static_cast<double>(current_translation[0]),
        static_cast<double>(current_translation[1]),
        static_cast<double>(current_translation[2]),
    };
    return result;
}

RigidTransform fit_rigid_transform(
    const TransformGroup& previous,
    const TransformGroup& current
) noexcept {
    RigidTransform result{};
    if (
        previous.exact_transform_valid &&
        current.exact_transform_valid
    ) {
        // A copied exact matrix on topology-generated clipping fragments is
        // positive evidence that these are not an ordinary articulated group
        // needing a reconstructed Horn transform. If either endpoint is
        // incoherent, use bounded provenance interpolation/hold instead of
        // letting the generic rigid solver turn car-body fragments into
        // wheel-like shards.
        if (
            !exact_transform_is_coherent(previous) ||
            !exact_transform_is_coherent(current)
        ) {
            result.hold = true;
            return result;
        }
        // Guest GTE matrices are affine object transforms, not guaranteed
        // unit rotations. Vehicle wheels in retail GT2 commonly carry about
        // 1/16 model scale in this matrix. Preserve the full residual
        // scale/shear while interpolating only its rotational component as a
        // quaternion; normalizing the raw matrix makes wheels expand to full
        // model size on every synthetic midpoint.
        return exact_rigid_transform(
            previous.transform_rotation,
            previous.transform_translation,
            current.transform_rotation,
            current.transform_translation);
    }
    std::size_t point_count = 0;
    for (const auto& entry : previous.vertices) {
        const auto found = current.vertices.find(entry.first);
        if (found == current.vertices.end())
            continue;
        const Vector3 p = view_position(*entry.second);
        const Vector3 q = view_position(*found->second);
        if (
            !std::isfinite(p.x) || !std::isfinite(p.y) ||
            !std::isfinite(p.z) || !std::isfinite(q.x) ||
            !std::isfinite(q.y) || !std::isfinite(q.z)
        )
            continue;
        result.previous_centroid.x += p.x;
        result.previous_centroid.y += p.y;
        result.previous_centroid.z += p.z;
        result.current_centroid.x += q.x;
        result.current_centroid.y += q.y;
        result.current_centroid.z += q.z;
        ++point_count;
    }
    if (point_count < 3)
        return result;
    result.point_count = point_count;
    const double inverse_count = 1.0 / point_count;
    result.previous_centroid.x *= inverse_count;
    result.previous_centroid.y *= inverse_count;
    result.previous_centroid.z *= inverse_count;
    result.current_centroid.x *= inverse_count;
    result.current_centroid.y *= inverse_count;
    result.current_centroid.z *= inverse_count;

    double covariance[3][3]{};
    double radius_squared = 0.0;
    for (const auto& entry : previous.vertices) {
        const auto found = current.vertices.find(entry.first);
        if (found == current.vertices.end())
            continue;
        const Vector3 source = view_position(*entry.second);
        const Vector3 target = view_position(*found->second);
        if (
            !std::isfinite(source.x) || !std::isfinite(source.y) ||
            !std::isfinite(source.z) || !std::isfinite(target.x) ||
            !std::isfinite(target.y) || !std::isfinite(target.z)
        )
            continue;
        const Vector3 p = subtract(source, result.previous_centroid);
        const Vector3 q = subtract(target, result.current_centroid);
        covariance[0][0] += p.x * q.x;
        covariance[0][1] += p.x * q.y;
        covariance[0][2] += p.x * q.z;
        covariance[1][0] += p.y * q.x;
        covariance[1][1] += p.y * q.y;
        covariance[1][2] += p.y * q.z;
        covariance[2][0] += p.z * q.x;
        covariance[2][1] += p.z * q.y;
        covariance[2][2] += p.z * q.z;
        radius_squared += p.x * p.x + p.y * p.y + p.z * p.z;
    }
    const double radius = std::sqrt(radius_squared * inverse_count);
    result.radius = radius;
    if (!std::isfinite(radius) || radius < 1.0)
        return result;

    const double sxx = covariance[0][0];
    const double sxy = covariance[0][1];
    const double sxz = covariance[0][2];
    const double syx = covariance[1][0];
    const double syy = covariance[1][1];
    const double syz = covariance[1][2];
    const double szx = covariance[2][0];
    const double szy = covariance[2][1];
    const double szz = covariance[2][2];
    double eigen[4][4] = {
        {sxx + syy + szz, syz - szy, szx - sxz, sxy - syx},
        {syz - szy, sxx - syy - szz, sxy + syx, szx + sxz},
        {szx - sxz, sxy + syx, -sxx + syy - szz, syz + szy},
        {sxy - syx, szx + sxz, syz + szy, -sxx - syy + szz},
    };
    double vectors[4][4]{};
    for (int index = 0; index < 4; ++index)
        vectors[index][index] = 1.0;
    // Symmetric Jacobi eigensolve avoids the largest-magnitude ambiguity of
    // power iteration.  The largest algebraic eigenvector is Horn's
    // least-squares proper rotation.
    for (int sweep = 0; sweep < 48; ++sweep) {
        int p = 0;
        int q = 1;
        double largest = std::fabs(eigen[p][q]);
        for (int row = 0; row < 4; ++row) {
            for (int column = row + 1; column < 4; ++column) {
                const double candidate = std::fabs(eigen[row][column]);
                if (candidate > largest) {
                    largest = candidate;
                    p = row;
                    q = column;
                }
            }
        }
        if (largest <= 1.0e-9 * std::max(1.0, radius_squared))
            break;
        const double app = eigen[p][p];
        const double aqq = eigen[q][q];
        const double apq = eigen[p][q];
        const double angle = 0.5 * std::atan2(2.0 * apq, aqq - app);
        const double cosine = std::cos(angle);
        const double sine = std::sin(angle);
        for (int index = 0; index < 4; ++index) {
            if (index == p || index == q)
                continue;
            const double aip = eigen[index][p];
            const double aiq = eigen[index][q];
            eigen[index][p] = eigen[p][index] =
                cosine * aip - sine * aiq;
            eigen[index][q] = eigen[q][index] =
                sine * aip + cosine * aiq;
        }
        eigen[p][p] =
            cosine * cosine * app - 2.0 * sine * cosine * apq +
            sine * sine * aqq;
        eigen[q][q] =
            sine * sine * app + 2.0 * sine * cosine * apq +
            cosine * cosine * aqq;
        eigen[p][q] = eigen[q][p] = 0.0;
        for (int row = 0; row < 4; ++row) {
            const double vip = vectors[row][p];
            const double viq = vectors[row][q];
            vectors[row][p] = cosine * vip - sine * viq;
            vectors[row][q] = sine * vip + cosine * viq;
        }
    }
    int largest_index = 0;
    for (int index = 1; index < 4; ++index) {
        if (eigen[index][index] > eigen[largest_index][largest_index])
            largest_index = index;
    }
    result.rotation = normalized(Quaternion{
        vectors[0][largest_index],
        vectors[1][largest_index],
        vectors[2][largest_index],
        vectors[3][largest_index],
    });

    double scale_numerator = 0.0;
    double scale_denominator = 0.0;
    for (const auto& entry : previous.vertices) {
        const auto found = current.vertices.find(entry.first);
        if (found == current.vertices.end())
            continue;
        const Vector3 source = view_position(*entry.second);
        const Vector3 target_position = view_position(*found->second);
        if (
            !std::isfinite(source.x) || !std::isfinite(source.y) ||
            !std::isfinite(source.z) ||
            !std::isfinite(target_position.x) ||
            !std::isfinite(target_position.y) ||
            !std::isfinite(target_position.z)
        )
            continue;
        const Vector3 rotated = rotate(
            result.rotation,
            subtract(source, result.previous_centroid));
        const Vector3 target = subtract(
            target_position, result.current_centroid);
        scale_numerator +=
            rotated.x * target.x + rotated.y * target.y +
            rotated.z * target.z;
        scale_denominator +=
            rotated.x * rotated.x + rotated.y * rotated.y +
            rotated.z * rotated.z;
    }
    if (scale_denominator > 1.0e-9) {
        const double fitted_scale = scale_numerator / scale_denominator;
        if (std::isfinite(fitted_scale) &&
            fitted_scale >= 0.5 && fitted_scale <= 2.0)
            result.scale = fitted_scale;
    }

    double residual_squared = 0.0;
    for (const auto& entry : previous.vertices) {
        const auto found = current.vertices.find(entry.first);
        if (found == current.vertices.end())
            continue;
        const Vector3 source = view_position(*entry.second);
        const Vector3 target_position = view_position(*found->second);
        if (
            !std::isfinite(source.x) || !std::isfinite(source.y) ||
            !std::isfinite(source.z) ||
            !std::isfinite(target_position.x) ||
            !std::isfinite(target_position.y) ||
            !std::isfinite(target_position.z)
        )
            continue;
        const Vector3 predicted = rotate(
            result.rotation,
            subtract(source, result.previous_centroid));
        const Vector3 target = subtract(
            target_position, result.current_centroid);
        const Vector3 error{
            predicted.x * result.scale - target.x,
            predicted.y * result.scale - target.y,
            predicted.z * result.scale - target.z,
        };
        residual_squared +=
            error.x * error.x + error.y * error.y + error.z * error.z;
    }
    const double rms = std::sqrt(residual_squared * inverse_count);
    result.rms = rms;
    // Captured GTE coordinates carry integer quantization.  A genuine rigid
    // group fits far below two percent of its radius; articulated/deforming
    // or incorrectly paired groups fall back to provenance vertex motion.
    const double relative_tolerance =
        previous.identity.category.object_kind == 2 ? 0.08 : 0.02;
    result.valid = std::isfinite(rms) &&
        rms <= std::max(2.0, radius * relative_tolerance);
    return result;
}

void prepare_rigid_transform_sample(
    RigidTransform* transform,
    float alpha
) noexcept {
    if (transform == nullptr || !transform->valid)
        return;
    transform->sample_rotation = fractional_rotation(
        transform->rotation, alpha);
    if (transform->exact) {
        transform->sample_rotation = multiply(
            transform->sample_rotation,
            transform->previous_rotation);
        transform->sample_matrix = multiply(
            rotation_matrix(transform->sample_rotation),
            interpolate(
                transform->previous_stretch,
                transform->current_stretch,
                alpha));
        transform->sample_translation = Vector3{
            interpolate(
                static_cast<float>(transform->previous_translation.x),
                static_cast<float>(transform->current_translation.x),
                alpha),
            interpolate(
                static_cast<float>(transform->previous_translation.y),
                static_cast<float>(transform->current_translation.y),
                alpha),
            interpolate(
                static_cast<float>(transform->previous_translation.z),
                static_cast<float>(transform->current_translation.z),
                alpha),
        };
    }
    transform->sample_scale = interpolate(
        1.0F,
        static_cast<float>(transform->scale),
        alpha);
}

void project_interpolated_vertex(
    const WorldDrawList& list,
    WorldDrawVertex* destination
) noexcept {
    constexpr float fixed_scale = 1.0F / 65536.0F;
    constexpr float near_plane = 16.0F;
    constexpr float far_plane = 1048576.0F;
    constexpr float depth_a = far_plane / (far_plane - near_plane);
    constexpr float depth_b =
        -near_plane * far_plane / (far_plane - near_plane);
    destination->screen_x =
        destination->draw_offset_x +
        destination->projection_offset_x * fixed_scale +
        destination->projection_plane * destination->view_x /
            destination->view_z;
    destination->screen_y =
        destination->draw_offset_y +
        destination->projection_offset_y * fixed_scale +
        destination->projection_plane * destination->view_y /
            destination->view_z;
    destination->clip_w = std::max(1.0F, destination->view_z);
    destination->clip_z = depth_a * destination->clip_w + depth_b;
    const float ndc_x =
        ((destination->screen_x - list.display_x) /
            static_cast<float>(list.display_width)) * 2.0F - 1.0F;
    const float ndc_y =
        1.0F - ((destination->screen_y - list.display_y) /
            static_cast<float>(list.display_height)) * 2.0F;
    destination->clip_x = ndc_x * destination->clip_w;
    destination->clip_y = ndc_y * destination->clip_w;
}

void interpolate_vertex(
    const WorldDrawList& previous_list,
    const WorldDrawList& current_list,
    const WorldDrawVertex& current,
    float alpha,
    WorldDrawVertex* destination
) noexcept {
    destination->world_x = interpolate(
        destination->world_x, current.world_x, alpha);
    destination->world_y = interpolate(
        destination->world_y, current.world_y, alpha);
    destination->world_z = interpolate(
        destination->world_z, current.world_z, alpha);
    destination->view_x = interpolate(
        destination->view_x, current.view_x, alpha);
    destination->view_y = interpolate(
        destination->view_y, current.view_y, alpha);
    destination->view_z = interpolate(
        destination->view_z, current.view_z, alpha);
    destination->projection_offset_x = interpolate(
        destination->projection_offset_x,
        current.projection_offset_x,
        alpha);
    destination->projection_offset_y = interpolate(
        destination->projection_offset_y,
        current.projection_offset_y,
        alpha);
    destination->projection_plane = interpolate(
        destination->projection_plane,
        current.projection_plane,
        alpha);
    destination->draw_offset_x = interpolate(
        destination->draw_offset_x,
        current.draw_offset_x - current_list.display_x +
            previous_list.display_x,
        alpha);
    destination->draw_offset_y = interpolate(
        destination->draw_offset_y,
        current.draw_offset_y - current_list.display_y +
            previous_list.display_y,
        alpha);
    const float normalized_current_x =
        current.screen_x - current_list.display_x +
        previous_list.display_x;
    const float normalized_current_y =
        current.screen_y - current_list.display_y +
        previous_list.display_y;
    destination->screen_x = interpolate(
        destination->screen_x, normalized_current_x, alpha);
    destination->screen_y = interpolate(
        destination->screen_y, normalized_current_y, alpha);
    destination->clip_w = interpolate(
        destination->clip_w, current.clip_w, alpha);
    destination->clip_z = interpolate(
        destination->clip_z, current.clip_z, alpha);
    const float ndc_x =
        ((destination->screen_x - previous_list.display_x) /
            static_cast<float>(previous_list.display_width)) * 2.0F - 1.0F;
    const float ndc_y =
        1.0F -
        ((destination->screen_y - previous_list.display_y) /
            static_cast<float>(previous_list.display_height)) * 2.0F;
    destination->clip_x = ndc_x * destination->clip_w;
    destination->clip_y = ndc_y * destination->clip_w;
}

void interpolate_projected_vertex(
    const WorldDrawList& previous_list,
    const WorldDrawList& current_list,
    const WorldDrawVertex& current,
    float alpha,
    WorldDrawVertex* destination
) noexcept {
    destination->world_x = interpolate(
        destination->world_x, current.world_x, alpha);
    destination->world_y = interpolate(
        destination->world_y, current.world_y, alpha);
    destination->world_z = interpolate(
        destination->world_z, current.world_z, alpha);
    destination->view_x = interpolate(
        destination->view_x, current.view_x, alpha);
    destination->view_y = interpolate(
        destination->view_y, current.view_y, alpha);
    destination->view_z = interpolate(
        destination->view_z, current.view_z, alpha);
    destination->exact_view_x = static_cast<std::int32_t>(
        std::lround(destination->view_x));
    destination->exact_view_y = static_cast<std::int32_t>(
        std::lround(destination->view_y));
    destination->exact_view_z = static_cast<std::int32_t>(
        std::lround(destination->view_z));
    destination->projection_offset_x = interpolate(
        destination->projection_offset_x,
        current.projection_offset_x,
        alpha);
    destination->projection_offset_y = interpolate(
        destination->projection_offset_y,
        current.projection_offset_y,
        alpha);
    destination->projection_plane = interpolate(
        destination->projection_plane,
        current.projection_plane,
        alpha);
    destination->draw_offset_x = interpolate(
        destination->draw_offset_x,
        current.draw_offset_x - current_list.display_x +
            previous_list.display_x,
        alpha);
    destination->draw_offset_y = interpolate(
        destination->draw_offset_y,
        current.draw_offset_y - current_list.display_y +
            previous_list.display_y,
        alpha);
    if (
        destination->view_z > 0.0F &&
        destination->projection_plane > 0.0F &&
        std::isfinite(destination->projection_plane)
    ) {
        project_interpolated_vertex(previous_list, destination);
        return;
    }
    interpolate_vertex(previous_list, current_list, current, alpha, destination);
}

bool interpolate_projected_command(
    const WorldDrawList& previous,
    const WorldDrawList& current,
    const WorldDrawCommand& previous_command,
    const ProjectedCommandIndex& current_projected_commands,
    std::vector<bool>* current_projected_used,
    float alpha,
    WorldDrawCommand* output
) noexcept {
    if (
        current_projected_used == nullptr ||
        output == nullptr ||
        !unowned_projected_command(previous, previous_command)
    )
        return false;
    const auto found = current_projected_commands.find(ProjectedCommandKey{
        previous_command.source_command_index,
        screen_command_key(previous, previous_command),
    });
    if (found == current_projected_commands.end())
        return false;

    constexpr double maximum_centroid_motion_squared = 96.0 * 96.0;
    std::size_t best = current.commands.size();
    double best_cost = maximum_centroid_motion_squared;
    for (const std::size_t current_index : found->second) {
        if (
            current_index >= current.commands.size() ||
            (*current_projected_used)[current_index]
        )
            continue;
        const double cost = screen_centroid_distance_squared(
            previous_command,
            current.commands[current_index]);
        if (cost < best_cost) {
            best = current_index;
            best_cost = cost;
        }
    }
    if (best >= current.commands.size())
        return false;

    const auto& current_command = current.commands[best];
    for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
        interpolate_projected_vertex(
            previous,
            current,
            current_command.vertices[vertex_index],
            alpha,
            &output->vertices[vertex_index]);
    }
    (*current_projected_used)[best] = true;
    output->exact_transform_valid = false;
    return true;
}

bool interpolate_screen_offset_anchor_command(
    const WorldDrawList& previous,
    const WorldDrawList& current,
    const WorldDrawCommand& previous_command,
    std::vector<bool>* current_used,
    float alpha,
    WorldDrawCommand* output
) noexcept {
    if (
        current_used == nullptr ||
        output == nullptr ||
        !screen_offset_anchor_command(previous_command)
    ) {
        return false;
    }
    const ScreenCommandKey material_key =
        screen_command_key(previous, previous_command);
    constexpr double maximum_centroid_motion_squared = 96.0 * 96.0;
    std::size_t best = current.commands.size();
    double best_cost = maximum_centroid_motion_squared;
    for (std::size_t index = 0; index < current.commands.size(); ++index) {
        if ((*current_used)[index])
            continue;
        const auto& candidate = current.commands[index];
        if (
            !screen_offset_anchor_command(candidate) ||
            candidate.object_kind != previous_command.object_kind ||
            candidate.object_id != previous_command.object_id ||
            candidate.model_pointer != previous_command.model_pointer ||
            candidate.channel != previous_command.channel ||
            !(screen_command_key(current, candidate) == material_key)
        ) {
            continue;
        }
        bool same_sources = true;
        for (int vertex = 0; vertex < 3; ++vertex) {
            same_sources = same_sources &&
                temporal_vertex_key(candidate.vertices[vertex]) ==
                    temporal_vertex_key(previous_command.vertices[vertex]);
        }
        if (!same_sources)
            continue;
        const double cost = screen_centroid_distance_squared(
            previous_command, candidate);
        if (cost < best_cost) {
            best = index;
            best_cost = cost;
        }
    }
    if (best >= current.commands.size())
        return false;
    const auto& current_command = current.commands[best];
    for (int vertex = 0; vertex < 3; ++vertex) {
        interpolate_vertex(
            previous,
            current,
            current_command.vertices[vertex],
            alpha,
            &output->vertices[vertex]);
    }
    output->exact_transform_valid = false;
    (*current_used)[best] = true;
    return true;
}

void interpolate_rigid_vertex(
    const WorldDrawList& previous_list,
    const WorldDrawList& current_list,
    const WorldDrawVertex& current,
    const RigidTransform& transform,
    float alpha,
    WorldDrawVertex* destination
) noexcept {
    destination->world_x = interpolate(
        destination->world_x, current.world_x, alpha);
    destination->world_y = interpolate(
        destination->world_y, current.world_y, alpha);
    destination->world_z = interpolate(
        destination->world_z, current.world_z, alpha);
    Vector3 rotated{};
    if (transform.exact) {
        rotated = multiply(
            transform.sample_matrix,
            Vector3{
                static_cast<double>(destination->model_x),
                static_cast<double>(destination->model_y),
                static_cast<double>(destination->model_z),
            });
        destination->view_x = static_cast<float>(
            rotated.x + transform.sample_translation.x);
        destination->view_y = static_cast<float>(
            rotated.y + transform.sample_translation.y);
        destination->view_z = static_cast<float>(
            rotated.z + transform.sample_translation.z);
    } else {
        const Vector3 relative = subtract(
            view_position(*destination), transform.previous_centroid);
        rotated = rotate(transform.sample_rotation, relative);
        rotated.x *= transform.sample_scale;
        rotated.y *= transform.sample_scale;
        rotated.z *= transform.sample_scale;
        destination->view_x = static_cast<float>(
            rotated.x + interpolate(
                static_cast<float>(transform.previous_centroid.x),
                static_cast<float>(transform.current_centroid.x), alpha));
        destination->view_y = static_cast<float>(
            rotated.y + interpolate(
                static_cast<float>(transform.previous_centroid.y),
                static_cast<float>(transform.current_centroid.y), alpha));
        destination->view_z = static_cast<float>(
            rotated.z + interpolate(
                static_cast<float>(transform.previous_centroid.z),
                static_cast<float>(transform.current_centroid.z), alpha));
    }
    destination->exact_view_x = static_cast<std::int32_t>(
        std::lround(destination->view_x));
    destination->exact_view_y = static_cast<std::int32_t>(
        std::lround(destination->view_y));
    destination->exact_view_z = static_cast<std::int32_t>(
        std::lround(destination->view_z));
    destination->projection_offset_x = interpolate(
        destination->projection_offset_x,
        current.projection_offset_x,
        alpha);
    destination->projection_offset_y = interpolate(
        destination->projection_offset_y,
        current.projection_offset_y,
        alpha);
    destination->projection_plane = interpolate(
        destination->projection_plane,
        current.projection_plane,
        alpha);
    destination->draw_offset_x = interpolate(
        destination->draw_offset_x,
        current.draw_offset_x - current_list.display_x +
            previous_list.display_x,
        alpha);
    destination->draw_offset_y = interpolate(
        destination->draw_offset_y,
        current.draw_offset_y - current_list.display_y +
            previous_list.display_y,
        alpha);
    if (
        destination->view_z > 0.0F &&
        destination->projection_plane > 0.0F &&
        std::isfinite(destination->projection_plane)
    ) {
        project_interpolated_vertex(previous_list, destination);
        return;
    }
    // Legacy/synthetic tests may not carry projection state.  Preserve their
    // prior screen-space behavior while still using rigid 3D motion when the
    // production capture data is present.
    const float normalized_current_x =
        current.screen_x - current_list.display_x +
        previous_list.display_x;
    const float normalized_current_y =
        current.screen_y - current_list.display_y +
        previous_list.display_y;
    destination->screen_x = interpolate(
        destination->screen_x, normalized_current_x, alpha);
    destination->screen_y = interpolate(
        destination->screen_y, normalized_current_y, alpha);
    destination->clip_w = interpolate(
        destination->clip_w, current.clip_w, alpha);
    destination->clip_z = interpolate(
        destination->clip_z, current.clip_z, alpha);
    const float ndc_x =
        ((destination->screen_x - previous_list.display_x) /
            static_cast<float>(previous_list.display_width)) * 2.0F - 1.0F;
    const float ndc_y =
        1.0F - ((destination->screen_y - previous_list.display_y) /
            static_cast<float>(previous_list.display_height)) * 2.0F;
    destination->clip_x = ndc_x * destination->clip_w;
    destination->clip_y = ndc_y * destination->clip_w;
}

enum class TrackMidpointSafety {
    safe,
    nonfinite,
    near_depth,
    shift,
    span,
};

struct TrackMidpointSafetyDetails {
    TrackMidpointSafety reason{TrackMidpointSafety::safe};
    float previous_span_x{};
    float previous_span_y{};
    float midpoint_span_x{};
    float midpoint_span_y{};
    float maximum_shift{};
    float minimum_depth{(std::numeric_limits<float>::max)()};
};

TrackMidpointSafetyDetails exact_track_midpoint_safety(
    const WorldDrawList& list,
    const WorldDrawCommand& previous,
    const WorldDrawCommand& midpoint
) noexcept {
    constexpr float protected_near_depth = 64.0F;
    const float maximum_shift = static_cast<float>(
        std::max(list.display_width, list.display_height));
    TrackMidpointSafetyDetails details{};
    float previous_minimum_x = (std::numeric_limits<float>::max)();
    float previous_minimum_y = (std::numeric_limits<float>::max)();
    float previous_maximum_x = (std::numeric_limits<float>::lowest)();
    float previous_maximum_y = (std::numeric_limits<float>::lowest)();
    float midpoint_minimum_x = (std::numeric_limits<float>::max)();
    float midpoint_minimum_y = (std::numeric_limits<float>::max)();
    float midpoint_maximum_x = (std::numeric_limits<float>::lowest)();
    float midpoint_maximum_y = (std::numeric_limits<float>::lowest)();
    for (int index = 0; index < 3; ++index) {
        const auto& source = previous.vertices[index];
        const auto& sample = midpoint.vertices[index];
        if (
            !std::isfinite(sample.screen_x) ||
            !std::isfinite(sample.screen_y) ||
            !std::isfinite(sample.view_z)
        ) {
            details.reason = TrackMidpointSafety::nonfinite;
            return details;
        }
        details.minimum_depth = std::min(
            details.minimum_depth, sample.view_z);
        details.maximum_shift = std::max(
            details.maximum_shift,
            std::max(
                std::fabs(sample.screen_x - source.screen_x),
                std::fabs(sample.screen_y - source.screen_y)));
        previous_minimum_x = std::min(previous_minimum_x, source.screen_x);
        previous_minimum_y = std::min(previous_minimum_y, source.screen_y);
        previous_maximum_x = std::max(previous_maximum_x, source.screen_x);
        previous_maximum_y = std::max(previous_maximum_y, source.screen_y);
        midpoint_minimum_x = std::min(midpoint_minimum_x, sample.screen_x);
        midpoint_minimum_y = std::min(midpoint_minimum_y, sample.screen_y);
        midpoint_maximum_x = std::max(midpoint_maximum_x, sample.screen_x);
        midpoint_maximum_y = std::max(midpoint_maximum_y, sample.screen_y);
        if (sample.view_z < protected_near_depth) {
            details.reason = TrackMidpointSafety::near_depth;
            return details;
        }
        if (
            std::fabs(sample.screen_x - source.screen_x) > maximum_shift ||
            std::fabs(sample.screen_y - source.screen_y) > maximum_shift
        ) {
            details.reason = TrackMidpointSafety::shift;
            return details;
        }
    }
    details.previous_span_x = previous_maximum_x - previous_minimum_x;
    details.previous_span_y = previous_maximum_y - previous_minimum_y;
    details.midpoint_span_x = midpoint_maximum_x - midpoint_minimum_x;
    details.midpoint_span_y = midpoint_maximum_y - midpoint_minimum_y;
    // Authored triangles can already span beyond two viewports when the PS1
    // submitted a clipped road or horizon polygon. Treat modest motion of
    // that existing footprint as safe while still rejecting a midpoint that
    // newly explodes from a normally bounded source triangle.
    constexpr float maximum_span_growth = 1.25F;
    const float allowed_span_x = std::max(
        list.display_width * 2.0F,
        details.previous_span_x * maximum_span_growth);
    const float allowed_span_y = std::max(
        list.display_height * 2.0F,
        details.previous_span_y * maximum_span_growth);
    if (
        details.midpoint_span_x <= allowed_span_x &&
        details.midpoint_span_y <= allowed_span_y
    ) {
        return details;
    }
    details.reason = TrackMidpointSafety::span;
    return details;
}

const WorldDrawVertex* find_temporal_vertex(
    const TransformGroup& group,
    const WorldDrawVertex& previous,
    bool require_coherent_transform
) noexcept {
    const auto found = group.temporal_vertices.find(
        temporal_vertex_key(previous));
    const WorldDrawVertex* best = nullptr;
    double best_distance = (std::numeric_limits<double>::max)();
    if (found != group.temporal_vertices.end()) {
        const auto consider = [&](const WorldDrawVertex* candidate) {
            if (
                candidate == nullptr ||
                (require_coherent_transform &&
                    !exact_transform_is_coherent(*candidate))
            ) {
                return;
            }
            const double dx = candidate->view_x - previous.view_x;
            const double dy = candidate->view_y - previous.view_y;
            const double dz = candidate->view_z - previous.view_z;
            const double distance = dx * dx + dy * dy + dz * dz;
            if (distance < best_distance) {
                best = candidate;
                best_distance = distance;
            }
        };
        consider(found->second.first);
        for (const WorldDrawVertex* candidate : found->second.overflow)
            consider(candidate);
    }
    if (best != nullptr)
        return best;
    const auto model_match = group.vertices.find(vertex_key(previous));
    if (
        model_match == group.vertices.end() ||
        (require_coherent_transform &&
            !exact_transform_is_coherent(*model_match->second))
    ) {
        return nullptr;
    }
    return model_match->second;
}

struct ExactVertexTransformMapping {
    const WorldDrawVertex* previous{};
    const WorldDrawVertex* current{};
    GroupCategory category{};
    RigidTransform transform{};
    bool valid{};
};

struct ExactVertexTransformKey {
    std::uint64_t transform_id{};
    WorldViewChannel channel{};

    bool operator==(const ExactVertexTransformKey& other) const noexcept {
        return transform_id == other.transform_id && channel == other.channel;
    }
};

struct ExactVertexTransformKeyHash {
    std::size_t operator()(const ExactVertexTransformKey& key) const noexcept {
        std::size_t result = static_cast<std::size_t>(key.transform_id);
        result ^= static_cast<std::size_t>(key.channel) +
            0x9E3779B97F4A7C15ULL + (result << 6U) + (result >> 2U);
        return result;
    }
};

using ExactVertexTransformMappings = std::unordered_map<
    ExactVertexTransformKey,
    ExactVertexTransformMapping,
    ExactVertexTransformKeyHash>;

enum class TrackCandidateResult {
    unavailable,
    unsafe,
    success,
};

struct TrackCandidateDiagnostics {
    std::uint32_t ambiguous_mappings{};
    std::uint32_t missing_candidates{};
    std::uint64_t first_missing_transform_id{};
    std::uint32_t first_missing_source_identity{};
    TrackMidpointSafety unsafe_reason{TrackMidpointSafety::safe};
    TrackMidpointSafetyDetails unsafe_details{};
    std::size_t unsafe_command_index{(std::numeric_limits<std::size_t>::max)()};
};

ExactVertexTransformMappings build_track_transform_mappings(
    const std::vector<TransformGroup>& previous_groups,
    const std::vector<TransformGroup>& current_groups,
    float alpha,
    MergedCategoryCache* current_merged_categories,
    TrackCandidateDiagnostics* diagnostics
) {
    ExactVertexTransformMappings mappings;
    for (const auto& previous_group : previous_groups) {
        if (previous_group.identity.category.object_kind != 1)
            continue;
        const TransformGroup& current_category = merged_category_vertices(
            current_groups,
            previous_group.identity.category,
            current_merged_categories);
        for (const auto& entry : previous_group.temporal_vertices) {
            const auto map_vertex = [&](const WorldDrawVertex* vertex) {
                if (
                    vertex == nullptr ||
                    !exact_transform_is_coherent(*vertex)
                ) {
                    return;
                }
                const WorldDrawVertex* current = find_temporal_vertex(
                    current_category, *vertex, true);
                if (current == nullptr)
                    return;
                const auto inserted = mappings.emplace(
                    ExactVertexTransformKey{
                        vertex->transform_id,
                        previous_group.identity.category.channel,
                    },
                    ExactVertexTransformMapping{});
                auto& mapping = inserted.first->second;
                if (inserted.second) {
                    mapping.previous = vertex;
                    mapping.current = current;
                    mapping.category = previous_group.identity.category;
                    mapping.transform = exact_rigid_transform(
                        vertex->transform_rotation,
                        vertex->transform_translation,
                        current->transform_rotation,
                        current->transform_translation);
                    prepare_rigid_transform_sample(&mapping.transform, alpha);
                    mapping.valid = true;
                } else if (
                    !same_exact_transform(*mapping.previous, *vertex) ||
                    !same_exact_transform(*mapping.current, *current)
                ) {
                    mapping.valid = false;
                    if (diagnostics != nullptr)
                        ++diagnostics->ambiguous_mappings;
                }
            };
            map_vertex(entry.second.first);
            for (const WorldDrawVertex* vertex : entry.second.overflow)
                map_vertex(vertex);
        }
    }
    return mappings;
}

struct TrackViewDelta {
    bool valid{};
    WorldViewChannel channel{};
    double source_scale{};
    Matrix3 matrix{};
    Vector3 translation{};
    RigidTransform midpoint{};
};

std::vector<TrackViewDelta> build_track_view_deltas(
    const ExactVertexTransformMappings& mappings,
    float alpha
) noexcept {
    const bool diagnostics =
        std::getenv("OPENGT_INTERPOLATION_VIEW_DELTA_DIAGNOSTICS") != nullptr;
    struct Candidate {
        WorldViewChannel channel{};
        Matrix3 matrix{};
        Vector3 translation{};
        double source_scale{};
    };
    std::vector<Candidate> candidates;
    for (const auto& entry : mappings) {
        const auto& mapping = entry.second;
        if (
            !mapping.valid ||
            mapping.previous == nullptr ||
            mapping.current == nullptr ||
            same_exact_transform(*mapping.previous, *mapping.current)
        ) {
            continue;
        }
        const Matrix3 previous_matrix = fixed_matrix(
            mapping.previous->transform_rotation);
        const Matrix3 current_matrix = fixed_matrix(
            mapping.current->transform_rotation);
        Matrix3 previous_inverse{};
        if (!inverse(previous_matrix, &previous_inverse))
            continue;
        Candidate candidate{};
        candidate.channel = mapping.category.channel;
        candidate.matrix = multiply(current_matrix, previous_inverse);
        const Vector3 previous_translation{
            static_cast<double>(mapping.previous->transform_translation[0]),
            static_cast<double>(mapping.previous->transform_translation[1]),
            static_cast<double>(mapping.previous->transform_translation[2]),
        };
        const Vector3 current_translation{
            static_cast<double>(mapping.current->transform_translation[0]),
            static_cast<double>(mapping.current->transform_translation[1]),
            static_cast<double>(mapping.current->transform_translation[2]),
        };
        const Vector3 moved_previous = multiply(
            candidate.matrix, previous_translation);
        candidate.translation = Vector3{
            current_translation.x - moved_previous.x,
            current_translation.y - moved_previous.y,
            current_translation.z - moved_previous.z,
        };
        candidate.source_scale = std::cbrt(std::fabs(
            determinant(previous_matrix)));
        const double relative_scale = std::cbrt(std::fabs(
            determinant(candidate.matrix)));
        if (relative_scale < 0.9 || relative_scale > 1.1) {
            if (diagnostics) {
                std::fprintf(
                    stderr,
                    "[Interpolation-View-Delta-Scale-Transition] "
                    "transform=%016llx channel=%u relativeScale=%.9f\n",
                    static_cast<unsigned long long>(
                        entry.first.transform_id),
                    static_cast<unsigned>(candidate.channel),
                    relative_scale);
            }
            continue;
        }
        candidates.push_back(candidate);
        if (diagnostics) {
            std::fprintf(
                stderr,
                "[Interpolation-View-Delta-Candidate] index=%zu "
                "transform=%016llx object=%u model=%08x channel=%u "
                "scale=%.9f translation=%.6f,%.6f,%.6f "
                "matrix=%.6f,%.6f,%.6f,%.6f,%.6f,%.6f,%.6f,%.6f,%.6f\n",
                candidates.size() - 1,
                static_cast<unsigned long long>(entry.first.transform_id),
                mapping.category.object_id,
                mapping.category.model_pointer,
                static_cast<unsigned>(mapping.category.channel),
                candidate.source_scale,
                candidate.translation.x,
                candidate.translation.y,
                candidate.translation.z,
                candidate.matrix.value[0],
                candidate.matrix.value[1],
                candidate.matrix.value[2],
                candidate.matrix.value[3],
                candidate.matrix.value[4],
                candidate.matrix.value[5],
                candidate.matrix.value[6],
                candidate.matrix.value[7],
                candidate.matrix.value[8]);
        }
    }
    if (candidates.empty())
        return {};
    constexpr double maximum_matrix_difference = 0.01;
    constexpr double maximum_translation_difference = 8.0;
    std::vector<TrackViewDelta> results;
    std::vector<bool> consumed(candidates.size(), false);
    for (std::size_t seed = 0; seed < candidates.size(); ++seed) {
        if (consumed[seed])
            continue;
        const Candidate& reference = candidates[seed];
        bool consistent = true;
        for (std::size_t index = seed; index < candidates.size(); ++index) {
            if (consumed[index])
                continue;
            if (candidates[index].channel != reference.channel)
                continue;
            const double scale_ratio = candidates[index].source_scale /
                reference.source_scale;
            if (scale_ratio < 0.9 || scale_ratio > 1.1)
                continue;
            consumed[index] = true;
            for (int component = 0; component < 9; ++component) {
                if (std::fabs(
                        candidates[index].matrix.value[component] -
                        reference.matrix.value[component]) >
                    maximum_matrix_difference) {
                    consistent = false;
                }
            }
            if (
                std::fabs(
                    candidates[index].translation.x -
                    reference.translation.x) >
                        maximum_translation_difference ||
                std::fabs(
                    candidates[index].translation.y -
                    reference.translation.y) >
                        maximum_translation_difference ||
                std::fabs(
                    candidates[index].translation.z -
                    reference.translation.z) >
                        maximum_translation_difference
            ) {
                consistent = false;
            }
        }
        if (!consistent)
        {
            if (diagnostics) {
                std::fprintf(
                    stderr,
                    "[Interpolation-View-Delta-Rejected] seed=%zu scale=%.9f\n",
                    seed,
                    reference.source_scale);
            }
            continue;
        }
        TrackViewDelta result{};
        result.valid = true;
        result.channel = reference.channel;
        result.source_scale = reference.source_scale;
        result.matrix = reference.matrix;
        result.translation = reference.translation;
        result.midpoint.valid = true;
        result.midpoint.exact = true;
        result.midpoint.previous_rotation = Quaternion{};
        result.midpoint.rotation = quaternion_from_matrix(
            nearest_rotation(result.matrix));
        result.midpoint.previous_stretch = Matrix3{{
            1.0, 0.0, 0.0,
            0.0, 1.0, 0.0,
            0.0, 0.0, 1.0,
        }};
        const Matrix3 current_rotation = nearest_rotation(result.matrix);
        result.midpoint.current_stretch = multiply(
            transpose(current_rotation), result.matrix);
        result.midpoint.current_translation = result.translation;
        prepare_rigid_transform_sample(&result.midpoint, alpha);
        results.push_back(result);
        if (diagnostics) {
            std::fprintf(
                stderr,
                "[Interpolation-View-Delta-Accepted] seed=%zu scale=%.9f\n",
                seed,
                reference.source_scale);
        }
    }
    return results;
}

const TrackViewDelta* find_track_view_delta(
    const std::vector<TrackViewDelta>& deltas,
    const WorldDrawVertex& vertex,
    WorldViewChannel channel
) noexcept {
    const double scale = std::cbrt(std::fabs(
        determinant(fixed_matrix(vertex.transform_rotation))));
    const TrackViewDelta* best = nullptr;
    double best_ratio_error = (std::numeric_limits<double>::max)();
    for (const auto& delta : deltas) {
        if (
            !delta.valid ||
            delta.channel != channel ||
            delta.source_scale <= 1.0e-9
        )
            continue;
        const double ratio = scale / delta.source_scale;
        const double error = std::fabs(std::log(ratio));
        if (error < best_ratio_error) {
            best = &delta;
            best_ratio_error = error;
        }
    }
    return best;
}

void interpolate_view_delta_vertex(
    const WorldDrawList& previous_list,
    const WorldDrawList& current_list,
    const WorldDrawVertex& current_projection,
    const TrackViewDelta& delta,
    float alpha,
    WorldDrawVertex* destination
) noexcept {
    const Vector3 moved = multiply(
        delta.midpoint.sample_matrix,
        view_position(*destination));
    const double destination_scale = std::cbrt(std::fabs(
        determinant(fixed_matrix(destination->transform_rotation))));
    const double translation_scale = delta.source_scale > 1.0e-9
        ? destination_scale / delta.source_scale
        : 1.0;
    destination->view_x = static_cast<float>(
        moved.x + delta.midpoint.sample_translation.x * translation_scale);
    destination->view_y = static_cast<float>(
        moved.y + delta.midpoint.sample_translation.y * translation_scale);
    destination->view_z = static_cast<float>(
        moved.z + delta.midpoint.sample_translation.z * translation_scale);
    destination->exact_view_x = static_cast<std::int32_t>(
        std::lround(destination->view_x));
    destination->exact_view_y = static_cast<std::int32_t>(
        std::lround(destination->view_y));
    destination->exact_view_z = static_cast<std::int32_t>(
        std::lround(destination->view_z));
    destination->projection_offset_x = interpolate(
        destination->projection_offset_x,
        current_projection.projection_offset_x,
        alpha);
    destination->projection_offset_y = interpolate(
        destination->projection_offset_y,
        current_projection.projection_offset_y,
        alpha);
    destination->projection_plane = interpolate(
        destination->projection_plane,
        current_projection.projection_plane,
        alpha);
    destination->draw_offset_x = interpolate(
        destination->draw_offset_x,
        current_projection.draw_offset_x - current_list.display_x +
            previous_list.display_x,
        alpha);
    destination->draw_offset_y = interpolate(
        destination->draw_offset_y,
        current_projection.draw_offset_y - current_list.display_y +
            previous_list.display_y,
        alpha);
    destination->exact_transform_valid = false;
    if (
        destination->view_z > 0.0F &&
        destination->projection_plane > 0.0F &&
        std::isfinite(destination->projection_plane)
    ) {
        project_interpolated_vertex(previous_list, destination);
    }
}

TrackCandidateResult build_track_visibility_candidates(
    const WorldDrawList& previous_list,
    const WorldDrawList& current_list,
    const TransformGroup& previous_group,
    const TransformGroup& current_group,
    const ExactVertexTransformMappings& mappings,
    const std::vector<TrackViewDelta>& view_deltas,
    float alpha,
    std::vector<WorldDrawCommand>* candidates,
    std::vector<std::size_t>* candidate_indices,
    TrackCandidateDiagnostics* diagnostics
) {
    if (
        candidates == nullptr ||
        candidate_indices == nullptr ||
        current_group.vertices.empty()
    ) {
        return TrackCandidateResult::unavailable;
    }
    const WorldDrawVertex& projection_sample =
        *current_group.vertices.begin()->second;
    std::vector<std::pair<std::size_t, WorldDrawCommand>> pending;
    pending.reserve(previous_group.commands.size());
    for (const std::size_t command_index : previous_group.commands) {
        const auto& previous_command = previous_list.commands[command_index];
        WorldDrawCommand candidate = previous_command;
        const char* candidate_methods[3]{"none", "none", "none"};
        for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
            const auto& previous_vertex =
                previous_command.vertices[vertex_index];
            const WorldDrawVertex* current_vertex = find_temporal_vertex(
                current_group, previous_vertex, false);
            const auto mapping = mappings.find(ExactVertexTransformKey{
                previous_vertex.transform_id,
                previous_group.identity.category.channel,
            });
            if (
                exact_transform_is_coherent(previous_vertex) &&
                mapping != mappings.end() &&
                mapping->second.valid
            ) {
                interpolate_rigid_vertex(
                    previous_list,
                    current_list,
                    current_vertex != nullptr
                        ? *current_vertex
                        : projection_sample,
                    mapping->second.transform,
                    alpha,
                    &candidate.vertices[vertex_index]);
                store_exact_sample_transform(
                    mapping->second.transform,
                    &candidate.vertices[vertex_index]);
                candidate_methods[vertex_index] = "mapping";
            } else if (current_vertex != nullptr) {
                interpolate_vertex(
                    previous_list,
                    current_list,
                    *current_vertex,
                    alpha,
                    &candidate.vertices[vertex_index]);
                candidate.vertices[vertex_index].exact_transform_valid = false;
                candidate_methods[vertex_index] = "current";
            } else if (exact_transform_is_coherent(previous_vertex)) {
                const TrackViewDelta* view_delta = find_track_view_delta(
                    view_deltas,
                    previous_vertex,
                    previous_group.identity.category.channel);
                if (view_delta == nullptr) {
                    if (
                        std::getenv(
                            "OPENGT_INTERPOLATION_MISSING_DIAGNOSTICS") !=
                            nullptr &&
                        diagnostics != nullptr &&
                        diagnostics->first_missing_transform_id == 0
                    ) {
                        const double source_scale = std::cbrt(std::fabs(
                            determinant(fixed_matrix(
                                previous_vertex.transform_rotation))));
                        std::fprintf(
                            stderr,
                            "[Interpolation-Missing] transform=%016llx "
                            "source=%08x object=%u model=%08x "
                            "currentVertex=%u mapping=%u valid=%u "
                            "sourceScale=%.9f mappings=%zu deltas=%zu",
                            static_cast<unsigned long long>(
                                previous_vertex.transform_id),
                            previous_vertex.source_vertex_identity,
                            previous_group.identity.category.object_id,
                            previous_group.identity.category.model_pointer,
                            current_vertex != nullptr ? 1U : 0U,
                            mapping != mappings.end() ? 1U : 0U,
                            mapping != mappings.end() &&
                                mapping->second.valid ? 1U : 0U,
                            source_scale,
                            mappings.size(),
                            view_deltas.size());
                        for (const auto& candidate_delta : view_deltas) {
                            std::fprintf(
                                stderr,
                                " %.9f",
                                candidate_delta.source_scale);
                        }
                        std::fprintf(stderr, "\n");
                    }
                    if (diagnostics != nullptr) {
                        ++diagnostics->missing_candidates;
                        if (diagnostics->first_missing_transform_id == 0) {
                            diagnostics->first_missing_transform_id =
                                previous_vertex.transform_id;
                            diagnostics->first_missing_source_identity =
                                previous_vertex.source_vertex_identity;
                        }
                    }
                    return TrackCandidateResult::unavailable;
                }
                interpolate_view_delta_vertex(
                    previous_list,
                    current_list,
                    projection_sample,
                    *view_delta,
                    alpha,
                    &candidate.vertices[vertex_index]);
                candidate_methods[vertex_index] = "view-delta";
            } else {
                if (diagnostics != nullptr) {
                    ++diagnostics->missing_candidates;
                    if (diagnostics->first_missing_transform_id == 0) {
                        diagnostics->first_missing_transform_id =
                            previous_vertex.transform_id;
                        diagnostics->first_missing_source_identity =
                            previous_vertex.source_vertex_identity;
                    }
                }
                return TrackCandidateResult::unavailable;
            }
        }
        candidate.exact_transform_valid =
            candidate.vertices[0].transform_id ==
                candidate.vertices[1].transform_id &&
            candidate.vertices[0].transform_id ==
                candidate.vertices[2].transform_id &&
            same_exact_transform(
                candidate.vertices[0], candidate.vertices[1]) &&
            same_exact_transform(
                candidate.vertices[0], candidate.vertices[2]);
        if (candidate.exact_transform_valid) {
            candidate.transform_id = candidate.vertices[0].transform_id;
            for (int component = 0; component < 9; ++component) {
                candidate.transform_rotation[component] =
                    candidate.vertices[0].transform_rotation[component];
            }
            for (int component = 0; component < 3; ++component) {
                candidate.transform_translation[component] =
                    candidate.vertices[0].transform_translation[component];
            }
        }
        const TrackMidpointSafetyDetails safety = exact_track_midpoint_safety(
            previous_list, previous_command, candidate);
        if (safety.reason != TrackMidpointSafety::safe) {
            if (
                std::getenv(
                    "OPENGT_INTERPOLATION_UNSAFE_DIAGNOSTICS") != nullptr
            ) {
                std::fprintf(
                    stderr,
                    "[Interpolation-Unsafe] command=%zu object=%u "
                    "model=%08x transform=%016llx reason=%u "
                    "previousSpan=%.3f,%.3f midpointSpan=%.3f,%.3f "
                    "shift=%.3f depth=%.3f\n",
                    command_index,
                    previous_command.object_id,
                    previous_command.model_pointer,
                    static_cast<unsigned long long>(
                        previous_command.transform_id),
                    static_cast<unsigned>(safety.reason),
                    safety.previous_span_x,
                    safety.previous_span_y,
                    safety.midpoint_span_x,
                    safety.midpoint_span_y,
                    safety.maximum_shift,
                    safety.minimum_depth);
                for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
                    const auto& source =
                        previous_command.vertices[vertex_index];
                    const auto& midpoint_vertex =
                        candidate.vertices[vertex_index];
                    std::fprintf(
                        stderr,
                        "[Interpolation-Unsafe-Vertex] index=%d "
                        "method=%s source=%08x model=%d,%d,%d "
                        "previousView=%.3f,%.3f,%.3f "
                        "midpointView=%.3f,%.3f,%.3f "
                        "previousScreen=%.3f,%.3f "
                        "midpointScreen=%.3f,%.3f\n",
                        vertex_index,
                        candidate_methods[vertex_index],
                        source.source_vertex_identity,
                        source.model_x,
                        source.model_y,
                        source.model_z,
                        source.view_x,
                        source.view_y,
                        source.view_z,
                        midpoint_vertex.view_x,
                        midpoint_vertex.view_y,
                        midpoint_vertex.view_z,
                        source.screen_x,
                        source.screen_y,
                        midpoint_vertex.screen_x,
                        midpoint_vertex.screen_y);
                }
            }
            if (diagnostics != nullptr) {
                diagnostics->unsafe_reason = safety.reason;
                diagnostics->unsafe_details = safety;
                diagnostics->unsafe_command_index = command_index;
            }
            const bool invalid_geometry =
                safety.reason == TrackMidpointSafety::nonfinite ||
                safety.reason == TrackMidpointSafety::near_depth;
            // Large finite screen shifts and spans are normal for triangles
            // crossing the viewport boundary. These candidates are assembled
            // from exact vertex mappings/view deltas, so D3D clipping is the
            // correct operation; holding their entire connected track group
            // exposes the clear buffer at the moving near-road boundary.
            if (invalid_geometry) {
                return TrackCandidateResult::unsafe;
            }
        }
        pending.emplace_back(command_index, std::move(candidate));
    }
    for (auto& entry : pending) {
        (*candidate_indices)[entry.first] = candidates->size();
        candidates->push_back(std::move(entry.second));
    }
    return TrackCandidateResult::success;
}

} // namespace

WorldInterpolationCache::~WorldInterpolationCache() {
    reset();
}

WorldInterpolationCache::WorldInterpolationCache(
    WorldInterpolationCache&& other
) noexcept : storage_(other.storage_) {
    other.storage_ = nullptr;
}

WorldInterpolationCache& WorldInterpolationCache::operator=(
    WorldInterpolationCache&& other
) noexcept {
    if (this == &other)
        return *this;
    reset();
    storage_ = other.storage_;
    other.storage_ = nullptr;
    return *this;
}

void WorldInterpolationCache::reset() noexcept {
    delete static_cast<InterpolationCacheStorage*>(storage_);
    storage_ = nullptr;
}

WorldInterpolationResult interpolate_world_draw_lists_cached(
    const WorldDrawList& previous,
    const WorldDrawList& current,
    WorldInterpolationCache* previous_cache,
    WorldInterpolationCache* current_cache,
    float alpha,
    WorldDrawList* output,
    WorldInterpolationStats* output_stats
) noexcept {
    if (
        output == nullptr ||
        output_stats == nullptr ||
        !std::isfinite(alpha) ||
        alpha <= 0.0F ||
        alpha >= 1.0F
    )
        return WorldInterpolationResult::invalid_argument;
    if (
        previous.display_width != current.display_width ||
        previous.display_height != current.display_height
    )
        return WorldInterpolationResult::incompatible_viewport;
    try {
        using InterpolationClock = std::chrono::steady_clock;
        const auto interpolation_started = InterpolationClock::now();
        // This is a process-wide diagnostic override. Sampling it inside the
        // command loop adds thousands of serialized CRT environment queries
        // to every midpoint.
        const bool track_camera_delta_enabled =
            std::getenv("OPENGT_DISABLE_TRACK_CAMERA_DELTA") == nullptr;
        WorldInterpolationStats stats{};
        stats.previous_commands = static_cast<std::uint32_t>(
            previous.commands.size());
        stats.current_commands = static_cast<std::uint32_t>(
            current.commands.size());
        WorldDrawList midpoint = previous;
        const auto copy_finished = InterpolationClock::now();
        const auto indexed_groups = [](
            const WorldDrawList& list,
            WorldInterpolationCache* cache,
            std::uint32_t* cache_hit
        ) -> const std::vector<TransformGroup>& {
            auto* storage = static_cast<InterpolationCacheStorage*>(
                cache->storage_);
            if (storage != nullptr && storage->matches(list)) {
                *cache_hit = 1;
                return storage->groups;
            }
            delete storage;
            storage = new InterpolationCacheStorage{};
            cache->storage_ = storage;
            storage->groups = build_groups(list);
            storage->commands = list.commands.data();
            storage->command_count = list.commands.size();
            return storage->groups;
        };
        WorldInterpolationCache temporary_previous_cache;
        WorldInterpolationCache temporary_current_cache;
        if (previous_cache == nullptr)
            previous_cache = &temporary_previous_cache;
        if (current_cache == nullptr)
            current_cache = &temporary_current_cache;
        const auto& previous_groups = indexed_groups(
            previous, previous_cache, &stats.previous_group_cache_hit);
        const auto& current_groups = indexed_groups(
            current, current_cache, &stats.current_group_cache_hit);
        const auto group_matches = match_groups(
            previous_groups, current_groups);
        ScreenCommandIndex current_screen_commands;
        bool current_screen_commands_built = false;
        std::vector<bool> current_screen_used;
        std::vector<bool> current_screen_offset_anchor_used;
        ProjectedCommandIndex current_projected_commands;
        bool current_projected_commands_built = false;
        std::vector<bool> current_projected_used;
        const auto grouping_finished = InterpolationClock::now();
        std::vector<RigidTransform> rigid_transforms(
            previous_groups.size());
        std::vector<bool> track_groups_with_visibility_churn(
            previous_groups.size(), false);
        std::vector<bool> track_groups_with_unsafe_exact_midpoint(
            previous_groups.size(), false);
        std::vector<TrackMidpointSafety> track_group_unsafe_reason(
            previous_groups.size(), TrackMidpointSafety::safe);
        std::vector<TrackMidpointSafetyDetails> track_group_unsafe_details(
            previous_groups.size());
        std::vector<std::size_t> track_group_unsafe_command(
            previous_groups.size(),
            (std::numeric_limits<std::size_t>::max)());
        std::vector<bool> track_groups_with_vertex_exact_midpoint(
            previous_groups.size(), false);
        std::vector<WorldDrawCommand> track_vertex_candidates;
        track_vertex_candidates.reserve(previous.track_commands);
        const std::size_t no_track_vertex_candidate =
            (std::numeric_limits<std::size_t>::max)();
        std::vector<std::size_t> track_vertex_candidate_indices(
            midpoint.commands.size(), no_track_vertex_candidate);
        MergedCategoryCache current_merged_categories;
        current_merged_categories.reserve(current_groups.size());
        TrackCandidateDiagnostics mapping_diagnostics{};
        const ExactVertexTransformMappings track_transform_mappings =
            build_track_transform_mappings(
                previous_groups,
                current_groups,
                alpha,
                &current_merged_categories,
                &mapping_diagnostics);
        stats.ambiguous_track_vertex_mappings =
            mapping_diagnostics.ambiguous_mappings;
        const std::vector<TrackViewDelta> track_view_deltas =
            build_track_view_deltas(
            track_transform_mappings, alpha);
        std::vector<bool> vehicle_body_groups(
            previous_groups.size(), false);
        std::vector<bool> vehicle_commands_interpolated(
            midpoint.commands.size(), false);
        struct VehicleHoldState {
            std::size_t held_commands{};
            bool body_group{};
        };
        std::unordered_map<
            GroupCategory,
            VehicleHoldState,
            GroupCategoryHash> held_vehicle_categories;
        held_vehicle_categories.reserve(previous_groups.size());
        stats.previous_transform_groups =
            static_cast<std::uint32_t>(previous_groups.size());
        stats.current_transform_groups =
            static_cast<std::uint32_t>(current_groups.size());

        const std::size_t no_previous_group =
            (std::numeric_limits<std::size_t>::max)();
        std::vector<std::size_t> command_to_previous_group(
            previous.commands.size(), no_previous_group);
        struct VehicleCategorySizes {
            std::size_t largest{};
            std::size_t second_largest{};
            std::size_t total{};
        };
        std::unordered_map<
            GroupCategory,
            VehicleCategorySizes,
            GroupCategoryHash> vehicle_category_sizes;
        vehicle_category_sizes.reserve(previous_groups.size());
        for (const auto& group : previous_groups) {
            if (group.identity.category.object_kind != 2)
                continue;
            auto& sizes = vehicle_category_sizes[group.identity.category];
            const std::size_t command_count = group.commands.size();
            sizes.total += command_count;
            if (command_count >= sizes.largest) {
                sizes.second_largest = sizes.largest;
                sizes.largest = command_count;
            } else if (command_count > sizes.second_largest) {
                sizes.second_largest = command_count;
            }
        }
        for (std::size_t index = 0;
             index < previous_groups.size();
             ++index) {
            if (previous_groups[index].identity.category.object_kind == 2) {
                const auto category_sizes = vehicle_category_sizes.find(
                    previous_groups[index].identity.category);
                const std::size_t largest_sibling =
                    category_sizes == vehicle_category_sizes.end()
                        ? 0
                        : previous_groups[index].commands.size() ==
                                category_sizes->second.largest
                            ? category_sizes->second.second_largest
                            : category_sizes->second.largest;
                // GT2 submits the body as one substantially larger group and
                // the four articulated wheels/shadow as smaller siblings.
                // A whole-body Horn fit amplifies integer GTE quantization
                // into depth/coverage changes on tiny bumper and exhaust
                // polygons. Exact vertex motion is stable for the body;
                // rigid fitting remains essential for spinning wheel groups.
                vehicle_body_groups[index] =
                    largest_sibling != 0 &&
                    previous_groups[index].commands.size() >=
                        largest_sibling * 2;
            }
            for (const std::size_t command_index :
                 previous_groups[index].commands) {
                if (command_index < command_to_previous_group.size())
                    command_to_previous_group[command_index] = index;
            }
            if (group_matches[index] < current_groups.size()) {
                ++stats.matched_transform_groups;
                rigid_transforms[index] = fit_rigid_transform(
                    previous_groups[index],
                    current_groups[group_matches[index]]);
                if (rigid_transforms[index].hold)
                    ++stats.incoherent_exact_transform_groups;
                else if (
                    rigid_transforms[index].valid &&
                    rigid_transforms[index].exact
                ) {
                    ++stats.exact_rigid_transform_groups;
                }
                if (previous_groups[index].identity.category.object_kind == 1) {
                    const auto& current_group =
                        current_groups[group_matches[index]];
                    for (const std::size_t command_index :
                         previous_groups[index].commands) {
                        for (const auto& vertex :
                             previous.commands[command_index].vertices) {
                            if (current_group.vertices.find(
                                    vertex_key(vertex)) ==
                                current_group.vertices.end()) {
                                track_groups_with_visibility_churn[index] =
                                    true;
                                break;
                            }
                        }
                        if (track_groups_with_visibility_churn[index])
                            break;
                    }
                }
                if (rigid_transforms[index].valid) {
                    // Articulated vehicle parts must receive a genuine
                    // midpoint pose even when a wheel advances through a
                    // large angle between 30 Hz authored states. Holding the
                    // prior roll duplicated one wheel pose and made the next
                    // authored pose read as a visible 30 Hz flicker.
                    prepare_rigid_transform_sample(
                        &rigid_transforms[index], alpha);
                }
            }
        }
        const auto transforms_finished = InterpolationClock::now();

        for (std::size_t group_index = 0;
             group_index < previous_groups.size();
             ++group_index) {
            const std::size_t current_group_index =
                group_matches[group_index];
            const auto& rigid_transform = rigid_transforms[group_index];
            if (
                previous_groups[group_index].identity.category.object_kind !=
                    1 ||
                current_group_index >= current_groups.size() ||
                !rigid_transform.valid ||
                !rigid_transform.exact
            ) {
                continue;
            }
            const auto& current_group = current_groups[current_group_index];
            const WorldDrawVertex& projection_sample =
                *current_group.vertices.begin()->second;
            for (const std::size_t command_index :
                 previous_groups[group_index].commands) {
                const auto& previous_command =
                    previous.commands[command_index];
                WorldDrawCommand candidate = previous_command;
                const WorldDrawVertex* current_vertices[3]{};
                bool complete = true;
                for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
                    const auto found = current_group.vertices.find(
                        vertex_key(previous_command.vertices[vertex_index]));
                    if (found == current_group.vertices.end()) {
                        complete = false;
                        break;
                    }
                    current_vertices[vertex_index] = found->second;
                }
                for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
                    interpolate_rigid_vertex(
                        midpoint,
                        current,
                        complete
                            ? *current_vertices[vertex_index]
                            : projection_sample,
                        rigid_transform,
                        alpha,
                        &candidate.vertices[vertex_index]);
                }
                const TrackMidpointSafetyDetails safety =
                    exact_track_midpoint_safety(
                        midpoint, previous_command, candidate);
                if (safety.reason != TrackMidpointSafety::safe) {
                    track_groups_with_unsafe_exact_midpoint[group_index] =
                        true;
                    track_group_unsafe_reason[group_index] = safety.reason;
                    track_group_unsafe_details[group_index] = safety;
                    track_group_unsafe_command[group_index] = command_index;
                    break;
                }
            }
        }
        const auto safety_finished = InterpolationClock::now();

        for (std::size_t group_index = 0;
             group_index < previous_groups.size();
             ++group_index) {
            const std::size_t current_group_index =
                group_matches[group_index];
            const auto& rigid_transform = rigid_transforms[group_index];
            if (
                previous_groups[group_index].identity.category.object_kind !=
                    1 ||
                !track_groups_with_visibility_churn[group_index] ||
                track_groups_with_unsafe_exact_midpoint[group_index] ||
                current_group_index >= current_groups.size() ||
                (rigid_transform.valid && rigid_transform.exact)
            ) {
                continue;
            }
            TrackCandidateDiagnostics diagnostics{};
            const TrackCandidateResult candidate_result =
                build_track_visibility_candidates(
                    previous,
                    current,
                    previous_groups[group_index],
                    merged_category_vertices(
                        current_groups,
                        previous_groups[group_index].identity.category,
                        &current_merged_categories),
                    track_transform_mappings,
                    track_view_deltas,
                    alpha,
                    &track_vertex_candidates,
                    &track_vertex_candidate_indices,
                    &diagnostics);
            stats.missing_track_vertex_candidates +=
                diagnostics.missing_candidates;
            if (
                stats.first_missing_track_transform_id == 0 &&
                diagnostics.first_missing_transform_id != 0
            ) {
                stats.first_missing_track_transform_id =
                    diagnostics.first_missing_transform_id;
                stats.first_missing_track_source_identity =
                    diagnostics.first_missing_source_identity;
            }
            if (candidate_result == TrackCandidateResult::success) {
                track_groups_with_vertex_exact_midpoint[group_index] = true;
                ++stats.exact_rigid_transform_groups;
                ++stats.vertex_exact_track_groups;
            } else if (candidate_result == TrackCandidateResult::unsafe) {
                track_groups_with_unsafe_exact_midpoint[group_index] = true;
                track_group_unsafe_reason[group_index] =
                    diagnostics.unsafe_reason;
                track_group_unsafe_details[group_index] =
                    diagnostics.unsafe_details;
                track_group_unsafe_command[group_index] =
                    diagnostics.unsafe_command_index;
            } else {
                ++stats.unavailable_vertex_exact_track_groups;
            }
        }
        const auto candidates_finished = InterpolationClock::now();

        for (std::size_t command_index = 0;
             command_index < midpoint.commands.size();
             ++command_index) {
            auto& command = midpoint.commands[command_index];
            if (
                !(screen_space_command(previous, command) &&
                    command.object_kind == 1) &&
                screen_offset_anchor_command(command)
            ) {
                if (current_screen_offset_anchor_used.empty()) {
                    current_screen_offset_anchor_used.assign(
                        current.commands.size(), false);
                }
                if (interpolate_screen_offset_anchor_command(
                        previous,
                        current,
                        previous.commands[command_index],
                        &current_screen_offset_anchor_used,
                        alpha,
                        &command)) {
                    ++stats.matched_commands;
                    ++stats.matched_track_commands;
                } else {
                    ++stats.held_unmatched_commands;
                }
                continue;
            }
            if (
                screen_space_command(previous, command) &&
                command.object_kind != 1
            ) {
                if (!current_screen_commands_built) {
                    current_screen_commands =
                        build_screen_command_index(current);
                    current_screen_used.assign(
                        current.commands.size(), false);
                    current_screen_commands_built = true;
                }
                if (interpolate_screen_command(
                        previous,
                        current,
                        previous.commands[command_index],
                        current_screen_commands,
                        &current_screen_used,
                        alpha,
                        &command)) {
                    ++stats.matched_commands;
                } else {
                    ++stats.held_screen_commands;
                }
                continue;
            }
            if (unowned_projected_command(previous, command)) {
                if (!current_projected_commands_built) {
                    current_projected_commands =
                        build_projected_command_index(current);
                    current_projected_used.assign(
                        current.commands.size(), false);
                    current_projected_commands_built = true;
                }
                if (interpolate_projected_command(
                        previous,
                        current,
                        previous.commands[command_index],
                        current_projected_commands,
                        &current_projected_used,
                        alpha,
                        &command)) {
                    ++stats.matched_commands;
                } else {
                    ++stats.held_unmatched_commands;
                }
                continue;
            }
            ++stats.eligible_world_commands;
            const std::size_t previous_group =
                command_index < command_to_previous_group.size()
                    ? command_to_previous_group[command_index]
                    : no_previous_group;
            if (previous_group == no_previous_group) {
                if (command.object_kind == 2) {
                    ++held_vehicle_categories[
                        GroupCategory{
                            command.object_kind,
                            command.object_id,
                            command.model_pointer,
                            command.channel}].held_commands;
                }
                ++stats.held_unmatched_commands;
                continue;
            }
            const std::size_t current_group_index =
                group_matches[previous_group];
            if (current_group_index >= current_groups.size()) {
                if (command.object_kind == 2) {
                    auto& held = held_vehicle_categories[
                        previous_groups[previous_group].identity.category];
                    ++held.held_commands;
                    held.body_group = held.body_group ||
                        vehicle_body_groups[previous_group];
                }
                ++stats.held_unmatched_commands;
                continue;
            }
            const auto& current_group =
                current_groups[current_group_index];
            const auto& rigid_transform =
                rigid_transforms[previous_group];
            // Track is static world geometry: in view space every one of
            // its vertices moves by exactly one transform, the camera's.
            // Matching it per group and holding whatever fails to match is
            // what leaves one section behind its neighbour and opens a seam.
            // Applying the shared camera step uniformly cannot crack,
            // because both sides of every shared edge take the same step.
            if (
                command.object_kind == 1 &&
                track_camera_delta_enabled
            ) {
                // A clip fallback vertex carries no object matrix of its own,
                // which previously disqualified the whole triangle and left
                // it frozen against advanced neighbours. The camera step is a
                // view-space transform and needs no per-vertex matrix; the
                // matrix is only used to choose among candidate deltas by
                // scale, so any vertex of this triangle that has one will
                // select the same delta for all three.
                const WorldDrawVertex* scale_reference = nullptr;
                for (const auto& vertex : command.vertices) {
                    if (vertex.exact_transform_valid) {
                        scale_reference = &vertex;
                        break;
                    }
                }
                const TrackViewDelta* camera_step = scale_reference == nullptr
                    ? nullptr
                    : find_track_view_delta(
                        track_view_deltas,
                        *scale_reference,
                        previous_groups[previous_group]
                            .identity.category.channel);
                if (camera_step != nullptr) {
                    const WorldDrawVertex& projection_sample =
                        *current_group.vertices.begin()->second;
                    for (int vertex_index = 0;
                         vertex_index < 3;
                         ++vertex_index) {
                        interpolate_view_delta_vertex(
                            midpoint,
                            current,
                            projection_sample,
                            *camera_step,
                            alpha,
                            &command.vertices[vertex_index]);
                    }
                    ++stats.matched_commands;
                    ++stats.matched_track_commands;
                    continue;
                }
            }
            if (
                command.object_kind == 1 &&
                track_groups_with_vertex_exact_midpoint[
                    previous_group] &&
                track_vertex_candidate_indices[command_index] !=
                    no_track_vertex_candidate
            ) {
                command = std::move(track_vertex_candidates[
                    track_vertex_candidate_indices[command_index]]);
                ++stats.matched_commands;
                ++stats.matched_track_commands;
                continue;
            }
            if (
                command.object_kind == 2 &&
                rigid_transform.hold
            ) {
                // The copied object matrix cannot reproduce this clipped
                // fragment's endpoint vertices. Holding the prior authored
                // triangle is the only bounded operation that also preserves
                // its shape; vertex morphing tears the car silhouette into
                // alternating shards at the viewport edge.
                auto& held = held_vehicle_categories[
                    previous_groups[previous_group].identity.category];
                ++held.held_commands;
                held.body_group = held.body_group ||
                    vehicle_body_groups[previous_group];
                ++stats.held_unmatched_commands;
                ++stats.held_incoherent_vehicle_commands;
                continue;
            }
            if (
                command.object_kind == 1 &&
                track_groups_with_unsafe_exact_midpoint[
                    previous_group]
            ) {
                // Keep a connected authored section atomic. Moving the safe
                // members while holding a near-field member would replace a
                // polygon explosion with a crack at their shared boundary.
                if (stats.held_unsafe_track_commands == 0) {
                    const std::size_t unsafe_group =
                        previous_group;
                    const auto& details =
                        track_group_unsafe_details[unsafe_group];
                    const std::size_t unsafe_command =
                        track_group_unsafe_command[unsafe_group];
                    stats.first_unsafe_track_command =
                        unsafe_command ==
                            (std::numeric_limits<std::size_t>::max)()
                        ? 0
                        : static_cast<std::uint32_t>(unsafe_command);
                    stats.first_unsafe_track_previous_span_x =
                        details.previous_span_x;
                    stats.first_unsafe_track_previous_span_y =
                        details.previous_span_y;
                    stats.first_unsafe_track_midpoint_span_x =
                        details.midpoint_span_x;
                    stats.first_unsafe_track_midpoint_span_y =
                        details.midpoint_span_y;
                    stats.first_unsafe_track_maximum_shift =
                        details.maximum_shift;
                    stats.first_unsafe_track_minimum_depth =
                        details.minimum_depth;
                }
                ++stats.held_unmatched_commands;
                ++stats.held_unsafe_track_commands;
                switch (track_group_unsafe_reason[previous_group]) {
                    case TrackMidpointSafety::nonfinite:
                        ++stats.held_unsafe_track_nonfinite_commands;
                        break;
                    case TrackMidpointSafety::near_depth:
                        ++stats.held_unsafe_track_near_depth_commands;
                        break;
                    case TrackMidpointSafety::shift:
                        ++stats.held_unsafe_track_shift_commands;
                        break;
                    case TrackMidpointSafety::span:
                        ++stats.held_unsafe_track_span_commands;
                        break;
                    case TrackMidpointSafety::safe:
                        break;
                }
                continue;
            }
            if (
                command.object_kind == 1 &&
                track_groups_with_visibility_churn[previous_group] &&
                !(rigid_transform.valid && rigid_transform.exact)
            ) {
                // A track section is a connected authored surface. Moving
                // only its surviving triangles tears road markings and
                // exposes wedges where the PS1 visibility list changed. A
                // fitted section translation also proved unsafe at connected
                // section boundaries in real SSR11 packets.
                //
                // Freezing the section is safe within itself but splits it
                // from the neighbouring section that did advance, which is
                // the zigzag crack visible across the road on midpoints.
                // Track is static world geometry, so every section shares one
                // view-space motion: the camera's. When that shared delta is
                // available, move the whole held section by it instead. That
                // is not a fitted per-section guess, it is the same rigid
                // camera step its neighbours took, so the shared boundary
                // stays closed.
                bool advanced_by_view_delta = false;
                if (
                    exact_transform_is_coherent(command.vertices[0]) &&
                    exact_transform_is_coherent(command.vertices[1]) &&
                    exact_transform_is_coherent(command.vertices[2])
                ) {
                    const TrackViewDelta* section_delta =
                        find_track_view_delta(
                            track_view_deltas,
                            command.vertices[0],
                            previous_groups[previous_group]
                                .identity.category.channel);
                    if (section_delta != nullptr) {
                        const WorldDrawVertex& projection_sample =
                            *current_group.vertices.begin()->second;
                        for (int vertex_index = 0;
                             vertex_index < 3;
                             ++vertex_index) {
                            interpolate_view_delta_vertex(
                                midpoint,
                                current,
                                projection_sample,
                                *section_delta,
                                alpha,
                                &command.vertices[vertex_index]);
                        }
                        advanced_by_view_delta = true;
                    }
                }
                if (advanced_by_view_delta) {
                    ++stats.matched_commands;
                    ++stats.matched_track_commands;
                    continue;
                }
                ++stats.held_unmatched_commands;
                ++stats.held_track_visibility_commands;
                continue;
            }
            const bool use_vehicle_rigid_transform =
                rigid_transform.valid &&
                command.object_kind == 2 &&
                !vehicle_body_groups[previous_group];
            const bool use_exact_track_transform =
                rigid_transform.valid &&
                rigid_transform.exact &&
                command.object_kind == 1;
            const WorldDrawVertex* current_vertices[3]{};
            bool complete = true;
            for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
                const auto found = current_group.vertices.find(
                    vertex_key(command.vertices[vertex_index]));
                if (found == current_group.vertices.end()) {
                    complete = false;
                    break;
                }
                current_vertices[vertex_index] = found->second;
            }
            // Fitted track/camera transforms remain unsafe when visibility
            // churn leaves unmatched near-field triangles, so those groups
            // take the atomic hold above. A coherent exact guest transform can
            // move the complete prior track group without inventing a fit; the
            // group depth, displacement, and span preflight above holds the
            // whole section if any triangle could expand across the frame.
            // Vehicle
            // articulation uses the exact or fitted rigid path, where the
            // group's projection sample is valid for topology-generated
            // vertices without an exact current counterpart.
            if (use_vehicle_rigid_transform || use_exact_track_transform) {
                if (!rigid_transform.exact)
                    command.exact_transform_valid = false;
                const WorldDrawVertex& projection_sample =
                    *current_group.vertices.begin()->second;
                for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
                    interpolate_rigid_vertex(
                        midpoint,
                        current,
                        complete
                            ? *current_vertices[vertex_index]
                            : projection_sample,
                        rigid_transform,
                        alpha,
                        &command.vertices[vertex_index]);
                }
                store_exact_sample_transform(
                    rigid_transform,
                    &command);
                ++stats.matched_commands;
                if (command.object_kind == 1)
                    ++stats.matched_track_commands;
                else if (command.object_kind == 2) {
                    ++stats.matched_vehicle_commands;
                    vehicle_commands_interpolated[command_index] = true;
                }
                continue;
            }
            if (!complete) {
                if (command.object_kind == 2) {
                    auto& held = held_vehicle_categories[
                        previous_groups[previous_group].identity.category];
                    ++held.held_commands;
                    held.body_group = held.body_group ||
                        vehicle_body_groups[previous_group];
                }
                ++stats.held_unmatched_commands;
                continue;
            }
            command.exact_transform_valid = false;
            for (int vertex_index = 0; vertex_index < 3; ++vertex_index) {
                interpolate_vertex(
                    midpoint,
                    current,
                    *current_vertices[vertex_index],
                    alpha,
                    &command.vertices[vertex_index]);
            }
            ++stats.matched_commands;
            if (command.object_kind == 1)
                ++stats.matched_track_commands;
            else if (command.object_kind == 2) {
                ++stats.matched_vehicle_commands;
                vehicle_commands_interpolated[command_index] = true;
            }
        }
        // Keep a car atomic when its body, or a substantial share of its
        // sibling groups, cannot advance coherently. A body whose triangles
        // split between advanced and held reads as alternating shards. A
        // small clipped wheel fragment may safely hold for one midpoint,
        // though; freezing the complete car for every such fragment would
        // unnecessarily restore 30 Hz vehicle motion during ordinary play.
        for (const auto& group : previous_groups) {
            if (group.identity.category.object_kind != 2)
                continue;
            const auto held = held_vehicle_categories.find(
                group.identity.category);
            const auto sizes = vehicle_category_sizes.find(
                group.identity.category);
            if (
                held == held_vehicle_categories.end() ||
                sizes == vehicle_category_sizes.end() ||
                (!held->second.body_group &&
                    held->second.held_commands * 4 < sizes->second.total)
            )
                continue;
            for (const std::size_t command_index : group.commands) {
                if (
                    command_index >= midpoint.commands.size() ||
                    command_index >= previous.commands.size()
                ) {
                    continue;
                }
                if (vehicle_commands_interpolated[command_index]) {
                    --stats.matched_commands;
                    --stats.matched_vehicle_commands;
                    ++stats.held_unmatched_commands;
                    ++stats.held_atomic_vehicle_commands;
                }
                midpoint.commands[command_index] =
                    previous.commands[command_index];
            }
        }
        const auto commands_finished = InterpolationClock::now();
        if (std::getenv("OPENGT_INTERPOLATION_HELD_DIAGNOSTICS") != nullptr) {
            // A held command is one the loop left exactly as copied from the
            // previous list. Those commands sit still for the midpoint and
            // then jump the whole guest interval on the authored frame, so
            // their share of the screen is the size of the 30 Hz judder the
            // 60 Hz output still carries. Report it by object kind and by
            // covered area, not just by command count: a handful of held
            // near-field track triangles cover far more of the frame than a
            // hundred distant ones.
            static thread_local std::uint64_t held_sample = 0;
            std::uint64_t held_interval = 120;
            if (const char* configured = std::getenv(
                    "OPENGT_INTERPOLATION_HELD_DIAGNOSTICS_INTERVAL")) {
                const unsigned long parsed = std::strtoul(
                    configured, nullptr, 10);
                if (parsed != 0)
                    held_interval = parsed;
            }
            if ((held_sample++ % held_interval) == 0) {
                std::array<std::uint32_t, 3> held_counts{};
                std::array<std::uint32_t, 3> total_counts{};
                std::array<double, 3> held_area{};
                std::array<double, 3> total_area{};
                for (std::size_t command_index = 0;
                     command_index < midpoint.commands.size();
                     ++command_index) {
                    const auto& moved = midpoint.commands[command_index];
                    const auto& source = previous.commands[command_index];
                    const std::size_t kind =
                        moved.object_kind <= 2U ? moved.object_kind : 0U;
                    double minimum_x = moved.vertices[0].screen_x;
                    double minimum_y = moved.vertices[0].screen_y;
                    double maximum_x = minimum_x;
                    double maximum_y = minimum_y;
                    bool identical = true;
                    for (int index = 0; index < 3; ++index) {
                        const auto& vertex = moved.vertices[index];
                        minimum_x = std::min<double>(
                            minimum_x, vertex.screen_x);
                        minimum_y = std::min<double>(
                            minimum_y, vertex.screen_y);
                        maximum_x = std::max<double>(
                            maximum_x, vertex.screen_x);
                        maximum_y = std::max<double>(
                            maximum_y, vertex.screen_y);
                        identical = identical &&
                            vertex.screen_x ==
                                source.vertices[index].screen_x &&
                            vertex.screen_y ==
                                source.vertices[index].screen_y;
                    }
                    const double area =
                        (maximum_x - minimum_x) * (maximum_y - minimum_y);
                    ++total_counts[kind];
                    total_area[kind] += area;
                    if (identical) {
                        ++held_counts[kind];
                        held_area[kind] += area;
                    }
                }
                const auto share = [] (double held, double total) {
                    return total > 0.0 ? 100.0 * held / total : 0.0;
                };
                std::size_t colliding_vehicle_keys = 0;
                std::size_t colliding_all_keys = 0;
                for (const auto& group : previous_groups) {
                    colliding_all_keys += group.colliding_vertex_keys;
                    if (group.identity.category.object_kind == 2)
                        colliding_vehicle_keys += group.colliding_vertex_keys;
                }
                std::fprintf(
                    stderr,
                    "[Interpolation-Held] screen=%u/%u track=%u/%u "
                    "vehicle=%u/%u trackAreaShare=%.1f%% "
                    "vehicleAreaShare=%.1f%% screenAreaShare=%.1f%% "
                    "heldUnmatched=%u heldVisibility=%u heldUnsafe=%u "
                    "heldIncoherentVehicle=%u heldAtomicVehicle=%u collidingKeys=%zu/%zu\n",
                    held_counts[0],
                    total_counts[0],
                    held_counts[1],
                    total_counts[1],
                    held_counts[2],
                    total_counts[2],
                    share(held_area[1], total_area[1]),
                    share(held_area[2], total_area[2]),
                    share(held_area[0], total_area[0]),
                    stats.held_unmatched_commands,
                    stats.held_track_visibility_commands,
                    stats.held_unsafe_track_commands,
                    stats.held_incoherent_vehicle_commands,
                    stats.held_atomic_vehicle_commands,
                    colliding_vehicle_keys,
                    colliding_all_keys);
            }
        }
        if (std::getenv("OPENGT_INTERPOLATION_PHASE_DIAGNOSTICS") != nullptr) {
            const auto milliseconds = [] (InterpolationClock::duration value) {
                return std::chrono::duration<double, std::milli>(
                    value).count();
            };
            std::fprintf(
                stderr,
                "[Interpolation-Phases] totalMs=%.3f copyMs=%.3f "
                "groupingMs=%.3f transformsMs=%.3f safetyMs=%.3f "
            "candidatesMs=%.3f commandsMs=%.3f groups=%zu/%zu "
            "candidates=%zu cache=%u/%u\n",
                milliseconds(commands_finished - interpolation_started),
                milliseconds(copy_finished - interpolation_started),
                milliseconds(grouping_finished - copy_finished),
                milliseconds(transforms_finished - grouping_finished),
                milliseconds(safety_finished - transforms_finished),
                milliseconds(candidates_finished - safety_finished),
                milliseconds(commands_finished - candidates_finished),
            previous_groups.size(),
            current_groups.size(),
            track_vertex_candidates.size(),
            stats.previous_group_cache_hit,
            stats.current_group_cache_hit);
        }
        *output = std::move(midpoint);
        *output_stats = stats;
        return WorldInterpolationResult::success;
    } catch (const std::bad_alloc&) {
        return WorldInterpolationResult::allocation_failed;
    }
}

WorldInterpolationResult interpolate_world_draw_lists(
    const WorldDrawList& previous,
    const WorldDrawList& current,
    float alpha,
    WorldDrawList* output,
    WorldInterpolationStats* output_stats
) noexcept {
    return interpolate_world_draw_lists_cached(
        previous,
        current,
        nullptr,
        nullptr,
        alpha,
        output,
        output_stats);
}

const char* world_interpolation_result_name(
    WorldInterpolationResult result
) noexcept {
    switch (result) {
        case WorldInterpolationResult::success: return "success";
        case WorldInterpolationResult::invalid_argument:
            return "invalid_argument";
        case WorldInterpolationResult::incompatible_viewport:
            return "incompatible_viewport";
        case WorldInterpolationResult::allocation_failed:
            return "allocation_failed";
    }
    return "unknown";
}

} // namespace opengt::render
