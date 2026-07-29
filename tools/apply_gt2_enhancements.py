#!/usr/bin/env python3
"""Apply configuration-aware GT2 race-overlay enhancements after recompilation."""

from pathlib import Path


REPO = Path(__file__).resolve().parents[1]
GENERATED = REPO / "generated" / "recompiled"


def replace_once(path: Path, old: str, new: str, description: str) -> None:
    source = path.read_text(encoding="utf-8")
    matches = source.count(old)
    if matches != 1:
        raise RuntimeError(
            f"{description}: expected one source match in {path}, found {matches}"
        )
    path.write_text(source.replace(old, new, 1), encoding="utf-8")


def main() -> int:
    race = GENERATED / "gt2_overlay_0.cs"
    track = GENERATED / "gt2_overlay_2.cs"
    entry = GENERATED / "Entry.cs"

    replace_once(
        entry,
        """        Dispatcher.Call(c, m, 0x8005D600u);
""",
        """        RecompOne.Runtime.Sdk.GT2Compat.RunGuestLoop(
            c, m, 0x8005D600u);
""",
        "GT2 non-local overlay transition trampoline",
    )

    replace_once(
        race,
        """        L80014344: ;
        if (c.S5 != 0u) {
""",
        """        L80014344: ;
        c.V0 = RecompOne.Runtime.Sdk.GT2Compat.GetForcedVehicleLodSelector();
        if (c.V0 != 0u) {
            c.S2 = c.V0;
            goto L80014448;
        }
        if (c.S5 != 0u) {
""",
        "maximum vehicle LOD",
    )

    main = GENERATED / "main.cs"
    replace_once(
        main,
        """        c.S4 = m.ReadU32((c.S2 + 0xCu));
        c.S3 = m.ReadU8(c.S2);
        c.FP = m.ReadU8((c.S2 + 0x1u));
""",
        """        c.S4 = m.ReadU32((c.S2 + 0xCu));
        c.S3 = m.ReadU8(c.S2);
        RecompOne.Runtime.Sdk.GT2Compat.TraceVehicleLodSelection(
            c.S6, c.S2, m);
        c.FP = m.ReadU8((c.S2 + 0x1u));
""",
        "vehicle LOD tracing hook",
    )

    replace_once(
        main,
        """        c.RA = m.ReadU32((c.SP + 0x5Cu));
        c.FP = m.ReadU32((c.SP + 0x58u));
""",
        """        RecompOne.Runtime.WorldCaptureContext.EndObject();
        c.RA = m.ReadU32((c.SP + 0x5Cu));
        c.FP = m.ReadU32((c.SP + 0x58u));
""",
        "vehicle world-capture object end hook",
    )

    replace_once(
        race,
        """    public static void func_8003E0C4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
""",
        """    public static void func_8003E0C4(CpuContext c, IMemory m)
    {
        RecompOne.Runtime.Sdk.GT2Compat.ApplyAiAutoDrive(
            c.A0, c.A1, m);
        RecompOne.Runtime.Sdk.GT2Compat.TraceAiDriverTable(
            c.A0, c.A1, m);
        c.V0 = 0x800B0000u;
""",
        "AI driver dispatch tracing hook",
    )

    replace_once(
        race,
        """        c.A1 = 0x000E0000u;
        c.A1 = c.A1 | 0x5700u;
        c.A0 = c.S0 + 0x58u;
        c.A1 = c.S1 + c.A1;
        c.A2 = 0x00030000u;
        m.WriteU32((c.SP + 0x18u), c.RA);
        c.A2 = c.A2 | 0x8000u;
""",
        """        c.A0 = c.S0 + 0x58u;
        if (RecompOne.Runtime.Sdk.GT2Compat.ExpandedPolygonBuffersEnabled) {
            c.A1 = 0x80200000u;
            c.A2 = 0x00070000u;
        } else {
            c.A1 = 0x000E0000u;
            c.A1 = c.A1 | 0x5700u;
            c.A1 = c.S1 + c.A1;
            c.A2 = 0x00030000u;
            c.A2 = c.A2 | 0x8000u;
        }
        m.WriteU32((c.SP + 0x18u), c.RA);
""",
        "expanded polygon buffers",
    )

    replace_once(
        race,
        """    public static void func_80020110_gt2_overlay_0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x1080u;
""",
        """    public static void func_80020110_gt2_overlay_0(CpuContext c, IMemory m)
    {
        RecompOne.Runtime.Sdk.GT2Compat.TraceTrackVisibility(c, m);
        c.SP = c.SP - 0x1080u;
""",
        "race track visibility tracing hook",
    )

    replace_once(
        race,
        """        c.FP = c.FP | 0xFFFFu;
        c.V0 = m.ReadU32((c.V0 + 0xA0u));
        c.S1 = c.SP + 0x10u;
""",
        """        c.FP = c.FP | 0xFFFFu;
        c.V0 = m.ReadU32((c.V0 + 0xA0u));
        c.V0 = RecompOne.Runtime.Sdk.GT2Compat.GetTrackVisibilityList(
            m, c.S6, c.V0);
        c.S1 = c.SP + 0x10u;
""",
        "full-track race visibility list",
    )

    replace_once(
        race,
        """        m.WriteU16((c.S1 + 0xEu), (ushort)c.V1);
        c.S5 = m.ReadU32((c.SP + 0x1054u));
""",
        """        m.WriteU16((c.S1 + 0xEu), (ushort)c.V1);
        RecompOne.Runtime.WorldCaptureContext.RegisterTrackObject(
            c.S1,
            m.ReadU16((c.S2 + 0x2u)) & 0x3FFFu,
            m.ReadU32((c.S1 + 0x4u)));
        c.S5 = m.ReadU32((c.SP + 0x1054u));
""",
        "track world-capture identity registration",
    )

    replace_once(
        race,
        """        c.S0 = m.ReadU32((c.S6 + 0x4u));
        c.S1 = m.ReadU16((c.S6 + 0xCu));
""",
        """        c.S0 = m.ReadU32((c.S6 + 0x4u));
        RecompOne.Runtime.WorldCaptureContext.BeginTrackObject(c.S6, c.S0);
        c.S1 = m.ReadU16((c.S6 + 0xCu));
""",
        "track world-capture object begin hook",
    )

    replace_once(
        race,
        """        c.RA = m.ReadU32((c.SP + 0x107Cu));
        c.FP = m.ReadU32((c.SP + 0x1078u));
""",
        """        RecompOne.Runtime.WorldCaptureContext.EndObject();
        c.RA = m.ReadU32((c.SP + 0x107Cu));
        c.FP = m.ReadU32((c.SP + 0x1078u));
""",
        "track world-capture object end hook",
    )

    replace_once(
        track,
        """    public static void func_80020224(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x58u;
""",
        """    public static void func_80020224(CpuContext c, IMemory m)
    {
        RecompOne.Runtime.Sdk.GT2Compat.TraceTrackRenderRequest(c, m);
        c.SP = c.SP - 0x58u;
""",
        "replay track rendering trace hook",
    )

    replace_once(
        track,
        """        L80020414: ;
        c.A0 = m.ReadU32((c.SP + 0x64u));
""",
        """        L80020414: ;
        if (RecompOne.Runtime.Config.ConfigManager.View.ExtendedDrawDistance) {
            c.S0 = 0x00000001u;
            goto L800204BC;
        }
        c.A0 = m.ReadU32((c.SP + 0x64u));
""",
        "extended track draw distance",
    )

    print("Applied GT2 graphics enhancements to generated race overlays")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
