"""Identify source discs by their ISO files, without dump fingerprints."""

import re
from pathlib import Path

from psx_iso import PsxIso, directory_records


# GT1's converter reads these data archives, not the regional executable.
GT1_DATA_FILES = (
    "ARCADE.DAT", "BG.DAT", "CAR.DAT", "CARINF.DAT", "COURSE.DAT",
    "MENU_IMG.ARC", "MENU_RAW.ARC", "SYSTEM.DAT",
)
GT2_BOOT_FILES = {"simulation": "SCUS_944.88", "arcade": "SCUS_944.55"}


def disc_files(iso: PsxIso) -> dict[str, tuple[int, int]]:
    extent, size = iso.root_record()
    if size <= 0 or size > 1024 * 1024:
        raise ValueError("Invalid ISO root directory size")
    files = {}
    for name, lba, length, flags in directory_records(iso.read_extent(extent, size)):
        if flags & 2:
            continue
        if name in files:
            raise ValueError(f"Duplicate ISO file: {name}")
        if length <= 0 or lba + (length + 2047) // 2048 > iso.sector_count:
            raise ValueError(f"Empty or truncated ISO file: {name}")
        files[name] = (lba, length)
    return files


def identify_disc(path: Path) -> str:
    with PsxIso(path) as iso:
        files = disc_files(iso)
        if all(name in files for name in GT1_DATA_FILES):
            return "gt1"
        if iso.data_offset != 24:
            raise ValueError("GT2 requires a raw Mode 2/2352 image")
        if "SYSTEM.CNF" not in files:
            raise ValueError("Disc has no SYSTEM.CNF or GT1 data archives")
        lba, size = files["SYSTEM.CNF"]
        if size > 4096:
            raise ValueError("Invalid SYSTEM.CNF size")
        config = iso.read_extent(lba, size).decode("ascii", errors="replace")
        boot = re.search(r"(?im)^\s*BOOT\s*=\s*cdrom:\\?([^;\s]+)", config)
        executable = boot.group(1).upper() if boot else ""
        for key, expected in GT2_BOOT_FILES.items():
            if executable != expected:
                continue
            required = [expected, "GT2.VOL", "GT2.OVL"]
            required += ["MUSIC.DAT", "FAULTY.PSX"] if key == "simulation" else ["STREAM.DAT"]
            missing = [name for name in required if name not in files]
            if missing:
                raise ValueError(f"Missing {key} disc files: {', '.join(missing)}")
            if iso.read_sector(files[expected][0])[:8] != b"PS-X EXE":
                raise ValueError(f"Invalid PlayStation executable: {expected}")
            return key
        raise ValueError("GT2 requires a USA Simulation (SCUS-94488) or Arcade (SCUS-94455) disc")
