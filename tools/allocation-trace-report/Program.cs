using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Etlx;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

if (args is ["--methods"])
{
    foreach (var method in typeof(TraceLog).GetMethods()
        .Where(method => method.Name.Contains("EventPipe")))
        Console.WriteLine(method);
    return 0;
}

if (args.Length != 1 || !File.Exists(args[0]))
{
    Console.Error.WriteLine(
        "usage: AllocationTraceReport <trace.nettrace>");
    return 2;
}

var byType = new Dictionary<string, (long Samples, long Bytes)>();
var byStack = new Dictionary<string, (long Samples, long Bytes)>();
string input = Path.GetFullPath(args[0]);
string etlx = Path.ChangeExtension(input, ".etlx");
TraceLog.CreateFromEventPipeDataFile(input, etlx, new TraceLogOptions());
using var traceLog = TraceLog.OpenOrConvert(etlx);
using var source = traceLog.Events.GetSource();
source.Clr.GCAllocationTick += allocation =>
{
    string type = string.IsNullOrWhiteSpace(allocation.TypeName)
        ? "<unknown>"
        : allocation.TypeName;
    long bytes = allocation.AllocationAmount64;
    byType.TryGetValue(type, out var typeTotal);
    byType[type] = (typeTotal.Samples + 1, typeTotal.Bytes + bytes);

    TraceCallStack? frame = allocation.CallStack();
    var names = new List<string>(12);
    while (frame != null && names.Count < 12)
    {
        string name = frame.CodeAddress.FullMethodName;
        if (!string.IsNullOrWhiteSpace(name))
            names.Add(name);
        frame = frame.Caller;
    }
    string stack = names.Count == 0
        ? "<no stack>"
        : string.Join(" <- ", names);
    byStack.TryGetValue(stack, out var stackTotal);
    byStack[stack] = (
        stackTotal.Samples + 1,
        stackTotal.Bytes + bytes);
};
source.Process();

Console.WriteLine("Top allocation types (sampled bytes):");
foreach (var entry in byType
    .OrderByDescending(entry => entry.Value.Bytes)
    .Take(30))
{
    Console.WriteLine(
        $"{entry.Value.Bytes / 1024.0 / 1024.0,10:F2} MiB " +
        $"{entry.Value.Samples,8} samples  {entry.Key}");
}

Console.WriteLine();
Console.WriteLine("Top allocation stacks (sampled bytes):");
foreach (var entry in byStack
    .OrderByDescending(entry => entry.Value.Bytes)
    .Take(30))
{
    Console.WriteLine(
        $"{entry.Value.Bytes / 1024.0 / 1024.0,10:F2} MiB " +
        $"{entry.Value.Samples,8} samples  {entry.Key}");
}

return 0;
