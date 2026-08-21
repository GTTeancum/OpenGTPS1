#!/usr/bin/env python3
"""Generate the initial GT2 RecompOne project and configuration."""

from __future__ import annotations

import json
import os
import shutil
from pathlib import Path


REPO = Path(__file__).resolve().parents[1]
DISC_ROOT = Path(
    os.environ.get("GT2_SIMULATION_DISC_ROOT", REPO / "work" / "disc")
).resolve()
CUE = Path(
    os.environ.get(
        "GT2_SIMULATION_CUE",
        REPO / "Gran Turismo 2 [Simulation Disc] [U] [SCUS-94488].cue",
    )
).resolve()
OUTPUT = REPO / "generated"

EXTRA_MAIN_FUNCTIONS = [
    # GT2 archive/overlay index initializer. Heuristic discovery split this
    # routine at several post-JAL continuations (0x80010010..0x80010040), so
    # none of the archive source/size globals or index records were populated.
    {"address": "0x80010000", "size": 0xC0},
    # Indirect parser/decompressor callback invoked while the main executable
    # consumes its first raw-disc bootstrap block.
    {"address": "0x80010430"},
    # Recursive continuation of the string hash routine. Discovery split the
    # public entry at 0x80083004 and its loop body at 0x8008300C, but the loop
    # tail deliberately calls through 0x80083008 to reload the next byte.
    {"address": "0x80083008"},
    # GPU command-list uploader has a public setup entry and an alternate
    # entry that accepts an already prepared pointer/count pair.
    {"address": "0x800810F8"},
    {"address": "0x80081110"},
    # Adjacent binary-search helpers share a tail-free code region, so the
    # scanner merges the second entry into the first function.
    {"address": "0x80060D74"},
    {"address": "0x80060DE0"},
    # Internal continuation reached by the GPU-environment dispatcher during
    # the first boot display setup. Call discovery stops at the earlier return.
    {"address": "0x8007C32C"},
    # Memory-card event callback registered by the original BIOS event path and
    # delivered during the first VBlank after card initialization.
    {"address": "0x8007EA68"},
    {"address": "0x8007EA88"},
    # Call discovery splits this routine at its post-JAL continuation (0x80010C20).
    # Preserve the complete function so its saved registers and stack are restored.
    {"address": "0x80010AF0", "size": 0x178},
    # Alternate entry into the archive reader. Callers select 0x8006830C when
    # the requested index must be negated and 0x80068310 when it must not; the
    # latter deliberately skips only the first instruction.
    {"address": "0x8006830C"},
    {"address": "0x80068310"},
    # GT Mode overlay calls the overlay loader's dispatch entry through a
    # callback table. The entry sits between two scanner-discovered routines
    # and therefore has no direct JAL edge in the main executable.
    {"address": "0x8005D80C"},
    {"address": "0x8005D810"},
]

OVERLAYS = [
    (0, 0x30, 144709, "0x80012254"),
    (1, 0x23578, 44333, "0x80011384"),
    (2, 0x2E2A8, 53389, "0x80011750"),
    (3, 0x3B338, 5195, "0x80012C00"),
    (4, 0x3C784, 38461, "0x80013628"),
    (5, 0x45DC4, 3602, "0x800114B8"),
]

OVERLAY_EXTRA_FUNCTIONS = {
    # Indirect object callback reached from overlay 0's simulation-mode
    # initializer. It has no direct JAL reference for discovery to follow.
    0: [
        # Alternate scheduler state entered after the transmission selection.
        "0x80016010",
        "0x80016040",
        "0x80016088",
        # Replay teardown uses the fourth scheduler method before selecting
        # the common epilogue. Both addresses are installed as indirect
        # callbacks, so linear sweep alone cannot expose callable entries.
        "0x800160D0",
        # Scheduler resume path installed in the race object's callback table.
        # It is an internal label within the scanner's 0x80015FF8 function,
        # so there is no direct JAL edge that would otherwise expose it.
        "0x80016100",
        # Object-method entry reached indirectly by the scheduler resume path.
        "0x80016130",
        # Results-screen dismissal method selected from the same object table.
        "0x80016160",
        "0x80016188",
        "0x800161B8",
        "0x800161E8",
        "0x80016258",
    ],
    # Title overlay callbacks installed in the shared list object's function
    # table. Option reaches 0x80018574 indirectly, and several branches use
    # 0x800186D0 as a callable common epilogue. Neither has a direct JAL edge,
    # so both must remain explicit recompiler roots.
    1: [
        "0x80018574",
        "0x800186D0",
    ],
}


def overlay_configs() -> list[dict[str, object]]:
    return [
        {
            "name": f"gt2_overlay_{index}",
            "base": "0x80010000",
            "file": "GT2.OVL",
            "offset": offset,
            "size": packed_size,
            "gzip": True,
            "linearSweep": True,
            "functions": [
                {"address": "0x80010000"},
                {"address": entry},
                *(
                    {"address": address}
                    for address in OVERLAY_EXTRA_FUNCTIONS.get(index, [])
                ),
            ],
        }
        for index, offset, packed_size, entry in OVERLAYS
    ]


def relative_posix(path: Path, start: Path) -> str:
    return Path(os.path.relpath(path.resolve(), start.resolve())).as_posix()


def main() -> int:
    required = [
        CUE,
        DISC_ROOT / "SYSTEM.CNF",
        DISC_ROOT / "SCUS_944.88",
        DISC_ROOT / "GT2.OVL",
        DISC_ROOT / "GT2.VOL",
    ]
    missing = [path for path in required if not path.is_file()]
    if missing:
        raise FileNotFoundError(
            "missing bootstrap input(s): " + ", ".join(str(path) for path in missing)
        )

    OUTPUT.mkdir(parents=True, exist_ok=True)
    config = {
        "game": {
            "id": "SCUS-94488",
            "name": "GranTurismo2PC",
            "namespace": "Recompiled.Simulation",
            "title": "Gran Turismo 2 PC",
            "output": "recompiled",
        },
        "cue": relative_posix(CUE, OUTPUT),
        "debug": os.environ.get("GT2_RECOMP_DEBUG") == "1",
        "linearSweep": True,
        "functions": EXTRA_MAIN_FUNCTIONS,
        "overlays": overlay_configs(),
        "stubs": [],
        "ignored": [],
        "patches": [
            {
                "overlay": "main",
                "address": "80010954",
                "target": "RecompOne.Runtime.Sdk.GT2Compat.WaitForInitialVBlanks",
                "mode": "replace",
            },
            {
                "overlay": "main",
                "address": "8007D23C",
                "target": "RecompOne.Runtime.Sdk.GT2Compat.VSync",
                "mode": "replace",
            },
            {
                "overlay": "main",
                "address": "80080B10",
                "target": "RecompOne.Runtime.Sdk.GT2Compat.TraceRenderSchedulerEntry",
                "mode": "pre",
            },
            {
                "overlay": "main",
                "address": "8007C550",
                "target": "RecompOne.Runtime.Sdk.GT2Compat.WaitForCdCommand",
                "mode": "replace",
            },
            {
                "overlay": "main",
                "address": "80082054",
                "target": "RecompOne.Runtime.Sdk.GT2Compat.WaitForCdBuffer",
                "mode": "replace",
            },
            {
                "overlay": "main",
                "address": "8008A088",
                "target": "RecompOne.Runtime.Sdk.LibCd.CdSyncCallback",
                "mode": "replace",
            },
            {
                "overlay": "main",
                "address": "8008A0A8",
                "target": "RecompOne.Runtime.Sdk.LibCd.CdReadyCallback",
                "mode": "replace",
            },
            {
                "overlay": "main",
                "address": "8008A0C8",
                "target": "RecompOne.Runtime.Sdk.LibCd.CdControl",
                "mode": "replace",
            },
            {
                "overlay": "main",
                "address": "8008A204",
                "target": "RecompOne.Runtime.Sdk.LibCd.CdControlF",
                "mode": "replace",
            },
            {
                "overlay": "main",
                "address": "8008A338",
                "target": "RecompOne.Runtime.Sdk.LibCd.CdControlB",
                "mode": "replace",
            },
            {
                "overlay": "main",
                "address": "80089F18",
                "target": "RecompOne.Runtime.Sdk.LibCd.CdGetSector",
                "mode": "replace",
            },
            {
                "overlay": "main",
                "address": "8008AAEC",
                "target": "RecompOne.Runtime.Sdk.LibCd.CdSync",
                "mode": "replace",
            },
            {
                "overlay": "main",
                "address": "8008AD6C",
                "target": "RecompOne.Runtime.Sdk.LibCd.CdReady",
                "mode": "replace",
            },
            {
                "overlay": "main",
                "address": "8005DAD8",
                "target": "RecompOne.Runtime.Sdk.GT2Compat.TraceOverlayLoad",
                "mode": "pre",
            },
            {
                "overlay": "main",
                "address": "8007AD90",
                "target": "RecompOne.Runtime.Sdk.GT2Compat.Longjmp",
                "mode": "replace",
            },
            {
                "overlay": "main",
                "address": "80087148",
                "target": "RecompOne.Runtime.Sdk.LibPad.PadInitDirect",
                "mode": "pre",
            }
        ],
    }

    config_path = OUTPUT / "gt2.recompone.json"
    config_path.write_text(json.dumps(config, indent=2) + "\n", encoding="utf-8")

    host_source = REPO / "tools" / "reference-host"
    generated_project = OUTPUT / "recompiled"
    generated_project.mkdir(parents=True, exist_ok=True)
    for host_file in ("Program.cs", "GranTurismo2PC.csproj"):
        shutil.copy2(host_source / host_file, generated_project / host_file)

    print(f"Wrote {config_path}")
    print(f"Refreshed host project in {generated_project}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
