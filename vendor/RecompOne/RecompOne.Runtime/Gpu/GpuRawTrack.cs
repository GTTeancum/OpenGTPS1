using System.Buffers;
using System.Buffers.Binary;
using RecompOne.Runtime.Config;
using RecompOne.Runtime.Hle;
using RecompOne.Runtime.Memory;

namespace RecompOne.Runtime;

/// <summary>
/// Development bridge from GT2's authored track model to the native world
/// renderer. It consumes the model before GT2 projects, subdivides, clips, or
/// rejects it. The guest packet renderer still executes while this path is
/// being proven, but its track packets are not admitted to the live capture.
/// </summary>
public sealed partial class Gpu
{
    readonly record struct RawTrackInstanceKey(
        uint StableId,
        uint ModelPointer,
        uint MeshPointer,
        ulong TransformId,
        TrackMeshProjectionPath ProjectionPath);

    readonly record struct RawTrackSourceMeshKey(
        uint ModelPointer,
        uint MeshPointer,
        uint VertexPointer,
        uint VertexCount);

    readonly record struct RawTrackResidentMeshKey(
        uint ModelPointer,
        uint MeshPointer,
        uint VertexPointer,
        uint VertexCount,
        uint TextureTableBase,
        int IndexBits,
        bool AuxiliaryFormat,
        TrackMeshProjectionPath ProjectionPath);

    sealed record RawTrackResidentMeshDefinition(
        ulong Key,
        byte[] Bytes,
        int SourcePrimitives,
        int PotentialTriangles,
        int TexturedPrimitives,
        int PackedFaceBitSetPrimitives,
        int PackedFaceBitClearPrimitives);

    readonly record struct RawTrackTriangleKey(
        uint StableId,
        uint ModelPointer,
        ulong TransformId,
        short Ax, short Ay, short Az,
        short Bx, short By, short Bz,
        short Cx, short Cy, short Cz);

    readonly record struct RawTrackPrimitiveKey(
        uint StableId,
        uint ModelPointer,
        ulong TransformId,
        uint PrimitiveAddress,
        TrackMeshProjectionPath ProjectionPath);

    readonly record struct RawTrackPrimitiveDecision(
        int Stream,
        int Item,
        bool Accepted,
        bool GteSaturatedAccepted,
        bool GteProjectionSaturated,
        double FirstDeterminant,
        double SecondDeterminant,
        long FirstNclip,
        long SecondNclip);

    readonly record struct RawTrackTemporalFaceKey(
        uint StableId,
        uint ModelPointer,
        uint PrimitiveAddress,
        TrackMeshProjectionPath ProjectionPath,
        int Stream,
        int PrimitiveTriangle);

    readonly record struct RawTrackTemporalFaceDecision(
        bool Accepted,
        bool PotentiallyVisible,
        bool FiniteProjection,
        double ProjectedArea,
        double TargetArea,
        double TargetCenterX,
        double TargetCenterY,
        double Ax,
        double Ay,
        double Bx,
        double By,
        double Cx,
        double Cy,
        double MinimumViewZ,
        double MaximumViewZ,
        ulong TransformId,
        short R00,
        short R01,
        short R02,
        short R10,
        short R11,
        short R12,
        short R20,
        short R21,
        short R22,
        int TranslateX,
        int TranslateY,
        int TranslateZ,
        double FirstDeterminant,
        double SecondDeterminant,
        int Item,
        int Poll);

    readonly record struct RawTrackScreenPoint(double X, double Y);

    readonly record struct RawTrackCoverage(
        double Area,
        double CenterX,
        double CenterY);

    readonly record struct RawTrackClipPoint(double X, double Y, double Z);

    readonly record struct RawTrackBillboardQuad(
        short X0, short Y0, short Z0,
        short X1, short Y1, short Z1,
        short X2, short Y2, short Z2,
        short X3, short Y3, short Z3);

    readonly record struct RawTrackExpectedTriangle(
        HleVertex A,
        HleVertex B,
        HleVertex C,
        GteProjectionOrigin OriginA,
        GteProjectionOrigin OriginB,
        GteProjectionOrigin OriginC,
        PrimFlags Flags,
        uint PrimitiveAddress,
        TrackMeshProjectionPath ProjectionPath,
        int Stream,
        int Item,
        int PrimitiveTriangle,
        double ViewDeterminant,
        bool AllInFront);

    readonly record struct RawTrackSourceDecision(
        uint PrimitiveAddress,
        TrackMeshProjectionPath ProjectionPath,
        int Stream,
        int Item,
        int PrimitiveTriangle,
        bool PackedFaceBitSet,
        bool Accepted,
        bool GteSaturatedAccepted,
        double TriangleDeterminant,
        double PrimitiveFacing,
        long TriangleNclip,
        long PrimitiveNclip0,
        long PrimitiveNclip1);

    static readonly int[] RawTrackPrimitiveRecordSizes =
        [12, 12, 20, 24, 12, 12, 20, 24];
    static readonly int[] AuxiliaryTrackPrimitiveRecordSizes =
        [12, 12, 20, 24, 24, 24, 32, 36];
    const int RawTrackBillboardStream = 8;
    const int RawTrackCorrelationStreamCount = 9;
    const int RawTrackProjectionPathCount = 2;
    const long RawTrackFixedUnit = 4096L;
    const long RawTrackNearFixed = 16L * RawTrackFixedUnit;
    const ulong RawTrackResidentMeshMagic = 0x314853454D54474FUL;
    const int RawTrackResidentMeshHeaderSize = 32;
    const int RawTrackResidentVertexStride = 12;
    const int RawTrackResidentPrimitiveStride = 80;

    // The modern renderer owns GT2 course geometry from the authored model
    // streams. Falling back to guest-projected visibility packets restores the
    // stock draw-distance/pop-in behavior, so this is a shipping invariant and
    // cannot be selected by process environment.
    readonly bool _rawTrackReplacementRequested = true;
    readonly bool _rawTrackCorrelationRequested =
        string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_RAW_TRACK_CORRELATION"),
            "1",
            StringComparison.Ordinal);
    readonly bool _rawTrackFlatDebugRequested =
        string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_DEV_GT2_RAW_TRACK_FLAT"),
            "1",
            StringComparison.Ordinal);
    readonly bool _rawTrackAllocationTraceRequested =
        string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_RAW_TRACK_ALLOCATIONS"),
            "1",
            StringComparison.Ordinal);
    readonly bool _rawTrackVisibilityAuditRequested =
        string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_AUDIT_GT2_RAW_TRACK_VISIBILITY"),
            "1",
            StringComparison.Ordinal);
    readonly bool _rawTrackResidentEquivalenceAuditRequested =
        string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_AUDIT_GT2_RESIDENT_EQUIVALENCE"),
            "1",
            StringComparison.Ordinal);
    readonly bool _rawTrackResidentDiscoveryTraceRequested =
        string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_RESIDENT_DISCOVERY"),
            "1",
            StringComparison.Ordinal);
    readonly int _rawTrackFaceTracePoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_RAW_TRACK_FACE_POLL"),
            out int rawTrackFaceTracePoll)
            ? Math.Max(0, rawTrackFaceTracePoll)
            : -1;
    readonly int _rawTrackFaceTraceEndPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_RAW_TRACK_FACE_END_POLL"),
            out int rawTrackFaceTraceEndPoll)
            ? Math.Max(0, rawTrackFaceTraceEndPoll)
            : -1;
    readonly int _rawTrackFaceTraceLimit =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_RAW_TRACK_FACE_LIMIT"),
            out int rawTrackFaceTraceLimit)
            ? Math.Max(1, rawTrackFaceTraceLimit)
            : 50_000;
    readonly int _rawTrackFaceTraceInterval =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_RAW_TRACK_FACE_INTERVAL"),
            out int rawTrackFaceTraceInterval)
            ? Math.Max(1, rawTrackFaceTraceInterval)
            : 1;
    readonly uint _rawTrackFaceTraceModel = ParseRawTrackTraceFilter(
        "RECOMPONE_TRACE_GT2_RAW_TRACK_FACE_MODEL");
    readonly uint _rawTrackCorrelationTraceModel = ParseRawTrackTraceFilter(
        "RECOMPONE_TRACE_GT2_RAW_TRACK_CORRELATION_MODEL");
    readonly int _rawTrackCorrelationTraceStartPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_RAW_TRACK_CORRELATION_START_POLL"),
            out int rawTrackCorrelationTraceStartPoll)
            ? Math.Max(0, rawTrackCorrelationTraceStartPoll)
            : -1;
    readonly int _rawTrackCorrelationTraceEndPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_RAW_TRACK_CORRELATION_END_POLL"),
            out int rawTrackCorrelationTraceEndPoll)
            ? Math.Max(0, rawTrackCorrelationTraceEndPoll)
            : -1;
    readonly bool _rawTrackTemporalFacingAuditRequested =
        string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_AUDIT_GT2_RAW_TRACK_TEMPORAL_FACING"),
            "1",
            StringComparison.Ordinal);
    readonly bool _rawTrackTemporalCoverageAuditRequested =
        string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_AUDIT_GT2_RAW_TRACK_TEMPORAL_COVERAGE"),
            "1",
            StringComparison.Ordinal);
    readonly bool _rawTrackNearClipAuditRequested =
        string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_AUDIT_GT2_RAW_TRACK_NEAR_CLIP"),
            "1",
            StringComparison.Ordinal);
    readonly int _rawTrackNearClipAuditStartPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_AUDIT_GT2_RAW_TRACK_NEAR_CLIP_START_POLL"),
            out int rawTrackNearClipAuditStartPoll)
            ? Math.Max(0, rawTrackNearClipAuditStartPoll)
            : 0;
    readonly int _rawTrackNearClipAuditEndPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_AUDIT_GT2_RAW_TRACK_NEAR_CLIP_END_POLL"),
            out int rawTrackNearClipAuditEndPoll)
            ? Math.Max(0, rawTrackNearClipAuditEndPoll)
            : int.MaxValue;
    readonly string? _rawTrackAuditStage = ParseRawTrackAuditStage();
    readonly int _rawTrackAuditStageStartPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_AUDIT_GT2_RAW_TRACK_STAGE_START_POLL"),
            out int rawTrackAuditStageStartPoll)
            ? Math.Max(0, rawTrackAuditStageStartPoll)
            : 0;
    readonly int _rawTrackAuditStageEndPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_AUDIT_GT2_RAW_TRACK_STAGE_END_POLL"),
            out int rawTrackAuditStageEndPoll)
            ? Math.Max(0, rawTrackAuditStageEndPoll)
            : int.MaxValue;
    readonly double _rawTrackTemporalCoverageTraceMinimum =
        double.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_AUDIT_GT2_RAW_TRACK_TEMPORAL_COVERAGE_MIN_DELTA"),
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out double rawTrackTemporalCoverageTraceMinimum)
            ? Math.Max(0.0, rawTrackTemporalCoverageTraceMinimum)
            : 1.0;
    readonly double _rawTrackAuditTargetAspect =
        RawTrackConfiguredTargetAspect();
    readonly HashSet<RawTrackInstanceKey> _rawTrackInstances = [];
    readonly HashSet<RawTrackInstanceKey> _rawTrackResidentAuditInstances = [];
    readonly Dictionary<RawTrackSourceMeshKey, uint[]>
        _rawTrackSourceMeshes = [];
    readonly Dictionary<RawTrackResidentMeshKey, RawTrackResidentMeshDefinition>
        _rawTrackResidentMeshes = [];
    readonly HashSet<ulong> _rawTrackTransforms = [];
    readonly Dictionary<RawTrackTriangleKey, List<RawTrackExpectedTriangle>>
        _rawTrackExpectedTriangles = [];
    readonly Dictionary<RawTrackTriangleKey, List<RawTrackSourceDecision>>
        _rawTrackSourceTriangles = [];
    readonly Dictionary<RawTrackPrimitiveKey, RawTrackPrimitiveDecision>
        _rawTrackPrimitiveDecisions = [];
    bool _rawTrackReplacementRegistered;
    long _rawTrackPendingFrame = long.MinValue;
    long _rawTrackFrames;
    long _rawTrackCompletedFrames;
    int _rawTrackCurrentFrameObjects;
    long _rawTrackCurrentFrameSourcePrimitives;
    long _rawTrackCurrentFrameTriangles;
    long _rawTrackCurrentFrameSetupAllocatedBytes;
    long _rawTrackCurrentFrameBillboardAllocatedBytes;
    long _rawTrackCurrentFrameStreamAllocatedBytes;
    long _rawTrackCurrentFramePoolAllocatedBytes;
    int _rawTrackMinimumObjectsPerFrame = int.MaxValue;
    int _rawTrackMaximumObjectsPerFrame;
    long _rawTrackMinimumSourcePrimitivesPerFrame = long.MaxValue;
    long _rawTrackMaximumSourcePrimitivesPerFrame;
    long _rawTrackMinimumTrianglesPerFrame = long.MaxValue;
    long _rawTrackMaximumTrianglesPerFrame;
    long _rawTrackObjects;
    long _rawTrackResidentInstances;
    long _rawTrackResidentDefinitions;
    long _rawTrackResidentAuditInstanceCount;
    long _rawTrackFrameDiscontinuities;
    long _rawTrackResidentInvalidations;
    long _rawTrackSourcePrimitives;
    long _rawTrackTriangles;
    long _rawTrackTexturedPrimitives;
    long _rawTrackNearMaterialPrimitives;
    long _rawTrackDistantMaterialPrimitives;
    long _rawTrackDuplicateCalls;
    long _rawTrackPackedFaceBitSetPrimitives;
    long _rawTrackPackedFaceBitClearPrimitives;
    long _rawTrackGuestTriangles;
    long _rawTrackCorrelationGeometryMatches;
    long _rawTrackCorrelationRejectedSourceMatches;
    long _rawTrackCorrelationRejectedSourceGteSaturatedAccepted;
    long _rawTrackCorrelationGuestPrimitiveDecisions;
    long _rawTrackCorrelationGuestPrimitiveMatches;
    long _rawTrackCorrelationGuestPrimitiveResidentOnly;
    long _rawTrackCorrelationGuestPrimitiveGuestOnly;
    long _rawTrackCorrelationGuestPrimitiveMissing;
    long _rawTrackCorrelationGuestPrimitiveModeledMatches;
    long _rawTrackCorrelationGuestPrimitiveModeledOnly;
    long _rawTrackCorrelationGuestPrimitiveExactOnly;
    long _rawTrackCorrelationGuestPrimitiveFirstNclipExact;
    long _rawTrackCorrelationGuestPrimitiveFirstNclipNegated;
    long _rawTrackCorrelationGuestPrimitiveFirstNclipOther;
    long _rawTrackCorrelationGuestPrimitiveSecondNclipExact;
    long _rawTrackCorrelationGuestPrimitiveSecondNclipNegated;
    long _rawTrackCorrelationGuestPrimitiveSecondNclipOther;
    long _rawTrackCorrelationGuestPrimitiveUnsaturatedDecisions;
    long _rawTrackCorrelationGuestPrimitiveUnsaturatedMatches;
    long _rawTrackCorrelationGuestPrimitiveUnsaturatedResidentOnly;
    long _rawTrackCorrelationGuestPrimitiveUnsaturatedGuestOnly;
    long _rawTrackCorrelationGuestPrimitiveQuantizedZeroDivergences;
    int _rawTrackCorrelationGuestPrimitiveTraceCount;
    int _rawTrackCorrelationTargetTraceCount;
    long _rawTrackCorrelationGeneratedGuestTriangles;
    readonly long[] _rawTrackCorrelationRejectedSourceByStream =
        new long[RawTrackProjectionPathCount * RawTrackCorrelationStreamCount];
    readonly long[] _rawTrackCorrelationRejectedSourceGteByStream =
        new long[RawTrackProjectionPathCount * RawTrackCorrelationStreamCount];
    readonly long[] _rawTrackCorrelationGuestPrimitiveByStream =
        new long[
            RawTrackProjectionPathCount *
            RawTrackCorrelationStreamCount *
            4];
    readonly long[] _rawTrackCorrelationGuestPrimitiveUnsaturatedByStream =
        new long[
            RawTrackProjectionPathCount *
            RawTrackCorrelationStreamCount *
            4];
    readonly long[] _rawTrackCorrelationGuestPrimitiveTruth = new long[8];
    readonly long[] _rawTrackCorrelationGuestPrimitiveTruthByStream =
        new long[
            RawTrackProjectionPathCount *
            RawTrackCorrelationStreamCount *
            8];
    long _rawTrackCorrelationExactMatches;
    long _rawTrackCorrelationUvMismatches;
    long _rawTrackCorrelationMaterialMismatches;
    long _rawTrackCorrelationColorMismatches;
    readonly long[] _rawTrackGuestGeometryPositiveByStream =
        new long[RawTrackProjectionPathCount * RawTrackCorrelationStreamCount];
    readonly long[] _rawTrackGuestGeometryNegativeByStream =
        new long[RawTrackProjectionPathCount * RawTrackCorrelationStreamCount];
    readonly long[] _rawTrackGuestFrontPositiveByStream =
        new long[RawTrackProjectionPathCount * RawTrackCorrelationStreamCount];
    readonly long[] _rawTrackGuestFrontNegativeByStream =
        new long[RawTrackProjectionPathCount * RawTrackCorrelationStreamCount];
    readonly long[] _rawTrackGuestPermutationByStream =
        new long[
            RawTrackProjectionPathCount *
            RawTrackCorrelationStreamCount *
            6];
    readonly long[] _rawTrackGuestPrimitivePositiveByStream =
        new long[
            RawTrackProjectionPathCount *
            RawTrackCorrelationStreamCount *
            2];
    readonly long[] _rawTrackGuestPrimitiveNegativeByStream =
        new long[
            RawTrackProjectionPathCount *
            RawTrackCorrelationStreamCount *
            2];
    long _rawTrackBillboardPrimitives;
    long _rawTrackBillboardTriangles;
    int _rawTrackEnvironmentTraceCount;
    long _rawTrackAllPositiveTriangles;
    long _rawTrackViewportIntersectingTriangles;
    long _rawTrackFacingAcceptedPrimitives;
    long _rawTrackFacingRejectedPrimitives;
    long _rawTrackFacingDegeneratePrimitives;
    long _rawTrackFacingAggregateOnlyPrimitives;
    long _rawTrackFacingAggregateOnlyTargetPrimitives;
    long _rawTrackFacingAggregateRestoredTargetTriangles;
    long _rawTrackFacingNearCrossingPrimitives;
    readonly long[] _rawTrackFacingAcceptedByStream =
        new long[RawTrackProjectionPathCount * RawTrackCorrelationStreamCount];
    readonly long[] _rawTrackFacingRejectedByStream =
        new long[RawTrackProjectionPathCount * RawTrackCorrelationStreamCount];
    int _rawTrackMaximumTransformsPerFrame;
    int _rawTrackVisibilityTraceCount;
    int _rawTrackFaceTraceCount;
    Dictionary<RawTrackTemporalFaceKey, RawTrackTemporalFaceDecision>
        _rawTrackTemporalCurrent = [];
    Dictionary<RawTrackTemporalFaceKey, RawTrackTemporalFaceDecision>
        _rawTrackTemporalPrevious = [];
    Dictionary<RawTrackTemporalFaceKey, RawTrackTemporalFaceDecision>
        _rawTrackTemporalBeforePrevious = [];
    long _rawTrackTemporalComparisons;
    long _rawTrackTemporalStateChanges;
    long _rawTrackTemporalIsolatedToggles;
    long _rawTrackTemporalVisibleComparisons;
    long _rawTrackTemporalVisibleStateChanges;
    long _rawTrackTemporalVisibleIsolatedToggles;
    long _rawTrackTemporalVisibleLargeIsolatedToggles;
    long _rawTrackTemporalAdditions;
    long _rawTrackTemporalRemovals;
    long _rawTrackTemporalDuplicateKeys;
    int _rawTrackTemporalToggleTraceCount;
    long _rawTrackTemporalCoverageComparisons;
    long _rawTrackTemporalCoverageAreaChanges;
    long _rawTrackTemporalCoverageIsolatedLosses;
    long _rawTrackTemporalCoverageIsolatedSpikes;
    long _rawTrackTemporalCoverageMotionReversals;
    long _rawTrackTemporalCoverageNearExcluded;
    long _rawTrackTemporalCoverageCameraCutEvents;
    long _rawTrackTemporalCoverageContinuousEdgeGrazes;
    long _rawTrackTemporalCoverageCullToggleEvents;
    double _rawTrackTemporalCoverageMaximumLoss;
    double _rawTrackTemporalCoverageMaximumSpike;
    double _rawTrackTemporalCoverageMaximumMidpointResidual;
    int _rawTrackTemporalCoverageTraceCount;
    long _rawTrackNearClipCrossings;
    long _rawTrackNearClipDecisionChanges;
    long _rawTrackNearClipRejectedToAccepted;
    long _rawTrackNearClipAcceptedToRejected;
    int _rawTrackNearClipTraceCount;
    WorldObjectContext _rawTrackGuestDecisionContext;
    ulong _rawTrackGuestDecisionTransform;
    TrackMeshProjectionPath _rawTrackGuestDecisionPath;

    internal bool RawTrackReplacementActive =>
        _rawTrackReplacementRegistered;

    bool RawTrackResidentMeshPathActive =>
        !_rawTrackCorrelationRequested &&
        !_rawTrackFlatDebugRequested &&
        !_rawTrackVisibilityAuditRequested &&
        !_rawTrackResidentEquivalenceAuditRequested &&
        !_rawTrackTemporalFacingAuditRequested &&
        !_rawTrackTemporalCoverageAuditRequested &&
        !_rawTrackNearClipAuditRequested &&
        _rawTrackFaceTracePoll < 0;

    void RegisterRawTrackReplacement()
    {
        if (!_rawTrackReplacementRequested)
            return;
        if (!_liveWorldCapture.Enabled)
            throw new InvalidOperationException(
                "Raw GT2 track replacement requires the native world renderer.");
        WorldCaptureContext.RegisterTrackMeshConsumer(CaptureRawTrackMesh);
        if (_rawTrackCorrelationRequested)
        {
            WorldCaptureContext.RegisterTrackFaceDecisionConsumer(
                CaptureRawTrackGuestFaceDecision);
        }
        _rawTrackReplacementRegistered = true;
        Console.Error.WriteLine(
            "[GT2-Raw-Track] enabled mode=replace " +
            "source=authored-preprojection textureDetail=primary " +
            "topology=authored-model " +
            "guestTrackFallback=disabled");
    }

    void UnregisterRawTrackReplacement()
    {
        if (!_rawTrackReplacementRegistered)
            return;
        WorldCaptureContext.UnregisterTrackMeshConsumer(CaptureRawTrackMesh);
        if (_rawTrackCorrelationRequested)
        {
            WorldCaptureContext.UnregisterTrackFaceDecisionConsumer(
                CaptureRawTrackGuestFaceDecision);
        }
        _rawTrackReplacementRegistered = false;
    }

    void ReportRawTrackReplacement()
    {
        if (!_rawTrackReplacementRequested)
            return;
        CompleteRawTrackFrame();
        WorldCaptureContext.ReportTrackProjectionOwnership();
        int minimumObjects = _rawTrackCompletedFrames == 0
            ? 0
            : _rawTrackMinimumObjectsPerFrame;
        long minimumSourcePrimitives = _rawTrackCompletedFrames == 0
            ? 0
            : _rawTrackMinimumSourcePrimitivesPerFrame;
        long minimumTriangles = _rawTrackCompletedFrames == 0
            ? 0
            : _rawTrackMinimumTrianglesPerFrame;
        Console.Error.WriteLine(
            $"[GT2-Raw-Track-Summary] frames={_rawTrackFrames} " +
            $"coverageFrames={_rawTrackCompletedFrames} " +
            $"objectsPerFrame={minimumObjects}.." +
            $"{_rawTrackMaximumObjectsPerFrame} " +
            $"sourcePrimitivesPerFrame={minimumSourcePrimitives}.." +
            $"{_rawTrackMaximumSourcePrimitivesPerFrame} " +
            $"trianglesPerFrame={minimumTriangles}.." +
            $"{_rawTrackMaximumTrianglesPerFrame} " +
            $"objects={_rawTrackObjects} " +
            $"residentInstances={_rawTrackResidentInstances} " +
            $"residentDefinitions={_rawTrackResidentDefinitions} " +
            $"residentAuditInstances={_rawTrackResidentAuditInstanceCount} " +
            $"frameDiscontinuities={_rawTrackFrameDiscontinuities} " +
            $"residentInvalidations={_rawTrackResidentInvalidations} " +
            $"residency={(RawTrackResidentMeshPathActive
                ? "static-mesh/native-transform"
                : "diagnostic-expanded-triangles")} " +
            $"sourcePrimitives={_rawTrackSourcePrimitives} " +
            $"triangles={_rawTrackTriangles} " +
            $"textured={_rawTrackTexturedPrimitives} " +
            $"materialLod={_rawTrackNearMaterialPrimitives}/" +
            $"{_rawTrackDistantMaterialPrimitives} " +
            $"billboards={_rawTrackBillboardPrimitives}/" +
            $"{_rawTrackBillboardTriangles} " +
            $"duplicateCalls={_rawTrackDuplicateCalls} " +
            $"faceBit31={_rawTrackPackedFaceBitSetPrimitives}/" +
            $"{_rawTrackPackedFaceBitClearPrimitives} " +
            "culling=authored-continuous-primitive " +
            $"facing={_rawTrackFacingAcceptedPrimitives}/" +
            $"{_rawTrackFacingRejectedPrimitives} " +
            $"facingDegenerate={_rawTrackFacingDegeneratePrimitives} " +
            $"facingAggregateOnly=" +
            $"{_rawTrackFacingAggregateOnlyPrimitives} " +
            $"facingAggregateTarget=" +
            $"{_rawTrackFacingAggregateOnlyTargetPrimitives}/" +
            $"{_rawTrackFacingAggregateRestoredTargetTriangles} " +
            $"facingNearCrossing=" +
            $"{_rawTrackFacingNearCrossingPrimitives} " +
            $"facingByStream={RawTrackFacingByStream()} " +
            (_rawTrackVisibilityAuditRequested
                ? $"allPositive={_rawTrackAllPositiveTriangles} " +
                  $"viewportIntersecting=" +
                  $"{_rawTrackViewportIntersectingTriangles} "
                : "visibility=disabled ") +
            $"maxTransformsPerFrame={_rawTrackMaximumTransformsPerFrame} " +
            $"faceTrace={_rawTrackFaceTraceCount}/" +
            $"{_rawTrackFaceTraceLimit}@{RawTrackFaceTraceRange()}/" +
            $"every{_rawTrackFaceTraceInterval} " +
            (_rawTrackTemporalFacingAuditRequested
                ? $"temporalFacing=" +
                  $"{_rawTrackTemporalComparisons}/" +
                  $"{_rawTrackTemporalStateChanges}/" +
                  $"{_rawTrackTemporalIsolatedToggles} " +
                  $"temporalFacingVisible=" +
                  $"{_rawTrackTemporalVisibleComparisons}/" +
                  $"{_rawTrackTemporalVisibleStateChanges}/" +
                  $"{_rawTrackTemporalVisibleIsolatedToggles}/" +
                  $"{_rawTrackTemporalVisibleLargeIsolatedToggles} " +
                  $"temporalIdentity=" +
                  $"{_rawTrackTemporalAdditions}/" +
                  $"{_rawTrackTemporalRemovals}/" +
                  $"{_rawTrackTemporalDuplicateKeys} "
                : "temporalFacing=disabled ") +
            (_rawTrackTemporalCoverageAuditRequested
                ? $"temporalCoverage=" +
                  $"{_rawTrackTemporalCoverageComparisons}/" +
                  $"{_rawTrackTemporalCoverageAreaChanges}/" +
                   $"{_rawTrackTemporalCoverageIsolatedLosses}/" +
                   $"{_rawTrackTemporalCoverageIsolatedSpikes} " +
                   $"temporalCoverageMotionReversals=" +
                   $"{_rawTrackTemporalCoverageMotionReversals} " +
                   $"temporalCoverageNearExcluded=" +
                   $"{_rawTrackTemporalCoverageNearExcluded} " +
                   $"temporalCoverageClassification=" +
                   $"{_rawTrackTemporalCoverageCameraCutEvents}/" +
                   $"{_rawTrackTemporalCoverageContinuousEdgeGrazes}/" +
                   $"{_rawTrackTemporalCoverageCullToggleEvents} " +
                   $"temporalCoverageMaximum=" +
                   $"{_rawTrackTemporalCoverageMaximumLoss:R}/" +
                   $"{_rawTrackTemporalCoverageMaximumSpike:R}/" +
                   $"{_rawTrackTemporalCoverageMaximumMidpointResidual:R} "
                : "temporalCoverage=disabled ") +
            (_rawTrackNearClipAuditRequested
                ? $"nearClip=" +
                  $"{_rawTrackNearClipCrossings}/" +
                  $"{_rawTrackNearClipDecisionChanges}/" +
                  $"{_rawTrackNearClipRejectedToAccepted}/" +
                  $"{_rawTrackNearClipAcceptedToRejected} " +
                  $"auditStage={RawTrackAuditStageRange()} "
                : "nearClip=disabled ") +
            (_rawTrackCorrelationRequested
                ? $"guestTriangles={_rawTrackGuestTriangles} " +
                  $"geometryMatches={_rawTrackCorrelationGeometryMatches} " +
                  $"rejectedSourceMatches=" +
                  $"{_rawTrackCorrelationRejectedSourceMatches} " +
                  $"rejectedSourceByStream=" +
                  $"{RawTrackRejectedSourceByStream()} " +
                  $"rejectedSourceGteSaturatedAccepted=" +
                  $"{_rawTrackCorrelationRejectedSourceGteSaturatedAccepted} " +
                  $"rejectedSourceGteByStream=" +
                  $"{RawTrackRejectedSourceGteByStream()} " +
                  $"guestPrimitiveDecisions=" +
                  $"{_rawTrackCorrelationGuestPrimitiveDecisions} " +
                  $"guestPrimitiveMatches=" +
                  $"{_rawTrackCorrelationGuestPrimitiveMatches} " +
                  $"guestPrimitiveResidentOnly=" +
                  $"{_rawTrackCorrelationGuestPrimitiveResidentOnly} " +
                  $"guestPrimitiveGuestOnly=" +
                  $"{_rawTrackCorrelationGuestPrimitiveGuestOnly} " +
                  $"guestPrimitiveMissing=" +
                  $"{_rawTrackCorrelationGuestPrimitiveMissing} " +
                  $"guestPrimitiveModeled=" +
                  $"{_rawTrackCorrelationGuestPrimitiveModeledMatches}/" +
                  $"{_rawTrackCorrelationGuestPrimitiveModeledOnly}/" +
                  $"{_rawTrackCorrelationGuestPrimitiveExactOnly} " +
                  $"guestPrimitiveFirstNclip=" +
                  $"{_rawTrackCorrelationGuestPrimitiveFirstNclipExact}/" +
                  $"{_rawTrackCorrelationGuestPrimitiveFirstNclipNegated}/" +
                  $"{_rawTrackCorrelationGuestPrimitiveFirstNclipOther} " +
                  $"guestPrimitiveSecondNclip=" +
                  $"{_rawTrackCorrelationGuestPrimitiveSecondNclipExact}/" +
                  $"{_rawTrackCorrelationGuestPrimitiveSecondNclipNegated}/" +
                  $"{_rawTrackCorrelationGuestPrimitiveSecondNclipOther} " +
                  $"guestPrimitiveUnsaturated=" +
                  $"{_rawTrackCorrelationGuestPrimitiveUnsaturatedDecisions}/" +
                  $"{_rawTrackCorrelationGuestPrimitiveUnsaturatedMatches}/" +
                  $"{_rawTrackCorrelationGuestPrimitiveUnsaturatedResidentOnly}/" +
                  $"{_rawTrackCorrelationGuestPrimitiveUnsaturatedGuestOnly} " +
                  $"guestPrimitiveQuantizedZeroDivergences=" +
                  $"{_rawTrackCorrelationGuestPrimitiveQuantizedZeroDivergences} " +
                  $"guestPrimitiveTruthRMG={RawTrackGuestPrimitiveTruth()} " +
                  $"guestPrimitiveTruthRMGByStream=" +
                  $"{RawTrackGuestPrimitiveTruthByStream()} " +
                  $"guestPrimitiveUnsaturatedByStream=" +
                  $"{RawTrackGuestPrimitiveUnsaturatedByStream()} " +
                  $"guestPrimitiveByStream=" +
                  $"{RawTrackGuestPrimitiveByStream()} " +
                  $"generatedGuestTriangles=" +
                  $"{_rawTrackCorrelationGeneratedGuestTriangles} " +
                  $"exactMatches={_rawTrackCorrelationExactMatches} " +
                  $"uvMismatches={_rawTrackCorrelationUvMismatches} " +
                  $"materialMismatches={_rawTrackCorrelationMaterialMismatches} " +
                  $"colorMismatches={_rawTrackCorrelationColorMismatches} "
                : string.Empty) +
            (_rawTrackCorrelationRequested
                ? $"guestGeometryWinding={RawTrackGuestGeometryWinding()} "
                  + $"guestFrontWinding={RawTrackGuestFrontWinding()} "
                  + $"guestPermutations={RawTrackGuestPermutations()} "
                  + $"guestPrimitiveWinding=" +
                    $"{RawTrackGuestPrimitiveWinding()} "
                : string.Empty) +
            "decodeFailures=0 guestTrackFallbacks=0");
    }

    void CompleteRawTrackFrame()
    {
        if (_rawTrackPendingFrame == long.MinValue)
            return;
        CompleteRawTrackTemporalFacingFrame();
        _rawTrackCompletedFrames++;
        _rawTrackMinimumObjectsPerFrame = Math.Min(
            _rawTrackMinimumObjectsPerFrame,
            _rawTrackCurrentFrameObjects);
        _rawTrackMaximumObjectsPerFrame = Math.Max(
            _rawTrackMaximumObjectsPerFrame,
            _rawTrackCurrentFrameObjects);
        _rawTrackMinimumSourcePrimitivesPerFrame = Math.Min(
            _rawTrackMinimumSourcePrimitivesPerFrame,
            _rawTrackCurrentFrameSourcePrimitives);
        _rawTrackMaximumSourcePrimitivesPerFrame = Math.Max(
            _rawTrackMaximumSourcePrimitivesPerFrame,
            _rawTrackCurrentFrameSourcePrimitives);
        _rawTrackMinimumTrianglesPerFrame = Math.Min(
            _rawTrackMinimumTrianglesPerFrame,
            _rawTrackCurrentFrameTriangles);
        _rawTrackMaximumTrianglesPerFrame = Math.Max(
            _rawTrackMaximumTrianglesPerFrame,
            _rawTrackCurrentFrameTriangles);
        if (_rawTrackAllocationTraceRequested &&
            (_rawTrackCompletedFrames <= 5 ||
             _rawTrackCompletedFrames % 300 == 0))
        {
            long total =
                _rawTrackCurrentFrameSetupAllocatedBytes +
                _rawTrackCurrentFrameBillboardAllocatedBytes +
                _rawTrackCurrentFrameStreamAllocatedBytes +
                _rawTrackCurrentFramePoolAllocatedBytes;
            Console.Error.WriteLine(
                "[GT2-Raw-Track-Alloc] " +
                $"frame={_rawTrackPendingFrame} " +
                $"objects={_rawTrackCurrentFrameObjects} " +
                $"setup={_rawTrackCurrentFrameSetupAllocatedBytes} " +
                $"billboards={_rawTrackCurrentFrameBillboardAllocatedBytes} " +
                $"streams={_rawTrackCurrentFrameStreamAllocatedBytes} " +
                $"pool={_rawTrackCurrentFramePoolAllocatedBytes} " +
                $"total={total}");
        }
        _rawTrackCurrentFrameObjects = 0;
        _rawTrackCurrentFrameSourcePrimitives = 0;
        _rawTrackCurrentFrameTriangles = 0;
        _rawTrackCurrentFrameSetupAllocatedBytes = 0;
        _rawTrackCurrentFrameBillboardAllocatedBytes = 0;
        _rawTrackCurrentFrameStreamAllocatedBytes = 0;
        _rawTrackCurrentFramePoolAllocatedBytes = 0;
        _rawTrackPendingFrame = long.MinValue;
    }

    void CompleteRawTrackTemporalFacingFrame()
    {
        if (!_rawTrackTemporalFacingAuditRequested &&
            !_rawTrackTemporalCoverageAuditRequested)
            return;
        if (!RawTrackAuditWindowActive())
        {
            _rawTrackTemporalCurrent.Clear();
            _rawTrackTemporalPrevious.Clear();
            _rawTrackTemporalBeforePrevious.Clear();
            return;
        }

        if (_rawTrackTemporalPrevious.Count != 0)
        {
            foreach ((RawTrackTemporalFaceKey key,
                      RawTrackTemporalFaceDecision current)
                     in _rawTrackTemporalCurrent)
            {
                if (!_rawTrackTemporalPrevious.TryGetValue(
                        key,
                        out RawTrackTemporalFaceDecision previous))
                {
                    _rawTrackTemporalAdditions++;
                    continue;
                }
                _rawTrackTemporalComparisons++;
                if (current.Accepted != previous.Accepted)
                    _rawTrackTemporalStateChanges++;
                bool potentiallyVisible =
                    current.PotentiallyVisible || previous.PotentiallyVisible;
                if (potentiallyVisible)
                {
                    _rawTrackTemporalVisibleComparisons++;
                    if (current.Accepted != previous.Accepted)
                        _rawTrackTemporalVisibleStateChanges++;
                }
                if (_rawTrackTemporalCoverageAuditRequested)
                {
                    AuditRawTrackTemporalCoverageTransition(
                        in key,
                        in current,
                        in previous);
                }
                if (!_rawTrackTemporalFacingAuditRequested)
                    continue;
                if (
                    current.Poll != previous.Poll + 1 ||
                    !_rawTrackTemporalBeforePrevious.TryGetValue(
                        key,
                        out RawTrackTemporalFaceDecision beforePrevious) ||
                    previous.Poll != beforePrevious.Poll + 1 ||
                    current.Accepted != beforePrevious.Accepted ||
                    current.Accepted == previous.Accepted
                ) {
                    continue;
                }

                _rawTrackTemporalIsolatedToggles++;
                potentiallyVisible |= beforePrevious.PotentiallyVisible;
                if (potentiallyVisible)
                {
                    _rawTrackTemporalVisibleIsolatedToggles++;
                    double maximumArea = Math.Max(
                        current.ProjectedArea,
                        Math.Max(
                            previous.ProjectedArea,
                            beforePrevious.ProjectedArea));
                    if (maximumArea < 1.0)
                        continue;
                    _rawTrackTemporalVisibleLargeIsolatedToggles++;
                }
                else
                    continue;
                if (_rawTrackTemporalToggleTraceCount >= 128)
                    continue;
                _rawTrackTemporalToggleTraceCount++;
                Console.Error.WriteLine(
                    $"[GT2-Raw-Track-Temporal-Face] " +
                    $"n={_rawTrackTemporalToggleTraceCount} " +
                    $"polls={beforePrevious.Poll}/" +
                    $"{previous.Poll}/{current.Poll} " +
                    $"object=0x{key.StableId:X8} " +
                    $"model=0x{key.ModelPointer:X8} " +
                    $"primitive=0x{key.PrimitiveAddress:X8} " +
                    $"path={(int)key.ProjectionPath} " +
                    $"stream={key.Stream} " +
                    $"triangle={key.PrimitiveTriangle} " +
                    $"item={current.Item} " +
                    $"accepted=" +
                    $"{(beforePrevious.Accepted ? 1 : 0)}/" +
                    $"{(previous.Accepted ? 1 : 0)}/" +
                    $"{(current.Accepted ? 1 : 0)} " +
                    $"area={beforePrevious.ProjectedArea:R}/" +
                    $"{previous.ProjectedArea:R}/" +
                    $"{current.ProjectedArea:R} " +
                    $"targetArea={beforePrevious.TargetArea:R}/" +
                    $"{previous.TargetArea:R}/" +
                    $"{current.TargetArea:R} " +
                    $"first={beforePrevious.FirstDeterminant:R}/" +
                    $"{previous.FirstDeterminant:R}/" +
                    $"{current.FirstDeterminant:R} " +
                    $"second={beforePrevious.SecondDeterminant:R}/" +
                    $"{previous.SecondDeterminant:R}/" +
                    $"{current.SecondDeterminant:R}");
            }
            foreach (RawTrackTemporalFaceKey key
                     in _rawTrackTemporalPrevious.Keys)
            {
                if (!_rawTrackTemporalCurrent.ContainsKey(key))
                    _rawTrackTemporalRemovals++;
            }
        }

        Dictionary<RawTrackTemporalFaceKey, RawTrackTemporalFaceDecision>
            recycled = _rawTrackTemporalBeforePrevious;
        _rawTrackTemporalBeforePrevious = _rawTrackTemporalPrevious;
        _rawTrackTemporalPrevious = _rawTrackTemporalCurrent;
        _rawTrackTemporalCurrent = recycled;
        _rawTrackTemporalCurrent.Clear();
    }

    void AuditRawTrackTemporalCoverageTransition(
        in RawTrackTemporalFaceKey key,
        in RawTrackTemporalFaceDecision current,
        in RawTrackTemporalFaceDecision previous)
    {
        if (!current.FiniteProjection || !previous.FiniteProjection)
        {
            _rawTrackTemporalCoverageNearExcluded++;
            return;
        }
        _rawTrackTemporalCoverageComparisons++;
        double currentArea = current.Accepted ? current.TargetArea : 0.0;
        double previousArea = previous.Accepted ? previous.TargetArea : 0.0;
        if (Math.Abs(currentArea - previousArea) >= 1.0)
            _rawTrackTemporalCoverageAreaChanges++;
        if (
            current.Poll != previous.Poll + 1 ||
            !_rawTrackTemporalBeforePrevious.TryGetValue(
                key,
                out RawTrackTemporalFaceDecision beforePrevious) ||
            previous.Poll != beforePrevious.Poll + 1
        ) {
            return;
        }
        if (!beforePrevious.FiniteProjection)
        {
            _rawTrackTemporalCoverageNearExcluded++;
            return;
        }

        double beforeArea = beforePrevious.Accepted
            ? beforePrevious.TargetArea
            : 0.0;
        double neighborMaximum = Math.Max(beforeArea, currentArea);
        double neighborMinimum = Math.Min(beforeArea, currentArea);
        // A camera cut or a genuinely fast passage does not return to a
        // comparable footprint one frame later. Restrict the detector to an
        // ABA-like pair of neighboring areas before labelling the middle
        // frame a coverage discontinuity.
        double neighborTolerance = Math.Max(1.0, neighborMaximum * 0.25);
        if (Math.Abs(beforeArea - currentArea) > neighborTolerance)
            return;

        double loss = neighborMinimum - previousArea;
        bool isolatedLoss =
            neighborMinimum >= 1.0 &&
            loss >= Math.Max(1.0, neighborMinimum * 0.5);
        double spike = previousArea - neighborMaximum;
        bool isolatedSpike =
            previousArea >= 1.0 &&
            spike >= Math.Max(1.0, neighborMaximum * 0.5);
        if (!isolatedLoss && !isolatedSpike)
            return;

        double beforeCenterX = beforePrevious.TargetCenterX;
        double beforeCenterY = beforePrevious.TargetCenterY;
        double previousCenterX = previous.TargetCenterX;
        double previousCenterY = previous.TargetCenterY;
        double currentCenterX = current.TargetCenterX;
        double currentCenterY = current.TargetCenterY;
        double firstStepX = previousCenterX - beforeCenterX;
        double firstStepY = previousCenterY - beforeCenterY;
        double secondStepX = currentCenterX - previousCenterX;
        double secondStepY = currentCenterY - previousCenterY;
        double motionDot = firstStepX * secondStepX + firstStepY * secondStepY;
        if (motionDot < 0.0)
            _rawTrackTemporalCoverageMotionReversals++;
        double midpointResidual = Math.Sqrt(
            Math.Pow(previousCenterX - (beforeCenterX + currentCenterX) * 0.5, 2) +
            Math.Pow(previousCenterY - (beforeCenterY + currentCenterY) * 0.5, 2));
        double firstRotationStep = RawTrackRotationStep(
            in beforePrevious,
            in previous);
        double secondRotationStep = RawTrackRotationStep(
            in previous,
            in current);
        double firstTranslationStep = RawTrackTranslationStep(
            in beforePrevious,
            in previous);
        double secondTranslationStep = RawTrackTranslationStep(
            in previous,
            in current);
        string classification;
        if (RawTrackCameraCutStep(
                firstRotationStep,
                firstTranslationStep) ||
            RawTrackCameraCutStep(
                secondRotationStep,
                secondTranslationStep))
        {
            _rawTrackTemporalCoverageCameraCutEvents++;
            classification = "camera-cut";
        }
        else if (beforePrevious.Accepted == previous.Accepted &&
                 previous.Accepted == current.Accepted)
        {
            _rawTrackTemporalCoverageContinuousEdgeGrazes++;
            classification = "continuous-edge-graze";
        }
        else
        {
            _rawTrackTemporalCoverageCullToggleEvents++;
            classification = "cull-toggle";
        }
        _rawTrackTemporalCoverageMaximumMidpointResidual = Math.Max(
            _rawTrackTemporalCoverageMaximumMidpointResidual,
            midpointResidual);

        if (isolatedLoss)
        {
            _rawTrackTemporalCoverageIsolatedLosses++;
            _rawTrackTemporalCoverageMaximumLoss = Math.Max(
                _rawTrackTemporalCoverageMaximumLoss,
                loss);
        }
        if (isolatedSpike)
        {
            _rawTrackTemporalCoverageIsolatedSpikes++;
            _rawTrackTemporalCoverageMaximumSpike = Math.Max(
                _rawTrackTemporalCoverageMaximumSpike,
                spike);
        }
        double delta = isolatedLoss ? loss : spike;
        if (delta < _rawTrackTemporalCoverageTraceMinimum ||
            _rawTrackTemporalCoverageTraceCount >= 128)
            return;
        _rawTrackTemporalCoverageTraceCount++;
        Console.Error.WriteLine(
            $"[GT2-Raw-Track-Temporal-Coverage] " +
            $"n={_rawTrackTemporalCoverageTraceCount} " +
            $"kind={(isolatedLoss ? "loss" : "spike")} " +
            $"classification={classification} " +
            $"polls={beforePrevious.Poll}/{previous.Poll}/{current.Poll} " +
            $"object=0x{key.StableId:X8} " +
            $"model=0x{key.ModelPointer:X8} " +
            $"primitive=0x{key.PrimitiveAddress:X8} " +
            $"path={(int)key.ProjectionPath} " +
            $"stream={key.Stream} " +
            $"triangle={key.PrimitiveTriangle} " +
            $"item={current.Item} " +
             $"accepted=" +
            $"{(beforePrevious.Accepted ? 1 : 0)}/" +
            $"{(previous.Accepted ? 1 : 0)}/" +
             $"{(current.Accepted ? 1 : 0)} " +
             $"projectedArea={beforePrevious.ProjectedArea:R}/" +
             $"{previous.ProjectedArea:R}/{current.ProjectedArea:R} " +
             $"targetArea={beforeArea:R}/{previousArea:R}/{currentArea:R} " +
             $"center={beforeCenterX:R},{beforeCenterY:R}/" +
             $"{previousCenterX:R},{previousCenterY:R}/" +
             $"{currentCenterX:R},{currentCenterY:R} " +
             $"viewZ={beforePrevious.MinimumViewZ:R}.." +
             $"{beforePrevious.MaximumViewZ:R}/" +
             $"{previous.MinimumViewZ:R}..{previous.MaximumViewZ:R}/" +
             $"{current.MinimumViewZ:R}..{current.MaximumViewZ:R} " +
             $"motionDot={motionDot:R} midpointResidual={midpointResidual:R} " +
             $"rotationStep={firstRotationStep:R}/{secondRotationStep:R} " +
             $"translationStep={firstTranslationStep:R}/" +
             $"{secondTranslationStep:R} " +
             $"transform=0x{beforePrevious.TransformId:X16}/" +
             $"0x{previous.TransformId:X16}/0x{current.TransformId:X16} " +
             $"projected=" +
             $"{beforePrevious.Ax:R},{beforePrevious.Ay:R}/" +
             $"{beforePrevious.Bx:R},{beforePrevious.By:R}/" +
             $"{beforePrevious.Cx:R},{beforePrevious.Cy:R}|" +
             $"{previous.Ax:R},{previous.Ay:R}/" +
             $"{previous.Bx:R},{previous.By:R}/" +
             $"{previous.Cx:R},{previous.Cy:R}|" +
             $"{current.Ax:R},{current.Ay:R}/" +
             $"{current.Bx:R},{current.By:R}/" +
             $"{current.Cx:R},{current.Cy:R} " +
             $"delta={delta:R}");
    }

    static double RawTrackRotationStep(
        in RawTrackTemporalFaceDecision left,
        in RawTrackTemporalFaceDecision right)
    {
        double sum = 0.0;
        static double Square(int value) => (double)value * value;
        sum += Square(right.R00 - left.R00);
        sum += Square(right.R01 - left.R01);
        sum += Square(right.R02 - left.R02);
        sum += Square(right.R10 - left.R10);
        sum += Square(right.R11 - left.R11);
        sum += Square(right.R12 - left.R12);
        sum += Square(right.R20 - left.R20);
        sum += Square(right.R21 - left.R21);
        sum += Square(right.R22 - left.R22);
        return Math.Sqrt(sum) / RawTrackFixedUnit;
    }

    static double RawTrackTranslationStep(
        in RawTrackTemporalFaceDecision left,
        in RawTrackTemporalFaceDecision right)
    {
        double x = (double)right.TranslateX - left.TranslateX;
        double y = (double)right.TranslateY - left.TranslateY;
        double z = (double)right.TranslateZ - left.TranslateZ;
        return Math.Sqrt(x * x + y * y + z * z);
    }

    static bool RawTrackCameraCutStep(
        double rotationStep,
        double translationStep) =>
        // The bounded natural Seattle replay measured ordinary adjacent
        // camera steps below 0.05 fixed-matrix units and 700 translation
        // units. Authored replay cuts begin above 1.9 and 3,800. Keep a wide
        // evidence-backed gap so fast same-camera motion is not mislabeled.
        rotationStep >= 0.5 || translationStep >= 2_000.0;

    void BeginRawTrackFrame(long pendingFrame)
    {
        if (_rawTrackPendingFrame == pendingFrame)
            return;
        bool discontinuity =
            _rawTrackPendingFrame != long.MinValue &&
            pendingFrame != _rawTrackPendingFrame + 1;
        CompleteRawTrackFrame();
        _rawTrackPendingFrame = pendingFrame;
        _rawTrackInstances.Clear();
        _rawTrackResidentAuditInstances.Clear();
        _rawTrackTransforms.Clear();
        _rawTrackExpectedTriangles.Clear();
        _rawTrackSourceTriangles.Clear();
        _rawTrackPrimitiveDecisions.Clear();
        if (discontinuity)
        {
            _rawTrackFrameDiscontinuities++;
        }
        _rawTrackFrames++;
    }

    // A presentation/capture frame gap is not an asset-lifetime boundary.
    // GT2 can advance the guest frame counter while the same immutable course
    // remains loaded, especially in an unthrottled diagnostic.  Resident
    // definitions therefore survive frame gaps and are invalidated only by an
    // actual GPU reset, where guest RAM may subsequently be repopulated at the
    // same addresses with different course data.
    void InvalidateRawTrackResidency()
    {
        if (_rawTrackResidentMeshes.Count == 0 &&
            _rawTrackSourceMeshes.Count == 0)
        {
            return;
        }
        _rawTrackResidentMeshes.Clear();
        _rawTrackSourceMeshes.Clear();
        _rawTrackResidentInvalidations++;
    }

    void CaptureRawTrackMesh(
        uint meshPointer,
        IMemory memory,
        TrackMeshProjectionPath projectionPath)
    {
        if (!_rawTrackReplacementRegistered ||
            !_liveWorldCapture.CanRecordDeferredStatic)
            return;
        WorldObjectContext context = WorldCaptureContext.Current;
        if (context.Kind != WorldObjectKind.Track)
            throw RawTrackFailure(
                meshPointer,
                "track mesh hook has no active track object");
        if (context.ScenePass == WorldScenePass.Auxiliary)
            return;
        if (!IsRawTrackGuestRange(meshPointer, 0x44u))
            throw RawTrackFailure(meshPointer, "model header is outside guest RAM");

        uint vertexPointer = memory.ReadU32(meshPointer);
        uint vertexCount = memory.ReadU32(meshPointer + 0x2Cu) + 1u;
        int indexBits = WorldCaptureContext.CurrentTrackMeshIndexBits;
        bool auxiliaryFormat =
            WorldCaptureContext.CurrentTrackMeshUsesAuxiliaryFormat;
        ushort commonDepthShift = memory.ReadU16(0x1F800098u);
        ushort normalizationAdjustment = memory.ReadU16(0x1F80009Au);
        // All GT2 renderers insert into the same 4096-entry ordering table, but
        // their normalized GTE coordinates use two equivalent formulas. Main
        // course meshes insert SZ >> (3 + adjustment); auxiliary meshes insert
        // SZ << commonShift >> 13. Multiplying an OT bucket by 8192 gives one
        // shared continuous depth space, hence the exponents below.
        int depthScaleExponent = auxiliaryFormat
            ? commonDepthShift
            : 10 - normalizationAdjustment;
        WorldCaptureContext.SetCurrentDepthScaleExponent(depthScaleExponent);
        context = WorldCaptureContext.Current;
        uint indexMask = (1u << indexBits) - 1u;
        uint maximumVertexCount = 1u << indexBits;
        if (vertexCount is 0 || vertexCount > maximumVertexCount)
            throw RawTrackFailure(
                meshPointer,
                $"vertex count {vertexCount} exceeds the " +
                $"{indexBits}-bit index contract");
        if (!IsRawTrackGuestRange(vertexPointer, (ulong)vertexCount * 8u))
            throw RawTrackFailure(meshPointer, "vertex table is outside guest RAM");

        long pendingFrame = _projectedCaptureFrame + 1;
        // Deduplicate instances within GT2's authored scene, not within the
        // host presentation frame. The guest can author multiple generations
        // while all asynchronous renderer buffers are in flight.
        BeginRawTrackFrame(
            context.SceneGeneration != 0
                ? context.SceneGeneration
                : pendingFrame);
        if (RawTrackResidentMeshPathActive)
        {
            CaptureResidentRawTrackMesh(
                meshPointer,
                vertexPointer,
                vertexCount,
                indexBits,
                auxiliaryFormat,
                memory,
                projectionPath,
                pendingFrame);
            return;
        }

        long allocationStart = _rawTrackAllocationTraceRequested
            ? GC.GetAllocatedBytesForCurrentThread()
            : 0;
        long allocationAfterSetup = allocationStart;
        long allocationAfterBillboards = allocationStart;
        long allocationAfterStreams = allocationStart;
        GteProjectionOrigin[] origins =
            ArrayPool<GteProjectionOrigin>.Shared.Rent(checked((int)vertexCount));
        try
        {
            for (uint index = 0; index < vertexCount; index++)
            {
                uint address = vertexPointer + index * 8u;
                origins[index] = Gte.SnapshotProjectionOrigin(
                    unchecked((short)memory.ReadU16(address)),
                    unchecked((short)memory.ReadU16(address + 2u)),
                    unchecked((short)memory.ReadU16(address + 4u)));
            }
            uint[] sourceIdentities = RawTrackSourceIdentities(
                meshPointer,
                vertexPointer,
                vertexCount);
            if (_rawTrackAllocationTraceRequested)
            {
                allocationAfterSetup =
                    GC.GetAllocatedBytesForCurrentThread();
            }
            _rawTrackGuestDecisionContext = context;
            _rawTrackGuestDecisionTransform = origins[0].TransformId;
            _rawTrackGuestDecisionPath = projectionPath;
            var instanceKey = new RawTrackInstanceKey(
                context.StableId,
                context.ModelPointer,
                meshPointer,
                origins[0].TransformId,
                projectionPath);
            if (!_rawTrackInstances.Add(instanceKey))
            {
                _rawTrackDuplicateCalls++;
                return;
            }
            _rawTrackTransforms.Add(origins[0].TransformId);
            _rawTrackMaximumTransformsPerFrame = Math.Max(
                _rawTrackMaximumTransformsPerFrame,
                _rawTrackTransforms.Count);

            uint textureTableBase = memory.ReadU32(0x1F8003A0u);
            var environment = CurEnv();
            if (_rawTrackEnvironmentTraceCount < 12)
            {
                _rawTrackEnvironmentTraceCount++;
                Console.Error.WriteLine(
                    $"[GT2-Raw-Track-Env] n={_rawTrackEnvironmentTraceCount} " +
                    $"frame={pendingFrame} object=0x{context.StableId:X8} " +
                    $"clip={environment.ClipX0},{environment.ClipY0}-" +
                    $"{environment.ClipX1},{environment.ClipY1} " +
                    $"offset={environment.DrawOffsetX},{environment.DrawOffsetY} " +
                    $"projection={origins[0].ProjectionOffsetX / 65536.0:F2}," +
                    $"{origins[0].ProjectionOffsetY / 65536.0:F2}/" +
                    $"{origins[0].ProjectionPlane} " +
                    $"view0={origins[0].ViewX},{origins[0].ViewY}," +
                    $"{origins[0].ViewZ}");
            }
            long objectPrimitives = 0;
            long objectTriangles = 0;
            long objectTextured = 0;
            Span<uint> indices = stackalloc uint[4];
            Span<uint> colors = stackalloc uint[4];
            Span<ushort> packetUvs = stackalloc ushort[4];

            CaptureRawTrackBillboards(
                meshPointer,
                memory,
                textureTableBase,
                auxiliaryFormat,
                pendingFrame,
                projectionPath,
                in environment,
                ref objectPrimitives,
                ref objectTriangles,
                ref objectTextured);
            if (
                _rawTrackResidentEquivalenceAuditRequested &&
                _rawTrackResidentAuditInstances.Add(instanceKey)
            )
            {
                // Match the shipping insertion point exactly: generated
                // billboards remain capture-stream geometry, then the indexed
                // resident mesh is inserted. Recording the audit instance
                // before billboards could prove only a multiset, not the
                // transparency-sensitive command order.
                RawTrackResidentMeshDefinition auditDefinition =
                    ResolveResidentRawTrackMeshDefinition(
                        context.ModelPointer,
                        meshPointer,
                        vertexPointer,
                        vertexCount,
                        textureTableBase,
                        indexBits,
                        auxiliaryFormat,
                        projectionPath,
                        memory);
                _liveWorldCapture.RecordResidentTrackInstance(
                    pendingFrame,
                    auditDefinition.Key,
                    in environment,
                    in origins[0],
                    (ushort)CurTPage());
                _rawTrackResidentAuditInstanceCount++;
            }
            if (_rawTrackAllocationTraceRequested)
            {
                allocationAfterBillboards =
                    GC.GetAllocatedBytesForCurrentThread();
            }

            for (int stream = 0; stream < 8; stream++)
            {
                uint primitivePointer = memory.ReadU32(
                    meshPointer + 0x04u + checked((uint)stream * 4u));
                ushort primitiveCount = memory.ReadU16(
                    meshPointer + 0x30u + checked((uint)stream * 2u));
                int recordSize = auxiliaryFormat
                    ? AuxiliaryTrackPrimitiveRecordSizes[stream]
                    : RawTrackPrimitiveRecordSizes[stream];
                if (!IsRawTrackGuestRange(
                        primitivePointer,
                        (ulong)primitiveCount * (uint)recordSize))
                {
                    throw RawTrackFailure(
                        meshPointer,
                        $"primitive stream {stream} is outside guest RAM");
                }

                for (int item = 0; item < primitiveCount; item++)
                {
                    uint primitive = primitivePointer +
                        checked((uint)item * (uint)recordSize);
                    uint packedIndices = memory.ReadU32(primitive);
                    bool packedFaceBitSet =
                        (packedIndices & 0x80000000u) != 0;
                    if (packedFaceBitSet)
                        _rawTrackPackedFaceBitSetPrimitives++;
                    else
                        _rawTrackPackedFaceBitClearPrimitives++;
                    indices[0] = packedIndices & indexMask;
                    indices[1] = (packedIndices >> indexBits) & indexMask;
                    indices[2] =
                        (packedIndices >> (indexBits * 2)) & indexMask;
                    bool quad = (stream & 1) != 0;
                    indices[3] = quad
                        ? memory.ReadU32(primitive + 4u) & indexMask
                        : 0u;
                    int sourceVertexCount = quad ? 4 : 3;
                    for (int index = 0; index < sourceVertexCount; index++)
                    {
                        if (indices[index] >= vertexCount)
                        {
                            throw RawTrackFailure(
                                meshPointer,
                                $"stream {stream} primitive {item} has invalid " +
                                $"{indexBits}-bit vertex index " +
                                $"{indices[index]}/{vertexCount}");
                        }
                    }

                    bool textured = stream >= 4;
                    bool gouraud = (stream & 2) != 0;
                    uint commandColor = memory.ReadU32(primitive + 8u);
                    byte command = (byte)(commandColor >> 24);
                    byte expectedFamily = (byte)(
                        0x20 |
                        (quad ? 0x08 : 0x00) |
                        (textured ? 0x04 : 0x00) |
                        (gouraud ? 0x10 : 0x00));
                    if ((command & 0xFC) != expectedFamily)
                    {
                        throw RawTrackFailure(
                            meshPointer,
                            $"stream {stream} primitive {item} has opcode " +
                            $"0x{command:X2}, expected family 0x{expectedFamily:X2}");
                    }

                    colors[0] = commandColor;
                    bool embeddedTextureRecord = auxiliaryFormat && textured;
                    uint gouraudColorOffset = embeddedTextureRecord
                        ? 0x18u
                        : 0x0Cu;
                    colors[1] = gouraud
                        ? memory.ReadU32(primitive + gouraudColorOffset)
                        : commandColor;
                    colors[2] = gouraud
                        ? memory.ReadU32(primitive + gouraudColorOffset + 4u)
                        : commandColor;
                    colors[3] = gouraud && quad
                        ? memory.ReadU32(primitive + gouraudColorOffset + 8u)
                        : commandColor;

                    packetUvs.Clear();
                    ushort clut = 0;
                    ushort texturePage = (ushort)CurTPage();
                    if (textured)
                    {
                        if (embeddedTextureRecord)
                        {
                            // GT2's auxiliary renderer consumes an expanded
                            // record: three packet-ready UV/material words are
                            // embedded after the command color. Gouraud colors
                            // follow those words at +0x18. This is the exact
                            // 12-byte difference from the primary records.
                            uint texture0 = memory.ReadU32(primitive + 0x0Cu);
                            uint texture1 = memory.ReadU32(primitive + 0x10u);
                            uint texture2 = memory.ReadU32(primitive + 0x14u);
                            packetUvs[0] = (ushort)texture0;
                            packetUvs[1] = (ushort)texture1;
                            packetUvs[2] = (ushort)texture2;
                            packetUvs[3] = (ushort)(texture2 >> 16);
                            clut = (ushort)(texture0 >> 16);
                            texturePage = (ushort)(texture1 >> 16);
                        }
                        else
                        {
                            if (!IsRawTrackGuestRange(textureTableBase, 0x20u))
                            {
                                throw RawTrackFailure(
                                    meshPointer,
                                    "texture table base is outside guest RAM");
                            }
                            uint descriptor = memory.ReadU32(primitive + 4u);
                            uint entryOffset =
                                (descriptor >> 4) & 0x0007FFE0u;
                            uint entry = unchecked(textureTableBase + entryOffset);
                            if (!IsRawTrackGuestRange(entry, 0x20u))
                            {
                                throw RawTrackFailure(
                                    meshPointer,
                                    $"texture entry 0x{entry:X8} is outside guest RAM");
                            }
                            // Geometry residency and material LOD are separate
                            // concerns. GT2 authored a second texture record at
                            // +0x10 and selects it from the exact GTE NCLIP
                            // coverage of this primitive. Keeping every course
                            // object resident must not force the near record:
                            // doing so feeds distant one-pixel scenery a large
                            // high-frequency bitmap, which no post-filter can
                            // reconstruct into the authored silhouette.
                            uint materialCoverage = RawTrackMaterialCoverage(
                                origins,
                                indices[..sourceVertexCount],
                                quad,
                                projectionPath);
                            ushort materialThreshold = memory.ReadU16(
                                entry + 0x0Cu);
                            bool distantMaterial =
                                materialThreshold >= materialCoverage;
                            uint selectedEntry = distantMaterial
                                ? entry + 0x10u
                                : entry;
                            if (distantMaterial)
                                _rawTrackDistantMaterialPrimitives++;
                            else
                                _rawTrackNearMaterialPrimitives++;
                            uint texture0 = memory.ReadU32(selectedEntry);
                            uint texture1 = memory.ReadU32(selectedEntry + 4u);
                            uint texture2 = memory.ReadU32(selectedEntry + 8u);
                            packetUvs[0] = (ushort)texture0;
                            packetUvs[1] = (ushort)texture1;
                            packetUvs[2] = (ushort)texture2;
                            packetUvs[3] = (ushort)(texture2 >> 16);
                            clut = (ushort)(texture0 >> 16);
                            texturePage = (ushort)(texture1 >> 16);
                        }
                        objectTextured++;
                    }

                    var flags = new PrimFlags
                    {
                        Textured = textured && !_rawTrackFlatDebugRequested,
                        SemiTrans = (command & 0x02) != 0,
                        RawTexture = (command & 0x01) != 0,
                        Gouraud = gouraud,
                        ResidentCourse = true,
                        TPage = texturePage,
                        Clut = clut,
                        OtIndex = 0,
                    };
                    if (_rawTrackFlatDebugRequested)
                    {
                        uint debugColor = RawTrackDebugColor(stream);
                        colors[0] = debugColor;
                        colors[1] = debugColor;
                        colors[2] = debugColor;
                        colors[3] = debugColor;
                    }

                    objectPrimitives++;
                    if (!quad)
                    {
                        // The GTE and guest packet both use authored 0,1,2.
                        // With GT2's packed face bit set, its NCLIP branch
                        // admits the negative determinant. Use the continuous
                        // view-space equivalent so screen saturation and the
                        // original 4:3 viewport cannot alter facing.
                        GteProjectionOrigin originA = origins[indices[0]];
                        GteProjectionOrigin originB = origins[indices[1]];
                        GteProjectionOrigin originC = origins[indices[2]];
                        double determinant = RawTrackViewDeterminant(
                            in originA,
                            in originB,
                            in originC);
                        bool gteProjectionSaturated = false;
                        long gteNclip = _rawTrackCorrelationRequested
                            ? RawTrackGteNclip(
                                in originA,
                                in originB,
                                in originC,
                                out gteProjectionSaturated)
                            : 0;
                        bool hasFront =
                            originA.ViewZFixed >= RawTrackNearFixed ||
                            originB.ViewZFixed >= RawTrackNearFixed ||
                            originC.ViewZFixed >= RawTrackNearFixed;
                        bool hasBehind =
                            originA.ViewZFixed < RawTrackNearFixed ||
                            originB.ViewZFixed < RawTrackNearFixed ||
                            originC.ViewZFixed < RawTrackNearFixed;
                        bool nearCrossing = hasFront && hasBehind;
                        bool preClipAccepted = packedFaceBitSet
                            ? determinant < 0.0
                            : determinant != 0.0;
                        bool accepted = nearCrossing
                            ? RawTrackNearClipFacingAccepted(
                                in originA,
                                in originB,
                                in originC,
                                packedFaceBitSet,
                                acceptPositive: false,
                                preClipAccepted)
                            : preClipAccepted;
                        if (_rawTrackNearClipAuditRequested)
                        {
                            AuditRawTrackNearClipDecision(
                                in originA,
                                in originB,
                                in originC,
                                primitive,
                                projectionPath,
                                stream,
                                item,
                                0,
                                packedFaceBitSet,
                                acceptPositive: false,
                                preClipAccepted,
                                determinant);
                        }
                        if (_rawTrackTemporalFacingAuditRequested ||
                            _rawTrackTemporalCoverageAuditRequested)
                        {
                            AuditRawTrackTemporalFacing(
                                in originA,
                                in originB,
                                in originC,
                                in environment,
                                primitive,
                                projectionPath,
                                stream,
                                0,
                                item,
                                accepted,
                                determinant,
                                0.0);
                        }
                        if (_rawTrackFaceTracePoll >= 0)
                        {
                            TraceRawTrackFace(
                                in originA,
                                in originB,
                                in originC,
                                primitive,
                                projectionPath,
                                stream,
                                item,
                                0,
                                accepted,
                                determinant);
                        }
                        CountRawTrackFacingPrimitive(
                            projectionPath,
                            stream,
                            accepted,
                            determinant == 0.0,
                            false,
                            nearCrossing);
                        bool gteSaturatedAccepted = packedFaceBitSet
                            ? gteNclip < 0
                            : gteNclip != 0;
                        if (_rawTrackCorrelationRequested)
                        {
                            CatalogRawTrackPrimitive(
                                in originA,
                                primitive,
                                projectionPath,
                                stream,
                                item,
                                accepted,
                                gteSaturatedAccepted,
                                gteProjectionSaturated,
                                determinant,
                                0.0,
                                gteNclip,
                                0);
                            CatalogRawTrackSourceTriangle(
                                in originA,
                                in originB,
                                in originC,
                                primitive,
                                projectionPath,
                                stream,
                                item,
                                0,
                                packedFaceBitSet,
                                accepted,
                                gteSaturatedAccepted,
                                determinant,
                                determinant,
                                gteNclip,
                                gteNclip,
                                0);
                        }
                        if (!accepted)
                            continue;

                        HleVertex a = RawTrackVertex(
                            in originA,
                            colors[0],
                            packetUvs[0],
                            environment.DrawOffsetX,
                            environment.DrawOffsetY);
                        HleVertex b = RawTrackVertex(
                            in originB,
                            colors[1],
                            packetUvs[1],
                            environment.DrawOffsetX,
                            environment.DrawOffsetY);
                        HleVertex c = RawTrackVertex(
                            in originC,
                            colors[2],
                            packetUvs[2],
                            environment.DrawOffsetX,
                            environment.DrawOffsetY);
                        _liveWorldCapture.RecordResidentTrackTriangle(
                            pendingFrame,
                            in environment,
                            in a,
                            in b,
                            in c,
                            in originA,
                            in originB,
                            in originC,
                            in flags,
                            sourceIdentities[indices[0]],
                            sourceIdentities[indices[1]],
                            sourceIdentities[indices[2]]);
                        if (_rawTrackCorrelationRequested)
                        {
                            CatalogRawTrackTriangle(
                                in originA,
                                in originB,
                                in originC,
                                in a,
                                in b,
                                in c,
                                in flags,
                                primitive,
                                projectionPath,
                                stream,
                                item,
                                0);
                        }
                        if (_rawTrackVisibilityAuditRequested)
                        {
                            CountRawTrackVisibility(
                                in originA,
                                in originB,
                                in originC,
                                in environment);
                        }
                        objectTriangles++;
                    }
                    else
                    {
                        // GT2 makes two NCLIP decisions for an authored quad.
                        // The primary and alternate renderers feed different
                        // source orders into both the GTE FIFO and the later
                        // GPU quad packet. Keep those two orders distinct:
                        // packet correlation proves that treating the GTE
                        // order as the UV packet order rotates distant signs
                        // whenever GT2 changes projection paths.
                        bool primary = projectionPath ==
                            TrackMeshProjectionPath.Primary;
                        GteProjectionOrigin faceFirstA = primary
                            ? origins[indices[2]]
                            : origins[indices[0]];
                        GteProjectionOrigin faceFirstB = primary
                            ? origins[indices[1]]
                            : origins[indices[1]];
                        GteProjectionOrigin faceFirstC = primary
                            ? origins[indices[3]]
                            : origins[indices[2]];
                        GteProjectionOrigin faceSecondA = primary
                            ? origins[indices[1]]
                            : origins[indices[2]];
                        GteProjectionOrigin faceSecondB = primary
                            ? origins[indices[3]]
                            : origins[indices[0]];
                        GteProjectionOrigin faceSecondC = primary
                            ? origins[indices[0]]
                            : origins[indices[3]];
                        int firstCornerA = RawTrackQuadPacketCorner(
                            projectionPath, 0, 0);
                        int firstCornerB = RawTrackQuadPacketCorner(
                            projectionPath, 0, 1);
                        int firstCornerC = RawTrackQuadPacketCorner(
                            projectionPath, 0, 2);
                        int secondCornerA = RawTrackQuadPacketCorner(
                            projectionPath, 1, 0);
                        int secondCornerB = RawTrackQuadPacketCorner(
                            projectionPath, 1, 1);
                        int secondCornerC = RawTrackQuadPacketCorner(
                            projectionPath, 1, 2);
                        uint packetFirstIndexA = indices[firstCornerA];
                        uint packetFirstIndexB = indices[firstCornerB];
                        uint packetFirstIndexC = indices[firstCornerC];
                        uint packetSecondIndexA = indices[secondCornerA];
                        uint packetSecondIndexB = indices[secondCornerB];
                        uint packetSecondIndexC = indices[secondCornerC];
                        GteProjectionOrigin packetFirstA =
                            origins[packetFirstIndexA];
                        GteProjectionOrigin packetFirstB =
                            origins[packetFirstIndexB];
                        GteProjectionOrigin packetFirstC =
                            origins[packetFirstIndexC];
                        GteProjectionOrigin packetSecondA =
                            origins[packetSecondIndexA];
                        GteProjectionOrigin packetSecondB =
                            origins[packetSecondIndexB];
                        GteProjectionOrigin packetSecondC =
                            origins[packetSecondIndexC];
                        double firstDeterminant = RawTrackViewDeterminant(
                            in faceFirstA,
                            in faceFirstB,
                            in faceFirstC);
                        double secondDeterminant = RawTrackViewDeterminant(
                            in faceSecondA,
                            in faceSecondB,
                            in faceSecondC);
                        double packetFirstDeterminant =
                            RawTrackViewDeterminant(
                                in packetFirstA,
                                in packetFirstB,
                                in packetFirstC);
                        double packetSecondDeterminant =
                            RawTrackViewDeterminant(
                                in packetSecondA,
                                in packetSecondB,
                                in packetSecondC);
                        bool firstGteProjectionSaturated = false;
                        long firstNclip = _rawTrackCorrelationRequested
                            ? RawTrackGteNclip(
                                in faceFirstA,
                                in faceFirstB,
                                in faceFirstC,
                                out firstGteProjectionSaturated)
                            : 0;
                        bool secondGteProjectionSaturated = false;
                        long secondNclip = _rawTrackCorrelationRequested
                            ? RawTrackGteNclip(
                                in faceSecondA,
                                in faceSecondB,
                                in faceSecondC,
                                out secondGteProjectionSaturated)
                            : 0;
                        long packetFirstNclip = _rawTrackCorrelationRequested
                            ? RawTrackGteNclip(
                                in packetFirstA,
                                in packetFirstB,
                                in packetFirstC)
                            : 0;
                        long packetSecondNclip = _rawTrackCorrelationRequested
                            ? RawTrackGteNclip(
                                in packetSecondA,
                                in packetSecondB,
                                in packetSecondC)
                            : 0;
                        double facing = primary
                            ? firstDeterminant - secondDeterminant
                            : secondDeterminant - firstDeterminant;
                        bool hasFront =
                            origins[indices[0]].ViewZFixed >= RawTrackNearFixed ||
                            origins[indices[1]].ViewZFixed >= RawTrackNearFixed ||
                            origins[indices[2]].ViewZFixed >= RawTrackNearFixed ||
                            origins[indices[3]].ViewZFixed >= RawTrackNearFixed;
                        bool hasBehind =
                            origins[indices[0]].ViewZFixed < RawTrackNearFixed ||
                            origins[indices[1]].ViewZFixed < RawTrackNearFixed ||
                            origins[indices[2]].ViewZFixed < RawTrackNearFixed ||
                            origins[indices[3]].ViewZFixed < RawTrackNearFixed;
                        bool nearCrossing = hasFront && hasBehind;
                        // GT2 admits the complete GPU quad when either NCLIP
                        // result is front-facing. Preserve that authored
                        // primitive-level decision: independently culling the
                        // two modern triangles removes half of railings,
                        // foliage, and road quads at ordinary camera angles.
                        // Perspective-correct interpolation and homogeneous
                        // clipping handle the old affine/near-plane failure
                        // modes without changing the quad's visibility.
                        bool firstPreClipAccepted = RawTrackQuadTriangleAccepted(
                            packedFaceBitSet,
                            projectionPath,
                            0,
                            packetFirstDeterminant);
                        bool secondPreClipAccepted = RawTrackQuadTriangleAccepted(
                            packedFaceBitSet,
                            projectionPath,
                            1,
                            packetSecondDeterminant);
                        bool firstAccepted = nearCrossing
                            ? RawTrackNearClipFacingAccepted(
                                in packetFirstA,
                                in packetFirstB,
                                in packetFirstC,
                                packedFaceBitSet,
                                acceptPositive: primary,
                                firstPreClipAccepted)
                            : firstPreClipAccepted;
                        bool secondAccepted = nearCrossing
                            ? RawTrackNearClipFacingAccepted(
                                in packetSecondA,
                                in packetSecondB,
                                in packetSecondC,
                                packedFaceBitSet,
                                acceptPositive: !primary,
                                secondPreClipAccepted)
                            : secondPreClipAccepted;
                        if (_rawTrackNearClipAuditRequested)
                        {
                            AuditRawTrackNearClipDecision(
                                in packetFirstA,
                                in packetFirstB,
                                in packetFirstC,
                                primitive,
                                projectionPath,
                                stream,
                                item,
                                0,
                                packedFaceBitSet,
                                acceptPositive: primary,
                                firstPreClipAccepted,
                                packetFirstDeterminant);
                            AuditRawTrackNearClipDecision(
                                in packetSecondA,
                                in packetSecondB,
                                in packetSecondC,
                                primitive,
                                projectionPath,
                                stream,
                                item,
                                1,
                                packedFaceBitSet,
                                acceptPositive: !primary,
                                secondPreClipAccepted,
                                packetSecondDeterminant);
                        }
                        bool accepted = firstAccepted || secondAccepted;
                        if (_rawTrackTemporalFacingAuditRequested ||
                            _rawTrackTemporalCoverageAuditRequested)
                        {
                            AuditRawTrackTemporalFacing(
                                in packetFirstA,
                                in packetFirstB,
                                in packetFirstC,
                                in environment,
                                primitive,
                                projectionPath,
                                stream,
                                0,
                                item,
                                accepted,
                                packetFirstDeterminant,
                                0.0);
                            AuditRawTrackTemporalFacing(
                                in packetSecondA,
                                in packetSecondB,
                                in packetSecondC,
                                in environment,
                                primitive,
                                projectionPath,
                                stream,
                                1,
                                item,
                                accepted,
                                packetSecondDeterminant,
                                0.0);
                        }
                        if (_rawTrackFaceTracePoll >= 0)
                        {
                            TraceRawTrackFace(
                                in packetFirstA,
                                in packetFirstB,
                                in packetFirstC,
                                primitive,
                                projectionPath,
                                stream,
                                item,
                                0,
                                accepted,
                                packetFirstDeterminant);
                            TraceRawTrackFace(
                                in packetSecondA,
                                in packetSecondB,
                                in packetSecondC,
                                primitive,
                                projectionPath,
                                stream,
                                item,
                                1,
                                accepted,
                                packetSecondDeterminant);
                        }
                        bool strictFacing =
                            firstAccepted && secondAccepted;
                        bool aggregateOnly =
                            packedFaceBitSet && accepted && !strictFacing;
                        if (aggregateOnly)
                        {
                            bool firstDroppedTarget =
                                !firstAccepted && RawTrackPotentiallyVisible(
                                    in packetFirstA,
                                    in packetFirstB,
                                    in packetFirstC,
                                    in environment);
                            bool secondDroppedTarget =
                                !secondAccepted && RawTrackPotentiallyVisible(
                                    in packetSecondA,
                                    in packetSecondB,
                                    in packetSecondC,
                                    in environment);
                            if (firstDroppedTarget || secondDroppedTarget)
                            {
                                _rawTrackFacingAggregateOnlyTargetPrimitives++;
                                _rawTrackFacingAggregateRestoredTargetTriangles +=
                                    (firstDroppedTarget ? 1 : 0) +
                                    (secondDroppedTarget ? 1 : 0);
                            }
                        }
                        CountRawTrackFacingPrimitive(
                            projectionPath,
                            stream,
                            accepted,
                            packetFirstDeterminant == 0.0 &&
                                packetSecondDeterminant == 0.0,
                            aggregateOnly,
                            nearCrossing);
                        bool gteSaturatedAccepted = packedFaceBitSet
                            ? (primary
                                ? firstNclip > 0 || secondNclip < 0
                                : firstNclip < 0 || secondNclip > 0)
                            : firstNclip != 0 || secondNclip != 0;
                        if (_rawTrackCorrelationRequested)
                        {
                            CatalogRawTrackPrimitive(
                                in packetFirstA,
                                primitive,
                                projectionPath,
                                stream,
                                item,
                                accepted,
                                gteSaturatedAccepted,
                                firstGteProjectionSaturated ||
                                    secondGteProjectionSaturated,
                                firstDeterminant,
                                secondDeterminant,
                                firstNclip,
                                secondNclip);
                            CatalogRawTrackSourceTriangle(
                                in packetFirstA,
                                in packetFirstB,
                                in packetFirstC,
                                primitive,
                                projectionPath,
                                stream,
                                item,
                                0,
                                packedFaceBitSet,
                                accepted,
                                gteSaturatedAccepted,
                                packetFirstDeterminant,
                                facing,
                                packetFirstNclip,
                                firstNclip,
                                secondNclip);
                            CatalogRawTrackSourceTriangle(
                                in packetSecondA,
                                in packetSecondB,
                                in packetSecondC,
                                primitive,
                                projectionPath,
                                stream,
                                item,
                                1,
                                packedFaceBitSet,
                                accepted,
                                gteSaturatedAccepted,
                                packetSecondDeterminant,
                                facing,
                                packetSecondNclip,
                                firstNclip,
                                secondNclip);
                        }
                        if (!accepted)
                            continue;

                        int firstUvA = RawTrackQuadPacketUvCorner(
                            projectionPath, 0, 0);
                        int firstUvB = RawTrackQuadPacketUvCorner(
                            projectionPath, 0, 1);
                        int firstUvC = RawTrackQuadPacketUvCorner(
                            projectionPath, 0, 2);
                        int secondUvA = RawTrackQuadPacketUvCorner(
                            projectionPath, 1, 0);
                        int secondUvB = RawTrackQuadPacketUvCorner(
                            projectionPath, 1, 1);
                        int secondUvC = RawTrackQuadPacketUvCorner(
                            projectionPath, 1, 2);
                        HleVertex firstVertexA = RawTrackVertex(
                            in packetFirstA,
                            colors[firstCornerA],
                            packetUvs[firstUvA],
                            environment.DrawOffsetX,
                            environment.DrawOffsetY);
                        HleVertex firstVertexB = RawTrackVertex(
                            in packetFirstB,
                            colors[firstCornerB],
                            packetUvs[firstUvB],
                            environment.DrawOffsetX,
                            environment.DrawOffsetY);
                        HleVertex firstVertexC = RawTrackVertex(
                            in packetFirstC,
                            colors[firstCornerC],
                            packetUvs[firstUvC],
                            environment.DrawOffsetX,
                            environment.DrawOffsetY);
                        HleVertex secondVertexA = RawTrackVertex(
                            in packetSecondA,
                            colors[secondCornerA],
                            packetUvs[secondUvA],
                            environment.DrawOffsetX,
                            environment.DrawOffsetY);
                        HleVertex secondVertexB = RawTrackVertex(
                            in packetSecondB,
                            colors[secondCornerB],
                            packetUvs[secondUvB],
                            environment.DrawOffsetX,
                            environment.DrawOffsetY);
                        HleVertex secondVertexC = RawTrackVertex(
                            in packetSecondC,
                            colors[secondCornerC],
                            packetUvs[secondUvC],
                            environment.DrawOffsetX,
                            environment.DrawOffsetY);
                        _liveWorldCapture.RecordResidentTrackTriangle(
                            pendingFrame,
                            in environment,
                            in firstVertexA,
                            in firstVertexB,
                            in firstVertexC,
                            in packetFirstA,
                            in packetFirstB,
                            in packetFirstC,
                            in flags,
                            sourceIdentities[packetFirstIndexA],
                            sourceIdentities[packetFirstIndexB],
                            sourceIdentities[packetFirstIndexC]);
                        if (_rawTrackCorrelationRequested)
                        {
                            CatalogRawTrackTriangle(
                                in packetFirstA,
                                in packetFirstB,
                                in packetFirstC,
                                in firstVertexA,
                                in firstVertexB,
                                in firstVertexC,
                                in flags,
                                primitive,
                                projectionPath,
                                stream,
                                item,
                                0);
                        }
                        if (_rawTrackVisibilityAuditRequested)
                        {
                            CountRawTrackVisibility(
                                in packetFirstA,
                                in packetFirstB,
                                in packetFirstC,
                                in environment);
                        }
                        objectTriangles++;
                        _liveWorldCapture.RecordResidentTrackTriangle(
                            pendingFrame,
                            in environment,
                            in secondVertexA,
                            in secondVertexB,
                            in secondVertexC,
                            in packetSecondA,
                            in packetSecondB,
                            in packetSecondC,
                            in flags,
                            sourceIdentities[packetSecondIndexA],
                            sourceIdentities[packetSecondIndexB],
                            sourceIdentities[packetSecondIndexC]);
                        if (_rawTrackCorrelationRequested)
                        {
                            CatalogRawTrackTriangle(
                                in packetSecondA,
                                in packetSecondB,
                                in packetSecondC,
                                in secondVertexA,
                                in secondVertexB,
                                in secondVertexC,
                                in flags,
                                primitive,
                                projectionPath,
                                stream,
                                item,
                                1);
                        }
                        if (_rawTrackVisibilityAuditRequested)
                        {
                            CountRawTrackVisibility(
                                in packetSecondA,
                                in packetSecondB,
                                in packetSecondC,
                                in environment);
                        }
                        objectTriangles++;
                    }
                }
            }

            _rawTrackObjects++;
            _rawTrackSourcePrimitives += objectPrimitives;
            _rawTrackTriangles += objectTriangles;
            _rawTrackTexturedPrimitives += objectTextured;
            _rawTrackCurrentFrameObjects++;
            _rawTrackCurrentFrameSourcePrimitives += objectPrimitives;
            _rawTrackCurrentFrameTriangles += objectTriangles;
            _liveWorldCapture.RegisterResidentTrackProjection(
                in origins[0],
                checked((int)objectTriangles * 3));
            if (_rawTrackAllocationTraceRequested)
            {
                allocationAfterStreams =
                    GC.GetAllocatedBytesForCurrentThread();
            }
        }
        finally
        {
            ArrayPool<GteProjectionOrigin>.Shared.Return(origins);
            if (_rawTrackAllocationTraceRequested)
            {
                long allocationAfterPool =
                    GC.GetAllocatedBytesForCurrentThread();
                _rawTrackCurrentFrameSetupAllocatedBytes +=
                    allocationAfterSetup - allocationStart;
                _rawTrackCurrentFrameBillboardAllocatedBytes +=
                    allocationAfterBillboards - allocationAfterSetup;
                _rawTrackCurrentFrameStreamAllocatedBytes +=
                    allocationAfterStreams - allocationAfterBillboards;
                _rawTrackCurrentFramePoolAllocatedBytes +=
                    allocationAfterPool - allocationAfterStreams;
            }
        }
    }

    void CaptureResidentRawTrackMesh(
        uint meshPointer,
        uint vertexPointer,
        uint vertexCount,
        int indexBits,
        bool auxiliaryFormat,
        IMemory memory,
        TrackMeshProjectionPath projectionPath,
        long pendingFrame)
    {
        WorldObjectContext context = WorldCaptureContext.Current;
        short firstX = unchecked((short)memory.ReadU16(vertexPointer));
        short firstY = unchecked((short)memory.ReadU16(vertexPointer + 2u));
        short firstZ = unchecked((short)memory.ReadU16(vertexPointer + 4u));
        GteProjectionOrigin origin = Gte.SnapshotProjectionOrigin(
            firstX,
            firstY,
            firstZ);
        int correlationEndPoll = _rawTrackCorrelationTraceEndPoll >= 0
            ? Math.Max(
                _rawTrackCorrelationTraceStartPoll,
                _rawTrackCorrelationTraceEndPoll)
            : _rawTrackCorrelationTraceStartPoll;
        int currentPoll = Host.InputManager.CurrentPoll;
        if (_rawTrackCorrelationTraceModel != 0 &&
            context.ModelPointer == _rawTrackCorrelationTraceModel &&
            _rawTrackCorrelationTraceStartPoll >= 0 &&
            currentPoll >= _rawTrackCorrelationTraceStartPoll &&
            currentPoll <= correlationEndPoll)
        {
            // func_8007B688 writes these two values immediately before the
            // authored track projector runs.  The low half is the shift used
            // to return GTE SZ to GT2's common ordering-table depth scale;
            // the high half is the normalization adjustment folded into it.
            // Record the source state here, while this exact model transform
            // still owns the scratchpad, so cross-object depth can be derived
            // from guest evidence instead of inferred from final pixels.
            ushort commonDepthShift = memory.ReadU16(0x1F800098u);
            ushort normalizationAdjustment = memory.ReadU16(0x1F80009Au);
            Console.Error.WriteLine(
                $"[GT2-Raw-Track-Depth-State] poll={currentPoll} " +
                $"object=0x{context.StableId:X8} " +
                $"model=0x{context.ModelPointer:X8} " +
                $"path={(int)projectionPath} " +
                $"commonShift={commonDepthShift} " +
                $"normalizationAdjustment={normalizationAdjustment} " +
                $"commonExponent={context.DepthScaleExponent} " +
                $"translate={origin.TranslateX}/" +
                $"{origin.TranslateY}/{origin.TranslateZ}");
        }
        var instanceKey = new RawTrackInstanceKey(
            context.StableId,
            context.ModelPointer,
            meshPointer,
            origin.TransformId,
            projectionPath);
        if (!_rawTrackInstances.Add(instanceKey))
        {
            _rawTrackDuplicateCalls++;
            return;
        }
        _rawTrackTransforms.Add(origin.TransformId);
        _rawTrackMaximumTransformsPerFrame = Math.Max(
            _rawTrackMaximumTransformsPerFrame,
            _rawTrackTransforms.Count);

        uint textureTableBase = memory.ReadU32(0x1F8003A0u);
        RawTrackResidentMeshDefinition definition =
            ResolveResidentRawTrackMeshDefinition(
                context.ModelPointer,
                meshPointer,
                vertexPointer,
                vertexCount,
                textureTableBase,
                indexBits,
                auxiliaryFormat,
                projectionPath,
                memory);

        var environment = CurEnv();
        long objectPrimitives = 0;
        long objectTriangles = 0;
        long objectTextured = 0;
        CaptureRawTrackBillboards(
            meshPointer,
            memory,
            textureTableBase,
            auxiliaryFormat,
            pendingFrame,
            projectionPath,
            in environment,
            ref objectPrimitives,
            ref objectTriangles,
            ref objectTextured);
        _liveWorldCapture.RecordResidentTrackInstance(
            pendingFrame,
            definition.Key,
            in environment,
            in origin,
            (ushort)CurTPage());
        _liveWorldCapture.RegisterResidentTrackProjection(
            in origin,
            definition.PotentialTriangles * 3);

        objectPrimitives += definition.SourcePrimitives;
        objectTriangles += definition.PotentialTriangles;
        objectTextured += definition.TexturedPrimitives;
        _rawTrackPackedFaceBitSetPrimitives +=
            definition.PackedFaceBitSetPrimitives;
        _rawTrackPackedFaceBitClearPrimitives +=
            definition.PackedFaceBitClearPrimitives;
        _rawTrackObjects++;
        _rawTrackResidentInstances++;
        _rawTrackSourcePrimitives += objectPrimitives;
        _rawTrackTriangles += objectTriangles;
        _rawTrackTexturedPrimitives += objectTextured;
        _rawTrackCurrentFrameObjects++;
        _rawTrackCurrentFrameSourcePrimitives += objectPrimitives;
        _rawTrackCurrentFrameTriangles += objectTriangles;
    }

    RawTrackResidentMeshDefinition ResolveResidentRawTrackMeshDefinition(
        uint modelPointer,
        uint meshPointer,
        uint vertexPointer,
        uint vertexCount,
        uint textureTableBase,
        int indexBits,
        bool auxiliaryFormat,
        TrackMeshProjectionPath projectionPath,
        IMemory memory)
    {
        var definitionKey = new RawTrackResidentMeshKey(
            modelPointer,
            meshPointer,
            vertexPointer,
            vertexCount,
            textureTableBase,
            indexBits,
            auxiliaryFormat,
            projectionPath);
        if (_rawTrackResidentMeshes.TryGetValue(
                definitionKey,
                out RawTrackResidentMeshDefinition? definition))
        {
            return definition;
        }

        definition = RegisterResidentRawTrackMeshDefinition(
            in definitionKey,
            memory);

        // Seattle's immutable course objects can move between GT2's primary
        // and intersecting packet orders as the camera advances. Register the
        // exact companion definition with the first catalog hit so a later
        // screen-boundary transition cannot allocate, decode, or upload a new
        // mesh during the authored 60 Hz race.
        TrackMeshProjectionPath companionPath =
            projectionPath == TrackMeshProjectionPath.Primary
                ? TrackMeshProjectionPath.Alternate
                : TrackMeshProjectionPath.Primary;
        var companionKey = new RawTrackResidentMeshKey(
            modelPointer,
            meshPointer,
            vertexPointer,
            vertexCount,
            textureTableBase,
            indexBits,
            auxiliaryFormat,
            companionPath);
        if (!_rawTrackResidentMeshes.ContainsKey(companionKey))
            RegisterResidentRawTrackMeshDefinition(in companionKey, memory);

        return definition;
    }

    RawTrackResidentMeshDefinition RegisterResidentRawTrackMeshDefinition(
        in RawTrackResidentMeshKey definitionKey,
        IMemory memory)
    {
        RawTrackResidentMeshDefinition definition =
            BuildResidentRawTrackMesh(in definitionKey, memory);
        _rawTrackResidentMeshes.Add(definitionKey, definition);
        _liveWorldCapture.RegisterResidentTrackMesh(definition.Bytes);
        _rawTrackResidentDefinitions++;
        if (_rawTrackResidentDiscoveryTraceRequested)
        {
            WorldObjectContext context = WorldCaptureContext.Current;
            Console.Error.WriteLine(
                $"[GT2-Resident-Discovery] " +
                $"definition={_rawTrackResidentDefinitions} " +
                $"frame={_rawTrackPendingFrame} " +
                $"stable=0x{context.StableId:X8} " +
                $"model=0x{definitionKey.ModelPointer:X8} " +
                $"mesh=0x{definitionKey.MeshPointer:X8} " +
                $"path={definitionKey.ProjectionPath} " +
                $"auxiliary={definitionKey.AuxiliaryFormat} " +
                $"vertices={definitionKey.VertexCount} " +
                $"primitives={definition.SourcePrimitives} " +
                $"triangles={definition.PotentialTriangles} " +
                $"bytes={definition.Bytes.Length}");
        }
        return definition;
    }

    RawTrackResidentMeshDefinition BuildResidentRawTrackMesh(
        in RawTrackResidentMeshKey key,
        IMemory memory)
    {
        int primitiveCount = 0;
        for (int stream = 0; stream < 8; stream++)
            primitiveCount += memory.ReadU16(
                key.MeshPointer + 0x30u + checked((uint)stream * 2u));
        int size = checked(
            RawTrackResidentMeshHeaderSize +
            (int)key.VertexCount * RawTrackResidentVertexStride +
            primitiveCount * RawTrackResidentPrimitiveStride);
        byte[] bytes = new byte[size];
        Span<byte> destination = bytes;
        BinaryPrimitives.WriteUInt64LittleEndian(
            destination,
            RawTrackResidentMeshMagic);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[8..], 1);
        BinaryPrimitives.WriteUInt32LittleEndian(
            destination[12..],
            RawTrackResidentMeshHeaderSize);
        ulong residentKey = RawTrackResidentKey(in key);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[16..], residentKey);
        BinaryPrimitives.WriteUInt32LittleEndian(
            destination[24..],
            key.VertexCount);
        BinaryPrimitives.WriteUInt32LittleEndian(
            destination[28..],
            checked((uint)primitiveCount));

        int offset = RawTrackResidentMeshHeaderSize;
        for (uint index = 0; index < key.VertexCount; index++)
        {
            uint address = key.VertexPointer + index * 8u;
            short x = unchecked((short)memory.ReadU16(address));
            short y = unchecked((short)memory.ReadU16(address + 2u));
            short z = unchecked((short)memory.ReadU16(address + 4u));
            BinaryPrimitives.WriteInt16LittleEndian(destination[offset..], x);
            BinaryPrimitives.WriteInt16LittleEndian(destination[(offset + 2)..], y);
            BinaryPrimitives.WriteInt16LittleEndian(destination[(offset + 4)..], z);
            BinaryPrimitives.WriteUInt32LittleEndian(
                destination[(offset + 8)..],
                RawTrackIndexedSourceIdentity(address));
            offset += RawTrackResidentVertexStride;
        }

        uint indexMask = (1u << key.IndexBits) - 1u;
        Span<uint> colors = stackalloc uint[4];
        Span<ushort> packetUvs = stackalloc ushort[4];
        Span<ushort> farUvs = stackalloc ushort[4];
        int texturedPrimitives = 0;
        int packedFaceSet = 0;
        int packedFaceClear = 0;
        int potentialTriangles = 0;
        for (int stream = 0; stream < 8; stream++)
        {
            uint primitivePointer = memory.ReadU32(
                key.MeshPointer + 0x04u + checked((uint)stream * 4u));
            ushort streamCount = memory.ReadU16(
                key.MeshPointer + 0x30u + checked((uint)stream * 2u));
            int recordSize = key.AuxiliaryFormat
                ? AuxiliaryTrackPrimitiveRecordSizes[stream]
                : RawTrackPrimitiveRecordSizes[stream];
            if (!IsRawTrackGuestRange(
                    primitivePointer,
                    (ulong)streamCount * (uint)recordSize))
            {
                throw RawTrackFailure(
                    key.MeshPointer,
                    $"primitive stream {stream} is outside guest RAM");
            }
            for (int item = 0; item < streamCount; item++)
            {
                uint primitive = primitivePointer +
                    checked((uint)item * (uint)recordSize);
                uint packedIndices = memory.ReadU32(primitive);
                bool faceBit = (packedIndices & 0x80000000u) != 0;
                if (faceBit) packedFaceSet++;
                else packedFaceClear++;
                bool quad = (stream & 1) != 0;
                bool textured = stream >= 4;
                bool gouraud = (stream & 2) != 0;
                uint index0 = packedIndices & indexMask;
                uint index1 = (packedIndices >> key.IndexBits) & indexMask;
                uint index2 =
                    (packedIndices >> (key.IndexBits * 2)) & indexMask;
                uint index3 = quad
                    ? memory.ReadU32(primitive + 4u) & indexMask
                    : 0u;
                if (index0 >= key.VertexCount || index1 >= key.VertexCount ||
                    index2 >= key.VertexCount ||
                    (quad && index3 >= key.VertexCount))
                {
                    throw RawTrackFailure(
                        key.MeshPointer,
                        $"stream {stream} primitive {item} has an invalid " +
                        $"{key.IndexBits}-bit vertex index");
                }
                uint commandColor = memory.ReadU32(primitive + 8u);
                byte command = (byte)(commandColor >> 24);
                byte expectedFamily = (byte)(
                    0x20 |
                    (quad ? 0x08 : 0) |
                    (textured ? 0x04 : 0) |
                    (gouraud ? 0x10 : 0));
                if ((command & 0xFC) != expectedFamily)
                {
                    throw RawTrackFailure(
                        key.MeshPointer,
                        $"stream {stream} primitive {item} has opcode " +
                        $"0x{command:X2}, expected family 0x{expectedFamily:X2}");
                }
                bool embedded = key.AuxiliaryFormat && textured;
                uint gouraudOffset = embedded ? 0x18u : 0x0Cu;
                colors[0] = commandColor;
                colors[1] = gouraud
                    ? memory.ReadU32(primitive + gouraudOffset)
                    : commandColor;
                colors[2] = gouraud
                    ? memory.ReadU32(primitive + gouraudOffset + 4u)
                    : commandColor;
                colors[3] = gouraud && quad
                    ? memory.ReadU32(primitive + gouraudOffset + 8u)
                    : commandColor;

                packetUvs.Clear();
                ushort nearPage = 0;
                ushort nearClut = 0;
                ushort farPage = 0;
                ushort farClut = 0;
                ushort threshold = 0;
                farUvs.Clear();
                if (textured)
                {
                    texturedPrimitives++;
                    uint near0;
                    uint near1;
                    uint near2;
                    uint far0;
                    uint far1;
                    uint far2;
                    if (embedded)
                    {
                        near0 = memory.ReadU32(primitive + 0x0Cu);
                        near1 = memory.ReadU32(primitive + 0x10u);
                        near2 = memory.ReadU32(primitive + 0x14u);
                        far0 = near0;
                        far1 = near1;
                        far2 = near2;
                    }
                    else
                    {
                        uint descriptor = memory.ReadU32(primitive + 4u);
                        uint entryOffset =
                            (descriptor >> 4) & 0x0007FFE0u;
                        uint entry = unchecked(
                            key.TextureTableBase + entryOffset);
                        if (!IsRawTrackGuestRange(entry, 0x20u))
                        {
                            throw RawTrackFailure(
                                key.MeshPointer,
                                $"texture entry 0x{entry:X8} is outside guest RAM");
                        }
                        near0 = memory.ReadU32(entry);
                        near1 = memory.ReadU32(entry + 4u);
                        near2 = memory.ReadU32(entry + 8u);
                        threshold = memory.ReadU16(entry + 0x0Cu);
                        far0 = memory.ReadU32(entry + 0x10u);
                        far1 = memory.ReadU32(entry + 0x14u);
                        far2 = memory.ReadU32(entry + 0x18u);
                    }
                    packetUvs[0] = (ushort)near0;
                    packetUvs[1] = (ushort)near1;
                    packetUvs[2] = (ushort)near2;
                    packetUvs[3] = (ushort)(near2 >> 16);
                    nearClut = (ushort)(near0 >> 16);
                    nearPage = (ushort)(near1 >> 16);
                    farUvs[0] = (ushort)far0;
                    farUvs[1] = (ushort)far1;
                    farUvs[2] = (ushort)far2;
                    farUvs[3] = (ushort)(far2 >> 16);
                    farClut = (ushort)(far0 >> 16);
                    farPage = (ushort)(far1 >> 16);
                }

                uint flags = 0;
                if (quad) flags |= 1u << 0;
                if (faceBit) flags |= 1u << 1;
                if (textured) flags |= 1u << 2;
                if ((command & 0x02) != 0) flags |= 1u << 3;
                if ((command & 0x01) != 0) flags |= 1u << 4;
                if (gouraud) flags |= 1u << 5;
                if (key.ProjectionPath == TrackMeshProjectionPath.Primary)
                    flags |= 1u << 6;
                BinaryPrimitives.WriteUInt32LittleEndian(
                    destination[offset..],
                    primitive);
                BinaryPrimitives.WriteUInt32LittleEndian(
                    destination[(offset + 4)..],
                    flags);
                BinaryPrimitives.WriteUInt16LittleEndian(
                    destination[(offset + 8)..],
                    checked((ushort)stream));
                BinaryPrimitives.WriteUInt16LittleEndian(
                    destination[(offset + 10)..],
                    threshold);
                BinaryPrimitives.WriteUInt16LittleEndian(
                    destination[(offset + 12)..],
                    checked((ushort)index0));
                BinaryPrimitives.WriteUInt16LittleEndian(
                    destination[(offset + 14)..],
                    checked((ushort)index1));
                BinaryPrimitives.WriteUInt16LittleEndian(
                    destination[(offset + 16)..],
                    checked((ushort)index2));
                BinaryPrimitives.WriteUInt16LittleEndian(
                    destination[(offset + 18)..],
                    checked((ushort)index3));
                WriteRawTrackResidentMaterial(
                    destination[(offset + 24)..],
                    nearPage,
                    nearClut,
                    packetUvs,
                    colors);
                WriteRawTrackResidentMaterial(
                    destination[(offset + 52)..],
                    farPage,
                    farClut,
                    farUvs,
                    colors);
                offset += RawTrackResidentPrimitiveStride;
                potentialTriangles += quad ? 2 : 1;
            }
        }
        if (offset != bytes.Length)
            throw new InvalidDataException(
                "Resident raw-track definition size did not close exactly.");
        return new RawTrackResidentMeshDefinition(
            residentKey,
            bytes,
            primitiveCount,
            potentialTriangles,
            texturedPrimitives,
            packedFaceSet,
            packedFaceClear);
    }

    static void WriteRawTrackResidentMaterial(
        Span<byte> destination,
        ushort texturePage,
        ushort clut,
        ReadOnlySpan<ushort> uvs,
        ReadOnlySpan<uint> colors)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(destination, texturePage);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[2..], clut);
        for (int index = 0; index < 4; index++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(
                destination[(4 + index * 2)..],
                uvs[index]);
            BinaryPrimitives.WriteUInt32LittleEndian(
                destination[(12 + index * 4)..],
                colors[index]);
        }
    }

    static ulong RawTrackResidentKey(in RawTrackResidentMeshKey key)
    {
        const ulong Offset = 14695981039346656037UL;
        const ulong Prime = 1099511628211UL;
        ulong hash = Offset;
        static void Add(ref ulong value, uint item)
        {
            for (int shift = 0; shift < 32; shift += 8)
            {
                value ^= (byte)(item >> shift);
                value *= Prime;
            }
        }
        Add(ref hash, key.ModelPointer);
        Add(ref hash, key.MeshPointer);
        Add(ref hash, key.VertexPointer);
        Add(ref hash, key.VertexCount);
        Add(ref hash, key.TextureTableBase);
        Add(ref hash, checked((uint)key.IndexBits));
        Add(ref hash, key.AuxiliaryFormat ? 1u : 0u);
        Add(ref hash, checked((uint)key.ProjectionPath));
        return hash == 0 ? 1UL : hash;
    }

    /// <summary>
    /// GT2 stores camera-facing course foliage and signs at model +0x24/+0x40.
    /// Primary records are 16 bytes and reference the texture table; auxiliary
    /// records are 28 bytes and append three packet-ready material words. The
    /// guest expands either form into a textured quad before calling the
    /// ordinary mesh renderer, so these faces are absent from all eight indexed
    /// streams. Decode the authored record directly for complete residency.
    /// </summary>
    void CaptureRawTrackBillboards(
        uint meshPointer,
        IMemory memory,
        uint textureTableBase,
        bool auxiliaryFormat,
        long pendingFrame,
        TrackMeshProjectionPath projectionPath,
        in HleDrawEnv environment,
        ref long objectPrimitives,
        ref long objectTriangles,
        ref long objectTextured)
    {
        ushort primitiveCount = memory.ReadU16(meshPointer + 0x40u);
        if (primitiveCount == 0)
            return;

        uint primitivePointer = memory.ReadU32(meshPointer + 0x24u);
        uint primitiveSize = auxiliaryFormat ? 0x1Cu : 0x10u;
        if (!IsRawTrackGuestRange(
                primitivePointer,
                (ulong)primitiveCount * primitiveSize))
        {
            throw RawTrackFailure(
                meshPointer,
                "billboard primitive stream is outside guest RAM");
        }
        if (!auxiliaryFormat &&
            !IsRawTrackGuestRange(textureTableBase, 0x20u))
        {
            throw RawTrackFailure(
                meshPointer,
                "billboard texture table base is outside guest RAM");
        }

        // The guest constructs the billboard's horizontal model-space axis
        // from these camera-yaw coefficients. They remain in scratch RAM when
        // the indexed-mesh hook runs, after the guest restores its GTE matrix.
        short cameraAxisX = unchecked((short)memory.ReadU16(0x1F8003F2u));
        short cameraAxisY = unchecked((short)memory.ReadU16(0x1F8003F0u));

        for (int item = 0; item < primitiveCount; item++)
        {
            uint primitive = primitivePointer +
                checked((uint)item * primitiveSize);
            uint packedCenter = memory.ReadU32(primitive);
            uint packedTextureAndZ = memory.ReadU32(primitive + 4u);
            uint packedSize = memory.ReadU32(primitive + 8u);
            uint commandColor = memory.ReadU32(primitive + 0x0Cu);
            byte command = (byte)(commandColor >> 24);
            if ((command & 0xFC) != 0x2C)
            {
                throw RawTrackFailure(
                    meshPointer,
                    $"billboard primitive {item} has opcode 0x{command:X2}, " +
                    "expected textured-quad family 0x2C");
            }

            short centerX = unchecked((short)packedCenter);
            short centerY = unchecked((short)(packedCenter >> 16));
            short baseZ = unchecked((short)packedTextureAndZ);
            uint textureIndex = packedTextureAndZ >> 16;
            short width = unchecked((short)packedSize);
            ushort height = (ushort)(packedSize >> 16);
            RawTrackBillboardQuad quad = RawTrackBillboardCoordinates(
                auxiliaryFormat,
                centerX,
                centerY,
                baseZ,
                width,
                height,
                cameraAxisX,
                cameraAxisY);

            uint texture0;
            uint texture1;
            uint texture2;
            if (auxiliaryFormat)
            {
                texture0 = memory.ReadU32(primitive + 0x10u);
                texture1 = memory.ReadU32(primitive + 0x14u);
                texture2 = memory.ReadU32(primitive + 0x18u);
            }
            else
            {
                uint entry = unchecked(textureTableBase + (textureIndex << 5));
                if (!IsRawTrackGuestRange(entry, 0x10u))
                {
                    throw RawTrackFailure(
                        meshPointer,
                        $"billboard texture entry 0x{entry:X8} is outside guest RAM");
                }
                texture0 = memory.ReadU32(entry);
                texture1 = memory.ReadU32(entry + 4u);
                texture2 = memory.ReadU32(entry + 8u);
            }
            ushort uv0 = (ushort)texture0;
            ushort uv1 = (ushort)texture1;
            ushort uv2 = (ushort)texture2;
            ushort uv3 = (ushort)(texture2 >> 16);
            ushort clut = (ushort)(texture0 >> 16);
            ushort texturePage = (ushort)(texture1 >> 16);

            GteProjectionOrigin origin0 = Gte.SnapshotProjectionOrigin(
                quad.X0, quad.Y0, quad.Z0);
            GteProjectionOrigin origin1 = Gte.SnapshotProjectionOrigin(
                quad.X1, quad.Y1, quad.Z1);
            GteProjectionOrigin origin2 = Gte.SnapshotProjectionOrigin(
                quad.X2, quad.Y2, quad.Z2);
            GteProjectionOrigin origin3 = Gte.SnapshotProjectionOrigin(
                quad.X3, quad.Y3, quad.Z3);
            uint color = _rawTrackFlatDebugRequested
                ? RawTrackDebugColor(RawTrackBillboardStream)
                : commandColor;
            HleVertex a = RawTrackVertex(
                in origin0,
                color,
                uv0,
                environment.DrawOffsetX,
                environment.DrawOffsetY);
            HleVertex b = RawTrackVertex(
                in origin1,
                color,
                uv1,
                environment.DrawOffsetX,
                environment.DrawOffsetY);
            HleVertex c = RawTrackVertex(
                in origin2,
                color,
                uv2,
                environment.DrawOffsetX,
                environment.DrawOffsetY);
            HleVertex d = RawTrackVertex(
                in origin3,
                color,
                uv3,
                environment.DrawOffsetX,
                environment.DrawOffsetY);
            var flags = new PrimFlags
            {
                Textured = !_rawTrackFlatDebugRequested,
                SemiTrans = (command & 0x02) != 0,
                RawTexture = (command & 0x01) != 0,
                Gouraud = false,
                ResidentCourse = true,
                TPage = texturePage,
                Clut = clut,
                OtIndex = 0,
            };

            bool faceDiagnostics =
                _rawTrackCorrelationRequested ||
                _rawTrackTemporalFacingAuditRequested ||
                _rawTrackTemporalCoverageAuditRequested ||
                _rawTrackFaceTracePoll >= 0;
            double firstDeterminant = faceDiagnostics
                ? RawTrackViewDeterminant(
                    in origin0,
                    in origin1,
                    in origin2)
                : 0.0;
            double secondDeterminant = faceDiagnostics
                ? RawTrackViewDeterminant(
                    in origin1,
                    in origin2,
                    in origin3)
                : 0.0;
            long firstNclip = _rawTrackCorrelationRequested
                ? RawTrackGteNclip(
                    in origin0,
                    in origin1,
                    in origin2)
                : 0;
            long secondNclip = _rawTrackCorrelationRequested
                ? RawTrackGteNclip(
                    in origin1,
                    in origin2,
                    in origin3)
                : 0;
            if (_rawTrackTemporalFacingAuditRequested ||
                _rawTrackTemporalCoverageAuditRequested)
            {
                AuditRawTrackTemporalFacing(
                    in origin0,
                    in origin1,
                    in origin2,
                    in environment,
                    primitive,
                    projectionPath,
                    RawTrackBillboardStream,
                    0,
                    item,
                    true,
                    firstDeterminant,
                    0.0);
                AuditRawTrackTemporalFacing(
                    in origin1,
                    in origin2,
                    in origin3,
                    in environment,
                    primitive,
                    projectionPath,
                    RawTrackBillboardStream,
                    1,
                    item,
                    true,
                    secondDeterminant,
                    0.0);
            }
            if (_rawTrackFaceTracePoll >= 0)
            {
                TraceRawTrackFace(
                    in origin0,
                    in origin1,
                    in origin2,
                    primitive,
                    projectionPath,
                    RawTrackBillboardStream,
                    item,
                    0,
                    true,
                    firstDeterminant);
                TraceRawTrackFace(
                    in origin1,
                    in origin2,
                    in origin3,
                    primitive,
                    projectionPath,
                    RawTrackBillboardStream,
                    item,
                    1,
                    true,
                    secondDeterminant);
            }

            if (_rawTrackCorrelationRequested)
            {
                CatalogRawTrackSourceTriangle(
                    in origin0,
                    in origin1,
                    in origin2,
                    primitive,
                    projectionPath,
                    RawTrackBillboardStream,
                    item,
                    0,
                    false,
                    true,
                    true,
                    firstDeterminant,
                    firstDeterminant,
                    firstNclip,
                    firstNclip,
                    secondNclip);
                CatalogRawTrackSourceTriangle(
                    in origin1,
                    in origin2,
                    in origin3,
                    primitive,
                    projectionPath,
                    RawTrackBillboardStream,
                    item,
                    1,
                    false,
                    true,
                    true,
                    secondDeterminant,
                    secondDeterminant,
                    secondNclip,
                    firstNclip,
                    secondNclip);
            }

            // The record itself is the authored provenance for a generated
            // camera-facing quad. Its four byte offsets are collision-free
            // across the non-overlapping, fixed-stride billboard stream and
            // remain identical every frame.
            uint source0 = primitive;
            uint source1 = primitive + 1u;
            uint source2 = primitive + 2u;
            uint source3 = primitive + 3u;
            _liveWorldCapture.RecordResidentTrackTriangle(
                pendingFrame,
                in environment,
                in a,
                in b,
                in c,
                in origin0,
                in origin1,
                in origin2,
                in flags,
                source0,
                source1,
                source2);
            if (_rawTrackCorrelationRequested)
            {
                CatalogRawTrackTriangle(
                    in origin0,
                    in origin1,
                    in origin2,
                    in a,
                    in b,
                    in c,
                    in flags,
                    primitive,
                    projectionPath,
                    RawTrackBillboardStream,
                    item,
                    0);
            }
            if (_rawTrackVisibilityAuditRequested)
            {
                CountRawTrackVisibility(
                    in origin0,
                    in origin1,
                    in origin2,
                    in environment);
            }
            _liveWorldCapture.RecordResidentTrackTriangle(
                pendingFrame,
                in environment,
                in b,
                in c,
                in d,
                in origin1,
                in origin2,
                in origin3,
                in flags,
                source1,
                source2,
                source3);
            if (_rawTrackCorrelationRequested)
            {
                CatalogRawTrackTriangle(
                    in origin1,
                    in origin2,
                    in origin3,
                    in b,
                    in c,
                    in d,
                    in flags,
                    primitive,
                    projectionPath,
                    RawTrackBillboardStream,
                    item,
                    1);
            }
            if (_rawTrackVisibilityAuditRequested)
            {
                CountRawTrackVisibility(
                    in origin1,
                    in origin2,
                    in origin3,
                    in environment);
            }

            objectPrimitives++;
            objectTriangles += 2;
            objectTextured++;
            _rawTrackBillboardPrimitives++;
            _rawTrackBillboardTriangles += 2;
        }
    }

    static int RawTrackBillboardOffset(short axis, short width)
    {
        long value = ((long)axis * width) >> 12;
        return (int)Math.Clamp(value, short.MinValue, short.MaxValue);
    }

    static RawTrackBillboardQuad RawTrackBillboardCoordinates(
        bool auxiliaryFormat,
        short centerX,
        short centerY,
        short baseZ,
        short width,
        ushort height,
        short cameraAxisX,
        short cameraAxisY)
    {
        int offsetX = RawTrackBillboardOffset(cameraAxisX, width);
        int secondAxisOffset = RawTrackBillboardOffset(cameraAxisY, width);
        short firstX = unchecked((short)(centerX - offsetX));
        short secondX = unchecked((short)(centerX + offsetX));

        // The two authored formats use different coordinate layouts.
        // Primary func_8002009C stores (centerX, centerY), baseZ, and a Z
        // height. Its temporary GTE matrix rotates the horizontal width
        // through X/Y. Auxiliary func_8001F784 instead stores a Y height and
        // rotates the width through X/Z with the second matrix axis negated.
        // Treating a primary Z height as Y turns low roadside signs into
        // carriageway-wide slabs even though every projection remains finite.
        if (!auxiliaryFormat)
        {
            short firstY = unchecked((short)(
                centerY + secondAxisOffset));
            short secondY = unchecked((short)(
                centerY - secondAxisOffset));
            short topZ = unchecked((short)(baseZ + height));
            return new RawTrackBillboardQuad(
                firstX, firstY, topZ,
                secondX, secondY, topZ,
                firstX, firstY, baseZ,
                secondX, secondY, baseZ);
        }

        short firstZ = unchecked((short)(baseZ - secondAxisOffset));
        short secondZ = unchecked((short)(baseZ + secondAxisOffset));
        short topY = unchecked((short)(centerY + height));
        return new RawTrackBillboardQuad(
            firstX, topY, firstZ,
            secondX, topY, secondZ,
            firstX, centerY, firstZ,
            secondX, centerY, secondZ);
    }

    void CountRawTrackVisibility(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c,
        in HleDrawEnv environment)
    {
        // This is an on-demand coverage oracle, not renderer input. Computing
        // it continuously repeated six perspective divides for every accepted
        // course triangle after the native renderer had already received the
        // exact model point and GT2 transform.
        if (!_rawTrackVisibilityAuditRequested)
            return;
        if (a.ViewZFixed < RawTrackNearFixed ||
            b.ViewZFixed < RawTrackNearFixed ||
            c.ViewZFixed < RawTrackNearFixed)
            return;
        _rawTrackAllPositiveTriangles++;
        (double ax, double ay) = RawTrackProject(in a, in environment);
        (double bx, double by) = RawTrackProject(in b, in environment);
        (double cx, double cy) = RawTrackProject(in c, in environment);
        double minimumX = Math.Min(ax, Math.Min(bx, cx));
        double maximumX = Math.Max(ax, Math.Max(bx, cx));
        double minimumY = Math.Min(ay, Math.Min(by, cy));
        double maximumY = Math.Max(ay, Math.Max(by, cy));
        if (maximumX >= environment.ClipX0 &&
            minimumX <= environment.ClipX1 &&
            maximumY >= environment.ClipY0 &&
            minimumY <= environment.ClipY1)
        {
            _rawTrackViewportIntersectingTriangles++;
            if (_rawTrackVisibilityTraceCount < 20)
            {
                _rawTrackVisibilityTraceCount++;
                Console.Error.WriteLine(
                    $"[GT2-Raw-Track-Visible] n={_rawTrackVisibilityTraceCount} " +
                    $"object=0x{a.Object.StableId:X8} " +
                    $"transform=0x{a.TransformId:X16} " +
                    $"xy={ax:F2},{ay:F2}/" +
                    $"{bx:F2},{by:F2}/{cx:F2},{cy:F2} " +
                    $"z={a.ViewZ},{b.ViewZ},{c.ViewZ} " +
                    $"model={a.ModelX},{a.ModelY},{a.ModelZ}/" +
                    $"{b.ModelX},{b.ModelY},{b.ModelZ}/" +
                    $"{c.ModelX},{c.ModelY},{c.ModelZ}");
            }
        }
    }

    static bool RawTrackPotentiallyVisible(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c,
        in HleDrawEnv environment)
    {
        bool aFront = a.ViewZFixed >= RawTrackNearFixed;
        bool bFront = b.ViewZFixed >= RawTrackNearFixed;
        bool cFront = c.ViewZFixed >= RawTrackNearFixed;
        if (!aFront && !bFront && !cFront)
            return false;
        // Near-plane crossings are audited by the native homogeneous clip
        // diagnostics. They are not temporal facing candidates: the camera
        // legitimately crosses their plane and projected area is unbounded at
        // Z=0, which would mislabel a camera passage as object flicker.
        if (!aFront || !bFront || !cFront)
            return false;

        (double ax, double ay) = RawTrackProject(in a, in environment);
        (double bx, double by) = RawTrackProject(in b, in environment);
        (double cx, double cy) = RawTrackProject(in c, in environment);
        double width = environment.ClipX1 - environment.ClipX0 + 1.0;
        // The direct Seattle diagnostic is true 16:9 Hor+: preserve vertical
        // framing and extend the native 4:3 half-width by 4/3.
        double horizontalExtension = width / 6.0;
        double minimumTargetX = environment.ClipX0 - horizontalExtension;
        double maximumTargetX = environment.ClipX1 + horizontalExtension;
        double minimumX = Math.Min(ax, Math.Min(bx, cx));
        double maximumX = Math.Max(ax, Math.Max(bx, cx));
        double minimumY = Math.Min(ay, Math.Min(by, cy));
        double maximumY = Math.Max(ay, Math.Max(by, cy));
        return maximumX >= minimumTargetX &&
            minimumX <= maximumTargetX &&
            maximumY >= environment.ClipY0 &&
            minimumY <= environment.ClipY1;
    }

    static bool RawTrackPotentiallyVisible(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c,
        in GteProjectionOrigin d,
        in HleDrawEnv environment) =>
        RawTrackPotentiallyVisible(in a, in b, in c, in environment) ||
        RawTrackPotentiallyVisible(in b, in c, in d, in environment);

    static double RawTrackProjectedArea(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c,
        in HleDrawEnv environment)
    {
        if (a.ViewZFixed < RawTrackNearFixed ||
            b.ViewZFixed < RawTrackNearFixed ||
            c.ViewZFixed < RawTrackNearFixed)
            return double.PositiveInfinity;
        (double ax, double ay) = RawTrackProject(in a, in environment);
        (double bx, double by) = RawTrackProject(in b, in environment);
        (double cx, double cy) = RawTrackProject(in c, in environment);
        return Math.Abs(
            (bx - ax) * (cy - ay) -
            (by - ay) * (cx - ax)) * 0.5;
    }

    static double RawTrackConfiguredTargetAspect()
    {
        string resolution = ConfigManager.View.OutputResolution;
        int separator = resolution.IndexOfAny(['x', 'X']);
        if (separator > 0 &&
            int.TryParse(resolution.AsSpan(0, separator), out int width) &&
            int.TryParse(resolution.AsSpan(separator + 1), out int height) &&
            width > 0 && height > 0)
        {
            return Math.Clamp((double)width / height, 1.0, 8.0);
        }
        return 16.0 / 9.0;
    }

    static int RawTrackClipScreenEdge(
        ReadOnlySpan<RawTrackScreenPoint> input,
        Span<RawTrackScreenPoint> output,
        bool horizontal,
        double boundary,
        bool keepGreater)
    {
        if (input.Length == 0)
            return 0;
        int count = 0;
        RawTrackScreenPoint previous = input[^1];
        double previousCoordinate = horizontal ? previous.X : previous.Y;
        bool previousInside = keepGreater
            ? previousCoordinate >= boundary
            : previousCoordinate <= boundary;
        foreach (RawTrackScreenPoint current in input)
        {
            double currentCoordinate = horizontal ? current.X : current.Y;
            bool currentInside = keepGreater
                ? currentCoordinate >= boundary
                : currentCoordinate <= boundary;
            if (currentInside != previousInside)
            {
                double amount =
                    (boundary - previousCoordinate) /
                    (currentCoordinate - previousCoordinate);
                output[count++] = new RawTrackScreenPoint(
                    previous.X + (current.X - previous.X) * amount,
                    previous.Y + (current.Y - previous.Y) * amount);
            }
            if (currentInside)
                output[count++] = current;
            previous = current;
            previousCoordinate = currentCoordinate;
            previousInside = currentInside;
        }
        return count;
    }

    static RawTrackCoverage RawTrackClippedTriangleCoverage(
        double ax,
        double ay,
        double bx,
        double by,
        double cx,
        double cy,
        double minimumX,
        double minimumY,
        double maximumX,
        double maximumY)
    {
        if (!(minimumX < maximumX) || !(minimumY < maximumY))
            return new RawTrackCoverage(0.0, double.NaN, double.NaN);
        Span<RawTrackScreenPoint> first = stackalloc RawTrackScreenPoint[8];
        Span<RawTrackScreenPoint> second = stackalloc RawTrackScreenPoint[8];
        first[0] = new RawTrackScreenPoint(ax, ay);
        first[1] = new RawTrackScreenPoint(bx, by);
        first[2] = new RawTrackScreenPoint(cx, cy);
        int count = RawTrackClipScreenEdge(
            first[..3], second, true, minimumX, true);
        count = RawTrackClipScreenEdge(
            second[..count], first, true, maximumX, false);
        count = RawTrackClipScreenEdge(
            first[..count], second, false, minimumY, true);
        count = RawTrackClipScreenEdge(
            second[..count], first, false, maximumY, false);
        if (count < 3)
            return new RawTrackCoverage(0.0, double.NaN, double.NaN);
        double twiceArea = 0.0;
        double weightedX = 0.0;
        double weightedY = 0.0;
        RawTrackScreenPoint previous = first[count - 1];
        for (int index = 0; index < count; ++index)
        {
            RawTrackScreenPoint current = first[index];
            double cross =
                previous.X * current.Y - previous.Y * current.X;
            twiceArea += cross;
            weightedX += (previous.X + current.X) * cross;
            weightedY += (previous.Y + current.Y) * cross;
            previous = current;
        }
        if (Math.Abs(twiceArea) <= double.Epsilon)
            return new RawTrackCoverage(0.0, double.NaN, double.NaN);
        return new RawTrackCoverage(
            Math.Abs(twiceArea) * 0.5,
            weightedX / (3.0 * twiceArea),
            weightedY / (3.0 * twiceArea));
    }

    internal static double RawTrackClippedTriangleArea(
        double ax,
        double ay,
        double bx,
        double by,
        double cx,
        double cy,
        double minimumX,
        double minimumY,
        double maximumX,
        double maximumY) =>
        RawTrackClippedTriangleCoverage(
            ax,
            ay,
            bx,
            by,
            cx,
            cy,
            minimumX,
            minimumY,
            maximumX,
            maximumY).Area;

    static RawTrackCoverage RawTrackTargetClippedCoverage(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c,
        in HleDrawEnv environment,
        double targetAspect)
    {
        // Near crossings are owned by the native homogeneous-clip audit. The
        // temporal target-coverage detector measures only finite projected
        // triangles so a camera-plane passage cannot masquerade as an edge
        // clipping discontinuity.
        if (a.ViewZFixed < RawTrackNearFixed ||
            b.ViewZFixed < RawTrackNearFixed ||
            c.ViewZFixed < RawTrackNearFixed)
            return new RawTrackCoverage(0.0, double.NaN, double.NaN);
        (double ax, double ay) = RawTrackProject(in a, in environment);
        (double bx, double by) = RawTrackProject(in b, in environment);
        (double cx, double cy) = RawTrackProject(in c, in environment);
        double nativeWidth = environment.ClipX1 - environment.ClipX0 + 1.0;
        double nativeHeight = environment.ClipY1 - environment.ClipY0 + 1.0;
        double targetWidth = Math.Max(nativeWidth, nativeHeight * targetAspect);
        double centerX = environment.ClipX0 + nativeWidth * 0.5;
        double minimumX = centerX - targetWidth * 0.5;
        double maximumX = centerX + targetWidth * 0.5;
        RawTrackCoverage coverage = RawTrackClippedTriangleCoverage(
            ax,
            ay,
            bx,
            by,
            cx,
            cy,
            minimumX,
            environment.ClipY0,
            maximumX,
            environment.ClipY1 + 1.0);
        if (coverage.Area > 0.0)
        {
            return new RawTrackCoverage(
                coverage.Area,
                coverage.CenterX - environment.ClipX0,
                coverage.CenterY - environment.ClipY0);
        }
        // A fully off-target triangle has no clipped-polygon centroid. Clamp
        // its finite projected centroid to the target boundary so an ABA edge
        // loss can still be classified without injecting NaN into the motion
        // diagnostic.
        return new RawTrackCoverage(
            0.0,
            Math.Clamp(
                (ax + bx + cx) / 3.0,
                minimumX,
                maximumX) - environment.ClipX0,
            Math.Clamp(
                (ay + by + cy) / 3.0,
                (double)environment.ClipY0,
                environment.ClipY1 + 1.0) - environment.ClipY0);
    }

    static string? ParseRawTrackAuditStage()
    {
        string? configured = Environment.GetEnvironmentVariable(
            "RECOMPONE_AUDIT_GT2_RAW_TRACK_STAGE");
        return string.IsNullOrWhiteSpace(configured)
            ? null
            : configured.Trim().ToLowerInvariant().Replace(' ', '_')
                .Replace('-', '_');
    }

    bool RawTrackAuditWindowActive()
    {
        if (_rawTrackAuditStage is null)
            return true;
        int stagePoll = Host.InputManager.CurrentScriptStagePoll;
        return string.Equals(
                Host.InputManager.CurrentScriptStage,
                _rawTrackAuditStage,
                StringComparison.OrdinalIgnoreCase) &&
            stagePoll >= _rawTrackAuditStageStartPoll &&
            stagePoll <= _rawTrackAuditStageEndPoll;
    }

    string RawTrackAuditStageRange() =>
        _rawTrackAuditStage is null
            ? "all"
            : $"{_rawTrackAuditStage}@{_rawTrackAuditStageStartPoll}.." +
              $"{_rawTrackAuditStageEndPoll}";

    static double RawTrackProjectedArea(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c,
        in GteProjectionOrigin d,
        in HleDrawEnv environment) =>
        RawTrackProjectedArea(in a, in b, in c, in environment) +
        RawTrackProjectedArea(in b, in c, in d, in environment);

    static (double X, double Y) RawTrackProject(
        in GteProjectionOrigin origin,
        in HleDrawEnv environment) =>
        (
            RawTrackProjectAxis(
                origin.ViewXFixed,
                origin.ViewZFixed,
                origin.ProjectionOffsetX,
                origin.ProjectionPlane,
                environment.DrawOffsetX),
            RawTrackProjectAxis(
                origin.ViewYFixed,
                origin.ViewZFixed,
                origin.ProjectionOffsetY,
                origin.ProjectionPlane,
                environment.DrawOffsetY));

    internal static double RawTrackProjectAxis(
        long viewAxisFixed,
        long viewZFixed,
        int projectionOffset,
        ushort projectionPlane,
        int drawOffset)
    {
        double center = drawOffset + projectionOffset / 65536.0;
        return viewZFixed == 0
            ? center
            : center +
                projectionPlane * (double)viewAxisFixed / viewZFixed;
    }

    internal static double RawTrackFixedViewDeterminant(
        long ax,
        long ay,
        long az,
        long bx,
        long by,
        long bz,
        long cx,
        long cy,
        long cz) =>
        (double)ax * ((double)by * cz - (double)bz * cy) -
        (double)ay * ((double)bx * cz - (double)bz * cx) +
        (double)az * ((double)bx * cy - (double)by * cx);

    static double RawTrackViewDeterminant(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c) =>
        // The GTE exposes integer IR/SZ values after each transformed vertex,
        // but the object-to-view matrix itself retains twelve fractional bits.
        // Dropping those bits before an edge-on facing test lets independent
        // per-vertex rounding reverse the determinant from one 60 Hz frame to
        // the next. Preserve the authored fixed-point transform for modern
        // culling; quantized NCLIP remains available separately as the guest
        // oracle and never drives the shipping decision.
        RawTrackFixedViewDeterminant(
            a.ViewXFixed,
            a.ViewYFixed,
            a.ViewZFixed,
            b.ViewXFixed,
            b.ViewYFixed,
            b.ViewZFixed,
            c.ViewXFixed,
            c.ViewYFixed,
            c.ViewZFixed);

    static int RawTrackNearClippedDeterminants(
        long ax,
        long ay,
        long az,
        long bx,
        long by,
        long bz,
        long cx,
        long cy,
        long cz,
        Span<double> determinants)
    {
        const double NearFixed = RawTrackNearFixed;
        Span<RawTrackClipPoint> input = stackalloc RawTrackClipPoint[4];
        Span<RawTrackClipPoint> output = stackalloc RawTrackClipPoint[4];
        input[0] = new RawTrackClipPoint(ax, ay, az);
        input[1] = new RawTrackClipPoint(bx, by, bz);
        input[2] = new RawTrackClipPoint(cx, cy, cz);
        int inputCount = 3;
        int outputCount = 0;
        RawTrackClipPoint previous = input[inputCount - 1];
        bool previousInside = previous.Z >= NearFixed;
        for (int index = 0; index < inputCount; index++)
        {
            RawTrackClipPoint current = input[index];
            bool currentInside = current.Z >= NearFixed;
            if (currentInside != previousInside)
            {
                double t = (NearFixed - previous.Z) /
                    (current.Z - previous.Z);
                output[outputCount++] = new RawTrackClipPoint(
                    previous.X + (current.X - previous.X) * t,
                    previous.Y + (current.Y - previous.Y) * t,
                    NearFixed);
            }
            if (currentInside)
                output[outputCount++] = current;
            previous = current;
            previousInside = currentInside;
        }
        if (outputCount < 3)
            return 0;
        int determinantCount = 0;
        for (int index = 1;
             index + 1 < outputCount &&
                determinantCount < determinants.Length;
             index++)
        {
            RawTrackClipPoint first = output[0];
            RawTrackClipPoint second = output[index];
            RawTrackClipPoint third = output[index + 1];
            determinants[determinantCount++] =
                first.X * (second.Y * third.Z - second.Z * third.Y) -
                first.Y * (second.X * third.Z - second.Z * third.X) +
                first.Z * (second.X * third.Y - second.Y * third.X);
        }
        return determinantCount;
    }

    internal static bool RawTrackNearClippedAccepted(
        long ax,
        long ay,
        long az,
        long bx,
        long by,
        long bz,
        long cx,
        long cy,
        long cz,
        bool oneSided,
        bool acceptPositive)
    {
        Span<double> clippedDeterminants = stackalloc double[2];
        clippedDeterminants.Clear();
        int clippedCount = RawTrackNearClippedDeterminants(
            ax, ay, az,
            bx, by, bz,
            cx, cy, cz,
            clippedDeterminants);
        for (int index = 0; index < clippedCount; index++)
        {
            double determinant = clippedDeterminants[index];
            if (oneSided
                ? acceptPositive
                    ? determinant > 0.0
                    : determinant < 0.0
                : determinant != 0.0)
            {
                return true;
            }
        }
        return false;
    }

    static bool RawTrackNearClipFacingAccepted(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c,
        bool oneSided,
        bool acceptPositive,
        bool preClipAccepted)
    {
        bool hasFront =
            a.ViewZFixed >= RawTrackNearFixed ||
            b.ViewZFixed >= RawTrackNearFixed ||
            c.ViewZFixed >= RawTrackNearFixed;
        bool hasBehind =
            a.ViewZFixed < RawTrackNearFixed ||
            b.ViewZFixed < RawTrackNearFixed ||
            c.ViewZFixed < RawTrackNearFixed;
        if (!hasFront || !hasBehind)
            return preClipAccepted;
        return RawTrackNearClippedAccepted(
            a.ViewXFixed, a.ViewYFixed, a.ViewZFixed,
            b.ViewXFixed, b.ViewYFixed, b.ViewZFixed,
            c.ViewXFixed, c.ViewYFixed, c.ViewZFixed,
            oneSided,
            acceptPositive);
    }

    void AuditRawTrackNearClipDecision(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c,
        uint primitive,
        TrackMeshProjectionPath projectionPath,
        int stream,
        int item,
        int primitiveTriangle,
        bool oneSided,
        bool acceptPositive,
        bool currentAccepted,
        double currentDeterminant)
    {
        if (!_rawTrackNearClipAuditRequested)
            return;
        if (!RawTrackAuditWindowActive())
            return;
        bool hasFront =
            a.ViewZFixed >= RawTrackNearFixed ||
            b.ViewZFixed >= RawTrackNearFixed ||
            c.ViewZFixed >= RawTrackNearFixed;
        bool hasBehind =
            a.ViewZFixed < RawTrackNearFixed ||
            b.ViewZFixed < RawTrackNearFixed ||
            c.ViewZFixed < RawTrackNearFixed;
        if (!hasFront || !hasBehind)
            return;
        _rawTrackNearClipCrossings++;
        Span<double> clippedDeterminants = stackalloc double[2];
        clippedDeterminants.Clear();
        int clippedCount = RawTrackNearClippedDeterminants(
            a.ViewXFixed, a.ViewYFixed, a.ViewZFixed,
            b.ViewXFixed, b.ViewYFixed, b.ViewZFixed,
            c.ViewXFixed, c.ViewYFixed, c.ViewZFixed,
            clippedDeterminants);
        bool clippedAccepted = RawTrackNearClippedAccepted(
            a.ViewXFixed, a.ViewYFixed, a.ViewZFixed,
            b.ViewXFixed, b.ViewYFixed, b.ViewZFixed,
            c.ViewXFixed, c.ViewYFixed, c.ViewZFixed,
            oneSided,
            acceptPositive);
        if (clippedAccepted == currentAccepted)
            return;
        _rawTrackNearClipDecisionChanges++;
        if (clippedAccepted)
            _rawTrackNearClipRejectedToAccepted++;
        else
            _rawTrackNearClipAcceptedToRejected++;
        int poll = Host.InputManager.CurrentPoll;
        if (poll < _rawTrackNearClipAuditStartPoll ||
            poll > _rawTrackNearClipAuditEndPoll ||
            _rawTrackNearClipTraceCount++ >= 256)
        {
            return;
        }
        Console.Error.WriteLine(
            $"[GT2-Raw-Near-Clip] n={_rawTrackNearClipTraceCount} " +
            $"poll={poll} object=0x{a.Object.StableId:X8} " +
            $"model=0x{a.Object.ModelPointer:X8} " +
            $"primitive=0x{primitive:X8} path={projectionPath} " +
            $"stream={stream} item={item} triangle={primitiveTriangle} " +
            $"oneSided={(oneSided ? 1 : 0)} " +
            $"acceptSign={(acceptPositive ? "+" : "-")} " +
            $"decision={(currentAccepted ? 1 : 0)}->" +
            $"{(clippedAccepted ? 1 : 0)} " +
            $"det={currentDeterminant:R} " +
            $"clipped={clippedCount}/" +
            $"{clippedDeterminants[0]:R}/" +
            $"{clippedDeterminants[1]:R} " +
            $"z={a.ViewZFixed / 4096.0:R}/" +
            $"{b.ViewZFixed / 4096.0:R}/" +
            $"{c.ViewZFixed / 4096.0:R}");
    }

    internal static bool RawTrackPrimaryQuadAccepted(
        bool packedFaceBitSet,
        double firstDeterminant,
        double secondDeterminant) =>
        packedFaceBitSet
            ? firstDeterminant > 0.0 || secondDeterminant < 0.0
            : firstDeterminant != 0.0 || secondDeterminant != 0.0;

    internal static bool RawTrackAlternateQuadAccepted(
        bool packedFaceBitSet,
        double firstDeterminant,
        double secondDeterminant) =>
        packedFaceBitSet
            ? firstDeterminant < 0.0 || secondDeterminant > 0.0
            : firstDeterminant != 0.0 || secondDeterminant != 0.0;

    internal static int RawTrackQuadPacketCorner(
        TrackMeshProjectionPath projectionPath,
        int primitiveTriangle,
        int triangleVertex)
    {
        int packed = projectionPath == TrackMeshProjectionPath.Primary
            ? primitiveTriangle == 0 ? 0x310 : 0x123
            : primitiveTriangle == 0 ? 0x013 : 0x132;
        return triangleVertex switch
        {
            0 => (packed >> 8) & 0xF,
            1 => (packed >> 4) & 0xF,
            2 => packed & 0xF,
            _ => throw new ArgumentOutOfRangeException(
                nameof(triangleVertex)),
        };
    }

    internal static int RawTrackQuadPacketUvCorner(
        TrackMeshProjectionPath projectionPath,
        int primitiveTriangle,
        int triangleVertex)
    {
        int packed = projectionPath == TrackMeshProjectionPath.Primary
            ? primitiveTriangle == 0 ? 0x310 : 0x123
            : primitiveTriangle == 0 ? 0x013 : 0x132;
        return triangleVertex switch
        {
            0 => (packed >> 8) & 0xF,
            1 => (packed >> 4) & 0xF,
            2 => packed & 0xF,
            _ => throw new ArgumentOutOfRangeException(
                nameof(triangleVertex)),
        };
    }

    internal static bool RawTrackQuadTriangleAccepted(
        bool packedFaceBitSet,
        TrackMeshProjectionPath projectionPath,
        int primitiveTriangle,
        double determinant)
    {
        if (!packedFaceBitSet)
            return determinant != 0.0;
        bool firstTriangle = primitiveTriangle == 0;
        return projectionPath == TrackMeshProjectionPath.Primary
            ? firstTriangle ? determinant > 0.0 : determinant < 0.0
            : firstTriangle ? determinant < 0.0 : determinant > 0.0;
    }

    static long RawTrackGteNclip(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c) =>
        RawTrackGteNclip(in a, in b, in c, out _);

    static long RawTrackGteNclip(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c,
        out bool saturated)
    {
        (int Ax, int Ay) = RawTrackGteScreen(in a, out bool saturatedA);
        (int Bx, int By) = RawTrackGteScreen(in b, out bool saturatedB);
        (int Cx, int Cy) = RawTrackGteScreen(in c, out bool saturatedC);
        saturated = saturatedA || saturatedB || saturatedC;
        return
            (long)Ax * By +
            (long)Bx * Cy +
            (long)Cx * Ay -
            (long)Ax * Cy -
            (long)Bx * Ay -
            (long)Cx * By;
    }

    /// <summary>
    /// Reconstructs the unsigned coverage value GT2 compares with the U16 at
    /// texture-record +0x0C. GT2 deliberately swaps source corners 1 and 2
    /// before triangle NCLIP. For quads, its FIFO evaluates the authored first
    /// triangle followed by 2,0,3, then takes abs(second - first). These are
    /// material-LOD orders; they are independent of the later packet split.
    /// </summary>
    static uint RawTrackMaterialCoverage(
        GteProjectionOrigin[] origins,
        ReadOnlySpan<uint> indices,
        bool quad,
        TrackMeshProjectionPath projectionPath)
    {
        _ = projectionPath;
        if (!quad)
        {
            long triangle = RawTrackGteNclip(
                in origins[indices[0]],
                in origins[indices[2]],
                in origins[indices[1]]);
            return RawTrackMaterialCoverageFromNclips(
                unchecked((int)triangle),
                0,
                false);
        }

        GteProjectionOrigin firstA = origins[indices[0]];
        GteProjectionOrigin firstB = origins[indices[1]];
        GteProjectionOrigin firstC = origins[indices[2]];
        GteProjectionOrigin secondA = origins[indices[2]];
        GteProjectionOrigin secondB = origins[indices[0]];
        GteProjectionOrigin secondC = origins[indices[3]];
        int first = unchecked((int)RawTrackGteNclip(
            in firstA,
            in firstB,
            in firstC));
        int second = unchecked((int)RawTrackGteNclip(
            in secondA,
            in secondB,
            in secondC));
        return RawTrackMaterialCoverageFromNclips(first, second, true);
    }

    static uint RawTrackMaterialCoverageFromNclips(
        int first,
        int second,
        bool quad)
    {
        if (!quad)
            return unchecked((uint)first);
        uint combined = unchecked((uint)second - (uint)first);
        if ((int)combined < 0)
            combined = unchecked(0u - combined);
        return combined;
    }

    static (int X, int Y) RawTrackGteScreen(
        in GteProjectionOrigin origin,
        out bool saturated)
    {
        saturated = origin.ViewZ < 0 || origin.ViewZ > ushort.MaxValue;
        ushort depth = (ushort)Math.Clamp(
            origin.ViewZ,
            0,
            ushort.MaxValue);
        uint quotient = Gte.Divide(origin.ProjectionPlane, depth);
        if (depth == 0 || origin.ProjectionPlane >= 2u * depth)
            saturated = true;
        int ir1 = Math.Clamp(origin.ViewX, short.MinValue, short.MaxValue);
        int ir2 = Math.Clamp(origin.ViewY, short.MinValue, short.MaxValue);
        if (ir1 != origin.ViewX || ir2 != origin.ViewY)
            saturated = true;
        long projectedX =
            (long)quotient * ir1 + origin.ProjectionOffsetX;
        long projectedY =
            (long)quotient * ir2 + origin.ProjectionOffsetY;
        long screenX = projectedX >> 16;
        long screenY = projectedY >> 16;
        if (screenX < -0x400 || screenX > 0x3FF ||
            screenY < -0x400 || screenY > 0x3FF)
        {
            saturated = true;
        }
        return (
            (int)Math.Clamp(screenX, -0x400, 0x3FF),
            (int)Math.Clamp(screenY, -0x400, 0x3FF));
    }

    void CountRawTrackFacingPrimitive(
        TrackMeshProjectionPath projectionPath,
        int stream,
        bool accepted,
        bool degenerate,
        bool aggregateOnly,
        bool nearCrossing)
    {
        int index =
            (int)projectionPath * RawTrackCorrelationStreamCount + stream;
        if (accepted)
        {
            _rawTrackFacingAcceptedPrimitives++;
            _rawTrackFacingAcceptedByStream[index]++;
        }
        else
        {
            _rawTrackFacingRejectedPrimitives++;
            _rawTrackFacingRejectedByStream[index]++;
        }
        if (degenerate)
            _rawTrackFacingDegeneratePrimitives++;
        if (aggregateOnly)
            _rawTrackFacingAggregateOnlyPrimitives++;
        if (nearCrossing)
            _rawTrackFacingNearCrossingPrimitives++;
    }

    void CatalogRawTrackTriangle(
        in GteProjectionOrigin originA,
        in GteProjectionOrigin originB,
        in GteProjectionOrigin originC,
        in HleVertex a,
        in HleVertex b,
        in HleVertex c,
        in PrimFlags flags,
        uint primitiveAddress,
        TrackMeshProjectionPath projectionPath,
        int stream,
        int item,
        int primitiveTriangle)
    {
        if (!_rawTrackCorrelationRequested)
            return;
        RawTrackTriangleKey key = RawTrackKey(
            in originA,
            in originB,
            in originC);
        if (!_rawTrackExpectedTriangles.TryGetValue(key, out var candidates))
        {
            candidates = [];
            _rawTrackExpectedTriangles.Add(key, candidates);
        }
        candidates.Add(new RawTrackExpectedTriangle(
            a,
            b,
            c,
            originA,
            originB,
            originC,
            flags,
            primitiveAddress,
            projectionPath,
            stream,
            item,
            primitiveTriangle,
            RawTrackViewDeterminant(
                in originA,
                in originB,
                in originC),
            originA.ViewZFixed >= RawTrackNearFixed &&
                originB.ViewZFixed >= RawTrackNearFixed &&
            originC.ViewZFixed >= RawTrackNearFixed));
    }

    void CatalogRawTrackPrimitive(
        in GteProjectionOrigin origin,
        uint primitiveAddress,
        TrackMeshProjectionPath projectionPath,
        int stream,
        int item,
        bool accepted,
        bool gteSaturatedAccepted,
        bool gteProjectionSaturated,
        double firstDeterminant,
        double secondDeterminant,
        long firstNclip,
        long secondNclip)
    {
        if (!_rawTrackCorrelationRequested)
            return;
        var key = new RawTrackPrimitiveKey(
            origin.Object.StableId,
            origin.Object.ModelPointer,
            origin.TransformId,
            primitiveAddress,
            projectionPath);
        _rawTrackPrimitiveDecisions[key] = new RawTrackPrimitiveDecision(
            stream,
            item,
            accepted,
            gteSaturatedAccepted,
            gteProjectionSaturated,
            firstDeterminant,
            secondDeterminant,
            firstNclip,
            secondNclip);
    }

    void CaptureRawTrackGuestFaceDecision(
        uint primitivePointer,
        TrackMeshProjectionPath projectionPath,
        int stream,
        uint packedIndices,
        int firstNclip,
        int secondNclip,
        bool guestAccepted)
    {
        if (!_rawTrackCorrelationRequested)
            return;
        _rawTrackCorrelationGuestPrimitiveDecisions++;
        var key = new RawTrackPrimitiveKey(
            _rawTrackGuestDecisionContext.StableId,
            _rawTrackGuestDecisionContext.ModelPointer,
            _rawTrackGuestDecisionTransform,
            primitivePointer,
            projectionPath);
        if (projectionPath != _rawTrackGuestDecisionPath ||
            !_rawTrackPrimitiveDecisions.TryGetValue(
                key,
                out RawTrackPrimitiveDecision resident))
        {
            _rawTrackCorrelationGuestPrimitiveMissing++;
            return;
        }
        if (resident.GteSaturatedAccepted == guestAccepted)
            _rawTrackCorrelationGuestPrimitiveModeledMatches++;
        else if (resident.GteSaturatedAccepted)
            _rawTrackCorrelationGuestPrimitiveModeledOnly++;
        else
            _rawTrackCorrelationGuestPrimitiveExactOnly++;
        CountRawTrackNclipRelation(
            firstNclip,
            resident.FirstNclip,
            ref _rawTrackCorrelationGuestPrimitiveFirstNclipExact,
            ref _rawTrackCorrelationGuestPrimitiveFirstNclipNegated,
            ref _rawTrackCorrelationGuestPrimitiveFirstNclipOther);
        if ((stream & 1) != 0)
        {
            CountRawTrackNclipRelation(
                secondNclip,
                resident.SecondNclip,
                ref _rawTrackCorrelationGuestPrimitiveSecondNclipExact,
                ref _rawTrackCorrelationGuestPrimitiveSecondNclipNegated,
                ref _rawTrackCorrelationGuestPrimitiveSecondNclipOther);
        }
        // Truth-table bits are continuous resident / modeled PS1 / exact GT2.
        int truth =
            (resident.Accepted ? 4 : 0) |
            (resident.GteSaturatedAccepted ? 2 : 0) |
            (guestAccepted ? 1 : 0);
        _rawTrackCorrelationGuestPrimitiveTruth[truth]++;
        int category;
        if (resident.Accepted == guestAccepted)
        {
            _rawTrackCorrelationGuestPrimitiveMatches++;
            category = resident.Accepted ? 0 : 1;
        }
        else if (resident.Accepted)
        {
            _rawTrackCorrelationGuestPrimitiveResidentOnly++;
            category = 2;
        }
        else
        {
            _rawTrackCorrelationGuestPrimitiveGuestOnly++;
            category = 3;
        }
        int streamIndex =
            (int)projectionPath * RawTrackCorrelationStreamCount + stream;
        _rawTrackCorrelationGuestPrimitiveByStream[streamIndex * 4 + category]++;
        _rawTrackCorrelationGuestPrimitiveTruthByStream[
            streamIndex * 8 + truth]++;
        bool modeledQuantizedZero =
            resident.FirstNclip == 0 &&
            ((stream & 1) == 0 || resident.SecondNclip == 0);
        if (resident.Accepted != guestAccepted && modeledQuantizedZero)
            _rawTrackCorrelationGuestPrimitiveQuantizedZeroDivergences++;
        if (!resident.GteProjectionSaturated)
        {
            _rawTrackCorrelationGuestPrimitiveUnsaturatedDecisions++;
            if (resident.Accepted == guestAccepted)
                _rawTrackCorrelationGuestPrimitiveUnsaturatedMatches++;
            else if (resident.Accepted)
                _rawTrackCorrelationGuestPrimitiveUnsaturatedResidentOnly++;
            else
                _rawTrackCorrelationGuestPrimitiveUnsaturatedGuestOnly++;
            _rawTrackCorrelationGuestPrimitiveUnsaturatedByStream[
                streamIndex * 4 + category]++;
        }
        if (resident.Accepted == guestAccepted ||
            resident.GteProjectionSaturated ||
            _rawTrackCorrelationGuestPrimitiveTraceCount >= 64)
        {
            return;
        }
        _rawTrackCorrelationGuestPrimitiveTraceCount++;
        Console.Error.WriteLine(
            $"[GT2-Raw-Track-Primitive-Decision] " +
            $"n={_rawTrackCorrelationGuestPrimitiveTraceCount} " +
            $"object=0x{key.StableId:X8} " +
            $"model=0x{key.ModelPointer:X8} " +
            $"transform=0x{key.TransformId:X16} " +
            $"primitive=0x{primitivePointer:X8} " +
            $"path={(int)projectionPath} stream={stream} " +
            $"item={resident.Item} resident={resident.Accepted} " +
            $"guest={guestAccepted} packed=0x{packedIndices:X8} " +
            $"gteProjectionSaturated={resident.GteProjectionSaturated} " +
            $"guestNclip={firstNclip}/{secondNclip} " +
            $"continuous={resident.FirstDeterminant:R}/" +
            $"{resident.SecondDeterminant:R} " +
            $"modeledNclip={resident.FirstNclip}/" +
            $"{resident.SecondNclip} modeledAccept=" +
            $"{resident.GteSaturatedAccepted}");
    }

    static void CountRawTrackNclipRelation(
        int guest,
        long modeled,
        ref long exact,
        ref long negated,
        ref long other)
    {
        if (guest == modeled)
            exact++;
        else if (guest == -modeled)
            negated++;
        else
            other++;
    }

    void CatalogRawTrackSourceTriangle(
        in GteProjectionOrigin originA,
        in GteProjectionOrigin originB,
        in GteProjectionOrigin originC,
        uint primitiveAddress,
        TrackMeshProjectionPath projectionPath,
        int stream,
        int item,
        int primitiveTriangle,
        bool packedFaceBitSet,
        bool accepted,
        bool gteSaturatedAccepted,
        double triangleDeterminant,
        double primitiveFacing,
        long triangleNclip,
        long primitiveNclip0,
        long primitiveNclip1)
    {
        if (!_rawTrackCorrelationRequested)
            return;
        RawTrackTriangleKey key = RawTrackKey(
            in originA,
            in originB,
            in originC);
        if (!_rawTrackSourceTriangles.TryGetValue(key, out var decisions))
        {
            decisions = [];
            _rawTrackSourceTriangles.Add(key, decisions);
        }
        decisions.Add(new RawTrackSourceDecision(
            primitiveAddress,
            projectionPath,
            stream,
            item,
            primitiveTriangle,
            packedFaceBitSet,
            accepted,
            gteSaturatedAccepted,
            triangleDeterminant,
            primitiveFacing,
            triangleNclip,
            primitiveNclip0,
            primitiveNclip1));
    }

    void TraceRawTrackFace(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c,
        uint primitiveAddress,
        TrackMeshProjectionPath projectionPath,
        int stream,
        int item,
        int primitiveTriangle,
        bool accepted,
        double primitiveFacing)
    {
        int poll = Host.InputManager.CurrentPoll;
        int endPoll = _rawTrackFaceTraceEndPoll >= 0
            ? Math.Max(_rawTrackFaceTracePoll, _rawTrackFaceTraceEndPoll)
            : _rawTrackFaceTracePoll;
        if (_rawTrackFaceTracePoll < 0 ||
            poll < _rawTrackFaceTracePoll ||
            poll > endPoll ||
            (_rawTrackFaceTraceModel != 0 &&
                a.Object.ModelPointer != _rawTrackFaceTraceModel) ||
            (poll - _rawTrackFaceTracePoll) %
                _rawTrackFaceTraceInterval != 0 ||
            _rawTrackFaceTraceCount >= _rawTrackFaceTraceLimit)
        {
            return;
        }
        _rawTrackFaceTraceCount++;
        double determinant = RawTrackViewDeterminant(in a, in b, in c);
        Console.Error.WriteLine(
            $"[GT2-Raw-Track-Face] poll={poll} " +
            $"n={_rawTrackFaceTraceCount} " +
            $"object=0x{a.Object.StableId:X8} " +
            $"model=0x{a.Object.ModelPointer:X8} " +
            $"path={(int)projectionPath} stream={stream} item={item} " +
            $"triangle={primitiveTriangle} " +
            $"accepted={(accepted ? 1 : 0)} " +
            $"primitiveFacing={primitiveFacing:R} " +
            $"primitive=0x{primitiveAddress:X8} " +
            $"modelVertices={a.ModelX},{a.ModelY},{a.ModelZ}/" +
            $"{b.ModelX},{b.ModelY},{b.ModelZ}/" +
            $"{c.ModelX},{c.ModelY},{c.ModelZ} " +
            $"view={a.ViewX},{a.ViewY},{a.ViewZ}/" +
            $"{b.ViewX},{b.ViewY},{b.ViewZ}/" +
            $"{c.ViewX},{c.ViewY},{c.ViewZ} " +
            $"viewFixed={a.ViewXFixed},{a.ViewYFixed},{a.ViewZFixed}/" +
            $"{b.ViewXFixed},{b.ViewYFixed},{b.ViewZFixed}/" +
            $"{c.ViewXFixed},{c.ViewYFixed},{c.ViewZFixed} " +
            $"projection={a.ProjectionOffsetX}," +
            $"{a.ProjectionOffsetY},{a.ProjectionPlane} " +
            $"det={determinant:R}");
    }

    void AuditRawTrackTemporalFacing(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c,
        in HleDrawEnv environment,
        uint primitiveAddress,
        TrackMeshProjectionPath projectionPath,
        int stream,
        int primitiveTriangle,
        int item,
        bool accepted,
        double firstDeterminant,
        double secondDeterminant)
    {
        if (!_rawTrackTemporalFacingAuditRequested &&
            !_rawTrackTemporalCoverageAuditRequested)
            return;
        if (!RawTrackAuditWindowActive())
            return;
        bool allInFront =
            a.ViewZFixed >= RawTrackNearFixed &&
            b.ViewZFixed >= RawTrackNearFixed &&
            c.ViewZFixed >= RawTrackNearFixed;
        (double ax, double ay) = allInFront
            ? RawTrackProject(in a, in environment)
            : (double.NaN, double.NaN);
        (double bx, double by) = allInFront
            ? RawTrackProject(in b, in environment)
            : (double.NaN, double.NaN);
        (double cx, double cy) = allInFront
            ? RawTrackProject(in c, in environment)
            : (double.NaN, double.NaN);
        if (allInFront)
        {
            // GT2 alternates 0..239 and 240..479 VRAM draw buffers. Native
            // target coverage is buffer-local, so compare motion in the same
            // local coordinate system instead of mistaking the 240-line
            // buffer swap for a camera reversal.
            ax -= environment.ClipX0;
            bx -= environment.ClipX0;
            cx -= environment.ClipX0;
            ay -= environment.ClipY0;
            by -= environment.ClipY0;
            cy -= environment.ClipY0;
        }
        RawTrackCoverage targetCoverage = RawTrackTargetClippedCoverage(
            in a,
            in b,
            in c,
            in environment,
            _rawTrackAuditTargetAspect);
        var key = new RawTrackTemporalFaceKey(
            a.Object.StableId,
            a.Object.ModelPointer,
            primitiveAddress,
            projectionPath,
            stream,
            primitiveTriangle);
        var decision = new RawTrackTemporalFaceDecision(
            accepted,
            RawTrackPotentiallyVisible(in a, in b, in c, in environment),
            allInFront,
            RawTrackProjectedArea(in a, in b, in c, in environment),
            targetCoverage.Area,
            targetCoverage.CenterX,
            targetCoverage.CenterY,
            ax,
            ay,
            bx,
            by,
            cx,
            cy,
            Math.Min(
                a.ViewZFixed,
                Math.Min(b.ViewZFixed, c.ViewZFixed)) /
                (double)RawTrackFixedUnit,
            Math.Max(
                a.ViewZFixed,
                Math.Max(b.ViewZFixed, c.ViewZFixed)) /
                (double)RawTrackFixedUnit,
            a.TransformId,
            a.R00,
            a.R01,
            a.R02,
            a.R10,
            a.R11,
            a.R12,
            a.R20,
            a.R21,
            a.R22,
            a.TranslateX,
            a.TranslateY,
            a.TranslateZ,
            firstDeterminant,
            secondDeterminant,
            item,
            Host.InputManager.CurrentPoll);
        if (!_rawTrackTemporalCurrent.TryAdd(key, decision))
        {
            _rawTrackTemporalDuplicateKeys++;
            _rawTrackTemporalCurrent[key] = decision;
        }
    }

    static uint ParseRawTrackTraceFilter(string environmentName)
    {
        string? configured = Environment.GetEnvironmentVariable(
            environmentName);
        if (string.IsNullOrWhiteSpace(configured))
            return 0;
        ReadOnlySpan<char> value = configured.AsSpan().Trim();
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            value = value[2..];
            return uint.TryParse(
                value,
                System.Globalization.NumberStyles.AllowHexSpecifier,
                System.Globalization.CultureInfo.InvariantCulture,
                out uint hexadecimal)
                    ? hexadecimal
                    : 0;
        }
        return uint.TryParse(
            value,
            System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture,
            out uint decimalValue)
                ? decimalValue
                : 0;
    }

    void TraceRawTrackGuestTriangle(
        in HleVertex a,
        in HleVertex b,
        in HleVertex c,
        in GteProjectionOrigin originA,
        in GteProjectionOrigin originB,
        in GteProjectionOrigin originC,
        in PrimFlags flags)
    {
        if (!_rawTrackCorrelationRequested ||
            !originA.Valid || !originB.Valid || !originC.Valid ||
            originA.Object.Kind != WorldObjectKind.Track ||
            originB.Object.Kind != WorldObjectKind.Track ||
            originC.Object.Kind != WorldObjectKind.Track)
        {
            return;
        }
        _rawTrackGuestTriangles++;
        RawTrackTriangleKey key = RawTrackKey(
            in originA,
            in originB,
            in originC);
        if (!_rawTrackExpectedTriangles.TryGetValue(key, out var candidates))
        {
            if (_rawTrackSourceTriangles.TryGetValue(
                    key,
                    out var sourceDecisions))
            {
                _rawTrackCorrelationRejectedSourceMatches++;
                RawTrackSourceDecision decision = sourceDecisions[0];
                int streamIndex =
                    (int)decision.ProjectionPath *
                        RawTrackCorrelationStreamCount +
                    decision.Stream;
                _rawTrackCorrelationRejectedSourceByStream[streamIndex]++;
                if (decision.GteSaturatedAccepted)
                {
                    _rawTrackCorrelationRejectedSourceGteSaturatedAccepted++;
                    _rawTrackCorrelationRejectedSourceGteByStream[
                        streamIndex]++;
                }
                if (_rawTrackCorrelationRejectedSourceMatches <= 24)
                {
                    Console.Error.WriteLine(
                        $"[GT2-Raw-Track-Rejected-Source] " +
                        $"n={_rawTrackCorrelationRejectedSourceMatches} " +
                        $"object=0x{originA.Object.StableId:X8} " +
                        $"model=0x{originA.Object.ModelPointer:X8} " +
                        $"transform=0x{originA.TransformId:X16} " +
                        $"primitive=0x{decision.PrimitiveAddress:X8} " +
                        $"path={(int)decision.ProjectionPath} " +
                        $"stream={decision.Stream} item={decision.Item} " +
                        $"triangle={decision.PrimitiveTriangle} " +
                        $"faceBit={(decision.PackedFaceBitSet ? 1 : 0)} " +
                        $"accepted={(decision.Accepted ? 1 : 0)} " +
                        $"gteAccepted=" +
                        $"{(decision.GteSaturatedAccepted ? 1 : 0)} " +
                        $"triangleDet={decision.TriangleDeterminant:R} " +
                        $"primitiveFacing={decision.PrimitiveFacing:R} " +
                        $"nclip={decision.TriangleNclip}/" +
                        $"{decision.PrimitiveNclip0}/" +
                        $"{decision.PrimitiveNclip1} " +
                        $"vertices={originA.ModelX},{originA.ModelY}," +
                        $"{originA.ModelZ}/{originB.ModelX}," +
                        $"{originB.ModelY},{originB.ModelZ}/" +
                        $"{originC.ModelX},{originC.ModelY}," +
                        $"{originC.ModelZ}");
                }
            }
            else
            {
                _rawTrackCorrelationGeneratedGuestTriangles++;
            }
            return;
        }
        _rawTrackCorrelationGeometryMatches++;

        RawTrackExpectedTriangle expected = candidates[0];
        HleVertex expectedA = expected.A;
        HleVertex expectedB = expected.B;
        HleVertex expectedC = expected.C;
        int permutation = -1;
        foreach (RawTrackExpectedTriangle candidate in candidates)
        {
            if (!TryAlignRawTrackExpected(
                    in candidate,
                    in originA,
                    in originB,
                    in originC,
                    out HleVertex candidateA,
                    out HleVertex candidateB,
                    out HleVertex candidateC,
                    out int candidatePermutation))
            {
                continue;
            }
            if (permutation < 0)
            {
                expected = candidate;
                expectedA = candidateA;
                expectedB = candidateB;
                expectedC = candidateC;
                permutation = candidatePermutation;
            }
            if (RawTrackUvEqual(candidateA, a) &&
                RawTrackUvEqual(candidateB, b) &&
                RawTrackUvEqual(candidateC, c) &&
                RawTrackMaterialEqual(candidate.Flags, flags) &&
                RawTrackColorEqual(candidateA, a) &&
                RawTrackColorEqual(candidateB, b) &&
                RawTrackColorEqual(candidateC, c))
            {
                expected = candidate;
                expectedA = candidateA;
                expectedB = candidateB;
                expectedC = candidateC;
                permutation = candidatePermutation;
                break;
            }
        }
        if (permutation < 0)
            return;
        int windingIndex =
            (int)expected.ProjectionPath * RawTrackCorrelationStreamCount +
            expected.Stream;
        double guestDeterminant = RawTrackViewDeterminant(
            in originA,
            in originB,
            in originC);
        if (guestDeterminant > 0.0)
        {
            _rawTrackGuestGeometryPositiveByStream[windingIndex]++;
            if (originA.ViewZFixed >= RawTrackNearFixed &&
                originB.ViewZFixed >= RawTrackNearFixed &&
                originC.ViewZFixed >= RawTrackNearFixed)
            {
                _rawTrackGuestFrontPositiveByStream[windingIndex]++;
            }
        }
        else if (guestDeterminant < 0.0)
        {
            _rawTrackGuestGeometryNegativeByStream[windingIndex]++;
            if (originA.ViewZFixed >= RawTrackNearFixed &&
                originB.ViewZFixed >= RawTrackNearFixed &&
                originC.ViewZFixed >= RawTrackNearFixed)
            {
                _rawTrackGuestFrontNegativeByStream[windingIndex]++;
            }
        }
        _rawTrackGuestPermutationByStream[windingIndex * 6 + permutation]++;
        int primitiveTriangle = Math.Clamp(
            expected.PrimitiveTriangle,
            0,
            1);
        int primitiveWindingIndex = windingIndex * 2 + primitiveTriangle;
        if (guestDeterminant > 0.0)
            _rawTrackGuestPrimitivePositiveByStream[primitiveWindingIndex]++;
        else if (guestDeterminant < 0.0)
            _rawTrackGuestPrimitiveNegativeByStream[primitiveWindingIndex]++;
        bool uvEqual =
            RawTrackUvEqual(expectedA, a) &&
            RawTrackUvEqual(expectedB, b) &&
            RawTrackUvEqual(expectedC, c);
        bool materialEqual = RawTrackMaterialEqual(
            expected.Flags,
            flags);
        bool colorEqual =
            RawTrackColorEqual(expectedA, a) &&
            RawTrackColorEqual(expectedB, b) &&
            RawTrackColorEqual(expectedC, c);
        TraceTargetedRawTrackCorrelation(
            in expected,
            in expectedA,
            in expectedB,
            in expectedC,
            in a,
            in b,
            in c,
            in originA,
            in flags,
            permutation,
            uvEqual,
            materialEqual,
            colorEqual);
        if (uvEqual && materialEqual && colorEqual)
        {
            _rawTrackCorrelationExactMatches++;
            return;
        }
        if (!uvEqual)
            _rawTrackCorrelationUvMismatches++;
        if (!materialEqual)
            _rawTrackCorrelationMaterialMismatches++;
        if (!colorEqual)
            _rawTrackCorrelationColorMismatches++;

        long mismatchCount =
            _rawTrackCorrelationUvMismatches +
            _rawTrackCorrelationMaterialMismatches +
            _rawTrackCorrelationColorMismatches;
        if (mismatchCount <= 24)
        {
            Console.Error.WriteLine(
                $"[GT2-Raw-Track-Correlation] primitive=0x{expected.PrimitiveAddress:X8} " +
                $"stream={expected.Stream} item={expected.Item} " +
                $"permutation={permutation} " +
                $"uv={uvEqual} material={materialEqual} color={colorEqual} " +
                $"expectedUv={RawTrackUv(expectedA)}/" +
                $"{RawTrackUv(expectedB)}/{RawTrackUv(expectedC)} " +
                $"guestUv={RawTrackUv(a)}/{RawTrackUv(b)}/" +
                $"{RawTrackUv(c)} " +
                $"expectedRgb={RawTrackRgb(expected.A)}/" +
                $"{RawTrackRgb(expected.B)}/{RawTrackRgb(expected.C)} " +
                $"guestRgb={RawTrackRgb(a)}/{RawTrackRgb(b)}/" +
                $"{RawTrackRgb(c)} " +
                $"expectedPage=0x{expected.Flags.TPage:X4}/" +
                $"0x{expected.Flags.Clut:X4} " +
                $"guestPage=0x{flags.TPage:X4}/0x{flags.Clut:X4}");
        }
    }

    void TraceTargetedRawTrackCorrelation(
        in RawTrackExpectedTriangle expected,
        in HleVertex expectedA,
        in HleVertex expectedB,
        in HleVertex expectedC,
        in HleVertex guestA,
        in HleVertex guestB,
        in HleVertex guestC,
        in GteProjectionOrigin origin,
        in PrimFlags guestFlags,
        int permutation,
        bool uvEqual,
        bool materialEqual,
        bool colorEqual)
    {
        int poll = Host.InputManager.CurrentPoll;
        int endPoll = _rawTrackCorrelationTraceEndPoll >= 0
            ? Math.Max(
                _rawTrackCorrelationTraceStartPoll,
                _rawTrackCorrelationTraceEndPoll)
            : _rawTrackCorrelationTraceStartPoll;
        if (_rawTrackCorrelationTraceModel == 0 ||
            origin.Object.ModelPointer != _rawTrackCorrelationTraceModel ||
            _rawTrackCorrelationTraceStartPoll < 0 ||
            poll < _rawTrackCorrelationTraceStartPoll ||
            poll > endPoll ||
            _rawTrackCorrelationTargetTraceCount++ >= 512)
        {
            return;
        }
        Console.Error.WriteLine(
            $"[GT2-Raw-Track-Correlation-Target] " +
            $"poll={poll} n={_rawTrackCorrelationTargetTraceCount} " +
            $"object=0x{origin.Object.StableId:X8} " +
            $"model=0x{origin.Object.ModelPointer:X8} " +
            $"primitive=0x{expected.PrimitiveAddress:X8} " +
            $"path={(int)expected.ProjectionPath} stream={expected.Stream} " +
            $"item={expected.Item} triangle={expected.PrimitiveTriangle} " +
            $"permutation={permutation} " +
            $"equal={uvEqual}/{materialEqual}/{colorEqual} " +
            $"residentUv={RawTrackUv(expectedA)}/" +
            $"{RawTrackUv(expectedB)}/{RawTrackUv(expectedC)} " +
            $"guestUv={RawTrackUv(guestA)}/" +
            $"{RawTrackUv(guestB)}/{RawTrackUv(guestC)} " +
            $"residentRgb={RawTrackRgb(expectedA)}/" +
            $"{RawTrackRgb(expectedB)}/{RawTrackRgb(expectedC)} " +
            $"guestRgb={RawTrackRgb(guestA)}/" +
            $"{RawTrackRgb(guestB)}/{RawTrackRgb(guestC)} " +
            $"residentXY={expectedA.X:F3},{expectedA.Y:F3}/" +
            $"{expectedB.X:F3},{expectedB.Y:F3}/" +
            $"{expectedC.X:F3},{expectedC.Y:F3} " +
            $"guestXY={guestA.X:F3},{guestA.Y:F3}/" +
            $"{guestB.X:F3},{guestB.Y:F3}/" +
            $"{guestC.X:F3},{guestC.Y:F3} " +
            $"residentZ={expectedA.Z:F3}/{expectedB.Z:F3}/" +
            $"{expectedC.Z:F3} guestZ={guestA.Z:F3}/" +
            $"{guestB.Z:F3}/{guestC.Z:F3} " +
            $"residentOt={expected.Flags.OtIndex} guestOt={guestFlags.OtIndex} " +
            $"residentPage=0x{expected.Flags.TPage:X4}/" +
            $"0x{expected.Flags.Clut:X4} " +
            $"guestPage=0x{guestFlags.TPage:X4}/0x{guestFlags.Clut:X4}");
    }

    static RawTrackTriangleKey RawTrackKey(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c)
    {
        GteProjectionOrigin first = a;
        GteProjectionOrigin second = b;
        GteProjectionOrigin third = c;
        if (CompareRawTrackModelVertex(in first, in second) > 0)
            (first, second) = (second, first);
        if (CompareRawTrackModelVertex(in second, in third) > 0)
            (second, third) = (third, second);
        if (CompareRawTrackModelVertex(in first, in second) > 0)
            (first, second) = (second, first);
        return new RawTrackTriangleKey(
            a.Object.StableId,
            a.Object.ModelPointer,
            a.TransformId,
            first.ModelX, first.ModelY, first.ModelZ,
            second.ModelX, second.ModelY, second.ModelZ,
            third.ModelX, third.ModelY, third.ModelZ);
    }

    static int CompareRawTrackModelVertex(
        in GteProjectionOrigin left,
        in GteProjectionOrigin right)
    {
        int comparison = left.ModelX.CompareTo(right.ModelX);
        if (comparison != 0)
            return comparison;
        comparison = left.ModelY.CompareTo(right.ModelY);
        return comparison != 0
            ? comparison
            : left.ModelZ.CompareTo(right.ModelZ);
    }

    static bool RawTrackModelVertexEqual(
        in GteProjectionOrigin left,
        in GteProjectionOrigin right) =>
        left.ModelX == right.ModelX &&
        left.ModelY == right.ModelY &&
        left.ModelZ == right.ModelZ;

    static bool TryAlignRawTrackExpected(
        in RawTrackExpectedTriangle expected,
        in GteProjectionOrigin guestA,
        in GteProjectionOrigin guestB,
        in GteProjectionOrigin guestC,
        out HleVertex expectedA,
        out HleVertex expectedB,
        out HleVertex expectedC,
        out int permutation)
    {
        expectedA = default;
        expectedB = default;
        expectedC = default;
        permutation = -1;
        GteProjectionOrigin originA = expected.OriginA;
        GteProjectionOrigin originB = expected.OriginB;
        GteProjectionOrigin originC = expected.OriginC;
        if (RawTrackModelVertexEqual(in guestA, in originA))
        {
            expectedA = expected.A;
            if (RawTrackModelVertexEqual(in guestB, in originB) &&
                RawTrackModelVertexEqual(in guestC, in originC))
            {
                expectedB = expected.B;
                expectedC = expected.C;
                permutation = 0;
                return true;
            }
            if (RawTrackModelVertexEqual(in guestB, in originC) &&
                RawTrackModelVertexEqual(in guestC, in originB))
            {
                expectedB = expected.C;
                expectedC = expected.B;
                permutation = 1;
                return true;
            }
        }
        if (RawTrackModelVertexEqual(in guestA, in originB))
        {
            expectedA = expected.B;
            if (RawTrackModelVertexEqual(in guestB, in originA) &&
                RawTrackModelVertexEqual(in guestC, in originC))
            {
                expectedB = expected.A;
                expectedC = expected.C;
                permutation = 2;
                return true;
            }
            if (RawTrackModelVertexEqual(in guestB, in originC) &&
                RawTrackModelVertexEqual(in guestC, in originA))
            {
                expectedB = expected.C;
                expectedC = expected.A;
                permutation = 3;
                return true;
            }
        }
        if (RawTrackModelVertexEqual(in guestA, in originC))
        {
            expectedA = expected.C;
            if (RawTrackModelVertexEqual(in guestB, in originA) &&
                RawTrackModelVertexEqual(in guestC, in originB))
            {
                expectedB = expected.A;
                expectedC = expected.B;
                permutation = 4;
                return true;
            }
            if (RawTrackModelVertexEqual(in guestB, in originB) &&
                RawTrackModelVertexEqual(in guestC, in originA))
            {
                expectedB = expected.B;
                expectedC = expected.A;
                permutation = 5;
                return true;
            }
        }
        return false;
    }

    static bool RawTrackUvEqual(HleVertex left, HleVertex right) =>
        left.U == right.U && left.V == right.V;

    static bool RawTrackColorEqual(HleVertex left, HleVertex right) =>
        left.R == right.R && left.G == right.G && left.B == right.B;

    static bool RawTrackMaterialEqual(
        PrimFlags left,
        PrimFlags right)
    {
        if (left.Textured != right.Textured ||
            left.SemiTrans != right.SemiTrans ||
            left.RawTexture != right.RawTexture ||
            left.Gouraud != right.Gouraud)
        {
            return false;
        }
        return !left.Textured ||
            left.TPage == right.TPage && left.Clut == right.Clut;
    }

    static string RawTrackUv(HleVertex vertex) =>
        $"{vertex.U},{vertex.V}";

    static string RawTrackRgb(HleVertex vertex) =>
        $"{vertex.R},{vertex.G},{vertex.B}";

    string RawTrackGuestGeometryWinding() => string.Join(
        ',',
        Enumerable.Range(
            0,
            RawTrackProjectionPathCount * RawTrackCorrelationStreamCount)
        .Select(index =>
            $"{index / RawTrackCorrelationStreamCount}:" +
            $"{index % RawTrackCorrelationStreamCount}:" +
            $"{_rawTrackGuestGeometryPositiveByStream[index]}/" +
            $"{_rawTrackGuestGeometryNegativeByStream[index]}"));

    string RawTrackFacingByStream() => string.Join(
        ',',
        Enumerable.Range(0, _rawTrackFacingAcceptedByStream.Length)
        .Where(index =>
            _rawTrackFacingAcceptedByStream[index] != 0 ||
            _rawTrackFacingRejectedByStream[index] != 0)
        .Select(index =>
            $"{index / RawTrackCorrelationStreamCount}:" +
            $"{index % RawTrackCorrelationStreamCount}:" +
            $"{_rawTrackFacingAcceptedByStream[index]}/" +
            $"{_rawTrackFacingRejectedByStream[index]}"));

    string RawTrackRejectedSourceByStream() => string.Join(
        ',',
        Enumerable.Range(
            0,
            _rawTrackCorrelationRejectedSourceByStream.Length)
        .Where(index =>
            _rawTrackCorrelationRejectedSourceByStream[index] != 0)
        .Select(index =>
            $"{index / RawTrackCorrelationStreamCount}:" +
            $"{index % RawTrackCorrelationStreamCount}:" +
            $"{_rawTrackCorrelationRejectedSourceByStream[index]}"));

    string RawTrackRejectedSourceGteByStream() => string.Join(
        ',',
        Enumerable.Range(
            0,
            _rawTrackCorrelationRejectedSourceGteByStream.Length)
        .Where(index =>
            _rawTrackCorrelationRejectedSourceGteByStream[index] != 0)
        .Select(index =>
            $"{index / RawTrackCorrelationStreamCount}:" +
            $"{index % RawTrackCorrelationStreamCount}:" +
            $"{_rawTrackCorrelationRejectedSourceGteByStream[index]}"));

    string RawTrackGuestPrimitiveByStream() => string.Join(
        ',',
        Enumerable.Range(
            0,
            RawTrackProjectionPathCount * RawTrackCorrelationStreamCount)
        .Where(index =>
            _rawTrackCorrelationGuestPrimitiveByStream[index * 4] != 0 ||
            _rawTrackCorrelationGuestPrimitiveByStream[index * 4 + 1] != 0 ||
            _rawTrackCorrelationGuestPrimitiveByStream[index * 4 + 2] != 0 ||
            _rawTrackCorrelationGuestPrimitiveByStream[index * 4 + 3] != 0)
        .Select(index =>
            $"{index / RawTrackCorrelationStreamCount}:" +
            $"{index % RawTrackCorrelationStreamCount}:" +
            $"{_rawTrackCorrelationGuestPrimitiveByStream[index * 4]}/" +
            $"{_rawTrackCorrelationGuestPrimitiveByStream[index * 4 + 1]}/" +
            $"{_rawTrackCorrelationGuestPrimitiveByStream[index * 4 + 2]}/" +
            $"{_rawTrackCorrelationGuestPrimitiveByStream[index * 4 + 3]}"));

    string RawTrackGuestPrimitiveUnsaturatedByStream() => string.Join(
        ',',
        Enumerable.Range(
            0,
            RawTrackProjectionPathCount * RawTrackCorrelationStreamCount)
        .Where(index =>
            _rawTrackCorrelationGuestPrimitiveUnsaturatedByStream[
                index * 4] != 0 ||
            _rawTrackCorrelationGuestPrimitiveUnsaturatedByStream[
                index * 4 + 1] != 0 ||
            _rawTrackCorrelationGuestPrimitiveUnsaturatedByStream[
                index * 4 + 2] != 0 ||
            _rawTrackCorrelationGuestPrimitiveUnsaturatedByStream[
                index * 4 + 3] != 0)
        .Select(index =>
            $"{index / RawTrackCorrelationStreamCount}:" +
            $"{index % RawTrackCorrelationStreamCount}:" +
            $"{_rawTrackCorrelationGuestPrimitiveUnsaturatedByStream[index * 4]}/" +
            $"{_rawTrackCorrelationGuestPrimitiveUnsaturatedByStream[index * 4 + 1]}/" +
            $"{_rawTrackCorrelationGuestPrimitiveUnsaturatedByStream[index * 4 + 2]}/" +
            $"{_rawTrackCorrelationGuestPrimitiveUnsaturatedByStream[index * 4 + 3]}"));

    string RawTrackGuestPrimitiveTruth() => string.Join(
        ',',
        Enumerable.Range(0, _rawTrackCorrelationGuestPrimitiveTruth.Length)
        .Where(index => _rawTrackCorrelationGuestPrimitiveTruth[index] != 0)
        .Select(index =>
            $"{((index & 4) != 0 ? 1 : 0)}" +
            $"{((index & 2) != 0 ? 1 : 0)}" +
            $"{((index & 1) != 0 ? 1 : 0)}:" +
            $"{_rawTrackCorrelationGuestPrimitiveTruth[index]}"));

    string RawTrackGuestPrimitiveTruthByStream() => string.Join(
        ',',
        Enumerable.Range(
            0,
            RawTrackProjectionPathCount * RawTrackCorrelationStreamCount)
        .Where(streamIndex => Enumerable.Range(0, 8).Any(truth =>
            _rawTrackCorrelationGuestPrimitiveTruthByStream[
                streamIndex * 8 + truth] != 0))
        .Select(streamIndex =>
            $"{streamIndex / RawTrackCorrelationStreamCount}:" +
            $"{streamIndex % RawTrackCorrelationStreamCount}:" +
            string.Join(
                "/",
                Enumerable.Range(0, 8).Select(truth =>
                    _rawTrackCorrelationGuestPrimitiveTruthByStream[
                        streamIndex * 8 + truth]))));

    string RawTrackFaceTraceRange()
    {
        if (_rawTrackFaceTracePoll < 0)
            return "disabled";
        int endPoll = _rawTrackFaceTraceEndPoll >= 0
            ? Math.Max(_rawTrackFaceTracePoll, _rawTrackFaceTraceEndPoll)
            : _rawTrackFaceTracePoll;
        return endPoll == _rawTrackFaceTracePoll
            ? _rawTrackFaceTracePoll.ToString()
            : $"{_rawTrackFaceTracePoll}-{endPoll}";
    }

    string RawTrackGuestFrontWinding() => string.Join(
        ',',
        Enumerable.Range(
            0,
            RawTrackProjectionPathCount * RawTrackCorrelationStreamCount)
        .Select(index =>
            $"{index / RawTrackCorrelationStreamCount}:" +
            $"{index % RawTrackCorrelationStreamCount}:" +
            $"{_rawTrackGuestFrontPositiveByStream[index]}/" +
            $"{_rawTrackGuestFrontNegativeByStream[index]}"));

    string RawTrackGuestPermutations() => string.Join(
        ',',
        Enumerable.Range(0, _rawTrackGuestPermutationByStream.Length)
        .Where(index => _rawTrackGuestPermutationByStream[index] != 0)
        .Select(index =>
            $"{index / (RawTrackCorrelationStreamCount * 6)}:" +
            $"{index / 6 % RawTrackCorrelationStreamCount}:" +
            $"{index % 6}:" +
            $"{_rawTrackGuestPermutationByStream[index]}"));

    string RawTrackGuestPrimitiveWinding() => string.Join(
        ',',
        Enumerable.Range(0, _rawTrackGuestPrimitivePositiveByStream.Length)
        .Where(index =>
            _rawTrackGuestPrimitivePositiveByStream[index] != 0 ||
            _rawTrackGuestPrimitiveNegativeByStream[index] != 0)
        .Select(index =>
            $"{index / (RawTrackCorrelationStreamCount * 2)}:" +
            $"{index / 2 % RawTrackCorrelationStreamCount}:" +
            $"{index % 2}:" +
            $"{_rawTrackGuestPrimitivePositiveByStream[index]}/" +
            $"{_rawTrackGuestPrimitiveNegativeByStream[index]}"));

    static uint RawTrackDebugColor(int stream) => stream switch
    {
        0 => 0x00FFFFFFu,
        1 => 0x0000FFFFu,
        2 => 0x00FF00FFu,
        3 => 0x00FFFF00u,
        4 => 0x000000FFu,
        5 => 0x0000FF00u,
        6 => 0x00FF0000u,
        _ => 0x008080FFu,
    };

    HleVertex RawTrackVertex(
        in GteProjectionOrigin origin,
        uint color,
        ushort packedUv,
        int drawOffsetX,
        int drawOffsetY)
    {
        float viewZ = (float)(origin.ViewZFixed / (double)RawTrackFixedUnit);
        float screenX = 0.0F;
        float screenY = 0.0F;
        if (_rawTrackCorrelationRequested)
        {
            // Guest-packet correlation reports the resident and guest screen
            // coordinates side by side. The shipping renderer does not use
            // these values: native reconstructs both the PS1 oracle and the
            // continuous modern projection from the exact origin below.
            screenX = (float)RawTrackProjectAxis(
                origin.ViewXFixed,
                origin.ViewZFixed,
                origin.ProjectionOffsetX,
                origin.ProjectionPlane,
                drawOffsetX);
            screenY = (float)RawTrackProjectAxis(
                origin.ViewYFixed,
                origin.ViewZFixed,
                origin.ProjectionOffsetY,
                origin.ProjectionPlane,
                drawOffsetY);
        }
        return new HleVertex
        {
            X = screenX,
            Y = screenY,
            Z = viewZ,
            R = (byte)color,
            G = (byte)(color >> 8),
            B = (byte)(color >> 16),
            U = (byte)packedUv,
            V = (byte)(packedUv >> 8),
            HasGteZ = true,
        };
    }

    uint[] RawTrackSourceIdentities(
        uint meshPointer,
        uint vertexPointer,
        uint vertexCount)
    {
        WorldObjectContext context = WorldCaptureContext.Current;
        var meshKey = new RawTrackSourceMeshKey(
            context.ModelPointer,
            meshPointer,
            vertexPointer,
            vertexCount);
        if (_rawTrackSourceMeshes.TryGetValue(meshKey, out uint[]? identities))
            return identities;

        identities = new uint[checked((int)vertexCount)];
        for (int index = 0; index < identities.Length; index++)
            identities[index] = RawTrackIndexedSourceIdentity(
                vertexPointer + checked((uint)index * 8u));
        _rawTrackSourceMeshes.Add(meshKey, identities);
        return identities;
    }

    static uint RawTrackIndexedSourceIdentity(uint vertexAddress)
    {
        // GT2's indexed source tables live in the validated 8 MiB guest RAM
        // window and each eight-byte record is the actual authored vertex.
        // Use that address directly: unlike a coordinate hash or encounter
        // order it is injective, deterministic across runs, and distinguishes
        // intentionally duplicated boundary vertices.
        if (!IsRawTrackGuestRange(vertexAddress, 8u))
            throw new InvalidDataException(
                $"Resident track source vertex 0x{vertexAddress:X8} is " +
                "outside guest RAM.");
        return vertexAddress;
    }

    static bool IsRawTrackGuestRange(uint pointer, ulong length)
    {
        if (pointer < 0x80000000u || pointer > 0x807FFFFFu)
            return false;
        ulong offset = pointer - 0x80000000u;
        return length <= 0x00800000u && offset + length <= 0x00800000u;
    }

    static InvalidDataException RawTrackFailure(
        uint meshPointer,
        string reason) =>
        new($"GT2 raw track decode failed for 0x{meshPointer:X8}: {reason}. " +
            "Guest track fallback is disabled.");
}
