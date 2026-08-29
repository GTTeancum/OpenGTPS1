# OpenGTPS1 0.8beta

This is the first public beta of the Windows x64 static-recompilation port.

## Important

OpenGTPS1 0.8beta supports only the byte-exact US Gran Turismo 2 Simulation
Disc (`SCUS-94488`, NTSC-U revision 2) and Arcade Disc (`SCUS-94455`) pair.
Other regions and revisions are not supported.

The release contains no game data. The included setup utility validates and
extracts the required loose files from the user's own matching raw IMG.

## Highlights

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
- Direct Arcade entry skips the Arcade-disc title while retaining its authored
  confirmation sound, and course selection supports both stock GT2-disc data
  and expanded GT1-content overlays without blank or immovable course lists.
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
