# OpenGT L18 Persistent Assets / Runtime Test 06

Date: 2026-09-25

## Scope

Validate that the newly persisted Gran Turismo assets are actually recoverable and usable for L18 rendering, not merely present in Library metadata.

## Persistence rehydrate

All persisted files were retrieved into a fresh directory strictly through Library file IDs. The three large CloneCD images were rebuilt from their 256 MiB chunks and every chunk plus final image was checked against the stored SHA-256 manifests.

Result: **PASS — all 11 raw disc assets matched exactly.**

Authoritative image identities:

- GT2 Simulation: `d0ab6e70539601057590a36299543c0adad219254d712f7d4273219094ed5031`
- GT1: `765a748c4f2975a063a47ba9e42708a4882954d765f9e352c5af3c0950eaefb6`
- GT2 Arcade: `c2e97d6b0c847ca4336d9d84d8d98c349d1240ed075e81ab3fd5c977e9a45075`

## OpenGT data reconstruction

Fresh PS1 Mode-2 extraction from the rehydrated images reproduced the previous working Simulation and Arcade loose roots byte-for-byte, including raw 2336-byte XA payload files and disc metadata.

`tools/prepare_unified_install.py` then rebuilt the clean unified root. Every produced file matched the prior working L18 unified root byte-for-byte.

Key unified identities:

- `GT2.VOL`: 701,837,312 bytes — `7c3bf68061e5867de5af831121c50091128ddbde4f13026a050d3c71ef0eee53`
- `MUSIC.DAT`: 97,252,352 bytes — `2d1b7a30f656900213fa1f4aff9371c22db9d7f41ae8a9e5c5d51de8ffc9e102`
- `TITLE_EXACT.DAT`: 1,966,100 bytes — `735d838c3a0f12e2917593648790f9fd1cb6ada13d402e19022d7c814737321c`

The prepared root is now persisted at `/OpenGT-Game-Assets/prepared-unified-clean`. Unified `GT2.VOL` is stored as three ordered chunks because Library's per-file ceiling is 512 MiB.

## L18 smoke

Fresh runner smoke tests passed:

- Simulation replay codec exact round-trip;
- Arcade replay codec exact round-trip;
- car-preview camera verification;
- no residual generated host state after smoke.

## Seattle real-race proof

PASS:

- native race construction verified;
- 31 submitted / 31 rendered / 31 actual / 31 consumed native outputs;
- 0 synthetic, repeated, or dropped outputs;
- 0 raw-track decode failures;
- 0 raw-background decode failures;
- 0 guest-track fallbacks;
- 0 guest-background fallbacks;
- native captures at polls 357 and 365;
- clean poll-372 shutdown, exit 0.

## Red Rock real-race proof

PASS with the same invariants: 31/31 actual native outputs, zero decode failures/fallbacks/drops, two real captures, clean exit 0.

## Pixel regression against prior known-good L18 proof

Four fresh captures were compared against the earlier scale-1 L18 proof captures. All differences were confined to the dynamic analog gauge needle:

- Seattle 357: 352 changed pixels, 0 outside gauge region;
- Seattle 365: 360 changed pixels, 0 outside gauge region;
- Red Rock 357: 295 changed pixels, 0 outside gauge region;
- Red Rock 365: 356 changed pixels, 0 outside gauge region.

Therefore **every world / scenery / track / car / non-gauge pixel was bit-identical in all four proof frames**. No renderer regression was detected from persistence/reconstruction.

## Non-blocking packaging observation

Running archived-source `tools/release_setup.py` reached successful Simulation/Arcade/GT1 image validation, then stopped because the L18 source candidate archive omits `release/Setup-From-GT2-Discs.ps1`. The Git repository contains that resource and the renderer validation path uses `prepare_unified_install.py`; this is a release-package completeness issue, not a rendering failure and is parked under the anti-drift rule.

## Result

**PASS.** The persistent asset workflow is proven usable end-to-end, and the latest L18 rendering update remains healthy on Seattle and Red Rock using data regenerated solely from Library-persisted assets.
