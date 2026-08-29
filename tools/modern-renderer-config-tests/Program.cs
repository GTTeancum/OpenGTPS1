using RecompOne.Runtime;
using RecompOne.Runtime.Config;
using RecompOne.Runtime.Context;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

static void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

static string ReadRepoFile(string relativePath)
{
    foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        for (DirectoryInfo? directory = new(start); directory != null; directory = directory.Parent)
        {
            string candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
                return File.ReadAllText(candidate);
        }
    }
    throw new FileNotFoundException($"Could not locate repository file: {relativePath}");
}

static int Occurrences(string text, string value) =>
    text.Split(value, StringSplitOptions.None).Length - 1;

static short[] ReadBillboardCoordinates(object value)
{
    string[] names =
    [
        "X0", "Y0", "Z0",
        "X1", "Y1", "Z1",
        "X2", "Y2", "Z2",
        "X3", "Y3", "Z3",
    ];
    return names.Select(name =>
        (short)value.GetType().GetProperty(
            name,
            BindingFlags.Public | BindingFlags.Instance)!.GetValue(value)!)
        .ToArray();
}

static void RequireModern(ViewConfig view, string context)
{
    Require(view.GraphicsPreset == "Enhanced", $"{context}: preset");
    Require(view.HighResolution3D, $"{context}: 4x source");
    Require(view.TextureSmoothing, $"{context}: smoothing");
    Require(view.HighResolutionTextures, $"{context}: 4x texture assets");
    Require(view.PerspectiveCorrectTextures, $"{context}: perspective textures");
    Require(view.StabilizeGeometrySeams, $"{context}: seams");
    Require(view.ExtendedDrawDistance, $"{context}: distance");
    Require(view.LevelOfDetail == "Maximum", $"{context}: LOD");
    Require(!view.Ps1Dithering, $"{context}: dithering");
}

static void VerifyCpuProjectionFastPath()
{
    const uint packed = 0x00200010u;
    var cpu = new CpuContext();
    var projected = new GteProjectedValue(
        packed,
        Z: 512,
        Generation: Gte.ProjectionGeneration,
        GteDepthProvenance.CpuRegisterFlow);
    WorldCaptureContext.LiveRenderingEnabled = false;
    Gte.SetProjectionTrackingEnabled(true);
    try
    {
        Gte.NotifyCpuRegisterRead(packed, in projected);
        cpu.V0 = packed;
        Require(
            !Gte.HasPendingCpuProjection,
            "CPU projection assignment did not consume pending metadata");

        Require(cpu.V0 == packed, "tracked CPU register value changed");
        Require(
            Gte.HasPendingCpuProjection,
            "tracked CPU register read did not restore projection metadata");

        _ = cpu.V1;
        Require(
            !Gte.HasPendingCpuProjection,
            "untracked CPU register read did not clear pending projection");

        _ = cpu.V0;
        Require(
            Gte.HasPendingCpuProjection,
            "tracked CPU register did not remain reusable");
        Gte.Write(0, 0);
        Require(
            !Gte.HasPendingCpuProjection,
            "non-SXY GTE write did not end CPU projection transfer");
    }
    finally
    {
        Gte.SetProjectionTrackingEnabled(false);
        WorldCaptureContext.LiveRenderingEnabled = false;
    }
}

VerifyCpuProjectionFastPath();

static void VerifyProjectionOriginHandleFlow()
{
    const uint directAddress = 0x00001000u;
    const uint cpuAddress = directAddress + 4;
    const uint modelPointer = 0x00123456u;
    var cpu = new CpuContext();
    WorldCaptureContext.LiveRenderingEnabled = true;
    Gte.SetProjectionTrackingEnabled(true);
    WorldCaptureContext.BeginTrackObject(1, modelPointer);
    try
    {
        Gte.WriteControl(0, 0x00001000u);
        Gte.WriteControl(1, 0x10000000u);
        Gte.WriteControl(2, 0x00001000u);
        Gte.WriteControl(3, 0u);
        Gte.WriteControl(4, 0x00001000u);
        Gte.WriteControl(5, 0u);
        Gte.WriteControl(6, 0u);
        Gte.WriteControl(7, 1024u);
        Gte.WriteControl(24, 160u << 16);
        Gte.WriteControl(25, 120u << 16);
        Gte.WriteControl(26, 256u);

        static void Project(short x, short y, short z)
        {
            Gte.Write(0, (ushort)x | ((uint)(ushort)y << 16));
            Gte.Write(1, (ushort)z);
            Gte.Execute(0x01u);
        }

        static GteProjectionOrigin Resolve(uint address, uint packed)
        {
            Require(
                Gte.TryGetPacketProjection(
                    address,
                    (short)packed,
                    (short)(packed >> 16),
                    out ushort z,
                    out _,
                    out _,
                    out GteProjectionOrigin origin) &&
                z != 0 && origin.Valid,
                "projection origin transport handle did not resolve");
            return origin;
        }

        Project(10, 20, 30);
        uint directPacked = Gte.StoreWord(14);
        Gte.NotifyRamWrite(directAddress, directPacked);
        GteProjectionOrigin direct = Resolve(directAddress, directPacked);

        Project(11, 21, 31);
        uint cpuPacked = Gte.Read(14);
        cpu.V0 = cpuPacked;
        uint transferredPacked = cpu.V0;
        Gte.NotifyRamWrite(cpuAddress, transferredPacked);
        GteProjectionOrigin transferred = Resolve(
            cpuAddress, transferredPacked);

        Require(
            direct.ModelX == 10 && direct.ModelY == 20 &&
            direct.ModelZ == 30 && transferred.ModelX == 11 &&
            transferred.ModelY == 21 && transferred.ModelZ == 31 &&
            direct.R00 == 4096 && direct.R11 == 4096 &&
            direct.R22 == 4096 && direct.TranslateZ == 1024 &&
            direct.ProjectionOffsetX == 160 << 16 &&
            direct.ProjectionOffsetY == 120 << 16 &&
            direct.ProjectionPlane == 256 &&
            direct.Object.Kind == WorldObjectKind.Track &&
            direct.Object.ModelPointer == modelPointer,
            "projection origin handle changed captured provenance");
    }
    finally
    {
        WorldCaptureContext.EndObject();
        Gte.SetProjectionTrackingEnabled(false);
        WorldCaptureContext.LiveRenderingEnabled = false;
    }
}

if (args.Contains("--verify-release-policy", StringComparer.OrdinalIgnoreCase))
{
    Require(ReleasePolicy.IsReleasePackage, "release policy was not compiled in");
    foreach (string name in new[]
    {
        "OPENGT_RELEASE_POLICY_SENTINEL",
        "RECOMPONE_AUDIT_RELEASE_POLICY_SENTINEL",
        "RECOMPONE_TRACE_RELEASE_POLICY_SENTINEL",
        "RECOMPONE_GT2_RELEASE_POLICY_SENTINEL",
        "RECOMPONE_NATIVE_WORLD_RELEASE_POLICY_SENTINEL",
    })
    {
        Require(
            Environment.GetEnvironmentVariable(name) is null,
            $"release policy retained development control: {name}");
    }
    Require(
        Environment.GetEnvironmentVariable("RECOMPONE_DISABLE_LIVE_INPUT") ==
            "keep",
        "release policy removed bounded package-test control");
    Type releaseGpuType = typeof(ReleasePolicy).Assembly.GetType(
        "RecompOne.Runtime.Gpu",
        throwOnError: true)!;
    MethodInfo releaseCompatibilityRasterDecision = releaseGpuType.GetMethod(
        "ShouldRasterizeCompatibilityTriangle",
        BindingFlags.NonPublic | BindingFlags.Static)!;
    bool releaseRasterizesWorld =
        (bool)releaseCompatibilityRasterDecision.Invoke(null, [true])!;
    bool releaseRasterizesScreen =
        (bool)releaseCompatibilityRasterDecision.Invoke(null, [false])!;
    Require(
        !releaseRasterizesWorld,
        "release package can route 3D world geometry to a compatibility rasterizer");
    Require(
        releaseRasterizesScreen,
        "release package disabled provenance-free screen composition");
    Type releaseRendererType = typeof(ReleasePolicy).Assembly.GetType(
        "RecompOne.Runtime.Hle.LiveWorldRenderer",
        throwOnError: true)!;
    Require(
        releaseRendererType.GetMethod(
            "ThrowIfFailed",
            BindingFlags.NonPublic | BindingFlags.Instance) is not null,
        "release package cannot surface a native world-renderer failure");
    Console.WriteLine(
        "release_policy=pass diagnostics=unavailable " +
        "compatibility_world_path=absent screen_compositor=present " +
        "native_failure=fail-closed");
    return;
}

VerifyProjectionOriginHandleFlow();

static void VerifyBackgroundOwnership()
{
    const uint modelPointer = 0x800AE324u;
    WorldCaptureContext.LiveRenderingEnabled = true;
    try
    {
        WorldCaptureContext.BeginBackgroundObject(modelPointer);
        WorldObjectContext background = WorldCaptureContext.Current;
        Require(
            background.Kind == WorldObjectKind.Background &&
            background.StableId == modelPointer &&
            background.ModelPointer == modelPointer,
            "authored background ownership was not retained");
    }
    finally
    {
        WorldCaptureContext.EndObject();
        WorldCaptureContext.LiveRenderingEnabled = false;
    }
}

VerifyBackgroundOwnership();

string unifiedHostProject = ReadRepoFile(
    @"tools\unified-host\GranTurismo2PC.csproj");
string unifiedHostProgram = ReadRepoFile(
    @"tools\unified-host\Program.cs");
string inputManager = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Host\InputManager.cs");
string runtimeProject = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\RecompOne.Runtime.csproj");
string releasePolicy = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\ReleasePolicy.cs");
string nativeProject = ReadRepoFile(@"native\CMakeLists.txt");
string releasePackager = ReadRepoFile(@"tools\package_release.ps1");
string releaseSetup = ReadRepoFile(@"release\Setup-From-GT2-Discs.ps1");
string releaseReadme = ReadRepoFile(@"release\README.md");
string releasePackageTest = ReadRepoFile(@"tools\test_release_package.ps1");
string seattleReleaseSmoke = ReadRepoFile(
    @"tools\test_seattle_release_smoke.ps1");
Require(
    unifiedHostProject.Contains(
        "<AppHostDotNetSearch>AppLocal;Global</AppHostDotNetSearch>",
        StringComparison.Ordinal),
    "self-contained apphost no longer prefers its bundled runtime");
Require(
    unifiedHostProject.Contains(
        "<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>",
        StringComparison.Ordinal) &&
    unifiedHostProject.Contains(
        "ExcludeFromSingleFile=\"false\"",
        StringComparison.Ordinal),
    "single-file publish no longer embeds the native renderer bridge");
Require(
    releasePackager.Contains(
        "Release exposed app-local DLL dependencies instead of",
        StringComparison.Ordinal) &&
    releasePackageTest.Contains(
        "|dat|dll|ovl|",
        StringComparison.Ordinal),
    "public package no longer rejects loose DLL dependencies");
Require(
    releasePackager.Contains(
        "work\\release-package-scratch", StringComparison.Ordinal) &&
    releasePackageTest.Contains(
        "work\\release-audit-scratch", StringComparison.Ordinal) &&
    !releasePackager.Contains("GetTempPath", StringComparison.Ordinal) &&
    !releasePackageTest.Contains("GetTempPath", StringComparison.Ordinal) &&
    releasePackageTest.Contains(
        "DOTNET_BUNDLE_EXTRACT_BASE_DIR", StringComparison.Ordinal) &&
    seattleReleaseSmoke.Contains(
        "DOTNET_BUNDLE_EXTRACT_BASE_DIR", StringComparison.Ordinal),
    "release packaging or validation can write artifacts outside the repository");
Require(
    releasePackageTest.Contains(
        "stage 'replay_1' at absolute poll", StringComparison.Ordinal) &&
    releasePackageTest.Contains(
        "$replayProofPolls -lt 300", StringComparison.Ordinal) &&
    releasePackageTest.Contains(
        "phase=replay car=0", StringComparison.Ordinal) &&
    releasePackageTest.Contains(
        "$replayWorldFrames.Count -lt 3", StringComparison.Ordinal),
    "release-package replay gate no longer proves the natural replay stage");
Require(
    unifiedHostProject.Contains(
        "<OutputType>WinExe</OutputType>",
        StringComparison.Ordinal) &&
    unifiedHostProgram.Contains(
        "PreloadBundledNative(\"glfw3.dll\");",
        StringComparison.Ordinal) &&
    unifiedHostProgram.Contains(
        "PreloadBundledNative(\"cimgui.dll\");",
        StringComparison.Ordinal),
    "interactive single-file launch no longer initializes a console-free window backend");
Require(
    unifiedHostProgram.Contains(
        "RECOMPONE_CAPTURE_AUTOMATIC_STAGE", StringComparison.Ordinal) &&
    unifiedHostProgram.Contains(
        "RECOMPONE_CAPTURE_AUTOMATIC_STAGE\", \"0", StringComparison.Ordinal) &&
    !unifiedHostProgram.Contains("300+1=CAPTURE", StringComparison.Ordinal) &&
    inputManager.Contains(
        "RECOMPONE_CAPTURE_INPUT_STAGE_POLL", StringComparison.Ordinal),
    "direct Seattle replay can still emit an unsolicited user capture");
Require(
    runtimeProject.Contains("OPENGT_RELEASE_PACKAGE", StringComparison.Ordinal) &&
    releasePolicy.Contains("[ModuleInitializer]", StringComparison.Ordinal) &&
    releasePolicy.Contains("\"OPENGT_\"", StringComparison.Ordinal) &&
    releasePolicy.Contains("\"RECOMPONE_AUDIT_\"", StringComparison.Ordinal) &&
    releasePolicy.Contains("\"RECOMPONE_TRACE_\"", StringComparison.Ordinal) &&
    releasePolicy.Contains("\"RECOMPONE_GT2_\"", StringComparison.Ordinal) &&
    releasePackager.Contains(
        "-p:OpenGTReleasePackage=true", StringComparison.Ordinal) &&
    Occurrences(
        releasePackager,
        "--artifacts-path $managedBuild") == 2 &&
    releasePackager.Contains(
        "OpenGTPS1-release-managed-", StringComparison.Ordinal) &&
    releasePackager.Contains(
        "[IO.Directory]::Delete($managedBuildFull, $true)",
        StringComparison.Ordinal),
    "release package can still activate development renderer controls");
Require(
    nativeProject.Contains(
        "OPENGT_BUILD_DEV_TOOLS", StringComparison.Ordinal) &&
    nativeProject.Contains(
        "opengt_renderer_dev_support", StringComparison.Ordinal) &&
    releasePackager.Contains(
        "-DOPENGT_BUILD_TESTS=OFF", StringComparison.Ordinal) &&
    releasePackager.Contains(
        "-DOPENGT_BUILD_DEV_TOOLS=OFF", StringComparison.Ordinal),
    "release native renderer still links diagnostic or oracle support");
Require(
    releasePackager.Contains(
        @"tools\unified-host\GranTurismo2PC.csproj",
        StringComparison.Ordinal) &&
    releasePackager.Contains(
        "Setup-From-GT2-Discs.ps1",
        StringComparison.Ordinal) &&
    releasePackager.Contains(
        "recompone.simulation.unified.json",
        StringComparison.Ordinal) &&
    releasePackager.Contains(
        "recompone.arcade.unified.json",
        StringComparison.Ordinal) &&
    !releasePackager.Contains(
        @"generated\recompiled\GranTurismo2PC.csproj",
        StringComparison.Ordinal) &&
    !releasePackager.Contains(
        "Setup-From-Simulation-Disc.ps1",
        StringComparison.Ordinal) &&
    releaseSetup.Contains(
        "D0AB6E70539601057590A36299543C0ADAD219254D712F7D4273219094ED5031",
        StringComparison.Ordinal) &&
    releaseSetup.Contains(
        "C2E97D6B0C847CA4336D9D84D8D98C349D1240ED075E81AB3FD5C977E9A45075",
        StringComparison.Ordinal) &&
    releaseSetup.Contains(
        "7C3BF68061E5867DE5AF831121C50091128DDBDE4F13026A050D3C71EF0EEE53",
        StringComparison.Ordinal) &&
    releaseSetup.Contains(
        "735D838C3A0F12E2917593648790F9FD1CB6ADA13D402E19022D7C814737321C",
        StringComparison.Ordinal) &&
    releaseReadme.Contains("SCUS-94488", StringComparison.Ordinal) &&
    releaseReadme.Contains("SCUS-94455", StringComparison.Ordinal) &&
    releasePackageTest.Contains(
        "--headless --arcade-replay seattle-circuit",
        StringComparison.Ordinal),
    "release packaging no longer proves the authoritative unified Seattle path");
Require(
    releaseSetup.Contains(
        "BEF591A382F4DCEC1990F5DB01B43CD42ED9CBDFE504BCB47E3FDB4013495A0E",
        StringComparison.Ordinal) &&
    Regex.IsMatch(
        releaseSetup,
        @"Join-Path \$arcade 'DISC_META\.DAT'\),\s*" +
        @"'GT2\.VOL;1',\s*473,\s*213596160\)"),
    "release setup no longer aligns Arcade unified metadata to LBA 473");
Require(
    unifiedHostProgram.Contains(
        "ResolveUnifiedGameRoot(AppContext.BaseDirectory, launchDirectory)",
        StringComparison.Ordinal) &&
    unifiedHostProgram.Contains(
        "Path.Combine(directory.FullName, \"work\", \"gt2-unified\")",
        StringComparison.Ordinal) &&
    unifiedHostProgram.Contains(
        "GranTurismo2PC-startup-latest.log",
        StringComparison.Ordinal) &&
    unifiedHostProgram.Contains(
        "Gran Turismo 2 PC - Startup Error",
        StringComparison.Ordinal),
    "no-argument startup no longer resolves developer data or reports a durable interactive failure");

string stockScenarioHarness = ReadRepoFile(
    @"tools\test_modern_renderer_scenario.ps1");
string stockSoakHarness = ReadRepoFile(
    @"tools\test_modern_renderer_extended_soak.ps1");
string visibleReviewHarness = ReadRepoFile(
    @"tools\run_visible_modern_renderer_review.ps1");
string frameClockSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Host\FrameClock.cs");
string liveRendererSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Gpu\Hle\LiveWorldRenderer.cs");
string gt2CompatSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\sdk\GT2Compat.cs");
string gteSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Hardware\Gte.cs");
string worldCaptureContextSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Gpu\WorldCaptureContext.cs");
string psMemorySource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Memory\PSMemory.cs");
string generatedMemoryAccessSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.GeneratedSupport\MemoryAccess.cs");
string rawTrackSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Gpu\GpuRawTrack.cs");
string nativeRendererSource = ReadRepoFile(
    @"native\src\world_gpu_renderer_d3d11.cpp");
string worldDrawListHeader = ReadRepoFile(
    @"native\include\opengt\world_draw_list.hpp");
string liveBridgeSource = ReadRepoFile(
    @"native\src\live_renderer_bridge.cpp");
string seattleDirectHarness = ReadRepoFile(
    @"tools\test_seattle_direct_replay.ps1");
string simulationEnhancements = ReadRepoFile(
    @"tools\apply_gt2_enhancements.py");
string arcadeEnhancements = ReadRepoFile(
    @"tools\apply_gt2_arcade_enhancements.py");
string presentationRendererSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Host\Window\PresentationRenderer.cs");
string oggMusicSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Hardware\OggMusic.cs");
string libCdSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\sdk\LibCd.cs");
string motionCaptureHarness = ReadRepoFile(
    @"tools\capture_modern_renderer_final_motion.ps1");
string ssr5VehicleBoundaryHarness = ReadRepoFile(
    @"tools\run_ssr5_vehicle_boundary_trace.ps1");
string arcadeRendererAuditHarness = ReadRepoFile(
    @"tools\run_arcade_renderer_audit.ps1");
string arcadeRendererAuditLauncher = ReadRepoFile(
    @"tools\run_arcade_renderer_audit.cmd");
const string selfContainedDeploy =
    @"tools\unified-host\bin\Release\net10.0\win-x64\publish";
Require(
    stockScenarioHarness.Contains("'TahitiRoad'", StringComparison.Ordinal) &&
    stockScenarioHarness.Contains("'RedRock'", StringComparison.Ordinal) &&
    stockScenarioHarness.Contains(selfContainedDeploy, StringComparison.Ordinal),
    "stock renderer scenarios no longer target the exact packaged build");
Require(
    stockSoakHarness.Contains(
        "[string[]]$Scenarios = @('TahitiRoad', 'RedRock')",
        StringComparison.Ordinal) &&
    !stockSoakHarness.Contains(
        "[string[]]$Scenarios = @('Arcade', 'SSR11', 'SupraTahiti')",
        StringComparison.Ordinal),
    "converted content became the primary renderer soak again");
Require(
    visibleReviewHarness.Contains(
        "[string]$Mode = 'Simulation'",
        StringComparison.Ordinal) &&
    visibleReviewHarness.Contains(
        @"tests\fixtures\modern-renderer-replay-soak.input",
        StringComparison.Ordinal) &&
    !visibleReviewHarness.Contains(
        "[string]$Mode = 'Arcade'",
        StringComparison.Ordinal),
    "visible renderer review no longer defaults to stock Red Rock");
Require(
    frameClockSource.Contains(
        "Thread.CurrentThread.Priority = ThreadPriority.Highest",
        StringComparison.Ordinal) &&
    liveRendererSource.Contains(
        "Priority = ThreadPriority.Highest",
        StringComparison.Ordinal),
    "paced emulation and native pair production no longer share the highest thread priority");
Require(
    gt2CompatSource.Contains(
        "CopyAlignedGuestWords(", StringComparison.Ordinal) &&
    psMemorySource.Contains(
        "TryCopyAlignedRamWords(", StringComparison.Ordinal) &&
    psMemorySource.Contains(
        "Gte.NotifyRamRead(offset, value);", StringComparison.Ordinal) &&
    arcadeEnhancements.Contains(
        "Arcade exact aligned scene render-record copy",
        StringComparison.Ordinal) &&
    ReadRepoFile(
        @"generated\arcade-recompiled\gt2_arcade_overlay_0.cs").Contains(
            "GT2Compat.CopyAlignedGuestWords(", StringComparison.Ordinal),
    "Seattle scene-record copy lost exact RAM/GTE observers or generated integration");
Require(
    Occurrences(
        generatedMemoryAccessSource,
        "MethodImplOptions.AggressiveInlining | " +
        "MethodImplOptions.AggressiveOptimization") == 10 &&
    !generatedMemoryAccessSource.Contains(
        "MethodImplOptions.NoInlining", StringComparison.Ordinal),
    "profiled guest RAM dispatch regained its redundant non-inlined boundary");
Require(
    gt2CompatSource.Contains(
        "static readonly bool True60HzEnabled = true;",
        StringComparison.Ordinal) &&
    !gt2CompatSource.Contains(
        "RECOMPONE_GT2_TRUE_60HZ",
        StringComparison.Ordinal) &&
    !gt2CompatSource.Contains(
        "True60HzExperiment",
        StringComparison.Ordinal) &&
    !liveRendererSource.Contains("RenderPair", StringComparison.Ordinal) &&
    !liveRendererSource.Contains("TryReadPair", StringComparison.Ordinal) &&
    !liveRendererSource.Contains(
        "LiveInterpolationStats",
        StringComparison.Ordinal) &&
    !liveRendererSource.Contains(
        "RECOMPONE_GT2_TRUE_60HZ",
        StringComparison.Ordinal),
    "genuine per-VBlank simulation can still be downgraded at runtime");
Require(
    rawTrackSource.Contains(
        "readonly bool _rawTrackReplacementRequested = true;",
        StringComparison.Ordinal) &&
    !rawTrackSource.Contains(
        "RECOMPONE_DEV_GT2_RAW_TRACK_REPLACE",
        StringComparison.Ordinal) &&
    releasePolicy.Contains("\"RECOMPONE_DEV_\"", StringComparison.Ordinal) &&
    rawTrackSource.Contains("objectsPerFrame=", StringComparison.Ordinal) &&
    rawTrackSource.Contains(
        "sourcePrimitivesPerFrame=",
        StringComparison.Ordinal) &&
    rawTrackSource.Contains("trianglesPerFrame=", StringComparison.Ordinal) &&
    rawTrackSource.Contains(
        "RegisterResidentRawTrackMeshDefinition(",
        StringComparison.Ordinal) &&
    rawTrackSource.Contains("companionPath =", StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "The release build has no stock-sector fallback.",
        StringComparison.Ordinal),
    "shipping course residency can still fall back to guest visibility packets");
Require(
    gt2CompatSource.Contains(
        "const int ExtendedVisibilitySectorRadius = 4;",
        StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "distance <= ExtendedVisibilitySectorRadius",
        StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "scope=bounded-sector-horizon",
        StringComparison.Ordinal) &&
    !gt2CompatSource.Contains(
        "scope=complete-static-course",
        StringComparison.Ordinal) &&
    worldCaptureContextSource.Contains(
        "policy=authored-mutual-exclusion",
        StringComparison.Ordinal) &&
    worldCaptureContextSource.Contains(
        "return visibilityMask;",
        StringComparison.Ordinal) &&
    !worldCaptureContextSource.Contains(
        "return _trackMeshConsumer == null ? visibilityMask : uint.MaxValue;",
        StringComparison.Ordinal),
    "course selection can co-render mutually exclusive sector or auxiliary meshes");
Require(
    rawTrackSource.Contains(
        "RawTrackPrimaryQuadAccepted(",
        StringComparison.Ordinal) &&
    rawTrackSource.Contains(
        "RawTrackAlternateQuadAccepted(",
        StringComparison.Ordinal) &&
    rawTrackSource.Contains(
        "RawTrackQuadTriangleAccepted(",
        StringComparison.Ordinal) &&
    rawTrackSource.Contains(
        "guestPrimitiveTruthRMG=",
        StringComparison.Ordinal) &&
    rawTrackSource.Contains(
        "guestPrimitiveQuantizedZeroDivergences=",
        StringComparison.Ordinal) &&
    Occurrences(simulationEnhancements, "TraceTrackFaceDecision(") == 4 &&
    Occurrences(arcadeEnhancements, "TraceTrackFaceDecision(") == 4 &&
    Occurrences(simulationEnhancements, "#if !OPENGT_RELEASE_PACKAGE") == 4 &&
    Occurrences(arcadeEnhancements, "#if !OPENGT_RELEASE_PACKAGE") == 4 &&
    rawTrackSource.Contains(
        "RECOMPONE_AUDIT_GT2_RAW_TRACK_TEMPORAL_COVERAGE",
        StringComparison.Ordinal) &&
    rawTrackSource.Contains(
        "RECOMPONE_AUDIT_GT2_RAW_TRACK_STAGE",
        StringComparison.Ordinal) &&
    rawTrackSource.Contains(
        "temporalCoverageNearExcluded=",
        StringComparison.Ordinal) &&
    rawTrackSource.Contains(
        "temporalCoverageMaximum=",
        StringComparison.Ordinal) &&
    rawTrackSource.Contains(
        "RECOMPONE_AUDIT_GT2_RAW_TRACK_NEAR_CLIP",
        StringComparison.Ordinal) &&
    rawTrackSource.Contains(
        "RawTrackNearClipFacingAccepted(",
        StringComparison.Ordinal) &&
    simulationEnhancements.Contains(
        "(int)c.A3 >= 0 &&",
        StringComparison.Ordinal) &&
    arcadeEnhancements.Contains(
        "(int)((c.T3 - 1u) & (c.V0 - 1u) & c.S6) >= 0",
        StringComparison.Ordinal) &&
    unifiedHostProject.Contains(
        "$(DefineConstants);OPENGT_RELEASE_PACKAGE",
        StringComparison.Ordinal),
    "track-face oracle no longer records exact GT2 branches or is present in release guest code");
Require(
    nativeRendererSource.Contains(
        "OPENGT_RENDER_TEXTURE_COVERAGE_DIAGNOSTICS",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "[Render-Texture-Coverage]",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "OPENGT_RENDER_PIXEL_PROVENANCE_POINTS",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "OPENGT_RENDER_SCENE_CONTINUITY_AUDIT",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "OPENGT_RENDER_SCENE_CONTINUITY_OBJECT",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "frameGaps=%llu",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "[Render-Pixel-Provenance]",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "[Render-Resident-Edges]",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "OPENGT_RENDER_VEHICLE_BOUNDARY_AUDIT",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "[Render-Vehicle-Boundary]",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "[Render-Vehicle-Boundary-Transition]",
        StringComparison.Ordinal),
    "native renderer lost the bounded texture-coverage or resident-edge diagnostics");
Require(
    worldDrawListHeader.Contains(
        "unclassified_world_commands",
        StringComparison.Ordinal) &&
    liveBridgeSource.Contains(
        "[Native-World-Classification]",
        StringComparison.Ordinal) &&
    liveBridgeSource.Contains(
        "frames_with_unclassified_world",
        StringComparison.Ordinal) &&
    seattleDirectHarness.Contains(
        "unclassifiedWorldCommands=0",
        StringComparison.Ordinal) &&
    seattleDirectHarness.Contains(
        "framesWithUnclassifiedWorld=0",
        StringComparison.Ordinal),
    "Seattle can no longer prove whole-run world/effect ownership");
Require(
    seattleDirectHarness.Contains(
        "$diagnosticReplay -or",
        StringComparison.Ordinal) &&
    seattleDirectHarness.Contains(
        "$ExitAtReplayHandoff",
        StringComparison.Ordinal) &&
    seattleDirectHarness.Contains(
        "$pacedStage = if ($diagnosticReplay) { 'replay_1' } else { 'race_1' }",
        StringComparison.Ordinal) &&
    seattleDirectHarness.Contains(
        "RECOMPONE_THROTTLE_ON_SCRIPT_STAGE = $(if ($Paced) { $pacedStage }",
        StringComparison.Ordinal),
    "Seattle natural replay no longer has an authored 60 Hz pacing gate");
Require(
    rawTrackSource.Contains(
        "RawTrackMaterialCoverageFromNclips(",
        StringComparison.Ordinal) &&
    rawTrackSource.Contains(
        "materialThreshold >= materialCoverage",
        StringComparison.Ordinal) &&
    rawTrackSource.Contains("materialLod=", StringComparison.Ordinal),
    "resident course geometry no longer preserves GT2's authored material LOD");
Require(
    worldDrawListHeader.Contains(
        "world_primitive_track_overlay_layer_mask",
        StringComparison.Ordinal) &&
    worldDrawListHeader.Contains(
        "world_primitive_track_overlay_support_flag",
        StringComparison.Ordinal) &&
    liveBridgeSource.Contains(
        "classify_resident_track_overlays(",
        StringComparison.Ordinal) &&
    liveBridgeSource.Contains(
        "resident_primitives_positive_overlap(",
        StringComparison.Ordinal) &&
    liveBridgeSource.Contains(
        "resident_overlay_is_smaller_than_support(",
        StringComparison.Ordinal) &&
    liveBridgeSource.Contains(
        "resident_elevated_overlay_relation(",
        StringComparison.Ordinal) &&
    liveBridgeSource.Contains(
        "overlay_support = true",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "PSMainRoadOverlay(",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "bounded four-view-unit depth",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "const bool use_depth =",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "render_phase == 2",
        StringComparison.Ordinal) &&
    !nativeRendererSource.Contains(
        "const bool use_road_support_mask",
        StringComparison.Ordinal),
    "resident course rendering lost typed road-artwork support priority");
Require(
    nativeRendererSource.Contains(
        "int majorTaps = clamp((int)ceil(majorLength), 3, 16);",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "int minorTaps = clamp((int)ceil(minorLength), 3, 4);",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "majorAmount * major + minorAmount * minor",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "(majorIndex + 0.5) / majorTaps) - 0.5",
        StringComparison.Ordinal),
    "world texture minification lost oriented anisotropic sampling");
Require(
    gt2CompatSource.Contains(
        "const uint authoredViewportBits = 0x003F001Eu;",
        StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "uint expanded = ApplyModernVehicleViewportMask(mask, enabled);",
        StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "public static bool ModernVehicleViewportClippingEnabled =>",
        StringComparison.Ordinal) &&
    Occurrences(simulationEnhancements, "ExpandVehicleFrustumMask(") == 1 &&
    Occurrences(arcadeEnhancements, "ExpandVehicleFrustumMask(") == 1 &&
    Occurrences(simulationEnhancements, "TraceVehicleWheelGate(") == 1 &&
    Occurrences(arcadeEnhancements, "TraceVehicleWheelGate(") == 1 &&
    Occurrences(simulationEnhancements, "TraceVehicleWheelDispatch(") == 1 &&
    Occurrences(arcadeEnhancements, "TraceVehicleWheelDispatch(") == 1 &&
    Occurrences(
        simulationEnhancements,
        "TraceVehicleWheelRendererEntry(") == 1 &&
    Occurrences(
        arcadeEnhancements,
        "TraceVehicleWheelRendererEntry(") == 1 &&
    Occurrences(
        simulationEnhancements,
        "BeginVehicleRenderIdentity(c.S0)") == 1 &&
    Occurrences(
        arcadeEnhancements,
        "BeginVehicleRenderIdentity(c.S0)") == 1 &&
    Occurrences(
        simulationEnhancements,
        "EndVehicleRenderIdentity()") == 1 &&
    Occurrences(
        arcadeEnhancements,
        "EndVehicleRenderIdentity()") == 1 &&
    simulationEnhancements.Contains(
        "\"func_800140A4\"", StringComparison.Ordinal) &&
    arcadeEnhancements.Contains(
        "\"func_800140A4\"", StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "WorldCaptureContext.BeginVehicle(vehicleIdentity, modelPointer);",
        StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "scopedCaptures=", StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "fallbackCaptures=", StringComparison.Ordinal) &&
    simulationEnhancements.Contains("L80067700: ;", StringComparison.Ordinal) &&
    arcadeEnhancements.Contains("L80067610: ;", StringComparison.Ordinal) &&
    simulationEnhancements.Contains("L8006772C: ;", StringComparison.Ordinal) &&
    arcadeEnhancements.Contains("L8006763C: ;", StringComparison.Ordinal) &&
    simulationEnhancements.Contains(
        "(track, \"func_80014708\"", StringComparison.Ordinal) &&
    simulationEnhancements.Contains(
        "(race, \"func_80048528\"", StringComparison.Ordinal) &&
    arcadeEnhancements.Contains(
        "(OVERLAY2, \"func_800146EC\"", StringComparison.Ordinal) &&
    arcadeEnhancements.Contains(
        "(OVERLAY0, \"func_80048448\"", StringComparison.Ordinal),
    "vehicle ownership, Hor+ expansion, or wheel control-flow evidence misses a renderer variant");
Require(
    !gt2CompatSource.Contains(
        "RECOMPONE_GT2_DIAGNOSTIC_SPEED_CAP_MPH",
        StringComparison.Ordinal) &&
    !gt2CompatSource.Contains(
        "public static void ApplyDiagnosticVehicleSpeedCap(",
        StringComparison.Ordinal) &&
    Occurrences(
        simulationEnhancements,
        "ApplyDiagnosticVehicleSpeedCap(") == 0 &&
    Occurrences(
        arcadeEnhancements,
        "ApplyDiagnosticVehicleSpeedCap(") == 0 &&
    Occurrences(
        ReadRepoFile(@"generated\recompiled\gt2_overlay_0.cs"),
        "ApplyDiagnosticVehicleSpeedCap(") == 0 &&
    Occurrences(
        ReadRepoFile(
            @"generated\arcade-recompiled\gt2_arcade_overlay_0.cs"),
        "ApplyDiagnosticVehicleSpeedCap(") == 0 &&
    !ssr5VehicleBoundaryHarness.Contains(
        "RECOMPONE_GT2_DIAGNOSTIC_SPEED_CAP_MPH",
        StringComparison.Ordinal) &&
    !ssr5VehicleBoundaryHarness.Contains(
        "RECOMPONE_DIAGNOSTIC_KEYBOARD_THROTTLE_PERCENT",
        StringComparison.Ordinal) &&
    !ReadRepoFile(
        @"vendor\RecompOne\RecompOne.Runtime\Host\InputManager.cs").Contains(
            "RECOMPONE_DIAGNOSTIC_KEYBOARD_THROTTLE_PERCENT",
            StringComparison.Ordinal) &&
    ssr5VehicleBoundaryHarness.Contains(
        "RECOMPONE_TRACE_GT2_VEHICLE_GTE_FLAGS = '1'",
        StringComparison.Ordinal) &&
    gteSource.Contains(
        "FilterModernVehicleProjectionLimits(FLAG)",
        StringComparison.Ordinal) &&
    gteSource.Contains(
        "[GT2-VEHICLE-GTE-FLAG]",
        StringComparison.Ordinal) &&
    ssr5VehicleBoundaryHarness.Contains(
        "special-stage-route-5",
        StringComparison.Ordinal) &&
    ssr5VehicleBoundaryHarness.Contains(
        "OPENGT_RENDER_VEHICLE_BOUNDARY_AUDIT = '1'",
        StringComparison.Ordinal) &&
    ssr5VehicleBoundaryHarness.Contains(
        "RECOMPONE_DISABLE_DISPLAY_CAPTURE = '1'",
        StringComparison.Ordinal),
    "SSR5 close-pass telemetry regressed or the unsafe physical speed cap returned");

foreach (string standaloneVehicleOverlay in new[]
{
    @"generated\recompiled\gt2_overlay_0.cs",
    @"generated\recompiled\gt2_overlay_2.cs",
    @"generated\arcade-recompiled\gt2_arcade_overlay_0.cs",
    @"generated\arcade-recompiled\gt2_arcade_overlay_2.cs",
})
{
    string source = ReadRepoFile(standaloneVehicleOverlay);
    Require(
        Occurrences(source, "ApplyModernVehicleViewportMask(c.V0)") == 1 &&
        Occurrences(source, "ModernVehicleViewportClippingEnabled") == 1,
        $"{standaloneVehicleOverlay}: standalone vehicle still uses GT2's 4:3 polygon clipper in modern mode");
}

const uint stockVehicleFrustumMask = 0x003F001Fu;
Require(
    RecompOne.Runtime.Sdk.GT2Compat.ApplyModernTrackFrustumClassification(
        0u, enabled: true) == 0u &&
    RecompOne.Runtime.Sdk.GT2Compat.ApplyModernTrackFrustumClassification(
        1u, enabled: true) == 1u &&
    RecompOne.Runtime.Sdk.GT2Compat.ApplyModernTrackFrustumClassification(
        2u, enabled: true) == 1u &&
    RecompOne.Runtime.Sdk.GT2Compat.ApplyModernTrackFrustumClassification(
        2u, enabled: false) == 2u,
    "expanded track objects did not retain GT2's intersecting packet order");
Require(
    RecompOne.Runtime.Sdk.GT2Compat.ApplyModernVehicleViewportMask(
        stockVehicleFrustumMask, enabled: false) == stockVehicleFrustumMask,
    "vehicle viewport policy no longer preserves the stock isolation mask");
Require(
    RecompOne.Runtime.Sdk.GT2Compat.ApplyModernVehicleViewportMask(
        stockVehicleFrustumMask, enabled: true) == 0x00000001u,
    "modern viewport did not delegate vehicle viewport clipping to D3D");
Require(
    RecompOne.Runtime.Sdk.GT2Compat.ApplyModernVehicleViewportMask(
        0x00010000u, enabled: true) == 0u,
    "vehicle near-plane intersection still routes through GT2's 4:3 polygon clipper");
Require(
    RecompOne.Runtime.Sdk.GT2Compat.ApplyModernVehicleViewportMask(
        0x00320012u, enabled: true) == 0u,
    "close vehicle still routes through GT2's 4:3 polygon clipper");
Require(
    presentationRendererSource.Contains(
        "Environment.GetEnvironmentVariable(\"RECOMPONE_VIDEO_CRF\")",
        StringComparison.Ordinal) &&
    presentationRendererSource.Contains(
        ": 12;",
        StringComparison.Ordinal) &&
    !presentationRendererSource.Contains(
        "\"-maxrate\"",
        StringComparison.Ordinal) &&
    !presentationRendererSource.Contains(
        "\"1500k\"",
        StringComparison.Ordinal) &&
    motionCaptureHarness.Contains(
        "RECOMPONE_VIDEO_CRF = $VideoCrf.ToString()",
        StringComparison.Ordinal) &&
    motionCaptureHarness.Contains(
        "RECOMPONE_GT2_TRUE_60HZ = '1'",
        StringComparison.Ordinal) &&
    presentationRendererSource.Contains(
        "\"60000/1001\"",
        StringComparison.Ordinal),
    "motion evidence capture is bandwidth-starved or lacks an explicit high-quality CRF");

var legacy = new ViewConfig
{
    GraphicsPreset = "PS1 Quality",
    HighResolution3D = false,
    TextureSmoothing = false,
    HighResolutionTextures = false,
    PerspectiveCorrectTextures = false,
    StabilizeGeometrySeams = false,
    ExtendedDrawDistance = false,
    LevelOfDetail = "Stock",
    Ps1Dithering = true,
};
legacy.EnforceGraphicsPreset();
RequireModern(legacy, "legacy migration");

var custom = new ViewConfig
{
    GraphicsPreset = "Custom",
    HighResolution3D = false,
    TextureSmoothing = false,
    HighResolutionTextures = false,
    PerspectiveCorrectTextures = false,
    StabilizeGeometrySeams = false,
    ExtendedDrawDistance = false,
    LevelOfDetail = "Stock",
    Ps1Dithering = true,
};
custom.EnforceGraphicsPreset();
RequireModern(custom, "custom migration");

var explicitLegacy = new ViewConfig();
explicitLegacy.ApplyGraphicsPreset("PS1 Quality");
RequireModern(explicitLegacy, "legacy preset request");

var runtimeDowngrade = new ViewConfig();
runtimeDowngrade.HighResolution3D = false;
runtimeDowngrade.TextureSmoothing = false;
runtimeDowngrade.HighResolutionTextures = false;
runtimeDowngrade.PerspectiveCorrectTextures = false;
runtimeDowngrade.StabilizeGeometrySeams = false;
runtimeDowngrade.ExtendedDrawDistance = false;
runtimeDowngrade.LevelOfDetail = "Stock";
runtimeDowngrade.Ps1Dithering = true;
runtimeDowngrade.GraphicsPreset = "Custom";
RequireModern(runtimeDowngrade, "runtime downgrade request");

Environment.SetEnvironmentVariable("RECOMPONE_NATIVE_WORLD_RENDERER", "0");
Type rendererType = typeof(ViewConfig).Assembly.GetType(
    "RecompOne.Runtime.Hle.LiveWorldRenderer",
    throwOnError: true)!;
Type gt2CompatType = typeof(ViewConfig).Assembly.GetType(
    "RecompOne.Runtime.Sdk.GT2Compat",
    throwOnError: true)!;
string ssr5ClassCPreBase64 = (string)gt2CompatType.GetField(
    "DirectSpecialStageRoute5PreFinalizeConfigBase64",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
string ssr5ClassCFinalBase64 = (string)gt2CompatType.GetField(
    "DirectSpecialStageRoute5RaceConfigBase64",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
string ssr5ClassCStateBase64 = (string)gt2CompatType.GetField(
    "DirectSpecialStageRoute5RaceStateBase64",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
byte[] ssr5ClassCPre = Convert.FromBase64String(ssr5ClassCPreBase64);
byte[] ssr5ClassCFinal = Convert.FromBase64String(ssr5ClassCFinalBase64);
byte[] ssr5ClassCState = Convert.FromBase64String(ssr5ClassCStateBase64);
MethodInfo replaceDirectArcadeCourseIdentity = gt2CompatType.GetMethod(
    "ReplaceDirectArcadeCourseIdentity",
    BindingFlags.NonPublic | BindingFlags.Static)!;
byte[] trialMountainClassCPre = (byte[])ssr5ClassCPre.Clone();
byte[] trialMountainClassCFinal = (byte[])ssr5ClassCFinal.Clone();
byte[] trialMountainClassCState = (byte[])ssr5ClassCState.Clone();
replaceDirectArcadeCourseIdentity.Invoke(
    null, [trialMountainClassCPre, false]);
replaceDirectArcadeCourseIdentity.Invoke(
    null, [trialMountainClassCFinal, false]);
replaceDirectArcadeCourseIdentity.Invoke(
    null, [trialMountainClassCState, true]);
int ssr5PlayerNameStart = 0x5C + 0x90;
int ssr5PlayerNameLength = Array.IndexOf(
    ssr5ClassCState,
    (byte)0,
    ssr5PlayerNameStart) - ssr5PlayerNameStart;
string ssr5PlayerName = System.Text.Encoding.ASCII.GetString(
    ssr5ClassCState,
    ssr5PlayerNameStart,
    ssr5PlayerNameLength);
Require(
    ssr5ClassCPre.Length == 0x2D4 &&
    ssr5ClassCPre[1] == 3 &&
    BitConverter.ToUInt32(ssr5ClassCPre, 0x0C) == 0x10362258u &&
    BitConverter.ToUInt32(ssr5ClassCPre, 0x10) == 0x10362258u &&
    ssr5ClassCFinal.Length == 0x2D4 &&
    ssr5ClassCState.Length == 0x58C &&
    ssr5PlayerName == "Citroen Xsara 1.8i 16V" &&
    Convert.ToHexString(
        System.Security.Cryptography.SHA256.HashData(ssr5ClassCPre)) ==
        "71FF6B22EC08B6C7516469BA24C0947B2470069436CDE80122A298E1497E0897" &&
    Convert.ToHexString(
        System.Security.Cryptography.SHA256.HashData(ssr5ClassCFinal)) ==
        "EDCC10480B919EEB5232E59220006ACF64B6509F7AC7AE99F5915F1D962A65BD" &&
    Convert.ToHexString(
        System.Security.Cryptography.SHA256.HashData(ssr5ClassCState)) ==
        "D5EF736926371AB91A89F9E058399B3D95867BA18DAC4790040CE337C580AF08",
    "direct SSR5 launch no longer carries the native Class C Xsara fixture");
string trialMountainCourseName = System.Text.Encoding.ASCII.GetString(
    trialMountainClassCPre,
    0xB8,
    Array.IndexOf(
        trialMountainClassCPre,
        (byte)0,
        0xB8) - 0xB8);
Require(
    trialMountainCourseName == "Trial Mountain Circuit" &&
    BitConverter.ToUInt32(trialMountainClassCPre, 0x1B8) == 0xAFD7E5BBu &&
    BitConverter.ToUInt32(trialMountainClassCFinal, 0x1B8) == 0xAFD7E5BBu &&
    BitConverter.ToUInt32(trialMountainClassCState, 0x40) == 0xAFD7E5BBu &&
    Convert.ToHexString(
        System.Security.Cryptography.SHA256.HashData(
            trialMountainClassCPre)) ==
        "B8C0A7851D52B1EA6907B9E1ABCA45B29822BD0A256F0C0FE3C4047AF37E690F" &&
    Convert.ToHexString(
        System.Security.Cryptography.SHA256.HashData(
            trialMountainClassCFinal)) ==
        "0440EB081039BD92E68AF0D78E90840DEE0944028A850F3E66A3CF839B3389E9" &&
    Convert.ToHexString(
        System.Security.Cryptography.SHA256.HashData(
            trialMountainClassCState)) ==
        "CA4E3C635A36EA2A9948573BB6A54114B91A5ED46D3732A5B8279A0DC15C9D44",
    "direct Trial Mountain launch lost its Class C native course identity");
Require(
    arcadeRendererAuditHarness.Contains(
        "@('--arcade-race', 'trial-mountain', $data)",
        StringComparison.Ordinal) &&
    arcadeRendererAuditHarness.Contains(
        "RECOMPONE_DISABLE_LIVE_INPUT = $(if ($HeadlessTest) { '1' } else { $null })",
        StringComparison.Ordinal) &&
    arcadeRendererAuditHarness.Contains(
        "RECOMPONE_DISABLE_DISPLAY_CAPTURE = '1'",
        StringComparison.Ordinal) &&
    arcadeRendererAuditHarness.Contains(
        "RECOMPONE_GT2_ARCADE_UNLOCK_ALL_COURSES = $null",
        StringComparison.Ordinal) &&
    arcadeRendererAuditHarness.Contains(
        "launcher-latest.log",
        StringComparison.Ordinal) &&
    arcadeRendererAuditHarness.Contains(
        "if ($process.HasExited)",
        StringComparison.Ordinal) &&
    arcadeRendererAuditHarness.Contains(
        "[switch]$HeadlessTest",
        StringComparison.Ordinal) &&
    arcadeRendererAuditHarness.Contains(
        "native Trial Mountain Circuit construction verified",
        StringComparison.Ordinal) &&
    arcadeRendererAuditHarness.Contains(
        "arcade_renderer_audit_headless_test=pass",
        StringComparison.Ordinal) &&
    arcadeRendererAuditHarness.Contains(
        "$startParameters.Wait = $true",
        StringComparison.Ordinal) &&
    !arcadeRendererAuditHarness.Contains(
        "$process.WaitForExit(",
        StringComparison.Ordinal) &&
    arcadeRendererAuditLauncher.Contains(
        "-ExecutionPolicy Bypass -File",
        StringComparison.Ordinal) &&
    arcadeRendererAuditLauncher.Contains(
        "pause",
        StringComparison.Ordinal),
    "Arcade renderer audit no longer launches direct Trial Mountain with live input and captures disabled");
bool true60GuestFixed = (bool)gt2CompatType.GetField(
    "True60HzEnabled",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
Require(
    true60GuestFixed,
    "shipping process did not lock authored per-VBlank operation");
Type nativeMethodsType = rendererType.GetNestedType(
    "NativeMethods",
    BindingFlags.NonPublic)!;
Require(
    rendererType.Assembly.GetType(
        "RecompOne.Runtime.Hle.LiveInterpolationStats",
        throwOnError: false) is null &&
    nativeMethodsType.GetMethod(
        "RenderPair",
        BindingFlags.NonPublic | BindingFlags.Static) is null &&
    nativeMethodsType.GetMethod(
        "TryReadPair",
        BindingFlags.NonPublic | BindingFlags.Static) is null,
    "managed runtime still carries a synthetic-frame native ABI");
bool rendererRequested = (bool)rendererType.GetProperty(
    "Requested",
    BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
Require(
    !OperatingSystem.IsWindows() || rendererRequested,
    "former disable environment variable still disabled native rendering");

foreach (string generatedOverlay in new[]
{
    @"generated\recompiled\gt2_overlay_0.cs",
    @"generated\arcade-recompiled\gt2_arcade_overlay_0.cs",
})
{
    string source = ReadRepoFile(generatedOverlay);
    Require(
        Occurrences(source, "Gte.BeginDerivedScreenProjection(") == 2 &&
        Occurrences(source, "0x31525353u") == 2 &&
        Occurrences(source, "Gte.EndDerivedScreenProjection();") == 2,
        $"{generatedOverlay}: auxiliary billboard projection scopes are not balanced");
}
int outputReadyWaitMilliseconds = (int)rendererType.GetField(
    "OutputReadyWaitMilliseconds",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
Require(
    outputReadyWaitMilliseconds is >= 1 and <= 12,
    "native output wait exceeded the bounded NTSC idle margin");
int liveTriangleStride = (int)rendererType.GetField(
    "TriangleStride",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
Require(
    liveTriangleStride == 384,
    "live world capture is not using the v6 per-vertex transform stride");
int outputBufferCount = (int)rendererType.GetField(
    "OutputBufferCount",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
int publishedOutputCapacity = (int)rendererType.GetField(
    "PublishedOutputCapacity",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
Require(
    outputBufferCount == 4 && publishedOutputCapacity == 3,
    "native output ring no longer covers the bounded capture work window");

Type recorderType = rendererType.Assembly.GetType(
    "RecompOne.Runtime.Hle.LiveWorldFrameRecorder",
    throwOnError: true)!;
int reservationRecords = (int)recorderType.GetField(
    "TriangleReservationRecords",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
Require(
    reservationRecords is >= 16 and <= 256,
    "direct capture writer reservation is not bounded in useful batches");
MethodInfo ensureTriangleStorage = recorderType.GetMethod(
    "EnsureTriangleStorage",
    BindingFlags.NonPublic | BindingFlags.Static)!;
const int captureHeaderBytes = 160;
const int triangleRecordBytes = 224;
byte[] captureBacking = new byte[128 * 1024];
using (var captureStream = new MemoryStream(
    captureBacking,
    0,
    captureBacking.Length,
    writable: true,
    publiclyVisible: true))
{
    captureStream.SetLength(captureHeaderBytes);
    captureBacking[0] = 0x4F;
    captureBacking[captureHeaderBytes - 1] = 0xA5;
    long triangleEnd = captureHeaderBytes + triangleRecordBytes;
    ensureTriangleStorage.Invoke(
        null,
        [captureStream, triangleEnd, (long)captureBacking.Length]);
    Require(
        captureStream.Length >= triangleEnd,
        "direct triangle backing bytes were not exposed to MemoryStream");
    captureBacking[captureHeaderBytes] = 0x71;
    captureBacking[triangleEnd - 1] = 0xE3;
    captureStream.Position = triangleEnd;
    // The frame finalizer grows/shrinks to the exact triangle + VRAM extent.
    // Both ends of the directly written record must survive that operation.
    captureStream.SetLength(64 * 1024);
    Require(
        captureBacking[0] == 0x4F &&
        captureBacking[captureHeaderBytes - 1] == 0xA5 &&
        captureBacking[captureHeaderBytes] == 0x71 &&
        captureBacking[triangleEnd - 1] == 0xE3,
        "MemoryStream finalization erased direct triangle capture bytes");
}

Type gpuType = rendererType.Assembly.GetType(
    "RecompOne.Runtime.Gpu",
    throwOnError: true)!;
MethodInfo billboardCoordinates = gpuType.GetMethod(
    "RawTrackBillboardCoordinates",
    BindingFlags.NonPublic | BindingFlags.Static)!;
object[] billboardInputs =
[
    false,
    (short)1000,
    (short)2000,
    (short)3000,
    (short)100,
    (ushort)400,
    (short)2048,
    (short)1024,
];
short[] primaryBillboard = ReadBillboardCoordinates(
    billboardCoordinates.Invoke(null, billboardInputs)!);
billboardInputs[0] = true;
short[] auxiliaryBillboard = ReadBillboardCoordinates(
    billboardCoordinates.Invoke(null, billboardInputs)!);
Require(
    primaryBillboard.SequenceEqual(new short[]
    {
        950, 2025, 3400,
        1050, 1975, 3400,
        950, 2025, 3000,
        1050, 1975, 3000,
    }) &&
    auxiliaryBillboard.SequenceEqual(new short[]
    {
        950, 2400, 2975,
        1050, 2400, 3025,
        950, 2000, 2975,
        1050, 2000, 3025,
    }),
    "primary and auxiliary GT2 billboard axes were conflated");
MethodInfo quadPacketCorner = gpuType.GetMethod(
    "RawTrackQuadPacketCorner",
    BindingFlags.NonPublic | BindingFlags.Static)!;
MethodInfo quadPacketUvCorner = gpuType.GetMethod(
    "RawTrackQuadPacketUvCorner",
    BindingFlags.NonPublic | BindingFlags.Static)!;
int[] ReadQuadPacketMap(
    MethodInfo method,
    TrackMeshProjectionPath path,
    int triangle) => Enumerable.Range(0, 3).Select(vertex =>
        (int)method.Invoke(null, [path, triangle, vertex])!).ToArray();
Require(
    ReadQuadPacketMap(
        quadPacketCorner,
        TrackMeshProjectionPath.Primary,
        0).SequenceEqual([3, 1, 0]) &&
    ReadQuadPacketMap(
        quadPacketCorner,
        TrackMeshProjectionPath.Primary,
        1).SequenceEqual([1, 2, 3]) &&
    ReadQuadPacketMap(
        quadPacketUvCorner,
        TrackMeshProjectionPath.Primary,
        0).SequenceEqual([3, 1, 0]) &&
    ReadQuadPacketMap(
        quadPacketUvCorner,
        TrackMeshProjectionPath.Primary,
        1).SequenceEqual([1, 2, 3]) &&
    ReadQuadPacketMap(
        quadPacketCorner,
        TrackMeshProjectionPath.Alternate,
        0).SequenceEqual([0, 1, 3]) &&
    ReadQuadPacketMap(
        quadPacketCorner,
        TrackMeshProjectionPath.Alternate,
        1).SequenceEqual([1, 3, 2]) &&
    ReadQuadPacketMap(
        quadPacketUvCorner,
        TrackMeshProjectionPath.Alternate,
        0).SequenceEqual([0, 1, 3]) &&
    ReadQuadPacketMap(
        quadPacketUvCorner,
        TrackMeshProjectionPath.Alternate,
        1).SequenceEqual([1, 3, 2]),
    "indexed course quad packet order no longer matches GT2's two projection paths");
MethodInfo primaryQuadAccepted = gpuType.GetMethod(
    "RawTrackPrimaryQuadAccepted",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    (bool)primaryQuadAccepted.Invoke(null, [true, 2.0, 3.0])! &&
    (bool)primaryQuadAccepted.Invoke(null, [true, -2.0, -3.0])! &&
    !(bool)primaryQuadAccepted.Invoke(null, [true, -2.0, 3.0])! &&
    !(bool)primaryQuadAccepted.Invoke(null, [true, 0.0, 0.0])! &&
    (bool)primaryQuadAccepted.Invoke(null, [false, -2.0, 0.0])!,
    "primary course quad facing no longer preserves GT2's two NCLIP half decisions");
MethodInfo alternateQuadAccepted = gpuType.GetMethod(
    "RawTrackAlternateQuadAccepted",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    (bool)alternateQuadAccepted.Invoke(null, [true, -2.0, -3.0])! &&
    (bool)alternateQuadAccepted.Invoke(null, [true, 2.0, 3.0])! &&
    !(bool)alternateQuadAccepted.Invoke(null, [true, 2.0, -3.0])! &&
    !(bool)alternateQuadAccepted.Invoke(null, [true, 0.0, 0.0])! &&
    (bool)alternateQuadAccepted.Invoke(null, [false, 2.0, 0.0])!,
    "alternate course quad oracle no longer matches GT2's two NCLIP orders");
MethodInfo quadTriangleAccepted = gpuType.GetMethod(
    "RawTrackQuadTriangleAccepted",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    (bool)quadTriangleAccepted.Invoke(
        null,
        [true, TrackMeshProjectionPath.Primary, 0, 2.0])! &&
    !(bool)quadTriangleAccepted.Invoke(
        null,
        [true, TrackMeshProjectionPath.Primary, 0, -2.0])! &&
    (bool)quadTriangleAccepted.Invoke(
        null,
        [true, TrackMeshProjectionPath.Primary, 1, -2.0])! &&
    (bool)quadTriangleAccepted.Invoke(
        null,
        [true, TrackMeshProjectionPath.Alternate, 0, -2.0])! &&
    (bool)quadTriangleAccepted.Invoke(
        null,
        [true, TrackMeshProjectionPath.Alternate, 1, 2.0])! &&
    !(bool)quadTriangleAccepted.Invoke(
        null,
        [true, TrackMeshProjectionPath.Alternate, 1, -2.0])!,
    "modern course quad halves no longer use independent authored winding");
MethodInfo fixedViewDeterminant = gpuType.GetMethod(
    "RawTrackFixedViewDeterminant",
    BindingFlags.NonPublic | BindingFlags.Static)!;
double positiveSubpixelFacing = (double)fixedViewDeterminant.Invoke(
    null,
    [
        0L, 0L, 4096L,
        1L, 0L, 4096L,
        0L, 1L, 4096L,
    ])!;
double negativeSubpixelFacing = (double)fixedViewDeterminant.Invoke(
    null,
    [
        0L, 0L, 4096L,
        0L, 1L, 4096L,
        1L, 0L, 4096L,
    ])!;
Require(
    positiveSubpixelFacing == 4096.0 &&
    negativeSubpixelFacing == -4096.0,
    "course facing discarded the GTE transform's twelve fractional bits");
MethodInfo projectFixedAxis = gpuType.GetMethod(
    "RawTrackProjectAxis",
    BindingFlags.NonPublic | BindingFlags.Static)!;
double fixedHalfPixel = (double)projectFixedAxis.Invoke(
    null,
    [2048L, 4096L, 0, (ushort)1, 0])!;
double fixedProjectionCenter = (double)projectFixedAxis.Invoke(
    null,
    [4096L, 0L, 65536, (ushort)320, 5])!;
Require(
    fixedHalfPixel == 0.5 && fixedProjectionCenter == 6.0,
    "course projection discarded the GTE transform's twelve fractional bits");
MethodInfo cameraCutStep = gpuType.GetMethod(
    "RawTrackCameraCutStep",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    !(bool)cameraCutStep.Invoke(null, [0.49, 1999.0])! &&
    (bool)cameraCutStep.Invoke(null, [0.5, 0.0])! &&
    (bool)cameraCutStep.Invoke(null, [0.0, 2000.0])!,
    "course continuity audit no longer separates replay camera cuts");
MethodInfo nearClippedAccepted = gpuType.GetMethod(
    "RawTrackNearClippedAccepted",
    BindingFlags.NonPublic | BindingFlags.Static)!;
const long nearFixed = 16L * 4096L;
Require(
    (bool)nearClippedAccepted.Invoke(
        null,
        [
            0L, 0L, nearFixed + 4096L,
            4096L, 0L, nearFixed + 4096L,
            0L, 4096L, nearFixed + 4096L,
            true, true,
        ])! &&
    !(bool)nearClippedAccepted.Invoke(
        null,
        [
            0L, 0L, nearFixed,
            4096L, 0L, nearFixed - 4096L,
            0L, 4096L, nearFixed - 4096L,
            false, true,
        ])!,
    "near-plane course facing does not reject a clipped tangent polygon");
MethodInfo materialCoverage = gpuType.GetMethod(
    "RawTrackMaterialCoverageFromNclips",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    (uint)materialCoverage.Invoke(null, [500, 0, false])! == 500u &&
    (uint)materialCoverage.Invoke(null, [-1, 0, false])! == uint.MaxValue &&
    (uint)materialCoverage.Invoke(null, [300, -500, true])! == 800u &&
    (uint)materialCoverage.Invoke(null, [-700, 200, true])! == 900u &&
    (uint)materialCoverage.Invoke(null, [400, 250, true])! == 150u,
    "authored course material LOD no longer matches GT2's FIFO NCLIP arithmetic");
Require(
    liveBridgeSource.Contains(
        "static_cast<std::uint32_t>(second) -",
        StringComparison.Ordinal) &&
    liveBridgeSource.Contains(
        "static_cast<std::uint32_t>(first);",
        StringComparison.Ordinal) &&
    liveBridgeSource.Contains("renderedFar=%llu", StringComparison.Ordinal) &&
    !liveBridgeSource.Contains(
        "material = primitive.near_material;",
        StringComparison.Ordinal),
    "native resident course rendering no longer preserves GT2's authored material selection");
Require(
    oggMusicSource.Contains("public static void PauseMusicStream()", StringComparison.Ordinal) &&
    oggMusicSource.Contains("public static void PrepareForCdPlay()", StringComparison.Ordinal) &&
    libCdSource.Contains("case Pause:", StringComparison.Ordinal) &&
    libCdSource.Contains("case Stop: case Init:", StringComparison.Ordinal) &&
    !libCdSource.Contains("case Pause: case Stop", StringComparison.Ordinal),
    "CD Pause no longer preserves the external music decoder for resume");
MethodInfo clippedTriangleArea = gpuType.GetMethod(
    "RawTrackClippedTriangleArea",
    BindingFlags.NonPublic | BindingFlags.Static)!;
double insideArea = (double)clippedTriangleArea.Invoke(
    null,
    [0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 1.0])!;
double partialArea = (double)clippedTriangleArea.Invoke(
    null,
    [-1.0, 0.0, 1.0, 0.0, 1.0, 1.0, 0.0, 0.0, 1.0, 1.0])!;
double outsideArea = (double)clippedTriangleArea.Invoke(
    null,
    [-2.0, 0.0, -1.0, 0.0, -1.0, 1.0, 0.0, 0.0, 1.0, 1.0])!;
Require(
    Math.Abs(insideArea - 0.5) < 1e-12 &&
    Math.Abs(partialArea - 0.75) < 1e-12 &&
    outsideArea == 0.0,
    "temporal target-coverage audit does not clip projected triangles exactly");
MethodInfo compatibilityRasterDecision = gpuType
    .GetMethod(
        "ShouldRasterizeCompatibilityTriangle",
        BindingFlags.NonPublic | BindingFlags.Static)!;
bool rasterizesWorld = (bool)compatibilityRasterDecision.Invoke(
    null,
    [true])!;
bool rasterizesScreen = (bool)compatibilityRasterDecision.Invoke(
    null,
    [false])!;
Require(
    !OperatingSystem.IsWindows() || !rasterizesWorld,
    "provenance-backed 3D can still reach a compatibility rasterizer");
Require(
    rasterizesScreen,
    "authored screen-space command composition was disabled");
Require(
    liveRendererSource.Contains(
        "the release build has no compatibility fallback.",
        StringComparison.Ordinal) &&
    liveRendererSource.Contains(
        "The release build has no compositor fallback.",
        StringComparison.Ordinal) &&
    !liveRendererSource.Contains(
        "[Native-World] compositor fallback",
        StringComparison.Ordinal),
    "native failure or oversize output can silently degrade to a compositor-only run");
Require(
    rendererType.GetMethod(
        "CommandMatchRate",
        BindingFlags.NonPublic | BindingFlags.Static) is null,
    "retired interpolation diagnostics remain in the live runtime");
Type gteType = rendererType.Assembly.GetType(
    "RecompOne.Runtime.Gte",
    throwOnError: true)!;
MethodInfo filterVehicleProjectionLimits = gteType.GetMethod(
    "FilterVehicleProjectionSummary",
    BindingFlags.NonPublic | BindingFlags.Static)!;
uint screenOnlyFlag = (uint)filterVehicleProjectionLimits.Invoke(
    null,
    [0x80006000u, true])!;
uint screenAndDivideFlag = (uint)filterVehicleProjectionLimits.Invoke(
    null,
    [0x80026000u, true])!;
uint depthAndDivideFlag = (uint)filterVehicleProjectionLimits.Invoke(
    null,
    [0x80060000u, true])!;
uint screenAndMatrixFlag = (uint)filterVehicleProjectionLimits.Invoke(
    null,
    [0xC0006000u, true])!;
uint nonProjectionFlag = (uint)filterVehicleProjectionLimits.Invoke(
    null,
    [0x80026000u, false])!;
Require(
    screenOnlyFlag == 0u &&
    screenAndDivideFlag == 0u &&
    depthAndDivideFlag == 0u &&
    screenAndMatrixFlag == 0u &&
    nonProjectionFlag == 0x80026000u,
    "vehicle GTE policy does not delegate the complete vehicle projection summary while preserving non-projection errors");
MethodInfo packetOriginCoordinatesMatch = gteType.GetMethod(
    "PacketOriginCoordinatesMatch",
    BindingFlags.NonPublic | BindingFlags.Static)!;
const int packetX = 287;
const int packetY = 38;
uint packedPacketXy =
    (uint)((ushort)(short)packetX |
        ((uint)(ushort)(short)packetY << 16));
Require(
    (bool)packetOriginCoordinatesMatch.Invoke(
        null,
        [packedPacketXy, packetX, packetY])! &&
    !(bool)packetOriginCoordinatesMatch.Invoke(
        null,
        [packedPacketXy, packetX + 1, packetY])! &&
    !(bool)packetOriginCoordinatesMatch.Invoke(
        null,
        [packedPacketXy, packetX, packetY + 1])!,
    "reused packet words can inherit stale world projection origins");
MethodInfo liveWorldExpectedFromHistory = gpuType.GetMethod(
    "IsLiveWorldExpectedFromHistory",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    !(bool)liveWorldExpectedFromHistory.Invoke(null, [false, 0])! &&
    (bool)liveWorldExpectedFromHistory.Invoke(null, [true, 0])! &&
    (bool)liveWorldExpectedFromHistory.Invoke(null, [true, 1])! &&
    (bool)liveWorldExpectedFromHistory.Invoke(null, [true, 2])! &&
    (bool)liveWorldExpectedFromHistory.Invoke(null, [true, 6])! &&
    !(bool)liveWorldExpectedFromHistory.Invoke(null, [false, 6])!,
    "native world ownership still mistakes SSR11 packet gaps for scene handoffs");

Type hostWindowType = rendererType.Assembly.GetType(
    "RecompOne.Runtime.Host.HostWindow",
    throwOnError: true)!;
Type configManagerType = rendererType.Assembly.GetType(
    "RecompOne.Runtime.Config.ConfigManager",
    throwOnError: true)!;
MethodInfo hasDockedOutputLayout = configManagerType.GetMethod(
    "HasDockedOutputLayout",
    BindingFlags.NonPublic | BindingFlags.Static)!;
const string incompleteLayout =
    "[Window][Debug##Default]\nPos=60,60\n\n[Docking][Data]\n";
const string dockedOutputLayout =
    "[Window][Output]\nDockId=0xBBE43958\n\n[Docking][Data]\n" +
    "DockSpace ID=0xBBE43958 Size=1920,1042 CentralNode=1\n";
Require(
    !(bool)hasDockedOutputLayout.Invoke(null, [incompleteLayout])! &&
    (bool)hasDockedOutputLayout.Invoke(null, [dockedOutputLayout])!,
    "incomplete interface layout can suppress the full-window Output dock");
Type frameClockType = rendererType.Assembly.GetType(
    "RecompOne.Runtime.Host.FrameClock",
    throwOnError: true)!;
double maximumCatchUpDebtMs = (double)frameClockType.GetField(
    "MaximumCatchUpDebtMs",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
double frameMs = (double)frameClockType.GetField(
    "FrameMs",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
Require(
    maximumCatchUpDebtMs >= frameMs * 2.0 &&
    maximumCatchUpDebtMs <= frameMs * 2.01,
    "frame clock can accelerate gameplay to repay long scheduler stalls");
MethodInfo shouldResetDeadline = frameClockType.GetMethod(
    "ShouldResetDeadline",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    !(bool)shouldResetDeadline.Invoke(null, [-frameMs * 1.99])! &&
    (bool)shouldResetDeadline.Invoke(null, [-frameMs * 2.01])!,
    "frame clock recovery boundary does not enforce two NTSC intervals");
MethodInfo shouldThrottleForStage = frameClockType.GetMethod(
    "ShouldThrottleForStage",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    !(bool)shouldThrottleForStage.Invoke(
        null,
        [false, "setup", "race_1"])! &&
    (bool)shouldThrottleForStage.Invoke(
        null,
        [false, "race_1", "race_1"])! &&
    (bool)shouldThrottleForStage.Invoke(
        null,
        [true, "replay_1", "race_1"])!,
    "script-stage release pacing does not remain latched after scene change");
int prebufferOutputs = (int)hostWindowType.GetField(
    "NativeWorldPrebufferOutputs",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
Require(
    prebufferOutputs == 3,
    "native presentation reserve no longer covers two-presentation GPU tails");
MethodInfo nativeStaleDiscardBeforePoll = hostWindowType.GetMethod(
    "SelectNativeWorldStaleDiscardBeforePoll",
    BindingFlags.NonPublic | BindingFlags.Static)!;
MethodInfo nativeShouldStartPrebuffer = hostWindowType.GetMethod(
    "ShouldStartNativeWorldPrebuffer",
    BindingFlags.NonPublic | BindingFlags.Static)!;
MethodInfo nativeShouldPreserveInitialPrebuffer = hostWindowType.GetMethod(
    "ShouldPreserveInitialNativeWorldPrebuffer",
    BindingFlags.NonPublic | BindingFlags.Static)!;
MethodInfo nativeOutputWaitMilliseconds = hostWindowType.GetMethod(
    "SelectNativeWorldOutputWaitMilliseconds",
    BindingFlags.NonPublic | BindingFlags.Static)!;
MethodInfo nativePendingOutputHold = hostWindowType.GetMethod(
    "ShouldHoldForPendingNativeWorldOutput",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    (int)nativeStaleDiscardBeforePoll.Invoke(null, [1000])! == 993,
    "new native world segments can still count stale queued outputs");
Require(
    (bool)nativeShouldStartPrebuffer.Invoke(
        null,
        [true, true, true, 0])! &&
    !(bool)nativeShouldStartPrebuffer.Invoke(
        null,
        [false, false, true, 0])! &&
    !(bool)nativeShouldStartPrebuffer.Invoke(
        null,
        [true, false, true, 0])! &&
    !(bool)nativeShouldStartPrebuffer.Invoke(
        null,
        [true, false, true, 10])! &&
    (bool)nativeShouldStartPrebuffer.Invoke(
        null,
        [true, false, true, 11])! &&
    (bool)nativeShouldStartPrebuffer.Invoke(
        null,
        [true, false, false, 1])!,
    "short in-race native ownership gaps still restart the output prebuffer");
Require(
    (bool)nativeShouldPreserveInitialPrebuffer.Invoke(
        null,
        [true, 0])! &&
    (bool)nativeShouldPreserveInitialPrebuffer.Invoke(
        null,
        [true, 1])! &&
    (bool)nativeShouldPreserveInitialPrebuffer.Invoke(
        null,
        [true, 10])! &&
    !(bool)nativeShouldPreserveInitialPrebuffer.Invoke(
        null,
        [true, 11])! &&
    !(bool)nativeShouldPreserveInitialPrebuffer.Invoke(
        null,
        [false, 1])!,
    "release-paced native reserve cannot survive a bounded ownership gap");
Require(
    (int)nativeOutputWaitMilliseconds.Invoke(null, [true, true])! == 12 &&
    (int)nativeOutputWaitMilliseconds.Invoke(null, [true, false])! == 0 &&
    (int)nativeOutputWaitMilliseconds.Invoke(null, [false, true])! == 0,
    "native output wait can still run outside paced 60 Hz world evidence");
Require(
    (bool)nativePendingOutputHold.Invoke(
        null,
        [true, false, false, true, 8])! &&
    (bool)nativePendingOutputHold.Invoke(
        null,
        [true, false, false, true, 17])! &&
    !(bool)nativePendingOutputHold.Invoke(
        null,
        [true, false, false, true, 18])! &&
    !(bool)nativePendingOutputHold.Invoke(
        null,
        [true, false, false, false, 7])! &&
    !(bool)nativePendingOutputHold.Invoke(
        null,
        [true, true, false, true, 7])!,
    "pending native output hold no longer stays bounded to short producer tails");
MethodInfo nativePresentationDecision = hostWindowType.GetMethod(
    "ShouldPresentNativeWorld",
    BindingFlags.NonPublic | BindingFlags.Static)!;
bool presentsNewWorld = (bool)nativePresentationDecision.Invoke(
    null,
    [true, true, false, true])!;
bool presentsRecentWorld = (bool)nativePresentationDecision.Invoke(
    null,
    [true, false, true, false])!;
bool suppressesResetBoundaryReuse = (bool)nativePresentationDecision.Invoke(
    null,
    [true, false, true, true])!;
bool presentsStaleWorld = (bool)nativePresentationDecision.Invoke(
    null,
    [true, false, false, false])!;
bool presentsAfterWorldHandoff = (bool)nativePresentationDecision.Invoke(
    null,
    [false, true, true, false])!;
Require(presentsNewWorld, "new native output was suppressed");
Require(presentsRecentWorld, "bounded current-segment native output was suppressed");
Require(
    !suppressesResetBoundaryReuse,
    "single-output temporal reset can still be reused as native motion");
Require(!presentsStaleWorld, "stale native texture can leak across a scene transition");
Require(
    !presentsAfterWorldHandoff,
    "native output can outlive authored world ownership");
MethodInfo nativeTemporalResetBoundaryHold = hostWindowType.GetMethod(
    "ShouldHoldTemporalResetBoundary",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    (bool)nativeTemporalResetBoundaryHold.Invoke(
        null,
        [true, false, true, true])! &&
    !(bool)nativeTemporalResetBoundaryHold.Invoke(
        null,
        [true, true, true, true])! &&
    !(bool)nativeTemporalResetBoundaryHold.Invoke(
        null,
        [true, false, true, false])!,
    "single-output temporal reset is not reported as a transition hold");
MethodInfo nativeOutputAgeEligible = hostWindowType.GetMethod(
    "IsNativeWorldOutputAgeEligible",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    !(bool)nativeOutputAgeEligible.Invoke(null, [-1])! &&
    (bool)nativeOutputAgeEligible.Invoke(null, [0])! &&
    (bool)nativeOutputAgeEligible.Invoke(null, [6])! &&
    (bool)nativeOutputAgeEligible.Invoke(null, [7])! &&
    !(bool)nativeOutputAgeEligible.Invoke(null, [8])!,
    "native texture reuse age guard no longer rejects stale frames");

Console.WriteLine(
    "modern_renderer_config=pass legacy_migration=pass " +
    "custom_migration=pass runtime_downgrade_removed=pass " +
    "native_disable_removed=pass legacy_world_rasterization_removed=pass " +
    "screen_compositor_retained=pass stale_world_transition_blocked=pass " +
    "bounded_output_wait=pass authored_output_prebuffer=pass " +
    "native_output_ring=pass capture_stream_reservation=pass " +
    "native_capture_v6=pass " +
    "true60_shipping_default=pass " +
    "no_argument_startup=pass " +
    "synthetic_runtime_abi_absent=pass " +
    "auxiliary_billboard_projection_scopes=pass " +
    "stage_throttle_latch=pass output_dock_validation=pass " +
    "native_reuse_age_guard=pass packet_origin_coordinate_guard=pass " +
    "bounded_world_ownership=pass cpu_projection_fast_path=pass " +
    "projection_origin_handle_flow=pass " +
    "continuous_track_facing=pass exact_face_oracle=pass " +
    "authored_material_lod=pass oriented_minification=pass");
