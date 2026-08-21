#!/usr/bin/env python3
"""Locate transient compact near-white sprites in a renderer proof video."""

from __future__ import annotations

import argparse
import json
import shutil
import subprocess
from pathlib import Path

import numpy as np
from scipy import ndimage


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("video", type=Path)
    parser.add_argument("--width", type=int, default=640)
    parser.add_argument("--height", type=int, default=480)
    parser.add_argument("--top", type=int, default=120)
    parser.add_argument("--bottom", type=int, default=430)
    parser.add_argument("--limit", type=int, default=100)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()

    ffmpeg = shutil.which("ffmpeg")
    if not ffmpeg:
        raise SystemExit("ffmpeg is required")

    command = [
        ffmpeg,
        "-v",
        "error",
        "-i",
        str(args.video),
        "-f",
        "rawvideo",
        "-pix_fmt",
        "rgb24",
        "-",
    ]
    process = subprocess.Popen(command, stdout=subprocess.PIPE)
    assert process.stdout is not None
    frame_size = args.width * args.height * 3
    previous: np.ndarray | None = None
    candidates: list[dict[str, object]] = []
    frame_index = 0
    structure = np.ones((3, 3), dtype=np.uint8)

    while True:
        data = process.stdout.read(frame_size)
        if len(data) != frame_size:
            break
        frame = np.frombuffer(data, dtype=np.uint8).reshape(
            (args.height, args.width, 3)
        )
        if previous is not None:
            roi = frame[args.top : args.bottom]
            old_roi = previous[args.top : args.bottom]
            maximum = roi.max(axis=2)
            minimum = roi.min(axis=2)
            old_mean = old_roi.mean(axis=2)
            # Backfire is expected to be a small, nearly neutral, newly bright sprite.
            onset = (
                (roi.mean(axis=2) >= 180.0)
                & ((maximum.astype(np.int16) - minimum.astype(np.int16)) <= 55)
                & (old_mean <= 150.0)
            )
            labels, count = ndimage.label(onset, structure=structure)
            if count:
                objects = ndimage.find_objects(labels)
                for label_index, slices in enumerate(objects, start=1):
                    if slices is None:
                        continue
                    ys, xs = slices
                    width = xs.stop - xs.start
                    height = ys.stop - ys.start
                    area = int(np.count_nonzero(labels[slices] == label_index))
                    box_area = width * height
                    if not (2 <= width <= 40 and 2 <= height <= 40):
                        continue
                    if not (4 <= area <= 900):
                        continue
                    fill = area / box_area
                    if fill < 0.30:
                        continue
                    component = roi[slices][labels[slices] == label_index]
                    brightness = float(component.mean())
                    compactness = min(width, height) / max(width, height)
                    score = area * fill * (brightness / 255.0) * (
                        0.40 + 0.60 * compactness
                    )
                    candidates.append(
                        {
                            "frame": frame_index,
                            "x": xs.start,
                            "y": ys.start + args.top,
                            "width": width,
                            "height": height,
                            "area": area,
                            "fill": round(fill, 4),
                            "brightness": round(brightness, 3),
                            "score": round(score, 4),
                        }
                    )
        previous = frame.copy()
        frame_index += 1

    return_code = process.wait()
    if return_code != 0:
        raise SystemExit(f"ffmpeg failed with exit code {return_code}")

    candidates.sort(key=lambda item: float(item["score"]), reverse=True)
    result = {
        "video": str(args.video),
        "frames": frame_index,
        "roi": [0, args.top, args.width, args.bottom],
        "candidates": candidates[: args.limit],
    }
    output = json.dumps(result, indent=2)
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(output + "\n", encoding="utf-8")
    print(output)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
