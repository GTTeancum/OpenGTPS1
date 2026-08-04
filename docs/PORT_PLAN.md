# Port plan

## Fixed vertical slice

1. Boot the USA Simulation Disc and play or skip all authored videos.
2. Reach the simulation-mode home screen.
3. Acquire the starter license and credits needed by an ordinary fresh-save
   path, without modifying guest state from the host.
4. Purchase a car and install at least one performance upgrade.
5. Enter an eligible race with that same garage car.
6. Drive on the track surface under player control.
7. Complete every lap and reach the original results/prize flow.

## Long-term content and distribution roadmap

- Make first-run preparation a blocking installation step, implemented either
  inside `GranTurismo2PC.exe` or in a separate installer executable. The
  preparation tool must prompt for the user's own Gran Turismo 2 Simulation
  Disc (`SCUS-94488`, NTSC-U revision 2), Gran Turismo 2 Arcade Mode Disc
  (`SCUS-94455`, NTSC-U), and Gran Turismo (`SCUS-94194`, NTSC-U).
  `GranTurismo2PC.exe` must validate the prepared output and refuse to start
  Arcade Mode or Gran Turismo Mode until all three images have been validated
  and the conversion has completed successfully.
- Promote the implemented deterministic Simulation/Arcade dual-member
  `GT2.VOL` builder into the first-start installer. The development pipeline
  already produces one generated volume and unified installation while
  preserving each disc's exact original payloads and game flow.
- Continue automatically extracting, converting, and merging Gran Turismo
  1-exclusive content into the generated `GT2.VOL`. The supplied archives and
  their embedded databases are the sole authority for this work; see
  `docs/GT1_CONTENT_AUDIT.md`. A shared car with GT1-only paint or livery
  artwork must gain those choices under its existing GT2 identity rather than
  appear as a duplicate car. Special Stage Route 11 is
  now implemented as native forward, reverse, Arcade, two-player, and HiFi
  course data, including its `dawn3` background and exact `ARCADE.DAT` entry
    81 selection art. The first car family is also implemented: GT1's
    three Arcade-only EUNOS ROADSTER families are native tenth through twelfth
    Class C entries with their exact wordmarks, all twenty-three authored
    palettes, sorted Racing/Drift records, and content-matched GT2-native
    physics. The ROADSTER RS includes native structural conversion of its
    separate GT1 day/night models rather than a GT2 body substitution. Route
    11 and all three Roadsters have clean menu-to-race AI smokes. The GT1
    Civic Racer is integrated as a native tenth Class B entry with all three
    liveries and clean menu/race proof. The GT1 DB7 Coupe is integrated as a
    native ninth Class A entry with its exact selection artwork, all three
    paints, correct nine-entry roster wrap, and clean full-lap AI smoke. The
    GT1 IMPREZA Sedan WRX-STi version III is integrated as the tenth Class A
    entry with its exact selection artwork, all three liveries, converted
    day/night models, and a clean full-lap AI smoke. The GT1 SOARER 2.5GT-T
    VVT-i is integrated as the eleventh Class A entry with exact selection
    artwork, all three palettes, converted day/night models, correct
    eleven-entry roster wrap, and a clean full-lap AI smoke. The GT1 SUPRA RZ
    is integrated as the twelfth Class A entry with exact selection artwork,
    all three liveries, correct twelve-entry roster wrap, and a clean full-lap
    AI smoke. The GT1 S13 SILVIA Q's 1800cc is integrated as the thirteenth
    Class C entry using its exact named GT1 menu TIM, all three palettes,
    correct thirteen-entry roster wrap, and a clean full-lap AI smoke.
    The GT1 LANCER Evolution IV GSR is integrated as the thirteenth Class A
    entry with its exact named menu TIM, all three palettes, correct roster
    wrap, and a clean 9,000-poll race smoke. The GT1 ALCYONE SVX S4 is
    integrated as the eleventh Class B entry with its exact named menu TIM,
    all three palettes, correct roster wrap, and a clean 9,000-poll race
    smoke.
    The GT1 CELICA SS-II is integrated as the twelfth Class B entry with its
    exact named menu TIM, all three authored palettes, structurally converted
    UV-preserving body, target-owned 1,220 kg chassis, correct roster wrap,
    and a clean 9,000-poll race smoke.
    The GT1 CIVIC CR-X '91 Si is integrated as the thirteenth Class B entry
    with its exact named menu TIM, all three authored palettes, structurally
    converted UV-preserving body, target-owned 970 kg chassis, correct roster
    wrap, and a clean 9,000-poll race smoke.
    Imported Arcade physics preserve GT2's native sharing of byte-identical
    physical part records. The original `0xB000` database workspace has been
    relocated into a dedicated 1 MiB native guest arena at `0x80200000`,
    within the PC runtime's existing 8 MiB devkit RAM map and clear of retail
    game state. The 45,930-byte thirteen-car database has completed a full
    race smoke 874 bytes beyond the old corruption boundary. Normal Arcade
    classes are capped at the proven thirteen-entry frontend size until their
    arrays are explicitly expanded. The complete native `arc_carlogo`
    frontend address family is relocated into the free `0x80400000`
    devkit-RAM MiB with a validated `0xF0000` archive bound; this removes the
    stock `0x66000` overflow exposed by the thirteenth Class B car. Named GT1
    car artwork must be imported directly from the
    validated `MENU_RAW.ARC`/`MENU_IMG.ARC` pair; synthesized wordmarks remain
    prohibited.
    These completed Arcade conversions remain the native body/race validation
    harness. Six archive-proven distinct identities (`a-ian`, `h-vrn`,
    `s-pbn`, `t-oan`, `t-eln`, and `h-rxn`) now also have a generated gated
    Gran Turismo Mode layer containing all 24 owned part families, stock and
    Racing Modification bodies, all seven localized databases and exact GT1
    wordmarks, and all 60 rotations of each regional used-car roster under
    the correct manufacturer and color IDs. All six have clean GT Mode purchase
    and garage proof; the EUNOS Roadster additionally has clean upgrade,
    two-lap auto-drive, result, and replay proof. Full per-car eligibility,
    upgrade, save, and reload coverage remains a release gate. Complete the
    remaining GT1-exclusive cars, prize and Arcade liveries, paint schemes,
    and wheel variants through the same deterministic `GTPATCH.VOL` layer.
    The Cerbera LM `v-rbr` and Castrol Supra GT `tsplr` are explicit same-car
    livery-fold cases; their differing GT1 body bitmaps cannot be reduced to
    palette-only edits. Database-derived color-ID comparison currently
    authorizes 51 GT1-only choices across 34 same-car identities. Both
    mode-specific gated patch layers now contain their converted day/night
    bodies, extended car-info records, and explicit
    target-color-to-body-palette mappings. The data-driven native resolver is
    wired into both executables' selection, race, showroom, and replay paths,
    and the installer validates/activates both mode layers atomically.
    Default activation still awaits a successful rebuilt-host smoke matrix.
- Provide first-start BIN/CUE-to-loose-file conversion. The program prompts
  for the user's own Gran Turismo 2 Simulation Disc, Gran Turismo 2 Arcade
  Mode Disc, and Gran Turismo 1 disc, validates all three supported images,
  converts the required files, and performs the reproducible `GT2.VOL` merge
  into the unified loose installation without modifying or retaining the
  source images. The original unified title menu is entered only after this
  process succeeds. Subsequent starts reuse the validated generated install.
  Missing or damaged output blocks the game and directs the user back to the
  preparation tool.
- Support mods, including manifests, dependency ordering, conflict detection,
  and possible guided conflict resolution where changes can be merged safely.

## Fidelity gates

- **Boot/CD:** `SYSTEM.CNF` launches `SCUS_944.88`; ordinary files and raw
  Mode 2 movie/audio sectors read from the original image.
- **CPU/overlays:** every executed direct, indirect, callback, and dynamically
  loaded target resolves to original recompiled code or a source-backed host
  shim.
- **Graphics/video:** GPU command lists, VRAM transfers, MDEC input/output,
  display modes, menu framebuffers, and race rendering remain live. Wrapper
  presets select PS1 or enhanced projection, seam handling, track visibility,
  vehicle LOD, and dithering without changing guest gameplay state.
- **Input:** digital and analog controller state reaches the original pad data
  structures with stable edge timing.
- **Audio:** SPU voices and streamed XA/CD input remain paced and audible.
- **Simulation:** suspension, tire contact, drivetrain, collision, timing, and
  lap state advance through original fixed-point logic.
- **Persistence:** original memory-card code can create and reload a garage
  containing the purchased and upgraded car.
- **Completion:** end-to-end validation crosses the finish, presents results,
  enters and exits the replay, and returns through the normal game flow
  without runtime faults. The reusable fixture validates pad delivery and
  simulation response; unattended racing-line quality is not a runtime gate.

## GT2 graphics enhancements

- Texture projection correction uses the recovered per-vertex GTE depth in the
  host shader. Disabling it restores the PS1's affine interpolation.
- Road/model seam stabilization uses authored boundary identity, exact
  view-space T-junction subdivision, and deterministic coplanar ownership.
  The known Red Rock replay line was a 3D texel-center sampling fault and is
  corrected without screen-space expansion, collision changes, UV nudges, or
  changes to menus/video/2D art.
- Extended draw distance selects GT2's longer replay visibility path during a
  race. The maximum remains bounded by the authored track data.
- Maximum vehicle LOD forces selector `1`, the player-quality model, for all
  cars instead of selecting three distance-dependent representations.
- The enhanced geometry paths use the original development-console polygon
  buffer layout at `0x80200000`, expanded to `0x70000` bytes, so additional
  road and vehicle polygons are not silently discarded.
- PS1 Quality, Enhanced, and Custom live in the wrapper. Output resolution is
  deliberately independent and widescreen is deferred.

## RecompOne gotchas carried from the reference

- Start with call discovery plus linear sweep, but replace it with explicit
  function maps as boundaries become known.
- Treat overlay relocation and linked callback addresses as game-specific
  evidence, never as reusable Vigilante assumptions.
- Preserve simultaneous-source behavior for GTE operations.
- Pace CD streaming and polling loops; a static process can otherwise starve
  frames or never observe asynchronous input edges.
- MDEC tables may reside in dynamically loaded code/data and need address
  translation at DMA boundaries.
- Retain raw 2336/2352-byte sectors for STR/XA behavior.
- Pace single-speed `ReadS` with the exact NTSC ratio
  `75 / (60000 / 1001)` sectors per VBlank. Approximating VBlank as 60 Hz
  undersupplies 0.075 sector per second, which slowly drains XA buffering and
  becomes recurrent music stutter during a full race.
- Add host patches narrowly and only after the original instruction path has
  identified the blocked hardware/library contract.

## Proven bootstrap patches

- `0x80010954`: the original installs callback `0x80010928`, spins until four
  VBlank interrupts increment `0x80011DF4`, then removes the callback. Static
  execution cannot receive the callback while blocked in that loop, so the
  host explicitly presents four frames and advances the same counter.
- `0x8007D23C`: GT2's `VSync` wrapper waits for original callback `0x8007D294`
  to advance total/interval counters `0x801F0680/0x801F0684`. The host presents
  the requested frames at that boundary so the installed callback can run.
- `0x8008A088`/`0x8008A0A8` and `0x8008A0C8`–`0x8008AD6C`: GT2's PsyQ
  `CdSyncCallback`, `CdReadyCallback`, `CdControl*`, `CdSync`, and `CdReady`
  implementations are mapped explicitly. The host CD implementation queues
  command-complete callbacks one per device tick, preserving the asynchronous
  ordering required by GT2's command state machine. Immediate-result commands
  such as `Getloc`, `GetTN`, and `GetTD` also queue that callback; returning
  their bytes without it leaves the replay-exit transition permanently busy.
- `0x80089F18`: normalized PsyQ fingerprint match for `CdGetSector`; sector
  payloads are copied through RecompOne's loose raw-sector reader rather than
  an unserviced guest CD DMA path. During `DataReady`, `CdGetSector` drains the
  callback-scoped sector FIFO that raised the interrupt; it must not select a
  concurrently active XA/music sector from the drive's global position.
- `0x8007C550`: the original waits for GT2's CD command object at `0x801F0510`
  to become idle. The static host services the registered ready callback and
  presents frames inside that wait; state transitions remain in the original
  command/callback routines.
- The virtual drive advances the completed command's sector before invoking
  its `DataReady` callback. Guest callback code is allowed to issue a new
  `Setloc`/`ReadN`; advancing after the callback would incorrectly increment
  that new location and can lock replay exit into a one-sector retry loop.

## Explicit main-executable continuations

- `0x8007C32C`: reached by the first boot GPU-environment setup; the bootstrap
  scanner stops at an earlier return in the same assembly dispatcher.
- `0x8007EA68`, `0x8007EA88`: original memory-card event callbacks delivered
  by the BIOS event path during the first post-initialization VBlank.
- `0x80010AF0` has an exact size of `0x178`. Heuristic discovery incorrectly
  split it at post-`JAL` continuation `0x80010C20`, skipping its epilogue and
  corrupting the caller's saved registers and stack.
- Overlay `longjmp` runs through a managed trampoline. Restoring registers and
  recursively calling the target leaves abandoned overlay frames on the CLR
  stack; throwing to the trampoline provides the non-local unwind required by
  the original setjmp/longjmp contract.
