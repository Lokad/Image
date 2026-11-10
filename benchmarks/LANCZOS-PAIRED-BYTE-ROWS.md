# Paired Lanczos byte rows

The [word-row stage](LANCZOS-PAIRED-WORD-ROWS.md) supersedes the eight costs and
whole-build qualification below. This report retains the byte-row design,
scoped gain, adverse transition and reproducibility.

LBR748 shares Q30 coefficient expansion across two required missing source rows,
using their existing admitted cache slots. Complete prepared RGBA8 reduction
improves **580.52→491.59 µs: 88.92 µs / 15.32% raw / 15.18% normalized**, with
separated raw/normalized filtered/all-actual confidence and native −0.158%.
Conservative upper **1.011 is uncertain near parity**, not a pass or a proven
remaining optimization need. Across eight refreshed controls: six passes, one
word-reduction gap, one uncertainty. Passing cases stop.

| Existing complete operation / explicitly labeled zimg | Managed µs | Native µs | Ratio / upper | GC B/op | Retained plan B |
| --- | ---: | ---: | ---: | ---: | ---: |
| Prepared RGBA8 reduction | 491.59 | 489.18 | 1.005 / 1.011 | 138 | 139,800 |
| One-shot RGBA8 reduction | 672.38 | 1,696.51 | 0.396 / 0.400 | 138,647 | 0 |
| Prepared RGBA8 enlargement | 3,780.94 | 4,376.66 | 0.864 / 0.877 | 148 | 325,156 |
| One-shot RGBA8 enlargement | 3,765.94 | 5,244.39 | 0.718 / 0.723 | 319,224 | 0 |
| Prepared RGBA16 reduction | 587.91 | 548.90 | 1.071 / 1.095 | 139 | 148,040 |
| One-shot RGBA16 reduction | 684.08 | 1,812.27 | 0.377 / 0.380 | 146,866 | 0 |
| Prepared RGBA16 enlargement | 3,784.95 | 3,898.01 | 0.971 / 0.979 | 4,265,332 | 333,396 |
| One-shot RGBA16 enlargement | 4,393.34 | 5,486.14 | 0.801 / 0.808 | 4,592,777 | 0 |

Both prepared sides retain plans/workspace; both one-shot sides include creation.
Source reads, row-ID reset, computation, independent output/normalization and
disposal remain timed. All twelve actuals per final confirmation are retained;
filtered and all-actual confidence views participate. Quiet idle-v2/single-CPU,
source/ownership/exact managed pixels and original scoped native float gates pass.
Other fresh costs qualify status, without causal companion-gain/equivalence
claims. Raw companion times move with reference clocks; no final normalized loss
separates. This is not a full release refresh or all-283-path parity claim.

One shared scheduler source has closed single/pair modes selected once at entry.
Only fast byte vertical reduction can pair two immediate nonzero taps when both
rows are missing and their modulo slots differ. Both complete before cache IDs
publish. Original vertical order, integer subsets/reduction/tails, Double
conversion, Q30/certificates/Guard/repair and cancellation remain. No unused-row
prefetch, retained coefficient vectors, new owner/session/pool or extra capacity.
Actual code preserves the original single-row668B, word1003B and entire default
scheduler1633B instructions. The pair producer977B/scheduler2575B are separate
specializations; static sizes alone do not prove performance.

The 162-check cache model preserves computed rows and accumulation order,
including zero taps, one-slot and64-slot collisions. Existing reduction uses
128pairs/3singles rather than259separate rows: coefficient expansions259→131 per
axis block. These are logical counts, not CPU savings. The supplied original
row cases now verify two distinct rows against BigInteger bits, poison guards,
unchanged source, zero producer allocation and retained shared-root ownership.

The transition retains adverse results. LBR746 confirms499.80µs reduction, but
enlargement3598.17µs versus one contemporary original3484.64µs exposes a material
regression. LBR747's shared-helper revision screens534.82/3571.98µs and remains
unselected. The final closed scheduler removes unused pair state from word and
enlargement, restoring measured single kernels. No blind replay, permanent family
ban or unchanged favorable repeats. Initial final-stage shorts are noisy and are
retained; only the independent confirmations supply current costs.

Final **6,153 full/live checks per OS**,327internal/729public resize-focused cases
in normal/scalar/AVX2-off/AVX512-off modes, and the SAME actual Release package/all
three consumer examples pass Windows/Linux. ISA-specific row kernels execute
only when supported; portable fallback coverage remains. Three coherent
main/embedded versions per OS, no new benchmark shape/settings/harness/native
build. An interim Windows full/package proof completed before its adverse result
was known; final proof repeats only after production changed.

GC138versus139B/op is amortization, not an allocation gain. Prepared byte plan
retention139,800B and independently owned reduction output132,096B are separate;
same admitted capacity, replacement/cancellation/refusal cleanup and existing
output-pool/governed-bypass policy remain. Physical/native/JIT-stack peaks are
unmeasured. The earlier horizontal profile becomes historical after this change.

[Evidence](lanczos-paired-byte-rows-evidence.json) owns current costs, code/source
proofs and qualification. Private0.2.1 package SHA-256 `e751a7af7505e5bedd1580904b4525aa116d370aeccc12b3dcf966b7e4c5f963`
contains timed DLL `6aaf8e270e8ed9d63605042e366b46e52f5ff52849f92419863c168af36fe2d2`; publication stays postponed and
published packages are unchanged. Raw `tmp/lanczos-paired-byte-rows.tar.xz`, SHA-256
`30ffc7e72eefc7e14294be1bf24fe95eae68a58c0ac981ccffa55baf8007f055`, 58,512,804bytes/525members, is mirrored and
member-verified. Dependencies retain [paired word](LANCZOS-PAIRED-WORD-WINDOWS.md)
and [unselected byte-row](LANCZOS-BYTE-ROW-EXPERIMENT.md) history. Current renderer
loads this provider alone for these eight costs. Larger independent demonstrated
gaps precede further tuning here; uncertainty is not an assumed optimization need.
