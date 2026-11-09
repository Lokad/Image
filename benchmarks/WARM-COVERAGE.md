# Material warm coverage

Current architectural phase, 2026-11-09. **283 material required paths** remain
in scope; the 127 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion needs current passing evidence for every named path, with scope stated.

[Current preparation/storage audit](ARCHITECTURAL-PREPARATION-AND-STORAGE.md)
reconciles one-shot/prepared boundaries and measures three WebP admission lifetimes.
The [Huffman workspace](WEBP-HUFFMAN-WORKSPACE.md) owns eight affected
lossless/alpha encode costs. [Rounded-border prediction](VP8-ROUNDED-BORDER-PREDICTION.md)
owns effort2; six unchanged lossy costs reuse [four-block histograms](VP8-FOUR-BLOCK-HISTOGRAM.md);
the alpha pair uses the current storage handoff.
[Live prepared Huffman decoding](JPEG-LIVE-PREPARED-HUFFMAN.md) owns23 affected
baseline costs and reuses six unchanged progressive paths:29reads19pass/7gap/
3uncertain. Complete livebaseline444 saves201.15µs/11.18%raw/11.14%normalized,
upper.935 passes/stops. Temporary allocation/admittedpeak+32816B, retainedscratch0.
No passing affected control becomes a gap. RGB422 is now a gap; restart420 and
progressive live444 remain uncertain. Companion historical points establish no
causal gain/nonregression/equivalence; the adverse4:1:1 point remains without
favorable resampling. [Paired tile packing](JPEG-PAIRED-TILE-PACKING.md) retains
its scoped sparse-CMYK gain and adverse/inconclusive companion evidence.

## Current family status

| Required family/boundary | Current evidence and remaining work |
| --- | --- |
| Resident lossless WebP | [Huffman workspace](WEBP-HUFFMAN-WORKSPACE.md) reuses construction storage across alphabets/groups; selected codes stay independent. Six lossless controls4pass/2uncertain: photograph85959.95/libwebp82521.57µs upper1.080 and opaque upper1.013 remain uncertain; previous photo pass is superseded. Direct photo/diagram GC falls553184/37640B; aggregate5358980/3721041Bop. Measured peaks3470059/3251328B unchanged, retained0; no physical-peak or causalCPU claim. Exact packets/settings and adverse/raw/bracket evidence remain. |
| Resident VP8 encoding | [Rounded-border prediction](VP8-ROUNDED-BORDER-PREDICTION.md) shares exact interpolation preparation within distortion search and stores only the surviving winner. Complete effort2 saves112.73µs/4.27%raw/4.55%normalized; filtered/all-actual intervals separate. Current2525.56/native2511.68µs,upper1.013 is uncertain; GC406705B unchanged. Six other costs reuse source-valid [histogram evidence](VP8-FOUR-BLOCK-HISTOGRAM.md); the alpha pair uses [current Huffman workspace](WEBP-HUFFMAN-WORKSPACE.md): combined5pass/0gaps/4uncertain. Shared prediction, RDO and decoder source remain unchanged. Methods0/1/2 preserve exact packets, ordered ties, cancellation and ownership;32B rounded stack payload stays inside16KiB decision allowance. No new heap capacity or retention; physical/native peaks unmeasured. Passing cases stop; uncertainty is not a demonstrated gap. |
| Other WebP encoding boundaries | [Huffman workspace](WEBP-HUFFMAN-WORKSPACE.md) refreshes all eight affected costs on the actual-package DLL:4pass/4uncertain. Full-depth normalization, awaited nonseekable output and independent ownership stay timed. Transparent-alpha directGC falls668512B; peak3656309B/retained0 unchanged. Both alpha cases are uncertain (uppers1.065/1.002), superseding earlier alpha16 pass. No inferredCPU gain or demonstrated gap from uncertainty. Normal/scalar426internal+239public and6026full/live eachOS, SAMEactualpackage/threeexamples pass. |
| PNG decoding/coding validation | [Ordinary/slab publication](PNG-NORMAL-CURSOR-PUBLICATION.md) removes three exception-only publication regions from abandoned private readers, preserving exact completed cursors/public errors/partial stores/lookahead/polls/ownership. Tight/default RGBA16, RGB16 and tight-live RGBA16 save192/125/216/217us; all four former gaps pass. Separate shared-root pair saves260us and passes upper0.920, retaining1,638,214B/op, admitted1,766,641B and retainedpair1,062,936B. [All51controls](PNG-CURRENT-FRAMING-COSTS.md):49pass/1gap/1uncertain;43 refreshed,8 unchanged providers reused, no allocation change. Separate materialized-live word remains2919.94/native2894.87us upper1.012 gap; livebyte upper1.004 uncertain. Stop passing cases; resolve uncertainty before tuning. Older stream/composition costs remain historical. |
| PNG encoding | [Exact length classification](PNG-LENGTH-CLASSIFICATION.md) replaces repeated range search with the exact integer-logarithm formula. All15 complete one-shot costs refresh:13pass/0gaps/2uncertain. WordSmallest now passes/stops; ByteFastest [current-package confirmation](PNG-FASTEST-CURRENT-CONFIDENCE.md) remains uncertain at4757.43/native4717.98µs upper1.028; no unchanged favorable repeat. Gray8 upper1.047 remains the other uncertainty; no optimization need is inferred. Word raw180.09us/1.00% intervals separate both views, normalized intervals overlap; byte84.29us point gain has allactual uncertainty. No robust normalized/blanket gain claim. All packets remain exact,384KiB admission/retained0scratch and primaryGC820916/2437660Bop unchanged. [Streaming census](PNG-STREAMING-TOKEN-FREQUENCIES.md) and prior preparation remain history. |
| WebP image reading | [Output-owned palettes](WEBP-PALETTE-OUTPUT.md) remove a full-frame intermediate and copy when final palette expansion can publish directly into an admitted result. Complete palette16 saves168.22µs/26.72%raw/26.84%normalized, with separated filtered/all-actual confidence;461.31/native590.92µs upper.789 passes/stops. GC820190→288462B/op, result531468B until disposal, scratchretention0. An original513×67 palette16 fits239128B shared capacity below two full frames; this is a bound, not benchmark/physical peak measurement. Nine shared-method costs refresh: six lossless3pass/3uncertain; resident/live/stream uppers1.035/1.088/1.021 are uncertain, without companion causal claims. ResidentVP8 alpha upper1.014 uncertain; stream/validation pass. Three opaque VP8 paths reuse [quantizer evidence](VP8-FRAME-QUANTIZER-PREPARATION.md), combined VP8 scope2pass/3gap/1uncertain. Exact pixels, packed-source/result overlap, terminal refusal/cancellation/async cleanup,6058full/live eachOS, normal/scalar and SAMEactualpackage/threeexamples pass. Existing pools/governed bypass unchanged. [Final publication](VP8L-FINAL-PUBLICATION.md) retains earlier gains/adverse history; its palette regression is superseded. |
| JPEG decoding | [Live prepared Huffman decoding](JPEG-LIVE-PREPARED-HUFFMAN.md) reuses existing9/10lookup and scan-owned pairs while retaining every logical prefix/amplitude checkpoint. Complete livebaseline4441799.58→1598.43µs saves201.15µs/11.18%raw/11.14%normalized; filtered/allactual intervals separate, upper.935 passes/stops.23baseline costs refresh,6progressive costs reuse:19pass/7gap/3uncertain. GC10120→42936B/op; governedpeak541253→574069B; result531468B until disposal, scratch0. No new pool/session; physical/native peaks unmeasured. No passing affected control becomes a gap; RGB422 is now a gap. Companion historical points are not causal gain/nonregression/equivalence claims; adverse4:1:1 remains without favorable repeats. Exact pixels/state, logical polls, admission/refusal/recovery and independent owners pass. Current6100full/live eachOS,452internal/375public pernormal/scalar/AVX2off/AVX512off, SAMEactualpackage/threeexamples pass. [Paired tile packing](JPEG-PAIRED-TILE-PACKING.md) and [narrow scan preparation](JPEG-NARROW-SCAN-PREPARATION.md) retain scoped prior gains and unchanged-path evidence. |
| JPEG encoding | [Grayscale tile preparation](JPEG-GRAY-TILE-PREPARATION.md) shares row setup across up to8MCUs, preserving exact normalization/gray/DCT/entropy order. Complete RGBA→gray saves299.16µs/20.49%raw/19.96%normalized with separated filtered/allactual confidence:1160.95/labeledSIMDlibjpeg-turbo1403.16µs upper.840 passes/stops. CallerGRAY1059.14/native1112.88µs upper.969 also passes; no causal companion gain claim. Exact58910Bpackets; GC155435/155438Bop, no allocationgain claim. Extra768Bstack fits existing8192Bfixedadmission; coarsefixedphase4804B includes heap/metadata/explicitarrays. No new heap/owner/retention; fulladmitted/physical/JITstack peaks unmeasured, existing coefficientpools/governedbypass unchanged. Only2costs refresh;15color costs reuse [Huffman replay](JPEG-PREPARED-HUFFMAN-BANK.md):16pass/0gap/1uncertain, callerRGB420 remains uncertain. 6073full/live eachOS, normal/scalar/AVX2off/AVX512off and SAMEactualpackage/threeexamples pass. Strict packets/metadataexceptions and tight/restart/input ownership remain. |
| Bicubic columns and narrow cached rows | [Wide overlap](BICUBIC-WIDE-OVERLAP.md) shares ordered window arithmetic and offset preparation across cached, narrow and wide columns. Tight wide-byte saves30.65µs/6.72% raw/6.68% normalized; upper1.233 remains a gap. Ten affected costs:7 pass/3 gaps/0 uncertain; wide-word now passes and no earlier pass is lost. Other fresh costs establish no causal companion gains. Tight reduction byte upper1.208 and cached byte1.106 remain gaps. No new owned capacity or retention; bounded4KiB/113Bop. The existing large managed-only default/compact short points27.33/31.32ms have overlapping intervals, with exact normal/scalar/prior frames: no generalized CPU gain, default promotion or native parity. Workspace1585440/4096B, result2097152B until disposal, governed peaks3682592/2101248B. Current6120 full/live checks each OS,285 internal/729 public scalar/ISA and SAME actual package/three examples pass. [Earlier overlap](BICUBIC-SHARED-OVERLAP.md)/[shared repair](BICUBIC-SHARED-BYTE-REPAIR.md) retain scoped gains, adverse results and raw history. |
| Bicubic streaming | [Shared streaming blocks](BICUBIC-SHARED-STREAMING.md) use two schedulers/four exact kernels within4KiB, saving66.62/51.17µs on byte/word8×17. Eight current controls: six pass/stop; RGBA8 8×17/4×9 retain gaps (uppers1.127/1.269). Allocations unchanged. |
| Lanczos3 | [Certified word rows](LANCZOS-WORD-CERTIFIED-ROWS.md) reuse existing UInt32 products, replacing integer filtering/conversion in the accelerated producer. Prepared word reduction576.41→551.39µs saves25.02µs/4.34%raw/4.99%normalized with separated filtered/allactual raw+normalized confidence. Word enlargement point+14.87µs/.37% has overlapping intervals; no uniform gain. All eight actual-package-DLL costs current:5pass/3prepared gaps (byte reductionupper1.186,word reduction1.084,word enlargement1.043). Current original-byte comparison resolves historical clock movement without a causal gain/regression/equivalence claim. Capacity/retention unchanged; primaryGC139/4265332Bop. Exact Q30 pixels, strict Guard/certificate/repair,6017full/live eachOS, normal/scalar/AVX512off and SAMEpackage/threeexamples pass. [Earlier paired certificates](LANCZOS-PAIRED-CERTIFICATE-PREPARATION.md) retain history. |
| Complete bilinear enlargement | [Packed row storage](BILINEAR-PACKED-ROW-STORAGE.md) shares source premultiplication and uses contiguous filtered rows in existing admitted workspace. Complete RGBA8 Up8 saves201.65µs/9.05%raw; filtered/all-actual raw and normalized intervals separate. Independent2027.72/native2165.52µs,upper0.993 passes/stops;102891Bop unchanged. Exact rational pixels, source/owners/idle and scoped native bounds pass. Full preparation/output ownership stay timed; native graph/context stays retained and labeled. No new capacity/pool/session; governed outputs bypass unchanged4MiBdefault pool.6036full/live eachOS, normal/scalar/AVX512off and SAMEactualpackage/threeexamples pass. Other source-valid costs reuse evidence; no all-path or equivalent fresh-graph claim. |
| Complete bicubic enlargement and small-support word publication | [Shared word preparation](BICUBIC-SHARED-WORD-PREPARATION.md), [identity geometry/cache](BICUBIC-IDENTITY-CACHE-ROWS.md), [result-backed bands](BICUBIC-RESULT-BACKED-BANDS.md) and current [wide overlap](BICUBIC-WIDE-OVERLAP.md) reconcile the existing15word controls:11pass/4gap/0uncertain. Full/bounded enlargement and default horizontal/vertical enlargement pass/stop. Exact division, certified rounding/repair and declared rational/binary64 pixel contracts remain. DefaultRGBA8 [unchanged-build confirmation](bicubic-byte-enlargement-current-evidence.json) remains3108.21/native3037.83µs,upper1.026/116Bop,~70µsgap; no causal historical gain. Older publication/band experiments retain linked raw history. |
| Mixed-axis RGBA8 bicubic | [Result-backed byte bands](BICUBIC-BYTE-RESULT-BANDS.md) save149.15us/13.20%raw/13.02%normalized on matching contemporary package artifacts,1130.05→980.90us, separated filtered/allactual intervals. Current default/tight960.04/980.90us remain gaps (uppers1.116/1.142); vertical503.50/zimg587.18us upper.862 passes/stops. GC114/114/113Bop, no added owned capacity/retained scratch;28136B borrows admitted output before geometry and retires before suffix. One numeric range source,4KiB root reservation, exact original pixels/certificates/replay and ownership remain. Earlier1319.86us and cross-artifact costs are not causal baselines; discrepancy retained/unexplained. Both OSes5964full/live, scalar/ISA, SAME actual package/byte-band consumers/allthreeexamples pass. The mixed-byte bodies remain unchanged through wide overlap. Their preparation, band storage and row scheduling are the next attribution target; prior column-phase attempts retain adverse evidence without banning revised designs. |
| Mixed-axis RGBA16 bicubic | [Result-backed bands](BICUBIC-RESULT-BACKED-BANDS.md) save86.35/182.46us versus contemporary originals, with separated filtered/allactual raw/normalized intervals. Default841.53/native847.03us upper0.997 passes/stops; tight862.21/native844.22us upper1.025 remains~18usgap. One shared exact arithmetic body borrows28136B of admitted unpublished output, retires it before suffix stores, keeps4/8KiB root reservations and no extra heap/retention. GC114/114Bop. Shared vertical regresses61.98us/14.62% against a contemporary original, but remains passing486.02/native593.16us upper0.824,113Bop; tradeoff disclosed.Current15word status is11pass/4gap/0uncertain after the affected shared-overlap refresh; historical band counts are superseded. Exact pixels/certificates/repair, terminal refusal and bounded cancellation remain. Mixed/vertical result owns1059864/529416B until disposal; physical/native peaks unmeasured. Prior band/geometry costs remain linked history. |
| Cached bicubic preparation/execution | [Direct identity cache](BICUBIC-IDENTITY-CACHE-ROWS.md) retains exact vertical byte/word gains105.52/154.84µs, uppers.908/.904 pass/stop. Identity workspace383852→367436B, retainedscratch0/result264708/529416B until disposal; exact repair4KiB remains separately admitted. Six narrower nonidentity costs now belong to [wide overlap](BICUBIC-WIDE-OVERLAP.md): cachedbyte upper1.106 remains a gap; word reduction, horizontal byte/word and cross byte/word pass. Historical9control counts are superseded where affected. No new cache/session/owner or numerical policy; default output-pool retention remains separately labeled. |
| Cached wider bicubic | [Prepared geometry](CACHED-WIDE-PREPARED-GEOMETRY.md) shares contiguous-window proof across cached rows using dead padding; actual weights/offsets/counts/ordered arithmetic/repair stay exact. Complete Wide8 saves19.33µs/4.15%raw/3.32%normalized after one targeted original-only confirmation resolves an observed native-before transient; every earlier sample remains, completed candidate reused. Current446.07/zimg346.46µs upper1.292 remains a gap,198450B/op unchanged. All8support32/64/128/256 byte/word controls refresh:7pass/1gap/0uncertain; both64×32uncertainties pass/stop. No new image/grid/owned capacity; source-valid workspace198432B/result32768B/scratch0. Full6096checks eachOS,265internal/729public pernormal/scalar/AVX2off/AVX512off, SAMEactualpackage/threeexamples pass. Prior [pixel preparation](CACHED-WIDE-PIXEL-PREPARATION.md)/window/profile reports retain history; old loop attribution is superseded. |
| Text and drawing | [Shared endpoint storage](TEXT-ENDPOINT-STORAGE.md) removes redundant event insertion/growth/sorting. Current98.44/102.57µs vs labeled ImageSharp.Drawing50.45/51.14µs, uppers3.155/3.208: both gaps. Favorable13.74%/8.03% points have overlapping raw/normalized intervals; CPU gain remains inconclusive. Latin allocation67,368→59,000B/draw; admitted Latin48,544→42,336B, Arabic275,776→238,848B, fitting47,896→41,688B. Eight exact frames, retainedscratch0; physical/native peaks unmeasured.6064full/live eachOS plus normal/scalar and SAMEpackage/threeexamples pass. [Prepared pixels/order](TEXT-PREPARED-PIXELS-AND-ORDER.md) retains history. [Layout/fitting and RGBA16composition](TEXT-DRAWING-CURRENT.md) and [RGBA8composition](BYTE-COMPOSITE-EXACT-VECTOR.md) pass/stop. [Font loading](FONT-OWNED-STORAGE.md) remains31.86/FreeType13.42µs,upper2.396gap,176B/op; existing2slot/4MiB pool idle757076B, governed calls bypass. Independent codec/resize work continues. |


[Shared byte repair](BICUBIC-SHARED-BYTE-REPAIR.md) retains its scoped historical
gain; wide-overlap owns the current affected costs. Unchanged small-word evidence
remains with [shared byte/word columns](BICUBIC-SHARED-BYTE-WORD-COLUMNS.md).
Historical strided/word-column clocks remain in their reports.
[Tight byte enlargement](BYTE-BICUBIC-ENLARGEMENT-CURRENT-PACKAGE.md) remains
3114.50/zimg3041.43us upper1.030,116B/op,73usgap. Existing preparation is present;
larger current column/codec gaps take priority. Full283-path objective unchanged.

[Wide bicubic overlap](BICUBIC-WIDE-OVERLAP.md) qualifies the current immutable
build:6120full/live eachOS,285internal/729public pernormal/scalar/disabledAVX2/
AVX512, SAMEactualReleasepackage/allthreeexamples. Ten affected resize costs
refresh; two existinglarge managed-only modes have a scoped current-package
short screen. JPEG costs/452internal/375public scalar/ISA proof and other unchanged
codec/resize/text/ownership paths reuse source-valid evidence.
Unchanged [wider-cache geometry](CACHED-WIDE-PREPARED-GEOMETRY.md) retains its eight
costs; its controls are included in current285internal/729public resize qualification. Historical reports retain
their raw proof. The full127-row actual-package release refresh remains separate.
Publication stays postponed and all283paths remain required.

Earlier unselected [cached JPEG corrections](JPEG-CACHED-CORRECTION-EXPERIMENT.md),
[validated progression](JPEG-VALIDATED-PROGRESSION-EXPERIMENT.md),
[immutable JPEG geometry](JPEG-IMMUTABLE-MAP-EXPERIMENT.md),
[JPEG input preparation](JPEG-PREPARED-INPUT-EXPERIMENT.md),
[result row history](BICUBIC-RESULT-HISTORY-EXPERIMENT.md),
[packed prediction](VP8-PACKED-PREDICTION-EXPERIMENT.md),
[compact geometry](BICUBIC-COMPACT-GEOMETRY-EXPERIMENT.md),
[row/certificate preparation](BICUBIC-ROW-CERTIFICATE-EXPERIMENT.md),
[private parser publication](PNG-PRIVATE-PARSER-PUBLICATION-EXPERIMENT.md),
[fixed-half live signs](VP8-HALF-PROBABILITY-EXPERIMENT.md),
[VP8 windows](VP8-ROW-WINDOW-EXPERIMENT.md),
[frame phases](VP8-FRAME-PHASE-PREPARATION-EXPERIMENT.md),
[JPEG table views](JPEG-BORROWED-TABLE-VIEWS-EXPERIMENT.md),
[entropy windows](JPEG-SHARED-ENTROPY-WINDOW-EXPERIMENT.md),
[PNG token/code preparation](PNG-PREPARED-TOKEN-CODES-EXPERIMENT.md) and
[resize phases](BICUBIC-COLUMN-PHASE-EXPERIMENT.md) and
[paired JPEG rows](JPEG-PAIRED-INVERSE-ROWS-EXPERIMENT.md) and
[PNG prefix preparation](PNG-PREFIX-PREPARATION-EXPERIMENT.md) and
[live-word hash preparation](PNG-FASTEST-LIVE-WORD-STUDY.md) retain exact restoration,
adverse/inconclusive results and raw evidence separately. They establish no
current production gain or permanent family ban.
[Current JPEG attribution](JPEG-CURRENT-ENTROPY-ATTRIBUTION.md) refreshes the complete
RGB444 capture after paired AC changes, without builds or new costs. Earlier
whole-operation shares are historical. Live JLB546 attribution belongs to the
shared-window report; instrumented elapsed is not timing evidence.
[Current live progressive attribution](JPEG-LIVE-CURRENT-ATTRIBUTION.md) uses one
qualified JPR608 capture: buffered refinement dominates sampled locations, with
little nonzero-mask discovery. No production, displayed cost or scope change.
[Bicubic byte attribution](BICUBIC-BYTE-CURRENT-ATTRIBUTION.md) records current
source/JIT boundaries and a completed diagnostic capture with a failed post-run
I/O guard. Its locations motivate an overlapping-window model; they establish
no passing quiet measurement, phase time or current-cost change.

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

- JHB622 owns17 recorded JPEG encoding boundaries, including caller samples,
  normal/live, byte/word, forward/async and governed inputs. Only replay changes;
  generators, writer, tables and ownership stay exact. Prior corresponding encoder
  costs are superseded. Restart/fixed-table and tight recomputation do not call
  replay. Other codecs, decoding, editing and text evidence stays source-valid.

- LSF584 owns15PNGencoding costs after exact shared length classification.
  All complete packets remain exact; no parser/owner/checkpoint/field change.
  RNC579decode/header and othercodec/edit/text paths are source-unchanged.

- RNC579 owns43 ordinary/rolling PNG controls and the two-image shared-root case.
  Its syntax/routing audit retains six stored/counting/BCL providers plus PLK571's
  two separate materialized-live costs. No other codec/encoder/editing path changes.
  Historical stream/composition timings stay historical rather than current proof.

- VQP630 owns all six VP8 pixel/validation costs after frame quantizer preparation:
  Native8, Vp8Stream, Opaque8, SimpleOpaque8, OpaqueCancelable8 and ValidateVp8Alpha.
  Quantizer preparation supersedes VPS588 clocks; its model stride remains. Earlier alpha publication,
  typed borrowing and successful-only private cursor behavior remain; every
  checkpoint, public failure and owner cleanup is preserved.
- LPF476 owns six unchanged VP8L reader costs after final inverse/publication;
  its former VP8 costs are superseded. Encoders, other codecs, resizing and text
  retain their own source-valid evidence.
- VPR619 owns distortion effort2 after search-local rounded-border preparation.
  Methods0/1/2 Try4 changes; other shared prediction/RDO/decoder paths are exact
  source-unchanged. Eight other encode costs reuse VHB517. Earlier effort2 costs
  and affected unmeasured H boundaries are historical. No broader speedup claim.
- MBQ635/MBW637 owns Mixed8/TightMixed8/TightVertical8 after result-backed
  bands, quartet publication and identity preparation. The exact actual package
  DLL is measured; contemporary matching-artifact gains replace historical and
  cross-artifact inferences. MXC632 remains linked adverse/diagnostic history.
  Unrefreshed affected byte/pipeline H costs remain historical; other byte
  kernels and word methods are source-unchanged and retain valid evidence.
- RWB627 owns Mixed16/TightMixed16/TightVertical16 after shared result-backed
  bands. Contemporary originals expose the vertical regression; current upper.824
  still passes. Older RBC567/identity geometry clocks remain historical.
  RPA503 retains eight unchanged word controls after shared preparation;
  ICR614 owns Down16/Horizontal16/Vertical16/Cross16 and five byte costs,
  superseding RHW611: both vertical cases and wider byte pass,
  horizontal word remains uncertain; source-valid byte controls
  remain attributed separately. Unrefreshed allocator/filter/pipeline H costs stay
  historical. Lanczos owns eight refreshed one-shot/prepared controls; exact
  certificates, repair, plans/owners and portable fallbacks remain required.
- [TAP604](TEXT-CURRENT-RASTER-ATTRIBUTION.md) retains the pre-TIR605 drawing capture
  and strict-interior model. Actual C# and complete-operation qualification landed
  in TIR605; earlier profile shares are historical, not current phase costs.
- TED698 owns the two prepared drawing costs on the actual package DLL.
  Only consecutive endpoint insertion changes; font/layout/fitting/composition,
  codecs and resize evidence are source-valid. TRP671 retains preceding drawing
  work reuse and TIR605 the interior-coverage costs and worker dependencies.
  TCR587 retains the earlier clipping costs. TSC573's
  earlier allocation savings remain history; font/layout/fitting and compositing
  evidence are unaffected. Drawing CPU gains remain inconclusive.
- Other current owners and their precise source scopes are linked in the family
  table. Reuse confirmations only when relevant paths/boundaries remain unchanged.

Current priorities: equivalent preparation/reuse boundaries, substantial WebP
workspace lifetimes, shared resize execution and further codec work selected from
attribution. Keep passing cases stopped. Text/drawing proceeds independently;
pending references never block justified codec/resizing work. PLAN.md stays ignored.

[Current cached-wide attribution](CACHED-WIDE-CURRENT-PREPARATION-ATTRIBUTION.md)
locates the remaining horizontal tap work after pixel preparation. It supplies
a bounded safe-window hypothesis, with no new displayed cost or renderer dependency.
The subsequent [window/scheduling experiment](CACHED-WIDE-WINDOW-SCHEDULING-EXPERIMENT.md)
has inconclusive normalized two-window evidence and no useful four-window lead.
Both sources/modules are restored exactly; existing current costs remain valid.

[Pre-bank grayscale attribution](JPEG-GRAYSCALE-CURRENT-ATTRIBUTION.md) led to the
accepted replay stage. Its old replay locations/shares are historical; unchanged
preparation locations remain leads, not current isolated phase costs. It adds no
renderer dependency. RGBA→gray and callerGRAY remain distinct complete boundaries.

All earlier reports, raw evidence, release packages and source-specific
invalidation notes are retained through the linked history. They do not establish
current blanket qualification or permanently close algorithm families.

[Fixed-alignment VP8 arithmetic](VP8-HIGH-ALIGNED-ARITHMETIC-EXPERIMENT.md)
retains an unselected implementation and current live profile. Its two short
complete-operation screens show no useful lead. Exact qualified sources and
binaries are restored; current costs and package qualification remain unchanged.
It adds no renderer dependency or restriction on materially revised work.

[Cache-pair views](BICUBIC-CACHE-PAIR-VIEWS-EXPERIMENT.md) retain two unselected
resize representations. A small-control gain is outweighed by the confirmed
larger-control loss; the root revision has inconclusive short bounds and keeps
the targeted reload. Qualified sources/modules/package are restored exactly;
current costs and qualification remain unchanged. No renderer dependency.

[Zero-run state reuse](JPEG-ZERO-RUN-STATE-EXPERIMENT.md) retains two unselected
live progressive JPEG implementations with inconclusive screens and no useful
point/allocation gain. Qualified production and package are restored exactly;
three new terminal-zero-ordinal oracle tests remain and pass on restored code.
Current timing/production qualification is unchanged; no renderer dependency.
