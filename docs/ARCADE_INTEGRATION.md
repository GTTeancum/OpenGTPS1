# Native Arcade Mode Integration

## Product requirement

Arcade Mode must execute the original `SCUS_944.55` program and its six
overlays through the same native runtime and renderer as Simulation Mode.
Replacing its menus, movies, race presentation, or other visuals with a host
approximation is not an acceptable final implementation.

## Disc findings

- The Arcade `GT2.VOL` contains 10,618 named entries.
- Every Arcade entry name is present in the Simulation `GT2.VOL`.
- The Simulation volume adds 960 names and is therefore a strict name
  superset.
- Eighty-six shared entries differ in payload, including the Arcade race text
  database, panel art, car data, and course data. Thirty-nine of those also
  differ in stored size. Substituting the Simulation payloads is not
  compatible: it corrupts Arcade HUD and results text even though the menus
  still boot.
- The raw `MUSIC.DAT` sector data is byte-identical between the two discs.

The unified install therefore keeps both original GTFS members byte-for-byte
inside one physical `GT2.VOL`. Simulation reads member 0; Arcade reads the
second member at byte offset `488241152`. Each guest still sees its original
disc-visible `GT2.VOL` at LBA 473. The Arcade executable, overlays, raw
`STREAM.DAT`, and disc metadata also remain the original Arcade versions.

## Implemented foundation

- `prepare_arcade_reference.py` recompiles the Arcade executable and all six
  Arcade overlays.
- Simulation and Arcade output use separate generated namespaces, allowing
  both original codebases to coexist in one managed assembly.
- The Arcade compatibility addresses are mapped from Simulation using unique,
  normalized MIPS instruction fingerprints rather than assumed address deltas.
- Loose manifests can map a disc-visible file to a bounded byte range inside
  one safe source file. This is how both native GTFS members share one
  generated `GT2.VOL` without exposing either disc's incompatible payloads to
  the other executable.
- Loose replacement files may be larger when opened by name without claiming
  LBAs belonging to later files in the original disc layout.
- `prepare_unified_install.py` performs the deterministic two-member volume
  merge and writes both bounded manifests.
- One unified install shares the merged `GT2.VOL` and `MUSIC.DAT`, with
  mode-specific bootstrap and streaming files under `simulation/` and
  `arcade/`.
- The original Simulation title list contains `Arcade Mode` first and
  `Gran Turismo Mode` second. Selecting Arcade triggers a clean guest/runtime
  reset and enters the original Arcade frontend directly.
- One `GranTurismo2PC` host assembly contains both original programs and all
  twelve overlays. There are no public mode-selection switches or substitute
  host menus.

`tools/test_unified_modes.ps1` selects both modes through the original unified
title menu and verifies their displays from the same executable and shared
loose install without an unmapped call or managed exception.

## Validated native flow

The deterministic full-race fixture has completed:

`Unified title → Arcade Mode → Single Player → Road Race → Easy → Class A →`
`Car/Transmission/Course → Starting Grid → two-lap race → Results → Replay →`
`Single Race menu → Exit → Arcade Mode → Single Player re-entry`

The same cleaned build also selects Gran Turismo Mode and reaches the original
Simulation home map. Both paths use the native renderer and original disc art.
