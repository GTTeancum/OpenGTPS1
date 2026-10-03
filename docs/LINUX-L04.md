# L04 — native Linux game-host integration

This is an experimental, in-game-tested Linux extension of the uploaded working
checkout and cumulative L03 renderer, not a reset to upstream Git HEAD. The
original Windows README and its historical test claims are retained unchanged.

The complete recompiled game now runs on Linux through the original direct-race
entry point. Seattle Circuit and Red Rock Valley Speedway were driven with
scripted ordinary controller inputs. Their original cars, wheels, track geometry,
auxiliary scenery, sky, HUD, racing clock and opponents are rendered in the game.
The C++ EGL renderer is used for provenance-backed 3D; the original CPU screen
compositor remains for world-free screens. No Wine, emulated CPU, staged asset
scene, or manufactured gameplay state is used for these captures.

`lighting/in-game-L04.shader` is an explicitly opt-in **two-track development
configuration**, not a validated all-track release preset. It resolves the two
course profiles through the original completed texture-upload registry. Its suns
are explicitly provisional (zero measured confidence). They enable actual sun
passes but cannot authorize baked-gain or shadow-polygon-removal rules. The
original baked shading is still present. The default full catalogue remains
`lighting/lighting.shader`. Do not deploy the two-track development configuration
as a global all-track preset.

## Linux implementation

`Directory.Build.props` selects the Linux host only for Linux, leaving the
Windows host and Windows dependency selection in place. The Linux configuration
uses the exact supplied offline Silk.NET packages. `LinuxHostWindow.cs` supplies
window creation, OpenGL final presentation, original input polling, resize/aspect
updates, display capture and shutdown. It presents one authored native output at
a time and records its frame/poll identity. It does not create synthetic frames.

Completed original CPU uploads now reach the native source-bank registry even
when the Windows-only HLE graphics backend is absent. Partial, aborted, masked,
filled, copied and wrapped transfers do not leave false or stale source identities.

Version-6 captures may now identify the source primary camera axes using reserved
flag bit 3. The lighting basis accepts the actual nondegenerate left-handed camera
and converts the primary source axes without changing guest geometry/projection.

The Linux renderer also preserves the uploaded horizontal-plus scissor policy:
main track, cars and backdrop span the widened view; secondary views and screen
art retain their own horizontal clips. Original vertical clipping is unchanged.

## Reproduction for continued development

`tools/build_linux_lighting.py --sdk SDK --feed FEED --window-tests` builds and
tests without downloading dependencies. SDK is the extracted supplied Linux
.NET 10 SDK; FEED contains the supplied .nupkg files. Native CMake/C++ build tools,
Mesa/OpenGL/EGL, SDL2 and an X11 display are system inputs. The script does not
install them or launch a game. Logs and a build result are written under build/.
The window regression requires an actual DISPLAY (Xvfb was used here).

Original compile-time embedded resources remain part of the user's checkout.
They, original game assets, saves, SDKs, NuGet caches, font files and old Windows
binaries are not included in the recovery overlay. The supplied resource/hash
manifests identify the external inputs; no new user upload is required for this
workspace. Do not delete the original checkout's embedded resource files.

The actual game is launched with the existing `--arcade-race seattle-circuit` or
`--arcade-race red-rock-valley-speedway` argument and the prepared game directory.
L04 logs include the exact commands, explicit lighting-script path, resolution,
software-driver settings, input pulses and loaded binary hashes. R1 is exercised
through the original controller input path for the exterior camera.

`OPENGT_LINUX_WAIT_FOR_RENDER=1` is explicit software-proof backpressure with a
bounded 10-second native wait. It changes wall-clock progress, not the guest clock
or driving rules. The default retains the original short output wait budget.
Videos sample one real frame per six guest input polls. Playback follows game-frame
time, not measured software wall-clock speed; they are not 60-FPS performance proof.

## Scope limits

Only bounded early-race sessions on the two requested tracks are accepted here,
not complete laps/races or every track. The existing direct-race constructor is
used; no full title/menu/save/replay navigation is claimed. Real audio and physical
controllers are untested; sessions ran muted. The Linux developer ImGui interface,
interactive file picker and external high-resolution texture atlas are not ported.
Original low-resolution texture aliasing, lighting/material tuning and remaining
2D/world visual artifacts have not all been eliminated. Hardware graphics speed,
Windows compilation and a general production Linux release remain unverified.
