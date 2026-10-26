# VP8 large-block prediction rows

Historical encoder cost: [source-analysis batching](WARM-VP8-SOURCE-ANALYSIS-FORWARD.md)
now supplies current affected timings. Prediction arithmetic remains unchanged.

The photograph independently measures 23.46 ms: 2.416× reference time (upper
2.435×), 909,696 B/op and an exact 17,688-byte packet. Elapsed time is 4.77% below
the preceding quiet short; this is not a paired independent-baseline gain claim.
Full warm parity remains open.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| PhotographLossy8 short | 23,346.74 | 9,692.72 | 2.409 / 2.507 | 909,696 | 17,688 |
| PhotographLossy8 confirm | 23,463.80 | 9,710.76 | 2.416 / 2.435 | 909,696 | 17,688 |
| DiagramLossy8 short | 13,039.56 | 5,838.87 | 2.233 / 2.335 | 841,608 | 7,100 |
| vp8 short | 82,389.91 | 53,625.42 | 1.536 / 1.637 | 23,358,400 | 98,590 |
| DiagramLossyTrellis8 short | 22,090.25 | 9,772.37 | 2.260 / 2.359 | 854,968 | 6,818 |

Both idle-v2 guards pass for every row. Photograph confirmation retains twelve
actual samples and eleven filtered results; the conservative upper also includes
all-actual uncertainty. Other rows are current shorts. Frozen inputs/settings and
complete normalization, fresh output ownership and release boundaries remain.
No preparation, startup, transport, validation or ToArray is timed. Packets and
registered reconstructed pixels agree, including normal/scalar native decoding.

Large-block prediction dispatches once per block: DC fills the block, horizontal
fills rows and vertical copies rows. True motion widens top bytes, adds each signed
left-minus-corner delta and packs with unsigned saturation. Values stay in
[-255,510], making the pack identical to the scalar clamp. The scalar fallback,
32-byte border snapshot, missing-edge defaults, DC rounding, aliases and tails
remain. No table, cache, allocation, owner or cancellation boundary changes.

Both operating systems pass 4,937 Release checks (1,955 internal, 2,035 public,
947 live), 616 scalar WebP checks and 42 focused checks. These include eight frozen
native prediction hashes across 561 macroblocks each and eight overlapping-border
controls, plus actual source-analysis, metric and mode/packet identities. The same
actual private 0.2.0 package and all three examples pass both systems; published
0.1.0 stays immutable. Production built once per system; all harnesses, native
workers and selectors were reused. [Evidence](warm-vp8-large-block-rows-evidence.json)
retains identities, samples and the mirrored, member-verified raw archive.

This encoder-only primitive changes source analysis and mode search. Older affected
encoder costs are historical; unrelated decoder/codec/editing/text evidence remains
valid. Stop prediction variants. The next bounded lead is reusing the existing
four-adjacent forward kernel in source-only luma analysis, whose source and compact
prediction have different strides. Preserve exact histogram counts, admitted
scratch and live-token/scalar traversal; add no new transform kernel or grid.
