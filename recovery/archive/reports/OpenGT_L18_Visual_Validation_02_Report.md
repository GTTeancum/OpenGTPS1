# OpenGT L18 Visual Validation 02

**Date:** September 24, 2026  
**Scope:** Re-centered validation of L18 against the approved L08 rendering mandate.  
**Tracks:** Seattle Circuit daytime; Red Rock Valley Speedway dusk.

## Result

L18 is confirmed as a healthy source/runtime checkpoint with real native gameplay rendering on both required tracks and no broad visual/presentation regression in the retained native pixel proof. It is **not yet promoted over L08 as the formal visual authority** because the retained L18 proof frames are not sufficiently matched to the accepted L08 moving exterior-camera captures.

L08 remains the visual authority. L18 remains the current cumulative source checkpoint.

## Validation performed this turn

### 1. Retained L18 Linux runner smoke test

`OpenGT-L18-Linux-Ready-v2.zip` was recovered and executed in the current Linux environment.

`RUN-SMOKE-TESTS.sh` passed:

- Simulation replay-codec round trip: exact.
- Arcade replay-codec round trip: exact.
- car-preview camera verification: pass.
- native-world/raw-track/raw-background initialization: pass.

The runner intentionally contains no GT2 game payload.

### 2. Analyzer v4 self-test

`tools/analyze_unbake_audit.py --self-test` returned:

`unbake audit analyzer self-test: PASS`

No destructive unbake rule is enabled or authorized.

### 3. Recovered real L18 gameplay evidence

The retained artifact `OpenGT_L18_Fresh_Gameplay_Audit_01.zip` proves that the original Arcade guest previously executed real L18 races on both required tracks after the loose-disc inputs were restored.

Seattle evidence:

- 859 raw-track frames.
- 859 raw-background frames.
- zero track/background decode failures.
- zero guest-track/background fallbacks.
- 35 actual native renderer outputs, zero synthetic/repeated.
- clean exit 0.

Red Rock evidence:

- 1,109 raw-track frames.
- 1,109 raw-background frames.
- zero track/background decode failures.
- zero guest-track/background fallbacks.
- 40 actual native renderer outputs, zero synthetic/repeated.
- clean exit 0.

The explicit native pixel-proof sessions each reported 31 submitted/rendered/actual/consumed frames, zero synthetic/repeated/dropped output, zero output-wait timeouts, and clean exit 0.

### 4. Human inspection of retained L18 pixels

Inspected both retained Seattle L18 frames and both retained Red Rock L18 frames against the accepted L08 reference material.

Observed:

- Seattle retains the approved broad daylight character: neutral road surface, green authored vegetation, intact cars/scenery/HUD, and no obvious global over-lighting, under-lighting, geometry failure, or presentation failure.
- Red Rock retains the approved dusk character: warm track surface, dark structures/stands, blue-purple dusk sky, intact light fixtures/cars/HUD, and no obvious global exposure/color regression.
- No catastrophic shadow/presentation artifact is visible in the sampled L18 frames.

These samples are **not sufficient for formal parity acceptance**. The L18 proof frames are 640x360 start-grid/spectator views around polls 357/365, while the principal accepted L08 evidence is moving 1280x720 exterior-camera footage including matched poll-1529 comparisons. The available L18 samples do not adequately exercise moving world-anchor stability, cascade-edge transitions, or billboard behavior under camera rotation.

### 5. L08 -> L18 source-risk audit

Compared complete packaged L08 and L18 source trees.

- 1 file added (`tools/analyze_unbake_audit.py`).
- 18 existing files changed.
- The approved L05 preset is byte-for-byte identical.

High-risk render files verified **byte-identical L08 -> L18**:

- `lighting/in-game-L05.shader`
  - SHA-256 `29e55120508bb0540bf29ab633b4177c0f77bdb546c6b89bacfa390bcbd4cf6e`
- `lighting/lighting.shader`
  - SHA-256 `8beacc52f6f4644bf45536dc07e1adf7ba1d8a3473e9cca91a2e4f4701dbf9fc`
- `native/shaders/lighting.hlsl`
  - SHA-256 `2cf73d3e414f1219f79e5b284998ef849c216f330f46db49cdb7fbcbfe95516a`
- `native/shaders/lighting.hlsl.inc`
  - SHA-256 `70a017025881bed1fd791eb9a2bd7f1d4bf0e2254941227ce2827978ea115d54`
- `native/shaders/world.glsl`
  - SHA-256 `7037b5e704b7b3b3e2040b816a90fb0aecd85f506d03cb195a0307b90a02abee`
- `native/src/shadow_grid.cpp`
  - SHA-256 `a40c4a14a7916a069298991f6d91b16631436b67cf209a5f70f027d24186e783`
- `native/include/opengt/shadow_grid.hpp`
  - SHA-256 `b458db27cbe8b784163482c0c240d48b8a15d729687ecedc0f147bf07c214fe9`
- `native/src/world_gpu_renderer_egl.cpp`
  - SHA-256 `153041bf0d4849c95a37c317df00c2312f12187c577d62e138b24d1c5671e9a2`
- `native/src/world_gpu_renderer_d3d11.cpp`
  - SHA-256 `e2c46cf516b2554d0996e713926993a3927517cc5ecb80b71bded14c21a55dda`
- `native/src/world_gpu_renderer_native.cpp`
  - SHA-256 `31d642563e9d52cc70a79f18feca314f257bb3d249b4884c653d66eaebc0e3fd`
- `native/src/projected_reference_renderer.cpp`
  - SHA-256 `79d55cc851ac0bf2f7eb175b080522103901f8cd51a1dd986b9a1b928df4f4a4`

The meaningful L08->L18 render behavior changes are the intended L09/L10 billboard corrections in `native/src/lighting.cpp` plus the managed billboard identity plumbing:

- camera-facing GT2 course billboard quads are excluded from directional-sun caster geometry;
- camera-facing billboard display normals are not fed into dynamic BRDF/material lighting;
- billboard authored color is preserved;
- billboard presentation planes do not receive the projected-shadow/material path intended for physical road/track geometry.

The remaining L11-L18 changes are source-provenance, content identity, audit logging, temporal/cross-view/world-geometry qualification, and safety constraints for future destructive-unbake rules. They do not retune the approved L05 lighting preset or change the shadow-grid implementation.

## Current acceptance decision

**Do not promote L18 over L08 yet.**

The evidence is strong enough to say:

- L18 runs real Seattle and Red Rock gameplay;
- the approved lighting preset is unchanged;
- the core shader, shadow-grid, and GPU-renderer files listed above are unchanged;
- retained L18 pixels preserve the expected broad Seattle-day / Red-Rock-dusk visual character;
- no broad rendering regression is presently visible;
- destructive unbaking remains unsupported by evidence and stays disabled.

But the evidence is not matched enough to certify the visual behaviors that motivated L08-L10 under motion.

## Required final validation

When the GT2 payload is available again, do **not** depend on recovering the historical L08 settings/card state. Instead create one new controlled baseline and run **both L08 and L18** against the same inputs:

1. same reconstructed GT2 data root;
2. same generated `settings.json` and memory cards;
3. same Seattle direct-race player/track setup;
4. same Red Rock direct-race player/track setup;
5. same scripted acceleration/steering/camera inputs;
6. same 1280x720 output resolution;
7. same capture polls/authored frames and renderer backpressure;
8. capture Seattle moving exterior-camera sequence;
9. capture Red Rock moving exterior-camera sequence;
10. capture Red Rock camera-switch sequence.

Compare L08 vs L18 specifically for:

- world-anchored shadow stability during vehicle/camera motion;
- cascade-edge coverage/seams;
- receiver-plane stability;
- billboard color stability under camera rotation;
- absence of camera-rotating billboard-caster shadows;
- contact-shadow appearance;
- authored road/scenery color preservation;
- geometry/HUD/presentation integrity.

If those matched captures pass, promote L18 as the visual authority. If not, fix only the smallest demonstrated rendering regression and rerun the same matched sequence.
