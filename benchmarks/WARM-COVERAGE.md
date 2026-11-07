# Material warm coverage

Current architectural phase, 2026-11-07. **283 material required paths** remain
in scope; the 125 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion needs current passing evidence for every named path, with scope stated.

[Current preparation/storage audit](ARCHITECTURAL-PREPARATION-AND-STORAGE.md)
reconciles one-shot/prepared boundaries and measures three WebP admission lifetimes.
The [matching-index handoff](WEBP-MATCHING-POPULATION-STORAGE.md) retains its eight
current encode costs. The latest [high-aligned JPEG stage](JPEG-HIGH-ALIGNED-READER.md)
refreshes 23 baseline costs and reuses six progressive confirmations. Historical
experiments remain linked separately, with raw evidence retained.

## Current family status

| Required family/boundary | Current evidence and remaining work |
| --- | --- |
| Resident lossless WebP | [Matching/population storage](WEBP-MATCHING-POPULATION-STORAGE.md) reuses the ordinary index after its final reader. Four lossless controls pass/stop; opaque/photo remain uncertain (uppers1.015/1.008). Direct photo/diagram allocation falls134736B each, admitted peaks3470059/3251328B unchanged; retained scratch zero. Exact packets and source settings remain. No causal CPU gain inferred from refreshed costs. [Isolated prediction](WEBP-ISOLATED-GREEN-PREDICTION.md) retains earlier numerical/CPU evidence and adverse history. |
| Resident VP8 encoding | [Per-block candidate preparation](VP8-SMALL-BLOCK-PREPARATION.md) reuses border/DC values and packed source across visited modes. Complete effort2 saves 54.30 µs/1.94% raw elapsed with both raw confidence views separated. Native moves −0.84%; normalized point improves 1.11%, but the all-actual interval overlaps neutral. Current 2,745.35/native 2,501.11 µs, upper 1.104 remains a gap; GC stays 406,705 B/op. Nine affected costs: 5 pass/3 gaps/1 uncertain. Effort6 passes the predeclared four-launch envelope, upper 0.993439; Alpha16 passes its current confirmation, Alpha8 remains uncertain. Diagram/dense admitted peaks stay 397,205/930,669 B, retained zero. [Compact residual programs](VP8-COEFFICIENT-PROGRAM.md) retain earlier storage gains and dense tradeoffs as history. No all-views relative-gain, other causal gain or generic launch-stability claim. Passing cases stop. |
| Other WebP encoding boundaries | [Current eight-control storage stage](WEBP-MATCHING-POPULATION-STORAGE.md):5pass/3uncertain across lossless/alpha. Full-depth normalization and fresh awaited nonseekable async ownership remain timed. Transparent alpha allocation falls70560B; admitted peak3656309B/retained0 unchanged. Alpha16 remains pass, Alpha8 uncertain. Native contexts retain their declared preparation boundary. Passing cases stop; photo/opaque/Alpha8 uncertainty is not a demonstrated optimization gap. Older fresh-histogram costs remain linked history. |
| PNG decoding/coding validation | [Current resident controls](PNG-CURRENT-FRAMING-COSTS.md): all47requiredWarmPngdecode+4codingvalidation cases now have independent evidence;45pass/stop,6gaps,none missing or uncertain. The final25controls all pass; prior liveword2 remain gaps. Largest gaps are liveRGBA8(~145µs), tightRGBA16(~124µs) andRGB16(~115µs); ordinary/live/tight word controls retain their exact bounds. No tuning of passing packed/indexed/tRNS/Paeth/Adam7/stored paths. [Shared-root control](PNG-PREPARED-PREFIX.md) separately remainsgap; stream/composition/inspection boundaries keep their own finite coverage. No production/build/package changes or historical causal-gain claim. |
| PNG encoding | [Lazy view preparation](PNG-LAZY-VIEW-PREPARATION.md) independently saves390.08/375.08µs (1.83%/2.16%) on complete Optimal16/Smallest8. Exact packets and384KiB admission remain; Grayscale8 measures+408B/op to551,772, ten other fresh allocations unchanged. All11level6/9 fresh one-shot costs refresh; fourFastest costs reuse unchanged source bodies. Fifteen controls:9pass/stop,2gap,4uncertain. ByteFastest and byteGray remain gaps; wordSmallest and ordinary/live/forward wordFastest are uncertain. Resolve uncertainty before tuning. [Greedy preparation](PNG-GREEDY-VIEW-PREPARATION.md) retains independent269/836µs gains; earlier evidence stays linked history. |
| WebP image reading | [Direct live coefficient classes](VP8-DIRECT-COEFFICIENT-CLASSES.md) save74.05µs/1.46%raw/1.55%normalized on complete liveopaqueVP8; both confidence views separate. Current5011.41/native4793.54µs,upper1.055 remains gap; GC228864B and owned capacity unchanged. Numeric token dispatch retires; real pixel/validation kernels witness all12symbols, preserving every poll, arithmetic/read order/cursor and public cleanup. [Successful publication](VP8-COMPLETED-BLOCK-PUBLICATION.md) retains earlier gains/profile and adverse studies as history. [VP8L final publication](VP8L-FINAL-PUBLICATION.md) owns nine unchanged costs:3pass/6gaps, including155.66µs gain and explicit19.25µs palette regression. Validation/literal pass and stop. |
| JPEG decoding | [High-aligned local entropy readers](JPEG-HIGH-ALIGNED-READER.md) save 37.62µs/2.13% raw on full-grid RGB and 82.26µs/4.01% on live-token decode; raw/normalized views separate. Allocation stays 10,088B/op; exact cursor/pixels/checkpoints and packets remain. All 29 costs current: 23 refresh/6 progressive confirmations reused; 14 pass/14 gaps/1 uncertain. Baseline420 now passes and stops; caller CMYK upper1.001 is uncertain. RGB/live retain ~273/254µs gaps. Sparse CMYK's initial contradictory normalized loss and reversed-order overlapping normalized evidence remain recorded, without a causal normalized gain/loss claim. Existing admitted capacity, retained output and default pool bounds are unchanged. No blanket JPEG gain or numerical/metadata relaxation. |
| Bicubic columns | [Wide shared preparation](BICUBIC-WIDE-PREPARATION.md) saves24.28/94.49µs (4.81%/15.07%raw) on byte/word128×64 controls, unchanged113B/op and4KiB admission. Both remain gaps, uppers1.394/1.329; native clocks−0.65%/−3.85% disclosed. [Small word preparation](BICUBIC-WORD-COLUMN-PREPARATION.md) and [small byte preparation](BICUBIC-COLUMN-PREPARATION.md) reuse unchanged gains and gaps1.223/1.363. Four drivers share the same result-tail geometry helper; kernels and exact pixels remain. Other affected named H costs remain historical. |
| Bicubic streaming | [Shared streaming blocks](BICUBIC-SHARED-STREAMING.md) use two schedulers/four exact kernels within4KiB, saving66.62/51.17µs on byte/word8×17. Eight current controls: six pass/stop; RGBA8 8×17/4×9 retain gaps (uppers1.127/1.269). Allocations unchanged. |
| Lanczos3 | [Paired certificate preparation](LANCZOS-PAIRED-CERTIFICATE-PREPARATION.md) confirms175.99µs/4.20% elapsed gain on prepared word enlargement with counterbalanced immutable builds. Standalone raw/native clock movement is disclosed. All eight affected costs are current: five pass/stop; prepared byte/word reduction and word enlargement retain gaps (uppers1.158/1.097/1.052). Capacity is unchanged. Preserve exact Q30/rational/certified results and native float/prepared-state boundaries; [shared scheduling](LANCZOS-SHARED-ROW-DRIVER.md) remains linked history. |
| Complete bilinear enlargement | [Current boundary audit](bilinear-current-boundary-evidence.json) reuses the qualified immutable build without production changes or rebuilds. Existing RGBA8 Up8 measures2357.04µs/native2155.92µs, ratio1.093/upper1.134/lower.971: uncertain.102891B/op includes fresh~103KBhorizontal preparation and independent output ownership; the unchanged4MiBdefault pool reuses output backing. Older2914.62µs/2235512Bcost is superseded, not paired causal gain evidence. Native graph/context preparation remains retained and labeled. Stop tuning this uncertain case while larger demonstrated gaps remain. |
| Complete bicubic enlargement and small-support word publication | [Shared word preparation](BICUBIC-SHARED-WORD-PREPARATION.md) removes repeated divisor/margin setup across pair/quartet/cached writers. All15word costs current:6pass/9gaps. Full/bounded enlargement and default horizontal/vertical enlargement pass; stop them. Existing division, certified rounding/repair and rational pixels remain. DefaultRGBA8 [unchanged-build confirmation](bicubic-byte-enlargement-current-evidence.json) resolves old uncertainty:3,108.21/native3,037.83µs,upper1.026/116Bop,70.38µs gap; no causal historical gain. Larger codec gaps take priority. No owned capacity/retention change. [Publication pipeline](BICUBIC-WORD-PUBLICATION-PIPELINE.md) retains earlier scoped gains/history. |
| Mixed-axis RGBA16 bicubic | [Shared preparation](BICUBIC-SHARED-WORD-PREPARATION.md) independently saves41.73us/3.94%raw (2.85%normalized) on Mixed16 and83.80us/8.86%raw (8.94%normalized) on Cross16; both confidence views separate. TightVertical16 regresses11.96us/1.66%raw/1.82%normalized, also separated. Current default/bounded mixed uppers1.217/1.204; tight vertical1.248; all remain gaps. Alloc114/114/113B unchanged; cross measures−1B without a storage claim. Arithmetic/ownership/cancellation contracts unchanged. [Four-row scheduling](BICUBIC-MIXED-WORD-SCHEDULING.md) remains linked history. |
| Text and drawing | All 14 additions below now have measured evidence. [Layout/fitting and approved references](TEXT-DRAWING-CURRENT.md) close the old reference decisions. Four layout/fitting controls and RGBA16 compositing pass and stop. [Active-edge scheduling](TEXT-ACTIVE-EDGE-SCHEDULING.md) saves179.16/170.49µs (48.55%/47.38%) on prepared drawing8/16, with exact frames. Allocation+8672B/draw to92384; admitted Latin workspace stays48544B, retained scratch zero. Current uppers5.230/5.226 remain gaps; eight-frame Arabic/wrapped/fitting generalization is correctness, not broader CPU proof. [Owned font storage](FONT-OWNED-STORAGE.md) saves258.73µs/89.04%raw against a contemporary original; current31.86/native13.42µs,upper2.396 remains gap. GC757597→176B/op; the isolated load retains757076B idle within the existing2slot/4MiBdefault cap. Rooted loads bypass the cache and retain exact charges; fullcopy/eager parsing stay timed. No physical-peak/general-parser/governedCPUclaim. Other current fill/copy/composite costs remain in the summary. |

The latest [live VP8 coefficient stage](VP8-DIRECT-COEFFICIENT-CLASSES.md) refreshes
one live complete cost in prose; all 125 numeric rows reuse unchanged paths.
Windows/Linux each pass 5,867 full/live checks plus929 internal/239 public per
normal/scalar mode and the SAME actual Release package/all three examples.
Exact pixels, refusal/cancellation cleanup and surviving owners remain qualified.
JPEG, word/WebP lossless/font/PNG/other resize and ISA evidence is reused.
This is not a 125-row actual-package release refresh or completion of all 283 paths.

Earlier [JPEG publication](JPEG-PLAIN-READER.md),
[word publication](BICUBIC-WORD-PUBLICATION-PIPELINE.md) and the unselected
[JPEG row-pair experiment](JPEG-ROW-PAIR-EXPERIMENT.md) retain their source-specific
comparisons, adverse results and raw evidence separately.

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
