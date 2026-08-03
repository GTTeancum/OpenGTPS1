#!/usr/bin/env python3
"""Minimal raw-sector ISO9660 reader for supported PlayStation disc images."""

from __future__ import annotations

import shutil
import struct
from pathlib import Path


RAW_SECTOR_SIZE = 2352
DATA_SECTOR_SIZE = 2048
MODE1_DATA_OFFSET = 16
MODE2_FORM1_DATA_OFFSET = 24


class PsxIso:
    def __init__(self, image: Path):
        self.image = image.resolve()
        self.sector_count = self.image.stat().st_size // RAW_SECTOR_SIZE
        self.data_offset = self._detect_data_offset()
        self.stream = self.image.open("rb")

    def _detect_data_offset(self) -> int:
        with self.image.open("rb") as stream:
            for candidate in (MODE1_DATA_OFFSET, MODE2_FORM1_DATA_OFFSET):
                stream.seek(16 * RAW_SECTOR_SIZE + candidate)
                descriptor = stream.read(DATA_SECTOR_SIZE)
                if descriptor[1:6] == b"CD001":
                    return candidate
        raise ValueError(f"ISO9660 primary volume descriptor is missing: {self.image}")

    def close(self) -> None:
        self.stream.close()

    def __enter__(self) -> "PsxIso":
        return self

    def __exit__(self, *_: object) -> None:
        self.close()

    def read_sector(self, lba: int) -> bytes:
        if lba < 0 or lba >= self.sector_count:
            raise ValueError(f"LBA {lba} is outside {self.image}")
        self.stream.seek(lba * RAW_SECTOR_SIZE + self.data_offset)
        data = self.stream.read(DATA_SECTOR_SIZE)
        if len(data) != DATA_SECTOR_SIZE:
            raise IOError(f"short ISO sector read at LBA {lba}")
        return data

    def read_extent(self, lba: int, size: int) -> bytes:
        data = bytearray()
        for sector in range((size + DATA_SECTOR_SIZE - 1) // DATA_SECTOR_SIZE):
            data.extend(self.read_sector(lba + sector))
        return bytes(data[:size])

    @property
    def volume_name(self) -> str:
        return (
            self.read_sector(16)[40:72]
            .decode("ascii", errors="replace")
            .strip()
        )

    def root_record(self) -> tuple[int, int]:
        descriptor = self.read_sector(16)
        record = descriptor[156 : 156 + descriptor[156]]
        return (
            struct.unpack_from("<I", record, 2)[0],
            struct.unpack_from("<I", record, 10)[0],
        )


def directory_records(data: bytes):
    position = 0
    while position < len(data):
        length = data[position]
        if length == 0:
            position = (
                (position // DATA_SECTOR_SIZE) + 1
            ) * DATA_SECTOR_SIZE
            continue
        if length < 34 or position + length > len(data):
            raise ValueError("malformed ISO9660 directory record")
        record = data[position : position + length]
        extent = struct.unpack_from("<I", record, 2)[0]
        size = struct.unpack_from("<I", record, 10)[0]
        flags = record[25]
        name_data = record[33 : 33 + record[32]]
        if name_data == b"\0":
            name = "."
        elif name_data == b"\1":
            name = ".."
        else:
            name = name_data.decode("ascii", errors="strict").split(";", 1)[0]
        yield name, extent, size, flags
        position += length


def extract_image(image: Path, destination: Path, clean: bool = False) -> None:
    destination = destination.resolve()
    if clean and destination.exists():
        shutil.rmtree(destination)
    destination.mkdir(parents=True, exist_ok=True)

    with PsxIso(image) as iso:
        root_extent, root_size = iso.root_record()
        file_count = 0
        directory_count = 0

        def walk(extent: int, size: int, output: Path) -> None:
            nonlocal file_count, directory_count
            output.mkdir(parents=True, exist_ok=True)
            directory_count += 1
            for name, child_extent, child_size, flags in directory_records(
                iso.read_extent(extent, size)
            ):
                if name in (".", ".."):
                    continue
                child = output / name
                if flags & 2:
                    walk(child_extent, child_size, child)
                else:
                    last_sector = child_extent + (
                        child_size + DATA_SECTOR_SIZE - 1
                    ) // DATA_SECTOR_SIZE
                    if last_sector > iso.sector_count:
                        raise ValueError(
                            f"out-of-track ISO extent for {name}: "
                            f"{child_extent}+{child_size}"
                        )
                    child.write_bytes(iso.read_extent(child_extent, child_size))
                    file_count += 1

        walk(root_extent, root_size, destination)
        print(
            f"extracted volume={iso.volume_name!r} "
            f"sector_data_offset={iso.data_offset} files={file_count} "
            f"directories={directory_count} to {destination}"
        )
