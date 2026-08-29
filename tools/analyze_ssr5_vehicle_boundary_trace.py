#!/usr/bin/env python3
"""Calculate close-car boundary pipeline health from a live GT2 stderr trace."""

from __future__ import annotations

import argparse
import collections
import pathlib
import re
import time


BOUNDARY = re.compile(
    r"^\[Render-Vehicle-Boundary\] .*frame=(?P<frame>\d+) "
    r"poll=(?P<poll>-?\d+) object=(?P<object>\d+) "
    r"model=(?P<model>[0-9a-fA-F]+) edge=(?P<edge>\w+) "
    r"commands=(?P<commands>\d+) .*"
    r"postClip=(?P<visible>\d+)/(?P<rejected>\d+)/(?P<nonfinite>\d+) "
    r"changedVertices=(?P<changed>\d+) "
    r"targetPostClip=(?P<target_visible>\d+)/"
    r"(?P<target_rejected>\d+)/(?P<target_nonfinite>\d+) .*"
    r"fullScissor=(?P<scissor>\d+)/(?P<scissor_total>\d+) "
    r"screen=(?P<minimum_x>-?[\d.]+),(?P<minimum_y>-?[\d.]+)\.\."
    r"(?P<maximum_x>-?[\d.]+),(?P<maximum_y>-?[\d.]+) "
    r"viewZ=(?P<minimum_z>-?[\d.]+)\.\.(?P<maximum_z>-?[\d.]+)"
)
TRANSITION = re.compile(
    r"^\[Render-Vehicle-Boundary-Transition\] .*"
    r"event=(?P<event>[\w-]+) object=(?P<object>\d+) "
    r"model=(?P<model>[0-9a-fA-F]+)"
)
FRUSTUM = re.compile(
    r"^\[GT2-VEHICLE-FRUSTUM\] poll=(?P<poll>-?\d+) .*"
    r"stock=0x(?P<stock>[0-9a-fA-F]+) "
    r"modern=0x(?P<modern>[0-9a-fA-F]+)"
)
GTE_FLAG = re.compile(
    r"^\[GT2-VEHICLE-GTE-FLAG\] poll=(?P<poll>-?\d+) "
    r"object=(?P<object>\d+) model=(?P<model>[0-9a-fA-F]+) "
    r"reads=(?P<reads>\d+) rawFatal=(?P<raw_fatal>\d+) "
    r"screenSaturated=(?P<screen_saturated>\d+) "
    r"recovered=(?P<recovered>\d+) "
    r"preservedFatal=(?P<preserved_fatal>\d+) "
    r"nclipCorrected=(?P<nclip_corrected>\d+)"
)


class Audit:
    def __init__(self, live: bool) -> None:
        self.live = live
        self.boundary_samples = 0
        self.frustum_samples = 0
        self.frustum_modern_nonzero = 0
        self.native_rejected = 0
        self.native_nonfinite = 0
        self.scissor_deficits = 0
        self.gte_policy = False
        self.gte_flag_groups = 0
        self.gte_flag_reads = 0
        self.gte_raw_fatal = 0
        self.gte_screen_saturated = 0
        self.gte_recovered = 0
        self.gte_preserved_fatal = 0
        self.gte_nclip_corrected = 0
        self.groups: dict[tuple[int, str], dict[str, int]] = {}
        self.transitions: collections.Counter[str] = collections.Counter()
        self.latest_gte: dict[int, dict[str, int]] = {}
        self.command_collapses: list[dict[str, int | float | str]] = []

    def consume(self, line: str) -> None:
        if line.startswith("[GT2-Vehicle-GTE-Policy]"):
            self.gte_policy = True
            if self.live:
                print(line, flush=True)
            return
        match = GTE_FLAG.match(line)
        if match:
            self.gte_flag_groups += 1
            self.gte_flag_reads += int(match["reads"])
            self.gte_raw_fatal += int(match["raw_fatal"])
            self.gte_screen_saturated += int(match["screen_saturated"])
            self.gte_recovered += int(match["recovered"])
            self.gte_preserved_fatal += int(match["preserved_fatal"])
            self.gte_nclip_corrected += int(match["nclip_corrected"])
            self.latest_gte[int(match["object"])] = {
                "poll": int(match["poll"]),
                "reads": int(match["reads"]),
                "screen_saturated": int(match["screen_saturated"]),
                "recovered": int(match["recovered"]),
                "preserved_fatal": int(match["preserved_fatal"]),
            }
            if self.live:
                print(
                    "vehicle_gte_edge "
                    f"poll={match['poll']} object={match['object']} "
                    f"model={match['model'].lower()} "
                    f"screen_saturated={match['screen_saturated']} "
                    f"recovered={match['recovered']} "
                    f"preserved_fatal={match['preserved_fatal']} "
                    f"nclip_corrected={match['nclip_corrected']}",
                    flush=True,
                )
            return
        match = FRUSTUM.match(line)
        if match:
            self.frustum_samples += 1
            if int(match["modern"], 16) & 0x003F001E:
                self.frustum_modern_nonzero += 1
            return
        match = TRANSITION.match(line)
        if match:
            self.transitions[match["event"]] += 1
            return
        match = BOUNDARY.match(line)
        if not match:
            return

        self.boundary_samples += 1
        commands = int(match["commands"])
        visible = int(match["visible"])
        target_visible = int(match["target_visible"])
        rejected = int(match["target_rejected"])
        nonfinite = int(match["target_nonfinite"])
        scissor = int(match["scissor"])
        key = (int(match["object"]), match["model"].lower())
        minimum_x = float(match["minimum_x"])
        minimum_y = float(match["minimum_y"])
        maximum_x = float(match["maximum_x"])
        maximum_y = float(match["maximum_y"])
        group = self.groups.setdefault(
            key,
            {
                "samples": 0,
                "minimum": commands,
                "maximum": commands,
                "previous": commands,
                "previous_target_visible": target_visible,
                "changes": 0,
            },
        )
        previous_commands = group["previous"]
        previous_target_visible = group["previous_target_visible"]
        intersects_display = (
            maximum_x > 0.0
            and minimum_x < 320.0
            and maximum_y > 0.0
            and minimum_y < 240.0
        )
        if (
            previous_commands > 0
            and commands * 5 < previous_commands * 3
            and previous_target_visible >= 16
            and target_visible >= 16
            and intersects_display
        ):
            gte = self.latest_gte.get(key[0], {})
            self.command_collapses.append(
                {
                    "poll": int(match["poll"]),
                    "object": key[0],
                    "model": key[1],
                    "previous": previous_commands,
                    "commands": commands,
                    "previous_target": previous_target_visible,
                    "target": target_visible,
                    "minimum_x": minimum_x,
                    "minimum_y": minimum_y,
                    "maximum_x": maximum_x,
                    "maximum_y": maximum_y,
                    "gte_reads": gte.get("reads", 0),
                    "gte_screen": gte.get("screen_saturated", 0),
                    "gte_recovered": gte.get("recovered", 0),
                    "gte_preserved": gte.get("preserved_fatal", 0),
                }
            )
        group["samples"] += 1
        group["minimum"] = min(group["minimum"], commands)
        group["maximum"] = max(group["maximum"], commands)
        if commands != group["previous"]:
            group["changes"] += 1
        group["previous"] = commands
        group["previous_target_visible"] = target_visible

        self.native_rejected += rejected
        self.native_nonfinite += nonfinite
        if scissor != commands:
            self.scissor_deficits += commands - scissor

        if self.live and (rejected or nonfinite or scissor != commands):
            print(
                "pipeline_anomaly "
                f"frame={match['frame']} poll={match['poll']} "
                f"object={key[0]} model={key[1]} commands={commands} "
                f"post_clip={visible}/{rejected}/{nonfinite} "
                f"full_scissor={scissor}/{commands}",
                flush=True,
            )
        if self.live and self.boundary_samples % 300 == 0:
            print(self.summary_line(), flush=True)

    def summary_line(self) -> str:
        return (
            "boundary_health "
            f"samples={self.boundary_samples} groups={len(self.groups)} "
            f"guest_frustum={self.frustum_samples} "
            f"modern_mask_nonzero={self.frustum_modern_nonzero} "
            f"native_rejected={self.native_rejected} "
            f"native_nonfinite={self.native_nonfinite} "
            f"scissor_deficit={self.scissor_deficits} "
            f"gte_policy={'active' if self.gte_policy else 'not-seen'} "
            f"gte_groups={self.gte_flag_groups} "
            f"gte_reads={self.gte_flag_reads} "
            f"gte_screen_saturated={self.gte_screen_saturated} "
            f"gte_recovered={self.gte_recovered} "
            f"gte_preserved_fatal={self.gte_preserved_fatal} "
            f"gte_nclip_corrected={self.gte_nclip_corrected}"
        )

    def report(self) -> None:
        print(self.summary_line())
        print(
            "transitions "
            + " ".join(
                f"{event}={count}"
                for event, count in sorted(self.transitions.items())
            )
        )
        print(f"command_collapses count={len(self.command_collapses)}")
        for collapse in sorted(
            self.command_collapses,
            key=lambda item: item["commands"] / item["previous"],
        )[:12]:
            print(
                "vehicle_command_collapse "
                f"poll={collapse['poll']} object={collapse['object']} "
                f"model={collapse['model']} "
                f"commands={collapse['previous']}->{collapse['commands']} "
                f"target_visible={collapse['previous_target']}->{collapse['target']} "
                f"screen={collapse['minimum_x']:.2f},{collapse['minimum_y']:.2f}.."
                f"{collapse['maximum_x']:.2f},{collapse['maximum_y']:.2f} "
                f"gte_reads={collapse['gte_reads']} "
                f"gte_screen={collapse['gte_screen']} "
                f"gte_recovered={collapse['gte_recovered']} "
                f"gte_preserved={collapse['gte_preserved']}"
            )
        for (object_id, model), group in sorted(self.groups.items()):
            maximum = group["maximum"]
            retention = group["minimum"] / maximum if maximum else 1.0
            print(
                "vehicle_group "
                f"object={object_id} model={model} "
                f"samples={group['samples']} commands={group['minimum']}..{maximum} "
                f"minimum_retention={retention:.3f} "
                f"count_changes={group['changes']}"
            )


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("trace", type=pathlib.Path)
    parser.add_argument(
        "--follow",
        action="store_true",
        help="continue reading appended records until interrupted",
    )
    parser.add_argument(
        "--tail-lines",
        type=int,
        default=0,
        help="analyze only the final N trace lines",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    audit = Audit(live=args.follow)
    with args.trace.open("r", encoding="utf-8", errors="replace") as stream:
        if args.tail_lines > 0:
            for line in collections.deque(stream, maxlen=args.tail_lines):
                audit.consume(line.rstrip("\r\n"))
            audit.report()
            return 0
        while True:
            line = stream.readline()
            if line:
                audit.consume(line.rstrip("\r\n"))
                continue
            if not args.follow:
                break
            time.sleep(0.1)
    audit.report()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
