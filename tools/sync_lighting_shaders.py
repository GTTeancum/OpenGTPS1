#!/usr/bin/env python3
"""Keep editable HLSL and its dependency-free C++ embedded strings identical."""
import argparse
from pathlib import Path
import sys

def main():
    ap=argparse.ArgumentParser(description=__doc__)
    ap.add_argument('--check',action='store_true',help='check only; never write files')
    args=ap.parse_args()
    root=Path(__file__).resolve().parents[1]/'native'/'shaders'
    errors=[]
    for name in ('lighting.hlsl','shadow.hlsl'):
        text=(root/name).read_text(encoding='utf-8').replace('\r\n','\n')
        if ')OGTL"' in text:
            raise ValueError('HLSL collides with C++ raw-string delimiter')
        embedded='R"OGTL(\n'+text+')OGTL"\n'
        target=root/(name+'.inc')
        if args.check:
            if not target.exists() or target.read_text(encoding='utf-8')!=embedded:
                errors.append(str(target))
        else:
            target.write_text(embedded,encoding='utf-8',newline='\n')
    if errors:
        print('Embedded shader differs from source: '+', '.join(errors),file=sys.stderr)
        return 1
    print('Both embedded HLSL sources match.' if args.check else 'Both embedded HLSL sources updated.')
    return 0
if __name__=='__main__':raise SystemExit(main())
