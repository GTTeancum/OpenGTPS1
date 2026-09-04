# OpenGTPS1 0.8beta

This is the first public beta of the Windows x64 static-recompilation port.

## Important

OpenGTPS1 0.8beta supports only the byte-exact US Gran Turismo 2 Simulation
Disc (`SCUS-94488`, NTSC-U revision 2) and Arcade Disc (`SCUS-94455`) pair.
Other regions and revisions are not supported.

The release contains no game data. The included setup utility validates and
extracts the required loose files from the user's own matching raw IMG.

## Highlights

- Early unified-title input is buffered through native initialization. Arcade
  handoff omits redundant hidden boot-panel waits and lets the confirmation
  voice and queued tail finish before switching guests.
- The already-final unified title no longer waits through the stock 16-update
  reveal countdown; required list finalization completes on its first input
  update, and a first-poll confirmation is committed one poll later.
- The original GT2 opening now plays from the Arcade disc's intact `STREAM.DAT`
  before the normal Simulation bootstrap. Native Start skipping is retained;
  duplicate Simulation legal panels are omitted after the movie.
- Filtered foliage fringes no longer write opaque depth; solid foliage retains
  depth ownership over farther terrain.
- Resident foliage uses normalized camera depth, fixing distant Midfield trees
  drawing through buildings and tunnel walls due to reversed ordering indices.

- One Direct3D 11/DXGI graphics path now owns authored menus, HUD, loading,
  Results, MDEC video, native 3D, scaling/FXAA, capture, and the desktop wrapper;
  the Windows release no longer carries an OpenGL renderer dependency.
- Native unified Simulation and Arcade programs with direct Seattle Circuit
  manual-race and natural-replay entry points.
- Native Windows race/replay renderer with perspective-correct resident-course texture mapping,
  exact track-seam handling, extended draw distance, maximum vehicle LOD, and
  configurable dithering.
- True horizontal-plus widescreen with HUD groups anchored to the corresponding
  left and right margins.
- Authored 60 Hz output with no synthetic presentation frames and no shipping
  compatibility renderer or runtime downgrade control.
- Keyboard and Xbox-compatible controller input.
- Memory-card persistence, sound effects, XA audio, external OGG music, mods,
  graphics presets, and structured logs.
- Unified and direct Arcade entry skip the Arcade-disc title, land on the
  native `ARCADE MODE` menu, and retain the authored confirmation sound. Back
  (`Triangle`) from the Arcade root returns directly to the unified title, and
  Arcade Single Player remains in the Arcade Game Selection flow. `Triangle`
  at the idle Gran Turismo world-map root also returns to that unified title;
  nested menus keep their original Back behavior. Course selection supports
  both stock GT2-disc data and expanded GT1-content overlays without blank or
  immovable course lists.
- Data-first Seattle Circuit qualification with a bounded no-pop sector
  horizon, authored mutually exclusive course selection, explicit background/
  world depth layers, and structured renderer diagnostics in development builds.
- Deterministic automatic replays reset 60 Hz physics carry at every race/
  replay boundary and verify the original recorded controller stream and
  resulting vehicle trajectory through bounded structured log oracles.

## Known beta limitations

- Windows x64 only.
- Seattle Circuit is the sole renderer qualification course for this beta.
- Some graphics and hardware-specific compatibility issues may remain.
- The executable is not code-signed, so Windows SmartScreen may warn.

See the `README.md` inside the ZIP for complete installation instructions,
controls, updating guidance, and troubleshooting.
