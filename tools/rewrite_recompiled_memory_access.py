#!/usr/bin/env python3
"""Rewrite generated guest loads/stores through the concrete memory boundary."""

from pathlib import Path
import re
import sys


METHODS = (
    "ReadU8",
    "ReadU16",
    "ReadU32",
    "WriteU8",
    "WriteU16",
    "WriteU32",
    "ReadWordLeft",
    "ReadWordRight",
    "WriteWordLeft",
    "WriteWordRight",
)

SEATTLE_PROFILE_PATH = Path(__file__).with_name(
    "seattle_arcade_hot_methods.txt"
)


def load_seattle_hot_methods() -> tuple[str, ...]:
    methods = tuple(
        line.strip()
        for line in SEATTLE_PROFILE_PATH.read_text(encoding="utf-8").splitlines()
        if line.strip()
    )
    invalid = [
        method for method in methods
        if re.fullmatch(
            r"func_[0-9A-F]{8}(?:(?:_gt2_arcade_overlay_[0-9]+)|_main)?",
            method,
        ) is None
    ]
    if invalid:
        raise RuntimeError(
            f"invalid Seattle profile methods: {', '.join(invalid)}"
        )
    if len(methods) != len(set(methods)):
        raise RuntimeError("Seattle profile contains duplicate methods")
    return methods


SEATTLE_STARTUP_METHODS = load_seattle_hot_methods()


def rewrite_tree(
    root: Path,
    reset_optimization_profile: bool = False,
) -> tuple[int, int]:
    if not root.is_dir():
        raise FileNotFoundError(f"generated source directory is missing: {root}")
    changed_files = 0
    replacements = 0
    for path in sorted(root.glob("*.cs")):
        source = path.read_text(encoding="utf-8")
        rewritten = source
        for method in METHODS:
            old = f"m.{method}("
            count = rewritten.count(old)
            if count:
                rewritten = rewritten.replace(
                    old, f"MemoryAccess.{method}(m, "
                )
                replacements += count
        if rewritten != source:
            path.write_text(rewritten, encoding="utf-8")
            changed_files += 1

    if root.name.casefold() == "arcade-recompiled":
        candidates = sorted(root.glob("*.cs"))
        if reset_optimization_profile:
            optimized = (
                "System.Runtime.CompilerServices.MethodImplOptions.NoInlining | "
                "System.Runtime.CompilerServices.MethodImplOptions."
                "AggressiveOptimization"
            )
            baseline = (
                "System.Runtime.CompilerServices.MethodImplOptions.NoInlining"
            )
            for path in candidates:
                source = path.read_text(encoding="utf-8")
                count = source.count(optimized)
                if not count:
                    continue
                path.write_text(
                    source.replace(optimized, baseline),
                    encoding="utf-8",
                )
                changed_files += 1
                replacements += count
        profile_sources = {
            path: path.read_text(encoding="utf-8")
            for path in candidates
        }
        for method in SEATTLE_STARTUP_METHODS:
            marker = f"    public static void {method}(CpuContext c, IMemory m)"
            matches = [
                path for path in candidates
                if marker in profile_sources[path]
            ]
            if len(matches) != 1:
                raise RuntimeError(
                    f"expected one Arcade {method} definition, found "
                    f"{len(matches)}"
                )
            path = matches[0]
            source = profile_sources[path]
            old = (
                "    [System.Runtime.CompilerServices.MethodImpl("
                "System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]\n"
                f"{marker}"
            )
            new = (
                "    [System.Runtime.CompilerServices.MethodImpl("
                "System.Runtime.CompilerServices.MethodImplOptions.NoInlining | "
                "System.Runtime.CompilerServices.MethodImplOptions."
                "AggressiveOptimization)]\n"
                f"{marker}"
            )
            if new in source:
                continue
            if source.count(old) != 1:
                raise RuntimeError(
                    f"expected one unoptimized attribute for {method} in {path}"
                )
            profile_sources[path] = source.replace(old, new, 1)
            replacements += 1
        for path, rewritten in profile_sources.items():
            source = path.read_text(encoding="utf-8")
            if rewritten != source:
                path.write_text(rewritten, encoding="utf-8")
                changed_files += 1
    return changed_files, replacements


def main() -> int:
    if len(sys.argv) not in (2, 3):
        raise SystemExit(
            "usage: rewrite_recompiled_memory_access.py <generated-directory> "
            "[--reset-optimization-profile]"
        )
    reset_optimization_profile = False
    if len(sys.argv) == 3:
        if sys.argv[2] != "--reset-optimization-profile":
            raise SystemExit(f"unknown option: {sys.argv[2]}")
        reset_optimization_profile = True
    changed_files, replacements = rewrite_tree(
        Path(sys.argv[1]).resolve(),
        reset_optimization_profile=reset_optimization_profile,
    )
    print(
        "Rewrote generated memory access: "
        f"files={changed_files} calls={replacements}"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
