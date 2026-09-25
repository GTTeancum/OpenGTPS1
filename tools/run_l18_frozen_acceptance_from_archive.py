#!/usr/bin/env python3
"""Restore the frozen L08 baseline, then execute the final L18 visual replay.

This wrapper deliberately keeps restoration and acceptance fail-closed. It
never auto-promotes L18; successful completion means only that the exact L08
baseline was restored and that comparable L18 evidence was produced.
"""
from __future__ import annotations

import argparse
from pathlib import Path
from types import SimpleNamespace

import restore_l08_visual_baseline as restore_mod
import run_l18_against_frozen_l08 as accept_mod


def main() -> int:
    p = argparse.ArgumentParser(description=__doc__)
    source = p.add_mutually_exclusive_group(required=True)
    source.add_argument("--source-root", type=Path)
    source.add_argument("--archive-glob")
    p.add_argument("--archive-part", type=Path, action="append")
    p.add_argument("--l18-runner", type=Path, required=True)
    p.add_argument("--l08-checkpoint", type=Path, required=True)
    p.add_argument("--work-root", type=Path, required=True)
    p.add_argument(
        "--scenario",
        action="append",
        choices=sorted(accept_mod.SCENARIOS),
    )
    p.add_argument("--deadline", type=int, default=1500)
    args = p.parse_args()

    work = args.work_root.resolve()
    baseline = work / "l08-exact-baseline"
    evidence = work / "l18-frozen-l08-acceptance"
    work.mkdir(parents=True, exist_ok=True)

    restore_args = SimpleNamespace(
        source_root=args.source_root,
        archive_glob=args.archive_glob,
        archive_part=args.archive_part,
        output=baseline,
        identity_file=None,
        allow_blank_cardb=True,
        overwrite=False,
    )
    report = restore_mod.restore(restore_args)
    verification = report["verification"]
    if not verification["exactBaselineReady"]:
        print(
            "Exact L08 baseline restoration is incomplete; "
            "acceptance not started."
        )
        print(
            "unresolved:",
            ", ".join(verification["unresolved"]) or "none",
        )
        print(
            "mismatched:",
            ", ".join(verification["mismatched"]) or "none",
        )
        return 2

    acceptance_args = SimpleNamespace(
        data_root=baseline,
        l18_runner=args.l18_runner,
        l08_checkpoint=args.l08_checkpoint,
        output=evidence,
        scenario=args.scenario,
        deadline=args.deadline,
    )
    summary = accept_mod.run(acceptance_args)
    print(f"Exact L08 baseline: {baseline}")
    print(f"L18 evidence: {evidence}")
    print(
        "Structural pairing pass:",
        summary["structuralPairingPass"],
    )
    print("Automatic L18 promotion: false")
    return 0 if summary["structuralPairingPass"] else 3


if __name__ == "__main__":
    raise SystemExit(main())
