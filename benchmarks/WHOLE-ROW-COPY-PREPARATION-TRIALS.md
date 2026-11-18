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
