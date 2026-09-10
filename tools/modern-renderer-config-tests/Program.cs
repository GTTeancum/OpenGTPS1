using RecompOne.Runtime;
using RecompOne.Runtime.Config;
using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;
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

static bool RepoFileExists(string relativePath)
{
    foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        for (DirectoryInfo? directory = new(start); directory != null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, relativePath)))
                return true;
    return false;
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

static PSMemory VerifyArcadeFrontendContracts()
{
    var memory = new PSMemory();
    const uint stock = 0x80050730u;
    const uint expanded = 0x800533C0u;
    memory.WriteU32(stock, 0x80050048u);
    memory.WriteU32(stock + 4u, 0x80050424u);
    memory.WriteU32(stock + 8u, 0x80050670u);
    Require(
        RecompOne.Runtime.Sdk.GT2Compat.ResolveArcadeCourseTable(
            stock, expanded, memory) == stock,
        "stock GT2 overlay did not reject the absent expanded course table");

    memory.WriteU32(expanded, 0x80050048u);
    memory.WriteU32(expanded + 4u, 0x80050424u);
    memory.WriteU32(expanded + 8u, 0x80050670u);
    Require(
        RecompOne.Runtime.Sdk.GT2Compat.ResolveArcadeCourseTable(
            stock, expanded, memory) == expanded,
        "converted Arcade overlay did not retain its expanded course table");

    RecompOne.Runtime.Sdk.GT2Compat.InstallUnifiedTitleMenu(memory);
    bool switched = false;
    try
    {
        RecompOne.Runtime.Sdk.GT2Compat.CommitUnifiedTitleSelection(
            memory, 1u, 0x80010000u);
    }
    catch (RecompOne.Runtime.Sdk.GT2VariantSwitch requested)
    {
        switched = requested.Variant == "arcade";
    }
    Require(
        switched,
        "Arcade title handoff was not immediate and can race Simulation overlay 4");

    RecompOne.Runtime.Sdk.GT2Compat.SetUnifiedArcadeTransition(true);
    // The complete loaded save follows the unified handoff through the BSS
    // reset. Test garage/profile data, mixed locked/unlocked results, full
    // license records, and one-shot restoration.
    memory.WriteU32(0x801D1568u, 100000u);
    memory.WriteU8(0x801CD554u, 67);
    memory.WriteU8(0x801D156Cu, 3);
    memory.WriteU8(0x801C9998u, 4);
    memory.WriteU8(0x801C9999u, 0);
    memory.WriteU8(0x801CACF9u, 4);
    memory.WriteU8(0x801CAD9Du, 0);
    memory.WriteU32(0x801CACFCu, 12345u);
    RecompOne.Runtime.Sdk.GT2Compat.PreserveUnifiedArcadeProgress(memory);
    var arcadeContext = new CpuContext { SP = 0x801FFF00u };
    RecompOne.Runtime.Sdk.GT2Compat.PrepareArcadeFrontendHandoff(arcadeContext, memory);
    Require(memory.ReadU8(0x801CA759u) == 0, "Arcade BSS was not reset");
    Require(
        !RecompOne.Runtime.Sdk.GT2Compat.ShouldPresentArcadeBootPanels(),
        "unified handoff still waits for duplicate timed Arcade boot panels");
    Require(
        RecompOne.Runtime.Sdk.GT2Compat.InitialArcadeOverlayIndex(memory) == 2u,
        "unified Arcade handoff did not skip the redundant disc title");
    Require(memory.ReadU32(0x801D0FC8u) == 100000u &&
        memory.ReadU8(0x801CCFB4u) == 67 && memory.ReadU8(0x801D0FCCu) == 3 &&
        memory.ReadU8(0x801C93F8u) == 4 && memory.ReadU8(0x801C93F9u) == 0 &&
        memory.ReadU8(0x801CA759u) == 4 && memory.ReadU8(0x801CA7FDu) == 0 &&
        memory.ReadU32(0x801CA75Cu) == 12345u,
        "loaded save did not survive the Arcade handoff exactly");
    memory.WriteU8(0x801C93F8u, 1);
    RecompOne.Runtime.Sdk.GT2Compat.InitialArcadeOverlayIndex(memory);
    Require(memory.ReadU8(0x801C93F8u) == 1, "stale handoff overwrote new Arcade progress");
    bool returnedToSimulation = false;
    try
    {
        RecompOne.Runtime.Sdk.GT2Compat.ReturnFromUnifiedArcade();
    }
    catch (RecompOne.Runtime.Sdk.GT2VariantSwitch requested)
    {
        returnedToSimulation = requested.Variant == "simulation";
    }
    Require(
        returnedToSimulation,
        "Arcade Mode Back did not return to the unified Simulation title");
    RecompOne.Runtime.Sdk.GT2Compat.SetUnifiedArcadeTransition(false);
    Require(
        RecompOne.Runtime.Sdk.GT2Compat.ShouldPresentArcadeBootPanels(),
        "standalone Arcade boot lost its original timed panels");
    Require(
        RecompOne.Runtime.Sdk.GT2Compat.InitialArcadeOverlayIndex(memory) == 5u,
        "standalone Arcade boot no longer retains its stock title path");
    RecompOne.Runtime.Sdk.GT2Compat.ReturnFromUnifiedArcade();
    var simulationContext = new CpuContext
    {
        A0 = 0x11111111u,
        A1 = 0x22222222u,
        SP = 0x801FFF00u,
        RA = 0x33333333u,
    };
    memory.WriteU32(0x801C93B0u, 0xAAAAAAAAu);
    memory.WriteU32(0x801F0D5Cu, 0xBBBBBBBBu);
    RecompOne.Runtime.Sdk.GT2Compat.PrepareSimulationTitleHandoff(
        simulationContext, memory);
    Require(
        memory.ReadU32(0x801C93B0u) == 0u &&
        memory.ReadU32(0x801F0D5Cu) == 0u &&
        memory.ReadU32(0x8009113Cu) == 0x11111111u &&
        memory.ReadU32(0x80091140u) == 0x22222222u &&
        simulationContext.SP == 0x801FFEE8u,
        "Simulation title reverse handoff does not match SCUS-94488 BSS/ABI");
    RecompOne.Runtime.Sdk.GT2Compat.InstallUnifiedTitleMenu(memory);
    return memory;
}

PSMemory testMemory = VerifyArcadeFrontendContracts();

// A prior looping music voice must not hold the newly keyed confirmation
// effect open, and an active effect must not be reported as completed.
{
    var spu = new Spu();
    const uint registers = 0x1F801C00u;
    spu.Ram[1] = 3; // loop/end, used by the pre-existing music voice
    spu.Ram[17] = 1; // non-looping end block for the confirmation voice
    spu.WriteReg16(registers + 4, 0x1000);
    spu.WriteReg16(registers + 0x188, 1);
    uint beforeEffect = spu.LatestKeyOnSerial;
    spu.WriteReg16(registers + 0x14, 0x1000);
    spu.WriteReg16(registers + 0x16, 2); // address units are eight bytes
    spu.WriteReg16(registers + 0x188, 2);
    uint effectMask = spu.CaptureVoiceMaskKeyedAfter(beforeEffect);
    Require(effectMask == 2, "confirmation voice mask includes older music");
    Require(!spu.WaitForVoicesToStop(effectMask, beforeEffect, 0),
        "active confirmation voice was reported as stopped");
    spu.Mix(new short[256], 128);
    Require(spu.WaitForVoicesToStop(effectMask, beforeEffect, 0),
        "finished confirmation voice did not release transition");
    Require(!spu.WaitForVoicesToStop(1, 0, 0),
        "test music voice must remain active independently of confirmation");
}

VerifyCpuProjectionFastPath();

static void VerifyWideCourseTranslation()
{
    short[][] matrices = [
        [4096, 0, 0, 0, 4096, 0, 0, 0, 4096],
        [63, 4095, 49, 10, 42, -3723, 4095, -63, 13]];
    int[][] vectors = [
        [0, 0, 0], [32767, -32768, 17], [-32768, 32767, -32768],
        [65016, -3590, -63], [-65016, 3590, 63],
        [32768, -32769, 65536], [2097151, -2097152, 4192]];
    foreach (short[] matrix in matrices)
    {
        for (int register = 0; register < 5; register++)
        {
            int index = register * 2;
            uint packed = (ushort)matrix[index];
            if (index + 1 < 9) packed |= (uint)(ushort)matrix[index + 1] << 16;
            Gte.WriteControl(register, packed);
        }
        foreach (int[] vector in vectors)
        {
            Gte.Write(9, unchecked((uint)vector[0]));
            Gte.Write(10, unchecked((uint)vector[1]));
            Gte.Write(11, unchecked((uint)vector[2]));
            Gte.Execute(0x4A49E012u);
            uint[] legacy = [Gte.Read(25), Gte.Read(26), Gte.Read(27)];
            uint legacyFlags = Gte.ReadControl(31);
            Gte.ExecuteTrackTranslation(unchecked((uint)vector[0]),
                unchecked((uint)vector[1]), unchecked((uint)vector[2]));
            for (int axis = 0; axis < 3; axis++)
            {
                long sum = 0;
                for (int column = 0; column < 3; column++)
                    sum += (long)matrix[axis * 3 + column] * vector[column];
                int expected = checked((int)(sum >> 12));
                Require(unchecked((int)Gte.Read(25 + axis)) == expected,
                    "wide course translation wrapped or changed fixed-point rounding");
                Require(unchecked((int)Gte.Read(9 + axis)) ==
                    Math.Clamp(expected, short.MinValue, short.MaxValue),
                    "wide course translation changed result-register saturation");
            }
            if (vector.All(value => value is >= short.MinValue and <= short.MaxValue))
            {
                Require(legacy.SequenceEqual(new[] {Gte.Read(25), Gte.Read(26), Gte.Read(27)}) &&
                    legacyFlags == Gte.ReadControl(31),
                    "in-range course translation differs from the original GTE path");
            }
            if (vector[0] == 65016)
                Require(!legacy.SequenceEqual(new[] {Gte.Read(25), Gte.Read(26), Gte.Read(27)}),
                    "measured Test Course negative control failed to reproduce 16-bit wrap");
        }
    }
}

VerifyWideCourseTranslation();

static void VerifyProjectionOriginHandleFlow(PSMemory memory)
{
    const uint directAddress = 0x00001000u;
    const uint cpuAddress = directAddress + 4;
    const uint derivedAddress = directAddress + 8;
    const uint untrackedAddress = directAddress + 12;
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
            // RTPS with sf=1 keeps the identity-matrix fixture in the
            // ordinary on-screen range. Without the shift, both axes
            // saturate at +1023 and the packed Y value is invalid for the
            // PS1 GPU's signed 10-bit vertical coordinate range.
            Gte.Execute(0x00080001u);
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

        Project(12, 22, 32);
        uint derivedAnchor = Gte.Read(14);
        int anchorX = (short)derivedAnchor;
        int anchorY = (short)(derivedAnchor >> 16);
        const int offsetX = -3;
        const int offsetY = -2;
        uint derivedPacked =
            (uint)((ushort)(short)(anchorX + offsetX) |
                ((uint)(ushort)(short)(anchorY + offsetY) << 16));
        Gte.BeginDerivedScreenProjection(derivedAnchor);
        memory.WriteU16(
            derivedAddress,
            (ushort)(short)(anchorX + offsetX));
        memory.WriteU16(
            derivedAddress + 2u,
            (ushort)(short)(anchorY + offsetY));
        Gte.EndDerivedScreenProjection();
        GteProjectionOrigin derived = Resolve(
            derivedAddress, derivedPacked);

        memory.WriteU16(untrackedAddress, (ushort)(short)anchorX);
        memory.WriteU16(untrackedAddress + 2u, (ushort)(short)anchorY);
        Require(
            !Gte.TryGetPacketOrigin(
                untrackedAddress,
                anchorX,
                anchorY,
                out _),
            "derived projection scope leaked into later screen primitives");

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
            direct.Object.ModelPointer == modelPointer &&
            derived.ModelX == 12 && derived.ModelY == 22 &&
            derived.ModelZ == 32 &&
            derived.Object.Kind == WorldObjectKind.Track &&
            derived.Object.ModelPointer == modelPointer &&
            derived.ScreenOffsetX == offsetX &&
            derived.ScreenOffsetY == offsetY &&
            (derived.Flags & GteProjectionOriginFlags.ScreenOffsetAnchor) != 0,
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

VerifyProjectionOriginHandleFlow(testMemory);

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

static void VerifyFrontendVehicleOwnership()
{
    const uint carState = 0x800F0000u;
    const uint modelPointer = 0x80027B84u;
    WorldCaptureContext.LiveRenderingEnabled = true;
    try
    {
        WorldCaptureContext.BeginVehicle(carState, modelPointer);
        Require(
            WorldCaptureContext.Current.Kind == WorldObjectKind.Vehicle &&
            WorldCaptureContext.Current.SceneGeneration == 0,
            "frontend car preview inherited race-scene ownership");
        WorldCaptureContext.EndObject();

        WorldCaptureContext.BeginScenePass(WorldScenePass.Main);
        WorldCaptureContext.BeginVehicle(carState, modelPointer);
        Require(
            WorldCaptureContext.Current.SceneGeneration != 0,
            "race vehicle lost authored scene-generation ownership");
        WorldCaptureContext.EndObject();
        WorldCaptureContext.EndScenePass();

        WorldCaptureContext.BeginVehicle(carState, modelPointer);
        Require(
            WorldCaptureContext.Current.SceneGeneration == 0,
            "post-race frontend car preview retained stale scene ownership");
    }
    finally
    {
        WorldCaptureContext.EndObject();
        WorldCaptureContext.EndScenePass();
        WorldCaptureContext.LiveRenderingEnabled = false;
    }
}

VerifyFrontendVehicleOwnership();

static void VerifyCarPreviewCameraDistance()
{
    const uint selectorCamera = 0x800F04E0u;
    const uint entryDistance = 0x00094CCCu;
    const uint finalDistance = 0x000F4CCCu;
    Require(
        RecompOne.Runtime.Sdk.GT2Compat.LimitCarPreviewCameraDistance(
            selectorCamera, entryDistance) == entryDistance,
        "car preview entry distance was changed");
    Require(
        RecompOne.Runtime.Sdk.GT2Compat.LimitCarPreviewCameraDistance(
            selectorCamera, finalDistance + 0x8000u) == finalDistance,
        "car preview camera did not stop at its authored distance");
    Require(
        RecompOne.Runtime.Sdk.GT2Compat.LimitCarPreviewCameraDistance(
            selectorCamera + 4u, finalDistance + 0x8000u) ==
                finalDistance + 0x8000u,
        "car preview ceiling leaked into another GT2 camera");
}

VerifyCarPreviewCameraDistance();

static void VerifyTrue60ReplaySegmentReset(PSMemory memory)
{
    const uint configuration = 0x80010000u;
    const uint carArray = 0x80020000u;
    const uint velocityX = carArray + 0x688u;
    memory.WriteU8(configuration + 0x8u, 2);

    RecompOne.Runtime.Sdk.GT2Compat.ConfigureTrue60HzRaceTimeStep(
        configuration, memory);
    RecompOne.Runtime.Sdk.GT2Compat.BeginTrue60HzLinearVelocityStep(
        carArray, 1, memory);
    memory.WriteU32(velocityX, 1);
    RecompOne.Runtime.Sdk.GT2Compat.EndTrue60HzLinearVelocityStep(
        carArray, 1, memory);
    Require(
        memory.ReadU32(velocityX) == 0,
        "first 60 Hz half-step did not retain its exact division carry");

    RecompOne.Runtime.Sdk.GT2Compat.BeginTrue60HzLinearVelocityStep(
        carArray, 1, memory);
    memory.WriteU32(velocityX, 1);
    RecompOne.Runtime.Sdk.GT2Compat.EndTrue60HzLinearVelocityStep(
        carArray, 1, memory);
    Require(
        memory.ReadU32(velocityX) == 1,
        "paired 60 Hz half-steps did not conserve the authored delta");

    memory.WriteU32(velocityX, 0);
    RecompOne.Runtime.Sdk.GT2Compat.BeginTrue60HzLinearVelocityStep(
        carArray, 1, memory);
    memory.WriteU32(velocityX, 1);
    RecompOne.Runtime.Sdk.GT2Compat.EndTrue60HzLinearVelocityStep(
        carArray, 1, memory);
    Require(
        memory.ReadU32(velocityX) == 0,
        "test did not establish a pending race-end division carry");

    // A replay commonly reuses the identical configuration and car-array
    // addresses. Its first half-step must nevertheless begin with clean host
    // state rather than consuming the final carry from the live race.
    RecompOne.Runtime.Sdk.GT2Compat.ConfigureTrue60HzRaceTimeStep(
        configuration, memory);
    RecompOne.Runtime.Sdk.GT2Compat.BeginTrue60HzLinearVelocityStep(
        carArray, 1, memory);
    memory.WriteU32(velocityX, 1);
    RecompOne.Runtime.Sdk.GT2Compat.EndTrue60HzLinearVelocityStep(
        carArray, 1, memory);
    Require(
        memory.ReadU32(velocityX) == 0,
        "replay inherited the live race's 60 Hz division carry");
}

VerifyTrue60ReplaySegmentReset(testMemory);

string unifiedHostProject = ReadRepoFile(
    @"tools\unified-host\GranTurismo2PC.csproj");
string unifiedHostProgram = ReadRepoFile(
    @"tools\unified-host\Program.cs");
string unifiedHostEntry = ReadRepoFile(
    @"tools\unified-host\UnifiedEntry.cs");
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
    inputManager.Contains(
        "RECOMPONE_CAPTURE_INPUT_STAGE_INTERVAL_POLLS",
        StringComparison.Ordinal) &&
    inputManager.Contains(
        "RECOMPONE_CAPTURE_INPUT_STAGE_END_POLL",
        StringComparison.Ordinal) &&
    inputManager.Contains(
        "AdvanceStageCaptureSchedule(",
        StringComparison.Ordinal),
    "stage-relative three-second track capture scheduling is missing");
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
string hostWindowSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Host\Window\HostWindow.cs");
string liveRendererSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Gpu\Hle\LiveWorldRenderer.cs");
string rawBackgroundSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Gpu\GpuRawBackground.cs");
Require(rawBackgroundSource.Contains(
        "vertexPointer + sourceIndices[sourceCorner] * 8u", StringComparison.Ordinal) &&
    Occurrences(rawBackgroundSource, "packetSourceIdentities[1],") == 2 &&
    Occurrences(rawBackgroundSource, "packetSourceIdentities[2]") == 2 &&
    liveRendererSource.Contains(
        "if (residentTrack || (sourceA != 0 && sourceB != 0 && sourceC != 0))",
        StringComparison.Ordinal),
    "authored background source identities are dropped before native serialization");
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
foreach (string coursePatch in new[] {simulationEnhancements, arcadeEnhancements})
    Require(Occurrences(coursePatch,
        "Gte.ExecuteTrackTranslation(c.A1, c.V1, c.A2);") == 1,
        "course translation widening must target exactly one transform per game");
foreach (string courseOverlay in new[] {
    @"generated\arcade-recompiled\gt2_arcade_overlay_0.cs",
    @"generated\recompiled\gt2_overlay_0.cs"})
    Require(Occurrences(ReadRepoFile(courseOverlay),
        "Gte.ExecuteTrackTranslation(c.A1, c.V1, c.A2);") == 1,
        "generated course translation hook is absent or duplicated");
string generatedArcadeFrontend = ReadRepoFile(
    @"generated\arcade-recompiled\gt2_arcade_overlay_2.cs");
string generatedSimulationTitle = ReadRepoFile(
    @"generated\recompiled\gt2_overlay_1.cs");
string unifiedModeHarness = ReadRepoFile(
    @"tools\test_unified_modes.ps1");
string hostAudioSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Host\Audio.cs");
string spuSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Hardware\Spu.cs");
string presentationRendererSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Host\Window\PresentationRenderer.cs");
string d3dRendererSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Host\Window\D3D11Renderer.cs");
string d3dImGuiSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Host\Window\D3D11ImGuiController.cs");
string d3dCompositorSource = ReadRepoFile(
    @"vendor\RecompOne\RecompOne.Runtime\Gpu\Hle\D3D11GpuBackend.cs");
string ssr11SelectorFixture = ReadRepoFile(
    @"tests\fixtures\unified-arcade-ssr11-selector.input");
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
Require(
    runtimeProject.Contains("Vortice.Direct3D11", StringComparison.Ordinal) &&
    runtimeProject.Contains("Vortice.DXGI", StringComparison.Ordinal) &&
    runtimeProject.Contains("ImGui.NET", StringComparison.Ordinal) &&
    !runtimeProject.Contains("Silk.NET.OpenGL", StringComparison.Ordinal) &&
    hostWindowSource.Contains("GraphicsAPI.None", StringComparison.Ordinal) &&
    hostWindowSource.Contains("new D3D11Renderer", StringComparison.Ordinal) &&
    hostWindowSource.Contains("new Hle.D3D11GpuBackend", StringComparison.Ordinal) &&
    hostWindowSource.Contains("new D3D11ImGuiController", StringComparison.Ordinal) &&
    d3dRendererSource.Contains("CreateSwapChainForHwnd", StringComparison.Ordinal) &&
    d3dImGuiSource.Contains("ImGui.GetDrawData()", StringComparison.Ordinal) &&
    d3dCompositorSource.Contains("WritebackFeedbackRegion", StringComparison.Ordinal) &&
    d3dCompositorSource.Contains(
        "ReferenceEquals(_batchTarget, old) && _vertexCount > 0",
        StringComparison.Ordinal) &&
    d3dCompositorSource.Contains("BlendOperation.ReverseSubtract", StringComparison.Ordinal) &&
    presentationRendererSource.Contains("api=D3D11", StringComparison.Ordinal) &&
    ssr11SelectorFixture.Contains("10000+1=CAPTURE", StringComparison.Ordinal) &&
    !RepoFileExists(@"vendor\RecompOne\RecompOne.Runtime\Gpu\Hle\Gl\GlBackend.cs") &&
    !RepoFileExists(@"vendor\RecompOne\RecompOne.Runtime\Gpu\Hle\Gl\GlVram.cs") &&
    !RepoFileExists(@"vendor\RecompOne\RecompOne.Runtime\Gpu\Hle\Gl\GlDisplayRt.cs") &&
    !RepoFileExists(@"vendor\RecompOne\RecompOne.Runtime\Gpu\Hle\Gl\GlShaders.cs"),
    "Windows shipping presentation is no longer wholly D3D11/DXGI");
Require(
    arcadeEnhancements.Contains(
        "ResolveArcadeCourseTable(", StringComparison.Ordinal) &&
    Occurrences(
        generatedArcadeFrontend,
        "GT2Compat.ResolveArcadeCourseTable(") == 10 &&
    !generatedArcadeFrontend.Contains(
        "GT2Compat.UnlockArcadeCourseTable(", StringComparison.Ordinal),
    "Arcade frontend can still read absent expanded course tables in a stock release");
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
    gt2CompatSource.Contains(
        "segment={segment} physicsCarry=reset",
        StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "public static void ObserveReplayControllerFrame(",
        StringComparison.Ordinal) &&
    Occurrences(
        simulationEnhancements,
        "ObserveReplayControllerFrame(") == 2 &&
    Occurrences(
        arcadeEnhancements,
        "ObserveReplayControllerFrame(") == 2 &&
    Occurrences(
        ReadRepoFile(@"generated\recompiled\gt2_overlay_0.cs"),
        "ObserveReplayControllerFrame(") == 2 &&
    Occurrences(
        ReadRepoFile(
            @"generated\arcade-recompiled\gt2_arcade_overlay_0.cs"),
        "ObserveReplayControllerFrame(") == 2 &&
    unifiedHostProgram.Contains(
        "--verify-replay-codec", StringComparison.Ordinal) &&
    unifiedHostProgram.Contains(
        "func_800166CC", StringComparison.Ordinal) &&
    unifiedHostProgram.Contains(
        "func_80016658", StringComparison.Ordinal) &&
    unifiedHostProgram.Contains(
        "source=original-guest-codec", StringComparison.Ordinal) &&
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
    gt2CompatSource.Contains("0x800B0F20u", StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "_unifiedArcadeFrontendPending", StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "_unifiedSimulationFrontendPending", StringComparison.Ordinal) &&
    gt2CompatSource.Contains("coverGuestHandoff", StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "UnifiedArcadeTransitionCoverActive", StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "return _unifiedArcadeTransition ? 2u : 5u;",
        StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "public static void ReturnFromUnifiedArcade()",
        StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "public static void ReturnFromUnifiedGranTurismoRoot(",
        StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "CompleteUnifiedTitleConfirmationAudio", StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "BeginUnifiedTitleConfirmationAudio", StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "WaitForVoicesToStop", StringComparison.Ordinal) &&
    spuSource.Contains(
        "CaptureVoiceMaskKeyedAfter", StringComparison.Ordinal) &&
    generatedSimulationTitle.Contains(
        "BeginUnifiedTitleConfirmationAudio(",
        StringComparison.Ordinal) &&
    hostAudioSource.Contains("WaitForMixAdvance(", StringComparison.Ordinal) &&
    arcadeEnhancements.Contains(
        "ShouldPresentArcadeBootPanels", StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "if (!_unifiedArcadeTransition)", StringComparison.Ordinal) &&
    hostWindowSource.Contains(
        "Sdk.GT2Compat.UnifiedArcadeTransitionCoverActive",
        StringComparison.Ordinal) &&
    hostWindowSource.Contains(
        "0, 0, coverWidth, coverHeight, false",
        StringComparison.Ordinal) &&
    arcadeEnhancements.Contains(
        "BeginUnifiedArcadeFrontendFrame", StringComparison.Ordinal) &&
    arcadeEnhancements.Contains(
        "CompleteUnifiedArcadeFrontendFrame", StringComparison.Ordinal) &&
    arcadeEnhancements.Contains(
        "ReturnFromUnifiedArcade", StringComparison.Ordinal) &&
    simulationEnhancements.Contains(
        "ReturnFromUnifiedGranTurismoRoot", StringComparison.Ordinal) &&
    arcadeEnhancements.Contains("func_80011750", StringComparison.Ordinal) &&
    generatedArcadeFrontend.Contains(
        "GT2Compat.CompleteUnifiedArcadeFrontendFrame();",
        StringComparison.Ordinal) &&
    generatedArcadeFrontend.Contains(
        "GT2Compat.ReturnFromUnifiedArcade();",
        StringComparison.Ordinal) &&
    unifiedHostEntry.Contains(
        "PrepareSimulationTitleHandoff", StringComparison.Ordinal) &&
    unifiedHostEntry.Contains(
        "RunSimulationTitleHandoff", StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "CompleteUnifiedTitleMenuInitialization", StringComparison.Ordinal) &&
    gt2CompatSource.Contains(
        "SignalScriptStage(\"arcade_frontend\")",
        StringComparison.Ordinal) &&
    generatedSimulationTitle.Contains(
        "CompleteUnifiedTitleMenuInitialization(m);",
        StringComparison.Ordinal) &&
    generatedSimulationTitle.Contains(
        "BufferUnifiedTitleInput(c.S3, m);",
        StringComparison.Ordinal) &&
    unifiedHostEntry.Contains(
        "CompleteUnifiedTitleConfirmationAudio", StringComparison.Ordinal) &&
    unifiedHostEntry.Contains("simulation-title", StringComparison.Ordinal) &&
    unifiedModeHarness.Contains("ExitPoll = 3200", StringComparison.Ordinal) &&
    unifiedModeHarness.Contains(
        "Arcade Mode Back did not return to the unified title",
        StringComparison.Ordinal) &&
    unifiedModeHarness.Contains(
        "Gran Turismo Mode Back did not return to the unified title",
        StringComparison.Ordinal),
    "unified Arcade/Gran Turismo round trips no longer bypass both disc titles");
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
    Regex.IsMatch(rawTrackSource,
        @"if\s*\(key\.AuxiliaryFormat\)\s+flags\s*\|=\s*1u\s*<<\s*7;"),
    "resident mesh serialization lost the object-local coordinate flag");
Require(
    rawTrackSource.Contains("WriteUInt32LittleEndian(destination[8..], 2)", StringComparison.Ordinal) &&
    liveBridgeSource.Contains("resident_mesh_version = 2", StringComparison.Ordinal) &&
    liveBridgeSource.Contains("resident_primitive_local_coordinates = 1U << 7", StringComparison.Ordinal),
    "managed/native resident mesh coordinate-space contract is inconsistent");
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
        "PSMainCutoutDepth(", StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "PSMainCutoutFringe(", StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "alpha_tested_cutout_coverage && pass == 1",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "road_overlay_depth_state(",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "description.DepthEnable = TRUE",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "description.DepthFunc = D3D11_COMPARISON_GREATER_EQUAL",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "const bool use_depth =",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "const bool road_support =",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "const bool track_overlay =",
        StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "depth_state = base.road_overlay_depth_states",
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
foreach (string trackOverlay in new[]
{
    @"generated\recompiled\gt2_overlay_0.cs",
    @"generated\arcade-recompiled\gt2_arcade_overlay_0.cs",
})
{
    string source = ReadRepoFile(trackOverlay);
    Require(
        Regex.IsMatch(source,
            @"ExpandTrackFrustumClassification\(\s*c\.V0, c\.V1, MemoryAccess\.ReadU32\(m, c\.S1 \+ 0x4u\)\)"),
        $"{trackOverlay}: track bridge lost the original bounding-box clip mask");
}
Require(
    RecompOne.Runtime.Sdk.GT2Compat.ApplyModernTrackFrustumClassification(
        0u, 0u, enabled: true) == 0u &&
    RecompOne.Runtime.Sdk.GT2Compat.ApplyModernTrackFrustumClassification(
        1u, 0x003F0000u, enabled: true) == 1u &&
    RecompOne.Runtime.Sdk.GT2Compat.ApplyModernTrackFrustumClassification(
        2u, 0x003F0006u, enabled: true) == 1u &&
    RecompOne.Runtime.Sdk.GT2Compat.ApplyModernTrackFrustumClassification(
        2u, 0x003F0006u, enabled: false) == 2u,
    "expanded track objects did not retain GT2's intersecting packet order");
foreach (uint mask in new uint[] { 0x003F003Fu, 0x00010001u, 0x003F0022u, 0u })
    Require(
        RecompOne.Runtime.Sdk.GT2Compat.ApplyModernTrackFrustumClassification(
            2u, mask, enabled: true) == 2u,
        $"whole-object depth/projection rejection was erased: 0x{mask:X8}");
foreach (uint mask in new uint[] { 2u, 4u, 8u, 16u, 0x003F0018u })
    Require(
        RecompOne.Runtime.Sdk.GT2Compat.ApplyModernTrackFrustumClassification(
            2u, mask, enabled: true) == 1u,
        $"authored viewport rejection still removes modern geometry: 0x{mask:X8}");
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
    null,
    [
        trialMountainClassCPre,
        false,
        "Trial Mountain Circuit",
        "Trial Mountain Circuit",
        0xAFD7E5BBu,
    ]);
replaceDirectArcadeCourseIdentity.Invoke(
    null,
    [
        trialMountainClassCFinal,
        false,
        "Trial Mountain Circuit",
        "Trial Mountain Circuit",
        0xAFD7E5BBu,
    ]);
replaceDirectArcadeCourseIdentity.Invoke(
    null,
    [
        trialMountainClassCState,
        true,
        "Trial Mountain Circuit",
        "Trial Mountain Circuit",
        0xAFD7E5BBu,
    ]);
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
var supportedDirectArcadeCourses =
    (IReadOnlyList<string>)gt2CompatType.GetProperty(
        "SupportedDirectArcadeCourses",
        BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
Require(
    supportedDirectArcadeCourses.Count == 54 &&
    supportedDirectArcadeCourses.Distinct(
        StringComparer.OrdinalIgnoreCase).Count() == 54 &&
    supportedDirectArcadeCourses.Contains(
        "high-speed-ring", StringComparer.OrdinalIgnoreCase) &&
    supportedDirectArcadeCourses.Contains(
        "green-forest-roadway", StringComparer.OrdinalIgnoreCase) &&
    supportedDirectArcadeCourses.Contains(
        "grand-valley-speedway-reverse", StringComparer.OrdinalIgnoreCase) &&
    supportedDirectArcadeCourses[^2] == "special-stage-route-11" &&
    supportedDirectArcadeCourses[^1] == "special-stage-route-11-reverse",
    "direct Arcade inventory is incomplete or no longer keeps Route 11 last");
MethodInfo buildDirectArcadeTemplate = gt2CompatType.GetMethod(
    "BuildDirectArcadeTemplate",
    BindingFlags.NonPublic | BindingFlags.Static)!;
string? originalDirectArcadeRace = Environment.GetEnvironmentVariable(
    "RECOMPONE_GT2_DIRECT_ARCADE_RACE");
try
{
    Environment.SetEnvironmentVariable(
        "RECOMPONE_GT2_DIRECT_ARCADE_RACE",
        "green-forest-roadway");
    byte[] greenForestState = (byte[])buildDirectArcadeTemplate.Invoke(
        null,
        [ssr5ClassCStateBase64, ssr5ClassCStateBase64, true])!;
    string greenForestStateName = System.Text.Encoding.ASCII.GetString(
        greenForestState,
        0x20,
        Array.IndexOf(greenForestState, (byte)0, 0x20) - 0x20);
    Require(
        greenForestStateName == "Green Forest Roadway" &&
        BitConverter.ToUInt32(greenForestState, 0x40) == 0x6B3F15FCu,
        "direct dirt-course launch lost its native course identity");

    Environment.SetEnvironmentVariable(
        "RECOMPONE_GT2_DIRECT_ARCADE_RACE",
        "grand-valley-speedway-reverse");
    byte[] grandValleyReverseState =
        (byte[])buildDirectArcadeTemplate.Invoke(
            null,
            [ssr5ClassCStateBase64, ssr5ClassCStateBase64, true])!;
    string grandValleyReverseStateName =
        System.Text.Encoding.ASCII.GetString(
            grandValleyReverseState,
            0x20,
            Array.IndexOf(
                grandValleyReverseState,
                (byte)0,
                0x20) - 0x20);
    Require(
        grandValleyReverseStateName == "Grand Valley Speedway" &&
        BitConverter.ToUInt32(grandValleyReverseState, 0x40) == 0xED4AED05u,
        "direct reverse-course launch lost its native course identity");
}
finally
{
    Environment.SetEnvironmentVariable(
        "RECOMPONE_GT2_DIRECT_ARCADE_RACE",
        originalDirectArcadeRace);
}
byte[] highSpeedRingClassCState = (byte[])ssr5ClassCState.Clone();
replaceDirectArcadeCourseIdentity.Invoke(
    null,
    [
        highSpeedRingClassCState,
        true,
        "High Speed Ring",
        "High Speed Ring",
        0x35B88252u,
    ]);
string highSpeedRingStateName = System.Text.Encoding.ASCII.GetString(
    highSpeedRingClassCState,
    0x20,
    Array.IndexOf(
        highSpeedRingClassCState,
        (byte)0,
        0x20) - 0x20);
Require(
    highSpeedRingStateName == "High Speed Ring" &&
    BitConverter.ToUInt32(highSpeedRingClassCState, 0x40) == 0x35B88252u,
    "direct High Speed Ring launch lost its native course identity");
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
        Occurrences(source, "Gte.BeginDerivedScreenProjection(") == 3 &&
        Occurrences(source, "0x31525353u") == 2 &&
        Occurrences(source, "Gte.EndDerivedScreenProjection();") == 3 &&
        Occurrences(
            source,
            "Gte.BeginDerivedScreenProjection(c.V0);") == 1,
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
    outputBufferCount == 5 && publishedOutputCapacity == 5,
    "native output ring no longer covers the bounded capture work window");
Require(
    hostWindowSource.Contains(
        "deferCaptureToNativeWorld",
        StringComparison.Ordinal) &&
    hostWindowSource.Contains(
        "_gpu?.LiveWorldExpected == true ||",
        StringComparison.Ordinal) &&
    hostWindowSource.Contains(
        "_gpu?.LiveWorldRecentlySeen == true",
        StringComparison.Ordinal),
    "race-stage presentation capture can still consume a worldless ownership gap");
Require(
    liveRendererSource.Contains(
        "action=evict-unconsumed",
        StringComparison.Ordinal) &&
    !liveRendererSource.Contains(
        "static-scene generation queue overflowed",
        StringComparison.Ordinal),
    "deferred static-scene overflow still aborts instead of evicting stale work");
Require(
    liveRendererSource.Contains(
        "_published.Count >= PublishedOutputCapacity &&",
        StringComparison.Ordinal) &&
    liveRendererSource.Contains(
        "!_stopping)",
        StringComparison.Ordinal) &&
    liveRendererSource.Contains(
        "Monitor.Wait(_gate);",
        StringComparison.Ordinal) &&
    liveRendererSource.Contains(
        "Monitor.PulseAll(_gate);",
        StringComparison.Ordinal) &&
    !liveRendererSource.Contains(
        "discarded = _published.Dequeue();",
        StringComparison.Ordinal),
    "full native presentation reserve still evicts a completed authored frame");

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
MethodInfo billboardUsesAuthoredDepth = gpuType.GetMethod(
    "RawTrackBillboardUsesAuthoredDepth",
    BindingFlags.NonPublic | BindingFlags.Static)!;
const long billboardNearFixed = 16L * 4096L;
Require(
    (bool)billboardUsesAuthoredDepth.Invoke(
        null,
        [3390, billboardNearFixed, billboardNearFixed + 1,
            billboardNearFixed + 2, billboardNearFixed + 3])! &&
    !(bool)billboardUsesAuthoredDepth.Invoke(
        null,
        [3390, billboardNearFixed, billboardNearFixed - 1,
            billboardNearFixed + 2, billboardNearFixed + 3])! &&
    !(bool)billboardUsesAuthoredDepth.Invoke(
        null,
        [0, billboardNearFixed, billboardNearFixed + 1,
            billboardNearFixed + 2, billboardNearFixed + 3])!,
    "track billboard flat depth no longer preserves near-plane clipping");
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
MethodInfo liveTriangleCaptureDecision = gpuType.GetMethod(
    "ClassifyLiveTriangleCapture",
    BindingFlags.NonPublic | BindingFlags.Static)!;
string liveWorldTriangle = liveTriangleCaptureDecision.Invoke(
    null,
    [true, true, true, false, false])!.ToString()!;
string provenanceFreeScreenTriangle = liveTriangleCaptureDecision.Invoke(
    null,
    [true, false, false, false, false])!.ToString()!;
string unselectedWorldTriangle = liveTriangleCaptureDecision.Invoke(
    null,
    [true, true, false, false, false])!.ToString()!;
string replacedTrackTriangle = liveTriangleCaptureDecision.Invoke(
    null,
    [true, true, true, true, false])!.ToString()!;
Require(
    liveWorldTriangle == "World" &&
    provenanceFreeScreenTriangle == "Screen" &&
    unselectedWorldTriangle == "None" &&
    replacedTrackTriangle == "None",
    "live capture no longer distinguishes provenance-free screen polygons " +
    "from native and unselected world geometry");
Require(
    nativeRendererSource.Contains(
        "PSMainScreenGridMask", StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "screen_grid_world_texture", StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "detect_authored_screen_arcs", StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "ScreenArcData", StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "policy=authored-radial-fan", StringComparison.Ordinal) &&
    nativeRendererSource.Contains(
        "resolved_color_texture = base.screen_grid_texture.Get()",
        StringComparison.Ordinal) &&
    !nativeRendererSource.Contains(
        "rounded_pause", StringComparison.OrdinalIgnoreCase),
    "authored screen primitives no longer resolve through the generic " +
    "PS1 coverage grid");
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
MethodInfo correctVehicleNclipDepthSign = gteType.GetMethod(
    "CorrectVehicleNclipDepthSign",
    BindingFlags.NonPublic | BindingFlags.Static)!;
// A=(-2,-1,2), B=(2,-1,2), C=(0,2,-1). Perspective projection
// gives signed area -3, but clipping at z=1 gives the CCW trapezoid
// (-1,-.5), (1,-.5), (4/3,0), (-4/3,0), signed double-area 7/3.
Require(
    (double)correctVehicleNclipDepthSign.Invoke(null, [-3.0, 2L, 2L, -1L])! == 3.0 &&
    (double)correctVehicleNclipDepthSign.Invoke(null, [3.0, 2L, -1L, 2L])! == -3.0,
    "vehicle facing flips across the camera plane instead of matching the clipped polygon");
Require(
    (double)correctVehicleNclipDepthSign.Invoke(null, [0.125, 2L, 3L, 4L])! == 0.125 &&
    (double)correctVehicleNclipDepthSign.Invoke(null, [-17.0, 2L, 3L, 4L])! == -17.0 &&
    (double)correctVehicleNclipDepthSign.Invoke(null, [5.0, -2L, -3L, 4L])! == 5.0 &&
    (double)correctVehicleNclipDepthSign.Invoke(null, [0.0, 2L, 3L, -4L])! == 0.0,
    "vehicle facing correction changed front-facing area, even depth parity, or degeneracy");
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
    "direct presentation no longer retains its two-frame scheduling reserve");
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
    "screen_compositor_retained=pass screen_polygon_capture=pass " +
    "screen_native_grid=pass screen_analytic_arcs=pass " +
    "stale_world_transition_blocked=pass " +
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
    "projection_origin_handle_flow=pass wide_course_translation=pass " +
    "background_source_identity_flow=pass " +
    "continuous_track_facing=pass exact_face_oracle=pass " +
    "authored_material_lod=pass oriented_minification=pass");
