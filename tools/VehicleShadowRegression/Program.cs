using System.Reflection;
using System.Buffers.Binary;
using System.Security.Cryptography;
using RecompOne.Runtime;
using RecompOne.Runtime.Memory;
using RecompOne.Runtime.Hle;

GpuHle.Active=false;GpuHle.Backend=null;
var memory=new PSMemory();int checks=0;
void Check(bool b,string why){checks++;if(!b)throw new Exception(why);}
var decode=typeof(Gpu).Assembly.GetType("RecompOne.Runtime.VehicleShadowSource")!.GetMethod("Decode",BindingFlags.Static|BindingFlags.NonPublic)!;
var owner=new WorldObjectContext(WorldObjectKind.Vehicle,2,0x80005000,3,true,WorldScenePass.Main,1);
byte[]? Decode(uint model=0x80001000)=>(byte[]?)decode.Invoke(null,[model,owner,10,memory]);
uint[] strides=[16,16,24,28,28,28,36,40];
const uint model=0x80001000, vertices=0x80002000;
try {
 memory.WriteU16(model,4);memory.WriteU32(model+0x14,vertices);
 for(uint j=0;j<4;j++){memory.WriteU16(vertices+j*8,(ushort)(j*100));memory.WriteU16(vertices+j*8+2,(ushort)(20+j));memory.WriteU16(vertices+j*8+4,(ushort)(30+j));}
 for(uint stream=0;stream<8;stream++){
  uint pointer=0x80003000+stream*0x100;
  memory.WriteU16(model+4+stream*2,2);memory.WriteU32(model+0x1C+stream*4,pointer);
  for(uint i=0;i<2;i++){
   uint p=pointer+i*strides[stream];memory.WriteU32(p,0x03020100);
   byte opcode=(byte)(0x20|(stream>=4?4:0)|((stream&1)!=0?8:0)|((stream&2)!=0?16:0));memory.WriteU8(p+15,opcode);
   if(stream>=4){uint uv=stream<6?16u:((stream&1)!=0?28u:24u);
    memory.WriteU32(p+uv,0x01000403);memory.WriteU32(p+uv+4,0x00000504);memory.WriteU32(p+uv+8,0x07060605);}
  }
 }
 memory.WriteU32(0x1F80039C,0x02000000);memory.WriteU32(0x1F8003A0,0);
 var before=SHA256.HashData(memory.Ram);var bytes=Decode();Check(bytes!=null,"all eight original streams decoded");
 Check(bytes!.Length==48+24*32&&BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(36))==24,"bounded wire face count and variable record strides");
 Check(SHA256.HashData(memory.Ram).SequenceEqual(before),"source capture must not modify guest RAM");
 // Stream1 first quad: source A/B/D then B/D/C, with no bow-tie overlap.
 int first=48+2*32;
 Check(BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(first+8))==0&&BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(first+16))==100&&BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(first+24))==300,"quad first triangle follows original GTE packet ordering");
 Check(BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(first+32+8))==100&&BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(first+32+16))==300&&BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(first+32+24))==200,"quad second triangle follows GPU strip split");
 int tex=48+12*32;Check(BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(tex+6))==0x300,"original per-car palette base applied");
 Check(bytes[tex+14]==3&&bytes[tex+15]==4,"original UVs retained, not regenerated");
 foreach(uint bad in new uint[]{0,0x1F800000,0x801FFFC0,0xFFFFFFFF})Check(Decode(bad)==null,"out-of-range selected mesh rejected");
 memory.WriteU16(model,257);Check(Decode()==null,"oversized vertex count rejected");memory.WriteU16(model,4);
 memory.WriteU32(model+0x1C,0x801FFFF8);Check(Decode()==null,"truncated face stream rejected");memory.WriteU32(model+0x1C,0x80003000);
 memory.WriteU8(0x80003000,9);Check(Decode()==null,"invalid source index rejected");memory.WriteU8(0x80003000,0);
 memory.WriteU8(0x8000300F,0);Check(Decode()==null,"non-polygon source opcode rejected");memory.WriteU8(0x8000300F,0x20);
 Check(Decode()!.SequenceEqual(bytes),"valid source restored without stale decoder state");
 if(args.Length>0)File.WriteAllBytes(args[0],bytes);
 Console.WriteLine($"Vehicle source decoder: {checks} checks passed");
} finally {typeof(Gpu).GetMethod("ShutdownLiveWorldRenderer",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(RecompOne.Runtime.Runtime.Gpu,null);}
