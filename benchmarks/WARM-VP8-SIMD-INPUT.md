# Exact SIMD VP8 input preparation

The existing effort-2 diagram uses **13.2% less elapsed with all actual samples**
in the final independent comparison (14.3% with filtered samples). Its selected
mean is 3.382 ms, **1.345× / upper 1.423×** reference time: CPU parity remains open.
Allocation stays 741,375 B/op and the complete 7,330-byte packet is exact.

| Quiet one-CPU control | Lokad µs | Reference µs | Ratio / upper | Managed B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Effort2 baseline, final confirm | 3,946.01 | 2,517.51 | 1.567 / 1.603 | 741,375 | 7,330 |
| Effort2 SIMD, final confirm | 3,381.88 | 2,514.90 | 1.345 / 1.423 | 741,375 | 7,330 |
| Photograph effort4, short | 14,820.95 | 10,186.52 | 1.455 / 5.376 | 809,072 | 17,688 |
| Diagram effort4, short | 8,431.04 | 6,233.83 | 1.352 / 2.469 | 741,137 | 7,100 |
| Diagram effort6, short | 15,559.99 | 9,960.93 | 1.562 / 2.816 | 754,344 | 6,818 |
| Alpha RGBA8, short | 76,492.29 | 53,601.78 | 1.427 / 4.906 | 21,353,208 | 98,590 |
| Opaque RGBA16, short | 21,213.46 | 15,682.46 | 1.353 / 2.085 | 1,201,552 | 72,380 |

Frozen 513×259 inputs/settings and complete normalization, encoding, fresh output
ownership and release remain. All native brackets and idle-v2 guards pass.
Companions have wide short bounds, without independent gain or regression claims.
Native allocation is unmeasured. No content/settings grid or quality search is added.

The first short was inconclusive against a retained baseline while the native
reference slowed about 12%. One fresh unchanged baseline gives a 13.2% screen
reduction. The first independent pair gives 5.4% with overlapping intervals; one
final reverse-order pair resolves uncertainty with separated filtered/all-actual
intervals. Baseline retains 12/12 actual/filtered samples; candidate retains 12/10.
All earlier observations remain in [the evidence](warm-vp8-simd-input-evidence.json).
These separate sequential means do not establish paired statistics or general gains.

The existing trace attributes 6.15% of all samples to scalar luma and 1.99% to
alpha scanning. AVX2 now scans eight alpha lanes and computes eight opaque luma
samples using the same Q16 weights, bias and rounding. Narrowing writes exactly
eight bytes. Scalar word samples, alpha output, tails and unsupported hardware
remain. No owned storage, table, cache, scope or cancellation boundary is added.

Each OS passes 5,055 Release checks, including 947 live comparisons, 14 focused
normal/scalar tests, 27 scalar public packet controls, the SAME actual private
0.2.1 package and three usage examples. New lane/tail controls compare byte
preparation with scalar word preparation. Native plane hashes, admission refusal,
cancellation, padding, source preservation and independent lifetimes remain.

One explicit core build per OS and cached workers/selectors serve the experiment;
full/package qualification runs once after selection. The mirrored, member-verified
raw master retains all observations, candidate/baseline producers and proofs.
RGBA8 preparation costs change; other affected callers need current evidence
before pass claims. Opaque16 also refreshes the shared method's changed code layout.
Other codecs, decoders, editing/text and applicable unchanged evidence remain.
