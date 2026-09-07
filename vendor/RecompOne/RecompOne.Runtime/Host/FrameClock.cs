using System.Diagnostics;
using System.Runtime;

namespace RecompOne.Runtime.Host;

internal static class FrameClock
{
    // NTSC PlayStation VBlank is 59.94 Hz. Sleeping for a truncated number of
    // milliseconds on every frame produces visible pacing jitter on Windows,
    // so sleep most of the interval and use a short spin only at the deadline.
    const double FrameMs = 1000.0 * 1001.0 / 60000.0;
    // Do not repay long scheduler stalls by accelerating gameplay. Two
    // authored intervals absorb ordinary one-frame scheduler jitter while
    // bounding recovery tightly enough to prevent a sustained overspeed
    // window after prebuffering or a host stall.
    const double MaximumCatchUpDebtMs = FrameMs * 2.0;

    static readonly Stopwatch _clock = Stopwatch.StartNew();
    static readonly bool _unthrottled =
        Environment.GetEnvironmentVariable("RECOMPONE_UNTHROTTLED") == "1";
    static readonly int _throttleAfterInputPoll =
        int.TryParse(Environment.GetEnvironmentVariable("RECOMPONE_THROTTLE_AFTER_INPUT_POLL"), out int throttlePoll)
            ? Math.Max(0, throttlePoll)
            : 0;
    static readonly string? _throttleOnScriptStage =
        NormalizeStage(Environment.GetEnvironmentVariable(
            "RECOMPONE_THROTTLE_ON_SCRIPT_STAGE"));
    static readonly bool _tracePerformance =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_PERFORMANCE") == "1";
    static bool _stageThrottleLatched;
    static bool _pacedThreadPriorityApplied;
    static bool _pacedGcPolicyApplied;
    static double _nextFrameMs;
    static double _lastThrottleReturnMs;

    internal static bool RealTimeThrottleActive
    {
        get
        {
            if (!_unthrottled)
                return true;
            if (_throttleOnScriptStage != null)
                return ShouldThrottleForStage(
                    _stageThrottleLatched,
                    InputManager.CurrentScriptStage,
                    _throttleOnScriptStage);
            return _throttleAfterInputPoll > 0 &&
                InputManager.CurrentPoll >= _throttleAfterInputPoll;
        }
    }

    static string? NormalizeStage(string? stage)
    {
        if (string.IsNullOrWhiteSpace(stage))
            return null;
        return stage.Trim().ToLowerInvariant().Replace(' ', '_').Replace('-', '_');
    }

    internal static bool ShouldThrottleForStage(
        bool latched,
        string? currentStage,
        string targetStage) =>
        latched || currentStage == targetStage;

    //maybe not the best but it seens to work for now
    public static double Throttle()
    {
        if (_unthrottled)
        {
            if (_throttleOnScriptStage != null)
            {
                if (!_stageThrottleLatched)
                {
                    if (InputManager.CurrentScriptStage != _throttleOnScriptStage)
                        return 0;
                    // Once the requested scripted stage is reached, pacing is
                    // permanent. Stage names describe successive scenes; they
                    // are not scoped modes. Unlatching on the next stage would
                    // silently return a long soak to unlimited host speed.
                    _stageThrottleLatched = true;
                    _nextFrameMs = _clock.Elapsed.TotalMilliseconds;
                    _lastThrottleReturnMs = _nextFrameMs;
                    Console.Error.WriteLine(
                        $"[PERF] throttle engaged at scripted stage " +
                        $"'{_throttleOnScriptStage}' poll={InputManager.CurrentPoll}");
                }
            }
            else if (_throttleAfterInputPoll == 0 ||
                InputManager.CurrentPoll < _throttleAfterInputPoll)
            {
                return 0;
            }
        }
        ApplyPacedThreadPriority();
        ApplyPacedGcPolicy();
        _nextFrameMs += FrameMs;
        double now = _clock.Elapsed.TotalMilliseconds;
        double wait = _nextFrameMs - now;
        bool resetDeadline = ShouldResetDeadline(wait);
        if (resetDeadline)
        {
            _nextFrameMs = now;
            TraceLongFrame(now, wait, resetDeadline);
            _lastThrottleReturnMs = now;
            return 0;
        }

        double waitedFrom = now;
        if (wait > 2.0)
            Thread.Sleep(Math.Max(0, (int)(wait - 1.0)));
        while ((now = _clock.Elapsed.TotalMilliseconds) < _nextFrameMs)
            Thread.SpinWait(32);
        TraceLongFrame(now, wait, resetDeadline);
        _lastThrottleReturnMs = now;
        return Math.Max(0, now - waitedFrom);
    }

    internal static bool ShouldResetDeadline(double requestedWaitMs) =>
        requestedWaitMs < -MaximumCatchUpDebtMs;

    static void ApplyPacedThreadPriority()
    {
        if (_pacedThreadPriorityApplied || !OperatingSystem.IsWindows())
            return;
        _pacedThreadPriorityApplied = true;
        try
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            if (_tracePerformance)
                Console.Error.WriteLine(
                    "[PERF] paced emulation thread priority=Highest");
        }
        catch (Exception exception) when (
            exception is ThreadStateException or System.Security.SecurityException)
        {
            if (_tracePerformance)
                Console.Error.WriteLine(
                    $"[PERF] unable to raise paced emulation thread priority: " +
                    $"{exception.Message}");
        }
    }

    static void ApplyPacedGcPolicy()
    {
        if (_pacedGcPolicyApplied)
            return;
        _pacedGcPolicyApplied = true;
        // The renderer's long-lived pools keep steady-state allocation low.
        // Ask the workstation GC to avoid foreground gen-2 pauses during
        // paced play; background collection can then use the vblank margin.
        if (GCSettings.LatencyMode == GCLatencyMode.Interactive)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        if (_tracePerformance)
            Console.Error.WriteLine(
                $"[PERF] GC latency={GCSettings.LatencyMode}");
    }

    static void TraceLongFrame(
        double completedMs,
        double requestedWaitMs,
        bool resetDeadline)
    {
        if (!_tracePerformance || _lastThrottleReturnMs <= 0)
            return;
        double intervalMs = completedMs - _lastThrottleReturnMs;
        // A one-frame debt limit can reset after an ordinary scheduler wake-up.
        // Logging every such reset adds enough redirected stderr traffic to
        // perturb the paced run, so retain this trace for genuinely long frames.
        if (intervalMs < 40.0 && !resetDeadline)
            return;
        double deadlineLateMs = completedMs - _nextFrameMs;
        Console.Error.WriteLine(
            $"[PERF-PACING] poll={InputManager.CurrentPoll} " +
            $"intervalMs={intervalMs:F3} requestedWaitMs={requestedWaitMs:F3} " +
            $"deadlineLateMs={deadlineLateMs:F3} reset={resetDeadline}");
    }
}
