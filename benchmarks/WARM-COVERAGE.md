# Material warm coverage

Current architectural phase, 2026-11-16. **289 material required paths** remain
in scope; the 151 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion requires valid passing evidence for every named path. Missing evidence,
gaps and uncertainty stay open. No image, shape, depth, filter or quality grid is added.

Current costs use the identified immutable producers in the family reports below.
[Raster copy scheduling](RASTER-COPY-SCHEDULING.md) refreshes six affected
copy/composition views and closes the normal-word gap and word-clone uncertainty.
Current flip costs are in [owned-flip confidence](OWNED-FLIP-CURRENT-CONFIDENCE.md);
unchanged edits retain identified [copy-result](COPY-RESULT-ADMISSION.md) evidence. [Fixed-position VP8 arithmetic](VP8-FIXED-CODE.md)
retains six passing decode/validation views. [Streaming column reuse](STREAMING-COLUMN-REUSE.md)
closes bounded16×8word uncertainty. Across the **192 existing image views: 189 pass, zero gaps, three uncertain**.
These include approved alternative boundaries, not 191 new material paths.
In-place drawing and inspection retain separate gaps; whole-library parity is open.

Each OS passes **6,743 full/live checks**, focused10internal/154public inspection
checks in normal/scalar modes and the **same actual Release-package consumer /
three examples**. Lanczos passes163internal/15public normal/AVX512-disabled/scalar, including the
three named resize-storage suites. Font/text passes268internal/163public normal/AVX-disabled/scalar;
PNG paths retain84internal/247public normal/AVX512-off/scalar checks; unchanged
copy paths retain25internal/190public normal/scalar checks. Existing resize
normal/scalar/AVX512-disabled, arithmetic/ISA and codec/text evidence is reused. Current complete costs supersede
historical stage clocks; no pipeline gain follows from summing them. Allocation,
logical admitted capacity, idle retention and unmeasured physical peaks stay separate.

## Current family status

| Required family/boundary | Current evidence and next action |
| --- | --- |
| PNG decoding | [Wide scan CRC](WIDE-PNG-SCAN-CRC.md) owns all 47 decode controls and four coding validators; all pass/stop on the same timed/package DLL. The caller-root two-retained-result decode also passes, with unchanged peak/lifetimes. Exact samples, cancellation positions and admission remain. Earlier storage/Adler reports retain history. |
| Stream decode/composition | [Wide scan CRC](WIDE-PNG-SCAN-CRC.md) retains five original PNG views;word composition with native retained decoder/graph stays uncertain at2871.46/2943.04µs,interval.928688–1.021958,338160B/op. [Equivalent one-shot PNG16 composition](PNG-COMPOSITION-ONE-SHOT.md) passes/stops at2856.94/FFmpeg3301.31µs,upper.871,338156–338160B/op:both sides include necessary fresh preparation and independent output/disposal. Exact full-word/input/stream/owner gates and immutable actual package remain;zero production/managed/package builds or broad repeats,one isolated native adapter build. A single native64-call paired diagnostic measures2942.75retained/3292.07freshµs,extra349.33µs including preparation/disposal and lost internal reuse,not pure phase time or managed gain. All original/short/independent actuals remain;old uncertainty is not relabeled. Other PNG/JPEG/WebP/VP8 stream/async passes are source-valid and stop. |
| JPEG reading | All 24 RGBA/fresh and four caller-buffer views pass and stop. [Horizontal preparation](JPEG-HORIZONTAL-PREPARATION.md) saves 4.03% / 16.92 µs in controlled complete-decode pairs, unchanged 20,944 B/op. Current horizontal410.41/FFmpeg418.11µs upper.988; fresh372.15/753.22µs upper.498; sparse414.93/433.23µs upper.970. RGB444's adverse standalone point is uncertain; one favorable controlled pair resolves the suspected contradiction without a standalone repeat. Two coding-only validators reuse unchanged proof. Earlier compact-output, row-product and raw-row studies retain adversity/inconclusive results separately. |
| JPEG encoding | All 18 declared comparisons pass/stop. [Direct block emission](JPEG-DIRECT-BLOCK-EMISSION.md) closes restart33: 1,339.28 / FFmpeg 1,609.15 µs, upper .841, 151,180 B/op, exact 58,010 / 58,012 B packets with the strict 2 B attribution exception. Separate complete paired steps save 19.21% and 17.00%; no summed gain. The 17 [adaptive replay controls](JPEG-LOCAL-ENTROPY-REPLAY.md) reuse unchanged paths/artifact identities. SIMD libjpeg-turbo grayscale stays explicitly labeled. Native contexts/table preparation remain retained; managed preparation stays timed. No added storage or broader content/settings claim. |
| PNG encoding | All 17 equivalent fresh-state controls pass/stop. [Longer Gray8 integration](PNG-GRAY-CURRENT-PATH.md) resolves uncertainty:1,964.07/FFmpeg1,978.51µs,upper.999681,546523B/op,exact72586B. No production speedup; original samples retained. Sixteen passing controls reuse unchanged [compressed heap](PNG-HUFFMAN-HEAP-ENTRIES.md) and [stored](PNG-STORED-WORD-NORMALIZATION.md) evidence. Stored-word naturalGC64 passes/stops; postGC4 retained-word uncertainty remains a distinct diagnostic. |
| Lossless WebP/alpha encoding | All eight pass/stop. Six unchanged lossless-only paths retain [workspace](WEBP-HUFFMAN-WORKSPACE.md),[opaque](WEBP-OPAQUE-CURRENT-CONFIDENCE.md) and [photograph](WEBP-LOSSLESS-PHOTOGRAPH-CONFIDENCE.md) evidence. Current [VP8+alpha](VP8-VARIABLE-PRICE-PREPARATION.md) byte/word52.704/52.346ms,upper.998/.988,6,241,792B/op,exact98590B. Candidate ownership,admitted workspace and exact packets remain. |
| Lossy WebP encoding | All nine declared controls pass/stop on the [factored variable-price preparation](VP8-VARIABLE-PRICE-PREPARATION.md) DLL/actual package. Former photograph gap closes:9,385.67/FFmpeg9,729.91µs upper.971,809080B/op,exact17688B. Controlled independent photograph gain1.623%/154.41µs; alpha16.433%/228.60µs,not historical-clock subtraction. Trellis longer integration resolves uncertainty at upper.935,keeping every original sample. Exact table/packet/owner gates remain; no new capacity/retention. Source-specific earlier attribution and rejected layouts remain history. |
| WebP reading | All twelve declared comparisons pass/stop. [Fixed-position arithmetic](VP8-FIXED-CODE.md) closes the live-token gap:4,767.34/FFmpeg4,784.37µs,interval.991113–.999450,230880B/op. Independent paired live gain41.53µs/.86% is scoped; all six affected actual-reference costs refresh before qualification. Five other standalone points are higher than the preceding round but retain passes; no uniform speedup/historical-clock gain or causal regression claim. Six lossless reads reuse unchanged proof. Exact pixels/cursors,all decision polls,input-read addresses,admission and output ownership remain. Stop this family; earlier mode/window profiles and rejected bridges stay history. |
| Bicubic/bilinear resizing | Current74views:73pass,one asymmetric uncertainty. [Streaming column reuse](STREAMING-COLUMN-REUSE.md) closes bounded16×8word:336.93/zimg495.11µs,upper.685485,112B/op. All six affected streaming controls pass/stop on the SAME qualified package; exact ordered sums,4096B workspace admission and ownership remain. The adverse initial implementation and earlier uncertain producer remain history. [Default Wide8](BICUBIC-WIDE-CURRENT-CONFIDENCE.md) and [mixed/bounded-wide](BICUBIC-CURRENT-ADJACENT-CONFIDENCE.md) passing paths are unchanged. [Bilinear Up16](BILINEAR-WORD-CURRENT-CONFIDENCE.md) remains1,805.35/FFmpeg1,924.57µs,interval.879672–1.000415,4,498,558B/op uncertain after its longer integration. No identical favorable retries or numerical-policy change. Asymmetric preparation stays explicit. [Equivalent one-shot bilinear](BILINEAR-EQUIVALENT-ONE-SHOT.md) passes/stops:1,973.05/FFmpeg5,139.83µs,upper0.434,4,498,574B/op,including fresh native graph/context/result wrappers and disposal;no production speedup. The eight earlier equivalent one-shot/prepared controls cover bicubic,not bilinear. The original Up16 diagnostic stays uncertain. |
| Lanczos3 | [Canonical identity-axis preparation](LANCZOS-IDENTITY-AXIS.md) retains one exactQ30tap and existing execution/ownership. Eight original one-shot/prepared comparisons pass/stop. Two [additional equivalent one-shot views](LANCZOS-IDENTITY-AXIS-REFERENCE.md) measure the canonical identity branch on the same immutable package:horizontal256×259 608.60/zimg2105.19µs upper.294;vertical513×129 735.96/2061.40µs upper.362,both pass/stop. Managed GC44712–44720/254672–254679B/op,retained explicit plans46656/257100B,governed one-shot peaks577088/786516B are separate. Previous controlled vertical34.22%gain and horizontal CPU uncertainty stay scoped diagnostics. Existing6742full/live eachOS,163internal/15public3modes,three namedstorage suites,SAMEactualpackage/all3examples are reused;source/core/package unchanged. One tooling-only native guard extension,zero production/harness/package builds or broad repeats. No new public boundary/material path,fixture grid,numerical relaxation,pool or API. |
| Copy/turn transforms and drawing | [Grouped copying](SNAPSHOT-BLOCK-COPY.md) retains unchanged flip/edit kernels. [Longer owned-flip integration](OWNED-FLIP-CURRENT-CONFIDENCE.md) resolves word confidence:17.426/FFmpeg17.539µs,upper.997935,56B/op passes/stops;byte8.494/8.412µs,interval.998230–1.019278 stays uncertain. One prespecified4096→16384-call capture retains all12pairs,three native brackets and exact gates;no production speedup or identical favorable retry. Current20owned views have18passes,zero gaps,two uncertainties:byte flip and [word PNG composition](WIDE-PNG-SCAN-CRC.md). [Prepared copy rows](DRAWING-ROW-PREPARATION.md) independently save2.87%/0.058µs in the original separate-byte control,0B/op. Current byte1.969/FFmpeg1.919µs upper1.032 remains a gap;overlap3.669/3.778µs upper.980 passes. [Matched word copy](RECTANGLE-COPY-ADJACENT.md) remains uncertain:3.776/3.768µs,interval.993778–1.006153,0B/op. One65536-call adjacent capture uses equal integration and every original gate;no production gain,passing claim or favorable replay. Five of seven borrowed drawing controls pass;fills/composites unchanged. Exact samples,aliases,chunk polls,admission and ownership remain. Reuse eachOS6721full/live,normal/scalar,SAMEactualpackage/all3examples;one tool-only build,no broad repeat or added storage. Stop passes;do not assume uncertainty needs optimization. |
| Font/text public integration | [Glyph-bounded rows](TEXT-GLYPH-ROW-BOUNDS.md) save4.62%/2.454µs in independent complete ordinary16pairs;Arabic420.483→396.049µs also separates. Current ordinary8 48.308/ImageSharp44.918µs upper1.208 and ordinary16 51.825/46.034µs upper1.244 are both uncertain. GC83216B/op (+80B);no new arrays/owners/admitted or retained capacity. Complete one-shot1151.490/6195.031µs upper.205 and prep+eight1249.351/6867.824µs upper.210 pass/stop;directamort156.169/858.478µs includesprep. Preparation1117.577/6132.818µs and314545.25B are diagnostics. Retainedfit4160B/governedpeak822460B unchanged. [Prepared drawing](TEXT-PREPARED-COVERAGE.md) and [font loading](FONT-PACKED-BOUNDS.md) reuse unchanged timed paths;loading18.424/FreeType14.327µs upper1.324 remains scoped by eager/lazy validation. EachOS6745full/live,268internal/165public3modes,48new sparse canvas checks,eight exact frames,SAMEactualpackage/all3examples. Prototype/final builds are explicit;zero harness/reference/fixturebuilds,oneNoBuildpack,no accepted timedLinuxrebuild. [Earlier clipping](TEXT-FINITE-HORIZONTAL-CLIPPING.md) retains history;no summed-stage or overalltextslowdown claim. |
| Inspection and complete metadata | [Probe owner completion](PROBE-OWNER-COMPLETION.md) preserves shared grammar/reads/checks/polls/metadata and holds admission through actual pending users. Independent async-probe pairs save3.55%/8.01ns;paired and all-actual intervals separate. Four original dimension references refresh on the actual timed/package DLL:PNG/WebP pass;JPEGsync0.199/FFmpeg adapter0.195µs,upper1.078226 remains uncertain;async0.220/0.202µs,upper1.123853 remains a gap. Immediate GC568B unchanged. Forced-pending diagnostic adds8B/probe with equal delivery/bridge overhead;not standalone GC or native performance. Admission512Bdimensions/8448Bbulk,zero added arrays/cache/retention;physical/native/JIT peaks unmeasured. EachOS6727full/live,10internal/154public normal/scalar,SAMEactualpackage/all3examples pass. Two new unsignaled-token stream-cancellation cases also pass baseline;existing pendingEOF/latefailure/refusal/recovery checks remain. One core/embedded/public build perOS,zero helper/harness/native/fixture builds,onepack,no timedLinuxrebuild. Unchanged PNGheader costs reuse their [prior producer](REFILL-CALL-INLINING.md);[JPEG grammar/header evidence](JPEG-INSPECTION-DRIVER.md) remains. No release-wide refresh or favorable replay. [Wide scan CRC](WIDE-PNG-SCAN-CRC.md) closes resident PNG Identify at 6.733/FFmpeg adapter 13.068µs, upper 0.520,96B/op. Seekable/async Identify pass at 8.80/11.80µs,192/216B/op. All three use the actual immutable package and original reference/control boundaries. [Matched batches](PNG-RESIDENT-IDENTIFY-ADJACENT.md) retain attribution/history. [Direct publication](PROBE-RESULT-PUBLICATION.md) retains two unchanged JPEG/WebP resident Identify passes. Labeled FFmpeg adapters/source-specific evidence preserved;no full-release refresh or workload expansion. |

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
[Current row-block copying](ROW-BLOCK-COPY-CURRENT-SCREEN.md) re-evaluates the
earlier block representation after BCL word cloning removed its PNG pipeline call.
Scoped paired gains conflict with the worse standalone overlapping-copy result;
qualified production is restored. One current baseline capture supplies the
overlap cost above;no favorable retries or broad/package repeats. Raw evidence and
the former PNG adversity remain. No permanent block-representation ban.
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

The existing `Up16` bilinear path also has an [equivalent complete one-shot view](BILINEAR-EQUIVALENT-ONE-SHOT.md)
on its frozen 513×259→1027×519 RGBA16 input/settings. Native graph/context/geometry,
first-use storage and result wrappers are fresh per operation; resident inputs are
borrowed on both sides. This adds one comparison view, no required material path
or settings grid. Its pass does not relabel the earlier asymmetric uncertainty.

The existing `PngCropFit16` stream pipeline also has an [equivalent one-shot view](PNG-COMPOSITION-ONE-SHOT.md)
on its frozen513×259 packet and127×64 output. Fresh native codec/options, graph,
wrappers, input acquisition, full decode/conversion and owned result/disposal are
inside timing. Managed public calls and all intermediate owners stay unchanged.
It passes/stops; the previous retained-native observation remains uncertain.
This is one comparison view, no new required material path or fixture/settings grid.

The canonical Lanczos identity-axis branch now has two named additional views of
the existing public one-shot resize boundary: `Horizontal16`513×259→256×259 and
`Vertical16`513×259→513×129, both on the frozen RGBA16 input. These isolate an
unchanged axis omitted from the eight earlier external comparisons. Both complete
one-shot operations pass against the labeled zimg reference and stop. This adds
two comparison views, no new API/material path or shape/depth/filter grid; the
finite289-path requirement is unchanged. [Evidence](LANCZOS-IDENTITY-AXIS-REFERENCE.md).

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

Sixteen required text/drawing additions use the existing raster/font/text recipes.
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
