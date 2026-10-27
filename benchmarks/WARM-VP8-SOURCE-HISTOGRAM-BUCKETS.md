# SIMD buckets for VP8 source histograms

Historical producer and costs. [Opaque chroma SIMD](WARM-VP8-OPAQUE-CHROMA-GAMMA.md)
supplies current affected costs and allocations. [Zero-population aggregation](WARM-VP8-SOURCE-HISTOGRAM-ZERO-COUNTS.md)
superseded this producer's serial updates; magnitudes, bins and populations remain exact.

The photograph independently measures 21.92 ms: 2.269× reference time (upper
2.279×), 909,696 B/op and an exact 17,688-byte packet. Elapsed time is 3.10% below
the preceding independent mean; these separate runs are not a paired baseline
gain. Full warm parity remains open.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| PhotographLossy8 short | 22,005.51 | 9,722.97 | 2.263 / 2.377 | 909,696 | 17,688 |
| PhotographLossy8 confirm | 21,922.94 | 9,664.04 | 2.269 / 2.279 | 909,696 | 17,688 |
| DiagramLossy8 short | 12,541.21 | 5,825.43 | 2.153 / 2.315 | 841,608 | 7,100 |
| vp8 short | 79,864.89 | 53,231.62 | 1.500 / 1.760 | 23,358,400 | 98,590 |
| DiagramLossyTrellis8 short | 21,500.14 | 9,804.38 | 2.193 / 2.276 | 854,968 | 6,818 |

Both idle-v2 guards pass for every row. Photograph confirmation retains all twelve
actual and filtered samples. Other rows are current shorts. Frozen inputs/settings
and complete normalization, fresh output ownership and release boundaries remain.
No preparation, startup, transport, validation or ToArray is timed. Packets and
registered reconstructed pixels agree, including normal/scalar native decoding.

AVX2 prepares sixteen unsigned magnitudes, shifts by three and clamps buckets to
31. This includes `short.MinValue` exactly. Population increments remain serial,
preserving collisions and integer overflow behavior. Overlapping inputs retain
the original scalar loop and its sequential feedback; other hardware uses that
portable fallback. Original length checks precede access, and tails stay intact.
The fixed 32-byte stack scratch fits existing admission. No heap, cache, owner,
memory allowance or cancellation boundary changes.

Both systems pass 4,938 Release checks (1,956 internal, 2,035 public, 947 live),
617 scalar WebP checks and 36 focused checks. Signed extremes, bucket thresholds,
native populations/source scores, mode/packet identities, ownership and cancellation
pass. The new literal alias control freezes the existing sequential feedback
rather than a snapshot interpretation. The same actual private 0.2.0 package and
three usage examples pass both systems; published 0.1.0 stays unchanged. Production
built once per system; harnesses, workers and selectors were reused, with all
three runtime folders updated explicitly. [Evidence](warm-vp8-source-histogram-buckets-evidence.json)
retains identities, raw samples and the mirrored, member-verified archive.

Older affected encoder costs are historical; unrelated decoder/codec/editing/text
evidence remains valid. Stop histogram variants. The next bounded lead is the
frame's repeated replacement of rate plans at probability updates: retain the
operation-owned storage and regenerate only coefficient-dependent prices if
table, lifetime and refusal contracts permit it. No new table cache or allowance.
