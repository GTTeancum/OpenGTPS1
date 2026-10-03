using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;
using RecompOne.Runtime.Sdk;

Check("Simulation", Recompiled.Simulation.GranTurismo2PC.func_8001523C,
    0x801D585C, 0x801C856C, 0x801C8570);
Check("Arcade", Recompiled.Arcade.GranTurismo2ArcadePC.func_8001523C,
    0x801D52BC, 0x801C7FC4, 0x801C7FC8);

static void Check(string mode, Action<CpuContext, IMemory> initialize,
    uint configuration, uint coefficient, uint frequency)
{
    var memory = new InitializationProbe(coefficient);
    const uint scheduler = 0x800A0000;
    uint RunGuestInitializer()
    {
        try { initialize(new CpuContext { SP = 0x801F0000 }, memory); }
        catch (InitializationComplete) { return memory.ReadU32(coefficient); }
        throw new Exception("Guest initializer did not reach its physics constants");
    }

    // Execute the original guest instructions up to the physics coefficient
    // write. This proves why retaining the previous presentation step is wrong.
    memory.WriteU8(configuration + 8, 2);
    if (RunGuestInitializer() != 0x888) throw new Exception("Unexpected authored coefficient");
    memory.WriteU8(configuration + 8, 1);
    if (RunGuestInitializer() != 0x444) throw new Exception("Baseline retry difference was not reproduced");

    foreach (byte step in new byte[] { 0, 2, 1, 1, 1 })
    {
        memory.WriteU8(configuration + 8, step);
        memory.WriteU32(scheduler + 0x18, step);
        memory.WriteU16(scheduler + 0x1E, 7);
        GT2Compat.PrepareTrue60HzRaceInitialization(configuration, scheduler, memory);
        if (RunGuestInitializer() != 0x888 || memory.ReadU32(frequency) != 30 ||
            memory.ReadU32(scheduler + 0x18) != 2)
            throw new Exception($"{mode} step {step}: inconsistent physics initialization");
        GT2Compat.ConfigureTrue60HzRaceTimeStep(configuration, memory);
        if (memory.ReadU8(configuration + 8) != 1 || memory.ReadU16(scheduler + 0x1E) != 7)
            throw new Exception("Presentation step or independent buffer phase changed incorrectly");
    }
    Console.WriteLine($"{mode}: original guest coefficient first/retry=0x888/0x444; fixed initial/retry/replay=0x888; presentation=60 Hz");
}

sealed class InitializationComplete : Exception { }

// Stop after the real guest has written both constants, before it needs car
// assets. This is an observation breakpoint, not a substitute physics model.
sealed class InitializationProbe(uint stopAddress) : IMemory
{
    readonly Dictionary<uint, byte> bytes = new();
    public byte ReadU8(uint a) => bytes.GetValueOrDefault(a);
    public ushort ReadU16(uint a) => (ushort)(ReadU8(a) | ReadU8(a + 1) << 8);
    public uint ReadU32(uint a) => ReadU16(a) | (uint)ReadU16(a + 2) << 16;
    public void WriteU8(uint a, byte v) => bytes[a] = v;
    public void WriteU16(uint a, ushort v) { WriteU8(a, (byte)v); WriteU8(a + 1, (byte)(v >> 8)); }
    public void WriteU32(uint a, uint v)
    {
        WriteU16(a, (ushort)v); WriteU16(a + 2, (ushort)(v >> 16));
        if (a == stopAddress) throw new InitializationComplete();
    }
    public uint ReadWordLeft(uint current, uint address) => throw new NotSupportedException();
    public uint ReadWordRight(uint current, uint address) => throw new NotSupportedException();
    public void WriteWordLeft(uint address, uint value) => throw new NotSupportedException();
    public void WriteWordRight(uint address, uint value) => throw new NotSupportedException();
    public void LoadBytes(uint address, byte[] data) => throw new NotSupportedException();
    public void ZeroRange(uint address, uint length) => throw new NotSupportedException();
    public void SetCd(RecompOne.Runtime.Cdrom.CdController cd) => throw new NotSupportedException();
}
