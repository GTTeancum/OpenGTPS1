"""Count wheel-shell pixels that escape the rear-view mirror drawing area.

The modern renderer paints replacement wheel shells with the flat colour
0.015 (4/255).  The GT2 race view submits its rear-view mirror through the
PS1 drawing area 100,20..219,51 of a 320x240 display, which is 200,40..439,103
in a 640x480 capture.  Shell pixels above the race-view horizon and outside
that rectangle are mirror geometry that escaped the guest clip.

Usage:
    python tools/redrock/scan_mirror_spill.py <video> [--top 20]
"""

from __future__ import annotations

import argparse
import pathlib
import subprocess

import numpy as np

WIDTH = 640
HEIGHT = 480
MIRROR = (200, 40, 440, 104)
HORIZON = 200
SHELL = 4


def erode(mask: np.ndarray, passes: int) -> np.ndarray:
    """Drop pixels that are not fully surrounded by shell colour.

    Isolated dark texels in signage, shadow, and tunnel geometry also land on
    the shell triple. A wheel shell is a solid disc tens of pixels across, so
    a few erosion passes keep shells and discard incidental speckle.
    """
    for _ in range(passes):
        keep = mask.copy()
        keep[1:, :] &= mask[:-1, :]
        keep[:-1, :] &= mask[1:, :]
        keep[:, 1:] &= mask[:, :-1]
        keep[:, :-1] &= mask[:, 1:]
        mask = keep
    return mask


def frames(video: pathlib.Path):
    process = subprocess.Popen(
        [
            "ffmpeg", "-hide_banner", "-loglevel", "error",
            "-i", str(video),
            "-f", "rawvideo", "-pix_fmt", "rgb24", "-",
        ],
        stdout=subprocess.PIPE,
    )
    size = WIDTH * HEIGHT * 3
    assert process.stdout is not None
    while True:
        raw = process.stdout.read(size)
        if len(raw) < size:
            break
        yield np.frombuffer(raw, dtype=np.uint8).reshape(HEIGHT, WIDTH, 3)
    process.stdout.close()
    process.wait()


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("video", type=pathlib.Path)
    parser.add_argument("--top", type=int, default=20)
    parser.add_argument("--erode", type=int, default=3)
    arguments = parser.parse_args()

    x0, y0, x1, y1 = MIRROR
    worst = []
    total = 0
    affected = 0
    for index, frame in enumerate(frames(arguments.video)):
        band = frame[:HORIZON]
        mask = np.all(band == SHELL, axis=2)
        mask[y0:min(y1, HORIZON), x0:x1] = False
        count = int(erode(mask, arguments.erode).sum())
        total += count
        if count:
            affected += 1
            worst.append((count, index))
    worst.sort(reverse=True)
    print(f"total spill pixels={total} frames with spill={affected}")
    for count, index in worst[:arguments.top]:
        print(f"  frame={index:5d} t={index / 60.0:7.2f}s pixels={count}")


if __name__ == "__main__":
    main()
