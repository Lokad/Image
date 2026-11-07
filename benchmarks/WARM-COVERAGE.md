# Material warm coverage

Current architectural phase, 2026-11-07. **283 material required paths** remain
in scope; the 125 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion needs current passing evidence for every named path, with scope stated.

## Current family status

| Required family/boundary | Current evidence and remaining work |
| --- | --- |
| Resident lossless WebP | [Isolated green prediction](WEBP-ISOLATED-GREEN-PREDICTION.md) preserves full-color visitor/fields and shares map coarsening/copy. Four refreshed lossless controls pass/stop; opaque/photo remain uncertain, uppers1.008/1.022. Final independent photo ratio.999646 has interval.991485–1.007836: no demonstrated gain/loss or equivalence. Previous cost owners and adverse stages remain linked history. |
| Resident VP8 encoding | [Per-block candidate preparation](VP8-SMALL-BLOCK-PREPARATION.md) reuses border/DC values and packed source across visited modes. Complete effort2 saves 54.30 µs/1.94% raw elapsed with both raw confidence views separated. Native moves −0.84%; normalized point improves 1.11%, but the all-actual interval overlaps neutral. Current 2,745.35/native 2,501.11 µs, upper 1.104 remains a gap; GC stays 406,705 B/op. Nine affected costs: 5 pass/3 gaps/1 uncertain. Effort6 passes the predeclared four-launch envelope, upper 0.993439; Alpha16 passes its current confirmation, Alpha8 remains uncertain. Diagram/dense admitted peaks stay 397,205/930,669 B, retained zero. [Compact residual programs](VP8-COEFFICIENT-PROGRAM.md) retain earlier storage gains and dense tradeoffs as history. No all-views relative-gain, other causal gain or generic launch-stability claim. Passing cases stop. |
| Other WebP encoding boundaries | [Current eight-control qualification](WEBP-FRESH-HISTOGRAM.md) preserves complete depth normalization and fresh awaited nonseekable async ownership. Passing cases stop. Other affected H costs remain historical; source/settings/full-packet/quality/resource gates stay unchanged. |
| PNG decoding/coding validation | [Current framing controls](PNG-CURRENT-FRAMING-COSTS.md):24 independent confirmations,20pass/4gaps. Gaps:liveRGBA8 upper1.113, tight/defaultRGBA16 uppers1.052/1.031, RGB16 upper1.062; allotherconfirmed cases stop. [Shared-root control](PNG-PREPARED-PREFIX.md) separately retains422µs/12.8%gain,upper1.014gap,1638214B/op and1766641Bcoarsepeak. Eight displayed controls5pass/3gaps; remaining costs and allocations in family report. Twenty-seven existing named decode cases still need current proof. Historical contradictions/raw retained; header-only operations/encoding unaffected. |
| PNG encoding | [Lazy view preparation](PNG-LAZY-VIEW-PREPARATION.md) independently saves390.08/375.08µs (1.83%/2.16%) on complete Optimal16/Smallest8. Exact packets and384KiB admission remain; Grayscale8 measures+408B/op to551,772, ten other fresh allocations unchanged. All11level6/9 fresh one-shot costs refresh; fourFastest costs reuse unchanged source bodies. Fifteen controls:9pass/stop,2gap,4uncertain. ByteFastest and byteGray remain gaps; wordSmallest and ordinary/live/forward wordFastest are uncertain. Resolve uncertainty before tuning. [Greedy preparation](PNG-GREEDY-VIEW-PREPARATION.md) retains independent269/836µs gains; earlier evidence stays linked history. |
| WebP image reading | [Successful-block publication](VP8-COMPLETED-BLOCK-PUBLICATION.md) saves82.55µs/1.60%raw and1.79%normalized on complete live opaque VP8; both confidence views separate. Current5072.31/native4798.30µs,upper1.065 remains gap; GC228864B/owned capacity unchanged. Only abandoned private failure-state publication removed; every poll/public cleanup and successful cursor remain. [VP8L final publication](VP8L-FINAL-PUBLICATION.md) owns nine unchanged costs:3pass/6gaps, including155.66µs gain and explicit19.25µs palette regression. Validation/literal pass and stop. Earlier output/validation reports retain source-valid storage and raw history. |
| JPEG decoding | [Progressive output ownership](JPEG-PROGRESSIVE-OUTPUT.md) independently saves161.72/183.00µs (4.46%/4.90%raw) on ordinary/live444, both raw/normalized confidence views separated. Current3466.85/3555.20µs versus native3355.46/3363.08µs, uppers1.037/1.066 retain gaps. GC701233→169584B/op. Governed peak1864806B and retained result531468B unchanged; default idle pixels+531468B, coefficients1697280B unchanged. Governed calls bypass both caches. Two displayed costs refresh; three distinct progressive readers/codingvalidation reuse unchanged paths: six total1pass/3gaps/2uncertain. [Reference lifetimes](JPEG-COEFFICIENT-REFERENCE-LIFETIME.md), [correction selection](JPEG-CORRECTION-BIT.md) and plane scheduling retain history. |
| Bicubic columns | [Wide shared preparation](BICUBIC-WIDE-PREPARATION.md) saves24.28/94.49µs (4.81%/15.07%raw) on byte/word128×64 controls, unchanged113B/op and4KiB admission. Both remain gaps, uppers1.394/1.329; native clocks−0.65%/−3.85% disclosed. [Small word preparation](BICUBIC-WORD-COLUMN-PREPARATION.md) and [small byte preparation](BICUBIC-COLUMN-PREPARATION.md) reuse unchanged gains and gaps1.223/1.363. Four drivers share the same result-tail geometry helper; kernels and exact pixels remain. Other affected named H costs remain historical. |
| Bicubic streaming | [Shared streaming blocks](BICUBIC-SHARED-STREAMING.md) use two schedulers/four exact kernels within4KiB, saving66.62/51.17µs on byte/word8×17. Eight current controls: six pass/stop; RGBA8 8×17/4×9 retain gaps (uppers1.127/1.269). Allocations unchanged. |
| Lanczos3 | [Paired certificate preparation](LANCZOS-PAIRED-CERTIFICATE-PREPARATION.md) confirms175.99µs/4.20% elapsed gain on prepared word enlargement with counterbalanced immutable builds. Standalone raw/native clock movement is disclosed. All eight affected costs are current: five pass/stop; prepared byte/word reduction and word enlargement retain gaps (uppers1.158/1.097/1.052). Capacity is unchanged. Preserve exact Q30/rational/certified results and native float/prepared-state boundaries; [shared scheduling](LANCZOS-SHARED-ROW-DRIVER.md) remains linked history. |
| Complete bilinear enlargement | [Current boundary audit](bilinear-current-boundary-evidence.json) reuses the qualified immutable build without production changes or rebuilds. Existing RGBA8 Up8 measures2357.04µs/native2155.92µs, ratio1.093/upper1.134/lower.971: uncertain.102891B/op includes fresh~103KBhorizontal preparation and independent output ownership; the unchanged4MiBdefault pool reuses output backing. Older2914.62µs/2235512Bcost is superseded, not paired causal gain evidence. Native graph/context preparation remains retained and labeled. Stop tuning this uncertain case while larger demonstrated gaps remain. |
| Complete bicubic enlargement and small-support word publication | [Word publication pipeline](BICUBIC-WORD-PUBLICATION-PIPELINE.md) independently saves130.43/135.29µs (4.11%/4.25%raw,4.40%/4.20%normalized) on complete bounded/default RGBA16 enlargement; both confidence views separate. Current3045.35/3048.24µs, uppers1.070/1.067 remain gaps. GC4264538→4264573B/op(+35 measured); required4264104B output and owned workspace unchanged. All15affected word costs current:1pass/stop(Down16),13gaps,1uncertain. Six word enlargement controls5gaps/1uncertain; source-valid byte costs give12enlargement controls7gaps/5uncertain. [Shared byte/word preparation](BICUBIC-SHARED-ENLARGEMENT.md) retains earlier workspace gains and byte uncertainty/adverse evidence as history. No broader causal or parity claim. |
| Text and drawing | All 14 additions below now have measured evidence. [Layout/fitting and approved references](TEXT-DRAWING-CURRENT.md) close the old reference decisions. Four layout/fitting controls and RGBA16 compositing pass and stop. [Active-edge scheduling](TEXT-ACTIVE-EDGE-SCHEDULING.md) saves179.16/170.49µs (48.55%/47.38%) on prepared drawing8/16, with exact frames. Allocation+8672B/draw to92384; admitted Latin workspace stays48544B, retained scratch zero. Current uppers5.230/5.226 remain gaps; eight-frame Arabic/wrapped/fitting generalization is correctness, not broader CPU proof. [Owned font preparation](FONT-LOAD-PREPARATION.md) saves14.84µs/4.80%raw against a contemporary original; current294.11/native13.95µs,upper21.246 remains gap. Owned capacity unchanged, allocation~758KB; no peak reduction inferred. Other current fill/copy/composite costs remain in the summary. |

The current [completed-block milestone](VP8-COMPLETED-BLOCK-PUBLICATION.md) qualifies
**5837 full/live checks per OS** (2572internal/2317public/948differential),
40internal/239public per normal/scalar mode, and the SAME actual private Release
package/all three examples on Windows/Linux. Public cancellation, ownership,
refusal/recovery, surviving outputs and actual async lifetimes pass. One immutable
core/embedded per stage perOS; two stages total, with no public/harness/selector/
native/fixture/diagnostic builds. The unselected fixed64 screen remains inconclusive;
arithmetic/tests restore before the accepted EH simplification. Package verification
captures current JIT with no extra timing/job: code3789->3195B, frame232B unchanged.
All125 numerical table rows reuse unchanged paths; current live cost is in summary
prose. Coarse memory numbers reuse source-valid ownership evidence; physical/native
peaks remain unmeasured. [VP8L final publication](VP8L-FINAL-PUBLICATION.md),
[Candidate preparation](VP8-SMALL-BLOCK-PREPARATION.md),
[Word publication](BICUBIC-WORD-PUBLICATION-PIPELINE.md),
[Progressive output](JPEG-PROGRESSIVE-OUTPUT.md),
[Overwrite VP8 output](VP8-OVERWRITE-OUTPUT.md), [residual programs](VP8-COEFFICIENT-PROGRAM.md), [validation elision](VP8L-VALIDATION-ELISION.md),
[owned fonts](FONT-LOAD-PREPARATION.md), [VP8 tokens](VP8-VALIDATION-MATERIALIZATION.md)
and [JPEG lifetimes](JPEG-COEFFICIENT-REFERENCE-LIFETIME.md) retain unchanged-path
evidence and raw history. The283-path objective remains open; this is not a full
actual-package performance refresh.

Superseded experiments, source-specific invalidation notes and raw archive identities
remain in linked family reports and the
[preceding coverage record](https://github.com/lokad/image/blob/65533a6c82743194dca90dcc4daae07955ea385a/benchmarks/WARM-COVERAGE.md).
In particular, [live-window prototypes](VP8-LIVE-WINDOW-PROTOTYPES.md),
[geometry study](BICUBIC-TAIL-GEOMETRY-EXPERIMENT.md),
[rate-core prototype](VP8-CANONICAL-RATE-BOUNDARY.md) and
[rate-epoch pricing](VP8-RATE-EPOCH-PRICING-EXPERIMENT.md) remain historical experiments.
They do not establish current costs, permanent exclusions or a mandate to replay.
The initial selected-mode effort5 boundary and its raw identities also remain in
that preceding record; its named required path is retained below.
Retain cited raw evidence; publication remains separate and postponed.

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

- CPT479 owns the live opaque VP8 cost after successful-only private publication;
  private failed cursor is unobserved/unresumed, every poll/public cleanup stays.
- LPF476 owns nine VP8L/ALPH reader costs after the private output-purpose/final
  publication change. Only eligible full RGBA paths fuse; all shared callers have
  current costs. Opaque VP8, encoders, other codecs, resizing and text are unchanged.
- PPS474 owns nine lossy WebP encode costs after shared prediction preparation;
  affected unmeasured H boundaries remain historical. Earlier profiles are leads,
  not current whole-execution CPU shares.
- WCP472 owns fifteen affected word bicubic controls; source-valid byte controls
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
