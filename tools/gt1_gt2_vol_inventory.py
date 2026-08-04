#!/usr/bin/env python3
"""Build a disc-derived GT1/GT2 car and livery comparison.

This tool deliberately treats the supplied archives as the authority.  It
compares GT1 CARINF.DAT/CAR.DAT with both US GT2 VOL archives, including the
native GT Mode and Arcade parameter databases.  Names from websites or
hand-authored compatibility lists are not inputs.
"""

from __future__ import annotations

import argparse
import gzip
import hashlib
import json
import struct
from collections import Counter, defaultdict
from dataclasses import dataclass
from pathlib import Path

from gt1_car_inventory import (
    _read_gtarc,
    gt1_colors,
    gt1_specs,
    read_carinf,
    read_gt1_car_graphics,
    read_gt1_graphic_stems,
)
from gt1_convert import (
    GT2_ARCADE_BLOCK_COUNT,
    GT2_ARCADE_DRIFT_BLOCK,
    GT2_ARCADE_RACING_BLOCK,
    GT2_GTD_CAR_REF_BLOCKS,
    GT2_GTD_PART_RECORD_SIZES,
    GT2_GTMODE_BLOCK_COUNT,
    GT2_GTMODE_CAR_BLOCK,
    _parse_gt2_carcolor,
    _parse_gt2_carinfo,
    _parse_gtdt_blocks,
    convert_gt1_car_model,
    convert_gt1_car_texture,
    decode_gt2_car_id,
    unpack_entry,
)
from gt2_vol import Entry, read_entries


CAR_ROLES = ("dayTexture", "dayModel", "nightTexture", "nightModel")
GT2_EXTENSIONS = ("cdp", "cdo", "cnp", "cno")


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


@dataclass
class Volume:
    path: Path

    def __post_init__(self) -> None:
        self.path = self.path.resolve()
        self.entries: dict[str, Entry] = {
            entry.name: entry for entry in read_entries(self.path)
        }
        self._stream = self.path.open("rb")

    def close(self) -> None:
        self._stream.close()

    def read(self, name: str) -> bytes:
        entry = self.entries.get(name)
        if entry is None:
            raise FileNotFoundError(f"{self.path} has no member {name}")
        self._stream.seek(entry.offset)
        data = self._stream.read(entry.size)
        if len(data) != entry.size:
            raise IOError(f"short VOL read for {name}")
        return data

    def read_gzip(self, name: str) -> bytes:
        return gzip.decompress(self.read(name))

    def member_metadata(self, name: str, *, decompress: bool = False) -> dict:
        packed = self.read(name)
        unpacked = gzip.decompress(packed) if decompress else packed
        return {
            "name": name,
            "offset": self.entries[name].offset,
            "packedSize": len(packed),
            "packedSha256": sha256(packed),
            "unpackedSize": len(unpacked),
            "unpackedSha256": sha256(unpacked),
        }


class Gt1Cars:
    def __init__(self, car_path: Path, stems: list[str]) -> None:
        self.stems = stems
        self.index = {stem: index for index, stem in enumerate(stems)}
        self.archive, self.entries = _read_gtarc(car_path.read_bytes())
        if len(self.entries) != len(stems) * 4:
            raise ValueError(
                f"GT1 CAR.DAT has {len(self.entries)} members for "
                f"{len(stems)} stems"
            )

    def members(self, stem: str) -> tuple[bytes, bytes, bytes, bytes]:
        stem_index = self.index[stem]
        night_bank = len(self.stems) * 2
        indices = (
            stem_index * 2,
            stem_index * 2 + 1,
            night_bank + stem_index * 2,
            night_bank + stem_index * 2 + 1,
        )
        return tuple(
            unpack_entry(self.archive, self.entries[index])
            for index in indices
        )


def texture_metadata(data: bytes) -> dict:
    count = data[0]
    if not 1 <= count <= 16 or len(data) != 0xB3A0:
        raise ValueError(
            f"invalid native GT2 car texture: count={count}, size={len(data):#x}"
        )
    palettes = [
        data[0x20 + index * 0x240 : 0x220 + index * 0x240]
        for index in range(count)
    ]
    return {
        "sha256": sha256(data),
        "colorCount": count,
        "colorIds": list(data[2 : 2 + count]),
        "paletteSha256": [sha256(palette) for palette in palettes],
        "bitmapSha256": sha256(data[0x43A0:0xB3A0]),
        "illuminationAndPaintMaskSha256": [
            sha256(data[0x220 + index * 0x240 : 0x260 + index * 0x240])
            for index in range(count)
        ],
    }


def parse_gt2_latin_color_names(data: bytes) -> list[str]:
    if len(data) < 4:
        raise ValueError("GT2 Latin color-name table is truncated")
    first_offset = struct.unpack_from("<H", data, 0)[0]
    if first_offset < 2 or first_offset & 1 or first_offset > len(data):
        raise ValueError(
            f"GT2 Latin color-name directory is invalid: {first_offset:#x}"
        )
    count = first_offset // 2
    offsets = list(struct.unpack_from(f"<{count}H", data, 0))
    if offsets != sorted(offsets) or offsets[0] != first_offset:
        raise ValueError("GT2 Latin color-name offsets are not ordered")
    offsets.append(len(data))
    names: list[str] = []
    for index in range(count):
        raw = data[offsets[index] : offsets[index + 1]]
        names.append(raw.split(b"\0", 1)[0].decode("cp1252"))
    return names


def gt2_car_database(volume: Volume) -> dict:
    carinfo_data = volume.read(".carinfoe")
    carinfo = _parse_gt2_carinfo(carinfo_data)
    carcolor_data = volume.read(".carcolor")
    carcolors = _parse_gt2_carcolor(carcolor_data, carinfo)
    latin_color_name_data = volume.read(".cclatain")
    latin_color_names = parse_gt2_latin_color_names(latin_color_name_data)

    gtmode_name = "carparam/usa_gtmode_data.dat.gz"
    arcade_name = "carparam/usa_arcade_data.dat.gz"
    gtmode_data = volume.read_gzip(gtmode_name)
    arcade_data = volume.read_gzip(arcade_name)
    gtmode_blocks = _parse_gtdt_blocks(
        gtmode_data, GT2_GTMODE_BLOCK_COUNT
    )
    arcade_blocks = _parse_gtdt_blocks(
        arcade_data, GT2_ARCADE_BLOCK_COUNT
    )

    carinfo_by_stem: dict[str, dict] = {}
    for index, (record, color_names) in enumerate(
        zip(carinfo, carcolors)
    ):
        stem = str(record["stem"])
        carinfo_by_stem[stem] = {
            "index": index,
            "name": bytes(record["name"]).decode("cp1252").replace(
                "\x7f", "[R]"
            ),
            "colorIds": list(record["colorIds"]),
            "mainColors": list(record["mainColors"]),
            "colorNameIndices": list(color_names),
            "colorNames": [
                latin_color_names[name_index]
                if name_index < len(latin_color_names)
                else f"<invalid:{name_index}>"
                for name_index in color_names
            ],
        }

    owned_counts: list[Counter[int]] = []
    for block_index, record_size in enumerate(GT2_GTD_PART_RECORD_SIZES):
        block = gtmode_blocks[block_index]
        if len(block) % record_size:
            raise ValueError(
                f"GT2 part block {block_index} is not divisible by "
                f"{record_size:#x}"
            )
        owned_counts.append(
            Counter(
                struct.unpack_from("<I", block, offset)[0]
                for offset in range(0, len(block), record_size)
            )
        )

    cars: dict[str, dict] = {}
    car_block = gtmode_blocks[GT2_GTMODE_CAR_BLOCK]
    if len(car_block) % 0x48:
        raise ValueError("GT2 GT Mode car block is not divisible by 0x48")
    for offset in range(0, len(car_block), 0x48):
        record = car_block[offset : offset + 0x48]
        car_id = struct.unpack_from("<I", record, 0)[0]
        stem = decode_gt2_car_id(car_id)
        references = list(struct.unpack_from("<24H", record, 4))
        base_part_owners: list[str] = []
        base_part_hashes: list[str] = []
        for reference_index, block_index in enumerate(
            GT2_GTD_CAR_REF_BLOCKS
        ):
            record_size = GT2_GTD_PART_RECORD_SIZES[block_index]
            part_offset = references[reference_index] * record_size
            part_block = gtmode_blocks[block_index]
            if part_offset + record_size > len(part_block):
                raise ValueError(
                    f"GT2 {stem} part reference {block_index} is out of range"
                )
            part = part_block[part_offset : part_offset + record_size]
            base_part_owners.append(
                decode_gt2_car_id(struct.unpack_from("<I", part, 0)[0])
            )
            base_part_hashes.append(sha256(part[4:]))

        cars[stem] = {
            "index": offset // 0x48,
            "recordSha256": sha256(record),
            "partReferences": references,
            "basePartOwners": base_part_owners,
            "basePartPayloadSha256": base_part_hashes,
            "ownedPartRecordCounts": [
                owned_counts[index][car_id]
                for index in range(len(GT2_GTD_PART_RECORD_SIZES))
            ],
            "activeStabilityControl": struct.unpack_from("<H", record, 0x34)[0],
            "tractionControlSystem": struct.unpack_from("<H", record, 0x36)[0],
            "rimsCode": struct.unpack_from("<H", record, 0x38)[0],
            "manufacturerId": struct.unpack_from("<H", record, 0x3A)[0],
            "nameFirstPartIndex": struct.unpack_from("<H", record, 0x3C)[0],
            "nameSecondPartIndex": struct.unpack_from("<H", record, 0x3E)[0],
            "hasAllTiresBought": bool(record[0x40]),
            "year": record[0x41],
            "unknown": struct.unpack_from("<H", record, 0x42)[0],
            "price": struct.unpack_from("<I", record, 0x44)[0],
            "carinfo": carinfo_by_stem.get(stem),
        }

    def arcade_ids(block_index: int) -> list[str]:
        block = arcade_blocks[block_index]
        if len(block) % 0x3C:
            raise ValueError("GT2 Arcade car block is not divisible by 0x3C")
        return [
            decode_gt2_car_id(struct.unpack_from("<I", block, offset)[0])
            for offset in range(0, len(block), 0x3C)
        ]

    racing = arcade_ids(GT2_ARCADE_RACING_BLOCK)
    drift = arcade_ids(GT2_ARCADE_DRIFT_BLOCK)
    if racing != drift:
        raise ValueError("GT2 Arcade Racing and Drift car IDs differ")

    return {
        "volume": {
            "path": str(volume.path),
            "size": volume.path.stat().st_size,
            "sha256": file_sha256(volume.path),
        },
        "members": {
            ".carinfoe": volume.member_metadata(".carinfoe"),
            ".carcolor": volume.member_metadata(".carcolor"),
            ".cclatain": volume.member_metadata(".cclatain"),
            gtmode_name: volume.member_metadata(
                gtmode_name, decompress=True
            ),
            arcade_name: volume.member_metadata(
                arcade_name, decompress=True
            ),
        },
        "carinfoCount": len(carinfo_by_stem),
        "gtmodeCarCount": len(cars),
        "arcadeCarCount": len(racing),
        "carinfo": carinfo_by_stem,
        "latinColorNameCount": len(latin_color_names),
        "latinColorNames": latin_color_names,
        "gtmodeCars": cars,
        "arcadeCars": racing,
        "_gtmodeData": gtmode_data,
        "_arcadeData": arcade_data,
    }


def file_sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def strip_private(database: dict) -> dict:
    return {
        key: value
        for key, value in database.items()
        if not key.startswith("_")
    }


def gt1_spec_metadata(
    spec_data,
) -> tuple[dict[str, dict], dict[str, list[str]]]:
    basic = gt1_specs(spec_data)
    records_by_stem = {
        record[:5].decode("ascii"): record for record in spec_data.records
    }
    physical_groups: dict[str, list[str]] = defaultdict(list)
    for stem, record in records_by_stem.items():
        # Bytes 0..4 encode the stem; bytes 0x184 onward are price, names,
        # display statistics, and classification metadata.  The middle payload
        # is GT1's complete consumed vehicle physics fingerprint.
        physical_groups[sha256(record[5:0x184])].append(stem)

    result: dict[str, dict] = {}
    for item in basic:
        stem = str(item["stem"])
        record = records_by_stem[stem]
        physical_hash = sha256(record[5:0x184])
        power, power_rpm, torque, torque_rpm = struct.unpack_from(
            "<4H", record, 0x198
        )
        result[stem] = {
            **item,
            "rawSha256": sha256(record),
            "physicalSha256": physical_hash,
            "physicalPeerStems": sorted(physical_groups[physical_hash]),
            "weightKg": struct.unpack_from("<H", record, 0x5A)[0],
            "powerPs": power,
            "powerRpm": power_rpm,
            "torqueKgfM100": torque,
            "torqueRpm": torque_rpm,
            "newOrUsed": record[0x1A0],
            "isRacing": bool(record[0x1A1]),
            "aspiration": record[0x1A3],
            "engineValvetrain": record[0x1A4],
            "engineLayout": record[0x1A5],
            "frontSuspensionType": record[0x1A6],
            "rearSuspensionType": record[0x1A7],
        }
    return result, physical_groups


def compare_native_texture(
    converted_gt1: bytes,
    native_gt2: bytes,
) -> dict:
    gt1 = texture_metadata(converted_gt1)
    gt2 = texture_metadata(native_gt2)
    gt1_palettes = gt1["paletteSha256"]
    gt2_palettes = gt2["paletteSha256"]
    return {
        "gt1": gt1,
        "gt2": gt2,
        "byteIdentical": converted_gt1 == native_gt2,
        "bitmapIdentical": (
            gt1["bitmapSha256"] == gt2["bitmapSha256"]
        ),
        "gt1PaletteGt2Indices": [
            [
                index
                for index, candidate in enumerate(gt2_palettes)
                if candidate == palette
            ]
            for palette in gt1_palettes
        ],
        "gt2PaletteGt1Indices": [
            [
                index
                for index, candidate in enumerate(gt1_palettes)
                if candidate == palette
            ]
            for palette in gt2_palettes
        ],
    }


def relevant_car_asset_names(volume: Volume) -> set[str]:
    return {
        entry.name
        for entry in volume.entries.values()
        if entry.name.startswith("carobj/")
        and any(
            entry.name.endswith(f".{extension}.gz")
            for extension in GT2_EXTENSIONS
        )
    }


def build_inventory(
    gt1_root: Path,
    simulation_volume: Volume,
    arcade_volume: Volume,
) -> dict:
    spec_data, color_data = read_carinf(gt1_root / "CARINF.DAT")
    specs, _ = gt1_spec_metadata(spec_data)
    colors = gt1_colors(
        color_data,
        {
            index: specs[record[:5].decode("ascii")]
            for index, record in enumerate(spec_data.records, start=1)
        },
    )
    global_gt1_color_names: dict[int, set[str]] = defaultdict(set)
    for variants in colors.values():
        for variant in variants:
            global_gt1_color_names[int(variant["id"])].add(
                str(variant["name"])
            )
    stems = read_gt1_graphic_stems(gt1_root / "SYSTEM.DAT")
    graphic_hashes = read_gt1_car_graphics(gt1_root / "CAR.DAT", stems)
    gt1_cars = Gt1Cars(gt1_root / "CAR.DAT", stems)

    simulation = gt2_car_database(simulation_volume)
    arcade = gt2_car_database(arcade_volume)
    simulation_assets = relevant_car_asset_names(simulation_volume)
    arcade_assets = relevant_car_asset_names(arcade_volume)
    simulation_stems = {
        Path(name).name.split(".", 1)[0] for name in simulation_assets
    }
    arcade_stems = {
        Path(name).name.split(".", 1)[0] for name in arcade_assets
    }
    gt2_gtmode_stems = set(simulation["gtmodeCars"])

    records: list[dict] = []
    paint_id_folds: list[dict] = []
    evidence_counts: Counter[str] = Counter()
    for stem in stems:
        day_texture, day_model, night_texture, night_model = (
            gt1_cars.members(stem)
        )
        converted_day = convert_gt1_car_texture(day_texture)
        converted_night = convert_gt1_car_texture(night_texture)
        spec = specs.get(stem)
        graphics = graphic_hashes[stem]
        model_peers = set(graphics["modelGt1Matches"]) if (
            "modelGt1Matches" in graphics
        ) else {
            other
            for other, candidate in graphic_hashes.items()
            if other != stem
            and candidate["modelPairSha256"] == graphics["modelPairSha256"]
        }
        physical_peers = (
            set(spec["physicalPeerStems"]) if spec is not None else set()
        )
        same_stem_gtmode = stem in gt2_gtmode_stems
        embedded_paint_ids = list(
            converted_day[2 : 2 + converted_day[0]]
        )
        gt2_carinfo = simulation["carinfo"].get(stem)
        gt2_paint_ids = (
            list(gt2_carinfo["colorIds"])
            if gt2_carinfo is not None
            else []
        )
        gt1_only_paint_ids = [
            color_id
            for color_id in embedded_paint_ids
            if color_id not in gt2_paint_ids
        ]
        gt2_only_paint_ids = [
            color_id
            for color_id in gt2_paint_ids
            if color_id not in embedded_paint_ids
        ]
        if same_stem_gtmode and gt1_only_paint_ids:
            paint_id_folds.append(
                {
                    "stem": stem,
                    "gt1Name": (
                        str(spec["name"]) if spec is not None else None
                    ),
                    "gt2Name": (
                        str(gt2_carinfo["name"])
                        if gt2_carinfo is not None
                        else None
                    ),
                    "gt1PaintIds": embedded_paint_ids,
                    "gt2PaintIds": gt2_paint_ids,
                    "gt1OnlyPaintIds": gt1_only_paint_ids,
                    "gt2OnlyPaintIds": gt2_only_paint_ids,
                }
            )
        strong_fold_targets = sorted(
            (
                physical_peers
                & (model_peers | {stem})
                & gt2_gtmode_stems
            )
            - {stem}
        )

        asset_comparison: dict[str, dict] = {}
        for label, volume in (
            ("simulation", simulation_volume),
            ("arcade", arcade_volume),
        ):
            names = [
                f"carobj/{stem}.{extension}.gz"
                for extension in GT2_EXTENSIONS
            ]
            if not all(name in volume.entries for name in names):
                asset_comparison[label] = {"present": False}
                continue
            native = {
                extension: volume.read_gzip(name)
                for extension, name in zip(GT2_EXTENSIONS, names)
            }
            model_comparison: dict[str, dict] = {}
            for role, source, extension in (
                ("day", day_model, "cdo"),
                ("night", night_model, "cno"),
            ):
                try:
                    converted, conversion = convert_gt1_car_model(
                        source, native[extension]
                    )
                    model_comparison[role] = {
                        "convertedSha256": sha256(converted),
                        "nativeSha256": sha256(native[extension]),
                        "byteIdentical": converted == native[extension],
                        "conversion": conversion,
                    }
                except (ValueError, OverflowError) as error:
                    model_comparison[role] = {
                        "nativeSha256": sha256(native[extension]),
                        "conversionError": str(error),
                    }
            asset_comparison[label] = {
                "present": True,
                "members": {
                    extension: volume.member_metadata(name, decompress=True)
                    for extension, name in zip(GT2_EXTENSIONS, names)
                },
                "dayTexture": compare_native_texture(
                    converted_day, native["cdp"]
                ),
                "nightTexture": compare_native_texture(
                    converted_night, native["cnp"]
                ),
                "models": model_comparison,
            }

        simulation_assets_for_stem = asset_comparison.get("simulation", {})
        texture_delta = bool(
            simulation_assets_for_stem.get("present")
            and (
                not simulation_assets_for_stem["dayTexture"]["byteIdentical"]
                or not simulation_assets_for_stem["nightTexture"]["byteIdentical"]
            )
        )
        if same_stem_gtmode and gt1_only_paint_ids:
            evidence_class = "same-gt2-car-with-gt1-only-paint-ids"
        elif same_stem_gtmode and texture_delta:
            evidence_class = "same-gt2-car-same-paint-ids-asset-delta"
        elif same_stem_gtmode:
            evidence_class = "shared-gt2-car-with-matching-texture"
        elif strong_fold_targets:
            evidence_class = "physical-and-model-equivalent-fold-candidate"
        elif spec is not None:
            evidence_class = "distinct-car-candidate"
        else:
            evidence_class = "graphics-only-candidate"
        evidence_counts[evidence_class] += 1

        records.append(
            {
                "stem": stem,
                "spec": spec,
                "namedPaintVariants": colors.get(stem, []),
                "embeddedPaintIds": embedded_paint_ids,
                "paintIdComparison": {
                    "gt1PaintIds": embedded_paint_ids,
                    "gt2PaintIds": gt2_paint_ids,
                    "gt1OnlyPaintIds": gt1_only_paint_ids,
                    "gt2OnlyPaintIds": gt2_only_paint_ids,
                    "authoritativeFold": bool(
                        same_stem_gtmode and gt1_only_paint_ids
                    ),
                },
                "embeddedPaintNamesFromGt1Database": {
                    str(color_id): sorted(
                        global_gt1_color_names.get(color_id, ())
                    )
                    for color_id in converted_day[
                        2 : 2 + converted_day[0]
                    ]
                },
                "graphics": graphics,
                "gt1ModelPeerStems": sorted(model_peers),
                "gt1PhysicalPeerStems": sorted(physical_peers),
                "strongFoldTargets": strong_fold_targets,
                "sameStem": {
                    "simulationCarobj": stem in simulation_stems,
                    "arcadeCarobj": stem in arcade_stems,
                    "gt2Carinfo": stem in simulation["carinfo"],
                    "gt2GtMode": same_stem_gtmode,
                    "gt2Arcade": stem in simulation["arcadeCars"],
                },
                "assets": asset_comparison,
                "evidenceClass": evidence_class,
            }
        )

    common_relevant_members = sorted(
        (
            set(simulation_volume.entries)
            & set(arcade_volume.entries)
        )
        & (
            simulation_assets
            | arcade_assets
            | {
                ".carinfoe",
                ".carcolor",
                ".cclatain",
                "carparam/usa_gtmode_data.dat.gz",
                "carparam/usa_arcade_data.dat.gz",
            }
        )
    )
    different_common_members = []
    for name in common_relevant_members:
        simulation_data = simulation_volume.read(name)
        arcade_data = arcade_volume.read(name)
        if simulation_data != arcade_data:
            different_common_members.append(
                {
                    "name": name,
                    "simulationSha256": sha256(simulation_data),
                    "arcadeSha256": sha256(arcade_data),
                    "simulationSize": len(simulation_data),
                    "arcadeSize": len(arcade_data),
                }
            )

    return {
        "authority": {
            "description": (
                "Direct comparison of supplied US GT1 CARINF.DAT/CAR.DAT "
                "and US GT2 Simulation/Arcade GT2.VOL archives"
            ),
            "gt1Carinf": {
                "path": str((gt1_root / "CARINF.DAT").resolve()),
                "size": (gt1_root / "CARINF.DAT").stat().st_size,
                "sha256": file_sha256(gt1_root / "CARINF.DAT"),
            },
            "gt1Car": {
                "path": str((gt1_root / "CAR.DAT").resolve()),
                "size": (gt1_root / "CAR.DAT").stat().st_size,
                "sha256": file_sha256(gt1_root / "CAR.DAT"),
            },
            "gt2SimulationVolume": strip_private(simulation)["volume"],
            "gt2ArcadeVolume": strip_private(arcade)["volume"],
        },
        "summary": {
            "gt1SpecCount": len(specs),
            "gt1GraphicStemCount": len(stems),
            "gt2SimulationCarinfoCount": simulation["carinfoCount"],
            "gt2SimulationGtModeCarCount": simulation["gtmodeCarCount"],
            "gt2SimulationArcadeCarCount": simulation["arcadeCarCount"],
            "sharedGt1Gt2CarobjStemCount": len(
                set(stems) & simulation_stems
            ),
            "evidenceClasses": dict(sorted(evidence_counts.items())),
            "sameCarPaintIdFoldEntryCount": len(paint_id_folds),
            "sameCarGt1OnlyPaintVariantCount": sum(
                len(item["gt1OnlyPaintIds"])
                for item in paint_id_folds
            ),
            "commonRelevantMemberCount": len(common_relevant_members),
            "differentCommonRelevantMemberCount": len(
                different_common_members
            ),
        },
        "gt2VolumeComparison": {
            "differentCommonRelevantMembers": different_common_members,
            "simulationOnlyCarAssets": sorted(
                simulation_assets - arcade_assets
            ),
            "arcadeOnlyCarAssets": sorted(
                arcade_assets - simulation_assets
            ),
        },
        "sameCarPaintIdFolds": paint_id_folds,
        "gt2SimulationDatabase": strip_private(simulation),
        "gt2ArcadeDatabase": strip_private(arcade),
        "cars": records,
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--gt1-disc-root", type=Path, required=True)
    parser.add_argument("--gt2-simulation-volume", type=Path, required=True)
    parser.add_argument("--gt2-arcade-volume", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()

    simulation = Volume(args.gt2_simulation_volume)
    arcade = Volume(args.gt2_arcade_volume)
    try:
        inventory = build_inventory(
            args.gt1_disc_root.resolve(), simulation, arcade
        )
    finally:
        simulation.close()
        arcade.close()

    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(
        json.dumps(inventory, indent=2) + "\n", encoding="utf-8"
    )
    print(json.dumps(inventory["summary"], indent=2))
    print(f"wrote {args.output.resolve()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
