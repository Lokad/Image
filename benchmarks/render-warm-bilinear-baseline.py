"""Render the single-raster warm bilinear baseline and attribution."""
import json,statistics
from pathlib import Path
root=Path(__file__).resolve().parent;v=json.loads((root/'warm-bilinear-baseline-evidence.json').read_text(encoding='utf-8'))
r=v['observation'];p=v['attribution'];native=[statistics.mean(s['NanosecondsPerOperation'] for s in r[k])/1000 for k in ('nativeBefore','nativeAfter')]
row=sum(x for name,x in p['exclusiveLeafPercent'].items() if name.endswith('ImageBuffer.Row') or name.endswith('ImageBuffer.GetRow8'))
text=f'''# Warm bilinear resize baseline

The first matched bilinear downsample reaches {r['worstBracketRatio']:.2f}x FFmpeg,
conservative upper {r['conservativeRatioUpper']:.2f}x. This is the largest currently
measured warm gap. Production is unchanged. One CPU profile selects row access
as the first primitive to optimize; uniform warm parity remains open.

The [numeric evidence](warm-bilinear-baseline-evidence.json) retains samples,
uncertainty, correctness, source/tool/DLL identities and CPU attribution.
Regenerate with `python benchmarks/render-warm-bilinear-baseline.py`.

| Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| ---: | ---: | ---: | ---: |
| {r['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {r['worstBracketRatio']:.2f}x | {r['conservativeRatioUpper']:.2f}x |

`Bilinear/Down8` reuses one original 513x259 RGBA8 raster to produce one 256x129
owned raster. Managed pixels match the independent integer triangle/rational
reference exactly. Native alpha is exact. Native RGB satisfies the existing
filtered maximum 1 / RMSE 0.25 bound, with one sample differing by one level
(observed RGB RMSE {r['correctness']['nativeRgbRmse']:.6f}). This comes from float
accumulation crossing a quantization boundary: exact 169.499987506 versus native
169.500012696. A full unrounded float audit observes RGB deviation below 0.000061
and alpha below 0.000036 on this raster. Production rounding and native comparison
bounds are unchanged; this empirical audit is not a universal certificate.

The original development adapter uses FFmpeg's public filter APIs with straight
alpha metadata and a zscale widened triangle kernel. Encoded-space
premultiplication, all alpha filtering/unpremultiplication, packed/planar
conversion, final RGBA conversion and output pixel acquisition/release belong
inside the timed native operation. The graph, immutable source, frame wrappers
and geometry contexts are reused; each operation filters the full raster. Padded
rows need no extra packing copy. Managed timing covers the complete public
Resize, admission, allocation, materialization and disposal. Neither subject
caches output pixels. Startup, IPC, I/O, hashing, generation and references are
excluded. No coordinate-expression evaluator replaces the native scaler.

The native worker reproduces pinned CLI pixels exactly and checks an independent
integer reference, operation counters, source integrity and two simultaneously
live independent outputs. Fourteen negative controls, bilinear and compatibility
nearest BDN smokes, and the new focused live test on both OSes pass. Source
transparency and hidden RGB participate in alpha weighting. This selected output
has no zero-alpha pixels; tiny-alpha and zero-output cases still need additional
native coverage. The alternative float-to-byte route changes 19,751 samples,
including 16,383 alpha samples, and is not timed.

One CPU sampling profile attributes {row:.2f}% of identifiable leaf time to
ImageBuffer.Row/GetRow8, and {p['exclusiveLeafPercent']['Lokad.Image!Lokad.Image.ImageTransforms.Resize']:.2f}%
to Resize itself. Sampling includes warmup/calibration and gives no additional
parity timing claim. The next narrow trial is one row-accessor inlining hint,
retaining all method bodies, pixels, limits, cancellation and exception behavior.
The actual saved statistical and diagnostic libraries match all production
methods/flags in the previously verified actual package. The prior 2,113 Release
tests per OS and three examples through that same package on both OSes are reused;
one new focused live test passes per OS. No full suite or package repetition is
needed for this benchmark-only checkpoint.

Measurement and profiling run sequentially in Release on CPU0, one filter thread,
normal SIMD, disabled tiering and non-concurrent workstation GC on the dedicated
VM. Prequiet passes; immediate postquiet fails under unchanged thresholds.
Original observations are retained. Independent quiet confirmation and all
uncovered directions/depths/filter cases remain open. No broader parity claim is
supported by this single case.
'''
(root/'WARM-BILINEAR-BASELINE.md').write_text(text,encoding='utf-8')
