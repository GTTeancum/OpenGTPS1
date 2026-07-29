#!/usr/bin/env python3
"""Prepare or validate GT2's standalone loose-file deployment.

The archival image is needed only to extract a missing raw-sector file. Once
the loose install exists, normal builds never open or require the image.
"""

from __future__ import annotations

import os
import shutil
import hashlib
from pathlib import Path


REPO = Path(__file__).resolve().parents[1]
INSTALL = REPO / "OpenGTPS1"
IMAGE = Path(os.environ.get(
    "GT2_ARCHIVAL_IMAGE",
    REPO / "Gran Turismo 2 [Simulation Disc] [U] [SCUS-94488].img",
))
EXTRACTED = REPO / "work" / "disc"
RAW_SECTOR_SIZE = 2352
RAW_PAYLOAD_OFFSET = 16
RAW_PAYLOAD_SIZE = 2336

RAW_FILES = {
    "MUSIC.DAT": (238872, 85262336),
    "FAULTY.PSX": (280504, 27648000),
}
COOKED_FILES = ("SYSTEM.CNF", "SCUS_944.88", "GT2.OVL", "GT2.VOL")
EXPECTED_SHA256 = {
    "DISC_META.DAT": "79c869a7b66685b1ec4618b3b0a83dfd3ae59b994741be74179d092e4a4370d6",
    "SYSTEM.CNF": "667aa36661aaa5f514c851dfb3d7ecd847d7cf4b6911ec0523a63c515779ef7f",
    "SCUS_944.88": "4dd40d01a3e83967e2d4301106890eb314d72027802bee077bbbc246f152e331",
    "GT2.OVL": "f8c6b8d94b5a5744e97b626cef1db247d49a1fc195a7615031f8468b6a86b074",
    "GT2.VOL": "9630aad04cabf50ad702a3dbbce77069153748385bcffc02015797a0c708eedd",
    "MUSIC.DAT": "2d1b7a30f656900213fa1f4aff9371c22db9d7f41ae8a9e5c5d51de8ffc9e102",
    "FAULTY.PSX": "4d2c78c7430db9a6329a454e8c736d488634e921b203a0ea889fcaf2bac0ff6e",
}


def extract_raw_file(name: str, lba: int, logical_size: int) -> None:
    sectors = (logical_size + 2047) // 2048
    expected_size = sectors * RAW_PAYLOAD_SIZE
    destination = INSTALL / name
    if destination.is_file() and destination.stat().st_size == expected_size:
        print(f"loose raw file already current: {destination}")
        return
    if not IMAGE.is_file():
        raise FileNotFoundError(
            f"loose runtime file is missing or has the wrong size: {destination}. "
            "Restore that loose file, or temporarily provide the archival image "
            f"for one-time extraction: {IMAGE}"
        )

    temporary = destination.with_suffix(destination.suffix + ".tmp")
    with IMAGE.open("rb") as source, temporary.open("wb") as output:
        for sector in range(sectors):
            source.seek((lba + sector) * RAW_SECTOR_SIZE + RAW_PAYLOAD_OFFSET)
            payload = source.read(RAW_PAYLOAD_SIZE)
            if len(payload) != RAW_PAYLOAD_SIZE:
                raise IOError(f"short raw sector read for {name} at LBA {lba + sector}")
            output.write(payload)
    if temporary.stat().st_size != expected_size:
        raise IOError(f"raw loose size mismatch for {name}")
    os.replace(temporary, destination)
    print(f"extracted loose raw file: {destination} ({expected_size} bytes)")


def extract_disc_metadata() -> None:
    destination = INSTALL / "DISC_META.DAT"
    expected_size = 8 * RAW_PAYLOAD_SIZE
    if destination.is_file() and destination.stat().st_size == expected_size:
        return
    if not IMAGE.is_file():
        raise FileNotFoundError(
            f"loose runtime file is missing or has the wrong size: {destination}. "
            "Restore it, or temporarily provide the archival image for one-time extraction."
        )
    temporary = destination.with_suffix(destination.suffix + ".tmp")
    with IMAGE.open("rb") as source, temporary.open("wb") as output:
        for lba in range(16, 24):
            source.seek(lba * RAW_SECTOR_SIZE + RAW_PAYLOAD_OFFSET)
            sector = source.read(RAW_PAYLOAD_SIZE)
            if len(sector) != RAW_PAYLOAD_SIZE:
                raise IOError(f"short metadata sector read at LBA {lba}")
            output.write(sector)
    os.replace(temporary, destination)
    print(f"extracted loose disc metadata: {destination}")


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def validate_install_hashes() -> None:
    for name, expected in EXPECTED_SHA256.items():
        path = INSTALL / name
        if not path.is_file():
            raise FileNotFoundError(f"required loose runtime file is missing: {path}")
        actual = sha256(path)
        if actual != expected:
            raise IOError(
                f"loose runtime file failed SHA-256 validation: {path}; "
                f"expected {expected}, got {actual}"
            )
        print(f"validated loose file: {name} sha256={actual}")


def main() -> int:
    INSTALL.mkdir(exist_ok=True)
    for name in COOKED_FILES:
        source = EXTRACTED / name
        if not source.is_file():
            raise FileNotFoundError(f"extracted loose source is missing: {source}")
        destination = INSTALL / name
        if not destination.is_file() or destination.stat().st_size != source.stat().st_size:
            shutil.copy2(source, destination)
            print(f"copied loose file: {destination}")

    extract_disc_metadata()
    for name, (lba, logical_size) in RAW_FILES.items():
        extract_raw_file(name, lba, logical_size)
    validate_install_hashes()

    manifest = REPO / "tools" / "recompone.loose.json"
    if not manifest.is_file():
        raise FileNotFoundError(f"loose manifest template is missing: {manifest}")
    shutil.copy2(manifest, INSTALL / manifest.name)
    (INSTALL / "music").mkdir(exist_ok=True)
    print("loose install ready; runtime no longer requires BIN/CUE")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
