# Arcade visual coverage reconciliation

Updated 2026-09-06. This is an evidence inventory, not a new claim that every
course is defect-free. Existing reviews were recovered from `work`, which the
initial search missed. Do not repeat their completed coverage merely because
the short TO-DO list omitted it.

## Scope and evidence rules

The launcher supports 30 stock forward courses, 22 stock reverse layouts, and
converted SSR11 in both directions. Earlier records alternated between totals
of 54 layouts and 31 forward courses; these denominators are not interchangeable.
Forward coverage does not certify reverse layouts. Record those separately.

Most full reviews inspected 60 native screenshots individually in chronological
order at three-second intervals over 180 seconds. That is sampled temporal
coverage, not inspection of every rendered frame. Preserve completed reviews;
newly discovered defects or global renderer changes warrant targeted regression
checks, not automatic erasure of earlier work.

## GT1 Racing Modification conversion and smoke matrix

The native GT1 conversion pipeline now includes all 31 archive-proven distinct
Racing Modification bodies. Each converted body overlays its existing native
GT2 resource identity, avoiding unsupported synthetic resource-name allocation.
Converted models are required to fit the exact corresponding native GT2 model
allocation. Forty of 62 day/night resources retain all three authored LODs;
the 22 oversized resources retain the exact authored maximum-detail LOD and
shadow while duplicating the authored low-detail model into the two fallback
entries that are nonresident under the shipped maximum-LOD policy. The minimum
remaining native-slot slack across all 62 resources is 36 bytes.

`artifacts/gt1-rm-livery-smoke-matrix-native-slots-v2-20260906/manifest.json`
records 62/62 passing native runs: both paints of all 31 bodies reached the
ordinary Arcade selector and a race, produced an exact body/palette resolver
trace, retained one selector and one settled stage-poll-300 race-grid capture,
and exited without an unmapped call or native exception. All 124 retained PNG
hashes were rechecked against their per-run manifests. Runs used only
process-local scripted input and the installed reviewed D4 executable,
SHA256 `D4A1826606AA70ACF8E42DC6E675DA9D278CFA03FAEA24058EA80748CD071160`.
This is complete silent smoke coverage, not a claim that all 124 captures were
human-reviewed frame by frame; the summary therefore correctly remains
`native-matrix-complete-review-pending`.

## Stock forward inventory

Paths below are repository-relative. Historical acceptance is credited from
explicit review text, even when the adjacent capture manifest still says
`capture-complete-review-pending`. A capture-only manifest is not acceptance.

| Course | Recovered evidence / present disposition |
| --- | --- |
| Tahiti Road | Accepted: `work/arcade-track-proofs/tahiti-road/manifest.json`, 60 reviewed frames. |
| Midfield Raceway | Accepted historical review, 50 frames; later tree-depth defect corrected and reviewed in `artifacts/midfield-depth-fix/verification.md`. |
| High Speed Ring | Current 60 native captures at stage 3..180 seconds individually reviewed in chronological order; sampled visual review accepted. Grid, overlays, close opponents, reflections, banked turns and tunnel transitions retained. See `artifacts/course-review/high-speed-ring/coverage-gap-20260905-215700/REVIEW.md`. Not an every-frame flicker certification. |
| Super Speedway | Accepted: proof manifest, 60 frames plus 13 startup frames; gantry obstruction verified authored. |
| Seattle Short Course | Accepted: proof `REVIEW.md`, 60 captures plus 17 denser overlay-window captures. |
| Rome Short Course | Accepted: proof `REVIEW.md`, 60 captures. |
| Red Rock Valley Speedway | Accepted historical 60-frame review; subsequent grid/reflection defects corrected in commit `083c601`, current full-strength capture approved by user. See `artifacts/start-grid-audit/REVISION.md`. |
| Seattle Circuit | Accepted: proof `REVIEW.md`, 60 captures and measured overpass repair. |
| Rome Circuit | Accepted: `work/arcade-track-review/rome-circuit/REVIEW.md`; panorama concerns resolved with guest-packet oracle. |
| Grindelwald | Accepted: proof `REVIEW.md`; panorama matches guest-packet reference byte-for-byte. |
| Laguna Seca Raceway | Historical full sampled review plus current 90..111-second targeted review accepted. Earlier stray road fragments are absent; faint sky seams match explicit color discontinuities in the original sky model. See `artifacts/course-review/laguna-seca-raceway/REVIEW.md`, including the excluded original 105-second frame and its verified replacement. |
| Apricot Hill Speedway | Accepted: proof `REVIEW.md`, 60 captures. |
| Motor Sports Land | Accepted: `work/arcade-track-review/motor-sports-land/REVIEW.md`; backdrop concern resolved by guest oracle. |
| Trial Mountain | Accepted sampled review: all 60 fresh 3..180-second captures individually reviewed, including a full lap; prior material evidence credited. Incorrect road-overlay annotation on rotated sign panels is fixed in isolated candidate B7C93BFD: the exact failing normal-path frame and further signs in all 13 reviewed 132..168-second captures pass. Sky artwork matches all 280 source triangles/38 TIM uploads. Cross-road/overhead structures match source geometry/materials and persist with stock selection (`STRUCTURES-SOURCE.md`). Rock/foliage gaps trace to mismatched original mesh boundaries (`ROCK-SEAM-SOURCE.md`, `FOLIAGE-SEAM-SOURCE.md`) and are preserved rather than welded. See `artifacts/course-review/trial-mountain/coverage-gap-20260905-222558/REVIEW.md`. |
| Clubman Stage Route 5 | Accepted: proof `REVIEW.md`; subsequent physical flare-depth regression reviewed in `artifacts/ssr5-depth-fix/verification.md`. |
| Grand Valley East Section | Accepted: proof `REVIEW.md`, 60 captures. |
| Grand Valley Speedway | Accepted: proof `REVIEW.md`, 60 captures and tunnel-junction diagnosis. |
| Special Stage Route 5 | Later flare-depth correction accepted in `artifacts/ssr5-depth-fix/verification.md`, superseding the failed `ssr5-depth-check-20260904` check. Nine-capture smoke, not a new full-lap certification. |
| Autumn Ring | Accepted: proof `REVIEW.md`, 60 captures; road-face lighting proved authored. |
| Test Course | Accepted: retained historical 60-capture review plus targeted corrections in isolated candidate 0C69ADF7. Whole-box depth gate removes object 79's car-covering strip; wide translation fixes displaced distant geometry. All three 90..96 wall checks, ten one-second foliage checks around 78/114, and three 101..103 historical-ribbon checks pass. Full scene retains the wall at its distant location. Gray sky streaks match original data. Six RRV and five Seattle regression checks pass. Candidate not installed; no every-frame claim. See `artifacts/course-review/test-course/REVIEW.md`. |
| Deep Forest Raceway | Accepted: proof `REVIEW.md`, 60 captures after global overlay-owner correction. |
| Rome Night | Accepted: proof `REVIEW.md`; backdrop and lamp effects verified against guest packets. |
| Autumn Ring Mini | Accepted: proof `REVIEW.md`, 60 captures including results sequence. |
| Green Forest Roadway | Accepted: proof `REVIEW.md`; near-camera foliage reproduced by guest packets. |
| Pikes Peak Downhill | Historical sampled review plus corrected installed-build check: all seven native captures at 123..129 seconds individually reviewed; black/ochre foreground defect resolved by perspective color interpolation without geometry removal. Regression and negative control pass. No blanket every-frame claim. See `artifacts/course-review/pikes-peak-downhill/COLOR-FIX.md`. |
| Pikes Peak Hill Climb | Full sampled review plus all 15 corrected-build captures at 135..177 seconds reviewed individually. Black road sheet at 144 is absent. Dark billboard texels/palette and RGB match original data (`artifacts/course-review/pikes-peak-hill-climb/FOLIAGE-SOURCE.md`). Shared Laguna sky's faint color boundaries are authored. The separate upper dashed line is repaired in candidate 8D1BA2B2: both native 159/162 captures reviewed, same-frame on/off replay fills 162 clear sky pixels and changes zero existing pixels. See `artifacts/course-review/pikes-peak-hill-climb/identity-provenance-20260905-233804/REVIEW.md`. Targeted deferred findings resolved; candidate-wide regressions still tracked below. |
| Smokey Mountain North | Retained full sampled review plus targeted deferred findings resolved in candidate D5B6A319. Both final normal-path 117/120 captures reviewed: identified sky dots are absent. Same-frame repair fills ten clear pixels without changing existing pixels (`artifacts/course-review/smokey-mountain-north/SKY-CRACK-DIAGNOSIS.md`). Stage-105 dirt boundary matches exact original geometry, RGB, UV/CLUTs, image payloads and final palette writers (`DIRT-SOURCE.md`); preserve authored mapping. Candidate remains isolated; no every-frame or reverse certification. |
| Smokey Mountain South | Retained full sampled review plus targeted deferred findings resolved in candidate D5B6A319. Stage-33 central dirt boundary matches exact source geometry/materials and final palette writer (`artifacts/course-review/smokey-mountain-south/DIRT-SOURCE.md`). All three final normal-path 120/123/126 captures individually reviewed: identified sky dots, including later stage-126 dots, are absent; gradients and reflections retained. Same-frame gradient-parent correction fills four clear pixels without changing existing pixels. Candidate remains isolated; no every-frame or reverse certification. |
| Tahiti Dirt Route 3 | Accepted: proof `REVIEW.md`; dirt boundaries verified authored. |
| Tahiti Maze | Accepted: proof `REVIEW.md`; exact packet correlation and reference resolve dirt tiling. |

Unless otherwise specified, `proof REVIEW.md` means
`work/arcade-track-proofs/<course-slug>/REVIEW.md`.

## Separate coverage

- Converted SSR11 forward has a full 60-capture accepted review at
  `work/arcade-track-proofs/special-stage-route-11/REVIEW.md`, followed by later
  installed-data and sky corrections. It is not a stock GT2 course.
- Stock reverse layouts: RRV reverse is accepted after all 60 native
  3..180-second captures were individually reviewed, including a complete
  lap (`artifacts/course-review/red-rock-valley-speedway-reverse/reverse-coverage-20260905-235617/REVIEW.md`).
  Trial Mountain reverse is accepted after all 60 samples were reviewed,
  including a complete lap. Its three thin foliage/rock lines trace to exact
  mismatched endpoints in the original reverse course and are preserved
  (`artifacts/course-review/trial-mountain-reverse/reverse-coverage-20260905-235628/REVIEW.md`).
  High Speed Ring reverse is accepted with all 60 samples reviewed, including
  a complete lap; its stage-54 ceiling strips match original lamp geometry,
  RGB, texture and palette (`artifacts/course-review/high-speed-ring-reverse/TUNNEL-SOURCE.md`)
  (`artifacts/course-review/high-speed-ring-reverse/reverse-coverage-20260906-000501/REVIEW.md`).
  Apricot Hill reverse is accepted after its clean serial run and individual
  review of all 60 samples, including a complete lap
  (`artifacts/course-review/apricot-hill-speedway-reverse/serial-allocation-check-20260906-002043/REVIEW.md`).
  Midfield reverse is accepted after all 60 samples were reviewed. Its
  stage-159 hillside lines recur in the exact guest compatibility framebuffer,
  resolving the original endpoint/placement mismatches as authored stock
  geometry (`artifacts/course-review/midfield-raceway-reverse/compatibility-oracle-20260906-064821/ORACLE.md`).
  Earlier allocation-failure runs are excluded. Autumn Ring reverse is accepted after all 60 samples were
  individually reviewed; its underpass foliage slit is original separated
  scenery with matching relative instance placement (`artifacts/course-review/autumn-ring-reverse/FOLIAGE-SOURCE.md`).
  Autumn Ring Mini reverse is accepted after all 40 samples at 3..120 seconds
  were individually reviewed, covering both laps, finish and results.
  Clubman Stage Route 5 reverse is accepted after all 60 samples at 3..180
  seconds were individually reviewed, including a complete lap and both
  tunnel passages. Deep Forest reverse has all 60 samples individually reviewed,
  including a complete lap. Its stage-60 tunnel seam matches original geometry
  and exact placement; its foliage seam also matches original geometry.
  Its near-camera missing car panels are fixed in candidate 6FDEC316 with
  same-frame proof and six Deep Forest plus twelve RRV/Clubman targeted
  regression samples. Deep Forest's sampled review is now accepted. All nine
  full reviews used D5B6A319; targeted correction used 6FDEC316.
  Grand Valley East reverse now has all 60 samples reviewed on 6FDEC316,
  including a complete lap. Its wall gap is corrected in candidate 3E50647D;
  source-proven terrain/foliage boundaries are preserved. Targeted native
  checks and exact-frame regressions pass, so the sampled review is accepted.
  Grand Valley Speedway reverse has all 60 samples at 3..180 seconds reviewed
  on 3E50647D, plus ten183..210 samples covering the lap transition. Candidate
  42F6FD53 fixes its intermittent road sheet over the sky. Candidate 496006F5
  separately fixes the stage-81 bridge road hole; all 36 consecutive one-poll
  frames were reviewed. Exact source correlation proves the pointed terrain
  and stage-210 near-plane kerb are authored. Candidate D4A18266 restores the
  boundary-authored starting-grid outline while retaining the bridge fix;
  all six startup samples and the exact stage-81 regression were reviewed.
  The sampled review is accepted. Grindelwald reverse is also accepted after
  all 60 stage-3..180 native samples were individually reviewed, including
  the grid, close traffic, a complete first lap and the transition to lap 2
  (`artifacts/course-review/grindelwald-reverse/reverse-coverage-20260906-035250/REVIEW.md`).
  Rome Circuit reverse is accepted after all 60 stage-3..180 native samples
  were individually reviewed, including the grid, close traffic, a complete
  first lap and the transition to lap 2
  (`artifacts/course-review/rome-circuit-reverse/reverse-coverage-20260906-040154/REVIEW.md`).
  Rome Short Course reverse is accepted after all 60 stage-3..180 native
  samples were individually reviewed, including the grid, close traffic, a
  complete race, the lap-2 transition and the results overlay
  (`artifacts/course-review/rome-short-course-reverse/reverse-coverage-20260906-041157/REVIEW.md`).
  Rome Night reverse is accepted after all 60 stage-3..180 native samples and
  two seven-frame one-second follow-ups were individually reviewed. The stage-33
  near-plane face is an authored curb tip, and the stage-105 dark prism tracks
  its building as authored rooftop geometry
  (`artifacts/course-review/rome-night-reverse/reverse-coverage-20260906-042348/REVIEW.md`).
  Seattle Circuit reverse is accepted after all 60 stage-3..180 native samples
  were individually reviewed, including the grid, close traffic, a complete
  first lap and the transition to lap 2
  (`artifacts/course-review/seattle-circuit-reverse/reverse-coverage-20260906-043811/REVIEW.md`).
  Seattle Short Course reverse is accepted after all 60 stage-3..180 native
  samples were individually reviewed, including the grid, close traffic, a
  complete first lap and continued lap-2 coverage
  (`artifacts/course-review/seattle-short-course-reverse/reverse-coverage-20260906-045046/REVIEW.md`).
  Smokey Mountain North reverse is accepted after all 60 stage-3..180 native
  samples were individually reviewed, including the grid, close traffic, a
  complete first lap and continued lap-2 coverage. Its transient stage-6 clear
  field is present in the exact guest framebuffer and preserved as authored
  (`artifacts/course-review/smokey-mountain-north-reverse/reverse-coverage-20260906-045822/REVIEW.md`).
  Smokey Mountain South reverse is accepted after all 60 stage-3..180 native
  samples were individually reviewed, including the grid, close traffic, a
  complete first lap and continued lap-2 coverage
  (`artifacts/course-review/smokey-mountain-south-reverse/reverse-coverage-20260906-052333/REVIEW.md`).
  Special Stage Route 5 reverse is accepted after all 60 stage-3..180 native
  samples and two seven-frame portal follow-ups were individually reviewed.
  Its dark tunnel-entry foreground is present in the exact guest framebuffer
  and preserved as authored
  (`artifacts/course-review/special-stage-route-5-reverse/reverse-coverage-20260906-053038/REVIEW.md`).
  Tahiti Dirt Route 3 reverse is accepted after all 60 stage-3..180 native
  samples and 20 stage-183..240 continuation samples were individually
  reviewed, covering a complete first lap and continued lap-two running. Its
  near-camera dirt divisions recur in both the installed build and the exact
  guest compatibility oracle, confirming authored PS1 polygon/affine texture
  mapping
  (`artifacts/course-review/tahiti-dirt-route-3-reverse/reverse-coverage-20260906-055401/REVIEW.md`).
  Tahiti Road reverse is accepted after all 60 stage-3..180 native samples
  were individually reviewed, covering a complete first lap and continued
  lap-two running. Its candidate and installed stage-3 startup images are
  byte-identical, confirming that the finish stripe without painted grid boxes
  is unchanged authored content
  (`artifacts/course-review/tahiti-road-reverse/reverse-coverage-20260906-063640/REVIEW.md`).
  All 22 stock reverse layouts now have accepted sampled reviews. Preserve the
  completed forward evidence rather than repeating it.

### Stock reverse ledger

Each row is a separate supported layout. A running or capture-only audit is
not acceptance. The detailed records above explain the completed reviews.

| Reverse layout | Disposition |
| --- | --- |
| Apricot Hill Speedway | Accepted sampled review after clean serial run; all 60 samples individually reviewed. Earlier -204 fallback captures excluded. |
| Autumn Ring | Accepted sampled review: 60 images, complete lap; underpass foliage spacing verified authored. `artifacts/course-review/autumn-ring-reverse/reverse-coverage-20260906-003531/REVIEW.md`. |
| Autumn Ring Mini | Accepted sampled review: all 40 images at 3..120 seconds, both laps, finish/results. `artifacts/course-review/autumn-ring-mini-reverse/reverse-coverage-20260906-003636/REVIEW.md`. |
| Clubman Stage Route 5 | Accepted sampled review: all 60 native images at 3..180 seconds, complete lap, grid, reflections and tunnel passages. `artifacts/course-review/clubman-stage-route-5-reverse/reverse-coverage-20260906-005131/REVIEW.md`. |
| Deep Forest Raceway | Accepted sampled review: all 60 native samples, complete lap; tunnel/foliage seams source-proven. Camera-plane vehicle facing defect fixed in 6FDEC316, six targeted samples and twelve RRV/Clubman regressions pass. `artifacts/course-review/deep-forest-raceway-reverse/CAR-NCLIP-FIX.md`. |
| Grand Valley East Section | Accepted sampled review: all 60 images and a complete lap. Wall crack fixed in 3E50647D with eight reviewed exact-frame replays, six native checks and RRV regressions. Pointed terrain and finite/one-sided foliage source-proven, preserved. `artifacts/course-review/grand-valley-east-section-reverse/WALL-FIX.md`. |
| Grand Valley Speedway | Accepted sampled review: all 70 route/transition samples individually reviewed; first lap 2:59.377. Stage-120 false road sheet fixed, bridge-81 hole fixed across 36 consecutive frames, terrain/near-plane kerb source-proven, and D4A18266 restores the full starting-grid outline with six reviewed startup samples. `artifacts/course-review/grand-valley-speedway-reverse/STARTBOX-BOUNDARY-FIX.md`. |
| Grindelwald | Accepted sampled review: all 60 native images at 3..180 seconds, complete first lap and lap-2 transition, grid, close traffic and reflections. `artifacts/course-review/grindelwald-reverse/reverse-coverage-20260906-035250/REVIEW.md`. |
| High Speed Ring | Accepted sampled review; ceiling strips verified authored. |
| Midfield Raceway | Accepted sampled review: serial pass completed cleanly; all 60 native samples reviewed. Stage-159 lines are sky through original primary/auxiliary gaps and recur in the exact guest compatibility framebuffer (`artifacts/course-review/midfield-raceway-reverse/compatibility-oracle-20260906-064821/ORACLE.md`). Earlier -302 fallback captures excluded. |
| Red Rock Valley Speedway | Accepted sampled review. |
| Rome Circuit | Accepted sampled review: all 60 native images at 3..180 seconds, complete first lap and lap-2 transition, grid, close traffic and reflections. `artifacts/course-review/rome-circuit-reverse/reverse-coverage-20260906-040154/REVIEW.md`. |
| Rome Short Course | Accepted sampled review: all 60 native images at 3..180 seconds, complete race and results transition, grid, close traffic and reflections. `artifacts/course-review/rome-short-course-reverse/reverse-coverage-20260906-041157/REVIEW.md`. |
| Rome Night | Accepted sampled review: all 60 native images at 3..180 seconds plus two seven-frame one-second follow-ups; complete first lap and lap-2 transition, grid, close traffic and reflections. Curb-tip and rooftop silhouettes resolved as authored geometry. `artifacts/course-review/rome-night-reverse/reverse-coverage-20260906-042348/REVIEW.md`. |
| Seattle Circuit | Accepted sampled review: all 60 native images at 3..180 seconds, complete first lap and lap-2 transition, grid, close traffic and reflections. `artifacts/course-review/seattle-circuit-reverse/reverse-coverage-20260906-043811/REVIEW.md`. |
| Seattle Short Course | Accepted sampled review: all 60 native images at 3..180 seconds, complete first lap and continued lap-2 coverage, grid, close traffic and reflections. `artifacts/course-review/seattle-short-course-reverse/reverse-coverage-20260906-045046/REVIEW.md`. |
| Smokey Mountain North | Accepted sampled review: all 60 native images at 3..180 seconds, complete first lap and continued lap-2 coverage, grid, close traffic and reflections. The stage-6 field is exact guest clear exposure, confirmed by candidate/installed controls and captured guest oracle. `artifacts/course-review/smokey-mountain-north-reverse/reverse-coverage-20260906-045822/REVIEW.md`. |
| Smokey Mountain South | Accepted sampled review: all 60 native images at 3..180 seconds, complete first lap and continued lap-2 coverage, grid, close traffic and stable reflections. `artifacts/course-review/smokey-mountain-south-reverse/reverse-coverage-20260906-052333/REVIEW.md`. |
| Special Stage Route 5 | Accepted sampled review: all 60 native images at 3..180 seconds plus two seven-frame one-second portal follow-ups; complete first lap and continued lap-2 coverage, grid, dense traffic and stable reflections. Tunnel-entry foreground confirmed in exact guest oracle. `artifacts/course-review/special-stage-route-5-reverse/reverse-coverage-20260906-053038/REVIEW.md`. |
| Tahiti Dirt Route 3 | Accepted sampled review: all 60 native images at 3..180 seconds plus 20 continuation images at 183..240 seconds; complete first lap and continued lap-2 coverage, grid, close traffic and stable reflections. Near-camera dirt partitions reproduced by the installed control and exact guest compatibility oracle. `artifacts/course-review/tahiti-dirt-route-3-reverse/reverse-coverage-20260906-055401/REVIEW.md`. |
| Tahiti Road | Accepted sampled review: all 60 native images at 3..180 seconds, complete first lap and continued lap-2 coverage, close traffic and stable reflections. Candidate/installed stage-3 startup images are byte-identical, confirming the authored finish stripe without painted grid boxes. `artifacts/course-review/tahiti-road-reverse/reverse-coverage-20260906-063640/REVIEW.md`. |
| Trial Mountain | Accepted sampled review: all 60 samples reviewed; stage-120/129/162 lines trace to mismatched original foliage/rock endpoints and are preserved rather than welded. `artifacts/course-review/trial-mountain-reverse/reverse-coverage-20260905-235628/REVIEW.md`. |

## Verified renderer build

The reviewed renderer build is
`artifacts/course-review/startbox-boundary-runtime/GranTurismo2PC.exe`,
SHA256 `D4A1826606AA70ACF8E42DC6E675DA9D278CFA03FAEA24058EA80748CD071160`.
It retains 42F6FD53's invalid/saturated PS1 seam-target guard and 496006F5's
same-texture material-overlap threshold so adjacent fixed-point road sectors
cannot be misclassified as stencil-only artwork. Mixed
untextured-marking/textured-road boundary pairs retain their authored overlay
relationship, restoring Grand Valley Speedway's starting-grid outline. Its
false road sheet is removed in all 16 reviewed exact-frame replays and seven
native checks; its bridge road hole is absent from all 36 reviewed consecutive
one-poll frames and the new exact stage-81 regression. All six new startup
samples retain the grid, close cars and reflections. All eight native CTest
suites pass. Corrected GVE wall, Deep Forest tunnel/foliage/car and Midfield
hillside replays were already byte-identical under the inherited candidate.
This exact binary was promoted to `OpenGTPS1/GranTurismo2PC.exe` on
2026-09-06 and verified from the installed path. See
`artifacts/course-review/grand-valley-speedway-reverse/STARTBOX-BOUNDARY-FIX.md`.
The preceding 3E50647D adds a guarded exact shared-endpoint adjustment across course chunks to
6FDEC316. This corrects Grand Valley East's transient wall crack without
welding nearby source vertices or altering materials. All eight native
CTest suites pass; eight exact-frame replays, six GVE native checks and six
RRV startup regressions were individually reviewed. Seven of eight GVE
replays are unchanged; the failing frame changes only 777 wall pixels.
Deep Forest tunnel/foliage/car and Midfield hillside replays are byte-identical
to retained evidence (`artifacts/course-review/grand-valley-east-section-reverse/WALL-FIX.md`).
The preceding 6FDEC316 added a camera-plane vehicle NCLIP depth-product sign correction to retained
D5B6A319. The modern configuration suite passes, as do six Deep Forest close-car
checks, six RRV startup checks and six Clubman night-traffic checks. No LOD or
reflection-strength change was made (`artifacts/course-review/deep-forest-raceway-reverse/CAR-NCLIP-FIX.md`).
The inherited candidate combines the whole-box depth gate, local-coordinate sign-overlay correction,
wide course translation, and source-proven sky midpoint repair with original
background vertex identities. Flat parents may be split; gradient parents
remain intact, with only a proven uncovered gap filled using original RGB.
All eight native CTest suites and the modern configuration/translation suite
pass. The latter was rerun after a concurrent publish caused a temporary PDB
file lock; `background-gap-config-retry.log` records success. North replay
fills all ten identified sky holes, South four, Pikes 162, changing zero
existing pixels (`artifacts/course-review/sky-pixel-comparisons.json`).
Six RRV startup regressions on the preceding constant-edge candidate pass,
as does a final-candidate stage-6 grid/reflection check. Both final North
117/120 checks, all three South 120/123/126 checks and all five Seattle
99..111 checks pass. Final Pikes replay is byte-identical to its reviewed
8D1BA2B2 result (SHA256 5276F25A1EC05BD526535B40A5B81250F36433C614CB7D3D3012434AEDF9C437).
The installed executable is now the reviewed D4A18266 build. A native,
process-local Red Rock Valley reverse startup smoke from the installed path
captured a unique authored frame with the starting boxes, cars and reflections
present and exited cleanly. Promotion proof is retained in
`artifacts/course-review/renderer-candidate-promotion-20260906/INSTALL.md`.
No commit or push was performed as part of promotion.

1. All 22 stock reverse layouts and all retained forward sampled reviews are
   accepted; preserve this evidence rather than repeating completed coverage.
2. Reproduce only still-actionable deferred findings in the installed build,
   using isolated copied cards and native capture, with no host input.
3. Preserve authored textures, reflection strength, lighting and geometry;
   compare suspicious content against existing guest-packet evidence before
   treating it as a renderer defect.
4. Keep this record synchronized with actual review outcomes. No blanket
   no-glitch or sustained-60-FPS claim follows from this visual audit.
