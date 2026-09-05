"""Check native A/B framebuffer evidence; does not generate or edit images."""
import json
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]/'artifacts/paint-specular'
def read(name): return np.array(Image.open(ROOT/name).convert('RGBA'))
def difference(a,b):
    assert a.shape==b.shape, (a.shape,b.shape)
    return np.any(a!=b,axis=2)
def summarize(delta,car,glass):
    outside=delta.copy()
    x0,y0,x1,y1=car
    outside[y0:y1,x0:x1]=False
    glass_region=Image.new('1',(delta.shape[1],delta.shape[0]))
    ImageDraw.Draw(glass_region).polygon(glass,fill=1)
    glass_delta=delta & np.array(glass_region,dtype=bool)
    ys,xs=np.nonzero(delta)
    result={'changedPixels':int(delta.sum()),'outsideCar':int(outside.sum()),
        'windscreenInterior':int(glass_delta.sum()),
        'changedBounds':[int(xs.min()),int(ys.min()),int(xs.max()),int(ys.max())] if len(xs) else None}
    assert result['changedPixels']>0,result
    assert result['outsideCar']==result['windscreenInterior']==0,result
    return result

original=read('redrock-committed-baseline.png')
disabled=read('redrock-final-before.png')
unchanged=int(difference(original,disabled).sum())
assert unchanged==0,unchanged
redrock=summarize(difference(original,read('redrock-final-after.png')),
    (1490,680,1930,1010),[(1606,716),(1805,716),(1854,791),(1557,791)])
night=summarize(difference(read('night-before.png'),read('night-after.png')),
    # Native close-up places the bonnet boundary at rows 398-399. Stop above
    # that boundary; those rows are paint, not windscreen glass.
    (738,337,974,509),[(804,356),(902,356),(925,396),(780,396)])
print(json.dumps({'committedVsDisabledChangedPixels':unchanged,'redRock':redrock,
    'ssr5':night,'note':'Full native frames; windscreen polygons exclude border trim. All scenery, HUD and flare pixels outside the car are bit-identical.'},indent=2))
