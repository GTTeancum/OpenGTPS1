using RecompOne.Runtime.Config;
using RecompOne.Runtime.Memory;

string launchDirectory = Environment.CurrentDirectory;
Environment.CurrentDirectory = AppContext.BaseDirectory;

bool headless = args.Any(arg =>
    arg.Equals("--headless", StringComparison.OrdinalIgnoreCase));
bool mute = args.Any(arg =>
    arg.Equals("--mute", StringComparison.OrdinalIgnoreCase));
bool validateLiveries = args.Any(arg =>
    arg.Equals("--validate-liveries", StringComparison.OrdinalIgnoreCase));
bool startArcade = args.Any(arg =>
    arg.Equals("--start-arcade", StringComparison.OrdinalIgnoreCase));

string[] positionalArgs = args.Where(arg =>
    !arg.Equals("--headless", StringComparison.OrdinalIgnoreCase) &&
    !arg.Equals("--mute", StringComparison.OrdinalIgnoreCase) &&
    !arg.Equals(
        "--validate-liveries",
        StringComparison.OrdinalIgnoreCase) &&
    !arg.Equals(
        "--start-arcade",
        StringComparison.OrdinalIgnoreCase)).ToArray();
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

ConfigManager.Load();
RecompOne.Runtime.Runtime.SetMode(RecompOne.Runtime.RunMode.Devkit);

string? looseRoot = positionalArgs.Length > 0
    ? Path.GetFullPath(positionalArgs[0], launchDirectory)
    : ResolveUnifiedGameRoot(AppContext.BaseDirectory, launchDirectory);
if (looseRoot is null)
{
    return StartupFailure(
        "Gran Turismo 2 game data was not found. Run " +
        "Setup-From-Simulation-Disc.ps1 in a release package, or pass the " +
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
