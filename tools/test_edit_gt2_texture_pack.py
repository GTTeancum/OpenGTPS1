import json
from pathlib import Path
import struct
import tempfile
import unittest
from unittest.mock import patch
from types import SimpleNamespace

from PIL import Image
from edit_gt2_texture_pack import export_project, build_project


class EditableTextureTests(unittest.TestCase):
    def export(self, root):
        # A small 4-bit authored TIM: palette index 1 is red.
        palette = struct.pack('<16H', 0, 31, *([0] * 14))
        tim = struct.pack('<II', 16, 8) + struct.pack('<IHHHH', 44, 0, 480, 16, 1) + palette
        tim += struct.pack('<IHHHH', 28, 640, 0, 2, 4) + bytes([0x11] * 16)
        with patch('edit_gt2_texture_pack.read_entries', return_value=[SimpleNamespace(name='menu.tim')]), patch(
            'edit_gt2_texture_pack.read_volume_member', return_value=tim):
            self.assertEqual(export_project([Path('test.vol')], root, ['menu.tim']), 1)
        return json.loads((root / 'textures.json').read_text())

    def test_edited_png_is_used_exactly_without_ai_or_resize(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            data = self.export(root)
            image = root / data['textures'][0]['image']
            Image.new('RGBA', (32, 16), (17, 255, 43, 255)).save(image)
            self.assertEqual(build_project(root, root / 'pack'), 1)
            manifest = json.loads((root / 'pack/manifest.json').read_text())
            entry = manifest['entries'][0]
            self.assertTrue(entry['screenSpace'])
            self.assertIn('paletteKey', entry)
            self.assertEqual(entry['sourceWidth'], 8)
            with Image.open(root / 'pack' / entry['image']) as dds:
                self.assertEqual(dds.size, (32, 16))
                self.assertEqual(dds.getpixel((31, 15)), (17, 255, 43, 255))

    def test_reject_nonuniform_scale_before_writing_pack(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            data = self.export(root)
            Image.new('RGBA', (32, 8)).save(root / data['textures'][0]['image'])
            with self.assertRaisesRegex(ValueError, 'uniform integer scale'):
                build_project(root, root / 'pack')
            self.assertFalse((root / 'pack').exists())

    def test_reject_path_escape(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            data = self.export(root)
            data['textures'][0]['image'] = '../outside.png'
            (root / 'textures.json').write_text(json.dumps(data))
            with self.assertRaisesRegex(ValueError, 'escapes'):
                build_project(root, root / 'pack')

    def test_existing_project_edits_are_not_overwritten(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            data = self.export(root)
            image = root / data['textures'][0]['image']
            image.write_bytes(b'user artwork')
            with self.assertRaisesRegex(ValueError, 'already exists'):
                export_project([], root)
            self.assertEqual(image.read_bytes(), b'user artwork')

    def test_car_detail_retains_separate_live_palette_baseline(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            data = self.export(root)
            record = data['textures'][0]
            record.update(replacementMode='paletteDetail', screenSpace=False, baseline='baseline.png')
            del record['paletteKey']
            Image.new('L', (8, 4), 80).save(root / 'baseline.png')
            Image.new('RGBA', (16, 8), (140, 140, 140, 255)).save(root / record['image'])
            (root / 'textures.json').write_text(json.dumps(data))
            build_project(root, root / 'pack')
            manifest = json.loads((root / 'pack/manifest.json').read_text())
            with Image.open(root / 'pack' / manifest['entries'][0]['image']) as dds:
                self.assertEqual(dds.getpixel((0, 0)), (140, 80, 140, 255))


if __name__ == '__main__':
    unittest.main()
