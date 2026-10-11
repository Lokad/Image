# Warm resize destination-row acquisition

Acquiring the destination row once reduces complete warm bilinear resize time
by 16.1%, from 8.65x
to 7.25x FFmpeg. The longer repeat's
conservative upper is 7.30x. This remains
the largest measured warm gap and exceeds the 2x target; independent quiet
confirmation and other required cases remain open.

The [numeric evidence](warm-resize-destination-evidence.json) preserves raw
samples, uncertainty, source/output/tool/actual DLL identities, allocations and
immutable archive hashes. Regenerate with
`python benchmarks/render-warm-resize-destination.py`.

| Stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
| Retained twelve-sample baseline | 3294.19 | 380.90 / 383.65 | 8.65x | 8.70x |
| Destination-row screen | 2764.48 | 381.19 / 379.90 | 7.28x | 7.85x |
| Destination-row twelve-sample repeat | 2763.89 | 380.97 / 383.87 | 7.25x | 7.30x |

Only Resize's destination sampling changes. After the original per-row Work/
cancellation charge, it obtains one checked byte or ushort destination span.
Direct safe writes replace four per-pixel SetSample calls, retaining the exact
Round expressions, channel/write order, rounded alpha, both depths and complete
overwrite of the private owned result. Source-row spans, coefficient reuse,
kernel/tap/accumulation order, tiny-alpha exact fallback, work/memory/output
admission, cancellation, bounds and orientation keep their implementation.
No new heap workspace, pool, pixel cache, array, API, dependency, unsafe code,
separable filtering or SIMD change occurs. All other production source bytes
remain identical. Resolved IL references prove Resize is the only changed
method among 535; all implementation flags remain identical.

All 171 focused controls pass before edits on both OSes, then normally and with
disabled intrinsics on both OSes. Four existing exact scalar-output digests cover
both precisions/filtered kernels and five target shapes. Independent rational,
tiny/half-alpha, hidden RGB, exact memory/work limits, cancellation and owner
controls pass. All 2,118 Release tests pass per OS: 533 internal, 690 public API
and 895 live FFmpeg comparisons. No new regression is needed for this row-span
primitive with unchanged rounding and writes.

The same actual dependency-free Release package passes all three usage examples
on both OSes. Saved actual short/long statistical DLLs and fresh Linux production
IL/flags match the package; fresh Windows DLL bytes match it. Three repeats of
sixteen complete public Resize/disposal operations allocate exactly the same
bytes through old/new actual packages. Thirty original JPEG outputs also remain
exact as package correctness controls, not additional performance images.

One existing original 513x259 RGBA8 raster produces independently owned 256x129
bilinear output. Managed rational-reference pixels and native alpha are exact.
Native RGB keeps the existing maximum 1 / RMSE 0.25 filtered bound, with one
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

Sequential Release measurement uses CPU0, one filter thread, normal SIMD,
disabled tiering and non-concurrent workstation GC on the dedicated EPYC 9V45 VM,
.NET 10.0.8 / SDK 10.0.204. One short screen receives one twelve-sample repeat;
BenchmarkDotNet retains ten samples after excluding two outliers, while all
twelve raw measurements remain. Prequiet passes and immediate postquiet fails
under unchanged thresholds. Failed cooldowns and observations remain archived;
optimization evidence does not establish independent quiet certification.

Next is vertical kernel-weight reuse across an output row, retaining the original
binary64 coefficients/tap order and bounded-storage/wide-support fallback.
Broader warm coverage and uniform parity remain open.
