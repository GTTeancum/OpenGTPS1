#!/usr/bin/env python3
"""Apply Arcade-specific guest control-flow fixes after recompilation."""

from pathlib import Path


REPO = Path(__file__).resolve().parents[1]
ENTRY = REPO / "generated" / "arcade-recompiled" / "Entry.cs"
MAIN = REPO / "generated" / "arcade-recompiled" / "main.cs"
PROJECT = (
    REPO
    / "generated"
    / "arcade-recompiled"
    / "GranTurismo2ArcadePC.csproj"
)
OVERLAY0 = (
    REPO
    / "generated"
    / "arcade-recompiled"
    / "gt2_arcade_overlay_0.cs"
)
OVERLAY2 = (
    REPO
    / "generated"
    / "arcade-recompiled"
    / "gt2_arcade_overlay_2.cs"
)
OVERLAY4 = (
    REPO
    / "generated"
    / "arcade-recompiled"
    / "gt2_arcade_overlay_4.cs"
)
OVERLAY5 = (
    REPO
    / "generated"
    / "arcade-recompiled"
    / "gt2_arcade_overlay_5.cs"
)


def replace_once(
    path: Path, old: str, new: str, description: str
) -> None:
    source = path.read_text(encoding="utf-8")
    matches = source.count(old)
    if matches != 1:
        raise RuntimeError(
            f"{description}: expected one source match in {path}, "
            f"found {matches}"
        )
    path.write_text(source.replace(old, new, 1), encoding="utf-8")


def replace_exact_count(
    path: Path,
    old: str,
    new: str,
    expected_count: int,
    description: str,
) -> None:
    source = path.read_text(encoding="utf-8")
    matches = source.count(old)
    if matches != expected_count:
        raise RuntimeError(
            f"{description}: expected {expected_count} source matches in "
            f"{path}, found {matches}"
        )
    path.write_text(source.replace(old, new), encoding="utf-8")


def include_livery_preview_helper() -> None:
    source = PROJECT.read_text(encoding="utf-8")
    if "ArcadeLiveryPreview.cs" in source:
        return

    replace_once(
        PROJECT,
        """  <ItemGroup>
    <ProjectReference Include="..\\..\\vendor\\RecompOne\\RecompOne.Runtime\\RecompOne.Runtime.csproj" />
""",
        """  <ItemGroup>
    <Compile Include="..\\..\\tools\\unified-host\\ArcadeLiveryPreview.cs"
             Link="ArcadeLiveryPreview.cs" />
    <ProjectReference Include="..\\..\\vendor\\RecompOne\\RecompOne.Runtime\\RecompOne.Runtime.csproj" />
""",
        "Arcade alternate-livery preview helper project include",
    )


def apply_frontend_arena() -> None:
    replace_exact_count(
        OVERLAY2,
        "0x80130000u",
        "0x80410000u",
        18,
        "Expanded Arcade frontend address family",
    )
    replace_once(
        OVERLAY2,
        """        c.A2 = 0x00060000u;
        c.A2 = c.A2 | 0x6000u;
""",
        """        // The merged native car-logo archive and its adjacent
        // descriptors live in a dedicated devkit-RAM MiB.
        c.A2 = 0x000F0000u;
        c.A2 = c.A2 | 0x0000u;
""",
        "Expanded Arcade frontend archive bound",
    )


def apply_renderer_enhancements() -> None:
    replace_once(
        OVERLAY0,
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
        "Arcade maximum vehicle LOD",
    )
    replace_once(
        MAIN,
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
        "Arcade vehicle LOD and world-capture tracing",
    )
    replace_once(
        MAIN,
        """        m.WriteU32((c.SP + 0x18u), c.S2);
        c.S2 = c.A2 + 0u;
        m.WriteU32((c.SP + 0x24u), c.S5);
""",
        """        m.WriteU32((c.SP + 0x18u), c.S2);
        c.S2 = c.A2 + 0u;
        RecompOne.Runtime.Sdk.GT2Compat.TraceWheelTransform(
            c.S6, c.S2, m.ReadU32(c.SP + 0x4Cu), m);
        m.WriteU32((c.SP + 0x24u), c.S5);
""",
        "Arcade wheel transform tracing hook",
    )
    replace_once(
        MAIN,
        """        L80067704: ;
        c.RA = m.ReadU32((c.SP + 0x5Cu));
""",
        """        L80067704: ;
        RecompOne.Runtime.WorldCaptureContext.EndObject();
        c.RA = m.ReadU32((c.SP + 0x5Cu));
""",
        "Arcade vehicle world-capture object end",
    )
    replace_once(
        OVERLAY0,
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
            // Arcade's merged parameter database owns 0x80200000-0x802FFFFF.
            // Keep renderer geometry in its own non-overlapping devkit arena.
            c.A1 = 0x80500000u;
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
        "Arcade expanded polygon buffers",
    )
    replace_once(
        OVERLAY0,
        """    public static void func_8002009C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x1080u;
""",
        """    public static void func_8002009C(CpuContext c, IMemory m)
    {
        RecompOne.Runtime.Sdk.GT2Compat.TraceTrackVisibility(c, m);
        c.SP = c.SP - 0x1080u;
""",
        "Arcade race track visibility tracing",
    )
    replace_once(
        OVERLAY0,
        """        c.FP = 0x00630000u;
""",
        """        c.FP =
            RecompOne.Runtime.Sdk.GT2Compat.GetTrackDrawDistanceLimit();
""",
        "Arcade extended radial track distance",
    )
    replace_once(
        OVERLAY0,
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
        "Arcade track visibility and LOD policy",
    )
    replace_once(
        OVERLAY0,
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
        "Arcade track world-capture identity",
    )
    replace_once(
        OVERLAY0,
        """        c.S0 = m.ReadU32((c.S6 + 0x4u));
        c.S1 = m.ReadU16((c.S6 + 0xCu));
""",
        """        c.S0 = m.ReadU32((c.S6 + 0x4u));
        RecompOne.Runtime.WorldCaptureContext.BeginTrackObject(c.S6, c.S0);
        c.S1 = m.ReadU16((c.S6 + 0xCu));
""",
        "Arcade track world-capture object begin",
    )
    replace_once(
        OVERLAY0,
        """        c.RA = m.ReadU32((c.SP + 0x107Cu));
        c.FP = m.ReadU32((c.SP + 0x1078u));
""",
        """        RecompOne.Runtime.WorldCaptureContext.EndObject();
        c.RA = m.ReadU32((c.SP + 0x107Cu));
        c.FP = m.ReadU32((c.SP + 0x1078u));
""",
        "Arcade track world-capture object end",
    )
    replace_exact_count(
        OVERLAY0,
        """        c.A0 = RecompOne.Runtime.Gte.Read(24);
        c.V1 = RecompOne.Runtime.Gte.Read(14);
""",
        """        c.A0 = RecompOne.Runtime.Gte.Read(24);
        c.V1 = RecompOne.Runtime.Gte.Read(14);
        RecompOne.Runtime.Gte.BeginDerivedScreenProjection(
            m.ReadU32(c.S2 + 0x8u) == 0x31525353u,
            c.V1);
""",
        2,
        "Arcade auxiliary billboard projection begin hooks",
    )
    for label in ("L80020954", "L80020D6C"):
        replace_once(
            OVERLAY0,
            f"""        m.WriteU32((c.At + 0x68u), c.T7);
        {label}: ;
""",
            f"""        m.WriteU32((c.At + 0x68u), c.T7);
        RecompOne.Runtime.Gte.EndDerivedScreenProjection();
        {label}: ;
""",
            f"Arcade auxiliary billboard projection end hook {label}",
        )
    replace_once(
        OVERLAY2,
        """        L800203F8: ;
        c.A0 = m.ReadU32((c.SP + 0x64u));
""",
        """        L800203F8: ;
        if (RecompOne.Runtime.Sdk.GT2Compat.ExtendedReplayTrackDrawDistanceEnabled) {
            c.S0 = 0x00000001u;
            goto L800204A0;
        }
        c.A0 = m.ReadU32((c.SP + 0x64u));
""",
        "Arcade replay track draw distance",
    )


def apply_livery_preview_reload() -> None:
    replace_once(
        OVERLAY2,
        """        m.WriteU32((c.SP + 0x214u), c.RA);
        c.V0 = (uint)(sbyte)m.ReadU8((c.S0 + 0xDu));
        L80015DD4: ;
        c.A0 = c.SP + 0x10u;
        c.A2 = 0u + 0u;
        c.A1 = c.V0 << 3;
""",
        """        m.WriteU32((c.SP + 0x214u), c.RA);
        c.V0 = (uint)(sbyte)m.ReadU8((c.S0 + 0xDu));
        L80015DD4: ;
        uint targetLiveryPalette = c.V0;
        c.V0 = (uint)(
            RecompOne.Runtime.Sdk.GT2Compat.ResolveLiveryBodyAndPalette(
                m.ReadU32((c.S0 + 0x8u)), targetLiveryPalette) >> 32);
        c.A0 = c.SP + 0x10u;
        c.A2 = 0u + 0u;
        c.A1 = c.V0 << 3;
""",
        "Arcade preview alternate-body local palette upload",
    )
    replace_once(
        OVERLAY2,
        """        m.WriteU8((c.S0 + 0xDu), (byte)c.V0);
        c.V0 = m.ReadU32((c.S0 + 0x44u));
""",
        """        m.WriteU8((c.S0 + 0xDu), (byte)targetLiveryPalette);
        c.V0 = m.ReadU32((c.S0 + 0x44u));
""",
        "Arcade preview customer-facing palette persistence",
    )
    replace_once(
        OVERLAY2,
        """        c.V0 = m.ReadU32((c.A0 + 0x44u));
        c.V0 = m.ReadU16(c.V0);
        return;
""",
        """        c.V0 = m.ReadU32((c.A0 + 0x44u));
        c.V0 = RecompOne.Runtime.Sdk.GT2Compat.ResolveLiveryChoiceCount(
            m.ReadU32((c.A0 + 0x8u)), m.ReadU16(c.V0));
        return;
""",
        "Arcade preview extended livery choice count",
    )
    replace_once(
        OVERLAY2,
        """        c.A1 = 0xFFFFFFFFu;
        c.RA = 0x8001ECB4u;
        GranTurismo2ArcadePC.func_80015E9C(c, m);
        c.V0 = c.S0 & 0x0002u;
""",
        """        c.A1 = 0xFFFFFFFFu;
        c.RA = 0x8001ECB4u;
        GranTurismo2ArcadePC.func_80015E9C(c, m);
        GranTurismo2ArcadePC.ReloadLiveryPreview(
            c, m, m.ReadU32((c.S5 + 0x228u)), c.S5);
        c.V0 = c.S0 & 0x0002u;
""",
        "Arcade one-player previous-livery body reload",
    )
    replace_once(
        OVERLAY2,
        """        c.A1 = 0x00000001u;
        c.RA = 0x8001ECCCu;
        GranTurismo2ArcadePC.func_80015E9C(c, m);
        c.V0 = c.S0 & 0x0003u;
""",
        """        c.A1 = 0x00000001u;
        c.RA = 0x8001ECCCu;
        GranTurismo2ArcadePC.func_80015E9C(c, m);
        GranTurismo2ArcadePC.ReloadLiveryPreview(
            c, m, m.ReadU32((c.S5 + 0x228u)), c.S5);
        c.V0 = c.S0 & 0x0003u;
""",
        "Arcade one-player next-livery body reload",
    )
    replace_once(
        OVERLAY2,
        """        c.A1 = 0xFFFFFFFFu;
        c.RA = 0x80021AC8u;
        GranTurismo2ArcadePC.func_80015E9C(c, m);
        L80021AC8: ;
""",
        """        c.A1 = 0xFFFFFFFFu;
        c.RA = 0x80021AC8u;
        GranTurismo2ArcadePC.func_80015E9C(c, m);
        GranTurismo2ArcadePC.ReloadLiveryPreview(
            c,
            m,
            m.ReadU32(
                (m.ReadU32((c.SP + 0x70u)) + c.S5 * 4u) + 0x228u),
            m.ReadU32((c.SP + 0x70u)));
        L80021AC8: ;
""",
        "Arcade two-player previous-livery body reload",
    )
    replace_once(
        OVERLAY2,
        """        c.A1 = 0x00000001u;
        c.RA = 0x80021AECu;
        GranTurismo2ArcadePC.func_80015E9C(c, m);
        L80021AEC: ;
""",
        """        c.A1 = 0x00000001u;
        c.RA = 0x80021AECu;
        GranTurismo2ArcadePC.func_80015E9C(c, m);
        GranTurismo2ArcadePC.ReloadLiveryPreview(
            c,
            m,
            m.ReadU32(
                (m.ReadU32((c.SP + 0x70u)) + c.S5 * 4u) + 0x228u),
            m.ReadU32((c.SP + 0x70u)));
        L80021AEC: ;
""",
        "Arcade two-player next-livery body reload",
    )


def main() -> int:
    include_livery_preview_helper()

    replace_once(
        OVERLAY0,
        """        c.V1 = m.ReadU8((c.FP + 0x8u));
        c.V0 = 0x800B0000u;
        m.WriteU8((c.V0 - 0x75A0u), (byte)0u);
        m.WriteU32((c.S4 + 0x18u), c.V1);
""",
        """        RecompOne.Runtime.Sdk.GT2Compat.ConfigureTrue60HzRaceTimeStep(
            c.FP, m);
        c.V1 = m.ReadU8((c.FP + 0x8u));
        c.V0 = 0x800B0000u;
        m.WriteU8((c.V0 - 0x75A0u), (byte)0u);
        m.WriteU32((c.S4 + 0x18u), c.V1);
""",
        "Arcade race NTSC time step",
    )

    replace_once(
        OVERLAY0,
        """        c.A1 = (uint)((int)c.A1 >> 8);
        c.A2 = (uint)((int)c.A2 >> 8);
        c.A3 = (uint)((int)c.A3 >> 8);
""",
        """        c.A1 = (uint)((int)c.A1 >> RecompOne.Runtime.Sdk.GT2Compat.GetTrue60HzVehicleIntegrationShift(8));
        c.A2 = (uint)((int)c.A2 >> RecompOne.Runtime.Sdk.GT2Compat.GetTrue60HzVehicleIntegrationShift(8));
        c.A3 = (uint)((int)c.A3 >> RecompOne.Runtime.Sdk.GT2Compat.GetTrue60HzVehicleIntegrationShift(8));
""",
        "Arcade vehicle position integration time step",
    )
    replace_once(
        OVERLAY0,
        """        c.RA = 0x8003AFF0u;
        GranTurismo2ArcadePC.func_800759A4(c, m);
        c.V1 = m.ReadU32((c.S4 + 0x64Cu));
""",
        """        c.RA = 0x8003AFF0u;
        GranTurismo2ArcadePC.func_800759A4(c, m);
        c.V0 = RecompOne.Runtime.Sdk.GT2Compat.ScaleTrue60HzVehicleDelta(c.V0);
        c.V1 = m.ReadU32((c.S4 + 0x64Cu));
""",
        "Arcade longitudinal acceleration integration time step",
    )
    replace_once(
        OVERLAY0,
        """        L80034488: ;
        c.A0 = c.S6 + 0u;
        c.A1 = c.S5 + 0u;
        c.RA = 0x80034494u;
        GranTurismo2ArcadePC.func_800342CC(c, m);
""",
        """        L80034488: ;
        c.A0 = c.S6 + 0u;
        c.A1 = c.S5 + 0u;
        RecompOne.Runtime.Sdk.GT2Compat.BeginTrue60HzLinearVelocityStep(
            c.S6, c.S5, m);
        c.RA = 0x80034494u;
        GranTurismo2ArcadePC.func_800342CC(c, m);
        RecompOne.Runtime.Sdk.GT2Compat.EndTrue60HzLinearVelocityStep(
            c.S6, c.S5, m);
""",
        "Arcade linear velocity integration time step",
    )
    replace_once(
        OVERLAY0,
        """        c.RA = 0x80045C38u;
        GranTurismo2ArcadePC.func_800759A4(c, m);
        c.V1 = m.ReadU32((c.S1 + 0x624u));
""",
        """        c.RA = 0x80045C38u;
        GranTurismo2ArcadePC.func_800759A4(c, m);
        c.V0 = RecompOne.Runtime.Sdk.GT2Compat.ScaleTrue60HzVehicleDelta(c.V0);
        c.V1 = m.ReadU32((c.S1 + 0x624u));
""",
        "Arcade secondary force accumulator time step",
    )
    replace_once(
        OVERLAY0,
        """        c.RA = 0x8003B228u;
        GranTurismo2ArcadePC.func_8007587C(c, m);
        c.S0 = c.S0 + c.V0;
        c.V1 = m.ReadU32((c.S3 + 0x628u));
""",
        """        c.RA = 0x8003B228u;
        GranTurismo2ArcadePC.func_8007587C(c, m);
        c.S0 = c.S0 + c.V0;
        c.S0 = RecompOne.Runtime.Sdk.GT2Compat.ScaleTrue60HzVehicleDelta(c.S0);
        c.V1 = m.ReadU32((c.S3 + 0x628u));
""",
        "Arcade wheel-pair force time step",
    )
    replace_once(
        OVERLAY0,
        """        c.V0 = m.ReadU32(c.S1);
        c.V1 = (uint)((int)c.V1 >> 1);
        c.V0 = m.ReadU32(c.V0);
""",
        """        c.V0 = m.ReadU32(c.S1);
        c.V1 = (uint)((int)c.V1 >> 1);
        c.V1 = RecompOne.Runtime.Sdk.GT2Compat.ScaleTrue60HzVehicleDelta(c.V1);
        c.V0 = m.ReadU32(c.V0);
""",
        "Arcade wheel-speed integration time step",
    )
    replace_once(
        OVERLAY0,
        """        c.V1 = m.ReadU32((c.S2 + 0x634u));
        c.V0 = (uint)((int)c.V0 >> 1);
        c.A1 = c.V1 + c.V0;
""",
        """        c.V1 = m.ReadU32((c.S2 + 0x634u));
        c.V0 = (uint)((int)c.V0 >> 1);
        c.V0 = RecompOne.Runtime.Sdk.GT2Compat.ScaleTrue60HzVehicleDelta(c.V0);
        c.A1 = c.V1 + c.V0;
""",
        "Arcade driven-wheel recurrence time step",
    )
    replace_once(
        OVERLAY0,
        """        m.WriteU32((c.SP + 0x14u), c.S1);
        c.S2 = m.ReadU8((c.S0 + 0x5D2Du));
        c.A0 = c.S3 + 0u;
""",
        """        m.WriteU32((c.SP + 0x14u), c.S1);
        c.S2 = m.ReadU8((c.S0 + 0x5D2Du));
        RecompOne.Runtime.Sdk.GT2Compat.TraceTrue60HzVehicleStage(
            "begin", c.S3, c.S2, m);
        c.A0 = c.S3 + 0u;
""",
        "Arcade true-60 vehicle state diagnostic",
    )

    replace_once(
        ENTRY,
        """        Dispatcher.Call(c, m, 0x8005D570u);
""",
        """        RecompOne.Runtime.Sdk.GT2Compat.RunGuestLoop(
            c, m, 0x8005D570u, "gt2_arcade_overlay");
""",
        "Arcade non-local overlay transition trampoline",
    )
    replace_once(
        MAIN,
        """        c.A0 = 0x00000005u;
        c.RA = 0x8005D678u;
""",
        """        c.A0 = RecompOne.Runtime.Sdk.GT2Compat.InitialArcadeOverlayIndex();
        c.RA = 0x8005D678u;
""",
        "Unified-menu direct native Arcade frontend entry",
    )
    replace_once(
        MAIN,
        """        c.RA = 0x80010EB4u;
        GranTurismo2ArcadePC.func_8007A014(c, m);
        c.RA = m.ReadU32((c.SP + 0x10u));
""",
        """        c.RA = 0x80010EB4u;
        GranTurismo2ArcadePC.func_8007A014(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x80010EBCu;
        GranTurismo2ArcadePC.func_8007F740(c, m);
        c.RA = m.ReadU32((c.SP + 0x10u));
""",
        "Arcade CD DMA initialization",
    )
    replace_once(
        OVERLAY0,
        """        c.A0 = m.ReadU32(c.S1);
        c.S2 = m.ReadU32((c.S1 + 0x4u));
        c.RA = 0x80028E78u;
""",
        """        c.A0 = m.ReadU32(c.S1);
        c.S2 = m.ReadU32((c.S1 + 0x4u));
        RecompOne.Runtime.Sdk.GT2Compat.ResolveLiveryBodyAndPaletteA0S2(c);
        c.RA = 0x80028E78u;
""",
        "Arcade race alternate native livery body and palette",
    )
    replace_once(
        OVERLAY2,
        """        c.S0 = m.ReadU32((c.V0 + 0x8Cu));
        c.S3 = m.ReadU32((c.V0 + 0x4u));
        c.S5 = m.ReadU32((c.V0 + 0x8u));
        c.A2 = c.S0 + 0u;
""",
        """        c.S0 = m.ReadU32((c.V0 + 0x8Cu));
        c.S3 = m.ReadU32((c.V0 + 0x4u));
        c.S5 = m.ReadU32((c.V0 + 0x8u));
        c.S0 = RecompOne.Runtime.Sdk.GT2Compat.ResolveLiveryBodyForColorId(
            c.S0, c.S3);
        c.A2 = c.S0 + 0u;
""",
        "Arcade frontend alternate native livery body",
    )
    replace_once(
        OVERLAY2,
        """        c.V1 = m.ReadU32((c.S5 + 0xA8u));
        c.A3 = c.S3 + 0x3u;
        m.WriteU32((c.SP + 0x18u), c.V0);
        m.WriteU32((c.SP + 0x1Cu), c.S1);
        m.WriteU32((c.SP + 0x14u), c.V1);
""",
        """        c.V1 = RecompOne.Runtime.Sdk.GT2Compat.ResolveLiveryBodyForPalette(
            m.ReadU32((c.S5 + 0xA8u)),
            (uint)(short)m.ReadU16((c.S4 + 0xB2u)));
        c.A3 = c.S3 + 0x3u;
        m.WriteU32((c.SP + 0x18u), c.V0);
        m.WriteU32((c.SP + 0x1Cu), c.S1);
        m.WriteU32((c.SP + 0x14u), c.V1);
""",
        "Arcade frontend primary record palette-index livery body",
    )
    replace_once(
        OVERLAY2,
        """        c.V1 = m.ReadU32((c.FP + 0x10u));
        c.A3 = 0x00000003u;
        m.WriteU32((c.SP + 0x18u), c.V0);
        m.WriteU32((c.SP + 0x1Cu), c.S0);
        m.WriteU32((c.SP + 0x14u), c.V1);
""",
        """        c.V1 = RecompOne.Runtime.Sdk.GT2Compat.ResolveLiveryBodyForPalette(
            m.ReadU32((c.FP + 0x10u)),
            (uint)(short)m.ReadU16((c.FP + 0x16u)));
        c.A3 = 0x00000003u;
        m.WriteU32((c.SP + 0x18u), c.V0);
        m.WriteU32((c.SP + 0x1Cu), c.S0);
        m.WriteU32((c.SP + 0x14u), c.V1);
""",
        "Arcade frontend secondary record palette-index livery body",
    )
    replace_once(
        OVERLAY2,
        """        c.V0 = m.ReadU32((c.FP + 0x10u));
        m.WriteU32((c.SP + 0x128u), c.V0);
        c.A0 = m.ReadU32((c.FP + 0x10u));
        c.A1 = (uint)(short)m.ReadU16((c.FP + 0x16u));
""",
        """        c.V0 = RecompOne.Runtime.Sdk.GT2Compat.ResolveLiveryBodyForPalette(
            m.ReadU32((c.FP + 0x10u)),
            (uint)(short)m.ReadU16((c.FP + 0x16u)));
        m.WriteU32((c.SP + 0x128u), c.V0);
        c.A0 = m.ReadU32((c.FP + 0x10u));
        c.A1 = (uint)(short)m.ReadU16((c.FP + 0x16u));
""",
        "Arcade frontend temporary record palette-index livery body",
    )
    replace_once(
        OVERLAY4,
        """        c.S6 = c.A0 + 0u;
        m.WriteU32((c.SP + 0xBCu), c.S1);
        c.S1 = c.A2 + 0u;
        m.WriteU32((c.SP + 0xCCu), c.S5);
        c.S5 = c.A3 + 0u;
        m.WriteU32((c.SP + 0xE4u), c.A1);
        c.A0 = c.A1 + 0u;
""",
        """        c.S6 = c.A0 + 0u;
        RecompOne.Runtime.Sdk.GT2Compat.ResolveLiveryBodyAndPaletteA1A2(c);
        m.WriteU32((c.SP + 0xBCu), c.S1);
        c.S1 = c.A2 + 0u;
        m.WriteU32((c.SP + 0xCCu), c.S5);
        c.S5 = c.A3 + 0u;
        m.WriteU32((c.SP + 0xE4u), c.A1);
        c.A0 = c.A1 + 0u;
""",
        "Arcade showroom and replay alternate native livery body and palette",
    )
    apply_livery_preview_reload()
    replace_once(
        OVERLAY0,
        """    public static void func_8003E070(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
""",
        """    public static void func_8003E070(CpuContext c, IMemory m)
    {
        RecompOne.Runtime.Sdk.GT2Compat.ApplyAiAutoDrive(
            c.A0, c.A1, m);
        RecompOne.Runtime.Sdk.GT2Compat.TraceAiDriverTable(
            c.A0, c.A1, m);
        c.V0 = 0x800B0000u;
""",
        "Arcade AI driver dispatch tracing and soak harness hook",
    )
    replace_once(
        OVERLAY2,
        """        c.A0 = 0x80100000u;
        c.A0 = c.A0 - 0x7B40u;
        c.A1 = 0u | 0xB000u;
""",
        """        // The unified runtime reserves one MiB of devkit guest RAM
        // for the persistent merged Arcade parameter database.
        c.A0 = 0x80200000u;
        c.A1 = 0u | 0xB000u;
""",
        "Expanded Arcade parameter database arena",
    )
    apply_frontend_arena()
    apply_renderer_enhancements()
    replace_once(
        OVERLAY2,
        """        L800266F4: ;
        c.V0 = m.ReadU8((c.S0 + 0x2Cu));
""",
        """        L800266F4: ;
        // MDEC DMA completes asynchronously on the PlayStation. Service the
        // queued hardware completion while the guest polls its decoder state.
        RecompOne.Runtime.Runtime.DrainDeferredIrqs();
        c.V0 = m.ReadU8((c.S0 + 0x2Cu));
""",
        "Arcade course-preview MDEC DMA completion wait",
    )
    for original, extended, label in (
        ("730", "33C0", "road-race forward"),
        ("9F0", "36A0", "road-race reverse"),
        ("CB0", "3980", "time-trial forward"),
        ("FB0", "3CA0", "time-trial reverse"),
        ("13F0", "3FC0", "two-player road"),
    ):
        replace_once(
            OVERLAY2,
            f"""        c.A0 = 0x80050000u;
        c.A0 = c.A0 + 0x{original}u;
""",
            f"""        c.A0 = 0x80050000u;
        c.A0 = c.A0 + 0x{extended}u;
        RecompOne.Runtime.Sdk.GT2Compat.UnlockArcadeCourseTable(c.A0, m);
""",
            f"GT1 SSR11 native {label} course table",
        )
    for register, original, extended, label in (
        ("c.T1 = c.V1", "730", "33C0", "road-race forward selection"),
        ("c.T0 = c.V1", "9F0", "36A0", "road-race reverse selection"),
        ("c.T1 = c.V1", "CB0", "3980", "time-trial forward selection"),
        ("c.T0 = c.V1", "FB0", "3CA0", "time-trial reverse selection"),
        ("c.T0 = c.A0", "13F0", "3FC0", "two-player road selection"),
    ):
        replace_once(
            OVERLAY2,
            f"        {register} + 0x{original}u;\n",
            f"        {register} + 0x{extended}u;\n",
            f"GT1 SSR11 native {label} table lookup",
        )
    replace_once(
        OVERLAY5,
        """    public static void func_800101B4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
""",
        """    public static void func_800101B4(CpuContext c, IMemory m)
    {
        RecompOne.Runtime.Sdk.GT2Compat.ServiceCdDevice(c, m);
        c.SP = c.SP - 0x20u;
""",
        "Arcade opening-overlay asynchronous CD status poll",
    )
    replace_once(
        OVERLAY5,
        """    public static void func_800104C4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
""",
        """    public static void func_800104C4(CpuContext c, IMemory m)
    {
        RecompOne.Runtime.Runtime.BeginIrqDeferral();
        c.SP = c.SP - 0x28u;
""",
        "Arcade MDEC output GPU-DMA interrupt ordering begin",
    )
    replace_once(
        OVERLAY5,
        """        c.S1 = m.ReadU32((c.SP + 0x14u));
        c.S0 = m.ReadU32((c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800105AC(CpuContext c, IMemory m)
""",
        """        c.S1 = m.ReadU32((c.SP + 0x14u));
        c.S0 = m.ReadU32((c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        RecompOne.Runtime.Runtime.EndIrqDeferral();
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800105AC(CpuContext c, IMemory m)
""",
        "Arcade MDEC output GPU-DMA interrupt ordering end",
    )
    print("Applied Arcade guest control-flow enhancements")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
