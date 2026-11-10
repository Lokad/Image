# Paired Lanczos word windows

LPM740 shares exact UInt32-to-double source conversions between adjacent word
windows with identical First/Count. Each output keeps its own Q30 coefficients,
four FMA chains, reduction and tail order. One numeric body has geometry-selected
paired/single modes; reduction erases unused pairing work. Byte processing,
vertical accumulation, Guard, certificates, exact repair and ownership remain.

| Existing complete word operation / zimg | Managed µs | Native µs | Ratio / upper | Allocated B/op | Retained plan B |
| --- | ---: | ---: | ---: | ---: | ---: |
| Prepared enlargement | 3,856.44 | 3,892.93 | 0.991 / 0.995 | 4,265,332 | 333,396 |
| Prepared reduction | 544.76 | 510.63 | 1.067 / 1.073 | 139 | 148,040 |
| One-shot enlargement | 4,099.16 | 5,077.69 | 0.807 / 0.811 | 4,592,818 | 0 |
| One-shot reduction | 684.21 | 1,801.96 | 0.380 / 0.382 | 146,866 | 0 |

Prepared enlargement improves **4021.02→3856.44 µs: 164.58 µs / 4.09% raw / 3.84%
normalized**, with separated raw/normalized filtered/all-actual confidence and
native −0.267%. Conservative upper 0.995 passes/stops. The other three refreshed
costs qualify current status; no causal companion-gain/equivalence claim. Four
unchanged byte controls reuse [certified-row evidence](LANCZOS-WORD-CERTIFIED-ROWS.md).
Across all eight controls: six passes, two prepared-reduction gaps. No passing
case is retuned after confirmation.

The quiet VM runs sequentially on one CPU, twelve actuals per confirmation.
Complete source reads, preparation, row reset, independent output and disposal
stay timed. Both prepared sides retain plans/workspace; both one-shot sides
include creation. Full-depth exact frozen managed pixels and original native
float RGB/alpha max 1 / RMSE 0.25, source, ownership and idle-v2 gates pass.

A source-only model finds 512 paired windows/3 singles on the existing 513→1027
geometry, with each proposed operation trace matching the original. Logical
conversions 4095→2053 per source row are not measured CPU savings. No full double
source row or changed vertical staging is introduced. This differs
from the earlier [full-row staging](LANCZOS-SOURCE-CONVERSION-EXPERIMENT.md) and
[fused traversal](LANCZOS-FUSED-VERTICAL-EXPERIMENT.md) implementations.

The first stage's enlargement 3801.79 / native 3899.30 µs short is promising; its
reduction 573.34 / native 510.57 µs short is less favorable. Both have broad intervals.
The final constant mode removes unused pair work from reduction. Actual paired/
single bodies are 1003/655 bytes, with six/three static FMA instructions but three
UInt32 conversion instructions each. These are codegen facts, not dynamic counts
or speed proof. All interim sources, modules and actuals remain; instrumented
elapsed is discarded. A native sample-count tooling assumption is corrected by
recovering a completed code capture without replay.

Declared capacity and admission are unchanged: word prepared plans 148,040/
333,396 B, enlargement output 4,264,104 B independently charged until disposal.
Prepared total GC 139/4,265,332 B/op is unchanged; one-shot amortization differences
are not intrinsic allocation claims. Register values and readonly views do not
escape a row. Existing default output-pool retention is separate, physical/native
peaks unmeasured. No buffer, pool, session, new root allowance or owner is added.

All **6,148 full/live checks per OS**, 322 internal/729 public resize checks per
normal/scalar/AVX2-off/AVX512-off, SAME actual Release package/three examples pass
Windows/Linux. The existing row test now covers small paired counts/tails, odd
edges, common enlargement, nonpair reduction and long support, with independent
scalar FMA-bit and BigInteger/envelope/alpha oracles, poisoned boundaries,
unsigned extremes and zero producer allocation. Existing cancellation/refusal/
retained ownership tests pass. Actual-package plan reuse now covers word
enlargement and asynchronous surviving output. Unchanged JPEG qualification is
reused. Two successful main/embedded stages per OS; no benchmark harness/native/
selector/fixture builds, new benchmark images/settings or favorable repeats.

[Evidence](lanczos-paired-word-windows-evidence.json) pins all current costs,
source undo, comparisons and producers. Mirrored/member-verified raw
`lanczos-paired-word-windows.tar.xz`: SHA256 `4e18a5e8e27547f7a5dab8f2a2d887d9bf7a06a6a3422e98a30dc793a5845c2c`,
33,609,896 B / 277 members. SAME actual private package SHA256
`09dccc3ea1ea4691239a90caae7fb5dafbbd00ff2c6e3e9814749053630b100d`. Published packages immutable; publication
postponed. This is scoped development proof, not a 127-row release refresh or
283-path completion. Passing prepared word enlargement stops; independent gaps
remain actionable.
