# Current async lossless WebP confirmation

End-to-end costs below are historical after [indexed histogram replay](WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md). Recorded pixel/packet/ownership proofs remain useful.

The existing 513×259 RGBA8 nonseekable async control independently measures
202.09 ms: 1.669× reference time, conservative upper 1.731×, unchanged
47,675,312 B/op and an exact 183,812-byte packet. This resolves the preceding
short's uncertainty; it is not a speedup claim. All 12 actual iterations and
both native brackets contribute to the bound, including two filtered samples.

| Existing one-CPU control | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Both packet bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| EncodeWebpAsync / fresh nonseekable output | 202,094.02 | 121,066.36 | 1.669 / 1.731 | 47,675,312 | 183,812 |

Inputs, libwebp exact=1/quality75/effort4 and complete operation boundaries are
unchanged. Normalization, encoding, staging, awaited nonseekable delivery/growth,
fresh independent output and release are timed on one CPU. Callbacks complete
immediately; startup, validation, hashing, final snapshots and real I/O are excluded.
All RGBA, alpha and hidden RGB samples and complete packets remain exact.

The immutable current producer, existing harness/native worker and fixtures are
reused. Only the exact cached selector/runtimeconfig were recovered from their
retained archive. Setup rejected an older DLL-bound validation receipt before
timing; live packet/normal+scalar pixels, ownership, resource/cancellation and sink
recovery checks were refreshed before the successful confirmation. No rebuild,
new settings grid or unrelated qualification repeat.

The [current qualification](WARM-VP8-WORD-ARITHMETIC.md) remains valid: 4,950 checks
and 629 scalar WebP checks per OS, plus the same actual Release package and three
usage examples on both systems. Production paths are unchanged. The earlier
[validation cleanup](WARM-ASYNC-WEBP-CURRENT.md) remains recorded separately.
[Evidence](warm-async-webp-confirmation-evidence.json) retains all samples, guards,
receipts and the mirrored/member-verified master, including the failed preflight.

This is the largest current independent point among the selected anchors, narrowly
ahead of photograph RDO 1.649×; full 255-path upper ≤1× remains open. Next investigate
reconstructing original clustering tile counts from borrowed token starts to
remove a duplicated histogram slab. Exact merge/map/packet decisions, admission,
cancellation and retained ownership remain gates.
