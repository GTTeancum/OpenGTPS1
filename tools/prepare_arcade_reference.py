#!/usr/bin/env python3
"""Generate a native RecompOne project for GT2's Arcade Mode disc.

This is an internal bring-up target, not the final user-facing mode switch.
It recompiles the original Arcade executable and overlays so their presentation
and behaviour remain original, while allowing them to run against the unified
loose-file installation.
"""

from __future__ import annotations

import json
import os
import shutil
import struct
from pathlib import Path

from prepare_reference import relative_posix


REPO = Path(__file__).resolve().parents[1]
OUTPUT = REPO / "generated"
DEFAULT_CUE = next(
    (
        root / "SCUS_944.55.cue"
        for root in (REPO, *REPO.parents)
        if (root / "SCUS_944.55.cue").is_file()
    ),
    REPO / "SCUS_944.55.cue",
)
DEFAULT_DISC_ROOT = REPO / "work" / "arcade-disc"

# The six executables occupy the same PSX load range. Overlay 1's Arcade entry
# is 0xB4 bytes earlier than the Simulation build; using the Simulation address
# starts in the middle of the function and breaks the native frontend boot.
# These are explicit native entry points, not replacements for Arcade code or
# visuals.
OVERLAY_ENTRIES = (
    "0x80012254",
    "0x800112D0",
    "0x80011750",
    "0x80012C00",
    "0x80013628",
    "0x800114B8",
)

ARCADE_PATCHES = [
    ("80010954", "RecompOne.Runtime.Sdk.GT2Compat.WaitForInitialVBlanks", "replace"),
    ("8007D14C", "RecompOne.Runtime.Sdk.GT2Compat.VSync", "replace"),
    ("8007C460", "RecompOne.Runtime.Sdk.GT2Compat.WaitForCdCommand", "replace"),
    ("80081F64", "RecompOne.Runtime.Sdk.GT2Compat.WaitForCdBuffer", "replace"),
    ("80089F98", "RecompOne.Runtime.Sdk.LibCd.CdSyncCallback", "replace"),
    ("80089FB8", "RecompOne.Runtime.Sdk.LibCd.CdReadyCallback", "replace"),
    ("80089FD8", "RecompOne.Runtime.Sdk.LibCd.CdControl", "replace"),
    ("8008A114", "RecompOne.Runtime.Sdk.LibCd.CdControlF", "replace"),
    ("8008A248", "RecompOne.Runtime.Sdk.LibCd.CdControlB", "replace"),
    ("80089E28", "RecompOne.Runtime.Sdk.LibCd.CdGetSector", "replace"),
    ("8008A9FC", "RecompOne.Runtime.Sdk.LibCd.CdSync", "replace"),
    ("8008AC7C", "RecompOne.Runtime.Sdk.LibCd.CdReady", "replace"),
    ("8005DA48", "RecompOne.Runtime.Sdk.GT2Compat.TraceOverlayLoad", "pre"),
    ("8007ACA0", "RecompOne.Runtime.Sdk.GT2Compat.Longjmp", "replace"),
    ("80087058", "RecompOne.Runtime.Sdk.LibPad.PadInitDirect", "pre"),
]

ARCADE_EXTRA_MAIN_FUNCTIONS = [
    {"address": "0x80010000", "size": 0xC0},
    {"address": "0x80010430"},
    {"address": "0x80082F18"},
    # Indirect class-selection callback. It is stored in Arcade frontend data,
    # so the linear sweep has no branch target from which to discover it.
    {"address": "0x80083A1C"},
    {"address": "0x80083A4C"},
    {"address": "0x80083A8C"},
    {"address": "0x80083AE4"},
    {"address": "0x80083BBC"},
    # Coroutine continuations reached only through the game's setjmp/longjmp
    # contexts. They are valid native instruction boundaries, but have no
    # ordinary branch edges for the recompiler to discover.
    {"address": "0x80083A38"},
    {"address": "0x80083A64"},
    {"address": "0x80083AA4"},
    {"address": "0x80083B70"},
    {"address": "0x80083BDC"},
    {"address": "0x8008BD84"},
    # Native frontend object-update loop and the return sites for its indirect
    # callbacks. Arcade's allocator can yield through any of these callbacks,
    # so each site must be independently dispatchable when the guest coroutine
    # resumes.
    {"address": "0x80080934"},
    {"address": "0x8008099C"},
    {"address": "0x800809B4"},
    {"address": "0x800809D4"},
    {"address": "0x800809F0"},
    {"address": "0x80080A08"},
    # Nested frontend state-machine callback/continuations reached while the
    # class-selection object completes its asynchronous allocation.
    {"address": "0x80083318"},
    {"address": "0x80083328"},
    {"address": "0x80083348"},
    {"address": "0x80083350"},
    {"address": "0x80083368"},
    {"address": "0x8008339C"},
    # Companion bitstream setup routine and each native indirect-callback
    # return site that can be exposed by its allocation coroutine.
    {"address": "0x80084274"},
    {"address": "0x800842E4"},
    {"address": "0x800842E8"},
    {"address": "0x800842EC"},
    {"address": "0x80084334"},
    {"address": "0x800843DC"},
    {"address": "0x800844C8"},
    {"address": "0x80084470"},
    {"address": "0x80084510"},
    {"address": "0x800845C8"},
    {"address": "0x80084624"},
    {"address": "0x80084654"},
    # Bitstream/decompression coroutine used by the native frontend asset
    # loader, plus every indirect-callback return site through which one of the
    # guest allocation coroutines can yield.
    {"address": "0x800846E0"},
    {"address": "0x80084774"},
    {"address": "0x800847E0"},
    {"address": "0x8008484C"},
    {"address": "0x800848B8"},
    {"address": "0x80084924"},
    {"address": "0x80084960"},
    {"address": "0x8008499C"},
    {"address": "0x800849EC"},
    {"address": "0x80084A28"},
    {"address": "0x80084A6C"},
    {"address": "0x80084AA8"},
    {"address": "0x80084B1C"},
    {"address": "0x80084B80"},
    {"address": "0x80084BEC"},
    {"address": "0x80084C64"},
    {"address": "0x80084CDC"},
    {"address": "0x80084D20"},
    {"address": "0x80084D84"},
    {"address": "0x80084E64"},
    {"address": "0x80084EC4"},
    {"address": "0x80084F28"},
    {"address": "0x80084FD8"},
    {"address": "0x800850A0"},
    {"address": "0x80085154"},
    {"address": "0x80085208"},
    {"address": "0x800852A4"},
    {"address": "0x8008535C"},
    {"address": "0x800853C8"},
    {"address": "0x8008543C"},
    {"address": "0x80085484"},
    {"address": "0x800854B0"},
    {"address": "0x8008550C"},
    {"address": "0x8008565C"},
    {"address": "0x800856A4"},
    {"address": "0x800856D4"},
    {"address": "0x80085710"},
    {"address": "0x80081008"},
    {"address": "0x80081020"},
    {"address": "0x80060C84"},
    {"address": "0x80060CF0"},
    {"address": "0x8007C23C"},
    {"address": "0x8007E978"},
    {"address": "0x8007E998"},
    {"address": "0x80010AF0", "size": 0x178},
    {"address": "0x8006821C"},
    {"address": "0x80068220"},
    {"address": "0x8005D77C"},
    {"address": "0x8005D780"},
]

ARCADE_OVERLAY_EXTRA_FUNCTIONS = {
    0: [
        "0x80015F9C",
        "0x80015FCC",
        "0x80016014",
        "0x8001605C",
        "0x8001608C",
        "0x800160BC",
        "0x800160EC",
        "0x80016114",
        "0x80016144",
        "0x80016174",
        "0x800161E4",
    ],
    2: [
        # Class-selection object callback stored only in overlay data; it
        # becomes reachable after the native assets finish loading.
        "0x80019C04",
        "0x80019C5C",
        "0x80019CC4",
        "0x80019CF4",
        "0x80019D70",
        "0x80019D90",
        # Course-selection coroutine return sites. The native state-machine
        # callback can yield while committing a course and again while
        # preparing the race-start payload.
        "0x80011780",
        "0x80011894",
        # Class-selection screen update callback. Its indirect allocator call
        # yields through the game's longjmp coroutine, so retain both the
        # original callback entry and the instruction after that call.
        "0x8001419C",
        "0x80014230",
        # Original allocator entry retained alongside its split continuation.
        "0x80015A98",
        # Return continuation after the main allocator coroutine yields.
        "0x80015B50",
        # Completion continuation after the allocator status coroutine yields.
        "0x80015BA0",
        # Remaining native return sites in the allocator state machine. Asset
        # loading can yield through any of these cases while constructing the
        # next Arcade selection screen.
        "0x80015BC4",
        "0x80015BF0",
        "0x80015C0C",
        "0x80015C68",
        "0x80015CC0",
        "0x80015D18",
        "0x80015D3C",
        "0x80015D78",
        "0x80015D84",
    ],
}


def environment_path(name: str, fallback: Path) -> Path:
    value = os.environ.get(name)
    return Path(value).expanduser().resolve() if value else fallback


def read_overlay_table(path: Path) -> list[tuple[int, int]]:
    raw = path.read_bytes()
    if len(raw) < 48:
        raise ValueError(f"Arcade GT2.OVL header is truncated: {path}")

    entries: list[tuple[int, int]] = []
    for index in range(6):
        offset, packed_size = struct.unpack_from("<II", raw, index * 8)
        if offset < 48 or packed_size == 0 or offset + packed_size > len(raw):
            raise ValueError(
                f"Arcade GT2.OVL entry {index} is invalid: "
                f"offset=0x{offset:X}, size={packed_size}, file={len(raw)}"
            )
        entries.append((offset, packed_size))
    return entries


def overlay_configs(overlay_path: Path) -> list[dict[str, object]]:
    return [
        {
            "name": f"gt2_arcade_overlay_{index}",
            "base": "0x80010000",
            "file": "GT2.OVL",
            "offset": offset,
            "size": packed_size,
            "gzip": True,
            "linearSweep": True,
            "functions": [
                {"address": "0x80010000"},
                {"address": OVERLAY_ENTRIES[index]},
                *(
                    {"address": address}
                    for address in ARCADE_OVERLAY_EXTRA_FUNCTIONS.get(index, [])
                ),
            ],
        }
        for index, (offset, packed_size) in enumerate(read_overlay_table(overlay_path))
    ]


def main() -> int:
    cue = environment_path("GT2_ARCADE_CUE", DEFAULT_CUE)
    disc_root = environment_path("GT2_ARCADE_DISC_ROOT", DEFAULT_DISC_ROOT)
    overlay_path = disc_root / "GT2.OVL"
    required = [
        cue,
        disc_root / "SYSTEM.CNF",
        disc_root / "SCUS_944.55",
        overlay_path,
        disc_root / "GT2.VOL",
    ]
    missing = [path for path in required if not path.is_file()]
    if missing:
        raise FileNotFoundError(
            "missing Arcade bootstrap input(s): "
            + ", ".join(str(path) for path in missing)
        )

    OUTPUT.mkdir(parents=True, exist_ok=True)
    config = {
        "game": {
            "id": "SCUS-94455",
            "name": "GranTurismo2ArcadePC",
            "namespace": "Recompiled.Arcade",
            "title": "Gran Turismo 2 PC - Arcade bring-up",
            "output": "arcade-recompiled",
        },
        "cue": relative_posix(cue, OUTPUT),
        "debug": os.environ.get("GT2_RECOMP_DEBUG") == "1",
        "linearSweep": True,
        "functions": ARCADE_EXTRA_MAIN_FUNCTIONS,
        "overlays": overlay_configs(overlay_path),
        "stubs": [],
        "ignored": [],
        # Each address was mapped from the corresponding Simulation routine by
        # a unique normalized MIPS fingerprint. No Simulation address is
        # assumed to be valid for Arcade merely because the binaries are close.
        "patches": [
            {"overlay": "main", "address": address, "target": target, "mode": mode}
            for address, target, mode in ARCADE_PATCHES
        ],
    }

    config_path = OUTPUT / "gt2_arcade.recompone.json"
    config_path.write_text(json.dumps(config, indent=2) + "\n", encoding="utf-8")

    host_source = REPO / "tools" / "reference-host"
    generated_project = OUTPUT / "arcade-recompiled"
    generated_project.mkdir(parents=True, exist_ok=True)
    program = (host_source / "Program.cs").read_text(encoding="utf-8")
    program = program.replace(
        "using Recompiled.Simulation;",
        "using Recompiled.Arcade;",
    )
    (generated_project / "Program.cs").write_text(program, encoding="utf-8")
    project = (host_source / "GranTurismo2PC.csproj").read_text(encoding="utf-8")
    project = project.replace(
        "<AssemblyName>GranTurismo2PC</AssemblyName>",
        "<AssemblyName>GranTurismo2ArcadePC</AssemblyName>",
    )
    (generated_project / "GranTurismo2ArcadePC.csproj").write_text(
        project, encoding="utf-8"
    )

    print(f"Wrote {config_path}")
    print(f"Refreshed Arcade host project in {generated_project}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
