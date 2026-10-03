"""Regression tests for native save wheel presets and persistent paint IDs."""
import struct
import unittest
from unittest.mock import patch

import gt1_convert as convert
import inject_gt1_prize_lm_garage as garage


class GarageTests(unittest.TestCase):
    def test_native_palette_identity_does_not_expand_other_car_tables(self):
        folds = [
            {"targetStem": "v-rbr", "bodyStem": "v1rbr", "finalColorIds": [98, 108, 99],
             "bodyMappings": [{"colorId": 99, "targetColorIndex": 2, "bodyPaletteIndex": 0}]},
            {"targetStem": "tsplr", "bodyStem": "tsplr", "finalColorIds": [108, 114], "nativePaletteFold": True,
             "bodyMappings": [{"colorId": 114, "targetColorIndex": 1, "bodyPaletteIndex": 1}]},
        ]
        records = convert._gt2_livery_table_records(folds)
        self.assertEqual(len(records), 3)
        self.assertEqual(sum(r["targetStem"] == "v-rbr" for r in records), 1)

    def test_stock_wheels_and_alternate_body_survive_record_construction(self):
        blocks = [bytes(size) for size in convert.GT2_GTD_PART_RECORD_SIZES]
        blocks.extend([b""] * (convert.GT2_GTMODE_BLOCK_COUNT - len(blocks)))
        car = bytearray(72)
        struct.pack_into("<I", car, 0, convert.encode_gt2_car_id("v-rbr"))
        blocks[convert.GT2_GTMODE_CAR_BLOCK] = car
        choice = garage.GarageChoice("Cerbera LM", "v-rbr", "v-rbr", 2, 99, "v1rbr", 0)
        for preset in (0, 5, 6, 10, 15):
            struct.pack_into("<H", car, 56, preset)
            record = garage.build_direct_record(blocks, choice)
            self.assertEqual(struct.unpack_from("<H", record, 64)[0], preset)
            self.assertEqual(struct.unpack_from("<I", record, 140)[0], convert.encode_gt2_car_id("v1rbr"))

    def test_native_lookup_rejects_black_paint_aliasing_white(self):
        choice = garage.GarageChoice("Castrol Supra prize", "tsplr", "t-plr", 2, 108, "tsplr", 2)
        with patch.object(garage, "read_gt2_member", return_value=b""), patch.object(
            garage, "_parse_gt2_carinfo", return_value=[{"stem": "tsplr", "colorIds": (108, 113, 108, 113)}]
        ):
            with self.assertRaisesRegex(ValueError, "different paint"):
                garage.validate_choice_data(None, [choice])
        choice = garage.GarageChoice("Castrol Supra prize", "tsplr", "t-plr", 2, 114, "tsplr", 2)
        with patch.object(garage, "read_gt2_member", return_value=b""), patch.object(
            garage, "_parse_gt2_carinfo", return_value=[{"stem": "tsplr", "colorIds": (108, 113, 114, 115)}]
        ):
            garage.validate_choice_data(None, [choice])

    def test_assigned_color_id_preserves_source_swatch_and_name(self):
        records = [
            {"stem": "tsplr", "carId": convert.encode_gt2_car_id("tsplr"), "mainColors": (123,), "colorIds": (108,), "name": b"Supra", "flags": 0},
            {"stem": "t-plr", "carId": convert.encode_gt2_car_id("t-plr"), "mainColors": (456,), "colorIds": (108,), "name": b"Black", "flags": 0},
        ]
        parse_info = convert._parse_gt2_carinfo
        parse_color = convert._parse_gt2_carcolor
        with patch.object(convert, "_parse_gt2_carinfo", side_effect=lambda data: records if not data else parse_info(data)), patch.object(
            convert, "_parse_gt2_carcolor", side_effect=lambda data, info: [(7,), (8,)] if not data else parse_color(data, info)
        ):
            info, colors, metadata = convert.extend_gt2_carinfo(b"", b"", "tsplr", ((108, "t-plr"),), target_color_ids=(114,))
        decoded = parse_info(info)
        self.assertEqual(decoded[0]["colorIds"], (108, 114))
        self.assertEqual(decoded[0]["mainColors"], (123, 456))
        self.assertEqual(parse_color(colors, decoded)[0], (7, 8))
        self.assertEqual(metadata["appended"][0]["colorId"], 114)


if __name__ == "__main__":
    unittest.main()
