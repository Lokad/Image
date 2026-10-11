# Warm typed-row accessor inlining

Typed-row accessor inlining reduces complete warm bilinear resize time by
2.3%, from 7.07x to
6.93x FFmpeg. The longer repeat's conservative upper
is 6.99x. This remains the largest measured
warm gap and exceeds the 2x target; independent quiet confirmation and other
required cases remain open.

The [numeric evidence](warm-typed-row-inline-evidence.json) retains raw samples,
uncertainty, baseline attribution, source/output/tool/actual DLL identities,
allocations and immutable archive hashes. Regenerate with
`python benchmarks/render-warm-typed-row-inline.py`.

| Stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
| Retained twelve-sample baseline | 2701.98 | 382.33 / 384.56 | 7.07x | 7.13x |
| Typed-row inlining screen | 2635.60 | 380.32 / 382.81 | 6.93x | 8.01x |
| Typed-row inlining twelve-sample repeat | 2639.35 | 380.77 / 382.07 | 6.93x | 6.99x |

A fresh CPU sampling profile of the complete retained operation assigns
9.6% of weighted workload time to GetRow8 and the remainder to Resize.
Its 363 weighted stack segments include warmup/calibration;
this diagnostic provides attribution, without a parity timing claim. The actual
diagnostic DLL's production IL/flags match the retained actual package; exact
pixels, native validation, source integrity and independent owners pass. The
profile trace and actual DLL remain in an immutable archive.

The production change adds AggressiveInlining to GetRow8 and GetRow16. All 535
production method bodies remain byte-for-byte identical, including resolved
metadata, locals and exception handling. Only these two inlining flags change.
Depth/bounds/disposal checks, row slicing, both depths, geometry/kernel/tap/
accumulation/rounding order, alpha/tiny-alpha fallback, work/memory/output
admission, cancellation and ownership/orientation retain their implementation.
No new API, dependency, unsafe code, native binding, allocation, pool, cache,
array, workspace, separable filter or SIMD change occurs.

All 171 focused controls pass before edits on both OSes, then normally and with
disabled intrinsics on both OSes. Four frozen exact-output digests cover both
precisions/filtered kernels and five target shapes. Rational-reference pixels,
tiny/half-alpha, hidden RGB, exact memory/work limits, cancellation and owners
pass. All 2,118 Release tests pass per OS: 533 internal, 690 public API and
895 live FFmpeg comparisons. The same actual dependency-free Release package
passes all three usage examples on both OSes. Actual short/long timed DLLs and
fresh Linux production IL/flags match the package; fresh Windows DLL bytes
match. Three batches of sixteen complete public Resize/disposal operations
allocate exactly the same bytes through old/new actual packages. Thirty JPEG
outputs are package correctness controls, not extra performance images.

One original 513x259 RGBA8 raster produces independently owned 256x129 bilinear
output. Managed rational-reference pixels and native alpha are exact. Native RGB
retains maximum 1 / RMSE 0.25, with one one-level float rounding-boundary difference
and observed RMSE 0.003177. Pinned native CLI/deterministic pixels, source integrity,
independent live owners, fourteen native negative controls and benchmark smoke
pass. Selected output has no zero-alpha pixels; other native alpha edges and
depth/direction/filter coverage remain open. No new raster, quality search,
native worker or comparison-bound change occurs. Performance evidence covers
this RGBA8 downsample only, without a claim about other callers or 16-bit speed.

Both subjects time complete operations and independent owned output acquisition/
release. Managed Resize/admission/allocation/materialization/disposal remains
included. Native packed/planar conversion, zscale widened triangle filtering with
straight-alpha metadata and encoded-space premultiplication/unpremultiplication,
final RGBA conversion and output acquisition/release remain included. Graph/
source/wrappers/geometry contexts are reused; every call filters the full raster
and exposes padded native rows directly. Neither subject caches output pixels.
Startup, IPC, I/O, generation, hashing and validation are excluded.

Sequential Release measurements use CPU0, one filter thread, normal SIMD,
disabled tiering and non-concurrent workstation GC on the dedicated EPYC 9V45 VM,
.NET 10.0.8 / SDK 10.0.204. One short screen receives one longer repeat, retaining
11 of 12
actual measurements; all raw measurements remain. Prequiet passes and immediate
postquiet fails under unchanged thresholds. A failed pre-start guard launches no
benchmark child and remains archived with its receipt; no live job is retried.
Failed cooldowns also remain. This evidence does not establish independent quiet
certification. No new candidate profile or JIT diagnostic is used for acceptance.

Next is one inspection of the current Resize machine code to select the next
primitive in the remaining body. Broader warm coverage and uniform parity remain
open.
