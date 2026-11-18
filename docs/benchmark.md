# Lokad.Image benchmarks

Current development version **0.2.1**. Warm single-CPU parity remains open across
**289 named material paths**; these **165 comparisons** summarize that scope.
This is an incremental update. Nearest resizing and PNG composition use the
completed-row package; self copies retain the source-valid shared-self-row package.
Three separate/clipped copy comparisons use the pending-publication package with
corrected per-operation native preparation. Current
font loading and complete text integrations use the mapping-validation package;
ordinary/clipped drawing retains the source-valid scan-band-coordinate package.
JPEG stream probes and complete headers use the pending-publication package.
[Resident PNG decode](../benchmarks/PNG-CURRENT-PACKAGE.md) passes for
**47 of 47** original controls on that actual package with natural GC.
The tight working limit RGBA16 confirmation has an upper ratio of **0.979774**
and **70,293 B/op**. The three displayed resident decode rows use these measurements.
Fresh seekable RGBA16 PNG decode also passes on the same package at
**2,876.67 / FFmpeg 3,104.71 µs**,
upper **0.991093**, **8,664 B/op**.
The RGBA8 seekable and async nonseekable loads, async nonseekable encode,
and caller-budget two-result decode also pass on that package. The encode
and retained-output rows use the current measurements, as do all four PNG
pixel-coding validation rows.
[Current bicubic measurements](../benchmarks/BICUBIC-CURRENT-PACKAGE.md) update
the RGBA8 reductions, the tight RGBA8 enlargement and both tight mixed-axis
RGBA16 directions. Compact horizontal coefficients close the tight 128×64
RGBA16 gap on the new actual package at **405.66 /
448.29 µs**, upper **0.918803**
and **112 B/op**. RGBA8 remains uncertain against the prepared zimg reference.
The tight 16×8 RGBA16 reduction passes on the same package at
**362.74 /
534.04 µs**, upper
**0.683124** and **112 B/op**.
The 4 KiB 64×32 RGBA8 reduction remains uncertain at upper
**1.014632**.
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

[Complete copy-operation boundaries](../benchmarks/COPY-OPERATION-BOUNDARY.md)
now include native dynamic clipping and address preparation every operation.
The old fixed adapter prevalidated geometry; compiler versioning hoisted
preparation in the extended clipped adapter. The explicit authored-wrapper
correction preserves FFmpeg, inputs, settings and exact output. Old figures
remain prepared-native diagnostics, without a production speedup claim.

Separate byte is **1.953 / 1.906 µs**, upper
**1.037472**, a gap; clipped byte also remains a gap. Separate word is
**3.827 / 3.817 µs**, interval
**0.991834–1.011718**, uncertain. All twelve units/group, original
65,536/262,144 operations, case-specific drivers, subdivisions, warmups and
conservative gates stay. No favorable replay or assumed word optimization need.
Byte overlap **3.885 / 4.183 µs**, upper
**0.938291**, passes/stops. Both self-copy passes retain identified
source-valid shared-self-row evidence; the three corrected comparisons use the
actual pending-publication package. Other primitives keep their own producers.

Copies allocate **0 B/op**, with no new owned/admitted/retained workspace.
Caller storage and unmeasured physical/native/JIT peaks stay separate. All three
corrected normal/scalar preflights and original full-frame/source/owner gates
pass. One authored native-worker build; zero production/harness/driver/archive/
fixture/package builds. Unchanged production reuses **6,844 full/live checks each
OS**, the SAME actual package and all three examples. Superseded/rejected
experiments remain linked history, outside current renderer dependencies.

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
[clipped byte copy](../benchmarks/COPY-OPERATION-BOUNDARY.md) is a demonstrated gap. Both use the original 254×129 effective region; earlier 256-pixel controls
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
two gaps, one uncertainty**. [Authored default variation](../benchmarks/DEFAULT-VARIATION.md)
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

[Current-package VP8 decode](../benchmarks/VP8-DECODE-CURRENT-PACKAGE.md) measures
the fresh stream at **6,082.74 / FFmpeg 6,494.10 µs**,
upper **0.947445**, **240,496 B/op**
(parity confirmed). The complete operation includes fresh stream
acquisition, decoding, independent output ownership and disposal. The partitioned
schedule also confirms the transparent resident case.
On the latest package the same schedule confirms simple-filter opaque decode at
**4,711.56 / FFmpeg 4,900.74 µs**,
upper **0.964514**, **230,880 B/op**.
Ordinary opaque decode remains uncertain at upper **1.052237**.

All five affected [VP8L decode comparisons](../benchmarks/VP8L-DECODE-CURRENT-PACKAGE.md)
pass on the same package. The resident, fresh-stream and indexed rows show their
current costs including complete public decoding and owned outputs.

[Opaque lossless WebP](../benchmarks/WEBP-OPAQUE-CURRENT-CONFIDENCE.md) now passes:
**67.959 / libwebp 69.457 ms, upper .985**, **4,822,760 B/op**, exact **121,896 B**.
One prespecified longer batch resolves uncertainty on the actual package DLL,
with zero builds or qualification repeats; no production speedup is claimed.
All eight lossless/alpha controls pass and stop. [Word alpha](../benchmarks/WARM-ADJACENT-CONFIDENCE.md)
now also passes on the actual package, upper .979, **6,241,792 B/op**, exact
**98,590 B**; its prespecified longer batch also requires no builds or qualification repeats.

[Current-package owned edits](../benchmarks/OWNED-EDITS-CURRENT-PACKAGE.md)
refresh eighteen original comparisons affected by the shared storage pool.
The six displayed transpose, quarter-turn and half-turn rows use those costs.
[Prepared vertical rows](../benchmarks/VERTICAL-PREPARED-ROWS.md)
close both original owned-flip controls: **8.916 / FFmpeg 9.146 µs,
upper 0.988240** for RGBA8 and **18.763 /
19.311 µs, upper 0.975846** for RGBA16, **56 B/op** each.
Both pass/stop. Reuse the existing bounded-row helper with opposite steps,
preparing bulk/tail geometry once; no new kernel, workspace, pool, pin or API.
Final-DLL controlled pairs save **0.241 µs /
2.69%** and **0.974 µs /
5.38%**. Private/historical effects are not added.
Complete public validation/admission, independent output materialization and disposal
stay timed; FFmpeg retains graph/source/frame wrappers. Every original full sample,
owner and guard passes, with all12actuals/three interleaved native groups retained.
Windows/Linux each pass **6,823 full/live checks**, focused normal/AVX-512-disabled/
scalar checks and the **SAME actual package/all three examples**. The timed DLL is
never rebuilt. Actual result ownership, unchanged four-array/4 MiB idle storage,
zero added scratch and unmeasured physical/native/JIT peaks remain distinct.
Other copy, font/text, codec and resizing paths retain identified source-valid
costs. The complete final-package refresh remains outstanding.

The [family reports](../benchmarks/WARM-COVERAGE.md) identify source-valid passing
paths and remaining copy/inspection gaps. Historical image-view aggregates
stay in their original reports; the complete final-package refresh remains open.
[Earlier bicubic comparisons](../benchmarks/BICUBIC-CURRENT-ADJACENT-CONFIDENCE.md)
and [streaming-column qualification](../benchmarks/STREAMING-COLUMN-REUSE.md)
retain their original producer evidence.
[RGBA16 bilinear enlargement](../benchmarks/BILINEAR-PARTITIONED-REFERENCE.md)
passes/stops: **1,847.74 / FFmpeg 2,024.23µs,
upper 0.944044**, **4,498,558 B/op**.
Finer interleaving partitions each256-operation actual into16subdivisions while
retaining twelve statistical units per group, every operation and the unchanged
warmups/conservative gate. Extra clock reads stay measured; more frequent native
IPC/client work stays outside. Reuse6,823checks per OS/SAME actual C620/f034 package/
all3examples; one small driver build, zero production/harness/native/package builds
or broad repeats. The frozen manifest is restored as an independent file before
timing, preserving current metadata. Old uncertainty remains history; no production
speedup or host cause is claimed. Fresh managed preparation and retained native
graph/geometry stay timed as declared, distinct from the equivalent view below.
[Equivalent one-shot bilinear](../benchmarks/BILINEAR-EQUIVALENT-ONE-SHOT.md) now passes:
**1,973.05 / FFmpeg 5,139.83 µs, upper 0.434**,
**4,498,574 B/op**. Fresh graph/context/geometry, lazy first use,
result wrapper, filtering and owned-output disposal are timed. Both sides borrow resident
source rasters. Exact managed/full-word/native/ownership gates remain; the existing
qualified package is reused with no production rebuild. This qualifies the complete
bilinear boundary; the eight earlier equivalent views cover bicubic. Both bilinear
views pass/stop without a production speedup or historical phase-cost subtraction.
No material path or settings grid is added.
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
in their scoped ALC comparison. The [current-package live-token comparison](../benchmarks/VP8-DECODE-CURRENT-PACKAGE.md)
is **5,198.14 / FFmpeg 5,194.48 µs**,
upper **1.016033**, **230,880 B/op**
(confidence open). All five owned-output comparisons keep their
exact pixels and reference boundaries. Coding-only validation retains its
source-valid evidence.
[Canonical Lanczos identity axes](../benchmarks/LANCZOS-IDENTITY-AXIS.md) remove
zero-only coefficient storage and size working rows from actual taps.
[Current-package measurements](../benchmarks/LANCZOS-CURRENT-PACKAGE.md) confirm
all six affected two-axis comparisons and both one-axis controls. Both RGBA16
enlargements exceed the pool's unchanged 4 MiB cap and keep valid passing evidence.

Two additional complete managed-DLL diagnostics use the existing full-word input:
513×259→513×129 independently improves **34.22% /
381.16 µs**. The horizontal
513×259→256×259 CPU effect is inconclusive, while allocations fall about
**100,136→44,712 B/op** and retained preparation **102,000→46,656 B**.
Its governed one-shot peak falls **632,432→577,088 B**. Vertical allocation falls
about **267,016→254,672 B/op**, retained **269,364→257,100 B**, and peak
**798,780→786,516 B**. Ranges and raw observations remain in the report.
Those diagnostics establish scoped prior/candidate effects. Both [one-axis
comparisons](../benchmarks/LANCZOS-IDENTITY-AXIS-REFERENCE.md) retain their
complete one-shot boundaries and original quality checks.
Managed GC ranges **44,712–44,720 / 254,672–254,679B/op** remain separate from
retained plans and governed peaks. All ten Lanczos views pass/stop; no stage-cost
sum or broader grid is implied. The full289-path objective stays open.
Windows/Linux each pass **6,742 full/live**, **163 internal/15 public** focused
normal/scalar/AVX512-disabled checks, the three named resize-storage suites and
the SAME actual package/all three examples, including one-axis asynchronous
survivors. Allocation, retained admission and physical peaks stay distinct.
[Horizontal JPEG preparation](../benchmarks/JPEG-HORIZONTAL-PREPARATION.md) saves
**4.03% / 16.92 µs** in controlled complete-decode pairs, with unchanged **20,944 B/op**.
The [current-package JPEG measurements](../benchmarks/JPEG-DECODE-CURRENT-PACKAGE.md)
cover all **24** original owned-output cases affected by the shared
output pool. **23** pass the conservative gate. Adjacent native sampling
confirms horizontal CMYK, with the earlier adverse capture retained.
The 13 displayed comparisons use these timings and allocations. Caller-buffer
and coding-only paths keep their source-valid evidence.

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

[JPEG pending publication](../benchmarks/JPEG-PENDING-PUBLICATION.md) keeps typed
suspension construction outside immediate parsing. Final complete async-probe
pairs save **2.56% /
5.57 ns**, without a uniform or cumulative
gain claim. JPEG sync now passes/stops (upper
**0.962**). Async remains uncertain after
[one longer integration](../benchmarks/JPEG-PROBE-LONGER-INTEGRATION.md):
**0.219/0.210 µs, upper
1.121**. Both actual counts increase fourfold; original
warmups, twelve units/group, sixteen subdivisions and every observation remain.
Its adverse wider interval is retained; unchanged-case captures stop.
Complete headers pass; their slower historical point is retained, while the matched
original-worker guard is neutral/inconclusive. No favorable recapture follows.
Immediate probes/header calls allocate **568/8,768 B**. Original compiled loops,
every observation and descriptor/source/owner gates remain.
The immediate parser frame shrinks **1,080→472 B**; this is not a whole-operation
physical peak. Admission stays **512/8,448 B**, with no new retained workspace.
Gated-read diagnostics stay **1,960/9,984 B**, including equal delivery/bridge/batch
overhead, not standalone production allocation. Each OS passes **6,844 full/live
checks**, **11 internal/182 public** normal/scalar checks and the SAME actual
package/three examples, without rebuilding the timed DLL. Unchanged PNG/WebP
probes/header and other families retain their identified evidence; the final
complete package refresh remains pending. [Earlier owner work](../benchmarks/PROBE-OWNER-COMPLETION.md)
and [reference interleaving](../benchmarks/JPEG-PROBE-ORIGINAL-LOOP-REFERENCE.md)
retain history.
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
[Shared scan-band coordinates](../benchmarks/TEXT-SCAN-BAND-COORDINATES.md)
save **2.555 µs / 5.33%**
in controlled complete ordinary-RGBA8 final-DLL pairs. Standalone RGBA8 now passes,
upper **0.983139**.
[Interleaved RGBA16 references](../benchmarks/ORDINARY-WORD-INTERLEAVED-REFERENCE.md)
resolve the affected word comparison as **pass**,
interval **0.915603–0.965760**.
The same 36 native actuals now span the same twelve pairs; the earlier grouped
uncertainty and all samples remain retained. This is a scheduling correction on
unchanged production, not a historical-clock speedup. Both ordinary depths stop.
Original exact frames,
stock reference identities, symmetric per-operation reset/checks, all twelve
16,384-operation actuals and three reference groups remain unchanged.
Ordinary public allocations are **83,008 B/draw**, up **7,168 B**, plus 24 B of
adapter storage; reference allocations remain **48,576 B/draw**. Clipped controls
retain their approved source-corrected references and both pass.

Current complete one-shot and preparation-plus-eight-draw integrations also pass,
including all public layout calls, 30 fitting trials, fresh explicitly cleared
owned canvases, independent results and disposal. These are complete public-API
integrations, not bare shaping-engine comparisons. The measured batch amortizes to
**154.27 / 847.33 µs/draw**.
Preparation-only **1113.89 / 6032.00 µs**
is diagnostic; historical stages are never summed. Reference preparation retains
positioned paths; managed preparation retains full 16-bit coverage. Validation
eagerness and GC-driven reference font release differ. Reference one-shot/batch
allocations are **9,256,953 /9,494,633 B**. Latin/fit retained plans remain
**2,112 /4,160 B**. Full-fit governed peak is **748,984 B**, up **6,144 B** for
temporary crossings. Total allocations, peak admitted capacity, retained state
and unmeasured physical/native/JIT peaks are separate.

[Batched font mapping validation](../benchmarks/FONT-MAPPING-GROUP-VALIDATION.md) saves
**0.505 µs / 3.63%** in controlled
complete final-DLL pairs. Current **13.394 / 13.668 µs**,
upper **0.986107**, passes/stops against the scoped **FreeType 2.13.2
copy/open/retain/dispose** reference at **176 B/op**. The original borrowed face is
757,076 B; managed owned backing is 678,016 B, native owned input stays 757,076 B.
Managed live admission is 678,272 B; default idle storage is separate. Original
directory/input limits remain; owned tables retain exact offsets/bytes. Eager/lazy
validation differs, not general parser equivalence or an overall text slowdown. Unchanged [prepared ordinary drawing](../benchmarks/TEXT-PREPARED-COVERAGE.md)
passes/stops at **56 B/op** with its distinct retained-state boundary.

Windows/Linux each pass **6,839 full/live checks**, focused **322 internal/222 public**
normal/AVX2-disabled/scalar checks and the SAME actual mapping-validation package/all three
examples. Affected performance reconciles before qualification; no timed DLL
rebuild occurs. Other families retain identified unchanged-path evidence. The
complete final-package refresh remains pending.

RGBA16 compositing uses **ImageSharp Rgba64 SrcOver**, not FFmpeg. Keep the approved
[visual/native bounds](../benchmarks/TEXT-DRAWING-CURRENT.md) distinct from exact
managed pixels. Complete encoder packets and narrow metadata exceptions retain
their [declared scope](codec-parity.md); equal quality numbers are not quality matching.

**Codecs**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Packet B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| WebP VP8 + alpha, fresh seekable Stream (parity confirmed) † | 6,082.74 | 6,494.10 | 0.937 / 0.947445 | 240,496 | — |
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
| PNG RGBA8 decode, live token † (pass) | 1,334.01 | 1,395.37 | 0.956 / 0.962890 | 4,472 | — |
| PNG RGBA8 decode, tight working limit † (pass) | 1,240.93 | 1,505.84 | 0.824 / 0.829439 | 68,224 | — |
| PNG RGBA16 decode, tight working limit † (pass) | 2,796.05 | 2,904.45 | 0.963 / 0.979774 | 70,293 | — |
| PNG predictable RGBA16 3000², Optimal6 / mixed6 † | 259,795.57 | 388,008.23 | 0.670 / 0.685 | 2,491,252 | 573,041 / 573,041 |
| PNG RGBA16, one-shot Fastest1 / native1 † | 17,266.79 | 18,600.99 | 0.928 / 0.934 | 2,432,924 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, live cancelable token † | 17,573.86 | 18,607.98 | 0.944 / 0.954 | 2,432,924 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, forward-only output † | 17,196.47 | 18,607.32 | 0.924 / 0.929 | 2,432,956 | 968,216 / 968,216 |
| PNG RGBA8 async, one-shot Optimal6, nonseekable output † | 17,544.42 | 18,428.84 | 0.952 / 0.978328 | 877,476 | 210,973 / 210,973 |
| JPEG baseline420, resident decode † | 718.85 | 1,088.64 | 0.660 / 0.671216 | 258,456 | — |
| JPEG DC-only, caller CMYK decode † | 322.49 | 331.83 | 0.972 / 0.994 | 1,085,981 | — |
| VP8L RGBA8, resident decode † | 2,951.16 | 3,269.16 | 0.903 / 0.907616 | 152,728 | — |
| VP8L RGBA8, fresh seekable Stream decode † | 2,942.95 | 3,283.26 | 0.896 / 0.902094 | 152,824 | — |
| VP8L palette16 RGBA8, resident decode † | 408.78 | 544.44 | 0.751 / 0.761869 | 288,464 | — |
| JPEG CMYK sparse-2D, resident decode (confidence open) † | 459.79 | 465.71 | 0.987 / 1.001969 | 20,944 | — |
| JPEG YCCK420, resident decode † | 460.94 | 778.26 | 0.592 / 0.600071 | 895,562 | — |
| JPEG 4:1:1, resident decode † | 410.15 | 489.45 | 0.838 / 0.849040 | 219,551 | — |
| JPEG 4:1:0, resident decode † | 364.25 | 553.58 | 0.658 / 0.669676 | 189,090 | — |
| VP8L RGBA16, including normalization / libwebp exact=1 † | 114,600.56 | 131,281.45 | 0.873 / 0.887 | 5,948,664 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 112,507.39 | 131,039.53 | 0.859 / 0.869 | 5,949,048 | 183,812 / 183,812 |
| JPEG caller RGB24 async nonseekable, 420 (parity confirmed) † | 1,422.33 | 1,626.70 | 0.874 / 0.878 | 309,523 | 52,091 / 52,093 |
| JPEG RGBA8 async nonseekable, 444 (parity confirmed) † | 3,106.93 | 3,858.03 | 0.805 / 0.810 | 820,502 | 133,039 / 133,041 |
| PNG8 decode, two retained images / shared caller root † | 2,652.54 | 2,849.73 | 0.931 / 0.972152 | 1,638,214 | — |
| JPEG RGBA8 4:2:0, row restart33 (parity confirmed) † | 1,339.28 | 1,609.15 | 0.832 / 0.841 | 151,180 | 58,010 / 58,012 |
| JPEG dense CMYK, resident decode † | 1,310.33 | 3,113.11 | 0.421 / 0.433833 | 20,944 | — |
| JPEG horizontal-only CMYK, resident decode † | 412.52 | 425.40 | 0.970 / 0.980020 | 20,948 | — |
| JPEG YCCK420, caller RGB24 decode † | 438.99 | 604.73 | 0.726 / 0.742 | 762,720 | — |
| JPEG progressive grayscale, resident decode † | 1,714.69 | 1,845.30 | 0.929 / 0.935937 | 151,432 | — |
| JPEG progressive420, resident decode † | 1,428.34 | 1,964.55 | 0.727 / 0.744485 | 243,676 | — |
| JPEG progressive422, resident decode † | 1,817.00 | 2,246.36 | 0.809 / 0.820470 | 315,496 | — |
| JPEG ordinary progressive444, resident decode † | 2,896.31 | 3,361.26 | 0.862 / 0.868653 | 169,664 | — |
| JPEG progressive444, live-token resident decode † | 3,276.96 | 3,644.66 | 0.899 / 0.905322 | 169,664 | — |

**Editing**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 256×129 | 591.66 | 1,574.50 | 0.376 / 0.390 | 138,650 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 256×129 | 459.28 | 488.52 | 0.940 / 0.947 | 138 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 256×129 | 632.28 | 1,823.31 | 0.347 / 0.355 | 155,078 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 256×129 | 497.44 | 512.29 | 0.971 / 0.984 | 141 |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 1027×519 | 3,779.99 | 5,260.25 | 0.719 / 0.723 | 319,224 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 1027×519 | 3,333.69 | 4,090.78 | 0.815 / 0.831 | 148 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 1027×519 | 4,080.93 | 5,074.41 | 0.804 / 0.808 | 4,592,818 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 1027×519 | 4,117.26 | 4,212.39 | 0.977 / 0.990 | 4,264,760 |
| Bicubic resize RGBA8 (confidence open) † → 256×129 | 414.12 | 416.74 | 0.994 / 1.024193 | 199,985 |
| Bicubic upscale RGBA16 † → 1027×519 | 2,630.58 | 2,872.22 | 0.916 / 0.923 | 4,264,573 |
| Bicubic RGBA8, width reduction / height enlargement, input+output+4 KiB † → 256×519 | 754.04 | 840.84 | 0.897 / 0.913422 | 113 |
| Bicubic RGBA16, width down / height up / input+output+4 KiB (confidence open) † → 256×519 | 820.10 | 825.87 | 0.993 / 1.007289 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 32×16 | 435.03 | 442.74 | 0.983 / 0.991529 | 113 |
| Bicubic RGBA8 extreme reduction, input+output+4 KiB † → 2×9 | 451.05 | 1,046.58 | 0.431 / 0.433 | 113 |
| Bicubic RGBA8 enlargement, input+output+4 KiB † → 1027×519 | 3,031.48 | 3,301.11 | 0.918 / 0.925615 | 116 |
| Bicubic RGBA8 width enlargement, input+output+4 KiB † → 1027×259 | 1,427.16 | 1,534.77 | 0.930 / 0.946599 | 114 |
| Bicubic RGBA8 height enlargement, input+output+4 KiB † → 513×519 | 1,562.10 | 1,830.43 | 0.853 / 0.864226 | 126 |
| Bicubic RGBA8 reduction, input+output+4 KiB (confidence open) † → 256×129 | 414.38 | 419.26 | 0.988 / 1.003866 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 256×129 | 495.34 | 569.89 | 0.869 / 0.873750 | 113 |
| Bicubic RGBA8, fresh managed / prepared zimg † → 128×64 | 337.24 | 349.38 | 0.965 / 0.971 | 198,350 |
| Bicubic RGBA8, width up / height down, input+output+4 KiB † → 1027×129 | 921.58 | 944.33 | 0.976 / 0.984461 | 114 |
| Bicubic RGBA16, width up / height down, input+output+4 KiB † → 1027×129 | 872.48 | 907.53 | 0.961 / 0.974925 | 114 |
| Bicubic RGBA16 extreme reduction, input+output+4 KiB † → 2×9 | 428.16 | 1,261.83 | 0.339 / 0.353 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 64×32 | 353.91 | 388.26 | 0.912 / 0.921 | 113 |
| Bicubic RGBA8 reduction, input+output+4 KiB (confidence open) † → 128×64 | 380.07 | 375.17 | 1.013 / 1.024773 | 112 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 128×64 | 405.66 | 448.29 | 0.905 / 0.918803 | 112 |
| Bicubic RGBA8, width down / height up, default workspace † → 256×519 | 810.11 | 902.40 | 0.898 / 0.906412 | 199,938 |
| Bicubic RGBA16, width down / height up, default workspace † → 256×519 | 732.70 | 825.62 | 0.887 / 0.898772 | 199,898 |
| Exact rectangle copy into borrowed RGBA8 canvas (gap) | 1.95 | 1.91 | 1.025 / 1.037 | 0 |
| Exact rectangle copy into borrowed RGBA16 canvas (confidence open) | 3.83 | 3.82 | 1.003 / 1.012 | 0 |
| Exact clipped fill into borrowed RGBA8 canvas (parity confirmed) | 1.24 | 1.57 | 0.789 / 0.793 | 0 |
| Exact clipped fill into borrowed RGBA16 canvas (parity confirmed) | 2.09 | 4.10 | 0.511 / 0.513 | 0 |
| Overlapping RGBA8 copy + reverse restoration, per pair (parity confirmed) | 3.88 | 4.18 | 0.929 / 0.938 | 0 |
| Original overlapping RGBA16 copy, 257×129, shift (1,1), per pair (parity confirmed) | 5.19 | 5.60 | 0.926 / 0.931 | 0 |
| RGBA8 source-over / FFmpeg overlay (parity confirmed) † | 65.01 | 451.06 | 0.144 / 0.150 | 0 |
| RGBA16 source-over / ImageSharp 3.1.12 (parity confirmed) † | 563.63 | 629.51 | 0.895 / 0.913 | 0 |
| Original self-composite RGBA16 / ImageSharp snapshot + SrcOver (parity confirmed) † | 649.77 | 716.96 | 0.906 / 0.977 | 0 |
| Complete text one-shot / ImageSharp public integration (pass) † | 1,109.50 | 6,197.30 | 0.179 / 0.201 | 304,932 |
| Complete text preparation + eight draws / ImageSharp public integration (pass) † | 1,234.14 | 6,778.67 | 0.182 / 0.196 | 314,660 |
| Text ordinary draw RGBA8 / ImageSharp.Drawing (pass) † | 43.82 | 44.96 | 0.975 / 0.983 | 83,008 |
| Text ordinary draw RGBA16 / ImageSharp.Drawing (pass) † | 47.01 | 49.95 | 0.941 / 0.966 | 83,008 |
| Text prepared coverage RGBA8 / ImageSharp.Drawing (pass) † | 2.93 | 36.37 | 0.081 / 0.087 | 56 |
| Text prepared coverage RGBA16 / ImageSharp.Drawing (pass) † | 5.30 | 37.68 | 0.141 / 0.152 | 56 |
| Font loading / FreeType 2.13.2 (scoped pass) † | 13.39 | 13.67 | 0.980 / 0.986 | 176 |
| Wrapped logical layout / SixLabors public API (pass) | 6.07 | 96.16 | 0.063 / 0.097 | 3,153 |
| Latin OpenType layout / SixLabors public API (pass) | 20.40 | 57.62 | 0.354 / 0.717 | 6,409 |
| Arabic OpenType layout / SixLabors public API (pass) | 88.93 | 337.84 | 0.263 / 0.421 | 28,658 |
| Complete fitting search / SixLabors public API (pass) | 1,144.00 | 1,882.11 | 0.608 / 0.866 | 224,181 |
| Bilinear upscale RGBA8 † → 1027×519 | 1,964.35 | 1,990.85 | 0.987 / 0.992 | 102,891 |
| Bilinear RGBA16 enlargement, fresh managed / prepared FFmpeg graph (parity confirmed) † → 1027×519 | 1,847.74 | 2,024.23 | 0.913 / 0.944044 | 4,498,558 |
| Bilinear one-shot RGBA16 / FFmpeg one-shot † → 1027×519 | 1,973.05 | 5,139.83 | 0.384 / 0.434 | 4,498,574 |
| Bilinear resize RGBA8 † → 256×129 | 373.69 | 384.94 | 0.971 / 0.979 | 8,473 |
| Bilinear resize RGBA16 † → 256×129 | 388.37 | 418.68 | 0.928 / 0.937 | 8,473 |
| Bicubic RGBA8, width reduction / same height, input+output+4 KiB † → 256×259 | 490.21 | 576.13 | 0.851 / 0.862076 | 113 |
| Bicubic one-shot RGBA8 / zimg one-shot † → 128×64 | 350.31 | 990.90 | 0.354 / 0.357 | 198,455 |
| Bicubic prepared RGBA8 / zimg prepared † → 128×64 | 259.35 | 348.80 | 0.744 / 0.747 | 137 |
| Bicubic one-shot RGBA16 / zimg one-shot † → 128×64 | 395.89 | 599.92 | 0.660 / 0.677 | 198,453 |
| Bicubic prepared RGBA16 / zimg prepared † → 128×64 | 328.80 | 404.94 | 0.812 / 0.822 | 137 |
| Bicubic one-shot narrow RGBA8 / zimg one-shot † → 256×129 | 447.86 | 1,038.21 | 0.431 / 0.437 | 199,991 |
| Bicubic prepared narrow RGBA8 / zimg prepared † → 256×129 | 398.55 | 422.93 | 0.942 / 0.952 | 137 |
| Bicubic one-shot narrow RGBA16 / zimg one-shot † → 256×129 | 446.16 | 715.28 | 0.624 / 0.628 | 199,983 |
| Bicubic prepared narrow RGBA16 / zimg prepared † → 256×129 | 373.53 | 570.51 | 0.655 / 0.662 | 137 |
| Lanczos3 one-axis one-shot RGBA16 / zimg one-shot † → 256×259 | 611.14 | 2,088.67 | 0.293 / 0.301 | 44,712–44,720 |
| Lanczos3 one-axis one-shot RGBA16 / zimg one-shot † → 513×129 | 734.50 | 2,054.59 | 0.357 / 0.363 | 254,672–254,679 |
| PNG RGBA16 one-shot stream decode/orient/crop/fit / FFmpeg one-shot † → 127×64 | 2,856.94 | 3,301.31 | 0.865 / 0.871 | 338,156–338,160 |
| Plain transpose RGBA16 † → 259×513 | 42.05 | 47.86 | 0.879 / 0.891 | 56 |
| Plain transpose RGBA8 † → 259×513 | 29.15 | 41.05 | 0.710 / 0.722 | 56 |
| Counterclockwise quarter turn RGBA8 † → 259×513 | 28.50 | 38.03 | 0.749 / 0.756 | 56 |
| Counterclockwise quarter turn RGBA16 † → 259×513 | 35.86 | 44.67 | 0.803 / 0.865 | 56 |
| Mirrored transpose RGBA8 † → 259×513 | 30.69 | 37.42 | 0.820 / 0.842 | 56 |
| Half turn RGBA8 † | 8.71 | 10.87 | 0.801 / 0.840 | 56 |
| Vertical flip RGBA8 (parity confirmed) † | 8.92 | 9.15 | 0.975 / 0.988 | 56 |
| Original clipped RGBA8 fill, 254×129 (parity confirmed) | 1.195 | 1.730 | 0.690 / 0.696649 | 0 |
| Original clipped RGBA16 fill, 254×129 (parity confirmed) | 2.184 | 4.829 | 0.452 / 0.456739 | 0 |
| Original clipped RGBA8 copy, 254×129 (gap) | 1.983 | 1.914 | 1.036 / 1.049530 | 0 |
| Original clipped RGBA8 composite / retained FFmpeg (parity confirmed) | 78.567 | 526.201 | 0.149 / 0.159260 | 0 |
| Authored default-variation layout / ImageSharp public integration (parity confirmed) | 3.551 | 20.357 | 0.174 / 0.184945 | 3,608 |
| Authored Unicode layout / corrected ImageSharp integration (parity confirmed) | 2.585 | 117.234 | 0.022 / 0.023246 | 5,592 |
| Authored cmap4 layout / corrected ImageSharp integration (parity confirmed) | 2.610 | 120.738 | 0.022 / 0.022768 | 5,088 |
| Authored compound layout / corrected ImageSharp integration (parity confirmed) | 2.604 | 112.021 | 0.023 / 0.023809 | 6,152 |
| Authored RGBA8 layout + draw / corrected ImageSharp integration (parity confirmed) | 37.325 | 225.481 | 0.166 / 0.289477 | 44,136 |
| Authored RGBA16 layout + draw / corrected ImageSharp integration (parity confirmed) | 34.925 | 226.992 | 0.154 / 0.265146 | 44,136 |

**Inspection and pixel-coding validation**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Probe PNG dimensions, seekable | 0.109 | 0.122 | 0.888 / 0.922 | 568 | Confirmed pass |
| Probe JPEG dimensions, seekable | 0.184 | 0.194 | 0.949 / 0.962 | 568 | Confirmed pass |
| Probe WebP dimensions, seekable | 0.103 | 0.121 | 0.853 / 0.884 | 568 | Confirmed pass |
| Probe JPEG dimensions, async nonseekable | 0.219 | 0.210 | 1.045 / 1.121 | 568 | Confidence open |
| Validate PNG8, span † | 1,259.842 | 1,507.479 | 0.836 / 0.845441 | 240 | Confirmed pass |
| Validate PNG16, span † | 2,931.938 | 3,250.048 | 0.902 / 0.910041 | 240 | Confirmed pass |
| Validate indexed Adam7 19×17, span † | 1.986 | 4.310 | 0.461 / 0.474500 | 792 | Confirmed pass |
| Validate JPEG baseline420, span † | 483.057 | 674.268 | 0.716 / 0.720 | 10,016 | Confirmed pass |
| Validate JPEG progressive420, span † | 288.749 | 297.280 | 0.971 / 0.984 | 4,088 | Confirmed pass |
| Validate VP8L+alpha, span † | 2,314.233 | 3,046.809 | 0.760 / 0.767 | 297,070 | Confirmed pass |
| Validate VP8+alpha, span † | 4,331.362 | 5,454.375 | 0.794 / 0.797 | 15,600 | Confirmed pass |
| Validate PNG16, async nonseekable † | 3,007.609 | 3,922.893 | 0.767 / 0.778735 | 360 | Confirmed pass |
| Inspect PNG8 complete headers, seekable, adapter reference | 0.291 | 0.580 | 0.501 / 0.509 | 568 | Confirmed pass |
| Identify PNG8 complete container, resident, adapter reference | 6.733 | 13.068 | 0.515 / 0.520 | 96 | Confirmed pass |
| Identify JPEG420, resident, canonical adapter | 0.497 | 1.181 | 0.421 / 0.431 | 96 | Confirmed pass |
| Identify VP8L+alpha, resident, parser adapter | 0.022 | 5.039 | 0.004 / 0.005 | 96 | Confirmed pass |
| Identify PNG8, fresh seekable Stream, adapter reference | 8.802 | 15.951 | 0.552 / 0.555 | 192 | Confirmed pass |
| Identify PNG8, async nonseekable, adapter reference | 11.801 | 19.297 | 0.612 / 0.621 | 216 | Confirmed pass |
| JPEG Adobe CMYK horizontal AC / FFmpeg fresh contexts † | 411.596 | 831.503 | 0.495 / 0.498143 | 20,944 | Confirmed pass |
| Complete JPEG headers, fresh source / FFmpeg AVIO adapter (pass) | 2.188 | 2.813 | 0.778 / 0.844795 | 8,768 |
| Complete WebP headers, fresh seekable source / FFmpeg AVIO adapter (pass) | 0.142 | 0.310 | 0.458 / 0.471884 | 552 |

[Coverage and current family reports](../benchmarks/WARM-COVERAGE.md) identify
remaining paths, boundaries, allocations, references and raw evidence. Superseded
experiments remain linked separately. Update with `python benchmarks/render-benchmark.py`;
verify with `--check`.
