using System.Buffers.Binary;
using System.Globalization;
using System.Text.Json;

namespace RecompOne.Runtime.Hle;

// Authored image uploads are movable assets, not persistent VRAM page identities.
internal sealed class MenuTexturePack
{
    internal sealed record Entry(ulong Key, ulong Palette, int Mode,
        int SourceWidth, int SourceHeight, int Width, int Height, byte[] Pixels);
    internal readonly record struct Upload(ulong Key, int X, int Y, int Width, int Height);
    readonly List<Entry> _entries = [];
    readonly List<Upload> _uploads = [];
    readonly HashSet<ulong> _hits = [];
    public bool Active => _entries.Count != 0;

    public void Load()
    {
        string? configured = Environment.GetEnvironmentVariable("OPENGT_TEXTURE_PACK_DIR");
        string directory = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(Environment.CurrentDirectory, "mods", "enhanced_textures_4x") : configured;
        string manifest = Path.Combine(directory, "manifest.json");
        if (!File.Exists(manifest)) return;
        try
        {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(manifest));
            JsonElement root = document.RootElement;
            if (root.GetProperty("format").GetInt32() != 7) return;
            long totalBytes = 0;
            foreach (JsonElement item in root.GetProperty("entries").EnumerateArray())
            {
                if (!item.TryGetProperty("screenSpace", out var screen) || !screen.GetBoolean()) continue;
                if (item.GetProperty("replacementMode").GetString() != "rgb" ||
                    item.GetProperty("colorFit").GetInt32() != 0)
                    throw new InvalidDataException("Menu textures require RGB with colorFit=0.");
                string path = Path.GetFullPath(Path.Combine(directory, item.GetProperty("image").GetString()!));
                string relative = Path.GetRelativePath(Path.GetFullPath(directory), path);
                if (Path.IsPathRooted(relative) || relative == ".." || relative.StartsWith(".." + Path.DirectorySeparatorChar))
                    throw new InvalidDataException("Menu texture escapes pack directory.");
                if (new FileInfo(path).Length > 256 * 1024 * 1024)
                    throw new InvalidDataException("Menu DDS exceeds memory budget.");
                byte[] dds = File.ReadAllBytes(path);
                int Read(int offset) => BinaryPrimitives.ReadInt32LittleEndian(dds.AsSpan(offset, 4));
                if (dds.Length < 128 || Read(0) != 0x20534444 || Read(4) != 124 ||
                    Read(76) != 32 || (Read(80) & 0x40) == 0 || Read(84) != 0 || Read(88) != 32)
                    throw new InvalidDataException("Menu textures require uncompressed 32-bit DDS.");
                int w = Read(16), h = Read(12);
                int sw = item.GetProperty("sourceWidth").GetInt32(), sh = item.GetProperty("sourceHeight").GetInt32();
                int mode = item.GetProperty("pixelMode").GetInt32();
                if (w <= 0 || h <= 0 || w > 16384 || h > 16384 || sw <= 0 || sh <= 0 ||
                    w % sw != 0 || h % sh != 0 || w / sw != h / sh || mode is < 0 or > 1 ||
                    item.GetProperty("x").GetInt32() != 0 || item.GetProperty("y").GetInt32() != 0 ||
                    item.GetProperty("width").GetInt32() != w || item.GetProperty("height").GetInt32() != h ||
                    dds.Length != 128L + w * (long)h * 4 || (totalBytes += w * (long)h * 4) > 256 * 1024 * 1024)
                    throw new InvalidDataException("Invalid menu texture dimensions or memory budget.");
                int r = Read(92), g = Read(96), b = Read(100), a = Read(104);
                if (g != 0xFF00 || a != unchecked((int)0xFF000000) ||
                    !((r == 0xFF && b == 0xFF0000) || (b == 0xFF && r == 0xFF0000)))
                    throw new InvalidDataException("Unsupported DDS channel masks.");
                byte[] pixels = dds[128..];
                if (r == 0xFF0000)
                    for (int i = 0; i < pixels.Length; i += 4)
                        (pixels[i], pixels[i + 2]) = (pixels[i + 2], pixels[i]);
                ulong key = ulong.Parse(item.GetProperty("key").GetString()!, NumberStyles.HexNumber);
                ulong palette = item.TryGetProperty("paletteKey", out var p)
                    ? ulong.Parse(p.GetString()!, NumberStyles.HexNumber) : 0;
                if (_entries.Any(e => e.Key == key && e.Palette == palette))
                    throw new InvalidDataException("Duplicate menu texture identity.");
                _entries.Add(new(key, palette, mode, sw, sh, w, h, pixels));
            }
            _entries.Sort((a, b) => (b.Palette != 0).CompareTo(a.Palette != 0));
            if (Active) Console.WriteLine($"[TexturePack] loaded {_entries.Count} authored menu textures");
        }
        catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or FormatException or OverflowException or ArgumentException or KeyNotFoundException)
        {
            _entries.Clear();
            Console.Error.WriteLine($"[TexturePack] rejected menu pack: {error.Message}");
        }
    }

    internal void AddForTest(Entry entry) => _entries.Add(entry);
    internal void Clear() { _entries.Clear(); _uploads.Clear(); _hits.Clear(); }

    public void Invalidate(int x, int y, int w, int h) => _uploads.RemoveAll(u =>
        x < u.X + u.Width && u.X < x + w && y < u.Y + u.Height && u.Y < y + h);

    public void Record(int x, int y, int w, int h, ReadOnlySpan<ushort> words)
    {
        if (!Active) return;
        Invalidate(x, y, w, h);
        if (w <= 0 || h <= 0 || x < 0 || y < 0 || x + w > 1024 || y + h > 512 || words.Length != w * h) return;
        _uploads.Add(new(Hash(w, h, words), x, y, w, h));
    }

    internal static ulong Hash(int w, int h, ReadOnlySpan<ushort> words)
    {
        ulong hash = 14695981039346656037UL;
        void Add(byte b) { hash = unchecked((hash ^ b) * 1099511628211UL); }
        Add((byte)w); Add((byte)(w >> 8)); Add((byte)h); Add((byte)(h >> 8));
        foreach (ushort word in words) { Add((byte)word); Add((byte)(word >> 8)); }
        return hash;
    }

    public (Entry? Entry, int U, int V) Resolve(int page, int clut, int u, int v, int w, int h)
    {
        int mode = (page >> 7) & 3, ppw = mode == 0 ? 4 : 2;
        if (mode > 1 || u < 0 || v < 0 || u + w > 256 || v + h > 256) return default;
        int px = (page & 15) * 64, py = ((page >> 4) & 1) * 256;
        int cx = (clut & 63) * 16, cy = (clut >> 6) & 511;
        foreach (Entry entry in _entries)
        foreach (Upload upload in _uploads)
        {
            if (entry.Key != upload.Key || entry.Mode != mode || entry.SourceWidth != upload.Width * ppw || entry.SourceHeight != upload.Height) continue;
            int ox = (upload.X - px) * ppw, oy = upload.Y - py;
            if (u < ox || v < oy || u + w > ox + entry.SourceWidth || v + h > oy + entry.SourceHeight) continue;
            if (entry.Palette != 0 && !_uploads.Any(p => p.Key == entry.Palette &&
                cx >= p.X && cx + (mode == 0 ? 16 : 256) <= p.X + p.Width && cy >= p.Y && cy < p.Y + p.Height)) continue;
            if (_hits.Add(entry.Key)) Console.WriteLine($"[TexturePack] matched authored menu bitmap={entry.Key:x16}");
            return (entry, ox, oy);
        }
        return default;
    }
}
