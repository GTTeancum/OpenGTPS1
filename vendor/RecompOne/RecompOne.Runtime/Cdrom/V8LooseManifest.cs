using System.Reflection;
using System.Text.Json;
using RecompOne.Runtime.Serialization;

namespace RecompOne.Runtime.Cdrom;

internal sealed class V8LooseManifest
{
    public int FormatVersion { get; set; }
    public string Volume { get; set; } = "";
    public int LeadOutLba { get; set; }
    public Dictionary<int, string> MetadataSectors { get; set; } = [];
    public List<V8LooseFile> Files { get; set; } = [];
    public List<V8LooseTrack> Tracks { get; set; } = [];

    public static V8LooseManifest Load(string looseRoot)
    {
        string? overridePath =
            Environment.GetEnvironmentVariable("RECOMPONE_LOOSE_MANIFEST");
        string externalPath = string.IsNullOrWhiteSpace(overridePath)
            ? Path.Combine(looseRoot, "recompone.loose.json")
            : Path.GetFullPath(
                Path.IsPathRooted(overridePath)
                    ? overridePath
                    : Path.Combine(looseRoot, overridePath));
        string relativeManifest = Path.GetRelativePath(
            Path.GetFullPath(looseRoot), externalPath);
        if (relativeManifest == ".." ||
            relativeManifest.StartsWith(
                $"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            throw new InvalidDataException(
                $"Loose manifest must remain inside the install root: {externalPath}");
        if (File.Exists(externalPath))
        {
            using Stream external = File.OpenRead(externalPath);
            var loaded = JsonSerializer.Deserialize(
                external, RuntimeJsonContext.Default.V8LooseManifest) ??
                throw new InvalidDataException(
                    $"Loose-disc manifest is empty: {externalPath}");
            Validate(loaded, externalPath);
            return loaded;
        }

        string systemPath = Path.Combine(looseRoot, "SYSTEM.CNF");
        string system = File.ReadAllText(systemPath);
        string manifestFile = system.Contains(
            "SLUS_008.68", StringComparison.OrdinalIgnoreCase)
            ? "V82LooseManifest.json"
            : system.Contains("SLUS_005.10", StringComparison.OrdinalIgnoreCase)
                ? "V8LooseManifest.json"
                : throw new InvalidDataException(
                    $"Unsupported loose SYSTEM.CNF: {systemPath}");
        var assembly = typeof(V8LooseManifest).Assembly;
        string resourceName = assembly.GetManifestResourceNames().Single(name =>
            name.EndsWith(manifestFile, StringComparison.Ordinal));
        using Stream stream = assembly.GetManifestResourceStream(resourceName) ??
            throw new InvalidOperationException(
                $"Embedded loose-disc manifest is missing: {resourceName}");
        var manifest = JsonSerializer.Deserialize(stream, RuntimeJsonContext.Default.V8LooseManifest) ??
            throw new InvalidDataException("Loose-disc manifest is empty");
        Validate(manifest, resourceName);
        return manifest;
    }

    private static void Validate(V8LooseManifest manifest, string source)
    {
        if (manifest.FormatVersion != 1 ||
            manifest.Files.Count == 0 ||
            manifest.Tracks.Count == 0)
        {
            throw new InvalidDataException(
                $"Unsupported loose-disc manifest version {manifest.FormatVersion}: {source}");
        }
    }
}

internal sealed class V8LooseFile
{
    public string Path { get; set; } = "";
    public string? Source { get; set; }
    public long SourceOffset { get; set; }
    public long? SourceLength { get; set; }
    public int Lba { get; set; }
    public uint Size { get; set; }
}

internal sealed class V8LooseTrack
{
    public int Number { get; set; }
    public int Index0Lba { get; set; }
    public int StartLba { get; set; }
    public int EndLba { get; set; }
    public string? Source { get; set; }
}
