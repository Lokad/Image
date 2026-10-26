# Reuse admitted WebP matcher storage

The existing 513×259 RGBA8 VP8L Q75/method4 exact=1 control retains complete
normalization/encoding/fresh independent output acquisition/release, one CPU.
The shared compressed-alpha VP8 control retains Q75/method4 and exact alpha.
No image or settings grid changed; complete packets remain raw exact.

| Quiet run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Packet bytes, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| VP8L short | 192,002.05 | 121,912.96 | 1.575 / 1.867 | 47,674,928 | 183,812 |
| VP8L independent | 192,274.46 | 121,576.90 | 1.582 / 1.607 | 47,674,928 | 183,812 |
| VP8 + alpha current short | 94,707.52 | 53,231.19 | 1.779 / 2.095 | 23,358,400 | 98,590 |

VP8L independent elapsed is 2.4% lower than its
[preceding independent run](WARM-VP8L-SPARSE-PREDICTOR.md), with 557,320 fewer
allocated bytes per operation. These are separate quiet runs, without a simultaneous
paired baseline. All 12 actuals are retained; bounds include unfiltered results
and both native brackets. Every idle-v2 guard passes. Warm 1× parity remains open.
The alpha cost screen allocates 1,066,544 fewer bytes (4.4%); elapsed is flat,
with broad short confidence and no independent speedup/parity claim.

Matcher construction uses its result array for the temporary backward chain.
Links point strictly backward; output is written right to left only after the
chain entry's final read. Sentinel entries remain intact until traversal finishes.
Signed backing storage and safe unsigned views retain every 32-bit entry pattern.
One owner admits the array before allocation; head scratch remains separately
admitted. The removed array saves four bytes per source pixel per matcher call.
The existing dense-head control's cooperative peak falls from 1,179,648 to
1,114,112 B, retaining 65,536 B of independent result capacity. This is admitted
capacity, distinct from managed allocation above. Refusal/cancellation cleanup,
borrowed inputs/root, result lifetime and asynchronous users remain unchanged.

Thirteen primitive indices were frozen before adoption, then checked on both OS
and scalar execution. Digest:
155b0e2308fa02397573b324cfd1e00ebfa35ebf0a7dda4502faef00ddf66cdd.
Existing native index/box/maximal-match, aggregate replacement/refusal, unsigned
view, disposal and recovery tests remain. Both OS pass 4,908 Release checks each,
including 947 live FFmpeg comparisons, plus 229 affected scalar checks each.
The SAME actual private 0.2.0 package and all three usage examples pass both OS;
published 0.1.0 is unchanged. Unaffected scalar/Vector256 proofs are reused.

An initial span-indexed representation had a confirmed lossless CPU cost and was
replaced after one bounded comparison. Its raw measurements and qualification are
retained; no second implementation remains. One selected immutable core serves
the accepted runs and qualification; prepared native/benchmark/generated hosts
are reused. [Evidence](warm-vp8l-chain-capacity-evidence.json) records producer,
package, raw archive, rejected representation and precise invalidation.
Stop storage tuning; the full 253-path warm 1× and replacement objective stay open.
