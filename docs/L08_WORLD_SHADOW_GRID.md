# L08 — primary-camera provenance and world-aligned shadow texels

This is cumulative over L07 and the supplied working checkout. Seattle daytime
and Red Rock dusk remain the only opt-in development profiles. The entire
`lighting/in-game-L05.shader` is unchanged. Night races, baked-shadow replacement,
and independently calibrated sun directions are not implemented by this pass.

## Original source, not a guessed world origin

The existing `TraceTrackTransformSetup` hook now retains six read-only source
values from the original primary course transform: the three negative camera
coordinates and the masked source sector origins. They are carried in immutable
`WorldObjectContext` provenance, not in a global last-camera cache. The selected
captured rotation and translation must reproduce the original full-width-IR GTE
MVMVA translation exactly before an anchor is published. Wrong scene pass, absent
normalization, unsupported exponent, invalid/truncated input and mismatched cached
transforms cannot publish an anchor.

Primary GTE vector order is source X/Z/Y. The hook records that explicit order;
it does not confuse a sector-local translation with the absolute course camera.
Source arithmetic `>>10` precision is retained. The managed regression executes
the actual GTE routine against varied signed offsets, sector origins and source
matrices, and checks the real live header writer.

A scenery instance may contribute more triangles than a primary sector, but
its object rotation is not the camera. The live selector now prefers a verified
primary origin with the SAME projection offsets, projection plane and scene
generation. It keeps the existing dominant choice if already verified, and
preserves its old fallback when no compatible verified choice exists. It never
imports another frame, mirror camera or a guessed matrix. Captured vertices,
projection instructions and visible geometry are not rewritten.

## Optional live version-6 extension

The live 160-byte header and 384-byte triangle records are unchanged in size.
Bit 4 authorizes a signed 16-bit primary normalization exponent at byte 114 and
three signed 32-bit negative source-camera coordinates at bytes 148/152/156.
Bit 3 still identifies primary coordinate axes. Existing bytes 116–147 retain
their original translation/projection/draw-offset meanings. Without the new flag,
reserved bytes are ignored and initialized to zero. The older standalone v5 file
recorder remains unchanged; it does not acquire this live extension implicitly.
Native validation also rejects singular/ill-conditioned camera matrices,
unsupported exponents and incorrectly quantized offsets. C bridge ABI unchanged.

## Exact shadow-coordinate transform

The primary camera matrix includes quantized shear and scale. Orthogonalizing
it is not the inverse of the transform that positioned the original vertices.
For common depth-normalized view coordinates, the primary map is
`A = R * 2^exponent / (4096 * 1024)`, with the explicit canonical X/-Z/Y column
conversion. Shadow projection uses the full inverse, including its handedness.
Material-control values and the existing shading model are not retuned.

World light axes become dual view-space rows through `axis^T * inverse(A)`.
The atlas center is snapped to an integer world texel. Large absolute source
positions cancel in CPU double precision BEFORE float shader constants are
uploaded; GPU positions remain camera-relative. Source vertices are not rounded.

The span is measured from complete, anchored source car geometry AFTER undoing
the primary camera transform. A context-owned scale lock prevents car selection,
normalization changes or camera shear from resizing the grid every frame. Only
a complete body can initialize the lock. Invalid metadata, scene changes,
backwards frame streams, disabled lighting and explicit renderer reset discard
it. Independent renderers never share scale/position state. There is no temporal
shadow image cache or invented off-camera geometry.

## Plane filtering and cascade coverage

The per-texel PCF receiver-plane gradient now uses a dual-basis plane transform.
The old dot-product shortcut was valid only for orthogonal shadow rows. The new
fixture checks isolated roads and independent casters through sheared cameras;
reverting the gradient reproduces false road self-shadowing.

A radial split alone does not guarantee that a point is covered by the near
square atlas. Coverage now blends onto the far map before a near-map filter
footprint exits, rather than blending a spurious fully-lit sample. A real GPU
fixture crosses that edge beneath one continuous occluder. It first confirms
the actual runtime profile has reloaded, so CPU fixture expectations and the
shader cannot accidentally use different sun directions.

## Boundaries of this repair

World-anchored texel phase does not undo original GTE/vertex quantization,
texture-painted lighting, billboard orientation or unloaded caster omissions.
Provisional suns and original baked shadows can still disagree. The physical
light-plane projection is corrected here; this is not a new physically exact
material model, a full-route guarantee or a complete time-of-day system.
The Linux GLSL path is built and exercised. Corresponding common CPU and HLSL
coverage changes are preserved, but Windows/D3D11 parity/execution is unverified.
Exact fresh tests, game runs and source/binary identities are in the accompanying
L08 report and validation directory, not inferred from older checkpoint results.
