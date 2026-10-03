# OpenGT L18 — Arcade Production-Minimal Save Bridge Candidate 01

Date: 2026-09-24
Repository: GTTeancum/OpenGTPS1
Branch: chatgpt/l18-runtime-recovery

## Result

A production-minimal, opt-in Arcade Save Game persistence bridge is now durable in source. It retains the already-proven semantic placement: snapshot GT2's CRC-valid outgoing payload immediately before state-7 operation 6 can reuse/clobber the guest buffer, then persist that captured payload through RecompOne MemoryCard only after GT2's existing native success branch.

The production candidate does **not** force `func_8006A124`, does not perform the QA direct progress mutation, and does not enable verbose proof traces by default. It remains gated by `RECOMPONE_GT2_ARCADE_SAVE_BRIDGE=1` until an ordinary legitimate gameplay-state save/reload regression is executed.

## Durable source changes

- `vendor/RecompOne/RecompOne.Runtime/sdk/GT2Compat.cs`
  - CRC-guarded `CaptureArcadeSaveWrite(...)`
  - single-pending-slot semantics
  - `CompleteArcadeSaveWrite()` commits through `MemoryCard.WriteByte` + `Flush()` only after native success
  - environment gate: `RECOMPONE_GT2_ARCADE_SAVE_BRIDGE=1`
- `tools/apply_gt2_arcade_enhancements.py`
  - patches only `func_800247D0`
  - capture immediately before `func_8006A0C4`
  - commit immediately before the existing success cleanup `func_8006A1C4`
  - no forced serializer call
- `tools/modern-renderer-config-tests/Program.cs`
  - generation/source contract proves capture/submit/complete/cleanup ordering and forbids a forced `func_8006A124` call in the production patcher
- `tools/arcade-save-bridge-tests/ArcadeSaveBridgeTests.csproj`
- `tools/arcade-save-bridge-tests/Program.cs`
  - no-game-data regression using a minimal `IMemory` fixture and real RecompOne `MemoryCard`

## Git commits

- `a94d3b85418a8e536f02a4af344cefc1b300c099` — runtime snapshot/commit bridge
- `f5e9d4daf5b0725e627bca95dc0ff1a85a0605a4` — durable Arcade generated-source patcher
- `76e5968d699751ae3483189264baa3d3b72a6a37` — generation/source contract
- `24d6bcf3ce512762f7308e232af325f1b3d282ad` — persistence regression project
- `495af7cae8fe32d172be2c51b1026660d8d470ff` — initial persistence regression
- `10fe9f40c5f16e231f194fc4f0bf0b2b79f51af4` — isolate test from full GPU initialization; this exact source/test commit received GitHub status `chatgpt/l18-save-bridge = success`
- `86440addee3456a5803077317be15a43a6622944` — final branch head for this turn; removes temporary validation workflow only

## Validation executed

1. Retained L18 source identity was recovered from `OpenGT_Lighting_L18_World_Geometry_Unbake_Audit_Candidate.zip`.
   - The retained `GT2Compat.cs` Git blob exactly matched the pre-bridge branch blob `4c1f2f3c55b0614936c6e7d4da0d24089a21fb20`.
2. The state-7 generated Arcade overlay anchors were reproduced against the retained L18 generated source:
   - exactly one capture anchor before `func_8006A0C4`
   - exactly one completion anchor before `func_8006A1C4`
   - order: capture < op6 submit < completion < cleanup
3. Temporary branch-only GitHub Actions validation installed .NET 10 and passed:
   - `python3 -m py_compile tools/apply_gt2_arcade_enhancements.py`
   - source contract checks
   - `dotnet build vendor/RecompOne/RecompOne.Runtime/RecompOne.Runtime.csproj -c Release -p:OpenGTLinuxHost=true`
   - `dotnet build tools/modern-renderer-config-tests/ModernRendererConfigTests.csproj -c Release -p:OpenGTLinuxHost=true`
   - `dotnet run --project tools/arcade-save-bridge-tests/ArcadeSaveBridgeTests.csproj -c Release -p:OpenGTLinuxHost=true`
4. Persistence regression passed end-to-end at the host bridge level:
   - CRC-valid outgoing payload captured
   - guest payload deliberately changed after capture to model op-6 reuse/clobber
   - completion persisted the pre-clobber snapshot
   - new `MemoryCard` instance reloaded exact persisted bytes from disk
   - GT2 CRC remained valid
   - disabled bridge produced no write
   - CRC-invalid payload was rejected and produced no write

The first version of the test used `PSMemory` and correctly failed because L18 raw-track replacement requires the native renderer. The test was then isolated to a minimal `IMemory` fixture so it exercises only save/card semantics. The corrected test passed.

## Retained artifact identities verified this turn

- `OpenGT_Lighting_L18_World_Geometry_Unbake_Audit_Candidate.zip`
  - SHA-256 `4ebbd8391e445b792f21dfffa9b23bb15a75d83054edefc0761349e96cbfd0d3`
- `OpenGT-L18-Linux-Ready-v2.zip`
  - SHA-256 `3a2c0da55b1e9b4efdad7419b14ccfd00b7d95be7d568a27eb8ff39061de6fb3`
- `OpenGT-L18-linux-selfcontained-packs.zip`
  - SHA-256 `8c73bf0830ef3af68d4520587bfedea30273bdf37182081fd2119f5195774a0e`

## Remaining gate

This is source/build/host-persistence validation, **not** the required ordinary gameplay save/reload regression. The current active environment does not contain the prepared Arcade/Simulation game-data root used by the earlier real Linux gameplay runs, so a legitimate gameplay/UI state change could not be executed this turn.

Next: restore/materialize that exact prepared game-data root, make a normal Arcade state change through gameplay/UI before Save Game, verify the payload was prepared naturally, save with this bridge, validate card structure/CRC/changed state, then clean-restart with QA mutation/forced-refresh hooks disabled. If normal gameplay does not prepare fresh serialized bytes, isolate the normal serializer-trigger gap rather than reintroducing the QA forced-refresh shortcut.
