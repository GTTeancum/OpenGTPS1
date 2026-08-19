#!/usr/bin/env python3
"""Render disc-derived GT1/GT2 Mitsubishi racing-livery comparisons."""

from __future__ import annotations

import argparse
import gzip
import json
import shutil
from pathlib import Path

from PIL import Image

from car_livery_viewer import (
    decode_model,
    decode_texture_atlas,
    render_variant_png,
)
from gt1_convert import (
    convert_gt1_car_model,
    convert_gt1_car_texture,
    read_gt1_car_members,
    read_gt1_car_stems,
)
from gt2_vol import read_entries


FAMILIES = {
    "fto": ("m-tor", "mftgr", "mftnr", "mftor", "mftrr", "mftxr"),
    "gto": (
        "mgtlr",
        "mgnor",
        "mgntr",
        "mgoor",
        "mgotr",
        "mgtmr",
        "mgtor",
        "mgttr",
    ),
}


def read_volume_member(volume: Path, name: str) -> bytes:
    entries = {entry.name: entry for entry in read_entries(volume)}
    entry = entries.get(name)
    if entry is None:
        raise FileNotFoundError(f"{volume} contains no {name}")
    with volume.open("rb") as stream:
        stream.seek(entry.offset)
        return stream.read(entry.size)


def render_choice(
    model_data: bytes,
    texture_data: bytes,
    palette: int,
    output: Path,
    label: str,
    *,
    yaw_degrees: float = -90.0,
) -> None:
    atlas, _ = decode_texture_atlas(texture_data, palette)
    render_variant_png(
        decode_model(model_data),
        atlas,
        output,
        label,
        width=960,
        height=640,
        yaw_degrees=yaw_degrees,
        pitch_degrees=-3.0,
    )


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--gt1-disc-root", type=Path, required=True)
    parser.add_argument("--gt2-volume", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()

    output = args.output.resolve()
    if output.is_dir():
        shutil.rmtree(output)
    renders = output / "renders"
    renders.mkdir(parents=True)

    stems = read_gt1_car_stems(args.gt1_disc_root / "SYSTEM.DAT")
    manifest: dict[str, object] = {
        "formatVersion": 1,
        "authority": {
            "gt1DiscRoot": str(args.gt1_disc_root.resolve()),
            "gt2Volume": str(args.gt2_volume.resolve()),
        },
        "minimumProofTile": [720, 480],
        "families": {},
    }

    for family, family_stems in FAMILIES.items():
        rows: list[dict[str, object]] = []
        for stem in family_stems:
            gt1_texture_raw, gt1_model_raw, _, _ = read_gt1_car_members(
                args.gt1_disc_root / "CAR.DAT", stems, stem
            )
            gt2_texture = gzip.decompress(
                read_volume_member(
                    args.gt2_volume, f"carobj/{stem}.cdp.gz"
                )
            )
            gt2_model = gzip.decompress(
                read_volume_member(
                    args.gt2_volume, f"carobj/{stem}.cdo.gz"
                )
            )
            gt1_texture = convert_gt1_car_texture(gt1_texture_raw)
            gt1_model, conversion = convert_gt1_car_model(
                gt1_model_raw, gt2_model
            )
            if gt1_texture[0] != 2 or gt2_texture[0] != 2:
                raise ValueError(
                    f"expected two archive-authored paints for {stem}"
                )

            cells: list[dict[str, object]] = []
            for source, model, texture in (
                ("GT1", gt1_model, gt1_texture),
                ("GT2", gt2_model, gt2_texture),
            ):
                for palette in range(2):
                    color_id = int(texture[2 + palette])
                    filenames = {}
                    for view, yaw in (
                        ("profile", -90.0),
                        ("frontThreeQuarter", -42.0),
                        ("rearThreeQuarter", -138.0),
                    ):
                        filename = (
                            f"{stem}-{source.lower()}-p{palette}-id{color_id}-"
                            f"{view}.png"
                        )
                        render_choice(
                            model,
                            texture,
                            palette,
                            renders / filename,
                            f"{source} {stem} | palette {palette} | ID {color_id} | {view}",
                            yaw_degrees=yaw,
                        )
                        filenames[view] = str(Path("renders") / filename)
                    cells.append(
                        {
                            "source": source,
                            "palette": palette,
                            "colorId": color_id,
                            "render": filenames["profile"],
                            "renders": filenames,
                        }
                    )
            rows.append(
                {
                    "stem": stem,
                    "gt1ColorIds": list(gt1_texture[2:4]),
                    "gt2ColorIds": list(gt2_texture[2:4]),
                    "modelConversion": conversion,
                    "cells": cells,
                }
            )

        tile_width, tile_height = 720, 480
        sheet = Image.new(
            "RGB",
            (tile_width * 4, tile_height * len(rows)),
            (7, 9, 12),
        )
        for row_index, row in enumerate(rows):
            for column, cell in enumerate(row["cells"]):
                source = Image.open(output / cell["render"]).convert("RGB")
                source = source.resize(
                    (tile_width, tile_height), Image.Resampling.LANCZOS
                )
                sheet.paste(
                    source,
                    (column * tile_width, row_index * tile_height),
                )
        sheet_path = output / f"{family}-racing-livery-audit.png"
        sheet.save(sheet_path, optimize=True)
        angle_sheets = {"profile": sheet_path.name}
        for view in ("frontThreeQuarter", "rearThreeQuarter"):
            view_sheet = Image.new(
                "RGB",
                (tile_width * 4, tile_height * len(rows)),
                (7, 9, 12),
            )
            for row_index, row in enumerate(rows):
                for column, cell in enumerate(row["cells"]):
                    source = Image.open(
                        output / cell["renders"][view]
                    ).convert("RGB")
                    source = source.resize(
                        (tile_width, tile_height), Image.Resampling.LANCZOS
                    )
                    view_sheet.paste(
                        source,
                        (column * tile_width, row_index * tile_height),
                    )
            view_path = output / f"{family}-{view}-audit.png"
            view_sheet.save(view_path, optimize=True)
            angle_sheets[view] = view_path.name
        manifest["families"][family] = {
            "sheet": sheet_path.name,
            "angleSheets": angle_sheets,
            "sheetDimensions": list(sheet.size),
            "columns": ["GT1 paint 0", "GT1 paint 1", "GT2 paint 0", "GT2 paint 1"],
            "rows": rows,
        }

    (output / "manifest.json").write_text(
        json.dumps(manifest, indent=2) + "\n", encoding="utf-8"
    )
    print(f"Mitsubishi racing-livery proof ready: {output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
