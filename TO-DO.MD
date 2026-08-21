# OpenGTPS1 TO-DO

Nothing in this list is considered complete merely because a setting, code path,
or wrapper control exists. An item can be checked only after its effect is
visible and repeatable in the packaged build at:

`C:\Programming\GitHub\OpenGTPS1\OpenGTPS1\GranTurismo2PC.exe`

## Repository and test-artifact hygiene

- [ ] Treat cleanup as a required final step of every test, capture, profiling,
  conversion, and smoke-test batch. Keep only the newest passing evidence, the
  newest actionable failure reproduction, user-requested deliverables, and
  compact validation logs. Delete superseded raw frame dumps, videos, traces,
  extracted smoke installs, RAM dumps, and rebuildable `bin`/`obj` caches before
  ending the task. Report repository size and free disk space after cleanup;
  do not allow generated `artifacts` and duplicate `work` trees to accumulate
  without an explicit retention reason.

## Unified frontend and mode integration

- [x] Replace the temporary two-line title treatment with Sony's complete
  retail `arcade/arc_topmenu_usa` 2x2 menu. Preserve its GT2 artwork and native
  selected/unselected labels for Arcade Mode, Gran Turismo, Replay Theater,
  and Option; do not synthesize replacement text.
  - All four selection states are rendered at the archive's native 512x480
    BGR555 resolution with the original unscaled 140x28 label rectangles.
    Automated comparisons report zero differing pixels for all four states;
    all four original destinations also pass. Proof is under
    `artifacts/retail-arc-topmenu-usa/exact-live`.
- [x] Make the main-menu `Arcade Mode` entry transition seamlessly into the
  first Arcade Mode menu. It must not visibly reboot, repeat the Arcade disc's
  boot/copyright/title sequence, or present a disc-switch boundary. The native
  Arcade executable and data may remain the implementation behind the handoff,
  but the player-facing result must behave like one shipped game.
  - The transition recreates the Arcade program's exact prologue state and
    enters native function `0x8005D650`, matching the stock disc's `START GAME`
    destination. Overlay 1 is required and boot/title overlay 5 is forbidden by
    automated assertions.
  - The current 9,000-poll silent/headless proof at
    `artifacts/retail-arc-topmenu-usa-ssr11-final` starts at the unified title,
    reaches the original Arcade course menus, selects Special Stage Route 11,
    and runs a live auto-driven race without an unmapped call or exception.

## Gran Turismo 1 content integration

- [ ] Complete the former GT1-integration goal through the native
  `GTPATCH.VOL`/merged-`GT2.VOL` first-run conversion pipeline. Finish every
  archive-proven GT1-exclusive track, car, paint, livery, wheel, and part
  conversion; fold shared identities into their GT2 counterparts; integrate
  appropriate cars fully into Gran Turismo Mode purchasing, upgrades,
  classes, eligibility, saving, and reloading; and retain native Arcade Mode
  availability. Treat direct `.VOL` archive and car-database comparison as
  authority rather than web lists. Smoke-test every conversion with auto-drive,
  retain screenshots and structured evidence, confirm race/replay behavior,
  and document the complete first-run workflow. Continue from the detailed
  inventory and completed-conversion record in `docs/PORT_PLAN.md` and
  `docs/GT1_CONTENT_AUDIT.md`.
  - [x] Complete a model-referenced-texel census of all 144 comparable GT1 RM
    bodies. Stage 31 distinct authored bodies (62 body/paint choices, including
    53 paints with no visible GT2 counterpart) on existing GT2 identities.
  - [x] Confirm all ten FTO RM paints use the same IDs and differ on zero
    model-referenced texels; do not import them as duplicate choices.
  - [x] Render every staged choice at 960x640 and assemble the validated v2
    gallery (116 folded proofs total, zero within-car duplicate visual hashes).
  - [ ] Complete silent selector/race smoke coverage for both paints on each
    of the 31 staged RM bodies before enabling the gated livery layer.

- [x] Eliminate the reported square exhaust/backfire flashes without hiding
  genuine effects. Exact consecutive SSR11 captures proved that the rectangles
  existed only on generated midpoint images: both adjacent authored images
  were clean, and the offending geometry belonged to fast-rolling vehicle
  wheel/cutout groups rather than a native backfire sprite. Midpoint generation
  now preserves the authored roll sample for a wheel that advances more than
  45 degrees per 30 Hz state while interpolating its rigid translation, and it
  moves every prior triangle in a valid rigid group coherently through authored
  clipping churn. No effect primitive is filtered or suppressed.
  - `artifacts\backfire-ssr11-pair-captures-v19` retains the exact failing
    normal-race capture pairs. The current offline rerender at
    `artifacts\backfire-ssr11-pair-rerender-v20` contains no rectangular
    midpoint exposure.
  - `artifacts\backfire-ssr11-autodrive-postfix-v21` is a fresh silent/headless
    native-AI SSR11 race, never replay view. Its 640x480 60 FPS video contains
    1,800/1,800 distinct frames and zero adjacent duplicates; the retained
    16-consecutive-frame sheet around the former failure is clean.
  - The independent no-video paced run at
    `artifacts\modern-renderer-ssr11-autodrive-postfix-v22` has 17 perfect
    59.89-60.11 Hz world windows, zero repeats/world misses, Maximum track and
    vehicle LOD, no bounded-resource wait/timeout, clean shutdown, and no
    crash, renderer-disable, or unmapped-call marker.

## Native renderer and original Xbox

- [x] Retire and remove the PS1-era compatibility **3D world** renderer from
  the shipping presentation path. OpenGTPS1 ships one modern 3D renderer rather
  than a PS1 Quality preset, legacy-renderer switch, or silent fallback.
  - The shipping configuration now exposes no PS1 Quality, Custom, stock-LOD,
    reduced-distance, affine-projection, seam-disable, dithering, smoothing-off,
    or native-renderer-disable switch. Old serialized values and direct runtime
    downgrade attempts are canonicalized to the fixed modern contract. Any
    frame containing authored 3D provenance asserts modern-world ownership
    before capture validation, so a native failure is reported as a world miss
    and cannot silently expose compatibility 3D. Authored world-free menus,
    Results, loading screens, and video continue through the GPU 2D command
    layer pending the final portable-interface consolidation.
  - Provenance classification now occurs even if the native worker is stopping
    or has failed. Provenance-backed triangles are captured for the native
    renderer but are rejected by both GL-HLE and software compatibility
    rasterizers; screen-space commands remain enabled. Reflection regressions
    prove both halves of that contract.
  - [ ] Consolidate the retained authored 2D command compositor (menus, HUD,
    loading screens, Results, MDEC video, and world-free transitions) behind
    the portable presentation interface. This is not a second 3D renderer and
    does not block retirement of compatibility world rasterization.
- [x] Establish a portable C++17 scene contract and explicit topology-based
  boundary stitcher with desktop unit tests.
- [x] Capture one bounded live projected race draw stream plus complete HLE
  VRAM, load it through the native C++ core, and independently render
  deterministic affine/perspective PNGs as a migration fixture.
  - `tools\capture_projected_scene.ps1` captures at a requested input poll,
    hard-caps triangle data at 25 MiB, adds exactly 1 MiB of BGR555 VRAM, and
    atomically publishes only a complete file.
  - The native reference path reconstructs PS1 textures/CLUTs, draw order,
    masks, transparency, dithering, and both interpolation modes without
    graphics-API dependencies or hidden core allocations.
  - Current live Red Rock evidence contains 5,493 triangles in a 1,575,984-byte
    capture. The independent perspective render used coherent depth on 3,213
    triangles and differs from affine across 20,207 of 76,800 pixels
    (25.31 dB PSNR). Perspective, affine, and the exact captured VRAM reference
    are ordinary 320x240 PNGs; automation proved the SDL dummy driver twice,
    restored the caller's audio environment, and left wrapper settings
    unchanged.
  - This projected fixture does not satisfy the world-space scene milestone
    below; it is the deterministic compatibility oracle used to reach it.
- [x] Capture a deterministic race scene containing camera, world geometry,
  materials, visibility identity, and original draw order.
  - `tools\capture_world_scene.ps1` captures upstream GTE model/view data,
    complete camera transforms, GT2 track/vehicle submission identity,
    material state, and original triangle order into the bounded `OGTWCAP`
    format. UI and other primitives without world provenance are excluded
    rather than inferred from screen pixels.
  - Format version 4 also records a capture-stable authored identity for every
    track vertex, in addition to the exact per-vertex GTE projection state and
    per-draw GPU offset. GT2's main and mirror cameras can therefore be
    separated without inferring a camera from screen pixels, and topology does
    not depend on float proximity.
  - The portable loader validates the format using caller-owned buffers,
    derives world coordinates from the captured camera, and exports an OBJ
    grouped by stable object identity. Its synthetic unit fixture verifies
    identity and exact world-coordinate recovery.
  - Current live Red Rock evidence contains 4,999 world triangles / 14,997
    valid vertices: 2,639 track, 1,817 vehicle, and 543 transform-complete
    unclassified effect/environment triangles. It records 48 identified track
    objects, two identified vehicles, 52 object/model combinations, 151
    materials, and 62 transforms; camera round-trip RMS is `0.000000`. All
    14,997 vertices also reproduce their captured screen position exactly
    (`0.000000` RMS/max) from their own recorded projection state. The current
    version-4 fixture assigns authored identity to all 7,917 captured track
    vertex instances and was produced with SDL dummy audio proven twice and
    wrapper settings unchanged.
- [x] Render the captured scene through a standalone PC backend and compare it
  deterministically with the compatibility renderer.
  - The API-neutral C++17 draw-list contains homogeneous clip coordinates,
    world/view positions, UVs, colors, face normals, PS1 materials, scissor,
    object identity, ordering-table data, and original submission order. No
    D3D type crosses that boundary, leaving an explicit NV2A backend seam.
  - The D3D11 backend decodes raw 4/8/15-bit VRAM and CLUT state in its shader,
    uses perspective-correct UVs, preserves Gouraud color interpolation and
    all four blend modes, handles STP transparency and mask bits, and applies
    depth within coherent object/model/ordering layers without discarding
    GT2's inter-model ordering contract. Dithering is off by default and
    available only through the explicit viewer switch.
  - The AI-driven fixture builds 3,657 main-view commands and excludes 1,342
    separately projected mirror commands. It includes 677 main-view vehicle
    triangles from two identified cars.
  - `tools\validate_world_renderer.ps1` runs the WARP backend twice, requires
    identical hashes, bounds PNG output, and removes repeat images. The
    retained run produced GPU hash `299b4de4d2d03d2e` twice and CPU oracle hash
    `17b87953231f3c05` twice; its compatibility path measures 4.173142 RGB RMS
    (35.72 dB PSNR). The default hardware D3D11 adapter also renders the same
    scene successfully.
- [x] Replace screen-space triangle padding in the standalone renderer with
  authored boundary topology, exact T-junction subdivision, and deterministic
  coplanar ownership.
  - The portable C++17 pass uses capture-stable authored identities and exact
    integer GTE view coordinates. It performs no proximity search,
    screen-space expansion, epsilon join, or UV nudge.
  - Live Red Rock race evidence transforms 3,657 main-view commands into 3,659:
    two source triangles are split into four at exact T-junctions. Diagnostics
    also report 227 authored boundary groups, 64 positive-area coplanar pairs,
    60 exact duplicates, and three deterministic ownership reorders.
    Repeated WARP renders match at GPU hash `63c96f524265c09b`; the independent
    compatibility oracle remains `17b87953231f3c05`.
  - The intermittent replay fault around 0:33 was recaptured at input poll
    32,492. The two road triangles share an exact view-space edge while their
    sector-local model X coordinates differ by 4096. The visible dotted line
    was a texture sampling fault, not a geometric hole: integer PS1 UVs were
    floored as texel edges instead of rounded to texel centers. The corrected
    point sampler reduces dark pixels on the known 4x line crop from 22 to
    zero, without padding.
  - Synthetic tests cover exact authored boundary copies, T-junction
    subdivision, and deterministic same-material coplanar ownership.
- [x] Integrate native rendering for every provenance-backed live world while
  retaining the authored GPU 2D command layer for menus, loading screens,
  world-free display transitions, Results, and MDEC video, plus exact
  screen-primitive semantics for the HUD.
  - The packaged runtime records complete provenance, explicit screen-space
    primitives, and VRAM into three bounded reusable OGTWCAP v4 buffers. A
    persistent D3D11 worker consumes a bounded two-capture FIFO and publishes
    reusable RGBA buffers without filesystem traffic. Only the first-ever
    world submission uses a bounded seed barrier; every subsequent frame and
    temporal reset remains asynchronous.
  - `artifacts\live-native-full-tail-fixed` reaches input poll 45,000 through
    the purchased/upgraded Mazda race and natural replay, renders 19,502
    native frames, engages the deterministic driver for race pass 1/2 and
    replay pass 2/2, proves both SDL dummy-audio checks, and exits cleanly.
    It records no native disable, fatal error, or unhandled exception.
  - The live recorder now retains ordinary no-GTE polygons as screen
    primitives, preserving the tachometer needle and redline wedge. GT2 emits
    the thin tachometer/turbo line layer on alternating screen-only frames, so
    a bounded 256-triangle cache carries only that line layer into the next
    native world frame. The native draw list also retains the authored
    secondary projection used by the rear-view mirror.
  - The former output pool covered only a 320x240 display at 4x and failed with
    `invalid_argument` when GT2 selected a wider late transition mode. The
    fixed pool remains bounded but covers every 640x512 authored display at 4x.
    Submission is provenance-based, so wider showroom, transition, and
    rotating-car views remain native when they contain 3D. A world-free Results
    frame is deliberately owned by the complete authored 2D compositor.
  - The earlier native/legacy comparison was invalid because its so-called
    legacy control disabled only the compositor while retaining Enhanced's
    all-sector visibility replacement. A true PS1 Quality control at input poll
    35,240 does not contain the reported gray diagonal surface. The extreme
    close replay camera at 35,570 is authored, but the diagonal surface was a
    native-only visibility defect.
  - Historical pre-provenance-ownership packaged validation
    `artifacts\native-track-surface-corrected-final-v3` reaches input poll
    45,100,
    records the natural replay through its end, deliberately hands the 352x300
    Results draw area to the authored GPU 2D compositor, and exits cleanly. It submits
    19,505 native frames, renders 19,496, consumes 19,494, and drops 11 obsolete
    queue entries without a native disable, truncation, fatal error, or
    unhandled exception.
- [ ] Add a RecompOne C++ emitter and native guest runtime with no CLR
  dependency.
- [ ] Add the NXDK/NV2A backend and validate the same captured scene on an
  original Xbox.
- [ ] Fit native runtime, track data, textures, audio, and transient geometry
  into a measured 64 MiB unified-memory budget.
- [ ] Port loose-file I/O, input, audio, memory cards, and the complete game
  loop to the native runtime.
- [ ] Add backend-neutral lighting/material extension points for future car
  reflections, normal/bump maps, and specular response.

## Graphics enhancements

- [x] Fix affine texture projection.
  - Texture mapping must remain stable on road surfaces, walls, cars, and other
    polygons as the camera moves.
  - The correction must affect the actual game rasterizer, not only expose a
    wrapper checkbox.
  - Compare the same replay camera and track section with the option off and on;
    the difference must be obvious and the corrected mode must not introduce
    clipping, warped geometry, missing polygons, or broken videos.
  - Current packaged-build evidence: GT2's track clipper reloads projected SXY
    values through writable GTE registers before emitting road packets. The
    runtime now carries the original GTE depth through that reload. At input
    poll 10,000, all 278 textured foreground-road triangles (`y >= 390`) have
    coherent direct depth, versus zero before the fix. The synchronized,
    dummy-audio-safe 15-second H.264 pair in
    `artifacts\projection-motion-sxy-depth-fix` shows the correction affecting
    the moving foreground road without missing polygons or clipping in that
    segment.
  - Additional live-track traces at poll 8,000 show trusted depth on 252/252
    sampled Tahiti foreground polygons and 177/177 plus 168/168 sampled High
    Speed Ring foreground polygons across its alternating display pages.
    Corrected-vs-affine matched frames measure 27.35 dB PSNR on Tahiti and
    31.26 dB on High Speed Ring, so this is a substantial road-surface
    rasterization change rather than a wrapper-only toggle.
  - Final replay-camera evidence in
    `artifacts\projection-replay-motion-final` contains synchronized 15-second
    corrected/affine H.264 clips from the exact final package. Across multiple
    road, wall, car, and camera views the pair measures 37.35 dB PSNR /
    0.9685 SSIM; the corrected path has no missing polygons, clipping, broken
    HUD, or camera-cut corruption. Both captures proved dummy audio and left
    wrapper settings unchanged.

- [x] Remove or conceal seams between streamed road/model sections in the
  packaged live renderer.
  - Road sections must meet without visible cracks, background-colored slivers,
    or intermittent gaps as track geometry loads.
  - The mitigation must work while driving and during replay cameras.
  - It must not cause z-fighting, overlapping road surfaces, floating geometry,
    or collision changes.
  - Packaged still evidence: disabling only seam stabilization on the matched
    Tahiti frame removes the conservative edge pass from 257,345 triangles and
    produces a 34.36 dB-PSNR edge-localized delta.
  - Replay-motion pair `artifacts\seams-replay-motion-final` isolates the seam
    pass across multiple cameras at 39.98 dB PSNR / 0.9760 SSIM. The ordinary
    on footage has no z-fighting, overlaps, floating geometry, or divergent
    vehicle motion.
  - Race-motion pair `artifacts\seams-tahiti-motion-final` adds 450 synchronized
    frames through Tahiti Road's changing streamed sections. Its isolated
    on/off delta is 36.97 dB PSNR / 0.9617 SSIM; the enabled footage retains
    continuous road edges and identical vehicle/camera motion without gaps,
    floating sections, overlap flicker, or collision changes. Both runs proved
    SDL dummy audio and produced bounded H.264 files under 3 MiB each.
  - Regression found in the retained Red Rock replay at video frame 996
    (about 0:33.17, replay clock 0:24.000): a dark diagonal road crack remained
    visible for intermittent frames despite the original radial padding.
    `artifacts\seams-replay-033-baseline` reproduced the exact frame with seam
    stabilization both on and off, proving the eligible triangles reached the
    pass but the 0.25-native-pixel centroid expansion was insufficient.
  - The compatibility renderer's 0.75-pixel miter is retained only as old
    evidence; it is a screen-space workaround and not the accepted final fix.
  - The packaged native renderer resolves authored topology and the 0:33
    texture-sampling fault without padding. Screen-space sprites use edge-based
    sampling while 3D polygons use texel-center sampling, so the
    minimap/tachometer also avoid the former one-column edge artifacts.
  - A later reported diagonal crack at replay poll 35,528 was audited directly
    from the submitted models. Sections 78/79 share 11 exact integer boundary
    edges, including identical endpoints `(2651,1975,2951)` and
    `(2818,2211,3034)`, ruling out an authored mesh gap. GT2 submits the two
    copies through almost exactly 1x/2x fixed-point transforms, whose integer
    projection rounds the latter endpoint to Y 345/346.
  - Enhanced now projects from the captured GTE ratios continuously, then
    canonicalizes only adjacent boundary copies demonstrated by two or more
    exact authored vertices (including 4096-unit local-cell translations).
    The captured frame identified 48 such groups and adjusted 36 submitted
    copies. No geometry, padding, epsilon join, UV nudge, or collision change
    is involved. PS1 Quality retains the original integer projection.
  - `artifacts\seam-geometry-proven-live-35528` contains the exact live
    35,510–35,550 regression. Its 20 composed H.264 frames were all reviewed
    clean with the HUD intact; the deterministic run proved dummy audio,
    native rendering, encoder completion, and a zero exit.
  - A second defect in the former accepted video was a long gray road slab
    crossing the view at 0:55. The old all-sector union combined GT2
    potential-visibility lists that contain mutually exclusive or occluded
    road surfaces. Enhanced now preserves the authored current-sector list;
    exact old/new frames at 0:55.000 and 0:55.500 in
    `artifacts\track-surface-old-new-comparison` show the false slab removed.
  - The replacement 216.63-second replay was reviewed end-to-end at four frames
    per second. Every encoded frame from 0:52 through 1:02 received an
    additional frame-by-frame review. Track boundaries remain continuous
    without the false slab, section pop-in, background slivers, overlap
    flicker, or floating geometry.
  - August 9 follow-up evidence reproduces the reported cyan road holes and
    displaced/partially rejected wheels from exact live captures at polls
    5701-5731. Vehicle depth ownership is now separated at body/wheel transform
    boundaries. Continuous topology retains authored integer SXY only to prove
    power-of-two road-LOD copies and sub-quarter-pixel vertex-to-edge joins.
    The reported frame drops from nine exposed background samples to zero and
    the sequence's worst LOD boundary drops from 151 to zero without triangle
    inflation. The two remaining cyan-colored samples are covered by valid
    road triangles and are transparent source texels, not geometry cracks.
    `artifacts\user-gap-wheel-repro-14s\fixed-sequence` contains the 16 fixed
    frames, contact sheet, per-frame logs, and scan CSV; all five native tests
    pass.

- [x] Extend draw distance far enough to show the entire track.
  - Track geometry and scenery must remain visible to the farthest practical
    distance instead of appearing in streamed chunks near the camera.
  - Distant geometry must not disappear when the camera changes direction.
  - The implementation must cover every track, not only the current test track.
  - GT2's per-sector lists are authored potential-visibility sets rather than
    independent chunks of a global object list. Unioning all lists exposed
    mutually exclusive road surfaces, including the false diagonal slab at
    0:55. Enhanced now retains the current authored list first and adds only a
    bounded three-sector horizon in both directions. This moves nearby sector
    transitions outside the practical sightline without exposing the rest of
    a looping track.
  - Overlay 0 also applies a camera-relative `0x0063FFFF` radial cutoff after
    consuming that list. Enhanced disables this redundant distance cutoff
    while retaining authored potential visibility, frustum, near-plane, and
    polygon clipping. Replay's separate stock distance gate is extended by the
    same setting.
  - Matched isolation runs prove the distinction: keeping the radial and replay
    distance extensions while removing only the all-sector union removes the
    gray slab. The bounded horizon's SSR11 audit then proves all 96 observed
    stock additions were already resident before their original sector
    boundary and all 90 removals remained resident after it. Seventeen complete
    paced windows hold `59.73-60.07 Hz`, with zero repeats, world misses,
    invalid lists, output waits, or resource exhaustion.

- [x] Force the highest available LOD at all distances.
  - Cars, track objects, and scenery must use their highest-detail model.
  - Objects must not visibly swap to lower-detail models with distance.
  - Validate both normal racing and replay cameras with multiple opponents.
  - Current packaged evidence: the wrapper Maximum path forces GT2's one-based
    vehicle selector `1`, which resolves to model-table index `0`, on all three
    validated tracks. Tahiti's matched Stock/Maximum frame differs at
    32.30 dB PSNR while the Maximum log identifies selector/index 1/0.
  - The final deterministic race/replay audit is
    `artifacts\modern-renderer-native-results-soak-v16-final-profile`. It
    records 7,768 race plus 3,717 replay visibility calls. Stock GT2 requested
    lower detail in 96,296 of 177,963 raw entries; after Maximum conversion all
    177,963 emitted track/scenery entries use selector `0` (highest), with zero
    nonzero selectors, null lists, or invalid lists. All 62,661 vehicle requests
    use selector `1` / model-table index `0` across seven model sets. The focused
    `artifacts\modern-renderer-lod-audit-v3-fixed` control independently changes
    5,992 lower-detail requests among 8,988 raw entries to 8,988 highest-detail
    entries and sends all 2,580 vehicle requests to selector `1`.

- [x] Add a working dithering toggle.
  - Off must disable PS1 color dithering.
  - On must reproduce the intended PS1-style dither pattern.
  - The setting must affect gameplay, menus, and videos wherever the original
    rendering path used dithering.
  - Current packaged evidence: Tahiti Enhanced reports zero dithered triangles;
    enabling only dithering reports 163,389 in the matched window and produces
    a visible 37.22 dB-PSNR frame delta. PS1 Quality similarly reports 127,999
    dithered triangles while its matched Enhanced frame reports zero. Broader
    The setting is consumed by the shared primitive rasterizer, so menus and
    video-composited primitives use the same switch wherever the guest enables
    PS1 dithering; already-decoded video pixels are correctly left untouched.

## Wrapper quality controls

- [x] Make every graphics enhancement independently controllable outside the
  game in the wrapper menus:
  - [x] Texture projection correction
  - [x] Model/road seam mitigation
  - [x] Extended draw distance
  - [x] Forced highest LOD
  - [x] Dithering
  - Custom exposes five separate wrapper checkboxes. Matched exact-package
    captures changed one runtime setting at a time and produced independent
    renderer/geometry/LOD/dithering deltas while all other Enhanced settings
    remained constant.

- [x] Implement and verify the three overarching presets:
  - [x] **PS1 Quality** restores original projection, seams/streaming behavior,
    draw distance, LOD behavior, and dithering.
  - [x] **Enhanced** enables the corrected projection, seam mitigation,
    extended draw distance, highest LOD, and the selected enhanced dithering
    default.
  - [x] **Custom** exposes and preserves all individual settings.
  - Named presets are authoritative and repair contradictory persisted
    individual values on load. Current paired PS1 Quality/Enhanced runs prove
    zero vs. nonzero perspective/seam passes, stock vs. complete visibility,
    stock vs. forced model index 0, and dithering on vs. off.

- [x] Ensure preset and individual-setting changes reach the live renderer.
  - Applying a setting must visibly change the game without editing files by
    hand.
  - Saved settings must survive a clean restart.
  - The wrapper must clearly indicate when changing an individual setting moves
    the preset to Custom.
  - Each Custom checkbox updates the live `ViewConfig`, calls `SaveCustom`,
    marks `GraphicsPreset=Custom`, and persists `interface.ini`. Test overrides
    intentionally suppress persistence; all paired regressions hash-verified
    that this diagnostic path did not alter the user's saved wrapper settings.

## Performance and audio

- [ ] Bring the single modern renderer to sustained, correctly paced 60 FPS
  with corrected perspective, seam/topology repair, full authored track draw
  distance, and maximum vehicle LOD enabled in both Arcade and Gran Turismo
  modes. Profile and remove CPU/GPU/synchronization bottlenecks without
  reducing output quality or reinstating the PS1 renderer. Report host
  presentation rate, new native-world frames, repeated frames, renderer time,
  guest time, queue drops, and frame-time percentiles separately so duplicated
  30 Hz world images cannot be mislabeled as 60 FPS. Validate representative
  races and replays, imported content, HUD completeness, stability, and frame
  pacing on the current target PC.
  - Current honest status: GT2 still authors world states at 29.97 Hz, while
    the native pair bridge produces a geometry-aware midpoint between complete
    authored states. Transform groups are matched as units, vertices use model
    coordinates, HUD/material/VRAM/visibility changes remain atomic, and no
    whole-frame cross-fade is used. Telemetry distinguishes authored,
    interpolated, repeated, and fallback presentations.
  - Historical pre-interpolation baseline: the August 9 silent/headless
    7,000-poll Arcade auto-drive soak at
    `artifacts\modern-renderer-road-wheel-live-soak` exits cleanly with no
    native disable or runtime fault. Race telemetry holds 59.94 host FPS,
    29.97 unique authored FPS, completion age 2, and three total obsolete
    startup drops across 1,341 submissions. The final race window reports
    native pipeline p95 `26.113 ms`, p99 `27.899 ms`, topology `5.346 ms`, and
    GPU render `18.132 ms`; this is stable but still above the `16.683 ms`
    budget required for genuine unique 59.94 Hz motion.
  - Historical pre-interpolation baseline: the subsequent modern-only pass
    spatially indexes projected topology
    candidates, batches compatible transparent primitives, moves per-command
    material selection out of per-draw constants, overlaps staging readback in
    the correct command order, and gives track sections one coherent depth
    surface while retaining separate vehicle body/wheel layers. The bounded
    8,000-poll Arcade auto-drive proof at
    `artifacts\modern-renderer-unified-track-soak-v4` exits cleanly after 1,691
    submissions and 1,686 complete authored renders, with five startup drops,
    no synthetic frames, and no renderer disable, exception, unmapped call, or
    crash. The final window is p50 `24.743 ms`, p95 `26.154 ms`, p99 `26.651 ms`.
    This was improved and stable at the authored 29.97 Hz rate, but remained an
    explicit failure against the unique-59.94-Hz completion criterion before
    the geometry-aware pair bridge below.
  - The 16-frame road/wheel regression now has zero exposed clear-color pixels.
    Isolated zero-valued road texels are repaired only for upward-facing opaque
    track surfaces with at least three opaque cardinal neighbors; authored
    transparent cutouts remain alpha-tested.
  - The bounded Arcade proof at
    `artifacts\modern-renderer-unique60-arcade-soak-v2` drives through poll
    12,000. Every complete race window from poll 5,100 onward contains exactly
    150 authored plus 150 interpolated images with zero repeats or fallbacks;
    unique rate is `59.57-59.98 Hz`. Shutdown reports 3,838 images of each kind,
    three obsolete startup drops, and no disable, exception, unmapped call, or
    crash.
  - `artifacts\modern-renderer-live-visual-proof-moving-v1` captures eight
    consecutive live presentations. Trace labels alternate interpolated and
    authored frames; visual inspection shows single car silhouettes, contained
    wheels, and continuous road/world geometry rather than blended cars or cyan
    clear-color holes.
  - Gran Turismo Mode proof at
    `artifacts\modern-renderer-unique60-simulation-purchase-race-v1` creates a
    test economy, buys and upgrades a Mazda, enters Sunday Cup, and emits 3,080
    authored plus 3,080 interpolated frames. Nineteen full native windows have
    zero repeats/fallbacks. One first-run window fell to `56.47 Hz` as guest
    time rose to `16.12 ms`, despite no lost native output. The exact path and
    interval were repeated in
    `artifacts\modern-renderer-simulation-pacing-repeat-v2`: all 19 complete
    windows are `59.89-59.98 Hz`, including `59.94 Hz` at the formerly slow
    poll 12,300 interval, with 150/150 unique images and no repeat/fallback.
    This classifies the original reading as a non-reproducible host scheduling
    transient rather than a persistent renderer or course bottleneck.
  - Imported-content coverage at
    `artifacts\modern-renderer-unique60-ssr11-night-soak-v1` enters SSR11 at
    night and emits 3,020 images of each kind. All complete post-throttle race
    windows contain no repeated or fallback presentation.
  - A second imported-content run at
    `artifacts\modern-renderer-unique60-supra-tahiti-soak-v1` exercises the GT1
    Supra body/palette on Tahiti Road. Its 23 complete race windows each contain
    150 authored plus 150 interpolated images, zero repeats/fallbacks, and
    `59.89-60.21 Hz` unique presentation; shutdown is clean with 3,590 images
    of each kind and zero obsolete queue drops.
  - Natural replay proof at
    `artifacts\modern-renderer-unique60-replay-soak-v3` enters reconstructed
    replay at poll 15,840 and remains native until the normal transition near
    poll 23,280. Every complete replay window is 150/150 with no repeat or
    fallback at approximately `59.67-60.18 Hz`; shutdown reports 7,719 images
    of each kind, one startup drop, no fault, and an exact memory-card restore.
  - Final bounded visible auto-drive proof at
    `artifacts\modern-renderer-final-visible-proof-v2` uses Enhanced/Stabilized,
    Extended draw distance, and Maximum LOD. All 24 complete live windows are
    150/150 with zero repeats/fallbacks at `59.86-60.00 Hz` (`59.939 Hz`
    average); shutdown is clean with 3,837 images of each kind. Final closure
    still requires the user's visual review of wheels, seams, prop stability,
    and distance transitions.
  - `artifacts\modern-renderer-unique60-motion-video-v2` supplies a 30-second,
    60 FPS review video. All 1,800 decoded frames have distinct hashes with no
    adjacent duplicate; dense 12 FPS sheets across two four-second motion
    intervals show contained wheels, single silhouettes, continuous road, and
    stable distance transitions. Synchronous GL recording perturbs host pacing,
    so this artifact is visual evidence only; the unrecorded runs remain the
    authoritative real-time performance proof. The unchanged 30 FPS default is
    verified by `artifacts\video-capture-default30-regression-v1` (5 seconds,
    150 frames).
  - Final automated closure is
    `artifacts\modern-renderer-native-results-soak-v34-independent-confirmation-final`. The reusable
    full-path validator buys/upgrades the Mazda, completes race and natural
    replay, crosses the discontinuous Results cameras, and restores the card
    hash exactly. Its 49 complete steady windows are 150 authored plus 150
    geometry-aware midpoint presentations with zero repeats and zero world
    misses; two discontinuity warmups are separately labelled repeated, and
    845 presentations cover authored 2D Results composition. No crash,
    unmapped call, truncation, rejection, or renderer-disable marker appears.
    Shutdown records zero pending-capture drops, zero output-pool starvation,
    one bounded 0.179 ms wait with no timeout, and only obsolete published
    outputs discarded at scene ownership changes. The independent v33 run adds
    51 complete zero-miss world windows at `59.80-60.11 Hz`.
    Its renderer audit also proves Maximum track/scenery and vehicle LOD over
    both race and replay, including the raw lower-detail requests converted by
    the shipping path.
  - The v34 run profiles the last 240 genuine track-world pairs, explicitly
    excluding menus/showroom/Results so their zero topology work cannot dilute
    the result. Pipeline p50/p95/p99 is `23.766/28.650/31.057 ms`, GPU
    render/readback is `15.755/18.959/19.912 ms`, and topology is
    `4.591/6.703/7.853 ms`. Pipeline p99 is below the `33.37 ms` authored-state
    budget. The reusable harness rejects missing/zero topology samples,
    unordered percentiles, a p99 above the 40 ms hard tail ceiling, or any
    complete world window outside 59.5-60.5 Hz.
  - Independent normal-speed cold-start coverage is
    `artifacts\modern-renderer-realtime-startup-v5-final-profile`: 192 native-world
    presentations, 2,199 compositor presentations, zero world misses, clean
    shutdown, exact card restore, and a retained first-showroom capture. The
    first-output barrier runs once, completed within the 100 ms bound, and
    prevents ownership from preceding the first validated modern texture.
  - D3D11 state binding is cached only when scissor, blend, or depth/stencil
    state is unchanged. The optimization is byte-exact across five retained
    reset/actual/midpoint PNGs and changes no geometry, full-distance policy,
    maximum LOD, topology repair, filtering, or output resolution. Final user
    visual review remains the only open gate for this item.
  - A second quality-neutral optimization reuses prior VRAM/material uploads
    only when exact per-command material, object-kind, and model-coordinate
    comparison proves midpoint compatibility. All 31 outputs from the retained
    16-capture road/wheel sequence remain byte-identical; pair p99 improved
    from `27.560 ms` to `25.783 ms`.
  - Current visible auto-drive evidence is
    `artifacts\modern-renderer-final-visible-review-v5-upload-reuse`. Every one
    of its 11 complete race windows is 150 authored/150 midpoint, zero repeats,
    zero world misses, and `59.90-60.19 Hz`, including the former poll-6,300
    repeat interval. It exits cleanly at poll 8,000; final user visual review
    remains the only open gate for this item.
  - Latest-build 30-second Arcade and Simulation review videos are retained at
    `artifacts\modern-renderer-final-motion-arcade-v2-final-profile` and
    `artifacts\modern-renderer-final-motion-simulation-v2-final-profile`. Each is
    640x480 at 60 FPS with 1,800/1,800 distinct decoded frames and zero adjacent
    duplicates. Contact, dense-motion, and worst-adjacent-SSIM sheets show
    contained wheels, single silhouettes, continuous road/world geometry, and
    stable props; the lowest-SSIM pairs are legitimate fast motion, start-line
    shadow, and close overtaking rather than one-frame defects. Recording is
    visual/temporal evidence, not the authoritative pacing measurement. User
    visual acceptance of these runs remains the sole closure gate.
  - Exact-current imported-content and replay review extends that evidence with
    `artifacts\modern-renderer-final-motion-ssr11-v1-final-profile`,
    `artifacts\modern-renderer-final-motion-supra-tahiti-v1-final-profile`, and
    `artifacts\modern-renderer-final-motion-replay-v2-final-profile`. Each
    30-second 60 FPS capture has 1,800/1,800 distinct decoded frames and zero
    adjacent duplicates. Their adjacent-frame SSIM min/p1/median values are
    `0.824554/0.860576/0.949322`, `0.819872/0.837272/0.946042`, and
    `0.509155/0.661939/0.897556`. Contact, motion, and temporal-outlier review
    found no wheel, silhouette, road/world, or prop-distance defect. Replay's
    lower outliers are genuine cuts and close passes: a dedicated inspection
    of frames 720-783 covers a silver car's full near-camera crossing and
    proves smooth authored/midpoint progression, attached wheels, and no
    alternating geometry.
  - The current replay-to-Results ownership proof is
    `artifacts\modern-renderer-replay-exit-v6-output-ring-latched-final`.
    It captures exactly 1,800 presentations; all first 740 replay-owned frames
    are unique with zero adjacent duplicates. Frame-level review shows replay
    handing directly to GT2's authored Results title animation, then a clean
    rotating car and Results UI, without the former stale race/replay flash.
    The full video has 1,727 unique frames; its 51 adjacent duplicates occur
    only after ownership reaches intentionally static Results/loading content.
  - Final silent/headless endurance evidence is
    `artifacts\modern-renderer-extended-soak-final-v84`: eight consecutive
    strict runs over 20 minutes (three Arcade, three imported SSR11, and two
    imported GT1 Supra/Tahiti) contribute 143 perfect steady world windows,
    zero repeats, zero world misses, clean shutdowns, and exact card hashes.
    Worst aggregate p99 is `28.134 ms` pipeline, `11.238 ms` GPU, and
    `12.396 ms` topology. No run reports an output wait or timeout.
  - Final exact coverage includes 30 distributed lossless Supra/SSR11 frames
    with zero clear-sentinel pixels anywhere. The former v85 SSR11 motion proof
    is superseded because it exposed a street-light billboard crossing the
    PS1 signed screen-coordinate boundary. The native draw list now applies
    the PS1 `1023x511` polygon-span rejection, and v101 supplies 1,800/1,800
    unique replacement frames with zero adjacent duplicate or light streak.
    Its proof sheets retain 640x480 detail per tile rather than the former
    320x240 review scale.
  - `artifacts\modern-renderer-ssr11-horizon-long-v102` validates the new
    bounded three-sector visibility horizon over 5,304 calls and 87 sector
    transitions. It preloads 96/96 stock additions, retains 90/90 stock
    removals, records 17 perfect `59.73-60.07 Hz` world windows, and exits with
    an exact memory-card restore.
  - Final Gran Turismo Mode closure at
    `artifacts\modern-renderer-final-host-simulation-replay-v86` covers
    purchase, race, Results, and natural replay. It has 50/50 perfect paced
    world windows, zero misses/violations/output waits, exact card restore,
    Maximum LOD throughout race and replay, and p99
    `22.146/11.352/8.816 ms` pipeline/GPU/topology.
  - Final visible GLFW/swap-path evidence at
    `artifacts\modern-renderer-final-visible-review-v87` adds 25 complete SSR11
    auto-drive windows at `59.89-60.00 Hz`, each exactly 150 authored plus 150
    geometry-midpoint images with zero repeats/misses. It records no long swap,
    crash, unmapped call, or output timeout, and exits cleanly at poll 12,000.
    Automated closure is now complete; this checkbox remains open only for the
    user's visual acceptance of wheels, road/world seams, prop stability, and
    distance transitions in that exact visible run.
  - Reproducible aggregate closure is
    `tools\verify_modern_renderer_closure.ps1`. The current retained output at
    `artifacts\modern-renderer-closure-audit-v103\closure-report.json` rebuilds
    and retests the current tree, independently revalidates v84/v86/v87, the
    fixed v101 street-light motion proof, and the v102 visibility horizon. It
    reports `machine-pass-user-visual-pending`; it cannot turn a machine pass
    into visual acceptance on the user's behalf.
  - The current durability pass removes a synchronous D3D11 readback from each
    generated pair: the staging slot is now mapped one complete authored pair
    later, and output provenance is delayed by the same amount. The native
    worker runs at normal priority so a full output queue cannot pre-empt guest
    time. A no-capture headless run also bypasses the hidden OpenGL compositor
    while continuing to consume and validate every native output.
  - A 125-second sampled trace at
    `artifacts\modern-renderer-guest-cpu-long-v40` found that SDL's muted dummy
    mixer consumed about 63 seconds of managed CPU even though no audio consumer
    existed. Silent renderer soaks now leave that dummy mixer inactive; explicit
    headless audio capture still starts it and is proven by the 19.354-second
    PCM smoke at `artifacts\modern-renderer-headless-audio-capture-smoke-v44`.
  - Post-correction strict evidence is
    `artifacts\modern-renderer-r2r-silent-mixer-off-soak-v42`: Arcade contributes
    20 perfect five-second windows, imported SSR11 17, and GT1 Supra/Tahiti 16.
    All 53 windows are unique 59.5-60.5 Hz world output with zero repeats, zero
    misses, Maximum track/vehicle LOD, clean shutdowns, and exact card restore.
    `artifacts\modern-renderer-r2r-silent-arcade-repeat-v43` then starts four
    fresh processes and adds 80/80 perfect Arcade windows with the same gates.
    Native tests remain 6/6 and the managed modern-only configuration suite
    passes. Visible/audible user review remains an explicit closure gate.
  - The August 15 producer-tail audit reproduced the remaining intermittent
    miss at `artifacts\modern-renderer-readback-phase-ssr11-v59`. Native phase
    timing proves every long call was a blocking D3D11 staging `Map`
    (`20-27 ms` readback versus roughly `0.2-2 ms` submission), after only one
    30 Hz interval of GPU overlap. Raising only the guest thread, raising the
    process before fixing readback, and extending the consumer wait to 12 ms
    were all rejected by strict failures in v51, v53, and v57 respectively.
  - Native readback now retains two complete midpoint/actual pairs in four
    staging textures. Pixel slots and bridge metadata derive from the shared
    `world_gpu_readback_pair_delay` invariant, so the extra completion interval
    cannot combine a midpoint with the wrong authored endpoint. Warmup outputs
    remain explicitly marked as repeats and are excluded from unique-motion
    counts. The fixed v61 SSR11 run has 17/17 perfect windows and zero waits.
  - A delayed sampled trace at
    `artifacts\modern-renderer-arcade-late-guest-trace-v64` found no runaway
    function, allocation storm, or GC pause during the former late-Arcade
    slowdown; normal-priority desktop scheduling descheduled the guest for
    `45-62 ms` while total machine CPU load was about four percent. The runtime
    now raises the whole Windows process to Above Normal, keeping emulation and
    renderer worker balanced. The external-policy experiment v65 and packaged
    proof v67 both pass Arcade 20/20 with no late slowdown or world miss.
  - Exact packaged scenario proofs are v67 Arcade (20/20), v68 GT1
    Supra/Tahiti (16/16), and v69 imported SSR11 (17/17): 53/53 complete
    five-second windows, each exactly 150 authored plus 150 geometric midpoint
    frames at `59.5-60.5 Hz`, with no repeat, compositor substitution, or miss.
    `artifacts\modern-renderer-pair-aligned-extended-soak-v70` adds six fresh
    round-robin processes over 15 minutes and another 106/106 perfect windows,
    clean exits, and exact card restores.
  - Gran Turismo Mode race/Results/replay closure is
    `artifacts\modern-renderer-pair-aligned-simulation-replay-nocapture-v72`:
    50/50 paced world windows, zero misses or output waits, 895 authored Results
    compositor frames, Maximum LOD over 7,768 race and 3,717 replay calls, and
    pipeline/GPU/topology p99 `30.201/20.416/8.937 ms`. The capture-enabled v71
    run is retained as visual evidence only: synchronous full-resolution GL
    readback reduced nine otherwise complete windows below 59.5 Hz, so capture
    is now opt-in and cannot contaminate performance claims.
  - Current visible/audible user-review run is
    `artifacts\modern-renderer-final-visible-ssr11-review-v73`. It uses normal
    auto-drive (not replay), full-window output, the shipping no-capture path,
    and adds 16 complete SSR11 windows with zero repeats, substitutions, misses,
    or card changes. Machine validation is green; this checkbox remains open
    until the user confirms the observed wheels, seams, props, and distance
    transitions.
  - The user subsequently observed small polygon explosions in SSR11, so the
    earlier visible review is rejected. A 5,400-frame reproduction at
    `artifacts\modern-renderer-ssr11-polygon-explosion-capture-v122` and exact
    source packets at v123 isolate the failure to generated midpoint frames:
    rigid whole-group interpolation reprojected already-culled, near-camera
    track triangles from objects 74/75 into large wedges. Whole-group rigid
    motion is now vehicle-only; track commands use exact provenance endpoints,
    and unmatched visibility-churn triangles hold their prior midpoint pose.
  - Exact formerly failing pairs are clean in v124, and the replacement live
    90-second v126 capture contains 5,400/5,400 unique 640x480 frames with zero
    duplicate and maximum luminance delta 4.046 versus 16.6334 before the fix.
    The exact ReadyToRun package is v131 (native SHA-256
    `47349A418581570BC264862F11684CE91F3FE9A668B6A582A11DFA20C46209A4`).
    Its strict SSR11 v132 passes 17/17 windows; the final 15-minute v136 soak
    runs two fresh processes each for Arcade, SSR11, and GT1 Supra/Tahiti and
    passes 106/106 complete windows; and the full Gran Turismo Mode
    race/Results/replay v134 passes 50/50 paced windows with zero misses,
    violations, waits, or timeouts. Fresh visible SSR11 acceptance remains
    required before this renderer goal can close.
  - The corrected maximized, normal-auto-drive visible run is v135. It passes
    16/16 complete SSR11 windows at 59.91-59.97 unique Hz with zero repeats,
    substitutions, or misses; final pipeline/GPU/topology p99 is
    18.681/6.400/8.427 ms. It exits cleanly and restores the card exactly.
    Machine validation is green; the user's visual verdict is still required.
  - Final machine closure is
    `artifacts\modern-renderer-closure-audit-v137\closure-report.json`. The
    verifier rebuilds and retests the current tree, fresh-publishes and
    byte-compares the ReadyToRun managed assemblies and native renderer,
    positively reproduces the rejected v122 explosion signal, validates all
    64 authored packets and 26 corrected 1280x960 midpoint pairs, recomputes
    all 5,400 v126 frame hashes/SSIM/luminance samples, and re-audits v132,
    v134, v135, and every v136 run. It reports
    `machine-pass-user-visual-pending`; visual acceptance is not automated.

- [x] Complete a performance pass against the packaged build.
  - Gameplay speed must match the original NTSC timing.
  - Menus, videos, racing, and replay must maintain correct pacing.
  - Add a user-visible or logged frame-time/FPS measurement so performance can
    be judged from evidence rather than feel alone.
  - Verified in the exact packaged Enhanced build with the visible,
    session-only `--mute` run
    `artifacts\enhanced-full-race-visible-muted-ntsc-audio`: all 51
    300-frame race windows measured 59.94 FPS; 49 replay windows measured
    59.91–59.95 FPS (59.9394 average). The host logs frame pacing, guest,
    device, and wait time without changing the user's saved audio settings.
  - The packaged native-world replay encode rendered 19,470 frames before the
    normal post-replay display-mode transition, averaging 14.033 ms per native
    frame with 12 stale-buffer drops (0.06%). GT2 submits its 3D world at
    30 Hz, so the persistent D3D11 path remains inside its 33.37 ms budget
    without blocking the emulation thread.

- [x] Eliminate frequent audio stutter.
  - Music, sound effects, and video audio must play continuously during menus,
    loading, racing, and replay.
  - Validate on a normal audible launch, not only headless mode.
  - Log underruns/queue starvation and require zero sustained starvation during
    a complete race.
  - Partial evidence: matched visible `--mute` runs through the XA-heavy menu
    path measured 1,178 source-underflow frames and 11 refills with the former
    1,024-frame prime, versus zero underflows/refills with the new 4,096-frame
    prime. ReadS delivery now uses the exact
    `75 / (60000 / 1001)` sectors-per-VBlank ratio instead of the slowly
    draining `75 / 60` approximation. The full visible race above recorded
    zero XA underflows/refills across all 51 race windows and zero SDL queue
    starvation across 704 reports. Three source-underflow reports occur only
    after the race music's channel ends, during the results/replay stream
    handoff; no recurring in-race drift remains. Full audible-race validation
    is still required before checking this item.
  - `artifacts\audio-health-race-visible-muted-final` exercises the real WASAPI
    device cadence under the session-only `--mute` switch while capturing the
    actual pre-volume SPU mix. Its 60.058-second, lossless FLAC race window has
    no silence of 20 ms or longer below -70 dBFS, no SDL starvation, and no XA
    underflow/refill. The temporary 10.1 MiB PCM was removed after compression;
    the retained FLAC is 7,900,450 bytes with SHA-256
    `BF46190E700D30356D91AF4F55B54960DF4C46F463566E59E5E97710870CE170`.
    A physical audible listen remains required for final acceptance.
  - The current final executable was revalidated in
    `artifacts\audio-health-final-build-meter-validation`: its corrected
    pre-volume meters report nonzero source peaks up to 32,766 and
    -13.21 dBFS RMS while the WASAPI device remains session-muted. The
    10.014-second FLAC has no >=20 ms silence, SDL starvation, XA underflow, or
    XA refill; the fixed harness also removes its temporary WAV automatically.
  - Final physical acceptance: the user ran the deployed build through the
    normal audible Windows audio path and reported that audio is perfect.

- [x] Keep automated/headless runs unconditionally muted.
  - `--headless` must always hide the window and force all output audio to zero.
  - A hidden/headless path must not briefly initialize audible output.
  - Verified in the exact packaged EXE: headless selects SDL's `dummy` driver
    and refuses startup if SDL reports any physical driver, in addition to
    forcing the software master volume to zero.

- [x] Preserve audio for real user testing.
  - A normal launch without `--headless` must honor the wrapper's volume and
    mute settings.
  - The headless safety behavior must not overwrite or persistently clobber the
    user's normal audio configuration.
  - `--mute` now provides a visible, session-only silent diagnostic path; a
    matched run left `MasterVolume=1` and `Muted=false` unchanged on disk.
  - The WASAPI pre-volume capture above proves `--mute` suppresses only the
    physical output: the complete source mix remains active and continuous,
    and the run again leaves saved volume/mute values untouched.
  - The packaged `settings.json` remains `MasterVolume=1`, `Muted=false` after
    all headless and `--mute` regressions. Runtime session muting is computed
    separately before audio initialization and never assigns either saved
    property; a normal launch therefore applies the saved values, while the
    wrapper Audio panel updates both the live gain and persisted values when
    the user changes them.

## Loose-files-only distribution and music

- [x] Make the shipped runtime completely independent of BIN/CUE/IMG/CCD/SUB
  disc images.
  - The packaged EXE must boot using only loose game files.
  - Menus, videos, dealerships, upgrades, licenses, every track, racing, and
    replay must load without the original image present.
  - Missing loose files must produce a clear error naming the missing file.
  - Current packaged evidence in `artifacts\loose-runtime-full-race-current`:
    all source CCD/CUE/IMG/SUB files were physically unavailable while the
    exact deployed EXE loaded the convenience save and upgraded Mazda,
    completed a native-AI race and natural replay, and returned to Results at
    poll 45,000. A second run with one loose asset withheld failed explicitly
    with `Standalone loose install is missing required assets (no BIN/CUE
    fallback): DISC_META.DAT`. Both runs proved the SDL dummy backend.
  - The standalone manifest maps all seven complete disc extents, including
    GT2's monolithic 488,241,152-byte `GT2.VOL` that supplies dealerships,
    upgrades, licenses, and every track through the same loose range reader.
    `tools\prepare_loose_install.py` now validates every extent against the
    exact Simulation Disc SHA-256 before packaging; all seven current files
    pass. Standalone `CueFs` has no image object and throws instead of falling
    back whenever an LBA is not covered.

- [x] Replace CD music with an external OGG queue.
  - Scan `OpenGTPS1\music\` for `.ogg` files.
  - Adding or removing OGG files must update the queue on the next launch.
  - Playback must advance through the queue and loop without stutter.
  - Game sound effects and video audio must remain intact.
  - Exact-package test `artifacts\ogg-queue-packaged-current` indexed three
    temporary OGGs, advanced through all three, re-entered queue item 1 with
    `loop=1`, and recorded zero SDL queue starvation. The temporary tracks were
    moved back out of the shipped `music` directory after the test.

- [x] Localize/display OGG metadata from the filename format:
  `[artist] - [song].ogg`.
  - Parse artist and song separately.
  - Display the parsed label in the wrapper.
  - Define sensible behavior for malformed filenames and duplicate names.
  - The packaged test parsed `Codex QA - Queue Track`, disambiguated its
    whitespace-normalized duplicate as `(2)`, and displayed a malformed
    filename as `Unknown Artist - Malformed Queue Track`.

- [x] Keep the deployable build exactly one folder below the repository root:
  `C:\Programming\GitHub\OpenGTPS1\OpenGTPS1`.
  - EXE, loose files, settings, memory cards, mods, and music must live in that
    folder.
  - Build and run scripts must not repopulate runtime clutter in the repository
    root.
  - The audited runtime is
    `C:\Programming\GitHub\OpenGTPS1\OpenGTPS1\GranTurismo2PC.exe`; generated
    QA PPMs are moved to `artifacts`, and the deployment root contains only the
    executable, required loose assets, DLLs, settings, cards, and intended
    runtime subdirectories.

## Save data and playability

- [x] Provide a verified convenience memory card with all licenses and
  approximately 100,000 credits.
  - The game must recognize the card through its normal memory-card UI.
  - Every license must appear completed after a clean launch.
  - The displayed credit balance must be approximately 100,000.
  - The save must remain usable for buying, upgrading, saving, and loading cars.
  - `tools\rewrite_and_validate_test_save.ps1` drove GT2's own Save Game UI,
    then clean-reloaded with the creation patch disabled. The live guest trace
    recovered `credits=100000`, `licenseTestsCompleted=60/60`, and
    `bronze=60/60`; the license UI visibly shows completed tests. The card's
    checksum-correct serialization remained byte-identical at SHA-256
    `ADA9AD3D2AB2BB8F276EC24122F6FCCD112764F39178AB81A5C98825824858BE`.
    Its purchased Stage-1-turbo Mazda was loaded and raced on Red Rock, Tahiti,
    and High Speed Ring.

- [x] Create an auto-drive validation harness using the game's own AI data.
  - Drive the player-controlled car from GT2's AI racing-line, braking, throttle,
    and steering data rather than a hand-authored input script.
  - Complete clean, repeatable laps without riding walls, teleporting, bypassing
    physics or collision, or granting the player car nonstandard capabilities.
  - Run a race through the finish and allow the resulting replay to reach its
    natural end without manual intervention.
  - Make runs deterministic enough to compare projection, seams, draw distance,
    LOD, dithering, frame pacing, audio, and replay behavior between builds.
  - Keep automated/headless harness runs unconditionally muted.
  - Verified with `tests\fixtures\ai-autodrive-save-sunday-race.input` and
    `RECOMPONE_GT2_AI_AUTODRIVE=1`: GT2's native mode-2 driver completes both
    laps with the purchased/upgraded Mazda, reaches Results, and drives the
    reconstructed replay car through the replay's natural end. The log records
    distinct `race` and `replay` engagements; SDL reports the `dummy` audio
    driver for the entire headless run. The post-fix packaged Enhanced run in
    `artifacts\cd-fifo-full-race-enhanced` is back on Results at poll 45,000
    without the former replay-exit sector retry loop.

- [x] Run a bounded multi-event crash-hunt soak.
  - `tools\test_gt2_soak.ps1` preserves and hash-verifies both memory cards and
    both settings files in a `finally` path, rejects known crash signatures,
    enforces a hard maximum of 180 minutes, and isolates every headless stage
    behind SDL's dummy audio backend.
  - The World League path completed five race/replay pairs on Trial Mountain,
    Laguna Seca Raceway, Apricot Hill, Rome Circuit, and Midfield Raceway. Each
    race ran 600 native AI ticks before an opt-in test transition asked GT2's
    original race flow to finish. The corrected final pass in
    `artifacts\gt2-soak-championship-final-20260730-114200` selected Continue
    after race five and returned to the league entry with the in-game day and
    credits advanced.
  - The economy paths purchased and upgraded a Mazda before a complete
    unshortened Red Rock Valley race/replay, then separately purchased a Nissan
    Silvia and purchased/equipped Nismo Sports Brakes. The retained crash-hunt
    logs contain zero unmapped-call, fatal-error, access-violation, or unhandled
    exception signatures and all end in a clean runtime shutdown.

- [x] Verify complete user-controlled playability in the packaged build.
  - Menus and videos render correctly.
  - Controller/keyboard input reaches the game.
  - A purchased and upgraded car can enter a race.
  - Vehicle tires remain on and interact with the track surface.
  - A race can be run through the finish, results, and replay without a crash,
    hang, broken load, or corrupted save.
  - The exact-package fixture supplies ordinary pad pulses only for title/save,
    map, event, transmission, and Results menus; the native AI hook does not
    engage until the race state. Those 49 pulses traverse GT2's normal LibPad
    path, while SDL independently enumerates the attached Xbox One Controller
    and the wrapper exposes complete keyboard/gamepad bindings.
  - `artifacts\convenience-save-packaged-current` proves GT2 loads the purchased
    and Stage-1-turbo-upgraded Mazda from a checksum-valid card. The native
    mode-2 harness then controls that same player vehicle through ordinary
    steering/throttle/brake input, tire contact, collision, and guest physics;
    it does not teleport or bypass the road surface.
  - `artifacts\loose-runtime-full-race-current` and the retained compressed
    replay prove menu/video loading, both race laps, finish, Results, and the
    replay's natural end in the packaged loose runtime without a crash, hang,
    broken load, or save corruption. Per the acceptance direction, authored
    human-quality driving is not required beyond proving that input reaches the
    game.

## Evidence required before checking items

- [x] Record a reasonably compressed complete-race replay video.
  - Let the race finish before entering replay.
  - Leave replay controls untouched.
  - Capture through the replay's natural end.
  - Use quality compression and a practical file-size ceiling; do not create
    multi-gigabyte evidence files.
  - Evidence:
    `artifacts\ai-autodrive-replay-video\GT2_AI_Enhanced_Replay.mp4`;
    H.264/yuv420p, 640x480, 30 fps, 233.33 seconds, 35,895,125 bytes,
    SHA-256
    `975DCB1BEBF1D82050442F86179CAB2C77B438876562BAA37E7749A495ABF76F`.
  - Rejected native-world evidence (retained for regression):
    `artifacts\native-live-replay-final\GT2_Native_Enhanced_Replay_Final.mp4`;
    H.264 High/yuv420p, 640x480, 30 fps, 216.63 seconds, 33,331,101
    bytes, SHA-256
    `1E7092C8E5ACE1DE7416BFF185863C5CC2BB7E36AF0E9694A06B9AF5E9DB571A`.
    Although the stream decodes end-to-end and reaches Results, review found
    recurring track-section pop-in plus intermittently missing tach/turbo
    needles and redline detail. The reported close-camera shapes around
    0:55-1:00 also required exact legacy comparison before acceptance. It is
    not final evidence.
  - Rejected native-world evidence (retained for regression):
    `artifacts\native-live-replay-corrected-final-v2\GT2_Native_Enhanced_Replay_Corrected.mp4`;
    H.264 High/yuv420p, 640x480, 30 fps, 6,499 frames, 216.633 seconds,
    33,437,139 bytes, SHA-256
    `D7093C0376D30D4B63A52A572C85532C296B235BE80FE5CB307008EA5B75282F`.
    The gauge and Results fixes are valid, but exact review confirms an invalid
    diagonal track surface at 0:55; it is not final evidence.
  - Accepted native-world evidence:
    `artifacts\native-track-surface-corrected-final-v3\GT2_Native_Enhanced_Replay_Track_Surface_Corrected.mp4`;
    H.264 High/yuv420p, 640x480, 30 fps, 6,499 frames, 216.633 seconds,
    33,224,796 bytes, SHA-256
    `76F3BB5C455EB87C741684EB689F25C73CFB847C2C1F2EA7091E9F58A5AEDA10`.
    End-to-end 4-fps motion review and every-frame review from 0:52 through
    1:02 find no false track surface, section pop-in, or gap. The complete
    tach/turbo layers remain present and the video reaches the PS1 Results UI.

- [x] Capture paired PS1 Quality and Enhanced replay frames from the same track,
  camera, and moment to prove projection, seams, draw distance, LOD, and
  dithering differences.
  - `artifacts\graphics-presets-replay-paired-final-38300` contains lossless
    PS1 Quality and Enhanced frames from the exact packaged executable at input
    poll 38,300, with the same Red Rock replay camera, lap time, cars, and
    vehicle positions. The long road/grandstand view differs by 22.83 dB PSNR /
    0.7663 SSIM.
  - The paired logs identify all five preset states. PS1 Quality records
    projection off, seam pass off, stock distance/LOD, and dithering on;
    Enhanced records projection on, 248,769 seam-padded triangles in the final
    trace window, forced vehicle model index zero, and dithering off. Both runs
    proved SDL dummy audio and left persistent wrapper settings unchanged. The
    old 107-object all-sector visibility count from this historical pair is
    explicitly superseded: it exposed occluded surfaces and is not evidence for
    the corrected distance policy. The accepted full-motion v3 replay validates
    that policy.

- [x] Run final checks from a clean copy of the exact deployment folder, with
  the source disc image unavailable and no development-tree dependencies.
  - `artifacts\clean-deployment-final-current` inventories 16 release files
    totaling 708,554,821 bytes, with zero BIN/CUE/CCD/IMG/SUB files. The audit
    copied only that inventory into a GUID-named Windows temporary directory,
    booted the copied executable as a seven-extent standalone loose install,
    found no generated/vendor path references, proved SDL dummy audio, and
    removed the isolated copy afterward.

## Explicitly deferred

- Resolution selection remains outside the quality preset system.
- Widescreen and its associated resolution work are for a future build.

## Version 2.0 - PSP asset glow-up and track geometry modernization

- [ ] Build an owner-supplied Gran Turismo PSP asset-import pipeline.
  - Use GTPSPVolTools or an equivalent reproducible extractor for the PSP
    volume, then decode ModelSet3/MDL3 models, texture sets, materials, model
    setup commands, course packs, and associated metadata.
  - Convert decoded data into a documented, versioned intermediate format
    shared by cars and tracks. Preserve source identifiers and transformation
    metadata so every converted asset can be traced and rebuilt.
  - Prefer host-side replacement assets loaded by the recompiled runtime over
    forcing the higher-detail data back through original PS1 file limits.
  - Create stable manifests that map each GT2 car and course ID to its PSP
    replacement, conversion settings, known differences, and original fallback.
  - Make conversion deterministic and cacheable, with validation for malformed
    offsets, unsupported commands, missing textures, invalid indices, and
    out-of-range geometry before an asset reaches the game.
  - Ship importer/converter code and manifests, not copyrighted donor data.
    Require users to build the optional asset pack from their own game files.

- [ ] Replace GT2 car models with their Gran Turismo PSP counterparts where a
  suitable match exists.
  - Import all useful PSP LODs, body meshes, wheels, glass, lights, badges,
    shadows, materials, diffuse textures, and specular data.
  - Translate wheel centers, axle positions, scale, origin, ride height, camera
    anchors, and other attachment points into GT2's coordinate conventions
    without changing the car's simulation data.
  - Preserve GT2 paint-color selection and any required race-number, light,
    brake-light, and reflection behavior through explicit material mappings.
  - Use bounded texture filtering and mip generation that retain the PSP visual
    character instead of producing mismatched modern materials.
  - Fall back cleanly to the original GT2 model for cars without a decoded,
    verified, or sufficiently close PSP donor.
  - Validate model scale, wheel contact, steering, suspension motion, culling,
    reflections, lighting, opponent rendering, garage views, races, and replays
    across representative body styles.

- [ ] Replace GT2 courses with their Gran Turismo PSP counterparts where the
  same or an acceptably equivalent layout exists.
  - Decode and import the visible course mesh, textures, materials, collision
    runway, sectors, boundaries, checkpoints, grid and pit positions, cameras,
    scenery objects, visibility data, minimap, and road-surface assignments.
  - Keep rendered geometry and collision coincident, and translate coordinates
    and units without altering the intended course length, width, banking, or
    elevation profile.
  - Adapt PSP sector and visibility information to the Version 2.0 renderer so
    complete courses remain practical without introducing streaming cracks or
    excessive per-frame geometry submission.
  - Map PSP surface properties to GT2 tire, curb, grass, sand, wall, and runoff
    behavior explicitly; do not infer gameplay surfaces from texture names.
  - Re-anchor start lights, signs, pits, cameras, scenery, and minimap data to
    the imported course and verify start/finish closure and lap timing.
  - Retain the original GT2 course as the fallback whenever the PSP conversion
    is incomplete or fails validation.

- [ ] Decode and translate PSP racing-line and AI-driving data independently
  from course geometry.
  - Identify the actual AI path, progress, speed/braking targets, boundaries,
    overtaking or alternate-line data, and direction metadata rather than
    confusing a visible driving aid with the AI controller's route.
  - Convert the PSP path into a neutral centerline-relative representation,
    then generate or adapt the data consumed by GT2's native mode-2 driver.
  - Reproject path heights onto the final collision surface while preserving
    lateral position, ordering, lap closure, and checkpoint progression.
  - When PSP AI data cannot be decoded completely, reconstruct an initial path
    from the collision runway and refine it using deterministic auto-drive
    telemetry instead of embedding hand-authored controller input.
  - Validate multiple vehicle speeds and performance levels for wall contact,
    missed checkpoints, oscillation, inappropriate braking, pit entry errors,
    and repeatable race completion.

- [ ] Integrate the PSP replacements as an optional Version 2.0 asset pack.
  - Add wrapper controls for original GT2 assets, the complete PSP glow-up, and
    per-car or per-track fallback while the pack is under development.
  - Detect missing, corrupt, incompatible, or partially generated asset packs
    at startup and report the exact asset and corrective action without
    preventing the original game from running.
  - Version the pack format separately from saved games and runtime settings so
    asset revisions do not invalidate player progress.
  - Verify every replacement in garage, race, replay, split-screen or other
    supported views, including loading, draw distance, LOD, frame pacing,
    collision, AI, timing, and natural replay completion.
  - Produce matched original/PSP screenshots and full-lap auto-drive telemetry
    for each completed course before enabling it by default.
  - Use decoded GT3/GTC assets, then verified GT4-family assets, only as
    owner-supplied gap-fillers where no suitable PSP donor exists; keep these
    converters and provenance mappings separate because their model, runway,
    and AI formats have different levels of support.

- [ ] Build a constrained track-mesh smoothing pipeline for the remaining
  original PS1 courses.
  - Use Red Rock Valley Speedway as the first proof-of-concept.
  - Smooth the rendered road and its collision surface together so the visual
    surface and tire contact remain coincident.
  - Infer the course direction and corresponding road cross-sections, then
    remove high-frequency height changes primarily along the direction of
    travel. Do not apply an unconstrained uniform or isotropic mesh smooth.
  - Preserve the original lateral road profile, corner radius, camber, banking,
    crests, major elevation changes, curbs, walls, boundaries, surface types,
    sector seams, and start/finish closure.
  - Begin by retaining the original topology and horizontal vertex positions,
    adjusting only road height where practical. Add longitudinal subdivisions
    and rebuild affected collision triangles only if the original sampling is
    too sparse for a smooth result.
  - Apply identical displacement to duplicated seam vertices and verify that
    smoothing does not create cracks, overlaps, inverted triangles, collision
    holes, or abrupt normal changes at streamed-section boundaries.
  - Reproject AI-line heights and road-anchored objects onto the resulting
    collision surface without changing the intended racing line in plan view.
  - Keep the operation deterministic and parameterized so its strength and
    locked features can be tuned per course without requiring manual edits to
    every road vertex.

- [ ] Validate the Red Rock Valley smoothed-course pilot before processing
  other tracks.
  - Complete repeatable native mode-2 auto-drive races and natural replays on
    both the original and smoothed course using the same car and setup.
  - Confirm that tire contact, collision, sector transitions, checkpoints,
    timing, AI progress, and race completion continue to function normally.
  - Record suspension travel, vertical acceleration/jerk, tire-contact loss,
    steering, speed, lap time, and racing-line deviation over matched runs.
  - Require a clear reduction in unintended high-frequency vertical motion
    without materially flattening deliberate banking or changing the course's
    driving line.
  - Inspect the full lap and replay for visible road cracks, floating scenery,
    buried or floating curbs, texture discontinuities, and vehicle reactions at
    every modified section.
  - Export the smoothed result in a form suitable for later manual texture
    remapping and replacement of trackside objects with higher-detail assets.

- [ ] Generalize the accepted Red Rock pipeline to every PS1 course that is not
  replaced by a higher-generation donor track.
  - Store per-track smoothing masks, locked features, and strength overrides as
    source-controlled data rather than hard-coded track-specific behavior.
  - Batch-check topology, seams, collision coverage, AI-line projection, and
    deterministic output before packaging each converted course.
  - Retain the untouched original course data so Version 2.0 can offer a safe
    comparison/fallback path during development and validation.
