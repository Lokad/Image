# Exact uniform-boundary VP8 prediction reuse

The existing 513×259 opaque text/diagram control independently confirms20.65 ms,
3.544× reference time / upper3.557×, with unchanged841,608 B/op and exact7,100 B.
Elapsed is 6.4% lower than the preceding quiet short. This is not a paired
independent-baseline gain claim; full CPU parity remains open.

| Quiet one-CPU run | Lokad µs | FFmpeg/libwebp µs | Ratio / upper | Allocated B/op | Both packet bytes |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| DiagramLossy8 short | 20,405.95 | 5,825.79 | 3.503 / 3.599 | 841,608 | 7,100 |
| DiagramLossy8 confirm | 20,646.27 | 5,826.33 | 3.544 / 3.557 | 841,608 | 7,100 |
| PhotographLossy8 short | 34,527.25 | 9,708.98 | 3.556 / 3.789 | 909,696 | 17,688 |
| vp8 short | 92,764.36 | 53,709.74 | 1.727 / 1.892 | 23,358,400 | 98,590 |

Both idle-v2 guards pass for every row. The diagram confirmation retains all12
actual iterations. Photograph and asymmetric VP8+alpha are current short screens,
with unchanged allocations and no independent gain claim. Complete operations
include normalization/import where needed, source processing, encoding, fresh
output growth/independent packet ownership and release. No ToArray copy is timed;
preparation, validation, I/O, IPC and startup are excluded. Frozen reference settings
and complete packets remain the gates, without equal-quality-number inference.

Equal top/left/corner bytes make all ten intra4 predictions identical. Mode0 is
computed normally once; the other modes reuse its pure score, retaining their
individual headers, the nonzero-mode flat-level surcharge and strict ordered tie
decisions. Levels, pixels and nonzero flags are identical. No memoization buffers,
new owner or per-block reservation is introduced. The extra Metric/flag fits the
existing16KiB scratch. Every mode keeps a cancellation checkpoint; cancellable
and trellis traversals remain, with exact scalar fallback. General reuse was
rejected after one4.7% short improvement because its added lookup/890-byte scratch
complexity was disproportionate. That prototype is fully removed.

Windows and Linux each pass4,926 Release checks (1,944 internal,2,035 public,
947 live),605 affected scalar WebP checks and12 focused native decision/ownership
checks. Two new frozen photograph/diagram tests compare default and live-token
traversals, root/input charges, exact packets and independent surviving outputs.
The same actual private0.2.0 package and allthree usage examples pass both systems.
Published0.1.0 remains unchanged. Only the selected production candidate receives
this full qualification batch; benchmark/native/archived selectors are reused.

Other affected VP8 encoder CPU evidence remains historical until reconciled.
VP8L/JPEG/PNG, decoders/editing/text and their valid evidence are unchanged.
[Evidence](warm-vp8-uniform-predictions-evidence.json) retains all raw samples,
rejected source/binaries, precision argument, exact core/package identities and
the mirrored member-verified archive. Stop this primitive round; the larger
remaining opaqueVP8 gaps take priority. Quantization's partial scalar publication
is the next bounded code lead; old profile percentages are not current cost claims.
