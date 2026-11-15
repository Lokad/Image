# Material warm coverage

Current architectural phase, 2026-11-15. **289 material required paths** remain
in scope; the 147 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion requires valid passing evidence for every named path. Missing evidence,
gaps and uncertainty stay open. No image, shape, depth, filter or quality grid is added.

Current costs use the identified immutable producers in the family reports below.
[Raster copy scheduling](RASTER-COPY-SCHEDULING.md) refreshes six affected
copy/composition views and closes the normal-word gap and word-clone uncertainty.
Fourteen unchanged edits reuse current [flip confidence](OWNED-EDIT-CURRENT-CONFIDENCE.md)
and [copy-result](COPY-RESULT-ADMISSION.md) evidence. [Fixed-position VP8 arithmetic](VP8-FIXED-CODE.md)
retains six passing decode/validation views. [Streaming column reuse](STREAMING-COLUMN-REUSE.md)
closes bounded16×8word uncertainty. Across the **191 existing image views: 188 pass, one gap, two uncertain**.
These include approved alternative boundaries, not 191 new material paths.
In-place drawing and inspection retain separate gaps; whole-library parity is open.

Each OS passes **6,721 full/live checks**, focused265internal/161public normal/scalar font/text checks
and the **same actual Release-package consumer / three examples**. Existing resize
normal/scalar/AVX512-disabled, arithmetic/ISA and codec/text evidence is reused. Current complete costs supersede
historical stage clocks; no pipeline gain follows from summing them. Allocation,
logical admitted capacity, idle retention and unmeasured physical peaks stay separate.

## Current family status

| Required family/boundary | Current evidence and next action |
| --- | --- |
| PNG decoding | [Current owner](RELEASED-IMAGE-STORAGE.md): all 47 existing decode controls pass. Four coding-only validators and the caller-root retained-output control reuse unchanged paths. Stop passes. |
| Stream decode/composition | [Current PNG16 stream](PNG-STREAM-WORD-CURRENT-CONFIDENCE.md) passes/stops:3,106.21/FFmpeg3,349.15µs,upper.934,8,664B/op. [Current complete decode/orient/crop/fit](RASTER-COPY-SCHEDULING.md) passes/stops:byte1,236.76/1,478.65µs,upper.845,169,454.8B/op; word2,867.64/2,934.06µs,upper.982,338,159.2B/op. Original uncertain/adverse and producer-position controls remain; no causal pipeline gain is claimed. [VP8/alpha stream](VP8-FIXED-CODE.md) retains its pass:5,701.59/5,982.33µs,upper.956,240,496B/op. All seven synchronous decode/composition views,Identify and unchanged async PNG8 pass. Reuse unchanged async inspection/validation evidence. No phase sums. |
| JPEG reading | All 24 RGBA/fresh and four caller-buffer views pass and stop. [Horizontal preparation](JPEG-HORIZONTAL-PREPARATION.md) saves 4.03% / 16.92 µs in controlled complete-decode pairs, unchanged 20,944 B/op. Current horizontal410.41/FFmpeg418.11µs upper.988; fresh372.15/753.22µs upper.498; sparse414.93/433.23µs upper.970. RGB444's adverse standalone point is uncertain; one favorable controlled pair resolves the suspected contradiction without a standalone repeat. Two coding-only validators reuse unchanged proof. Earlier compact-output, row-product and raw-row studies retain adversity/inconclusive results separately. |
| JPEG encoding | All 18 declared comparisons pass/stop. [Direct block emission](JPEG-DIRECT-BLOCK-EMISSION.md) closes restart33: 1,339.28 / FFmpeg 1,609.15 µs, upper .841, 151,180 B/op, exact 58,010 / 58,012 B packets with the strict 2 B attribution exception. Separate complete paired steps save 19.21% and 17.00%; no summed gain. The 17 [adaptive replay controls](JPEG-LOCAL-ENTROPY-REPLAY.md) reuse unchanged paths/artifact identities. SIMD libjpeg-turbo grayscale stays explicitly labeled. Native contexts/table preparation remain retained; managed preparation stays timed. No added storage or broader content/settings claim. |
| PNG encoding | All 17 equivalent fresh-state controls pass/stop. [Longer Gray8 integration](PNG-GRAY-CURRENT-PATH.md) resolves uncertainty:1,964.07/FFmpeg1,978.51µs,upper.999681,546523B/op,exact72586B. No production speedup; original samples retained. Sixteen passing controls reuse unchanged [compressed heap](PNG-HUFFMAN-HEAP-ENTRIES.md) and [stored](PNG-STORED-WORD-NORMALIZATION.md) evidence. Stored-word naturalGC64 passes/stops; postGC4 retained-word uncertainty remains a distinct diagnostic. |
| Lossless WebP/alpha encoding | All eight pass/stop. Six unchanged lossless-only paths retain [workspace](WEBP-HUFFMAN-WORKSPACE.md),[opaque](WEBP-OPAQUE-CURRENT-CONFIDENCE.md) and [photograph](WEBP-LOSSLESS-PHOTOGRAPH-CONFIDENCE.md) evidence. Current [VP8+alpha](VP8-VARIABLE-PRICE-PREPARATION.md) byte/word52.704/52.346ms,upper.998/.988,6,241,792B/op,exact98590B. Candidate ownership,admitted workspace and exact packets remain. |
| Lossy WebP encoding | All nine declared controls pass/stop on the [factored variable-price preparation](VP8-VARIABLE-PRICE-PREPARATION.md) DLL/actual package. Former photograph gap closes:9,385.67/FFmpeg9,729.91µs upper.971,809080B/op,exact17688B. Controlled independent photograph gain1.623%/154.41µs; alpha16.433%/228.60µs,not historical-clock subtraction. Trellis longer integration resolves uncertainty at upper.935,keeping every original sample. Exact table/packet/owner gates remain; no new capacity/retention. Source-specific earlier attribution and rejected layouts remain history. |
| WebP reading | All twelve declared comparisons pass/stop. [Fixed-position arithmetic](VP8-FIXED-CODE.md) closes the live-token gap:4,767.34/FFmpeg4,784.37µs,interval.991113–.999450,230880B/op. Independent paired live gain41.53µs/.86% is scoped; all six affected actual-reference costs refresh before qualification. Five other standalone points are higher than the preceding round but retain passes; no uniform speedup/historical-clock gain or causal regression claim. Six lossless reads reuse unchanged proof. Exact pixels/cursors,all decision polls,input-read addresses,admission and output ownership remain. Stop this family; earlier mode/window profiles and rejected bridges stay history. |
| Bicubic/bilinear resizing | Current73views:72pass,one uncertainty. [Streaming column reuse](STREAMING-COLUMN-REUSE.md) closes bounded16×8word:336.93/zimg495.11µs,upper.685485,112B/op. All six affected streaming controls pass/stop on the SAME qualified package; exact ordered sums,4096B workspace admission and ownership remain. The adverse initial implementation and earlier uncertain producer remain history. [Default Wide8](BICUBIC-WIDE-CURRENT-CONFIDENCE.md) and [mixed/bounded-wide](BICUBIC-CURRENT-ADJACENT-CONFIDENCE.md) passing paths are unchanged. [Bilinear Up16](BILINEAR-WORD-CURRENT-CONFIDENCE.md) remains1,805.35/FFmpeg1,924.57µs,interval.879672–1.000415,4,498,558B/op uncertain after its longer integration. No identical favorable retries or numerical-policy change. Asymmetric preparation stays explicit; all eight equivalent one-shot/prepared controls pass/stop. |
| Lanczos3 | All eight declared one-shot/prepared byte/word comparisons pass and stop. [Current prepared word enlargement](LANCZOS-ENLARGEMENT-CURRENT-CONFIDENCE.md):4117.26/zimg4212.39µs, upper.990,4264760B/op and333396B retained plan. [Reduction product reuse](LANCZOS-WORD-PRODUCT-REUSE.md) retains its6.18% normalized gain and four passing reduction views. Exact Q30/rational arithmetic, float-native bounds and equivalent prepared/one-shot ownership stay explicit; no gain is inferred from the confidence update. |
| Copy/turn transforms and drawing | [Raster copy scheduling](RASTER-COPY-SCHEDULING.md):20views/18pass,one gap,one uncertainty. Normal/clone RGBA16 now15.481/FFmpeg16.648µs,upper.936 and15.455/16.740µs,upper.926 pass/stop; scoped normal-word gains about9–12% across producer assignments. Reusing the16KiBBCL loop adds no kernel/storage; all18basic edits retain56B/op. [Current flip confidence](OWNED-EDIT-CURRENT-CONFIDENCE.md) retains byte vertical gap8.614/8.507µs,upper1.022566 and word-flip uncertainty upper1.005451. Exact pixels,cancellation,caller root,independent output/capacity remain;6696full/live perOS and SAMEactualpackage/all3examples pass. [Borrowed drawing](BORROWED-DRAWING-VALIDATION.md) retains0B/op:four pass,three small copy gaps. The [current-package overlap control](VECTOR-EXTENT-COPY-SCREEN.md) is3.850/3.776µs,upper1.024484; it resolves uncertainty without a production change. Shared extent copying is rejected after conflicting standalone/pair evidence; qualified source/package restored. Word compositing uses labeled ImageSharp3.1.12. |
| Font/text public integration | [Prepared immutable edges](TEXT-PREPARED-EDGE-INVARIANTS.md) save5.39µs/12.87%raw and13.05%paired-normalized on contemporary prepared RGBA16; wider bracket-normalized gain inconclusive. Existing Arabic managed-DLL check saves51.52µs/10.46%,GC441696→583776B; no external parity claim. [Longer reference integration](TEXT-DRAW-CURRENT-CONFIDENCE.md) confirms ordinary8/16 scoped gaps at54.41/44.72µs and58.12/47.48µs; prepared8/16 remain uncertain. No production change or CPU-gain claim. Complete one-shot1167.24/6182.75µs upper.214 and eight-draw1653.58/6783.43µs upper.269 pass/stop,including preparation/owned canvases/disposal. Latin GC59304→75944Bordinary/56Bprepared,+24Badapter; retained40408→48984B and preppeak42488→52920B. Fit retained48480B; actual governed peak is recorded in the current evidence. Edge40→56B is admitted before allocation/replacement and through retention. [FreeType loading](FONT-HEADER-VALIDATION.md) retains scoped22.16/13.22µs gap,176B and eager/lazy differences. Exact full-word/layout/visual/owner gates remain; no overall slowdown or summed-stage estimate. |
| Inspection and complete metadata | [Direct result publication](PROBE-RESULT-PUBLICATION.md) removes a transient descriptor: four probes568B/op (40B lower); complete JPEG sync/async pairs confirm3.67/5.18%scoped gains. Ten current standalone costs use the SAME timed/packageDLL. JPEG/WebP probes remain gaps; PNGdimensions .123µs upper1.026 and residentPNGIdentify13.083µs upper1.005245 are uncertain. Complete PNGheaders .296µs upper.534, JPEG/WebP residentIdentify and both PNGstreamIdentify pass/stop. Labeled original FFmpeg adapters, exact metadata/owners,512B initial/8448B bulk admission and zero retention remain. Shared factory rules stay single; initial dictionary-dispatch screen, two zero-actual stale-harness rejects and receipt recovery remain reproducible history. Windows/Linux6712full/live,10internal/184public normal/scalar and SAMEactualpackage/all3examples pass. Unchanged codec/edit/resize/text kernels reuse evidence; no full-release refresh or new workload grid. |

Earlier [copy preparation](VERTICAL-COPY-PREPARATION-SCREEN.md),
[block-value copying](VECTOR-BLOCK-VALUE-COPY-SCREEN.md) and
[compact VP8L reuse](WEBP-COMPACT-PAIR-REUSE-TRIALS.md) remain unaccepted historical
studies with raw evidence retained. Current producer-position controls do not
retroactively qualify an older candidate. Require concrete attribution or a
materially revised design before another trial; there is no permanent family ban.
[Bilinear word row-storage trials](BILINEAR-WORD-ROW-STORAGE-TRIALS.md) also stop
without a retained production change: reduced capacity did not establish useful
elapsed gains. Qualified sources, producer bindings and package are restored;
one public-operation ownership/refusal/cancellation test is retained. All adverse
and inconclusive observations remain, outside current-cost renderer dependencies.
[Bilinear reciprocal sharing](BILINEAR-QUOTIENT-NORMALIZATION-TRIALS.md) likewise
retains no production change: the first confirmation is adverse and the independent-
pair revision has no promising screen. Qualified sources, builds and package are
restored; current Up16 remains uncertain, without another favorable timing retry.
Copy-cost rendering selects the 20 current records once; superseded gain and
rejection checks remain in their reports, outside renderer dependencies.

[Prepared-text attribution](TEXT-PREPARED-CURRENT-ATTRIBUTION.md) retains
the read-only profile and the insufficient initial capture. The [prepared-edge owner](TEXT-PREPARED-EDGE-INVARIANTS.md) now supplies current drawing
and complete integration costs. Prior captures remain history; historical stage
times are never added.

[Current ordinary-draw attribution](TEXT-ORDINARY-CURRENT-ATTRIBUTION.md) locates
event preparation as a bounded lead on the preceding endpoint DLL. It changes
no cost or qualification claim and adds no renderer dependency.

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
