using RecompOne.Runtime.Memory;

namespace RecompOne.Runtime;

public enum WorldObjectKind : uint
{
    Unknown = 0,
    Track = 1,
    Vehicle = 2,
}

public readonly record struct WorldObjectContext(
    WorldObjectKind Kind,
    uint StableId,
    uint ModelPointer);

/// <summary>
/// GT2-specific generated-code hooks identify the object whose model is about
/// to use the GTE. Projection provenance snapshots this value, so later GPU
/// packets retain authored object identity without guessing from screen space.
/// </summary>
public static class WorldCaptureContext
{
    readonly record struct TrackMeshTraceKey(
        uint StableId,
        uint ModelPointer,
        uint MeshPointer);

    const int MaxTracedTrackMeshes = 512;
    static readonly int[] TrackPrimitiveRecordSizes =
        [12, 12, 20, 24, 12, 12, 20, 24];

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
    static bool _trackMeshExitTraceRegistered;
    static bool _trackMeshTraceReported;
    static long _tracedTrackVertices;
    static long _tracedTrackPrimitives;
    static long _tracedTrackTriangles;
    static long _tracedTrackInvalidIndices;
    static long _tracedTrackInvalidPointers;
    static long _tracedTrackNoncontiguousStreams;
    static readonly System.Text.StringBuilder TrackMeshObj = new(2_000_000);
    static readonly System.Text.StringBuilder TrackMeshGroundObj = new(1_000_000);
    static int _trackMeshObjVertices;
    static long _trackMeshObjGroundTriangles;

    public static bool LiveRenderingEnabled { get; set; }
    public static bool CaptureEnabled
    {
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => FileCaptureEnabled || LiveRenderingEnabled;
    }
    public static WorldObjectContext Current =>
        CaptureEnabled ? _current : default;

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
        if (TrackObjects.TryGetValue(submissionPointer, out var context) &&
            context.ModelPointer == modelPointer)
        {
            _current = context;
            return;
        }
        _current = new WorldObjectContext(WorldObjectKind.Track, 0, modelPointer);
    }

    /// <summary>
    /// Development-only GT2 model-header trace. This records the unprojected
    /// vertex table and primitive streams before the guest renderer can apply
    /// its original screen-space rejection rules. It does not alter rendering.
    /// </summary>
    public static void TraceTrackMesh(uint meshPointer, IMemory memory)
    {
        WorldObjectContext current = _current;
        TrackMeshTraceKey key = new(
            current.StableId,
            current.ModelPointer,
            meshPointer);
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
        Span<uint> streamPointers = stackalloc uint[8];
        Span<ushort> primitiveCounts = stackalloc ushort[8];
        uint totalPrimitives = 0;
        uint totalTriangles = 0;
        uint invalidIndices = 0;
        uint invalidPointers = 0;
        uint noncontiguousStreams = 0;
        var streams = new System.Text.StringBuilder(256);

        if (!IsGuestRange(vertexPointer, (ulong)vertexRecords * 8u))
            invalidPointers++;

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
            totalPrimitives += primitiveCounts[index];
            totalTriangles += (uint)primitiveCounts[index] *
                ((index & 1) == 0 ? 1u : 2u);

            ulong streamBytes = (ulong)primitiveCounts[index] *
                (uint)TrackPrimitiveRecordSizes[index];
            if (!IsGuestRange(streamPointers[index], streamBytes))
                invalidPointers++;
            if (index > 0)
            {
                uint expected = unchecked(
                    streamPointers[index - 1] +
                    (uint)primitiveCounts[index - 1] *
                    (uint)TrackPrimitiveRecordSizes[index - 1]);
                if (streamPointers[index] != expected)
                    noncontiguousStreams++;
            }

            if (IsGuestRange(streamPointers[index], streamBytes))
            {
                uint primitive = streamPointers[index];
                int recordSize = TrackPrimitiveRecordSizes[index];
                for (int item = 0; item < primitiveCounts[index]; item++)
                {
                    uint packed = memory.ReadU32(primitive);
                    uint vertex0 = packed & 0x1FFu;
                    uint vertex1 = (packed >> 9) & 0x1FFu;
                    uint vertex2 = (packed >> 18) & 0x1FFu;
                    invalidIndices += CountInvalidTrackIndex(vertex0, vertexRecords);
                    invalidIndices += CountInvalidTrackIndex(vertex1, vertexRecords);
                    invalidIndices += CountInvalidTrackIndex(vertex2, vertexRecords);
                    uint vertex3 = 0;
                    if ((index & 1) != 0)
                    {
                        vertex3 = memory.ReadU32(primitive + 4u) & 0x1FFu;
                        invalidIndices += CountInvalidTrackIndex(
                            vertex3,
                            vertexRecords);
                    }
                    if (writeObj &&
                        vertex0 < vertexRecords &&
                        vertex1 < vertexRecords &&
                        vertex2 < vertexRecords &&
                        ((index & 1) == 0 || vertex3 < vertexRecords))
                    {
                        AppendObjFace(
                            TrackMeshObj,
                            objVertexBase,
                            vertex0,
                            vertex1,
                            vertex2);
                        if ((index & 1) != 0)
                        {
                            AppendObjFace(
                                TrackMeshObj,
                                objVertexBase,
                                vertex1,
                                vertex2,
                                vertex3);
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
                            AppendObjFace(
                                TrackMeshGroundObj,
                                objVertexBase,
                                vertex0,
                                vertex1,
                                vertex2);
                            _trackMeshObjGroundTriangles++;
                            if ((index & 1) != 0)
                            {
                                AppendObjFace(
                                    TrackMeshGroundObj,
                                    objVertexBase,
                                    vertex1,
                                    vertex2,
                                    vertex3);
                                _trackMeshObjGroundTriangles++;
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
        RegisterTrackMeshExitTrace();

        Console.Error.WriteLine(
            $"[GT2-Track-Mesh] ordinal={TracedTrackMeshes.Count} " +
            $"stable={current.StableId} base={current.ModelPointer:X8} " +
            $"mesh={meshPointer:X8} vertices={vertexPointer:X8}/{vertexRecords} " +
            $"primitives={totalPrimitives} triangles={totalTriangles} " +
            $"bounds={minimumX},{minimumY},{minimumZ}.." +
            $"{maximumX},{maximumY},{maximumZ} " +
            $"invalidIndices={invalidIndices} invalidPointers={invalidPointers} " +
            $"noncontiguous={noncontiguousStreams} streams={streams}");
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
            $"noncontiguous={_tracedTrackNoncontiguousStreams}");
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

    public static void BeginVehicle(uint carState, uint modelPointer)
    {
        if (!CaptureEnabled)
            return;
        if (!VehicleIds.TryGetValue(carState, out uint stableId))
        {
            stableId = checked((uint)VehicleIds.Count);
            VehicleIds[carState] = stableId;
        }
        _current = new WorldObjectContext(
            WorldObjectKind.Vehicle,
            stableId,
            modelPointer);
    }

    public static void EndObject()
    {
        if (CaptureEnabled)
            _current = default;
    }
}
