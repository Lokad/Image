# Material warm coverage

Current architectural phase, 2026-11-12. **289 material required paths** remain
in scope; the 138 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion needs source-valid passing evidence for every named path, with scope
stated. Missing/uncertain evidence remains open; all formerly marked display costs are refreshed.

[Compact paired weights](COMPACT-BICUBIC-PAIRED-WEIGHTS.md) is accepted: the bounded
byte reduction independently confirms414.18/prepared-zimg418.13µs, upper.998,
113B/op and exact managed pixels. Raw5.77µs/1.38% and all-actual6.24µs/1.48% gains
have separated intervals; normalized confidence overlaps, native moves−0.36%.
Contemporary baseline was uncertain; no causal historical-gap closure claim.
No new storage/preparation/loan/tail/ownership state. Source/actual9388Bcaller
proof reuses72other resize costs and unrelated families. Current73controls:
64pass/0gaps/9uncertain. Windows/Linux6512full/live,570/791focused normal/scalar,
three storage suites and SAME actual measured package/all3examples pass. Disabled
ISA paths reuse unchanged guards/fallback evidence. One core/embedded build perOS,
no public/harness/native/fixture builds, onepack. Stop this passing control;
publication remains postponed.

Other current resize owners and their measured tradeoffs are summarized in the
family table below, including [bilinear metadata](BILINEAR-OPERATION-METADATA.md).
[Adjacent confidence](RESIZE-CURRENT-ADJACENT-CONFIDENCE.md) still owns unchanged
Wide8:371.91/377.37µs,range.953–1.012,uncertain. Its former TightDown8 cost is
superseded above. The [stride](RASTER-STRIDE-PREPARATION-TRIAL.md) and
[expanded-coefficient](COLUMN-COEFFICIENT-PREPARATION-TRIAL.md) trials retain
their adverse/inconclusive results and raw evidence as separate history; neither
is a renderer dependency or a permanent design-family ban.

[Wide chunk CRC](PNG-WIDE-CHUNK-CRC.md) improves the retained-native stored-word
complete encode by 29.48 µs/6.23% raw and 6.86% normalized, with separated
filtered/all-actual/normalized confidence. Current word 443.92/
406.92 µs upper 1.105 remains a gap. Fresh stored byte/word
upper 0.136/0.328 pass on equivalent one-shot boundaries;
natural-GC64 passes only its separate scope. All 20 encoder views preserve exact
complete packets; 17 fresh-state controls have 15 passes / 0 gaps /
2 uncertain. Gray8 and RGB16 need confidence resolution before tuning.
No new substantial owned/retained storage or allocation saving is claimed.
Windows/Linux each pass 6,510 full/live checks, 703 internal/246 public PNG checks
per normal/scalar/AVX512-off/AVX2-off mode and the same actual Release-package
consumer/all three usage examples. Two core/embedded builds per OS cover untimed A
and measured B; public/harness/native/fixture builds are reused. Publication postponed.

[Adler evidence](PNG-BLOCK-ADLER.md) still owns 51 unchanged read/validation controls:
49 pass/two uncertain (live8/tight cancelable16), plus shared-root passing evidence.
Encode-only CRC changes preserve standalone scan/identify Compute/Reduce generated
bodies and all shared fields/initialization/read/fold/reduce source. Other codec,
resize and text costs remain source-valid. Prior [IDAT](PNG-IDAT-FORWARDING.md) and
[stored workspace](PNG-STORED-WORKSPACE.md) ownership qualification remains valid;
superseded encoder timings and original raw evidence are retained as history.

[Eight-residue CRC trial](PNG-INTERLEAVED-CRC-TRIAL.md) is not retained: a scoped
10.40 µs/2.37% raw stored-word gain accompanies a material 1,253.99 µs/6.17% raw
adverse compressed-word capture against the unchanged contemporary baseline.
Normalized confidence overlaps; no uniform causal regression or family ban is
claimed. Exact packets pass, but this concern prevents acceptance. Production
source/DLLs/package are restored byte-for-byte, so current costs above remain.
One useful nonperiodic checksum test is retained: 896 independent comparisons per
normal/scalar/AVX512-off/AVX2-off mode; 3,182 internal checks each OS. Unchanged
public/live/SAME-package qualification is reused, with no repeated package proof.
All raw evidence is retained; rejected results are not renderer inputs. Continue
the independent demonstrated byte bilinear gap, without more CRC sampling.

[Output-backed Cross rows](CROSS-BICUBIC-OUTPUT-PREFIX.md) close both byte
Cross gaps:793.42/zimg875.67µs upper.913 and same-height496.61/573.93µs
upper.872,113B/op each. Independent178.42/141.71µs,18.36%/22.20%raw and
17.02%/16.65%normalized savings retain all actuals/native shifts. Shared prefix
preparation borrows91168B of already charged output, unwinds, then uses the
original strip tail. One4KiB reservation,80Bprefix/3440Btail explicit arrays,
no new heap/retention; physical/JIT/native peaks unmeasured. Exact qualified
managed pixels/native bounds/owners/cancellation stay intact. Ten new boundary/
alignment/poison/alpha/refusal/cancel/recovery tests pass. BothOSes6497full/live,
569internal/791public normal/scalar,3storage suites and SAMEactualpackage with
two explicit frozen Cross consumers/all3examples pass. One core/embedded eachOS,
no public/frozen-harness/native/fixture build,one diagnostic bridge/onepack.

All14affectedcosts reconcile:2Crosspasses,11enlargement passes/1uncertain.
HorizontalUp8firstsource-affected1486.10/zimg1518.51µs upper1.025 is uncertain,
not a presumed gap. Four passing raw captures are slower; two scoped paired
checks/clock shifts remain distinct from standalone costs, no universal gain.
At the Cross milestone73standalonecontrols60pass/5gaps/8uncertain. Its unchanged59costs reuse53
[base](SHARED-RESIZE-FILTER-WEIGHTS.md),five[generated passes](GENERATED-BICUBIC-SHARED-EXECUTION.md)
and one[DeepWord](DEEP-WORD-VECTOR-WEIGHTS.md) uncertainty. Root IL/actual timed
JIT match; disabled-ISA/other-family proof reused. All8equivalent prepared/
one-shot boundaries pass, plans retain198432Bwide/199840Bnarrow. Scoped adjacent
views stay separate. Stop confirmed passes and do not repeat for favorable bounds.

The [stored-PNG streaming trial](PNG-STORED-DIRECT-STREAM.md) is not retained:
65,536B/op saved, no demonstrated CPU gain, additional format state and two
adverse historical compressed comparisons. Exact qualified sources/modules/package
are restored; no broad/package repeat. The post-GC word gap and distinct passing
natural-GC64op scope remain open/unchanged.

[Streaming byte weights](STREAMING-BYTE-VECTOR-WEIGHTS.md) now close both resize
gaps:16×8 costs463.98/zimg483.72µs upper.970;8×17 costs457.40/516.75µs upper.890.
The affected8×4 control also passes,380.48/663.44µs upper.577. All113B/op, exact
pixels, original4KiB/4048B explicit arrays and zero added capacity/retention.
Raw47.05/88.49/121.42µs gains and normalized15.24%/17.96%/23.47% retain every
actual and separate native clocks. Same lane bits/products/add order/repair.
Actual shared2579Bcaller/root9388B instructions and block offsets match after
addresses/method-number/proven GS-cookie normalization, so70other costs remain
valid. At that stage73standalonecontrols62pass/3gaps/8uncertain; current bilinear metadata preparation above supersedes two costs. BothOSes6497full/live,
569internal/791public normal/scalar/3storage suites and SAMEactualpackage/all3
examples pass; existing package consumer explicitly covers all3frozen controls.
One core/embedded build eachOS, zero public/harness/native/fixture builds,onepack.
Current remaining resize gaps: TightDown8(~7.73µs), Wide8(~2.47µs).
Choose the next justified material preparation/reuse improvement; uncertainties
are not presumed gaps, and passing controls stop unless actually affected.
All289paths remain required; publication postponed. Historical
[Cross overlap](BYTE-CROSS-HORIZONTAL-OVERLAP.md),[quartets](BYTE-CROSS-SHARED-QUARTETS.md)
and[geometry](BYTE-CROSS-SHARED-GEOMETRY.md) retain original raw evidence;
their superseded Cross costs do not own the current summary. Prior adverse/
inconclusive publication/cache-view trials are linked from the current owner.

[Per-tile inverse color](WEBP-INVERSE-COLOR-TILES.md) saves191.79µs/6.14%raw/
7.18%normalized on complete live-token VP8L decode, with separated confidence
and native+1.127%. Current2,933.10/FFmpeg3,059.18µs,range0.907–0.989 passes/stops;
allocation stays152,728B/op. Seven actual decode costs refresh before qualification;
two validators bypass inverse transforms and reuse source-valid HFR847 evidence.
All six lossless reads/validators now pass; VP8/ALPH plus unchanged opaque controls
have5passes/1uncertain (live opaque). All frozen pixels/alpha/hiddenRGB/owners/
cancellation/idle-v2 gates pass. Native retains contexts and has no equivalent
cancellation token. Coefficients prepare once per tile; AVX2 batches and scalar
tails/fallback preserve exact signed/modulo arithmetic. No new owned/retained
capacity or allocation saving. One immutable core/embedded build perOS;
seven new oracle/boundary/cancellation tests. Full6,456checks perOS,1,033internal/
239public normal/scalar/AVX512-off/AVX2-off and SAMEactualpackage/all3examples
pass. Unchanged Huffman BMI2 and other family/ISA evidence reused.
Stop all six confirmed lossless controls; continue independent demonstrated
resizing work without assuming gaps in uncertain controls. The
[Huffman predecessor](WEBP-HUFFMAN-READ-FRONTEND.md) and
[current-build attribution](WEBP-INVERSE-COLOR-CURRENT-ATTRIBUTION.md) retain
their original sources/samples/JIT and hypothesis. No historical-stage sum or
uniform causal companion gain is claimed; profile counts are not phase times.

[Current opaque lossless confidence](WEBP-OPAQUE-CURRENT-CONFIDENCE.md) remains
uncertain after its first adjacent capture:73.41/libwebp73.50ms, range.915–1.070.
Complete121,896B packets and all pixels/owners/settings/quiet guards pass. Native
bracket variation remains visible; no presumed gap, speedup or favorable repeat.
Direct allocation4,822,760B/op, zero retained operation scratch; peaks unremeasured.
One allowlist-only driver build, unchanged production/frozen harnesses/current
package and6,439-check qualification reused. No new workload or broad campaign.

Unchanged [live PNG run preparation](PNG-LIVE-RUN-PREPARATION.md)
passes6,351full/live checks per OS,712internal/246public PNG/admission checks in
normal/scalar modes, and SAME actual Release-package/all three examples on
Windows/Linux. Word decoding saves93.08µs/2.89%raw/1.86%normalized against a
contemporary predecessor, with separated filtered/all-actual/normalized confidence;
native time moves−1.05%. Current3126.33/native3153.78µs upper1.001253 remains
uncertain. Byte1529.69/1535.97µs upper1.012 also stays uncertain; raw+9.90%
tracks native+10.19% with normalized overlap, not a byte speedup or uniform gain.
GC4,472/8,568B/op and4KiB framing admission are unchanged. Both cancellation
polls remain per symbol; no new storage/owner/pool. The51read/validation controls
now have49pass/0gap/2uncertain; stop unchanged passing paths. Resize/codec/text
ISA evidence is reused.

[Matched one-shot stored PNG](PNG-STORED-ONE-SHOT-BOUNDARIES.md) retains the
reference-boundary qualification and historical captures. [Current wide-CRC
costs](PNG-WIDE-CHUNK-CRC.md) supersede its encoder costs; fresh/prepared lifetimes
remain separate and historical subtraction measures no preparation time.

## Current family status

| Required family/boundary | Current owner and remaining work |
| --- | --- |
| JPEG decoding and coding validation | [Shared product/support preparation](JPEG-SHARED-BLOCK-SUPPORT.md): 29 reads, 28 pass / 0 gaps / 1 uncertain. All 27 pixel-decode costs refresh on the actual package; two unchanged coding-only validations reuse evidence. Sparse CMYK saves 16.85 µs/3.86% raw and dense RGB444 45.35 µs/3.87%; both confidence views separate. Sparse/caller CMYK pass and stop. Retained-context horizontal CMYK remains uncertain; equivalent fresh-context 398.40/745.74 µs upper .537 passes. No new owned capacity/retention; primary allocations 20,944/42,936 B/op. |
| JPEG encoding | [Gray tile preparation](JPEG-GRAY-TILE-PREPARATION.md) and [prepared replay](JPEG-PREPARED-HUFFMAN-BANK.md) retain current grayscale/caller and unchanged color evidence. Current default/sampling scopes, exact packets and explicitly labeled SIMD libjpeg-turbo grayscale boundaries remain distinct. |
| PNG reading | [Current block Adler costs](PNG-CURRENT-FRAMING-COSTS.md):51controls,49pass/0gap/2uncertain. Live16passes, upper.988; live8/tight cancelable16 remain uncertain. Complete full-depth pixels/metadata/owners/limits remain exact, allocation/admission unchanged. Three historical adverse comparisons reconcile against current baselines; no uniform gain or favorable repeat. Two-retained-image shared-root also passes. Historical stream/composition costs remain H. |
| PNG encoding | [Wide chunk CRC](PNG-WIDE-CHUNK-CRC.md): 17 fresh controls, 15 pass / 0 gaps / 2 uncertain. Exact complete packets, original settings and output ownership. Stored-word saves 29.48 µs/6.23% raw and 6.86% normalized, with separated filtered/all-actual/normalized confidence. Fresh stored8/16 upper 0.136/0.328 pass; retained-native word post-GC4 upper 1.105 remains a gap. Natural64 passes only its scope. No new owned/retained storage; allocation/admission unchanged. All affected costs reconcile before W/L/scalar/ISA/SAME-package qualification; read and other-family evidence is reused through explicit source/JIT scope. |
| WebP lossless/alpha encoding | [Huffman workspace](WEBP-HUFFMAN-WORKSPACE.md): 8 controls, 6 pass / 2 uncertain. [Current photograph](WEBP-LOSSLESS-PHOTOGRAPH-CONFIDENCE.md)81.00/libwebp83.39ms upper.981 passes/stops on the actual package DLL; no production speedup claim. Photograph allocation5,358,368B/op uses direct batches; supersedes20–30MB, not a new allocation saving. [Current transparent RGBA8](WARM-ADJACENT-CONFIDENCE.md) 52.99/53.66 ms upper .998 passes/stops with exact 98,590 B, 6,241,792 B/op, unchanged 3,656,309 B admitted peak. This resolves uncertainty, not a production speedup. Selected candidates remain independently owned; opaque lossless remains uncertain; [current word alpha](WARM-ADJACENT-CONFIDENCE.md)53.40/53.30 ms upper1.008 remains uncertain with full16-bit normalization timed, exact98,590B and6,241,792B/op. |
| WebP lossy encoding | [Rounded-border prediction](VP8-ROUNDED-BORDER-PREDICTION.md), unchanged [histograms](VP8-FOUR-BLOCK-HISTOGRAM.md) and alpha storage: 6 pass / 3 uncertain after the [current byte-alpha confirmation](WARM-ADJACENT-CONFIDENCE.md). Exact packets, ordered ties and current effort/content boundaries remain; uncertainty alone is not a tuning need. |
| WebP reading | [Inverse color tiles](WEBP-INVERSE-COLOR-TILES.md): live-token VP8L2.933/FFmpeg3.059ms,range0.907–0.989 passes after a verified191.79µs/7.18%normalized gain;152,728B/op unchanged. Seven actual decodes refresh, two validators reuse source-valid evidence. All six lossless reads/validators pass/stop; VP8/ALPH plus unchanged opaque controls5pass/1uncertain(live opaque). No new owned/retained capacity; exact samples, cancellation and owners retained. Do not presume a gap for the unchanged uncertainty. |
| Bicubic/bilinear resizing | [Compact paired weights](COMPACT-BICUBIC-PAIRED-WEIGHTS.md) confirms TightDown8 414.18/418.13µs upper.998 pass,113B/op;5.77µs/1.38%raw gain, normalized overlap. No new storage/state,72other costs reuse source/JIT scope. [Bilinear metadata](BILINEAR-OPERATION-METADATA.md): word50.44µs/11.55%raw/12.31%normalized gain; byte4.55µs/1.16%raw cost, normalized overlap. Both pass,8473B/op, unchanged12608B admission. Wide8 remains371.91/377.37µs range.953–1.012 uncertain. Across73controls64pass/0gaps/9uncertain; eight equivalent prepared/one-shot controls pass. No new retained capacity. Other source-valid costs reuse streaming/Cross/generated/DeepWord evidence. Current6512W/Lfull/live/SAME-package qualification follows affected reconciliation; stop passing cases and resolve uncertainty before tuning. |
| Lanczos3 | [Current byte confidence](LANCZOS-BYTE-PRODUCT-PREPARATION.md): unchanged actual package 489.94/zimg 491.00 µs upper1.015 is uncertain, superseding the former byte gap. Eight controls now6pass/0gap/2uncertain; six passes stop. GC138B/op and plan139800B unchanged. Product-row A confirms4.15%raw/4.21%normalized adverse with separated intervals; B adverse screen, both restored. Two useful ownership tests pass454resize checks per normal/scalar/AVX2off/AVX512off perOS; full/package proof reused. Other seven costs remain [source-valid](LANCZOS-PAIRED-WORD-ROWS.md), including word uncertainty upper1.012 and156248B retained plan. Exact Q30 and explicit complete boundaries unchanged. |
| Text, drawing and composition | [Cleared canvas storage/public pipelines](CLEARED-CANVAS-STORAGE.md): one-shot1313.88/ImageSharp6739.69µs upper.218 and preparation+8fresh-canvas draws2055.00/7634.49µs upper.302 pass/stop; measured amortization256.87/954.31µs/draw. Complete public integration includes all fitting/layout calls and canvas/output ownership/disposal. GC283275/9256953B one-shot and284623/9494633B per batch. Active prepared capacity39904B, governed peak807708B; default idle font/canvas sequence can retain1248596B within unchanged two-array/4MiB ceiling. Steady fresh-canvas105.65/81.93µs range.850–2.006 is uncertain; existing-canvas103.39/78.81µs upper1.609 remains a gap. Earlier [loading](FONT-OWNED-STORAGE.md)/[drawing](TEXT-RENDER-PREPARATION.md) stages retain eager/lazy validation and preparation differences; never sum them or infer overall slowdown. Original exact full-word/layout/fitting/visual/owner gates pass; current W/L/scalar/SAME-package proof follows affected reconciliation. No arbitrary repeat-count/general-font claim. Independent codec/resizing work continues. |
| Inspection and other public boundaries | Use the named inventory below and [current summary](../docs/benchmark.md). Native adapters, acquisition/copy/output ownership and source metadata stay explicit. Sub-microsecond gaps remain required but do not displace material codec/resize work. |

[Canvas/text attribution](TEXT-CANVAS-ATTRIBUTION.md) retains the original lead.
Its accepted [cleared-rental stage](CLEARED-CANVAS-STORAGE.md) reduces fresh canvas
ownership/clear/disposal80.66→7.18µs,491657→113B, with separate live/idle ownership.
The two complete public comparisons precede further text tuning and pass/stop;
diagnostics remain scoped, never additive phases or an overall slowdown claim.

[Preparation/storage attribution](ARCHITECTURAL-PREPARATION-AND-STORAGE.md) explains
complete one-shot versus retained native state. Current owners link their earlier
milestones and raw evidence. The [exact coefficient trial](CACHED-CUBIC-WEIGHT-PREPARATION-EXPERIMENT.md)
adds no production gain or current cost; accepted builds are restored. The
[current bounded narrow attribution](NARROW-BICUBIC-QUARTET-ATTRIBUTION.md) refreshes
the actual package after accepted vertical quartets. One2s capture identifies
source-pair split stores/indexing as a bounded lead for a single64B write, already
used by the wide executor. No new statistical timing/build/fixture/qualification;
instrumented elapsed is discarded. Earlier pre-quartet attribution remains history.
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

The [current generated-byte profile](GENERATED-BYTE-CURRENT-ATTRIBUTION.md)
confirms2×9 executes `GeneratedBicubicResize`:1,241instruction locations in row
filtering,731in coefficient generation and20in its scheduler, not phase costs.
Proposed next step: prepare one coefficient/index block for a bounded group of
source rows, preserving per-row tap order, vertical/clamped order, certificates,
exact repair and4KiB. Check nested storage and affected existing2×2/1×1 controls;
no full table, pool/session or new grid. Actual current package/harness/fixtures
and qualification are reused, zero builds or timing/qualification repeats.
Rawe16ae8b6/99868B/23members is mirrored/memberverified; instrumented elapsed is
discarded. This supplies no displayed result and is not a renderer dependency.

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

- CBG854 changes only byte cross vertical-geometry routing. Two complete bounded
  byte reverse-mixed/identity costs supersede CMP853; original helper, decoding,
  numerical loops, repair, word paths, cached/prepared and other filters/codecs/text
  remain unchanged. Seven focused cases and actual package6463eachOS pass. New
  code follows the unchanged ISA guard; disabled fallback proof is reused.
  Inspect current byte2×9 before the next redesign; no new workload matrix.
- CMP853 extracts only the unchanged filter polynomial and three fixed bicubic
  preparation calls. Its thin dispatcher changes caller inlining, so all61existing
  bicubic, eight prepared and four bilinear controls are reconciled, not silently
  reused. Earlier CPU providers for these paths are superseded; their arithmetic,
  ownership designs, qualification and raw evidence remain history. Two adjacent
  scopes retain original uncertain standalone results; a contemporary previous-DLL
  byte-bilinear comparison is inconclusive. Historical raw time alone is not a
  gain claim. Codec/text/orientation/Lanczos paths retain relevant source validity.
  CBG854 now owns the two updated byte cross costs; the other71paths retain
  applicable source validity. No new matrix or global blocker; all289paths remain required.
- CIT849 replaces only inverse-color traversal with per-tile preparation and
  exact SIMD batches/scalar tails. Seven decode costs supersede affected HFR847
  observations; two validators bypass inverse transforms and retain HFR847 proof.
  Reader/prefix/cache/other-transform/encoder bodies reconstruct verbatim, and
  generated ReadImageCore remains8,245B. Other codecs/editing/text and Huffman
  BMI2 evidence remain valid. All six lossless reads pass/stop; continue an
  independent demonstrated resizing path rather than retuning this family.
- HFR847 changes only the common VP8L Huffman call boundary. Nine complete
  VP8L/ALPH/validation costs originally superseded WLR845 and affected WPP695/
  VPP788 observations; CIT849 now owns seven current costs and reuses two
  validators. The primary matched HFR baseline remains the valid incremental
  comparator. Three opaque VP8 controls do not use this reader and retain their
  prior evidence; unrelated codecs/editing/text remain source-valid.
- WCF876B changes only encoder chunk CRC, preserving exact polynomial state,
  complete packets and all ownership/cancellation/admission boundaries. Its 20
  complete encode views and natural64 replace the affected ABP875 costs. Standalone
  scan/identify Compute/Reduce generated bodies and shared source/initialization
  remain identical; ABP875's 51 reads/shared root and all other families are reused.
  Both correct prototypes retain their evidence; only final B is measured.
  Primary gain is scoped, retained-native word remains a gap, and uncertainty is
  resolved before any further tuning. No broad timing campaign or new grid.
- ABP875 changes only shared PNG Adler SIMD accumulation. All51read/validation,
  17fresh encodes, retained-native stored2/predictable/natural64word and shared-root
  costs supersede affected earlier CPU providers. Full packets/managed pixels,
  original settings/boundaries, scalar path, modulo, cancellation and ownership
  remain. Three contemporary baseline checks resolve historical read adversities;
  current uncertainty is not presumed to need optimization. PNG identify/inspection/
  CRC, other codecs, resizing and font/text/drawing sources remain unchanged.
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
