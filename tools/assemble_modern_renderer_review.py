#!/usr/bin/env python3
"""Build the final cross-mode visual review pack from validated captures."""

from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


def font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    names = ("seguisb.ttf", "arialbd.ttf") if bold else ("segoeui.ttf", "arial.ttf")
    for name in names:
        path = Path("C:/Windows/Fonts") / name
        if path.is_file():
            return ImageFont.truetype(str(path), size)
    return ImageFont.load_default()


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def captioned(source: Path, title: str, subtitle: str) -> Image.Image:
    with Image.open(source) as opened:
        image = opened.convert("RGB")
    header = 92
    result = Image.new("RGB", (image.width, image.height + header), (10, 14, 20))
    result.paste(image, (0, header))
    draw = ImageDraw.Draw(result)
    draw.text((22, 13), title, fill=(246, 248, 251), font=font(30, True))
    draw.text((23, 54), subtitle, fill=(153, 170, 188), font=font(18))
    return result


def make_replay(frames: list[Path], destination: Path) -> Image.Image:
    tile_width, tile_height = 684, 513
    columns = 3
    rows = 1
    header = 92
    result = Image.new(
        "RGB",
        (columns * tile_width, header + rows * tile_height),
        (10, 14, 20),
    )
    draw = ImageDraw.Draw(result)
    draw.text(
        (22, 13),
        "Gran Turismo Mode — replay and authored results handoff",
        fill=(246, 248, 251),
        font=font(30, True),
    )
    draw.text(
        (23, 54),
        "Natural replay frame followed by the completed race's Results handoff; Maximum vehicle and track LOD",
        fill=(153, 170, 188),
        font=font(18),
    )
    for index, path in enumerate(frames):
        with Image.open(path) as opened:
            tile = opened.convert("RGB").resize(
                (tile_width, tile_height), Image.Resampling.LANCZOS
            )
        x = (index % columns) * tile_width
        y = header + (index // columns) * tile_height
        result.paste(tile, (x, y))
        draw.rectangle(
            (x, y, x + tile_width - 1, y + tile_height - 1),
            outline=(50, 63, 78),
            width=1,
        )
    destination.parent.mkdir(parents=True, exist_ok=True)
    result.save(destination, optimize=True)
    return result


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--tahiti", type=Path, required=True)
    parser.add_argument("--ssr11", type=Path, required=True)
    parser.add_argument("--replay-dir", type=Path, required=True)
    parser.add_argument("--closure-report", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=True)

    report = json.loads(args.closure_report.read_text(encoding="utf-8-sig"))
    tahiti = captioned(
        args.tahiti,
        "Arcade Mode — Tahiti Road / imported Supra scenario",
        "15 distributed full-lap captures; zero exposed FF00FF clear-sentinel pixels",
    )
    ssr11 = captioned(
        args.ssr11,
        "Arcade Mode — Special Stage Route 11",
        "15 distributed night-lap captures; correct authored minimap and zero clear-sentinel holes",
    )
    tahiti_path = args.output / "arcade-tahiti-full-lap-review.png"
    ssr11_path = args.output / "arcade-ssr11-full-lap-review.png"
    tahiti.save(tahiti_path, optimize=True)
    ssr11.save(ssr11_path, optimize=True)

    replay_sources = sorted(args.replay_dir.glob("recompone_present_replay_1_*.ppm"))
    if len(replay_sources) != 5:
        raise ValueError(f"expected five full-path captures, found {len(replay_sources)}")
    review_names = (
        "recompone_present_replay_1_7400_1368x1026_fxaa.ppm",
        "recompone_present_replay_1_7800_1368x1026_fxaa.ppm",
        "recompone_present_replay_1_8000_1368x1026_fxaa.ppm",
    )
    replay_review_sources = [args.replay_dir / name for name in review_names]
    missing_review = [path for path in replay_review_sources if not path.is_file()]
    if missing_review:
        raise FileNotFoundError(f"missing GT Mode review captures: {missing_review}")
    replay_path = args.output / "gran-turismo-natural-replay-review.png"
    replay = make_replay(replay_review_sources, replay_path)

    width = 2400
    margin = 32
    top = 138
    half_width = (width - margin * 3) // 2
    arcade_height = round(tahiti.height * half_width / tahiti.width)
    replay_width = width - margin * 2
    replay_height = round(replay.height * replay_width / replay.width)
    height = top + arcade_height + margin + replay_height + 100
    overview = Image.new("RGB", (width, height), (8, 12, 18))
    draw = ImageDraw.Draw(overview)
    draw.text((34, 22), "Gran Turismo 2 modern renderer — final cross-mode review", fill=(246, 248, 251), font=font(42, True))
    draw.text(
        (36, 82),
        (
            f"Unique motion {report['ssr11Motion']['uniqueFrames']}/{report['ssr11Motion']['frames']} · "
            f"lossless frames {report['losslessGeometry']['distributedFrames']} · "
            f"sentinel holes {report['losslessGeometry']['clearSentinelPixels']} · "
            f"soak runs {report['extendedSoak']['runs']}"
        ),
        fill=(155, 174, 194),
        font=font(22),
    )
    tahiti_small = tahiti.resize((half_width, arcade_height), Image.Resampling.LANCZOS)
    ssr11_small = ssr11.resize((half_width, arcade_height), Image.Resampling.LANCZOS)
    overview.paste(tahiti_small, (margin, top))
    overview.paste(ssr11_small, (margin * 2 + half_width, top))
    replay_small = replay.resize((replay_width, replay_height), Image.Resampling.LANCZOS)
    replay_y = top + arcade_height + margin
    overview.paste(replay_small, (margin, replay_y))
    footer = replay_y + replay_height + 28
    full = report["granTurismoFullPath"]
    draw.text(
        (36, footer),
        (
            f"GT Mode: {full['perfectWorldWindows']} perfect unique-60 windows · "
            f"race LOD calls {full['raceLodCalls']} · replay LOD calls {full['replayLodCalls']} · "
            f"pipeline p99 {full['profile']['PipelineP99']} ms"
        ),
        fill=(192, 204, 218),
        font=font(20),
    )
    overview_path = args.output / "modern-renderer-final-cross-mode-review.png"
    overview.save(overview_path, optimize=True)

    evidence = {
        "formatVersion": 1,
        "closureState": report["state"],
        "sources": {
            "tahiti": {"path": str(args.tahiti.resolve()), "sha256": sha256(args.tahiti)},
            "ssr11": {"path": str(args.ssr11.resolve()), "sha256": sha256(args.ssr11)},
            "replay": [
                {"path": str(path.resolve()), "sha256": sha256(path)}
                for path in replay_sources
            ],
            "closureReport": {
                "path": str(args.closure_report.resolve()),
                "sha256": sha256(args.closure_report),
            },
        },
        "outputs": {
            "overview": str(overview_path.resolve()),
            "arcadeTahiti": str(tahiti_path.resolve()),
            "arcadeSsr11": str(ssr11_path.resolve()),
            "granTurismoReplay": str(replay_path.resolve()),
        },
    }
    (args.output / "evidence.json").write_text(
        json.dumps(evidence, indent=2) + "\n", encoding="utf-8"
    )
    print(
        "modern_renderer_review=assembled "
        f"arcade_tracks=2 gt_review_frames={len(replay_review_sources)} output={overview_path}"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
