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
