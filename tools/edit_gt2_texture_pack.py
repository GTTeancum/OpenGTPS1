#!/usr/bin/env python3
"""Export authored GT2 textures to editable PNGs and build a loose DDS pack."""

import argparse
import gzip
import json
from pathlib import Path
import struct

from PIL import Image

from build_gt2_texture_pack import (
    asset_stem, decode_tim_asset, read_car_assets, read_course_assets,
    read_volume_member, upload_hash,
)
from gt2_vol import read_entries


def inside(root: Path, name: str) -> Path:
    path = (root / name).resolve()
    if not path.is_relative_to(root.resolve()):
        raise ValueError(f"path escapes texture project: {name}")
    return path


def export_project(volumes, output, tim_names=(), courses=(), cars=()):
    if (output / "textures.json").exists():
        raise ValueError("texture project already exists; choose a new output directory")
    records = {}

    def add(identity, image, mode, name, replacement="rgb", screen=False):
        stem = asset_stem(identity)
        if stem in records:
            records[stem]["sources"].append(name)
            return
        relative = f"textures/{stem}.png"
        path = output / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        image.save(path)
        records[stem] = dict(
            key=f"{identity[0]:016x}", image=relative,
            sourceWidth=image.width, sourceHeight=image.height,
            pixelMode=mode, replacementMode=replacement, colorFit=0,
            screenSpace=screen, sources=[name],
        )
        if identity[1]:
            records[stem]["paletteKey"] = f"{identity[1]:016x}"
        if replacement == "paletteDetail":
            baseline = f"originals/{stem}.png"
            (output / "originals").mkdir(exist_ok=True)
            image.save(output / baseline)
            records[stem]["baseline"] = baseline

    for name in tim_names:
        found = False
        for volume in volumes:
            for entry in read_entries(volume):
                if entry.name.casefold() != name.casefold():
                    continue
                payload = read_volume_member(volume, entry)
                if entry.name.endswith(".gz"):
                    payload = gzip.decompress(payload)
                key, image, mode = decode_tim_asset(payload)
                # A single decoded image cannot represent multiple CLUT rows.
                # Refuse to bake the first palette into every menu variant.
                size, _, _, cw, ch = struct.unpack_from("<IHHHH", payload, 8)
                if cw != (16 if mode == 0 else 256) or ch != 1:
                    raise ValueError(f"{name}: select a single-CLUT TIM; multi-palette TIM editing is not supported")
                palette_key = upload_hash(payload[20:8 + size], cw, ch)
                add((key, palette_key), image, mode, entry.name, screen=True)
                found = True
        if not found:
            raise ValueError(f"TIM not found in supplied volumes: {name}")
    for course in courses:
        images, modes, inventory = read_course_assets(volumes, course)
        for key, image in images.items():
            add((key, 0), image, modes[key], "; ".join(inventory["packages"]))
    if cars:
        images, modes, fits, replacements, inventory = read_car_assets(volumes, list(cars), None)
        for identity, image in images.items():
            add(identity, image, modes[identity], "car: " + ", ".join(cars), replacements[identity])
    if not records:
        raise ValueError("select at least one --tim, --course, or --car")
    (output / "textures.json").write_text(json.dumps(
        {"format": 1, "textures": list(records.values())}, indent=2) + "\n", encoding="utf-8")
    return len(records)


def build_project(project, output):
    data = json.loads((project / "textures.json").read_text(encoding="utf-8"))
    if data.get("format") != 1:
        raise ValueError("unsupported editable texture project format")
    entries, images, identities = [], [], set()
    for record in data["textures"]:
        identity = (int(record["key"], 16), int(record.get("paletteKey", "0"), 16))
        if identity in identities:
            raise ValueError("duplicate texture identity")
        identities.add(identity)
        image = Image.open(inside(project, record["image"])).convert("RGBA")
        w, h = record["sourceWidth"], record["sourceHeight"]
        if w <= 0 or h <= 0 or image.width % w or image.height % h or image.width // w != image.height // h:
            raise ValueError(f"{record['image']}: replacement must use a uniform integer scale")
        if image.width > 16368 or image.height > 16368:
            raise ValueError("replacement exceeds renderer atlas limit")
        mode = record["replacementMode"]
        if record["pixelMode"] not in (0, 1) or mode not in ("rgb", "paletteDetail"):
            raise ValueError("unsupported replacement mode")
        if mode == "paletteDetail":
            if identity[1] or record.get("colorFit") or record.get("screenSpace"):
                raise ValueError("car detail must use the live palette")
            original = Image.open(inside(project, record["baseline"])).convert("L")
            if original.size != (w, h):
                raise ValueError("car baseline dimensions changed")
            detail = image.convert("L")
            baseline = original.resize(image.size, Image.Resampling.BILINEAR)
            image = Image.merge("RGBA", (detail, baseline, detail, image.getchannel("A")))
        relative = f"images/{asset_stem(identity)}.dds"
        entry = {k: record[k] for k in (
            "key", "sourceWidth", "sourceHeight", "pixelMode", "replacementMode", "colorFit", "screenSpace")}
        if identity[1]:
            entry["paletteKey"] = record["paletteKey"]
        entry.update(image=relative, x=0, y=0, width=image.width, height=image.height)
        entries.append(entry)
        images.append((relative, image))
    if not entries:
        raise ValueError("texture project has no entries")
    # Validate everything before writing the pack; no AI/provenance gate for authored art.
    (output / "images").mkdir(parents=True, exist_ok=True)
    for relative, image in images:
        image.save(output / relative)
    (output / "manifest.json").write_text(json.dumps(
        {"format": 7, "generator": "OpenGTPS1 editable authored textures", "entries": entries},
        indent=2) + "\n", encoding="utf-8")
    return len(entries)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    sub = parser.add_subparsers(dest="command", required=True)
    export = sub.add_parser("export")
    export.add_argument("--volume", type=Path, action="append", required=True)
    export.add_argument("--tim", action="append", default=[])
    export.add_argument("--course", action="append", default=[])
    export.add_argument("--car", action="append", default=[])
    export.add_argument("--output", type=Path, required=True)
    build = sub.add_parser("build")
    build.add_argument("--project", type=Path, required=True)
    build.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    if args.command == "export":
        count = export_project(args.volume, args.output, args.tim, args.course, args.car)
    else:
        count = build_project(args.project, args.output)
    print(f"Wrote {count} individual textures to {args.output}")


if __name__ == "__main__":
    main()
