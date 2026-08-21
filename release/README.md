# OpenGTPS1 0.8beta for Windows x64

OpenGTPS1 is an experimental static-recompilation port of Gran Turismo 2.
Version 0.8beta is a public beta and may still contain compatibility or visual
issues.

## Simulation Disc only

This build supports only the US Gran Turismo 2 **Simulation Disc**:

```text
Serial:  SCUS-94488
Region:  NTSC-U
Revision: 2
IMG size: 691,850,208 bytes
IMG SHA-256: D0AB6E70539601057590A36299543C0ADAD219254D712F7D4273219094ED5031
```

The Arcade Disc, other regions, and earlier US revisions are not supported.
Setup rejects any image that does not match the exact size and SHA-256 above.

This release contains no Gran Turismo 2 disc data, Sony BIOS, music, or save
files. You must supply your own matching disc image.

## Installation

1. Extract the entire `OpenGTPS1-0.8beta-win-x64` folder to a writable
   location, such as `C:\Games\OpenGTPS1-0.8beta-win-x64`.
2. Rip your matching Simulation Disc as a raw Mode 2/2352 `.img` file.
3. Open PowerShell in this folder.
4. Run:

   ```powershell
   powershell -ExecutionPolicy Bypass -File `
     .\Setup-From-Simulation-Disc.ps1 `
     -ImagePath "D:\Rips\Gran Turismo 2 [Simulation Disc] [U] [SCUS-94488].img"
   ```

5. Wait for `Installation complete`.
6. Start `GranTurismo2PC.exe`.

The setup utility verifies the complete IMG before extracting the seven loose
runtime files. It does not copy or retain the original disc image. After setup,
the IMG is not required to play.

No Python, .NET SDK, mounted image, or original PlayStation BIOS is required.
The Windows runtime is self-contained.

Keep the installation in a writable folder. OpenGTPS1 stores memory cards,
settings, external music, mods, and logs beside the executable. Blank
`carda.sav` and `cardb.sav` files are created automatically on first launch.

Windows SmartScreen may display a warning because this beta is not
code-signed.

## Updating

Before replacing an older build, back up:

```text
carda.sav
cardb.sav
settings.json
```

Extract the new release, run its setup utility against your matching IMG, then
copy the backed-up files into the new folder.

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

- `Disc image size mismatch` or `Disc image SHA-256 mismatch`: the IMG is not
  the supported US Simulation Disc revision 2.
- Missing `SYSTEM.CNF` or `recompone.loose.json`: setup has not completed in
  this folder.
- Save or settings errors: move the installation to a writable location
  outside `Program Files`.

## Legal

Gran Turismo and Gran Turismo 2 are trademarks of Sony Interactive
Entertainment. OpenGTPS1 is an independent preservation and compatibility
project and is not affiliated with or endorsed by Sony Interactive
Entertainment or Polyphony Digital.
