# Exact packed VP8 quantization publication

The existing 513×259 opaque photograph independently confirms30.31 ms,
3.119× reference time / upper3.133×, with unchanged909,696 B/op and exact17,688 B.
Elapsed is 12.2% lower than the preceding quiet short. This is not a paired
independent-baseline gain claim; full CPU parity remains open.

| Quiet one-CPU run | Lokad µs | FFmpeg/libwebp µs | Ratio / upper | Allocated B/op | Both packet bytes |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| PhotographLossy8 short | 30,395.03 | 9,701.51 | 3.133 / 3.327 | 909,696 | 17,688 |
| PhotographLossy8 confirm | 30,306.33 | 9,717.06 | 3.119 / 3.133 | 909,696 | 17,688 |
| DiagramLossy8 short | 17,754.68 | 5,828.56 | 3.046 / 3.530 | 841,608 | 7,100 |
| vp8 short | 88,463.85 | 53,785.23 | 1.645 / 1.803 | 23,358,400 | 98,590 |

Both idle-v2 guards pass for every row. Photograph confirmation retains all12
actual iterations. Diagram/asymmetric VP8+alpha are current shorts, with unchanged
allocations and no independent gain claim. The frozen complete operations include
normalization/import as applicable, source processing, encoding, fresh output
growth/independent ownership and release. No redundant ToArray is timed. Startup,
preparation, validation, I/O and IPC are excluded. Same inputs/settings/reference;
packet equality establishes reconstructed-quality equivalence, not equal numbers.

AVX2 quantization now publishes raster coefficients and diagonal levels in packed
stores. Signed levels are bounded by16,384. Steps0..32,767 keep products within
Int32; every product must fit signed16 before any store. Other values use the
existing checked multiplication/narrowing/prefix path with untouched outputs.
Selectors derive from RFC6386 diagonal geometry. Two vectors occupy64bytes of
constant value metadata, with no heap arrays or per-operation owner/reservation.
Original input/overlap/domain checks, scalar fallback, cancellation and memory
admission remain. Arithmetic fallback does not retry admission.

Windows and Linux each pass4,930 Release checks (1,948 internal,2,035 public,
947 live),609 scalar WebP and30 focused quantization/decision checks. Existing
73,504 wrapped-arithmetic blocks, native coefficient/score hashes,96prefix-failure
cases, alias/tail/input checks and zero warm allocation remain exact. Four new
theory rows exercise signed-word endpoints and one-past failures at every position,
including diagonal placement and exact valid prefixes. The same actual private
0.2.0 package and allthree usage examples pass both platforms;0.1.0 is unchanged.

One production build per OS is reused through the bounded experiment and full
qualification; benchmark/native/archived selectors are not rebuilt. Older affected
VP8 CPU confirmations become historical. VP8L/JPEG/PNG, all decoders/editing/text
and their valid evidence remain unchanged. [Evidence](warm-vp8-vector-quantization-evidence.json)
records all samples, identities, range proof and mirrored member-verified raw master.
Stop this publication round. Adjacent intra16 forward-block batching is the next
bounded code lead; prior sample percentages are not current cost attribution.
