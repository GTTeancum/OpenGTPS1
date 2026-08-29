using System.Runtime.CompilerServices;

namespace RecompOne.Runtime.Memory;

/// <summary>
/// Provides a concrete dispatch boundary for recompiled guest loads and
/// stores. Generated methods receive IMemory because relocatable overlays can
/// substitute an address-translating view, but the process has only two hot
/// implementations. Keeping this shim in its own assembly lets the renderer
/// runtime remain ahead-of-time compiled while the JIT specializes and folds
/// this small boundary into each profiled guest hot path.
/// </summary>
public static class MemoryAccess
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static byte ReadU8(IMemory memory, uint address)
    {
        if (memory is PSMemory ps) return ps.ReadU8(address);
        return memory.ReadU8(address);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ushort ReadU16(IMemory memory, uint address)
    {
        if (memory is PSMemory ps) return ps.ReadU16(address);
        return memory.ReadU16(address);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static uint ReadU32(IMemory memory, uint address)
    {
        if (memory is PSMemory ps) return ps.ReadU32(address);
        return memory.ReadU32(address);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void WriteU8(IMemory memory, uint address, byte value)
    {
        if (memory is PSMemory ps) ps.WriteU8(address, value);
        else memory.WriteU8(address, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void WriteU16(IMemory memory, uint address, ushort value)
    {
        if (memory is PSMemory ps) ps.WriteU16(address, value);
        else memory.WriteU16(address, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void WriteU32(IMemory memory, uint address, uint value)
    {
        if (memory is PSMemory ps) ps.WriteU32(address, value);
        else memory.WriteU32(address, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static uint ReadWordLeft(IMemory memory, uint current, uint address)
    {
        if (memory is PSMemory ps) return ps.ReadWordLeft(current, address);
        return memory.ReadWordLeft(current, address);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static uint ReadWordRight(IMemory memory, uint current, uint address)
    {
        if (memory is PSMemory ps) return ps.ReadWordRight(current, address);
        return memory.ReadWordRight(current, address);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void WriteWordLeft(IMemory memory, uint address, uint value)
    {
        if (memory is PSMemory ps) ps.WriteWordLeft(address, value);
        else memory.WriteWordLeft(address, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void WriteWordRight(IMemory memory, uint address, uint value)
    {
        if (memory is PSMemory ps) ps.WriteWordRight(address, value);
        else memory.WriteWordRight(address, value);
    }
}
