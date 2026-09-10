using System.Buffers.Binary;
using System.Diagnostics;
using System.Threading;
using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;

namespace RecompOne.Runtime.Sdk;

public sealed class GT2VariantSwitch(string variant) : Exception
{
    public string Variant { get; } = variant;
}

/// <summary>
/// Narrow host bridges proven against Gran Turismo 2 SCUS-94488.
/// </summary>
public static class GT2Compat
{
    const uint CarPreviewCameraAddress = 0x800F04E0u;
    const uint CarPreviewCameraDistanceLimit = 0x000F4CCCu;

    readonly record struct LiverySelection(
        uint BodyId, byte BodyPaletteIndex, byte ColorId);

    /// <summary>
    /// Keeps the selector's probabilistic 0x8000 pullback steps inside GT2's
    /// authored preview stage. The car enters at 0x94CCC and completes twelve
    /// steps at 0xF4CCC; only the shared selector camera uses this ceiling.
    /// </summary>
    public static uint LimitCarPreviewCameraDistance(
        uint cameraAddress, uint distance) =>
        cameraAddress == CarPreviewCameraAddress &&
        distance > CarPreviewCameraDistanceLimit
            ? CarPreviewCameraDistanceLimit
            : distance;

    /// <summary>
    /// Activates GT2's current view projection before its vehicle pass. The
    /// vehicle path can emit or classify geometry before its selected model
    /// calls func_8007B688, so inheriting projection state from the preceding
    /// auxiliary/main pass assigns those cars to the wrong view. The view
    /// record stores OFX, OFY, and H at +0x5C, +0x60, and +0x64.
    /// </summary>
    public static void ActivateVehicleProjectionFromView(
        uint viewRecord, IMemory memory)
    {
        if (!IsGuestRam(viewRecord))
            throw new InvalidOperationException(
                $"GT2 view record is outside guest RAM: 0x{viewRecord:X8}");
        RecompOne.Runtime.Gte.WriteControl(
            24, memory.ReadU32(viewRecord + 0x5Cu));
        RecompOne.Runtime.Gte.WriteControl(
            25, memory.ReadU32(viewRecord + 0x60u));
        RecompOne.Runtime.Gte.WriteControl(
            26, memory.ReadU16(viewRecord + 0x64u));
    }

    /// <summary>
    /// Exact aligned MIPS word-copy bridge for bounded, proven hot loops. The
    /// PSMemory path retains every RAM observer; other IMemory implementations
    /// preserve behavior through ordinary word accesses.
    /// </summary>
    public static void CopyAlignedGuestWords(
        CpuContext context,
        IMemory memory,
        uint source,
        uint destination,
        uint byteCount)
    {
        if (byteCount == 0u ||
            ((source | destination | byteCount) & 0xFu) != 0u)
            throw new ArgumentException(
                "Guest quad-word copy must be nonempty and 16-byte aligned.");
        if (memory is PSMemory psMemory &&
            psMemory.TryCopyAlignedRamWords(
                source,
                destination,
                byteCount,
                out uint finalWord0,
                out uint finalWord1,
                out uint finalWord2,
                out uint finalWord3))
        {
            context.T0 = finalWord0;
            context.T1 = finalWord1;
            context.T2 = finalWord2;
            context.T3 = finalWord3;
            context.V0 = source + byteCount;
            context.V1 = destination + byteCount;
            return;
        }
        for (uint offset = 0; offset < byteCount; offset += 16u)
        {
            context.T0 = memory.ReadU32(source + offset);
            context.T1 = memory.ReadU32(source + offset + 4u);
            context.T2 = memory.ReadU32(source + offset + 8u);
            context.T3 = memory.ReadU32(source + offset + 12u);
            memory.WriteU32(destination + offset, context.T0);
            memory.WriteU32(destination + offset + 4u, context.T1);
            memory.WriteU32(destination + offset + 8u, context.T2);
            memory.WriteU32(destination + offset + 12u, context.T3);
        }
        context.V0 = source + byteCount;
        context.V1 = destination + byteCount;
    }

    private sealed class NonLocalJump(
        uint target, bool returnTrampoline = false) : Exception
    {
        public uint Target { get; } = target;
        public bool ReturnTrampoline { get; } = returnTrampoline;
    }

    public static bool ExpandedPolygonBuffersEnabled =>
        Config.ConfigManager.View.ExtendedDrawDistance ||
        Config.ConfigManager.View.LevelOfDetail.Equals(
            "Maximum", StringComparison.OrdinalIgnoreCase);

    // Internal regression-isolation switches default to enabled. Environment
    // variables cannot change after process startup, so read each override
    // once. ExpandTrackFrustumClassification runs for every resident course
    // object and querying the Windows process environment there accounted for
    // several percent of the emulation thread's Seattle frame time.
    static readonly bool ExtendedReplayTrackDrawOverride =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_GT2_EXTENDED_REPLAY_TRACK_DRAW") != "0";
    static readonly bool ExtendedTrackRadialLimitOverride =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_GT2_EXTENDED_TRACK_RADIAL_LIMIT") != "0";
    static readonly bool ExtendedTrackFrustumOverride =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_GT2_EXTENDED_TRACK_FRUSTUM") != "0";
    static readonly bool ExtendedVehicleFrustumOverride =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_GT2_EXTENDED_VEHICLE_FRUSTUM") != "0";
#if !OPENGT_RELEASE_PACKAGE
    // Development oracle only. This isolates the bounded resident horizon
    // from projection/depth changes at an identical deterministic race poll.
    // Public builds always retain the bounded PC course horizon.
    static readonly bool ResidentCourseCatalogEnabled =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_GT2_RESIDENT_COURSE_CATALOG") != "0";
#endif

    public static bool ExtendedReplayTrackDrawDistanceEnabled =>
        Config.ConfigManager.View.ExtendedDrawDistance &&
        ExtendedReplayTrackDrawOverride;

    /// <summary>
    /// Overlay 0 applies a second, camera-relative radial cutoff after walking
    /// the current sector visibility list. Replacing that list alone therefore
    /// cannot extend draw distance: distant objects remain absent until they
    /// cross the stock 0x0063FFFF threshold, which exposes whole section
    /// boundaries as visible pop-in. Disable only this redundant radial cutoff
    /// when Extended Draw Distance is selected; retain the authored
    /// current-sector potential-visibility set. Frustum/near-plane rejection
    /// and the game's ordinary polygon clipping still run unchanged.
    /// </summary>
    public static uint GetTrackDrawDistanceLimit() =>
        Config.ConfigManager.View.ExtendedDrawDistance &&
        ExtendedTrackRadialLimitOverride
            ? uint.MaxValue
            : 0x0063FFFFu;

    /// <summary>
    /// GT2 classifies each course object against its authored 4:3 viewport
    /// before emitting any primitives: 0 is inside, 1 intersects, and 2 is
    /// outside. GT2's intersecting route has a distinct authored packet order;
    /// it is not interchangeable with the inside route. Expanded objects use
    /// that intersecting route so the resident renderer receives the correct
    /// topology and UV association, reconstructs continuous vertices, and
    /// delegates clipping to D3D at the actual target aspect. Only viewport
    /// rejection can be expanded: low mask bit 0 is shared near/depth rejection
    /// and bit 5 is shared GTE projection failure. Admitting those distant or
    /// behind-camera objects can turn normalized model coordinates into a
    /// false foreground surface. A box merely intersecting the near plane is
    /// still retained; this is not per-polygon camera-plane rejection.
    /// </summary>
    public static uint ApplyModernTrackFrustumClassification(
        uint classification,
        uint clipMask,
        bool enabled)
    {
        const uint viewportRejection = 0x1Eu;
        const uint depthOrProjectionRejection = 0x21u;
        bool onlyViewportOutside =
            (clipMask & viewportRejection) != 0u &&
            (clipMask & depthOrProjectionRejection) == 0u;
        return enabled && classification == 2u && onlyViewportOutside
            ? 1u
            : classification;
    }

    public static uint ExpandTrackFrustumClassification(
        uint classification, uint clipMask, uint modelAddress)
    {
        int tracePoll = Host.InputManager.CurrentPoll;
        if (TraceTrackFrustum && TraceTrackVisibilityStartPoll >= 0 &&
            tracePoll >= TraceTrackVisibilityStartPoll &&
            tracePoll <= TraceTrackVisibilityEndPoll)
            Console.Error.WriteLine(
                $"[GT2-Track-Box] poll={tracePoll} model=0x{modelAddress:X8} " +
                $"classification={classification} mask=0x{clipMask:X8}");
        switch (classification)
        {
            case 0u: _trackFrustumInside++; break;
            case 1u: _trackFrustumIntersecting++; break;
            case 2u: _trackFrustumOutside++; break;
        }
        uint expanded = ApplyModernTrackFrustumClassification(
            classification,
            clipMask,
            Config.ConfigManager.View.ExtendedDrawDistance &&
                ExtendedTrackFrustumOverride);
        if (expanded != classification)
        {
            _trackFrustumExpanded++;
        }
        return expanded;
    }

    static readonly bool TraceBoot =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_BOOT") == "1";
    static readonly bool TraceMenu =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_MENU") == "1";
    static readonly bool TraceRaceScheduler =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_SCHEDULER") == "1";
    // Genuine per-VBlank simulation is the sole GT2 architecture. Retired
    // 30 Hz and synthetic-midpoint modes cannot be re-enabled at runtime.
    static readonly bool True60HzEnabled = true;
    static readonly bool TraceTrue60HzCadence =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_GT2_TRUE60_CADENCE") == "1";
    static readonly int True60HzStateTracePoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_TRUE60_STATE_POLL"),
            out int true60StateTracePoll)
            ? true60StateTracePoll
            : -1;
    static readonly bool TraceTrackRendering =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_TRACK_RENDERING") == "1";
    static readonly bool TraceTrackFrustum =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_TRACK_FRUSTUM") == "1";
    static readonly int TraceTrackVisibilityStartPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_VISIBILITY_START_POLL"),
            out int traceTrackVisibilityStartPoll)
            ? traceTrackVisibilityStartPoll
            : -1;
    static readonly int TraceTrackVisibilityEndPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_VISIBILITY_END_POLL"),
            out int traceTrackVisibilityEndPoll)
            ? traceTrackVisibilityEndPoll
            : -1;
    static readonly bool TraceVehicleLod =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_VEHICLE_LOD") == "1";
    static readonly int TraceDepthNormalizationPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_DEPTH_NORMALIZATION_POLL"),
            out int traceDepthNormalizationPoll)
                ? traceDepthNormalizationPoll
                : -1;
    static readonly int TraceProjectionPhasePoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_PROJECTION_PHASE_POLL"),
            out int traceProjectionPhasePoll)
                ? traceProjectionPhasePoll
                : -1;
    static readonly bool TraceWheelTransforms =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_GT2_WHEEL_TRANSFORMS") == "1";
    static readonly bool TraceVehicleVisibility =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_GT2_VEHICLE_VISIBILITY") == "1";
    static readonly int TraceVehicleVisibilityStartPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_VEHICLE_VISIBILITY_START_POLL"),
            out int traceVehicleVisibilityStartPoll)
                ? traceVehicleVisibilityStartPoll
                : -1;
    static readonly int TraceVehicleVisibilityEndPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_VEHICLE_VISIBILITY_END_POLL"),
            out int traceVehicleVisibilityEndPoll)
                ? traceVehicleVisibilityEndPoll
                : int.MaxValue;
    static readonly int TraceVehicleVisibilityLimit =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_VEHICLE_VISIBILITY_LIMIT"),
            out int traceVehicleVisibilityLimit) &&
            traceVehicleVisibilityLimit > 0
                ? traceVehicleVisibilityLimit
                : TraceVehicleVisibilityStartPoll < 0 ? 128 : 4096;
    static readonly string? WheelTransformTracePath =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_GT2_WHEEL_TRANSFORM_TRACE_PATH");
    static readonly bool AuditRenderer =
        Environment.GetEnvironmentVariable("RECOMPONE_AUDIT_RENDERER") == "1";
    static readonly bool TraceAiDrivers =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_AI_DRIVERS") == "1";
    static readonly string? AiMemoryTracePath =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_GT2_AI_MEMORY_PATH");
    static readonly bool TraceLiveries =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_LIVERIES") == "1";
    static readonly string? ArcadeRaceConfigTracePath =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_GT2_ARCADE_RACE_CONFIG_PATH");
    static readonly string? ArcadePreFinalizeConfigTracePath =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_GT2_ARCADE_PRE_FINALIZE_CONFIG_PATH");
    static readonly string? ArcadeRaceStateTracePath =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_GT2_ARCADE_RACE_STATE_PATH");
    static readonly string? ArcadeRaceMemoryTracePath =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_GT2_ARCADE_RACE_MEMORY_PATH");
    static readonly bool AiAutoDrive =
        Environment.GetEnvironmentVariable("RECOMPONE_GT2_AI_AUTODRIVE") == "1";
    static readonly bool UnlockArcadeCourses =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_GT2_ARCADE_UNLOCK_ALL_COURSES") == "1";
    static string? DirectArcadeRace =>
        Environment.GetEnvironmentVariable(
            "RECOMPONE_GT2_DIRECT_ARCADE_RACE");
    static readonly int AiAutoDriveMaxEngagements =
        ParseAiAutoDriveMaxEngagements(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS"));
    static readonly int AiAutoDriveQuickWinAfterTicks =
        ParseAiAutoDriveQuickWinAfterTicks(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_GT2_SOAK_QUICK_WIN_AFTER_AI_TICKS"));
    static readonly uint DiagnosticVehicleLodSelector =
        ParseVehicleLodSelector(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_GT2_VEHICLE_LOD_SELECTOR_OVERRIDE"));
    static readonly HashSet<string> RaceSchedulerStates = [];
    static int _true60HzSchedulerReports;
    static int _arcadeCourseUnlockReported;
    static int _arcadeCourseStockFallbackReported;
    static int _arcadeRaceConfigTraceReported;
    static int _arcadePreFinalizeConfigTraceReported;
    static int _arcadeRaceStateTraceReported;
    static int _arcadeRaceMemoryTraceReported;
    static int _true60HzVSyncReports;
    static uint[]? _true60HzStateSnapshot;
    static uint _true60HzStateCar;
    static readonly uint[] True60HzLinearVelocityOffsets =
    [
        0x688u, 0x68Cu, 0x690u,
    ];
    static readonly uint[] True60HzLinearVelocityBefore =
        new uint[16 * True60HzLinearVelocityOffsets.Length];
    static readonly int[] True60HzLinearVelocityRemainders =
        new int[16 * True60HzLinearVelocityOffsets.Length];
    static uint _true60HzVelocityCarArray;
    static uint _true60HzVelocityCarCount;
    static int _true60HzRaceSegment;

    readonly record struct ReplayPhysicalState(
        int X,
        int Y,
        int Z,
        int VelocityX,
        int VelocityY,
        int VelocityZ,
        int LongitudinalSpeed,
        ushort Progress,
        ushort Heading)
    {
        public ulong Hash
        {
            get
            {
                ulong hash = Fnv64Offset;
                hash = HashValue(hash, unchecked((uint)X));
                hash = HashValue(hash, unchecked((uint)Y));
                hash = HashValue(hash, unchecked((uint)Z));
                hash = HashValue(hash, unchecked((uint)VelocityX));
                hash = HashValue(hash, unchecked((uint)VelocityY));
                hash = HashValue(hash, unchecked((uint)VelocityZ));
                hash = HashValue(
                    hash, unchecked((uint)LongitudinalSpeed));
                hash = HashValue(hash, Progress);
                return HashValue(hash, Heading);
            }
        }
    }

    static readonly List<ulong> ReplayOracleRecordedControls = [];
    static readonly List<ReplayPhysicalState?> ReplayOracleRecordedStates = [];
    static uint _replayOracleRecordBuffer;
    static uint _replayOracleRecordFrameCount;
    static uint _replayOraclePlaybackBuffer;
    static int _replayOraclePlaybackFrames;
    static int _replayOracleControlComparisons;
    static int _replayOracleControlMismatches;
    static int _replayOracleStateComparisons;
    static int _replayOracleStateMismatches;
    static int _replayOracleLastStateFrame = -1;
    static bool _replayOraclePlaybackActive;
    static bool _replayOracleMode;
    static int _replayOracleControlFrame = -1;
    static ulong _replayOracleRecordedControlHash = Fnv64Offset;
    static ulong _replayOracleExpectedControlHash = Fnv64Offset;
    static ulong _replayOraclePlaybackControlHash = Fnv64Offset;
    static ulong _replayOracleRecordedStateHash = Fnv64Offset;
    static ulong _replayOraclePlaybackStateHash = Fnv64Offset;
    static long _replayOraclePlaybackStartTimestamp;
    const ulong Fnv64Offset = 14695981039346656037ul;
    const ulong Fnv64Prime = 1099511628211ul;

    /// <summary>
    /// Select the expanded GT1-aware table only when the loaded Arcade
    /// overlay actually contains it. Release installs reconstructed from the
    /// two GT2 discs carry the stock overlay, so unconditionally redirecting
    /// them past the end of that image produces a zero-filled one-course list.
    /// </summary>
    public static uint ResolveArcadeCourseTable(
        uint stockTable, uint expandedTable, IMemory m)
    {
        const int recordBytes = 32;
        (uint expectedStock, int stockCount, int expandedCount) =
            expandedTable switch
        {
            0x800533C0u => (0x80050730u, 21, 22),
            0x800536A0u => (0x800509F0u, 21, 22),
            0x80053980u => (0x80050CB0u, 23, 24),
            0x80053CA0u => (0x80050FB0u, 23, 24),
            0x80053FC0u => (0x800513F0u, 21, 22),
            _ => (0u, 0, 0),
        };
        if (!IsGuestRam(stockTable) || !IsGuestRam(expandedTable) ||
            stockTable != expectedStock || stockCount == 0)
            throw new InvalidOperationException(
                "Unknown Arcade course-table pair: " +
                $"stock=0x{stockTable:X8} expanded=0x{expandedTable:X8}");

        bool RecordIsEmpty(uint address)
        {
            for (int offset = 0; offset < recordBytes; offset++)
            {
                if (m.ReadU8(address + (uint)offset) != 0)
                    return false;
            }
            return true;
        }

        bool RecordLooksValid(uint address)
        {
            return IsGuestRam(m.ReadU32(address)) &&
                IsGuestRam(m.ReadU32(address + 4u)) &&
                IsGuestRam(m.ReadU32(address + 8u));
        }

        bool TableLooksValid(uint address, int count) =>
            RecordLooksValid(address) &&
            RecordIsEmpty(address + (uint)(count * recordBytes));

        bool expandedValid = TableLooksValid(expandedTable, expandedCount);
        bool stockValid = TableLooksValid(stockTable, stockCount);
        if (!expandedValid && !stockValid)
            throw new InvalidDataException(
                "Loaded Arcade overlay contains neither a valid stock nor " +
                "expanded course table");

        uint selected = expandedValid ? expandedTable : stockTable;
        int selectedCount = expandedValid ? expandedCount : stockCount;
        if (!expandedValid &&
            Interlocked.Exchange(ref _arcadeCourseStockFallbackReported, 1) == 0)
        {
            Console.Error.WriteLine(
                "[GT2] stock Arcade course tables selected; expanded " +
                "GT1 overlay data is absent from this GT2-disc install");
        }

        if (!UnlockArcadeCourses)
            return selected;

        const uint unlockOffset = 20u;
        for (int index = 0; index < selectedCount; index++)
        {
            uint record = selected + (uint)(index * recordBytes);
            m.WriteU32(record + unlockOffset, 0xFFFFu);
        }
        if (Interlocked.Exchange(ref _arcadeCourseUnlockReported, 1) == 0)
            Console.Error.WriteLine(
                $"[GT2] diagnostic Arcade course unlock active; " +
                $"firstTable=0x{selected:X8} entries={selectedCount}");
        return selected;
    }
    static readonly HashSet<uint> TrackObjects = [];
    static readonly HashSet<uint> TrackViewIndices = [];
    static readonly HashSet<uint> TrackMeshIndices = [];
    static readonly HashSet<uint> TrackPasses = [];
    static readonly Dictionary<int, long> ReplayTrackSelectorCounts = [];
    static readonly HashSet<uint> ReplayTrackModelPointers = [];
    static readonly HashSet<uint> ReplayTrackModelIds = [];
    static long _trackRenderRequests;
    static int _trackRenderSamples;
    static int _trackVisibilitySamples;
    static int _trackFrustumSamples;
    static long _trackVisibilityFunctionCalls;
    static long _trackVisibilityRaceCalls;
    static long _trackVisibilityReplayCalls;
    static long _trackVisibilityRawEntries;
    static long _trackVisibilityRawNonzeroSelectors;
    static int _renderSchedulerSamples;
    static bool _trackExitTraceRegistered;
    static readonly HashSet<uint> TrackVisibilitySectors = [];
    static long _visibilityLodRaceCalls;
    static long _visibilityLodReplayCalls;
    static long _visibilityLodMaximumCalls;
    static long _visibilityLodStockCalls;
    static long _visibilityLodEntriesScanned;
    static long _visibilityLodNonzeroSelectors;
    static long _visibilityLodNullLists;
    static long _visibilityLodInvalidLists;
    static long _visibilityExpandedCalls;
    static long _visibilityExpandedStockEntries;
    static long _visibilityExpandedOutputEntries;
    static int _visibilityExpandedMaximumAdded;
    static long _visibilitySectorTransitions;
    static long _visibilityStockTransitionAdds;
    static long _visibilityStockTransitionRemoves;
    static long _visibilityStockAddsPrecovered;
    static long _visibilityStockRemovesRetained;
    static long _visibilityExpandedTransitionAdds;
    static long _visibilityExpandedTransitionRemoves;
    static int _visibilityStockMaximumTransition;
    static int _visibilityExpandedMaximumTransition;
    static long _trackFrustumInside;
    static long _trackFrustumIntersecting;
    static long _trackFrustumOutside;
    static long _trackFrustumExpanded;
    static readonly HashSet<uint> VisibilityLodInvalidListSamples = [];
    static int _visibilityLodExitTraceRegistered;
    static int _wheelTransformTraceEnabledReported;
    static int _wheelTransformTraceInvalidIndexReported;
    static int _wheelGateTraceSamples;
    static int _wheelDispatchTraceSamples;
    static int _wheelRendererEntryTraceSamples;
    static uint _vehicleRenderIdentity;
    static long _vehicleRenderIdentityBegins;
    static long _vehicleRenderIdentityEnds;
    static long _vehicleRenderScopedCaptures;
    static long _vehicleRenderFallbackCaptures;
    static readonly HashSet<uint> VehicleRenderIdentities = [];
    static long _wheelGateRequests;
    static long _wheelGateGuestDistanceEligible;
    static long _wheelGateComputedDistanceEligible;
    static long _wheelGateModeEligible;
    static long _wheelGateMismatches;
    static long _wheelDispatchRequests;
    static long _wheelDispatchNearDetail;
    static long _wheelDispatchMiddleDetail;
    static long _wheelDispatchFarDetail;
    static long _wheelRendererEntries;
    static long _wheelRendererNearDetail;
    static long _wheelRendererMiddleDetail;
    static long _wheelRendererFarDetail;
    static long _vehicleFrustumRequests;
    static long _vehicleFrustumHorizontalOutside;
    static long _vehicleFrustumHorizontalIntersecting;
    static long _vehicleFrustumHorizontalOutsideOnly;
    static long _vehicleFrustumHorizontalIntersectingOnly;
    static long _vehicleFrustumHorizontalBoth;
    static long _vehicleFrustumOtherOutside;
    static long _vehicleFrustumOtherIntersecting;
    static long _vehicleFrustumExpanded;
    static int _vehicleFrustumTraceSamples;
    static readonly Dictionary<uint, long> VehicleLodSelectorCounts = [];
    static readonly HashSet<uint> VehicleLodModelSets = [];
    static readonly HashSet<uint> VehicleLodModelPointers = [];
    sealed class WheelTransformTraceState
    {
        public long Samples;
        public readonly int[] Previous = new int[3];
        public readonly long[] Changes = new long[3];
        public readonly int[] MinimumShortestDelta =
            [int.MaxValue, int.MaxValue, int.MaxValue];
        public readonly int[] MaximumShortestDelta =
            [int.MinValue, int.MinValue, int.MinValue];
    }
    static readonly object WheelTransformTraceLock = new();
    static readonly Dictionary<ulong, WheelTransformTraceState>
        WheelTransformTraceStates = [];
    static readonly System.Text.StringBuilder WheelTransformTraceCsv = new();
    static bool _wheelTransformTraceExitRegistered;
    static bool _wheelTransformTraceFlushed;
    static long _wheelTransformTraceRecords;
    const uint ExpandedVisibilityListAddress = 0x807F0000u;
    const int ExtendedVisibilitySectorRadius = 4;
    static readonly int[] VisibilityEntryGenerations = new int[0x4000];
    static readonly int[] VisibilityEntryPositions = new int[0x4000];
    static readonly ushort[] ExpandedVisibilityEntries = new ushort[0x4000];
    static readonly bool[] PreviousStockVisibilityEntries = new bool[0x4000];
    static readonly bool[] PreviousExpandedVisibilityEntries = new bool[0x4000];
    static readonly bool[] CurrentStockVisibilityEntries = new bool[0x4000];
    static readonly bool[] CurrentExpandedVisibilityEntries = new bool[0x4000];
    static int _visibilityEntryGeneration;
    static uint _visibilityTransitionTrackRoot;
    static int _visibilityTransitionSector = -1;
    static int _bootObjectTraceCount;
    static int _overlayLoadTraceCount;
    static int _finiteCdReadTraceCount;
    static int _menuListTraceCount;
    static uint _overlayIndex;
    static string _overlayPrefix = "gt2_overlay";
    static bool _unifiedTitleInstalled;
    static bool _unifiedSimulationSaveImported;
    static bool _unifiedArcadeTransition;
    static bool _unifiedOpeningPrelude;
    static bool _unifiedOpeningCompleted;
    static bool _reportedSimulationBootPanelOmission;
    static bool _unifiedArcadeFrontendPending;
    static bool _unifiedSimulationFrontendPending;
    static long _unifiedTitleConfirmationMixFrame;
    static uint _unifiedTitleConfirmationKeyOnSerial;
    static long _unifiedArcadeSelectionTimestamp;
    static int _unifiedArcadeSelectionInputPoll;
    static readonly object LiveryTableLock = new();
    static Dictionary<ulong, LiverySelection>? _liveriesByPalette;
    static Dictionary<ulong, LiverySelection>? _liveriesByColorId;
    static Dictionary<uint, uint>? _liveryChoiceCounts;
    static int _liveryPaletteTraceCount;
    static int _liveryColorTraceCount;

    static ulong LiveryKey(uint bodyId, uint selector) =>
        ((ulong)bodyId << 32) | selector;

    static void EnsureLiveryTable()
    {
        if (Volatile.Read(ref _liveriesByPalette) != null)
            return;
        lock (LiveryTableLock)
        {
            if (Volatile.Read(ref _liveriesByPalette) != null)
                return;

            var byPalette = new Dictionary<ulong, LiverySelection>();
            var byColorId = new Dictionary<ulong, LiverySelection>();
            var choiceCounts = new Dictionary<uint, uint>();
            var ambiguousColorIds = new HashSet<ulong>();
            string? root = Runtime.ResolveLoosePath();
            string? path = root == null
                ? null
                : Path.Combine(root, "GTLIVERY.BIN");
            if (path != null && File.Exists(path))
            {
                byte[] data = File.ReadAllBytes(path);
                if (data.Length < 8 ||
                    !data.AsSpan(0, 4).SequenceEqual("GTLV"u8))
                    throw new InvalidDataException(
                        $"Invalid GT2 livery table header: {path}");
                ushort version = BinaryPrimitives.ReadUInt16LittleEndian(
                    data.AsSpan(4, 2));
                ushort count = BinaryPrimitives.ReadUInt16LittleEndian(
                    data.AsSpan(6, 2));
                if (version != 3 || data.Length != 8 + count * 12)
                    throw new InvalidDataException(
                        "Unsupported GT2 livery table: " +
                        $"version={version}, records={count}, " +
                        $"bytes={data.Length}");

                for (int index = 0; index < count; index++)
                {
                    int offset = 8 + index * 12;
                    uint targetBody =
                        BinaryPrimitives.ReadUInt32LittleEndian(
                            data.AsSpan(offset, 4));
                    uint alternateBody =
                        BinaryPrimitives.ReadUInt32LittleEndian(
                            data.AsSpan(offset + 4, 4));
                    byte targetPalette = data[offset + 8];
                    byte bodyPalette = data[offset + 9];
                    ushort encodedColorId =
                        BinaryPrimitives.ReadUInt16LittleEndian(
                            data.AsSpan(offset + 10, 2));
                    if (encodedColorId > byte.MaxValue)
                        throw new InvalidDataException(
                            $"GT2 livery record {index} has an invalid color ID");
                    var selection = new LiverySelection(
                        alternateBody, bodyPalette, (byte)encodedColorId);
                    if (!byPalette.TryAdd(
                            LiveryKey(targetBody, targetPalette), selection))
                        throw new InvalidDataException(
                            $"GT2 livery record {index} is duplicated");
                    choiceCounts[targetBody] = Math.Max(
                        choiceCounts.GetValueOrDefault(targetBody),
                        (uint)targetPalette + 1);
                    ulong colorKey =
                        LiveryKey(targetBody, encodedColorId);
                    if (!ambiguousColorIds.Contains(colorKey))
                    {
                        if (byColorId.TryGetValue(
                                colorKey, out var existing) &&
                            existing != selection)
                        {
                            // Retail assumes a color ID uniquely identifies a
                            // palette. GT1's alternate Castrol Supra packages
                            // disprove that assumption: the same authored ID
                            // selects different native bodies. Palette-index
                            // hooks retain the exact choice; ID-only fallback
                            // deliberately becomes a no-op for ambiguity.
                            byColorId.Remove(colorKey);
                            ambiguousColorIds.Add(colorKey);
                        }
                        else
                        {
                            byColorId[colorKey] = selection;
                        }
                    }
                }
                Console.WriteLine(
                    "[GT2] native alternate-livery table loaded: " +
                    $"{count} body/palette mappings, " +
                    $"{ambiguousColorIds.Count} ambiguous color IDs");
            }
            _liveryChoiceCounts = choiceCounts;
            _liveriesByColorId = byColorId;
            // Publish the palette table last. Readers use it as the
            // initialization sentinel, so observing it also makes the
            // color-ID table and all populated dictionary entries visible.
            Volatile.Write(ref _liveriesByPalette, byPalette);
        }
    }

    /// <summary>
    /// Resolve an added color choice to its native alternate car-object body
    /// and that body's original palette. The low 32 bits are the body ID and
    /// the high 32 bits are the palette index.
    /// </summary>
    public static ulong ResolveLiveryBodyAndPalette(
        uint bodyId, uint paletteIndex)
    {
        EnsureLiveryTable();
        var byPalette = Volatile.Read(ref _liveriesByPalette)!;
        bool mapped = byPalette.TryGetValue(
            LiveryKey(bodyId, paletteIndex), out var selection);
        uint resolvedBody = mapped ? selection.BodyId : bodyId;
        uint resolvedPalette = mapped
            ? selection.BodyPaletteIndex
            : paletteIndex;
        if (TraceLiveries &&
            Interlocked.Increment(ref _liveryPaletteTraceCount) <= 256)
            Console.WriteLine(
                "[GT2-Livery] palette " +
                $"body=0x{bodyId:X8} index={paletteIndex} " +
                $"mapped={mapped} -> body=0x{resolvedBody:X8} " +
                $"index={resolvedPalette}");
        return resolvedBody | ((ulong)resolvedPalette << 32);
    }

    public static void ResolveLiveryBodyAndPaletteA0S2(CpuContext c)
    {
        ulong resolved = ResolveLiveryBodyAndPalette(c.A0, c.S2);
        c.A0 = (uint)resolved;
        c.S2 = (uint)(resolved >> 32);
    }

    public static void ResolveLiveryBodyAndPaletteA1A2(CpuContext c)
    {
        ulong resolved = ResolveLiveryBodyAndPalette(c.A1, c.A2);
        c.A1 = (uint)resolved;
        c.A2 = (uint)(resolved >> 32);
    }

    /// <summary>
    /// Resolve only the body half of a palette-index choice while a frontend
    /// record is being authored. Keeping that resolved body in the record
    /// preserves packages that intentionally reuse another package's color ID.
    /// </summary>
    public static uint ResolveLiveryBodyForPalette(
        uint bodyId, uint paletteIndex) =>
        (uint)ResolveLiveryBodyAndPalette(bodyId, paletteIndex);

    public static uint ResolveLiveryChoiceCount(
        uint bodyId, uint nativeCount)
    {
        EnsureLiveryTable();
        return Math.Max(
            nativeCount,
            _liveryChoiceCounts!.GetValueOrDefault(bodyId));
    }

    /// <summary>
    /// Frontend records carry the database color ID rather than its palette
    /// slot. Swap only the native body here; the original frontend then finds
    /// that same color ID in the hidden body's carinfo record and derives its
    /// correct local palette normally.
    /// </summary>
    public static uint ResolveLiveryBodyForColorId(
        uint bodyId, uint colorId)
    {
        EnsureLiveryTable();
        bool mapped = _liveriesByColorId!.TryGetValue(
            LiveryKey(bodyId, colorId), out var selection);
        uint resolvedBody = mapped ? selection.BodyId : bodyId;
        if (TraceLiveries &&
            Interlocked.Increment(ref _liveryColorTraceCount) <= 256)
            Console.WriteLine(
                "[GT2-Livery] color " +
                $"body=0x{bodyId:X8} id={colorId} mapped={mapped} " +
                $"-> body=0x{resolvedBody:X8}");
        return resolvedBody;
    }

    const uint UnifiedTitleList = 0x8004BC28u;
    const uint UnifiedTitleDescriptorBase = 0x803FF000u;
    const uint UnifiedTitleBlankDescriptor = UnifiedTitleDescriptorBase + 0x30u;
    static bool _unifiedTitleMenuActive;
    static ushort[]? _unifiedTitlePanels;
    static uint _unifiedTitleBufferedPressed;
    static uint _unifiedTitleBufferedRepeated;
    const int UnifiedTitleWidth = 512;
    const int UnifiedTitleHeight = 480;

    public static bool UnifiedTitleMenuActive => _unifiedTitleMenuActive;
    public static bool UnifiedArcadeTransitionCoverActive =>
        _unifiedArcadeFrontendPending ||
        _unifiedSimulationFrontendPending;
    /// <summary>
    /// Arcade's service initializer includes two timed boot panels in
    /// func_80010CEC, before mounting the game filesystem. A selector-driven
    /// handoff has already presented the unified boot sequence and covers
    /// these panels; do not spend another 310 display ticks on them. Keep the
    /// surrounding GPU, filesystem, audio and frontend initialization intact.
    /// </summary>
    public static bool ShouldPresentArcadeBootPanels()
    {
        if (!_unifiedArcadeTransition)
            return true;
        Console.WriteLine(
            "[GT2] Arcade handoff: omitted duplicate timed boot panels");
        return false;
    }

    /// <summary>
    /// The retail Arcade disc owns GT2's intact opening STR data. A normal
    /// unified launch runs that original opening before booting Simulation,
    /// whose native bootstrap still loads the memory card and enters the
    /// unified title. Standalone Arcade diagnostics retain their own title.
    /// </summary>
    public static void SetUnifiedOpeningPrelude(bool enabled)
    {
        _unifiedOpeningPrelude = enabled;
        _unifiedOpeningCompleted = false;
        _reportedSimulationBootPanelOmission = false;
    }

    public static bool ShouldPresentSimulationBootPanels()
    {
        if (!_unifiedOpeningCompleted)
            return true;
        if (!_reportedSimulationBootPanelOmission)
        {
            _reportedSimulationBootPanelOmission = true;
            Console.WriteLine(
                "[GT2] Simulation boot: omitted duplicate timed panels after " +
                "the original Arcade opening");
        }
        return false;
    }

    public static void ConfigureUnifiedTitlePanels(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        const int headerSize = 20;
        const int panelPixels = UnifiedTitleWidth * UnifiedTitleHeight;
        const int panelCount = 4;
        if (data.Length != headerSize + panelPixels * panelCount * 2 ||
            System.Text.Encoding.ASCII.GetString(data, 0, 8) != "GT2TITLE" ||
            BitConverter.ToInt32(data, 8) != UnifiedTitleWidth ||
            BitConverter.ToInt32(data, 12) != UnifiedTitleHeight ||
            BitConverter.ToInt32(data, 16) != panelCount)
        {
            throw new InvalidDataException(
                $"invalid exact GT2 title panel asset: {path}");
        }
        var pixels = new ushort[panelPixels * panelCount];
        Buffer.BlockCopy(data, headerSize, pixels, 0, pixels.Length * 2);
        _unifiedTitlePanels = pixels;
        Console.WriteLine(
            "[GT2] loaded exact Sony title panels: " +
            $"{panelCount}x{UnifiedTitleWidth}x{UnifiedTitleHeight} 15-bit");
    }

    public static void CompositeUnifiedTitlePanel(
        global::RecompOne.Runtime.Gpu gpu, IMemory m)
    {
        ushort[]? panels = _unifiedTitlePanels;
        bool coverGuestHandoff =
            _unifiedArcadeFrontendPending ||
            _unifiedSimulationFrontendPending;
        if ((!_unifiedTitleMenuActive && !coverGuestHandoff) || panels == null)
            return;
        // The live title owns the guest display registers. During the Arcade
        // handoff, HostWindow presents this VRAM panel explicitly so the cover
        // cannot alter the Arcade frontend's newly initialized GPU state.
        if (_unifiedTitleMenuActive)
            EnableExactTitleDisplay();
        // Simulation overlay 1 starts an attract-mode race after 901 idle
        // ticks. The unified frontend is a persistent PC main menu, so keep
        // that private counter at zero while preserving ordinary pad input.
        if (_unifiedTitleMenuActive)
            m.WriteU32(0x800B1228u, 0u);
        int selected = coverGuestHandoff
            ? 1
            : m.ReadU16(UnifiedTitleList + 6u);
        if (selected is < 1 or > 4)
            selected = 1;
        int panelPixels = UnifiedTitleWidth * UnifiedTitleHeight;
        int source = (selected - 1) * panelPixels;
        ReadOnlySpan<ushort> frame = panels.AsSpan(source, panelPixels);
        for (int y = 0; y < UnifiedTitleHeight; y++)
            frame.Slice(y * UnifiedTitleWidth, UnifiedTitleWidth).CopyTo(
                gpu.Vram.AsSpan(y * RecompOne.Runtime.Hle.VramShadow.Width,
                    UnifiedTitleWidth));
        if (RecompOne.Runtime.Hle.GpuHle.Backend?.Ready == true)
            RecompOne.Runtime.Hle.GpuHle.Backend.WriteVram(
                0, 0, UnifiedTitleWidth, UnifiedTitleHeight, frame);
    }

    public static bool SuppressUnifiedTitleListDecorations(uint list) =>
        _unifiedTitleMenuActive && list == UnifiedTitleList;

    /// <summary>
    /// Keep the skipped Arcade-disc title resident only for standalone Arcade
    /// boot diagnostics. The unified path enters overlay 2 directly, so this
    /// counter is normally never exposed to the player.
    /// </summary>
    public static void BeginUnifiedArcadeFrontendFrame(IMemory m)
    {
        if (_unifiedArcadeTransition)
            m.WriteU32(0x800B0F20u, 0u);
    }

    /// <summary>
    /// Remove the unified-title transition cover only after the requested
    /// Arcade Mode menu enters its native presentation loop. Its original
    /// reveal animation still runs after this boundary. This prevents
    /// the guest-image and display initializers from exposing a disc-style
    /// reboot between menus.
    /// </summary>
    public static void CompleteUnifiedArcadeFrontendFrame()
    {
        if (!_unifiedArcadeFrontendPending)
            return;
        _unifiedArcadeFrontendPending = false;
        double elapsedMilliseconds = _unifiedArcadeSelectionTimestamp > 0
            ? Stopwatch.GetElapsedTime(
                _unifiedArcadeSelectionTimestamp).TotalMilliseconds
            : 0.0;
        _unifiedArcadeSelectionTimestamp = 0;
        int readyPoll = Host.InputManager.CurrentPoll;
        int elapsedPolls = readyPoll - _unifiedArcadeSelectionInputPoll;
        _unifiedArcadeSelectionInputPoll = 0;
        string[] profile = Dispatch.Dispatcher.EndMethodProfile(
            "arcade-menu-handoff");
        Console.WriteLine(
            "[GT2] seamless Arcade frontend entered; transition cover released " +
            $"selectionToFrontendMs={elapsedMilliseconds:F3} " +
            $"selectionToFrontendPolls={elapsedPolls}");
        Host.InputManager.SignalScriptStage("arcade_frontend");
        if (profile.Length != 0)
        {
            Console.Error.WriteLine(
                "[GT2-Handoff-Methods] " +
                $"count={profile.Length} names={string.Join(',', profile)}");
        }
    }

    /// <summary>
    /// Record the SPU key-on generation immediately before the original title
    /// selector invokes confirmation effect 3. The generation lets the host
    /// distinguish that short voice from persistent title music.
    /// </summary>
    public static void BeginUnifiedTitleConfirmationAudio(uint index)
    {
        if (index != 1u)
            return;
        _unifiedTitleConfirmationKeyOnSerial =
            Runtime.Spu?.LatestKeyOnSerial ?? 0u;
    }

    /// <summary>
    /// The unified Arcade selection swaps guest RAM immediately so Simulation
    /// overlay 4 cannot win a title-state race. Before that swap, advance only
    /// VBlank/BIOS boundaries until the original confirmation effect is keyed,
    /// then let that exact SPU voice finish plus a short output-queue tail. No
    /// Simulation guest title update runs during this bounded drain.
    /// </summary>
    public static void CompleteUnifiedTitleConfirmationAudio()
    {
        long baseline = Interlocked.Exchange(
            ref _unifiedTitleConfirmationMixFrame, 0);
        uint keyedAfterSerial = _unifiedTitleConfirmationKeyOnSerial;
        _unifiedTitleConfirmationKeyOnSerial = 0;
        Spu? spu = Runtime.Spu;
        if (baseline <= 0 || spu == null ||
            !RecompOne.Runtime.Host.Audio.MixerActive)
            return;

        long started = Stopwatch.GetTimestamp();
        uint voiceMask = spu.CaptureVoiceMaskKeyedAfter(keyedAfterSerial);
        int queuedVBlanks = 0;
        // GT2 may defer sequenced effects to its installed VBlank callback.
        // Run that boundary without re-entering overlay 1's title update.
        while (voiceMask == 0 && queuedVBlanks < 12)
        {
            Runtime.PresentFrame();
            queuedVBlanks++;
            voiceMask = spu.CaptureVoiceMaskKeyedAfter(keyedAfterSerial);
        }

        bool voiceCompleted = voiceMask != 0 && spu.WaitForVoicesToStop(
            voiceMask, keyedAfterSerial, timeoutMilliseconds: 3000);
        // SDL normally holds about 4096 frames (~93 ms). Advancing another
        // 6144 frames drains the sample end from that queue and leaves a small
        // ~46 ms silent margin before the guest reset.
        long tailBaseline = RecompOne.Runtime.Host.Audio.MixedFrameCount;
        bool tailCompleted = RecompOne.Runtime.Host.Audio.WaitForMixAdvance(
            tailBaseline,
            additionalFrames: 6144,
            timeoutMilliseconds: 500);
        Console.WriteLine(
            "[GT2] unified title confirmation audio: " +
            $"voiceMask=0x{voiceMask:X6} " +
            $"queuedVBlanks={queuedVBlanks} " +
            $"voiceCompleted={voiceCompleted} tailCompleted={tailCompleted} " +
            $"elapsedMs={Stopwatch.GetElapsedTime(started).TotalMilliseconds:F3} " +
            $"mixedFrames=" +
            (RecompOne.Runtime.Host.Audio.MixedFrameCount - baseline));
    }

    /// <summary>
    /// Overlay 2's native Back result normally requests the Arcade disc title
    /// overlay. In the unified product that title was deliberately skipped, so
    /// preserve the authored Back operation while routing its destination to
    /// the unified Simulation title instead.
    /// </summary>
    public static void ReturnFromUnifiedArcade()
    {
        if (!_unifiedArcadeTransition)
            return;
        _unifiedArcadeFrontendPending = false;
        _unifiedSimulationFrontendPending = true;
        Console.WriteLine(
            "[GT2] Arcade Mode Back: returning to unified title");
        throw new GT2VariantSwitch("simulation");
    }

    /// <summary>
    /// Gran Turismo Mode's world-map root has no disc-era parent screen, so
    /// its native Triangle handling intentionally does nothing there. In the
    /// unified PC shell that root does have a parent: the unified title menu.
    /// Intercept Triangle only while the world-map controller is idle; nested
    /// dealerships, garages and modal transitions retain their native Back
    /// behavior.
    /// </summary>
    public static void ReturnFromUnifiedGranTurismoRoot(
        uint worldMapController, IMemory m)
    {
        if (_overlayPrefix != "gt2_overlay" ||
            _overlayIndex != 4u ||
            worldMapController == 0u ||
            m.ReadU16(worldMapController + 0x1CEu) != 0u ||
            m.ReadU8(worldMapController + 0x1B0u) != 0u ||
            m.ReadU8(worldMapController + 0x1C4u) != 0u ||
            m.ReadU8(worldMapController + 0x1CCu) != 0u ||
            (RecompOne.Runtime.Hardware.Controller.State & 0x1000u) != 0u)
        {
            return;
        }

        _unifiedTitleMenuActive = false;
        _unifiedSimulationFrontendPending = true;
        Console.WriteLine(
            "[GT2] Gran Turismo Mode Back: returning to unified title");
        throw new GT2VariantSwitch("simulation-title");
    }

    public static bool ArcadeVariant =>
        _overlayPrefix.Equals(
            "gt2_arcade_overlay", StringComparison.Ordinal);

    public static uint CdDriveStateAddress =>
        ArcadeVariant ? 0x801EFF00u : 0x801F0510u;

    public static uint CdReadyCallbackAddress =>
        ArcadeVariant ? 0x8007CD04u : 0x8007CDF4u;

    public static uint CdFiniteReadHandlerAddress =>
        ArcadeVariant ? 0x8007DE28u : 0x8007DF18u;

    public static void SetUnifiedArcadeTransition(bool enabled) =>
        _unifiedArcadeTransition = enabled;

    readonly record struct DirectArcadeCourseSpec(
        string SelectionName,
        string RaceName,
        uint Hash);

    // This is the complete native stock-course inventory used by the renderer
    // certification harness: all 30 forward layouts plus every reverse layout
    // exposed by GT2 Arcade.  The dirt and reverse identities come directly
    // from the archive's .crsinfo table.  Converted Route 11 forward/reverse
    // identities are deliberately kept last by the certification harness.
    // Course identity is data only: the same native Class C race constructor
    // and renderer path is used for every non-Seattle entry.
    static readonly Dictionary<string, DirectArcadeCourseSpec>
        DirectArcadeCourses = new(StringComparer.OrdinalIgnoreCase)
        {
            ["tahiti-road"] = new(
                "Tahiti Road", "Tahiti Road", 0x6AD87E5Eu),
            ["midfield-raceway"] = new(
                "Midfield Raceway", "Midfield Raceway", 0x718B3BA1u),
            ["high-speed-ring"] = new(
                "High Speed Ring", "High Speed Ring", 0x35B88252u),
            ["super-speedway"] = new(
                "Super Speedway", "Super Speedway", 0x34C670ECu),
            ["seattle-short-course"] = new(
                "Seattle Short Course", "Seattle Short Course", 0xA2D75F7Cu),
            ["rome-short-course"] = new(
                "Rome Short Course", "Rome Short Course", 0x5197C71Cu),
            ["red-rock-valley-speedway"] = new(
                "Red Rock Valley Speedway",
                "Red Rock Valley Speedway",
                0x73AB047Eu),
            ["seattle-circuit"] = new(
                "Seattle Circuit", "Seattle Circuit Full Course", 0xA2D762AEu),
            ["rome-circuit"] = new(
                "Rome Circuit", "Rome Circuit Full Course", 0x01CF0BA1u),
            ["grindelwald"] = new(
                "Grindelwald", "Grindelwald", 0xEABE6038u),
            ["laguna-seca-raceway"] = new(
                "Laguna Seca Raceway", "Laguna Seca Raceway", 0x62A36BFCu),
            ["apricot-hill-speedway"] = new(
                "Apricot Hill Speedway", "Apricot Hill Speedway", 0x7EB5CA9Fu),
            ["motor-sports-land"] = new(
                "Motor Sports Land", "Motor Sports Land", 0x01922CF4u),
            ["trial-mountain"] = new(
                "Trial Mountain Circuit",
                "Trial Mountain Circuit",
                0xAFD7E5BBu),
            ["clubman-stage-route-5"] = new(
                "Clubman Stage Route 5",
                "Clubman Stage Route 5",
                0x33D95B55u),
            ["grand-valley-east-section"] = new(
                "Grand Valley East Section",
                "Grand Valley East Section",
                0x74A70CF4u),
            ["grand-valley-speedway"] = new(
                "Grand Valley Speedway",
                "Grand Valley Speedway",
                0xB39370FEu),
            ["special-stage-route-5"] = new(
                "Special Stage Route 5",
                "Special Stage Route 5",
                0xA8A78F53u),
            ["autumn-ring"] = new(
                "Autumn Ring", "Autumn Ring", 0xB6D76BC6u),
            ["test-course"] = new(
                "Test Course", "Test Course", 0x74C669A4u),
            ["deep-forest-raceway"] = new(
                "Deep Forest Raceway", "Deep Forest Raceway", 0x3584821Fu),
            ["rome-night"] = new(
                "Rome-Night", "Rome-Night", 0x4C9B449Cu),
            ["autumn-ring-mini"] = new(
                "Autumn Ring Mini", "Autumn Ring Mini", 0x01BAABE9u),
            ["green-forest-roadway"] = new(
                "Green Forest Roadway", "Green Forest Roadway", 0x6B3F15FCu),
            ["pikes-peak-downhill"] = new(
                "Pikes Peak Downhill", "Pikes Peak Downhill", 0xB4F4E47Fu),
            ["pikes-peak-hill-climb"] = new(
                "Pikes Peak Hill Climb",
                "Pikes Peak Hill Climb",
                0x71AAC9B3u),
            ["smokey-mountain-north"] = new(
                "Smokey Mountain North",
                "Smokey Mountain North",
                0xA8C6399Cu),
            ["smokey-mountain-south"] = new(
                "Smokey Mountain South",
                "Smokey Mountain South",
                0x6B3F15FBu),
            ["tahiti-dirt-route-3"] = new(
                "Tahiti Dirt Route 3", "Tahiti Dirt Route 3", 0x4FEDD235u),
            ["tahiti-maze"] = new(
                "Tahiti Maze", "Tahiti Maze", 0x600625B5u),
            ["apricot-hill-speedway-reverse"] = new(
                "Apricot Hill Speedway",
                "Apricot Hill Speedway",
                0xA101EF80u),
            ["autumn-ring-reverse"] = new(
                "Autumn Ring", "Autumn Ring", 0xD3BE49B6u),
            ["autumn-ring-mini-reverse"] = new(
                "Autumn Ring Mini", "Autumn Ring Mini", 0xE0BC7A56u),
            ["clubman-stage-route-5-reverse"] = new(
                "Clubman Stage Route 5",
                "Clubman Stage Route 5",
                0x342B5B55u),
            ["deep-forest-raceway-reverse"] = new(
                "Deep Forest Raceway", "Deep Forest Raceway", 0xA36383EDu),
            ["grand-valley-east-section-reverse"] = new(
                "Grand Valley East Section",
                "Grand Valley East Section",
                0x351AA86Cu),
            ["grand-valley-speedway-reverse"] = new(
                "Grand Valley Speedway",
                "Grand Valley Speedway",
                0xED4AED05u),
            ["grindelwald-reverse"] = new(
                "Grindelwald", "Grindelwald", 0xB79B445Eu),
            ["high-speed-ring-reverse"] = new(
                "High Speed Ring", "High Speed Ring", 0xA3978420u),
            ["midfield-raceway-reverse"] = new(
                "Midfield Raceway", "Midfield Raceway", 0xA2F4C4F1u),
            ["red-rock-valley-speedway-reverse"] = new(
                "Red Rock Valley Speedway",
                "Red Rock Valley Speedway",
                0xAD628085u),
            ["rome-circuit-reverse"] = new(
                "Rome Circuit", "Rome Circuit Full Course", 0xE0D0DA0Eu),
            ["rome-short-course-reverse"] = new(
                "Rome Short Course", "Rome Short Course", 0x41B4ADFAu),
            ["rome-night-reverse"] = new(
                "Rome-Night", "Rome-Night", 0x3CB82B7Au),
            ["seattle-circuit-reverse"] = new(
                "Seattle Circuit", "Seattle Circuit Full Course", 0x762B025Fu),
            ["seattle-short-course-reverse"] = new(
                "Seattle Short Course",
                "Seattle Short Course",
                0x75F7E25Fu),
            ["smokey-mountain-north-reverse"] = new(
                "Smokey Mountain North",
                "Smokey Mountain North",
                0x9E2BFFEFu),
            ["smokey-mountain-south-reverse"] = new(
                "Smokey Mountain South",
                "Smokey Mountain South",
                0xFCEE78CBu),
            ["special-stage-route-5-reverse"] = new(
                "Special Stage Route 5",
                "Special Stage Route 5",
                0xA8A8D753u),
            ["tahiti-dirt-route-3-reverse"] = new(
                "Tahiti Dirt Route 3", "Tahiti Dirt Route 3", 0x36D32788u),
            ["tahiti-road-reverse"] = new(
                "Tahiti Road", "Tahiti Road", 0x5FEE1234u),
            ["trial-mountain-reverse"] = new(
                "Trial Mountain Circuit",
                "Trial Mountain Circuit",
                0x1DB6E78Au),
            ["special-stage-route-11"] = new(
                "Special Stage Route 11",
                "Special Stage Route 11",
                0xBFF9AB74u),
            ["special-stage-route-11-reverse"] = new(
                "Special Stage Route 11",
                "Special Stage Route 11",
                0xFE6ADDA1u),
        };

    public static IReadOnlyList<string> SupportedDirectArcadeCourses { get; } =
    [
        "tahiti-road",
        "midfield-raceway",
        "high-speed-ring",
        "super-speedway",
        "seattle-short-course",
        "rome-short-course",
        "red-rock-valley-speedway",
        "seattle-circuit",
        "rome-circuit",
        "grindelwald",
        "laguna-seca-raceway",
        "apricot-hill-speedway",
        "motor-sports-land",
        "trial-mountain",
        "clubman-stage-route-5",
        "grand-valley-east-section",
        "grand-valley-speedway",
        "special-stage-route-5",
        "autumn-ring",
        "test-course",
        "deep-forest-raceway",
        "rome-night",
        "autumn-ring-mini",
        "green-forest-roadway",
        "pikes-peak-downhill",
        "pikes-peak-hill-climb",
        "smokey-mountain-north",
        "smokey-mountain-south",
        "tahiti-dirt-route-3",
        "tahiti-maze",
        "apricot-hill-speedway-reverse",
        "autumn-ring-reverse",
        "autumn-ring-mini-reverse",
        "clubman-stage-route-5-reverse",
        "deep-forest-raceway-reverse",
        "grand-valley-east-section-reverse",
        "grand-valley-speedway-reverse",
        "grindelwald-reverse",
        "high-speed-ring-reverse",
        "midfield-raceway-reverse",
        "red-rock-valley-speedway-reverse",
        "rome-circuit-reverse",
        "rome-short-course-reverse",
        "rome-night-reverse",
        "seattle-circuit-reverse",
        "seattle-short-course-reverse",
        "smokey-mountain-north-reverse",
        "smokey-mountain-south-reverse",
        "special-stage-route-5-reverse",
        "tahiti-dirt-route-3-reverse",
        "tahiti-road-reverse",
        "trial-mountain-reverse",
        "special-stage-route-11",
        "special-stage-route-11-reverse",
    ];

    public static bool IsSupportedDirectArcadeCourse(string course) =>
        DirectArcadeCourses.ContainsKey(course);

    static DirectArcadeCourseSpec RequiredDirectArcadeCourse =>
        !string.IsNullOrWhiteSpace(DirectArcadeRace) &&
        DirectArcadeCourses.TryGetValue(
            DirectArcadeRace, out DirectArcadeCourseSpec course)
            ? course
            : throw new InvalidOperationException(
                $"Unsupported direct Arcade race: {DirectArcadeRace}");

    static bool DirectSeattle =>
        DirectArcadeRace?.Equals(
            "seattle-circuit",
            StringComparison.OrdinalIgnoreCase) == true;

    // Every new audit course deliberately uses the verified Class C Xsara
    // template. It gives manual and automated audits a slower field while the
    // native constructor still owns every vehicle and race-state record.
    static bool DirectClassC =>
        !string.IsNullOrWhiteSpace(DirectArcadeRace) && !DirectSeattle;

    static bool IsSupportedDirectArcadeRace() =>
        !string.IsNullOrWhiteSpace(DirectArcadeRace) &&
        IsSupportedDirectArcadeCourse(DirectArcadeRace);

    static string DirectArcadeRaceLabel =>
        RequiredDirectArcadeCourse.SelectionName;

    /// <summary>
    /// A unified-menu handoff has already shown the Simulation-disc legal and
    /// opening presentation.  Preserve Arcade's normal bootstrap, but ask its
    /// original overlay loader to enter the native Arcade frontend directly.
    /// Standalone diagnostics retain the stock Arcade-disc opening overlay.
    /// </summary>
    public static uint InitialArcadeOverlayIndex(IMemory m)
    {
        if (_unifiedArcadeTransition)
            RestoreUnifiedArcadeProgress(m);
        else
            ReconcileArcadeOpeningMovieExtent(m);
        if (!string.IsNullOrWhiteSpace(DirectArcadeRace))
        {
            if (!IsSupportedDirectArcadeRace())
                throw new InvalidOperationException(
                    $"Unsupported direct Arcade race: {DirectArcadeRace}");
            // Overlay 2 owns the native race-state constructor.  Enter it
            // directly and let the generated guest hook below bypass only
            // its interactive menu loop; jumping to overlay 3 would omit the
            // separate 0x58C-byte race state that overlay 0 consumes.
            return 2u;
        }
        // The unified title's Arcade selection is already equivalent to the
        // stock disc's START GAME confirmation. Overlay 2 is the resulting
        // ARCADE MODE menu (Single Player / 2 Player Battle / Bonus Items /
        // Load Guest Garage); overlay 1 is the redundant disc title screen.
        return _unifiedArcadeTransition ? 2u : 5u;
    }

    /// <summary>
    /// Reconcile the opening-movie extent with the loose-disc manifest before
    /// the retail Arcade movie player turns its file-relative sector table
    /// into absolute CD positions. The guest's compact ISO cache can retain
    /// the metadata-sector-relative value in standalone loose mode; the
    /// manifest is the authoritative equivalent of the disc directory record.
    /// </summary>
    public static void ReconcileArcadeOpeningMovieExtent(IMemory m)
    {
        if (Runtime.Cd == null ||
            !Runtime.Cd.Fs.Locate("STREAM.DAT", out int lba, out uint size))
        {
            if (TraceBoot)
                Console.Error.WriteLine(
                    "[GT2Compat] opening movie extent unavailable in active disc");
            return;
        }

        const uint streamBaseAddress = 0x801C8E2Cu;
        uint guestBase = m.ReadU32(streamBaseAddress);
        if (guestBase == (uint)lba)
            return;
        m.WriteU32(streamBaseAddress, (uint)lba);
        Console.WriteLine(
            "[GT2] Arcade opening movie extent reconciled: " +
            $"guestLba={guestBase} manifestLba={lba} bytes={size}");
    }

    // Both NTSC-U guests use the same save-data layout at different working
    // bases. The seamless path skips Arcade's disc-title card load, so carry
    // the complete retail payload across its BSS reset and restore it AFTER
    // the native new-game initializer. This is the same 0x7C9C-byte copy made
    // by each disc's original load routine; it includes the garage, current
    // car, credits, licenses, records, and their native lock state.
    static byte[]? _pendingUnifiedArcadeSave;
    const uint SimulationProgressBase = 0x801C98E0u;
    const uint ArcadeProgressBase = 0x801C9340u;
    const int UnifiedSavePayloadLength = 0x7C9C;

    public static void PreserveUnifiedArcadeProgress(IMemory m)
    {
        var save = new byte[UnifiedSavePayloadLength];
        for (int offset = 0; offset < save.Length; offset++)
            save[offset] = m.ReadU8(
                SimulationProgressBase + (uint)offset);
        _pendingUnifiedArcadeSave = save;
    }

    static void RestoreUnifiedArcadeProgress(IMemory m)
    {
        if (_pendingUnifiedArcadeSave is not { } save)
            return;
        for (int offset = 0; offset < save.Length; offset++)
            m.WriteU8(
                ArcadeProgressBase + (uint)offset,
                save[offset]);
        _pendingUnifiedArcadeSave = null;

        // Match SCUS-94455 func_8006A258 after its payload copy.
        MirrorLoadedSettings(m, ArcadeProgressBase + 0x48u, 0x800A6BE4u);
        MirrorLoadedSettings(m, ArcadeProgressBase + 0x9Au, 0x800A6BF8u);
        int licenses = 0;
        int courses = 0;
        for (uint test = 0; test < 60; test++)
            if (m.ReadU8(ArcadeProgressBase + 0x1419u + test * 0xA4u) != 0)
                licenses++;
        for (uint course = 0; course < 21; course++)
            if (m.ReadU8(ArcadeProgressBase + 0xB8u + course) != 0)
                courses++;
        Console.WriteLine(
            $"[GT2] Arcade save preserved from loaded Simulation save: " +
            $"credits={m.ReadU32(ArcadeProgressBase + 0x7C88u)} " +
            $"garageCars={m.ReadU8(ArcadeProgressBase + 0x3C74u)} " +
            $"currentCar={m.ReadU8(ArcadeProgressBase + 0x7C8Cu)} " +
            $"licenseTests={licenses}/60 courseResults={courses}/21");
    }

    /// <summary>
    /// Once overlay 2's own asynchronous setup has created the merged Arcade
    /// parameter database, install a deterministic pre-finalization selection
    /// record. The generated guest hook then calls the original race
    /// constructor; no vehicle, race-state, or post-construction data is
    /// synthesized by the host. Trial Mountain changes only the native course
    /// identity in the verified Class C record.
    /// </summary>
    public static bool PrepareDirectArcadeRaceConfig(IMemory m)
    {
        if (string.IsNullOrWhiteSpace(DirectArcadeRace))
            return false;
        if (!IsSupportedDirectArcadeRace())
            throw new InvalidOperationException(
                $"Unsupported direct Arcade race: {DirectArcadeRace}");

        const uint parameterDatabasePointer = 0x80092B64u;
        uint parameterDatabase = m.ReadU32(parameterDatabasePointer);
        if (parameterDatabase == 0u)
            throw new InvalidOperationException(
                $"Direct {DirectArcadeRaceLabel} frontend resumed before " +
                "the native Arcade " +
                "parameter database was ready");
        if (parameterDatabase != 0x80200000u)
            throw new InvalidOperationException(
                $"Direct {DirectArcadeRaceLabel} native parameter database " +
                "was created at " +
                $"0x{parameterDatabase:X8}; expected 0x80200000");

        InstallDirectArcadePreFinalizeConfig(m, 0x801C3010u);
        return true;
    }

    /// <summary>
    /// Overlay 2's frontend update is a guest coroutine: its one-time setup
    /// callback finishes deep inside the managed call stack and the normal
    /// continuation is reached only after interactive menu selection. Direct
    /// launch needs the setup but not the menus, so unwind to the exact saved
    /// outer stack and resume at the update's authored return continuation.
    /// </summary>
    public static void ResumeDirectArcadeRaceAfterSetup(
        uint loaderObject, CpuContext c, IMemory m)
    {
        if (string.IsNullOrWhiteSpace(DirectArcadeRace))
            return;

        uint parameterDatabase = m.ReadU32(0x80092B64u);
        if (parameterDatabase != 0x80200000u)
            throw new InvalidOperationException(
                $"Direct {DirectArcadeRaceLabel} native frontend setup " +
                "completed with " +
                $"parameter database 0x{parameterDatabase:X8}; " +
                "expected 0x80200000");

        c.SP = loaderObject - 0x10u;
        c.V0 = 0u;
        Console.WriteLine(
            "[GT2-Direct] native Arcade frontend setup complete; " +
            "resuming its race-selection continuation");
        throw new NonLocalJump(0x80011780u, returnTrampoline: true);
    }

    public static uint DirectArcadeRaceSelectionA =>
        DirectClassC ? 0xB957B957u : 0x79977997u;
    public static uint DirectArcadeRaceSelectionB =>
        DirectClassC ? 0x4EDA4EDAu : 0x131E131Eu;

    /// <summary>
    /// Copy overlay 2's verified finalized selection into the fixed handoff
    /// block consumed by overlay 3. This is the byte-for-byte copy performed by
    /// SCUS-94455 after its menu-only fade controller completes.
    /// </summary>
    public static void PrepareDirectArcadeRaceHandoff(IMemory m)
    {
        const uint source = 0x801C3010u;
        const uint destination = 0x801D5A00u;
        const int length = 0x2D4;
        for (int offset = 0; offset < length; offset++)
            m.WriteU8(
                destination + (uint)offset,
                m.ReadU8(source + (uint)offset));
        m.WriteU8(0x801EF021u, 1);
        m.WriteU8(0x801EF022u, 1);
        Console.WriteLine(
            "[GT2-Direct] skipped menu-only fade; native overlay-3 " +
            $"{DirectArcadeRaceLabel} handoff prepared");
    }

    /// <summary>
    /// Fail closed unless the unmodified Arcade constructor reproduces the
    /// exact finalized selection and all non-roster race state. GT2
    /// selects the five opponents from its live frontend RNG state, so each
    /// native vehicle record is validated structurally instead of requiring a
    /// particular menu-timing-dependent lineup.
    /// </summary>
    public static void VerifyDirectArcadeRaceConstruction(IMemory m)
    {
        if (!string.IsNullOrWhiteSpace(ArcadeRaceConfigTracePath))
            CaptureArcadeRaceConfig(
                0x801C3010u,
                ArcadeRaceConfigTracePath,
                "direct-post-constructor",
                m);
        if (!string.IsNullOrWhiteSpace(ArcadeRaceStateTracePath))
            CaptureArcadeRaceState(
                0x801D52BCu,
                ArcadeRaceStateTracePath,
                "direct-post-constructor",
                m);
        VerifyDirectArcadeRaceBytes(
            m,
            0x801C3010u,
            BuildDirectArcadeTemplate(
                DirectSeattleRaceConfigBase64,
                DirectSpecialStageRoute5RaceConfigBase64,
                raceState: false),
            0x2D4,
            "finalized config");
        VerifyDirectArcadeRaceState(
            m,
            BuildDirectArcadeTemplate(
                DirectSeattleRaceStateBase64,
                DirectSpecialStageRoute5RaceStateBase64,
                raceState: true));
        Console.WriteLine(
            $"[GT2-Direct] native {DirectArcadeRaceLabel} construction " +
            "verified; " +
            "parameter-db=0x80200000 config=0x801C3010 state=0x801D52BC");
    }

    public static void TraceArcadeRacePreFinalizeConfig(
        uint selectionA,
        uint selectionB,
        uint address,
        IMemory m)
    {
        if (!ArcadeVariant ||
            string.IsNullOrWhiteSpace(ArcadePreFinalizeConfigTracePath) ||
            Interlocked.Exchange(
                ref _arcadePreFinalizeConfigTraceReported, 1) != 0)
            return;

        CaptureArcadeRaceConfig(
            address,
            ArcadePreFinalizeConfigTracePath,
            "pre-finalize",
            m);
        Console.Error.WriteLine(
            "[GT2-Direct] native race selection inputs " +
            $"a=0x{selectionA:X8} b=0x{selectionB:X8} " +
            $"config=0x{address:X8}");
    }

    // Deterministic input to the authoritative SCUS-94455 native Arcade race
    // constructor for the default road-race grid on Seattle Circuit. Two
    // independent menu-driven captures were byte-identical (SHA-256
    // 47DE747C3F0179D8C8F55910FCE129B186B05701F4AA15686D538CDA39DACAEC),
    // including the Seattle identity already resolved by the course selector.
    const string DirectSeattlePreFinalizeConfigBase64 =
        "AAEEAAIA//8AAAAAWMM1DVjDNQ0AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFNlYXR0bGUgQ2lyY3VpdAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAACuYteiAwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "oHEMgA==";

    // Deterministic output of the authoritative SCUS-94455 native Arcade
    // frontend for the default road-race grid on Seattle Circuit. Two Seattle
    // runs were byte-identical; a Tahiti control differed only in the course
    // name and identity fields. Overlay 3 consumes this exact 0x2D4-byte
    // selection record before entering the unmodified race engine.
    const string DirectSeattleRaceConfigBase64 =
        "AAEEAAIA//8AAAAAWMM1DVjDNQ0AAAAAAAAAAAABAAAKAAAAAAAKAAoACgAKAAoACgARABEAAAALAAAAAAAAAAAAAAAAAAAA" +
        "CgAAAAAAAAAAAAAACQAAAHQJUAr0BhQF6APkAuoB//96Df//DAwRFQAAAAAAAAAAjIyAgCQhS0sBAQEBAQEBAQEBAAcAHgAP" +
        "AQEAAAD/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFNlYXR0bGUgQ2lyY3VpdAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAACuYteiAwAAAAABAAAKAAAAAAAKAAoACgAKAAoACgARABEAAAALAAAAAAAAAAAAAAAAAAAACgAAAAAAAAAAAAAA" +
        "CQAAAHQJUAr0BhQF6APkAuoB//96Df//DAwRFQAAAAAAAAAAjIyAgCQhS0sBAQEBAQEBAQEBAAcAHgAPAQEAAAD/AAAAAAAA" +
        "AAAAAAABAAAKAAAAAAAKAAoACgAKAAoACgARABEAAAALAAAAAAAAAAAAAAAAAAAACgAAAAAAAAAAAAAACQAAAHQJUAr0BhQF" +
        "6APkAuoB//96Df//DAwRFQAAAAAAAAAAjIyAgCQhS0sBAQEBAQEBAQEBAAcAHgAPAQEAAAD/AAAAAAAAAAAAAAAAAAAAAAAA" +
        "oHEMgA==";

    // Stable reference output of Arcade overlay 2's race-state constructor
    // for the Seattle fixture (SHA-256
    // E7F4E82ECE0B0EC1A6726BE732D880C9517C271D6548654BA35EFA1E172FD1F2).
    // Validation compares every byte except the five 0xD0-byte opponent
    // records: GT2 selects that roster from its live frontend RNG state, so
    // those native records are checked structurally below.
    const string DirectSeattleRaceStateBase64 =
        "AAACAQACAAECAAQFAAEAAkEwQQAAAAAAAAAAAAAAAABTZWF0dGxlIENpcmN1aXQgRnVsbCBDb3Vyc2UAAAAAAK5i16IwAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAABgBYwzUNMQAAAAABAAAKAAAAAAAKAAoACgAKAAoACgARABEAAAALAAAAAAAAAAAAAAAAAAAA" +
        "CgAAAAAAAAAAAAAACQAAAHQJUAr0BhQF6APkAuoB//96Df//DAwRFQAAAAAAAAAAjIyAgCQhS0sBAQEBAQEBAQEBAAcAHgAP" +
        "AQEWJwD/AAAAAAAAAAAAAAEFAwBDb3J2ZXR0ZSBDb3VwZSAnOTYAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAWGQNHWgAAAAAAQAAMgAAAAAAMgAyADIAMgAyADIAUABQAAAAPgAAAAAAAAAAAAAAAAAAADIAAAAAAAAA" +
        "AAAAAAkAZAAFDV4MWgcQBcwD4gL/////XBH/HgwMDBUAAAAAAAAAAKCggIAcF1paAQEBAQEBAQEBAQAFABQACgEBBVQA/wEA" +
        "AAAAAAAAAAABBAEAU3ViYXJ1IExlZ2FjeSBCNCBSU0sgJzk4AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAFiXDAs0AAAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAJAGQA" +
        "2AybDd8HbwXoA/oC/////8wQ//8MDA8ZAAAAAAAAAACHh4CAMCRLSwEBAQEBAQEBAQEABQAUAAoBAcpUAP8BAAAAAAAAAAAA" +
        "AQMBAE1hemRhIFJYLTcgVHlwZSBSUyAnOTgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACY0kAf" +
        "NgAAAAABAAA5AAAAAAA5ADkAOQA5ADkAOQBZAFkAAABIAAAAAAAAAAAAAAAAAAAAOQAAAAAAAAAAAAAACQBkAJQMKg3GBzIF" +
        "6AOeAv/////GDP//DAwMGQAAAAAAAAAAoKCAgCAcWloBAQEBAQEBAQEBAAcAHgAPAQFXTwD/AQAAAAAAAAAAAAECAQBNdXN0" +
        "YW5nIFNWVCBDb2JyYSAnOTkAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAGFgMGDYAAAAAAQAA" +
        "LQAAAAAALQAtAC0ALQAtAC0ASABIAAAANwAAAAAAAAAAAAAAAAAAAC0AAAAAAAAAAAAAAAkAZADQDPMOOAmVBiAF6AMZA///" +
        "2Q3//wwMDxkAAAAAAAAAAIKCgIAoMk5OAQEBAQEBAQEBAQAHAB4ADwEBaVQA/wEAAAAAAAAAAAABAQEAU2t5bGluZSBHVC1S" +
        "IFYtc3BlYyhSMzQpAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABhJDhVyAAAAAAEAACYAAAAAACYA" +
        "JgAmACYAJgAmAD4APgAAAC4AAAAAAAAAAAAAAAAAAAAmAAAAAAAAAAAAAAAJAGQAUQz3CyIHtwSeA/QC/////1AQ//8MDA8Z" +
        "AAAAAAAAAAB4eICAGCRLSwEBAQEBAQEBAQEABwAeAA8BAXpZAP8BAAAAAAAAAAAAAQABAFRvbW15a2FpcmEgWlotUyBDb3Vw" +
        "ZSAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA//8DAQAA/////wAAAQAAAA==";

    // Special Stage Route 5's menu-driven course identity combined with the
    // byte-identical Class C selector/player fields from two independent
    // native frontend captures. The original overlay-2 constructor turns this
    // input into the lower-class player and opponent grid verified below.
    const string DirectSpecialStageRoute5PreFinalizeConfigBase64 =
        "AAMEAAIA//8AAAAAWCI2EFgiNhAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFNwZWNpYWwgU3RhZ2UgUm91dGUgNQAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABTj6eoEQAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAoHEMgA==";

    const string DirectSpecialStageRoute5RaceConfigBase64 =
        "AAMEAAIA//8AAAAAWCI2EFgiNhAAAAAAAAAAAAABAAAPAAAAAAAPAA8ADwAPAA8ADwAZABkAAAASAAAAAAAAAAAAAAAAAAAADwAA" +
        "AAAAAAAAAAAABQAAAFkNBQ2jBwYFngPdAv/////YD///DAwMFQAAAAAAAAAAoKCAgBcUWloBAQEBAQEBAQEBAAAAAAAAAQEAAAD/" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFNwZWNpYWwgU3RhZ2UgUm91dGUgNQAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABTj6eoEQAAAAAB" +
        "AAAPAAAAAAAPAA8ADwAPAA8ADwAZABkAAAASAAAAAAAAAAAAAAAAAAAADwAAAAAAAAAAAAAABQAAAFkNBQ2jBwYFngPdAv/////Y" +
        "D///DAwMFQAAAAAAAAAAoKCAgBcUWloBAQEBAQEBAQEBAAAAAAAAAQEAAAD/AAAAAAAAAAAAAAABAAAPAAAAAAAPAA8ADwAPAA8A" +
        "DwAZABkAAAASAAAAAAAAAAAAAAAAAAAADwAAAAAAAAAAAAAABQAAAFkNBQ2jBwYFngPdAv/////YD///DAwMFQAAAAAAAAAAoKCA" +
        "gBcUWloBAQEBAQEBAQEBAAAAAAAAAQEAAAD/AAAAAAAAAAAAAAAAAAAAAAAAoHEMgA==";

    const string DirectSpecialStageRoute5RaceStateBase64 =
        "AAACAQACAAECAAQFAAEAAkEwQwAAAAAAAAAAAAAAAABTcGVjaWFsIFN0YWdlIFJvdXRlIDUAAAAAAAAAAAAAAFOPp6gwAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAABgBYIjYQMQAAAAABAAAPAAAAAAAPAA8ADwAPAA8ADwAZABkAAAASAAAAAAAAAAAAAAAAAAAADwAAAAAA" +
        "AAAAAAAABQAAAFkNBQ2jBwYFngPdAv/////YD///DAwMFQAAAAAAAAAAoKCAgBcUWloBAQEBAQEBAQEBAAAAAAAAAQHlUAD/AAAA" +
        "AAAAAAAAAAEFAwBDaXRyb2VuIFhzYXJhIDEuOGkgMTZWAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "WLIMImIAAAAAAQAAPwAAAAAAPwA/AD8APwA/AD8AZABkAAAATgAAAAAAAAAAAAAAAAAAAD8AAAAAAAAAAAAAAAkAZAD/DeoO5QhI" +
        "BkUERQP/////YRL/FAwMDBMAAAAAAAAAAIeHgIAVE1paAQEBAQEBAQEBAQMAEgAIAAEBRAAA/wEAAAAAAAAAAAABBAEAU3V6dWtp" +
        "IEFsdG8gV29ya3MgUlMvWiAnOTgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFhzDRswAAAAAAEAAC8AAAAA" +
        "AC8ALwAvAC8ALwAvAEsASwAAADoAAAAAAAAAAAAAAAAAAAAvAAAAAAAAAAAAAAAJAGQARgxYDZsH4gSUA+4C/////6kV/xQMDAoQ" +
        "AAAAAAAAAACMjICAFBxaTgEBAQEBAQEBAQEAAAAAAAABAclUAP8BAAAAAAAAAAAAAQMBAERhaWhhdHN1IE1vdmUgQWVyby1DICc5" +
        "OAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABYVIARdAAAAAABAAAeAAAAAAAeAB4AHgAeAB4AHgAwADAA" +
        "AAAjAAAAAAAAAAAAAAAAAAAAHgAAAAAAAAAAAAAACQBkAPQL5AyUBx4FBgRIA/////+GEP//DAwMEwAAAAAAAAAAlpaAgBcUWloB" +
        "AQEBAQEBAQEBMgAeAAAAAQHmUAD/AQAAAAAAAAAAAAECAQBWb2xrc3dhZ2VuIEdvbGYgR1RJAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAWHNxDHAAAAAAAQAABgAAAAAABgAGAAYABgAGAAYACwALAAAABwAAAAAAAAAAAAAAAAAA" +
        "AAYAAAAAAAAAAAAAAAkAZABSDj8OiQiRBegD////////igz//wwMDBAAAAAAAAAAAGlpgIAgG1JaAQEBAQEBAQEBAQAAAAAAAAEB" +
        "eVAA/wEAAAAAAAAAAAABAQEAUm92ZXIgTWluaSBDb29wZXIgMS4zaQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAFhzcQxnAAAAAAEAAAYAAAAAAAYABgAGAAYABgAGAAsACwAAAAcAAAAAAAAAAAAAAAAAAAAGAAAAAAAAAAAAAAAJAGQA" +
        "Ug4/DokIkQXoA////////4oM//8MDAwQAAAAAAAAAABpaYCAIBtSWgEBAQEBAQEBAQEAAAAAAAABAXlQAP8BAAAAAAAAAAAAAQAB" +
        "AFJvdmVyIE1pbmkgQ29vcGVyIDEuM2kAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA//8RAQAA/////wAAAQAAAA==";

    static void InstallDirectArcadePreFinalizeConfig(IMemory m, uint address)
    {
        const int expectedLength = 0x2D4;
        byte[] config = BuildDirectArcadeTemplate(
            DirectSeattlePreFinalizeConfigBase64,
            DirectSpecialStageRoute5PreFinalizeConfigBase64,
            raceState: false);
        if (config.Length != expectedLength)
            throw new InvalidDataException(
                $"Direct {DirectArcadeRaceLabel} race config has " +
                $"{config.Length} bytes; " +
                $"expected {expectedLength}");
        for (int offset = 0; offset < config.Length; offset++)
            m.WriteU8(address + (uint)offset, config[offset]);
        Console.WriteLine(
            $"[GT2-Direct] installed native {DirectArcadeRaceLabel} " +
            "pre-finalize config; " +
            $"address=0x{address:X8} bytes={config.Length} " +
            "native-constructor=overlay-2");
    }

    static byte[] BuildDirectArcadeTemplate(
        string seattleBase64,
        string classCBase64,
        bool raceState)
    {
        byte[] template = Convert.FromBase64String(
            DirectClassC ? classCBase64 : seattleBase64);
        if (DirectClassC)
        {
            DirectArcadeCourseSpec course = RequiredDirectArcadeCourse;
            ReplaceDirectArcadeCourseIdentity(
                template,
                raceState,
                course.SelectionName,
                course.RaceName,
                course.Hash);
        }
        return template;
    }

    static void ReplaceDirectArcadeCourseIdentity(
        byte[] template,
        bool raceState,
        string selectionName,
        string raceName,
        uint courseHash)
    {
        int expectedLength = raceState ? 0x58C : 0x2D4;
        int nameOffset = raceState ? 0x20 : 0xB8;
        int nameCapacity = raceState ? 0x20 : 0x100;
        int hashOffset = raceState ? 0x40 : 0x1B8;
        if (template.Length != expectedLength)
            throw new InvalidDataException(
                $"Direct Arcade template has {template.Length} " +
                $"bytes; expected {expectedLength}");

        byte[] name = System.Text.Encoding.ASCII.GetBytes(
            raceState ? raceName : selectionName);
        if (name.Length >= nameCapacity)
            throw new InvalidDataException(
                "Direct Arcade course name exceeds its native field");
        Array.Clear(template, nameOffset, nameCapacity);
        name.CopyTo(template, nameOffset);
        BitConverter.GetBytes(courseHash)
            .CopyTo(template, hashOffset);
    }

    static void VerifyDirectArcadeRaceBytes(
        IMemory m,
        uint address,
        byte[] expected,
        int expectedLength,
        string label)
    {
        if (expected.Length != expectedLength)
            throw new InvalidDataException(
                $"Direct {DirectArcadeRaceLabel} expected {label} has " +
                $"{expected.Length} bytes; " +
                $"expected {expectedLength}");
        for (int offset = 0; offset < expected.Length; offset++)
        {
            byte actual = m.ReadU8(address + (uint)offset);
            if (actual != expected[offset])
                throw new InvalidOperationException(
                    $"Direct {DirectArcadeRaceLabel} native {label} differs " +
                    $"at +0x{offset:X}: " +
                    $"actual=0x{actual:X2} expected=0x{expected[offset]:X2}");
        }
    }

    static void VerifyDirectArcadeRaceState(
        IMemory m,
        byte[] expected)
    {
        const uint address = 0x801D52BCu;
        const int expectedLength = 0x58C;
        const int playerStart = 0x5C;
        const int opponentStart = 0x12C;
        const int opponentSize = 0xD0;
        const int opponentCount = 5;
        const int opponentEnd = opponentStart + opponentSize * opponentCount;
        const int nameOffset = 0x90;
        const int nameCapacity = 0x40;

        if (expected.Length != expectedLength)
            throw new InvalidDataException(
                $"Direct {DirectArcadeRaceLabel} reference race state has " +
                $"{expected.Length} " +
                $"bytes; expected {expectedLength}");

        var actual = new byte[expectedLength];
        for (int offset = 0; offset < actual.Length; offset++)
        {
            actual[offset] = m.ReadU8(address + (uint)offset);
            if (offset >= opponentStart && offset < opponentEnd)
                continue;
            if (actual[offset] != expected[offset])
                throw new InvalidOperationException(
                    $"Direct {DirectArcadeRaceLabel} native non-roster race " +
                    "state differs at " +
                    $"+0x{offset:X}: actual=0x{actual[offset]:X2} " +
                    $"expected=0x{expected[offset]:X2}");
        }

        string ReadVehicleName(int recordOffset, string label)
        {
            int length = 0;
            while (length < nameCapacity &&
                   actual[recordOffset + nameOffset + length] != 0)
            {
                byte value = actual[recordOffset + nameOffset + length];
                if (value < 0x20 || value >= 0x7F)
                    throw new InvalidOperationException(
                        $"Direct {DirectArcadeRaceLabel} {label} has a " +
                        $"non-ASCII vehicle name byte 0x{value:X2}");
                length++;
            }
            if (length < 4 || length == nameCapacity)
                throw new InvalidOperationException(
                    $"Direct {DirectArcadeRaceLabel} {label} has an " +
                    "invalid native vehicle name");
            return System.Text.Encoding.ASCII.GetString(
                actual,
                recordOffset + nameOffset,
                length);
        }

        string playerName = ReadVehicleName(playerStart, "player");
        var opponentNames = new string[opponentCount];
        for (int index = 0; index < opponentCount; index++)
        {
            int recordOffset = opponentStart + index * opponentSize;
            uint vehicleId = BinaryPrimitives.ReadUInt32LittleEndian(
                actual.AsSpan(recordOffset, sizeof(uint)));
            if (vehicleId == 0u || vehicleId == uint.MaxValue ||
                actual[recordOffset + 9] != 1)
                throw new InvalidOperationException(
                    $"Direct {DirectArcadeRaceLabel} opponent {index} has " +
                    "an invalid native " +
                    $"vehicle record (id=0x{vehicleId:X8})");

            opponentNames[index] = ReadVehicleName(
                recordOffset,
                $"opponent {index}");
        }

        string digest = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(actual));
        Console.WriteLine(
            $"[GT2-Direct] native {DirectArcadeRaceLabel} race state " +
            "verified; " +
            $"sha256={digest} player={playerName} " +
            $"opponents={string.Join(" | ", opponentNames)}");
    }

    /// <summary>
    /// Reproduce the Arcade executable's authored BSS state without invoking
    /// its top-level bootstrap. The range is the exact clear performed by
    /// SCUS-94455 at 0x8005D570 before it initializes services and selects its
    /// first overlay.
    /// </summary>
    public static void PrepareArcadeFrontendHandoff(
        CpuContext c, IMemory m)
    {
        const uint arcadeBssStart = 0x801C8E10u;
        const uint arcadeBssEnd = 0x801F0750u;
        const uint saved = 0x80090E88u;
        m.WriteU32(saved - 8u, c.A0);
        m.WriteU32(saved - 4u, c.A1);
        m.WriteU32(saved, c.S0);
        m.WriteU32(saved + 4u, c.S1);
        m.WriteU32(saved + 8u, c.S2);
        m.WriteU32(saved + 12u, c.S3);
        m.WriteU32(saved + 16u, c.S4);
        m.WriteU32(saved + 20u, c.S5);
        m.WriteU32(saved + 24u, c.S6);
        m.WriteU32(saved + 28u, c.S7);
        m.WriteU32(saved + 32u, c.GP);
        m.WriteU32(saved + 36u, c.SP);
        m.WriteU32(saved + 40u, c.FP);
        m.WriteU32(saved + 44u, c.RA);
        c.SP -= 0x18u;
        m.ZeroRange(arcadeBssStart, arcadeBssEnd - arcadeBssStart);
        Console.WriteLine(
            "[GT2] Arcade handoff prepared: native BSS initialized; " +
            "boot/title overlay omitted");
    }

    /// <summary>
    /// Enter the Arcade frontend through the post-bootstrap initializer used
    /// immediately before the original disc requests its first overlay. The
    /// unified selector changes only that destination to overlay 2. This
    /// retains the native Arcade executable, frontend, overlays and data while
    /// avoiding a second game boot in the unified player-facing flow.
    /// </summary>
    public static void RunArcadeFrontendHandoff(CpuContext c, IMemory m)
    {
        const string arcadePrefix = "gt2_arcade_overlay";
        string previousOverlayPrefix = _overlayPrefix;
        _overlayPrefix = arcadePrefix;
        try
        {
            long serviceStarted = Stopwatch.GetTimestamp();
            // This is the sole pre-initializer call made by the stock Arcade
            // entry after clearing BSS and before entering func_8005D650.
            c.RA = 0x8005D5F0u;
            Dispatch.Dispatcher.Call(c, m, 0x8008CD18u);
            Console.WriteLine(
                "[GT2-Handoff] Arcade service initializer " +
                $"elapsedMs={Stopwatch.GetElapsedTime(serviceStarted).TotalMilliseconds:F3}");
            // SCUS-94455 saves these at 0x80090E80/84 before the initializer
            // and restores them for func_8005D650. Preserve that ABI without
            // running the top-level boot function.
            c.A0 = m.ReadU32(0x80090E80u);
            c.A1 = m.ReadU32(0x80090E84u);
            c.RA = 0x8005D608u;
            Console.WriteLine(
                "[GT2] Arcade frontend handoff: " +
                "entry=0x8005D650 Arcade Mode menu overlay=2");
            RunGuestLoop(c, m, 0x8005D650u, arcadePrefix);
        }
        finally
        {
            _overlayPrefix = previousOverlayPrefix;
        }
    }

    /// <summary>
    /// Extend the original Simulation-disc title list in guest memory.  The
    /// list engine, cursor, fading and draw path remain GT2's; only its item
    /// count and Sony-authored demo TIM descriptors are supplied here. The
    /// obsolete vertical-list boundary triangles are suppressed on this exact
    /// list because the authored 2x2 panel does not use them.
    /// </summary>
    public static void InstallUnifiedTitleMenu(IMemory m)
    {
        ImportUnifiedSimulationSave(m);
        _unifiedTitleMenuActive = true;
        EnableExactTitleDisplay();
        // Two sentinels plus the four complete Sony-authored demo entries:
        // Arcade Mode, Gran Turismo, Replay Theater and Option.
        m.WriteU16(UnifiedTitleList, 6);
        // The final 15-bit panel is composited verbatim at presentation time.
        // Keep the guest list's visual primitives empty while retaining its
        // input, selection, animation timing, and destination dispatch.
        for (uint item = 0; item < 4u; item++)
            WriteTitleDescriptor(
                m, UnifiedTitleDescriptorBase + item * 12u,
                u: 0, v: 0,
                width: 0, height: 0, tpage: 0, clut: 0);
        WriteTitleDescriptor(
            m, UnifiedTitleBlankDescriptor,
            u: 0, v: 0,
            width: 0, height: 0, tpage: 0, clut: 0);

        if (_unifiedSimulationFrontendPending)
        {
            _unifiedSimulationFrontendPending = false;
            Console.WriteLine(
                "[GT2] seamless unified title ready; " +
                "transition cover released");
        }

        if (_unifiedTitleInstalled)
            return;
        _unifiedTitleInstalled = true;
        if (Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_TITLE") == "1")
        {
            Console.WriteLine(
                "[GT2-Title] list=" +
                string.Join(' ', Enumerable.Range(0, 12).Select(index =>
                    $"{index * 4:X2}:{m.ReadU32(UnifiedTitleList + (uint)index * 4u):X8}")));
            uint language = Math.Min(m.ReadU8(0x801C98E0u), (byte)6);
            uint table = m.ReadU32(0x8004BC5Cu + language * 4u);
            for (uint item = 0; item < 8u; item++)
            {
                int label = (short)m.ReadU16(0x8004BC14u + item * 2u);
                uint descriptor = table + (uint)Math.Max(0, label) * 12u;
                Console.WriteLine(
                    $"[GT2-Title] item={item} label={label} " +
                    $"descriptor=0x{descriptor:X8} " +
                    $"uv=0x{m.ReadU16(descriptor):X4} " +
                    $"clut=0x{m.ReadU16(descriptor + 2u):X4} " +
                    $"size={m.ReadU16(descriptor + 4u)}x" +
                    $"{m.ReadU16(descriptor + 6u)} " +
                    $"tpage={m.ReadU16(descriptor + 8u)}");
            }
        }
        string palette = Runtime.Gpu == null
            ? "unavailable"
            : string.Join(
                ',',
                Enumerable.Range(0, 16).Select(index =>
                    $"{Runtime.Gpu.Vram[252 * 1024 + 848 + index]:X4}"));
        Console.WriteLine(
            "[GT2] native unified title menu installed: " +
            "Arcade Mode, Gran Turismo, Replay Theater, Option; " +
            $"language={m.ReadU8(0x801C98E0u)} 16bpp " +
            $"palette={palette}");
    }

    /// <summary>
    /// The unified title replaces the retail START/LOAD menu, so import the
    /// first valid NTSC-U Simulation save before its Gran Turismo and Arcade
    /// destinations are exposed. The payload copy and the two settings mirrors
    /// are the operations performed by SCUS-94488 func_8006A278/func_8006A348.
    /// Run this only on the initial title; returning from either mode must keep
    /// the live session instead of reloading the card.
    /// </summary>
    static void ImportUnifiedSimulationSave(IMemory m)
    {
        if (_unifiedSimulationSaveImported)
            return;
        _unifiedSimulationSaveImported = true;

        Hardware.MemoryCard[] cards = [Runtime.CardA, Runtime.CardB];
        string[] names = ["BASCUS-94455GAME", "BASCUS-94488GAME"];
        foreach (Hardware.MemoryCard card in cards)
        {
            if (!card.Enabled)
                continue;
            foreach (string name in names)
            {
                int firstBlock = card.Find(name);
                if (firstBlock == 0 || card.FileSize(firstBlock) != 0x8000)
                    continue;
                int[] chain = card.Chain(firstBlock);
                if (chain.Length != 4)
                    continue;

                var save = new byte[0x8000];
                for (int offset = 0; offset < save.Length; offset++)
                    save[offset] = card.ReadByte(chain, offset);
                uint storedCrc = BinaryPrimitives.ReadUInt32LittleEndian(
                    save.AsSpan(0x7E9C, 4));
                uint actualCrc = Gt2SaveCrc32(save.AsSpan(0, 0x7E9C));
                if (save[0] != (byte)'S' || save[1] != (byte)'C' ||
                    save[2] != 0x13 || save[3] != 4 ||
                    storedCrc != actualCrc)
                {
                    Console.Error.WriteLine(
                        $"[GT2-Save] rejected {name} from {card.Path}: " +
                        $"header={save[0]:X2}{save[1]:X2}{save[2]:X2}{save[3]:X2} " +
                        $"storedCrc=0x{storedCrc:X8} actualCrc=0x{actualCrc:X8}");
                    continue;
                }

                const uint workingData = 0x801C98E0u;
                for (int offset = 0; offset < 0x7C9C; offset++)
                    m.WriteU8(
                        workingData + (uint)offset,
                        save[0x200 + offset]);
                MirrorLoadedSettings(m, workingData + 0x48u, 0x800A6EECu);
                MirrorLoadedSettings(m, workingData + 0x9Au, 0x800A6F00u);

                Console.WriteLine(
                    $"[GT2-Save] loaded {name} from {card.Path}: " +
                    $"credits={m.ReadU32(workingData + 0x7C88u)} " +
                    $"garageCars={m.ReadU8(workingData + 0x3C74u)} " +
                    $"currentCar={m.ReadU8(workingData + 0x7C8Cu)} " +
                    $"crc32=0x{actualCrc:X8}");
                return;
            }
        }
        Console.WriteLine(
            "[GT2-Save] no valid NTSC-U save found; starting a new game");
    }

    static void MirrorLoadedSettings(IMemory m, uint source, uint destination)
    {
        for (uint offset = 0; offset < 20u; offset++)
            m.WriteU8(destination + offset, m.ReadU8(source + offset));
    }

    static uint Gt2SaveCrc32(ReadOnlySpan<byte> data)
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

    static void EnableExactTitleDisplay()
    {
        var gpu = Runtime.Gpu;
        if (gpu == null)
            return;
        const int hStart = 0x260;
        const int hEnd = hStart + 2550;
        gpu.WriteGp1(
            0x06000000u | ((uint)hEnd << 12) | (uint)hStart);
        gpu.WriteGp1(0x08000026u); // 512 horizontal, 480-line interlaced NTSC.
        RecompOne.Runtime.Hle.GpuHle.NotifyDisplay(
            gpu.DisplayX, gpu.DisplayY, UnifiedTitleWidth, UnifiedTitleHeight);
    }

    static void WriteTitleDescriptor(
        IMemory m, uint address, byte u, byte v,
        ushort width, ushort height, ushort tpage, ushort clut)
    {
        m.WriteU16(address, (ushort)(u | (v << 8)));
        m.WriteU16(address + 2u, clut);
        m.WriteU16(address + 4u, width);
        m.WriteU16(address + 6u, height);
        m.WriteU16(address + 8u, tpage);
        m.WriteU16(address + 10u, 0);
    }

    public static int UnifiedTitleSelectionValue(uint index)
    {
        return index switch
        {
            1u => 0, // Arcade Mode: intercepted by CommitUnifiedTitleSelection.
            2u => 0, // Gran Turismo: retail START GAME destination.
            3u => 1, // Retail Replay Theater destination.
            4u => 2, // Retail Option destination.
            _ => -1,
        };
    }

    public static int UnifiedTitleItemValidity(uint index) =>
        index is >= 1u and <= 4u ? 0 : -1;

    public static uint UnifiedTitleDescriptor(
        IMemory m, uint index, uint language)
    {
        if (index is >= 1u and <= 4u)
            return UnifiedTitleDescriptorBase + (index - 1u) * 12u;

        uint languageIndex = Math.Min(language, 6u);
        uint descriptorTable =
            m.ReadU32(0x8004BC5Cu + languageIndex * 4u);
        int label = (short)m.ReadU16(0x8004BC14u);
        return descriptorTable + (uint)Math.Max(0, label) * 12u;
    }

    public static uint UnifiedTitleDescriptorForState(
        uint index, uint selected) =>
        selected != 0u && index is >= 1u and <= 4u
            ? UnifiedTitleDescriptorBase + (index - 1u) * 12u
            : UnifiedTitleBlankDescriptor;

    public static void PositionUnifiedTitleItem(
        IMemory m, uint itemRecord, uint index)
    {
        (int x, int y) = index switch
        {
            // The retail list primitive applies a fixed (-44,-8) title-panel
            // origin adjustment. These anchors place the archive's unscaled
            // 140x28 rectangles at (124,284), (124,312), (264,284),
            // and (264,312), respectively.
            1u => (168, 292),
            2u => (168, 320),
            3u => (308, 292),
            4u => (308, 320),
            _ => (0, 0),
        };
        m.WriteU16(itemRecord + 4u, (ushort)x);
        m.WriteU16(itemRecord + 6u, (ushort)y);
    }

    public static void CommitUnifiedTitleSelection(
        IMemory m, uint index, uint titleState)
    {
        if (index == 1u)
        {
            // The native selector has already invoked sound effect 3. It also
            // commits its retail START GAME destination before this hook, so
            // leaving the Simulation title state machine alive for additional
            // frames can dispatch Simulation overlay 4 before the guest swap.
            // Switch at this exact confirmation boundary instead.
            _unifiedTitleMenuActive = false;
            _unifiedArcadeFrontendPending = true;
            _unifiedTitleConfirmationMixFrame =
                RecompOne.Runtime.Host.Audio.MixedFrameCount;
            _unifiedArcadeSelectionTimestamp = Stopwatch.GetTimestamp();
            _unifiedArcadeSelectionInputPoll = Host.InputManager.CurrentPoll;
            Dispatch.Dispatcher.BeginMethodProfile("arcade-menu-handoff");
            Console.WriteLine(
                "[GT2] title selection: Arcade Mode " +
                $"inputPoll={Host.InputManager.CurrentPoll}");
            throw new GT2VariantSwitch("arcade");
        }

        _unifiedTitleMenuActive = false;
        int value = UnifiedTitleSelectionValue(index);
        if (value < 0)
            return;
        if (index == 2u)
            Console.WriteLine(
                "[GT2] title selection: Gran Turismo Mode " +
                $"inputPoll={Host.InputManager.CurrentPoll}");
        else if (index == 3u)
            Console.WriteLine(
                "[GT2] title selection: Replay Theater " +
                $"inputPoll={Host.InputManager.CurrentPoll}");
        else if (index == 4u)
            Console.WriteLine(
                "[GT2] title selection: Option " +
                $"inputPoll={Host.InputManager.CurrentPoll}");
        m.WriteU8(titleState + 3u, (byte)value);
    }

    static long _vehicleLodRequests;
    static int _vehicleLodTraceRegistered;
    static int _forcedVehicleLodReported;
    static long _aiDriverTicks;
    static int _aiMemoryTraceReported;
    static int _aiAutoDriveReported;
    static int _aiAutoDriveRaceTicks;
    static int _aiAutoDriveQuickWinApplied;

    static int ParseAiAutoDriveMaxEngagements(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 2;
        if (!int.TryParse(text, out int value) || value is < 2 or > 64)
            throw new InvalidOperationException(
                "RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS must be from 2 through 64");
        return value;
    }

    static int ParseAiAutoDriveQuickWinAfterTicks(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;
        if (!int.TryParse(text, out int value) || value is < 1 or > 60_000)
            throw new InvalidOperationException(
                "RECOMPONE_GT2_SOAK_QUICK_WIN_AFTER_AI_TICKS must be from 1 through 60000");
        return value;
    }

    static uint ParseVehicleLodSelector(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;
        if (!uint.TryParse(text, out uint selector) ||
            selector is < 1u or > 3u)
            throw new InvalidOperationException(
                "RECOMPONE_GT2_VEHICLE_LOD_SELECTOR_OVERRIDE must be 1, 2, or 3");
        return selector;
    }

    /// <summary>
    /// GT2 stores a one-based forced vehicle-detail selector in the first byte
    /// of its render request. The common renderer subtracts one before indexing
    /// the model table, so selector 1 is model-table index 0 (highest detail).
    /// A diagnostic override is intentionally separate from wrapper settings so
    /// fixed-scene tests can isolate vehicle LOD from track-object LOD.
    /// </summary>
    public static uint GetForcedVehicleLodSelector()
    {
        bool maximum =
            Config.ConfigManager.View.LevelOfDetail.Equals(
                "Maximum", StringComparison.OrdinalIgnoreCase);
        uint selector = maximum ? 1u : DiagnosticVehicleLodSelector;
        if (selector != 0u &&
            Interlocked.Exchange(ref _forcedVehicleLodReported, 1) == 0)
            Console.Error.WriteLine(
                $"[GT2-LOD] vehicle selector forced={selector} " +
                $"model-index={selector - 1u} " +
                $"source={(maximum ? "wrapper Maximum" : "diagnostic override")}");
        return selector;
    }

    /// <summary>
    /// Mark the exact point at which the unified title begins its native
    /// initialization. Diagnostics use this stage to prove that input offered
    /// during the short authored fade is accepted instead of silently lost.
    /// </summary>
    public static void CompleteUnifiedTitleMenuInitialization(IMemory m)
    {
        _unifiedTitleBufferedPressed = 0;
        _unifiedTitleBufferedRepeated = 0;
        // State 0 is the retail title list's sixteen-update reveal, but its last
        // update also performs required native list finalization. The unified
        // screen is already composited in its final form, so reduce the reveal
        // countdown to one update rather than bypassing state 0. The first
        // advertised input poll then finalizes the list and consumes input in
        // that same update. Keep the buffer for input arriving earlier in boot.
        m.WriteU16(0x800B122Eu, 1);
        Host.InputManager.SignalScriptStage("unified_title");
    }

    /// <summary>
    /// Preserve input edges produced while the retail title controller is in
    /// its sixteen-update initialization state. Once that controller reaches
    /// its ordinary interactive state, replay the edges through GT2's own
    /// input record so its native list code handles navigation and selection.
    /// </summary>
    public static void BufferUnifiedTitleInput(uint input, IMemory m)
    {
        if (!_unifiedTitleMenuActive || !IsGuestRam(input))
            return;
        uint pressed = m.ReadU32(input + 4u);
        uint repeated = m.ReadU32(input + 0xCu);
        ushort state = m.ReadU16(0x800B122Cu);
        if (state == 0)
        {
            _unifiedTitleBufferedPressed |= pressed;
            _unifiedTitleBufferedRepeated |= repeated;
            return;
        }
        if (_unifiedTitleBufferedPressed == 0 &&
            _unifiedTitleBufferedRepeated == 0)
            return;
        m.WriteU32(
            input + 4u,
            pressed | _unifiedTitleBufferedPressed);
        m.WriteU32(
            input + 0xCu,
            repeated | _unifiedTitleBufferedRepeated);
        Console.WriteLine(
            "[GT2] buffered unified-title input accepted after native " +
            $"initialization: pressed=0x{_unifiedTitleBufferedPressed:X8} " +
            $"repeated=0x{_unifiedTitleBufferedRepeated:X8}");
        _unifiedTitleBufferedPressed = 0;
        _unifiedTitleBufferedRepeated = 0;
    }

    /// <summary>
    /// Brackets the common model renderer with GT2's owning race-car record.
    /// The renderer itself receives a camera/projection scratch pointer that
    /// every car reuses; treating that scratch address as object identity
    /// merges independent cars in native grouping and UV continuity.
    /// </summary>
    public static void BeginVehicleRenderIdentity(uint vehicleRecord)
    {
        if (!IsGuestRam(vehicleRecord))
            throw new InvalidOperationException(
                $"GT2 vehicle owner is outside guest RAM: 0x{vehicleRecord:X8}");
        if (_vehicleRenderIdentity != 0u)
            throw new InvalidOperationException(
                $"Nested GT2 vehicle owners: active=0x{_vehicleRenderIdentity:X8} " +
                $"new=0x{vehicleRecord:X8}");
        _vehicleRenderIdentity = vehicleRecord;
        if (AuditRenderer || TraceVehicleVisibility || TraceWheelTransforms)
        {
            _vehicleRenderIdentityBegins++;
            lock (VehicleRenderIdentities)
                VehicleRenderIdentities.Add(vehicleRecord);
        }
    }

    public static void EndVehicleRenderIdentity()
    {
        if (_vehicleRenderIdentity == 0u)
            throw new InvalidOperationException(
                "GT2 vehicle-owner scope ended without a matching begin");
        _vehicleRenderIdentity = 0u;
        if (AuditRenderer || TraceVehicleVisibility || TraceWheelTransforms)
            _vehicleRenderIdentityEnds++;
    }

    static uint ResolveVehicleRenderIdentity(uint renderScratch) =>
        _vehicleRenderIdentity != 0u
            ? _vehicleRenderIdentity
            : renderScratch;

    public static void TraceVehicleLodSelection(
        uint carState, uint renderRequest, IMemory m)
    {
        if (TraceWheelTransforms &&
            Interlocked.Exchange(
                ref _wheelTransformTraceEnabledReported, 1) == 0)
            Console.Error.WriteLine(
                "[GT2-WHEEL] guest transform trace enabled");
        uint selector = m.ReadU8(renderRequest);
        uint modelSet = m.ReadU32(renderRequest + 0xCu);
        uint modelPointer = 0;
        if (selector is >= 1u and <= 3u && IsGuestRam(modelSet))
        {
            uint modelIndex = selector - 1u;
            modelPointer =
                m.ReadU32(modelSet + 0x870u + modelIndex * 8u);
        }
        uint vehicleIdentity = ResolveVehicleRenderIdentity(carState);
        if (AuditRenderer || TraceVehicleVisibility || TraceWheelTransforms)
        {
            if (_vehicleRenderIdentity != 0u)
                _vehicleRenderScopedCaptures++;
            else
                _vehicleRenderFallbackCaptures++;
        }
        WorldCaptureContext.BeginVehicle(vehicleIdentity, modelPointer);

        if (!TraceVehicleLod && !AuditRenderer)
            return;

        lock (VehicleLodSelectorCounts)
        {
            _vehicleLodRequests++;
            VehicleLodSelectorCounts.TryGetValue(selector, out long count);
            VehicleLodSelectorCounts[selector] = count + 1;
            if (IsGuestRam(modelSet))
                VehicleLodModelSets.Add(modelSet);
            if (IsGuestRam(modelPointer))
                VehicleLodModelPointers.Add(modelPointer);
            if (TraceVehicleLod && !AuditRenderer &&
                _vehicleLodTraceRegistered == 0)
            {
                _vehicleLodTraceRegistered = 1;
                AppDomain.CurrentDomain.ProcessExit += (_, _) =>
                {
                    lock (VehicleLodSelectorCounts)
                    {
                        string selectors = string.Join(
                            ',',
                            VehicleLodSelectorCounts
                                .OrderBy(pair => pair.Key)
                                .Select(pair =>
                                    $"{pair.Key}:{pair.Value}"));
                        Console.Error.WriteLine(
                            $"[GT2-LOD] vehicle requests={_vehicleLodRequests} " +
                            $"selectors=[{selectors}] " +
                            $"modelSets={VehicleLodModelSets.Count} " +
                            $"selectedModelPointers={VehicleLodModelPointers.Count}");
                    }
                };
            }
        }
    }

    /// <summary>
    /// Reproduce the Simulation executable's authored BSS state without
    /// replaying its top-level boot when Arcade's Back action returns to the
    /// unified title.
    /// </summary>
    public static void PrepareSimulationTitleHandoff(
        CpuContext c, IMemory m)
    {
        const uint simulationBssStart = 0x801C93B0u;
        const uint simulationBssEnd = 0x801F0D60u;
        const uint saved = 0x80091144u;
        m.WriteU32(saved - 8u, c.A0);
        m.WriteU32(saved - 4u, c.A1);
        m.WriteU32(saved, c.S0);
        m.WriteU32(saved + 4u, c.S1);
        m.WriteU32(saved + 8u, c.S2);
        m.WriteU32(saved + 12u, c.S3);
        m.WriteU32(saved + 16u, c.S4);
        m.WriteU32(saved + 20u, c.S5);
        m.WriteU32(saved + 24u, c.S6);
        m.WriteU32(saved + 28u, c.S7);
        m.WriteU32(saved + 32u, c.GP);
        m.WriteU32(saved + 36u, c.SP);
        m.WriteU32(saved + 40u, c.FP);
        m.WriteU32(saved + 44u, c.RA);
        c.SP -= 0x18u;
        m.ZeroRange(
            simulationBssStart,
            simulationBssEnd - simulationBssStart);
        Console.WriteLine(
            "[GT2] Simulation title handoff prepared: native BSS " +
            "initialized; boot/legal overlays omitted");
    }

    /// <summary>
    /// Enter the original Simulation title through its post-bootstrap
    /// initializer, mirroring SCUS-94488 immediately before it requests
    /// overlay 1.
    /// </summary>
    public static void RunSimulationTitleHandoff(CpuContext c, IMemory m)
    {
        const string simulationPrefix = "gt2_overlay";
        string previousOverlayPrefix = _overlayPrefix;
        _overlayPrefix = simulationPrefix;
        try
        {
            c.RA = 0x8005D680u;
            Dispatch.Dispatcher.Call(c, m, 0x8008CE08u);
            c.A0 = m.ReadU32(0x8009113Cu);
            c.A1 = m.ReadU32(0x80091140u);
            c.RA = 0x8005D698u;
            Console.WriteLine(
                "[GT2] Simulation title handoff: " +
                "entry=0x8005D6E0 unified title overlay=1");
            RunGuestLoop(c, m, 0x8005D6E0u, simulationPrefix);
        }
        finally
        {
            _overlayPrefix = previousOverlayPrefix;
        }
    }

    /// <summary>
    /// Reads GT2's two depth-normalization results immediately after the
    /// selected vehicle model has configured the shared rendering scratchpad.
    /// The low word is the exponent used by vehicle OT insertion; the high word
    /// is the complementary normalization adjustment. Keeping this hook after
    /// the guest helper makes the trace an observation of authored state, not a
    /// host reconstruction of it.
    /// </summary>
    public static void CaptureVehicleDepthNormalization(
        uint modelPointer, IMemory m)
    {
        ushort commonShift = m.ReadU16(0x1F800098u);
        ushort normalizationAdjustment = m.ReadU16(0x1F80009Au);
        WorldCaptureContext.SetCurrentDepthScaleExponent(commonShift);
        int poll = Host.InputManager.CurrentPoll;
        if (poll != TraceDepthNormalizationPoll)
            return;
        WorldObjectContext context = WorldCaptureContext.Current;
        Console.Error.WriteLine(
            $"[GT2-Vehicle-Depth-State] poll={poll} " +
            $"vehicle=0x{context.StableId:X8} " +
            $"model=0x{(modelPointer != 0 ? modelPointer : context.ModelPointer):X8} " +
            $"commonShift={commonShift} " +
            $"normalizationAdjustment={normalizationAdjustment}");
    }

    /// <summary>
    /// Billboard packets are emitted by the guest after the resident mesh
    /// capture hook, and some track objects contain only that billboard path.
    /// Attach the same authored ordering-table normalization used by the
    /// corresponding raw-track decoder before any billboard packet is
    /// recorded. Primary and auxiliary formats use GT2's two equivalent
    /// normalization formulas.
    /// </summary>
    public static void CaptureTrackBillboardDepthNormalization(
        bool auxiliaryFormat,
        IMemory m)
    {
        ushort commonShift = m.ReadU16(0x1F800098u);
        ushort normalizationAdjustment = m.ReadU16(0x1F80009Au);
        int depthScaleExponent = auxiliaryFormat
            ? commonShift
            : 10 - normalizationAdjustment;
        WorldCaptureContext.SetCurrentDepthScaleExponent(depthScaleExponent);
    }

    /// <summary>
    /// Diagnostic-only trace of the exact view record and live GTE projection
    /// state at GT2's scene-pass boundaries. In the view record passed to the
    /// model renderers, OFX/OFY/H occupy +0x5C/+0x60/+0x64; render helpers copy
    /// the record starting at +8 into scratch, where the same values become
    /// +0x54/+0x58/+0x5C before func_8007B688 activates them.
    /// </summary>
    public static void TraceProjectionPhase(
        string phase, uint viewRecord, IMemory memory)
    {
        int poll = Host.InputManager.CurrentPoll;
        if (poll != TraceProjectionPhasePoll)
            return;
        if (!IsGuestRam(viewRecord))
        {
            Console.Error.WriteLine(
                $"[GT2-Projection-Phase] poll={poll} phase={phase} " +
                $"view=0x{viewRecord:X8} invalid=1");
            return;
        }
        ulong hash = 14695981039346656037UL;
        for (uint offset = 0x8u; offset < 0x70u; offset += 4u)
        {
            hash ^= memory.ReadU32(viewRecord + offset);
            hash *= 1099511628211UL;
        }
        Console.Error.WriteLine(
            $"[GT2-Projection-Phase] poll={poll} phase={phase} " +
            $"view=0x{viewRecord:X8} viewHash=0x{hash:X16} " +
            $"viewOfx=0x{memory.ReadU32(viewRecord + 0x5Cu):X8} " +
            $"viewOfy=0x{memory.ReadU32(viewRecord + 0x60u):X8} " +
            $"viewH={memory.ReadU16(viewRecord + 0x64u)} " +
            $"gteOfx=0x{RecompOne.Runtime.Gte.ReadControl(24):X8} " +
            $"gteOfy=0x{RecompOne.Runtime.Gte.ReadControl(25):X8} " +
            $"gteH={RecompOne.Runtime.Gte.ReadControl(26)}");
    }

    /// <summary>
    /// Removes GT2's authored 4:3 viewport rejection from the eight-corner
    /// bounding-box result. The authoritative clip routine (Arcade 0x8007B550,
    /// Simulation 0x8007B640) assigns bits 1/2 to screen X and bits 3/4 to
    /// screen Y in both halves of the result: the low half is the intersection
    /// used for whole-object rejection and the high half selects the guest
    /// polygon clipper. High bit 5 is also a clipper selector: it is set when
    /// any bounding corner saturates the GTE even if the model remains
    /// recoverable from captured model/view coordinates. Leaving it set routes
    /// a close vehicle through GT2's 4:3 polygon clipper after the X/Y bits are
    /// removed, which makes the car appear to be eaten away at the old side
    /// plane. D3D owns target-viewport and homogeneous clipping for continuous
    /// geometry, so clear all six high-half polygon-clip selectors. Retain low
    /// bit 0 (near/depth) and low bit 5 (unrecoverable whole-object projection
    /// failure); those low-half bits still control whole-object rejection and
    /// never dispatch the polygon clipper.
    /// </summary>
    public static uint ApplyModernVehicleViewportMask(uint mask, bool enabled)
    {
        const uint authoredViewportBits = 0x003F001Eu;
        return enabled ? mask & ~authoredViewportBits : mask;
    }

    /// <summary>
    /// True when the modern renderer, rather than GT2's 4:3 packet clipper,
    /// owns vehicle viewport clipping. This policy also applies to the
    /// standalone vehicle renderers used by alternate race and replay paths;
    /// those paths invoke the guest polygon clipper unconditionally and do
    /// not consume the high-half mask selectors used by the primary renderer.
    /// </summary>
    public static bool ModernVehicleViewportClippingEnabled =>
        Config.ConfigManager.View.ExtendedDrawDistance &&
        ExtendedVehicleFrustumOverride;

    public static uint ApplyModernVehicleViewportMask(uint mask) =>
        ApplyModernVehicleViewportMask(
            mask,
            ModernVehicleViewportClippingEnabled);

    public static uint ExpandVehicleFrustumMask(uint mask, uint carState)
    {
        const uint lowOtherBits = 0x00000019u;
        const uint highOtherBits = 0x00390000u;

        _vehicleFrustumRequests++;
        bool horizontalOutside = (mask & 0x00000006u) != 0;
        bool horizontalIntersecting = (mask & 0x00060000u) != 0;
        if (horizontalOutside)
            _vehicleFrustumHorizontalOutside++;
        if (horizontalIntersecting)
            _vehicleFrustumHorizontalIntersecting++;
        if (horizontalOutside && horizontalIntersecting)
            _vehicleFrustumHorizontalBoth++;
        else if (horizontalOutside)
            _vehicleFrustumHorizontalOutsideOnly++;
        else if (horizontalIntersecting)
            _vehicleFrustumHorizontalIntersectingOnly++;
        if ((mask & lowOtherBits) != 0)
            _vehicleFrustumOtherOutside++;
        if ((mask & highOtherBits) != 0)
            _vehicleFrustumOtherIntersecting++;

        bool enabled = ModernVehicleViewportClippingEnabled;
        uint expanded = ApplyModernVehicleViewportMask(mask, enabled);
        if (expanded != mask)
            _vehicleFrustumExpanded++;

        int poll = Host.InputManager.CurrentPoll;
        bool tracePoll =
            TraceVehicleVisibilityStartPoll < 0 ||
            (poll >= TraceVehicleVisibilityStartPoll &&
             poll <= TraceVehicleVisibilityEndPoll);
        if (TraceVehicleVisibility && tracePoll &&
            Interlocked.Increment(ref _vehicleFrustumTraceSamples) <=
                TraceVehicleVisibilityLimit)
            Console.Error.WriteLine(
                $"[GT2-VEHICLE-FRUSTUM] poll={poll} " +
                $"car=0x{carState:X8} " +
                $"stock=0x{mask:X8} modern=0x{expanded:X8} " +
                $"horizontalOutside={(mask & 0x6u) != 0} " +
                $"horizontalIntersecting={(mask & 0x00060000u) != 0} " +
                $"enabled={enabled}");
        return expanded;
    }

    /// <summary>
    /// Records the driver-mode dispatch and the small set of live fields that
    /// surround GT2's racing-line/controller handoff. This is intentionally
    /// read-only: the trace establishes which original AI path and outputs can
    /// safely be reused by the validation auto-drive harness.
    /// </summary>
    public static void TraceAiDriverTable(
        uint carArray, uint carCount, IMemory m)
    {
        if (!TraceAiDrivers ||
            !IsGuestRam(carArray) ||
            carCount is < 1u or > 16u)
            return;

        long tick = Interlocked.Increment(ref _aiDriverTicks);
        if (!string.IsNullOrWhiteSpace(AiMemoryTracePath) &&
            Interlocked.Exchange(ref _aiMemoryTraceReported, 1) == 0)
        {
            const uint start = 0x80000000u;
            const int length = 0x200000;
            byte[] snapshot = new byte[length];
            for (int offset = 0; offset < snapshot.Length; offset++)
                snapshot[offset] = m.ReadU8(start + (uint)offset);
            string fullPath = Path.GetFullPath(AiMemoryTracePath);
            string? directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllBytes(fullPath, snapshot);
            Console.Error.WriteLine(
                "[GT2-AI] captured first-dispatch memory " +
                $"address=0x{start:X8} bytes={snapshot.Length} " +
                $"sha256={Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(snapshot))} " +
                $"path={fullPath}");
        }
        if (tick > 12 && tick % 120 != 0)
            return;

        for (uint index = 0; index < carCount; index++)
        {
            uint car = carArray + index * 0xB40u + 0x2Cu;
            int mode = (sbyte)m.ReadU8(car + 0x45Du);
            Console.Error.WriteLine(
                $"[GT2-AI] poll={Host.InputManager.CurrentPoll} " +
                $"tick={tick} car={index} mode={mode} " +
                $"line={m.ReadU16(car + 0x610u)} " +
                $"lineNext={m.ReadU16(car + 0x612u)} " +
                $"speedTarget={m.ReadU16(car + 0x640u)} " +
                $"speed={m.ReadU32(car + 0x64Cu)} " +
                $"progress={m.ReadU16(car + 0x6FEu)} " +
                $"route=0x{m.ReadU32(car + 0x624u):X8} " +
                $"param10C=0x{m.ReadU32(car + 0x10Cu):X8} " +
                $"control0={m.ReadU16(car + 0x708u)} " +
                $"control1={m.ReadU32(car + 0x710u)} " +
                $"control2={m.ReadU32(car + 0x714u)} " +
                $"flags={m.ReadU8(car + 0x718u):X2}");
        }
    }

    /// <summary>
    /// Diagnostic race harness: route the human-controlled car through GT2's
    /// original per-car AI driver (mode 2). The player keeps its own complete
    /// vehicle/upgrade/physics object; only the native driver dispatch changes.
    /// </summary>
    public static void ApplyAiAutoDrive(
        uint carArray, uint carCount, IMemory m)
    {
        if (!AiAutoDrive ||
            !IsGuestRam(carArray) ||
            carCount is < 1u or > 16u)
            return;

        // GT2 records the human pad stream, not the output of a mode-2 driver.
        // Permit one engagement for the live race and one when replay rebuilds
        // the player car. The default two-pass ceiling prevents a later
        // menu/race object that reuses mode 0 from being touched. Long,
        // explicitly opted-in soak sessions can raise the ceiling for
        // subsequent championship races and replays.
        int engagement = Volatile.Read(ref _aiAutoDriveReported);
        if (AiAutoDriveQuickWinAfterTicks > 0 &&
            engagement > 0 &&
            engagement % 2 == 1)
        {
            int ticks = Interlocked.Increment(ref _aiAutoDriveRaceTicks);
            if (ticks >= AiAutoDriveQuickWinAfterTicks &&
                Interlocked.Exchange(ref _aiAutoDriveQuickWinApplied, 1) == 0)
            {
                // NTSC-U v1.2 live-race working data. This test-only transition
                // is equivalent to the established quick-win diagnostic: it
                // leaves boot, loading, vehicle setup, physics, and the grace
                // window untouched, then asks the original race flow to finish.
                m.WriteU8(0x801D586Bu, 1);
                if (m.ReadU16(0x800A9CBCu) == 0)
                    m.WriteU16(0x800A9CBCu, 1);
                m.WriteU16(0x801D5944u, 1);
                m.WriteU16(0x801D5D54u, 0x0500);
                Console.Error.WriteLine(
                    $"[GT2-Soak] quick-win transition applied after {ticks} AI ticks " +
                    $"engagement={engagement}");
            }
        }
        if (engagement >= AiAutoDriveMaxEngagements)
            return;

        for (uint index = 0; index < carCount; index++)
        {
            uint car = carArray + index * 0xB40u + 0x2Cu;
            if ((sbyte)m.ReadU8(car + 0x45Du) != 0)
                continue;

            m.WriteU8(car + 0x45Du, 2);
            int pass = Interlocked.Increment(ref _aiAutoDriveReported);
            string phase = pass % 2 == 1 ? "race" : "replay";
            if (pass % 2 == 1)
            {
                Interlocked.Exchange(ref _aiAutoDriveRaceTicks, 0);
                Interlocked.Exchange(ref _aiAutoDriveQuickWinApplied, 0);
            }
            int session = (pass + 1) / 2;
            Host.InputManager.SignalScriptStage($"{phase}_{session}");
            Console.Error.WriteLine(
                $"[GT2-AI] auto-drive engaged pass={pass}/{AiAutoDriveMaxEngagements} " +
                $"phase={phase} car={index}: " +
                "native driver mode 0 -> 2; vehicle physics unchanged");
            return;
        }
    }

    public static void TraceTrackRenderRequest(CpuContext c, IMemory m)
    {
        if (!TraceTrackRendering && !AuditRenderer)
            return;

        lock (TrackObjects)
        {
            _trackRenderRequests++;
            TrackObjects.Add(c.A0);
            TrackViewIndices.Add(c.A1);
            TrackMeshIndices.Add(c.A2);
            TrackPasses.Add(c.A3);
            int selector = (int)c.A3;
            ReplayTrackSelectorCounts.TryGetValue(
                selector, out long selectorCount);
            ReplayTrackSelectorCounts[selector] = selectorCount + 1;

            ushort modelIndex = 0;
            uint modelPointer = 0;
            uint modelId = 0;
            if (IsGuestRam(c.A0))
            {
                modelIndex = m.ReadU16(c.A0 + 0x10u);
                uint tableEntry = unchecked(
                    (uint)(0x800520E8L + (long)selector * 4L));
                if (IsGuestRam(tableEntry))
                {
                    uint table = m.ReadU32(tableEntry);
                    uint modelEntry = unchecked(
                        table + (uint)modelIndex * 4u);
                    if (IsGuestRam(table) && IsGuestRam(modelEntry))
                    {
                        modelPointer = m.ReadU32(modelEntry);
                        if (IsGuestRam(modelPointer))
                        {
                            ReplayTrackModelPointers.Add(modelPointer);
                            modelId = DecodeTrackModelId(m, modelPointer);
                            ReplayTrackModelIds.Add(modelId);
                        }
                    }
                }
            }
            if (_trackRenderSamples++ < 240)
                Console.Error.WriteLine(
                    $"[GT2-Track-LOD] poll={Host.InputManager.CurrentPoll} " +
                    $"object=0x{c.A0:X8} view={c.A1} mesh={c.A2} " +
                    $"selector={selector} modelIndex={modelIndex} " +
                    $"model=0x{modelPointer:X8} id=0x{modelId:X8}");
            if (!_trackExitTraceRegistered)
            {
                _trackExitTraceRegistered = true;
                AppDomain.CurrentDomain.ProcessExit += (_, _) =>
                {
                    lock (TrackObjects)
                        Console.Error.WriteLine(
                            $"[GT2-Track] requests={_trackRenderRequests} " +
                            $"objects={TrackObjects.Count} " +
                            $"viewIndices={TrackViewIndices.Count} " +
                            $"meshIndices={TrackMeshIndices.Count} " +
                            $"passes={TrackPasses.Count} " +
                            $"viewRange={Range(TrackViewIndices)} " +
                            $"meshRange={Range(TrackMeshIndices)} " +
                            $"passRange={Range(TrackPasses)} " +
                            $"selectors=[{string.Join(
                                ',',
                                ReplayTrackSelectorCounts
                                    .OrderBy(pair => pair.Key)
                                    .Select(pair =>
                                        $"{pair.Key}:{pair.Value}"))}] " +
                            $"modelPointers={ReplayTrackModelPointers.Count} " +
                            $"modelIds={ReplayTrackModelIds.Count}");
                };
            }
        }
    }

    static uint DecodeTrackModelId(IMemory m, uint pointer)
    {
        const uint lookup = 0x801EF630u;
        uint result = 0;
        for (int index = 0; index < 5; index++)
        {
            int offset = (sbyte)m.ReadU8(pointer + (uint)index);
            uint value = m.ReadU8(unchecked(lookup + (uint)offset));
            result |= value << ((4 - index) * 6);
        }
        return result;
    }

    static string Range(HashSet<uint> values)
    {
        if (values.Count == 0)
            return "none";
        return $"{values.Min():X8}-{values.Max():X8}";
    }

    public static void TraceTrackVisibility(CpuContext c, IMemory m)
    {
        if (!TraceTrackRendering && !TraceTrackFrustum && !AuditRenderer)
            return;

        uint renderState = c.A0;
        uint cameraState = c.A1;
        uint trackRoot = m.ReadU32(cameraState + 0x10u);
        uint sector = m.ReadU32(renderState + 0xA0u);
        uint sectorEntry = m.ReadU32(trackRoot + sector * 4u + 0xCu);
        uint visibilityList = sectorEntry == 0
            ? 0
            : m.ReadU32(sectorEntry + 0xA0u);

        if (TraceTrackFrustum && _trackFrustumSamples++ < 16)
        {
            Console.Error.WriteLine(
                $"[GT2-Frustum] poll={Host.InputManager.CurrentPoll} " +
                $"render=0x{renderState:X8} mode={c.A2} " +
                $"translation=({unchecked((int)m.ReadU32(renderState + 0x54u))}," +
                $"{unchecked((int)m.ReadU32(renderState + 0x58u))}," +
                $"{m.ReadU16(renderState + 0x5Cu)}) " +
                $"projection=({(short)m.ReadU16(renderState + 0x5Eu)}," +
                $"{(short)m.ReadU16(renderState + 0x60u)}," +
                $"{(short)m.ReadU16(renderState + 0x62u)}) " +
                $"display=({m.ReadU16(renderState + 0x98u)}," +
                $"{m.ReadU16(renderState + 0x9Au)})");
        }

        lock (TrackVisibilitySectors)
        {
            _trackVisibilityFunctionCalls++;
            if (Volatile.Read(ref _aiAutoDriveReported) >= 2)
                _trackVisibilityReplayCalls++;
            else
                _trackVisibilityRaceCalls++;
            if (IsGuestRam(visibilityList))
            {
                int rawCount = Math.Min(
                    (int)m.ReadU16(visibilityList), 0x4000);
                for (int index = 0; index < rawCount; index++)
                {
                    ushort item = m.ReadU16(
                        visibilityList + 2u + (uint)index * 2u);
                    _trackVisibilityRawEntries++;
                    if ((item & 0xC000) != 0)
                        _trackVisibilityRawNonzeroSelectors++;
                }
            }
            if (!TraceTrackRendering)
                return;
            if (_trackVisibilitySamples++ < 120)
                Console.Error.WriteLine(
                    $"[GT2-Render-Cadence] " +
                    $"poll={Host.InputManager.CurrentPoll} " +
                    $"mode={c.A2} sector={sector}");
            if (!TrackVisibilitySectors.Add(sector))
                return;
            ushort count = visibilityList == 0
                ? (ushort)0
                : m.ReadU16(visibilityList);
            string items = visibilityList == 0
                ? ""
                : string.Join(
                    ',',
                    Enumerable.Range(0, Math.Min(12, (int)count))
                        .Select(index =>
                            $"{m.ReadU16(visibilityList + 2u + (uint)index * 2u):X4}"));
            Console.Error.WriteLine(
                $"[GT2-Visibility] render=0x{renderState:X8} " +
                $"camera=0x{cameraState:X8} mode={c.A2} " +
                $"root=0x{trackRoot:X8} sector={sector} " +
                $"entry=0x{sectorEntry:X8} list=0x{visibilityList:X8} " +
                $"count={count} items=[{items}]");
        }
    }

    /// <summary>
    /// Overlay 0 renders the packed visibility list authored for the camera's
    /// current track sector. Those lists are potential-visibility sets, not
    /// independent slices of a global object catalog: combining every sector
    /// exposes mutually exclusive, occluded, and visual-proxy road surfaces.
    /// Extended draw distance keeps the current authored ordering first and
    /// adds only a bounded four-sector horizon in both directions. This
    /// preloads upcoming geometry beyond Seattle's long sightlines without
    /// drawing the far side of the loop. Maximum LOD clears selector bits
    /// while deduplicating, so one object index cannot submit competing LOD
    /// variants from the selected sector lists. Modern frustum and depth
    /// policy apply after this authored coarse selection.
    /// </summary>
    public static uint GetTrackVisibilityList(
        IMemory m, uint trackRoot, uint stockList)
    {
#if !OPENGT_RELEASE_PACKAGE
        if (!ResidentCourseCatalogEnabled)
            return stockList;
#endif
        bool extended =
            Config.ConfigManager.View.ExtendedDrawDistance;
        bool maximumLod =
            Config.ConfigManager.View.LevelOfDetail.Equals(
                "Maximum", StringComparison.OrdinalIgnoreCase);
        if (!extended && !maximumLod)
        {
            TraceTrackVisibilityLod(m, stockList, maximumLod);
            return stockList;
        }

        if (!IsGuestRam(stockList))
        {
#if OPENGT_RELEASE_PACKAGE
            throw new InvalidDataException(
                "The required resident course visibility list is outside " +
                $"guest RAM: root=0x{trackRoot:X8} list=0x{stockList:X8}. " +
                "The release build has no stock-sector fallback.");
#else
            TraceTrackVisibilityLod(m, stockList, maximumLod);
            return stockList;
#endif
        }
        int stockCount = Math.Min((int)m.ReadU16(stockList), 0x4000);
        int visibleCount;
        if (extended)
        {
            if (!TryLocateVisibilitySector(
                    m,
                    trackRoot,
                    stockList,
                    out int sectorCount,
                    out int currentSector))
            {
#if OPENGT_RELEASE_PACKAGE
                throw new InvalidDataException(
                    "The required resident course horizon could not locate " +
                    $"the stock sector: root=0x{trackRoot:X8} " +
                    $"list=0x{stockList:X8}. The release build has no " +
                    "stock-sector fallback.");
#else
                TraceTrackVisibilityLod(m, stockList, maximumLod);
                return stockList;
#endif
            }
            visibleCount = BuildExtendedVisibilityList(
                m,
                trackRoot,
                stockList,
                stockCount,
                sectorCount,
                currentSector,
                maximumLod);
            int tracePoll = Host.InputManager.CurrentPoll;
            if (TraceTrackVisibilityStartPoll >= 0 &&
                tracePoll >= TraceTrackVisibilityStartPoll &&
                tracePoll <= Math.Max(
                    TraceTrackVisibilityStartPoll,
                    TraceTrackVisibilityEndPoll))
            {
                string stockObjects = string.Join(
                    ',',
                    Enumerable.Range(0, stockCount)
                        .Select(index => m.ReadU16(
                            stockList + 2u + (uint)index * 2u))
                        .Select(entry => $"{entry:X4}"));
                string expandedObjects = string.Join(
                    ',',
                    ExpandedVisibilityEntries
                        .AsSpan(0, visibleCount)
                        .ToArray()
                        .Select(entry => $"{entry:X4}"));
                Console.Error.WriteLine(
                    $"[GT2-Visibility-Exact] poll={tracePoll} " +
                    $"root=0x{trackRoot:X8} stock=0x{stockList:X8} " +
                    $"sector={currentSector}/{sectorCount} " +
                    $"stockCount={stockCount} expandedCount={visibleCount} " +
                    $"stock=[{stockObjects}] expanded=[{expandedObjects}]");
            }
            _visibilityExpandedCalls++;
            _visibilityExpandedStockEntries += stockCount;
            _visibilityExpandedOutputEntries += visibleCount;
            _visibilityExpandedMaximumAdded = Math.Max(
                _visibilityExpandedMaximumAdded,
                visibleCount - stockCount);
            if (AuditRenderer)
                AuditVisibilityTransition(
                    m,
                    trackRoot,
                    stockList,
                    stockCount,
                    currentSector,
                    visibleCount);
        }
        else
        {
            visibleCount = stockCount;
            for (int item = 0; item < visibleCount; item++)
            {
                ushort entry = m.ReadU16(
                    stockList + 2u + (uint)item * 2u);
                ExpandedVisibilityEntries[item] = maximumLod
                    ? (ushort)(entry & 0x3FFF)
                    : entry;
            }
        }

        uint output = ExpandedVisibilityListAddress;
        m.WriteU16(output, (ushort)visibleCount);
        for (int item = 0; item < visibleCount; item++)
            m.WriteU16(
                output + 2u + (uint)item * 2u,
                ExpandedVisibilityEntries[item]);
        TraceTrackVisibilityLod(m, output, maximumLod);
        return output;
    }

    public static void TraceTrackTransformSetup(IMemory m, uint model, uint camera)
    {
#if !OPENGT_RELEASE_PACKAGE
        int poll = Host.InputManager.CurrentPoll;
        if (!TraceTrackRendering || TraceTrackVisibilityStartPoll < 0 ||
            poll < TraceTrackVisibilityStartPoll ||
            poll > Math.Max(TraceTrackVisibilityStartPoll, TraceTrackVisibilityEndPoll))
            return;
        uint x = m.ReadU32(model + 0x30u), y = m.ReadU32(model + 0x38u), z = m.ReadU32(model + 0x34u);
        uint cx = m.ReadU32(camera + 0x20u), cy = m.ReadU32(camera + 0x28u), cz = m.ReadU32(camera + 0x24u);
        int ix = unchecked((int)((x & 0xFFC00000u) + cx)) >> 10;
        int iy = unchecked((int)((y & 0xFFC00000u) + cy)) >> 10;
        int iz = unchecked((int)((z & 0xFFC00000u) + cz)) >> 10;
        Console.Error.WriteLine($"[GT2-Track-Translation] poll={poll} model=0x{model:X8} " +
            $"center={(int)x}/{(int)y}/{(int)z} camera={(int)cx}/{(int)cy}/{(int)cz} " +
            $"input={ix}/{iy}/{iz} narrowed={(short)ix}/{(short)iy}/{(short)iz}");
#endif
    }

    static bool TryLocateVisibilitySector(
        IMemory m,
        uint trackRoot,
        uint stockList,
        out int sectorCount,
        out int currentSector)
    {
        sectorCount = 0;
        currentSector = -1;
        if (!IsGuestRam(trackRoot))
            return false;

        uint tableStart = trackRoot + 0xCu;
        uint firstDescriptor = m.ReadU32(tableStart);
        uint tableBytes = unchecked(firstDescriptor - tableStart);
        if (!IsGuestRam(firstDescriptor) ||
            firstDescriptor <= tableStart ||
            (tableBytes & 3u) != 0)
            return false;
        sectorCount = checked((int)(tableBytes / 4u));
        if (sectorCount is < 1 or > 0x1000)
            return false;

        for (int sector = 0; sector < sectorCount; sector++)
        {
            uint descriptor = m.ReadU32(
                tableStart + (uint)sector * 4u);
            if (!IsGuestRam(descriptor))
                return false;
            if (m.ReadU32(descriptor + 0xA0u) == stockList)
                currentSector = sector;
        }
        return currentSector >= 0;
    }

    static int BuildExtendedVisibilityList(
        IMemory m,
        uint trackRoot,
        uint stockList,
        int stockCount,
        int sectorCount,
        int currentSector,
        bool maximumLod)
    {
        int generation = unchecked(++_visibilityEntryGeneration);
        if (generation == 0)
        {
            Array.Clear(VisibilityEntryGenerations);
            generation = ++_visibilityEntryGeneration;
        }
        int outputCount = 0;

        void AddList(uint list, int count)
        {
            for (int item = 0; item < count; item++)
            {
                ushort entry = m.ReadU16(
                    list + 2u + (uint)item * 2u);
                int objectIndex = entry & 0x3FFF;
                ushort outputEntry = maximumLod
                    ? (ushort)objectIndex
                    : entry;
                if (VisibilityEntryGenerations[objectIndex] != generation)
                {
                    if (outputCount >= ExpandedVisibilityEntries.Length)
                        return;
                    VisibilityEntryGenerations[objectIndex] = generation;
                    VisibilityEntryPositions[objectIndex] = outputCount;
                    ExpandedVisibilityEntries[outputCount++] = outputEntry;
                }
                else if (!maximumLod)
                {
                    int position = VisibilityEntryPositions[objectIndex];
                    if ((outputEntry >> 14) <
                        (ExpandedVisibilityEntries[position] >> 14))
                        ExpandedVisibilityEntries[position] = outputEntry;
                }
            }
        }

        AddList(stockList, stockCount);
        uint tableStart = trackRoot + 0xCu;
        // Walk outward from the current sector so nearby authored ordering is
        // retained. Do not union the full looping course: distant sector lists
        // contain mutually exclusive and occluded visual surfaces which are
        // invalid from the current camera region.
        for (int distance = 1;
             distance <= ExtendedVisibilitySectorRadius;
             distance++)
        {
            int forward = (currentSector + distance) % sectorCount;
            int backward =
                (currentSector - distance + sectorCount) % sectorCount;
            AddSector(forward);
            if (backward != forward)
                AddSector(backward);
        }
        return outputCount;

        void AddSector(int sector)
        {
            uint descriptor = m.ReadU32(
                tableStart + (uint)sector * 4u);
            if (!IsGuestRam(descriptor))
                return;
            uint list = m.ReadU32(descriptor + 0xA0u);
            if (!IsGuestRam(list))
                return;
            int count = m.ReadU16(list);
            if (count is < 1 or > 0x400)
                return;
            AddList(list, count);
        }
    }

    /// <summary>
    /// One-poll diagnostic for locating GT2's fixed-step vehicle accumulators.
    /// Each call compares the first car's complete native object with the
    /// preceding stage. It is read-only and inactive unless an exact input poll
    /// is selected through RECOMPONE_TRACE_GT2_TRUE60_STATE_POLL.
    /// </summary>
    public static void TraceTrue60HzVehicleStage(
        string stage, uint carArray, uint carCount, IMemory m)
    {
        int poll = Host.InputManager.CurrentPoll;
        if (True60HzStateTracePoll == -2)
        {
            if (stage == "begin" && poll is >= 4800 and <= 4920 &&
                IsGuestRam(carArray) && carCount is >= 1u and <= 16u)
            {
                uint traceCar = carArray + 0x2Cu;
                Console.Error.WriteLine(
                    $"[GT2-True60-Velocity] poll={poll} " +
                    $"v=({unchecked((int)m.ReadU32(traceCar + 0x65Cu))}," +
                    $"{unchecked((int)m.ReadU32(traceCar + 0x660u))}," +
                    $"{unchecked((int)m.ReadU32(traceCar + 0x664u))}) " +
                    $"longitudinal={unchecked((int)m.ReadU32(traceCar + 0x64Cu))}");
            }
            return;
        }

        if (poll != True60HzStateTracePoll ||
            !IsGuestRam(carArray) ||
            carCount is < 1u or > 16u)
            return;

        const int carBytes = 0xB40;
        const int wordCount = carBytes / sizeof(uint);
        uint car = carArray;

        // A stage hook inside a per-car loop can observe every vehicle. The
        // begin hook establishes the first car as the diagnostic target; do
        // not reset the comparison snapshot when later loop iterations pass a
        // different car.
        if (stage != "begin" &&
            _true60HzStateSnapshot is not null &&
            _true60HzStateCar != car)
            return;

        uint[] current = new uint[wordCount];
        for (int word = 0; word < wordCount; word++)
            current[word] = m.ReadU32(car + (uint)(word * sizeof(uint)));

        if (_true60HzStateSnapshot is null ||
            _true60HzStateCar != car ||
            stage == "begin")
        {
            _true60HzStateSnapshot = current;
            _true60HzStateCar = car;
            System.Text.StringBuilder words = new(wordCount * 9);
            for (int word = 0; word < wordCount; word++)
            {
                if (word != 0)
                    words.Append(',');
                words.Append(current[word].ToString("X8"));
            }
            Console.Error.WriteLine(
                $"[GT2-True60-State] poll={True60HzStateTracePoll} " +
                $"stage={stage} car=0x{car:X8} words={words}");
            return;
        }

        List<string> changes = [];
        for (int word = 0; word < wordCount; word++)
        {
            uint before = _true60HzStateSnapshot[word];
            uint after = current[word];
            if (before == after)
                continue;

            changes.Add(
                $"+0x{word * sizeof(uint):X3}:" +
                $"{unchecked((int)before)}->{unchecked((int)after)}" +
                $"(d={unchecked((int)(after - before))})");
        }

        Console.Error.WriteLine(
            $"[GT2-True60-State] poll={True60HzStateTracePoll} " +
            $"stage={stage} changes={changes.Count} " +
            string.Join(' ', changes));
        _true60HzStateSnapshot = current;
    }

    static int WheelAngle(uint address, IMemory m) =>
        m.ReadU16(address) & 0x0FFF;

    static int ShortestWheelAngleDelta(int current, int previous) =>
        ((current - previous + 0x800) & 0x0FFF) - 0x800;

    /// <summary>
    /// Samples the exact per-wheel transform record consumed by GT2's native
    /// wheel primitive renderer. This hook is deliberately placed before the
    /// guest Euler-to-matrix conversion, so its output cannot be affected by
    /// native scene capture, transform fitting, interpolation, or D3D drawing.
    /// </summary>
    public static void TraceWheelTransform(
        uint carState, uint wheelRecord, uint wheelIndex, IMemory m)
    {
        if (!TraceWheelTransforms)
            return;
        if (wheelIndex > 3u)
        {
            if (Interlocked.Exchange(
                    ref _wheelTransformTraceInvalidIndexReported, 1) == 0)
                Console.Error.WriteLine(
                    $"[GT2-WHEEL] rejected invalid wheel index " +
                    $"{wheelIndex} record=0x{wheelRecord:X8}");
            return;
        }

        int[] angles =
        [
            WheelAngle(wheelRecord + 0x8u, m),
            WheelAngle(wheelRecord + 0xAu, m),
            WheelAngle(wheelRecord + 0xCu, m),
        ];
        short x = (short)m.ReadU16(wheelRecord);
        short y = (short)m.ReadU16(wheelRecord + 0x2u);
        short z = (short)m.ReadU16(wheelRecord + 0x4u);
        carState = ResolveVehicleRenderIdentity(carState);
        ulong key = ((ulong)carState << 2) | wheelIndex;

        lock (WheelTransformTraceLock)
        {
            if (!_wheelTransformTraceExitRegistered)
            {
                _wheelTransformTraceExitRegistered = true;
                WheelTransformTraceCsv.AppendLine(
                    "car_state,wheel,sample,x,y,z,angle_8,angle_a,angle_c," +
                    "delta_8,delta_a,delta_c");
                AppDomain.CurrentDomain.ProcessExit += (_, _) =>
                    FlushWheelTransformTrace();
            }

            if (!WheelTransformTraceStates.TryGetValue(
                    key, out WheelTransformTraceState? state))
            {
                state = new WheelTransformTraceState();
                WheelTransformTraceStates.Add(key, state);
            }

            int[] deltas = [0, 0, 0];
            if (state.Samples != 0)
            {
                for (int axis = 0; axis < 3; ++axis)
                {
                    int delta = ShortestWheelAngleDelta(
                        angles[axis], state.Previous[axis]);
                    deltas[axis] = delta;
                    if (delta == 0)
                        continue;
                    ++state.Changes[axis];
                    state.MinimumShortestDelta[axis] = Math.Min(
                        state.MinimumShortestDelta[axis], delta);
                    state.MaximumShortestDelta[axis] = Math.Max(
                        state.MaximumShortestDelta[axis], delta);
                }
            }

            ++state.Samples;
            WheelTransformTraceCsv.Append(
                $"0x{carState:X8},{wheelIndex},{state.Samples}," +
                $"{x},{y},{z},{angles[0]},{angles[1]},{angles[2]}," +
                $"{deltas[0]},{deltas[1]},{deltas[2]}\n");
            _wheelTransformTraceRecords++;
            Array.Copy(angles, state.Previous, angles.Length);
        }
    }

    static void FlushWheelTransformTrace()
    {
        lock (WheelTransformTraceLock)
        {
            if (!_wheelTransformTraceExitRegistered ||
                _wheelTransformTraceFlushed)
                return;
            _wheelTransformTraceFlushed = true;

            if (!string.IsNullOrWhiteSpace(WheelTransformTracePath))
            {
                string path = Path.GetFullPath(WheelTransformTracePath);
                string? directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);
                File.WriteAllText(path, WheelTransformTraceCsv.ToString());
                Console.Error.WriteLine(
                    $"[GT2-WHEEL] wrote transform trace path={path} " +
                    $"records={_wheelTransformTraceRecords}");
            }
            foreach (var pair in WheelTransformTraceStates)
            {
                uint tracedCar = (uint)(pair.Key >> 2);
                uint tracedWheel = (uint)(pair.Key & 3u);
                WheelTransformTraceState state = pair.Value;
                string Axis(int index) =>
                    state.Changes[index] == 0
                        ? "static"
                        : $"changes={state.Changes[index]} " +
                          $"shortest=[{state.MinimumShortestDelta[index]}," +
                          $"{state.MaximumShortestDelta[index]}]";
                Console.Error.WriteLine(
                    $"[GT2-WHEEL] car=0x{tracedCar:X8} " +
                    $"wheel={tracedWheel} samples={state.Samples} " +
                    $"angle8({Axis(0)}) angleA({Axis(1)}) " +
                    $"angleC({Axis(2)})");
            }
        }
    }

    /// <summary>
    /// Records GT2's distance result immediately after either vehicle-body
    /// renderer returns, before the guest applies its render-mode gate. The
    /// guest result is recorded separately from the equivalent host predicate
    /// so this diagnostic cannot infer a branch that the guest did not take.
    /// </summary>
    public static void TraceVehicleWheelGate(
        uint carState,
        uint distance,
        uint renderMode,
        uint guestDistanceResult)
    {
        bool guestDistanceEligible = guestDistanceResult != 0u;
        bool computedDistanceEligible = distance < 0x2400u;
        bool modeEligible = (int)renderMode < 3;
        _wheelGateRequests++;
        if (guestDistanceEligible)
            _wheelGateGuestDistanceEligible++;
        if (computedDistanceEligible)
            _wheelGateComputedDistanceEligible++;
        if (modeEligible)
            _wheelGateModeEligible++;
        if (guestDistanceEligible != computedDistanceEligible)
            _wheelGateMismatches++;

        if ((!TraceWheelTransforms && !TraceVehicleVisibility) ||
            Interlocked.Increment(ref _wheelGateTraceSamples) > 128)
            return;
        Console.Error.WriteLine(
            $"[GT2-WHEEL-GATE] car=0x{ResolveVehicleRenderIdentity(carState):X8} " +
            $"distance=0x{distance:X} mode={renderMode} " +
            $"guestDistanceEligible={guestDistanceEligible} " +
            $"computedDistanceEligible={computedDistanceEligible} " +
            $"modeEligible={modeEligible}");
    }

    /// <summary>
    /// Records the path after GT2 has passed both wheel gates and selected a
    /// detail tier, immediately before the first of four wheel-renderer calls.
    /// One dispatch request therefore represents exactly four expected entry
    /// calls unless the generated call sequence itself is interrupted.
    /// </summary>
    public static void TraceVehicleWheelDispatch(
        uint carState, uint distance, uint renderMode, uint detail)
    {
        _wheelDispatchRequests++;
        if (distance < 0x240u)
            _wheelDispatchNearDetail++;
        else if (distance < 0x900u)
            _wheelDispatchMiddleDetail++;
        else
            _wheelDispatchFarDetail++;

        if ((!TraceWheelTransforms && !TraceVehicleVisibility) ||
            Interlocked.Increment(ref _wheelDispatchTraceSamples) > 128)
            return;
        Console.Error.WriteLine(
            $"[GT2-WHEEL-DISPATCH] car=0x{ResolveVehicleRenderIdentity(carState):X8} " +
            $"distance=0x{distance:X} mode={renderMode} " +
            $"detail={detail}");
    }

    /// <summary>
    /// Records entry into GT2's standalone wheel renderer before its packet-
    /// capacity rejection or transform setup. This is deliberately separate
    /// from transform sampling, whose optional CSV path must not be used as a
    /// proxy for control-flow evidence.
    /// </summary>
    public static void TraceVehicleWheelRendererEntry(
        uint carState, uint wheelRecord, uint wheelIndex, uint detail)
    {
        _wheelRendererEntries++;
        if (detail == 0u)
            _wheelRendererNearDetail++;
        else if (detail == 1u)
            _wheelRendererMiddleDetail++;
        else
            _wheelRendererFarDetail++;

        if ((!TraceWheelTransforms && !TraceVehicleVisibility) ||
            Interlocked.Increment(ref _wheelRendererEntryTraceSamples) > 128)
            return;
        Console.Error.WriteLine(
            $"[GT2-WHEEL-ENTRY] car=0x{ResolveVehicleRenderIdentity(carState):X8} " +
            $"record=0x{wheelRecord:X8} wheel={wheelIndex} detail={detail}");
    }

    static void AuditVisibilityTransition(
        IMemory m,
        uint trackRoot,
        uint stockList,
        int stockCount,
        int currentSector,
        int expandedCount)
    {
        if (_visibilityTransitionTrackRoot == trackRoot &&
            _visibilityTransitionSector == currentSector)
            return;

        Array.Clear(CurrentStockVisibilityEntries);
        Array.Clear(CurrentExpandedVisibilityEntries);
        for (int item = 0; item < stockCount; item++)
        {
            int objectIndex = m.ReadU16(
                stockList + 2u + (uint)item * 2u) & 0x3FFF;
            CurrentStockVisibilityEntries[objectIndex] = true;
        }
        for (int item = 0; item < expandedCount; item++)
        {
            int objectIndex = ExpandedVisibilityEntries[item] & 0x3FFF;
            CurrentExpandedVisibilityEntries[objectIndex] = true;
        }

        if (_visibilityTransitionTrackRoot == trackRoot &&
            _visibilityTransitionSector >= 0)
        {
            int stockAdds = 0;
            int stockRemoves = 0;
            int expandedAdds = 0;
            int expandedRemoves = 0;
            for (int objectIndex = 0;
                 objectIndex < CurrentStockVisibilityEntries.Length;
                 objectIndex++)
            {
                if (CurrentStockVisibilityEntries[objectIndex] &&
                    !PreviousStockVisibilityEntries[objectIndex])
                {
                    stockAdds++;
                    if (PreviousExpandedVisibilityEntries[objectIndex])
                        _visibilityStockAddsPrecovered++;
                }
                else if (!CurrentStockVisibilityEntries[objectIndex] &&
                    PreviousStockVisibilityEntries[objectIndex])
                {
                    stockRemoves++;
                    if (CurrentExpandedVisibilityEntries[objectIndex])
                        _visibilityStockRemovesRetained++;
                }
                if (CurrentExpandedVisibilityEntries[objectIndex] &&
                    !PreviousExpandedVisibilityEntries[objectIndex])
                    expandedAdds++;
                else if (!CurrentExpandedVisibilityEntries[objectIndex] &&
                    PreviousExpandedVisibilityEntries[objectIndex])
                    expandedRemoves++;
            }
            _visibilitySectorTransitions++;
            _visibilityStockTransitionAdds += stockAdds;
            _visibilityStockTransitionRemoves += stockRemoves;
            _visibilityExpandedTransitionAdds += expandedAdds;
            _visibilityExpandedTransitionRemoves += expandedRemoves;
            _visibilityStockMaximumTransition = Math.Max(
                _visibilityStockMaximumTransition,
                stockAdds + stockRemoves);
            _visibilityExpandedMaximumTransition = Math.Max(
                _visibilityExpandedMaximumTransition,
                expandedAdds + expandedRemoves);
        }

        CurrentStockVisibilityEntries.CopyTo(
            PreviousStockVisibilityEntries, 0);
        CurrentExpandedVisibilityEntries.CopyTo(
            PreviousExpandedVisibilityEntries, 0);
        _visibilityTransitionTrackRoot = trackRoot;
        _visibilityTransitionSector = currentSector;
    }

    static void TraceTrackVisibilityLod(
        IMemory m, uint visibilityList, bool maximumLod)
    {
        if (!TraceTrackRendering && !AuditRenderer)
            return;

        if (Volatile.Read(ref _aiAutoDriveReported) >= 2)
            _visibilityLodReplayCalls++;
        else
            _visibilityLodRaceCalls++;
        if (maximumLod)
            _visibilityLodMaximumCalls++;
        else
            _visibilityLodStockCalls++;

        if (visibilityList == 0)
        {
            _visibilityLodNullLists++;
        }
        else if (IsGuestRam(visibilityList))
        {
            int count = Math.Min((int)m.ReadU16(visibilityList), 0x4000);
            for (int index = 0; index < count; index++)
            {
                ushort entry = m.ReadU16(
                    visibilityList + 2u + (uint)index * 2u);
                _visibilityLodEntriesScanned++;
                if ((entry & 0xC000) != 0)
                    _visibilityLodNonzeroSelectors++;
            }
        }
        else
        {
            _visibilityLodInvalidLists++;
            if (VisibilityLodInvalidListSamples.Count < 8)
                VisibilityLodInvalidListSamples.Add(visibilityList);
        }

        if (TraceTrackRendering && !AuditRenderer &&
            Interlocked.Exchange(
                ref _visibilityLodExitTraceRegistered, 1) == 0)
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
                Console.Error.WriteLine(
                    $"[GT2-Visibility-LOD] raceCalls={_visibilityLodRaceCalls} " +
                    $"replayCalls={_visibilityLodReplayCalls} " +
                    $"maximumCalls={_visibilityLodMaximumCalls} " +
                    $"stockCalls={_visibilityLodStockCalls} " +
                    $"entriesScanned={_visibilityLodEntriesScanned} " +
                    $"nonzeroSelectors={_visibilityLodNonzeroSelectors}");
    }

    /// <summary>
    /// Emits renderer acceptance counters at the runtime's deterministic
    /// shutdown boundary. ProcessExit callbacks are unsuitable as completion
    /// evidence because host-driven termination can occur after redirected
    /// log capture has already completed.
    /// </summary>
    public static void DumpRendererAudit()
    {
        // Bounded Windows runs terminate with the native TerminateProcess API
        // after deterministic shutdown, so ProcessExit callbacks cannot be
        // the primary persistence path for development traces.
        FlushWheelTransformTrace();
        if (!AuditRenderer)
            return;
        Console.Error.WriteLine(
            $"[GT2-Renderer-Audit] track raceCalls={_visibilityLodRaceCalls} " +
            $"replayCalls={_visibilityLodReplayCalls} " +
            $"maximumCalls={_visibilityLodMaximumCalls} " +
            $"stockCalls={_visibilityLodStockCalls} " +
            $"entriesScanned={_visibilityLodEntriesScanned} " +
            $"nonzeroSelectors={_visibilityLodNonzeroSelectors} " +
            $"nullLists={_visibilityLodNullLists} " +
            $"invalidLists={_visibilityLodInvalidLists} " +
            $"invalidSamples=[{string.Join(',', VisibilityLodInvalidListSamples.Select(value => $"0x{value:X8}"))}]");
        Console.Error.WriteLine(
            $"[GT2-Renderer-Audit] visibility functionCalls={_trackVisibilityFunctionCalls} " +
            $"raceCalls={_trackVisibilityRaceCalls} " +
            $"replayCalls={_trackVisibilityReplayCalls} " +
            $"rawEntries={_trackVisibilityRawEntries} " +
            $"rawNonzeroSelectors={_trackVisibilityRawNonzeroSelectors}");
        Console.Error.WriteLine(
            $"[GT2-Renderer-Audit] expandedVisibility calls={_visibilityExpandedCalls} " +
            $"scope=bounded-sector-horizon " +
            $"radius={ExtendedVisibilitySectorRadius} " +
            $"stockEntries={_visibilityExpandedStockEntries} " +
            $"outputEntries={_visibilityExpandedOutputEntries} " +
            $"maximumAdded={_visibilityExpandedMaximumAdded}");
        Console.Error.WriteLine(
            $"[GT2-Renderer-Audit] visibilityTransitions " +
            $"sectors={_visibilitySectorTransitions} " +
            $"stockAdds={_visibilityStockTransitionAdds} " +
            $"stockRemoves={_visibilityStockTransitionRemoves} " +
            $"stockAddsPrecovered={_visibilityStockAddsPrecovered} " +
            $"stockRemovesRetained={_visibilityStockRemovesRetained} " +
            $"stockMaximum={_visibilityStockMaximumTransition} " +
            $"expandedAdds={_visibilityExpandedTransitionAdds} " +
            $"expandedRemoves={_visibilityExpandedTransitionRemoves} " +
            $"expandedMaximum={_visibilityExpandedMaximumTransition}");
        Console.Error.WriteLine(
            $"[GT2-Renderer-Audit] objectFrustum " +
            $"inside={_trackFrustumInside} " +
            $"intersecting={_trackFrustumIntersecting} " +
            $"outside={_trackFrustumOutside} " +
            $"expanded={_trackFrustumExpanded}");
        lock (VehicleLodSelectorCounts)
        {
            string selectors = string.Join(
                ',',
                VehicleLodSelectorCounts
                    .OrderBy(pair => pair.Key)
                    .Select(pair => $"{pair.Key}:{pair.Value}"));
        Console.Error.WriteLine(
            $"[GT2-Renderer-Audit] vehicles requests={_vehicleLodRequests} " +
                $"selectors=[{selectors}] modelSets={VehicleLodModelSets.Count} " +
                $"selectedModelPointers={VehicleLodModelPointers.Count}");
        }
        lock (VehicleRenderIdentities)
        {
            Console.Error.WriteLine(
                $"[GT2-Renderer-Audit] vehicleIdentity " +
                $"begins={_vehicleRenderIdentityBegins} " +
                $"ends={_vehicleRenderIdentityEnds} " +
                $"active=0x{_vehicleRenderIdentity:X8} " +
                $"distinct={VehicleRenderIdentities.Count} " +
                $"scopedCaptures={_vehicleRenderScopedCaptures} " +
                $"fallbackCaptures={_vehicleRenderFallbackCaptures}");
        }
        Console.Error.WriteLine(
            $"[GT2-Renderer-Audit] vehicleFrustum " +
            $"requests={_vehicleFrustumRequests} " +
            $"horizontalOutside={_vehicleFrustumHorizontalOutside} " +
            $"horizontalIntersecting={_vehicleFrustumHorizontalIntersecting} " +
            $"horizontalRelation=" +
            $"{_vehicleFrustumHorizontalOutsideOnly}/" +
            $"{_vehicleFrustumHorizontalIntersectingOnly}/" +
            $"{_vehicleFrustumHorizontalBoth} " +
            $"otherOutside={_vehicleFrustumOtherOutside} " +
            $"otherIntersecting={_vehicleFrustumOtherIntersecting} " +
            $"expanded={_vehicleFrustumExpanded}");
        Console.Error.WriteLine(
            $"[GT2-Renderer-Audit] wheelGate " +
            $"requests={_wheelGateRequests} " +
            $"guestDistanceEligible={_wheelGateGuestDistanceEligible} " +
            $"computedDistanceEligible={_wheelGateComputedDistanceEligible} " +
            $"modeEligible={_wheelGateModeEligible} " +
            $"mismatches={_wheelGateMismatches}");
        Console.Error.WriteLine(
            $"[GT2-Renderer-Audit] wheelDispatch " +
            $"requests={_wheelDispatchRequests} " +
            $"expectedEntries={_wheelDispatchRequests * 4} " +
            $"near={_wheelDispatchNearDetail} " +
            $"middle={_wheelDispatchMiddleDetail} " +
            $"far={_wheelDispatchFarDetail}");
        Console.Error.WriteLine(
            $"[GT2-Renderer-Audit] wheelRenderer " +
            $"entries={_wheelRendererEntries} " +
            $"near={_wheelRendererNearDetail} " +
            $"middle={_wheelRendererMiddleDetail} " +
            $"far={_wheelRendererFarDetail}");
    }

    static bool IsGuestRam(uint address) =>
        address is >= 0x80000000u and < 0x807FFF00u;

    public static void TraceRaceSchedulerDispatch(CpuContext c, IMemory m, uint address)
    {
        if (!TraceRaceScheduler) return;
        bool schedulerTarget = address is >= 0x80015FF8u and <= 0x80016258u;
        bool schedulerCaller = c.RA is >= 0x80016000u and <= 0x80016258u;
        if (!schedulerTarget && !schedulerCaller) return;

        uint raceObject = c.S2;
        uint state = raceObject is >= 0x80000000u and < 0x80800000u
            ? m.ReadU32(raceObject + 8u)
            : uint.MaxValue;
        string key =
            $"{address:X8}:{c.RA:X8}:{raceObject:X8}:{state:X8}:{c.A0:X8}";
        lock (RaceSchedulerStates)
        {
            if (!RaceSchedulerStates.Add(key)) return;
        }
        Console.WriteLine(
            $"[GT2Scheduler] call=0x{address:X8} ra=0x{c.RA:X8} " +
            $"object=0x{raceObject:X8} state=0x{state:X8} a0=0x{c.A0:X8} " +
            $"sp=0x{c.SP:X8}");
    }
    /// <summary>
    /// The original routine at 0x80010954 installs the callback at 0x80010928,
    /// spins until four VBlank interrupts increment 0x80011DF4, and removes
    /// the callback. A statically executing host cannot receive those
    /// asynchronous callbacks while it remains inside the spin loop, so
    /// advance the same four presentation/interrupt intervals explicitly.
    /// </summary>
    public static void WaitForInitialVBlanks(CpuContext c, IMemory m)
    {
        uint counterAddress = ArcadeVariant ? 0x80011DECu : 0x80011DF4u;
        m.WriteU32(counterAddress, 0u);
        for (uint count = 1; count <= 4; count++)
        {
            Runtime.PresentFrame();
            m.WriteU32(counterAddress, count);
        }
        c.V0 = 0u;
    }

    /// <summary>
    /// GT2's wrapper at 0x8007D23C waits for callback 0x8007D294 to advance
    /// the total and interval counters at 0x801F0680/0x801F0684. Presenting
    /// frames inside the host bridge lets the already-installed original
    /// callback run instead of deadlocking in the guest spin loop.
    /// </summary>
    public static void VSync(CpuContext c, IMemory m)
    {
        uint totalCounterAddress = CdDriveStateAddress + 0x170u;
        uint intervalCounterAddress = CdDriveStateAddress + 0x174u;

        int requested = (int)c.A0;
        int poll = Host.InputManager.CurrentPoll;
        if (
            TraceTrue60HzCadence && True60HzEnabled && poll >= 4280 &&
            Interlocked.Increment(ref _true60HzVSyncReports) <= 160)
        {
            Console.Error.WriteLine(
                $"[GT2-True60-VSync] poll={poll} requested={requested} " +
                $"ra=0x{c.RA:X8}");
        }
        if (requested < 0)
        {
            m.WriteU32(intervalCounterAddress, 0u);
            requested = 1;
        }
        else if (requested == 0)
        {
            // VSync(0) is a non-blocking counter query on real hardware, where
            // VBlank interrupts continue asynchronously. Recompiled guest code
            // runs synchronously, so polling loops would otherwise prevent the
            // callback that advances this counter from ever executing.
            Runtime.PresentFrame();
        }

        for (int frame = 0; frame < requested; frame++)
            Runtime.PresentFrame();

        if (requested > 0)
            m.WriteU32(intervalCounterAddress, 0u);
        c.V0 = m.ReadU32(totalCounterAddress);
    }

    /// <summary>
    /// Convert GT2's authored race time step from two NTSC fields to one. The
    /// original race code reads this byte for its timer, physics, effects, and
    /// replay rate branches, then copies it into the scheduler's +0x18 VBlank
    /// wait. Applying the conversion before that copy selects GT2's existing
    /// one-field calculations and scheduler cadence together. Object +0x1E is
    /// an independent buffer phase and must remain untouched.
    ///
    /// Systems that do not consult this authored time step still require
    /// explicit parity validation before the mode can become user-facing.
    /// </summary>
    public static void ConfigureTrue60HzRaceTimeStep(
        uint raceConfiguration,
        IMemory m)
    {
        if (!True60HzEnabled)
            return;

        if (!IsGuestRam(raceConfiguration))
            throw new InvalidOperationException(
                $"GT2 true-60 time-step hook received invalid configuration " +
                $"0x{raceConfiguration:X8}");

        uint timeStepAddress = raceConfiguration + 0x8u;
        byte authoredTimeStep = m.ReadU8(timeStepAddress);
        if (authoredTimeStep is not (1 or 2))
            throw new InvalidOperationException(
                $"GT2 true-60 expected race time step 1 or 2 at " +
                $"0x{timeStepAddress:X8}, found {authoredTimeStep}");

        // The half-step force solver retains one-bit signed division carry so
        // two consecutive 60 Hz fields sum exactly to GT2's authored 30 Hz
        // delta. Race and replay rebuild their car arrays at the same guest
        // addresses, so address/count identity cannot identify a new physical
        // simulation. Reset at the authored race/replay setup boundary or a
        // replay inherits the final rounding phase of the just-finished race.
        Array.Clear(True60HzLinearVelocityBefore);
        Array.Clear(True60HzLinearVelocityRemainders);
        _true60HzVelocityCarArray = 0u;
        _true60HzVelocityCarCount = 0u;
        _replayOracleControlFrame = -1;
        _replayOracleLastStateFrame = -1;
        int segment = Interlocked.Increment(ref _true60HzRaceSegment);

        m.WriteU8(timeStepAddress, 1);
        if (Interlocked.Increment(ref _true60HzSchedulerReports) <= 4)
            Console.Error.WriteLine(
                $"[GT2-True60] race time step {authoredTimeStep} -> 1 " +
                $"configuration=0x{raceConfiguration:X8} " +
                $"segment={segment} physicsCarry=reset");
    }

    /// <summary>
    /// Observe GT2's exact five-byte controller sample immediately after the
    /// original replay encoder or decoder. The host does not replace either
    /// codec. It retains the just-recorded samples long enough to prove that
    /// an automatic replay decodes the identical sequence and reports a
    /// bounded mismatch with no image capture or interactive instrumentation.
    /// </summary>
    public static void ObserveReplayControllerFrame(
        bool playback,
        uint replayBuffer,
        uint sample,
        IMemory m)
    {
        if (!IsGuestRam(replayBuffer) || !IsGuestRam(sample))
            return;

        ulong packed = PackReplayControllerSample(sample, m);
        if (!playback)
        {
            uint frameCount = m.ReadU32(replayBuffer);
            if (_replayOracleRecordBuffer != replayBuffer ||
                frameCount < _replayOracleRecordFrameCount)
            {
                BeginReplayOracleRecording(replayBuffer);
            }
            if (frameCount == _replayOracleRecordFrameCount)
                return;
            if (frameCount != _replayOracleRecordFrameCount + 1u)
            {
                Console.Error.WriteLine(
                    $"[GT2-Replay-Oracle] recording frame discontinuity " +
                    $"previous={_replayOracleRecordFrameCount} " +
                    $"current={frameCount}");
                BeginReplayOracleRecording(replayBuffer);
            }

            ReplayOracleRecordedControls.Add(packed);
            _replayOracleRecordFrameCount = frameCount;
            _replayOracleRecordedControlHash =
                HashValue(_replayOracleRecordedControlHash, packed);
            _replayOracleMode = false;
            _replayOracleControlFrame =
                ReplayOracleRecordedControls.Count - 1;
            return;
        }

        if (!_replayOraclePlaybackActive &&
            _replayOraclePlaybackBuffer == replayBuffer &&
            m.ReadU16(replayBuffer + 0xCu) != 0)
            return;
        if (!_replayOraclePlaybackActive ||
            _replayOraclePlaybackBuffer != replayBuffer)
        {
            BeginReplayOraclePlayback(replayBuffer, m);
        }

        // The decoder marks +0x0C when it consumes its terminal sentinel. That
        // call does not emit a sample; summarize the completed replay once.
        if (m.ReadU16(replayBuffer + 0xCu) != 0)
        {
            CompleteReplayOraclePlayback(m.ReadU32(replayBuffer));
            return;
        }

        int frame = _replayOraclePlaybackFrames++;
        _replayOraclePlaybackControlHash =
            HashValue(_replayOraclePlaybackControlHash, packed);
        _replayOracleMode = true;
        _replayOracleControlFrame = frame;
        if (frame < ReplayOracleRecordedControls.Count)
        {
            _replayOracleControlComparisons++;
            ulong expected = ReplayOracleRecordedControls[frame];
            if (packed != expected)
            {
                _replayOracleControlMismatches++;
                if (_replayOracleControlMismatches == 1)
                    Console.Error.WriteLine(
                        $"[GT2-Replay-Oracle] CONTROL MISMATCH frame={frame} " +
                        $"recorded={FormatReplayControllerSample(expected)} " +
                        $"decoded={FormatReplayControllerSample(packed)} " +
                        $"readOffset={m.ReadU16(replayBuffer + 0xEu)} " +
                        $"runRemaining={m.ReadU32(replayBuffer + 0x8u)}");
            }
        }

        if (frame > 0 && frame % 600 == 0)
        {
            double seconds = Stopwatch.GetElapsedTime(
                _replayOraclePlaybackStartTimestamp).TotalSeconds;
            double hz = seconds > 0.0 ? frame / seconds : 0.0;
            Console.Error.WriteLine(
                $"[GT2-Replay-Oracle] playback frame={frame} " +
                $"total={m.ReadU32(replayBuffer)} wall={seconds:F2}s " +
                $"effective={hz:F2}Hz controlMismatches=" +
                _replayOracleControlMismatches);
        }
    }

    static void BeginReplayOracleRecording(uint replayBuffer)
    {
        ReplayOracleRecordedControls.Clear();
        ReplayOracleRecordedStates.Clear();
        _replayOracleRecordBuffer = replayBuffer;
        _replayOracleRecordFrameCount = 0u;
        _replayOracleRecordedControlHash = Fnv64Offset;
        _replayOracleRecordedStateHash = Fnv64Offset;
        _replayOracleLastStateFrame = -1;
        _replayOracleControlFrame = -1;
        _replayOracleMode = false;
        _replayOraclePlaybackActive = false;
        Console.Error.WriteLine(
            $"[GT2-Replay-Oracle] recording started " +
            $"buffer=0x{replayBuffer:X8}");
    }

    static void BeginReplayOraclePlayback(uint replayBuffer, IMemory m)
    {
        _replayOraclePlaybackBuffer = replayBuffer;
        _replayOraclePlaybackFrames = 0;
        _replayOracleControlComparisons = 0;
        _replayOracleControlMismatches = 0;
        _replayOracleStateComparisons = 0;
        _replayOracleStateMismatches = 0;
        _replayOracleLastStateFrame = -1;
        _replayOraclePlaybackControlHash = Fnv64Offset;
        _replayOracleExpectedControlHash = Fnv64Offset;
        int playableFrames = Math.Max(
            0, ReplayOracleRecordedControls.Count - 1);
        for (int frame = 0; frame < playableFrames; frame++)
            _replayOracleExpectedControlHash = HashValue(
                _replayOracleExpectedControlHash,
                ReplayOracleRecordedControls[frame]);
        _replayOraclePlaybackStateHash = Fnv64Offset;
        _replayOracleControlFrame = -1;
        _replayOracleMode = true;
        _replayOraclePlaybackActive = true;
        _replayOraclePlaybackStartTimestamp = Stopwatch.GetTimestamp();
        Console.Error.WriteLine(
            $"[GT2-Replay-Oracle] playback started " +
            $"buffer=0x{replayBuffer:X8} encodedFrames=" +
            $"{m.ReadU32(replayBuffer)} recordedFrames=" +
            $"{ReplayOracleRecordedControls.Count} " +
            $"playableFrames={playableFrames} expectedControlHash=" +
            $"{_replayOracleExpectedControlHash:X16}");
    }

    static void CompleteReplayOraclePlayback(uint encodedFrames)
    {
        if (!_replayOraclePlaybackActive)
            return;
        _replayOraclePlaybackActive = false;
        double seconds = Stopwatch.GetElapsedTime(
            _replayOraclePlaybackStartTimestamp).TotalSeconds;
        double hz = seconds > 0.0
            ? _replayOraclePlaybackFrames / seconds
            : 0.0;
        int expectedControlFrames = Math.Max(
            0, ReplayOracleRecordedControls.Count - 1);
        int expectedStateFrames = ReplayOracleRecordedStates
            .Take(expectedControlFrames)
            .Count(state => state.HasValue);
        bool controlsExact =
            ReplayOracleRecordedControls.Count > 0 &&
            _replayOracleControlMismatches == 0 &&
            _replayOracleControlComparisons ==
                expectedControlFrames &&
            _replayOraclePlaybackFrames ==
                expectedControlFrames;
        bool statesExact =
            ReplayOracleRecordedStates.Count > 0 &&
            _replayOracleStateMismatches == 0 &&
            _replayOracleStateComparisons ==
                expectedStateFrames;
        Console.Error.WriteLine(
            $"[GT2-Replay-Oracle] completed encodedFrames={encodedFrames} " +
            $"decodedFrames={_replayOraclePlaybackFrames} " +
            $"recordedFrames={ReplayOracleRecordedControls.Count} " +
            $"playableFrames={expectedControlFrames} " +
            $"controls={(_replayOracleControlComparisons == 0 ? "unpaired" : controlsExact ? "exact" : "FAILED")} " +
            $"controlComparisons={_replayOracleControlComparisons} " +
            $"controlMismatches={_replayOracleControlMismatches} " +
            $"expectedControlHash={_replayOracleExpectedControlHash:X16} " +
            $"playbackControlHash={_replayOraclePlaybackControlHash:X16} " +
            $"physics={(_replayOracleStateComparisons == 0 ? "unpaired" : statesExact ? "exact" : "FAILED")} " +
            $"stateComparisons={_replayOracleStateComparisons} " +
            $"stateMismatches={_replayOracleStateMismatches} " +
            $"recordedStateHash={_replayOracleRecordedStateHash:X16} " +
            $"playbackStateHash={_replayOraclePlaybackStateHash:X16} " +
            $"wall={seconds:F2}s effective={hz:F2}Hz");
    }

    static ulong PackReplayControllerSample(uint sample, IMemory m)
    {
        ulong packed = 0;
        for (int index = 0; index < 5; index++)
            packed |= (ulong)m.ReadU8(sample + (uint)index) << (index * 8);
        return packed;
    }

    static string FormatReplayControllerSample(ulong packed) =>
        $"[{(byte)packed:X2},{(byte)(packed >> 8):X2}," +
        $"{(byte)(packed >> 16):X2},{(byte)(packed >> 24):X2}," +
        $"{(byte)(packed >> 32):X2}]";

    static ulong HashValue(ulong hash, ulong value)
    {
        for (int index = 0; index < sizeof(ulong); index++)
        {
            hash ^= (byte)(value >> (index * 8));
            hash *= Fnv64Prime;
        }
        return hash;
    }

    /// <summary>
    /// Apply the one-field duration to a signed fixed-point state delta from a
    /// vehicle force accumulator which GT2 otherwise treats as one 30 Hz step.
    /// </summary>
    public static uint ScaleTrue60HzVehicleDelta(uint delta) =>
        True60HzEnabled
            ? unchecked((uint)(unchecked((int)delta) >> 1))
            : delta;

    /// <summary>
    /// Convert a fixed-point velocity-to-position shift from 30 Hz to 60 Hz.
    /// The velocity remains a physical state value; only its integration over
    /// the shorter authored field is scaled.
    /// </summary>
    public static int GetTrue60HzVehicleIntegrationShift(int stockShift) =>
        True60HzEnabled ? stockShift + 1 : stockShift;

    /// <summary>
    /// Capture physical linear velocity before GT2's contact/force solver. The
    /// solver still runs every field; the matching end hook converts only its
    /// accumulated state change to the one-field duration.
    /// </summary>
    public static void BeginTrue60HzLinearVelocityStep(
        uint carArray, uint carCount, IMemory m)
    {
        if (!True60HzEnabled || !IsGuestRam(carArray) ||
            carCount is < 1u or > 16u)
            return;

        if (_true60HzVelocityCarArray != carArray ||
            _true60HzVelocityCarCount != carCount)
        {
            Array.Clear(True60HzLinearVelocityRemainders);
        }

        CaptureTrue60HzVehicleFields(
            carArray, carCount, True60HzLinearVelocityOffsets,
            True60HzLinearVelocityBefore, m);
        ObserveReplayPhysicalState(carArray, m);
        _true60HzVelocityCarArray = carArray;
        _true60HzVelocityCarCount = carCount;
    }

    public static void EndTrue60HzLinearVelocityStep(
        uint carArray, uint carCount, IMemory m)
    {
        if (!True60HzEnabled ||
            carArray != _true60HzVelocityCarArray ||
            carCount != _true60HzVelocityCarCount)
            return;

        CommitTrue60HzVehicleFields(
            carArray, carCount, True60HzLinearVelocityOffsets,
            True60HzLinearVelocityBefore,
            True60HzLinearVelocityRemainders, m);
    }

    static void ObserveReplayPhysicalState(uint carArray, IMemory m)
    {
        int frame = _replayOracleControlFrame;
        if (frame < 0 || frame == _replayOracleLastStateFrame)
            return;
        _replayOracleLastStateFrame = frame;

        uint car = carArray + 0x2Cu;
        ReplayPhysicalState state = new(
            unchecked((int)m.ReadU32(car + 0x830u)),
            unchecked((int)m.ReadU32(car + 0x834u)),
            unchecked((int)m.ReadU32(car + 0x838u)),
            unchecked((int)m.ReadU32(car + 0x65Cu)),
            unchecked((int)m.ReadU32(car + 0x660u)),
            unchecked((int)m.ReadU32(car + 0x664u)),
            unchecked((int)m.ReadU32(car + 0x64Cu)),
            m.ReadU16(car + 0x6FEu),
            m.ReadU16(car + 0x5Au));

        if (!_replayOracleMode)
        {
            while (ReplayOracleRecordedStates.Count <= frame)
                ReplayOracleRecordedStates.Add(null);
            ReplayOracleRecordedStates[frame] = state;
            _replayOracleRecordedStateHash =
                HashValue(_replayOracleRecordedStateHash, state.Hash);
            return;
        }

        _replayOraclePlaybackStateHash =
            HashValue(_replayOraclePlaybackStateHash, state.Hash);
        if (frame >= ReplayOracleRecordedStates.Count ||
            !ReplayOracleRecordedStates[frame].HasValue)
            return;
        ReplayPhysicalState expected =
            ReplayOracleRecordedStates[frame]!.Value;
        _replayOracleStateComparisons++;
        if (state == expected)
            return;

        _replayOracleStateMismatches++;
        if (_replayOracleStateMismatches == 1)
            Console.Error.WriteLine(
                $"[GT2-Replay-Oracle] PHYSICS MISMATCH frame={frame} " +
                $"recorded=pos({expected.X},{expected.Y},{expected.Z})/" +
                $"vel({expected.VelocityX},{expected.VelocityY}," +
                $"{expected.VelocityZ})/speed={expected.LongitudinalSpeed}/" +
                $"progress={expected.Progress}/heading={expected.Heading} " +
                $"replay=pos({state.X},{state.Y},{state.Z})/" +
                $"vel({state.VelocityX},{state.VelocityY}," +
                $"{state.VelocityZ})/speed={state.LongitudinalSpeed}/" +
                $"progress={state.Progress}/heading={state.Heading}");
    }

    static void CaptureTrue60HzVehicleFields(
        uint carArray,
        uint carCount,
        ReadOnlySpan<uint> offsets,
        Span<uint> before,
        IMemory m)
    {
        for (uint index = 0; index < carCount; index++)
        {
            uint car = carArray + index * 0xB40u;
            int baseIndex = checked((int)index) * offsets.Length;
            for (int field = 0; field < offsets.Length; field++)
                before[baseIndex + field] = m.ReadU32(car + offsets[field]);
        }
    }

    static void CommitTrue60HzVehicleFields(
        uint carArray,
        uint carCount,
        ReadOnlySpan<uint> offsets,
        ReadOnlySpan<uint> before,
        Span<int> remainders,
        IMemory m)
    {
        for (uint index = 0; index < carCount; index++)
        {
            uint car = carArray + index * 0xB40u;
            int baseIndex = checked((int)index) * offsets.Length;
            for (int field = 0; field < offsets.Length; field++)
            {
                int stateIndex = baseIndex + field;
                uint address = car + offsets[field];
                uint oldValue = before[stateIndex];
                uint newValue = m.ReadU32(address);
                long numerator =
                    unchecked((int)(newValue - oldValue)) +
                    remainders[stateIndex];
                int scaledDelta = checked((int)(numerator >> 1));
                remainders[stateIndex] =
                    checked((int)(numerator - ((long)scaledDelta << 1)));
                m.WriteU32(
                    address,
                    oldValue + unchecked((uint)scaledDelta));
            }
        }
    }

    public static void TraceRenderSchedulerEntry(CpuContext c, IMemory m)
    {
        if (!TraceTrackRendering || _renderSchedulerSamples++ >= 120)
            return;

        Console.Error.WriteLine(
            $"[GT2-Render-Scheduler] poll={Host.InputManager.CurrentPoll} " +
            $"ra=0x{c.RA:X8} a0=0x{c.A0:X8}");
    }

    public static void WaitForCdCommand(CpuContext c, IMemory m)
    {
        uint state = CdDriveStateAddress;
        uint busyAddress = state + 0x166u;
        uint deferredScriptAddress = state + 0x84u;
        uint currentScriptAddress = state + 0x7Cu;

        int attempt = 0;
        for (; m.ReadU8(busyAddress) != 0 && attempt < 65536; attempt++)
        {
            LibCd.Tick();
            uint deferredScript = m.ReadU32(deferredScriptAddress);
            bool transferInactive = m.ReadU8(state + 0x167u) == 0;
            bool readReachedEnd =
                m.ReadU32(state + 0x48u) != 0 &&
                m.ReadU32(state + 0x60u) >= m.ReadU32(state + 0x48u) &&
                (transferInactive || m.ReadU32(state + 0x4Cu) == 0);
            if (m.ReadU8(busyAddress) == 3 &&
                ((transferInactive && deferredScript != 0) || readReachedEnd) &&
                LibCd.QueueReadCompletion())
            {
                Console.Error.WriteLine(
                    $"[GT2Compat] queued deferred ReadN completion " +
                    $"script=0x{(deferredScript != 0 ? deferredScript : m.ReadU32(currentScriptAddress)):X8}");
            }
            if ((attempt & 63) == 63)
                Runtime.PresentFrame();
        }

        byte busy = m.ReadU8(busyAddress);
        if (busy != 0)
            throw new InvalidOperationException(
                $"GT2 CD command queue remained busy ({busy}) after {attempt} " +
                $"device ticks at 0x8007C550; current={m.ReadU32(state + 0x60u)} " +
                $"target={m.ReadU32(state + 0x48u)} handler=0x{m.ReadU32(state + 0x4Cu):X8}");
    }

    /// <summary>
    /// Give the virtual CD device the interrupt boundary that original PS1
    /// hardware supplied between consecutive low-level status polls.
    /// </summary>
    public static void ServiceCdDevice(CpuContext c, IMemory m)
    {
        LibCd.Tick();
        Runtime.DrainDeferredIrqs();
        // The original MDEC/CD producer-consumer pipeline continues from
        // interrupts while the foreground code polls the opening movie state.
        // Deliver only those pending interrupts here. Presenting a complete
        // host frame would re-enter the same guest object through its VBlank
        // scheduler while it is already updating.
        Runtime.DispatchIrq(0);

        // A decoded STR frame is emitted through a chain of MDEC-out and GPU
        // DMA slices. Each slice can raise another completion while the
        // previous callback is still unwinding, so drain the chain exactly as
        // the hardware interrupt controller would.
        const uint dmaInterruptControl = 0x1F8010F4u;
        for (int guard = 0;
             guard < 64 &&
             (m.ReadU32(dmaInterruptControl) & 0x7F000000u) != 0;
             guard++)
        {
            Runtime.DispatchIrq(0);
        }
    }

    /// <summary>
    /// GT2's ring-buffer accessor at 0x80082054 waits for its ready callback
    /// to increment the buffered-sector count at +0x5A. Service the virtual
    /// drive while that original asynchronous producer is starved by static
    /// execution, then return the same current ring-slot pointer as 0x80082000.
    /// </summary>
    public static void WaitForCdBuffer(CpuContext c, IMemory m)
    {
        uint state = CdDriveStateAddress;
        int attempt = 0;
        while (m.ReadU16(state + 0x5Au) == 0 && attempt < 65536)
        {
            LibCd.Tick();
            if ((++attempt & 63) == 0)
                Runtime.PresentFrame();
        }

        if (m.ReadU16(state + 0x5Au) == 0)
            throw new InvalidOperationException(
                $"GT2 CD ring remained empty after {attempt} device ticks; " +
                $"busy={m.ReadU8(state + 0x166u)} current={m.ReadU32(state + 0x44u)}");

        c.V0 = m.ReadU32(state + 0x50u) +
               ((uint)m.ReadU16(state + 0x5Cu) << 11);
    }

    public static void TraceBootObject(CpuContext c, IMemory m)
    {
        if (!TraceBoot)
            return;

        int invocation = _bootObjectTraceCount++;
        if (invocation >= 8 && invocation % 60 != 0)
            return;
        uint instance = c.A0;
        uint table = m.ReadU32(instance);
        var entries = new string[13];
        for (uint index = 0; index < entries.Length; index++)
            entries[index] = $"0x{m.ReadU32(table + index * 4u):X8}";
        Console.Error.WriteLine(
            $"[GT2Compat] boot-object frame={invocation} instance=0x{instance:X8} " +
            $"clock={m.ReadU32(instance + 0x80u)}/{m.ReadU32(instance + 0x84u)} " +
            $"flags=0x{m.ReadU32(instance + 0x88u):X8}/0x{m.ReadU32(instance + 0x90u):X8} " +
            $"vtable=0x{table:X8} entries={string.Join(',', entries)}");
    }

    public static void TraceOverlayLoad(CpuContext c, IMemory m)
    {
        uint index = c.A0;
        _overlayIndex = index;
        if (_unifiedOpeningPrelude && ArcadeVariant && index == 5u)
            Host.InputManager.SignalScriptStage("gt2_opening");
        if (_unifiedOpeningPrelude && ArcadeVariant && index == 1u)
        {
            _unifiedOpeningPrelude = false;
            _unifiedOpeningCompleted = true;
            Console.WriteLine(
                "[GT2] original Arcade opening complete; continuing into " +
                "the Simulation bootstrap and unified title");
            throw new GT2VariantSwitch("simulation-opening-complete");
        }
        if (index == 5u)
            ReconcileArcadeOpeningMovieExtent(m);
        TraceArcadeRaceConfig(index, m);
        TraceArcadeRaceState(index, m);
        TraceArcadeRaceMemory(index, m);
        // Widen the display and drawing areas before the title overlay builds
        // its environments.  The Sony demo panel is authored at 512x480.
        if (_overlayPrefix == "gt2_overlay")
        {
            if (index == 1u)
                _unifiedTitleMenuActive = true;
            else if (_unifiedTitleMenuActive)
            {
                _unifiedTitleMenuActive = false;
                Console.WriteLine(
                    $"[GT2] exact title compositor disabled before overlay {index}");
            }
        }
        if (!TraceBoot)
            return;

        int invocation = _overlayLoadTraceCount++;
        if (invocation >= 16)
            return;

        uint tableBase = 0x801EF610u;
        uint entry = index < 4096u
            ? tableBase + index * 8u + 0xCu
            : 0u;
        Console.Error.WriteLine(
            $"[GT2Compat] overlay-load invocation={invocation} index={index} " +
            $"callerRA=0x{c.RA:X8} " +
            $"returnTarget=0x{c.S0:X8} " +
            $"archiveBase={m.ReadU32(0x801C93D0u)} " +
            $"archiveSize={m.ReadU32(0x801C93E0u)} loadedBase={m.ReadU32(tableBase + 8u):X8} " +
            $"entry=0x{entry:X8} offset=0x{(entry != 0 ? m.ReadU32(entry) : 0):X8} " +
            $"flags=0x{(entry != 0 ? m.ReadU32(entry + 4u) : 0):X8}");
    }

    static void TraceArcadeRaceConfig(uint overlayIndex, IMemory m)
    {
        if (!ArcadeVariant || overlayIndex != 3u ||
            string.IsNullOrWhiteSpace(ArcadeRaceConfigTracePath) ||
            Interlocked.Exchange(ref _arcadeRaceConfigTraceReported, 1) != 0)
            return;

        // Overlay 2 copies the completed frontend selection record here
        // immediately before requesting overlay 3. Overlay 3 then copies the
        // same 0x2D4-byte record into the fixed race-engine parameter block.
        CaptureArcadeRaceConfig(
            0x801D5A00u,
            ArcadeRaceConfigTracePath,
            "post-finalize",
            m);
    }

    static void TraceArcadeRaceState(uint overlayIndex, IMemory m)
    {
        if (!ArcadeVariant || overlayIndex != 3u ||
            string.IsNullOrWhiteSpace(ArcadeRaceStateTracePath) ||
            Interlocked.Exchange(ref _arcadeRaceStateTraceReported, 1) != 0)
            return;

        CaptureArcadeRaceState(
            0x801D52BCu,
            ArcadeRaceStateTracePath,
            "overlay-3-handoff",
            m);
    }

    static void CaptureArcadeRaceState(
        uint address,
        string tracePath,
        string stage,
        IMemory m)
    {
        const int length = 0x58C;
        var data = new byte[length];
        for (int offset = 0; offset < data.Length; offset++)
            data[offset] = m.ReadU8(address + (uint)offset);

        string path = Path.GetFullPath(tracePath);
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        File.WriteAllBytes(path, data);
        string digest = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(data));
        Console.Error.WriteLine(
            $"[GT2-Direct] captured Arcade race state stage={stage} " +
            $"address=0x{address:X8} bytes={length} sha256={digest} path={path}");
    }

    static void TraceArcadeRaceMemory(uint overlayIndex, IMemory m)
    {
        if (!ArcadeVariant || overlayIndex != 3u ||
            string.IsNullOrWhiteSpace(ArcadeRaceMemoryTracePath) ||
            Interlocked.Exchange(ref _arcadeRaceMemoryTraceReported, 1) != 0)
            return;

        const uint address = 0x80000000u;
        const int length = 0x200000;
        var data = new byte[length];
        for (int offset = 0; offset < data.Length; offset++)
            data[offset] = m.ReadU8(address + (uint)offset);

        string path = Path.GetFullPath(ArcadeRaceMemoryTracePath);
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        File.WriteAllBytes(path, data);
        string digest = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(data));
        Console.Error.WriteLine(
            "[GT2-Direct] captured Arcade overlay-3 handoff memory " +
            $"address=0x{address:X8} bytes={length} sha256={digest} path={path}");
    }

    static void CaptureArcadeRaceConfig(
        uint address,
        string tracePath,
        string stage,
        IMemory m)
    {
        const int length = 0x2D4;
        var data = new byte[length];
        for (int offset = 0; offset < data.Length; offset++)
            data[offset] = m.ReadU8(address + (uint)offset);

        string path = Path.GetFullPath(tracePath);
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        File.WriteAllBytes(path, data);
        string digest = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(data));
        Console.Error.WriteLine(
            $"[GT2-Direct] captured Arcade race config stage={stage} " +
            $"address=0x{address:X8} bytes={length} sha256={digest} path={path}");
    }

    public static void TraceFiniteCdRead(CpuContext c, IMemory m)
    {
        int invocation = _finiteCdReadTraceCount++;
        if (invocation >= 40)
            return;

        uint drive = CdDriveStateAddress;
        const uint transfer = 0x801C9500u;
        Console.Error.WriteLine(
            $"[GT2Compat] finite-read invocation={invocation} sector={c.A0} " +
            $"expected={m.ReadU32(drive + 0x44u)} end={m.ReadU32(drive + 0x48u)} " +
            $"dest=0x{m.ReadU32(transfer):X8} offset={m.ReadU16(transfer + 0x6u)} " +
            $"stride={m.ReadU16(transfer + 0x10u)} remaining={m.ReadU16(transfer + 0x12u)} " +
            $"active={m.ReadU8(drive + 0x167u)} busy={m.ReadU8(drive + 0x166u)}");
    }

    public static void TraceMenuListInput(CpuContext c, IMemory m)
    {
        if (!TraceMenu || c.A1 == 0)
            return;

        int invocation = _menuListTraceCount++;
        uint input = c.A1;
        uint pressed = m.ReadU32(input + 0x4u);
        uint repeated = m.ReadU32(input + 0xCu);
        if (pressed == 0 && repeated == 0 && invocation >= 12)
            return;

        Console.Error.WriteLine(
            $"[GT2Compat] menu-list call={invocation} input=0x{input:X8} " +
            $"pressed=0x{pressed:X8} repeated=0x{repeated:X8} " +
            $"list=0x{c.A0:X8} selected={m.ReadU16(c.A0 + 0x6u)}");
    }

    /// <summary>
    /// Run GT2 through a trampoline that gives its setjmp/longjmp overlay
    /// loader genuine non-local control-flow semantics. Every jump unwinds the
    /// abandoned managed guest call stack before the selected overlay target
    /// is dispatched. Calling the target directly from Longjmp would retain
    /// every prior overlay on the CLR stack; replay exit performs enough
    /// transitions for those stale frames to re-enter old loaders.
    /// </summary>
    public static void RunGuestLoop(
        CpuContext c, IMemory m, uint entry,
        string overlayPrefix = "gt2_overlay")
    {
        string previousOverlayPrefix = _overlayPrefix;
        _overlayPrefix = overlayPrefix;
        try
        {
            uint target = entry;
            int transition = 0;
            bool returnTrampoline = false;
            while (true)
            {
                try
                {
                    Dispatch.Dispatcher.Call(c, m, target);
                    if (!returnTrampoline)
                        return;
                    target = c.RA;
                    if (TraceBoot)
                        Console.Error.WriteLine(
                            $"[GT2Compat] coroutine return target=0x{target:X8}");
                    if (target == 0u)
                        throw new InvalidOperationException(
                            "GT2 coroutine return trampoline reached a null return address");
                }
                catch (NonLocalJump jump)
                {
                    target = jump.Target;
                    returnTrampoline = jump.ReturnTrampoline;
                    transition++;
                    if (TraceBoot)
                        Console.Error.WriteLine(
                            $"[GT2Compat] non-local overlay transition={transition} " +
                            $"target=0x{target:X8}");
                }
            }
        }
        finally
        {
            _overlayPrefix = previousOverlayPrefix;
        }
    }

    /// <summary>
    /// GT2 uses setjmp/longjmp to transfer control from its bootstrap overlay
    /// loader into the newly decompressed image. Restore the saved MIPS
    /// register context, then signal RunGuestLoop to abandon the current
    /// managed guest call stack and dispatch the selected overlay target.
    /// </summary>
    public static void Longjmp(CpuContext c, IMemory m)
    {
        uint context = c.A0;
        uint value = c.A1 == 0u ? 1u : c.A1;
        bool overlayTransition = value >= 0x80000000u;
        if (overlayTransition)
        {
            if (_overlayIndex > 5)
                throw new InvalidOperationException(
                    $"GT2 longjmp selected unknown overlay {_overlayIndex}");
            Dispatch.Dispatcher.Load($"{_overlayPrefix}_{_overlayIndex}");
        }

        c.RA = m.ReadU32(context);
        c.SP = m.ReadU32(context + 0x4u);
        c.FP = m.ReadU32(context + 0x8u);
        c.S0 = m.ReadU32(context + 0xCu);
        c.S1 = m.ReadU32(context + 0x10u);
        c.S2 = m.ReadU32(context + 0x14u);
        c.S3 = m.ReadU32(context + 0x18u);
        c.S4 = m.ReadU32(context + 0x1Cu);
        c.S5 = m.ReadU32(context + 0x20u);
        c.S6 = m.ReadU32(context + 0x24u);
        c.S7 = m.ReadU32(context + 0x28u);
        c.GP = m.ReadU32(context + 0x2Cu);
        c.V0 = value;

        c.A0 = m.ReadU32(0x801C945Cu);
        c.A1 = m.ReadU32(0x801C9460u);
        c.A2 = m.ReadU32(0x801C9464u);
        c.A3 = m.ReadU32(0x801C9468u);
        uint target = overlayTransition ? value : c.RA;
        if (TraceBoot)
            Console.Error.WriteLine(
                overlayTransition
                    ? $"[GT2Compat] longjmp overlay={_overlayIndex} target=0x{target:X8}"
                    : $"[GT2Compat] longjmp coroutine target=0x{target:X8} value={value}");
        throw new NonLocalJump(target, returnTrampoline: !overlayTransition);
    }
}
