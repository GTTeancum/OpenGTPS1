using System.Buffers;
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

    readonly record struct RawTrackTriangleKey(
        uint StableId,
        uint ModelPointer,
        ulong TransformId,
        short Ax, short Ay, short Az,
        short Bx, short By, short Bz,
        short Cx, short Cy, short Cz);

    readonly record struct RawTrackExpectedTriangle(
        HleVertex A,
        HleVertex B,
        HleVertex C,
        PrimFlags Flags,
        uint PrimitiveAddress,
        int Stream,
        int Item,
        double ViewDeterminant,
        bool AllInFront);

    static readonly int[] RawTrackPrimitiveRecordSizes =
        [12, 12, 20, 24, 12, 12, 20, 24];
    const int RawTrackBillboardStream = 8;
    const int RawTrackCorrelationStreamCount = 9;

    readonly bool _rawTrackReplacementRequested =
        string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_DEV_GT2_RAW_TRACK_REPLACE"),
            "1",
            StringComparison.Ordinal);
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
    readonly bool _rawTrackCullDisabled =
        string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_DEV_GT2_RAW_TRACK_NO_CULL"),
            "1",
            StringComparison.Ordinal);
    readonly HashSet<RawTrackInstanceKey> _rawTrackInstances = [];
    readonly HashSet<ulong> _rawTrackTransforms = [];
    readonly Dictionary<RawTrackTriangleKey, List<RawTrackExpectedTriangle>>
        _rawTrackExpectedTriangles = [];
    bool _rawTrackReplacementRegistered;
    long _rawTrackPendingFrame = long.MinValue;
    long _rawTrackFrames;
    long _rawTrackObjects;
    long _rawTrackSourcePrimitives;
    long _rawTrackTriangles;
    long _rawTrackTexturedPrimitives;
    long _rawTrackDuplicateCalls;
    long _rawTrackSingleSidedPrimitives;
    long _rawTrackDoubleSidedPrimitives;
    long _rawTrackGuestTriangles;
    long _rawTrackCorrelationGeometryMatches;
    long _rawTrackCorrelationExactMatches;
    long _rawTrackCorrelationUvMismatches;
    long _rawTrackCorrelationMaterialMismatches;
    long _rawTrackCorrelationColorMismatches;
    readonly long[] _rawTrackGuestGeometryPositiveByStream =
        new long[RawTrackCorrelationStreamCount];
    readonly long[] _rawTrackGuestGeometryNegativeByStream =
        new long[RawTrackCorrelationStreamCount];
    readonly long[] _rawTrackGuestFrontPositiveByStream =
        new long[RawTrackCorrelationStreamCount];
    readonly long[] _rawTrackGuestFrontNegativeByStream =
        new long[RawTrackCorrelationStreamCount];
    long _rawTrackBillboardPrimitives;
    long _rawTrackBillboardTriangles;
    long _rawTrackBackfaceCulledTriangles;
    long _rawTrackDegenerateTriangles;
    int _rawTrackEnvironmentTraceCount;
    long _rawTrackAllPositiveTriangles;
    long _rawTrackViewportIntersectingTriangles;
    int _rawTrackMaximumTransformsPerFrame;
    int _rawTrackVisibilityTraceCount;

    internal bool RawTrackReplacementActive =>
        _rawTrackReplacementRegistered;

    void RegisterRawTrackReplacement()
    {
        if (!_rawTrackReplacementRequested)
            return;
        if (!_liveWorldCapture.Enabled)
            throw new InvalidOperationException(
                "Raw GT2 track replacement requires the native world renderer.");
        WorldCaptureContext.RegisterTrackMeshConsumer(CaptureRawTrackMesh);
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
        _rawTrackReplacementRegistered = false;
    }

    void ReportRawTrackReplacement()
    {
        if (!_rawTrackReplacementRequested)
            return;
        Console.Error.WriteLine(
            $"[GT2-Raw-Track-Summary] frames={_rawTrackFrames} " +
            $"objects={_rawTrackObjects} " +
            $"sourcePrimitives={_rawTrackSourcePrimitives} " +
            $"triangles={_rawTrackTriangles} " +
            $"textured={_rawTrackTexturedPrimitives} " +
            $"billboards={_rawTrackBillboardPrimitives}/" +
            $"{_rawTrackBillboardTriangles} " +
            $"duplicateCalls={_rawTrackDuplicateCalls} " +
            $"singleSided={_rawTrackSingleSidedPrimitives} " +
            $"doubleSided={_rawTrackDoubleSidedPrimitives} " +
            $"backfaceCulled={_rawTrackBackfaceCulledTriangles} " +
            $"degenerateCulled={_rawTrackDegenerateTriangles} " +
            $"allPositive={_rawTrackAllPositiveTriangles} " +
            $"viewportIntersecting={_rawTrackViewportIntersectingTriangles} " +
            $"maxTransformsPerFrame={_rawTrackMaximumTransformsPerFrame} " +
            (_rawTrackCorrelationRequested
                ? $"guestTriangles={_rawTrackGuestTriangles} " +
                  $"geometryMatches={_rawTrackCorrelationGeometryMatches} " +
                  $"exactMatches={_rawTrackCorrelationExactMatches} " +
                  $"uvMismatches={_rawTrackCorrelationUvMismatches} " +
                  $"materialMismatches={_rawTrackCorrelationMaterialMismatches} " +
                  $"colorMismatches={_rawTrackCorrelationColorMismatches} "
                : string.Empty) +
            (_rawTrackCorrelationRequested
                ? $"guestGeometryWinding={RawTrackGuestGeometryWinding()} "
                  + $"guestFrontWinding={RawTrackGuestFrontWinding()} "
                : string.Empty) +
            "decodeFailures=0 guestTrackFallbacks=0");
    }

    void CaptureRawTrackMesh(
        uint meshPointer,
        IMemory memory,
        TrackMeshProjectionPath projectionPath)
    {
        if (!_rawTrackReplacementRegistered || !_liveWorldCapture.Enabled)
            return;
        WorldObjectContext context = WorldCaptureContext.Current;
        if (context.Kind != WorldObjectKind.Track)
            throw RawTrackFailure(
                meshPointer,
                "track mesh hook has no active track object");
        if (!IsRawTrackGuestRange(meshPointer, 0x44u))
            throw RawTrackFailure(meshPointer, "model header is outside guest RAM");

        uint vertexPointer = memory.ReadU32(meshPointer);
        uint vertexCount = memory.ReadU32(meshPointer + 0x2Cu) + 1u;
        if (vertexCount is 0 or > 512)
            throw RawTrackFailure(
                meshPointer,
                $"vertex count {vertexCount} exceeds the 9-bit index contract");
        if (!IsRawTrackGuestRange(vertexPointer, (ulong)vertexCount * 8u))
            throw RawTrackFailure(meshPointer, "vertex table is outside guest RAM");

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

            long pendingFrame = _projectedCaptureFrame + 1;
            if (_rawTrackPendingFrame != pendingFrame)
            {
                _rawTrackPendingFrame = pendingFrame;
                _rawTrackInstances.Clear();
                _rawTrackTransforms.Clear();
                _rawTrackExpectedTriangles.Clear();
                _rawTrackFrames++;
            }
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
                pendingFrame,
                in environment,
                ref objectPrimitives,
                ref objectTriangles,
                ref objectTextured);

            for (int stream = 0; stream < 8; stream++)
            {
                uint primitivePointer = memory.ReadU32(
                    meshPointer + 0x04u + checked((uint)stream * 4u));
                ushort primitiveCount = memory.ReadU16(
                    meshPointer + 0x30u + checked((uint)stream * 2u));
                int recordSize = RawTrackPrimitiveRecordSizes[stream];
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
                    bool singleSided = (packedIndices & 0x80000000u) != 0;
                    if (singleSided)
                        _rawTrackSingleSidedPrimitives++;
                    else
                        _rawTrackDoubleSidedPrimitives++;
                    indices[0] = packedIndices & 0x1FFu;
                    indices[1] = (packedIndices >> 9) & 0x1FFu;
                    indices[2] = (packedIndices >> 18) & 0x1FFu;
                    bool quad = (stream & 1) != 0;
                    indices[3] = quad
                        ? memory.ReadU32(primitive + 4u) & 0x1FFu
                        : 0u;
                    int sourceVertexCount = quad ? 4 : 3;
                    for (int index = 0; index < sourceVertexCount; index++)
                    {
                        if (indices[index] >= vertexCount)
                        {
                            throw RawTrackFailure(
                                meshPointer,
                                $"stream {stream} primitive {item} has invalid " +
                                $"vertex index {indices[index]}/{vertexCount}");
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
                    colors[1] = gouraud
                        ? memory.ReadU32(primitive + 0x0Cu)
                        : commandColor;
                    colors[2] = gouraud
                        ? memory.ReadU32(primitive + 0x10u)
                        : commandColor;
                    colors[3] = gouraud && quad
                        ? memory.ReadU32(primitive + 0x14u)
                        : commandColor;

                    packetUvs.Clear();
                    ushort clut = 0;
                    ushort texturePage = (ushort)CurTPage();
                    if (textured)
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
                        // GT2 uses the base record when projected coverage is
                        // above the threshold at +0x0C, and advances to +0x10
                        // only for the distant/small form. A resident modern
                        // course keeps the authored near material everywhere
                        // instead of reproducing PS1 distance transitions.
                        uint detailEntry = entry;
                        uint texture0 = memory.ReadU32(detailEntry);
                        uint texture1 = memory.ReadU32(detailEntry + 4u);
                        uint texture2 = memory.ReadU32(detailEntry + 8u);
                        packetUvs[0] = (ushort)texture0;
                        packetUvs[1] = (ushort)texture1;
                        packetUvs[2] = (ushort)texture2;
                        packetUvs[3] = (ushort)(texture2 >> 16);
                        clut = (ushort)(texture0 >> 16);
                        texturePage = (ushort)(texture1 >> 16);
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

                    if (!quad)
                    {
                        // Both guest paths submit source 0,2,1. Texture records
                        // are already stored in that packet order.
                        HleVertex a = RawTrackVertex(
                            in origins[indices[0]], colors[0], packetUvs[0]);
                        HleVertex b = RawTrackVertex(
                            in origins[indices[2]], colors[2], packetUvs[1]);
                        HleVertex c = RawTrackVertex(
                            in origins[indices[1]], colors[1], packetUvs[2]);
                        if (RawTrackTrianglePrimitiveAccepted(
                                singleSided,
                                projectionPath,
                                in origins[indices[0]],
                                in origins[indices[1]],
                                in origins[indices[2]]))
                        {
                            _liveWorldCapture.RecordTriangle(
                                pendingFrame,
                                in environment,
                                in a,
                                in b,
                                in c,
                                in origins[indices[0]],
                                in origins[indices[2]],
                                in origins[indices[1]],
                                in flags);
                            CatalogRawTrackTriangle(
                                in origins[indices[0]],
                                in origins[indices[2]],
                                in origins[indices[1]],
                                in a,
                                in b,
                                in c,
                                in flags,
                                primitive,
                                stream,
                                item);
                            CountRawTrackVisibility(
                                in origins[indices[0]],
                                in origins[indices[2]],
                                in origins[indices[1]],
                                in environment);
                            objectTriangles++;
                        }
                    }
                    else
                    {
                        // The two GT2 paths use opposite packet rotations but
                        // the same 1-3 authored diagonal. Preserve the chosen
                        // path because its NCLIP convention and UV-to-corner
                        // mapping are coupled.
                        int packetCorner0 = projectionPath ==
                            TrackMeshProjectionPath.Primary
                                ? 2
                                : 0;
                        const int packetCorner1 = 1;
                        const int packetCorner2 = 3;
                        int packetCorner3 = projectionPath ==
                            TrackMeshProjectionPath.Primary
                                ? 0
                                : 2;
                        uint packet0 = indices[packetCorner0];
                        uint packet1 = indices[packetCorner1];
                        uint packet2 = indices[packetCorner2];
                        uint packet3 = indices[packetCorner3];
                        HleVertex a = RawTrackVertex(
                            in origins[packet0],
                            colors[packetCorner0],
                            packetUvs[0]);
                        HleVertex b = RawTrackVertex(
                            in origins[packet1],
                            colors[packetCorner1],
                            packetUvs[1]);
                        HleVertex c = RawTrackVertex(
                            in origins[packet2],
                            colors[packetCorner2],
                            packetUvs[3]);
                        HleVertex d = RawTrackVertex(
                            in origins[packet3],
                            colors[packetCorner3],
                            packetUvs[2]);
                        if (RawTrackQuadPrimitiveAccepted(
                                singleSided,
                                projectionPath,
                                in origins[indices[0]],
                                in origins[indices[1]],
                                in origins[indices[2]],
                                in origins[indices[3]]))
                        {
                            _liveWorldCapture.RecordTriangle(
                                pendingFrame,
                                in environment,
                                in a,
                                in b,
                                in c,
                                in origins[packet0],
                                in origins[packet1],
                                in origins[packet2],
                                in flags);
                            CatalogRawTrackTriangle(
                                in origins[packet0],
                                in origins[packet1],
                                in origins[packet2],
                                in a,
                                in b,
                                in c,
                                in flags,
                                primitive,
                                stream,
                                item);
                            CountRawTrackVisibility(
                                in origins[packet0],
                                in origins[packet1],
                                in origins[packet2],
                                in environment);
                            objectTriangles++;
                            _liveWorldCapture.RecordTriangle(
                                pendingFrame,
                                in environment,
                                in b,
                                in c,
                                in d,
                                in origins[packet1],
                                in origins[packet2],
                                in origins[packet3],
                                in flags);
                            CatalogRawTrackTriangle(
                                in origins[packet1],
                                in origins[packet2],
                                in origins[packet3],
                                in b,
                                in c,
                                in d,
                                in flags,
                                primitive,
                                stream,
                                item);
                            CountRawTrackVisibility(
                                in origins[packet1],
                                in origins[packet2],
                                in origins[packet3],
                                in environment);
                            objectTriangles++;
                        }
                    }
                    objectPrimitives++;
                }
            }

            _rawTrackObjects++;
            _rawTrackSourcePrimitives += objectPrimitives;
            _rawTrackTriangles += objectTriangles;
            _rawTrackTexturedPrimitives += objectTextured;
        }
        finally
        {
            ArrayPool<GteProjectionOrigin>.Shared.Return(origins);
        }
    }

    /// <summary>
    /// GT2 stores camera-facing course foliage and signs in the auxiliary
    /// 16-byte stream at model +0x24/+0x40. The guest expands each authored
    /// record into a textured quad before calling the ordinary mesh renderer;
    /// consequently these faces are not present in any of the eight indexed
    /// primitive streams. Decode the record itself so complete-course
    /// residency does not depend on accepting those guest-generated packets.
    /// </summary>
    void CaptureRawTrackBillboards(
        uint meshPointer,
        IMemory memory,
        uint textureTableBase,
        long pendingFrame,
        in HleDrawEnv environment,
        ref long objectPrimitives,
        ref long objectTriangles,
        ref long objectTextured)
    {
        ushort primitiveCount = memory.ReadU16(meshPointer + 0x40u);
        if (primitiveCount == 0)
            return;

        uint primitivePointer = memory.ReadU32(meshPointer + 0x24u);
        if (!IsRawTrackGuestRange(
                primitivePointer,
                (ulong)primitiveCount * 0x10u))
        {
            throw RawTrackFailure(
                meshPointer,
                "billboard primitive stream is outside guest RAM");
        }
        if (!IsRawTrackGuestRange(textureTableBase, 0x20u))
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
            uint primitive = primitivePointer + checked((uint)item * 0x10u);
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
            int offsetX = RawTrackBillboardOffset(cameraAxisX, width);
            int offsetY = RawTrackBillboardOffset(cameraAxisY, width);
            short firstX = unchecked((short)(centerX - offsetX));
            short firstY = unchecked((short)(centerY + offsetY));
            short secondX = unchecked((short)(centerX + offsetX));
            short secondY = unchecked((short)(centerY - offsetY));
            short topZ = unchecked((short)(baseZ + height));

            uint entry = unchecked(textureTableBase + (textureIndex << 5));
            if (!IsRawTrackGuestRange(entry, 0x10u))
            {
                throw RawTrackFailure(
                    meshPointer,
                    $"billboard texture entry 0x{entry:X8} is outside guest RAM");
            }
            uint texture0 = memory.ReadU32(entry);
            uint texture1 = memory.ReadU32(entry + 4u);
            uint texture2 = memory.ReadU32(entry + 8u);
            ushort uv0 = (ushort)texture0;
            ushort uv1 = (ushort)texture1;
            ushort uv2 = (ushort)texture2;
            ushort uv3 = (ushort)(texture2 >> 16);
            ushort clut = (ushort)(texture0 >> 16);
            ushort texturePage = (ushort)(texture1 >> 16);

            GteProjectionOrigin origin0 = Gte.SnapshotProjectionOrigin(
                firstX, firstY, topZ);
            GteProjectionOrigin origin1 = Gte.SnapshotProjectionOrigin(
                secondX, secondY, topZ);
            GteProjectionOrigin origin2 = Gte.SnapshotProjectionOrigin(
                firstX, firstY, baseZ);
            GteProjectionOrigin origin3 = Gte.SnapshotProjectionOrigin(
                secondX, secondY, baseZ);
            uint color = _rawTrackFlatDebugRequested
                ? RawTrackDebugColor(RawTrackBillboardStream)
                : commandColor;
            HleVertex a = RawTrackVertex(in origin0, color, uv0);
            HleVertex b = RawTrackVertex(in origin1, color, uv1);
            HleVertex c = RawTrackVertex(in origin2, color, uv2);
            HleVertex d = RawTrackVertex(in origin3, color, uv3);
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

            _liveWorldCapture.RecordTriangle(
                pendingFrame,
                in environment,
                in a,
                in b,
                in c,
                in origin0,
                in origin1,
                in origin2,
                in flags);
            CatalogRawTrackTriangle(
                in origin0,
                in origin1,
                in origin2,
                in a,
                in b,
                in c,
                in flags,
                primitive,
                RawTrackBillboardStream,
                item);
            CountRawTrackVisibility(
                in origin0,
                in origin1,
                in origin2,
                in environment);
            _liveWorldCapture.RecordTriangle(
                pendingFrame,
                in environment,
                in b,
                in c,
                in d,
                in origin1,
                in origin2,
                in origin3,
                in flags);
            CatalogRawTrackTriangle(
                in origin1,
                in origin2,
                in origin3,
                in b,
                in c,
                in d,
                in flags,
                primitive,
                RawTrackBillboardStream,
                item);
            CountRawTrackVisibility(
                in origin1,
                in origin2,
                in origin3,
                in environment);

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

    void CountRawTrackVisibility(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c,
        in HleDrawEnv environment)
    {
        if (a.ViewZ < 16 || b.ViewZ < 16 || c.ViewZ < 16)
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

    static (double X, double Y) RawTrackProject(
        in GteProjectionOrigin origin,
        in HleDrawEnv environment) =>
        (
            environment.DrawOffsetX +
                origin.ProjectionOffsetX / 65536.0 +
                origin.ProjectionPlane * (double)origin.ViewX / origin.ViewZ,
            environment.DrawOffsetY +
                origin.ProjectionOffsetY / 65536.0 +
                origin.ProjectionPlane * (double)origin.ViewY / origin.ViewZ);

    bool RawTrackTrianglePrimitiveAccepted(
        bool singleSided,
        TrackMeshProjectionPath projectionPath,
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c)
    {
        double determinant = RawTrackViewDeterminant(in a, in b, in c);
        if (determinant == 0.0)
        {
            _rawTrackDegenerateTriangles++;
            return false;
        }
        // GT2 accepts a single-sided triangle when source-order NCLIP is
        // positive, then submits the reversed packet winding. The view-space
        // determinant has the same sign without depending on aspect ratio or
        // saturated PS1 screen coordinates.
        bool rejected = projectionPath == TrackMeshProjectionPath.Alternate
            ? determinant >= 0.0
            : determinant <= 0.0;
        if (!_rawTrackCullDisabled && singleSided && rejected)
        {
            _rawTrackBackfaceCulledTriangles++;
            return false;
        }
        return true;
    }

    bool RawTrackQuadPrimitiveAccepted(
        bool singleSided,
        TrackMeshProjectionPath projectionPath,
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c,
        in GteProjectionOrigin d)
    {
        // The guest loads the first RTPT as source 1,0,2, then RTPS source 3
        // leaves 0,2,3 in the GTE FIFO for the second NCLIP.
        double first = RawTrackViewDeterminant(in b, in a, in c);
        double second = RawTrackViewDeterminant(in a, in c, in d);
        if (first == 0.0 && second == 0.0)
        {
            _rawTrackDegenerateTriangles += 2;
            return false;
        }
        // The guest evaluates source triangles 1/0/2 and 0/2/3, negates the
        // first NCLIP, and rejects the single-sided quad only when both
        // resulting signs are negative. Keep that authored face contract,
        // but perform it before projection so widescreen and near-plane
        // clipping remain the native renderer's responsibility.
        bool rejected = projectionPath == TrackMeshProjectionPath.Alternate
            ? first <= 0.0 && second >= 0.0
            : first >= 0.0 && second <= 0.0;
        if (!_rawTrackCullDisabled && singleSided && rejected)
        {
            _rawTrackBackfaceCulledTriangles += 2;
            return false;
        }
        return true;
    }

    static double RawTrackViewDeterminant(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c) =>
        (double)a.ViewX *
            ((double)b.ViewY * c.ViewZ - (double)b.ViewZ * c.ViewY) -
        (double)a.ViewY *
            ((double)b.ViewX * c.ViewZ - (double)b.ViewZ * c.ViewX) +
        (double)a.ViewZ *
            ((double)b.ViewX * c.ViewY - (double)b.ViewY * c.ViewX);

    void CatalogRawTrackTriangle(
        in GteProjectionOrigin originA,
        in GteProjectionOrigin originB,
        in GteProjectionOrigin originC,
        in HleVertex a,
        in HleVertex b,
        in HleVertex c,
        in PrimFlags flags,
        uint primitiveAddress,
        int stream,
        int item)
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
            flags,
            primitiveAddress,
            stream,
            item,
            RawTrackViewDeterminant(
                in originA,
                in originB,
                in originC),
            originA.ViewZ >= 16 &&
                originB.ViewZ >= 16 &&
                originC.ViewZ >= 16));
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
            return;
        _rawTrackCorrelationGeometryMatches++;

        RawTrackExpectedTriangle expected = candidates[0];
        foreach (RawTrackExpectedTriangle candidate in candidates)
        {
            if (RawTrackUvEqual(candidate.A, a) &&
                RawTrackUvEqual(candidate.B, b) &&
                RawTrackUvEqual(candidate.C, c) &&
                RawTrackMaterialEqual(candidate.Flags, flags))
            {
                expected = candidate;
                break;
            }
        }
        if (expected.ViewDeterminant > 0.0)
        {
            _rawTrackGuestGeometryPositiveByStream[expected.Stream]++;
            if (expected.AllInFront)
                _rawTrackGuestFrontPositiveByStream[expected.Stream]++;
        }
        else if (expected.ViewDeterminant < 0.0)
        {
            _rawTrackGuestGeometryNegativeByStream[expected.Stream]++;
            if (expected.AllInFront)
                _rawTrackGuestFrontNegativeByStream[expected.Stream]++;
        }
        bool uvEqual =
            RawTrackUvEqual(expected.A, a) &&
            RawTrackUvEqual(expected.B, b) &&
            RawTrackUvEqual(expected.C, c);
        bool materialEqual = RawTrackMaterialEqual(
            expected.Flags,
            flags);
        bool colorEqual =
            RawTrackColorEqual(expected.A, a) &&
            RawTrackColorEqual(expected.B, b) &&
            RawTrackColorEqual(expected.C, c);
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
                $"uv={uvEqual} material={materialEqual} color={colorEqual} " +
                $"expectedUv={RawTrackUv(expected.A)}/" +
                $"{RawTrackUv(expected.B)}/{RawTrackUv(expected.C)} " +
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

    static RawTrackTriangleKey RawTrackKey(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c) =>
        new(
            a.Object.StableId,
            a.Object.ModelPointer,
            a.TransformId,
            a.ModelX, a.ModelY, a.ModelZ,
            b.ModelX, b.ModelY, b.ModelZ,
            c.ModelX, c.ModelY, c.ModelZ);

    static bool RawTrackUvEqual(HleVertex left, HleVertex right) =>
        left.U == right.U && left.V == right.V;

    static bool RawTrackColorEqual(HleVertex left, HleVertex right) =>
        left.R == right.R && left.G == right.G && left.B == right.B;

    static bool RawTrackMaterialEqual(
        PrimFlags left,
        PrimFlags right) =>
        left.Textured == right.Textured &&
        left.SemiTrans == right.SemiTrans &&
        left.RawTexture == right.RawTexture &&
        left.Gouraud == right.Gouraud &&
        left.TPage == right.TPage &&
        left.Clut == right.Clut;

    static string RawTrackUv(HleVertex vertex) =>
        $"{vertex.U},{vertex.V}";

    static string RawTrackRgb(HleVertex vertex) =>
        $"{vertex.R},{vertex.G},{vertex.B}";

    string RawTrackGuestGeometryWinding() => string.Join(
        ',',
        Enumerable.Range(0, RawTrackCorrelationStreamCount).Select(stream =>
            $"{stream}:" +
            $"{_rawTrackGuestGeometryPositiveByStream[stream]}/" +
            $"{_rawTrackGuestGeometryNegativeByStream[stream]}"));

    string RawTrackGuestFrontWinding() => string.Join(
        ',',
        Enumerable.Range(0, RawTrackCorrelationStreamCount).Select(stream =>
            $"{stream}:" +
            $"{_rawTrackGuestFrontPositiveByStream[stream]}/" +
            $"{_rawTrackGuestFrontNegativeByStream[stream]}"));

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

    static HleVertex RawTrackVertex(
        in GteProjectionOrigin origin,
        uint color,
        ushort packedUv)
    {
        float screenX = origin.ProjectionOffsetX / 65536.0f;
        float screenY = origin.ProjectionOffsetY / 65536.0f;
        if (origin.ViewZ > 0)
        {
            float scale = origin.ProjectionPlane /
                (float)origin.ViewZ;
            screenX += origin.ViewX * scale;
            screenY += origin.ViewY * scale;
        }
        return new HleVertex
        {
            X = screenX,
            Y = screenY,
            Z = origin.ViewZ,
            R = (byte)color,
            G = (byte)(color >> 8),
            B = (byte)(color >> 16),
            U = (byte)packedUv,
            V = (byte)(packedUv >> 8),
            HasGteZ = true,
        };
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
