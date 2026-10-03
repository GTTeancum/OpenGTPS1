using RecompOne.Runtime.Hle;

static void Check(bool condition, string description)
{
    if (!condition) throw new Exception(description);
    Console.WriteLine("PASS: " + description);
}

var pack = new MenuTexturePack();
ushort[] bitmap = Enumerable.Repeat((ushort)0x1111, 64).ToArray();
ushort[] palette = new ushort[16];
palette[1] = 31;
ulong key = MenuTexturePack.Hash(8, 8, bitmap);
Check(key == 0xf25950c6903d0c45UL, "upload identity matches offline FNV convention");
pack.AddForTest(new(key, MenuTexturePack.Hash(16, 1, palette), 0, 32, 8, 128, 32, new byte[128 * 32 * 4]));
pack.Record(640, 0, 8, 8, bitmap);
Check(pack.Resolve(10, 400 << 6, 0, 0, 32, 8).Entry == null, "unmatched palette falls back");
pack.Record(0, 400, 16, 1, palette);
Check(pack.Resolve(10, 400 << 6, 0, 0, 32, 8).Entry != null, "complete authored asset resolves");
Check(pack.Resolve(10, 400 << 6, 24, 0, 16, 8).Entry == null, "primitive cannot sample beyond asset edge");
pack.Invalidate(640, 0, 8, 8);
pack.Record(704, 256, 8, 8, bitmap);
Check(pack.Resolve(27, 400 << 6, 0, 0, 32, 8).Entry != null, "relocated bitmap retains replacement identity");
pack.Record(704, 256, 1, 1, new ushort[] { 0 });
Check(pack.Resolve(27, 400 << 6, 0, 0, 32, 8).Entry == null, "partial overwrite invalidates whole authored asset");

string temporary = Path.Combine(Path.GetTempPath(), "opengt-menu-test-" + Guid.NewGuid());
Directory.CreateDirectory(temporary);
string? previous = Environment.GetEnvironmentVariable("OPENGT_TEXTURE_PACK_DIR");
try
{
    Environment.SetEnvironmentVariable("OPENGT_TEXTURE_PACK_DIR", temporary);
    File.WriteAllText(Path.Combine(temporary, "manifest.json"), "{broken json");
    var invalid = new MenuTexturePack();
    invalid.Load();
    Check(!invalid.Active, "malformed optional pack falls back without crashing");
}
finally
{
    Environment.SetEnvironmentVariable("OPENGT_TEXTURE_PACK_DIR", previous);
    File.Delete(Path.Combine(temporary, "manifest.json"));
    Directory.Delete(temporary);
}
