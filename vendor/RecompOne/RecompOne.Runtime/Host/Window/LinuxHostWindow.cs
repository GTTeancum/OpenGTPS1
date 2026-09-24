#if OPENGT_LINUX_HOST
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using RecompOne.Runtime.Config;
using RecompOne.Runtime.Hardware;
using RecompOne.Runtime.Hle;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RecompOne.Runtime.Host;

/// <summary>
/// Linux window and final presentation. The recompiled guest and its original
/// CPU screen compositor keep executing. Provenance-backed 3D is rendered ONLY
/// by the native EGL renderer, with its complete authored screen-command stream.
/// No emulated CPU, staged asset scene, Wine, or reconstructed race state lives here.
/// </summary>
internal static unsafe class HostWindow
{
    static IWindow? _window;
    static IInputContext? _input;
    static GL? _gl;
    static Gpu? _gpu;
    static uint _program, _vao, _texture;
    static int _textureWidth, _textureHeight;
    static byte[] _screen = [];
    static bool _closed;
    static long _presents, _nativePresents, _lastAuthored = -1;
    static int _lastNativePoll = -1;
    static string? _requestedCapture;
    static readonly bool _visible = Environment.GetEnvironmentVariable("RECOMPONE_WINDOW_VISIBLE") != "0";
    static string? _resolution = Environment.GetEnvironmentVariable("RECOMPONE_OUTPUT_RESOLUTION");
    static bool _fullscreen;
    static int _framebufferWidth, _framebufferHeight;
    static readonly string? _captureDirectory = Environment.GetEnvironmentVariable("OPENGT_LINUX_CAPTURE_DIR");
    static readonly int _captureEvery = ReadInt("OPENGT_LINUX_CAPTURE_EVERY", 60, 1, 36000);
    static readonly int _captureStart = ReadInt("OPENGT_LINUX_CAPTURE_START_POLL", 0, 0, int.MaxValue);
    static readonly int _captureLimit = ReadInt("OPENGT_LINUX_CAPTURE_LIMIT", 60, 1, 10000);
    // Software-driver proof mode explicitly backpressures presentation, not the
    // guest clock. Default hardware operation uses the original 12 ms wait budget.
    static readonly bool _waitForRenderer = Environment.GetEnvironmentVariable("OPENGT_LINUX_WAIT_FOR_RENDER") == "1";
    static int _captures;
    static Stopwatch _elapsed = Stopwatch.StartNew();
    static StreamWriter? _telemetry;
    public static bool IsHeadless => !_visible;

    static int ReadInt(string name, int fallback, int minimum, int maximum) =>
        int.TryParse(Environment.GetEnvironmentVariable(name), out int value)
            && value >= minimum && value <= maximum ? value : fallback;
    static (int Width, int Height) ParseResolution(string? text)
    {
        string[] parts = (text ?? "960x720").Split('x', 'X');
        return parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h)
            && w >= 320 && w <= 8192 && h >= 240 && h <= 8192 ? (w, h) : (960, 720);
    }
    internal static (int Width, int Height) GetWorldTargetAspect() {
        int w=Volatile.Read(ref _framebufferWidth),h=Volatile.Read(ref _framebufferHeight);
        return _fullscreen&&w>0&&h>0 ? (w,h) : ParseResolution(_resolution ?? ConfigManager.View.OutputResolution);
    }
    public static bool IsKeyDown(Key key) => InputManager.IsKeyDown(key);
    public static void SetOutputResolution(string text) { _resolution=text; if (_window != null) { var (w,h)=ParseResolution(text); _window.Size=new(w,h); } }
    public static void SetFullscreen(bool on) { _fullscreen=on; if (_window != null) _window.WindowState=on ? WindowState.Fullscreen : WindowState.Normal; }
    public static void RequestDiscPath() => throw new NotSupportedException("Use the prepared game directory argument on Linux.");
    public static void WaitForValidDisc() => throw new NotSupportedException("Linux requires a prepared game directory; interactive disc selection is unavailable.");
    internal static void RequestDisplayCapture(string label) => _requestedCapture = label;

    public static void Initialize(string title)
    {
        if(_window!=null)throw new InvalidOperationException("Linux window already initialized");
        _closed=false;_gpu=null;_presents=0;_nativePresents=0;_lastAuthored=-1;_lastNativePoll=-1;
        _textureWidth=0;_textureHeight=0;_requestedCapture=null;_captures=0;_fullscreen=false;
        _elapsed=Stopwatch.StartNew();
        ConfigManager.Load();
        var (w,h)=GetWorldTargetAspect();
        var options=WindowOptions.Default with {
            Size=new Vector2D<int>(w,h), Title=title+" — Linux / OpenGT L04",
            IsVisible=_visible, VSync=false, ShouldSwapAutomatically=false,
            UpdatesPerSecond=0, FramesPerSecond=0,
            API=new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Default, new APIVersion(3,3)),
            WindowState=WindowState.Normal
        };
        _window=Silk.NET.Windowing.Window.Create(options);
        _window.Initialize();
        _window.FramebufferResize += size => {
            Volatile.Write(ref _framebufferWidth,size.X);
            Volatile.Write(ref _framebufferHeight,size.Y);
        };
        _framebufferWidth=_window.FramebufferSize.X;_framebufferHeight=_window.FramebufferSize.Y;
        _input=_window.CreateInput();
        InputManager.Initialize(_input);
        _gl=GL.GetApi(_window);
        uint vs=Compile(ShaderType.VertexShader,"""
            #version 330 core
            out vec2 uv;
            void main() {
              vec2 p=vec2((gl_VertexID<<1)&2,gl_VertexID&2);
              uv=vec2(p.x,1.0-p.y);
              gl_Position=vec4(p*2.0-1.0,0.0,1.0);
            }
            """);
        uint fs=Compile(ShaderType.FragmentShader,"""
            #version 330 core
            in vec2 uv; out vec4 pixel; uniform sampler2D image;
            void main() { pixel=texture(image,uv); }
            """);
        _program=_gl.CreateProgram();_gl.AttachShader(_program,vs);_gl.AttachShader(_program,fs);_gl.LinkProgram(_program);
        _gl.DeleteShader(vs);_gl.DeleteShader(fs);
        _gl.GetProgram(_program,ProgramPropertyARB.LinkStatus,out int ok);
        if(ok==0)throw new InvalidOperationException("Linux presentation shader link failed: "+_gl.GetProgramInfoLog(_program));
        _vao=_gl.GenVertexArray();_texture=_gl.GenTexture();
        _gl.BindVertexArray(_vao);_gl.ActiveTexture(TextureUnit.Texture0);_gl.BindTexture(TextureTarget.Texture2D,_texture);
        _gl.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMinFilter,(int)TextureMinFilter.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMagFilter,(int)TextureMagFilter.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureWrapS,(int)TextureWrapMode.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureWrapT,(int)TextureWrapMode.ClampToEdge);
        _gl.PixelStore(PixelStoreParameter.UnpackAlignment,1);_gl.PixelStore(PixelStoreParameter.PackAlignment,1);
        _gl.Disable(EnableCap.DepthTest);_gl.Disable(EnableCap.Blend);_gl.Disable(EnableCap.Dither);
        _gl.UseProgram(_program);_gl.Uniform1(_gl.GetUniformLocation(_program,"image"),0);
        GpuHle.Active=false;GpuHle.Backend=null;
        GpuHle.OutputAspect=(float)w/h;GpuHle.TargetAspect=(float)w/h;
        Console.Error.WriteLine($"[Linux-Host] GL={_gl.GetStringS(StringName.Renderer)} {_gl.GetStringS(StringName.Version)} output={w}x{h}");
        Console.Error.WriteLine("[Linux-Host] world=native-EGL; screen-compositor=original-CPU; host-debug-UI=not-ported; external-HD-atlas=not-supported");
        Console.Error.WriteLine($"[Linux-Host] renderer-wait={(_waitForRenderer ? "explicit-software-proof-backpressure" : "bounded-realtime")} guest-timing=unchanged");
        if(!string.IsNullOrWhiteSpace(_captureDirectory)) {
            Directory.CreateDirectory(_captureDirectory);
            _telemetry=new StreamWriter(Path.Combine(_captureDirectory,"presentations.jsonl")){AutoFlush=true};
        }
    }
    static uint Compile(ShaderType type,string source) {
        uint shader=_gl!.CreateShader(type);_gl.ShaderSource(shader,source);_gl.CompileShader(shader);
        _gl.GetShader(shader,ShaderParameterName.CompileStatus,out int ok);
        if(ok==0) {string error=_gl.GetShaderInfoLog(shader);_gl.DeleteShader(shader);throw new InvalidOperationException(error);}
        return shader;
    }
    internal static void Pump() {
        if(_closed||_window==null)return;
        _window.DoEvents();
        if(_window.IsClosing)Runtime.RequestShutdown();
    }
    public static void Present(Gpu? gpu)
    {
        if(_closed||_window==null||_gl==null)return;
        _gpu=gpu;
        gpu?.CapturePresentedFrame(); // Exactly once, before polling the next input.
        Pump();InputManager.Poll();
        if(InputManager.ConsumeFullscreenToggle())SetFullscreen(_window.WindowState!=WindowState.Fullscreen);
        if(gpu==null)return;
        _presents++;
        bool world=gpu.LiveWorldExpected;
        if(world) {
            int wait=_waitForRenderer ? 10000 : LiveWorldRenderer.OutputReadyWaitMilliseconds;
            if(gpu.TryTakeLiveWorldOutput(out LiveWorldOutput image,wait)) {
                try {
                    if(image.NativeTexture!=0)throw new InvalidOperationException("Windows texture handle returned on Linux");
                    if(image.Frame<=_lastAuthored)throw new InvalidOperationException("Non-chronological native frame");
                    _lastAuthored=image.Frame;_lastNativePoll=image.InputPoll;_nativePresents++;
                    Draw(image.Pixels,image.Width,image.Height,(float)image.Width/image.Height);
                    Record(true,image.Frame,image.InputPoll,image.Stats.OutputCommands,image.Stats.RenderMicroseconds);
                } finally {gpu.ReturnLiveWorldOutput(image.Pixels);}
            } else {
                if(_waitForRenderer && gpu.LiveWorldWorkPending)
                    throw new TimeoutException("Linux native renderer did not complete the authored frame within 10 seconds");
                // Preserve the last genuine frame while the asynchronous worker
                // finishes. Do not substitute PS1 world rendering for missing 3D.
            }
        } else {
            gpu.DiscardLiveWorldOutputs();
            if(gpu.DisplayEnabled && gpu.DisplayWidth>0 && gpu.DisplayHeight>0) {
                int w=gpu.DisplayWidth,h=gpu.DisplayHeight;
                ConvertDisplay(gpu,w,h);
                Draw(_screen,w,h,4f/3f);
                Record(false,0,InputManager.CurrentPoll,0,0);
            }
        }
        _window.SwapBuffers();
        if(_presents<=3||_presents%120==0)
            Console.Error.WriteLine($"[Linux-Host] poll={InputManager.CurrentPoll} stage={InputManager.CurrentScriptStage} presents={_presents} native={_nativePresents} authored={_lastAuthored} nativePoll={_lastNativePoll} elapsed={_elapsed.Elapsed.TotalSeconds:F2}s dropped={gpu.LiveWorldDroppedFrames}");
    }
    static void Draw(byte[] rgba,int width,int height,float aspect) {
        if(width<=0||height<=0||rgba.Length<checked(width*height*4))throw new ArgumentException("Invalid presented image");
        var gl=_gl!;var fb=_window!.FramebufferSize;if(fb.X<=0||fb.Y<=0)return;
        gl.Viewport(0,0,(uint)fb.X,(uint)fb.Y);gl.ClearColor(0,0,0,1);gl.Clear(ClearBufferMask.ColorBufferBit);
        int w=fb.X,h=(int)Math.Round(w/aspect);if(h>fb.Y){h=fb.Y;w=(int)Math.Round(h*aspect);}
        gl.Viewport((fb.X-w)/2,(fb.Y-h)/2,(uint)w,(uint)h);gl.UseProgram(_program);gl.BindVertexArray(_vao);
        gl.ActiveTexture(TextureUnit.Texture0);gl.BindTexture(TextureTarget.Texture2D,_texture);
        fixed(byte* p=rgba) {
            if(width!=_textureWidth||height!=_textureHeight) {
                gl.TexImage2D(TextureTarget.Texture2D,0,InternalFormat.Rgba8,(uint)width,(uint)height,0,PixelFormat.Rgba,PixelType.UnsignedByte,p);
                _textureWidth=width;_textureHeight=height;
            } else gl.TexSubImage2D(TextureTarget.Texture2D,0,0,0,(uint)width,(uint)height,PixelFormat.Rgba,PixelType.UnsignedByte,p);
        }
        gl.DrawArrays(PrimitiveType.Triangles,0,3);
        var error=gl.GetError();if(error!=GLEnum.NoError)throw new InvalidOperationException($"Linux presentation GL error: {error}");
    }
    static void Record(bool native,long authored,int sourcePoll,uint commands,ulong microseconds) {
        _telemetry?.WriteLine(JsonSerializer.Serialize(new {
            presentation=_presents,poll=InputManager.CurrentPoll,stage=InputManager.CurrentScriptStage,
            native,authoredFrame=authored,sourcePoll,commands,renderMicroseconds=microseconds,
            elapsedSeconds=_elapsed.Elapsed.TotalSeconds,sourceWidth=_textureWidth,sourceHeight=_textureHeight
        }));
        bool automatic=native&&_nativePresents%_captureEvery==0&&InputManager.CurrentPoll>=_captureStart&&_captures<_captureLimit;
        string? requested=_requestedCapture;
        if(requested!=null)_requestedCapture=null;
        if((automatic||requested!=null)&&!string.IsNullOrWhiteSpace(_captureDirectory)) {
            var fb=_window!.FramebufferSize;
            var bytes=new byte[checked(fb.X*fb.Y*4)];
            fixed(byte* p=bytes)_gl!.ReadPixels(0,0,(uint)fb.X,(uint)fb.Y,PixelFormat.Rgba,PixelType.UnsignedByte,p);
            string label=new string((requested??"game").Where(c=>char.IsAsciiLetterOrDigit(c)||c=='_'||c=='-').Take(80).ToArray());
            string name=$"{_captures++:0000}_{label}_poll{InputManager.CurrentPoll}_frame{authored}";
            string path=Path.Combine(_captureDirectory,name+".ppm");
            using(var output=File.Create(path)) {
                output.Write(Encoding.ASCII.GetBytes($"P6\n{fb.X} {fb.Y}\n255\n"));
                byte[] row=new byte[fb.X*3];
                for(int y=fb.Y-1;y>=0;y--) {for(int x=0;x<fb.X;x++) {int src=(y*fb.X+x)*4;row[x*3]=bytes[src];row[x*3+1]=bytes[src+1];row[x*3+2]=bytes[src+2];}output.Write(row);}
            }
            File.WriteAllText(Path.Combine(_captureDirectory,name+".json"),JsonSerializer.Serialize(new {
                native,authoredFrame=authored,sourcePoll,hostPoll=InputManager.CurrentPoll,
                stage=InputManager.CurrentScriptStage,width=fb.X,height=fb.Y,
                sourceWidth=_textureWidth,sourceHeight=_textureHeight,commands,renderMicroseconds=microseconds,
                note="Actual full game host backbuffer before swap. No staged asset renderer or image relighting."
            },new JsonSerializerOptions{WriteIndented=true}));
            Console.Error.WriteLine($"[Linux-Capture] native={native} authored={authored} sourcePoll={sourcePoll} file={path}");
        }
    }
    static byte VramByte(ushort[] vram,int byteOffset) {
        ushort value=vram[(byteOffset>>1)&(vram.Length-1)];return (byte)(value>>((byteOffset&1)*8));
    }
    static void ConvertDisplay(Gpu gpu,int w,int h) {
        int bytes=checked(w*h*4);if(_screen.Length<bytes)_screen=new byte[bytes];
        ushort[] vram=gpu.Vram;
        for(int y=0;y<h;y++)for(int x=0;x<w;x++) {
            int i=(y*w+x)*4;
            if(gpu.Display24Bit) {
                int b=((gpu.DisplayY+y)*Gpu.VramWidth+gpu.DisplayX)*2+x*3;
                _screen[i]=VramByte(vram,b);_screen[i+1]=VramByte(vram,b+1);_screen[i+2]=VramByte(vram,b+2);
            } else {
                ushort c=vram[((gpu.DisplayY+y)&511)*1024+((gpu.DisplayX+x)&1023)];
                _screen[i]=(byte)((c&31)<<3);_screen[i+1]=(byte)(((c>>5)&31)<<3);_screen[i+2]=(byte)(((c>>10)&31)<<3);
            }
            _screen[i+3]=255;
        }
    }
    public static void Shutdown() {
        if(_closed)return;_closed=true;
        Console.Error.WriteLine($"[Linux-Host] shutdown presents={_presents} native={_nativePresents} captures={_captures}");
        _gpu?.ShutdownLiveWorldRenderer();InputManager.Shutdown();
        _telemetry?.Dispose();_telemetry=null;_input?.Dispose();_input=null;
        if(_gl!=null){_gl.DeleteTexture(_texture);_gl.DeleteVertexArray(_vao);_gl.DeleteProgram(_program);_gl.Dispose();}
        _window?.Dispose();_gl=null;_window=null;
    }
}
#endif
