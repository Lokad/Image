# JPEG row-pair experiment

Neither implementation is selected. Exact matrix symmetry removes repeated row
products, but the complete-operation evidence does not establish a robust elapsed
gain. Qualified production sources, assemblies and package are restored exactly.
Current JPEG costs remain owned by [plain-reader publication](JPEG-PLAIN-READER.md).

| Existing full RGB decode, independent twelve actuals | Managed µs | FFmpeg µs | Managed B/op |
| --- | ---: | ---: | ---: |
| Qualified original | 1,801.70 | 1,451.29 | 10,088 |
| A: adaptive adjacent row pairs | 1,801.64 | 1,450.17 | 10,088 |
| B: fixed four pairs and exact DC-row repair | 1,762.01 | 1,450.97 | 10,088 |

A is inconclusive. B's 39.69µs/2.20% raw point improvement has overlapping
conservative raw and normalized intervals in both filtered/all-actual views. Its
short-screen point is not independent gain evidence. Sparse-CMYK independent
points530.24/532.51µs also provide no all-views raw/normalized gain; B's native
reference moves+2.92%, so its improved normalized ratio is not a raw CPU claim.

One targeted same-process comparison addresses the missing comparable-clock
evidence: twelve adjacent pairs,32 complete operations per subject, alternating
order and no filtering. It observes **1,789.46→1,856.54µs**, ratio**1.03749**, log-ratio
interval mapped to**1.03176–1.04324**. Both builds have isolated load contexts and
independent identical inputs/outputs; source/JIT/validation setup is excluded on
both sides. This is an adverse scoped result, not a new FFmpeg comparison or a
general standalone regression claim. The contradiction precludes selecting B;
no further sampling follows. One small private runner adapts the established
counterbalanced protocol to the exact ordinary span boundary.

The original6630-block input makes23558 full-row pairs eligible, reducing logical
row multiply-adds48.14%; shuffle/load/branch costs are omitted from that count.
Independent8321 row-pair comparisons and5120 mixed full-matrix blocks preserve Q14
reflection, overflowing Int32 sums, shifts, signed saturation, DC word-wrap and
lane order. The full oracle first matches the existing640-basis/2048-dense hashes.
Actual production readers retain their original managed pixels and native bounds.

Both stages pass93 internal/375 public checks per normal/scalar mode on each OS,
including the actual mixed-row dispatch. No operation storage is added;128 bytes
of fixed vector data is the only proposed retained addition. Columns, scalar,
dequantization, cancellation checkpoints and ownership are unchanged. B keeps the
two-low-row arithmetic and repairs constant rows before columns can observe them.
No encoder, metadata or numerical-policy change is involved.

All original/A/B source and main/embedded modules, tests, setup/outputs, guards and
measurements are retained. Restoring the original sources/modules/manifest and
public/differential bindings reuses5,850-check/scalar/same-actual-package proof;
no rebuild, broad suite or package run is repeated. The private release package
remains`c741bd88…`. The experiment-only test leaves with its dispatch; independent
oracle/source evidence remains available. This result is not a permanent ban on
materially different row designs. All283 required performance paths remain in scope.

[Evidence](jpeg-row-pair-experiment-evidence.json) pins both contexts and decisions.
Raw`jpeg-row-pair-experiment.tar.xz`, SHA256
`5f846bb93d093bf3740e1f68d1be7cfaeee4b2876ff70b7c3bd48ecfd16efb5c`,
1,541,348B/161members, is mirrored and member-verified; its source/input/tool
providers and qualified-parent archives are retained. Publication is postponed.
