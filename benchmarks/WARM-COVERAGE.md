# Material warm coverage

Current architectural phase, 2026-11-11. **289 material required paths** remain
in scope; the 138 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion needs source-valid passing evidence for every named path, with scope
stated. Missing/uncertain evidence and affected historical H rows remain open.

The latest accepted [bilinear preparation](BILINEAR-SHARED-PREPARATION.md)
passes 6,251 full/live checks per OS, 363 internal/779 public resize checks in
normal/scalar/disabled-AVX2/AVX512 modes and the SAME actual Release-package
consumer/all three examples. Byte reduction independently saves 68.39 µs/14.00%
raw/13.72% normalized; raw/all-actual and normalized intervals separate, allocations
fall 25,785→8,473 B/op. Word causal effect remains inconclusive with material
native movement. Four complete bilinear controls refresh: one pass/three gaps;
Up8 passes/stops, Up16's approximately 579 µs gap is next for attribution.
Exact pixels/packets, terminal admission/cancellation and ownership remain.
No new retained scratch. Unchanged [JPEG product/support preparation](JPEG-SHARED-BLOCK-SUPPORT.md),
[wide quartets](WIDE-BICUBIC-SHARED-QUARTETS.md) and other codec/resize/text
paths and affected ISA evidence are reused. This is not a whole-release timing
refresh; all 289 named paths still require valid passing evidence.

## Current family status

| Required family/boundary | Current owner and remaining work |
| --- | --- |
| JPEG decoding and coding validation | [Shared product/support preparation](JPEG-SHARED-BLOCK-SUPPORT.md): 29 reads, 28 pass / 0 gaps / 1 uncertain. All 27 pixel-decode costs refresh on the actual package; two unchanged coding-only validations reuse evidence. Sparse CMYK saves 16.85 µs/3.86% raw and dense RGB444 45.35 µs/3.87%; both confidence views separate. Sparse/caller CMYK pass and stop. Retained-context horizontal CMYK remains uncertain; equivalent fresh-context 398.40/745.74 µs upper .537 passes. No new owned capacity/retention; primary allocations 20,944/42,936 B/op. |
| JPEG encoding | [Gray tile preparation](JPEG-GRAY-TILE-PREPARATION.md) and [prepared replay](JPEG-PREPARED-HUFFMAN-BANK.md) retain current grayscale/caller and unchanged color evidence. Current default/sampling scopes, exact packets and explicitly labeled SIMD libjpeg-turbo grayscale boundaries remain distinct. |
| PNG reading | [Current framing costs](PNG-CURRENT-FRAMING-COSTS.md): 51 controls, 49 pass / 1 gap / 1 uncertain. Materialized live RGBA16 upper1.012 is a gap; live byte upper1.004 is uncertain. [Cached literal-run trials](PNG-CACHED-LITERAL-RUN-EXPERIMENT.md) show no useful lead and restore production; three independent behavior tests remain. Other affected historical stream/composition costs remain H. |
| PNG encoding | [Length classification](PNG-LENGTH-CLASSIFICATION.md): 15 complete fresh-state controls, 13 pass / 2 uncertain. ByteFastest/Gray8 need confidence resolution; the [bounded Fastest check](PNG-FASTEST-CURRENT-CONFIDENCE.md) remains uncertain. Exact complete packets are required. |
| WebP lossless/alpha encoding | [Huffman workspace](WEBP-HUFFMAN-WORKSPACE.md): 8 controls, 5 pass / 3 uncertain. [Current photograph](WEBP-LOSSLESS-PHOTOGRAPH-CONFIDENCE.md)81.00/libwebp83.39ms upper.981 passes/stops on the actual package DLL; no production speedup claim. Photograph allocation5,358,368B/op uses direct batches; supersedes20–30MB, not a new allocation saving. Selected candidates remain independently owned. Prior matching/clustering/counting gains are retained, not work to replay. |
| WebP lossy encoding | [Rounded-border prediction](VP8-ROUNDED-BORDER-PREDICTION.md), unchanged [histograms](VP8-FOUR-BLOCK-HISTOGRAM.md) and alpha storage: 5 pass / 4 uncertain. Exact packets, ordered ties and current effort/content boundaries remain; uncertainty alone is not a tuning need. |
| WebP reading | [Position preparation](VP8-POSITION-MODEL-PREPARATION.md): six refreshed VP8 controls, 4 pass / 0 gaps / 2 uncertain. Ordinary opaque has a confirmed 2.02% normalized gain; live 4,803.55/native 4,786.85 µs, upper 1.009, and stream remain uncertain. Every VP8 total grows 1,536 B/op, within unchanged 8 KiB admission; no retained scratch. Prior window failures remain history. Unchanged [output-owned palettes](WEBP-PALETTE-OUTPUT.md) retains six lossless controls, 3 pass / 3 uncertain. |
| Bicubic enlargement/shared byte publication | [Shared quartet execution](BICUBIC-SHARED-QUARTET-EXECUTION.md): 15 affected enlargement/mixed controls, 9 pass / 2 gaps / 4 uncertain. Complete bounded byte enlargement saves159.58µs/5.10% raw/5.12% normalized, upper.981 passes; same116B/op/4KiB. Horizontal byte and default mixed byte remain gaps. Word horizontal and tight mixed uncertainty remain explicit. |
| Mixed-axis RGBA16 bicubic | [Result-backed bands](BICUBIC-RESULT-BACKED-BANDS.md) retain unchanged word paths: default/vertical pass; bounded mixed893.16/zimg851.29µs upper1.056 is a gap. Byte mixed costs are owned by the quartet stage above. |
| Bounded packed bicubic | [Four-pixel preparation](PACKED-BICUBIC-FOUR-PIXEL-PREPARATION.md): byte405.59/zimg398.21µs upper1.028 and word422.81/397.48µs upper1.098 remain gaps. Same113B/op/4KiB; earlier indexed clocks are history. |
| Bounded narrow bicubic | [Shared vertical quartets](BOUNDED-BICUBIC-SHARED-QUARTETS.md): byte459.65/zimg451.35µs upper1.027 remains a gap; word506.31/574.04µs upper.888 passes/stops. Paired confirms save46.34/74.66µs raw and9.04%/5.71% normalized; both confidence views separate. Same113B/op/4KiB, no added owned capacity or retention; extra64B vector lifetime/JIT physical peak remains unmeasured. |
| Bounded wide bicubic | [Shared vertical quartets](WIDE-BICUBIC-SHARED-QUARTETS.md): byte371.62/zimg351.70µs upper1.069 remains a gap; word407.47/417.67µs upper.988 passes/stops. Paired byte saves44.34µs/10.66% raw/5.35% normalized with both confidence views separate; word CPU effect is inconclusive. Same113B/op/4KiB and no added owned capacity or retention; extra64B vector lifetime/physical JIT peak remains unmeasured. |
| Cached wider/other bicubic | [Shared preparation](BICUBIC-SHARED-PREPARATION.md):16 refreshed controls,13 pass/2 gaps/1 uncertain. Ordinary Wide8 retains its native-prepared gap; [current narrow byte publication](CACHED-NARROW-SHARED-PUBLICATION.md) refreshes Down8 to438.33/native419.09 µs, upper1.064 gap, plus passing Horizontal8/Vertical8; Down64x32_16 needs confidence. Both equivalent one-shot and both caller-owned prepared controls pass. Plans retain198432B, prepared137B/op. Narrow word-row isolation saves338.94µs versus contemporary original vertical-word cost; ordinary Wide8 costs11.83µs more. Older cache/word-preparation observations are superseded only on these16 affected paths. |
| Prepared narrow bicubic | [Shared certified publication](CACHED-NARROW-SHARED-PUBLICATION.md): prepared/one-shot byte saves 24.97/23.62 µs, with separated raw/all-actual/normalized intervals. Prepared byte 413.36/zimg418.98 µs, upper1.011 is uncertain; one-shot byte and unchanged word boundaries pass. Narrow plans retain199840 B; controlled byte peak331936 B excludes borrowed input. Aggregate totals137–138 B/op prepared, with no added owned capacity or allocation-saving claim. Four equivalent boundaries now3 pass/1 uncertain. |
| Lanczos3 | [Paired word rows](LANCZOS-PAIRED-WORD-ROWS.md): 8 refreshed controls, 6 pass / 1 prepared-byte gap (upper1.039) / 1 prepared-word uncertainty (upper1.012). The paired study saves41.10/41.15µs prepared/one-shot; final costs and clock reconciliation are separate. Word plan retains156248 B, up8208 B; same exact Q30/rational output and explicit prepared/one-shot boundaries. |
| Bilinear resizing | [Shared preparation](BILINEAR-SHARED-PREPARATION.md): four complete current controls, 1 pass / 3 gaps. Up8 upper.916 passes/stops. Down8 420.16/385.48 µs upper1.101 and Down16 465.51/449.98 µs upper1.051 remain gaps; 8,473 B/op each. Byte saves68.39 µs and17,312 B/op with separated confidence views; word causal effect is inconclusive. Up16 2724.96/2146.42 µs upper1.316,4,498,923 B/op is the next larger gap for preparation/exact-execution/output-storage attribution. No new retained scratch or numerical-policy change. |
| Text, drawing and composition | [Complete integration](TEXT-DRAWING-CURRENT.md) and [RGBA8 composition](BYTE-COMPOSITE-EXACT-VECTOR.md) pass/stop. [Owned render preparation](TEXT-RENDER-PREPARATION.md): ordinary byte/word87.12/92.57µs remain gaps,59,304B/op (+304), CPU change inconclusive. Prepared53.81/56.80µs save33.30/35.77µs versus current ordinary,56B/op with40,408B idle ownership; byte parity uncertain/word gap against explicitly prepared ImageSharp.Drawing. All8 frozen frames exact. [Font loading](FONT-OWNED-STORAGE.md) upper2.396 remains a gap; capacity/parser differences stay labeled. Independent codec/resizing work continues. |
| Inspection and other public boundaries | Use the named inventory below and [current summary](../docs/benchmark.md). Native adapters, acquisition/copy/output ownership and source metadata stay explicit. Sub-microsecond gaps remain required but do not displace material codec/resize work. |

[Preparation/storage attribution](ARCHITECTURAL-PREPARATION-AND-STORAGE.md) explains
complete one-shot versus retained native state. Current owners link their earlier
milestones and raw evidence. The [exact coefficient trial](CACHED-CUBIC-WEIGHT-PREPARATION-EXPERIMENT.md)
adds no production gain or current cost; accepted builds are restored. The
[current bounded narrow attribution](NARROW-BICUBIC-CURRENT-ATTRIBUTION.md) selects
shared ordered vertical traversal, now qualified by the bounded-quartet stage.
Neither diagnostic becomes a renderer dependency. The [previous coverage narrative](https://github.com/lokad/image/blob/54112a2147d325eb69faac255e9e71ad5c808fe6/benchmarks/WARM-COVERAGE.md)
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
both complete one-shot controls and prepared word. The later
[current shared publication](CACHED-NARROW-SHARED-PUBLICATION.md) supersedes the
byte cost: prepared413.36/native418.98µs, upper1.011 is uncertain; one-shot byte
passes. Plans retain199840B. Two new prepared paths extend285→287; fresh-native
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

- CBP780B refreshes all16 cached executor controls: Wide8/16, Down64x32_8/16,
  Down32x16_8/16, Down16x8_8/16, Down8/16, Horizontal8/16, Vertical8/16 and
  Cross8/16. Preparation/execution and the word-row call boundary changed;
  previous costs on these paths are superseded. Mixed8/16 return earlier through
  unchanged result-band executors; bounded fallbacks/enlargement and other filters
  keep source-valid evidence. The existing large default/compact managed screen
  refreshes to20.40/21.89ms without a native or historical-gain claim.
- EQE772B changes shared four-pixel enlargement accumulation and the private byte
  quartet preparation. Its fifteen named controls supersede all twelve enlargement
  costs plus Mixed8/TightMixed8/TightVertical8. Other BicubicMath bodies, codec,
  Lanczos and other unchanged providers retain their source scope. Earlier word
  mixed and other unchanged providers stay valid within their tested scope.
- CNP789 replaces only the private cached-narrow byte publication block with the
  existing certified helper. Repeated8/OneShot8 and default Down8/Horizontal8/
  Vertical8 refresh. Geometry, ordered sums, exact repair and word/wide/mixed/
  bounded paths remain unchanged; their qualified costs and codec/text evidence
  retain source validity. No added image, profile or settings grid.
- VPP788 replaces repeated coefficient-position/band addressing with one frame
  model preparation shared by ordinary/live/window/scalar/validation decoding.
  Six named VP8 cost paths refresh; exact probability syntax, numerical operations,
  polling and ownership remain. Encoder/VP8L and other codec/editing/text evidence
  is unaffected. No profile or fixture is repeated.
- JPEG's current shared-support owner refreshes all 27 pixel-decode paths,
  including DC-only and caller samples, plus the existing fresh-context view.
  Two coding-only validation paths retain unchanged source-valid evidence.
  Encoder Ac and scalar arithmetic/query bodies remain verbatim; older JPEG
  pixel CPU costs are superseded on these affected paths.
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
  Publication and the all289-path completion audit are separate; PLAN.md is ignored.
