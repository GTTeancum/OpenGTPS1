using RecompOne.Runtime.Context;
using RecompOne.Runtime.Dispatch;
using RecompOne.Runtime.Memory;

namespace RecompOne.Runtime.Sdk;

public static class LibCd
{
    const byte Nop = 0x01, 
        Setloc = 0x02,
        Play = 0x03,
        ReadN = 0x06, 
        Stop = 0x08,
        Pause = 0x09,
        Init = 0x0A,
        Mute = 0x0B, 
        Demute = 0x0C, 
        Setfilter = 0x0D,
        Setmode = 0x0E,
        GetlocL = 0x10,
        GetlocP = 0x11,
        GetTN = 0x13,
        GetTD = 0x14,
        SeekL = 0x15,
        SeekP = 0x16,
        ReadS = 0x1B;

    const int Complete = 0x02;
    const int DataReady = 0x01;
    const byte ModeSize1 = 0x20, ModeSize0 = 0x10;

    const byte StatMotor = 0x02;
    const byte StatRead = 0x20;
    const byte StatSeek = 0x40;
    const byte StatPlay = 0x80;
    static byte _status;
    static byte _mode;
    static byte _com;
    static readonly byte[] _pos = new byte[4];
    static readonly byte[] _lastResult = new byte[8];
    static readonly object _locatedFileGate = new();
    static readonly Dictionary<int, (int EndLba, string Name)> _locatedFiles = new();
    static int _lastIntr = Complete;
    static int _sectorReadOffset;
    static byte[]? _callbackSectorData;
    static int _callbackSectorLba = -1;
    static int _lastReadStartLba = -1;
    static int _sameReadStartCount;
    static bool _traceCurrentReadStart;

    static uint Gt2Drive => GT2Compat.CdDriveStateAddress;

    static uint _cbSync;
    static uint _cbReady;
    static uint _cbData;
    sealed record PendingSync(byte Command, byte[] Result, uint ResultAddress);
    static readonly Queue<PendingSync> _pendingSync = new();

    static bool _readActive;
    static bool _xaActive;
    static int _readSSectorPhase;
    static readonly byte[] _readSSectorBuffer = new byte[2336];
    static int _readSSectorBufferLba = -1;
    static int _readSFileEndLba = int.MaxValue;
    static int _xaReportLba = -1;
    static int _xaPendingReportLba = -1;
    static int _xaLastTraceSecond = -1;
    static volatile bool _cddaActive;
    static int _cddaLba;
    static int _cddaPendingReportLba = -1;
    static volatile int _cddaTrackNumber;
    static int _cddaLastReportSecond = -1;
    static byte _filterFile;
    static byte _filterChannel;
    static byte _cdMixLl = 0x80;
    static byte _cdMixLr;
    static byte _cdMixRr = 0x80;
    static byte _cdMixRl;
    static bool _cdMuted;
    static int _v8FileStartLba = -1;
    static int _v82FileStartLba = -1;
    static readonly bool TraceAudio =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_AUDIO") == "1";
    static readonly bool TraceCd =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_CD") == "1";
    static readonly bool TracePerformance =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_PERFORMANCE") == "1";
    static long _performanceTicks;
    static long _performanceAllocatedBytes;
    static long _performanceSectorAllocatedBytes;
    static long _performanceDecodeAllocatedBytes;
    static long _performanceXaReportAllocatedBytes;
    static long _performanceXaSnapshotAllocatedBytes;
    static long _performanceXaCallbackAllocatedBytes;
    static int _sectorTraceCount;
    static int _gt2RawReadyTraceCount;

    internal static readonly object DiscLock = new();
    static readonly object _posGate = new();

    static Thread? _xaThread;
    static volatile bool _xaRun;

    static readonly bool[] NeedsLoc = BuildNeedsLoc();

    static bool[] BuildNeedsLoc()
    {
        var t = new bool[32];
        t[Play] = t[ReadN] = t[SeekL] = t[SeekP] = t[ReadS] = true;
        return t;
    }

    public static void CdInit(CpuContext c, IMemory m)
    {
        CdResetState();
        c.V0 = CdInitInternal() ? 0u : 1u;
    }

    public static void CdReset(CpuContext c, IMemory m)
    {
        CdResetState();
        c.V0 = CdInitInternal() ? 1u : 0u;
    }

    public static void CdControl(CpuContext c, IMemory m) => c.V0 = (uint)(CommandWait(c, m, (byte)c.A0, c.A1, c.A2, 0) == 0 ? 1 : 0);
    public static void CdControlF(CpuContext c, IMemory m) => c.V0 = (uint)(CommandWait(c, m, (byte)c.A0, c.A1, 0, 1) == 0 ? 1 : 0);

    public static void CdControlB(CpuContext c, IMemory m)
    {
        if (CommandWait(c, m, (byte)c.A0, c.A1, c.A2, 0) != 0) { c.V0 = 0; return; }
        c.V0 = (uint)(SyncResult(m, c.A2) == Complete ? 1 : 0);
    }

    public static void CdSync(CpuContext c, IMemory m)
 => c.V0 = (uint)SyncResult(m, c.A1);

    public static void CdReady(CpuContext c, IMemory m)
    {
        if (c.A1 != 0) WriteResult(m, c.A1);
        c.V0 = (uint)_lastIntr;
    }

    public static void CdRead(CpuContext c, IMemory m)
    {
        int sectors = (int)c.A0;
        uint buf = c.A1;
        _mode = (byte)c.A2;
        int lba = CurrentLba;
        int size = SectorSize(_mode);
        Dispatcher.LoadByLba(lba);
        Log.Sdk($"CdRead sectors={sectors} buf=0x{buf:X8} mode=0x{_mode:X2} lba={lba} size={size}");

        for (int i = 0; i < sectors; i++)
        {
            Dispatcher.LoadByLba(lba + i);
            byte[] data;
            lock (DiscLock) data = Runtime.Cd!.ReadSectorData(lba + i, size);
            for (int j = 0; j < data.Length; j++)
                m.WriteU8(buf + (uint)(i * size + j), data[j]);
        }
        _lastIntr = Complete;
        c.V0 = 1;
    }

    internal static int CurrentLba { get { lock (_posGate) return PosToInt(_pos); } }
    internal static double SectorsPerSecond => (_mode & 0x80) != 0 ? 150.0 : 75.0; //cd pacer

    internal static bool AcceptXaSector(byte file, byte channel) =>
        (_mode & 0x08) == 0 || (file == _filterFile && channel == _filterChannel);

    internal static void ReportXaSector(int lba) =>
        Interlocked.Exchange(ref _xaPendingReportLba, lba);

    internal static void MixCdInput(short left, short right, out int mixedLeft, out int mixedRight)
    {
        if (_cdMuted)
        {
            mixedLeft = mixedRight = 0;
            return;
        }
        mixedLeft = Math.Clamp((left * _cdMixLl + right * _cdMixRl) >> 7, short.MinValue, short.MaxValue);
        mixedRight = Math.Clamp((left * _cdMixLr + right * _cdMixRr) >> 7, short.MinValue, short.MaxValue);
    }

    internal static bool TryDescribeLocatedFile(int lba, out string name, out int endLba)
    {
        lock (_locatedFileGate)
        {
            foreach (var (start, file) in _locatedFiles)
            {
                if (lba < start || lba >= file.EndLba) continue;
                name = file.Name;
                endLba = file.EndLba;
                return true;
            }
        }

        if (Runtime.Cd?.Fs.TryDescribeLba(lba, out name, out endLba) == true)
            return true;

        name = $"LBA {lba}";
        endLba = int.MaxValue;
        return false;
    }

    internal static void Tick()
    {
        if (!TracePerformance)
        {
            TickCore();
            return;
        }

        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        TickCore();
        _performanceAllocatedBytes +=
            GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        if (++_performanceTicks < 300)
            return;
        Console.Error.WriteLine(
            $"[PERF-CD] alloc={_performanceAllocatedBytes / 1024.0:F0}KiB " +
            $"sector={_performanceSectorAllocatedBytes / 1024.0:F0}KiB " +
            $"decode={_performanceDecodeAllocatedBytes / 1024.0:F0}KiB " +
            $"xaReport={_performanceXaReportAllocatedBytes / 1024.0:F0}KiB " +
            $"xaSnapshot={_performanceXaSnapshotAllocatedBytes / 1024.0:F0}KiB " +
            $"xaCallback={_performanceXaCallbackAllocatedBytes / 1024.0:F0}KiB");
        _performanceTicks = 0;
        _performanceAllocatedBytes = 0;
        _performanceSectorAllocatedBytes = 0;
        _performanceDecodeAllocatedBytes = 0;
        _performanceXaReportAllocatedBytes = 0;
        _performanceXaSnapshotAllocatedBytes = 0;
        _performanceXaCallbackAllocatedBytes = 0;
    }

    static void TickCore()
    {
        long started = TracePerformance ? System.Diagnostics.Stopwatch.GetTimestamp() : 0;
        DispatchPendingSync();
        long afterSync = TracePerformance ? System.Diagnostics.Stopwatch.GetTimestamp() : 0;
        DispatchXaReport();
        long afterXaReport = TracePerformance ? System.Diagnostics.Stopwatch.GetTimestamp() : 0;
        DispatchCddaReport();
        long afterCddaReport = TracePerformance ? System.Diagnostics.Stopwatch.GetTimestamp() : 0;
        bool xaMode = (_mode & 0x40) != 0;
        int servicedSectors = 0;
        bool servicedReadN = false;

        if (_readActive && xaMode)
        {
            // ReadS without a PsyQ STR ring is GT2's XA/sector-report path.
            // Service it on the guest thread so tight command waits can still
            // receive deterministic data-ready interrupts.
            if (_xaActive && !LibCdStream.InUse)
            {
                // A single-speed PlayStation CD-ROM delivers 75 sectors per
                // second, not one sector per VBlank. NTSC VBlank is
                // 60000/1001 Hz rather than exactly 60 Hz; using 75/60 is
                // short by 0.075 sector/sec and slowly drains an otherwise
                // healthy XA buffer during a race. Accumulate the exact
                // rational ratio, 75 / (60000/1001).
                _readSSectorPhase += 75 * 1001;
                int sectors = _readSSectorPhase / 60000;
                _readSSectorPhase %= 60000;
                for (int i = 0; i < sectors; i++)
                    ServiceReadSOnce();
                servicedSectors = sectors;
            }
        }
        else if (_readActive && _pendingSync.Count == 0 &&
            (_cbReady != 0 || _cbData != 0))
        {
            ServiceReadOnce();
            servicedReadN = true;
        }

        if (TracePerformance)
        {
            long finished = System.Diagnostics.Stopwatch.GetTimestamp();
            double scale = 1000.0 / System.Diagnostics.Stopwatch.Frequency;
            double totalMs = (finished - started) * scale;
            if (totalMs >= 10.0)
            {
                Console.Error.WriteLine(
                    $"[PERF-CD-LONG] totalMs={totalMs:F3} " +
                    $"syncMs={(afterSync - started) * scale:F3} " +
                    $"xaReportMs={(afterXaReport - afterSync) * scale:F3} " +
                    $"cddaReportMs={(afterCddaReport - afterXaReport) * scale:F3} " +
                    $"serviceMs={(finished - afterCddaReport) * scale:F3} " +
                    $"readActive={_readActive} xaMode={xaMode} xaActive={_xaActive} " +
                    $"readSectors={servicedSectors} readN={servicedReadN} " +
                    $"pendingSync={_pendingSync.Count} " +
                    $"callbacks=0x{_cbSync:X8}/0x{_cbReady:X8}/0x{_cbData:X8} " +
                    $"lba={CurrentLba}");
            }
        }
    }

    /// <summary>
    /// Some PsyQ command queues defer a script selected by a data-ready
    /// callback until the active ReadN command reports completion. The
    /// hardware supplies that second interrupt asynchronously; static guest
    /// execution must ask the virtual drive to schedule it explicitly.
    /// </summary>
    internal static bool QueueReadCompletion()
    {
        if (!_readActive || _cbSync == 0 || _pendingSync.Count != 0)
            return false;

        _lastIntr = Complete;
        _lastResult[0] = _status;
        for (int i = 1; i < _lastResult.Length; i++)
            _lastResult[i] = 0;
        _pendingSync.Enqueue(new PendingSync(ReadN, [.. _lastResult], 0));
        return true;
    }

    public static void ServiceReadOnce()
    {
        bool xaMode = (_mode & 0x40) != 0;
        if (!_readActive || xaMode || (_cbReady == 0 && _cbData == 0))
            return;

        var c = Runtime.Cpu;
        var m = Runtime.Mem;
        if (c == null || m == null) return;

        const uint resultAddress = 0x8000FF20u;
        var snap = c.Snapshot();
        _lastIntr = DataReady;
        WriteResult(m, resultAddress);
        int rawTrace = _gt2RawReadyTraceCount++;
        bool traceRawReady = TraceCd && _traceCurrentReadStart &&
            (_mode & ModeSize1) != 0 &&
            (rawTrace < 12 || (rawTrace & 0x3FFF) == 0);
        uint gt2Drive = Gt2Drive;
        bool gt2TransferWasActive =
            _cbReady == GT2Compat.CdReadyCallbackAddress &&
            m.ReadU8(gt2Drive + 0x167u) != 0;
        if (gt2TransferWasActive &&
            m.ReadU32(gt2Drive + 0x4Cu) ==
                GT2Compat.CdFiniteReadHandlerAddress)
        {
            uint expected = m.ReadU32(gt2Drive + 0x44u);
            uint end = m.ReadU32(gt2Drive + 0x48u);
            uint current = (uint)CurrentLba;
            ushort remainingSectors = m.ReadU16(0x801C9512u);

            // GT2 can arm the next finite transfer while the previous
            // completion script is still retiring. In that case a stale
            // physical drive position can survive the guest's requested
            // callback range. A position beyond that entire range can never
            // match by reading forward, so honor the guest's authoritative
            // expected sector and seek the virtual drive back to it.
            if (current > end && remainingSectors != 0)
            {
                lock (_posGate)
                {
                    IntToPos(
                        checked((int)expected),
                        out _pos[0],
                        out _pos[1],
                        out _pos[2]);
                }
                Dispatcher.LoadByLba(CurrentLba);
                if (TraceCd)
                {
                    Console.Error.WriteLine(
                        $"[LibCd] corrected stale GT2 drive position " +
                        $"{current} to requested range {expected}-{end}");
                }
            }
        }

        int readyLba = CurrentLba;
        int readySize = SectorSize(_mode);
        byte[] readyData;
        lock (DiscLock)
            readyData = Runtime.Cd!.ReadSectorData(readyLba, readySize);
        byte[]? previousCallbackData = _callbackSectorData;
        int previousCallbackLba = _callbackSectorLba;
        int previousSectorReadOffset = _sectorReadOffset;
        _callbackSectorData = readyData;
        _callbackSectorLba = readyLba;
        _sectorReadOffset = 0;

        // The drive has consumed this sector before it raises DataReady. Move
        // its free-running position first. If the guest callback starts a new
        // Setloc/ReadN command, that new position must win; incrementing after
        // the callback corrupts the freshly requested location.
        AdvancePos(1);
        Dispatcher.LoadByLba(CurrentLba);

        if (traceRawReady)
        {
            Console.Error.WriteLine(
                $"[LibCd] raw-ready before lba={readyLba} drive={CurrentLba} mode=0x{_mode:X2} " +
                $"handler=0x{m.ReadU32(gt2Drive + 0x4Cu):X8} " +
                $"current={m.ReadU32(gt2Drive + 0x60u)} end={m.ReadU32(gt2Drive + 0x48u)} " +
                $"dest=0x{m.ReadU32(gt2Drive + 0x50u):X8} remaining={m.ReadU32(gt2Drive + 0x54u)} " +
                $"headerMode={m.ReadU8(gt2Drive + 0x163u)} stop={m.ReadU8(gt2Drive + 0x16Au)}");
        }
        try
        {
            if (_cbReady != 0)
            {
                c.A0 = DataReady;
                c.A1 = resultAddress;
                Dispatcher.Call(c, m, _cbReady);
            }
            if (gt2TransferWasActive &&
                m.ReadU8(gt2Drive + 0x167u) == 0 &&
                QueueReadCompletion())
            {
                Console.Error.WriteLine(
                    $"[LibCd] queued GT2 ReadN completion at LBA {readyLba}");
            }
            if (traceRawReady)
            {
                Console.Error.WriteLine(
                    $"[LibCd] raw-ready after lba={readyLba} drive={CurrentLba} " +
                    $"active={_readActive} current={m.ReadU32(gt2Drive + 0x60u)} " +
                    $"end={m.ReadU32(gt2Drive + 0x48u)} " +
                    $"dest=0x{m.ReadU32(gt2Drive + 0x50u):X8} " +
                    $"remaining={m.ReadU32(gt2Drive + 0x54u)} " +
                    $"busy={m.ReadU8(gt2Drive + 0x166u)} stop={m.ReadU8(gt2Drive + 0x16Au)}");
            }
            if (_cbData != 0)
            {
                c.A0 = DataReady;
                c.A1 = resultAddress;
                Dispatcher.Call(c, m, _cbData);
            }
        }
        finally
        {
            c.Restore(snap);
            _callbackSectorData = previousCallbackData;
            _callbackSectorLba = previousCallbackLba;
            _sectorReadOffset = previousSectorReadOffset;
        }
    }

    static void EnsureXaThread()
    {
        if (_xaThread is { IsAlive: true }) return;
        _xaRun = true;
        _xaThread = new Thread(XaLoop) { IsBackground = true, Name = "CdXa" };
        _xaThread.Start();
    }

    static void XaLoop()
    {
        while (_xaRun)
        {
            if (_cddaActive && Runtime.Cd != null)
                PumpCdda();
            else if (_readActive && !_xaActive &&
                (_mode & 0x40) != 0 && Runtime.Cd != null)
                PumpXa();
            Thread.Sleep(2);
        }
    }

    static void PumpCdda()
    {
        var cd = Runtime.Cd;
        if (cd == null) return;
        const int MinBufferFrames = 4096;
        const int MaxScan = 16;
        int scanned = 0;

        while (_cddaActive && CddaAudio.BufferedFrames < MinBufferFrames && scanned++ < MaxScan)
        {
            int lba = _cddaLba;
            byte[] sector;
            int trackNumber;
            int trackEndLba;
            lock (DiscLock)
            {
                if (!cd.TryReadAudioSector(lba, out sector, out trackNumber, out trackEndLba))
                {
                    _cddaActive = false;
                    Console.Error.WriteLine($"[CDDA] stopped at unmapped audio LBA {lba}");
                    return;
                }
            }

            _cddaTrackNumber = trackNumber;
            CddaAudio.QueueSector(sector, trackNumber, lba);
            _cddaLba = lba + 1;
            int reportSecond = lba / 75;
            if (reportSecond != _cddaLastReportSecond)
            {
                _cddaLastReportSecond = reportSecond;
                Interlocked.Exchange(ref _cddaPendingReportLba, lba);
            }
            if (lba + 1 >= trackEndLba) return;
        }
    }

    static void DispatchCddaReport()
    {
        int lba = Interlocked.Exchange(ref _cddaPendingReportLba, -1);
        if (lba < 0 || _cbReady == 0 || Runtime.Cpu == null || Runtime.Mem == null) return;

        IntToPos(lba, out byte mm, out byte ss, out byte ff);
        _lastResult[0] = _status;
        _lastResult[1] = ToBcd(_cddaTrackNumber);
        _lastResult[2] = 1;
        _lastResult[3] = mm;
        _lastResult[4] = ss;
        _lastResult[5] = ff;
        _lastResult[6] = mm;
        _lastResult[7] = ss;

        var c = Runtime.Cpu;
        var m = Runtime.Mem;
        const uint resultAddress = 0x8000FF00u;
        WriteResult(m, resultAddress);
        var snap = c.Snapshot();
        c.A0 = DataReady;
        c.A1 = resultAddress;
        Dispatcher.Call(c, m, _cbReady);
        c.Restore(snap);
    }

    static void DispatchXaReport()
    {
        int lba = Interlocked.Exchange(ref _xaPendingReportLba, -1);
        if (lba < 0 || !_xaActive || _cbReady == 0 || Runtime.Cpu == null || Runtime.Mem == null)
            return;

        long allocatedBefore = TracePerformance
            ? GC.GetAllocatedBytesForCurrentThread()
            : 0;
        _xaReportLba = lba;
        int traceSecond = lba / 75;
        if (TraceAudio && traceSecond != _xaLastTraceSecond)
        {
            _xaLastTraceSecond = traceSecond;
            Console.Error.WriteLine($"[XA-REPORT] lba={lba} ready=0x{_cbReady:X8}");
        }
        var c = Runtime.Cpu;
        var m = Runtime.Mem;
        IntToPos(lba, out byte mm, out byte ss, out byte ff);
        _lastResult[0] = _status;
        _lastResult[1] = 0;
        _lastResult[2] = 0;
        _lastResult[3] = mm;
        _lastResult[4] = ss;
        _lastResult[5] = ff;
        _lastResult[6] = 0;
        _lastResult[7] = 0;
        const uint resultAddress = 0x8000FF08u;
        WriteResult(m, resultAddress);
        byte[]? previousCallbackData = _callbackSectorData;
        int previousCallbackLba = _callbackSectorLba;
        int previousSectorReadOffset = _sectorReadOffset;
        if (_readSSectorBufferLba == lba)
        {
            _callbackSectorData = _readSSectorBuffer;
            _callbackSectorLba = lba;
        }
        _sectorReadOffset = 0;
        long allocatedBeforeSnapshot = TracePerformance
            ? GC.GetAllocatedBytesForCurrentThread()
            : 0;
        var snap = c.Snapshot();
        long allocatedAfterSnapshot = TracePerformance
            ? GC.GetAllocatedBytesForCurrentThread()
            : 0;
        _lastIntr = DataReady;
        c.A0 = DataReady;
        c.A1 = resultAddress;
        if (TracePerformance)
            Dispatcher.BeginAllocationScope();
        try
        {
            Dispatcher.Call(c, m, _cbReady);
        }
        finally
        {
            _callbackSectorData = previousCallbackData;
            _callbackSectorLba = previousCallbackLba;
            _sectorReadOffset = previousSectorReadOffset;
        }
        long allocatedAfterCallback = TracePerformance
            ? GC.GetAllocatedBytesForCurrentThread()
            : 0;
        if (TracePerformance)
            Dispatcher.EndAllocationScope();
        c.Restore(snap);
        if (TracePerformance)
        {
            _performanceXaSnapshotAllocatedBytes +=
                allocatedAfterSnapshot - allocatedBeforeSnapshot;
            _performanceXaCallbackAllocatedBytes +=
                allocatedAfterCallback - allocatedAfterSnapshot;
            _performanceXaReportAllocatedBytes +=
                GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        }
    }

    static void PumpXa()
    {
        if (Runtime.Cd == null) return;
        const int MinBuffer = 4096;
        const int MaxScan = 32;
        bool useFilter = (_mode & 0x08) != 0;
        int scanned = 0;

        while (_readActive && XaAudio.BufferedSamples < MinBuffer && scanned < MaxScan)
        {
            int lba = CurrentLba;
            if (lba < 0) break;
            byte[] sec;
            lock (DiscLock) sec = Runtime.Cd.ReadSectorData(lba, 2336);
            AdvancePos(1);
            scanned++;
            if ((sec[2] & 0x04) == 0) continue;
            if (useFilter && (sec[0] != _filterFile || sec[1] != _filterChannel)) continue;
            XaAudio.DecodeSector(sec, 8, sec[3]);
        }
    }

    static void ServiceReadSOnce()
    {
        if (!_readActive || !_xaActive || Runtime.Cd == null)
            return;

        int lba = CurrentLba;
        if (lba >= _readSFileEndLba)
        {
            _status = (byte)(_status & ~StatRead);
            _lastIntr = Complete;
            _lastResult[0] = _status;
            for (int i = 1; i < _lastResult.Length; i++)
                _lastResult[i] = 0;
            if (_cbSync != 0)
            {
                _pendingSync.Enqueue(
                    new PendingSync(ReadS, [.. _lastResult], 0));
            }
            _readActive = false;
            _xaActive = false;
            _readSSectorPhase = 0;
            OggMusic.SetMusicStreamActive(false);
            LibCdStream.OnStopStream();
            if (TraceCd)
            {
                Console.Error.WriteLine(
                    $"[LibCd] ReadS reached file boundary LBA={lba}");
            }
            return;
        }
        bool replaceMusic =
            Runtime.Cd.IsMusicLba(lba) &&
            OggMusic.HasTracks;
        OggMusic.SetMusicStreamActive(replaceMusic);
        byte[] sector = _readSSectorBuffer;
        long allocatedBeforeSector = TracePerformance
            ? GC.GetAllocatedBytesForCurrentThread()
            : 0;
        lock (DiscLock)
            Runtime.Cd.ReadSectorData(lba, sector);
        _readSSectorBufferLba = lba;
        if (TracePerformance)
            _performanceSectorAllocatedBytes +=
                GC.GetAllocatedBytesForCurrentThread() - allocatedBeforeSector;

        if (!replaceMusic &&
            (sector[2] & 0x04) != 0 &&
            AcceptXaSector(sector[0], sector[1]) &&
            XaAudio.BufferedSamples < 8192)
        {
            long allocatedBeforeDecode = TracePerformance
                ? GC.GetAllocatedBytesForCurrentThread()
                : 0;
            XaAudio.DecodeSector(
                sector, 8, sector[3], lba, sector[0], sector[1]);
            if (TracePerformance)
                _performanceDecodeAllocatedBytes +=
                    GC.GetAllocatedBytesForCurrentThread() -
                    allocatedBeforeDecode;
        }

        AdvancePos(1);
        ReportXaSector(lba);
    }

    static void AdvancePos(int n)
    {
        lock (_posGate)
            IntToPos(PosToInt(_pos) + n, out _pos[0], out _pos[1], out _pos[2]);
    }

    public static void CdReadSync(CpuContext c, IMemory m)
    {
        if (c.A1 != 0) WriteResult(m, c.A1);
        c.V0 = 0;
    }

    public static void CdGetSector(CpuContext c, IMemory m)
    {
        uint madr = c.A0;
        int words = (int)c.A1;
        // The retail XA-ready callback asks for exactly one word and treats it
        // as the current absolute MSF. Preserve normal sector payload reads.
        if (_xaActive && words == 1 && _xaReportLba >= 0)
        {
            IntToPos(_xaReportLba, out byte mm, out byte ss, out byte ff);
            m.WriteU8(madr, mm);
            m.WriteU8(madr + 1, ss);
            m.WriteU8(madr + 2, ff);
            m.WriteU8(madr + 3, 0);
            c.V0 = 1;
            return;
        }
        int lba;
        byte[] data;
        int sectorSize;
        if (_callbackSectorData != null)
        {
            // CdGetSector drains the CD FIFO belonging to the DataReady
            // interrupt currently being serviced. A callback may issue a new
            // Play/ReadS/ReadN command before draining that FIFO; selecting
            // data from the new global command returns an unrelated XA sector.
            lba = _callbackSectorLba;
            data = _callbackSectorData;
            sectorSize = data.Length;
        }
        else
        {
            lba = _xaActive && _xaReportLba >= 0
                ? _xaReportLba
                : CurrentLba;
            sectorSize = SectorSize(_mode);
            lock (DiscLock)
                data = Runtime.Cd!.ReadSectorData(lba, sectorSize);
        }
        int start = Math.Clamp(_sectorReadOffset, 0, data.Length);
        int bytes = Math.Min(data.Length - start, words * 4);
        for (int j = 0; j < bytes; j++)
            m.WriteU8(madr + (uint)j, data[start + j]);
        _sectorReadOffset = start + bytes;
        int traceIndex = _sectorTraceCount++;
        if (TraceCd && (traceIndex < 16 || (traceIndex & 0xFF) == 0))
        {
            string prefix = Convert.ToHexString(
                data.AsSpan(start, Math.Min(32, data.Length - start)));
            Console.Error.WriteLine(
                $"[LibCd] CdGetSector LBA={lba} dest=0x{madr:X8} words={words} " +
                $"size={sectorSize} offset={start}->{_sectorReadOffset} data={prefix}");
        }
        c.V0 = 1;
    }

    public static void CdDataSync(CpuContext c, IMemory m) => c.V0 = 0;

    public static void CdSearchFile(CpuContext c, IMemory m)
    {
        uint fp = c.A0;
        string name = ReadCString(m, c.A1);

        if (Runtime.Cd == null || !Runtime.Cd.Fs.Locate(name, out int lba, out uint size))
        {
            Log.Sdk($"CdSearchFile '{name}'wasnt found");
            c.V0 = 0;
            return;
        }
        Log.Sdk($"CdSearchFile '{name}' lba={lba} size={size}");
        lock (_locatedFileGate)
            _locatedFiles[lba] = (lba + (int)((size + 2047u) >> 11), name);

        IntToPos(lba, out byte mm, out byte ss, out byte ff);
        m.WriteU8(fp + 0, mm);
        m.WriteU8(fp + 1, ss);
        m.WriteU8(fp + 2, ff);
        m.WriteU8(fp + 3, 0);
        m.WriteU32(fp + 4, size);

        int slash = name.LastIndexOfAny(['/', '\\']);
        string basename = slash >= 0 ? name[(slash + 1)..] : name;
        
        for (int i = 0; i < 16; i++)
        {
            m.WriteU8(fp + 8 + (uint)i, i < basename.Length ? (byte)basename[i] : (byte)0);
        }

        c.V0 = fp;
    }

    public static void CdSyncCallback(CpuContext c, IMemory m)
    {
        c.V0 = _cbSync;
        _cbSync = c.A0;
        if (TraceCd) Console.Error.WriteLine($"[LibCd] sync callback=0x{_cbSync:X8}");
    }
    public static void CdReadyCallback(CpuContext c, IMemory m)
    {
        c.V0 = _cbReady;
        _cbReady = c.A0;
        if (TraceCd) Console.Error.WriteLine($"[LibCd] ready callback=0x{_cbReady:X8}");
    }
    public static void CdReadCallback(CpuContext c, IMemory m) { c.V0 = _cbData; _cbData = c.A0; }
    public static void CdDataCallback(CpuContext c, IMemory m) { c.V0 = _cbData; _cbData = c.A0; }

    public static void CdStatus(CpuContext c, IMemory m) => c.V0 = _status;
    public static void CdMode(CpuContext c, IMemory m) => c.V0 = _mode;
    public static void CdLastCom(CpuContext c, IMemory m) => c.V0 = _com;
    public static void CdMix(CpuContext c, IMemory m)
    {
        if (c.A0 != 0)
        {
            _cdMixLl = m.ReadU8(c.A0);
            _cdMixLr = m.ReadU8(c.A0 + 1);
            _cdMixRr = m.ReadU8(c.A0 + 2);
            _cdMixRl = m.ReadU8(c.A0 + 3);
            if (TraceAudio)
                Console.Error.WriteLine(
                    $"[CDMIX] ll={_cdMixLl} lr={_cdMixLr} rr={_cdMixRr} rl={_cdMixRl}");
        }
        c.V0 = 1;
    }

    static void CdResetState()
    {
        LibCdStream.OnStopStream();
        _status = StatMotor; //drive aways spin
        _mode = 0;
        _com = 0;
        _lastIntr = Complete;
        _sectorReadOffset = 0;
        _cbSync = _cbReady = _cbData = 0;
        _pendingSync.Clear();
        _readActive = false;
        _xaActive = false;
        _readSFileEndLba = int.MaxValue;
        _xaReportLba = -1;
        _xaPendingReportLba = -1;
        _xaLastTraceSecond = -1;
        _cddaActive = false;
        _cddaLba = 0;
        _cddaPendingReportLba = -1;
        _cddaTrackNumber = 0;
        _cddaLastReportSecond = -1;
        CddaAudio.Reset();
        XaAudio.Reset();
        _filterFile = _filterChannel = 0;
        _cdMixLl = _cdMixRr = 0x80;
        _cdMixLr = _cdMixRl = 0;
        _cdMuted = false;
        _v8FileStartLba = _v82FileStartLba = -1;
        _sectorTraceCount = 0;
        _gt2RawReadyTraceCount = 0;
        _callbackSectorData = null;
        _callbackSectorLba = -1;
        _readSSectorBufferLba = -1;
        _lastReadStartLba = -1;
        _sameReadStartCount = 0;
        _traceCurrentReadStart = false;
        Array.Clear(_pos);
        Array.Clear(_lastResult);
        Dispatcher.ClearPending();
    }

    static bool CdInitInternal()
    {
        _lastIntr = Complete;
        _lastResult[0] = _status;
        return true;
    }

    static int CommandWait(
        CpuContext c, IMemory m, byte com, uint param, uint result, uint arg)
    {
        if (param != 0 && com < NeedsLoc.Length && NeedsLoc[com])
            ExecCommand(c, m, Setloc, param, 0, notify: false);
        return ExecCommand(c, m, com, param, result);
    }

    static int ExecCommand(
        CpuContext c, IMemory m, byte com, uint param, uint result,
        bool notify = true)
    {
        _com = com;
        _lastIntr = Complete;
        Log.Sdk($"Cd cmd 0x{com:X2} param=0x{param:X8} pos={_pos[0]:X2}:{_pos[1]:X2}:{_pos[2]:X2}");

        switch (com)
        {
            case Setloc:
                if (param != 0)
                    lock (_posGate)
                        for (int i = 0; i < 4; i++) _pos[i] = m.ReadU8(param + (uint)i);
                break;
            case Setmode:
                if (param != 0) _mode = m.ReadU8(param);
                break;
            case Setfilter:
                if (param != 0) { _filterFile = m.ReadU8(param); _filterChannel = m.ReadU8(param + 1); }
                break;
            case ReadN:
                _status = (byte)((_status | StatMotor | StatRead) &
                    ~(StatSeek | StatPlay));
                _cddaActive = false;
                CddaAudio.Reset();
                XaAudio.Reset();
                _xaActive = false;
                _xaReportLba = -1;
                _xaPendingReportLba = -1;
                _xaLastTraceSecond = -1;
                _readActive = true;
                _gt2RawReadyTraceCount = 0;
                int readLba = CurrentLba;
                if (readLba == _lastReadStartLba)
                    _sameReadStartCount++;
                else
                {
                    _lastReadStartLba = readLba;
                    _sameReadStartCount = 1;
                }
                _traceCurrentReadStart =
                    _sameReadStartCount <= 4 ||
                    (_sameReadStartCount & (_sameReadStartCount - 1)) == 0;
                Dispatcher.LoadByLba(readLba);
                if (_traceCurrentReadStart)
                {
                    Console.Error.WriteLine(
                        $"[LibCd] ReadN start LBA={readLba} repeat={_sameReadStartCount} " +
                        $"msf={_pos[0]:X2}:{_pos[1]:X2}:{_pos[2]:X2} " +
                        $"ready=0x{_cbReady:X8} data=0x{_cbData:X8} " +
                        $"sync=0x{_cbSync:X8} callerRA=0x{c.RA:X8}");
                    if (TraceCd)
                        TraceGt2ReadState(m, "start");
                }
                EnsureXaThread();
                break;
            case ReadS:
                _status = (byte)((_status | StatMotor | StatRead) &
                    ~(StatSeek | StatPlay));
                _cddaActive = false;
                CddaAudio.Reset();
                XaAudio.Reset();
                _xaActive = true;
                _readSSectorPhase = 0;
                _readSFileEndLba = TryDescribeLocatedFile(
                    CurrentLba, out _, out int readSEndLba)
                    ? readSEndLba
                    : int.MaxValue;
                _xaReportLba = CurrentLba;
                _xaPendingReportLba = -1;
                _xaLastTraceSecond = -1;
                _readActive = true;
                LibCdStream.OnReadStream(CurrentLba);
                break;
            case GetlocL:
            case GetlocP:
                lock (_posGate)
                {
                    _lastResult[0] = _pos[0];
                    _lastResult[1] = _pos[1];
                    _lastResult[2] = _pos[2];
                }
                _lastResult[3] = _mode;
                _lastResult[4] = _filterFile;
                _lastResult[5] = _filterChannel;
                _lastResult[6] = 0;
                _lastResult[7] = 0;
                if (result != 0) WriteResult(m, result);
                return CompleteImmediateCommand(com, result, notify);
            case GetTN:
                if (Runtime.Cd == null) return -1;
                _lastResult[0] = _status;
                _lastResult[1] = ToBcd(Runtime.Cd.FirstTrackNumber);
                _lastResult[2] = ToBcd(Runtime.Cd.LastTrackNumber);
                for (int i = 3; i < _lastResult.Length; i++) _lastResult[i] = 0;
                if (result != 0) WriteResult(m, result);
                Console.Error.WriteLine(
                    $"[CDDA] TOC tracks={Runtime.Cd.FirstTrackNumber}-{Runtime.Cd.LastTrackNumber}");
                return CompleteImmediateCommand(com, result, notify);
            case GetTD:
                if (Runtime.Cd == null) return -1;
                int trackNumber = param == 0 ? 0 : Bcd(m.ReadU8(param));
                int trackLba;
                if (trackNumber == 0) trackLba = Runtime.Cd.LeadOutLba;
                else if (!Runtime.Cd.TryGetTrackStartLba(trackNumber, out trackLba)) return -1;
                IntToPos(trackLba, out byte tdMm, out byte tdSs, out byte tdFf);
                _lastResult[0] = _status;
                _lastResult[1] = tdMm;
                _lastResult[2] = tdSs;
                _lastResult[3] = tdFf;
                for (int i = 4; i < _lastResult.Length; i++) _lastResult[i] = 0;
                if (result != 0) WriteResult(m, result);
                return CompleteImmediateCommand(com, result, notify);
            case Pause: case Stop: case Init:
                _status = (byte)((_status | StatMotor) &
                    ~(StatRead | StatSeek | StatPlay));
                LibCdStream.OnStopStream();
                _readActive = false;
                _xaActive = false;
                _readSSectorPhase = 0;
                _readSFileEndLba = int.MaxValue;
                _xaReportLba = -1;
                _xaPendingReportLba = -1;
                _xaLastTraceSecond = -1;
                _cddaActive = false;
                CddaAudio.Reset();
                XaAudio.Reset();
                OggMusic.SetMusicStreamActive(false);
                Dispatcher.ClearPending();
                break;
            case Play:
                _status = (byte)((_status | StatMotor | StatPlay) &
                    ~(StatRead | StatSeek));
                _readActive = false;
                _xaActive = false;
                _readSSectorPhase = 0;
                _readSFileEndLba = int.MaxValue;
                _xaReportLba = -1;
                _xaPendingReportLba = -1;
                _xaLastTraceSecond = -1;
                _cddaLba = CurrentLba;
                _cddaActive = true;
                _cddaPendingReportLba = -1;
                _cddaLastReportSecond = -1;
                XaAudio.Reset();
                CddaAudio.Reset();
                OggMusic.SetMusicStreamActive(false);
                EnsureXaThread();
                Console.Error.WriteLine($"[CDDA] play LBA={_cddaLba} mode=0x{_mode:X2}");
                break;
            case Mute:
                _cdMuted = true;
                if (TraceAudio) Console.Error.WriteLine("[CDMIX] muted");
                break;
            case Demute:
                _cdMuted = false;
                if (TraceAudio) Console.Error.WriteLine("[CDMIX] demuted");
                break;
            case SeekL: case SeekP:
                _status = (byte)((_status | StatMotor | StatSeek) &
                    ~(StatRead | StatPlay));
                break;
            case Nop:
                break;
            default:
                break;
        }

        _lastResult[0] = _status;
        for (int i = 1; i < _lastResult.Length; i++) _lastResult[i] = 0;
        if (result != 0) WriteResult(m, result);
        if (notify && _cbSync != 0)
            _pendingSync.Enqueue(new PendingSync(com, [.. _lastResult], result));
        return 0;
    }

    static int CompleteImmediateCommand(byte command, uint resultAddress, bool notify)
    {
        // PsyQ's non-blocking control path still receives a sync callback for
        // commands whose result is available immediately. GT2's replay-exit
        // transition waits on that callback after issuing Getloc; returning
        // the bytes without the callback leaves its command script busy
        // forever on a black framebuffer.
        if (notify && _cbSync != 0)
            _pendingSync.Enqueue(
                new PendingSync(command, [.. _lastResult], resultAddress));
        return 0;
    }

    static void DispatchPendingSync()
    {
        if (_pendingSync.Count == 0 || _cbSync == 0 ||
            Runtime.Cpu == null || Runtime.Mem == null) return;

        var pending = _pendingSync.Dequeue();
        var m = Runtime.Mem;
        const uint callbackResultAddress = 0x8000FF10u;
        uint resultAddress = pending.ResultAddress != 0
            ? pending.ResultAddress
            : callbackResultAddress;
        Array.Copy(pending.Result, _lastResult, _lastResult.Length);
        WriteResult(m, resultAddress);
        bool traceCompletion = TraceCd &&
            (pending.Command != ReadN || _traceCurrentReadStart);
        if (traceCompletion)
        {
            Console.Error.WriteLine(
                $"[LibCd] command complete com=0x{pending.Command:X2} " +
                $"callback=0x{_cbSync:X8} pending={_pendingSync.Count}");
            if (pending.Command == ReadN)
                TraceGt2ReadState(m, "before-sync");
        }

        var c = Runtime.Cpu;
        var snap = c.Snapshot();
        uint drive = Gt2Drive;
        byte busyBefore = m.ReadU8(drive + 0x166u);
        c.A0 = Complete;
        c.A1 = resultAddress;
        Dispatcher.Call(c, m, _cbSync);
        if (traceCompletion)
        {
            Console.Error.WriteLine(
                $"[LibCd] sync callback returned busy={busyBefore}->{m.ReadU8(drive + 0x166u)} " +
                $"cmd=0x{m.ReadU32(drive + 0x88u):X8} ptr=0x{m.ReadU32(drive + 0x7Cu):X8} " +
                $"next=0x{m.ReadU32(drive + 0x80u):X8} pending=0x{m.ReadU32(drive + 0x84u):X8}");
            if (pending.Command == ReadN)
                TraceGt2ReadState(m, "after-sync");
        }
        c.Restore(snap);
    }

    static void TraceGt2ReadState(IMemory m, string phase)
    {
        uint drive = Gt2Drive;
        const uint transfer = 0x801C9500u;
        Console.Error.WriteLine(
            $"[LibCd] GT2-read {phase} lba={CurrentLba} mode=0x{_mode:X2} " +
            $"active={m.ReadU8(drive + 0x167u)} busy={m.ReadU8(drive + 0x166u)} " +
            $"stop={m.ReadU8(drive + 0x16Au)} buffered={m.ReadU16(drive + 0x5Au)} " +
            $"expected={m.ReadU32(drive + 0x44u)} end={m.ReadU32(drive + 0x48u)} " +
            $"current={m.ReadU32(drive + 0x60u)} handler=0x{m.ReadU32(drive + 0x4Cu):X8} " +
            $"dest=0x{m.ReadU32(drive + 0x50u):X8} remaining={m.ReadU32(drive + 0x54u)} " +
            $"script=0x{m.ReadU32(drive + 0x7Cu):X8} " +
            $"next=0x{m.ReadU32(drive + 0x80u):X8} " +
            $"deferred=0x{m.ReadU32(drive + 0x84u):X8} " +
            $"transferDest=0x{m.ReadU32(transfer):X8} " +
            $"transferOffset={m.ReadU16(transfer + 0x6u)} " +
            $"transferStride={m.ReadU16(transfer + 0x10u)} " +
            $"transferRemaining={m.ReadU16(transfer + 0x12u)} " +
            $"pendingSync={_pendingSync.Count}");
    }

    static void DeliverInitialReadyCallback(IMemory m)
    {
        if (_cbReady == 0 || Runtime.Cpu == null) return;

        var c = Runtime.Cpu;
        var snap = c.Snapshot();
        if (TraceCd)
            Console.Error.WriteLine($"[LibCd] DeliverInitial before callback LBA={CurrentLba}");
        _lastIntr = DataReady;
        _sectorReadOffset = 0;
        c.A0 = DataReady;
        c.A1 = 0;
        Dispatcher.Call(c, m, _cbReady);
        if (TraceCd)
            Console.Error.WriteLine($"[LibCd] DeliverInitial after callback LBA={CurrentLba}");
        AdvancePos(1);
        Dispatcher.LoadByLba(CurrentLba);
        if (_cbData != 0)
        {
            c.A0 = DataReady;
            c.A1 = 0;
            Dispatcher.Call(c, m, _cbData);
        }
        c.Restore(snap);
    }

    public static void WaitForV8Sector(CpuContext c, IMemory m)
    {
        uint consumedPtr = m.ReadU32(c.GP + 0x6A4u);
        uint producedPtr = m.ReadU32(c.GP + 0x6A8u);
        if (producedPtr == consumedPtr)
        {
            DeliverInitialReadyCallback(m);
            producedPtr = m.ReadU32(c.GP + 0x6A8u);
        }
        m.WriteU32(c.GP + 0x6A4u, producedPtr);
        c.V0 = consumedPtr;
    }

    public static void BeginV8FileRead(CpuContext c, IMemory m)
    {
        _v8FileStartLba = unchecked((int)c.A0);
    }

    public static void BeginV82FileRead(CpuContext c, IMemory m)
    {
        _v82FileStartLba = unchecked((int)c.A0);
    }

    // Vigilante 8's file reader consumes a two-sector callback ring. On the
    // original console the CD interrupt can refill that ring during a long
    // memcpy; the recompiled single-threaded path cannot reproduce that timing
    // safely. Read the same 2048-byte sectors directly while retaining the
    // game's byte-offset state and public reader semantics.
    public static void ReadV8FileBytes(CpuContext c, IMemory m)
    {
        ReadFileBytes(c, m, _v8FileStartLba, c.GP + 0x6ACu, "Vigilante 8");
    }

    public static void ReadV82FileBytes(CpuContext c, IMemory m)
    {
        ReadFileBytes(c, m, _v82FileStartLba, c.GP + 0xD64u, "Vigilante 8: 2nd Offense");
    }

    static void ReadFileBytes(
        CpuContext c, IMemory m, int fileStartLba, uint offsetAddress, string game)
    {
        if (fileStartLba < 0 || Runtime.Cd == null)
            throw new InvalidOperationException($"{game} file read started without a disc/LBA");

        m = Dispatcher.UnwrapMemory(m);
        uint destination = c.A0;
        uint length = c.A1;
        uint offset = m.ReadU32(offsetAddress);

        if (Runtime.Cd.Fs.TryReadLooseFileRange(
                fileStartLba, offset, checked((int)length), out byte[] looseData))
        {
            for (int i = 0; i < looseData.Length; i++)
                m.WriteU8(destination + (uint)i, looseData[i]);
            m.WriteU32(offsetAddress, offset + length);
            c.V0 = 1u;
            return;
        }

        uint copied = 0u;
        while (copied < length)
        {
            int lba = fileStartLba + (int)(offset >> 11);
            int inSector = (int)(offset & 0x7FFu);
            byte[] sector;
            lock (DiscLock) sector = Runtime.Cd.ReadSectorData(lba, 2048);
            int take = Math.Min((int)(length - copied), 2048 - inSector);
            for (int i = 0; i < take; i++)
            {
                uint writeAddress = destination + copied + (uint)i;
                m.WriteU8(writeAddress, sector[inSector + i]);
            }
            copied += (uint)take;
            offset += (uint)take;
        }

        m.WriteU32(offsetAddress, offset);
        c.V0 = 1u;
    }

    public static void SeekV8File(CpuContext c, IMemory m)
    {
        SeekFile(c, m, c.GP + 0x6ACu);
    }

    public static void SeekV82File(CpuContext c, IMemory m)
    {
        SeekFile(c, m, c.GP + 0xD64u);
    }

    static void SeekFile(CpuContext c, IMemory m, uint offsetAddress)
    {
        m = Dispatcher.UnwrapMemory(m);
        uint current = m.ReadU32(offsetAddress);
        uint target = c.A1 == 0u ? c.A0 : current + c.A0;
        m.WriteU32(offsetAddress, target);
    }

    static int SyncResult(IMemory m, uint result)
    {
        if (result != 0) WriteResult(m, result);
        return _lastIntr;
    }

    static void WriteResult(IMemory m, uint addr)
    {
        for (int i = 0; i < _lastResult.Length; i++)
            m.WriteU8(addr + (uint)i, _lastResult[i]);
    }

    static int SectorSize(byte mode)
    {
        if ((mode & ModeSize1) != 0) return 2340;
        if ((mode & ModeSize0) != 0) return 2328;
        return 2048;
    }
    
    static string ReadCString(IMemory m, uint addr)
    {
        var sb = new System.Text.StringBuilder();
        for (uint i = 0; i < 128; i++)
        {
            byte b = m.ReadU8(addr + i);
            if (b == 0) break;
            sb.Append((char)b);
        }
        return sb.ToString();
    }

    static int Bcd(byte b) => (b >> 4) * 10 + (b & 0xF);
    static byte ToBcd(int n) => (byte)(((n / 10) << 4) + (n % 10));

    static int PosToInt(byte[] p) => (Bcd(p[0]) * 60 + Bcd(p[1])) * 75 + Bcd(p[2]) - 150;

    static void IntToPos(int i, out byte mm, out byte ss, out byte ff)
    {
        i += 150;
        ff = ToBcd(i % 75);
        ss = ToBcd(i / 75 % 60);
        mm = ToBcd(i / 75 / 60);
    }
}
