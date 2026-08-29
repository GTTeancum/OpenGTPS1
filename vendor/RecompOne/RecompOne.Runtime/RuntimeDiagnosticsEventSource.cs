using System.Diagnostics.Tracing;

namespace RecompOne.Runtime;

[EventSource(Name = "OpenGTPS1-Diagnostics")]
internal sealed class RuntimeDiagnosticsEventSource : EventSource
{
    public static readonly RuntimeDiagnosticsEventSource Log = new();

    private RuntimeDiagnosticsEventSource()
    {
    }

    [Event(1, Level = EventLevel.Informational)]
    public void LongFrame(
        int poll,
        double intervalMs,
        double guestMs,
        double hostMs,
        double waitMs)
    {
        WriteEvent(1, poll, intervalMs, guestMs, hostMs, waitMs);
    }
}
