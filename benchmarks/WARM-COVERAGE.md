# Material warm coverage

Current architectural phase, 2026-11-14. **289 material required paths** remain
in scope; the 143 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion requires valid passing evidence for every named path. Missing evidence,
gaps and uncertainty stay open. No image, shape, depth, filter or quality grid is added.

[Released-image storage](RELEASED-IMAGE-STORAGE.md), with five refreshed
[Lanczos word-product reuse](LANCZOS-WORD-PRODUCT-REUSE.md) and three
[packed mixed-word cache](MIXED-WORD-PACKED-CACHE.md) costs and one current
[Lanczos enlargement](LANCZOS-ENLARGEMENT-CURRENT-CONFIDENCE.md), plus eight current
[shared axis-swapping edits](SHARED-AXIS-SWAP.md) and one
[bounded horizontal quartet](BOUNDED-WIDE-HORIZONTAL-QUARTET.md), plus six
[shared-copy/composition costs](SHARED-RASTER-COPY.md), four
[shared-reversal costs](SHARED-PIXEL-REVERSAL.md) and 24 refreshed
[JPEG RGBA/fresh costs](JPEG-HORIZONTAL-PREPARATION.md), with seven refreshed
[synchronous stream costs](PREPARED-MEMORY-INPUT.md), and four current
[row/copy views](VECTOR-ROW-COPY.md), with two refreshed shared-row flip costs in
[independent row copying](INDEPENDENT-ROW-COPY.md) and two current
[PNG16 stream comparisons](PNG-STREAM-WORD-CURRENT-CONFIDENCE.md) and one
[VP8/alpha stream comparison](VP8-STREAM-CURRENT-CONFIDENCE.md), owns the current **191
comparisons: 176 pass, 1 gap, 14 uncertain**. These are existing views, including
approved alternative boundaries, not 191 new material paths. The unchanged encoder,
inspection paths retain their identified evidence; three in-place copy controls
refresh separately in the new row-copy report. Four affected
JPEG caller-buffer costs also refresh; coding-only validation reuses unchanged proof.
Each OS passes **6,624 full/live checks**, focused normal/scalar/AVX512-off/AVX2-off checks and
the **same actual Release-package consumer / three examples**. Source-valid ISA
proof is reused. Current scope and raw evidence are in the linked owner, rather
than repeated experimental or qualification narratives here.

The released-storage stage's **317.42 µs / 9.74%** word-composition gain and
**1,063,078 allocated B/op** reduction remain history. Its refreshed complete cost
is superseded by the passing 2,891.18 / FFmpeg 2,943.12 µs adjacent comparison,
upper .994. The byte composition
passes at 1,257.22 / 1,478.18 µs, upper .857. No pipeline gain is inferred from
historical stage sums. The bounded byte-wide control's earlier adverse captures
remain history; its current cost is uncertain.
The default store remains two arrays / 4 MiB; larger idle retention, allocation,
logical admitted capacity and unmeasured physical peaks remain distinct.
The bounded [vertical-copy preparation screen](VERTICAL-COPY-PREPARATION-SCREEN.md)
supplies no useful gain; its separate row path is removed and qualified modules
restored. Its original adverse/inconclusive samples remain, without a broad rerun
or renderer dependency. The later [safe vector primitive](VECTOR-ROW-COPY.md)
keeps the original cancellation scheduling and qualifies a useful row-only gain.
Its full-raster extension stays inconclusive and is removed. Passing controls
stop; uncertainty is not passing evidence. The later shared-row stage retains
byte-specific rectangle gains after rejecting the word dispatch. Current byte
vertical flip is a small gap; historical confidence overlaps. Drawing/probe gaps
remain outside these 191 views; whole-library parity remains open.

## Current family status

| Required family/boundary | Current evidence and next action |
| --- | --- |
| PNG decoding | [Current owner](RELEASED-IMAGE-STORAGE.md): all 47 existing decode controls pass. Four coding-only validators and the caller-root retained-output control reuse unchanged paths. Stop passes. |
| Stream decode/composition | Both [current PNG16 adjacent comparisons](PNG-STREAM-WORD-CURRENT-CONFIDENCE.md) pass/stop: plain3,106.21/FFmpeg3,349.15µs upper.934,8,664Bop; complete decode/orient/crop/fit2,891.18/2,943.12µs upper.994,338,271.1Bop. [VP8/alpha stream](VP8-STREAM-CURRENT-CONFIDENCE.md) also passes/stops:5,841.97/5,986.04µs upper.988,240,496Bop, zero builds. Changed adjacency resolves old uncertainties without a production speedup claim; original samples and the inconclusive two-DLL pair remain. All seven synchronous decode/composition views, Identify and unchanged async PNG8 now pass. [Direct acquisition](DIRECT-STREAM-INPUT.md) retains async inspection/validation evidence. |
| JPEG reading | All 24 RGBA/fresh and four caller-buffer views pass and stop. [Horizontal preparation](JPEG-HORIZONTAL-PREPARATION.md) saves 4.03% / 16.92 µs in controlled complete-decode pairs, unchanged 20,944 B/op. Current horizontal410.41/FFmpeg418.11µs upper.988; fresh372.15/753.22µs upper.498; sparse414.93/433.23µs upper.970. RGB444's adverse standalone point is uncertain; one favorable controlled pair resolves the suspected contradiction without a standalone repeat. Two coding-only validators reuse unchanged proof. Earlier compact-output, row-product and raw-row studies retain adversity/inconclusive results separately. |
| JPEG encoding | [Current caller-RGB420 adjacency](JPEG-CALLER-CURRENT-CONFIDENCE.md) stays uncertain:1712.07/FFmpeg1650.90µs, ratio interval.991–1.057,388220B/op. Exact52091/52093B packets retain the strict2B metadata exception. No presumed gap or unchanged favorable repeat. [Gray preparation](JPEG-GRAY-TILE-PREPARATION.md) and other [Huffman replay](JPEG-PREPARED-HUFFMAN-BANK.md) costs reuse unchanged evidence; SIMD libjpeg-turbo grayscale stays explicitly labeled. |
| PNG encoding | [Current Gray8 adjacency](PNG-GRAY-CURRENT-PATH.md) remains uncertain, ratio .983–1.006, after the heap-entry path change; no favorable retry or presumed optimization gap. [Compressed heap entries](PNG-HUFFMAN-HEAP-ENTRIES.md) retain 16 passing fresh controls, exact packets and scoped 600 B/op reduction. [Stored/fresh/natural64 views](PNG-STORED-WORD-NORMALIZATION.md) keep their distinct lifetimes; retained word is uncertain. |
| Lossless WebP/alpha encoding | [Huffman workspace](WEBP-HUFFMAN-WORKSPACE.md), [photograph confidence](WEBP-LOSSLESS-PHOTOGRAPH-CONFIDENCE.md) and [adjacent alpha views](WARM-ADJACENT-CONFIDENCE.md) remain source-valid: six pass, opaque/word-alpha uncertain. Exact packets, selected-candidate ownership and operation-owned workspace reuse remain. |
| Lossy WebP encoding | [Current opaque adjacency](VP8-ENCODE-CURRENT-CONFIDENCE.md) remains uncertain: photograph .991–1.007, effort2diagram .980–1.001. No presumed gap or favorable retry. [Rounded-border prediction](VP8-ROUNDED-BORDER-PREDICTION.md) and [alpha scopes](WARM-ADJACENT-CONFIDENCE.md) retain six passes / three uncertainties across nine controls. Exact packets and tie order remain. |
| WebP reading | Current12views:nine pass, three uncertainties. [Opaque adjacency](VP8-DECODE-CURRENT-CONFIDENCE.md) keeps ordinary/simple/live-token opaque controls uncertain. [Current VP8/alpha stream](VP8-STREAM-CURRENT-CONFIDENCE.md) passes/stops at5,841.97/FFmpeg5,986.04µs upper.988. Exact pixels/alpha, source, ownership and reference gates pass; no presumed gap, production speedup or unchanged favorable repeat. VP8L stream retains its [prepared-input](PREPARED-MEMORY-INPUT.md) pass; seven other passing views reuse [released-storage](RELEASED-IMAGE-STORAGE.md) evidence. |
| Bicubic/bilinear resizing | Current 73 views: 67 pass, six uncertainties. [Bounded horizontal reuse](BOUNDED-WIDE-HORIZONTAL-QUARTET.md) saves 3.29% in paired complete byte-wide resize; standalone360.12/zimg348.02µs, upper1.041 remains uncertain,113B/op. Five unaffected specializations compile identically; reuse their valid costs. [Packed mixed-word cache](MIXED-WORD-PACKED-CACHE.md) passes bounded mixed-word and tight vertical; default mixed-word remains uncertain. Earlier adverse captures are retained. All eight equivalent one-shot/prepared controls pass and stop. Native float versus managed exact arithmetic stays explicit. |
| Lanczos3 | All eight declared one-shot/prepared byte/word comparisons pass and stop. [Current prepared word enlargement](LANCZOS-ENLARGEMENT-CURRENT-CONFIDENCE.md):4117.26/zimg4212.39µs, upper.990,4264760B/op and333396B retained plan. [Reduction product reuse](LANCZOS-WORD-PRODUCT-REUSE.md) retains its6.18% normalized gain and four passing reduction views. Exact Q30/rational arithmetic, float-native bounds and equivalent prepared/one-shot ownership stay explicit; no gain is inferred from the confidence update. |
| Copy/turn transforms and drawing | [Independent row copying](INDEPENDENT-ROW-COPY.md) saves10.73%raw/10.75%normalized on separate byte rectangles and8.17%/9.14% on the two-move overlapping pair,56/112Bop and zero new owned storage. Both retain small gaps; word rectangle is uncertain after reverting its adverse dispatch. Current byte vertical8.752/FFmpeg8.299µs upper1.083 is a gap, with historical confidence overlap; word flip passes. Current20independent-result edits:14pass,one gap,five uncertainties. Word clone/normal remain uncertain. Other18edit paths reuse valid evidence; all eight [axis swaps](SHARED-AXIS-SWAP.md) pass/stop and [shared reversal](SHARED-PIXEL-REVERSAL.md) retains its4.64µs byte-half-turn gain. Original adverse/inconclusive extensions remain in their reports, without favorable retries or permanent family bans. Tiny probes remain visible. |
| Font/text public integration | Both complete one-shot and preparation-plus-eight-draw comparisons pass and stop. Report preparation separately and amortize the measured complete batch. Stage loading/drawing costs remain diagnostics with validation/retained-state differences, not evidence of overall slowdown. Preserve exact layout/fitting/full-word and visual gates; no face/script grid expansion. |
| Inspection | [Prepared input](PREPARED-MEMORY-INPUT.md) resolves complete seekable PNG Identify:15.08/reference adapter15.88µs, upper.953,192B/op,2.75µs/15.40%raw gain. Input acquisition peak393216→262144B; no idle-retention or universal decode-peak claim. [Direct-input](DIRECT-STREAM-INPUT.md) async and named prefix/header adapters retain valid evidence. Metadata acquisition, container validation, coding validation and pixel decoding remain distinct. |

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
