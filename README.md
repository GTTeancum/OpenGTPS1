# OpenGTPS1

OpenGTPS1 0.8beta is an experimental static-recompilation port of the US
Gran Turismo 2 **Simulation Disc** (`SCUS-94488`, NTSC-U revision 2), built with
[RecompOne](vendor/RecompOne/UPSTREAM.md).

The project currently targets Windows x64. It boots through the original game
flow, renders menus and videos, supports controllers and memory cards, and can
run a purchased and upgraded car through complete races, championships, and
replays. Its portable native race/replay renderer is designed to support both
modern PCs and, in a future port, the original Xbox through
[NXDK](https://github.com/XboxDev/nxdk).

> [!IMPORTANT]
> This repository contains no Gran Turismo 2 disc data, Sony BIOS, music, save
> files, or other copyrighted game assets. You must supply your own matching
> disc. Do not open an issue asking for game files.

## Project status

OpenGTPS1 0.8beta is a public beta, not a finished 1.0 release.

- The core Windows port is playable from boot through a complete race.
- Menus, videos, input, memory-card persistence, sound effects, and XA audio
  are implemented.
- The distributable runtime uses loose files only; it never needs a mounted or
  adjacent BIN/CUE/CCD/IMG/SUB image after preparation.
- External OGG music, wrapper-level graphics presets, structured logging, and a
  deterministic AI-driven test harness are available.
- The packaged native race/replay renderer provides perspective-correct
  textures, exact road-seam handling, extended draw distance, maximum vehicle
  LOD, and dithering controls. Original PS1 presentation remains in use for
  menus, videos, HUD layers, and display transitions.
- Graphics work remains active; visual defects and hardware-specific problems
  may still exist.
- Resolution and widescreen expansion remain deferred while that renderer
  matures.
- Original Xbox support has not landed yet.

[`TO-DO.MD`](TO-DO.MD) is the detailed implementation and validation record.
[`docs/PORT_PLAN.md`](docs/PORT_PLAN.md) documents the playable vertical slice
and the RecompOne-specific discoveries behind it. The
[`modern renderer architecture`](docs/MODERN_RENDERER.md) defines the shared PC
and original-Xbox direction.

## Planned unified first-run installation

The completed unified release will include a first-run preparation tool. It
may be built into `GranTurismo2PC.exe` or shipped as a separate installer
executable. Before either game mode can run, it will ask the user to locate
images of all three supported US discs:

- Gran Turismo 2 Simulation Disc (`SCUS-94488`, NTSC-U revision 2)
- Gran Turismo 2 Arcade Mode Disc (`SCUS-94455`, NTSC-U)
- Gran Turismo (`SCUS-94194`, NTSC-U)

The preparation tool will validate all three images, extract the required
files, convert Gran Turismo 1-exclusive cars, liveries, and tracks to the
native Gran Turismo 2 formats, and build the deterministic unified `GT2.VOL`
and loose-file installation. Source disc images are treated as read-only and
are not copied into the completed installation.

`GranTurismo2PC.exe` must validate the prepared installation on startup and
must not open the game or either mode until conversion and merging have
succeeded. If preparation is incomplete, missing, or damaged, it directs the
user to the preparation tool instead. Later launches use the validated
prepared data and do not request the discs again.

This is a product requirement for the unified release, not the behavior of the
current 0.8beta package described below. The current package still uses its
separate Simulation-Disc setup script.

The development conversion pipeline now validates the US Gran Turismo image
and imports Special Stage Route 11, all three GT1-exclusive EUNOS ROADSTER
Arcade families, and the GT1 Civic Racer as native GT2 data. It converts all
six Route 11 variants and
the authored
`dawn3` background, preserves the exact `ARCADE.DAT` entry 81 selection art,
and adds the Roadsters as the tenth through twelfth Class C entries with their
original wordmarks and all twenty-three authored GT1 paint/livery palettes.
The third entry is the six-palette 145 PS `EUNOS ROADSTER RS`, whose separate
GT1 day/night models are structurally converted to native GT2 CDO/CNO data
without substituting GT2 body geometry. The Civic Racer is a native tenth
Class B entry with its unique body and all three turquoise, pink, and yellow
GT1 liveries. The GT1 DB7 Coupe is a native ninth Class A entry with its exact
selection artwork and all three white, burgundy, and deep-purple paints. The
GT1 Impreza WRX-STi Version III is a native tenth Class A entry with its unique
converted body and all three liveries. The GT1 Soarer 2.5GT-T VVT-i is the
eleventh Class A entry with its exact selection artwork, unique converted
day/night body, and all three wine-red, yellow, and purple palettes. The GT1
Supra RZ is the twelfth Class A entry with its exact selection artwork and
three turquoise, purple, and bronze GT1 liveries. The GT1 S13 Silvia Q's
1800cc is a native thirteenth Class C entry with its original named menu
artwork and three wine-red, yellow, and green palettes. The GT1 Lancer
Evolution IV GSR is the thirteenth Class A entry with its exact named logo and
yellow, teal, and purple palettes. The GT1 Alcyone SVX S4 is the eleventh
Class B entry with its exact named logo and white, blue, and purple palettes.
The GT1 Celica SS-II is the twelfth Class B entry with its exact named menu
art, structurally converted body/UV data, teal, purple, and yellow palettes,
and target-owned 1,220 kg chassis record.
The deterministic
`GTPATCH.VOL` also carries sorted native Racing and Drift parameter records
assembled from the matching GT2 V-Special chassis/suspension, S-Special
wheel/tire package, GT1-equivalent Mazda brake conversion, and direct Roadster
RS, Civic, DB7, Impreza, Soarer, Supra, Silvia, Lancer, Alcyone, and Celica
specifications. Menu-to-race smokes have run the track and all twelve cars
under GT2's native AI controller
without an
unmapped call, managed exception, or software fault. Remaining exclusive cars
and livery families are the next content milestone.

## Install the 0.8beta Windows release

The prebuilt release requires:

- Windows 10 or 11, x64
- PowerShell
- Your own US Gran Turismo 2 **Simulation Disc**, revision 2

The Arcade Disc, other regions, and earlier US revisions are not supported.
The required raw Mode 2 IMG has:

```text
Serial:  SCUS-94488
Size:    691,850,208 bytes
SHA-256: D0AB6E70539601057590A36299543C0ADAD219254D712F7D4273219094ED5031
```

The release contains no game data. To install:

1. Download `OpenGTPS1-0.8beta-win-x64.zip` from the GitHub release.
2. Extract the entire `OpenGTPS1-0.8beta-win-x64` folder to a writable
   location. Do not run it from inside the ZIP.
3. Rip your matching Simulation Disc as a raw Mode 2/2352 `.img` file.
4. Open PowerShell in the extracted folder and run:

   ```powershell
   powershell -ExecutionPolicy Bypass -File `
     .\Setup-From-Simulation-Disc.ps1 `
     -ImagePath "D:\Rips\Gran Turismo 2 [Simulation Disc] [U] [SCUS-94488].img"
   ```

5. When setup reports `Installation complete`, run `GranTurismo2PC.exe`.

Setup validates the complete disc hash before writing anything, extracts only
the required loose runtime files beside the executable, and does not copy or
retain the original IMG. The game creates blank `carda.sav` and `cardb.sav`
memory cards on first launch.

Keep the installation in a writable folder because saves, settings, and the
latest diagnostic log are stored beside the executable. Windows SmartScreen
may warn because this beta is not code-signed.

The ZIP also contains an installation-focused `README.md`. Existing users
should back up `carda.sav`, `cardb.sav`, and `settings.json` before replacing
an older build.

## Build requirements

Building from source additionally requires:

- Python 3
- .NET 10 SDK
- CMake and a Visual Studio C++ x64 toolchain

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
dithering, CPU-oracle comparison, a `--window` inspection mode, and viewer-only
`--scale 1` through `--scale 8` diagnostic output. The scale switch rerasterizes
at the requested size and is separate from the deferred wrapper
resolution/widescreen work.

World-capture format version 4 supplies capture-stable authored track vertex
identity. The portable topology pass uses that identity plus exact integer GTE
view coordinates to join authored sector boundaries, subdivide exact
T-junctions, and choose deterministic ownership for same-material coplanar
overlap. It performs no proximity search or screen-space triangle expansion.
The D3D11 point sampler also treats PS1 integer UVs as texel centers; this
removes the intermittent Red Rock replay road line around 0:33 without padding.
The same native renderer is integrated into packaged live race/replay
presentation; the standalone viewer remains available for deterministic
capture inspection and renderer development.

Validate a captured world frame twice with bounded lossless PNG output:

```powershell
cmake -S native -B build\native
cmake --build build\native --config Release
powershell -ExecutionPolicy Bypass -File tools\validate_world_renderer.ps1 `
  -Capture artifacts\modern-world-v4-topology-live\race-frame.ogtwcap
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
