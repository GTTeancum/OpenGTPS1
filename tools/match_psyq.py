#!/usr/bin/env python3
"""Find relocatable PsyQ library routines by normalized MIPS fingerprints."""

from __future__ import annotations

import json
import struct
from collections import defaultdict
from pathlib import Path


REPO = Path(__file__).resolve().parents[1]
REFERENCE_ROOT = Path(r"C:\Programming\GitHub\Vigilante-8-recomp")
REFERENCE_EXE = REFERENCE_ROOT / "PS1 game" / "SLUS_005.10"
REFERENCE_CLASSIFICATION = (
    REFERENCE_ROOT / "analysis" / "SLUS_005.10" / "classification.json"
)
TARGET_EXE = REPO / "work" / "disc" / "SCUS_944.88"


def load_exe(path: Path) -> tuple[int, list[int]]:
    data = path.read_bytes()
    load = struct.unpack_from("<I", data, 0x18)[0]
    size = struct.unpack_from("<I", data, 0x1C)[0]
    payload = data[0x800 : 0x800 + size]
    return load, list(struct.unpack(f"<{len(payload) // 4}I", payload))


def normalize(word: int) -> int:
    op = word >> 26
    if op in (2, 3):  # J/JAL: linked target changes.
        return word & 0xFC000000
    if op in (1, 4, 5, 6, 7):  # Relative control flow should remain exact.
        return word
    if op == 0:  # Register-form instructions have no linked immediate.
        return word
    if op in (16, 17, 18, 19):  # Coprocessor instructions.
        return word
    return word & 0xFFFF0000  # Ignore linked globals and literal offsets.


def main() -> int:
    ref_load, ref_words = load_exe(REFERENCE_EXE)
    target_load, target_words = load_exe(TARGET_EXE)
    classification = json.loads(REFERENCE_CLASSIFICATION.read_text(encoding="utf-8"))

    normalized_target = [normalize(word) for word in target_words]
    prefixes: dict[tuple[int, ...], list[int]] = defaultdict(list)
    for index in range(len(normalized_target) - 3):
        prefixes[tuple(normalized_target[index : index + 4])].append(index)

    matches: list[tuple[str, int, int, int]] = []
    for address_text, item in classification.items():
        if item.get("reason") != "psyq-name":
            continue
        size = int(item.get("size", 0))
        if size < 16 or size % 4:
            continue
        address = int(address_text, 16)
        start = (address - ref_load) // 4
        count = size // 4
        if start < 0 or start + count > len(ref_words):
            continue

        fingerprint = tuple(normalize(word) for word in ref_words[start : start + count])
        candidates = [
            index
            for index in prefixes.get(fingerprint[:4], [])
            if tuple(normalized_target[index : index + count]) == fingerprint
        ]
        if len(candidates) == 1:
            matches.append(
                (str(item["name"]), target_load + candidates[0] * 4, size, address)
            )

    for name, target, size, reference in sorted(matches, key=lambda item: item[1]):
        print(
            f"0x{target:08X} {name:<24} size=0x{size:X} "
            f"reference=0x{reference:08X}"
        )
    print(f"Unique PsyQ matches: {len(matches)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
