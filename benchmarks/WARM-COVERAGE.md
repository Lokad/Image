# Material warm coverage

Current architectural phase, 2026-11-08. **283 material required paths** remain
in scope; the 126 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion needs current passing evidence for every named path, with scope stated.

[Current preparation/storage audit](ARCHITECTURAL-PREPARATION-AND-STORAGE.md)
reconciles one-shot/prepared boundaries and measures three WebP admission lifetimes.
The [matching-index handoff](WEBP-MATCHING-POPULATION-STORAGE.md) retains six
unchanged lossless costs. [Four-block histograms](VP8-FOUR-BLOCK-HISTOGRAM.md)
own all nine lossy encode controls. [Completed JPEG output](JPEG-COMPLETED-OUTPUT.md)
owns twelve read costs, with seventeen unchanged paired-AC controls; [model rows](VP8-MODEL-ROW-STRIDE.md) own six VP8
pixel/validation costs. Six lossless reader controls retain source-valid evidence.
Historical experiments remain linked separately, with raw evidence retained.

## Current family status

| Required family/boundary | Current evidence and remaining work |
| --- | --- |
| Resident lossless WebP | [Matching/population storage](WEBP-MATCHING-POPULATION-STORAGE.md) reuses the ordinary index after its final reader. Four lossless controls pass/stop; opaque/photo remain uncertain (uppers1.015/1.008). Direct photo/diagram allocation falls134736B each, admitted peaks3470059/3251328B unchanged; retained scratch zero. Exact packets and source settings remain. No causal CPU gain inferred from refreshed costs. [Isolated prediction](WEBP-ISOLATED-GREEN-PREDICTION.md) retains earlier numerical/CPU evidence and adverse history. |
| Resident VP8 encoding | [Four-block histograms](VP8-FOUR-BLOCK-HISTOGRAM.md) recycle dead coefficients after all transform reads, with alias/non512 fallbacks unchanged. Complete effort2 saves101.15µs/3.69%raw/3.47%normalized; both confidence views separate, native−0.23%. Current2638.29/native2504.45µs,upper1.058 remains134µs gap; GC406705B unchanged. Nine affected current costs:6pass/1gap/2uncertain. Effort6 passes its predeclared four-launch envelope,upper0.998836; photograph/Alpha8 remain uncertain, Alpha16 passes. Diagram/dense admitted peaks397205/930669B and retained0 remain. No new scratch payload/owner/cache or other causal gain claim. [Earlier preparation](VP8-SMALL-BLOCK-PREPARATION.md) retains its raw gain/normalized uncertainty as history. Passing cases stop. |
| Other WebP encoding boundaries | [Storage stage](WEBP-MATCHING-POPULATION-STORAGE.md) retains six unchanged lossless costs:4pass/2uncertain. Full-depth normalization and fresh awaited nonseekable async ownership remain timed. Transparent alpha allocation falls70560B; admitted peak3656309B/retained0 unchanged. [Current histogram stage](VP8-FOUR-BLOCK-HISTOGRAM.md) refreshes alpha8/16: Alpha16 passes, Alpha8 remains uncertain. Native contexts retain their declared preparation boundary. Passing cases stop; uncertainty is not a demonstrated optimization gap. Earlier costs remain linked history. |
| PNG decoding/coding validation | [Ordinary/slab publication](PNG-NORMAL-CURSOR-PUBLICATION.md) removes three exception-only publication regions from abandoned private readers, preserving exact completed cursors/public errors/partial stores/lookahead/polls/ownership. Tight/default RGBA16, RGB16 and tight-live RGBA16 save192/125/216/217us; all four former gaps pass. Separate shared-root pair saves260us and passes upper0.920, retaining1,638,214B/op, admitted1,766,641B and retainedpair1,062,936B. [All51controls](PNG-CURRENT-FRAMING-COSTS.md):49pass/1gap/1uncertain;43 refreshed,8 unchanged providers reused, no allocation change. Separate materialized-live word remains2919.94/native2894.87us upper1.012 gap; livebyte upper1.004 uncertain. Stop passing cases; resolve uncertainty before tuning. Older stream/composition costs remain historical. |
| PNG encoding | [Exact length classification](PNG-LENGTH-CLASSIFICATION.md) replaces repeated range search with the exact integer-logarithm formula. All15 complete one-shot costs refresh:13pass/0gaps/2uncertain. WordSmallest now passes/stops; ByteFastest upper1.017 and Gray8 upper1.047 remain uncertain, requiring confidence before more tuning. Word raw180.09us/1.00% intervals separate both views, normalized intervals overlap; byte84.29us point gain has allactual uncertainty. No robust normalized/blanket gain claim. All packets remain exact,384KiB admission/retained0scratch and primaryGC820916/2437660Bop unchanged. [Streaming census](PNG-STREAMING-TOKEN-FREQUENCIES.md) and prior preparation remain history. |
| WebP image reading | [Power-of-two model rows](VP8-MODEL-ROW-STRIDE.md) prepare one1,536B model instead of1,056B. Six complete costs save49.93–137.23us, filtered/allactual raw/normalized intervals separate; live59.31us/1.19%raw/1.18%normalized. AllGC+480B/op; same8KiBadmitted model/state/frame allowance, physical/native peaks unmeasured, independent retainedframe531468B. Current3pass/3gaps/0uncertain: alpharesident/stream andvalidation pass/stop; live4920.12/native4789.06us upper1.032gap. Exact semanticprobabilities/headerorder/polls/cursor/pixels andterminalrefusal/cleanup remain. [Earlier preparation](VP8-PREPARED-COEFFICIENT-MODEL.md) and [alpha publication](WEBP-HORIZONTAL-ALPHA-PUBLICATION.md) retain history. [VP8L final publication](VP8L-FINAL-PUBLICATION.md) retains six unchanged lossless costs:2pass/4gaps,155.66us gain and19.25us palette regression. |
| JPEG decoding | [Completed output](JPEG-COMPLETED-OUTPUT.md) reuses existing overwrite storage after exact full-plane writes. Progressivegray saves98.02us/5.17%raw/5.11%normalized with separatedfiltered/allactual confidence; GC683176→151432B. Current1797.74/native1711.60us upper1.054 remainsgap. Twelve costs refresh/seventeen unchanged reuse:29now16pass/12gap/1uncertain. Ownedgray nowpasses602.47/native681.27us upper.891; stream420restart upper1.002 isuncertain, resolvebeforetuning. Fourgoverned peaks remain699834/756276/752177/721969B; result531468B until disposal. Default idlepixels0→531468B insideexisting2slot/4MiBpool, governedcalls bypass; no newowner/pool/cache. Gray idlecoefficients549120B unchanged. [Paired AC](JPEG-PAIRED-AC-WINDOW.md) retains RGB1530.34/native1450.52us upper1.065 and its574069Badmittedpeak. Liveprogressive444 remainsanindependent201.57usgap. |
| Bicubic columns | [Wide shared preparation](BICUBIC-WIDE-PREPARATION.md) saves24.28/94.49µs (4.81%/15.07%raw) on byte/word128×64 controls, unchanged113B/op and4KiB admission. Both remain gaps, uppers1.394/1.329; native clocks−0.65%/−3.85% disclosed. [Small word preparation](BICUBIC-WORD-COLUMN-PREPARATION.md) and [small byte preparation](BICUBIC-COLUMN-PREPARATION.md) reuse unchanged gains and gaps1.223/1.363. Four drivers share the same result-tail geometry helper; kernels and exact pixels remain. Other affected named H costs remain historical. |
| Bicubic streaming | [Shared streaming blocks](BICUBIC-SHARED-STREAMING.md) use two schedulers/four exact kernels within4KiB, saving66.62/51.17µs on byte/word8×17. Eight current controls: six pass/stop; RGBA8 8×17/4×9 retain gaps (uppers1.127/1.269). Allocations unchanged. |
| Lanczos3 | [Paired certificate preparation](LANCZOS-PAIRED-CERTIFICATE-PREPARATION.md) confirms175.99µs/4.20% elapsed gain on prepared word enlargement with counterbalanced immutable builds. Standalone raw/native clock movement is disclosed. All eight affected costs are current: five pass/stop; prepared byte/word reduction and word enlargement retain gaps (uppers1.158/1.097/1.052). Capacity is unchanged. Preserve exact Q30/rational/certified results and native float/prepared-state boundaries; [shared scheduling](LANCZOS-SHARED-ROW-DRIVER.md) remains linked history. |
| Complete bilinear enlargement | [Current boundary audit](bilinear-current-boundary-evidence.json) reuses the qualified immutable build without production changes or rebuilds. Existing RGBA8 Up8 measures2357.04µs/native2155.92µs, ratio1.093/upper1.134/lower.971: uncertain.102891B/op includes fresh~103KBhorizontal preparation and independent output ownership; the unchanged4MiBdefault pool reuses output backing. Older2914.62µs/2235512Bcost is superseded, not paired causal gain evidence. Native graph/context preparation remains retained and labeled. Stop tuning this uncertain case while larger demonstrated gaps remain. |
| Complete bicubic enlargement and small-support word publication | [Shared word preparation](BICUBIC-SHARED-WORD-PREPARATION.md) retains divisor/margin setup across pair/quartet/cached writers. [Direct identity cache](BICUBIC-IDENTITY-CACHE-ROWS.md) refreshes four cached word costs; eleven unchanged paths, including [identity geometry](BICUBIC-IDENTITY-WORD-GEOMETRY.md), are reused:15controls,9pass/5gaps/1uncertain. Full/bounded enlargement and default horizontal/vertical enlargement pass; stop them. Existing division, certified rounding/repair and rational pixels remain. DefaultRGBA8 [unchanged-build confirmation](bicubic-byte-enlargement-current-evidence.json) measures3,108.21/native3,037.83µs,upper1.026/116Bop,70.38µs gap; no causal historical gain. Current cached workspace reduction is stated separately below. [Publication pipeline](BICUBIC-WORD-PUBLICATION-PIPELINE.md) retains earlier scoped history. |
| Mixed-axis RGBA16 bicubic | [Shared bounded bands](BICUBIC-BOUNDED-BAND-EXECUTION.md) save159.23us/14.79%raw/15.40%normalized on defaultMixed16 and86.23us/8.03%raw/8.71%normalized at4KiB, with separated filtered/allactual raw/normalized confidence. Current917.38/native851.87us upper1.079 and987.47/native851.08us upper1.164 remain gaps; [Identity-axis geometry](BICUBIC-IDENTITY-WORD-GEOMETRY.md) now refreshes tightvertical:458.51/native589.46us upper0.779 passes/stop, saving271.81us/37.22%raw/36.98%normalized with separated intervals;113B/op unchanged. GC114/114/113Bop unchanged. One source loop has entry-selected constant capacities; sixteen-row scratch7596B/admitted8192B, bounded3992/4096B, retainedscratch0 and mixedoutput1059864B until disposal. Exact pixels, certificates, repair, terminal admission and bounded cancellation remain. The vertical result owns529416B; current fifteen-word inventory9pass/5gaps/1uncertain after four cached costs refresh; eleven other paths are reused. Initial dynamic-capacity regression is retained as history; the revised complete costs resolve it. |
| Cached bicubic preparation/execution | [Direct identity cache](BICUBIC-IDENTITY-CACHE-ROWS.md) removes the separate decoded row/copy, sharing exact conversion with entry-selected polling. Vertical byte636.49→530.97us saves105.52us/16.58%raw/16.50%normalized; word666.59→511.76us saves154.84us/23.23%raw/21.26%normalized. Both confidence views separate; uppers0.908/0.904 pass, superseding the earlier word regression. Nine refreshed controls:7pass/1gap/1uncertain. Byte reduction remains485.10/native416.46us gap; horizontal word~2us difference is uncertain. Workspace383852→367436B; totalGC367675/367595Bop, scratch0/result264708/529416B retained until disposal. Exact repair4KiB remains separately admitted; no new owner/pool/cache/API. Native graph/input/scratch remain prepared, managed one-shot setup timed. Four displayed rows refresh; passing cases stop. |
| Text and drawing | [Exact interior coverage](TEXT-INTERIOR-COVERAGE.md) reuses one exact area per fully covered run, preserving clipping at boundaries, pixel additions and cancellation. Current130.55/132.38us vs labeledImageSharp.Drawing45.72/45.83us, uppers4.456/4.174: both gaps. Contemporaneous filtered pointmeans5.94%/6.49% lower, allactual4.05%/5.80%; intervals overlap, CPUgain inconclusive. Allocation67,368B/draw,48,544BadmittedLatinworkspace, retainedscratch0 unchanged; physical/native peaks unmeasured. Eight-frame Latin/Arabic/wrapping/fitting correctness does not establish broaderCPUclaims. [Earlier clipping](TEXT-CLIPPING-WORKSPACE.md) and [shared sorting](TEXT-RASTER-SHARED-SORT.md) retain history. [Layout/fitting and RGBA16composition](TEXT-DRAWING-CURRENT.md) pass/stop. [Font loading](FONT-OWNED-STORAGE.md) remains31.86/nativeFreeType13.42us,upper2.396gap,176B/op and757076Bidleretention in the existing2slot/4MiBpool; governed loads bypass it. Independent codec/resize work continues. |

The latest [identity-cache stage](BICUBIC-IDENTITY-CACHE-ROWS.md) refreshes nine
affected costs and four displayed rows, preserving126comparisons/283requiredpaths.
Windows/Linux each pass5,938full/live,188internal/703public normal/scalar and
disabledAVX2/AVX512 modes, andthe SAMEactualReleasepackage/threeexamples. Six new
cached controls cover exact byte/word samples, odd/even tails, live polling,
admission/refusal/cancellation/recovery and overlapping exact repair. One adverse
first execution layout and two test-only storage setup repairs are retained;
the selected main/embedded build perOS is reused throughout. Exact packets/pixels
and independent ownership remain. Other families retain source-valid evidence.
This is scoped development qualification; fullparity and the full126row actual-
package release refresh remain open.

Earlier unselected [cached JPEG corrections](JPEG-CACHED-CORRECTION-EXPERIMENT.md),
[validated progression](JPEG-VALIDATED-PROGRESSION-EXPERIMENT.md),
[immutable JPEG geometry](JPEG-IMMUTABLE-MAP-EXPERIMENT.md),
[JPEG input preparation](JPEG-PREPARED-INPUT-EXPERIMENT.md),
[result row history](BICUBIC-RESULT-HISTORY-EXPERIMENT.md),
[packed prediction](VP8-PACKED-PREDICTION-EXPERIMENT.md),
[compact geometry](BICUBIC-COMPACT-GEOMETRY-EXPERIMENT.md),
[row/certificate preparation](BICUBIC-ROW-CERTIFICATE-EXPERIMENT.md),
[private parser publication](PNG-PRIVATE-PARSER-PUBLICATION-EXPERIMENT.md),
[fixed-half live signs](VP8-HALF-PROBABILITY-EXPERIMENT.md),
[VP8 windows](VP8-ROW-WINDOW-EXPERIMENT.md),
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
[Current JPEG attribution](JPEG-CURRENT-ENTROPY-ATTRIBUTION.md) refreshes the complete
RGB444 capture after paired AC changes, without builds or new costs. Earlier
whole-operation shares are historical. Live JLB546 attribution belongs to the
shared-window report; instrumented elapsed is not timing evidence.
[Current live progressive attribution](JPEG-LIVE-CURRENT-ATTRIBUTION.md) uses one
qualified JPR608 capture: buffered refinement dominates sampled locations, with
little nonzero-mask discovery. No production, displayed cost or scope change.
[Bicubic byte attribution](BICUBIC-BYTE-CURRENT-ATTRIBUTION.md) records current
source/JIT boundaries and a completed diagnostic capture with a failed post-run
I/O guard. Its locations motivate an overlapping-window model; they establish
no passing quiet measurement, phase time or current-cost change.

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

- LSF584 owns15PNGencoding costs after exact shared length classification.
  All complete packets remain exact; no parser/owner/checkpoint/field change.
  RNC579decode/header and othercodec/edit/text paths are source-unchanged.

- RNC579 owns43 ordinary/rolling PNG controls and the two-image shared-root case.
  Its syntax/routing audit retains six stored/counting/BCL providers plus PLK571's
  two separate materialized-live costs. No other codec/encoder/editing path changes.
  Historical stream/composition timings stay historical rather than current proof.

- VPS588 owns all six VP8 pixel/validation costs after model-row preparation:
  Native8, Vp8Stream, Opaque8, SimpleOpaque8, OpaqueCancelable8 and ValidateVp8Alpha.
  Model stride invalidates older WAF542/VPM524 costs. Their alpha publication,
  typed borrowing and successful-only private cursor behavior remain; every
  checkpoint, public failure and owner cleanup is preserved.
- LPF476 owns six unchanged VP8L reader costs after final inverse/publication;
  its former VP8 costs are superseded. Encoders, other codecs, resizing and text
  retain their own source-valid evidence.
- VHB517 owns nine lossy WebP encode costs after four-block histogram preparation;
  affected unmeasured H boundaries remain historical. Earlier profiles are leads,
  not current whole-execution CPU shares.
- RBC567 owns Mixed16/TightMixed16 after shared bounded-band execution;
  [identity geometry](BICUBIC-IDENTITY-WORD-GEOMETRY.md) owns TightVertical16.
  RPA503 retains eight unchanged word controls after shared preparation;
  ICR614 owns Down16/Horizontal16/Vertical16/Cross16 and five byte costs,
  superseding RHW611: both vertical cases and wider byte pass,
  horizontal word remains uncertain; source-valid byte controls
  remain attributed separately. Unrefreshed allocator/filter/pipeline H costs stay
  historical. Lanczos owns eight refreshed one-shot/prepared controls; exact
  certificates, repair, plans/owners and portable fallbacks remain required.
- [TAP604](TEXT-CURRENT-RASTER-ATTRIBUTION.md) retains one current drawing sample
  capture and strict-interior numerical model; no production or displayed cost
  changes. Actual C# exactness and complete-operation evidence are required next.
- TIR605 owns the two prepared drawing costs after exact interior coverage reuse.
  TCR587 retains the earlier clipping costs. TSC573's
  earlier allocation savings remain history; font/layout/fitting and compositing
  evidence are unaffected. Drawing CPU gains remain inconclusive.
- Other current owners and their precise source scopes are linked in the family
  table. Reuse confirmations only when relevant paths/boundaries remain unchanged.

Current priorities: equivalent preparation/reuse boundaries, substantial WebP
workspace lifetimes, shared resize execution and further codec work selected from
attribution. Keep passing cases stopped. Text/drawing proceeds independently;
pending references never block justified codec/resizing work. PLAN.md stays ignored.

All earlier reports, raw evidence, release packages and source-specific
invalidation notes are retained through the linked history. They do not establish
current blanket qualification or permanently close algorithm families.

[Fixed-alignment VP8 arithmetic](VP8-HIGH-ALIGNED-ARITHMETIC-EXPERIMENT.md)
retains an unselected implementation and current live profile. Its two short
complete-operation screens show no useful lead. Exact qualified sources and
binaries are restored; current costs and package qualification remain unchanged.
It adds no renderer dependency. Mixed-word bicubic attribution proceeds next.
