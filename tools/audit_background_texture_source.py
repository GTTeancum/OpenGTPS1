#!/usr/bin/env python3
"""Compare authored TIM uploads and optional background model with a native capture."""
import argparse
import hashlib
import json
import struct
import sys
from collections import Counter, defaultdict
from pathlib import Path

from gt1_convert import read_gt2_gzip_member, tim_stream_members


def shared_color_seams(triangles):
    """Report original untextured edges whose two faces disagree on RGB."""
    edges = defaultdict(set)
    for flags, _tpage, _clut, vertices in triangles:
        if flags & 1:
            continue
        for first, second in ((0, 1), (1, 2), (2, 0)):
            a, b = sorted((vertices[first], vertices[second]))
            edges[(a[:3], b[:3])].add((a[-3:], b[-3:]))
    return [{'edge': edge, 'endpointColorsByFace': sorted(colors)}
            for edge, colors in sorted(edges.items()) if len(colors) > 1]


def model_audit(volume, member, data):
    """Match authored model coordinates, UVs, RGB, opcodes and palette refs."""
    model = read_gt2_gzip_member(volume, member)
    vertex_count = struct.unpack_from('<I', model, 12)[0]
    counts = struct.unpack_from('<8H', model, 16)
    sizes = (0x30, 0x38, 0x40, 0x50, 0x48, 0x58, 0x58, 0x70)
    points = [struct.unpack_from('<3h', model, 32 + i * 8) for i in range(vertex_count)]
    cursor = 32 + vertex_count * 8
    expected = []
    for stream, (count, size) in enumerate(zip(counts, sizes, strict=True)):
        quad, gouraud, textured = bool(stream & 1), bool(stream & 2), stream >= 4
        for item in range(count):
            primitive = cursor + item * size
            a, b = struct.unpack_from('<II', model, primitive)
            indices = (a & 4095, (a >> 12) & 4095, b & 4095, (b >> 12) & 4095) if quad else (a & 4095, (a >> 12) & 4095, (b >> 12) & 4095)
            packet = primitive + 8
            opcode = model[packet + 7]
            flags = int(textured) | (opcode & 2) | ((opcode & 1) << 2) | (int(gouraud) << 3)
            tpage = struct.unpack_from('<H', model, packet + (26 if gouraud else 22))[0] if textured else 0
            clut = struct.unpack_from('<H', model, packet + 14)[0] if textured else 0
            vertices = []
            for corner, index in enumerate(indices):
                uv_offset = packet + 12 + corner * (12 if gouraud else 8)
                uv = tuple(model[uv_offset:uv_offset + 2]) if textured else (0, 0)
                color_offset = packet + 4 + (corner * (12 if textured else 8) if gouraud else 0)
                vertices.append(points[index] + uv + tuple(model[color_offset:color_offset + 3]))
            expected.append((flags, tpage, clut, tuple(vertices[:3])))
            if quad:
                expected.append((flags, tpage, clut, tuple(vertices[1:4])))
        cursor += count * size
    if cursor != len(model):
        raise ValueError('background model layout size mismatch')
    count, stride = struct.unpack_from('<II', data, 44)
    start = struct.unpack_from('<Q', data, 60)[0]
    actual = []
    for i in range(count):
        triangle = start + i * stride
        if struct.unpack_from('<I', data, triangle + 32)[0] != 3:
            continue
        flags, tpage, clut = struct.unpack_from('<IHH', data, triangle)
        vertices = []
        for corner in range(3):
            vertex = triangle + 88 + corner * 96
            vertices.append(struct.unpack_from('<3h', data, vertex + 20) + struct.unpack_from('<2h', data, vertex + 12) + tuple(data[vertex + 16:vertex + 19]))
        actual.append((flags & 15, tpage if flags & 1 else 0, clut if flags & 1 else 0, tuple(vertices)))
    missing = Counter(expected) - Counter(actual)
    unexpected = Counter(actual) - Counter(expected)
    return {'member': member, 'sourceTriangles': len(expected), 'captureTriangles': len(actual),
            'missingTriangles': sum(missing.values()), 'unexpectedTriangles': sum(unexpected.values()),
            'matchesGeometryUvAndMaterial': not missing and not unexpected,
            'sourceUntexturedColorSeams': shared_color_seams(expected)}


def audit(volume, member, capture, tim_indices=None, include_model=True):
    data = capture.read_bytes()
    if data[:8] != b'OGTWCAP\0' or struct.unpack_from('<I', data, 8)[0] != 6:
        raise ValueError('expected version-6 native capture')
    vram_offset = struct.unpack_from('<Q', data, 68)[0]
    vram = data[vram_offset:vram_offset + 1024 * 512 * 2]
    if len(vram) != 1024 * 512 * 2:
        raise ValueError('capture VRAM is incomplete')
    records = []
    for index, tim in enumerate(tim_stream_members(read_gt2_gzip_member(volume, member))):
        if tim_indices is not None and index not in tim_indices:
            continue
        cursor = 8
        record = {'tim': index}
        for kind in ('palette', 'image'):
            size, x, y, width, height = struct.unpack_from('<I4H', tim, cursor)
            if x + width > 1024 or y + height > 512:
                raise ValueError('TIM upload outside VRAM')
            source = tim[cursor + 12:cursor + size]
            actual = b''.join(vram[((y + row) * 1024 + x) * 2:
                                  ((y + row) * 1024 + x + width) * 2]
                              for row in range(height))
            record[kind] = {'rect': [x, y, width, height],
                            'bytes': len(source), 'matching': source == actual,
                            'differentBytes': sum(a != b for a, b in zip(source, actual, strict=True))}
            cursor += size
        records.append(record)
    if not records or (tim_indices is not None and {r['tim'] for r in records} != set(tim_indices)):
        raise ValueError('requested TIM selection is empty or contains missing indices')
    return {'volume': str(volume), 'member': member, 'capture': str(capture),
            'captureSha256': hashlib.sha256(data).hexdigest(), 'uploads': records,
            'model': model_audit(volume, member.replace('.bsp.gz', '.bso.gz'), data) if include_model else None}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('volume', type=Path)
    parser.add_argument('member')
    parser.add_argument('capture', type=Path)
    parser.add_argument('--output', type=Path)
    parser.add_argument('--uploads-only', action='store_true', help='Skip background BSO comparison, e.g. for a course TRP stream')
    parser.add_argument('--tim', type=int, action='append', help='Compare only this TIM index; repeat for multiple uploads')
    args = parser.parse_args()
    result = audit(args.volume, args.member, args.capture, args.tim, not args.uploads_only)
    if args.output:
        args.output.write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({'uploads': len(result['uploads']),
                      'mismatchingUploads': [r for r in result['uploads'] if not r['palette']['matching'] or not r['image']['matching']],
                      'model': result['model']}))
    sys.exit(0 if (result['model'] is None or result['model']['matchesGeometryUvAndMaterial']) and
             all(r['palette']['matching'] and r['image']['matching'] for r in result['uploads']) else 1)
