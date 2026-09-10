# OpenGTPS1 0.9b

OpenGTPS1 0.9b is the current Windows x64 release of the Gran Turismo 2
static-recompilation port.

> [!IMPORTANT]
> **Please test this release and report problems through the
> [public issue tracker](https://github.com/GTTeancum/OpenGTPS1/issues).**
> Search existing reports first, then use
> **[New issue](https://github.com/GTTeancum/OpenGTPS1/issues/new)** and attach
> `logs\OpenGTPS1-latest.log` with your hardware and reproduction steps.

## Same-version installer hotfix

The 0.9b archive was replaced without a version increment to correct an
undocumented packaging omission:

- Running `GranTurismo2PC.exe` on a fresh extraction now opens a graphical
  first-run installer automatically.
- Setup searches nearby folders and removable drives, supplies normal Browse
  buttons, validates the supported images, shows progress, and continues into
  the game without requiring a PowerShell command.
- The GUI also accepts the optional US Gran Turismo disc (`SCUS-94194`) and
  locally merges the completed GT1 content conversion into the installation.
  This includes Special Stage Route 11, native car imports, GT1-only
  paints/liveries, and supported Racing Modification body/paint families.
- The setup executable contains its conversion runtime and dependencies. It
  never modifies, copies, or retains the source disc images.

## Where the game is now

- One executable boots the original GT2 opening and presents a unified title
  menu for the native US Arcade and Simulation programs.
- Gran Turismo Mode supports its original home map, dealers, garage, upgrades,
  licenses, events, championships, Results, and replay flow.
- Arcade Mode supports its native menus, classes, cars, courses, races,
  two-player flow, guest garage, Results, and replay flow.
- Memory cards load before the unified title. The complete native save payload,
  including credits, licenses, garage, current car, settings, and records, is
  preserved when entering Arcade Mode so Home Garage remains available.
- Authored menus, video, HUD, and transitions use GT2's original 2D command
  compositor. Race and replay worlds use one bundled D3D11 renderer with
  perspective-correct textures, stabilized authored topology, extended draw
  distance, maximum vehicle LOD, horizontal-plus widescreen, and roughly
  59.94/60 Hz presentation without synthetic frames.
- Sound effects, XA audio, external OGG music, keyboard input,
  Xbox-compatible controllers, configurable output presentation, bounded logs,
  and local mods are available.
- The archive is self-contained. Its single executable bundles the .NET
  runtime, native renderer, windowing, input, audio, and UI dependencies. Users
  do not need Python, a .NET installation, CMake, Visual Studio, or a
  PlayStation BIOS.

## Major changes since 0.8beta

- Added the unified native Arcade/Simulation host and exact two-disc setup.
- Restored the original Arcade-disc opening before the seamless unified title,
  while retaining native Start skipping and responsive first-menu input.
- Added direct native Arcade race/replay paths and deterministic bounded test
  harnesses.
- Replaced the selectable compatibility graphics paths with one fail-closed
  authored-world D3D11 path shared by development and release builds.
- Added genuine 59.94 Hz game/render timing, 60 FPS output, horizontal-plus
  widescreen, perspective-correct world textures, stabilized seams, extended
  visibility, maximum vehicle LOD, and bundled native rendering.
- Restored starting-grid visibility and retained stable authored vehicle
  reflections without the earlier flicker or fundamental reflection-style
  replacement.
- Fixed unified-title save loading and full-save transfer into Arcade Home
  Garage.
- Added GT1 conversion and validation for
  Special Stage Route 11, native cars, Racing Modification bodies, wheels,
  paints, and alternate-body liveries. The optional GUI setup path now creates
  and merges this content locally from the user's validated US GT1 image; the
  release does not distribute proprietary GT1 content.
- Added bounded release-policy, renderer, race, replay, save, audio, and package
  verification tools.

## Known open issues

- The generated prize/LM development smoke-test save gives some cars incorrect
  wheel widths.
- At least the black JGTC Castrol Supra in that test save can revert to white
  when a race begins.
- Original-Xbox support remains future work.
- Hardware- and driver-specific graphical, input, audio, and performance
  problems may still exist outside the systems used during development.

The live list is the
**[OpenGTPS1 issue tracker](https://github.com/GTTeancum/OpenGTPS1/issues)**.

## What needs testing

- Fresh GUI installation from both exact supported GT2 images, with and
  without the optional supported GT1 image.
- First title-menu input and long controller/keyboard sessions.
- New and existing save creation, loading, updating, backup/restore, and Arcade
  Home Garage behavior.
- Complete races and replays across every course, including starting grids,
  vehicle reflections, flicker, texture seams, pop-in, wheel placement, and
  paint/livery persistence.
- Sustained performance on a broad range of GPUs and CPUs. Normal gameplay is
  intended to remain around 55–60 FPS; report repeatable material drops below
  that range.
- XA music, sound effects, external OGG playback, and unusual audio-device
  configurations.

Active development is entering a hiatus, so concise reproducible community
reports are the best way to preserve useful next steps.

## Supported discs and installation

The two GT2 images are required. The GT1 image is optional:

```text
Simulation Disc
  Serial:  SCUS-94488
  Revision: 2
  IMG size: 691,850,208 bytes
  IMG SHA-256: D0AB6E70539601057590A36299543C0ADAD219254D712F7D4273219094ED5031

Arcade Disc
  Serial:  SCUS-94455
  IMG size: 729,423,408 bytes
  IMG SHA-256: C2E97D6B0C847CA4336D9D84D8D98C349D1240ED075E81AB3FD5C977E9A45075

Optional Gran Turismo content source
  Serial:  SCUS-94194
  IMG size: 693,668,304 bytes
  IMG SHA-256: 765A748C4F2975A063A47BA9E42708A4882954D765F9E352C5AF3C0950EAEFB6
```

Extract `OpenGTPS1-0.9b-win-x64.zip` and run `GranTurismo2PC.exe`. Its
graphical first-run setup finds or asks for both GT2 images and offers the GT1
merge as an optional checkbox. Complete instructions are in the packaged
`README.md`.

The release contains no Gran Turismo game data, Sony BIOS, save files, or
music. Every selected source image is validated, read without modification,
and is not copied into or retained by the installed game.
