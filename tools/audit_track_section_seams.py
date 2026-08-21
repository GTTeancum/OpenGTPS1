#!/usr/bin/env python3
"""Audit exact track-section boundaries in an OGTWCAP v4 world capture.

The capture records both authored int16 model coordinates and transformed
int32 GTE view coordinates.  Keep those spaces separate: model-space results
answer whether the submitted meshes share authored vertices, while view-space
results expose transform/projection discontinuities introduced after authoring.
"""

from __future__ import annotations

import argparse
import csv
import json
import math
import struct
from collections import Counter, defaultdict
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable


Point = tuple[int, int, int]
Section = tuple[int, int]


@dataclass(frozen=True)
class Triangle:
    section: Section
    vertices: tuple[Point, Point, Point]
    source_index: int


@dataclass(frozen=True)
class BoundaryEdge:
    section: Section
    a: Point
    b: Point

    @property
    def key(self) -> tuple[Point, Point]:
        return canonical_edge(self.a, self.b)


@dataclass(frozen=True)
class Capture:
    paths: tuple[Path, ...]
    frame: int
    poll: int
    coordinate_space: str
    triangles: tuple[Triangle, ...]
    input_track_triangles: int


def u32(data: bytes, offset: int) -> int:
    return struct.unpack_from("<I", data, offset)[0]


def i16(data: bytes, offset: int) -> int:
    return struct.unpack_from("<h", data, offset)[0]


def i32(data: bytes, offset: int) -> int:
    return struct.unpack_from("<i", data, offset)[0]


def u64(data: bytes, offset: int) -> int:
    return struct.unpack_from("<Q", data, offset)[0]


def canonical_edge(a: Point, b: Point) -> tuple[Point, Point]:
    return (a, b) if a <= b else (b, a)


def read_capture(path: Path, coordinate_space: str) -> Capture:
    data = path.read_bytes()
    if len(data) < 160 or data[:8] != b"OGTWCAP\0":
        raise ValueError(f"{path}: not an OGTWCAP capture")
    version = u32(data, 8)
    header_size = u32(data, 12)
    triangle_count = u32(data, 44)
    stride = u32(data, 48)
    triangle_offset = u64(data, 60)
    expected_header = 128 if version == 1 else 160
    expected_stride = 224 if version == 4 else 212 if version == 3 else 176
    if (
        version not in (1, 2, 3, 4)
        or header_size != expected_header
        or stride != expected_stride
    ):
        raise ValueError(
            f"{path}: expected OGTWCAP v1-v4 geometry, got "
            f"v{version}/{header_size}/{stride}"
        )
    required = triangle_offset + triangle_count * stride
    if required > len(data):
        raise ValueError(f"{path}: truncated triangle data")

    projection_x = i32(data, 128) if version >= 2 else 0
    projection_y = i32(data, 132) if version >= 2 else 0
    projection_plane = u32(data, 136) if version >= 2 else 0
    draw_x = i32(data, 140) if version >= 2 else 0
    draw_y = i32(data, 144) if version >= 2 else 0
    triangles: list[Triangle] = []
    input_track_triangles = 0
    vertex_stride = 56 if version == 4 else 52 if version == 3 else 40
    for index in range(triangle_count):
        base = triangle_offset + index * stride
        if u32(data, base + 32) != 1:
            continue
        input_track_triangles += 1
        if (
            coordinate_space == "view"
            and
            version >= 3
            and (
                i16(data, base + 44) != draw_x
                or i16(data, base + 46) != draw_y
            )
        ):
            continue
        points: list[Point] = []
        main_projection = True
        complete = True
        for vertex_index in range(3):
            vertex = base + 56 + vertex_index * vertex_stride
            complete = complete and (data[vertex + 19] & 1) != 0
            if coordinate_space == "view" and version >= 3:
                main_projection = main_projection and (
                    i32(data, vertex + 40) == projection_x
                    and i32(data, vertex + 44) == projection_y
                    and u32(data, vertex + 48) == projection_plane
                )
            if coordinate_space == "model":
                points.append(
                    (
                        i16(data, vertex + 20),
                        i16(data, vertex + 22),
                        i16(data, vertex + 24),
                    )
                )
            else:
                points.append(
                    (
                        i32(data, vertex + 28),
                        i32(data, vertex + 32),
                        i32(data, vertex + 36),
                    )
                )
        if not complete or not main_projection:
            continue
        triangles.append(
            Triangle(
                section=(u32(data, base + 36), u32(data, base + 40)),
                vertices=(points[0], points[1], points[2]),
                source_index=index,
            )
        )
    return Capture(
        paths=(path,),
        frame=u64(data, 16),
        poll=i32(data, 24),
        coordinate_space=coordinate_space,
        triangles=tuple(triangles),
        input_track_triangles=input_track_triangles,
    )


def merge_captures(captures: list[Capture]) -> Capture:
    if not captures:
        raise ValueError("at least one capture is required")
    coordinate_space = captures[0].coordinate_space
    if any(capture.coordinate_space != coordinate_space for capture in captures):
        raise ValueError("cannot merge different coordinate spaces")
    if len(captures) > 1 and coordinate_space != "model":
        raise ValueError(
            "multiple captures can only be merged in authored model space"
        )
    return Capture(
        paths=tuple(
            path for capture in captures for path in capture.paths
        ),
        frame=captures[0].frame,
        poll=captures[0].poll,
        coordinate_space=coordinate_space,
        triangles=tuple(
            triangle
            for capture in captures
            for triangle in capture.triangles
        ),
        input_track_triangles=sum(
            capture.input_track_triangles for capture in captures
        ),
    )


def unique_triangles(
    triangles: Iterable[Triangle],
) -> tuple[list[Triangle], int]:
    result: list[Triangle] = []
    seen: set[tuple[Section, tuple[Point, Point, Point]]] = set()
    duplicates = 0
    for triangle in triangles:
        key = (triangle.section, tuple(sorted(triangle.vertices)))
        if key in seen:
            duplicates += 1
            continue
        seen.add(key)
        result.append(triangle)
    return result, duplicates


def boundary_edges(
    triangles: Iterable[Triangle],
) -> tuple[list[BoundaryEdge], int, int]:
    counts: dict[Section, Counter[tuple[Point, Point]]] = defaultdict(Counter)
    for triangle in triangles:
        a, b, c = triangle.vertices
        for left, right in ((a, b), (b, c), (c, a)):
            if left != right:
                counts[triangle.section][canonical_edge(left, right)] += 1
    result: list[BoundaryEdge] = []
    manifold = 0
    nonmanifold = 0
    for section, edges in counts.items():
        for (a, b), count in edges.items():
            if count == 1:
                result.append(BoundaryEdge(section, a, b))
            elif count == 2:
                manifold += 1
            else:
                nonmanifold += 1
    return result, manifold, nonmanifold


def subtract(a: Point, b: Point) -> Point:
    return (a[0] - b[0], a[1] - b[1], a[2] - b[2])


def cross(a: Point, b: Point) -> Point:
    return (
        a[1] * b[2] - a[2] * b[1],
        a[2] * b[0] - a[0] * b[2],
        a[0] * b[1] - a[1] * b[0],
    )


def collinear(left: BoundaryEdge, right: BoundaryEdge) -> bool:
    ld = subtract(left.b, left.a)
    rd = subtract(right.b, right.a)
    return cross(ld, rd) == (0, 0, 0) and cross(
        ld, subtract(right.a, left.a)
    ) == (0, 0, 0)


def dominant_axis(edge: BoundaryEdge) -> int:
    delta = subtract(edge.b, edge.a)
    return max(range(3), key=lambda axis: abs(delta[axis]))


def covered_by_other_sections(
    edge: BoundaryEdge,
    edges: list[BoundaryEdge],
) -> tuple[bool, set[Section]]:
    axis = dominant_axis(edge)
    low, high = sorted((edge.a[axis], edge.b[axis]))
    intervals: list[tuple[int, int, Section]] = []
    for candidate in edges:
        if candidate.section == edge.section or not collinear(edge, candidate):
            continue
        candidate_low, candidate_high = sorted(
            (candidate.a[axis], candidate.b[axis])
        )
        overlap_low = max(low, candidate_low)
        overlap_high = min(high, candidate_high)
        if overlap_low < overlap_high:
            intervals.append(
                (overlap_low, overlap_high, candidate.section)
            )
    intervals.sort()
    cursor = low
    sections: set[Section] = set()
    for interval_low, interval_high, section in intervals:
        if interval_low > cursor:
            break
        if interval_high > cursor:
            cursor = interval_high
            sections.add(section)
        if cursor >= high:
            return True, sections
    return False, sections


def dot(a: tuple[float, float, float], b: tuple[float, float, float]) -> float:
    return a[0] * b[0] + a[1] * b[1] + a[2] * b[2]


def segment_distance_squared(left: BoundaryEdge, right: BoundaryEdge) -> float:
    # Closest points between two finite 3D segments, from the standard
    # clamped two-parameter solution. Exact matches are handled separately.
    p = tuple(float(value) for value in left.a)
    q = tuple(float(value) for value in right.a)
    d1 = tuple(float(value) for value in subtract(left.b, left.a))
    d2 = tuple(float(value) for value in subtract(right.b, right.a))
    r = tuple(p[axis] - q[axis] for axis in range(3))
    a = dot(d1, d1)
    e = dot(d2, d2)
    epsilon = 1.0e-12
    if a <= epsilon and e <= epsilon:
        return dot(r, r)
    if a <= epsilon:
        s = 0.0
        t = min(1.0, max(0.0, dot(d2, r) / e))
    else:
        c = dot(d1, r)
        if e <= epsilon:
            t = 0.0
            s = min(1.0, max(0.0, -c / a))
        else:
            b = dot(d1, d2)
            denominator = a * e - b * b
            s = (
                min(1.0, max(0.0, (b * dot(d2, r) - c * e) / denominator))
                if abs(denominator) > epsilon
                else 0.0
            )
            t = (b * s + dot(d2, r)) / e
            if t < 0.0:
                t = 0.0
                s = min(1.0, max(0.0, -c / a))
            elif t > 1.0:
                t = 1.0
                s = min(1.0, max(0.0, (b - c) / a))
    delta = tuple(
        r[axis] + d1[axis] * s - d2[axis] * t for axis in range(3)
    )
    return dot(delta, delta)


def section_label(section: Section) -> str:
    return f"{section[0]}:0x{section[1]:08X}"


def audit(
    capture: Capture,
    calculate_nearest: bool = True,
) -> tuple[dict[str, object], list[dict[str, object]]]:
    triangles, duplicate_triangles = unique_triangles(capture.triangles)
    edges, manifold_edges, nonmanifold_edges = boundary_edges(triangles)
    exact_map: dict[tuple[Point, Point], list[BoundaryEdge]] = defaultdict(list)
    for edge in edges:
        exact_map[edge.key].append(edge)

    classifications: list[dict[str, object]] = []
    unmatched: list[tuple[int, BoundaryEdge]] = []
    exact_count = 0
    collinear_count = 0
    for index, edge in enumerate(edges):
        exact_sections = {
            candidate.section
            for candidate in exact_map[edge.key]
            if candidate.section != edge.section
        }
        if exact_sections:
            classification = "exact"
            matches = exact_sections
            exact_count += 1
        else:
            covered, matches = covered_by_other_sections(edge, edges)
            if covered:
                classification = "collinear_partition"
                collinear_count += 1
            else:
                classification = "unmatched"
                unmatched.append((index, edge))
        classifications.append(
            {
                "section": section_label(edge.section),
                "object_id": edge.section[0],
                "model_pointer": f"0x{edge.section[1]:08X}",
                "ax": edge.a[0],
                "ay": edge.a[1],
                "az": edge.a[2],
                "bx": edge.b[0],
                "by": edge.b[1],
                "bz": edge.b[2],
                "classification": classification,
                "matching_sections": ";".join(
                    sorted(section_label(section) for section in matches)
                ),
                "nearest_other_distance": "",
                "nearest_other_section": "",
            }
        )

    near_thresholds = (1.0, 4.0, 16.0, 64.0)
    near_counts = {threshold: 0 for threshold in near_thresholds}
    if calculate_nearest:
        for index, edge in unmatched:
            nearest_squared = math.inf
            nearest_section: Section | None = None
            for candidate in edges:
                if candidate.section == edge.section:
                    continue
                distance_squared = segment_distance_squared(edge, candidate)
                if distance_squared < nearest_squared:
                    nearest_squared = distance_squared
                    nearest_section = candidate.section
            distance = (
                math.sqrt(nearest_squared)
                if math.isfinite(nearest_squared)
                else math.inf
            )
            classifications[index]["nearest_other_distance"] = (
                f"{distance:.6f}" if math.isfinite(distance) else ""
            )
            classifications[index]["nearest_other_section"] = (
                section_label(nearest_section) if nearest_section else ""
            )
            for threshold in near_thresholds:
                if distance <= threshold:
                    near_counts[threshold] += 1

    boundary_vertices: dict[Point, set[Section]] = defaultdict(set)
    for edge in edges:
        boundary_vertices[edge.a].add(edge.section)
        boundary_vertices[edge.b].add(edge.section)
    shared_vertex_groups = sum(
        1 for sections in boundary_vertices.values() if len(sections) > 1
    )
    sections = sorted({triangle.section for triangle in triangles})
    summary: dict[str, object] = {
        "capture": str(capture.paths[0]) if len(capture.paths) == 1 else "",
        "captures": [str(path) for path in capture.paths],
        "frame": capture.frame,
        "poll": capture.poll,
        "coordinate_space": capture.coordinate_space,
        "sections": len(sections),
        "section_labels": [section_label(section) for section in sections],
        "input_track_triangles": capture.input_track_triangles,
        "main_track_triangles": len(capture.triangles),
        "unique_geometric_triangles": len(triangles),
        "duplicate_geometric_triangles": duplicate_triangles,
        "boundary_edges": len(edges),
        "manifold_edges": manifold_edges,
        "nonmanifold_edges": nonmanifold_edges,
        "cross_section_exact_edges": exact_count,
        "cross_section_collinear_partition_edges": collinear_count,
        "unmatched_outer_or_mismatched_edges": len(unmatched),
        "nearest_distance_analysis": calculate_nearest,
        "cross_section_shared_boundary_vertices": shared_vertex_groups,
        "unmatched_within_1_unit": near_counts[1.0],
        "unmatched_within_4_units": near_counts[4.0],
        "unmatched_within_16_units": near_counts[16.0],
        "unmatched_within_64_units": near_counts[64.0],
    }
    return summary, classifications


def write_obj(path: Path, capture: Capture) -> None:
    triangles, _ = unique_triangles(capture.triangles)
    by_section: dict[Section, list[Triangle]] = defaultdict(list)
    for triangle in triangles:
        by_section[triangle.section].append(triangle)
    lines = [
        f"# Exact {capture.coordinate_space}-space track geometry exported "
        "from OGTWCAP v4",
        f"# captures={len(capture.paths)} frame={capture.frame} "
        f"poll={capture.poll}",
    ]
    vertex_index = 1
    for section in sorted(by_section):
        lines.append(f"o track_{section[0]}_{section[1]:08x}")
        indices: dict[Point, int] = {}
        for triangle in by_section[section]:
            for point in triangle.vertices:
                if point not in indices:
                    indices[point] = vertex_index
                    vertex_index += 1
                    lines.append(f"v {point[0]} {point[1]} {point[2]}")
        for triangle in by_section[section]:
            a, b, c = (indices[point] for point in triangle.vertices)
            lines.append(f"f {a} {b} {c}")
    path.write_text("\n".join(lines) + "\n", encoding="utf-8", newline="\n")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("capture", type=Path, nargs="+")
    parser.add_argument("--summary", type=Path)
    parser.add_argument("--edges", type=Path)
    parser.add_argument("--obj", type=Path)
    parser.add_argument(
        "--space",
        choices=("model", "view"),
        default="model",
        help=(
            "model audits authored mesh coordinates; view audits transformed "
            "GTE coordinates (default: model)"
        ),
    )
    parser.add_argument(
        "--skip-nearest",
        action="store_true",
        help="skip the quadratic nearest-unmatched-edge diagnostic",
    )
    args = parser.parse_args()

    capture = merge_captures(
        [
            read_capture(path.resolve(), args.space)
            for path in args.capture
        ]
    )
    summary, classifications = audit(
        capture,
        calculate_nearest=not args.skip_nearest,
    )
    if args.summary:
        args.summary.parent.mkdir(parents=True, exist_ok=True)
        args.summary.write_text(
            json.dumps(summary, indent=2) + "\n",
            encoding="utf-8",
            newline="\n",
        )
    if args.edges:
        args.edges.parent.mkdir(parents=True, exist_ok=True)
        with args.edges.open("w", newline="", encoding="utf-8") as output:
            writer = csv.DictWriter(output, fieldnames=list(classifications[0]))
            writer.writeheader()
            writer.writerows(classifications)
    if args.obj:
        args.obj.parent.mkdir(parents=True, exist_ok=True)
        write_obj(args.obj, capture)
    print(json.dumps(summary, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
