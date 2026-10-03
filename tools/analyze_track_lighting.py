#!/usr/bin/env python3
"""Build auditable OpenGT lighting profiles from the user's own GT2.VOL.

No downloads, VRAM screenshots, fixed guest addresses or preselected sun angles.
Source contracts: gt2_vol.py, build_gt2_texture_pack.py, GpuRawTrack.cs,
WorldCaptureContext.cs, and generated func_80026B40/func_80027BA8.
Only revision-31 PRIMARY course meshes are decoded here. Auxiliary props and
cutout surfaces are not assumed opaque. Weak/ambiguous inverse fits stay OFF.
"""
from __future__ import annotations
import argparse
from array import array
from collections import defaultdict
import gzip
import hashlib
import json
import math
from pathlib import Path
import statistics
import struct
import subprocess
import sys
from gt2_vol import read_entries
from build_gt2_texture_pack import trp_tim_members, tim_payload_size, upload_hash

POOL_SIZES=(12,12,20,24,12,12,20,24)
CANONICAL=('autumn','mini','circuit','short','mountain','test_in2','testline',
    'speed','maxspeed','sprint2','seattle','seatt_s','roma','roma_short',
    'laguna','grindel','new_parmaS','parma','s_speed','cart','pikes','pikes_rev',
    'nn_dirt_2','nn_dirt_3','no_name_dirt','tahiti_d_new','tahiti_t')

def dot(a,b): return sum(x*y for x,y in zip(a,b))
def sub(a,b): return tuple(x-y for x,y in zip(a,b))
def cross(a,b):return (a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0])
def normalized(a):
    length=math.sqrt(dot(a,a))
    return tuple(x/length for x in a) if length>1e-20 else (0.,0.,0.)
def mean(values):return tuple(sum(v[j] for v in values)/len(values) for j in range(3))
def sha(data):return hashlib.sha256(data).hexdigest()
def linear(x):return x/12.92 if x<=.04045 else ((x+.055)/1.055)**2.4

def track_id(name):
    value=0
    for ch in name.encode('ascii'):value=((((value<<6)&0xffffffff)|(value>>26))+ch)&0xffffffff
    return value

class Volume:
    def __init__(self,path):
        self.path=Path(path);self.entries={e.name:e for e in read_entries(self.path)}
    def read(self,name):
        e=self.entries[name]
        with self.path.open('rb') as stream:
            stream.seek(e.offset);data=stream.read(e.size)
        if len(data)!=e.size:raise ValueError('truncated volume member: '+name)
        if name.endswith('.gz'):
            data=gzip.decompress(data)
            if len(data)>32*1024*1024:raise ValueError('oversized decompressed member')
        return data
    def courses(self):
        b=self.read('.crsinfo')
        if b[:4]!=b'CRS\0':raise ValueError('invalid .crsinfo magic')
        version,count=struct.unpack_from('<HH',b,4)
        if version!=2 or 8+24*count>len(b):raise ValueError('unsupported .crsinfo')
        stems=[Path(n).name[:-7] for n in self.entries if n.startswith('crsobj/') and n.endswith('.tro.gz')]
        skies=[Path(n).name[:-7] for n in self.entries if n.startswith('bgsobj/') and n.endswith('.bso.gz')]
        if len(stems)!=count:raise ValueError('course directory/table mismatch')
        rows=[]
        for i,stem in enumerate(stems):
            record=struct.unpack_from('<IIBBH6H',b,8+24*i)
            if record[1]!=track_id(stem) or record[4]>=len(skies) or not 8+24*count<=record[0]<len(b):raise ValueError('invalid course record')
            end=b.index(0,record[0]);name=b[record[0]:end].decode('ascii')
            # Bit zero consistently marks the explicitly named SSR5 and Rome
            # night records in this supplied asset set. Preserve raw flags in
            # report rather than assigning a real-world time of day.
            rows.append(dict(stem=stem,name=name,flags=record[2],sky=skies[record[4]],night=bool(record[2]&1)))
        return rows

def package(data):
    """Apply original TIM uploads, including palettes, in their authored order."""
    vram=array('H',[0])*(1024*512);keys=[]
    for tim in trp_tim_members(data):
        flags=struct.unpack_from('<I',tim,4)[0];cursor=8
        blocks=2 if flags&8 else 1
        for block in range(blocks):
            size,x,y,w,h=struct.unpack_from('<IHHHH',tim,cursor)
            payload=tim[cursor+12:cursor+size]
            if len(payload)!=w*h*2 or x+w>1024 or y+h>512:raise ValueError('invalid TIM upload rectangle')
            words=array('H');words.frombytes(payload)
            if sys.byteorder!='little':words.byteswap()
            for row in range(h):vram[(y+row)*1024+x:(y+row)*1024+x+w]=words[row*w:(row+1)*w]
            # Bitmap keys match the completed-upload FNV64 contract. Palette
            # uploads change with GT2 lighting and are NOT used to select track.
            if block==blocks-1:keys.append(upload_hash(payload,w,h))
            cursor+=size
        if cursor!=len(tim):raise ValueError('TIM trailing bytes')
    return vram,sorted(set(keys))

def word(vram,page,clut,u,v):
    u=int(math.floor(u+0.0001))&255;v=int(math.floor(v+0.0001))&255
    x=(page&15)*64;y=((page>>4)&1)*256;mode=(page>>7)&3
    if mode==0:
        packed=vram[((y+v)&511)*1024+((x+(u>>2))&1023)];index=(packed>>((u&3)*4))&15
    elif mode==1:
        packed=vram[((y+v)&511)*1024+((x+(u>>1))&1023)];index=(packed>>((u&1)*8))&255
    elif mode==2:return vram[((y+v)&511)*1024+((x+u)&1023)]
    else:return 0
    return vram[((clut>>6)&511)*1024+(((clut&63)*16+index)&1023)]

def geometry(data,vram):
    if len(data)<0x44 or struct.unpack_from('<H',data,14)[0]!=31:raise ValueError('only revision-31 TRO supported')
    def u16(o):
        if not 0<=o<=len(data)-2:raise ValueError('u16 outside TRO')
        return struct.unpack_from('<H',data,o)[0]
    def u32(o):
        if not 0<=o<=len(data)-4:raise ValueError('u32 outside TRO')
        return struct.unpack_from('<I',data,o)[0]
    primary=u32(16);count=u16(primary+4);table=u32(primary+8)
    if not 0<count<8192:raise ValueError('invalid primary object count')
    triangles=[];seen=set();samples=[];groups=defaultdict(list);opcode_count=defaultdict(int)
    for object_id in range(count):
        obj=u32(primary+12+4*object_id)
        centers=struct.unpack_from('<iii',data,obj+0x30)
        # Exact inverse of func_80026B40: mask source sector XYZ to 0xFFC00000,
        # shift by ten, and pass (X,Z,Y) to ExecuteTrackTranslation.
        shifts=tuple((centers[j]>>22)*4096 for j in (0,2,1))
        # These are mutually exclusive variants, not two geometric layers.
        for mesh in (obj+0xa4,):
            vp=u32(mesh);nv=u32(mesh+0x2c)+1
            if not 0<nv<=512 or vp+nv*8>len(data):raise ValueError('invalid 9-bit primary vertex table')
            local=[struct.unpack_from('<hhh',data,vp+i*8) for i in range(nv)]
            raw_vertices=[tuple(a+b for a,b in zip(p,shifts)) for p in local]
            vertices=[(p[0],-p[2],p[1]) for p in raw_vertices] # XY ground / Z height -> canonical +Y down
            for stream,size in enumerate(POOL_SIZES):
                ptr=u32(mesh+4+4*stream);n=u16(mesh+0x30+2*stream)
                if ptr+n*size>len(data):raise ValueError('primitive stream outside TRO')
                textured=stream>=4;gouraud=bool(stream&2);quad=bool(stream&1)
                for item in range(n):
                    offset=ptr+size*item;packed,descriptor,color=struct.unpack_from('<III',data,offset)
                    ids=[packed&511,(packed>>9)&511,(packed>>18)&511]
                    if quad:ids.append(descriptor&511)
                    if max(ids)>=nv:raise ValueError('invalid primary vertex index')
                    opcode=color>>24;expected=0x20|(8 if quad else 0)|(4 if textured else 0)|(16 if gouraud else 0)
                    if opcode&0xfc!=expected:raise ValueError('primitive opcode/stream mismatch')
                    opcode_count[opcode]+=1
                    if opcode&2:continue
                    page=clut=entry=0;uv=[(0,0)]*4
                    if textured:
                        entry=table+((descriptor>>4)&0x0007ffe0)
                        if entry+32>len(data):raise ValueError('texture descriptor outside TRO')
                        page=u16(entry+6);clut=u16(entry+2)
                        uv=[(data[entry+k],data[entry+k+1]) for k in (0,4,8,10)]
                    rgb=[(color&255,(color>>8)&255,(color>>16)&255)]
                    for j in range(1,len(ids)):
                        c=u32(offset+12+4*(j-1)) if gouraud else color
                        rgb.append((c&255,(c>>8)&255,(c>>16)&255))
                    for corners in (((3,1,0),(1,2,3)) if quad else ((0,1,2),)):
                        points=tuple(vertices[ids[j]] for j in corners);key=tuple(sorted(points))
                        if key in seen:continue
                        seen.add(key)
                        vector=cross(sub(points[1],points[0]),sub(points[2],points[0]));area=math.sqrt(dot(vector,vector))*.5
                        if area<1:continue
                        normal=normalized(vector)
                        colors=[rgb[j] for j in corners];tuv=[uv[j] for j in corners]
                        opacity=1.0
                        if textured:
                            tests=[mean([(p[0],p[1],0) for p in tuv])[:2],*tuv]
                            # Cutouts are conservatively omitted from inverse
                            # fit; live shadow rasterizer does sample alpha.
                            opacity=sum(word(vram,page,clut,*p)!=0 for p in tests)/len(tests)
                        if opacity==1:triangles.append(points)
                        if textured and opacity==1 and abs(normal[1])>.96 and len(set(colors))==1 and area>20:
                            sample=dict(position=mean(points),normal=tuple(-x for x in normal) if normal[1]>0 else normal,
                                color=colors[0],area=area,object=object_id,recordOffset=offset,textureDescriptor=entry,
                                page=page,clut=clut,points=points,local=tuple(local[ids[j]] for j in corners),uv=tuv)
                            groups[(page,clut,tuple(sorted(tuv)))].append(sample)
    # Match only the SAME source UV tile + palette + planar class. This is a
    # candidate photometric label, never enough on its own to erase a shadow.
    color_deltas=[];discarded=0
    for key,group in groups.items():
        if len(group)<8:continue
        ordered=sorted(group,key=lambda s:sum(s['color']));lit=ordered[max(0,int(len(ordered)*.9)-1)]['color'];lit_luma=sum(lit)
        if lit_luma<96:continue
        dark=[s for s in group if .25<sum(s['color'])/lit_luma<.74]
        bright=[s for s in group if .92<sum(s['color'])/lit_luma<1.08]
        if len(dark)<3 or len(bright)<3:continue
        # Different material tints must not masquerade as a shadow factor.
        accepted_dark=[]
        for s in dark:
            ratios=[a/max(b,1) for a,b in zip(s['color'],lit)]
            if max(ratios)-min(ratios)>.22:discarded+=1;continue
            accepted_dark.append(s)
        if len(accepted_dark)<3:continue
        med=tuple(statistics.median(s['color'][i] for s in accepted_dark) for i in range(3))
        color_deltas.append(tuple(max(0,linear(a/255)-linear(b/255)) for a,b in zip(lit,med)))
        for label,subset in ((1,accepted_dark),(0,bright)):
            # Keep each material/class balanced and bound per-region sampling.
            for s in subset[::max(1,len(subset)//80)]:
                s=dict(s,label=label,litColor=lit,weight=1.0/max(1,len(subset)))
                samples.append(s)
    # Bound work reproducibly without random seeds or cherry-picking a fit.
    samples=sorted(samples,key=lambda s:(s['object'],s['recordOffset'],s['label']))
    if len(samples)>1800:samples=samples[::math.ceil(len(samples)/1800)]
    tint=mean(color_deltas) if color_deltas else (1.,1.,1.)
    maximum=max(tint)
    tint=tuple(x/maximum for x in tint) if maximum>1e-10 else (1.,1.,1.)
    return triangles,samples,dict(primaryObjects=count,opaqueCasterTriangles=len(triangles),photometricSamples=len(samples),
        materialGroupsWithContrast=len(color_deltas),rejectedTintCandidates=discarded,opcodes={str(k):v for k,v in opcode_count.items()},sunColorEstimate=tint)

def write_evidence(path,triangles,samples):
    with path.open('wb') as out:
        out.write(b'OGTSUN1\0'+struct.pack('<II',len(triangles),len(samples)))
        for tri in triangles:out.write(struct.pack('<9f',*(x for p in tri for x in p)))
        for s in samples:out.write(struct.pack('<6fIIf',*s['position'],*s['normal'],s['label'],s['object'],s['weight']))

def sky_tint(data):
    # A restrained environment tint, not a claim to have recovered radiance
    # or sun intensity. Use median visible source sky texels in linear space.
    from build_gt2_texture_pack import decode_tim_asset
    pixels=[]
    for tim in trp_tim_members(data):
        try:_,im,_=decode_tim_asset(tim)
        except ValueError:continue
        rgba=list(im.get_flattened_data() if hasattr(im,"get_flattened_data") else im.getdata());step=max(1,len(rgba)//2048)
        pixels.extend(tuple(linear(c/255) for c in pixel[:3]) for pixel in rgba[::step] if pixel[3] and max(pixel[:3])>24)
    if not pixels:return (.38,.48,.62)
    med=tuple(statistics.median(p[i] for p in pixels) for i in range(3))
    # Keep original tint but avoid importing black silhouetted mountains as a
    # zero-radiance sky. Absolute exposure remains deliberately art-directed.
    peak=max(med)
    return tuple(.08+.5*x/max(peak,.02) for x in med)

DEFAULTS='''// OpenGT L01: reloads once per second. Remove this file for stock rendering.
// Colors are linear RGB. Course coordinates use +Y down.
settings { enabled 1 shadows 1 shadowResolution 1024 minimumSunConfidence 0.75 normalCrease 55 shadowBias 0.001 }
material car/paint { surface car roughness 0.34 specular 0.22 reflection 0.12 clearcoat 0.24 clearcoatRoughness 0.16 diffuseMix 0.12 }
material car/rubber { surface rubber roughness 0.92 specular 0.015 reflection 0 clearcoat 0 diffuseMix 0.10 }
material road/dry { surface road roughness 0.84 specular 0.07 reflection 0.025 clearcoat 0 diffuseMix 0.14 }
material track/default { surface track roughness 0.88 specular 0.025 reflection 0 clearcoat 0 diffuseMix 0.10 }
// Deliberately NOT selected automatically: a renderer test profile, not an
// inferred track sun. Add `forceTrack diagnostic` inside settings to exercise
// the directional shadow pass independently of calibration.
track diagnostic { sunDirection 0.4 -0.8 0.3 sunColor 1 0.93 0.83 skyColor 0.36 0.46 0.60 groundColor 0.17 0.16 0.15 sunIntensity 0.55 sunConfidence 1 ambient 0.84 shadowStrength 0.35 }
'''

def main():
    ap=argparse.ArgumentParser(description=__doc__);ap.add_argument('--volume',type=Path,required=True);ap.add_argument('--solver',type=Path,required=True);ap.add_argument('--output',type=Path,required=True);ap.add_argument('--courses',nargs='*',default=list(CANONICAL));ap.add_argument('--keep-evidence',action='store_true');args=ap.parse_args()
    vol=Volume(args.volume);out=args.output;out.mkdir(parents=True,exist_ok=True);courses=vol.courses();inventory=[];cache={};estimates={}
    for row in courses:
        stem=row['stem'];textures=vol.read(f'crsobj/{stem}.trp.gz');key=sha(textures)
        if key not in cache:cache[key]=package(textures)
        vram,keys=cache[key];row=dict(row,textureSha256=key,uploadKeys=[f'{x:016x}' for x in keys]);inventory.append(row)
        if stem not in args.courses or row['night']:continue
        try:
            data=vol.read(f'crsobj/{stem}.tro.gz');tris,samples,stats=geometry(data,vram)
            evidence=out/f'{stem}.solar.bin';write_evidence(evidence,tris,samples)
            process=subprocess.run([str(args.solver.resolve()),str(evidence.resolve())],capture_output=True,text=True,timeout=240,check=True)
            result=json.loads(process.stdout);result.update(stats);result.update(geometrySha256=sha(data),evidenceSha256=sha(evidence.read_bytes()),textureSha256=key)
            estimates[stem]=result
            if not args.keep_evidence:evidence.unlink()
            print(stem,json.dumps({k:result.get(k) for k in ('accepted','score','angularAmbiguityDegrees','photometricSamples','reason')}),flush=True)
        except (ValueError,KeyError,subprocess.SubprocessError,struct.error) as error:
            estimates[stem]=dict(accepted=False,reason=str(error));print(stem,'REJECT',error,flush=True)
    # Equivalent bitmap sets may be loaded by reverse and 2-player variants.
    # Group them explicitly instead of assigning two ambiguous runtime entries.
    groups=defaultdict(list)
    for row in inventory:groups[(tuple(row['uploadKeys']),row['sky'],row['night'])].append(row)
    script=DEFAULTS;profile_report=[];sky_cache={}
    for (_,sky,night),rows in groups.items():
        name=next((x['stem'] for x in rows if x['stem'] in CANONICAL),rows[0]['stem']);keys=rows[0]['uploadKeys'];accepted=[(r['stem'],estimates[r['stem']]) for r in rows if estimates.get(r['stem'],{}).get('accepted')]
        # Never inherit a calibrated direction across a different course file:
        # alias mapping is useful for environment only until each is audited.
        result=accepted[0][1] if len(rows)==1 and len(accepted)==1 else None
        try:
            if sky not in sky_cache:sky_cache[sky]=sky_tint(vol.read(f'bgsobj/{sky}.bsp.gz'))
            tint=sky_cache[sky]
        except (ValueError,KeyError,struct.error):tint=(.38,.48,.62)
        script+=f'\n// Source courses: {", ".join(r["stem"] for r in rows)}\ntrack {name} {{\n'
        for key in keys:script+=f'    upload 0x{key}\n'
        script+=f'    night {int(night)}\n    skyColor '+ ' '.join(f'{x:.6f}' for x in tint)+'\n    ambient 0.88\n'
        if result:
            script+='    sunDirection '+' '.join(f'{x:.7f}' for x in result['direction'])+'\n'
            script+='    sunColor '+' '.join(f'{x:.6f}' for x in result['sunColorEstimate'])+'\n'
            script+=f'    sunIntensity 0.50\n    sunConfidence {result["confidence"]:.6f}\n'
        else:script+='    sunIntensity 0\n    sunConfidence 0\n'
        script+='}\n';profile_report.append(dict(profile=name,aliases=[r['stem'] for r in rows],night=night,solarEnabled=result is not None,
            reason='accepted inverse fit' if result else 'night excluded' if night else 'uncalibrated or shared bitmap identity; preserve baked diffuse',skyTint=tint))
    (out/'lighting.shader').write_text(script,encoding='utf-8')
    report=dict(format='OpenGT-lighting-audit-L01',volumeBytes=args.volume.stat().st_size,source='user-supplied GT2.VOL',
        method='primary source geometry ray-occlusion vs matched same-UV/palette baked vertex-color regions; deterministic two-fold validation',
        limitations=['Primary source meshes only; auxiliary models not reconstructed by this analyzer.',
            'Offline alpha rejection samples triangle corners/centroid, not every covered texel; missed cutouts remain possible. Live D3D uses original keyed alpha.',
            'Confidence is an engineering acceptance score, not a calibrated probability.',
            'Shared bitmap aliases remain solar-disabled until all source environments are verified.',
            'No dark geometry is automatically deleted; no unbake rule is inferred from color alone.'],
        courses=inventory,estimates=estimates,profiles=profile_report)
    (out/'track-lighting-audit.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    print('Wrote',out/'lighting.shader', 'profiles=',len(profile_report),'accepted fits=',sum(r.get('accepted',False) for r in estimates.values()),flush=True)
    return 0
if __name__=='__main__':raise SystemExit(main())
