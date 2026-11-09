# Material warm coverage

Current architectural phase, 2026-11-09. **283 material required paths** remain
in scope; the 126 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion needs current passing evidence for every named path, with scope stated.

[Current preparation/storage audit](ARCHITECTURAL-PREPARATION-AND-STORAGE.md)
reconciles one-shot/prepared boundaries and measures three WebP admission lifetimes.
The [Huffman workspace](WEBP-HUFFMAN-WORKSPACE.md) owns eight affected
lossless/alpha encode costs. [Rounded-border prediction](VP8-ROUNDED-BORDER-PREDICTION.md)
owns effort2; six unchanged lossy costs reuse [four-block histograms](VP8-FOUR-BLOCK-HISTOGRAM.md);
the alpha pair uses the current storage handoff.
[Completed JPEG output](JPEG-COMPLETED-OUTPUT.md)
owns twelve read costs, with seventeen unchanged paired-AC controls; [frame quantizers](VP8-FRAME-QUANTIZER-PREPARATION.md) own six VP8
pixel/validation costs. Six lossless reader controls retain source-valid evidence.
[Prepared Huffman replay](JPEG-PREPARED-HUFFMAN-BANK.md) owns17 affected JPEG
encoder boundaries; restart/tight recomputation retains unchanged evidence.
Historical experiments remain linked separately, with raw evidence retained.

## Current family status

| Required family/boundary | Current evidence and remaining work |
| --- | --- |
| Resident lossless WebP | [Huffman workspace](WEBP-HUFFMAN-WORKSPACE.md) reuses construction storage across alphabets/groups; selected codes stay independent. Six lossless controls4pass/2uncertain: photograph85959.95/libwebp82521.57µs upper1.080 and opaque upper1.013 remain uncertain; previous photo pass is superseded. Direct photo/diagram GC falls553184/37640B; aggregate5358980/3721041Bop. Measured peaks3470059/3251328B unchanged, retained0; no physical-peak or causalCPU claim. Exact packets/settings and adverse/raw/bracket evidence remain. |
| Resident VP8 encoding | [Rounded-border prediction](VP8-ROUNDED-BORDER-PREDICTION.md) shares exact interpolation preparation within distortion search and stores only the surviving winner. Complete effort2 saves112.73µs/4.27%raw/4.55%normalized; filtered/all-actual intervals separate. Current2525.56/native2511.68µs,upper1.013 is uncertain; GC406705B unchanged. Six other costs reuse source-valid [histogram evidence](VP8-FOUR-BLOCK-HISTOGRAM.md); the alpha pair uses [current Huffman workspace](WEBP-HUFFMAN-WORKSPACE.md): combined5pass/0gaps/4uncertain. Shared prediction, RDO and decoder source remain unchanged. Methods0/1/2 preserve exact packets, ordered ties, cancellation and ownership;32B rounded stack payload stays inside16KiB decision allowance. No new heap capacity or retention; physical/native peaks unmeasured. Passing cases stop; uncertainty is not a demonstrated gap. |
| Other WebP encoding boundaries | [Huffman workspace](WEBP-HUFFMAN-WORKSPACE.md) refreshes all eight affected costs on the actual-package DLL:4pass/4uncertain. Full-depth normalization, awaited nonseekable output and independent ownership stay timed. Transparent-alpha directGC falls668512B; peak3656309B/retained0 unchanged. Both alpha cases are uncertain (uppers1.065/1.002), superseding earlier alpha16 pass. No inferredCPU gain or demonstrated gap from uncertainty. Normal/scalar426internal+239public and6026full/live eachOS, SAMEactualpackage/threeexamples pass. |
| PNG decoding/coding validation | [Ordinary/slab publication](PNG-NORMAL-CURSOR-PUBLICATION.md) removes three exception-only publication regions from abandoned private readers, preserving exact completed cursors/public errors/partial stores/lookahead/polls/ownership. Tight/default RGBA16, RGB16 and tight-live RGBA16 save192/125/216/217us; all four former gaps pass. Separate shared-root pair saves260us and passes upper0.920, retaining1,638,214B/op, admitted1,766,641B and retainedpair1,062,936B. [All51controls](PNG-CURRENT-FRAMING-COSTS.md):49pass/1gap/1uncertain;43 refreshed,8 unchanged providers reused, no allocation change. Separate materialized-live word remains2919.94/native2894.87us upper1.012 gap; livebyte upper1.004 uncertain. Stop passing cases; resolve uncertainty before tuning. Older stream/composition costs remain historical. |
| PNG encoding | [Exact length classification](PNG-LENGTH-CLASSIFICATION.md) replaces repeated range search with the exact integer-logarithm formula. All15 complete one-shot costs refresh:13pass/0gaps/2uncertain. WordSmallest now passes/stops; ByteFastest upper1.017 and Gray8 upper1.047 remain uncertain, requiring confidence before more tuning. Word raw180.09us/1.00% intervals separate both views, normalized intervals overlap; byte84.29us point gain has allactual uncertainty. No robust normalized/blanket gain claim. All packets remain exact,384KiB admission/retained0scratch and primaryGC820916/2437660Bop unchanged. [Streaming census](PNG-STREAMING-TOKEN-FREQUENCIES.md) and prior preparation remain history. |
| WebP image reading | [Frame-private quantizers](VP8-FRAME-QUANTIZER-PREPARATION.md) prepare6values/segment once,96B inside existing8KiB allowance. Live saves45.36us/.92%raw with separated filtered/allactual raw confidence; .86%normalized is not robust across allactuals. Resident alpha saves110.85us/1.83%raw/1.82%normalized with separated intervals versus a contemporary6072.55us original; current5961.70/native5968.47us upper1.003 uncertain, not a new demonstrated gap. Live4881.47/native4792.22us upper1.028gap; other opaque uppers1.009/1.011 remain gaps. Stream/validation pass; six costs2pass/3gaps/1uncertain. AllGC unchanged (opaque229344Bop), existing1536Bmodel and independently owned531468Bframes; newheap/retention0, physical/native peaks unmeasured. Exact pixels/token/context/transform/filter/cancellation and shared admission remain. Prior model-row preparation stays history. [VP8L final publication](VP8L-FINAL-PUBLICATION.md) retains six unchanged lossless costs:2pass/4gaps,155.66us gain and19.25us palette regression. |
| JPEG decoding | [Completed output](JPEG-COMPLETED-OUTPUT.md) reuses existing overwrite storage after exact full-plane writes. Progressivegray saves98.02us/5.17%raw/5.11%normalized with separatedfiltered/allactual confidence; GC683176→151432B. Current1797.74/native1711.60us upper1.054 remainsgap. Twelve costs refresh/seventeen unchanged reuse:29now16pass/12gap/1uncertain. Ownedgray nowpasses602.47/native681.27us upper.891; stream420restart upper1.002 isuncertain, resolvebeforetuning. Fourgoverned peaks remain699834/756276/752177/721969B; result531468B until disposal. Default idlepixels0→531468B insideexisting2slot/4MiBpool, governedcalls bypass; no newowner/pool/cache. Gray idlecoefficients549120B unchanged. [Paired AC](JPEG-PAIRED-AC-WINDOW.md) retains RGB1530.34/native1450.52us upper1.065 and its574069Badmittedpeak. Liveprogressive444 remainsanindependent201.57usgap. [Paired component preparation](JPEG-PAIRED-COMPONENT-PREPARATION.md) has no useful complete gain in three exact short stages; qualified source/modules/package restored, no passing/parity claim. |
| JPEG encoding | [Prepared Huffman replay](JPEG-PREPARED-HUFFMAN-BANK.md) replaces repeated table/code/length traversal with a4KiB stack bank inside the existing16KiB owner. Complete RGBA→gray saves151.45µs/9.43%raw/10.09%normalized; filtered/allactual intervals separate. Current1455.31/labeledSIMDlibjpeg-turbo1409.83µs upper1.042 remains~45µs gap. All17 affected boundaries refresh:15pass/1gap/1uncertain. CallerGRAY now passes upper.994; callerRGB420 upper1.009 is uncertain. PrimaryGC155436B/op and governedRGBA16 peak1704856B unchanged; no new heap/retention/cache. Existing coefficient pools/governed bypass remain; physical/native peaks unmeasured. Exact packets and strict metadata exceptions remain. Passing cases stop; restart/tight recomputation reuses source-valid evidence. |
| Bicubic columns | [Shared original-order byte repair](BICUBIC-SHARED-BYTE-REPAIR.md) removes ordinary whole-frame replay. One existing large managed-only compact control643.41→30.78ms saves612.63ms/95.22%,20.90×; exact normal/scalar/prior frames,4KiB and owners pass. Default25.87ms stays faster, so dispatch unchanged. Frozen wide-byte476.04/zimg348.99us upper1.378 regresses24.93us/5.53%raw; adverse confirmation retained. Small-byte542.51/zimg417.07us upper1.321 and wide-word453.66/zimg403.32us upper1.134 remain gaps,113B/op. Prior small-byte/current word native drift preclude causal companion gains. Current6006full/live eachOS plus scalar/ISA/SAMEpackage pass. [Small-word590.84/zimg572.81us upper1.033](BICUBIC-SHARED-BYTE-WORD-COLUMNS.md) and [packed485.40/zimg370.61us upper1.314](PACKED-BICUBIC-DECODED-PREFIX.md) retain unchanged-path evidence. Earlier wider costs remain linked history. |
| Bicubic streaming | [Shared streaming blocks](BICUBIC-SHARED-STREAMING.md) use two schedulers/four exact kernels within4KiB, saving66.62/51.17µs on byte/word8×17. Eight current controls: six pass/stop; RGBA8 8×17/4×9 retain gaps (uppers1.127/1.269). Allocations unchanged. |
| Lanczos3 | [Certified word rows](LANCZOS-WORD-CERTIFIED-ROWS.md) reuse existing UInt32 products, replacing integer filtering/conversion in the accelerated producer. Prepared word reduction576.41→551.39µs saves25.02µs/4.34%raw/4.99%normalized with separated filtered/allactual raw+normalized confidence. Word enlargement point+14.87µs/.37% has overlapping intervals; no uniform gain. All eight actual-package-DLL costs current:5pass/3prepared gaps (byte reductionupper1.186,word reduction1.084,word enlargement1.043). Current original-byte comparison resolves historical clock movement without a causal gain/regression/equivalence claim. Capacity/retention unchanged; primaryGC139/4265332Bop. Exact Q30 pixels, strict Guard/certificate/repair,6017full/live eachOS, normal/scalar/AVX512off and SAMEpackage/threeexamples pass. [Earlier paired certificates](LANCZOS-PAIRED-CERTIFICATE-PREPARATION.md) retain history. |
| Complete bilinear enlargement | [Current boundary audit](bilinear-current-boundary-evidence.json) reuses the qualified immutable build without production changes or rebuilds. Existing RGBA8 Up8 measures2357.04µs/native2155.92µs, ratio1.093/upper1.134/lower.971: uncertain.102891B/op includes fresh~103KBhorizontal preparation and independent output ownership; the unchanged4MiBdefault pool reuses output backing. Older2914.62µs/2235512Bcost is superseded, not paired causal gain evidence. Native graph/context preparation remains retained and labeled. Stop tuning this uncertain case while larger demonstrated gaps remain. |
| Complete bicubic enlargement and small-support word publication | [Shared word preparation](BICUBIC-SHARED-WORD-PREPARATION.md) retains divisor/margin setup across pair/quartet/cached writers. [Direct identity cache](BICUBIC-IDENTITY-CACHE-ROWS.md) refreshes four cached word costs; eleven unchanged paths, including [identity geometry](BICUBIC-IDENTITY-WORD-GEOMETRY.md), are reused:15controls,9pass/5gaps/1uncertain. Full/bounded enlargement and default horizontal/vertical enlargement pass; stop them. Existing division, certified rounding/repair and rational pixels remain. DefaultRGBA8 [unchanged-build confirmation](bicubic-byte-enlargement-current-evidence.json) measures3,108.21/native3,037.83µs,upper1.026/116Bop,70.38µs gap; no causal historical gain. Current cached workspace reduction is stated separately below. [Publication pipeline](BICUBIC-WORD-PUBLICATION-PIPELINE.md) retains earlier scoped history. |
| Mixed-axis RGBA8 bicubic | [Result-backed byte bands](BICUBIC-BYTE-RESULT-BANDS.md) save149.15us/13.20%raw/13.02%normalized on matching contemporary package artifacts,1130.05→980.90us, separated filtered/allactual intervals. Current default/tight960.04/980.90us remain gaps (uppers1.116/1.142); vertical503.50/zimg587.18us upper.862 passes/stops. GC114/114/113Bop, no added owned capacity/retained scratch;28136B borrows admitted output before geometry and retires before suffix. One numeric range source,4KiB root reservation, exact original pixels/certificates/replay and ownership remain. Earlier1319.86us and cross-artifact costs are not causal baselines; discrepancy retained/unexplained. Both OSes5964full/live, scalar/ISA, SAME actual package/byte-band consumers/allthreeexamples pass. Live progressive444~202us remains open after the unselected paired study; current shared tight-byte reduction542.51/zimg417.07us upper1.321 retains a125.45usgap after shared byte repair. Earlier adverse raw/inconclusive normalized evidence stays linked history; current wide-byte local-repair code generation is the next scoped attribution. |
| Mixed-axis RGBA16 bicubic | [Result-backed bands](BICUBIC-RESULT-BACKED-BANDS.md) save86.35/182.46us versus contemporary originals, with separated filtered/allactual raw/normalized intervals. Default841.53/native847.03us upper0.997 passes/stops; tight862.21/native844.22us upper1.025 remains~18usgap. One shared exact arithmetic body borrows28136B of admitted unpublished output, retires it before suffix stores, keeps4/8KiB root reservations and no extra heap/retention. GC114/114Bop. Shared vertical regresses61.98us/14.62% against a contemporary original, but remains passing486.02/native593.16us upper0.824,113Bop; tradeoff disclosed.15word controls now10pass/4gap/1uncertain,12unchanged paths reused. Exact pixels/certificates/repair, terminal refusal and bounded cancellation remain. Mixed/vertical result owns1059864/529416B until disposal; physical/native peaks unmeasured. Prior band/geometry costs remain linked history. |
| Cached bicubic preparation/execution | [Direct identity cache](BICUBIC-IDENTITY-CACHE-ROWS.md) removes the separate decoded row/copy, sharing exact conversion with entry-selected polling. Vertical byte636.49→530.97us saves105.52us/16.58%raw/16.50%normalized; word666.59→511.76us saves154.84us/23.23%raw/21.26%normalized. Both confidence views separate; uppers0.908/0.904 pass, superseding the earlier word regression. Nine refreshed controls:7pass/1gap/1uncertain. Byte reduction remains485.10/native416.46us gap; horizontal word~2us difference is uncertain. Workspace383852→367436B; totalGC367675/367595Bop, scratch0/result264708/529416B retained until disposal. Exact repair4KiB remains separately admitted; no new owner/pool/cache/API. Native graph/input/scratch remain prepared, managed one-shot setup timed. Four displayed rows refresh; passing cases stop. |
| Cached wider bicubic | [Shared pixel preparation](CACHED-WIDE-PIXEL-PREPARATION.md) saves34.35us/6.89%raw/6.84%normalized: Wide8 now463.92/zimg346.90us upper1.344,198450B/op,117us gap. Eight existing support32/64/128/256 controls now5pass/1gap/2uncertain on actual package406f7d1e; default64×32byte/word remain uncertain. Wide16 and default32×16/16×8bothdepths pass/stop. Primary native is stable; companion native means move −8–10%, so larger raw differences are not causal gain claims. Exact pixels, full W/L/ISA/SAMEpackage qualification pass. No new images/settings; earlier family and WRI epochs remain linked history. |
| Text and drawing | [Prepared pixels and edge order](TEXT-PREPARED-PIXELS-AND-ORDER.md) removes a redundant Edge sort and shares exact blend weights. Current111.21/97.71µs vs labeled ImageSharp.Drawing48.06/45.93µs, uppers3.275/3.348: both gaps. Filtered point gains11.61%/31.39%, allactual12.82%/25.55%; robust CPUgain remains inconclusive. Actual-package DLL, eight exact frames,67,368B/draw,48,544B admitted Latin workspace and retainedscratch0; physical/native peaks unmeasured.6011full/live eachOS plus normal/scalar and SAMEpackage/threeexamples pass. [Interior coverage](TEXT-INTERIOR-COVERAGE.md) retains history. [Layout/fitting and RGBA16composition](TEXT-DRAWING-CURRENT.md) and [RGBA8composition](BYTE-COMPOSITE-EXACT-VECTOR.md) pass/stop. [Font loading](FONT-OWNED-STORAGE.md) remains31.86/FreeType13.42µs,upper2.396gap,176B/op; existing2slot/4MiB pool idle757076B, governed calls bypass. Independent codec/resize work continues. |


[Shared byte repair](BICUBIC-SHARED-BYTE-REPAIR.md) owns three current tight costs
and6006full/live/SAMEpackage qualification. Unchanged small-word evidence remains
with [shared byte/word columns](BICUBIC-SHARED-BYTE-WORD-COLUMNS.md). Historical
strided/word-column clocks are retained in their reports, not renderer inputs.
[Tight byte enlargement](BYTE-BICUBIC-ENLARGEMENT-CURRENT-PACKAGE.md) remains
3114.50/zimg3041.43us upper1.030,116B/op,73usgap. Existing preparation is present;
larger current column/codec gaps take priority. Full283-path objective unchanged.

The latest [Huffman replay stage](JPEG-PREPARED-HUFFMAN-BANK.md) refreshes17costs
and seven displayed rows, preserving126comparisons/283requiredpaths. Windows/Linux
each pass5,943full/live,424internal/375public JPEG checks per normal/scalar and
disabledAVX2/AVX512 mode, andthe SAMEactualReleasepackage/threeexamples. ActualC#
independent packing checks114688records; the frozen28672block/946249record corpus,
complete packets, cancellation/admission/failure prefixes and owners remain exact.
One main perOS is reused; a corrected Windows test setup needs a second embedded
version. Completed async receipts are recovered after schema-only reporting
corrections, without replaying measurements. The [rounded-border](VP8-ROUNDED-BORDER-PREDICTION.md)
and [identity-cache](BICUBIC-IDENTITY-CACHE-ROWS.md) stages remain valid for WebP/resizing.
Other families retain source-valid evidence.
This is scoped development qualification; fullparity and the full126row actual-
package release refresh remain open.

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
- TRP671 owns the two prepared drawing costs on the actual package DLL.
  TIR605 retains earlier interior-coverage costs and worker dependencies.
  TCR587 retains the earlier clipping costs. TSC573's
  earlier allocation savings remain history; font/layout/fitting and compositing
  evidence are unaffected. Drawing CPU gains remain inconclusive.
- Other current owners and their precise source scopes are linked in the family
  table. Reuse confirmations only when relevant paths/boundaries remain unchanged.

Current priorities: equivalent preparation/reuse boundaries, substantial WebP
workspace lifetimes, shared resize execution and further codec work selected from
attribution. Keep passing cases stopped. Text/drawing proceeds independently;
pending references never block justified codec/resizing work. PLAN.md stays ignored.

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
