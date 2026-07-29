using System.Diagnostics;

namespace RecompOne.Runtime.Host;

internal static class FrameClock
{
    // NTSC PlayStation VBlank is 59.94 Hz. Sleeping for a truncated number of
    // milliseconds on every frame produces visible pacing jitter on Windows,
    // so sleep most of the interval and use a short spin only at the deadline.
    const double FrameMs = 1000.0 * 1001.0 / 60000.0;

    static readonly Stopwatch _clock = Stopwatch.StartNew();
    static readonly bool _unthrottled =
        Environment.GetEnvironmentVariable("RECOMPONE_UNTHROTTLED") == "1";
    static readonly int _throttleAfterInputPoll =
        int.TryParse(Environment.GetEnvironmentVariable("RECOMPONE_THROTTLE_AFTER_INPUT_POLL"), out int throttlePoll)
            ? Math.Max(0, throttlePoll)
            : 0;
    static double _nextFrameMs;

    //maybe not the best but it seens to work for now
    public static double Throttle()
    {
        if (_unthrottled &&
            (_throttleAfterInputPoll == 0 || InputManager.CurrentPoll < _throttleAfterInputPoll))
            return 0;
        _nextFrameMs += FrameMs;
        double now = _clock.Elapsed.TotalMilliseconds;
        double wait = _nextFrameMs - now;
        if (wait < -100)
        {
            _nextFrameMs = now;
            return 0;
        }

        double waitedFrom = now;
        if (wait > 2.0)
            Thread.Sleep(Math.Max(0, (int)(wait - 1.0)));
        while ((now = _clock.Elapsed.TotalMilliseconds) < _nextFrameMs)
            Thread.SpinWait(32);
        return Math.Max(0, now - waitedFrom);
    }
}
