#!/usr/bin/env python3
"""Apply Arcade-specific guest control-flow fixes after recompilation."""

from pathlib import Path


REPO = Path(__file__).resolve().parents[1]
ENTRY = REPO / "generated" / "arcade-recompiled" / "Entry.cs"
MAIN = REPO / "generated" / "arcade-recompiled" / "main.cs"
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


def main() -> int:
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
