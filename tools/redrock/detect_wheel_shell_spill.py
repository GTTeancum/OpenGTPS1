"""Detect smooth-wheel shell pixels that escape the rear-view mirror band.

The modern renderer draws replacement wheel shells with the flat colour
0.015 (4/255 after quantisation).  No authored GT2 material lands on that
exact triple, so counting (4,4,4) pixels isolates shell coverage in a
finished capture without any renderer instrumentation.

Usage:
    python tools/redrock/detect_wheel_shell_spill.py <frame-dir-or-video> [...]
"""

from __future__ import annotations

import argparse
import pathlib
import sys

import numpy as np
from PIL import Image

SHELL = np.array([4, 4, 4], dtype=np.int16)


def shell_mask(frame: np.ndarray) -> np.ndarray:
    return np.all(frame == SHELL, axis=2)


def components(mask: np.ndarray) -> list[tuple[int, int, int, int, int]]:
    """Label 4-connected runs without SciPy; returns (area,x0,y0,x1,y1)."""
    height, width = mask.shape
    labels = np.zeros((height, width), dtype=np.int32)
    out: list[tuple[int, int, int, int, int]] = []
    stack: list[tuple[int, int]] = []
    label = 0
    for start_y in range(height):
        row = mask[start_y]
        if not row.any():
            continue
        for start_x in np.flatnonzero(row):
            if labels[start_y, start_x]:
                continue
            label += 1
            area = 0
            x0 = x1 = int(start_x)
            y0 = y1 = start_y
            stack.append((start_y, int(start_x)))
            labels[start_y, start_x] = label
            while stack:
                y, x = stack.pop()
                area += 1
                x0 = min(x0, x)
                x1 = max(x1, x)
                y0 = min(y0, y)
                y1 = max(y1, y)
                for ny, nx in ((y - 1, x), (y + 1, x), (y, x - 1), (y, x + 1)):
                    if 0 <= ny < height and 0 <= nx < width:
                        if mask[ny, nx] and not labels[ny, nx]:
                            labels[ny, nx] = label
                            stack.append((ny, nx))
            out.append((area, x0, y0, x1, y1))
    out.sort(reverse=True)
    return out


def analyse(path: pathlib.Path, minimum_area: int) -> None:
    frames = sorted(path.glob("*.png"))
    if not frames:
        print(f"no frames in {path}", file=sys.stderr)
        raise SystemExit(2)
    union = None
    worst = []
    for index, frame_path in enumerate(frames, start=1):
        frame = np.array(Image.open(frame_path).convert("RGB")).astype(np.int16)
        mask = shell_mask(frame)
        union = mask if union is None else (union | mask)
        total = int(mask.sum())
        if total < minimum_area:
            continue
        blobs = [blob for blob in components(mask) if blob[0] >= minimum_area]
        for area, x0, y0, x1, y1 in blobs:
            worst.append((area, index, frame_path.name, x0, y0, x1, y1))
    worst.sort(reverse=True)
    print(f"frames={len(frames)} blobs>={minimum_area}px: {len(worst)}")
    for area, index, name, x0, y0, x1, y1 in worst[:30]:
        print(
            f"  frame={index:4d} {name} area={area:6d} "
            f"box={x0},{y0}..{x1},{y1}")
    if union is not None and union.any():
        ys, xs = np.nonzero(union)
        print(
            f"union shell coverage box="
            f"{xs.min()},{ys.min()}..{xs.max()},{ys.max()} "
            f"pixels={int(union.sum())}")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("frames", type=pathlib.Path)
    parser.add_argument("--minimum-area", type=int, default=200)
    arguments = parser.parse_args()
    analyse(arguments.frames, arguments.minimum_area)


if __name__ == "__main__":
    main()
