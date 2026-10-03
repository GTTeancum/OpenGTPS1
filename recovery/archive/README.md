# OpenGTPS1 L18 ChatGPT recovery bundle

Created 2026-10-03 from persistent ChatGPT Library artifacts.

## What is byte-exact

- `L18-source/OpenGT_Lighting_L18_World_Geometry_Unbake_Audit_Candidate.zip`
  is the exact delivered L18 source checkpoint.
- SHA-256 of that archive is recorded in `SHA256SUMS.txt` and the canonical handoff.
- Persistence/op-7 ZIPs and reports are the exact artifacts retained in the Library.

## What is documentation-only

Work continued after the exact L18 source archive, especially September 25-26
resident-model-space audit and Seattle evidence work. The newest canonical handoff
contains those results in detail, but a complete byte-for-byte September 26 source
tree was not retained as one archive. Do not silently claim reconstructed later
source is byte-identical.

## Game data

No copyrighted GT2 game payload is included here. Only persistence/prepared-asset
manifests are retained. The actual prepared game assets remain in the user's
persistent ChatGPT Library under `/OpenGT-Game-Assets`.

## Recovery workflow

The GitHub recovery branch contains a workflow that extracts this bundle, then
extracts the exact L18 source archive and overlays its `recovery/source/` tree onto
the branch root. Main-only release/docs files are intentionally left in place;
files present in L18 are replaced with the exact L18 bytes.
