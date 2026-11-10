# Material warm coverage

Current architectural phase, 2026-11-10. **283 material required paths** remain
in scope; the 127 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion needs source-valid passing evidence for every named path, with scope
stated. Missing/uncertain evidence and affected historical H rows remain open.

The latest accepted [Lanczos word-row stage](LANCZOS-PAIRED-WORD-ROWS.md) passes
6,197 full/live checks per OS, affected normal/scalar/ISA and the same actual
Release-package consumer/all three examples on Windows/Linux. This qualifies
production correctness and ownership; it is not a whole-release timing refresh.
Unchanged performance/qualification evidence is reused only within its source scope.

## Current family status

| Required family/boundary | Current owner and remaining work |
| --- | --- |
| JPEG decoding and coding validation | [Batched two-row columns](JPEG-BATCHED-TWO-ROW-COLUMNS.md): 29 reads, 22 pass / 2 gaps / 5 uncertain. Horizontal CMYK and caller CMYK remain gaps; RGB444/422/420, sparse CMYK and progressive live444 remain uncertain. Sparse saves26.08µs/5.38% raw; no blanket RGB gain. |
| JPEG encoding | [Gray tile preparation](JPEG-GRAY-TILE-PREPARATION.md) and [prepared replay](JPEG-PREPARED-HUFFMAN-BANK.md) retain current grayscale/caller and unchanged color evidence. Current default/sampling scopes, exact packets and explicitly labeled SIMD libjpeg-turbo grayscale boundaries remain distinct. |
| PNG reading | [Current framing costs](PNG-CURRENT-FRAMING-COSTS.md): 51 controls, 49 pass / 1 gap / 1 uncertain. Materialized live RGBA16 upper1.012 is a gap; live byte upper1.004 is uncertain. [Current word attribution](PNG-LIVE-WORD-CURRENT-ATTRIBUTION.md) identifies the literal-run loop as the next bounded lead, with no new timing claim. Other affected historical stream/composition costs remain H. |
| PNG encoding | [Length classification](PNG-LENGTH-CLASSIFICATION.md): 15 complete fresh-state controls, 13 pass / 2 uncertain. ByteFastest/Gray8 need confidence resolution; the [bounded Fastest check](PNG-FASTEST-CURRENT-CONFIDENCE.md) remains uncertain. Exact complete packets are required. |
| WebP lossless/alpha encoding | [Huffman workspace](WEBP-HUFFMAN-WORKSPACE.md): 8 controls, 4 pass / 4 uncertain; photograph allocation about5.3 MB supersedes20–30 MB. Selected candidates remain independently owned. Prior matching/clustering/counting gains are retained, not work to replay. |
| WebP lossy encoding | [Rounded-border prediction](VP8-ROUNDED-BORDER-PREDICTION.md), unchanged [histograms](VP8-FOUR-BLOCK-HISTOGRAM.md) and alpha storage: 5 pass / 4 uncertain. Exact packets, ordered ties and current effort/content boundaries remain; uncertainty alone is not a tuning need. |
| WebP reading | [Output-owned palettes](WEBP-PALETTE-OUTPUT.md): lossless6 controls, 3 pass / 3 uncertain. VP8 combined6 controls: 2 pass / 3 gaps / 1 uncertain. Live opaque upper1.028 remains a gap; [window experiments](VP8-LIVE-MACROBLOCK-WINDOW-EXPERIMENT.md) establish no useful gain and restore production. |
| Bicubic enlargement/shared byte publication | [Shared quartet execution](BICUBIC-SHARED-QUARTET-EXECUTION.md): 15 affected enlargement/mixed controls, 9 pass / 2 gaps / 4 uncertain. Complete bounded byte enlargement saves159.58µs/5.10% raw/5.12% normalized, upper.981 passes; same116B/op/4KiB. Horizontal byte and default mixed byte remain gaps. Word horizontal and tight mixed uncertainty remain explicit. |
| Mixed-axis RGBA16 bicubic | [Result-backed bands](BICUBIC-RESULT-BACKED-BANDS.md) retain unchanged word paths: default/vertical pass; bounded mixed893.16/zimg851.29µs upper1.056 is a gap. Byte mixed costs are owned by the quartet stage above. |
| Bounded packed bicubic | [Four-pixel preparation](PACKED-BICUBIC-FOUR-PIXEL-PREPARATION.md): byte405.59/zimg398.21µs upper1.028 and word422.81/397.48µs upper1.098 remain gaps. Same113B/op/4KiB; earlier indexed clocks are history. |
| Bounded narrow bicubic | [Result-row loan](NARROW-BICUBIC-ROW-LOAN.md): byte472.44/zimg417.11µs upper1.165 remains a gap; word upper.971 passes/stops. Same113B/op/4KiB, no added owned capacity. |
| Bounded wide bicubic | [Result-row loan](BICUBIC-RESULT-ROW-LOAN.md): byte406.16/zimg367.01µs upper1.119 remains a gap; word upper1.003 is uncertain. No substitution of an older pass over a changed driver. |
| Cached wider/other bicubic | [Certified publication](CACHED-WIDE-CERTIFIED-PUBLICATION.md): 8 controls, 7 pass / 1 gap. Wide byte395.96/zimg345.54µs upper1.206 remains a gap. [Identity caches](BICUBIC-IDENTITY-CACHE-ROWS.md), [overlap](BICUBIC-WIDE-OVERLAP.md) and unchanged [word preparation](BICUBIC-SHARED-WORD-PREPARATION.md) own other named paths. |
| Lanczos3 | [Paired word rows](LANCZOS-PAIRED-WORD-ROWS.md): 8 refreshed controls, 6 pass / 1 prepared-byte gap (upper1.039) / 1 prepared-word uncertainty (upper1.012). The paired study saves41.10/41.15µs prepared/one-shot; final costs and clock reconciliation are separate. Word plan retains156248 B, up8208 B; same exact Q30/rational output and explicit prepared/one-shot boundaries. |
| Bilinear enlargement | [Packed row storage](BILINEAR-PACKED-ROW-STORAGE.md): complete RGBA8 Up8 upper.993 passes; source-valid other bilinear boundaries retain their own evidence/H status. |
| Text, drawing and composition | [Complete integration](TEXT-DRAWING-CURRENT.md) and [RGBA8 composition](BYTE-COMPOSITE-EXACT-VECTOR.md) pass/stop. [Prepared drawing](TEXT-ENDPOINT-STORAGE.md) remains two gaps; allocation59000B/draw and inconclusive CPU gains stay distinct. [Font loading](FONT-OWNED-STORAGE.md) upper2.396 is a gap,176B/op; pooled capacity/lazy native validation are separately labeled. Independent codec/resizing work continues. |
| Inspection and other public boundaries | Use the named inventory below and [current summary](../docs/benchmark.md). Native adapters, acquisition/copy/output ownership and source metadata stay explicit. Sub-microsecond gaps remain required but do not displace material codec/resize work. |

[Preparation/storage attribution](ARCHITECTURAL-PREPARATION-AND-STORAGE.md) explains
complete one-shot versus retained native state. Current owners link their earlier
milestones and raw evidence. The [previous coverage narrative](https://github.com/lokad/image/blob/54112a2147d325eb69faac255e9e71ad5c808fe6/benchmarks/WARM-COVERAGE.md)
retains superseded costs, qualification and experiment links in Git history;
those do not govern current status or permanently close algorithm families.

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

Current invalidation and next execution:

- EQE772B changes shared four-pixel enlargement accumulation and the private byte
  quartet preparation. Its fifteen named controls supersede all twelve enlargement
  costs plus Mixed8/TightMixed8/TightVertical8. Other BicubicMath bodies, codec,
  Lanczos, text/drawing and ownership algorithms remain unchanged. Earlier word
  mixed and other unchanged providers stay valid within their tested scope.
- JPEG's current owner refreshes25 changed pixel-decode paths; four unchanged
  DC-only/coding-validation cases retain their source-valid confirmations.
- Reuse passing evidence only where the relevant actual production paths and
  complete boundaries are unchanged. Current uncertainties are not demonstrated
  optimization needs; resolve them before tuning. Every named H/missing case
  remains outstanding until applicable current passing evidence exists.
- Select useful absolute savings from current attribution. The [cached-wide
  attribution](CACHED-WIDE-CURRENT-PREPARATION-ATTRIBUTION.md) and [scheduling study](CACHED-WIDE-WINDOW-SCHEDULING-EXPERIMENT.md)
  retain safe-window leads and adverse/inconclusive implementations; inspect
  current sources before proposing a materially revised design. Passing quartet
  cases stop. Text/drawing never globally blocks independent codec/resizing work.
- All cited raw archives, released packages and reproducibility remain retained.
  Publication and the all283-path completion audit are separate; PLAN.md is ignored.
