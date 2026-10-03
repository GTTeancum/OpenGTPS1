# L05 — shadow contrast repair

L05 is cumulative over the actual L04 Linux game source. It changes the shadow
renderer and the opt-in two-track development preset, not game instructions,
cameras, timing, texture assets or the driving simulation.

The prior native renderer forced the original untextured, subtractive vehicle
contact geometry to 0.06 alpha. `contactShadowOpacity` now controls that source
footprint per selected track. The L05 development preset uses 0.48 for Seattle
and 0.50 for Red Rock (`sprint2`). A missing directive preserves 0.06; the finite
accepted range is 0 through 0.8. Invalid/duplicate directives reject the reload
transactionally. Only main-view physical vehicle footprints use the stronger
value, and only after a valid active solar-shadow frame is prepared. Unidentified
tracks, disabled shadows/lighting, night fallback, secondary views, screen-space
art and unrelated subtractive effects preserve the prior treatment. This is not
new blob geometry or an image-space darkening pass.

Projected `shadowStrength` is 0.8 on both tracks. This is a shading parameter,
not a claim that every output pixel becomes 80 percent darker. Shadow resolution
is 2048 per cascade rather than 1024. Sun directions, colors, intensity, ambient,
paint/reflection controls and the dry-road material are unchanged from L04.

## Receiver-plane filtering

The old PCF filter compared all displaced taps to the center receiver depth.
On a road oblique to the light, neighboring shadow texels have different depths
on that same road. The result can be partial self-shadow across a surface that
has no occluder. Simply increasing style strength also intensifies that defect.

L05 derives depth gradients in shadow UV coordinates from the rasterized
receiver's position. Its equivalent 3-by-3 bilinear PCF footprint is evaluated
as 16 weighted texels, with each depth comparison evaluated at that actual
texel center. This retains continuous filter movement without relying on a large
receiver offset to cover hardware bilinear comparisons. The small existing
normal-dependent bias, cascade blending, cutout sampling and far fade remain.

Both GLSL and the embedded HLSL implement the same receiver-plane comparison.
The editable HLSL was recovered from the existing embedded strings and kept
synchronized with `tools/sync_lighting_shaders.py`. Linux/EGL is the executed
path; Windows/HLSL runtime execution has not been tested in this checkpoint.

`OPENGT_SHADOW_DIAGNOSTIC=1` is a separate development-only grayscale view of
actual projected occlusion. Contact geometry is omitted in that view so it
cannot masquerade as projected shadows. It does not alter the normal preset.
`OPENGT_SHADOW_AUDIT=1` records selected shadow constants every 120 authored
frames. Neither variable is needed for normal output.

## Regressions and limits

`opengt_shadow_visibility_tests` tests actual EGL pixels, parser/reload safety,
main/secondary/UI isolation, and a receiver with an independent off-camera
caster. Twelve additional isolated-receiver cases span flat/sloping roads,
day/dusk/low-grazing light and 512/2048 maps; a receiver without a caster must not
produce false occlusion. Existing native and managed-host suites remain.

The full default catalogue is still separate. The two suns remain provisional
with measured confidence zero. This pass does not authorize baked-color gains,
remove baked shadow polygons, recover a calibrated sun, add full off-camera
residency or prove every location in a lap. Original baked shading stays intact.
Only the new report's executed sessions/test logs describe current validation;
prior L01–L04 reports remain historical, not extra passing tests.
