#!/usr/bin/env python3
"""Inspect or extract files from Gran Turismo 2's GTFS VOL archive.

The table layout is based on adeyblue/GTVolTools (MIT):
https://github.com/adeyblue/GTVolTools
"""

from __future__ import annotations

import argparse
import dataclasses
import gzip
import os
import pathlib
import struct
import sys
from contextlib import ExitStack
from typing import Iterable


SECTOR_SIZE = 0x800
ENTRY_SIZE = 0x20


@dataclasses.dataclass(frozen=True)
class Entry:
    name: str
    offset: int
    size: int
    flags: int
    date_time: int = 0

    @property
    def is_directory(self) -> bool:
        return bool(self.flags & 1)


@dataclasses.dataclass(frozen=True)
class Member:
    """One file to stream into a newly written GTFS archive."""

    name: str
    source: pathlib.Path
    offset: int
    size: int
    date_time: int = 0


@dataclasses.dataclass
class _Directory:
    name: str
    parent: "_Directory | None"
    directories: dict[str, "_Directory"] = dataclasses.field(default_factory=dict)
    files: dict[str, Member] = dataclasses.field(default_factory=dict)
    start_index: int = 0


@dataclasses.dataclass
class _TocItem:
    name: str
    directory: _Directory | None = None
    member: Member | None = None
    parent: _Directory | None = None
    is_last: bool = False


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
    pending: list[tuple[str, int, int, int, int]] = []

    for index in range(entry_count):
        record = toc[index * ENTRY_SIZE : (index + 1) * ENTRY_SIZE]
        date_time = struct.unpack_from("<I", record, 0)[0]
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
                (
                    name,
                    aligned_offset(packed_offset),
                    packed_offset & (SECTOR_SIZE - 1),
                    flags,
                    date_time,
                )
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
    for index, (name, offset, tail_bytes, flags, date_time) in enumerate(pending):
        next_offset = pending[index + 1][1] if index + 1 < len(pending) else archive_size
        size = max(0, next_offset - offset - tail_bytes)
        if not size:
            size = SECTOR_SIZE
        entries.append(Entry(name, offset, size, flags, date_time))
    return entries


def members_from_volume(path: pathlib.Path) -> list[Member]:
    path = path.resolve()
    return [
        Member(entry.name, path, entry.offset, entry.size, entry.date_time)
        for entry in read_entries(path)
    ]


def members_from_directory(path: pathlib.Path) -> list[Member]:
    path = path.resolve()
    if not path.is_dir():
        raise FileNotFoundError(f"GTFS input directory is missing: {path}")
    members: list[Member] = []
    for source in sorted(candidate for candidate in path.rglob("*") if candidate.is_file()):
        name = source.relative_to(path).as_posix()
        members.append(Member(name, source, 0, source.stat().st_size, 0))
    return members


def _normalise_member_name(name: str) -> tuple[str, ...]:
    pure = pathlib.PurePosixPath(name.replace("\\", "/"))
    parts = pure.parts
    if (
        pure.is_absolute()
        or not parts
        or any(part in ("", ".", "..") for part in parts)
    ):
        raise ValueError(f"invalid GTFS member name: {name!r}")
    for part in parts:
        try:
            encoded = part.encode("ascii")
        except UnicodeEncodeError as exc:
            raise ValueError(f"GTFS names must be ASCII: {name!r}") from exc
        if len(encoded) > 25:
            raise ValueError(f"GTFS path component exceeds 25 bytes: {part!r}")
    return parts


def _build_toc(members: Iterable[Member]) -> tuple[bytes, list[Member]]:
    root = _Directory("", None)
    seen: set[str] = set()
    for member in members:
        parts = _normalise_member_name(member.name)
        canonical = "/".join(parts)
        folded = canonical.casefold()
        if folded in seen:
            raise ValueError(f"duplicate GTFS member name: {canonical}")
        seen.add(folded)
        directory = root
        for part in parts[:-1]:
            if part in directory.files:
                raise ValueError(f"GTFS path is both a file and directory: {part}")
            directory = directory.directories.setdefault(
                part, _Directory(part, directory)
            )
        leaf = parts[-1]
        if leaf in directory.directories:
            raise ValueError(f"GTFS path is both a file and directory: {canonical}")
        directory.files[leaf] = dataclasses.replace(member, name=canonical)

    flat: list[_TocItem] = []

    def emit(directory: _Directory) -> None:
        directory.start_index = len(flat)
        block: list[_TocItem] = []
        if directory.parent is not None:
            block.append(_TocItem("..", parent=directory.parent))
        for name, child in directory.directories.items():
            block.append(_TocItem(name, directory=child))
        for name, member in directory.files.items():
            block.append(_TocItem(name, member=member))
        block.sort(key=lambda item: item.name)
        if block:
            block[-1].is_last = True
        flat.extend(block)
        for child in sorted(directory.directories.values(), key=lambda item: item.name):
            emit(child)

    emit(root)
    file_order = [item.member for item in flat if item.member is not None]
    if len(flat) > 0x7FFF or len(file_order) + 3 > 0x7FFF:
        raise ValueError("GTFS archive exceeds signed 16-bit table limits")

    next_file_offset_index = 2
    toc = bytearray()
    for item in flat:
        flags = 0
        date_time = 0
        if item.member is not None:
            offset_index = next_file_offset_index
            next_file_offset_index += 1
            date_time = item.member.date_time
        elif item.directory is not None:
            flags = 1
            offset_index = item.directory.start_index
        else:
            flags = 1
            offset_index = item.parent.start_index if item.parent is not None else 0
        if item.is_last:
            flags |= 0x80
        leaf = item.name.encode("ascii")
        toc.extend(struct.pack("<IhB", date_time, offset_index, flags))
        toc.extend(leaf)
        toc.extend(b"\0" * (25 - len(leaf)))
    return bytes(toc), [member for member in file_order if member is not None]


def write_volume(
    destination: pathlib.Path,
    members: Iterable[Member],
) -> None:
    """Write a deterministic GTFS archive without loading member data in memory."""
    destination = destination.resolve()
    toc, file_order = _build_toc(members)
    offset_count = len(file_order) + 3
    entry_count = len(toc) // ENTRY_SIZE
    header_size = 16 + offset_count * 4
    header_padding = (-header_size) % SECTOR_SIZE
    toc_start = header_size + header_padding
    toc_padding = (-len(toc)) % SECTOR_SIZE
    cursor = toc_start + len(toc) + toc_padding

    offsets = [0] * offset_count
    offsets[0] = header_padding
    offsets[1] = toc_start | toc_padding
    paddings: list[int] = []
    for index, member in enumerate(file_order):
        padding = (-member.size) % SECTOR_SIZE
        offsets[index + 2] = cursor | padding
        paddings.append(padding)
        cursor += member.size + padding
    offsets[-1] = cursor
    if cursor > 0xFFFFFFFF:
        raise ValueError("GTFS archive exceeds 32-bit offset range")

    destination.parent.mkdir(parents=True, exist_ok=True)
    temporary = destination.with_suffix(destination.suffix + ".tmp")
    with ExitStack() as stack, temporary.open("wb") as output:
        output.write(b"GTFS\0\0\0\0")
        output.write(struct.pack("<HHI", offset_count, entry_count, 0))
        output.write(struct.pack(f"<{offset_count}I", *offsets))
        output.write(b"\0" * header_padding)
        output.write(toc)
        output.write(b"\0" * toc_padding)

        shared_streams: dict[pathlib.Path, object] = {}
        for member, padding in zip(file_order, paddings):
            source_path = member.source.resolve()
            stream = shared_streams.get(source_path)
            if stream is None:
                stream = stack.enter_context(source_path.open("rb"))
                shared_streams[source_path] = stream
            stream.seek(member.offset)
            remaining = member.size
            while remaining:
                chunk = stream.read(min(1024 * 1024, remaining))
                if not chunk:
                    raise IOError(
                        f"short read for {member.name} from {member.source}"
                    )
                output.write(chunk)
                remaining -= len(chunk)
            output.write(b"\0" * padding)
        if output.tell() != cursor:
            raise IOError(
                f"GTFS output size mismatch: {output.tell()} != {cursor}"
            )
    os.replace(temporary, destination)


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
