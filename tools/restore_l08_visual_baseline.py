#!/usr/bin/env python3
"""Restore the exact OpenGT L08 visual-acceptance game baseline.

This is a hardened descendant of the restore.py preserved inside the L08
checkpoint. It reads the original split OpenGTPS1.7z.* archive directly
through libarchive, or copies from an already extracted OpenGTPS1 root.
Every required baseline file is verified by byte length and SHA-256.

The tool never invents carda.sav or settings.json. The only synthesized
fallback is cardb.sav, whose blank RecompOne representation is deterministic
and must hash to the frozen L08 identity.
"""
from __future__ import annotations
import argparse
import ctypes as C
import hashlib
import json
from pathlib import Path, PurePosixPath
import shutil

EXPECTED_L08 = {
    "GT2.VOL": (708_319_232, "2156fa6c18bd39866a15cf7c62f59af1c71d6dc1eca2d3b9e80a3b96169c9a73"),
    "MUSIC.DAT": (97_252_352, "2d1b7a30f656900213fa1f4aff9371c22db9d7f41ae8a9e5c5d51de8ffc9e102"),
    "TITLE_EXACT.DAT": (1_966_100, "735d838c3a0f12e2917593648790f9fd1cb6ada13d402e19022d7c814737321c"),
    "settings.json": (1_902, "92ed1825e05bd76d2b0db453cfee9acbe762d1a4e2b445a1772a783f5a1f640a"),
    "carda.sav": (131_072, "4c463036ea7cf941834195cc28631a0300d63ffa98a9900accfac16e994c56c3"),
    "cardb.sav": (131_072, "78b6d4ac9ab4d23caf7e5f04f83539bf5d994cccfb0a709d14ac53d05c8e21ef"),
    "GTLIVERY.BIN": (1_748, "fb388e5a7573f471ca4efadade6d8cf1f506a56f8ba6a5bad8bbb818543c21c8"),
}
ARCHIVE_PREFIX = PurePosixPath("OpenGTPS1/OpenGTPS1")
PRESERVED_SUBTREES = {"arcade", "simulation", "manifests"}


def sha256_file(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def load_identities(path: Path | None) -> dict[str, tuple[int, str]]:
    if path is None:
        return dict(EXPECTED_L08)
    data = json.loads(path.read_text())
    result = {}
    if isinstance(data, list):
        for row in data:
            result[str(row["name"])] = (
                int(row.get("size", row.get("bytes"))),
                str(row["sha256"]).lower(),
            )
    elif isinstance(data, dict):
        for name, row in data.items():
            if isinstance(row, dict):
                result[str(name)] = (
                    int(row.get("size", row.get("bytes"))),
                    str(row["sha256"]).lower(),
                )
            else:
                result[str(name)] = (int(row[0]), str(row[1]).lower())
    else:
        raise ValueError("identity file must be a JSON object or array")
    return result


def blank_memory_card_bytes() -> bytes:
    d = bytearray(0x20000)
    frame = 0x80

    def fix(offset: int) -> None:
        checksum = 0
        for i in range(0x7F):
            checksum ^= d[offset + i]
        d[offset + 0x7F] = checksum

    d[0:2] = b"MC"
    fix(0)
    for i in range(1, 16):
        o = i * frame
        d[o] = 0xA0
        d[o + 8:o + 10] = b"\xFF\xFF"
        fix(o)
    for i in range(16, 36):
        o = i * frame
        d[o:o + 4] = b"\xFF\xFF\xFF\xFF"
        d[o + 8:o + 10] = b"\xFF\xFF"
        fix(o)
    return bytes(d)


def safe_relative(name: str, required_names: set[str]) -> Path | None:
    q = PurePosixPath(name)
    if q.is_absolute() or ".." in q.parts:
        raise ValueError(f"unsafe archive path: {name}")
    prefix = ARCHIVE_PREFIX.parts
    if q.parts[:len(prefix)] != prefix or len(q.parts) <= len(prefix):
        return None
    rel = PurePosixPath(*q.parts[len(prefix):])
    if len(rel.parts) == 1 and rel.name in required_names:
        return Path(rel.name)
    if rel.parts and rel.parts[0] in PRESERVED_SUBTREES:
        return Path(*rel.parts)
    return None


class LibArchiveReader:
    def __init__(self) -> None:
        last = None
        for soname in ("libarchive.so.13", "libarchive.so"):
            try:
                self.a = C.CDLL(soname)
                break
            except OSError as exc:
                last = exc
        else:
            raise RuntimeError("libarchive is unavailable") from last
        a = self.a
        a.archive_read_new.restype = C.c_void_p
        for n in (
            "archive_read_support_format_all",
            "archive_read_support_filter_all",
            "archive_read_free",
            "archive_read_data_skip",
        ):
            getattr(a, n).argtypes = [C.c_void_p]
        a.archive_read_open_filenames.argtypes = [
            C.c_void_p, C.POINTER(C.c_char_p), C.c_size_t
        ]
        a.archive_read_next_header.argtypes = [
            C.c_void_p, C.POINTER(C.c_void_p)
        ]
        a.archive_entry_pathname.argtypes = [C.c_void_p]
        a.archive_entry_pathname.restype = C.c_char_p
        a.archive_entry_size.argtypes = [C.c_void_p]
        a.archive_entry_size.restype = C.c_int64
        a.archive_read_data.argtypes = [
            C.c_void_p, C.c_void_p, C.c_size_t
        ]
        a.archive_read_data.restype = C.c_int64
        a.archive_error_string.argtypes = [C.c_void_p]
        a.archive_error_string.restype = C.c_char_p

    def extract(
        self, parts: list[Path], output: Path, required_names: set[str]
    ) -> dict:
        a = self.a
        reader = a.archive_read_new()
        a.archive_read_support_format_all(reader)
        a.archive_read_support_filter_all(reader)
        names = [str(p.resolve()).encode() for p in parts]
        arr = (C.c_char_p * (len(names) + 1))(*names, None)
        status = a.archive_read_open_filenames(
            reader, arr, 1024 * 1024
        )
        if status < 0:
            message = a.archive_error_string(reader)
            raise RuntimeError(
                message.decode(errors="replace")
                if message else f"libarchive open failed: {status}"
            )

        entry = C.c_void_p()
        buf = C.create_string_buffer(1024 * 1024)
        restored = []
        inventory = []
        try:
            while True:
                status = a.archive_read_next_header(
                    reader, C.byref(entry)
                )
                if status == 1:  # ARCHIVE_EOF
                    break
                if status < 0:
                    message = a.archive_error_string(reader)
                    raise RuntimeError(
                        message.decode(errors="replace")
                        if message else "libarchive header read failed"
                    )
                raw_name = a.archive_entry_pathname(entry)
                name = (
                    raw_name.decode(errors="replace")
                    if raw_name else ""
                )
                size = int(a.archive_entry_size(entry))
                inventory.append({"path": name, "size": size})
                rel = safe_relative(name, required_names)
                if rel is None or size <= 0:
                    a.archive_read_data_skip(reader)
                    continue

                dst = output / rel
                dst.parent.mkdir(parents=True, exist_ok=True)
                written = 0
                with dst.open("wb") as f:
                    while True:
                        n = int(a.archive_read_data(
                            reader, buf, len(buf)
                        ))
                        if n < 0:
                            message = a.archive_error_string(reader)
                            raise RuntimeError(
                                message.decode(errors="replace")
                                if message else "libarchive data read failed"
                            )
                        if n == 0:
                            break
                        f.write(buf.raw[:n])
                        written += n
                if written != size:
                    raise IOError(
                        f"short extraction for {name}: "
                        f"{written} != {size}"
                    )
                restored.append(str(rel))
        finally:
            a.archive_read_free(reader)
        return {"inventory": inventory, "restored": restored}


def discover_parts(args: argparse.Namespace) -> list[Path]:
    parts = []
    if args.archive_part:
        parts.extend(args.archive_part)
    if args.archive_glob:
        pattern = Path(args.archive_glob)
        parts.extend(pattern.parent.glob(pattern.name))
    result = sorted(
        {p.resolve() for p in parts},
        key=lambda p: p.name,
    )
    for path in result:
        if not path.is_file():
            raise FileNotFoundError(path)
    return result


def find_extracted_root(
    source: Path, required_names: set[str]
) -> Path:
    candidates = [
        source,
        source / "OpenGTPS1",
        source / "OpenGTPS1/OpenGTPS1",
    ]
    candidates.extend(
        p for p in source.rglob("OpenGTPS1") if p.is_dir()
    )
    scored = []
    seen = set()
    for candidate in candidates:
        candidate = candidate.resolve()
        if candidate in seen or not candidate.is_dir():
            continue
        seen.add(candidate)
        file_score = sum(
            (candidate / name).is_file()
            for name in required_names
        )
        tree_score = sum(
            (candidate / name).is_dir()
            for name in PRESERVED_SUBTREES
        )
        scored.append((file_score, tree_score, candidate))
    if not scored:
        raise FileNotFoundError(
            f"no OpenGT baseline root found under {source}"
        )
    scored.sort(
        key=lambda row: (
            row[0], row[1], -len(row[2].parts)
        ),
        reverse=True,
    )
    return scored[0][2]


def copy_from_root(
    source: Path, output: Path, required_names: set[str]
) -> dict:
    root = find_extracted_root(source, required_names)
    restored = []
    for name in sorted(required_names):
        src = root / name
        if src.is_file():
            shutil.copy2(src, output / name)
            restored.append(name)
    for subtree in sorted(PRESERVED_SUBTREES):
        src = root / subtree
        if src.is_dir():
            shutil.copytree(
                src, output / subtree, dirs_exist_ok=True
            )
            restored.append(subtree + "/")
    return {"sourceRoot": str(root), "restored": restored}


def verify(
    output: Path,
    identities: dict[str, tuple[int, str]],
    allow_blank_cardb: bool,
) -> dict:
    fallback = None
    card_b = output / "cardb.sav"
    if (
        allow_blank_cardb
        and "cardb.sav" in identities
        and not card_b.is_file()
    ):
        card_b.write_bytes(blank_memory_card_bytes())
        fallback = (
            "generated deterministic blank RecompOne memory card"
        )

    files = {}
    unresolved = []
    mismatched = []
    for name, (size, digest) in identities.items():
        path = output / name
        if not path.is_file():
            unresolved.append(name)
            continue
        actual = {
            "bytes": path.stat().st_size,
            "sha256": sha256_file(path),
        }
        exact = (
            actual["bytes"] == size
            and actual["sha256"] == digest
        )
        files[name] = {
            "expected": {"bytes": size, "sha256": digest},
            "actual": actual,
            "exact": exact,
        }
        if not exact:
            mismatched.append(name)

    trees = {
        name: (output / name).is_dir()
        for name in sorted(PRESERVED_SUBTREES)
    }
    return {
        "files": files,
        "unresolved": unresolved,
        "mismatched": mismatched,
        "preservedSubtrees": trees,
        "cardbFallback": fallback,
        "exactBaselineReady": (
            not unresolved
            and not mismatched
            and all(trees.values())
        ),
    }


def restore(args: argparse.Namespace) -> dict:
    identities = load_identities(args.identity_file)
    output = args.output.resolve()
    if (
        output.exists()
        and any(output.iterdir())
        and not args.overwrite
    ):
        raise FileExistsError(
            f"output directory is not empty: {output}"
        )
    output.mkdir(parents=True, exist_ok=True)

    if args.source_root is not None:
        mode = "extracted-root"
        source = copy_from_root(
            args.source_root.resolve(),
            output,
            set(identities),
        )
    else:
        parts = discover_parts(args)
        if not parts:
            raise ValueError(
                "supply --source-root, --archive-part, "
                "or --archive-glob"
            )
        mode = "libarchive"
        source = LibArchiveReader().extract(
            parts, output, set(identities)
        )
        source["archiveParts"] = [str(p) for p in parts]

    verification = verify(
        output, identities, args.allow_blank_cardb
    )
    report = {
        "format": "OpenGT-L08-visual-baseline-restore-v1",
        "mode": mode,
        "output": str(output),
        "source": source,
        "verification": verification,
    }
    report_path = output / "L08-BASELINE-RESTORE-REPORT.json"
    report_path.write_text(
        json.dumps(report, indent=2) + "\n"
    )
    return report


def parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(description=__doc__)
    source = p.add_mutually_exclusive_group()
    source.add_argument(
        "--source-root",
        type=Path,
        help="already extracted OpenGTPS1 root or ancestor",
    )
    source.add_argument(
        "--archive-glob",
        help=(
            "glob for original archive parts, e.g. "
            "'/mnt/data/OpenGTPS1.7z.*'"
        ),
    )
    p.add_argument(
        "--archive-part",
        type=Path,
        action="append",
        help="archive part; repeat for every split part",
    )
    p.add_argument("--output", type=Path, required=True)
    p.add_argument(
        "--identity-file",
        type=Path,
        help=(
            "override identities for testing; production defaults "
            "to frozen L08"
        ),
    )
    p.add_argument(
        "--allow-blank-cardb",
        action=argparse.BooleanOptionalAction,
        default=True,
    )
    p.add_argument("--overwrite", action="store_true")
    args = p.parse_args()
    if args.source_root is not None and args.archive_part:
        p.error(
            "--source-root cannot be combined with --archive-part"
        )
    return args


def main() -> int:
    args = parse_args()
    report = restore(args)
    verification = report["verification"]
    print(json.dumps({
        "output": report["output"],
        "exactBaselineReady": (
            verification["exactBaselineReady"]
        ),
        "unresolved": verification["unresolved"],
        "mismatched": verification["mismatched"],
        "preservedSubtrees": (
            verification["preservedSubtrees"]
        ),
        "cardbFallback": verification["cardbFallback"],
        "report": str(
            Path(report["output"])
            / "L08-BASELINE-RESTORE-REPORT.json"
        ),
    }, indent=2))
    return 0 if verification["exactBaselineReady"] else 2


if __name__ == "__main__":
    raise SystemExit(main())
