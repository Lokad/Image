# Shared text-event ordering trials

The residual-endpoint revision is a **confirmed private lead**, not a promoted
production change or an external-reference parity pass. The qualified E1 library
and package remain unchanged.

| Complete ordinary RGBA8 draw | Baseline →candidate µs | Paired ratio interval | Measured GC B/op |
| --- | ---: | ---: | ---: |
| Full upper-endpoint merge, three-pair screen | 46.156 →48.991 | 0.969471–1.162154 /unpromising | 75,864 →75,864 |
| Residual endpoints, three-pair screen | 46.483 →45.803 | 0.912274–1.064307 /promising | 75,864 →75,864 |
| Residual endpoints, independent twelve-pair confirmation | 46.459 →45.634 | 0.974749–0.989776 /increment confirmed | 75,864 →75,864 |

The confirmed saving is **0.825 µs /1.78%**, with separated mean intervals.
Public drawing allocates **75,840 B** plus the unchanged 24 B adapter. CPU0,
optimized JIT, natural nonconcurrent GC, fixed 4,096-operation warmups/batches,
alternating order, original full-frame checks and all idle-v2 guards remain.
Every actual stays; no favorable retry or first-implementation confirmation.

Crossing descriptors already order lower endpoints. Range-pruning scratch retires
after intersection discovery. The first implementation uses that scratch to sort
all upper endpoints, then merges starts, ends and the intersection suffix. It adds
work while still sorting almost as many entries. Its slower mean is unpromising;
the wide short interval does not prove a regression.

The coherent revision omits an upper endpoint only when it equals a neighboring
edge's lower endpoint, which is already represented among sorted starts. This
does not assume connected contours. It fuses finite/zero eligibility with residual
selection, sorts only residual ends and intersections, and unique-merges into the
existing event array. Output cannot overtake unread intersections: distinct
endpoint writes are bounded by the original endpoint-prefix length. No new array,
retained state, implicit cache or admission allowance exists. All raw Event calls,
container bounds, allocation/replacement peaks, intersection arithmetic and final
edge permutation remain; refusal is terminal and cancellation stays direct.
Small/nonfinite/mixed-zero geometry uses the original sort. Uniform zero signs
preserve event bits, as well as pixels.

Three deterministic 8,192-case models retain their scope and corrections. The
actual compiled implementations each pass **313 internal** normal/AVX2-disabled/
scalar and **177 public** normal/scalar checks on both OSes, plus four original
frozen-frame preflights. The new tests compare original event bits/permutations,
canaries, admission/container refusal, cancellation cleanup and subsequent reuse.
These are focused checks, not full prototype qualification. Source review catches
an omitted opposite-sign zero before timing; the initial source/binary/model stay.
A C# local-name collision and a new test's too-complex low-limit owner are corrected
before measurements, without changing frozen benchmark inputs.

Standalone stock ImageSharp.Drawing 2.1.7 /Fonts 2.1.3 /ImageSharp 3.1.12 comparisons
use the [same corrected sampling tool](ORDINARY-DRAW-SYMMETRIC-CHECKS.md), twelve
16,384-operation actuals, three reference groups and the unchanged conservative
gate:

| Private prototype / ordinary draw | Managed /reference µs | Conservative interval /status | Public managed /reference GC B/op |
| --- | ---: | ---: | ---: |
| RGBA8 | 49.414 /48.251 | 0.966114–1.077573 /uncertain | 75,840 /48,576 |
| RGBA16 | 46.562 /47.553 | 0.878172–1.071872 /uncertain | 75,840 /48,576 |

Managed/reference batches vary materially; native CPU/wall ratios remain about
0.998–1.000, which does not identify a cause. Do not normalize clocks, drop samples,
reinterpret actuals as warmup or infer a regression from historical means. The
long RGBA16 paired-reference view passes, but its full bracket envelope remains
uncertain; the conservative gate is retained. The separately measured complete
one-shot and preparation-plus-eight-draw integrations pass at **1,138.480 /6,200.844**
and **1,259.497 /6,924.312 µs**, upper **0.207966 /0.203702**. Preparation and
prepared-draw diagnostics remain separate; stages are never summed.

Before promotion, diagnose whether the confirmed short-batch effect transfers to
the existing 16,384-operation ordinary boundary. Reuse immutable DLLs and all
original checks; no identical standalone recapture for favorable results. Resolve
affected performance before broad Windows/Linux/package qualification. Unchanged
qualified RGBA16/other passing paths remain stopped on the production baseline;
these prototype observations do not replace them. Independent copy work need not
wait on measurement uncertainty.

Linux preparation initially fills the root filesystem before tests. Twelve remote
preparation-input archives free **581,232,640 B** only after hashes match retained
local copies; release packages, reference sources and compressed evidence remain.
An existing worker alias mismatch is caught before sampling; atomically binding
the already pinned worker changes only the private clone. Completed checks are
reused without rebuilding canonical production, harnesses, VM projects, fixtures
or packages. Isolated candidate/core-test builds are pinned. All failures stay.

[Evidence](text-event-ordering-trials-evidence.json) pins models, sources, private
DLLs, all actuals, qualification scope and cleanup. Mirrored/member-verified
`text-event-ordering-trials.tar.xz`: **5,531,856 B /162 members**, SHA256
`94832250a2dfb4472d88a3db7d7726c71d6ed6c2d647998283a0079fe373ee7c`.
Archived `sop1219-*`, `eom1220-*`, `epr1221-*` and declared dependencies reproduce
the round. Reuse the baseline's 6,819 checks per OS/SAME package/all three examples
only for that unchanged baseline. No new broad/package campaign, current-cost
renderer dependency or whole-library parity claim is added.
