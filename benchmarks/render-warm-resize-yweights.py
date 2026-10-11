"""Render the single-raster vertical kernel-weight reuse evidence."""
import json, statistics
from pathlib import Path

root = Path(__file__).resolve().parent
v = json.loads((root / 'warm-resize-yweights-evidence.json').read_text(encoding='utf-8'))
repeat = v['candidate'][1]
actual = len(repeat['managedActualMeasurements'])
retained = repeat['managedStatistics']['N']
rows = []
for label, r in [('Retained twelve-sample baseline', v['baseline']), ('Vertical weight-reuse screen', v['candidate'][0]),
                 ('Vertical weight-reuse twelve-sample repeat', repeat)]:
    native = [statistics.mean(s['NanosecondsPerOperation'] for s in r[k]) / 1000 for k in ('nativeBefore', 'nativeAfter')]
    rows.append(f"| {label} | {r['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {r['worstBracketRatio']:.2f}x | {r['conservativeRatioUpper']:.2f}x |")

text = f'''# Warm resize vertical kernel-weight reuse

Reusing short vertical kernel weights across each output row reduces complete
warm bilinear resize time by {v['screenedMeanGainPercent']:.1f}%, from
{v['baseline']['worstBracketRatio']:.2f}x to {repeat['worstBracketRatio']:.2f}x FFmpeg.
The longer repeat's conservative upper is {repeat['conservativeRatioUpper']:.2f}x.
This remains the largest measured warm gap and exceeds the 2x target;
independent quiet confirmation and other required cases remain open.

The [numeric evidence](warm-resize-yweights-evidence.json) preserves raw samples,
uncertainty, source/output/tool/actual DLL identities, allocations and immutable
archive hashes. Regenerate with `python benchmarks/render-warm-resize-yweights.py`.

| Stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
''' + '\n'.join(rows) + '''

Only Resize's vertical kernel evaluation changes. After the original per-row
Work/cancellation charge, compute the original vertical bounds and binary64
coefficients once per output row. Hold support up to eight taps in local doubles
and reuse across output columns; wider support retains direct kernel evaluation.
The original upper bound remains binary64. Kernel formulas, wy*wx multiplication,
tap/accumulation order, both depths, rounding, alpha/tiny-alpha exact fallback,
source/destination row spans, horizontal coefficient reuse, bounds/work/memory/
output admission, cancellation and ownership/orientation retain their behavior.
No new heap workspace, pool, array, cache, API, dependency, unsafe code, separable
filter or SIMD change occurs. All other production source bytes remain identical.
Resolved IL references prove Resize is the only changed method among 535; all
implementation flags remain identical.

All 171 focused controls pass before edits on both OSes, then normally and with
disabled intrinsics on both OSes. Four existing exact scalar-output digests cover
both precisions/filtered kernels and five target shapes, including cached and
wide-support paths. Independent rational, tiny/half-alpha, hidden RGB, exact
memory/work limits, cancellation and owner controls pass. All 2,118 Release tests
pass per OS: 533 internal, 690 public API and 895 live FFmpeg comparisons.

The same actual dependency-free Release package passes all three usage examples
on both OSes. Saved actual short/long statistical DLLs and fresh Linux production
IL/flags match the package; fresh Windows DLL bytes match it. Three repeats of
sixteen complete public Resize/disposal operations allocate exactly the same
bytes through old/new actual packages. Thirty original JPEG outputs remain exact
as package correctness controls, not additional performance images.

One existing original 513x259 RGBA8 raster produces independently owned 256x129
bilinear output. Managed rational-reference pixels and native alpha are exact.
Native RGB retains the existing maximum 1 / RMSE 0.25 filtered bound, with one
one-level float rounding-boundary difference and observed RMSE 0.003177. Native
CLI/deterministic pixels, source integrity, independent owners, fourteen negative
controls and benchmark smoke pass. The selected output has no zero-alpha pixels;
other native alpha edge cases and 16-bit/direction/filter coverage remain open.
No new raster, quality search, native-worker or reference-bound change occurs.

Both subjects time complete operations and independent owned output acquisition/
release. Managed Resize/admission/allocation/materialization/disposal remains
included. Native packed/planar conversion, zscale widened triangle filtering with
straight-alpha metadata and encoded-space premultiplication/unpremultiplication,
final RGBA conversion and output pixel acquisition/release remain included.
Graph/source/wrappers/geometry contexts are reused; each call filters the full
raster and exposes padded native output rows directly. Neither subject caches
output pixels. Startup, IPC, I/O, generation, hashes and validation are excluded.

''' + f'''Sequential Release measurement uses CPU0, one filter thread, normal SIMD,
disabled tiering and non-concurrent workstation GC on the dedicated EPYC 9V45 VM,
.NET 10.0.8 / SDK 10.0.204. One short screen receives one twelve-sample repeat;
BenchmarkDotNet retains {retained} of {actual} actual measurements, while all raw
measurements remain. Prequiet passes and immediate postquiet fails under unchanged
thresholds. Failed cooldowns remain archived; optimization evidence does not
establish independent quiet certification. No new profile or JIT diagnostic is
used in this round.

Next is a short profile of the current complete bilinear operation to select one
remaining production primitive. Broader warm coverage and uniform parity remain
open.
'''
(root / 'WARM-RESIZE-YWEIGHTS.md').write_text(text, encoding='utf-8')
