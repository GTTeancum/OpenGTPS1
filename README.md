# OpenGTPS1

OpenGTPS1 is an experimental static-recompilation port of the US Gran Turismo
2 Simulation Disc (`SCUS-94488`, NTSC-U revision 2), built with
[RecompOne](vendor/RecompOne/UPSTREAM.md).

The project currently targets Windows x64. It boots through the original game
flow, renders menus and videos, supports controllers and memory cards, and can
run a purchased and upgraded car through a complete race and replay. The next
major milestone is a portable native renderer designed for both modern PCs and
the original Xbox through [NXDK](https://github.com/XboxDev/nxdk).

> [!IMPORTANT]
> This repository contains no Gran Turismo 2 disc data, Sony BIOS, music, save
> files, or other copyrighted game assets. You must supply your own matching
> disc. Do not open an issue asking for game files.

## Project status

OpenGTPS1 is a development build, not a finished release.

- The core Windows port is playable from boot through a complete race.
- Menus, videos, input, memory-card persistence, sound effects, and XA audio
  are implemented.
- The distributable runtime uses loose files only; it never needs a mounted or
  adjacent BIN/CUE/CCD/IMG/SUB image after preparation.
- External OGG music, wrapper-level graphics presets, structured logging, and a
  deterministic AI-driven test harness are available.
- Perspective-correct textures, road-seam handling, extended draw distance,
  maximum vehicle LOD, and dithering controls exist in the current renderer,
  but graphics work remains active. The planned native renderer will replace
  PS1-era rasterization workarounds with real geometry, depth, and material
  handling.
- Resolution and widescreen support are deliberately deferred until that
  renderer is established.
- Original Xbox support has not landed yet.

[`TO-DO.MD`](TO-DO.MD) is the detailed implementation and validation record.
[`docs/PORT_PLAN.md`](docs/PORT_PLAN.md) documents the playable vertical slice
and the RecompOne-specific discoveries behind it. The
[`modern renderer architecture`](docs/MODERN_RENDERER.md) defines the shared PC
and original-Xbox direction.

## Requirements

- Windows 10 or 11, x64
- PowerShell
- Python 3
- .NET 10 SDK
- Your own US Gran Turismo 2 Simulation Disc, revision 2

The archival input must use these exact names in the repository root:

```text
Gran Turismo 2 [Simulation Disc] [U] [SCUS-94488].cue
Gran Turismo 2 [Simulation Disc] [U] [SCUS-94488].img
```

Other dump formats and game revisions are not currently supported. Disc files
and all extracted/generated data are excluded by `.gitignore`.

## Build from source

Clone the repository, place the matching CUE/IMG pair in its root, and run:

```powershell
python tools\extract_disc.py
powershell -ExecutionPolicy Bypass -File tools\build.ps1 -Regenerate
```

Recompilation creates ignored developer output under `generated\`. Packaging
creates the one-folder installation under `OpenGTPS1\`:

```text
OpenGTPS1\
  GranTurismo2PC.exe
  interface.ini
  recompone.loose.json
  music\
  ...loose game files...
```

After the first regeneration, ordinary rebuilds do not read the archival disc
image:

```powershell
powershell -ExecutionPolicy Bypass -File tools\build.ps1
```

Run the packaged executable directly:

```powershell
OpenGTPS1\GranTurismo2PC.exe
```

Or launch the development build:

```powershell
powershell -ExecutionPolicy Bypass -File tools\run.ps1
```

## Controls

Xbox-compatible controllers map to the equivalent PlayStation controls.
Default keyboard bindings are:

| PlayStation control | Keyboard |
| --- | --- |
| D-pad | Arrow keys |
| Cross / accelerate | Z |
| Circle | X |
| Square / brake | A |
| Triangle | S |
| Start | Enter |
| Select | Right Shift |
| L1 / R1 | Q / W |
| L2 / R2 | E / R |

Bindings, display, audio, and graphics options are available in the wrapper
menus.

## Graphics presets

Resolution and fullscreen are separate from the quality preset.

- **PS1 Quality** uses affine texture projection, original visibility and
  vehicle LOD behavior, no seam stabilization, and dithering.
- **Enhanced** enables perspective-correct projection, seam stabilization,
  extended track visibility, maximum vehicle LOD, and disables dithering.
- **Custom** exposes projection, seam stabilization, draw distance, vehicle
  LOD, and dithering as independent settings.

These settings describe the current compatibility renderer. The native
renderer roadmap keeps the same wrapper-facing controls while moving geometry,
lighting, depth, and material work into portable C++ backends.

## External music

Place OGG Vorbis files next to the executable:

```text
OpenGTPS1\music\[artist] - [song].ogg
```

Files are sorted into a looping queue. Artist and title labels are derived from
the filename. Sound effects and video XA audio remain independent.

## Diagnostics and safe automation

Each launch replaces:

```text
OpenGTPS1\logs\OpenGTPS1-latest.log
```

The log is capped at 4 MiB. Attach it when reporting graphics, frame-pacing,
audio, CD, or save problems.

Unattended runs must use:

```powershell
OpenGTPS1\GranTurismo2PC.exe --headless
```

Headless mode hides the window, forces SDL's dummy audio backend, and fails
closed if a physical audio device is active. For a visible but silent
diagnostic session, use `--mute`; it does not overwrite the saved audio
settings.

The scripts in `tools\` include bounded capture and regression helpers. The
input fixtures under `tests\fixtures\` drive deterministic game flows; they do
not contain game data.

The `modern-renderer` branch includes bounded projected- and world-scene
bridges. `tools\capture_projected_scene.ps1` captures one live draw stream plus
VRAM and renders independent perspective and affine PNGs through the portable
C++ core. `tools\capture_world_scene.ps1` additionally captures upstream
model/view coordinates, camera transforms, stable track/vehicle identity,
exact per-vertex projection state, materials, and original draw order, then
validates and exports the scene through the native loader.

The branch now also builds `opengt_world_viewer.exe`, a standalone D3D11
backend over the API-neutral C++17 world draw list. It supports hardware
rendering, deterministic WARP validation, perspective-correct PS1 materials,
object-scoped depth that preserves GT2 ordering layers, explicit optional
dithering, CPU-oracle comparison, and a `--window` inspection mode. Validate a
captured world frame twice with bounded lossless PNG output:

```powershell
cmake -S native -B build\native
cmake --build build\native --config Release
powershell -ExecutionPolicy Bypass -File tools\validate_world_renderer.ps1 `
  -Capture artifacts\modern-world-v3-vehicles\race-frame.ogtwcap
```

See
[`docs/MODERN_RENDERER.md`](docs/MODERN_RENDERER.md) for the formats, exact
commands, audio-safety checks, and next renderer milestone.

## Repository layout

```text
docs\                  Port notes and architecture documentation
tests\fixtures\        Deterministic input/configuration fixtures
tools\                 Extraction, build, packaging, capture, and test tools
vendor\RecompOne\      RecompOne source plus OpenGTPS1 runtime changes
generated\             Ignored generated recompilation output
OpenGTPS1\             Ignored one-folder local deployment
work\                  Ignored extracted and intermediate game data
artifacts\             Ignored local captures, saves, and test evidence
```

## Legal

Gran Turismo and Gran Turismo 2 are trademarks of Sony Interactive
Entertainment. OpenGTPS1 is an independent preservation and compatibility
project and is not affiliated with or endorsed by Sony Interactive
Entertainment or Polyphony Digital.

No license has yet been granted for original OpenGTPS1 code. The vendored
RecompOne project retains its own license and attribution files.
