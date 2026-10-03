using System.Reflection;
using System.Runtime.CompilerServices;
using System.Buffers.Binary;
using System.Security.Cryptography;
using RecompOne.Runtime;
using RecompOne.Runtime.Memory;
using RecompOne.Runtime.Hle;

int checks=0;
void Check(bool b,string why){++checks;if(!b)throw new Exception(why);}
var assembly=typeof(Gpu).Assembly;
var helper=assembly.GetType("RecompOne.Runtime.PrimaryShadowCamera")!.GetMethod("TryGet",BindingFlags.NonPublic|BindingFlags.Static)!;
var capture=typeof(WorldCaptureContext).GetMethod("CapturePrimaryShadowCamera",BindingFlags.NonPublic|BindingFlags.Static)!;
(bool Valid,int X,int Y,int Z) Get(GteProjectionOrigin c){object?[] args=[c,0,0,0];bool v=(bool)helper.Invoke(null,args)!;return(v,(int)args[1]!, (int)args[2]!, (int)args[3]!);}
var memory=new PSMemory();GpuHle.Active=false;GpuHle.Backend=null;
WorldCaptureContext.LiveRenderingEnabled=true;
const uint model=0x80001000,camera=0x1F800000;
void Write(int nx,int ny,int nz,int sx,int sy,int sz){
    memory.WriteU32(camera+0x20,unchecked((uint)nx));memory.WriteU32(camera+0x28,unchecked((uint)ny));memory.WriteU32(camera+0x24,unchecked((uint)nz));
    memory.WriteU32(model+0x30,unchecked((uint)sx));memory.WriteU32(model+0x38,unchecked((uint)sy));memory.WriteU32(model+0x34,unchecked((uint)sz));
}
GteProjectionOrigin Origin(WorldObjectContext owner,short[] r){
    var a=owner.ShadowCamera;
    int x=unchecked(a.NegativeX+a.SectorX)>>10,y=unchecked(a.NegativeY+a.SectorY)>>10,z=unchecked(a.NegativeZ+a.SectorZ)>>10;
    // Execute the ACTUAL uploaded GTE translation routine, including its
    // 32-bit IR input extension, rather than copying TryGet's arithmetic.
    for(int j=0;j<4;++j)Gte.WriteControl(j,unchecked((uint)(ushort)r[j*2]|((uint)(ushort)r[j*2+1]<<16)));
    Gte.WriteControl(4,unchecked((uint)(ushort)r[8]));Gte.ExecuteTrackTranslation(unchecked((uint)x),unchecked((uint)y),unchecked((uint)z));
    return new GteProjectionOrigin(0,0,0,0,0,0,0,0,0,r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8],unchecked((int)Gte.Read(25)),unchecked((int)Gte.Read(26)),unchecked((int)Gte.Read(27)),160<<16,120<<16,256,77,owner,0,0,GteProjectionOriginFlags.None);
}
byte[] Header(GteProjectionOrigin origin){
    var type=assembly.GetType("RecompOne.Runtime.Hle.LiveWorldFrameRecorder")!;
    var renderer=RuntimeHelpers.GetUninitializedObject(type);
    using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);
    type.GetField("_stream",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(renderer,stream);
    type.GetField("_writer",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(renderer,writer);
    object display=new HleDispEnv {W=320,H=240};
    type.GetMethod("WriteHeader",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(renderer,[11L,35,display,160L,1048576L,origin]);
    writer.Flush();return stream.ToArray();
}
try {
    short[][] rotations=[[0,4095,0,-1226,0,-3531,3974,0,-996],[4096,0,0,0,0,-4096,0,4096,0],[14500,2400,-1500,3100,-14000,2000,-2100,3100,14500]];
    var rng=new Random(801026);
    GteProjectionOrigin last=default;
    for(int exp=8;exp<=10;++exp)for(int j=0;j<160;++j){
        int nx=rng.Next(-200_000_000,200_000_000),ny=rng.Next(-200_000_000,200_000_000),nz=rng.Next(-200_000_000,200_000_000);
        int sx=rng.Next(-200_000_000,200_000_000),sy=rng.Next(-200_000_000,200_000_000),sz=rng.Next(-200_000_000,200_000_000);
        WorldCaptureContext.BeginScenePass(WorldScenePass.Main);WorldCaptureContext.BeginTrackObject(5,model);
        Write(nx,ny,nz,sx,sy,sz);var before=SHA256.HashData(memory.Ram);
        capture.Invoke(null,[memory,model,camera]);WorldCaptureContext.SetCurrentDepthScaleExponent(exp);
        var owner=WorldCaptureContext.Current;Check(owner.ShadowCamera.Valid,"source hook failed to retain primary camera");
        Check(SHA256.HashData(memory.Ram).SequenceEqual(before),"source hook wrote guest memory");
        var c=Origin(owner,rotations[j%rotations.Length]);var result=Get(c);
        Check(result==(true,nx&~1023,ny&~1023,nz&~1023),"world offset not verified by actual GTE translation");
        Check(!Get(c with {TranslateX=c.TranslateX+1}).Valid,"mismatched cached/aligned translation accepted");
        Check(!Get(c with {Object=c.Object with {DepthScaleValid=false}}).Valid,"missing normalization accepted");
        Check(!Get(c with {Object=c.Object with {ScenePass=WorldScenePass.Auxiliary}}).Valid,"secondary view inherits main camera");
        last=c;
    }
    foreach(int exponent in new[]{-1,7,11,16})Check(!Get(last with{Object=last.Object with{DepthScaleExponent=exponent}}).Valid,"invalid primary exponent accepted");
    Check(!Get(last with{Object=last.Object with{Kind=WorldObjectKind.Vehicle}}).Valid,"car masquerades as a world camera");
    Check(!Get(last with{TransformId=0}).Valid,"uninitialized camera accepted");
    var header=Header(last);Check(header.Length==160,"v6 header size changed");
    Check(BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(8))==6,"live capture version changed");
    Check((BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(84))&24)==24,"primary/anchor flags missing");
    Check(BinaryPrimitives.ReadInt16LittleEndian(header.AsSpan(114))==10,"depth exponent changed wire width/location");
    Check(BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(116))==last.TranslateX,"extension overwrote existing translation");
    var expected=Get(last);Check(BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(148))==expected.X&&BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(152))==expected.Y&&BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(156))==expected.Z,"source anchor byte layout mismatch");
    var absent=Header(last with{Object=last.Object with{ShadowCamera=default}});Check(absent.Length==160&&(BinaryPrimitives.ReadUInt32LittleEndian(absent.AsSpan(84))&16)==0,"old/no-anchor header not compatible");
    Check(absent.AsSpan(148,12).SequenceEqual(new byte[12]),"invalid anchor serialized stale offsets");
    if(args.Length>0)File.WriteAllBytes(args[0],header);
    var select=assembly.GetType("RecompOne.Runtime.PrimaryShadowCamera")!.GetMethod("SelectVerified",BindingFlags.NonPublic|BindingFlags.Static)!;
    var dominant=last with{TransformId=99, Object=last.Object with{ShadowCamera=default}};
    GteProjectionOrigin Select(params (int Count,GteProjectionOrigin Origin)[] choices) =>
        (GteProjectionOrigin)select.Invoke(null,[dominant,choices])!;
    Check(Select((900,dominant),(30,last)).TransformId==last.TransformId,"rotated non-primary instance wins over verified course camera");
    Check(Select((900,dominant),(30,last with{ProjectionPlane=320})).TransformId==99,"different projection selected as source camera");
    Check(Select((900,dominant),(30,last with{ProjectionOffsetX=last.ProjectionOffsetX+1})).TransformId==99,"different subview origin selected");
    Check(Select((900,dominant),(30,last with{Object=last.Object with{SceneGeneration=last.Object.SceneGeneration-1}})).TransformId==99,"previous frame generation borrowed camera");
    Check(Select((900,dominant),(30,last with{TranslateZ=last.TranslateZ+1})).TransformId==99,"failed source verification still wins selection");
    Check(Select((10,last),(40,last with{TransformId=100})).TransformId==100,"verified camera weight ordering incorrect");
    Check((GteProjectionOrigin)select.Invoke(null,[last,new (int,GteProjectionOrigin)[]{(999,dominant)}])! == last,"already valid dominant replaced unnecessarily");
    // Scope and malformed pointer guards. The existing beginning of each
    // object/frame clears provenance; failed reads must not reuse an old one.
    WorldCaptureContext.BeginTrackObject(5,model);capture.Invoke(null,[memory,model,camera]);Check(WorldCaptureContext.Current.ShadowCamera.Valid,"guard fixture not initialized");
    capture.Invoke(null,[memory,model,0x1F8003F0u]);Check(!WorldCaptureContext.Current.ShadowCamera.Valid,"truncated scratch camera retained old anchor");
    WorldCaptureContext.BeginTrackObject(5,0x801FFFF0);capture.Invoke(null,[memory,0x801FFFF0u,camera]);Check(!WorldCaptureContext.Current.ShadowCamera.Valid,"truncated model header accepted");
    WorldCaptureContext.EndScenePass();Check(!WorldCaptureContext.Current.ShadowCamera.Valid,"scene end retained camera");
    WorldCaptureContext.BeginScenePass(WorldScenePass.Auxiliary);WorldCaptureContext.BeginTrackObject(5,model);capture.Invoke(null,[memory,model,camera]);Check(!WorldCaptureContext.Current.ShadowCamera.Valid,"auxiliary pass captured a primary anchor");
    Console.WriteLine($"Primary world camera and live wire: {checks} checks passed");
} finally {WorldCaptureContext.EndScenePass();WorldCaptureContext.LiveRenderingEnabled=false;typeof(Gpu).GetMethod("ShutdownLiveWorldRenderer",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(RecompOne.Runtime.Runtime.Gpu,null);}
