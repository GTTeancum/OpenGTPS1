#!/usr/bin/env python3

import json
from pathlib import Path
import struct
import tempfile
import unittest

import numpy as np
from PIL import Image

from build_gt2_texture_pack import (
    PAD,
    PACK_FORMAT,
    REALESRGAN_MODEL,
    REALESRGAN_MODEL_FILES,
    REALESRGAN_TILE,
    REPLACEMENT_PALETTE_DETAIL,
    REPLACEMENT_RGB,
    SCALE,
    asset_stem,
    audit_car_composite,
    decode_car_composite,
    decode_car_neutral_detail,
    png_inventory,
    prepare_sources,
    realesrgan_identity,
    run_or_verify_realesrgan,
    runtime_upload_keys,
    sha256_file,
    validate_pack,
    write_pack,
)


class TexturePackTests(unittest.TestCase):
    def test_asset_name_supports_plain_and_legacy_compound_identity(self) -> None:
        self.assertEqual(asset_stem((0x12, 0)), "0000000000000012")
        self.assertEqual(
            asset_stem((0x12, 0x34)),
            "0000000000000012-0000000000000034",
        )

    def test_car_composite_uses_the_uv_islands_material_palette(self) -> None:
        texture = bytearray(0xB3A0)
        texture[0] = 1
        # Every source texel selects color index 1. Material 0 makes it red;
        # material 1 makes it green.
        texture[0x43A0:0xB3A0] = bytes([0x11]) * (0x7000)
        struct.pack_into("<H", texture, 0x20 + 2, 31)
        struct.pack_into("<H", texture, 0x20 + 32 + 2, 31 << 5)
        masks = np.zeros((16, 224, 256), dtype=bool)
        masks[0, :, :128] = True
        masks[1, :, 128:] = True

        audit = audit_car_composite(bytes(texture), 0, masks)
        self.assertEqual(audit["conflictingOpaqueTexels"], 0)
        image = np.asarray(decode_car_composite(bytes(texture), 0, masks))
        self.assertTupleEqual(tuple(image[100, 64]), (255, 0, 0, 255))
        self.assertTupleEqual(tuple(image[100, 192]), (0, 255, 0, 255))

        masks[1, 100, 64] = True
        audit = audit_car_composite(bytes(texture), 0, masks)
        self.assertEqual(audit["conflictingOpaqueTexels"], 1)
        with self.assertRaisesRegex(ValueError, "conflicting palettes"):
            decode_car_composite(bytes(texture), 0, masks)

    def test_car_neutral_detail_uses_one_grayscale_bitmap_for_all_paints(self) -> None:
        bitmap = bytes([0x11]) * 0x7000
        first = bytearray(0x200)
        second = bytearray(0x200)
        struct.pack_into("<H", first, 2, 31)
        struct.pack_into("<H", first, 32 + 2, 31 << 5)
        struct.pack_into("<H", second, 2, 31 << 10)
        struct.pack_into("<H", second, 32 + 2, 0x7FFF)
        masks = np.zeros((16, 224, 256), dtype=bool)
        masks[0, :, :128] = True
        masks[1, :, 128:] = True

        detail = np.asarray(decode_car_neutral_detail(
            bitmap, [bytes(first), bytes(second)], masks
        ))
        self.assertTrue(np.array_equal(detail[:, :, 0], detail[:, :, 1]))
        self.assertTrue(np.array_equal(detail[:, :, 1], detail[:, :, 2]))
        self.assertNotEqual(int(detail[100, 64, 0]), int(detail[100, 192, 0]))
        self.assertEqual(int(detail[100, 64, 3]), 255)

    def test_source_padding_extends_edges_without_reflected_repetition(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            source = Image.new("RGBA", (3, 3), (0, 0, 0, 255))
            source.putpixel((1, 0), (255, 0, 0, 255))
            source.putpixel((1, 1), (0, 255, 0, 255))
            source.putpixel((1, 2), (0, 0, 255, 255))
            prepare_sources({(1, 0): source}, root)
            padded = np.asarray(Image.open(root / f"{asset_stem((1, 0))}.png"))
            self.assertTupleEqual(tuple(padded[0, PAD + 1]), (255, 0, 0))
            self.assertTupleEqual(tuple(padded[-1, PAD + 1]), (0, 0, 255))

    def test_manifest_writes_one_4x_dds_per_bitmap_with_provenance(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            sources = root / "sources"
            generated = root / "generated"
            output = root / "pack"
            sources.mkdir()
            generated.mkdir()
            course = (0x1111, 0)
            car = (0x2222, 0)
            images = {
                course: Image.new("RGBA", (4, 4), (255, 0, 0, 255)),
                car: Image.new("RGBA", (4, 4), (96, 96, 96, 255)),
            }
            generated_size = (4 + PAD * 2) * SCALE
            for identity, image in images.items():
                stem = asset_stem(identity)
                Image.new("RGB", (4 + PAD * 2, 4 + PAD * 2),
                          image.getpixel((0, 0))[:3]).save(sources / f"{stem}.png")
                Image.new("RGB", (generated_size, generated_size),
                          image.getpixel((0, 0))[:3]).save(generated / f"{stem}.png")
            neural_run = {
                "status": "complete",
                "identity": {"model": REALESRGAN_MODEL, "scale": SCALE},
                "inputs": png_inventory(sources),
                "outputs": png_inventory(generated),
            }

            write_pack(
                images,
                {course: 0, car: 0},
                {course: True, car: False},
                {course: REPLACEMENT_RGB,
                 car: REPLACEMENT_PALETTE_DETAIL},
                generated,
                output,
                neural_run,
            )
            validate_pack(output, 2)
            manifest = json.loads(
                (output / "manifest.json").read_text(encoding="utf-8")
            )
            self.assertEqual(manifest["format"], PACK_FORMAT)
            self.assertEqual(manifest["scale"], 4)
            self.assertEqual(len({entry["image"] for entry in manifest["entries"]}), 2)
            by_key = {entry["key"]: entry for entry in manifest["entries"]}
            self.assertNotIn("paletteKey", by_key[f"{course[0]:016x}"])
            self.assertEqual(by_key[f"{course[0]:016x}"]["colorFit"], 1)
            self.assertEqual(
                by_key[f"{course[0]:016x}"]["replacementMode"],
                REPLACEMENT_RGB,
            )
            self.assertEqual(by_key[f"{car[0]:016x}"]["colorFit"], 0)
            self.assertNotIn("paletteKey", by_key[f"{car[0]:016x}"])
            self.assertEqual(
                by_key[f"{car[0]:016x}"]["replacementMode"],
                REPLACEMENT_PALETTE_DETAIL,
            )
            with Image.open(output / by_key[f"{course[0]:016x}"]["image"]) as dds:
                self.assertTupleEqual(dds.getpixel((4, 4))[:3], (255, 0, 0))

    def test_pack_rejects_a_neural_output_that_changes_the_source(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            generated = root / "generated"
            generated.mkdir()
            identity = (0xABCD, 0)
            source = Image.new("RGBA", (4, 4), (255, 255, 255, 255))
            size = (4 + PAD * 2) * SCALE
            Image.new("RGB", (size, size), (0, 0, 0)).save(
                generated / f"{asset_stem(identity)}.png"
            )
            name = f"{asset_stem(identity)}.png"
            neural_run = {
                "status": "complete",
                "identity": {"model": REALESRGAN_MODEL, "scale": SCALE},
                "inputs": {name: {"sha256": "0" * 64}},
                "outputs": png_inventory(generated),
            }
            with self.assertRaisesRegex(ValueError, "fidelity gate"):
                write_pack(
                    {identity: source},
                    {identity: 0},
                    {identity: True},
                    {identity: REPLACEMENT_RGB},
                    generated,
                    root / "pack",
                    neural_run,
                )

    def test_reviewed_graphic_exception_is_explicit_and_key_scoped(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            generated = root / "generated"
            generated.mkdir()
            reviewed = (0xABCD, 0)
            protected = (0xDCBA, 0)
            source = Image.new("RGBA", (4, 4), (255, 255, 255, 255))
            size = (4 + PAD * 2) * SCALE
            for identity in (reviewed, protected):
                Image.new("RGB", (size, size), (0, 0, 0)).save(
                    generated / f"{asset_stem(identity)}.png"
                )
            neural_run = {
                "status": "complete",
                "identity": {"model": REALESRGAN_MODEL, "scale": SCALE},
                "inputs": {
                    f"{asset_stem(identity)}.png": {"sha256": "0" * 64}
                    for identity in (reviewed, protected)
                },
                "outputs": png_inventory(generated),
            }

            with self.assertRaisesRegex(ValueError, "fidelity gate"):
                write_pack(
                    {reviewed: source, protected: source},
                    {reviewed: 0, protected: 0},
                    {reviewed: True, protected: True},
                    {reviewed: REPLACEMENT_RGB, protected: REPLACEMENT_RGB},
                    generated,
                    root / "pack",
                    neural_run,
                    {reviewed[0]},
                )

            write_pack(
                {reviewed: source},
                {reviewed: 0},
                {reviewed: True},
                {reviewed: REPLACEMENT_RGB},
                generated,
                root / "reviewed-pack",
                neural_run,
                {reviewed[0]},
            )
            manifest = json.loads(
                (root / "reviewed-pack" / "manifest.json").read_text(
                    encoding="utf-8"
                )
            )
            self.assertEqual(manifest["entries"][0]["fidelityExempt"], 1)

    def test_reuse_requires_exact_model_input_and_output_hashes(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            executable = root / "realesrgan-ncnn-vulkan.exe"
            models = root / "models"
            sources = root / "source_padded"
            generated = root / "esrgan"
            models.mkdir()
            sources.mkdir()
            generated.mkdir()
            executable.write_bytes(b"test executable")
            (models / f"{REALESRGAN_MODEL_FILES}.param").write_bytes(b"test graph")
            (models / f"{REALESRGAN_MODEL_FILES}.bin").write_bytes(b"test weights")
            Image.new("RGB", (8, 8), (20, 30, 40)).save(sources / "a.png")
            Image.new("RGB", (32, 32), (21, 31, 41)).save(generated / "a.png")
            command = [
                str(executable.resolve()),
                "-i", str(sources.resolve()),
                "-o", str(generated.resolve()),
                "-n", REALESRGAN_MODEL,
                "-s", str(SCALE),
                "-t", str(REALESRGAN_TILE),
                "-j", "1:1:1",
                "-f", "png",
            ]
            record = {
                "status": "complete",
                "identity": realesrgan_identity(executable.resolve()),
                "command": command,
                "inputs": png_inventory(sources.resolve()),
                "outputs": png_inventory(generated.resolve()),
            }
            (root / "esrgan_run.json").write_text(
                json.dumps(record), encoding="utf-8"
            )
            verified = run_or_verify_realesrgan(
                sources.resolve(), generated.resolve(), executable.resolve(), True
            )
            self.assertEqual(verified["outputs"], record["outputs"])

            Image.new("RGB", (32, 32), (22, 32, 42)).save(generated / "a.png")
            with self.assertRaisesRegex(ValueError, "rejected stale"):
                run_or_verify_realesrgan(
                    sources.resolve(), generated.resolve(), executable.resolve(), True
                )

    def test_upload_trace_parser_includes_ui_sized_uploads(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            trace = Path(temporary) / "trace.log"
            trace.write_text(
                "[TextureUpload] key=0123456789abcdef x=0 y=0 words=16 "
                "height=8 data=00\n",
                encoding="utf-8",
            )
            self.assertEqual(runtime_upload_keys(trace), {0x0123456789ABCDEF: (16, 8)})


if __name__ == "__main__":
    unittest.main()
