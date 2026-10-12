"""Render the single progressive 4:2:0 coverage point."""
import json,sys
from pathlib import Path
folder=Path(__file__).resolve().parent;e=json.loads((folder/'warm-jpegprogressive420-evidence.json').read_text());a,b=e['observations']
assert not e['productionChanged'] and e['thisFixedPairClearsConservativeTwo'] and not e['uniformWarmParityComplete']
rows='\n'.join(f"| {o['label']} | {o['managed']['statistics']['Mean']/1000:.2f} | {o['worstBracketRatio']:.5f}x | {o['conservativeRatioUpper']:.5f}x | {'Pass' if all(g['passed'] for g in o['quietness'].values()) else 'FAIL'} |" for o in (a,b))
text=f'''# Warm progressive JPEG 4:2:0 coverage

The existing implementation independently confirms at {b['worstBracketRatio']:.3f}x
FFmpeg, conservative upper {b['conservativeRatioUpper']:.3f}x. Both unchanged quiet
guards pass. This fixed progressive 4:2:0 point clears 2x without a production
change. Broader required, unmatched and unconfirmed warm cases remain open.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

One default lossless jpegtran rewrite converts the existing native baseline
4:2:0 qscale5/RGB0x204060 JPEG from the original 513x259 raster. The new input has
10 scans and 52,364 bytes. No source raster, geometry, quality or scan-strategy
enumeration is added. The default benchmark selection remains one Decode8 case;
Progressive420 is explicit.

The original public-libjpeg-API coefficient visitor now supports all three
4:2:0 components as well as grayscale. Its 418,640-byte canonical dumps prove
all 209,088 valid signed coefficients, component geometry and quantization
unchanged. Padded dummy blocks outside valid component dimensions are not
claimed as image coefficients. Owned decoded bytes retain the exact baseline
hash. The prior grayscale canonical dump retains its original hash. Producer
revision, original visitor/build script and binary hashes are recorded; checkout,
static library, producer and reference tools stay ignored under external/.
No upstream source, tables, binaries, documentation or corpus is tracked.

Normal/scalar Windows decoded bytes and allocation controls are exact to the
retained actual package, at 1,820,544 allocated bytes per public Stream decode
with tiering off. The adapter matches the normal native CLI exactly. Owned/native
RGB maximum3/RMSE0.579363 remain within the unchanged maximum4/RMSE1.5 bounds;
alpha is exactly opaque. Source preservation, independent held outputs and
15 invalid protocol/input/sampling/affinity controls pass. Both actual generated
timing libraries retain all 560 production methods and implementation flags
from the previously accepted actual package. No new production package or codec
optimization is needed.

The resident-input boundary includes full public decode, admission, allocation
and disposal. Native software MJPEG performs per-operation state reset, complete
decode, exact chroma-grid expansion/cropping, full-range conversion and independent
output acquisition/release, while reusing context and packet/frame wrappers.
CPU0, one codec/conversion thread, thread type zero, normal SIMD, tiering off and
nonconcurrent workstation GC remain fixed. Startup, I/O, IPC, preparation,
hashing, coefficient dumps and validation are excluded. Native brackets target
50ms and managed iterations200ms. Prepared children validate outside scored
windows with one native operation per bracket and null ratios.

One short screen is followed by one independent confirmation containing 12 actual
managed iterations and 12 native samples in each bracket. BenchmarkDotNet selects
11 results for its reported statistics. All 12 actual measurements remain
available, and a second Statistics calculation includes every one without
overhead subtraction. Both views clear 2x. The displayed conservative upper is
the larger of their bounds: filtered {b['filteredConservativeRatioUpper']:.5f}x,
all-actual {b['unfilteredActualConservativeRatioUpper']:.5f}x. Each calculation uses
its managed confidence critical value for the native bracket lower bound.
The parser preserves its existing output fields and adds the all-actual view;
earlier raw/statistical artifacts remain unchanged.

The [numeric evidence](warm-jpegprogressive420-evidence.json) retains raw actual
and filtered measurements, both statistical views, native samples, guards,
provenance and actual package/child/source identities with immutable archives.
Regenerate with `python benchmarks/render-warm-jpegprogressive420.py`; use
`--check` to verify. The [preceding refinement improvement](WARM-JPEG-REFINEMENT.md)
supplies the production implementation. See the
[reference recipe](../tools/WarmReference/README.md). No further tuning of this
passing fixed point is planned; cover one material unmeasured case and rerank.
'''
p=folder/'WARM-JPEG-PROGRESSIVE420.md'
if '--check' in sys.argv:
 assert p.read_text(encoding='utf-8')==text,'Progressive420 report differs.'
 print('Progressive420 report matches numeric evidence.')
else:p.write_text(text,encoding='utf-8',newline='\n')
