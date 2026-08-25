#!/usr/bin/env python3
"""Apply configuration-aware GT2 race-overlay enhancements after recompilation."""

from pathlib import Path


REPO = Path(__file__).resolve().parents[1]
GENERATED = REPO / "generated" / "recompiled"
PROJECT = GENERATED / "GranTurismo2PC.csproj"


def replace_once(path: Path, old: str, new: str, description: str) -> None:
    source = path.read_text(encoding="utf-8")
    matches = source.count(old)
    if matches != 1:
        raise RuntimeError(
            f"{description}: expected one source match in {path}, found {matches}"
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


def apply_auxiliary_billboard_projection(race: Path) -> None:
    replace_exact_count(
        race,
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
        "Simulation auxiliary billboard projection begin hooks",
    )
    for label in ("L800209C8", "L80020DE0"):
        replace_once(
            race,
            f"""        m.WriteU32((c.At + 0x68u), c.T7);
        {label}: ;
""",
            f"""        m.WriteU32((c.At + 0x68u), c.T7);
        RecompOne.Runtime.Gte.EndDerivedScreenProjection();
        {label}: ;
""",
            f"Simulation auxiliary billboard projection end hook {label}",
        )


def include_livery_preview_helper() -> None:
    if "SimulationLiveryPreview.cs" in PROJECT.read_text(encoding="utf-8"):
        return
    replace_once(
        PROJECT,
        """  <ItemGroup>
    <ProjectReference Include="..\\..\\vendor\\RecompOne\\RecompOne.Runtime\\RecompOne.Runtime.csproj" />
""",
        """  <ItemGroup>
    <Compile Include="..\\..\\tools\\unified-host\\SimulationLiveryPreview.cs"
             Link="SimulationLiveryPreview.cs" />
    <ProjectReference Include="..\\..\\vendor\\RecompOne\\RecompOne.Runtime\\RecompOne.Runtime.csproj" />
""",
        "Simulation alternate-livery preview helper project include",
    )


def include_bundled_native_renderer() -> None:
    source = PROJECT.read_text(encoding="utf-8")
    property_present = "IncludeNativeLibrariesForSelfExtract" in source
    item_present = "ExcludeFromSingleFile=\"false\"" in source
    if property_present and item_present:
        return
    if property_present or item_present:
        raise RuntimeError(
            "native renderer single-file project configuration is incomplete"
        )
    replace_once(
        PROJECT,
        """    <AssemblyName>GranTurismo2PC</AssemblyName>
""",
        """    <AssemblyName>GranTurismo2PC</AssemblyName>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
""",
        "single-file native renderer extraction property",
    )
    replace_once(
        PROJECT,
        """    <ProjectReference Include="..\\..\\vendor\\RecompOne\\RecompOne.Runtime\\RecompOne.Runtime.csproj" />
  </ItemGroup>
""",
        """    <ProjectReference Include="..\\..\\vendor\\RecompOne\\RecompOne.Runtime\\RecompOne.Runtime.csproj" />
    <None Include="..\\..\\build\\native\\Release\\opengt_live_renderer.dll"
          Condition="Exists('..\\..\\build\\native\\Release\\opengt_live_renderer.dll')"
          Link="opengt_live_renderer.dll"
          CopyToOutputDirectory="PreserveNewest"
          CopyToPublishDirectory="PreserveNewest"
          ExcludeFromSingleFile="false" />
  </ItemGroup>
""",
        "single-file native renderer project item",
    )


def preload_bundled_window_dependencies(program: Path) -> None:
    source = program.read_text(encoding="utf-8")
    if 'PreloadBundledNative("glfw3.dll");' in source:
        return
    replace_once(
        program,
        'PreloadBundledNative("SDL2.dll");\n',
        'PreloadBundledNative("glfw3.dll");\n'
        'PreloadBundledNative("cimgui.dll");\n'
        'PreloadBundledNative("SDL2.dll");\n',
        "bundled GLFW and cimgui native preloads",
    )


def use_windows_gui_subsystem() -> None:
    source = PROJECT.read_text(encoding="utf-8")
    if "<OutputType>WinExe</OutputType>" in source:
        return
    replace_once(
        PROJECT,
        "    <OutputType>Exe</OutputType>\n",
        "    <OutputType>WinExe</OutputType>\n",
        "Windows GUI executable subsystem",
    )


def apply_livery_preview_reload(track: Path) -> None:
    replace_once(
        track,
        """        m.WriteU32((c.SP + 0x214u), c.RA);
        c.V0 = (uint)(sbyte)m.ReadU8((c.S0 + 0xDu));
        L80015DF0: ;
        c.A0 = c.SP + 0x10u;
        c.A2 = 0u + 0u;
        c.A1 = c.V0 << 3;
""",
        """        m.WriteU32((c.SP + 0x214u), c.RA);
        c.V0 = (uint)(sbyte)m.ReadU8((c.S0 + 0xDu));
        L80015DF0: ;
        uint targetLiveryPalette = c.V0;
        c.V0 = (uint)(
            RecompOne.Runtime.Sdk.GT2Compat.ResolveLiveryBodyAndPalette(
                m.ReadU32((c.S0 + 0x8u)), targetLiveryPalette) >> 32);
        c.A0 = c.SP + 0x10u;
        c.A2 = 0u + 0u;
        c.A1 = c.V0 << 3;
""",
        "Simulation preview alternate-body local palette upload",
    )
    replace_once(
        track,
        """        m.WriteU8((c.S0 + 0xDu), (byte)c.V0);
        c.V0 = m.ReadU32((c.S0 + 0x44u));
""",
        """        m.WriteU8((c.S0 + 0xDu), (byte)targetLiveryPalette);
        c.V0 = m.ReadU32((c.S0 + 0x44u));
""",
        "Simulation preview customer-facing palette persistence",
    )
    replace_once(
        track,
        """        c.V0 = m.ReadU32((c.A0 + 0x44u));
        c.V0 = m.ReadU16(c.V0);
        return;
""",
        """        c.V0 = m.ReadU32((c.A0 + 0x44u));
        c.V0 = RecompOne.Runtime.Sdk.GT2Compat.ResolveLiveryChoiceCount(
            m.ReadU32((c.A0 + 0x8u)), m.ReadU16(c.V0));
        return;
""",
        "Simulation preview extended livery choice count",
    )
    replace_once(
        track,
        """        c.A1 = 0xFFFFFFFFu;
        c.RA = 0x8001ECD0u;
        GranTurismo2PC.func_80015EB8(c, m);
        c.V0 = c.S0 & 0x0002u;
""",
        """        c.A1 = 0xFFFFFFFFu;
        c.RA = 0x8001ECD0u;
        GranTurismo2PC.func_80015EB8(c, m);
        GranTurismo2PC.ReloadLiveryPreview(
            c, m, m.ReadU32((c.S5 + 0x228u)), c.S5);
        c.V0 = c.S0 & 0x0002u;
""",
        "Simulation one-player previous-livery body reload",
    )
    replace_once(
        track,
        """        c.A1 = 0x00000001u;
        c.RA = 0x8001ECE8u;
        GranTurismo2PC.func_80015EB8(c, m);
        c.V0 = c.S0 & 0x0003u;
""",
        """        c.A1 = 0x00000001u;
        c.RA = 0x8001ECE8u;
        GranTurismo2PC.func_80015EB8(c, m);
        GranTurismo2PC.ReloadLiveryPreview(
            c, m, m.ReadU32((c.S5 + 0x228u)), c.S5);
        c.V0 = c.S0 & 0x0003u;
""",
        "Simulation one-player next-livery body reload",
    )
    replace_once(
        track,
        """        c.A1 = 0xFFFFFFFFu;
        c.RA = 0x80021AE4u;
        GranTurismo2PC.func_80015EB8(c, m);
        L80021AE4: ;
""",
        """        c.A1 = 0xFFFFFFFFu;
        c.RA = 0x80021AE4u;
        GranTurismo2PC.func_80015EB8(c, m);
        GranTurismo2PC.ReloadLiveryPreview(
            c,
            m,
            m.ReadU32(
                (m.ReadU32((c.SP + 0x70u)) + c.S5 * 4u) + 0x228u),
            m.ReadU32((c.SP + 0x70u)));
        L80021AE4: ;
""",
        "Simulation two-player previous-livery body reload",
    )
    replace_once(
        track,
        """        c.A1 = 0x00000001u;
        c.RA = 0x80021B08u;
        GranTurismo2PC.func_80015EB8(c, m);
        L80021B08: ;
""",
        """        c.A1 = 0x00000001u;
        c.RA = 0x80021B08u;
        GranTurismo2PC.func_80015EB8(c, m);
        GranTurismo2PC.ReloadLiveryPreview(
            c,
            m,
            m.ReadU32(
                (m.ReadU32((c.SP + 0x70u)) + c.S5 * 4u) + 0x228u),
            m.ReadU32((c.SP + 0x70u)));
        L80021B08: ;
""",
        "Simulation two-player next-livery body reload",
    )


def main() -> int:
    race = GENERATED / "gt2_overlay_0.cs"
    track = GENERATED / "gt2_overlay_2.cs"
    title = GENERATED / "gt2_overlay_1.cs"
    main_executable = GENERATED / "main.cs"
    showroom = GENERATED / "gt2_overlay_4.cs"
    entry = GENERATED / "Entry.cs"
    program = GENERATED / "Program.cs"

    include_livery_preview_helper()
    include_bundled_native_renderer()
    preload_bundled_window_dependencies(program)
    use_windows_gui_subsystem()

    replace_once(
        race,
        """        c.V1 = m.ReadU8((c.FP + 0x8u));
        c.V0 = 0x800B0000u;
        m.WriteU8((c.V0 - 0x7298u), (byte)0u);
        m.WriteU32((c.S4 + 0x18u), c.V1);
""",
        """        RecompOne.Runtime.Sdk.GT2Compat.ConfigureTrue60HzRaceTimeStep(
            c.FP, m);
        c.V1 = m.ReadU8((c.FP + 0x8u));
        c.V0 = 0x800B0000u;
        m.WriteU8((c.V0 - 0x7298u), (byte)0u);
        m.WriteU32((c.S4 + 0x18u), c.V1);
""",
        "Simulation race NTSC time step",
    )

    replace_once(
        race,
        """        c.A1 = (uint)((int)c.A1 >> 8);
        c.A2 = (uint)((int)c.A2 >> 8);
        c.A3 = (uint)((int)c.A3 >> 8);
""",
        """        c.A1 = (uint)((int)c.A1 >> RecompOne.Runtime.Sdk.GT2Compat.GetTrue60HzVehicleIntegrationShift(8));
        c.A2 = (uint)((int)c.A2 >> RecompOne.Runtime.Sdk.GT2Compat.GetTrue60HzVehicleIntegrationShift(8));
        c.A3 = (uint)((int)c.A3 >> RecompOne.Runtime.Sdk.GT2Compat.GetTrue60HzVehicleIntegrationShift(8));
""",
        "Simulation vehicle position integration time step",
    )
    replace_once(
        race,
        """        c.RA = 0x8003B044u;
        GranTurismo2PC.func_80075A94(c, m);
        c.V1 = m.ReadU32((c.S4 + 0x64Cu));
""",
        """        c.RA = 0x8003B044u;
        GranTurismo2PC.func_80075A94(c, m);
        c.V0 = RecompOne.Runtime.Sdk.GT2Compat.ScaleTrue60HzVehicleDelta(c.V0);
        c.V1 = m.ReadU32((c.S4 + 0x64Cu));
""",
        "Simulation longitudinal acceleration integration time step",
    )
    replace_once(
        race,
        """        L800344DC: ;
        c.A0 = c.S6 + 0u;
        c.A1 = c.S5 + 0u;
        c.RA = 0x800344E8u;
        GranTurismo2PC.func_80034320(c, m);
""",
        """        L800344DC: ;
        c.A0 = c.S6 + 0u;
        c.A1 = c.S5 + 0u;
        RecompOne.Runtime.Sdk.GT2Compat.BeginTrue60HzLinearVelocityStep(
            c.S6, c.S5, m);
        c.RA = 0x800344E8u;
        GranTurismo2PC.func_80034320(c, m);
        RecompOne.Runtime.Sdk.GT2Compat.EndTrue60HzLinearVelocityStep(
            c.S6, c.S5, m);
""",
        "Simulation linear velocity integration time step",
    )
    replace_once(
        race,
        """        c.RA = 0x80045D18u;
        GranTurismo2PC.func_80075A94(c, m);
        c.V1 = m.ReadU32((c.S1 + 0x624u));
""",
        """        c.RA = 0x80045D18u;
        GranTurismo2PC.func_80075A94(c, m);
        c.V0 = RecompOne.Runtime.Sdk.GT2Compat.ScaleTrue60HzVehicleDelta(c.V0);
        c.V1 = m.ReadU32((c.S1 + 0x624u));
""",
        "Simulation secondary force accumulator time step",
    )
    replace_once(
        race,
        """        c.RA = 0x8003B27Cu;
        GranTurismo2PC.func_8007596C(c, m);
        c.S0 = c.S0 + c.V0;
        c.V1 = m.ReadU32((c.S3 + 0x628u));
""",
        """        c.RA = 0x8003B27Cu;
        GranTurismo2PC.func_8007596C(c, m);
        c.S0 = c.S0 + c.V0;
        c.S0 = RecompOne.Runtime.Sdk.GT2Compat.ScaleTrue60HzVehicleDelta(c.S0);
        c.V1 = m.ReadU32((c.S3 + 0x628u));
""",
        "Simulation wheel-pair force time step",
    )
    replace_once(
        race,
        """        c.V0 = m.ReadU32(c.S1);
        c.V1 = (uint)((int)c.V1 >> 1);
        c.V0 = m.ReadU32(c.V0);
""",
        """        c.V0 = m.ReadU32(c.S1);
        c.V1 = (uint)((int)c.V1 >> 1);
        c.V1 = RecompOne.Runtime.Sdk.GT2Compat.ScaleTrue60HzVehicleDelta(c.V1);
        c.V0 = m.ReadU32(c.V0);
""",
        "Simulation wheel-speed integration time step",
    )
    replace_once(
        race,
        """        c.V1 = m.ReadU32((c.S2 + 0x634u));
        c.V0 = (uint)((int)c.V0 >> 1);
        c.A1 = c.V1 + c.V0;
""",
        """        c.V1 = m.ReadU32((c.S2 + 0x634u));
        c.V0 = (uint)((int)c.V0 >> 1);
        c.V0 = RecompOne.Runtime.Sdk.GT2Compat.ScaleTrue60HzVehicleDelta(c.V0);
        c.A1 = c.V1 + c.V0;
""",
        "Simulation driven-wheel recurrence time step",
    )
    replace_once(
        race,
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
        "Simulation true-60 vehicle state diagnostic",
    )

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
        main_executable,
        """        L8006D8CC: ;
        c.V1 = (uint)(short)m.ReadU16((c.S2 + 0x26u));
""",
        """        L8006D8CC: ;
        if (RecompOne.Runtime.Sdk.GT2Compat.SuppressUnifiedTitleListDecorations(c.S2)) {
            goto L8006D998;
        }
        c.V1 = (uint)(short)m.ReadU16((c.S2 + 0x26u));
""",
        "Sony demo title 2x2 layout without vertical-list boundary arrows",
    )

    replace_once(
        title,
        """        c.A3 = (uint)(short)m.ReadU16((c.A1 + 0x10u));
        c.A2 = m.ReadU32(c.A1);
        c.V0 = (uint)(short)m.ReadU16(c.V1);
        c.S2 = m.ReadU8((c.A0 - 0x6720u));
""",
        """        c.A3 = (uint)(short)m.ReadU16((c.A1 + 0x10u));
        c.A2 = m.ReadU32(c.A1);
        c.V0 = (uint)RecompOne.Runtime.Sdk.GT2Compat.UnifiedTitleItemValidity(c.S1);
        c.S2 = m.ReadU8((c.A0 - 0x6720u));
""",
        "unified title item validity map",
    )

    replace_once(
        title,
        """        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x43A4u;
        c.A1 = c.S2 << 2;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x43ECu;
        c.V0 = c.S1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V1 = (uint)(short)m.ReadU16(c.V0);
        c.A1 = c.A1 + c.A0;
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V1 = m.ReadU32(c.A1);
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
""",
        """        c.V0 = RecompOne.Runtime.Sdk.GT2Compat.UnifiedTitleDescriptor(
            m, c.S1, c.S2);
""",
        "unified title TIM descriptor map",
    )

    replace_once(
        title,
        """    public static void func_8001792C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x80050000u;
""",
        """    public static void func_8001792C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        RecompOne.Runtime.Sdk.GT2Compat.InstallUnifiedTitleMenu(m);
        c.V0 = 0x80050000u;
""",
        "unified title list installation",
    )

    replace_once(
        title,
        """        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000001u;
        c.S0 = c.V0 + 0u;
        c.A2 = 0x00000006u;
        c.RA = 0x80017B28u;
""",
        """        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000001u;
        c.S0 = c.V0 + 0u;
        c.A2 = 0x00000004u;
        c.RA = 0x80017B28u;
""",
        "unified title navigation bounds",
    )

    replace_once(
        title,
        """        L800178D0: ;
        m.WriteU16((c.S0 + 0x12u), (ushort)c.A3);
        c.V0 = (uint)(short)m.ReadU16((c.A1 + 0xCu));
""",
        """        L800178D0: ;
        m.WriteU16((c.S0 + 0x12u), (ushort)c.A3);
        m.WriteU32(
            (c.S0 + 0xCu),
            RecompOne.Runtime.Sdk.GT2Compat.UnifiedTitleDescriptorForState(
                c.S1, c.A2));
        c.V0 = (uint)(short)m.ReadU16((c.A1 + 0xCu));
""",
        "unified title selected/unselected native sprite",
    )

    replace_once(
        title,
        """        m.WriteU16((c.A0 + 0x4u), (ushort)c.V0);
        m.WriteU16((c.A0 + 0x6u), (ushort)c.V1);
        c.A1 = m.ReadU32((c.A1 + 0x4u));
        c.RA = 0x800178F4u;
""",
        """        m.WriteU16((c.A0 + 0x4u), (ushort)c.V0);
        m.WriteU16((c.A0 + 0x6u), (ushort)c.V1);
        RecompOne.Runtime.Sdk.GT2Compat.PositionUnifiedTitleItem(
            m, c.A0, c.S1);
        c.A1 = m.ReadU32((c.A1 + 0x4u));
        c.RA = 0x800178F4u;
""",
        "unified title authored 2x2 positions",
    )

    replace_once(
        title,
        """        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x43FCu;
        c.V1 = c.S0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V0 = m.ReadU8(c.V1);
        m.WriteU8((c.A0 + 0x3u), (byte)c.V0);
""",
        """        RecompOne.Runtime.Sdk.GT2Compat.CommitUnifiedTitleSelection(
            m, c.S0, c.A0);
""",
        "unified title selection dispatch",
    )

    replace_once(
        race,
        """        c.A0 = m.ReadU32(c.S1);
        c.S2 = m.ReadU32((c.S1 + 0x4u));
        c.RA = 0x80028ECCu;
""",
        """        c.A0 = m.ReadU32(c.S1);
        c.S2 = m.ReadU32((c.S1 + 0x4u));
        RecompOne.Runtime.Sdk.GT2Compat.ResolveLiveryBodyAndPaletteA0S2(c);
        c.RA = 0x80028ECCu;
""",
        "race alternate native livery body and palette",
    )

    replace_once(
        track,
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
        "frontend alternate native livery body",
    )

    replace_once(
        track,
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
        "frontend primary record palette-index livery body",
    )

    replace_once(
        track,
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
        "frontend secondary record palette-index livery body",
    )

    replace_once(
        track,
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
        "frontend temporary record palette-index livery body",
    )

    replace_once(
        showroom,
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
        "showroom and replay alternate native livery body and palette",
    )
    apply_livery_preview_reload(track)

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
        "wheel transform tracing hook",
    )

    replace_once(
        main,
        """        L800677F4: ;
        c.RA = m.ReadU32((c.SP + 0x5Cu));
        c.FP = m.ReadU32((c.SP + 0x58u));
""",
        """        L800677F4: ;
        RecompOne.Runtime.WorldCaptureContext.EndObject();
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
        """        c.FP = 0x00630000u;
""",
        """        c.FP =
            RecompOne.Runtime.Sdk.GT2Compat.GetTrackDrawDistanceLimit();
""",
        "extended track radial draw distance",
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
        "track visibility and LOD policy",
    )

    replace_once(
        race,
        """        c.RA = 0x80020320u;
        GranTurismo2PC.func_80020EC4(c, m);
        c.V1 = c.V0 + 0u;
""",
        """        c.RA = 0x80020320u;
        GranTurismo2PC.func_80020EC4(c, m);
        c.V0 = RecompOne.Runtime.Sdk.GT2Compat.ExpandTrackFrustumClassification(
            c.V0);
        c.V1 = c.V0 + 0u;
""",
        "modern horizontal track frustum",
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
        race,
        """        c.A0 = m.ReadU32((c.SP + 0x1020u));
        c.RA = 0x800209E4u;
        GranTurismo2PC.func_8002106C(c, m);
""",
        """        c.A0 = m.ReadU32((c.SP + 0x1020u));
        RecompOne.Runtime.WorldCaptureContext.TraceTrackMesh(c.A0, m);
        c.RA = 0x800209E4u;
        GranTurismo2PC.func_8002106C(c, m);
""",
        "primary track mesh trace hook",
    )

    replace_once(
        race,
        """        c.A0 = m.ReadU32((c.SP + 0x1020u));
        c.RA = 0x800209F8u;
        GranTurismo2PC.func_800234F8(c, m);
""",
        """        c.A0 = m.ReadU32((c.SP + 0x1020u));
        RecompOne.Runtime.WorldCaptureContext.TraceTrackMesh(c.A0, m);
        c.RA = 0x800209F8u;
        GranTurismo2PC.func_800234F8(c, m);
""",
        "alternate track mesh trace hook",
    )

    apply_auxiliary_billboard_projection(race)

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
        if (RecompOne.Runtime.Sdk.GT2Compat.ExtendedReplayTrackDrawDistanceEnabled) {
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
