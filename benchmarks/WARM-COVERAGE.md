# Material warm coverage

Current architectural phase, 2026-11-05. **282 material required paths** remain
in scope; the 125 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion needs current passing evidence for every named path, with scope stated.

## Current family status

| Required family/boundary | Current evidence and remaining work |
| --- | --- |
| Resident lossless WebP | [Matcher preparation](WEBP-MATCHER-CANCELLATION-PREPARATION.md) retains four passing original/diagram/depth/async lossless boundaries; stop them. Opaque remains a gap (upper 1.063). [Photograph's counterbalanced check](WEBP-PHOTOGRAPH-COUNTERBALANCED.md) remains uncertain (interval 0.990–1.020), with unchanged 6,516,744 B/op and exact160,032 B packet. No CPU gain or parity claim; no unchanged repeat. Preserve earlier [index ownership](WEBP-ALPHA-MATCH-INDEX-REUSE.md) evidence. |
| Resident VP8 encoding | [Luma4 preparation](VP8-LUMA4-PREPARATION.md) confirms286.80µs/2.32% scoped photo gain in twelve counterbalanced pairs. All eight costs remain gaps; the linked table states every current upper. No owned capacity; paired809072B/op unchanged. Standalone photo809225B/op uses different diagnostic batching/GC; no causal allocation claim. Adverse raw alpha points/native clock movement retained without an alpha CPU claim. Earlier [residual preparation](VP8-RESIDUAL-PREPARATION.md), [full-block quantization](VP8-FULL-BLOCK-QUANTIZATION.md), [matcher](WEBP-MATCHER-CANCELLATION-PREPARATION.md) and [ALPH storage](WEBP-ALPHA-COEFFICIENT-STORAGE.md) retain scoped history. |
| Other WebP encoding boundaries | [Current eight-control qualification](WEBP-MATCHER-CANCELLATION-PREPARATION.md) includes full depth normalization and fresh async nonseekable output; the async producer proof is refreshed without builds. Passing cases stop. Other affected H costs remain historical; source/settings/packet/ownership gates stay unchanged. |
| PNG encoding | [Block preparation](PNG-BLOCK-PREPARATION.md) shares frequency/code storage and uses linear canonical assignment. Same-batch allocation saves148,320B (5.7%) on wordSmallest9 and19,936B (2.4%) on byteFastest1; no CPU gain/equivalence/regression claim. Fifteen complete one-shot costs current: thirteen gaps; byteOptimal6 uncertain (upper1.045), gray16 passes/stops (upper0.987). [Four color boundaries](PNG-COLOR-ONE-SHOT.md) now include identical conversion/packing setup and disposal. [Delivery/cancellation boundaries](PNG-BOUNDARY-ONE-SHOT.md) retain word token/forward uppers1.116/1.140, asyncbyte1.029. Exact packets and complete delivery/ownership remain; no speedup claim follows the baseline change. Existing384KiB admission,2620B extra operation-array capacity, zero scratch afterdisposal; physical/complete admitted peaks unmeasured. Earlier tree/token preparation is linked history; other affectedH boundaries remain historical. |
| JPEG and image reading | [Exact grayscale row batches](JPEG-GRAYSCALE-ROW-BATCH.md) save32.71µs/1.93%raw/4.04%normalized on the existing RGBA8→gray control, unchanged155,436B/op and exact packets. The gap remains (upper1.103); native clock change+2.19% is disclosed. Other image-grayscale timing affected by preparation is historical until refreshed. Caller-GRAY/RGB and other color/sampling/reading paths reuse valid proof; keep their named resident/stream/async/inspection/validation scope. |
| Bicubic columns | [Shared word preparation](BICUBIC-WORD-COLUMN-PREPARATION.md) saves126.92µs/14.46%raw on the existing word256×1294KiB control, upper1.223; nativeclock+0.15% disclosed. [Byte preparation](BICUBIC-COLUMN-PREPARATION.md) retains its8.84µs/1.41%raw gain and upper1.363, nativeclock+9.05% disclosed. Both remain gaps with exact pixels and4KiB admission; no allocation-saving claim. Other affected named H costs remain historical. |
| Bicubic streaming | [Shared streaming blocks](BICUBIC-SHARED-STREAMING.md) use two schedulers/four exact kernels within4KiB, saving66.62/51.17µs on byte/word8×17. Eight current controls: six pass/stop; RGBA8 8×17/4×9 retain gaps (uppers1.127/1.269). Allocations unchanged. |
| Lanczos3 | [Paired certificate preparation](LANCZOS-PAIRED-CERTIFICATE-PREPARATION.md) confirms175.99µs/4.20% elapsed gain on prepared word enlargement with counterbalanced immutable builds. Standalone raw/native clock movement is disclosed. All eight affected costs are current: five pass/stop; prepared byte/word reduction and word enlargement retain gaps (uppers1.158/1.097/1.052). Capacity is unchanged. Preserve exact Q30/rational/certified results and native float/prepared-state boundaries; [shared scheduling](LANCZOS-SHARED-ROW-DRIVER.md) remains linked history. |
| Text and drawing | All 14 additions below now have measured evidence. [Layout/fitting and approved references](TEXT-DRAWING-CURRENT.md) close the old reference decisions. Four layout/fitting controls and RGBA16 compositing pass and stop. [Raster preparation](TEXT-EDGE-INTERVAL-PREPARATION.md) improves both depths but retains gaps; other current fill/copy/composite/font costs remain in the summary. |

The current production build passes **5,639 full/live checks per OS**, affected
normal/scalar and AVX512-off checks, and the same actual Release package/all three
examples on Windows/Linux. Three strengthened test cases rerun normal/scalar after
correcting their generator; all other checks reuse unchanged proof. That qualifies
correctness and packaging, not all282 CPU gates.
Fourteen displayed WebP and eight streaming-resize costs are current. Other
affected boundaries marked H remain historical until their named reconciliation.

The [transparent capture before full-block quantization](WEBP-ALPHA-CURRENT-ATTRIBUTION.md)
retains source-specific matching, coefficient, predictor and cache-history leads.
The [current capture and fixed-distance audit](WEBP-FIXED-DISTANCE-ATTRIBUTION.md)
retain the matcher lead, but counts do not justify updating two fixed-distance
recurrences at every visited position. No production or current-cost change.
The [scalar-window stage](WEBP-SCALAR-WINDOW-PREPARATION.md) is a measured1.11%
photographic regression, restored exactly. Fewer checks or allocations alone do
not establish elapsed gains; a revised design needs distinct supporting evidence.
The [expanded Lanczos coefficient stages](LANCZOS-COEFFICIENT-PREPARATION.md) have
adverse short points and add97,952B retained capacity; both restore exactly.
No independent timing, package reproof or current-row replacement follows.

The [ALPH representation/lifetime audit](WEBP-ALPHA-REPRESENTATION-AUDIT.md) finds
that narrowing the working frame would lose its larger coefficient-storage
handoff, adding183,066 B net payload allocation across those phases. No CPU gain
is demonstrated and no production change follows. This source-derived result
does not close compact representations or variable token storage as families.

The [RGB16](PNG-RGB16-ONE-SHOT.md) and
[gray-alpha16](PNG-GRAY-ALPHA16-ONE-SHOT.md) PNG controls now include fresh native
codec/packet/frame preparation and disposal. Their uppers1.048/1.052 remain gaps;
boundary corrections are not incremental speedup claims.

Superseded experiments, attribution captures, useful retained tests and raw
manifests remain in the [preceding coverage record](https://github.com/lokad/image/blob/14155d1596330af7b97dcfd8707e160ca3e39b10/benchmarks/WARM-COVERAGE.md)
and linked family reports. Those records preserve adverse/inconclusive findings
without turning this current checklist into accumulated experiment chronology.

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

Current WebP invalidation: ordinary quantization changes all eight displayed VP8
encodes, including trellis chroma/Walsh and distortion-only selection; all eight
are current. Other affected VP8 H boundaries remain historical. Six VP8L costs,
decoders, PNG/JPEG, resizing and text/drawing reuse unchanged-path evidence. Earlier
whole-execution profiles predate the new quantization path; unchanged-method
source/JIT observations remain attributable without claiming current CPU shares.

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

Current priorities: equivalent preparation/reuse boundaries, substantial WebP
workspace lifetimes, shared resize execution and further codec work selected from
attribution. Keep passing cases stopped. Text/drawing proceeds independently;
pending references never block justified codec/resizing work. PLAN.md stays ignored.

All earlier reports, raw evidence, release packages and source-specific
invalidation notes are retained through the linked history. They do not establish
current blanket qualification or permanently close algorithm families.
