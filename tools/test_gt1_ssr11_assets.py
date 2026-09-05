#!/usr/bin/env python3
"""SSR11 sky/track VRAM contract tests (source-disc checks skip if absent)."""

import struct
import unittest

import gt1_convert as convert


def tim(x=704, y=256, clut_x=0, clut_y=488, palette=0x8000):
    return (
        struct.pack("<II", 16, 8)
        + struct.pack("<I4H16H", 44, clut_x, clut_y, 16, 1, *([palette] * 16))
        + struct.pack("<I4HH", 14, x, y, 1, 1, 0x3210)
    )


def named_package(members):
    directory = bytearray(struct.pack("<I", len(members)))
    offset = 4 + len(members) * 20
    for index, member in enumerate(members):
        directory.extend(struct.pack("<16sI", f"image{index}.tim".encode(), offset))
        offset += len(member)
    return bytes(directory) + b"".join(members)


class SkyConversionTests(unittest.TestCase):
    def test_palette_relocation_preserves_all_art_and_stp_bits(self):
        source = tim(palette=0x8123)
        template = struct.pack("<I", 1) + tim(clut_x=624, clut_y=498, palette=0x4567)
        result = convert.tim_stream_members(convert.convert_gt1_sky_texture_package(
            named_package([source]), template
        ))[0]
        self.assertEqual(result[12:16], struct.pack("<HH", 624, 498))
        self.assertEqual(result[:12] + result[16:], source[:12] + source[16:])

    def test_mismatched_native_image_is_rejected(self):
        with self.assertRaisesRegex(ValueError, "image planes differ"):
            convert.convert_gt1_sky_texture_package(
                named_package([tim()]), struct.pack("<I", 1) + tim(x=768)
            )

    def test_malformed_stream_is_rejected(self):
        for data in (b"", struct.pack("<I", 1), struct.pack("<I", 1) + tim()[:-1],
                     struct.pack("<I", 1) + tim() + b"x"):
            with self.subTest(size=len(data)), self.assertRaises(ValueError):
                convert.tim_stream_members(data)

    def test_course_does_not_allocate_reserved_sky_palette(self):
        source = named_package([tim(x=384), tim(x=448), tim(x=640, clut_y=489)])
        output, _, relocation = convert.convert_gt1_texture_package(
            source, reserved_cluts=frozenset({(496, 496)})
        )
        self.assertEqual(set(relocation.clut_ids.values()), {496 * 64 + 32, 496 * 64 + 33})
        self.assertNotIn((496, 496), {
            struct.unpack_from("<HH", member, 12)
            for member in convert.tim_stream_members(output)
        })


@unittest.skipUnless(
    (convert.REPO / "work/gt1-disc/BG.DAT").is_file()
    and (convert.REPO / "work/arcade-disc/GT2.VOL").is_file(),
    "user-supplied GT1 and stock GT2 source assets are unavailable",
)
class SourceAssetTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        archive, entries = convert.read_gtarc(convert.REPO / "work/gt1-disc/BG.DAT")
        cls.source = convert.unpack_entry(archive, entries[convert.GT1_SSR11_SKY_INDEX * 2])
        volume = convert.REPO / "work/arcade-disc/GT2.VOL"
        cls.native = convert.read_gt2_gzip_member(volume, "bgsobj/dawn.bsp.gz")
        cls.converted = convert.convert_gt1_sky_texture_package(cls.source, cls.native)

    def test_original_art_matches_stock_layout_not_stock_palette(self):
        original = convert.named_tim_members(self.source)
        native = convert.tim_stream_members(self.native)
        result = convert.tim_stream_members(self.converted)
        self.assertEqual(len(result), 3)
        self.assertEqual({struct.unpack_from("<HH", t, 12) for t in result},
                         {(624, 498), (624, 499), (624, 500)})
        for (_, source), template, converted in zip(original, native, result, strict=True):
            self.assertEqual(converted[12:16], template[12:16])
            self.assertEqual(converted[:12] + converted[16:], source[:12] + source[16:])

    def test_all_six_track_variants_keep_art_and_avoid_sky_palettes(self):
        archive, entries = convert.read_gtarc(convert.REPO / "work/gt1-disc/COURSE.DAT")
        reserved = frozenset(struct.unpack_from("<HH", t, 12)
                             for t in convert.tim_stream_members(self.converted))
        self.assertEqual(len(convert.SSR11_VARIANTS), 6)
        for stem, index, _ in convert.SSR11_VARIANTS:
            with self.subTest(course=stem):
                source = convert.unpack_entry(archive, entries[index * 2])
                result, _, relocation = convert.convert_gt1_texture_package(
                    source, reserved_cluts=reserved
                )
                output = convert.tim_stream_members(result)
                self.assertFalse(reserved.intersection(
                    struct.unpack_from("<HH", t, 12) for t in output
                ))
                old_by_palette = {struct.unpack_from("<HH", t, 12): t[20:52]
                                  for _, t in convert.named_tim_members(source)}
                new_by_palette = {struct.unpack_from("<HH", t, 12): t[20:52] for t in output}
                for old, new in relocation.clut_ids.items():
                    old_xy = ((old & 63) * 16, old >> 6)
                    new_xy = ((new & 63) * 16, new >> 6)
                    self.assertEqual(old_by_palette[old_xy], new_by_palette[new_xy])


if __name__ == "__main__":
    unittest.main(verbosity=2)
