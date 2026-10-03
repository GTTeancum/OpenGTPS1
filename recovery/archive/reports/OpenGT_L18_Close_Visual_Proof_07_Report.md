# OpenGT L18 Close Visual Proof 07

Date: 2026-09-25

## Why this proof was redone

The previous screenshots were rejected because they were too distant to judge, and the amplified diff images were not useful visual evidence. Those captures ended at poll 372, before the historical L08 chase-camera input at poll 1000.

## Replacement capture contract

- 1280x720 output
- deterministic input: `1000+8=R1;1020+1500=CROSS`
- capture start: poll 990
- capture cadence: every 12 polls for this bounded proof
- exit: poll 1200
- L05 lighting shader unchanged
- software GL validation with explicit renderer backpressure
- lighting/shadow/world-camera audits enabled

Seattle and Red Rock each completed with 859/859 native outputs, zero dropped/synthetic/repeated outputs, zero raw track/background decode failures, zero guest fallbacks, and clean exit.

## Matched proof frame

The retained accepted L08 gameplay videos contain the frozen acceptance sequence at 10 fps / 115 frames, corresponding to poll 995 through poll 1679 every 6 polls. Poll 1157 is video frame index 27.

The replacement proof therefore pairs L08 and L18 at the exact same poll 1157, same chase-camera state, same HUD time, and same 1280x720 presentation.

### Seattle

- `Seattle_L08_vs_L18_poll1157.png`
- `Seattle_L08_vs_L18_poll1157_car_road.png`
- `Seattle_L08_vs_L18_poll1157_trackside.png`

### Red Rock

- `RedRock_L08_vs_L18_poll1157.png`
- `RedRock_L08_vs_L18_poll1157_car_road.png`
- `RedRock_L08_vs_L18_poll1157_trackside.png`

## Acceptance status

No automatic promotion is claimed. L08 remains visual authority until the user reviews and accepts these replacement close chase-camera comparisons.
