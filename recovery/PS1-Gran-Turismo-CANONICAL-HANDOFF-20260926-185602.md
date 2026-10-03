# PS1 Gran Turismo / OpenGT — CANONICAL HANDOFF

**Last updated:** September 23, 2026  
**Project:** PS1 Gran Turismo / OpenGT lighting and shadow work  
**Canonical status:** Latest cumulative checkpoint is **L18**. L18 has fresh real Linux gameplay/runtime validation across Arcade + Simulation/unified flow. **L08 remains the last formally matched visual-acceptance checkpoint** because its exact historical save/settings state is unavailable.

---

## MANDATORY HANDOFF RULE

This Markdown file is the canonical transfer document for this project.

**At the end of every future work turn, the assistant MUST:**

1. Update this same handoff with all material progress from that turn.
2. Record any new artifact filenames and SHA-256 hashes.
3. Record source files changed, tests run, validation results, regressions, blockers, and exact next steps.
4. Preserve the distinction between **source/test validation** and **actual gameplay/visual validation**.
5. Return the updated handoff file to the user at the end of the turn.
6. Never assume a new chat can see previous chat history. This file must remain sufficient to transfer the project.
7. If a checkpoint is superseded, keep enough history to explain what changed and which checkpoint is authoritative.
8. Do not claim gameplay success, visual acceptance, destructive unbaking, or runtime behavior unless it was actually executed and verified.
9. If no meaningful project state changes during a turn, still update the `Turn Log` section so the file remains current.

A new chat should be able to continue by receiving **this file plus the latest relevant project/source archive and any runtime inputs described below**.

---

# 1. CURRENT AUTHORITATIVE STATE

## Latest cumulative source checkpoint: L18

Latest delivered artifact:

- `OpenGT_Lighting_L18_World_Geometry_Unbake_Audit_Candidate.zip`
- SHA-256: `4ebbd8391e445b792f21dfffa9b23bb15a75d83054edefc0761349e96cbfd0d3`
- Report: `OpenGT_Lighting_L18_Candidate_Report.md`

L18 is cumulative over L17 and contains **400 source files**.

L18 is a **non-destructive source/runtime-evidence checkpoint**. It is **not** a fresh gameplay validation and it does **not** enable baked-light removal.

### L18 changed files relative to L17

Exactly four source files differ from L17:

- `native/include/opengt/lighting.hpp`
- `native/src/lighting.cpp`
- `native/tests/lighting_tests.cpp`
- `tools/analyze_unbake_audit.py`

`overlay/` contains the exact replacement bytes. Applying it to L17 reconstructs all 400 L18 source files byte-for-byte. `L18-from-L17.review.patch` is review-only.

### L18 validation

The exact L18 source passed:

- **1,195 / 1,195 focused lighting checks** in Release.
- **1,195 / 1,195 focused lighting checks** under Clang 17 ASan/UBSan.
- **18 / 18 native suites** in Release.
- **18 / 18 native suites** under Clang 17 ASan/UBSan.
- EGL pixel tests.
- Shadow-visibility tests.
- Analyzer v4 self-test including:
  - temporal qualification,
  - immutable-content qualification,
  - cross-view qualification,
  - authored-color ambiguity rejection,
  - geometry-instability rejection,
  - legacy-log handling,
  - truncated-input handling.

Sanitizer validation reused the already validated L17 Clang ASan/UBSan tree, rebuilt/relinked all L18-dependent targets, then hash-compared the complete 400-file source against the packaged L18 source. `validation/sanitizer-source-equivalence.json` reported zero differences.

---

# 2. LAST ACTUAL GAMEPLAY-VALIDATED CHECKPOINT: L08

**L08 remains the last actual gameplay-validated/accepted checkpoint.**

Do not describe L09-L18 as visually accepted or gameplay validated until fresh runtime execution has occurred.

L08 report:

- `OpenGT_Lighting_L08_Report.md`

L08 contains **399 cumulative source files** and is a source recovery/overlay, not a standalone game distribution.

## Approved look that must remain unchanged unless explicitly re-approved

Scope:

- **Seattle Circuit — daytime**
- **Red Rock Valley Speedway — dusk**

Approved preset:

- `lighting/in-game-L05.shader`
- SHA-256: `29e55120508bb0540bf29ab633b4177c0f77bdb546c6b89bacfa390bcbd4cf6e`

Known approved controls preserved through L18:

- Seattle contact opacity: **0.48**
- Red Rock contact opacity: **0.50**
- Projected strength: **0.80**
- Cascade size: **2048 × 2048**
- Sun directions/colors/intensities not retuned after L08.
- Ambient, roughness, clearcoat, reflection controls, and textures not retuned.
- Night lighting remains deferred.

## L08 defects fixed

L08 repaired:

1. **Primary-camera provenance**
   - Prevented an auxiliary scenery transform from being mistaken for the track camera.
   - Red Rock final run selected verified primary model `800B9228` rather than auxiliary `80105974`.

2. **World-anchored shadow texel grid**
   - Shadow atlas center snaps to world-space texels.
   - Uses the full inverse camera transform with the source X/Z/Y convention.
   - Handles quantized scale/shear rather than assuming an orthogonal camera basis.
   - Renderer-owned scale lock prevents nearest-car/camera normalization changes from resizing the grid.

3. **Receiver-plane filtering**
   - Uses a full dual-basis transform rather than an orthogonal dot-product shortcut.

4. **Cascade edge coverage**
   - Transfers to the far map before the near-map filter footprint leaves valid coverage.
   - Regression fixture: corrected result had zero false-bright pixels in 12,544 tested pixels; the reverted coverage behavior produced 5,496 false-bright pixels in that fixture.

## L08 real gameplay execution

Four final native Linux sessions rendered **5,396 native frames** with lighting and sun rendering active:

| Session | Native frames | Captured backbuffers |
|---|---:|---:|
| Seattle matched exterior-camera drive | 1,339 | 115 |
| Red Rock matched exterior-camera drive | 1,339 | 115 |
| Red Rock camera switches + steering | 1,559 | 58 |
| Seattle independently reconstructed build | 1,159 | 25 |

All sessions:

- exited normally at configured input-poll limits,
- had zero dropped/repeated/synthesized output,
- had zero native output-wait timeouts,
- retained normal race construction, cars, wheels, opponents, physics, sky, scenery, and HUD,
- used scripted controller input to accelerate, steer, and change camera,
- were bounded early-race sessions, not completed races.

Visual evidence included exact 1280×720 PNG/PPM captures and silent MP4 clips derived from real sampled backbuffers. Mesa llvmpipe software OpenGL was used; those runs were not hardware-GPU performance benchmarks.

## L08 validation

Among the retained validation:

- **18 / 18 native Release suites passed**
- **18 / 18 native ASan/UBSan suites passed**
- **2,906 managed source-camera/live-header checks passed**
- **29 managed host/window checks passed**
- **16 managed vehicle-source checks passed**
- **1,846 native world-grid checks passed**
- 34-check native reader invocation for the real managed writer's 160-byte header passed

The L08 native library identity recorded in the report is:

`343a97ba4bc62427bfd3ac71031eea82cf8ec4b6e9bc5346d668896fd0b06032`

---

# 3. CHECKPOINT HISTORY AFTER L08

## L09 — billboard shadow stability

Artifact:

- `OpenGT_Lighting_L09_Billboard_Shadow_Candidate_v2.zip`
- SHA-256: `2ec877bc522dcac1887931a310ac8685882ef23e69db82d1ab30117fb954fe68`
- Report: `OpenGT_Lighting_L09_Candidate_v2_Report.md`

Purpose:

- Added managed `PrimFlags.TrackBillboard`.
- Serialized billboard identity as primitive flag bit 9.
- Native `world_primitive_track_billboard_flag` carries the identity.
- Directional-sun caster collection rejects:
  - explicit bit-9 billboards,
  - legacy bit-8 billboard-depth subset.
- Camera-facing billboard presentation planes no longer become unstable sun-shadow casters.
- No fixed billboard azimuth or invented physical tree/sign geometry was introduced.

Validation:

- 1,144 / 1,144 focused checks.
- 18 / 18 Release suites.
- 18 / 18 ASan/UBSan suites.
- Exact 10-file overlay reconstruction to all 399 source files.

Historical L08 path activity showed billboard primitives are materially active:

- Seattle: 96,619 billboard primitives across 1,339 frames.
- Red Rock: 16,068 across 1,339 frames.
- Red Rock camera-switch run: 18,708 across 1,559 frames.

L09 is not fresh gameplay validated.

## L10 — billboard lighting stability

Artifact:

- `OpenGT_Lighting_L10_Billboard_Lighting_Candidate.zip`
- SHA-256: `065c36b7da768795553434b7a655819ac13a32d28e44769c9a5faea0facff709`
- Report: `OpenGT_Lighting_L10_Candidate_Report.md`

Purpose:

- Billboard cards no longer feed their camera-facing display normal into the new BRDF/material lighting path.
- Billboards preserve authored color.
- Billboard presentation planes do not receive projected shadows.
- L09 caster exclusion remains in force.
- Ordinary road/track geometry continues to use the approved material lighting and projected shadows.
- No fake cylindrical caster, fixed azimuth, or replacement geometry is invented.
- Source-proven untextured reverse-subtract vehicle contact footprint remains unchanged.

Validation:

- 1,146 / 1,146 focused checks in Release and sanitizer.
- 18 / 18 Release suites.
- 18 / 18 ASan/UBSan suites.
- Tested native renderer SHA-256:
  `a07ea40423eef3ce280c1e15bd50d3ee9b62721eac8039b6a1b690e48f8fb50e`
- Exactly three native files changed from L09.
- Overlay reconstructs all 399 source files byte-for-byte.

L10 is not fresh gameplay validated.

## L11 — exact destructive-unbake provenance

Report:

- `OpenGT_Lighting_L11_Candidate_Report.md`

Problem addressed:

`primitiveKey` alone can alias duplicated course primitives with identical geometry/UV/material bits, so it is too weak for destructive baked-light edits.

L11 added live exact source provenance:

- 64-bit resident mesh key.
- Original 32-bit primitive address.

A destructive unbake rule required all four:

1. exact track,
2. `primitiveKey`,
3. `sourceMeshKey`,
4. `sourcePrimitiveAddress`.

Historical file captures do not fabricate this live-only provenance; decoding explicitly zeros the fields.

No baked-shadow artwork was removed.

Validation:

- 1,160 / 1,160 focused checks.
- 18 / 18 Release suites.
- 18 / 18 ASan/UBSan suites.
- Release native renderer SHA-256:
  `9f18aa1bb53182df0ab5224d8307aacdb36cdc059a969d923e76972dc6bedd01`

## L12 — path-stable physical source identity

Report:

- `OpenGT_Lighting_L12_Candidate_Report.md`

Problem addressed:

GT2 maintains separate Primary and Alternate projection-path definitions for the same immutable course mesh. Using a projection-path-dependent resident definition key could make a destructive rule flicker as a physical primitive moved between paths.

L12 separated:

- transient resident-definition lookup identity,
- path-neutral physical source identity used by destructive lighting.

For indexed resident course geometry, `sourceMeshKey` became an exact packed pair:

- high 32 bits: authored contiguous source vertex-table address,
- low 32 bits: authored vertex count.

It is **not a hash**.

`sourcePrimitiveAddress` remains the original 32-bit primitive record address.

Validation:

- 1,160 / 1,160 focused checks.
- 18 / 18 Release suites.
- 18 / 18 ASan/UBSan suites.
- Release native renderer SHA-256:
  `71008fb7f70cebb20b3d8a582683d5fffb16f5e02c70f03695e3bb1e2ca92007`

## L13 — resident lookup reset-alias hardening

L13 changed resident lookup identity so the resident key is derived from the **complete serialized immutable mesh definition**, with the key field treated as zero while hashing.

Purpose:

- Prevent reuse of the same guest/model/source addresses after reset/reload from aliasing a different resident definition.
- Keep path-neutral physical source identity separate from transient resident lookup identity.

L13 is a bridge/safety checkpoint between L12 provenance work and the L14 audit recorder. No gameplay acceptance was claimed.

## L14 — diagnostic-only exact unbake evidence audit

Artifact:

- `OpenGT_Lighting_L14_Exact_Unbake_Audit_Candidate.zip`
- SHA-256: `cf6adea0d53a95c9fa3d31a4a2b25eff583aae64330b4f78836f3a8663c6256a`
- Report: `OpenGT_Lighting_L14_Candidate_Report.md`

Enabling:

`OPENGT_LIGHTING_UNBAKE_AUDIT=1`

The real native lighting `prepare()` path records eligible resident course triangles with exact provenance including:

- track,
- frame/poll,
- inferred road/track class,
- `primitiveKey`,
- path-stable `sourceMeshKey`,
- exact `sourcePrimitiveAddress`,
- object/model identity,
- texture page/CLUT,
- flags,
- source UVs,
- uniform authored RGB,
- world-up alignment.

Eligibility is deliberately conservative:

- resident course only,
- textured,
- opaque,
- non-billboard,
- nearly horizontal,
- uniformly vertex-colored road/track triangles.

Rejected:

- missing provenance,
- historical captures,
- semitransparency,
- billboards,
- vehicles,
- nonuniform color,
- non-horizontal geometry.

`tools/analyze_unbake_audit.py` only reports neutral dark-vs-bright authored-color **candidates**. It never edits the shader and reports `destructiveRulesGenerated=false`.

Validation:

- 1,174 / 1,174 focused checks.
- 18 / 18 Release.
- 18 / 18 ASan/UBSan.
- Exact L13→L14 reconstruction for all 400 files.

## L15 — temporal qualification

Report:

- `OpenGT_Lighting_L15_Candidate_Report.md`

Audit stages:

- stage 0: first sighting,
- stage 1: at least 3 distinct frames spanning at least 30 native frames,
- stage 2: at least 8 distinct frames spanning at least 180 native frames.

An identity must reach stage 1 before it can contribute to a candidate group. Stage 2 is stronger evidence.

L14 logs remain parseable but cannot satisfy the new temporal qualification.

Validation:

- 1,182 / 1,182 focused checks.
- 18 / 18 Release.
- 18 / 18 ASan/UBSan.
- Release native SHA-256:
  `c5616032f8c4b8afaa070f06bc145365c7700307383b58771c84a15ee8b6e759`
- Sanitizer native SHA-256:
  `2f6126a7b9a4b92408564320155bc8c3ef272ceaf621800a0851ae3fbdeabc5f`

## L16 — immutable-content + cross-view qualification

Report:

- `OpenGT_Lighting_L16_Candidate_Report.md`

Added live-only `residentContentKey`:

- FNV-1a over the complete serialized immutable resident definition.
- The eight-byte resident registry definition key is treated as zero during fingerprinting.
- Primary/Alternate registry definitions with the same immutable source bytes produce the same content fingerprint.
- Changed geometry/material/UV/color/primitive bytes change the fingerprint.
- Serialized `.ogtwcap` files do not carry this live-only provenance and decode it as zero.

Cross-view evidence:

- camera forward orientation goes into coarse 15-degree yaw/pitch buckets,
- maximum angular separation from the first observation is retained.

Analyzer v3 candidate requires:

1. L15 temporal qualification,
2. nonzero immutable `residentContentKey`,
3. at least two view buckets and at least 8° maximum view separation.

Stronger evidence:

- stage 2,
- at least three view buckets,
- at least 20° separation.

Validation:

- 1,183 / 1,183 focused checks.
- 18 / 18 Release.
- 18 / 18 ASan/UBSan.
- Release native SHA-256:
  `ed64ed13558736818141983725da22ee6f3fd533c1d954ac74159cda159bf457`
- Sanitizer native SHA-256:
  `e07f9c47e434e553f64c8e344a371f76dced6e67910db8591f38c731234b12c1`

No destructive rules were generated.

## L17 — content-bound destructive rule contract

Artifact:

- `OpenGT_Lighting_L17_Content_Bound_Unbake_Rules_Candidate.zip`
- SHA-256: `4d7e515d51a3768ef91edd48d1db39361e93c652fb558fbc81ddcf33f8eec24e`
- Report: `OpenGT_Lighting_L17_Candidate_Report.md`

L17 made future destructive rules require all five exact provenance fields:

1. exact track,
2. `primitiveKey`,
3. `sourceMeshKey`,
4. `sourcePrimitiveAddress`,
5. `residentContentKey`.

`Material` now carries `resident_content_key`, the shader language accepts `residentContentKey`, and `select_material()` checks it before a destructive rule can win.

A content mismatch leaves gain at 1 and overlay removal disabled even if the older address fields match.

Non-destructive ordinary material rules remain compatible.

Validation:

- 1,192 focused checks in Release and sanitizer.
- 18 / 18 Release.
- 18 / 18 ASan/UBSan.
- Exactly three source files changed from L16.
- Overlay reconstructs all 400 files byte-for-byte.

No Seattle/Red Rock destructive rule was added.

## L18 — world-geometry stability qualification

L18 closes another false-positive path: a stable source identity could still correspond to reconstructed world geometry that moves or rotates.

For each conservative audit primitive L18 accumulates:

- maximum centroid drift / triangle edge scale,
- maximum receiver-plane offset / triangle edge scale,
- maximum receiver-plane normal-angle change.

Analyzer format is now **v4**.

A record is eligible only when prior L16/L17 requirements pass and geometry remains within:

- centroid drift ≤ **0.10 edge lengths**,
- plane offset ≤ **0.05 edge lengths**,
- plane-angle change ≤ **5°**.

Missing geometry metrics make the record ineligible.

The audit remains observational. These metrics do not feed shader constants, material selection, caster lists, draw lists, or destructive material matching.

`destructiveRulesGenerated` remains false.

---

# 4. NON-NEGOTIABLE SAFETY / CORRECTNESS CONTRACTS

These are established project constraints and must be preserved unless the user explicitly changes direction.

1. **L08 is the last visual/gameplay acceptance point.**
   - L09-L18 source/test success must not be represented as gameplay acceptance.

2. **Do not retune the approved L05 look casually.**
   - `lighting/in-game-L05.shader` must remain byte-for-byte unchanged unless a deliberate new visual tuning pass is approved.

3. **Do not invent source geometry.**
   - In particular, do not manufacture billboard tree/sign casters, fixed billboard azimuths, cylindrical casters, or other geometry unsupported by GT2 source data.

4. **No destructive baked-light rule is currently enabled.**
   - No `bakedGain`.
   - No `removeShadowOverlay`.
   - No course primitive has been approved for destructive unbaking.

5. **Current suns remain provisional.**
   - A provisional sun is insufficient justification for destructive baked-shadow removal.

6. **Future destructive rules must use exact provenance.**
   - Exact track.
   - `primitiveKey`.
   - path-stable `sourceMeshKey`.
   - exact `sourcePrimitiveAddress`.
   - exact `residentContentKey`.

7. **Audit candidates are evidence, not permission.**
   - A fully-qualified analyzer candidate must still receive human/source verification before authoring a destructive rule.

8. **Historical captures must not fabricate live provenance.**
   - Missing source/content identity must remain zero/ineligible rather than being guessed or inherited from stale memory.

9. **Do not substitute guessed runtime data.**
   - No replacement disc.
   - No mismatched SDK.
   - No fabricated prepared-game payload.
   - If exact runtime inputs are missing, say so and stop short of claiming gameplay validation.

---

# 5. CURRENT STATUS / BLOCKER

## Build, game-data recovery, unified flow, and save/reload mechanism: PROVEN

The retained historical L18 Linux runtime has been rebuilt and validated. Both NTSC-U prepared disc bootstraps are restored. Real Seattle/Red Rock Arcade races, Arcade frontend/audio/input, Simulation boot, unified title, Gran Turismo Mode, Arcade Mode, Replay Theater, Option, and the real shipping opening -> Simulation bootstrap -> unified title -> Arcade handoff have all executed on Linux.

Lighting/unbake remains non-destructive: analyzer v4 produced **0 candidate groups / 0 destructive candidates**. Preserve the exact L05 shader and keep destructive unbake disabled.

### Native Arcade Save Game path is mapped through completion

The CRC-valid diagnostic existing-save seed remains a QA-only precondition:

- `BASCUS-94455GAME`, 4 blocks,
- seed SHA-256 `f747c31342346f76cfc242bc930c2adf867c48746f8607437d7bdc74459f8ba5`,
- seed GT2 CRC `0x8D5C9132`.

The retimed native Arcade Save Game path is proven:

- CROSS 880 opens top-level selector,
- DOWN 940 / 1000 / 1060,
- CROSS 1120 -> Save Game selection 3,
- LEFT 1440 + CROSS 1500 -> existing-save classification event 6,
- LEFT 1740 + CROSS 1840 -> overwrite-confirmation event 7,
- state 7 `0x800247D0`,
- state 7 completion event 8,
- state 8 `0x80024930`,
- CROSS 2500 -> event 10 / clean exit.

### Confirmed L18 persistence gap

Tracing exact RecompOne card entry points during the native state-7/event-8 path showed **none** of the persistent card APIs are reached:

- no BIOS A card-file open/write,
- no BIOS B `CardWrite`,
- no `MemoryCard.FrameWrite`,
- no card-A `MemoryCard.Flush`.

GT2's low-level async layer reports completion while the backing card remains unchanged. RecompOne L18 has no modeled persistent bridge for this low-level SIO-style path.

### End-to-end persistence is now proven with an opt-in semantic bridge candidate

An HLE compatibility candidate was developed around GT2's CRC-valid outgoing save payload:

1. snapshot the CRC-valid outgoing payload at async submission,
2. let GT2's native async state machine run unchanged,
3. commit the captured payload to the existing RecompOne `MemoryCard` only after GT2 reports async success.

A one-byte **QA-only** Arcade progress mutation changed live course-result byte `0x801C93F8` from `0x00` to `0x04`. Because that deliberate test mutation occurs later than GT2's normal serializer timing, a QA-only refresh called GT2's own serializer `0x8006A124` immediately before submission.

Refresh proof:

- before refresh: working `0x04`, serialized payload `0x00`, CRC field `0x00000000`,
- after GT2 serializer: working `0x04`, payload offset `0x2B8 = 0x04`, CRC `0xB9CA1C8C`.

Corrected snapshot/commit bridge proof:

- captured outgoing CRC `0xB9CA1C8C`,
- native state 7 completed successfully,
- committed the **captured** CRC-valid payload after success,
- `inotifywait` observed a real card-A `MODIFY/CLOSE_WRITE`,
- saved card SHA-256 changed to `2502605c07a8349c5397431c52aec652f1261a602fad611cac506abd56d30fa2`.

Card validation:

- `BASCUS-94455GAME`,
- chain `1,2,3,4`,
- logical save length `0x8000`,
- stored CRC `0xB9CA1C8C`,
- computed CRC `0xB9CA1C8C`,
- payload offset `0x2B8 = 0x04`.

### Clean reload is proven

A completely new process booted the newly saved card with **all save mutation / bridge hooks disabled**:

- no `RECOMPONE_GT2_CREATE_TEST_SAVE`,
- no `RECOMPONE_GT2_ARCADE_TEST_PROGRESS`,
- no `RECOMPONE_GT2_ARCADE_SAVE_BRIDGE`,
- no card-bridge trace dependency.

The clean process reported:

- `arcadeDifficult=1/21`,
- `arcade=[0x04,0x00,...]`,
- card SHA-256 remained `2502605c07a8349c5397431c52aec652f1261a602fad611cac506abd56d30fa2`.

Therefore **meaningful GT2 Arcade save persistence and clean reload are proven at the mechanism level**.

## Main remaining in-container item: productionize the proven Save Game semantic bridge

The op-6/op-7 semantic ambiguity is now resolved for this retained L18 Arcade Save Game flow.

Runtime + static evidence:

- `func_8006A0C4` queues operation **6**.
- Operation 6 is definitively the **read/load/verify** primitive: its completion CRC-checks the card buffer and imports it into working memory.
- `func_8006A064` queues paired operation **7**, consistent with the write-side primitive elsewhere in GT2.
- Complete Save Game traces, including an extended post-close run, show **no op 7 at all**.
- A falsification run moved a legitimate QA Arcade working-state mutation (`0x801C93F8=0x04`) to poll 1, kept the compatibility bridge disabled, and enabled both serializer/op-code traces. The live state remained dirty (`arcade[0]=0x04`), but this Save Game path still:
  - never called serializer `0x8006A124`,
  - issued only op 6 at startup/load and state 7,
  - never issued op 7,
  - never changed the backing card.

Therefore this recomposed Save Game path cannot be productionized by simply waiting for or bridging a native op-7 continuation that never occurs. The previously proven **snapshot-on-submit / commit-after-native-success Save Game compatibility bridge** is justified at the higher Save Game semantic boundary. It preserves GT2's native UI/status state machine while supplying the persistence bridge missing below it.

The remaining production gate is not semantic placement anymore; it is **ordinary-game-state validation without QA mutation/forced-refresh behavior**.

QA-only pieces that must remain non-production:

- `RECOMPONE_GT2_ARCADE_TEST_PROGRESS`,
- forced state-7 payload refresh used only to prove persistence with an artificial direct memory mutation,
- payload/op-code/card tracing.

Production candidate behavior should retain only the minimal snapshot/commit compatibility layer, guarded by native GT2 success and using GT2-produced save bytes/CRC.

## Remaining real-hardware/platform QA

After production bridge placement and a short software regression:

- physical controller mapping/hotplug/rumble,
- hardware-GPU performance/frame pacing,
- real desktop fullscreen/window/resolution behavior,
- real audio-device behavior,
- Windows/D3D11 parity when Windows becomes the target.

---

# 6. EXACT NEXT ACTIONS

1. **Create a production-minimal Save Game bridge candidate.**
   - Keep the proven snapshot-before-op6-clobber / commit-after-native-success semantics.
   - Preserve GT2's UI, state 7/event 8 success/failure flow, and GT2-generated CRC/payload bytes.
   - Remove/disable QA-only direct progress mutation, forced payload refresh, and verbose traces from default behavior.
   - Do not wait for op 7 in this Save Game path; runtime falsification shows it is absent even with dirty live state.

2. **Run an ordinary-game-state save regression.**
   - Produce a legitimate Arcade state change through normal game/UI/gameplay logic **before entering Save Game** rather than direct memory mutation.
   - Verify the normal game path has already prepared changed save bytes before state-7 op6 can clobber them.
   - Save through the production-minimal bridge.
   - Validate card structure, GT2 CRC, and changed state.
   - Clean restart with all QA/test/bridge mutation hooks disabled for read and verify the state reloads from disk.

3. **If ordinary gameplay does not prepare fresh save bytes, isolate that as a separate serializer-compatibility gap.**
   - Do not silently keep the QA forced-refresh hook.
   - Identify the missing normal serializer trigger and either restore that trigger or document a narrowly-scoped production serializer call before snapshot.

4. **Run a short unified-flow soak after the ordinary-state save regression passes.**
   - title -> Gran Turismo -> title -> Arcade -> title,
   - one Simulation path + one Arcade race,
   - one ordinary save/reload,
   - clean exit and no unintended card mutation.

5. **Then move remaining validation to real Linux hardware.**

---

# 7. EXPECTED REMAINING WORK

The original lighting/audit objective is closed unless a new visual defect appears. Meaningful Arcade save/write/reload persistence is proven through the compatibility bridge, and the op-6/op-7 production-placement ambiguity is now resolved: this Save Game path never reaches op 7, even with dirty live state. Remaining container work is to **strip the proof-only hooks, validate the minimal Save Game semantic bridge with an ordinary legitimate game-state change, and run a short unified-flow soak**. If normal gameplay does not prepare fresh save bytes before Save Game, treat serializer triggering as the final separate compatibility gap rather than retaining the QA forced-refresh hook. After that, meaningful validation moves to real Linux hardware.

---

# 8. IMPORTANT FILE / REPORT INDEX

Latest Linux QA evidence:

- `OpenGT_L18_Linux_QA_01_Report.md`
- `OpenGT_L18_Linux_QA_01.zip`
- `OpenGT_L18_Linux_QA_02_Report.md`
- `OpenGT_L18_Linux_QA_02.zip`
- `L18-memory-card-write-trace.patch` — exact-L18 opt-in save-write instrumentation candidate.
- `L18-save-ui-state-trace.patch` — exact-L18 guest save-callback/state instrumentation, SHA-256 `6dbf0fe114ee7635e50a802557a42bc95afad44423da9bdeb9366feb507a8ed8`.
- `OpenGT_L18_Save_UI_State_Trace_Build_Report.md`
- `OpenGT_L18_Save_UI_State_Trace_Build.zip` — patch + managed build log only; no game payload.
- `OpenGT_L18_Save_UI_Trace_Probe_01_Report.md` — traced A/B save attempt proving `0x800503B8` is not reached.
- `OpenGT_L18_Save_UI_Trace_Probe_01.zip` — logs/inotify/card hashes/result only; no card images or game payload.
- `L18-save-menu-selection-trace.patch` — distinct Simulation UI-record selection trace, SHA-256 `e79ffad6484ce0a5fd9837263574295c5493a2723ae646c204e8958fc06c7188`.
- `OpenGT_L18_Save_Menu_Selection_Trace_01_Report.md` — record-index mapping + corrected-fixture diagnosis.
- `OpenGT_L18_Save_Menu_Selection_Trace_01.zip` — managed-build log, trace logs, inotify/card hashes, and patch only; no card images/game payload.
- `L18-arcade-save-menu-trace.patch` — opt-in Arcade main-menu selector trace, SHA-256 `9450e122f909408ddbac34ef78a4c24d5e6d7f5ffa9b1c3eab6c4ed9236a4880`.
- `OpenGT_L18_Arcade_Save_Menu_Trace_Build_01_Report.md` — native Arcade seed-route diagnosis + trace build.
- `OpenGT_L18_Arcade_Save_Menu_Trace_Build_01.zip` — patch/build log/report only; no game payload or card image.
- `OpenGT_L18_Save_Persistence_Probe_01_Report.md` — OS-level card write/no-write diagnosis.
- `OpenGT_L18_Save_Persistence_Probe_01.zip` — logs/inotify/card hashes only; no card images or game payload.
- `L18-arcade-save-event-trace.patch` — exact-L18 `func_80025258` dispatcher/event trace, SHA-256 `c8e1a331205861b067d534f0ed23e53693fc8d82118694a80a103076feb9604f`.
- `OpenGT_L18_Arcade_Save_Event_Probe_01_Report.md` — internal Save Game state-machine/event mapping.
- `OpenGT_L18_Arcade_Save_Event_Probe_01.zip` — logs/inotify/card hashes/state table/build log/patch only; no card images or game payload.


Primary reports known from the project:

- `OpenGT_Lighting_L08_Report.md`
- `OpenGT_Lighting_L09_Candidate_v2_Report.md`
- `OpenGT_Lighting_L10_Candidate_Report.md`
- `OpenGT_Lighting_L11_Candidate_Report.md`
- `OpenGT_Lighting_L12_Candidate_Report.md`
- L13 checkpoint: resident immutable-definition lookup-key hardening
- `OpenGT_Lighting_L14_Candidate_Report.md`
- `OpenGT_Lighting_L15_Candidate_Report.md`
- `OpenGT_Lighting_L16_Candidate_Report.md`
- `OpenGT_Lighting_L17_Candidate_Report.md`
- `OpenGT_Lighting_L18_Candidate_Report.md`

Important source/runtime files:

- `lighting/in-game-L05.shader`
- `tools/build_linux_lighting.py`
- `tools/analyze_unbake_audit.py`
- `native/include/opengt/lighting.hpp`
- `native/src/lighting.cpp`
- `native/tests/lighting_tests.cpp`

Important validation artifact named by L18:

- `validation/sanitizer-source-equivalence.json`
- `validation/reconstruction.json`
- `L18-from-L17.review.patch`

---


Latest persistence/write-state evidence:

- `OpenGT_L18_Arcade_Native_Write_State_Probe_01_Report.md` — seeded native overwrite path through state 7/event 8 with no backing-file write.
- `OpenGT_L18_Arcade_Native_Write_State_Probe_01.zip` — logs/inotify/hash/result only; no card image or game payload.

Latest persistence-closure evidence:

- `OpenGT_L18_Arcade_Persistence_Closure_01_Report.md` — meaningful Arcade save mutation, CRC validation, physical backing-card write, and clean reload proof.
- `OpenGT_L18_Arcade_Persistence_Closure_01.zip` — logs/patches/hashes/summary only; no memory-card image or game payload.
- `L18-gt2-arcade-save-snapshot-bridge.patch` — snapshot-on-submit / commit-on-success bridge delta, SHA-256 `eedc96ffc5fa33fbe8a46b659877c4d05579de2590f87c6feabc87b68e3d007b`.
- `L18-arcade-save-payload-refresh-test.patch` — QA-only GT2 serializer refresh, SHA-256 `15cf6dcd4173a17ccaad51b7ee94e67554f4d828e9c21cf4b385be5f3cc13450`.
- `L18-arcade-save-refresh-byte-trace.patch` — diagnostic before/after serializer trace, SHA-256 `c2b4cff0d117ef404086b99b43bb318f211a5518ecd8f371e54ef46e4a9c9813`.

- `OpenGT_L18_Arcade_Op7_Falsification_01_Report.md` — dirty-live-state/no-bridge test proving this Save Game flow still does not serialize or invoke op 7.
- `OpenGT_L18_Arcade_Op7_Falsification_01.zip` — logs/inotify/result/diagnostic patches only; no card image or game payload.

# 9. KNOWN ARTIFACT HASHES

| Artifact | SHA-256 |
|---|---|
| `lighting/in-game-L05.shader` | `29e55120508bb0540bf29ab633b4177c0f77bdb546c6b89bacfa390bcbd4cf6e` |
| `OpenGT_Lighting_L09_Billboard_Shadow_Candidate_v2.zip` | `2ec877bc522dcac1887931a310ac8685882ef23e69db82d1ab30117fb954fe68` |
| `OpenGT_Lighting_L10_Billboard_Lighting_Candidate.zip` | `065c36b7da768795553434b7a655819ac13a32d28e44769c9a5faea0facff709` |
| `OpenGT_Lighting_L14_Exact_Unbake_Audit_Candidate.zip` | `cf6adea0d53a95c9fa3d31a4a2b25eff583aae64330b4f78836f3a8663c6256a` |
| `OpenGT_Lighting_L17_Content_Bound_Unbake_Rules_Candidate.zip` | `4d7e515d51a3768ef91edd48d1db39361e93c652fb558fbc81ddcf33f8eec24e` |
| `OpenGT_Lighting_L18_World_Geometry_Unbake_Audit_Candidate.zip` | `4ebbd8391e445b792f21dfffa9b23bb15a75d83054edefc0761349e96cbfd0d3` |

| `OpenGT_L18_Save_Persistence_Probe_01_Report.md` | `e6097a3f299e6e6b39e27e713a1735834f95ed3676175a3efe0e0996aec9983c` |
| `OpenGT_L18_Save_Persistence_Probe_01.zip` | `0307f843604bcb50f031e61de66fbe1278b392a80e1737e11fd6ad4ffc43d05b` |

| `OpenGT_L18_Save_UI_Trace_Probe_01_Report.md` | `168fce9567a61d2dab9c508df05b15dffa4e31b75043412cb1a64cfb828796f4` |
| `OpenGT_L18_Save_UI_Trace_Probe_01.zip` | `9ec81a94b39623e32765c14d6746a3e6e23278f63a368943b775722c2fc7ec93` |

| `L18-save-menu-selection-trace.patch` | `e79ffad6484ce0a5fd9837263574295c5493a2723ae646c204e8958fc06c7188` |
| menu-selection-trace `GranTurismo2PC.dll` | `0b76056dfbf0edb3c3f7556c304fb2388747532214ab251c75da49c4c98e6b50` |
| `OpenGT_L18_Save_Menu_Selection_Trace_01_Report.md` | `d4bc903cf4502cb07e171f4cdf31c3c57cd6c57860ed70ea7f9f7c854da185a5` |
| `OpenGT_L18_Save_Menu_Selection_Trace_01.zip` | `5ce89c0c7e2d396bb7dec6522339602721f0686d974c9eddfb539253a4cea57f` |
| `L18-arcade-save-menu-trace.patch` | `9450e122f909408ddbac34ef78a4c24d5e6d7f5ffa9b1c3eab6c4ed9236a4880` |
| Arcade-save-menu-trace `GranTurismo2PC.dll` | `77b5ba91ea171d5ec5da7faad4d27fb0e4a1a96bb53b243043d0968efffce529` |
| `OpenGT_L18_Arcade_Save_Menu_Trace_Build_01_Report.md` | `6737f194c3de3de7df4dc0660cfb080f11fa4f9ca0bc7adeec6eb02928451f2d` |
| `OpenGT_L18_Arcade_Save_Menu_Trace_Build_01.zip` | `f3a2b0cdab60b77c42fdb7c5b17e95e59a37762bd1423b492ff94a22a946680e` |

| `L18-arcade-common-selector-trace.patch` | `b6fc4b967de3b9fd42aad8e2cc9a59be3d684d4f903c6498ce1054c78e782dc1` |
| common-selector-trace `GranTurismo2PC.dll` | `286308a08564515279b798f8e5eb1d37457ba2d226ecaf775c0b0393a8fe686c` |
| `OpenGT_L18_Arcade_Common_Selector_Probe_01_Report.md` | `38ea388a14c6d952287645b0c645a0520331c8a816d4ea6d0bc843879f24525e` |
| `OpenGT_L18_Arcade_Common_Selector_Probe_01.zip` | `259a681cd50dbe235d490390153262c2b3bb1aba374719f1a6d32553f2cbb6d7` |

| `L18-arcade-save-state-trace.patch` | `5d0d3dd590ede8280019480becc9dcb59c2e6f2cf72af10848e21c943742c934` |
| save-state-trace `GranTurismo2PC.dll` | `e9b8dc86c3605db2878fcca064fe45cebaaa86dab91f70ad2e7ad5d7f2541f09` |
| `OpenGT_L18_Arcade_Save_State_Probe_01_Report.md` | `69e8585ac6b71503c1b22be6138800f657892b93153215ce80ced97ef0996670` |
| `OpenGT_L18_Arcade_Save_State_Probe_01.zip` | `fa344e3b908b111020bb67a8b1c9c1702d227540220f62d54d9387d4ceb7b1ed` |

| `L18-arcade-save-event-trace.patch` | `c8e1a331205861b067d534f0ed23e53693fc8d82118694a80a103076feb9604f` |
| save-event-trace `GranTurismo2PC.dll` | `9b289439e30dc71bdac8eed4a4759d0104d6af211cb704830ee6376db04cd128` |
| `OpenGT_L18_Arcade_Save_Event_Probe_01_Report.md` | `939f97dcac4e759633a0b87395fcccbc5764c96d1d34d5567177173886d923aa` |
| `OpenGT_L18_Arcade_Save_Event_Probe_01.zip` | `f8488a3bd162c3e56c225992bcae3bab1a6d4ae2e4bc908f52f67e1268ce4319` |

| diagnostic CRC-valid seed card (not distributed in evidence ZIP) | `f747c31342346f76cfc242bc930c2adf867c48746f8607437d7bdc74459f8ba5` |
| `OpenGT_L18_Arcade_Native_Write_State_Probe_01_Report.md` | `1c4ba3780e3996661a870b879060a755f72aac5f7294c544ec5feaa676c5c4d3` |
| `OpenGT_L18_Arcade_Native_Write_State_Probe_01.zip` | `6835de84e07b5c77f56162d6f4ba62a5ec456d4fc70171dfe485fbd9f07686c9` |

| `L18-gt2-arcade-save-snapshot-bridge.patch` | `eedc96ffc5fa33fbe8a46b659877c4d05579de2590f87c6feabc87b68e3d007b` |
| `L18-arcade-save-payload-refresh-test.patch` | `15cf6dcd4173a17ccaad51b7ee94e67554f4d828e9c21cf4b385be5f3cc13450` |
| `L18-arcade-save-refresh-byte-trace.patch` | `c2b4cff0d117ef404086b99b43bb318f211a5518ecd8f371e54ef46e4a9c9813` |
| snapshot-bridge proof `GranTurismo2PC.dll` | `40a5e9694264b17e84f2e29f60593545ef1dcca33dfe03f269838665344228a0` |
| snapshot-bridge proof `RecompOne.Runtime.dll` | `6d7e6aab9ded800b8943d4613666ec409d3137acb49f06d914774efee4712e94` |
| meaningful saved diagnostic card (not distributed) | `2502605c07a8349c5397431c52aec652f1261a602fad611cac506abd56d30fa2` |
| `OpenGT_L18_Arcade_Persistence_Closure_01_Report.md` | `5080bad13c244ff8b5f432faeff52eac5d3c6cb9fa6fc0e23be3b22debd27aa9` |
| `OpenGT_L18_Arcade_Persistence_Closure_01.zip` | `49337c2a77cfc03e91e3ac27ee4ccf7f3f3a33dd5492982912b617f02ba2e184` |

| `OpenGT_L18_Arcade_Op7_Falsification_01_Report.md` | `df29919125b3531937e8eec4d70c69171735c6635b7a19c7315a31a1ba13a77e` |
| `OpenGT_L18_Arcade_Op7_Falsification_01.zip` | `d235b217f5c94d73516f8eaff43da7788c4fdcf4a662066e5fcee9c0a96b062d` |
| early-progress-test `GranTurismo2PC.dll` | `2933331178fe151ff643c68397f5b180130fb35478d0805fe616081ddb89b14b` |
| early-progress-test `RecompOne.Runtime.dll` | `cf540c61860d7c1e84e692cf35dff1dc1d5d7d155a4da5fea5bdd4e2f8bd0230` |

**Note:** Preserve hashes exactly as recorded. If any future artifact is repacked, give it a new filename/hash rather than silently replacing an old identity.

---

# 10. DO NOT CLAIM

Do not claim:

- exact L08 visual parity for L18,
- any evidence-based destructive unbake rule,
- that operation 6 is definitively GT2's canonical physical-write primitive,
- that the current opt-in operation-6 snapshot bridge is already the finalized/default production fix,
- that QA-only progress mutation or payload-refresh hooks belong in release gameplay,
- hardware-GPU performance from Xvfb/software-rendered timings,
- physical-controller validation,
- Windows/D3D11 parity.

It **is** now valid to state that:

- L18 executes real Seattle/Red Rock gameplay and produces real native backbuffers,
- analyzer v4 has zero destructive candidates,
- Arcade and Simulation/unified Linux QA are broadly green,
- the native Arcade Save Game UI/state path is mapped through async completion,
- RecompOne's native L18 low-level path does not persist that transaction to backing storage on its own,
- an opt-in snapshot/commit compatibility bridge has physically written a **meaningfully changed**, GT2-CRC-valid four-block save,
- a clean new process with all save/test bridge flags disabled reloaded the persisted `arcade[0]=0x04` state from disk.

---

# 11. TURN LOG

## September 23, 2026 — Canonical handoff created

A dedicated canonical handoff was created from the prior PS1 Gran Turismo/OpenGT project state.

Authoritative transfer state:

- L18 is the latest cumulative source-only checkpoint.
- L08 remains the last actual gameplay-validated checkpoint.
- The L05 approved look remains unchanged.
- Source-only audit hardening is considered complete enough to stop extending without real runtime data.
- The next meaningful task is restoration of the exact GT2/.NET/offline-build inputs, followed by fresh Seattle/Red Rock L18 execution and analyzer-v4 evidence collection.
- No destructive baked-light rule is enabled.
- Future chats must update and return this handoff at the end of every work turn.

---


## September 24, 2026 — Persistence diagnosis: exact L18 card writes are synchronous

- Restored exact L18 source archive and inspected `Hardware/MemoryCard.cs`, `Bios/BiosA.cs`, and `Bios/BiosB.cs`.
- Proved BIOS A card-file writes flush immediately after copying the requested byte range.
- Proved BIOS B 128-byte frame writes flush immediately through `MemoryCard.FrameWrite()`.
- Proved card-file creation also flushes immediately.
- Conclusion: previous unchanged blank-card hashes mean the scripted native Save Game flow never reached an actual card write; there is no evidence of a delayed flush problem.
- Prepared exact-L18 opt-in instrumentation patch `L18-memory-card-write-trace.patch`, SHA-256 `e0a284b7cb704653f33eb9ed1c03bbe70e4cc02c6c6d53df9b84f0c98d7f7501`, gated by `RECOMPONE_TRACE_CARD_WRITES=1`.
- Patch logs card create requests/completion, BIOS A file-write ranges, and BIOS B raw frame writes.
- Patch has **not yet been rebuilt/tested**; that is the next short turn.

---

## September 24, 2026 — Save persistence probe 01: no GT2 card write reached

- Kept the turn bounded because prior long turns were timing out.
- Exact L18 source inspection from the previous turn remains authoritative: `MemoryCard.Create()`, BIOS A file writes, and BIOS B `FrameWrite()` all flush synchronously.
- SDK/offline-feed artifacts were not mounted in this reset, so the prepared C# trace patch was not rebuilt. Instead, the unchanged validated L18 runner was monitored at the OS/filesystem level with `inotifywait`.
- Reconstructed only the minimum Simulation runtime needed for the isolated persistence probe.
- Ran Gran Turismo Mode with `RECOMPONE_GT2_CREATE_TEST_SAVE=1`; live save telemetry again showed 100,000 credits and all 60 licenses bronze.
- Monitored the isolated card directory for `create`, `open`, `modify`, `close_write`, and `moved_to` events through input poll 2450.
- The only write events were the two initial blank-card formatting writes at startup. **No later `MODIFY` or `CLOSE_WRITE` occurred on either card.**
- Final `carda.sav` and `cardb.sav` hashes remained the known blank-card identity `78b6d4ac9ab4d23caf7e5f04f83539bf5d994cccfb0a709d14ac53d05c8e21ef`.
- Conclusion is now stronger: the scripted Save Game sequence did not reach any persistent GT2 card write. This further exonerates delayed flush/backend behavior for that path; the next work is native Save Game UI state/confirmation instrumentation.
- Created `OpenGT_L18_Save_Persistence_Probe_01_Report.md` (`e6097a3f299e6e6b39e27e713a1735834f95ed3676175a3efe0e0996aec9983c`) and no-game-payload evidence ZIP `OpenGT_L18_Save_Persistence_Probe_01.zip` (`0307f843604bcb50f031e61de66fbe1278b392a80e1737e11fd6ad4ffc43d05b`).


## September 24, 2026 — Save UI native boundary mapped and trace host rebuilt

- Kept the turn bounded to static save-flow mapping + managed trace build.
- Exact L18 generated Simulation code identifies `func_800503B8` (guest `0x800503B8`) as the native save-start callback.
- `0x800503B8` immediately calls `func_80072F9C`, which constructs the full 0x8000-byte GT2 save image through `func_8006A214`, including CRC at offset `0x7E9C`, then sets the save-controller state to **4**.
- Save-controller object pointer is stored at guest address `0x801C94AC`.
- `func_80071B30` is the central save/card state dispatcher. Its six-state table at `0x8008FAD0` maps state 4 -> `0x80071BBC` and state 5 -> `0x80071C64`.
- Created opt-in diagnostic patch `L18-save-ui-state-trace.patch`, gated by `RECOMPONE_TRACE_GT2_SAVE_UI=1`. It logs entry to `0x800503B8`, payload/state-4 creation, and save-controller state changes.
- Recovered exact SDK 10.0.401, offline NuGet feed, and historical fonts; rebuilt only the managed unified host against the already-validated L18 native `.so`.
- Managed restore/build PASS: **0 errors**, 467 existing warnings, elapsed 1:03.29.
- Instrumented `GranTurismo2PC.dll` SHA-256: `dbf89d64ec647a11d45c344dfdb0fd725c4209f04a70724eb03778c37b181ea5`.
- `L18-save-ui-state-trace.patch` SHA-256: `6dbf0fe114ee7635e50a802557a42bc95afad44423da9bdeb9366feb507a8ed8`.
- Exact L05 shader remains unchanged at `29e55120508bb0540bf29ab633b4177c0f77bdb546c6b89bacfa390bcbd4cf6e`.
- No traced gameplay/save attempt was run in this short turn; next turn is a single isolated run using the rebuilt host and the new guest-state trace.
- Created `OpenGT_L18_Save_UI_State_Trace_Build_Report.md` (`63c326eb81970cbca001a804324bbf37cf8c460fb143e1af18f27d3ae65e5faa`) and `OpenGT_L18_Save_UI_State_Trace_Build.zip` (`aab96052add6b5fab7f07e03cb13f4aeb4ca9545ff14210dea91a3f88bbe2528`).

## September 24, 2026 — Save UI trace probe 01: native callback not reached

- Ran exactly one isolated Simulation save attempt using instrumented exact-L18 `GranTurismo2PC.dll` SHA-256 `dbf89d64ec647a11d45c344dfdb0fd725c4209f04a70724eb03778c37b181ea5`.
- Used the exact same absolute-poll Save Game script and isolated-card setup as persistence probe 01, plus `RECOMPONE_TRACE_GT2_SAVE_UI=1` and OS-level `inotifywait`.
- Process exited cleanly with status 0.
- No `0x800503B8` save-start callback marker appeared.
- No `0x80072F9C` payload-builder/state-4 marker appeared.
- No post-format card `MODIFY`/`CLOSE_WRITE` occurred; both cards remained blank hash `78b6d4ac9ab4d23caf7e5f04f83539bf5d994cccfb0a709d14ac53d05c8e21ef`.
- Conclusion: the scripted flow never reaches native Save Game confirmation; card backend/state-4 code is not entered.
- Static decompressed-overlay analysis found `0x800503B8` in the Simulation UI table. Overlay-2 records at raw offsets `0x40CA4` and `0x41264` identify the relevant record as callback trio `0x800503B8 / 0x80050734 / 0x800508C4`, flags `0x10`, group `4`, item `3`.
- Next task is to trace/activate that exact group-4/item-3 UI state, not to change card persistence code.
- Created `OpenGT_L18_Save_UI_Trace_Probe_01_Report.md` (`168fce9567a61d2dab9c508df05b15dffa4e31b75043412cb1a64cfb828796f4`) and evidence ZIP `OpenGT_L18_Save_UI_Trace_Probe_01.zip` (`9ec81a94b39623e32765c14d6746a3e6e23278f63a368943b775722c2fc7ec93`).


## September 24, 2026 — Save menu selection trace 01: packaged-save precondition identified

- Static Simulation UI analysis mapped the active record selection more precisely. `func_800228D8` selects one of five UI-table pairs from mode `0x800F3954`; `func_800229FC` / `func_800229CC` select a 0x20-byte record by the UI object's `+0x0E` index.
- The native Save Game item is physical **record index 7** in both relevant primary table copies (`0x80050BC4` and `0x80051184`), corresponding to group 4 / item 3 and primary callback `0x800503B8`.
- Added opt-in `RECOMPONE_TRACE_GT2_SAVE_MENU=1`, logging only distinct `(mode, recordIndex, group, item, primaryCallback)` tuples after record resolution.
- Incremental managed rebuild PASS: **0 errors**, 454 existing unreachable-code warnings. Instrumented DLL SHA-256 `0b76056dfbf0edb3c3f7556c304fb2388747532214ab251c75da49c4c98e6b50`.
- The old scripted save route was recognized as the repository's proven packaged-save path, shifted earlier. A timing hypothesis was tested by restoring the proven title timing so the poll-820 Home-entry CROSS occurred after `RECOMPONE_GT2_CREATE_TEST_SAVE` became active.
- Test-save activation was logged **before** the poll-820 CROSS, disproving “hook activated too late” as the cause.
- Despite corrected timing, the run still loaded only `main`, `gt2_overlay_1`, and `gt2_overlay_4`; it never loaded `gt2_overlay_2`, so no save-record trace or `0x800503B8` callback could occur. Cards remained blank.
- Repository fixture comments explain the mismatch: the proven save-navigation fixture assumes a pre-existing packaged convenience save that auto-loads a purchased/upgraded Mazda and starts on Home/overview-map state. The L18 test-save hook patches only live credits/licenses; it does not recreate persistent save/home/garage state.
- Therefore further blind button timing is not useful. Next: create an isolated valid GT2 seed card establishing the same Home-state precondition, then rerun the proven Save Game route with record-index and save-callback tracing. The seed is diagnostic only; final write/reload proof must still come from GT2's native save path.
- Created `L18-save-menu-selection-trace.patch` (`e79ffad6484ce0a5fd9837263574295c5493a2723ae646c204e8958fc06c7188`), report `OpenGT_L18_Save_Menu_Selection_Trace_01_Report.md` (`d4bc903cf4502cb07e171f4cdf31c3c57cd6c57860ed70ea7f9f7c854da185a5`), and no-game-payload evidence ZIP `OpenGT_L18_Save_Menu_Selection_Trace_01.zip` (`5ce89c0c7e2d396bb7dec6522339602721f0686d974c9eddfb539253a4cea57f`).

## September 24, 2026 — Native Arcade seed route repaired; Arcade Save Game selector mapped

- Revisited the project-supported `create_arcade_audit_card.ps1` strategy instead of synthesizing a GT2 seed card. The intended pipeline is: let GT2 create a native base save through Arcade's own Save Game UI, then patch that already-valid save for diagnostic progression.
- Found the cached Arcade runtime root was incomplete from an earlier bounded extraction: `DISC_META.DAT` was absent from its manifest and cached `STREAM.DAT` was truncated at 318,767,104 bytes.
- Re-extracted the user's authoritative `arcade.7z` fully: `DISC_META.DAT` 18,688 bytes, `STREAM.DAT` 382,122,880 bytes, `SYSTEM.CNF` 68 bytes. Rebuilt the Arcade manifest using `Raw2336` encoding for `DISC_META.DAT` and `STREAM.DAT`; ISO metadata resolves `STREAM.DAT` to 335,011,840 logical bytes.
- Reran the native Arcade seed-card fixture with the Linux-proven opening-skip pulses followed by the repository's original Save Game sequence. The run reached overlays 5 -> 1 -> 2, activated the 100,000-credit/60-license test state, consumed every fixture input through poll 2500, exited 0, but both cards still remained the blank-card SHA-256.
- This proves the reduced-root issue is fixed but the current L18 Linux input timing still does not reach the actual Arcade save write.
- Static exact-L18 Arcade overlay analysis mapped the frontend selector: `func_8001E094` calls `func_8001B84C`; returned `S1` is the menu selection. Table `0x80027140` maps selection values 0-3 into the menu-action branch. Repository fixture order is `Start Game -> Replay Theater -> Options -> Save Game`, so **selection 3 is Save Game**.
- Added opt-in `RECOMPONE_TRACE_GT2_ARCADE_SAVE_MENU=1`, logging only distinct `[GT2-ArcadeSaveMenu] selection=<n>` transitions. No game/card/save logic changed.
- Incremental exact-L18 managed rebuild PASS: 454 existing warnings, **0 errors**. Instrumented DLL SHA-256 `77b5ba91ea171d5ec5da7faad4d27fb0e4a1a96bb53b243043d0968efffce529`.
- Patch SHA-256 `9450e122f909408ddbac34ef78a4c24d5e6d7f5ffa9b1c3eab6c4ed9236a4880`.
- Created `OpenGT_L18_Arcade_Save_Menu_Trace_Build_01_Report.md` (`6737f194c3de3de7df4dc0660cfb080f11fa4f9ca0bc7adeec6eb02928451f2d`) and `OpenGT_L18_Arcade_Save_Menu_Trace_Build_01.zip` (`f3a2b0cdab60b77c42fdb7c5b17e95e59a37762bd1423b492ff94a22a946680e`).
- No traced runtime probe with the new Arcade selector build was run in this short turn. Next turn is exactly one native seed-card attempt with `RECOMPONE_TRACE_GT2_ARCADE_SAVE_MENU=1`.

## September 24, 2026 — Arcade save selector probe 01: traced dispatcher not entered

- Ran exactly one native Arcade seed-card attempt using the exact instrumented L18 host with `RECOMPONE_TRACE_GT2_ARCADE_SAVE_MENU=1`.
- Instrumented DLL SHA-256 remained `77b5ba91ea171d5ec5da7faad4d27fb0e4a1a96bb53b243043d0968efffce529`.
- Complete authoritative Arcade prepared root was used; process exited cleanly with status 0.
- Runtime reached `main -> gt2_arcade_overlay_5 -> gt2_arcade_overlay_1 -> gt2_arcade_overlay_2`.
- The 100,000-credit / all-60-license test state activated successfully.
- **No `[GT2-ArcadeSaveMenu] selection=...` marker appeared at all.** Therefore the specifically instrumented handler `func_8001E094` was never entered; this is stronger than merely failing to reach selection 3.
- Both isolated card files remained the blank-card SHA-256 `78b6d4ac9ab4d23caf7e5f04f83539bf5d994cccfb0a709d14ac53d05c8e21ef`. No native save write occurred.
- Static exact-L18 analysis shows common selector routine `func_8001B84C` is called by eight overlay-2 handlers: `0x8001D6C8`, `0x8001D8F0`, `0x8001DB74`, `0x8001DE14`, `0x8001E094`, `0x8001E330`, `0x80020D88`, and `0x8002416C`.
- Conclusion: current fixture remains in an earlier/different overlay-2 UI substate. Next short step is to instrument **inside `func_8001B84C`** and log only distinct `(caller RA, returned selection)` pairs, identifying the active handler deterministically before changing any input timing.
- No memory-card backend change is warranted; synchronous flush behavior remains proven and GT2 still has not issued a card write.
- Created `OpenGT_L18_Arcade_Save_Selector_Probe_01_Report.md` (`374caa3f71418f5c735df2909dcf2e2b09f95c8066a4c2f2ec95fe0584d433b5`) and no-game-payload evidence ZIP `OpenGT_L18_Arcade_Save_Selector_Probe_01.zip` (`59fb63c8519a69a0c21527fa7e13ec6c1bd666cd29513c2c8688f246090d0948`).

## September 24, 2026 — Arcade common selector probe 01: Save Game top-level selection proven

- Added opt-in exact-L18 trace inside common Arcade selector `func_8001B84C`, logging only distinct `(caller RA, returned result)` pairs under `RECOMPONE_TRACE_GT2_ARCADE_COMMON_SELECTOR=1`.
- Patch is clean/generated-source-only: `L18-arcade-common-selector-trace.patch`, SHA-256 `b6fc4b967de3b9fd42aad8e2cc9a59be3d684d4f903c6498ce1054c78e782dc1`.
- Incremental managed rebuild PASS: 454 existing warnings, **0 errors**. Instrumented DLL SHA-256 `286308a08564515279b798f8e5eb1d37457ba2d226ecaf775c0b0393a8fe686c`.
- Ran the same complete-root native Arcade seed-card fixture once. Process exited 0; cards remained blank.
- Common selector emitted exactly three distinct pairs, all from caller return address `0x8001D734`: results `-2`, `-3`, then `3`.
- `0x8001D734` identifies handler `func_8001D6C8`. Static exact-L18 analysis shows its four-entry action table begins at `0x8004F8DC`; selection 3 chooses descriptor `0x800523DC`.
- This corrects the previous interpretation: **the fixture does reach the fourth top-level Arcade menu action, i.e. Save Game**. The prior `func_8001E094` trace point was simply on the wrong overlay-2 handler.
- Descriptor `0x800523DC` contains callback trio `0x80023478 / 0x800234C8 / 0x80023550` and is pushed onto the UI state stack by `func_800157A4`.
- Therefore further menu retiming at the top level is unnecessary. The next exact task is to instrument those three Save Game state callbacks and run the same fixture once, identifying which create/slot/confirmation state is reached after Save Game selection 3.
- No memory-card backend change is warranted; no native card write occurred in this probe.
- Created report `OpenGT_L18_Arcade_Common_Selector_Probe_01_Report.md` (`38ea388a14c6d952287645b0c645a0520331c8a816d4ea6d0bc843879f24525e`) and no-game-payload evidence ZIP `OpenGT_L18_Arcade_Common_Selector_Probe_01.zip` (`259a681cd50dbe235d490390153262c2b3bb1aba374719f1a6d32553f2cbb6d7`).

## September 24, 2026 — Arcade Save Game state probe 01: state entered, internal transition stalls at 0

- Instrumented only Save Game descriptor `0x800523DC` callbacks under `RECOMPONE_TRACE_GT2_ARCADE_SAVE_STATE=1`: init `0x80023478`, update `0x800234C8`, draw `0x80023550`.
- Exact-L18 managed rebuild PASS with 467 existing generated-code warnings and **0 errors**. Instrumented DLL SHA-256 `e9b8dc86c3605db2878fcca064fe45cebaaa86dab91f70ad2e7ad5d7f2541f09`.
- Reconstructed the complete Arcade + Simulation runtime root after scratch reset and reran the same native Arcade seed-card fixture once with isolated cards and `inotifywait`.
