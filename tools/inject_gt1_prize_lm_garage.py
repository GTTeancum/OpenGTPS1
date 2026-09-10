#!/usr/bin/env python3
"""Build and validate a native GT2 garage containing GT1 prize/LM paints."""

from __future__ import annotations

import argparse
import binascii
import gzip
import json
import struct
import tempfile
from dataclasses import dataclass
from pathlib import Path

from gt1_convert import (
    GT1_PROVEN_DISTINCT_RACING_MODIFICATION_STEMS,
    GT2_GTD_PART_RECORD_SIZES,
    GT2_GTMODE_BLOCK_COUNT,
    GT2_GTMODE_CAR_BLOCK,
    _build_gt2_used_car_database,
    _find_gt2_gtdt_car,
    _parse_gt2_used_car_database,
    _parse_gtdt_blocks,
    encode_gt2_car_id,
    read_gt2_member,
)
from gt2_vol import members_from_directory, write_volume


CARD_BYTES = 0x20000
BLOCK_BYTES = 0x2000
DIRECTORY_FRAME_BYTES = 0x80
SAVE_BYTES = 4 * BLOCK_BYTES
GT2_GAME_IDS = {b"BASCUS-94455GAME", b"BASCUS-94488GAME"}
USED_CAR_DATABASES = (".usedcar", ".usedcar_jpn", ".usedcar_usa")
MAZDA_MANUFACTURER_ID = 18
SMOKE_PRICE = 100
CAR_COUNT_OFFSET = 15988
FIRST_CAR_OFFSET = 15992
CAR_SIZE = 164
MAX_CARS = 100
CURRENT_CAR_OFFSET = 32396
CRC32_OFFSET = 32412
GTMODE_DATA_MEMBER = "carparam/usa_gtmode_data.dat.gz"
GARAGE_REF_BLOCKS = (
    0, 1, 2, 3, 6, 13, 17, 18, 21, 22, 23, 4, 5, 7,
    8, 9, 10, 11, 12, 14, 15, 16, 20, 19, 27, 28,
)
EXTRA_GTD_RECORD_SIZES = {
    27: 0x10,
    28: 0x10,
}
NATIVE_RACE_GEAR_SETTINGS = {
    "v-rbr": (3471, 3799, 2489, 1788, 1349, 1070, 891, -32760),
    "tsplr": (3323, 3645, 2359, 1683, 1262, 995, 825, 50),
}


@dataclass(frozen=True)
class GarageChoice:
    category: str
    target_stem: str
    source_stem: str
    color_index: int
    color_id: int
    body_stem: str
    body_palette_index: int


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--manifest",
        type=Path,
        default=Path("work/gt1-converted/manifest.json"),
    )
    parser.add_argument(
        "--simulation-volume",
        type=Path,
        default=Path("work/disc/GT2.VOL"),
    )
    parser.add_argument("--inventory", action="store_true")
    parser.add_argument("--build-smoke-patch", type=Path)
    parser.add_argument("--write-acquisition-fixtures", type=Path)
    parser.add_argument("--extract-record", type=Path, metavar="CARD")
    parser.add_argument("--extract-live-save", type=Path, metavar="SAVE")
    parser.add_argument("--build-direct", type=Path, metavar="SOURCE_CARD")
    parser.add_argument("--stem")
    parser.add_argument("--record-directory", type=Path)
    parser.add_argument("--assemble", type=Path, metavar="SOURCE_CARD")
    parser.add_argument("--output", type=Path)
    parser.add_argument("--validate", type=Path, metavar="CARD")
    return parser.parse_args()


def load_choices(manifest_path: Path) -> list[GarageChoice]:
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    folds = manifest["liveryFolds"]["simulation"]
    rm_sources = set(GT1_PROVEN_DISTINCT_RACING_MODIFICATION_STEMS)
    selected: list[tuple[str, dict[str, object]]] = []
    for fold in folds:
        target = str(fold["targetStem"])
        source = str(fold["sourceStem"])
        if target == "v-rbr" and source == "v-rbr":
            selected.append(("Cerbera LM", fold))
        elif target == "tsplr" and "t-plr" in source:
            selected.append(("Castrol Supra prize", fold))
    for fold in folds:
        if str(fold["sourceStem"]) in rm_sources:
            selected.append(("Racing Modification", fold))

    choices: list[GarageChoice] = []
    for category, fold in selected:
        for mapping in fold["bodyMappings"]:
            choices.append(
                GarageChoice(
                    category=category,
                    target_stem=str(fold["targetStem"]),
                    source_stem=str(fold["sourceStem"]),
                    color_index=int(mapping["targetColorIndex"]),
                    color_id=int(mapping["colorId"]),
                    body_stem=str(fold["bodyStem"]),
                    body_palette_index=int(mapping["bodyPaletteIndex"]),
                )
            )
    counts = {
        "Cerbera LM": 2,
        "Castrol Supra prize": 3,
        "Racing Modification": 62,
    }
    actual = {
        category: sum(choice.category == category for choice in choices)
        for category in counts
    }
    if actual != counts or len(choices) != 67:
        raise ValueError(
            f"unexpected GT1 prize/LM inventory: {actual}, total={len(choices)}"
        )
    if len({(c.target_stem, c.color_index) for c in choices}) != len(choices):
        raise ValueError("GT1 prize/LM inventory has duplicate car/paint choices")
    return choices


def acquisition_targets(choices: list[GarageChoice]) -> list[GarageChoice]:
    targets: list[GarageChoice] = []
    seen: set[str] = set()
    for choice in choices:
        if choice.target_stem not in seen:
            targets.append(choice)
            seen.add(choice.target_stem)
    if len(targets) != 33:
        raise ValueError(f"expected 33 native acquisition targets, got {len(targets)}")
    return targets


def inventory_dict(choices: list[GarageChoice]) -> dict[str, object]:
    targets = acquisition_targets(choices)
    return {
        "garageChoiceCount": len(choices),
        "nativeAcquisitionCount": len(targets),
        "categories": {
            category: sum(choice.category == category for choice in choices)
            for category in (
                "Cerbera LM",
                "Castrol Supra prize",
                "Racing Modification",
            )
        },
        "acquisitionTargets": [choice.target_stem for choice in targets],
        "choices": [choice.__dict__ for choice in choices],
    }


def build_smoke_patch(
    simulation_volume: Path,
    choices: list[GarageChoice],
    output: Path,
) -> None:
    targets = acquisition_targets(choices)
    aliases = [
        (
            encode_gt2_car_id(choice.target_stem),
            SMOKE_PRICE,
            0,
            choice.color_id,
        )
        for choice in targets
    ]
    output.parent.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="gt1-garage-patch-") as temporary:
        root = Path(temporary)
        for name in USED_CAR_DATABASES:
            source = gzip.decompress(read_gt2_member(simulation_volume, name))
            rotations = _parse_gt2_used_car_database(source)
            for manufacturers in rotations:
                manufacturers[MAZDA_MANUFACTURER_ID][0:0] = aliases
            rebuilt = _build_gt2_used_car_database(rotations)
            verified = _parse_gt2_used_car_database(rebuilt)
            for rotation in verified:
                if rotation[MAZDA_MANUFACTURER_ID][: len(aliases)] != aliases:
                    raise ValueError(f"smoke aliases failed round-trip in {name}")
            (root / name).write_bytes(
                gzip.compress(rebuilt, compresslevel=9, mtime=0)
            )
        write_volume(output, members_from_directory(root))
    print(
        f"smoke_patch={output.resolve()} targets={len(targets)} "
        f"price={SMOKE_PRICE}"
    )


def write_acquisition_fixtures(
    choices: list[GarageChoice], destination: Path
) -> None:
    targets = acquisition_targets(choices)
    destination.mkdir(parents=True, exist_ok=True)
    prefix = [
        "# Process-local native GT2 used-car acquisition.",
        "680+8=CROSS,START",
        "760+3=RIGHT",
        "780+3=RIGHT",
        "820+4=DOWN",
        "860+4=CROSS",
        "1100+1=CAPTURE",
        # The packaged save starts on Home. Follow the map links through
        # License and Wheel Shop to East City before entering Mazda.
        "1180+4=RIGHT",
        "1220+4=UP",
        "1260+4=RIGHT",
        "1280+1=CAPTURE",
        "1300+4=CROSS",
        "1400+1=CAPTURE",
        "1420+3=RIGHT",
        "1440+4=CROSS",
        "1480+1=CAPTURE",
        "1520+4=CROSS",
        "1540+1=CAPTURE",
    ]
    purchase = [
        (1640, "RIGHT"),
        (1660, "RIGHT"),
        (1680, "CROSS"),
        (1720, "CIRCLE"),
        (1800, "CIRCLE"),
        (1880, "TRIANGLE"),
        (1960, "SQUARE"),
        (2000, "DOWN"),
        (2040, "CROSS"),
    ]
    for row, target in enumerate(targets):
        delay = row * 40
        lines = list(prefix)
        lines.extend(
            f"{1560 + index * 40}+4=DOWN" for index in range(row)
        )
        lines.append(f"{1560 + delay}+4=CROSS")
        lines.append(f"{1580 + delay}+1=CAPTURE")
        for poll, control in purchase:
            lines.append(
                f"{poll + delay}+{3 if control == 'RIGHT' else 4}={control}"
            )
            if poll not in (1600,):
                lines.append(f"{poll + delay + 20}+1=CAPTURE")
        path = destination / f"{row:02d}-{target.target_stem}.input"
        path.write_text("\n".join(lines) + "\n", encoding="ascii")
    print(f"fixtures={destination.resolve()} count={len(targets)}")


def gt2_save_chain(card: bytearray) -> tuple[bytes, list[int]]:
    for first_block in range(1, 16):
        directory = first_block * DIRECTORY_FRAME_BYTES
        if card[directory] != 0x51:
            continue
        game_id = bytes(card[directory + 10 : directory + 26])
        if game_id not in GT2_GAME_IDS:
            continue
        size = struct.unpack_from("<I", card, directory + 4)[0]
        if size != SAVE_BYTES:
            raise ValueError(f"GT2 save has {size} bytes; expected {SAVE_BYTES}")
        chain: list[int] = []
        block = first_block
        while True:
            if block < 1 or block > 15 or block in chain:
                raise ValueError("invalid or cyclic GT2 memory-card block chain")
            chain.append(block)
            next_block = struct.unpack_from(
                "<H", card, block * DIRECTORY_FRAME_BYTES + 8
            )[0]
            if next_block == 0xFFFF:
                break
            block = next_block + 1
        if len(chain) != 4:
            raise ValueError(f"GT2 save uses {len(chain)} blocks; expected 4")
        return game_id, chain
    raise ValueError("no NTSC-U GT2 save found")


def read_logical_save(card: bytearray, chain: list[int]) -> bytearray:
    return bytearray(
        b"".join(
            card[block * BLOCK_BYTES : (block + 1) * BLOCK_BYTES]
            for block in chain
        )
    )


def write_logical_save(
    card: bytearray, chain: list[int], save: bytearray
) -> None:
    for index, block in enumerate(chain):
        start = index * BLOCK_BYTES
        card[block * BLOCK_BYTES : (block + 1) * BLOCK_BYTES] = save[
            start : start + BLOCK_BYTES
        ]


def load_card(path: Path) -> tuple[bytearray, bytes, list[int], bytearray]:
    card = bytearray(path.read_bytes())
    if len(card) != CARD_BYTES:
        raise ValueError(f"raw memory card has {len(card)} bytes; expected {CARD_BYTES}")
    game_id, chain = gt2_save_chain(card)
    save = read_logical_save(card, chain)
    stored = struct.unpack_from("<I", save, CRC32_OFFSET)[0]
    actual = binascii.crc32(save[:CRC32_OFFSET]) & 0xFFFFFFFF
    if stored != actual:
        raise ValueError(
            f"GT2 CRC mismatch: stored=0x{stored:08X}, actual=0x{actual:08X}"
        )
    return card, game_id, chain, save


def garage_records(save: bytearray) -> list[bytes]:
    count = struct.unpack_from("<I", save, CAR_COUNT_OFFSET)[0]
    if count > MAX_CARS:
        raise ValueError(f"invalid GT2 garage count: {count}")
    return [
        bytes(
            save[
                FIRST_CAR_OFFSET + index * CAR_SIZE :
                FIRST_CAR_OFFSET + (index + 1) * CAR_SIZE
            ]
        )
        for index in range(count)
    ]


def extract_record(card_path: Path, stem: str, record_directory: Path) -> None:
    _, _, _, save = load_card(card_path)
    records = garage_records(save)
    expected_id = encode_gt2_car_id(stem)
    matches = [record for record in records if struct.unpack_from("<I", record)[0] == expected_id]
    if len(matches) != 1:
        found = [f"0x{struct.unpack_from('<I', record)[0]:08X}" for record in records]
        raise ValueError(
            f"expected one purchased {stem} record, found {len(matches)}; garage={found}"
        )
    record_directory.mkdir(parents=True, exist_ok=True)
    destination = record_directory / f"{stem}.bin"
    destination.write_bytes(matches[0])
    print(
        f"record={destination.resolve()} stem={stem} "
        f"color_index={matches[0][4]} bytes={len(matches[0])}"
    )


def extract_live_record(
    save_path: Path, stem: str, record_directory: Path
) -> None:
    save = bytearray(save_path.read_bytes())
    if len(save) != SAVE_BYTES:
        raise ValueError(
            f"live GT2 save has {len(save)} bytes; expected {SAVE_BYTES}"
        )
    records = garage_records(save)
    expected_id = encode_gt2_car_id(stem)
    matches = [
        record
        for record in records
        if struct.unpack_from("<I", record)[0] == expected_id
    ]
    if len(matches) != 1:
        found = [
            f"0x{struct.unpack_from('<I', record)[0]:08X}"
            for record in records
        ]
        raise ValueError(
            f"expected one live {stem} record, found {len(matches)}; "
            f"garage={found}"
        )
    record_directory.mkdir(parents=True, exist_ok=True)
    destination = record_directory / f"{stem}.bin"
    destination.write_bytes(matches[0])
    print(
        f"live_record={destination.resolve()} stem={stem} "
        f"color_index={matches[0][4]} bytes={len(matches[0])}"
    )


def gtd_record(
    blocks: list[bytes], block_index: int, record_index: int
) -> bytes:
    if block_index < len(GT2_GTD_PART_RECORD_SIZES):
        size = GT2_GTD_PART_RECORD_SIZES[block_index]
    else:
        size = EXTRA_GTD_RECORD_SIZES[block_index]
    start = record_index * size
    record = blocks[block_index][start : start + size]
    if len(record) != size:
        raise ValueError(
            f"GT2 part reference is out of range: block={block_index}, "
            f"index={record_index}"
        )
    return record


def racing_modify_record(
    blocks: list[bytes], choice: GarageChoice, car_refs: tuple[int, ...]
) -> tuple[int, bytes]:
    if choice.category != "Racing Modification":
        index = car_refs[5]
        return index, gtd_record(blocks, 5, index)
    owner_id = encode_gt2_car_id(choice.target_stem)
    body_id = encode_gt2_car_id(choice.source_stem)
    matches: list[tuple[int, bytes]] = []
    size = GT2_GTD_PART_RECORD_SIZES[5]
    for index in range(len(blocks[5]) // size):
        record = gtd_record(blocks, 5, index)
        if (
            struct.unpack_from("<I", record)[0] == owner_id
            and struct.unpack_from("<I", record, 8)[0] == body_id
            and record[14] == 1
        ):
            matches.append((index, record))
    if len(matches) != 1:
        raise ValueError(
            f"expected one racing modification for {choice.target_stem}, "
            f"found {len(matches)}"
        )
    return matches[0]


def build_direct_record(
    blocks: list[bytes], choice: GarageChoice
) -> bytes:
    car = _find_gt2_gtdt_car(
        blocks[GT2_GTMODE_CAR_BLOCK], 0x48, choice.target_stem
    )
    car_refs = struct.unpack_from("<27H", car, 4)
    rm_index, racing_modify = racing_modify_record(
        blocks, choice, car_refs
    )
    refs = [car_refs[index] for index in (
        0, 1, 2, 3, 6, 13, 18, 19, 17, 22, 23, 4,
    )]
    refs.append(rm_index)
    refs.extend(car_refs[index] for index in (
        7, 8, 9, 10, 11, 12, 14, 15, 16, 21, 20, 24, 25,
    ))
    refs.extend((0, 0))  # stock wheels, stock power multiplier
    if len(refs) != 28:
        raise AssertionError(f"unexpected garage reference count: {len(refs)}")

    record = bytearray(CAR_SIZE)
    struct.pack_into("<I", record, 0, encode_gt2_car_id(choice.target_stem))
    record[4] = choice.color_id
    struct.pack_into("<I", record, 8, 0x100)  # stock wheel filename
    struct.pack_into("<28H", record, 12, *refs)

    gear = gtd_record(blocks, 17, car_refs[18])
    ratios = struct.unpack_from("<8h", gear, 10)
    used_ratio_count = min(8, gear[9] + 1)  # reverse plus forward gears
    if any(ratio == -1 for ratio in ratios[:used_ratio_count]):
        try:
            ratios = NATIVE_RACE_GEAR_SETTINGS[choice.target_stem]
        except KeyError as exc:
            raise ValueError(
                f"{choice.target_stem} needs calculated gearbox ratios"
            ) from exc
    struct.pack_into("<8h", record, 68, *ratios)
    final_drive = struct.unpack_from("<H", gear, 26)[0]
    struct.pack_into("<H", record, 84, final_drive)
    record[86] = gear[33]
    record[87] = 0xFF

    brake_controller = gtd_record(blocks, 1, car_refs[1])
    brake_bias = brake_controller[12]
    record[88:90] = bytes((brake_bias, brake_bias))
    record[90] = racing_modify[18]
    record[91] = racing_modify[21]

    turbine = gtd_record(blocks, 12, car_refs[12])
    record[92:98] = turbine[10:16]

    suspension = gtd_record(blocks, 18, car_refs[19])
    record[98] = suspension[11]
    record[99] = suspension[14]
    record[100] = suspension[21]
    record[101] = suspension[24]
    record[102:104] = b"\x80\x80"
    record[104] = suspension[31]
    record[105] = suspension[34]
    record[106] = suspension[35]
    record[107] = suspension[36]
    record[108:110] = bytes((suspension[42], suspension[42]))
    record[110:112] = bytes((suspension[49], suspension[49]))
    record[112:114] = bytes((suspension[56], suspension[56]))
    record[114:116] = bytes((suspension[63], suspension[63]))
    record[116] = suspension[70]
    record[117] = suspension[74]

    lsd = gtd_record(blocks, 21, car_refs[17])
    record[118:124] = bytes(
        (lsd[13], lsd[23], lsd[16], lsd[26], lsd[19], lsd[29])
    )
    record[124:126] = b"\0\0"

    engine = gtd_record(blocks, 6, car_refs[6])
    struct.pack_into("<H", record, 126, struct.unpack_from("<H", engine, 10)[0])
    record[128] = 4 if choice.target_stem == "tsplr" else 0
    record[129] = struct.unpack_from("<H", car, 0x3A)[0] & 0xFF
    record[130] = 0xC2 if car_refs[12] else 0xC0
    record[131] = gear[33]
    struct.pack_into("<H", record, 132, final_drive)

    body_id = struct.unpack_from("<I", racing_modify, 8)[0]
    struct.pack_into("<I", record, 140, body_id)
    price = struct.unpack_from("<I", car, 0x44)[0]
    if choice.category == "Racing Modification":
        price += struct.unpack_from("<I", racing_modify, 4)[0]
    struct.pack_into("<I", record, 144, price)
    chassis = gtd_record(blocks, 3, car_refs[3])
    chassis_weight = struct.unpack_from("<H", chassis, 14)[0]
    displayed_weight = (chassis_weight * racing_modify[12] + 50) // 100
    struct.pack_into("<H", record, 148, displayed_weight)
    struct.pack_into("<H", record, 150, struct.unpack_from("<H", engine, 0x32)[0])
    displayed_power = struct.unpack_from("<H", engine, 0x2E)[0] | 0x8000
    struct.pack_into("<H", record, 152, displayed_power)
    if choice.category == "Racing Modification":
        record[158] = 0x04
    return bytes(record)


def build_direct_card(
    source: Path,
    output: Path,
    simulation_volume: Path,
    choices: list[GarageChoice],
) -> None:
    card, _, chain, save = load_card(source)
    gtmode_data = gzip.decompress(
        read_gt2_member(simulation_volume, GTMODE_DATA_MEMBER)
    )
    blocks = _parse_gtdt_blocks(gtmode_data, GT2_GTMODE_BLOCK_COUNT)
    garage = bytearray(CAR_SIZE * MAX_CARS)
    for index, choice in enumerate(choices):
        record = build_direct_record(blocks, choice)
        garage[index * CAR_SIZE : (index + 1) * CAR_SIZE] = record
    struct.pack_into("<I", save, CAR_COUNT_OFFSET, len(choices))
    save[FIRST_CAR_OFFSET : FIRST_CAR_OFFSET + len(garage)] = garage
    struct.pack_into("<h", save, CURRENT_CAR_OFFSET, 0)
    checksum = binascii.crc32(save[:CRC32_OFFSET]) & 0xFFFFFFFF
    struct.pack_into("<I", save, CRC32_OFFSET, checksum)
    write_logical_save(card, chain, save)
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_bytes(card)
    validate_card(output, choices)
    print(
        f"direct_card={output.resolve()} cars={len(choices)} "
        f"current=0 crc32=0x{checksum:08X}"
    )


def assemble_card(
    source: Path,
    output: Path,
    record_directory: Path,
    choices: list[GarageChoice],
) -> None:
    card, _, chain, save = load_card(source)
    records_by_stem: dict[str, bytes] = {}
    for target in acquisition_targets(choices):
        path = record_directory / f"{target.target_stem}.bin"
        record = path.read_bytes()
        if len(record) != CAR_SIZE:
            raise ValueError(f"{path} has {len(record)} bytes; expected {CAR_SIZE}")
        actual_id = struct.unpack_from("<I", record)[0]
        expected_id = encode_gt2_car_id(target.target_stem)
        if actual_id != expected_id:
            raise ValueError(
                f"{path} car ID is 0x{actual_id:08X}; expected 0x{expected_id:08X}"
            )
        records_by_stem[target.target_stem] = record

    garage = bytearray(CAR_SIZE * MAX_CARS)
    for index, choice in enumerate(choices):
        record = bytearray(records_by_stem[choice.target_stem])
        record[4] = choice.color_id
        garage[index * CAR_SIZE : (index + 1) * CAR_SIZE] = record
    struct.pack_into("<I", save, CAR_COUNT_OFFSET, len(choices))
    save[FIRST_CAR_OFFSET : FIRST_CAR_OFFSET + len(garage)] = garage
    struct.pack_into("<h", save, CURRENT_CAR_OFFSET, 0)
    checksum = binascii.crc32(save[:CRC32_OFFSET]) & 0xFFFFFFFF
    struct.pack_into("<I", save, CRC32_OFFSET, checksum)
    write_logical_save(card, chain, save)
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_bytes(card)
    validate_card(output, choices)
    print(
        f"assembled_card={output.resolve()} cars={len(choices)} "
        f"current=0 crc32=0x{checksum:08X}"
    )


def validate_card(path: Path, choices: list[GarageChoice]) -> None:
    _, game_id, chain, save = load_card(path)
    records = garage_records(save)
    expected = [
        (encode_gt2_car_id(choice.target_stem), choice.color_id)
        for choice in choices
    ]
    actual = [
        (struct.unpack_from("<I", record)[0], record[4])
        for record in records
    ]
    if actual != expected:
        raise ValueError("GT1 prize/LM garage records do not match the manifest")
    current = struct.unpack_from("<h", save, CURRENT_CAR_OFFSET)[0]
    if current != 0:
        raise ValueError(f"current garage car is {current}; expected 0")
    checksum = struct.unpack_from("<I", save, CRC32_OFFSET)[0]
    print(
        f"validated_card={path.resolve()} game_id={game_id.decode('ascii')} "
        f"blocks={','.join(str(block) for block in chain)} cars={len(records)} "
        f"unique_car_paints={len(set(actual))} crc32=0x{checksum:08X}"
    )


def main() -> int:
    args = parse_args()
    choices = load_choices(args.manifest)
    if args.inventory:
        print(json.dumps(inventory_dict(choices), indent=2))
        return 0
    if args.build_smoke_patch:
        build_smoke_patch(args.simulation_volume, choices, args.build_smoke_patch)
        return 0
    if args.write_acquisition_fixtures:
        write_acquisition_fixtures(choices, args.write_acquisition_fixtures)
        return 0
    if args.extract_record:
        if not args.stem or not args.record_directory:
            raise ValueError("--extract-record requires --stem and --record-directory")
        extract_record(args.extract_record, args.stem, args.record_directory)
        return 0
    if args.extract_live_save:
        if not args.stem or not args.record_directory:
            raise ValueError(
                "--extract-live-save requires --stem and --record-directory"
            )
        extract_live_record(
            args.extract_live_save, args.stem, args.record_directory
        )
        return 0
    if args.build_direct:
        if not args.output:
            raise ValueError("--build-direct requires --output")
        build_direct_card(
            args.build_direct,
            args.output,
            args.simulation_volume,
            choices,
        )
        return 0
    if args.assemble:
        if not args.output or not args.record_directory:
            raise ValueError("--assemble requires --output and --record-directory")
        assemble_card(args.assemble, args.output, args.record_directory, choices)
        return 0
    if args.validate:
        validate_card(args.validate, choices)
        return 0
    raise ValueError("select one operation")


if __name__ == "__main__":
    raise SystemExit(main())
