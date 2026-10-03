# OpenGT L18 Frozen-L08 Acceptance Harness 04

Date: 2026-09-25
Repository: `GTTeancum/OpenGTPS1`
Branch: `chatgpt/l18-runtime-recovery`
Current head: `aea5cb55f7160b79dbe3b3424143bf4650a1f271`

## Purpose

Eliminate the need to rebuild or recover the historical L08 executable while preserving a deterministic, fail-closed final visual acceptance path for L18.

## Frozen L08 evidence recovered

The L08 checkpoint retains complete per-frame acceptance manifests:

- Seattle: 115 native 1280x720 frames, poll/frame 995 through 1679; manifest SHA-256 `1c7970909d336c71e3b73783ef1a5cc42d28f9a138142d031d3b5bda19ffd8e3`.
- Red Rock: 115 native 1280x720 frames, poll/frame 995 through 1679; manifest SHA-256 `2b60b4020658bfb8b62bef2336d4b1ceeaf0d48520e15e886b0da952533e07db`.
- Red Rock camera-stress: 58 native 1280x720 frames, poll/frame 1205 through 1889; manifest SHA-256 `f0874cfd29a16382fb34776108681e6696967bfeae4d189842de128c2f01e40d`.

Each entry records filename, byte length, image SHA-256, native-render flag, authored frame, source/host poll, dimensions, command count, and render timing.

## Historical baseline is hash-locked

`recovery/manifests/prepared-game-input-identities.json` records the exact accepted L08 baseline. Frozen acceptance requires exact identity for:

- `GT2.VOL` — 708,319,232 bytes — `2156fa6c18bd39866a15cf7c62f59af1c71d6dc1eca2d3b9e80a3b96169c9a73`
- `MUSIC.DAT` — 97,252,352 bytes — `2d1b7a30f656900213fa1f4aff9371c22db9d7f41ae8a9e5c5d51de8ffc9e102`
- `TITLE_EXACT.DAT` — 1,966,100 bytes — `735d838c3a0f12e2917593648790f9fd1cb6ada13d402e19022d7c814737321c`
- `settings.json` — 1,902 bytes — `92ed1825e05bd76d2b0db453cfee9acbe762d1a4e2b445a1772a783f5a1f640a`
- `carda.sav` — 131,072 bytes — `4c463036ea7cf941834195cc28631a0300d63ffa98a9900accfac16e994c56c3`
- `cardb.sav` — 131,072 bytes — `78b6d4ac9ab4d23caf7e5f04f83539bf5d994cccfb0a709d14ac53d05c8e21ef`
- `GTLIVERY.BIN` — 1,748 bytes — `fb388e5a7573f471ca4efadade6d8cf1f506a56f8ba6a5bad8bbb818543c21c8`

A different data root, settings file, or card state is rejected before rendering so baseline drift cannot be misdiagnosed as an L18 renderer regression.

## Tooling

The original dual-run harness remains at commit `c127e41fad838b3508049c0a32163df8b3e4fdce`.

Preferred final runner:

- `tools/run_l18_against_frozen_l08.py`
- introduced: `e0c0f7fc822e1d0395498d7c57facb68cbc964df`
- exact-baseline enforcement: `aea5cb55f7160b79dbe3b3424143bf4650a1f271`

It runs only L18, replays the exact retained L08 Seattle/Red Rock/camera-stress controller and capture contracts, pairs every L18 frame to the frozen L08 `(poll, frame)` set, enforces dimensions, and reports frame hashes.

The script intentionally does not auto-promote L18. An exact SHA-256 match proves pixel identity. A mismatch only identifies a frame for review because L09/L10 intentionally altered billboard/shadow behavior.

## Validation

Local Python compile: PASS.

Self-test: PASS for both:

1. exact frozen-manifest identity; and
2. deliberately modified L18 pixels, which remain structurally paired while correctly reporting hash mismatches.

## Remaining gate

There is now only one external prerequisite: restore the exact historical prepared GT2 baseline bytes listed above. No historical L08 executable is required.

Once restored, run the frozen-L08 tool against the current L18 Linux runner, inspect hash-mismatched frames against retained L08 screenshots/videos and shadow/camera audit evidence, and promote L18 only after visual review confirms parity plus the intentional billboard fixes.
