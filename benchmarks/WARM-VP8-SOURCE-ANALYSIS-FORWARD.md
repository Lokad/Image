# Batched forward transforms in VP8 source analysis

The photograph independently measures 22.62 ms: 2.341× reference time (upper
2.361×), 909,696 B/op and an exact 17,688-byte packet. Elapsed time is 3.58% below
the preceding independent mean; these separate runs are not a paired baseline
gain. Full warm parity remains open.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| PhotographLossy8 short | 22,656.37 | 9,714.47 | 2.332 / 2.467 | 909,696 | 17,688 |
| PhotographLossy8 confirm | 22,623.48 | 9,664.54 | 2.341 / 2.361 | 909,696 | 17,688 |
| DiagramLossy8 short | 12,478.68 | 5,927.85 | 2.105 / 2.247 | 841,608 | 7,100 |
| vp8 short | 82,233.44 | 53,529.53 | 1.536 / 1.757 | 23,358,400 | 98,590 |
| DiagramLossyTrellis8 short | 21,758.77 | 9,810.60 | 2.218 / 2.366 | 854,968 | 6,818 |

Both idle-v2 guards pass for every row. Photograph confirmation retains all twelve
actual and filtered samples. Other rows are current shorts. Frozen inputs/settings
and complete normalization, fresh output ownership and release boundaries remain.
Preparation, startup, transport, validation and ToArray are untimed. Complete
packets and registered normal/scalar reconstructed pixels remain exact.

Uncancelable source-only luma16 analysis reuses the existing four-adjacent DCT.
Its one internal entry now accepts independent source/prediction strides; existing
callers pass both explicitly. No new transform kernel or forwarding implementation.
Four histogram records retain the original block order, coefficients and counts.
Chroma8, live-token and scalar paths retain their per-block traversal. The extra
96 bytes of coefficient scratch fit existing admitted workspace; no new owner,
cache, allocation, frame copy or memory allowance is introduced.

Both systems pass 4,937 Release checks (1,955 internal, 2,035 public, 947 live),
616 scalar WebP checks and 68 focused checks. Extended helper controls cover
independent strides, offsets, signed extremes, overlapping inputs/output,
separated pairs, untouched storage, zero allocation and rejection before output.
Native coefficient, source-analysis, mode and packet identities agree. The same
actual private 0.2.0 package and three usage examples pass both systems; published
0.1.0 remains unchanged. Production built once per system; harnesses, workers and
selectors were reused.

One preflight caught the preceding core still copied into the trellis runtime.
No benchmark had started. Copying the selected DLL/PDB corrected propagation
without a rebuild; the failed preflight and correction are retained.
[Evidence](warm-vp8-source-analysis-forward-evidence.json) records all identities,
samples and the mirrored, member-verified archive. Older affected encoder costs
are historical; unrelated decoder/codec/editing/text evidence stays valid.

Stop forward/source batching variants. The next bounded lead is the unchanged
source coefficient histogram's per-value magnitude/bucket arithmetic, retaining
serial bucket updates, alias behavior and portable fallback. Prior boolean-emitter
group publication was already rejected and will not be reopened.
