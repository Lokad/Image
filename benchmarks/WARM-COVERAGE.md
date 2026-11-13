# Material warm coverage

Current architectural phase, 2026-11-13. **289 material required paths** remain
in scope; the 138 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion requires valid passing evidence for every named path. Missing evidence,
gaps and uncertainty stay open. No image, shape, depth, filter or quality grid is added.

[Released-image storage](RELEASED-IMAGE-STORAGE.md), with two refreshed
[paired Lanczos output](LANCZOS-PAIRED-OUTPUT.md) and three
[packed mixed-word cache](MIXED-WORD-PACKED-CACHE.md) costs, owns the current **191
comparisons: 163 pass, 12 gaps, 16 uncertain**. These are existing views, including
approved alternative boundaries, not 191 new material paths. The unchanged encoder,
caller-sample, inspection and in-place drawing paths retain their identified evidence.
Each OS passes **6,551 full/live checks**, focused normal/scalar ownership checks and
the **same actual Release-package consumer / three examples**. Source-valid ISA
proof is reused. Current scope and raw evidence are in the linked owner, rather
than repeated experimental or qualification narratives here.

The complete word composition independently saves **317.42 µs / 9.74%** and
**1,063,078 allocated B/op**; parity remains uncertain. The bounded byte wide
control records **1.59 µs / 0.446% raw adversity**, with inconclusive normalization;
both contemporary builds miss parity. Preserve that tradeoff and all raw captures.
The default store remains two arrays / 4 MiB; larger idle retention, allocation,
logical admitted capacity and unmeasured physical peaks remain distinct.

## Current family status

| Required family/boundary | Current evidence and next action |
| --- | --- |
| PNG decoding | [Current owner](RELEASED-IMAGE-STORAGE.md): all 47 existing decode controls pass. Four coding-only validators and the caller-root retained-output control reuse unchanged paths. Stop passes. |
| Stream decode/composition | Five PNG views: four pass, word composition uncertain. JPEG/VP8L/VP8 stream decode passes. [Direct acquisition](DIRECT-STREAM-INPUT.md) remains the source-valid inspection/async-validation owner; its old affected decode/composition costs are historical. |
| JPEG reading | Current 24 RGBA/fresh-reference comparisons: 23 pass, retained horizontal-CMYK gap. The contemporary old build has the same retained-reference gap; the equivalent fresh view passes. Six caller-sample/validation paths reuse unchanged evidence. [Kernel history](JPEG-SHARED-BLOCK-SUPPORT.md). |
| JPEG encoding | [Gray preparation](JPEG-GRAY-TILE-PREPARATION.md), [prepared replay](JPEG-PREPARED-HUFFMAN-BANK.md) and identified color/sampling evidence are unchanged. Keep SIMD libjpeg-turbo grayscale explicitly labeled and packets exact. |
| PNG encoding | [Compressed heap entries](PNG-HUFFMAN-HEAP-ENTRIES.md) retain 16 passing fresh controls and one Gray8 uncertainty; exact packets and 600 B/op reduction remain scoped. [Stored/fresh/natural64 views](PNG-STORED-WORD-NORMALIZATION.md) keep their distinct lifetimes; retained word is uncertain. No speed tuning based on uncertainty alone. |
| Lossless WebP/alpha encoding | [Huffman workspace](WEBP-HUFFMAN-WORKSPACE.md), [photograph confidence](WEBP-LOSSLESS-PHOTOGRAPH-CONFIDENCE.md) and [adjacent alpha views](WARM-ADJACENT-CONFIDENCE.md) remain source-valid: six pass, opaque/word-alpha uncertain. Exact packets, selected-candidate ownership and operation-owned workspace reuse remain. |
| Lossy WebP encoding | [Rounded-border prediction](VP8-ROUNDED-BORDER-PREDICTION.md) retains six passes / three uncertainties on the existing content/effort scopes. Exact packets and tie order remain. |
| WebP reading | Current 12 views: nine pass, three opaque VP8 uncertainties. Full output, native bounds, cancellation and ownership gates pass. [Earlier color-tile work](WEBP-INVERSE-COLOR-TILES.md) remains history. |
| Bicubic/bilinear resizing | Current 73 views: 67 pass, one bounded byte-wide gap, five uncertainties. [Packed mixed-word cache](MIXED-WORD-PACKED-CACHE.md) passes bounded mixed-word: 820.00/native846.10 µs, upper0.978, 113 B/op; 3.38% normalized gain is confirmed. Tight vertical also improves; default mixed-word remains uncertain. The earlier vertical regression is retained. All eight equivalent one-shot/prepared controls pass. Stop passes; investigate demonstrated gaps before tuning uncertainties. Native float versus managed exact arithmetic stays explicit. |
| Lanczos3 | [Paired outputs](LANCZOS-PAIRED-OUTPUT.md) pass and stop prepared byte reduction: 462.72/zimg489.40 µs, upper0.950; 138 B/op, 139,800 B retained plan. Eight views: six pass, one prepared-word gap, one word-enlargement uncertainty. The new sum borrows existing output capacity; exact arithmetic and equivalent one-shot/prepared ownership remain. |
| Copy/turn transforms and drawing | Current 20 independent-result edits: five pass, nine gaps, six uncertainties. Most gaps are a few microseconds; retain them without displacing larger absolute work. In-place fill/copy/compositing reuse unchanged evidence and approved reference identities. |
| Font/text public integration | Both complete one-shot and preparation-plus-eight-draw comparisons pass and stop. Report preparation separately and amortize the measured complete batch. Stage loading/drawing costs remain diagnostics with validation/retained-state differences, not evidence of overall slowdown. Preserve exact layout/fitting/full-word and visual gates; no face/script grid expansion. |
| Inspection | [Direct-input](DIRECT-STREAM-INPUT.md) and named prefix/header adapters remain source-valid. Seekable complete PNG Identify has a small gap; async passes. Metadata acquisition, container validation, coding validation and pixel decoding remain distinct. |

Preparation/reuse boundaries are described in
[architectural attribution](ARCHITECTURAL-PREPARATION-AND-STORAGE.md).
[Prior coverage](https://github.com/lokad/image/blob/31988ad292e291faa62f156498b0e8c58be9a698/benchmarks/WARM-COVERAGE.md)
retains superseded costs, experiments and qualification links. Those are history,
not current timing proof or permanent algorithm-family restrictions. The required
inventory below is retained unchanged; current family costs above supersede its
older illustrative timings.

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
admission. The remaining **253** base controls plus the **30** previously named additions
and **four** prepared-bicubic public boundaries below give **287**, without adding a shape/depth/filter/quality grid. A later case
requires evidence of a distinct required algorithm or public boundary omitted
from this finite set. Differences in default option values are accepted.

Two wide public boundaries use the existing original513×259 byte/word rasters
and128×64 outputs: `BicubicResizePlan.Resize` for RGBA8 and RGBA16. Both retain
admitted preparation explicitly while reading fresh input and returning independent
results on every call. No new image, shape or settings grid. Existing Wide8/16
one-shot operations now also have equivalent fresh-zimg views; their former
fresh-managed/prepared-native observations remain separately visible diagnostic
comparisons. Setup and output ownership are included on equivalent sides.

Two additional prepared narrow boundaries, `BicubicPreparedNarrow8` and
`BicubicPreparedNarrow16`, use existing Down8/16 rasters and256×129 outputs.
Their byte/word row certificates and execution differ from the wide prepared
kernels. [Equivalent narrow comparisons](BICUBIC-NARROW-BOUNDARIES.md) qualify
both complete one-shot controls and prepared word. The latest
[shared-schedule owner](CACHED-BICUBIC-SHARED-HORIZONTAL-SCHEDULE.md) refreshes all
four boundaries: all pass, including prepared byte436.53/native455.85µs upper0.968.
Plans retain199840B. Two new prepared paths extend285→287; fresh-native
views reuse existing one-shot operations. No new source image, filter or settings grid.

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
| PreparedText8 | TextRenderPlan.Draw, RGBA8 | Distinct caller-owned copied glyph geometry/events/workspace, fixed shape/origin; reference positions public paths once and performs complete identity Fill. |
| PreparedText16 | TextRenderPlan.Draw, RGBA16 | Distinct exact full16-bit coverage/composition with explicit retained preparation; full samples remain in the approved visual reference gates. |

## Evidence and execution rules

Use original frozen controls, reference settings, exact encoder packets and managed
pixels, approved native/visual bounds, complete ownership boundaries and the
dedicated quiet VM. Run sequentially on one CPU with the versioned idle/process
guard; historical own load is not a reason to wait. Preserve every actual sample
and adverse result. Do not retime solely for favorable numbers or infer overall
cost by adding historical stages.

Resolve uncertain measurements before assuming an optimization is needed. Stop
confirmed passes unless their production paths change. Architectural transitions
may span coherent stages; judge completed stages by absolute time, allocation,
retention and simplification without fixed percentage thresholds. A pending family
does not block independent codec, resizing or ownership work.

Reuse verified immutable builds and unchanged evidence. Use focused checks during
development; finish affected performance reconciliation before one coherent full
Windows/Linux, scalar/affected-ISA and actual Release-package milestone. Preserve
the three named resize-storage suites. Report allocation, admitted peak and retained
capacity separately. Publication is separate and postponed.

Keep the current summary at three tables or fewer and verify it with
`python benchmarks/render-benchmark.py --check`. Current owners retain reproduction,
source/reference identities and mirrored member-verified raw evidence. Historical
reports and published release artifacts remain preserved, without an arbitrary
checkout-size cap.
