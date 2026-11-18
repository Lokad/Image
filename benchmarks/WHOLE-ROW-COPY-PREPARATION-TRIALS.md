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
