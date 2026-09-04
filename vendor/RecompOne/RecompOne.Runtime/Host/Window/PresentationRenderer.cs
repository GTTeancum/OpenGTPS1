using System.Diagnostics;
using Vortice.Mathematics;

namespace RecompOne.Runtime.Host.Window;

/// <summary>D3D11-only final presentation, capture, and video resolve.</summary>
internal sealed class PresentationRenderer : IDisposable
{
    readonly D3D11Renderer _renderer;
    D3D11Renderer.Texture? _upscale;
    D3D11Renderer.Texture? _fxaa;
    D3D11Renderer.Texture? _video;
    int _lastSourceWidth, _lastSourceHeight;
    int _lastOutputWidth, _lastOutputHeight;
    bool _lastFxaa;

    readonly string? _videoCapturePath = Environment.GetEnvironmentVariable("RECOMPONE_VIDEO_CAPTURE");
    readonly int _videoStartPoll = int.TryParse(Environment.GetEnvironmentVariable("RECOMPONE_VIDEO_START_INPUT_POLL"), out int videoStartPoll) ? Math.Max(0, videoStartPoll) : 0;
    readonly int _videoEndPoll = int.TryParse(Environment.GetEnvironmentVariable("RECOMPONE_VIDEO_END_INPUT_POLL"), out int videoEndPoll) ? Math.Max(0, videoEndPoll) : 0;
    readonly int _videoOutputWidth = int.TryParse(Environment.GetEnvironmentVariable("RECOMPONE_VIDEO_WIDTH"), out int videoWidth) ? Math.Clamp(videoWidth, 160, 1920) : 640;
    readonly int _videoOutputHeight = int.TryParse(Environment.GetEnvironmentVariable("RECOMPONE_VIDEO_HEIGHT"), out int videoHeight) ? Math.Clamp(videoHeight, 120, 1080) : 480;
    readonly bool _videoCaptureEveryPresentation = Environment.GetEnvironmentVariable("RECOMPONE_VIDEO_FPS") == "60";
    readonly string _videoFrameRate = Environment.GetEnvironmentVariable("RECOMPONE_VIDEO_FPS") == "60" ? "60000/1001" : "30000/1001";
    readonly int _videoCrf = int.TryParse(Environment.GetEnvironmentVariable("RECOMPONE_VIDEO_CRF"), out int videoCrf) ? Math.Clamp(videoCrf, 0, 51) : 12;
    readonly int _videoFrameLimit = int.TryParse(Environment.GetEnvironmentVariable("RECOMPONE_VIDEO_CAPTURE_FRAMES"), out int videoFrames) ? Math.Max(0, videoFrames) : 0;
    Process? _videoProcess;
    Stream? _videoInput;
    byte[] _readback = [];
    byte[] _videoRgb = [];
    int _videoPresentationFrame, _videoWrittenFrames;
    bool _videoFinished;

    public bool Ready { get; private set; }

    public PresentationRenderer(D3D11Renderer renderer) => _renderer = renderer;

    public void Initialize()
    {
        _upscale = _renderer.CreateTexture(1, 1, renderTarget: true);
        _fxaa = _renderer.CreateTexture(1, 1, renderTarget: true);
        if (!string.IsNullOrWhiteSpace(_videoCapturePath))
            _video = _renderer.CreateTexture(_videoOutputWidth, _videoOutputHeight, renderTarget: true);
        Ready = true;
    }

    public D3D11Renderer.Texture Render(D3D11Renderer.Texture source,
        int sourceWidth, int sourceHeight, int outputWidth, int outputHeight,
        bool fxaa, string? captureLabel = null)
    {
        if (!Ready || sourceWidth <= 0 || sourceHeight <= 0) return source;
        outputWidth = Math.Clamp(outputWidth, 1, 8192);
        outputHeight = Math.Clamp(outputHeight, 1, 8192);
        _renderer.EnsureTexture(_upscale!, outputWidth, outputHeight, renderTarget: true);
        _renderer.EnsureTexture(_fxaa!, outputWidth, outputHeight, renderTarget: true);
        if (sourceWidth != _lastSourceWidth || sourceHeight != _lastSourceHeight ||
            outputWidth != _lastOutputWidth || outputHeight != _lastOutputHeight || fxaa != _lastFxaa)
        {
            Console.WriteLine($"[Host] presentation source={sourceWidth}x{sourceHeight} output={outputWidth}x{outputHeight} aa={(fxaa ? "FXAA" : "Off")} api=D3D11");
            (_lastSourceWidth, _lastSourceHeight) = (sourceWidth, sourceHeight);
            (_lastOutputWidth, _lastOutputHeight) = (outputWidth, outputHeight);
            _lastFxaa = fxaa;
        }

        _renderer.DrawFullscreen(source, _upscale!.Target!, outputWidth, outputHeight, linear: false);
        D3D11Renderer.Texture final = _upscale;
        if (fxaa)
        {
            _renderer.DrawFullscreen(_upscale, _fxaa!.Target!, outputWidth, outputHeight, linear: true, fxaa: true);
            final = _fxaa;
        }
        if (!string.IsNullOrEmpty(captureLabel)) CapturePpm(final, captureLabel, fxaa);
        CaptureVideoFrame(final);
        _renderer.RestoreBackBuffer();
        return final;
    }

    void EnsureReadback(int bytes)
    {
        if (_readback.Length < bytes) _readback = new byte[bytes];
    }

    void CapturePpm(D3D11Renderer.Texture texture, string label, bool fxaa)
    {
        int rgbaBytes = texture.Width * texture.Height * 4;
        EnsureReadback(rgbaBytes);
        _renderer.Readback(texture, _readback.AsSpan(0, rgbaBytes));
        byte[] rgb = new byte[texture.Width * texture.Height * 3];
        RgbaToRgb(_readback.AsSpan(0, rgbaBytes), rgb);
        string mode = fxaa ? "fxaa" : "off";
        string path = $"recompone_present_{label}_{texture.Width}x{texture.Height}_{mode}.ppm";
        using var output = File.Create(path);
        output.Write(System.Text.Encoding.ASCII.GetBytes($"P6\n{texture.Width} {texture.Height}\n255\n"));
        output.Write(rgb);
        Console.WriteLine(
            $"[Host] captured presentation '{label}' at " +
            $"{texture.Width}x{texture.Height} aa={mode} api=D3D11 to {path}");
    }

    void CaptureVideoFrame(D3D11Renderer.Texture source)
    {
        if (_videoFinished || string.IsNullOrWhiteSpace(_videoCapturePath)) return;
        if (_videoFrameLimit > 0 && _videoWrittenFrames >= _videoFrameLimit) { FinishVideo(); return; }
        int poll = InputManager.CurrentPoll;
        if (poll < _videoStartPoll) return;
        if (_videoEndPoll > 0 && poll >= _videoEndPoll) { FinishVideo(); return; }
        if (!_videoCaptureEveryPresentation && (_videoPresentationFrame++ & 1) != 0) return;
        if (_videoCaptureEveryPresentation) _videoPresentationFrame++;

        try
        {
            EnsureVideoEncoder();
            _renderer.Clear(_video!, new Color4(0f, 0f, 0f, 1f));
            double scale = Math.Min((double)_videoOutputWidth / source.Width, (double)_videoOutputHeight / source.Height);
            int width = Math.Clamp((int)Math.Round(source.Width * scale), 1, _videoOutputWidth);
            int height = Math.Clamp((int)Math.Round(source.Height * scale), 1, _videoOutputHeight);
            int x = (_videoOutputWidth - width) / 2;
            int y = (_videoOutputHeight - height) / 2;
            _renderer.DrawFullscreen(source, _video!.Target!, _videoOutputWidth, _videoOutputHeight,
                linear: true, x: x, y: y, drawWidth: width, drawHeight: height);

            int rgbaBytes = _videoOutputWidth * _videoOutputHeight * 4;
            int rgbBytes = _videoOutputWidth * _videoOutputHeight * 3;
            EnsureReadback(rgbaBytes);
            if (_videoRgb.Length != rgbBytes) _videoRgb = new byte[rgbBytes];
            _renderer.Readback(_video, _readback.AsSpan(0, rgbaBytes));
            RgbaToRgb(_readback.AsSpan(0, rgbaBytes), _videoRgb);
            _videoInput!.Write(_videoRgb);
            _videoWrittenFrames++;
            if (_videoFrameLimit > 0 && _videoWrittenFrames >= _videoFrameLimit) FinishVideo();
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"[Host] video capture failed: {exception.Message}");
            FinishVideo();
        }
    }

    static void RgbaToRgb(ReadOnlySpan<byte> rgba, Span<byte> rgb)
    {
        for (int source = 0, destination = 0; source + 3 < rgba.Length; source += 4, destination += 3)
        {
            rgb[destination] = rgba[source];
            rgb[destination + 1] = rgba[source + 1];
            rgb[destination + 2] = rgba[source + 2];
        }
    }

    void EnsureVideoEncoder()
    {
        if (_videoProcess != null) return;
        string path = Path.GetFullPath(_videoCapturePath!);
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        var start = new ProcessStartInfo { FileName = "ffmpeg", UseShellExecute = false, RedirectStandardInput = true, CreateNoWindow = true };
        foreach (string argument in new[]
        {
            "-y", "-loglevel", "error", "-f", "rawvideo", "-pix_fmt", "rgb24",
            "-video_size", $"{_videoOutputWidth}x{_videoOutputHeight}", "-framerate", _videoFrameRate,
            "-i", "pipe:0", "-an", "-c:v", "libx264", "-preset", "medium", "-crf", _videoCrf.ToString(),
            "-pix_fmt", "yuv420p", "-movflags", "+faststart", path,
        }) start.ArgumentList.Add(argument);
        _videoProcess = Process.Start(start) ?? throw new InvalidOperationException("ffmpeg did not start");
        _videoInput = _videoProcess.StandardInput.BaseStream;
        Console.Error.WriteLine($"[Host] D3D11 video capture started at input poll {InputManager.CurrentPoll}: {_videoOutputWidth}x{_videoOutputHeight} at {_videoFrameRate} fps -> {path}");
    }

    void FinishVideo()
    {
        if (_videoFinished) return;
        _videoFinished = true;
        try
        {
            _videoInput?.Flush();
            _videoInput?.Dispose();
            _videoInput = null;
            if (_videoProcess != null)
            {
                if (!_videoProcess.WaitForExit(30000)) _videoProcess.Kill();
                Console.Error.WriteLine($"[Host] video capture complete: frames={_videoWrittenFrames}/{_videoFrameLimit} ffmpeg exit={_videoProcess.ExitCode}");
                _videoProcess.Dispose();
                _videoProcess = null;
                if (Environment.GetEnvironmentVariable("RECOMPONE_EXIT_AFTER_VIDEO_CAPTURE") == "1") Runtime.RequestShutdown();
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"[Host] video finalization failed: {exception.Message}");
        }
    }

    public void Dispose()
    {
        FinishVideo();
        _renderer.DisposeTexture(_video);
        _renderer.DisposeTexture(_fxaa);
        _renderer.DisposeTexture(_upscale);
    }
}
