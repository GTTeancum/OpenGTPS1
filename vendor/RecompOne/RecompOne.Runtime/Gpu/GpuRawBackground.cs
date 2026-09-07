using System.Buffers;
using RecompOne.Runtime.Hle;
using RecompOne.Runtime.Memory;

namespace RecompOne.Runtime;

/// <summary>
/// Resident decoder for GT2's camera-centred backdrop model. The guest walks
/// the complete model but submits only primitives which survive its original
/// 4:3 GTE screen test. Decoding at the model boundary preserves the authored
/// mesh and materials while allowing the native Hor+ frustum to perform the
/// final clipping.
/// </summary>
public sealed partial class Gpu
{
    static readonly int[] RawBackgroundRecordSizes =
        [0x30, 0x38, 0x40, 0x50, 0x48, 0x58, 0x58, 0x70];

    bool _rawBackgroundReplacementRegistered;
    uint _rawBackgroundLastGeneration;
    ulong _rawBackgroundLastTransform;
    long _rawBackgroundFrames;
    long _rawBackgroundDuplicateCalls;
    long _rawBackgroundVertices;
    long _rawBackgroundPrimitives;
    long _rawBackgroundTriangles;
    long _rawBackgroundTexturedPrimitives;
    long _rawBackgroundMaximumPrimitivesPerFrame;
    int _rawBackgroundTraceCount;

    internal bool RawBackgroundReplacementActive =>
        _rawBackgroundReplacementRegistered;

    void RegisterRawBackgroundReplacement()
    {
        if (!_liveWorldCapture.Enabled)
            throw new InvalidOperationException(
                "Raw GT2 background replacement requires the native world renderer.");
        WorldCaptureContext.RegisterBackgroundMeshConsumer(
            CaptureRawBackgroundMesh);
        _rawBackgroundReplacementRegistered = true;
        Console.Error.WriteLine(
            "[GT2-Raw-Background] enabled mode=replace " +
            "source=authored-pre-4:3-cull clipping=native-hor-plus " +
            "guestBackgroundFallback=disabled");
    }

    void UnregisterRawBackgroundReplacement()
    {
        if (!_rawBackgroundReplacementRegistered)
            return;
        WorldCaptureContext.UnregisterBackgroundMeshConsumer(
            CaptureRawBackgroundMesh);
        _rawBackgroundReplacementRegistered = false;
    }

    void ReportRawBackgroundReplacement()
    {
        WorldCaptureContext.ReportBackgroundProjectionOwnership();
        Console.Error.WriteLine(
            $"[GT2-Raw-Background-Summary] frames={_rawBackgroundFrames} " +
            $"vertices={_rawBackgroundVertices} " +
            $"primitives={_rawBackgroundPrimitives} " +
            $"triangles={_rawBackgroundTriangles} " +
            $"textured={_rawBackgroundTexturedPrimitives} " +
            $"maxPrimitivesPerFrame={_rawBackgroundMaximumPrimitivesPerFrame} " +
            $"duplicateCalls={_rawBackgroundDuplicateCalls} " +
            "decodeFailures=0 guestBackgroundFallbacks=0");
    }

    void CaptureRawBackgroundMesh(uint modelPointer, IMemory memory)
    {
        if (!_rawBackgroundReplacementRegistered ||
            !_liveWorldCapture.CanRecordDeferredStatic)
        {
            return;
        }
        WorldObjectContext context = WorldCaptureContext.Current;
        if (context.Kind != WorldObjectKind.Background ||
            context.ModelPointer != modelPointer)
        {
            throw RawBackgroundFailure(
                modelPointer,
                "background mesh hook has no matching authored owner");
        }
        if (context.ScenePass == WorldScenePass.Auxiliary)
            return;
        if (!IsRawTrackGuestRange(modelPointer, 0x20u))
            throw RawBackgroundFailure(modelPointer, "header is outside guest RAM");

        uint vertexCount = memory.ReadU32(modelPointer + 0x0Cu);
        if (vertexCount is 0 or > 4096)
        {
            throw RawBackgroundFailure(
                modelPointer,
                $"vertex count {vertexCount} violates the 12-bit index contract");
        }
        Span<ushort> streamCounts = stackalloc ushort[8];
        ulong modelSize = 0x20u + (ulong)vertexCount * 8u;
        for (int stream = 0; stream < streamCounts.Length; stream++)
        {
            streamCounts[stream] = memory.ReadU16(
                modelPointer + 0x10u + checked((uint)stream * 2u));
            modelSize += (ulong)streamCounts[stream] *
                (uint)RawBackgroundRecordSizes[stream];
        }
        if (!IsRawTrackGuestRange(modelPointer, modelSize))
            throw RawBackgroundFailure(modelPointer, "model body is outside guest RAM");

        GteProjectionOrigin[] origins =
            ArrayPool<GteProjectionOrigin>.Shared.Rent(checked((int)vertexCount));
        uint vertexPointer = modelPointer + 0x20u;
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
            ulong transform = origins[0].TransformId;
            // The asynchronous native renderer can temporarily own every
            // presentation buffer while GT2 continues authoring complete
            // scenes. Host presentation-frame identity therefore cannot
            // distinguish those scenes. GT2's main-pass generation can.
            if (_rawBackgroundLastGeneration == context.SceneGeneration &&
                _rawBackgroundLastTransform == transform)
            {
                _rawBackgroundDuplicateCalls++;
                return;
            }
            _rawBackgroundLastGeneration = context.SceneGeneration;
            _rawBackgroundLastTransform = transform;

            HleDrawEnv environment = CurEnv();
            uint primitivePointer = vertexPointer + checked(vertexCount * 8u);
            long framePrimitives = 0;
            long frameTriangles = 0;
            long frameTextured = 0;
            Span<uint> sourceIndices = stackalloc uint[4];
            Span<HleVertex> vertices = stackalloc HleVertex[4];
            Span<GteProjectionOrigin> packetOrigins =
                stackalloc GteProjectionOrigin[4];
            Span<uint> packetSourceIdentities = stackalloc uint[4];

            for (int stream = 0; stream < streamCounts.Length; stream++)
            {
            bool quad = (stream & 1) != 0;
            bool gouraud = (stream & 2) != 0;
            bool textured = stream >= 4;
            int recordSize = RawBackgroundRecordSizes[stream];
            for (int item = 0; item < streamCounts[stream]; item++)
            {
                uint primitive = primitivePointer +
                    checked((uint)item * (uint)recordSize);
                uint packed0 = memory.ReadU32(primitive);
                uint packed1 = memory.ReadU32(primitive + 4u);
                sourceIndices[0] = packed0 & 0x0FFFu;
                sourceIndices[1] = (packed0 >> 12) & 0x0FFFu;
                sourceIndices[2] = (packed1 >> 12) & 0x0FFFu;
                sourceIndices[3] = packed1 & 0x0FFFu;
                int sourceVertexCount = quad ? 4 : 3;
                for (int corner = 0; corner < sourceVertexCount; corner++)
                {
                    if (sourceIndices[corner] >= vertexCount)
                    {
                        throw RawBackgroundFailure(
                            modelPointer,
                            $"stream {stream} primitive {item} has invalid " +
                            $"vertex {sourceIndices[corner]}/{vertexCount}");
                    }
                }

                // The guest stores two complete GPU packets after the shared
                // eight-byte index header. Buffer zero is sufficient because
                // projected XY words are the only per-frame fields; command,
                // RGB, UV, CLUT, and texture-page words are authored constants.
                uint packet = primitive + 8u;
                uint commandColor = memory.ReadU32(packet + 4u);
                byte command = (byte)(commandColor >> 24);
                byte expectedFamily = (byte)(
                    0x20 |
                    (quad ? 0x08 : 0x00) |
                    (textured ? 0x04 : 0x00) |
                    (gouraud ? 0x10 : 0x00));
                if ((command & 0xFC) != expectedFamily)
                {
                    throw RawBackgroundFailure(
                        modelPointer,
                        $"stream {stream} primitive {item} has opcode " +
                        $"0x{command:X2}, expected family 0x{expectedFamily:X2}");
                }

                var flags = new PrimFlags
                {
                    Textured = textured,
                    SemiTrans = (command & 0x02) != 0,
                    RawTexture = (command & 0x01) != 0,
                    Gouraud = gouraud,
                    TPage = textured
                        ? memory.ReadU16(packet + (gouraud ? 0x1Au : 0x16u))
                        : (ushort)0,
                    Clut = textured
                        ? memory.ReadU16(packet + 0x0Eu)
                        : (ushort)0,
                    OtIndex = 0,
                };

                // GT2's quad records encode source corners A,B,C,D, then put
                // them in the PS1 polygon packet as A,B,D,C. Preserve that
                // authored perimeter before splitting the quad into triangles.
                for (int corner = 0; corner < sourceVertexCount; corner++)
                {
                    int sourceCorner = !quad || corner < 2
                        ? corner
                        : corner == 2 ? 3 : 2;
                    GteProjectionOrigin origin =
                        origins[sourceIndices[sourceCorner]];
                    uint color = memory.ReadU32(
                        packet + RawBackgroundColorOffset(
                            textured,
                            gouraud,
                            corner));
                    ushort uv = textured
                        ? memory.ReadU16(
                            packet + RawBackgroundUvOffset(gouraud, corner))
                        : (ushort)0;
                    vertices[corner] = RawTrackVertex(
                        in origin,
                        color,
                        uv,
                        environment.DrawOffsetX,
                        environment.DrawOffsetY);
                    packetOrigins[corner] = origin;
                    packetSourceIdentities[corner] =
                        vertexPointer + sourceIndices[sourceCorner] * 8u;
                }

                _liveWorldCapture.RecordTriangle(
                    pendingFrame,
                    in environment,
                    in vertices[0],
                    in vertices[1],
                    in vertices[2],
                    in packetOrigins[0],
                    in packetOrigins[1],
                    in packetOrigins[2],
                    in flags,
                    packetSourceIdentities[0],
                    packetSourceIdentities[1],
                    packetSourceIdentities[2]);
                frameTriangles++;
                if (quad)
                {
                    _liveWorldCapture.RecordTriangle(
                        pendingFrame,
                        in environment,
                        in vertices[1],
                        in vertices[2],
                        in vertices[3],
                        in packetOrigins[1],
                        in packetOrigins[2],
                        in packetOrigins[3],
                        in flags,
                        packetSourceIdentities[1],
                        packetSourceIdentities[2],
                        packetSourceIdentities[3]);
                    frameTriangles++;
                }
                framePrimitives++;
                frameTextured += textured ? 1 : 0;
            }
                primitivePointer += checked(
                    (uint)streamCounts[stream] * (uint)recordSize);
            }

            _rawBackgroundFrames++;
            _rawBackgroundVertices += vertexCount;
            _rawBackgroundPrimitives += framePrimitives;
            _rawBackgroundTriangles += frameTriangles;
            _rawBackgroundTexturedPrimitives += frameTextured;
            _rawBackgroundMaximumPrimitivesPerFrame = Math.Max(
                _rawBackgroundMaximumPrimitivesPerFrame,
                framePrimitives);
            if (_rawBackgroundTraceCount < 4 ||
                (_rawBackgroundFrames % 120) == 0)
            {
                _rawBackgroundTraceCount++;
                Console.Error.WriteLine(
                    $"[GT2-Raw-Background-Frame] frame={pendingFrame} " +
                    $"model=0x{modelPointer:X8} transform=0x{transform:X16} " +
                    $"vertices={vertexCount} primitives={framePrimitives} " +
                    $"triangles={frameTriangles} textured={frameTextured} " +
                    $"clip={environment.ClipX0},{environment.ClipY0}-" +
                    $"{environment.ClipX1},{environment.ClipY1}");
            }
        }
        finally
        {
            ArrayPool<GteProjectionOrigin>.Shared.Return(origins);
        }
    }

    static uint RawBackgroundColorOffset(
        bool textured,
        bool gouraud,
        int corner)
    {
        if (!gouraud)
            return 4u;
        return textured
            ? corner switch
            {
                0 => 0x04u,
                1 => 0x10u,
                2 => 0x1Cu,
                _ => 0x28u,
            }
            : 0x04u + checked((uint)corner * 8u);
    }

    static uint RawBackgroundUvOffset(bool gouraud, int corner) =>
        gouraud
            ? corner switch
            {
                0 => 0x0Cu,
                1 => 0x18u,
                2 => 0x24u,
                _ => 0x30u,
            }
            : 0x0Cu + checked((uint)corner * 8u);

    static InvalidDataException RawBackgroundFailure(
        uint modelPointer,
        string reason) =>
        new($"GT2 raw background decode failed for 0x{modelPointer:X8}: " +
            $"{reason}. Guest background fallback is disabled.");
}
