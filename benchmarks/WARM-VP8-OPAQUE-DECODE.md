# Warm opaque VP8 decoding

Opaque VP8 decoding averages **1.983× FFmpeg**, conservative upper **2.004×**,
at **9,504.58 µs** with **760,548 managed bytes allocated per operation**. Both
quietness guards pass; all twelve actuals are retained. This fixed pair remains
open because its upper exceeds two. Do not repeat unchanged timing to seek a pass.

| Stage | Lokad µs | Ratio / upper | Managed allocated B/op |
| --- | ---: | ---: | ---: |
| Three-actual screen | 9,483.19 | 1.965 / 3.479 | 760,548 |
| Independent quiet repeat | 9,504.58 | 1.983 / 2.004 | 760,548 |

The existing original 513×259 RGBA8 raster keeps every RGB byte and sets alpha
to 255, reusing its existing PNG RGB8 opacity projection. One FFmpeg CLI encode
at quality 75 / effort 4 prepares a **72,380-byte VP8 keyframe** outside timing.
No quality sweep or original raster is added. Both subjects decode identical
frozen bytes; encoder quality matching is irrelevant to this comparison.

The development workers previously required alpha-bearing containers and reject
the opaque preparation. Those untimed failures remain recorded. The decode
worker now explicitly accepts YUV420P opaque input while retaining YUVA420P and
the previous alpha path. It times software decoding/state reset, centered
bilinear chroma expansion, limited BT.601 conversion through RGBA16, no-dither
RGBA8 reduction and real scratch/output allocation and release. Opaque alpha
comes from conversion; alpha-bearing input retains its exact alpha-plane merge.
The previous alpha packet and managed/native pixels are byte-identical, and its
benchmark smoke passes. Eight native protocol/framing negative controls pass.

Normal/scalar native CLI outputs are exact. Windows/Linux managed outputs are
also exact with intrinsics enabled and disabled. Managed/native RGB disagreement
is at most **1**, RMSE **0.106899**, inside the unchanged maximum-4 / RMSE-1.5
bound. Alpha is exactly 255; geometry, unchanged source and simultaneously held
independent output checks pass. Production is unchanged: all 630 fresh/actual
method bodies and flags match the tested package. Its **2,784 tests** and **three
actual-package examples per OS** are reused.

Sequential Release runs use one pinned CPU, one codec/conversion thread, normal
SIMD, tiering off and workstation nonconcurrent GC. Complete public decoding,
independent owned output and disposal are timed. Contexts and packet/frame
wrappers may be reused; every operation decodes and converts all pixels. Startup,
I/O, IPC, encoding, fixture preparation and checks are excluded. Native allocations
are unmeasured. Pending preparation/result writes are flushed outside timing
before the five-second quietness checks.

The [numeric evidence](warm-vp8opaque-decode-evidence.json) retains both runs,
native controls, untimed rejections and source/package identities. The old native
worker and alpha input were frozen before mutation; all four actual child files,
raw results, source and package were frozen before later preparation. The
[rejected normalization round](VP8-NORMALIZATION-REJECTED.md) records one
diagnostic CPU profile and discards a 2.016× candidate, restoring all production.
Prototype a wider arithmetic window next. JPEG420 RGBA8's upper-bound
gap and the remaining material contract stay open.
