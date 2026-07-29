using System.Text;
using RecompOne.Runtime.Host;

namespace RecompOne.Runtime.Hle;

/// <summary>
/// Writes one bounded, deterministic frame of the projected GT2 triangle
/// stream plus native PS1 VRAM. This is a migration/diagnostic format: it
/// preserves real game draw data while the world-space scene extractor is
/// being developed.
/// </summary>
internal sealed class ProjectedSceneCapture : IDisposable
{
    const uint Version = 1;
    const int HeaderSize = 80;
    const int TriangleStride = 96;
    const int MaxTriangles = 262_144; // 25 MiB triangle payload, hard cap.

    readonly string? _outputPath;
    readonly string? _temporaryPath;
    readonly int _targetInputPoll;

    FileStream? _stream;
    BinaryWriter? _writer;
    long _geometryFrame;
    uint _triangleCount;
    bool _armed;
    bool _capturing;
    bool _reportedFirstTriangle;
    int _viewportX;
    int _viewportY;
    int _viewportWidth;
    int _viewportHeight;
    long _viewportArea;
    bool _truncated;
    bool _completed;
    bool _failed;

    public bool Enabled => _outputPath != null && !_completed && !_failed;
    public bool NeedsVramSnapshot => Enabled && _capturing;

    public ProjectedSceneCapture()
    {
        string? configuredPath =
            Environment.GetEnvironmentVariable("RECOMPONE_GPU_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(configuredPath))
            return;

        if (!int.TryParse(
                Environment.GetEnvironmentVariable(
                    "RECOMPONE_GPU_CAPTURE_INPUT_POLL"),
                out _targetInputPoll) ||
            _targetInputPoll < 0)
        {
            Console.Error.WriteLine(
                "[GPU-Capture] RECOMPONE_GPU_CAPTURE_INPUT_POLL must be " +
                "a non-negative integer; capture disabled");
            return;
        }

        _outputPath = Path.GetFullPath(configuredPath);
        _temporaryPath = _outputPath + ".tmp";
        if (_targetInputPoll == 0)
            _armed = true;

        Console.Error.WriteLine(
            $"[GPU-Capture] armed path={_outputPath} " +
            $"targetPoll={_targetInputPoll} maxTriangles={MaxTriangles}");
    }

    public void RecordTriangle(
        long pendingFrame,
        int inputPoll,
        in HleDrawEnv env,
        in HleVertex a,
        in HleVertex b,
        in HleVertex c,
        in PrimFlags flags)
    {
        if (!Enabled)
            return;
        if (!_reportedFirstTriangle)
        {
            _reportedFirstTriangle = true;
            Console.Error.WriteLine(
                $"[GPU-Capture] first triangle observed " +
                $"frame={pendingFrame} poll={inputPoll}");
        }
        if (!_armed && inputPoll >= _targetInputPoll)
        {
            _armed = true;
            Console.Error.WriteLine(
                $"[GPU-Capture] target reached at poll={inputPoll}; " +
                "waiting for next geometry list");
        }
        if (!_armed)
            return;
        if (!_capturing)
        {
            _capturing = true;
            _geometryFrame = pendingFrame;
            Console.Error.WriteLine(
                $"[GPU-Capture] geometry capture started " +
                $"frame={_geometryFrame} poll={inputPoll}");
        }
        if (_triangleCount >= MaxTriangles)
        {
            _truncated = true;
            return;
        }

        int viewportWidth = env.ClipX1 - env.ClipX0 + 1;
        int viewportHeight = env.ClipY1 - env.ClipY0 + 1;
        long viewportArea = (long)viewportWidth * viewportHeight;
        if (
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
        }

        try
        {
            EnsureOpen();
            uint primitiveFlags = 0;
            if (flags.Textured) primitiveFlags |= 1U << 0;
            if (flags.SemiTrans) primitiveFlags |= 1U << 1;
            if (flags.RawTexture) primitiveFlags |= 1U << 2;
            if (flags.Gouraud) primitiveFlags |= 1U << 3;

            _writer!.Write(primitiveFlags);
            _writer.Write(flags.TPage);
            _writer.Write(flags.Clut);
            _writer.Write(flags.OtIndex);
            _writer.Write(ToInt16(env.ClipX0));
            _writer.Write(ToInt16(env.ClipY0));
            _writer.Write(ToInt16(env.ClipX1));
            _writer.Write(ToInt16(env.ClipY1));
            _writer.Write(ToInt16(env.TwMaskX));
            _writer.Write(ToInt16(env.TwMaskY));
            _writer.Write(ToInt16(env.TwOffX));
            _writer.Write(ToInt16(env.TwOffY));

            uint environmentFlags = 0;
            if (env.SetMask) environmentFlags |= 1U << 0;
            if (env.CheckMask) environmentFlags |= 1U << 1;
            if (env.Dither) environmentFlags |= 1U << 2;
            _writer.Write(environmentFlags);

            WriteVertex(in a);
            WriteVertex(in b);
            WriteVertex(in c);
            _writer.Write(0U);
            _triangleCount++;
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
    }

    public void OnPresentedFrame(
        long presentedFrame,
        int inputPoll,
        in HleDispEnv display,
        ReadOnlySpan<ushort> vram)
    {
        if (!Enabled)
            return;

        if (_capturing && presentedFrame >= _geometryFrame)
        {
            try
            {
                if (vram.Length != VramShadow.Width * VramShadow.Height)
                    throw new InvalidDataException(
                        $"VRAM snapshot has {vram.Length} words");

                EnsureOpen();
                long vramOffset = _stream!.Position;
                foreach (ushort pixel in vram)
                    _writer!.Write(pixel);
                long vramSize = _stream.Position - vramOffset;

                WriteHeader(
                    presentedFrame,
                    inputPoll,
                    in display,
                    vramOffset,
                    vramSize);
                _writer!.Flush();
                _stream!.Flush(true);
                _writer.Dispose();
                _writer = null;
                _stream = null;

                File.Move(_temporaryPath!, _outputPath!, true);
                _completed = true;
                Console.Error.WriteLine(
                    $"[GPU-Capture] complete frame={presentedFrame} " +
                    $"poll={inputPoll} triangles={_triangleCount} " +
                    $"bytes={new FileInfo(_outputPath!).Length} " +
                    $"truncated={_truncated} path={_outputPath}");
            }
            catch (Exception exception)
            {
                Fail(exception);
            }
            return;
        }

        if (!_armed && inputPoll >= _targetInputPoll)
        {
            _armed = true;
            Console.Error.WriteLine(
                $"[GPU-Capture] target reached at poll={inputPoll}; " +
                "waiting for next geometry list");
        }
    }

    void EnsureOpen()
    {
        if (_writer != null)
            return;

        string? directory = Path.GetDirectoryName(_outputPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        if (File.Exists(_temporaryPath))
            File.Delete(_temporaryPath);

        _stream = new FileStream(
            _temporaryPath!,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.Read,
            64 * 1024,
            FileOptions.SequentialScan);
        _writer = new BinaryWriter(_stream, Encoding.UTF8, leaveOpen: false);
        _writer.Write(new byte[HeaderSize]);
    }

    void WriteVertex(in HleVertex vertex)
    {
        _writer!.Write(vertex.X);
        _writer.Write(vertex.Y);
        _writer.Write(vertex.Z);
        _writer.Write(vertex.U);
        _writer.Write(vertex.V);
        _writer.Write(vertex.R);
        _writer.Write(vertex.G);
        _writer.Write(vertex.B);
        _writer.Write((byte)(vertex.HasGteZ ? 1 : 0));
    }

    void WriteHeader(
        long frame,
        int inputPoll,
        in HleDispEnv display,
        long vramOffset,
        long vramSize)
    {
        int outputX = _viewportArea > 0 ? _viewportX : display.X;
        int outputY = _viewportArea > 0 ? _viewportY : display.Y;
        int outputWidth =
            _viewportArea > 0 ? _viewportWidth : display.W;
        int outputHeight =
            _viewportArea > 0 ? _viewportHeight : display.H;
        _stream!.Position = 0;
        _writer!.Write(Encoding.ASCII.GetBytes("OGTPCAP\0"));
        _writer.Write(Version);
        _writer.Write((uint)HeaderSize);
        _writer.Write((ulong)frame);
        _writer.Write(inputPoll);
        _writer.Write(outputX);
        _writer.Write(outputY);
        _writer.Write(outputWidth);
        _writer.Write(outputHeight);
        _writer.Write(_triangleCount);
        _writer.Write((uint)TriangleStride);
        _writer.Write((uint)VramShadow.Width);
        _writer.Write((uint)VramShadow.Height);
        _writer.Write((ulong)vramOffset);
        _writer.Write((ulong)vramSize);
        uint captureFlags = _truncated ? 1U : 0U;
        if (display.Rgb24)
            captureFlags |= 1U << 1;
        _writer.Write(captureFlags);
        _stream.Position = vramOffset + vramSize;
    }

    static short ToInt16(int value) =>
        (short)Math.Clamp(value, short.MinValue, short.MaxValue);

    void Fail(Exception exception)
    {
        _failed = true;
        Console.Error.WriteLine(
            $"[GPU-Capture] failed: {exception.GetType().Name}: " +
            exception.Message);
        Dispose();
    }

    public void Dispose()
    {
        _writer?.Dispose();
        _writer = null;
        _stream = null;
        if (!_completed && _temporaryPath != null)
        {
            try
            {
                if (File.Exists(_temporaryPath))
                    File.Delete(_temporaryPath);
            }
            catch
            {
                // Best-effort cleanup during process shutdown.
            }
        }
    }
}
