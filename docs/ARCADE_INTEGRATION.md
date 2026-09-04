# Native Arcade Mode Integration

## Product requirement

Arcade Mode must execute the original `SCUS_944.55` program and its six
overlays through the same native runtime and renderer as Simulation Mode.
Replacing its menus, movies, race presentation, or other visuals with a host
approximation is not an acceptable final implementation.

The completed unified release must also provide a blocking first-run
preparation tool. It may be integrated into `GranTurismo2PC.exe` or shipped as
a separate installer executable. The tool requests the user's US Gran Turismo
2 Simulation Disc (`SCUS-94488`), US Gran Turismo 2 Arcade Mode Disc
(`SCUS-94455`), and US Gran Turismo disc (`SCUS-94194`), validates the three
source images, extracts and converts the required data, imports the approved
Gran Turismo 1-exclusive content, and builds the unified `GT2.VOL` and loose
installation.

`GranTurismo2PC.exe` must validate that prepared output and refuse to present
the original title menu if it is incomplete, missing, or damaged. The source
images remain read-only and are not required for later launches once the
generated installation passes validation.

## Loaded-card progress across the unified handoff

The direct Simulation-title to Arcade-overlay-2 path skips Arcade's disc-title
card load. Arcade also clears its BSS and constructs new-game defaults. Preserve
the already-loaded Simulation progress before switching executables and restore
it after Arcade's initializer, before choosing its first overlay. The shared
NTSC-U record ranges are `+0xB8` (0x160 bytes of Arcade progress) and `+0x1418`
(60 license records, stride 0xA4), relative to Simulation `0x801C98E0` and Arcade
`0x801C9340`. Do not force course-table gates open: incomplete saves must retain
their native locks. Restoration is one-shot, so later Arcade results are not
overwritten by a stale snapshot.

SSR11 availability separately requires the converted data set. Deploy
`GT2.VOL`, `arcade/GT2.OVL`, `arcade/DISC_META.DAT`, and `manifests/arcade.json`
together from the prepared unified install. Replacing that set with stock Arcade
data removes SSR11 even when the EXE and memory card are unchanged.

`tests/fixtures/unified-arcade-course-access.input` selects SSR11 (road-list item
4) and SSR5 (item 18) through the ordinary loaded-card path. Run without the
diagnostic unlock override. Public-release builds strip fixture controls;
the local single-file test build retains them for native screenshot evidence.

## Disc findings

- The Arcade `GT2.VOL` contains 10,618 named entries.
- Every Arcade entry name is present in the Simulation `GT2.VOL`.
- The Simulation volume adds 960 names and is therefore a strict name
  superset.
- Eighty-six shared entries differ in payload, including the Arcade race text
  database, panel art, car data, and course data. Thirty-nine of those also
  differ in stored size. Substituting the Simulation payloads is not
  compatible: it corrupts Arcade HUD and results text even though the menus
  still boot.
- The raw `MUSIC.DAT` sector data is byte-identical between the two discs.

The unified install therefore keeps both original GTFS members byte-for-byte
inside one physical `GT2.VOL`. Simulation reads member 0; Arcade reads the
second member at byte offset `488241152`. Each guest still sees its original
disc-visible `GT2.VOL` at LBA 473. The Arcade executable, overlays, raw
`STREAM.DAT`, and disc metadata also remain the original Arcade versions.

## Implemented foundation

- `prepare_arcade_reference.py` recompiles the Arcade executable and all six
  Arcade overlays.
- Simulation and Arcade output use separate generated namespaces, allowing
  both original codebases to coexist in one managed assembly.
- The Arcade compatibility addresses are mapped from Simulation using unique,
  normalized MIPS instruction fingerprints rather than assumed address deltas.
- Loose manifests can map a disc-visible file to a bounded byte range inside
  one safe source file. This is how both native GTFS members share one
  generated `GT2.VOL` without exposing either disc's incompatible payloads to
  the other executable.
- Loose replacement files may be larger when opened by name without claiming
  LBAs belonging to later files in the original disc layout.
- `prepare_unified_install.py` performs the deterministic two-member volume
  merge and writes both bounded manifests.
- One unified install shares the merged `GT2.VOL` and `MUSIC.DAT`, with
  mode-specific bootstrap and streaming files under `simulation/` and
  `arcade/`.
- The unified title uses the complete retail `arcade/arc_topmenu_usa` payload,
  SHA-256
  `128556787BBA8AB8317B8BE47D11DC8670C1503B79668CB3AF8BDF4B69EB564F`.
  The payload is byte-identical in both US retail GT2 volumes and contains the
  four Sony-authored 512x120 background strips plus selected and unselected
  sprites for `Arcade Mode`, `Gran Turismo`, `Replay Theater`, and `Option`.
  The installer decodes those native 15-bit TIM resources into four complete
  512x480 selection states. Each state retains the original 140x28 label
  rectangles at their authored coordinates. There is no resampling, palette
  reduction, font recreation, synthesized artwork, or substituted pixel.
- The original guest selector now drives the asset as its authored 2x2 menu.
  The Simulation vertical-list boundary triangles are disabled only for this
  title list. The runtime presents the exact 512x480 BGR555 state after GT2's
  title draw, while the original guest list continues to own input, selection,
  animation timing, and dispatch. Captures of all four states compare against
  the archive-derived frames with zero differing pixels. Selecting Arcade
  performs an in-process frontend handoff. It does
  not invoke the Arcade executable's top-level boot, legal/title sequence, or
  title overlay. The host recreates the exact Arcade prologue state, runs its
  native service initializer, and enters the post-bootstrap function at
  `0x8005D650`; the unified overlay selector requests overlay 2, which is the
  native `ARCADE MODE` menu reached after `START GAME` on the stock US Arcade
  disc. The redundant Arcade-disc title (overlay 1) is never shown.
- Arcade selection switches before Simulation's title state machine can
  dispatch Gran Turismo overlay 4. To retain the original confirmation sound,
  the host records the SPU key-on generation before effect 3, services the
  original sequencer until its voices start, waits for those voices to finish,
  and drains 6144 additional mixer frames for the queued audio and tail. It
  holds the selected title without executing another Simulation overlay-1
  update, so the sound finishes before guest replacement.
- `0x80018574` and `0x800186D0` are explicit Simulation overlay-1 function
  roots. They are original callback entries required by the retail Option path
  but were not discovered by the previous linear sweep.
- The unified renderer holds the already-selected Arcade title panel across
  guest-image and display initialization, then releases it only after Arcade
  overlay 2 enters its native presentation loop. Its original menu reveal
  animation remains intact after this boundary. The player therefore sees
  a direct menu-to-menu transition rather than the intermediate disc-reset
  states. Overlay 2's original Back result normally requests Arcade overlay 1;
  the unified hook maps that exact result to a post-bootstrap Simulation-title
  handoff, so Back returns to the unified title without exposing the skipped
  Arcade title or replaying either disc's legal/opening sequence.
- `Triangle` is the unified root-level Back operation in both modes. Arcade
  uses overlay 2's authored Back branch. Gran Turismo's disc-era world-map root
  has no native parent, so the PC hook recognizes Triangle only while that
  world-map controller is idle and returns to the post-bootstrap unified title;
  nested dealerships, garages, and modal menus retain their original Back
  behavior.
- Arcade overlay 2 resolves each course list against the data actually loaded.
  A release install made from the two supported GT2 discs uses the five stock
  tables; a converted GT1-content overlay uses its five expanded tables. The
  previous unconditional expanded-table redirect read beyond the stock
  overlay into zero-filled RAM, producing a blank `1.` course, `0 ft`, and no
  usable course navigation. The memory-level regression now covers both data
  layouts, and an invalid layout fails closed instead of drawing a corrupt
  menu.
- One `GranTurismo2PC` host assembly contains both original programs and all
  twelve overlays. There are no public mode-selection switches or substitute
  host menus.

`tools/test_unified_modes.ps1` selects all four entries through the original
guest title selector and verifies Arcade Mode, Gran Turismo, Replay Theater,
and Option from the same executable and shared loose install without an
unmapped call or managed exception. Its Arcade cases require the `0x8005D650`
handoff directly into overlay 2, reject overlays 1 and 5, and exercise the
reverse Triangle handoff into Simulation overlay 1. They also require Arcade
Single Player to reach the exact native Game Selection framebuffer rather than
the Simulation garage, and require Gran Turismo's world-map root to return to
the same unified title through Triangle. Accepted visual captures for
all four selection states and the packaged menu are under
`artifacts/retail-arc-topmenu-usa/exact-live`. The
`tools/verify_unified_title_exact.py` validator compares those live P6 frames
to `TITLE_EXACT.DAT` in BGR555 space and rejects any differing pixel. The
title-to-Route-11 race proof is under
`artifacts/retail-arc-topmenu-usa-ssr11-final`.

## September 2026 handoff latency investigation

The unified path now omits `func_80010CEC`, the two timed Arcade boot panels
called from `func_80010E14`. They were hidden behind the unified title but
still consumed 310 display ticks. Standalone Arcade boot retains them; the
surrounding native GPU, audio, filesystem, and frontend setup is unchanged.
The title also buffers early input across its original 16-update initialization
instead of losing the player's first press or forcing the native state machine.

The measured no-audio diagnostic handoff entered the frontend in 752.538 ms
and 15 input polls, versus 327 polls before this change. This measures entry
to native presentation, not completion of the original reveal animation.
Audio-enabled handoff additionally waits for the real confirmation voice and
queued tail; dummy-audio wall time is not a physical-device latency benchmark.

Opening the prepared Arcade disc view took 2.746 ms in that run. It indexes
the manifest and reads bounded data lazily; it does not load another entire
volume into memory. Both programs are already in the host assembly and both
original volumes already share the physical file. A small Arcade patch volume
would not remove the measured boot-panel wait, so no speculative volume or
CD-sector scheduling change was introduced.

## Native Gran Turismo 1 content layer

`tools/gt1_convert.py` validates the supported US Gran Turismo image, extracts
Special Stage Route 11, and writes a deterministic `GTPATCH.VOL`. The patch is
then materialized into Arcade's native `GT2.VOL`; it is not a host-rendered
replacement or a runtime donor-course switch.

Arcade remains the quickest native body/race conversion harness, but it is not
the release data model by itself. The converter now also emits the gated
`GTPATCH.GT1CARS.SIMULATION.VOL` layer. It contains six archive-proven
distinct GT1 identities with native GT Mode car, part, color, used-car,
day/night body, Racing Modification body, and localized wordmark records.
Every imported identity is therefore acquired and owned through the original
Gran Turismo Mode data path rather than a wrapper switch or Arcade-only alias.
Archive-derived identity classification and livery-fold evidence are recorded
in `docs/GT1_CONTENT_AUDIT.md`.

The installer accepts independent ordered Simulation and Arcade patch-volume
lists and computes the second native volume's source offset from the
materialized Simulation volume size. This is required because GT Mode and
Arcade databases are not interchangeable. Same-car livery folds are generated
separately as gated
`GTPATCH.LIVERY.ARCADE.VOL`/`GTPATCH.LIVERY.SIMULATION.VOL` layers. Its
cross-mode native alternate-body resolver is implemented in the frontend,
race, showroom, and replay paths; default activation remains gated on
rebuilt-host interactive smoke coverage.

The current conversion includes:

- a process-local renderer-certification launch inventory covering all 30
  stock forward courses, all 22 stock Arcade reverse layouts, and converted
  Route 11 in both directions. Dirt and reverse identities are read from the
  native `.crsinfo` data and enter the same Arcade race constructor as the
  tarmac forward layouts; the harness does not synthesize course geometry or
  use host operating-system input;

- forward, reverse, Arcade forward, Arcade reverse, two-player, and HiFi
  Route 11 course objects and maps;
- the GT1-authored `dawn3` background converted to GT2's native BSO/BSP
  layout;
- the exact 120x88 GT1 course-selection route art from `ARCADE.DAT` entry 81,
  placed without scaling or synthesis in GT2's native compressed preview;
- native `arcade/course_map`, `arcade/course_mapinfo`, and `.crsinfo` merges;
- sorted `.crsinfo` insertion for all six new `crsobj` pairs, preserving the
  one-to-one index relationship used by GT2's race loader;
- extended null-terminated Arcade forward, reverse, time-trial, and
  two-player course tables in `GT2.OVL`;
- the GT1 Arcade-only `a-ian` EUNOS ROADSTER model/texture family as a native
  tenth Class C entry, with the exact GT1 wordmark and all three authored
  olive/gold, violet-blue, and bright-yellow paint/livery palettes;
- the separate `amian` EUNOS ROADSTER ARCADE family as the eleventh Class C
  entry, retaining all fourteen authored GT1 palettes. Its complete 130 PS,
  990 kg physical basis resolves to GT2's native S-Special conversion, whose
  front and rear tire records already use the Arcade car's exact widened
  15-inch size-table entry;
- the `a-odn` EUNOS ROADSTER RS as the twelfth Class C entry, with its
  dedicated GT1 `ROADSTER RS` wordmark, all six silver, black, red, gold,
  deep-blue, and white palettes, and the exact 145 PS / 1030 kg `arodn`
  production specification;
- a deterministic GT-CAR-to-CDO/CNO converter for the RS's separate day and
  night bodies. It preserves all three authored LODs, 418 vertices, 624 day
  normals, 851 night normals, 426 faces, UVs, render ordering, wheel placement,
  and shadow data while using the corresponding native GT2 Roadster header
  conventions. It does not copy or substitute GT2 model geometry;
- the GT1 `h-vrn` CIVIC (Racer) as a native tenth Class B entry, including its
  unique converted hatchback body, original Honda/Civic selection artwork,
  and all three turquoise, hot-pink, and yellow liveries. The converter also
  preserves the model's authored `0x080808` untextured face colors instead of
  flattening them to black;
- the GT1 `l-7cn` DB7 COUPE as a native ninth Class A entry, including the
  exact Aston Martin DB7 Coupe selection artwork and all three authored white,
  burgundy, and deep-purple paints. GT1's Arcade DB7 body is byte-identical to
  its production DB7 body, so the converter uses GT2's native DB7 CDO/CNO
  container without substituting unrelated geometry;
- the GT1 `s-pbn` IMPREZA Sedan WRX-STi version III as a native tenth Class A
  entry, including the exact Subaru/Impreza WRX selection artwork, all three
  authored liveries, structurally converted day/night models, and its
  target-owned 1,220 kg chassis record;
- the GT1 `t-oan` SOARER 2.5GT-T VVT-i as a native eleventh Class A entry,
  including the exact Toyota/Soarer selection artwork, structurally converted
  day/night models, and all three authored wine-red, yellow, and purple
  palettes;
- the GT1 `t-pnn` SUPRA RZ as a native twelfth Class A entry, including the
  exact Toyota/Supra selection artwork and all three authored turquoise,
  purple, and bronze liveries. Its body is reused only because GT1 proves the
  complete model pair is byte-identical to the production Supra RZ;
- the GT1 `n-13n` S13 SILVIA Q's 1800cc as a native thirteenth Class C entry,
  including the exact named `n-13.tim` GT1 menu artwork and all three authored
  wine-red, yellow, and green palettes. Its production Q's model and physical
  records are reused only after byte-exact GT1 comparisons;
- sorted insertion into both native `CarArcadeRacing` and `CarArcadeDrift`
  tables, plus target-owned visual/body records and stock-compatible sharing
  of byte-identical physical part records;
- physics assembled from GT2's native conversions of the exact GT1 component
  signatures: V-Special chassis/suspension, S-Special wheel/tire package, and
  the Mazda brake signature shared by GT1's RX-7 Type-R family. The converter
  validates the complete GT1 SPEC difference fingerprint before producing
  those records.

Route 11 is inserted after each table's always-available prefix. This preserves
GT2's original progression ordering while making the imported course
immediately selectable. The deterministic
`tests/fixtures/unified-arcade-ssr11-race.input` smoke enters Arcade Mode
through the unified title and direct `ARCADE MODE` destination, selects the
fourth course, starts Route 11, engages the original AI racing-line controller
for the player car, and captures live race frames. The current handoff contract
requires overlay 2, rejects the skipped title overlays 1 and 5, and then
requires the native Route 11 course menu and live race to run through the
9,000-poll bound without an unmapped call, managed exception, or software
fault.

`tests/fixtures/unified-arcade-roadster-race.input` selects the appended
Roadster through the same native frontend, verifies the first livery in car
selection, starts Tahiti Road, and enables the original AI racing-line
controller. The corrected cumulative smoke completes lap one in 1:47.598 and
enters lap two in second place. Separate menu captures
verify all three GT1 palettes and the ten-entry Class C wrap. The run has no
unmapped call, managed exception, or software fault.

`tests/fixtures/unified-arcade-roadster-arcade-probe.input` captures all
fourteen `amian` palettes and verifies the eleven-entry wrap.
`tests/fixtures/unified-arcade-roadster-arcade-race.input` then runs the second
Roadster through the same Tahiti Road smoke: it completes lap one in 1:47.748
and enters lap two in first place with no runtime fault.

`tests/fixtures/unified-arcade-roadster-rs-probe.input` captures all six
ROADSTER RS palettes and verifies the twelve-entry Class C wrap.
`tests/fixtures/unified-arcade-roadster-rs-race.input` then loads the converted
day body on Tahiti Road: auto-drive completes lap one in first at 1:48.673 and
remains first on lap two. The separate
`tests/fixtures/unified-arcade-roadster-rs-ssr11-night-race.input` loads the
converted night body on Special Stage Route 11 and sustains live race rendering
through the 9,000-poll capture. Both runs are clean of unmapped calls, managed
exceptions, and software faults.

`tests/fixtures/unified-arcade-civic-racer-probe.input` captures all three
Civic Racer liveries and verifies the ten-entry Class B wrap.
`tests/fixtures/unified-arcade-civic-racer-race.input` completes a Tahiti Road
lap in 1:44.209 and enters lap two in third with clean body rendering and no
runtime fault.

`tests/fixtures/unified-arcade-db7-coupe-probe.input` captures all three DB7
Coupe paints and verifies the nine-entry Class A wrap.
`tests/fixtures/unified-arcade-db7-coupe-race.input` then runs the car on
Tahiti Road: auto-drive completes lap one in 1:32.603 and enters lap two in
third. The full smoke is clean of unmapped calls, managed exceptions, and
software faults.

`tests/fixtures/unified-arcade-impreza-sti-v3-probe.input` captures the three
GT1 Impreza liveries and verifies the ten-entry Class A wrap.
`tests/fixtures/unified-arcade-impreza-sti-v3-race.input` reaches Tahiti Road,
engages the original AI racing-line controller, renders the converted car and
a complete six-car grid, completes lap one in 1:44.903, and enters lap two in
third place. The full 9,000-poll smoke is clean of unmapped calls, managed
exceptions, inflater faults, and software crashes.

`tests/fixtures/unified-arcade-soarer-vvti-probe.input` captures all three GT1
Soarer palettes and verifies the eleven-entry Class A wrap.
`tests/fixtures/unified-arcade-soarer-vvti-race.input` then runs the converted
body on Tahiti Road: auto-drive completes lap one in 1:40.356 and enters lap
two in fourth place. The full 9,000-poll smoke is clean of unmapped calls,
managed exceptions, inflater faults, and software crashes.

`tests/fixtures/unified-arcade-supra-rz-probe.input` captures all three GT1
Supra liveries and verifies the twelve-entry Class A wrap.
`tests/fixtures/unified-arcade-supra-rz-race.input` then runs the imported
palette package on Tahiti Road: auto-drive completes lap one in 1:33.067 and
enters lap two in first place. The full 9,000-poll smoke is clean of unmapped
calls, managed exceptions, inflater faults, and software crashes.

`tests/fixtures/unified-arcade-silvia-qs-1800-probe.input` captures all three
GT1 Silvia palettes and verifies the thirteen-entry Class C wrap.
`tests/fixtures/unified-arcade-silvia-qs-1800-race.input` then runs the
imported car on Tahiti Road: auto-drive completes lap one in 1:54.118 and
enters lap two in second place. The full 9,000-poll smoke is clean of unmapped
calls, managed exceptions, inflater faults, and software crashes.

`tests/fixtures/unified-arcade-lancer-evo-iv-gsr-probe.input` captures the
exact named GT1 logo and all three yellow, teal, and purple Lancer palettes,
then verifies the thirteen-entry Class A wrap.
`tests/fixtures/unified-arcade-lancer-evo-iv-gsr-race.input` completes lap one
on Tahiti Road in 1:39.859 and enters lap two with a complete six-car grid.
The full 9,000-poll smoke is clean of unmapped calls, managed exceptions,
inflater faults, and software crashes.

`tests/fixtures/unified-arcade-alcyone-svx-s4-probe.input` captures the exact
named GT1 logo and all three white, blue, and purple Alcyone palettes, then
verifies the eleven-entry Class B wrap.
`tests/fixtures/unified-arcade-alcyone-svx-s4-race.input` completes lap one on
Tahiti Road in 1:43.229 and enters lap two in fifth place. This is the first
full 9,000-poll race smoke with a 45,106-byte parameter database, 50 bytes
beyond the stock workspace boundary; it retains a complete six-car grid and
exits cleanly without an unmapped call, managed exception, inflater fault, or
software crash.

`tests/fixtures/unified-arcade-celica-ssii-probe.input` captures the exact
named Toyota/Celica GT1 art, the authored teal, purple, and yellow palettes,
and the twelve-entry Class B wrap. The native structural converter preserves
the GT1 model's UV layout rather than projecting it against GT2's later
Celica texture package. `tests/fixtures/unified-arcade-celica-ssii-race.input`
then completes lap one on Tahiti Road in 1:42.230 and enters lap two in third
place. The full 9,000-poll smoke, plus a separate run through the interactive
exception path, both exit cleanly without an unmapped call, managed exception,
inflater fault, or software crash.

`tests/fixtures/unified-arcade-crx-91-si-probe.input` captures the exact named
Honda/CR-X GT1 art, the authored black, yellow, and purple palettes, and the
thirteen-entry Class B wrap. GT1 proves that `h-rxn` and production `hcrxn`
share byte-identical day/night geometry; structural conversion is still
required to preserve GT1's packet UVs against its exclusive texture package.
`tests/fixtures/unified-arcade-crx-91-si-race.input` then completes lap one on
Tahiti Road in 1:44.516 and enters lap two in fourth place with a complete
six-car grid. The full 9,000-poll smoke and a separate uncaught interactive
path both exit cleanly without an unmapped call, managed exception, inflater
fault, or software crash.

The Racing/Drift master tables must remain sorted by packed car ID. An early
diagnostic build appended `a-ian`, so the binary-searching race loader missed
the otherwise valid record and left the car stationary. Ordered insertion
fixed the data at its source; no runtime lookup patch or donor slot remains.
The serialized car-reference order also deliberately follows GT2's schema:
LSD precedes Gear, Suspension, Intercooler, and Muffler even though the GTDT
block directory places LSD after them. Correcting that non-isomorphic order
and rerunning all three Roadsters removed an accidentally inflated RS lap time
while retaining clean, mutually consistent driving behavior.

Stock Arcade expands `arcade_data.dat` at `0x800F84C0` into a fixed
`0xB000`-byte workspace. A naive sixth-car conversion reached 45,258 bytes,
overwrote the adjacent opponent pool, and produced a blank second grid record
before the race loader faulted. Stock GT2 deliberately lets different cars
reference byte-identical physical part records. The converter now interns
those records by their consumed payload and retains target-owned visual/body
data.

The unified PC runtime already supplies the original guest with an 8 MiB
devkit RAM map. Its patched native Arcade overlay therefore relocates the
persistent database to a dedicated 1 MiB arena at
`0x80200000`-`0x802FFFFF`, wholly above the retail game's first 2 MiB and
below the other unified-runtime reservations. The same destination is applied
to the recompiled guest source, and the converter rejects output larger than
that arena. The thirteenth imported car produces a 45,930-byte database,
leaving 1,002,646 bytes of deterministic expansion capacity. Its clean
full-race smoke exercises 874 bytes beyond the stock boundary without
corrupting the opponent pool. The converter also refuses to grow a normal
Arcade class beyond
the highest proven thirteen-entry frontend capacity until the corresponding
native arrays and call sites are explicitly audited.

Stock Arcade loads `arcade/arc_carlogo` at `0x80129520` with a
`0x66000`-byte bound and keeps four related frontend descriptors in the
immediately preceding 64 bytes. Adding the CR-X grows the native archive to
`0x6630C`; the overrun visibly corrupts `CAR SELECTION` art and persists after
wrapping to a stock car. The unified native overlay relocates the complete
17-reference address family—not only the loader destination—into the free
`0x80400000` devkit-RAM MiB. Descriptors begin at `0x804094E0`, the archive
begins at `0x80409520`, and its validated `0xF0000`-byte bound ends at
`0x804F9520`. The recompiled guest applies the same relocation, and the
converter rejects a logo archive larger than the reserved bound. The
corrected CR-X menu, stock-car wrap, full race, and uncaught interactive-path
smokes are all clean.

Car selection artwork is not limited to `ARCADE.DAT`. The converter also
validates the US disc's `MENU_RAW.ARC` name table against `MENU_IMG.ARC` and
can import an exact named 4-bit TIM directly. The mapping is direct—including
the real `gt.ins` member at index zero—and is rejected if a name is absent,
ambiguous, or not native 4-bit artwork. This supports GT1-exclusive and prize
cars without synthesized wordmarks or unrelated family logos.

Remaining GT1-exclusive cars, paint schemes, prize/Arcade liveries, and wheel
variants still need conversion through the same content layer.

## Validated native flow

The deterministic full-race fixture has completed:

`Unified title → Arcade Mode → Single Player → Road Race → Easy → Class A →`
`Car/Transmission/Course → Starting Grid → two-lap race → Results → Replay →`
`Single Race menu → Exit → Arcade Mode → Single Player re-entry`

The same cleaned build also selects Gran Turismo Mode and reaches the original
Simulation home map. Both paths use the native renderer and original disc art.
