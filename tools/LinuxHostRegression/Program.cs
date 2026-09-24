using System.Reflection;
using RecompOne.Runtime;
using RecompOne.Runtime.Hle;

// Execute the REAL GPU command parser with the Linux CPU screen compositor.
// Reflection is test-only access to the private source-upload registry.
const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
int checks=0;
void Check(bool value,string label){checks++;if(!value)throw new Exception(label);}
GpuHle.Active=false;GpuHle.Backend=null;
var gpu=new Gpu();
object renderer=typeof(Gpu).GetField("_liveWorldRenderer",Private)!.GetValue(gpu)!;
Array Uploads()=>(Array)renderer.GetType().GetField("_textureUploads",Private)!.GetValue(renderer)!;
ulong Key(int i=0)=>(ulong)Uploads().GetValue(i)!.GetType().GetField("Key")!.GetValue(Uploads().GetValue(i))!;
void Command(params uint[] words){foreach(var w in words)gpu.WriteGp0(w);}
uint XY(int x,int y)=>(uint)(x|(y<<16));
void Begin(int x,int y,int w,int h)=>Command(0xa0000000,XY(x,y),XY(w,h));
void Load(int x,int y,int w,int h,params ushort[] words){Begin(x,y,w,h);for(int i=0;i<words.Length;i+=2)gpu.WriteGp0((uint)(words[i]|(i+1<words.Length?words[i+1]<<16:0)));}
try {
    Load(800,10,2,1,0x1234,0x4321);
    Check(Uploads().Length==1,"CPU upload publishes original bank identity");
    Check(gpu.Vram[10*1024+800]==0x1234&&gpu.Vram[10*1024+801]==0x4321,"actual CPU VRAM receives exact words");
    const ulong seed=14695981039346656037UL,prime=1099511628211UL;
    ulong expected=seed;foreach(byte b in new byte[]{2,0,1,0,0x34,0x12,0x21,0x43}){expected^=b;expected=unchecked(expected*prime);}
    Check(Key()==expected,"source upload dimensions/payload FNV matches original contract");
    Begin(800,10,2,1);Check(Uploads().Length==0,"partial overwrite immediately invalidates old identity");
    gpu.WriteGp1(0x01000000);Check(Uploads().Length==0,"aborted upload never publishes an identity");
    Load(800,10,2,1,0x1111,0x2222);Load(850,12,2,1,0x3333,0x4444);
    Check(Uploads().Length==2,"unrelated banks retained");
    Command(0x020000ff,XY(800,10),XY(16,1));
    Check(Uploads().Length==1,"CPU fill invalidates only overlapping upload");
    Check(gpu.Vram[10*1024+800]==31,"fill preserves original CPU color conversion");
    Command(0x80000000,XY(800,10),XY(850,12),XY(2,1));
    Check(Uploads().Length==0,"CPU copy invalidates destination upload");
    Check(gpu.Vram[12*1024+850]==31,"CPU copy still writes actual source pixels");
    Load(800,10,2,1,0x8001,0x1234);
    Command(0xe6000002);Load(800,10,2,1,0x5555,0x6666);
    Check(gpu.Vram[10*1024+800]==0x8001&&gpu.Vram[10*1024+801]==0x6666,"mask-check behavior unchanged");
    Check(Uploads().Length==0,"mask-dependent upload not misidentified as source TIM");
    Command(0xe6000001);Load(800,10,2,1,1,2);
    Check(gpu.Vram[10*1024+800]==0x8001&&gpu.Vram[10*1024+801]==0x8002,"set-mask behavior unchanged");
    Check(Uploads().Length==0,"set-mask upload not claimed to be original source TIM");Command(0xe6000000);
    Load(0,30,2,1,1,2);Load(1022,30,2,1,3,4);Load(500,30,2,1,5,6);
    Begin(1022,30,4,1);Check(Uploads().Length==1,"wrapped transfer invalidates both horizontal edges");Command(0x08070605,0x04030201);
    Check(gpu.Vram[30*1024]==0x0201&&gpu.Vram[30*1024+1]==0x0403,"original horizontal VRAM wrap preserved");
    Check(Uploads().Length==1,"wrapped transfer has no false contiguous bank identity");
    Load(200,0,2,1,1,2);Load(200,511,2,1,3,4);
    Begin(200,511,2,2);Check(Uploads().Length==1,"wrapped transfer invalidates both vertical edges");Command(0x08070605,0x04030201);
    Check(gpu.Vram[200]==0x0201&&gpu.Vram[201]==0x0403,"original vertical wrap preserved");
    Load(300,50,2,1,1,2);Load(300,50,2,1,3,4);
    Check(Uploads().Length==2,"same rectangle replaces its old identity without duplication");
    Check(!GpuHle.Active&&GpuHle.Backend==null,"tests never enabled substitute HLE graphics");
    Console.WriteLine($"Linux host source-upload regression: {checks} checks passed");
} finally {typeof(Gpu).GetMethod("ShutdownLiveWorldRenderer",Private)!.Invoke(gpu,null);}

if(args.Contains("--window")) {
    string directory=Path.Combine(Path.GetTempPath(),"opengt-host-test-"+Guid.NewGuid().ToString("N"));
    Environment.SetEnvironmentVariable("OPENGT_LINUX_CAPTURE_DIR",directory);
    Environment.SetEnvironmentVariable("RECOMPONE_WINDOW_VISIBLE","0");
    Environment.SetEnvironmentVariable("RECOMPONE_DISABLE_LIVE_INPUT","1");
    Environment.SetEnvironmentVariable("RECOMPONE_OUTPUT_RESOLUTION","640x480");
    var host=typeof(Gpu).Assembly.GetType("RecompOne.Runtime.Host.HostWindow")!;
    object? Call(string name,params object?[] values)=>host.GetMethod(name,BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic)!.Invoke(null,values);
    try {
        for(int i=0;i<2;i++) {
            Call("Initialize","Linux presentation regression");
            var screen=new Gpu();screen.WriteGp1(0x03000000);
            screen.WriteGp0(0x020000ff);screen.WriteGp0(0);screen.WriteGp0((511u<<16)|1023u);
            Call("RequestDisplayCapture","red");Call("Present",screen);
            var image=Directory.GetFiles(directory,"*.ppm").Single();
            var bytes=File.ReadAllBytes(image);int at=0,lines=0;while(lines<3){if(bytes[at++]==10)lines++;}
            var dimensions=System.Text.Encoding.ASCII.GetString(bytes,0,at).Split('\n')[1].Split(' ');
            int width=int.Parse(dimensions[0]),height=int.Parse(dimensions[1]);
            Check(width==(i==0?640:960)&&height==(i==0?480:540),"capture dimensions match current host resolution across recreation");
            Check(bytes.Length==at+width*height*3,"capture byte count matches declared framebuffer dimensions");
            int center=at+((height/2)*width+width/2)*3;
            Check(bytes[center]==248&&bytes[center+1]==0&&bytes[center+2]==0,"actual original CPU screen presented as red, including host recreation");
            Call("SetOutputResolution","960x540");
            Check(((ValueTuple<int,int>)Call("GetWorldTargetAspect")!).Equals((960,540)),"changed output resolution reaches world aspect");
            Call("Shutdown");File.Delete(image);
        }
    } finally {Call("Shutdown");Directory.Delete(directory,true);}
    Console.WriteLine($"Linux host combined regression: {checks} checks passed, including two GL context lifecycles");
}
