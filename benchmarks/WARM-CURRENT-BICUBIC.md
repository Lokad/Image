# Current bicubic evidence reconciliation

Seventeen of the existing 61 named bicubic cases have valid current-path evidence.
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
