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
UNIFIED_MUSIC_LBA = 238872
UNIFIED_MUSIC_SIZE = 85262336
UNIFIED_ARCADE_STREAM_LBA = 280504
UNIFIED_ARCADE_STREAM_SIZE = 335011840

def configured_patch_volumes(mode: str) -> list[Path]:
    if mode not in ("simulation", "arcade"):
        raise ValueError(f"unsupported GT2 patch target: {mode}")
    configured = os.environ.get(f"GT2_{mode.upper()}_PATCH_VOLUMES")
    if configured is not None:
        return [
            Path(item).resolve()
            for item in configured.split(os.pathsep)
            if item.strip()
        ]
    # GT2_PATCH_VOLUMES was the original Arcade-only setting. Preserve it as
    # an Arcade alias, but never apply it to Simulation: the existing layer
    # replaces Arcade-specific databases which are incompatible with GT Mode.
    if mode == "arcade":
        legacy = os.environ.get("GT2_PATCH_VOLUMES")
        if legacy is not None:
            return [
                Path(item).resolve()
                for item in legacy.split(os.pathsep)
                if item.strip()
            ]
    patch_root = REPO / "work" / "gt1-converted"
    targeted = patch_root / f"GTPATCH.{mode.upper()}.VOL"
    if targeted.is_file():
        return [targeted.resolve()]
    legacy_default = patch_root / "GTPATCH.VOL"
    if mode == "arcade" and legacy_default.is_file():
        return [legacy_default.resolve()]
    return []


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


def read_gtfs_member(volume: Path, name: str) -> bytes | None:
    matches = [entry for entry in read_entries(volume) if entry.name == name]
    if not matches:
        return None
    if len(matches) != 1:
        raise ValueError(f"{volume} has duplicate GTFS member {name}")
    entry = matches[0]
    with volume.open("rb") as stream:
        stream.seek(entry.offset)
        return stream.read(entry.size)


def install_livery_resolver_table(
    simulation_volume: Path,
    arcade_volume: Path,
) -> None:
    """Install the alternate-body table only when both modes carry its assets."""

    tables = (
        read_gtfs_member(simulation_volume, ".gtlivery"),
        read_gtfs_member(arcade_volume, ".gtlivery"),
    )
    destination = INSTALL / "GTLIVERY.BIN"
    if tables == (None, None):
        if destination.is_file():
            destination.unlink()
            print(f"removed inactive livery resolver table: {destination}")
        return
    if tables[0] is None or tables[1] is None:
        raise ValueError(
            "GT1 livery integration must be enabled atomically for both "
            "Simulation and Arcade modes"
        )
    if tables[0] != tables[1]:
        raise ValueError(
            "Simulation and Arcade GT1 livery resolver tables differ"
        )
    table = tables[0]
    if table is None or len(table) < 8:
        raise ValueError("GT1 livery resolver table is truncated")
    magic, version, count = struct.unpack_from("<4sHH", table)
    expected_size = 8 + count * 12
    if magic != b"GTLV" or version != 3 or len(table) != expected_size:
        raise ValueError(
            "GT1 livery resolver table has an unsupported format: "
            f"magic={magic!r}, version={version}, "
            f"size={len(table)}, expected={expected_size}"
        )
    destination.write_bytes(table)
    print(
        "installed native GT1 livery resolver table: "
        f"{count} body/palette mappings"
    )


def merge_native_volumes(
    simulation_volume: Path,
    arcade_volume: Path,
    destination: Path,
) -> int:
    """Store the Simulation and materialized Arcade GTFS members together."""
    simulation_size = simulation_volume.stat().st_size
    arcade_size = arcade_volume.stat().st_size
    expected = simulation_size + arcade_size

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
        f"Simulation@0+{simulation_size}, "
        f"Arcade@{simulation_size}+{arcade_size}"
    )
    return simulation_size


def materialize_volume(
    mode: str,
    base: Path,
    expected_original_size: int,
) -> Path:
    if base.stat().st_size != expected_original_size:
        raise ValueError(
            f"unexpected original {mode.title()} GT2.VOL size: "
            f"{base.stat().st_size} != {expected_original_size}"
        )
    patches = configured_patch_volumes(mode)
    if not patches:
        return base
    missing = [path for path in patches if not path.is_file()]
    if missing:
        raise FileNotFoundError(
            f"configured {mode.title()} GTPATCH volume is missing: "
            + ", ".join(str(path) for path in missing)
        )
    materialized = INSTALL / f"{mode}.materialized.vol"
    apply_patches(base, patches, materialized)
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


def write_volume_manifest(
    mode: str,
    destination: Path,
    source_offset: int,
    volume_size: int,
) -> None:
    template = REPO / "tools" / f"recompone.{mode}.unified.json"
    data = json.loads(template.read_text(encoding="utf-8"))
    matches = [item for item in data["files"] if item["path"] == "GT2.VOL"]
    if len(matches) != 1:
        raise ValueError(
            f"{mode.title()} unified manifest has no unique GT2.VOL entry"
        )
    entry = matches[0]
    entry["sourceOffset"] = source_offset
    entry["sourceLength"] = volume_size
    entry["size"] = volume_size
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_text(
        json.dumps(data, indent=2) + "\n",
        encoding="utf-8",
    )
    print(
        f"wrote {mode.title()} manifest: "
        f"GT2.VOL@{source_offset}+{volume_size}"
    )


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


def decode_16bpp_tim(tim: bytes) -> tuple[int, int, tuple[int, ...]]:
    if len(tim) < 20 or struct.unpack_from("<II", tim, 0) != (0x10, 2):
        raise ValueError("unexpected native GT2 menu-label TIM")
    block_size, _, _, width, height = struct.unpack_from("<IHHHH", tim, 8)
    if block_size != 12 + width * height * 2 or len(tim) < 8 + block_size:
        raise ValueError("malformed native GT2 menu-label TIM")
    pixels = struct.unpack_from(f"<{width * height}H", tim, 20)
    return width, height, pixels


def decode_native_demo_title(pack: bytes) -> list[int]:
    """Decode Sony's exact 512x480 16-bit demo-menu background."""
    background_ranges = (
        (0x00000, 0x01000),
        (0x01000, 0x06800),
        (0x06800, 0x0E800),
        (0x0E800, 0x14800),
    )
    canvas: list[int] = []
    for start, end in background_ranges:
        width, height, pixels = decode_16bpp_tim(
            gzip.decompress(pack[start:end])
        )
        if (width, height) != (512, 120):
            raise ValueError("unexpected native GT2 demo background strip")
        canvas.extend(pixels)

    # The stored background contains all four bright labels. Replace each with
    # its authored dim state; the guest menu overlays only the current bright
    # label at runtime.
    for offset, (destination_x, destination_y) in zip(
        (0x15800, 0x17800, 0x19800, 0x1B800),
        ((124, 284), (124, 312), (264, 284), (264, 312)),
    ):
        width, height, pixels = decode_16bpp_tim(
            gzip.decompress(pack[offset : offset + 0x1000])
        )
        if (width, height) != (140, 28):
            raise ValueError("unexpected native GT2 demo label rectangle")
        for y in range(height):
            start = (destination_y + y) * 512 + destination_x
            canvas[start : start + width] = pixels[y * width : (y + 1) * width]

    return canvas


def install_unified_title_panels(volume: Path) -> Path:
    """Export all four exact 512x480 title selection states for the host."""
    entries = {
        entry.name: entry
        for entry in read_entries(volume)
    }
    topmenu_entry = entries.get("arcade/arc_topmenu_usa")
    if topmenu_entry is None:
        raise ValueError("GT2.VOL is missing arcade/arc_topmenu_usa")

    with volume.open("rb") as stream:
        stream.seek(topmenu_entry.offset)
        topmenu_pack = stream.read(topmenu_entry.size)
    base = decode_native_demo_title(topmenu_pack)
    destinations = ((124, 284), (124, 312), (264, 284), (264, 312))
    selected_offsets = (0x14800, 0x16800, 0x18800, 0x1A800)
    unselected_offsets = (0x15800, 0x17800, 0x19800, 0x1B800)
    panels: list[list[int]] = []
    for selected_index in range(4):
        panel = base.copy()
        for item_index, (destination_x, destination_y) in enumerate(destinations):
            source_offset = (
                selected_offsets[item_index]
                if item_index == selected_index
                else unselected_offsets[item_index]
            )
            width, height, pixels = decode_16bpp_tim(
                gzip.decompress(
                    topmenu_pack[source_offset : source_offset + 0x1000]
                )
            )
            if (width, height) != (140, 28):
                raise ValueError("unexpected native GT2 demo label rectangle")
            for y in range(height):
                start = (destination_y + y) * 512 + destination_x
                panel[start : start + width] = pixels[y * width : (y + 1) * width]
        panels.append(panel)

    output = INSTALL / "TITLE_EXACT.DAT"
    payload = bytearray(struct.pack("<8sIII", b"GT2TITLE", 512, 480, 4))
    for panel in panels:
        payload.extend(struct.pack(f"<{len(panel)}H", *panel))
    output.write_bytes(payload)
    print(
        "installed pixel-exact Sony GT2 demo title: four complete 512x480 "
        "15-bit selection states; unscaled 140x28 authored labels"
    )
    return volume


def main() -> int:
    INSTALL.mkdir(parents=True, exist_ok=True)
    simulation_volume = materialize_volume(
        "simulation",
        SIMULATION_ROOT / "GT2.VOL",
        UNIFIED_VOLUME_SIZE,
    )
    simulation_volume = install_unified_title_panels(simulation_volume)
    arcade_volume = materialize_volume(
        "arcade",
        ARCADE_VOLUME,
        ARCADE_ORIGINAL_VOLUME_SIZE,
    )
    simulation_volume_size = simulation_volume.stat().st_size
    arcade_volume_size = arcade_volume.stat().st_size
    install_livery_resolver_table(simulation_volume, arcade_volume)

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
            (
                simulation_volume_size
                if mode == "simulation"
                else arcade_volume_size
            ),
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
    arcade_source_offset = merge_native_volumes(
        simulation_volume,
        arcade_volume,
        unified_volume,
    )
    manifest_root = INSTALL / "manifests"
    write_volume_manifest(
        "simulation",
        manifest_root / "simulation.json",
        0,
        simulation_volume_size,
    )
    write_volume_manifest(
        "arcade",
        manifest_root / "arcade.json",
        arcade_source_offset,
        arcade_volume_size,
    )
    (INSTALL / "music").mkdir(exist_ok=True)
    print(f"Unified native Simulation/Arcade install ready: {INSTALL}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
