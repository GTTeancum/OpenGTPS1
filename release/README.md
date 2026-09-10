# OpenGTPS1 0.9b for Windows x64

OpenGTPS1 is an experimental static-recompilation port of Gran Turismo 2.
Version 0.9b is playable but may still contain compatibility or visual
issues.

> [!IMPORTANT]
> **Please test 0.9b and report what you find.** Search the
> **[public issue tracker](https://github.com/GTTeancum/OpenGTPS1/issues)**,
> then use **[New issue](https://github.com/GTTeancum/OpenGTPS1/issues/new)**
> for crashes, graphical glitches, performance, input, audio, installation,
> and save problems. Include your hardware, reproduction steps, and
> `logs\OpenGTPS1-latest.log`.

## Supported NTSC-U discs

This build supports one byte-exact US Gran Turismo 2 two-disc set:

```text
Simulation Disc
  Serial:  SCUS-94488
  Region:  NTSC-U
  Revision: 2
  IMG size: 691,850,208 bytes
  IMG SHA-256: D0AB6E70539601057590A36299543C0ADAD219254D712F7D4273219094ED5031

Arcade Disc
  Serial:  SCUS-94455
  Region:  NTSC-U
  IMG size: 729,423,408 bytes
  IMG SHA-256: C2E97D6B0C847CA4336D9D84D8D98C349D1240ED075E81AB3FD5C977E9A45075
```

Both images are required. Other regions and revisions are not supported. Setup
rejects any image that does not match the exact size and SHA-256 above.

The matching US Gran Turismo disc is optional and enables the local GT1
content merge:

```text
Gran Turismo
  Serial:  SCUS-94194
  Region:  NTSC-U
  IMG size: 693,668,304 bytes
  IMG SHA-256: 765A748C4F2975A063A47BA9E42708A4882954D765F9E352C5AF3C0950EAEFB6
```

This release contains no Gran Turismo 2 disc data, Sony BIOS, music, or save
files. You must supply your own matching disc image.

## Installation

1. Extract the entire `OpenGTPS1-0.9b-win-x64` folder to a writable
   location, such as `C:\Games\OpenGTPS1-0.9b-win-x64`.
2. Rip both matching GT2 discs as raw Mode 2/2352 `.img` files. Rip the
   optional matching GT1 disc too if you want its supported content merged.
3. Start `GranTurismo2PC.exe`.
4. The graphical first-run installer searches nearby folders and removable
   drives. Use its Browse buttons for any image it does not find.
5. Choose whether to merge GT1 content, then select **Install and play**.
   Later game launches reuse the prepared installation.

The setup utility verifies both complete images, extracts each original native
program and data set, and writes bounded Simulation and Arcade manifests over a
single native two-member `GT2.VOL`. It does not copy or retain any disc image.
After setup, the images are not required to play.

When the optional GT1 image is selected, the same self-contained installer
converts and merges Special Stage Route 11, the completed native car imports,
GT1-only paints/liveries, and supported Racing Modification body/paint
families. The conversion happens locally and the release contains no GT1
game data.

No Python, .NET SDK, mounted image, or original PlayStation BIOS is required.
The Windows runtime is self-contained.

Keep the installation in a writable folder. OpenGTPS1 stores memory cards,
settings, external music, mods, and logs beside the executable. Blank
`carda.sav` and `cardb.sav` files are created automatically on first launch.

Windows SmartScreen may display a warning because this beta is not
code-signed.

## Current state

- The original opening leads to one unified title menu for native Arcade Mode
  and Gran Turismo Mode.
- Complete races, championships, Results, and native replays are playable.
- Simulation saves load their full native payload. Credits, licenses, garage,
  current car, and records also reach Arcade Mode's Home Garage.
- The sole 3D renderer is the bundled D3D11 authored-world path with
  perspective-correct textures, stabilized topology, extended draw distance,
  maximum vehicle LOD, horizontal-plus widescreen, and roughly 59.94/60 Hz
  output without synthetic frames.
- The package is self-contained and includes its managed and native runtime
  dependencies. It contains no game data, save, BIOS, or music.
- Original-Xbox support is planned but not implemented.

## Known issues

- The generated prize/LM development smoke-test save gives some cars incorrect
  wheel widths.
- At least the black JGTC Castrol Supra in that test save can revert to white
  when a race begins.
- Other visual defects and hardware-specific compatibility problems may remain.

Track current reports through the
**[OpenGTPS1 issues page](https://github.com/GTTeancum/OpenGTPS1/issues)**.

## What needs testing

- Fresh graphical setup and first launch, both with and without the optional
  GT1 content merge.
- Controller and keyboard input, especially the first title-menu press.
- Save creation, reload, updates, backup/restore, and Arcade Home Garage.
- Full races and replays on every course: watch starting grids, reflections,
  flicker, seams, pop-in, wheel placement, and paint/livery persistence.
- Sustained performance; report hardware and situations materially below the
  intended roughly 55–60 FPS range.
- XA music, sound effects, external OGG music, and audio-device behavior.

Reports are welcome while active development is on hiatus. Please search for
duplicates, attach the latest log, and describe the shortest reliable repro.

## Updating

Before replacing an older build, back up:

```text
carda.sav
cardb.sav
settings.json
```

Extract the new release, run `GranTurismo2PC.exe` and complete its graphical
setup, then copy the backed-up files into the new folder.

## Controls

Xbox-compatible controllers map to the equivalent PlayStation controls.

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

Bindings, display, audio, and graphics options are available from the wrapper
menus.

Seattle Circuit also has direct no-menu entry points:

```powershell
.\GranTurismo2PC.exe --arcade-race seattle-circuit
.\GranTurismo2PC.exe --arcade-replay seattle-circuit
```

The race command leaves the player car under normal player control. The replay
command reaches GT2's own CPU driver and replay controller; it does not steer
the car with a scripted input harness.

## External music

Create a `music` folder beside the executable and add OGG Vorbis files named:

```text
music\[artist] - [song].ogg
```

Files are sorted into a looping queue. Sound effects and video audio remain
independent.

## Troubleshooting

Each launch replaces:

```text
logs\OpenGTPS1-latest.log
```

Attach that log when reporting a crash, graphics problem, audio issue, or save
failure.

Common setup failures:

- `Disc image size mismatch` or `Disc image SHA-256 mismatch`: one IMG is not
  from the supported US two-disc set.
- Missing `GT2.VOL`, `TITLE_EXACT.DAT`, or a file below `simulation` or
  `arcade`: setup has not completed in this folder.
- Save or settings errors: move the installation to a writable location
  outside `Program Files`.

## Legal

Gran Turismo and Gran Turismo 2 are trademarks of Sony Interactive
Entertainment. OpenGTPS1 is an independent preservation and compatibility
project and is not affiliated with or endorsed by Sony Interactive
Entertainment or Polyphony Digital.
