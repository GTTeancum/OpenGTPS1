#include "opengt/geometry_stitcher.hpp"

#include <cstdlib>
#include <iostream>

using opengt::render::BoundaryAnchor;
using opengt::render::RenderVertex;
using opengt::render::StitchOptions;
using opengt::render::StitchWorkspace;
using opengt::render::Vec2;
using opengt::render::Vec3;
using opengt::render::stitch_mesh_boundaries;

namespace {

RenderVertex vertex(float x, float y, float z, float u, float v) {
    return RenderVertex{
        Vec3{x, y, z},
        Vec3{0.0F, 1.0F, 0.0F},
        Vec2{u, v},
        0xFFFFFFFFU,
    };
}

bool expect(bool condition, const char* message) {
    if (!condition) {
        std::cerr << "FAILED: " << message << '\n';
        return false;
    }
    return true;
}

bool stitches_only_explicit_boundaries() {
    RenderVertex vertices[] = {
        vertex(10.0F, 0.0F, 4.0F, 0.0F, 0.0F),
        vertex(12.0F, 0.0F, 4.0F, 1.0F, 0.0F),
        vertex(10.018F, 0.0F, 4.0F, 0.4F, 1.0F),
        vertex(10.01F, 0.0F, 4.0F, 0.8F, 1.0F),
    };
    const BoundaryAnchor anchors[] = {
        BoundaryAnchor{2, 42, 1, 0},
        BoundaryAnchor{0, 42, 0, 0},
    };
    BoundaryAnchor anchor_scratch[2]{};
    std::uint32_t vertex_scratch[2]{};

    const auto stats = stitch_mesh_boundaries(
        vertices,
        4,
        anchors,
        2,
        StitchWorkspace{anchor_scratch, vertex_scratch, 2},
        StitchOptions{0.05F}
    );

    bool okay = true;
    okay &= expect(stats.stitched_group_count == 1, "one group stitched");
    okay &= expect(stats.adjusted_vertex_count == 1, "one vertex adjusted");
    okay &= expect(vertices[2].position.x == 10.0F, "boundary snapped");
    okay &= expect(vertices[2].texcoord.x == 0.4F, "UV seam preserved");
    okay &= expect(
        vertices[3].position.x == 10.01F,
        "nearby unlinked vertex left untouched"
    );
    return okay;
}

bool rejects_corrupt_links() {
    RenderVertex vertices[] = {
        vertex(0.0F, 0.0F, 0.0F, 0.0F, 0.0F),
        vertex(20.0F, 0.0F, 0.0F, 1.0F, 0.0F),
    };
    const BoundaryAnchor anchors[] = {
        BoundaryAnchor{0, 7, 0, 0},
        BoundaryAnchor{1, 7, 1, 0},
        BoundaryAnchor{99, 8, 0, 0},
    };
    BoundaryAnchor anchor_scratch[3]{};
    std::uint32_t vertex_scratch[3]{};

    const auto stats = stitch_mesh_boundaries(
        vertices,
        2,
        anchors,
        3,
        StitchWorkspace{anchor_scratch, vertex_scratch, 3},
        StitchOptions{0.1F}
    );

    bool okay = true;
    okay &= expect(stats.rejected_group_count == 1, "bad group rejected");
    okay &= expect(stats.invalid_anchor_count == 1, "bad index rejected");
    okay &= expect(stats.adjusted_vertex_count == 0, "mesh not corrupted");
    okay &= expect(vertices[1].position.x == 20.0F, "position unchanged");
    return okay;
}

bool requires_explicit_workspace() {
    RenderVertex vertices[] = {
        vertex(0.0F, 0.0F, 0.0F, 0.0F, 0.0F),
        vertex(0.01F, 0.0F, 0.0F, 1.0F, 0.0F),
    };
    const BoundaryAnchor anchors[] = {
        BoundaryAnchor{0, 1, 0, 0},
        BoundaryAnchor{1, 1, 1, 0},
    };
    BoundaryAnchor anchor_scratch[1]{};
    std::uint32_t vertex_scratch[1]{};

    const auto stats = stitch_mesh_boundaries(
        vertices,
        2,
        anchors,
        2,
        StitchWorkspace{anchor_scratch, vertex_scratch, 1},
        StitchOptions{0.1F}
    );

    bool okay = true;
    okay &= expect(stats.insufficient_workspace, "small workspace rejected");
    okay &= expect(vertices[1].position.x == 0.01F, "mesh left untouched");
    return okay;
}

} // namespace

int main() {
    const bool okay =
        stitches_only_explicit_boundaries() &&
        rejects_corrupt_links() &&
        requires_explicit_workspace();
    if (!okay) {
        return EXIT_FAILURE;
    }

    std::cout << "OpenGT renderer core tests passed\n";
    return EXIT_SUCCESS;
}
