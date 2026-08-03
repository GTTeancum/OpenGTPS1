#!/usr/bin/env python3
"""Inventory GT1 car graphics, specifications, and paint variants against GT2.

This is intentionally a read-only mining tool.  It turns the opaque indices in
the US GT1 disc databases into stable JSON that the first-run converter can use
when deciding which native GT2 records need to be added.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import struct
from dataclasses import dataclass
from pathlib import Path

from gt1_convert import GtArcEntry, gt1_lzss_decompress, unpack_entry
from gt2_vol import read_entries


CAR_ID_CHARACTERS = "-0123456789abcdefghijklmnopqrstuvwxyz"
GT1_SPEC_ENTRY = 13
GT1_COLOR_ENTRY = 38
GT1_SPEC_SIZE = 0x1A8
GT1_COLOR_SIZE = 0x54


@dataclass(frozen=True)
class Gt1Database:
    kind: str
    records: tuple[bytes, ...]
    strings: tuple[tuple[str, ...], ...]


def _read_gtarc(data: bytes) -> tuple[bytes, list[GtArcEntry]]:
    if data[:10] != b"@(#)GT-ARC":
        raise ValueError("data is not an uncompressed GT-ARC")
    count = struct.unpack_from("<H", data, 14)[0] & 0x7FFF
    entries: list[GtArcEntry] = []
    for index in range(count):
        offset, packed_size, unpacked_size = struct.unpack_from(
            "<III", data, 16 + index * 12
        )
        if (
            offset < 16 + count * 12
            or offset + packed_size > len(data)
            or unpacked_size <= 0
        ):
            raise ValueError(f"invalid GT-ARC entry {index}")
        entries.append(
            GtArcEntry(index, offset, packed_size, unpacked_size)
        )
    return data, entries


def _read_string_table(data: bytes, cursor: int) -> tuple[tuple[str, ...], int]:
    count = struct.unpack_from("<H", data, cursor)[0]
    cursor += 2
    strings: list[str] = []
    for _ in range(count):
        length = data[cursor]
        cursor += 1
        raw = data[cursor : cursor + length]
        cursor += length
        if cursor >= len(data) or data[cursor] != 0:
            raise ValueError("GT1 string is not null terminated")
        cursor += 1
        strings.append(raw.decode("cp932"))
    if cursor & 1:
        cursor += 1
    return tuple(strings), cursor


def read_gt1_database(data: bytes) -> Gt1Database:
    if data[:4] != b"@(#)":
        raise ValueError("GT1 database header is missing")
    kind = data[4:12].split(b"\0", 1)[0].decode("ascii")
    revision, count = struct.unpack_from("<HH", data, 12)
    zero, record_size = struct.unpack_from("<II", data, 16)
    if revision != 0x10 or zero != 0:
        raise ValueError(
            f"unsupported {kind} database header: {revision:#x}, {zero:#x}"
        )
    records_end = 24 + count * record_size
    if records_end > len(data):
        raise ValueError(f"{kind} records exceed the database")
    records = tuple(
        data[24 + index * record_size : 24 + (index + 1) * record_size]
        for index in range(count)
    )
    strings: list[tuple[str, ...]] = []
    if records_end < len(data):
        table_count = struct.unpack_from("<I", data, records_end)[0]
        cursor = records_end + 4 + table_count * 4
        for _ in range(table_count):
            table, cursor = _read_string_table(data, cursor)
            strings.append(table)
    return Gt1Database(kind, records, tuple(strings))


def read_carinf(path: Path) -> tuple[Gt1Database, Gt1Database]:
    data = path.read_bytes()
    if data[:10] != b"@(#)GT-ARC":
        data = gt1_lzss_decompress(data)
    archive, entries = _read_gtarc(data)
    if len(entries) <= GT1_COLOR_ENTRY:
        raise ValueError("GT1 CARINF archive has too few entries")
    def member(entry: GtArcEntry) -> bytes:
        if entry.packed_size == entry.unpacked_size:
            return archive[entry.offset : entry.offset + entry.packed_size]
        return unpack_entry(archive, entry)

    specs = read_gt1_database(member(entries[GT1_SPEC_ENTRY]))
    colors = read_gt1_database(member(entries[GT1_COLOR_ENTRY]))
    if (
        specs.kind != "SPEC"
        or len(specs.records[0]) != GT1_SPEC_SIZE
        or colors.kind != "COLOR"
        or len(colors.records[0]) != GT1_COLOR_SIZE
    ):
        raise ValueError("GT1 CARINF entry map does not match the US disc")
    return specs, colors


def read_gt1_graphic_stems(path: Path) -> list[str]:
    data = path.read_bytes()
    # The US SYSTEM.DAT car table is a fixed list of five-character names,
    # each followed by a null.  Locate it by its known first sentinel and
    # consume the contiguous table rather than depending on a raw offset.
    start = data.index(b"0logn\0")
    stems: list[str] = []
    cursor = start
    while cursor + 6 <= len(data):
        raw = data[cursor : cursor + 6]
        if raw[5] != 0:
            break
        try:
            stem = raw[:5].decode("ascii")
        except UnicodeDecodeError:
            break
        if any(character not in CAR_ID_CHARACTERS for character in stem):
            break
        stems.append(stem)
        cursor += 6
    if len(stems) != 344:
        raise ValueError(f"expected 344 GT1 graphic stems, found {len(stems)}")
    return stems


def read_gt1_car_graphics(
    path: Path, stems: list[str]
) -> dict[str, dict[str, object]]:
    archive, entries = _read_gtarc(path.read_bytes())
    if len(entries) != len(stems) * 4:
        raise ValueError(
            f"GT1 CAR.DAT has {len(entries)} entries for {len(stems)} stems"
        )
    result: dict[str, dict[str, object]] = {}
    for stem_index, stem in enumerate(stems):
        day_index = stem_index * 2
        night_index = len(stems) * 2 + stem_index * 2
        members = [
            unpack_entry(archive, entries[day_index]),
            unpack_entry(archive, entries[day_index + 1]),
            unpack_entry(archive, entries[night_index]),
            unpack_entry(archive, entries[night_index + 1]),
        ]
        expected_headers = (
            b"@(#)GT-CTEX",
            b"@(#)GT-CAR",
            b"@(#)GT-CTEX",
            b"@(#)GT-CAR",
        )
        for member_index, (member, header) in enumerate(
            zip(members, expected_headers)
        ):
            if not member.startswith(header):
                raise ValueError(
                    f"GT1 car {stem} member {member_index} has an "
                    f"unexpected header"
                )
        hashes = [hashlib.sha256(member).hexdigest() for member in members]
        result[stem] = {
            "members": [
                {
                    "role": role,
                    "size": len(member),
                    "sha256": digest,
                }
                for role, member, digest in zip(
                    ("dayTexture", "dayModel", "nightTexture", "nightModel"),
                    members,
                    hashes,
                )
            ],
            "quartetSha256": hashlib.sha256(
                "".join(hashes).encode("ascii")
            ).hexdigest(),
            "texturePairSha256": hashlib.sha256(
                (hashes[0] + hashes[2]).encode("ascii")
            ).hexdigest(),
            "modelPairSha256": hashlib.sha256(
                (hashes[1] + hashes[3]).encode("ascii")
            ).hexdigest(),
        }
    return result


def decode_gt2_car_id(car_id: int) -> str:
    characters: list[str] = []
    for shift in range(24, -1, -6):
        index = (car_id >> shift) & 0x3F
        if index >= len(CAR_ID_CHARACTERS):
            raise ValueError(f"invalid GT2 car ID character {index}")
        characters.append(CAR_ID_CHARACTERS[index])
    return "".join(characters)


def read_gt2_carinfo(path: Path) -> dict[str, dict[str, object]]:
    data = path.read_bytes()
    if data[:4].lower() != b"car\0":
        raise ValueError("GT2 carinfo header is missing")
    count = struct.unpack_from("<I", data, 4)[0]
    result: dict[str, dict[str, object]] = {}
    for index in range(count):
        car_id, encoded = struct.unpack_from("<II", data, 8 + index * 8)
        color_count = ((encoded >> 18) & 0x1F) + 1
        offset = encoded & 0x3FFFF
        name_offset = offset + color_count * 2 + color_count + 1
        name_end = data.index(0, name_offset)
        name = data[name_offset:name_end].decode("cp1252")
        stem = decode_gt2_car_id(car_id)
        result[stem] = {
            "index": index,
            "name": name.replace("\x7f", "[R]"),
            "colorCount": color_count,
        }
    if len(result) != count:
        raise ValueError("GT2 carinfo contains duplicate car IDs")
    return result


def gt1_specs(database: Gt1Database) -> list[dict[str, object]]:
    if len(database.strings) != 2:
        raise ValueError("GT1 SPEC does not have two name tables")
    specs: list[dict[str, object]] = []
    for index, record in enumerate(database.records, start=1):
        stem = record[:5].decode("ascii")
        first_index, first_table, second_index, second_table = (
            struct.unpack_from("<HHHH", record, 0x188)
        )
        if first_table != 0 or second_table != 1:
            raise ValueError(f"GT1 SPEC {stem} has unexpected name tables")
        first = database.strings[0][first_index]
        second = database.strings[1][second_index]
        specs.append(
            {
                "id": index,
                "stem": stem,
                "name": " ".join(part for part in (first, second) if part),
                "price": struct.unpack_from("<I", record, 0x184)[0],
                "isRacing": bool(record[0x1A1]),
            }
        )
    return specs


def gt1_colors(
    database: Gt1Database, specs_by_id: dict[int, dict[str, object]]
) -> dict[str, list[dict[str, object]]]:
    if len(database.strings) != 16:
        raise ValueError("GT1 COLOR does not have sixteen name tables")
    result: dict[str, list[dict[str, object]]] = {}
    for record in database.records:
        car_id = struct.unpack_from("<H", record, 0)[0]
        spec = specs_by_id.get(car_id)
        if spec is None:
            raise ValueError(f"GT1 COLOR references missing car ID {car_id}")
        variants: list[dict[str, object]] = []
        for slot in range(16):
            color_id = record[2 + slot]
            string_index, table_index = struct.unpack_from(
                "<HH", record, 20 + slot * 4
            )
            if table_index != slot:
                raise ValueError(
                    f"GT1 COLOR {car_id} slot {slot} points to table "
                    f"{table_index}"
                )
            if color_id:
                variants.append(
                    {
                        "id": color_id,
                        "name": database.strings[slot][string_index],
                    }
                )
        result[str(spec["stem"])] = variants
    return result


def build_inventory(
    carinf_path: Path,
    system_path: Path,
    car_path: Path,
    gt2_carinfo_path: Path,
    gt2_carobj_stems: set[str],
) -> dict[str, object]:
    spec_db, color_db = read_carinf(carinf_path)
    specs = gt1_specs(spec_db)
    specs_by_id = {int(spec["id"]): spec for spec in specs}
    colors = gt1_colors(color_db, specs_by_id)
    graphics = read_gt1_graphic_stems(system_path)
    graphic_data = read_gt1_car_graphics(car_path, graphics)
    gt2_carinfo = read_gt2_carinfo(gt2_carinfo_path)
    missing = sorted(set(graphics) - gt2_carobj_stems)
    specs_by_stem = {str(spec["stem"]): spec for spec in specs}
    missing_records = []
    for stem in missing:
        spec = specs_by_stem.get(stem)
        data = graphic_data[stem]
        exact_matches = sorted(
            other
            for other, candidate in graphic_data.items()
            if other != stem
            and candidate["quartetSha256"] == data["quartetSha256"]
        )
        model_matches = sorted(
            other
            for other, candidate in graphic_data.items()
            if other != stem
            and candidate["modelPairSha256"] == data["modelPairSha256"]
        )
        texture_matches = sorted(
            other
            for other, candidate in graphic_data.items()
            if other != stem
            and candidate["texturePairSha256"] == data["texturePairSha256"]
        )
        missing_records.append(
            {
                "graphicStem": stem,
                "directSpec": spec,
                "paintVariants": colors.get(stem, []),
                "exactGt1Matches": exact_matches,
                "modelGt1Matches": model_matches,
                "textureGt1Matches": texture_matches,
                "graphics": data,
            }
        )
    return {
        "gt1": {
            "graphicStemCount": len(graphics),
            "graphicStems": graphics,
            "graphics": graphic_data,
            "specCount": len(specs),
            "specs": specs,
            "colorRecordCount": len(color_db.records),
            "colorsBySpec": colors,
        },
        "gt2": {
            "graphicStemCount": len(gt2_carobj_stems),
            "carinfoCount": len(gt2_carinfo),
            "carinfo": gt2_carinfo,
        },
        "comparison": {
            "sharedGraphicStemCount": len(set(graphics) & gt2_carobj_stems),
            "missingGraphicStemCount": len(missing),
            "missingGraphicStems": missing_records,
        },
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--carinf", type=Path, required=True)
    parser.add_argument("--system", type=Path, required=True)
    parser.add_argument("--car", type=Path, required=True)
    parser.add_argument("--gt2-carinfo", type=Path, required=True)
    parser.add_argument(
        "--gt2-volume",
        type=Path,
        required=True,
        help="materialized GT2 Arcade volume containing carobj",
    )
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()

    stems = {
        Path(entry.name).name.split(".", 1)[0]
        for entry in read_entries(args.gt2_volume)
        if entry.name.startswith("carobj/")
        and entry.name.endswith(".cdo.gz")
    }
    inventory = build_inventory(
        args.carinf, args.system, args.car, args.gt2_carinfo, stems
    )
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(
        json.dumps(inventory, indent=2) + "\n", encoding="utf-8"
    )
    comparison = inventory["comparison"]
    print(
        "GT1 car inventory: "
        f"{inventory['gt1']['specCount']} specs, "
        f"{inventory['gt1']['graphicStemCount']} graphic stems, "
        f"{inventory['gt1']['colorRecordCount']} color records"
    )
    print(
        "GT2 comparison: "
        f"{comparison['sharedGraphicStemCount']} shared stems, "
        f"{comparison['missingGraphicStemCount']} missing stems"
    )
    print(f"wrote {args.output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
