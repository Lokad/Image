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
| Resident VP8 encoding | [Compact residual programs](VP8-COEFFICIENT-PROGRAM.md) replace method2 full-frame coefficients with block scratch and operation-owned compact/literal instructions recorded with direct statistics. Independent complete effort2 saves61.81µs/2.14%raw/2.15%normalized; both confidence views separate. Current2825.91/native2510.76µs,upper1.132 remains gap. GC741386→406705B; public admitted diagrampeak732597→397205B, retained0. Densepeak886733→930669B is a measured tradeoff. Nine existing costs current:4pass/3gaps/2uncertain. Effort6 passes a fixed four-current-launch check, upper.981384; selectedeffort5/photo gaps, bothALPHcontrols uncertain. Earlier [four-mode launch history](VP8-FOUR-MODE-TRELLIS.md) is retained; no other incremental or general stability claim. Passing cases stop. |
| Other WebP encoding boundaries | [Current eight-control qualification](WEBP-FRESH-HISTOGRAM.md) preserves complete depth normalization and fresh awaited nonseekable async ownership. Passing cases stop. Other affected H costs remain historical; source/settings/full-packet/quality/resource gates stay unchanged. |
| PNG decoding/coding validation | [Current framing controls](PNG-CURRENT-FRAMING-COSTS.md):24 independent confirmations,20pass/4gaps. Gaps:liveRGBA8 upper1.113, tight/defaultRGBA16 uppers1.052/1.031, RGB16 upper1.062; allotherconfirmed cases stop. [Shared-root control](PNG-PREPARED-PREFIX.md) separately retains422µs/12.8%gain,upper1.014gap,1638214B/op and1766641Bcoarsepeak. Eight displayed controls5pass/3gaps; remaining costs and allocations in family report. Twenty-seven existing named decode cases still need current proof. Historical contradictions/raw retained; header-only operations/encoding unaffected. |
| PNG encoding | [Lazy view preparation](PNG-LAZY-VIEW-PREPARATION.md) independently saves390.08/375.08µs (1.83%/2.16%) on complete Optimal16/Smallest8. Exact packets and384KiB admission remain; Grayscale8 measures+408B/op to551,772, ten other fresh allocations unchanged. All11level6/9 fresh one-shot costs refresh; fourFastest costs reuse unchanged source bodies. Fifteen controls:9pass/stop,2gap,4uncertain. ByteFastest and byteGray remain gaps; wordSmallest and ordinary/live/forward wordFastest are uncertain. Resolve uncertainty before tuning. [Greedy preparation](PNG-GREEDY-VIEW-PREPARATION.md) retains independent269/836µs gains; earlier evidence stays linked history. |
| WebP image reading | [Main validation elision](VP8L-VALIDATION-ELISION.md) removes discarded main VP8L raster/cache/inverse output, preserving auxiliary values and the shared syntax scheduler. Complete VP8L validation saves1030.69µs/30.66%raw/30.60%normalized;2330.73/native3047.56µs,upper.769PASS/STOP. VP8+ALPH validation4656.82/native5448.61µs,upper.858PASS/STOP. GC297070/13584B, first/live admitted peaks235152/11121B, retained0. Nine affected controls refresh:3pass/6gaps/0uncertain. Contemporary full-render comparison inconclusive, no demonstrated gain/loss. Opaque/live/simple VP8 paths reuse unchanged evidence; liveopaque5289.47/native4796.26µs remains gap. [Prior materialization](VP8-VALIDATION-MATERIALIZATION.md) and [alpha handoff](VP8-ALPHA-OUTPUT-HANDOFF.md) retain history and separate decode ownership evidence. |
| JPEG decoding | [Coefficient-reference lifetimes](JPEG-COEFFICIENT-REFERENCE-LIFETIME.md) prepares fixed block bounds and keeps integer cursors across fallible bit delivery. Independent live444 saves102.83µs/2.69%raw and2.77%normalized, both confidence views separated;3722.09/native3361.34µs,upper1.114 remains gap. Bothcontemporary GC samples701233B, no newheap/retention; frame200→168B. Fourordinary decoder controls/codingvalidation reuse unchanged paths: six total1pass/3gaps/2uncertain. [Earlier correction selection](JPEG-CORRECTION-BIT.md) and plane scheduling retain history/ownership evidence. |
| Bicubic columns | [Wide shared preparation](BICUBIC-WIDE-PREPARATION.md) saves24.28/94.49µs (4.81%/15.07%raw) on byte/word128×64 controls, unchanged113B/op and4KiB admission. Both remain gaps, uppers1.394/1.329; native clocks−0.65%/−3.85% disclosed. [Small word preparation](BICUBIC-WORD-COLUMN-PREPARATION.md) and [small byte preparation](BICUBIC-COLUMN-PREPARATION.md) reuse unchanged gains and gaps1.223/1.363. Four drivers share the same result-tail geometry helper; kernels and exact pixels remain. Other affected named H costs remain historical. |
| Bicubic streaming | [Shared streaming blocks](BICUBIC-SHARED-STREAMING.md) use two schedulers/four exact kernels within4KiB, saving66.62/51.17µs on byte/word8×17. Eight current controls: six pass/stop; RGBA8 8×17/4×9 retain gaps (uppers1.127/1.269). Allocations unchanged. |
| Lanczos3 | [Paired certificate preparation](LANCZOS-PAIRED-CERTIFICATE-PREPARATION.md) confirms175.99µs/4.20% elapsed gain on prepared word enlargement with counterbalanced immutable builds. Standalone raw/native clock movement is disclosed. All eight affected costs are current: five pass/stop; prepared byte/word reduction and word enlargement retain gaps (uppers1.158/1.097/1.052). Capacity is unchanged. Preserve exact Q30/rational/certified results and native float/prepared-state boundaries; [shared scheduling](LANCZOS-SHARED-ROW-DRIVER.md) remains linked history. |
| Complete bilinear enlargement | [Current boundary audit](bilinear-current-boundary-evidence.json) reuses the qualified immutable build without production changes or rebuilds. Existing RGBA8 Up8 measures2357.04µs/native2155.92µs, ratio1.093/upper1.134/lower.971: uncertain.102891B/op includes fresh~103KBhorizontal preparation and independent output ownership; the unchanged4MiBdefault pool reuses output backing. Older2914.62µs/2235512Bcost is superseded, not paired causal gain evidence. Native graph/context preparation remains retained and labeled. Stop tuning this uncertain case while larger demonstrated gaps remain. |
| Complete bicubic enlargement | [Shared byte/word preparation](BICUBIC-SHARED-ENLARGEMENT.md) uses one geometry/filter source and two output-backed schedules. Complete default RGBA16 independently saves179.26µs/4.97%raw (4.90%normalized),5,016,602→4,264,538B/op; the4,264,104B independent output remains required. All12existing full/axial/depth/default/bounded controls refresh:8gaps/4uncertain. Byte incremental timing is inconclusive, including its adverse normalized point; no equivalence or broader parity claim. Earlier output-reuse gains remain linked history. |
| Text and drawing | All 14 additions below now have measured evidence. [Layout/fitting and approved references](TEXT-DRAWING-CURRENT.md) close the old reference decisions. Four layout/fitting controls and RGBA16 compositing pass and stop. [Active-edge scheduling](TEXT-ACTIVE-EDGE-SCHEDULING.md) saves179.16/170.49µs (48.55%/47.38%) on prepared drawing8/16, with exact frames. Allocation+8672B/draw to92384; admitted Latin workspace stays48544B, retained scratch zero. Current uppers5.230/5.226 remain gaps; eight-frame Arabic/wrapped/fitting generalization is correctness, not broader CPU proof. [Owned font preparation](FONT-LOAD-PREPARATION.md) saves14.84µs/4.80%raw against a contemporary original; current294.11/native13.95µs,upper21.246 remains gap. Owned capacity unchanged, allocation~758KB; no peak reduction inferred. Other current fill/copy/composite costs remain in the summary. |

The current [residual-program milestone](VP8-COEFFICIENT-PROGRAM.md) qualifies
5,790 full/live checks per OS (2,525 internal / 2,317 public / 948 differential),
73 internal / 260 public per normal/scalar mode, and the SAME actual private
Release package/all three examples on Windows/Linux, including exact method2
async encoding under512KiB shared admission, cancellation/recovery and independent
output. Existing vector kernels reuse disabled-ISA evidence. Two coherent core
versions per OS; final core/embedded immutable for full/package qualification;
extra Windows internal build extends domain/ownership tests. No public/benchmark/
native/selector/fixture build. Eight displayed costs refresh;117 numerical rows
reuse unchanged paths. [Validation elision](VP8L-VALIDATION-ELISION.md),
[owned fonts](FONT-LOAD-PREPARATION.md), [VP8 tokens](VP8-VALIDATION-MATERIALIZATION.md)
and [JPEG lifetimes](JPEG-COEFFICIENT-REFERENCE-LIFETIME.md) retain unchanged-path
evidence and raw history. The283-path objective remains open; this is not a full
actual-package performance refresh.
The following [seven-control refresh](PNG-CURRENT-FRAMING-COSTS.md) changes no production:
seven current independentconfirmations,5pass/2gap,7displayedrowsfresh/118numbersreused.
Alloriginal gates andfrozenworkers pass. No rebuild/newfixture/profile or repeated
full/package qualification. The following seventeen core/color/filter controls add15passes/2gaps;27otherexisting PNGdecodecases remain affected
andunconfirmed; their exactnames are in currentevidence. No125-row actual-package
release refresh or283-path pass. [Attribution](PNG-RETAINED-ROOT-CURRENT-ATTRIBUTION.md)
retains frozen sources/failedpreflights. [VP8 block state](VP8-BLOCK-STATE-RESERVOIR.md)
and earlier codec/resize reports retain valid unchanged-path costs and raw evidence.

The preceding [shared resize milestone](BICUBIC-SHARED-ENLARGEMENT.md) retains179µs/
4.97%word gain,752064B/op saved, all12enlargement costs and both-depth package proof.
[Fixed model delivery](VP8-LIVE-MODEL-PREPARATION.md) and
[progressive JPEG scheduling](JPEG-PROGRESSIVE-PLANE-SCHEDULING.md) retain their
narrow raw gains/normalized uncertainty and JPEG984KB→701KBallocation reduction.

The bounded bilinear refresh reuses the same qualification and actual package:
no production change, builds, new fixtures or repeated timings. Its original
frozen selector/harness/native/pixel gates and12actuals pass. Raw
`bilinear-current-boundary-audit.tar.xz`,179,892B/33members,
SHA256`3639131210285af2f483a1fc3b005f1c8dc31ee599e13c4e789b762524f1f018`,
is mirrored/member-verified. A harness-identity preflight failure is retained;
zero timing starts before that correction. No causal claim from historical clocks.

Two [shared large-prediction prototypes](VP8-SHARED-STRIDED-PREDICTION.md) are
inconclusive and restored byte for byte. Independent complete live decode is flat
for compact staging and saves only a26.52µs point with overlapping confidence for
direct stride; allocation is unchanged. No unchanged favorable repeat or broad/
package repetition. Current qualified costs remain valid. Its following current
live-reader attribution leads to fixed-model delivery, then the current unshifted
reservoir above. Preserve all points; no repeated unchanged favorable samples.

The [rate-epoch pricing round](VP8-RATE-EPOCH-PRICING-EXPERIMENT.md) completes three
exact preparation stages and restores2e277bba. Its final Int32design has an
inconclusive.996332[.990248,1.002433]counterbalanced trellis ratio and+53,896B/op;
earlier partial preparation has a confirmed scoped loss. No broad/package repeat
or current-cost replacement. Retain the count/range/ownership evidence and every
adverse point. The following [invariant preparation/compact-storage round](VP8-INVARIANT-PRICE-STORAGE-EXPERIMENT.md)
verifies−6,624B/op but independently slows complete trellis encoding165.49µs/1.42%raw
and1.84%normalized, both confidence views separated. Its paired gain conflicts with
that actual boundary; photograph remains inconclusive. Restore the same qualified
checkpoint and retain every stage without broad/package repetition. Current costs
remain unchanged. Its following resize audit leads to the accepted shared output
workspace above; no pricing gain or raw allocation reduction is used as CPU proof.

A [broader rate-core prototype](VP8-CANONICAL-RATE-BOUNDARY.md) is rejected:
its paired gain conflicts with a confirmed326µs/2.96% actual standalone photograph
loss. Qualified source/modules/package restore exactly; current costs and proofs
above remain valid. Retain raw history outside renderer dependencies; no family ban.

[PNG preparation](PNG-LAZY-VIEW-PREPARATION.md) retains9passes/2gaps/4uncertain
controls and its independent gains. The [reader milestone](VP8-PREPARED-TOKEN-STATE.md)
retains440/304µs validation/stream gains and allsix reader gaps. Five unchanged
paths reuse that proof; the live control is owned by the current reservoir milestone.
Earlier accepted/adverse stages and source attribution stay
linked from family reports and the
[preceding coverage record](https://github.com/lokad/image/blob/97b1ad81/benchmarks/WARM-COVERAGE.md).
Retain cited raw evidence; history is neither current timing proof nor a permanent
family exclusion. Stop passing cases, resolve uncertainty before tuning and choose
material independent work from current attribution. The finite inventory below
is unchanged; publication remains separate and postponed.

The selected-mode effort5control uses unchanged qualified production/package inputs.
One benchmark harness build perOS, one generated selector and one derived native
worker on Linux suffice; production/internal/public tests and package are reused.
The native adapter changes only the three effort expressions from the existing
opaque wrapper. Full source processing, admission and independent packet ownership
remain timed; native wrappers/private scratch remain retained as labeled.
Raw `vp8-selected-trellis-boundary.tar.xz`,4,501,756B/88members,
SHA256`923340a5011063d8196bb6fff05ca95923388ff024abb7bf80159e005e1a8f79`,
is mirrored/member-verified. Source projections preserve previous control bodies
and enum values. This adds one required algorithm, not a causal performance gain.

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
  The initial baseline7384.12µs/upper1.185gap is retained in VCS420evidence.
  Current prepared-price production7058.42µs/native6423.68µs,741065B/op,
  lower.973/upper1.137 is uncertain; adverse275µspoint versus preceding stage
  remains inconclusive. Complete packet/pixel/source/ownership and
  quiet guards pass. No extra photograph/settings grid.
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
