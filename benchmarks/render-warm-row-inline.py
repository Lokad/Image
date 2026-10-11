"""Render the single-raster row-access inlining measurements."""
import json, statistics
from pathlib import Path

root = Path(__file__).resolve().parent
v = json.loads((root / 'warm-row-inline-evidence.json').read_text(encoding='utf-8'))
rows = []
for label, r in [('Baseline screen', v['baseline']), ('Inlining screen', v['candidate'][0]),
                 ('Inlining twelve-sample repeat', v['candidate'][1])]:
    native = [statistics.mean(s['NanosecondsPerOperation'] for s in r[k]) / 1000
              for k in ('nativeBefore', 'nativeAfter')]
    rows.append(f"| {label} | {r['managedStatistics']['Mean']/1000:.2f} | "
                f"{native[0]:.2f} / {native[1]:.2f} | {r['worstBracketRatio']:.2f}x | "
                f"{r['conservativeRatioUpper']:.2f}x |")
text = f'''# Warm bilinear row-access inlining

One `ImageBuffer.Row` inlining hint reduces the complete warm bilinear resize
mean by {v['screenedMeanGainPercent']:.1f}%, from the first screen's
{v['baseline']['worstBracketRatio']:.2f}x to {v['candidate'][1]['worstBracketRatio']:.2f}x
FFmpeg. The longer repeat's conservative upper is
{v['candidate'][1]['conservativeRatioUpper']:.2f}x. This remains the largest measured
warm gap and is well above the 2x target. Independent quiet confirmation and
other required cases remain open.

The [numeric evidence](warm-row-inline-evidence.json) retains individual samples,
uncertainty, correctness, source/tool/actual DLL identities, allocations and
immutable archive hashes. Regenerate with
`python benchmarks/render-warm-row-inline.py`.

| Stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
''' + '\n'.join(rows) + '''

Only `ImageBuffer.Row` gains `AggressiveInlining`. All 535 production method
bodies are byte-identical; this method's implementation flag is the only flag
change. Disposal/row/checked-stride bounds, work/memory/output admission,
cancellation, precision, alpha/hidden RGB, geometry, rounding and independently
owned outputs keep their existing implementation. No API, dependency, table,
buffer, cache or unsafe code is introduced.

Optimized-JIT diagnostics load the saved actual baseline and candidate
statistical libraries. The baseline GetRow8 has a tail jump to Row; the candidate
inlines Row into GetRow8, which grows from 73 to 266 native bytes. Resize remains
3,335 native bytes with eight GetRow8 call sites. Both diagnostic pre/post quiet
checks pass; pixels, source integrity and two independent live owners pass.
These are code-shape diagnostics, with no additional timing or CPU attribution
claim. The previous CPU profile selected row access; no new CPU profile is taken.

All 167 existing transform/admission/resize/flip controls pass before the edit
on both OSes and after it with normal and disabled intrinsics on both OSes.
All 2,114 Release tests pass per OS: 533 internal, 686 public API and 895 live
FFmpeg comparisons. The same actual dependency-free Release package passes all
three usage examples on both OSes. Actual short/long statistical libraries and
fresh Linux production IL/flags match that package; fresh Windows DLL bytes
match it. Three repeats of sixteen complete public Resize/disposal operations
allocate exactly the same bytes through the old/new actual packages. Thirty
existing original JPEG outputs also remain exact as package correctness controls;
they are not additional benchmark images.

One existing original 513x259 RGBA8 raster produces independently owned 256x129
bilinear output. Managed pixels match the unchanged independent rational
reference exactly; native alpha is exact and native RGB retains the existing
filtered maximum 1 / RMSE 0.25 bound. One RGB sample differs by one level, with
observed RMSE 0.003177, at the previously documented float rounding boundary.
Native CLI/deterministic pixels, source integrity, independent owners and fourteen
native negative controls pass. The selected output has no zero-alpha pixels;
other alpha edge cases and 16-bit/other directions remain open. No new raster,
quality search, native worker change or reference-bound change occurs.

Both subjects time complete public operations and owned output acquisition/
release. Managed timing includes Resize/admission/allocation/materialization/
disposal. Native timing includes packed/planar conversion, the zscale widened
triangle kernel with straight-alpha metadata and internal encoded-space
premultiplication/unpremultiplication, final RGBA conversion and output pixel
acquisition/release. Graph/source/frame wrappers and geometry contexts are reused;
each call filters the full raster, with padded native rows exposed directly.
Neither subject caches output pixels. Startup, IPC, I/O, hashing, generation and
validation are excluded. Sequential Release measurement uses CPU0, one filter
thread, normal SIMD, disabled tiering and non-concurrent workstation GC on the
dedicated EPYC 9V45 VM, .NET 10.0.8 / SDK 10.0.204.

The initial candidate screen fails its quiet guard before spawning a timed
child. Its terminal receipt and absence of a live worker are verified before a
fresh attempt. One successful short screen and one twelve-sample repeat follow;
BenchmarkDotNet retains eleven samples in the latter statistics after excluding
one outlier, while all twelve raw measurements remain. Statistical prequiet
checks pass and immediate postquiet checks fail under unchanged thresholds.
Failures and cooldown observations are retained. The measured improvement guides
the next iteration; independent quiet confirmation remains required.

The next primitive is acquiring a checked source-row span once per nonzero
vertical tap and reading all RGBA components from it. Preserve the original
weight/tap order, arithmetic, exact-alpha fallback, work and cancellation checks.
Use this same single matched pair for a short screen before any longer repeat.
'''
(root / 'WARM-ROW-INLINE.md').write_text(text, encoding='utf-8')
