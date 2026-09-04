# OpenGTPS1 TO-DO

Only unresolved work belongs in this list.

1. Make cleanup the final step of every test, capture, profiling, conversion, and smoke-test batch, retaining only current evidence and reporting repository size and free disk space.
2. Finish visual validation of every stock Arcade course with the unlocked North American card. Loaded-card progress now survives the unified handoff; SSR5 and restored SSR11 are visually confirmed in the installed EXE (`artifacts/course-access-final/verification.md`).
3. Recreate the original main-menu transition animation and validate it frame by frame across every menu destination.
4. Complete the native GT1 conversion pipeline, including every archive-proven exclusive asset and silent selector/race smoke coverage for both paints of all 31 staged RM bodies.
5. Bring the modern renderer to sustained, correctly paced 60 FPS at full quality in Arcade and Gran Turismo modes with representative race, replay, imported-content, HUD, stability, and telemetry validation.
6. Correct SSR11's remaining DirectX rendering defects and validate the result against stock GT2.
7. Add resolution selection outside the graphics-quality preset system.
8. Implement widescreen support and its associated resolution work in a future build.
9. Restore playback of the game’s intro videos.
10. Later, deliver an original-Xbox port with a CLR-free C++ runtime, NXDK/NV2A rendering, complete platform services, extensible materials, and a verified 64 MiB memory budget.
11. Reproduce and correct the live main-menu confirm press being ignored until moving away and back. Cross-only process-local tests enter Arcade without that workaround; this does not close the user's live-input report.
