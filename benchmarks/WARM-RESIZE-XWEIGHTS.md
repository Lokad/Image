# Warm resize horizontal kernel-weight reuse

Reusing horizontal kernel weights across vertical taps reduces complete warm
bilinear resize time by 5.7%, from
9.08x to 8.65x
FFmpeg. The longer repeat's conservative upper is
8.70x. This remains the largest measured
warm gap and exceeds the 2x target; independent quiet confirmation and other
required cases remain open.

The [numeric evidence](warm-resize-xweights-evidence.json) preserves samples,
uncertainty, source/output/tool/actual DLL identities, allocations and immutable
archive hashes. Regenerate with `python benchmarks/render-warm-resize-xweights.py`.

| Stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
| Retained twelve-sample baseline | 3493.22 | 387.21 / 384.54 | 9.08x | 9.16x |
| Weight-reuse screen | 3300.72 | 384.65 / 383.47 | 8.61x | 9.94x |
| Weight-reuse twelve-sample repeat | 3294.19 | 380.90 / 383.65 | 8.65x | 8.70x |

Only Resize's horizontal kernel evaluation changes. For support of up to eight
taps, the original binary64 coefficients are computed once per output pixel and
held in eight local doubles; wider support evaluates directly. The original
source coordinate expression, kernel formula, combined weight multiplication,
tap/accumulation order, alpha rounding and exact tiny-alpha fallback remain.
The source-row span optimization remains. Work/memory/output admission,
cancellation, bounds, orientation and independent ownership keep their checks.
No heap workspace, coefficient array, pixel cache, API, dependency, unsafe code,
separable filtering or SIMD change is introduced. All other production source
bytes remain identical. Resolved IL metadata references prove Resize is the
only changed method among 535; all implementation flags remain identical.

All 171 focused controls pass before edits on both OSes, then normally and with
disabled intrinsics on both OSes. Existing four frozen scalar-output digests
cover both precisions/filtered kernels and five target shapes, including support
around the eight-tap cutoff and wider-support fallback. Rational-reference,
tiny/half-alpha, hidden RGB, exact work/memory limits, cancellation and ownership
controls remain. All 2,118 Release tests pass per OS: 533 internal, 690 public API
and 895 live FFmpeg comparisons. No new test is needed for this arithmetic-preserving
reuse primitive.

The same actual dependency-free Release package passes all three usage examples
on both OSes. Saved actual short/long statistical libraries and fresh Linux
production IL/flags match the package; fresh Windows DLL bytes match it. Three
repeats of sixteen complete public Resize/disposal operations allocate exactly
the same bytes through old/new actual packages. Thirty existing original JPEG
outputs remain exact as package correctness controls, not extra performance images.

One existing original 513x259 RGBA8 raster produces independently owned 256x129
bilinear output. Managed rational-reference pixels and native alpha are exact.
Native RGB keeps the existing filtered maximum 1 / RMSE 0.25 bound, with one
one-level float rounding-boundary difference and observed RMSE 0.003177. Native
CLI/deterministic pixels, source integrity, independent owners, fourteen negative
controls and benchmark smoke pass. The selected output has no zero-alpha pixels;
other native alpha edge cases and 16-bit/direction/filter coverage remain open.
No new raster, quality search, native-worker or reference-bound change occurs.

Both subjects time complete operations and independent owned output acquisition/
release. Managed Resize/admission/allocation/materialization/disposal remains
inside timing. Native packed/planar conversion, zscale widened triangle filtering
with straight-alpha metadata and encoded-space premultiplication/unpremultiplication,
final RGBA conversion and output pixel acquisition/release remain inside timing.
Graph/source/wrappers/geometry contexts are reused; each call filters the full
raster, exposing padded native output rows directly. Neither subject caches
output pixels. Startup, IPC, I/O, generation, hashes and validation are excluded.

Sequential Release measurement uses CPU0, one filter thread, normal SIMD,
disabled tiering and non-concurrent workstation GC on the dedicated EPYC 9V45 VM,
.NET 10.0.8 / SDK 10.0.204. One short screen receives one twelve-sample repeat;
BenchmarkDotNet retains eleven samples after excluding one outlier, while all
twelve raw measurements remain. Prequiet passes and immediate postquiet fails
under unchanged thresholds. Failed cooldowns and observations remain archived;
this is optimization evidence, not independent quiet certification.

Next is one checked destination-row acquisition per output row, replacing four
per-pixel SetSample calls while preserving rounding/write order and both depths.
Broader warm coverage and uniform parity remain open.
