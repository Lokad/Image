# Material warm coverage

Current architectural phase, 2026-11-13. **289 material required paths** remain
in scope; the 141 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion requires valid passing evidence for every named path. Missing evidence,
gaps and uncertainty stay open. No image, shape, depth, filter or quality grid is added.

[Released-image storage](RELEASED-IMAGE-STORAGE.md), with five refreshed
[Lanczos word-product reuse](LANCZOS-WORD-PRODUCT-REUSE.md) and three
[packed mixed-word cache](MIXED-WORD-PACKED-CACHE.md) costs, plus eight current
[shared axis-swapping edits](SHARED-AXIS-SWAP.md) and one
[bounded horizontal quartet](BOUNDED-WIDE-HORIZONTAL-QUARTET.md), plus six
[shared-copy/composition costs](SHARED-RASTER-COPY.md) and two refreshed
[horizontal-CMYK costs](CMYK-CURRENT-OWNED-CONFIDENCE.md) and its sparse-2D update, owns the current **191
comparisons: 171 pass, 5 gaps, 15 uncertain**. These are existing views, including
approved alternative boundaries, not 191 new material paths. The unchanged encoder,
caller-sample, inspection and in-place drawing paths retain their identified evidence.
Each OS passes **6,600 full/live checks**, focused normal/scalar ownership checks and
the **same actual Release-package consumer / three examples**. Source-valid ISA
proof is reused. Current scope and raw evidence are in the linked owner, rather
than repeated experimental or qualification narratives here.

The released-storage stage's **317.42 µs / 9.74%** word-composition gain and
**1,063,078 allocated B/op** reduction remain history. Its refreshed complete cost
is 3,168.24 / FFmpeg 3,161.97 µs, upper 1.012, still uncertain. The byte composition
passes at 1,369.97 / 1,619.91 µs, upper .857. No pipeline gain is inferred from
historical stage sums. The bounded byte-wide control's earlier adverse captures
remain history; its current cost is uncertain.
The default store remains two arrays / 4 MiB; larger idle retention, allocation,
logical admitted capacity and unmeasured physical peaks remain distinct.

## Current family status

| Required family/boundary | Current evidence and next action |
| --- | --- |
| PNG decoding | [Current owner](RELEASED-IMAGE-STORAGE.md): all 47 existing decode controls pass. Four coding-only validators and the caller-root retained-output control reuse unchanged paths. Stop passes. |
| Stream decode/composition | Five PNG views: four pass, word composition uncertain. JPEG/VP8L/VP8 stream decode passes. [Direct acquisition](DIRECT-STREAM-INPUT.md) remains the source-valid inspection/async-validation owner; its old affected decode/composition costs are historical. |
| JPEG reading | Current24RGBA/fresh views:22pass, retained horizontal-CMYK gap and sparse-2D uncertainty. [Current accepted costs](CMYK-CURRENT-OWNED-CONFIDENCE.md) retain434.53/FFmpeg392.72µs upper1.119; equivalent fresh contexts pass399.32/748.79µs upper.562. Contemporary unchanged sparse-2D449.95/461.96µs upper1.001 replaces its older pass. [Compact output](JPEG-COMPACT-OUTPUT-STUDY.md) gives4.44–6.24% scoped horizontal gains but material sparse-2D regressions; both implementations are rejected and production/package restored exactly. Preserve attribution before another fusion redesign; continue independent work. Six caller/validation paths reuse unchanged evidence. |
| JPEG encoding | [Current caller-RGB420 adjacency](JPEG-CALLER-CURRENT-CONFIDENCE.md) stays uncertain:1712.07/FFmpeg1650.90µs, ratio interval.991–1.057,388220B/op. Exact52091/52093B packets retain the strict2B metadata exception. No presumed gap or unchanged favorable repeat. [Gray preparation](JPEG-GRAY-TILE-PREPARATION.md) and other [Huffman replay](JPEG-PREPARED-HUFFMAN-BANK.md) costs reuse unchanged evidence; SIMD libjpeg-turbo grayscale stays explicitly labeled. |
| PNG encoding | [Current Gray8 adjacency](PNG-GRAY-CURRENT-PATH.md) remains uncertain, ratio .983–1.006, after the heap-entry path change; no favorable retry or presumed optimization gap. [Compressed heap entries](PNG-HUFFMAN-HEAP-ENTRIES.md) retain 16 passing fresh controls, exact packets and scoped 600 B/op reduction. [Stored/fresh/natural64 views](PNG-STORED-WORD-NORMALIZATION.md) keep their distinct lifetimes; retained word is uncertain. |
| Lossless WebP/alpha encoding | [Huffman workspace](WEBP-HUFFMAN-WORKSPACE.md), [photograph confidence](WEBP-LOSSLESS-PHOTOGRAPH-CONFIDENCE.md) and [adjacent alpha views](WARM-ADJACENT-CONFIDENCE.md) remain source-valid: six pass, opaque/word-alpha uncertain. Exact packets, selected-candidate ownership and operation-owned workspace reuse remain. |
| Lossy WebP encoding | [Current opaque adjacency](VP8-ENCODE-CURRENT-CONFIDENCE.md) remains uncertain: photograph .991–1.007, effort2diagram .980–1.001. No presumed gap or favorable retry. [Rounded-border prediction](VP8-ROUNDED-BORDER-PREDICTION.md) and [alpha scopes](WARM-ADJACENT-CONFIDENCE.md) retain six passes / three uncertainties across nine controls. Exact packets and tie order remain. |
| WebP reading | Current 12 views: nine pass, three opaque VP8 uncertainties. Full output, native bounds, cancellation and ownership gates pass. [Earlier color-tile work](WEBP-INVERSE-COLOR-TILES.md) remains history. |
| Bicubic/bilinear resizing | Current 73 views: 67 pass, six uncertainties. [Bounded horizontal reuse](BOUNDED-WIDE-HORIZONTAL-QUARTET.md) saves 3.29% in paired complete byte-wide resize; standalone360.12/zimg348.02µs, upper1.041 remains uncertain,113B/op. Five unaffected specializations compile identically; reuse their valid costs. [Packed mixed-word cache](MIXED-WORD-PACKED-CACHE.md) passes bounded mixed-word and tight vertical; default mixed-word remains uncertain. Earlier adverse captures are retained. All eight equivalent one-shot/prepared controls pass and stop. Native float versus managed exact arithmetic stays explicit. |
| Lanczos3 | [Word product reuse](LANCZOS-WORD-PRODUCT-REUSE.md) passes and stops all four affected byte/word reduction views. Prepared word: 485.36/zimg512.07 µs, upper0.958; 6.18% normalized gain, 138 B/op, 156,248 B retained plan. Eight views: seven pass, one word-enlargement uncertainty; its unchanged-path refresh remains open at upper1.035. Product/sum loans reuse admitted storage; exact arithmetic and equivalent one-shot/prepared ownership remain. |
| Copy/turn transforms and drawing | [Shared axis-swapping tiles](SHARED-AXIS-SWAP.md) pass/stop all eight byte/word transposes and quarter turns. Mirrored byte transpose: 32.07/FFmpeg39.50 µs, upper .826; confirmed 10.00 µs / 23.76% raw / 21.24% normalized gain, unchanged 112 B/op and capacity. Three duplicate classes are removed. Current 20 independent-result edits: 13 pass, four gaps, three uncertainties. [Shared raster copy](SHARED-RASTER-COPY.md) retains its word-clone gain and passing byte copies. Other dispatcher branches compile identically; in-place drawing/reference evidence is reused. Keep small gaps visible without displacing material work. |
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
