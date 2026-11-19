# Whole-row copy preparation trials

Current copy-reference boundaries and status are in
[complete operations](COPY-OPERATION-BOUNDARY.md). The rejected implementations
below retain their original scoped measurements.

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

## Public drawing preparation follow-up

**Private, unpromoted (DPP1257).** Complete separate-byte copying screens neutral:
**2.124714→2.125413µs**, interval **.975307–1.026003**, **0 B/op**, three reversed
65,536-operation pairs. There is no confirmation, promotion, passing-path retiming,
broad/package repetition or neighboring inlining/policy sweep.

The coherent preparation removes redundant validity checks only for the existing
sealed immutable `ImageLimits.Default` singleton and inlines the unchanged Clip
and ValidateRegion bodies. Caller-supplied policies still validate before
cancellation. Every region bound, failure order, checkpoint, borrowed root,
storage lifetime and pixel loop remains. No global policy/getter, owner, cache,
buffer or public API changes. The native adapter already times clipping; no
reference correction, fixture change or relaxed gate is justified.

Actual caller/validator code is1694/230B; both region helper calls disappear and
the default-policy branch is realized. This proves no elapsed-time contribution
or gain. All original source/full-frame/alpha/hidden-RGB/untouched/owner/native
checks and idle-v2 guards pass. Both OSes pass10internal/190public checks per
normal, AVX512-disabled and scalar mode. One private Windows core/embedded build;
zero public/driver/native/fixture/package builds. Owned/admitted/retained capacity
is unchanged; physical peaks remain unmeasured.

Qualified [JSP1255](JPEG-PENDING-PUBLICATION.md) source, DLLs, package and costs
remain exact. Reuse6844full/live checks each OS, SAMEactualpackage/all3examples
and greenCI44226067549. Three copy gaps and asynchronous JPEG probe uncertainty
remain open; the full289-path objective and final165-comparison refresh remain.

[Compact evidence](drawing-preparation-trial-evidence.json) retains the decision,
identities, original actuals and qualification reuse. Mirrored/member-verified
`drawing-preparation-trial.tar.xz`: **2,207,228B /50members**, SHA-256
`bbe7a485bd08420c179f2d31df8ef382a7c31c6e61e2f3c19562fdd198efa13f`.
Archived `dpp1257-*` recipes retain exact source/modules, all logs/guards, codegen
and the unchanged original actor dependencies. No current renderer dependency
is added.

## Parallel plane views follow-up

**Unpromoted (PPV1259).** Whole borrowed row envelopes share one relative cursor
and stride. The design replaces the dedicated self-copy kernel, keeping two
execution kernels overall; different steps retain the original general body.
Unlike per-row view/tail trials, it prepares the complete parallel geometry once.
The intended consolidation is realized but supplies no useful complete-copy gain.

| Original complete separate-buffer control, three reversed pairs | Baseline /candidate µs | Ratio interval | GC B/op |
| --- | --- | --- | --- |
| RGBA8 | 1.958125 /1.958504 | .970457–1.030841 | 0 /0 |
| RGBA16 | 3.775455 /3.904982 | .970700–1.102073 | 0 /0 |

Byte copying is neutral; the word point is adverse with an inconclusive interval.
Stop before confirmation, the passing self-word guard, repository-DLL transfer,
affected native comparisons or broad/package checks. The existing measured
self-copy specialization and every qualified source/module/package/cost remain.
No neighboring envelope/cursor/stride sweep follows.

Actual preparation inlines in both controls; caller1653B/parallel kernel403B
implements the single cursor. There is no physical-peak or causal speed claim.
Original four-load snapshots, bounded tails, row checks, Y traversal and final
cursor rule remain. Equal-stride separate-word rows alone join the prepared
path; small/wide/scalar/different-stride fallbacks remain. No owner, cache, pin,
pool, public API or extra heap/admitted/retained storage is added.

Each OS passes13internal/190public checks per normal, AVX512-disabled and scalar
mode. Existing independent775-length alias snapshots remain; parallel source/
padding/canary/live-token checks and a single-row unused extreme-step control
cover the new preparation. All original frozen full-frame/source/alpha/hidden-RGB/
untouched/owner/native and idle-v2 gates pass. One private Windows core/embedded
build; zero other builds. Original65536-operation warmups/batches, every actual
and natural single-CPU GC remain. Reuse JSP1255's6844full/live checks each OS and
SAMEactualpackage/all3examples; no current-cost renderer dependency is added.

[Compact evidence](parallel-row-views-trial-evidence.json) pins both cases,
source/modules, checks and qualification reuse. Mirrored/member-verified
`parallel-row-views-trial.tar.xz`:2,225,836B/69members; SHA-256
`69a022fa07e6cfc1c6abd14a715d55a839eeb66ed27339cc3635a5943c718092`.
Archived `ppv1259-*` recipes retain exact prototypes, meaningful private test
oracles, all actuals/guards and codegen. This is evidence against this
implementation, not a proof that every materially revised design must fail.

## Pixel getter cold construction

**Both private implementations unpromoted (GCO1260/GCE1261).** Outlining rare
disposed-exception construction removes four pixel-getter calls, but supplies
no acceptable complete-copy improvement. A source-guided correction removes
the first implementation's duplicate null tests; its byte screen regresses.

| Original complete copy, three reversed pairs | Baseline /candidate µs | Ratio interval | GC B/op |
| --- | --- | --- | --- |
| Array-returning throw helper, RGBA8 | 1.964849 /1.956622 | .899370–1.102620 | 0 /0 |
| Array-returning throw helper, RGBA16 | 3.774168 /3.775871 | .980864–1.020428 | 0 /0 |
| Exception construction only, RGBA8 | 1.957045 /2.109573 | 1.052886–1.103587 | 0 /0 |
| Exception construction only, RGBA16 | 3.813156 /3.800298 | .989407–1.003902 | 0 /0 |

The initial helper returns an opaque array after its nonreturning throw; actual
inlined code tests that array twice. The coherent revision keeps explicit throw
in the getter and returns only the constructed exception from a cold helper.
Actual code now has one backing guard, with caller/validator1609/227B versus
1644/254B initially. Those observations explain the revision, not the elapsed
cause. The revised byte point is7.79% slower with a separated interval; word
results remain inconclusive. No confirmation, actual transfer, passing-family
retiming, package/broad campaign or adjacent getter/attribute sweep follows.

Both preserve one backing observation, actual retained capacity, identical
`ObjectDisposedException(nameof(ImageBuffer))`, failure order and ordinary Span
conversion. ByteLength, rows, policies, ownership, pool/dispose, cancellation
and pixel kernels remain. No new field, reservation, owner, cache, unsafe access
or public API is introduced. Each implementation passes41internal/190public
checks per normal, AVX512-disabled and scalar mode on both OSes, with original
full-frame/source/alpha/hidden-RGB/untouched/owner/native and idle-v2 gates.
All65536-operation warmups, three reversed pairs and every actual remain.

Two private Windows core/embedded builds; all other builds0. Current qualified
source/modules/package/costs remain exact. Reuse6844full/live checks per OS and
SAMEactualpackage/all3examples from JSP1255. Allocation/new owned/admitted/retained
capacity remain0; physical peaks are unmeasured. Three copy gaps and asynchronous
probe uncertainty remain; no current renderer dependency is added.

[Compact evidence](pixel-getter-cold-path-trials-evidence.json) retains both
implementations, all actuals, checks and qualification reuse. Mirrored/member-
verified `pixel-getter-cold-path-trials.tar.xz`:3,624,412B/116members; SHA-256
`0870bc3508418cee64da13f70a284e4bd8767e68c2dd253c6c1b451359aaa4a9`.
Archived `gco1260-*`, `gce1261-*` and `gct1261-record.py` preserve exact source,
modules/symbols, codegen, original actors/reference dependencies and recipes.

## Once-per-operation tail dispatch

**Unpromoted.** Moving the one-to-four-vector end decision out of every row
realizes the intended constant-tail kernel, but shows no useful complete gain.
Clipped byte is **1.959137→1.956509 µs**, interval **.979175–1.018530**; separate
byte is **1.945215→1.954328 µs**, **.989650–1.019948**, both **0 B/op**. All three
reversed 65,536-operation pairs stay. Stop without confirmation, actual-DLL
transfer, affected passing-path retiming or broad/package repetition.

One general source schedule has four value-type tail specializations; actual
Tail4 emits four exact ordered loads/stores without tail iteration/count branches.
The wrapper is269B and selected kernel414B. The original bulk, per-row polls,
final cursor rule and bounded addresses remain. Measured self-word, single-row,
portable paths and all public validation/ownership stay verbatim. Extra dispatch
and code retention may offset saved decisions; the elapsed cause is unproved.
This differs from the earlier per-row conditional tail unrolling, but neither
implementation establishes a useful final gain. No neighboring generic-tail sweep.

Both OSes pass **15 internal/190 public checks per normal/AVX512-off/scalar mode**,
including reused775-length alias/separate snapshots, default/live/canceled tokens,
canaries and the unused single-row step. Original full-frame/source/alpha/hidden
RGB/untouched/owner/native/idle-v2 checks pass. The corrected native worker is
used on both sides; instrumented clocks are discarded. One private Windows core
and embedded build, all other builds0. Added owned/admitted/retained capacity0;
physical/JIT peaks are unmeasured. Qualified source, modules, f4package and current
costs remain unchanged; reuse6,844full/live checks eachOS/SAMEactualpackage/all
three examples. CI44239936401 passes all three jobs.

[Evidence](prepared-tail-dispatch-trial-evidence.json) retains the bounded design,
source reconstruction, codegen and every actual. Mirrored/member-verified
`prepared-tail-dispatch-trial.tar.xz`: **2,222,520 B /67 members**, SHA256
`76e138c49ee49b633f63da77e9a9bb6e610399a84494f7fba0862da9d4c80237`.
Archived std1263 recipes and pinned actor/reference/qualified dependencies reproduce
it. No displayed result or renderer dependency changes. Two byte gaps, word/async
uncertainty and the final full-scope package refresh remain open.

## Prepared complete runtime rows

**Unpromoted.** One safe runtime CopyTo per complete separate-buffer row
regresses separate byte **1.949181→2.036424 µs**, interval **1.030002–1.059728**.
Clipped byte **1.993786→1.958381 µs**, **.865089–1.115231**, is uncertain. Both
allocate0B. All three reversed65,536-operation pairs remain. Stop without word
generalization, confirmation, actual-DLL transfer, passing-path retiming or broad/
package repetition. No alignment/shape dispatch variant follows these results.

The small prepared row schedule replaces custom bulk/tail execution only for
eligible separate buffers, sharing8/16-bit byte copying and row geometry. Actual
1599B caller/180B kernel makes one System.SpanHelpers.Memmove call per row.
The initial assertion expected the older System.Buffer symbol; actual assembly
corrects that tooling expectation, reusing completed checks/codegen without
scored samples or rebuilds. All original custom/alias/vertical kernels, small/
wide/scalar fallbacks, clipping, public validation, row checkpoints, bounds and
ownership remain. RBP1191 custom bulk/remainder and adverse VRP1058 owned flips
are distinct earlier scopes; none establishes a general runtime-copy advantage.

Both OSes pass15internal/190public checks in each normal/AVX512-off/scalar mode,
including the reused775-length actual-helper alias/separate oracles. All original
frames/source/alpha/hiddenRGB/untouched/owners/native/idle-v2 gates pass. One
private core/embedded build, zero other builds. Added owned/admitted/retained
capacity0; physical/JIT peaks unmeasured. Qualified source/modules/f4package and
current CBR costs remain unchanged; reuse6,844full/live checks eachOS/SAMEactual
package/all three examples. Preceding e13d69e3 CI44240809521 passes all three jobs.

[Evidence](prepared-runtime-rows-trial-evidence.json) pins the source, actual code,
checks and every observation. Mirrored/member-verified
`prepared-runtime-rows-trial.tar.xz`: **2,221,320 B /70 members**, SHA256
`0b5ae99be11f0863f136f62983717653013472a3d9bdadb59d6dcb2f684f9172`.
Archived rpr1264 recipes and corrected-worker/qualified dependencies reproduce
it. The preceding tail trial now also explicitly declares that already retained
corrected-worker archive dependency; its raw data is unchanged. Neither rejected
trial supplies a displayed result or renderer dependency.

## Current separate-byte attribution

One two-second profile of the unmodified actual package places **1,955/1,998
leaves** in the438B row helper; all1,998 resolve, with no lost records in raw perf
decoding. All1,955 helper offsets match exact same-process instruction starts:
bulk loop1,164, tail loop508, row advance101, tail views82, bulk views75, row poll22,
entry3. Public caller12, validator6, stride getter5, input validation4 and pixel
getter3 locations provide little support for another public-entry/getter campaign.
These are sampled instruction locations, not phase times, stall diagnoses or
removable-time estimates. Sampling skid and memory effects remain unresolved.

Retained word-self profiles use different row geometry/traversal; the old byte
capture includes startup and predates this prepared helper. Neither was replayed.
The current separate-byte control keeps original513×259 input,256×129 rectangle,
65,536-operation actor, actual7b/f4package and corrected911f reference. CPU0 runs
alone; CPU1 collects999Hz software samples. All twenty instrumented batch clocks
are discarded. Full source/canvas/alpha/hiddenRGB/untouched/owner/native checks
and both idle-v2 guards pass. Zero builds, fixture/settings/gate/production changes
or passing-path retiming. Current costs remain unchanged; reuse6,844full/live
checks eachOS/SAMEactualpackage/all three examples.

[Evidence](separate-byte-copy-attribution-evidence.json) retains every leaf,
method map, exact JIT and instruction reconciliation. Mirrored/member-verified
`separate-byte-copy-attribution.tar.xz`: **63,680 B /24 members**, SHA256
`e5656dffb3028e95fa738cd1244c8e313b50e52893a177fce087cbf8f06a4b2e`.
Archived bcp1265 recipes and named dependencies reproduce it; the loss-record
check is read-only raw decoding. This diagnostic supplies no displayed cost or
renderer dependency. Reconcile retained native small-copy scheduling and prior
alignment trials before selecting another implementation; no blind row/tail/
runtime replay or whole-library profile follows. Two byte gaps, word/async
uncertainty and final289-path/165-comparison qualification remain open.

## Fixed-geometry aligned ends

**Rejected after independent confirmation.** The clipped-byte short improves
**2.085939→1.970546µs**, interval **.932084–.957449**; the independent twelve-pair
capture reverses this: **1.954970→2.007816µs**, **1.021708–1.032399**, a2.70%
regression. Separate byte's short **1.962929→1.957140µs**, **.976829–1.017692**,
and confirmation **1.950342→1.944141µs**, **.992335–1.001331**, remain uncertain.
All allocate0B. Keep every observation; no favorable repeat or causal host/
alignment/code-placement claim follows the contradiction. Stop before word
generalization, final-DLL transfer, passing-path retiming or broad/package checks.

The revised strategy uses one constant bulk and fixed256Bend for separate rows
whose original remainder already needs four vectors. An always64Bhead and a
short-lived released alignment hint replace per-row divisions, plan selection
and varying ends from the earlier word trials. It transfers64extra bytes per row;
all accesses stay bounded and unaligned-safe even with stale hints. Actual501B
kernel/1662Bcaller realize the design with18unaligned vector moves. Code shape
and the earlier hint diagnostic do not establish an elapsed gain. Existing self,
vertical, scalar and other-width kernels, public limits, row checkpoints and
ownership remain. No persistent pin, cache, owner or API is added.

The positive-domain geometry model checks516,160length/phase combinations.
BothOSes pass13internal/190public checks in each normal/AVX512-off/scalar mode,
including all64correct/stale hints across11eligible lengths in both directions,
source/canary exactness and prompt pin/owner release. Original frame/alpha/hidden
RGB/untouched/source/owner/native/idle-v2 gates pass. Same private source/DLL,
harness, inputs, references and codegen are used in both captures without rebuilding.
One private core/embedded build; zero others. Added owned/admitted/retained image
capacity0; native handle-pool retention and physical/JIT peaks are unmeasured.
Qualified source/modules/f4package/current CBR costs remain unchanged; reuse
6,844full/live checks eachOS/SAMEactualpackage/all three examples.

[Evidence](fixed-end-aligned-row-trial-evidence.json) preserves the source model,
codegen, all actuals and contradiction. Mirrored/member-verified
`fixed-end-aligned-row-trial.tar.xz`: **2,227,272 B /81 members**, SHA256
`6c1aec253bb9f2c85fe4b8eaecbf1ef54d2c4ddbd0878cf8a867fb48880a6557`.
Archived far1268 recipes, actual source/tests and named dependencies reproduce
it. No renderer dependency or new current-cost claim. No neighboring alignment
sweep follows; materially different designs require their own evidence.

## Complete row-footprint validation

**Unpromoted.** One operation-level check proves each input/output row footprint
using signed 64-bit endpoints before obtaining GC-tracked managed references.
The existing four-load bulk snapshot and ordered vector tail then avoid four
per-row span-view checks. This follows the library's existing bounded
`Vector512.LoadUnsafe`/`StoreUnsafe` pattern: no unsafe blocks, pointers, pins,
cache or new owner. The project retains `AllowUnsafeBlocks=false`. The earlier
one-row managed-reference experiment still prepared and checked each row; it
did not test this complete-footprint boundary.

Actual code has a743B helper and unchanged1531B public caller. Once-only guards
and cold exception constructors contribute to growth; the row schedule has no
range/view checks. Code generation establishes the intended mechanism, not a
speed gain. Three reversed pairs give clipped byte **2.137976→2.118278µs**,
interval **.973811–1.008059**, and separate byte **1.948929→1.956361µs**,
**.990603–1.017200**. Both allocate0B; every actual and original source/frame/
alpha/hiddenRGB/untouched/owner/native/idle-v2 check is retained. Neither result
justifies additional proof complexity. Stop without confirmation, transfer,
passing-path retiming or broader/package checks.

Both OSes pass31internal/190public checks in each normal/AVX512-off/scalar mode.
The reused775-length actual-helper oracles, four pitch-sign combinations, sliced
origins/canaries, invalid/overflowing footprints rejected before mutation,
unused extreme single-row steps and default/live/precanceled tokens pass.
Cancellation polling order is preserved. Self-copy, single-row and portable
kernels stay unchanged. One private core/embedded build; zero other builds.
Added owned/admitted/retained capacity0; physical/JIT peaks are unmeasured.
Qualified source/modules/f4package/current CBR costs remain unchanged; reuse
6,844full/live checks eachOS/SAMEactualpackage/all three examples.

[Evidence](row-footprint-validation-trial-evidence.json) retains source proofs,
actual JIT, focused checks and all observations. Mirrored/member-verified
`row-footprint-validation-trial.tar.xz`: **2,226,372 B /67 members**, SHA256
`1a6ed44bf8535cfca355ae2cbe505f081a9affe6fb87d0f6f8c0bb90bfe137f7`.
Archived rfb1269 recipes and named dependencies reproduce it. No renderer
dependency or new current-cost claim. Do not repeat neighboring bounds/view
variants without a materially different, evidence-backed mechanism.

## End-anchored blocks for separate RGBA8 copies

**Screen complete, unpromoted.** Clipped RGBA8 measures **2.125345 →
2.108777 µs**, paired interval **0.901802–1.091632**. Separate RGBA8 measures
**1.947860 → 1.952657 µs**, interval **0.990424–1.014647**. Both allocate
0 B/op. The clipped point improvement is about 17 ns against a demonstrated
69 ns gap. The two results give no basis for independent confirmation or
standalone reference timing. The originally measured gaps remain open.
[Current operation boundary](COPY-OPERATION-BOUNDARY.md)

A single loop walks fixed 256-byte groups from the end of a checked row and
clamps the final head offset to zero. This replaces the distinct bulk and
tail loops for separate RGBA8 rows whose original tail requires four vectors.
It moves the same number of bytes with up to 63 bytes of overlap within the
row. One row poll, both span bounds, and four loads before four stores remain.
No pin or new storage. The current self-copy and portable paths stay outside
the trial. Geometry checks cover 4,033 eligible byte lengths. Nine Windows
checks cover pitches, eligible residues, canaries, exact copies and
cancellation. The Linux JIT emits a 268-byte vector helper.

The short screen retains three alternating pairs of 65,536 complete calls
per case with the actual package as its baseline and the original CBR1262
worker for exact frame checks. Both idle guards, source preservation, caller
ownership, hidden RGB, alpha and untouched pixels pass. A private Release
build per OS and one embedded Windows build leave the current package and
qualification in place. No new required cases.
[Screen evidence](end-anchored-copy-screen-evidence.json)

The mirrored `end-anchored-copy-screen.tar.xz` retains 32 verified members and
1,215,804 B, SHA-256 `7b5912ebcc236b52e9bb548ad26e8ad63f7e7bd8fd83ca631770d25b5268b8a2`.

## Disjoint runtime block-copy screen

The private block-copy variant showed no measurable clipped RGBA8 gain and
slowed separate RGBA8 in the short screen. Clipped RGBA8 measured **2.114341
→ 2.118251 µs**, interval **0.938698–1.069272**. Separate RGBA8 measured
**1.965312 → 2.006125 µs**, interval **1.002035–1.039848**. Both original
complete operations allocate 0 B/op. Keep the qualified implementation and
the original reference gaps.

Two checked row slices bound managed references passed to
`Unsafe.CopyBlockUnaligned`. This replaces the explicit vector schedule for
the proposed separate-buffer route and leaves one cancellation poll per row.
The 187-byte helper calls `CORINFO_HELP_MEMCPY` once per row. This differs
from the earlier full-row Span.CopyTo trial, which invokes SpanHelpers.Memmove.
There is no new allocation or ownership mechanism.

The dedicated quiet VM uses three alternating 65,536-operation pairs per
case. The actual package supplies the baseline. The immutable managed
candidate passes the original exact frame, hidden RGB, alpha, source and
borrowed-owner checks with the same native worker. All samples and both
idle guards pass. One private core build and a separate unscored codegen
run precede the pairs. The prototype stays private.
[Screen evidence](disjoint-runtime-block-copy-evidence.json)

The mirrored `disjoint-runtime-block-copy-screen.tar.xz` retains 35 verified members and
1,216,056 B, SHA-256 `2caf04f5830fc60cf6dbe7150bc85eee7124a3a69c44ffcedc64dc730a097818`.

## Native byte-copy attribution and direct vector scheduling

Both original separate-buffer RGBA8 workloads reach libc’s forward aligned
256-byte store loop through the frozen FFmpeg worker. The clipped profile has
**963 of 1,999** leaves in that loop and the separate profile has **916 of
1,997**. Neither samples the backward loop or the `rep movsb` instructions.
The samples locate executed code. They do not measure stalls or attribute the
time difference to alignment. Earlier [destination-alignment trials](COPY-DESTINATION-ALIGNMENT-TRIALS.md)
and [fixed-geometry aligned ends](#fixed-geometry-aligned-ends) already tested
managed alignment hints without establishing a usable gain.

The private candidate schedules the same sixteen vectors directly for rows of
961–1,024 bytes. Three leading groups and one end group preserve the original
sample coverage. Checked row slices bound both buffers, the row cancellation
poll stays in place, and no pin or storage is added. The 435-byte JIT helper
contains 32 vector moves. The original three alternating 65,536-operation pairs
per case give these scoped managed-DLL results.

| Complete RGBA8 copy | Current → candidate µs | Paired ratio interval | GC B/op |
| --- | ---: | ---: | ---: |
| Clipped | 2.122681 → 2.113002 | 0.976492–1.014754 | 0 |
| Separate | 2.123739 → 2.110441 | 0.897644–1.100135 | 0 |

Both intervals cross no change. The point savings of about 10 ns and 13 ns
fall short of the demonstrated 69 ns and 47 ns gaps. The candidate stays private
and the [qualified reference costs](COPY-OPERATION-BOUNDARY.md) remain in use.
The geometry check covers all 64 applicable byte lengths, and 190 existing
focused Windows tests pass with the candidate. The quiet-VM runs retain all
observations and exact frames, hidden RGB, alpha, untouched pixels, source and
caller ownership. Both two-second native profiles retain all leaf samples,
zero lost records and successful idle guards. Their clocks are unscored.

[Evidence](native-rgba8-copy-scheduling-evidence.json) retains source, reference,
JIT and package identities, raw captures and replay commands. Both archives
were mirrored and member-verified: `native-rgba8-copy-attribution.tar.xz`
(52,140 B, 25 members) and `four-group-rgba8-copy-screen.tar.xz`
(390,912 B, 34 members).

## Full-row loads before stores

On both frozen RGBA8 copies the private candidate loads all sixteen vectors
before any store. Three leading groups cover 768 bytes; the final group
covers the last 256 bytes of a checked row. The route is confined to
separate-buffer RGBA8 rows of 961–1,024 bytes and keeps one cancellation
check per row. The 448-byte Linux JIT helper shows all sixteen loads before
all sixteen stores, with no vector spills. There is no new owned capacity.

Independent quiet-VM confirmation uses twelve alternating 65,536-call units
per case, after the three-pair screen. Against the unchanged managed DLL,
clipped copy improves from **2.149239 to 2.103730 µs**, interval
**0.973796–0.983881**, and separate copy from **2.124057 to 2.100476 µs**,
interval **0.985259–0.992548**. Both allocate 0 B/op. The exact canvas,
source, hidden RGB, alpha, untouched pixels and caller ownership checks pass.

The same candidate still has both gaps in the standalone complete comparison.
Clipped copy is **2.102999 / 2.011793 µs** with upper **1.051546**; separate
copy is **2.102463 / 2.049461 µs** with upper **1.035749**. Each retains
twelve full units per group and the original [FFmpeg boundary](COPY-OPERATION-BOUNDARY.md).
Pairs establish savings of about 45.5 and 23.6 ns within the managed
implementation. The two standalone results do not establish parity. Keep
the extra specialized path private and reuse production evidence.

[Evidence](full-row-snapshot-copy-evidence.json) identifies the original
drivers, procedure, all observations and immutable DLLs. Mirrored
`full-row-snapshot-copy-trials.tar.xz` contains **465,784 B / 70 verified
members**, SHA-256 `793a3ce07cdcc0a59f05e8edbda72ac2384544a572d069acf57234eca56986af`.

## Full-row snapshots with checked footprints

This follow-up checks complete source and destination footprints once and
then advances managed references between valid rows. The original 16-load,
16-store order and one cancellation checkpoint per row remain. Complete
spans bound every access. The exact helper passes all 64 eligible lengths,
unequal pitches, canaries, overflow, invalid footprints and precancellation.
The 568-byte generated Linux helper retains the requested vector schedule
without vector spills.

On three quiet-VM pairs, clipped copy saves 11.1 ns and separate copy saves
28.4 ns. Independent twelve-pair confirmation gives **1.989202 → 1.985329 µs**
for clipped copy with interval **0.971907–1.024750**, and
**2.085173 → 2.083510 µs** for separate copy with interval
**0.994530–1.003919**. Both allocate **0 B/op**. The qualified package supplies
the baseline. All original operations, exact output and owner gates stay.

Stop this candidate after confirmation. The gain seen in the short screen
does not transfer to the independent pairs. No standalone FFmpeg replay or
production change follows. [Evidence](bounded-snapshot-copy-trial-evidence.json)

## Disjoint runtime copy for separate RGBA16 rows

The retained current word comparison is uncertain even in adjacent pairs.
Its paired interval is **0.998521–1.004941**, inside the full
**0.991834–1.011718** interval. The paired clocks correlate 0.791.
This capture does not isolate frequency as the cause of the uncertainty.

An isolated variant chooses checked disjoint runtime copies once for
separate word buffers within a row cancellation chunk. The original
per-row overlap-safe copies are replaced by a single checked-copy
helper with one poll per row. The 187-byte JIT helper contains one
`CORINFO_HELP_MEMCPY` call. Small and wider copies use the existing implementation.

Three alternating quiet-VM pairs give **3.795276 → 3.821405 µs**,
interval **0.979366–1.035176**, at **0 B/op**. All exact frame, source,
ownership and idle guards pass on the original fixture. This scoped screen
gives no reason for confirmation or a standalone remeasurement. The
qualified implementation and the uncertainty stay.
[Evidence](disjoint-word-copy-trial-evidence.json)

## Cancellation dispatch for independent rows

The private helper selects its SIMD row loop once using `CanBeCanceled`.
Live tokens keep every row checkpoint. The default-token loop omits the poll
and preserves row order, checked spans, bulk/tail transfers and ownership.
The Linux JIT emits a 407-byte default-token body with no cancellation throw
path. No buffer, retained storage or pixel change is introduced.

Three alternating quiet-VM pairs of 65,536 complete calls per side give
**2.136186 → 2.126348 µs** for clipped RGBA8, interval **0.955427–1.037013**,
and **2.087520 → 2.088312 µs** for separate RGBA8, interval
**0.905494–1.105209**. Both allocate **0 B/op**. Exact canvas and source,
hidden RGB, alpha, untouched pixels, caller ownership and idle guards pass on
the original controls. Windows passes 208 focused public tests in each of
normal and scalar modes.

The clipped point savings of about 10 ns fall short of the 69 ns measured gap.
Both intervals include unchanged performance. The trial ends at the screen and
the qualified package keeps its existing costs and statuses.
[Timing, generated code and verification evidence](uncancelable-row-copy-trial-evidence.json)

## Interleaved blocks from two separate rows

The isolated AVX-512 path loads a 512-byte block from each of two separate
RGBA8 rows before storing either. Both footprints are checked. An end block
overlaps the previous block by at most 64 bytes, and the original helper
handles an odd final row. Live-token, overlapping and portable calls retain
the original traversal. The Linux JIT emits all 16 loads before all 16 stores
with no vector spills. An independent model checks 528 eligible row lengths.

Three quiet-VM pairs of 65,536 complete calls per side give
**2.059332 → 2.037190 µs** for clipped copy, interval **0.803406–1.218235**,
and **1.963476 → 1.958535 µs** for separate copy, interval **0.985298–1.009820**.
Both allocate **0 B/op** and pass the original full-canvas, source, pixel,
ownership and idle guards. Windows passes 215 focused public tests in each
mode, including seven alternate-pitch and edge cases.

The two-row trial ends at the screen. The point savings are smaller than the
retained gaps and both intervals include unchanged performance.
[Timing and verification evidence](paired-row-copy-trial-evidence.json)

## Narrower vector transfers for separate rows

The first private variant retains each 64-byte load and writes its two
32-byte halves. The second uses 32-byte transfers on both sides and removes
the vector extractions. Both keep the original 256-byte bulk groups, checked
row spans and live-token checkpoints. Overlapping and portable routes keep
their original implementations. Generated Linux code confirms the intended
loads and stores with no vector spills.

| Complete RGBA8 copy | Baseline → candidate µs | Paired interval |
| --- | ---: | ---: |
| Split stores, clipped | 1.965854 → 1.960526 | 0.975943–1.019104 |
| Split stores, separate | 1.949804 → 1.963899 | 0.854232–1.187534 |
| 32-byte loads and stores, clipped | 1.968877 → 2.033832 | 0.884298–1.206750 |
| 32-byte loads and stores, separate | 1.959309 → 1.955032 | 0.962757–1.034151 |

Each screen retains three quiet-VM pairs of 65,536 complete calls per side,
at 0 B/op. Original pixel, source, ownership and idle guards pass. Windows
passes 215 focused public tests per mode for each variant. Both variants stop
at the screen. The original complete-operation costs retain the two gaps.

A separate untimed check confirms that constant 128- and 256-byte disjoint
block copies compile to alternating 64-byte loads and stores on the VM.
Exact copies and canaries pass. This exposes an instruction schedule to test
in the row helper. [Timing and generated-code evidence](copy-vector-width-trials-evidence.json)

## Alternating constant-block transfers

The isolated RGBA8 helper uses a constant 256-byte copy within each checked
row. The Linux JIT emits alternating 64-byte loads and stores inline, with
no spill or runtime copy call. The route keeps the original row bounds,
tail and cancellation checkpoints, and is limited to distinct buffers.

Independent confirmation uses twelve quiet-VM pairs of 65,536 complete
operations per side. Clipped copy improves from **1.970086 to 1.954262 µs**
(**15.8 ns**, ratio interval **0.990495–0.993442**). Separate copy is flat at
**2.100070 and 2.100288 µs** (interval **0.996548–1.003667**), despite a
promising point estimate in the three-pair screen. Both allocate **0 B/op**.
Original exact-frame, source, hidden RGB, alpha, untouched-pixel and
ownership checks pass. Windows passes 215 focused tests per mode, normal
and scalar.

The clipped saving is smaller than the demonstrated gap. The candidate stays
private, and both complete-operation FFmpeg gaps remain open.
[Screen, confirmation and generated-code evidence](copy-vector-width-trials-evidence.json)

## Bottom-up traversal for separate RGBA8 rows

An isolated variant starts at the last clipped row and passes negative strides
to the existing SIMD helper. This tests the effect of changing the order of
memory accesses. It applies to distinct buffers with a noncancelable token.
Self-overlap and live-token calls keep their original mutation order and
checkpoints. The Linux JIT passes both negated strides to the shared helper.

Three quiet-VM pairs of 65,536 complete operations per side give **1.965180 →
2.012467 µs** for clipped copy (interval **0.963879–1.087999**) and **2.124874 →
2.149211 µs** for separate copy (interval **0.965271–1.059836**). All six
candidate units are slower than their matches. Both cases allocate **0 B/op**
and pass the original exact-frame, source and ownership checks. Windows
passes 353 focused tests in each of normal and scalar modes.

The trial ends at the screen. The quiet guard first caught the build server
before any samples. Shutting it down restored the required idle conditions.
The production copy gaps remain open.
[Screen, generated code and verification evidence](copy-vector-width-trials-evidence.json)

## Staging the next source row

An isolated variant loads the next source row's first 256 bytes before
finishing the current row's tail. It uses checked spans and applies only
to separate RGBA8 copies with noncancelable tokens. The Linux JIT keeps
four head vectors live across the tail, with no vector spills. The
overlapping, portable and live-token routes keep their existing execution.

Twelve independent quiet-VM pairs per case use 65,536 complete operations
per side. Clipped RGBA8 improves from **1.975373 to 1.953665 µs**
(**21.7 ns**, interval **0.987635–0.990388**). Separate RGBA8 improves from
**1.968961 to 1.951165 µs** (**17.8 ns**, interval **0.989123–0.992802**).
Both allocate **0 B/op** and pass all original pixel, source and ownership
checks. Windows passes 353 focused tests in normal mode and 353 in scalar
mode.

The scoped improvements are smaller than the demonstrated FFmpeg gaps.
The trial stays private. It provides a lead for combining earlier source
loads with a smaller checked row loop, without adding the gains from
independent trials.
[Screen, confirmation and generated-code evidence](copy-vector-width-trials-evidence.json)

The follow-up combines staged head loads with a complete row snapshot. Each
checked row uses 16 vectors. The 494-byte Linux method keeps the next four
head loads ahead of the current 16 stores, with no vector spills. An
independent check covers all 64 eligible byte lengths. The same 353 Windows
focused tests pass per mode.

The three-pair screen gives **1.969525 → 1.947051 µs** for clipped
(interval **0.982294–0.994923**) and **1.963342 → 1.941004 µs** for separate
(interval **0.977524–0.999848**), at **0 B/op** with the same gates. Its
roughly 22 ns point savings on each control are in the range of the
simpler staging trial and below both FFmpeg gaps. The follow-up ends at
screening, with both trials retained.

## Aligned middle with bounded row edges

This variant revisits the native aligned-store strategy with fewer redundant
transfers than the earlier [fixed-end trial](#fixed-geometry-aligned-ends).
One temporary destination alignment hint drives the middle stores, with one
bounded vector at each edge when needed. The pin is released before copying
and all accesses remain valid if the array moves. The checked schedule uses
16 or 17 vectors for each of the original row lengths. A model verifies
coverage across 254,016 width and alignment combinations.

Three quiet-VM pairs per case give **2.022648 → 1.987316 µs** for clipped RGBA8
(interval **0.527449–1.831696**) and **2.113849 → 2.118632 µs** for separate
RGBA8 (interval **0.961664–1.044585**). Both allocate **0 B/op** and preserve
the original pixels, checks and borrow lifetimes. All 353 focused Windows
tests pass in each mode, normal and scalar.

The clipped point difference varies substantially across pairs. The separate
point is slightly adverse. This variant stops at screening, with both
complete-operation gaps still open.
[Screen, generated code and verification evidence](copy-vector-width-trials-evidence.json)

## Shared block offsets across four rows

This separate-buffer variant checks a strip of up to four rows at a time.
It computes each 256-byte block offset once and applies that offset across
the strip. The portable, overlapping and live-token routes preserve their
original traversal. A model checks 16,144 geometries, including partial
final strips and alternate row pitches. The 342-byte Linux helper emits
four vector loads and four stores in the inner loop, without spills.

Three quiet-VM pairs per case, at 65,536 complete operations per side, give
**1.955832 → 1.948444 µs** for clipped RGBA8 (interval **0.985906–1.006647**)
and **1.968387 → 1.951524 µs** for separate RGBA8 (interval
**0.984343–0.998574**). Both allocate **0 B/op**. Exact pixels, input and
ownership checks pass. Windows passes 353 focused tests in each of normal
and scalar modes.

The separate point gain is about 17 ns and the clipped interval includes
unchanged performance. The trial stops at the screen, with both gaps open.
[Screen, generated code and verification evidence](copy-vector-width-trials-evidence.json)

## Variation between baseline sessions

Eight retained paired sessions per case use the same baseline DLL, managed
driver, case harness, native worker, inputs, outputs and .NET executable.
All use 65,536 operations per statistical unit and pass the quiet-VM guards.
The unchanged clipped baseline means span **1.956–2.035 µs** (79 ns).
Separate baseline means span **1.961–2.159 µs** (197 ns).

Those ranges exceed the recorded gaps of 69 ns and 47 ns. Keep comparisons
within their matched sessions. The saved data establish variation across
sessions, with no new samples. They give no cause for the variation and
leave the original standalone FFmpeg statuses open.
[Archived session identities and means](copy-vector-width-trials-evidence.json)

## Clock and storage-placement diagnostics

Four fresh launches alternate the original separate RGBA8 workload and
its frozen FFmpeg worker. The diagnostic retains all 16 pairs of 65,536
calls per side. CPU 1 observes CPU 0's APERF and MPERF, and the driver
reads scheduler counters outside the operation clocks. Managed launch
means span **1.953–1.974 µs**. Observed managed scheduler wait is
**1.04–1.58 ns/op**, and the managed APERF/MPERF ratio spans
**1.7444–1.7479**. The paired mean difference is **45.5 ns**. Clock
adjustment leaves essentially the same spread in paired ratios.
This run did not reproduce the earlier 197 ns baseline spread.

A separate run uses the same package, source, compiled copy method and
four byte-identical destination canvases. The 16 batches rotate each
canvas through every position. Allocation and brief pins to read address
offsets are outside the clocks. All observed address offsets stay stable.
The four means span **1.962–1.991 µs**, a **29.7 ns** difference. Two
destinations with the same offset within a cache line differ by
**17.8 ns**. Average APERF/MPERF for each destination spans
**1.7442–1.7486**. The timing follows the destination backing in this
capture. The diagnostic cannot isolate virtual offset from physical
placement or explain the whole historical spread.

Both captures retain exact pixels, source and ownership checks, quiet
guards and **0 B/op** within the measured copy. These diagnostics add
no scored result. The standalone FFmpeg statuses remain open.
[Raw clocks, layout offsets and verification evidence](copy-vector-width-trials-evidence.json)

## Two complete row snapshots

For eligible separate buffers with no live cancellation token, this trial
loads two full rows before writing either. The bounds-checked helper covers
961–1024 row bytes, uses the original copy for an odd final row, and leaves
the portable and same-buffer routes in place. The optimized Linux method is
701 bytes. It emits 32 vector loads followed by 32 stores without spills.
Exact pixels, input and ownership checks pass, as do 353 focused Windows tests
in each of normal and scalar modes.

Three quiet-VM pairs per case initially favor the new schedule by **74 ns**
for clipped RGBA8 and **44 ns** for separate RGBA8, with wide intervals.
Twelve independent pairs per case give **2.015887 → 2.023235 µs** for
clipped (candidate/baseline interval **0.987156–1.020501**) and
**2.124117 → 2.125917 µs** for separate (**0.997514–1.004192**).
All subjects allocate **0 B/op**. The confirmatory pairs give no reason to
adopt the schedule.

A diagnostic then runs both DLLs on exactly the same backings, with four
ordinary source/canvas allocations and two opposite-order pairs per
placement. The separate case is **5–57 ns slower** across the four
placements. The clipped effects range from **42 ns slower to 10 ns faster**.
Both complete managed calls reproduce the frozen results. The shared backings
are a diagnostic setup. The standalone FFmpeg comparisons keep their existing
scores. Choose a distinct source of removable work before another copy trial.
[Paired timings, placement results and verification evidence](copy-vector-width-trials-evidence.json)

## Native array placement and cache-line footprint

Three untimed launches per open copy case use the frozen FFmpeg worker.
A diagnostic allocator shim delegates to glibc and records only the low
12 address bits of the original allocations. Both borrowed buffers start
at byte 16 of a page in all nine launches. The worker output hashes and
original readiness records match. The worker receives no sample commands.

The clipped RGBA8 copy starts at cache-line offsets 28 and 36 in the
source and destination. Separate RGBA8 starts at 16 and 32, and separate
RGBA16 at 16 and 48. The four previously observed managed placements for
separate RGBA8 have different starting offsets but touch the same
**2,185 cache lines per buffer** as the native control. A 16-row window
also reaches the same maximum of **16 addressed lines in one L1 set**.
This describes the addressed footprint. It gives no cache-miss count,
instruction split count, or causal explanation of the placement timings.
Keep the scored comparisons and the existing scope. A future alignment
trial needs a more specific transfer mechanism than the total footprint.
[Frozen geometry, allocation records and model evidence](copy-vector-width-trials-evidence.json)

## Aligned stores without pinning

Modeling the recorded placements and the executed vector schedules gives
**1,920–1,936** cache-line-crossing stores per complete managed RGBA8 copy.
The frozen FFmpeg worker's forward schedule gives **605** on each control.
The managed aligned-middle schedule gives **240–242**. It can add split
source loads when the source and destination phases differ. These counts
describe the scheduled accesses. They do not estimate stalls.

The trial takes the low six bits of a managed reference difference once
and uses them as a destination alignment hint. Every transfer stays within
a checked row and supports unaligned addresses, including after relocation.
The inlined hint produced an 809-byte MinOpts method on the VM. Computing
the hint in a separate helper restores the optimized 412-byte row loop
without vector spills. The initial build stopped at code generation.
[.NET reference-offset behavior](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.unsafe.byteoffset?view=net-10.0)

Three quiet-VM pairs of 65,536 complete calls per side give
**1.949564 → 1.937849 µs** for clipped RGBA8 (interval
**0.946170–1.044227**) and **1.948343 → 1.956745 µs** for separate RGBA8
(**0.981540–1.027614**), both at **0 B/op**. Original exactness, source
and ownership gates pass. Windows passes 125 focused tests per mode with
intrinsics enabled and disabled. Stop the trial at the screen. The two
standalone gaps remain open.
[Transfer model, generated code and comparison evidence](copy-vector-width-trials-evidence.json)

A follow-up geometry model carries a source vector across adjacent
destination-aligned stores and joins it with the next source vector.
It verifies **86,016** width and phase combinations with bounded edge
loads. On the observed mismatched phases it projects **354–355** split
loads at the cost of about **1,800** vector permutations per operation.
On the observed matching phase, the aligned vectors need no permutation.
The model gives a bounded design to check against the actual generated
loop. It supplies no timing result.
[Geometry proof](copy-vector-width-trials-evidence.json)

## Carried source vectors

The candidate uses two source vectors to assemble each destination
vector in the aligned middle of a checked row. It carries the last
source vector across iterations and uses ordinary bounded transfers at
the edges. Equal row-step residues make the relative phase constant.
Equal phases and unsupported hardware use the existing copy path.
The 674-byte Linux helper issues four new source loads, four byte
permutations and four stores per bulk iteration, with no vector spills.

Three quiet-VM pairs per case, at 65,536 complete calls per side, give
**2.125504 → 2.119812 µs** for clipped RGBA8 (paired interval
**0.977058–1.018006**) and **2.098118 → 2.084585 µs** for separate
RGBA8 (**0.929409–1.062111**). Both allocate **0 B/op**. Exact image,
source and ownership checks pass, as do 125 focused Windows tests per
mode with intrinsics enabled and disabled.

The separate case's initial guard rejected CPU0 I/O wait before any
samples. A separate-case-only run passed both guards. The clipped
samples were retained from their original run. Stop at screening. The
point gains are under 14 ns and the standalone FFmpeg gaps remain open.
[Both screens, guard records and generated code](copy-vector-width-trials-evidence.json)

## Shared storage and copy preparation

Lokad.Image and FFmpeg ran on the same caller-owned source and canvas
arrays, with four natural placements per depth and two reversed-order
pairs per placement. The diagnostic adapter wraps each FFmpeg batch in
one native call. Its clipping wrapper and FFmpeg copy instructions match
the scored worker after relocation. Byte placement means put Lokad.Image
**49–86 ns behind** on clipped RGBA8 and **59–117 ns behind** on separate
RGBA8. All sixteen individual byte pairs have the same direction.
Separate RGBA16 placement means range from **26 ns ahead to 67 ns
behind**. All four RGBA16 placements have the same relative page offset,
so that offset alone cannot explain the range.

The second diagnostic compares the public call with its actual RGBA8 row
kernel on the same arrays. Geometry and borrowing precede the kernel
clock. The kernel means are **2.4–8.1 ns faster** for clipped RGBA8 and
**2.6–11.6 ns faster** for separate RGBA8 across four placements. The
different invocation wrappers make these bounds an attribution aid.
The observed preparation cost is small compared with the byte gaps.

Both diagnostics use the immutable package DLL and batches of
65,536 operations on CPU 0. Exact frames, source bytes, live owners,
zero managed allocation and both idle guards pass. Handles are acquired
and released outside timing and remain live through both sides.
The shared-clock experiment retains the distinct managed and native
result consumers. Use the original complete standalone comparisons for
parity. The word case remains uncertain.
[Placement, operation and code verification evidence](copy-vector-width-trials-evidence.json)

## Aligned source loads without pinning

The complementary alignment trial takes the source address phase and
row step as hints. Checked row slices and unaligned-safe edge transfers
preserve exact copies even if the hint goes stale. The geometry proof
covers 254,016 width and phase combinations. At the four observed
backings, the model projects **240–242** split loads per copy in place of
**1,920–1,936**. It also projects **2,153–2,169** split stores.
The optimized Linux helper is 412 bytes and spills no vectors.

Three quiet-VM pairs of 65,536 complete calls per side give
**1.951291 → 1.945392 µs** for clipped RGBA8 (paired interval
**0.913257–1.088360**) and **1.955877 → 1.952489 µs** for separate
RGBA8 (**0.859683–1.159146**). Both allocate **0 B/op**. Exact pixels,
source, ownership, and quiet guards pass, along with 125 focused Windows
tests in each of normal and scalar modes. Stop at screening. This result
gives little reason for another alignment-only trial.
[Transfer model, tested build and paired timings](copy-vector-width-trials-evidence.json)

## Separate RGBA16 confidence review

The original complete word copy averages **6.4–10.5 ns slower** than each
of its three native brackets. Increasing precision while holding these
point estimates fixed would leave its mean above parity. Holding the
native bound and managed spread fixed, the existing all-actual interval
would need a uniform **44.5 ns** reduction per managed call to reach
the gate. This is a sensitivity calculation on the retained capture.

The shared-storage diagnostic averages **3.8 ns slower** across its four
placements, with differences on both sides of parity. It supplies a
reason to account for placement in a future confirmation. Keep the
original standalone result uncertain. A new scored run needs a concrete
reason to expect different timing or better control of a measured
source of variance.
[Reproducible review of the retained samples](copy-vector-width-trials-evidence.json)

## RGBA8 cache-line reuse and staged tail

On the quiet VM, CPU 0 reports a 48 KiB L1 data cache with 64 sets, 12 ways
and 64-byte lines. Hardware cycle, instruction and cache events are unavailable.
An in-order, write-allocate LRU model replays the original managed and native
vector schedules on the eight retained shared-buffer placements. Both schedules
fill the same number of source and destination lines in every placement. Clipped
RGBA8 fills 2,169 lines per buffer per warm copy. Separate RGBA8 ranges from 2,153
to 2,185 depending on placement and buffer. The model gives no evidence of a
difference in L1 line reuse. Prefetching, split-instruction cost, other working
data and actual cache replacement are outside the model.

The private candidate loads the last four vectors before the middle loop and
writes them afterward. This follows the ordering seen in the frozen native
copy, using the existing bounded unaligned managed accesses. Separate RGBA8
rows with four end vectors are eligible. The model verifies coverage for all
1,009 eligible row lengths. The 510-byte Linux helper retains all four tail
vectors across the middle loop with no vector spills. Its original per-row
cancellation checkpoint remains.

Three alternating quiet-VM pairs of 65,536 complete calls per side give
**1.953571 → 1.960442 µs** for clipped RGBA8 (interval **0.981198–1.026343**)
and **1.959899 → 1.961131 µs** for separate RGBA8 (**0.938142–1.067267**).
The qualified JPEG marker-coalescing package supplies the managed baseline.
Both operations allocate **0 B/op** and pass the original exact-frame, source,
ownership and idle guards. Both point estimates are adverse and both intervals
include parity between the managed builds. Stop the candidate at screening.
The original standalone copy scores still apply.

The 37-member archive retains the model, source, assembly, actuals, guard
records and commands. [Verified evidence](copy-vector-width-trials-evidence.json)

## RGBA16 backing roles

The original separate RGBA16 workload runs through package `d52e53f2` and
the frozen FFmpeg plane-copy adapter on four shared backing pairs. Each pair
is measured in both source/canvas assignments. Both buffers receive the same
original pixels before warmup, and both timed loops observe the same destination
byte on every call. Each assignment has two 65,536-call pairs in opposite
managed/native order. Exact canvas, source and ownership checks pass at 0 B/op.

Swapping roles increases the managed-minus-native difference by **41.9, 22.3,
14.5 and 48.8 ns**, respectively. The native side improves by **15.5–43.6 ns**
in all four pairs. The managed side changes by **−4.5 to +26.4 ns**. These
observations depend on which array supplies each address. Swapping roles also
changes address phase, so the capture leaves physical placement, alignment and
memory direction unresolved as individual causes. Both quiet guards pass.

Keep the original standalone RGBA16 status uncertain. A revised confirmation
can preselect independent placements and balance both roles while preserving
the full copy, fixed workload and existing confidence gate. Keep the native
and managed observations for every placement in the result.

An untimed check on the separate RGBA8 path uses the same package. Default,
disabled and enabled JIT loop alignment all emit the same 438-byte copy body,
with matching instruction text and block offsets. The default records two
zero-byte alignment slots. There is no useful runtime-switch timing comparison
for this body. The archive retains all layouts, observations, build logs, hashes
and verification commands. [Verified evidence](copy-vector-width-trials-evidence.json)

## Prepared rows for the existing RGBA16 runtime mover

This scope keeps the existing `SpanHelpers.Memmove` call and dispatches once
for a separate RGBA16 rectangle whose rows fit a 4,096-pixel cancellation
chunk. The helper prepares row offsets and byte count once, retains one
cancellation checkpoint per row, and emits one runtime copy per row. It reuses
the 180-byte helper from the separate-byte investigation. Byte copies and
overlap paths stay on their existing routes.

Four shared backing pairs each run both assignments and opposite execution
orders. The private DLL saves **6.4 ns** on average at **0 B/op**. The means by
backing pair are **−0.8, 17.7, 4.5 and 4.2 ns**. The same immutable development
package supplies the baseline. Exact full-frame, source and ownership checks
pass with normal and scalar execution. Both quiet guards pass.

The measured savings leave the [placement-balanced word uncertainty](COPY-OPERATION-BOUNDARY.md#placement-balanced-rgba16-confirmation)
open. The added specialization stays private. [Verified observations and code](copy-vector-width-trials-evidence.json)
