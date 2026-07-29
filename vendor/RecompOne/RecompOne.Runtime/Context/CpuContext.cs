using RecompOne.Runtime;

namespace RecompOne.Runtime.Context;

public sealed class CpuContext
{
    readonly uint[] _gpr = new uint[32];
    readonly GteProjectedValue[] _projected = new GteProjectedValue[32];

    uint Get(int index)
    {
        uint value = index == 0 ? 0u : _gpr[index];
        if (Gte.ProjectionTrackingEnabled)
            Gte.NotifyCpuRegisterRead(
                value,
                index == 0 ? default : _projected[index]);
        return value;
    }

    void Set(int index, uint value)
    {
        GteProjectedValue projected =
            Gte.ProjectionTrackingEnabled
                ? Gte.ConsumeCpuRegisterWrite(value)
                : default;
        if (index == 0)
            return;
        _gpr[index] = value;
        _projected[index] = projected;
    }

    public void ClearProjectionMetadata() =>
        Array.Clear(_projected);

    // Read-only diagnostics sometimes run inside memory-write callbacks where
    // the normal register getters would themselves alter projection-flow
    // metadata. This accessor deliberately has no architectural side effects.
    public uint PeekRaw(int index) =>
        index is <= 0 or >= 32 ? 0u : _gpr[index];

    public uint At { get => Get(1);  set => Set(1, value); }
    public uint V0 { get => Get(2);  set => Set(2, value); }
    public uint V1 { get => Get(3);  set => Set(3, value); }
    public uint A0 { get => Get(4);  set => Set(4, value); }
    public uint A1 { get => Get(5);  set => Set(5, value); }
    public uint A2 { get => Get(6);  set => Set(6, value); }
    public uint A3 { get => Get(7);  set => Set(7, value); }
    public uint T0 { get => Get(8);  set => Set(8, value); }
    public uint T1 { get => Get(9);  set => Set(9, value); }
    public uint T2 { get => Get(10); set => Set(10, value); }
    public uint T3 { get => Get(11); set => Set(11, value); }
    public uint T4 { get => Get(12); set => Set(12, value); }
    public uint T5 { get => Get(13); set => Set(13, value); }
    public uint T6 { get => Get(14); set => Set(14, value); }
    public uint T7 { get => Get(15); set => Set(15, value); }
    public uint S0 { get => Get(16); set => Set(16, value); }
    public uint S1 { get => Get(17); set => Set(17, value); }
    public uint S2 { get => Get(18); set => Set(18, value); }
    public uint S3 { get => Get(19); set => Set(19, value); }
    public uint S4 { get => Get(20); set => Set(20, value); }
    public uint S5 { get => Get(21); set => Set(21, value); }
    public uint S6 { get => Get(22); set => Set(22, value); }
    public uint S7 { get => Get(23); set => Set(23, value); }
    public uint T8 { get => Get(24); set => Set(24, value); }
    public uint T9 { get => Get(25); set => Set(25, value); }
    public uint K0 { get => Get(26); set => Set(26, value); }
    public uint K1 { get => Get(27); set => Set(27, value); }
    public uint GP { get => Get(28); set => Set(28, value); }
    public uint SP { get => Get(29); set => Set(29, value); }
    public uint FP { get => Get(30); set => Set(30, value); }
    public uint RA { get => Get(31); set => Set(31, value); }

    public uint HI;
    public uint LO;
    
    public uint SR; 
    public uint Cause; 
    public uint EPC;
    public uint BadVAddr; 
    public uint PRId; 
    
    public uint this[int index]
    {
        get => Get(index);
        set => Set(index, value);
    }

    public (uint[] gpr, uint hi, uint lo, GteProjectedValue[] projected)
        Snapshot() =>
        ((uint[])_gpr.Clone(), HI, LO,
         (GteProjectedValue[])_projected.Clone());

    public void Restore(
        (uint[] gpr, uint hi, uint lo, GteProjectedValue[] projected) s)
    {
        Array.Copy(s.gpr, _gpr, 32);
        Array.Copy(s.projected, _projected, 32);
        HI = s.hi;
        LO = s.lo;
    }
}
