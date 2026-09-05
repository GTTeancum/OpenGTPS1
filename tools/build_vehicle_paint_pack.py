#!/usr/bin/env python3
"""Build an opt-in material mask/normal pack from the user's own GT2 car data.

No proprietary model, texture or normal data is checked into the repository.
The source image is unchanged; mask red=specular strength, green=gloss,
alpha=explicit paint ownership. Alpha zero retains original glass/trim.
"""
from __future__ import annotations
import argparse
import gzip
import itertools
import json
import math
import struct
from pathlib import Path
from PIL import Image, ImageDraw
from car_livery_viewer import read_member, decode_texture_atlas, png_rgba

ROOT=Path(__file__).resolve().parents[1]

def bitmap_key(data):
    value=14695981039346656037
    for byte in data:
        value=((value^byte)*1099511628211)&0xffffffffffffffff
    return value

def make_mask(description):
    if description['format']!=1: raise ValueError('unsupported mask description')
    mask=Image.new('RGBA',(256,224),(0,0,0,0))
    draw=ImageDraw.Draw(mask)
    for region in description['regions']:
        points=[tuple(p) for p in region['polygon']]
        if len(points)<3 or any(not (0<=x<256 and 0<=y<224) for x,y in points):
            raise ValueError('paint region outside source atlas')
        strength,gloss=region['strength'],region['gloss']
        if not (0<=strength<=255 and 0<=gloss<=255): raise ValueError('invalid material value')
        draw.polygon(points,fill=(strength,gloss,0,255))
    return mask

def decode_faces(model):
    if model[:3]!=b'GT\x02': raise ValueError('not a GT2 car model')
    lod=0x884
    counts=struct.unpack_from('<8H',model,lod)
    offsets=struct.unpack_from('<7I',model,lod+0x1c)
    positions=[struct.unpack_from('<3h',model,lod+0x50+i*8) for i in range(counts[0])]
    normals=[]
    for i in range(counts[1]):
        packed=struct.unpack_from('<I',model,lod+offsets[0]+i*4)[0]
        values=[((packed>>shift)&1023) for shift in (2,12,22)]
        values=[x-1024 if x>=512 else x for x in values]
        length=math.sqrt(sum(x*x for x in values))
        normals.append(tuple(round(x/length*4096) for x in values) if length else (0,0,0))
    faces={}
    ambiguous=set()
    for count,offset,quad in [(counts[6],offsets[3],False),(counts[7],offsets[6],True)]:
        for i in range(count):
            packet=model[lod+offset+i*28:lod+offset+(i+1)*28]
            n0=struct.unpack_from('<H',packet,4)[0]>>5
            n123=struct.unpack_from('<I',packet,8)[0]
            refs=(n0&511,(n123>>1)&511,(n123>>10)&511,(n123>>19)&511)
            uv=[tuple(packet[j:j+2]) for j in (16,20,24,26)]
            corners=[positions[packet[j]]+normals[refs[j]]+uv[j] for j in range(4 if quad else 3)]
            # GT2 emits both packet layouts. Match exact source corners while
            # accepting either quad diagonal and either triangle winding.
            for indices in itertools.combinations(range(len(corners)),3):
                triangle=tuple(corners[j] for j in indices)
                key=tuple(sorted(c[:3] for c in triangle))
                if len(set(key))!=3:
                    continue
                ordered=tuple(sorted(triangle,key=lambda c:c[:3]))
                if key in faces and faces[key]!=ordered:
                    ambiguous.add(key)
                faces[key]=ordered
    # Coincident faces can intentionally carry different trim materials.
    # Without unique authored ownership leave those faces completely original.
    for key in ambiguous:
        del faces[key]
    return list(faces.values())

def build_pack(model,texture,description,night_model=None,night_texture=None):
    if len(texture)!=0xb3a0: raise ValueError('unexpected GT2 car texture size')
    mask=make_mask(description)
    faces=decode_faces(model)
    alternate=0
    if night_model is not None:
        if night_texture is None or len(night_texture)!=0xb3a0:
            raise ValueError('unexpected night texture size')
        # A shared mask is safe only with identical authored positions, UVs and
        # normals, not merely a related model name or similar-looking bitmap.
        if set(faces)!=set(decode_faces(night_model)):
            raise ValueError('night model needs a separately authored material pack')
        alternate=bitmap_key(night_texture[0x43a0:0xb3a0])
    key=bitmap_key(texture[0x43a0:0xb3a0])
    payload=bytearray(struct.pack('<8s4IQQ',b'OGTPAINT',2,256,224,len(faces),key,alternate))
    for face in faces:
        for corner in face: payload.extend(struct.pack('<8h',*corner))
    payload.extend(mask.tobytes())
    return bytes(payload),mask

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--volume',type=Path,default=ROOT/'work/arcade-disc/GT2.VOL')
    parser.add_argument('--mask',type=Path,default=ROOT/'mods/gt2000_paint/masks/fcx8n.json')
    parser.add_argument('--output',type=Path,default=ROOT/'mods/gt2000_paint/generated/fcx8n.ogtpaint')
    parser.add_argument('--evidence',type=Path,default=ROOT/'artifacts/paint-specular')
    args=parser.parse_args()
    description=json.loads(args.mask.read_text())
    stem=description['car']
    if not stem.isalnum() or len(stem)!=5: raise ValueError('invalid car stem')
    model=gzip.decompress(read_member(args.volume,f'carobj/{stem}.cdo.gz'))
    texture=gzip.decompress(read_member(args.volume,f'carobj/{stem}.cdp.gz'))
    night_model=gzip.decompress(read_member(args.volume,f'carobj/{stem}.cno.gz'))
    night_texture=gzip.decompress(read_member(args.volume,f'carobj/{stem}.cnp.gz'))
    pack,mask=build_pack(model,texture,description,night_model,night_texture)
    args.output.parent.mkdir(parents=True,exist_ok=True)
    args.output.write_bytes(pack)
    args.evidence.mkdir(parents=True,exist_ok=True)
    mask.save(args.evidence/'paint-mask.png')
    rgba,_=decode_texture_atlas(texture,0)
    (args.evidence/'source-atlas.png').write_bytes(png_rgba(256,224,rgba[:256*224*4]))
    print(json.dumps({'path':str(args.output),'bytes':len(pack),'faces':len(decode_faces(model)),
        'bitmap':f'{bitmap_key(texture[0x43a0:]):016x}',
        'nightBitmap':f'{bitmap_key(night_texture[0x43a0:]):016x}',
        'paintTexels':sum(a>0 for a in mask.getchannel('A').tobytes())},indent=2))

if __name__=='__main__': main()
