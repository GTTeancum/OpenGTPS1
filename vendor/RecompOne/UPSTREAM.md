# Vendored RecompOne provenance

This source snapshot was copied from:

```text
C:\Programming\GitHub\Vigilante-8-recomp\tools\recompone-reference
```

Reference repository commit:

```text
8460f5bb18aad47c8a2cad39918bb723abae97b1
```

Latest source-lane commit at the time of copying:

```text
56845ed Catch up V8 runtime and tooling fixes
```

Local GT2-specific recompiler extension:

- `OverlayConfig.gzip` allows an overlay slice stored as a gzip member to be
  decompressed before MIPS analysis. GT2's `GT2.OVL` is a six-member indexed
  gzip container.
- Generated entry points fall back to the `SYSTEM.CNF` stack when a valid
  executable, such as GT2, leaves the PS-X EXE initial-SP field zero.

The upstream MIT license is retained in `LICENSE`.
