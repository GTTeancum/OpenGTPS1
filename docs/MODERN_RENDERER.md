# Modern renderer and original Xbox architecture

## Decision

OpenGTPS1 will replace race-scene rasterization with a portable native C++17
renderer. The same scene representation will feed a PC backend and an NXDK
NV2A backend. PS1 framebuffer/VRAM presentation remains available for menus,
HUD, loading screens, and MDEC video until each path has an evidence-backed
replacement.

This is a renderer replacement, not a screen-space patch layer.

## Boundaries

The runtime is divided into four layers:

1. **Guest extraction** observes GT2's camera, transformed model data,
   materials, texture pages, visibility lists, and draw ordering at known
   game-specific boundaries.
2. **Scene construction** converts fixed-point guest data into stable world
   geometry and immutable frame views. LOD and visibility policy live here.
3. **Renderer core** owns backend-neutral vertices, materials, draw items,
   geometry stitching, frame diagnostics, and memory budgets.
4. **Platform backends** translate the same frame into desktop GPU commands or
   NXDK/NV2A commands. They do not infer GT2 model topology.

The current C# runtime will initially call the native core through a narrow
C-compatible bridge. Original Xbox support also requires a C++ RecompOne
emitter/runtime path; the CLR-based guest runtime cannot be carried to NXDK.

## Projected capture bridge

The first executable vertical slice is deliberately narrower than the final
scene contract. RecompOne can capture one real race draw stream as:

- an 80-byte, little-endian `OGTPCAP` header;
- fixed 96-byte projected triangle records in original submission order
  (polygon triangles directly and rectangle sprites as two triangles); and
- one complete 1024 x 512 BGR555 VRAM snapshot, including an explicit HLE
  framebuffer/texture readback.

The runtime waits for the requested input poll, captures one geometry list, and
atomically publishes the file only after VRAM is present. Triangle storage is
hard-capped at 262,144 records (25 MiB) and VRAM adds exactly 1 MiB. A normal
GT2 race frame is about 1.6 MiB. Captures and rendered evidence live under the
ignored `artifacts\` tree and cannot bloat source control.

The native C++17 loader takes caller-owned triangle and VRAM buffers. The
allocation-free CPU reference rasterizer reconstructs PS1 4/8/15-bit textures,
CLUTs, texture windows, mask behavior, transparency, draw order, dithering, and
affine or perspective interpolation. The command-line tool writes compact QOI
or universally viewable PNG output, can extract the exact captured framebuffer
as a VRAM reference, and emits deterministic RGBA hashes.

Build the native target, then capture and render the same deterministic race
frame both ways:

```powershell
cmake -S native -B build\native
cmake --build build\native --config Release
powershell -ExecutionPolicy Bypass -File tools\capture_projected_scene.ps1 `
  -LoosePath C:\path\to\OpenGTPS1 -AiAutoDrive
```

The script refuses success unless both runtime checks prove SDL's dummy audio
driver, the `.ogtcap` file stays between 1 and 32 MiB, both independent native
renders and the VRAM-reference extraction finish, and the wrapper settings
file remains byte-identical.

This bridge is not presented as the modern renderer itself. It still starts
from projected PS1 XY and opportunistic GTE depth, so it cannot establish
authored model adjacency, world-space lighting, a free camera, or Xbox-ready
vertex buffers. Its purpose is to give the native core a real, deterministic
GT2 workload and a compatibility oracle while world-space extraction replaces
it field by field.

## World-space capture

The second executable slice captures GT2 geometry before screen projection
rather than attempting to reconstruct it from framebuffer pixels. It combines
three upstream sources:

- the model-space vertex entering each GTE perspective transform;
- the complete fixed-point GTE rotation and translation plus the resulting
  camera-space vertex; and
- object context installed at GT2's track submission and vehicle model
  submission boundaries.

Track objects use their visibility-table index as a stable identifier.
Vehicles use a stable per-car-state identifier. Every triangle retains its
original submission index, material/PS1 draw state, model pointer, object
identity, transform identity, screen coordinates, model coordinates, view
coordinates, and vertex color. UI sprites and other primitives without a
world-space GTE origin are deliberately excluded instead of being guessed.
The dominant track transform supplies the frame camera; its fixed-point
inverse produces world coordinates in the native loader.

Version 3 of the little-endian `OGTWCAP` format has a fixed 160-byte header,
fixed 212-byte triangle records, and one complete 1 MiB VRAM snapshot. It is
hard-capped at 262,144 triangles. Every triangle records its GPU draw offset;
every vertex records the exact GTE projection offset and projection plane that
created it. This is necessary because GT2 renders the main view and mirror with
different projection state in the same frame. The loader remains compatible
with version 1 and 2 captures. It validates every bound into caller-owned
storage, derives world coordinates, and rejects invalid camera transforms or
vertices. The inspector preserves draw order while exporting a diagnostic OBJ
grouped by stable object identity.

Capture and validate the deterministic race frame with:

```powershell
powershell -ExecutionPolicy Bypass -File tools\capture_world_scene.ps1 `
  -LoosePath C:\path\to\OpenGTPS1 -AiAutoDrive
```

The helper delegates launch safety to the existing capture harness: headless
audio must report SDL's dummy driver twice, the caller's audio environment and
wrapper settings are restored, and incomplete or unexpectedly large captures
are rejected.

Current AI-driven Red Rock evidence at input poll 10,000 contains 4,999 world
triangles and 14,997 valid vertices. Of those triangles, 2,639 are identified
track geometry, 1,817 are identified vehicles, and 543 retain complete
transforms but remain unclassified effects/environment submissions. The scene
has 48 identified track objects, two identified vehicles, 52 object/model
combinations, 151 materials, and 62 transforms. Reapplying the captured camera
to every derived world vertex has 0.000000 RMS error. Reprojecting every vertex
with its own captured GTE state has 0.000000 RMS and maximum screen error:
14,997 of 14,997 vertices reproduce exactly. The main view contains 3,657 draw
commands; 1,342 mirror-view commands are identified and excluded from that
draw list instead of being mistaken for malformed main-camera geometry.

The diagnostic path is environment-gated; normal gameplay avoids provenance
construction and packet-origin lookup.

## Standalone PC backend

`opengt_world_viewer` is the first GPU consumer of the world contract. The
portable C++17 draw-list builder:

- separates main and secondary projection channels;
- emits homogeneous clip coordinates from exact GTE view/projection data;
- retains world position, view position, UVs, vertex color, material state,
  scissor, ordering-table index, object identity, and submission order; and
- derives a face normal for the future lighting/reflection material path.

The Windows backend uses D3D11. It uploads the captured 1024 x 512 BGR555 VRAM
as an integer texture and decodes PS1 4-bit, 8-bit, and 15-bit pages plus CLUT
and texture-window state in the pixel shader. UV interpolation is
perspective-correct; vertex color retains non-perspective Gouraud interpolation.
It implements nearest sampling, raw/modulated texture color, transparent texel
discard, all four PS1 blend modes, STP-aware two-pass textured transparency,
mask-bit stencil behavior, scissoring, and optional dithering. Dithering is off
unless `--dither` is explicitly supplied.

Depth is scoped to coherent identified object/model and ordering-table layers.
This gives each mesh a real Z buffer without allowing legacy sky, scenery,
road, and vehicle layer conventions to overwrite GT2's intentional
inter-model ordering. Secondary mirror geometry is retained in the capture for
a later compositor pass but is not mixed into the main-world buffer.

Hardware D3D11 is the normal path. `--warp` uses Microsoft's software adapter
for deterministic validation. The validator renders twice, requires identical
GPU and compatibility-oracle hashes, bounds each PNG below 2 MiB, and removes
the repeat images:

```powershell
powershell -ExecutionPolicy Bypass -File tools\validate_world_renderer.ps1 `
  -Capture artifacts\modern-world-v3-vehicles\race-frame.ogtwcap
```

The retained AI-driven fixture produces 3,657 main-view commands, including
track and vehicle meshes, with dithering off. Two WARP runs produced identical
GPU hash `299b4de4d2d03d2e`; the independent CPU compatibility oracle produced
`17b87953231f3c05`. The no-depth comparison path measures 4.173142 RGB RMS
(35.72 dB PSNR). A separate default-adapter run proves the hardware D3D11
device path.

For interactive inspection, the same executable can show its GPU output in a
resizable window:

```powershell
build\native\Release\opengt_world_viewer.exe `
  artifacts\modern-world-v3-vehicles\race-frame.ogtwcap `
  artifacts\world-gpu.png --window
```

This backend is standalone. Integrating it into live race/replay presentation,
deduplicating authored topology, and replacing the old road-padding path remain
later milestones.

## Texture projection

Race vertices carry model/world position and exact GTE view/projection state
through the native scene path. The backend performs homogeneous projection and
perspective-correct interpolation from those values. It does not reconstruct
perspective from already projected PS1 XY packets or correlate framebuffer
pixels.

PS1 Quality can still request affine interpolation for visual compatibility.
Enhanced and Custom use the native perspective path.

## Road and model seams

Screen-space triangle expansion is not part of the new architecture.

The scene decoder assigns a stable stitch-group identifier only when GT2 data
establishes that two vertices represent the same authored boundary point. The
renderer core then snaps every explicitly linked copy to one canonical 3D
position before vertex-buffer upload. Per-corner UVs, normals, colors, and
material boundaries remain independent, so joining geometry does not smear
textures across intentional seams.

The first implementation is `native/src/geometry_stitcher.cpp`. Its distance
setting is solely a corruption guard for bad topology metadata; it never
searches for nearby vertices. T-junctions will require boundary subdivision,
and coplanar overlap flicker will require deterministic primitive ownership.
Those are separate topology cases and must be diagnosed from captured scene
data rather than hidden with padding.

Every scene capture will eventually log:

- stitch group and source object identifiers;
- chosen canonical vertex;
- maximum pre-stitch displacement;
- rejected/corrupt groups;
- T-junction candidates; and
- overlapping coplanar triangle candidates.

## LOD and visibility

Draw distance and LOD are scene-selection policies, not shader tricks.

- PS1 Quality submits GT2's original visibility result and selected vehicle
  LOD.
- Enhanced submits the complete authored track object set and highest vehicle
  LOD.
- Custom controls the two choices independently.

The original Xbox backend must stream this data within a fixed budget instead
of assuming desktop memory. Scene content will be partitioned into immutable
track geometry, transient cars/effects, and PS1 UI/video surfaces.

## Materials and future lighting

The initial material record preserves PS1 texture, vertex color, transparency,
and ordering semantics. It deliberately reserves a backend-neutral path for
normals and specular strength. Later milestones can add:

- directional and ambient lighting;
- environment-mapped car reflections;
- normal/bump maps supplied by mods; and
- per-material specular response.

These additions must not alter simulation, collision, replay, or save state.

## Platform contract

The core uses C++17, fixed-width types, explicit array views, and no graphics
API types in public scene structures. Capture loading uses caller-owned
buffers. The desktop draw-list builder uses bounded `std::vector` storage and
reports allocation failure; the NXDK implementation will replace those vectors
with preallocated arenas while retaining the same public records. This keeps
graphics API and desktop runtime dependencies out of the shared contract.

### PC

The first backend uses D3D11 for hardware rendering and WARP-based deterministic
validation. D3D headers, shader compilation, and device objects are isolated in
`world_gpu_renderer_d3d11.cpp`; none appear in the capture or draw-list API.

### Original Xbox

The NXDK backend will target NV2A directly and observe the console's 64 MiB
unified-memory limit. It will use:

- 16-bit indices when a mesh partition permits them;
- bounded vertex/index/material arenas;
- preallocated transient frame storage;
- explicit texture residency and eviction;
- no runtime shader compilation; and
- no dependency on .NET, SDL desktop windows, or desktop-only filesystem APIs.

## Milestones

0. Capture and independently rasterize the live projected draw stream and VRAM
   as a bounded migration fixture.
1. **Complete:** capture a deterministic world-space race frame with camera,
   geometry, materials, object identity, and original draw order.
2. **Complete:** render that capture in a standalone D3D11 PC viewer and
   compare deterministic WARP output against the independent compatibility
   renderer.
3. Replace screen-space road padding with explicit boundary stitching and add
   tests for coincident edges, T-junctions, and coplanar overlap.
4. Integrate the PC backend into race/replay while retaining the PS1 2D
   compositor for menus, HUD, and video.
5. Add the RecompOne C++ guest emitter/runtime needed by NXDK.
6. Render the same captured scene through the NV2A backend within a measured
   memory budget.
7. Move input, audio, loose-file I/O, memory cards, and the complete game loop
   to the native runtime.

Each milestone needs structured logs and deterministic captures. A visually
plausible screenshot alone is not an acceptance test.
