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

Version 4 of the little-endian `OGTWCAP` format has a fixed 160-byte header,
fixed 224-byte triangle records, and one complete 1 MiB VRAM snapshot. It is
hard-capped at 262,144 triangles. Every triangle records its GPU draw offset;
every vertex records the exact GTE projection offset and projection plane that
created it plus a capture-stable authored identity interned from exact model
provenance. This is necessary because GT2 renders the main view and mirror with
different projection state in the same frame and repeats boundary vertices in
different 4096-unit model sectors. The loader remains compatible with versions
1 through 3. It validates every bound into caller-owned storage, derives world
coordinates, and rejects invalid camera transforms or vertices. The inspector
preserves draw order while exporting a diagnostic OBJ and optional topology
CSV grouped by stable object identity.

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
  -Capture artifacts\modern-world-v4-topology-live\race-frame.ogtwcap
```

The retained AI-driven fixture produces 3,657 main-view commands, including
track and vehicle meshes, with dithering off. The current version-4 topology
fixture has complete authored identity for all 7,917 captured track vertex
instances. Its topology pass emits 3,659 commands from 3,657 inputs after two
real source triangles become four exact T-junction subdivisions. The same
frame reports 227 authored boundary groups, 64 coplanar overlap pairs, 60 exact
duplicate pairs, and three deterministic ownership reorders. WARP validation
is deterministic at GPU hash `63c96f524265c09b`; the independent CPU
compatibility renderer remains stable at `17b87953231f3c05`. A separate
default-adapter run proves the hardware D3D11 device path.

For interactive inspection, the same executable can show its GPU output in a
resizable window:

```powershell
build\native\Release\opengt_world_viewer.exe `
  artifacts\modern-world-v3-vehicles\race-frame.ogtwcap `
  artifacts\world-gpu.png --window
```

`--scale 1` through `--scale 8` increases the standalone GPU target itself,
so diagnostic screenshots contain newly rasterized samples rather than a
post-process enlargement. This is viewer-only evidence plumbing; it does not
expose or prematurely implement the future wrapper resolution/widescreen
setting.

The same backend now runs in the packaged PC build. The standalone viewer
remains the deterministic capture oracle and diagnostic surface.

## Live PC integration

Race and replay presentation use a bounded C ABI bridge in
`opengt_live_renderer.dll`. RecompOne records the newest complete world frame
directly into one of three reusable OGTWCAP v4 buffers, including exact
provenance and a complete VRAM snapshot. A persistent native worker consumes
only the newest pending frame, builds the same draw list/topology used by the
standalone viewer, and renders through D3D11 without filesystem traffic or a
per-frame device rebuild. The emulation thread never waits for native
rendering; stale pending or published buffers are returned to their pools.

Native world output replaces only the live 3D region. Explicit screen-space
triangles carry the PS1 HUD, tachometer, minimap, text, and other race/replay
overlays through the same composition. Ordinary polygons without GTE
provenance are retained as screen primitives, which includes the tachometer
needle and redline wedge. GT2 emits the thin tachometer and turbo line layers
on alternating screen-only frames, so the recorder retains that bounded line
layer and appends it to the next world frame. Menus, loading screens,
display-mode transitions, and MDEC video remain owned by the proven PS1
compositor.

The reusable output pool is sized for a 640x512 PS1 display at the maximum 4x
native scale, preventing a late `invalid_argument` failure while keeping
allocation bounded. Submission is intentionally limited to GT2's 320x240 live
race/replay viewport. Larger transition and rotating-car Results draw areas
return to the complete PS1 compositor; they are not partial native-world
surfaces.

PS1 UV rules differ across those two classes. Authored 3D polygons sample
integer UVs as texel centers with `floor(uv + 0.5)`, which fixes the Red Rock
0:33 road fault. Screen-space sprites retain edge-based `floor(uv)` sampling,
preventing the minimap and tachometer from reading one texture column beyond
their rectangles.

The live recorder grows its fixed MemoryStream before copying VRAM into the
public backing array. This order is required because `SetLength` zero-fills
newly exposed bytes; copying first would erase the entire submitted VRAM
snapshot and produce a sparse, dark native frame.

Packaged validation in
`artifacts/native-track-surface-corrected-final-v3` reaches input poll 45,100
through race, natural replay, the deliberate 352x300 Results compositor
handoff, and orderly shutdown. It submits 19,505 native frames, renders 19,496,
consumes 19,494, and drops 11 obsolete queue entries. It logs no native
disable, truncation, fatal error, or unhandled exception and proves the
headless harness opened only SDL's dummy audio backend.

The accepted native replay video is
`artifacts/native-track-surface-corrected-final-v3/GT2_Native_Enhanced_Replay_Track_Surface_Corrected.mp4`
(H.264 High/yuv420p, 640x480, 30 fps, 6,499 frames, 216.633 seconds,
33,224,796 bytes, SHA-256
`76F3BB5C455EB87C741684EB689F25C73CFB847C2C1F2EA7091E9F58A5AEDA10`).
End-to-end four-frame-per-second review shows continuous track sections and
complete gauge layers. Every encoded frame from 0:52 through 1:02 received an
additional frame-by-frame review. Exact failed/replacement frames at 0:55.000
and 0:55.500 prove the former diagonal road slab is gone. The extreme
car-filling replay cut is authored; the slab was not. The final Results review
confirms the complete PS1 UI after the native handoff.

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

`native/src/world_topology.cpp` is an API-neutral C++17 topology pass. It
requires complete authored vertex identities and operates on exact integer GTE
view coordinates. That common coordinate frame is important: GT2 stores
neighboring road sectors in local model frames separated by 4096 units, while
their transformed boundary copies are exactly equal in view space.

The pass builds an exact vertex/edge graph, canonicalizes only positions backed
by distinct authored identities, classifies boundary/manifold/nonmanifold
edges, subdivides exact T-junctions, and detects positive-area coplanar
overlap. Same-material opaque overlap components receive deterministic
primitive ownership; different-material overlaps retain original submission
order. Per-corner UVs, colors, normals, and material seams are never merged.
There are no proximity searches, epsilon joins, or screen-space expansion.

The inspector reports all topology counters on stdout and can write one CSV
row per source triangle. Synthetic tests cover boundary copies, T-junction
subdivision, and deterministic coplanar ownership.

The intermittent Red Rock replay fault at about 0:33 was captured at input poll
32,492 and isolated to two triangles whose shared GTE-view edge is exact, but
whose model-space X coordinates differ by 4096 and whose texture pages differ.
It was not a geometric hole. The D3D11 PS1 sampler incorrectly treated integer
UVs as texel edges with `floor(uv)`; PS1 integer UVs identify texel centers.
Sampling the nearest texel with `floor(uv + 0.5)` removes the dotted black line
without padding or UV nudges. In the fixed 4x replay crop, dark pixels on the
known line fell from 22 to zero.

## LOD and visibility

Draw distance and LOD are scene-selection policies, not shader tricks.

- PS1 Quality submits GT2's original visibility result and selected vehicle
  LOD.
- Enhanced retains GT2's authored current-sector potential-visibility set,
  extends its later distance gates, and selects the highest vehicle LOD.
- Custom controls the two choices independently.

Overlay 0 applies a second camera-relative radial cutoff after consuming that
visibility set. Extended Draw Distance disables only this redundant stock
cutoff; frustum, near-plane, and ordinary polygon clipping remain active. This
prevents distant authored objects from crossing the radial threshold and
popping into view. GT2's sector lists must not be unioned: they are
potential-visibility sets containing mutually exclusive or occluded surfaces,
and the former union exposed a false diagonal road slab at replay time 0:55.
Replay's separate stock distance gate is extended by the same option.

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
3. **Complete in the standalone renderer:** replace screen-space road padding
   with authored topology, exact T-junction subdivision, deterministic
   coplanar ownership, structured diagnostics, and synthetic/live tests.
4. **Complete on PC:** integrate the D3D11 backend into live race/replay,
   preserve PS1 screen-primitive semantics and persistence for every HUD
   layer, eliminate motion-visible track seams and section pop-in, and retain
   the PS1 compositor for menus, loading screens, display-mode transitions,
   Results, and video.
5. Add the RecompOne C++ guest emitter/runtime needed by NXDK.
6. Render the same captured scene through the NV2A backend within a measured
   memory budget.
7. Move input, audio, loose-file I/O, memory cards, and the complete game loop
   to the native runtime.

Each milestone needs structured logs and deterministic captures. A visually
plausible screenshot alone is not an acceptance test.
