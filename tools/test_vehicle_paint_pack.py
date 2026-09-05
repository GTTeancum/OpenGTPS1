"""Deterministic, asset-free tests for authored companion paint masks."""
import json
import unittest
from pathlib import Path
from build_vehicle_paint_pack import bitmap_key, make_mask

class VehiclePaintMaskTests(unittest.TestCase):
    def setUp(self):
        self.description=json.loads((Path(__file__).resolve().parents[1]/
            'mods/gt2000_paint/masks/fcx8n.json').read_text())

    def test_material_channels_and_glass_exclusion(self):
        mask=make_mask(self.description)
        self.assertEqual(mask.size,(256,224))
        self.assertEqual(mask.getpixel((30,150)),(210,150,0,255))
        # Authored windscreen, rear glass, side glass and grille islands.
        for point in [(45,112),(210,12),(90,38),(120,154)]:
            self.assertEqual(mask.getpixel(point),(0,0,0,0),point)
        self.assertEqual(sum(a>0 for a in mask.getchannel('A').tobytes()),7339)

    def test_invalid_region_rejected(self):
        self.description['regions'][0]['polygon'][0]=[-1,128]
        with self.assertRaises(ValueError): make_mask(self.description)

    def test_invalid_material_rejected(self):
        self.description['regions'][0]['strength']=256
        with self.assertRaises(ValueError): make_mask(self.description)

    def test_bitmap_identity_stable(self):
        self.assertEqual(bitmap_key(b''),0xcbf29ce484222325)
        self.assertEqual(bitmap_key(b'hello'),0xa430d84680aabd0b)
        self.assertNotEqual(bitmap_key(b'hello'),bitmap_key(b'jello'))

if __name__=='__main__': unittest.main()
