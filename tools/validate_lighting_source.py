#!/usr/bin/env python3
"""Local static source consistency checks; does not compile HLSL or emulate D3D."""
from pathlib import Path
import re
import subprocess
import sys

root=Path(__file__).resolve().parents[1]
subprocess.run([sys.executable,str(root/'tools/sync_lighting_shaders.py'),'--check'],check=True)
gpu=(root/'native/src/world_gpu_renderer_d3d11.cpp').read_text()
checks={
    'main shader embeds lighting': '#include "../shaders/lighting.hlsl.inc"' in gpu,
    'main shader embeds shadow entries': '#include "../shaders/shadow.hlsl.inc"' in gpu,
    'material byte layout asserts present': 'sizeof(GpuMaterial)==160' in gpu,
    'vertex byte layout asserts present': 'sizeof(GpuVertex)==68' in gpu,
    'sun pass follows VRAM update': gpu.index('context->UpdateSubresource(\n        frame.vram_texture.Get()')<gpu.index('submit_lighting_pass(base,frame,lighting_frame'),
    'extra caster path built independently': 'lighting_extra_casters' in (root/'native/src/live_renderer_bridge.cpp').read_text(),
    'WARP shader gate configured': 'opengt_lighting_gpu_tests' in (root/'native/CMakeLists.txt').read_text(),
}
for name,ok in checks.items():
    print(('PASS: ' if ok else 'FAIL: ')+name)
raise SystemExit(0 if all(checks.values()) else 1)
