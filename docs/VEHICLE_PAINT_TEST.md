# GT2000-inspired paint experiment

Isolated on `codex/gt2000-paint-specular-test`; not merged into main. This is a
small material prototype, **not** a claim of GT3/PS2 renderer parity.

## Scope

- One authored mask: Citroen Xsara 1.8i 16V (`fcx8n`), highest-detail source model.
- Bonnet, roof and selected door paint islands get smooth per-pixel specular
  highlights. The original colour/livery atlas and low-poly geometry are unchanged.
- Unmarked pixels, including opaque glass, lights and trim, retain GT2's original
  environment pass. Other cars, unsupported faces and lower-detail models fall
  back unchanged. This deliberately is not a whole-fleet material conversion.
- Lighting is a broad **viewer-relative** highlight, modulated by the guest's
  authored environment-pass intensity. It does not reflect track objects or track
  lamps, and is not shadow-aware scene lighting. No probes, extra scene renders,
  interiors, ray tracing, new geometry or new depth exceptions are introduced.

## Run locally

The separate build is `artifacts/paint-specular/test-build/GranTurismo2PC.exe`.
`Test Red Rock.cmd` launches the Xsara directly on Red Rock Valley Speedway;
`Test Red Rock Original.cmd` launches the same executable and track with the
experiment disabled. Launching the EXE alone opens the normal game. The installed
`OpenGTPS1/GranTurismo2PC.exe` is not replaced.

`OPENGT_VEHICLE_PAINT_DISABLE=1` disables the experiment before process startup.
`OPENGT_VEHICLE_PAINT_PACK` selects an alternate absolute pack path. Otherwise the
optional pack is loaded from `mods/gt2000_paint/generated/fcx8n.ogtpaint` under the
application working directory. An absent, malformed or incompatible pack retains
original materials; restart after changing the pack.

## Build the companion mask

```powershell
python tools/build_vehicle_paint_pack.py --volume work/arcade-disc/GT2.VOL
python tools/test_vehicle_paint_pack.py
cmake --build build/native --config Release --parallel 2
ctest --test-dir build/native -C Release --output-on-failure
```

The JSON in `mods/gt2000_paint/masks/` is hand-authored UV material ownership,
not a paint-colour heuristic. R stores strength, G gloss and A paint ownership.
A=0 takes the original material path. Point sampling prevents mask bleed into
adjacent glass/trim islands. Generated packs contain source-derived geometry and
are local/ignored, not redistributed in git. This pack is 252,552 bytes, including
a 256x224 RGBA mask with 7,339 marked texels and 482 unique source-face keys.

Runtime identity requires both the original 4-bit bitmap hash and matching
source geometry. The selected CLUT/paint colour is not part of identity. Only
the explicitly verified day and night bitmaps are accepted. The builder requires
identical positions, UVs and normals before sharing a mask between them. Only
that vehicle's authored additive environment triangles can be replaced; its
base diffuse commands, depth state and draw ordering are untouched. Source
normals use inverse-transpose transforms, and incomplete/singular/ambiguous
faces fall back as whole triangles.

## Evidence and reproduction

Red Rock replaces Midfield as the visual acceptance course to avoid its grid
shadow. `tools/test_vehicle_paint_live.ps1` runs a specifically targeted headless
game process with process-local AI input and native capture, isolated memory
cards, and no host keyboard/mouse/controller input or desktop capture.

The native viewer replays the same captured frame for A/B, at 16:9. Its `--detail`
option copies an exact rectangle from its own framebuffer without scaling or
retouching. Full frames are retained alongside the close-ups.

- `artifacts/paint-specular/redrock-detail-before.png`, `redrock-detail-after.png`
- `artifacts/paint-specular/redrock-committed-baseline.png`, `redrock-final-after.png`
- `artifacts/paint-specular/night-before.png`, `night-after.png` (SSR5 regression)
- `tools/verify_vehicle_paint_evidence.py` checks full-frame differences and glass.

The committed baseline renderer is rebuilt separately from `baa460c` for the
comparison. Feature-off output must match it exactly. The enabled output must
leave all pixels outside the car, including track geometry, HUD and SSR5 flares,
unchanged. This preserves the existing depth fixes; it does not re-certify every
possible camera position on every track.

Validation: all eight native test suites and four mask-builder tests pass. The
actual separately published executable completes the headless Red Rock test,
logs a matching paint vehicle, and emits its native presentation capture. The
day/night A/B assertions are in `artifacts/paint-specular/preservation.json`.

Three alternating runs of 120 measured frames each (8 warm-up frames, identical
Red Rock capture, hardware D3D11, 1280x960) gave mean end-to-end native pipeline
times of 7.24 ms original and 7.16 ms prototype. Individual paired differences
ranged from -0.44 to +0.27 ms: no consistent slowdown was distinguishable from
run-to-run noise. Do not interpret the negative aggregate as a speedup. These
numbers include CPU decode, topology, submission and GPU readback, not just GPU
shader execution, and cover one matched car rather than a fleet-wide stress test.
The prototype adds no draw calls (424 in both benchmark paths), no extra scene
render, and one 224 KiB mask plus small per-frame normal/UV buffers.

## Limits to evaluate in the prototype

The remaining original environment pass on unmasked bumpers/trim is intentional.
The mask is conservative and still needs an art decision before expanding to
all cars. At small race distances the difference is subtle. A viewer-relative
highlight is inexpensive but less natural than track-relative lighting; that
tradeoff is explicit in this experiment.
