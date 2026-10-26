# Exact empty spatial histogram pruning

The existing 513×259 text/diagram lossless WebP control independently confirms
13.19 ms, 1.362× reference time / upper 1.408×, with the exact 7,234-byte packet.
Elapsed is 40.0% lower than the preceding quiet short; this is not a paired
independent-baseline gain claim. Full CPU parity remains open.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Both packet bytes |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| DiagramLossless8 short | 13,169.00 | 9,614.17 | 1.370 / 1.957 | 45,404,900 | 7,234 |
| DiagramLossless8 confirm | 13,192.85 | 9,685.69 | 1.362 / 1.408 | 45,404,838 | 7,234 |
| PhotographLossless8 short | 133,800.97 | 83,340.00 | 1.605 / 2.159 | 29,610,520 | 160,032 |
| vp8l short | 191,649.39 | 122,403.16 | 1.566 / 1.827 | 47,674,928 | 183,812 |
| vp8 short | 92,523.60 | 53,407.61 | 1.732 / 1.948 | 23,358,400 | 98,590 |

Both idle-v2 guards pass for every row. The diagram confirmation retains all
12 actual iterations, including the filtered-out sample in the conservative
upper bound. Photograph and asymmetric controls are current screens, with no
independent gain claim. Allocations retain the same owned storage capacities;
the table reports each raw GC result, including B/op rounding differences.

Each operation includes public source processing, encoding and fresh output
growth, independent packet ownership and release, without redundant ToArray.
Reference normalization/import and independently owned packet acquisition/release
are included. VP8L uses libwebp exact=1, FFmpeg's encoder backend; VP8 uses FFmpeg
libwebp. Frozen method4/quality75 reference settings remain unchanged. Lossless
managed Quality is ignored. Complete bytes and exact reconstructed pixels are
the gates, rather than numerical quality settings. No new content/settings grid.

A bounded profile identified clustering; counts showed 1,880 of 2,145 diagram
tiles had no token starts. The encoder now marks starts in the already allocated
stats field, skipping empty-tile initialization and final zero-population sums.
Every token increments green at its start; unmarked tiles contain only zeroes.
They never participate in pair selection, and their map still inherits its
predecessor. Active bank costs, merge decisions, order and final counts are exact.
No new bitmap, allocation, cache or alternate algorithm is introduced. Admission,
capacity accounting, retained ownership and per-token/per-tile cancellation remain.

Windows and Linux each pass 4,924 Release checks (1,942 internal, 2,035 public,
947 live comparisons), 603 affected scalar WebP checks and 23 focused native-map,
population and ownership checks. Sparse tiles, all frozen merge populations,
independent retained plans, refusal, cancellation cleanup/recovery and invalid
tokens remain covered. The same actual private 0.2.0 package and all three usage
examples pass both platforms. Published 0.1.0 is unchanged.

Clustering consumers include VP8L and compressed VP8 alpha, so their earlier cost
confirmations are historical. The current screens above reconcile the photograph
and asymmetric anchors; other affected named boundaries remain open. JPEG/PNG,
all decoders, editing and text retain their valid evidence. No unrelated test,
package, tool or timing rebuild was required during the bounded experiment.

[Evidence](warm-vp8l-empty-histograms-evidence.json) retains all samples, allocation
figures, exact packet/core/package identities, the prior profile/count lead and
mirrored member-verified raw archive. Stop this primitive round; resolve remaining
named uncertain measurements before selecting the next substantial optimization.
