#!/usr/bin/env python3
"""Assemble archive-derived GT1 car and paint renders into proof sheets."""

from __future__ import annotations

import argparse
import hashlib
import html
import json
from collections import defaultdict
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


def load_json(path: Path) -> dict:
    return json.loads(path.read_text(encoding="utf-8"))


def font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    candidates = (
        Path("C:/Windows/Fonts/seguisb.ttf") if bold else Path("C:/Windows/Fonts/segoeui.ttf"),
        Path("C:/Windows/Fonts/arialbd.ttf") if bold else Path("C:/Windows/Fonts/arial.ttf"),
    )
    for candidate in candidates:
        if candidate.is_file():
            return ImageFont.truetype(str(candidate), size)
    return ImageFont.load_default()


def visual_hash(path: Path) -> str:
    with Image.open(path) as image:
        image = image.convert("RGB")
        # The viewer's bottom 80 pixels contain the variant label. Exclude it so
        # differently-labelled but visually identical bodies compare equal.
        body = image.crop((0, 0, image.width, max(1, image.height - 80)))
        return hashlib.sha256(body.tobytes()).hexdigest()


def make_sheet(
    entries: list[dict],
    destination: Path,
    title: str,
    subtitle: str,
    columns: int = 5,
) -> None:
    # Preserve at least 640x480 detail in every review tile. The archive
    # viewer renders are 960x640 (3:2), so 720x480 keeps their aspect ratio
    # while meeting the screenshot floor without synthetic enlargement.
    tile_width = 720
    tile_height = 480
    header_height = 124
    rows = (len(entries) + columns - 1) // columns
    sheet = Image.new(
        "RGB",
        (columns * tile_width, header_height + rows * tile_height),
        (12, 16, 22),
    )
    draw = ImageDraw.Draw(sheet)
    draw.text((36, 22), title, fill=(245, 247, 250), font=font(38, bold=True))
    draw.text((38, 76), subtitle, fill=(158, 172, 189), font=font(22))
    for index, entry in enumerate(entries):
        with Image.open(entry["proofRenderPath"]) as source:
            tile = source.convert("RGB").resize(
                (tile_width, tile_height), Image.Resampling.LANCZOS
            )
        x = (index % columns) * tile_width
        y = header_height + (index // columns) * tile_height
        sheet.paste(tile, (x, y))
        draw.rectangle(
            (x, y, x + tile_width - 1, y + tile_height - 1),
            outline=(45, 55, 68),
            width=1,
        )
    destination.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(destination, optimize=True)


def make_proof_render(entry: dict, destination: Path) -> None:
    with Image.open(entry["renderPath"]) as source:
        image = source.convert("RGB")
    draw = ImageDraw.Draw(image)
    draw.rectangle((0, image.height - 82, image.width, image.height), fill=(5, 9, 14))
    if entry["category"] == "standalone":
        line_one = (
            f"GT1 standalone import · {entry['name']} · {entry['stem']} · "
            f"palette {entry['palette']} · color ID {entry['colorId']}"
        )
        line_two = "Converted archive model + indexed texture/CLUT from merged Arcade proof volume"
    else:
        line_one = (
            f"GT1 folded paint · target {entry['stem']} · source {entry['sourceStem']} · "
            f"palette {entry['palette']} · color ID {entry['colorId']}"
        )
        line_two = entry["description"]
    draw.text((18, image.height - 70), line_one, fill=(242, 246, 250), font=font(22, bold=True))
    draw.text((18, image.height - 39), line_two, fill=(151, 167, 184), font=font(16))
    destination.parent.mkdir(parents=True, exist_ok=True)
    image.save(destination, optimize=True)


def make_ssr11_sheet(menu_path: Path, race_path: Path, destination: Path) -> None:
    width, height = 2400, 1120
    sheet = Image.new("RGB", (width, height), (12, 16, 22))
    draw = ImageDraw.Draw(sheet)
    draw.text((36, 22), "Special Stage Route 11 — integrated visual proof", fill=(245, 247, 250), font=font(38, bold=True))
    draw.text(
        (38, 76),
        "Native Arcade course entry/minimap + modern-renderer auto-drive sequence",
        fill=(158, 172, 189),
        font=font(22),
    )
    with Image.open(menu_path) as source:
        menu = source.convert("RGB").resize((576, 864), Image.Resampling.NEAREST)
    with Image.open(race_path) as source:
        race = source.convert("RGB").resize((1720, 911), Image.Resampling.LANCZOS)
    sheet.paste(menu, (36, 164))
    sheet.paste(race, (644, 164))
    draw.rectangle((36, 164, 611, 1027), outline=(55, 68, 84), width=2)
    draw.rectangle((644, 164, 2363, 1074), outline=(55, 68, 84), width=2)
    draw.text((36, 1042), "Installed Arcade selection and authored SSR11 course map", fill=(190, 202, 215), font=font(18))
    destination.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(destination, optimize=True)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--manifest", type=Path, required=True)
    parser.add_argument("--viewer-root", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--ssr11-menu", type=Path)
    parser.add_argument("--ssr11-race", type=Path)
    args = parser.parse_args()

    conversion = load_json(args.manifest)
    standalone: list[dict] = []
    folded: list[dict] = []

    def viewer(stem: str) -> tuple[Path, dict]:
        root = args.viewer_root / stem
        return root, load_json(root / "viewer_manifest.json")

    for car in conversion["arcadeCars"]:
        stem = car["stem"]
        root, rendered = viewer(stem)
        variants = rendered["variants"]
        actual_ids = [variant["colorId"] for variant in variants]
        expected_ids = car["colorIds"]
        if actual_ids != expected_ids:
            raise ValueError(
                f"{stem}: rendered color IDs {actual_ids} != manifest {expected_ids}"
            )
        for variant in variants:
            render_path = root / Path(variant["render"])
            standalone.append(
                {
                    "category": "standalone",
                    "stem": stem,
                    "name": car["displayName"],
                    "colorId": variant["colorId"],
                    "palette": variant["bodyPalette"],
                    "label": variant["label"],
                    "render": str(render_path.resolve()),
                    "renderPath": render_path,
                    "visualHash": visual_hash(render_path),
                }
            )

    for fold in conversion["liveryFolds"]["arcade"]:
        stem = fold["targetStem"]
        root, rendered = viewer(stem)
        integrated = [
            variant
            for variant in rendered["variants"]
            if variant["status"].startswith("Integrated choice")
        ]
        target_choices = {
            mapping["targetColorIndex"]
            for mapping in fold.get("bodyMappings", [])
        }
        variants = [
            variant
            for variant in integrated
            if variant.get("targetPalette") in target_choices
        ]
        expected = len(fold.get("bodyMappings", []))
        if fold.get("nativePaletteFold"):
            variants = integrated
            expected = len(fold["acceptedVisualChoices"])
        if len(variants) != expected:
            raise ValueError(
                f"{stem}: rendered {len(variants)} integrated choices, expected {expected}"
            )
        for variant in variants:
            render_path = root / Path(variant["render"])
            folded.append(
                {
                    "category": "folded",
                    "stem": stem,
                    "sourceStem": fold["sourceStem"],
                    "description": fold["description"],
                    "colorId": variant["colorId"],
                    "palette": variant["bodyPalette"],
                    "targetChoice": variant["targetPalette"],
                    "label": variant["label"],
                    "render": str(render_path.resolve()),
                    "renderPath": render_path,
                    "visualHash": visual_hash(render_path),
                }
            )

    duplicate_groups: list[dict] = []
    for category, entries in (("standalone", standalone), ("folded", folded)):
        by_stem_hash: dict[tuple[str, str], list[dict]] = defaultdict(list)
        for entry in entries:
            by_stem_hash[(entry["stem"], entry["visualHash"])].append(entry)
        for (stem, digest), matches in by_stem_hash.items():
            if len(matches) > 1:
                duplicate_groups.append(
                    {
                        "category": category,
                        "stem": stem,
                        "visualHash": digest,
                        "labels": [item["label"] for item in matches],
                    }
                )

    args.output.mkdir(parents=True, exist_ok=True)
    for entry in standalone + folded:
        suffix = f"palette-{entry['palette']}-id-{entry['colorId']}"
        if entry["category"] == "folded" and entry["targetChoice"] is not None:
            suffix += f"-choice-{entry['targetChoice']}"
        proof_path = args.output / "individual" / entry["category"] / entry["stem"] / f"{suffix}.png"
        make_proof_render(entry, proof_path)
        entry["sourceRender"] = str(entry["renderPath"].resolve())
        entry["render"] = str(proof_path.resolve())
        entry["proofRenderPath"] = proof_path

    standalone_sheet = args.output / "gt1-standalone-cars-all-paints.png"
    folded_sheet = args.output / "gt1-folded-paints-and-liveries.png"
    livery_highlights = args.output / "gt1-livery-highlights-cerbera-supra.png"
    make_sheet(
        standalone,
        standalone_sheet,
        "GT1 standalone Arcade imports — every installed paint",
        f"{len(standalone)} archive-native choices across {len(conversion['arcadeCars'])} cars",
    )
    make_sheet(
        folded,
        folded_sheet,
        "GT1 paints and liveries folded into GT2 counterparts",
        f"{len(folded)} archive-native choices across {len(conversion['liveryFolds']['arcade'])} target cars",
    )
    highlighted = [entry for entry in folded if entry["stem"] in ("v-rbr", "tsplr")]
    make_sheet(
        highlighted,
        livery_highlights,
        "GT1 LM/JGTC livery detail — Cerbera and Castrol Supra",
        "Two exclusive TVR LM paints and all four accepted Castrol Supra liveries",
        columns=4,
    )
    ssr11_sheet = None
    if args.ssr11_menu is not None and args.ssr11_race is not None:
        ssr11_sheet = args.output / "ssr11-integrated-modern-renderer-proof.png"
        make_ssr11_sheet(args.ssr11_menu, args.ssr11_race, ssr11_sheet)

    serializable_entries = []
    for entry in standalone + folded:
        serializable_entries.append(
            {
                key: value
                for key, value in entry.items()
                if key not in ("renderPath", "proofRenderPath")
            }
        )
    proof_manifest = {
        "formatVersion": 1,
        "sourceManifest": str(args.manifest.resolve()),
        "standaloneCarCount": len(conversion["arcadeCars"]),
        "standalonePaintCount": len(standalone),
        "foldTargetCount": len(conversion["liveryFolds"]["arcade"]),
        "foldedPaintCount": len(folded),
        "duplicateVisualGroupsWithinStem": duplicate_groups,
        "entries": serializable_entries,
    }
    (args.output / "visual-proof-manifest.json").write_text(
        json.dumps(proof_manifest, indent=2) + "\n", encoding="utf-8"
    )

    rows = []
    for entry in serializable_entries:
        relative = Path(entry["render"]).relative_to(args.output.resolve())
        rows.append(
            "<article><a href='"
            + html.escape(relative.as_posix())
            + "'><img loading='lazy' src='"
            + html.escape(relative.as_posix())
            + "'></a><p>"
            + html.escape(entry["label"])
            + "</p></article>"
        )
    ssr11_html = (
        f"<a href='{ssr11_sheet.name}'><img src='{ssr11_sheet.name}'></a>"
        if ssr11_sheet is not None
        else ""
    )
    page = f"""<!doctype html><meta charset='utf-8'>
<title>GT1 visual conversion proofs</title>
<style>body{{background:#0c1016;color:#eef3f8;font:16px Segoe UI,sans-serif;margin:24px}}
h1,h2{{margin-bottom:8px}} .sheets img{{width:min(100%,1200px);display:block;margin:16px 0 32px}}
.grid{{display:grid;grid-template-columns:repeat(auto-fill,minmax(360px,1fr));gap:16px}}
article{{background:#151c25;border:1px solid #2d3948;border-radius:8px;overflow:hidden}}
article img{{width:100%;display:block}}article p{{margin:10px 14px 14px}}</style>
<h1>GT1 content conversion — visual proofs</h1>
<p>{len(standalone)} standalone paints + {len(folded)} folded paints/liveries, sourced from the converted volume.</p>
<div class='sheets'><h2>Master sheets</h2>
<a href='{standalone_sheet.name}'><img src='{standalone_sheet.name}'></a>
<a href='{folded_sheet.name}'><img src='{folded_sheet.name}'></a>
<a href='{livery_highlights.name}'><img src='{livery_highlights.name}'></a>
{ssr11_html}</div>
<h2>Full-resolution individual renders</h2><div class='grid'>{''.join(rows)}</div>"""
    (args.output / "index.html").write_text(page, encoding="utf-8")

    print(
        f"assembled {len(standalone)} standalone and {len(folded)} folded proofs; "
        f"{len(duplicate_groups)} within-car duplicate visual groups"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
