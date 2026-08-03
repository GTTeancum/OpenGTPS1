#!/usr/bin/env python3
"""Create and apply installer-time GTFS overlay volumes.

GTPATCH.VOL is a normal GTFS archive containing only additions or
replacements. Applying one or more layers materializes a conventional GT2.VOL
for the original game executable; no runtime archive hook is required.
"""

from __future__ import annotations

import argparse
import pathlib
import sys

from gt2_vol import (
    Member,
    members_from_directory,
    members_from_volume,
    write_volume,
)


def create_patch(source: pathlib.Path, output: pathlib.Path) -> None:
    members = members_from_directory(source)
    if not members:
        raise ValueError(f"patch source contains no files: {source}")
    write_volume(output, members)
    print(f"created GTPATCH layer: {output} ({len(members)} members)")


def apply_patches(
    base: pathlib.Path,
    patches: list[pathlib.Path],
    output: pathlib.Path,
) -> None:
    merged: dict[str, Member] = {}
    canonical_names: dict[str, str] = {}
    for member in members_from_volume(base):
        key = member.name.casefold()
        merged[key] = member
        canonical_names[key] = member.name

    total_replacements = 0
    total_additions = 0
    layer_claims: dict[str, pathlib.Path] = {}
    for patch in patches:
        layer_replacements = 0
        layer_additions = 0
        for member in members_from_volume(patch):
            key = member.name.casefold()
            previous_layer = layer_claims.get(key)
            if previous_layer is not None:
                print(
                    f"conflict: {member.name} from {patch} overrides "
                    f"{previous_layer}",
                    file=sys.stderr,
                )
            layer_claims[key] = patch
            if key in merged:
                layer_replacements += 1
                total_replacements += 1
                member = Member(
                    canonical_names[key],
                    member.source,
                    member.offset,
                    member.size,
                    member.date_time,
                )
            else:
                layer_additions += 1
                total_additions += 1
                canonical_names[key] = member.name
            merged[key] = member
        print(
            f"layer {patch}: {layer_additions} additions, "
            f"{layer_replacements} replacements"
        )

    write_volume(output, merged.values())
    print(
        f"materialized native GT2.VOL: {output} "
        f"({len(merged)} members, {total_additions} additions, "
        f"{total_replacements} replacements)"
    )


def main() -> int:
    parser = argparse.ArgumentParser()
    subparsers = parser.add_subparsers(dest="command", required=True)

    create = subparsers.add_parser("create")
    create.add_argument("source", type=pathlib.Path)
    create.add_argument("output", type=pathlib.Path)

    apply = subparsers.add_parser("apply")
    apply.add_argument("base", type=pathlib.Path)
    apply.add_argument("patch", type=pathlib.Path, nargs="+")
    apply.add_argument("--output", required=True, type=pathlib.Path)

    args = parser.parse_args()
    if args.command == "create":
        create_patch(args.source, args.output)
    else:
        apply_patches(args.base, args.patch, args.output)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
