#!/usr/bin/env python3
"""Self-contained first-run installer for the public OpenGTPS1 package."""

from __future__ import annotations

import argparse
import contextlib
import ctypes
import hashlib
import json
import os
import queue
import shutil
import struct
import subprocess
import sys
import tempfile
import threading
from dataclasses import dataclass
from pathlib import Path
from typing import Callable, Iterable

import gt1_convert
from gt2_patch import apply_patches
from gt2_vol import read_entries


@dataclass(frozen=True)
class Disc:
    key: str
    label: str
    serial: str
    size: int
    sha256: str


DISCS = {
    "simulation": Disc(
        "simulation",
        "Gran Turismo 2 Simulation Disc (NTSC-U revision 2)",
        "SCUS-94488",
        691_850_208,
        "D0AB6E70539601057590A36299543C0ADAD219254D712F7D4273219094ED5031",
    ),
    "arcade": Disc(
        "arcade",
        "Gran Turismo 2 Arcade Disc (NTSC-U)",
        "SCUS-94455",
        729_423_408,
        "C2E97D6B0C847CA4336D9D84D8D98C349D1240ED075E81AB3FD5C977E9A45075",
    ),
    "gt1": Disc(
        "gt1",
        "Gran Turismo (NTSC-U)",
        "SCUS-94194",
        693_668_304,
        "765A748C4F2975A063A47BA9E42708A4882954D765F9E352C5AF3C0950EAEFB6",
    ),
}

SIMULATION_VOLUME_SIZE = 488_241_152
ARCADE_VOLUME_SIZE = 213_596_160


class InstallerLog:
    def __init__(self, path: Path, notify: Callable[[str], None] | None = None):
        path.parent.mkdir(parents=True, exist_ok=True)
        self._stream = path.open("w", encoding="utf-8", newline="\n")
        self._notify = notify
        self._lock = threading.Lock()

    def write(self, message: str) -> None:
        message = message.rstrip("\r\n")
        if not message:
            return
        with self._lock:
            self._stream.write(message + "\n")
            self._stream.flush()
        if self._notify is not None:
            self._notify(message)

    def close(self) -> None:
        with self._lock:
            self._stream.close()


class LineSink:
    def __init__(self, log: InstallerLog):
        self._log = log
        self._buffer = ""

    def write(self, text: str) -> int:
        self._buffer += text
        while "\n" in self._buffer:
            line, self._buffer = self._buffer.split("\n", 1)
            self._log.write(line)
        return len(text)

    def flush(self) -> None:
        if self._buffer:
            self._log.write(self._buffer)
            self._buffer = ""


def resource_root() -> Path:
    if getattr(sys, "frozen", False):
        return Path(sys._MEIPASS)
    return Path(__file__).resolve().parents[1] / "release"


def executable_root() -> Path:
    if getattr(sys, "frozen", False):
        return Path(sys.executable).resolve().parent
    return Path(__file__).resolve().parents[1] / "release"


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(4 * 1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def validate_disc(path: Path, disc: Disc, log: InstallerLog) -> Path:
    resolved = path.expanduser().resolve()
    if not resolved.is_file():
        raise FileNotFoundError(f"{disc.label} image was not found: {resolved}")
    actual_size = resolved.stat().st_size
    if actual_size != disc.size:
        raise ValueError(
            f"{disc.label} has the wrong size. Expected {disc.size:,} bytes; "
            f"found {actual_size:,}."
        )
    log.write(f"Validating {disc.label} ({disc.serial})...")
    actual_hash = sha256(resolved)
    if actual_hash != disc.sha256:
        raise ValueError(
            f"{disc.label} SHA-256 mismatch. This installer accepts only the "
            f"supported NTSC-U image. Expected {disc.sha256}; found {actual_hash}."
        )
    log.write(f"Validated {disc.serial}: {resolved}")
    return resolved


def _drive_roots() -> Iterable[Path]:
    if os.name != "nt":
        return []
    mask = ctypes.windll.kernel32.GetLogicalDrives()
    roots: list[Path] = []
    for index in range(26):
        if not mask & (1 << index):
            continue
        root = Path(f"{chr(65 + index)}:\\")
        drive_type = ctypes.windll.kernel32.GetDriveTypeW(str(root))
        if drive_type in (2, 5):  # removable or optical
            roots.append(root)
    return roots


def search_roots(install_root: Path) -> list[tuple[Path, int]]:
    home = Path.home()
    roots: list[tuple[Path, int]] = [
        (install_root, 3),
        (install_root.parent, 2),
        (home / "Downloads", 4),
        (home / "Desktop", 4),
        (home / "Documents", 4),
    ]
    roots.extend((root, 5) for root in _drive_roots())
    extra = os.environ.get("OPENGT_DISC_SEARCH_ROOTS", "")
    roots.extend((Path(item), 6) for item in extra.split(os.pathsep) if item)
    unique: list[tuple[Path, int]] = []
    seen: set[str] = set()
    for root, depth in roots:
        try:
            key = str(root.resolve()).casefold()
        except OSError:
            continue
        if key not in seen and root.exists():
            seen.add(key)
            unique.append((root, depth))
    return unique


def discover_discs(install_root: Path, log: InstallerLog) -> dict[str, Path]:
    expected_sizes = {disc.size: disc for disc in DISCS.values()}
    found: dict[str, Path] = {}
    skipped = {
        "$recycle.bin",
        "system volume information",
        ".git",
        "artifacts",
        "generated",
        "work",
        "node_modules",
    }
    log.write("Searching nearby folders and removable drives for supported images...")
    for root, max_depth in search_roots(install_root):
        root_parts = len(root.parts)
        for current, directories, files in os.walk(root, topdown=True):
            depth = len(Path(current).parts) - root_parts
            directories[:] = [
                name
                for name in directories
                if name.casefold() not in skipped and depth < max_depth
            ]
            for name in files:
                if Path(name).suffix.casefold() not in (".img", ".bin", ".iso"):
                    continue
                candidate = Path(current) / name
                try:
                    disc = expected_sizes.get(candidate.stat().st_size)
                except OSError:
                    continue
                if disc is None or disc.key in found:
                    continue
                if sha256(candidate) == disc.sha256:
                    found[disc.key] = candidate.resolve()
                    log.write(f"Found {disc.serial}: {candidate}")
            if len(found) == len(DISCS):
                return found
    log.write(
        "Automatic search complete: "
        + ", ".join(DISCS[key].serial for key in found)
        if found
        else "Automatic search did not find supported disc images."
    )
    return found


def run_gt2_setup(
    simulation: Path,
    arcade: Path,
    install_root: Path,
    log: InstallerLog,
) -> None:
    script = resource_root() / "Setup-From-GT2-Discs.ps1"
    if not script.is_file():
        raise FileNotFoundError(f"Embedded GT2 setup resource is missing: {script}")
    command = [
        "powershell.exe",
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        str(script),
        "-SimulationImagePath",
        str(simulation),
        "-ArcadeImagePath",
        str(arcade),
        "-InstallRoot",
        str(install_root),
    ]
    flags = 0x08000000 if os.name == "nt" else 0
    process = subprocess.Popen(
        command,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        text=True,
        encoding="utf-8",
        errors="replace",
        creationflags=flags,
    )
    assert process.stdout is not None
    for line in process.stdout:
        log.write(line)
    exit_code = process.wait()
    if exit_code != 0:
        raise RuntimeError(f"GT2 setup failed with exit code {exit_code}")


def copy_range(source: Path, destination: Path, offset: int, size: int) -> None:
    with source.open("rb") as input_stream, destination.open("wb") as output:
        input_stream.seek(offset)
        remaining = size
        while remaining:
            chunk = input_stream.read(min(4 * 1024 * 1024, remaining))
            if not chunk:
                raise EOFError(f"Unexpected end of {source}")
            output.write(chunk)
            remaining -= len(chunk)


def read_member(volume: Path, name: str) -> bytes:
    matches = [entry for entry in read_entries(volume) if entry.name == name]
    if len(matches) != 1:
        raise ValueError(f"{volume} does not contain one {name} member")
    entry = matches[0]
    with volume.open("rb") as stream:
        stream.seek(entry.offset)
        data = stream.read(entry.size)
    if len(data) != entry.size:
        raise EOFError(f"Truncated {name} member in {volume}")
    return data


def patch_iso_record(path: Path, iso_name: str, lba: int, size: int) -> None:
    data = bytearray(path.read_bytes())
    encoded = iso_name.encode("ascii")
    name_offset = data.find(encoded)
    if name_offset < 33:
        raise ValueError(f"{iso_name} ISO record is missing from {path}")
    record = name_offset - 33
    name_length = data[record + 32]
    if data[name_offset : name_offset + name_length] != encoded:
        raise ValueError(f"Malformed {iso_name} ISO record in {path}")
    struct.pack_into("<I", data, record + 2, lba)
    struct.pack_into(">I", data, record + 6, lba)
    struct.pack_into("<I", data, record + 10, size)
    struct.pack_into(">I", data, record + 14, size)
    path.write_bytes(data)


def update_manifest(
    source: Path,
    destination: Path,
    volume_offset: int,
    volume_size: int,
    overlay_size: int,
) -> None:
    data = json.loads(source.read_text(encoding="utf-8"))
    volumes = [item for item in data["files"] if item["path"] == "GT2.VOL"]
    overlays = [item for item in data["files"] if item["path"] == "GT2.OVL"]
    if len(volumes) != 1 or len(overlays) != 1:
        raise ValueError(f"Unified manifest is missing GT2.VOL/GT2.OVL: {source}")
    volumes[0]["sourceOffset"] = volume_offset
    volumes[0]["sourceLength"] = volume_size
    volumes[0]["size"] = volume_size
    overlays[0]["size"] = overlay_size
    destination.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")


def merge_gt1_content(gt1_image: Path, install_root: Path, log: InstallerLog) -> None:
    unified = install_root / "GT2.VOL"
    if unified.stat().st_size != SIMULATION_VOLUME_SIZE + ARCADE_VOLUME_SIZE:
        raise ValueError("The base two-disc GT2.VOL is missing or has the wrong size")
    with tempfile.TemporaryDirectory(
        prefix=".opengt-setup-", dir=install_root
    ) as temporary_name:
        temporary = Path(temporary_name)
        runtime_repo = temporary / "converter"
        converter_work = runtime_repo / "work"
        output = converter_work / "gt1-converted"
        disc_root = converter_work / "gt1-disc"
        runtime_repo.mkdir(parents=True)
        simulation_base = temporary / "simulation-base.vol"
        arcade_base = temporary / "arcade-base.vol"
        copy_range(unified, simulation_base, 0, SIMULATION_VOLUME_SIZE)
        copy_range(
            unified,
            arcade_base,
            SIMULATION_VOLUME_SIZE,
            ARCADE_VOLUME_SIZE,
        )

        log.write("Converting supported Gran Turismo 1 content locally...")
        previous_repo = gt1_convert.REPO
        previous_argv = sys.argv
        sink = LineSink(log)
        try:
            gt1_convert.REPO = runtime_repo
            sys.argv = [
                "gt1_convert",
                "--image",
                str(gt1_image),
                "--disc-root",
                str(disc_root),
                "--output",
                str(output),
                "--gt2-arcade-volume",
                str(arcade_base),
                "--gt2-simulation-volume",
                str(simulation_base),
                "--gt2-arcade-overlay",
                str(install_root / "arcade" / "GT2.OVL"),
                "--extract-clean",
            ]
            with contextlib.redirect_stdout(sink), contextlib.redirect_stderr(sink):
                result = gt1_convert.main()
            sink.flush()
            if result != 0:
                raise RuntimeError(f"GT1 converter exited with code {result}")
        finally:
            gt1_convert.REPO = previous_repo
            sys.argv = previous_argv

        simulation_materialized = temporary / "simulation-gt1.vol"
        arcade_materialized = temporary / "arcade-gt1.vol"
        with contextlib.redirect_stdout(sink), contextlib.redirect_stderr(sink):
            apply_patches(
                simulation_base,
                [
                    output / "GTPATCH.GT1CARS.SIMULATION.VOL",
                    output / "GTPATCH.LIVERY.SIMULATION.VOL",
                ],
                simulation_materialized,
            )
            apply_patches(
                arcade_base,
                [
                    output / "GTPATCH.ARCADE.VOL",
                    output / "GTPATCH.LIVERY.ARCADE.VOL",
                ],
                arcade_materialized,
            )
        sink.flush()

        livery_simulation = read_member(simulation_materialized, ".gtlivery")
        livery_arcade = read_member(arcade_materialized, ".gtlivery")
        if livery_simulation != livery_arcade:
            raise ValueError("Simulation and Arcade GT1 livery tables differ")
        magic, version, count = struct.unpack_from("<4sHH", livery_simulation)
        if (
            magic != b"GTLV"
            or version != 3
            or len(livery_simulation) != 8 + count * 12
        ):
            raise ValueError("The generated GT1 livery resolver is invalid")

        combined = temporary / "GT2.VOL"
        with combined.open("wb") as destination:
            for source in (simulation_materialized, arcade_materialized):
                with source.open("rb") as stream:
                    shutil.copyfileobj(stream, destination, 4 * 1024 * 1024)
        simulation_size = simulation_materialized.stat().st_size
        arcade_size = arcade_materialized.stat().st_size
        patched_overlay = output / "GT2.OVL"
        overlay_size = patched_overlay.stat().st_size

        simulation_metadata = temporary / "simulation-DISC_META.DAT"
        arcade_metadata = temporary / "arcade-DISC_META.DAT"
        shutil.copy2(install_root / "simulation" / "DISC_META.DAT", simulation_metadata)
        shutil.copy2(install_root / "arcade" / "DISC_META.DAT", arcade_metadata)
        patch_iso_record(simulation_metadata, "GT2.VOL;1", 473, simulation_size)
        patch_iso_record(arcade_metadata, "GT2.VOL;1", 473, arcade_size)
        patch_iso_record(arcade_metadata, "GT2.OVL;1", 331, overlay_size)

        simulation_manifest = temporary / "simulation.json"
        arcade_manifest = temporary / "arcade.json"
        update_manifest(
            install_root / "manifests" / "simulation.json",
            simulation_manifest,
            0,
            simulation_size,
            (install_root / "simulation" / "GT2.OVL").stat().st_size,
        )
        update_manifest(
            install_root / "manifests" / "arcade.json",
            arcade_manifest,
            simulation_size,
            arcade_size,
            overlay_size,
        )

        conversion = json.loads((output / "manifest.json").read_text(encoding="utf-8"))
        marker = temporary / "GT1_CONTENT.json"
        marker.write_text(
            json.dumps(
                {
                    "formatVersion": 1,
                    "source": conversion["source"],
                    "specialStageRoute11": True,
                    "arcadeCars": len(conversion["arcadeCars"]),
                    "simulationCars": len(conversion["simulationCars"]),
                    "liveryFolds": len(conversion["liveryFolds"]["simulation"]),
                    "liveryMappings": count,
                },
                indent=2,
            )
            + "\n",
            encoding="utf-8",
        )
        resolver = temporary / "GTLIVERY.BIN"
        resolver.write_bytes(livery_simulation)

        replacements = (
            (combined, install_root / "GT2.VOL"),
            (patched_overlay, install_root / "arcade" / "GT2.OVL"),
            (resolver, install_root / "GTLIVERY.BIN"),
            (marker, install_root / "GT1_CONTENT.json"),
            (simulation_metadata, install_root / "simulation" / "DISC_META.DAT"),
            (arcade_metadata, install_root / "arcade" / "DISC_META.DAT"),
            (simulation_manifest, install_root / "manifests" / "simulation.json"),
            (arcade_manifest, install_root / "manifests" / "arcade.json"),
        )
        for source, destination in replacements:
            os.replace(source, destination)
        log.write(
            "Gran Turismo 1 merge complete: Special Stage Route 11, "
            f"{len(conversion['arcadeCars'])} Arcade entries, "
            f"{len(conversion['simulationCars'])} Gran Turismo Mode cars, and "
            f"{count} alternate body/paint mappings installed."
        )


def install(
    simulation_path: Path,
    arcade_path: Path,
    gt1_path: Path | None,
    install_root: Path,
    log: InstallerLog,
) -> None:
    install_root.mkdir(parents=True, exist_ok=True)
    simulation = validate_disc(simulation_path, DISCS["simulation"], log)
    arcade = validate_disc(arcade_path, DISCS["arcade"], log)
    gt1 = validate_disc(gt1_path, DISCS["gt1"], log) if gt1_path else None
    log.write("Installing the unified Gran Turismo 2 data...")
    run_gt2_setup(simulation, arcade, install_root, log)
    if gt1 is not None:
        merge_gt1_content(gt1, install_root, log)
    else:
        log.write("Optional Gran Turismo 1 content was not selected.")
    log.write("Installation complete. You can now run GranTurismo2PC.exe.")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--headless", action="store_true")
    parser.add_argument("--return-to-game", action="store_true")
    parser.add_argument("--simulation", type=Path)
    parser.add_argument("--arcade", type=Path)
    parser.add_argument("--gt1", type=Path)
    parser.add_argument("--install-root", type=Path, default=executable_root())
    return parser.parse_args()


def run_headless(args: argparse.Namespace) -> int:
    if args.simulation is None or args.arcade is None:
        raise ValueError("--headless requires --simulation and --arcade")
    install_root = args.install_root.resolve()
    console = sys.__stdout__

    def notify(message: str) -> None:
        if console is not None:
            console.write(message + "\n")
            console.flush()

    log = InstallerLog(install_root / "OpenGTPS1-Setup.log", notify)
    try:
        install(args.simulation, args.arcade, args.gt1, install_root, log)
        return 0
    finally:
        log.close()


def run_gui(args: argparse.Namespace) -> int:
    import tkinter as tk
    from tkinter import filedialog, messagebox, ttk

    install_root = args.install_root.resolve()
    events: queue.Queue[tuple[str, object]] = queue.Queue()
    result = {"code": 1}
    root = tk.Tk()
    root.title("OpenGTPS1 0.9b Setup")
    root.geometry("820x560")
    root.minsize(720, 500)

    outer = ttk.Frame(root, padding=20)
    outer.pack(fill="both", expand=True)
    ttk.Label(
        outer,
        text="OpenGTPS1 0.9b First-Run Setup",
        font=("Segoe UI", 18, "bold"),
    ).pack(anchor="w")
    ttk.Label(
        outer,
        text=(
            "Choose your own supported US disc images. Setup validates them, "
            "builds the playable installation, and never copies or modifies "
            "the source images. Gran Turismo 1 content is optional."
        ),
        wraplength=760,
    ).pack(anchor="w", pady=(8, 18))

    values = {
        "simulation": tk.StringVar(value=str(args.simulation or "")),
        "arcade": tk.StringVar(value=str(args.arcade or "")),
        "gt1": tk.StringVar(value=str(args.gt1 or "")),
    }
    include_gt1 = tk.BooleanVar(value=args.gt1 is not None)
    entries: dict[str, ttk.Entry] = {}

    def browse(key: str) -> None:
        path = filedialog.askopenfilename(
            title=f"Select {DISCS[key].label}",
            filetypes=[("Raw disc images", "*.img *.bin *.iso"), ("All files", "*.*")],
        )
        if path:
            values[key].set(path)
            if key == "gt1":
                include_gt1.set(True)

    for key in ("simulation", "arcade", "gt1"):
        row = ttk.Frame(outer)
        row.pack(fill="x", pady=4)
        label = DISCS[key].label + (" — optional content merge" if key == "gt1" else "")
        ttk.Label(row, text=label, width=49).pack(side="left")
        entry = ttk.Entry(row, textvariable=values[key])
        entry.pack(side="left", fill="x", expand=True, padx=8)
        ttk.Button(row, text="Browse…", command=lambda item=key: browse(item)).pack(side="left")
        entries[key] = entry

    ttk.Checkbutton(
        outer,
        text="Merge supported Gran Turismo 1 cars, paints/liveries, and Special Stage Route 11",
        variable=include_gt1,
    ).pack(anchor="w", pady=(8, 12))

    status = tk.StringVar(value="Ready. Setup can search common folders and removable drives.")
    ttk.Label(outer, textvariable=status, wraplength=760).pack(anchor="w", pady=(4, 8))
    progress = ttk.Progressbar(outer, mode="indeterminate")
    progress.pack(fill="x", pady=(0, 10))
    details = tk.Text(outer, height=10, state="disabled", wrap="word")
    details.pack(fill="both", expand=True)

    controls = ttk.Frame(outer)
    controls.pack(fill="x", pady=(12, 0))

    def post_log(message: str) -> None:
        events.put(("log", message))

    def auto_search() -> None:
        search_button.configure(state="disabled")
        install_button.configure(state="disabled")
        progress.start(12)
        status.set("Searching for supported disc images…")

        def worker() -> None:
            search_log = InstallerLog(install_root / "OpenGTPS1-Setup.log", post_log)
            try:
                events.put(("found", discover_discs(install_root, search_log)))
            except Exception as error:  # noqa: BLE001 - surfaced in the UI
                events.put(("error", str(error)))
            finally:
                search_log.close()
                events.put(("search_done", None))

        threading.Thread(target=worker, daemon=True).start()

    def begin_install() -> None:
        if not values["simulation"].get().strip() or not values["arcade"].get().strip():
            messagebox.showerror(
                "Disc images required",
                "Select the supported Simulation and Arcade disc images first.",
            )
            return
        if include_gt1.get() and not values["gt1"].get().strip():
            messagebox.showerror(
                "Gran Turismo image required",
                "Select the supported Gran Turismo image, or clear the optional GT1 checkbox.",
            )
            return
        install_button.configure(state="disabled")
        search_button.configure(state="disabled")
        progress.start(12)
        status.set("Installing. This can take several minutes…")
        selected = {
            key: Path(value.get().strip()) if value.get().strip() else None
            for key, value in values.items()
        }
        merge_gt1 = include_gt1.get()

        def worker() -> None:
            setup_log = InstallerLog(install_root / "OpenGTPS1-Setup.log", post_log)
            try:
                install(
                    selected["simulation"],
                    selected["arcade"],
                    selected["gt1"] if merge_gt1 else None,
                    install_root,
                    setup_log,
                )
                events.put(("complete", None))
            except Exception as error:  # noqa: BLE001 - surfaced in the UI
                setup_log.write(f"ERROR: {error}")
                events.put(("install_error", str(error)))
            finally:
                setup_log.close()

        threading.Thread(target=worker, daemon=True).start()

    search_button = ttk.Button(controls, text="Search for discs", command=auto_search)
    search_button.pack(side="left")
    install_button = ttk.Button(controls, text="Install and play", command=begin_install)
    install_button.pack(side="right")
    ttk.Button(controls, text="Cancel", command=root.destroy).pack(side="right", padx=8)

    def poll_events() -> None:
        try:
            while True:
                kind, payload = events.get_nowait()
                if kind == "log":
                    details.configure(state="normal")
                    details.insert("end", str(payload) + "\n")
                    details.see("end")
                    details.configure(state="disabled")
                    message = str(payload)
                    if not message.startswith(("conflict:", "layer ", "materialized ")):
                        status.set(message)
                elif kind == "found":
                    found = payload
                    assert isinstance(found, dict)
                    for key, path in found.items():
                        values[key].set(str(path))
                elif kind == "search_done":
                    progress.stop()
                    search_button.configure(state="normal")
                    install_button.configure(state="normal")
                elif kind == "error":
                    messagebox.showwarning("Disc search", str(payload))
                elif kind == "complete":
                    progress.stop()
                    result["code"] = 0
                    messagebox.showinfo(
                        "OpenGTPS1 setup complete",
                        "Installation complete. OpenGTPS1 is ready to play.",
                    )
                    if not args.return_to_game:
                        game = install_root / "GranTurismo2PC.exe"
                        if game.is_file():
                            try:
                                subprocess.Popen([str(game)], cwd=str(install_root))
                            except OSError as error:
                                messagebox.showwarning(
                                    "OpenGTPS1 could not start",
                                    "Setup completed, but the game could not be "
                                    f"started automatically:\n\n{error}",
                                )
                    root.destroy()
                    return
                elif kind == "install_error":
                    progress.stop()
                    install_button.configure(state="normal")
                    search_button.configure(state="normal")
                    messagebox.showerror(
                        "OpenGTPS1 setup failed",
                        f"{payload}\n\nSee OpenGTPS1-Setup.log for details.",
                    )
        except queue.Empty:
            pass
        root.after(100, poll_events)

    root.after(100, poll_events)
    root.after(250, auto_search)
    root.mainloop()
    return int(result["code"])


def main() -> int:
    args = parse_args()
    return run_headless(args) if args.headless else run_gui(args)


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as error:  # noqa: BLE001 - command-line diagnostic
        if sys.stderr is not None:
            print(f"OpenGTPS1 setup failed: {error}", file=sys.stderr)
        raise
