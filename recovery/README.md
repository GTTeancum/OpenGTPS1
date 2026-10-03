# L18 recovery branch

This branch was reconstructed on 2026-10-03 from persistent ChatGPT Library artifacts.

## Recovery policy

- `main` is intentionally untouched.
- Exact L18 source bytes come from the original delivered L18 archive.
- The original L18 archive, newest retained canonical handoff, and later evidence are preserved in the multipart recovery bundle under `recovery/`.
- Post-L18 September 25-26 work that survives only in the canonical handoff is documented, not silently recreated as byte-exact source.
- No GT2 game data is committed.

The bootstrap commit stores the recovery bundle as Git blobs split into parts. The one-shot GitHub Actions workflow concatenates the parts, verifies SHA-256 `70f4bb6a8ccf2b032aa19815b0d069ed732e8c38bea04cb5de66bab79aa8f121`, extracts the exact L18 source archive, overlays its 400-file source tree onto this branch, and commits the recovered source.
