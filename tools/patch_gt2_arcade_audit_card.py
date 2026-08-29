#!/usr/bin/env python3
"""Create a checksum-valid GT2 Arcade audit card from a native GT2 save."""

from __future__ import annotations

import argparse
import binascii
import shutil
import struct
from pathlib import Path


CARD_BYTES = 0x20000
BLOCK_BYTES = 0x2000
DIRECTORY_FRAME_BYTES = 0x80
SAVE_BYTES = 4 * BLOCK_BYTES
GT2_GAME_IDS = {b"BASCUS-94455GAME", b"BASCUS-94488GAME"}

ARCADE_PROGRESS_OFFSET = 696
ARCADE_COURSE_COUNT = 21
ARCADE_DIFFICULT = 4
ENDING_MOVIE_OFFSET = 1045
LICENSE_OFFSETS = (5657, 7297, 8937, 10577, 12217, 13857)
LICENSE_TEST_COUNT = 10
LICENSE_TEST_STRIDE = 164
LICENSE_GOLD = 4
MONEY_OFFSET = 32392
MONEY = 100_000
CRC32_OFFSET = 32412


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description=(
            "Set all 21 Arcade courses to Difficult, all licenses to gold, "
            "and the ending flag in a raw GT2 memory-card image."
        )
    )
    parser.add_argument("card", type=Path, help="input raw 128 KiB card image")
    parser.add_argument(
        "--output",
        type=Path,
        help="output card image (defaults to updating the input image)",
    )
    parser.add_argument(
        "--validate-only",
        action="store_true",
        help="verify the completed Arcade save without changing either card",
    )
    return parser.parse_args()


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
            raise ValueError(
                f"GT2 save {game_id.decode('ascii')} has {size} bytes; "
                f"expected {SAVE_BYTES}"
            )

        chain: list[int] = []
        block = first_block
        while len(chain) < 15:
            if block < 1 or block > 15 or block in chain:
                raise ValueError("invalid or cyclic memory-card block chain")
            chain.append(block)
            entry = block * DIRECTORY_FRAME_BYTES
            next_block = struct.unpack_from("<H", card, entry + 8)[0]
            if next_block == 0xFFFF:
                break
            block = next_block + 1
        if len(chain) != 4:
            raise ValueError(f"GT2 save uses {len(chain)} blocks; expected 4")
        return game_id, chain
    raise ValueError("no NTSC-U GT2 save found on the memory card")


def read_logical_save(card: bytearray, chain: list[int]) -> bytearray:
    save = bytearray()
    for block in chain:
        start = block * BLOCK_BYTES
        save.extend(card[start : start + BLOCK_BYTES])
    return save


def write_logical_save(
    card: bytearray, chain: list[int], save: bytearray
) -> None:
    if len(save) != SAVE_BYTES:
        raise ValueError(f"logical GT2 save has {len(save)} bytes")
    for index, block in enumerate(chain):
        source = index * BLOCK_BYTES
        destination = block * BLOCK_BYTES
        card[destination : destination + BLOCK_BYTES] = save[
            source : source + BLOCK_BYTES
        ]


def patch_save(save: bytearray) -> None:
    save[
        ARCADE_PROGRESS_OFFSET : ARCADE_PROGRESS_OFFSET + ARCADE_COURSE_COUNT
    ] = bytes([ARCADE_DIFFICULT]) * ARCADE_COURSE_COUNT
    save[ENDING_MOVIE_OFFSET] = 1
    for license_offset in LICENSE_OFFSETS:
        for test in range(LICENSE_TEST_COUNT):
            save[license_offset + test * LICENSE_TEST_STRIDE] = LICENSE_GOLD
    struct.pack_into("<I", save, MONEY_OFFSET, MONEY)
    checksum = binascii.crc32(save[:CRC32_OFFSET]) & 0xFFFFFFFF
    struct.pack_into("<I", save, CRC32_OFFSET, checksum)


def validate_save(save: bytearray) -> int:
    expected = struct.unpack_from("<I", save, CRC32_OFFSET)[0]
    actual = binascii.crc32(save[:CRC32_OFFSET]) & 0xFFFFFFFF
    if expected != actual:
        raise ValueError(
            f"GT2 CRC mismatch: stored=0x{expected:08X} actual=0x{actual:08X}"
        )
    arcade = save[
        ARCADE_PROGRESS_OFFSET : ARCADE_PROGRESS_OFFSET + ARCADE_COURSE_COUNT
    ]
    if arcade != bytes([ARCADE_DIFFICULT]) * ARCADE_COURSE_COUNT:
        raise ValueError("not every Arcade course is complete on Difficult")
    for license_offset in LICENSE_OFFSETS:
        for test in range(LICENSE_TEST_COUNT):
            if save[license_offset + test * LICENSE_TEST_STRIDE] != LICENSE_GOLD:
                raise ValueError("not every license result is gold")
    if save[ENDING_MOVIE_OFFSET] == 0:
        raise ValueError("Arcade ending remains locked")
    return actual


def main() -> int:
    args = parse_args()
    source = args.card.resolve()
    destination = (args.output or args.card).resolve()
    if not source.is_file():
        raise FileNotFoundError(source)
    if args.validate_only and args.output is not None:
        raise ValueError("--validate-only cannot be combined with --output")
    if source != destination:
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, destination)

    card = bytearray(destination.read_bytes())
    if len(card) != CARD_BYTES:
        raise ValueError(
            f"raw memory card has {len(card)} bytes; expected {CARD_BYTES}"
        )
    game_id, chain = gt2_save_chain(card)
    save = read_logical_save(card, chain)
    if args.validate_only:
        checksum = validate_save(save)
        print(
            f"arcade_audit_card={source} game_id={game_id.decode('ascii')} "
            f"blocks={','.join(str(block) for block in chain)} "
            f"arcade_difficult={ARCADE_COURSE_COUNT}/{ARCADE_COURSE_COUNT} "
            f"licenses_gold={len(LICENSE_OFFSETS) * LICENSE_TEST_COUNT}/60 "
            f"ending=unlocked credits={MONEY} crc32=0x{checksum:08X} "
            "changed=false"
        )
        return 0
    patch_save(save)
    checksum = validate_save(save)
    write_logical_save(card, chain, save)
    destination.write_bytes(card)

    reloaded = bytearray(destination.read_bytes())
    _, reloaded_chain = gt2_save_chain(reloaded)
    reloaded_crc = validate_save(read_logical_save(reloaded, reloaded_chain))
    print(
        f"arcade_audit_card={destination} game_id={game_id.decode('ascii')} "
        f"blocks={','.join(str(block) for block in chain)} "
        f"arcade_difficult={ARCADE_COURSE_COUNT}/{ARCADE_COURSE_COUNT} "
        f"licenses_gold={len(LICENSE_OFFSETS) * LICENSE_TEST_COUNT}/60 "
        f"ending=unlocked credits={MONEY} crc32=0x{reloaded_crc:08X}"
    )
    if checksum != reloaded_crc:
        raise AssertionError("CRC changed after card write")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
