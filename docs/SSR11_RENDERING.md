# SSR11 sky corruption correction — 2026-09-04

The reproduced black bands beneath the elevated roadway and corrupted horizon
blocks were a converted-asset palette-address defect, not depth ordering.
The installed game and reusable GT1 patch packages now contain the correction.
No renderer, executable, menu, save, or other-course changes were needed.

## Source reference and cause

SSR11 is imported from GT1; there is no stock GT2 SSR11 race to compare against.
The reference is the user's GT1 `BG.DAT` dawn3 artwork and stock GT2's shared
`bgsobj/dawn.bsp.gz` / `dawn.bso.gz` layout, not a substituted GT2 sky.

The three image planes and their placements match across games. GT1 uploads
their CLUTs to `(0,490)`, `(0,488)`, `(0,489)`, while the GT2 BSO reads
`(624,500)`, `(624,498)`, `(624,499)`. Those latter slots held SSR11 road/scenery
palettes. The cloud shader therefore sampled unrelated colors and transparency
bits, producing black bands. The horizon similarly sampled incorrect palettes.

The converter now copies only the native CLUT destination coordinates into the
GT1 sky TIMs, preserving every image byte and original palette/STP bit. Course
palette allocation reserves those three slots. Both texture packages and all
associated geometry texture packets are regenerated for **all six** variants:
forward, reverse, Arcade forward/reverse, two-player, and Hi-Fi. Updating only
the normal forward/reverse files is insufficient: normal Arcade uses `_a`.

## Verification

- Six Python asset tests pass, including original-image/palette preservation,
  malformed input rejection, and sky/course palette separation for all six
  variants. Source-dependent tests skip explicitly when discs are unavailable.
- All seven native CTest suites pass. No native code changed.
- Native process-local AI test: 24 presentation captures, every 600 stage polls
  through poll 14,400. Every saved capture was inspected individually in order.
  The first lap completed in **3:19.354**; the final image is lap 2 at 3:50.790.
  The repaired clouds/horizon, road surfaces, tunnel geometry, signs, foliage,
  and lamp occlusion remain coherent in the reviewed views.
- This is a full-route smoke test with 24 reviewed captures, **not** an
  every-rendered-frame review, a live-input test, or a visual certification of
  every variant/camera. The other five variant exports were asset-tested.
- Five exact before/after world-frame matches (7536, 7537, 7538, 7543, 7544)
  preserve all captured primitive geometry/material data except relocated
  CLUT addresses. Each frame preserves all 1,729–1,750 non-sky texture palettes
  byte-for-byte, including the 736–740 primitives whose addresses moved.
- Native resident equivalence audit: 7,379 frames / 16,238,841 matched triangles,
  no unmatched triangles or geometry/material-order mismatch in the final run.
- Saved SSR5 flare and Midfield tree/building regression replays remain
  byte-for-byte identical to the previous correct renders. SHA256 respectively:
  `B3F920441C5197F1FCCA096DFF5CB8CF747EAE244679784722F6016295A06A40` and
  `0DA11713F912BAE3D5625D8AF1C72F72D7724327613D9431B2A61503C7AFBF84`.
- Exactly **13** of 10,690 Arcade members changed: one sky BSP and six pairs
  of SSR11 TRP/TRO files. All 10,677 other members are byte-identical. The
  complete Simulation volume is byte-identical. Course selector data and saves
  were not changed. Tests used copies of the installed cards.

The broad brown road strip in an older September 2 screenshot did not reproduce
in the current baseline and is not attributed to this fix. The partial legacy
projected capture omits modern-owned geometry and is not a valid full-scene
stock reference; it was not used as parity proof.

## Evidence and reproduction

Local evidence root: `artifacts/ssr11-rendering/` (not distributed game assets).

- `before.png` / `after.png`: exact matched frame 7536, native D3D11 replay,
  1280×960, identical perspective/filter/HUD options.
- `bridge-source/scene-2.ogtwcap` / `final-lap/scene-1.ogtwcap`: matching inputs.
- `final-lap/race-600.png` through `race-14400.png`: native presentation proof.
- `asset-validation.json`: changed-member inventory and converted-asset hashes.
- `previous-ssr11-assets/`: the 13 replaced original asset members retained
  for rollback. `fixed-lap/` is the superseded partial-variant test, **not** the
  accepted result; use `final-lap/` and the root before/after images.

```powershell
python tools/test_gt1_ssr11_assets.py
ctest --test-dir build/native -C Release --output-on-failure
./tools/test_ssr11_rendering.ps1 -Tag check -EndPoll 14410 -FullGeometry
python tools/test_ssr11_capture_palettes.py artifacts/ssr11-rendering/bridge-source/scene-2.ogtwcap artifacts/ssr11-rendering/final-lap/scene-1.ogtwcap
```

## Installed identity

Run `OpenGTPS1/GranTurismo2PC.exe` normally; this is an asset-only correction,
with no extra runtime work or executable rebuild.

- EXE: `0804C125900DAE3664EED4522721FFE7C6D5DBC73B21E31F0437F0FCCEF48734`.
- Unified GT2.VOL: `4E96A123EC246169547A9AE7B4FB18BB93441CF2664C40728A27B67A1BA1BAFC`.
- Arcade member: `44941D3F76ED151B59A08BD9B2A1CC32EBE7AD7898778FEE5FD8564DC886E676`.
- Unchanged Simulation member: `9630AAD04CABF50AD702A3DBBCE77069153748385BCFFC02015797A0C708EEDD`.

## Cleanup

Each capture batch converted its own native PPMs to PNG and removed those exact
PPMs successfully. Final cleanup of the obsolete partial-variant run, generated
conversion staging, and duplicate Arcade volume was rejected by the execution
policy before running. No workaround was attempted: 244.38 MiB of that staging
remains, in addition to small diagnostic files. Repository size is 28.62 GiB;
C: has 154.82 GiB free. No user saves or unrelated files were deleted.
