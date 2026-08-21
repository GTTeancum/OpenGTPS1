using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using RecompOne.Runtime.Config;
using RecompOne.Runtime.Hardware;
using RecompOne.Runtime.Host.Window;

namespace RecompOne.Runtime.Host;

internal static class HostWindow
{
    static IWindow? _window;
    static GL? _gl;
    static ImGuiController? _imgui;
    static bool _headless;
    static Gpu? _gpu;

    static uint _displayTex;
    static uint _nativeWorldTex;
    static uint _vramTex;
    static uint _ramTex;
    static Hle.GlBackend? _glBackend;
    static PresentationRenderer? _presentationRenderer;

    static byte[] _rgbDisplay = [];
    static ushort[] _hleDisplay = [];
    static byte[] _rgbVram = [];
    static byte[] _ramFront = new byte[Memory.RamLogger.Width * Memory.RamLogger.Height * 4];
    static byte[] _ramBack = new byte[Memory.RamLogger.Width * Memory.RamLogger.Height * 4];
    static Task? _ramTask;
    static volatile bool _ramReady;
    static int _ramFrame;
    static int _displayProbeFrame;
    static bool _nativeWorldAvailable;
    static int _nativeWorldWidth;
    static int _nativeWorldHeight;
    static int _nativeWorldAllocatedWidth;
    static int _nativeWorldAllocatedHeight;
    static int _nativeWorldInputPoll;
    static long _nativeWorldFrame;
    static bool _nativeWorldSynthetic;
    static bool _nativeWorldRepeated;
    static bool _nativeWorldTemporalResetBoundary;
    static bool _nativeWorldWasExpected;
    static bool _nativeWorldPrebuffering;
    static bool _nativeRealTimeThrottleWasActive;
    static bool _nativeInitialPrebufferPending;
    static bool _nativeInitialPrebufferInProgress;
    static int _nativeWorldNonRecentHandoffPolls;
    static int _nativeWorldPrebufferTarget = NativeWorldPrebufferOutputs;
    const int NativeWorldPrebufferOutputs = 8;
    const int NativeWorldInitialPrebufferOutputs = 8;
    const int NativeWorldHandoffFlushPolls = 10;
    const int NativeWorldMaxOutputAgePolls = 6;
    static readonly bool _tracePerformance =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_PERFORMANCE") == "1";
    static long _nativePerfTimestamp;
    static int _nativePerfPresents;
    static int _nativePerfNewFrames;
    static int _nativePerfActualFrames;
    static int _nativePerfSyntheticFrames;
    static int _nativePerfRepeatedFrames;
    static int _nativePerfCompositorFrames;
    static int _nativePerfTransitionHolds;
    static int _nativePerfWorldMissFrames;
    static long _nativePerfAgeTotal;
    static int _nativePerfAgeMaximum;
    static int _nativeRejectedTraceCount;
    static int _nativePendingTraceCount;
    static int _nativeDecisionTraceCount;
    static uint _lastDisplayHash;
    static string? _requestedDisplayCapture;
    static string? _pendingPresentationCapture;
    static readonly string? _outputResolutionOverride =
        Environment.GetEnvironmentVariable("RECOMPONE_OUTPUT_RESOLUTION");
    static readonly string? _antiAliasingOverride =
        Environment.GetEnvironmentVariable("RECOMPONE_ANTI_ALIASING");
    static readonly string? _presentationResolutionOverride =
        Environment.GetEnvironmentVariable("RECOMPONE_PRESENTATION_RESOLUTION");
    static readonly int _presentationCaptureFrame =
        int.TryParse(Environment.GetEnvironmentVariable("RECOMPONE_PRESENTATION_CAPTURE_FRAME"), out int captureFrame)
            ? Math.Max(1, captureFrame)
            : 0;
    static readonly HashSet<int> _presentationCaptureFrames =
        (Environment.GetEnvironmentVariable("RECOMPONE_PRESENTATION_CAPTURE_FRAMES") ?? "")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(value => int.TryParse(value, out int frame) ? frame : 0)
        .Where(frame => frame > 0)
        .ToHashSet();
    static readonly HashSet<long> _presentationCaptureSourceFrames =
        (Environment.GetEnvironmentVariable(
            "RECOMPONE_PRESENTATION_CAPTURE_SOURCE_FRAMES") ?? "")
        .Split(',', StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries)
        .Select(value => long.TryParse(value, out long frame) ? frame : 0)
        .Where(frame => frame > 0)
        .ToHashSet();
    static int _presentationFrame;
    static readonly bool _capturePresentation =
        Environment.GetEnvironmentVariable("RECOMPONE_PRESENTATION_CAPTURE") == "1";
    static readonly bool _disableDisplayCapture =
        Environment.GetEnvironmentVariable("RECOMPONE_DISABLE_DISPLAY_CAPTURE") == "1";
    static readonly bool _captureVideo =
        !string.IsNullOrWhiteSpace(
            Environment.GetEnvironmentVariable("RECOMPONE_VIDEO_CAPTURE"));
    static readonly bool _exitAfterPresentationCapture =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_EXIT_AFTER_PRESENTATION_CAPTURE") == "1";
    static readonly bool _windowVisible =
        Environment.GetEnvironmentVariable("RECOMPONE_WINDOW_VISIBLE") != "0";
    public static bool IsHeadless => _headless || !_windowVisible;
    static readonly int _displayProbeInterval =
        int.TryParse(Environment.GetEnvironmentVariable("RECOMPONE_DISPLAY_PROBE_INTERVAL"), out int interval)
            ? Math.Max(1, interval)
            : 0;

    static bool _layoutPending = true;
    static bool _closed;
    static DiscPickerPopup? _discPicker;

    public static void Initialize(string title)
    {
        ConfigManager.Load();
        var outputSize = ParseOutputResolution(_outputResolutionOverride ?? ConfigManager.View.OutputResolution);

        try
        {
            var options = WindowOptions.Default with
            {
                Size = new Vector2D<int>(outputSize.width, outputSize.height),
                Title = title,
                IsVisible = _windowVisible,
                VSync = false,
                // Keep buffer submission explicit. Hidden/headless sessions
                // still execute the complete render callback (including
                // capture and native texture upload) but have no visible
                // surface to present. Swapping that hidden surface can block
                // inside DWM/GLFW for seconds and falsely attribute an OS
                // compositor pause to the game or native renderer.
                ShouldSwapAutomatically = false,
                UpdatesPerSecond = 0,
                FramesPerSecond = 0,
                WindowState = ConfigManager.View.Fullscreen ? WindowState.Fullscreen : WindowState.Maximized,
                API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Default, new APIVersion(4, 5)),
            };
            _window = Silk.NET.Windowing.Window.Create(options);
            _window.Load += OnLoad;
            _window.Render += OnRender;
            _window.Closing += OnClosing;
            _window.Initialize();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"[Host] window unavailable {e.Message}");
            _headless = true;
        }
    }

    public static void Present(Gpu? gpu)
    {
        long traceStart = _tracePerformance
            ? Stopwatch.GetTimestamp()
            : 0;
        _gpu = gpu;
        gpu?.CapturePresentedFrame();
        long afterCapture = _tracePerformance
            ? Stopwatch.GetTimestamp()
            : 0;
        if (_headless || _window == null) return;
        try { _window.DoEvents(); }
        catch (Exception e) {
            Console.WriteLine(e.Message);
        }
        long afterEvents = _tracePerformance
            ? Stopwatch.GetTimestamp()
            : 0;
        if (_window.IsClosing)
        {
            Console.Error.WriteLine(
                "[Host] window closing observed during Present");
            Runtime.Shutdown();
            Runtime.TerminateProcess(0);
        }
        InputManager.Poll();
        long afterInput = _tracePerformance
            ? Stopwatch.GetTimestamp()
            : 0;
        if (InputManager.ConsumeTopBarToggle())
        {
            ConfigManager.View.HideTopBar = !ConfigManager.View.HideTopBar;
            ConfigManager.SaveView(PanelManager.Panels);
        }
        if (InputManager.ConsumeFullscreenToggle())
        {
            ConfigManager.View.Fullscreen = !ConfigManager.View.Fullscreen;
            SetFullscreen(ConfigManager.View.Fullscreen);
            ConfigManager.SaveView(PanelManager.Panels);
        }
        RenderWindow();
        if (_tracePerformance)
        {
            long afterRender = Stopwatch.GetTimestamp();
            double scale = 1000.0 / Stopwatch.Frequency;
            if ((afterRender - traceStart) * scale >= 40.0)
            {
                Console.Error.WriteLine(
                    $"[Host-Long-Present] " +
                    $"poll={InputManager.CurrentPoll} " +
                    $"captureMs={(afterCapture - traceStart) * scale:F3} " +
                    $"eventsMs={(afterEvents - afterCapture) * scale:F3} " +
                    $"inputMs={(afterInput - afterEvents) * scale:F3} " +
                    $"renderMs={(afterRender - afterInput) * scale:F3} " +
                    $"totalMs={(afterRender - traceStart) * scale:F3}");
            }
        }
    }

    internal static void Pump()
    {
        if (_headless || _window == null) return;
        try { _window.DoEvents(); } catch { }
        if (_window.IsClosing)
        {
            Console.Error.WriteLine(
                "[Host] window closing observed during Pump");
            Runtime.Shutdown();
            Runtime.TerminateProcess(0);
        }
        RenderWindow();
    }

    static void RenderWindow()
    {
        _window!.DoRender();
        if (!_windowVisible)
        {
            if (!_capturePresentation)
            {
                // A telemetry-only headless soak consumes native-world output
                // in OnRender without issuing GL commands. Finishing an empty
                // hidden GL stream can still serialize the driver against the
                // independent D3D11 renderer for hundreds of milliseconds, so
                // there is deliberately nothing to flush in this mode.
                return;
            }
            // SwapBuffers normally flushes and applies backpressure to the GL
            // command queue. A hidden soak deliberately has no swap, so finish
            // the small offscreen presentation explicitly; otherwise queued
            // GL work accumulates and periodically contends with the native
            // D3D11 renderer, creating a test-only output starvation spike.
            long finishStarted = _tracePerformance
                ? Stopwatch.GetTimestamp()
                : 0;
            _gl!.Finish();
            if (_tracePerformance)
            {
                long completed = Stopwatch.GetTimestamp();
                double elapsedMs =
                    (completed - finishStarted) * 1000.0 /
                        Stopwatch.Frequency;
                if (elapsedMs >= 40.0)
                {
                    Console.Error.WriteLine(
                        $"[Host-Long-Headless-Finish] " +
                        $"poll={InputManager.CurrentPoll} " +
                        $"finishMs={elapsedMs:F3}");
                }
            }
            return;
        }
        long started = _tracePerformance
            ? Stopwatch.GetTimestamp()
            : 0;
        _window.SwapBuffers();
        if (_tracePerformance)
        {
            long completed = Stopwatch.GetTimestamp();
            double elapsedMs =
                (completed - started) * 1000.0 / Stopwatch.Frequency;
            if (elapsedMs >= 40.0)
            {
                Console.Error.WriteLine(
                    $"[Host-Long-Swap] poll={InputManager.CurrentPoll} " +
                    $"swapMs={elapsedMs:F3}");
            }
        }
    }

    public static void Shutdown()
    {
        // Silk/GLFW can wait indefinitely when Close is requested from the
        // same render callback that owns the current GL context. Runtime
        // shutdown always terminates the process immediately after this
        // method, so perform the registered close work directly and let
        // Environment.Exit release the native window after resources and
        // capture encoders are finalized.
        Console.Error.WriteLine("[Host] shutdown request=resources");
        OnClosing();
        Console.Error.WriteLine("[Host] shutdown request=input");
        InputManager.Shutdown();
        Console.Error.WriteLine("[Host] shutdown request=returned");
    }

    public static void SetFullscreen(bool on)
    {
        if (_window == null) return;
        _window.WindowState = on ? WindowState.Fullscreen : WindowState.Normal;
        if (!on)
        {
            var size = ParseOutputResolution(ConfigManager.View.OutputResolution);
            _window.Size = new Vector2D<int>(size.width, size.height);
        }
    }

    public static void SetOutputResolution(string resolution)
    {
        if (_window == null || ConfigManager.View.Fullscreen) return;
        var size = ParseOutputResolution(resolution);
        _window.Size = new Vector2D<int>(size.width, size.height);
    }

    static (int width, int height) ParseOutputResolution(string resolution)
    {
        string[] parts = resolution.Split('x', 'X');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out int width) &&
            int.TryParse(parts[1], out int height) &&
            width is >= 640 and <= 7680 && height is >= 480 and <= 4320)
            return (width, height);
        return (1280, 720);
    }

    public static bool IsKeyDown(Key k) => InputManager.IsKeyDown(k);

    internal static void RequestDisplayCapture(string label)
    {
        if (_disableDisplayCapture)
            return;
        string sanitized = new string(
            label.Where(ch => char.IsAsciiLetterOrDigit(ch) || ch == '_').ToArray());
        if (_capturePresentation && Hle.LiveWorldRenderer.Requested)
        {
            // Capture the final modern presentation directly. Re-queueing the
            // same label through the compatibility framebuffer can overwrite
            // a correct native-world image later, after a world-free
            // transition, with an intentionally worldless 2D probe.
            _requestedDisplayCapture = null;
            _pendingPresentationCapture = sanitized;
            return;
        }
        _requestedDisplayCapture = sanitized;
        if (_capturePresentation && Hle.GpuHle.Active)
            _pendingPresentationCapture = sanitized;
    }

    public static void RequestDiscPath() => _discPicker?.Show();

    public static void WaitForValidDisc() // wait for disc path to be valid before running it!!
    {
        if (_headless || _window == null) return;
        while (true)
        {
            var path = ConfigManager.Game.CdPath;
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path)) return;

            try { _window.DoEvents(); } catch { }
            if (_window.IsClosing)
            {
                Runtime.Shutdown();
                Runtime.TerminateProcess(0);
            }
            InputManager.Poll();
            RenderWindow();
        }
    }

    static void OnLoad()
    {
        // GLFW does not consistently honor an initial Maximized state when a
        // visible window is created while another desktop application owns
        // focus. Reassert it after native window creation so the shipping
        // visible path always starts maximized instead of silently falling
        // back to the 640x480 minimum client area. Fullscreen remains an
        // explicit user choice and hidden/headless validation is unaffected.
        if (_windowVisible && !ConfigManager.View.Fullscreen)
            _window!.WindowState = WindowState.Maximized;

        var input = _window!.CreateInput();
        InputManager.Initialize(input);

        _gl = GL.GetApi(_window);
        _gl.ClearColor(0.08f, 0.08f, 0.08f, 1f);

        var fb = _window!.FramebufferSize;
        _gl.Viewport(0, 0, (uint)fb.X, (uint)fb.Y);
        _window.FramebufferResize += size => _gl?.Viewport(0, 0, (uint)size.X, (uint)size.Y);
        _displayTex = CreateTexture(_gl);
        _nativeWorldTex = CreateTexture(_gl);
        _vramTex= CreateTexture(_gl);
        _ramTex = CreateTexture(_gl);
        _presentationRenderer = new PresentationRenderer(_gl);
        _presentationRenderer.Initialize();

        // There is no shipping low-resolution/legacy 3D mode. The command
        // compositor still draws authored 2D menus, videos, HUD, and Results,
        // while all live race/replay worlds are owned by the native renderer.
        const bool highResolution3D = true;
        Hle.GlVram.Scale = 4;
        _glBackend = new Hle.GlBackend(_gl);
        _glBackend.InitGl();
        Hle.GpuHle.Active = highResolution3D;
        Hle.GpuHle.Backend = _glBackend;
        Hle.GpuHle.NativeResolution = false;
        Console.WriteLine(
            $"[Host] color dithering={(ConfigManager.View.Ps1Dithering ? "On" : "Off (modern fixed)")}");
        Console.WriteLine(
            $"[Host] texture smoothing={(ConfigManager.View.TextureSmoothing ? "On (modern fixed)" : "Off")}");
        Console.WriteLine(
            $"[Host] external 4x texture assets={(ConfigManager.View.HighResolutionTextures ? "On (when pack installed)" : "Off")}");
        Console.WriteLine(
            $"[Host] texture projection fix={(ConfigManager.View.PerspectiveCorrectTextures ? "On (modern fixed)" : "Off")}");
        Console.WriteLine(
            $"[Host] graphics preset={ConfigManager.View.GraphicsPreset} modern-only " +
            $"seams={(ConfigManager.View.StabilizeGeometrySeams ? "Stabilized" : "Disabled")} " +
            $"draw-distance={(ConfigManager.View.ExtendedDrawDistance ? "Extended" : "Reduced")} " +
            $"LOD={ConfigManager.View.LevelOfDetail}");
        Console.WriteLine(
            $"[Host] native world renderer=" +
            $"{(Hle.LiveWorldRenderer.Requested ? "Enabled" : "Disabled")}");

        _imgui = new ImGuiController(_gl, _window, input, null, ConfigureImGui);

        PanelManager.Register(new OutputPanel());
        PanelManager.Register(new VramViewerPanel());
        PanelManager.Register(new CpuStatePanel());
        PanelManager.Register(new RamMapPanel());
        PanelManager.Register(new MemoryEditorPanel());
        PanelManager.Register(new SpuViewerPanel());
        PanelManager.Register(new CdDebugPanel());
        PanelManager.Register(new ConsolePanel());
        PanelManager.Register(new OverlayEventsPanel());
        PanelManager.Register(new SettingsPopup());
        PanelManager.Register(new Modding.ModsPopup());
        PanelManager.Register(new AboutPopup());

        SettingsRegistry.Register(new InputSettingsSection());
        SettingsRegistry.Register(new DisplaySettingsSection());
        SettingsRegistry.Register(new AudioSettingsSection());
        MenuRegistry.Register("Guest Vehicles", GuestVehicleMenu.Draw);

        ConfigManager.ApplyViewToPanels(PanelManager.Panels);

        // Standalone loose builds do not expose the legacy disc-image picker.
        // Keep it available to other RecompOne hosts that still launch without
        // a loose directory, but remove BIN/CUE setup from OpenGTPS1's menus.
        if (Runtime.ResolveLoosePath() == null)
        {
            _discPicker = new DiscPickerPopup();
            PanelManager.Register(_discPicker);
            var cdPath = ConfigManager.Game.CdPath;
            if (string.IsNullOrWhiteSpace(cdPath) || !File.Exists(cdPath))
                _discPicker.Show();
        }
    }

    static void ConfigureImGui()
    {
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigWindowsMoveFromTitleBarOnly = true;
        unsafe { io.NativePtr->IniFilename = null; }

        if (Config.ConfigManager.ApplyImGuiLayout())
            _layoutPending = false;
    }

    static void OnRender(double dt)
    {
        long traceStart = _tracePerformance
            ? Stopwatch.GetTimestamp()
            : 0;
        var gl = _gl!;
        if (!_windowVisible && !_capturePresentation && !_captureVideo &&
            string.IsNullOrEmpty(_requestedDisplayCapture))
        {
            // Silent performance/soak runs need to consume and audit every
            // modern-world output, but they do not need an invisible OpenGL
            // upload, ImGui pass, or framebuffer draw. Keep the D3D11 renderer
            // fully active and measure its real queue while removing the
            // otherwise test-only cross-API synchronization path.
            Runtime.RamLog.Tick();
            if (_gpu is { } headlessGpu)
                PresentNativeWorld(null, headlessGpu);
            return;
        }
        _imgui!.Update((float)dt);
        long afterImGuiUpdate = _tracePerformance
            ? Stopwatch.GetTimestamp()
            : 0;
    
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        var fbDef = _window!.FramebufferSize;
        gl.Viewport(0, 0, (uint)fbDef.X, (uint)fbDef.Y);
        gl.ClearColor(0.08f, 0.08f, 0.08f, 1f);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        long afterClear = _tracePerformance
            ? Stopwatch.GetTimestamp()
            : 0;

        Runtime.RamLog.Tick();
        bool trackRamActivity =
            PanelManager.Get<RamMapPanel>()?.IsOpen == true ||
            PanelManager.Get<MemoryEditorPanel>()?.IsOpen == true;
        Memory.RamLogger.TrackReads = trackRamActivity;
        Memory.RamLogger.TrackWrites = trackRamActivity;

        var gpu = _gpu;
        if (gpu != null)
        {
            // A scripted diagnostic may explicitly request the authored VRAM
            // while the native renderer owns presentation (for example, to
            // identify a frontend course choice behind a 3D preview). Consume
            // that one-shot request without changing the visible output path.
            if (!string.IsNullOrEmpty(_requestedDisplayCapture) &&
                Hle.GpuHle.Active &&
                _glBackend is { Ready: true } &&
                gpu.DisplayEnabled)
                ProbeHleDisplay(
                    _glBackend, gpu, gpu.DisplayWidth, gpu.DisplayHeight);

            bool nativePresented = PresentNativeWorld(gl, gpu);
            if (nativePresented)
            {
                // PresentNativeWorld already submitted the completed texture
                // to the output panel.
            }
            else if (gpu.LiveWorldExpected)
            {
                // Never expose the legacy 3D world while the modern stream is
                // warming or unavailable. Menus/videos/results still use the
                // authored 2D command compositor when no live world is expected.
                if (_tracePerformance && _nativePendingTraceCount < 3)
                {
                    _nativePendingTraceCount++;
                    Console.Error.WriteLine(
                        "[Native-Present] modern world pending; " +
                        "legacy 3D fallback suppressed");
                }
            }
            else if (
                Hle.GpuHle.Active &&
                _glBackend is { Ready: true } &&
                gpu.DisplayEnabled
            )
            {
                var wf = _window!.FramebufferSize;
                var (tex, tw, th, aspect) = _glBackend.PresentDisplay(
                    gpu.DisplayX, gpu.DisplayY,
                    gpu.DisplayWidth, gpu.DisplayHeight,
                    gpu.Display24Bit,
                    outW: wf.X, outH: wf.Y);
                ProbeHleDisplay(_glBackend, gpu, gpu.DisplayWidth, gpu.DisplayHeight);
                if (tex != 0) PresentTexture(gl, tex, tw, th, aspect);
                gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                gl.Viewport(0, 0, (uint)wf.X, (uint)wf.Y);
            }
            else
            {
                UploadDisplayTexture(gl, gpu);
            }

            if (PanelManager.Get<VramViewerPanel>()?.IsOpen == true)
                UploadVramTexture(gl, gpu);
        }
        long afterGpu = _tracePerformance
            ? Stopwatch.GetTimestamp()
            : 0;

        if (PanelManager.Get<RamMapPanel>()?.IsOpen == true)
        {
            QueueRamConvert();
            if (_ramReady) FlushRamTexture(gl);
        }

        if (!ConfigManager.View.HideTopBar)
            MainMenuBar.Draw();

        DrawDockspace();
        PanelManager.DrawPanels();
        MenuRegistry.DrawWindows();
        Modding.ModLoadingPopup.Draw();
        long afterPanels = _tracePerformance
            ? Stopwatch.GetTimestamp()
            : 0;
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        gl.Viewport(0, 0, (uint)fbDef.X, (uint)fbDef.Y);
        _imgui.Render();
        if (_tracePerformance)
        {
            long afterImGuiRender = Stopwatch.GetTimestamp();
            double scale = 1000.0 / Stopwatch.Frequency;
            if ((afterImGuiRender - traceStart) * scale >= 40.0)
            {
                Console.Error.WriteLine(
                    $"[Host-Long-Render] " +
                    $"poll={InputManager.CurrentPoll} " +
                    $"imguiUpdateMs={(afterImGuiUpdate - traceStart) * scale:F3} " +
                    $"clearMs={(afterClear - afterImGuiUpdate) * scale:F3} " +
                    $"gpuMs={(afterGpu - afterClear) * scale:F3} " +
                    $"panelsMs={(afterPanels - afterGpu) * scale:F3} " +
                    $"imguiRenderMs={(afterImGuiRender - afterPanels) * scale:F3} " +
                    $"totalMs={(afterImGuiRender - traceStart) * scale:F3}");
            }
        }
    }

    static void DrawDockspace()
    {
        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);

        const ImGuiWindowFlags hostFlags = ImGuiWindowFlags.NoDocking | 
                                           ImGuiWindowFlags.NoTitleBar |
                                           ImGuiWindowFlags.NoCollapse |
                                           ImGuiWindowFlags.NoResize |
                                           ImGuiWindowFlags.NoMove |
                                           ImGuiWindowFlags.NoBringToFrontOnFocus |
                                           ImGuiWindowFlags.NoBackground;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.Begin("##DockHost", hostFlags);
        ImGui.PopStyleVar(3);
        uint dockId = ImGui.GetID("##MainDock");
        int openCount = PanelManager.Panels.Count(p => p.IsOpen && p is not AboutPopup);
        var dockFlags = openCount <= 1 ? (ImGuiDockNodeFlags)4096 : ImGuiDockNodeFlags.None;
        ImGui.DockSpace(dockId, Vector2.Zero, dockFlags);

        if (_layoutPending)
        {
            _layoutPending = false;
            DockBuilder.SetupCenterLayout(dockId, viewport.WorkSize, "Output");
        }

        ImGui.End();
    }

    static void OnClosing()
    {
        if (_closed) return;
        _closed = true;
        Console.Error.WriteLine("[Host] shutdown stage=config");
        ConfigManager.SaveView(PanelManager.Panels);
        ConfigManager.SaveGame();
        Console.Error.WriteLine("[Host] shutdown stage=panels");
        PanelManager.Shutdown();
        Console.Error.WriteLine("[Host] shutdown stage=hle");
        _glBackend?.Dispose();
        Console.Error.WriteLine("[Host] shutdown stage=native-world");
        _gpu?.ShutdownLiveWorldRenderer();
        Console.Error.WriteLine("[Host] shutdown stage=presentation");
        _presentationRenderer?.Dispose();
        Console.Error.WriteLine("[Host] shutdown stage=imgui");
        _imgui?.Dispose();
        Console.Error.WriteLine("[Host] shutdown stage=textures");
        _gl?.DeleteTexture(_displayTex);
        _gl?.DeleteTexture(_nativeWorldTex);
        _gl?.DeleteTexture(_vramTex);
        _gl?.DeleteTexture(_ramTex);
        Console.Error.WriteLine("[Host] shutdown stage=complete");
    }

    static uint CreateTexture(GL gl)
    {
        var tex = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, tex);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        return tex;
    }

    static void UploadDisplayTexture(GL gl, Gpu gpu)
    {
        int w = gpu.DisplayWidth, h = gpu.DisplayHeight;
        if (!gpu.DisplayEnabled || w <= 0 || h <= 0) return;
        int needed = w * h * 3;
        if (_rgbDisplay.Length < needed) _rgbDisplay = new byte[needed];
        ConvertDisplay(gpu, w, h);
        ProbeDisplay(gpu, w, h);
        gl.BindTexture(TextureTarget.Texture2D, _displayTex);
        gl.TexImage2D<byte>(TextureTarget.Texture2D, 0, InternalFormat.Rgb, (uint)w, (uint)h, 0,
            PixelFormat.Rgb, PixelType.UnsignedByte, _rgbDisplay.AsSpan(0, needed));
        PresentTexture(gl, _displayTex, w, h, 4f / 3f);
    }

    static bool PresentNativeWorld(GL? gl, Gpu gpu)
    {
        long traceStart = _tracePerformance
            ? Stopwatch.GetTimestamp()
            : 0;
        bool worldExpected = gpu.LiveWorldExpected;
        bool worldRecentlySeen = gpu.LiveWorldRecentlySeen;
        bool worldStarted = worldExpected && !_nativeWorldWasExpected;
        _nativeWorldWasExpected = worldExpected;
        bool realTimeThrottleActive = FrameClock.RealTimeThrottleActive;
        bool realTimeThrottleStarted =
            realTimeThrottleActive && !_nativeRealTimeThrottleWasActive;
        _nativeRealTimeThrottleWasActive = realTimeThrottleActive;
        if (realTimeThrottleStarted)
            _nativeInitialPrebufferPending = true;
        if (!worldExpected)
        {
            int nextNonRecentHandoffPolls = worldRecentlySeen
                ? 0
                : _nativeWorldNonRecentHandoffPolls + 1;
            bool preserveInitialPrebuffer =
                ShouldPreserveInitialNativeWorldPrebuffer(
                    _nativeInitialPrebufferInProgress,
                    nextNonRecentHandoffPolls);
            if (!preserveInitialPrebuffer)
            {
                _nativeWorldPrebuffering = false;
                _nativeInitialPrebufferInProgress = false;
            }
            _nativeWorldAvailable = false;
            _nativeWorldTemporalResetBoundary = false;
            if (worldRecentlySeen)
                _nativeWorldNonRecentHandoffPolls = 0;
            else
            {
                if (++_nativeWorldNonRecentHandoffPolls >
                    NativeWorldHandoffFlushPolls)
                {
                    gpu.DiscardLiveWorldOutputs();
                    _nativeInitialPrebufferInProgress = false;
                }
            }
            int handoffOutputAge =
                InputManager.CurrentPoll - _nativeWorldInputPoll;
            TraceNativeDecision(
                worldRecentlySeen ? "ownership-gap" : "handoff",
                worldExpected,
                worldStarted,
                realTimeThrottleStarted,
                _nativeWorldPrebuffering,
                gpu.LiveWorldOutputCount,
                handoffOutputAge,
                false,
                false);
            RecordNativePerformance(
                false,
                false,
                false,
                false,
                handoffOutputAge,
                worldExpected);
            return false;
        }
        int nonRecentHandoffPolls = _nativeWorldNonRecentHandoffPolls;
        bool hadFlushedHandoff =
            nonRecentHandoffPolls > NativeWorldHandoffFlushPolls;
        _nativeWorldNonRecentHandoffPolls = 0;
        if (worldStarted && hadFlushedHandoff)
        {
            gpu.DiscardLiveWorldOutputsBefore(
                SelectNativeWorldStaleDiscardBeforePoll(
                    InputManager.CurrentPoll));
        }
        bool initialPrebufferStart = _nativeInitialPrebufferPending;
        if (ShouldStartNativeWorldPrebuffer(
            worldStarted,
            realTimeThrottleStarted,
            worldRecentlySeen,
            nonRecentHandoffPolls))
        {
            // GT2 can withdraw world ownership for one vblank immediately
            // after release pacing begins. Returning from that bounded gap is
            // not a new prebuffer: retain the original target and its
            // in-progress marker until the full reserve is actually ready.
            if (!_nativeWorldPrebuffering || initialPrebufferStart)
            {
                _nativeWorldPrebuffering = true;
                _nativeInitialPrebufferInProgress =
                    initialPrebufferStart;
                _nativeWorldPrebufferTarget =
                    SelectNativeWorldPrebufferTarget(
                        initialPrebufferStart,
                        worldStarted,
                        worldRecentlySeen,
                        gpu.LiveWorldOutputCount);
            }
            _nativeInitialPrebufferPending = false;
        }
        bool receivedNewFrame = false;
        bool receivedSyntheticFrame = false;
        bool receivedRepeatedFrame = false;
        static void TraceRejectedOutput(
            in Hle.LiveWorldOutput rejected,
            int needed)
        {
            if (!_tracePerformance || _nativeRejectedTraceCount >= 3)
                return;
            _nativeRejectedTraceCount++;
            Console.Error.WriteLine(
                $"[Native-Present] rejected output " +
                $"size={rejected.Width}x{rejected.Height} " +
                $"needed={needed} capacity={rejected.Pixels.Length} " +
                $"reserved={rejected.Stats.Reserved} " +
                $"frame={rejected.Frame} poll={rejected.InputPoll}");
        }
        // Prime eight presentations behind the presentation cursor. Native
        // then continues to publish two chronological, unique images per
        // authored state while the host consumes two per NTSC interval. The
        // 100 ms reserve absorbs a dense-pair/scheduler or host-event tail
        // without
        // blocking an ordinary vblank, repeating a 30 Hz image, or increasing
        // the renderer's output-wait cap. Test-only fast-forward deliberately
        // re-primes when real-time pacing begins; normal play primes only at a
        // new 3D segment.
        if (
            _nativeWorldPrebuffering &&
            gpu.LiveWorldOutputCount < _nativeWorldPrebufferTarget
        ) {
            if (worldStarted)
                _nativeWorldAvailable = false;
            int prebufferOutputAge =
                InputManager.CurrentPoll - _nativeWorldInputPoll;
            TraceNativeDecision(
                "prebuffer",
                worldExpected,
                worldStarted,
                realTimeThrottleStarted,
                _nativeWorldPrebuffering,
                gpu.LiveWorldOutputCount,
                prebufferOutputAge,
                receivedNewFrame,
                false);
            RecordNativePerformance(
                false,
                false,
                false,
                false,
                prebufferOutputAge,
                worldExpected,
                transitionHold: true);
            return false;
        }
        _nativeWorldPrebuffering = false;
        _nativeInitialPrebufferInProgress = false;

        int preTakeOutputAge =
            InputManager.CurrentPoll - _nativeWorldInputPoll;
        bool preTakeRecentWorld =
            _nativeWorldAvailable &&
            IsNativeWorldOutputAgeEligible(preTakeOutputAge);
        int outputWaitMilliseconds =
            SelectNativeWorldOutputWaitMilliseconds(
                worldExpected,
                realTimeThrottleActive);

        // GT2 toggles the legacy PS1 display flag while it prepares the next
        // 30 Hz authored framebuffer. That flag must not blank the independent
        // native-world stream on the intervening 60 Hz vblank. The recorder's
        // LiveWorldExpected state and the bounded output age below are the
        // authoritative transition guards for the modern renderer.
        if (gpu.TryTakeLiveWorldOutput(
            out var output,
            outputWaitMilliseconds))
        {
            long afterTake = _tracePerformance
                ? Stopwatch.GetTimestamp()
                : 0;
            try
            {
                int needed = checked(
                    output.Width * output.Height * 4);
                if (
                    output.Width > 0 &&
                    output.Height > 0 &&
                    needed <= output.Pixels.Length
                )
                {
                    if (gl != null)
                    {
                        gl.BindTexture(
                            TextureTarget.Texture2D,
                            _nativeWorldTex);
                        if (
                            output.Width != _nativeWorldAllocatedWidth ||
                            output.Height != _nativeWorldAllocatedHeight
                        )
                        {
                            gl.TexImage2D<byte>(
                                TextureTarget.Texture2D,
                                0,
                                InternalFormat.Rgba8,
                                (uint)output.Width,
                                (uint)output.Height,
                                0,
                                PixelFormat.Rgba,
                                PixelType.UnsignedByte,
                                output.Pixels.AsSpan(0, needed));
                            _nativeWorldAllocatedWidth = output.Width;
                            _nativeWorldAllocatedHeight = output.Height;
                        }
                        else
                        {
                            gl.TexSubImage2D<byte>(
                                TextureTarget.Texture2D,
                                0,
                                0,
                                0,
                                (uint)output.Width,
                                (uint)output.Height,
                                PixelFormat.Rgba,
                                PixelType.UnsignedByte,
                                output.Pixels.AsSpan(0, needed));
                        }
                    }
                    _nativeWorldWidth = output.Width;
                    _nativeWorldHeight = output.Height;
                    _nativeWorldFrame = output.Frame;
                    _nativeWorldInputPoll = output.InputPoll;
                    _nativeWorldAvailable = true;
                    receivedNewFrame = true;
                    receivedSyntheticFrame =
                        (output.Stats.Reserved & 1u) != 0;
                    receivedRepeatedFrame =
                        (output.Stats.Reserved & 2u) != 0;
                    _nativeWorldSynthetic = receivedSyntheticFrame;
                    _nativeWorldRepeated = receivedRepeatedFrame;
                    _nativeWorldTemporalResetBoundary =
                        (output.Stats.Reserved & 4u) != 0;
                }
                else
                    TraceRejectedOutput(in output, needed);
            }
            finally
            {
                gpu.ReturnLiveWorldOutput(output.Pixels);
            }
            if (_tracePerformance)
            {
                long afterUpload = Stopwatch.GetTimestamp();
                double scale = 1000.0 / Stopwatch.Frequency;
                if ((afterUpload - traceStart) * scale >= 40.0)
                {
                    Console.Error.WriteLine(
                        $"[Host-Long-Native-Upload] " +
                        $"poll={InputManager.CurrentPoll} " +
                        $"takeMs={(afterTake - traceStart) * scale:F3} " +
                        $"uploadMs={(afterUpload - afterTake) * scale:F3} " +
                        $"totalMs={(afterUpload - traceStart) * scale:F3}");
                }
            }
        }
        // GT2 authors its 3D scene at 30 Hz while the host presents at 60 Hz.
        // Reuse the latest native frame for the intervening vblank. Do not
        // cross-fade whole frames: moving cars then appear twice as translucent
        // silhouettes. True 60 Hz motion requires geometry-aware interpolation.
        // The short age bound also prevents a delayed result from lingering
        // after a transition back to a menu or video.
        int outputAge =
            InputManager.CurrentPoll - _nativeWorldInputPoll;
        bool recentWorld =
            _nativeWorldAvailable &&
            IsNativeWorldOutputAgeEligible(outputAge);
        bool temporalResetBoundaryHold =
            ShouldHoldTemporalResetBoundary(
                worldExpected,
                receivedNewFrame,
                recentWorld,
                _nativeWorldTemporalResetBoundary);
        bool holdForPendingNativeOutput =
            ShouldHoldForPendingNativeWorldOutput(
                worldExpected,
                receivedNewFrame,
                recentWorld,
                gpu.LiveWorldWorkPending,
                outputAge);
        // LiveWorldExpected already tolerates GT2's single world-free vblank
        // between 30 Hz authored states, so midpoint output remains eligible.
        // Once two world-free presentations hand ownership to the authored 2D
        // compositor, even a late queued native image must be rejected; a
        // validated image from the prior segment is still the wrong scene.
        if (!ShouldPresentNativeWorld(
                worldExpected,
                receivedNewFrame,
                recentWorld,
                _nativeWorldTemporalResetBoundary))
        {
            _nativeWorldAvailable = false;
            _nativeWorldTemporalResetBoundary = false;
            // Never carry a texture across a discontinuous world segment.
            // In particular, the first Results-car capture arrives after an
            // authored 2D title sequence; showing the last race/replay image
            // for its one-vblank native warmup creates a visible stale flash.
            // Leave the last authored compositor presentation in the output
            // panel until the first validated image for the new segment lands.
            TraceNativeDecision(
                "reject",
                worldExpected,
                worldStarted,
                realTimeThrottleStarted,
                _nativeWorldPrebuffering,
                gpu.LiveWorldOutputCount,
                outputAge,
                receivedNewFrame,
                recentWorld);
            RecordNativePerformance(
                false,
                false,
                false,
                false,
                outputAge,
                worldExpected,
                transitionHold:
                    worldStarted ||
                    holdForPendingNativeOutput ||
                    temporalResetBoundaryHold);
            return false;
        }
        if (gl != null)
        {
            PresentTexture(
                gl,
                _nativeWorldTex,
                _nativeWorldWidth,
                _nativeWorldHeight,
                4f / 3f);
        }
        if (_tracePerformance)
        {
            long afterPresentTexture = Stopwatch.GetTimestamp();
            double scale = 1000.0 / Stopwatch.Frequency;
            if ((afterPresentTexture - traceStart) * scale >= 40.0)
            {
                Console.Error.WriteLine(
                    $"[Host-Long-Native-Present] " +
                    $"poll={InputManager.CurrentPoll} " +
                    $"totalMs={(afterPresentTexture - traceStart) * scale:F3}");
            }
        }
        RecordNativePerformance(
            true,
            receivedNewFrame,
            receivedSyntheticFrame,
            receivedRepeatedFrame,
            outputAge,
            worldExpected);
        return true;
    }

    static void TraceNativeDecision(
        string reason,
        bool worldExpected,
        bool worldStarted,
        bool realTimeThrottleStarted,
        bool prebuffering,
        int queuedOutputs,
        int outputAge,
        bool receivedNewFrame,
        bool recentWorld)
    {
        if (
            !_tracePerformance ||
            !FrameClock.RealTimeThrottleActive ||
            _nativeDecisionTraceCount >= 128)
            return;
        _nativeDecisionTraceCount++;
        Console.Error.WriteLine(
            $"[Native-Present-Decision] reason={reason} " +
            $"poll={InputManager.CurrentPoll} " +
            $"worldExpected={worldExpected} " +
            $"worldStarted={worldStarted} " +
            $"throttleStarted={realTimeThrottleStarted} " +
            $"prebuffering={prebuffering} " +
            $"queued={queuedOutputs} " +
            $"available={_nativeWorldAvailable} " +
            $"outputAge={outputAge} " +
            $"receivedNew={receivedNewFrame} " +
            $"recentWorld={recentWorld}");
    }

    internal static bool ShouldPresentNativeWorld(
        bool worldExpected,
        bool receivedNewFrame,
        bool recentWorld,
        bool temporalResetBoundary) =>
        worldExpected &&
        (receivedNewFrame || (recentWorld && !temporalResetBoundary));

    internal static bool ShouldHoldTemporalResetBoundary(
        bool worldExpected,
        bool receivedNewFrame,
        bool recentWorld,
        bool temporalResetBoundary) =>
        worldExpected &&
        !receivedNewFrame &&
        recentWorld &&
        temporalResetBoundary;

    internal static bool IsNativeWorldOutputAgeEligible(int outputAge) =>
        outputAge >= 0 && outputAge <= NativeWorldMaxOutputAgePolls;

    internal static int SelectNativeWorldOutputWaitMilliseconds(
        bool worldExpected,
        bool realTimeThrottleActive) =>
        worldExpected && realTimeThrottleActive
            ? Hle.LiveWorldRenderer.OutputReadyWaitMilliseconds
            : 0;

    internal static int SelectNativeWorldStaleDiscardBeforePoll(
        int currentPoll) =>
        currentPoll - NativeWorldMaxOutputAgePolls;

    internal static bool ShouldStartNativeWorldPrebuffer(
        bool worldStarted,
        bool realTimeThrottleStarted,
        bool worldRecentlySeen,
        int nonRecentHandoffPolls)
    {
        if (realTimeThrottleStarted)
            return true;
        if (!worldStarted)
            return false;
        if (worldRecentlySeen &&
            nonRecentHandoffPolls <= NativeWorldHandoffFlushPolls)
            return false;
        return true;
    }

    internal static bool ShouldPreserveInitialNativeWorldPrebuffer(
        bool initialPrebufferInProgress,
        int nonRecentHandoffPolls) =>
        initialPrebufferInProgress &&
        nonRecentHandoffPolls <= NativeWorldHandoffFlushPolls;

    internal static bool ShouldHoldForPendingNativeWorldOutput(
        bool worldExpected,
        bool receivedNewFrame,
        bool recentWorld,
        bool nativeWorkPending,
        int outputAge) =>
        worldExpected &&
        !receivedNewFrame &&
        !recentWorld &&
        nativeWorkPending &&
        outputAge > NativeWorldMaxOutputAgePolls &&
        outputAge <= NativeWorldMaxOutputAgePolls + NativeWorldHandoffFlushPolls;

    internal static int SelectNativeWorldPrebufferTarget(
        bool initialPrebufferPending,
        bool worldStarted,
        bool worldRecentlySeen,
        int queuedOutputs)
    {
        if (initialPrebufferPending)
            return NativeWorldInitialPrebufferOutputs;
        if (!worldStarted || !worldRecentlySeen)
            return NativeWorldPrebufferOutputs;
        return Math.Min(
            NativeWorldPrebufferOutputs,
            Math.Max(6, queuedOutputs));
    }

    static void RecordNativePerformance(
        bool nativePresented,
        bool receivedNewFrame,
        bool receivedSyntheticFrame,
        bool receivedRepeatedFrame,
        int outputAge,
        bool worldExpected,
        bool transitionHold = false)
    {
        if (!_tracePerformance)
            return;
        if (!FrameClock.RealTimeThrottleActive)
        {
            ResetNativePerformanceCounters();
            return;
        }
        long now = Stopwatch.GetTimestamp();
        if (_nativePerfTimestamp == 0)
            _nativePerfTimestamp = now;
        _nativePerfPresents++;
        if (nativePresented)
        {
            if (receivedNewFrame)
            {
                if (receivedRepeatedFrame)
                    _nativePerfRepeatedFrames++;
                else
                {
                    _nativePerfNewFrames++;
                    if (receivedSyntheticFrame)
                        _nativePerfSyntheticFrames++;
                    else
                        _nativePerfActualFrames++;
                }
            }
            else
                _nativePerfRepeatedFrames++;
            int age = Math.Max(0, outputAge);
            _nativePerfAgeTotal += age;
            _nativePerfAgeMaximum = Math.Max(_nativePerfAgeMaximum, age);
        }
        else
        {
            if (transitionHold)
                _nativePerfTransitionHolds++;
            else if (worldExpected)
                _nativePerfWorldMissFrames++;
            else
                _nativePerfCompositorFrames++;
        }
        if (_nativePerfPresents < 300)
            return;

        double seconds = (now - _nativePerfTimestamp) /
            (double)Stopwatch.Frequency;
        int nativePresents =
            _nativePerfNewFrames + _nativePerfRepeatedFrames;
        Console.Error.WriteLine(
            $"[Native-Present] hostHz={_nativePerfPresents / seconds:F2} " +
            $"uniqueHz={_nativePerfNewFrames / seconds:F2} " +
            $"new={_nativePerfNewFrames} " +
            $"actual={_nativePerfActualFrames} " +
            $"synthetic={_nativePerfSyntheticFrames} " +
            $"repeated={_nativePerfRepeatedFrames} " +
            $"compositor={_nativePerfCompositorFrames} " +
            $"worldMiss={_nativePerfWorldMissFrames} " +
            $"transitionHold={_nativePerfTransitionHolds} " +
            $"ageAvg={_nativePerfAgeTotal / (double)Math.Max(1, nativePresents):F2} " +
            $"ageMax={_nativePerfAgeMaximum}");
        ResetNativePerformanceCounters(now);
    }

    static void ResetNativePerformanceCounters(long timestamp = 0)
    {
        _nativePerfTimestamp = timestamp;
        _nativePerfPresents = 0;
        _nativePerfNewFrames = 0;
        _nativePerfActualFrames = 0;
        _nativePerfSyntheticFrames = 0;
        _nativePerfRepeatedFrames = 0;
        _nativePerfCompositorFrames = 0;
        _nativePerfTransitionHolds = 0;
        _nativePerfWorldMissFrames = 0;
        _nativePerfAgeTotal = 0;
        _nativePerfAgeMaximum = 0;
    }

    static void PresentTexture(GL gl, uint sourceTexture, int sourceWidth, int sourceHeight, float aspect)
    {
        var framebuffer = _window!.FramebufferSize;
        var output = OutputPanel.GetPresentationSize(aspect, framebuffer.X, framebuffer.Y);
        if (!string.IsNullOrWhiteSpace(_presentationResolutionOverride))
        {
            var forced = ParseOutputResolution(_presentationResolutionOverride);
            output = (forced.width, forced.height);
        }
        bool fxaa = (_antiAliasingOverride ?? ConfigManager.View.AntiAliasing)
            .Equals("FXAA", StringComparison.OrdinalIgnoreCase);
        uint texture = sourceTexture;
        if (_presentationRenderer is { Ready: true })
        {
            string? capture = _pendingPresentationCapture;
            _pendingPresentationCapture = null;
            ++_presentationFrame;
            bool captureSourceFrame =
                _presentationCaptureSourceFrames.Contains(_nativeWorldFrame);
            if (_capturePresentation && captureSourceFrame)
                capture =
                    $"source_{_nativeWorldFrame:000000}_" +
                    (_nativeWorldSynthetic ? "synthetic" : "actual");
            else if (_capturePresentation &&
                (_presentationFrame == _presentationCaptureFrame ||
                 _presentationCaptureFrames.Contains(_presentationFrame)))
                capture = $"frame_{_presentationFrame:000000}";
            texture = _presentationRenderer.Render(sourceTexture, sourceWidth, sourceHeight,
                output.w, output.h, fxaa, capture);
            if (!string.IsNullOrEmpty(capture) && sourceTexture == _nativeWorldTex)
                Console.Error.WriteLine(
                    $"[Host] native presentation capture={capture} " +
                    $"sourceFrame={_nativeWorldFrame} " +
                    $"poll={_nativeWorldInputPoll} " +
                    $"synthetic={_nativeWorldSynthetic} " +
                    $"repeated={_nativeWorldRepeated}");
            if (!string.IsNullOrEmpty(capture) && _exitAfterPresentationCapture)
                Runtime.RequestShutdown();
        }
        OutputPanel.SetTexture(texture, output.w, output.h, aspect);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        gl.Viewport(0, 0, (uint)framebuffer.X, (uint)framebuffer.Y);
    }

    static void ProbeDisplay(Gpu gpu, int w, int h)
    {
        string? captureLabel = _requestedDisplayCapture;
        _requestedDisplayCapture = null;
        bool periodicProbe = _displayProbeInterval > 0 &&
            ++_displayProbeFrame % _displayProbeInterval == 1;
        if (!periodicProbe && string.IsNullOrEmpty(captureLabel)) return;

        RecordDisplayProbe(w, h, captureLabel);
    }

    static void ProbeHleDisplay(Hle.IGpuBackend backend, Gpu gpu, int w, int h)
    {
        string? captureLabel = _requestedDisplayCapture;
        _requestedDisplayCapture = null;
        bool periodicProbe = _displayProbeInterval > 0 &&
            ++_displayProbeFrame % _displayProbeInterval == 1;
        if (!periodicProbe && string.IsNullOrEmpty(captureLabel)) return;

        int pixels = w * h;
        if (_hleDisplay.Length < pixels) _hleDisplay = new ushort[pixels];
        if (_rgbDisplay.Length < pixels * 3) _rgbDisplay = new byte[pixels * 3];
        backend.ReadVram(gpu.DisplayX, gpu.DisplayY, w, h, _hleDisplay.AsSpan(0, pixels));
        for (int i = 0, o = 0; i < pixels; i++)
        {
            ushort px = _hleDisplay[i];
            _rgbDisplay[o++] = (byte)((px & 0x1F) << 3);
            _rgbDisplay[o++] = (byte)(((px >> 5) & 0x1F) << 3);
            _rgbDisplay[o++] = (byte)(((px >> 10) & 0x1F) << 3);
        }

        RecordDisplayProbe(w, h, captureLabel);
    }

    static void RecordDisplayProbe(int w, int h, string? captureLabel)
    {

        int pixels = w * h;
        int nonzero = 0;
        uint hash = 2166136261u;
        for (int i = 0; i < pixels; i++)
        {
            int o = i * 3;
            uint rgb = (uint)(_rgbDisplay[o] | (_rgbDisplay[o + 1] << 8) | (_rgbDisplay[o + 2] << 16));
            if (rgb != 0) nonzero++;
            hash = (hash ^ rgb) * 16777619u;
        }

        if (hash != _lastDisplayHash)
        {
            _lastDisplayHash = hash;
            Console.WriteLine($"[GPU] framebuffer {w}x{h} nonzero={nonzero} hash=0x{hash:X8}");
            WriteDisplayPpm("recompone_vram_latest.ppm", w, h, pixels);
        }

        if (!string.IsNullOrEmpty(captureLabel))
        {
            string path = $"recompone_capture_{captureLabel}.ppm";
            WriteDisplayPpm(path, w, h, pixels);
            Console.WriteLine($"[GPU] captured stage '{captureLabel}' to {path}");
            if (_capturePresentation)
                _pendingPresentationCapture = captureLabel;
        }
    }

    static void WriteDisplayPpm(string path, int w, int h, int pixels)
    {
        using var dump = File.Create(path);
        byte[] header = System.Text.Encoding.ASCII.GetBytes($"P6\n{w} {h}\n255\n");
        dump.Write(header);
        dump.Write(_rgbDisplay, 0, pixels * 3);
    }

    static ushort[] _vramView = new ushort[Gpu.VramWidth * Gpu.VramHeight];
    static void UploadVramTexture(GL gl, Gpu gpu)
    {
        const int sz = Gpu.VramWidth * Gpu.VramHeight * 3;
        if (_rgbVram.Length < sz) _rgbVram = new byte[sz];
        ushort[] src;
        if (Hle.GpuHle.Active && _glBackend is { Ready: true })
        {
            _glBackend.ReadVram(0, 0, Gpu.VramWidth, Gpu.VramHeight, _vramView);
            src = _vramView;
        }
        else src = gpu.Vram;
        ConvertVramToBuffer(src, _rgbVram);
        gl.BindTexture(TextureTarget.Texture2D, _vramTex);
        gl.TexImage2D<byte>(TextureTarget.Texture2D, 0, InternalFormat.Rgb, Gpu.VramWidth, Gpu.VramHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, _rgbVram.AsSpan(0, sz));
        VramViewerPanel.SetTexture(_vramTex, Gpu.VramWidth, Gpu.VramHeight);
    }

    static void QueueRamConvert()
    {
        if (_ramTask is { IsCompleted: false }) return;
        if (++_ramFrame < 6) return;
        _ramFrame = 0;
        var psMem = Runtime.Mem as Memory.PSMemory;
        if (psMem == null) return;
        var ram = psMem.RamBuffer;
        var back = _ramBack;
        _ramTask = Task.Run(() => Runtime.RamLog.BuildTexture(ram, back))
            .ContinueWith(_ =>
            {
                (_ramFront, _ramBack) = (_ramBack, _ramFront);
                _ramReady = true;
            }, TaskContinuationOptions.ExecuteSynchronously);
    }

    static void FlushRamTexture(GL gl)
    {
        _ramReady = false;
        gl.BindTexture(TextureTarget.Texture2D, _ramTex);
        gl.TexImage2D<byte>(TextureTarget.Texture2D, 0, InternalFormat.Rgba,
            Memory.RamLogger.Width, Memory.RamLogger.Height, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, _ramFront);
        RamMapPanel.SetTexture(_ramTex);
    }

    static void ConvertDisplay(Gpu gpu, int w, int h)
    {
        var vram = gpu.Vram;
        int dx = gpu.DisplayX, dy = gpu.DisplayY;
        int o = 0;
        if (gpu.Display24Bit)
        {
            for (int y = 0; y < h; y++)
            {
                int lineByte = ((dy + y) * Gpu.VramWidth + dx) * 2;
                for (int x = 0; x < w; x++)
                {
                    int bo = lineByte + x * 3;
                    _rgbDisplay[o++] = VramByte(vram, bo);
                    _rgbDisplay[o++] = VramByte(vram, bo + 1);
                    _rgbDisplay[o++] = VramByte(vram, bo + 2);
                }
            }
        }
        else
        {
            for (int y = 0; y < h; y++)
            {
                int line = ((dy + y) & (Gpu.VramHeight - 1)) * Gpu.VramWidth;
                for (int x = 0; x < w; x++)
                {
                    ushort px = vram[line + ((dx + x) & (Gpu.VramWidth - 1))];
                    _rgbDisplay[o++] = (byte)((px & 0x1F) << 3);
                    _rgbDisplay[o++] = (byte)(((px >> 5) & 0x1F) << 3);
                    _rgbDisplay[o++] = (byte)(((px >> 10) & 0x1F) << 3);
                }
            }
        }
    }

    static void ConvertVramToBuffer(ushort[] vram, byte[] output)
    {
        int o = 0;
        for (int y = 0; y < Gpu.VramHeight; y++)
        for (int x = 0; x < Gpu.VramWidth; x++)
        {
            ushort px = vram[y * Gpu.VramWidth + x];
            output[o++] = (byte)((px & 0x1F) << 3);
            output[o++] = (byte)(((px >> 5) & 0x1F) << 3);
            output[o++] = (byte)(((px >> 10) & 0x1F) << 3);
        }
    }

    static byte VramByte(ushort[] vram, int byteOffset)
    {
        int hw = (byteOffset >> 1) & (Gpu.VramWidth * Gpu.VramHeight - 1);
        ushort v = vram[hw];
        return (byte)((byteOffset & 1) == 0 ? v & 0xFF : v >> 8);
    }
}
