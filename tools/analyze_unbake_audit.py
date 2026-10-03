#!/usr/bin/env python3
"""Analyze non-destructive [Lighting-Unbake-Audit] runtime records.

This tool never writes lighting.shader and never enables bakedGain or
removeShadowOverlay. It only groups exact resident-source records that share the
same source texture tile/palette and reports dark-vs-bright authored-color
candidates for later visual/source verification.
"""
from __future__ import annotations
import argparse
import json
import math
import re
import sys
from pathlib import Path

PREFIX = "[Lighting-Unbake-Audit] "
HEX_FIELDS = {"primitiveKey", "sourceMeshKey", "sourcePrimitiveAddress", "model", "flags"}
OPTIONAL_HEX_FIELDS = {"residentContentKey", "cameraTransform"}
INT_FIELDS = {"frame", "poll", "object", "page", "clut"}
OPTIONAL_INT_FIELDS = {"auditStage", "seenFrames", "firstFrame", "spanFrames", "viewBuckets"}
OPTIONAL_FLOAT_FIELDS = {"maxViewAngle", "maxWorldCentroidDrift", "maxWorldPlaneOffset", "maxWorldPlaneAngle"}


def parse_line(line: str):
    pos = line.find(PREFIX)
    if pos < 0:
        return None
    text = line[pos + len(PREFIX):].strip()
    if text.startswith("truncated="):
        return {"truncated": True, "raw": text}
    fields = {}
    for token in text.split():
        if "=" not in token:
            continue
        key, value = token.split("=", 1)
        fields[key] = value
    required = {
        "track", "frame", "poll", "surface", "primitiveKey", "sourceMeshKey",
        "sourcePrimitiveAddress", "object", "model", "page", "clut", "flags",
        "normalUp", "uv", "rgb",
    }
    if not required.issubset(fields):
        return None
    try:
        result = {"track": fields["track"], "surface": fields["surface"]}
        for key in HEX_FIELDS:
            result[key] = int(fields[key], 16)
        for key in OPTIONAL_HEX_FIELDS:
            result[key] = int(fields[key], 16) if key in fields else None
        for key in INT_FIELDS:
            result[key] = int(fields[key], 10)
        result["normalUp"] = float(fields["normalUp"])
        uv = []
        for pair in fields["uv"].split(";"):
            u, v = pair.split(",", 1)
            uv.append((float(u), float(v)))
        if len(uv) != 3 or not all(math.isfinite(x) for p in uv for x in p):
            return None
        result["uv"] = uv
        rgb = tuple(int(x) for x in fields["rgb"].split(","))
        if len(rgb) != 3 or any(x < 0 or x > 255 for x in rgb):
            return None
        result["rgb"] = rgb
        for key in OPTIONAL_INT_FIELDS:
            result[key] = int(fields[key], 10) if key in fields else None
        for key in OPTIONAL_FLOAT_FIELDS:
            result[key] = float(fields[key]) if key in fields else None
        if not math.isfinite(result["normalUp"]) or any(
            result[key] is not None and not math.isfinite(result[key])
            for key in OPTIONAL_FLOAT_FIELDS
        ):
            return None
        if result["auditStage"] is not None and result["auditStage"] not in (0, 1, 2):
            return None
        if any(result[key] is not None and result[key] < 0 for key in OPTIONAL_INT_FIELDS):
            return None
        if any(result[key] is not None and result[key] < 0 for key in OPTIONAL_FLOAT_FIELDS):
            return None
        return result
    except (ValueError, OverflowError):
        return None


def identity(record):
    return (
        record["track"], record["primitiveKey"], record["sourceMeshKey"],
        record["sourcePrimitiveAddress"],
    )


def group_key(record):
    # Same source UV triangle + palette + surface class, matching the
    # conservative L01 inverse-audit grouping but retaining exact provenance.
    uv = tuple(sorted((round(u, 4), round(v, 4)) for u, v in record["uv"]))
    return (record["track"], record["surface"], record["page"], record["clut"], uv)


def analyze(records):
    variants = {}
    truncated = False
    raw_records = 0
    for record in records:
        if record is None:
            continue
        if record.get("truncated"):
            truncated = True
            continue
        raw_records += 1
        key = identity(record)
        signature = (record["rgb"], record["page"], record["clut"], tuple(record["uv"]),
                     record["surface"], record.get("residentContentKey"))
        bucket = variants.setdefault(key, {}).setdefault(signature, {
            "record": record,
            "observations": 0,
            "maxStage": -1,
            "seenFrames": 0,
            "firstFrame": None,
            "spanFrames": 0,
            "viewBuckets": 0,
            "maxViewAngle": 0.0,
            "maxWorldCentroidDrift": None,
            "maxWorldPlaneOffset": None,
            "maxWorldPlaneAngle": None,
        })
        bucket["observations"] += 1
        stage = record.get("auditStage")
        seen = record.get("seenFrames")
        first = record.get("firstFrame")
        span = record.get("spanFrames")
        view_buckets = record.get("viewBuckets")
        max_view_angle = record.get("maxViewAngle")
        if stage is not None:
            bucket["maxStage"] = max(bucket["maxStage"], stage)
        if seen is not None:
            bucket["seenFrames"] = max(bucket["seenFrames"], seen)
        if first is not None:
            bucket["firstFrame"] = first if bucket["firstFrame"] is None else min(bucket["firstFrame"], first)
        if span is not None:
            bucket["spanFrames"] = max(bucket["spanFrames"], span)
        if view_buckets is not None:
            bucket["viewBuckets"] = max(bucket["viewBuckets"], view_buckets)
        if max_view_angle is not None:
            bucket["maxViewAngle"] = max(bucket["maxViewAngle"], max_view_angle)
        for metric in ("maxWorldCentroidDrift", "maxWorldPlaneOffset", "maxWorldPlaneAngle"):
            value = record.get(metric)
            if value is not None:
                bucket[metric] = value if bucket[metric] is None else max(bucket[metric], value)

    ambiguous = {key: values for key, values in variants.items() if len(values) > 1}
    unique_buckets = {key: next(iter(values.values())) for key, values in variants.items() if len(values) == 1}
    temporal_missing = []
    view_missing = []
    content_missing = []
    geometry_missing = []
    qualified = {}
    temporally_qualified = 0
    view_qualified = 0
    content_qualified = 0
    geometry_qualified = 0
    strongly_qualified = 0
    for key, bucket in unique_buckets.items():
        short_ok = bucket["maxStage"] >= 1 and bucket["seenFrames"] >= 3 and bucket["spanFrames"] >= 30
        long_ok = bucket["maxStage"] >= 2 and bucket["seenFrames"] >= 8 and bucket["spanFrames"] >= 180
        content_ok = bool(bucket["record"].get("residentContentKey"))
        view_ok = bucket["viewBuckets"] >= 2 and bucket["maxViewAngle"] >= 8.0
        strong_view = bucket["viewBuckets"] >= 3 and bucket["maxViewAngle"] >= 20.0
        geometry_present = all(bucket[m] is not None for m in ("maxWorldCentroidDrift", "maxWorldPlaneOffset", "maxWorldPlaneAngle"))
        geometry_ok = geometry_present and bucket["maxWorldCentroidDrift"] <= 0.10 and bucket["maxWorldPlaneOffset"] <= 0.05 and bucket["maxWorldPlaneAngle"] <= 5.0
        bucket["temporalQualified"] = short_ok
        bucket["strongTemporalQualified"] = long_ok
        bucket["sourceContentQualified"] = content_ok
        bucket["crossViewQualified"] = view_ok
        bucket["strongCrossViewQualified"] = strong_view
        bucket["worldGeometryQualified"] = geometry_ok
        temporally_qualified += int(short_ok)
        content_qualified += int(content_ok)
        view_qualified += int(view_ok)
        geometry_qualified += int(geometry_ok)
        if not short_ok:
            temporal_missing.append((key, bucket))
        if not content_ok:
            content_missing.append((key, bucket))
        if not view_ok:
            view_missing.append((key, bucket))
        if not geometry_ok:
            geometry_missing.append((key, bucket))
        if short_ok and content_ok and view_ok and geometry_ok:
            qualified[key] = bucket
            strongly_qualified += int(long_ok and strong_view)

    groups = {}
    for bucket in qualified.values():
        record = bucket["record"]
        groups.setdefault(group_key(record), []).append(bucket)

    candidate_groups = []
    candidates = []
    for key, group in sorted(groups.items(), key=lambda item: repr(item[0])):
        if len(group) < 8:
            continue
        ordered = sorted(group, key=lambda b: (sum(b["record"]["rgb"]), identity(b["record"])))
        lit = ordered[max(0, int(len(ordered) * .9) - 1)]["record"]["rgb"]
        lit_luma = sum(lit)
        if lit_luma < 96:
            continue
        dark = [b for b in group if .25 < sum(b["record"]["rgb"]) / lit_luma < .74]
        bright = [b for b in group if .92 < sum(b["record"]["rgb"]) / lit_luma < 1.08]
        accepted_dark = []
        for bucket in dark:
            record = bucket["record"]
            ratios = [a / max(b, 1) for a, b in zip(record["rgb"], lit)]
            if max(ratios) - min(ratios) <= .22:
                accepted_dark.append(bucket)
        if len(accepted_dark) < 3 or len(bright) < 3:
            continue
        track, surface, page, clut, uv = key
        group_index = len(candidate_groups)
        candidate_groups.append({
            "track": track,
            "surface": surface,
            "page": page,
            "clut": clut,
            "uv": [list(p) for p in uv],
            "records": len(group),
            "dark": len(accepted_dark),
            "bright": len(bright),
            "strongTemporalRecords": sum(1 for b in group if b["strongTemporalQualified"]),
            "litColor": list(lit),
        })
        for bucket in accepted_dark:
            record = bucket["record"]
            ratio = sum(record["rgb"]) / lit_luma
            candidates.append({
                "group": group_index,
                "track": record["track"],
                "surface": record["surface"],
                "primitiveKey": f"0x{record['primitiveKey']:016x}",
                "sourceMeshKey": f"0x{record['sourceMeshKey']:016x}",
                "sourcePrimitiveAddress": f"0x{record['sourcePrimitiveAddress']:08x}",
                "residentContentKey": f"0x{record['residentContentKey']:016x}",
                "page": record["page"],
                "clut": record["clut"],
                "uv": [list(p) for p in record["uv"]],
                "rgb": list(record["rgb"]),
                "litColor": list(lit),
                "relativeLuma": ratio,
                "normalUp": record["normalUp"],
                "object": record["object"],
                "model": f"0x{record['model']:08x}",
                "firstFrame": bucket["firstFrame"],
                "seenFrames": bucket["seenFrames"],
                "spanFrames": bucket["spanFrames"],
                "auditStage": bucket["maxStage"],
                "viewBuckets": bucket["viewBuckets"],
                "maxViewAngle": bucket["maxViewAngle"],
                "strongTemporalEvidence": bucket["strongTemporalQualified"],
                "crossViewEvidence": bucket["crossViewQualified"],
                "strongCrossViewEvidence": bucket["strongCrossViewQualified"],
                "maxWorldCentroidDrift": bucket["maxWorldCentroidDrift"],
                "maxWorldPlaneOffset": bucket["maxWorldPlaneOffset"],
                "maxWorldPlaneAngle": bucket["maxWorldPlaneAngle"],
                "worldGeometryEvidence": bucket["worldGeometryQualified"],
            })
    candidates.sort(key=lambda c: (c["track"], c["group"], c["relativeLuma"], c["sourcePrimitiveAddress"], c["primitiveKey"]))
    return {
        "format": "OpenGT-unbake-audit-candidates-v4",
        "destructiveRulesGenerated": False,
        "truncatedInput": truncated,
        "rawRecords": raw_records,
        "identities": len(variants),
        "records": len(unique_buckets),
        "temporallyQualifiedRecords": temporally_qualified,
        "sourceContentQualifiedRecords": content_qualified,
        "crossViewQualifiedRecords": view_qualified,
        "worldGeometryQualifiedRecords": geometry_qualified,
        "fullyQualifiedRecords": len(qualified),
        "strongTemporalAndViewRecords": strongly_qualified,
        "temporalEvidenceMissingCount": len(temporal_missing),
        "temporalEvidenceMissingTruncated": len(temporal_missing) > 1024,
        "temporalEvidenceMissing": [
            {
                "track": key[0],
                "primitiveKey": f"0x{key[1]:016x}",
                "sourceMeshKey": f"0x{key[2]:016x}",
                "sourcePrimitiveAddress": f"0x{key[3]:08x}",
                "maxStage": bucket["maxStage"],
                "seenFrames": bucket["seenFrames"],
                "spanFrames": bucket["spanFrames"],
            }
            for key, bucket in sorted(temporal_missing)[:1024]
        ],
        "sourceContentEvidenceMissingCount": len(content_missing),
        "sourceContentEvidenceMissingTruncated": len(content_missing) > 1024,
        "sourceContentEvidenceMissing": [
            {
                "track": key[0],
                "primitiveKey": f"0x{key[1]:016x}",
                "sourceMeshKey": f"0x{key[2]:016x}",
                "sourcePrimitiveAddress": f"0x{key[3]:08x}",
            }
            for key, bucket in sorted(content_missing)[:1024]
        ],
        "crossViewEvidenceMissingCount": len(view_missing),
        "crossViewEvidenceMissingTruncated": len(view_missing) > 1024,
        "crossViewEvidenceMissing": [
            {
                "track": key[0],
                "primitiveKey": f"0x{key[1]:016x}",
                "sourceMeshKey": f"0x{key[2]:016x}",
                "sourcePrimitiveAddress": f"0x{key[3]:08x}",
                "viewBuckets": bucket["viewBuckets"],
                "maxViewAngle": bucket["maxViewAngle"],
            }
            for key, bucket in sorted(view_missing)[:1024]
        ],
        "worldGeometryEvidenceMissingCount": len(geometry_missing),
        "worldGeometryEvidenceMissingTruncated": len(geometry_missing) > 1024,
        "worldGeometryEvidenceMissing": [
            {
                "track": key[0],
                "primitiveKey": f"0x{key[1]:016x}",
                "sourceMeshKey": f"0x{key[2]:016x}",
                "sourcePrimitiveAddress": f"0x{key[3]:08x}",
                "maxWorldCentroidDrift": bucket["maxWorldCentroidDrift"],
                "maxWorldPlaneOffset": bucket["maxWorldPlaneOffset"],
                "maxWorldPlaneAngle": bucket["maxWorldPlaneAngle"],
            }
            for key, bucket in sorted(geometry_missing)[:1024]
        ],
        "ambiguousIdentitiesCount": len(ambiguous),
        "ambiguousIdentitiesTruncated": len(ambiguous) > 1024,
        "ambiguousIdentities": [
            {
                "track": key[0],
                "primitiveKey": f"0x{key[1]:016x}",
                "sourceMeshKey": f"0x{key[2]:016x}",
                "sourcePrimitiveAddress": f"0x{key[3]:08x}",
                "variants": len(values),
                "colors": [list(signature[0]) for signature in sorted(values, key=repr)],
                "residentContentKeys": sorted({
                    f"0x{signature[5]:016x}" if signature[5] else None
                    for signature in values
                }, key=lambda value: "" if value is None else value),
            }
            for key, values in sorted(ambiguous.items())[:1024]
        ],
        "groups": len(groups),
        "candidateGroups": candidate_groups,
        "candidates": candidates,
        "warning": "Candidates require repeated cross-frame evidence, immutable resident-content provenance, cross-view confirmation, and stable reconstructed world geometry. They remain authored-color contrast evidence only. Visually/source-verify each exact primitive before enabling bakedGain or removeShadowOverlay.",
    }


def self_test():
    def line(i, rgb, *, page=12, clut=32289, uv="0.000,0.000;16.000,0.000;0.000,16.000", track="seattle",
             stage=1, seen=3, span=30, frame=None, first=None, content=None, view_buckets=2, view_angle=12.0,
             centroid_drift=0.002, plane_offset=0.001, plane_angle=0.25):
        frame = 100 + i if frame is None else frame
        first = frame - span if first is None else first
        content = 0x9000000000000000 + i if content is None else content
        return (f"noise {PREFIX}track={track} frame={frame} poll={200+i} surface=road "
                f"primitiveKey=0x{0x1000+i:016x} sourceMeshKey=0x{0x8000000000000040+i:016x} "
                f"sourcePrimitiveAddress=0x{0x80100000+i*16:08x} residentContentKey=0x{content:016x} "
                f"object={i} model=0x800b0000 "
                f"page={page} clut={clut} flags=0x00000051 normalUp=0.995000 uv={uv} "
                f"rgb={rgb[0]},{rgb[1]},{rgb[2]} auditStage={stage} seenFrames={seen} "
                f"firstFrame={first} spanFrames={span} viewBuckets={view_buckets} maxViewAngle={view_angle:.3f} "
                f"maxWorldCentroidDrift={centroid_drift:.6f} maxWorldPlaneOffset={plane_offset:.6f} "
                f"maxWorldPlaneAngle={plane_angle:.3f} cameraTransform=0x{0x7000000000000000+frame:016x}")
    lines = []
    for i in range(4):
        lines.append(line(i, (60, 55, 45)))
    for i in range(4, 10):
        lines.append(line(i, (120, 110, 90)))
    # A colored-tint outlier is dark in luma but must not pass the neutral
    # shadow-ratio guard.
    lines.append(line(10, (40, 70, 40)))
    # Insufficient unrelated group.
    lines += [line(20+i, (50, 50, 50), page=13) for i in range(3)]
    parsed = [parse_line(x) for x in lines]
    result = analyze(parsed)
    assert result["rawRecords"] == len(lines)
    assert result["records"] == len(lines)
    assert result["temporallyQualifiedRecords"] == len(lines)
    assert result["crossViewQualifiedRecords"] == len(lines)
    assert result["sourceContentQualifiedRecords"] == len(lines)
    assert result["worldGeometryQualifiedRecords"] == len(lines)
    assert result["fullyQualifiedRecords"] == len(lines)
    assert len(result["candidateGroups"]) == 1
    assert len(result["candidates"]) == 4
    assert all(c["track"] == "seattle" for c in result["candidates"])
    assert all(c["auditStage"] == 1 and c["seenFrames"] == 3 and c["spanFrames"] == 30 for c in result["candidates"])
    assert all(c["primitiveKey"].startswith("0x") and c["sourceMeshKey"].startswith("0x") for c in result["candidates"])
    # Duplicate exact confirmation cannot inflate group support.
    dup = analyze(parsed + [parsed[0]])
    assert dup["records"] == result["records"] and len(dup["candidates"]) == 4
    # First-sighting-only records are preserved for diagnostics but cannot
    # become candidates until the runtime emits temporal confirmation.
    transient = [parse_line(line(i, (60,55,45) if i < 4 else (120,110,90), stage=0, seen=1, span=0)) for i in range(10)]
    transient_result = analyze(transient)
    assert transient_result["temporallyQualifiedRecords"] == 0
    assert transient_result["fullyQualifiedRecords"] == 0
    assert len(transient_result["temporalEvidenceMissing"]) == 10
    assert transient_result["candidates"] == []
    # Legacy L14 lines remain parseable but are intentionally ineligible for
    # v2 candidates because they contain no cross-frame confirmation fields.
    legacy = re.sub(r" residentContentKey=\S+", "", lines[0])
    legacy = re.sub(r" auditStage=.*$", "", legacy)
    legacy_result = analyze([parse_line(legacy)])
    assert legacy_result["records"] == 1 and legacy_result["fullyQualifiedRecords"] == 0
    assert legacy_result["sourceContentEvidenceMissingCount"] == 1
    assert legacy_result["worldGeometryEvidenceMissingCount"] == 1
    # Long-span confirmation is exposed separately but is not required for a
    # candidate; it raises confidence for later human/source verification.
    strong = analyze([parse_line(line(i, (60,55,45), stage=2, seen=8, span=180, frame=300+i)) for i in range(4)] +
                     [parse_line(line(i+4, (120,110,90), stage=2, seen=8, span=180, frame=304+i)) for i in range(6)])
    assert strong["strongTemporalAndViewRecords"] == 0
    # Same exact destructive identity with another authored color is an LOD/
    # material ambiguity. It must be removed from candidate output regardless
    # of how strong either temporal observation is.
    variant_line = line(0, (118, 108, 88), stage=2, seen=8, span=180, frame=300)
    variant = analyze(parsed + [parse_line(variant_line)])
    assert len(variant["ambiguousIdentities"]) == 1
    assert len(variant["candidates"]) == 3
    narrow = [parse_line(line(i, (60,55,45) if i < 4 else (120,110,90), view_buckets=1, view_angle=2.0)) for i in range(10)]
    narrow_result = analyze(narrow)
    assert narrow_result["temporallyQualifiedRecords"] == 10
    assert narrow_result["crossViewQualifiedRecords"] == 0
    assert narrow_result["fullyQualifiedRecords"] == 0 and narrow_result["candidates"] == []
    # Same destructive identity and authored color but a different immutable
    # resident definition is a reload/content ambiguity, even if addresses match.
    content_variant = line(0, (60,55,45), content=0x9fffffffffffffff)
    content_result = analyze(parsed + [parse_line(content_variant)])
    assert content_result["ambiguousIdentitiesCount"] == 1
    assert len(content_result["candidates"]) == 3
    strong_view = analyze([parse_line(line(i, (60,55,45), stage=2, seen=8, span=180, frame=300+i, view_buckets=3, view_angle=25.0)) for i in range(4)] +
                          [parse_line(line(i+4, (120,110,90), stage=2, seen=8, span=180, frame=304+i, view_buckets=3, view_angle=25.0)) for i in range(6)])
    assert strong_view["strongTemporalAndViewRecords"] == 10
    unstable = [parse_line(line(i, (60,55,45) if i < 4 else (120,110,90), centroid_drift=0.25, plane_offset=0.12, plane_angle=12.0)) for i in range(10)]
    unstable_result = analyze(unstable)
    assert unstable_result["worldGeometryQualifiedRecords"] == 0
    assert unstable_result["fullyQualifiedRecords"] == 0 and unstable_result["candidates"] == []
    assert unstable_result["worldGeometryEvidenceMissingCount"] == 10
    trunc = analyze(parsed + [parse_line(PREFIX + "truncated=1 maximumRecords=250000")])
    assert trunc["truncatedInput"] is True
    print("unbake audit analyzer self-test: PASS")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("logs", nargs="*", type=Path, help="runtime log(s); stdin when omitted")
    parser.add_argument("--output", type=Path, help="write JSON report here instead of stdout")
    parser.add_argument("--self-test", action="store_true")
    args = parser.parse_args()
    if args.self_test:
        self_test()
        return 0
    lines = []
    if args.logs:
        for path in args.logs:
            lines.extend(path.read_text(encoding="utf-8", errors="replace").splitlines())
    else:
        lines = sys.stdin.read().splitlines()
    report = analyze(parse_line(line) for line in lines)
    text = json.dumps(report, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(text, encoding="utf-8")
    else:
        sys.stdout.write(text)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
