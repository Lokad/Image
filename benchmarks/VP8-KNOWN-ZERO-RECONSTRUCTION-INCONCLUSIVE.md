# VP8 known-zero reconstruction: inconclusive

The prototype skips inverse reconstruction at two actual RDO callers using their
existing quantizer nonzero flags: luma4 after rate pruning and ordinary chroma.
All-zero dequantized coefficients preserve the prediction exactly. The general
inverse kernel, luma16/Walsh path, decisions, ties and packets remain unchanged.
No coefficient scan, owner, allocation or numerical policy is added.

| Existing complete encode | Managed µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| photograph / baseline / short | 14,842.30 | 10,496.38 | 1.414 / 1.570 | 809,072 |
| photograph / candidate / short | 13,788.39 | 9,725.31 | 1.418 / 1.557 | 809,072 |
| photograph / baseline / independent | 14,663.65 | 10,251.37 | 1.430 / 1.443 | 809,072 |
| photograph / candidate / independent | 15,249.76 | 10,544.45 | 1.446 / 1.453 | 809,072 |
| diagram / baseline / short | 7,846.47 | 5,817.58 | 1.349 / 1.456 | 741,137 |
| diagram / candidate / short | 7,625.00 | 6,272.30 | 1.216 / 2.254 | 741,137 |
| diagram / baseline / independent | 8,146.63 | 5,861.54 | 1.390 / 1.444 | 741,137 |
| diagram / candidate / independent | 7,729.97 | 5,811.29 | 1.330 / 1.343 | 741,137 |

Independent diagram time saves416.65µs/5.11%raw; both raw confidence views
separate. Its4.29%normalized point lead has overlapping confidence. Photograph
raw3.997%/normalized1.107%adverse points also overlap. Native clocks move−0.86%
and+2.86% respectively; short clocks move+7.82%/−7.35%. These mixed results do
not establish a robust complete-operation gain or causal regression. Restore the
two guards; retain every actual, adverse observation and bracket. No favorable
resampling or permanent family restriction follows.

BothOS pass842internal/357public checks per normal/scalar mode. The frozen
17,688/7,100B packets remain exact. Existing trellis tests additionally verify
the nonzero flag against dequantized coefficients for432signed-fragment/context
controls. All-zero type3 exits clear16coefficients; the type0/AC-only flag is
deliberately outside the proposed guard.

Production sources/core/public copies restore byte-for-byte to the qualified
checkpoint. The assertion remains in seven existing trellis cases, passing on
bothOS in normal/scalar modes. One core/internal build perOS for the experiment,
one internal-only restoration build; no public/harness/native/selector/package
build. Reuse valid5,615full/live and SAMEactualpackage/all-three-example proof,
plus the two previously retained cancellation cases; no fresh5,617full-run claim.

These are complete resident513×259Q75/effort4 public operations, including
normalization, preparation, fresh output acquisition/growth and disposal. Frozen
inputs/reference settings and all existing gates remain. Allocation is unchanged;
physical/admitted peaks and native allocations are not newly measured. The prior
untimed count projection is attribution, not CPU shares or speed proof.

[Evidence](vp8-known-zero-reconstruction-inconclusive-evidence.json) retains
producers, allactuals/confidence, scope and exact restoration. Mirrored/member-
verified raw `vp8-known-zero-reconstruction-inconclusive.tar.xz`: 1,900,492B /
275members, SHA256 `9dc200957e32993aff3d3344f225d386eb72275de0b6900dca5f2076735ceffb`.

The separate count archive and qualified-core/reference dependencies are pinned.
Current benchmark comparisons/package stay unchanged. Publication is postponed;
the full282-path objective remains open.
