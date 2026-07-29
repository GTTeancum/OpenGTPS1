#!/usr/bin/env python3
"""Inspect or extract files from Gran Turismo 2's GTFS VOL archive.

The table layout is based on adeyblue/GTVolTools (MIT):
https://github.com/adeyblue/GTVolTools
"""

from __future__ import annotations

import argparse
import dataclasses
import gzip
import pathlib
import struct
import sys


SECTOR_SIZE = 0x800
ENTRY_SIZE = 0x20


@dataclasses.dataclass(frozen=True)
class Entry:
    name: str
    offset: int
    size: int
    flags: int

    @property
    def is_directory(self) -> bool:
        return bool(self.flags & 1)


def aligned_offset(value: int) -> int:
    return value & ~(SECTOR_SIZE - 1)


def read_entries(path: pathlib.Path) -> list[Entry]:
    archive_size = path.stat().st_size
    with path.open("rb") as stream:
        header = stream.read(16)
        if len(header) != 16 or header[:8] != b"GTFS\0\0\0\0":
            raise ValueError(f"{path} is not a GT2 GTFS archive")
        file_count, entry_count = struct.unpack_from("<HH", header, 8)
        offsets = list(struct.unpack(f"<{file_count}I", stream.read(file_count * 4)))
        offsets.append(archive_size)

        toc_offset = aligned_offset(offsets[1])
        stream.seek(toc_offset)
        toc = stream.read(entry_count * ENTRY_SIZE)
        if len(toc) != entry_count * ENTRY_SIZE:
            raise ValueError("truncated GTFS table of contents")

    directories: list[str] = []
    current_directory = ""
    directory_index = 0
    directory_insert_index = 0
    pending: list[tuple[str, int, int, int]] = []

    for index in range(entry_count):
        record = toc[index * ENTRY_SIZE : (index + 1) * ENTRY_SIZE]
        offset_index = struct.unpack_from("<h", record, 4)[0]
        flags = record[6]
        leaf = record[7:32].split(b"\0", 1)[0].decode("ascii")
        name = str(pathlib.PurePosixPath(current_directory, leaf))

        if flags & 1:
            if leaf != "..":
                if current_directory:
                    directories.insert(directory_insert_index, name)
                    directory_insert_index += 1
                else:
                    directories.append(name)
        elif offset_index:
            packed_offset = offsets[offset_index]
            pending.append(
                (name, aligned_offset(packed_offset), packed_offset & (SECTOR_SIZE - 1), flags)
            )

        if flags & 0x80:
            current_directory = (
                directories[directory_index]
                if directory_index < len(directories)
                else ""
            )
            directory_index += 1
            directory_insert_index = directory_index

    entries: list[Entry] = []
    for index, (name, offset, tail_bytes, flags) in enumerate(pending):
        next_offset = pending[index + 1][1] if index + 1 < len(pending) else archive_size
        size = max(0, next_offset - offset - tail_bytes)
        if not size:
            size = SECTOR_SIZE
        entries.append(Entry(name, offset, size, flags))
    return entries


def parse_integer(value: str) -> int:
    return int(value, 0)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("archive", type=pathlib.Path)
    parser.add_argument("--at", type=parse_integer, help="show the file containing this byte offset")
    parser.add_argument("--match", help="show names containing this case-insensitive text")
    parser.add_argument("--extract", help="extract the exact named entry")
    parser.add_argument("--output", type=pathlib.Path)
    parser.add_argument("--decompress", action="store_true")
    args = parser.parse_args()

    entries = read_entries(args.archive)
    selected = entries
    if args.at is not None:
        selected = [
            entry
            for entry in entries
            if entry.offset <= args.at < entry.offset + entry.size
        ]
    if args.match:
        needle = args.match.casefold()
        selected = [entry for entry in selected if needle in entry.name.casefold()]
    if args.extract:
        selected = [entry for entry in entries if entry.name == args.extract]
        if len(selected) != 1:
            raise ValueError(f"expected one exact entry named {args.extract!r}, found {len(selected)}")
        entry = selected[0]
        output = args.output or pathlib.Path(entry.name).name
        with args.archive.open("rb") as source:
            source.seek(entry.offset)
            data = source.read(entry.size)
        if args.decompress:
            data = gzip.decompress(data)
        output.parent.mkdir(parents=True, exist_ok=True)
        output.write_bytes(data)
        print(f"extracted {entry.name} -> {output} ({len(data):#x} bytes)")
        return 0

    for entry in selected:
        print(f"{entry.offset:#010x} {entry.size:#010x} {entry.name}")
    if not selected:
        print("no matching entries", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
