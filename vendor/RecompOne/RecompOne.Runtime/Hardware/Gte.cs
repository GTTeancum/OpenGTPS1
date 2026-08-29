namespace RecompOne.Runtime;

public enum GteDepthProvenance : byte
{
    None,
    DirectStore,
    CpuRegisterFlow,
    RegisterValueMatch,
}

public readonly record struct GteProjectedValue(
    uint PackedXy,
    ushort Z,
    int Generation,
    GteDepthProvenance Provenance)
{
    public bool Valid => Provenance != GteDepthProvenance.None && Z != 0;
}

[Flags]
public enum GteProjectionOriginFlags : ushort
{
    None = 0,
    ScreenOffsetAnchor = 1 << 0,
}

public readonly record struct GteProjectionOrigin(
    short ModelX, short ModelY, short ModelZ,
    int ViewX, int ViewY, int ViewZ,
    long ViewXFixed, long ViewYFixed, long ViewZFixed,
    short R00, short R01, short R02,
    short R10, short R11, short R12,
    short R20, short R21, short R22,
    int TranslateX, int TranslateY, int TranslateZ,
    int ProjectionOffsetX, int ProjectionOffsetY,
    ushort ProjectionPlane,
    ulong TransformId,
    WorldObjectContext Object,
    short ScreenOffsetX,
    short ScreenOffsetY,
    GteProjectionOriginFlags Flags)
{
    public bool Valid => TransformId != 0;
}

internal readonly record struct GteProjectionOriginHandle(
    int Slot,
    int Sequence)
{
    public bool Valid => Sequence != 0;
}

public static class Gte
{
    public const float MaximumPerspectiveDepthRatio = 8f;
    public static bool ProjectionTrackingEnabled { get; private set; }
    static readonly short[] V = new short[9];
    static byte RGBC_R, RGBC_G, RGBC_B, RGBC_CODE;
    static ushort OTZ;
    static int IR0, IR1, IR2, IR3;
    static readonly short[] SX = new short[3];
    static readonly short[] SY = new short[3];
    static readonly ushort[] SZ = new ushort[4];
    // SXY and SZ are architecturally separate writable FIFOs. Guest code can
    // shift or overwrite SXY without doing the same to SZ, so deriving an
    // SXY register's depth from a fixed SZ index at StoreWord time is unsafe.
    // Keep projection metadata attached to the SXY slot that Rtp produced.
    static readonly ushort[] SxyDepth = new ushort[3];
    static readonly bool[] SxyDepthValid = new bool[3];
    static readonly GteProjectionOriginHandle[] SxyOrigin =
        new GteProjectionOriginHandle[3];
    // Projection provenance crosses several emulated transport stages before
    // a GPU packet owns it. Keep each full value once in a fixed value-type
    // ring and move only validated 8-byte handles through the GTE FIFO and CPU
    // registers. Capacity covers more than two maximum-size live frames; a
    // sequence tag makes overwrite fail closed instead of returning stale
    // provenance.
    const int ProjectionOriginCapacity = 262_144;
    const int ProjectionOriginMask = ProjectionOriginCapacity - 1;
    static readonly GteProjectionOrigin[] ProjectionOrigins =
        new GteProjectionOrigin[ProjectionOriginCapacity];
    static readonly int[] ProjectionOriginSequences =
        new int[ProjectionOriginCapacity];
    static int ProjectionOriginCursor;
    static int ProjectionOriginSequence;
    static int ProjectionOriginMisses;
    static readonly uint[] RGB = new uint[3];
    // Recovering depth from screen XY alone is ambiguous: a race frame can
    // project thousands of vertices and many unrelated vertices land on the
    // same integer pixel. Keep the depth attached to the exact RAM word that
    // receives an SXY register instead. DrawOTag can then carry that packet
    // address through to the GPU command decoder.
    readonly record struct PacketDepthSample(
        uint PackedXy, ushort Z, int Generation,
        GteDepthProvenance Provenance);
    readonly record struct PacketOriginSample(
        uint PackedXy,
        ushort Z,
        int Generation,
        GteDepthProvenance Provenance,
        GteProjectionOrigin Origin);
    static PacketDepthSample? PendingDirectStore;
    static GteProjectionOriginHandle PendingDirectOrigin;
    static GteProjectedValue PendingCpuValue;
    static GteProjectionOriginHandle PendingCpuOrigin;
    static GteProjectedValue DerivedScreenValue;
    static GteProjectionOrigin DerivedScreenOrigin;
    static float DerivedScreenOffsetScale = 1.0f;
    static readonly bool TraceDerivedScreenProjection =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_DERIVED_SCREEN_PROJECTION") == "1";
    static int DerivedScreenBeginTraceCount;
    static int DerivedScreenOffsetTraceCount;
#if !OPENGT_RELEASE_PACKAGE
    static readonly bool TraceUnknownWorldStacks =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_UNKNOWN_WORLD_STACK") == "1";
    static readonly int TraceUnknownWorldStackStartPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_UNKNOWN_WORLD_STACK_START_POLL"),
            out int traceUnknownWorldStackStartPoll)
            ? traceUnknownWorldStackStartPoll
            : 0;
    static readonly int TraceUnknownWorldStackLimit =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_UNKNOWN_WORLD_STACK_LIMIT"),
            out int traceUnknownWorldStackLimit)
            ? Math.Max(1, traceUnknownWorldStackLimit)
            : 12;
    static int TraceUnknownWorldStackCount;
#endif
    // Packet addresses already provide a collision-free key. Keep depth
    // samples in direct word-indexed storage instead of hashing every packet
    // read, write, and invalidation on the guest thread. The validity bitsets
    // below remain authoritative, so clearing tracking does not require
    // touching the 32 MiB backing store.
    static readonly PacketDepthSample[] PacketDepthRam =
        new PacketDepthSample[Memory.MemoryMap.DevkitRamSize / 4u];
    static readonly PacketDepthSample[] PacketDepthScratch =
        new PacketDepthSample[Memory.MemoryMap.ScratchpadSize / 4u];
    // Projection origins are much larger than depth samples, so a dense
    // origin array for every possible 8 MiB RAM word would waste hundreds of
    // MiB. Map packet words directly to compact reusable slots instead. This
    // keeps the hot read/write path hash-free while allocating full origins
    // only for addresses GT2 actually uses.
    static readonly int[] PacketOriginRamSlots =
        new int[Memory.MemoryMap.DevkitRamSize / 4u];
    static readonly int[] PacketOriginScratchSlots =
        new int[Memory.MemoryMap.ScratchpadSize / 4u];
    static readonly List<PacketOriginSample> PacketOriginSamples =
        new(65_536);
    static readonly Stack<int> FreePacketOriginSlots = new();
    static int PacketDepthCount;

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    static void ClearPendingCpuOrigin()
    {
        if (WorldCaptureContext.CaptureEnabled)
            PendingCpuOrigin = default;
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    static void ClearPendingDirectOrigin()
    {
        if (WorldCaptureContext.CaptureEnabled)
            PendingDirectOrigin = default;
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining |
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    static GteProjectionOriginHandle StoreProjectionOrigin(
        in GteProjectionOrigin origin)
    {
        int slot = ProjectionOriginCursor++ & ProjectionOriginMask;
        int sequence = unchecked(++ProjectionOriginSequence);
        if (sequence == 0)
            sequence = unchecked(++ProjectionOriginSequence);
        ProjectionOrigins[slot] = origin;
        ProjectionOriginSequences[slot] = sequence;
        return new GteProjectionOriginHandle(slot, sequence);
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining |
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    internal static bool TryResolveProjectionOrigin(
        in GteProjectionOriginHandle handle,
        out GteProjectionOrigin origin)
    {
        if (handle.Valid &&
            (uint)handle.Slot < ProjectionOriginCapacity &&
            ProjectionOriginSequences[handle.Slot] == handle.Sequence)
        {
            origin = ProjectionOrigins[handle.Slot];
            return true;
        }
        origin = default;
        if (handle.Valid && Interlocked.Increment(ref ProjectionOriginMisses) <= 8)
        {
            Console.Error.WriteLine(
                $"[GTE-Origin] stale transport handle " +
                $"slot={handle.Slot} sequence={handle.Sequence} " +
                $"generation={ScreenDepthGeneration}");
        }
        return false;
    }
    // Most guest RAM traffic has nothing to do with GPU packets. A compact
    // bitset prevents an expensive dictionary lookup on every RAM read/write
    // while still invalidating exact packet metadata on partial overwrites.
    static readonly uint[] PacketDepthRamBits =
        new uint[(Memory.MemoryMap.DevkitRamSize / 4u + 31u) / 32u];
    static readonly uint[] PacketDepthScratchBits =
        new uint[(Memory.MemoryMap.ScratchpadSize / 4u + 31u) / 32u];
    static int PacketDepthHits;
    static int PacketDepthMisses;
    static readonly bool TraceScreenDepth =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GTE_DEPTH") == "1";
    static readonly bool TracePacketWrites =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_GTE_PACKET_WRITES") == "1";
    static readonly int TracePacketWritesStartPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GTE_PACKET_WRITES_START_POLL"),
            out int packetWriteStartPoll)
            ? Math.Max(0, packetWriteStartPoll)
            : 0;
    static readonly int TracePacketWritesEndPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GTE_PACKET_WRITES_END_POLL"),
            out int packetWriteEndPoll)
            ? Math.Max(0, packetWriteEndPoll)
            : int.MaxValue;
    static readonly uint TracePacketWritesMinimumAddress =
        uint.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GTE_PACKET_WRITES_MIN_ADDRESS"),
            out uint packetWriteMinimumAddress)
            ? packetWriteMinimumAddress
            : 0x00200000u;
    static readonly uint TracePacketWritesMaximumAddress =
        uint.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GTE_PACKET_WRITES_MAX_ADDRESS"),
            out uint packetWriteMaximumAddress)
            ? packetWriteMaximumAddress
            : 0x00270000u;
    static int PacketWriteTraceCount;
    static int ScreenDepthGeneration;
    public static int ProjectionGeneration => ScreenDepthGeneration;
    const int PacketDepthMaxAge = 2;

    public static bool HasPendingCpuProjection
    {
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => PendingCpuValue.Valid;
    }
    static uint RES1;
    static int MAC0, MAC1, MAC2, MAC3;
    static uint LZCS, LZCR;

    static readonly short[] RT = new short[9];
    static ulong ProjectionTransformHash;
    static bool ProjectionTransformHashDirty = true;
    static readonly short[] LLM = new short[9];
    static readonly short[] LCM = new short[9];
    static readonly int[] TR = new int[3];
    static readonly int[] BK = new int[3];
    static readonly int[] FC = new int[3];
    static int OFX, OFY;
    static ushort H;
    static short DQA;
    static int DQB;
    static short ZSF3, ZSF4;
    static uint FLAG;
    static uint LastOperation;
    static readonly bool TraceVehicleProjectionLimits =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_GT2_VEHICLE_GTE_FLAGS") == "1";
    static int VehicleFlagPolicyReported;
    static int VehicleFlagPoll = -1;
    static uint VehicleFlagObject;
    static uint VehicleFlagModel;
    static int VehicleFlagReads;
    static int VehicleFlagRawFatal;
    static int VehicleFlagScreenSaturated;
    static int VehicleFlagDepthSaturated;
    static int VehicleFlagDivideOverflow;
    static int VehicleFlagProjectionArithmetic;
    static int VehicleFlagNonProjectionFatal;
    static int VehicleFlagRecovered;
    static int VehicleFlagPreservedFatal;
    static int VehicleFlagNclipCorrected;
    static bool VehicleProjectionClipLimited;

    static readonly byte[] Unr = BuildUnr();

    static byte[] BuildUnr()
    {
        var t = new byte[0x101];
        for (int i = 0; i < 0x101; i++)
        {
            int v = (0x40000 / (i + 0x100) + 1) / 2 - 0x101;
            t[i] = (byte)(v < 0 ? 0 : v > 0xFF ? 0xFF : v);
        }
        return t;
    }

    static void Flag(int bit) => FLAG |= 1u << bit;

    /// <summary>
    /// GT2 rejects a complete vehicle face whenever the GTE summary bit is
    /// set. Screen X/Y saturation, projection-depth saturation, and divide
    /// overflow are all clipping conditions for RTPS/RTPT. The modern renderer
    /// reconstructs the unsaturated vertex from its exact model/view
    /// provenance and clips it in homogeneous space, so rejecting the face in
    /// the guest makes close cars disappear in large slabs. RTPS/RTPT MAC and
    /// IR overflow flags are part of the same legacy projection failure: the
    /// exact pre-saturation coordinates remain valid. Defer the complete GTE
    /// projection summary only for vehicle RTPS/RTPT operations; preserve all
    /// non-projection GTE failures.
    /// </summary>
    static uint FilterModernVehicleProjectionLimits(uint value)
    {
        if (!WorldCaptureContext.CaptureEnabled ||
            WorldCaptureContext.Current.Kind != WorldObjectKind.Vehicle)
            return value;

        bool projection = LastOperation is 0x01u or 0x30u;
        uint filtered = FilterVehicleProjectionSummary(
            value,
            projection);

        TraceModernVehicleProjectionLimits(value, filtered, projection);
        return filtered;
    }

    static uint FilterVehicleProjectionSummary(
        uint value,
        bool projection)
    {
        if (!projection)
            return value;
        const uint projectionSummaryCauses = 0x7F87E000u;
        return value & ~(0x80000000u | projectionSummaryCauses);
    }

    static bool TryExactVehicleNclip(out int result)
    {
        result = 0;
        if (!VehicleProjectionClipLimited ||
            !WorldCaptureContext.CaptureEnabled ||
            WorldCaptureContext.Current.Kind != WorldObjectKind.Vehicle)
            return false;

        VehicleProjectionClipLimited = false;
        if (!TryResolveProjectionOrigin(SxyOrigin[0], out var a) ||
            !TryResolveProjectionOrigin(SxyOrigin[1], out var b) ||
            !TryResolveProjectionOrigin(SxyOrigin[2], out var c) ||
            a.Object.Kind != WorldObjectKind.Vehicle ||
            b.Object.Kind != WorldObjectKind.Vehicle ||
            c.Object.Kind != WorldObjectKind.Vehicle ||
            a.Object.StableId != b.Object.StableId ||
            a.Object.StableId != c.Object.StableId ||
            a.TransformId != b.TransformId ||
            a.TransformId != c.TransformId ||
            a.ViewZFixed == 0 || b.ViewZFixed == 0 || c.ViewZFixed == 0)
            return false;

        static double Project(long axis, long depth, ushort plane) =>
            plane * (double)axis / depth;
        double ax = Project(a.ViewXFixed, a.ViewZFixed, a.ProjectionPlane);
        double ay = Project(a.ViewYFixed, a.ViewZFixed, a.ProjectionPlane);
        double bx = Project(b.ViewXFixed, b.ViewZFixed, b.ProjectionPlane);
        double by = Project(b.ViewYFixed, b.ViewZFixed, b.ProjectionPlane);
        double cx = Project(c.ViewXFixed, c.ViewZFixed, c.ProjectionPlane);
        double cy = Project(c.ViewYFixed, c.ViewZFixed, c.ProjectionPlane);
        double determinant =
            ax * (by - cy) + bx * (cy - ay) + cx * (ay - by);
        if (!double.IsFinite(determinant))
            return false;

        long rounded = checked((long)Math.Round(
            Math.Clamp(
                determinant,
                (double)int.MinValue,
                (double)int.MaxValue),
            MidpointRounding.AwayFromZero));
        if (rounded == 0 && determinant != 0.0)
            rounded = determinant < 0.0 ? -1 : 1;
        result = (int)rounded;
        VehicleFlagNclipCorrected++;
        return true;
    }

    static void TraceModernVehicleProjectionLimits(
        uint raw,
        uint filtered,
        bool projection)
    {
        if (Interlocked.Exchange(ref VehicleFlagPolicyReported, 1) == 0)
        {
            Console.Error.WriteLine(
                "[GT2-Vehicle-GTE-Policy] " +
                "projectionSummary=defer-to-native " +
                "source=exact-view-provenance " +
                "preservedErrors=non-projection");
        }
        if (!TraceVehicleProjectionLimits)
            return;

        WorldObjectContext context = WorldCaptureContext.Current;
        int poll = Host.InputManager.CurrentPoll;
        if (VehicleFlagReads != 0 &&
            (poll != VehicleFlagPoll ||
             context.StableId != VehicleFlagObject ||
             context.ModelPointer != VehicleFlagModel))
        {
            FlushVehicleProjectionLimitTrace();
        }
        VehicleFlagPoll = poll;
        VehicleFlagObject = context.StableId;
        VehicleFlagModel = context.ModelPointer;
        VehicleFlagReads++;
        if ((raw & 0x80000000u) != 0)
            VehicleFlagRawFatal++;
        if ((raw & 0x00006000u) != 0)
            VehicleFlagScreenSaturated++;
        if (projection && (raw & 0x00040000u) != 0)
            VehicleFlagDepthSaturated++;
        if (projection && (raw & 0x00020000u) != 0)
            VehicleFlagDivideOverflow++;
        if (projection && (raw & 0x7F818000u) != 0)
            VehicleFlagProjectionArithmetic++;
        if (!projection && (raw & 0x80000000u) != 0)
            VehicleFlagNonProjectionFatal++;
        if ((raw & 0x80000000u) != 0 &&
            (filtered & 0x80000000u) == 0)
            VehicleFlagRecovered++;
        if ((filtered & 0x80000000u) != 0)
            VehicleFlagPreservedFatal++;
    }

    static void FlushVehicleProjectionLimitTrace()
    {
        if (VehicleFlagScreenSaturated != 0 ||
            VehicleFlagRawFatal != 0 ||
            VehicleFlagPreservedFatal != 0)
        {
            Console.Error.WriteLine(
                $"[GT2-VEHICLE-GTE-FLAG] poll={VehicleFlagPoll} " +
                $"object={VehicleFlagObject} model={VehicleFlagModel:X8} " +
                $"reads={VehicleFlagReads} rawFatal={VehicleFlagRawFatal} " +
                $"screenSaturated={VehicleFlagScreenSaturated} " +
                $"recovered={VehicleFlagRecovered} " +
                $"preservedFatal={VehicleFlagPreservedFatal} " +
                $"nclipCorrected={VehicleFlagNclipCorrected} " +
                $"depthSaturated={VehicleFlagDepthSaturated} " +
                $"divideOverflow={VehicleFlagDivideOverflow} " +
                $"projectionArithmetic={VehicleFlagProjectionArithmetic} " +
                $"nonProjectionFatal={VehicleFlagNonProjectionFatal}");
        }
        VehicleFlagReads = 0;
        VehicleFlagRawFatal = 0;
        VehicleFlagScreenSaturated = 0;
        VehicleFlagDepthSaturated = 0;
        VehicleFlagDivideOverflow = 0;
        VehicleFlagProjectionArithmetic = 0;
        VehicleFlagNonProjectionFatal = 0;
        VehicleFlagRecovered = 0;
        VehicleFlagPreservedFatal = 0;
        VehicleFlagNclipCorrected = 0;
    }

    static int SatIR(int n, int v, bool lm)
    {
        int min = lm ? 0 : -0x8000;
        if (v < min) { v = min; Flag(25 - n); }
        else if (v > 0x7FFF) { v = 0x7FFF; Flag(25 - n); }
        return v;
    }

    static int SatIR0(int v)
    {
        if (v < 0) { Flag(12); return 0; }
        if (v > 0x1000) { Flag(12); return 0x1000; }
        return v;
    }

    static int SatColor(int n, int v)
    {
        if (v < 0) { Flag(21 - n); return 0; }
        if (v > 0xFF) { Flag(21 - n); return 0xFF; }
        return v;
    }

    static int SatSZ(int v)
    {
        if (v < 0) { Flag(18); return 0; }
        if (v > 0xFFFF) { Flag(18); return 0xFFFF; }
        return v;
    }

    static int SatX(int v)
    {
        if (v < -0x400) { Flag(14); return -0x400; }
        if (v > 0x3FF) { Flag(14); return 0x3FF; }
        return v;
    }

    static int SatY(int v)
    {
        if (v < -0x400) { Flag(13); return -0x400; }
        if (v > 0x3FF) { Flag(13); return 0x3FF; }
        return v;
    }

    static long CheckMac0(long v)
    {
        if (v > 0x7FFFFFFFL) Flag(16);
        else if (v < -0x80000000L) Flag(15);
        return v;
    }

    static void CheckMac(int n, long v)
    {
        if (v >= (1L << 43)) Flag(31 - n);
        else if (v < -(1L << 43)) Flag(28 - n);
    }

    static void SetMac(int n, long v, int sf, bool lm)
    {
        CheckMac(n, v);
        int m = (int)(v >> sf);
        if (n == 1) { MAC1 = m; IR1 = SatIR(1, m, lm); }
        else if (n == 2) { MAC2 = m; IR2 = SatIR(2, m, lm); }
        else { MAC3 = m; IR3 = SatIR(3, m, lm); }
    }

    static void MatVec(short[] mx, int t0, int t1, int t2, int vx, int vy, int vz, int sf, bool lm)
    {
        SetMac(1, ((long)t0 << 12) + (long)mx[0] * vx + (long)mx[1] * vy + (long)mx[2] * vz, sf, lm);
        SetMac(2, ((long)t1 << 12) + (long)mx[3] * vx + (long)mx[4] * vy + (long)mx[5] * vz, sf, lm);
        SetMac(3, ((long)t2 << 12) + (long)mx[6] * vx + (long)mx[7] * vy + (long)mx[8] * vz, sf, lm);
    }
    static void PushColor()
    {
        int r = SatColor(0, MAC1 >> 4);
        int g = SatColor(1, MAC2 >> 4);
        int b = SatColor(2, MAC3 >> 4);
        RGB[0] = RGB[1]; RGB[1] = RGB[2];
        RGB[2] = (uint)(r | (g << 8) | (b << 16) | (RGBC_CODE << 24));
    }

    static void Interp(long in1, long in2, long in3, int sf, bool lm)
    {
        IR1 = SatIR(1, (int)((((long)FC[0] << 12) - in1) >> sf), false);
        IR2 = SatIR(2, (int)((((long)FC[1] << 12) - in2) >> sf), false);
        IR3 = SatIR(3, (int)((((long)FC[2] << 12) - in3) >> sf), false);
        SetMac(1, (long)IR1 * IR0 + in1, sf, lm);
        SetMac(2, (long)IR2 * IR0 + in2, sf, lm);
        SetMac(3, (long)IR3 * IR0 + in3, sf, lm);
        PushColor();
    }

    static void Modulate(int sf, bool lm)
    {
        SetMac(1, ((long)RGBC_R * IR1) << 4, sf, lm);
        SetMac(2, ((long)RGBC_G * IR2) << 4, sf, lm);
        SetMac(3, ((long)RGBC_B * IR3) << 4, sf, lm);
        PushColor();
    }

    static int Clz16(uint v)
    {
        int n = 0;
        for (int i = 15; i >= 0 && (v & (1u << i)) == 0; i--) n++;
        return n;
    }

    internal static uint Divide(uint h, uint sz3)
    {
        if (h >= sz3 * 2) { Flag(17); return 0x1FFFF; }
        int z = Clz16(sz3);
        ulong n = (ulong)h << z;
        ulong d = (ulong)sz3 << z;
        int idx = (int)((d - 0x7FC0) >> 7);
        if (idx < 0) idx = 0; else if (idx > 0x100) idx = 0x100;
        ulong u = (ulong)Unr[idx] + 0x101;
        d = (0x2000080UL - d * u) >> 8;
        d = (0x0000080UL + d * u) >> 8;
        ulong res = (n * d + 0x8000) >> 16;
        return res > 0x1FFFF ? 0x1FFFFu : (uint)res;
    }

    static void Rtp(int vx, int vy, int vz, int sf, bool lm, bool last)
    {
        long m1 = ((long)TR[0] << 12) + (long)RT[0] * vx + (long)RT[1] * vy + (long)RT[2] * vz;
        long m2 = ((long)TR[1] << 12) + (long)RT[3] * vx + (long)RT[4] * vy + (long)RT[5] * vz;
        long m3 = ((long)TR[2] << 12) + (long)RT[6] * vx + (long)RT[7] * vy + (long)RT[8] * vz;
        CheckMac(1, m1); CheckMac(2, m2); CheckMac(3, m3);
        MAC1 = (int)(m1 >> sf); MAC2 = (int)(m2 >> sf); MAC3 = (int)(m3 >> sf);
        IR1 = SatIR(1, MAC1, lm);
        IR2 = SatIR(2, MAC2, lm);
        int ir3flag = (int)(m3 >> 12);
        if (ir3flag < -0x8000 || ir3flag > 0x7FFF) Flag(22);
        IR3 = MAC3 < (lm ? 0 : -0x8000) ? (lm ? 0 : -0x8000) : MAC3 > 0x7FFF ? 0x7FFF : MAC3;

        int sz = SatSZ((int)(m3 >> 12));
        SZ[0] = SZ[1]; SZ[1] = SZ[2]; SZ[2] = SZ[3]; SZ[3] = (ushort)sz;

        uint div = Divide(H, SZ[3]);
        long sx = CheckMac0((long)div * IR1 + OFX); MAC0 = (int)sx;
        long sy = CheckMac0((long)div * IR2 + OFY); MAC0 = (int)sy;
        int nx = SatX((int)(sx >> 16));
        int ny = SatY((int)(sy >> 16));
        SX[0] = SX[1]; SX[1] = SX[2]; SX[2] = (short)nx;
        SY[0] = SY[1]; SY[1] = SY[2]; SY[2] = (short)ny;
        SxyDepth[0] = SxyDepth[1];
        SxyDepth[1] = SxyDepth[2];
        SxyDepth[2] = (ushort)sz;
        SxyDepthValid[0] = SxyDepthValid[1];
        SxyDepthValid[1] = SxyDepthValid[2];
        SxyDepthValid[2] = sz != 0;
        if (WorldCaptureContext.CaptureEnabled)
        {
            SxyOrigin[0] = SxyOrigin[1];
            SxyOrigin[1] = SxyOrigin[2];
            SxyOrigin[2] = CreateProjectionOrigin(
                vx, vy, vz,
                (int)(m1 >> 12),
                (int)(m2 >> 12),
                (int)(m3 >> 12));
        }
        if (last)
        {
            long dp = CheckMac0((long)div * DQA + DQB);
            MAC0 = (int)dp;
            IR0 = SatIR0((int)(dp >> 12));
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    static GteProjectionOriginHandle CreateProjectionOrigin(
        int modelX, int modelY, int modelZ,
        int viewX, int viewY, int viewZ)
    {
        ulong hash = CurrentProjectionTransformId();
        WorldObjectContext context = WorldCaptureContext.Current;
#if !OPENGT_RELEASE_PACKAGE
        if (
            TraceUnknownWorldStacks &&
            context.Kind == WorldObjectKind.Unknown &&
            Host.InputManager.CurrentPoll >= TraceUnknownWorldStackStartPoll
        ) {
            TraceUnknownWorldProjectionStack(
                modelX, modelY, modelZ,
                viewX, viewY, viewZ,
                hash);
        }
#endif
        long viewXFixed = ((long)TR[0] << 12) +
            (long)RT[0] * modelX +
            (long)RT[1] * modelY +
            (long)RT[2] * modelZ;
        long viewYFixed = ((long)TR[1] << 12) +
            (long)RT[3] * modelX +
            (long)RT[4] * modelY +
            (long)RT[5] * modelZ;
        long viewZFixed = ((long)TR[2] << 12) +
            (long)RT[6] * modelX +
            (long)RT[7] * modelY +
            (long)RT[8] * modelZ;
        GteProjectionOrigin origin = new(
            (short)modelX, (short)modelY, (short)modelZ,
            viewX, viewY, viewZ,
            viewXFixed, viewYFixed, viewZFixed,
            RT[0], RT[1], RT[2],
            RT[3], RT[4], RT[5],
            RT[6], RT[7], RT[8],
            TR[0], TR[1], TR[2],
            OFX, OFY, H,
            hash,
            context,
            0, 0,
            GteProjectionOriginFlags.None);
        return StoreProjectionOrigin(in origin);
    }

#if !OPENGT_RELEASE_PACKAGE
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    static void TraceUnknownWorldProjectionStack(
        int modelX,
        int modelY,
        int modelZ,
        int viewX,
        int viewY,
        int viewZ,
        ulong transformId)
    {
        int trace = Interlocked.Increment(ref TraceUnknownWorldStackCount);
        if (trace > TraceUnknownWorldStackLimit)
            return;
        Console.Error.WriteLine(
            $"[GTE-Unknown-Stack] n={trace} " +
            $"poll={Host.InputManager.CurrentPoll} " +
            $"model={modelX},{modelY},{modelZ} " +
            $"view={viewX},{viewY},{viewZ} " +
            $"transform=0x{transformId:X16}{Environment.NewLine}" +
            Environment.StackTrace);
    }
#endif

    static ulong CurrentProjectionTransformId()
    {
        ulong hash = ProjectionTransformHash;
        if (ProjectionTransformHashDirty)
        {
            hash = 14695981039346656037UL;
            static ulong Mix(ulong value, int item)
            {
                value ^= unchecked((uint)item);
                return value * 1099511628211UL;
            }
            for (int index = 0; index < RT.Length; index++)
                hash = Mix(hash, RT[index]);
            for (int index = 0; index < TR.Length; index++)
                hash = Mix(hash, TR[index]);
            if (hash == 0)
                hash = 1;
            ProjectionTransformHash = hash;
            ProjectionTransformHashDirty = false;
        }
        return hash;
    }

    /// <summary>
    /// Captures the exact current object-to-view transform for a raw model
    /// vertex without executing a GTE command or modifying any GTE register.
    /// The modern track decoder uses this before the guest's projection and
    /// screen-space rejection stages.
    /// </summary>
    public static GteProjectionOrigin SnapshotProjectionOrigin(
        short modelX,
        short modelY,
        short modelZ)
    {
        long viewX = ((long)TR[0] << 12) +
            (long)RT[0] * modelX +
            (long)RT[1] * modelY +
            (long)RT[2] * modelZ;
        long viewY = ((long)TR[1] << 12) +
            (long)RT[3] * modelX +
            (long)RT[4] * modelY +
            (long)RT[5] * modelZ;
        long viewZ = ((long)TR[2] << 12) +
            (long)RT[6] * modelX +
            (long)RT[7] * modelY +
            (long)RT[8] * modelZ;
        return new GteProjectionOrigin(
            modelX, modelY, modelZ,
            (int)(viewX >> 12),
            (int)(viewY >> 12),
            (int)(viewZ >> 12),
            viewX, viewY, viewZ,
            RT[0], RT[1], RT[2],
            RT[3], RT[4], RT[5],
            RT[6], RT[7], RT[8],
            TR[0], TR[1], TR[2],
            OFX, OFY, H,
            CurrentProjectionTransformId(),
            WorldCaptureContext.Current,
            0, 0,
            GteProjectionOriginFlags.None);
    }

    public static void BeginDerivedScreenProjection(
        uint packedXy,
        float screenOffsetScale = 1.0f)
    {
        GteProjectionOrigin origin = default;
        GteProjectedValue projected = WorldCaptureContext.CaptureEnabled
            ? ConsumeCpuRegisterWrite(packedXy, out origin)
            : ConsumeCpuRegisterWrite(packedXy);
        if (TraceDerivedScreenProjection &&
            DerivedScreenBeginTraceCount++ < 48)
            Console.Error.WriteLine(
                $"[GTE-Derived] begin packed=0x{packedXy:X8} " +
                $"scale={screenOffsetScale:F3} " +
                $"capture={WorldCaptureContext.CaptureEnabled} " +
                $"projected={projected.Valid} origin={origin.Valid} " +
                $"plane={origin.ProjectionPlane} " +
                $"object={origin.Object.Kind} " +
                $"model=0x{origin.Object.ModelPointer:X8}");
        if (!projected.Valid || !origin.Valid || origin.ProjectionPlane == 0)
        {
            DerivedScreenValue = default;
            DerivedScreenOrigin = default;
            DerivedScreenOffsetScale = 1.0f;
            return;
        }
        DerivedScreenValue = projected;
        DerivedScreenOrigin = origin;
        DerivedScreenOffsetScale = Math.Clamp(screenOffsetScale, 0.0f, 1.0f);
    }

    public static void BeginDerivedScreenProjection(
        bool scaleImportedOffsets,
        uint packedXy) =>
        BeginDerivedScreenProjection(
            packedXy,
            scaleImportedOffsets ? 0.25f : 1.0f);

    public static void EndDerivedScreenProjection()
    {
        DerivedScreenValue = default;
        DerivedScreenOrigin = default;
        DerivedScreenOffsetScale = 1.0f;
    }

    static GteProjectionOrigin DeriveScreenOffsetOrigin(uint packedXy)
    {
        int x = (short)packedXy;
        int y = (short)(packedXy >> 16);
        int anchorX = (short)DerivedScreenValue.PackedXy;
        int anchorY = (short)(DerivedScreenValue.PackedXy >> 16);
        int authoredOffsetX = x - anchorX;
        int authoredOffsetY = y - anchorY;
        double offsetX = authoredOffsetX * DerivedScreenOffsetScale;
        double offsetY = authoredOffsetY * DerivedScreenOffsetScale;
        if (TraceDerivedScreenProjection &&
            DerivedScreenOffsetTraceCount++ < 96)
            Console.Error.WriteLine(
                $"[GTE-Derived] offset packed=0x{packedXy:X8} " +
                $"anchor=0x{DerivedScreenValue.PackedXy:X8} " +
                $"authored={authoredOffsetX},{authoredOffsetY} " +
                $"scaled={offsetX:F3},{offsetY:F3} " +
                $"z={DerivedScreenValue.Z}");
        double viewUnitsPerPixel =
            (double)DerivedScreenOrigin.ViewZ /
            DerivedScreenOrigin.ProjectionPlane;
        int viewX = DerivedScreenOrigin.ViewX +
            (int)Math.Round(offsetX * viewUnitsPerPixel);
        int viewY = DerivedScreenOrigin.ViewY +
            (int)Math.Round(offsetY * viewUnitsPerPixel);
        return DerivedScreenOrigin with
        {
            ViewX = viewX,
            ViewY = viewY,
            ScreenOffsetX = (short)Math.Clamp(
                (int)Math.Round(offsetX), short.MinValue, short.MaxValue),
            ScreenOffsetY = (short)Math.Clamp(
                (int)Math.Round(offsetY), short.MinValue, short.MaxValue),
            Flags = DerivedScreenOrigin.Flags |
                GteProjectionOriginFlags.ScreenOffsetAnchor,
        };
    }

    static uint ReadProjectedScreen(int screenIndex)
    {
        uint packed = (uint)((ushort)SX[screenIndex] | (SY[screenIndex] << 16));
        if (!ProjectionTrackingEnabled)
        {
            PendingCpuValue = default;
            ClearPendingCpuOrigin();
            return packed;
        }
        ushort depth = SxyDepth[screenIndex];
        if (SxyDepthValid[screenIndex] && depth != 0)
        {
            PendingCpuValue = new GteProjectedValue(
                packed,
                depth,
                ScreenDepthGeneration,
                GteDepthProvenance.CpuRegisterFlow);
            if (WorldCaptureContext.CaptureEnabled)
                PendingCpuOrigin = SxyOrigin[screenIndex];
        }
        else
        {
            PendingCpuValue = default;
            ClearPendingCpuOrigin();
        }
        return packed;
    }

    public static void NotifyCpuRegisterRead(
        uint value,
        in GteProjectedValue projected)
    {
        if (!ProjectionTrackingEnabled)
        {
            PendingCpuValue = default;
            ClearPendingCpuOrigin();
            return;
        }
        bool accepted =
            projected.Valid &&
            projected.PackedXy == value &&
            ScreenDepthGeneration - projected.Generation <=
                PacketDepthMaxAge;
        PendingCpuValue = accepted
            ? projected with
            {
                Provenance = GteDepthProvenance.CpuRegisterFlow,
            }
            : default;
        ClearPendingCpuOrigin();
    }

    internal static void NotifyCpuRegisterRead(
        uint value,
        in GteProjectedValue projected,
        in GteProjectionOriginHandle origin)
    {
        NotifyCpuRegisterRead(value, in projected);
        if (PendingCpuValue.Valid)
            PendingCpuOrigin = origin;
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static GteProjectedValue ConsumeCpuRegisterWrite(uint value)
    {
        if (!ProjectionTrackingEnabled)
        {
            PendingCpuValue = default;
            ClearPendingCpuOrigin();
            return default;
        }
        GteProjectedValue projected = PendingCpuValue;
        PendingCpuValue = default;
        ClearPendingCpuOrigin();
        return projected.Valid && projected.PackedXy == value
            ? projected with
            {
                Provenance = GteDepthProvenance.CpuRegisterFlow,
            }
            : default;
    }

    public static GteProjectedValue ConsumeCpuRegisterWrite(
        uint value,
        out GteProjectionOrigin origin)
    {
        GteProjectionOriginHandle projectedOrigin = PendingCpuOrigin;
        GteProjectedValue projected = ConsumeCpuRegisterWrite(value);
        if (!projected.Valid ||
            !WorldCaptureContext.CaptureEnabled ||
            !TryResolveProjectionOrigin(in projectedOrigin, out origin))
        {
            origin = default;
        }
        return projected;
    }

    internal static GteProjectedValue ConsumeCpuRegisterWrite(
        uint value,
        out GteProjectionOriginHandle origin)
    {
        GteProjectionOriginHandle projectedOrigin = PendingCpuOrigin;
        GteProjectedValue projected = ConsumeCpuRegisterWrite(value);
        origin = projected.Valid && WorldCaptureContext.CaptureEnabled
            ? projectedOrigin
            : default;
        return projected;
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining |
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    public static void NotifyRamRead(uint wordAddress, uint value)
    {
        if (!ProjectionTrackingEnabled)
        {
            PendingCpuValue = default;
            ClearPendingCpuOrigin();
            return;
        }
        wordAddress &= ~3u;
        if (!IsPacketDepthBound(wordAddress))
        {
            PendingCpuValue = default;
            ClearPendingCpuOrigin();
            return;
        }
        NotifyBoundRamRead(wordAddress, value);
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining |
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    static void NotifyBoundRamRead(uint wordAddress, uint value)
    {
        if (TryGetPacketDepthSample(wordAddress, out var sample) &&
            sample.PackedXy == value &&
            ScreenDepthGeneration - sample.Generation <=
                PacketDepthMaxAge)
        {
            PendingCpuValue = new GteProjectedValue(
                value,
                sample.Z,
                sample.Generation,
                GteDepthProvenance.CpuRegisterFlow);
            ClearPendingCpuOrigin();
            if (WorldCaptureContext.CaptureEnabled &&
                TryGetPacketOriginSample(wordAddress, out var origin) &&
                origin.PackedXy == value &&
                ScreenDepthGeneration - origin.Generation <=
                    PacketDepthMaxAge)
            {
                GteProjectionOrigin packetOrigin = origin.Origin;
                PendingCpuOrigin = StoreProjectionOrigin(in packetOrigin);
            }
            return;
        }
        PendingCpuValue = default;
        ClearPendingCpuOrigin();
    }

    /// <summary>
    /// Called after a RAM word changes. If the word is an SXY value recently
    /// read from the GTE, bind its exact packet address to the matching depth.
    /// Any older binding is removed first so reused primitive buffers cannot
    /// retain stale depth.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining |
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    public static void NotifyRamWrite(uint wordAddress, uint value)
    {
        if (!ProjectionTrackingEnabled)
        {
            PendingDirectStore = null;
            ClearPendingDirectOrigin();
            PendingCpuValue = default;
            ClearPendingCpuOrigin();
            return;
        }
        // The overwhelming majority of guest RAM writes neither overwrite a
        // bound packet word nor carry a freshly projected SXY value.  Avoid
        // dictionary and metadata plumbing on that common path while keeping
        // the exact next-write consumption semantics for real projections.
        uint alignedAddress = wordAddress & ~3u;
        if (TracePacketWrites)
            TraceRamWrite(alignedAddress, value);
        bool derivedScreenProjection =
            DerivedScreenValue.Valid &&
            DerivedScreenOrigin.Valid &&
            ScreenDepthGeneration - DerivedScreenValue.Generation <=
                PacketDepthMaxAge;
        if (
            PendingDirectStore == null &&
            !PendingCpuValue.Valid &&
            !derivedScreenProjection &&
            !IsPacketDepthBound(alignedAddress)
        ) {
            return;
        }
        NotifyTrackedRamWrite(
            alignedAddress, value, derivedScreenProjection);
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    static void TraceRamWrite(uint alignedAddress, uint value)
    {
        int poll = Host.InputManager.CurrentPoll;
        if (poll < TracePacketWritesStartPoll ||
            poll > TracePacketWritesEndPoll ||
            alignedAddress < TracePacketWritesMinimumAddress ||
            alignedAddress >= TracePacketWritesMaximumAddress)
            return;

        int x = (short)value;
        int y = (short)(value >> 16);
        if (x is < -1024 or > 1023 || y is < -512 or > 511)
            return;

        int trace = Interlocked.Increment(ref PacketWriteTraceCount);
        if (trace > 4096)
            return;

        Context.CpuContext? cpu = Runtime.Cpu;
        Console.Error.WriteLine(
            $"[GTE-PACKET-WRITE] n={trace} " +
            $"poll={poll} " +
            $"address=0x{alignedAddress:X8} xy={x},{y} " +
            $"value=0x{value:X8} generation={ScreenDepthGeneration} " +
            $"pendingDirect={PendingDirectStore != null} " +
            $"pendingCpu={PendingCpuValue.Valid} " +
            $"ra=0x{cpu?.PeekRaw(31) ?? 0u:X8} " +
            $"s0=0x{cpu?.PeekRaw(16) ?? 0u:X8} " +
            $"s1=0x{cpu?.PeekRaw(17) ?? 0u:X8} " +
            $"s2=0x{cpu?.PeekRaw(18) ?? 0u:X8} " +
            $"t0=0x{cpu?.PeekRaw(8) ?? 0u:X8} " +
            $"t1=0x{cpu?.PeekRaw(9) ?? 0u:X8} " +
            $"t2=0x{cpu?.PeekRaw(10) ?? 0u:X8} " +
            $"t3=0x{cpu?.PeekRaw(11) ?? 0u:X8}");
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining |
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    static void NotifyTrackedRamWrite(
        uint wordAddress,
        uint value,
        bool derivedScreenProjection)
    {
        if (PendingDirectStore is { } direct)
        {
            PendingDirectStore = null;
            PendingCpuValue = default;
            ClearPendingCpuOrigin();
            if (direct.PackedXy == value &&
                direct.Generation == ScreenDepthGeneration)
            {
                SetPacketDepth(wordAddress, direct);
                if (WorldCaptureContext.CaptureEnabled)
                {
                    if (PendingDirectOrigin.Valid &&
                        TryResolveProjectionOrigin(
                            in PendingDirectOrigin,
                            out GteProjectionOrigin origin))
                    {
                        SetPacketOrigin(wordAddress, new PacketOriginSample(
                            value,
                            direct.Z,
                            direct.Generation,
                            direct.Provenance,
                            origin));
                    }
                    else
                    {
                        RemovePacketOrigin(wordAddress);
                    }
                }
                ClearPendingDirectOrigin();
                return;
            }
            ClearPendingDirectOrigin();
        }

        GteProjectionOriginHandle cpuOrigin = default;
        GteProjectedValue cpuValue = WorldCaptureContext.CaptureEnabled
            ? ConsumeCpuRegisterWrite(value, out cpuOrigin)
            : ConsumeCpuRegisterWrite(value);
        if (cpuValue.Valid &&
            ScreenDepthGeneration - cpuValue.Generation <=
                PacketDepthMaxAge)
        {
            SetPacketDepth(wordAddress, new PacketDepthSample(
                value,
                cpuValue.Z,
                ScreenDepthGeneration,
                GteDepthProvenance.CpuRegisterFlow));
            if (WorldCaptureContext.CaptureEnabled)
            {
                if (cpuOrigin.Valid &&
                    TryResolveProjectionOrigin(
                        in cpuOrigin,
                        out GteProjectionOrigin origin))
                {
                    SetPacketOrigin(wordAddress, new PacketOriginSample(
                        value,
                        cpuValue.Z,
                        ScreenDepthGeneration,
                        GteDepthProvenance.CpuRegisterFlow,
                        origin));
                }
                else
                {
                    RemovePacketOrigin(wordAddress);
                }
            }
            return;
        }
        if (derivedScreenProjection)
        {
            int x = (short)value;
            int y = (short)(value >> 16);
            if (x is >= -1024 and <= 1023 && y is >= -512 and <= 511)
            {
                SetPacketDepth(wordAddress, new PacketDepthSample(
                    value,
                    DerivedScreenValue.Z,
                    ScreenDepthGeneration,
                    GteDepthProvenance.CpuRegisterFlow));
                SetPacketOrigin(wordAddress, new PacketOriginSample(
                    value,
                    DerivedScreenValue.Z,
                    ScreenDepthGeneration,
                    GteDepthProvenance.CpuRegisterFlow,
                    DeriveScreenOffsetOrigin(value)));
                return;
            }
        }
        RemovePacketDepth(wordAddress);
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    static void SetPacketDepth(uint wordAddress, PacketDepthSample sample)
    {
        PacketDepthSample[] storage;
        uint[] boundBits;
        uint word;
        if (wordAddress < Memory.MemoryMap.DevkitRamSize)
        {
            storage = PacketDepthRam;
            boundBits = PacketDepthRamBits;
            word = wordAddress >> 2;
        }
        else if (wordAddress >= Memory.MemoryMap.ScratchpadBase &&
            wordAddress <
                Memory.MemoryMap.ScratchpadBase +
                Memory.MemoryMap.ScratchpadSize)
        {
            storage = PacketDepthScratch;
            boundBits = PacketDepthScratchBits;
            word =
                (wordAddress - Memory.MemoryMap.ScratchpadBase) >> 2;
        }
        else
        {
            return;
        }

        uint mask = 1u << (int)(word & 31u);
        ref uint bits = ref boundBits[word >> 5];
        if ((bits & mask) == 0)
            PacketDepthCount++;
        storage[word] = sample;
        bits |= mask;
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    static void RemovePacketDepth(uint wordAddress)
    {
        if (!IsPacketDepthBound(wordAddress))
            return;
        if (TryGetPacketDepthStorage(
                wordAddress, out PacketDepthSample[] storage, out int index))
            storage[index] = default;
        if (WorldCaptureContext.CaptureEnabled)
            RemovePacketOrigin(wordAddress);
        SetPacketDepthBound(wordAddress, false);
        PacketDepthCount--;
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining |
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    static bool TryGetPacketDepthStorage(
        uint wordAddress,
        out PacketDepthSample[] storage,
        out int index)
    {
        if (wordAddress < Memory.MemoryMap.DevkitRamSize)
        {
            storage = PacketDepthRam;
            index = (int)(wordAddress >> 2);
            return true;
        }
        if (wordAddress >= Memory.MemoryMap.ScratchpadBase &&
            wordAddress <
                Memory.MemoryMap.ScratchpadBase +
                Memory.MemoryMap.ScratchpadSize)
        {
            storage = PacketDepthScratch;
            index = (int)((wordAddress - Memory.MemoryMap.ScratchpadBase) >> 2);
            return true;
        }
        storage = PacketDepthRam;
        index = 0;
        return false;
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining |
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    static bool TryGetPacketOriginSlotStorage(
        uint wordAddress,
        out int[] storage,
        out int index)
    {
        if (wordAddress < Memory.MemoryMap.DevkitRamSize)
        {
            storage = PacketOriginRamSlots;
            index = (int)(wordAddress >> 2);
            return true;
        }
        if (wordAddress >= Memory.MemoryMap.ScratchpadBase &&
            wordAddress <
                Memory.MemoryMap.ScratchpadBase +
                Memory.MemoryMap.ScratchpadSize)
        {
            storage = PacketOriginScratchSlots;
            index = (int)((wordAddress - Memory.MemoryMap.ScratchpadBase) >> 2);
            return true;
        }
        storage = PacketOriginRamSlots;
        index = 0;
        return false;
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    static void SetPacketOrigin(uint wordAddress, PacketOriginSample sample)
    {
        if (!TryGetPacketOriginSlotStorage(
                wordAddress, out int[] storage, out int index))
            return;
        int encodedSlot = storage[index];
        if (encodedSlot != 0)
        {
            PacketOriginSamples[encodedSlot - 1] = sample;
            return;
        }
        int slot;
        if (FreePacketOriginSlots.TryPop(out int reusable))
        {
            slot = reusable;
            PacketOriginSamples[slot] = sample;
        }
        else
        {
            slot = PacketOriginSamples.Count;
            PacketOriginSamples.Add(sample);
        }
        storage[index] = checked(slot + 1);
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    static void RemovePacketOrigin(uint wordAddress)
    {
        if (!TryGetPacketOriginSlotStorage(
                wordAddress, out int[] storage, out int index))
            return;
        int encodedSlot = storage[index];
        if (encodedSlot == 0)
            return;
        storage[index] = 0;
        int slot = encodedSlot - 1;
        PacketOriginSamples[slot] = default;
        FreePacketOriginSlots.Push(slot);
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining |
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    static bool TryGetPacketOriginSample(
        uint wordAddress,
        out PacketOriginSample sample)
    {
        if (TryGetPacketOriginSlotStorage(
                wordAddress, out int[] storage, out int index))
        {
            int encodedSlot = storage[index];
            if (encodedSlot != 0)
            {
                sample = PacketOriginSamples[encodedSlot - 1];
                return true;
            }
        }
        sample = default;
        return false;
    }

    static void ClearPacketOrigins()
    {
        Array.Clear(PacketOriginRamSlots);
        Array.Clear(PacketOriginScratchSlots);
        PacketOriginSamples.Clear();
        FreePacketOriginSlots.Clear();
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining |
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    static bool TryGetPacketDepthSample(
        uint wordAddress,
        out PacketDepthSample sample)
    {
        if (IsPacketDepthBound(wordAddress) &&
            TryGetPacketDepthStorage(
                wordAddress, out PacketDepthSample[] storage, out int index))
        {
            sample = storage[index];
            return true;
        }
        sample = default;
        return false;
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining |
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    static bool IsPacketDepthBound(uint wordAddress)
    {
        if (wordAddress < Memory.MemoryMap.DevkitRamSize)
        {
            uint word = wordAddress >> 2;
            return (PacketDepthRamBits[word >> 5] &
                    (1u << (int)(word & 31u))) != 0;
        }
        if (wordAddress >= Memory.MemoryMap.ScratchpadBase &&
            wordAddress <
                Memory.MemoryMap.ScratchpadBase +
                Memory.MemoryMap.ScratchpadSize)
        {
            uint word =
                (wordAddress - Memory.MemoryMap.ScratchpadBase) >> 2;
            return (PacketDepthScratchBits[word >> 5] &
                    (1u << (int)(word & 31u))) != 0;
        }
        return false;
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining |
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    static void SetPacketDepthBound(uint wordAddress, bool bound)
    {
        uint[] bits;
        uint word;
        if (wordAddress < Memory.MemoryMap.DevkitRamSize)
        {
            bits = PacketDepthRamBits;
            word = wordAddress >> 2;
        }
        else if (wordAddress >= Memory.MemoryMap.ScratchpadBase &&
                 wordAddress <
                     Memory.MemoryMap.ScratchpadBase +
                     Memory.MemoryMap.ScratchpadSize)
        {
            bits = PacketDepthScratchBits;
            word = (wordAddress - Memory.MemoryMap.ScratchpadBase) >> 2;
        }
        else
        {
            return;
        }

        uint mask = 1u << (int)(word & 31u);
        if (bound)
            bits[word >> 5] |= mask;
        else
            bits[word >> 5] &= ~mask;
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    public static bool TryGetPacketDepth(
        uint wordAddress, int x, int y, out ushort z, out int age,
        out GteDepthProvenance provenance)
    {
        uint packed = (uint)((ushort)(short)x | ((uint)(ushort)(short)y << 16));
        PacketDepthSample sample = default;
        bool found =
            wordAddress != uint.MaxValue &&
            TryGetPacketDepthSample(wordAddress & ~3u, out sample) &&
            sample.PackedXy == packed &&
            ScreenDepthGeneration - sample.Generation <= PacketDepthMaxAge;
        z = found ? sample.Z : (ushort)0;
        age = found
            ? Math.Max(0, ScreenDepthGeneration - sample.Generation)
            : 0;
        provenance = found ? sample.Provenance : GteDepthProvenance.None;
        if (TraceScreenDepth)
        {
            if (found) PacketDepthHits++;
            else PacketDepthMisses++;
        }
        return found;
    }

    /// <summary>
    /// Resolves the depth and world origin carried by one GPU packet word.
    /// The combined query shares address normalization and coordinate packing
    /// for the two parallel metadata stores used by live polygon capture.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining |
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    public static bool TryGetPacketProjection(
        uint wordAddress,
        int x,
        int y,
        out ushort z,
        out int age,
        out GteDepthProvenance provenance,
        out GteProjectionOrigin origin)
    {
        uint packed =
            (uint)((ushort)(short)x | ((uint)(ushort)(short)y << 16));
        PacketOriginSample originSample = default;
        bool hasOriginSample = false;
        int addressRegion = 0;
        int word = 0;
        if (wordAddress != uint.MaxValue)
        {
            uint alignedAddress = wordAddress & ~3u;
            if (alignedAddress < Memory.MemoryMap.DevkitRamSize)
            {
                addressRegion = 1;
                word = (int)(alignedAddress >> 2);
                int encodedOrigin = PacketOriginRamSlots[word];
                if (encodedOrigin != 0)
                {
                    originSample = PacketOriginSamples[encodedOrigin - 1];
                    hasOriginSample = true;
                }
            }
            else if (alignedAddress >= Memory.MemoryMap.ScratchpadBase &&
                alignedAddress <
                    Memory.MemoryMap.ScratchpadBase +
                    Memory.MemoryMap.ScratchpadSize)
            {
                addressRegion = 2;
                word = (int)((alignedAddress -
                    Memory.MemoryMap.ScratchpadBase) >> 2);
                int encodedOrigin = PacketOriginScratchSlots[word];
                if (encodedOrigin != 0)
                {
                    originSample = PacketOriginSamples[encodedOrigin - 1];
                    hasOriginSample = true;
                }
            }
        }
        bool foundOrigin =
            hasOriginSample &&
            originSample.PackedXy == packed &&
            ScreenDepthGeneration - originSample.Generation <=
                PacketDepthMaxAge &&
            originSample.Origin.Valid;

        // World-capture origins are written together with the exact depth and
        // provenance for the packet. Prefer that compact slot so the common
        // path does not also touch the 32 MiB direct-indexed depth store. A
        // packet without an origin can still use the depth-only fallback for
        // perspective correction and diagnostics.
        PacketDepthSample depthSample = default;
        bool hasDepthSample = false;
        if (!foundOrigin)
        {
            if (addressRegion == 1)
            {
                hasDepthSample =
                    (PacketDepthRamBits[word >> 5] &
                        (1u << (word & 31))) != 0;
                if (hasDepthSample)
                    depthSample = PacketDepthRam[word];
            }
            else if (addressRegion == 2)
            {
                hasDepthSample =
                    (PacketDepthScratchBits[word >> 5] &
                        (1u << (word & 31))) != 0;
                if (hasDepthSample)
                    depthSample = PacketDepthScratch[word];
            }
        }
        bool foundDepthSample =
            hasDepthSample &&
            depthSample.PackedXy == packed &&
            ScreenDepthGeneration - depthSample.Generation <=
                PacketDepthMaxAge;
        bool foundDepth = foundOrigin || foundDepthSample;
        z = foundOrigin
            ? originSample.Z
            : foundDepthSample
                ? depthSample.Z
                : (ushort)0;
        int generation = foundOrigin
            ? originSample.Generation
            : depthSample.Generation;
        age = foundDepth
            ? Math.Max(0, ScreenDepthGeneration - generation)
            : 0;
        provenance = foundOrigin
            ? originSample.Provenance
            : foundDepthSample
                ? depthSample.Provenance
                : GteDepthProvenance.None;
        if (TraceScreenDepth)
        {
            if (foundDepth) PacketDepthHits++;
            else PacketDepthMisses++;
        }

        origin = foundOrigin ? originSample.Origin : default;
        return foundDepth;
    }

    public static bool TryGetPacketOrigin(
        uint wordAddress,
        int x,
        int y,
        out GteProjectionOrigin origin)
    {
        PacketOriginSample sample = default;
        bool found =
            wordAddress != uint.MaxValue &&
            TryGetPacketOriginSample(wordAddress & ~3u, out sample) &&
            PacketOriginCoordinatesMatch(sample.PackedXy, x, y) &&
            ScreenDepthGeneration - sample.Generation <= PacketDepthMaxAge &&
            sample.Origin.Valid;
        origin = found ? sample.Origin : default;
        return found;
    }

    internal static bool PacketOriginCoordinatesMatch(
        uint storedPackedXy,
        int x,
        int y) =>
        storedPackedXy ==
            (uint)((ushort)(short)x | ((uint)(ushort)(short)y << 16));

    public static void BeginScreenDepthFrame()
    {
        if (TraceScreenDepth &&
            (PacketDepthHits != 0 || PacketDepthMisses != 0))
        {
            Console.Error.WriteLine(
                $"[GTE] projection-depth bindings={PacketDepthCount} " +
                $"packetHits={PacketDepthHits} packetMisses={PacketDepthMisses} " +
                $"generation={ScreenDepthGeneration}");
        }
        ScreenDepthGeneration++;
        PendingCpuValue = default;
        ClearPendingCpuOrigin();
        EndDerivedScreenProjection();
        if ((ScreenDepthGeneration & 127) == 0 && PacketDepthCount != 0)
        {
            PruneStalePacketDepth(
                PacketDepthRam, PacketDepthRamBits, baseAddress: 0);
            PruneStalePacketDepth(
                PacketDepthScratch,
                PacketDepthScratchBits,
                Memory.MemoryMap.ScratchpadBase);
        }
        PacketDepthHits = 0;
        PacketDepthMisses = 0;
    }

    public static void SetProjectionTrackingEnabled(bool enabled)
    {
        if (ProjectionTrackingEnabled == enabled)
            return;
        ProjectionTrackingEnabled = enabled;
        PendingDirectStore = null;
        ClearPendingDirectOrigin();
        PendingCpuValue = default;
        ClearPendingCpuOrigin();
        PacketDepthCount = 0;
        if (WorldCaptureContext.CaptureEnabled)
            ClearPacketOrigins();
        Array.Clear(PacketDepthRamBits);
        Array.Clear(PacketDepthScratchBits);
        Runtime.Cpu?.ClearProjectionMetadata();
    }

    static void PruneStalePacketDepth(
        PacketDepthSample[] samples,
        uint[] boundBits,
        uint baseAddress)
    {
        int oldestGeneration =
            ScreenDepthGeneration - PacketDepthMaxAge;
        for (int bitWordIndex = 0;
             bitWordIndex < boundBits.Length;
             bitWordIndex++)
        {
            uint pending = boundBits[bitWordIndex];
            while (pending != 0)
            {
                int bit = System.Numerics.BitOperations.TrailingZeroCount(
                    pending);
                int sampleIndex = bitWordIndex * 32 + bit;
                uint mask = 1u << bit;
                pending &= pending - 1;
                if (sampleIndex >= samples.Length ||
                    samples[sampleIndex].Generation >= oldestGeneration)
                    continue;

                samples[sampleIndex] = default;
                boundBits[bitWordIndex] &= ~mask;
                PacketDepthCount--;
                if (WorldCaptureContext.CaptureEnabled)
                {
                    uint address = baseAddress + (uint)(sampleIndex * 4);
                    RemovePacketOrigin(address);
                }
            }
        }
    }

    public static void Execute(uint cmd)
    {
        FLAG = 0;
        uint operation = cmd & 0x3Fu;
        LastOperation = operation;
        int sf = (cmd & (1u << 19)) != 0 ? 12 : 0;
        bool lm = (cmd & (1u << 10)) != 0;
        int mx = (int)((cmd >> 17) & 3);
        int vn = (int)((cmd >> 15) & 3);
        int cv = (int)((cmd >> 13) & 3);

        switch (operation)
        {
            case 0x01: Rtp(V[0], V[1], V[2], sf, lm, true); break;
            case 0x30:
                Rtp(V[0], V[1], V[2], sf, lm, false);
                Rtp(V[3], V[4], V[5], sf, lm, false);
                Rtp(V[6], V[7], V[8], sf, lm, true);
                break;
            case 0x06:
                if (!TryExactVehicleNclip(out MAC0))
                    MAC0 = (int)CheckMac0((long)SX[0] * (SY[1] - SY[2]) + (long)SX[1] * (SY[2] - SY[0]) + (long)SX[2] * (SY[0] - SY[1]));
                break;
            case 0x2D:
                MAC0 = (int)CheckMac0((long)ZSF3 * (SZ[1] + SZ[2] + SZ[3]));
                OTZ = (ushort)SatSZ(MAC0 >> 12);
                break;
            case 0x2E:
                MAC0 = (int)CheckMac0((long)ZSF4 * (SZ[0] + SZ[1] + SZ[2] + SZ[3]));
                OTZ = (ushort)SatSZ(MAC0 >> 12);
                break;
            case 0x12: Mvmva(sf, lm, mx, vn, cv); break;
            case 0x28:
                SetMac(1, (long)IR1 * IR1, sf, lm);
                SetMac(2, (long)IR2 * IR2, sf, lm);
                SetMac(3, (long)IR3 * IR3, sf, lm);
                break;
            case 0x0C:
                // OP reads all three source IR registers before writing any
                // result. SetMac also updates IRn, so retain the original
                // vector or MAC2/MAC3 incorrectly consume earlier results.
                int opIr1 = IR1, opIr2 = IR2, opIr3 = IR3;
                long opMac1 = (long)RT[4] * opIr3 - (long)RT[8] * opIr2;
                long opMac2 = (long)RT[8] * opIr1 - (long)RT[0] * opIr3;
                long opMac3 = (long)RT[0] * opIr2 - (long)RT[4] * opIr1;
                SetMac(1, opMac1, sf, lm);
                SetMac(2, opMac2, sf, lm);
                SetMac(3, opMac3, sf, lm);
                break;
            case 0x3D:
                SetMac(1, (long)IR0 * IR1, sf, lm);
                SetMac(2, (long)IR0 * IR2, sf, lm);
                SetMac(3, (long)IR0 * IR3, sf, lm);
                PushColor();
                break;
            case 0x3E:
                SetMac(1, ((long)MAC1 << sf) + (long)IR0 * IR1, sf, lm);
                SetMac(2, ((long)MAC2 << sf) + (long)IR0 * IR2, sf, lm);
                SetMac(3, ((long)MAC3 << sf) + (long)IR0 * IR3, sf, lm);
                PushColor();
                break;
            case 0x10: Interp((long)RGBC_R << 16, (long)RGBC_G << 16, (long)RGBC_B << 16, sf, lm); break;
            case 0x2A:
                for (int i = 0; i < 3; i++)
                    Interp((long)(RGB[0] & 0xFF) << 16, (long)((RGB[0] >> 8) & 0xFF) << 16, (long)((RGB[0] >> 16) & 0xFF) << 16, sf, lm);
                break;
            case 0x11: Interp((long)IR1 << 12, (long)IR2 << 12, (long)IR3 << 12, sf, lm); break;
            case 0x29: Interp(((long)RGBC_R * IR1) << 4, ((long)RGBC_G * IR2) << 4, ((long)RGBC_B * IR3) << 4, sf, lm); break;
            case 0x1E: Ncs(0, sf, lm); break;
            case 0x20: Ncs(0, sf, lm); Ncs(1, sf, lm); Ncs(2, sf, lm); break;
            case 0x13: Ncds(0, sf, lm); break;
            case 0x16: Ncds(0, sf, lm); Ncds(1, sf, lm); Ncds(2, sf, lm); break;
            case 0x1B: Nccs(0, sf, lm); break;
            case 0x3F: Nccs(0, sf, lm); Nccs(1, sf, lm); Nccs(2, sf, lm); break;
            case 0x1C:
                MatVec(LCM, BK[0], BK[1], BK[2], IR1, IR2, IR3, sf, lm);
                Modulate(sf, lm);
                break;
            case 0x14:
                MatVec(LCM, BK[0], BK[1], BK[2], IR1, IR2, IR3, sf, lm);
                Interp(((long)RGBC_R * IR1) << 4, ((long)RGBC_G * IR2) << 4, ((long)RGBC_B * IR3) << 4, sf, lm);
                break;
        }

        if ((FLAG & 0x7F87E000u) != 0) FLAG |= 0x80000000u;
        if (operation is 0x01u or 0x30u)
        {
            VehicleProjectionClipLimited =
                WorldCaptureContext.CaptureEnabled &&
                WorldCaptureContext.Current.Kind == WorldObjectKind.Vehicle &&
                (FLAG & 0x7F87E000u) != 0;
        }
        else if (operation != 0x06u)
        {
            VehicleProjectionClipLimited = false;
        }
    }

    static void Ncs(int vec, int sf, bool lm)
    {
        MatVec(LLM, 0, 0, 0, V[vec * 3], V[vec * 3 + 1], V[vec * 3 + 2], sf, lm);
        MatVec(LCM, BK[0], BK[1], BK[2], IR1, IR2, IR3, sf, lm);
        PushColor();
    }

    static void Ncds(int vec, int sf, bool lm)
    {
        MatVec(LLM, 0, 0, 0, V[vec * 3], V[vec * 3 + 1], V[vec * 3 + 2], sf, lm);
        MatVec(LCM, BK[0], BK[1], BK[2], IR1, IR2, IR3, sf, lm);
        Interp(((long)RGBC_R * IR1) << 4, ((long)RGBC_G * IR2) << 4, ((long)RGBC_B * IR3) << 4, sf, lm);
    }

    static void Nccs(int vec, int sf, bool lm)
    {
        MatVec(LLM, 0, 0, 0, V[vec * 3], V[vec * 3 + 1], V[vec * 3 + 2], sf, lm);
        MatVec(LCM, BK[0], BK[1], BK[2], IR1, IR2, IR3, sf, lm);
        Modulate(sf, lm);
    }

    static void Mvmva(int sf, bool lm, int mx, int vn, int cv)
    {
        short[] mat = mx == 0 ? RT : mx == 1 ? LLM : mx == 2 ? LCM : RT;
        int vx, vy, vz;
        if (vn < 3) { vx = V[vn * 3]; vy = V[vn * 3 + 1]; vz = V[vn * 3 + 2]; }
        else { vx = IR1; vy = IR2; vz = IR3; }

        if (cv == 2)
        {
            SatIR(1, (int)((((long)FC[0] << 12) + (long)mat[0] * vx) >> sf), lm);
            SatIR(2, (int)((((long)FC[1] << 12) + (long)mat[3] * vx) >> sf), lm);
            SatIR(3, (int)((((long)FC[2] << 12) + (long)mat[6] * vx) >> sf), lm);
            SetMac(1, (long)mat[1] * vy + (long)mat[2] * vz, sf, lm);
            SetMac(2, (long)mat[4] * vy + (long)mat[5] * vz, sf, lm);
            SetMac(3, (long)mat[7] * vy + (long)mat[8] * vz, sf, lm);
            return;
        }

        int t0 = 0, t1 = 0, t2 = 0;
        if (cv == 0) { t0 = TR[0]; t1 = TR[1]; t2 = TR[2]; }
        else if (cv == 1) { t0 = BK[0]; t1 = BK[1]; t2 = BK[2]; }
        MatVec(mat, t0, t1, t2, vx, vy, vz, sf, lm);
    }

    public static uint Read(int reg)
    {
        switch (reg)
        {
            case 0: return (uint)((ushort)V[0] | (V[1] << 16));
            case 1: return (uint)V[2];
            case 2: return (uint)((ushort)V[3] | (V[4] << 16));
            case 3: return (uint)V[5];
            case 4: return (uint)((ushort)V[6] | (V[7] << 16));
            case 5: return (uint)V[8];
            case 6: return (uint)(RGBC_R | (RGBC_G << 8) | (RGBC_B << 16) | (RGBC_CODE << 24));
            case 7: return OTZ;
            case 8: return (uint)IR0;
            case 9: return (uint)IR1;
            case 10: return (uint)IR2;
            case 11: return (uint)IR3;
            case 12: return ReadProjectedScreen(0);
            case 13: return ReadProjectedScreen(1);
            case 14:
            case 15: return ReadProjectedScreen(2);
            case 16: return SZ[0];
            case 17: return SZ[1];
            case 18: return SZ[2];
            case 19: return SZ[3];
            case 20: return RGB[0];
            case 21: return RGB[1];
            case 22: return RGB[2];
            case 23: return RES1;
            case 24: return (uint)MAC0;
            case 25: return (uint)MAC1;
            case 26: return (uint)MAC2;
            case 27: return (uint)MAC3;
            case 28:
            case 29:
                int r = Math.Clamp(IR1 >> 7, 0, 0x1F);
                int g = Math.Clamp(IR2 >> 7, 0, 0x1F);
                int b = Math.Clamp(IR3 >> 7, 0, 0x1F);
                return (uint)(r | (g << 5) | (b << 10));
            case 30: return LZCS;
            case 31: return LZCR;
            default: return 0;
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    public static void Write(int reg, uint val)
    {
        // GT2's track clipper reloads previously projected SXY values through
        // the writable GTE FIFO before emitting its final road packets. Keep
        // the exact CPU/RAM-carried depth attached to that SXY write; dropping
        // it here made the entire foreground road fall back to affine even
        // though the source packet word still had valid projection metadata.
        GteProjectionOriginHandle projectedOrigin = default;
        GteProjectedValue projected = default;
        if (ProjectionTrackingEnabled)
        {
            if (reg is >= 12 and <= 15)
            {
                projected = WorldCaptureContext.CaptureEnabled
                    ? ConsumeCpuRegisterWrite(val, out projectedOrigin)
                    : ConsumeCpuRegisterWrite(val);
            }
            else if (PendingCpuValue.Valid ||
                (WorldCaptureContext.CaptureEnabled && PendingCpuOrigin.Valid))
            {
                // A non-SXY GTE write still ends the immediate CPU-register
                // transfer opportunity. Avoid doing that bookkeeping for the
                // overwhelmingly common case where nothing is pending.
                PendingCpuValue = default;
                ClearPendingCpuOrigin();
            }
        }
        switch (reg)
        {
            case 0: V[0] = (short)val; V[1] = (short)(val >> 16); break;
            case 1: V[2] = (short)val; break;
            case 2: V[3] = (short)val; V[4] = (short)(val >> 16); break;
            case 3: V[5] = (short)val; break;
            case 4: V[6] = (short)val; V[7] = (short)(val >> 16); break;
            case 5: V[8] = (short)val; break;
            case 6: RGBC_R = (byte)val; RGBC_G = (byte)(val >> 8); RGBC_B = (byte)(val >> 16); RGBC_CODE = (byte)(val >> 24); break;
            case 7: OTZ = (ushort)val; break;
            case 8: IR0 = (short)val; break;
            case 9: IR1 = (short)val; break;
            case 10: IR2 = (short)val; break;
            case 11: IR3 = (short)val; break;
            case 12:
                SX[0] = (short)val;
                SY[0] = (short)(val >> 16);
                SxyDepth[0] = projected.Z;
                SxyDepthValid[0] = projected.Valid;
                if (WorldCaptureContext.CaptureEnabled)
                    SxyOrigin[0] = projectedOrigin;
                break;
            case 13:
                SX[1] = (short)val;
                SY[1] = (short)(val >> 16);
                SxyDepth[1] = projected.Z;
                SxyDepthValid[1] = projected.Valid;
                if (WorldCaptureContext.CaptureEnabled)
                    SxyOrigin[1] = projectedOrigin;
                break;
            case 14:
                SX[2] = (short)val;
                SY[2] = (short)(val >> 16);
                SxyDepth[2] = projected.Z;
                SxyDepthValid[2] = projected.Valid;
                if (WorldCaptureContext.CaptureEnabled)
                    SxyOrigin[2] = projectedOrigin;
                break;
            case 15:
                SX[0] = SX[1]; SY[0] = SY[1]; SX[1] = SX[2]; SY[1] = SY[2];
                SX[2] = (short)val; SY[2] = (short)(val >> 16);
                SxyDepth[0] = SxyDepth[1];
                SxyDepth[1] = SxyDepth[2];
                SxyDepthValid[0] = SxyDepthValid[1];
                SxyDepthValid[1] = SxyDepthValid[2];
                SxyDepth[2] = projected.Z;
                SxyDepthValid[2] = projected.Valid;
                if (WorldCaptureContext.CaptureEnabled)
                {
                    SxyOrigin[0] = SxyOrigin[1];
                    SxyOrigin[1] = SxyOrigin[2];
                    SxyOrigin[2] = projectedOrigin;
                }
                break;
            case 16: SZ[0] = (ushort)val; break;
            case 17: SZ[1] = (ushort)val; break;
            case 18: SZ[2] = (ushort)val; break;
            case 19: SZ[3] = (ushort)val; break;
            case 20: RGB[0] = val; break;
            case 21: RGB[1] = val; break;
            case 22: RGB[2] = val; break;
            case 23: RES1 = val; break;
            case 24: MAC0 = (int)val; break;
            case 25: MAC1 = (int)val; break;
            case 26: MAC2 = (int)val; break;
            case 27: MAC3 = (int)val; break;
            case 28:
                IR1 = (int)((val & 0x1F) << 7);
                IR2 = (int)(((val >> 5) & 0x1F) << 7);
                IR3 = (int)(((val >> 10) & 0x1F) << 7);
                break;
            case 29: break;
            case 30:
                LZCS = val;
                uint test = (val & 0x80000000u) != 0 ? ~val : val;
                LZCR = (uint)(test == 0 ? 32 : System.Numerics.BitOperations.LeadingZeroCount(test));
                break;
            case 31: break;
        }
    }

    public static uint ReadControl(int reg)
    {
        switch (reg)
        {
            case 0: return (uint)((ushort)RT[0] | (RT[1] << 16));
            case 1: return (uint)((ushort)RT[2] | (RT[3] << 16));
            case 2: return (uint)((ushort)RT[4] | (RT[5] << 16));
            case 3: return (uint)((ushort)RT[6] | (RT[7] << 16));
            case 4: return (uint)RT[8];
            case 5: return (uint)TR[0];
            case 6: return (uint)TR[1];
            case 7: return (uint)TR[2];
            case 8: return (uint)((ushort)LLM[0] | (LLM[1] << 16));
            case 9: return (uint)((ushort)LLM[2] | (LLM[3] << 16));
            case 10: return (uint)((ushort)LLM[4] | (LLM[5] << 16));
            case 11: return (uint)((ushort)LLM[6] | (LLM[7] << 16));
            case 12: return (uint)LLM[8];
            case 13: return (uint)BK[0];
            case 14: return (uint)BK[1];
            case 15: return (uint)BK[2];
            case 16: return (uint)((ushort)LCM[0] | (LCM[1] << 16));
            case 17: return (uint)((ushort)LCM[2] | (LCM[3] << 16));
            case 18: return (uint)((ushort)LCM[4] | (LCM[5] << 16));
            case 19: return (uint)((ushort)LCM[6] | (LCM[7] << 16));
            case 20: return (uint)LCM[8];
            case 21: return (uint)FC[0];
            case 22: return (uint)FC[1];
            case 23: return (uint)FC[2];
            case 24: return (uint)OFX;
            case 25: return (uint)OFY;
            case 26: return (uint)(short)H;
            case 27: return (uint)DQA;
            case 28: return (uint)DQB;
            case 29: return (uint)ZSF3;
            case 30: return (uint)ZSF4;
            case 31: return FilterModernVehicleProjectionLimits(FLAG);
            default: return 0;
        }
    }

    public static void WriteControl(int reg, uint val)
    {
        if ((uint)reg <= 7u)
            ProjectionTransformHashDirty = true;
        switch (reg)
        {
            case 0: RT[0] = (short)val; RT[1] = (short)(val >> 16); break;
            case 1: RT[2] = (short)val; RT[3] = (short)(val >> 16); break;
            case 2: RT[4] = (short)val; RT[5] = (short)(val >> 16); break;
            case 3: RT[6] = (short)val; RT[7] = (short)(val >> 16); break;
            case 4: RT[8] = (short)val; break;
            case 5: TR[0] = (int)val; break;
            case 6: TR[1] = (int)val; break;
            case 7: TR[2] = (int)val; break;
            case 8: LLM[0] = (short)val; LLM[1] = (short)(val >> 16); break;
            case 9: LLM[2] = (short)val; LLM[3] = (short)(val >> 16); break;
            case 10: LLM[4] = (short)val; LLM[5] = (short)(val >> 16); break;
            case 11: LLM[6] = (short)val; LLM[7] = (short)(val >> 16); break;
            case 12: LLM[8] = (short)val; break;
            case 13: BK[0] = (int)val; break;
            case 14: BK[1] = (int)val; break;
            case 15: BK[2] = (int)val; break;
            case 16: LCM[0] = (short)val; LCM[1] = (short)(val >> 16); break;
            case 17: LCM[2] = (short)val; LCM[3] = (short)(val >> 16); break;
            case 18: LCM[4] = (short)val; LCM[5] = (short)(val >> 16); break;
            case 19: LCM[6] = (short)val; LCM[7] = (short)(val >> 16); break;
            case 20: LCM[8] = (short)val; break;
            case 21: FC[0] = (int)val; break;
            case 22: FC[1] = (int)val; break;
            case 23: FC[2] = (int)val; break;
            case 24: OFX = (int)val; break;
            case 25: OFY = (int)val; break;
            case 26: H = (ushort)val; break;
            case 27: DQA = (short)val; break;
            case 28: DQB = (int)val; break;
            case 29: ZSF3 = (short)val; break;
            case 30: ZSF4 = (short)val; break;
            case 31: FLAG = val & 0x7FFFF000u; if ((FLAG & 0x7F87E000u) != 0) FLAG |= 0x80000000u; break;
        }
    }

    public static void LoadWord(int reg, uint val) => Write(reg, val);
    public static uint StoreWord(int reg)
    {
        uint value = Read(reg);
        if (reg is >= 12 and <= 15)
        {
            int screenIndex = reg switch
            {
                12 => 0,
                13 => 1,
                _ => 2,
            };
            ushort depth = SxyDepth[screenIndex];
            PendingDirectStore =
                !ProjectionTrackingEnabled ||
                !SxyDepthValid[screenIndex] ||
                depth == 0
                ? null
                : new PacketDepthSample(
                    value, depth, ScreenDepthGeneration,
                    GteDepthProvenance.DirectStore);
            if (WorldCaptureContext.CaptureEnabled)
            {
                PendingDirectOrigin =
                    PendingDirectStore != null &&
                    SxyOrigin[screenIndex].Valid
                    ? SxyOrigin[screenIndex]
                    : default;
            }
        }
        else
        {
            PendingDirectStore = null;
            ClearPendingDirectOrigin();
        }
        return value;
    }
    public static bool GetCondition() => false;
}
