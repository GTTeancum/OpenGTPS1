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

string[] positionalArgs = args.Where(arg =>
    !arg.Equals("--headless", StringComparison.OrdinalIgnoreCase) &&
    !arg.Equals("--mute", StringComparison.OrdinalIgnoreCase) &&
    !arg.Equals(
        "--validate-liveries",
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

string looseRoot = positionalArgs.Length > 0
    ? Path.GetFullPath(positionalArgs[0], launchDirectory)
    : AppContext.BaseDirectory;
if (!Directory.Exists(looseRoot))
{
    Console.Error.WriteLine($"Unified game directory is missing: {looseRoot}");
    return 1;
}

foreach (string variant in new[] { "simulation", "arcade" })
{
    string manifestPath = Path.Combine(
        looseRoot, "manifests", $"{variant}.json");
    if (!File.Exists(manifestPath))
    {
        Console.Error.WriteLine(
            $"Unified {variant} manifest is missing: {manifestPath}");
        return 1;
    }
}
foreach (string sharedFile in new[] { "GT2.VOL", "MUSIC.DAT" })
{
    if (!File.Exists(Path.Combine(looseRoot, sharedFile)))
    {
        Console.Error.WriteLine(
            $"Unified shared game data is missing: {Path.Combine(looseRoot, sharedFile)}");
        return 1;
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
    PreloadBundledNative("SDL2.dll");
    var memory = new PSMemory();
    UnifiedEntry.Run(memory, looseRoot);
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
    // Archive-derived Castrol Supra package map:
    // target tsplr palette 0 = native GT2 body;
    // palette 1 = GT1 tsplr body;
    // palettes 2/3 = GT1 t-plr body.
    const uint target = 0x1E75A59Cu;
    const uint gt1TsplrBody = 0x24041060u;
    const uint gt1ShortStemBody = 0x2405E696u;
    (uint Body, uint Palette)[] expected =
    [
        (target, 0),
        (gt1TsplrBody, 1),
        (gt1ShortStemBody, 0),
        (gt1ShortStemBody, 1),
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

    // Both IDs are intentionally shared by more than one body. ID-only
    // fallback must retain the retail identity instead of guessing.
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
