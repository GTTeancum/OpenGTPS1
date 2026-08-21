#!/usr/bin/env python3
"""Build a loose 4x Real-ESRGAN texture pack from GT2's authored assets.

The primary inventory comes directly from materialized GT2 VOL archives so
cars, paint banks, courses, and UI textures do not have to be triggered in the
game. Each authored bitmap is upscaled independently. Cars retain GT2's native
one-indexed-bitmap/many-live-palettes organization: Real-ESRGAN enhances one
neutral detail source per shared bitmap and the renderer applies the selected,
dynamically lit material CLUTs at runtime. No resampling fallback is permitted.
The tool writes one loose uncompressed DDS file per authored bitmap.
"""

from __future__ import annotations

import argparse
from collections import Counter, deque
from datetime import datetime, timezone
import gzip
import hashlib
import json
from pathlib import Path
import re
import shutil
import struct
import subprocess
import sys

import numpy as np
from PIL import Image, ImageDraw

from gt2_vol import read_entries


FNV_OFFSET = 14695981039346656037
FNV_PRIME = 1099511628211
SCALE = 4
PAD = 8
PACK_FORMAT = 7
REALESRGAN_MODEL = "realesr-animevideov3"
REALESRGAN_MODEL_FILES = f"{REALESRGAN_MODEL}-x{SCALE}"
REALESRGAN_TILE = 32
MAX_ROUNDTRIP_MAE = 12.0
REPLACEMENT_RGB = "rgb"
REPLACEMENT_PALETTE_DETAIL = "paletteDetail"
DEFAULT_REALESRGAN = Path(
    r"C:\Programming\GitHub\Vigilante-8-recomp\build\realesrgan"
    r"\realesrgan-ncnn-vulkan.exe"
)

AssetIdentity = tuple[int, int]


def asset_stem(identity: AssetIdentity) -> str:
    bitmap_key, palette_key = identity
    return (
        f"{bitmap_key:016x}"
        if palette_key == 0
        else f"{bitmap_key:016x}-{palette_key:016x}"
    )


def runtime_hash(image: Image.Image) -> int:
    width, height = image.size
    value = FNV_OFFSET
    for byte in (width & 0xFF, width >> 8, height & 0xFF, height >> 8):
        value ^= byte
        value = (value * FNV_PRIME) & 0xFFFFFFFFFFFFFFFF
    for byte in image.tobytes():
        value ^= byte
        value = (value * FNV_PRIME) & 0xFFFFFFFFFFFFFFFF
    return value


def runtime_index_hash(indices: bytes, width: int, height: int, mode: int) -> int:
    """Mirror the native page key without depending on a live CLUT."""

    if len(indices) != width * height or mode not in (0, 1):
        raise ValueError("invalid indexed runtime-key input")
    value = FNV_OFFSET
    for byte in (
        width & 0xFF, width >> 8, height & 0xFF, height >> 8, mode
    ):
        value ^= byte
        value = (value * FNV_PRIME) & 0xFFFFFFFFFFFFFFFF
    for byte in indices:
        value ^= byte
        value = (value * FNV_PRIME) & 0xFFFFFFFFFFFFFFFF
    return value


def upload_hash(words: bytes, word_width: int, height: int) -> int:
    """Mirror RecompOne's identity for one completed GPU image upload."""

    if (
        word_width <= 0 or height <= 0 or
        len(words) != word_width * height * 2
    ):
        raise ValueError("invalid texture-upload dimensions")
    value = FNV_OFFSET
    for byte in (
        word_width & 0xFF, word_width >> 8,
        height & 0xFF, height >> 8,
    ):
        value ^= byte
        value = (value * FNV_PRIME) & 0xFFFFFFFFFFFFFFFF
    for byte in words:
        value ^= byte
        value = (value * FNV_PRIME) & 0xFFFFFFFFFFFFFFFF
    return value


def tim_payload_size(data: bytes, offset: int = 0) -> int:
    """Return the exact byte span of one standard PlayStation TIM."""

    if offset < 0 or offset + 20 > len(data):
        raise ValueError("truncated TIM header")
    magic, flags = struct.unpack_from("<II", data, offset)
    if magic != 0x10 or flags & ~0x0F:
        raise ValueError("invalid TIM header")
    cursor = offset + 8
    if flags & 8:
        block_size = struct.unpack_from("<I", data, cursor)[0]
        if block_size < 12 or cursor + block_size > len(data):
            raise ValueError("invalid TIM CLUT block")
        cursor += block_size
    image_size = struct.unpack_from("<I", data, cursor)[0]
    if image_size < 12 or cursor + image_size > len(data):
        raise ValueError("invalid TIM image block")
    return cursor + image_size - offset


def trp_indexed_page_keys(data: bytes) -> dict[int, tuple[int, int]]:
    """Recreate indexed texture-page keys from a native GT2 TRP package."""

    if len(data) < 4:
        raise ValueError("truncated GT2 TRP")
    count = struct.unpack_from("<I", data, 0)[0]
    if not 0 < count < 4096:
        raise ValueError(f"invalid GT2 TRP TIM count: {count}")
    cursor = 4
    vram = [0] * (1024 * 512)
    touched: set[tuple[int, int, int]] = set()
    for _ in range(count):
        size = tim_payload_size(data, cursor)
        flags = struct.unpack_from("<I", data, cursor + 4)[0]
        mode = flags & 7
        block = cursor + 8
        if flags & 8:
            clut_size, clut_x, clut_y, clut_width, clut_height = (
                struct.unpack_from("<IHHHH", data, block)
            )
            clut_words = struct.unpack_from(
                f"<{clut_width * clut_height}H", data, block + 12
            )
            for y in range(clut_height):
                start = (clut_y + y) * 1024 + clut_x
                row = y * clut_width
                vram[start:start + clut_width] = clut_words[
                    row:row + clut_width
                ]
            block += clut_size
        image_size, image_x, image_y, width_words, height = struct.unpack_from(
            "<IHHHH", data, block
        )
        if image_size != 12 + width_words * height * 2:
            raise ValueError("TIM image payload size is inconsistent")
        words = struct.unpack_from(
            f"<{width_words * height}H", data, block + 12
        )
        for y in range(height):
            start = (image_y + y) * 1024 + image_x
            row = y * width_words
            vram[start:start + width_words] = words[row:row + width_words]
        if mode in (0, 1):
            first_page_x = image_x // 64
            last_page_x = (image_x + width_words - 1) // 64
            first_page_y = image_y // 256
            last_page_y = (image_y + height - 1) // 256
            for page_y in range(first_page_y, last_page_y + 1):
                for page_x in range(first_page_x, last_page_x + 1):
                    touched.add((page_x, page_y, mode))
        cursor += size
    if cursor != len(data):
        raise ValueError(f"GT2 TRP has {len(data) - cursor} trailing bytes")

    result: dict[int, tuple[int, int]] = {}
    for page_x, page_y, mode in sorted(touched):
        base_x = page_x * 64
        base_y = page_y * 256
        indices = bytearray(256 * 256)
        for v in range(256):
            row = (base_y + v) * 1024 + base_x
            for u in range(256):
                packed = vram[row + (u >> (2 if mode == 0 else 1))]
                shift = (u & (3 if mode == 0 else 1)) * (4 if mode == 0 else 8)
                indices[v * 256 + u] = (packed >> shift) & (
                    15 if mode == 0 else 255
                )
        result[runtime_index_hash(bytes(indices), 256, 256, mode)] = (
            page_x | (page_y << 4), mode
        )
    return result


def decode_tim_asset(data: bytes) -> tuple[int, Image.Image, int]:
    """Decode one authored 4/8-bit TIM and return its exact upload key."""

    if tim_payload_size(data) != len(data):
        raise ValueError("TIM payload has trailing bytes")
    _, flags = struct.unpack_from("<II", data, 0)
    mode = flags & 7
    if mode not in (0, 1) or not flags & 8:
        raise ValueError(f"unsupported replacement TIM flags: {flags:#x}")
    cursor = 8
    clut_size, _, _, clut_width, clut_height = struct.unpack_from(
        "<IHHHH", data, cursor
    )
    if clut_width < (16 if mode == 0 else 256) or clut_height < 1:
        raise ValueError("TIM does not contain a complete first CLUT")
    palette = struct.unpack_from(
        f"<{clut_width}H", data, cursor + 12
    )
    cursor += clut_size
    image_size, _, _, word_width, height = struct.unpack_from(
        "<IHHHH", data, cursor
    )
    words = data[cursor + 12:cursor + image_size]
    width = word_width * (4 if mode == 0 else 2)
    rgba = bytearray(width * height * 4)
    for y in range(height):
        for x in range(width):
            packed = struct.unpack_from(
                "<H", words,
                (y * word_width + (x >> (2 if mode == 0 else 1))) * 2,
            )[0]
            shift = (x & (3 if mode == 0 else 1)) * (4 if mode == 0 else 8)
            index = (packed >> shift) & (15 if mode == 0 else 255)
            color = palette[index]
            output = (y * width + x) * 4
            rgba[output] = ((color & 31) << 3) | ((color & 31) >> 2)
            green = (color >> 5) & 31
            blue = (color >> 10) & 31
            rgba[output + 1] = (green << 3) | (green >> 2)
            rgba[output + 2] = (blue << 3) | (blue >> 2)
            rgba[output + 3] = 0 if color == 0 else 255
    return (
        upload_hash(words, word_width, height),
        Image.frombytes("RGBA", (width, height), bytes(rgba)),
        mode,
    )


def tim_upload_identity(data: bytes) -> tuple[int, int, int, int]:
    """Return upload key, word width, height, and mode without decoding RGB."""

    if tim_payload_size(data) != len(data):
        raise ValueError("TIM payload has trailing bytes")
    _, flags = struct.unpack_from("<II", data, 0)
    mode = flags & 7
    if mode not in (0, 1):
        raise ValueError(f"unsupported replacement TIM flags: {flags:#x}")
    cursor = 8
    if flags & 8:
        cursor += struct.unpack_from("<I", data, cursor)[0]
    image_size, _, _, word_width, height = struct.unpack_from(
        "<IHHHH", data, cursor
    )
    words = data[cursor + 12:cursor + image_size]
    if len(words) != word_width * height * 2:
        raise ValueError("TIM image payload size is inconsistent")
    return upload_hash(words, word_width, height), word_width, height, mode


def trp_tim_members(data: bytes) -> list[bytes]:
    if len(data) < 4:
        raise ValueError("truncated GT2 TRP")
    count = struct.unpack_from("<I", data, 0)[0]
    if not 0 < count < 4096:
        raise ValueError(f"invalid GT2 TRP TIM count: {count}")
    cursor = 4
    members: list[bytes] = []
    for _ in range(count):
        size = tim_payload_size(data, cursor)
        members.append(data[cursor:cursor + size])
        cursor += size
    if cursor != len(data):
        raise ValueError(f"GT2 TRP has {len(data) - cursor} trailing bytes")
    return members


def read_course_assets(
    volumes: list[Path], name_filter: str
) -> tuple[dict[int, Image.Image], dict[int, int], dict[str, object]]:
    """Read one independently replaceable texture per matching TRP TIM."""

    needle = Path(name_filter.casefold()).name
    if needle.endswith(".trp.gz"):
        needle = needle[:-7]
    images: dict[int, Image.Image] = {}
    modes: dict[int, int] = {}
    packages: list[str] = []
    aliases = 0
    for volume in volumes:
        for entry in read_entries(volume):
            name = entry.name.casefold()
            stem = Path(name).name
            if stem.endswith(".trp.gz"):
                stem = stem[:-7]
            if (
                not name.startswith("crsobj/") or
                not name.endswith(".trp.gz") or
                stem != needle
            ):
                continue
            packages.append(f"{volume.name}:{entry.name}")
            payload = gzip.decompress(read_volume_member(volume, entry))
            for member in trp_tim_members(payload):
                try:
                    key, image, mode = decode_tim_asset(member)
                except ValueError as error:
                    if "unsupported replacement TIM flags" in str(error):
                        continue
                    raise
                aliases += 1
                prior = images.get(key)
                if prior is not None:
                    if prior.size != image.size or prior.tobytes() != image.tobytes():
                        raise ValueError(
                            f"upload-key palette collision for {key:016x}"
                        )
                    continue
                images[key] = image
                modes[key] = mode
    if not packages:
        raise ValueError(f"no course TRP names contain {name_filter!r}")
    return images, modes, {
        "packages": packages,
        "authoredTimAliases": aliases,
        "uniqueIndividualTextures": len(images),
    }


def read_dumps(directories: list[Path]) -> dict[int, Image.Image]:
    images: dict[int, Image.Image] = {}
    for directory in directories:
        if not directory.is_dir():
            raise FileNotFoundError(f"texture dump directory not found: {directory}")
        for path in sorted(directory.glob("*.rgba")):
            try:
                key_text, dimensions = path.stem.split("_", 1)
                width_text, height_text = dimensions.split("x", 1)
                key = int(key_text, 16)
                width, height = int(width_text), int(height_text)
            except ValueError as error:
                raise ValueError(f"invalid runtime dump name: {path.name}") from error
            payload = path.read_bytes()
            if width <= 0 or height <= 0 or len(payload) != width * height * 4:
                raise ValueError(f"invalid runtime dump dimensions: {path.name}")
            image = Image.frombytes("RGBA", (width, height), payload)
            # Paletted GT2 textures use an index-content key so per-frame
            # lighting CLUT changes resolve the same learned detail. Direct
            # color textures use the same key convention over their RGBA.
            # The native dumper is the authority for both forms.
            prior = images.get(key)
            if prior is not None and prior.tobytes() != payload:
                raise ValueError(f"64-bit runtime texture collision: {key:016x}")
            images[key] = image
    if not images:
        raise ValueError("no .rgba runtime texture dumps were found")
    return images


def read_volume_member(volume: Path, entry: object) -> bytes:
    with volume.open("rb") as stream:
        stream.seek(entry.offset)
        payload = stream.read(entry.size)
    if len(payload) != entry.size:
        raise IOError(f"short VOL read for {entry.name}")
    return payload


def match_car_upload_trace(
    volumes: list[Path], trace_path: Path
) -> dict[str, object]:
    """Match exact runtime GPU uploads to offline CDP/CNP car assets."""

    upload_pattern = re.compile(
        r"^\[TextureUpload\] key=([0-9a-f]{16}) .* "
        r"words=(\d+) height=(\d+)(?: data=[0-9A-F]+)?$"
    )
    runtime: dict[int, tuple[int, int]] = {}
    for line in trace_path.read_text(encoding="utf-8", errors="replace").splitlines():
        match = upload_pattern.match(line)
        if match:
            runtime[int(match.group(1), 16)] = (
                int(match.group(2)), int(match.group(3))
            )

    bitmap_assets: dict[int, set[str]] = {}
    palette_assets: dict[int, set[str]] = {}
    for volume in volumes:
        for entry in read_entries(volume):
            name = entry.name.casefold()
            if not (
                name.startswith("carobj/") and
                name.endswith((".cdp.gz", ".cnp.gz"))
            ):
                continue
            payload = gzip.decompress(read_volume_member(volume, entry))
            if len(payload) != 0xB3A0 or not 1 <= payload[0] <= 16:
                raise ValueError(f"malformed native car texture: {entry.name}")
            identity = f"{volume.name}:{entry.name}"
            bitmap_key = upload_hash(payload[0x43A0:0xB3A0], 64, 224)
            bitmap_assets.setdefault(bitmap_key, set()).add(identity)
            for paint_index in range(payload[0]):
                palette = payload[
                    0x20 + paint_index * 0x240 :
                    0x220 + paint_index * 0x240
                ]
                label = (
                    f"{identity}:paint={paint_index}:colorId="
                    f"{payload[2 + paint_index]}"
                )
                for word_width, height in ((256, 1), (16, 16), (64, 4)):
                    palette_key = upload_hash(palette, word_width, height)
                    palette_assets.setdefault(palette_key, set()).add(label)
                for material in range(16):
                    clut = palette[material * 32:(material + 1) * 32]
                    clut_key = upload_hash(clut, 16, 1)
                    palette_assets.setdefault(clut_key, set()).add(
                        f"{label}:material={material}"
                    )

    def matches(
        assets: dict[int, set[str]]
    ) -> list[dict[str, object]]:
        return [
            {
                "key": f"{key:016x}",
                "wordWidth": runtime[key][0],
                "height": runtime[key][1],
                "assets": sorted(aliases),
            }
            for key, aliases in sorted(assets.items())
            if key in runtime
        ]

    bitmap_matches = matches(bitmap_assets)
    palette_matches = matches(palette_assets)
    return {
        "trace": str(trace_path),
        "uniqueRuntimeUploads": len(runtime),
        "carBitmapMatches": bitmap_matches,
        "carPaletteMatches": palette_matches,
        "uniqueMatchedCarBitmaps": len(bitmap_matches),
        "uniqueMatchedCarPalettes": len(palette_matches),
    }


def runtime_upload_keys(trace_path: Path) -> dict[int, tuple[int, int]]:
    pattern = re.compile(
        r"^\[TextureUpload\] key=([0-9a-f]{16}) .* "
        r"words=(\d+) height=(\d+)(?: data=[0-9A-F]+)?$"
    )
    result: dict[int, tuple[int, int]] = {}
    for line in trace_path.read_text(encoding="utf-8", errors="replace").splitlines():
        match = pattern.match(line)
        if match:
            result[int(match.group(1), 16)] = (
                int(match.group(2)), int(match.group(3))
            )
    return result


def match_course_upload_trace(
    volumes: list[Path], trace_path: Path
) -> dict[str, object]:
    """Rank exact authored course packages by their live image-upload keys."""

    runtime = runtime_upload_keys(trace_path)
    packages: dict[str, dict[int, tuple[int, int]]] = {}
    for volume in volumes:
        for entry in read_entries(volume):
            name = entry.name.casefold()
            if not (
                name.startswith("crsobj/") and name.endswith(".trp.gz")
            ):
                continue
            payload = gzip.decompress(read_volume_member(volume, entry))
            keys: dict[int, tuple[int, int]] = {}
            for member in trp_tim_members(payload):
                try:
                    key, word_width, height, _ = tim_upload_identity(member)
                except ValueError as error:
                    if "unsupported replacement TIM flags" in str(error):
                        continue
                    raise
                keys[key] = (word_width, height)
            packages[f"{volume.name}:{entry.name}"] = keys
    ranked = []
    for package, keys in packages.items():
        matches = sorted(key for key, size in keys.items() if runtime.get(key) == size)
        ranked.append({
            "package": package,
            "individualTextures": len(keys),
            "matchedUploads": len(matches),
            "coveragePercent": round(100.0 * len(matches) / max(1, len(keys)), 3),
            "matchedKeys": [f"{key:016x}" for key in matches],
        })
    ranked.sort(
        key=lambda item: (
            -int(item["matchedUploads"]),
            -float(item["coveragePercent"]),
            str(item["package"]),
        )
    )
    return {
        "trace": str(trace_path),
        "uniqueRuntimeUploads": len(runtime),
        "rankedCoursePackages": ranked[:20],
    }


def car_material_palette(encoded: int) -> int:
    return ((encoded >> 4) & 0x0C) | (encoded & 0x03)


def car_model_palette_masks(model: bytes) -> tuple[np.ndarray, dict[str, int]]:
    """Rasterize all three native car LODs' material ownership in UV space."""

    if len(model) < 0x884 or model[:3] != b"GT\x02":
        raise ValueError("GT2 car model header is missing")
    if struct.unpack_from("<I", model, 0x868)[0] != 3:
        raise ValueError("GT2 car model does not contain three LODs")
    mask_images = [Image.new("1", (256, 224)) for _ in range(16)]
    draws = [ImageDraw.Draw(mask) for mask in mask_images]
    cursor = 0x884
    triangle_count = 0
    for _ in range(3):
        if cursor + 0x50 > len(model):
            raise ValueError("GT2 car LOD is truncated")
        counts = struct.unpack_from("<8H", model, cursor)
        vertex_count, normal_count, tri_count, quad_count = counts[:4]
        uv_tri_count, uv_quad_count = counts[6:8]
        offsets = struct.unpack_from("<7I", model, cursor + 0x1C)
        uv_triangle_offset = cursor + offsets[3]
        uv_quad_offset = cursor + offsets[6]

        def draw_packet(offset: int, corners: int) -> None:
            nonlocal triangle_count
            if offset < cursor or offset + 28 > len(model):
                raise ValueError("GT2 car textured polygon is truncated")
            packet = model[offset:offset + 28]
            palette = car_material_palette(
                struct.unpack_from("<H", packet, 18)[0]
            )
            uvs = [
                (packet[16], packet[17]),
                (packet[20], packet[21]),
                (packet[24], packet[25]),
                (packet[26], packet[27]),
            ][:corners]
            triangles = (
                (uvs,)
                if corners == 3
                else ((uvs[0], uvs[1], uvs[2]), (uvs[0], uvs[2], uvs[3]))
            )
            for triangle in triangles:
                draws[palette].polygon(triangle, fill=1)
                triangle_count += 1

        for index in range(uv_tri_count):
            draw_packet(uv_triangle_offset + index * 28, 3)
        for index in range(uv_quad_count):
            draw_packet(uv_quad_offset + index * 28, 4)
        cursor += (
            0x50 + vertex_count * 8 + normal_count * 4 +
            (tri_count + quad_count) * 16 +
            (uv_tri_count + uv_quad_count) * 28
        )
    masks = np.stack(
        [np.asarray(mask, dtype=bool) for mask in mask_images], axis=0
    )
    return masks, {
        "texturedTrianglesAcrossLods": triangle_count,
        "coveredTexels": int(np.any(masks, axis=0).sum()),
        "multiMaterialTexels": int((masks.sum(axis=0) > 1).sum()),
    }


def audit_car_composite(
    texture: bytes, paint_index: int, masks: np.ndarray
) -> dict[str, int]:
    """Prove whether one decoded car-paint image can represent every face."""

    if len(texture) != 0xB3A0 or not 0 <= paint_index < texture[0]:
        raise ValueError("invalid GT2 car texture or paint index")
    packed = np.frombuffer(texture, dtype=np.uint8, offset=0x43A0)
    indices = np.empty(256 * 224, dtype=np.uint8)
    indices[0::2] = packed & 15
    indices[1::2] = packed >> 4
    indices = indices.reshape(224, 256)
    cluts = np.frombuffer(
        texture,
        dtype="<u2",
        count=16 * 16,
        offset=0x20 + paint_index * 0x240,
    ).reshape(16, 16)
    active = masks
    ownership_count = active.sum(axis=0)
    colors = np.take_along_axis(
        cluts[:, None, :],
        np.broadcast_to(indices[None, :, :], (16, 224, 256)),
        axis=2,
    )
    first_material = np.argmax(active, axis=0)
    first_color = np.take_along_axis(
        colors, first_material[None, :, :], axis=0
    )[0]
    conflicting = np.any(
        active & (colors != first_color[None, :, :]), axis=0
    )
    possible_colors = np.take_along_axis(
        cluts[:, None, :],
        np.broadcast_to(indices[None, :, :], (16, 224, 256)),
        axis=2,
    )
    potentially_opaque = np.any(possible_colors != 0, axis=0)
    owned = ownership_count > 0
    return {
        "opaqueIndexedTexels": int((indices != 0).sum()),
        "potentiallyOpaqueTexels": int(potentially_opaque.sum()),
        "ownedOpaqueTexels": int((owned & (first_color != 0)).sum()),
        "unownedOpaqueTexels": int((potentially_opaque & ~owned).sum()),
        "multiMaterialOpaqueTexels": int(
            ((ownership_count > 1) & (first_color != 0)).sum()
        ),
        "conflictingOpaqueTexels": int(conflicting.sum()),
    }


def decode_car_composite(
    texture: bytes, paint_index: int, masks: np.ndarray
) -> Image.Image:
    """Decode one car paint into one image after proving UV palette ownership."""

    audit = audit_car_composite(texture, paint_index, masks)
    if audit["conflictingOpaqueTexels"] != 0:
        raise ValueError(
            "car UV islands require conflicting palettes at "
            f"{audit['conflictingOpaqueTexels']} texels"
        )
    packed = np.frombuffer(texture, dtype=np.uint8, offset=0x43A0)
    indices = np.empty(256 * 224, dtype=np.uint8)
    indices[0::2] = packed & 15
    indices[1::2] = packed >> 4
    indices = indices.reshape(224, 256)
    cluts = np.frombuffer(
        texture,
        dtype="<u2",
        count=16 * 16,
        offset=0x20 + paint_index * 0x240,
    ).reshape(16, 16)

    owned = np.any(masks, axis=0)
    if not np.any(owned):
        raise ValueError("car model has no textured UV ownership")
    owners = np.full((224, 256), -1, dtype=np.int16)
    owners[owned] = np.argmax(masks[:, owned], axis=0)
    # Unreferenced source texels still need stable RGB padding for bilinear
    # samples at UV-island edges. Propagate the nearest authored material;
    # these texels never select an asset or paint on their own.
    queue = deque((int(y), int(x)) for y, x in np.argwhere(owned))
    while queue:
        y, x = queue.popleft()
        material = owners[y, x]
        for next_y, next_x in (
            (y - 1, x), (y + 1, x), (y, x - 1), (y, x + 1)
        ):
            if (
                0 <= next_y < 224 and 0 <= next_x < 256 and
                owners[next_y, next_x] < 0
            ):
                owners[next_y, next_x] = material
                queue.append((next_y, next_x))

    colors = cluts[owners, indices]
    rgba = np.empty((224, 256, 4), dtype=np.uint8)
    red = colors & 31
    green = (colors >> 5) & 31
    blue = (colors >> 10) & 31
    rgba[:, :, 0] = (red << 3) | (red >> 2)
    rgba[:, :, 1] = (green << 3) | (green >> 2)
    rgba[:, :, 2] = (blue << 3) | (blue >> 2)
    rgba[:, :, 3] = np.where(colors == 0, 0, 255)
    return Image.fromarray(rgba, "RGBA")


def decode_car_neutral_detail(
    bitmap: bytes,
    palette_banks: list[bytes],
    masks: np.ndarray,
) -> Image.Image:
    """Decode one paint-neutral detail source for a shared GT2 car bitmap.

    The indexed bitmap is the authored structure shared by every paint. Each
    model polygon chooses one of sixteen material CLUTs. We therefore derive a
    grayscale source from the median luminance of every authored paint bank at
    that exact material/index pair. The output contains no selected paint hue;
    the renderer continues to fetch GT2's live, dynamically lit CLUT color.
    """

    if len(bitmap) != 0x7000 or not palette_banks:
        raise ValueError("invalid shared car bitmap or palette inventory")
    if masks.shape != (16, 224, 256):
        raise ValueError("invalid car material ownership masks")
    for palette in palette_banks:
        if len(palette) != 0x200:
            raise ValueError("invalid GT2 car paint bank")

    packed = np.frombuffer(bitmap, dtype=np.uint8)
    indices = np.empty(256 * 224, dtype=np.uint8)
    indices[0::2] = packed & 15
    indices[1::2] = packed >> 4
    indices = indices.reshape(224, 256)

    owned = np.any(masks, axis=0)
    if not np.any(owned):
        raise ValueError("car model has no textured UV ownership")
    owners = np.full((224, 256), -1, dtype=np.int16)
    owners[owned] = np.argmax(masks[:, owned], axis=0)
    queue = deque((int(y), int(x)) for y, x in np.argwhere(owned))
    while queue:
        y, x = queue.popleft()
        material = owners[y, x]
        for next_y, next_x in (
            (y - 1, x), (y + 1, x), (y, x - 1), (y, x + 1)
        ):
            if (
                0 <= next_y < 224 and 0 <= next_x < 256 and
                owners[next_y, next_x] < 0
            ):
                owners[next_y, next_x] = material
                queue.append((next_y, next_x))

    words = np.stack([
        np.frombuffer(palette, dtype="<u2").reshape(16, 16)
        for palette in palette_banks
    ])
    red = ((words & 31) << 3) | ((words & 31) >> 2)
    green5 = (words >> 5) & 31
    green = (green5 << 3) | (green5 >> 2)
    blue5 = (words >> 10) & 31
    blue = (blue5 << 3) | (blue5 >> 2)
    luminance = (
        red.astype(np.float32) * 0.2126 +
        green.astype(np.float32) * 0.7152 +
        blue.astype(np.float32) * 0.0722
    )
    visible = words != 0
    neutral = np.zeros((16, 16), dtype=np.uint8)
    for material in range(16):
        for index in range(16):
            samples = luminance[:, material, index][
                visible[:, material, index]
            ]
            if samples.size:
                neutral[material, index] = np.uint8(
                    np.clip(np.rint(np.median(samples)), 0, 255)
                )

    gray = neutral[owners, indices]
    opaque = np.any(visible[:, owners, indices], axis=0)
    rgba = np.empty((224, 256, 4), dtype=np.uint8)
    rgba[:, :, :3] = gray[:, :, None]
    rgba[:, :, 3] = np.where(opaque, 255, 0)
    return Image.fromarray(rgba, "RGBA")


def read_car_assets(
    volumes: list[Path], filters: list[str], world_capture: Path | None
) -> tuple[
    dict[AssetIdentity, Image.Image],
    dict[AssetIdentity, int],
    dict[AssetIdentity, bool],
    dict[AssetIdentity, str],
    dict[str, object],
]:
    """Build one paint-neutral source image per shared indexed car bitmap."""

    needles = {value.casefold() for value in filters}
    if not needles:
        raise ValueError("car pack generation requires at least one exact filter")
    capture_masks: dict[tuple[int, int], np.ndarray] = {}
    capture_stats: dict[str, object] = {}
    if world_capture is not None:
        capture_masks, capture_stats = captured_car_palette_masks(world_capture)
    grouped: dict[int, dict[str, object]] = {}
    for volume in volumes:
        entries = {entry.name.casefold(): entry for entry in read_entries(volume)}
        for name, entry in entries.items():
            if not (
                name.startswith("carobj/") and
                name.endswith((".cdp.gz", ".cnp.gz"))
            ):
                continue
            stem = Path(name).name.split(".", 1)[0]
            if stem not in needles:
                continue
            model_suffix = ".cdo.gz" if name.endswith(".cdp.gz") else ".cno.gz"
            model_entry = entries.get(f"carobj/{stem}{model_suffix}")
            if model_entry is None:
                raise ValueError(f"missing car model for {entry.name}")
            texture = gzip.decompress(read_volume_member(volume, entry))
            model = gzip.decompress(read_volume_member(volume, model_entry))
            if len(texture) != 0xB3A0 or not 1 <= texture[0] <= 16:
                raise ValueError(f"malformed native car texture: {entry.name}")
            masks, _ = car_model_palette_masks(model)
            bitmap = texture[0x43A0:0xB3A0]
            bitmap_key = upload_hash(bitmap, 64, 224)
            details = grouped.get(bitmap_key)
            if details is None:
                details = {
                    "bitmap": bitmap,
                    "masks": masks.copy(),
                    "palettes": {},
                    "samples": {},
                    "aliases": set(),
                }
                grouped[bitmap_key] = details
            else:
                if details["bitmap"] != bitmap:
                    raise ValueError(
                        "64-bit car bitmap identity collision for "
                        f"{bitmap_key:016x}"
                    )
                details["masks"] |= masks
            for paint_index in range(texture[0]):
                palette = texture[
                    0x20 + paint_index * 0x240:
                    0x220 + paint_index * 0x240
                ]
                palette_key = upload_hash(palette, 64, 4)
                alias = (
                    f"{volume.name}:{entry.name}:paint={paint_index}:"
                    f"colorId={texture[2 + paint_index]}"
                )
                prior_palette = details["palettes"].get(palette_key)
                if prior_palette is not None and prior_palette != palette:
                    raise ValueError(
                        "64-bit car palette identity collision for "
                        f"{palette_key:016x}"
                    )
                details["palettes"][palette_key] = palette
                details["samples"].setdefault(
                    palette_key, (texture, paint_index)
                )
                details["aliases"].add(alias)

    if not grouped:
        raise ValueError("no car assets matched the requested exact filters")
    images: dict[AssetIdentity, Image.Image] = {}
    modes: dict[AssetIdentity, int] = {}
    color_fits: dict[AssetIdentity, bool] = {}
    replacement_modes: dict[AssetIdentity, str] = {}
    entries: list[dict[str, object]] = []
    captured_bitmaps = {
        bitmap_key for bitmap_key, _ in capture_masks
    }
    for bitmap_key, details in sorted(grouped.items()):
        masks = details["masks"]
        for (captured_bitmap, _), live_masks in capture_masks.items():
            if captured_bitmap == bitmap_key:
                masks = masks | live_masks
        maximum_conflicts = 0
        for texture, paint in details["samples"].values():
            audit = audit_car_composite(texture, int(paint), masks)
            maximum_conflicts = max(
                maximum_conflicts, audit["conflictingOpaqueTexels"]
            )
        if maximum_conflicts != 0:
            raise ValueError(
                f"car bitmap {bitmap_key:016x} has conflicting material UVs"
            )
        identity = (bitmap_key, 0)
        images[identity] = decode_car_neutral_detail(
            details["bitmap"],
            list(details["palettes"].values()),
            masks,
        )
        modes[identity] = 0
        color_fits[identity] = False
        replacement_modes[identity] = REPLACEMENT_PALETTE_DETAIL
        entries.append({
            "bitmapKey": f"{bitmap_key:016x}",
            "paintBanks": len(details["palettes"]),
            "captured": bitmap_key in captured_bitmaps,
            "aliases": sorted(details["aliases"]),
            "maximumConflictingOpaqueTexels": maximum_conflicts,
        })
    return images, modes, color_fits, replacement_modes, {
        "filters": sorted(needles),
        "uniqueCarIndexedTextures": len(images),
        "authoredPaintBanks": sum(
            len(details["palettes"]) for details in grouped.values()
        ),
        "capturedCarIndexedTextures": sum(
            bitmap_key in captured_bitmaps for bitmap_key in grouped
        ),
        **capture_stats,
        "entries": entries,
    }


def palette_affine_error(source: bytes, live: bytes) -> dict[str, object]:
    """Measure one authored paint bank against a live lit 64x4 palette bank."""

    if len(source) != 512 or len(live) != 512:
        raise ValueError("car palette banks must contain 256 BGR555 words")
    source_words = np.frombuffer(source, dtype="<u2")
    live_words = np.frombuffer(live, dtype="<u2")

    def rgb(words: np.ndarray) -> np.ndarray:
        return np.stack((
            words & 31,
            (words >> 5) & 31,
            (words >> 10) & 31,
        ), axis=1).astype(np.float64)

    source_rgb = rgb(source_words)
    live_rgb = rgb(live_words)
    common = (source_words != 0) & (live_words != 0)
    if int(common.sum()) < 16:
        return {
            "rmse5": float("inf"),
            "maximumError5": float("inf"),
            "comparedColors": int(common.sum()),
            "transparencyMismatches": int(
                ((source_words == 0) != (live_words == 0)).sum()
            ),
            "scale": [1.0, 1.0, 1.0],
            "bias": [0.0, 0.0, 0.0],
        }
    predictions = np.empty_like(source_rgb[common])
    scales: list[float] = []
    biases: list[float] = []
    for channel in range(3):
        x = source_rgb[common, channel]
        y = live_rgb[common, channel]
        design = np.stack((x, np.ones_like(x)), axis=1)
        scale, bias = np.linalg.lstsq(design, y, rcond=None)[0]
        scale = float(np.clip(scale, 0.0, 4.0))
        bias = float(np.clip(bias, -31.0, 31.0))
        predictions[:, channel] = np.clip(x * scale + bias, 0.0, 31.0)
        scales.append(scale)
        biases.append(bias)
    errors = predictions - live_rgb[common]
    magnitudes = np.sqrt(np.sum(errors * errors, axis=1))
    return {
        "rmse5": round(float(np.sqrt(np.mean(errors * errors))), 6),
        "maximumError5": round(float(magnitudes.max()), 6),
        "comparedColors": int(common.sum()),
        "transparencyMismatches": int(
            ((source_words == 0) != (live_words == 0)).sum()
        ),
        "scale": [round(value, 6) for value in scales],
        "bias": [round(value, 6) for value in biases],
    }


def audit_car_palette_candidates(
    volumes: list[Path], capture_path: Path
) -> dict[str, object]:
    """Rank authored paints for every live body bitmap/palette pair."""

    _, capture_stats = captured_car_palette_masks(capture_path)
    data = capture_path.read_bytes()
    vram_offset = struct.unpack_from("<Q", data, 68)[0]
    vram = np.frombuffer(
        data, dtype="<u2", count=1024 * 512, offset=vram_offset
    ).reshape(512, 1024)
    pair_items = capture_stats["capturePairs"]
    live_banks: dict[tuple[int, int], bytes] = {}
    for item in pair_items:
        bitmap_key = int(item["bitmapKey"], 16)
        live_palette_key = int(item["paletteKey"], 16)
        palette_x, palette_y = item["paletteOrigins"][0]
        live_banks[(bitmap_key, live_palette_key)] = vram[
            palette_y:palette_y + 4,
            palette_x:palette_x + 64,
        ].tobytes()
    candidates: dict[tuple[int, int], list[dict[str, object]]] = {
        pair: [] for pair in live_banks
    }
    seen: set[tuple[int, int, int, str]] = set()
    for volume in volumes:
        for entry in read_entries(volume):
            name = entry.name.casefold()
            if not (
                name.startswith("carobj/") and
                name.endswith((".cdp.gz", ".cnp.gz"))
            ):
                continue
            texture = gzip.decompress(read_volume_member(volume, entry))
            if len(texture) != 0xB3A0 or not 1 <= texture[0] <= 16:
                continue
            bitmap_key = upload_hash(texture[0x43A0:0xB3A0], 64, 224)
            live_pairs = [pair for pair in live_banks if pair[0] == bitmap_key]
            if not live_pairs:
                continue
            for paint in range(texture[0]):
                source = texture[
                    0x20 + paint * 0x240:0x220 + paint * 0x240
                ]
                palette_key = upload_hash(source, 64, 4)
                for pair in live_pairs:
                    identity = (
                        bitmap_key, pair[1], palette_key, entry.name
                    )
                    if identity in seen:
                        continue
                    seen.add(identity)
                    candidates[pair].append({
                        "asset": f"{volume.name}:{entry.name}",
                        "paint": paint,
                        "colorId": texture[2 + paint],
                        "authoredPaletteKey": f"{palette_key:016x}",
                        **palette_affine_error(source, live_banks[pair]),
                    })
    results: list[dict[str, object]] = []
    details_by_pair = {
        (int(item["bitmapKey"], 16), int(item["paletteKey"], 16)): item
        for item in pair_items
    }
    for pair, ranked in candidates.items():
        bitmap_key, live_palette_key = pair
        item = details_by_pair[pair]
        ranked.sort(key=lambda candidate: (
            int(candidate["transparencyMismatches"]),
            float(candidate["rmse5"]),
            str(candidate["asset"]),
            int(candidate["paint"]),
        ))
        best = ranked[0] if ranked else None
        next_distinct = next((
            candidate for candidate in ranked[1:]
            if best is not None and
            candidate["authoredPaletteKey"] != best["authoredPaletteKey"]
        ), None)
        results.append({
            "bitmapKey": f"{bitmap_key:016x}",
            "livePaletteKey": f"{live_palette_key:016x}",
            "triangles": item["triangles"],
            "bestRmseMargin5": (
                None if best is None or next_distinct is None else
                round(
                    float(next_distinct["rmse5"]) - float(best["rmse5"]),
                    6,
                )
            ),
            "candidates": ranked[:5],
        })
    return {
        "capture": str(capture_path),
        "capturedPairs": len(results),
        "pairs": sorted(results, key=lambda item: item["bitmapKey"]),
    }


def captured_car_palette_masks(
    capture_path: Path,
) -> tuple[dict[tuple[int, int], np.ndarray], dict[str, object]]:
    """Recover live car bitmap/paint pairs and UV ownership from a world capture."""

    data = capture_path.read_bytes()
    if len(data) < 160 or data[:8] != b"OGTWCAP\0":
        raise ValueError("world capture header is missing")
    version, header_size = struct.unpack_from("<II", data, 8)
    triangle_count, triangle_stride = struct.unpack_from("<II", data, 44)
    triangle_offset = struct.unpack_from("<Q", data, 60)[0]
    vram_offset, vram_size = struct.unpack_from("<QQ", data, 68)
    if (
        version != 6 or header_size != 160 or triangle_stride != 384 or
        vram_size != 1024 * 512 * 2 or
        triangle_offset + triangle_count * triangle_stride > len(data) or
        vram_offset + vram_size > len(data)
    ):
        raise ValueError("unsupported or truncated world capture")
    vram = np.frombuffer(
        data, dtype="<u2", count=1024 * 512, offset=vram_offset
    ).reshape(512, 1024)
    masks: dict[tuple[int, int], list[Image.Image]] = {}
    pair_details: dict[tuple[int, int], dict[str, object]] = {}
    textured_vehicle_triangles = 0

    def rectangle_key(x: int, y: int, width: int, height: int) -> int:
        if x < 0 or y < 0 or x + width > 1024 or y + height > 512:
            return 0
        return upload_hash(
            vram[y:y + height, x:x + width].tobytes(), width, height
        )

    for index in range(triangle_count):
        record = triangle_offset + index * triangle_stride
        primitive_flags = struct.unpack_from("<I", data, record)[0]
        texture_page, clut = struct.unpack_from("<HH", data, record + 4)
        object_kind = struct.unpack_from("<I", data, record + 32)[0]
        if object_kind != 2 or not primitive_flags & 1:
            continue
        mode = (texture_page >> 7) & 3
        if mode != 0:
            continue
        page_x = (texture_page & 15) * 64
        page_y = ((texture_page >> 4) & 1) * 256
        bitmap_key = rectangle_key(page_x, page_y, 64, 224)
        clut_x = (clut & 63) * 16
        clut_y = (clut >> 6) & 511
        bank_x = clut_x & ~63
        bank_y = clut_y & ~3
        palette_key = rectangle_key(bank_x, bank_y, 64, 4)
        material = (clut_y - bank_y) * 4 + (clut_x - bank_x) // 16
        if not 0 <= material < 16:
            continue
        pair = (bitmap_key, palette_key)
        details = pair_details.setdefault(pair, {
            "triangles": 0,
            "textureOrigins": set(),
            "paletteOrigins": set(),
        })
        details["triangles"] = int(details["triangles"]) + 1
        details["textureOrigins"].add((page_x, page_y))
        details["paletteOrigins"].add((bank_x, bank_y))
        pair_masks = masks.setdefault(
            pair, [Image.new("1", (256, 224)) for _ in range(16)]
        )
        texture_mask_x, texture_mask_y, texture_offset_x, texture_offset_y = (
            struct.unpack_from("<4h", data, record + 20)
        )
        uvs: list[tuple[int, int]] = []
        for vertex in range(3):
            vertex_record = record + 88 + vertex * 96
            u, v = struct.unpack_from("<2h", data, vertex_record + 12)
            u = ((u & ~(texture_mask_x * 8)) |
                 ((texture_offset_x & texture_mask_x) * 8)) & 255
            v = ((v & ~(texture_mask_y * 8)) |
                 ((texture_offset_y & texture_mask_y) * 8)) & 255
            uvs.append((u, v))
        ImageDraw.Draw(pair_masks[material]).polygon(uvs, fill=1)
        textured_vehicle_triangles += 1
    result = {
        pair: np.stack(
            [np.asarray(mask, dtype=bool) for mask in pair_masks], axis=0
        )
        for pair, pair_masks in masks.items()
    }
    return result, {
        "texturedVehicleTriangles": textured_vehicle_triangles,
        "capturedCarPairs": len(result),
        "capturePairs": [
            {
                "bitmapKey": f"{pair[0]:016x}",
                "paletteKey": f"{pair[1]:016x}",
                "triangles": details["triangles"],
                "textureOrigins": sorted(details["textureOrigins"]),
                "paletteOrigins": sorted(details["paletteOrigins"]),
                "coveredTexels": int(np.any(result[pair], axis=0).sum()),
            }
            for pair, details in sorted(pair_details.items())
        ],
    }


def audit_car_composites(
    volumes: list[Path], filters: list[str], world_capture: Path | None
) -> dict[str, object]:
    needles = {value.casefold() for value in filters}
    capture_masks: dict[tuple[int, int], np.ndarray] = {}
    capture_stats: dict[str, object] = {}
    if world_capture is not None:
        capture_masks, capture_stats = captured_car_palette_masks(world_capture)
    packages: list[dict[str, object]] = []
    seen: set[str] = set()
    for volume in volumes:
        entries = {entry.name.casefold(): entry for entry in read_entries(volume)}
        for name, entry in entries.items():
            if not (
                name.startswith("carobj/") and
                name.endswith((".cdp.gz", ".cnp.gz"))
            ):
                continue
            stem = Path(name).name.split(".", 1)[0]
            if needles and stem not in needles:
                continue
            model_suffix = ".cdo.gz" if name.endswith(".cdp.gz") else ".cno.gz"
            model_name = f"carobj/{stem}{model_suffix}"
            model_entry = entries.get(model_name)
            if model_entry is None:
                raise ValueError(f"missing car model for {entry.name}")
            texture = gzip.decompress(read_volume_member(volume, entry))
            model = gzip.decompress(read_volume_member(volume, model_entry))
            digest = hashlib.sha256(texture + model).hexdigest()
            if digest in seen:
                continue
            seen.add(digest)
            masks, model_stats = car_model_palette_masks(model)
            bitmap_key = upload_hash(texture[0x43A0:0xB3A0], 64, 224)
            paints: list[dict[str, int]] = []
            captured_paints = 0
            for paint in range(texture[0]):
                palette = texture[
                    0x20 + paint * 0x240:0x220 + paint * 0x240
                ]
                palette_key = upload_hash(palette, 64, 4)
                live_masks = capture_masks.get((bitmap_key, palette_key))
                combined = masks if live_masks is None else masks | live_masks
                stats = audit_car_composite(texture, paint, combined)
                stats["capturedRuntimeTexels"] = (
                    0 if live_masks is None else int(np.any(live_masks, axis=0).sum())
                )
                if live_masks is not None:
                    captured_paints += 1
                paints.append(stats)
            packages.append({
                "asset": f"{volume.name}:{entry.name}",
                "paintCount": texture[0],
                "capturedPaints": captured_paints,
                **model_stats,
                "maximumUnownedOpaqueTexels": max(
                    item["unownedOpaqueTexels"] for item in paints
                ),
                "maximumConflictingOpaqueTexels": max(
                    item["conflictingOpaqueTexels"] for item in paints
                ),
                "paints": paints,
            })
    if not packages:
        raise ValueError("no car texture/model pairs matched the requested filters")
    return {
        "filters": sorted(needles),
        "uniquePackages": len(packages),
        "totalPaints": sum(item["paintCount"] for item in packages),
        "packagesWithConflicts": sum(
            item["maximumConflictingOpaqueTexels"] != 0
            for item in packages
        ),
        **capture_stats,
        "packages": packages,
    }


def offline_inventory(volumes: list[Path]) -> dict[str, object]:
    """Enumerate every authored texture container without running the game."""

    unique_cars: dict[str, tuple[int, int, set[str]]] = {}
    unique_car_bitmaps: set[str] = set()
    unique_car_palette_banks: set[str] = set()
    unique_car_visuals: set[tuple[str, str]] = set()
    unique_courses: dict[str, set[str]] = {}
    unique_tim: dict[str, set[str]] = {}
    role_counts: Counter[str] = Counter()
    volume_entry_counts: Counter[str] = Counter()
    for volume in volumes:
        if not volume.is_file():
            raise FileNotFoundError(f"GT2 volume not found: {volume}")
        for entry in read_entries(volume):
            name = entry.name.lower()
            identity = f"{volume.name}:{entry.name}"
            if name.startswith("carobj/") and name.endswith((".cdp.gz", ".cnp.gz")):
                payload = gzip.decompress(read_volume_member(volume, entry))
                if len(payload) != 0xB3A0 or not 1 <= payload[0] <= 16:
                    raise ValueError(f"malformed native car texture: {identity}")
                digest = hashlib.sha256(payload).hexdigest()
                bitmap_digest = hashlib.sha256(payload[0x43A0:0xB3A0]).hexdigest()
                unique_car_bitmaps.add(bitmap_digest)
                for paint_index in range(payload[0]):
                    palette_begin = 0x20 + paint_index * 0x240
                    palette_digest = hashlib.sha256(
                        payload[palette_begin:palette_begin + 0x240]
                    ).hexdigest()
                    unique_car_palette_banks.add(palette_digest)
                    unique_car_visuals.add((bitmap_digest, palette_digest))
                if digest not in unique_cars:
                    unique_cars[digest] = (payload[0], len(payload), {identity})
                else:
                    unique_cars[digest][2].add(identity)
                role_counts["carDay" if name.endswith(".cdp.gz") else "carNight"] += 1
                volume_entry_counts[volume.name] += 1
            elif name.startswith("crsobj/") and name.endswith(".trp.gz"):
                payload = gzip.decompress(read_volume_member(volume, entry))
                digest = hashlib.sha256(payload).hexdigest()
                unique_courses.setdefault(digest, set()).add(identity)
                role_counts["courseTrp"] += 1
                volume_entry_counts[volume.name] += 1
            elif name.endswith(".tim") or name.endswith(".tim.gz"):
                payload = read_volume_member(volume, entry)
                if name.endswith(".gz"):
                    payload = gzip.decompress(payload)
                digest = hashlib.sha256(payload).hexdigest()
                unique_tim.setdefault(digest, set()).add(identity)
                role_counts["standaloneTim"] += 1
                volume_entry_counts[volume.name] += 1
    paint_counts = Counter(item[0] for item in unique_cars.values())
    return {
        "volumes": [str(volume) for volume in volumes],
        "uniqueCarTexturePackages": len(unique_cars),
        "uniqueCarPaints": sum(item[0] for item in unique_cars.values()),
        "uniqueCarIndexedBitmaps": len(unique_car_bitmaps),
        "uniqueCarPaletteBanks": len(unique_car_palette_banks),
        "uniqueCarBitmapPalettePairs": len(unique_car_visuals),
        "carPaintCountHistogram": dict(sorted(paint_counts.items())),
        "uniqueCourseTexturePackages": len(unique_courses),
        "uniqueStandaloneTimImages": len(unique_tim),
        "sourceRoleCounts": dict(role_counts),
        "textureEntriesByVolume": dict(volume_entry_counts),
        "carTextureAliases": sum(len(item[2]) for item in unique_cars.values()),
        "courseTextureAliases": sum(len(item) for item in unique_courses.values()),
        "standaloneTimAliases": sum(len(item) for item in unique_tim.values()),
    }


def bleed_transparent_rgb(image: Image.Image, rounds: int = PAD) -> np.ndarray:
    """Extend visible RGB into word-zero texels so filtering cannot make halos."""

    rgba = np.asarray(image.convert("RGBA"), dtype=np.uint8)
    rgb = rgba[:, :, :3].copy()
    known = np.any(rgba != 0, axis=2)
    height, width = known.shape
    for _ in range(rounds):
        missing = ~known
        if not np.any(missing):
            break
        padded_rgb = np.pad(rgb, ((1, 1), (1, 1), (0, 0)), mode="edge")
        padded_known = np.pad(known, ((1, 1), (1, 1)), mode="constant")
        changed = np.zeros_like(known)
        for dy, dx in (
            (-1, 0), (1, 0), (0, -1), (0, 1),
            (-1, -1), (-1, 1), (1, -1), (1, 1),
        ):
            source_known = padded_known[
                1 + dy:1 + dy + height,
                1 + dx:1 + dx + width,
            ]
            take = missing & ~changed & source_known
            if np.any(take):
                source_rgb = padded_rgb[
                    1 + dy:1 + dy + height,
                    1 + dx:1 + dx + width,
                ]
                rgb[take] = source_rgb[take]
                changed[take] = True
        if not np.any(changed):
            break
        known |= changed
    return rgb


def prepare_sources(
    images: dict[AssetIdentity, Image.Image], source_dir: Path
) -> None:
    source_dir.mkdir(parents=True, exist_ok=True)
    for stale in source_dir.glob("*.png"):
        stale.unlink()
    for identity, image in sorted(images.items()):
        rgb = bleed_transparent_rgb(image)
        # Reflected padding visibly repeats signs/logos in neural intermediates
        # and gives the model false mirrored context at texture boundaries.
        # Edge extension is neutral for non-tiling art. Proven tileable assets
        # can receive wrap padding later only when runtime UV evidence exists.
        padded = np.pad(rgb, ((PAD, PAD), (PAD, PAD), (0, 0)), mode="edge")
        Image.fromarray(padded, "RGB").save(
            source_dir / f"{asset_stem(identity)}.png"
        )


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        while block := stream.read(1024 * 1024):
            digest.update(block)
    return digest.hexdigest()


def png_inventory(directory: Path) -> dict[str, dict[str, object]]:
    inventory: dict[str, dict[str, object]] = {}
    for path in sorted(directory.glob("*.png")):
        with Image.open(path) as image:
            size = list(image.size)
        inventory[path.name] = {
            "sha256": sha256_file(path),
            "size": size,
        }
    return inventory


def realesrgan_identity(executable: Path) -> dict[str, object]:
    model_directory = executable.parent / "models"
    graph = model_directory / f"{REALESRGAN_MODEL_FILES}.param"
    weights = model_directory / f"{REALESRGAN_MODEL_FILES}.bin"
    for required in (executable, graph, weights):
        if not required.is_file():
            raise FileNotFoundError(f"Real-ESRGAN component missing: {required}")
    return {
        "executable": str(executable),
        "executableSha256": sha256_file(executable),
        "model": REALESRGAN_MODEL,
        "graphSha256": sha256_file(graph),
        "weightsSha256": sha256_file(weights),
        "scale": SCALE,
    }


def validate_neural_outputs(
    inputs: dict[str, dict[str, object]],
    generated_dir: Path,
) -> dict[str, dict[str, object]]:
    outputs = png_inventory(generated_dir)
    if set(outputs) != set(inputs):
        missing = sorted(set(inputs) - set(outputs))
        unexpected = sorted(set(outputs) - set(inputs))
        raise ValueError(
            "Real-ESRGAN output inventory mismatch: "
            f"missing={missing}, unexpected={unexpected}"
        )
    for name, source in inputs.items():
        expected = [int(source["size"][0]) * SCALE,
                    int(source["size"][1]) * SCALE]
        if outputs[name]["size"] != expected:
            raise ValueError(
                f"Real-ESRGAN output has the wrong size for {name}: "
                f"{outputs[name]['size']}, expected {expected}"
            )
    return outputs


def run_or_verify_realesrgan(
    source_dir: Path,
    generated_dir: Path,
    executable: Path,
    reuse: bool,
) -> dict[str, object]:
    """Run the requested model or verify a cryptographically exact prior run.

    A clean output directory plus one successful Real-ESRGAN process is the
    only generation path. Reuse is allowed solely when every input, model
    component, and output hash matches the recorded completed invocation.
    """

    identity = realesrgan_identity(executable)
    inputs = png_inventory(source_dir)
    if not inputs:
        raise ValueError("Real-ESRGAN input inventory is empty")
    record_path = generated_dir.parent / "esrgan_run.json"
    command = [
        str(executable),
        "-i", str(source_dir),
        "-o", str(generated_dir),
        "-n", REALESRGAN_MODEL,
        "-s", str(SCALE),
        "-t", str(REALESRGAN_TILE),
        "-j", "1:1:1",
        "-f", "png",
    ]
    if reuse:
        if not record_path.is_file():
            raise ValueError(
                "--reuse-esrgan requires a completed cryptographic run record"
            )
        record = json.loads(record_path.read_text(encoding="utf-8"))
        outputs = validate_neural_outputs(inputs, generated_dir)
        if (
            record.get("status") != "complete" or
            record.get("identity") != identity or
            record.get("inputs") != inputs or
            record.get("outputs") != outputs or
            record.get("command") != command
        ):
            raise ValueError(
                "--reuse-esrgan rejected stale inputs, model, command, or outputs"
            )
        return record

    if generated_dir.exists():
        shutil.rmtree(generated_dir)
    generated_dir.mkdir(parents=True)
    started = datetime.now(timezone.utc).isoformat()
    print("Running:", " ".join(command), flush=True)
    completed = subprocess.run(
        command,
        check=False,
        cwd=executable.parent,
        capture_output=True,
        text=True,
    )
    log_path = generated_dir.parent / "esrgan.log"
    log_path.write_text(
        completed.stdout + completed.stderr,
        encoding="utf-8",
    )
    if completed.returncode != 0:
        raise subprocess.CalledProcessError(
            completed.returncode,
            command,
            output=completed.stdout,
            stderr=completed.stderr,
        )
    outputs = validate_neural_outputs(inputs, generated_dir)
    record: dict[str, object] = {
        "format": 1,
        "status": "complete",
        "startedUtc": started,
        "completedUtc": datetime.now(timezone.utc).isoformat(),
        "identity": identity,
        "command": command,
        "inputs": inputs,
        "outputs": outputs,
        "logSha256": sha256_file(log_path),
    }
    record_path.write_text(
        json.dumps(record, indent=2) + "\n", encoding="utf-8"
    )
    return record


def write_pack(
    images: dict[AssetIdentity, Image.Image],
    pixel_modes: dict[AssetIdentity, int],
    color_fits: dict[AssetIdentity, bool],
    replacement_modes: dict[AssetIdentity, str],
    generated_dir: Path,
    output: Path,
    neural_run: dict[str, object],
    fidelity_exemptions: set[int] | None = None,
) -> None:
    fidelity_exemptions = fidelity_exemptions or set()
    image_dir = output / "images"
    preview_dir = output / "cropped_neural_png"
    image_dir.mkdir(parents=True, exist_ok=True)
    preview_dir.mkdir(parents=True, exist_ok=True)
    for stale in image_dir.glob("*.dds"):
        stale.unlink()
    for stale in preview_dir.glob("*.png"):
        stale.unlink()
    entries: list[dict[str, object]] = []
    run_inputs = neural_run.get("inputs", {})
    run_outputs = neural_run.get("outputs", {})
    for identity, source in sorted(images.items()):
        bitmap_key, palette_key = identity
        stem = asset_stem(identity)
        pixel_mode = pixel_modes.get(identity)
        if pixel_mode not in (0, 1):
            raise ValueError(f"missing indexed pixel mode for {stem}")
        if identity not in color_fits:
            raise ValueError(f"missing color-fit policy for {stem}")
        replacement_mode = replacement_modes.get(identity)
        if replacement_mode not in (
            REPLACEMENT_RGB, REPLACEMENT_PALETTE_DETAIL
        ):
            raise ValueError(f"missing replacement mode for {stem}")
        if replacement_mode == REPLACEMENT_PALETTE_DETAIL and (
            palette_key != 0 or color_fits[identity]
        ):
            raise ValueError(
                f"palette detail must use the shared bitmap identity for {stem}"
            )
        generated_path = generated_dir / f"{stem}.png"
        if not generated_path.is_file():
            raise FileNotFoundError(f"Real-ESRGAN output missing: {generated_path}")
        generated = Image.open(generated_path).convert("RGB")
        expected_padded = (
            (source.width + PAD * 2) * SCALE,
            (source.height + PAD * 2) * SCALE,
        )
        if generated.size != expected_padded:
            raise ValueError(
                f"unexpected Real-ESRGAN size for {generated_path.name}: "
                f"{generated.size}, expected {expected_padded}"
            )
        crop = PAD * SCALE
        neural = generated.crop((
            crop,
            crop,
            crop + source.width * SCALE,
            crop + source.height * SCALE,
        ))
        visible = np.asarray(source.getchannel("A"), dtype=np.uint8) != 0
        downsampled = np.asarray(
            neural.resize(source.size, Image.Resampling.LANCZOS),
            dtype=np.float32,
        )
        authored = np.asarray(source.convert("RGB"), dtype=np.float32)
        compare = visible if np.any(visible) else np.ones(
            (source.height, source.width), dtype=bool
        )
        roundtrip_mae = float(np.abs(
            authored[compare] - downsampled[compare]
        ).mean())
        fidelity_exempt = bitmap_key in fidelity_exemptions
        if roundtrip_mae > MAX_ROUNDTRIP_MAE and not fidelity_exempt:
            raise ValueError(
                f"Real-ESRGAN changed {stem} beyond the fidelity gate: "
                f"MAE {roundtrip_mae:.3f} > {MAX_ROUNDTRIP_MAE:.3f}"
            )
        preview_relative = Path("cropped_neural_png") / f"{stem}.png"
        neural.save(output / preview_relative)
        if replacement_mode == REPLACEMENT_RGB:
            packed = neural.convert("RGBA")
        else:
            neural_pixels = np.asarray(neural, dtype=np.uint8)
            neural_luma = np.clip(np.rint(
                neural_pixels[:, :, 0].astype(np.float32) * 0.2126 +
                neural_pixels[:, :, 1].astype(np.float32) * 0.7152 +
                neural_pixels[:, :, 2].astype(np.float32) * 0.0722
            ), 0, 255).astype(np.uint8)
            baseline_source = Image.fromarray(
                bleed_transparent_rgb(source), "RGB"
            ).convert("L")
            baseline = np.asarray(
                baseline_source.resize(neural.size, Image.Resampling.BILINEAR),
                dtype=np.uint8,
            )
            channels = np.empty((neural.height, neural.width, 4), dtype=np.uint8)
            channels[:, :, 0] = neural_luma
            channels[:, :, 1] = baseline
            channels[:, :, 2] = neural_luma
            channels[:, :, 3] = 255
            packed = Image.fromarray(channels, "RGBA")
        stp = source.getchannel("A").resize(packed.size, Image.Resampling.NEAREST)
        packed.putalpha(stp)
        relative = Path("images") / f"{stem}.dds"
        packed.save(output / relative)
        source_name = f"{stem}.png"
        if source_name not in run_inputs or source_name not in run_outputs:
            raise ValueError(f"neural provenance missing for {source_name}")
        entry: dict[str, object] = {
            "key": f"{bitmap_key:016x}",
            "image": relative.as_posix(),
            "x": 0,
            "y": 0,
            "width": packed.width,
            "height": packed.height,
            "sourceWidth": source.width,
            "sourceHeight": source.height,
            "pixelMode": pixel_mode,
            "colorFit": 1 if color_fits[identity] else 0,
            "replacementMode": replacement_mode,
            "roundtripMae": round(roundtrip_mae, 6),
            "fidelityExempt": 1 if fidelity_exempt else 0,
            "neuralPreview": preview_relative.as_posix(),
            "neuralPreviewSha256": sha256_file(output / preview_relative),
            "neuralSourceSha256": run_inputs[source_name]["sha256"],
            "neuralOutputSha256": run_outputs[source_name]["sha256"],
            "ddsSha256": sha256_file(output / relative),
        }
        if palette_key != 0:
            entry["paletteKey"] = f"{palette_key:016x}"
        entries.append(entry)
    manifest = {
        "format": PACK_FORMAT,
        "generator": (
            f"pure Real-ESRGAN {REALESRGAN_MODEL}; "
            "one 4x DDS per independently authored GT2 bitmap"
        ),
        "scale": SCALE,
        "neuralRun": neural_run["identity"],
        "entries": entries,
    }
    (output / "manifest.json").write_text(
        json.dumps(manifest, indent=2) + "\n", encoding="utf-8"
    )
    (output / "esrgan-provenance.json").write_text(
        json.dumps(neural_run, indent=2) + "\n", encoding="utf-8"
    )


def validate_pack(output: Path, source_count: int) -> None:
    manifest = json.loads((output / "manifest.json").read_text(encoding="utf-8"))
    if (
        manifest.get("format") != PACK_FORMAT or
        manifest.get("scale") != SCALE or
        len(manifest.get("entries", [])) != source_count
    ):
        raise ValueError("generated manifest is incomplete")
    provenance_path = output / "esrgan-provenance.json"
    if not provenance_path.is_file():
        raise ValueError("generated pack has no Real-ESRGAN provenance")
    provenance = json.loads(provenance_path.read_text(encoding="utf-8"))
    if (
        provenance.get("status") != "complete" or
        provenance.get("identity") != manifest.get("neuralRun")
    ):
        raise ValueError("generated pack has invalid Real-ESRGAN provenance")
    images = [entry.get("image") for entry in manifest["entries"]]
    if len(set(images)) != len(images):
        raise ValueError("each texture identity must have its own DDS image")
    identities: set[tuple[str, str]] = set()
    for entry in manifest["entries"]:
        if entry.get("pixelMode") not in (0, 1):
            raise ValueError("generated manifest has an invalid pixel mode")
        if entry.get("colorFit") not in (0, 1):
            raise ValueError("generated manifest has an invalid color-fit policy")
        replacement_mode = entry.get("replacementMode")
        if replacement_mode not in (
            REPLACEMENT_RGB, REPLACEMENT_PALETTE_DETAIL
        ):
            raise ValueError("generated manifest has an invalid replacement mode")
        if replacement_mode == REPLACEMENT_PALETTE_DETAIL and (
            entry.get("colorFit") != 0 or entry.get("paletteKey") is not None
        ):
            raise ValueError("palette detail must retain GT2's live palette")
        roundtrip_mae = entry.get("roundtripMae")
        fidelity_exempt = entry.get("fidelityExempt", 0)
        if fidelity_exempt not in (0, 1):
            raise ValueError("generated manifest has an invalid fidelity exemption")
        if not isinstance(roundtrip_mae, (int, float)) or (
            roundtrip_mae > MAX_ROUNDTRIP_MAE and fidelity_exempt != 1
        ):
            raise ValueError("generated manifest failed the fidelity gate")
        identity = (entry.get("key", ""), entry.get("paletteKey", ""))
        if identity in identities:
            raise ValueError("generated manifest has a duplicate texture identity")
        identities.add(identity)
        path = (output / entry["image"]).resolve()
        if output.resolve() not in path.parents or path.suffix.lower() != ".dds":
            raise ValueError(f"pack entry escapes output: {entry['image']}")
        with Image.open(path) as image:
            if image.size != (entry["width"], entry["height"]):
                raise ValueError(f"DDS dimensions do not match: {entry['image']}")
        if sha256_file(path) != entry.get("ddsSha256"):
            raise ValueError(f"DDS hash does not match: {entry['image']}")
        preview = (output / entry.get("neuralPreview", "")).resolve()
        if (
            output.resolve() not in preview.parents or
            preview.suffix.lower() != ".png" or
            not preview.is_file() or
            sha256_file(preview) != entry.get("neuralPreviewSha256")
        ):
            raise ValueError(f"neural preview does not match: {entry['image']}")
        with Image.open(preview) as image:
            if image.size != (entry["width"], entry["height"]):
                raise ValueError(
                    f"neural preview dimensions do not match: {entry['image']}"
                )
        source_name = Path(entry["image"]).stem + ".png"
        if (
            provenance.get("inputs", {}).get(source_name, {}).get("sha256") !=
                entry.get("neuralSourceSha256") or
            provenance.get("outputs", {}).get(source_name, {}).get("sha256") !=
                entry.get("neuralOutputSha256")
        ):
            raise ValueError(f"neural provenance does not match: {entry['image']}")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--dump", type=Path, action="append")
    parser.add_argument(
        "--volume", type=Path, action="append",
        help="scan a materialized GT2 VOL directly (repeatable)",
    )
    parser.add_argument(
        "--course-filter",
        help="build individual TIM assets from one exact crsobj TRP stem",
    )
    parser.add_argument(
        "--match-upload-trace", type=Path,
        help="match an opt-in runtime upload log to offline car assets",
    )
    parser.add_argument(
        "--match-course-upload-trace", type=Path,
        help="rank authored crsobj TRP packages against an opt-in upload log",
    )
    parser.add_argument(
        "--audit-car-composites", action="store_true",
        help="audit whether car paints can be represented by one decoded image",
    )
    parser.add_argument(
        "--audit-car-palette-candidates", action="store_true",
        help="rank authored paints for car palettes in a v6 world capture",
    )
    parser.add_argument(
        "--car-filter", action="append", default=[],
        help=(
            "include an exact five-character car stem in a pack, or restrict "
            "the car composite audit (repeatable)"
        ),
    )
    parser.add_argument(
        "--world-capture", type=Path,
        help="merge generated-wheel/live vehicle UV ownership into a car audit",
    )
    parser.add_argument("--realesrgan", type=Path, default=DEFAULT_REALESRGAN)
    parser.add_argument("--work", type=Path, default=Path("work/gt2-texture-pack"))
    parser.add_argument(
        "--output", type=Path,
        default=Path("work/gt2-texture-pack/enhanced_textures_4x"),
    )
    parser.add_argument("--inventory-only", action="store_true")
    parser.add_argument("--prepare-only", action="store_true")
    parser.add_argument("--reuse-esrgan", action="store_true")
    parser.add_argument(
        "--allow-graphic-key", action="append", default=[],
        help=(
            "allow a reviewed text/signage/decorative bitmap to exceed the "
            "natural-texture fidelity limit (16-digit hexadecimal, repeatable)"
        ),
    )
    args = parser.parse_args()

    fidelity_exemptions: set[int] = set()
    for key_text in args.allow_graphic_key:
        try:
            key = int(key_text, 16)
        except ValueError:
            parser.error(f"invalid --allow-graphic-key: {key_text}")
        if len(key_text) != 16 or key == 0:
            parser.error(
                "--allow-graphic-key must be a nonzero 16-digit hexadecimal key"
            )
        fidelity_exemptions.add(key)

    volumes = [path.resolve() for path in (args.volume or [])]
    if args.match_course_upload_trace:
        if not volumes:
            parser.error(
                "--match-course-upload-trace requires at least one --volume"
            )
        matches = match_course_upload_trace(
            volumes, args.match_course_upload_trace.resolve()
        )
        print(json.dumps(matches, indent=2), flush=True)
        return 0
    if args.match_upload_trace:
        if not volumes:
            parser.error("--match-upload-trace requires at least one --volume")
        matches = match_car_upload_trace(
            volumes, args.match_upload_trace.resolve()
        )
        print(json.dumps(matches, indent=2), flush=True)
        return 0
    if args.audit_car_composites:
        if not volumes:
            parser.error("--audit-car-composites requires at least one --volume")
        audit = audit_car_composites(
            volumes,
            args.car_filter,
            args.world_capture.resolve() if args.world_capture else None,
        )
        print(json.dumps(audit, indent=2), flush=True)
        return 0
    if args.audit_car_palette_candidates:
        if not volumes or not args.world_capture:
            parser.error(
                "--audit-car-palette-candidates requires --volume and "
                "--world-capture"
            )
        audit = audit_car_palette_candidates(
            volumes, args.world_capture.resolve()
        )
        print(json.dumps(audit, indent=2), flush=True)
        return 0
    if (
        volumes and args.inventory_only and
        not args.course_filter and not args.car_filter
    ):
        inventory = offline_inventory(volumes)
        print(json.dumps(inventory, indent=2), flush=True)
        inventory_path = args.work.resolve() / "offline_inventory.json"
        inventory_path.parent.mkdir(parents=True, exist_ok=True)
        inventory_path.write_text(
            json.dumps(inventory, indent=2) + "\n", encoding="utf-8"
        )
        return 0
    if args.dump and (args.course_filter or args.car_filter):
        parser.error("runtime dumps cannot be combined with authored assets")
    if (args.course_filter or args.car_filter) and not volumes:
        parser.error("authored texture selection requires at least one --volume")

    images: dict[AssetIdentity, Image.Image] = {}
    pixel_modes: dict[AssetIdentity, int] = {}
    color_fits: dict[AssetIdentity, bool] = {}
    replacement_modes: dict[AssetIdentity, str] = {}
    inventories: dict[str, object] = {}
    if args.course_filter:
        course_images, course_modes, course_inventory = read_course_assets(
            volumes, args.course_filter
        )
        for key, image in course_images.items():
            identity = (key, 0)
            images[identity] = image
            pixel_modes[identity] = course_modes[key]
            color_fits[identity] = True
            replacement_modes[identity] = REPLACEMENT_RGB
        inventories["course"] = course_inventory
    if args.car_filter:
        (
            car_images,
            car_modes,
            car_color_fits,
            car_replacement_modes,
            car_inventory,
        ) = read_car_assets(
                volumes,
                args.car_filter,
                args.world_capture.resolve() if args.world_capture else None,
            )
        for identity, image in car_images.items():
            prior = images.get(identity)
            if prior is not None and prior.tobytes() != image.tobytes():
                raise ValueError(
                    f"texture identity collision for {asset_stem(identity)}"
                )
            images[identity] = image
            pixel_modes[identity] = car_modes[identity]
            color_fits[identity] = car_color_fits[identity]
            replacement_modes[identity] = car_replacement_modes[identity]
        inventories["cars"] = car_inventory
    if args.dump:
        if not args.inventory_only:
            parser.error(
                "runtime page dumps are inspection-only; build from authored "
                "assets with --course-filter"
            )
        dumped = read_dumps([path.resolve() for path in args.dump])
        images = {(key, 0): image for key, image in dumped.items()}
    if not images:
        parser.error(
            "an authored selector such as --course-filter or --car-filter is required"
        )

    print(json.dumps(inventories, indent=2), flush=True)
    inventory_path = args.work.resolve() / "selected_inventory.json"
    inventory_path.parent.mkdir(parents=True, exist_ok=True)
    inventory_path.write_text(
        json.dumps(inventories, indent=2) + "\n", encoding="utf-8"
    )
    source_pixels = sum(image.width * image.height for image in images.values())
    print(
        f"GT2 texture inventory: {len(images)} individual textures, "
        f"{source_pixels:,} source texels",
        flush=True,
    )
    if args.inventory_only:
        return 0

    work = args.work.resolve()
    source_dir = work / "source_padded"
    generated_dir = work / "esrgan"
    if source_dir.exists():
        shutil.rmtree(source_dir)
    prepare_sources(images, source_dir)
    if args.prepare_only:
        print(f"Prepared padded RGB inputs in {source_dir}", flush=True)
        return 0

    executable = args.realesrgan.resolve()
    neural_run = run_or_verify_realesrgan(
        source_dir,
        generated_dir,
        executable,
        args.reuse_esrgan,
    )

    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    write_pack(
        images,
        pixel_modes,
        color_fits,
        replacement_modes,
        generated_dir,
        output,
        neural_run,
        fidelity_exemptions,
    )
    validate_pack(output, len(images))
    print(
        f"PASS: wrote {len(images)} verified 4x DDS replacements to {output}",
        flush=True,
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
