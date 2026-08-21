#!/usr/bin/env python3
"""Prepare an internal Arcade boot root backed by Simulation's GT2.VOL.

The result lives under ignored work/ and is a bring-up fixture. It preserves
the Arcade executable, overlays, XA sectors, and original LBA layout while
substituting the Simulation volume, which is a verified strict name superset
of the Arcade volume.
"""

from __future__ import annotations

import os
import shutil
from pathlib import Path

from gt2_vol import read_entries


REPO = Path(__file__).resolve().parents[1]
ARCADE_DISC = Path(
    os.environ.get("GT2_ARCADE_DISC_ROOT", REPO / "work" / "arcade-disc")
).resolve()
ARCADE_IMAGE = Path(
    os.environ.get("GT2_ARCADE_IMAGE", REPO / "SCUS_944.55.img")
).resolve()
SIMULATION_VOL = Path(
    os.environ.get("GT2_SIMULATION_VOL", REPO / "work" / "disc" / "GT2.VOL")
).resolve()
INSTALL = Path(
    os.environ.get("GT2_ARCADE_LOOSE_ROOT", REPO / "work" / "arcade-unified")
).resolve()

RAW_SECTOR_SIZE = 2352
RAW_PAYLOAD_OFFSET = 16
RAW_PAYLOAD_SIZE = 2336
RAW_FILES = {
    "DISC_META.DAT": (16, 8 * 2048),
    "MUSIC.DAT": (104767, 85262336),
    "STREAM.DAT": (146399, 335011840),
}
ARCADE_BOOTSTRAP = ("SYSTEM.CNF", "SCUS_944.55", "GT2.OVL")


def copy_if_needed(source: Path, destination: Path) -> None:
    if not source.is_file():
        raise FileNotFoundError(f"required source is missing: {source}")
    if destination.is_file() and destination.stat().st_size == source.stat().st_size:
        return
    shutil.copy2(source, destination)
    print(f"copied {source.name}: {destination}")


def extract_raw_file(name: str, lba: int, logical_size: int) -> None:
    sectors = (logical_size + 2047) // 2048
    expected_size = sectors * RAW_PAYLOAD_SIZE
    destination = INSTALL / name
    if destination.is_file() and destination.stat().st_size == expected_size:
        return
    if not ARCADE_IMAGE.is_file():
        raise FileNotFoundError(
            f"Arcade image is required for one-time raw XA extraction: {ARCADE_IMAGE}"
        )

    temporary = destination.with_suffix(destination.suffix + ".tmp")
    with ARCADE_IMAGE.open("rb") as source, temporary.open("wb") as output:
        for sector in range(sectors):
            source.seek((lba + sector) * RAW_SECTOR_SIZE + RAW_PAYLOAD_OFFSET)
            payload = source.read(RAW_PAYLOAD_SIZE)
            if len(payload) != RAW_PAYLOAD_SIZE:
                raise IOError(f"short raw sector read for {name} at LBA {lba + sector}")
            output.write(payload)
    if temporary.stat().st_size != expected_size:
        raise IOError(f"raw loose size mismatch for {name}")
    os.replace(temporary, destination)
    print(f"extracted raw {name}: {expected_size} bytes")


def validate_volume_superset() -> None:
    arcade_volume = ARCADE_DISC / "GT2.VOL"
    if not arcade_volume.is_file():
        raise FileNotFoundError(f"Arcade volume is missing: {arcade_volume}")
    arcade_names = {entry.name.casefold() for entry in read_entries(arcade_volume)}
    simulation_names = {entry.name.casefold() for entry in read_entries(SIMULATION_VOL)}
    missing = sorted(arcade_names - simulation_names)
    if missing:
        preview = ", ".join(missing[:10])
        raise ValueError(
            f"Simulation GT2.VOL is missing {len(missing)} Arcade entries: {preview}"
        )
    print(
        "validated unified GT2.VOL: "
        f"{len(arcade_names)} Arcade names covered, "
        f"{len(simulation_names) - len(arcade_names)} additional names"
    )


def main() -> int:
    INSTALL.mkdir(parents=True, exist_ok=True)
    for name in ARCADE_BOOTSTRAP:
        copy_if_needed(ARCADE_DISC / name, INSTALL / name)

    validate_volume_superset()
    copy_if_needed(SIMULATION_VOL, INSTALL / "GT2.VOL")
    for name, (lba, logical_size) in RAW_FILES.items():
        extract_raw_file(name, lba, logical_size)

    manifest = REPO / "tools" / "recompone.arcade.loose.json"
    copy_if_needed(manifest, INSTALL / "recompone.loose.json")
    (INSTALL / "music").mkdir(exist_ok=True)
    print(f"Arcade unified loose root ready: {INSTALL}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
