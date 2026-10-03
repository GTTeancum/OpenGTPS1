# OpenGT L18 Arcade Save Freshness Guard 01

Date: 2026-09-24
Repository: `GTTeancum/OpenGTPS1`
Branch: `chatgpt/l18-runtime-recovery`
Final branch head: `17e2b552e85dc2d6ea617b2e1cc1d054d2225596`

## Goal

Harden the production-minimal Arcade save persistence bridge so an older but internally CRC-valid payload cannot be written as if it represented the current live Arcade game state.

## Exact GT2 serializer mapping

Exact retained L18 generated Arcade source maps `func_8006A124` as follows:

- payload `+0x0000`: copy `0x0200` bytes from the supplied header/source;
- payload `+0x0200`: copy `0x7C9C` bytes from live Arcade state at `0x801C9340`;
- payload `+0x7E9C`: store CRC computed over the preceding `0x7E9C` bytes.

The live base comes directly from generated code expression `0x801D0000 - 0x6CC0 = 0x801C9340`.

The earlier proof mutation at live address `0x801C93F8` is therefore live offset `0xB8` and maps exactly to payload offset `0x2B8`.

`func_80072EAC` invokes this serializer and then puts the save controller in state 4. Static callers include Arcade callbacks `0x800502D8` and `0x80012F40`. The separately traced overlay-2 Save Game overwrite path did not naturally invoke that serializer.

## Problem found

GT2 CRC proves only that a payload is internally self-consistent. It does **not** prove that it reflects current gameplay state. Earlier runtime evidence already demonstrated a live Arcade state byte changing while an older payload remained CRC-valid until the QA-only forced serializer ran.

Therefore a CRC-only persistence bridge could silently write old progress while the native UI reports Save Game success.

## Production guard

Commit `160c760275f342f702cfdba2857a7a8cd3d23979` adds a freshness check to `GT2Compat.CaptureArcadeSaveWrite()`:

1. retain the existing GT2 CRC validation;
2. compare payload bytes `0x200..0x7E9B` against live Arcade bytes `0x801C9340..0x801D0FDB`;
3. reject on the first mismatch and do not stage a persistent write;
4. only a CRC-valid, live-state-matching payload can reach the existing snapshot/commit bridge.

This is intentionally fail-closed. It does not synthesize new save bytes or invoke the QA serializer.

## Regression extension

Commit `7839abe848adfee35d205ec29cb4c4278a028f9b` extends `tools/arcade-save-bridge-tests/Program.cs` to cover:

- valid/fresh payload capture and commit;
- guest buffer clobber after snapshot without changing persisted bytes;
- disk flush and clean `MemoryCard` reload;
- disabled bridge -> no write;
- CRC-valid but stale payload -> reject and no write;
- CRC-invalid payload -> reject and no write.

## Source rewrite validation

The bridge-only generated-source rewrite was run against the exact retained L18 generated Arcade source. It applied exactly once, with required ordering:

- capture inserted before `func_8006A0C4` / state-7 op 6 submission;
- completion inserted only on the existing native success path before cleanup.

A full historical enhancement-script rerun is not expected to be idempotent because the retained L18 archive already contains the earlier L18 generated-source enhancements.

## Linux validation

Temporary validation commit `d3d972ea387161ed3efc2ade5920ac6b0f94f321` ran on GitHub Actions with .NET 10.x and passed:

1. `dotnet build vendor/RecompOne/RecompOne.Runtime/RecompOne.Runtime.csproj -c Release -p:OpenGTLinuxHost=true`
2. `dotnet run --project tools/arcade-save-bridge-tests/ArcadeSaveBridgeTests.csproj -c Release -p:OpenGTLinuxHost=true`
3. `dotnet build tools/modern-renderer-config-tests/ModernRendererConfigTests.csproj -c Release -p:OpenGTLinuxHost=true`

Published status: `chatgpt/l18-save-freshness = success`.

The temporary workflow was removed afterward; final branch head `17e2b552...` differs only by that cleanup.

## Remaining production gate

No ordinary-gameplay regression is claimed in this turn. The original Arcade/Simulation user game-data archives are not retained in the current environment.

When real Arcade game data is restored, perform a legitimate normal gameplay state change with QA mutation/forced serializer hooks disabled, then use Save Game and verify:

- naturally fresh payload is accepted;
- persistent card hash changes;
- GT2 CRC remains valid;
- clean restart reloads the changed state.

If the freshness guard rejects that normal save, the result is useful rather than ambiguous: it demonstrates that the recomp path is missing the normal serializer trigger and that `func_80072EAC` / `func_8006A124` lifecycle must be restored explicitly rather than hiding the problem in the persistence bridge.
