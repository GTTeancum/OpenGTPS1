#!/usr/bin/env python3
"""Map known GT2 functions between closely related PS-X executables.

Linked retail variants move many SDK and support routines while retaining the
same MIPS instruction structure. This tool masks relocatable immediates and
jump targets, then finds unique structural fingerprints. Every proposed
compatibility patch can therefore be reviewed against the Arcade binary rather
than copied from Simulation by address.
"""

from __future__ import annotations

import argparse
import struct
from pathlib import Path


PSX_HEADER_SIZE = 0x800
DEFAULT_LOAD_ADDRESS = 0x80010000


def words(path: Path, raw_binary: bool) -> tuple[int, ...]:
    raw = path.read_bytes()
    if raw_binary:
        payload = raw
    else:
        if len(raw) <= PSX_HEADER_SIZE or raw[:8] != b"PS-X EXE":
            raise ValueError(f"not a PS-X executable: {path}")
        payload = raw[PSX_HEADER_SIZE:]
    return struct.unpack(f"<{len(payload) // 4}I", payload[: len(payload) & ~3])


def structural_word(word: int) -> int:
    opcode = word >> 26
    if opcode == 0:
        return word
    if opcode in (2, 3):
        return word & 0xFC000000
    # Immediate operands include linked globals, branches, stack sizes, and
    # constants. Retain the opcode/register shape; longer fingerprints recover
    # specificity without assuming any immediate is invariant.
    return word & 0xFFFF0000


def fingerprint(program: tuple[int, ...], index: int, length: int) -> tuple[int, ...]:
    return tuple(structural_word(word) for word in program[index : index + length])


def parse_address(value: str) -> int:
    return int(value, 0)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("target", type=Path)
    parser.add_argument("addresses", nargs="+", type=parse_address)
    parser.add_argument("--load-address", type=parse_address, default=DEFAULT_LOAD_ADDRESS)
    parser.add_argument("--words", type=int, default=20)
    parser.add_argument(
        "--raw",
        action="store_true",
        help="treat both inputs as headerless binaries loaded at --load-address",
    )
    args = parser.parse_args()

    source = words(args.source, args.raw)
    target = words(args.target, args.raw)
    target_fingerprints: dict[tuple[int, ...], list[int]] = {}
    for index in range(0, len(target) - args.words + 1):
        key = fingerprint(target, index, args.words)
        target_fingerprints.setdefault(key, []).append(index)

    failed = False
    for address in args.addresses:
        source_index = (address - args.load_address) // 4
        if (
            address < args.load_address
            or (address - args.load_address) % 4
            or source_index + args.words > len(source)
        ):
            print(f"0x{address:08X}: source address out of range")
            failed = True
            continue
        matches = target_fingerprints.get(
            fingerprint(source, source_index, args.words), []
        )
        mapped = [args.load_address + index * 4 for index in matches]
        if len(mapped) == 1:
            delta = mapped[0] - address
            print(f"0x{address:08X} -> 0x{mapped[0]:08X} (delta {delta:+#x})")
        else:
            formatted = ", ".join(f"0x{candidate:08X}" for candidate in mapped[:8])
            print(
                f"0x{address:08X}: {len(mapped)} candidate(s)"
                + (f": {formatted}" if formatted else "")
            )
            failed = True
    return 1 if failed else 0


if __name__ == "__main__":
    raise SystemExit(main())
