using RecompOne.Runtime.Config;
using RecompOne.Runtime.Memory;
using Recompiled.Simulation;

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
    // Session-only safety switch for visible automated diagnostics. This does
    // not change settings.json, so a later normal user launch remains audible.
    Environment.SetEnvironmentVariable("RECOMPONE_MUTE", "1");
    Console.Error.WriteLine(
        "[Host] --mute: audio output muted for this launch; saved audio settings unchanged");
}

ConfigManager.Load();
// GT2's enhanced draw-distance and maximum-LOD modes use the larger polygon
// buffer layout from PlayStation development hardware. The guest still boots
// and otherwise behaves as the retail Simulation Disc.
RecompOne.Runtime.Runtime.SetMode(RecompOne.Runtime.RunMode.Devkit);

string looseRoot = positionalArgs.Length > 0
    ? Path.GetFullPath(positionalArgs[0], launchDirectory)
    : AppContext.BaseDirectory;
if (!Directory.Exists(looseRoot))
{
    Console.Error.WriteLine($"Loose game directory is missing: {looseRoot}");
    return 1;
}

string[] missingBootstrapFiles = new[] { "SYSTEM.CNF", "recompone.loose.json" }
    .Where(fileName => !File.Exists(Path.Combine(looseRoot, fileName)))
    .ToArray();
if (missingBootstrapFiles.Length > 0)
{
    Console.Error.WriteLine("Loose game data is missing required file(s):");
    foreach (string fileName in missingBootstrapFiles)
        Console.Error.WriteLine($"  {Path.Combine(looseRoot, fileName)}");
    Console.Error.WriteLine("Disc-image fallback is disabled.");
    return 1;
}

Environment.SetEnvironmentVariable("RECOMPONE_LOOSE_DIR", looseRoot);
Console.WriteLine($"[Host] loose game data={looseRoot}");

PreloadBundledNative("SDL2.dll");
Entry.Run(new PSMemory(), loosePath: looseRoot);
return 0;

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
