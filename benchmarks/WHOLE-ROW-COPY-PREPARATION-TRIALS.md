# Whole-row copy preparation trials

**Not promoted.** Both complete separate-RGBA16 copy screens have adverse means;
the revision's interval is inconclusive. No useful gain is demonstrated. Stop
this design without confirmation, production/reference refresh or broad/package
repetition. Qualified source, DLLs, package and all displayed costs stay unchanged.

| Original complete public copy, three reversed pairs | Baseline /candidate µs | Paired ratio interval | GC B/op |
| --- | --- | --- | --- |
| Whole-row views and grouped aligned end | 3.777470 /3.844278 | 1.007432–1.028044 | 0 /0 |
| Explicit matching source/destination extents | 3.793806 /3.860514 | .985276–1.050952 | 0 /0 |

One complete bounded row per side feeds the existing four-load/four-store blocks;
aligned rows keep their last256B in that grouped kernel and need no tail view.
Partial rows retain bounded overlapping ends. Only eligible separate-word rows
also select this revised shared helper. Same-row memmove, small/wide/scalar bodies,
self-word specialization, public validation, clipping, snapshot traversal and
row/chunk cancellation remain. No API, cache, pin or owned storage is added.

The first helper grows438→653B with extra length-versioned loop bodies and spills.
The coherent revision explicitly matches destination view lengths to source;
actual code removes those duplicate bodies and shrinks to482B. Both realize the
intended grouped aligned end, without a useful complete-operation benefit.
Instrumented clocks are discarded with zero sample commands. Code size does not
establish the cause of either adverse observation.

The earlier [whole-row study](COPY-ROW-VIEW-TRIAL.md) already shows setup/spill risk
on the original self-copy path. This round uses the separate-word control and a
different aligned-end schedule, but should have reconciled that prior study before
prototyping. Retain both scopes; no neighboring row-view, width, extent or tail
sweep follows. The prior [dispatch-only trial](PREPARED-WORD-ROWS-TRIAL.md) also
remains unpromoted. These observations do not establish a family-wide impossibility.

Each candidate passes **12 internal/190 public checks per mode per OS**, normal,
AVX512-off and scalar. Two private actual-helper tests cover775 row lengths in
both directions, distinct strides, separate/aliased backings, exact snapshots,
canaries and live/precanceled tokens. Original full canvas/source, alpha, hidden
RGB, untouched pixels, refusal/recovery and owner checks remain. All frozen
513×259 inputs,256×129 rectangles,65,536 warmups/operations, CPU0 optimized JIT,
natural nonconcurrent GC, idle-v2 guards and actuals stay. These are incremental
managed-DLL comparisons, not FFmpeg parity. No fixture or correctness gate changes.

Two isolated Windows core/embedded builds; zero canonical/VM/public-test/actor/
reference/fixture/package builds. Exact existing actor is reused. Added owned/
admitted/retained capacity stays0; caller payload2,125,872B and unmeasured physical/
native/JIT peaks stay separate. Reuse [qualified mapping validation](FONT-MAPPING-GROUP-VALIDATION.md):
6,839 full/live checks each OS, SAME actual package/all three examples and green
CI44194127767. Three copy gaps and inspection gaps/uncertainty remain; stopped
font/text/codec/resize paths never retime. Publication and the final165-comparison
immutable-package refresh remain separate, with all289 material paths required.

Mirrored/member-verified `whole-row-copy-preparation-trials.tar.xz`: **1,193,776 B /89 members**,
SHA256 `37c67315add3aad0d36198205ae61fb79d41dbd619f63c92b2926024211bfe3f`. Archived rvp1244/rvm1245 recipes, source/DLLs,
tests, codegen, original frames/owners, actuals and guards reproduce the round
from the named baseline and actor dependency. This rejected round supplies no
displayed result or renderer dependency.

## Borrowed-view follow-up

**Unpromoted, near-neutral:** complete separate-byte copy measures
**1.957207→1.959905µs**, interval **.978770–1.024509**, **0B/op**, on three reversed
65,536-operation pairs. No useful gain, confirmation, standalone retiming or
broad/package repetition. Qualified production/package/current costs stay.

A scoped untimed observation of the actual801f DLL finds four pixel-span getter
calls per public CopyRegion: two for capacity validation, two afterward. Returning
the already borrowed views through the single shared validator eliminates the
later two. Caller code changes1531→1536B, helper225→285B; out-span ABI/shuffle
costs may offset removed calls, but these observations do not establish that cause.
All instrumented clocks are discarded, with zero sample commands.

Pixel kernels, clipping, traversal, policy/cancellation/canvas-first/source failure
order, alias capacity, empty regions and borrowed-root/input ownership remain.
Fill/Composite discard the extra outputs; no new heap/admitted/retained storage,
owner, API, pin or cache. Each OS passes10internal/190public normal/AVX512-off/scalar
checks; original full-byte/source/alpha/hidden-RGB/untouched/owner gates pass.
One private Windows core/embedded build; zero other builds. Reuse6,839full/live
eachOS/SAMEactualpackage/all3examples and greenCI44196350570. No adjacent ABI,
inlining or validation sweep follows. The same three copy gaps remain open.

The original input, CPU0 optimized JIT/natural GC, warmups, every actual and idle-v2
guards stay. Caller payload1,062,936B is separate from zero added library workspace;
physical/native/JIT peaks remain unmeasured. Sixty-four verified completed plain
generated consumers are pruned, retaining completion logs/packages/raw/sources.

Mirrored/member-verified `borrowed-copy-view-trial.tar.xz`: **1,115,648B /54members**,
SHA256 `f19111f067dadf8b584252cf0bd1000feb357c9441d8800c2df84186b2a82a2b`. Archived bvr1247/cpa1246 recipes retain the exact
source/DLLs, codegen, gates, all actuals and cleanup proof, using the named
mapping-validation baseline and original prepared-word actor. This supplies no
displayed result or renderer prerequisite.

## Capacity getter follow-up

**Private, unpromoted:** direct null-checked backing-array length replaces a pixel
span constructed solely to read its length. Complete separate-byte copying screens
**1.971874→1.963101µs**, interval**.975628–1.015881**,0B/op on three reversed
65,536-operation pairs. The small uncertain8.77ns point does not justify a shared
setup getter change across codec, resize, text and copy boundaries. No independent
confirmation, promotion, affected-passing retiming or broad/package campaign.

The source change preserves the single backing-reference observation, actual
retained array capacity, identical disposed-object name/failure and operand order.
No fields, out-span ABI, cached geometry, owner, admission, public API or pixel
kernel changes. Existing per-operation borrowing/lifetimes/cancellation remain.
Actual code is1531Bcaller/217Bvalidator/75Bcapacity getter; the latter remains a
call boundary. This is not proof of removable time or a general inlining proposal.

Each OS passes41internal/190public checks per normal/AVX-512-disabled/scalar mode,
with full ownership/transform scope selected on the first attempt. All frozen
source/canvas/sample/alpha/hidden-RGB/untouched/owner/native checks and quiet guards
pass. One private Windows core/embedded build, zero other builds or new fixtures.
Both canonical DLLs, the package and current costs remain byte-identical; reuse
6,839full/live checks and SAMEactualpackage/threeexamples each OS. Added GC,
admitted and retained capacity remain zero; physical/native/JIT peaks unmeasured.

Mirrored/member-verified `image-capacity-getter-trial.tar.xz`:2,205,424B,50members,
SHA256`7d50ae839f1fbe8310ad37df5369bec1ec544c4cc3f4c8c022b856126e00224b`.
Archived `blc1252-*` recipes retain exact source/modules, codegen, every actual,
focused checks and unchanged-source pins. The mapping-validation and prepared-copy
reports retain reused qualified/runtime dependencies. No displayed result or
renderer dependency is added; three copy gaps remain open.

## Stride preparation follow-up

**Private, unpromoted:** reusing the already established pixel size for two
checked stride products screens **1.959387→1.957662µs**, interval
**.956175–1.043994**,0B/op. This near-neutral result does not justify confirmation,
canonical changes, standalone/passing-path retiming or broad/package repetition.

Only two CopyRegion expressions change. Matching immutable8/16-bit depths and
array-sized rasters establish equivalence with the old checked products. All
validation, borrowing, clipping, traversal, checkpoints, kernels and ownership
remain. No global getter, ABI, field, cache, owner or API changes. Actual code
removes both stride-getter calls, growing the caller1531→1537B with the225B
validator unchanged; these observations establish no CPU gain.

Both OSes pass10internal/190public checks per normal/AVX-512-off/scalar mode.
The original three reversed65,536-operation pairs, complete frozen inputs/frames,
alpha/hidden-RGB/untouched/source/owner/native gates, optimized CPU0/natural GC
and idle-v2 guards remain; every actual stays. One private Windows core/embedded
build, zero other builds or new fixtures. Canonical DLLs/package/current costs
remain exact; reuse6,839full/live checks each OS and SAMEpackage/threeexamples.
Allocation and added owned/admitted/retained capacity remain zero; physical peaks
unmeasured. No neighboring arithmetic/inlining sweep follows.

Mirrored/member-verified `copy-stride-preparation-trial.tar.xz`:2,209,064B,
56members, SHA256`06fe98f8a2235aca80d336c9c02b96002b3660db753239e28a95ff2b2218b1fd`.
Archived `sgr1253-*` recipes retain exact source/modules, codegen, every actual,
checks and unchanged-source pins. The mapping-validation/prepared-copy reports
retain reused dependencies. No displayed result or renderer dependency is added.
