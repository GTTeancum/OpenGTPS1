using System.Buffers;
using RecompOne.Runtime;

namespace RecompOne.Runtime.Context;

public sealed class CpuContext
{
    internal readonly struct SnapshotState
    {
        internal readonly uint[] Gpr;
        internal readonly GteProjectedValue[] Projected;
        internal readonly GteProjectionOrigin[] ProjectionOrigins;
        internal readonly uint Hi;
        internal readonly uint Lo;

        internal SnapshotState(
            uint[] gpr,
            GteProjectedValue[] projected,
            GteProjectionOrigin[] projectionOrigins,
            uint hi,
            uint lo)
        {
            Gpr = gpr;
            Projected = projected;
            ProjectionOrigins = projectionOrigins;
            Hi = hi;
            Lo = lo;
        }
    }

    readonly uint[] _gpr = new uint[32];
    readonly GteProjectedValue[] _projected = new GteProjectedValue[32];
    readonly GteProjectionOrigin[] _projectionOrigins =
        new GteProjectionOrigin[32];

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    uint Get(int index)
    {
        uint value = index == 0 ? 0u : _gpr[index];
        if (Gte.ProjectionTrackingEnabled)
        {
            if (WorldCaptureContext.CaptureEnabled)
            {
                Gte.NotifyCpuRegisterRead(
                    value,
                    index == 0 ? default : _projected[index],
                    index == 0 ? default : _projectionOrigins[index]);
            }
            else
            {
                Gte.NotifyCpuRegisterRead(
                    value,
                    index == 0 ? default : _projected[index]);
            }
        }
        return value;
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    void Set(int index, uint value)
    {
        GteProjectionOrigin origin = default;
        GteProjectedValue projected = default;
        if (Gte.ProjectionTrackingEnabled && Gte.HasPendingCpuProjection)
        {
            projected = WorldCaptureContext.CaptureEnabled
                ? Gte.ConsumeCpuRegisterWrite(value, out origin)
                : Gte.ConsumeCpuRegisterWrite(value);
        }
        if (index == 0)
            return;
        _gpr[index] = value;
        _projected[index] = projected;
        if (WorldCaptureContext.CaptureEnabled)
            _projectionOrigins[index] = origin;
    }

    public void ClearProjectionMetadata()
    {
        Array.Clear(_projected);
        Array.Clear(_projectionOrigins);
    }

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

    internal SnapshotState Snapshot()
    {
        uint[] gpr = ArrayPool<uint>.Shared.Rent(32);
        GteProjectedValue[] projected =
            ArrayPool<GteProjectedValue>.Shared.Rent(32);
        GteProjectionOrigin[] projectionOrigins =
            ArrayPool<GteProjectionOrigin>.Shared.Rent(32);
        _gpr.CopyTo(gpr, 0);
        _projected.CopyTo(projected, 0);
        _projectionOrigins.CopyTo(projectionOrigins, 0);
        return new SnapshotState(
            gpr,
            projected,
            projectionOrigins,
            HI,
            LO);
    }

    internal void Restore(SnapshotState snapshot)
    {
        try
        {
            Array.Copy(snapshot.Gpr, _gpr, 32);
            Array.Copy(snapshot.Projected, _projected, 32);
            Array.Copy(
                snapshot.ProjectionOrigins,
                _projectionOrigins,
                32);
            HI = snapshot.Hi;
            LO = snapshot.Lo;
        }
        finally
        {
            ArrayPool<uint>.Shared.Return(snapshot.Gpr);
            ArrayPool<GteProjectedValue>.Shared.Return(snapshot.Projected);
            ArrayPool<GteProjectionOrigin>.Shared.Return(
                snapshot.ProjectionOrigins);
        }
    }
}
