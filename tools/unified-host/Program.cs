using RecompOne.Runtime.Config;
using RecompOne.Runtime.Memory;

string launchDirectory = Environment.CurrentDirectory;
Environment.CurrentDirectory = AppContext.BaseDirectory;

bool headless = false;
bool mute = false;
bool validateLiveries = false;
bool verifyReplayCodec = false;
bool verifyCarPreviewCamera = false;
bool startArcade = false;
string? directArcadeRace = null;
bool directArcadeReplay = false;
var positionalArguments = new List<string>();
for (int index = 0; index < args.Length; index++)
{
    string argument = args[index];
    if (argument.Equals("--headless", StringComparison.OrdinalIgnoreCase))
        headless = true;
    else if (argument.Equals("--mute", StringComparison.OrdinalIgnoreCase))
        mute = true;
    else if (argument.Equals(
                 "--validate-liveries",
                 StringComparison.OrdinalIgnoreCase))
        validateLiveries = true;
    else if (argument.Equals(
                 "--verify-replay-codec",
                 StringComparison.OrdinalIgnoreCase))
        verifyReplayCodec = true;
    else if (argument.Equals(
                 "--verify-car-preview-camera",
                 StringComparison.OrdinalIgnoreCase))
        verifyCarPreviewCamera = true;
    else if (argument.Equals(
                 "--start-arcade",
                 StringComparison.OrdinalIgnoreCase))
        startArcade = true;
    else if (argument.Equals(
                 "--arcade-race",
                 StringComparison.OrdinalIgnoreCase) ||
             argument.Equals(
                 "--arcade-replay",
                 StringComparison.OrdinalIgnoreCase))
    {
        if (directArcadeRace != null)
            return StartupFailure(
                "Set only one direct Arcade launch mode: " +
                "--arcade-race or --arcade-replay.",
                headless);
        bool replay = argument.Equals(
            "--arcade-replay",
            StringComparison.OrdinalIgnoreCase);
        if (++index >= args.Length)
            return StartupFailure(
                $"{argument} requires a course name; supported courses are " +
                string.Join(
                    ", ",
                    RecompOne.Runtime.Sdk.GT2Compat.
                        SupportedDirectArcadeCourses) + ".",
                headless);
        directArcadeRace = args[index].Trim().ToLowerInvariant();
        if (!RecompOne.Runtime.Sdk.GT2Compat.
                IsSupportedDirectArcadeCourse(directArcadeRace))
            return StartupFailure(
                $"Unsupported direct Arcade course: {args[index]}. " +
                "Supported courses are " +
                string.Join(
                    ", ",
                    RecompOne.Runtime.Sdk.GT2Compat.
                        SupportedDirectArcadeCourses) + ".",
                headless);
        directArcadeReplay = replay;
        startArcade = true;
    }
    else if (argument.StartsWith("--", StringComparison.Ordinal))
    {
        return StartupFailure(
            $"Unknown command-line option: {argument}",
            headless);
    }
    else
    {
        positionalArguments.Add(argument);
    }
}
string[] positionalArgs = positionalArguments.ToArray();

if (directArcadeRace != null)
{
    Environment.SetEnvironmentVariable(
        "RECOMPONE_GT2_DIRECT_ARCADE_RACE",
        directArcadeRace);
    Console.WriteLine(
        $"[Host] direct Arcade " +
        $"{(directArcadeReplay ? "natural replay" : "race")} requested: " +
        directArcadeRace);
}
if (directArcadeReplay)
{
    // Reach GT2's own replay controller without depending on menus, a memory
    // card, or timing-sensitive absolute input. Keep Arcade's native player
    // record so the race owns a result and can finish, but route that car
    // through GT2's original CPU driver dispatch. Stage-relative confirmations
    // then advance Results; no synthetic finish, replay state, or camera is
    // used.
    Environment.SetEnvironmentVariable("RECOMPONE_GT2_AI_AUTODRIVE", "1");
    Environment.SetEnvironmentVariable(
        "RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS", "2");
    Environment.SetEnvironmentVariable(
        "RECOMPONE_GT2_SOAK_QUICK_WIN_AFTER_AI_TICKS", null);
    Environment.SetEnvironmentVariable("RECOMPONE_DISABLE_LIVE_INPUT", "1");
    // Direct replay is a user-facing launch path, not an image-producing QA
    // fixture. Development gates opt into one labelled stage capture through
    // InputManager's bounded diagnostic controls.
    Environment.SetEnvironmentVariable(
        "RECOMPONE_CAPTURE_AUTOMATIC_STAGE", "0");
    Environment.SetEnvironmentVariable("RECOMPONE_INPUT_FILE", null);
    Environment.SetEnvironmentVariable(
        "RECOMPONE_INPUT_SCRIPT",
        BuildDirectReplayInputScript());
}
if (headless)
{
    Environment.SetEnvironmentVariable("RECOMPONE_WINDOW_VISIBLE", "0");
    Environment.SetEnvironmentVariable("RECOMPONE_MUTE", "1");
    Console.Error.WriteLine("[Host] --headless: window hidden and audio output muted");
}
else if (mute)
{
    Environment.SetEnvironmentVariable("RECOMPONE_MUTE", "1");
}

if (verifyReplayCodec)
{
    VerifyReplayCodecs();
    return 0;
}
if (verifyCarPreviewCamera)
{
    VerifyCarPreviewCameras();
    return 0;
}

ConfigManager.Load();
RecompOne.Runtime.Runtime.SetMode(RecompOne.Runtime.RunMode.Devkit);

string? looseRoot = positionalArgs.Length > 0
    ? Path.GetFullPath(positionalArgs[0], launchDirectory)
    : ResolveUnifiedGameRoot(AppContext.BaseDirectory, launchDirectory);
if (looseRoot is null)
{
    return StartupFailure(
        "Gran Turismo 2 game data was not found. Run " +
        "Setup-From-GT2-Discs.ps1 in a release package, or pass the " +
        "prepared unified game-data directory on the command line.",
        headless);
}
if (!Directory.Exists(looseRoot))
{
    return StartupFailure(
        $"Unified game directory is missing: {looseRoot}",
        headless);
}
if (positionalArgs.Length == 0 &&
    !Path.GetFullPath(AppContext.BaseDirectory).Equals(
        Path.GetFullPath(looseRoot),
        StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine($"[Host] auto-resolved unified game data: {looseRoot}");
}

foreach (string variant in new[] { "simulation", "arcade" })
{
    string manifestPath = Path.Combine(
        looseRoot, "manifests", $"{variant}.json");
    if (!File.Exists(manifestPath))
    {
        return StartupFailure(
            $"Unified {variant} manifest is missing: {manifestPath}",
            headless);
    }
}
foreach (string sharedFile in new[] {
    "GT2.VOL", "MUSIC.DAT", "TITLE_EXACT.DAT" })
{
    if (!File.Exists(Path.Combine(looseRoot, sharedFile)))
    {
        return StartupFailure(
            $"Unified shared game data is missing: " +
            Path.Combine(looseRoot, sharedFile),
            headless);
    }
}

Environment.SetEnvironmentVariable("RECOMPONE_LOOSE_DIR", looseRoot);
Environment.SetEnvironmentVariable(
    "RECOMPONE_LOOSE_MANIFEST",
    Path.Combine("manifests", "simulation.json"));

if (validateLiveries)
{
    ValidateLiveryResolver();
    return 0;
}

try
{
    PreloadBundledNative("glfw3.dll");
    PreloadBundledNative("cimgui.dll");
    PreloadBundledNative("SDL2.dll");
    if (startArcade || directArcadeRace != null)
        PrepareDirectArcadeRenderer();
    var memory = new PSMemory();
    UnifiedEntry.Run(
        memory,
        looseRoot,
        startArcade ? "arcade" : "simulation");
}
catch (Exception exception)
{
    // Keep all managed failures out of Windows' opaque 0xC0434352 dialog.
    // Interactive and headless runs both leave a durable diagnostic containing
    // the exception type, message, inner exceptions, and complete stack trace.
    string diagnostic = $"[Host] fatal {(headless ? "headless" : "interactive")} exception:" +
        $"{Environment.NewLine}{exception}";
    Console.Error.WriteLine(diagnostic);
    WriteCrashDiagnostic(diagnostic);
    return 1;
}
return 0;

static string? ResolveUnifiedGameRoot(
    string applicationDirectory,
    string launchDirectory)
{
    var candidates = new List<string>();
    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    void AddCandidate(string candidate)
    {
        string fullPath = Path.GetFullPath(candidate);
        if (seen.Add(fullPath))
            candidates.Add(fullPath);
    }

    // A release setup installs the prepared data beside the executable.
    AddCandidate(applicationDirectory);
    AddCandidate(launchDirectory);
    AddCandidate(Path.Combine(launchDirectory, "work", "gt2-unified"));

    // Developer self-contained publishes live several levels below the repo.
    // Walk ancestors instead of depending on one fixed bin/TFM/RID layout.
    for (DirectoryInfo? directory = new(applicationDirectory);
         directory is not null;
         directory = directory.Parent)
    {
        AddCandidate(Path.Combine(directory.FullName, "work", "gt2-unified"));
    }

    return candidates.FirstOrDefault(HasUnifiedGameData);
}

static string BuildDirectReplayInputScript()
{
    // Results timing is native and can vary with race pace. Sparse Cross
    // pulses begin late enough to preserve a useful race window and remain
    // harmless while GT2's AI is still driving. The AI hook signals replay_1
    // only when GT2 instantiates replay vehicles: the acceptance marker.
    var lines = new List<string> { "[race_1]" };
    for (int poll = 9000; poll <= 40000; poll += 1000)
        lines.Add($"{poll}+4=CROSS");
    lines.Add("[replay_1]");
    return string.Join(Environment.NewLine, lines);
}

static bool HasUnifiedGameData(string directory) =>
    Directory.Exists(directory) &&
    File.Exists(Path.Combine(directory, "manifests", "simulation.json")) &&
    File.Exists(Path.Combine(directory, "manifests", "arcade.json")) &&
    File.Exists(Path.Combine(directory, "GT2.VOL")) &&
    File.Exists(Path.Combine(directory, "MUSIC.DAT")) &&
    File.Exists(Path.Combine(directory, "TITLE_EXACT.DAT"));

static int StartupFailure(string diagnostic, bool headless)
{
    Console.Error.WriteLine(diagnostic);
    try
    {
        string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logDirectory);
        string report =
            $"UTC: {DateTimeOffset.UtcNow:O}{Environment.NewLine}" +
            $"Command line: {Environment.CommandLine}{Environment.NewLine}" +
            $"Startup error: {diagnostic}{Environment.NewLine}";
        File.WriteAllText(
            Path.Combine(logDirectory, "GranTurismo2PC-startup-latest.log"),
            report);
    }
    catch
    {
        // The primary diagnostic remains on stderr.
    }

    if (!headless && OperatingSystem.IsWindows())
    {
        try
        {
            StartupDialog.MessageBox(
                0,
                diagnostic,
                "Gran Turismo 2 PC - Startup Error",
                0x10u);
        }
        catch
        {
            // A dialog is supplemental; never mask the persistent diagnostic.
        }
    }
    return 1;
}

static void WriteCrashDiagnostic(string diagnostic)
{
    try
    {
        string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logDirectory);
        string timestamp = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss-fff");
        string report =
            $"UTC: {DateTimeOffset.UtcNow:O}{Environment.NewLine}" +
            $"Command line: {Environment.CommandLine}{Environment.NewLine}" +
            $"Runtime: {Environment.Version}{Environment.NewLine}" +
            $"OS: {Environment.OSVersion}{Environment.NewLine}" +
            $"{diagnostic}{Environment.NewLine}";
        File.WriteAllText(
            Path.Combine(logDirectory, $"GranTurismo2PC-crash-{timestamp}.log"),
            report);
        File.WriteAllText(
            Path.Combine(logDirectory, "GranTurismo2PC-crash-latest.log"),
            report);
    }
    catch
    {
        // The original exception is authoritative. Never replace it with a
        // secondary failure while attempting to persist diagnostics.
    }
}

static void PreloadBundledNative(string fileName)
{
    string? searchDirectories =
        AppContext.GetData("NATIVE_DLL_SEARCH_DIRECTORIES") as string;
    if (string.IsNullOrWhiteSpace(searchDirectories))
        return;

    foreach (string directory in searchDirectories.Split(
                 Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
    {
        string candidate = Path.Combine(directory, fileName);
        if (!File.Exists(candidate))
            continue;
        System.Runtime.InteropServices.NativeLibrary.Load(candidate);
        Console.WriteLine($"[Host] preloaded bundled native library: {fileName}");
        return;
    }
}

static void PrepareDirectArcadeRenderer()
{
    string[] guestMethodNames = LoadSeattleArcadeMethodProfile(
            "OpenGTPS1.SeattleArcadeHotMethods.txt")
        .Concat(LoadSeattleArcadeMethodProfile(
            "OpenGTPS1.SeattleArcadePreloadMethods.txt"))
        .Distinct(StringComparer.Ordinal)
        .ToArray();
    string[] memoryMethodNames =
    [
        "ReadU8",
        "ReadU16",
        "ReadU32",
        "WriteU8",
        "WriteU16",
        "WriteU32",
        "ReadWordLeft",
        "ReadWordRight",
        "WriteWordLeft",
        "WriteWordRight",
    ];
    var timer = System.Diagnostics.Stopwatch.StartNew();
    int prepared = PrepareMethods(
        typeof(Recompiled.Arcade.GranTurismo2ArcadePC),
        guestMethodNames);
    prepared += PrepareMethods(
        typeof(RecompOne.Runtime.Memory.MemoryAccess),
        memoryMethodNames);
    prepared += PrepareMethods(
        typeof(RecompOne.Runtime.Memory.PSMemory),
        memoryMethodNames);
    timer.Stop();
    Console.WriteLine(
        $"[Host] prepared direct Arcade runtime hot paths: " +
        $"methods={prepared} elapsedMs={timer.Elapsed.TotalMilliseconds:F3}");
}

static int PrepareMethods(Type type, IReadOnlyList<string> methodNames)
{
    foreach (string methodName in methodNames)
    {
        var method = type.GetMethod(
            methodName,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Static) ??
            type.GetMethod(
                methodName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance) ??
            throw new MissingMethodException(type.FullName, methodName);
        System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(
            method.MethodHandle);
    }
    return methodNames.Count;
}

static string[] LoadSeattleArcadeMethodProfile(string resourceName)
{
    var assembly = typeof(Recompiled.Arcade.GranTurismo2ArcadePC).Assembly;
    using Stream stream = assembly.GetManifestResourceStream(resourceName) ??
        throw new InvalidOperationException(
            $"Embedded Seattle optimization profile is missing: {resourceName}");
    using var reader = new StreamReader(stream);
    return reader.ReadToEnd()
        .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);
}

static void ValidateLiveryResolver()
{
    // Archive-derived Castrol Supra package map. The GT1 tsplr and t-plr
    // textures share one indexed bitmap, so all four accepted choices are
    // native palettes in the sole visible/persisted tsplr body.
    const uint target = 0x1E75A59Cu;
    (uint Body, uint Palette)[] expected =
    [
        (target, 0),
        (target, 1),
        (target, 2),
        (target, 3),
    ];
    for (uint palette = 0; palette < expected.Length; palette++)
    {
        ulong resolved =
            RecompOne.Runtime.Sdk.GT2Compat.ResolveLiveryBodyAndPalette(
                target, palette);
        var actual = ((uint)resolved, (uint)(resolved >> 32));
        if (actual != expected[palette])
            throw new InvalidDataException(
                "Castrol Supra livery resolver mismatch: " +
                $"palette={palette}, actual=0x{actual.Item1:X8}/{actual.Item2}, " +
                $"expected=0x{expected[palette].Body:X8}/" +
                $"{expected[palette].Palette}");
    }

    // Both IDs are intentionally shared by more than one palette. ID-only
    // fallback must retain the sole body identity instead of guessing.
    foreach (uint colorId in new uint[] { 108, 113 })
    {
        uint resolved =
            RecompOne.Runtime.Sdk.GT2Compat.ResolveLiveryBodyForColorId(
                target, colorId);
        if (resolved != target)
            throw new InvalidDataException(
                "Ambiguous Castrol Supra color ID selected a body: " +
                $"id={colorId}, body=0x{resolved:X8}");
    }
    Console.WriteLine(
        "[Host] livery resolver validation passed: " +
        "Castrol Supra palettes=4, duplicate IDs=108/113");
}

static void VerifyReplayCodecs()
{
    var memory = new PSMemory();
    VerifyReplayCodec(
        "Simulation",
        Recompiled.Simulation.GranTurismo2PC.func_800163B8,
        Recompiled.Simulation.GranTurismo2PC.func_800166CC,
        Recompiled.Simulation.GranTurismo2PC.func_800167D0,
        Recompiled.Simulation.GranTurismo2PC.func_80016428,
        0x80010000u,
        memory);
    VerifyReplayCodec(
        "Arcade",
        Recompiled.Arcade.GranTurismo2ArcadePC.func_80016344,
        Recompiled.Arcade.GranTurismo2ArcadePC.func_80016658,
        Recompiled.Arcade.GranTurismo2ArcadePC.func_8001675C,
        Recompiled.Arcade.GranTurismo2ArcadePC.func_800163B4,
        0x80014000u,
        memory);
    Console.WriteLine(
        "[GT2-Replay-Codec] pass variants=Simulation,Arcade " +
        "source=original-guest-codec");
}

static void VerifyCarPreviewCameras()
{
    var memory = new PSMemory();
    VerifyCarPreviewCamera(
        "Simulation",
        Recompiled.Simulation.GranTurismo2PC.func_8001E5D8,
        memory);
    VerifyCarPreviewCamera(
        "Arcade",
        Recompiled.Arcade.GranTurismo2ArcadePC.func_8001E5BC,
        memory);
    Console.WriteLine(
        "[GT2-Car-Preview-Camera] pass variants=Simulation,Arcade " +
        "entry=0x00094CCC step=0x00008000 limit=0x000F4CCC");
}

static void VerifyCarPreviewCamera(
    string variant,
    Action<RecompOne.Runtime.Context.CpuContext, IMemory> update,
    PSMemory memory)
{
    const uint camera = 0x800F04E0u;
    const uint distanceOffset = 0xA8u;
    const uint entryDistance = 0x00094CCCu;
    const uint distanceStep = 0x00008000u;
    const uint distanceLimit = 0x000F4CCCu;
    const uint zoomTrigger = 0x0000FB91u;
    const int authoredSteps = 12;
    var context = new RecompOne.Runtime.Context.CpuContext();
    memory.WriteU32(camera + distanceOffset, entryDistance);

    for (int step = 1; step <= authoredSteps; step++)
    {
        context.A0 = camera;
        context.A2 = zoomTrigger;
        update(context, memory);
        uint expected = entryDistance + (uint)step * distanceStep;
        uint actual = memory.ReadU32(camera + distanceOffset);
        if (actual != expected)
            throw new InvalidDataException(
                $"{variant} car preview distance mismatch at step {step}: " +
                $"actual=0x{actual:X8} expected=0x{expected:X8}");
    }

    for (int stress = 0; stress < 256; stress++)
    {
        context.A0 = camera;
        context.A2 = zoomTrigger;
        update(context, memory);
    }
    uint heldDistance = memory.ReadU32(camera + distanceOffset);
    if (heldDistance != distanceLimit)
        throw new InvalidDataException(
            $"{variant} car preview did not hold its final distance: " +
            $"actual=0x{heldDistance:X8} expected=0x{distanceLimit:X8}");
}

static void VerifyReplayCodec(
    string variant,
    Action<RecompOne.Runtime.Context.CpuContext, IMemory> initialize,
    Action<RecompOne.Runtime.Context.CpuContext, IMemory> record,
    Action<RecompOne.Runtime.Context.CpuContext, IMemory> finalize,
    Action<RecompOne.Runtime.Context.CpuContext, IMemory> decode,
    uint buffer,
    PSMemory memory)
{
    const ushort capacity = 0x1800;
    uint input = buffer + 0x2000u;
    uint output = input + 0x20u;
    byte[][] samples =
    [
        [0x00, 0x00, 0x00, 0x00, 0x00],
        [0x00, 0x00, 0x00, 0x00, 0x00],
        [0x09, 0x12, 0x34, 0x05, 0x0A],
        [0x09, 0x12, 0x34, 0x05, 0x0A],
        [0x09, 0x12, 0x35, 0x05, 0x0A],
        [0x01, 0x7F, 0x35, 0x0F, 0x01],
        [0x01, 0x7F, 0x35, 0x0F, 0x01],
        [0x00, 0x80, 0x00, 0x00, 0x00],
    ];

    var context = new RecompOne.Runtime.Context.CpuContext
    {
        SP = 0x801FF000u,
        A0 = buffer,
        A1 = 0u,
        A2 = capacity,
    };
    initialize(context, memory);
    foreach (byte[] sample in samples)
    {
        for (int index = 0; index < sample.Length; index++)
            memory.WriteU8(input + (uint)index, sample[index]);
        context.A0 = buffer;
        context.A1 = input;
        record(context, memory);
    }
    context.A0 = buffer;
    context.A1 = 0u;
    finalize(context, memory);
    if (memory.ReadU32(buffer) != samples.Length ||
        memory.ReadU16(buffer + 0xCu) == 0)
        throw new InvalidDataException(
            $"{variant} replay encoder did not finalize the synthetic stream");

    context.A0 = buffer;
    context.A1 = 1u;
    context.A2 = capacity;
    initialize(context, memory);
    var decoded = new List<byte[]>();
    for (int guard = 0; guard <= samples.Length; guard++)
    {
        context.A0 = buffer;
        context.A1 = output;
        decode(context, memory);
        if (memory.ReadU16(buffer + 0xCu) != 0)
            break;
        var sample = new byte[5];
        for (int index = 0; index < sample.Length; index++)
            sample[index] = memory.ReadU8(output + (uint)index);
        decoded.Add(sample);
    }

    byte[][] expectedSamples = samples[..^1];
    if (decoded.Count != expectedSamples.Length ||
        !decoded.Zip(expectedSamples).All(
            pair => pair.First.SequenceEqual(pair.Second)))
    {
        string expected = string.Join(
            ' ', samples.Select(Convert.ToHexString));
        string actual = string.Join(
            ' ', decoded.Select(Convert.ToHexString));
        throw new InvalidDataException(
            $"{variant} replay codec round trip failed: " +
            $"expected={expected} actual={actual} " +
            $"encodedFrames={memory.ReadU32(buffer)}");
    }
    Console.WriteLine(
        $"[GT2-Replay-Codec] variant={variant} frames={decoded.Count} " +
        $"compressedBytes={memory.ReadU16(buffer + 0x10u)} roundTrip=exact");
}

internal static class StartupDialog
{
    [System.Runtime.InteropServices.DllImport(
        "user32.dll",
        EntryPoint = "MessageBoxW",
        CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    internal static extern int MessageBox(
        nint window,
        string text,
        string caption,
        uint type);
}
