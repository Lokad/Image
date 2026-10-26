# Exact sparse WebP predictor scoring

The existing 513×259 RGBA8 control retains libwebp exact=1, Q75/method4, one CPU
and complete normalization/encoding/fresh independent output acquisition/release.
No raster, quality, effort or shape grid was added. Complete packets stay 183,812 B;
the shared compressed-VP8-alpha control stays 98,590 B.

| Quiet run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Packet bytes, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| VP8L short | 196,739.32 | 121,084.00 | 1.625 / 1.826 | 48,232,248 | 183,812 |
| VP8L independent | 196,989.06 | 121,270.80 | 1.624 / 1.648 | 48,232,248 | 183,812 |
| VP8 alpha current short | 94,953.68 | 53,366.70 | 1.779 / 2.697 | 24,424,944 | 98,590 |

Independent VP8L confirmation uses 9.8% less elapsed than the
[preceding independent run](WARM-VP8L-BOUNDED-REFINEMENT.md), with unchanged
allocation. This is a comparison of separate quiet runs, not a simultaneous
paired baseline. All 12 actuals are retained; upper bounds include unfiltered
results and both native brackets. All idle-v2 guards pass. Parity remains open.
The alpha row is a current cost screen; its broad confidence interval supports
neither a speedup/regression claim nor an independent parity claim.

Safe Vector512/Vector256 masks enumerate nonzero histogram pairs in ascending
order without narrowing counts. Only pairs whose two counts are zero are skipped;
their contributions to both sums and both entropy terms are exactly zero. Shared
green/four-plane scoring retains original Q23 precision, residual bias, integer
rounding and ties. Scalar fallback remains. No heap allocation, owner, reservation
or cache is added; cancellation still precedes input access.

A dense independent mathematical oracle passed on the preceding production code
before adoption. It covers 512 four-plane histogram pairs across five densities,
entropy thresholds, 96 mask-edge/unaligned/full-width controls, zero allocation,
borrowed ownership and cancellation. Existing 16,384 green controls retain frozen
SHA 844c370b8ffaf7560f80fb26a5debb23d948d1d806bc44cc4f6e96595aeed7c7.

Both systems pass 4,907 Release checks each, including 947 live FFmpeg comparisons,
plus 228 affected scalar and five focused Vector256-only checks each. The same actual
private 0.2.0 package and all three usage examples pass both systems. Published 0.1.0 is
unchanged. One immutable timing core serves the bounded round and Linux
qualification; native/benchmark/generated hosts and unaffected proofs are reused.

[Evidence](warm-vp8l-sparse-predictor-evidence.json) records samples, allocations,
producer/package/archive identities and precise invalidation. Earlier affected
scorer costs stay historical. Full 253-path warm 1× and replacement readiness remain
open. Stop this round; resolve other named producer uncertainties before tuning.
