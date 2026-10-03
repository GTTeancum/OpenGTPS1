# OpenGT L18 Structural Validation 03

**Date:** September 24, 2026  
**Scope:** Rendering-centered validation of L18 against L08 without changing the formal visual-authority boundary.

## Result

L18 is now independently revalidated in the current Linux environment as a clean, healthy renderer/source checkpoint. A fresh native build passes **18/18 renderer test suites**, the packaged managed smoke tests pass, the unbake analyzer self-test passes, and the intended billboard-shadow correction is explicitly regression-tested.

This does **not** replace the missing matched L08-vs-L18 moving-camera visual run. L08 therefore remains the formal visual authority.

## L08 -> L18 source differential

Complete packaged source-tree comparison found only **19 differences** total: 18 changed files plus one added analyzer tool.

Approved lighting inputs remain byte-identical:

- `lighting/in-game-L05.shader` — `29e55120508bb0540bf29ab633b4177c0f77bdb546c6b89bacfa390bcbd4cf6e`
- `lighting/lighting.shader` — `8beacc52f6f4644bf45536dc07e1adf7ba1d8a3473e9cca91a2e4f4701dbf9fc`

No active shader contains destructive `bakedGain` / `removeShadowOverlay` directives or source-provenance bindings.

High-risk renderer code that remains byte-identical includes the shadow grid, lighting/world shaders, EGL/D3D11/native GPU renderer implementations, projected reference renderer, vehicle-shadow source, and core PS1 raster/GPU path.

## Intentional visual change after L08

The substantive rendering behavior change is the L09/L10 billboard repair:

- raw GT2 course billboard quads are marked explicitly with `TrackBillboard` / primitive flag bit 9;
- camera-facing billboard presentation quads are excluded from directional-sun caster geometry;
- their authored color is preserved instead of treating their camera-facing display normal as physical lighting geometry.

Later L11-L18 changes add resident/source provenance, audit evidence, content identity, and safety gating for future destructive-unbake rules. They do not retune the approved L05 lighting values or shadow-grid implementation.

## Fresh native build/test

Built the exact packaged L18 `native/` source in the current Linux container using GCC 14.2 and CMake 3.31.6, Release configuration, `OPENGT_BUILD_TESTS=ON`.

All **18 / 18** native suites passed:

1. shadow grid
2. shadow anchor capture
3. lighting
4. resident overlay
5. renderer core
6. projected capture
7. world capture
8. world draw list
9. world topology
10. world interpolation
11. EGL lighting
12. live lighting
13. shadow visibility
14. vehicle shadow source
15. depth precision
16. depth precision fallback
17. shadow projection
18. track shadow materials

The initial aggregate command reached the outer command timeout while entering EGL; EGL and all remaining suites were then executed separately and passed. This was not a test failure.

## Billboard regression proof

`native/tests/lighting_tests.cpp` explicitly verifies the behavior that motivated L09/L10:

- billboard quad contributes zero directional-sun casters;
- authored billboard color remains passthrough;
- billboard cannot acquire unbake/removal material state;
- suppressing billboard caster geometry does not suppress stable physical track casters;
- rotating the camera-facing billboard presentation plane leaves the stable caster count and caster stream unchanged.

Those assertions passed in the fresh build.

## Other current-environment checks

`RUN-SMOKE-TESTS.sh` passed:

- Simulation replay codec exact round-trip;
- Arcade replay codec exact round-trip;
- car-preview camera verification;
- native-world/raw-track/raw-background initialization.

`tools/analyze_unbake_audit.py --self-test` returned PASS.

No destructive unbake rule is enabled.

## Visual sanity check

Retained L08 accepted Seattle/Red Rock poll-1529 frames and retained L18 Seattle/Red Rock poll-365 native frames were directly inspected again.

No broad regression is apparent in daylight/dusk character, authored road/scenery color, geometry, cars, HUD, or presentation. However the frames are not matched in camera, resolution, race state, or poll, so this remains a sanity check rather than visual-parity certification.

## Remaining acceptance gate

The exact GT2 runtime payload is not present in current retained artifacts. The restore manifest records identities/hashes only and does not contain retrievable game-data handles.

Formal promotion still requires one new controlled baseline run of **both L08 and L18** with:

- identical GT2 data root;
- identical generated settings/cards;
- identical Seattle and Red Rock direct-race setup;
- identical acceleration/steering/camera script;
- 1280x720 output;
- identical capture polls/authored frames.

The matched comparison must verify world-anchored shadow stability, cascade edges, receiver-plane stability, billboard color/caster behavior, contact shadows, authored world color, and presentation integrity.

Until that run exists: **L08 = visual authority; L18 = source/runtime authority.**
