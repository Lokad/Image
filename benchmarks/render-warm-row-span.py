"""Render the single-raster filtered source-row acquisition evidence."""
import json, statistics
from pathlib import Path

root = Path(__file__).resolve().parent
v = json.loads((root / 'warm-row-span-evidence.json').read_text(encoding='utf-8'))
rows = []
for label, r in [('Retained twelve-sample baseline', v['baseline']),
                 ('Row-span screen', v['candidate'][0]), ('Row-span twelve-sample repeat', v['candidate'][1])]:
    native = [statistics.mean(s['NanosecondsPerOperation'] for s in r[k]) / 1000
              for k in ('nativeBefore', 'nativeAfter')]
    rows.append(f"| {label} | {r['managedStatistics']['Mean']/1000:.2f} | "
                f"{native[0]:.2f} / {native[1]:.2f} | {r['worstBracketRatio']:.2f}x | "
                f"{r['conservativeRatioUpper']:.2f}x |")
text = f'''# Warm filtered source-row acquisition

Acquiring one checked source-row span per nonzero vertical tap reduces complete
warm bilinear resize time by {v['screenedMeanGainPercent']:.1f}%, from
{v['baseline']['worstBracketRatio']:.2f}x to {v['candidate'][1]['worstBracketRatio']:.2f}x
FFmpeg. The longer repeat's conservative upper is
{v['candidate'][1]['conservativeRatioUpper']:.2f}x. This remains the largest measured
warm gap and exceeds the 2x target; independent quiet confirmation and other
required cases remain open.

The [numeric evidence](warm-row-span-evidence.json) preserves raw samples,
uncertainty, source/output/tool/actual DLL identities, allocations and immutable
archive hashes. Regenerate with `python benchmarks/render-warm-row-span.py`.

| Stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
''' + '\n'.join(rows) + '''

Only the filtered source-sample acquisition in ImageTransforms.Resize changes.
It obtains one checked 8-bit or 16-bit row span outside the horizontal tap loop,
then reads all RGBA channels from that span. All other production source files
remain byte-identical. A comparison resolving IL metadata references verifies
that Resize is the only method with changed instructions among 535 production
methods; all implementation flags remain identical. The retained Row inlining
hint remains. Kernel weights, tap order, double arithmetic/accumulation, alpha
rounding and exact tiny-alpha fallback, orientation, work/cancellation/admission
checks and independently owned output keep their original implementation.
No table, heap buffer, cache, API, dependency or unsafe code is added.

All 167 existing transform/admission/resize/flip controls pass before the edit
on both OSes, then with normal and disabled intrinsics on both OSes. They include
both precisions, bilinear/bicubic/nearest paths, support edges, tiny/half alpha,
zero-alpha hidden RGB, exact working-memory/work limits, cancellation and owners.
All 2,114 Release tests pass per OS: 533 internal, 686 public API and 895 live
FFmpeg comparisons. The same actual dependency-free Release package passes all
three usage examples on both OSes. Actual short/long statistical libraries and
fresh Linux production IL/flags match the package; fresh Windows DLL bytes
match it. Three repeats of sixteen complete public Resize/disposal operations
allocate exactly the same bytes through the old/new actual packages. Thirty
existing original JPEG outputs remain exact as package correctness controls;
these are not additional benchmark images.

The initial VM preflight fails after a successful production build because its
helper path still selects the previous inlining-only verifier. Its terminal
receipt and absent worker are checked before running only the corrected tail.
The new verifier resolves metadata-token shifts before comparing instructions;
an initial raw-byte comparison also rejects a token-only change in Round.
These helper failures are retained; no statistical job is repeated. The corrected
method comparison, both focused control modes and benchmark smoke pass. No new
CPU profile or JIT diagnostic is needed for this source-level primitive.

One existing original 513x259 RGBA8 raster produces independently owned 256x129
bilinear output. Managed pixels match the unchanged independent rational
reference exactly. Native alpha is exact; native RGB keeps the existing filtered
maximum 1 / RMSE 0.25 bound, with one one-level difference and observed RMSE
0.003177 at the previously documented float rounding boundary. Native CLI/
deterministic pixels, source integrity, independent owners and fourteen native
negative controls pass. The selected output has no zero-alpha pixels; other alpha
edge cases and native 16-bit/other directions remain open. No new raster, quality
search, native worker change or reference-bound change occurs.

Managed timing includes complete public Resize/admission/allocation/
materialization/disposal. Native timing includes packed/planar conversion, the
zscale widened triangle kernel with straight-alpha metadata and internal encoded-
space premultiplication/unpremultiplication, final RGBA conversion and output
pixel acquisition/release. Graph/source/frame wrappers and geometry contexts are
reused; each call filters the full raster, exposing padded native rows directly.
Neither subject caches output pixels. Startup, IPC, I/O, hashing, generation and
validation are excluded. Sequential Release measurement uses CPU0, one filter
thread, normal SIMD, disabled tiering and non-concurrent workstation GC on the
dedicated EPYC 9V45 VM, .NET 10.0.8 / SDK 10.0.204.

One short screen receives one twelve-sample repeat. BenchmarkDotNet retains ten
samples in the latter statistics after excluding two outliers; all twelve raw
measurements remain. Statistical prequiet checks pass and immediate postquiet
checks fail under unchanged thresholds. Failed cooldowns and observations remain
archived. This is an optimization screen, not independent quiet certification.

The next narrow primitive is safe SIMD accumulation of the four independent
double-precision channel sums. Retain the original per-channel multiplication/
addition order, alpha semantics and scalar fallback, with this same single pair
as the complete-operation performance check. Broader warm parity remains open.
'''
(root / 'WARM-ROW-SPAN.md').write_text(text, encoding='utf-8')
