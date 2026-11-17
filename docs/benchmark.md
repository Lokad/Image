# Lokad.Image benchmarks

Current development version **0.2.1**. Warm single-CPU parity remains open across
**289 named material paths**; these **165 comparisons** summarize that scope.
This is an incremental update. Nearest resizing and PNG composition use the
completed-row package; rectangle copies use the shared-self-row package. Current
font loading reuses its header-normalization evidence; affected text costs use
the conditional-polygon-scratch package.
Other entries that
call the changed shared image/font pool, PNG workspace or async encoded-output
pool retain **historical artifact costs pending refresh**, not current timing
proof. [Affected boundaries](../benchmarks/COMPOSITION-STORAGE-REUSE.md) are explicit.
This is not a full release refresh or a whole-library parity claim.

[Direct JPEG block emission](../benchmarks/JPEG-DIRECT-BLOCK-EMISSION.md) closes
the restart gap: **1,339.28 / FFmpeg 1,609.15 µs, upper 0.841**,
**151,180 B/op**, exact **58,010 / 58,012 B** packets with the strict metadata exception.
All 18 declared JPEG encoding comparisons pass and stop; 17 adaptive controls
reuse their unchanged paths and original artifact identities. Separate complete
paired confirmations measure **19.21%** for block emission and **17.00%** for bounded
sign arithmetic. These successive effects are not added into a cumulative claim.
No heap capacity or retention is added; validation and independent owners remain.

[Shared self-row storage](../benchmarks/SELF-ROW-STORAGE.md) saves
**20.26% / 1.110 µs** in controlled complete word pairs.
One shared bounded backing and row step replace duplicate execution state; copying,
snapshot order, cancellation and ownership remain exact. Current original word
cost **5.186 / FFmpeg 5.602 µs**, upper
**0.930762**, passes/stops. This is a scoped final-DLL gain;
historical clocks and prototype effects are not added.

Separate byte copy is **2.172 / 2.080 µs**, upper
**1.059**, a gap. Separate word copy **4.029 / 3.898 µs**,
upper **1.061029**, remains uncertain on the frozen 262,144-operation cadence.
All actuals and three native brackets stay, without an assumed gap, causal
regression claim or favorable replay. Byte overlap **3.885 / 4.183 µs**,
upper **0.938291**, passes/stops. Clipped byte copy retains a small gap.
All five actual-reference costs use the same final timed/package DLL; other
primitives retain their identified source-valid evidence.

These copies allocate **0 B/op** and add no owned/admitted/retained workspace,
pin, cache or API. Caller pixels and unmeasured physical/native/JIT peaks stay
separate. Windows/Linux each pass **6,805 full/live checks**, focused **6 internal /
190 public** normal/AVX-512-disabled/scalar checks and the SAME actual package/all
three examples. Performance reconciles before broad qualification; no timed DLL
rebuild occurs. Earlier [alignment trials](../benchmarks/COPY-DESTINATION-ALIGNMENT-TRIALS.md)
remain unpromoted history, outside the renderer's measurement dependencies.

[Original RGBA16 self-compositing](../benchmarks/ORIGINAL-WORD-COMPOSITE.md)
passes/stops at **649.772 / ImageSharp 716.960 µs**, upper
**0.976714**, **0 / 26,800 GC B/op**. The reference is explicitly
ImageSharp 3.1.12 Rgba64 **source Clone + SrcOver + Dispose**, with the necessary
snapshot timed; reset is excluded before EVERY operation on both sides. Exact
managed/full-word and reference max1/RMSE .25, untouched/zero-alpha gates pass.
Snapshot payload is at least 1,062,936 B, possibly pooled; retained pool and physical
peaks are unmeasured. No broader content/settings or historical-clock gain claim.


[Original planned coverage](../benchmarks/ORIGINAL-PLANNED-COVERAGE-RECONCILIATION.md)
retains inherited unmeasured names. Passing related inputs or different public
boundaries do not retire those requirements; the full 289-path objective stays open.

[Original clipped fills](../benchmarks/ORIGINAL-CLIPPED-FILL.md) pass; the
[clipped byte copy](../benchmarks/RECTANGLE-COPY-PREPARED-ROWS.md) retains a scoped gap. Both use the original 254×129 effective region; earlier 256-pixel controls
stay separate. The [original byte composite](../benchmarks/ORIGINAL-CLIPPED-COMPOSITE.md)
passes at **78.567 / 526.201 µs**, upper **0.159260**, over 510×254 pixels.
Its reference is a **retained FFmpeg n8.1 overlay graph/static source**, with reset
excluded before EVERY operation on both sides; graph preparation is outside this
repeated borrowed mutation, not a one-shot cost. Managed pixels remain exact;
native RGB max 1/RMSE .403307, alpha/untouched/zero-source-alpha gates pass.
All four original clipped operations allocate **0 B/public operation**; composite
allocation uses a separate 3×128 normal/scalar diagnostic because its BDN driver
emits no GC line. Added owned capacity per operation is zero; standing caller pixels,
native graph/allocator retention and unmeasured physical/JIT peaks stay separate.
Reuse unchanged **6,747 checks per OS/SAME actual package/all 3 examples**. All inherited
drawing recipes now have measurements: **thirteen borrowed controls, ten passes,
two gaps, one uncertain**. [Authored default variation](../benchmarks/DEFAULT-VARIATION.md)
now passes/stops at **3.551 / 20.357 µs**, upper **0.184945**,
with **3,608 / 58,496 B/op**. Four reference public layout calls, outlines,
measurement, independent output and disposal are included; fonts are prepared
outside both. This is ImageSharp integration, not FFmpeg, bare shaping or one-shot
font loading. Managed surviving-layout charge **3,568 B**, governed peak **5,738 B**
and unmeasured reference/physical retention are separate from allocations.
The [five original authored-font controls](../benchmarks/AUTHORED-FONT-COMPARISONS.md)
remain passing: affected rendering costs use the conditional-polygon-scratch package, while
the three layout costs reuse unchanged paths. Their reference is explicitly
**source-corrected Fonts 2.1.3 / Drawing 2.1.7 public integration**, retaining stock
identities, exact patches and independent defect reproductions. All nine public
layout calls/tab adaptation, owned results, optional drawing and disposal stay timed;
fonts and per-operation canvas reset stay outside both. Managed frames/fixtures and
all exact/visual gates remain unchanged. Public-operation allocations are listed;
single-invocation BDN drawing additionally counts 736 B of common scaffolding.
Independent confirmation retains all actuals/guards and stops all five. All 17
inherited names now have measurements: **16 passes, one copy gap**. Broader text
processing and the remaining 289-path objective stay open; no historical stage sums.

[Batched JPEG entropy inspection](../benchmarks/JPEG-BATCHED-ENTROPY.md) saves
**47.63% / 1.912 µs** in independent controlled
complete-operation pairs. Current standalone **JPEG 1.999 /
2.799 µs, upper 0.730193, 8,768 B/op**
passes/stops. Two new lane/buffer/pending-read controls pass original/current,
scalar and AVX2-disabled execution; **6,749 full/live checks per OS** and the SAME
actual package/all three examples pass, without rebuilding the timed artifact.
No added heap capacity or retention; admission stays 512/8,448 B, native bounded
scratch 16,416 B excluding contexts/descriptors, physical/JIT peaks unmeasured.
The [earlier stuffed-pair round](../benchmarks/JPEG-STUFFED-PAIRS.md) retains history;
its gains are not added. Source-valid **WebP .142106 / .310231 µs, upper .471884,
552 B/op** stays stopped. The earlier **2.698 ns / 1.255% async JPEG dimension-probe
regression** remains explicit; its unchanged-path cost is **.220388 / .201119 µs,
upper1.133673**, still a gap. Sync probe pairs are inconclusive; unchanged prefix
paths reuse their identified evidence. Different wrappers'8,776/8,768B are not an
allocation improvement. Header references are
explicitly **FFmpeg AVIO/owned descriptor APIs plus original bounded structural/EXIF
adapters**, not public packet-parser performance. Both include fresh non-exposed
source/working state/descriptor ownership, complete late-metadata/EOF checks and
exact nine-field source metadata, with no pixels or whole-input copy. Twenty
same-packet controls and two new one-byte/pending/late-EXIF/restart-limit cases pass.
After affected costs reconcile, Windows/Linux each pass **6,747 full/live checks**,
**10 internal/156 public normal/scalar**, the **SAME actual package/all three examples**.
One core/embedded/public build per OS, zero helper/native/harness/fixture builds,
one NoBuild pack, no timed Linux rebuild. New storage/retention stays zero;
admission remains512/8,448B. Native scratch16,416B excludes contexts/descriptors;
physical/native/JIT peaks remain unmeasured. Packets/pixels and ownership stay exact.

[Prepared stream input](../benchmarks/PREPARED-MEMORY-INPUT.md) saves **2.75 µs /
15.40% raw**, **15.01% reference-normalized**, on complete PNG `Identify(Stream)`:
**15.05 / reference adapter 15.88 µs, upper .950**, **192 B/op** on the
current [publication round](../benchmarks/PROBE-RESULT-PUBLICATION.md). One owned input
capacity replaces geometric growth for ordinary synchronous `MemoryStream` inputs
up to 4 MiB. This control's logical acquisition peak falls **393,216 → 262,144 B**;
no idle-pool or complete-decode peak saving is inferred. The original uncertain
stream captures remain linked beside their current confirmations.

[Completed nearest rows](../benchmarks/NEAREST-ROW-REUSE.md) qualify the current complete PNG16 pipeline at **2,772.04 / FFmpeg 2,937.90 µs, upper 0.951202**, **8,888.16 B/op**. PNG8 passes at **1,185.37 / 1,475.39 µs, upper 0.807373**, **4,792.08 B/op**. Native contexts remain retained; managed stream delivery, decode, orientation, crop, nearest fit, every intermediate and independent output/disposal stay timed. All four original direct-nearest controls pass and stop. RGBA16 enlargement is **949.45 / FFmpeg 981.40 µs, upper 0.981828**, **4,264,364 B/op**; the other three allocate112B/op. Independent final-DLL pairs save **110.756 µs / 10.35%**, with unchanged allocation and owned/retained capacity. Copies retain exact complete samples and1024-pixel cancellation checkpoints; no pool-cap or numerical change. The original I/O-guard failures remain: bounded tmpfs holds only untimed temporary images, with original inputs/settings, complete-operation boundaries and idle-v2 thresholds unchanged. A dual-context screen stays diagnostic; accepted gain uses one default-loader process per DLL. Windows/Linux each pass6776full/live checks,534internal/204public normal/scalar and the SAME actual package/all three examples. Earlier [shared storage](../benchmarks/COMPOSITION-STORAGE-REUSE.md) independently establishes97.4%lessallocation, not a summed CPU gain. Earlier fresh-native [one-shot composition](../benchmarks/PNG-COMPOSITION-ONE-SHOT.md) and other historical storage-dependent costs still await the final complete package refresh.

[Current VP8 arithmetic](../benchmarks/VP8-FIXED-CODE.md) retains the
stream pass: **5,701.59 / FFmpeg
5,982.33µs, upper
0.956**, **240,496 B/op**. This complete
operation includes fresh stream acquisition, decoding, owned output and disposal.

[Opaque lossless WebP](../benchmarks/WEBP-OPAQUE-CURRENT-CONFIDENCE.md) now passes:
**67.959 / libwebp 69.457 ms, upper .985**, **4,822,760 B/op**, exact **121,896 B**.
One prespecified longer batch resolves uncertainty on the actual package DLL,
with zero builds or qualification repeats; no production speedup is claimed.
All eight lossless/alpha controls pass and stop. [Word alpha](../benchmarks/WARM-ADJACENT-CONFIDENCE.md)
now also passes on the actual package, upper .979, **6,241,792 B/op**, exact
**98,590 B**; its prespecified longer batch also requires no builds or qualification repeats.

[Current owned-flip checks](../benchmarks/COMPOSITION-STORAGE-REUSE.md)
retain uncertainty at **8.693 / FFmpeg 8.330 µs,
upper 1.101581** for RGBA8 and **17.658 /
17.586 µs, upper 1.010163** for RGBA16, **56 B/op** each.
The same complete public calls, independent outputs, original full samples and all
12 actuals/three native brackets remain. Neither is a current passing claim.
[Earlier copy/flip evidence](../benchmarks/OWNED-FLIP-CURRENT-CONFIDENCE.md) stays
linked, including useful kernel gains; its old storage costs do not qualify the
changed shared pool. Borrowed rectangle copies do not call that pool and reuse
their unchanged-path evidence. No favorable retry or pixel-kernel retuning.

The [family reports](../benchmarks/WARM-COVERAGE.md) retain a historical reconciliation
of **192 image views: 189 pass,
0 gaps, 3 uncertain**.
This is not a current shared-storage status count; affected costs await refresh.
Unchanged borrowed operations retain source-valid evidence; remaining uncertainty,
drawing and inspection gaps keep whole-library parity open.
[Current bicubic confidence](../benchmarks/BICUBIC-CURRENT-ADJACENT-CONFIDENCE.md) resolves default mixed-word resize (**789.05 / zimg 837.28 µs, upper 0.946**) and bounded byte-wide resize (**340.70 / 347.49 µs, upper 0.984**). Bounded word-wide also passes (**398.16 / 405.63 µs, upper 0.991**). All three stop, **112 B/op**, zero builds. [Default byte-wide longer integration](../benchmarks/BICUBIC-WIDE-CURRENT-CONFIDENCE.md) also passes/stops: **337.24 / zimg 349.38µs, upper 0.970941**, **198,350 B/op**. One 64→256 integration retains all actuals and the original uncertain capture; zero builds/qualification repeats, no production speedup. [Streaming column reuse](../benchmarks/STREAMING-COLUMN-REUSE.md) closes the bounded 16×8 word uncertainty: **336.93 / zimg 495.11µs, upper 0.685485**, **112 B/op**. Adjacent columns share pixel conversion and vertical preparation, preserving exact ordered sums. All six affected streaming controls pass/stop; workspace admission remains **4 KiB**, with no retained operation workspace. Both OSes pass **6,702 full/live** checks, focused normal/scalar/AVX512-disabled checks and the **SAME actual package/all three examples**. The adverse first implementation and earlier uncertainty remain history. Fresh managed/prepared native boundaries remain explicit; equivalent prepared/one-shot passes are reused.
[RGBA16 bilinear enlargement](../benchmarks/BILINEAR-WORD-CURRENT-CONFIDENCE.md)
remains uncertain: **1,805.35 / FFmpeg 1,924.57µs,
upper 1.000415**, **4,498,558 B/op**. The longer
integration retains every native fluctuation; this is not a pass or a production
speedup. Necessary managed preparation and the native prepared-graph boundary stay explicit.
[Equivalent one-shot bilinear](../benchmarks/BILINEAR-EQUIVALENT-ONE-SHOT.md) now passes:
**1,973.05 / FFmpeg 5,139.83 µs, upper 0.434**,
**4,498,574 B/op**. Fresh graph/context/geometry, lazy first use,
result wrapper, filtering and owned-output disposal are timed. Both sides borrow resident
source rasters. Exact managed/full-word/native/ownership gates remain; the existing
qualified package is reused with no production rebuild. This qualifies the complete
bilinear boundary; the eight earlier equivalent views cover bicubic. No production
speedup or historical phase-cost subtraction is claimed; the asymmetric view above
stays uncertain. No material path or settings grid is added.
Longer-integration [grayscale PNG](../benchmarks/PNG-GRAY-CURRENT-PATH.md) resolves
uncertainty at **1,964.07 / FFmpeg 1,978.51µs,
upper 0.999681**, **546,523 B/op**. All 17
equivalent fresh-state PNG encodes pass/stop. Original uncertain samples remain;
this measurement-only update is not a production speedup. Stored RGBA16's
post-GC diagnostic stays separate from its passing natural-GC workload.
[Variable-price preparation](../benchmarks/VP8-VARIABLE-PRICE-PREPARATION.md) closes
the photograph gap: **9,385.67 / FFmpeg
9,729.91µs, upper 0.971**,
**809,080 B/op**, exact **17,688 B**. All nine lossy and all eight lossless/alpha
controls pass/stop. Independent controlled gains are **154.41µs/1.623%** photograph
and **228.60µs/.433%** RGBA16alpha; allocations/retention remain unchanged.
The longer trellis integration retains its original uncertain capture. Unchanged
encoder paths reuse this source-valid evidence on the current qualified package.
This scoped refresh does not establish whole-library parity; publication is postponed.
[Fixed-position VP8 arithmetic](../benchmarks/VP8-FIXED-CODE.md) keeps the code
byte at a fixed UInt64 position, preserving scalar cursors, every cancellation
check and input-refill addresses. Independent live-token pairs save **41.53µs/.86%**
in their scoped ALC comparison. The actual standalone live comparison now passes:
**4,767.34 / FFmpeg 4,784.37µs**, ratio
**0.9964**, upper **0.999450**,
**230,880 B/op**. All pixels remain exact; original
uncertainties remain in history. All six affected costs refresh on the timed,
immutable actual package; **all twelve WebP reads pass/stop**. Separate ordinary,
stream and coding-validation points are higher than the preceding round; current
costs stay visible and no uniform speedup or historical-clock gain is claimed.
Allocations, capacity and retention are unchanged. Both OSes pass **6,692 full/live** checks,
focused normal/scalar checks and the **same actual package/all three examples**.
[Earlier unaccepted arithmetic-window trials](../benchmarks/VP8-LIVE-WINDOW-TRIALS.md)
remain separate history; none of their coefficient state-bridge changes survives.
[Canonical Lanczos identity axes](../benchmarks/LANCZOS-IDENTITY-AXIS.md) remove
zero-only coefficient storage and size working rows from actual taps. Four affected
one-shot reference comparisons refresh on the immutable timed/package DLL; four
unchanged prepared nonidentity paths reuse qualified evidence. **All eight declared
Lanczos comparisons pass/stop**; equivalent bicubic controls are unchanged.

Two additional complete managed-DLL diagnostics use the existing full-word input:
513×259→513×129 independently improves **34.22% /
381.16 µs**. The horizontal
513×259→256×259 CPU effect is inconclusive, while allocations fall about
**100,136→44,712 B/op** and retained preparation **102,000→46,656 B**.
Its governed one-shot peak falls **632,432→577,088 B**. Vertical allocation falls
about **267,016→254,672 B/op**, retained **269,364→257,100 B**, and peak
**798,780→786,516 B**. Ranges and raw observations remain in the report.
Those diagnostics establish only scoped prior/candidate effects. Two subsequent
[equivalent one-shot comparisons](../benchmarks/LANCZOS-IDENTITY-AXIS-REFERENCE.md)
on the same immutable package now pass: horizontal **608.60 / 2,105.19µs,
upper0.294**, vertical **735.96 / 2,061.40µs, upper0.362**.
Managed GC ranges **44,712–44,720 / 254,672–254,679B/op** remain separate from
retained plans and governed peaks. All ten Lanczos views pass/stop; no stage-cost
sum or broader grid is implied. The full289-path objective stays open.
Windows/Linux each pass **6,742 full/live**, **163 internal/15 public** focused
normal/scalar/AVX512-disabled checks, the three named resize-storage suites and
the SAME actual package/all three examples, including one-axis asynchronous
survivors. Allocation, retained admission and physical peaks stay distinct.
[Horizontal JPEG preparation](../benchmarks/JPEG-HORIZONTAL-PREPARATION.md) saves
**4.03% / 16.92 µs** in controlled complete-decode pairs, with unchanged **20,944 B/op**.
The 27 unchanged RGBA/fresh/caller-buffer read views retain passing evidence;
the affected stream view refreshes and passes in the prepared-input round. The
horizontal-CMYK cost is **410.41 / FFmpeg 418.11 µs, upper .988**. Companion
confidence is reconciled before acceptance; historical clock shifts are not gains.
Adverse and uncertain captures stay linked; copy/turn confidence remains open.

Every operation runs sequentially on the dedicated quiet VM, pinned to one CPU,
with .NET 10, optimized JIT and nonconcurrent workstation GC. Startup, transport,
fixture preparation and validation are outside the clocks. Timed calls retain
their declared normalization, input delivery, output ownership and disposal.
Ratios are Lokad/reference; **upper** is the conservative confidence bound, with
all actuals retained. Upper ≤1 passes, lower >1 is a gap; otherwise uncertainty stays open.
**GC B/op** reports managed allocation, including result ownership where applicable,
not peak live capacity or native allocation.

The shared image/font store now has **four slots / 4 MiB**; codec-local pools
retain **two slots / 4 MiB** each. Source-derived word-composition idle payload
is **2,455,088 B**, up from **2,125,872 B**; four simultaneous image payloads still
total **2,455,088 B**. This added idle retention is separate from reduced allocation.
Caller-governed storage bypasses that cache. Logical admitted
capacity, allocation and retained storage are separate; physical/native/JIT peaks
are unmeasured. Prepared word Lanczos reduction retains **156,248 B** of plan
storage, separately from its **138 B/op**. That storage stage passed **6,683
full/live checks** per OS, focused normal/scalar copy-ownership checks and the
**same actual Release-package consumer / three examples**.
Unchanged arithmetic/ISA qualification is reused; publication is postponed.

[Probe owner completion](../benchmarks/PROBE-OWNER-COMPLETION.md) avoids the
outer async wrapper for immediate completion and transfers pending work with its
owner. Independent complete async-probe pairs save
**3.55% /
8.01 ns**;
paired and all-actual intervals separate. These scoped effects are not added to
earlier gains or described as FFmpeg parity. That stage refreshed all four dimension
probes; unchanged PNG/WebP pass and stop. Source-valid
JPEG sync is uncertain at **0.199 µs,
upper 1.078**;async remains a gap at
**0.220 µs,
upper 1.134**.
The async cost uses the current stuffed-pair package above, including its small
measured regression; other cases retain their original source-valid producers.
Filtered results and all-actual statistics remain separate;the displayed upper
takes the worse view. Original inputs,callbacks,native brackets and exact
descriptor/owner gates stay fixed. Unchanged PNG-header costs retain their
[source-valid producer](../benchmarks/REFILL-CALL-INLINING.md);
[JPEG grammar/header evidence](../benchmarks/JPEG-INSPECTION-DRIVER.md) remains.
Immediate GC stays **568 B/probe**. One-gated-read allocation diagnostics add
**8 B/probe**,with unchanged full-header diagnostic allocation. Absolute totals
include equal gated-input/bridge overhead and are not production GC costs.
Admission remains **512 B dimensions /8,448 B bulk**,with zero added arrays,
cache or retained workspace;physical/native/JIT peaks are unmeasured.
Windows/Linux each pass **6,727 full/live checks**, **10 internal/154 public**
normal/scalar,the SAME actual package and all three examples. Two new stream
cancellation cases also pass baseline,including unsignaled tokens. Actual pending
users retain ownership;late I/O failure,cleanup,refusal and recovery checks remain.
One coherent qualification/pack milestone follows performance reconciliation;
the accepted timed Linux DLL is never rebuilt. Earlier/adverse evidence remains.
[Wide scan CRC](../benchmarks/WIDE-PNG-SCAN-CRC.md) closes resident PNG Identify:
**6.733 / FFmpeg adapter
13.068 µs**, conservative upper
**0.520**, **96 B/op**. Seekable/async Identify
also pass on the same timed/package DLL. Four existing 128-bit residues advance
together under 512-bit hardware guards; the seed, polynomial, joins, tails,
cancellation positions, portable fallbacks and ownership stay exact. Encoder CRC
and shared fields/helpers are unchanged. Earlier matched-batch diagnostics remain
[history](../benchmarks/PNG-RESIDENT-IDENTIFY-ADJACENT.md).
[Direct publication](../benchmarks/PROBE-RESULT-PUBLICATION.md) retains its allocation
reduction and two source-valid JPEG/WebP resident Identify passes. All 60 affected
PNG costs reconcile before one Windows/Linux qualification milestone: **6,721
full/live checks per OS**, **84 internal /247 public** normal/AVX512-off/scalar,
the SAME actual package and all three examples. One core/embedded build per OS,
one pack and no accepted-module rebuild after timing. No new heap array, static
field, pool or retained operation storage. Other codec/edit/resize paths reuse
unchanged evidence; font construction and complete text integrations are qualified
separately below. This is not a whole-release timing refresh. Probe/header admission stays
**512 B dimension /8,448 B bulk maximum**, zero retained operation workspace;
physical/native/JIT peaks are unmeasured. No historical stage gains are added.

† Reference boundaries require care. Codec contexts or filter graphs may remain
prepared; packed conversion, independent payload acquisition and release are timed
as declared. **zimg** is FFmpeg's labeled scaler backend, using float intermediates;
managed Q30/rational pixel guarantees remain exact. One-shot and prepared results
are distinct. Grayscale JPEG uses approved **SIMD libjpeg-turbo**; lossless WebP uses
**libwebp exact=1** where labeled. These are benchmark tools, never production dependencies.

Text comparisons use **ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12**;
the five authored controls explicitly label their scoped source corrections above.
They are public integration workloads, not FFmpeg or bare shaping-engine comparisons.
[Conditional polygon scratch](../benchmarks/TEXT-CONDITIONAL-POLYGON-SCRATCH.md)
owns current ordinary, clipped and complete-integration costs on the actual timed
package DLL. Controlled complete RGBA8 draws save **0.769 µs /
1.65%** with separated intervals. Allocations remain
**75,840 B/draw** plus 24 B of adapter storage. Current 16,384-operation ordinary
RGBA8/16 confirmations remain uncertain: upper **1.127899 /
1.054277**. All actuals, three reference groups, resets
and exact frames remain; no historical-clock or cumulative gain is claimed.

Complete one-shot and preparation-plus-eight-draw integrations pass, including
all public layout calls, 30 fitting trials, fresh owned explicitly cleared canvases,
independent results and disposal. The measured batch amortizes to
**158.84 / 868.28 µs/draw**.
Preparation-only **1159.78 / 6212.83 µs** is diagnostic;
historical stages are never summed. The reference retains positioned paths and
managed preparation retains full 16-bit coverage. Validation eagerness and
GC-driven reference font release differ. Reference one-shot/batch allocations are
**9,256,953 /9,494,633 B**. Latin/fit retained plans remain **2,112 /4,160 B** and
the full-fit governed peak remains **821,900 B**. Allocations, admitted capacity
and unmeasured physical/native/JIT peaks are distinct.

[Font loading](../benchmarks/FONT-HEADER-NORMALIZATION.md) reuses unchanged
**FreeType 2.13.2 copy/open/retain/dispose** evidence: **176 B/op**, upper
**1.165**, a scoped eager/lazy-validation gap, not general parser equivalence
or an overall text slowdown. Unchanged [prepared ordinary drawing](../benchmarks/TEXT-PREPARED-COVERAGE.md)
passes/stops at **56 B/op** with its distinct retained-state boundary.

Windows/Linux each pass **6,803 full/live checks**, focused **292 internal/222 public**
normal/AVX-disabled/scalar checks and the SAME actual timed package/all three
examples. No timed DLL rebuild occurs. The complete final-package refresh remains pending.

RGBA16 compositing uses **ImageSharp Rgba64 SrcOver**, not FFmpeg. Keep the approved
[visual/native bounds](../benchmarks/TEXT-DRAWING-CURRENT.md) distinct from exact
managed pixels. Complete encoder packets and narrow metadata exceptions retain
their [declared scope](codec-parity.md); equal quality numbers are not quality matching.

**Codecs**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Packet B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| WebP VP8 + alpha, fresh seekable Stream (parity confirmed) † | 5,701.59 | 5,982.33 | 0.953 / 0.956 | 240,496 | — |
| JPEG 4:4:4, forward-only / exact reference (parity confirmed) † | 3,083.86 | 3,772.61 | 0.817 / 0.825 | 820,120 | 133,039 / 133,041 |
| JPEG 4:2:0 from caller RGB samples (parity confirmed) † | 1,431.80 | 1,578.72 | 0.907 / 0.912 | 388,222 | 52,091 / 52,093 |
| JPEG RGBA16 4:2:0 / caller IMemoryBudget (parity confirmed) † | 1,525.81 | 1,841.57 | 0.829 / 0.832 | 819,475 | 52,115 / 52,117 |
| JPEG grayscale, standalone SIMD libjpeg-turbo (parity confirmed) † | 962.45 | 1,408.06 | 0.684 / 0.688 | 155,435 | 58,910 / 58,910 |
| JPEG from caller GRAY8, standalone SIMD libjpeg-turbo (parity confirmed) † | 889.23 | 1,077.61 | 0.825 / 0.834 | 155,439 | 58,910 / 58,910 |
| VP8L photograph / libwebp exact=1 † | 81,000.31 | 83,387.76 | 0.971 / 0.981 | 5,358,368 | 160,032 / 160,032 |
| VP8L text/diagram / libwebp exact=1 † | 7,441.59 | 9,575.22 | 0.777 / 0.791 | 3,721,041 | 7,234 / 7,234 |
| VP8 opaque RGBA8 † | 13,566.83 | 15,414.61 | 0.880 / 0.886 | 1,201,560 | 72,380 / 72,380 |
| VP8 opaque RGBA16, including normalization † | 13,936.40 | 15,494.52 | 0.899 / 0.906 | 1,201,560 | 72,380 / 72,380 |
| VP8 opaque photograph † | 9,385.67 | 9,729.91 | 0.965 / 0.971 | 809,080 | 17,688 / 17,688 |
| VP8 opaque text/diagram † | 5,197.67 | 5,818.65 | 0.893 / 0.899 | 740,992 | 7,100 / 7,100 |
| VP8 diagram, effort2 / distortion-only search † | 2,438.71 | 2,509.32 | 0.972 / 0.977 | 406,664 | 7,330 / 7,330 |
| VP8 diagram, effort6 / trellis † | 9,111.36 | 9,762.54 | 0.933 / 0.935 | 754,352 | 6,818 / 6,818 |
| VP8 RGBA8 + lossless alpha † | 52,704.44 | 53,106.80 | 0.992 / 0.998 | 6,241,792 | 98,590 / 98,590 |
| VP8 RGBA16 + alpha, including normalization † | 52,346.05 | 53,555.75 | 0.977 / 0.988 | 6,241,792 | 98,590 / 98,590 |
| VP8L RGBA8 / libwebp exact=1 † | 107,509.18 | 126,289.11 | 0.851 / 0.910 | 5,948,664 | 183,812 / 183,812 |
| VP8L opaque RGBA8 / libwebp exact=1 † | 67,959.20 | 69,456.89 | 0.978 / 0.985 | 4,822,760 | 121,896 / 121,896 |
| PNG RGBA8, one-shot SmallestSize9 / native9 † | 16,887.12 | 17,258.24 | 0.978 / 0.992 | 815,716 | 210,973 / 210,973 |
| PNG RGBA16, one-shot SmallestSize9 / native9 † | 19,824.87 | 20,956.16 | 0.946 / 0.958 | 2,432,924 | 963,202 / 963,202 |
| PNG RGBA8, one-shot Fastest1 / native1 † | 4,111.01 | 4,361.68 | 0.943 / 0.963 | 816,180 | 236,828 / 236,828 |
| PNG RGBA16, one-shot stored0 / native0 † | 389.62 | 1,387.71 | 0.281 / 0.288 | 1,143,324 | 1,066,552 / 1,066,552 |
| PNG grayscale8, one-shot Optimal6 / native6 † | 1,964.07 | 1,978.51 | 0.993 / 1.000 | 546,523 | 72,586 / 72,586 |
| PNG RGB8, one-shot Optimal6 / native6 † | 9,398.61 | 9,773.57 | 0.962 / 0.978 | 815,148 | 168,729 / 168,729 |
| PNG gray-alpha8, one-shot Optimal6 / native6 † | 4,197.60 | 4,477.89 | 0.937 / 0.953 | 547,596 | 120,366 / 120,366 |
| PNG RGBA8, one-shot Optimal6 / native6 † | 16,718.11 | 17,275.89 | 0.968 / 0.979 | 815,716 | 210,973 / 210,973 |
| PNG RGBA16, one-shot Optimal6 / native6 † | 19,971.87 | 20,773.12 | 0.961 / 0.968 | 2,432,924 | 963,202 / 963,202 |
| PNG RGB16, one-shot Optimal6 / native6 † | 15,326.44 | 16,232.30 | 0.944 / 0.955 | 2,426,756 | 739,472 / 739,472 |
| PNG grayscale16, one-shot Optimal6 / native6, red-plane reference † | 4,602.49 | 5,408.55 | 0.851 / 0.867 | 818,004 | 265,433 / 265,433 |
| PNG gray-alpha16, one-shot Optimal6 / native6 † | 9,892.73 | 10,500.84 | 0.942 / 0.952 | 1,356,164 | 506,887 / 506,887 |
| PNG RGBA8, one-shot stored0 / native0 † | 53.27 | 541.40 | 0.098 / 0.114 | 607,628 | 533,444 / 533,444 |
| PNG RGBA8 decode, live token † | 1,378.18 | 1,395.98 | 0.987 / 0.996 | 4,472 | — |
| PNG RGBA8 decode, tight working limit † | 1,161.50 | 1,394.24 | 0.833 / 0.840 | 68,224 | — |
| PNG RGBA16 decode, tight working limit † | 2,797.18 | 2,898.56 | 0.965 / 0.975 | 70,272 | — |
| PNG predictable RGBA16 3000², Optimal6 / mixed6 † | 259,795.57 | 388,008.23 | 0.670 / 0.685 | 2,491,252 | 573,041 / 573,041 |
| PNG RGBA16, one-shot Fastest1 / native1 † | 17,266.79 | 18,600.99 | 0.928 / 0.934 | 2,432,924 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, live cancelable token † | 17,573.86 | 18,607.98 | 0.944 / 0.954 | 2,432,924 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, forward-only output † | 17,196.47 | 18,607.32 | 0.924 / 0.929 | 2,432,956 | 968,216 / 968,216 |
| PNG RGBA8 async, one-shot Optimal6, nonseekable output † | 16,720.65 | 17,676.67 | 0.946 / 0.961 | 877,476 | 210,973 / 210,973 |
| JPEG baseline420, resident decode † | 665.65 | 1,009.30 | 0.660 / 0.670 | 258,454 | — |
| JPEG DC-only, caller CMYK decode † | 322.49 | 331.83 | 0.972 / 0.994 | 1,085,981 | — |
| VP8L RGBA8, resident decode † | 2,709.07 | 3,019.90 | 0.897 / 0.902 | 152,728 | — |
| VP8L RGBA8, fresh seekable Stream decode † | 2,805.41 | 3,016.87 | 0.930 / 0.935 | 152,824 | — |
| VP8L palette16 RGBA8, resident decode † | 408.48 | 545.38 | 0.749 / 0.759 | 288,464 | — |
| JPEG CMYK sparse-2D, resident decode † | 414.93 | 433.23 | 0.958 / 0.970 | 20,944 | — |
| JPEG YCCK420, resident decode † | 417.49 | 708.60 | 0.589 / 0.593 | 895,563 | — |
| JPEG 4:1:1, resident decode † | 374.05 | 448.00 | 0.835 / 0.841 | 219,552 | — |
| JPEG 4:1:0, resident decode † | 339.24 | 507.96 | 0.668 / 0.680 | 189,090 | — |
| VP8L RGBA16, including normalization / libwebp exact=1 † | 114,600.56 | 131,281.45 | 0.873 / 0.887 | 5,948,664 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 112,507.39 | 131,039.53 | 0.859 / 0.869 | 5,949,048 | 183,812 / 183,812 |
| JPEG caller RGB24 async nonseekable, 420 (parity confirmed) † | 1,422.33 | 1,626.70 | 0.874 / 0.878 | 309,523 | 52,091 / 52,093 |
| JPEG RGBA8 async nonseekable, 444 (parity confirmed) † | 3,106.93 | 3,858.03 | 0.805 / 0.810 | 820,502 | 133,039 / 133,041 |
| PNG8 decode, two retained images / shared caller root † | 2,575.83 | 2,846.82 | 0.905 / 0.909 | 1,638,214 | — |
| JPEG RGBA8 4:2:0, row restart33 (parity confirmed) † | 1,339.28 | 1,609.15 | 0.832 / 0.841 | 151,180 | 58,010 / 58,012 |
| JPEG dense CMYK, resident decode † | 1,280.01 | 3,113.05 | 0.411 / 0.413 | 20,944 | — |
| JPEG horizontal-only CMYK, resident decode † | 410.41 | 418.11 | 0.982 / 0.988 | 20,944 | — |
| JPEG YCCK420, caller RGB24 decode † | 438.99 | 604.73 | 0.726 / 0.742 | 762,720 | — |
| JPEG progressive grayscale, resident decode † | 1,588.74 | 1,711.21 | 0.928 / 0.933 | 151,432 | — |
| JPEG progressive420, resident decode † | 1,412.35 | 1,949.00 | 0.725 / 0.729 | 243,676 | — |
| JPEG progressive422, resident decode † | 1,777.18 | 2,247.92 | 0.791 / 0.802 | 315,496 | — |
| JPEG ordinary progressive444, resident decode † | 2,840.13 | 3,368.87 | 0.843 / 0.855 | 169,664 | — |
| JPEG progressive444, live-token resident decode † | 3,039.33 | 3,356.43 | 0.906 / 0.915 | 169,664 | — |

**Editing**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 256×129 | 588.87 | 1,568.56 | 0.375 / 0.378 | 138,650 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 256×129 | 462.40 | 488.52 | 0.947 / 0.953 | 138 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 256×129 | 623.14 | 1,806.50 | 0.345 / 0.350 | 155,075 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 256×129 | 485.36 | 512.07 | 0.948 / 0.958 | 138 |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 1027×519 | 3,768.25 | 5,225.86 | 0.721 / 0.726 | 319,224 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 1027×519 | 3,554.71 | 4,074.82 | 0.872 / 0.882 | 148 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 1027×519 | 4,080.93 | 5,074.41 | 0.804 / 0.808 | 4,592,818 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 1027×519 | 4,117.26 | 4,212.39 | 0.977 / 0.990 | 4,264,760 |
| Bicubic resize RGBA8 † → 256×129 | 415.34 | 419.25 | 0.991 / 0.997 | 199,985 |
| Bicubic upscale RGBA16 † → 1027×519 | 2,630.58 | 2,872.22 | 0.916 / 0.923 | 4,264,573 |
| Bicubic RGBA8, width reduction / height enlargement, input+output+4 KiB † → 256×519 | 754.23 | 838.25 | 0.900 / 0.907 | 113 |
| Bicubic RGBA16, width down / height up / input+output+4 KiB † → 256×519 | 804.84 | 818.22 | 0.984 / 0.990 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 32×16 | 403.27 | 413.28 | 0.976 / 0.982 | 113 |
| Bicubic RGBA8 extreme reduction, input+output+4 KiB † → 2×9 | 451.05 | 1,046.58 | 0.431 / 0.433 | 113 |
| Bicubic RGBA8 enlargement, input+output+4 KiB † → 1027×519 | 2,816.92 | 3,042.03 | 0.926 / 0.931 | 116 |
| Bicubic RGBA8 width enlargement, input+output+4 KiB † → 1027×259 | 1,418.23 | 1,515.14 | 0.936 / 0.943 | 114 |
| Bicubic RGBA8 height enlargement, input+output+4 KiB † → 513×519 | 1,397.56 | 1,747.50 | 0.800 / 0.805 | 114 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129 | 409.71 | 418.67 | 0.979 / 0.993 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 256×129 | 503.81 | 576.25 | 0.874 / 0.886 | 113 |
| Bicubic RGBA8, fresh managed / prepared zimg † → 128×64 | 337.24 | 349.38 | 0.965 / 0.971 | 198,350 |
| Bicubic RGBA8, width up / height down, input+output+4 KiB † → 1027×129 | 830.84 | 864.78 | 0.961 / 0.967 | 113 |
| Bicubic RGBA16, width up / height down, input+output+4 KiB † → 1027×129 | 820.00 | 846.10 | 0.969 / 0.978 | 113 |
| Bicubic RGBA16 extreme reduction, input+output+4 KiB † → 2×9 | 428.16 | 1,261.83 | 0.339 / 0.353 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 64×32 | 353.91 | 388.26 | 0.912 / 0.921 | 113 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 128×64 | 340.70 | 347.49 | 0.980 / 0.984 | 112 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 128×64 | 398.16 | 405.63 | 0.982 / 0.991 | 112 |
| Bicubic RGBA8, width down / height up, default workspace † → 256×519 | 771.67 | 844.28 | 0.914 / 0.926 | 199,947 |
| Bicubic RGBA16, width down / height up, default workspace † → 256×519 | 752.15 | 827.29 | 0.909 / 0.926 | 199,898 |
| Exact rectangle copy into borrowed RGBA8 canvas (gap) | 2.17 | 2.08 | 1.044 / 1.059 | 0 |
| Exact rectangle copy into borrowed RGBA16 canvas (confidence open) | 4.03 | 3.90 | 1.034 / 1.061 | 0 |
| Exact clipped fill into borrowed RGBA8 canvas (parity confirmed) | 1.24 | 1.57 | 0.789 / 0.793 | 0 |
| Exact clipped fill into borrowed RGBA16 canvas (parity confirmed) | 2.09 | 4.10 | 0.511 / 0.513 | 0 |
| Overlapping RGBA8 copy + reverse restoration, per pair (parity confirmed) | 3.88 | 4.18 | 0.929 / 0.938 | 0 |
| Original overlapping RGBA16 copy, 257×129, shift (1,1), per pair (parity confirmed) | 5.19 | 5.60 | 0.926 / 0.931 | 0 |
| RGBA8 source-over / FFmpeg overlay (parity confirmed) † | 65.01 | 451.06 | 0.144 / 0.150 | 0 |
| RGBA16 source-over / ImageSharp 3.1.12 (parity confirmed) † | 563.63 | 629.51 | 0.895 / 0.913 | 0 |
| Original self-composite RGBA16 / ImageSharp snapshot + SrcOver (parity confirmed) † | 649.77 | 716.96 | 0.906 / 0.977 | 0 |
| Complete text one-shot / ImageSharp public integration (pass) † | 1,206.81 | 6,552.16 | 0.184 / 0.210 | 298,786 |
| Complete text preparation + eight draws / ImageSharp public integration (pass) † | 1,270.75 | 6,946.25 | 0.183 / 0.208 | 308,516 |
| Text ordinary draw RGBA8 / ImageSharp.Drawing (confidence open) † | 48.13 | 45.27 | 1.063 / 1.128 | 75,840 |
| Text ordinary draw RGBA16 / ImageSharp.Drawing (confidence open) † | 47.17 | 47.04 | 1.003 / 1.054 | 75,840 |
| Text prepared coverage RGBA8 / ImageSharp.Drawing (pass) † | 2.93 | 36.37 | 0.081 / 0.087 | 56 |
| Text prepared coverage RGBA16 / ImageSharp.Drawing (pass) † | 5.30 | 37.68 | 0.141 / 0.152 | 56 |
| Font loading / FreeType 2.13.2 (scoped gap) † | 15.24 | 13.19 | 1.156 / 1.165 | 176 |
| Wrapped logical layout / SixLabors public API (pass) | 6.07 | 96.16 | 0.063 / 0.097 | 3,153 |
| Latin OpenType layout / SixLabors public API (pass) | 20.40 | 57.62 | 0.354 / 0.717 | 6,409 |
| Arabic OpenType layout / SixLabors public API (pass) | 88.93 | 337.84 | 0.263 / 0.421 | 28,658 |
| Complete fitting search / SixLabors public API (pass) | 1,144.00 | 1,882.11 | 0.608 / 0.866 | 224,181 |
| Bilinear upscale RGBA8 † → 1027×519 | 1,964.35 | 1,990.85 | 0.987 / 0.992 | 102,891 |
| Bilinear RGBA16 enlargement, fresh managed / prepared FFmpeg graph (uncertain) † → 1027×519 | 1,805.35 | 1,924.57 | 0.938 / 1.000415 | 4,498,558 |
| Bilinear one-shot RGBA16 / FFmpeg one-shot † → 1027×519 | 1,973.05 | 5,139.83 | 0.384 / 0.434 | 4,498,574 |
| Bilinear resize RGBA8 † → 256×129 | 373.69 | 384.94 | 0.971 / 0.979 | 8,473 |
| Bilinear resize RGBA16 † → 256×129 | 388.37 | 418.68 | 0.928 / 0.937 | 8,473 |
| Bicubic RGBA8, width reduction / same height, input+output+4 KiB † → 256×259 | 500.42 | 574.64 | 0.871 / 0.875 | 113 |
| Bicubic one-shot RGBA8 / zimg one-shot † → 128×64 | 350.31 | 990.90 | 0.354 / 0.357 | 198,455 |
| Bicubic prepared RGBA8 / zimg prepared † → 128×64 | 259.35 | 348.80 | 0.744 / 0.747 | 137 |
| Bicubic one-shot RGBA16 / zimg one-shot † → 128×64 | 395.89 | 599.92 | 0.660 / 0.677 | 198,453 |
| Bicubic prepared RGBA16 / zimg prepared † → 128×64 | 328.80 | 404.94 | 0.812 / 0.822 | 137 |
| Bicubic one-shot narrow RGBA8 / zimg one-shot † → 256×129 | 447.86 | 1,038.21 | 0.431 / 0.437 | 199,991 |
| Bicubic prepared narrow RGBA8 / zimg prepared † → 256×129 | 398.55 | 422.93 | 0.942 / 0.952 | 137 |
| Bicubic one-shot narrow RGBA16 / zimg one-shot † → 256×129 | 446.16 | 715.28 | 0.624 / 0.628 | 199,983 |
| Bicubic prepared narrow RGBA16 / zimg prepared † → 256×129 | 373.53 | 570.51 | 0.655 / 0.662 | 137 |
| Lanczos3 one-axis one-shot RGBA16 / zimg one-shot † → 256×259 | 608.60 | 2,105.19 | 0.289 / 0.294 | 44,712–44,720 |
| Lanczos3 one-axis one-shot RGBA16 / zimg one-shot † → 513×129 | 735.96 | 2,061.40 | 0.357 / 0.362 | 254,672–254,679 |
| PNG RGBA16 one-shot stream decode/orient/crop/fit / FFmpeg one-shot † → 127×64 | 2,856.94 | 3,301.31 | 0.865 / 0.871 | 338,156–338,160 |
| Plain transpose RGBA16 † → 259×513 | 34.98 | 43.85 | 0.798 / 0.859 | 56 |
| Plain transpose RGBA8 † → 259×513 | 26.85 | 37.83 | 0.710 / 0.716 | 56 |
| Counterclockwise quarter turn RGBA8 † → 259×513 | 28.57 | 37.43 | 0.763 / 0.772 | 56 |
| Counterclockwise quarter turn RGBA16 † → 259×513 | 38.49 | 44.19 | 0.871 / 0.875 | 56 |
| Mirrored transpose RGBA8 † → 259×513 | 30.07 | 37.55 | 0.801 / 0.805 | 56 |
| Half turn RGBA8 † | 7.90 | 11.79 | 0.671 / 0.677 | 56 |
| Vertical flip RGBA8 (uncertain) † | 8.69 | 8.33 | 1.044 / 1.102 | 56 |
| Original clipped RGBA8 fill, 254×129 (parity confirmed) | 1.195 | 1.730 | 0.690 / 0.696649 | 0 |
| Original clipped RGBA16 fill, 254×129 (parity confirmed) | 2.184 | 4.829 | 0.452 / 0.456739 | 0 |
| Original clipped RGBA8 copy, 254×129 (gap) | 2.145 | 2.091 | 1.026 / 1.034826 | 0 |
| Original clipped RGBA8 composite / retained FFmpeg (parity confirmed) | 78.567 | 526.201 | 0.149 / 0.159260 | 0 |
| Authored default-variation layout / ImageSharp public integration (parity confirmed) | 3.551 | 20.357 | 0.174 / 0.184945 | 3,608 |
| Authored Unicode layout / corrected ImageSharp integration (parity confirmed) | 2.585 | 117.234 | 0.022 / 0.023246 | 5,592 |
| Authored cmap4 layout / corrected ImageSharp integration (parity confirmed) | 2.610 | 120.738 | 0.022 / 0.022768 | 5,088 |
| Authored compound layout / corrected ImageSharp integration (parity confirmed) | 2.604 | 112.021 | 0.023 / 0.023809 | 6,152 |
| Authored RGBA8 layout + draw / corrected ImageSharp integration (parity confirmed) | 41.223 | 224.429 | 0.184 / 0.299789 | 41,064 |
| Authored RGBA16 layout + draw / corrected ImageSharp integration (parity confirmed) | 38.190 | 225.899 | 0.169 / 0.294813 | 41,064 |

**Inspection and pixel-coding validation**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Probe PNG dimensions, seekable | 0.109 | 0.122 | 0.888 / 0.922 | 568 | Confirmed pass |
| Probe JPEG dimensions, seekable | 0.199 | 0.195 | 1.022 / 1.078 | 568 | Confidence open |
| Probe WebP dimensions, seekable | 0.103 | 0.121 | 0.853 / 0.884 | 568 | Confirmed pass |
| Probe JPEG dimensions, async nonseekable | 0.220 | 0.201 | 1.096 / 1.134 | 568 | Confirmed gap |
| Validate PNG8, span † | 1,158.656 | 1,377.875 | 0.841 / 0.848 | 240 | Confirmed pass |
| Validate PNG16, span † | 2,697.838 | 2,960.838 | 0.911 / 0.918 | 240 | Confirmed pass |
| Validate indexed Adam7 19×17, span † | 1.794 | 3.950 | 0.454 / 0.458 | 792 | Confirmed pass |
| Validate JPEG baseline420, span † | 483.057 | 674.268 | 0.716 / 0.720 | 10,016 | Confirmed pass |
| Validate JPEG progressive420, span † | 288.749 | 297.280 | 0.971 / 0.984 | 4,088 | Confirmed pass |
| Validate VP8L+alpha, span † | 2,314.233 | 3,046.809 | 0.760 / 0.767 | 297,070 | Confirmed pass |
| Validate VP8+alpha, span † | 4,331.362 | 5,454.375 | 0.794 / 0.797 | 15,600 | Confirmed pass |
| Validate PNG16, async nonseekable † | 2,753.691 | 3,613.167 | 0.762 / 0.766 | 360 | Confirmed pass |
| Inspect PNG8 complete headers, seekable, adapter reference | 0.291 | 0.580 | 0.501 / 0.509 | 568 | Confirmed pass |
| Identify PNG8 complete container, resident, adapter reference | 6.733 | 13.068 | 0.515 / 0.520 | 96 | Confirmed pass |
| Identify JPEG420, resident, canonical adapter | 0.497 | 1.181 | 0.421 / 0.431 | 96 | Confirmed pass |
| Identify VP8L+alpha, resident, parser adapter | 0.022 | 5.039 | 0.004 / 0.005 | 96 | Confirmed pass |
| Identify PNG8, fresh seekable Stream, adapter reference | 8.802 | 15.951 | 0.552 / 0.555 | 192 | Confirmed pass |
| Identify PNG8, async nonseekable, adapter reference | 11.801 | 19.297 | 0.612 / 0.621 | 216 | Confirmed pass |
| JPEG Adobe CMYK horizontal AC / FFmpeg fresh contexts † | 372.151 | 753.216 | 0.494 / 0.498 | 20,944 | Confirmed pass |
| Complete JPEG headers, fresh source / FFmpeg AVIO adapter (pass) | 1.999 | 2.799 | 0.714 / 0.730193 | 8,768 |
| Complete WebP headers, fresh seekable source / FFmpeg AVIO adapter (pass) | 0.142 | 0.310 | 0.458 / 0.471884 | 552 |

[Coverage and current family reports](../benchmarks/WARM-COVERAGE.md) identify
remaining paths, boundaries, allocations, references and raw evidence. Superseded
experiments remain linked separately. Update with `python benchmarks/render-benchmark.py`;
verify with `--check`.
