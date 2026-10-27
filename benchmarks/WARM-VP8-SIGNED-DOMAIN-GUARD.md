# Exact VP8 coefficient-domain validation

The existing photograph control independently measures 15.483 ms, 1.590× the
reference (upper 1.610×), with the exact 17,688 B packet and 809,072 B/op.
Elapsed is 3.40% below its preceding independent mean; separate runs are not a
paired baseline gain. All 12 actual/filtered samples and both native brackets
are retained. Full warm upper ≤1× remains open.

| Existing quiet one-CPU control | Lokad µs | Ratio / upper | Allocated B/op | Both packet bytes |
| --- | ---: | ---: | ---: | ---: |
| Photograph effort4 short | 15,570.38 | 1.602 / 1.774 | 809,072 | 17,688 |
| Photograph effort4 independent | 15,482.66 | 1.590 / 1.610 | 809,072 | 17,688 |
| Diagram effort2 short | 3,994.60 | 1.589 / 1.725 | 741,375 | 7,330 |
| Diagram effort4 short | 8,373.77 | 1.437 / 1.567 | 741,137 | 7,100 |
| Diagram effort6 short | 15,753.29 | 1.606 / 1.856 | 754,344 | 6,818 |
| VP8 plus alpha short | 73,032.84 | 1.363 / 1.590 | 21,353,192 | 98,590 |
| Opaque RGBA16 short | 23,860.60 | 1.544 / 1.571 | 1,201,552 | 72,380 |

Only the AVX2 entry guard in `Vp8BlockQuantization.Apply` changes: two signed
comparisons validate the complete [-16320, 16320] coefficient domain before any
store. A private exhaustive proof checks every signed16 value at every lane,
including -32768: 1,048,576 exact comparisons. The portable scalar guard,
quantization arithmetic, span/matrix overlap checks and checked valid-prefix
publication remain. No new storage or admission abstraction is introduced.

Both systems pass 4,959 Release checks (1,977 internal, 2,035 public, 947 live),
638 scalar WebP checks and 20 initial focused checks. Invalid endpoints are
checked at every position, with exact exception parameters and neither output
mutated. Existing native coefficient/packet hashes, malformed matrices,
saturation, overlap and allocation tests remain. The same actual private 0.2.0
package plus three usage examples passes both systems. Published 0.1.0 is unchanged.

Complete operations retain normalization, equivalent delivery, fresh output
ownership and release. Allocation, direct cancellation, terminal refusal,
capacity-before-allocation and awaited async ownership remain unchanged.
One production and linked-test build per OS is reused throughout; no new profile,
native worker, harness, selector or image/settings grid. The retained photograph
attribution supplies the lead, separately from the closed quantization kernels.

[Evidence](warm-vp8-signed-domain-guard-evidence.json) retains samples, guards,
producer/package identities and the mirrored, member-verified raw master.
Six affected VP8 anchors replace preceding costs; other affected stream/async
observations remain historical. VP8L-only encoding, decoders, other codecs and
editing/text keep applicable evidence. Stop domain-guard variants. The subsequent
[lossless photograph confirmation](WARM-VP8L-PHOTOGRAPH-CONFIRMATION.md) changes no
production code; full 255-path warm parity and replacement readiness remain open.
