#!/usr/bin/env python3
"""Deterministic L08-vs-L18 OpenGT visual acceptance runner.

The tool intentionally does not choose a visual winner or auto-promote L18.
It recreates the recorded L08 Seattle/Red Rock launch contract for both
checkpoints from one byte-identical baseline, pairs captures by poll/frame,
and emits exact identities plus objective pixel-difference evidence for review.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import math
import os
from pathlib import Path
import re
import shutil
import subprocess
import tempfile
import time
from typing import Iterable

SCENARIOS = {
    "seattle": {
        "course": "seattle-circuit",
        "input": "1000+8=R1;1020+1500=CROSS",
        "exit_poll": 1680,
        "capture_start": 990,
        "capture_every": 6,
    },
    "redrock": {
        "course": "red-rock-valley-speedway",
        "input": "1000+8=R1;1020+1500=CROSS",
        "exit_poll": 1680,
        "capture_start": 990,
        "capture_every": 6,
    },
    "redrock-camera-stress": {
        "course": "red-rock-valley-speedway",
        "input": (
            "1000+8=R1;1020+900=CROSS;1280+8=R1;1450+8=R1;"
            "1640+8=R1;1720+16=LEFT"
        ),
        "exit_poll": 1900,
        "capture_start": 1200,
        "capture_every": 12,
    },
}

BASE_ENV = {
    "DISPLAY": ":98",
    "DOTNET_CLI_TELEMETRY_OPTOUT": "1",
    "LIBGL_ALWAYS_SOFTWARE": "1",
    "LP_NUM_THREADS": "2",
    "SDL_AUDIODRIVER": "dummy",
    "RECOMPONE_OUTPUT_RESOLUTION": "1280x720",
    "RECOMPONE_DISABLE_LIVE_INPUT": "1",
    "RECOMPONE_CAPTURE_AUTOMATIC_STAGE": "0",
    "OPENGT_LINUX_RENDER_SCALE": "3",
    "OPENGT_LINUX_WAIT_FOR_RENDER": "1",
    "OPENGT_LINUX_CAPTURE_LIMIT": "400",
    "OPENGT_LIGHTING_AUDIT": "1",
    "OPENGT_SHADOW_AUDIT": "1",
    "OPENGT_WORLD_CAMERA_AUDIT": "1",
}

REQUIRED_DATA = (
    "GT2.VOL",
    "MUSIC.DAT",
    "TITLE_EXACT.DAT",
    "settings.json",
    "carda.sav",
    "cardb.sav",
    "manifests/simulation.json",
    "manifests/arcade.json",
)
RUNNER_FILES = (
    "runtime/dotnet",
    "bin/GranTurismo2PC.dll",
    "bin/RecompOne.Runtime.dll",
    "bin/libopengt_live_renderer.so",
    "lighting/in-game-L05.shader",
)
CAPTURE_RE = re.compile(r"poll(?P<poll>\d+)_frame(?P<frame>\d+)\.ppm$", re.I)


def sha256_file(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def require_file(root: Path, relative: str) -> Path:
    path = root / relative
    if not path.is_file():
        raise FileNotFoundError(f"missing required file: {path}")
    return path


def validate_inputs(data_root: Path, runners: dict[str, Path]) -> dict:
    data = {}
    for rel in REQUIRED_DATA:
        path = require_file(data_root, rel)
        data[rel] = {"bytes": path.stat().st_size, "sha256": sha256_file(path)}
    optional = data_root / "GTLIVERY.BIN"
    if optional.is_file():
        data["GTLIVERY.BIN"] = {
            "bytes": optional.stat().st_size,
            "sha256": sha256_file(optional),
        }

    runner_info = {}
    for name, root in runners.items():
        files = {}
        for rel in RUNNER_FILES:
            path = require_file(root, rel)
            files[rel] = {"bytes": path.stat().st_size, "sha256": sha256_file(path)}
        runner_info[name] = files

    shader_hashes = {
        info["lighting/in-game-L05.shader"]["sha256"]
        for info in runner_info.values()
    }
    if len(shader_hashes) != 1:
        raise RuntimeError("L08 and L18 lighting/in-game-L05.shader are not byte-identical")
    return {"baseline": data, "runners": runner_info}


def clone_tree_baseline(source: Path, destination: Path) -> None:
    if destination.exists():
        raise FileExistsError(destination)
    destination.mkdir(parents=True)
    mutable_names = {"settings.json", "carda.sav", "cardb.sav"}
    for src in source.rglob("*"):
        rel = src.relative_to(source)
        dst = destination / rel
        if src.is_dir():
            dst.mkdir(parents=True, exist_ok=True)
            continue
        dst.parent.mkdir(parents=True, exist_ok=True)
        if src.name in mutable_names:
            shutil.copy2(src, dst)
            continue
        try:
            os.link(src, dst)
        except OSError:
            shutil.copy2(src, dst)


def runner_env(runner: Path, capture_dir: Path, spec: dict) -> dict[str, str]:
    env = os.environ.copy()
    env.update(BASE_ENV)
    env.update(
        {
            "DOTNET_ROOT": str((runner / "runtime").resolve()),
            "LD_LIBRARY_PATH": (
                f"{(runner / 'bin').resolve()}:"
                f"{(runner / 'bin/runtimes/linux-x64/native').resolve()}"
                + (f":{os.environ['LD_LIBRARY_PATH']}" if os.environ.get("LD_LIBRARY_PATH") else "")
            ),
            "RECOMPONE_EXIT_AFTER_INPUT_POLL": str(spec["exit_poll"]),
            "RECOMPONE_INPUT_SCRIPT": spec["input"],
            "OPENGT_LINUX_CAPTURE_DIR": str(capture_dir.resolve()),
            "OPENGT_LINUX_CAPTURE_EVERY": str(spec["capture_every"]),
            "OPENGT_LINUX_CAPTURE_START_POLL": str(spec["capture_start"]),
            "OPENGT_LIGHTING_SCRIPT": str((runner / "lighting/in-game-L05.shader").resolve()),
        }
    )
    return env


def run_one(
    checkpoint: str,
    runner: Path,
    baseline: Path,
    scenario: str,
    spec: dict,
    output: Path,
    deadline: int,
) -> dict:
    run_root = output / scenario / checkpoint
    game = run_root / "game"
    captures = run_root / "captures"
    run_root.mkdir(parents=True, exist_ok=False)
    clone_tree_baseline(baseline, game)
    captures.mkdir()

    env = runner_env(runner, captures, spec)
    cmd = [
        str((runner / "runtime/dotnet").resolve()),
        str((runner / "bin/GranTurismo2PC.dll").resolve()),
        "--headless",
        "--mute",
        "--arcade-race",
        spec["course"],
        str(game.resolve()),
    ]
    launch_env = {k: env[k] for k in sorted(set(BASE_ENV) | {
        "DOTNET_ROOT", "LD_LIBRARY_PATH", "RECOMPONE_EXIT_AFTER_INPUT_POLL",
        "RECOMPONE_INPUT_SCRIPT", "OPENGT_LINUX_CAPTURE_DIR",
        "OPENGT_LINUX_CAPTURE_EVERY", "OPENGT_LINUX_CAPTURE_START_POLL",
        "OPENGT_LIGHTING_SCRIPT",
    })}
    (run_root / "launch.json").write_text(
        json.dumps({"command": cmd, "environment": launch_env, "deadlineSeconds": deadline}, indent=2)
        + "\n"
    )

    started = time.monotonic()
    timed_out = False
    log_path = run_root / "runtime.log"
    with log_path.open("w", encoding="utf-8", errors="replace") as log:
        try:
            proc = subprocess.run(
                cmd,
                env=env,
                stdout=log,
                stderr=subprocess.STDOUT,
                timeout=deadline,
                check=False,
            )
            exit_code = proc.returncode
        except subprocess.TimeoutExpired:
            exit_code = 124
            timed_out = True
    elapsed = time.monotonic() - started
    capture_files = sorted(captures.glob("*.ppm"))
    result = {
        "checkpoint": checkpoint,
        "scenario": scenario,
        "exitCode": exit_code,
        "timeout": timed_out,
        "wallSeconds": elapsed,
        "captureCount": len(capture_files),
    }
    (run_root / "result.json").write_text(json.dumps(result, indent=2) + "\n")
    if exit_code != 0:
        raise RuntimeError(f"{checkpoint}/{scenario} failed with exit code {exit_code}; see {log_path}")
    if not capture_files:
        raise RuntimeError(f"{checkpoint}/{scenario} produced no PPM captures")
    return result


def ppm_tokens(f) -> Iterable[bytes]:
    while True:
        token = bytearray()
        while True:
            ch = f.read(1)
            if not ch:
                if token:
                    yield bytes(token)
                return
            if ch == b"#":
                f.readline()
                if token:
                    yield bytes(token)
                    break
                continue
            if ch.isspace():
                if token:
                    yield bytes(token)
                    break
                continue
            token.extend(ch)


def read_ppm(path: Path) -> tuple[int, int, bytes]:
    with path.open("rb") as f:
        tokens = ppm_tokens(f)
        magic = next(tokens)
        if magic != b"P6":
            raise ValueError(f"{path}: only binary P6 PPM is supported, got {magic!r}")
        width = int(next(tokens))
        height = int(next(tokens))
        maxval = int(next(tokens))
        if maxval != 255:
            raise ValueError(f"{path}: expected maxval 255, got {maxval}")
        pixels = f.read(width * height * 3)
        if len(pixels) != width * height * 3:
            raise ValueError(f"{path}: truncated PPM pixel payload")
        return width, height, pixels


def capture_key(path: Path) -> tuple[int, int]:
    m = CAPTURE_RE.search(path.name)
    if not m:
        raise ValueError(f"unrecognized capture name: {path.name}")
    return int(m.group("poll")), int(m.group("frame"))


def write_diff_ppm(path: Path, width: int, height: int, a: bytes, b: bytes) -> None:
    diff = bytes(min(255, abs(x - y) * 4) for x, y in zip(a, b))
    path.write_bytes(f"P6\n{width} {height}\n255\n".encode("ascii") + diff)


def compare_scenario(output: Path, scenario: str) -> dict:
    roots = {
        cp: output / scenario / cp / "captures"
        for cp in ("L08", "L18")
    }
    indexed = {}
    for cp, root in roots.items():
        indexed[cp] = {capture_key(p): p for p in sorted(root.glob("*.ppm"))}
    keys08 = set(indexed["L08"])
    keys18 = set(indexed["L18"])
    common = sorted(keys08 & keys18)
    if keys08 != keys18:
        missing18 = sorted(keys08 - keys18)
        missing08 = sorted(keys18 - keys08)
    else:
        missing18 = missing08 = []

    diff_dir = output / scenario / "diff-amplified-4x"
    diff_dir.mkdir(exist_ok=True)
    frames = []
    for poll, frame in common:
        p08 = indexed["L08"][(poll, frame)]
        p18 = indexed["L18"][(poll, frame)]
        w08, h08, a = read_ppm(p08)
        w18, h18, b = read_ppm(p18)
        if (w08, h08) != (w18, h18):
            raise RuntimeError(
                f"{scenario} poll {poll}: dimension mismatch "
                f"L08={w08}x{h08} L18={w18}x{h18}"
            )
        n = len(a)
        sum_abs = 0
        sum_sq = 0
        max_abs = 0
        changed_pixels = 0
        for i in range(0, n, 3):
            pixel_changed = False
            for channel in range(3):
                d = abs(a[i + channel] - b[i + channel])
                sum_abs += d
                sum_sq += d * d
                max_abs = max(max_abs, d)
                pixel_changed |= d != 0
            changed_pixels += int(pixel_changed)
        pixels = w08 * h08
        diff_path = diff_dir / f"poll{poll:04d}_frame{frame:04d}.ppm"
        write_diff_ppm(diff_path, w08, h08, a, b)
        frames.append(
            {
                "poll": poll,
                "frame": frame,
                "width": w08,
                "height": h08,
                "l08": {"file": p08.name, "sha256": sha256_file(p08)},
                "l18": {"file": p18.name, "sha256": sha256_file(p18)},
                "meanAbsoluteChannelDelta": sum_abs / n,
                "rmsChannelDelta": math.sqrt(sum_sq / n),
                "maxChannelDelta": max_abs,
                "changedPixelPercent": 100.0 * changed_pixels / pixels,
                "diffAmplified4x": str(diff_path.relative_to(output)),
            }
        )
    return {
        "scenario": scenario,
        "l08CaptureCount": len(indexed["L08"]),
        "l18CaptureCount": len(indexed["L18"]),
        "pairedCaptureCount": len(common),
        "missingFromL18": [{"poll": p, "frame": f} for p, f in missing18],
        "missingFromL08": [{"poll": p, "frame": f} for p, f in missing08],
        "pairingPass": bool(common) and not missing18 and not missing08,
        "frames": frames,
    }


def run_acceptance(args: argparse.Namespace) -> dict:
    data_root = args.data_root.resolve()
    runners = {"L08": args.l08_runner.resolve(), "L18": args.l18_runner.resolve()}
    output = args.output.resolve()
    if output.exists():
        raise FileExistsError(f"output directory already exists: {output}")
    output.mkdir(parents=True)

    identities = validate_inputs(data_root, runners)
    (output / "input-identities.json").write_text(json.dumps(identities, indent=2) + "\n")

    selected = args.scenario or ["seattle", "redrock", "redrock-camera-stress"]
    run_results = []
    comparisons = []
    for scenario in selected:
        spec = SCENARIOS[scenario]
        for checkpoint in ("L08", "L18"):
            run_results.append(
                run_one(
                    checkpoint,
                    runners[checkpoint],
                    data_root,
                    scenario,
                    spec,
                    output,
                    args.deadline,
                )
            )
        comparisons.append(compare_scenario(output, scenario))

    summary = {
        "purpose": "L08 visual authority vs L18 candidate matched-baseline evidence",
        "automaticPromotion": False,
        "promotionRule": (
            "This harness never promotes L18 automatically. Review paired captures, "
            "4x diff images, runtime logs, shadow/camera audits, and presentation integrity."
        ),
        "recordedL08Contract": {
            name: dict(spec) for name, spec in SCENARIOS.items() if name in selected
        },
        "runs": run_results,
        "comparisons": comparisons,
        "structuralPairingPass": all(x["pairingPass"] for x in comparisons),
    }
    (output / "summary.json").write_text(json.dumps(summary, indent=2) + "\n")
    return summary


def make_fake_runner(root: Path, delta: int) -> None:
    (root / "runtime").mkdir(parents=True)
    (root / "bin/runtimes/linux-x64/native").mkdir(parents=True)
    (root / "lighting").mkdir(parents=True)
    for rel in (
        "bin/GranTurismo2PC.dll",
        "bin/RecompOne.Runtime.dll",
        "bin/libopengt_live_renderer.so",
    ):
        (root / rel).write_bytes((rel + "\n").encode())
    (root / "lighting/in-game-L05.shader").write_text("identical-shader\n")
    fake = root / "runtime/dotnet"
    fake.write_text(
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
    fake.chmod(0o755)


def self_test() -> None:
    with tempfile.TemporaryDirectory(prefix="opengt-visual-accept-") as td:
        root = Path(td)
        data = root / "data"
        data.mkdir()
        for rel in REQUIRED_DATA:
            p = data / rel
            p.parent.mkdir(parents=True, exist_ok=True)
            p.write_bytes((rel + "\n").encode())
        l08 = root / "l08"
        l18 = root / "l18"
        make_fake_runner(l08, 0)
        make_fake_runner(l18, 0)
        ns = argparse.Namespace(
            data_root=data,
            l08_runner=l08,
            l18_runner=l18,
            output=root / "out",
            scenario=["seattle"],
            deadline=30,
        )
        summary = run_acceptance(ns)
        comp = summary["comparisons"][0]
        assert comp["pairingPass"] and comp["pairedCaptureCount"] == 3
        assert all(f["rmsChannelDelta"] == 0 for f in comp["frames"])
        target = ns.output / "seattle/L18/captures/0001_game_poll996_frame996.ppm"
        raw = bytearray(target.read_bytes())
        raw[-1] ^= 0x01
        target.write_bytes(raw)
        changed = compare_scenario(ns.output, "seattle")
        assert changed["pairingPass"]
        assert any(f["rmsChannelDelta"] > 0 for f in changed["frames"])
    print("OpenGT L08/L18 visual acceptance harness self-test: PASS")


def parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("--data-root", type=Path)
    p.add_argument("--l08-runner", type=Path)
    p.add_argument("--l18-runner", type=Path)
    p.add_argument("--output", type=Path)
    p.add_argument("--scenario", action="append", choices=sorted(SCENARIOS))
    p.add_argument("--deadline", type=int, default=1500)
    p.add_argument("--self-test", action="store_true")
    args = p.parse_args()
    if not args.self_test:
        missing = [name for name in ("data_root", "l08_runner", "l18_runner", "output") if getattr(args, name) is None]
        if missing:
            p.error("required without --self-test: " + ", ".join("--" + x.replace("_", "-") for x in missing))
    return args


def main() -> int:
    args = parse_args()
    if args.self_test:
        self_test()
        return 0
    summary = run_acceptance(args)
    print(json.dumps({
        "output": str(args.output.resolve()),
        "structuralPairingPass": summary["structuralPairingPass"],
        "automaticPromotion": False,
    }, indent=2))
    return 0 if summary["structuralPairingPass"] else 2


if __name__ == "__main__":
    raise SystemExit(main())
