# L06 — Linux shadow coverage and depth repair

This changes shadow geometry handling and depth precision, not the approved L05
art direction. `lighting/in-game-L05.shader` is intentionally unchanged.

## Source car geometry

`GpuVehicleShadows.cs` reads the original selected LOD at the existing
`GT2Compat.CaptureVehicleDepthNormalization` hook. It does not modify guest RAM,
select a different LOD, or replace game-generated instructions. Original body
stream strides are 16, 16, 24, 28, 28, 28, 36 and 40 bytes. Source quad vertices
A/B/C/D are emitted by the original game as A/B/D/C; the two triangles are A/B/D
and B/D/C. Texture coordinates receive the same permutation. Original page,
CLUT adjustment, texture window and keyed transparency are retained.

The bounded OGTCAR01 message is separate from the course-mesh format. A current
main-view body must supply at least two matching source faces, including their
UVs, source material and exact active transform/depth normalization. Only then
are missing source faces added to the shadow-only stream. Camera-visible color
geometry is not replaced. These extra faces are excluded from reflection probes
so this repair does not revise the approved reflection capture geometry.

Definitions are scene-generation scoped and bounded to the current/nearby prior
input polls. Future, stale or unanchored source data is not assigned an invented
pose. In particular, a car wholly absent from the visible frame is not a newly
supported off-camera caster. Complete unloaded-course residency is also not
provided by this change.

## Contact footprint coverage

The original contact-shadow strips are resolved once per covered pixel using a
stencil union after opaque scene depth, before other effects/HUD. At 0.5 opacity,
two overlapping strip triangles must not turn into 0.75 opacity. Foreground
geometry still occludes the footprint, and coverage is cleared each frame and
on target recreation. Only the active main-view contact path changes; disabled
lighting, secondary views and unrelated subtractive effects retain their paths.

## Depth and receiver planes

The EGL backend retains floating-point reverse depth and uses zero-to-one clip
control when core/extension support is actually present. The GL 4.3 fallback
preserves the depth numerator through interpolation and writes fragment depth;
it does not rely on a function pointer as proof of extension support. Set
`OPENGT_GL_DEPTH_FALLBACK=1` to exercise that fallback explicitly in regressions.

PCF receiver depth uses the unsmoothed geometric plane, not finite screen-quad
derivatives or a fixed gradient clamp. Paint lighting still uses the existing
smoothed normal. This does not make low-resolution shadow maps or original
low-poly meshes infinitely smooth; it also does not remove original baked art.

## Reproduction

From the source checkout, with its original embedded compile resources:

```
python tools/build_linux_lighting.py --sdk /path/to/extracted/dotnet-sdk \
  --feed /path/to/nuget-feed --window-tests
```

`--window-tests` requires X11 (Xvfb works). The helper builds the full Linux host,
runs native CTest, the managed window/source-upload regressions, and the managed
vehicle-source decoder regression. It does not launch a race by itself. The SDK,
NuGet feed, game assets, and original embedded font files are external inputs;
none is included or downloaded by this recovery overlay.

Native test additions are `opengt_vehicle_shadow_source_tests`,
`opengt_depth_precision_tests`, and `opengt_depth_precision_fallback_tests`.
Contact coverage regressions extend `opengt_shadow_visibility_tests`. The managed
`tools/VehicleShadowRegression` exercises all eight stream layouts, the source
quad permutation, palette/UV handling, range rejection, and unchanged RAM.

The shader and graphics repair described here is implemented and tested on
Linux/EGL. A future Windows integration still requires a D3D11 equivalent and
its own execution checks; this is not a tested Windows update.
