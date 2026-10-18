# Current bicubic evidence reconciliation

Latest: [64-tap byte quotient bounds](#sixty-four-tap-byte-quotient-bounds)
replace default Down64x32_8 with a qualified short gap at1.134x/upper1.215x,
31.6% faster. Its preceding independent1.662x result below is historical;
other selected paths retain applicable evidence. The <=1x objective remains open.

Historical timing note: [byte-row output specialization](WARM-BICUBIC-BYTE-ROW.md)
replaces thirteen default full-row-cache comparisons with current qualified short
gaps. Only the affected default-cache timings here are superseded; other selected
paths retain their applicable evidence in the [current summary](../docs/benchmark.md).

Historical timing checkpoint: [wider enlargement strips](WARM-BICUBIC-ENLARGEMENT-STRIPS.md)
records seven current qualified short results. Its affected enlargement timings and source/qualification checkpoints are historical; other selected paths retain their applicable evidence.

The [paired-column round](WARM-BICUBIC-PAIRED-COLUMNS.md) supersedes the three
Mixed16, TightMixed16 and TightVertical16 timings below with qualified short
screens. Their paths changed; the old independent results remain historical.
The [byte paired-column round](WARM-BICUBIC-BYTE-PAIRED.md) also supersedes
Mixed8, TightMixed8 and TightVertical8 with qualified short screens. The [byte strip round](WARM-BICUBIC-BYTE-STRIPS.md) supersedes TightDown8. The
other 54 comparisons retain applicable unchanged-path proofs. The current
strict <=1x objective remains open; historical stopped/closed labels refer to
the completed <=2x phase.

At completion of the previous phase, all 61 named cases had current-path evidence.
The current horizontal RGBA16 enlargement and single-pixel RGBA8 reduction remain
stopped. Three more confirmations are reused without new timings:

| Existing case, input+output+4 KiB limit | Ratio / upper | Verification |
| --- | ---: | --- |
| TightHorizontalUp8, 1027x259 | 1.583 / 1.591 | Reused independent twelve |
| TightUp8, 1027x519 | 1.783 / 1.795 | Reused independent twelve |
| TightUp16, 1027x519 | 1.773 / 1.783 | Reused independent twelve |

The original two 513x259 rasters, exact managed pixels, approved native bounds,
complete packing/filtering/output ownership, all twelve actuals and both quiet
guards are preserved. Allocation figures and unchanged timing details remain in
the [original enlargement report](WARM-BICUBIC-TIGHT-HORIZONTAL-UP8.md) and the
[benchmark summary](../docs/benchmark.md). No new image or workload grid.

Actual archived source inventories establish that 35 resize, helper, buffer,
limit, geometry and build inputs match current production. The only changed
shared-context source adds `CommitAdmittedWork`, which resizing never calls;
all executed context methods and fields are unchanged. Ordinary Resize allocates
zero-initialized output and does not use the changed overwrite-storage pool.
The captured byte hashes establish the measured code even where the recorded
parent revision precedes its uncommitted optimization.

The three pinned native worker binaries, identities and scaler archive remain
byte-identical on the VM. The shared adapter source later added an unselected
compile-time single-pixel mode, an equal vertical-halo constant alias for this
horizontal case, and untimed ready metadata. Removing those scoped differences
yields identical remaining adapter source; complete timed work is unchanged.
The reference remains labeled zimg, FFmpeg's zscale backend. No baseline or
pixel/quality adjustment is introduced.

[Reuse evidence](warm-current-bicubic-evidence.json) pins original observations,
archives, receipts, relevant source hashes, native identities and current
qualification. The existing 3,483 Release tests per OS, 898 live comparisons and
same actual package consumer/all three examples remain applicable. That reuse
round required zero builds, statistical jobs or repeated qualification.

The ordinary Mixed16/Mixed8 uncertainties now independently confirm after the
vertical-first helpers replaced their old non-tight row-cache selection:

| Default limit, 1027x129 output | Lokad.Image / backend µs | Ratio / upper | Managed B/op | Actual / selected |
| --- | ---: | ---: | ---: | ---: |
| Mixed16 | 1,627.26 / 851.20 | 1.912 / 1.964 | 1,060,419 | 12 / 11 |
| Mixed8 | 1,600.47 / 864.39 | 1.852 / 1.872 | 530,370 | 12 / 10 |

The same byte/word rasters and frozen managed/native output hashes pass exact
rational pixels, source preservation and independent-owner checks. Approved
native RGB/alpha maximum1 and RMSE0.25 remain unchanged. Full deinterleaving,
four-pixel clamped halo, premultiplied float filtering, independent planar/packed
output acquisition, unpremultiplication, quantization, zero-alpha clearing and
release remain timed. This is the labeled FFmpeg scaler backend, zimg.

Three-actual screens and independent twelve-actual confirmations retain every
actual and pass both versioned quiet guards. Conservative uppers include selected/
all-actual intervals against the faster native bracket's lower confidence bound.
Sequential Release uses CPU0, one filter thread, normal SIMD, tiering disabled and
workstation nonconcurrent GC. Managed allocations include harness costs; native
allocation totals are unmeasured. Startup, IPC, I/O and validation are excluded.

The word screen was borderline2.007x. Its twelve-actual job had already launched
before that mean was read; the retained independent proof passes1.912x/upper1.964x
and is reused without another repeat or optimization. Later screen means are
read before launching longer jobs. This workflow exception changes no gate.

Only benchmark selector parsing changes: one-to-three distinct existing names,
same default and all61 enum cases. Source prefix/suffix prove Setup and the timed
operation unchanged. One harness build and two shared selection drivers serve
all four jobs; verified assemblies preload every cached child including WarmDry.
Two selected native workers refresh changed original adapter source using the
same compiler/flags/scaler archive; frozen native pixels and complete boundaries
remain unchanged. Production/package qualification is reused without a rebuild,
new original raster, profile, whole-library audit or repeated suite/package proof.

Stop both ordinary Mixed cases. B12 remains open for the other named helper/
dispatch uncertainties; reuse valid unchanged proofs before scheduling another
measurement. Identification boundaries and the full warm objective remain open.

The existing tight-limit mixed-axis byte/word cases also independently confirm.
Their entry preference guards changed in d3ab2b4/b228c29 after the old proofs;
unchanged hot loops alone did not certify the complete operation.

| Input+output+4 KiB limit, 1027x129 output | Lokad.Image / backend µs | Ratio / upper | Managed B/op | Actual / selected |
| --- | ---: | ---: | ---: | ---: |
| TightMixed16 | 1,644.19 / 848.47 | 1.938 / 1.957 | 1,060,419 | 12 / 12 |
| TightMixed8 | 1,617.87 / 860.30 | 1.881 / 1.901 | 530,370 | 12 / 12 |

The original frozen managed/native pixels, exact rational output, approved
maximum1/RMSE0.25 native bounds, working limit, complete ownership, all twelve
actuals and both quiet guards pass. These two cases reuse the same qualified
production DLL, selector harness and already-refreshed native workers with zero
rebuilds or repeated qualification. Two shared drivers serve the four short/
confirmation jobs; means are read before confirmations. No profile, new image,
shape grid or pixel/quality/baseline change. Stop both pairs. Next: reconcile the
existing vertical-reduction proofs, reusing unchanged selected paths.

The four existing height-reduction cases also reuse valid current-path evidence:
Vertical8/Vertical16 at1.287x/upper1.301x and1.423x/upper1.428x, plus
TightVertical8/TightVertical16 at1.822x/upper1.838x and1.813x/upper1.823x.
Their allocation figures remain in the generated summary; no new timing result.

Actual archived inventories match the complete Resize dispatcher, allocated
buffer, limits, selected word/mixed/cache bodies, alpha/sampling and build inputs.
Only the unused context method was added. Word cases reject the modified byte
helper at its unchanged first short-circuit depth test; every changed width
clause/preference predicate is unreachable, and the remaining helper source
matches. Thus these changes do not alter their executed paths. The original
pinned native workers/identities and scaler archive are byte-identical on the VM.
The byte cases preserve frozen qualified binary64 pixels and the existing25
independently certified exact rational alpha halves; no managed tolerance is
added. Word rational pixels, original native bounds, ownership, all twelve actuals
and both guards remain valid. Zero builds, statistical jobs or qualification
repeats. Stop all four; next reconcile the named tight horizontal reductions.

Tight horizontal reduction now stops at both depths. TightHorizontal16 independently
confirms1.447x/upper1.459x at530,858B/op, after a passing1.452x screen. The
enlargement helper had acquired an executed wrapper/core call before rejecting
width reduction, so unchanged cross-filter loops did not certify its old complete
timing. Exact rational/frozen pixels, approved native bounds, working limit,
owners, all twelve actuals and both guards pass. Same qualified production and
harness; one selected adapter refresh for changed source and two single-case
drivers. No production/harness rebuild, profile or repeated qualification.

TightHorizontal8 reuses1.414x/upper1.425x at265,502B/op. Its existing byte wrapper/
core prefix is identical. Width256 is strictly less than source513, so both the
old<= and current< checks perform the same comparisons and reject before the
changed enlargement logic. The word branch is unselected, and the selected
cross-helper/dispatcher match the actual archived inputs. The context addition is
unused. Frozen binary64 pixels, including the existing6 independently certified
exact rational alpha halves, native bounds, owners, all twelve actuals and guards
remain valid. The native worker/identity/scaler are byte-identical on the VM.
Zero byte-case builds or timing repeats. No new original raster or shape grid.

The two existing tight256x129 reductions independently confirm on current
production: TightDown8 1.880x/upper1.904x at132,295B/op; TightDown16 1.434x/upper1.445x at264,469B/op.
The selected bounded column filters are unchanged, but new rejected mixed-axis
calls precede them, so the old complete-operation proofs needed reconciliation.
Original byte/word rasters and frozen managed/native hashes, exact rational
pixels, unchanged maximum1/RMSE0.25 native bounds, working limits and independent
owners pass. The same approved integer-output scaler workers retain full timed
packing/halo/filtering/materialization/release; no float-path substitution.
Short means precede independent twelve-actual confirmations; all actuals and
both quiet guards pass, with selected/all confidence. Zero production/harness/
native rebuilds or repeated qualification; two shared selection drivers serve
the four jobs. No profile, new image or shape grid. Stop both confirmed pairs.

Default-limit Horizontal16 independently confirms1.539x/upper1.552x at730,690B/op,
after a1.537x short screen. The actual archived dispatcher lacked the two
mixed-axis entry calls that now reject before its row cache; this invalidated
the old complete-operation timing. The current complete operation preserves
exact rational/frozen pixels, approved native bounds and independent ownership.
All twelve actuals are retained; selected and all-actual confidence bounds and
both quiet guards pass. The already refreshed horizontal-word worker from the
tight case serves the identical native geometry. Zero production/harness/native
builds or repeated qualification; two selection drivers. Stop this case. Next
reconcile the two existing64x32 byte paths, without a new image or shape grid.

Tight64x32_8 independently confirms1.718x/upper1.743x at8,313B/op after a
passing1.747x screen. Its old dispatcher also predated the mixed entry calls.
Exact rational/frozen pixels, unchanged native bounds, tight working limit,
independent owners, all twelve actuals and both guards pass. One selected
adapter refresh retains the same compiler, flags and scaler archive; no
production/harness build or qualification repeat. Two shared selection drivers.
Stop this case.

Default-limit Down64x32_8 initially screened above target: its valid three-actual control is2.121x/
upper2.361x at205,882B/op. This is not an independent passing confirmation.
The original failed metadata-only postcheck is retained: the template expected
halo4 while this pre-existing baseline uses16. Revalidating the original receipt,
binaries and both guards against frozen16 corrects that check without retiming.
No pixel, quality or workload boundary changed. A six-second diagnostic profile
locates81% in the wide row-cache method, chiefly its horizontal accumulation and
indexed loads;18% is the exact original-pixel fallback. The bounded experiment targets this cache primitive only. No new case or image is added.

The accepted row-cache change hoists each paired column's weight and index views
out of the inner tap loop. Zero-initialized padding, safe bounds, sample order,
exact fallback, cancellation and admission remain unchanged. Down64x32_8 now
independently confirms1.662x/upper1.668x,611.55us at205,881B/op:21.6% less managed
time than its preceding valid780.00us screen. Short1.654x precedes twelve actuals;
all actuals, both guards, exact rational/frozen pixels, native bounds and full
ownership pass. The same native worker and both prepared selection drivers are
reused through the experiment. One production build and benchmark recompilation
against that assembly; benchmark source unchanged.

Fresh qualification passes3,483 Release tests per OS, including898 live FFmpeg
comparisons per OS, and the same actual dependency-free Release package's
consumer and all three examples on Windows and Linux. The new qualification is
retained in the evidence. No new raster, grid or baseline adjustment.

Stop Down64x32_8. The other nineteen stopped bicubic cases select mixed or narrow
helpers, or reject this wide helper before the edited loop; entry guards and
other production files are unchanged, so their proofs remain valid. The edited
loop also serves the seven already named open Wide8/Wide16, Down64x32_16,
Down32x16_8/16 and Down16x8_8/16 cases; their older timings need current-loop
evidence. This identifies the exact invalidation without reopening passing
unaffected paths. Full61-case reconciliation and identification coverage stay open.

The two named byte-axis enlargements also independently confirm: tight vertical
1.726x/upper1.737x at1,280,402B/op; default horizontal1.739x/upper1.751x at
1,930,602B/op. The tight vertical dispatcher already matched, but newly executed
identity-height predicates in its selected helper invalidated the old complete
timing. The horizontal result predated the mixed-reject dispatch calls.

Both short means pass before independent twelve-actual confirmations. Frozen
qualified binary64 pixels and the existing one certified exact rational RGB half
are retained for vertical; horizontal remains exact rational. Original native
bounds, limits, source/independent owners, all actuals and both guards pass. Same
qualified production and benchmark assembly; two selected adapters refreshed for
changed owned source with unchanged compiler/flags/scaler and output hashes.
Two shared selection drivers serve four jobs. Zero production/harness builds or
repeated qualification. Stop both cases; continue the finite named checklist.

Default256x129 reductions independently confirm on the same qualified production:
Down8 1.690x/upper1.697x at332,177B/op and Down16 1.309x/upper1.319x at464,342B/op.
Their actual archived dispatcher lacked the mixed-axis rejection calls that now
precede the narrow cache. This is distinct from the recently edited wide loop,
which these fixed tap counts bypass. Short means pass before twelve-actual
confirmations; all actuals, both guards, exact rational/frozen pixels, unchanged
native bounds and full ownership pass. Original integer-output native baselines
are retained. Zero production/harness/native builds or repeated qualification;
two shared selection drivers serve four jobs. Stop both.

TightHorizontalUp16 reuses1.606x/upper1.614x at2,129,311B/op without new timing.
One comparison of35 scoped resize/build inputs finds the actual dispatcher,
selected word cardinal helper and preceding executed helper entries unchanged.
Only unused context code and uncalled byte/wide helper bodies differ. Frozen
exact pixels, approved native bounds, full owners, all twelve actuals and both
guards remain valid. The original approved reference worker is hash-verified in
the retained raw archive; the later default-case worker is not substituted.
The scaler archive is unchanged. Adapter changes add only an unselected mode,
an equal halo constant alias and untimed ready metadata; removing those yields
identical complete timed source. No build, statistical job or qualification
repeat. Stop this case. Next reconcile existing TightWide8 and Wide8; the full
finite checklist and identification coverage remain open.

The existing128x64 wide cases independently confirm: TightWide8 1.687x/upper1.701x at32,889B/op; Wide8 1.534x/upper1.540x at231,229B/op; Wide16 1.212x/upper1.218x at263,991B/op.
Their actual archived dispatchers predate the mixed-axis rejection calls now
executed before the selected paths. Default Wide8/Wide16 also exercise the
edited paired-tap row-cache loop; these current measurements resolve both
uncertainties. TightWide8 retains its bounded column fallback. Exact rational/
frozen pixels, unchanged native bounds, source and full ownership pass.

All short means precede independent twelve-actual confirmations. Every actual
and both quiet guards are retained; selected/all-actual confidence passes. The
same qualified production/harness and original two rasters are reused. Two
selected native adapters are refreshed from changed owned source with identical
compiler/flags/scaler and frozen outputs; two shared drivers serve six jobs.
Zero production/harness builds or repeated qualification. Stop all three.
Five already named wide-cache cases still need current-loop evidence. Next
reconcile TightVerticalUp16 and Tight64x32_16; full coverage remains open.

The next three existing word cases independently confirm: TightVerticalUp16 1.630x/upper1.647x at2,133,096B/op; Tight64x32_16 1.594x/upper1.608x at16,505B/op; Down64x32_16 1.338x/upper1.351x at214,071B/op.
The tight vertical helper acquired executed identity-height predicates after its
older confirmation. Both64x32 dispatchers predate the mixed-axis rejection calls;
the default case also exercises the edited wide tap loop. Current measurements
resolve these complete-operation uncertainties without a production change.

Managed pixels remain exact rational and match frozen outputs. The existing
vertical-word native RGB allowance remains2 only where reference alpha<=16,
1 elsewhere; alpha stays within1 and RGB/alpha RMSE within0.25. Both reductions
keep the unchanged maximum1/RMSE0.25 bounds. Source, limits and independent output
ownership pass. Every short mean passes before its independent twelve actuals;
all actuals and both guards are retained, with selected/all confidence.

Same qualified production/harness and original rasters. Two selected adapters
are refreshed from changed owned source with identical compiler/flags/scaler and
frozen outputs; two shared drivers serve six jobs. Zero production/harness builds
or repeated qualification. Stop all three. Four already named wide-cache paths
and the remaining finite checklist stay open; next TightCross16 and Tight32x16_8.

The next three named cross/32x16 cases independently confirm: TightCross16 1.574x/upper1.579x at1,063,477B/op; Tight32x16_8 1.575x/upper1.580x at2,169B/op; Down32x16_8 1.204x/upper1.208x at199,355B/op.
TightCross16's old word-enlarger had no wrapper/Core call; the new rejection
call executes before its selected cross filter. The cross filter's changed
height comparison is equivalent for the frozen strict enlargement and does not
alone invalidate it. Both32x16 dispatchers predate the mixed-axis rejection
calls, while the default case also executes the edited wide-cache tap loop.
Current complete-operation measurements resolve these uncertainties.

Exact rational/frozen pixels, unchanged maximum1/RMSE0.25 native bounds, source,
working limits and full independent ownership pass. Short means precede each
independent twelve-actual confirmation; every actual and both quiet guards are
retained, with selected/all confidence. Same qualified production/harness and
original two rasters. Two selected adapters are refreshed from changed owned
source with identical compiler/flags/scaler and frozen outputs; two shared
drivers serve six jobs. Zero production/harness builds or repeated qualification.
Stop all three. Three named wide-cache paths and the remaining finite checklist
stay open; next reconcile Up8 and the next largest retained named cases.

The existing upscales and horizontal byte reduction independently confirm: Up8 1.555x/upper1.569x at3,244,622B/op; Horizontal8 1.465x/upper1.475x at465,375B/op; Up16 1.530x/upper1.542x at5,016,578B/op.
Their actual archived dispatcher lacks the mixed-axis rejection calls now
executed before their selected narrow caches. The edited wide loop is uncalled
for these frozen tap counts. Current complete operations resolve the uncertainty.

Upscales remain exact rational. Horizontal8 preserves frozen qualified binary64
pixels and its existing six independently certified exact rational alpha halves;
no managed tolerance is added. Native maximum1/RMSE0.25, source and independent
ownership pass. Short means precede independent twelve-actual confirmations;
every actual and both quiet guards are retained, with selected/all confidence.

Same qualified production/harness and two original rasters. Both standalone
upscale reference workers are reused unchanged; only the selected horizontal
adapter is refreshed from changed owned source with identical compiler/flags/
scaler and frozen outputs. Two shared drivers serve six jobs. Zero production/
harness builds or repeated qualification. Stop all three. Next TightCross8 and
TightWide16; the remaining finite checklist and three wide-cache paths stay open.

The existing byte cross and tight word-wide cases independently confirm: TightCross8 1.502x/upper1.513x at531,910B/op; TightWide16 1.462x/upper1.479x at65,658B/op; Cross8 1.180x/upper1.184x at731,753B/op.
TightCross8's actual archived dispatcher already had both mixed-axis entries.
Its uncertainty instead comes from the later packed-streaming and generated
rejection calls now executed before the selected cross filter. The cross
filter's changed height comparison is equivalent for the frozen strict
enlargement and does not alone invalidate it. Default Cross8 and TightWide16
predate the mixed-axis entry calls. Current complete timings resolve all three.

Exact rational/frozen pixels, unchanged maximum1/RMSE0.25 native bounds, source,
working limits and independent ownership pass. Short means precede independent
twelve-actual confirmations; every actual and both quiet guards are retained,
with selected/all confidence. Same qualified production/harness and original
two rasters. The existing word-wide worker is reused; only the byte cross
adapter is refreshed from changed owned source with unchanged compiler/flags/
scaler and frozen outputs. Two shared drivers serve six jobs. Zero production/
harness builds or repeated qualification. Stop all three. Three named wide-cache
paths and the remaining finite checklist stay open; next Tight32x16_16/Down16x8_8.

The next three existing support cases independently confirm: Tight32x16_16 1.383x/upper1.394x at4,217B/op; Down16x8_8 0.803x/upper0.809x at201,728B/op; Down32x16_16 1.105x/upper1.110x at201,398B/op.
Their actual archived dispatchers lack the mixed-axis rejection calls now
executed before selected paths. Both default cases also execute the edited
wide-cache paired-tap loop. Current complete timings resolve these uncertainties.

Exact rational/frozen pixels, unchanged maximum1/RMSE0.25 native bounds, source,
working limits and independent ownership pass. Each short mean precedes its
independent twelve-actual confirmation; every actual and both quiet guards are
retained, with selected/all confidence. Same qualified production/harness and
two original rasters. Two selected adapters are refreshed from changed owned
source with identical compiler/flags/scaler and frozen outputs; two shared
drivers serve six jobs. Zero production/harness builds or repeated qualification.
Stop all three. Only Down16x8_16 remains among the edited wide-cache paths;
the remaining finite checklist stays open. Next Tight2x9_8 and VerticalUp16.

The existing tiny-output and vertical-enlargement cases independently confirm: Tight2x9_8 1.274x/upper1.286x at194B/op; VerticalUp16 1.260x/upper1.270x at2,516,605B/op; VerticalUp8 1.274x/upper1.282x at1,664,240B/op.
Tight2x9_8's selected generated math and dispatcher are unchanged, but earlier
streaming/packed-streaming helpers removed executed height tests. This changes
its full rejection sequence even though the same filter remains selected.
Both vertical cases predate the mixed-axis rejection calls. Current complete
measurements resolve these uncertainties without a production optimization.

Tiny/word pixels remain exact rational. VerticalUp8 preserves frozen binary64
pixels and its one independently certified exact rational RGB half. The approved
word-vertical native RGB allowance stays2 only where reference alpha<=16,1
elsewhere, alpha within1 and RGB/alpha RMSE within0.25; the byte cases retain
maximum1/RMSE0.25. Source, limits and full independent ownership pass. Every short
mean precedes its independent twelve actuals; all actuals and both guards are
retained, with selected/all confidence. Same qualified production/harness and
two original rasters. Both vertical reference workers are reused; only the tiny
adapter is refreshed from changed owned source with identical compiler/flags/
scaler and frozen outputs. Two shared drivers serve six jobs. Zero production/
harness builds or repeated qualification. Stop all three; full coverage stays open.

The next cross/block cases independently confirm: Cross16 1.268x/upper1.275x at1,263,255B/op; Tight2x9_16 1.146x/upper1.180x at266B/op; Tight4x9_8 1.484x/upper1.488x at266B/op.
Cross16 predates the mixed-axis rejection calls. The two tiny cases retain their
selected generated-word/packed-byte math, but earlier streaming helpers removed
executed height tests. Current complete timings resolve these uncertainties.
Exact rational/frozen pixels, maximum1/RMSE0.25 native bounds, source, limits and
full ownership pass. Short means precede independent twelve actuals; every
actual and both quiet guards are retained, with selected/all confidence. Same
qualified production/harness and original rasters. The cross worker is reused;
two tiny adapters are refreshed with identical compiler/flags/scaler and frozen
outputs. Two shared drivers serve six jobs; no production/harness build or
repeated qualification. Stop all three.

The two existing8x17 proofs also reuse unchanged paths: Tight8x17_8 1.089x/upper1.100x at665B/op; Tight8x17_16 0.973x/upper0.981x at1,209B/op.
The saved comparison of35 scoped inputs per snapshot establishes unchanged
selected streaming kernels, dispatcher and prior column entries. Mixed entries
retain their wrapper/prelude and reject at the same first depth test or strict
width8<513 test, before changed preference clauses. The wide-cache entry prefix
is unchanged and rejects before the edited loop: even minimum cache storage
exceeds the fixed4KiB remainder. Changed byte-only bodies are unselected for
word input; later cross/enlargement helpers are uncalled after streaming succeeds.
The context addition is unused. Both original workers/identities and scaler
archive are byte-identical on the VM; excluding an unselected mode, equal halo
alias and untimed ready metadata yields identical timed adapter source. Exact
pixels, native bounds, full owners, all twelve actuals and both guards remain
valid. Zero new jobs/builds/qualification for these two; stop them. The finite
checklist remains open; continue the remaining ten named bicubic cases.

The three existing streaming/block cases independently confirm: Tight16x8_8 1.084x / upper 1.090x, 633 B/op; Tight4x9_16 1.025x / upper 1.030x, 409 B/op; Tight16x8_16 0.984x / upper 0.995x, 1,145 B/op.
The 16x8 selected streaming kernels changed capacity, workspace and per-column
row-block preparation; their older dispatchers also lack the two mixed rejection
calls. The 4x9 word kernel and dispatcher are unchanged, but the earlier streaming
rejection removed an executed height test. Current complete timings resolve
these uncertainties. Exact rational/frozen pixels, strict native RGB/alpha
maximum1 and RMSE0.25, limits and full ownership pass. Every short mean is read
before its independent twelve-actual job; all actuals and both quiet guards are
retained. Same qualified production/harness and original rasters throughout.
The byte16x8 worker is reused; two word adapters are refreshed with the same
compiler, flags and scaler and frozen outputs. Two shared drivers serve six jobs;
zero production/harness builds or repeated qualification. Stop all three.

Three existing default-cache/streaming cases independently confirm: Down16x8_16 0.889x / upper 0.893x, 202,231 B/op; Tight8x4_8 0.765x / upper 0.772x, 249 B/op; Tight8x4_16 0.643x / upper 0.651x, 377 B/op.
The default word16x8 case executes the edited paired-tap wide-cache loop and
mixed rejection calls; all eight existing paths affected by that production
change now have current evidence. Both tight8x4 selected streaming kernels
moved row-block preparation inside each output column and removed the executed
height predicate; later uncalled dispatch additions alone are not invalidators.
Exact rational/frozen pixels, strict native RGB/alpha maximum1 and RMSE0.25,
limits and full ownership pass. Every short mean precedes independent twelve;
all actuals and both quiet guards are retained on the same qualified build.
The word16x8 worker is reused; two8x4 adapters are refreshed with unchanged
compiler, flags, scaler and frozen outputs. Two shared drivers serve six jobs;
zero production/harness builds or repeated qualification. Stop all three.

Three existing tiny packed/generated cases independently confirm: Tight2x2_8 0.718x / upper 0.745x, 138 B/op; Tight4x2_8 0.691x / upper 0.695x, 153 B/op; Tight4x2_16 0.506x / upper 0.509x, 185 B/op.
The selected generated-byte and packed byte/word kernels changed whole-height
setup to per-column row blocks and removed executed height predicates. Earlier
streaming/packed rejection paths also changed. Actual measured source hashes
establish these invalidations; parent revisions alone do not establish reuse.
Exact rational/frozen pixels, strict native RGB/alpha maximum1 and RMSE0.25,
limits and full ownership pass. Short means precede independent twelve;
all actuals and both quiet guards are retained on the same qualified build.
Three selected adapters are refreshed with the same compiler, flags and scaler
and frozen outputs; two shared drivers serve six jobs. Zero production/harness
builds or repeated qualification. Stop all three. Only Tight2x2_16 remains in B12.

The final existing word2x2 generated case independently confirms 0.458x / upper 0.487x, 154 B/op.
Its selected kernel changed whole-height preparation into per-column row blocks
and removed the executed height predicate; preceding rejection predicates also
changed. Current complete timing resolves the uncertainty. Exact rational/frozen
pixels, strict native RGB/alpha maximum1 and RMSE0.25, limits/full ownership,
all twelve actuals and both quiet guards pass. The short mean precedes independent
twelve on the same qualified production/harness. One selected adapter refresh
uses the same compiler/flags/scaler/frozen outputs; two one-case drivers serve
two jobs. Zero production/harness builds or repeated qualification. Stop it.

All61 fixed bicubic selectors now have valid current-path passing evidence,
including explicitly reused unchanged proofs. No bicubic case or grid is added.
B01-B15 reconciliation is closed. Full required warm coverage remains open for
the four named Identify boundaries C01-C04; equivalent public-API validation,
metadata and descriptor ownership must be established before timing them.

The subsequent [PNG Identify change](WARM-PNG-IDENTIFY.md) affects only resident/Stream PNG inspection. Pixel Scan/shared CRC, editing and every resize operation retain their executed bodies and native workers; all61 bicubic proofs remain valid. New full3,517-tests/OS and the same actual package consumer/all three examples pass on Windows/Linux. No bicubic retiming or retuning.

The subsequent [JPEG Identify change](WARM-JPEG-IDENTIFY.md) leaves pixel decoder preflight, PNG inspection, edits and resizes unchanged. Keep all61 bicubic proofs stopped; new full3,535-tests/OS and same actual package consumer/all three examples pass on both OS. No bicubic retiming.

## Sixty-four-tap byte quotient bounds

Default RGBA8 513×259→64×32 now uses the existing quotient-error calculation
to certify rounding. Its former fixed color margin sent certain pixels through
the original two-dimensional tap loop. The new bound avoids those replays;
horizontal/vertical arithmetic, direct fallback, half handling, alpha, resource
admission, work charges, cancellation and independent output remain unchanged.
There is no extra allocation or output cache. For at most64 taps per axis,
the existing absolute unit-error bound covers both accumulation orders; the
quotient perturbation bounds retain direct evaluation wherever rounding could differ.

The complete-operation three-actual screen is **418.52µs versus369.03µs**,
**1.134x/upper1.215x**, **205,884B/op**:31.56% less managed time than611.55µs.
This is a qualified improvement and an open parity gap. No routine twelve-actual
confirmation was run while the mean remained above1. Both versioned quiet guards
pass, all actuals are retained, and exact rational/frozen pixels, source
preservation and independent-owner checks pass.

Two neighboring shared-method controls pass: Wide8 at1.535x/upper1.891x,
231,229B/op; word Down64x32_16 at1.180x/upper1.335x,214,070B/op. Their calculation
and selection outcomes remain unchanged, so their existing independent proofs
remain selected. Only default byte Down64x32_8 newly takes the quotient branch.
The tight byte64×32 case refuses the unchanged cache admission and keeps its
separate helper. Other206 numeric comparisons/all60 passing proofs remain valid.

The original two rasters, native worker binaries/build pins, halo16 for64×32
andhalo8 for128×64, normal/scalar comparisons and native RGB/alpha max1/RMSE0.25
remain fixed. Full conversion, filtering, owned output, unpremultiplication,
quantization and release remain timed. Historical profiles predate a changed hot
loop; no current attribution claim, fresh profile, grid or driver build was needed.

Qualification passes **3,707 Release tests per OS**:1,374 internal,1,435 public
and898 live FFmpeg, plus209 scalar resize checks per OS. Six new public checks
pin dyadic half ties, clamped edges, tiny/zero alpha, odd output and independent
owners against direct evaluation. The same actual Release package's consumer
and all three examples pass on Windows and Linux. One producer build per OS,
three short jobs, zero harness/selector/native builds during measurement.
The [numeric evidence](warm-current-bicubic-evidence.json) retains exact sources,
raw archives, controls, uncertainty and qualification. Prior release artifacts
remain unchanged. The finite207-case parity objective stays open.
