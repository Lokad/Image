# Material warm coverage

Current architectural phase, 2026-11-18. **289 material required paths** remain
in scope; the 165 displayed comparisons in [benchmark.md](../docs/benchmark.md)
are a summary. Publication is postponed. Functional compatibility, cooperative
memory, compression correctness and CPU parity are separate requirements.
Completion requires valid passing evidence for every named path. Missing evidence,
gaps and uncertainty stay open. No image, shape, depth, filter or quality grid is added.

All 47 original resident PNG decoder controls pass on the actual package with
natural GC. Interleaved native samples resolve the tight working limit RGBA16
control at 2,796.05 / FFmpeg 2,904.45 µs, upper 0.979774 and 70,293 B/op. All
frozen inputs, exact pixels, resource and ownership gates passed. The earlier
uncertain capture is retained. [Current-package PNG](PNG-CURRENT-PACKAGE.md)

Fresh seekable RGBA16 PNG decode passes on the same package at 2,876.67 /
FFmpeg 3,104.71 µs, upper 0.991093 and 8,664 B/op. The original interleaved
driver includes complete stream delivery and independent output ownership.
Four more original storage controls pass on that package. The RGBA8 async
nonseekable encode and two-retained-result decode have uppers of 0.978328 and
0.972152. The fresh seekable and async nonseekable RGBA8 decodes also pass.
All four original PNG coding validators also pass on the current package,
with conservative upper ratios from 0.474500 to 0.910041. Their validation
paths use the changed workspace pools. [Current-package PNG](PNG-CURRENT-PACKAGE.md)

The remaining original fresh nonseekable encodes pass on the actual package.
The conservative upper ratios are 0.889992 for exact lossless WebP, 0.921474
for caller RGB420 JPEG, and 0.829650 for RGBA8 444 JPEG. The async
encoded-output storage change is covered across all four original controls.
Reuse these passes and the original packet, stream and ownership evidence.
[Current-package async encodes](ASYNC-ENCODE-CURRENT-PACKAGE.md)

[Completed nearest rows](NEAREST-ROW-REUSE.md) retains source-valid evidence:
complete controlled RGBA16 enlargement saves 10.35% / 110.756 us with unchanged
4,264,364 B/op. Current standalone 949.453/FFmpeg981.396 us upper.981828 passes.
All four original direct-nearest controls pass/stop; other three allocate112B/op.
Both affected complete PNG pipelines pass. The I/O preparation correction keeps
only untimed temporary image files in bounded tmpfs; all original checks and
idle-v2 thresholds remain. The dual-context screen stays diagnostic; accepted
gain uses one default-loader process per DLL. [Font-header batching](FONT-HEADER-BATCHING.md)
and unchanged borrowed text/editing/codec/other resize paths retain their identified
evidence; its historical costs are superseded by the current mapping-validation round below.

[Shared composition storage](COMPOSITION-STORAGE-REUSE.md) retains the original
PNG16/8 complete decode/orient/crop/fit contracts. Current final-module costs are
2772.041/2937.903us and1185.369/1475.389us,8888.16/4792.08B/op.
Final controlled pairs establish97.4%lessallocation,not a confirmedCPUgain.
The previous Up16 guard-rejected captures remain in their reports; the corrected
complete measurement and accepted row reuse close that named path. Both current
owned flips now [pass/stop](VERTICAL-PREPARED-ROWS.md) on the final immutable
package:RGBA8 8.916/FFmpeg9.146us upper.988240;RGBA16 18.763/19.311us upper.975846,
56B/op. Existing prepared-row execution removes repeated bulk/tail setup with
unchanged checkpoints/ownership. Controlled final-DLL gains.241/.974us are scoped,
not sums of private/historical gains. EachOS6823full/live/10internal190public3modes/
SAMEactualpackage/all3examples pass after affected performance reconciles.
The previous192-view/189pass aggregate is historical,not current storage proof.
Other default ownedimage/font,privatePNGworkspace andasyncencoded-output costs
retain their identified historical artifacts pending refresh;the report names
the affected boundaries. Borrowed drawing/text/copies,caller JPEGsamples and
header/probe paths retain unchanged evidence. No passing pixelkernel is retuned.

Each OS passes **6,776 full/live checks** after nearest row reuse,
534internal/204public checks per normal/scalar mode,and the **same actual
Release-package consumer /three examples**. Prior text/raster arithmetic proofs
remain valid;these do not turn historical storage costs into current timings.
Inspection reuses unchanged10internal/
158public normal/scalar evidence. Lanczos passes163internal/15public normal/AVX512-disabled/scalar, including the
three named resize-storage suites. Font/text passes268internal/165public normal/AVX-disabled/scalar;
PNG paths retain84internal/247public normal/AVX512-off/scalar checks;
copy paths pass25internal/190public normal/scalar checks. Existing resize
normal/scalar/AVX512-disabled, arithmetic/ISA and codec/text evidence is reused. Current complete costs supersede
historical stage clocks; no pipeline gain follows from summing them. Allocation,
logical admitted capacity, idle retention and unmeasured physical peaks stay separate.

## Current family status

| Required family/boundary | Current evidence and next action |
| --- | --- |
| PNG decoding | [Current package](PNG-CURRENT-PACKAGE.md) passes all 47 resident decode controls and the caller-root two-retained-result decode. The four coding validators also pass. [Wide scan CRC](WIDE-PNG-SCAN-CRC.md) retains the original qualification, including exact samples, cancellation and admission. |
| Stream decode/composition | [Current completed-row package](NEAREST-ROW-REUSE.md): original retained-native PNG16/8 pipelines pass at 2772.041/2937.903 us and 1185.369/1475.389 us, upper .951202/.807373, 8888.16/4792.08 B/op. All four original direct-nearest controls pass/stop; Up16 is 949.453/981.396 us upper .981828, 4264364 B/op. Independent controlled complete Up16 gain 10.35%/110.756 us; exact cells/hidden RGB, 1024-pixel polls, no new capacity or pool-cap change. Shared-storage allocation gains remain separately attributed. Every public pipeline call/intermediate/output/disposal and original pixel/source/stream/owner gate remains. Earlier fresh-native one-shot and other historical pool-dependent costs still need the final complete package refresh. |
| JPEG reading | [Current package](JPEG-DECODE-CURRENT-PACKAGE.md) covers all 24 original owned-output cases. 23 pass, including horizontal CMYK with adjacent FFmpeg samples at upper 0.980020. The earlier adverse capture stays available. Sparse-2D is uncertain at upper 1.001969. Resident, fresh stream and fresh native contexts retain separate boundaries. Caller-buffer and coding-only routes retain source-valid evidence. [Horizontal preparation](JPEG-HORIZONTAL-PREPARATION.md) preserves the earlier scoped paired improvement. |
| JPEG encoding | All 18 declared comparisons pass/stop. [Direct block emission](JPEG-DIRECT-BLOCK-EMISSION.md) closes restart33: 1,339.28 / FFmpeg 1,609.15 µs, upper .841, 151,180 B/op, exact 58,010 / 58,012 B packets with the strict 2 B attribution exception. Separate complete paired steps save 19.21% and 17.00%; no summed gain. The 17 [adaptive replay controls](JPEG-LOCAL-ENTROPY-REPLAY.md) reuse unchanged paths/artifact identities. SIMD libjpeg-turbo grayscale stays explicitly labeled. Native contexts/table preparation remain retained; managed preparation stays timed. No added storage or broader content/settings claim. |
| PNG encoding | All 17 equivalent fresh-state controls pass/stop. [Longer Gray8 integration](PNG-GRAY-CURRENT-PATH.md) resolves uncertainty:1,964.07/FFmpeg1,978.51µs,upper.999681,546523B/op,exact72586B. No production speedup; original samples retained. Sixteen passing controls reuse unchanged [compressed heap](PNG-HUFFMAN-HEAP-ENTRIES.md) and [stored](PNG-STORED-WORD-NORMALIZATION.md) evidence. Stored-word naturalGC64 passes/stops; postGC4 retained-word uncertainty remains a distinct diagnostic. |
| Lossless WebP/alpha encoding | All eight pass/stop. Six unchanged lossless-only paths retain [workspace](WEBP-HUFFMAN-WORKSPACE.md),[opaque](WEBP-OPAQUE-CURRENT-CONFIDENCE.md) and [photograph](WEBP-LOSSLESS-PHOTOGRAPH-CONFIDENCE.md) evidence. Current [VP8+alpha](VP8-VARIABLE-PRICE-PREPARATION.md) byte/word52.704/52.346ms,upper.998/.988,6,241,792B/op,exact98590B. Candidate ownership,admitted workspace and exact packets remain. |
| Lossy WebP encoding | All nine declared controls pass/stop on the [factored variable-price preparation](VP8-VARIABLE-PRICE-PREPARATION.md) DLL/actual package. Former photograph gap closes:9,385.67/FFmpeg9,729.91µs upper.971,809080B/op,exact17688B. Controlled independent photograph gain1.623%/154.41µs; alpha16.433%/228.60µs,not historical-clock subtraction. Trellis longer integration resolves uncertainty at upper.935,keeping every original sample. Exact table/packet/owner gates remain; no new capacity/retention. Source-specific earlier attribution and rejected layouts remain history. |
| WebP reading | [VP8L current package](VP8L-DECODE-CURRENT-PACKAGE.md) passes all five affected owned-output cases. [Partitioned VP8 confirmation](VP8-DECODE-CURRENT-PACKAGE.md) passes transparent stream and resident decode at upper 0.947445 and 0.971896. Simple-filter opaque passes on the latest package at upper 0.964514. Ordinary opaque remains uncertain at 0.913911–1.052237 despite the passing paired interval. The live-token interval remains open at 0.984622–1.016033. The two coding-only validators retain source-valid evidence. Exact pixels, admitted owners and original reference boundaries pass. [Fixed-position arithmetic](VP8-FIXED-CODE.md) retains the earlier scoped live-token gain and passing measurements on its producer. |
| Bicubic/bilinear resizing | [Current-package bicubic](BICUBIC-CURRENT-PACKAGE.md) covers all 59 original pool-affected controls. Together with the two source-valid full word enlargements above the 4 MiB pool cap, 56 of 61 original controls pass. All four required prepared bicubic paths also pass on the current package, at 137 B/op with 198,432 B wide and 199,840 B narrow plan retention. The default and tight RGBA8 reductions to 256×129, tight RGBA8 reductions to 128×64 and 64×32, and tight width-down/height-up RGBA16 remain uncertain. Compact weights close the tight 128×64 RGBA16 reduction at 405.66 / 448.29 µs (upper 0.918803, 112 B/op). Retain the exact managed sums and repair, portable fallbacks and prepared native geometry boundary. The [partitioned asymmetric RGBA16 enlargement](BILINEAR-PARTITIONED-REFERENCE.md) passes at 1,847.74 / 2,024.23 µs (upper 0.944044, 4,498,558 B/op). Its [equivalent one-shot](BILINEAR-EQUIVALENT-ONE-SHOT.md) also passes at 1,973.05 / 5,139.83 µs (upper 0.434, 4,498,574 B/op). Keep the [streaming-column](STREAMING-COLUMN-REUSE.md), [mixed/bounded-wide](BICUBIC-CURRENT-ADJACENT-CONFIDENCE.md) and [prior default Wide8](BICUBIC-WIDE-CURRENT-CONFIDENCE.md) measurements with their recorded producer identities. |
| Lanczos3 | [Current package](LANCZOS-CURRENT-PACKAGE.md) passes all eight affected displayed comparisons. The two RGBA16 enlargements bypass the changed idle slots and keep their original passing evidence. Exact pixels, independent outputs and the prepared and one-shot boundaries remain. [Canonical identity axes](LANCZOS-IDENTITY-AXIS.md) retains coefficient proof, prior diagnostics, and Windows/Linux/scalar/package qualification. [Original asymmetric measurements](WARM-CURRENT-JPEG-ROOT-LANCZOS.md) remain setup diagnostics. |
| Copy/turn transforms and drawing | [Current owned edits](OWNED-EDITS-CURRENT-PACKAGE.md) pass sixteen affected cases, with 56 B/op. The two crop comparisons stay uncertain after longer integration at upper 1.064256 and 1.005834. Both [vertical flips](VERTICAL-PREPARED-ROWS.md) retain valid passes after the pool change. The separate composition passes retain their declared boundaries and evidence. **Thirteen borrowed controls:ten passes,two gaps,one uncertainty.** [Shared self-row storage](SELF-ROW-STORAGE.md) retains the scoped20.26%/1.110us final-DLL word-pair gain and source-valid wordself5.186/5.602us upper.930762 / byteself3.885/4.183 upper.938291 PASS-stop. [Complete copy-operation boundaries](COPY-OPERATION-BOUNDARY.md) correct the authored native wrapper: fixed separate arguments were prevalidated; compiler versioning hoisted extended clipped preparation. Dynamic clipping/address preparation now runs every operation, preserving FFmpeg/settings/inputs/exact pixels. Three current actual7b/f4package comparisons:separatebyte1.952957/1.905536us upper1.037472 GAP;clippedbyte1.982622/1.913508 upper1.049530 GAP;separateword3.827293/3.816783 interval.991834–1.011718 UNCERTAIN. All0B,12units/group,original65536/262144ops and case-specific drivers/subdivisions/warmups/conservative gates/idle-v2/full-frame/source/owner checks stay. One authored-worker build,zero production/harness/driver/archive/fixture/package builds;allthree normal/scalar preflights pass. Reuse6844full/live eachOS/SAMEactualpackage/all3examples for unchanged production. Older prepared-native diagnostics and adverse managed trials remain history;no production gain inferred. Do not assume word optimization need or repeat captures for favorable results. Unchangedfill/composite andotherprimitive evidence reused. [Original clipped fills](ORIGINAL-CLIPPED-FILL.md) pass with byte/word upper.696649/.456739;[Original clipped composite](ORIGINAL-CLIPPED-COMPOSITE.md) passes78.567/retainedFFmpegoverlay526.201us,upper.159260:exact managed/native approved RGB1/RMSE.403307,alpha/untouched/zero-alpha exact,reset excluded beforeEVERYoperation both. Its graph/static-source preparation is outside repeated mutation,not one-shot. These public borrowed operations allocate0B;composite uses a separate3x128normal/scalar diagnostic,not inferred BDN GC. [Word self-composite](ORIGINAL-WORD-COMPOSITE.md) passes649.772/ImageSharp716.960us,upper.976714 with sourceClone+Rgba64SrcOver+Dispose included;0/26800GC B/op and at least1062936B snapshot payload,possiblypooled. Caller storage,admitted capacity,retained native graph/pool and unmeasured physical/JIT peaks remain separate. Unchangedfill/composite proof reuses6747full/live eachOS/SAMEactualpackage/all3examples;copy production reuses the6844-check round above. All inherited drawing recipes now measured;two copy gaps and word uncertainty remain. Passing cases stop and adverse/uncertain history stays in linked reports. |
| Font/text public integration | [Shared scan-band coordinates](TEXT-SCAN-BAND-COORDINATES.md) confirms5.33%/2.555us controlled final-DLL ordinary8 gain. Standalone ordinary8 43.817/44.962us upper0.983139 PASS-stop;ordinary16 47.008/49.953us upper0.965760 PASS-stop after [reference interleaving](ORDINARY-WORD-INTERLEAVED-REFERENCE.md),same36nativeactuals acrosssame12pairs;oldgroupeduncertainty retained. 83008/48576B eachdepth,+24managedadapter,+7168temporaryB. All12actuals/threegroups/exactframes/visual andownership gates unchanged;shared phasevariation retained,no favorable replay. Both clippedcontrols andcomplete one-shot/prep+8 pass;all30fittrials/canvases/cleanup stay timed. Fitgovernedpeak748984B(+6144),retained4160B unchanged. EachOS6824full/live/307internal177public normal/AVXoff/scalar/SAMEactualpackage/all3examples pass;timedDLL neverrebuilt. [Batched mapping validation](FONT-MAPPING-GROUP-VALIDATION.md) closes owned font loading:13.393890/FreeType13.668022us upper.986107 PASS-stop,176B. Final controlled pairs save3.63%/.505us,not historical subtraction. Both complete integrations/preparation diagnostic refresh and pass with every original gate;unchanged PreparedDraw neverretimed. EachOS6839full/live/322internal222public3modes/SAMEactualpackage/all3examples;timedDLLnotrebuilt. Parser eager/lazy differences remain explicit. Other layout/default-variation/prepared/codec/resize/copy passes stop. No historical stage sums oroveralltextslowdown claim. |
| Inspection and complete metadata | [Pending publication](JPEG-PENDING-PUBLICATION.md) supplies the unchanged timed/package DLL. Sync probe .184243/FFmpeg adapter .194156us,upper.961768 and complete headers2.187728/2.813302us,upper.844795 PASS-stop. [One longer asynchronous integration](JPEG-PROBE-LONGER-INTEGRATION.md) remains UNCERTAIN:.219043/.209541us,interval.972123–1.120761,568B. Counts increase4x, retaining original warmups,12units/group,16subdivisions,all observations and original conservative/source/descriptor/owner/quiet gates. Adverse wider interval stays; unchanged-case confidence captures stop,with no assumed CPU gap or causal clock claim. Probe/header allocation568/8768B and admission512/8448B remain. JSP's paired2.56% gain is scoped;1080→472B entry frame is not physical peak. Header historical adversity and neutral matched guard remain. Reuse6844full/live per OS,11internal182public normal/scalar,SAMEf4package/all3examples;zero production/harness/reference/original-loop/fixture/package rebuilds. One orchestration-only build. Reference remains FFmpeg AVIO/owned descriptors plus bounded adapter,not public packet-parser performance. PNG/WebP and other families retain identified source-valid evidence. Two byte-copy gaps,word uncertainty and the289-path objective/final165-comparison refresh remain open. |

[JPEG prefix acquisition](JPEG-PREFIX-ACQUISITION-TRIAL.md) is rejected and exactly
restored: the scoped asynchronous gain fails complete synchronous reference
reconciliation. Five useful prefix/lifetime tests remain; both OS pass182focused
public checks in normal/scalar modes. Qualified production/package and current
costs remain unchanged; no new broad/package repetition or renderer dependency.

[Checkpoint preparation](COPY-CHECKPOINT-PREPARATION-TRIAL.md) confirms a scoped
private gain but does not establish useful actual-DLL transfer. The exact qualified
production, embedded modules, source manifest and package are restored; current
costs remain unchanged. All actuals and both code shapes stay retained, without
favorable retry, stopped-path retiming or broad/package repetition.

Recent copy leads remain private: [prepared-row grouping](EIGHT-VECTOR-COPY-GROUP-TRIAL.md#prepared-row-follow-up),
[capacity access](WHOLE-ROW-COPY-PREPARATION-TRIALS.md#capacity-getter-follow-up) and
[stride preparation](WHOLE-ROW-COPY-PREPARATION-TRIALS.md#stride-preparation-follow-up).
Their small/inconclusive screens preserve all exact gates and current costs;
no confirmation, cross-family retiming or renderer dependency follows.

[Direct copy addressing](COPY-DIRECT-ADDRESSING-TRIAL.md) is inconclusive and
unaccepted;production/package/current costs remain unchanged. The bounded trial
retains one prototype,profile/codegen and all actuals,without confirmation or broad
repeats. The five authored-font controls pass. Current ordinary RGBA8/16 pass/stop after final-DLL and explicit
reference-schedule reconciliation; meaningful loading work remains qualified. Remaining copy gaps require
distinct attribution before another implementation trial; rejected variants
are not replayed solely because their family remains open.

[Byte-channel quotient trials](TEXT-BYTE-QUOTIENT-TRIALS.md) retain exact corrected
scalar/SIMD arithmetic but establish no useful complete ordinary8 gain. Both stay
private; no confirmation, current-cost change, passing-path retiming or broad/package
repeat. The original bilinear16 comparison now [passes/stops](BILINEAR-PARTITIONED-REFERENCE.md)
after intra-actual interleaving, preserving its original uncertainty as history.
No production speedup or relaxed gate follows; independent font/copy work continues.

[Post-prefix font attribution](FONT-PREFIX-ATTRIBUTION.md) remains source-valid.
The [later trace](FONT-RETAINED-PREFIX-ATTRIBUTION.md) was an unnecessary repeat;
retain its raw data without treating it as new timing proof or a reason to restart
the completed [copy/validation scheduling trials](FONT-COPY-VALIDATION-TRIALS.md).
Required copy/eager-header locations identify no redundant copy/table pass.
Font costs remain unchanged; a new prototype requires a materially distinct cost.

[Decoded location carry](FONT-LOCATION-CARRY-TRIAL.md) removes one overlapping
load/decode per eight glyphs, but the complete owned-load screen is near-neutral:
14.831643->14.789937us,interval.950562–1.046099,176B unchanged. Keep it private;
no confirmation,standalone refresh or broad/package repeat. BothOS308internal/
222public focused checks and the original ordered oracle pass; the explicitly
recorded tool assertion correction reflects existing retained-prefix ownership,
without changing inputs or correctness gates. Current costs remain unchanged.

[Eight-header wide bounds](FONT-WIDE-BOUNDS-TRIALS.md) also stays private. The
512-bit predicate screens13.860->14.157us; a codegen-driven constant/native-mask
revision screens13.959->13.927us,interval.971060–1.025125,176B unchanged.
BothOS308internal/222public focused checks,65,536actual-helper batches and the
original16,384ordered oracle pass. Instruction changes occur without a useful
complete-load gain; no confirmation, reference refresh or broad/package repeat.
Keep qualified production, font costs and passing integrations unchanged.

[Prepared copy-tail trials](COPY-VECTOR-TAIL-TRIALS.md) remain rejected. A private
confirmation's .961811–.998421 interval does not transfer: the final DLL is
adverse at1.009814–1.024389 despite matching source, dependencies and selected
machine code. Restore the exact qualified sources, Windows/Linux core and embedded
DLLs, and unchanged package; retain all captures without another timing attempt.
No standalone refresh, broad/package repeat or current-cost change follows.

[Prepared separate-word rows](PREPARED-WORD-ROWS-TRIAL.md) also stays private:
3.795249→3.840771us,interval.970497–1.055266,0B unchanged. The existing prepared
kernel runs, but one eligibility change establishes no useful complete-operation
gain. BothOS10internal/190public checks in three modes and original full-word
checks pass. No confirmation, passing-path retiming or broad/package repeat;
qualified production remains unchanged; current complete-boundary copy status is
recorded above, superseding the older prepared-native costs.

[Endpoint classification](FONT-ENDPOINT-CLASSIFICATION-TRIAL.md) preserves ordered
validation and ownership but its independent confirmation remains inconclusive.
It is not promoted; current font costs and passing integrations stay unchanged.

[Destination-aligned copy trials](COPY-DESTINATION-ALIGNMENT-TRIALS.md) retain two
correct, unpromoted implementations with adverse means and inconclusive screens.
Preparation reuse establishes no gain; a scoped hint diagnostic does not justify
more per-image state. Current copy costs remain unchanged; the separate-word
uncertainty is now a demonstrated gap in the partitioned-actual capture.

[Certified crossing-order trials](TEXT-CERTIFIED-ORDER-TRIALS.md) retain two
unpromoted stages: local descriptors are slower; prepared-range reuse is
unpromising with an inconclusive interval and higher allocation/lifetime.
Current ordering, package and costs remain unchanged. Independent copy work
continues from concrete alignment attribution rather than another sort variant.

[Pinned copy alignment](PINNED-COPY-ALIGNMENT-TRIAL.md) is unpromoted: the complete
original word pair is 13.20%/0.803 us slower despite a real alignment opportunity.
Qualified copying and current costs remain unchanged; pinning costs stay inside
measurement and no neighboring implementation is automatically replayed.

[Exact edge storage](TEXT-EDGE-STORAGE-TRIAL.md) remains unpromoted: ordinary
allocation falls 8,320 B, but independent complete draws are 0.498 us/1.07% slower.
Current production/package costs stay unchanged; retain the adverse evidence
without adding a renderer dependency or another neighboring layout trial.

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

The [original planned-case reconciliation](ORIGINAL-PLANNED-COVERAGE-RECONCILIATION.md)
retains all 17 inherited names outside the numeric snapshot. All are measured:
16 passes and one copy gap. [Five authored-font comparisons](AUTHORED-FONT-COMPARISONS.md)
now pass/stop after independent confirmation on the unchanged actual package.
Their explicitly source-corrected Fonts/Drawing public integration includes all
nine layout calls/tab adaptation, owned outputs, optional drawing and disposal;
stock references, exact patches and independent defect reproductions are retained.
Frozen fixtures/managed frames and exact/visual gates remain unchanged; Windows,
Linux normal/scalar and survivor/root cleanup checks pass. Current JPEGheaders pass
via [batched inspection](JPEG-BATCHED-ENTROPY.md), while default variation retains
its original passing reference/boundary. Related inputs
or different boundaries do not retire a missing original control. No new path or
workload grid;all 289 material paths remain required. Prior rounds and raw evidence
remain linked in that reconciliation rather than repeated here.

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
and **four** prepared-bicubic plus **two** prepared-text public boundaries give **289**, without adding a shape/depth/filter/quality grid. The sixteen-row text/drawing table below includes those last two. A later case
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

[Authored-font preflight](AUTHORED-FONT-REFERENCE-PREFLIGHT.md) adds correctness-only
observations for the six remaining original tiny-face controls, using the same
actual package on Windows/Linux. All retained-owner/shared-root checks pass;
default variation subsequently [passes/stops](DEFAULT-VARIATION.md) at
3.551/ImageSharp public integration20.357us,upper.184945,3608/58496B per operation.
Four reference public layout calls plus outlines/measurement/independent outputs
and disposal are inside timing;fonts outside. Managed retained-layout charge3568B
and governedpeak5738B are separate from allocation;reference/physical peaks remain
unmeasured. Zero production/VM/package builds;reuse unchanged qualification.
[Source-built cmap4 correction](AUTHORED-FONT-CMAP4-CORRECTION.md) now isolates the
reference mapping defect:unmodified source control matches all six stock
observations,and corrected non-tab packed glyphs match,with all18observations
perOS agreeing across the three reference contexts. Fixtures/managed results and
stock identities remain unchanged. The subsequent
[five authored public comparisons](AUTHORED-FONT-COMPARISONS.md) resolve the scoped
tab, compound and raster reference defects and all pass/stop. The three layout
costs reuse unchanged paths; two rendering costs and complete one-shot/repeated
integrations use the current [word-dispatch package](ORDINARY-WORD-TEXT-DISPATCH.md).
These public integration results establish no general parser/shaping equivalence.
Independent codec/copy/resize work remains available.
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

[Text edge-capacity trials](TEXT-EDGE-CAPACITY-TRIALS.md) remain unpromoted.
Both reduce public ordinary-draw allocation83,216->51,264B;the separate-pass CPU
effect is inconclusive,and retained-hint confirmation regresses2.53%/1.224us.
All actuals/guards/exactframes are retained. Production/package/current costs
remain unchanged;no broad/package repeat or renderer dependency. Further work
needs attributable useful work,not favorable recaptures of either implementation.

[Whole-row copy preparation](WHOLE-ROW-COPY-PREPARATION-TRIALS.md) stays private.
Both original separate-word screens have adverse means; the extent-proof revision
removes duplicate JIT loops but remains inconclusive. No confirmation, production
change, reference refresh or broad/package repeat. Current gaps and qualified
mapping package remain; the earlier row-view risk is reconciled explicitly.
