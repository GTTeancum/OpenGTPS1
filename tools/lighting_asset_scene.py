#!/usr/bin/env python3
"""L03 reproducible asset -> version-6 command capture -> live native C ABI proof.

This is a STAGED ORIGINAL-ASSET test, not a recording of the game running.
No generated image or prerecorded L02 picture contributes to the output.
Original game members stay read-only and are not included in the checkpoint.
"""
from __future__ import annotations
import argparse
from array import array
import ctypes as C
from dataclasses import dataclass
import hashlib
import json
import math
from pathlib import Path
import struct
import sys
import time
import numpy as np
from PIL import Image
from analyze_track_lighting import Volume, POOL_SIZES, package, sha
from build_gt2_texture_pack import trp_tim_members, upload_hash, car_material_palette

@dataclass
class Triangle:
    points: tuple
    uv: tuple
    rgb: tuple
    flags: int
    page: int
    clut: int
    kind: int
    owner: int
    model: int
    identity: tuple
    origin: tuple
    axes: np.ndarray

PRIMARY_AXES=np.array([[1,0,0],[0,0,-1],[0,1,0]],dtype=float)
CAR_AXES=np.diag([1.,-1.,1.])

def primary_triangles(data: bytes, variant: int=0) -> list[Triangle]:
    if variant not in (0,1):raise ValueError('primary mesh variant must be 0 or 1')
    def get(fmt,offset):
        if offset<0 or offset+struct.calcsize(fmt)>len(data):raise ValueError('truncated source course field')
        return struct.unpack_from(fmt,data,offset)
    def u32(o):return get('<I',o)[0]
    if len(data)<0x44 or get('<H',14)[0]!=31:raise ValueError('unsupported TRO revision')
    primary=u32(16);count=get('<H',primary+4)[0];table=u32(primary+8)
    if not 0<count<8192:raise ValueError('invalid primary object count')
    result=[]
    for owner in range(count):
        obj=u32(primary+12+owner*4)
        centers=get('<iii',obj+0x30)
        # Generated func_80026B40 source sector shift. The mesh coordinates
        # then use XY horizontal and Z height; that conversion is independent.
        shifts=np.array([(centers[j]>>22)*4096 for j in (0,2,1)],dtype=float)
        origin=tuple(PRIMARY_AXES@shifts)
        mesh=obj+0xA4 if variant==0 else u32(obj+0x94)
        vp=u32(mesh);nv=u32(mesh+0x2c)+1
        if not 0<nv<=512:raise ValueError('invalid primary vertex count')
        vertices=[get('<hhh',vp+i*8) for i in range(nv)]
        for stream,size in enumerate(POOL_SIZES):
            ptr=u32(mesh+4+stream*4);count2=get('<H',mesh+0x30+stream*2)[0]
            textured=stream>=4;gouraud=bool(stream&2);quad=bool(stream&1)
            for item in range(count2):
                off=ptr+item*size;packed,descriptor,color=get('<III',off)
                refs=[packed&511,(packed>>9)&511,(packed>>18)&511]
                if quad:refs.append(descriptor&511)
                if max(refs)>=nv:raise ValueError('invalid source vertex reference')
                op=color>>24;expected=0x20|(8 if quad else 0)|(4 if textured else 0)|(16 if gouraud else 0)
                if op&0xfc!=expected:raise ValueError('source primitive stream/opcode mismatch')
                uv=[(0,0)]*4;page=clut=0
                if textured:
                    entry=table+((descriptor>>4)&0x7ffe0)
                    page=get('<H',entry+6)[0];clut=get('<H',entry+2)[0]
                    uv=[get('<BB',entry+k) for k in (0,4,8,10)]
                colors=[]
                for j in range(len(refs)):
                    c=u32(off+12+(j-1)*4) if gouraud and j else color
                    colors.append((c&255,(c>>8)&255,(c>>16)&255))
                for corners in (((3,1,0),(1,2,3)) if quad else ((0,1,2),)):
                    pts=tuple(vertices[refs[j]] for j in corners)
                    if np.linalg.norm(np.cross(np.subtract(pts[1],pts[0]),np.subtract(pts[2],pts[0])))<2:continue
                    result.append(Triangle(pts,tuple(uv[j] for j in corners),tuple(colors[j] for j in corners),
                        (1 if textured else 0)|(2 if op&2 else 0)|(4 if op&1 else 0),page,clut,1,owner+1,obj,
                        tuple(vp+refs[j]*8+1 for j in corners),origin,PRIMARY_AXES))
    return result

def source_uploads(data: bytes):
    used=np.zeros((512,1024),dtype=bool);uploads=[]
    for tim in trp_tim_members(data):
        flags=struct.unpack_from('<I',tim,4)[0];pos=8
        for _ in range(2 if flags&8 else 1):
            size,x,y,w,h=struct.unpack_from('<IHHHH',tim,pos)
            payload=tim[pos+12:pos+size]
            if w<=0 or h<=0 or len(payload)!=w*h*2 or x+w>1024 or y+h>512:raise ValueError('invalid TIM upload')
            used[y:y+h,x:x+w]=True;uploads.append((upload_hash(payload,w,h),x,y,w,h));pos+=size
    return used,uploads

def bind_car_texture(vram,used,uploads,texture,paint):
    if len(texture)!=0xb3a0 or not 0<=paint<texture[0]:raise ValueError('invalid native car texture/paint')
    page=None
    for py in (0,256):
        for px in range(0,1024,64):
            if not used[py:py+228,px:px+64].any():page=(px,py);break
        if page:break
    if page is None:raise ValueError('no source-unoccupied diagnostic car texture page')
    x,y=page
    pixels=texture[0x43a0:0xb3a0];palette=texture[0x20+paint*0x240:0x20+paint*0x240+512]
    for payload,w,h,dy in ((pixels,64,224,0),(palette,64,4,224)):
        words=array('H');words.frombytes(payload)
        if sys.byteorder!='little':words.byteswap()
        for row in range(h):vram[(y+dy+row)*1024+x:(y+dy+row)*1024+x+w]=words[row*w:row*w+w]
        uploads.append((upload_hash(payload,w,h),x,y+dy,w,h))
        used[y+dy:y+dy+h,x:x+w]=True
    return (x//64)|((y//256)<<4),[(y+224+i//4)*64+x//16+i%4 for i in range(16)],dict(x=x,y=y,bitmapWidthWords=64,bitmapHeight=224,paletteY=y+224)

def car_triangles(model, page,cluts,offset=(29730,-18,10400),scale=.065,yaw=0,owner=1000001):
    if len(model)<0x8d4 or model[:3]!=b'GT\x02' or struct.unpack_from('<I',model,0x868)[0]!=3:raise ValueError('invalid native car model')
    lod=0x884;nv,nn,nt,nq,_,_,nut,nuq=struct.unpack_from('<8H',model,lod)
    if not 0<nv<=256:raise ValueError('invalid 8-bit car vertex count')
    vertices=[struct.unpack_from('<hhh',model,lod+80+i*8) for i in range(nv)]
    offsets=struct.unpack_from('<7I',model,lod+0x1c)
    a=math.radians(yaw);rotation=np.array([[math.cos(a),0,math.sin(a)],[0,1,0],[-math.sin(a),0,math.cos(a)]])
    axes=rotation@CAR_AXES*scale;result=[]
    for group,count,quad,texture in ((1,nt,False,False),(2,nq,True,False),(3,nut,False,True),(6,nuq,True,True)):
        size=28 if texture else 16
        for i in range(count):
            pos=lod+offsets[group]+i*size;packet=model[pos:pos+size]
            if len(packet)!=size:raise ValueError('truncated car primitive')
            refs=tuple(packet[:4 if quad else 3])
            if max(refs)>=nv:raise ValueError('invalid native car index')
            opcode=packet[15];palette=car_material_palette(struct.unpack_from('<H',packet,18)[0]) if texture else 0
            uv=tuple((packet[k],packet[k+1]) for k in (16,20,24,26)) if texture else ((0,0),)*4
            color=(128,128,128) if texture else tuple(packet[12:15])
            for corners in (((0,1,2),(0,2,3)) if quad else ((0,1,2),)):
                result.append(Triangle(tuple(vertices[refs[j]] for j in corners),tuple(uv[j] for j in corners),(color,)*3,
                    (1 if texture else 0)|(2 if opcode&2 else 0)|(4 if opcode&1 else 0),page,cluts[palette],2,owner,lod,
                    tuple(lod+80+refs[j]*8+1 for j in corners),tuple(offset),axes))
    return result

def camera(eye,target):
    forward=np.subtract(target,eye).astype(float);forward/=np.linalg.norm(forward)
    right=np.cross(forward,[0,-1,0]);right/=np.linalg.norm(right)
    down=np.cross(forward,right);return np.array([right,down,forward])

def capture(triangles,vram,eye,target,width=960,height=540,frame=1,poll=1):
    if len(triangles)>262144:raise ValueError('too many fixture triangles')
    count=len(triangles);offset=160+count*384;output=bytearray(offset+1048576)
    def put(fmt,o,*args):struct.pack_into('<'+fmt,output,o,*args)
    output[:8]=b'OGTWCAP\0';put('IIQi',8,6,160,frame,poll);put('iiIIIII',28,0,0,width,height,count,384,1024);put('IQQQI',56,512,160,offset,1048576,4)
    r=camera(eye,target);camrot=np.rint(r*4096).astype(int).ravel();translation=np.rint(-r@eye).astype(int)
    put('Q9h',88,1,*camrot);put('3i',116,*translation)
    plane=round(height*.88);put('iiIii',128,width//2<<16,height//2<<16,plane,0,0)
    for i,t in enumerate(triangles):
        base=160+i*384
        # Reconstruct through exactly the same 12-bit fixed transform contract
        # that the native draw-list builder consumes; do not bypass its loader.
        m=np.rint(r@t.axes*4096).astype(int)
        trans=np.rint(r@(np.asarray(t.origin)-eye)).astype(int)
        if abs(m).max()>32767:raise ValueError('fixture transform outside signed 16-bit')
        transform_id=t.owner+100
        put('IHHi4h4hIIIIhhQ',base,t.flags,t.page,t.clut,i,0,0,width-1,height-1,0,0,0,0,0,t.kind,t.owner,t.model,0,0,transform_id)
        put('9h',base+56,*m.ravel());put('3i',base+76,*trans)
        for j in range(3):
            v=base+88+j*96;p=np.asarray(t.points[j]);view=m@p/4096+trans
            z=view[2];sx=width/2+plane*view[0]/max(z,1);sy=height/2+plane*view[1]/max(z,1)
            put('3f2h4B3h',v,sx,sy,z,*t.uv[j],*t.rgb[j],1,*p)
            put('3iiiIIQ',v+28,*np.rint(view).astype(int),width//2<<16,height//2<<16,plane,t.identity[j],transform_id)
            put('9h',v+64,*m.ravel());put('3i',v+84,*trans)
        put('iI',base+376,0,1)
    raw=vram.tobytes()
    if sys.byteorder!='little':a=array('H',vram);a.byteswap();raw=a.tobytes()
    output[offset:]=raw;return bytes(output)

class Options(C.Structure):
    _fields_=[(n,C.c_uint32) for n in ('struct_size','flags','output_scale','clear_color_rgba8','target_aspect_width','target_aspect_height','direct_output_slot')]
class Stats(C.Structure):
    _fields_=[(n,C.c_uint32) for n in ('struct_size','result','capture_triangles','output_commands','draw_calls','transparent_draw_calls','topology_boundary_groups','topology_t_junctions','topology_split_triangles','topology_coplanar_pairs','topology_ownership_reorders','output_width','output_height')]+[
        ('render_microseconds',C.c_uint64),('frame_index',C.c_uint64),('input_poll',C.c_int32),('reserved',C.c_uint32)]+[(n,C.c_uint64) for n in ('decode_microseconds','draw_list_microseconds','topology_microseconds','pipeline_microseconds','output_fingerprint','world_fingerprint','output_texture')]
class Upload(C.Structure):
    _fields_=[('key',C.c_uint64)]+[(n,C.c_int32) for n in ('x','y','word_width','height')]
class Renderer:
    def __init__(self,library,uploads):
        if C.sizeof(Stats)!=136 or C.sizeof(Options)!=28:raise RuntimeError('C ABI layout mismatch')
        self.lib=C.CDLL(str(Path(library).resolve()))
        self.lib.opengt_live_create.restype=C.c_void_p
        self.lib.opengt_live_destroy.argtypes=[C.c_void_p]
        self.lib.opengt_live_render.argtypes=[C.c_void_p,C.c_void_p,C.c_size_t,C.c_void_p,C.c_size_t,C.POINTER(Options),C.POINTER(Stats)]
        self.lib.opengt_live_set_texture_uploads.argtypes=[C.c_void_p,C.c_int32,C.POINTER(Upload),C.c_size_t]
        self.handle=self.lib.opengt_live_create()
        if not self.handle:raise RuntimeError('native renderer allocation failed')
        arr=(Upload*len(uploads))(*[Upload(*u) for u in uploads])
        result=self.lib.opengt_live_set_texture_uploads(self.handle,1,arr,len(arr))
        if result:raise RuntimeError(f'upload metadata rejected: {result}')
    def close(self):
        if self.handle:self.lib.opengt_live_destroy(self.handle);self.handle=None
    def render(self,data,width,height,frame):
        options=Options(C.sizeof(Options),1|4|8|16|32,1,0xff17130f,0,0,0)
        output=(C.c_ubyte*(width*height*4))();stats=Stats();stats.struct_size=C.sizeof(Stats)
        inp=C.create_string_buffer(data);start=time.monotonic()
        result=self.lib.opengt_live_render(self.handle,inp,len(data),output,len(output),C.byref(options),C.byref(stats))
        metadata={name:int(getattr(stats,name)) for name,_ in stats._fields_};metadata['wall_seconds']=time.monotonic()-start
        if result or stats.result:raise RuntimeError(f'native live render failed: {result}; {metadata}')
        if stats.reserved&8 or stats.frame_index!=frame or stats.output_width!=width or stats.output_height!=height:raise RuntimeError('wrong output/frame identity')
        return Image.frombytes('RGBA',(width,height),bytes(output)),metadata

def main():
    ap=argparse.ArgumentParser(description=__doc__);ap.add_argument('--volume',type=Path,required=True);ap.add_argument('--library',type=Path,required=True);ap.add_argument('--output',type=Path,required=True)
    ap.add_argument('--track',default='seattle');ap.add_argument('--car',default='n24rn');ap.add_argument('--paint',type=int,default=5);ap.add_argument('--frames',type=int,default=1);ap.add_argument('--width',type=int,default=960);ap.add_argument('--height',type=int,default=540);ap.add_argument('--car-x-step',type=float,default=0);ap.add_argument('--yaw-step',type=float,default=0);ap.add_argument('--save-capture',action='store_true')
    ap.add_argument('--eye',nargs=3,type=float,default=[30130,-310,9730]);ap.add_argument('--target',nargs=3,type=float,default=[29730,-75,10400])
    ap.add_argument('--car-position',nargs=3,type=float,default=[29730,-18,10400]);ap.add_argument('--car-yaw',type=float,default=0)
    ap.add_argument('--second-paint',type=int);ap.add_argument('--second-position',nargs=3,type=float,default=[30020,-18,10720]);ap.add_argument('--second-yaw',type=float,default=0)
    a=ap.parse_args()
    if not 1<=a.frames<=120 or not 64<=a.width<=2048 or not 64<=a.height<=2048:ap.error('invalid bounded proof dimensions/frame count')
    a.output.mkdir(parents=True,exist_ok=True);volume=Volume(a.volume)
    members={name:volume.read(name) for name in (f'crsobj/{a.track}.tro.gz',f'crsobj/{a.track}.trp.gz',f'carobj/{a.car}.cdo.gz',f'carobj/{a.car}.cdp.gz')}
    tro,trp,car,tex=members.values();vram,_=package(trp);used,uploads=source_uploads(trp);page,cluts,binding=bind_car_texture(vram,used,uploads,tex,a.paint)
    primary=primary_triangles(tro);eye=np.array(a.eye,dtype=float);target=np.array(a.target,dtype=float);records=[]
    second=[];second_binding=None
    if a.second_paint is not None:
        page2,cluts2,second_binding=bind_car_texture(vram,used,uploads,tex,a.second_paint)
        second=car_triangles(car,page2,cluts2,a.second_position,yaw=a.second_yaw,owner=1000002)
    if not np.isfinite([*eye,*target,*a.car_position,a.car_yaw,a.yaw_step,a.car_x_step,*a.second_position,a.second_yaw]).all():ap.error('nonfinite camera/car setting')
    if np.linalg.norm(target-eye)<.001 or np.linalg.norm(np.cross(target-eye,[0,-1,0]))<.001:ap.error('degenerate inspection camera')
    # All scene choices are stated here. The track selector remains the real
    # source-upload registry; no pixel hash or forced guessed guest address.
    renderer=Renderer(a.library,uploads)
    try:
        for frame in range(1,a.frames+1):
            body=car_triangles(car,page,cluts,(a.car_position[0]+(frame-1)*a.car_x_step,*a.car_position[1:]),yaw=a.car_yaw+(frame-1)*a.yaw_step)+second
            triangles=primary+body;data=capture(triangles,vram,eye,target,a.width,a.height,frame,frame)
            image,meta=renderer.render(data,a.width,a.height,frame);out=a.output/f'frame-{frame:03d}.png';image.save(out)
            meta.update(frame=frame,kind='staged source assets through live C ABI; NOT gameplay',capture_sha256=sha(data),png_sha256=sha(out.read_bytes()),car_triangles=len(body),primary_triangles=len(primary))
            records.append(meta);(a.output/f'frame-{frame:03d}.json').write_text(json.dumps(meta,indent=2)+'\n')
            if a.save_capture and frame==1:(a.output/'staged-v6.wcap').write_bytes(data)
            print(json.dumps(meta),flush=True)
    finally:renderer.close()
    manifest=dict(kind='staged original-asset inspection; not gameplay',members={n:sha(d) for n,d in members.items()},camera_eye=eye.tolist(),camera_target=target.tolist(),texture_binding=binding,uploads=uploads,
        original_volume_open_mode="read-only",car=a.car,paint=a.paint,car_position=a.car_position,car_yaw=a.car_yaw,car_x_step=a.car_x_step,yaw_step=a.yaw_step,second_paint=a.second_paint,second_position=a.second_position,second_yaw=a.second_yaw,second_binding=second_binding,frames=records)
    (a.output/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
if __name__=='__main__':main()
