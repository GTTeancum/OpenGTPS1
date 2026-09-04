# Modern renderer and original Xbox architecture

## Decision

OpenGTPS1 will replace race-scene rasterization with a portable native C++17
renderer. The same scene representation will feed a PC backend and an NXDK
NV2A backend. The final PC port will ship one modern renderer. The PS1-era
compatibility world renderer and its PS1 Quality selector are no longer
user-selectable. Runtime configuration is canonicalized to the fixed modern
contract: 4x source rendering, corrected projection, smoothed texture
sampling, stabilized topology, complete authored distance, maximum LOD, and
no PS1 dithering. Authored 2D command composition remains responsible for
menus, loading screens, Results, video, and world-free transitions; it is a
layer of the modern presentation path, not an alternate 3D renderer.
Standalone PS1 framebuffer/VRAM tools remain deterministic development oracles
only. Original artwork, layout, timing, and authored game behavior remain the
fidelity target; renderer-era limitations do not.

This is a renderer replacement, not a screen-space patch layer.

## Authoritative reconstruction mandate (2026-08-25)

The authoritative game build is the exact installed NTSC-U disc pair already
used by this repository:

- Simulation Disc revision 2, `SCUS-94488`, executable `SCUS_944.88`,
  SHA-256 `4DD40D01A3E83967E2D4301106890EB314D72027802BEE077BBBC246F152E331`;
- Arcade Mode Disc, `SCUS-94455`, executable `SCUS_944.55`, SHA-256
  `67782AE7520105B39BD1716D910332240D0F11171DABFD04518F8AA04E1CA519`.

Renderer reconstruction is constrained as follows:

- Seattle Circuit is the initial root-cause development course. After its race
  and replay/attract cameras pass, audits expand to other courses to expose new
  renderer failures. Both manual driving and replay views are required
  validation scenarios. Fixes must remain general renderer invariants; no
  course-specific rendering exceptions are permitted.
- Direct command-line Arcade race entries replace menu navigation in the
  development loop. Only the in-race HUD is part of this vertical slice.
- Real-PS1 behavior is a development oracle for game state, transforms,
  visibility, materials, and timing. Affine texture warping and other
  rasterizer-era limitations are not fidelity targets.
- Emulator use is permitted only when necessary and requires explicit approval
  at the beginning of each major work turn. That approval covers the described
  oracle experiments and their repeated deterministic runs for that work turn,
  but does not carry into a later turn. Development-only guest hooks or patches
  may export pre-projection vertices, GTE state, and object identity.
- Oracle, compatibility-rasterizer, trace, emulator, and fallback paths must be
  absent from release packages and unavailable to users. The released game has
  one renderer: the modern renderer.
- The modern result must use corrected, non-affine world projection; stable
  object placement and motion; artifact-free edge clipping; and modern depth
  handling while preserving deliberately identified transparency/effect order.
- GT2 has no fog, and the modern renderer must not add any. Maximum draw
  distance must extend beyond the original game's visibility limit far enough
  that road and scenery pop-in cannot be seen during ordinary play. The PC
  renderer keeps a bounded three-sector horizon resident in both directions
  and removes the original radial distance cutoff. It must still preserve
  GT2's authored coarse selection of mutually exclusive sector and auxiliary
  meshes; a full looping-course union is invalid. Only demonstrably safe
  frustum, backface, explicit game-state, or later invisible occlusion culling
  may remain.
- Widescreen is part of the reconstruction requirement and is not deferred.
  It must be true horizontal-plus widescreen: preserve the intended vertical
  view while revealing additional track to the left and right, without image
  stretching, vertical cropping, or a screen-space hack. Visibility, clipping,
  projection, and draw-distance fixes must work across supported aspect ratios
  rather than being fitted to 4:3 output.
- Widescreen HUD artwork retains its authored size. Connected glyph and sprite
  groups remain centered or anchor to their corresponding left/right edge;
  edge groups preserve the same proportional margin they had at 4:3 instead of
  remaining inside a centered 4:3 island.
- PC is the only active renderer target. Original Xbox/NXDK work is deferred
  until the PC renderer is accepted as complete and must not constrain the
  current architecture or implementation work.
- Modernized pixels need not match the PS1 framebuffer. Correctness means that
  objects appear when and where intended, remain stable, do not clip at screen
  edges, and produce no geometry, projection, ordering, or road artifacts.

The development host exposes no-menu race and replay entry points for Seattle
Circuit, Special Stage Route 5, and Trial Mountain Circuit:

```powershell
GranTurismo2PC.exe --arcade-race seattle-circuit
GranTurismo2PC.exe --arcade-replay seattle-circuit
GranTurismo2PC.exe --arcade-race special-stage-route-5
GranTurismo2PC.exe --arcade-replay special-stage-route-5
GranTurismo2PC.exe --arcade-race trial-mountain
GranTurismo2PC.exe --arcade-replay trial-mountain
```

The race switch runs Arcade overlay 2's native asynchronous parameter-database
setup, installs a verified byte-exact native selection captured before
finalization, and invokes the original race constructor. Trial Mountain reuses
the verified native Class C selection and changes only the course identity
before construction, yielding the Citroen Xsara player roster. The host verifies
the resulting configuration and race-state invariants, copies the constructor's
own finalized selection through the stock overlay-3 handoff, and resumes the
original loader. It does not copy a downstream vehicle or race-state fixture.
The skipped controller belongs only to the unconstructed menu fade scene.

The unified install relocates Arcade's source-disc `GT2.VOL` from LBA 472 to the
shared unified volume location at LBA 473. The Arcade loose manifest and
relocated ISO metadata must agree on the unified value; install preparation now
rejects either template before copying data when they do not. The release smoke
test independently checks the installed manifest. This prevents a one-sector
displacement from booting plausible data and then failing later at an unrelated
unmapped guest address.

The replay switch begins identically and retains Arcade's native role-`3`
player record because that record owns race completion and Results. Once the
race engine has constructed the player car, the harness selects GT2's original
per-car CPU driver (`mode 2`) instead of its pad driver (`mode 0`); vehicle,
route, physics, lap, result, replay, and camera state remain native. Stage-
relative input confirms Results until GT2 instantiates its own replay vehicles
and cameras. The harness does not install a fabricated finish, replay state,
vehicle record, or camera. No scripted steering, throttle, or braking is
supplied. The only replay-launch input is a late Results confirmation after the
native CPU-driven race has finished.

`tools/test_seattle_direct_replay.ps1` is the end-to-end acceptance gate for
that path. It does not enable the soak quick-win transition or provide driving
input. It stops on the specifically labelled `replay_1_0300` presentation so
the automatic `race_1` proof cannot accidentally shorten the test. The
2026-08-26 run entered the race at input poll 341, entered GT2's native replay
at poll 16,346, and captured a fresh non-synthetic, non-repeated replay frame
at poll 16,640. Both CPU-driver engagements were observed and the memory card
was restored byte-exactly afterward.

The shipping raw course decoder observes every resident mesh immediately before
GT2 would project or reject a primitive. It validates all model pointers and
vertex indices and consumes the eight authored primitive streams
(`F3/F4/G3/G4/FT3/FT4/GT3/GT4`). Seattle's complete deduplicated catalog is
not a valid visibility set: sector lists include mutually exclusive, occluded,
and visual-proxy surfaces. The shipping selector retains the current authored
order plus three sectors in each direction, while the auxiliary path preserves
GT2's authored mask. A 4,501-frame CPU-driven Seattle run therefore varied
correctly from 61 to 94 objects and 2,038 to 3,218 source primitives per frame.
It crossed 57 sector boundaries; all 56 stock additions were already resident
and all 60 stock removals remained resident. The run decoded 12,561,878 source
primitives with zero failures, duplicate calls, or guest-track fallbacks, and
rendered 4,479 authored outputs with zero synthetic, repeated, or dropped
outputs. This pre-projection source does not use screen-edge padding or infer
missing geometry from the framebuffer.
A subsequent installed-release race/replay smoke covered 16,659 frames and the
wider route varied from 61 to 97 objects and 2,038 to 3,389 source primitives,
again with zero decode failures or guest-track fallbacks.
Development controls such as `RECOMPONE_TRACE_GT2_TRACK_MESH=1` and
`RECOMPONE_GT2_TRACK_MESH_OBJ_PATH` add tracing or OBJ export only; release
policy removes them before runtime initialization.

Face visibility is likewise reconstructed before PS1 screen projection. A
triangle uses the continuous view-space sign corresponding to its authored
winding. GT2 makes one coarse NCLIP decision for each GPU quad and submits both
halves when either test passes. The modern triangle list does not copy that
packet limitation: it evaluates the two authored halves independently, so a
tiny warped half crossing edge-on cannot make its much larger back-facing
partner appear for one frame. Homogeneous clipping, rather than culling, owns
triangles that cross the near plane.

The two GT2 course functions use different GTE FIFO orders. The primary path
tests its packet halves and admits `first > 0 OR second < 0`. Disassembly and a
development-only hook in Arcade `0x80023484` established that the alternate
path tests source orders `(0,1,2)` and `(2,0,3)` and admits `first < 0 OR second
> 0`. The previous alternate rule compared the difference between the two
areas. That was not GT2's branch: two large same-facing areas could trade
numeric rank under camera motion and toggle the whole quad. The exact guest
branch remains an oracle; shipping uses the corresponding independent
per-triangle windings.

A historical 1,089-frame CPU-driven Seattle face audit compared 21,865,705
consecutive triangle decisions under the now-rejected full-course union.
Primitive identity had zero duplicate keys. Of 235
isolated face-state changes, 68 overlapped the 16:9 target while remaining
fully in front of the near plane, and none reached one native pixel of
projected area in any of its three frames. Near-plane passages are excluded
from that metric and audited by the native homogeneous-clip diagnostics. This
makes macroscopic course-face flicker a logged failure without adding temporal
hysteresis to rendering.

The PS1 GTE projection remains a development oracle, not a shipping visibility
rule. Diagnostic builds can join each generated renderer branch to the exact
source primitive, object, transform, packed face bits, and both NCLIP values.
A 79-frame direct Seattle correlation joined all 529,097 decisions with zero
missing primitives. The local GTE projection model agreed with 526,344 exact
GT2 branch results; 2,247 were model-only and 506 exact-only. Of 300,808 faces
kept only by continuous geometry, both GT2 and the modeled PS1 projection
rejected 298,921, and 297,928 had quantized to zero screen-space area. Of 2,093
faces kept only by GT2, 1,779 were matching PS1 projection-induced winding
flips. Copying either behavior would restore distant disappearance and
flicker. The modern renderer therefore uses continuous view-space facing and
homogeneous clipping; clamped integer NCLIP is logged only to explain oracle
divergence. Exact face-oracle calls are conditionally removed from release
guest code with `OPENGT_RELEASE_PACKAGE`.

Continuous facing retains the GTE matrix's full 12-bit fractional transform;
it does not first truncate each transformed vertex to integer view units.
That truncation was independently rounding the corners of nearly edge-on
course primitives and could reverse a modern facing decision on adjacent 60 Hz
frames. A bounded CPU-driven replay trace isolated the failure to Seattle
object `0x800B558C`, model `0x8011D410`: the integer-derived rule alternated
through polls 25,734--25,741, while the preserved fixed-point rule remained
coherently rejected through 25,741 and changed once at the authored camera cut
at 25,742. The face-only regression then covered 31,529 world frames with zero
facing degeneracies (previously 4,848), zero track or background decode
failures, zero guest fallbacks, and no synthetic, repeated, or missing authored
outputs. That run used the subsequently rejected full-course selector; its
facing evidence remains valid, but its residency counts are not an acceptance
target. A sub-view-unit winding test protects the fractional-coordinate path
without depending on that Seattle object.

The native projection boundary follows the same rule. Capture version 6
already carries each vertex's authored model coordinate and exact GT2 `RT/TR`
transform, but the former "continuous" path still divided the GTE's truncated
integer `IR/SZ` result. Native draw-list construction now reconstructs
camera-space X/Y/Z from `model * RT / 4096 + TR` before perspective division,
homogeneous clipping, and depth generation. Screen-anchored effects and the
PS1 projection oracle remain on their explicit integer/screen paths. The same
historical 31,529-frame CPU race/replay repeated with fractional track, vehicle,
and background depths, zero non-finite findings in 243 bounded diagnostic
records, zero secondary-view leakage, and zero renderer fallbacks or authored-
output gaps. A half-view-unit native test proves that projection and clip W no
longer snap to integer camera space.

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
It implements decoded bilinear sampling for perspective-correct 3D materials,
point sampling for exact HUD/text texels, raw/modulated texture color,
transparent texel discard, all four PS1 blend modes, STP-aware two-pass
textured transparency, mask-bit stencil behavior, scissoring, and optional
dithering. Transparent neighboring palette entries are excluded and the
remaining bilinear weights are renormalized, preventing dark fringes around
car decals and track billboards. Dithering is off unless `--dither` is
explicitly supplied.

Depth is scoped to coherent identified object/model and ordering-table layers.
This gives each mesh a real Z buffer without allowing legacy sky, scenery,
road, and vehicle layer conventions to overwrite GT2's intentional
inter-model ordering. Secondary mirror geometry is retained in the capture for
a later compositor pass but is not mixed into the main-world buffer.

Resident course meshes preserve GT2's authored road-marking ownership on top
of that physical depth buffer. Positive-area opaque overlaps form deterministic
overlay layers only when the smaller surface occupies at most one quarter of
the supporting primitive. Source order is deliberately irrelevant. Seattle
uses both conventions: textured road artwork is commonly authored after its
support, while the nine untextured white rectangles after the first hairpin are
authored before four much larger asphalt primitives in the same resident mesh.
One authored class is exactly coplanar. RE traces identify a second family as
detail quads raised one or two integer model units over road support; normalized
plane distance can be smaller on a sloped surface, so the classifier verifies
parallel planes and bounds the separation to two model units rather than
matching one world axis. Seattle's decoded data has a clean geometric boundary:
authored detail relationships end below 20 percent while alternate road/LOD
relationships begin above 28 percent.
The renderer treats the resulting detail stack as a road-artwork category, not
as a texture/color ID or a per-track exception. After opaque world depth is
complete, a dedicated pass draws every classified white line, yellow line,
arrow, lane marking, and grid box with four view units of post-raster depth
priority. That bound covers the complete measured support/detail separation,
including the two-unit tunnel class, while an actually nearer wall, vehicle, or
tunnel structure still wins ordinary reversed-depth testing. Applying priority
through `SV_Depth` leaves homogeneous clipping, screen position, raster
coverage, and perspective-correct interpolants untouched, so close artwork
cannot be clipped away by the priority rule. This rejects a measured
false-positive road slab at 177--182 percent of its supports and full-road
replacements above 1,700 percent while retaining the smaller lane, arrow, and
grid artwork.

The first-hairpin acceptance capture identifies the affected course model as
`0x800E4D80`. Its overlay audit changes from zero to nine untextured overlays,
and an isolated diagnostic pass shows the complete dash as one coherent surface.
The unfiltered moving capture was then inspected sequentially for every frame
from 510 through 810: the rectangles enter, grow, translate, and leave the view
without a frame in which asphalt replaces any visible portion. A full 3,600-
frame lap additionally retains the same category pass for the later yellow road
lines; the rule contains no Seattle object, model, material, or color identity.

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

## External 4x texture assets

The fixed 4x scene rasterization and the optional texture pack are separate
operations. A normal 320x240 GT2 race is rasterized directly into a 1280x960
3D target. That adds geometry, edge, depth, and sampling precision but does
not add authored texture detail. When installed, the external pack instead
replaces decoded PS1 texture regions with 4x Real-ESRGAN assets before those
assets are sampled by the 4x renderer.

The replacement path follows the loose-asset approach proven in the local
Vigilante 8: 2nd Offense recomp, but identity comes from each exact CPU-to-VRAM
image upload rather than a shared VRAM page. Every unique authored bitmap is
therefore one independently upscaled file; archive aliases do not duplicate
it. Course textures may use a small live-CLUT color fit for dynamic lighting.
Cars retain GT2's native indexed-bitmap and live paint-bank architecture: one
paint-neutral 4x detail asset is generated for each unique day/night bitmap,
then its bounded luminance detail is applied to the palette color selected by
the game at runtime. Paint choices and track-light palettes are never baked
into DDS files. The native D3D11 backend loads a format-7 loose-DDS
`manifest.json` from `mods\enhanced_textures_4x`, or from
`OPENGT_TEXTURE_PACK_DIR` when set. It also accepts format 5 and 6 packs for
compatibility, but rejects the old page-dump formats.

Missing identities fall back per primitive to the exact live PS1 VRAM texture.
Visibility, transparent word-zero discard, and STP blending continue to come
from the original BGR555 word; neural output supplies RGB detail only.

Build stock assets directly from the materialized GT2 volumes. Runtime dumps
remain diagnostic only and cannot be used to build a pack:

```powershell
python tools\build_gt2_texture_pack.py `
  --volume work\gt2-unified\simulation.materialized.vol `
  --volume work\gt2-unified\arcade.materialized.vol `
  --course-filter tahiti_t `
  --car-filter ccrcn --car-filter t002n `
  --output work\gt2-texture-pack\tahiti\enhanced_textures_4x
```

The builder extends edge pixels around every source region, extends visible RGB
beneath transparent word-zero borders to prevent dark filtering halos, and
restores the original nearest-neighbor STP mask after inference. The selected
`realesr-animevideov3-x4` model preserves low-resolution road and vegetation
structure more reliably than the sharper general/anime model on the available
GT2 assets. A downsampled round-trip fidelity gate rejects altered natural
textures. A reviewed text, signage, or decorative bitmap may cross that limit
only through an explicit per-key exception recorded in the manifest; it still
must have a verified Real-ESRGAN output and cannot fall back to a resize.

Every run hashes the executable, model graph and weights, padded inputs, neural
outputs, cropped previews, and final DDS files. Reuse is allowed only when the
entire provenance record matches. The pack writes one ordinary uncompressed DDS
and one padding-free `cropped_neural_png` preview per bitmap, and validates that
no identities share a file. Generated GT2 assets remain under ignored `work`,
`bin`, or `artifacts` paths and are never committed. Runtime logs report the
pack path, GPU cache size, hit coverage, and matched palette-native car
bitmaps. Upload tracing also sees UI and font-sheet uploads, but screen-space
replacement stays disabled until the final UI-specific sweep validates those
assets separately.

## Live PC integration

Race and replay presentation use a bounded native C ABI bridge. Release builds
embed that bridge in the single-file `GranTurismo2PC.exe`; the .NET host
extracts it into its private runtime directory when needed, so the distributed
package has no loose renderer DLL. RecompOne records the newest complete world
frame directly into one of three reusable OGTWCAP v4 buffers, including exact
provenance and a complete VRAM snapshot. A persistent native worker consumes a
bounded two-capture FIFO, builds the same draw list/topology used by the
standalone viewer, and renders through D3D11 without filesystem traffic or a
per-frame device rebuild. The FIFO absorbs one short scheduling overrun; older
work and obsolete published buffers return to their fixed pools. The first-ever
world submission uses one bounded 100 ms seed barrier so ownership cannot
precede the first validated native texture. It completed in 11.25 ms in the
retained cold-start proof. Every later submission and reset is asynchronous,
so emulation does not acquire a recurring renderer wait.

Native world output replaces only the live 3D region. Explicit screen-space
triangles carry the PS1 HUD, tachometer, minimap, text, and other race/replay
overlays through the same composition. Ordinary polygons without GTE
provenance are retained as screen primitives, which includes the tachometer
needle and redline wedge. GT2 emits the thin tachometer and turbo line layers
on alternating screen-only frames, so the recorder retains that bounded line
layer and appends it to the next world frame. Menus, loading screens,
world-free display transitions, Results, and MDEC video remain owned by the
authored 2D GPU command compositor; that path cannot rasterize a shipping 3D
world.

The reusable output pool is sized for a 640x512 PS1 display at the fixed 4x
native scale, preventing a late `invalid_argument` failure while keeping
allocation bounded. Its queue absorbs dense-object timing variance without
exceeding the native stream's short recency bound. Modern-world ownership is
provenance-based rather than restricted to GT2's usual 320x240 race viewport.
Wider showroom, transition, and rotating-car draw areas are submitted whenever
they contain authored 3D provenance and fit the bounded pool. A world-free
Results or menu frame is deliberately composed by the authored 2D layer.

### Genuine authored NTSC update rate (current authority)

The shipping GT2 path runs the race engine once per NTSC VBlank. Both Arcade
and Gran Turismo overlays set GT2's authored race time step and scheduler wait
from two fields to one. Input, AI, the force/contact solver, collision,
position and rotation integration, camera, timers, effects, and replay advance
on every field. Velocity-to-position shifts and force-derived state deltas use
the shorter interval; signed remainder carry prevents repeated half-step
rounding from changing long-term speed or distance. The mode therefore
preserves the original real-time game rate while producing a newly simulated
state at approximately 59.94 Hz.

Each authored race/replay setup now also resets the host's signed half-step
carry unconditionally. GT2 commonly rebuilds the replay car array at the same
guest address and with the same car count as the live race; those identifiers
therefore cannot distinguish two physical simulations. Allowing the final
race field's carry to survive that boundary changes the replay's initial fixed-
point state and can grow into a different trajectory.

Automatic replays are a runtime oracle rather than visual-only evidence. The
original guest encoder and decoder remain authoritative. A bounded host
observer records their exact five-byte control samples, compares every playable
decoded sample, and compares player position, velocity, speed, progress, and
heading at the matching solver field. The structured log reports the first
control or physics mismatch, exact stream hashes, final comparison counts, and
effective wall-clock replay frequency. It emits one progress line per 600
replay updates, so a slow replay can be diagnosed without screenshots or input
tracing. GT2 intentionally uses the final encoded sample as its terminal
sentinel; an N-sample recording therefore has N-1 playable comparisons.

The non-interactive `--verify-replay-codec` check calls both original generated
guest codecs directly with control changes, runs, and packed nibble values. Both
Simulation and Arcade variants must round-trip every playable sample exactly.

This is the only packaged and development runtime mode. The former
`RECOMPONE_GT2_TRUE_60HZ` downgrade is absent; retired midpoint reproduction is
compiled only into explicit native development-oracle targets.

Live presentation is authored-only in this mode. Native capture API v6 submits
one independently authored image per update through an asynchronous D3D11
readback queue. Pixels and telemetry share one FIFO identity; while the queue
primes, `OPENGT_LIVE_STATS_NO_OUTPUT` prevents the runtime from publishing a
new frame with stale pixels. Synthetic attempts, midpoint generation, and
repeat substitution remain zero. When an authored world segment ends, any
undrainable tail stays private and ownership returns to GT2's 2D compositor;
the last race or replay image is not relabelled as a new frame.

`tools/test_modern_renderer_scenario.ps1` now sets True60 on its child process
itself. It cannot silently inherit or omit the mode. Every complete world
window must contain exactly 300 actual frames, zero synthetic frames, zero
repeats, and zero world misses. It also rejects sustained cadence outside
59.5-60.5 Hz, resource exhaustion, invalid Maximum LOD, non-headless audio, a
changed memory card, or an unclean exit. Full-resolution video remains visual
and identity evidence only because synchronous recording can reduce measured
host cadence.

Current acceptance evidence, all on stock tracks and explicitly excluding
SSR11, is. The final cadence and identity runs v222-v225 use the exact rebuilt
shipping package whose default is described above:

- `artifacts/gt2-true60-parity-samples-v129` and
  `artifacts/gt2-true60-force628-parity-v137`: matched stock/True60 wall-time
  checkpoints retain the same race timer and track/opponent phase; the final
  one-field force/integration path remains within one displayed mph while
  advancing the complete solver on every field;
- `artifacts/gt2-true60-tahiti-final-v223`: Arcade/Tahiti Road, 19 complete
  windows, 0 misses/repeats, 59.961 Hz aggregate, 59.920-60.125 Hz adjacent
  pairs, and 11.643 ms pipeline p99;
- `artifacts/gt2-true60-red-rock-final-v222`: Gran Turismo/Red Rock race,
  replay, Results, and both ownership handoffs, 8 complete windows, 0
  misses/repeats, 17 bounded transition holds, 59.941 Hz aggregate,
  59.915-59.960 Hz adjacent pairs, and 13.087 ms pipeline p99;
- `artifacts/gt2-true60-replay-final-v224`: 300/300 unique authored replay
  presentations with no adjacent duplicate;
- `artifacts/gt2-true60-replay-exit-final-v225`: 391/391 unique replay-owned
  presentations and no adjacent duplicate before the normal compositor
  handoff in a complete 1,800-frame capture; and
- `artifacts/gt2-true60-tahiti-motion-v205/gulf-audit`: literal sequential
  inspection of frames 180-479 (about 0:03-0:08), with both GULF signs stable
  on every even and odd authored frame.

The native CTest suite passes 7/7, and the managed policy/source-contract suite
passes the capture-v6 identity, bounded ownership, pacing, memory fast-path,
and projection-origin contracts. The Tahiti lower-road rectangle is also
present in the 320x240 PS1-compatible oracle and is classified as an authored
mesh/UV boundary in
`artifacts/tahiti-road-patch-history-v210/inspection.txt`; it is not a True60,
topology, depth, upscale, or video defect.

### Retired midpoint pipeline (historical evidence only)

The following midpoint discussion records the earlier 30 Hz development path.
It is retained to explain old artifacts and regressions, but it does not
describe the current shipping architecture or current acceptance criteria.

GT2 advances race simulation and emits world geometry at 30 Hz even though the
host presents at 59.94 Hz. The live worker now renders two ordered images for
each consecutive pair of complete authored states: the prior authored state
and a geometry-aware midpoint. It matches complete object transform groups,
then moves their vertices by authored model-coordinate identity. Vehicle body
and wheel groups therefore retain one coherent silhouette instead of becoming
two blended cars. HUD and screen-space commands, material/VRAM state, command
membership, visibility changes, and unmatched objects remain atomic at the
prior authored state until the next actual state. Physics, input, timers, and
guest scheduling remain at their authored rate.

The rejected 50/50 whole-frame midpoint remains forbidden: direct evidence
showed two translucent silhouettes for moving vehicles. A cross-fade is not a
valid 60 FPS frame-generation method and cannot be reported as unique motion.
The pair bridge labels authored and interpolated output independently and
resets on temporal/settings discontinuities. A reset publishes its current
authored frame immediately. The next pair labels its staged carry-forward as a
repeat before returning to ordinary authored/midpoint output; it can never be
misreported as unique 60 Hz motion.

Performance telemetry reports authored, interpolated, repeated, pure-2D
compositor, one-vblank scene-transition holds, and missing-modern-world
presentations separately; a repeated texture or legitimate 2D screen cannot be
counted as a unique world frame. Ownership tolerates GT2's one world-free GPU
presentation between authored 30 Hz states, so the geometric midpoint remains
eligible. Two consecutive world-free presentations make the authored 2D
compositor authoritative. Once that handoff occurs, even a late queued native
image is rejected because it belongs to the prior segment. At the start of a
new discontinuous 3D segment, one explicitly labelled transition hold may keep
the last authored 2D screen while native seeds the new pair; it may never show
the previous race/replay texture. Any additional delay is a real world miss.

The D3D11 backend caches its currently bound scissor, blend, and depth/stencil
state. It omits only byte-for-byte redundant API calls; five reset/actual/
midpoint reference PNGs remain SHA-256-identical before and after this change.
Geometry, topology, maximum LOD, full authored distance, texture filtering, and
output scale are unchanged.

Steady midpoint rendering also reuses the immediately preceding authored
VRAM/material uploads, but only after an exact per-command proof: command
count/order, every material field, object kind, and all model-space triangle
coordinates must match. Any mismatch takes the normal upload path, and newly
allocated GPU resources force initialization regardless of the hint. The
16-capture road/wheel sequence records upload reuse on all 15 steady pairs;
all 31 actual/midpoint PNGs are byte-for-byte identical to the pre-optimization
reference. Pair p99 fell from `27.560 ms` to `25.783 ms` without altering
geometry, filtering, distance, LOD, or presentation semantics.

The rejected authored/midpoint proof pair is retained in
`artifacts/geometry-gap-pixel-midpoint-proof` as regression evidence for the
ghosting failure. `artifacts/no-ghost-frame-006000.png` proves the clean authored
presentation after removing that path.

The D3D11 path supplies material state and command identity through per-command
structured/vertex data, allowing compatible opaque and transparent triangles to
share a draw even when their textures differ. Vehicle body and wheel transforms
remain distinct authored depth layers. Track sections instead share one modern
depth surface inside each ordering-table layer; this removes the object-boundary
visibility resets responsible for distant roadside twitch while preserving the
authored ordering-table contract. A two-resource staging ring reads the completed
preceding 1280x960 image before submitting the next draw/copy, so command ordering
does not accidentally serialize the new render ahead of the old readback.

The current bounded live proof at
`artifacts/modern-renderer-unique60-arcade-soak-v2/stderr.log` enters an Arcade
race, engages native AI, drives through input poll 12,000, and exits normally.
From poll 5,100 onward every 300-vblank window contains 150 authored plus 150
interpolated images with zero repeats or fallbacks. Unique presentation stays
between `59.57` and `59.98 Hz`; the worker produces 3,838 images of each kind
with three obsolete startup drops and no native disable, exception, unmapped
call, or crash. Its sampled pair p95 remains below the `33.37 ms` authored-state
budget. `artifacts/modern-renderer-live-visual-proof-moving-v1` contains eight
consecutive presented frames whose trace alternates interpolated and authored
images; the captures retain single vehicle silhouettes, contained wheels, and
continuous road geometry.

Gran Turismo Mode is covered by
`artifacts/modern-renderer-unique60-simulation-purchase-race-v1`: the fixture
creates a licensed test state, buys and upgrades a Mazda, enters Sunday Cup,
and produces 3,080 authored plus 3,080 interpolated frames. Nineteen complete
race windows contain 150/150 images with no repeat or fallback. One five-second
track interval in that first run averaged `56.47 Hz` while measured guest time
rose to `16.12 ms`; no native output was lost. The exact route and interval were
repeated at `artifacts/modern-renderer-simulation-pacing-repeat-v2`. All 19
complete repeat windows hold `59.89-59.98 Hz`, including `59.94 Hz` at the
formerly slow poll 12,300 window, with exactly 150 authored plus 150
interpolated images and no repeat/fallback. The original reading is therefore a
non-reproducible host scheduling transient, not a persistent track or renderer
bottleneck. The imported SSR11 night soak at
`artifacts/modern-renderer-unique60-ssr11-night-soak-v1` supplies a third track
and content path; all post-throttle complete windows remain unique with no
repeat or fallback. A separate imported GT1 Supra/Tahiti Road run at
`artifacts/modern-renderer-unique60-supra-tahiti-soak-v1` adds a different
vehicle body and palette: 23 complete windows contain 150/150 unique images,
zero repeats/fallbacks, and `59.89-60.21 Hz` presentation.

Natural replay coverage is retained at
`artifacts/modern-renderer-unique60-replay-soak-v3`. The deterministic fixture
buys and upgrades the Mazda, completes its race, enters GT2's reconstructed
replay controller at poll 15,840, and remains in native replay presentation
until the normal transition near poll 23,280. Every complete replay window is
150 authored plus 150 interpolated images with no repeats or fallbacks and
approximately `59.67-60.18 Hz` unique presentation. Shutdown reports 7,719
images of each kind, one startup drop, no renderer disable, unmapped call,
exception, or crash, and an exact memory-card hash restore.

The final bounded visible auto-drive run is
`artifacts/modern-renderer-final-visible-proof-v2`. It launches the native
Arcade guest directly, uses the Enhanced preset (`Stabilized` seams, `Extended`
draw distance, `Maximum` LOD), engages GT2's native AI at poll 4,317, and exits
normally at poll 12,000. Its 24 complete live windows each contain exactly 150
authored plus 150 interpolated images with no repeats or fallbacks; unique rate
is `59.86-60.00 Hz` (`59.939 Hz` average). Shutdown reports 3,837 images of each
kind, four obsolete startup/queue drops, and no renderer disable, unmapped call,
exception, rejection, or crash.

For visual review, the presentation recorder now accepts an explicit
`RECOMPONE_VIDEO_FPS=60`; its historical default remains 30 FPS and still skips
alternate host presentations. The default regression at
`artifacts/video-capture-default30-regression-v1` is exactly five seconds,
30/1 FPS, and 150 frames. The modern-motion proof at
`artifacts/modern-renderer-unique60-motion-video-v2` is H.264 High, 640x480,
60/1 FPS, 30 seconds, and 1,800 frames. Decoded-frame hashing finds 1,800 unique
hashes and zero adjacent duplicates. The 12 FPS motion sheets covering seconds
12-16 and 22-26 retain continuous road surfaces, contained opponent wheels,
single car silhouettes, and stable roadside geometry throughout passing and
distance transitions.

The recorder performs synchronous D3D11 staging readback and encoding; even at a 640x480
presentation target it reduces the instrumented host rate while active. The
video therefore proves motion continuity and absence of duplicate recorded
images, but it is deliberately not used as the real-time pacing benchmark. The
unrecorded visible and headless runs above are the authoritative 59.94 Hz
pacing evidence.

The final race-to-Results validator is
`tools/test_modern_renderer_full_path.ps1`. The current modern-only authority at
`artifacts/modern-renderer-native-results-soak-v34-independent-confirmation-final`
buys and upgrades the Mazda, completes the race, runs the natural replay in
real time, crosses two discontinuous 3D/2D Results transitions, and exits
normally. All 49 complete steady world windows contain exactly 150 authored
plus 150 geometry-aware midpoint images, zero repeats, zero pacing violations,
and zero missing-modern-world presentations. Two reset-only warmup
presentations are labelled repeated, 845 presentations exercise authored
Results composition, and the test restores the card to SHA-256
`78B6D4AC9AB4D23CAF7E5F04F83539BF5D994CCCFB0A709D14AC53D05C8E21EF`.
No disable, rejection, truncation, unmapped call, fatal error, or unhandled
exception marker appears. Shutdown accounts for zero pending-capture drops,
zero output-pool starvation, one bounded 0.179 ms output wait with no timeout,
and only 321 obsolete published images discarded at ownership changes. The
independent preceding run at
`artifacts/modern-renderer-native-results-soak-v33-latched-release-pacing-final`
adds 51 consecutive complete world windows at `59.80-60.11 Hz`, also with zero
world misses and zero pending/output-pool drops.

The v34 artifact records an aligned rolling profile over the
last 240 track-world pairs; menus, showroom, and Results screens cannot dilute
the topology component. Pipeline p50/p95/p99 is
`23.766/28.650/31.057 ms`, GPU render/readback is
`15.755/18.959/19.912 ms`, and topology is `4.591/6.703/7.853 ms`.
Pipeline p99 remains below the `33.37 ms` authored-state budget. The harness
rejects absent/zero topology samples, unordered percentiles, or a pipeline p99
above its 40 ms hard tail ceiling; the stricter per-window 59.5-60.5 Hz audit
proves that a bounded pair-production tail cannot slow or duplicate output.

That run is also the first end-to-end proof after removing compatibility-world
rasterization. Provenance-backed triangles are classified before the native
worker's enabled state is consulted and are sent to neither the GL-HLE nor
software rasterizer. Screen-space GPU commands remain available for authored
2D composition. Five requested final-presentation checkpoints were written at
polls 7,400-8,000; no obsolete raw-framebuffer capture was produced, so a later
world-free probe cannot overwrite a valid modern image. The checkpoints cover
race finish, black handoff, Results car, and two Results-detail states.

That same run carries a deterministic renderer audit through 7,768 race and
3,717 replay visibility calls. GT2's unmodified visibility output requested a
lower-detail selector in 96,296 of 177,963 entries; after the modern-renderer
Maximum conversion, all 177,963 emitted track/scenery entries use selector `0`
(highest), with zero nonzero selectors, null lists, or invalid lists. All 62,661
vehicle requests use selector `1`, GT2's one-based selector for model-table
index `0` (highest), across seven model sets. The focused control at
`artifacts/modern-renderer-lod-audit-v3-fixed` independently converts 5,992
lower-detail requests among 8,988 raw entries to 8,988 highest-detail entries
and sends all 2,580 vehicle requests to selector `1`.

`tools/test_modern_renderer_realtime_startup.ps1` supplies the independent
cold-start check. The current-tree run at
`artifacts/modern-renderer-realtime-startup-v5-final-profile`
starts at normal NTSC pacing with no pre-warmed native texture, records 192
native-world and 2,199 authored-compositor presentations, records zero world
misses, dumps the first showroom world, exits cleanly, and restores the same
card hash. The standalone render of that dump is retained under its `rendered`
directory and shows the complete showroom car and contained wheels.

The current final visible Arcade auto-drive is
`artifacts/modern-renderer-final-visible-review-v5-upload-reuse`. After the
mixed compositor-to-world ownership window, all 11 complete race windows
contain exactly 150 authored plus 150 geometry-aware midpoint presentations,
zero repeats, and zero world misses at `59.90-60.19 Hz`. This includes the
former poll-6,300 failure interval and continues through poll 7,800. Sampled
native pair p99 remains `29.346-30.193 ms` after warmup, below the `33.37 ms`
authored-state budget; shutdown at poll 8,000 is orderly with no disable,
rejection, truncation, unmapped call, fatal error, or unhandled exception.

Latest-build visual evidence is split by game mode. The Arcade capture at
`artifacts/modern-renderer-final-motion-arcade-v2-final-profile` and Simulation
capture at `artifacts/modern-renderer-final-motion-simulation-v2-final-profile`
are each 30 seconds at 640x480 and 60 FPS. Frame-level decoded hashes prove all
1,800 frames in each video are unique with zero adjacent duplicates. Contact,
motion, and low-adjacent-SSIM outlier sheets were inspected: the Arcade minimum
SSIM (`0.744751`) is a legitimate high-speed corner and the Simulation minimum
(`0.842946`) is the start-line checker shadow/close overtake. Neither sequence
reveals a one-frame wheel, road, scenery, silhouette, or prop failure. These
captures are visual/temporal evidence; their synchronous readback and encoding
cost means the unrecorded runs remain authoritative for real-time pacing. Final
closure still requires the user's visual acceptance of these runs.
The videos' SHA-256 values are respectively
`FA54260F9E35AF11789A403F9EF234FBF0D2E4C109DABA977D02D78360289C89` and
`E763BF5494159565DE40943A2BF2686249E5C16C5F6F23E4AF731D17BD1B2EF6`.

Three additional exact-current captures extend that review across imported and
replay content. `artifacts/modern-renderer-final-motion-ssr11-v1-final-profile`
uses the imported SSR11 night track, while
`artifacts/modern-renderer-final-motion-supra-tahiti-v1-final-profile` uses the
imported GT1 Supra body and palette on Tahiti Road. The natural-replay capture
at `artifacts/modern-renderer-final-motion-replay-v2-final-profile` positively
logs the `replay_1` stage before recording. Each is 30 seconds at 640x480 and
60 FPS, contains 1,800/1,800 distinct decoded frames, and has zero adjacent
duplicates. Their adjacent-frame SSIM min/p1/median values are respectively
`0.824554/0.860576/0.949322`, `0.819872/0.837272/0.946042`, and
`0.509155/0.661939/0.897556`. Review of the contact, dense-motion, and temporal
outlier sheets finds no wheel displacement, split silhouette, road/world gap,
or prop-distance failure. Replay's lower outliers are genuine camera cuts and
close passes. In particular, 64 consecutive frames 720-783 were inspected
through a silver car's complete near-camera entry, occlusion, and exit: both
authored and midpoint frames advance coherently, the wheels stay attached, and
there is no even/odd geometry toggle. The videos' SHA-256 values are
`0A22DE9FC86306E1D4B99C626AA7FF603887CA8D55C23DCDC10AC6C80E59222E`,
`4E6EA6E413A42135F2B6857242F3FFC4E484FE3B6E361FA878442F12AFE754A9`, and
`EB5FCF8E9832E2E1AEC4C58894F0F42C32B73DA9B977C19AE3BCEFCAB50279EE`.

The dedicated replay-to-Results proof at
`artifacts/modern-renderer-replay-exit-v6-output-ring-latched-final` extends the recording
through the ownership handoff. The harness over-captures the recorder's bounded
three/four-poll startup variation, trims to exactly the first 1,800
presentations, and verifies a 30.000-second 60 FPS stream. All 740 replay-owned
frames are distinct with zero adjacent duplicates. The full video has 1,727
distinct frames and 51 adjacent duplicates, all after replay relinquishes the
world to intentionally static Results/loading composition. Consecutive-frame
inspection proves the transition is Results title directly to the rotating car;
the prior one-frame stale replay flash is gone. The title's short horizontal
letter trail is authored GPU-command animation: its transition frames are
individually distinct and never pass through host whole-frame blending. The
video SHA-256 is
`18FFEB8C47E0987CBBA00F39CA95DC1A92E87A869E38405A7ACE414503310620`.

PS1 UV rules differ across those two classes. Authored 3D polygons sample
integer UVs as texel centers with `floor(uv + 0.5)`, which fixes the Red Rock
0:33 road fault. Screen-space sprites retain edge-based `floor(uv)` sampling,
preventing the minimap and tachometer from reading one texture column beyond
their rectangles.

The live recorder exposes logical `MemoryStream` storage before directly
encoding fixed-stride triangles into its public backing array, reserving in
bounded 64-record chunks. It likewise grows the stream before copying VRAM.
This ordering is required because `SetLength` zero-fills newly exposed bytes;
copying first would erase triangle semantics or the complete VRAM snapshot and
produce an empty/sparse native frame. The configuration regression writes
sentinels at both ends of a direct record and proves they survive frame-final
stream sizing.

Test-only fast-forward pacing is also latched. Once the requested scripted
stage is reached, later stage names cannot silently return the host to unlimited
speed. The regression covers setup -> race -> replay explicitly; the v33/v34
soaks then prove the release-paced world remains at 59.5-60.5 Hz across that
same transition.

Historical packaged validation in
`artifacts/native-track-surface-corrected-final-v3` reaches input poll 45,100
through race, natural replay, the deliberate 352x300 authored GPU 2D Results
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
confirms the complete authored GT2 UI after the native handoff. This older
artifact is retained for regression history; the v10 soak above is the current
ownership and pacing authority.

## Final modern-only validation

The August 15, 2026 validation uses the ReadyToRun host in
`artifacts/modern-renderer-r2r-v82/publish`. It retains the complete modern
quality contract: 4x source geometry, perspective-correct and smoothed
textures, topology repair, full authored visibility distance, Maximum
track/scenery and vehicle LOD, and no compatibility-world fallback.

- `artifacts/modern-renderer-extended-soak-final-v84` runs eight consecutive
  strict, silent/headless auto-drive scenarios for 20 minutes: Arcade three
  times, imported SSR11 three times, and the imported GT1 Supra on Tahiti Road
  twice. All eight exit normally. Their 143 complete steady world windows have
  zero repeated presentations and zero world misses. The worst aggregate p99
  times are `28.134 ms` pipeline, `11.238 ms` GPU, and `12.396 ms` topology,
  all within the `33.37 ms` authored-pair budget. Every run retains exact card
  hash `78B6D4AC9AB4D23CAF7E5F04F83539BF5D994CCCFB0A709D14AC53D05C8E21EF`.
- The current queue/readback durability pass removes a synchronous GPU stall
  from pair generation. D3D11 maps the staging slot being replaced, which was
  submitted one complete authored pair earlier; the bridge delays matching
  midpoint/actual provenance by the same amount. The native worker uses normal
  thread priority, and no-capture headless runs skip presentation readback and
  encoding while continuing to consume, audit, and time every D3D11 output.
- A 125-second sampled trace at
  `artifacts/modern-renderer-guest-cpu-long-v40` identified a test-only load:
  SDL's dummy sink spent about 63 seconds of managed CPU mixing muted audio
  with no consumer. Silent renderer soaks keep the verified dummy device open
  but leave its mixer inactive. Audio-capture requests still start the mixer;
  `artifacts/modern-renderer-headless-audio-capture-smoke-v44` records 19.354
  seconds of 44.1 kHz stereo PCM and shuts down cleanly.
- Fresh ReadyToRun validation at
  `artifacts/modern-renderer-r2r-silent-mixer-off-soak-v42` gives Arcade,
  imported SSR11, and GT1 Supra/Tahiti 20, 17, and 16 perfect five-second world
  windows. All 53 are unique 59.5-60.5 Hz output with zero repeats, zero world
  misses, Maximum track and vehicle LOD, clean shutdowns, and exact card
  restoration. `artifacts/modern-renderer-r2r-silent-arcade-repeat-v43` follows
  with four fresh-process Arcade runs and 80/80 additional perfect windows.
  The native suite remains 6/6 and the managed modern-only configuration suite
  passes.
- The distributed lossless audits in
  `artifacts/modern-renderer-lossless-supra-lap-v70` and
  `artifacts/modern-renderer-lossless-ssr11-lap-v71` sample 15 authored states
  per course at 60-state intervals. All 30 exact 640x480 renders contain zero
  pixels of the impossible magenta clear sentinel anywhere in the frame. This
  is a full-frame geometry-hole test rather than a selected road crop.
- `artifacts/modern-renderer-final-motion-ssr11-v85` first exposed a brief
  bridge-light streak. Adjacent raw projected captures identify it as an
  offscreen textured street-light billboard crossing the PS1 signed 11-bit
  screen-coordinate boundary. One vertex wrapped from about `-1025` to
  `+1023`, turning the half-quad into a full-height polygon. The modern draw
  list now applies the same `1023x511` polygon-span rejection used by GT2's
  compatibility GPU before accepting explicit screen primitives. The exact
  wrapped triangle is retained as a native regression test.
- `artifacts/modern-renderer-ssr11-horizon-audit-v101` is the accepted
  replacement 30-second motion proof. It contains `1,800/1,800` unique
  640x480 frames with zero adjacent duplicate. Its luminance delta p99/max is
  `2.5682/4.0407`; the rejected streak sequence peaked at `28.2055`. Contact,
  motion, and temporal-outlier sheets preserve every constituent screenshot
  at 640x480, producing `3200x1440`, `3840x1920`, and `2560x1440` evidence.
  Inspection finds no street-light streak, false road surface, terrain hole,
  detached wheel, or double vehicle silhouette.
- A later user review rejected that visual closure after observing several
  small SSR11 polygon explosions. The retained 5,400-frame reproduction at
  `artifacts/modern-renderer-ssr11-polygon-explosion-capture-v122` proves the
  defect occurred only on generated midpoint frames. Exact adjacent authored
  packets in `artifacts/modern-renderer-ssr11-polygon-source-captures-v123`
  identify stale near-camera track commands from objects 74/75 (models
  `0x800B9878`/`0x800B9960`): whole-group rigid reprojection enlarged triangles
  already culled from the next authored visibility list into screen-covering
  wedges. Track interpolation now uses exact per-command provenance and holds
  unmatched track triangles at the prior midpoint pose; articulated vehicle
  groups retain rigid interpolation.
- The exact 26 failing source pairs rerender cleanly at
  `artifacts/modern-renderer-ssr11-polygon-source-captures-v123/midpoints-fixed-v124`.
  The live 90-second replacement at
  `artifacts/modern-renderer-ssr11-track-visibility-proof-v126` contains
  `5,400/5,400` unique 640x480 frames and zero adjacent duplicate; maximum
  adjacent luminance change falls from `16.6334` in the rejected run to
  `4.046`. `previous-failure-frames-fixed.png` preserves the formerly bad
  frame indices at native detail.
- Extended distance now combines the current authored visibility set with a
  bounded three-sector horizon in both directions. It deliberately does not
  restore the rejected all-sector union, which exposed mutually exclusive and
  occluded road surfaces. In v101, every one of 23 stock appearance events was
  already resident before its original boundary and all 21 stock removal
  events remained resident afterward. The longer silent/headless SSR11 proof
  at `artifacts/modern-renderer-ssr11-horizon-long-v102` covers 5,304
  visibility calls and 87 sector transitions: all 96 stock additions are
  precovered and all 90 removals retained. Its 17 complete paced windows hold
  `59.73-60.07 Hz` with zero repeats/world misses; the worst late-run pipeline
  p99 is `28.566 ms`, and all visibility lists and bounded renderer resources
  remain valid.
- `artifacts/modern-renderer-final-host-simulation-replay-v86` covers the full
  Gran Turismo Mode purchase/race/Results/natural-replay path. All 50 complete
  paced world windows are perfect with zero pacing violation or world miss;
  the 2D Results compositor owns 893 presentations, then replay returns to the
  native world without stale ownership. Maximum LOD records 7,768 race and
  3,717 replay calls over 177,963 entries, with no lower/null selection. Its
  p50/p95/p99 profile is `17.772/20.169/22.146 ms` pipeline,
  `7.930/9.442/11.352 ms` GPU, and `6.067/7.765/8.816 ms` topology. Shutdown is
  clean, output waits/timeouts are zero, and the card restore is exact.
- The corrected self-contained ReadyToRun package is
  `artifacts/modern-renderer-track-visibility-r2r-v131/publish`, with native
  renderer SHA-256
  `47349A418581570BC264862F11684CE91F3FE9A668B6A582A11DFA20C46209A4`.
  Its strict SSR11 run v132 passes 17/17 complete world windows. The final
  15-minute round-robin v136 runs two fresh processes each for Arcade, SSR11,
  and GT1 Supra/Tahiti. Its 106 complete steady windows have zero repeats or
  world misses, and every process restores the exact card hash. The full
  Gran Turismo Mode race/Results/natural-replay v134 passes 50/50 paced world
  windows, zero pacing violations, zero waits/timeouts, Maximum LOD throughout,
  and pipeline/GPU/topology p50/p95/p99 of
  `11.852/14.095/15.750`, `3.081/4.593/6.176`, and
  `5.187/6.307/6.823 ms`.
- `artifacts/modern-renderer-final-visible-review-v87` exercises the real
  visible GLFW swap path in an Arcade SSR11 auto-drive race. Its 25 complete
  steady windows each contain exactly 150 authored plus 150 geometry-midpoint
  images, zero repeats, zero world misses, and `59.89-60.00 Hz` unique motion.
  No `Host-Long-Swap` interval occurs. The final p50/p95/p99 profile is
  `17.265/19.687/21.397 ms` pipeline, `6.280/7.257/7.588 ms` GPU, and
  `7.227/9.250/10.393 ms` topology. It reaches programmed poll 12,000, tears
  down audio/native/presentation resources in order, and exits zero.
- The corrected visible replacement is
  `artifacts/modern-renderer-track-visibility-visible-ssr11-v135`. It launches
  normal (non-replay) SSR11 auto-drive from the v131 ReadyToRun package in a
  maximized window. All 16 complete steady race windows contain exactly 150
  authored plus 150 geometric midpoint frames at `59.91-59.97 Hz`, with zero
  repeats, compositor substitutions, or world misses. The final profile is
  `14.713/17.581/18.681 ms` pipeline, `4.628/6.160/6.400 ms` GPU, and
  `6.401/7.941/8.427 ms` topology; shutdown and card restoration are clean.
  User visual acceptance of this replacement remains pending.

The current tree builds with zero managed errors; its modern-renderer policy
executable passes every fixed-quality and no-fallback invariant, and all seven
native CTests pass. Aggregate scans of v84-v87 contain no crash, fatal,
unhandled exception, unmapped-call, or output-timeout signature. The earlier
v87 visual acceptance is superseded by the user's polygon-explosion report.
Automated validation of the corrected package is green; a fresh visible SSR11
auto-drive review of that exact package remains required.

`tools/verify_modern_renderer_closure.ps1` makes that boundary executable. It
rebuilds the current managed tree, reruns all seven native CTests and the
modern-only policy regression, performs a fresh ReadyToRun publish and
byte-compares both managed assemblies plus the native renderer against v131,
and validates every v136 soak log. It rescans all 30 lossless PNGs for exact
clear-sentinel pixels, requires all 64 authored explosion-source packets and
26 corrected 1280x960 midpoint pairs, and recomputes all 5,400 v126 frame
hashes, 5,399 SSIM pairs, and luminance deltas. It positively requires v122 to
reproduce the rejected `16.6334` signal before accepting v126's `4.046`, then
re-audits v132 horizon coverage, v134 Simulation/replay, and v135 visible-path
cadence, LOD, profiles, resources, and fault signatures. The retained final
report is
`artifacts/modern-renderer-closure-audit-v137/closure-report.json`; its state is
deliberately `machine-pass-user-visual-pending`, not complete.

## Texture projection

Race vertices carry model/world position and exact GTE view/projection state
through the native scene path. The backend performs homogeneous geometry
projection from those values. Course primitives captured from GT2's authored
model before guest projection carry the resident-course provenance flag. Their
model-space UVs always use hardware perspective interpolation, including after
homogeneous clipping of polygons that cross the camera plane. Applying the
former affine fallback to those polygons magnified a small asphalt tile across
the near road; the resident contract removes that corruption at its cause.

All textured 3D packet geometry uses the same hardware perspective contract.
Resident course surfaces carry authored pre-projection positions and UVs;
vehicles and other reconstructed packets carry exact model/view/clip state.
The renderer does not use depth-ratio, near-plane, temporal-seam, or per-island
affine escape hatches. Explicit screen-space sprites and HUD remain
screen-linear because they are two-dimensional artwork, not world-projection
fallbacks. The serialized perspective setting is migration-only and cannot
disable the fixed modern contract.

`OPENGT_RENDER_UV_DIAGNOSTICS=1` reports individual and island-level
perspective counts and requires world fallback counts to remain zero. The
upstream packet correlation trace is independently gated by
`RECOMPONE_TRACE_MIXED_PROJECTION_TRIANGLES=1`; start/end-poll and record-limit
variants use `RECOMPONE_TRACE_MIXED_PROJECTION_START_POLL`,
`RECOMPONE_TRACE_MIXED_PROJECTION_END_POLL`, and
`RECOMPONE_TRACE_MIXED_PROJECTION_LIMIT`. That trace includes packet address,
depth age/provenance, object/model identity, model and view coordinates,
transform ID, and projection state for every mixed-basis triangle.

The first complete shipping Seattle frame emits the same UV contract once
without enabling diagnostics. `worldFallback`, `trackFallback`,
`vehicleFallback`, and `otherFallback` must all be zero and the projection is
labelled `fixed-modern`. This makes an affine world regression observable in a
normal clean-room package run.

## World depth and effect layers

The native backend classifies each command into one of five explicit layers:
authored background, resident course, vehicle, unclassified 3D, and screen.
Resident course and vehicle geometry share one coherent modern depth surface
across ordering-table buckets; a bucket change does not clear depth. World
vertices use an infinite reversed projection (`clipZ=16`, `clipW=viewZ`) into
`D32_FLOAT_S8X24_UINT`, clear depth `0`, and compare `GREATER_EQUAL`. This
retains homogeneous clipping at the 16-unit near plane, removes the arbitrary
far plane, and preserves stencil for GT2's mask-bit behavior. The background
paints color without reserving world depth because
Seattle's camera-centred backdrop overlaps distant course view Z. HUD, video,
and presentation packets are a final screen layer. Any remaining ownerless 3D
packet stays visibly `unclassified`; it is never silently treated as sky or an
effect. A one-time shipping `Render-Batches` summary reports command, batch,
draw, transparency, and blend-mode counts for all five layers and records
`depth=track+vehicle`. A one-time `Render-Depth` line independently records the
projection, near plane, resource format, comparison, clear value, and stencil
width active in the shipping path.

A bounded CPU-driven Seattle replay capture at input poll 1420 reproduced the
reported turn-one road band with 15,492 triangles, including 12,315 course
triangles and 7,141 opaque course commands. Rasterizing those exact triangles
found 108 distinct opaque-road overlaps that conventional D24 mapped to equal
depth values in screen bounds `x=0..215, y=127..150`. One measured pair was
only `0.013244297` view units apart at roughly `Z=5465.5`. Infinite reversed
D32 reduced the same capture to two equality collisions, both from nearly
coincident authored surfaces. The native GPU regression uses that measured
road pair in reverse submission order; the nearer surface must remain visible,
which fails with the former D24 precision and passes with D32.

Background ownership comes from exact GT2 control flow rather than a screen-
shape or material heuristic. Arcade `func_800298E8` passes its fixed backdrop
model to `func_80018CA8`; Simulation `func_8002993C` passes its corresponding
model to `func_80018D1C`. Generated hooks bracket only those calls and attach
`WorldObjectKind.Background` to their GTE projections. Before that correction,
a 609-frame direct Seattle replay found the entire ownerless 3D stream on one
exact zero-translation transform, with 2..184 commands per frame and model
bounds forming the camera-centred backdrop ring. Development-only stack
attribution independently resolved the same Arcade call chain. The stack probe
and its environment controls are compiled out of release packages.

The corrected direct Seattle CPU replay produced 602 classified world frames.
Every one of the 563 frames in which GT2 submitted its backdrop contained
exactly one background group (`2..184` commands); no frame contained an
unclassified 3D command. That historical classification run used the later-
rejected full-course selection, but still established background ownership:
it reported zero secondary views, non-finite projections, decode failures,
guest course fallbacks, synthetic outputs, repeated outputs, or dropped native
outputs. Shutdown was clean after 600 actual rendered outputs; the two authored
no-output submissions were startup/pipeline state and were not synthesized.

That classification is now a whole-run invariant rather than a sampled log.
The draw-list core distinguishes explicit screen artwork from ownerless 3D
even though both use object kind zero in the capture format. The live bridge
accumulates every authored frame and emits `Native-World-Classification` at
shutdown; Seattle acceptance requires zero unclassified world commands, zero
frames containing one, and a zero per-frame maximum. HUD and other explicit
2D packets do not contribute to that failure count.

The guest backdrop packet list is still clipped to its authored 4:3 viewport,
so ownership alone cannot fill a true Hor+ target. The modern path now decodes
the fixed backdrop model immediately after GT2 installs its exact GTE transform
and before the guest's projection/cull loop. It validates the 353-vertex model
and all eight primitive streams, then submits all 257 authored primitives (514
triangles) to homogeneous target-aspect clipping. A 1,099-frame direct Seattle
CPU replay decoded the same 353/257/514 contract on every frame with zero
duplicates, failures, or guest fallback. At poll 1,420 both 214-pixel side
margins contained authored backdrop and zero clear-color pixels, while the
native 4:3 sky remained byte-identical. The replacement costs two opaque
background batches and does not reserve world depth.

The authoritative Seattle backdrop is an open cylinder, not a closed sky
dome. Its upper boundary is one authored 16-edge ring at model Y=3266; every
edge has the same untextured RGB 119/126/164 material. A high replay-camera
pitch can therefore look through that ring and expose the clear colour. Exact
capture geometry proves the visible aperture follows those source edges,
including their projected corners, at both 4:3 and 16:9. This is preserved as
authored geometry rather than hidden with a generated cap or clear-colour
substitution.

Seattle's distinct static-mesh effect table is empty: an audit of 367 meshes,
34,731 vertices, 16,666 source primitives, and 30,989 expanded triangles found
zero effect records and zero effect colors, with all pointer/index validations
clean. The renderer therefore does not invent a Seattle effect decoder from an
absent stream. Other gameplay effects remain ordinary captured packets until
their own authored provenance is demonstrated.

Projection-channel identity is based on the drawing target and projection
centre, not GTE focal length `H`. `H` remains per primitive for perspective
projection, but it does not change the meaning of view-space Z. Seattle's
native replay zoom demonstrated why this matters: the resident course receives
the new `H` before vehicles and sky, producing adjacent authored values from
909 down to 160. The old one-native-pixel tolerance classified 61 of 609
authored frames as separate depth channels even though every command retained
the same full-screen target and projection centre. The corrected run reports
zero secondary frames in the same 609-frame A/B. The extended corrected run
reports 0/1,459 secondary frames, zero incomplete/rejected course or vehicle
commands, 1,459/1,459 completed course-capture frames, and zero dropped native
outputs.
`OPENGT_RENDER_CLIP_DIAGNOSTICS=1` now includes frame/poll identity plus exact
transform and projection/draw-state ranges for each channel, so a future real
secondary view remains distinguishable from an intra-frame lens update. Its
`_START_POLL`, `_END_POLL`, and `_INTERVAL` suffixes permit an exhaustive trace
of a replay window without logging the direct-load setup and CPU-driven race.
`OPENGT_RENDER_PRIMITIVE_DIAGNOSTICS=1` reports camera/near-plane crossings,
target-frustum intersections, oversized spans, and non-finite commands. Its
`_START_POLL` and `_END_POLL` variants bound the trace to the suspect replay
window; `_INTERVAL=1` and `_VERBOSE=1` then provide per-frame and per-command
provenance without flooding the startup sequence.
`OPENGT_RENDER_VEHICLE_DIAGNOSTICS=1` accepts the same `_START_POLL`,
`_END_POLL`, and `_INTERVAL` suffixes. It reports each car's body, part, and
wheel transform groups only inside the requested replay window.
`OPENGT_RENDER_SCENE_DIAGNOSTICS=1` also accepts those three suffixes. This
keeps per-frame resident-course, projection-channel, clip-plane, and Hor+
margin evidence bounded to the same replay interval.
`OPENGT_RENDER_SCENE_TRANSITION_DIAGNOSTICS=1` adds only object-level additions,
removals, and resident-course command-count changes to the ordinary scene
summary. It is the focused continuity trace for intermittent prop loss; unlike
the broader scene-verbose switch, it does not emit every Hor+ margin group.
The authored-face trace can likewise be restricted to one model pointer with
`RECOMPONE_TRACE_GT2_RAW_TRACK_FACE_MODEL` (decimal or `0x` hexadecimal) while
retaining its existing poll range, interval, and record-limit controls.
`RECOMPONE_AUDIT_GT2_RAW_TRACK_TEMPORAL_FACING=1` compares stable source-
triangle identities across consecutive frames and reports face-state changes,
isolated ABA toggles, target overlap, and projected area. Triangles crossing
the near plane are deliberately excluded because the homogeneous-clip
diagnostics own those passages; this audit treats any fully in-front isolated
toggle reaching one native pixel as a macroscopic culling failure.
`RECOMPONE_AUDIT_GT2_RAW_TRACK_TEMPORAL_COVERAGE=1` clips every fully in-front
stable source triangle, including authored billboard signs and posts, against
the configured Hor+ target. It reports one-poll ABA coverage losses or spikes
of at least one native pixel while leaving near-plane passages to the
homogeneous-clip diagnostics.

No fog state, fog shader, or distance-fade path exists in the native renderer.
Distance visibility comes only from the resident course catalog plus continuous
modern frustum, backface, and homogeneous clipping decisions.

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
There is no screen-space triangle expansion.

After exact shared anchors prove that two source track objects are adjacent,
the pass may also join mutually nearest boundary vertices on the same material
and ordering-table layer when their projected separation is at most 0.75 native
pixel. This bounded second stage handles adjacent chunks with different
tessellation without allowing unrelated geometry to attract or cross a material
seam. The marked early-race road crop falls from 63 exposed background pixels
to zero and reports 28 projected seam groups with 52 adjusted instances.

Resident course objects normally bypass that repair because their model
topology is authoritative. A separate global rule handles the narrower case
where different resident objects are power-of-two LOD copies meeting at a
shared solid boundary in a Hor+ side margin. Both edges must be instance-local
boundaries with identical opaque material and ordering-table state, opposite
triangle interiors, endpoint view coordinates related by an exact 2/4/8/16x
scale within a four-GTE-unit residual, and projections no more than one-half
native pixel apart. The endpoints may share exact authored SXY or merely the
same continuous source-raster cells, which covers adjacent integer rounding
without using a track, object, model, address, texture, or color identity. The
far boundary is placed one-half native pixel into the near triangle and the
correction is propagated to every copy of each affected model vertex. The
rule is restricted to the horizontal margins and cannot alter the original
4:3 image.

Seattle Circuit exposed this as a one-pixel diagonal backdrop seam through
the right side of the overpass after the first hairpin. At the exact
105-second reproduction, command-ID readback found 72 backdrop-owned pixels
in the marked region before the rule and zero afterward. All 60 captures in a
separate 180-second pass were then inspected individually in chronological
order without recurrence.

The inspector reports all topology counters on stdout and can write one CSV
row per source triangle. Synthetic tests cover boundary copies, T-junction
subdivision, deterministic coplanar ownership, and exact projected-position
joins between adjacent authored sections.

The August 9 wheel/road regression added two modern-only cases. Vehicle bodies
and individual wheel transforms now retain their authored ordered depth
layers, so body depth cannot reject a wheel merely because GT2 reused one
model pointer for all five submissions. Track topology also retains authored
integer SXY alongside continuous projection. It uses that data only as proof
for same-object/model/transform/material road LOD copies and bounded
vertex-to-edge joins; it does not render through the PS1 path or expand
triangles. Power-of-two LOD-scale checks, a half-native-pixel limit, and
exact-vertex propagation prevent unrelated surfaces or adjacent materials
from tearing.

A later SSR11 normal-race report described intermittent white/gray rectangles
under a car as square exhaust backfires. Exact adjacent authored captures at
`artifacts/backfire-ssr11-pair-captures-v19` contain no such primitive; the
rectangle appears only in the generated midpoint. It was the opaque atlas
background exposed when a fast-rolling wheel/cutout group was inferred at an
un-authored intermediate angle, not a missing or incorrectly blended backfire
texture. The interpolator now retains the prior authored wheel roll whenever
the fitted step exceeds 45 degrees while still interpolating the rigid group's
translation. Whole-group rigid interpolation is limited to articulated vehicle
submodels. Track triangles use exact command provenance: matched triangles
interpolate between their authored endpoints, while a triangle absent from the
next authored visibility list remains at its prior midpoint pose and then
disappears on the next authored frame. This keeps wheels and small
bumper/exhaust geometry coherent without reprojecting a stale, already-culled
track triangle through the near field.

`artifacts/backfire-ssr11-pair-rerender-v20` rerenders the exact failing pairs
without a rectangular exposure. The fresh normal-race native-AI proof at
`artifacts/backfire-ssr11-autodrive-postfix-v21` is 640x480 at 60 FPS, has
1,800/1,800 distinct decoded frames and zero adjacent duplicates, and retains
16 consecutive full-resolution frames around the former failure. The no-video
release-paced companion at
`artifacts/modern-renderer-ssr11-autodrive-postfix-v22` records 17 complete
59.89-60.11 Hz world windows with zero repeats or world misses, Maximum LOD,
zero resource waits/timeouts, and clean shutdown. Dedicated native regressions
cover fast wheel roll, clipped vehicle-group membership, track visibility
churn, and alternating field origins.

The retained 16-frame sequence at
`artifacts/user-gap-wheel-repro-14s/transparent-batch-sequence` covers polls
5701-5731.
The reported frame-15 road region fell from nine exposed sky samples to zero;
the worst nearby LOD boundary fell from 151 to zero. A full sequence scan finds
no uncovered geometry. The final sampler also repairs isolated transparent
source texels only on opaque, upward-facing track surfaces and only when at
least three cardinal neighbors are opaque. That removes the remaining two cyan
road pixels without filling authored billboard/cutout regions. The same frames
show stable, complete wheels with depth enabled. All six native test targets
pass, including dedicated projected T-junction and 8x road-LOD regressions.

The topology pass now uses bounded screen-cell indices for projected seam and
vertex-to-edge candidates, plus a projected sweep for coplanar ownership. The
four retained 120-frame scale-4 benchmarks in
`artifacts/modern-renderer-optimized-bench` measure `31.400 ms` dense-distance,
`29.116 ms` dense-HUD, `17.510 ms` replay, and `18.435 ms` gap/wheel average
pipeline time. The original respective baselines were `39.681`, `36.885`,
`19.114`, and `20.562 ms`. Output-scale, full-distance geometry, topology repair,
filtered textures, and maximum vehicle LOD were unchanged.

`tools/audit_track_section_seams.py` independently audits captured track
triangles in authored model space or transformed GTE view space. It writes a
machine-readable summary, a boundary-edge CSV, and an optional OBJ; the OBJ is
only a visual cross-check. Exact integer vertex and edge comparisons determine
the result.

A later Red Rock replay crack at input poll 35,528 was not a mesh gap either.
Sections 78 and 79 share 11 exact model-space boundary edges. The endpoints
under the visible line are exactly `(2651,1975,2951)`–`(2818,2211,3034)` in
both models. GT2 submits the second copy through an almost exactly doubled
fixed-point transform: the latter endpoint becomes view-space
`(891,-354,15091)` in section 78 and `(1784,-706,30184)` in section 79.
PS1 integer projection therefore rounds the same authored endpoint to screen Y
345 and 346.

Enhanced rendering now evaluates projection continuously from the captured GTE
view coordinates. The topology pass then gives adjacent boundary copies one
exact projected position only when at least two authored vertices demonstrate
the join, including GT2's exact 4096-unit local-coordinate-cell translations.
It does not add geometry, move unproven vertices, use a proximity threshold, or
expand triangles. The poll-35,528 capture reports 48 demonstrated projection
groups and 36 adjusted copies. The retained development oracle bypasses both
continuous projection and topology to provide the original integer comparison
result; it is not a shipping preset.

`artifacts/seam-geometry-proven-live-35528/native-35510-35550.mp4` is the
composed live regression: H.264 High/yuv420p, 640x480, 30 fps, 20 frames.
Every frame spanning the reported junction is clean, with the complete HUD and
without a diagonal background sliver. The run used dummy audio and exited with
code zero. `contact-sheet.png` contains the complete 20-frame review.

The intermittent Red Rock replay fault at about 0:33 was captured at input poll
32,492 and isolated to two triangles whose shared GTE-view edge is exact, but
whose model-space X coordinates differ by 4096 and whose texture pages differ.
It was not a geometric hole. The D3D11 PS1 sampler incorrectly treated integer
UVs as texel edges with `floor(uv)`; PS1 integer UVs identify texel centers.
Sampling the nearest texel with `floor(uv + 0.5)` removes the dotted black line
without padding or UV nudges. In the fixed 4x replay crop, dark pixels on the
known line fell from 22 to zero.

## Superseded Seattle PC package evidence

The release package at
`artifacts/release-modern-renderer-ring4-20260826-2040` is the current clean
PC packaging baseline, not a renderer qualification build. Its ZIP SHA-256 is
`B920A27CB1E39E02765334E05302F3C94D85AAE9D640C0EA3E6DB83CC15C9AC9`.
The public archive contains nine files and no loose DLL; the managed runtime,
native D3D11 renderer, window/input libraries, and required runtime components
are embedded in the single `GranTurismo2PC.exe`.

`artifacts/release-modern-renderer-ring4-validation-20260826-2048` records a
clean extraction and setup from the authoritative SCUS-94488 revision-2 and
SCUS-94455 images. The installed release entered the direct CPU-driven Seattle
race at absolute input poll 341 and GT2's natural replay at poll 16,346. The
run continued through poll 17,000, then shut down cleanly. It used the rejected
243-object full-course union. That union co-rendered mutually exclusive road
and auxiliary meshes, so this package is retained only as packaging/timing
evidence and must not be shipped or used for visual acceptance.

The same run submitted 16,644 authored images and completed 16,641 before the
bounded diagnostic shutdown. It reported 16,641 actual images, zero synthetic
images, zero repeated images, and zero dropped pending, pool, or published
outputs. The three unfinished tail submissions are explicitly reported as
`authoredNoOutput=3`; they are shutdown work, not relabelled or substituted
frames.

The lossless completion ownership is deliberate. Native capture permits one
active readback plus two pending readbacks. Managed output therefore owns four
buffers: three completion/publication slots and one host-owned buffer. The
published queue has three entries, while steady presentation still prebuffers
only two completed images. This matches the bounded producer window without
restoring the former high-latency prebuffer. The preceding three-buffer/two-
publication package exposed two real publication drops after a roughly 150 ms
host stall; it is retained only as the rejected A/B and is not a release
candidate.

The one-frame visual proof at
`work/seattle-ring4-screenshot-20260826-2040.png` is a 2560x1080 presentation
from GT2's CPU-driven Seattle race at stage poll 600. The modern world is true
Hor+ at 2276x960 before final presentation, and HUD groups retain their
relative left/right edge margins. Its independent 589-frame capture run also
used the rejected full-course union. The image remains evidence for Hor+ and
HUD anchoring only; it is not evidence for course visibility or final visual
qualification.

## Historical pair-aligned readback and pacing

This section describes the retired midpoint pipeline. Current True60 uses the
single-authored-image API v6 queue and evidence listed above; it does not
submit or drain midpoint/actual pairs.

The final intermittent producer miss was not topology or interpolation. Native
phase timing isolated it to a blocking D3D11 staging `Map`: the two-slot ring
allowed one 30 Hz authored interval for completion, but loaded-machine GPU tails
reached 39-49 ms and left 20-27 ms of synchronous readback on the producer.
Command preparation and submission remained roughly 0.2-2 ms.

That retired implementation retained two midpoint/actual pairs in four staging
textures. Current shipping code instead queues individual authored images with
matching metadata in a sixteen-image asynchronous ring. The first authored
submission alone completes its GPU copy synchronously so the managed ownership
barrier cannot deadlock into its 100 ms timeout; later submissions remain
asynchronous and are never relabelled as repeats or synthetic frames.

A second late-Arcade investigation found broad 45-62 ms guest descheduling,
with no runaway function, GC pause, or allocation storm and only about four
percent total machine CPU load. Windows runs the whole process at Above Normal
priority so the emulation and renderer worker rise together; raising only one
thread is deliberately avoided because it starves the other side.

Current retained evidence from the exact packaged build is:

- v67 Arcade, v68 Supra/Tahiti, and v69 SSR11: 53/53 strict windows;
- v70 15-minute round-robin soak: six fresh processes and 106/106 strict
  windows across all three scenarios;
- v72 Simulation race, Results, and natural replay: 50/50 strict windows,
  zero world misses/waits, Maximum LOD, and pipeline/GPU/topology p99 of
  30.201/20.416/8.937 ms; and
- v73 visible/audible SSR11 auto-drive: 16 strict full-window presentations,
  zero repeats, substitutions, or misses, with both cards restored exactly.

Full-resolution presentation capture is opt-in and is never used as pacing
evidence. It synchronizes D3D11 staging readback and can add 18-26 ms to a captured
host frame. Captured runs remain visual evidence; no-capture runs are the
authoritative 59.94 Hz measurement.

## LOD and visibility

Draw distance and LOD are scene-selection policies, not shader tricks.

- The retained development oracle can submit GT2's original current-sector
  visibility result and selected vehicle LOD for explicitly approved
  comparisons only.
- The shipping modern renderer starts with the current sector's authored order,
  adds a bounded three-sector horizon in both directions, deduplicates by
  14-bit object index, and clears competing LOD selector bits. GT2's auxiliary
  visibility mask is preserved because it selects mutually exclusive camera-
  region alternatives; model zero supplies maximum detail for admitted objects.
- Stock race and replay radial distance gates are permanently removed, and the
  highest course and vehicle geometry is fixed. Course texture records remain
  selected by GT2's authored per-primitive material rule; public settings and
  process environment cannot restore stock distance or geometry LOD behavior.
- Invalid stock pointers or failure to locate the authoritative sector are
  fatal in a release build. They cannot silently fall back to current-sector
  packets and reintroduce pop-in.

Vehicles have a separate GT2 bounding-box gate before their body and four
wheel renderers. Reverse engineering the authoritative Arcade function at
`0x8007B550` and Simulation function at `0x8007B640` established that bits 1
and 2 are the left/right half-spaces in both the low common-plane mask (whole
object rejection) and high union mask (guest clipper selection). The modern
path clears only those four horizontal bits. Near, top/bottom, and GTE-fault
bits remain intact, and D3D applies the target-aspect frustum to continuous
view-space vertices. In a matched 1,000-poll Seattle replay, this restored 265
of 3,775 vehicle submissions that the 4:3 gate had removed; the remaining 735
near/vertical rejections were unchanged. All restored submissions used GT2's
highest wheel-detail path, and both sides of the A/B exited cleanly.

Vehicle ownership is taken from GT2's race-car array, not from the common
model renderer's first argument. The authoritative race loop at `0x8001545C`
advances one 0xB40-byte record per car and passes that record to
`0x800140A4`. The latter constructs a temporary request and invokes the common
renderer with a camera/projection scratch address; every car reuses that
scratch address. The generated hooks therefore bracket only the common call
with the owning race-car record, and capture interns that record as the stable
vehicle identity. A bounded Seattle replay recorded 654 begins, 654 ends, six
distinct owners, 654 scoped captures, and zero fallback captures. Its exact
wheel CSV contains six car identities and 24 car/wheel keys instead of the
former single false owner. Native grouping independently reports object IDs
0 through 5 across 36 body/part/wheel transform groups. This identity is used
by live draw ordering, wheel grouping, and perspective-UV edge continuity; it
is not diagnostic-only metadata.

Course material selection reproduces the guest GTE FIFO rather than treating
the secondary texture record as an ordinary modern mip. A triangle evaluates
`NCLIP(0,2,1)`. A quad evaluates `NCLIP(0,1,2)` and then the FIFO state after
corner 3 is pushed, `NCLIP(2,0,3)`; the threshold input is the unsigned absolute
value of `second - first`. The selected record supplies the texture page,
palette, and all authored UV corners. The geometry and packet UV order remain
the source order `0,1,2`, independent of the GTE's material-selection order.
This distinction is global: it fixes directional billboards, alpha-edged
foliage and cliffs, road shading, and kerbs without recognizing a track,
object, texture, or color.

The retained Trial Mountain guest-packet correlation closed 389,231 matched
triangles with zero UV, material, or color mismatches. An AI-driven moving
camera run through poll 9,000 then compared 13,546,490 native resident
triangles against the managed expansion with zero geometry, order, material,
or UV mismatches while rendering 8,522,966 authored secondary-material
selections. Bounded Seattle and Special Stage Route 5 runs independently close
715,164/715,164 and 663,957/663,957 resident triangles with zero mismatches.

A clean direct replay bounded vehicle diagnostics to polls 1400 through 1440.
At poll 1420 the frame contained 23 exact transform groups and 2,417 vehicle
commands. The center car's body, shadow, and four wheels were separately
identified; all vertices were finite, in front of the near plane, and covered
by visible scissors. A second car crossed the left edge continuously with all
305 body commands still submitted. The coarse center-car appearance in that
clean build was therefore not missing geometry or a failed transform: the
runtime independently reported that no external 4x DDS pack was installed and
was filtering GT2's original low-resolution car bitmap. Vehicle diagnostics
label authored shadows and report textured/semitransparent command counts so a
texture-quality issue cannot be misclassified as detached geometry again.

The bounded horizon is a residency source, not a union of already projected
triangles. Each selected object is decoded once from its authored model and
receives continuous modern backface, homogeneous near-plane, target-aspect
frustum, and depth processing after reconstruction. This avoids sector pop-in
without co-rendering the looping course's mutually exclusive road/proxy meshes.

Resident billboard depth correction (September 3 evening): model-space tree
corners use the same per-vertex normalized camera depth as other resident course
geometry. `Dma.TryGetOrderingTableIndex` returns `(head - entry) / 4`, a reverse
traversal ordinal, not a metric distance bucket. Multiplying that ordinal by
8192 inverted near/far ownership. In the captured Midfield building overlap,
ordinal 168 produced Z 1,376,256 while the actual tree was near Z 33,515,000,
behind a wall near Z 4,049,442. The renderer ignores the old resident billboard
depth-override bit, including in existing captures. No tree geometry, texture,
position, alpha cutoff, or per-track exception is changed. Screen-offset effects
are a separate legacy path and are not claimed fixed by this correction.

Screen-offset flare follow-up (September 4): the same invalid DMA-ordinal
conversion remained in the legacy halo path. The matched SSR5 starting-straight
capture has a near streetlight halo at normalized anchor Z 2,019,328 and a
building behind it at Z 12,567,668. Ordinal 4020 was instead converted to
32,931,840, incorrectly hiding the halo behind that building while its pole
remained in front. All world primitives now retain their normalized camera Z;
screen-offset halos retain their anchor Z and resident foliage retains its
per-corner Z. Ordering metadata remains available for submission ordering but
never substitutes for distance. The former Clubman unit test encoded the same
incorrect ordinal-as-distance assumption and is replaced by physical-depth
expectations plus GPU tests with a halo in front of / behind an opaque wall in
both submission orders. No blend, alpha-coverage, geometry, texture, or
course-specific rule changes. Evidence: `artifacts/ssr5-depth-fix`.

Course billboard stream 8 has two distinct authored coordinate layouts and is
decoded as such. Arcade `func_8002009C` stores primary billboard center X/Y,
base Z, and Z height; its temporary GTE matrix rotates width through X/Y.
Auxiliary `func_8001F784` stores Y height and rotates width through X/Z with
the second temporary matrix axis negated. Applying the auxiliary layout to the
primary records turned a 622-unit Z height into a roughly 2,500-unit view-X
span and produced a carriageway-wide horizon slab. The split decoder now emits
the exact packet vertex order for each layout. A 1,099-frame direct Seattle
correlation matched every guest-generated stream-8 triangle with permutation
zero across both projection paths, with zero decode failures and zero runtime
fallback. The poll-1,420 regression changed only 554 billboard commands; the
former slab pixel is once again the authored road surface. A fixed numeric
policy test independently locks both axis/sign contracts.

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

## Filtered course cutout depth

Course cutouts use two complementary coverage passes: alpha at least 0.5
owns depth, while lower nonzero filtered coverage blends with depth testing
but without depth writes. Both retain coverage blending. This prevents faint
minified foliage from punching opaque depth holes into later terrain without
making solid leaves transparent to farther objects. The passes remain in
authored batch order; this is not order-independent transparency.

The GPU regression renders sparse and solid foliage before farther opaque
terrain. Sparse coverage must reveal the terrain; solid coverage must occlude
it. Re-enabling fringe depth writes fails the sparse-coverage assertion.
Midfield native race captures provide the corresponding visual audit.

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
4. **Complete on PC:** integrate the D3D11 backend into every
   provenance-backed live world, preserve authored screen-primitive semantics
   and persistence for every HUD layer, eliminate motion-visible track seams
   and section pop-in, remove PS1 Quality/custom switches, and retain the GPU
   command layer for world-free menus, loading screens, Results, and video.
5. **Complete on Windows:** the authored 2D command layer, native world backend,
   presentation scaling/FXAA, capture, and ImGui wrapper share one D3D11 device
   and DXGI swap chain; the OpenGL shipping path and dependencies are removed.
6. Add the RecompOne C++ guest emitter/runtime needed by NXDK.
7. Render the same captured scene through the NV2A backend within a measured
   memory budget.
8. Move input, audio, loose-file I/O, memory cards, and the complete game loop
   to the native runtime.

Each milestone needs structured logs and deterministic captures. A visually
plausible screenshot alone is not an acceptance test.
