#!/usr/bin/env python3
"""Independent analytic-box fixtures for the C++ inverse-shadow solver.

No game assets, NumPy, or third-party Python packages are required. Ground
labels come from slab/box intersection, not the solver's triangle/BVH code.
"""
import argparse
import json
import math
from pathlib import Path
import random
import struct
import subprocess
import tempfile
import unittest

SOLVER = None

def direction(azimuth, elevation):
    a, e = math.radians(azimuth), math.radians(elevation)
    return math.cos(e)*math.sin(a), -math.sin(e), math.cos(e)*math.cos(a)

def blocked(origin, ray, boxes):
    for lo, hi in boxes:
        t0, t1 = 1.0, float('inf')
        for axis in range(3):
            if abs(ray[axis]) < 1e-12:
                if not lo[axis] <= origin[axis] <= hi[axis]:
                    t0, t1 = 1, 0
                    break
            else:
                a, b = (lo[axis]-origin[axis])/ray[axis], (hi[axis]-origin[axis])/ray[axis]
                t0, t1 = max(t0,min(a,b)), min(t1,max(a,b))
        if t0 <= t1:
            return True
    return False

def fixture():
    rng = random.Random(1987)
    sun = direction(135, 40)
    boxes, triangles, samples = [], [], []
    for i in range(12):
        x,z=(i%4)*600,(i//4)*600
        w,d,h=rng.uniform(25,55),rng.uniform(20,45),rng.uniform(50,110)
        boxes.append(((x-w,-h,z-d),(x+w,0,z+d)))
    faces=((0,1,3,2),(4,6,7,5),(0,4,5,1),(2,3,7,6),(0,2,6,4),(1,5,7,3))
    for group,(lo,hi) in enumerate(boxes):
        p=[(hi[0] if i&1 else lo[0],hi[1] if i&2 else lo[1],hi[2] if i&4 else lo[2]) for i in range(8)]
        for a,b,c,d in faces:
            triangles.extend([(p[a],p[b],p[c]),(p[a],p[c],p[d])])
        cx,cz=(lo[0]+hi[0])/2,(lo[2]+hi[2])/2
        for _ in range(300):
            x,z=cx+rng.uniform(-220,220),cz+rng.uniform(-220,220)
            if lo[0]-3<x<hi[0]+3 and lo[2]-3<z<hi[2]+3:
                continue
            label=int(blocked((x,-2,z),sun,boxes))
            samples.append(((x,0,z),(0,-1,0),label,group,1.0))
    return sun,triangles,samples

def evidence(path, triangles, samples):
    with path.open('wb') as f:
        f.write(struct.pack('<8sII',b'OGTSUN1\0',len(triangles),len(samples)))
        for triangle in triangles:
            f.write(struct.pack('<9f',*(v for p in triangle for v in p)))
        for p,n,label,group,weight in samples:
            f.write(struct.pack('<6fIIf',*p,*n,label,group,weight))

class InverseShadowTests(unittest.TestCase):
    def run_fit(self,triangles,samples):
        with tempfile.TemporaryDirectory() as d:
            path=Path(d)/'evidence.bin'
            evidence(path,triangles,samples)
            p=subprocess.run([str(SOLVER),str(path)],capture_output=True,text=True,check=True,timeout=90)
            return json.loads(p.stdout)

    def test_known_sun_recovers_independent_box_geometry(self):
        sun,tris,samples=fixture()
        fit=self.run_fit(tris,samples)
        self.assertTrue(fit['accepted'],fit)
        angle=math.degrees(math.acos(max(-1,min(1,sum(a*b for a,b in zip(sun,fit['direction']))))))
        self.assertLess(angle,8,fit)
        self.assertGreater(fit['score'],.94,fit)

    def test_unrelated_baked_colors_do_not_become_a_sun(self):
        _,tris,samples=fixture()
        rng=random.Random(983)
        samples=[(p,n,rng.randrange(2),g,w) for p,n,label,g,w in samples]
        self.assertFalse(self.run_fit(tris,samples)['accepted'])

    def test_absent_geometry_is_not_calibration(self):
        _,_,samples=fixture()
        self.assertFalse(self.run_fit([],samples)['accepted'])

    def test_one_object_is_not_independent_evidence(self):
        _,tris,samples=fixture()
        self.assertFalse(self.run_fit(tris,[(p,n,l,1,w) for p,n,l,g,w in samples])['accepted'])

    def test_truncated_evidence_fails(self):
        with tempfile.TemporaryDirectory() as d:
            path=Path(d)/'broken.bin';path.write_bytes(b'OGTSUN1\0'+struct.pack('<II',1,1))
            p=subprocess.run([str(SOLVER),str(path)],capture_output=True,text=True,timeout=10)
            self.assertNotEqual(p.returncode,0)
            self.assertIn('truncated',p.stderr)

if __name__=='__main__':
    ap=argparse.ArgumentParser(description=__doc__)
    ap.add_argument('--solver',type=Path,required=True)
    args,remaining=ap.parse_known_args()
    SOLVER=args.solver.resolve()
    if not SOLVER.is_file():
        ap.error('solver executable does not exist')
    unittest.main(argv=[__file__]+remaining)
