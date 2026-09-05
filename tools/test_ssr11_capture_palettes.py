#!/usr/bin/env python3
"""Verify matched native captures preserve non-sky geometry and palette colors."""
import argparse
import json
import struct
from pathlib import Path


def load(path):
    data = path.read_bytes()
    if data[:8] != b'OGTWCAP\0' or struct.unpack_from('<I', data, 8)[0] != 6:
        raise ValueError('expected a version-6 native world capture')
    count, stride = struct.unpack_from('<II', data, 44)
    start, vram = struct.unpack_from('<QQ', data, 60)
    return data[:160], [data[start + i * stride:start + (i + 1) * stride]
                       for i in range(count)], data[vram:]


def palette(vram, clut):
    offset = (((clut >> 6) * 1024) + ((clut & 63) * 16)) * 2
    return vram[offset:offset + 32]


def verify(before, after):
    ah, a, av = load(before)
    bh, b, bv = load(after)
    assert ah == bh, 'captures are not the same native frame/camera'
    assert len(a) == len(b)
    unchanged_palettes = 0
    remapped = 0
    sky = 0
    for index, (old, new) in enumerate(zip(a, b, strict=True)):
        assert old[:6] + old[8:] == new[:6] + new[8:], f'geometry/material changed: {index}'
        flags, _, old_clut = struct.unpack_from('<IHH', old)
        new_clut = struct.unpack_from('<H', new, 6)[0]
        kind = struct.unpack_from('<I', old, 32)[0]
        if flags & 1:
            if kind == 3:
                sky += 1
            else:
                assert palette(av, old_clut) == palette(bv, new_clut), f'non-sky palette changed: {index}'
                unchanged_palettes += 1
                remapped += old_clut != new_clut
    return {'frame': struct.unpack_from('<Q', ah, 16)[0], 'triangles': len(a),
            'unchangedNonSkyPalettes': unchanged_palettes,
            'remappedNonSkyPalettes': remapped, 'skyTexturedTriangles': sky}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('before', type=Path)
    parser.add_argument('after', type=Path)
    args = parser.parse_args()
    print(json.dumps(verify(args.before, args.after), indent=2))
