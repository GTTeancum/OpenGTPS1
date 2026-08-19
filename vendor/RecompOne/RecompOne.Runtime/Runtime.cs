using System.Diagnostics;
using System.Runtime.InteropServices;
using RecompOne.Runtime.Context;
using RecompOne.Runtime.Host;
using RecompOne.Runtime.Memory;

namespace RecompOne.Runtime;

public enum RunMode { Retail, Devkit }

public static class Runtime
{
    static bool? _lastDisplayEnabled;
    static int _presentTraceCount;
    static bool _shutdownRequested;
    static readonly bool TracePerformance =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_PERFORMANCE") == "1";
    static readonly int ExitAfterInputPoll =
        int.TryParse(Environment.GetEnvironmentVariable("RECOMPONE_EXIT_AFTER_INPUT_POLL"), out int exitPoll)
            ? Math.Max(0, exitPoll)
            : 0;
    static readonly bool CreateGt2TestSave =
        Environment.GetEnvironmentVariable("RECOMPONE_GT2_CREATE_TEST_SAVE") == "1";
    static readonly bool UnlockGt2SoakEvents =
        Environment.GetEnvironmentVariable("RECOMPONE_GT2_SOAK_UNLOCK_ALL_RACES") == "1";
    static readonly bool TraceGt2Save =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_SAVE") == "1";
    static readonly uint? TraceGt2CarId = ParseHexEnvironment(
        "RECOMPONE_TRACE_GT2_CAR_ID");
    static bool _testSavePatchReported;
    static bool _soakEventUnlockReported;
    static bool _saveTraceReported;
    static bool _carIdTraceReported;
    static int _lastCarIdTracePoll = -1;
    static long _performanceStarted;
    static long _performanceHostTicks;
    static long _performanceWaitTicks;
    static long _performanceDeviceTicks;
    static long _performanceCdTicks;
    static long _performanceCardTicks;
    static long _performanceBiosPadTicks;
    static long _performanceLibPadTicks;
    static long _performanceIrqTicks;
    static long _performanceGuestTicks;
    static long _performancePreviousEnd;
    static long _performancePreviousAfterWait;
    static long _performanceAllocatedBytes;
    static long _performanceHostAllocatedBytes;
    static long _performanceWaitAllocatedBytes;
    static long _performanceDeviceAllocatedBytes;
    static long _performanceCdAllocatedBytes;
    static long _performanceCardAllocatedBytes;
    static long _performanceBiosPadAllocatedBytes;
    static long _performanceLibPadAllocatedBytes;
    static long _performanceIrqAllocatedBytes;
    static long _performanceGuestAllocatedBytes;
    static long _performancePreviousEndAllocatedBytes;
    static int _performanceGen0;
    static int _performanceGen1;
    static int _performanceGen2;
    static int _performanceFrames;
    static readonly bool TraceVSync =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_VSYNC") == "1";
    public static CpuContext? Cpu { get; private set; }
    public static IMemory? Mem { get; private set; }
    public static Gpu? Gpu;
    public static Spu? Spu;
    public static Cdrom.CdController? Cd;

    public static RunMode Mode { get; private set; } = RunMode.Retail;
    public static void SetMode(RunMode mode) => Mode = mode; //devkit vs retail, devkits reads from sim and has more ram
    public static string CdPath => Config.ConfigManager.Game.CdPath;

    public static string? ResolveLoosePath()
    {
        string? configured = Environment.GetEnvironmentVariable("RECOMPONE_LOOSE_DIR");
        if (configured == "0") return null;
        string candidate = string.IsNullOrWhiteSpace(configured)
            ? AppContext.BaseDirectory
            : Path.GetFullPath(configured);
        if (string.IsNullOrWhiteSpace(configured) &&
            !File.Exists(Path.Combine(candidate, "SYSTEM.CNF")))
            return null;
        return Directory.Exists(candidate) ? candidate : null;
    }
    
    public static Config.ViewConfig View => Config.ConfigManager.View;
    public static void SaveView() => Config.ConfigManager.SaveView(Host.Window.PanelManager.Panels);
    
    public static Hardware.MemoryCard CardA = new("carda.sav") { Enabled = true };
    public static Hardware.MemoryCard CardB = new("cardb.sav") { Enabled = true };
    public static readonly Memory.RamLogger RamLog = new();
    public static readonly Dispatch.OverlayEventLog OverlayLog = new();

    public static void Initialize(string title)
    {
        Diagnostics.ConsoleMirror.Install();
        // The emulation and native-render producer threads jointly feed a
        // hard 59.94 Hz presentation deadline. Interactive use defaults to a
        // balanced elevated priority, while diagnostics can explicitly lower
        // the entire process to avoid disrupting other desktop work.
        if (OperatingSystem.IsWindows())
        {
            try
            {
                using Process process = Process.GetCurrentProcess();
                string? configuredPriority =
                    Environment.GetEnvironmentVariable(
                        "RECOMPONE_PROCESS_PRIORITY");
                ProcessPriorityClass priority =
                    string.Equals(
                        configuredPriority,
                        "BelowNormal",
                        StringComparison.OrdinalIgnoreCase)
                        ? ProcessPriorityClass.BelowNormal
                        : ProcessPriorityClass.AboveNormal;
                process.PriorityClass = priority;
                Console.Error.WriteLine(
                    $"[Host] scheduling priority={priority} " +
                    "(balanced emulation/renderer)");
            }
            catch (Exception exception) when (
                exception is InvalidOperationException or
                System.ComponentModel.Win32Exception or
                NotSupportedException)
            {
                Console.Error.WriteLine(
                    $"[Host] unable to set scheduling priority: " +
                    $"{exception.Message}");
            }
        }
        HostWindow.Initialize(title);
        bool forceMute =
            HostWindow.IsHeadless ||
            Environment.GetEnvironmentVariable("RECOMPONE_MUTE") == "1";
        if (HostWindow.IsHeadless)
            Console.Error.WriteLine("[Host] headless/hidden run: audio output forced muted");
        Audio.SetMasterVolume(forceMute || Config.ConfigManager.Game.Muted
            ? 0f
            : Config.ConfigManager.Game.MasterVolume);
        Audio.Initialize(noPhysicalOutput: HostWindow.IsHeadless);
    }

    public static void WaitForValidDisc() => HostWindow.WaitForValidDisc();

    public static void SetContext(CpuContext c, IMemory m)
    {
        Cpu = c;
        Mem = m;
    }

    public static void PresentFrame()
    {
        long performanceStart = Stopwatch.GetTimestamp();
        long allocatedAtStart = TracePerformance
            ? GC.GetAllocatedBytesForCurrentThread()
            : 0;
        int traceFrame = _presentTraceCount++;
        ApplyGt2TestSavePatch();
        ApplyGt2SoakEventUnlock();
        ReportGt2SaveState();
        ReportGt2CarLocations();
        if (TraceVSync && traceFrame < 10) Console.Error.WriteLine($"[VSync] present {traceFrame}: window");
        if (Gpu != null && _lastDisplayEnabled != Gpu.DisplayEnabled)
        {
            _lastDisplayEnabled = Gpu.DisplayEnabled;
            Console.WriteLine($"[GPU] display={Gpu.DisplayEnabled} area={Gpu.DisplayX},{Gpu.DisplayY} {Gpu.DisplayWidth}x{Gpu.DisplayHeight} hle={Hle.GpuHle.Active}");
        }
        if (Gpu != null && Mem != null)
            Sdk.GT2Compat.CompositeUnifiedTitlePanel(Gpu, Mem);
        HostWindow.Present(Gpu);
        long afterHost = Stopwatch.GetTimestamp();
        long allocatedAfterHost = TracePerformance
            ? GC.GetAllocatedBytesForCurrentThread()
            : 0;
        if (ExitAfterInputPoll > 0 && InputManager.CurrentPoll >= ExitAfterInputPoll)
            _shutdownRequested = true;
        if (_shutdownRequested)
        {
            Console.Error.WriteLine(
                $"[Runtime] shutdown requested poll={InputManager.CurrentPoll}");
            Shutdown();
            Console.Error.WriteLine("[Runtime] shutdown complete; exit=0");
            TerminateProcess(0);
        }
        if (TraceVSync && traceFrame < 10) Console.Error.WriteLine($"[VSync] present {traceFrame}: audio");
        Audio.Attach(Spu);
        double waitedMs = FrameClock.Throttle();
        long afterWait = Stopwatch.GetTimestamp();
        long allocatedAfterWait = TracePerformance
            ? GC.GetAllocatedBytesForCurrentThread()
            : 0;
        if (TraceVSync && traceFrame < 10) Console.Error.WriteLine($"[VSync] present {traceFrame}: devices");
        Sdk.LibCd.Tick();
        long afterCd = Stopwatch.GetTimestamp();
        long allocatedAfterCd = TracePerformance
            ? GC.GetAllocatedBytesForCurrentThread()
            : 0;
        if (Cpu != null && Mem != null) Bios.BiosB.TickCards(Cpu, Mem);
        long afterCards = Stopwatch.GetTimestamp();
        long allocatedAfterCards = TracePerformance
            ? GC.GetAllocatedBytesForCurrentThread()
            : 0;
        if (Mem != null) Bios.BiosB.RefreshPad(Mem);
        long afterBiosPad = Stopwatch.GetTimestamp();
        long allocatedAfterBiosPad = TracePerformance
            ? GC.GetAllocatedBytesForCurrentThread()
            : 0;
        if (Mem != null) Sdk.LibPad.Refresh(Mem); //is this correct?
        long afterLibPad = Stopwatch.GetTimestamp();
        long allocatedAfterLibPad = TracePerformance
            ? GC.GetAllocatedBytesForCurrentThread()
            : 0;
        long afterDevices = afterLibPad;
        long allocatedAfterDevices = TracePerformance
            ? GC.GetAllocatedBytesForCurrentThread()
            : 0;
        if (TraceVSync && traceFrame < 10) Console.Error.WriteLine($"[VSync] present {traceFrame}: irq");
        DispatchIrq(0); //using this to dispatch irqs too if necessary, probably not needed after the rest of stuff is reimplemented
        DrainDeferredIrqs();
        // GT2 submits the completed ordering table from its VBlank callback.
        // Clear recovered depths only after that callback has consumed them.
        Gte.BeginScreenDepthFrame();
        long afterIrq = Stopwatch.GetTimestamp();
        long allocatedAfterIrq = TracePerformance
            ? GC.GetAllocatedBytesForCurrentThread()
            : 0;
        RecordPerformance(
            performanceStart, afterHost, afterWait, afterCd, afterCards,
            afterBiosPad, afterLibPad, afterDevices, afterIrq,
            allocatedAtStart, allocatedAfterHost, allocatedAfterWait,
            allocatedAfterCd, allocatedAfterCards, allocatedAfterBiosPad,
            allocatedAfterLibPad,
            allocatedAfterDevices, allocatedAfterIrq, waitedMs);
        if (TraceVSync && traceFrame < 10) Console.Error.WriteLine($"[VSync] present {traceFrame}: done");
    }

    static void ApplyGt2TestSavePatch()
    {
        if (!CreateGt2TestSave || Mem == null || InputManager.CurrentPoll < 820)
            return;

        // NTSC-U v1.2 Simulation Mode working data. This hook is deliberately
        // opt-in and exists only to let GT2 itself serialize a convenient,
        // checksum-correct test card through its normal Save Game screen.
        // The repository image is the NTSC-U Rev 2 / game v1.2 pressing.
        Mem.WriteU32(0x801D1568u, 100_000u);
        const uint firstLicenseTest = 0x801CACF8u;
        const uint licenseTestStride = 0xA4u;
        for (uint test = 0; test < 60; test++)
            Mem.WriteU16(firstLicenseTest + test * licenseTestStride, 0x0100);

        if (_testSavePatchReported) return;
        _testSavePatchReported = true;
        Console.Error.WriteLine(
            "[GT2] test-save patch active: all 60 licenses bronze, credits=100000");
    }

    static void ReportGt2SaveState()
    {
        if (!TraceGt2Save || _saveTraceReported || Mem == null ||
            InputManager.CurrentPoll < 1000)
            return;

        const uint creditsAddress = 0x801D1568u;
        const uint firstLicenseTest = 0x801CACF8u;
        const uint licenseTestStride = 0xA4u;
        int completed = 0;
        int bronze = 0;
        string[] classes = new string[6];
        for (uint license = 0; license < 6; license++)
        {
            ushort[] results = new ushort[10];
            for (uint test = 0; test < 10; test++)
            {
                uint index = license * 10u + test;
                ushort result =
                    Mem.ReadU16(firstLicenseTest + index * licenseTestStride);
                results[test] = result;
                if (result != 0)
                    completed++;
                if (result == 0x0100)
                    bronze++;
            }
            classes[license] = string.Join(
                ',', results.Select(result => $"0x{result:X4}"));
        }

        _saveTraceReported = true;
        Console.Error.WriteLine(
            $"[GT2-Save] credits={Mem.ReadU32(creditsAddress)} " +
            $"licenseTestsCompleted={completed}/60 bronze={bronze}/60 " +
            $"classes=[{string.Join('|', classes)}]");
    }

    static void RecordPerformance(
        long started, long afterHost, long afterWait, long afterCd,
        long afterCards, long afterBiosPad, long afterLibPad,
        long afterDevices, long afterIrq,
        long allocatedAtStart, long allocatedAfterHost,
        long allocatedAfterWait, long allocatedAfterCd,
        long allocatedAfterCards, long allocatedAfterBiosPad,
        long allocatedAfterLibPad,
        long allocatedAfterDevices,
        long allocatedAfterIrq, double waitedMs)
    {
        if (!TracePerformance) return;
        if (_performanceStarted == 0)
        {
            _performanceStarted = started;
            _performanceAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();
            _performanceGen0 = GC.CollectionCount(0);
            _performanceGen1 = GC.CollectionCount(1);
            _performanceGen2 = GC.CollectionCount(2);
        }
        if (_performancePreviousEnd != 0)
        {
            _performanceGuestTicks += started - _performancePreviousEnd;
            _performanceGuestAllocatedBytes +=
                allocatedAtStart - _performancePreviousEndAllocatedBytes;
        }
        if (_performancePreviousAfterWait != 0)
        {
            long intervalTicks = afterWait - _performancePreviousAfterWait;
            double intervalMs = intervalTicks * 1000.0 / Stopwatch.Frequency;
            if (intervalMs >= 40.0)
            {
                double tickScale = 1000.0 / Stopwatch.Frequency;
                Console.Error.WriteLine(
                    $"[PERF-LONG-FRAME] poll={InputManager.CurrentPoll} " +
                    $"intervalMs={intervalMs:F3} " +
                    $"priorTailMs={(_performancePreviousEnd - _performancePreviousAfterWait) * tickScale:F3} " +
                    $"guestMs={(started - _performancePreviousEnd) * tickScale:F3} " +
                    $"hostMs={(afterHost - started) * tickScale:F3} " +
                    $"waitMs={(afterWait - afterHost) * tickScale:F3}");
            }
        }
        _performancePreviousEnd = afterIrq;
        _performancePreviousAfterWait = afterWait;
        _performancePreviousEndAllocatedBytes = allocatedAfterIrq;
        _performanceHostTicks += afterHost - started;
        _performanceWaitTicks += afterWait - afterHost;
        _performanceDeviceTicks += afterDevices - afterWait;
        _performanceCdTicks += afterCd - afterWait;
        _performanceCardTicks += afterCards - afterCd;
        _performanceBiosPadTicks += afterBiosPad - afterCards;
        _performanceLibPadTicks += afterLibPad - afterBiosPad;
        _performanceIrqTicks += afterIrq - afterDevices;
        _performanceHostAllocatedBytes +=
            allocatedAfterHost - allocatedAtStart;
        _performanceWaitAllocatedBytes +=
            allocatedAfterWait - allocatedAfterHost;
        _performanceDeviceAllocatedBytes +=
            allocatedAfterDevices - allocatedAfterWait;
        _performanceCdAllocatedBytes += allocatedAfterCd - allocatedAfterWait;
        _performanceCardAllocatedBytes +=
            allocatedAfterCards - allocatedAfterCd;
        _performanceBiosPadAllocatedBytes +=
            allocatedAfterBiosPad - allocatedAfterCards;
        _performanceLibPadAllocatedBytes +=
            allocatedAfterLibPad - allocatedAfterBiosPad;
        _performanceIrqAllocatedBytes +=
            allocatedAfterIrq - allocatedAfterDevices;
        if (++_performanceFrames < 300) return;

        double scale = 1000.0 / Stopwatch.Frequency;
        double elapsedMs = (afterIrq - _performanceStarted) * scale;
        double fps = elapsedMs > 0 ? _performanceFrames * 1000.0 / elapsedMs : 0;
        long allocatedNow = GC.GetAllocatedBytesForCurrentThread();
        int gen0Now = GC.CollectionCount(0);
        int gen1Now = GC.CollectionCount(1);
        int gen2Now = GC.CollectionCount(2);
        Console.Error.WriteLine(
            $"[PERF] poll={InputManager.CurrentPoll} fps={fps:F2} " +
            $"host={_performanceHostTicks * scale / _performanceFrames:F2}ms " +
            $"wait={_performanceWaitTicks * scale / _performanceFrames:F2}ms " +
            $"devices={_performanceDeviceTicks * scale / _performanceFrames:F2}ms " +
            $"devicePhases=" +
            $"{_performanceCdTicks * scale / _performanceFrames:F2}/" +
            $"{_performanceCardTicks * scale / _performanceFrames:F2}/" +
            $"{_performanceBiosPadTicks * scale / _performanceFrames:F2}/" +
            $"{_performanceLibPadTicks * scale / _performanceFrames:F2}ms " +
            $"irq+guest={_performanceIrqTicks * scale / _performanceFrames:F2}ms " +
            $"guest={_performanceGuestTicks * scale / _performanceFrames:F2}ms " +
            $"alloc={(allocatedNow - _performanceAllocatedBytes) / 1024.0:F0}KiB " +
            $"allocPhases=" +
            $"{_performanceHostAllocatedBytes / 1024.0:F0}/" +
            $"{_performanceWaitAllocatedBytes / 1024.0:F0}/" +
            $"{_performanceDeviceAllocatedBytes / 1024.0:F0}/" +
            $"{_performanceIrqAllocatedBytes / 1024.0:F0}/" +
            $"{_performanceGuestAllocatedBytes / 1024.0:F0}KiB " +
            $"allocDevices=" +
            $"{_performanceCdAllocatedBytes / 1024.0:F0}/" +
            $"{_performanceCardAllocatedBytes / 1024.0:F0}/" +
            $"{_performanceBiosPadAllocatedBytes / 1024.0:F0}/" +
            $"{_performanceLibPadAllocatedBytes / 1024.0:F0}KiB " +
            $"gc={gen0Now - _performanceGen0}/" +
            $"{gen1Now - _performanceGen1}/{gen2Now - _performanceGen2} " +
            $"last_wait={waitedMs:F2}ms");
        _performanceStarted = afterIrq;
        _performanceHostTicks = 0;
        _performanceWaitTicks = 0;
        _performanceDeviceTicks = 0;
        _performanceCdTicks = 0;
        _performanceCardTicks = 0;
        _performanceBiosPadTicks = 0;
        _performanceLibPadTicks = 0;
        _performanceIrqTicks = 0;
        _performanceGuestTicks = 0;
        _performanceHostAllocatedBytes = 0;
        _performanceWaitAllocatedBytes = 0;
        _performanceDeviceAllocatedBytes = 0;
        _performanceCdAllocatedBytes = 0;
        _performanceCardAllocatedBytes = 0;
        _performanceBiosPadAllocatedBytes = 0;
        _performanceLibPadAllocatedBytes = 0;
        _performanceIrqAllocatedBytes = 0;
        _performanceGuestAllocatedBytes = 0;
        _performanceAllocatedBytes = allocatedNow;
        _performanceGen0 = gen0Now;
        _performanceGen1 = gen1Now;
        _performanceGen2 = gen2Now;
        _performanceFrames = 0;
    }

    public static void DispatchIrq(int irq)
    {
        if (Cpu != null && Mem != null)
            Interrupts.Deliver(irq, Cpu, Mem);
    }

    static uint? ParseHexEnvironment(string name)
    {
        string? text = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(text))
            return null;
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            text = text[2..];
        return uint.TryParse(
            text,
            System.Globalization.NumberStyles.HexNumber,
            System.Globalization.CultureInfo.InvariantCulture,
            out uint value)
            ? value
            : null;
    }

    static void ReportGt2CarLocations()
    {
        if (TraceGt2CarId is not uint carId ||
            _carIdTraceReported ||
            InputManager.CurrentPoll < 1000 ||
            InputManager.CurrentPoll % 100 != 0 ||
            InputManager.CurrentPoll == _lastCarIdTracePoll ||
            Mem is not PSMemory psMemory)
            return;

        _lastCarIdTracePoll = InputManager.CurrentPoll;
        ReadOnlySpan<byte> ram = psMemory.Ram;
        int reported = 0;
        int firstOffset = Math.Min(0x1C0000, ram.Length);
        int endOffset = Math.Min(0x1D8000, ram.Length);
        for (int offset = firstOffset;
             offset <= endOffset - sizeof(uint);
             offset++)
        {
            if (System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(
                    ram[offset..]) != carId)
                continue;

            int start = Math.Max(0, offset - 16);
            int length = Math.Min(160, ram.Length - start);
            Console.Error.WriteLine(
                $"[GT2-CarTrace] id=0x{carId:X8} " +
                $"address=0x{0x80000000u + (uint)offset:X8} " +
                $"bytes={Convert.ToHexString(ram.Slice(start, length))}");
            if (++reported == 64)
                break;
        }
        if (reported != 0 || InputManager.CurrentPoll >= 3000)
        {
            _carIdTraceReported = true;
            Console.Error.WriteLine(
                $"[GT2-CarTrace] id=0x{carId:X8} occurrences={reported}");
        }
    }

    static int _irqDeferralDepth;
    static uint _deferredIrqMask;
    static readonly Queue<Action> DeferredHardwareActions = [];

    public static void BeginIrqDeferral() => _irqDeferralDepth++;

    public static void EndIrqDeferral()
    {
        if (_irqDeferralDepth <= 0)
            throw new InvalidOperationException(
                "Interrupt deferral ended without a matching begin");
        _irqDeferralDepth--;
    }

    public static void RaiseIrq(int irq)
    {
        if (_irqDeferralDepth != 0)
        {
            _deferredIrqMask |= 1u << irq;
            return;
        }
        DispatchIrq(irq);
    }

    /// <summary>
    /// Queue a hardware interrupt for the next runtime interrupt boundary.
    /// DMA completes asynchronously on the original hardware. Delivering its
    /// IRQ from inside the register write lets a completion callback start a
    /// second DMA before the first callback can update its transfer state,
    /// recursively replaying the same chunk.
    /// </summary>
    public static void DeferIrq(int irq) =>
        _deferredIrqMask |= 1u << irq;

    public static void DeferHardwareAction(Action action)
    {
        lock (DeferredHardwareActions)
            DeferredHardwareActions.Enqueue(action);
    }

    static bool DrainDeferredHardwareActions()
    {
        int actionCount;
        lock (DeferredHardwareActions)
        {
            actionCount = DeferredHardwareActions.Count;
            if (actionCount == 0)
                return false;
        }

        // Drain exactly the actions visible at this hardware boundary. An
        // action may queue follow-up work, which belongs to the next pass of
        // DrainDeferredIrqs just as it did when this queue was snapshotted via
        // ToArray, but no temporary Action[] is required every frame.
        for (int index = 0; index < actionCount; index++)
        {
            Action action;
            lock (DeferredHardwareActions)
                action = DeferredHardwareActions.Dequeue();
            action();
        }
        return true;
    }

    public static void DrainDeferredIrqs()
    {
        if (_irqDeferralDepth != 0)
            return;

        for (int guard = 0; guard < 64; guard++)
        {
            bool completedHardware = DrainDeferredHardwareActions();
            uint pending = _deferredIrqMask;
            _deferredIrqMask = 0;
            if (!completedHardware && pending == 0)
                break;
            for (int irq = 0; irq < 32; irq++)
            {
                if ((pending & (1u << irq)) != 0)
                    DispatchIrq(irq);
            }
        }
    }

    public static void Shutdown()
    {
        Sdk.GT2Compat.DumpRendererAudit();
        Audio.Shutdown();
        HostWindow.Shutdown();
    }

    public static void RequestShutdown() => _shutdownRequested = true;

    public static void TerminateProcess(int exitCode)
    {
        // Console.Out may be a long-lived redirected writer in automated
        // replays. Its managed Flush can wait indefinitely after the GL/D3D
        // teardown, while the bounded session log and capture encoders have
        // already flushed their own files during Shutdown.
        if (OperatingSystem.IsWindows())
        {
            using Process current = Process.GetCurrentProcess();
            if (NativeTerminateProcess(
                    current.Handle,
                    unchecked((uint)exitCode)))
                throw new InvalidOperationException(
                    "TerminateProcess unexpectedly returned success");
            Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
        }
        Environment.Exit(exitCode);
        throw new InvalidOperationException(
            "Process termination unexpectedly returned");
    }

    static void ApplyGt2SoakEventUnlock()
    {
        if (!UnlockGt2SoakEvents || Mem == null || InputManager.CurrentPoll < 820)
            return;

        // NTSC-U v1.2 Simulation Mode race-result working data. The long soak
        // uses this opt-in setup hook only to satisfy the regional-league
        // prerequisite for entering World League. The run still starts and
        // completes the championship normally, and its memory card is restored
        // by the soak harness.
        const uint firstRaceResult = 0x801C99F8u;
        const uint raceResultCount = 0x44u;
        for (uint race = 0; race < raceResultCount; race++)
            Mem.WriteU16(firstRaceResult + race * 2u, 0x1111);

        if (_soakEventUnlockReported) return;
        _soakEventUnlockReported = true;
        Console.Error.WriteLine(
            "[GT2-Soak] setup active: race prerequisites unlocked in working memory");
    }

    [DllImport(
        "kernel32.dll",
        EntryPoint = "TerminateProcess",
        ExactSpelling = true,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool NativeTerminateProcess(
        nint processHandle,
        uint exitCode);
}
