// Compile the private classifier into this test only; the shipping DLL gains
// no test entry points and the production implementation is exercised directly.
#include "../src/live_renderer_bridge.cpp"

int main() {
    ResidentMesh mesh{};
    // Exact local coordinates from Trial Mountain sign primitive 2 and the
    // opposite-facing full sign primitive 8. The smaller panel is wrongly
    // classified as road artwork if object-local orientation is ignored.
    mesh.vertices = {
        {-1988, -724, -2, 1}, {-1988, -3131, -2, 2},
        {59, -3131, -2, 3}, {59, -724, -2, 4},
        {-4036, -724, -2, 5}, {4155, -724, -2, 6},
        {4155, -3131, -2, 7}, {-4036, -3131, -2, 8}};
    ResidentPrimitive panel{}, support{};
    panel.flags = support.flags = resident_primitive_quad |
        resident_primitive_one_sided | resident_primitive_textured;
    for (int i = 0; i < 4; ++i) {
        panel.indices[i] = static_cast<std::uint16_t>(i);
        support.indices[i] = static_cast<std::uint16_t>(i + 4);
    }
    mesh.primitives = {panel, support};
    for (bool primary : {false, true}) {
        for (auto& primitive : mesh.primitives)
            primitive.flags = panel.flags |
                (primary ? resident_primitive_primary_path : 0);
        classify_resident_track_overlays(&mesh);
        if (mesh.primitives[0].overlay_layer != 1 ||
            !mesh.primitives[1].overlay_support) {
            std::fprintf(stderr, "world-aligned road artwork lost its support\n");
            return 1;
        }
        for (auto& primitive : mesh.primitives)
            primitive.flags |= resident_primitive_local_coordinates;
        classify_resident_track_overlays(&mesh);
        for (const auto& primitive : mesh.primitives) {
            if (primitive.overlay_layer || primitive.overlay_support ||
                primitive.replacement_surface) {
                std::fprintf(stderr, "rotated scenery received road annotations\n");
                return 1;
            }
        }
        if (mesh.overlay_pairs || mesh.overlay_primitives ||
            mesh.replacement_primitives || mesh.maximum_overlay_layer) {
            std::fprintf(stderr, "local-space mesh retained annotation counters\n");
            return 1;
        }
    }
    // Lock the wire contract too: both sides must opt into v2; an old
    // definition cannot silently omit the coordinate-frame distinction.
    std::vector<std::uint8_t> wire(resident_mesh_header_size +
        mesh.vertices.size() * resident_vertex_stride +
        mesh.primitives.size() * resident_primitive_stride);
    const auto write = [&](std::size_t offset, std::uint64_t value, int size) {
        for (int byte = 0; byte < size; ++byte)
            wire[offset + byte] = static_cast<std::uint8_t>(value >> (byte * 8));
    };
    write(0, resident_mesh_magic, 8);
    write(8, 2, 4);
    write(12, resident_mesh_header_size, 4);
    write(16, 1, 8);
    write(24, mesh.vertices.size(), 4);
    write(28, mesh.primitives.size(), 4);
    std::size_t cursor = resident_mesh_header_size;
    for (const auto& vertex : mesh.vertices) {
        write(cursor, static_cast<std::uint16_t>(vertex.x), 2);
        write(cursor + 2, static_cast<std::uint16_t>(vertex.y), 2);
        write(cursor + 4, static_cast<std::uint16_t>(vertex.z), 2);
        write(cursor + 8, vertex.source_identity, 4);
        cursor += resident_vertex_stride;
    }
    for (const auto& primitive : mesh.primitives) {
        write(cursor, 1, 4);
        write(cursor + 4, primitive.flags, 4);
        for (int corner = 0; corner < 4; ++corner)
            write(cursor + 12 + corner * 2, primitive.indices[corner], 2);
        cursor += resident_primitive_stride;
    }
    ResidentMesh parsed{};
    if (!parse_resident_mesh(wire.data(), wire.size(), &parsed) ||
        parsed.overlay_pairs != 0 ||
        (parsed.primitives[0].flags & resident_primitive_local_coordinates) == 0) {
        std::fprintf(stderr, "v2 coordinate-space flag failed to round-trip\n");
        return 1;
    }
    write(8, 1, 4);
    if (parse_resident_mesh(wire.data(), wire.size(), &parsed)) {
        std::fprintf(stderr, "obsolete resident mesh format was accepted\n");
        return 1;
    }

    // Exact Grand Valley bridge coordinates for the textured road sector
    // 0x800FC5FC and its larger adjacent sector 0x800FC818.  Their fixed-point
    // edges differ by half a model unit, producing a 0.032% clipping sliver;
    // they touch at a sector boundary but are not an overlay/support pair.
    ResidentMesh bridge{};
    bridge.vertices = {
        {3843, 3490, 0, 1}, {4814, 3176, 0, 2},
        {4741, 2948, 0, 3}, {3769, 3261, 0, 4},
        {3019, 4262, 0, 5}, {4962, 3634, 0, 6},
        {4814, 3176, 0, 7}, {2871, 3804, 0, 8}};
    ResidentPrimitive road{}, adjacent{};
    road.flags = adjacent.flags = resident_primitive_quad |
        resident_primitive_one_sided | resident_primitive_textured;
    for (int i = 0; i < 4; ++i) {
        road.indices[i] = static_cast<std::uint16_t>(i);
        adjacent.indices[i] = static_cast<std::uint16_t>(i + 4);
    }
    bridge.primitives = {road, adjacent};
    classify_resident_track_overlays(&bridge);
    if (bridge.overlay_pairs || bridge.overlay_primitives ||
        bridge.primitives[0].overlay_layer ||
        bridge.primitives[0].overlay_support ||
        bridge.primitives[1].overlay_layer ||
        bridge.primitives[1].overlay_support) {
        std::fprintf(stderr, "adjacent bridge road sectors became overlays\n");
        return 1;
    }

    // Exact Grand Valley start-box coordinates for untextured segment
    // 0x80122F34 and textured support 0x801231C8.  The marking belongs to the
    // neighboring resident chunk and only crosses this support at the fixed-
    // point boundary.  Its overlap is narrow but measurably larger than the
    // bridge-sector sliver above, and is required to keep the outlined grid
    // box from turning into a filled road strip near the camera.
    ResidentMesh start_box{};
    start_box.vertices = {
        {4287, 5896, 0, 1}, {3232, 5908, 0, 2},
        {3232, 5927, 0, 3}, {4288, 5915, 0, 4},
        {5341, 5700, 0, 5}, {4286, 5712, 0, 6},
        {4288, 5954, 0, 7}, {5343, 5943, 0, 8}};
    ResidentPrimitive marking{}, marking_support{};
    marking.flags = resident_primitive_quad |
        resident_primitive_one_sided;
    marking_support.flags = resident_primitive_quad |
        resident_primitive_one_sided | resident_primitive_textured;
    for (int i = 0; i < 4; ++i) {
        marking.indices[i] = static_cast<std::uint16_t>(i);
        marking_support.indices[i] = static_cast<std::uint16_t>(i + 4);
    }
    start_box.primitives = {marking, marking_support};
    classify_resident_track_overlays(&start_box);
    if (start_box.overlay_pairs != 1 ||
        start_box.overlay_primitives != 1 ||
        start_box.untextured_overlay_primitives != 1 ||
        start_box.primitives[0].overlay_layer != 1 ||
        !start_box.primitives[1].overlay_support) {
        std::fprintf(stderr, "start-box boundary marking lost its support\n");
        return 1;
    }
    return 0;
}
