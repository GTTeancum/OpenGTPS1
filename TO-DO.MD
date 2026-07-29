# OpenGTPS1 TO-DO

Nothing in this list is considered complete merely because a setting, code path,
or wrapper control exists. An item can be checked only after its effect is
visible and repeatable in the packaged build at:

`C:\Programming\GitHub\OpenGTPS1\OpenGTPS1\GranTurismo2PC.exe`

## Native renderer and original Xbox

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
- [x] Integrate native race/replay rendering while retaining the proven PS1
  presentation path for menus, loading screens, display-mode transitions, and
  MDEC video, plus PS1 screen-primitive semantics for the HUD.
  - The packaged runtime records complete provenance, explicit screen-space
    primitives, and VRAM into three bounded reusable OGTWCAP v4 buffers. A
    persistent D3D11 worker consumes only the newest frame and publishes
    reusable RGBA buffers without blocking emulation or touching the
    filesystem.
  - `artifacts\live-native-full-tail-fixed` reaches input poll 45,000 through
    the purchased/upgraded Mazda race and natural replay, renders 19,502
    native frames, engages the deterministic driver for race pass 1/2 and
    replay pass 2/2, proves both SDL dummy-audio checks, and exits cleanly.
    It records no native disable, fatal error, or unhandled exception.
  - The post-replay 352x300 viewport at 4x scale exceeds the deliberately
    bounded 4,915,200-byte native output pool. The renderer logs one compositor
    fallback, returns to the PS1 presentation path without disabling native
    rendering, then completes audio, native renderer, host, and runtime
    shutdown.
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
  - The packaged native renderer now resolves authored topology and the 0:33
    texture-sampling fault without padding. The exact final input-poll 32,492
    presentation is retained at
    `artifacts\native-live-replay-final\presentation-32492.png`; it has no
    diagonal road crack. Screen-space sprites use edge-based sampling while 3D
    polygons use texel-center sampling, so the minimap/tachometer also avoid
    the former one-column edge artifacts.

- [x] Extend draw distance far enough to show the entire track.
  - Track geometry and scenery must remain visible to the farthest practical
    distance instead of appearing in streamed chunks near the camera.
  - Distant geometry must not disappear when the camera changes direction.
  - The implementation must cover every track, not only the current test track.
  - Current packaged evidence: Enhanced discovers and submits the complete
    native visibility union on Red Rock Valley (107/107 sector objects),
    Tahiti Road (135/135), and High Speed Ring (95/95). On Tahiti, disabling
    only extended distance removes roughly 34,000 triangles from the matched
    120-frame window.
  - The runtime contains no track IDs, names, or per-track counts. It derives
    the exact sector count from the loaded track's sector-pointer table, unions
    every native object index, and returns that same complete set on every
    race/replay visibility call regardless of camera direction. The exact-table
    regressions scan 107, 135, and 95 entries per call on the three structurally
    different validated tracks; the isolated stock control averages only 17.34
    entries per call on Red Rock.

- [x] Force the highest available LOD at all distances.
  - Cars, track objects, and scenery must use their highest-detail model.
  - Objects must not visibly swap to lower-detail models with distance.
  - Validate both normal racing and replay cameras with multiple opponents.
  - Current packaged evidence: the wrapper Maximum path forces GT2's one-based
    vehicle selector `1`, which resolves to model-table index `0`, on all three
    validated tracks. Tahiti's matched Stock/Maximum frame differs at
    32.30 dB PSNR while the Maximum log identifies selector/index 1/0.
  - Authoritative race/replay trace
    `artifacts\visibility-lod-maximum-race-replay-trace` recorded 25,316 live
    race visibility calls and 771 reconstructed-replay calls. Across 2,791,309
    emitted track/scenery entries, every two-bit LOD selector was zero (highest
    detail), while vehicles simultaneously used selector `1` / model index
    `0`. The isolated Stock control scanned 305,806 entries and found 285,610
    nonzero selectors, proving the wrapper option causes the all-highest result.

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
  - Final native-world evidence:
    `artifacts\native-live-replay-final\GT2_Native_Enhanced_Replay_Final.mp4`;
    H.264 High/yuv420p, 640x480, 30 fps, 216.63 seconds, 33,331,101
    bytes, SHA-256
    `1E7092C8E5ACE1DE7416BFF185863C5CC2BB7E36AF0E9694A06B9AF5E9DB571A`.
    The stream decodes end-to-end, covers the corrected live replay, and
    reaches the original Results screen through the PS1 compositor.

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
    trace window, all 107 visibility objects, forced vehicle model index zero,
    and dithering off. Both runs proved SDL dummy audio and left persistent
    wrapper settings unchanged.

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
