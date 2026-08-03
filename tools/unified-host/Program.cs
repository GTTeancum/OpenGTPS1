using RecompOne.Runtime.Config;
using RecompOne.Runtime.Memory;

string launchDirectory = Environment.CurrentDirectory;
Environment.CurrentDirectory = AppContext.BaseDirectory;

bool headless = args.Any(arg =>
    arg.Equals("--headless", StringComparison.OrdinalIgnoreCase));
bool mute = args.Any(arg =>
    arg.Equals("--mute", StringComparison.OrdinalIgnoreCase));

string[] positionalArgs = args.Where(arg =>
    !arg.Equals("--headless", StringComparison.OrdinalIgnoreCase) &&
    !arg.Equals("--mute", StringComparison.OrdinalIgnoreCase)).ToArray();
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
