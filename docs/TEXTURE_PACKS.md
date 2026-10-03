# Texture replacements

Texture packs replace individual authored game images. They do not replace VRAM
screenshots. Export textures from your materialized GT2 VOL, edit the PNGs, and
build a pack. AI upscaling is optional; hand-painted artwork uses the same runtime.

## Export and edit

Install Python, Pillow and NumPy (`python -m pip install pillow numpy`). From the
repository root:

```powershell
python tools/edit_gt2_texture_pack.py export `
  --volume OpenGTPS1/GT2.VOL `
  --course tahiti_t --car t002n `
  --tim carlogo/v-rbrl--.tim `
  --output work/my-textures
```

Selectors are repeatable. `--course` selects a course TRP stem, `--car` a car
stem, and `--tim` an exact archive path. Exporting into an existing project is
refused to protect edits. `textures.json` records the source names, dimensions,
and runtime bindings; artwork is in `textures/`. Leave those bindings intact.

Edit PNGs at their original size or a uniform integer multiple (2x, 4x, etc.).
Different images can use different scales. Do not add padding. Each authored
image remains separate; a missing pack entry retains the original game texture.
Delete an entry from `textures.json` to omit it from the built pack.

Car PNGs are neutral detail maps. Their original baseline stays in `originals/`.
Editing detail preserves the game's selected paint and lighting palettes; this
is not a car livery editor. Course and supported menu PNGs supply replacement RGB.
The original game texels still control transparency, mask bits, and PS1 blending.
Changing PNG alpha cannot make originally transparent geometry opaque.

## Build and install

```powershell
python tools/edit_gt2_texture_pack.py build `
  --project work/my-textures `
  --output work/my-texture-pack
```

Copy the resulting `manifest.json` and `images/` directory into
`OpenGTPS1/mods/enhanced_textures_4x/`, then restart the game. Alternatively set
`OPENGT_TEXTURE_PACK_DIR` to the absolute pack directory before launch. One pack
directory is active at a time. Remove/rename it to restore original textures.
Build validation preserves edited pixels without resampling or an AI fidelity
gate. The runtime consumes ordinary uncompressed RGBA DDS images (format 7).

## Supported assets and limits

- Course TRP textures and car day/night detail maps use the native world renderer.
- Single-CLUT 4-bit/8-bit TIMs, including car logos, can be replaced in menu sprites
  and screen-space triangles. Menu entries explicitly set `screenSpace: true`.
  Upload relocation retains identity; overlapping writes invalidate it. Missing
  palettes and unsupported texture-window operations fall back to the original.
- Packed GT Mode menu documents, TIMs with external/multiple palettes, movies,
  live framebuffer effects, and the separate `TITLE_EXACT.DAT` unified title
  composition are not exported by this tool. They remain unchanged. A nominal
  title TIM can be exported without affecting that separate title composition.
- Menu replacement sampling is nearest within the individual image. The existing
  menu tiling fix remains active with and without a pack, including fades and FXAA
  off. No filtering crosses unrelated menu tiles.
- Menu packs have a 256 MiB decoded-image budget; native world packs must fit the
  renderer's 16384-pixel atlas. Large libraries should be split into smaller packs.

Keep extracted game art and generated packs out of the repository. Distribute
only artwork you have permission to distribute.

## Validation

`tools/test_edit_gt2_texture_pack.py` checks authored PNG round trips, car baseline
encoding, size/path validation, and edit preservation. `tools/menu-texture-tests`
checks upload relocation, palette matching, bounds, overwrite invalidation, and
malformed-pack fallback. `RECOMPONE_D3D_COMPOSITOR_SELF_TEST=1` also checks real GPU
replacement rendering across adjacent tiles at neutral and faded brightness.
