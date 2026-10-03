# L07 — track shadow stability on the Linux renderer

The L05 two-track lighting preset remains byte-identical. These repairs do not
change exposure, color, opacity, sun direction, texture content or visible LOD.

## Source-material shadow casters

Resident course decoding now generates a separate sun-only stream from each
selected primitive's original near material. Camera near/far selection continues
unchanged for visible geometry and the existing eye-rejected reflection stream.
The sun-only stream retains both original quad halves, source UV/page/CLUT,
texture windows, source transforms and depth normalization. Semi-transparent
surfaces are excluded; the existing typed overlay exclusion still applies.

The internal `world_primitive_sun_caster_replaced_flag` suppresses only duplicate
sun submission of the corresponding eye-LOD primitive. The additional faces carry
`world_primitive_shadow_only_flag`, so they do not enter color or reflection
passes. Source conversion must complete before replacement is committed. Failure
clears suppression and retains old casters. There is no global forced-high-LOD
setting, new texture-ID matcher or guessed off-camera pose.

## Projection corrections

Sun-plane axes are defined in the script/world basis and transformed into the
camera, rather than chosen from camera up. This removes camera-tilt/roll changes
and the view-dependent fallback-axis flip. Cascade dimensions prefer the complete
visibly anchored selected car body's model-space extent, accounting for the
original transform/depth normalization. Rotating a camera-space partial bounding
box no longer resizes the entire track shadow map. Captures without verified
source-body completion retain the existing fallback.

The receiver-centered XY atlas retains its configured coverage. Its depth range
now also includes resident casters whose sun projection intersects that coverage,
with a bounded-by-source geometric margin. Tall/upstream casters are not clipped
merely because they sit outside the old receiver-centered Z slab. GLSL and the
corresponding HLSL bias expression preserve scene-space bias when depth expands;
this is not an opacity or large-bias workaround. The shader file and embedded
HLSL string are synchronized. HLSL execution is not validated by Linux tests.

## Regression coverage

`opengt_shadow_projection_tests` tests source-complete sizing, camera-oriented
basis invariance, unanchored-source fallback, upstream depth coverage, preserved
bias and actual GPU pixels for an off-camera/upstream occluder. These assertions
fail against the L06 projection implementation. `opengt_track_shadow_material_tests`
executes the production resident decoder and transactional source commit across
primary/auxiliary layouts, triangle/quad faces, both windings and both eye LODs.
It also checks invalid depth-source fallback and exclusion of transparent effects.
Reverting sun material selection to the camera-selected material fails this test.

The continuation build entry point remains `tools/build_linux_lighting.py` with
the user's existing SDK/feed and original embedded compile resources. This is
not a request to rebuild or deploy on Windows.

## Limits

This is not a finished all-track relighting release. Both selected suns remain
provisional. No baked-shadow unbaking/removal rule was enabled. The existing source
PVS/resident selection remains; completely unloaded or unanchored off-camera
objects are not reconstructed. Full world-translation texel snapping, every
billboard case, all-route testing, Windows graphics parity and hardware-GPU
performance remain unverified. Low-polygon outlines and baked artwork can still
have angular or conflicting boundaries.
