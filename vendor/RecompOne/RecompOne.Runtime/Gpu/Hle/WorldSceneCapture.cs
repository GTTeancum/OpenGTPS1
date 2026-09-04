using System.Text;
using RecompOne.Runtime.Host;

namespace RecompOne.Runtime.Hle;

/// <summary>
/// Captures GT2 primitives whose packet vertices retain their original GTE
/// model/view provenance. Camera selection is the most frequently used track
/// transform, not a screen-space inference.
/// </summary>
internal sealed class WorldSceneCapture : IDisposable
{
    const uint Version = 5;
    const int HeaderSize = 160;
    const int TriangleStride = 256;
    const int MaxTriangles = 262_144;

    readonly string? _outputPath;
    readonly string? _temporaryPath;
    readonly int _targetInputPoll;
    readonly bool _traceProjectionMismatches;
    readonly record struct CameraProjectionKey(
        ulong TransformId,
        int OffsetX,
        int OffsetY,
        ushort Plane);
    readonly record struct ProjectionKey(
        int OffsetX,
        int OffsetY,
        ushort Plane,
        WorldObjectKind Kind);
    readonly record struct SourceVertexKey(
        uint ModelPointer,
        short X,
        short Y,
        short Z);
    readonly Dictionary<
        CameraProjectionKey,
        (int Count, GteProjectionOrigin Origin)> _trackCameraStates = [];
    readonly Dictionary<ulong, (int Count, GteProjectionOrigin Origin)>
        _allTransforms = [];
    readonly Dictionary<ProjectionKey, int> _projectionStates = [];
    readonly Dictionary<SourceVertexKey, uint> _syntheticSourceVertices = [];

    FileStream? _stream;
    BinaryWriter? _writer;
    long _geometryFrame;
    uint _triangleCount;
    uint _skippedTriangles;
    uint _validVertices;
    uint _sourceVertices;
    uint _trackSourceVertices;
    bool _armed;
    bool _capturing;
    bool _truncated;
    bool _completed;
    bool _failed;
    int _viewportX;
    int _viewportY;
    int _viewportWidth;
    int _viewportHeight;
    long _viewportArea;
    int _drawOffsetX;
    int _drawOffsetY;
    double _ownProjectionSquaredError;
    double _ownProjectionMaximumError;
    uint _ownProjectionSamples;
    uint _ownProjectionOverTwoPixels;
    int _projectionMismatchTraceCount;

    public bool Enabled => _outputPath != null && !_completed && !_failed;
    public bool NeedsVramSnapshot => Enabled && _capturing;

    public WorldSceneCapture()
    {
        _traceProjectionMismatches = string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_WORLD_PROJECTION_MISMATCH"),
            "1",
            StringComparison.Ordinal);
        string? configuredPath =
            Environment.GetEnvironmentVariable("RECOMPONE_WORLD_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(configuredPath))
            return;
        if (!int.TryParse(
                Environment.GetEnvironmentVariable(
                    "RECOMPONE_WORLD_CAPTURE_INPUT_POLL"),
                out _targetInputPoll) ||
            _targetInputPoll < 0)
        {
            Console.Error.WriteLine(
                "[World-Capture] input poll must be a non-negative integer");
            return;
        }
        _outputPath = Path.GetFullPath(configuredPath);
        _temporaryPath = _outputPath + ".tmp";
        _armed = _targetInputPoll == 0;
        Console.Error.WriteLine(
            $"[World-Capture] armed path={_outputPath} " +
            $"targetPoll={_targetInputPoll} maxTriangles={MaxTriangles}");
    }

    public void RecordTriangle(
        long pendingFrame,
        int inputPoll,
        in HleDrawEnv env,
        in HleVertex a,
        in HleVertex b,
        in HleVertex c,
        in GteProjectionOrigin originA,
        in GteProjectionOrigin originB,
        in GteProjectionOrigin originC,
        in PrimFlags flags)
    {
        if (!Enabled)
            return;
        if (!_armed && inputPoll >= _targetInputPoll)
        {
            _armed = true;
            Console.Error.WriteLine(
                $"[World-Capture] target reached poll={inputPoll}");
        }
        if (!_armed)
            return;

        int valid = (originA.Valid ? 1 : 0) +
            (originB.Valid ? 1 : 0) +
            (originC.Valid ? 1 : 0);
        if (valid == 0)
        {
            _skippedTriangles++;
            return;
        }
        if (!_capturing)
        {
            _capturing = true;
            _geometryFrame = pendingFrame;
            Console.Error.WriteLine(
                $"[World-Capture] geometry started frame={pendingFrame} " +
                $"poll={inputPoll}");
        }
        if (_triangleCount >= MaxTriangles)
        {
            _truncated = true;
            return;
        }

        int viewportWidth = env.ClipX1 - env.ClipX0 + 1;
        int viewportHeight = env.ClipY1 - env.ClipY0 + 1;
        long viewportArea = (long)viewportWidth * viewportHeight;
        if (viewportWidth > 0 &&
            viewportHeight > 0 &&
            viewportArea > _viewportArea)
        {
            _viewportX = env.ClipX0;
            _viewportY = env.ClipY0;
            _viewportWidth = viewportWidth;
            _viewportHeight = viewportHeight;
            _viewportArea = viewportArea;
            _drawOffsetX = env.DrawOffsetX;
            _drawOffsetY = env.DrawOffsetY;
        }

        try
        {
            EnsureOpen();
            GteProjectionOrigin identity = originA.Valid
                ? originA
                : originB.Valid ? originB : originC;
            CountTransform(in originA);
            CountTransform(in originB);
            CountTransform(in originC);
            CountProjection(in originA);
            CountProjection(in originB);
            CountProjection(in originC);
            AccumulateProjection(in a, in originA, in env);
            AccumulateProjection(in b, in originB, in env);
            AccumulateProjection(in c, in originC, in env);
            _validVertices += (uint)valid;
            CountSource(in originA);
            CountSource(in originB);
            CountSource(in originC);

            uint primitiveFlags = 0;
            if (flags.Textured) primitiveFlags |= 1U << 0;
            if (flags.SemiTrans) primitiveFlags |= 1U << 1;
            if (flags.RawTexture) primitiveFlags |= 1U << 2;
            if (flags.Gouraud) primitiveFlags |= 1U << 3;
            if (flags.ResidentCourse) primitiveFlags |= 1U << 4;
            if (flags.AuthoredTrackBillboardDepth)
                primitiveFlags |= 1U << 8;
            _writer!.Write(primitiveFlags);
            _writer.Write(flags.TPage);
            _writer.Write(flags.Clut);
            _writer.Write(flags.OtIndex);
            _writer.Write((short)env.ClipX0);
            _writer.Write((short)env.ClipY0);
            _writer.Write((short)env.ClipX1);
            _writer.Write((short)env.ClipY1);
            _writer.Write((short)env.TwMaskX);
            _writer.Write((short)env.TwMaskY);
            _writer.Write((short)env.TwOffX);
            _writer.Write((short)env.TwOffY);
            uint environmentFlags = 0;
            if (env.SetMask) environmentFlags |= 1U << 0;
            if (env.CheckMask) environmentFlags |= 1U << 1;
            if (env.Dither) environmentFlags |= 1U << 2;
            _writer.Write(environmentFlags);
            _writer.Write((uint)identity.Object.Kind);
            _writer.Write(identity.Object.StableId);
            _writer.Write(identity.Object.ModelPointer);
            _writer.Write((short)env.DrawOffsetX);
            _writer.Write((short)env.DrawOffsetY);
            _writer.Write(identity.TransformId);
            WriteTransform(in identity);
            WriteVertex(in a, in originA);
            WriteVertex(in b, in originB);
            WriteVertex(in c, in originC);
            _triangleCount++;
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
    }

    void CountTransform(in GteProjectionOrigin origin)
    {
        if (!origin.Valid)
            return;
        Add(_allTransforms, in origin);
        if (origin.Object.Kind == WorldObjectKind.Track)
        {
            var key = new CameraProjectionKey(
                origin.TransformId,
                origin.ProjectionOffsetX,
                origin.ProjectionOffsetY,
                origin.ProjectionPlane);
            _trackCameraStates.TryGetValue(key, out var entry);
            _trackCameraStates[key] = (entry.Count + 1, origin);
        }
    }

    void CountSource(in GteProjectionOrigin origin)
    {
        uint sourceIdentity = SourceIdentity(in origin);
        if (sourceIdentity == 0)
            return;
        _sourceVertices++;
        if (origin.Object.Kind == WorldObjectKind.Track)
            _trackSourceVertices++;
    }

    uint SourceIdentity(in GteProjectionOrigin origin)
    {
        if (!origin.Valid)
            return 0;
        if (
            origin.Object.Kind != WorldObjectKind.Track ||
            origin.Object.ModelPointer == 0
        )
            return 0;
        var key = new SourceVertexKey(
            origin.Object.ModelPointer,
            origin.ModelX,
            origin.ModelY,
            origin.ModelZ);
        if (_syntheticSourceVertices.TryGetValue(key, out uint identity))
            return identity;
        // 0x40000000 is outside the cached guest pointer range used by GT2.
        // The lower bits are a deterministic frame-local intern ID keyed by
        // exact model provenance, never by a float/proximity comparison.
        identity = 0x40000000u |
            checked((uint)_syntheticSourceVertices.Count + 1u);
        _syntheticSourceVertices.Add(key, identity);
        return identity;
    }

    void CountProjection(in GteProjectionOrigin origin)
    {
        if (!origin.Valid)
            return;
        var key = new ProjectionKey(
            origin.ProjectionOffsetX,
            origin.ProjectionOffsetY,
            origin.ProjectionPlane,
            origin.Object.Kind);
        _projectionStates.TryGetValue(key, out int count);
        _projectionStates[key] = count + 1;
    }

    void AccumulateProjection(
        in HleVertex vertex,
        in GteProjectionOrigin origin,
        in HleDrawEnv env)
    {
        if (!origin.Valid || origin.ProjectionPlane == 0)
            return;

        int depth = Math.Clamp(origin.ViewZ, 0, 0xFFFF);
        uint quotient = Gte.Divide(
            origin.ProjectionPlane,
            (ushort)depth);
        int ir1 = Math.Clamp(origin.ViewX, -0x8000, 0x7FFF);
        int ir2 = Math.Clamp(origin.ViewY, -0x8000, 0x7FFF);
        long projectedX =
            (long)quotient * ir1 + origin.ProjectionOffsetX;
        long projectedY =
            (long)quotient * ir2 + origin.ProjectionOffsetY;
        int screenX = Math.Clamp((int)(projectedX >> 16), -0x400, 0x3FF);
        int screenY = Math.Clamp((int)(projectedY >> 16), -0x400, 0x3FF);
        double deltaX = screenX + env.DrawOffsetX - vertex.X;
        double deltaY = screenY + env.DrawOffsetY - vertex.Y;
        double error = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        _ownProjectionSquaredError += error * error;
        _ownProjectionMaximumError =
            Math.Max(_ownProjectionMaximumError, error);
        _ownProjectionSamples++;
        if (error > 2.0)
        {
            _ownProjectionOverTwoPixels++;
            if (_traceProjectionMismatches &&
                _projectionMismatchTraceCount++ < 512)
            {
                Console.Error.WriteLine(
                    $"[World-Projection-Mismatch] " +
                    $"object={origin.Object.Kind}/{origin.Object.StableId}/" +
                    $"0x{origin.Object.ModelPointer:X8} " +
                    $"packet={vertex.X},{vertex.Y} " +
                    $"projected={screenX + env.DrawOffsetX}," +
                    $"{screenY + env.DrawOffsetY} error={error:F3} " +
                    $"model={origin.ModelX},{origin.ModelY},{origin.ModelZ} " +
                    $"view={origin.ViewX},{origin.ViewY},{origin.ViewZ} " +
                    $"projection={origin.ProjectionOffsetX}," +
                    $"{origin.ProjectionOffsetY},{origin.ProjectionPlane} " +
                    $"drawOffset={env.DrawOffsetX},{env.DrawOffsetY} " +
                    $"transform=0x{origin.TransformId:X16} " +
                    $"flags={origin.Flags}");
            }
        }
    }

    static void Add(
        Dictionary<ulong, (int Count, GteProjectionOrigin Origin)> transforms,
        in GteProjectionOrigin origin)
    {
        transforms.TryGetValue(origin.TransformId, out var entry);
        transforms[origin.TransformId] = (entry.Count + 1, origin);
    }

    public void OnPresentedFrame(
        long presentedFrame,
        int inputPoll,
        in HleDispEnv display,
        ReadOnlySpan<ushort> vram)
    {
        if (!Enabled || !_capturing || presentedFrame < _geometryFrame)
            return;
        try
        {
            EnsureOpen();
            long vramOffset = _stream!.Position;
            foreach (ushort pixel in vram)
                _writer!.Write(pixel);
            long vramSize = _stream.Position - vramOffset;
            GteProjectionOrigin camera = SelectCamera();
            WriteHeader(
                presentedFrame,
                inputPoll,
                in display,
                vramOffset,
                vramSize,
                in camera);
            _writer!.Flush();
            _stream!.Flush(true);
            _writer.Dispose();
            _writer = null;
            _stream = null;
            File.Move(_temporaryPath!, _outputPath!, true);
            _completed = true;
            Console.Error.WriteLine(
                $"[World-Capture] complete frame={presentedFrame} " +
                $"poll={inputPoll} triangles={_triangleCount} " +
                $"validVertices={_validVertices} skipped={_skippedTriangles} " +
                $"sourceVertices={_sourceVertices} " +
                $"trackSourceVertices={_trackSourceVertices} " +
                $"syntheticSourceIdentities={_syntheticSourceVertices.Count} " +
                $"trackCameraStates={_trackCameraStates.Count} transforms={_allTransforms.Count} " +
                $"cameraTransform=0x{camera.TransformId:X16} " +
                $"projection={camera.ProjectionOffsetX / 65536.0:F3}," +
                $"{camera.ProjectionOffsetY / 65536.0:F3}," +
                $"{camera.ProjectionPlane} drawOffset={_drawOffsetX},{_drawOffsetY} " +
                $"bytes={new FileInfo(_outputPath!).Length} " +
                $"truncated={_truncated} path={_outputPath}");
            double ownProjectionRms = _ownProjectionSamples == 0
                ? 0.0
                : Math.Sqrt(
                    _ownProjectionSquaredError /
                    _ownProjectionSamples);
            Console.Error.WriteLine(
                $"[World-Capture] ownProjection samples={_ownProjectionSamples} " +
                $"rms={ownProjectionRms:F6} " +
                $"max={_ownProjectionMaximumError:F6} " +
                $"over2={_ownProjectionOverTwoPixels} " +
                $"states={_projectionStates.Count}");
            foreach (var state in _projectionStates
                         .OrderByDescending(entry => entry.Value)
                         .Take(12))
            {
                Console.Error.WriteLine(
                    $"[World-Capture] projectionState count={state.Value} " +
                    $"kind={state.Key.Kind} " +
                    $"offset={state.Key.OffsetX / 65536.0:F3}," +
                    $"{state.Key.OffsetY / 65536.0:F3} " +
                    $"plane={state.Key.Plane}");
            }
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
    }

    GteProjectionOrigin SelectCamera()
    {
        GteProjectionOrigin selected = default;
        int maximumCount = int.MinValue;
        if (_trackCameraStates.Count != 0)
        {
            foreach (var candidate in _trackCameraStates.Values)
            {
                // MaxBy returns the first maximum. Retain that behavior while
                // avoiding its boxed value enumerator on every frame.
                if (candidate.Count <= maximumCount)
                    continue;
                maximumCount = candidate.Count;
                selected = candidate.Origin;
            }
        }
        else
        {
            foreach (var candidate in _allTransforms.Values)
            {
                if (candidate.Count <= maximumCount)
                    continue;
                maximumCount = candidate.Count;
                selected = candidate.Origin;
            }
        }
        return selected;
    }

    void EnsureOpen()
    {
        if (_writer != null)
            return;
        string? directory = Path.GetDirectoryName(_outputPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        if (File.Exists(_temporaryPath))
            File.Delete(_temporaryPath);
        _stream = new FileStream(
            _temporaryPath!,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.Read,
            64 * 1024,
            FileOptions.SequentialScan);
        _writer = new BinaryWriter(_stream, Encoding.UTF8, leaveOpen: false);
        _writer.Write(new byte[HeaderSize]);
    }

    void WriteVertex(
        in HleVertex vertex,
        in GteProjectionOrigin origin)
    {
        _writer!.Write(vertex.X);
        _writer.Write(vertex.Y);
        _writer.Write(vertex.Z);
        _writer.Write(vertex.U);
        _writer.Write(vertex.V);
        _writer.Write(vertex.R);
        _writer.Write(vertex.G);
        _writer.Write(vertex.B);
        _writer.Write((byte)(origin.Valid ? 1 : 0));
        _writer.Write(origin.ModelX);
        _writer.Write(origin.ModelY);
        _writer.Write(origin.ModelZ);
        _writer.Write((short)0);
        _writer.Write(origin.ViewX);
        _writer.Write(origin.ViewY);
        _writer.Write(origin.ViewZ);
        _writer.Write(origin.ProjectionOffsetX);
        _writer.Write(origin.ProjectionOffsetY);
        _writer.Write((uint)origin.ProjectionPlane);
        _writer.Write(SourceIdentity(in origin));
    }

    void WriteTransform(in GteProjectionOrigin origin)
    {
        _writer!.Write(origin.R00);
        _writer.Write(origin.R01);
        _writer.Write(origin.R02);
        _writer.Write(origin.R10);
        _writer.Write(origin.R11);
        _writer.Write(origin.R12);
        _writer.Write(origin.R20);
        _writer.Write(origin.R21);
        _writer.Write(origin.R22);
        _writer.Write((short)0);
        _writer.Write(origin.TranslateX);
        _writer.Write(origin.TranslateY);
        _writer.Write(origin.TranslateZ);
    }

    void WriteHeader(
        long frame,
        int inputPoll,
        in HleDispEnv display,
        long vramOffset,
        long vramSize,
        in GteProjectionOrigin camera)
    {
        _stream!.Position = 0;
        _writer!.Write(Encoding.ASCII.GetBytes("OGTWCAP\0"));
        _writer.Write(Version);
        _writer.Write((uint)HeaderSize);
        _writer.Write((ulong)frame);
        _writer.Write(inputPoll);
        _writer.Write(_viewportArea > 0 ? _viewportX : display.X);
        _writer.Write(_viewportArea > 0 ? _viewportY : display.Y);
        _writer.Write(_viewportArea > 0 ? _viewportWidth : display.W);
        _writer.Write(_viewportArea > 0 ? _viewportHeight : display.H);
        _writer.Write(_triangleCount);
        _writer.Write((uint)TriangleStride);
        _writer.Write((uint)VramShadow.Width);
        _writer.Write((uint)VramShadow.Height);
        _writer.Write((ulong)HeaderSize);
        _writer.Write((ulong)vramOffset);
        _writer.Write((ulong)vramSize);
        uint flags = _truncated ? 1U : 0U;
        if (display.Rgb24) flags |= 1U << 1;
        if (camera.ProjectionPlane != 0) flags |= 1U << 2;
        _writer.Write(flags);
        _writer.Write(camera.TransformId);
        _writer.Write(camera.R00);
        _writer.Write(camera.R01);
        _writer.Write(camera.R02);
        _writer.Write(camera.R10);
        _writer.Write(camera.R11);
        _writer.Write(camera.R12);
        _writer.Write(camera.R20);
        _writer.Write(camera.R21);
        _writer.Write(camera.R22);
        _writer.Write((short)0);
        _writer.Write(camera.TranslateX);
        _writer.Write(camera.TranslateY);
        _writer.Write(camera.TranslateZ);
        _writer.Write(camera.ProjectionOffsetX);
        _writer.Write(camera.ProjectionOffsetY);
        _writer.Write((uint)camera.ProjectionPlane);
        _writer.Write(_drawOffsetX);
        _writer.Write(_drawOffsetY);
        _writer.Write(0U);
        _writer.Write(0U);
        _writer.Write(0U);
        _stream.Position = vramOffset + vramSize;
    }

    void Fail(Exception exception)
    {
        _failed = true;
        Console.Error.WriteLine(
            $"[World-Capture] failed: {exception.GetType().Name}: " +
            exception.Message);
        Dispose();
    }

    public void Dispose()
    {
        _writer?.Dispose();
        _writer = null;
        _stream = null;
        if (!_completed && _temporaryPath != null)
        {
            try
            {
                if (File.Exists(_temporaryPath))
                    File.Delete(_temporaryPath);
            }
            catch
            {
            }
        }
    }
}
