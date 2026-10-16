# Current bicubic evidence reconciliation

Twenty of the existing 61 named bicubic cases have valid current-path evidence.
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
