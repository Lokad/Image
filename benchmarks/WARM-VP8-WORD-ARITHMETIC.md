# Exact word-based VP8 interval coding

Compressed-alpha cost rows below are historical after [indexed histogram replay](WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md). Opaque VP8 evidence remains valid.

Effort2 diagram independently measures 4.028 ms: 1.603× reference time (upper
1.614×), exact 7,330 B and 741,375 B/op. Elapsed is 6.53% below the preceding
independent mean; separate runs are not a paired baseline gain. Photograph
independently measures 16.027 ms, 1.649×/upper 1.655, 809,072 B/op and exact
17,688 B. Both retain all 12 actual/filtered samples. Alpha's wide short is
resolved by one cached confirmation: 1.385×/upper 1.395, 12 actual/11 filtered.
Conservative bounds include all actuals and both native brackets.

| Existing quiet one-CPU control | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| DiagramLossyDistortion8 short | 4,085.99 | 2,520.34 | 1.621 / 1.943 | 741,375 | 7,330 |
| DiagramLossyDistortion8 confirm | 4,028.03 | 2,512.42 | 1.603 / 1.614 | 741,375 | 7,330 |
| PhotographLossy8 short | 16,753.23 | 10,089.14 | 1.661 / 1.871 | 809,072 | 17,688 |
| PhotographLossy8 confirm | 16,027.22 | 9,721.50 | 1.649 / 1.655 | 809,072 | 17,688 |
| DiagramLossy8 short | 8,698.88 | 5,832.06 | 1.492 / 1.574 | 741,137 | 7,100 |
| DiagramLossyTrellis8 short | 16,011.45 | 9,794.12 | 1.635 / 1.968 | 754,344 | 6,818 |
| vp8 short | 76,817.62 | 54,653.28 | 1.406 / 3.253 | 23,258,184 | 98,590 |
| vp8 confirm | 74,746.58 | 53,985.33 | 1.385 / 1.395 | 23,258,184 | 98,590 |
| EncodeOpaque16 short | 24,231.22 | 15,598.42 | 1.553 / 1.590 | 1,201,552 | 72,380 |

The [VP8 integer interval definition](https://www.rfc-editor.org/rfc/rfc6386.html#section-7.2)
is unchanged. The independently factored writer retains pending arithmetic bits
in a UInt64, publishes 32 bits at a time, and propagates an escaped carry through
previous complete bytes. Pending bits reach at most 38 before publication; the
selected interval stays within its predecessor, bounding the accumulator to
47 bits including carry and eight residual bits. Residual/alignment/lookahead
termination remains canonical. No upstream source or implementation tables.

Output capacity is still admitted at logical byte boundaries before allocation;
replacement overlap, direct cancellation, terminal refusal and awaited async
lifetimes remain. No added buffer/cache/owner or admission abstraction. Coarse
128 B writer metadata remains conservative. Fixed state increases observed
allocation by 16–33 B/op. Complete normalization, fresh output ownership/delivery
and release stay inside equivalent one-CPU operation boundaries.

Both systems pass 4,950 Release checks (1,968 internal, 2,035 public, 947 live),
629 scalar WebP and 60 focused checks. The same actual private 0.2.0 package and
all three usage examples pass both systems. One production and linked-test build
per OS serve the bounded round. Published 0.1.0 remains unchanged.

Private proofs compare 1,048,576 integer states and 8,192 complete partitions with
the original bit-by-bit definition; all 15 native controls agree. A new frozen
length/packet hash exercises every probability, seeded symbols, charged capacity
and disposal. Existing carry/termination, exact output limits, local/shared
refusal, replacement/cancellation cleanup and later success checks remain.

[Evidence](warm-vp8-word-arithmetic-evidence.json) retains every sample, guard,
producer/package/source/native/fixture identity and the mirrored/member-verified
raw master. Workers, harnesses and selectors are reused; no profile or image/
settings grid. Other affected VP8 costs are historical; unrelated paths retain
valid evidence. Stop word/fragment variants. Full 255-path warm upper ≤1× and
replacement readiness remain open.
