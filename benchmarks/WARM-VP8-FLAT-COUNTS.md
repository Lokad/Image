# Exact VP8 flat-coefficient counting

The photograph independently measures 18.19 ms, 1.873× reference time (upper
1.877×), 809,056 B/op and an exact 17,688-byte packet. Elapsed time is 6.30%
below the preceding independent mean; these separate runs are not a paired
baseline gain. The required conservative upper ≤1× remains open.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| PhotographLossy8 short | 18,404.62 | 9,702.01 | 1.897 / 1.951 | 809,056 | 17,688 |
| PhotographLossy8 confirm | 18,193.07 | 9,715.83 | 1.873 / 1.877 | 809,056 | 17,688 |
| DiagramLossy8 short | 10,702.02 | 5,832.45 | 1.835 / 2.037 | 741,121 | 7,100 |
| vp8 short | 77,878.52 | 53,316.96 | 1.461 / 1.535 | 23,258,168 | 98,590 |
| DiagramLossyTrellis8 short | 18,432.46 | 9,870.91 | 1.867 / 1.914 | 754,328 | 6,818 |
| EncodeOpaque16 short | 28,044.72 | 15,648.26 | 1.792 / 1.973 | 1,201,536 | 72,380 |

All idle-v2 guards pass. Photograph confirmation retains all twelve actual and
filtered samples; the other four affected rows are guarded shorts. Exact packets,
registered reconstruction/alpha, source preservation and independent outputs
remain. Complete operations include normalization, fresh packet growth/acquisition
and release; startup, IPC, validation and ToArray remain untimed.

A bounded pre-change trace found 47 of 717 exclusive workload samples in scalar
flatness counting. Its timings are discarded; sampling and inlining limit phase
interpretation. The helper now compares one 16-word block with zero on AVX2,
excludes the two DC bits and counts the remaining paired byte bits. This exactly
counts fifteen nonzero AC coefficients. Block early exit, empty/negative-cutoff
behavior and the original scalar fallback remain. No quantizer math, mode policy,
owner, reservation, heap, copy, cancellation or lifetime changes.

Both systems pass 4,943 Release checks (1,961 internal, 2,035 public, 947 live),
622 scalar WebP checks and 40 focused decision/frame checks. The actual private
helper also passes 798,720 checks per system with SIMD enabled and disabled:
all 65,536 occupancy patterns, signed extremes, offsets, DC exclusion, thresholds,
multiple blocks and unchanged input/guards. No test-only production API was added.
The same actual private 0.2.0 package and all three examples pass both systems.
Production was built once per system; native workers, harnesses and selectors
were reused. Published 0.1.0 is unchanged.

[Evidence](warm-vp8-flat-counts-evidence.json) retains the diagnostic trace,
identities, samples, qualification and mirrored, member-verified raw archive.
Older affected VP8 costs are historical. Unchanged decoder/VP8L/JPEG/PNG/edit/text
proof remains reusable. Stop flatness variants; full warm parity and replacement
readiness remain open under the current [finite checklist](WARM-COVERAGE.md).
