# Checked vector extents

**Shared implementation rejected.** Restricting the destination vector view to
the input extent removes destination checks from the four-vector loop and shrinks
the compiled CopyRegion from 1,655 to 1,523 B. Existing callers already pass equal
chunk lengths. The vector schedule, tails, portable fallback, cancellation,
admission and owners stay unchanged. Code size alone does not establish a gain.

Independent twelve-pair comparisons improve complete byte flip **8.604 → 8.384 µs**,
ratio **.974392 (.971609–.977183)**, and overlap **3.822 → 3.732 µs**,
**.976549 (.972744–.980370)**. They retain 4096/8192 complete calls per subject,
56/0 B per operation, original pixels and independent or borrowed ownership.
These scoped DLL effects do not establish standalone reference costs.

| Rejected producer, existing complete 513×259 control | Lokad / FFmpeg µs | Conservative upper | GC B/op |
| --- | ---: | ---: | ---: |
| VerticalFlip RGBA8 | 8.422 / 8.677 | 0.974 | 56 |
| VerticalFlip RGBA16 | 17.134 / 17.851 | 0.962 | 56 |
| Separate RGBA8 copy | 2.039 / 1.935 | 1.058 | 0 |
| Separate RGBA16 copy | 3.804 / 3.788 | 1.014 | 0 |
| Overlapping RGBA8 pair | 4.481 / 3.760 | 1.198 | 0 |

The same standalone overlap harness on the prior qualified DLL gives
**3.850 / FFmpeg 3.776 µs, upper 1.024484**.
Candidate overlap is about 16.4% slower than that prior-producer control, despite
its favorable isolated pair. This material contradiction rejects the shared
implementation. No explanation from code alignment, caches or clocks is asserted;
no favorable timing retry, fixed correction or mixture of boundaries follows.

The prior DLL is restored and is the library in the already-qualified actual
package. Its new overlap measurement resolves the previous uncertainty as a
small gap, with no production change or speedup claim. The other rejected costs
above are not current results. [Current borrowed drawing](BORROWED-DRAWING-VALIDATION.md)
and [current copy scheduling](RASTER-COPY-SCHEDULING.md) remain the summary owners.

A preceding boundary audit finds no excluded native graph/scratch preparation.
Plane-copy arguments describe prevalidated resident rectangles; managed public
calls also include policy validation, clipping and cancellation. These API
differences remain explicit, without subtracting validation or changing the target.
The overlap adapter calculates all directional bands inside each timed operation.

One attribution capture saves 350 locations: 258 in CopyRegion, with 89 native or
unresolved. Collector exit is SIGTERM(-15); application and perf-script exit0.
Both idle guards and full source/canvas checks pass. Existing bytes/map are
recovered without recapture; samples including startup/JIT are only bounded
generated-code leads, not phase costs or successful performance qualification.
The initial missing-evidence preflight failed before any workload and is retained.

Linux **25 internal/190 public** focused tests pass normal/scalar. One core and
embedded build, zero public/harness/helper/native/fixture/Windows builds.
No broad/package campaign. Qualified source/core/embedded/public bindings and
manifest are restored byte-exactly without rebuilding; actual package is unchanged.
Reuse 6,696 full/live checks per OS and SAME-package/all-three-example proofs.
No new capacity or retention; physical peaks remain unmeasured. No renderer
dependency on rejected results; only the valid prior-producer overlap cost is selected.

Raw vector-extent-copy-screen.tar.xz, 1,959,648 B/209 members,
SHA256 9c39d70ea48cf4ac1a24e439e44fcb2affd8a78c41a7d2539a15a9c95a47557f, mirrored/member-verified. It retains sources, immutable
producers, original helpers, all five rejected reference captures, the two
independent confirmations, prior-producer control, partial attribution and cleanup.
[Current overlap evidence](borrowed-overlap-current-evidence.json) pins the selected
measurement and reused qualification. No production change is accepted in this round.
