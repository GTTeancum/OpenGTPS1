"""Assemble the Red Rock before/after evidence for the mirror-shell and
vehicle-coherence fixes.

Builds, for one capture:
  * a 6x4 contact sheet of the 34-38s rear-view-mirror window,
  * the frames of that window as PNGs,
  * the race intro and third-person chase reference frames.

Usage:
    python tools/redrock/build_redrock_evidence.py <artifact-dir>
"""

from __future__ import annotations

import argparse
import pathlib
import subprocess

VIDEO = "GT2_Modern_Final_Simulation.mp4"


def run(*command: str) -> None:
    subprocess.run(command, check=True)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("artifact", type=pathlib.Path)
    arguments = parser.parse_args()
    video = arguments.artifact / VIDEO
    if not video.is_file():
        raise SystemExit(f"missing capture: {video}")
    frames = arguments.artifact / "ghost-wheels-034-038-frames"
    frames.mkdir(exist_ok=True)
    run(
        "ffmpeg", "-hide_banner", "-loglevel", "error", "-y",
        "-ss", "34", "-t", "4", "-i", str(video),
        str(frames / "frame_%04d.png"))
    run(
        "ffmpeg", "-hide_banner", "-loglevel", "error", "-y",
        "-ss", "34", "-t", "4", "-i", str(video),
        "-vf",
        "fps=6,scale=640:480:flags=neighbor,tile=6x4:padding=2:margin=2",
        "-frames:v", "1",
        str(arguments.artifact / "red-rock-ghost-wheels-034-038s-sheet.png"))
    run(
        "ffmpeg", "-hide_banner", "-loglevel", "error", "-y",
        "-ss", "34", "-t", "4", "-i", str(video),
        "-c:v", "libx264", "-preset", "medium", "-crf", "12",
        "-pix_fmt", "yuv420p",
        str(arguments.artifact / "red-rock-ghost-wheels-034-038s.mp4"))
    run(
        "ffmpeg", "-hide_banner", "-loglevel", "error", "-y",
        "-ss", "2", "-i", str(video), "-frames:v", "1",
        str(arguments.artifact / "race-intro-002s.png"))
    run(
        "ffmpeg", "-hide_banner", "-loglevel", "error", "-y",
        "-ss", "8", "-i", str(video), "-frames:v", "1",
        str(arguments.artifact / "third-person-start-008s.png"))
    run(
        "ffmpeg", "-hide_banner", "-loglevel", "error", "-y",
        "-i", str(video),
        "-vf",
        r"select='not(mod(n\,600))',scale=640:480:flags=neighbor,"
        "tile=5x3:padding=2:margin=2",
        "-frames:v", "1", "-fps_mode", "vfr",
        str(arguments.artifact / "intro-plus-full-lap-sheet.png"))
    print(f"evidence written under {arguments.artifact}")


if __name__ == "__main__":
    main()
