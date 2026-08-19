#!/usr/bin/env python3
"""Render the retail GT2 arc_topmenu_usa packed TIM sprites."""

from __future__ import annotations

import argparse
import gzip
import pathlib
import struct

from PIL import Image, ImageDraw


def psx_rgba(color: int, transparent_zero: bool) -> tuple[int, int, int, int]:
    if transparent_zero and color == 0:
        return 0, 0, 0, 0
    red = (color & 0x1F) * 255 // 31
    green = ((color >> 5) & 0x1F) * 255 // 31
    blue = ((color >> 10) & 0x1F) * 255 // 31
    return red, green, blue, 255


def decode_16bpp_tim(data: bytes, transparent_zero: bool) -> Image.Image:
    if len(data) < 20 or struct.unpack_from("<II", data, 0) != (0x10, 2):
        raise ValueError("expected a native 16-bit PlayStation TIM")
    block_size, _x, _y, width, height = struct.unpack_from("<I4H", data, 8)
    if block_size != 12 + width * height * 2 or len(data) < 8 + block_size:
        raise ValueError("malformed 16-bit TIM image block")
    colors = struct.unpack_from(f"<{width * height}H", data, 20)
    image = Image.new("RGBA", (width, height))
    image.putdata([psx_rgba(color, transparent_zero) for color in colors])
    return image


def packed_members(data: bytes) -> list[tuple[int, bytes]]:
    offsets: list[int] = []
    cursor = 0
    while True:
        cursor = data.find(b"\x1f\x8b\x08", cursor)
        if cursor < 0:
            break
        offsets.append(cursor)
        cursor += 1
    members: list[tuple[int, bytes]] = []
    for index, offset in enumerate(offsets):
        end = offsets[index + 1] if index + 1 < len(offsets) else len(data)
        members.append((offset, gzip.decompress(data[offset:end])))
    return members


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("pack", type=pathlib.Path)
    parser.add_argument("output", type=pathlib.Path)
    args = parser.parse_args()

    args.output.mkdir(parents=True, exist_ok=True)
    decoded: list[tuple[int, Image.Image]] = []
    for index, (offset, member) in enumerate(packed_members(args.pack.read_bytes())):
        image = decode_16bpp_tim(member, transparent_zero=index >= 4)
        path = args.output / f"sprite-{index:02d}-0x{offset:05x}.png"
        image.save(path)
        decoded.append((offset, image))

    sheet = Image.new("RGB", (1100, 670), (24, 28, 34))
    draw = ImageDraw.Draw(sheet)
    for index, (offset, image) in enumerate(decoded[:4]):
        x = 12 + (index % 2) * 540
        y = 28 + (index // 2) * 145
        sheet.paste(image.convert("RGB"), (x, y))
        draw.text((x, y - 18), f"sprite {index} @ 0x{offset:x}", fill="white")
    for index, (offset, image) in enumerate(decoded[4:], start=4):
        x = 12 + ((index - 4) % 2) * 540
        y = 340 + ((index - 4) // 2) * 72
        checker = Image.new("RGB", image.size, (55, 59, 67))
        sheet.paste(checker, (x, y))
        sheet.paste(image, (x, y), image)
        draw.text(
            (x + 155, y + 7),
            f"sprite {index} @ 0x{offset:x}",
            fill="white",
        )
    sheet.save(args.output / "contact-sheet.png")
    print(f"rendered {len(decoded)} retail sprites to {args.output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
