// ARTIFICIAL SUN TEST ONLY. NOT AN ASSET-INFERRED TRACK PROFILE.
// OpenGT L01: reloads once per second. Remove this file for stock rendering.
// Colors are linear RGB. Course coordinates use +Y down.
settings { forceTrack diagnostic enabled 1 shadows 1 shadowResolution 1024 minimumSunConfidence 0.75 normalCrease 55 shadowBias 0.001 }
material car/paint { surface car roughness 0.34 specular 0.22 reflection 0.12 clearcoat 0.24 clearcoatRoughness 0.16 diffuseMix 0.12 }
material car/rubber { surface rubber roughness 0.92 specular 0.015 reflection 0 clearcoat 0 diffuseMix 0.10 }
material road/dry { surface road roughness 0.84 specular 0.07 reflection 0.025 clearcoat 0 diffuseMix 0.14 }
material track/default { surface track roughness 0.88 specular 0.025 reflection 0 clearcoat 0 diffuseMix 0.10 }
// Deliberately NOT selected automatically: a renderer test profile, not an
// inferred track sun. Add `forceTrack diagnostic` inside settings to exercise
// the directional shadow pass independently of calibration.
track diagnostic { sunDirection 0.4 -0.8 0.3 sunColor 1 0.93 0.83 skyColor 0.36 0.46 0.60 groundColor 0.17 0.16 0.15 sunIntensity 0.55 sunConfidence 1 ambient 0.84 shadowStrength 0.35 }
