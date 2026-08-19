using System.Collections.Concurrent;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
    public ulong DecodeMicroseconds;
    public ulong DrawListMicroseconds;
    public ulong TopologyMicroseconds;
    public ulong PipelineMicroseconds;
}

[StructLayout(LayoutKind.Sequential)]
internal struct LiveInterpolationStats
{
    public uint StructSize;
    public uint Result;
    public uint OutputCount;
    public uint PreviousCommands;
    public uint CurrentCommands;
    public uint EligibleWorldCommands;
    public uint MatchedCommands;
    public uint MatchedTrackCommands;
    public uint MatchedVehicleCommands;
    public uint HeldScreenCommands;
    public uint HeldUnmatchedCommands;
    public uint PreviousTransformGroups;
    public uint CurrentTransformGroups;
    public uint MatchedTransformGroups;
    public uint TemporalReset;
    public uint Reserved;
    public uint ExactRigidTransformGroups;
    public uint IncoherentExactTransformGroups;
    public uint HeldIncoherentVehicleCommands;
    public uint HeldTrackVisibilityCommands;
    public uint HeldUnsafeTrackCommands;
    public ulong InterpolationMicroseconds;
    public ulong PairPipelineMicroseconds;
    public ulong MidpointRenderMicroseconds;
    public ulong ActualRenderMicroseconds;
    public ulong CurrentTopologyMicroseconds;
}

internal readonly record struct LiveRenderSettings(
    bool Depth,
    bool Dithering,
    bool Topology,
    bool PerspectiveCorrect,
    bool TextureSmoothing,
    int OutputScale);

/// <summary>
/// Bounded producer/consumer bridge to the persistent native D3D11 renderer.
/// The emulation thread never waits for rendering. A two-capture FIFO absorbs
/// one short renderer/scheduler overrun without destroying an authored state;
/// only work older than that bounded window is dropped and returned to the
/// fixed pool.
/// </summary>
internal sealed class LiveWorldRenderer : IDisposable
{
    const int CaptureBufferCount = 3;
    const int PendingCaptureCount = CaptureBufferCount - 1;
    internal const int OutputBufferCount = 11;
    // Eight completed outputs, two buffers owned by RenderPair, and one briefly
    // owned by the host upload path must coexist without starving the worker.
    internal const int PublishedOutputCapacity = 8;
    // Presentation spends this only when the chronological output queue is
    // empty. It is returned by the later vblank throttle in the normal case;
    // Ten milliseconds catches imminent native completions without letting a
    // scheduler oversleep consume most of the next NTSC presentation interval.
    internal const int OutputReadyWaitMilliseconds = 8;
    internal const int MaxTriangles = 32_768;
    internal const int HeaderSize = 160;
    internal const int TriangleStride = 384;
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
    const uint TextureSmoothingFlag = 1u << 5;
    const uint RealtimeReadbackFlag = 1u << 6;

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
    static readonly int DumpCaptureCount =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_NATIVE_WORLD_DUMP_COUNT"),
            out int dumpCaptureCount)
            ? Math.Clamp(dumpCaptureCount, 1, 64)
            : 1;
    static readonly int DumpCaptureInterval =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_NATIVE_WORLD_DUMP_INTERVAL"),
            out int dumpCaptureInterval)
            ? Math.Clamp(dumpCaptureInterval, 1, 3_600)
            : 1;

    readonly ConcurrentQueue<byte[]> _capturePool = new();
    readonly ConcurrentQueue<byte[]> _outputPool = new();
    readonly object _gate = new();
    readonly AutoResetEvent _workReady = new(false);
    readonly ManualResetEventSlim _firstOutputReady = new(false);
    readonly Thread? _worker;

    readonly Queue<PendingCapture> _pendingCaptures = new();
    readonly Queue<LiveWorldOutput> _published = new();
    bool _stopping;
    bool _failed;
    long _submitted;
    long _rendered;
    long _renderedActual;
    long _renderedSynthetic;
    long _renderedRepeated;
    long _syntheticAttempts;
    long _syntheticNoOutput;
    long _dropped;
    long _droppedPendingCaptures;
    long _droppedOutputPool;
    long _droppedPublished;
    long _consumed;
    long _outputWaitCount;
    long _outputWaitTimeouts;
    long _outputWaitPendingTails;
    long _outputWaitTicks;
    long _outputWaitMaxTicks;
    int _activeCaptureInputPoll = -1;
    long _activePairStartTicks;
    long _totalRenderMicroseconds;
    long _totalPipelineMicroseconds;
    long _pairOperations;
    int _dumpEligibleCaptures;
    // Keep the three components aligned so each percentile window describes
    // the same authored-state pairs. Submit is CPU time inside the two D3D11
    // render calls; the asynchronous live path does not wait for GPU
    // completion. Topology is paid once while building the current authored
    // state; pipeline includes both plus decode, interpolation, draw-list
    // construction, and bridge overhead.
    readonly ulong[] _recentPipelineMicroseconds = new ulong[240];
    readonly ulong[] _recentSubmitMicroseconds = new ulong[240];
    readonly ulong[] _recentTopologyMicroseconds = new ulong[240];
    int _recentProfileCount;
    int _recentProfileCursor;
    int _dumpedCaptureCount;

    readonly record struct PendingCapture(
        byte[] Buffer,
        int Size,
        LiveRenderSettings Settings);

    public static bool Requested => OperatingSystem.IsWindows();

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
            // GT2 authors 30 complete world states per second. Native emits a
            // provenance-matched geometric midpoint followed by its authored
            // endpoint, yielding two unique states per authoring interval
            // without blending two complete car silhouettes.
            // Native pair production is presentation-critical once real-time
            // pacing begins. Keep it at the same priority class as the paced
            // emulation thread; the bounded pending/output queues still
            // prevent it from running ahead and turning a full output queue
            // into slow game time.
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
        byte[]? discarded = null;
        bool seedFirstOutput;
        lock (_gate)
        {
            seedFirstOutput = _submitted == 0;
            if (_pendingCaptures.Count >= PendingCaptureCount)
            {
                discarded = _pendingCaptures.Dequeue().Buffer;
                _dropped++;
                _droppedPendingCaptures++;
            }
            _pendingCaptures.Enqueue(new PendingCapture(
                capture,
                size,
                settings));
            _submitted++;
        }
        if (discarded != null)
            ReturnCaptureBuffer(discarded);
        _workReady.Set();
        if (seedFirstOutput)
        {
            // Ownership is asserted after LiveWorldFrameRecorder.Submit
            // returns. Seed the very first native texture before that point,
            // otherwise the independently-rendering host can present several
            // blank vblanks while the first 10-20 ms D3D call initializes.
            // This bounded barrier runs once per process; every subsequent
            // authored state and temporal reset remains asynchronous.
            if (!_firstOutputReady.Wait(TimeSpan.FromMilliseconds(100)))
                Console.Error.WriteLine(
                    "[Native-World] first-output seed exceeded 100 ms");
            else
                Console.Error.WriteLine(
                    "[Native-World] first-output seed ready");
        }
        return true;
    }

    public bool TryTakeOutput(
        out LiveWorldOutput output,
        int waitMilliseconds = 0)
    {
        lock (_gate)
        {
            if (
                _published.Count == 0 &&
                waitMilliseconds > 0 &&
                !_stopping &&
                !_failed
            )
            {
                // Presentation runs before FrameClock consumes the remaining
                // NTSC vblank budget. Spend only that bounded idle margin here
                // when a dense native pair finishes a few milliseconds late;
                // this preserves a unique midpoint instead of duplicating the
                // prior 30 Hz image. Monitor.PulseAll in PublishOutput wakes the
                // host immediately, normally reducing the later throttle wait
                // by the same amount rather than extending the frame.
                long waitStart = Stopwatch.GetTimestamp();
                long waitDeadline = waitStart +
                    waitMilliseconds * Stopwatch.Frequency / 1000;
                _outputWaitCount++;
                while (
                    _published.Count == 0 &&
                    !_stopping &&
                    !_failed)
                {
                    long now = Stopwatch.GetTimestamp();
                    long remainingTicks = waitDeadline - now;
                    if (remainingTicks <= 0)
                        break;
                    int remainingMilliseconds = Math.Max(
                        1,
                        (int)Math.Ceiling(
                            remainingTicks * 1000.0 /
                            Stopwatch.Frequency));
                    Monitor.Wait(_gate, remainingMilliseconds);
                }
                long waitTicks = Stopwatch.GetTimestamp() - waitStart;
                _outputWaitTicks += waitTicks;
                _outputWaitMaxTicks = Math.Max(
                    _outputWaitMaxTicks,
                    waitTicks);
                if (_published.Count == 0)
                {
                    int activePoll = Volatile.Read(
                        ref _activeCaptureInputPoll);
                    long activeStart = Volatile.Read(
                        ref _activePairStartTicks);
                    double activeMilliseconds = activeStart == 0
                        ? 0.0
                        : (Stopwatch.GetTimestamp() - activeStart) *
                            1000.0 / Stopwatch.Frequency;
                    bool producerTail =
                        activeStart != 0 || _pendingCaptures.Count != 0;
                    if (producerTail)
                        _outputWaitPendingTails++;
                    else
                        _outputWaitTimeouts++;
                    Console.Error.WriteLine(
                        $"[Native-Output-{(producerTail ? "Pending" : "Timeout")}] " +
                        $"poll={InputManager.CurrentPoll} " +
                        $"activeCapturePoll={activePoll} " +
                        $"activeMs={activeMilliseconds:F3} " +
                        $"pending={_pendingCaptures.Count} " +
                        $"submitted={_submitted} rendered={_rendered} " +
                        $"consumed={_consumed} dropped={_dropped}");
                }
            }
            if (_published.Count == 0)
            {
                output = default;
                return false;
            }
            _consumed++;
            output = _published.Dequeue();
            return true;
        }
    }

    public bool HasPendingOrActiveWork
    {
        get
        {
            if (Volatile.Read(ref _activePairStartTicks) != 0)
                return true;
            lock (_gate)
                return _pendingCaptures.Count != 0;
        }
    }

    public int PublishedOutputCount
    {
        get
        {
            lock (_gate)
                return _published.Count;
        }
    }

    public void DiscardPublishedOutputs()
    {
        lock (_gate)
        {
            while (_published.Count != 0)
            {
                ReturnOutput(_published.Dequeue().Pixels);
                _dropped++;
                _droppedPublished++;
            }
        }
    }

    public void DiscardPublishedOutputsBefore(int minimumInputPoll)
    {
        List<byte[]> discarded = [];
        lock (_gate)
        {
            int count = _published.Count;
            for (int index = 0; index < count; ++index)
            {
                LiveWorldOutput output = _published.Dequeue();
                if (output.InputPoll < minimumInputPoll)
                {
                    discarded.Add(output.Pixels);
                    _dropped++;
                    _droppedPublished++;
                }
                else
                    _published.Enqueue(output);
            }
        }
        foreach (byte[] pixels in discarded)
            ReturnOutput(pixels);
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
            if (NativeMethods.ApiVersion() != 4)
                throw new InvalidOperationException(
                    "native renderer API version mismatch");
            handle = NativeMethods.Create();
            if (handle == 0)
                throw new InvalidOperationException(
                    "native renderer creation failed");
            while (true)
            {
                _workReady.WaitOne();
                PendingCapture pending;
                lock (_gate)
                {
                    if (_stopping)
                        break;
                    if (_pendingCaptures.Count == 0)
                        continue;
                    pending = _pendingCaptures.Dequeue();
                }
                byte[] capture = pending.Buffer;
                int size = pending.Size;
                LiveRenderSettings settings = pending.Settings;
                if (!_outputPool.TryDequeue(out byte[]? firstOutput))
                {
                    ReturnCaptureBuffer(capture);
                    lock (_gate)
                    {
                        _dropped++;
                        _droppedOutputPool++;
                    }
                    continue;
                }
                if (!_outputPool.TryDequeue(out byte[]? secondOutput))
                {
                    ReturnOutput(firstOutput);
                    ReturnCaptureBuffer(capture);
                    lock (_gate)
                    {
                        _dropped++;
                        _droppedOutputPool++;
                    }
                    continue;
                }
                bool firstPublished = false;
                bool secondPublished = false;
                try
                {
                    int captureInputPoll =
                        BitConverter.ToInt32(capture, 24);
                    bool dumpEligible =
                        _dumpedCaptureCount < DumpCaptureCount &&
                        !string.IsNullOrWhiteSpace(
                            DumpFirstCapturePath) &&
                        captureInputPoll >= DumpCaptureInputPoll;
                    bool dumpSelected = dumpEligible &&
                        (_dumpEligibleCaptures++ % DumpCaptureInterval) == 0;
                    if (dumpSelected)
                    {
                        string basePath = Path.GetFullPath(
                            DumpFirstCapturePath!);
                        string dumpPath = DumpCaptureCount == 1
                            ? basePath
                            : Path.Combine(
                                Path.GetDirectoryName(basePath) ?? "",
                                $"{Path.GetFileNameWithoutExtension(basePath)}-" +
                                $"{_dumpedCaptureCount + 1}" +
                                Path.GetExtension(basePath));
                        string? directory =
                            Path.GetDirectoryName(dumpPath);
                        if (!string.IsNullOrEmpty(directory))
                            Directory.CreateDirectory(directory);
                        using var dump = File.Create(dumpPath);
                        dump.Write(capture, 0, size);
                        _dumpedCaptureCount++;
                        Console.Error.WriteLine(
                            $"[Native-World] dumped first live " +
                            $"capture bytes={size} poll={captureInputPoll} " +
                            $"interval={DumpCaptureInterval} path={dumpPath}");
                    }
                    uint flags = 0;
                    if (settings.Depth) flags |= DepthFlag;
                    if (settings.Dithering) flags |= DitherFlag;
                    if (settings.Topology) flags |= TopologyFlag;
                    if (settings.PerspectiveCorrect)
                        flags |= PerspectiveFlag;
                    if (ForceWarp) flags |= WarpFlag;
                    if (settings.TextureSmoothing)
                        flags |= TextureSmoothingFlag;
                    if (FrameClock.RealTimeThrottleActive)
                        flags |= RealtimeReadbackFlag;
                    var options = new LiveNativeOptions
                    {
                        StructSize =
                            (uint)Marshal.SizeOf<LiveNativeOptions>(),
                        Flags = flags,
                        OutputScale =
                            (uint)Math.Clamp(settings.OutputScale, 1, 4),
                        ClearColorRgba8 = 0xFF402820,
                    };
                    var firstStats = new LiveNativeStats
                    {
                        StructSize =
                            (uint)Marshal.SizeOf<LiveNativeStats>(),
                    };
                    var secondStats = new LiveNativeStats
                    {
                        StructSize =
                            (uint)Marshal.SizeOf<LiveNativeStats>(),
                    };
                    var interpolation = new LiveInterpolationStats
                    {
                        StructSize =
                            (uint)Marshal.SizeOf<LiveInterpolationStats>(),
                    };
                    Volatile.Write(
                        ref _activeCaptureInputPoll,
                        captureInputPoll);
                    Volatile.Write(
                        ref _activePairStartTicks,
                        Stopwatch.GetTimestamp());
                    int result = NativeMethods.RenderPair(
                        handle,
                        capture,
                        (nuint)size,
                        firstOutput,
                        secondOutput,
                        (nuint)firstOutput.Length,
                        in options,
                        ref firstStats,
                        ref secondStats,
                        ref interpolation);
                    if (result != 0)
                        throw new InvalidOperationException(
                            $"native render failed result={result} " +
                            $"detail={interpolation.Result} " +
                            $"captureDisplay=" +
                            $"{BitConverter.ToInt32(capture, 36)}x" +
                            $"{BitConverter.ToInt32(capture, 40)} " +
                            $"outputScale={options.OutputScale} " +
                            $"outputCapacity={firstOutput.Length}");
                    if (interpolation.OutputCount > 2)
                        throw new InvalidOperationException(
                            $"native render returned invalid output count " +
                            $"{interpolation.OutputCount}");
                    if (interpolation.OutputCount == 0)
                    {
                        int lateRead = NativeMethods.TryReadPair(
                            handle,
                            firstOutput,
                            secondOutput,
                            (nuint)firstOutput.Length,
                            ref firstStats,
                            ref secondStats);
                        if (lateRead < 0)
                            throw new InvalidOperationException(
                                $"native readback drain failed " +
                                $"result={lateRead} " +
                                $"detail={firstStats.Result}");
                        if (lateRead == 1)
                            interpolation.OutputCount = 2;
                    }
                    _syntheticAttempts++;
                    if (interpolation.OutputCount >= 1)
                    {
                        PublishRenderedOutput(firstOutput, in firstStats);
                        firstPublished = true;
                    }
                    if (interpolation.OutputCount >= 2)
                    {
                        PublishRenderedOutput(secondOutput, in secondStats);
                        secondPublished = true;
                    }
                    DrainCompletedPairs(handle);
                    if (interpolation.OutputCount < 2)
                        _syntheticNoOutput++;
                    _pairOperations++;
                    _totalRenderMicroseconds +=
                        checked((long)(
                            interpolation.MidpointRenderMicroseconds +
                            interpolation.ActualRenderMicroseconds));
                    _totalPipelineMicroseconds +=
                        checked((long)interpolation.PairPipelineMicroseconds);
                    // Component percentiles deliberately describe track-world
                    // work only. Menus, showroom, and Results are valid native
                    // 2D/3D transitions but would dilute topology to zero and
                    // make the shutdown profile misleading.
                    if ((interpolation.Reserved & 2u) != 0)
                    {
                        RecordProfileDurations(
                            interpolation.PairPipelineMicroseconds,
                            checked(
                                interpolation.MidpointRenderMicroseconds +
                                interpolation.ActualRenderMicroseconds),
                            interpolation.CurrentTopologyMicroseconds);
                    }
                    if (
                        _pairOperations <= 3 ||
                        interpolation.TemporalReset != 0 ||
                        (_pairOperations % TraceInterval) == 0
                    )
                    {
                        double averageMs =
                            _totalRenderMicroseconds /
                            1000.0 /
                            Math.Max(1, _pairOperations);
                        double pipelineAverageMs =
                            _totalPipelineMicroseconds /
                            1000.0 /
                            Math.Max(1, _pairOperations);
                        var pipelinePercentiles = GetPercentiles(
                            _recentPipelineMicroseconds);
                        var submitPercentiles = GetPercentiles(
                            _recentSubmitMicroseconds);
                        var topologyPercentiles = GetPercentiles(
                            _recentTopologyMicroseconds);
                        LiveNativeStats traceStats =
                            interpolation.OutputCount != 0 ||
                            interpolation.TemporalReset != 0
                                ? firstStats
                                : secondStats;
                        int completionAge = Math.Max(
                            0, InputManager.CurrentPoll -
                                traceStats.InputPoll);
                        double matchRate = CommandMatchRate(
                            interpolation.MatchedCommands,
                            interpolation.PreviousCommands);
                        Console.Error.WriteLine(
                            $"[Native-World] frame={traceStats.FrameIndex} " +
                            $"poll={traceStats.InputPoll} " +
                            $"outputs={interpolation.OutputCount} " +
                            $"reset={interpolation.TemporalReset} " +
                            $"triangles={traceStats.CaptureTriangles} " +
                            $"commands={traceStats.OutputCommands} " +
                            $"drawCalls={traceStats.DrawCalls} " +
                            $"transparent={traceStats.TransparentDrawCalls} " +
                            $"size={traceStats.OutputWidth}x{traceStats.OutputHeight} " +
                            $"pairMs={interpolation.PairPipelineMicroseconds / 1000.0:F3} " +
                            $"interpolationMs={interpolation.InterpolationMicroseconds / 1000.0:F3} " +
                            $"midpointMs={interpolation.MidpointRenderMicroseconds / 1000.0:F3} " +
                            $"actualMs={interpolation.ActualRenderMicroseconds / 1000.0:F3} " +
                            $"averageMs={averageMs:F3} " +
                            $"pipelineAverageMs={pipelineAverageMs:F3} " +
                            $"decodeMs={traceStats.DecodeMicroseconds / 1000.0:F3} " +
                            $"drawListMs={traceStats.DrawListMicroseconds / 1000.0:F3} " +
                            $"topologyMs={traceStats.TopologyMicroseconds / 1000.0:F3} " +
                            // Preserve the original names as pipeline aliases
                            // for existing harnesses, then report every required
                            // component explicitly.
                            $"p50Ms={pipelinePercentiles.p50 / 1000.0:F3} " +
                            $"p95Ms={pipelinePercentiles.p95 / 1000.0:F3} " +
                            $"p99Ms={pipelinePercentiles.p99 / 1000.0:F3} " +
                            $"pipelineP50Ms={pipelinePercentiles.p50 / 1000.0:F3} " +
                            $"pipelineP95Ms={pipelinePercentiles.p95 / 1000.0:F3} " +
                            $"pipelineP99Ms={pipelinePercentiles.p99 / 1000.0:F3} " +
                            $"submitP50Ms={submitPercentiles.p50 / 1000.0:F3} " +
                            $"submitP95Ms={submitPercentiles.p95 / 1000.0:F3} " +
                            $"submitP99Ms={submitPercentiles.p99 / 1000.0:F3} " +
                            $"topologyP50Ms={topologyPercentiles.p50 / 1000.0:F3} " +
                            $"topologyP95Ms={topologyPercentiles.p95 / 1000.0:F3} " +
                            $"topologyP99Ms={topologyPercentiles.p99 / 1000.0:F3} " +
                            $"profileSamples={_recentProfileCount} " +
                            $"profileScope=track-world " +
                            $"matched={interpolation.MatchedCommands}/" +
                            $"{interpolation.PreviousCommands} " +
                            $"matchRate={matchRate:F2}% " +
                            $"worldEligible=" +
                            $"{interpolation.EligibleWorldCommands} " +
                            $"trackMatched={interpolation.MatchedTrackCommands} " +
                            $"vehicleMatched={interpolation.MatchedVehicleCommands} " +
                            $"groups={interpolation.MatchedTransformGroups}/" +
                            $"{interpolation.PreviousTransformGroups}/" +
                            $"{interpolation.CurrentTransformGroups} " +
                            $"exactGroups={interpolation.ExactRigidTransformGroups} " +
                            $"incoherentExactGroups=" +
                            $"{interpolation.IncoherentExactTransformGroups} " +
                            $"heldIncoherentVehicle=" +
                            $"{interpolation.HeldIncoherentVehicleCommands} " +
                            $"heldTrackVisibility=" +
                            $"{interpolation.HeldTrackVisibilityCommands} " +
                            $"heldUnsafeTrack=" +
                            $"{interpolation.HeldUnsafeTrackCommands} " +
                            $"firstOutput={firstStats.OutputWidth}x" +
                            $"{firstStats.OutputHeight}/r{firstStats.Reserved} " +
                            $"secondOutput={secondStats.OutputWidth}x" +
                            $"{secondStats.OutputHeight}/r{secondStats.Reserved} " +
                            $"completionAge={completionAge} " +
                            $"topologyBoundary={traceStats.TopologyBoundaryGroups} " +
                            $"tJunctions={traceStats.TopologyTJunctions} " +
                            $"coplanar={traceStats.TopologyCoplanarPairs} " +
                            $"submitted={_submitted} rendered={_rendered} " +
                            $"consumed={_consumed} dropped={_dropped}");
                    }
                }
                finally
                {
                    Volatile.Write(ref _activePairStartTicks, 0);
                    Volatile.Write(ref _activeCaptureInputPoll, -1);
                    if (!firstPublished)
                        ReturnOutput(firstOutput);
                    if (!secondPublished)
                        ReturnOutput(secondOutput);
                    ReturnCaptureBuffer(capture);
                    lock (_gate)
                    {
                        if (_pendingCaptures.Count != 0)
                            _workReady.Set();
                    }
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

    void PublishRenderedOutput(
        byte[] pixels,
        in LiveNativeStats stats)
    {
        PublishOutput(pixels, in stats);
        _rendered++;
        if ((stats.Reserved & 2u) != 0)
            _renderedRepeated++;
        else if ((stats.Reserved & 1u) != 0)
            _renderedSynthetic++;
        else
            _renderedActual++;
    }

    void DrainCompletedPairs(nint handle)
    {
        while (PublishedOutputCount <= PublishedOutputCapacity - 2)
        {
            if (!_outputPool.TryDequeue(out byte[]? first))
                return;
            if (!_outputPool.TryDequeue(out byte[]? second))
            {
                ReturnOutput(first);
                return;
            }
            bool firstPublished = false;
            bool secondPublished = false;
            try
            {
                var firstStats = new LiveNativeStats
                {
                    StructSize =
                        (uint)Marshal.SizeOf<LiveNativeStats>(),
                };
                var secondStats = new LiveNativeStats
                {
                    StructSize =
                        (uint)Marshal.SizeOf<LiveNativeStats>(),
                };
                int result = NativeMethods.TryReadPair(
                    handle,
                    first,
                    second,
                    (nuint)first.Length,
                    ref firstStats,
                    ref secondStats);
                if (result < 0)
                    throw new InvalidOperationException(
                        $"native readback drain failed result={result} " +
                        $"detail={firstStats.Result}");
                if (result == 0)
                    return;
                PublishRenderedOutput(first, in firstStats);
                firstPublished = true;
                PublishRenderedOutput(second, in secondStats);
                secondPublished = true;
            }
            finally
            {
                if (!firstPublished)
                    ReturnOutput(first);
                if (!secondPublished)
                    ReturnOutput(second);
            }
        }
    }

    void PublishOutput(byte[] pixels, in LiveNativeStats stats)
    {
        LiveWorldOutput? discarded = null;
        lock (_gate)
        {
            // Keep a short queue of complete authored frames. Native work lands
            // in short bursts around dense GT2 object buckets even when its
            // average rate is above 60 Hz. A two-image queue discarded those
            // bursts and then left later vblanks with no unique image. Eight
            // chronological outputs absorb opposing producer/consumer bursts;
            // the remaining fixed buffers cover RenderPair, one catch-up pair,
            // and the host upload without starving the worker.
            if (_published.Count >= PublishedOutputCapacity)
            {
                discarded = _published.Dequeue();
                _dropped++;
                _droppedPublished++;
            }
            _published.Enqueue(new LiveWorldOutput(
                pixels,
                checked((int)stats.OutputWidth),
                checked((int)stats.OutputHeight),
                checked((long)stats.FrameIndex),
                stats.InputPoll,
                stats));
            Monitor.PulseAll(_gate);
        }
        if (discarded is { } stale)
            ReturnOutput(stale.Pixels);
        _firstOutputReady.Set();
    }

    void RecordProfileDurations(
        ulong pipelineMicroseconds,
        ulong submitMicroseconds,
        ulong topologyMicroseconds)
    {
        _recentPipelineMicroseconds[_recentProfileCursor] =
            pipelineMicroseconds;
        _recentSubmitMicroseconds[_recentProfileCursor] = submitMicroseconds;
        _recentTopologyMicroseconds[_recentProfileCursor] =
            topologyMicroseconds;
        _recentProfileCursor =
            (_recentProfileCursor + 1) % _recentPipelineMicroseconds.Length;
        _recentProfileCount = Math.Min(
            _recentProfileCount + 1,
            _recentPipelineMicroseconds.Length);
    }

    (ulong p50, ulong p95, ulong p99) GetPercentiles(ulong[] source)
    {
        if (_recentProfileCount == 0)
            return default;
        var samples = new ulong[_recentProfileCount];
        Array.Copy(source, samples, samples.Length);
        Array.Sort(samples);
        return (
            Percentile(samples, 0.50),
            Percentile(samples, 0.95),
            Percentile(samples, 0.99));
    }

    internal static double CommandMatchRate(
        uint matchedCommands,
        uint previousCommands) =>
        previousCommands == 0
            ? 0.0
            : matchedCommands * 100.0 / previousCommands;

    static ulong Percentile(ulong[] sortedSamples, double percentile)
    {
        int index = Math.Clamp(
            (int)Math.Ceiling(sortedSamples.Length * percentile) - 1,
            0,
            sortedSamples.Length - 1);
        return sortedSamples[index];
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_stopping)
                return;
            _stopping = true;
            Monitor.PulseAll(_gate);
            while (_pendingCaptures.Count != 0)
                ReturnCaptureBuffer(_pendingCaptures.Dequeue().Buffer);
            while (_published.Count != 0)
                ReturnOutput(_published.Dequeue().Pixels);
        }
        _workReady.Set();
        _worker?.Join(TimeSpan.FromSeconds(5));
        _firstOutputReady.Dispose();
        _workReady.Dispose();
        var pipelinePercentiles = GetPercentiles(
            _recentPipelineMicroseconds);
        var submitPercentiles = GetPercentiles(_recentSubmitMicroseconds);
        var topologyPercentiles = GetPercentiles(
            _recentTopologyMicroseconds);
        double outputWaitAverageMilliseconds = _outputWaitCount == 0
            ? 0.0
            : _outputWaitTicks * 1000.0 /
                (Stopwatch.Frequency * _outputWaitCount);
        double outputWaitMaximumMilliseconds =
            _outputWaitMaxTicks * 1000.0 / Stopwatch.Frequency;
        Console.Error.WriteLine(
            $"[Native-Profile] scope=track-world samples={_recentProfileCount} " +
            $"pipelineP50Ms={pipelinePercentiles.p50 / 1000.0:F3} " +
            $"pipelineP95Ms={pipelinePercentiles.p95 / 1000.0:F3} " +
            $"pipelineP99Ms={pipelinePercentiles.p99 / 1000.0:F3} " +
            $"submitP50Ms={submitPercentiles.p50 / 1000.0:F3} " +
            $"submitP95Ms={submitPercentiles.p95 / 1000.0:F3} " +
            $"submitP99Ms={submitPercentiles.p99 / 1000.0:F3} " +
            $"topologyP50Ms={topologyPercentiles.p50 / 1000.0:F3} " +
            $"topologyP95Ms={topologyPercentiles.p95 / 1000.0:F3} " +
            $"topologyP99Ms={topologyPercentiles.p99 / 1000.0:F3}");
        Console.Error.WriteLine(
            $"[Native-World] shutdown submitted={_submitted} " +
            $"rendered={_rendered} actual={_renderedActual} " +
            $"synthetic={_renderedSynthetic} " +
            $"repeated={_renderedRepeated} " +
            $"syntheticAttempts={_syntheticAttempts} " +
            $"syntheticNoOutput={_syntheticNoOutput} " +
            $"consumed={_consumed} dropped={_dropped} " +
            $"droppedPending={_droppedPendingCaptures} " +
            $"droppedOutputPool={_droppedOutputPool} " +
            $"droppedPublished={_droppedPublished} " +
            $"outputWaits={_outputWaitCount} " +
            $"outputWaitTimeouts={_outputWaitTimeouts} " +
            $"outputWaitAvgMs={outputWaitAverageMilliseconds:F3} " +
            $"outputWaitMaxMs={outputWaitMaximumMilliseconds:F3} " +
            $"outputWaitPendingTails={_outputWaitPendingTails}");
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
            EntryPoint = "opengt_live_render_pair",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int RenderPair(
            nint handle,
            byte[] capture,
            nuint captureSize,
            byte[] firstOutput,
            byte[] secondOutput,
            nuint outputCapacity,
            in LiveNativeOptions options,
            ref LiveNativeStats firstStats,
            ref LiveNativeStats secondStats,
            ref LiveInterpolationStats interpolationStats);

        [DllImport(
            Library,
            EntryPoint = "opengt_live_try_read_pair",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int TryReadPair(
            nint handle,
            byte[] firstOutput,
            byte[] secondOutput,
            nuint outputCapacity,
            ref LiveNativeStats firstStats,
            ref LiveNativeStats secondStats);

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
    internal const int TriangleReservationRecords = 64;

    readonly record struct CameraProjectionKey(
        ulong TransformId,
        int OffsetX,
        int OffsetY,
        ushort Plane);
    readonly record struct SourceVertexKey(
        uint ModelPointer,
        short X,
        short Y,
        short Z,
        byte ScreenOffsetDirection);

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
    public bool LastPresentedFrameContainedWorld { get; private set; }
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
        uint environmentFlags = 0;
        if (env.SetMask) environmentFlags |= 1U << 0;
        if (env.CheckMask) environmentFlags |= 1U << 1;
        if (env.Dither) environmentFlags |= 1U << 2;
        int streamOffset = checked((int)_stream!.Position);
        EnsureTriangleStorage(
            _stream,
            checked(streamOffset + LiveWorldRenderer.TriangleStride),
            LiveWorldRenderer.HeaderSize +
                LiveWorldRenderer.MaxTriangles *
                LiveWorldRenderer.TriangleStride);
        Span<byte> record = _buffer!.AsSpan(
            streamOffset,
            LiveWorldRenderer.TriangleStride);
        int offset = 0;
        WriteUInt32(record, ref offset, primitiveFlags);
        WriteUInt16(record, ref offset, flags.TPage);
        WriteUInt16(record, ref offset, flags.Clut);
        WriteInt32(record, ref offset, flags.OtIndex);
        WriteInt16(record, ref offset, (short)env.ClipX0);
        WriteInt16(record, ref offset, (short)env.ClipY0);
        WriteInt16(record, ref offset, (short)env.ClipX1);
        WriteInt16(record, ref offset, (short)env.ClipY1);
        WriteInt16(record, ref offset, (short)env.TwMaskX);
        WriteInt16(record, ref offset, (short)env.TwMaskY);
        WriteInt16(record, ref offset, (short)env.TwOffX);
        WriteInt16(record, ref offset, (short)env.TwOffY);
        WriteUInt32(record, ref offset, environmentFlags);
        WriteUInt32(record, ref offset, (uint)identity.Object.Kind);
        WriteUInt32(record, ref offset, identity.Object.StableId);
        WriteUInt32(record, ref offset, identity.Object.ModelPointer);
        WriteInt16(record, ref offset, (short)env.DrawOffsetX);
        WriteInt16(record, ref offset, (short)env.DrawOffsetY);
        WriteUInt64(record, ref offset, identity.TransformId);
        WriteTransform(record, ref offset, in identity);
        WriteVertex(record, ref offset, in a, in originA);
        WriteVertex(record, ref offset, in b, in originB);
        WriteVertex(record, ref offset, in c, in originC);
        WriteUInt64(record, ref offset, 0);
        Debug.Assert(offset == LiveWorldRenderer.TriangleStride);
        _stream.Position = streamOffset + offset;
        _triangleCount++;
    }

    /// <summary>
    /// Exposes backing-array space before a record is written directly. A
    /// MemoryStream only advances its logical length through Stream writes or
    /// SetLength; filling GetBuffer()/the public backing array alone does not.
    /// If the frame-final VRAM reservation grew the stream afterward, it would
    /// zero-fill the apparent gap and erase every directly encoded triangle.
    /// Reserve in small batches so the hot path avoids one SetLength call per
    /// triangle without retaining an oversized logical capture.
    /// </summary>
    internal static void EnsureTriangleStorage(
        MemoryStream stream,
        long requiredEnd,
        long maximumEnd)
    {
        if (requiredEnd <= stream.Length)
            return;
        long reservationBytes =
            (long)TriangleReservationRecords * LiveWorldRenderer.TriangleStride;
        long reservedEnd = Math.Min(
            maximumEnd,
            checked(requiredEnd + reservationBytes - 1) /
                reservationBytes * reservationBytes);
        if (reservedEnd < requiredEnd)
            throw new InvalidOperationException(
                "Triangle capture exceeded its bounded storage reservation.");
        stream.SetLength(reservedEnd);
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
        LastPresentedFrameContainedWorld = false;
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
        // Decide ownership from authored 3D provenance before validating or
        // submitting the capture. A malformed, oversized, truncated, or
        // temporarily unrenderable world frame must become an observable
        // modern-renderer miss, never a silent compatibility-world fallback.
        LastPresentedFrameContainedWorld = true;
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
            TextureSmoothing: ConfigManager.View.TextureSmoothing,
            OutputScale:
                ConfigManager.View.HighResolution3D ? 4 : 1);
        int outputWidth =
            _viewportArea > 0 ? _viewportWidth : display.W;
        int outputHeight =
            _viewportArea > 0 ? _viewportHeight : display.H;
        // World ownership is provenance-based, not tied to the usual 320x240
        // race viewport. GT2's wider transition and rotating Results-car views
        // contain complete captured screen primitives plus 3D provenance and
        // are rendered natively as long as they fit the bounded output pool.
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
        if (!_renderer.Submit(submitted, size, settings))
        {
            Console.Error.WriteLine(
                $"[Native-World] submission rejected frame={presentedFrame} " +
                $"poll={inputPoll}");
        }
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
            origin.ModelZ,
            ScreenOffsetDirection(in origin));
        if (_sourceVertices.TryGetValue(key, out uint identity))
            return identity;
        identity = 0x40000000u |
            checked((uint)_sourceVertices.Count + 1u);
        _sourceVertices.Add(key, identity);
        return identity;
    }

    static byte ScreenOffsetDirection(in GteProjectionOrigin origin)
    {
        if ((origin.Flags & GteProjectionOriginFlags.ScreenOffsetAnchor) == 0)
            return 0;
        int x = origin.ScreenOffsetX;
        int y = origin.ScreenOffsetY;
        if (x == 0 && y == 0)
            return 1;
        int horizontal = Math.Abs(x);
        int vertical = Math.Abs(y);
        if (horizontal > vertical * 2)
            return (byte)(x < 0 ? 2 : 3);
        if (vertical > horizontal * 2)
            return (byte)(y < 0 ? 4 : 5);
        if (x < 0)
            return (byte)(y < 0 ? 6 : 7);
        return (byte)(y < 0 ? 8 : 9);
    }

    GteProjectionOrigin SelectCamera()
    {
        if (_trackCameras.Count != 0)
        {
            int maximum = int.MinValue;
            GteProjectionOrigin selected = default;
            foreach (var entry in _trackCameras.Values)
            {
                if (entry.Count <= maximum)
                    continue;
                maximum = entry.Count;
                selected = entry.Origin;
            }
            return selected;
        }
        if (_allTransforms.Count == 0)
            return default;
        int allMaximum = int.MinValue;
        GteProjectionOrigin allSelected = default;
        foreach (var entry in _allTransforms.Values)
        {
            if (entry.Count <= allMaximum)
                continue;
            allMaximum = entry.Count;
            allSelected = entry.Origin;
        }
        return allSelected;
    }

    void WriteVertex(
        Span<byte> destination,
        ref int offset,
        in HleVertex vertex,
        in GteProjectionOrigin origin)
    {
        WriteSingle(destination, ref offset, vertex.X);
        WriteSingle(destination, ref offset, vertex.Y);
        WriteSingle(destination, ref offset, vertex.Z);
        WriteInt16(destination, ref offset, vertex.U);
        WriteInt16(destination, ref offset, vertex.V);
        destination[offset++] = vertex.R;
        destination[offset++] = vertex.G;
        destination[offset++] = vertex.B;
        byte captureFlags = (byte)(origin.Valid ? 1 : 0);
        if ((origin.Flags & GteProjectionOriginFlags.ScreenOffsetAnchor) != 0)
            captureFlags |= 1 << 1;
        destination[offset++] = captureFlags;
        WriteInt16(destination, ref offset, origin.ModelX);
        WriteInt16(destination, ref offset, origin.ModelY);
        WriteInt16(destination, ref offset, origin.ModelZ);
        WriteInt16(destination, ref offset, 0);
        WriteInt32(destination, ref offset, origin.ViewX);
        WriteInt32(destination, ref offset, origin.ViewY);
        WriteInt32(destination, ref offset, origin.ViewZ);
        WriteInt32(destination, ref offset, origin.ProjectionOffsetX);
        WriteInt32(destination, ref offset, origin.ProjectionOffsetY);
        WriteUInt32(destination, ref offset, origin.ProjectionPlane);
        WriteUInt32(destination, ref offset, SourceIdentity(in origin));
        WriteUInt64(destination, ref offset, origin.TransformId);
        WriteTransform(destination, ref offset, in origin);
    }

    static void WriteTransform(
        Span<byte> destination,
        ref int offset,
        in GteProjectionOrigin origin)
    {
        WriteInt16(destination, ref offset, origin.R00);
        WriteInt16(destination, ref offset, origin.R01);
        WriteInt16(destination, ref offset, origin.R02);
        WriteInt16(destination, ref offset, origin.R10);
        WriteInt16(destination, ref offset, origin.R11);
        WriteInt16(destination, ref offset, origin.R12);
        WriteInt16(destination, ref offset, origin.R20);
        WriteInt16(destination, ref offset, origin.R21);
        WriteInt16(destination, ref offset, origin.R22);
        WriteInt16(destination, ref offset, 0);
        WriteInt32(destination, ref offset, origin.TranslateX);
        WriteInt32(destination, ref offset, origin.TranslateY);
        WriteInt32(destination, ref offset, origin.TranslateZ);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void WriteInt16(Span<byte> destination, ref int offset, short value)
    {
        BinaryPrimitives.WriteInt16LittleEndian(destination[offset..], value);
        offset += sizeof(short);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void WriteUInt16(
        Span<byte> destination,
        ref int offset,
        ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(destination[offset..], value);
        offset += sizeof(ushort);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void WriteInt32(Span<byte> destination, ref int offset, int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(destination[offset..], value);
        offset += sizeof(int);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void WriteUInt32(
        Span<byte> destination,
        ref int offset,
        uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(destination[offset..], value);
        offset += sizeof(uint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void WriteUInt64(
        Span<byte> destination,
        ref int offset,
        ulong value)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(destination[offset..], value);
        offset += sizeof(ulong);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void WriteSingle(
        Span<byte> destination,
        ref int offset,
        float value) => WriteInt32(
            destination,
            ref offset,
            BitConverter.SingleToInt32Bits(value));

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
        _writer.Write(6U);
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
