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
    nint NativeTexture,
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
    public uint TargetAspectWidth;
    public uint TargetAspectHeight;
    public uint DirectOutputSlot;
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
    public ulong OutputFingerprint;
    public ulong WorldFingerprint;
    public ulong OutputTexture;
}

[StructLayout(LayoutKind.Sequential)]
internal struct LiveTextureUpload
{
    public ulong Key;
    public int X;
    public int Y;
    public int WordWidth;
    public int Height;
}

internal readonly record struct LiveRenderSettings(
    bool Depth,
    bool Dithering,
    bool Topology,
    bool PerspectiveCorrect,
    bool TextureSmoothing,
    bool HighResolutionTextures,
    int OutputScale,
    int TargetAspectWidth,
    int TargetAspectHeight);

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
    internal const int OutputBufferCount = 5;
    // The capture side owns one active frame plus a two-frame pending FIFO.
    // The five fixed output buffers form the completed-output reserve. During
    // initial prebuffering all five may be published; the host then consumes
    // and returns one before the worker needs its next destination. GPU
    // readback uses a separate staging ring and cannot consume these slots.
    internal const int PublishedOutputCapacity = 5;
    // Presentation spends this only when the chronological output queue is
    // empty. It is returned by the later vblank throttle in the normal case;
    // Twelve milliseconds catches imminent native completions without letting a
    // scheduler oversleep consume most of the next NTSC presentation interval.
    internal const int OutputReadyWaitMilliseconds = 12;
    internal const int MaxTriangles = 32_768;
    internal const int HeaderSize = 160;
    internal const int TriangleStride = 384;
    internal const int VramBytes = 1024 * 512 * 2;
    internal const int ResidentTrackInstanceStride = 104;
    internal const int MaxResidentTrackInstances = 1024;
    internal const int CaptureCapacity =
        HeaderSize + MaxTriangles * TriangleStride + VramBytes +
        MaxResidentTrackInstances * ResidentTrackInstanceStride;
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
    const uint HighResolutionTexturesFlag = 1u << 7;
    const uint DirectGpuOutputFlag = 1u << 8;
    const uint NoOutputStatsFlag = 1u << 3;

    static readonly bool ForceWarp =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_NATIVE_WORLD_WARP") == "1";
#if !OPENGT_RELEASE_PACKAGE
    // Diagnostic classifier only. Release builds always use the modern depth
    // path; this switch exists to isolate depth reconstruction from guest
    // submission and visibility failures at an identical authored frame.
    internal static readonly bool DisableDepthForDiagnostics =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_NATIVE_WORLD_DISABLE_DEPTH") == "1";
#endif
    static readonly bool TraceTextureUploads =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_TEXTURE_UPLOADS") == "1";
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
    static readonly string? DumpOutputPath =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_NATIVE_WORLD_OUTPUT_DUMP_PATH");
    static readonly int DumpOutputInputPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_NATIVE_WORLD_OUTPUT_DUMP_INPUT_POLL"),
            out int dumpOutputInputPoll)
            ? Math.Max(0, dumpOutputInputPoll)
            : -1;
    static readonly int DumpOutputCount =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_NATIVE_WORLD_OUTPUT_DUMP_COUNT"),
            out int dumpOutputCount)
            ? Math.Clamp(dumpOutputCount, 1, 16)
            : 1;
    static readonly bool DirectGpuOutputAllowed =
        !string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_DISABLE_DIRECT_GPU_OUTPUT"),
            "1",
            StringComparison.Ordinal) &&
        !string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_AUDIT_NATIVE_WORLD_OUTPUT_HASH"),
            "1",
            StringComparison.Ordinal) &&
        string.IsNullOrWhiteSpace(DumpOutputPath);

    readonly ConcurrentQueue<byte[]> _capturePool = new();
    readonly ConcurrentQueue<byte[]> _outputPool = new();
    readonly Dictionary<byte[], uint> _outputSlots =
        new(ReferenceEqualityComparer.Instance);
    readonly ConcurrentQueue<byte[]> _residentMeshRegistrations = new();
    readonly object _gate = new();
    readonly object _textureUploadGate = new();
    readonly AutoResetEvent _workReady = new(false);
    readonly ManualResetEventSlim _firstOutputReady = new(false);
    readonly ManualResetEventSlim _presentationDeviceReady = new(false);
    readonly Thread? _worker;

    readonly Queue<PendingCapture> _pendingCaptures = new();
    readonly Queue<LiveWorldOutput> _published = new();
    bool _stopping;
    bool _failed;
    Exception? _failure;
    long _submitted;
    long _rendered;
    long _renderedActual;
    long _renderedSynthetic;
    long _renderedRepeated;
    long _authoredNoOutput;
    long _dropped;
    long _droppedPendingCaptures;
    // Retained in shutdown telemetry as an explicit invariant. Output-buffer
    // exhaustion now back-pressures only this worker, so it must remain zero.
    long _droppedOutputPool = 0;
    long _droppedPublished;
    long _consumed;
    long _outputWaitCount;
    long _outputWaitTimeouts;
    long _outputWaitPendingTails;
    long _outputWaitTicks;
    long _outputWaitMaxTicks;
    int _activeCaptureInputPoll = -1;
    long _activeRenderStartTicks;
    nint _presentationDevice;
    long _totalRenderMicroseconds;
    long _totalPipelineMicroseconds;
    long _renderOperations;
    int _dumpEligibleCaptures;
    // Keep the three components aligned so each percentile window describes
    // the same authored frame submissions. Submit is CPU time inside the
    // D3D11 render call; the asynchronous live path does not wait for GPU
    // completion. Pipeline includes capture decode, topology, draw-list
    // construction, submission, and bridge overhead for that authored frame.
    readonly ulong[] _recentPipelineMicroseconds = new ulong[240];
    readonly ulong[] _recentSubmitMicroseconds = new ulong[240];
    readonly ulong[] _recentTopologyMicroseconds = new ulong[240];
    int _recentProfileCount;
    int _recentProfileCursor;
    int _dumpedCaptureCount;
    int _dumpedOutputCount;
    LiveTextureUpload[] _textureUploads = [];
    readonly HashSet<ulong> _tracedTextureUploadKeys = [];

    readonly record struct PendingCapture(
        byte[] Buffer,
        int Size,
        int ResidentInstanceOffset,
        int ResidentInstanceCount,
        LiveRenderSettings Settings,
        LiveTextureUpload[] TextureUploads);

    public static bool Requested => OperatingSystem.IsWindows();

    public bool Enabled => Requested && !_failed && !_stopping;

    internal long DroppedFrames => Interlocked.Read(ref _dropped);

    internal void ThrowIfFailed()
    {
#if OPENGT_RELEASE_PACKAGE
        Exception? failure = Volatile.Read(ref _failure);
        if (failure is not null)
        {
            throw new InvalidOperationException(
                "The required modern world renderer failed; " +
                "the release build has no compatibility fallback.",
                failure);
        }
#endif
    }

    public LiveWorldRenderer()
    {
        if (!Requested)
            return;
        for (int index = 0; index < CaptureBufferCount; ++index)
            _capturePool.Enqueue(new byte[CaptureCapacity]);
        for (uint index = 0; index < OutputBufferCount; ++index)
        {
            byte[] output = new byte[
                DirectGpuOutputAllowed ? 1 : MaxOutputBytes];
            _outputSlots.Add(output, index);
            _outputPool.Enqueue(output);
        }
        _worker = new Thread(WorkerMain)
        {
            IsBackground = true,
            Name = "OpenGT native world renderer",
            // The patched NTSC-U build authors one complete world state per
            // vblank. Rendering those states is presentation-critical once
            // real-time pacing begins. Keep this at the same highest thread
            // priority as the paced emulation thread while the process itself
            // remains AboveNormal. The bounded pending/output queues prevent
            // the worker from running ahead and turning a full output queue
            // into slow game time.
            Priority = ThreadPriority.Highest,
        };
        _worker.Start();
        Console.Error.WriteLine(
            $"[Native-World] enabled buffers={CaptureBufferCount} " +
            $"maxTriangles={MaxTriangles} outputBuffers={OutputBufferCount} " +
            "mode=authored-only syntheticPath=absent");
    }

    public void ConfigurePresentationDevice(nint d3d11Device)
    {
        if (!DirectGpuOutputAllowed || d3d11Device == 0)
            return;
        Volatile.Write(ref _presentationDevice, d3d11Device);
        _workReady.Set();
        if (!_presentationDeviceReady.Wait(TimeSpan.FromSeconds(10)))
            throw new TimeoutException(
                "native presentation device initialization timed out");
        Exception? failure = Volatile.Read(ref _failure);
        if (failure is not null)
            throw new InvalidOperationException(
                "native presentation device initialization failed",
                failure);
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
        int residentInstanceOffset,
        int residentInstanceCount,
        LiveRenderSettings settings)
    {
        if (!Enabled || size <= HeaderSize || size > capture.Length ||
            residentInstanceCount < 0 ||
            residentInstanceCount > MaxResidentTrackInstances ||
            residentInstanceOffset < 0 ||
            residentInstanceOffset +
                residentInstanceCount * ResidentTrackInstanceStride > size)
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
                residentInstanceOffset,
                residentInstanceCount,
                settings,
                Volatile.Read(ref _textureUploads)));
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

    public void RegisterResidentTrackMesh(byte[] definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (definition.Length < 32)
            throw new ArgumentException(
                "Resident track mesh definition is truncated.",
                nameof(definition));
        _residentMeshRegistrations.Enqueue(definition);
    }

    static bool Overlaps(
        in LiveTextureUpload upload,
        int x,
        int y,
        int width,
        int height) =>
        upload.X < x + width && x < upload.X + upload.WordWidth &&
        upload.Y < y + height && y < upload.Y + upload.Height;

    public void InvalidateTextureUploads(int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0)
            return;
        lock (_textureUploadGate)
        {
            LiveTextureUpload[] current = _textureUploads;
            LiveTextureUpload[] retained = current
                .Where(upload => !Overlaps(upload, x, y, width, height))
                .ToArray();
            if (retained.Length != current.Length)
                Volatile.Write(ref _textureUploads, retained);
        }
    }

    public void RecordTextureUpload(
        int x,
        int y,
        int wordWidth,
        int height,
        ReadOnlySpan<ushort> words)
    {
        if (
            wordWidth <= 0 || height <= 0 ||
            words.Length != checked(wordWidth * height) ||
            x < 0 || y < 0 ||
            x + wordWidth > 1024 || y + height > 512)
        {
            InvalidateTextureUploads(x, y, wordWidth, height);
            return;
        }
        const ulong Offset = 14695981039346656037UL;
        const ulong Prime = 1099511628211UL;
        ulong key = Offset;
        static void Add(ref ulong value, byte item)
        {
            value ^= item;
            value *= Prime;
        }
        Add(ref key, (byte)wordWidth);
        Add(ref key, (byte)(wordWidth >> 8));
        Add(ref key, (byte)height);
        Add(ref key, (byte)(height >> 8));
        foreach (ushort word in words)
        {
            Add(ref key, (byte)word);
            Add(ref key, (byte)(word >> 8));
        }
        var upload = new LiveTextureUpload
        {
            Key = key,
            X = x,
            Y = y,
            WordWidth = wordWidth,
            Height = height,
        };
        lock (_textureUploadGate)
        {
            var next = _textureUploads
                .Where(item => !Overlaps(item, x, y, wordWidth, height))
                .Append(upload)
                .ToArray();
            Volatile.Write(ref _textureUploads, next);
            if (TraceTextureUploads && _tracedTextureUploadKeys.Add(key))
            {
                string payload = words.Length <= 256
                    ? $" data={Convert.ToHexString(MemoryMarshal.AsBytes(words))}"
                    : string.Empty;
                Console.Error.WriteLine(
                    $"[TextureUpload] key={key:x16} x={x} y={y} " +
                    $"words={wordWidth} height={height}{payload}");
            }
        }
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
                // when a dense authored frame finishes a few milliseconds
                // late. Monitor.PulseAll in PublishOutput wakes the host
                // immediately, normally reducing the later throttle wait by
                // the same amount rather than extending the frame.
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
                        ref _activeRenderStartTicks);
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
            // A completed fifth frame may be waiting for the host to consume
            // one member of the presentation reserve. Wake only
            // the renderer worker; the emulation/presentation thread never
            // waits on a full output queue.
            Monitor.PulseAll(_gate);
            return true;
        }
    }

    public bool HasPendingOrActiveWork
    {
        get
        {
            if (Volatile.Read(ref _activeRenderStartTicks) != 0)
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
            bool discardedAny = false;
            while (_published.Count != 0)
            {
                ReturnOutput(_published.Dequeue().Pixels);
                _dropped++;
                _droppedPublished++;
                discardedAny = true;
            }
            if (discardedAny)
                Monitor.PulseAll(_gate);
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
            if (discarded.Count != 0)
                Monitor.PulseAll(_gate);
        }
        foreach (byte[] pixels in discarded)
            ReturnOutput(pixels);
    }

    public void ReturnOutput(byte[] pixels)
    {
        lock (_gate)
        {
            if (!_outputSlots.ContainsKey(pixels))
                return;
            _outputPool.Enqueue(pixels);
            Monitor.PulseAll(_gate);
        }
    }

    void WorkerMain()
    {
        nint handle = 0;
        nint boundPresentationDevice = 0;
        LiveTextureUpload[] boundTextureUploads = [];
        try
        {
            if (NativeMethods.ApiVersion() != 11)
                throw new InvalidOperationException(
                    "native renderer API version mismatch");
            handle = NativeMethods.Create();
            if (handle == 0)
                throw new InvalidOperationException(
                    "native renderer creation failed");
            while (true)
            {
                _workReady.WaitOne();
                nint requestedPresentationDevice = Volatile.Read(
                    ref _presentationDevice);
                if (
                    boundPresentationDevice == 0 &&
                    requestedPresentationDevice != 0
                )
                {
                    int deviceResult = NativeMethods.SetPresentationDevice(
                        handle, requestedPresentationDevice);
                    if (deviceResult != 0)
                        throw new InvalidOperationException(
                            "native presentation device binding failed " +
                            $"result={deviceResult}");
                    boundPresentationDevice = requestedPresentationDevice;
                    Console.Error.WriteLine(
                        "[Native-World] direct GPU presentation enabled");
                    _presentationDeviceReady.Set();
                }
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
                while (_residentMeshRegistrations.TryDequeue(
                    out byte[]? definition))
                {
                    int registrationResult =
                        NativeMethods.RegisterResidentMesh(
                            handle,
                            definition,
                            (nuint)definition.Length);
                    if (registrationResult != 0)
                    {
                        throw new InvalidOperationException(
                            "native resident mesh registration failed " +
                            $"result={registrationResult}");
                    }
                }
                unsafe
                {
                    fixed (byte* capturePointer = capture)
                    {
                        nint instancePointer = pending.ResidentInstanceCount == 0
                            ? 0
                            : (nint)(capturePointer +
                                pending.ResidentInstanceOffset);
                        int instanceResult =
                            NativeMethods.SetResidentInstances(
                                handle,
                                instancePointer,
                                (nuint)pending.ResidentInstanceCount);
                        if (instanceResult != 0)
                        {
                            throw new InvalidOperationException(
                                "native resident instance selection failed " +
                                $"result={instanceResult}");
                        }
                    }
                }
                if (!ReferenceEquals(
                        boundTextureUploads,
                        pending.TextureUploads))
                {
                    int uploadResult = NativeMethods.SetTextureUploads(
                        handle,
                        ForceWarp ? 1 : 0,
                        pending.TextureUploads,
                        (nuint)pending.TextureUploads.Length);
                    if (uploadResult != 0)
                        throw new InvalidOperationException(
                            $"native texture upload registry failed " +
                            $"result={uploadResult}");
                    boundTextureUploads = pending.TextureUploads;
                }
                byte[]? firstOutput = null;
                lock (_gate)
                {
                    // All five fixed buffers can briefly be owned by the
                    // published reserve or host upload. Wait for that
                    // upload to return its buffer instead of dropping the next
                    // authored capture. This blocks only the renderer worker.
                    while (
                        !_outputPool.TryDequeue(out firstOutput) &&
                        !_stopping)
                    {
                        Monitor.Wait(_gate);
                    }
                }
                if (firstOutput is null)
                {
                    ReturnCaptureBuffer(capture);
                    break;
                }
                if (
                    boundPresentationDevice == 0 &&
                    firstOutput.Length != MaxOutputBytes
                )
                {
                    uint fallbackSlot;
                    lock (_gate)
                    {
                        fallbackSlot = _outputSlots[firstOutput];
                        _outputSlots.Remove(firstOutput);
                        firstOutput = new byte[MaxOutputBytes];
                        _outputSlots.Add(firstOutput, fallbackSlot);
                    }
                }
                bool firstPublished = false;
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
                    if (settings.HighResolutionTextures)
                        flags |= HighResolutionTexturesFlag;
                    if (FrameClock.RealTimeThrottleActive)
                        flags |= RealtimeReadbackFlag;
                    bool directGpuOutput = boundPresentationDevice != 0;
                    if (directGpuOutput)
                        flags |= DirectGpuOutputFlag;
                    uint outputSlot = _outputSlots[firstOutput];
                    var options = new LiveNativeOptions
                    {
                        StructSize =
                            (uint)Marshal.SizeOf<LiveNativeOptions>(),
                        Flags = flags,
                        OutputScale =
                            (uint)Math.Clamp(settings.OutputScale, 1, 4),
                        ClearColorRgba8 = 0xFF402820,
                        TargetAspectWidth = (uint)Math.Max(
                            1, settings.TargetAspectWidth),
                        TargetAspectHeight = (uint)Math.Max(
                            1, settings.TargetAspectHeight),
                        DirectOutputSlot = outputSlot,
                    };
                    var firstStats = new LiveNativeStats
                    {
                        StructSize =
                            (uint)Marshal.SizeOf<LiveNativeStats>(),
                    };
                    Volatile.Write(
                        ref _activeCaptureInputPoll,
                        captureInputPoll);
                    Volatile.Write(
                        ref _activeRenderStartTicks,
                        Stopwatch.GetTimestamp());
                    int result = NativeMethods.Render(
                        handle,
                        capture,
                        (nuint)size,
                        firstOutput,
                        (nuint)firstOutput.Length,
                        in options,
                        ref firstStats);
                    if (result != 0)
                        throw new InvalidOperationException(
                            $"native render failed result={result} " +
                            $"detail={firstStats.Result} " +
                            $"captureDisplay=" +
                            $"{BitConverter.ToInt32(capture, 36)}x" +
                            $"{BitConverter.ToInt32(capture, 40)} " +
                            $"outputScale={options.OutputScale} " +
                            $"outputCapacity={firstOutput.Length}");
                    bool hasOutput =
                        (firstStats.Reserved & NoOutputStatsFlag) == 0;
                    if (hasOutput)
                    {
                        firstPublished = PublishRenderedOutput(
                            firstOutput,
                            in firstStats);
                    }
                    else
                        _authoredNoOutput++;
                    _renderOperations++;
                    _totalRenderMicroseconds +=
                        checked((long)firstStats.RenderMicroseconds);
                    _totalPipelineMicroseconds +=
                        checked((long)firstStats.PipelineMicroseconds);
                    // Component percentiles deliberately describe track-world
                    // work only. Menus, showroom, and Results are valid native
                    // 2D/3D transitions but would dilute topology to zero and
                    // make the shutdown profile misleading.
                    if (firstStats.TopologyMicroseconds != 0)
                    {
                        RecordProfileDurations(
                            firstStats.PipelineMicroseconds,
                            firstStats.RenderMicroseconds,
                            firstStats.TopologyMicroseconds);
                    }
                    if (
                        _renderOperations <= 3 ||
                        (_renderOperations % TraceInterval) == 0
                    )
                    {
                        double averageMs =
                            _totalRenderMicroseconds /
                            1000.0 /
                            Math.Max(1, _renderOperations);
                        double pipelineAverageMs =
                            _totalPipelineMicroseconds /
                            1000.0 /
                            Math.Max(1, _renderOperations);
                        var pipelinePercentiles = GetPercentiles(
                            _recentPipelineMicroseconds);
                        var submitPercentiles = GetPercentiles(
                            _recentSubmitMicroseconds);
                        var topologyPercentiles = GetPercentiles(
                            _recentTopologyMicroseconds);
                        int completionAge = Math.Max(
                            0, InputManager.CurrentPoll -
                                firstStats.InputPoll);
                        Console.Error.WriteLine(
                            $"[Native-World] frame={firstStats.FrameIndex} " +
                            $"poll={firstStats.InputPoll} " +
                            $"mode=authored outputs={(hasOutput ? 1 : 0)} " +
                            "syntheticPath=absent " +
                            $"triangles={firstStats.CaptureTriangles} " +
                            $"commands={firstStats.OutputCommands} " +
                            $"drawCalls={firstStats.DrawCalls} " +
                            $"transparent={firstStats.TransparentDrawCalls} " +
                            $"outputHash=0x{firstStats.OutputFingerprint:X16} " +
                            $"worldHash=0x{firstStats.WorldFingerprint:X16} " +
                            $"size={firstStats.OutputWidth}x{firstStats.OutputHeight} " +
                            $"pipelineMs={firstStats.PipelineMicroseconds / 1000.0:F3} " +
                            $"submitMs={firstStats.RenderMicroseconds / 1000.0:F3} " +
                            $"actualMs={firstStats.RenderMicroseconds / 1000.0:F3} " +
                            $"averageMs={averageMs:F3} " +
                            $"pipelineAverageMs={pipelineAverageMs:F3} " +
                            $"decodeMs={firstStats.DecodeMicroseconds / 1000.0:F3} " +
                            $"drawListMs={firstStats.DrawListMicroseconds / 1000.0:F3} " +
                            $"topologyMs={firstStats.TopologyMicroseconds / 1000.0:F3} " +
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
                            $"firstOutput={firstStats.OutputWidth}x" +
                            $"{firstStats.OutputHeight}/r{firstStats.Reserved} " +
                            $"completionAge={completionAge} " +
                            $"topologyBoundary={firstStats.TopologyBoundaryGroups} " +
                            $"tJunctions={firstStats.TopologyTJunctions} " +
                            $"coplanar={firstStats.TopologyCoplanarPairs} " +
                            $"submitted={_submitted} rendered={_rendered} " +
                            $"consumed={_consumed} dropped={_dropped}");
                    }
                }
                finally
                {
                    Volatile.Write(ref _activeRenderStartTicks, 0);
                    Volatile.Write(ref _activeCaptureInputPoll, -1);
                    if (!firstPublished)
                        ReturnOutput(firstOutput);
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
            Volatile.Write(ref _failure, exception);
            _failed = true;
            Console.Error.WriteLine(
                $"[Native-World] disabled: " +
                $"{exception.GetType().Name}: {exception.Message}");
            _presentationDeviceReady.Set();
        }
        finally
        {
            if (handle != 0)
                NativeMethods.Destroy(handle);
        }
    }

    bool PublishRenderedOutput(
        byte[] pixels,
        in LiveNativeStats stats)
    {
        if (stats.OutputTexture == 0)
            DumpRenderedOutput(pixels, in stats);
        if (!PublishOutput(pixels, in stats))
            return false;
        _rendered++;
        if ((stats.Reserved & 2u) != 0)
            _renderedRepeated++;
        else if ((stats.Reserved & 1u) != 0)
            _renderedSynthetic++;
        else
            _renderedActual++;
        return true;
    }

    void DumpRenderedOutput(
        byte[] pixels,
        in LiveNativeStats stats)
    {
        if (
            _dumpedOutputCount >= DumpOutputCount ||
            string.IsNullOrWhiteSpace(DumpOutputPath) ||
            stats.InputPoll < DumpOutputInputPoll
        )
            return;

        int width = checked((int)stats.OutputWidth);
        int height = checked((int)stats.OutputHeight);
        int pixelCount = checked(width * height);
        int rgbaBytes = checked(pixelCount * 4);
        if (width <= 0 || height <= 0 || rgbaBytes > pixels.Length)
            throw new InvalidOperationException(
                $"native output dump dimensions are invalid: " +
                $"{width}x{height} capacity={pixels.Length}");

        string path = Path.GetFullPath(DumpOutputPath!);
        if (DumpOutputCount > 1)
        {
            string? parent = Path.GetDirectoryName(path);
            string stem = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            path = Path.Combine(
                parent ?? string.Empty,
                $"{stem}_frame_{stats.FrameIndex}_poll_" +
                $"{stats.InputPoll}{extension}");
        }
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        byte[] rgb = ArrayPool<byte>.Shared.Rent(checked(pixelCount * 3));
        try
        {
            for (int source = 0, destination = 0;
                source < rgbaBytes;
                source += 4)
            {
                rgb[destination++] = pixels[source];
                rgb[destination++] = pixels[source + 1];
                rgb[destination++] = pixels[source + 2];
            }
            using var dump = File.Create(path);
            byte[] header = Encoding.ASCII.GetBytes(
                $"P6\n{width} {height}\n255\n");
            dump.Write(header);
            dump.Write(rgb, 0, pixelCount * 3);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rgb);
        }

        _dumpedOutputCount++;
        Console.Error.WriteLine(
            $"[Native-World-Output-Dump] frame={stats.FrameIndex} " +
            $"poll={stats.InputPoll} size={width}x{height} " +
            $"outputHash=0x{stats.OutputFingerprint:X16} path={path}");
    }

    bool PublishOutput(byte[] pixels, in LiveNativeStats stats)
    {
        lock (_gate)
        {
            // The five-frame limit combines the complete bounded capture work
            // window with two extra completion slots for dense preparation and
            // scheduler tails, without returning to the retired eight-frame
            // latency queue.
            // A renderer completion that races the next vblank waits here for
            // one reserve slot; discarding the oldest completed authored image
            // creates a visible temporal skip at world startup.
            while (
                _published.Count >= PublishedOutputCapacity &&
                !_stopping)
            {
                Monitor.Wait(_gate);
            }
            if (_stopping)
                return false;
            _published.Enqueue(new LiveWorldOutput(
                pixels,
                unchecked((nint)stats.OutputTexture),
                checked((int)stats.OutputWidth),
                checked((int)stats.OutputHeight),
                checked((long)stats.FrameIndex),
                stats.InputPoll,
                stats));
            Monitor.PulseAll(_gate);
        }
        _firstOutputReady.Set();
        return true;
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
        // Native direct-output frames share the host's D3D11 device. The
        // worker drains its GPU fences during native destruction, so do not
        // dispose synchronization objects while that drain is still active.
        _worker?.Join();
        _presentationDeviceReady.Dispose();
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
            "syntheticPath=absent " +
            $"authoredNoOutput={_authoredNoOutput} " +
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
            EntryPoint = "opengt_live_set_presentation_device",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int SetPresentationDevice(
            nint handle,
            nint d3d11Device);

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
            EntryPoint = "opengt_live_set_texture_uploads",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int SetTextureUploads(
            nint handle,
            int softwareAdapter,
            [In] LiveTextureUpload[] uploads,
            nuint uploadCount);

        [DllImport(
            Library,
            EntryPoint = "opengt_live_register_resident_mesh",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int RegisterResidentMesh(
            nint handle,
            byte[] definition,
            nuint definitionSize);

        [DllImport(
            Library,
            EntryPoint = "opengt_live_set_resident_instances",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int SetResidentInstances(
            nint handle,
            nint instances,
            nuint instanceCount);

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
    const int MaxDeferredStaticScenes = 8;
    const int InitialDeferredStaticTriangles = 4096;
    internal const int TriangleReservationRecords = 64;

    static (int Width, int Height) ParseTargetAspect()
    {
        var aspect = Host.HostWindow.GetWorldTargetAspect();
        return (
            Math.Clamp(aspect.Width, 1, 8192),
            Math.Clamp(aspect.Height, 1, 8192));
    }

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

    sealed class DeferredStaticScene
    {
        public uint Generation;
        public int TriangleCount;
        public int ResidentInstanceCount;
        public byte[] Triangles = new byte[
            InitialDeferredStaticTriangles *
            LiveWorldRenderer.TriangleStride];
        public readonly byte[] ResidentInstances = new byte[
            LiveWorldRenderer.MaxResidentTrackInstances *
            LiveWorldRenderer.ResidentTrackInstanceStride];
        public readonly Dictionary<
            CameraProjectionKey,
            (int Count, GteProjectionOrigin Origin)> Cameras = [];
#if !OPENGT_RELEASE_PACKAGE
        public readonly Dictionary<(WorldObjectKind Kind, ushort Plane), int>
            TraceCounts = [];
#endif

        public void EnsureTriangleCapacity(int requiredTriangles)
        {
            if (requiredTriangles <=
                Triangles.Length / LiveWorldRenderer.TriangleStride)
            {
                return;
            }
            int capacity = Math.Min(
                LiveWorldRenderer.MaxTriangles,
                Math.Max(
                    requiredTriangles,
                    checked(Triangles.Length /
                        LiveWorldRenderer.TriangleStride * 2)));
            if (capacity < requiredTriangles)
                throw new InvalidOperationException(
                    "Deferred static-scene triangle capacity exceeded.");
            Array.Resize(
                ref Triangles,
                checked(capacity * LiveWorldRenderer.TriangleStride));
        }

        public void Reset(uint generation = 0)
        {
            Generation = generation;
            TriangleCount = 0;
            ResidentInstanceCount = 0;
            Cameras.Clear();
#if !OPENGT_RELEASE_PACKAGE
            TraceCounts.Clear();
#endif
        }
    }

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
    readonly byte[] _alignedResidentTrackInstances =
        new byte[
            LiveWorldRenderer.MaxResidentTrackInstances *
            LiveWorldRenderer.ResidentTrackInstanceStride];
    readonly DeferredStaticScene[] _staticScenes =
        new DeferredStaticScene[MaxDeferredStaticScenes];
    readonly Dictionary<uint, int> _vehicleGenerationCounts = [];
#if !OPENGT_RELEASE_PACKAGE
    readonly int _cameraTraceStartPoll = ParseDiagnosticPoll(
        "RECOMPONE_TRACE_GT2_CAMERA_START_POLL");
    readonly int _cameraTraceEndPoll = ParseDiagnosticPoll(
        "RECOMPONE_TRACE_GT2_CAMERA_END_POLL");
    readonly int _scenePassTracePoll = ParseDiagnosticPoll(
        "RECOMPONE_TRACE_GT2_SCENE_PASS_POLL");
    readonly int _staticSceneTraceStartPoll = ParseDiagnosticPoll(
        "RECOMPONE_TRACE_GT2_STATIC_SCENE_START_POLL");
    readonly int _staticSceneTraceEndPoll = ParseDiagnosticPoll(
        "RECOMPONE_TRACE_GT2_STATIC_SCENE_END_POLL");
    readonly Dictionary<
        (WorldObjectKind Kind, uint Generation, ushort Plane), int>
        _sceneGenerationCounts = [];
#endif

    byte[]? _buffer;
    MemoryStream? _stream;
    BinaryWriter? _writer;
    long _geometryFrame;
    uint _triangleCount;
    uint _worldTriangleCount;
    int _outputResidentTrackInstanceCount;
    uint _selectedStaticGeneration;
    bool _staticSceneInjected;
    bool _staticSceneActivity;
    bool _truncated;
#if !OPENGT_RELEASE_PACKAGE
    bool _reportedOversizeOutput;
#endif
    int _nonprojectableWorldFrames;
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
    // Static Seattle geometry is cached by authored scene generation in
    // recorder-owned storage. It remains recordable while the transient
    // per-presentation capture buffers are all in flight on the render thread.
    public bool CanRecordDeferredStatic => _renderer.Enabled;
    public bool NeedsVramSnapshot =>
        Enabled &&
        (_worldTriangleCount != 0 ||
         _staticSceneActivity);
    public bool LastPresentedFrameContainedWorld { get; private set; }
    public GteProjectionOrigin MainProjection { get; private set; }

    public LiveWorldFrameRecorder(LiveWorldRenderer renderer)
    {
        _renderer = renderer;
        for (int index = 0; index < _staticScenes.Length; index++)
            _staticScenes[index] = new DeferredStaticScene();
        RentAndReset();
    }

    DeferredStaticScene GetOrCreateStaticScene(uint generation)
    {
        if (generation == 0)
            throw new InvalidOperationException(
                "A deferred static scene requires authored generation identity.");
        DeferredStaticScene? empty = null;
        DeferredStaticScene? oldest = null;
        foreach (DeferredStaticScene scene in _staticScenes)
        {
            if (scene.Generation == generation)
                return scene;
            if (scene.Generation == 0)
                empty ??= scene;
            if (oldest == null || scene.Generation < oldest.Generation)
                oldest = scene;
        }
        DeferredStaticScene selected = empty ?? oldest!;
#if !OPENGT_RELEASE_PACKAGE
        if (selected.Generation != 0 &&
            selected.Generation > _selectedStaticGeneration)
        {
            int evictionPoll = Host.InputManager.CurrentPoll;
            if (_staticSceneTraceStartPoll >= 0 &&
                evictionPoll >= _staticSceneTraceStartPoll &&
                (_staticSceneTraceEndPoll < 0 ||
                 evictionPoll <= _staticSceneTraceEndPoll))
            {
                Console.Error.WriteLine(
                    $"[GT2-Static-Scene] poll={evictionPoll} " +
                    "action=evict-unconsumed " +
                    $"generation={selected.Generation} incoming={generation} " +
                    $"selected={_selectedStaticGeneration} " +
                    $"captureBuffer={(_buffer != null ? "available" : "unavailable")}");
            }
        }
#endif
#if !OPENGT_RELEASE_PACKAGE
        int poll = Host.InputManager.CurrentPoll;
        if (_staticSceneTraceStartPoll >= 0 &&
            poll >= _staticSceneTraceStartPoll &&
            (_staticSceneTraceEndPoll < 0 ||
             poll <= _staticSceneTraceEndPoll))
        {
            Console.Error.WriteLine(
                $"[GT2-Static-Scene] poll={poll} action=create " +
                $"generation={generation} replaced={selected.Generation}");
        }
#endif
        selected.Reset(generation);
        return selected;
    }

    DeferredStaticScene? FindStaticScene(uint generation)
    {
        foreach (DeferredStaticScene scene in _staticScenes)
        {
            if (scene.Generation == generation)
                return scene;
        }
        return null;
    }

    [MethodImpl(
        MethodImplOptions.AggressiveInlining |
        MethodImplOptions.AggressiveOptimization)]
    public void RecordTriangle(
        long pendingFrame,
        in HleDrawEnv env,
        in HleVertex a,
        in HleVertex b,
        in HleVertex c,
        in GteProjectionOrigin originA,
        in GteProjectionOrigin originB,
        in GteProjectionOrigin originC,
        in PrimFlags flags,
        uint sourceA = 0,
        uint sourceB = 0,
        uint sourceC = 0)
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
            explicitScreenSpace: false,
            residentTrack: false,
            sourceA,
            sourceB,
            sourceC);
    }

    [MethodImpl(
        MethodImplOptions.AggressiveInlining |
        MethodImplOptions.AggressiveOptimization)]
    public void RegisterResidentTrackProjection(
        in GteProjectionOrigin origin,
        int vertexWeight)
    {
        if (vertexWeight > 0)
            CountStaticTransform(in origin, vertexWeight);
    }

    public void RegisterResidentTrackMesh(byte[] definition) =>
        _renderer.RegisterResidentTrackMesh(definition);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void RecordResidentTrackInstance(
        long pendingFrame,
        ulong meshKey,
        in HleDrawEnv env,
        in GteProjectionOrigin origin,
        ushort defaultTexturePage)
    {
        // Resident/static scene storage is independent of the transient live
        // capture buffer. Keep authoring it while every capture buffer is
        // briefly owned by the asynchronous renderer; the matching vehicle
        // ordering-table packets can arrive after a buffer has been returned.
        if (!_renderer.Enabled || !origin.Valid || meshKey == 0 ||
            origin.Object.ScenePass == WorldScenePass.Auxiliary)
            return;
        bool hadDeferredStatic = _staticSceneActivity;
        DeferredStaticScene scene = GetOrCreateStaticScene(
            origin.Object.SceneGeneration);
        _staticSceneActivity = true;
        if (scene.ResidentInstanceCount >=
            LiveWorldRenderer.MaxResidentTrackInstances)
        {
            _truncated = true;
            return;
        }
        if (_triangleCount == 0 && !hadDeferredStatic)
            _geometryFrame = pendingFrame;
        int viewportWidth = env.ClipX1 - env.ClipX0 + 1;
        int viewportHeight = env.ClipY1 - env.ClipY0 + 1;
        long viewportArea = (long)viewportWidth * viewportHeight;
        if (viewportWidth > 0 && viewportHeight > 0 &&
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
        Span<byte> record = scene.ResidentInstances.AsSpan(
            scene.ResidentInstanceCount *
                LiveWorldRenderer.ResidentTrackInstanceStride,
            LiveWorldRenderer.ResidentTrackInstanceStride);
        record.Clear();
        int offset = 0;
        WriteUInt64(record, ref offset, meshKey);
        WriteUInt32(record, ref offset, origin.Object.StableId);
        WriteUInt32(record, ref offset, origin.Object.ModelPointer);
        WriteUInt64(record, ref offset, origin.TransformId);
        WriteInt16(record, ref offset, origin.R00);
        WriteInt16(record, ref offset, origin.R01);
        WriteInt16(record, ref offset, origin.R02);
        WriteInt16(record, ref offset, origin.R10);
        WriteInt16(record, ref offset, origin.R11);
        WriteInt16(record, ref offset, origin.R12);
        WriteInt16(record, ref offset, origin.R20);
        WriteInt16(record, ref offset, origin.R21);
        WriteInt16(record, ref offset, origin.R22);
        WriteInt16(record, ref offset, 0);
        WriteInt32(record, ref offset, origin.TranslateX);
        WriteInt32(record, ref offset, origin.TranslateY);
        WriteInt32(record, ref offset, origin.TranslateZ);
        WriteInt32(record, ref offset, origin.ProjectionOffsetX);
        WriteInt32(record, ref offset, origin.ProjectionOffsetY);
        WriteUInt32(record, ref offset, origin.ProjectionPlane);
        WriteInt16(record, ref offset, (short)env.ClipX0);
        WriteInt16(record, ref offset, (short)env.ClipY0);
        WriteInt16(record, ref offset, (short)env.ClipX1);
        WriteInt16(record, ref offset, (short)env.ClipY1);
        WriteInt16(record, ref offset, (short)env.TwMaskX);
        WriteInt16(record, ref offset, (short)env.TwMaskY);
        WriteInt16(record, ref offset, (short)env.TwOffX);
        WriteInt16(record, ref offset, (short)env.TwOffY);
        WriteInt16(record, ref offset, (short)env.DrawOffsetX);
        WriteInt16(record, ref offset, (short)env.DrawOffsetY);
        uint environmentFlags = 0;
        if (env.SetMask) environmentFlags |= 1U << 0;
        if (env.CheckMask) environmentFlags |= 1U << 1;
        if (env.Dither) environmentFlags |= 1U << 2;
        WriteUInt32(record, ref offset, environmentFlags);
        uint packedResidentState =
            defaultTexturePage |
            ((uint)checked((ushort)scene.TriangleCount) << 16);
        WriteUInt32(record, ref offset, packedResidentState);
        WriteInt32(record, ref offset, origin.Object.DepthScaleExponent);
        WriteUInt32(
            record,
            ref offset,
            origin.Object.DepthScaleValid ? 1U : 0U);
        Debug.Assert(offset == LiveWorldRenderer.ResidentTrackInstanceStride);
        scene.ResidentInstanceCount++;
        CountStaticTransform(in origin, 1);
    }

    [MethodImpl(
        MethodImplOptions.AggressiveInlining |
        MethodImplOptions.AggressiveOptimization)]
    public void RecordResidentTrackTriangle(
        long pendingFrame,
        in HleDrawEnv env,
        in HleVertex a,
        in HleVertex b,
        in HleVertex c,
        in GteProjectionOrigin originA,
        in GteProjectionOrigin originB,
        in GteProjectionOrigin originC,
        in PrimFlags flags,
        uint sourceA,
        uint sourceB,
        uint sourceC)
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
            explicitScreenSpace: false,
            residentTrack: true,
            sourceA,
            sourceB,
            sourceC);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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
        bool explicitScreenSpace,
        bool residentTrack,
        uint sourceA,
        uint sourceB,
        uint sourceC)
    {
        if (!_renderer.Enabled)
            return;
        int valid = (originA.Valid ? 1 : 0) +
            (originB.Valid ? 1 : 0) +
            (originC.Valid ? 1 : 0);
        if (valid == 0 && !explicitScreenSpace)
            return;
        GteProjectionOrigin identity = originA.Valid
            ? originA
            : originB.Valid ? originB : originC;
        bool deferredStatic = IsDeferredStaticScene(in identity);
        // Track/background generations live in their own bounded cache and do
        // not require a free output-capture buffer. Screen-space and vehicle
        // records still do, because they are written directly to that buffer.
        if (!deferredStatic && !Enabled)
            return;
        bool hadDeferredStatic = _staticSceneActivity;
        DeferredStaticScene? staticScene = deferredStatic
            ? GetOrCreateStaticScene(identity.Object.SceneGeneration)
            : null;
        if (deferredStatic)
            _staticSceneActivity = true;
        if (_triangleCount == 0 && !hadDeferredStatic)
            _geometryFrame = pendingFrame;
        if ((deferredStatic
                ? staticScene!.TriangleCount
                : _triangleCount) >= LiveWorldRenderer.MaxTriangles)
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
#if !OPENGT_RELEASE_PACKAGE
        if (_scenePassTracePoll >= 0)
        {
            if (deferredStatic)
            {
                var key = (
                    identity.Object.Kind,
                    identity.ProjectionPlane);
                ref int sceneCount = ref
                    CollectionsMarshal.GetValueRefOrAddDefault(
                        staticScene!.TraceCounts,
                        key,
                        out _);
                sceneCount++;
            }
            else
            {
                var key = (
                    identity.Object.Kind,
                    identity.Object.SceneGeneration,
                    identity.ProjectionPlane);
                ref int sceneCount = ref
                    CollectionsMarshal.GetValueRefOrAddDefault(
                        _sceneGenerationCounts,
                        key,
                        out _);
                sceneCount++;
            }
        }
#endif
        if (!deferredStatic && valid != 0)
            _worldTriangleCount++;
        if (!residentTrack)
        {
            if (deferredStatic)
                CountStaticTransforms(in originA, in originB, in originC);
            else
                CountTransforms(in originA, in originB, in originC);
        }
        if (identity.Object.Kind == WorldObjectKind.Vehicle &&
            identity.Object.ScenePass == WorldScenePass.Main &&
            identity.Object.SceneGeneration != 0)
        {
            ref int generationCount = ref
                CollectionsMarshal.GetValueRefOrAddDefault(
                    _vehicleGenerationCounts,
                    identity.Object.SceneGeneration,
                    out _);
            generationCount++;
        }
        uint primitiveFlags = 0;
        if (flags.Textured) primitiveFlags |= 1U << 0;
        if (flags.SemiTrans) primitiveFlags |= 1U << 1;
        if (flags.RawTexture) primitiveFlags |= 1U << 2;
        if (flags.Gouraud) primitiveFlags |= 1U << 3;
        if (flags.ResidentCourse) primitiveFlags |= 1U << 4;
        if (flags.AuthoredTrackBillboardDepth)
            primitiveFlags |= 1U << 8;
        if (identity.Object.ScenePass == WorldScenePass.Auxiliary)
            primitiveFlags |= 1U << 5;
        uint environmentFlags = 0;
        if (env.SetMask) environmentFlags |= 1U << 0;
        if (env.CheckMask) environmentFlags |= 1U << 1;
        if (env.Dither) environmentFlags |= 1U << 2;
        int streamOffset = deferredStatic
            ? staticScene!.TriangleCount * LiveWorldRenderer.TriangleStride
            : checked((int)_stream!.Position);
        Span<byte> record;
        if (deferredStatic)
        {
            staticScene!.EnsureTriangleCapacity(
                staticScene.TriangleCount + 1);
            record = staticScene.Triangles.AsSpan(
                streamOffset,
                LiveWorldRenderer.TriangleStride);
        }
        else
        {
            EnsureTriangleStorage(
                _stream!,
                checked(streamOffset + LiveWorldRenderer.TriangleStride),
                LiveWorldRenderer.HeaderSize +
                    LiveWorldRenderer.MaxTriangles *
                    LiveWorldRenderer.TriangleStride);
            record = _buffer!.AsSpan(
                streamOffset,
                LiveWorldRenderer.TriangleStride);
        }
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
        // Authored background meshes also supply exact RAM vertex addresses.
        // Keep the legacy origin-derived identity path for ordinary packets.
        if (residentTrack || (sourceA != 0 && sourceB != 0 && sourceC != 0))
        {
            WriteVertex(record, ref offset, in a, in originA, sourceA);
            WriteVertex(record, ref offset, in b, in originB, sourceB);
            WriteVertex(record, ref offset, in c, in originC, sourceC);
        }
        else
        {
            WriteVertex(record, ref offset, in a, in originA);
            WriteVertex(record, ref offset, in b, in originB);
            WriteVertex(record, ref offset, in c, in originC);
        }
        WriteInt32(record, ref offset, identity.Object.DepthScaleExponent);
        WriteUInt32(
            record,
            ref offset,
            identity.Object.DepthScaleValid ? 1U : 0U);
        Debug.Assert(offset == LiveWorldRenderer.TriangleStride);
        if (deferredStatic)
        {
            staticScene!.TriangleCount++;
        }
        else
        {
            _stream!.Position = streamOffset + offset;
            _triangleCount++;
        }
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
            explicitScreenSpace: true,
            residentTrack: false,
            0,
            0,
            0);
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
        _renderer.ThrowIfFailed();
        LastPresentedFrameContainedWorld = false;
        if (!Enabled || presentedFrame < _geometryFrame)
        {
            ResetCurrent();
            return;
        }
        if (_worldTriangleCount == 0 && !_staticSceneActivity)
        {
            CacheCurrentScreenLines();
            ResetCurrent();
            return;
        }
        InjectAlignedStaticScene(inputPoll);
        if (_worldTriangleCount == 0 &&
            _outputResidentTrackInstanceCount == 0)
        {
            CacheCurrentScreenLines();
            ResetCurrent();
            return;
        }
        // Provenance without a usable projection plane occurs in authored 2D
        // transitions and cannot define a native 3D camera. Preserve only the
        // provenance-free screen compositor on those frames. Once a usable
        // camera exists, malformed, oversized, truncated, or temporarily
        // unrenderable world work remains an observable modern-renderer miss.
        GteProjectionOrigin camera = SelectCamera();
#if !OPENGT_RELEASE_PACKAGE
        TraceSceneGenerations(inputPoll);
#endif
        if (!camera.Valid || camera.ProjectionPlane == 0)
        {
            if (++_nonprojectableWorldFrames <= 3)
            {
                Console.Error.WriteLine(
                    $"[Native-World] compositor retained nonprojectable " +
                    $"provenance frame={presentedFrame} poll={inputPoll} " +
                    $"triangles={_worldTriangleCount} " +
                    $"residentInstances={_outputResidentTrackInstanceCount}");
            }
            ResetCurrent();
            return;
        }
#if !OPENGT_RELEASE_PACKAGE
        TraceCameraTransform(inputPoll, in camera);
#endif
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
        long residentInstanceOffset = vramOffset + vramBytes;
        int residentInstanceBytes = checked(
            _outputResidentTrackInstanceCount *
            LiveWorldRenderer.ResidentTrackInstanceStride);
        long captureLength = residentInstanceOffset +
            residentInstanceBytes;
        if (captureLength > _buffer.Length)
        {
            ResetCurrent();
            return;
        }
        // The fixed-capacity stream deliberately retains the full backing
        // length. Native receives the exact used byte count separately, so
        // there is no reason to zero-fill the VRAM range immediately before
        // overwriting every byte of it.
        MemoryMarshal.AsBytes(vram).CopyTo(
            _buffer.AsSpan(checked((int)vramOffset), vramBytes));
        _alignedResidentTrackInstances.AsSpan(0, residentInstanceBytes).CopyTo(
            _buffer.AsSpan(
                checked((int)residentInstanceOffset),
                residentInstanceBytes));
        _stream.Position = captureLength;
        MainProjection = camera;
        var targetAspect = ParseTargetAspect();
        bool enableDepth = true;
#if !OPENGT_RELEASE_PACKAGE
        enableDepth = !LiveWorldRenderer.DisableDepthForDiagnostics;
#endif
        var settings = new LiveRenderSettings(
            Depth: enableDepth,
            Dithering: ConfigManager.View.Ps1Dithering,
            Topology: ConfigManager.View.StabilizeGeometrySeams,
            PerspectiveCorrect:
                ConfigManager.View.PerspectiveCorrectTextures,
            TextureSmoothing: ConfigManager.View.TextureSmoothing,
            HighResolutionTextures:
                ConfigManager.View.HighResolutionTextures,
            OutputScale:
                ConfigManager.View.HighResolution3D ? 4 : 1,
            TargetAspectWidth: targetAspect.Width,
            TargetAspectHeight: targetAspect.Height);
        int outputWidth =
            _viewportArea > 0 ? _viewportWidth : display.W;
        int outputHeight =
            _viewportArea > 0 ? _viewportHeight : display.H;
        // World ownership is provenance-based, not tied to the usual 320x240
        // race viewport. GT2's wider transition and rotating Results-car views
        // contain complete captured screen primitives plus 3D provenance and
        // are rendered natively as long as they fit the bounded output pool.
        int targetOutputWidth = Math.Max(
            outputWidth,
            checked((outputHeight * settings.TargetAspectWidth +
                settings.TargetAspectHeight - 1) /
                settings.TargetAspectHeight));
        long requiredOutputBytes =
            (long)targetOutputWidth * settings.OutputScale *
            outputHeight * settings.OutputScale * 4;
        if (
            outputWidth <= 0 ||
            outputHeight <= 0 ||
            requiredOutputBytes > LiveWorldRenderer.MaxOutputBytes
        )
        {
#if OPENGT_RELEASE_PACKAGE
            throw new InvalidOperationException(
                "The required modern world frame exceeds the bounded " +
                $"output pool: viewport={outputWidth}x{outputHeight} " +
                $"scale={settings.OutputScale} " +
                $"requiredBytes={requiredOutputBytes} " +
                $"capacity={LiveWorldRenderer.MaxOutputBytes}. " +
                "The release build has no compositor fallback.");
#else
            if (!_reportedOversizeOutput)
            {
                _reportedOversizeOutput = true;
                Console.Error.WriteLine(
                    $"[Native-World] development compositor-only " +
                    $"viewport={outputWidth}x{outputHeight} " +
                    $"scale={settings.OutputScale} " +
                    $"requiredBytes={requiredOutputBytes} " +
                    $"capacity={LiveWorldRenderer.MaxOutputBytes}");
            }
            ResetCurrent();
            return;
#endif
        }
#if !OPENGT_RELEASE_PACKAGE
        _reportedOversizeOutput = false;
#endif
        WriteHeader(
            presentedFrame,
            inputPoll,
            in display,
            vramOffset,
            vramBytes,
            in camera);
        _writer!.Flush();
        int size = checked((int)captureLength);
        byte[] submitted = _buffer;
        _writer.Dispose();
        _stream.Dispose();
        _writer = null;
        _stream = null;
        _buffer = null;
        if (!_renderer.Submit(
            submitted,
            size,
            checked((int)residentInstanceOffset),
            _outputResidentTrackInstanceCount,
            settings))
        {
            Console.Error.WriteLine(
                $"[Native-World] submission rejected frame={presentedFrame} " +
                $"poll={inputPoll}");
        }
        RentAndReset();
    }

    void InjectAlignedStaticScene(int inputPoll)
    {
        if (_staticSceneInjected)
            return;
        _staticSceneInjected = true;

        uint vehicleGeneration = 0;
        int vehicleMaximum = 0;
        foreach (var entry in _vehicleGenerationCounts)
        {
            if (entry.Value <= vehicleMaximum)
                continue;
            vehicleMaximum = entry.Value;
            vehicleGeneration = entry.Key;
        }

        DeferredStaticScene? staticScene = vehicleGeneration == 0
            ? null
            : FindStaticScene(vehicleGeneration);
        if (vehicleGeneration == 0)
        {
            foreach (DeferredStaticScene candidate in _staticScenes)
            {
                if (candidate.Generation != 0 &&
                    (staticScene == null ||
                     candidate.Generation > staticScene.Generation))
                {
                    staticScene = candidate;
                }
            }
        }
        if (staticScene == null)
        {
            string available = string.Join(
                ',',
                _staticScenes
                    .Where(static scene => scene.Generation != 0)
                    .Select(static scene => scene.Generation)
                    .Order());
            Console.Error.WriteLine(
                $"[GT2-Scene-Alignment] poll={inputPoll} " +
                $"vehicleGeneration={vehicleGeneration} " +
                $"availableStaticGenerations={available} " +
                "result=no-exact-static-generation");
            _truncated = true;
            return;
        }

        byte[] staticTriangles = staticScene.Triangles;
        int staticTriangleCount = staticScene.TriangleCount;
        byte[] residentInstances = staticScene.ResidentInstances;
        int residentInstanceCount = staticScene.ResidentInstanceCount;
        var staticCameras = staticScene.Cameras;
        uint staticGeneration = staticScene.Generation;
        _selectedStaticGeneration = Math.Max(
            _selectedStaticGeneration,
            staticGeneration);

        int staticInsertIndex = checked((int)_triangleCount);
        if (staticTriangleCount >
            LiveWorldRenderer.MaxTriangles - staticInsertIndex)
        {
            _truncated = true;
            return;
        }
        int staticBytes = checked(
            staticTriangleCount * LiveWorldRenderer.TriangleStride);
        int streamOffset = checked((int)_stream!.Position);
        EnsureTriangleStorage(
            _stream,
            checked(streamOffset + staticBytes),
            LiveWorldRenderer.HeaderSize +
                LiveWorldRenderer.MaxTriangles *
                LiveWorldRenderer.TriangleStride);
        for (int index = 0; index < staticTriangleCount; index++)
        {
            ReadOnlySpan<byte> source = staticTriangles.AsSpan(
                index * LiveWorldRenderer.TriangleStride,
                LiveWorldRenderer.TriangleStride);
            Span<byte> destination = _buffer!.AsSpan(
                streamOffset + index * LiveWorldRenderer.TriangleStride,
                LiveWorldRenderer.TriangleStride);
            RetargetTriangleRecord(
                source,
                destination,
                _drawOffsetX,
                _drawOffsetY);
        }
        _stream.Position = streamOffset + staticBytes;
        _triangleCount += checked((uint)staticTriangleCount);
        _worldTriangleCount += checked((uint)staticTriangleCount);

        foreach (var entry in staticCameras)
            _trackCameras[entry.Key] = entry.Value;
#if !OPENGT_RELEASE_PACKAGE
        foreach (var entry in staticScene.TraceCounts)
        {
            _sceneGenerationCounts[(
                entry.Key.Kind,
                staticGeneration,
                entry.Key.Plane)] = entry.Value;
        }
#endif

        _outputResidentTrackInstanceCount = residentInstanceCount;
        for (int index = 0; index < residentInstanceCount; index++)
        {
            ReadOnlySpan<byte> source = residentInstances.AsSpan(
                index * LiveWorldRenderer.ResidentTrackInstanceStride,
                LiveWorldRenderer.ResidentTrackInstanceStride);
            Span<byte> destination = _alignedResidentTrackInstances.AsSpan(
                index * LiveWorldRenderer.ResidentTrackInstanceStride,
                LiveWorldRenderer.ResidentTrackInstanceStride);
            RetargetResidentInstanceRecord(
                source,
                destination,
                _drawOffsetX,
                _drawOffsetY,
                staticInsertIndex);
        }

#if !OPENGT_RELEASE_PACKAGE
        if (_scenePassTracePoll == inputPoll)
        {
            Console.Error.WriteLine(
                $"[GT2-Scene-Alignment] poll={inputPoll} " +
                $"vehicleGeneration={vehicleGeneration} " +
                $"staticGeneration={staticGeneration} " +
                "source=generation-cache " +
                $"triangles={staticTriangleCount} " +
                $"residentInstances={residentInstanceCount} " +
                $"insert={staticInsertIndex} result=aligned");
        }
#endif
    }


    static void RetargetTriangleRecord(
        ReadOnlySpan<byte> source,
        Span<byte> destination,
        int targetDrawOffsetX,
        int targetDrawOffsetY)
    {
        source.CopyTo(destination);
        int sourceDrawOffsetX =
            BinaryPrimitives.ReadInt16LittleEndian(source[44..]);
        int sourceDrawOffsetY =
            BinaryPrimitives.ReadInt16LittleEndian(source[46..]);
        int deltaX = targetDrawOffsetX - sourceDrawOffsetX;
        int deltaY = targetDrawOffsetY - sourceDrawOffsetY;
        RetargetInt16(destination, 12, deltaX);
        RetargetInt16(destination, 14, deltaY);
        RetargetInt16(destination, 16, deltaX);
        RetargetInt16(destination, 18, deltaY);
        BinaryPrimitives.WriteInt16LittleEndian(
            destination[44..], checked((short)targetDrawOffsetX));
        BinaryPrimitives.WriteInt16LittleEndian(
            destination[46..], checked((short)targetDrawOffsetY));
    }

    static void RetargetResidentInstanceRecord(
        ReadOnlySpan<byte> source,
        Span<byte> destination,
        int targetDrawOffsetX,
        int targetDrawOffsetY,
        int staticInsertIndex)
    {
        source.CopyTo(destination);
        int sourceDrawOffsetX =
            BinaryPrimitives.ReadInt16LittleEndian(source[84..]);
        int sourceDrawOffsetY =
            BinaryPrimitives.ReadInt16LittleEndian(source[86..]);
        int deltaX = targetDrawOffsetX - sourceDrawOffsetX;
        int deltaY = targetDrawOffsetY - sourceDrawOffsetY;
        RetargetInt16(destination, 68, deltaX);
        RetargetInt16(destination, 70, deltaY);
        RetargetInt16(destination, 72, deltaX);
        RetargetInt16(destination, 74, deltaY);
        BinaryPrimitives.WriteInt16LittleEndian(
            destination[84..], checked((short)targetDrawOffsetX));
        BinaryPrimitives.WriteInt16LittleEndian(
            destination[86..], checked((short)targetDrawOffsetY));
        int relativeInsert =
            BinaryPrimitives.ReadUInt16LittleEndian(source[94..]);
        BinaryPrimitives.WriteUInt16LittleEndian(
            destination[94..],
            checked((ushort)(staticInsertIndex + relativeInsert)));
    }

    static void RetargetInt16(
        Span<byte> destination,
        int offset,
        int delta)
    {
        int value = BinaryPrimitives.ReadInt16LittleEndian(
            destination[offset..]);
        BinaryPrimitives.WriteInt16LittleEndian(
            destination[offset..],
            checked((short)(value + delta)));
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
        // The stream retains its full fixed-capacity length; the explicit
        // cursor and submitted byte count define the valid capture extent.
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

    static bool SameCameraProjection(
        in GteProjectionOrigin left,
        in GteProjectionOrigin right) =>
        left.Valid && right.Valid &&
        left.Object.Kind == right.Object.Kind &&
        left.TransformId == right.TransformId &&
        (left.Object.Kind != WorldObjectKind.Track ||
            (left.ProjectionOffsetX == right.ProjectionOffsetX &&
             left.ProjectionOffsetY == right.ProjectionOffsetY &&
             left.ProjectionPlane == right.ProjectionPlane));

    static bool IsDeferredStaticScene(in GteProjectionOrigin origin) =>
        origin.Valid &&
        origin.Object.ScenePass == WorldScenePass.Main &&
        origin.Object.SceneGeneration != 0 &&
        (origin.Object.Kind == WorldObjectKind.Track ||
         origin.Object.Kind == WorldObjectKind.Background);

    void CountStaticTransforms(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c)
    {
        int aWeight = a.Valid ? 1 : 0;
        int bWeight = b.Valid ? 1 : 0;
        int cWeight = c.Valid ? 1 : 0;
        if (SameCameraProjection(in a, in b))
        {
            aWeight += bWeight;
            bWeight = 0;
        }
        if (SameCameraProjection(in a, in c))
        {
            aWeight += cWeight;
            cWeight = 0;
        }
        else if (SameCameraProjection(in b, in c))
        {
            bWeight += cWeight;
            cWeight = 0;
        }
        if (aWeight != 0)
            CountStaticTransform(in a, aWeight);
        if (bWeight != 0)
            CountStaticTransform(in b, bWeight);
        if (cWeight != 0)
            CountStaticTransform(in c, cWeight);
    }

    void CountStaticTransform(in GteProjectionOrigin origin, int weight)
    {
        if (!IsDeferredStaticScene(in origin) ||
            origin.Object.Kind != WorldObjectKind.Track)
        {
            return;
        }
        DeferredStaticScene scene = GetOrCreateStaticScene(
            origin.Object.SceneGeneration);
        var key = new CameraProjectionKey(
            origin.TransformId,
            origin.ProjectionOffsetX,
            origin.ProjectionOffsetY,
            origin.ProjectionPlane);
        ref var camera = ref CollectionsMarshal.GetValueRefOrAddDefault(
            scene.Cameras,
            key,
            out _);
        camera = (camera.Count + weight, origin);
    }

    void CountTransforms(
        in GteProjectionOrigin a,
        in GteProjectionOrigin b,
        in GteProjectionOrigin c)
    {
        int aWeight = a.Valid ? 1 : 0;
        int bWeight = b.Valid ? 1 : 0;
        int cWeight = c.Valid ? 1 : 0;
        if (SameCameraProjection(in a, in b))
        {
            aWeight += bWeight;
            bWeight = 0;
        }
        if (SameCameraProjection(in a, in c))
        {
            aWeight += cWeight;
            cWeight = 0;
        }
        else if (SameCameraProjection(in b, in c))
        {
            bWeight += cWeight;
            cWeight = 0;
        }
        if (aWeight != 0)
            CountTransform(in a, aWeight);
        if (bWeight != 0)
            CountTransform(in b, bWeight);
        if (cWeight != 0)
            CountTransform(in c, cWeight);
    }

    void CountTransform(in GteProjectionOrigin origin, int weight)
    {
        if (!origin.Valid ||
            origin.Object.ScenePass == WorldScenePass.Auxiliary)
            return;
        if (origin.Object.Kind == WorldObjectKind.Track)
        {
            var key = new CameraProjectionKey(
                origin.TransformId,
                origin.ProjectionOffsetX,
                origin.ProjectionOffsetY,
                origin.ProjectionPlane);
            ref var camera = ref CollectionsMarshal.GetValueRefOrAddDefault(
                _trackCameras,
                key,
                out _);
            camera = (camera.Count + weight, origin);
            return;
        }
        ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(
            _allTransforms,
            origin.TransformId,
            out _);
        entry = (entry.Count + weight, origin);
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
        ref uint identity = ref CollectionsMarshal.GetValueRefOrAddDefault(
            _sourceVertices,
            key,
            out bool exists);
        if (!exists)
            identity = 0x40000000u |
                checked((uint)_sourceVertices.Count);
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

#if !OPENGT_RELEASE_PACKAGE
    static int ParseDiagnosticPoll(string name) =>
        int.TryParse(Environment.GetEnvironmentVariable(name), out int poll)
            ? Math.Max(0, poll)
            : -1;

    void TraceSceneGenerations(int inputPoll)
    {
        if (_scenePassTracePoll < 0 || inputPoll != _scenePassTracePoll)
            return;
        foreach (var entry in _sceneGenerationCounts)
        {
            Console.Error.WriteLine(
                $"[GT2-Scene-Generation] poll={inputPoll} " +
                $"kind={entry.Key.Kind} " +
                $"generation={entry.Key.Generation} " +
                $"projectionPlane={entry.Key.Plane} " +
                $"triangles={entry.Value}");
        }
    }

    void TraceCameraTransform(
        int inputPoll,
        in GteProjectionOrigin camera)
    {
        if (_cameraTraceStartPoll < 0)
            return;
        int endPoll = _cameraTraceEndPoll >= 0
            ? Math.Max(_cameraTraceStartPoll, _cameraTraceEndPoll)
            : _cameraTraceStartPoll;
        if (inputPoll < _cameraTraceStartPoll || inputPoll > endPoll)
            return;
        Console.Error.WriteLine(
            $"[GT2-Camera-Transform] poll={inputPoll} " +
            $"object=0x{camera.Object.StableId:X8} " +
            $"model=0x{camera.Object.ModelPointer:X8} " +
            $"transform=0x{camera.TransformId:X16} " +
            $"rotation={camera.R00},{camera.R01},{camera.R02}/" +
            $"{camera.R10},{camera.R11},{camera.R12}/" +
            $"{camera.R20},{camera.R21},{camera.R22} " +
            $"translation={camera.TranslateX},{camera.TranslateY}," +
            $"{camera.TranslateZ} " +
            $"projection={camera.ProjectionOffsetX}," +
            $"{camera.ProjectionOffsetY},{camera.ProjectionPlane}");
    }
#endif

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    void WriteVertex(
        Span<byte> destination,
        ref int offset,
        in HleVertex vertex,
        in GteProjectionOrigin origin)
    {
        WriteVertex(
            destination,
            ref offset,
            in vertex,
            in origin,
            SourceIdentity(in origin));
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    static void WriteVertex(
        Span<byte> destination,
        ref int offset,
        in HleVertex vertex,
        in GteProjectionOrigin origin,
        uint sourceIdentity)
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
        WriteUInt32(destination, ref offset, sourceIdentity);
        WriteUInt64(destination, ref offset, origin.TransformId);
        WriteTransform(destination, ref offset, in origin);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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
        _stream.Position = LiveWorldRenderer.HeaderSize;
        ClearFrameState();
    }

    void ClearFrameState()
    {
        PruneConsumedStaticScenes();
        _trackCameras.Clear();
        _allTransforms.Clear();
        _sourceVertices.Clear();
        _screenLineRecordIndices.Clear();
        _vehicleGenerationCounts.Clear();
#if !OPENGT_RELEASE_PACKAGE
        _sceneGenerationCounts.Clear();
#endif
        _geometryFrame = 0;
        _triangleCount = 0;
        _worldTriangleCount = 0;
        _outputResidentTrackInstanceCount = 0;
        _staticSceneInjected = false;
        _staticSceneActivity = false;
        _truncated = false;
        _viewportX = _viewportY = 0;
        _viewportWidth = _viewportHeight = 0;
        _viewportArea = 0;
        _drawOffsetX = _drawOffsetY = 0;
    }

    void PruneConsumedStaticScenes()
    {
        if (_selectedStaticGeneration == 0)
            return;
        foreach (DeferredStaticScene scene in _staticScenes)
        {
            if (scene.Generation != 0 &&
                scene.Generation < _selectedStaticGeneration)
            {
#if !OPENGT_RELEASE_PACKAGE
                int poll = Host.InputManager.CurrentPoll;
                if (_staticSceneTraceStartPoll >= 0 &&
                    poll >= _staticSceneTraceStartPoll &&
                    (_staticSceneTraceEndPoll < 0 ||
                     poll <= _staticSceneTraceEndPoll))
                {
                    Console.Error.WriteLine(
                        $"[GT2-Static-Scene] poll={poll} action=prune " +
                        $"generation={scene.Generation} " +
                        $"selected={_selectedStaticGeneration}");
                }
#endif
                scene.Reset();
            }
        }
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
