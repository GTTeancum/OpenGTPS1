#!/usr/bin/env python3
"""Extract the GT2 Simulation Disc with the proven raw-sector extractor."""

from __future__ import annotations

import subprocess
import sys
from pathlib import Path


REPO = Path(__file__).resolve().parents[1]
REFERENCE = Path(r"C:\Programming\GitHub\Vigilante-8-recomp")
EXTRACTOR = REFERENCE / "tools" / "extract_psx_iso.py"
IMAGE = REPO / "Gran Turismo 2 [Simulation Disc] [U] [SCUS-94488].img"
OUTPUT = REPO / "work" / "disc"


def main() -> int:
    if not EXTRACTOR.is_file():
        raise FileNotFoundError(f"reference extractor is missing: {EXTRACTOR}")
    if not IMAGE.is_file():
        raise FileNotFoundError(f"disc image is missing: {IMAGE}")
    return subprocess.call(
        [sys.executable, str(EXTRACTOR), str(IMAGE), str(OUTPUT), "--clean"]
    )


if __name__ == "__main__":
    raise SystemExit(main())
