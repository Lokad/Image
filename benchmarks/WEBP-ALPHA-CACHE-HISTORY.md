# WebP alpha cache-history traversal

Normalized ALPH now discovers each pixel's first cache hit while updating missed
prefixes in the same forward traversal. Ordinary full-color encoding keeps its
maximum-width probe and binary search. Existing representation flags carry the
choice through preparation and parsing; palette, subtract-green and all-family
trials keep their existing traversal. Candidate order, cache-width ties, histogram
costs and complete packets stay exact.

Twelve independent counterbalanced pairs ×16 complete operations confirm
**665.60 µs / 1.05% saved** on the frozen 513×259 RGBA8
transparent Q75/effort4 encode: 63,556.91→62,891.31 µs,
ratio 0.989527, interval [0.985588, 0.993473].
All pairs remain. Fresh output construction/growth, complete encoding and disposal
stay timed; equal resident inputs, JIT and validation are excluded. This scoped
managed-build effect is separate from the standalone native comparisons below.

Three-pair byte/word screens lead by 0.96%/1.19%. The word control includes its
normalization and exact complete packet; it is a bounded generalization lead,
not a confirmed word speedup. Actual standalone alpha/photo three-actual screens
retain their gates before final qualification. No additional variant or completed
timing-job replay follows.

The preceding current-module audit compares every hit minimum and final cell
against independent per-width caches on both actual alpha prediction residuals.
Forward traversal implies 1,189,294→873,914 logical cache reads, with 608,449 stores
either way. The two filters differ substantially; counts are not phase times or
gain forecasts. Globally applying forward traversal would turn maximum-width
misses from one probe into ten. Eligibility uses the declared ALPH representation.

| Existing complete control | Lokad µs | Reference µs | Ratio / upper | Managed B/op | Exact packet B, both | Status |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| ordinary | 117,390.73 | 130,709.46 | 0.898 / 0.910 | 7,476,544 | 183,812 | pass |
| opaque | 76,619.02 | 75,233.20 | 1.018 / 1.035 | 5,983,328 | 121,896 | gap |
| photograph | 89,812.49 | 88,799.65 | 1.011 / 1.020 | 6,517,158 | 160,032 | uncertain |
| diagram | 8,081.16 | 10,172.38 | 0.794 / 0.813 | 4,363,930 | 7,234 | pass |
| rgba16 | 118,073.06 | 130,799.76 | 0.903 / 0.914 | 7,476,544 | 183,812 | pass |
| async | 115,329.29 | 130,980.40 | 0.881 / 0.890 | 7,476,928 | 183,812 | pass |
| alpha | 63,127.00 | 58,046.30 | 1.088 / 1.100 | 7,034,392 | 98,590 | gap |
| alpha16 | 63,691.22 | 58,310.15 | 1.092 / 1.103 | 7,033,984 | 98,590 | gap |

Each current comparison retains twelve actuals, native brackets and quiet guards.
Four lossless controls pass and stop; opaque is a measured gap. Photograph remains
uncertain (lower 0.994, upper 1.020); resolve measurement
uncertainty before assuming optimization there. Transparent byte/word remain gaps.
The six lossless references are explicitly labeled libwebp exact=1; transparent
encoding uses FFmpeg/libwebp. Frozen inputs/settings, full exact packets, quality,
alpha and independent-output gates remain. Normalization and awaited async output
stay inside their complete boundaries. Refreshed costs are current state, not
additional incremental gains.

Allocation stays **7,033,576 B/op** at equal paired batching. Existing admitted
history/cache/token-tail capacities and ownership are unchanged; no array, owner,
pool or session is added. Retained operation scratch is zero. Complete admitted/
physical peaks and native allocations are not remeasured. Standalone allocation
ceilings reflect their declared batching and are not new allocation savings.

Windows/Linux each pass **5,704 full/live checks**, 901 internal /357 public affected
checks per normal/scalar mode, and the **SAME actual Release package/all three
examples**. Eight additional test cases compare both traversals with independent
cache simulations, frozen seed choices/populations, guarded spans and dirty borrowed
tails. Borrowed reuse still allocates zero and adds no admission; source/token
prefixes survive. Live cancelable tokens keep original checkpoints and traversal.
Terminal refusal, cleanup, shared-root and asynchronous ownership contracts remain.

One immutable core/internal build per OS; zero public-test/benchmark/native/
selector builds. One coherent broad/package milestone follows selection. Common
call-site changes conservatively invalidate these eight displayed costs; other117
comparisons, including the latest wide resizing gains, reuse unchanged paths.
[Predictor runs](WEBP-PREDICTOR-RUNS.md) and
[workspace reuse](WEBP-PREDICTION-WORKSPACE.md) retain earlier scoped gains/history.
Full282-path parity remains open; publication is postponed.

[Evidence](webp-alpha-cache-history-evidence.json) pins sources, producers, every
actual, counts, complete packets and qualification. Mirrored/member-verified raw
webp-alpha-cache-history-qualified.tar.xz: 4,846,840 B / 305 members, SHA256
`0d06142905908d33f8d56fef64e797e3c8016520196ed640ebde14201fbaf88e`. Source/module snapshots, cached selectors,
guards and reproduction remain with their pinned unchanged dependencies.
