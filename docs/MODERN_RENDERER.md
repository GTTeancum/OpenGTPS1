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

## Texture projection

Race vertices will carry model/world position through the native scene path.
The backend performs homogeneous projection and perspective-correct
interpolation from those coordinates. It will not reconstruct perspective from
already projected PS1 XY packets or correlate transient GTE depth metadata.

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
API types in public scene structures. Geometry processing takes caller-owned
scratch arenas; the renderer core performs no hidden heap allocation. This
makes memory cost visible on both desktop and the 64 MiB Xbox target.

### PC

The first backend will use a desktop API suitable for rapid capture and shader
validation. Backend choice remains isolated from scene extraction.

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

1. Capture a deterministic race frame with camera, geometry, materials, object
   identity, and original draw order.
2. Render that capture in a standalone PC viewer and compare it against the
   compatibility renderer.
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
