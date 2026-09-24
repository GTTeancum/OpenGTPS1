using System.Buffers.Binary;
using RecompOne.Runtime;
using RecompOne.Runtime.Hardware;
using RecompOne.Runtime.Memory;
using RecompOne.Runtime.Sdk;

static void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

static uint SaveCrc32(ReadOnlySpan<byte> data)
{
    uint crc = uint.MaxValue;
    foreach (byte value in data)
    {
        crc ^= value;
        for (int bit = 0; bit < 8; bit++)
            crc = (crc & 1u) != 0u
                ? 0xEDB88320u ^ (crc >> 1)
                : crc >> 1;
    }
    return ~crc;
}

static void StoreValidCrc(byte[] payload)
{
    uint crc = SaveCrc32(payload.AsSpan(0, 0x7E9C));
    BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(0x7E9C, 4), crc);
}

static byte[] ReadSavePayload(MemoryCard card, int length)
{
    int first = card.Find("BASCUS-94455GAME");
    Require(first != 0, "Arcade save file disappeared");
    int[] chain = card.Chain(first);
    byte[] data = new byte[length];
    for (int offset = 0; offset < data.Length; offset++)
        data[offset] = card.ReadByte(chain, offset);
    return data;
}

static void LoadLiveArcadeState(IMemory memory, byte[] payload)
{
    const uint liveArcadeStateAddress = 0x801C9340u;
    const int payloadStateOffset = 0x200;
    const int liveArcadeStateLength = 0x7C9C;
    for (int offset = 0; offset < liveArcadeStateLength; offset++)
        memory.WriteU8(
            liveArcadeStateAddress + (uint)offset,
            payload[payloadStateOffset + offset]);
}

string root = Path.Combine(
    Path.GetTempPath(),
    "opengt-arcade-save-bridge-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
string cardAPath = Path.Combine(root, "carda.sav");
string cardBPath = Path.Combine(root, "cardb.sav");

Environment.SetEnvironmentVariable("RECOMPONE_CARD_A_PATH", cardAPath);
Environment.SetEnvironmentVariable("RECOMPONE_CARD_B_PATH", cardBPath);
Environment.SetEnvironmentVariable("RECOMPONE_GT2_ARCADE_SAVE_BRIDGE", "1");

try
{
    Runtime.CardA = new MemoryCard(cardAPath) { Enabled = true };
    Runtime.CardB = new MemoryCard(cardBPath) { Enabled = true };
    Require(
        Runtime.CardA.Create("BASCUS-94455GAME", 4) != 0,
        "Could not create four-block Arcade save fixture");

    const uint payloadAddress = 0x80010000u;
    const int payloadLength = 0x7F00;
    var memory = new TestMemory();
    byte[] payload = new byte[payloadLength];
    payload[0] = (byte)'S';
    payload[1] = (byte)'C';
    payload[2] = 0x13;
    payload[3] = 4;
    payload[0x2B8] = 4;
    payload[0x1200] = 0x5A;
    StoreValidCrc(payload);

    for (int offset = 0; offset < payload.Length; offset++)
        memory.WriteU8(payloadAddress + (uint)offset, payload[offset]);
    LoadLiveArcadeState(memory, payload);

    Require(
        GT2Compat.CaptureArcadeSaveWrite(
            memory, 0, payloadAddress, payloadLength),
        "CRC-valid Arcade payload was not captured");

    // Operation 6 is allowed to reuse/clobber guest RAM after submission.
    memory.WriteU8(payloadAddress + 0x2B8u, 0x99);
    memory.WriteU8(payloadAddress + 0x1200u, 0xA5);

    Require(
        GT2Compat.CompleteArcadeSaveWrite(),
        "native-success completion did not commit the captured payload");

    var reloaded = new MemoryCard(cardAPath) { Enabled = true };
    byte[] persisted = ReadSavePayload(reloaded, payloadLength);
    Require(
        persisted.AsSpan().SequenceEqual(payload),
        "card reload did not preserve the pre-operation snapshot exactly");
    Require(
        BinaryPrimitives.ReadUInt32LittleEndian(
            persisted.AsSpan(0x7E9C, 4)) ==
        SaveCrc32(persisted.AsSpan(0, 0x7E9C)),
        "persisted Arcade save CRC is invalid");
    Require(
        persisted[0x2B8] == 4 && persisted[0x1200] == 0x5A,
        "guest post-submit clobber leaked into persisted Arcade save");

    // With the bridge disabled, neither capture nor completion may touch disk.
    byte[] disabledCandidate = (byte[])payload.Clone();
    disabledCandidate[0x2B8] = 5;
    StoreValidCrc(disabledCandidate);
    for (int offset = 0; offset < disabledCandidate.Length; offset++)
        memory.WriteU8(
            payloadAddress + (uint)offset,
            disabledCandidate[offset]);
    Environment.SetEnvironmentVariable(
        "RECOMPONE_GT2_ARCADE_SAVE_BRIDGE", "0");
    Require(
        !GT2Compat.CaptureArcadeSaveWrite(
            memory, 0, payloadAddress, payloadLength) &&
        !GT2Compat.CompleteArcadeSaveWrite(),
        "disabled Arcade save bridge unexpectedly accepted a write");
    reloaded = new MemoryCard(cardAPath) { Enabled = true };
    Require(
        ReadSavePayload(reloaded, payloadLength).AsSpan().SequenceEqual(payload),
        "disabled Arcade save bridge changed the card");

    // A corrupt outgoing payload must be rejected even when the bridge is on.
    Environment.SetEnvironmentVariable(
        "RECOMPONE_GT2_ARCADE_SAVE_BRIDGE", "1");

    // The disabled candidate still has a valid CRC, but it describes Arcade
    // state 5 while the live working model remains at state 4. It must not
    // be accepted merely because the old serialized image is self-consistent.
    Require(
        !GT2Compat.CaptureArcadeSaveWrite(
            memory, 0, payloadAddress, payloadLength) &&
        !GT2Compat.CompleteArcadeSaveWrite(),
        "CRC-valid stale Arcade save payload was accepted");
    reloaded = new MemoryCard(cardAPath) { Enabled = true };
    Require(
        ReadSavePayload(reloaded, payloadLength).AsSpan().SequenceEqual(payload),
        "rejected stale payload changed the card");

    // A corrupt outgoing payload must also be rejected even when live state
    // itself is still current.
    for (int offset = 0; offset < payload.Length; offset++)
        memory.WriteU8(payloadAddress + (uint)offset, payload[offset]);
    memory.WriteU8(payloadAddress + 0x2B8u, 6);
    Require(
        !GT2Compat.CaptureArcadeSaveWrite(
            memory, 0, payloadAddress, payloadLength) &&
        !GT2Compat.CompleteArcadeSaveWrite(),
        "CRC-invalid Arcade save payload was accepted");
    reloaded = new MemoryCard(cardAPath) { Enabled = true };
    Require(
        ReadSavePayload(reloaded, payloadLength).AsSpan().SequenceEqual(payload),
        "rejected CRC-invalid payload changed the card");

    Console.WriteLine("Arcade save bridge persistence regression: PASS");
}
finally
{
    Environment.SetEnvironmentVariable(
        "RECOMPONE_GT2_ARCADE_SAVE_BRIDGE", null);
    Environment.SetEnvironmentVariable("RECOMPONE_CARD_A_PATH", null);
    Environment.SetEnvironmentVariable("RECOMPONE_CARD_B_PATH", null);
    try
    {
        Directory.Delete(root, recursive: true);
    }
    catch
    {
    }
}


sealed class TestMemory : IMemory
{
    readonly Dictionary<uint, byte> _bytes = new();

    public byte ReadU8(uint address) =>
        _bytes.TryGetValue(address, out byte value) ? value : (byte)0;

    public ushort ReadU16(uint address) =>
        (ushort)(ReadU8(address) | (ReadU8(address + 1u) << 8));

    public uint ReadU32(uint address) =>
        (uint)(
            ReadU8(address) |
            (ReadU8(address + 1u) << 8) |
            (ReadU8(address + 2u) << 16) |
            (ReadU8(address + 3u) << 24));

    public void WriteU8(uint address, byte value) => _bytes[address] = value;

    public void WriteU16(uint address, ushort value)
    {
        WriteU8(address, (byte)value);
        WriteU8(address + 1u, (byte)(value >> 8));
    }

    public void WriteU32(uint address, uint value)
    {
        WriteU8(address, (byte)value);
        WriteU8(address + 1u, (byte)(value >> 8));
        WriteU8(address + 2u, (byte)(value >> 16));
        WriteU8(address + 3u, (byte)(value >> 24));
    }

    public uint ReadWordLeft(uint current, uint address) =>
        throw new NotSupportedException();

    public uint ReadWordRight(uint current, uint address) =>
        throw new NotSupportedException();

    public void WriteWordLeft(uint address, uint value) =>
        throw new NotSupportedException();

    public void WriteWordRight(uint address, uint value) =>
        throw new NotSupportedException();

    public void LoadBytes(uint address, byte[] data)
    {
        for (int offset = 0; offset < data.Length; offset++)
            WriteU8(address + (uint)offset, data[offset]);
    }

    public void ZeroRange(uint address, uint length)
    {
        for (uint offset = 0; offset < length; offset++)
            WriteU8(address + offset, 0);
    }

    public void SetCd(RecompOne.Runtime.Cdrom.CdController cd)
    {
    }
}
