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
    const uint Version = 1;
    const int HeaderSize = 128;
    const int TriangleStride = 176;
    const int MaxTriangles = 262_144;

    readonly string? _outputPath;
    readonly string? _temporaryPath;
    readonly int _targetInputPoll;
    readonly Dictionary<ulong, (int Count, GteProjectionOrigin Origin)>
        _trackTransforms = [];
    readonly Dictionary<ulong, (int Count, GteProjectionOrigin Origin)>
        _allTransforms = [];

    FileStream? _stream;
    BinaryWriter? _writer;
    long _geometryFrame;
    uint _triangleCount;
    uint _skippedTriangles;
    uint _validVertices;
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

    public bool Enabled => _outputPath != null && !_completed && !_failed;
    public bool NeedsVramSnapshot => Enabled && _capturing;

    public WorldSceneCapture()
    {
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
            _validVertices += (uint)valid;

            uint primitiveFlags = 0;
            if (flags.Textured) primitiveFlags |= 1U << 0;
            if (flags.SemiTrans) primitiveFlags |= 1U << 1;
            if (flags.RawTexture) primitiveFlags |= 1U << 2;
            if (flags.Gouraud) primitiveFlags |= 1U << 3;
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
            _writer.Write(0U);
            _writer.Write(identity.TransformId);
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
            Add(_trackTransforms, in origin);
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
                $"trackTransforms={_trackTransforms.Count} transforms={_allTransforms.Count} " +
                $"cameraTransform=0x{camera.TransformId:X16} " +
                $"bytes={new FileInfo(_outputPath!).Length} " +
                $"truncated={_truncated} path={_outputPath}");
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
    }

    GteProjectionOrigin SelectCamera()
    {
        var source = _trackTransforms.Count != 0
            ? _trackTransforms
            : _allTransforms;
        return source.Count == 0
            ? default
            : source.Values.MaxBy(entry => entry.Count).Origin;
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
