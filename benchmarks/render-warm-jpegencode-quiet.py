"""Render the retained matched JPEG encode pair and independent quiet confirmation."""
import json
import math
import re
import sys
from pathlib import Path

folder = Path(__file__).resolve().parent
e = json.loads((folder / 'warm-jpegencode-quiet-evidence.json').read_text())
a, failed, b = e['observations']
assert not e['productionChanged'] and e['thisFixedPairClearsConservativeTwo'] and not e['uniformWarmParityComplete']
assert not all(g['passed'] for g in failed['quietness'].values())
gc = re.fullmatch(r'// GC:\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)', b['managed']['rawGc'])
assert gc
allocated = math.ceil(int(gc[1]) / int(gc[2]))
v = b['receipt']
rows = '\n'.join(f"| {label} | {o['managed']['statistics']['Mean']/1000:.2f} | {o['worstBracketRatio']:.5f}x | {o['conservativeRatioUpper']:.5f}x | {'Pass' if all(g['passed'] for g in o['quietness'].values()) else 'FAIL'} |"
                 for label, o in [('Quiet screen', a), ('Guard-failed confirmation', failed), ('Independent quiet confirmation', b)])
text = f'''# Warm matched JPEG encode: quiet confirmation

The retained complete public JPEG encoder independently confirms at
{b['worstBracketRatio']:.3f}x FFmpeg, conservative upper
{b['conservativeRatioUpper']:.3f}x. Both unchanged quiet guards pass. This fixed
pair clears 2x; full required warm coverage remains open. Production is unchanged.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

One existing original 513x259 RGBA8 raster and its original matched preparation
are reused. Both subjects flatten alpha once onto RGB 0x204060 and encode
opaque 8-bit 4:4:4 JPEG. Managed quality {v['managedQuality']['Setting']} and native
qscale {v['nativeQuality']['Setting']} retain source PSNR gap {v['psnrGapDb']:.5f} dB,
within the unchanged 0.25 dB band. Bitstreams, decoder bounds, alpha, source and
independent outputs are freshly checked before/after; preparation bytes remain
identical. No quality, effort, shape or source-raster enumeration.

Managed fixed Huffman output is {v['managedOutputBytes']:,} bytes; native optimal
Huffman output is {v['nativeOutputBytes']:,} bytes. This is quality matching, not
equal compression efficiency. Complete operations include single-round alpha
flattening, full-range BT.601 conversion, encoding, independent output acquisition
and release. Native codec/conversion/scratch/frame/packet wrappers are reused;
every pixel is reprocessed. Fresh managed MemoryStream creation/growth and
disposal are timed; no redundant ToArray. Startup, I/O, IPC, preparation,
quality enumeration and validation are excluded.

The first 12-actual confirmation fails post-load
{failed['quietness']['after']['loadAverage'][0]:.6f} above the unchanged 0.25 limit.
It stays failed and is frozen before one bounded fresh attempt. The fresh process
reuses the same four prepared child identities and waits for starting load<=0.01;
code, input, settings, targets and guards remain unchanged. Its 12 actual/12
filtered managed measurements and 12 native samples per bracket pass both guards.
The upper takes the larger filtered/all-actual bound:
{b['filteredConservativeRatioUpper']:.5f}x / {b['unfilteredActualConservativeRatioUpper']:.5f}x.
All original actuals and failed guards remain retained.

Managed allocation is {allocated:,} B/op from raw GC bytes / operation count,
rounded up and including harness overhead. Native allocation totals are unmeasured.
CPU0, one codec/conversion thread, thread type zero, normal SIMD, disabled tiering
and nonconcurrent workstation GC are fixed on the dedicated reference VM.

Both actual timing children and the fresh Windows build match all 565 production
method bodies/flags in the accepted actual package. The preceding 2,385 tests per
OS and same-package/all-three-example checks remain applicable. There is no
production or benchmark implementation change. The
[numeric evidence](warm-jpegencode-quiet-evidence.json) retains all measurements,
quality/source/input/output/child/package identities, guards and immutable archive
digests. Regenerate with `python benchmarks/render-warm-jpegencode-quiet.py`;
`--check` verifies this report. See the [current summary](../docs/benchmark.md)
and [reference recipe](../tools/WarmReference/README.md). Stop tuning this passing
pair and prioritize the largest remaining gap.
'''
output = folder / 'WARM-JPEG-ENCODE-QUIET.md'
if '--check' in sys.argv:
    assert output.read_text(encoding='utf-8') == text
    print('JPEG encode quiet report matches numeric evidence.')
else:
    output.write_text(text, encoding='utf-8', newline='\n')
