# Material warm coverage

Current architectural phase, 2026-11-08. **283 material required paths** remain
in scope; the 125 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion needs current passing evidence for every named path, with scope stated.

[Current preparation/storage audit](ARCHITECTURAL-PREPARATION-AND-STORAGE.md)
reconciles one-shot/prepared boundaries and measures three WebP admission lifetimes.
The [matching-index handoff](WEBP-MATCHING-POPULATION-STORAGE.md) retains six
unchanged lossless costs. [Four-block histograms](VP8-FOUR-BLOCK-HISTOGRAM.md)
refresh all nine lossy encode controls. [Shared live JPEG windows](JPEG-LIVE-ENTROPY-WINDOW.md)
refresh23baseline costs and reuse6unchanged progressive comparisons. Historical
experiments remain linked separately, with raw evidence retained.
[Horizontal alpha publication](WEBP-HORIZONTAL-ALPHA-PUBLICATION.md) refreshes
two delivery costs; four unchanged VP8 model/validation costs and six lossless
reader costs retain source-valid evidence.

## Current family status

| Required family/boundary | Current evidence and remaining work |
| --- | --- |
| Resident lossless WebP | [Matching/population storage](WEBP-MATCHING-POPULATION-STORAGE.md) reuses the ordinary index after its final reader. Four lossless controls pass/stop; opaque/photo remain uncertain (uppers1.015/1.008). Direct photo/diagram allocation falls134736B each, admitted peaks3470059/3251328B unchanged; retained scratch zero. Exact packets and source settings remain. No causal CPU gain inferred from refreshed costs. [Isolated prediction](WEBP-ISOLATED-GREEN-PREDICTION.md) retains earlier numerical/CPU evidence and adverse history. |
| Resident VP8 encoding | [Four-block histograms](VP8-FOUR-BLOCK-HISTOGRAM.md) recycle dead coefficients after all transform reads, with alias/non512 fallbacks unchanged. Complete effort2 saves101.15µs/3.69%raw/3.47%normalized; both confidence views separate, native−0.23%. Current2638.29/native2504.45µs,upper1.058 remains134µs gap; GC406705B unchanged. Nine affected current costs:6pass/1gap/2uncertain. Effort6 passes its predeclared four-launch envelope,upper0.998836; photograph/Alpha8 remain uncertain, Alpha16 passes. Diagram/dense admitted peaks397205/930669B and retained0 remain. No new scratch payload/owner/cache or other causal gain claim. [Earlier preparation](VP8-SMALL-BLOCK-PREPARATION.md) retains its raw gain/normalized uncertainty as history. Passing cases stop. |
| Other WebP encoding boundaries | [Storage stage](WEBP-MATCHING-POPULATION-STORAGE.md) retains six unchanged lossless costs:4pass/2uncertain. Full-depth normalization and fresh awaited nonseekable async ownership remain timed. Transparent alpha allocation falls70560B; admitted peak3656309B/retained0 unchanged. [Current histogram stage](VP8-FOUR-BLOCK-HISTOGRAM.md) refreshes alpha8/16: Alpha16 passes, Alpha8 remains uncertain. Native contexts retain their declared preparation boundary. Passing cases stop; uncertainty is not a demonstrated optimization gap. Earlier costs remain linked history. |
| PNG decoding/coding validation | [Current resident controls](PNG-CURRENT-FRAMING-COSTS.md): all47requiredWarmPngdecode+4codingvalidation cases now have independent evidence;45pass/stop,6gaps,none missing or uncertain. The final25controls all pass; prior liveword2 remain gaps. Largest gaps are liveRGBA8(~145µs), tightRGBA16(~124µs) andRGB16(~115µs); ordinary/live/tight word controls retain their exact bounds. No tuning of passing packed/indexed/tRNS/Paeth/Adam7/stored paths. [Shared-root control](PNG-PREPARED-PREFIX.md) separately remainsgap; stream/composition/inspection boundaries keep their own finite coverage. No production/build/package changes or historical causal-gain claim. |
| PNG encoding | [Streaming token frequencies](PNG-STREAMING-TOKEN-FREQUENCIES.md) removes the greedy token rescan with the existing335int workspace. Liveword independently saves516–550µs and plainword557µs, all filtered/allactual raw/normalized intervals separate; ordinary/live/forward word Fastest pass/stop. Byte filteredgain113µs has allactual uncertainty. All15 complete one-shot costs refresh:12pass/2gap/1uncertain. ByteFastest(~87µs) andGray8(~60µs) remain gaps; wordSmallest remains uncertain. Exact packets,384KiB admission andretained0 remain; primaryGC unchanged, Gray8measures−408B without a capacity claim. Earlier [lazy](PNG-LAZY-VIEW-PREPARATION.md)/[greedy](PNG-GREEDY-VIEW-PREPARATION.md) independent gains remain linked history. |
| WebP image reading | [Horizontal alpha publication](WEBP-HORIZONTAL-ALPHA-PUBLICATION.md) saves196.04µs/3.21%raw/3.25%normalized on complete resident decoding, exact pixels and unchanged238384B/op. Residentupper1.002 remains uncertain; streamupper0.992 passes/stop,238480B/op. Retained output531468B; no new owned capacity. Two costs refresh; [prepared models](VP8-PREPARED-COEFFICIENT-MODEL.md) retain four unchanged opaque/live/validation costs and their155.50µs opaque gain. Combined VP8 scope:2pass/3gaps/1uncertain. [VP8L final publication](VP8L-FINAL-PUBLICATION.md) retains six unchanged lossless costs:2pass/4gaps,155.66µs resident gain and explicit19.25µs palette regression. Passing cases stop. |
| JPEG decoding | [Paired AC windows](JPEG-PAIRED-AC-WINDOW.md) save113.83µs/6.92%raw/6.75%normalized onRGB444 and15.91µs/3.02%raw on sparseCMYK; bothfiltered/allactual intervals separate. Scan-owned12bit/UInt32 tables cost32,848/16,424 extraGC B/op; RGBadmittedpeak541253→574069B, retainedoutput531468B unchanged, newretainedscratch0. Emptyplans allocate nothing; localchoice precedes sharedadmission, refusalterminal. All29current costs refresh (inclprogressiveconstructor layout):14pass/14gap/1uncertain. Native444 nowpasses; callerCMYK nowgap. Ownedgray movedpass→uncertain (679.83/native679.12µs upper1.009); retainadversepoint andresolvebeforeitsoptimization. LibraryJPEG8adversepoint1589→1642µs remainspassing; no broadernonregressionclaim. CurrentRGB80µs gap isbelow liveopaqueVP8/mixedwordresize priorities. [Live windows](JPEG-LIVE-ENTROPY-WINDOW.md) andearliergains remainlinkedhistory. |
| Bicubic columns | [Wide shared preparation](BICUBIC-WIDE-PREPARATION.md) saves24.28/94.49µs (4.81%/15.07%raw) on byte/word128×64 controls, unchanged113B/op and4KiB admission. Both remain gaps, uppers1.394/1.329; native clocks−0.65%/−3.85% disclosed. [Small word preparation](BICUBIC-WORD-COLUMN-PREPARATION.md) and [small byte preparation](BICUBIC-COLUMN-PREPARATION.md) reuse unchanged gains and gaps1.223/1.363. Four drivers share the same result-tail geometry helper; kernels and exact pixels remain. Other affected named H costs remain historical. |
| Bicubic streaming | [Shared streaming blocks](BICUBIC-SHARED-STREAMING.md) use two schedulers/four exact kernels within4KiB, saving66.62/51.17µs on byte/word8×17. Eight current controls: six pass/stop; RGBA8 8×17/4×9 retain gaps (uppers1.127/1.269). Allocations unchanged. |
| Lanczos3 | [Paired certificate preparation](LANCZOS-PAIRED-CERTIFICATE-PREPARATION.md) confirms175.99µs/4.20% elapsed gain on prepared word enlargement with counterbalanced immutable builds. Standalone raw/native clock movement is disclosed. All eight affected costs are current: five pass/stop; prepared byte/word reduction and word enlargement retain gaps (uppers1.158/1.097/1.052). Capacity is unchanged. Preserve exact Q30/rational/certified results and native float/prepared-state boundaries; [shared scheduling](LANCZOS-SHARED-ROW-DRIVER.md) remains linked history. |
| Complete bilinear enlargement | [Current boundary audit](bilinear-current-boundary-evidence.json) reuses the qualified immutable build without production changes or rebuilds. Existing RGBA8 Up8 measures2357.04µs/native2155.92µs, ratio1.093/upper1.134/lower.971: uncertain.102891B/op includes fresh~103KBhorizontal preparation and independent output ownership; the unchanged4MiBdefault pool reuses output backing. Older2914.62µs/2235512Bcost is superseded, not paired causal gain evidence. Native graph/context preparation remains retained and labeled. Stop tuning this uncertain case while larger demonstrated gaps remain. |
| Complete bicubic enlargement and small-support word publication | [Shared word preparation](BICUBIC-SHARED-WORD-PREPARATION.md) removes repeated divisor/margin setup across pair/quartet/cached writers. All15word costs current:6pass/9gaps. Full/bounded enlargement and default horizontal/vertical enlargement pass; stop them. Existing division, certified rounding/repair and rational pixels remain. DefaultRGBA8 [unchanged-build confirmation](bicubic-byte-enlargement-current-evidence.json) resolves old uncertainty:3,108.21/native3,037.83µs,upper1.026/116Bop,70.38µs gap; no causal historical gain. Larger codec gaps take priority. No owned capacity/retention change. [Publication pipeline](BICUBIC-WORD-PUBLICATION-PIPELINE.md) retains earlier scoped gains/history. |
| Mixed-axis RGBA16 bicubic | [Shared preparation](BICUBIC-SHARED-WORD-PREPARATION.md) independently saves41.73us/3.94%raw (2.85%normalized) on Mixed16 and83.80us/8.86%raw (8.94%normalized) on Cross16; both confidence views separate. TightVertical16 regresses11.96us/1.66%raw/1.82%normalized, also separated. Current default/bounded mixed uppers1.217/1.204; tight vertical1.248; all remain gaps. Alloc114/114/113B unchanged; cross measures−1B without a storage claim. Arithmetic/ownership/cancellation contracts unchanged. [Four-row scheduling](BICUBIC-MIXED-WORD-SCHEDULING.md) remains linked history. |
| Text and drawing | All 14 additions below now have measured evidence. [Layout/fitting and approved references](TEXT-DRAWING-CURRENT.md) close the old reference decisions. Four layout/fitting controls and RGBA16 compositing pass and stop. [Active-edge scheduling](TEXT-ACTIVE-EDGE-SCHEDULING.md) saves179.16/170.49µs (48.55%/47.38%) on prepared drawing8/16, with exact frames. Allocation+8672B/draw to92384; admitted Latin workspace stays48544B, retained scratch zero. Current uppers5.230/5.226 remain gaps; eight-frame Arabic/wrapped/fitting generalization is correctness, not broader CPU proof. [Owned font storage](FONT-OWNED-STORAGE.md) saves258.73µs/89.04%raw against a contemporary original; current31.86/native13.42µs,upper2.396 remains gap. GC757597→176B/op; the isolated load retains757076B idle within the existing2slot/4MiBdefault cap. Rooted loads bypass the cache and retain exact charges; fullcopy/eager parsing stay timed. No physical-peak/general-parser/governedCPUclaim. Other current fill/copy/composite costs remain in the summary. |

The latest [paired JPEG stage](JPEG-PAIRED-AC-WINDOW.md) refreshes all29JPEG
read/validation costs, includingprogressive Huffmanobject layout. Otherfamilies,
including [streaming PNG census](PNG-STREAMING-TOKEN-FREQUENCIES.md), retain valid
unchanged-path costs. Windows/Linux eachpass5,883full/live checks, affected407
internal/375public pernormal/scalar andthe SAMEactualReleasepackage/threeexamples.
The package exercises originalauthored groupedAC pixels, sync/async scanownership,
terminal aggregate refusal, recovery and independent survivors. All283required
paths remain; no whole-libraryCPU or125-row actual-package release refresh claim.

Earlier unselected [VP8 windows](VP8-ROW-WINDOW-EXPERIMENT.md),
[frame phases](VP8-FRAME-PHASE-PREPARATION-EXPERIMENT.md),
[JPEG table views](JPEG-BORROWED-TABLE-VIEWS-EXPERIMENT.md),
[entropy windows](JPEG-SHARED-ENTROPY-WINDOW-EXPERIMENT.md),
[PNG token/code preparation](PNG-PREPARED-TOKEN-CODES-EXPERIMENT.md) and
[resize phases](BICUBIC-COLUMN-PHASE-EXPERIMENT.md) and
[paired JPEG rows](JPEG-PAIRED-INVERSE-ROWS-EXPERIMENT.md) and
[PNG prefix preparation](PNG-PREFIX-PREPARATION-EXPERIMENT.md) and
[live-word hash preparation](PNG-FASTEST-LIVE-WORD-STUDY.md) retain exact restoration,
adverse/inconclusive results and raw evidence separately. They establish no
current production gain or permanent family ban.
[Source-valid JPEG attribution](JPEG-CURRENT-ENTROPY-ATTRIBUTION.md) remains useful
for unchanged ordinary entropy/transform bodies. Live JLB546 attribution belongs
to the shared-window report; instrumented elapsed is not timing evidence.

## Finite required inventory

The [pinned original inventory](https://github.com/lokad/image/blob/27fc0425be9e21d913e7bd049fe3ce629bf28955/docs/benchmark-pre-cleanup.md)
names 257 controls with their original inputs, shapes, depths, sampling/settings
and boundaries. Every surviving row remains an individually required checklist
item, including rows omitted from the current summary. Its historical costs are
not current proof.

Only these four boundaries are retired:

- PNG resident ImageBudget quota.
- JPEG420 ImageBudget quota.
- VP8 caller work budget32M.
- Optional JPEG CompactColor encoder.

Their APIs/encoders were removed; retained caller-root cases still qualify shared
admission. The remaining **253** base controls plus the **30** named additions
below give **283**, without adding a shape/depth/filter/quality grid. A later case
requires evidence of a distinct required algorithm or public boundary omitted
from this finite set. Differences in default option values are accepted.

Four required algorithm additions retain their frozen recipes:

- `DiagramLossyTrellis8`: existing 513x259 diagram, Q75/effort6. Coefficient
  dynamic-programming trellis per luma candidate, absent from method4. Complete
  6,818-byte packet SHA256
  `3c98407656841ff97b185e241e76f750f036c972cfe2a0b704ae2eb35cf0adee`.
- `DiagramLossySelectedTrellis8`: same existing diagram, Q75/effort5. The
  production `method == 5` branch reconstructs only the selected modes with
  trellis; effort6 instead evaluates candidates with trellis. This distinct
  required path was absent from the earlier timing checklist. Reuse the frozen
  7,026-byte packet SHA256
  `0e994ebda4b10234f2e9f00caf7ea6c27ae03ce82c1f3cada8eda355430cba68`.
  Historical baseline and adverse/inconclusive pricing observations remain in
  the linked history; current costs are owned by
  [per-block preparation](VP8-SMALL-BLOCK-PREPARATION.md). No extra content/settings grid.
- `DiagramLossyDistortion8`: same original diagram, Q75/effort2. Original
  prediction-error mode selection before chosen reconstruction, distinct from
  RDO/trellis. Complete 7,330-byte packet SHA256
  `325a9d5b55b300db2554681735642edcdc8dcb866a88760501d3fe95fac1423b`.
- `EncodePredictable16`: original 3000x3000 RGBA16 gradient after horizontal flip,
  default Optimal; R=16x/G=16y/B=8(x+y)/A=65535. Bounded content admission and
  per-row prediction are distinct from the None-filter controls. Keep exact
  samples and the declared complete-packet size gate (<2 MiB), not a new byte
  identity claim. Native level6/pred=mixed and the retained four-operation
  batches stay frozen. [Evidence](WARM-PNG-WORD-MATCH.md).

Twelve required read-boundary additions use their existing original recipes.
Async cases acquire fresh nonseekable input; other probes use fresh seekable
input; validators borrow an encoded span. Inspection, container identification,
complete coding validation and pixel decoding are distinct operations.

| New control | Existing input | Distinct required work/boundary |
| --- | --- | --- |
| ProbeDimensionsPng | OriginalExif PNG8, 513×259 | IHDR prefix acquisition, without raster/tail validation |
| ProbeDimensionsJpeg | OriginalExif baseline420, 513×259 | SOF acquisition before compressed scans |
| ProbeDimensionsWebp | OriginalExif VP8L alpha, 513×259 | RIFF/VP8 dimension header acquisition |
| ProbeDimensionsJpegAsync | Same JPEG | Actual async prefix acquisition and ownership |
| ValidatePng8 | OriginalExif PNG8, 513×259 | Inflated rows/filter/checksum without RGBA |
| ValidatePng16 | OriginalExif PNG16, 513×259 | Required 16-bit row/workspace boundary |
| ValidateIndexedAdam7 | AuthoredPng c3-d4-adam7-all-filters, 19×17 | Seven passes and palette-index validity |
| ValidateJpegBaseline420 | OriginalExif baseline420, 513×259 | Huffman/coefficient validation without inverse/render planes |
| ValidateJpegProgressive420 | OriginalExif progressive420, 513×259 | Distinct spectral/refinement/progression checks |
| ValidateVp8lAlpha | OriginalExif VP8L alpha, 513×259 | Complete main syntax and auxiliary numeric validation; no discarded main raster/inverses |
| ValidateVp8Alpha | OriginalExif lossy alpha, 513×259 | VP8 token/context validation plus compressed-ALPH syntax; no discarded pixel planes |
| ValidatePng16Async | Same PNG16 | Complete asynchronous acquisition, retained input capacity and cleanup |

Validation is optional for replacement readiness, but these supported public
boundaries remain required for full performance completion. Return equivalent
requested metadata or validate complete coding with matching input delivery,
ownership and actual async completion. Do not substitute normalization/RGBA output
when the requested operation does not require it.

Fourteen required text/drawing additions use the existing raster/font/text recipes.
The [original omission audit](MATERIAL-TEXT-DRAWING-COVERAGE-AUDIT.md) records their
provenance; its former pending-reference/timing statements are historical. Use the
approved scopes and current evidence above.

| Required control | Public boundary | Why material |
|---|---|---|
| DrawingFill8 | FillRectangle, RGBA8 | Packed byte replacement including hidden RGB; supplied color, clipped rectangle. |
| DrawingFill16 | FillRectangle, RGBA16 | Distinct ushort destination loop and full-precision color. |
| DrawingCopy8 | CopyRegion, separate RGBA8 buffers | Exact rectangle placement into a borrowed existing canvas; no new output frame. |
| DrawingCopy16 | CopyRegion, separate RGBA16 buffers | Distinct ushort span-copy boundary. |
| DrawingCopyOverlap8 | CopyRegion, overlapping RGBA8 rectangle | Required snapshot/reverse traversal; two moves per pair, including reverse restoration in both totals. |
| DrawingComposite8 | Composite, RGBA8 source/canvas | Straight-alpha rational source-over, including zero and partial alpha. |
| DrawingComposite16 | Composite, RGBA16 source/canvas | Full-precision samples and 64-bit coverage arithmetic. |
| FontLoadRegular | TrueTypeFont.Load, existing DejaVu Sans regular | Copy/validate supplied bytes and release an independently retained face. |
| TextLayoutLogicalWrapped | TextLayout.Create, Logical | Default logical mapping, kerning, wrapping, bounds and retained layout ownership. |
| TextLayoutOpenTypeLatin | TextLayout.Create, OpenType Latin | Required substitution/positioning path on the existing Latin kerning control. |
| TextLayoutOpenTypeArabic | TextLayout.Create, OpenType Arabic | Existing ArabicMarksDigits control exercises joining, bidi and mark positioning. |
| TextFitOpenType | TextLayout.TryFit, existing ConstrainedFit | Complete bounded size search, releasing rejected candidates and retaining the winner. |
| DrawText8 | ImageDrawing.DrawText, RGBA8 | Prepared-layout rasterization and alpha compositing into a borrowed canvas. |
| DrawText16 | ImageDrawing.DrawText, RGBA16 | Distinct full-precision destination/color behavior with the same prepared layout. |

## Evidence and execution rules

- Resolve missing or uncertain measurements before assuming optimization is
  needed. Stop cases with applicable confirmed passing evidence; changes to their
  actual production paths can invalidate that evidence.
- Preserve complete encoder packets and declared metadata exceptions, exact
  lossless/managed pixels, lossy reconstruction/alpha gates, ownership, cancellation,
  shared admission and meaningful local bounds. Equal numeric quality is no match.
- Compare equivalent one-shot or repeated prepared-state operations. Include
  necessary setup, workspace, normalization, independent output ownership and
  disposal; disclose retained native graphs/codec contexts. Do not remove managed
  work merely to improve the ratio.
- Use the dedicated quiet VM sequentially, one CPU per operation, Release,
  versioned idle/process guards, pinned sources/producers/settings and output
  checks. Keep all actuals/brackets and adverse results. Profiling/counts are
  attribution, not component timings or savings.
- Prioritize useful absolute time savings in codecs/resizing. Sub-microsecond
  probe/copy gaps remain in scope without displacing millisecond work. No fixed
  continuation percentage, one-primitive mandate or permanent closed-family list.
- Text is the approved Drawing2.1.7/Fonts2.1.3 complete public-API integration,
  including multiple layout calls and the full fitting search, not bare shaping.
  FreeType2.13.2 font loading discloses eager/lazy validation differences. RGBA16
  compositing is ImageSharp3.1.12 Rgba64 SrcOver, not FFmpeg. Existing scoped visual
  and native bounds remain distinct from exact managed contracts.
- Reuse immutable builds and qualification for unchanged inputs/paths. Focused
  representative checks precede broad Windows/Linux/scalar/differential and actual
  same-package qualification at coherent accepted production milestones. A release
  refresh measures every displayed row on the actual immutable package DLL.

Current evidence ownership and invalidation:

- WAF542 owns Native8 and Vp8Stream costs after fused ALPHcompression1/filter1
  final-green publication. Resident remains uncertain; stream passes. Otherfilters
  retain old extraction/inversion; rawalpha and codingvalidation do not enter it.
- VPM524 owns four unchanged opaque/live/validation costs. Earlier VCT508 classes
  and CPT479 successful-only publication remain; failed private cursor is not
  observed/resumed and every poll/public cleanup stays.
- LPF476 owns six unchanged VP8L reader costs after final inverse/publication;
  its former VP8 costs are superseded. Encoders, other codecs, resizing and text
  retain their own source-valid evidence.
- VHB517 owns nine lossy WebP encode costs after four-block histogram preparation;
  affected unmeasured H boundaries remain historical. Earlier profiles are leads,
  not current whole-execution CPU shares.
- RPA503 owns fifteen current word bicubic controls after shared preparation; source-valid byte controls
  remain attributed separately. Unrefreshed allocator/filter/pipeline H costs stay
  historical. Lanczos owns eight refreshed one-shot/prepared controls; exact
  certificates, repair, plans/owners and portable fallbacks remain required.
- Other current owners and their precise source scopes are linked in the family
  table. Reuse confirmations only when relevant paths/boundaries remain unchanged.

Current priorities: equivalent preparation/reuse boundaries, substantial WebP
workspace lifetimes, shared resize execution and further codec work selected from
attribution. Keep passing cases stopped. Text/drawing proceeds independently;
pending references never block justified codec/resizing work. PLAN.md stays ignored.

All earlier reports, raw evidence, release packages and source-specific
invalidation notes are retained through the linked history. They do not establish
current blanket qualification or permanently close algorithm families.
