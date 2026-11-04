# Material warm coverage

Current architectural phase, 2026-11-04. **282 material required paths** remain
in scope; the 124 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion needs current passing evidence for every named path, with scope stated.

## Current family status

| Required family/boundary | Current evidence and remaining work |
| --- | --- |
| Resident lossless WebP | [Histogram separation](WEBP-COLOR-HISTOGRAM-SEPARATION.md) saves 10.75 ms on the complete photograph. [Current reconciliation](VP8L-INVARIANT-ALPHA-PREPARATION-INCONCLUSIVE.md) restores the unchanged qualified encoder after an inconclusive preparation trial. Global photograph/opaque uppers are0.997/1.037; stricter paired uppers1.026/1.028 keep both OPEN. Exact packets/allocations remain. Original/diagram/depth/async pass and stop. |
| Resident VP8 encoding | Five unchanged opaque/word/trellis controls reuse [source texture proof](VP8-SOURCE-TEXTURE-PREPARATION.md). [Token-tail history](WEBP-TOKEN-TAIL-HISTORY.md) refreshes transparent RGBA8/16, saving about 476 KB/op; the measured alpha8 admitted peak falls 191,579 B. CPU gaps remain, with no established incremental gain/regression. |
| Other WebP encoding boundaries | [Current depth/async confirmations](WEBP-COLOR-HISTOGRAM-SEPARATION.md) pass on the accepted core, including normalization and fresh nonseekable output. Stop those controls. Other affected H cases remain historical; no blanket family pass. |
| PNG encoding | [Tree workspace reuse](PNG-TREE-WORKSPACE.md) saves834,544/94,960B per complete word/byte Smallest9 encode (24.4%/10.2%, same batching), without a CPU gain/equivalence/regression claim. Six equivalent one-shot costs are current. Five gaps: Smallest word/byte uppers1.104/1.061, wordOptimal1.064, Fastest1.124/1.282. ByteOptimal is uncertain (upper1.055); resolve before tuning. Existing384KiB admission,12,648B tree payload duringoperation/zero scratch afterdisposal; completephysical peaks unmeasured. Other affectedH costs stay historical. Prior token-output gain stays linked history. |
| JPEG and image reading | Keep the named gray/RGB/CMYK/YCCK/sampling and resident/stream/async/inspection/validation cases. This stage changes none of these paths; reuse valid proof. Current gaps and allocations remain in the summary and linked reports. |
| Bicubic and Lanczos3 | [Shared output ownership](RESIZE-OVERWRITE-OUTPUT-STORAGE.md) retains scoped bicubic gains/gaps; affected bilinear/bicubic H costs need reconciliation. [Paired Lanczos row output](LANCZOS-PAIRED-ROW-OUTPUT.md) saves 598 µs / 12.50% raw on prepared word enlargement and 51 µs / 8.35% on reduction. All eight affected costs are current: five pass and stop; prepared byte reduction and word reduction/enlargement retain gaps. Exact Q30/rational/certified pixels remain required. |
| Text and drawing | All 14 additions below now have measured evidence. [Layout/fitting and approved references](TEXT-DRAWING-CURRENT.md) close the old reference decisions. Four layout/fitting controls and RGBA16 compositing pass and stop. [Raster preparation](TEXT-EDGE-INTERVAL-PREPARATION.md) improves both depths but retains gaps; other current fill/copy/composite/font costs remain in the summary. |

The [current WebP capacity audit](WEBP-CACHE-HISTORY-CAPACITY.md) identifies
cache-history preparation at the prior peaks and unused alpha-token capacity.
The separately [qualified reuse](WEBP-TOKEN-TAIL-HISTORY.md) establishes current
allocation/admitted-peak savings without a CPU gain claim.

Recent [shared-event stream](VP8-SHARED-EVENT-STREAM-INCONCLUSIVE.md),
[alpha preparation](VP8L-INVARIANT-ALPHA-PREPARATION-INCONCLUSIVE.md) and
[shared magnitude grammar](VP8-MAGNITUDE-GRAMMAR-INCONCLUSIVE.md) investigations
leave production unchanged after inconclusive completed stages. Their linked
reports retain adverse results, ownership/correctness checks and raw evidence.
The [opaque audit](VP8L-INVARIANT-ALPHA-PREPARATION-INCONCLUSIVE.md) found no exercised 16-wide
color row; the [neighbor audit](WEBP-ALPHA-CURRENT-ATTRIBUTION.md) found little
reusable consecutive prefix work. Neither result closes a family or proves a CPU
savings bound. Do not replay unchanged candidates or retime unchanged passing paths.

The [JPEG frontier](JPEG-REFINEMENT-FRONTIER-INCONCLUSIVE.md) and magnitude rounds
retain five actual-reader and two exhaustive actual-statistics cases respectively.
Restored statistics/frame/rate/writer checks pass 70 per OS/mode. Reuse applicable
full/package proof for unchanged paths. The
[current PNG9 attribution](PNG-CURRENT-SMALLEST-WORD-ATTRIBUTION.md) led to two
[search execution stages](PNG-SEARCH-EXECUTION-INCONCLUSIVE.md). Neither has a
useful complete-operation lead; original sources, cores, tests and package are
restored exactly without rebuilds before the current token-output milestone. Its
count audit also rules out useful wider filtering on the word control. Older
attribution is history, not current machine-code or available-savings proof.

Eight existing WebP controls now have current costs; unchanged opaque
VP8, resizing and other codec algorithms retain applicable evidence. Other
adaptive VP8L/transparent ALPH boundaries remain historical until refreshed.
The current immutable production build passes 5,569 full/live checks per OS,
affected normal/scalar checks and the same actual Release-package consumer with
all three examples on Windows/Linux. That is correctness/package qualification,
not a claim that all 282 CPU gates pass.

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
admission. The remaining **253** base controls plus the **29** named additions
below give **282**, without adding a shape/depth/filter/quality grid. A later case
requires evidence of a distinct required algorithm or public boundary omitted
from this finite set. Differences in default option values are accepted.

Three required algorithm additions retain their frozen recipes:

- `DiagramLossyTrellis8`: existing 513x259 diagram, Q75/effort6. Coefficient
  dynamic-programming trellis per luma candidate, absent from method4. Complete
  6,818-byte packet SHA256
  `3c98407656841ff97b185e241e76f750f036c972cfe2a0b704ae2eb35cf0adee`.
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
| ValidateVp8lAlpha | OriginalExif VP8L alpha, 513×259 | Required ARGB history/inverse transforms without RGBA copy |
| ValidateVp8Alpha | OriginalExif lossy alpha, 513×259 | VP8 planes plus compressed-ALPH coding |
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

Current WebP invalidation: adaptive lossless predictor scoring and VP8 luma
candidate scoring are separate changed paths. Four resident lossless, the named
RGBA16/async lossless boundaries and seven resident VP8 costs are current through
their respective receipts. Other affected method/quality costs remain historical
until reconciled. Low-effort
bypasses, lossless-alpha encoding itself, decoders, PNG/JPEG, resizing/editing/text
retain applicable unchanged-path proof. Older whole-profile fractions of changed
bodies are attribution history, not current component costs or available savings.

Current resize invalidation: the common bilinear/bicubic output allocator changes;
unrefreshed filter/pipeline costs are historical, while pixel/repair kernels and
governed geometry admission remain unchanged. Lanczos/nearest and codec/read/text/
drawing paths reuse applicable evidence. The existing Up8 case checks the
2 MiB-versus-4 MiB retention boundary without adding a material required path.

Current Lanczos invalidation: accelerated row output changes at both depths,
including repair dispatch. All eight displayed one-shot/prepared reduction and
enlargement costs are refreshed. Earlier affected profiles/costs are history.
Plans, workspace owners, horizontal source products, numeric certificate bounds,
exact repair and portable integer execution remain unchanged, as do other families.
The removed final-tap fusion stage has no useful screen lead; all adverse evidence
is retained without another build or confirmation.

The [compact matching trial](WEBP-COMPACT-MATCHING-INCONCLUSIVE.md) preserves
exact packets but establishes no CPU/allocation gain; production is restored.
The [current opaque capture](WEBP-OPAQUE-CURRENT-ATTRIBUTION.md) identifies dense
scoring and predictor/histogram work for a bounded reuse audit, without timing or
available-savings claims. No affected qualification is invalidated.
The [current transparent capture](WEBP-ALPHA-CURRENT-ATTRIBUTION.md) retains matcher,
VP8, predictor and cache-history attribution. Its hot regions overlap earlier
inconclusive matching designs; the capture alone does not justify replaying them.
The [final-assignment redesign](WEBP-FINAL-REMAP-INCONCLUSIVE.md) retains both exact
stages and their adverse screens. Production/package are restored; two frozen-map/
ownership Facts supplement unchanged qualification. No useful gain is established.

Current priorities: preparation/reuse and operation-owned workspace, then shared
resize execution and further codec work selected from attribution. The
existing [word PNG controls](PNG-WORD-ONE-SHOT-BOUNDARY.md) have equivalent
one-shot boundaries including native endian workspace ownership. The accepted
[lazy parser](PNG-LOCAL-EXECUTION-STATE.md) refreshes byte/word Optimal; the
[greedy parser](PNG-FAST-LOCAL-EXECUTION-STATE.md) refreshes byte/word Fastest.
Byte gains remain inconclusive and all four retain gaps. Changed level6/9 and
level1 bodies invalidate other affected color/depth/async costs, including
SmallestSize and Fastest cancelable/forward-only boundaries. Stored0, decoding/
inspection and all other families retain applicable proof. The [word capture](PNG-WORD-CURRENT-ATTRIBUTION.md) is now
pre-change attribution; hot offsets/shares are not current component costs.
The [parsing-lifetime audit](WEBP-PARSING-LIFETIMES-AUDIT.md) does not justify a simple
rejected-token handoff. The [photograph profile before invariant scoring](WEBP-PHOTOGRAPH-CURRENT-ATTRIBUTION.md)
identifies a transform-search lead; its whole-profile fractions are now historical.
Trial failures are leads to reassess only with a materially revised design.
The [current alpha-filter lifetime audit](WEBP-PARSING-LIFETIMES-AUDIT.md#reuse-after-alpha-filter-emission)
identified reusable token capacity. The [completed handoff](WEBP-ALPHA-TOKEN-REUSE.md)
now proves an11% complete-operation allocation reduction, unchanged admitted peak,
exact packets and full package ownership qualification. CPU gain/regression is
unproved; adverse global and overlapping interleaved evidence are both retained.
The [cache-history lifetime trial](WEBP-CACHE-HISTORY-REUSE-INCONCLUSIVE.md)
reduces alpha allocation by8,672B/op but has no elapsed lead; production is
restored and current costs remain unchanged. Its exhaustive admission-refusal
coverage remains against the actual parser.
The [current JPEG refinement profile and lifetime audit](JPEG-REFINEMENT-STATE-ATTRIBUTION.md)
locate a material live-refinement cost. The [scan-state redesign](JPEG-SCAN-REFINEMENT-INCONCLUSIVE.md)
does not establish a useful gain and is restored; two stronger behavioral tests
remain. Current costs stay unchanged. The current instruction capture locates symbol/sign, position-mask and correction
work; its sample shares and byte counts are not CPU gains. No further sampling of
the unchanged scan-state design is justified.
Passing text/reference decisions do not block
independent codec/resizing work. PLAN.md remains local and ignored.

The [previous full coverage document](https://github.com/lokad/image/blob/b20a8738f858fe6117783445c7c4a5b17a84c388/benchmarks/WARM-COVERAGE.md)
retains checkpoint chronology, source-specific invalidation notes and every earlier
report/raw-evidence link. Those historical records and all cited raw archives are
preserved; they are not current blanket qualification or current experiment rules.
