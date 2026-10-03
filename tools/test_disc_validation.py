"""Regression coverage for source disc identification and extraction."""

import contextlib
import io
import shutil
import struct
import subprocess
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

from disc_validation import GT1_DATA_FILES, GT2_BOOT_FILES, identify_disc
from gt1_convert import validate_gt1_image, validate_disc_root
from release_setup import DISCS, discover_discs, validate_disc


def record(name, lba, size, flags=0):
    name = name.encode("ascii")
    data = bytearray(33 + len(name) + (len(name) % 2 == 0))
    data[0] = len(data)
    struct.pack_into("<I", data, 2, lba)
    struct.pack_into(">I", data, 6, lba)
    struct.pack_into("<I", data, 10, size)
    struct.pack_into(">I", data, 14, size)
    data[25] = flags
    data[28:32] = b"\x01\x00\x00\x01"
    data[32] = len(name)
    data[33:33 + len(name)] = name
    return data


def make_disc(path, kind="simulation", boot=None, padding=0, omitted=()):
    """Small authored fixture with file LBAs unlike the original retail dump."""
    if kind == "gt1":
        members = {name: (name + " data").encode() for name in GT1_DATA_FILES}
        if boot:
            members["SYSTEM.CNF"] = f"BOOT = cdrom:\\{boot};1\r\n".encode()
    else:
        boot = boot or GT2_BOOT_FILES[kind]
        members = {
            "SYSTEM.CNF": f"BOOT = cdrom:\\{boot};1\r\n".encode(),
            boot: b"PS-X EXE" + bytes(2040),
            "GT2.VOL": b"volume" * 700,
            "GT2.OVL": b"overlay" * 50,
        }
        members.update({name: b"stream" * 400 for name in (
            ("MUSIC.DAT", "FAULTY.PSX") if kind == "simulation" else ("STREAM.DAT",)
        )})
    sectors = {16: bytearray(2048), 22: bytearray(2048)}
    sectors[16][:7] = b"\x01CD001\x01"
    root = record("\0", 22, 2048, 2)
    sectors[16][156:156 + len(root)] = root
    offset, lba = 0, 30
    for name, payload in members.items():
        if name in omitted:
            continue
        entry = record(name + ";1", lba, len(payload))
        sectors[22][offset:offset + len(entry)] = entry
        offset += len(entry)
        for pos in range(0, len(payload), 2048):
            sectors[lba] = payload[pos:pos + 2048].ljust(2048, b"\0")
            lba += 1
    with path.open("wb") as output:
        for index in range(lba + padding):
            sector = bytearray(2352)
            sector[24:2072] = sectors.get(index, bytes(2048))
            output.write(sector)
    return members


class DiscValidationTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.log = type("Log", (), {"write": lambda self, message: None})()

    def test_usa_discs_accept_padding_and_changed_content(self):
        for kind in GT2_BOOT_FILES:
            for padding in (0, 78):
                path = self.root / f"{kind}-{padding}.bin"
                make_disc(path, kind, padding=padding)
                self.assertEqual(validate_disc(path, DISCS[kind], self.log), path.resolve())

    def test_wrong_disc_and_non_usa_rejected(self):
        path = self.root / "disc.img"
        make_disc(path, "arcade")
        with self.assertRaisesRegex(ValueError, "Expected"):
            validate_disc(path, DISCS["simulation"], self.log)
        for boot in ("SCES_023.80", "SCPS_101.16"):
            make_disc(path, boot=boot)
            with self.assertRaisesRegex(ValueError, "USA"):
                identify_disc(path)

    def test_gt1_has_no_region_or_executable_requirement(self):
        path = self.root / "gt1.img"
        for boot in (None, "SCUS_941.94", "SCES_009.84", "SCPS_100.45"):
            make_disc(path, "gt1", boot=boot, padding=10)
            with contextlib.redirect_stdout(io.StringIO()):
                validate_gt1_image(path)
        extracted = self.root / "extracted"
        extracted.mkdir()
        for name in GT1_DATA_FILES:
            (extracted / name).write_bytes(b"data")
        validate_disc_root(extracted)

    def test_missing_and_truncated_files_rejected(self):
        path = self.root / "broken.img"
        for omitted in ("GT2.OVL", "MUSIC.DAT"):
            make_disc(path, omitted=(omitted,))
            with self.assertRaisesRegex(ValueError, "Missing"):
                identify_disc(path)
        make_disc(path)
        with path.open("r+b") as stream:
            stream.truncate(path.stat().st_size - 2352)
        with self.assertRaisesRegex(ValueError, "truncated"):
            identify_disc(path)
        path.write_bytes(b"not an ISO")
        with self.assertRaisesRegex(ValueError, "descriptor"):
            identify_disc(path)

    def test_discovery_uses_contents_not_sizes_or_names(self):
        for i, kind in enumerate(DISCS):
            make_disc(self.root / f"unknown-{i}.bin", kind, padding=i)
        (self.root / "junk.iso").write_bytes(bytes(100))
        with patch("release_setup.search_roots", return_value=[(self.root, 1)]):
            self.assertEqual(set(discover_discs(self.root, self.log)), set(DISCS))

    def test_powershell_reader_and_extractor(self):
        shell = shutil.which("pwsh") or shutil.which("powershell")
        if not shell:
            self.skipTest("PowerShell is unavailable")
        payloads = make_disc(self.root / "simulation.bin", padding=78)
        make_disc(self.root / "arcade.bin", "arcade")
        make_disc(self.root / "pal.bin", boot="SCES_023.80")
        make_disc(self.root / "truncated.bin")
        with (self.root / "truncated.bin").open("r+b") as stream:
            stream.truncate((self.root / "truncated.bin").stat().st_size - 2352)
        script = Path(__file__).with_name("test_release_disc_validation.ps1")
        subprocess.run([shell, "-NoProfile", "-File", str(script), "-FixtureRoot", str(self.root)], check=True)
        self.assertEqual((self.root / "extracted.vol").read_bytes(), payloads["GT2.VOL"])
        self.assertEqual((self.root / "unified.vol").read_bytes(), payloads["GT2.VOL"] * 2)


if __name__ == "__main__":
    unittest.main()
