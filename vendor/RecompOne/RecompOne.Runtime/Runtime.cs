using System.Diagnostics;
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
    static readonly bool TraceGt2Save =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_SAVE") == "1";
    static bool _testSavePatchReported;
    static bool _saveTraceReported;
    static long _performanceStarted;
    static long _performanceHostTicks;
    static long _performanceWaitTicks;
    static long _performanceDeviceTicks;
    static long _performanceIrqTicks;
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
        int traceFrame = _presentTraceCount++;
        ApplyGt2TestSavePatch();
        ReportGt2SaveState();
        if (TraceVSync && traceFrame < 10) Console.Error.WriteLine($"[VSync] present {traceFrame}: window");
        if (Gpu != null && _lastDisplayEnabled != Gpu.DisplayEnabled)
        {
            _lastDisplayEnabled = Gpu.DisplayEnabled;
            Console.WriteLine($"[GPU] display={Gpu.DisplayEnabled} area={Gpu.DisplayX},{Gpu.DisplayY} {Gpu.DisplayWidth}x{Gpu.DisplayHeight} hle={Hle.GpuHle.Active}");
        }
        HostWindow.Present(Gpu);
        long afterHost = Stopwatch.GetTimestamp();
        if (ExitAfterInputPoll > 0 && InputManager.CurrentPoll >= ExitAfterInputPoll)
            _shutdownRequested = true;
        if (_shutdownRequested)
        {
            Shutdown();
            Environment.Exit(0);
        }
        if (TraceVSync && traceFrame < 10) Console.Error.WriteLine($"[VSync] present {traceFrame}: audio");
        Audio.Attach(Spu);
        double waitedMs = FrameClock.Throttle();
        long afterWait = Stopwatch.GetTimestamp();
        if (TraceVSync && traceFrame < 10) Console.Error.WriteLine($"[VSync] present {traceFrame}: devices");
        Sdk.LibCd.Tick();
        if (Cpu != null && Mem != null) Bios.BiosB.TickCards(Cpu, Mem);
        if (Mem != null) { Bios.BiosB.RefreshPad(Mem); Sdk.LibPad.Refresh(Mem); } //is this correct?
        long afterDevices = Stopwatch.GetTimestamp();
        if (TraceVSync && traceFrame < 10) Console.Error.WriteLine($"[VSync] present {traceFrame}: irq");
        DispatchIrq(0); //using this to dispatch irqs too if necessary, probably not needed after the rest of stuff is reimplemented
        // GT2 submits the completed ordering table from its VBlank callback.
        // Clear recovered depths only after that callback has consumed them.
        Gte.BeginScreenDepthFrame();
        long afterIrq = Stopwatch.GetTimestamp();
        RecordPerformance(
            performanceStart, afterHost, afterWait, afterDevices, afterIrq, waitedMs);
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
        long started, long afterHost, long afterWait, long afterDevices,
        long afterIrq, double waitedMs)
    {
        if (!TracePerformance) return;
        if (_performanceStarted == 0) _performanceStarted = started;
        _performanceHostTicks += afterHost - started;
        _performanceWaitTicks += afterWait - afterHost;
        _performanceDeviceTicks += afterDevices - afterWait;
        _performanceIrqTicks += afterIrq - afterDevices;
        if (++_performanceFrames < 300) return;

        double scale = 1000.0 / Stopwatch.Frequency;
        double elapsedMs = (afterIrq - _performanceStarted) * scale;
        double fps = elapsedMs > 0 ? _performanceFrames * 1000.0 / elapsedMs : 0;
        Console.Error.WriteLine(
            $"[PERF] poll={InputManager.CurrentPoll} fps={fps:F2} " +
            $"host={_performanceHostTicks * scale / _performanceFrames:F2}ms " +
            $"wait={_performanceWaitTicks * scale / _performanceFrames:F2}ms " +
            $"devices={_performanceDeviceTicks * scale / _performanceFrames:F2}ms " +
            $"irq+guest={_performanceIrqTicks * scale / _performanceFrames:F2}ms " +
            $"last_wait={waitedMs:F2}ms");
        _performanceStarted = afterIrq;
        _performanceHostTicks = 0;
        _performanceWaitTicks = 0;
        _performanceDeviceTicks = 0;
        _performanceIrqTicks = 0;
        _performanceFrames = 0;
    }

    public static void DispatchIrq(int irq)
    {
        if (Cpu != null && Mem != null)
            Interrupts.Deliver(irq, Cpu, Mem);
    }

    public static void Shutdown()
    {
        Audio.Shutdown();
        HostWindow.Shutdown();
    }

    public static void RequestShutdown() => _shutdownRequested = true;
}
