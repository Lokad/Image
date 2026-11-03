# Prepared WebP clustering costs

Candidate comparisons reuse the unchanged green/cache population's exact run
model and boundaries. Sorted immutable leaf entries identify changed regions;
repair includes neighboring original runs so equal-count boundaries remain exact.
Nonnegative edits preserve the original maximum as a lower bound. Existing
sparse-color and merged-population kernels remain in use. Candidate order, ties,
metadata, reconstructed pixels and complete packets are unchanged.

| Complete resident encode | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Original RGBA8 / libwebp exact=1 | 121,549.25 | 131,901.50 | 0.922 / 0.934 | 7,476,640 | 183,812 |
| Opaque RGBA8 / libwebp exact=1 | 75,360.46 | 70,990.17 | 1.062 / 1.132 | 6,031,704 | 121,896 |
| Photograph / libwebp exact=1 | 96,646.76 | 83,608.73 | 1.156 / 1.218 | 6,561,482 | 160,032 |
| Text/diagram / libwebp exact=1 | 7,888.05 | 9,566.26 | 0.825 / 0.869 | 4,560,910 | 7,234 |
| Transparent VP8 RGBA8 / FFmpeg | 70,735.54 | 57,660.81 | 1.227 / 1.238 | 9,694,712 | 98,590 |
| Transparent VP8 RGBA16 / FFmpeg | 66,471.78 | 53,363.58 | 1.246 / 1.254 | 9,694,712 | 98,590 |

Fresh independent baseline/candidate confirmation shows the original control
saving **17.63 ms / 12.67% raw / 13.14% normalized**, with separated filtered and
all-actual raw/normalized confidence. Allocation increases **194,816 B/op**.
Original and diagram now pass and stop. Transparent controls retain gaps.
Photograph and opaque global-bracket confidence intervals cross parity. The
interleaved check below independently confirms both gaps; the table retains
its original time estimates and wider intervals.

Photograph's adverse **3.79 ms / 4.09% raw** point has overlapping raw/normalized
intervals. Transparent RGBA8 has a separated **2.02 ms / 2.94% adverse raw** point,
alongside a **5.35% slower native clock**; its normalized 2.29% improvement has
overlapping intervals. Neither establishes a causal regression or gain. All
actuals and adverse points are retained. Companions refresh costs without
incremental speedup claims against historical results.

| Separate complete-operation storage diagnostic | Added allocated B/op | Admitted peak B, unchanged |
| --- | ---: | ---: |
| Photograph | 44,112 | 3,470,059 |
| Text/diagram | 99,056 | 3,386,043 |
| Transparent VP8 RGBA8 | 99,848 | 3,847,888 |
| Authored bars/mixed-alpha patch | 7,472 | 312,716 |

Prepared arrays belong to existing physical merged blocks or the current leaf
view. Actual merges/view changes invalidate them; transfer and recycling preserve
their ownership. Capacity is admitted before allocation, stays charged through
actual users and is released at operation disposal. Idle scratch retention after
the operation is zero. There is no public cache, cross-operation pool or new
admission abstraction. The isolated four-tile capacity test now includes prepared
metadata within 32 KiB rather than 16 KiB, retaining refusal one byte below its
observed peak. Complete-operation peaks above occur in other overlapping phases
and do not increase. They exclude borrowed input; physical heap peaks and native
allocation remain unmeasured. Original/opaque governed peaks are unmeasured.

Attribution ruled out dense-view writes as a substantial opportunity. Instead,
roughly 90% of observed bank comparisons reuse an unchanged operand generation.
The isolated prepared algorithm matches 276,649 actual costs and 640 deterministic
corner controls. Span/repair counts motivated implementation, not a speed claim.
An initial full-bank implementation was exact but materially adverse on the
photograph; its sources, binaries and screens remain retained. The completed
stage preserves useful color/distance kernels and owns only one green model/map.

Windows/Linux each pass **5,506 full/live checks**, plus 304 internal and 194 public
WebP controls per normal/scalar mode. Tests cover run boundaries, large counts,
view/merge/transfer/recycle invalidation, warmed allocation, admission refusal,
subsequent use and cancellation. The **same actual Release package** and all three
examples pass both OSes, including exact sync/async packets and surviving outputs.

One immutable production/internal build per OS serves the completed stage.
Nine confirmation jobs retain twelve actuals and native brackets with quiet
guards; a fresh transparent baseline resolves the adverse short-screen point.
No scored candidate replay or workload expansion. Complete preparation, input
processing and independent output ownership remain timed. Libwebp exact=1 is
explicitly FFmpeg's encoder backend.

[Evidence](webp-prepared-clustering-costs-evidence.json) contains compact current
costs, all confidence decisions and exact raw/package identities. Full receipts,
guards, intermediate stages and feasibility projections are retained in mirrored,
member-verified archives. [Prior sparse ownership](WEBP-SPARSE-CLUSTERING-WORKSPACE.md)
remains valid history; other affected adaptive VP8L/ALPH costs are historical.
Unchanged codec, resize, reading and text algorithms reuse applicable evidence.
Full 282-path parity remains open; publication is postponed.
Raw `webp-prepared-clustering-costs.tar.xz`: 32,565,148 B / 1,466 members,
SHA256 `eb32174c05cb8d782f3a3da07b56212004a0e8ce55e1362425223674eae823fe`.

## Interleaved confidence check

Unchanged production, fixtures, compiled public benchmark methods and native
workers pass every original packet/settings/ownership gate. Twelve fixed blocks
place native sampling before and after two complete managed calls. Each ratio
uses the fastest of six adjacent individual native samples, favoring the native
reference; all twelve ratios enter the pinned BDN 99.9% interval without filtering.
Native calibration/warmup stays outside measured complete operations. This reduces
exposure to clock changes between global setup and cleanup; it does not establish
host-frequency stability or replace the table's aggregate-time estimator.

| Existing control | Mean paired ratio | 99.9% interval | Status |
| --- | ---: | ---: | --- |
| Photograph VP8L | 1.150 | 1.132–1.168 | Gap |
| Opaque RGBA8 VP8L | 1.065 | 1.056–1.074 | Gap |

This is complementary confidence evidence, not an incremental speedup or broader
claim. No production, benchmark, selector or native rebuild; one small measurement
tool needs two compile attempts to resolve a statistics reference. Windows/Linux,
scalar and same-package proof remain valid. [Interleaved evidence](webp-interleaved-confidence-evidence.json)
pins the raw pairs, original gates and producer identities. Raw
`webp-interleaved-confidence.tar.xz`: 4,454,740 B / 51 members, SHA256
`2b3cde75ef3260a8e0c75df6a02cb07f2c68c32b014896db540d27abd2285a7e`,
mirrored and member-verified. Earlier adverse/global-bracket evidence is retained.
