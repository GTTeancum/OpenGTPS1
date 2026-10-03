# OpenGT L18 Baseline Recovery 05

Date: 2026-09-25
Repository: `GTTeancum/OpenGTPS1`
Branch: `chatgpt/l18-runtime-recovery`
Branch head: `8099fc1ca5ca66cd66ad439d658d3c1b5340dff9`

## Purpose

Remove ambiguity from the final L18-vs-L08 visual acceptance prerequisite without weakening the frozen L08 baseline.

## Historical recovery path recovered

The retained L08 checkpoint contains its original `validation/execution-drivers/restore.py`. It opened the user's multipart `OpenGTPS1.7z.*` with `libarchive.so.13` and restored only the required content from `OpenGTPS1/OpenGTPS1/`:

- `GT2.VOL`
- `MUSIC.DAT`
- `TITLE_EXACT.DAT`
- `GTLIVERY.BIN`
- `settings.json`
- `carda.sav`
- `cardb.sav`
- complete `arcade/`, `simulation/`, and `manifests/` trees

Thus the original OpenGT archive set (or an exact extracted prepared directory) is the authoritative and sufficient recovery source.

## Current retention search

A full current Library/conversation scan by filename, expected size, and known SHA-256 did not recover the original game payload or archive parts. Existing L04-L18 checkpoint/evidence archives intentionally retain identities, source, logs and visual evidence rather than copyrighted game payload bytes.

## Stable L04-L08 baseline state

The L04, L05, L06, L07 and L08 checkpoints all record identical hashes for the mutable prepared state:

- `settings.json`: 1,902 bytes — `92ed1825e05bd76d2b0db453cfee9acbe762d1a4e2b445a1772a783f5a1f640a`
- `carda.sav`: 131,072 bytes — `4c463036ea7cf941834195cc28631a0300d63ffa98a9900accfac16e994c56c3`
- `cardb.sav`: 131,072 bytes — `78b6d4ac9ab4d23caf7e5f04f83539bf5d994cccfb0a709d14ac53d05c8e21ef`
- `GTLIVERY.BIN`: 1,748 bytes — `fb388e5a7573f471ca4efadade6d8cf1f506a56f8ba6a5bad8bbb818543c21c8`

`cardb.sav` is reproducible exactly from RecompOne's blank-card formatter and the generated SHA matches the historical identity. Historical `settings.json` and `carda.sav` are preserved baseline state and are not synthesized.

Current default settings are *not* the historical settings: 1,752 bytes / `b7c272ede3be61224d91d33a2c05c8d1a8fa8c318916df1c6aab821b5f9a0395`, so substituting them would invalidate formal parity.

## New tooling

### `tools/restore_l08_visual_baseline.py`
Commit `1a3afbcf5cc2f6aae607636263cab87992eacf6b`.

- reads original split archives through the same libarchive strategy as L08;
- supports already-extracted roots;
- applies path-safety checks;
- restores only the required prepared baseline and mode/manifests trees;
- verifies exact frozen L08 byte counts and SHA-256 values;
- can create only the proven deterministic card-B fallback;
- writes a machine-readable restore report and fails on unresolved/mismatched inputs.

The scratch implementation passed syntax testing, synthetic libarchive extraction, extracted-root mode, exact SHA verification and deterministic card-B fallback testing.

### `tools/run_l18_frozen_acceptance_from_archive.py`
Commit `8099fc1ca5ca66cd66ad439d658d3c1b5340dff9`.

One command now performs:

1. exact L08 baseline restore;
2. fail-closed historical identity verification;
3. L18 replay using the frozen L08 Seattle / Red Rock / camera-stress contracts;
4. poll/frame pairing against authoritative frozen L08 manifests;
5. evidence output for human review.

It never promotes L18 automatically.

## Remaining external input

The only remaining external input is the original `OpenGTPS1.7z.*` archive set (or an exact extracted `OpenGTPS1/OpenGTPS1` directory). Once supplied, no L08 executable rebuild and no manual baseline reconstruction are necessary.
