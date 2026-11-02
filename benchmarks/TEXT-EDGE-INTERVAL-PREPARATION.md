# Text edge-interval preparation

A shared preparation/render buffer replaces disjoint edge-pair testing with
ordered vertical-interval traversal. It retains the original edge-index order
inside each intersection calculation and the same sorted event values; scan
conversion, coverage accumulation and exact sample rounding are unchanged.
No cache, new array, public API or production dependency is introduced.

| Existing complete drawing control | Before µs | Current µs | Reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| Prepared RGBA8 | 863.54 | 360.59 | 45.27 | 7.966 / 9.721 | 83,712 |
| Prepared RGBA16 | 789.90 | 392.22 | 50.08 | 7.832 / 10.030 | 83,712 |

Independent twelve-actual confirmations follow short three-actual screens, with
fresh before/candidate runs and native brackets. Raw gains are **58.24% / 50.35%**,
reference-normalized gains **55.95% / 54.54%**. Filtered/all-sample raw and normalized
confidence separate. Native clocks shift −5.21% / +9.22%; these are retained,
not treated as managed regressions or improvements. Both comparisons retain CPU gaps.

Drawing includes raster preparation, scan conversion, composition and disposal.
The frozen DejaVu Sans layout/font are resident; canvas reset is excluded equally
before each operation. Single invocation/unroll, exact full-frame checks after
every iteration, original colors/settings and complete independent output rules
remain. Reference: explicitly labeled ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 public
pipeline. This prepared-drawing scope is not a bare shaping-engine measurement.
The broader text integration includes multiple layout calls and complete fitting.
Managed pixels remain exact; the approved sixteen-bit native visual tolerances
remain normalized 0..255 errors, with full sixteen-bit samples preserved.

Allocation rises **256 B/draw**, from 83,456 to 83,712. Admitted drawing workspace
peaks rise **44,416→48,544 B** for the Latin control: the crossing buffer is now
live during event preparation. Retained scratch remains zero. Eight existing
wrapped/Latin/Arabic/fitting frames remain exact at both depths; their separately
measured workspace peaks and borrowed-root cleanup are in the evidence. Physical
peak and native allocation are unmeasured. These small storage costs accompany
the measured elapsed-time gain, rather than implying an allocation reduction.

Diagnostic profiles first locate 60.3%/61.4% of identifiable samples in preparation
and 28.6%/29.2% in band execution. These fractions are not component times or promised
savings. Initial profile stop-handoff failures and a corrected receipt check are
retained; the already completed baseline screen was reused without rerunning it.
No production or reference retuning and no favorable resampling.

Windows/Linux each pass **5,466 full/live checks**, plus 111 internal/142 public affected
checks in each normal/scalar mode. New authored crossing/touching/nested/quadratic
mixed-alpha clipped frames freeze the original exact result at both depths.
Admission refusal and cancellation at every preparation growth release owned
storage, preserve the borrowed root and allow a subsequent operation. The same
actual 0.2.1 Release package and three examples pass on both OSes; its consumer
checks both new exact frames. Published releases remain immutable; publication
is postponed. One production build per OS is reused throughout the stage.

[Evidence](text-edge-interval-preparation-evidence.json) retains every actual,
bracket, confidence interval, allocation/peak observation and producer identity.
Raw archive: text-edge-interval-preparation.tar.xz, 23,362,780 B / 4,286 members,
SHA256 51f2a6e4d249e07bb43610c0efe8179fbd84c5cf4655a8cca3799108f16c21bf,
mirrored and member-verified. Source, failed diagnostics, immutable runtimes,
traces and exact package are retained. Unchanged font/layout/fitting, standalone
compositing, codecs and resizing reuse their applicable evidence; other affected
raster costs are historical until refreshed.
