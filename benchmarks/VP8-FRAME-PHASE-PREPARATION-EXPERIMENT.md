# VP8 frame preparation and execution study

VQP526/VDP527/VDS528/VDP529 is unselected. Three coherent stages preserve exact
focused behavior but do not establish useful complete-operation gains. Qualified
[prepared-model sources](VP8-PREPARED-COEFFICIENT-MODEL.md), main/embedded modules
and public bindings are restored byte for byte on Windows/Linux. The actual
package and all 125 displayed comparisons are unchanged.

| Stage, three actuals per control | Opaque µs / reference µs | Alpha µs / reference µs | GC B/op, opaque / alpha |
| --- | ---: | ---: | ---: |
| A: exact per-segment dequantizers | 4,888.86 / 4,792.14 | 6,219.17 / 5,963.39 | 228,864 / 238,384 |
| B: specialize the shared pixel/validation driver | 4,879.27 / 4,788.87 | 6,392.91 / 5,978.87 | 228,864 / 238,384 |
| C: separate mode, residual/reconstruction and filter phases | 4,985.04 / 4,791.35 | 6,212.51 / 5,968.30 | 228,864 / 238,384 |

All confidence intervals are wide. The small opaque point differences in A/B
and alpha difference in C are not causal gains. B alpha and C opaque points
are unpromising against qualified observations. No gain, regression or equivalence
claim, independent confirmation, workload expansion or favorable repeat follows.
The [preceding study](VP8-ROW-WINDOW-EXPERIMENT.md) retains the unchanged alpha
baseline's launch variation; historical points are not substituted for contemporary
subjects. These are implementation results and inconclusive measurements, without
a permanent ban on dequantizer preparation, specialization or phased execution.

The initial audit uses the actual qualified module and frozen decoding packet,
with exact pixels and zero builds. Its 10,564-byte `ReadPlanes` JIT contains
noninlined DC/AC calls inside the 24-block loop: statically 48 calls per active
macroblock, plus two for Y2. That establishes repeated work, not elapsed cost.
An initial path preflight rejected the distinct coding-validation packet before
execution; the authoritative decode preparation selected the correct input.
Failure, correction and the completed JIT capture remain retained.

A prepares four immutable rows in 96 stack bytes within the existing 8,192-byte
coarse allowance. It preserves the original base clamp before deltas and exact
Y2 arithmetic, independently of native formulas. B selects existing pixel or
validation tags at entry, retiring the per-block runtime dispatch wrapper while
keeping one geometry source. C moves the exact mode, residual and filter loops
into natural frame phases, aiming to reduce overlapping caller state. Whole-source
projections reconstruct B from C, A from B and the qualified original from A.
All input/bit/model/transform/filter/color logic, decision polls, ordering and
admission/ownership boundaries remain exact. No table or implementation is copied
from external references; their preparation lifetimes supplied insight only.

Each stage passes **41 internal / 239 public checks per normal/scalar mode on
each OS**. The added scalar oracle covers all 382 possible combined base values
and all 31 values for every plane delta, including clamp order and exact Y2
scaling. Existing actual-route block/cursor, corpus, cancellation, aggregate
admission, refusal cleanup and surviving-output checks remain. One immutable
core/embedded build per stage/OS; no public/harness/native/selector/fixture rebuild.

Complete frozen 513×259 resident decoding includes parsing, preparation, entropy,
reconstruction/filter/color, independently owned output and disposal. Original
native contexts/wrappers retain their declared preparation. One CPU, unchanged
packets/settings, exact pixels/alpha, source/owner checks, both native brackets
and idle-v2 guards pass. There is no fresh-context comparison or encoder claim.
All six screens are retained; instrumented attribution elapsed is discarded.

No new heap owner/capacity, cache, session or retained state. GC is unchanged above;
admitted/physical/native peaks are not remeasured or inferred from GC. Exact
restoration reuses the 5,875-check and same-actual-package qualification. No broad,
package or restore build runs for these unselected stages. Publication remains
postponed; the full 283-path performance objective remains open.

[Evidence](vp8-frame-phase-preparation-experiment-evidence.json) pins attribution,
stages, projections, inputs/modules, screens, decisions and restoration. Mirrored,
member-verified raw `vp8-frame-phase-preparation-experiment.tar.xz`, SHA256
`8f58f2296071f259e71f297b960e07cac873bb7948326290798c980b757dafef`,
16,563,652 bytes / 304 members, retains sources, scripts, commands, actuals,
guards, failures and original/intermediate/final modules, with its pinned
prepared-model provider. It supplies no displayed result and is not a renderer
dependency. Next audit the existing `DirectRgb444` JPEG decoding gap using current
source and attributable work, without retuning passing or uncertain controls.
