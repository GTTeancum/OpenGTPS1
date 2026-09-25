#!/usr/bin/env python3
"""Replay L18 against the frozen L08 visual-acceptance manifests.

This is the preferred final L18 acceptance path because the L08 checkpoint
retains authoritative CAPTURE-MANIFEST.json files for every historical
acceptance frame. No L08 executable is required.

The script does not auto-promote L18. It enforces the historical launch
contract, poll/frame pairing, output dimensions, and records exact SHA-256
matches/mismatches for human visual review of intentional L09/L10 deltas.
"""
from __future__ import annotations

import argparse
import json
from pathlib import Path
import shutil
import subprocess
import tempfile

from run_l08_l18_visual_acceptance import (
    REQUIRED_DATA,
    RUNNER_FILES,
    SCENARIOS,
    capture_key,
    read_ppm,
    run_one,
    sha256_file,
)

MANIFEST_DIRS = {
    "seattle": "seattle-L08",
    "redrock": "redrock-L08",
    "redrock-camera-stress": "redrock-L08-camera-stress",
}

FROZEN_BASELINE_NAMES = (
    "GT2.VOL",
    "MUSIC.DAT",
    "TITLE_EXACT.DAT",
    "settings.json",
    "carda.sav",
    "cardb.sav",
    "GTLIVERY.BIN",
)


def require(path: Path) -> Path:
    if not path.is_file():
        raise FileNotFoundError(path)
    return path


def manifest_path(checkpoint: Path, scenario: str) -> Path:
    return checkpoint / "validation/game-runs" / MANIFEST_DIRS[scenario] / "CAPTURE-MANIFEST.json"


def validate_inputs(data: Path, l18: Path, l08_checkpoint: Path, scenarios: list[str]) -> dict:
    identities_path = require(
        l08_checkpoint / "recovery/manifests/prepared-game-input-identities.json"
    )
    identity_rows = json.loads(identities_path.read_text())
    expected_by_name = {row["name"]: row for row in identity_rows}

    data_rows = {}
    for rel in REQUIRED_DATA:
        p = require(data / rel)
        data_rows[rel] = {"bytes": p.stat().st_size, "sha256": sha256_file(p)}

    # Frozen-L08 mode is intentionally strict: the game payload and mutable
    # settings/card state must be byte-identical to the historical acceptance
    # baseline, otherwise visual differences are not attributable to L18.
    baseline_checks = {}
    for name in FROZEN_BASELINE_NAMES:
        expected = expected_by_name.get(name)
        if expected is None:
            raise ValueError(f"L08 checkpoint is missing historical identity for {name}")
        p = require(data / name)
        actual = {"size": p.stat().st_size, "sha256": sha256_file(p)}
        baseline_checks[name] = {"expected": expected, "actual": actual}
        if actual["size"] != expected["size"] or actual["sha256"] != expected["sha256"]:
            raise RuntimeError(
                f"prepared GT2 baseline mismatch for {name}: "
                f"expected {expected['size']} bytes {expected['sha256']}, "
                f"got {actual['size']} bytes {actual['sha256']}"
            )
        data_rows[name] = {"bytes": actual["size"], "sha256": actual["sha256"]}

    runner_rows = {}
    for rel in RUNNER_FILES:
        p = require(l18 / rel)
        runner_rows[rel] = {"bytes": p.stat().st_size, "sha256": sha256_file(p)}

    l08_shader = require(l08_checkpoint / "recovery/source/lighting/in-game-L05.shader")
    frozen = {
        "shader": {"bytes": l08_shader.stat().st_size, "sha256": sha256_file(l08_shader)},
        "manifests": {},
    }
    if frozen["shader"]["sha256"] != runner_rows["lighting/in-game-L05.shader"]["sha256"]:
        raise RuntimeError("frozen L08 and L18 lighting/in-game-L05.shader differ")

    for scenario in scenarios:
        p = require(manifest_path(l08_checkpoint, scenario))
        rows = json.loads(p.read_text())
        if not isinstance(rows, list) or not rows:
            raise ValueError(f"invalid frozen L08 capture manifest: {p}")
        frozen["manifests"][scenario] = {
            "path": str(p), "count": len(rows), "sha256": sha256_file(p)
        }
    frozen["preparedBaselineIdentityFile"] = {
        "path": str(identities_path),
        "sha256": sha256_file(identities_path),
    }
    frozen["preparedBaselineChecks"] = baseline_checks
    return {"baseline": data_rows, "L18": runner_rows, "frozenL08": frozen}


def load_manifest(checkpoint: Path, scenario: str) -> dict[tuple[int, int], dict]:
    rows = json.loads(manifest_path(checkpoint, scenario).read_text())
    indexed = {}
    for row in rows:
        key = capture_key(Path(row["file"]))
        if key in indexed:
            raise ValueError(f"duplicate frozen L08 capture key {scenario}: {key}")
        indexed[key] = row
    return indexed


def compare(checkpoint: Path, output: Path, scenario: str) -> dict:
    expected = load_manifest(checkpoint, scenario)
    cap_root = output / scenario / "L18" / "captures"
    actual = {capture_key(p): p for p in sorted(cap_root.glob("*.ppm"))}
    exp_keys, act_keys = set(expected), set(actual)
    common = sorted(exp_keys & act_keys)
    frames = []
    exact = 0
    dim_mismatches = []
    for poll, frame in common:
        row = expected[(poll, frame)]
        p = actual[(poll, frame)]
        width, height, _ = read_ppm(p)
        meta = row.get("metadata") or {}
        expected_dims = (meta.get("width"), meta.get("height"))
        dims_match = (width, height) == expected_dims
        if not dims_match:
            dim_mismatches.append({
                "poll": poll, "frame": frame,
                "L08": list(expected_dims), "L18": [width, height],
            })
        actual_sha = sha256_file(p)
        hash_match = actual_sha == row.get("sha256")
        exact += int(hash_match)
        frames.append({
            "poll": poll,
            "frame": frame,
            "exactHashMatch": hash_match,
            "dimensionsMatch": dims_match,
            "L08": {
                "file": row.get("file"),
                "sha256": row.get("sha256"),
                "bytes": row.get("bytes"),
                "width": meta.get("width"),
                "height": meta.get("height"),
                "native": meta.get("native"),
                "commands": meta.get("commands"),
                "authoredFrame": meta.get("authoredFrame"),
                "sourcePoll": meta.get("sourcePoll"),
                "hostPoll": meta.get("hostPoll"),
            },
            "L18": {
                "file": p.name,
                "sha256": actual_sha,
                "bytes": p.stat().st_size,
                "width": width,
                "height": height,
            },
        })
    return {
        "scenario": scenario,
        "L08CaptureCount": len(expected),
        "L18CaptureCount": len(actual),
        "pairedCaptureCount": len(common),
        "exactHashMatchCount": exact,
        "exactHashMismatchCount": len(common) - exact,
        "missingFromL18": [{"poll": p, "frame": f} for p, f in sorted(exp_keys - act_keys)],
        "unexpectedInL18": [{"poll": p, "frame": f} for p, f in sorted(act_keys - exp_keys)],
        "dimensionMismatches": dim_mismatches,
        "pairingPass": bool(common) and exp_keys == act_keys and not dim_mismatches,
        "interpretation": (
            "Exact hash matches prove pixel identity. Hash mismatches are evidence, not an "
            "automatic failure, because L09/L10 intentionally changed billboard/shadow behavior."
        ),
        "frames": frames,
    }


def run(args: argparse.Namespace) -> dict:
    data = args.data_root.resolve()
    l18 = args.l18_runner.resolve()
    checkpoint = args.l08_checkpoint.resolve()
    output = args.output.resolve()
    if output.exists():
        raise FileExistsError(output)
    output.mkdir(parents=True)
    scenarios = args.scenario or ["seattle", "redrock", "redrock-camera-stress"]
    identities = validate_inputs(data, l18, checkpoint, scenarios)
    (output / "input-identities.json").write_text(json.dumps(identities, indent=2) + "\n")

    runs, comparisons = [], []
    for scenario in scenarios:
        runs.append(run_one("L18", l18, data, scenario, SCENARIOS[scenario], output, args.deadline))
        comparisons.append(compare(checkpoint, output, scenario))

    summary = {
        "purpose": "Replay L18 against frozen L08 visual authority",
        "automaticPromotion": False,
        "promotionRule": (
            "Never promote L18 automatically. Pairing/dimensions are hard structural gates; "
            "hash results identify exact identity or changed frames; final promotion requires visual review."
        ),
        "recordedL08Contract": {name: SCENARIOS[name] for name in scenarios},
        "runs": runs,
        "comparisons": comparisons,
        "structuralPairingPass": all(x["pairingPass"] for x in comparisons),
    }
    (output / "summary.json").write_text(json.dumps(summary, indent=2) + "\n")
    return summary


def make_fake_runner(root: Path, delta: int = 0) -> None:
    (root / "runtime").mkdir(parents=True)
    (root / "bin/runtimes/linux-x64/native").mkdir(parents=True)
    (root / "lighting").mkdir(parents=True)
    for rel in ("bin/GranTurismo2PC.dll", "bin/RecompOne.Runtime.dll", "bin/libopengt_live_renderer.so"):
        (root / rel).write_bytes((rel + "\n").encode())
    (root / "lighting/in-game-L05.shader").write_text("identical-shader\n")
    dotnet = root / "runtime/dotnet"
    dotnet.write_text(
        "#!/usr/bin/env python3\n"
        "import os,pathlib\n"
        "out=pathlib.Path(os.environ['OPENGT_LINUX_CAPTURE_DIR']);out.mkdir(parents=True,exist_ok=True)\n"
        "start=int(os.environ['OPENGT_LINUX_CAPTURE_START_POLL']);step=int(os.environ['OPENGT_LINUX_CAPTURE_EVERY'])\n"
        f"delta={delta}\n"
        "for k in range(3):\n"
        " p=start+k*step; w,h=4,3; data=bytearray()\n"
        " for i in range(w*h): data.extend(((i*11+delta)%256,(i*7)%256,(i*3)%256))\n"
        " (out/f'{k:04d}_game_poll{p}_frame{p}.ppm').write_bytes(f'P6\\n{w} {h}\\n255\\n'.encode()+data)\n"
    )
    dotnet.chmod(0o755)


def self_test() -> None:
    with tempfile.TemporaryDirectory(prefix="opengt-frozen-l08-") as td:
        root = Path(td)
        data = root / "data"
        data.mkdir()
        for rel in REQUIRED_DATA:
            p = data / rel
            p.parent.mkdir(parents=True, exist_ok=True)
            p.write_bytes((rel + "\n").encode())
        (data / "GTLIVERY.BIN").write_bytes(b"GTLIVERY.BIN\n")
        l18 = root / "l18"
        make_fake_runner(l18)
        checkpoint = root / "l08-checkpoint"
        shader = checkpoint / "recovery/source/lighting/in-game-L05.shader"
        shader.parent.mkdir(parents=True)
        shutil.copy2(l18 / "lighting/in-game-L05.shader", shader)
        identities = []
        for name in FROZEN_BASELINE_NAMES:
            p = data / name
            identities.append({
                "name": name,
                "size": p.stat().st_size,
                "sha256": sha256_file(p),
            })
        identity_path = checkpoint / "recovery/manifests/prepared-game-input-identities.json"
        identity_path.parent.mkdir(parents=True, exist_ok=True)
        identity_path.write_text(json.dumps(identities, indent=2) + "\n")

        expected_out = root / "expected"
        expected_out.mkdir()
        for scenario in ("seattle",):
            run_one("L18", l18, data, scenario, SCENARIOS[scenario], expected_out, 30)
            rows = []
            for p in sorted((expected_out / scenario / "L18/captures").glob("*.ppm")):
                w, h, _ = read_ppm(p)
                poll, frame = capture_key(p)
                rows.append({
                    "file": p.name, "bytes": p.stat().st_size, "sha256": sha256_file(p),
                    "metadata": {"native": True, "width": w, "height": h,
                                 "authoredFrame": frame, "sourcePoll": poll - 1,
                                 "hostPoll": poll, "commands": 123},
                })
            m = checkpoint / "validation/game-runs/seattle-L08/CAPTURE-MANIFEST.json"
            m.parent.mkdir(parents=True)
            m.write_text(json.dumps(rows, indent=2) + "\n")

        args = argparse.Namespace(data_root=data, l18_runner=l18, l08_checkpoint=checkpoint,
                                  output=root / "actual", scenario=["seattle"], deadline=30)
        summary = run(args)
        comp = summary["comparisons"][0]
        assert comp["pairingPass"] and comp["exactHashMismatchCount"] == 0

        changed_l18 = root / "l18-changed"
        make_fake_runner(changed_l18, 1)
        shutil.copy2(l18 / "lighting/in-game-L05.shader", changed_l18 / "lighting/in-game-L05.shader")
        changed = argparse.Namespace(data_root=data, l18_runner=changed_l18, l08_checkpoint=checkpoint,
                                     output=root / "changed", scenario=["seattle"], deadline=30)
        changed_summary = run(changed)
        changed_comp = changed_summary["comparisons"][0]
        assert changed_comp["pairingPass"] and changed_comp["exactHashMismatchCount"] > 0
    print("OpenGT frozen-L08 acceptance runner self-test: PASS")


def parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("--data-root", type=Path)
    p.add_argument("--l18-runner", type=Path)
    p.add_argument("--l08-checkpoint", type=Path)
    p.add_argument("--output", type=Path)
    p.add_argument("--scenario", action="append", choices=sorted(SCENARIOS))
    p.add_argument("--deadline", type=int, default=1500)
    p.add_argument("--self-test", action="store_true")
    args = p.parse_args()
    if not args.self_test:
        missing = [n for n in ("data_root", "l18_runner", "l08_checkpoint", "output") if getattr(args, n) is None]
        if missing:
            p.error("required without --self-test: " + ", ".join(missing))
    return args


def main() -> int:
    args = parse_args()
    if args.self_test:
        self_test()
        return 0
    summary = run(args)
    print(json.dumps({
        "output": str(args.output.resolve()),
        "structuralPairingPass": summary["structuralPairingPass"],
        "automaticPromotion": False,
    }, indent=2))
    return 0 if summary["structuralPairingPass"] else 2


if __name__ == "__main__":
    raise SystemExit(main())
