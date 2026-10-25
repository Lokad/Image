# Exact SIMD JPEG row conversion

Existing 513×259 color controls retain frozen inputs/settings and exact complete
JPEG packets, with only the approved COM attribution difference. Complete
normalization, encoding, fresh output acquisition and release use one CPU.
The main RGBA16 control borrows a caller IMemoryBudget root of 32 MiB.

| Quiet VM run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op | Packet B managed/native |
|---|---:|---:|---:|---:|---:|---:|
| Governed RGBA16 short | 3,288.28 | 1,837.32 | 1.790/1.975 | 804,635 | 52,115/52,117 |
| Governed RGBA16 independent | 3,289.84 | 1,838.76 | 1.789/1.799 | 804,635 | 52,115/52,117 |
| rgba8 | 3,105.04 | 1,770.55 | 1.754/2.020 | 373,416 | 52,091/52,093 |
| rgba16 | 3,231.33 | 1,839.60 | 1.757/2.042 | 373,416 | 52,115/52,117 |
| rgba422 | 3,805.19 | 2,345.20 | 1.623/1.894 | 515,072 | 71,166/71,168 |
| caller420 | 3,101.43 | 1,579.65 | 1.963/2.333 | 373,424 | 52,091/52,093 |

Independent confirmation saves **13.7% elapsed** against
[the preceding confirmation](WARM-JPEG-BUFFERED-ENTROPY.md), with unchanged
804,635 B/op and observed 1,704,856 B shared-root peak, including 1,062,936 B source.
Both idle-v2 guards pass. The other rows reconcile costs without independent
speedup claims. Timing scope is the existing odd-width controls; paired/even
matrix conversion is correctness-qualified without a new CPU claim.

Eight AVX2 lanes preserve every matrix rounding stage.
Rows use the existing admitted tile; all luma precedes in-place chroma. Area
filters, scalar/tiny edges, work, admission, cancellation and ownership stay
unchanged. No extra owned storage. The full RGB domain and 112,128 row controls
remain exact; the row/guard/zero-allocation regression is now a durable test.

Both platforms pass 5,128 checks each, including 947 live comparisons, 5 scalar
matrix/area/table checks and 148 scalar public JPEG checks. The same actual
private 0.2.0 Release package and all three examples pass both platforms.

Caller measurement first stopped at stale derived managed-decoder metadata.
Only that fingerprint/agreement was refreshed: the same native pixels now match
exactly. Native packet/pixel hashes, source quality, inputs, settings and gates
remain unchanged. The successful timing/cleanup was retained after a receipt
schema correction, without remeasurement or benchmark/native/host rebuild.

[Evidence](warm-jpeg-vector-row-matrix-evidence.json) records identities,
allocation, compressed sizes and reuse. Other affected 420/422 timings stay
historical; 444/grayscale, decoding, other codecs, editing and text reuse valid
evidence. Full 257-case/1× and replacement readiness remain open.
