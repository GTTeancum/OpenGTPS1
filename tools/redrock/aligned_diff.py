"""Compare two captures of the same fixture, tolerating run-to-run drift.

Captures of this fixture are frame-locked early and drift by a few frames over
a 150-second lap, so comparing equal frame numbers measures the drift rather
than the renderer. For each sampled frame of the reference capture this picks
the best-matching frame within a search window of the candidate capture and
reports that minimum, which is the real rendering delta.

Usage:
    python tools/redrock/aligned_diff.py <reference.mp4> <candidate.mp4>
"""

from __future__ import annotations

import argparse
import pathlib
import subprocess

import numpy as np

WIDTH = 640
HEIGHT = 480
FRAME = WIDTH * HEIGHT * 3


def read_range(path: pathlib.Path, first: int, count: int):
    process = subprocess.Popen(
        [
            "ffmpeg", "-hide_banner", "-loglevel", "error",
            "-i", str(path),
            "-vf", f"select='between(n,{first},{first + count - 1})'",
            "-fps_mode", "vfr",
            "-f", "rawvideo", "-pix_fmt", "rgb24", "-",
        ],
        stdout=subprocess.PIPE,
    )
    out = []
    assert process.stdout is not None
    while True:
        raw = process.stdout.read(FRAME)
        if len(raw) < FRAME:
            break
        out.append(
            np.frombuffer(raw, dtype=np.uint8)
            .astype(np.int16)
            .reshape(HEIGHT, WIDTH, 3))
    process.stdout.close()
    process.wait()
    return out


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("reference", type=pathlib.Path)
    parser.add_argument("candidate", type=pathlib.Path)
    parser.add_argument("--samples", type=int, default=12)
    parser.add_argument("--window", type=int, default=10)
    parser.add_argument("--threshold", type=int, default=24)
    parser.add_argument("--first", type=int, default=600)
    parser.add_argument("--last", type=int, default=8800)
    arguments = parser.parse_args()

    step = max(1, (arguments.last - arguments.first) // arguments.samples)
    rows = []
    for index in range(arguments.first, arguments.last, step):
        reference = read_range(arguments.reference, index, 1)
        if not reference:
            continue
        window = read_range(
            arguments.candidate,
            max(0, index - arguments.window),
            arguments.window * 2 + 1)
        if not window:
            continue
        best = None
        for offset, candidate in enumerate(window):
            differing = int(
                (np.abs(reference[0] - candidate).max(axis=2) >
                    arguments.threshold).sum())
            shift = offset - min(arguments.window, index)
            if best is None or differing < best[0]:
                best = (differing, shift)
        rows.append((index, best[0], best[1]))
        print(
            f"  frame={index:5d} aligned-diff={best[0]:6d}px "
            f"drift={best[1]:+d}")
    if rows:
        values = [row[1] for row in rows]
        print(
            f"aligned rendering delta: median={int(np.median(values))}px "
            f"mean={int(np.mean(values))}px max={max(values)}px")


if __name__ == "__main__":
    main()
