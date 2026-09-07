#!/usr/bin/env python3
"""Apply configuration-aware GT2 race-overlay enhancements after recompilation."""

from pathlib import Path

from rewrite_recompiled_memory_access import rewrite_tree


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


def replace_in_function_once(
    path: Path,
    function_name: str,
    old: str,
    new: str,
    description: str,
) -> None:
    source = path.read_text(encoding="utf-8")
    marker = (
        f"    public static void {function_name}(CpuContext c, IMemory m)\n"
    )
    if source.count(marker) != 1:
        raise RuntimeError(
            f"{description}: expected one {function_name} definition in "
            f"{path}, found {source.count(marker)}"
        )
    start = source.index(marker)
    end = source.find("\n    public static void ", start + len(marker))
    if end < 0:
        end = len(source)
    body = source[start:end]
    if body.count(old) != 1:
        raise RuntimeError(
            f"{description}: expected one source match in {function_name}, "
            f"found {body.count(old)}"
        )
    body = body.replace(old, new, 1)
    path.write_text(source[:start] + body + source[end:], encoding="utf-8")


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
    # Auxiliary flare cores are assembled from a projected world anchor using
    # CPU-side 16-bit packet writes. Preserve that anchor through construction
    # so the native renderer classifies the cores as track effects and applies
    # their authored ordering-table depth rather than drawing them as HUD.
    replace_once(
        race,
        """        c.A2 = RecompOne.Runtime.Gte.Read(24);
        c.V0 = RecompOne.Runtime.Gte.Read(14);
        c.A1 = c.A1 << (int)(c.A3 & 31u);
""",
        """        c.A2 = RecompOne.Runtime.Gte.Read(24);
        c.V0 = RecompOne.Runtime.Gte.Read(14);
        RecompOne.Runtime.Gte.BeginDerivedScreenProjection(c.V0);
        c.A1 = c.A1 << (int)(c.A3 & 31u);
""",
        "Simulation auxiliary flare projection begin hook",
    )
    replace_once(
        race,
        """        L8001FF88: ;
        c.S1 = c.S1 + 0x14u;
""",
        """        L8001FF88: ;
        RecompOne.Runtime.Gte.EndDerivedScreenProjection();
        c.S1 = c.S1 + 0x14u;
""",
        "Simulation auxiliary flare projection end hook",
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


def apply_car_preview_camera_limit(track: Path) -> None:
    replace_in_function_once(
        track,
        "func_8001E5D8",
        """        c.V0 = m.ReadU32((c.A0 + 0xA8u));
        c.V0 = c.V0 + c.V1;
        m.WriteU32((c.A0 + 0xA8u), c.V0);
""",
        """        c.V0 = m.ReadU32((c.A0 + 0xA8u));
        c.V0 = c.V0 + c.V1;
        c.V0 = RecompOne.Runtime.Sdk.GT2Compat.
            LimitCarPreviewCameraDistance(c.A0, c.V0);
        m.WriteU32((c.A0 + 0xA8u), c.V0);
""",
        "Simulation car-preview camera distance ceiling",
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

    replace_in_function_once(
        main_executable,
        "func_80081D64",
        """        c.V0 = MemoryAccess.ReadU16(m, c.A1);
        c.V1 = MemoryAccess.ReadU16(m, c.A1);
""",
        """        c.V0 = MemoryAccess.ReadU16(m, c.A1);
        // A real PS1 completes these adjacent counter loads before Timer 1 can
        // advance. Reuse the first sample so host wall-clock timer reads cannot
        // starve the guest's stable-sample loop during opening-movie startup.
        c.V1 = c.V0;
""",
        "Simulation stable Timer 1 sample for opening-movie startup",
    )
    replace_in_function_once(
        main_executable,
        "func_80080C94",
        """    {
        return;
""",
        """    {
        // This leaf is the false half of the adjacent false/true callback pair.
        // Its retail MIPS body leaves v0 undefined; static indirect dispatch has
        // just loaded this function's address into v0, so normalize false here.
        c.V0 = 0u;
        return;
""",
        "Simulation deterministic false callback return",
    )

    replace_in_function_once(
        main_executable,
        "func_80010E14",
        """        c.RA = 0x80010E84u;
        GranTurismo2PC.func_80010CEC(c, m);
""",
        """        c.RA = 0x80010E84u;
        if (RecompOne.Runtime.Sdk.GT2Compat.ShouldPresentSimulationBootPanels())
            GranTurismo2PC.func_80010CEC(c, m);
""",
        "Unified boot omits duplicate Simulation panels after original opening",
    )

    replace_in_function_once(
        showroom,
        "func_80013EEC",
        """    {
        c.SP = c.SP - 0x28u;
""",
        """    {
        RecompOne.Runtime.Sdk.GT2Compat.ReturnFromUnifiedGranTurismoRoot(
            c.A0, m);
        c.SP = c.SP - 0x28u;
""",
        "Unified Gran Turismo world-map reverse title handoff",
    )

    replace_in_function_once(
        race,
        "func_8002993C",
        "        GranTurismo2PC.func_80018D1C(c, m);\n",
        """        RecompOne.Runtime.WorldCaptureContext.BeginBackgroundObject(c.A2);
        GranTurismo2PC.func_80018D1C(c, m);
        RecompOne.Runtime.WorldCaptureContext.EndObject();
""",
        "Simulation authored background ownership",
    )
    replace_in_function_once(
        race,
        "func_800294D4",
        """        c.RA = 0x800296A8u;
        GranTurismo2PC.func_800298FC(c, m);
        c.A0 = 0x800B0000u;
""",
        """        RecompOne.Runtime.WorldCaptureContext.BeginScenePass(
            RecompOne.Runtime.WorldScenePass.Auxiliary);
        c.RA = 0x800296A8u;
        GranTurismo2PC.func_800298FC(c, m);
        RecompOne.Runtime.Sdk.GT2Compat.ActivateVehicleProjectionFromView(
            c.S1, m);
        c.A0 = 0x800B0000u;
""",
        "Simulation auxiliary-view vehicle projection activation",
    )
    replace_in_function_once(
        race,
        "func_800294D4",
        """        c.RA = 0x800296D0u;
        GranTurismo2PC.func_8002993C(c, m);
        L800296D0: ;
""",
        """        c.RA = 0x800296D0u;
        GranTurismo2PC.func_8002993C(c, m);
        RecompOne.Runtime.WorldCaptureContext.EndScenePass();
        L800296D0: ;
""",
        "Simulation auxiliary scene-pass boundary",
    )
    replace_in_function_once(
        race,
        "func_800294D4",
        """        c.RA = 0x800296E0u;
        GranTurismo2PC.func_800298FC(c, m);
        c.A0 = 0x800B0000u;
""",
        """        RecompOne.Runtime.WorldCaptureContext.BeginScenePass(
            RecompOne.Runtime.WorldScenePass.Main);
        c.RA = 0x800296E0u;
        GranTurismo2PC.func_800298FC(c, m);
        RecompOne.Runtime.Sdk.GT2Compat.ActivateVehicleProjectionFromView(
            c.S4, m);
        RecompOne.Runtime.Sdk.GT2Compat.TraceProjectionPhase(
            "main-before-vehicles", c.S4, m);
        c.A0 = 0x800B0000u;
""",
        "Simulation main-view projection pre-vehicle trace",
    )
    replace_in_function_once(
        race,
        "func_800294D4",
        """        c.RA = 0x800296F8u;
        GranTurismo2PC.func_8001545C(c, m);
        c.A0 = c.S5 + 0u;
""",
        """        c.RA = 0x800296F8u;
        GranTurismo2PC.func_8001545C(c, m);
        RecompOne.Runtime.Sdk.GT2Compat.TraceProjectionPhase(
            "main-after-vehicles", c.S4, m);
        c.A0 = c.S5 + 0u;
""",
        "Simulation main-view projection post-vehicle trace",
    )
    replace_in_function_once(
        race,
        "func_800294D4",
        """        c.RA = 0x80029708u;
        GranTurismo2PC.func_8002993C(c, m);
        c.RA = m.ReadU32((c.SP + 0x158u));
""",
        """        c.RA = 0x80029708u;
        GranTurismo2PC.func_8002993C(c, m);
        RecompOne.Runtime.WorldCaptureContext.EndScenePass();
        c.RA = m.ReadU32((c.SP + 0x158u));
""",
        "Simulation main scene-pass boundary",
    )
    replace_in_function_once(
        race,
        "func_80018D1C",
        """        m.WriteU32((c.S3 + 0x64u), c.S0);
        c.V0 = m.ReadU32((c.S2 + 0xCu));
""",
        """        m.WriteU32((c.S3 + 0x64u), c.S0);
        RecompOne.Runtime.WorldCaptureContext.TraceBackgroundMesh(c.S2, m);
        if (!RecompOne.Runtime.WorldCaptureContext.
                ShouldRunGuestBackgroundProjection()) {
            goto L8001962C;
        }
        c.V0 = m.ReadU32((c.S2 + 0xCu));
""",
        "Simulation resident authored background mesh",
    )

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

    replace_in_function_once(
        race,
        "func_80013C90",
        """        GranTurismo2PC.func_800166CC(c, m);
        MemoryAccess.WriteU16(m, c.S7, (ushort)c.S0);
""",
        """        GranTurismo2PC.func_800166CC(c, m);
        RecompOne.Runtime.Sdk.GT2Compat.ObserveReplayControllerFrame(
            false, MemoryAccess.ReadU32(m, (c.FP + 0x1Cu)), c.S6, m);
        MemoryAccess.WriteU16(m, c.S7, (ushort)c.S0);
""",
        "Simulation replay recorder oracle",
    )
    replace_in_function_once(
        race,
        "func_80013C90",
        """        GranTurismo2PC.func_80016428(c, m);
        c.S0 = MemoryAccess.ReadU8(m, (c.SP + 0x10u));
""",
        """        GranTurismo2PC.func_80016428(c, m);
        RecompOne.Runtime.Sdk.GT2Compat.ObserveReplayControllerFrame(
            true, MemoryAccess.ReadU32(m, (c.FP + 0x1Cu)), c.S6, m);
        c.S0 = MemoryAccess.ReadU8(m, (c.SP + 0x10u));
""",
        "Simulation replay decoder oracle",
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
        """        c.S4 = 0x00000004u;
        L80017A9C: ;
        c.V0 = MemoryAccess.ReadU32(m, c.S3);
""",
        """        c.S4 = 0x00000004u;
        L80017A9C: ;
        RecompOne.Runtime.Sdk.GT2Compat.BufferUnifiedTitleInput(c.S3, m);
        c.V0 = MemoryAccess.ReadU32(m, c.S3);
""",
        "unified title initialization input buffer",
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
        """        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1228u), 0u);
        c.SP = c.SP + 0x18u;
""",
        """        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1228u), 0u);
        RecompOne.Runtime.Sdk.GT2Compat.CompleteUnifiedTitleMenuInitialization(m);
        c.SP = c.SP + 0x18u;
""",
        "unified title immediate input readiness",
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
        """        L80017B7C: ;
        c.A0 = 0x00000003u;
""",
        """        L80017B7C: ;
        RecompOne.Runtime.Sdk.GT2Compat.BeginUnifiedTitleConfirmationAudio(
            c.S0);
        c.A0 = 0x00000003u;
""",
        "unified title confirmation audio key-on boundary",
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
    replace_in_function_once(
        race,
        "func_800140A4",
        """        c.A0 = c.S4 + 0u;
        c.A1 = c.SP + 0x10u;
        c.A2 = c.S5 + 0u;
        c.RA = 0x800145D0u;
        GranTurismo2PC.func_80067444(c, m);
""",
        """        c.A0 = c.S4 + 0u;
        c.A1 = c.SP + 0x10u;
        c.A2 = c.S5 + 0u;
        RecompOne.Runtime.Sdk.GT2Compat.BeginVehicleRenderIdentity(c.S0);
        c.RA = 0x800145D0u;
        GranTurismo2PC.func_80067444(c, m);
        RecompOne.Runtime.Sdk.GT2Compat.EndVehicleRenderIdentity();
""",
        "Simulation race vehicle stable ownership",
    )
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

    replace_in_function_once(
        main,
        "func_80067444",
        """        c.RA = 0x800675E4u;
        GranTurismo2PC.func_8007B8A0(c, m);
        c.A0 = c.S1 + 0u;
""",
        """        c.RA = 0x800675E4u;
        GranTurismo2PC.func_8007B8A0(c, m);
        RecompOne.Runtime.Sdk.GT2Compat.CaptureVehicleDepthNormalization(
            c.S0, m);
        c.A0 = c.S1 + 0u;
""",
        "vehicle authored depth-normalization tracing",
    )

    replace_in_function_once(
        main,
        "func_800670F0",
        """        c.RA = 0x80067290u;
        GranTurismo2PC.func_8007B8A0(c, m);
        c.V0 = m.ReadU8((c.S1 + 0x398u));
""",
        """        c.RA = 0x80067290u;
        GranTurismo2PC.func_8007B8A0(c, m);
        RecompOne.Runtime.Sdk.GT2Compat.CaptureVehicleDepthNormalization(
            0u, m);
        c.V0 = m.ReadU8((c.S1 + 0x398u));
""",
        "vehicle wheel depth-normalization capture",
    )

    replace_in_function_once(
        main,
        "func_80068004",
        """        c.RA = 0x80068084u;
        GranTurismo2PC.func_8007B8A0(c, m);
        c.A0 = c.S0 + 0u;
""",
        """        c.RA = 0x80068084u;
        GranTurismo2PC.func_8007B8A0(c, m);
        RecompOne.Runtime.Sdk.GT2Compat.CaptureVehicleDepthNormalization(
            c.S0, m);
        c.A0 = c.S0 + 0u;
""",
        "standalone vehicle-part depth-normalization capture",
    )

    replace_in_function_once(
        main,
        "func_80067444",
        """        c.A1 = c.V0 + 0u;
        c.V0 = c.A1 & 0x001Fu;
""",
        """        c.A1 = c.V0 + 0u;
        c.A1 = RecompOne.Runtime.Sdk.GT2Compat.ExpandVehicleFrustumMask(
            c.A1, c.S6);
        c.V0 = c.A1 & 0x001Fu;
""",
        "vehicle horizontal frustum expansion",
    )
    for path, function_name, projector_return, clipper_return in (
        (track, "func_80014708", "80014860", "80014874"),
        (race, "func_80048528", "8004871C", "80048730"),
    ):
        replace_in_function_once(
            path,
            function_name,
            f"""        c.RA = 0x{projector_return}u;
        GranTurismo2PC.func_8007B8F8(c, m);
        c.V0 = c.V0 & 0x001Fu;
""",
            f"""        c.RA = 0x{projector_return}u;
        GranTurismo2PC.func_8007B8F8(c, m);
        c.V0 = RecompOne.Runtime.Sdk.GT2Compat.
            ApplyModernVehicleViewportMask(c.V0);
        c.V0 = c.V0 & 0x001Fu;
""",
            f"Simulation {function_name} standalone vehicle viewport mask",
        )
        replace_in_function_once(
            path,
            function_name,
            f"""        c.A0 = c.S0 + 0u;
        c.A1 = c.SP + 0x10u;
        c.RA = 0x{clipper_return}u;
        GranTurismo2PC.func_80063EF4(c, m);
""",
            f"""        if (!RecompOne.Runtime.Sdk.GT2Compat.
                ModernVehicleViewportClippingEnabled) {{
            c.A0 = c.S0 + 0u;
            c.A1 = c.SP + 0x10u;
            c.RA = 0x{clipper_return}u;
            GranTurismo2PC.func_80063EF4(c, m);
        }}
""",
            f"Simulation {function_name} modern viewport clip ownership",
        )

    replace_in_function_once(
        main,
        "func_80067444",
        """        L80067700: ;
        if (c.V0 == 0u) {
""",
        """        L80067700: ;
        RecompOne.Runtime.Sdk.GT2Compat.TraceVehicleWheelGate(
            c.S6, c.S5, c.FP, c.V0);
        if (c.V0 == 0u) {
""",
        "vehicle wheel gate tracing hook",
    )

    replace_in_function_once(
        main,
        "func_80067444",
        """        L8006772C: ;
        c.A1 = c.S7 + 0u;
""",
        """        L8006772C: ;
        RecompOne.Runtime.Sdk.GT2Compat.TraceVehicleWheelDispatch(
            c.S6, c.S5, c.FP, c.S0);
        c.A1 = c.S7 + 0u;
""",
        "vehicle wheel actual-dispatch tracing hook",
    )

    replace_once(
        main,
        """        m.WriteU32((c.SP + 0x18u), c.S2);
        c.S2 = c.A2 + 0u;
        m.WriteU32((c.SP + 0x24u), c.S5);
""",
        """        m.WriteU32((c.SP + 0x18u), c.S2);
        c.S2 = c.A2 + 0u;
        RecompOne.Runtime.Sdk.GT2Compat.TraceVehicleWheelRendererEntry(
            c.S6, c.S2, m.ReadU32(c.SP + 0x4Cu),
            m.ReadU32(c.SP + 0x48u));
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
            c.V0, c.V1, m.ReadU32(c.S1 + 0x4u));
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
        RecompOne.Runtime.Sdk.GT2Compat.TraceTrackTransformSetup(m, c.S0, 0x1F800000u);
        c.S1 = m.ReadU16((c.S6 + 0xCu));
""",
        "track world-capture object begin hook",
    )
    replace_once(
        OVERLAY0,
        """        c.V0 = c.S3 & c.S2;
        c.V0 = c.S6 + c.V0;
        c.V1 = (uint)((int)c.V0 >> 10);
        RecompOne.Runtime.Gte.Write(9, c.A1);
        RecompOne.Runtime.Gte.Write(10, c.V1);
        RecompOne.Runtime.Gte.Write(11, c.A2);
        RecompOne.Runtime.Gte.Execute(0x4A49E012u);
""",
        """        c.V0 = c.S3 & c.S2;
        c.V0 = c.S6 + c.V0;
        c.V1 = (uint)((int)c.V0 >> 10);
        RecompOne.Runtime.Gte.ExecuteTrackTranslation(c.A1, c.V1, c.A2);
""",
        "wide course translation input",
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
        RecompOne.Runtime.WorldCaptureContext.TraceTrackMesh(
            c.A0, m, RecompOne.Runtime.TrackMeshProjectionPath.Primary);
        if (RecompOne.Runtime.WorldCaptureContext.ShouldRunGuestTrackProjection()) {
            c.RA = 0x800209E4u;
            GranTurismo2PC.func_8002106C(c, m);
        }
""",
        "primary track mesh ownership hook",
    )

    replace_once(
        race,
        """        c.A0 = m.ReadU32((c.SP + 0x1020u));
        c.RA = 0x800209F8u;
        GranTurismo2PC.func_800234F8(c, m);
""",
        """        c.A0 = m.ReadU32((c.SP + 0x1020u));
        RecompOne.Runtime.WorldCaptureContext.TraceTrackMesh(
            c.A0, m, RecompOne.Runtime.TrackMeshProjectionPath.Alternate);
        if (RecompOne.Runtime.WorldCaptureContext.ShouldRunGuestTrackProjection()) {
            c.RA = 0x800209F8u;
            GranTurismo2PC.func_800234F8(c, m);
        }
""",
        "alternate track mesh ownership hook",
    )

    replace_once(
        race,
        """        c.T2 = c.A0 + c.V0;
        c.A1 = RecompOne.Runtime.Gte.Read(24);
        if ((int)c.A3 < 0) {
""",
        """        c.T2 = c.A0 + c.V0;
        c.A1 = RecompOne.Runtime.Gte.Read(24);
#if !OPENGT_RELEASE_PACKAGE
        RecompOne.Runtime.WorldCaptureContext.TraceTrackFaceDecision(
            c.T3,
            RecompOne.Runtime.TrackMeshProjectionPath.Primary,
            4,
            c.T0,
            c.A1,
            0u,
            (int)c.A3 >= 0 &&
            c.A1 != 0u &&
            (int)(c.A1 & c.T0) >= 0);
#endif
        if ((int)c.A3 < 0) {
""",
        "primary triangle exact NCLIP oracle",
    )

    replace_once(
        race,
        """        c.S0 = m.ReadU32(c.T8);
        c.V0 = c.T0 | c.V0;
        if ((int)c.V0 < 0) {
            c.At = (int)c.T1 < (int)c.V1 ? 1u : 0u;
""",
        """        c.S0 = m.ReadU32(c.T8);
        c.V0 = c.T0 | c.V0;
        if ((int)c.V0 < 0) {
#if !OPENGT_RELEASE_PACKAGE
            RecompOne.Runtime.WorldCaptureContext.TraceTrackFaceDecision(
                c.T4,
                RecompOne.Runtime.TrackMeshProjectionPath.Primary,
                5,
                c.S6,
                c.T3,
                0u,
                false);
#endif
            c.At = (int)c.T1 < (int)c.V1 ? 1u : 0u;
""",
        "primary quad flag-rejection oracle",
    )

    replace_once(
        race,
        """        c.A1 = c.T3 - 0x1u;
        c.A2 = c.T3 + c.V0;
        c.V1 = c.T3 | c.V0;
        if (c.V1 == 0u) {
""",
        """        c.A1 = c.T3 - 0x1u;
        c.A2 = c.T3 + c.V0;
        c.V1 = c.T3 | c.V0;
#if !OPENGT_RELEASE_PACKAGE
        RecompOne.Runtime.WorldCaptureContext.TraceTrackFaceDecision(
            c.T4,
            RecompOne.Runtime.TrackMeshProjectionPath.Primary,
            5,
            c.S6,
            c.T3,
            c.V0,
            c.V1 != 0u &&
            (int)((c.T3 - 1u) & (c.V0 - 1u) & c.S6) >= 0);
#endif
        if (c.V1 == 0u) {
""",
        "primary quad exact NCLIP oracle",
    )
    replace_once(
        race,
        """        c.V0 = c.V0 | c.V1;
        c.A0 = c.S5 & 0x0020u;
        c.V0 = c.V0 | c.A0;
        c.A1 = c.A0 - 0x20u;
        c.A2 = m.ReadU32(c.T5);
        if (c.V0 == 0u) {
""",
        """        c.V0 = c.V0 | c.V1;
        c.A0 = c.S5 & 0x0020u;
        c.V0 = c.V0 | c.A0;
        c.A1 = c.A0 - 0x20u;
        c.A2 = m.ReadU32(c.T5);
#if !OPENGT_RELEASE_PACKAGE
        RecompOne.Runtime.WorldCaptureContext.TraceTrackFaceDecision(
            c.S3,
            RecompOne.Runtime.TrackMeshProjectionPath.Alternate,
            1,
            c.S1,
            ~c.T1,
            c.V1,
            c.V0 != 0u &&
            (int)(c.T1 & (c.V1 - 1u) & c.S1 & c.A1) >= 0);
#endif
        if (c.V0 == 0u) {
""",
        "alternate F4 exact NCLIP oracle",
    )

    replace_once(
        race,
        """        m.WriteU32((c.SP + 0x18u), c.S2);
        c.S2 = c.A2 + 0u;
        m.WriteU32((c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
""",
        """        m.WriteU32((c.SP + 0x18u), c.S2);
        c.S2 = c.A2 + 0u;
        c.S2 = RecompOne.Runtime.WorldCaptureContext.
            ExpandAuxiliaryTrackVisibility(c.S2);
        m.WriteU32((c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
""",
        "resident auxiliary track visibility",
    )

    replace_once(
        race,
        """        c.S0 = c.S0 << 2;
        c.S0 = c.S0 + 0x4u;
        c.S0 = c.S4 + c.S0;
        c.S5 = 0x1F800000u;
""",
        """        c.S0 = c.S0 << 2;
        c.S0 = c.S0 + 0x4u;
        c.S0 = c.S4 + c.S0;
        RecompOne.Runtime.WorldCaptureContext.
            BeginAuxiliaryTrackInstance(c.S0);
        c.S5 = 0x1F800000u;
""",
        "auxiliary track instance identity",
    )

    replace_once(
        race,
        """        GranTurismo2PC.func_8007AEF4(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFFu;
""",
        """        GranTurismo2PC.func_8007AEF4(c, m);
        c.V1 = c.V0 + 0u;
        c.V1 = RecompOne.Runtime.WorldCaptureContext.
            SelectAuxiliaryTrackModel(c.V1);
        c.V0 = 0xFFFFFFFFu;
""",
        "resident auxiliary track model selection",
    )

    replace_once(
        race,
        """        c.V0 = c.V1 << 3;
        c.V0 = c.V0 + 0x4u;
        c.V0 = c.S0 + c.V0;
        c.S3 = m.ReadU32((c.V0 + 0x4u));
        c.A1 = 0x00001000u;
""",
        """        c.V0 = c.V1 << 3;
        c.V0 = c.V0 + 0x4u;
        c.V0 = c.S0 + c.V0;
        c.S3 = m.ReadU32((c.V0 + 0x4u));
        RecompOne.Runtime.WorldCaptureContext.
            SetAuxiliaryTrackModel(c.S3);
        c.A1 = 0x00001000u;
""",
        "auxiliary track model identity",
    )

    replace_once(
        race,
        """        m.WriteU32((c.SP + 0x14u), c.V0);
        c.V0 = c.V0 & 0x001Fu;
        if (c.V0 != 0u) {
""",
        """        m.WriteU32((c.SP + 0x14u), c.V0);
        c.V0 = c.V0 & 0x001Fu;
        c.V0 = RecompOne.Runtime.WorldCaptureContext.
            IncludeAuxiliaryTrackObject(c.V0);
        if (c.V0 != 0u) {
""",
        "auxiliary track screen-cull bypass",
    )

    replace_once(
        race,
        """        c.V1 = m.ReadU16((c.S3 + 0x40u));
        c.T4 = m.ReadU32((c.S3 + 0x24u));
        if (c.V1 == 0u) {
""",
        """        c.V1 = m.ReadU16((c.S3 + 0x40u));
        c.T4 = m.ReadU32((c.S3 + 0x24u));
        if (!RecompOne.Runtime.WorldCaptureContext.
                ShouldRunGuestTrackBillboardProjection()) {
            c.T1 = c.S4 + 0u;
            goto L8001FBA8;
        }
        if (c.V1 == 0u) {
""",
        "auxiliary billboard preprojection ownership",
    )

    replace_once(
        race,
        """        c.V0 = m.ReadU16((c.A0 + 0x40u));
        c.T5 = m.ReadU32((c.A0 + 0x24u));
        if (c.V0 == 0u) {
""",
        """        c.V0 = m.ReadU16((c.A0 + 0x40u));
        c.T5 = m.ReadU32((c.A0 + 0x24u));
        if (!RecompOne.Runtime.WorldCaptureContext.
                ShouldRunGuestTrackBillboardProjection()) {
            c.T2 = 0x1F800000u;
            goto L800205E4;
        }
        if (c.V0 == 0u) {
""",
        "primary billboard preprojection ownership",
    )

    replace_once(
        race,
        """        c.A0 = c.S3 + 0u;
        c.RA = 0x8001FFC8u;
        GranTurismo2PC.func_80019B58(c, m);
""",
        """        c.A0 = c.S3 + 0u;
        RecompOne.Runtime.WorldCaptureContext.TraceTrackMesh(
            c.A0, m, RecompOne.Runtime.TrackMeshProjectionPath.Primary);
        if (RecompOne.Runtime.WorldCaptureContext.ShouldRunGuestTrackProjection()) {
            c.RA = 0x8001FFC8u;
            GranTurismo2PC.func_80019B58(c, m);
        }
""",
        "primary auxiliary track mesh ownership",
    )

    replace_once(
        race,
        """        c.A0 = c.S3 + 0u;
        c.RA = 0x8001FFD8u;
        GranTurismo2PC.func_8001C17C(c, m);
""",
        """        c.A0 = c.S3 + 0u;
        RecompOne.Runtime.WorldCaptureContext.TraceTrackMesh(
            c.A0, m, RecompOne.Runtime.TrackMeshProjectionPath.Alternate);
        if (RecompOne.Runtime.WorldCaptureContext.ShouldRunGuestTrackProjection()) {
            c.RA = 0x8001FFD8u;
            GranTurismo2PC.func_8001C17C(c, m);
        }
""",
        "alternate auxiliary track mesh ownership",
    )

    replace_once(
        race,
        """        L8001FFE8: ;
        c.S4 = m.ReadU32((c.SP + 0x10u));
""",
        """        L8001FFE8: ;
        RecompOne.Runtime.WorldCaptureContext.EndObject();
        c.S4 = m.ReadU32((c.SP + 0x10u));
""",
        "auxiliary track object end",
    )

    apply_auxiliary_billboard_projection(race)
    apply_car_preview_camera_limit(track)

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

    changed_files, memory_calls = rewrite_tree(GENERATED)
    print(
        "Applied GT2 graphics enhancements to generated race overlays; "
        f"concrete memory files={changed_files} calls={memory_calls}"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
