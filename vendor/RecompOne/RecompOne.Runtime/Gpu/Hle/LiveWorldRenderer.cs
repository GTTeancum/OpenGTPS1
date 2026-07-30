using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using RecompOne.Runtime.Config;
using RecompOne.Runtime.Host;

namespace RecompOne.Runtime.Hle;

internal readonly record struct LiveWorldOutput(
    byte[] Pixels,
    int Width,
    int Height,
    long Frame,
    int InputPoll,
    LiveNativeStats Stats);

[StructLayout(LayoutKind.Sequential)]
internal struct LiveNativeOptions
{
    public uint StructSize;
    public uint Flags;
    public uint OutputScale;
    public uint ClearColorRgba8;
}

[StructLayout(LayoutKind.Sequential)]
internal struct LiveNativeStats
{
    public uint StructSize;
    public uint Result;
    public uint CaptureTriangles;
    public uint OutputCommands;
    public uint DrawCalls;
    public uint TransparentDrawCalls;
    public uint TopologyBoundaryGroups;
    public uint TopologyTJunctions;
    public uint TopologySplitTriangles;
    public uint TopologyCoplanarPairs;
    public uint TopologyOwnershipReorders;
    public uint OutputWidth;
    public uint OutputHeight;
    public ulong RenderMicroseconds;
    public ulong FrameIndex;
    public int InputPoll;
    public uint Reserved;
}

internal readonly record struct LiveRenderSettings(
    bool Depth,
    bool Dithering,
    bool Topology,
    bool PerspectiveCorrect,
    int OutputScale);

/// <summary>
/// Bounded producer/consumer bridge to the persistent native D3D11 renderer.
/// The emulation thread always keeps the newest complete world frame and never
/// waits for rendering; stale pending work is dropped and its fixed buffer is
/// returned to the pool.
/// </summary>
internal sealed class LiveWorldRenderer : IDisposable
{
    const int CaptureBufferCount = 3;
    const int OutputBufferCount = 3;
    internal const int MaxTriangles = 32_768;
    internal const int HeaderSize = 160;
    internal const int TriangleStride = 224;
    internal const int VramBytes = 1024 * 512 * 2;
    internal const int CaptureCapacity =
        HeaderSize + MaxTriangles * TriangleStride + VramBytes;
    internal const int LiveViewportWidth = 320;
    internal const int LiveViewportHeight = 240;
    // GT2 races normally use a 320x240 display, but race/replay transitions
    // briefly select a wider PS1 display mode. Keep the fixed output pool large
    // enough for every GT2 mode at the maximum supported 4x scale so that one
    // transition frame cannot fail the native renderer with an undersized
    // output buffer.
    internal const int MaxOutputBytes = 640 * 512 * 4 * 16;

    const uint DepthFlag = 1u << 0;
    const uint DitherFlag = 1u << 1;
    const uint TopologyFlag = 1u << 2;
    const uint PerspectiveFlag = 1u << 3;
    const uint WarpFlag = 1u << 4;

    static readonly bool Disabled =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_NATIVE_WORLD_RENDERER") == "0";
    static readonly bool ForceWarp =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_NATIVE_WORLD_WARP") == "1";
    static readonly int TraceInterval =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_NATIVE_WORLD_TRACE_INTERVAL"),
            out int interval)
            ? Math.Max(1, interval)
            : 120;
    static readonly string? DumpFirstCapturePath =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_NATIVE_WORLD_DUMP_PATH");
    static readonly int DumpCaptureInputPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_NATIVE_WORLD_DUMP_INPUT_POLL"),
            out int dumpInputPoll)
            ? Math.Max(0, dumpInputPoll)
            : 0;

    readonly ConcurrentQueue<byte[]> _capturePool = new();
    readonly ConcurrentQueue<byte[]> _outputPool = new();
    readonly object _gate = new();
    readonly AutoResetEvent _workReady = new(false);
    readonly Thread? _worker;

    byte[]? _pendingCapture;
    int _pendingSize;
    LiveRenderSettings _pendingSettings;
    LiveWorldOutput? _published;
    bool _stopping;
    bool _failed;
    long _submitted;
    long _rendered;
    long _dropped;
    long _consumed;
    long _totalRenderMicroseconds;
    bool _dumpedCapture;

    public static bool Requested =>
        OperatingSystem.IsWindows() && !Disabled;

    public bool Enabled => Requested && !_failed && !_stopping;

    public LiveWorldRenderer()
    {
        if (!Requested)
            return;
        for (int index = 0; index < CaptureBufferCount; ++index)
            _capturePool.Enqueue(new byte[CaptureCapacity]);
        for (int index = 0; index < OutputBufferCount; ++index)
            _outputPool.Enqueue(new byte[MaxOutputBytes]);
        _worker = new Thread(WorkerMain)
        {
            IsBackground = true,
            Name = "OpenGT native world renderer",
            Priority = ThreadPriority.AboveNormal,
        };
        _worker.Start();
        Console.Error.WriteLine(
            $"[Native-World] enabled buffers={CaptureBufferCount} " +
            $"maxTriangles={MaxTriangles} outputBuffers={OutputBufferCount}");
    }

    public bool TryRentCaptureBuffer(out byte[] buffer) =>
        _capturePool.TryDequeue(out buffer!);

    public void ReturnCaptureBuffer(byte[] buffer)
    {
        if (buffer.Length == CaptureCapacity)
            _capturePool.Enqueue(buffer);
    }

    public bool Submit(
        byte[] capture,
        int size,
        LiveRenderSettings settings)
    {
        if (!Enabled || size <= HeaderSize || size > capture.Length)
        {
            ReturnCaptureBuffer(capture);
            return false;
        }
        byte[]? replaced = null;
        lock (_gate)
        {
            if (_pendingCapture != null)
            {
                replaced = _pendingCapture;
                _dropped++;
            }
            _pendingCapture = capture;
            _pendingSize = size;
            _pendingSettings = settings;
            _submitted++;
        }
        if (replaced != null)
            ReturnCaptureBuffer(replaced);
        _workReady.Set();
        return true;
    }

    public bool TryTakeOutput(out LiveWorldOutput output)
    {
        lock (_gate)
        {
            if (_published is not { } ready)
            {
                output = default;
                return false;
            }
            _published = null;
            _consumed++;
            output = ready;
            return true;
        }
    }

    public void ReturnOutput(byte[] pixels)
    {
        if (pixels.Length == MaxOutputBytes)
            _outputPool.Enqueue(pixels);
    }

    void WorkerMain()
    {
        nint handle = 0;
        try
        {
            if (NativeMethods.ApiVersion() != 1)
                throw new InvalidOperationException(
                    "native renderer API version mismatch");
            handle = NativeMethods.Create();
            if (handle == 0)
                throw new InvalidOperationException(
                    "native renderer creation failed");
            while (true)
            {
                _workReady.WaitOne();
                byte[]? capture;
                int size;
                LiveRenderSettings settings;
                lock (_gate)
                {
                    if (_stopping)
                        break;
                    capture = _pendingCapture;
                    size = _pendingSize;
                    settings = _pendingSettings;
                    _pendingCapture = null;
                }
                if (capture == null)
                    continue;
                if (!_outputPool.TryDequeue(out byte[]? output))
                {
                    ReturnCaptureBuffer(capture);
                    lock (_gate)
                        _dropped++;
                    continue;
                }
                try
                {
                    if (
                        !_dumpedCapture &&
                        !string.IsNullOrWhiteSpace(
                            DumpFirstCapturePath) &&
                        BitConverter.ToInt32(capture, 24) >=
                            DumpCaptureInputPoll
                    )
                    {
                        string dumpPath = Path.GetFullPath(
                            DumpFirstCapturePath);
                        string? directory =
                            Path.GetDirectoryName(dumpPath);
                        if (!string.IsNullOrEmpty(directory))
                            Directory.CreateDirectory(directory);
                        using var dump = File.Create(dumpPath);
                        dump.Write(capture, 0, size);
                        _dumpedCapture = true;
                        Console.Error.WriteLine(
                            $"[Native-World] dumped first live " +
                            $"capture bytes={size} path={dumpPath}");
                    }
                    uint flags = 0;
                    if (settings.Depth) flags |= DepthFlag;
                    if (settings.Dithering) flags |= DitherFlag;
                    if (settings.Topology) flags |= TopologyFlag;
                    if (settings.PerspectiveCorrect)
                        flags |= PerspectiveFlag;
                    if (ForceWarp) flags |= WarpFlag;
                    var options = new LiveNativeOptions
                    {
                        StructSize =
                            (uint)Marshal.SizeOf<LiveNativeOptions>(),
                        Flags = flags,
                        OutputScale =
                            (uint)Math.Clamp(settings.OutputScale, 1, 4),
                        ClearColorRgba8 = 0xFF402820,
                    };
                    var stats = new LiveNativeStats
                    {
                        StructSize =
                            (uint)Marshal.SizeOf<LiveNativeStats>(),
                    };
                    int result = NativeMethods.Render(
                        handle,
                        capture,
                        (nuint)size,
                        output,
                        (nuint)output.Length,
                        in options,
                        ref stats);
                    if (result != 0)
                        throw new InvalidOperationException(
                            $"native render failed result={result} " +
                            $"detail={stats.Result} " +
                            $"captureDisplay=" +
                            $"{BitConverter.ToInt32(capture, 36)}x" +
                            $"{BitConverter.ToInt32(capture, 40)} " +
                            $"outputScale={options.OutputScale} " +
                            $"outputCapacity={output.Length}");
                    LiveWorldOutput? replaced;
                    lock (_gate)
                    {
                        replaced = _published;
                        _published = new LiveWorldOutput(
                            output,
                            checked((int)stats.OutputWidth),
                            checked((int)stats.OutputHeight),
                            checked((long)stats.FrameIndex),
                            stats.InputPoll,
                            stats);
                        _rendered++;
                        _totalRenderMicroseconds +=
                            checked((long)stats.RenderMicroseconds);
                    }
                    if (replaced is { } stale)
                    {
                        ReturnOutput(stale.Pixels);
                        lock (_gate)
                            _dropped++;
                    }
                    if (
                        _rendered <= 3 ||
                        (_rendered % TraceInterval) == 0
                    )
                    {
                        double averageMs =
                            _totalRenderMicroseconds /
                            1000.0 /
                            Math.Max(1, _rendered);
                        Console.Error.WriteLine(
                            $"[Native-World] frame={stats.FrameIndex} " +
                            $"poll={stats.InputPoll} " +
                            $"triangles={stats.CaptureTriangles} " +
                            $"commands={stats.OutputCommands} " +
                            $"drawCalls={stats.DrawCalls} " +
                            $"size={stats.OutputWidth}x{stats.OutputHeight} " +
                            $"renderMs={stats.RenderMicroseconds / 1000.0:F3} " +
                            $"averageMs={averageMs:F3} " +
                            $"topologyBoundary={stats.TopologyBoundaryGroups} " +
                            $"tJunctions={stats.TopologyTJunctions} " +
                            $"coplanar={stats.TopologyCoplanarPairs} " +
                            $"submitted={_submitted} rendered={_rendered} " +
                            $"consumed={_consumed} dropped={_dropped}");
                    }
                }
                finally
                {
                    ReturnCaptureBuffer(capture);
                }
            }
        }
        catch (Exception exception)
        {
            _failed = true;
            Console.Error.WriteLine(
                $"[Native-World] disabled: " +
                $"{exception.GetType().Name}: {exception.Message}");
        }
        finally
        {
            if (handle != 0)
                NativeMethods.Destroy(handle);
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_stopping)
                return;
            _stopping = true;
            if (_pendingCapture != null)
            {
                ReturnCaptureBuffer(_pendingCapture);
                _pendingCapture = null;
            }
            if (_published is { } output)
            {
                ReturnOutput(output.Pixels);
                _published = null;
            }
        }
        _workReady.Set();
        _worker?.Join(TimeSpan.FromSeconds(5));
        _workReady.Dispose();
        Console.Error.WriteLine(
            $"[Native-World] shutdown submitted={_submitted} " +
            $"rendered={_rendered} consumed={_consumed} dropped={_dropped}");
    }

    static class NativeMethods
    {
        const string Library = "opengt_live_renderer";

        [DllImport(
            Library,
            EntryPoint = "opengt_live_create",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern nint Create();

        [DllImport(
            Library,
            EntryPoint = "opengt_live_destroy",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Destroy(nint handle);

        [DllImport(
            Library,
            EntryPoint = "opengt_live_render",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Render(
            nint handle,
            byte[] capture,
            nuint captureSize,
            byte[] output,
            nuint outputCapacity,
            in LiveNativeOptions options,
            ref LiveNativeStats stats);

        [DllImport(
            Library,
            EntryPoint = "opengt_live_api_version",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint ApiVersion();
    }
}

/// <summary>
/// Allocation-bounded, frame-local OGTWCAP encoder for live rendering.
/// It uses the same exact provenance contract as diagnostic capture, but
/// publishes reusable memory buffers instead of touching the filesystem.
/// </summary>
internal sealed class LiveWorldFrameRecorder : IDisposable
{
    const int MaxDeferredScreenLineTriangles = 256;

    readonly record struct CameraProjectionKey(
        ulong TransformId,
        int OffsetX,
        int OffsetY,
        ushort Plane);
    readonly record struct SourceVertexKey(
        uint ModelPointer,
        short X,
        short Y,
        short Z);

    readonly LiveWorldRenderer _renderer;
    readonly Dictionary<
        CameraProjectionKey,
        (int Count, GteProjectionOrigin Origin)> _trackCameras = [];
    readonly Dictionary<ulong, (int Count, GteProjectionOrigin Origin)>
        _allTransforms = [];
    readonly Dictionary<SourceVertexKey, uint> _sourceVertices = [];
    readonly List<int> _screenLineRecordIndices = [];
    readonly byte[] _deferredScreenLines =
        new byte[
            MaxDeferredScreenLineTriangles *
            LiveWorldRenderer.TriangleStride];

    byte[]? _buffer;
    MemoryStream? _stream;
    BinaryWriter? _writer;
    long _geometryFrame;
    uint _triangleCount;
    uint _worldTriangleCount;
    bool _truncated;
    bool _reportedOversizeOutput;
    bool _reportedNonRaceViewport;
    int _deferredScreenLineTriangles;
    long _deferredScreenLineUpdates;
    long _deferredScreenLineReuses;
    int _viewportX;
    int _viewportY;
    int _viewportWidth;
    int _viewportHeight;
    long _viewportArea;
    int _drawOffsetX;
    int _drawOffsetY;

    public bool Enabled => _renderer.Enabled && _buffer != null;
    public bool NeedsVramSnapshot =>
        Enabled && _worldTriangleCount != 0;
    public bool LastPresentedFrameWasWorld { get; private set; }
    public GteProjectionOrigin MainProjection { get; private set; }

    public LiveWorldFrameRecorder(LiveWorldRenderer renderer)
    {
        _renderer = renderer;
        RentAndReset();
    }

    public void RecordTriangle(
        long pendingFrame,
        in HleDrawEnv env,
        in HleVertex a,
        in HleVertex b,
        in HleVertex c,
        in GteProjectionOrigin originA,
        in GteProjectionOrigin originB,
        in GteProjectionOrigin originC,
        in PrimFlags flags)
    {
        RecordTriangleCore(
            pendingFrame,
            in env,
            in a,
            in b,
            in c,
            in originA,
            in originB,
            in originC,
            in flags,
            explicitScreenSpace: false);
    }

    void RecordTriangleCore(
        long pendingFrame,
        in HleDrawEnv env,
        in HleVertex a,
        in HleVertex b,
        in HleVertex c,
        in GteProjectionOrigin originA,
        in GteProjectionOrigin originB,
        in GteProjectionOrigin originC,
        in PrimFlags flags,
        bool explicitScreenSpace)
    {
        if (!Enabled)
            return;
        int valid = (originA.Valid ? 1 : 0) +
            (originB.Valid ? 1 : 0) +
            (originC.Valid ? 1 : 0);
        if (valid == 0 && !explicitScreenSpace)
            return;
        if (_triangleCount == 0)
            _geometryFrame = pendingFrame;
        if (_triangleCount >= LiveWorldRenderer.MaxTriangles)
        {
            _truncated = true;
            return;
        }
        int viewportWidth = env.ClipX1 - env.ClipX0 + 1;
        int viewportHeight = env.ClipY1 - env.ClipY0 + 1;
        long viewportArea = (long)viewportWidth * viewportHeight;
        if (
            valid != 0 &&
            viewportWidth > 0 &&
            viewportHeight > 0 &&
            viewportArea > _viewportArea
        )
        {
            _viewportX = env.ClipX0;
            _viewportY = env.ClipY0;
            _viewportWidth = viewportWidth;
            _viewportHeight = viewportHeight;
            _viewportArea = viewportArea;
            _drawOffsetX = env.DrawOffsetX;
            _drawOffsetY = env.DrawOffsetY;
        }
        GteProjectionOrigin identity = originA.Valid
            ? originA
            : originB.Valid ? originB : originC;
        if (valid != 0)
            _worldTriangleCount++;
        CountTransform(in originA);
        CountTransform(in originB);
        CountTransform(in originC);
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
        _writer.Write((short)env.DrawOffsetX);
        _writer.Write((short)env.DrawOffsetY);
        _writer.Write(identity.TransformId);
        WriteVertex(in a, in originA);
        WriteVertex(in b, in originB);
        WriteVertex(in c, in originC);
        _triangleCount++;
    }

    public void RecordScreenTriangle(
        long pendingFrame,
        in HleDrawEnv env,
        in HleVertex a,
        in HleVertex b,
        in HleVertex c,
        in PrimFlags flags)
    {
        GteProjectionOrigin origin = default;
        RecordTriangleCore(
            pendingFrame,
            in env,
            in a,
            in b,
            in c,
            in origin,
            in origin,
            in origin,
            in flags,
            explicitScreenSpace: true);
    }

    public void RecordScreenLine(
        long pendingFrame,
        in HleDrawEnv env,
        in HleVertex start,
        in HleVertex end,
        in PrimFlags flags)
    {
        int firstTriangle = checked((int)_triangleCount);
        float x1 = start.X;
        float y1 = start.Y;
        float x2 = end.X;
        float y2 = end.Y;
        var a = start;
        var b = end;
        if (x1 == x2 && y1 == y2)
        {
            var topRight = a;
            topRight.X += 1f;
            var bottomRight = topRight;
            bottomRight.Y += 1f;
            var bottomLeft = a;
            bottomLeft.Y += 1f;
            RecordScreenTriangle(
                pendingFrame,
                in env,
                in a,
                in topRight,
                in bottomRight,
                in flags);
            RecordScreenTriangle(
                pendingFrame,
                in env,
                in bottomRight,
                in bottomLeft,
                in a,
                in flags);
            RememberScreenLineTriangles(firstTriangle);
            return;
        }

        float dx = x2 - x1;
        float dy = y2 - y1;
        float offsetX;
        float offsetY;
        if (MathF.Abs(dx) > MathF.Abs(dy))
        {
            offsetX = 0f;
            offsetY = 1f;
            if (dx > 0f)
            {
                x2 += 1f;
                b.X += 1f;
            }
            else
            {
                x1 += 1f;
                a.X += 1f;
            }
        }
        else
        {
            offsetX = 1f;
            offsetY = 0f;
            if (dy > 0f)
            {
                y2 += 1f;
                b.Y += 1f;
            }
            else
            {
                y1 += 1f;
                a.Y += 1f;
            }
        }
        var bOffset = b;
        bOffset.X = x2 + offsetX;
        bOffset.Y = y2 + offsetY;
        var aOffset = a;
        aOffset.X = x1 + offsetX;
        aOffset.Y = y1 + offsetY;
        RecordScreenTriangle(
            pendingFrame,
            in env,
            in a,
            in b,
            in bOffset,
            in flags);
        RecordScreenTriangle(
            pendingFrame,
            in env,
            in bOffset,
            in aOffset,
            in a,
            in flags);
        RememberScreenLineTriangles(firstTriangle);
    }

    public void OnPresentedFrame(
        long presentedFrame,
        int inputPoll,
        in HleDispEnv display,
        ReadOnlySpan<ushort> vram)
    {
        LastPresentedFrameWasWorld = false;
        if (!Enabled || presentedFrame < _geometryFrame)
        {
            ResetCurrent();
            return;
        }
        if (_worldTriangleCount == 0)
        {
            CacheCurrentScreenLines();
            ResetCurrent();
            return;
        }
        if (_truncated)
        {
            Console.Error.WriteLine(
                $"[Native-World] dropped truncated frame={presentedFrame} " +
                $"triangles={_triangleCount}");
            ResetCurrent();
            return;
        }
        if (_screenLineRecordIndices.Count != 0)
            CacheCurrentScreenLines();
        else
            AppendDeferredScreenLines();
        long vramOffset = _stream!.Position;
        int vramBytes = vram.Length * sizeof(ushort);
        if (
            vramBytes != LiveWorldRenderer.VramBytes ||
            vramOffset + vramBytes > _buffer!.Length
        )
        {
            ResetCurrent();
            return;
        }
        long captureLength = vramOffset + vramBytes;
        // Grow the MemoryStream before copying through the backing array.
        // SetLength zero-fills newly exposed bytes, so growing it after this
        // copy would erase the complete VRAM snapshot submitted to native.
        _stream.SetLength(captureLength);
        MemoryMarshal.AsBytes(vram).CopyTo(
            _buffer.AsSpan(checked((int)vramOffset), vramBytes));
        _stream.Position = captureLength;
        GteProjectionOrigin camera = SelectCamera();
        if (!camera.Valid || camera.ProjectionPlane == 0)
        {
            ResetCurrent();
            return;
        }
        MainProjection = camera;
        var settings = new LiveRenderSettings(
            Depth: true,
            Dithering: ConfigManager.View.Ps1Dithering,
            Topology: ConfigManager.View.StabilizeGeometrySeams,
            PerspectiveCorrect:
                ConfigManager.View.PerspectiveCorrectTextures,
            OutputScale:
                ConfigManager.View.HighResolution3D ? 4 : 1);
        int outputWidth =
            _viewportArea > 0 ? _viewportWidth : display.W;
        int outputHeight =
            _viewportArea > 0 ? _viewportHeight : display.H;
        // The native path is deliberately a race/replay renderer. GT2 changes
        // to larger draw areas for transitions and the rotating Results car;
        // those frames contain UI composition that is not a native-world
        // surface. Return them to the complete PS1 compositor instead of
        // replacing its presentation with the partial 3D capture.
        if (
            outputWidth != LiveWorldRenderer.LiveViewportWidth ||
            outputHeight != LiveWorldRenderer.LiveViewportHeight
        )
        {
            if (!_reportedNonRaceViewport)
            {
                _reportedNonRaceViewport = true;
                Console.Error.WriteLine(
                    $"[Native-World] compositor fallback non-race " +
                    $"viewport={outputWidth}x{outputHeight}");
            }
            ResetCurrent();
            return;
        }
        _reportedNonRaceViewport = false;
        long requiredOutputBytes =
            (long)outputWidth * settings.OutputScale *
            outputHeight * settings.OutputScale * 4;
        if (
            outputWidth <= 0 ||
            outputHeight <= 0 ||
            requiredOutputBytes > LiveWorldRenderer.MaxOutputBytes
        )
        {
            if (!_reportedOversizeOutput)
            {
                _reportedOversizeOutput = true;
                Console.Error.WriteLine(
                    $"[Native-World] compositor fallback " +
                    $"viewport={outputWidth}x{outputHeight} " +
                    $"scale={settings.OutputScale} " +
                    $"requiredBytes={requiredOutputBytes} " +
                    $"capacity={LiveWorldRenderer.MaxOutputBytes}");
            }
            ResetCurrent();
            return;
        }
        _reportedOversizeOutput = false;
        WriteHeader(
            presentedFrame,
            inputPoll,
            in display,
            vramOffset,
            vramBytes,
            in camera);
        _writer!.Flush();
        int size = checked((int)(vramOffset + vramBytes));
        byte[] submitted = _buffer;
        _writer.Dispose();
        _stream.Dispose();
        _writer = null;
        _stream = null;
        _buffer = null;
        LastPresentedFrameWasWorld =
            _renderer.Submit(submitted, size, settings);
        RentAndReset();
    }

    void RememberScreenLineTriangles(int firstTriangle)
    {
        int lastTriangle = checked((int)_triangleCount);
        for (
            int triangle = firstTriangle;
            triangle < lastTriangle;
            ++triangle
        )
            _screenLineRecordIndices.Add(triangle);
    }

    void CacheCurrentScreenLines()
    {
        if (_screenLineRecordIndices.Count == 0)
            return;
        _writer!.Flush();
        int count = Math.Min(
            _screenLineRecordIndices.Count,
            MaxDeferredScreenLineTriangles);
        for (int destination = 0; destination < count; ++destination)
        {
            int source = _screenLineRecordIndices[destination];
            int sourceOffset =
                LiveWorldRenderer.HeaderSize +
                source * LiveWorldRenderer.TriangleStride;
            int destinationOffset =
                destination * LiveWorldRenderer.TriangleStride;
            _buffer!.AsSpan(
                sourceOffset,
                LiveWorldRenderer.TriangleStride).CopyTo(
                    _deferredScreenLines.AsSpan(
                        destinationOffset,
                        LiveWorldRenderer.TriangleStride));
        }
        _deferredScreenLineTriangles = count;
        _deferredScreenLineUpdates++;
    }

    void AppendDeferredScreenLines()
    {
        if (_deferredScreenLineTriangles == 0)
            return;
        int availableTriangles =
            LiveWorldRenderer.MaxTriangles -
            checked((int)_triangleCount);
        int count = Math.Min(
            _deferredScreenLineTriangles,
            availableTriangles);
        if (count <= 0)
            return;
        int bytes = count * LiveWorldRenderer.TriangleStride;
        long offset = _stream!.Position;
        long length = offset + bytes;
        if (length > _buffer!.Length)
            return;
        // As with the VRAM snapshot, grow before copying into the public
        // backing array so MemoryStream cannot zero the appended records.
        _stream.SetLength(length);
        _deferredScreenLines.AsSpan(0, bytes).CopyTo(
            _buffer.AsSpan(checked((int)offset), bytes));
        _stream.Position = length;
        _triangleCount += checked((uint)count);
        _deferredScreenLineReuses++;
        if (_deferredScreenLineReuses <= 3)
        {
            Console.Error.WriteLine(
                $"[Native-World] reused deferred HUD line layer " +
                $"triangles={count} updates={_deferredScreenLineUpdates} " +
                $"reuses={_deferredScreenLineReuses}");
        }
    }

    void CountTransform(in GteProjectionOrigin origin)
    {
        if (!origin.Valid)
            return;
        _allTransforms.TryGetValue(origin.TransformId, out var entry);
        _allTransforms[origin.TransformId] =
            (entry.Count + 1, origin);
        if (origin.Object.Kind == WorldObjectKind.Track)
        {
            var key = new CameraProjectionKey(
                origin.TransformId,
                origin.ProjectionOffsetX,
                origin.ProjectionOffsetY,
                origin.ProjectionPlane);
            _trackCameras.TryGetValue(key, out var camera);
            _trackCameras[key] = (camera.Count + 1, origin);
        }
    }

    uint SourceIdentity(in GteProjectionOrigin origin)
    {
        if (
            !origin.Valid ||
            origin.Object.Kind != WorldObjectKind.Track ||
            origin.Object.ModelPointer == 0
        )
            return 0;
        var key = new SourceVertexKey(
            origin.Object.ModelPointer,
            origin.ModelX,
            origin.ModelY,
            origin.ModelZ);
        if (_sourceVertices.TryGetValue(key, out uint identity))
            return identity;
        identity = 0x40000000u |
            checked((uint)_sourceVertices.Count + 1u);
        _sourceVertices.Add(key, identity);
        return identity;
    }

    GteProjectionOrigin SelectCamera()
    {
        if (_trackCameras.Count != 0)
            return _trackCameras.Values
                .MaxBy(entry => entry.Count).Origin;
        return _allTransforms.Count == 0
            ? default
            : _allTransforms.Values
                .MaxBy(entry => entry.Count).Origin;
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
        _writer.Write(4U);
        _writer.Write((uint)LiveWorldRenderer.HeaderSize);
        _writer.Write((ulong)frame);
        _writer.Write(inputPoll);
        _writer.Write(_viewportArea > 0 ? _viewportX : display.X);
        _writer.Write(_viewportArea > 0 ? _viewportY : display.Y);
        _writer.Write(
            _viewportArea > 0 ? _viewportWidth : display.W);
        _writer.Write(
            _viewportArea > 0 ? _viewportHeight : display.H);
        _writer.Write(_triangleCount);
        _writer.Write((uint)LiveWorldRenderer.TriangleStride);
        _writer.Write(1024U);
        _writer.Write(512U);
        _writer.Write((ulong)LiveWorldRenderer.HeaderSize);
        _writer.Write((ulong)vramOffset);
        _writer.Write((ulong)vramSize);
        uint flags = 1U << 2;
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

    void RentAndReset()
    {
        if (!_renderer.TryRentCaptureBuffer(out byte[] buffer))
            return;
        _buffer = buffer;
        _stream = new MemoryStream(
            buffer, 0, buffer.Length, writable: true,
            publiclyVisible: true);
        _stream.SetLength(LiveWorldRenderer.HeaderSize);
        _stream.Position = LiveWorldRenderer.HeaderSize;
        _writer = new BinaryWriter(
            _stream, Encoding.UTF8, leaveOpen: true);
        ClearFrameState();
    }

    void ResetCurrent()
    {
        if (_stream == null)
        {
            RentAndReset();
            return;
        }
        Array.Clear(_buffer!, 0, LiveWorldRenderer.HeaderSize);
        _stream.SetLength(LiveWorldRenderer.HeaderSize);
        _stream.Position = LiveWorldRenderer.HeaderSize;
        ClearFrameState();
    }

    void ClearFrameState()
    {
        _trackCameras.Clear();
        _allTransforms.Clear();
        _sourceVertices.Clear();
        _screenLineRecordIndices.Clear();
        _geometryFrame = 0;
        _triangleCount = 0;
        _worldTriangleCount = 0;
        _truncated = false;
        _viewportX = _viewportY = 0;
        _viewportWidth = _viewportHeight = 0;
        _viewportArea = 0;
        _drawOffsetX = _drawOffsetY = 0;
    }

    public void Dispose()
    {
        _writer?.Dispose();
        _stream?.Dispose();
        if (_buffer != null)
            _renderer.ReturnCaptureBuffer(_buffer);
        _buffer = null;
        _writer = null;
        _stream = null;
    }
}
