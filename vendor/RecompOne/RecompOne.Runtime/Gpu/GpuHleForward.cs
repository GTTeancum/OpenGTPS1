using RecompOne.Runtime.Hle;
using RecompOne.Runtime.Config;

namespace RecompOne.Runtime;

public sealed partial class Gpu
{
    readonly ProjectedSceneCapture _projectedCapture = new();
    readonly WorldSceneCapture _worldCapture = new();
    long _projectedCaptureFrame;

    static bool HleOn => GpuHle.Active && GpuHle.Backend is { Ready: true };
    bool DitherEnabled => _dither && ConfigManager.View.Ps1Dithering;

    int CurTPage() => ((_texPageX / 64) & 0xf) | (((_texPageY / 256) & 1) << 4)
                    | ((_blendMode & 3) << 5) | ((_texDepth & 3) << 7);

    HleDrawEnv CurEnv() => new()
    {
        ClipX0 = _drawAreaLeft, ClipY0 = _drawAreaTop, ClipX1 = _drawAreaRight, ClipY1 = _drawAreaBottom,
        TwMaskX = _texWinMaskX, TwMaskY = _texWinMaskY, TwOffX = _texWinOffX, TwOffY = _texWinOffY,
        SetMask = _setMask, CheckMask = _checkMask, Dither = DitherEnabled,
    };

    static HleVertex HV(in Vert v) => new()
    {
        X = v.X, Y = v.Y, R = (byte)v.R, G = (byte)v.G, B = (byte)v.B, U = (short)v.U, V = (short)v.V,
        Z = v.Z, HasGteZ = v.HasGteZ,
    };

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

    void CaptureTri(
        in Vert a,
        in Vert b,
        in Vert c,
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
        CaptureHleTri(in ha, in hb, in hc, in flags);
        if (WorldCaptureContext.CaptureEnabled && _worldCapture.Enabled)
        {
            Gte.TryGetPacketOrigin(
                a.SourceAddress,
                out GteProjectionOrigin originA);
            Gte.TryGetPacketOrigin(
                b.SourceAddress,
                out GteProjectionOrigin originB);
            Gte.TryGetPacketOrigin(
                c.SourceAddress,
                out GteProjectionOrigin originC);
            var environment = CurEnv();
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
    }

    void CaptureHleTri(
        in HleVertex a,
        in HleVertex b,
        in HleVertex c,
        in PrimFlags flags)
    {
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
    }

    internal void CapturePresentedFrame()
    {
        _projectedCaptureFrame++;
        if ((_projectedCapture.NeedsVramSnapshot ||
             (WorldCaptureContext.CaptureEnabled &&
              _worldCapture.NeedsVramSnapshot)) && HleOn)
        {
            GpuHle.Backend!.ReadVram(
                0,
                0,
                VramShadow.Width,
                VramShadow.Height,
                Shadow.Pixels);
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
            _worldCapture.OnPresentedFrame(
                _projectedCaptureFrame,
                Host.InputManager.CurrentPoll,
                in display,
                Shadow.Pixels);
        }
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

        var be = GpuHle.Backend!;
        be.SetDrawEnv(CurEnv());
        be.DrawLine(
            new HleVertex { X = x0, Y = y0, R = (byte)r0, G = (byte)g0, B = (byte)b0 },
            new HleVertex { X = x1, Y = y1, R = (byte)r1, G = (byte)g1, B = (byte)b1 },
            PrimOf(false, semi, false, 0, gouraud));
    }

    void HleFill(int x, int y, int w, int h, ushort color) => GpuHle.Backend!.FillRect(x, y, w, h, color);
    void HleCopy(int sx, int sy, int dx, int dy, int w, int h) => GpuHle.Backend!.CopyVram(sx, sy, dx, dy, w, h);

    ushort[] _readBuf = Array.Empty<ushort>();

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
        GpuHle.Backend!.WriteVram(_loadX, _loadY, _loadW, _loadH, _hleLoad.AsSpan(0, _loadW * _loadH));
        _hleLoadActive = false;
    }
}
