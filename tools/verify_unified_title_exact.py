#!/usr/bin/env python3
"""Verify four live unified-title captures against archive-derived BGR555."""

from __future__ import annotations

import argparse
import struct
from pathlib import Path


WIDTH = 512
HEIGHT = 480
PANEL_PIXELS = WIDTH * HEIGHT
HEADER = struct.Struct("<8sIII")


def read_ppm(path: Path) -> bytes:
    data = path.read_bytes()
    header = f"P6\n{WIDTH} {HEIGHT}\n255\n".encode("ascii")
    if not data.startswith(header) or len(data) != len(header) + PANEL_PIXELS * 3:
        raise ValueError(f"unexpected 512x480 P6 capture: {path}")
    return data[len(header) :]


def expected_rgb(panel: tuple[int, ...]) -> bytes:
    return bytes(
        channel
        for pixel in panel
        for channel in (
            (pixel & 0x1F) << 3,
            ((pixel >> 5) & 0x1F) << 3,
            ((pixel >> 10) & 0x1F) << 3,
        )
    )


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--asset", type=Path, required=True)
    parser.add_argument("captures", type=Path, nargs=4)
    args = parser.parse_args()

    data = args.asset.read_bytes()
    magic, width, height, count = HEADER.unpack_from(data)
    expected_size = HEADER.size + PANEL_PIXELS * 2 * 4
    if (
        (magic, width, height, count) != (b"GT2TITLE", WIDTH, HEIGHT, 4)
        or len(data) != expected_size
    ):
        raise ValueError(f"invalid exact title asset: {args.asset}")

    for state, capture_path in enumerate(args.captures):
        panel = struct.unpack_from(
            f"<{PANEL_PIXELS}H",
            data,
            HEADER.size + state * PANEL_PIXELS * 2,
        )
        actual = read_ppm(capture_path)
        expected = expected_rgb(panel)
        different = sum(
            actual[offset : offset + 3] != expected[offset : offset + 3]
            for offset in range(0, len(actual), 3)
        )
        if different:
            raise ValueError(
                f"title state {state + 1} differs at {different} pixels: "
                f"{capture_path}"
            )
        print(f"title state {state + 1}: 0 differing pixels")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
