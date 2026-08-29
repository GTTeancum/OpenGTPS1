using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Silk.NET.SDL;
using Thread = System.Threading.Thread;

namespace RecompOne.Runtime.Host;

internal static unsafe class Audio
{
    const int SampleRate = 44100;
    const int Channels = 2;
    const int NumBuffers = 16;
    const int FramesPerBuffer = 256;
    const uint BytesPerBuffer = FramesPerBuffer * Channels * sizeof(short);
    const uint TargetQueuedBytes = NumBuffers * BytesPerBuffer;

    static Sdl? _sdl;
    static uint _device;
    static readonly short[] _sampleBuf = new short[FramesPerBuffer * Channels];

    static Thread? _mixerThread;
    static volatile Spu? _spu;
    static volatile bool _running;
    static float _masterVolume = 1.0f;
    static long _mixedFrames;
    static volatile bool _firstAudibleBufferReported;
    internal static bool HasProducedAudibleOutput =>
        _firstAudibleBufferReported;
    static readonly bool _traceAudio =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_AUDIO") == "1";
    static long _traceSamples;
    static double _traceSquareSum;
    static int _tracePeak;
    static uint _minimumQueuedBytes = uint.MaxValue;
    static long _queueStarvations;
    static long _lastQueueReportTicks;

    static FileStream? _capture;
    static long _capturedBytes;
    static int _captureBuffersSinceHeader;
    static readonly int _captureStartPoll =
        int.TryParse(Environment.GetEnvironmentVariable("RECOMPONE_AUDIO_CAPTURE_START_INPUT_POLL"), out int captureStartPoll)
            ? Math.Max(0, captureStartPoll)
            : 0;
    static readonly int _captureEndPoll =
        int.TryParse(Environment.GetEnvironmentVariable("RECOMPONE_AUDIO_CAPTURE_END_INPUT_POLL"), out int captureEndPoll)
            ? Math.Max(0, captureEndPoll)
            : 0;
    static readonly bool _capturePreVolume =
        Environment.GetEnvironmentVariable("RECOMPONE_AUDIO_CAPTURE_PRE_VOLUME") == "1";

    public static void Initialize(bool noPhysicalOutput = false)
    {
        try
        {
            if (noPhysicalOutput)
            {
                // A hidden automated run must never touch the user's physical
                // output device. Audio-capture runs still feed this dummy
                // device; silent renderer soaks leave its mixer inactive.
                Environment.SetEnvironmentVariable("SDL_AUDIODRIVER", "dummy");
                Console.Error.WriteLine(
                    "[Host] headless audio backend=dummy (no physical output device)");
            }
            _sdl = Sdl.GetApi();
            if (_sdl.InitSubSystem(Sdl.InitAudio) != 0)
                throw new InvalidOperationException($"SDL audio init failed: {GetError()}");
            string audioDriver =
                Marshal.PtrToStringUTF8((nint)_sdl.GetCurrentAudioDriver()) ??
                "unknown";
            if (noPhysicalOutput &&
                !audioDriver.Equals("dummy", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"headless audio safety refused unexpected SDL driver '{audioDriver}'");

            AudioSpec wanted = new()
            {
                Freq = SampleRate,
                Format = (ushort)Sdl.AudioS16Sys,
                Channels = Channels,
                Samples = FramesPerBuffer,
                Callback = default,
                Userdata = null,
            };
            AudioSpec obtained = default;
            _device = _sdl.OpenAudioDevice((byte*)null, 0, &wanted, &obtained, 0);
            if (_device == 0)
                throw new InvalidOperationException($"SDL audio device open failed: {GetError()}");

            if (obtained.Freq != SampleRate || obtained.Format != (ushort)Sdl.AudioS16Sys ||
                obtained.Channels != Channels)
            {
                throw new InvalidOperationException(
                    $"SDL returned unsupported audio format {obtained.Freq} Hz/0x{obtained.Format:X4}/{obtained.Channels}ch");
            }

            Array.Clear(_sampleBuf);
            OpenCapture();
            bool runMixer = !noPhysicalOutput || _capture != null;
            if (runMixer)
            {
                for (int i = 0; i < NumBuffers; i++)
                    QueueCurrentBuffer();

                _running = true;
                _mixerThread = new Thread(MixerLoop)
                {
                    IsBackground = true,
                    Name = "spu-mixer",
                    Priority = System.Threading.ThreadPriority.AboveNormal,
                };
                _mixerThread.Start();
                _sdl.PauseAudioDevice(_device, 0);
            }
            else
            {
                // A renderer soak is intentionally silent and has no audio
                // capture consumer. SDL's dummy queued-audio implementation
                // can spend roughly half of a CPU core mixing samples nobody
                // can hear, distorting both guest and renderer scheduling.
                // Keep the verified dummy device open for physical-output
                // safety, but do not start the SPU sink unless a headless
                // audio capture explicitly requires it.
                _sdl.PauseAudioDevice(_device, 1);
                Console.Error.WriteLine(
                    "[Host] headless audio mixer inactive (no capture consumer)");
            }
            Console.Error.WriteLine(
                $"[Host] SDL audio ready: driver={audioDriver} device={_device} " +
                $"{obtained.Freq} Hz stereo S16 queue={TargetQueuedBytes} bytes");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"[Host] audio init failed: {e.Message}");
            Shutdown();
        }
    }

    public static void Attach(Spu? spu)
    {
        if (spu != null) _spu = spu;
    }

    public static void SetMasterVolume(float volume)
    {
        _masterVolume = Math.Clamp(volume, 0f, 1f);
    }

    static void MixerLoop()
    {
        while (_running)
        {
            var sdl = _sdl;
            var spu = _spu;
            if (sdl == null || _device == 0 || spu == null)
            {
                Thread.Sleep(3);
                continue;
            }

            uint queued = sdl.GetQueuedAudioSize(_device);
            _minimumQueuedBytes = Math.Min(_minimumQueuedBytes, queued);
            if (queued < BytesPerBuffer)
                _queueStarvations++;
            ReportQueueHealth();

            while (_running && queued < TargetQueuedBytes)
            {
                spu.Mix(_sampleBuf, FramesPerBuffer);
                if (_capturePreVolume)
                {
                    CaptureCurrentBuffer();
                    ReportAudioSummary();
                }
                ApplyMasterVolume();
                ReportFirstAudibleBuffer();
                if (!_capturePreVolume)
                {
                    ReportAudioSummary();
                    CaptureCurrentBuffer();
                }
                QueueCurrentBuffer();
                _mixedFrames += FramesPerBuffer;
                queued += BytesPerBuffer;
            }

            Thread.Sleep(1);
        }
    }

    static void ReportQueueHealth()
    {
        if (!_traceAudio) return;
        long now = Environment.TickCount64;
        if (_lastQueueReportTicks == 0) _lastQueueReportTicks = now;
        if (now - _lastQueueReportTicks < 1000) return;
        Console.Error.WriteLine(
            $"[AUDIO-QUEUE] poll={InputManager.CurrentPoll} " +
            $"min={_minimumQueuedBytes}B/{TargetQueuedBytes}B " +
            $"starvations={_queueStarvations}");
        _minimumQueuedBytes = uint.MaxValue;
        _queueStarvations = 0;
        _lastQueueReportTicks = now;
    }

    static void ApplyMasterVolume()
    {
        float volume = _masterVolume;
        if (volume >= 0.999f) return;
        if (volume <= 0.001f)
        {
            Array.Clear(_sampleBuf);
            return;
        }

        for (int i = 0; i < _sampleBuf.Length; i++)
            _sampleBuf[i] = (short)Math.Clamp((int)MathF.Round(_sampleBuf[i] * volume), short.MinValue, short.MaxValue);
    }

    static void ReportFirstAudibleBuffer()
    {
        if (_firstAudibleBufferReported) return;
        int peak = 0;
        foreach (short sample in _sampleBuf)
            peak = Math.Max(peak, Math.Abs((int)sample));
        if (peak == 0) return;

        _firstAudibleBufferReported = true;
        Console.Error.WriteLine($"[Host] first nonzero SPU output at mixed frame {_mixedFrames}: peak={peak}");
    }

    static void ReportAudioSummary()
    {
        if (!_traceAudio) return;
        foreach (short sample in _sampleBuf)
        {
            int value = sample;
            _tracePeak = Math.Max(_tracePeak, Math.Abs(value));
            _traceSquareSum += (double)value * value;
            _traceSamples++;
        }
        if (_traceSamples < SampleRate * Channels) return;

        double rms = Math.Sqrt(_traceSquareSum / _traceSamples);
        double rmsDb = rms > 0 ? 20.0 * Math.Log10(rms / 32768.0) : double.NegativeInfinity;
        string rmsText = double.IsNegativeInfinity(rmsDb) ? "-inf" : rmsDb.ToString("F2");
        var xaHealth = XaAudio.GetHealthAndReset();
        Console.Error.WriteLine(
            $"[AUDIO] poll={InputManager.CurrentPoll} frame={_mixedFrames} " +
            $"peak={_tracePeak} rms_dbfs={rmsText} " +
            $"cdda={CddaAudio.Playing}/{CddaAudio.BufferedFrames} " +
            $"xa={XaAudio.Playing}/{XaAudio.BufferedSamples} " +
            $"xaUnderflow={xaHealth.underflowFrames} " +
            $"xaRefills={xaHealth.refills} xaMaxGap={xaHealth.maximumGap} " +
            $"ogg={OggMusic.Status}");
        _traceSamples = 0;
        _traceSquareSum = 0;
        _tracePeak = 0;
        _minimumQueuedBytes = uint.MaxValue;
        _queueStarvations = 0;
        _lastQueueReportTicks = 0;
    }

    static void OpenCapture()
    {
        string? path = Environment.GetEnvironmentVariable("RECOMPONE_AUDIO_CAPTURE");
        if (string.IsNullOrWhiteSpace(path)) return;
        path = Path.GetFullPath(path);
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        _capture = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        _capture.Write(new byte[44]);
        WriteCaptureHeader(flush: true);
        Console.Error.WriteLine(
            $"[Host] audio capture: {path} " +
            $"tap={(_capturePreVolume ? "pre-volume" : "post-volume")}");
    }

    static void CaptureCurrentBuffer()
    {
        if (_capture == null) return;
        int poll = InputManager.CurrentPoll;
        if (poll < _captureStartPoll ||
            (_captureEndPoll > 0 && poll >= _captureEndPoll))
            return;
        ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes(_sampleBuf.AsSpan());
        _capture.Seek(0, SeekOrigin.End);
        _capture.Write(bytes);
        _capturedBytes += bytes.Length;
        if (++_captureBuffersSinceHeader >= 64)
        {
            _captureBuffersSinceHeader = 0;
            WriteCaptureHeader(flush: true);
        }
    }

    static void WriteCaptureHeader(bool flush)
    {
        if (_capture == null) return;
        Span<byte> header = stackalloc byte[44];
        "RIFF"u8.CopyTo(header);
        BinaryPrimitives.WriteUInt32LittleEndian(header[4..], checked((uint)(36 + _capturedBytes)));
        "WAVE"u8.CopyTo(header[8..]);
        "fmt "u8.CopyTo(header[12..]);
        BinaryPrimitives.WriteUInt32LittleEndian(header[16..], 16);
        BinaryPrimitives.WriteUInt16LittleEndian(header[20..], 1);
        BinaryPrimitives.WriteUInt16LittleEndian(header[22..], Channels);
        BinaryPrimitives.WriteUInt32LittleEndian(header[24..], SampleRate);
        BinaryPrimitives.WriteUInt32LittleEndian(header[28..], SampleRate * Channels * sizeof(short));
        BinaryPrimitives.WriteUInt16LittleEndian(header[32..], Channels * sizeof(short));
        BinaryPrimitives.WriteUInt16LittleEndian(header[34..], sizeof(short) * 8);
        "data"u8.CopyTo(header[36..]);
        BinaryPrimitives.WriteUInt32LittleEndian(header[40..], checked((uint)_capturedBytes));
        long position = _capture.Position;
        _capture.Position = 0;
        _capture.Write(header);
        _capture.Position = position;
        if (flush) _capture.Flush();
    }

    static void QueueCurrentBuffer()
    {
        var sdl = _sdl ?? throw new InvalidOperationException("SDL audio is not initialized");
        fixed (short* samples = _sampleBuf)
        {
            if (sdl.QueueAudio(_device, samples, BytesPerBuffer) != 0)
                throw new InvalidOperationException($"SDL audio queue failed: {GetError()}");
        }
    }

    static string GetError()
    {
        if (_sdl == null) return "SDL unavailable";
        return Marshal.PtrToStringUTF8((nint)_sdl.GetError()) ?? "unknown SDL error";
    }

    public static void Shutdown()
    {
        Console.Error.WriteLine("[Audio] shutdown stage=mixer-stop");
        _running = false;
        Thread? mixerThread = _mixerThread;
        if (
            mixerThread != null &&
            !mixerThread.Join(TimeSpan.FromSeconds(5))
        )
        {
            // Runtime termination follows immediately after shutdown. Do not
            // deadlock the render thread or tear SDL/capture storage out from
            // under a mixer that is still returning from guest SPU mixing.
            Console.Error.WriteLine(
                "[Audio] shutdown mixer join timed out; " +
                "deferring audio teardown to process exit");
            _mixerThread = null;
            _spu = null;
            return;
        }
        _mixerThread = null;
        _spu = null;
        Console.Error.WriteLine("[Audio] shutdown stage=mixer-stopped");
        _mixedFrames = 0;
        _firstAudibleBufferReported = false;
        _traceSamples = 0;
        _traceSquareSum = 0;
        _tracePeak = 0;

        if (_capture != null)
        {
            WriteCaptureHeader(flush: true);
            _capture.Dispose();
            _capture = null;
            _capturedBytes = 0;
            _captureBuffersSinceHeader = 0;
        }

        if (_sdl != null)
        {
            if (_device != 0)
            {
                _sdl.PauseAudioDevice(_device, 1);
                _sdl.ClearQueuedAudio(_device);
                _sdl.CloseAudioDevice(_device);
                _device = 0;
            }
            _sdl.QuitSubSystem(Sdl.InitAudio);
            _sdl.Dispose();
            _sdl = null;
        }
        Console.Error.WriteLine("[Audio] shutdown stage=complete");
    }
}
