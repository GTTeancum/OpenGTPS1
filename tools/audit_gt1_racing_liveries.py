#!/usr/bin/env python3
"""Census GT1 racing-modification paints against GT2 archive artwork.

The comparison is intentionally archive driven.  It decodes both indexed
texture packages, restricts comparison to texels referenced by either native
car model, and checks every GT1 paint against every GT2 paint for the same
body.  This prevents unused padding or palette-index shuffles from being
mistaken for customer-visible liveries.
"""

from __future__ import annotations

import argparse
import gzip
import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

from car_livery_viewer import decode_model
from gt1_convert import (
    convert_gt1_car_model,
    convert_gt1_car_texture,
    read_gt1_car_members,
    read_gt1_car_stems,
)
from gt2_vol import read_entries


ATLAS_WIDTH = 256
ATLAS_HEIGHT = 224 * 16


def referenced_texel_mask(model_data: bytes) -> np.ndarray:
    model = decode_model(model_data)
    uvs = np.asarray(model["uvs"], dtype=np.float32).reshape(-1, 2)
    textured = np.asarray(model["textured"], dtype=np.float32)
    mask_image = Image.new("L", (ATLAS_WIDTH, ATLAS_HEIGHT), 0)
    draw = ImageDraw.Draw(mask_image)
    for begin in range(0, len(uvs), 3):
        if textured[begin] <= 0.5:
            continue
        points = [
            (
                float(uvs[begin + corner, 0] * ATLAS_WIDTH - 0.5),
                float(uvs[begin + corner, 1] * ATLAS_HEIGHT - 0.5),
            )
            for corner in range(3)
        ]
        draw.polygon(points, fill=255)
    # Nearest sampling can select an adjacent texel on polygon boundaries.
    mask_image = mask_image.filter(ImageFilter.MaxFilter(3))
    return np.asarray(mask_image, dtype=np.uint8) != 0


def atlas(texture: bytes, palette: int) -> np.ndarray:
    if len(texture) != 0xB3A0:
        raise ValueError(f"unsupported native car texture size: {len(texture):#x}")
    packed = np.frombuffer(texture, dtype=np.uint8, offset=0x43A0)
    packed = packed.reshape(224, 128)
    indices = np.empty((224, 256), dtype=np.uint8)
    indices[:, 0::2] = packed & 15
    indices[:, 1::2] = packed >> 4
    clut_base = 0x20 + palette * 0x240
    cluts = np.frombuffer(
        texture, dtype="<u2", count=16 * 16, offset=clut_base
    ).reshape(16, 16)
    values = cluts[:, indices]
    rgba = np.empty((16, 224, 256, 4), dtype=np.uint8)
    rgba[..., 0] = ((values & 31) * 255 // 31).astype(np.uint8)
    rgba[..., 1] = (((values >> 5) & 31) * 255 // 31).astype(np.uint8)
    rgba[..., 2] = (((values >> 10) & 31) * 255 // 31).astype(np.uint8)
    rgba[..., 3] = np.where(indices[None, ...] == 0, 0, 255).astype(
        np.uint8
    )
    return rgba.reshape(ATLAS_HEIGHT, ATLAS_WIDTH, 4)


def compare_atlases(
    gt1: np.ndarray, gt2: np.ndarray, mask: np.ndarray
) -> dict[str, int | str | bool]:
    difference = np.abs(gt1.astype(np.int16) - gt2.astype(np.int16))
    changed = np.any(difference != 0, axis=2) & mask
    visible = int(np.count_nonzero(mask))
    changed_count = int(np.count_nonzero(changed))
    masked_rgba = gt1[mask].tobytes() + gt2[mask].tobytes()
    return {
        "identical": changed_count == 0,
        "referencedTexels": visible,
        "changedReferencedTexels": changed_count,
        "changedPercent": (
            round(changed_count * 100.0 / visible, 6) if visible else 0.0
        ),
        "maximumChannelDelta": (
            int(difference[changed].max()) if changed_count else 0
        ),
        "comparisonSha256": hashlib.sha256(masked_rgba).hexdigest(),
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--gt1-disc-root", type=Path, required=True)
    parser.add_argument("--gt2-volume", type=Path, required=True)
    parser.add_argument("--inventory", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()

    inventory = json.loads(args.inventory.read_text(encoding="utf-8"))
    racing_rows = inventory["racingModificationLiveries"]
    stems = read_gt1_car_stems(args.gt1_disc_root / "SYSTEM.DAT")
    volume_entries = {
        entry.name: entry for entry in read_entries(args.gt2_volume)
    }

    def volume_member(name: str) -> bytes:
        entry = volume_entries.get(name)
        if entry is None:
            raise FileNotFoundError(f"{args.gt2_volume} contains no {name}")
        with args.gt2_volume.open("rb") as stream:
            stream.seek(entry.offset)
            return stream.read(entry.size)

    results: list[dict[str, object]] = []
    counts = {
        "bodies": 0,
        "gt1Paints": 0,
        "sameIdAndReferencedTexels": 0,
        "sameIdButReferencedTexelsDiffer": 0,
        "differentIdButReferencedTexelsMatch": 0,
        "noReferencedTexelMatch": 0,
    }

    for source_row in racing_rows:
        stem = str(source_row["racingBodyStem"])
        gt1_texture_raw, gt1_model_raw, _, _ = read_gt1_car_members(
            args.gt1_disc_root / "CAR.DAT", stems, stem
        )
        gt2_texture = gzip.decompress(
            volume_member(f"carobj/{stem}.cdp.gz")
        )
        gt2_model = gzip.decompress(
            volume_member(f"carobj/{stem}.cdo.gz")
        )
        gt1_texture = convert_gt1_car_texture(gt1_texture_raw)
        try:
            gt1_model, conversion = convert_gt1_car_model(
                gt1_model_raw, gt2_model
            )
            gt1_mask = referenced_texel_mask(gt1_model)
        except ValueError as error:
            # A few bodies use a still-undocumented GT1 polygon opcode.  The
            # GT2 body is nevertheless the actual integration target and its
            # complete high-LOD UV footprint remains authoritative for
            # determining whether the GT1 paint adds visible customer art.
            gt1_mask = np.zeros(
                (ATLAS_HEIGHT, ATLAS_WIDTH), dtype=np.bool_
            )
            conversion = {
                "method": "GT2 target UV footprint fallback",
                "gt1ConversionError": str(error),
            }
        mask = gt1_mask | referenced_texel_mask(gt2_model)
        gt1_count = int(gt1_texture[0])
        gt2_count = int(gt2_texture[0])
        gt1_atlases = [atlas(gt1_texture, index) for index in range(gt1_count)]
        gt2_atlases = [atlas(gt2_texture, index) for index in range(gt2_count)]
        paint_rows: list[dict[str, object]] = []

        for gt1_index, gt1_atlas in enumerate(gt1_atlases):
            gt1_id = int(gt1_texture[2 + gt1_index])
            comparisons: list[dict[str, object]] = []
            for gt2_index, gt2_atlas in enumerate(gt2_atlases):
                gt2_id = int(gt2_texture[2 + gt2_index])
                comparison = compare_atlases(gt1_atlas, gt2_atlas, mask)
                comparisons.append(
                    {
                        "gt2Palette": gt2_index,
                        "gt2ColorId": gt2_id,
                        "sameColorId": gt1_id == gt2_id,
                        **comparison,
                    }
                )
            exact = [row for row in comparisons if row["identical"]]
            same_id = [row for row in comparisons if row["sameColorId"]]
            same_id_exact = [row for row in exact if row["sameColorId"]]
            if same_id_exact:
                classification = "same-id-and-referenced-texels"
                counts["sameIdAndReferencedTexels"] += 1
            elif same_id:
                classification = "same-id-but-referenced-texels-differ"
                counts["sameIdButReferencedTexelsDiffer"] += 1
            elif exact:
                classification = "different-id-but-referenced-texels-match"
                counts["differentIdButReferencedTexelsMatch"] += 1
            else:
                classification = "no-referenced-texel-match"
                counts["noReferencedTexelMatch"] += 1
            paint_rows.append(
                {
                    "gt1Palette": gt1_index,
                    "gt1ColorId": gt1_id,
                    "classification": classification,
                    "gt2Comparisons": comparisons,
                }
            )

        counts["bodies"] += 1
        counts["gt1Paints"] += gt1_count
        results.append(
            {
                "racingBodyStem": stem,
                "baseCarStem": source_row["baseCarStem"],
                "baseCarName": source_row["baseCarName"],
                "gt1PaintIds": list(gt1_texture[2 : 2 + gt1_count]),
                "gt2PaintIds": list(gt2_texture[2 : 2 + gt2_count]),
                "referencedTexels": int(np.count_nonzero(mask)),
                "modelConversion": conversion,
                "paints": paint_rows,
            }
        )
        print(
            f"[{counts['bodies']:03d}/{len(racing_rows):03d}] {stem} "
            f"paints={gt1_count}",
            flush=True,
        )

    report = {
        "formatVersion": 1,
        "authority": {
            "gt1DiscRoot": str(args.gt1_disc_root.resolve()),
            "gt2Volume": str(args.gt2_volume.resolve()),
            "inventory": str(args.inventory.resolve()),
                "method": (
                    "decoded RGBA comparison restricted to texels referenced by "
                    "the union of converted GT1 and native GT2 high-LOD models; "
                    "native GT2 target UV footprint alone for explicitly recorded "
                    "unsupported GT1 polygon opcodes"
                ),
        },
        "summary": counts,
        "bodies": results,
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(
        json.dumps(report, indent=2) + "\n", encoding="utf-8"
    )
    print(json.dumps(counts, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
