using System.Text;

namespace RecompOne.Runtime.Diagnostics;

public static class ConsoleMirror
{
    const int MaxLines = 4000;
    const int MaxSessionLogChars = 4 * 1024 * 1024;

    static readonly object _gate = new();
    static readonly List<string> _lines = new();
    static readonly StringBuilder _pending = new();
    static TextWriter? _sessionLog;
    static int _version;
    static bool _installed;

    public static int Version { get { lock (_gate) return _version; } }

    public static void Install()
    {
        if (_installed) return;
        _installed = true;
        TextWriter originalOut = Console.Out;
        TextWriter originalError = Console.Error;
        string? sessionLogPath = null;
        try
        {
            string logDirectory = Path.Combine(
                AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logDirectory);
            sessionLogPath = Path.Combine(
                logDirectory, "OpenGTPS1-latest.log");
            _sessionLog = new BoundedSessionLog(
                sessionLogPath, MaxSessionLogChars);
            _sessionLog.WriteLine(
                $"[Log] OpenGTPS1 session UTC={DateTimeOffset.UtcNow:O}");
            _sessionLog.WriteLine(
                $"[Log] command={Environment.CommandLine}");
        }
        catch (Exception exception)
        {
            originalError.WriteLine(
                $"[Log] session log unavailable: {exception.Message}");
        }

        Console.SetOut(new Tee(originalOut));
        Console.SetError(new Tee(originalError));
        if (sessionLogPath != null)
            Console.WriteLine($"[Log] bounded session log={sessionLogPath}");
    }

    public static void Clear()
    {
        lock (_gate)
        {
            _lines.Clear();
            _pending.Clear();
            _version++;
        }
    }

    public static int SnapshotInto(List<string> dst)
    {
        lock (_gate)
        {
            dst.Clear();
            dst.AddRange(_lines);
            if (_pending.Length > 0) dst.Add(_pending.ToString());
            return _version;
        }
    }

    static void Append(string? text)
    {
        if (string.IsNullOrEmpty(text)) return;
        lock (_gate)
        {
            foreach (char c in text)
            {
                if (c == '\n') FlushPendingLocked();
                else if (c != '\r') _pending.Append(c);
            }
            _version++;
        }
    }

    static void AppendChar(char c)
    {
        lock (_gate)
        {
            if (c == '\n') FlushPendingLocked();
            else if (c != '\r') _pending.Append(c);
            _version++;
        }
    }

    static void FlushPendingLocked()
    {
        _lines.Add(_pending.ToString());
        _pending.Clear();
        if (_lines.Count > MaxLines) _lines.RemoveRange(0, _lines.Count - MaxLines);
    }

    sealed class Tee : TextWriter
    {
        readonly TextWriter _inner;
        public Tee(TextWriter inner) => _inner = inner;

        public override Encoding Encoding => _inner.Encoding;

        public override void Write(char value)
        {
            _inner.Write(value);
            _sessionLog?.Write(value);
            AppendChar(value);
        }

        public override void Write(string? value)
        {
            _inner.Write(value);
            _sessionLog?.Write(value);
            Append(value);
        }

        public override void WriteLine(string? value)
        {
            _inner.WriteLine(value);
            _sessionLog?.WriteLine(value);
            Append(value);
            AppendChar('\n');
        }

        public override void Flush()
        {
            _inner.Flush();
            _sessionLog?.Flush();
        }
    }

    sealed class BoundedSessionLog : TextWriter
    {
        readonly object _writeGate = new();
        readonly StreamWriter _writer;
        int _remaining;
        bool _truncationReported;

        public BoundedSessionLog(string path, int maximumChars)
        {
            _remaining = Math.Max(1024, maximumChars);
            _writer = new StreamWriter(
                new FileStream(
                    path,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.ReadWrite),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = true,
            };
        }

        public override Encoding Encoding => _writer.Encoding;

        public override void Write(char value)
        {
            lock (_writeGate)
            {
                if (_remaining > 0)
                {
                    _writer.Write(value);
                    _remaining--;
                }
                else
                {
                    ReportTruncation();
                }
            }
        }

        public override void Write(string? value)
        {
            if (string.IsNullOrEmpty(value)) return;
            lock (_writeGate)
            {
                int count = Math.Min(value.Length, _remaining);
                if (count > 0)
                {
                    _writer.Write(value.AsSpan(0, count));
                    _remaining -= count;
                }
                if (count < value.Length)
                    ReportTruncation();
            }
        }

        public override void WriteLine(string? value)
        {
            Write(value);
            Write(Environment.NewLine);
        }

        public override void Flush()
        {
            lock (_writeGate)
                _writer.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing) return;
            lock (_writeGate)
                _writer.Dispose();
            base.Dispose(disposing);
        }

        void ReportTruncation()
        {
            if (_truncationReported) return;
            _truncationReported = true;
            _writer.WriteLine();
            _writer.WriteLine(
                $"[Log] session log capped at {MaxSessionLogChars} characters");
        }
    }
}
