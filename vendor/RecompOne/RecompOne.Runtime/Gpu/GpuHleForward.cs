using RecompOne.Runtime.Hle;
using RecompOne.Runtime.Config;

namespace RecompOne.Runtime;

public sealed partial class Gpu
{
    readonly ProjectedSceneCapture _projectedCapture = new();
    readonly WorldSceneCapture _worldCapture = new();
    readonly LiveWorldRenderer _liveWorldRenderer = new();
    readonly LiveWorldFrameRecorder _liveWorldCapture;
    long _projectedCaptureFrame;
    bool _liveWorldExpected;
    int _liveWorldLastSeenPoll = int.MinValue;
    int _liveWorldFreePresentationCount;
    bool _liveVramReported;
    bool _liveVramInitialized;
    long _mirroredUploadWords;
    long _mirroredFillWords;
    long _mirroredCopyWords;
    readonly bool _traceScreenEffectPrimitives =
        string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_SCREEN_EFFECT_PRIMITIVES"),
            "1",
            StringComparison.Ordinal);
    readonly int _traceScreenEffectStartPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_SCREEN_EFFECT_START_POLL"),
            out int traceScreenEffectStartPoll)
            ? traceScreenEffectStartPoll
            : 0;
    readonly int _traceScreenEffectEndPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_SCREEN_EFFECT_END_POLL"),
            out int traceScreenEffectEndPoll)
            ? traceScreenEffectEndPoll
            : int.MaxValue;
    readonly int _traceScreenEffectMinX =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_SCREEN_EFFECT_MIN_X"),
            out int traceScreenEffectMinX)
            ? traceScreenEffectMinX
            : int.MinValue;
    readonly int _traceScreenEffectMaxX =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_SCREEN_EFFECT_MAX_X"),
            out int traceScreenEffectMaxX)
            ? traceScreenEffectMaxX
            : int.MaxValue;
    readonly int _traceScreenEffectMinY =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_SCREEN_EFFECT_MIN_Y"),
            out int traceScreenEffectMinY)
            ? traceScreenEffectMinY
            : int.MinValue;
    readonly int _traceScreenEffectMaxY =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_SCREEN_EFFECT_MAX_Y"),
            out int traceScreenEffectMaxY)
            ? traceScreenEffectMaxY
            : int.MaxValue;
    readonly int _traceScreenEffectMinWidth =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_SCREEN_EFFECT_MIN_WIDTH"),
            out int traceScreenEffectMinWidth)
            ? Math.Max(1, traceScreenEffectMinWidth)
            : 1;
    readonly int _traceScreenEffectMinHeight =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_SCREEN_EFFECT_MIN_HEIGHT"),
            out int traceScreenEffectMinHeight)
            ? Math.Max(1, traceScreenEffectMinHeight)
            : 1;
    readonly int _traceScreenEffectMaxWidth =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_SCREEN_EFFECT_MAX_WIDTH"),
            out int traceScreenEffectMaxWidth)
            ? Math.Max(1, traceScreenEffectMaxWidth)
            : 24;
    readonly int _traceScreenEffectMaxHeight =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_SCREEN_EFFECT_MAX_HEIGHT"),
            out int traceScreenEffectMaxHeight)
            ? Math.Max(1, traceScreenEffectMaxHeight)
            : 24;
    readonly string _traceScreenEffectWorldFilter =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_SCREEN_EFFECT_WORLD_FILTER") ?? string.Empty;
    readonly string _traceScreenEffectTextureFilter =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_SCREEN_EFFECT_TEXTURE_FILTER") ?? string.Empty;
    readonly int _traceScreenEffectTPage =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_SCREEN_EFFECT_TPAGE"),
            out int traceScreenEffectTPage)
            ? traceScreenEffectTPage
            : -1;
    readonly int _traceScreenEffectClut =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_SCREEN_EFFECT_CLUT"),
            out int traceScreenEffectClut)
            ? traceScreenEffectClut
            : -1;
    readonly bool _traceScreenEffectPositions =
        string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_SCREEN_EFFECT_POSITIONS"),
            "1",
            StringComparison.Ordinal);
    readonly bool _traceScreenEffectNewAfterBaseline =
        string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_SCREEN_EFFECT_NEW_AFTER_BASELINE"),
            "1",
            StringComparison.Ordinal);
    readonly HashSet<string> _screenEffectBaselineSignatures = new();
    readonly HashSet<string> _screenEffectNewPacketSignatures = new();
    readonly HashSet<string> _screenEffectPrimitiveSignatures = new();
    readonly bool _traceMixedProjectionTriangles =
        string.Equals(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_MIXED_PROJECTION_TRIANGLES"),
            "1",
            StringComparison.Ordinal);
    readonly int _traceMixedProjectionStartPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_MIXED_PROJECTION_START_POLL"),
            out int traceMixedProjectionStartPoll)
            ? traceMixedProjectionStartPoll
            : 0;
    readonly int _traceMixedProjectionEndPoll =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_MIXED_PROJECTION_END_POLL"),
            out int traceMixedProjectionEndPoll)
            ? traceMixedProjectionEndPoll
            : int.MaxValue;
    readonly int _traceMixedProjectionLimit =
        int.TryParse(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_MIXED_PROJECTION_LIMIT"),
            out int traceMixedProjectionLimit)
            ? Math.Max(1, traceMixedProjectionLimit)
            : 4096;
    int _mixedProjectionTraceCount;

    public Gpu()
    {
        _liveWorldCapture =
            new LiveWorldFrameRecorder(_liveWorldRenderer);
        WorldCaptureContext.LiveRenderingEnabled =
            _liveWorldRenderer.Enabled;
    }

    static bool HleOn => GpuHle.Active && GpuHle.Backend is { Ready: true };
    bool DitherEnabled => _dither && ConfigManager.View.Ps1Dithering;

    int CurTPage() => ((_texPageX / 64) & 0xf) | (((_texPageY / 256) & 1) << 4)
                    | ((_blendMode & 3) << 5) | ((_texDepth & 3) << 7);

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    HleDrawEnv CurEnv() => new()
    {
        ClipX0 = _drawAreaLeft, ClipY0 = _drawAreaTop, ClipX1 = _drawAreaRight, ClipY1 = _drawAreaBottom,
        DrawOffsetX = _drawOffsetX, DrawOffsetY = _drawOffsetY,
        TwMaskX = _texWinMaskX, TwMaskY = _texWinMaskY, TwOffX = _texWinOffX, TwOffY = _texWinOffY,
        SetMask = _setMask, CheckMask = _checkMask, Dither = DitherEnabled,
    };

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    static HleVertex HV(in Vert v) => new()
    {
        X = v.X, Y = v.Y, R = (byte)v.R, G = (byte)v.G, B = (byte)v.B, U = (short)v.U, V = (short)v.V,
        Z = v.Z, HasGteZ = v.HasGteZ,
    };

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    PrimFlags PrimOf(bool tex, bool semi, bool raw, int clut, bool gouraud = false) => new()
    {
        Textured = tex, SemiTrans = semi, RawTexture = raw, Gouraud = gouraud, TPage = (ushort)CurTPage(), Clut = (ushort)clut,
    };

    void HleTri(in Vert a, in Vert b, in Vert c, bool tex, bool gouraud, bool semi, bool raw, int clut)
    {
        int spanX = Math.Max(a.X, Math.Max(b.X, c.X)) - Math.Min(a.X, Math.Min(b.X, c.X));
        int spanY = Math.Max(a.Y, Math.Max(b.Y, c.Y)) - Math.Min(a.Y, Math.Min(b.Y, c.Y));
        if (spanX > 1023 || spanY > 511) return;

        var be = GpuHle.Backend!;
        be.SetDrawEnv(CurEnv());
        be.DrawTri(HV(a), HV(b), HV(c), PrimOf(tex, semi, raw, clut, gouraud));
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    bool CaptureTri(
        in Vert a,
        in Vert b,
        in Vert c,
        in GteProjectionOrigin originA,
        in GteProjectionOrigin originB,
        in GteProjectionOrigin originC,
        bool tex,
        bool gouraud,
        bool semi,
        bool raw,
        int clut)
    {
        var flags = PrimOf(tex, semi, raw, clut, gouraud);
        var ha = HV(a);
        var hb = HV(b);
        var hc = HV(c);
        if (_projectedCapture.Enabled)
            CaptureHleTri(in ha, in hb, in hc, in flags);
        TraceMixedProjectionTriangle(
            in a,
            in b,
            in c,
            in originA,
            in originB,
            in originC,
            in flags);
        bool fileWorldCapture = _worldCapture.Enabled;
        bool liveWorldCapture = _liveWorldCapture.Enabled;
        bool containsWorldProvenance =
            originA.Valid || originB.Valid || originC.Valid;
        if (_traceScreenEffectPrimitives &&
            (!string.Equals(
                 _traceScreenEffectTextureFilter,
                 "untextured",
                 StringComparison.OrdinalIgnoreCase) ||
             !flags.Textured) &&
            (!string.Equals(
                 _traceScreenEffectTextureFilter,
                 "textured",
                 StringComparison.OrdinalIgnoreCase) ||
             flags.Textured))
        {
            TraceScreenEffectPrimitive(
                in ha,
                in hb,
                in hc,
                in flags,
                in originA,
                in originB,
                in originC,
                a.SourceAddress,
                b.SourceAddress,
                c.SourceAddress);
        }
        if (fileWorldCapture || liveWorldCapture)
        {
            var environment = CurEnv();
            if (fileWorldCapture)
            {
                _worldCapture.RecordTriangle(
                    _projectedCaptureFrame + 1,
                    Host.InputManager.CurrentPoll,
                    in environment,
                    in ha,
                    in hb,
                    in hc,
                    in originA,
                    in originB,
                    in originC,
                    in flags);
            }
            if (liveWorldCapture)
            {
                if (originA.Valid || originB.Valid || originC.Valid)
                {
                    _liveWorldCapture.RecordTriangle(
                        _projectedCaptureFrame + 1,
                        in environment,
                        in ha,
                        in hb,
                        in hc,
                        in originA,
                        in originB,
                        in originC,
                        in flags);
                }
                else
                {
                    // HUD needles and redline wedges are ordinary polygon
                    // packets, not only GPU line commands. Preserve triangles
                    // with no GTE provenance as explicit screen primitives.
                    _liveWorldCapture.RecordScreenTriangle(
                        _projectedCaptureFrame + 1,
                        in environment,
                        in ha,
                        in hb,
                        in hc,
                        in flags);
                }
            }
        }
        // On the Windows shipping path, provenance-backed 3D is native-only.
        // Return the classification even when the native worker has failed or
        // is stopping so neither the GL-HLE nor software compatibility
        // rasterizer can silently reappear as a world-renderer fallback.
        return containsWorldProvenance;
    }

    void TraceMixedProjectionTriangle(
        in Vert a,
        in Vert b,
        in Vert c,
        in GteProjectionOrigin originA,
        in GteProjectionOrigin originB,
        in GteProjectionOrigin originC,
        in PrimFlags flags)
    {
        if (!_traceMixedProjectionTriangles)
            return;
        int poll = Host.InputManager.CurrentPoll;
        if (poll < _traceMixedProjectionStartPoll ||
            poll > _traceMixedProjectionEndPoll)
            return;
        if (!originA.Valid || !originB.Valid || !originC.Valid)
            return;
        bool coherent =
            originA.TransformId == originB.TransformId &&
            originA.TransformId == originC.TransformId &&
            originA.ProjectionPlane == originB.ProjectionPlane &&
            originA.ProjectionPlane == originC.ProjectionPlane &&
            originA.ProjectionOffsetX == originB.ProjectionOffsetX &&
            originA.ProjectionOffsetX == originC.ProjectionOffsetX &&
            originA.ProjectionOffsetY == originB.ProjectionOffsetY &&
            originA.ProjectionOffsetY == originC.ProjectionOffsetY;
        if (coherent)
            return;
        int trace = Interlocked.Increment(ref _mixedProjectionTraceCount);
        if (trace > _traceMixedProjectionLimit)
            return;

        static string DescribeVertex(
            char name,
            in Vert vertex,
            in GteProjectionOrigin origin) =>
            $"{name}[packet=0x{vertex.SourceAddress:X8} " +
            $"screen={vertex.X},{vertex.Y} uv={vertex.U},{vertex.V} " +
            $"z={vertex.Z} age={vertex.DepthAge} " +
            $"depth={vertex.DepthProvenance} " +
            $"object={origin.Object.Kind}/0x{origin.Object.StableId:X8}/" +
            $"0x{origin.Object.ModelPointer:X8} " +
            $"model={origin.ModelX},{origin.ModelY},{origin.ModelZ} " +
            $"view={origin.ViewX},{origin.ViewY},{origin.ViewZ} " +
            $"transform=0x{origin.TransformId:X16} " +
            $"projection={origin.ProjectionPlane}/" +
            $"{origin.ProjectionOffsetX},{origin.ProjectionOffsetY} " +
            $"flags={origin.Flags}]";

        Console.Error.WriteLine(
            $"[GTE-MIXED-TRI] n={trace} poll={poll} " +
            $"frame={_projectedCaptureFrame + 1} " +
            $"textured={flags.Textured} tpage=0x{flags.TPage:X4} " +
            $"clut=0x{flags.Clut:X4} " +
            DescribeVertex('A', in a, in originA) + " " +
            DescribeVertex('B', in b, in originB) + " " +
            DescribeVertex('C', in c, in originC));
    }

    void TraceScreenEffectPrimitive(
        in HleVertex a,
        in HleVertex b,
        in HleVertex c,
        in PrimFlags flags,
        in GteProjectionOrigin originA,
        in GteProjectionOrigin originB,
        in GteProjectionOrigin originC,
        uint sourceA,
        uint sourceB,
        uint sourceC)
    {
        if (Host.InputManager.CurrentPoll < _traceScreenEffectStartPoll ||
            Host.InputManager.CurrentPoll > _traceScreenEffectEndPoll)
            return;
        if ((_traceScreenEffectTPage >= 0 &&
             flags.TPage != _traceScreenEffectTPage) ||
            (_traceScreenEffectClut >= 0 &&
             flags.Clut != _traceScreenEffectClut))
            return;
        float lowX = Math.Min(a.X, Math.Min(b.X, c.X));
        float highX = Math.Max(a.X, Math.Max(b.X, c.X));
        float lowY = Math.Min(a.Y, Math.Min(b.Y, c.Y));
        float highY = Math.Max(a.Y, Math.Max(b.Y, c.Y));
        float width = highX - lowX;
        float height = highY - lowY;
        // Effects assembled around a projected world point arrive without
        // per-vertex GTE provenance. Restrict this opt-in diagnostic to small
        // textured gameplay primitives, excluding the outer HUD bands.
        if (width < _traceScreenEffectMinWidth ||
            height < _traceScreenEffectMinHeight ||
            width > _traceScreenEffectMaxWidth ||
            height > _traceScreenEffectMaxHeight ||
            highX < 24 || lowX > 296 || highY < 36 || lowY > 218 ||
            highX < _traceScreenEffectMinX ||
            lowX > _traceScreenEffectMaxX ||
            highY < _traceScreenEffectMinY ||
            lowY > _traceScreenEffectMaxY)
        {
            return;
        }
        int lowU = Math.Min(a.U, Math.Min(b.U, c.U));
        int highU = Math.Max(a.U, Math.Max(b.U, c.U));
        int lowV = Math.Min(a.V, Math.Min(b.V, c.V));
        int highV = Math.Max(a.V, Math.Max(b.V, c.V));
        GteProjectionOrigin origin = originA.Valid
            ? originA
            : (originB.Valid ? originB : originC);
        if (string.Equals(
                _traceScreenEffectWorldFilter,
                "unknown",
                StringComparison.OrdinalIgnoreCase) &&
            origin.Valid && origin.Object.Kind != WorldObjectKind.Unknown)
        {
            return;
        }
        if (string.Equals(
                _traceScreenEffectWorldFilter,
                "vehicle",
                StringComparison.OrdinalIgnoreCase) &&
            (!origin.Valid || origin.Object.Kind != WorldObjectKind.Vehicle))
        {
            return;
        }
        string provenance = origin.Valid
            ? $"world={origin.Object.Kind}/0x{origin.Object.StableId:X8}/" +
                $"0x{origin.Object.ModelPointer:X8}"
            : "world=none";
        string packetSignature =
            $"tex={flags.Textured} uv={lowU},{lowV}-{highU},{highV} " +
            $"tpage=0x{flags.TPage:X4} clut=0x{flags.Clut:X4} " +
            $"semi={flags.SemiTrans} raw={flags.RawTexture} " +
            $"gouraud={flags.Gouraud} rgb={a.R},{a.G},{a.B} " +
            provenance;
        if (_traceScreenEffectNewAfterBaseline)
        {
            if (Host.InputManager.CurrentPoll <
                _traceScreenEffectStartPoll + 300)
            {
                _screenEffectBaselineSignatures.Add(packetSignature);
                return;
            }
            if (_screenEffectBaselineSignatures.Contains(packetSignature) ||
                !_screenEffectNewPacketSignatures.Add(packetSignature))
            {
                return;
            }
        }
        string signature =
            $"size={width:F2}x{height:F2} tex={flags.Textured} " +
            $"uv={lowU},{lowV}-" +
            $"{highU},{highV} tpage=0x{flags.TPage:X4} " +
            $"clut=0x{flags.Clut:X4} semi={flags.SemiTrans} " +
            $"raw={flags.RawTexture} gouraud={flags.Gouraud} " +
            $"rgb={a.R},{a.G},{a.B} {provenance}" +
            (_traceScreenEffectPositions
                ? $" xy={lowX:F2},{lowY:F2}-{highX:F2},{highY:F2}"
                : string.Empty);
        int traceLimit = _traceScreenEffectPositions ? 65536 : 4096;
        if (_screenEffectPrimitiveSignatures.Count >= traceLimit ||
            !_screenEffectPrimitiveSignatures.Add(signature))
        {
            return;
        }
        Console.Error.WriteLine(
            $"[Screen-Effect] poll={Host.InputManager.CurrentPoll} " +
            $"xy={lowX:F2},{lowY:F2}-{highX:F2},{highY:F2} " +
            $"src=0x{sourceA:X8},0x{sourceB:X8},0x{sourceC:X8} " +
            signature);
    }

    internal static bool ShouldRasterizeCompatibilityTriangle(
        bool containsWorldProvenance) =>
        !LiveWorldRenderer.Requested || !containsWorldProvenance;

    void CaptureHleTri(
        in HleVertex a,
        in HleVertex b,
        in HleVertex c,
        in PrimFlags flags)
    {
        if (!_projectedCapture.Enabled)
            return;
        var environment = CurEnv();
        _projectedCapture.RecordTriangle(
            _projectedCaptureFrame + 1,
            Host.InputManager.CurrentPoll,
            in environment,
            in a,
            in b,
            in c,
            in flags);
    }

    void CaptureRect(
        int x,
        int y,
        int w,
        int h,
        int u,
        int v,
        int clut,
        int r,
        int g,
        int b,
        bool textured,
        bool semi,
        bool raw)
    {
        var flags = PrimOf(textured, semi, raw, clut);
        var a = new HleVertex
        {
            X = x, Y = y, U = (short)u, V = (short)v,
            R = (byte)r, G = (byte)g, B = (byte)b,
        };
        var topRight = a;
        topRight.X += w;
        topRight.U = (short)(u + w);
        var bottomLeft = a;
        bottomLeft.Y += h;
        bottomLeft.V = (short)(v + h);
        var bottomRight = topRight;
        bottomRight.Y += h;
        bottomRight.V = (short)(v + h);
        CaptureHleTri(in a, in topRight, in bottomLeft, in flags);
        CaptureHleTri(
            in topRight,
            in bottomRight,
            in bottomLeft,
            in flags);
        if (_liveWorldCapture.Enabled)
        {
            var environment = CurEnv();
            _liveWorldCapture.RecordScreenTriangle(
                _projectedCaptureFrame + 1,
                in environment,
                in a,
                in topRight,
                in bottomLeft,
                in flags);
            _liveWorldCapture.RecordScreenTriangle(
                _projectedCaptureFrame + 1,
                in environment,
                in topRight,
                in bottomRight,
                in bottomLeft,
                in flags);
        }
    }

    internal void CapturePresentedFrame()
    {
        _projectedCaptureFrame++;
        // Diagnostic captures require the completed GL framebuffer. Live
        // rendering only needs texture/CLUT VRAM, whose CPU mirror is kept
        // current at upload/fill/copy time. Reading all 1 MiB of GL VRAM on
        // every presented frame serializes the emulation and render threads
        // and is both unnecessary and catastrophically slow.
        bool initializeLiveVram =
            !_liveVramInitialized &&
            _liveWorldCapture.NeedsVramSnapshot;
        if ((_projectedCapture.NeedsVramSnapshot ||
             (WorldCaptureContext.CaptureEnabled &&
              _worldCapture.NeedsVramSnapshot) ||
             initializeLiveVram) && HleOn)
        {
            GpuHle.Backend!.ReadVram(
                0,
                0,
                VramShadow.Width,
                VramShadow.Height,
                Shadow.Pixels);
            if (initializeLiveVram)
            {
                _liveVramInitialized = true;
                Console.Error.WriteLine(
                    "[Native-World] initialized texture VRAM from " +
                    "one-time HLE snapshot");
            }
        }
        var display = new HleDispEnv
        {
            X = DisplayX,
            Y = DisplayY,
            W = DisplayWidth,
            H = DisplayHeight,
            Rgb24 = Display24Bit,
        };
        _projectedCapture.OnPresentedFrame(
            _projectedCaptureFrame,
            Host.InputManager.CurrentPoll,
            in display,
            Shadow.Pixels);
        if (WorldCaptureContext.CaptureEnabled)
        {
            if (_worldCapture.Enabled)
            {
                _worldCapture.OnPresentedFrame(
                    _projectedCaptureFrame,
                    Host.InputManager.CurrentPoll,
                    in display,
                    Shadow.Pixels);
            }
            _liveWorldCapture.OnPresentedFrame(
                _projectedCaptureFrame,
                Host.InputManager.CurrentPoll,
                in display,
                Shadow.Pixels);
            int inputPoll = Host.InputManager.CurrentPoll;
            if (_liveWorldCapture.LastPresentedFrameContainedWorld)
            {
                _liveWorldLastSeenPoll = inputPoll;
                _liveWorldFreePresentationCount = 0;
            }
            else
                _liveWorldFreePresentationCount++;
            long worldAge = (long)inputPoll - _liveWorldLastSeenPoll;
            _liveWorldExpected = worldAge >= 0 && worldAge <= 6;
            if (
                !_liveVramReported &&
                _liveWorldExpected &&
                _liveWorldCapture.Enabled
            )
            {
                _liveVramReported = true;
                int nonzero = 0;
                uint hash = 2166136261u;
                foreach (ushort word in Shadow.Pixels)
                {
                    if (word != 0)
                        nonzero++;
                    hash = (hash ^ word) * 16777619u;
                }
                Console.Error.WriteLine(
                    $"[Native-World] VRAM mirror nonzero={nonzero}/" +
                    $"{Shadow.Pixels.Length} hash=0x{hash:X8} " +
                    $"uploadWords={_mirroredUploadWords} " +
                    $"fillWords={_mirroredFillWords} " +
                    $"copyWords={_mirroredCopyWords}");
            }
        }
    }

    // GPU presentation packets are not scene-boundary evidence. Dense scenery
    // can emit bounded world-free packets while retaining the same 3D segment,
    // so buffered or active native work keeps ownership through those gaps.
    // Once the current presentation is world-free and no native image can
    // still arrive, hand off immediately instead of waiting for an impossible
    // readback and recording a false world miss at Results.
    internal bool LiveWorldExpected =>
        IsLiveWorldExpectedFromHistory(
            _liveWorldExpected,
            _liveWorldFreePresentationCount) &&
        (_liveWorldFreePresentationCount == 0 ||
         _liveWorldRenderer.PublishedOutputCount != 0 ||
         _liveWorldRenderer.HasPendingOrActiveWork);

    internal static bool IsLiveWorldExpectedFromHistory(
        bool recentlyContainedWorld,
        int worldFreePresentationCount) =>
        recentlyContainedWorld;

    internal bool LiveWorldRecentlySeen => _liveWorldExpected;

    internal GteProjectionOrigin LiveWorldMainProjection =>
        _liveWorldCapture.MainProjection;

    internal bool TryTakeLiveWorldOutput(
        out LiveWorldOutput output,
        int waitMilliseconds = 0) =>
        _liveWorldRenderer.TryTakeOutput(out output, waitMilliseconds);

    internal int LiveWorldOutputCount =>
        _liveWorldRenderer.PublishedOutputCount;

    internal bool LiveWorldWorkPending =>
        _liveWorldRenderer.HasPendingOrActiveWork;

    internal void DiscardLiveWorldOutputs() =>
        _liveWorldRenderer.DiscardPublishedOutputs();

    internal void DiscardLiveWorldOutputsBefore(int minimumInputPoll) =>
        _liveWorldRenderer.DiscardPublishedOutputsBefore(minimumInputPoll);

    internal void ReturnLiveWorldOutput(byte[] pixels) =>
        _liveWorldRenderer.ReturnOutput(pixels);

    internal void ShutdownLiveWorldRenderer()
    {
        _liveWorldCapture.Dispose();
        _liveWorldRenderer.Dispose();
        WorldCaptureContext.LiveRenderingEnabled = false;
    }

    void HleRect(int x, int y, int w, int h, int u, int v, int clut, int r, int g, int b, bool tex, bool semi, bool raw)
    {
        var be = GpuHle.Backend!;
        be.SetDrawEnv(CurEnv());
        be.DrawRect(new HleRect { X = x, Y = y, W = w, H = h, U = (short)u, V = (short)v, R = (byte)r, G = (byte)g, B = (byte)b },
            PrimOf(tex, semi, raw, clut));
    }

    void HleLine(int x0, int y0, int r0, int g0, int b0, int x1, int y1, int r1, int g1, int b1, bool semi, bool gouraud)
    {
        if (Math.Abs(x1 - x0) > 1023 || Math.Abs(y1 - y0) > 511) return;

        var flags = PrimOf(false, semi, false, 0, gouraud);
        var a = new HleVertex
        {
            X = x0,
            Y = y0,
            R = (byte)r0,
            G = (byte)g0,
            B = (byte)b0,
        };
        var b = new HleVertex
        {
            X = x1,
            Y = y1,
            R = (byte)r1,
            G = (byte)g1,
            B = (byte)b1,
        };
        if (_liveWorldCapture.Enabled)
        {
            var environment = CurEnv();
            _liveWorldCapture.RecordScreenLine(
                _projectedCaptureFrame + 1,
                in environment,
                in a,
                in b,
                in flags);
        }
        var be = GpuHle.Backend!;
        be.SetDrawEnv(CurEnv());
        be.DrawLine(
            in a,
            in b,
            in flags);
    }

    void HleFill(int x, int y, int w, int h, ushort color)
    {
        _liveWorldRenderer.InvalidateTextureUploads(x, y, w, h);
        MirrorFill(x, y, w, h, color);
        GpuHle.Backend!.FillRect(x, y, w, h, color);
    }

    void HleCopy(int sx, int sy, int dx, int dy, int w, int h)
    {
        _liveWorldRenderer.InvalidateTextureUploads(dx, dy, w, h);
        MirrorCopy(sx, sy, dx, dy, w, h);
        GpuHle.Backend!.CopyVram(sx, sy, dx, dy, w, h);
    }

    void MirrorFill(int x, int y, int w, int h, ushort color)
    {
        _mirroredFillWords += (long)w * h;
        for (int row = 0; row < h; ++row)
            for (int column = 0; column < w; ++column)
                Shadow[
                    (x + column) & (VramWidth - 1),
                    (y + row) & (VramHeight - 1)] = color;
    }

    void MirrorCopy(int sx, int sy, int dx, int dy, int w, int h)
    {
        _mirroredCopyWords += (long)w * h;
        int pixelCount = checked(w * h);
        if (_mirrorCopy.Length < pixelCount)
            _mirrorCopy = new ushort[pixelCount];
        for (int row = 0; row < h; ++row)
            for (int column = 0; column < w; ++column)
                _mirrorCopy[row * w + column] =
                    Shadow[
                        (sx + column) & (VramWidth - 1),
                        (sy + row) & (VramHeight - 1)];
        for (int row = 0; row < h; ++row)
            for (int column = 0; column < w; ++column)
            {
                int destinationX =
                    (dx + column) & (VramWidth - 1);
                int destinationY =
                    (dy + row) & (VramHeight - 1);
                ushort value = _mirrorCopy[row * w + column];
                ushort previous =
                    Shadow[destinationX, destinationY];
                if (_checkMask && (previous & 0x8000) != 0)
                    continue;
                if (_setMask)
                    value |= 0x8000;
                Shadow[destinationX, destinationY] = value;
            }
    }

    ushort[] _readBuf = Array.Empty<ushort>();
    ushort[] _mirrorCopy = Array.Empty<ushort>();

    void HleReadback(int x, int y, int w, int h)
    {
        int n = w * h;
        if (_readBuf.Length < n) _readBuf = new ushort[n];
        GpuHle.Backend!.ReadVram(x, y, w, h, _readBuf);
    }

    //img load
    ushort[] _hleLoad = Array.Empty<ushort>();
    bool _hleLoadActive;
    int _hleLoadPos;

    void HleLoadBegin()
    {
        _hleLoadActive = HleOn;
        if (!_hleLoadActive) return;
        int n = _loadW * _loadH;
        if (_hleLoad.Length < n) _hleLoad = new ushort[n];
        _hleLoadPos = 0;
    }

    void HleLoadPut(ushort value)
    {
        if (_hleLoadActive && _hleLoadPos < _hleLoad.Length) _hleLoad[_hleLoadPos++] = value;
    }

    void HleLoadFlush()
    {
        if (!_hleLoadActive) return;
        int count = _loadW * _loadH;
        _mirroredUploadWords += count;
        for (int index = 0; index < count; ++index)
        {
            int x = (_loadX + index % _loadW) &
                (VramWidth - 1);
            int y = (_loadY + index / _loadW) &
                (VramHeight - 1);
            ushort previous = Shadow[x, y];
            if (_checkMask && (previous & 0x8000) != 0)
                continue;
            Shadow[x, y] = _hleLoad[index];
        }
        GpuHle.Backend!.WriteVram(_loadX, _loadY, _loadW, _loadH, _hleLoad.AsSpan(0, _loadW * _loadH));
        _liveWorldRenderer.RecordTextureUpload(
            _loadX,
            _loadY,
            _loadW,
            _loadH,
            _hleLoad.AsSpan(0, count));
        _hleLoadActive = false;
    }
}
