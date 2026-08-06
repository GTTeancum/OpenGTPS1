# GT1 content audit

## Authority

Gran Turismo 1 content is approved for conversion only when it is present in
the supplied US disc archives and can be compared directly with the supplied
US Gran Turismo 2 archives. Web lists, recollections, screenshots, and
hand-authored compatibility lists are not evidence for an import.

The reproducible audit is implemented by
`tools/gt1_gt2_vol_inventory.py`. Its current source set is:

| Source | Size | SHA-256 |
| --- | ---: | --- |
| GT1 `CARINF.DAT` | 135,301 | `0f98c1c75391adb064eceae901065bc38dcea9985fc62b3a0bc4a6ed9ce25f22` |
| GT1 `CAR.DAT` | 16,379,904 | `674f872d6a71a6e0d1d0fea08918970db2d903f2e25de80d4e0ded7c1ef83792` |
| GT2 Simulation `GT2.VOL` | 488,241,152 | `9630aad04cabf50ad702a3dbbce77069153748385bcffc02015797a0c708eedd` |
| GT2 Arcade `GT2.VOL` | 213,596,160 | `8d441307bc5dd11e36b8098f971bcbcfb4047fa3b7bcd8d291d0a5e9810e2f1f` |

The tool reads GT1's SPEC and COLOR records from `CARINF.DAT`, all day/night
model and texture members from `CAR.DAT`, and GT2's `.carinfoe`, `.carcolor`,
`.cclatain`, GT Mode parameter database, Arcade parameter database, and
`carobj` members from both volumes. It hashes the archive payloads and decoded
or converted model, bitmap, and palette data. A generated detailed JSON report
is kept under `work/gt1-vol-audit/` and is deliberately not a hand-maintained
source list.

Current database counts are 178 GT1 SPEC records, 344 GT1 graphic stems, 1,110
GT2 car-info records, 618 GT2 GT Mode car records, and 63 stock GT2 Arcade car
records. GT1 and GT2 share 302 `carobj` stems.

## Classification rules

Each candidate must be placed into one of these implementation classes before
conversion:

1. **Existing GT2 car with additional GT1 visual payloads.** Keep one
   purchasable/prize/garage identity. Extend that car's color/livery choices
   and preserve the GT1 body, indexed bitmap, and palettes needed by those
   choices. Do not create a duplicate customer-visible car.
2. **Different GT1 stem with physically and geometrically equivalent GT2
   target.** Fold the GT1 visual choices into the proven target car. Matching
   names are not sufficient; the GT1 physical fingerprint and authored model
   relationship must support the fold.
3. **Distinct car candidate.** Create a new GT2 car only after its GT1 SPEC
   record and visual members prove that it is not merely another color or
   livery of an existing GT2 car.
4. **Graphics-only record.** Do not expose it as a car until a consumed GT1
   specification or a proven GT2 body relationship establishes its role.

For the same car identity, a GT1 color ID absent from that GT2 entry is the
authoritative signal that the GT1 choice must be ported. This applies whether
the entry is a production, LM, race, or prize car. Texture, bitmap, palette,
and model comparisons determine whether the additional choice can be appended
to GT2's existing CDP or requires an alternate native body; they do not veto a
database-qualified color.

The current source databases produce 34 same-car folds containing 51 GT1-only
color IDs:

| Stem | GT1 IDs | GT2 IDs | IDs to port |
| --- | --- | --- | --- |
| `aminn` | 49, 52, 53, 98, 108, 112, 114, 116 | 49, 52, 54, 98, 104, 109, 112, 114, 116 | 53, 108 |
| `amisn` | 49, 53, 98, 101, 110, 113, 114, 115 | 49, 54, 98, 101, 109, 113, 114, 115 | 53, 110 |
| `as16n` | 53, 98 | 54, 98 | 53 |
| `d-phr` | 104, 117 | 52 | 104, 117 |
| `dvpgn` | 98, 113 | 52, 54, 98 | 113 |
| `dvprn` | 98, 113 | 54, 98 | 113 |
| `hcrxn` | 49, 54, 98, 108 | 49, 54, 98, 109 | 108 |
| `hpnen` | 49, 51, 52, 54, 98, 101, 109, 115 | 49, 50, 53, 54, 98, 100, 101, 108, 109, 113, 115 | 51, 52 |
| `hpnvn` | 49, 51, 52, 54, 98, 101 | 49, 50, 53, 54, 98, 100 | 51, 52, 101 |
| `ld7cn` | 52, 53, 54, 98, 101, 104, 109 | 52, 53, 54, 98, 101, 109 | 104 |
| `ld7vn` | 52, 53, 54, 98, 101, 104, 109 | 52, 53, 54, 98, 101, 109 | 104 |
| `mgnon` | 49, 52, 54, 98, 108, 113 | 49, 52, 54, 98, 109 | 108, 113 |
| `mgntn` | 49, 52, 54, 98, 108, 113 | 49, 52, 54, 98, 109 | 108, 113 |
| `mgoon` | 49, 52, 54, 98, 101, 113 | 49, 52, 54, 98, 101, 115 | 113 |
| `mgotn` | 49, 52, 54, 98, 101, 113 | 49, 52, 54, 98, 101, 115 | 113 |
| `mmgrn` | 49, 52, 54, 98, 101, 109, 113 | 49, 53, 54, 98, 101, 108, 113 | 52, 109 |
| `n180n` | 49, 53, 54, 98, 113 | 49, 53, 54, 98 | 113 |
| `nm32n` | 49, 52, 53, 54, 98, 111, 115 | 49, 52, 54, 55, 98, 109, 111, 115 | 53 |
| `nt32n` | 49, 50, 52, 53, 54, 98, 111, 115 | 49, 51, 52, 53, 54, 55, 98, 109, 111, 115 | 50 |
| `sipan` | 49, 52, 54, 98, 113 | 49, 52, 54, 101, 115 | 98, 113 |
| `sipbn` | 49, 52, 54, 112 | 49, 52, 54 | 112 |
| `sipcn` | 52, 54, 98, 113 | 52, 54, 101, 115 | 98, 113 |
| `sipdn` | 49, 52, 54, 112, 113 | 49, 52, 54, 115 | 112, 113 |
| `siprn` | 49, 104, 113 | 49, 104, 115 | 113 |
| `sipsn` | 49, 52, 98 | 49, 52, 101 | 98 |
| `siptn` | 49, 52, 98 | 49, 52, 101 | 98 |
| `sipwn` | 52, 54, 98, 113 | 52, 54, 101, 115 | 98, 113 |
| `sipzn` | 49, 52, 54, 98, 113 | 49, 52, 54, 101, 115 | 98, 113 |
| `tcegn` | 49, 52, 53, 54, 98, 109 | 49, 52, 54, 98, 107, 113 | 53, 109 |
| `tceln` | 49, 52, 53, 54, 98, 109 | 49, 52, 54, 98, 107, 113 | 53, 109 |
| `tlvon` | 49, 54, 98 | 49, 98 | 54 |
| `tsplr` | 108, 113 | 108 | 113 |
| `ttron` | 49, 54, 98 | 49, 98 | 54 |
| `v-rbr` | 99, 109 | 98, 108 | 99, 109 |

## Proven folds

The following five current Arcade imports have a GT1 physical fingerprint and
authored model relationship that point to an existing GT2 GT Mode car. Their
GT1 paints/liveries must be folded into the target rather than remain separate
customer-visible cars:

| GT1 stem and archive name | Existing GT2 target | GT1 embedded paint IDs |
| --- | --- | --- |
| `l-7cn` DB7 COUPE | `ld7cn` | 49, 101, 117 |
| `m-nnn` LANCER EvolutionlV GSR | `mlnnn` | 104, 112, 119 |
| `n-13n` S13 SILVIA Q's 1800cc | `nq13n` | 101, 104, 108 |
| `s-v4n` ALCYONE SVX S4 | `ssv4n` | 49, 115, 117 |
| `t-pnn` SUPRA RZ | `tspnn` | 103, 111, 119 |

Their existing separate Arcade entries are conversion prototypes, not the
final unified data model.

## Cerbera LM finding

The supplied archives confirm the overlooked Cerbera LM artwork:

- GT1 stem `v-rbr` is `Cerbera LM Edition` and contains two embedded visual
  choices with IDs 99 and 109.
- GT2 stem `v-rbr` is `[R]TVR Cerbera LM Edition` and contains two choices
  with IDs 98 and 108.
- Neither converted GT1 day palette matches either native GT2 day palette.
- The converted GT1 and native GT2 indexed body bitmaps are different.
- Only 4,875 of 57,344 decoded body-bitmap pixels (8.50%) retain the same
  four-bit index. Every GT2 pixel index maps to all 16 GT1 indices somewhere
  in the bitmap, ruling out a global palette/index permutation.
- The GT1 model converts successfully, but is not byte-identical to GT2's
  model.
- The Simulation and Arcade volumes contain the same GT2 `v-rbr` assets, so
  the missing GT1 artwork is not hiding on the other GT2 disc.

Therefore the GT1 `v-rbr` package is additional authored livery/body artwork
for the existing GT2 Cerbera LM. It must be exposed as extra livery choices
on that car. It must not overwrite GT2's artwork and must not become a second
Cerbera LM entry.

GT2's native CDP texture stores one shared indexed bitmap plus multiple
palettes. Since both the bitmap and model differ here, appending two palettes
to GT2's existing CDP would not reproduce the GT1 liveries. The integrated
implementation needs an alternate native body/texture asset selected by the
extended livery index while retaining one car identity and one saved-garage
record.

The converter now produces that data shape for all 35 entries in gated
`GTPATCH.LIVERY.ARCADE.VOL` and `GTPATCH.LIVERY.SIMULATION.VOL` layers:

- `v-rbr` remains the only customer-visible car identity; a hidden `v1rbr`
  car-info record gives GT2's original archive loader a native index for the
  alternate body without adding a dealership, prize, garage, or Arcade entry;
- its two original GT2 choices remain first and GT1 IDs 99 and 109 are
  appended as choices three and four;
- the exact converted GT1 day/night texture and model members are stored under
  hidden body stem `v1rbr`;
- `.gtlivery` version 3 contains 53 explicit mappings from each extended car
  color index and authoritative color ID to its hidden body and original GT1
  palette index, plus one identity disambiguator for the retail Castrol
  palette. For `v-rbr`, indices 2 and 3 map to `v1rbr` palettes 0 and 1; and
- the converted day and night models are 20,032 and 21,004 bytes,
  respectively, both within the original frontend's audited `0x6000`-byte
  native model slot.

The native resolver is now wired into both executables' frontend palette-index
record construction, color-ID fallback, race body/palette path, and
showroom/replay body/palette path. Resolving while GT2 still carries the
palette index is required when two body packages reuse one color ID. The
installer enables the layers atomically for Simulation and Arcade, requires
their 54-record tables to be byte-identical, and installs the validated table
as `GTLIVERY.BIN`. The fold layers remain excluded from the default unified
install until the complete 35-target interactive smoke matrix passes. This
gate prevents an older executable from treating extended color indices as
out-of-range palettes on each target's original body.

The representative alternate-body gate now passes for Cerbera LM in both
native executables. Clean regeneration reapplies the resolver and preview
reload hooks, and the standalone Simulation, standalone Arcade, and unified
projects all compile. Arcade presents one `Cerbera LM Edition` entry with the
two retail GT2 colors followed by GT1 grey/red and grey/green. A fifth color
input wraps to the first retail choice. Selecting grey/green resolves customer
body `v-rbr`, choice 3 to hidden loader body `v1rbr`, local palette 1, then
loads and auto-drives that native body through a normal race with no unmapped
call, managed exception, or software fault.

The transient GT Mode acquisition smoke pins its developer-only used-car alias
to authoritative color ID 109 so a fresh save deterministically exercises the
newest imported paint instead of depending on the dealer-rotation day. Its
listing, information preview, purchase/active-garage state, race, result, and
replay all complete, and the preview trace resolves `v-rbr` choice 3 to
`v1rbr` palette 1. The alias is intentionally non-shipping and lives in the
Mazda test roster; that borrowed dealer screen still shows Mazda's
`RX-7 A-Spec LM` information-page wordmark even though the listing, car,
specification, and garage identity are Cerbera LM. The current generic upgrade
fixture also enters the Tommy Kaira special-model page rather than proving a
Cerbera upgrade purchase. Those two smoke-scaffold presentation/navigation
issues remain open and are not counted as production GT Mode parity.

The same database rule imports `tsplr` ID 113 as the second Castrol Supra GT
choice and covers the other 48 variants listed above. Cross-stem archive
comparison additionally folds both `t-plr` IDs 108 and 113 into that same
identity. Each gated layer now contains 140 converted day/night car assets, 35
hidden loader-only car-info records, the extended customer-visible records,
`.carcolor`, and the 54-record `.gtlivery` table. Conversion fails unless every
target index and hidden-body palette resolves back to the same authoritative
color ID.

## Cross-stem identity review and Gran Turismo Mode scaffold

The mechanical archive comparison produces 16
`distinct-car-candidate` rows:

`a-ian`, `amian`, `h-rxn`, `h-vrn`, `hnslr`, `n-15r`, `n-32n`, `n-33n`,
`nl33r`, `s-pbn`, `t-eln`, `t-hvr`, `t-oan`, `t-plr`, `t-ron`, and `tceen`.

That label means only that the first audit found no same-stem GT2 GT Mode
record and no byte-exact physical-and-model fold. It does **not** establish a
unique customer-visible car. GT1 uses shorter Arcade aliases, separate
Arcade/Simulation specifications, and separate stock/Racing Modification body
stems. Cross-stem name, specification, body, color-ID, and acquisition
relationships must be checked before adding a new identity.

The supplied archives already establish these counterpart relationships:

| GT1 row | GT2 identity/body target | Archive evidence and required action |
| --- | --- | --- |
| `h-vrn` | `hcvrn` | Same `CIVIC (Racer)` name and 185 PS/1,050 kg specification. Fold the Arcade artwork and color IDs; do not retain a duplicate GT Mode car. |
| `t-oan` | `tsoan` | Same `SOARER 2.5GT-T VVT-i` name and 280 PS/1,560 kg specification. Fold. |
| `h-rxn` | `hcrxn` | Same `CIVIC CR-X '91 Si` name and byte-identical GT1 geometry; the Arcade record carries a deliberate weight difference and exclusive texture package. Preserve those assets without creating a second road-car identity. |
| `t-eln` | `tceln` | Same `CELICA SS-II` name and model peer. Treat its Arcade-tuned specification and artwork as variants of the existing identity pending year/trim confirmation. |
| `s-pbn` | `sipbn` | GT1/GT2 Version III Impreza counterpart already used as its native part/model basis. Fold the authored Arcade body and colors into that identity. |
| `n-32n` | `nr32n` | Exact `R32SKYLINE '91 GT-R` name and 280 PS/1,480 kg specification. Fold its IDs 101/104/113 and alternate body as required. |
| `t-ron` | `ttron` | Exact `AE86 SPRINTER TRUENO GT-APEX` name and 130 PS/925 kg specification. Fold GT1-only IDs 97/111 into the GT2 '85 identity. |
| `tceen` | `tcegn` | Exact `CELICA GT-FOUR` name, 255 PS/1,380 kg specification, paint set, and mutual model-peer relationship. This is an alias, not a new car. |
| `t-plr` | `tsplr` | Both GT1 records are `CASTROL SUPRA GT`, 665 PS/1,150 kg, with IDs 108/113, but carry different model and texture packages. GT2 has `tsplr` with ID 108 only. Fold every authored GT1 body/livery combination into one GT2 identity. |
| `nl33r` | `nl33n` Racing Modification | GT2 already contains `nl33r` car-info and day/night body assets with IDs 113/118; it is the Racing Modification of the Nismo GT-R LM Road Car, not another purchasable car. |

The following relationships are strong leads but still require a complete
archive linkage proof:

- `a-ian` and zero-price `amian` belong to the EUNOS Roadster family; their
  exact trim/acquisition targets must be resolved across `aminn`, `amisn`,
  `amivn`, `an16n`, `as16n`, and `av16n`.
- `n-33n` is the 305 PS/1,580 kg NISMO GT-R LM road/Arcade record and is
  expected to fold into GT2's `nl33n` road-car identity, while `nl33r`
  remains its Racing Modification.
- `t-hvr` is the 600 PS/1,260 kg CHASER LM Edition and is expected to become
  the authored Racing Modification body/livery of the Chaser Tourer V family,
  not a separate dealership entry.
- `hnslr` is the NSX-R LM GT2. Secondary catalog evidence identifies it as a
  planned GT2 Racing Modification/hidden car, but the supplied US GT2 volume
  has no same-stem car-info or body quartet. Its actual surviving target body
  must be located in the archive before folding.
- `n-15r` S14 SILVIA LM Edition has no final US GT2 identity or body quartet
  found so far and remains the clearest genuinely GT1-exclusive car candidate.

The Gran Turismo Wiki is used only as a secondary naming/history index. Its
[Castrol Supra page](https://gran-turismo.fandom.com/wiki/Toyota_Castrol_SUPRA_GT_%28JGTC%29_%2796)
identifies the Grand Valley prize as a black version and records the regular
and black presentations; the supplied archives, not that page, authorize the
conversion. The hard-data finding is that GT1 `t-plr` and `tsplr` have the
same two color IDs but different body bitmaps/models, whereas GT2 retains only
one body and ID 108. Therefore color-ID comparison alone is insufficient for
this cross-stem alternate-body case.

That representation is now implemented as one native four-palette body. GT2
`tsplr` remains the sole visible, purchasable, upgradeable identity. Its
converted day and night textures contain, in order, GT1 `tsplr` IDs 108 and
113 followed by GT1 `t-plr` IDs 108 and 113. The two GT1 source packages have
byte-identical indexed bitmaps, so their authored palette blocks can be
combined without resampling or synthesizing pixels. The GT1 `tsplr` ID 108
white/green presentation is visually identical to retail GT2 and replaces
that single slot rather than appearing twice. No hidden Supra body and no
runtime body swap are involved: GT2's original palette selector cycles all
four choices, and the car identity used by Arcade rosters, GT Mode ownership,
upgrades, saves, and races never changes.

The rebuilt native Arcade host completed the deterministic five-capture
selector smoke: all four choices rendered distinctly, and a fifth Down input
wrapped to white/green without an exception. The generated table retains four
identity/disambiguation records for the repeated IDs, so palette-index lookup
is exact while ambiguous ID-only lookup cannot select the wrong presentation.
The two patch layers regenerate byte-for-byte deterministically.

A transient, non-shipping GT Mode used-car alias then exercised the same
`tsplr` identity through native acquisition, active-garage state, Sunday Cup
race loading, AI auto-drive, results, and replay. The car finished second by
0.116 seconds; the run exited normally with no unmapped call, managed
exception, software fault, or memory-card mutation. The alias changes only its
test listing to 20,000 credits so the existing 100,000-credit deterministic
fixture can buy it; normal conversion preserves GT2's 1,000,000-credit record
and original acquisition path. GT2 correctly reports that this purpose-built
race car cannot be tuned at Mazda. That rejection is the native applicability
rule for the existing identity, not a missing cloned part family.

The accepted visual set contains four presentations: white/green, white/blue,
black/green, and black/blue. All four are native palette choices under
`tsplr`; database color-ID deduplication is not permitted to discard any of
them. The retail GT2 and GT1 `tsplr` white/green source records render
identically, so they collapse to one customer-facing choice. Their separate
archive hashes remain recorded as provenance.

Six rows have a consumed GT1 SPEC record, complete day/night assets, nonzero
archive price, and already-proven native Arcade body/race conversion. They are
currently generated as a gated **standalone GT Mode conversion scaffold** in
`GTPATCH.GT1CARS.SIMULATION.VOL`:

| GT1 stem | Archive identity | GT2 price | Authoritative color IDs | Native manufacturer |
| --- | --- | ---: | --- | --- |
| `a-ian` | EUNOS ROADSTER | 20,000 | 104, 105, 114 | Mazda |
| `h-vrn` | CIVIC (Racer) | 20,000 | 104, 111, 118 | Honda |
| `s-pbn` | IMPREZA Sedan WRX-STi version III | 20,000 | 103, 104, 111 | Subaru |
| `t-oan` | SOARER 2.5GT-T VVT-i | 20,000 | 101, 104, 117 | Toyota |
| `t-eln` | CELICA SS-II | 20,000 | 104, 112, 119 | Toyota |
| `h-rxn` | CIVIC CR-X '91 Si | 20,000 | 54, 104, 113 | Honda |

The scaffold proves that the converted bodies, wordmarks, colors, database
records, dealer acquisition, garage ownership, upgrades, and races can pass
through native GT2 systems. It is not the final identity model: the counterpart
rows above must be folded into existing cars, and the temporary 20,000-credit
standalone entries removed. `amian` remains excluded because its supplied
record has zero archive price and no proven prize/acquisition path.

For each scaffold car, the converter:

- clones every owned record in all 24 GT Mode part blocks, changes only the
  owner ID or archive-proven visual/body reference, re-sorts the blocks, and
  rewrites every original and imported absolute part reference;
- adds stock and Racing Modification day/night CDP/CNP/CDO/CNO members;
- adds the stock and Racing Modification wordmarks to every GT2 GT Mode
  locale/context archive using the exact supplied US GT1 4-bit TIM pixels and
  palette, with only native VRAM placement normalized;
- updates all seven localized GT Mode database/string-database pairs;
- writes all 60 complete dealer rotations in `.usedcar`, `.usedcar_jpn`, and
  `.usedcar_usa`, under the correct manufacturer, price-sorted, at 20,000
  credits, cycling the three archive color IDs without renumbering them; and
- keeps the Simulation car patch separate from Arcade and same-car livery
  layers so the installer can validate and apply the native data sets in an
  explicit order.

The used-car format was derived from the archive, not guessed from a web list:
gzip payload `UCAR`, 60 rotations, 39 manufacturer descriptors per rotation,
and eight-byte records containing `CarId`, 16-bit price, flags, and `ColorId`.
Normal output contains exactly 60 occurrences of each imported car in each
regional roster. The converter rejects a wrong count, manufacturer, price,
color ID, block order, unresolved reference, missing body, or missing logo.
An explicit `--smoke-gtmode-car` option may place one target in Mazda row one
to reuse a deterministic controller fixture; that alias exists only in smoke
output and is never emitted by normal conversion.

Runtime proof currently stands at:

| Car | GT Mode detail/purchase/garage | GT Mode upgrade/race | Prior Arcade full race |
| --- | --- | --- | --- |
| `a-ian` | Pass | Pass: Turbo Stage 1, two-lap race, result and replay | Pass |
| `h-vrn` | Pass | Pending dedicated GT Mode race | Pass |
| `s-pbn` | Pass | Pending dedicated GT Mode race | Pass |
| `t-oan` | Pass | Pending dedicated GT Mode race | Pass |
| `t-eln` | Pass | Pending dedicated GT Mode race | Pass |
| `h-rxn` | Pass | Pending dedicated GT Mode race | Pass |

The Roadster proof uses the original dealer, garage, upgrade, event, physics,
AI racing-line, result, and replay code. It completed cleanly with no unmapped
call, managed exception, software fault, or persistent test-save mutation.
The other five completed purchase proofs show their exact GT1 wordmarks,
archive-derived specs and 20,000-credit price, then the purchased identity as
the active garage car. Each uses a transient test save and restores the
original memory-card bytes after capture.

The 24 populated GT2 part blocks are functional target-owned clones of selected
GT2 basis cars. They are not yet a record-for-record conversion of GT1's 13
`EQUIP` databases and 24 authored part databases in `CARINF.DAT`. Before
release, every final folded or genuinely distinct identity must receive its
archive-derived acquisition, upgrade applicability, event eligibility/class,
results, save, and reload matrix. Arcade success and a standalone frontend
purchase alone do not satisfy that gate.

## Native car and livery viewer

`tools/car_livery_viewer.py` removes menu navigation from the visual-audit
loop. It reads the materialized native GT2 volume and `.gtlivery` table,
decodes LOD0 CDO geometry plus the CDP four-bit indexed bitmap and all CLUT
subpalettes, and writes a self-contained WebGL viewer:

```powershell
python tools/car_livery_viewer.py --stem tsplr
```

The default output is `artifacts/car-livery-viewer/index.html`. It includes
body/palette switching, turntable and pitch controls, source hashes, resolver
status, downloadable three-quarter/profile PNGs, a profile comparison sheet,
and a machine-readable `viewer_manifest.json`. The deterministic offline PNG
renderer uses the same corrected native quad order as the WebGL path, so proof
images do not depend on browser automation. This is an audit accelerator, not
a replacement for the required Arcade and Gran Turismo Mode race, save, and
reload smokes.

The default viewer contains only customer-facing choices. Pass
`--include-source-duplicates` to retain visually duplicate raw archive records
for hash/provenance review without adding them to the game-facing inventory.

## Required validation

Every converted car or folded livery must pass:

- deterministic source hash and record-fingerprint checks;
- native day/night model and texture conversion checks;
- Arcade selection, livery cycling, race load, auto-drive, and result flow;
- Gran Turismo Mode acquisition, garage display, upgrade purchase and install,
  eligible race load, auto-drive, result flow, save, and reload;
- screenshot capture at selection and on track; and
- no unmapped call, managed exception, software fault, or archive overflow.
