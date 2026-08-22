"""Scan a capture for one-frame temporal pops (flicker).

A pop is a pixel that differs strongly from BOTH neighbouring frames while
those neighbours agree with each other: content that appears for exactly one
frame and vanishes. Smooth motion never produces that signature, so the count
isolates popping/flicker from ordinary camera movement.

Usage:
    python tools/redrock/scan_temporal_pops.py <video> [--top 30]
"""

from __future__ import annotations

import argparse
import pathlib
import subprocess
import sys

import numpy as np

WIDTH = 640
HEIGHT = 480


def frames(video: pathlib.Path):
    process = subprocess.Popen(
        [
            "ffmpeg", "-hide_banner", "-loglevel", "error",
            "-i", str(video),
            "-vf", f"scale={WIDTH}:{HEIGHT}:flags=area",
            "-f", "rawvideo", "-pix_fmt", "gray", "-",
        ],
        stdout=subprocess.PIPE,
    )
    size = WIDTH * HEIGHT
    assert process.stdout is not None
    while True:
        raw = process.stdout.read(size)
        if len(raw) < size:
            break
        yield np.frombuffer(raw, dtype=np.uint8).reshape(HEIGHT, WIDTH)
    process.stdout.close()
    process.wait()


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("video", type=pathlib.Path)
    parser.add_argument("--top", type=int, default=30)
    parser.add_argument("--threshold", type=int, default=40)
    parser.add_argument("--agree", type=int, default=12)
    parser.add_argument("--row-start", type=int, default=0)
    parser.add_argument("--row-end", type=int, default=HEIGHT)
    parser.add_argument("--col-start", type=int, default=0)
    parser.add_argument("--col-end", type=int, default=WIDTH)
    arguments = parser.parse_args()

    previous = None
    middle = None
    scores = []
    for index, frame in enumerate(frames(arguments.video)):
        band = frame[
            arguments.row_start:arguments.row_end,
            arguments.col_start:arguments.col_end,
        ].astype(np.int16)
        if previous is not None and middle is not None:
            pop = (
                (np.abs(middle - previous) > arguments.threshold) &
                (np.abs(middle - band) > arguments.threshold) &
                (np.abs(band - previous) <= arguments.agree)
            )
            scores.append((int(pop.sum()), index - 1))
        previous, middle = middle, band
    if not scores:
        print("no frames decoded", file=sys.stderr)
        raise SystemExit(2)
    counts = np.array([score for score, _ in scores])
    print(
        f"frames={len(scores)} mean={counts.mean():.1f} "
        f"median={np.median(counts):.1f} p99={np.percentile(counts, 99):.0f} "
        f"max={counts.max()}")
    scores.sort(reverse=True)
    print("worst pop frames (frame, seconds, popped pixels):")
    for score, index in scores[:arguments.top]:
        print(
            f"  frame={index:5d} "
            f"t={index * 1001.0 / 60000.0:7.2f}s pixels={score}")


if __name__ == "__main__":
    main()
