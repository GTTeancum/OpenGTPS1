#!/usr/bin/env python3
"""Build a standalone WebGL viewer for native GT2 car/livery assets."""

from __future__ import annotations

import argparse
import base64
import gzip
import hashlib
import json
import math
import struct
import zlib
from pathlib import Path

from gt1_convert import (
    _parse_gt2_carinfo,
    decode_gt2_car_id,
    encode_gt2_car_id,
)
from gt2_vol import read_entries


REPO = Path(__file__).resolve().parents[1]


def read_member(volume: Path, name: str) -> bytes:
    matches = [entry for entry in read_entries(volume) if entry.name == name]
    if len(matches) != 1:
        raise ValueError(
            f"{volume} contains {len(matches)} members named {name!r}"
        )
    entry = matches[0]
    with volume.open("rb") as stream:
        stream.seek(entry.offset)
        return stream.read(entry.size)


def png_rgba(width: int, height: int, rgba: bytes) -> bytes:
    if len(rgba) != width * height * 4:
        raise ValueError("PNG input dimensions do not match its RGBA payload")
    raw = b"".join(
        b"\0" + rgba[y * width * 4 : (y + 1) * width * 4]
        for y in range(height)
    )

    def chunk(kind: bytes, payload: bytes) -> bytes:
        return (
            struct.pack(">I", len(payload))
            + kind
            + payload
            + struct.pack(">I", zlib.crc32(kind + payload) & 0xFFFFFFFF)
        )

    return (
        b"\x89PNG\r\n\x1a\n"
        + chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0))
        + chunk(b"IDAT", zlib.compress(raw, 9))
        + chunk(b"IEND", b"")
    )


def psx_color(value: int, transparent: bool) -> tuple[int, int, int, int]:
    if transparent:
        return (0, 0, 0, 0)
    return (
        ((value >> 0) & 31) * 255 // 31,
        ((value >> 5) & 31) * 255 // 31,
        ((value >> 10) & 31) * 255 // 31,
        255,
    )


def decode_texture_atlas(
    texture: bytes, paint_index: int
) -> tuple[bytes, bytes]:
    if len(texture) != 0xB3A0:
        raise ValueError(
            f"unsupported native car texture size: {len(texture):#x}"
        )
    paint_count = texture[0]
    if not 0 <= paint_index < paint_count:
        raise ValueError(
            f"paint index {paint_index} is outside 0..{paint_count - 1}"
        )
    pixels = texture[0x43A0:0xB3A0]
    if len(pixels) != 256 * 224 // 2:
        raise ValueError("native car indexed bitmap is truncated")

    output = bytearray(256 * 224 * 16 * 4)
    clut_base = 0x20 + paint_index * 0x240
    for subpalette in range(16):
        clut = struct.unpack_from("<16H", texture, clut_base + subpalette * 32)
        destination_row = subpalette * 224
        for y in range(224):
            for x in range(256):
                packed = pixels[y * 128 + x // 2]
                index = (packed >> 4) if x & 1 else (packed & 15)
                rgba = psx_color(clut[index], index == 0)
                offset = ((destination_row + y) * 256 + x) * 4
                output[offset : offset + 4] = bytes(rgba)
    return bytes(output), pixels


def material_palette(encoded: int) -> int:
    return ((encoded >> 4) & 0x0C) | (encoded & 0x03)


def face_normal(
    a: tuple[int, int, int],
    b: tuple[int, int, int],
    c: tuple[int, int, int],
) -> tuple[float, float, float]:
    ab = (b[0] - a[0], b[1] - a[1], b[2] - a[2])
    ac = (c[0] - a[0], c[1] - a[1], c[2] - a[2])
    normal = (
        ab[1] * ac[2] - ab[2] * ac[1],
        ab[2] * ac[0] - ab[0] * ac[2],
        ab[0] * ac[1] - ab[1] * ac[0],
    )
    length = math.sqrt(sum(value * value for value in normal)) or 1.0
    return tuple(value / length for value in normal)


def decode_model(model: bytes) -> dict[str, object]:
    if len(model) < 0x8D4 or model[:3] != b"GT\x02":
        raise ValueError("native GT2 car model header is missing")
    if struct.unpack_from("<I", model, 0x868)[0] != 3:
        raise ValueError("native GT2 car model does not contain three LODs")

    lod = 0x884
    counts = struct.unpack_from("<8H", model, lod)
    vertex_count, normal_count, tri_count, quad_count = counts[:4]
    uv_tri_count, uv_quad_count = counts[6:8]
    vertices = [
        struct.unpack_from("<3h", model, lod + 0x50 + index * 8)
        for index in range(vertex_count)
    ]
    group_offsets = struct.unpack_from("<7I", model, lod + 0x1C)
    triangle_offset = lod + group_offsets[1]
    quad_offset = lod + group_offsets[2]
    uv_triangle_offset = lod + group_offsets[3]
    uv_quad_offset = lod + group_offsets[6]

    positions: list[float] = []
    normals: list[float] = []
    uvs: list[float] = []
    colors: list[float] = []
    textured: list[float] = []

    def append_triangle(
        refs: tuple[int, int, int],
        face_uvs: tuple[tuple[int, int], ...] | None,
        palette: int,
        color: tuple[int, int, int],
    ) -> None:
        points = tuple(vertices[index] for index in refs)
        normal = face_normal(*points)
        for corner, point in enumerate(points):
            positions.extend(float(value) for value in point)
            normals.extend(normal)
            colors.extend(channel / 255.0 for channel in color)
            if face_uvs is None:
                uvs.extend((0.0, 0.0))
                textured.append(0.0)
            else:
                u, v = face_uvs[corner]
                uvs.extend(((u + 0.5) / 256.0, (palette * 224 + v + 0.5) / 3584.0))
                textured.append(1.0)

    def decode_polygon(offset: int, is_quad: bool, has_texture: bool) -> None:
        packet = model[offset : offset + (28 if has_texture else 16)]
        refs = tuple(packet[: 4 if is_quad else 3])
        if any(index >= len(vertices) for index in refs):
            raise ValueError("native car polygon has an invalid vertex")
        face_uvs = None
        palette = 0
        color = (packet[12], packet[13], packet[14])
        if has_texture:
            palette = material_palette(struct.unpack_from("<H", packet, 18)[0])
            all_uvs = (
                (packet[16], packet[17]),
                (packet[20], packet[21]),
                (packet[24], packet[25]),
                (packet[26], packet[27]),
            )
            face_uvs = all_uvs[: 4 if is_quad else 3]
            color = (255, 255, 255)
        if is_quad:
            append_triangle(
                (refs[0], refs[1], refs[2]),
                (
                    (face_uvs[0], face_uvs[1], face_uvs[2])
                    if face_uvs is not None
                    else None
                ),
                palette,
                color,
            )
            append_triangle(
                (refs[0], refs[2], refs[3]),
                (
                    (face_uvs[0], face_uvs[2], face_uvs[3])
                    if face_uvs is not None
                    else None
                ),
                palette,
                color,
            )
        else:
            append_triangle(refs, face_uvs, palette, color)

    for index in range(tri_count):
        decode_polygon(triangle_offset + index * 16, False, False)
    for index in range(quad_count):
        decode_polygon(quad_offset + index * 16, True, False)
    for index in range(uv_tri_count):
        decode_polygon(uv_triangle_offset + index * 28, False, True)
    for index in range(uv_quad_count):
        decode_polygon(uv_quad_offset + index * 28, True, True)

    low = [min(vertex[axis] for vertex in vertices) for axis in range(3)]
    high = [max(vertex[axis] for vertex in vertices) for axis in range(3)]
    return {
        "positions": positions,
        "normals": normals,
        "uvs": uvs,
        "colors": colors,
        "textured": textured,
        "center": [(low[axis] + high[axis]) / 2 for axis in range(3)],
        "radius": max(high[axis] - low[axis] for axis in range(3)) / 2,
        "source": {
            "vertices": vertex_count,
            "normals": normal_count,
            "triangles": tri_count,
            "quads": quad_count,
            "uvTriangles": uv_tri_count,
            "uvQuads": uv_quad_count,
        },
    }


def render_variant_png(
    model: dict[str, object],
    atlas_rgba: bytes,
    output: Path,
    label: str,
    *,
    width: int = 960,
    height: int = 640,
    yaw_degrees: float = -62.0,
    pitch_degrees: float = -8.0,
) -> None:
    """Render the same native triangle soup without requiring a browser."""
    import numpy as np
    from PIL import Image, ImageDraw, ImageFont

    y_grid = np.linspace(0.0, 1.0, height, dtype=np.float32)[:, None]
    x_grid = np.linspace(0.0, 1.0, width, dtype=np.float32)[None, :]
    vignette = np.clip(
        1.0
        - 0.7
        * (
            (x_grid - 0.53) ** 2
            + (y_grid - 0.42) ** 2
        ),
        0.0,
        1.0,
    )
    top = np.array((42, 50, 59), dtype=np.float32)
    bottom = np.array((7, 9, 12), dtype=np.float32)
    background = (
        top[None, None, :] * (1.0 - y_grid[:, :, None])
        + bottom[None, None, :] * y_grid[:, :, None]
    )
    frame = np.clip(
        background * vignette[:, :, None], 0, 255
    ).astype(np.uint8)
    z_buffer = np.full((height, width), -np.inf, dtype=np.float32)

    positions = np.asarray(model["positions"], dtype=np.float32).reshape(-1, 3)
    normals = np.asarray(model["normals"], dtype=np.float32).reshape(-1, 3)
    uvs = np.asarray(model["uvs"], dtype=np.float32).reshape(-1, 2)
    colors = np.asarray(model["colors"], dtype=np.float32).reshape(-1, 3)
    textured = np.asarray(model["textured"], dtype=np.float32)
    center = np.asarray(model["center"], dtype=np.float32)
    radius = float(model["radius"]) or 1.0
    atlas = np.frombuffer(atlas_rgba, dtype=np.uint8).reshape(224 * 16, 256, 4)

    yaw = math.radians(yaw_degrees)
    pitch = math.radians(pitch_degrees)
    cy, sy = math.cos(yaw), math.sin(yaw)
    cx, sx = math.cos(pitch), math.sin(pitch)
    rotate_y = np.array(
        ((cy, 0.0, sy), (0.0, 1.0, 0.0), (-sy, 0.0, cy)),
        dtype=np.float32,
    )
    rotate_x = np.array(
        ((1.0, 0.0, 0.0), (0.0, cx, -sx), (0.0, sx, cx)),
        dtype=np.float32,
    )
    rotation = rotate_x @ rotate_y
    view = (positions - center) @ rotation.T
    pixels_per_unit = min(width * 0.92, height * 1.00) / (radius * 2.0)
    screen = np.empty((len(view), 2), dtype=np.float32)
    screen[:, 0] = width * 0.54 + view[:, 0] * pixels_per_unit
    screen[:, 1] = height * 0.47 - view[:, 1] * pixels_per_unit
    view_normals = normals @ rotation.T
    light_direction = np.array((-0.35, 0.8, 0.55), dtype=np.float32)
    light_direction /= np.linalg.norm(light_direction)

    for triangle in range(len(screen) // 3):
        begin = triangle * 3
        xy = screen[begin : begin + 3]
        z = view[begin : begin + 3, 2]
        low = np.maximum(np.floor(xy.min(axis=0)).astype(int), (0, 0))
        high = np.minimum(
            np.ceil(xy.max(axis=0)).astype(int),
            (width - 1, height - 1),
        )
        if np.any(high < low):
            continue
        x0, y0 = xy[0]
        x1, y1 = xy[1]
        x2, y2 = xy[2]
        denominator = (y1 - y2) * (x0 - x2) + (x2 - x1) * (y0 - y2)
        if abs(float(denominator)) < 1e-6:
            continue
        grid_x, grid_y = np.meshgrid(
            np.arange(low[0], high[0] + 1, dtype=np.float32) + 0.5,
            np.arange(low[1], high[1] + 1, dtype=np.float32) + 0.5,
        )
        a = (
            (y1 - y2) * (grid_x - x2)
            + (x2 - x1) * (grid_y - y2)
        ) / denominator
        b = (
            (y2 - y0) * (grid_x - x2)
            + (x0 - x2) * (grid_y - y2)
        ) / denominator
        c = 1.0 - a - b
        inside = (a >= -1e-5) & (b >= -1e-5) & (c >= -1e-5)
        depth = a * z[0] + b * z[1] + c * z[2]
        z_slice = z_buffer[low[1] : high[1] + 1, low[0] : high[0] + 1]
        visible = inside & (depth >= z_slice - 1e-4)
        if not np.any(visible):
            continue

        if textured[begin] > 0.5:
            uv = (
                a[:, :, None] * uvs[begin]
                + b[:, :, None] * uvs[begin + 1]
                + c[:, :, None] * uvs[begin + 2]
            )
            texture_x = np.clip((uv[:, :, 0] * 256).astype(int), 0, 255)
            texture_y = np.clip(
                (uv[:, :, 1] * (224 * 16)).astype(int),
                0,
                224 * 16 - 1,
            )
            sampled = atlas[texture_y, texture_x]
            visible &= sampled[:, :, 3] >= 16
            rgb = sampled[:, :, :3].astype(np.float32)
        else:
            rgb = np.broadcast_to(
                colors[begin] * 255.0,
                (*inside.shape, 3),
            ).copy()
        if not np.any(visible):
            continue

        normal = view_normals[begin]
        normal_length = float(np.linalg.norm(normal)) or 1.0
        light = 0.58 + 0.42 * max(
            float(np.dot(normal / normal_length, light_direction)),
            0.0,
        )
        shaded = np.clip(rgb * light, 0, 255).astype(np.uint8)
        frame_slice = frame[low[1] : high[1] + 1, low[0] : high[0] + 1]
        frame_slice[visible] = shaded[visible]
        z_slice[visible] = depth[visible]

    image = Image.fromarray(frame, "RGB")
    draw = ImageDraw.Draw(image)
    font = ImageFont.load_default(size=20)
    small = ImageFont.load_default(size=13)
    draw.rounded_rectangle(
        (24, height - 76, width - 24, height - 20),
        radius=7,
        fill=(5, 7, 10, 205),
        outline=(56, 65, 75),
    )
    draw.text((42, height - 62), label, fill=(240, 244, 248), font=font)
    draw.text(
        (42, height - 36),
        "Native CDO geometry + native CDP indexed bitmap/CLUT",
        fill=(159, 169, 181),
        font=small,
    )
    output.parent.mkdir(parents=True, exist_ok=True)
    image.save(output, optimize=True)


def parse_livery_table(data: bytes) -> list[dict[str, int | str]]:
    magic, version, count = struct.unpack_from("<4sHH", data)
    if magic != b"GTLV" or version != 3 or len(data) != 8 + count * 12:
        raise ValueError("unsupported native livery table")
    records = []
    for index in range(count):
        target, body, target_palette, body_palette, color_id = (
            struct.unpack_from("<IIBBH", data, 8 + index * 12)
        )
        records.append(
            {
                "target": decode_gt2_car_id(target),
                "body": decode_gt2_car_id(body),
                "targetPalette": target_palette,
                "bodyPalette": body_palette,
                "colorId": color_id,
            }
        )
    return records


def source_body_names(manifest: Path, target: str) -> dict[str, str]:
    names = {target: f"Retail GT2 {target}"}
    if not manifest.is_file():
        return names
    data = json.loads(manifest.read_text(encoding="utf-8"))
    folds = data.get("liveryFolds", {}).get("simulation", [])
    for fold in folds:
        if fold.get("targetStem") == target:
            names[str(fold["bodyStem"])] = (
                f"GT1 {fold['sourceStem']}"
            )
    return names


def html_document(payload: dict[str, object]) -> str:
    encoded = json.dumps(payload, separators=(",", ":"))
    return f"""<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>OpenGTPS1 Car &amp; Livery Viewer</title>
<style>
html,body{{margin:0;width:100%;height:100%;background:#080a0d;color:#edf1f5;
font:14px/1.35 "Segoe UI",sans-serif;overflow:hidden}}
#app{{display:grid;grid-template-columns:310px 1fr;height:100%}}
aside{{padding:24px;background:#11151b;border-right:1px solid #29303a;
box-shadow:8px 0 30px #0008;z-index:2}}
h1{{font-size:20px;margin:0 0 4px}} .sub{{color:#929cab;margin-bottom:24px}}
label{{display:block;color:#aeb7c4;font-size:12px;margin:18px 0 6px}}
select,input{{width:100%;box-sizing:border-box}} select{{background:#1d232c;color:white;
border:1px solid #3b4654;padding:10px;border-radius:4px}}
input[type=range]{{accent-color:#e8442e}}
#facts{{margin-top:22px;padding-top:18px;border-top:1px solid #2a313b;color:#aeb7c4;
white-space:pre-wrap;font-family:Consolas,monospace;font-size:12px}}
#badge{{display:inline-block;margin-top:12px;padding:5px 8px;border-radius:3px;
background:#263342;color:#d9eaff;font-size:11px}}
#links{{display:flex;gap:8px;margin-top:12px}} #links a{{color:#d9eaff;
text-decoration:none;border:1px solid #3b4654;border-radius:3px;padding:5px 8px;
font-size:11px}} #links a:hover{{background:#263342}}
main{{position:relative;min-width:0}} canvas{{width:100%;height:100%;display:block;
background:radial-gradient(circle at 50% 38%,#46515d 0,#1a2027 36%,#07090c 78%)}}
#label{{position:absolute;left:28px;bottom:26px;text-shadow:0 2px 5px #000;
font-size:18px;font-weight:600}} #hint{{position:absolute;right:24px;bottom:22px;
color:#9ba5b2;font-size:12px}}
</style>
</head>
<body><div id="app"><aside>
<h1>Car &amp; Livery Viewer</h1><div class="sub">Direct native archive assets</div>
<label for="variant">Source body / palette</label><select id="variant"></select>
<label for="angle">Turntable angle</label><input id="angle" type="range" min="-180" max="180" value="-28">
<label for="pitch">Camera pitch</label><input id="pitch" type="range" min="-35" max="25" value="-8">
<div id="badge"></div><div id="links"><a id="hero" download>3/4 PNG</a>
<a id="profile" download>Profile PNG</a></div><div id="facts"></div>
</aside><main><canvas id="canvas"></canvas><div id="label"></div>
<div id="hint">Drag to rotate · wheel to zoom</div></main></div>
<script>
const DATA={encoded};
const canvas=document.querySelector("#canvas"),gl=canvas.getContext("webgl",{{antialias:true,alpha:true}});
if(!gl)throw new Error("WebGL unavailable");
const vs=`attribute vec3 p,n;attribute vec2 uv;attribute vec3 color;attribute float tex;
uniform mat4 mvp;uniform mat3 normalMatrix;varying vec2 vuv;varying vec3 vc;varying float vt;
varying float light;void main(){{gl_Position=mvp*vec4(p,1.);vuv=uv;vc=color;vt=tex;
vec3 nn=normalize(normalMatrix*n);light=.58+.42*max(dot(nn,normalize(vec3(-.35,.8,.55))),0.);}}`;
const fs=`precision mediump float;uniform sampler2D atlas;varying vec2 vuv;varying vec3 vc;
varying float vt;varying float light;void main(){{vec4 t=texture2D(atlas,vuv);
vec4 base=mix(vec4(vc,1.),t,vt);if(vt>.5&&base.a<.1)discard;
gl_FragColor=vec4(base.rgb*light,base.a);}}`;
function shader(type,src){{const s=gl.createShader(type);gl.shaderSource(s,src);gl.compileShader(s);
if(!gl.getShaderParameter(s,gl.COMPILE_STATUS))throw new Error(gl.getShaderInfoLog(s));return s}}
const program=gl.createProgram();gl.attachShader(program,shader(gl.VERTEX_SHADER,vs));
gl.attachShader(program,shader(gl.FRAGMENT_SHADER,fs));gl.linkProgram(program);gl.useProgram(program);
function buffer(name,size,data){{const b=gl.createBuffer();gl.bindBuffer(gl.ARRAY_BUFFER,b);
gl.bufferData(gl.ARRAY_BUFFER,new Float32Array(data),gl.STATIC_DRAW);const a=gl.getAttribLocation(program,name);
gl.enableVertexAttribArray(a);gl.vertexAttribPointer(a,size,gl.FLOAT,false,0,0);return b}}
function perspective(fov,aspect,near,far){{const f=1/Math.tan(fov/2),nf=1/(near-far);
return [f/aspect,0,0,0,0,f,0,0,0,0,(far+near)*nf,-1,0,0,2*far*near*nf,0]}}
function mul(a,b){{let o=new Array(16).fill(0);for(let c=0;c<4;c++)for(let r=0;r<4;r++)
for(let k=0;k<4;k++)o[c*4+r]+=a[k*4+r]*b[c*4+k];return o}}
function translate(x,y,z){{return [1,0,0,0,0,1,0,0,0,0,1,0,x,y,z,1]}}
function scale(s){{return [s,0,0,0,0,s,0,0,0,0,s,0,0,0,0,1]}}
function rx(a){{let c=Math.cos(a),s=Math.sin(a);return [1,0,0,0,0,c,s,0,0,-s,c,0,0,0,0,1]}}
function ry(a){{let c=Math.cos(a),s=Math.sin(a);return [c,0,-s,0,0,1,0,0,s,0,c,0,0,0,0,1]}}
let current=0,yaw=-28,pitch=-8,zoom=1,buffers=[],texture=null,vertexCount=0;
const select=document.querySelector("#variant"),facts=document.querySelector("#facts"),
badge=document.querySelector("#badge"),label=document.querySelector("#label"),
hero=document.querySelector("#hero"),profile=document.querySelector("#profile");
DATA.variants.forEach((v,i)=>{{const o=document.createElement("option");o.value=i;o.textContent=v.label;select.append(o)}});
function load(index){{current=+index;const v=DATA.variants[current],m=DATA.models[v.body];
buffers.forEach(gl.deleteBuffer);buffers=[buffer("p",3,m.positions),buffer("n",3,m.normals),
buffer("uv",2,m.uvs),buffer("color",3,m.colors),buffer("tex",1,m.textured)];
vertexCount=m.positions.length/3;if(texture)gl.deleteTexture(texture);texture=gl.createTexture();
const image=new Image();image.onload=()=>{{gl.bindTexture(gl.TEXTURE_2D,texture);
gl.texImage2D(gl.TEXTURE_2D,0,gl.RGBA,gl.RGBA,gl.UNSIGNED_BYTE,image);
gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_MIN_FILTER,gl.NEAREST);
gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_MAG_FILTER,gl.NEAREST);
gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_WRAP_S,gl.CLAMP_TO_EDGE);
gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_WRAP_T,gl.CLAMP_TO_EDGE);draw()}};
image.src=v.texture;label.textContent=v.label;badge.textContent=v.status;
hero.href=v.render;profile.href=v.sideRender;
facts.textContent=`Body: ${{v.body}}\\nBody palette: ${{v.bodyPalette}}\\nColor ID: ${{v.colorId}}\\n`+
`Model SHA: ${{v.modelSha.slice(0,16)}}…\\nTexture SHA: ${{v.textureSha.slice(0,16)}}…\\n`+
`Bitmap SHA: ${{v.bitmapSha.slice(0,16)}}…`;window.captureLabel=v.slug;draw()}}
function draw(){{const w=canvas.clientWidth*devicePixelRatio|0,h=canvas.clientHeight*devicePixelRatio|0;
if(canvas.width!==w||canvas.height!==h){{canvas.width=w;canvas.height=h}}gl.viewport(0,0,w,h);
gl.clearColor(0,0,0,0);gl.clear(gl.COLOR_BUFFER_BIT|gl.DEPTH_BUFFER_BIT);gl.enable(gl.DEPTH_TEST);
gl.disable(gl.CULL_FACE);const v=DATA.variants[current],m=DATA.models[v.body];
let model=translate(-m.center[0],-m.center[1],-m.center[2]);
model=mul(scale(1.55/(m.radius||1)*zoom),model);model=mul(ry(yaw*Math.PI/180),model);
model=mul(rx(pitch*Math.PI/180),model);model=mul(translate(0,0,-4),model);
const mvp=mul(perspective(.62,w/h,.1,100),model);gl.uniformMatrix4fv(gl.getUniformLocation(program,"mvp"),false,mvp);
const cy=Math.cos(yaw*Math.PI/180),sy=Math.sin(yaw*Math.PI/180),cx=Math.cos(pitch*Math.PI/180),sx=Math.sin(pitch*Math.PI/180);
gl.uniformMatrix3fv(gl.getUniformLocation(program,"normalMatrix"),false,[cy,sx*sy,-cx*sy,0,cx,sx,sy,-sx*cy,cx*cy]);
gl.activeTexture(gl.TEXTURE0);gl.bindTexture(gl.TEXTURE_2D,texture);gl.drawArrays(gl.TRIANGLES,0,vertexCount)}}
select.onchange=()=>load(select.value);document.querySelector("#angle").oninput=e=>{{yaw=+e.target.value;draw()}};
document.querySelector("#pitch").oninput=e=>{{pitch=+e.target.value;draw()}};
let drag=false,lastX=0,lastY=0;canvas.onpointerdown=e=>{{drag=true;lastX=e.clientX;lastY=e.clientY;canvas.setPointerCapture(e.pointerId)}};
canvas.onpointermove=e=>{{if(!drag)return;yaw+=e.clientX-lastX;pitch=Math.max(-60,Math.min(45,pitch+e.clientY-lastY));
lastX=e.clientX;lastY=e.clientY;draw()}};canvas.onpointerup=()=>drag=false;
canvas.onwheel=e=>{{e.preventDefault();zoom=Math.max(.55,Math.min(2,zoom*Math.exp(-e.deltaY*.001)));draw()}};
window.selectVariant=i=>{{select.value=i;load(i)}};window.viewerReady=true;window.addEventListener("resize",draw);load(0);
</script></body></html>"""


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--stem", default="tsplr")
    parser.add_argument(
        "--volume",
        type=Path,
        default=REPO / "work" / "gt2-unified" / "simulation.materialized.vol",
    )
    parser.add_argument(
        "--manifest",
        type=Path,
        default=REPO / "work" / "gt1-converted" / "manifest.json",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=REPO / "artifacts" / "car-livery-viewer",
    )
    args = parser.parse_args()

    carinfo = _parse_gt2_carinfo(read_member(args.volume, ".carinfoe"))
    targets = {
        str(record["stem"]): record for record in carinfo
    }
    if args.stem not in targets:
        raise ValueError(f"car-info identity is absent: {args.stem}")
    target = targets[args.stem]
    mappings = [
        record
        for record in parse_livery_table(
            read_member(args.volume, ".gtlivery")
        )
        if record["target"] == args.stem
    ]
    mapped_by_body_palette = {
        (str(record["body"]), int(record["bodyPalette"])): record
        for record in mappings
    }
    body_names = source_body_names(args.manifest, args.stem)
    bodies = [args.stem] + sorted(
        {
            str(record["body"])
            for record in mappings
            if record["body"] != args.stem
        }
    )

    models: dict[str, dict[str, object]] = {}
    variants: list[dict[str, object]] = []
    bitmap_by_body: dict[str, bytes] = {}
    atlas_by_body_palette: dict[tuple[str, int], bytes] = {}
    for body in bodies:
        model = gzip.decompress(
            read_member(args.volume, f"carobj/{body}.cdo.gz")
        )
        texture = gzip.decompress(
            read_member(args.volume, f"carobj/{body}.cdp.gz")
        )
        models[body] = decode_model(model)
        model_sha = hashlib.sha256(model).hexdigest()
        texture_sha = hashlib.sha256(texture).hexdigest()
        paint_count = texture[0]
        color_ids = list(texture[2 : 2 + paint_count])
        for paint_index, color_id in enumerate(color_ids):
            rgba, bitmap = decode_texture_atlas(texture, paint_index)
            bitmap_by_body[body] = bitmap
            atlas_by_body_palette[(body, paint_index)] = rgba
            mapping = mapped_by_body_palette.get((body, paint_index))
            target_palette = (
                int(mapping["targetPalette"]) if mapping is not None else None
            )
            if body == args.stem and paint_index == 0:
                source_label = f"Retail GT2 {args.stem}"
            else:
                source_label = body_names.get(body, body)
            status = (
                f"Integrated choice {target_palette + 1}"
                if target_palette is not None
                else "Source-only duplicate-ID comparison"
            )
            label = (
                f"{source_label} · palette {paint_index} · ID {color_id}"
            )
            png = png_rgba(256, 224 * 16, rgba)
            variants.append(
                {
                    "label": label,
                    "slug": (
                        f"{body}-palette-{paint_index}-id-{color_id}"
                    ),
                    "status": status,
                    "body": body,
                    "bodyPalette": paint_index,
                    "targetPalette": target_palette,
                    "colorId": color_id,
                    "modelSha": model_sha,
                    "textureSha": texture_sha,
                    "bitmapSha": hashlib.sha256(bitmap).hexdigest(),
                    "texture": (
                        "data:image/png;base64,"
                        + base64.b64encode(png).decode("ascii")
                    ),
                }
            )

    order = {
        (args.stem, 0): 0,
        **{
            (str(record["body"]), int(record["bodyPalette"])): (
                int(record["targetPalette"]) + 2
            )
            for record in mappings
            if record["body"] != args.stem
        },
    }
    variants.sort(
        key=lambda item: (
            order.get(
                (str(item["body"]), int(item["bodyPalette"])), 1
            ),
            str(item["body"]),
            int(item["bodyPalette"]),
        )
    )
    for variant in variants:
        render_path = (
            args.output / "renders" / f"{variant['slug']}.png"
        )
        render_variant_png(
            models[str(variant["body"])],
            atlas_by_body_palette[
                (str(variant["body"]), int(variant["bodyPalette"]))
            ],
            render_path,
            str(variant["label"]),
        )
        side_render_path = (
            args.output / "renders" / "side" / f"{variant['slug']}.png"
        )
        render_variant_png(
            models[str(variant["body"])],
            atlas_by_body_palette[
                (str(variant["body"]), int(variant["bodyPalette"]))
            ],
            side_render_path,
            str(variant["label"]) + " - profile",
            yaw_degrees=-90.0,
            pitch_degrees=-3.0,
        )
        variant["render"] = str(render_path.relative_to(args.output))
        variant["sideRender"] = str(
            side_render_path.relative_to(args.output)
        )

    from PIL import Image

    sheet_width, cell_width, cell_height = 1200, 600, 400
    sheet_rows = math.ceil(len(variants) / 2)
    sheet = Image.new(
        "RGB",
        (sheet_width, sheet_rows * cell_height),
        (7, 9, 12),
    )
    for index, variant in enumerate(variants):
        source = Image.open(
            args.output / str(variant["sideRender"])
        ).convert("RGB")
        source.thumbnail((cell_width, cell_height))
        sheet.paste(
            source,
            (
                (index % 2) * cell_width,
                (index // 2) * cell_height,
            ),
        )
    sheet.save(args.output / "supra-comparison.png", optimize=True)

    comparisons = []
    for left_index, left in enumerate(variants):
        for right in variants[left_index + 1 :]:
            left_bitmap = bitmap_by_body[str(left["body"])]
            right_bitmap = bitmap_by_body[str(right["body"])]
            comparisons.append(
                {
                    "left": left["slug"],
                    "right": right["slug"],
                    "sameModel": left["modelSha"] == right["modelSha"],
                    "sameTexture": left["textureSha"] == right["textureSha"],
                    "sameIndexedBitmap": left["bitmapSha"] == right["bitmapSha"],
                    "indexedBitmapByteDifferences": sum(
                        a != b
                        for a, b in zip(left_bitmap, right_bitmap)
                    ),
                }
            )

    payload = {
        "formatVersion": 1,
        "targetStem": args.stem,
        "targetColorIds": list(target["colorIds"]),
        "models": models,
        "variants": variants,
    }
    args.output.mkdir(parents=True, exist_ok=True)
    html = args.output / "index.html"
    html.write_text(html_document(payload), encoding="utf-8")
    manifest = {
        "formatVersion": 1,
        "targetStem": args.stem,
        "volume": str(args.volume.resolve()),
        "variants": [
            {key: value for key, value in item.items() if key != "texture"}
            for item in variants
        ],
        "comparisons": comparisons,
    }
    (args.output / "viewer_manifest.json").write_text(
        json.dumps(manifest, indent=2) + "\n", encoding="utf-8"
    )
    print(
        f"car viewer ready: {html} "
        f"({len(models)} bodies, {len(variants)} source palettes)"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
