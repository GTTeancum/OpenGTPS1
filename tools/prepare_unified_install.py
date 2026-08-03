#!/usr/bin/env python3
"""Assemble one loose install containing native Simulation and Arcade modes."""

from __future__ import annotations

import gzip
import hashlib
import json
import os
import shutil
import struct
from pathlib import Path

from gt2_patch import apply_patches
from gt2_vol import read_entries


REPO = Path(__file__).resolve().parents[1]
DEFAULT_SIMULATION_ROOT = next(
    (
        candidate
        for candidate in (
            REPO / "OpenGTPS1",
            REPO.parents[1] / "OpenGTPS1",
            REPO / "work" / "disc",
        )
        if (candidate / "GT2.VOL").is_file()
    ),
    REPO / "OpenGTPS1",
)
SIMULATION_ROOT = Path(
    os.environ.get("GT2_SIMULATION_LOOSE_ROOT", DEFAULT_SIMULATION_ROOT)
).resolve()
ARCADE_ROOT = Path(
    os.environ.get("GT2_ARCADE_LOOSE_ROOT", REPO / "work" / "arcade-unified")
).resolve()
ARCADE_VOLUME = Path(
    os.environ.get(
        "GT2_ARCADE_ORIGINAL_VOL",
        REPO / "work" / "arcade-disc" / "GT2.VOL",
    )
).resolve()
INSTALL = Path(
    os.environ.get("GT2_UNIFIED_LOOSE_ROOT", REPO / "work" / "gt2-unified")
).resolve()
UNIFIED_VOLUME_LBA = 473
UNIFIED_VOLUME_SIZE = 488241152
ARCADE_ORIGINAL_VOLUME_SIZE = 213596160
ARCADE_VOLUME_SOURCE_OFFSET = UNIFIED_VOLUME_SIZE
UNIFIED_MUSIC_LBA = 238872
UNIFIED_MUSIC_SIZE = 85262336
UNIFIED_ARCADE_STREAM_LBA = 280504
UNIFIED_ARCADE_STREAM_SIZE = 335011840

TITLE_PALETTE = (
    0x801E,
    0x884E,
    0x9084,
    0xFFFF,
    0xFBDE,
    0xF39C,
    0xE739,
    0xDAD6,
    0xCA52,
    0xAD6B,
    0x9CE7,
    0x8842,
    0x8421,
)


def configured_patch_volumes() -> list[Path]:
    configured = os.environ.get("GT2_PATCH_VOLUMES")
    if configured is not None:
        return [
            Path(item).resolve()
            for item in configured.split(os.pathsep)
            if item.strip()
        ]
    default = REPO / "work" / "gt1-converted" / "GTPATCH.VOL"
    return [default.resolve()] if default.is_file() else []


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def copy_if_needed(source: Path, destination: Path) -> None:
    if not source.is_file():
        raise FileNotFoundError(f"required unified-install source is missing: {source}")
    destination.parent.mkdir(parents=True, exist_ok=True)
    if (
        destination.is_file()
        and destination.stat().st_size == source.stat().st_size
        and sha256(destination) == sha256(source)
    ):
        return
    shutil.copy2(source, destination)
    print(f"copied {source.name}: {destination}")


def merge_native_volumes(
    simulation_volume: Path,
    arcade_volume: Path,
    destination: Path,
) -> None:
    """Store the Simulation and materialized Arcade GTFS members together."""
    arcade_size = arcade_volume.stat().st_size
    expected = UNIFIED_VOLUME_SIZE + arcade_size
    if simulation_volume.stat().st_size != UNIFIED_VOLUME_SIZE:
        raise ValueError(
            "unexpected Simulation GT2.VOL size: "
            f"{simulation_volume.stat().st_size} != {UNIFIED_VOLUME_SIZE}"
        )

    temporary = destination.with_suffix(destination.suffix + ".tmp")
    with temporary.open("wb") as output:
        for source in (simulation_volume, arcade_volume):
            with source.open("rb") as stream:
                shutil.copyfileobj(stream, output, 1024 * 1024)
    if temporary.stat().st_size != expected:
        raise IOError(
            f"merged GT2.VOL size mismatch: {temporary.stat().st_size} != {expected}"
        )
    os.replace(temporary, destination)
    print(
        "merged native GT2.VOL members: "
        f"Simulation@0+{UNIFIED_VOLUME_SIZE}, "
        f"Arcade@{ARCADE_VOLUME_SOURCE_OFFSET}+{arcade_size}"
    )


def materialize_arcade_volume() -> Path:
    if ARCADE_VOLUME.stat().st_size != ARCADE_ORIGINAL_VOLUME_SIZE:
        raise ValueError(
            "unexpected original Arcade GT2.VOL size: "
            f"{ARCADE_VOLUME.stat().st_size} != {ARCADE_ORIGINAL_VOLUME_SIZE}"
        )
    patches = configured_patch_volumes()
    if not patches:
        return ARCADE_VOLUME
    missing = [path for path in patches if not path.is_file()]
    if missing:
        raise FileNotFoundError(
            "configured GTPATCH volume is missing: "
            + ", ".join(str(path) for path in missing)
        )
    materialized = INSTALL / "arcade.materialized.vol"
    apply_patches(ARCADE_VOLUME, patches, materialized)
    return materialized


def materialized_arcade_overlay() -> Path:
    configured = os.environ.get("GT2_ARCADE_PATCHED_OVL")
    if configured is not None:
        candidate = Path(configured).resolve()
        if not candidate.is_file():
            raise FileNotFoundError(
                f"configured patched Arcade GT2.OVL is missing: {candidate}"
            )
        return candidate
    default = REPO / "work" / "gt1-converted" / "GT2.OVL"
    return default.resolve() if default.is_file() else ARCADE_ROOT / "GT2.OVL"


def write_arcade_manifest(destination: Path, arcade_volume_size: int) -> None:
    template = REPO / "tools" / "recompone.arcade.unified.json"
    data = json.loads(template.read_text(encoding="utf-8"))
    matches = [item for item in data["files"] if item["path"] == "GT2.VOL"]
    if len(matches) != 1:
        raise ValueError("Arcade unified manifest has no unique GT2.VOL entry")
    entry = matches[0]
    entry["sourceOffset"] = ARCADE_VOLUME_SOURCE_OFFSET
    entry["sourceLength"] = arcade_volume_size
    entry["size"] = arcade_volume_size
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_text(
        json.dumps(data, indent=2) + "\n",
        encoding="utf-8",
    )
    print(f"wrote Arcade manifest for materialized volume size {arcade_volume_size}")


def relocate_iso_file(
    metadata: Path,
    iso_name: bytes,
    lba: int,
    logical_size: int,
) -> None:
    """Rewrite one ISO9660 directory record in the generated metadata dump."""
    data = bytearray(metadata.read_bytes())
    offset = data.find(iso_name)
    if offset < 33:
        raise ValueError(
            f"{iso_name.decode('ascii')} directory record is missing: {metadata}"
        )
    record = offset - 33
    record_length = data[record]
    name_length = data[record + 32]
    if (
        record_length < 33 + name_length
        or data[offset : offset + name_length] != iso_name
    ):
        raise ValueError(
            f"malformed {iso_name.decode('ascii')} directory record: {metadata}"
        )
    struct.pack_into("<I", data, record + 2, lba)
    struct.pack_into(">I", data, record + 6, lba)
    struct.pack_into("<I", data, record + 10, logical_size)
    struct.pack_into(">I", data, record + 14, logical_size)
    updated = bytes(data)
    if metadata.read_bytes() != updated:
        metadata.write_bytes(updated)
        print(
            f"relocated {iso_name.decode('ascii')}: "
            f"LBA {lba}, size {logical_size} in {metadata}"
        )


def psx_rgb(color: int) -> tuple[int, int, int]:
    return (
        (color & 0x1F) * 255 // 31,
        ((color >> 5) & 0x1F) * 255 // 31,
        ((color >> 10) & 0x1F) * 255 // 31,
    )


def decode_16bpp_tim(tim: bytes) -> tuple[int, int, tuple[int, ...]]:
    if len(tim) < 20 or struct.unpack_from("<II", tim, 0) != (0x10, 2):
        raise ValueError("unexpected native GT2 menu-label TIM")
    block_size, _, _, width, height = struct.unpack_from("<IHHHH", tim, 8)
    if block_size != 12 + width * height * 2 or len(tim) < 8 + block_size:
        raise ValueError("malformed native GT2 menu-label TIM")
    pixels = struct.unpack_from(f"<{width * height}H", tim, 20)
    return width, height, pixels


def native_label_pixels(
    selected_tim: bytes,
    unselected_tim: bytes,
) -> list[list[int]]:
    width, height, selected = decode_16bpp_tim(selected_tim)
    other_width, other_height, unselected = decode_16bpp_tim(unselected_tim)
    if (
        (width, height) != (140, 28)
        or (other_width, other_height) != (width, height)
    ):
        raise ValueError("unexpected native GT2 top-menu label dimensions")

    palette = tuple(psx_rgb(color) for color in TITLE_PALETTE)
    rows = [[13] * width for _ in range(22)]
    for destination_y, source_y in enumerate(range(3, 25)):
        for x in range(width):
            source = psx_rgb(selected[source_y * width + x])
            other = psx_rgb(unselected[source_y * width + x])
            if sum(abs(a - b) for a, b in zip(source, other)) <= 30:
                continue
            rows[destination_y][x] = min(
                range(len(palette)),
                key=lambda index: sum(
                    (source[channel] - palette[index][channel]) ** 2
                    for channel in range(3)
                ),
            )
    return rows


def build_native_unified_labels(pack: bytes) -> tuple[list[list[int]], ...]:
    def member(offset: int) -> bytes:
        return gzip.decompress(pack[offset : offset + 0x1000])

    arcade = native_label_pixels(member(0x14800), member(0x15800))
    gran_turismo = native_label_pixels(member(0x16800), member(0x17800))

    # Preserve Sony's original red // Arcade Mode layout and glyph shapes.
    # The second entry combines its red // prefix and Mode word with the
    # shipped Gran Turismo wordmark from the adjacent native top-menu sprite;
    # every visible glyph remains original GT2 artwork.
    arcade_mode = [row[:132] for row in arcade]
    gran_turismo_mode = []
    for y in range(22):
        row = [13] * 177
        row[3:21] = arcade[y][3:21]
        row[23:127] = gran_turismo[y][23:127]
        row[132:177] = arcade[y][87:132]
        gran_turismo_mode.append(row)
    return arcade_mode, gran_turismo_mode


def patch_unified_title_texture(volume: Path) -> None:
    """Install Sony's native mode labels in GT2's title TIM atlas."""
    entries = {
        entry.name: entry
        for entry in read_entries(volume)
    }
    entry = entries.get("arcade/title_item.tim.gz")
    if entry is None:
        raise ValueError("GT2.VOL is missing arcade/title_item.tim.gz")
    topmenu_entry = entries.get("arcade/arc_topmenu_usa")
    if topmenu_entry is None:
        raise ValueError("GT2.VOL is missing arcade/arc_topmenu_usa")

    with volume.open("rb") as stream:
        stream.seek(topmenu_entry.offset)
        topmenu_pack = stream.read(topmenu_entry.size)
    labels = build_native_unified_labels(topmenu_pack)

    with volume.open("r+b") as stream:
        stream.seek(entry.offset)
        packed = stream.read(entry.size)
        tim = bytearray(gzip.decompress(packed))
        if (
            len(tim) != 0xFE14
            or struct.unpack_from("<II", tim, 0) != (0x10, 0)
            or struct.unpack_from("<HH", tim, 16) != (128, 254)
        ):
            raise ValueError("unexpected GT2 title_item.tim layout")

        row_bytes = 256

        def set_pixel(x: int, y: int, value: int) -> None:
            offset = 20 + y * row_bytes + x // 2
            if x & 1:
                tim[offset] = (tim[offset] & 0x0F) | (value << 4)
            else:
                tim[offset] = (tim[offset] & 0xF0) | value

        # Replace two PAL-only title-label rows.  These exact atlas rectangles
        # are unreachable from the NTSC-U language table, unlike apparently
        # blank regions which are reused by the GT2 logo and footer sprites.
        layouts = ((24, 132), (48, 177))
        for (y, width), label in zip(layouts, labels):
            for row in range(22):
                for x in range(width):
                    set_pixel(x, y + row, label[row][x])

        replacement = gzip.compress(bytes(tim), compresslevel=9, mtime=0)
        if len(replacement) > entry.size:
            raise ValueError(
                "unified title texture no longer fits its GT2.VOL allocation: "
                f"{len(replacement)} > {entry.size}"
            )
        stream.seek(entry.offset)
        stream.write(replacement)
        stream.write(b"\0" * (entry.size - len(replacement)))
    print(
        "installed original GT2 title-menu artwork in GT2.VOL: "
        "// Arcade Mode, // Gran Turismo Mode"
    )


def main() -> int:
    INSTALL.mkdir(parents=True, exist_ok=True)
    arcade_volume = materialize_arcade_volume()
    arcade_volume_size = arcade_volume.stat().st_size

    simulation_files = (
        "DISC_META.DAT",
        "SYSTEM.CNF",
        "SCUS_944.88",
        "GT2.OVL",
        "FAULTY.PSX",
    )
    arcade_files = (
        "DISC_META.DAT",
        "SYSTEM.CNF",
        "SCUS_944.55",
        "GT2.OVL",
        "STREAM.DAT",
    )
    for name in simulation_files:
        copy_if_needed(SIMULATION_ROOT / name, INSTALL / "simulation" / name)
    arcade_overlay = materialized_arcade_overlay()
    for name in arcade_files:
        source = arcade_overlay if name == "GT2.OVL" else ARCADE_ROOT / name
        copy_if_needed(source, INSTALL / "arcade" / name)
    for mode in ("simulation", "arcade"):
        relocate_iso_file(
            INSTALL / mode / "DISC_META.DAT",
            b"GT2.VOL;1",
            UNIFIED_VOLUME_LBA,
            UNIFIED_VOLUME_SIZE if mode == "simulation" else arcade_volume_size,
        )
    relocate_iso_file(
        INSTALL / "arcade" / "DISC_META.DAT",
        b"MUSIC.DAT;1",
        UNIFIED_MUSIC_LBA,
        UNIFIED_MUSIC_SIZE,
    )
    relocate_iso_file(
        INSTALL / "arcade" / "DISC_META.DAT",
        b"STREAM.DAT;1",
        UNIFIED_ARCADE_STREAM_LBA,
        UNIFIED_ARCADE_STREAM_SIZE,
    )

    simulation_music = SIMULATION_ROOT / "MUSIC.DAT"
    arcade_music = ARCADE_ROOT / "MUSIC.DAT"
    if (
        simulation_music.stat().st_size != arcade_music.stat().st_size
        or sha256(simulation_music) != sha256(arcade_music)
    ):
        raise ValueError("Simulation and Arcade MUSIC.DAT raw sector data differ")
    copy_if_needed(simulation_music, INSTALL / "MUSIC.DAT")
    unified_volume = INSTALL / "GT2.VOL"
    # Keep both Sony GTFS volumes byte-for-byte in one physical container.
    # The loose manifests expose the matching member at the original disc LBA,
    # so neither executable receives the other disc's incompatible payloads.
    merge_native_volumes(
        SIMULATION_ROOT / "GT2.VOL",
        arcade_volume,
        unified_volume,
    )
    patch_unified_title_texture(unified_volume)

    manifest_root = INSTALL / "manifests"
    copy_if_needed(
        REPO / "tools" / "recompone.simulation.unified.json",
        manifest_root / "simulation.json",
    )
    write_arcade_manifest(manifest_root / "arcade.json", arcade_volume_size)
    (INSTALL / "music").mkdir(exist_ok=True)
    print(f"Unified native Simulation/Arcade install ready: {INSTALL}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
