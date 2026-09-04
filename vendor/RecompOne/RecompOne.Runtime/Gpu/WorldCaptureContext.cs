using RecompOne.Runtime.Memory;

namespace RecompOne.Runtime;

public enum WorldObjectKind : uint
{
    Unknown = 0,
    Track = 1,
    Vehicle = 2,
    Background = 3,
}

public enum WorldScenePass : byte
{
    Main = 0,
    Auxiliary = 1,
}

public enum TrackMeshProjectionPath : byte
{
    Primary = 0,
    Alternate = 1,
}

public readonly record struct WorldObjectContext(
    WorldObjectKind Kind,
    uint StableId,
    uint ModelPointer,
    int DepthScaleExponent = 0,
    bool DepthScaleValid = false,
    WorldScenePass ScenePass = WorldScenePass.Main,
    uint SceneGeneration = 0);

/// <summary>
/// GT2-specific generated-code hooks identify the object whose model is about
/// to use the GTE. Projection provenance snapshots this value, so later GPU
/// packets retain authored object identity without guessing from screen space.
/// </summary>
public static class WorldCaptureContext
{
    internal delegate void TrackMeshConsumer(
        uint meshPointer,
        IMemory memory,
        TrackMeshProjectionPath projectionPath);

    internal delegate void BackgroundMeshConsumer(
        uint modelPointer,
        IMemory memory);

    internal delegate void TrackFaceDecisionConsumer(
        uint primitivePointer,
        TrackMeshProjectionPath projectionPath,
        int stream,
        uint packedIndices,
        int firstNclip,
        int secondNclip,
        bool guestAccepted);

    readonly record struct TrackMeshTraceKey(
        uint StableId,
        uint ModelPointer,
        uint MeshPointer,
        TrackMeshProjectionPath ProjectionPath);

    const int MaxTracedTrackMeshes = 512;
    static readonly int[] TrackPrimitiveRecordSizes =
        [12, 12, 20, 24, 12, 12, 20, 24];
    static readonly int[] AuxiliaryTrackPrimitiveRecordSizes =
        [12, 12, 20, 24, 24, 24, 32, 36];

    static readonly bool FileCaptureEnabled =
        !string.IsNullOrWhiteSpace(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_WORLD_CAPTURE_PATH"));
    static readonly string TrackMeshObjPath =
        Environment.GetEnvironmentVariable("RECOMPONE_GT2_TRACK_MESH_OBJ_PATH") ??
        string.Empty;
    static readonly bool TrackMeshTraceEnabled =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_TRACK_MESH") == "1" ||
        !string.IsNullOrWhiteSpace(TrackMeshObjPath);
    static readonly Dictionary<uint, WorldObjectContext> TrackObjects = [];
    static readonly Dictionary<uint, uint> VehicleIds = [];
    static readonly HashSet<TrackMeshTraceKey> TracedTrackMeshes = [];
    static WorldObjectContext _current;
    static WorldScenePass _scenePass;
    static uint _sceneGeneration;
    static bool _scenePassActive;
#if !OPENGT_RELEASE_PACKAGE
    static readonly int TraceScenePassPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_SCENE_PASS_POLL"),
            out int traceScenePassPoll)
            ? Math.Max(0, traceScenePassPoll)
            : -1;
#endif
    static bool _currentAuxiliaryTrack;
    static readonly bool AuxiliaryTrackTraceEnabled =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_GT2_AUXILIARY_TRACK") == "1";
    static readonly int AuxiliaryTrackTraceLimit =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_AUXILIARY_TRACK_LIMIT"),
            out int auxiliaryTrackTraceLimit)
            ? Math.Max(1, auxiliaryTrackTraceLimit)
            : 256;
    static int _auxiliaryTrackTraceCount;
    static int _auxiliaryTrackExitTraceRegistered;
    static int _auxiliaryTrackSummaryReported;
    static long _auxiliaryTrackVisibilityMasksPreserved;
    static long _auxiliaryTrackModelSelectionsForced;
    static long _auxiliaryTrackClipRejectionsBypassed;
    static long _auxiliaryTrackObjectsBegun;
    static bool _trackMeshExitTraceRegistered;
    static bool _trackMeshTraceReported;
    static long _tracedTrackVertices;
    static long _tracedTrackPrimitives;
    static long _tracedTrackTriangles;
    static long _tracedTrackInvalidIndices;
    static long _tracedTrackInvalidPointers;
    static long _tracedTrackNoncontiguousStreams;
    static long _tracedTrackMaterialRecords;
    static long _tracedTrackMaterialCommandMismatches;
    static long _tracedTrackTexturedPrimitives;
    static long _tracedTrackTextureTableInvalid;
    static long _tracedTrackTextureEntryInvalid;
    static long _tracedTrackTextureAlternateEntryInvalid;
    static long _tracedTrackTextureClutHighBit;
    static long _tracedTrackEffectMeshes;
    static long _tracedTrackEffectRecords;
    static long _tracedTrackEffectPointerInvalid;
    static long _tracedTrackEffectNonzeroReservedWords;
    static readonly long[] TrackPrimitiveStreamTotals = new long[8];
    static readonly long[] TrackPrimitiveCommandTotals = new long[256];
    static readonly HashSet<uint> TrackTextureTableBases = [];
    static readonly HashSet<uint> TrackTextureEntries = [];
    static readonly HashSet<uint> TrackEffectColors = [];
    static readonly System.Text.StringBuilder TrackMeshObj = new(2_000_000);
    static readonly System.Text.StringBuilder TrackMeshGroundObj = new(1_000_000);
    static int _trackMeshObjVertices;
    static long _trackMeshObjGroundTriangles;
    static TrackMeshConsumer? _trackMeshConsumer;
    static BackgroundMeshConsumer? _backgroundMeshConsumer;
    static TrackFaceDecisionConsumer? _trackFaceDecisionConsumer;
    static long _guestTrackProjectionRuns;
    static long _guestTrackProjectionBypasses;
    static long _guestTrackBillboardProjectionRuns;
    static long _guestTrackBillboardProjectionBypasses;
    static int _trackProjectionOwnershipReported;
    static long _guestBackgroundProjectionRuns;
    static long _guestBackgroundProjectionBypasses;
    static int _backgroundProjectionOwnershipReported;

    public static bool LiveRenderingEnabled { get; set; }
    public static bool CaptureEnabled
    {
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => FileCaptureEnabled || LiveRenderingEnabled;
    }
    public static WorldObjectContext Current =>
        CaptureEnabled ? _current : default;

    /// <summary>
    /// Identifies which complete GT2 scene submission owns subsequently
    /// projected objects. This is authored pass identity, not an inference
    /// from focal length or screen coordinates.
    /// </summary>
    public static void BeginScenePass(WorldScenePass scenePass)
    {
        _scenePassActive = true;
        _scenePass = scenePass;
        if (scenePass == WorldScenePass.Main)
            _sceneGeneration = unchecked(_sceneGeneration + 1u);
        if (CaptureEnabled)
            _current = default;
#if !OPENGT_RELEASE_PACKAGE
        TraceScenePass("begin");
#endif
    }

    public static void EndScenePass()
    {
        _scenePassActive = false;
        _scenePass = WorldScenePass.Main;
        if (CaptureEnabled)
            _current = default;
#if !OPENGT_RELEASE_PACKAGE
        TraceScenePass("end");
#endif
    }

#if !OPENGT_RELEASE_PACKAGE
    static void TraceScenePass(string phase, uint modelPointer = 0)
    {
        int poll = Host.InputManager.CurrentPoll;
        if (TraceScenePassPoll < 0 || poll != TraceScenePassPoll)
            return;
        Console.Error.WriteLine(
            $"[GT2-Scene-Pass] poll={poll} phase={phase} " +
            $"pass={_scenePass} generation={_sceneGeneration} " +
            $"model=0x{modelPointer:X8}");
    }
#endif
    internal static int CurrentTrackMeshIndexBits =>
        _currentAuxiliaryTrack ? 10 : 9;
    internal static bool CurrentTrackMeshUsesAuxiliaryFormat =>
        _currentAuxiliaryTrack;

    internal static void RegisterTrackMeshConsumer(TrackMeshConsumer consumer)
    {
        if (_trackMeshConsumer != null && _trackMeshConsumer != consumer)
            throw new InvalidOperationException(
                "Only one live GT2 track-mesh consumer may be registered.");
        _trackMeshConsumer = consumer;
    }

    internal static void UnregisterTrackMeshConsumer(TrackMeshConsumer consumer)
    {
        if (_trackMeshConsumer == consumer)
        {
            _trackMeshConsumer = null;
            ReportAuxiliaryTrackSummary();
        }
    }

    /// <summary>
    /// The authored-mesh consumer has already captured every input the modern
    /// renderer needs before GT2 enters its GTE projection/packet functions.
    /// Those original functions must still run when no modern consumer owns
    /// the mesh, or when the development face-decision oracle is attached.
    /// Otherwise their packets are guaranteed to be discarded and executing
    /// them only repeats the entire course renderer on the guest thread.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool ShouldRunGuestTrackProjection()
    {
        bool required =
            _trackMeshConsumer is null || _trackFaceDecisionConsumer is not null;
        if (required)
            Interlocked.Increment(ref _guestTrackProjectionRuns);
        else
            Interlocked.Increment(ref _guestTrackProjectionBypasses);
        return required;
    }

    /// <summary>
    /// The authored-mesh owner also consumes GT2's model-header +0x24/+0x40
    /// billboard stream. Generated code asks here before expanding those
    /// records into discard-only PS1 GPU packets. The guest path remains for
    /// the exact face oracle and whenever no modern owner is registered.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool ShouldRunGuestTrackBillboardProjection()
    {
        bool required =
            _trackMeshConsumer is null || _trackFaceDecisionConsumer is not null;
        if (required)
            Interlocked.Increment(ref _guestTrackBillboardProjectionRuns);
        else
            Interlocked.Increment(ref _guestTrackBillboardProjectionBypasses);
        return required;
    }

    internal static void ReportTrackProjectionOwnership()
    {
        if (Interlocked.Exchange(ref _trackProjectionOwnershipReported, 1) != 0)
            return;
        long runs = Interlocked.Read(ref _guestTrackProjectionRuns);
        long bypasses = Interlocked.Read(ref _guestTrackProjectionBypasses);
        long billboardRuns =
            Interlocked.Read(ref _guestTrackBillboardProjectionRuns);
        long billboardBypasses =
            Interlocked.Read(ref _guestTrackBillboardProjectionBypasses);
        Console.Error.WriteLine(
            "[GT2-Track-Projection] " +
            $"modernOwned={bypasses} guestRuns={runs} " +
            $"billboardModernOwned={billboardBypasses} " +
            $"billboardGuestRuns={billboardRuns} " +
            $"total={runs + bypasses} " +
            $"billboardTotal={billboardRuns + billboardBypasses} " +
            "policy=preprojection-owner");
    }

    internal static void RegisterBackgroundMeshConsumer(
        BackgroundMeshConsumer consumer)
    {
        if (_backgroundMeshConsumer != null &&
            _backgroundMeshConsumer != consumer)
        {
            throw new InvalidOperationException(
                "Only one live GT2 background-mesh consumer may be registered.");
        }
        _backgroundMeshConsumer = consumer;
    }

    internal static void UnregisterBackgroundMeshConsumer(
        BackgroundMeshConsumer consumer)
    {
        if (_backgroundMeshConsumer == consumer)
            _backgroundMeshConsumer = null;
    }

    /// <summary>
    /// The modern backdrop consumer captures every authored vertex, material,
    /// and primitive after GT2 installs the camera-relative transform. Once it
    /// owns that model, the following guest 4:3 projection loops produce only
    /// packets which the modern world compositor discards.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool ShouldRunGuestBackgroundProjection()
    {
        bool required = _backgroundMeshConsumer is null;
        if (required)
            Interlocked.Increment(ref _guestBackgroundProjectionRuns);
        else
            Interlocked.Increment(ref _guestBackgroundProjectionBypasses);
        return required;
    }

    internal static void ReportBackgroundProjectionOwnership()
    {
        if (Interlocked.Exchange(
                ref _backgroundProjectionOwnershipReported,
                1) != 0)
        {
            return;
        }
        long runs = Interlocked.Read(ref _guestBackgroundProjectionRuns);
        long bypasses =
            Interlocked.Read(ref _guestBackgroundProjectionBypasses);
        Console.Error.WriteLine(
            "[GT2-Background-Projection] " +
            $"modernOwned={bypasses} guestRuns={runs} " +
            $"total={runs + bypasses} policy=preprojection-owner");
    }

    internal static void RegisterTrackFaceDecisionConsumer(
        TrackFaceDecisionConsumer consumer)
    {
        if (_trackFaceDecisionConsumer != null &&
            _trackFaceDecisionConsumer != consumer)
        {
            throw new InvalidOperationException(
                "Only one GT2 track-face decision consumer may be registered.");
        }
        _trackFaceDecisionConsumer = consumer;
    }

    internal static void UnregisterTrackFaceDecisionConsumer(
        TrackFaceDecisionConsumer consumer)
    {
        if (_trackFaceDecisionConsumer == consumer)
            _trackFaceDecisionConsumer = null;
    }

    public static void RegisterTrackObject(
        uint submissionPointer,
        uint stableId,
        uint modelPointer)
    {
        if (!CaptureEnabled || submissionPointer == 0 || modelPointer == 0)
            return;
        TrackObjects[submissionPointer] = new WorldObjectContext(
            WorldObjectKind.Track,
            stableId,
            modelPointer);
    }

    public static void BeginTrackObject(
        uint submissionPointer,
        uint modelPointer)
    {
        if (!CaptureEnabled)
            return;
        _currentAuxiliaryTrack = false;
        if (TrackObjects.TryGetValue(submissionPointer, out var context) &&
            context.ModelPointer == modelPointer)
        {
            _current = context with
            {
                ScenePass = _scenePass,
                SceneGeneration = _sceneGeneration,
            };
            return;
        }
        _current = new WorldObjectContext(
            WorldObjectKind.Track,
            0,
            modelPointer,
            ScenePass: _scenePass,
            SceneGeneration: _sceneGeneration);
    }

    /// <summary>
    /// Marks GT2's authored camera-centred backdrop model. Generated-code
    /// hooks bracket only the fixed model call, rather than classifying every
    /// otherwise-unowned projected packet as sky or scenery.
    /// </summary>
    public static void BeginBackgroundObject(uint modelPointer)
    {
        if (!CaptureEnabled)
            return;
        _currentAuxiliaryTrack = false;
        _current = new WorldObjectContext(
            WorldObjectKind.Background,
            modelPointer,
            modelPointer,
            ScenePass: _scenePass,
            SceneGeneration: _sceneGeneration);
    }

    /// <summary>
    /// Exposes GT2's authored backdrop model after the guest has installed its
    /// exact camera-relative GTE transform but before the original 4:3 screen
    /// rejection removes the side sectors required by a Hor+ camera.
    /// </summary>
    public static void TraceBackgroundMesh(uint modelPointer, IMemory memory) =>
        _backgroundMeshConsumer?.Invoke(modelPointer, memory);

    /// <summary>
    /// GT2 submits a second family of static course instances after its main
    /// sector list. Its authored mask selects camera-region alternatives and
    /// cannot be replaced with all bits set: doing so co-renders mutually
    /// exclusive road overlays and distant course regions. Preserve that
    /// coarse selection while maximum-detail model selection and the native
    /// homogeneous frustum own LOD and screen-edge clipping respectively.
    /// </summary>
    public static uint ExpandAuxiliaryTrackVisibility(uint visibilityMask)
    {
        if (AuxiliaryTrackTraceEnabled)
        {
            if (_trackMeshConsumer != null)
                Interlocked.Increment(ref _auxiliaryTrackVisibilityMasksPreserved);
            TraceAuxiliaryTrack(
                $"visibility=0x{visibilityMask:X8}->0x{visibilityMask:X8} " +
                "policy=authored-mutual-exclusion");
        }
        return visibilityMask;
    }

    /// <summary>
    /// The auxiliary instance path asks GT2's visibility/LOD policy for a
    /// model-table index. For resident capture, index zero is the authoritative
    /// maximum-detail member for every record admitted by GT2's authored
    /// auxiliary mask. The native renderer, rather than this PS1-era selector,
    /// owns detail reduction and final camera visibility.
    /// </summary>
    public static uint SelectAuxiliaryTrackModel(uint selectedModel)
    {
        if (_trackMeshConsumer == null)
            return selectedModel;
        if (AuxiliaryTrackTraceEnabled)
        {
            if (selectedModel != 0)
                Interlocked.Increment(ref _auxiliaryTrackModelSelectionsForced);
            TraceAuxiliaryTrack(
                $"model={unchecked((int)selectedModel)}->0 " +
                "policy=maximum-resident-detail");
        }
        return 0;
    }

    /// <summary>
    /// The guest's auxiliary bounding-box result is an early 4:3/PS1 draw
    /// rejection. Keep an authored-mask-selected object resident and let the
    /// native homogeneous frustum clip it. This is required for true Hor+
    /// without admitting mutually exclusive auxiliary groups.
    /// </summary>
    public static uint IncludeAuxiliaryTrackObject(uint clipFlags)
    {
        if (_trackMeshConsumer == null)
            return clipFlags;
        if (AuxiliaryTrackTraceEnabled)
        {
            if (clipFlags != 0)
                Interlocked.Increment(ref _auxiliaryTrackClipRejectionsBypassed);
            TraceAuxiliaryTrack(
                $"clip=0x{clipFlags:X8}->0x00000000 " +
                "policy=native-homogeneous-frustum");
        }
        return 0;
    }

    /// <summary>
    /// Identifies one auxiliary static-course instance at the point where GT2
    /// has computed its authored record address. That address remains stable
    /// and unique within the loaded course even though later guest code reuses
    /// the registers which held the list pointer and instance index.
    /// </summary>
    public static void BeginAuxiliaryTrackInstance(uint recordPointer)
    {
        if (!CaptureEnabled)
            return;
        _currentAuxiliaryTrack = true;
        _current = new WorldObjectContext(
            WorldObjectKind.Track,
            recordPointer,
            0,
            ScenePass: _scenePass,
            SceneGeneration: _sceneGeneration);
        if (AuxiliaryTrackTraceEnabled)
        {
            Interlocked.Increment(ref _auxiliaryTrackObjectsBegun);
            TraceAuxiliaryTrack(
                $"begin record=0x{recordPointer:X8} indices=10-bit");
        }
    }

    /// <summary>
    /// Completes the auxiliary instance identity only after GT2 has selected
    /// the authored model-table entry. Keeping this separate from the record
    /// hook prevents register reuse from corrupting stable object identity.
    /// </summary>
    public static void SetAuxiliaryTrackModel(uint modelPointer)
    {
        if (!CaptureEnabled)
            return;
        if (!_currentAuxiliaryTrack || _current.Kind != WorldObjectKind.Track)
            throw new InvalidOperationException(
                "GT2 auxiliary model has no active auxiliary track instance.");
        _current = _current with { ModelPointer = modelPointer };
        if (AuxiliaryTrackTraceEnabled)
        {
            TraceAuxiliaryTrack(
                $"model record=0x{_current.StableId:X8} " +
                $"pointer=0x{modelPointer:X8}");
        }
    }

    /// <summary>
    /// Attaches GT2's authored ordering-table normalization to the active
    /// object. The exponent maps normalized GTE view space back to one common
    /// depth space: commonZ = viewZ * 2^exponent. Projection snapshots retain
    /// this value alongside the exact transform so native depth reconstruction
    /// never has to infer scale from pixels or object type.
    /// </summary>
    public static void SetCurrentDepthScaleExponent(int exponent)
    {
        if (!CaptureEnabled)
            return;
        if (_current.Kind == WorldObjectKind.Unknown)
            throw new InvalidOperationException(
                "GT2 depth normalization has no active world object.");
        if (exponent is < -16 or > 16)
            throw new InvalidOperationException(
                $"GT2 depth normalization exponent is invalid: {exponent}.");
        _current = _current with
        {
            DepthScaleExponent = exponent,
            DepthScaleValid = true,
        };
    }

    static void TraceAuxiliaryTrack(string detail)
    {
        if (!AuxiliaryTrackTraceEnabled)
            return;
        if (Interlocked.Exchange(
                ref _auxiliaryTrackExitTraceRegistered,
                1) == 0)
        {
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
                ReportAuxiliaryTrackSummary();
        }
        int trace = Interlocked.Increment(ref _auxiliaryTrackTraceCount);
        if (trace <= AuxiliaryTrackTraceLimit)
            Console.Error.WriteLine(
                $"[GT2-Aux-Track] n={trace} {detail}");
    }

    static void ReportAuxiliaryTrackSummary()
    {
        if (!AuxiliaryTrackTraceEnabled ||
            Interlocked.Exchange(ref _auxiliaryTrackSummaryReported, 1) != 0)
        {
            return;
        }
        Console.Error.WriteLine(
            "[GT2-Aux-Track-Summary] " +
            $"visibilityMasksPreserved=" +
            $"{Interlocked.Read(ref _auxiliaryTrackVisibilityMasksPreserved)} " +
            $"forcedModels=" +
            $"{Interlocked.Read(ref _auxiliaryTrackModelSelectionsForced)} " +
            $"bypassedClipRejections=" +
            $"{Interlocked.Read(ref _auxiliaryTrackClipRejectionsBypassed)} " +
            $"objects=" +
            $"{Interlocked.Read(ref _auxiliaryTrackObjectsBegun)}");
    }

    /// <summary>
    /// Development-only primitive oracle. Generated GT2 code supplies the
    /// exact NCLIP registers and resolved branch result; the resident renderer
    /// uses this only for correlation and never for shipping visibility.
    /// </summary>
    public static void TraceTrackFaceDecision(
        uint primitivePointer,
        TrackMeshProjectionPath projectionPath,
        int stream,
        uint packedIndices,
        uint firstNclip,
        uint secondNclip,
        bool guestAccepted) =>
        _trackFaceDecisionConsumer?.Invoke(
            primitivePointer,
            projectionPath,
            stream,
            packedIndices,
            unchecked((int)firstNclip),
            unchecked((int)secondNclip),
            guestAccepted);

    /// <summary>
    /// Development-only GT2 model-header hook. This exposes the unprojected
    /// vertex table and primitive streams before the guest renderer can apply
    /// its original screen-space rejection rules. With no consumer it is
    /// trace-only; the resident-course renderer registers one explicitly.
    /// </summary>
    public static void TraceTrackMesh(
        uint meshPointer,
        IMemory memory,
        TrackMeshProjectionPath projectionPath)
    {
        _trackMeshConsumer?.Invoke(meshPointer, memory, projectionPath);

        WorldObjectContext current = _current;
        TrackMeshTraceKey key = new(
            current.StableId,
            current.ModelPointer,
            meshPointer,
            projectionPath);
        if (!TrackMeshTraceEnabled ||
            meshPointer < 0x80000000u ||
            meshPointer > 0x807FFFBBu ||
            TracedTrackMeshes.Count >= MaxTracedTrackMeshes ||
            !TracedTrackMeshes.Add(key))
        {
            return;
        }

        uint vertexPointer = memory.ReadU32(meshPointer);
        uint vertexRecords = memory.ReadU32(meshPointer + 0x2Cu) + 1u;
        int indexBits = CurrentTrackMeshIndexBits;
        bool auxiliaryFormat = CurrentTrackMeshUsesAuxiliaryFormat;
        uint indexMask = (1u << indexBits) - 1u;
        Span<uint> streamPointers = stackalloc uint[8];
        Span<ushort> primitiveCounts = stackalloc ushort[8];
        uint totalPrimitives = 0;
        uint totalTriangles = 0;
        uint invalidIndices = 0;
        uint invalidPointers = 0;
        uint noncontiguousStreams = 0;
        uint materialRecords = 0;
        uint materialCommandMismatches = 0;
        uint texturedPrimitives = 0;
        uint textureTableInvalid = 0;
        uint textureEntryInvalid = 0;
        uint textureAlternateEntryInvalid = 0;
        uint textureClutHighBit = 0;
        uint effectPointer = memory.ReadU32(meshPointer + 0x28u);
        ushort effectRecords = memory.ReadU16(meshPointer + 0x42u);
        uint effectPointerInvalid = 0;
        uint effectNonzeroReservedWords = 0;
        short effectMinimumX = short.MaxValue;
        short effectMinimumY = short.MaxValue;
        short effectMinimumZ = short.MaxValue;
        short effectMinimumScale = short.MaxValue;
        short effectMaximumX = short.MinValue;
        short effectMaximumY = short.MinValue;
        short effectMaximumZ = short.MinValue;
        short effectMaximumScale = short.MinValue;
        var streams = new System.Text.StringBuilder(256);
        uint textureTableBase = memory.ReadU32(0x1F8003A0u);
        if (textureTableBase != 0)
            TrackTextureTableBases.Add(textureTableBase);

        if (!IsGuestRange(vertexPointer, (ulong)vertexRecords * 8u))
            invalidPointers++;

        // func_8001F784 consumes the final model-header stream separately
        // from the indexed mesh: count records at +0x42, pointer at +0x28,
        // and a fixed 20-byte stride. Each record supplies a world-space
        // point (+0/+2/+4), a projection scale (+6), and packed RGB (+0x10),
        // from which the guest synthesizes four translucent screen-facing
        // quads. Keep this audit distinct from resident mesh triangles so a
        // modern effect implementation cannot silently omit or misclassify it.
        ulong effectBytes = (ulong)effectRecords * 20u;
        if (effectRecords != 0 && !IsGuestRange(effectPointer, effectBytes))
        {
            effectPointerInvalid++;
            invalidPointers++;
        }
        else
        {
            for (uint item = 0; item < effectRecords; item++)
            {
                uint record = effectPointer + item * 20u;
                short x = unchecked((short)memory.ReadU16(record));
                short y = unchecked((short)memory.ReadU16(record + 2u));
                short z = unchecked((short)memory.ReadU16(record + 4u));
                short scale = unchecked((short)memory.ReadU16(record + 6u));
                effectMinimumX = Math.Min(effectMinimumX, x);
                effectMinimumY = Math.Min(effectMinimumY, y);
                effectMinimumZ = Math.Min(effectMinimumZ, z);
                effectMinimumScale = Math.Min(effectMinimumScale, scale);
                effectMaximumX = Math.Max(effectMaximumX, x);
                effectMaximumY = Math.Max(effectMaximumY, y);
                effectMaximumZ = Math.Max(effectMaximumZ, z);
                effectMaximumScale = Math.Max(effectMaximumScale, scale);
                if (memory.ReadU32(record + 8u) != 0)
                    effectNonzeroReservedWords++;
                if (memory.ReadU32(record + 0x0Cu) != 0)
                    effectNonzeroReservedWords++;
                TrackEffectColors.Add(memory.ReadU32(record + 0x10u));
            }
        }

        short minimumX = short.MaxValue;
        short minimumY = short.MaxValue;
        short minimumZ = short.MaxValue;
        short maximumX = short.MinValue;
        short maximumY = short.MinValue;
        short maximumZ = short.MinValue;
        bool writeObj = !string.IsNullOrWhiteSpace(TrackMeshObjPath) &&
            invalidPointers == 0;
        int objVertexBase = _trackMeshObjVertices;
        short[]? modelHeights = writeObj
            ? new short[checked((int)vertexRecords)]
            : null;
        if (writeObj)
        {
            AppendObjObject(TrackMeshObj, meshPointer);
            AppendObjObject(TrackMeshGroundObj, meshPointer);
        }
        if (invalidPointers == 0)
        {
            for (uint index = 0; index < vertexRecords; index++)
            {
                uint vertex = vertexPointer + index * 8u;
                short x = unchecked((short)memory.ReadU16(vertex));
                short y = unchecked((short)memory.ReadU16(vertex + 2u));
                short z = unchecked((short)memory.ReadU16(vertex + 4u));
                minimumX = Math.Min(minimumX, x);
                minimumY = Math.Min(minimumY, y);
                minimumZ = Math.Min(minimumZ, z);
                maximumX = Math.Max(maximumX, x);
                maximumY = Math.Max(maximumY, y);
                maximumZ = Math.Max(maximumZ, z);
                if (modelHeights != null)
                    modelHeights[index] = z;
                if (writeObj)
                {
                    GteProjectionOrigin origin =
                        Gte.SnapshotProjectionOrigin(x, y, z);
                    AppendObjVertex(TrackMeshObj, in origin);
                    AppendObjVertex(TrackMeshGroundObj, in origin);
                }
            }
            if (writeObj)
                _trackMeshObjVertices += checked((int)vertexRecords);
        }

        for (int index = 0; index < streamPointers.Length; index++)
        {
            streamPointers[index] = memory.ReadU32(
                meshPointer + 0x04u + checked((uint)index * 4u));
            primitiveCounts[index] = memory.ReadU16(
                meshPointer + 0x30u + checked((uint)index * 2u));
            TrackPrimitiveStreamTotals[index] += primitiveCounts[index];
            totalPrimitives += primitiveCounts[index];
            totalTriangles += (uint)primitiveCounts[index] *
                ((index & 1) == 0 ? 1u : 2u);

            ulong streamBytes = (ulong)primitiveCounts[index] *
                (uint)(auxiliaryFormat
                    ? AuxiliaryTrackPrimitiveRecordSizes[index]
                    : TrackPrimitiveRecordSizes[index]);
            if (!IsGuestRange(streamPointers[index], streamBytes))
                invalidPointers++;
            if (index > 0)
            {
                uint expected = unchecked(
                    streamPointers[index - 1] +
                    (uint)primitiveCounts[index - 1] *
                    (uint)(auxiliaryFormat
                        ? AuxiliaryTrackPrimitiveRecordSizes[index - 1]
                        : TrackPrimitiveRecordSizes[index - 1]));
                if (streamPointers[index] != expected)
                    noncontiguousStreams++;
            }

            if (IsGuestRange(streamPointers[index], streamBytes))
            {
                uint primitive = streamPointers[index];
                int recordSize = auxiliaryFormat
                    ? AuxiliaryTrackPrimitiveRecordSizes[index]
                    : TrackPrimitiveRecordSizes[index];
                for (int item = 0; item < primitiveCounts[index]; item++)
                {
                    uint packed = memory.ReadU32(primitive);
                    uint vertex0 = packed & indexMask;
                    uint vertex1 = (packed >> indexBits) & indexMask;
                    uint vertex2 = (packed >> (indexBits * 2)) & indexMask;
                    invalidIndices += CountInvalidTrackIndex(vertex0, vertexRecords);
                    invalidIndices += CountInvalidTrackIndex(vertex1, vertexRecords);
                    invalidIndices += CountInvalidTrackIndex(vertex2, vertexRecords);
                    uint vertex3 = 0;
                    if ((index & 1) != 0)
                    {
                        vertex3 = memory.ReadU32(primitive + 4u) & indexMask;
                        invalidIndices += CountInvalidTrackIndex(
                            vertex3,
                            vertexRecords);
                    }
                    uint colorCommand = memory.ReadU32(primitive + 8u);
                    byte command = (byte)(colorCommand >> 24);
                    TrackPrimitiveCommandTotals[command]++;
                    materialRecords++;
                    byte expectedCommandFamily = (byte)(
                        0x20 |
                        ((index & 1) != 0 ? 0x08 : 0x00) |
                        (index >= 4 ? 0x04 : 0x00) |
                        ((index & 2) != 0 ? 0x10 : 0x00));
                    if ((command & 0xFC) != expectedCommandFamily)
                        materialCommandMismatches++;
                    if (index >= 4)
                    {
                        texturedPrimitives++;
                        if (auxiliaryFormat)
                        {
                            ushort clut = (ushort)(
                                memory.ReadU32(primitive + 0x0Cu) >> 16);
                            if ((clut & 0x8000) != 0)
                                textureClutHighBit++;
                        }
                        else
                        {
                            uint descriptor = memory.ReadU32(primitive + 4u);
                            uint entryOffset =
                                (descriptor >> 4) & 0x0007FFE0u;
                            if (!IsGuestRange(textureTableBase, 0x20u))
                            {
                                textureTableInvalid++;
                            }
                            else
                            {
                                uint entry = unchecked(
                                    textureTableBase + entryOffset);
                                TrackTextureEntries.Add(entry);
                                if (!IsGuestRange(entry, 0x10u))
                                {
                                    textureEntryInvalid++;
                                }
                                else
                                {
                                    ushort clut = (ushort)(
                                        memory.ReadU32(entry) >> 16);
                                    if ((clut & 0x8000) != 0)
                                        textureClutHighBit++;
                                }
                                if (!IsGuestRange(
                                        unchecked(entry + 0x10u),
                                        0x10u))
                                {
                                    textureAlternateEntryInvalid++;
                                }
                            }
                        }
                    }
                    if (writeObj &&
                        vertex0 < vertexRecords &&
                        vertex1 < vertexRecords &&
                        vertex2 < vertexRecords &&
                        ((index & 1) == 0 || vertex3 < vertexRecords))
                    {
                        // GT2 submits triangles as 0,2,1. Quads are submitted
                        // as 2,1,3,0 and the GPU packet path splits them into
                        // (2,1,3) and (1,3,0). Preserve that authored topology
                        // instead of assuming sequential 0,1,2,3 adjacency.
                        if ((index & 1) == 0)
                        {
                            AppendObjFace(
                                TrackMeshObj,
                                objVertexBase,
                                vertex0,
                                vertex2,
                                vertex1);
                        }
                        else
                        {
                            AppendObjFace(
                                TrackMeshObj,
                                objVertexBase,
                                vertex2,
                                vertex1,
                                vertex3);
                            AppendObjFace(
                                TrackMeshObj,
                                objVertexBase,
                                vertex1,
                                vertex3,
                                vertex0);
                        }
                        short minimumHeight = Math.Min(
                            modelHeights![vertex0],
                            Math.Min(
                                modelHeights[vertex1],
                                modelHeights[vertex2]));
                        short maximumHeight = Math.Max(
                            modelHeights[vertex0],
                            Math.Max(
                                modelHeights[vertex1],
                                modelHeights[vertex2]));
                        if ((index & 1) != 0)
                        {
                            minimumHeight = Math.Min(
                                minimumHeight,
                                modelHeights[vertex3]);
                            maximumHeight = Math.Max(
                                maximumHeight,
                                modelHeights[vertex3]);
                        }
                        if (maximumHeight - minimumHeight <= 64)
                        {
                            if ((index & 1) == 0)
                            {
                                AppendObjFace(
                                    TrackMeshGroundObj,
                                    objVertexBase,
                                    vertex0,
                                    vertex2,
                                    vertex1);
                                _trackMeshObjGroundTriangles++;
                            }
                            else
                            {
                                AppendObjFace(
                                    TrackMeshGroundObj,
                                    objVertexBase,
                                    vertex2,
                                    vertex1,
                                    vertex3);
                                AppendObjFace(
                                    TrackMeshGroundObj,
                                    objVertexBase,
                                    vertex1,
                                    vertex3,
                                    vertex0);
                                _trackMeshObjGroundTriangles += 2;
                            }
                        }
                    }
                    primitive += (uint)recordSize;
                }
            }

            if (streamPointers[index] == 0u && primitiveCounts[index] == 0)
                continue;
            if (streams.Length != 0)
                streams.Append(',');
            streams.Append(index)
                .Append(':')
                .Append(streamPointers[index].ToString("X8"))
                .Append('/')
                .Append(primitiveCounts[index]);
        }

        _tracedTrackVertices += vertexRecords;
        _tracedTrackPrimitives += totalPrimitives;
        _tracedTrackTriangles += totalTriangles;
        _tracedTrackInvalidIndices += invalidIndices;
        _tracedTrackInvalidPointers += invalidPointers;
        _tracedTrackNoncontiguousStreams += noncontiguousStreams;
        _tracedTrackMaterialRecords += materialRecords;
        _tracedTrackMaterialCommandMismatches +=
            materialCommandMismatches;
        _tracedTrackTexturedPrimitives += texturedPrimitives;
        _tracedTrackTextureTableInvalid += textureTableInvalid;
        _tracedTrackTextureEntryInvalid += textureEntryInvalid;
        _tracedTrackTextureAlternateEntryInvalid +=
            textureAlternateEntryInvalid;
        _tracedTrackTextureClutHighBit += textureClutHighBit;
        if (effectRecords != 0)
            _tracedTrackEffectMeshes++;
        _tracedTrackEffectRecords += effectRecords;
        _tracedTrackEffectPointerInvalid += effectPointerInvalid;
        _tracedTrackEffectNonzeroReservedWords +=
            effectNonzeroReservedWords;
        RegisterTrackMeshExitTrace();

        string effectBounds = effectRecords != 0 && effectPointerInvalid == 0
            ? $"{effectMinimumX},{effectMinimumY},{effectMinimumZ}.." +
                $"{effectMaximumX},{effectMaximumY},{effectMaximumZ} " +
                $"scale={effectMinimumScale}..{effectMaximumScale}"
            : "none";

        Console.Error.WriteLine(
            $"[GT2-Track-Mesh] ordinal={TracedTrackMeshes.Count} " +
            $"stable={current.StableId} base={current.ModelPointer:X8} " +
            $"format={(auxiliaryFormat ? "auxiliary" : "primary")}/" +
            $"{indexBits}-bit " +
            $"mesh={meshPointer:X8} vertices={vertexPointer:X8}/{vertexRecords} " +
            $"primitives={totalPrimitives} triangles={totalTriangles} " +
            $"bounds={minimumX},{minimumY},{minimumZ}.." +
            $"{maximumX},{maximumY},{maximumZ} " +
            $"invalidIndices={invalidIndices} invalidPointers={invalidPointers} " +
            $"noncontiguous={noncontiguousStreams} materials={materialRecords} " +
            $"commandMismatch={materialCommandMismatches} " +
            $"textured={texturedPrimitives} " +
            $"textureTable={textureTableBase:X8} " +
            $"textureTableInvalid={textureTableInvalid} " +
            $"textureEntryInvalid={textureEntryInvalid}/" +
            $"{textureAlternateEntryInvalid} " +
            $"effects={effectPointer:X8}/{effectRecords} " +
            $"effectPointerInvalid={effectPointerInvalid} " +
            $"effectReservedNonzero={effectNonzeroReservedWords} " +
            $"effectBounds={effectBounds} streams={streams}");
    }

    static uint CountInvalidTrackIndex(uint index, uint vertexRecords) =>
        index < vertexRecords ? 0u : 1u;

    static void AppendObjObject(
        System.Text.StringBuilder output,
        uint meshPointer)
    {
        output.Append("o track_")
            .Append(_current.StableId)
            .Append('_')
            .Append(meshPointer.ToString("X8"))
            .AppendLine();
    }

    static void AppendObjVertex(
        System.Text.StringBuilder output,
        in GteProjectionOrigin origin)
    {
        output.Append("v ")
            .Append(origin.ViewX)
            .Append(' ')
            .Append(origin.ViewY)
            .Append(' ')
            .Append(origin.ViewZ)
            .AppendLine();
    }

    static void AppendObjFace(
        System.Text.StringBuilder output,
        int vertexBase,
        uint vertex0,
        uint vertex1,
        uint vertex2)
    {
        output.Append("f ")
            .Append(vertexBase + vertex0 + 1)
            .Append(' ')
            .Append(vertexBase + vertex1 + 1)
            .Append(' ')
            .Append(vertexBase + vertex2 + 1)
            .AppendLine();
    }

    static bool IsGuestRange(uint pointer, ulong length)
    {
        if (pointer < 0x80000000u || pointer > 0x807FFFFFu)
            return false;
        ulong offset = pointer - 0x80000000u;
        return length <= 0x00800000u && offset + length <= 0x00800000u;
    }

    static void RegisterTrackMeshExitTrace()
    {
        if (_trackMeshExitTraceRegistered)
            return;
        _trackMeshExitTraceRegistered = true;
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            ReportTrackMeshTraceSummary();
    }

    public static void ReportTrackMeshTraceSummary()
    {
        if (!TrackMeshTraceEnabled ||
            _trackMeshTraceReported ||
            TracedTrackMeshes.Count == 0)
        {
            return;
        }
        _trackMeshTraceReported = true;
        Console.Error.WriteLine(
            $"[GT2-Track-Mesh-Summary] objects={TracedTrackMeshes.Count} " +
            $"vertices={_tracedTrackVertices} " +
            $"primitives={_tracedTrackPrimitives} " +
            $"triangles={_tracedTrackTriangles} " +
            $"invalidIndices={_tracedTrackInvalidIndices} " +
            $"invalidPointers={_tracedTrackInvalidPointers} " +
            $"noncontiguous={_tracedTrackNoncontiguousStreams} " +
            $"materials={_tracedTrackMaterialRecords} " +
            $"commandMismatch={_tracedTrackMaterialCommandMismatches} " +
            $"textured={_tracedTrackTexturedPrimitives} " +
            $"textureTables={TrackTextureTableBases.Count} " +
            $"textureEntries={TrackTextureEntries.Count} " +
            $"textureTableInvalid={_tracedTrackTextureTableInvalid} " +
            $"textureEntryInvalid={_tracedTrackTextureEntryInvalid}/" +
            $"{_tracedTrackTextureAlternateEntryInvalid} " +
            $"clutHighBit={_tracedTrackTextureClutHighBit} " +
            $"effects={_tracedTrackEffectMeshes}/" +
            $"{_tracedTrackEffectRecords} " +
            $"effectPointerInvalid={_tracedTrackEffectPointerInvalid} " +
            $"effectReservedNonzero=" +
            $"{_tracedTrackEffectNonzeroReservedWords} " +
            $"effectColors={TrackEffectColors.Count} " +
            $"streams={FormatNonzeroTotals(TrackPrimitiveStreamTotals)} " +
            $"commands={FormatNonzeroTotals(TrackPrimitiveCommandTotals)}");
        if (!string.IsNullOrWhiteSpace(TrackMeshObjPath))
        {
            try
            {
                string output = Path.GetFullPath(TrackMeshObjPath);
                string? directory = Path.GetDirectoryName(output);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);
                File.WriteAllText(output, TrackMeshObj.ToString());
                string groundOutput = Path.Combine(
                    directory ?? string.Empty,
                    Path.GetFileNameWithoutExtension(output) + ".ground.obj");
                File.WriteAllText(
                    groundOutput,
                    TrackMeshGroundObj.ToString());
                Console.Error.WriteLine(
                    $"[GT2-Track-Mesh-OBJ] path={output} " +
                    $"vertices={_trackMeshObjVertices} " +
                    $"triangles={_tracedTrackTriangles} " +
                    $"groundPath={groundOutput} " +
                    $"groundTriangles={_trackMeshObjGroundTriangles}");
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    $"[GT2-Track-Mesh-OBJ] failed: {exception.Message}");
            }
        }
    }

    static string FormatNonzeroTotals(long[] totals)
    {
        var output = new System.Text.StringBuilder(128);
        for (int index = 0; index < totals.Length; index++)
        {
            if (totals[index] == 0)
                continue;
            if (output.Length != 0)
                output.Append(',');
            output.Append(index.ToString("X2"))
                .Append(':')
                .Append(totals[index]);
        }
        return output.ToString();
    }

    public static void BeginVehicle(uint carState, uint modelPointer)
    {
        if (!CaptureEnabled)
            return;
        _currentAuxiliaryTrack = false;
        if (!VehicleIds.TryGetValue(carState, out uint stableId))
        {
            stableId = checked((uint)VehicleIds.Count);
            VehicleIds[carState] = stableId;
        }
        _current = new WorldObjectContext(
            WorldObjectKind.Vehicle,
            stableId,
            modelPointer,
            ScenePass: _scenePass,
            // Preview/showroom cars are not part of a race scene even after
            // returning from a previous race. Generation zero keeps their
            // authored screen projection in the DirectX command compositor.
            SceneGeneration: _scenePassActive ? _sceneGeneration : 0);
#if !OPENGT_RELEASE_PACKAGE
        TraceScenePass("vehicle", modelPointer);
#endif
    }

    public static void EndObject()
    {
        _currentAuxiliaryTrack = false;
        if (CaptureEnabled)
            _current = default;
    }
}
