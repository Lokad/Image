# Lokad.Image benchmarks

Updated 2026-11-10. These **127 selected comparisons** combine qualified
**0.2.1 development** costs with retained **0.2.0** evidence. Full parity remains
open across the [283 required paths](../benchmarks/WARM-COVERAGE.md). **†** marks
development costs; **H** marks affected historical costs that are not current
performance proof. Publication is postponed.

All measurements use the dedicated quiet Ubuntu x64/AMD EPYC 9V45 VM, sequential
warm operations on one pinned CPU, .NET 10 with tiering off and nonconcurrent
workstation GC. Times are **µs per complete operation**. Ratios use the faster
native bracket; **upper** includes conservative filtered/all-actual confidence.
Short screens do not establish independent parity. **B/op** is total managed
allocation, separate from peak live capacity and retained storage. Native
allocations are unmeasured. Normalization, output ownership and disposal stay
timed; setup, delivery and retained state follow the stated case boundaries.

Current scope and remaining work:

- **WebP decoding:** [output-owned palette expansion](../benchmarks/WEBP-PALETTE-OUTPUT.md)
  removes a full-frame intermediate and copy. Complete palette16 confirmation saves
  **168.22µs/26.72% raw**, with separated filtered/all-actual raw/normalized confidence:
  461.31/native590.92µs, upper0.789 passes. Total GC falls820,190→288,462B/op;
  returned output remains independently charged until disposal, scratch retention0.
  An original513×67 palette control fits a239,128B shared bound below two full
  frames; this is neither the benchmark's measured peak nor a physical heap peak.
  Nine shared-decoder costs refresh. Six lossless controls now3pass/3uncertain;
  resident/live/stream uppers1.035/
  1.088/1.021 are uncertain, without causal gain/regression/equivalence claims.
  Three opaque VP8 controls reuse unchanged [frame-quantizer evidence](../benchmarks/VP8-FRAME-QUANTIZER-PREPARATION.md).
  Current combined VP8 scope:2pass/3gaps/1uncertain;
  resident alpha upper1.014 is uncertain; stream/validation pass.
  Live opaque remains4,881.47/native4,792.22µs,
  upper1.028gap,229,344B/op. Exact pixels, cancellation,
  shared admission,6,058full/live perOS, normal/scalar and SAMEactualpackage/
  threeexamples pass. Existing pools/governed bypass remain; physical/native peaks
  unmeasured. Passing cases stop. [Final publication](../benchmarks/VP8L-FINAL-PUBLICATION.md)
  retains preceding gains/adverse history; current palette evidence supersedes its
  palette regression. The newly displayed palette comparison was already required
  and measured:127selectedrows/283paths, without a new image or timing workload.
- **WebP encoding:** [Huffman workspace](../benchmarks/WEBP-HUFFMAN-WORKSPACE.md)
  reuses one operation-owned tree workspace/queue across alphabets and histogram
  groups. Complete screens save553,184B/photo,37,640B/diagram,668,512B/transparent
  alpha and112,632B/authored bars. Selected codes remain independently owned.
  Four admitted peaks remain3,470,059/3,251,328/3,656,309/312,716B; retained scratch0.
  All eight affected costs refresh on the actual-package DLL:4pass/4uncertain.
  Photograph, opaque and both alpha cases remain uncertain; prior photo/alpha16
  passes are superseded. A [current-package photograph confirmation](../benchmarks/WEBP-LOSSLESS-PHOTOGRAPH-CONFIDENCE.md)
  also remains uncertain; the selected table retains the source-valid workspace
  stage comparison. No causal CPU-gain/regression/equivalence or physical-peak
  claim; adverse short points and later bracket variation remain retained.
  Normal/scalar and6,026full/live eachOS plus SAMEactualpackage/threeexamples pass.
  No worker/native rebuild or completed actual is repeated. [Earlier clustering](../benchmarks/WEBP-CLUSTERING-STORAGE.md)
  and [matching reuse](../benchmarks/WEBP-MATCHING-POPULATION-STORAGE.md) retain history.
  [Rounded-border prediction](../benchmarks/VP8-ROUNDED-BORDER-PREDICTION.md)
  saves **112.73µs/4.27% raw**,
  4.55% reference-normalized, on complete effort2.
  Both filtered/all-actual confidence views separate. GC remains406,705B/op;
  no new owned heap capacity or retained state. Current
  2,525.56/native2,511.68µs,
  upper1.013 is uncertain, not a parity pass or demonstrated gap.
  Six unchanged encode costs reuse [histogram-stage evidence](../benchmarks/VP8-FOUR-BLOCK-HISTOGRAM.md);
  the alpha pair uses the current storage-stage costs above.
  shared RDO/decoder prediction code is untouched. Combined lossy scope:
  5pass/0gaps/4uncertain.
  Exact complete packets remain unchanged; passing controls stop.
- **PNG:** [ordinary/slab cursor publication](../benchmarks/PNG-NORMAL-CURSOR-PUBLICATION.md)
  removes exception-only publication of abandoned private state. Completed cursors,
  public errors/partial stores, lookahead, polls and owner cleanup remain exact.
  Tight/default RGBA16, RGB16 and tight-live RGBA16 save **192/125/216/217µs**;
  all four former gaps now pass. The two-image shared-root operation saves260µs:
  2610.86/native2847.04µs, upper0.920, passes. Filtered/allactual raw/normalized
  intervals separate. Its admitted peak1,766,641B, retained pair1,062,936B and
  GC1,638,214B/op remain unchanged; all51 inventory allocations are unchanged.
  [Current framing controls](../benchmarks/PNG-CURRENT-FRAMING-COSTS.md):
  51 confirmed controls, 49 pass/stop,
  1 gap and 1 uncertain.
  The publication stage refreshed43 affected costs and retained eight unchanged
  providers. The current encoding change preserves these decode paths.
  Live materialized RGBA16 remains2919.94/native2894.87µs upper1.012, a gap;
  liveRGBA8 upper1.004 remains uncertain. No favorable resampling or global parity
  claim. Header-only work is unchanged; older stream/composition
  costs remain historical. Physical/native peaks are not remeasured. Earlier
  [prefix](../benchmarks/PNG-PREPARED-PREFIX.md) and
  [live-cursor](../benchmarks/PNG-LIVE-PREPARED-CURSOR.md) gains remain linked history.
  [Exact length classification](../benchmarks/PNG-LENGTH-CLASSIFICATION.md) replaces
  repeated range search with the exact integer-logarithm formula. All15 current
  one-shot encodes refresh: **13 pass/stop, 0 gaps,
  2 uncertain**. WordSmallest now passes. ByteFastest
  upper1.028 and Gray8 upper1.047 remain uncertain; resolve before assuming tuning.
  WordFastest saves180.09µs/1.00% raw with separated filtered/allactual intervals;
  normalized intervals overlap. Byte's84.29µs point gain has allactual uncertainty.
  A [current-package Fastest confirmation](../benchmarks/PNG-FASTEST-CURRENT-CONFIDENCE.md)
  remains uncertain; its updated cost is displayed. No unchanged favorable repeat.
  No robust normalized or blanket gain claim. Exact packets,384KiB admission,
  retained0scratch and primaryGC820916/2437660Bop remain. Earlier
  [streaming census](../benchmarks/PNG-STREAMING-TOKEN-FREQUENCIES.md),
  [lazy](../benchmarks/PNG-LAZY-VIEW-PREPARATION.md) and
  [greedy preparation](../benchmarks/PNG-GREEDY-VIEW-PREPARATION.md) retain history.
- **Resizing:** [shared packed byte/word execution](../benchmarks/PACKED-BYTE-WORD-CONSOLIDATION.md)
  removes the separate word scheduler and reuses decoded source products across
  four output columns. Complete word64×32 saves **43.16µs/8.26% raw**,
  8.09% normalized; both confidence views separate.
  Current word479.19/zimg387.34µs,
  upper1.262; byte485.70/zimg367.42µs,
  upper1.329. Both remain gaps,113B/op each.
  Byte's earlier485.40µs point is context, not an equivalence claim. Same4KiB
  admission, phase payloads3880/3992/4044B, independently owned8192/16384B outputs,
  scratch0; physical/native peaks unmeasured. Exact publishers/repair remain.
  Initial1160.25µs adverse stage and the actual-code span-view attribution are retained;
  revision prepares borrowed readonly views once. Complete managed preparation and
  fresh output stay timed; native zimg graph/scratch stay retained and labeled.
  Both existing controls refresh, no new benchmark grid. Earlier
  [word confirmation](../benchmarks/CURRENT-PACKED-WORD-RESIZE.md) and
  [geometry-only study](../benchmarks/PACKED-WORD-GEOMETRY-EXPERIMENT.md) retain history.
  [Shared original-order byte repair](../benchmarks/BICUBIC-SHARED-BYTE-REPAIR.md)
  retains the large managed-only compact643.41→30.78ms/20.90× gain, exact pixels
  and4KiB admission as history. The existing large control refreshes on this package:
  default24.05/compact29.68ms, three actuals each,
  with overlapping intervals. No useful generalized CPU gain, regression,
  equivalence, native parity or default promotion is established. Its source and
  normal/scalar/prior full frames agree. Dispatch is unchanged. Earlier costs and
  adverse stages remain historical; current overlapping
  execution costs are stated below. Certificates, tiny-alpha unwind and owners remain.
  [Packed byte source preparation](../benchmarks/PACKED-BICUBIC-DECODED-PREFIX.md)
  saves **544.63→485.40µs /59.23µs/10.88%raw/10.11%normalized**, with separated
  filtered/all-actual raw/normalized confidence. Current zimg370.61µs, upper1.314
  leaves115µs;113B/op unchanged. Four-column prefix and original two-column
  suffix share one numeric range and4KiB admission; no new owned capacity or
  retained scratch. Predecoded doubles preserve exact products/tap order and
  original certificates. The adverse/flat transitional stages remain in the report.
  [Shared cached-wide windows](../benchmarks/CACHED-WIDE-SHARED-WINDOWS.md)
  reuse the qualified ordered horizontal kernel after once-per-operation geometry
  proof and checked fixed-size weight views. Complete Wide8 saves
  **18.69µs/4.19% raw/4.07% normalized**,
  with separated filtered/all-actual raw/normalized confidence. One original-only
  check resolves observed native377→350µs movement; the completed candidate and
  every earlier sample remain. Native movement in the selected comparison is
  -0.12%. No further primary repeats.
  Current427.76/zimg348.46µs,
  upper1.234 remains a gap. Eight existing support/depth controls
  refresh:7pass/1gap; no earlier pass is lost. Other points are fresh costs,
  not causal companion gains. Workspace198,432B/result32,768B/scratch0 remain;
  primary GC198,448Bop is unchanged apart from amortization.
  Fixed views remove the dynamic loop length reload/checks; a constant index guard
  remains. Instruction structure alone proves no CPU gain or physical peak.
  [Earlier preparation](../benchmarks/CACHED-WIDE-PREPARED-GEOMETRY.md) retains its
  scoped gain/raw history, without supplying displayed costs or renderer dependencies.
  [Pixel preparation](../benchmarks/CACHED-WIDE-PIXEL-PREPARATION.md) retains prior history.
  [Current tight-byte enlargement](../benchmarks/BYTE-BICUBIC-ENLARGEMENT-CURRENT-PACKAGE.md)
  is **3,114.50 / zimg3,041.43µs**, upper1.030 and116B/op:73µs remains. The
  qualified package and frozen workload are reused; no causal historical gain
  is claimed. Shared column prefix/suffix frames retire before overlap/replay;
  current cross-platform and SAME actual-package checks cover both depths.
  Other source-valid paths reuse evidence; scoped costs do not prove all-path parity.
  [Mixed-byte vertical reuse](../benchmarks/BICUBIC-MIXED-BYTE-VERTICAL-REUSE.md)
  shares overlapping decoded rows across independent ordered accumulations.
  Complete bounded mixed resize saves **145.36µs/14.06% raw/13.54% normalized**,
  with separated filtered/all-actual confidence and native movement
  -0.61%.
  Current default/bounded byte costs936.03/
  888.37µs, uppers
  1.084/1.030
  remain gaps; vertical upper0.786 passes/stops.
  Other fresh points establish no causal companion gain. GC114/114/113Bop,
  existing4KiB admission,28,136B result-backed workspace and retained scratch0
  remain. Outputs stay charged until disposal; exact pixels/certification/repair
  and native float gates are unchanged. The tested word extension had a54.32µs
  separated raw loss; its normalized point was inconclusive. The complete word
  implementation is restored byte-for-byte, with every adverse sample retained.
  [Earlier byte bands](../benchmarks/BICUBIC-BYTE-RESULT-BANDS.md) retain their
  scoped gains and artifact reconciliation as history, without a renderer dependency.
  [Direct identity-axis cache rows](../benchmarks/BICUBIC-IDENTITY-CACHE-ROWS.md)
  close byte/word vertical reduction: **636.49→530.97µs / 16.58% raw / 16.50%
  normalized** and **666.59→511.76µs / 23.23% raw / 21.26% normalized**.
  Filtered/all-actual confidence separates; parity uppers0.908/0.904.
  The previous vertical-word regression is superseded. These identity paths remain
  source-valid; affected narrow cached costs now belong to shared overlap below.
  Identity workspace383,852→367,436B; total allocation367,675/367,595Bop,
  retainedscratch0/result264,708/529,416B until disposal. Exact repair's4KiB
  overlap stays separately admitted. Physical/native peaks are unmeasured;
  existing output-pool retention remains separate. No new API/owner/cache/pool.
  Native zimg retains graph/input/scratch; managed one-shot preparation is timed.
  Both OSes pass5,938full/live, affected normal/scalar/disabled-ISA checks and
  the SAME actual package/three examples. Four displayed rows refresh.
  [Result-backed word bands](../benchmarks/BICUBIC-RESULT-BACKED-BANDS.md) remain
  source-valid. Default mixed841.53µs,
  upper0.997 passes/stops. The newer unchanged-DLL bounded
  confirmation is893.16/zimg851.29µs,
  upper1.056 remains a gap; no causal historical regression is claimed.
  Fifteen small-support word controls: 11pass/
  4gap/0uncertain.
  Earlier band gains/tradeoffs and raw proof remain in the report.
  [Shared wide overlap](../benchmarks/BICUBIC-WIDE-OVERLAP.md) unifies fixed8tap/+2
  and16tap/+4 arithmetic plus byte-offset preparation across three schedules.
  Complete wide-byte saves **30.65µs/6.72% raw/6.68% normalized**,
  with separated filtered/all-actual raw/normalized confidence and stable reference.
  Current425.28/zimg346.96µs,
  upper1.233 remains a gap. All10 affected costs refresh:
  **7pass/3gaps/0uncertain**; wide-word now passes and no earlier pass is lost.
  Other point differences are fresh costs, not causal companion gains. Passing
  cases stop. Earlier [overlap gains/adverse evidence](../benchmarks/BICUBIC-SHARED-OVERLAP.md)
  stay linked history. No new owned capacity, pool/session or retention:
  bounded workspace4KiB/113Bop, cached byte199,840B workspace/199,981Bop.
  Wide results retain32,768/65,536B and smaller-support results132,096/264,192B
  until disposal; operation scratch0. Physical/native/total-stack peaks are unmeasured.
  [Certified Lanczos word rows](../benchmarks/LANCZOS-WORD-CERTIFIED-ROWS.md)
  reuse existingUInt32 products and remove integer filtering/conversion in the
  accelerated producer. Prepared word reduction saves25.02µs/4.34%raw/4.99%normalized,
  with separated filtered/allactual confidence. Enlargement's+14.87µs point has
  overlapping intervals; no uniform gain claim. All eight costs refresh on the
  actual-package DLL: fivepasses/three preparedgaps; retention139,800–333,396B
  unchanged, equivalent one-shot setup timed. Exact Q30 pixels/strict certificate/
  repair remain;6,017full/live eachOS, normal/scalar/AVX512off and SAMEpackage pass.
  [Earlier certificate preparation](../benchmarks/LANCZOS-PAIRED-CERTIFICATE-PREPARATION.md)
  retains history. [Streaming](../benchmarks/BICUBIC-SHARED-STREAMING.md)
  passes six of eight; small/wide-column gaps remain linked in coverage.
  [Byte enlargement](../benchmarks/BICUBIC-SHARED-ENLARGEMENT.md) remains
  3,108.21/native3,037.83µs,upper1.026/116Bop. [Packed bilinear rows](../benchmarks/BILINEAR-PACKED-ROW-STORAGE.md)
  save201.65µs/9.05% raw on complete public RGBA8 enlargement, with separated
  filtered/all-actual raw and normalized confidence. Current
  2,027.72/native2,165.52µs,
  lower0.815/upper0.993,
  102,891Bop passes this control. Existing admitted
  capacity and output pool are unchanged; no new cross-operation retention.
  Exact pixels,6,036full/live eachOS, normal/scalar/AVX512off and SAMEactualpackage
  pass. Geometry/output ownership stay timed; native graph/context preparation
  stays retained and labeled. Preserve exact Q30/rational/certified pixels versus
  native float; historical gains stay in linked reports.
- **JPEG and reading:** [live prepared Huffman decoding](../benchmarks/JPEG-LIVE-PREPARED-HUFFMAN.md)
  shares existing long-prefix lookup and scan-owned pair descriptors, preserving
  every logical cancellation checkpoint. Complete live baseline444 saves
  **201.15µs/11.18% raw**,
  11.14% normalized; filtered/all-actual confidence separates.
  Current1598.43/FFmpeg1718.09µs,
  upper0.935 passes/stops.
  GC10,120→42,936B/op; governed peak541,253→574,069B; result531,468B until disposal,
  retained scratch0. Physical/native peaks are unmeasured; no new pool/session.
  [Quarter interpolation](../benchmarks/JPEG-QUARTER-INTERPOLATION.md) removes duplicate
  horizontal reads/averages and zero-weight vertical blending within shared kernels.
  Complete4:1:1 saves **125.06µs/24.28% raw**,
  22.51% normalized; both confidence views separate.
  Current390.09/FFmpeg447.08µs, upper.885 passes/stops; shared4:1:0 remains passing,
  372.66/519.59µs, upper.752. GC219,551/189,088B/op; no new capacity or retained
  scratch, same1,536/1,792B render workspace and531,468B independently owned result.
  These are frozen DC-coded sampling controls, not general photographic speed claims.
  Both affected costs refresh;27 unchanged JPEG costs reuse valid evidence:
  **20pass/6gaps/3uncertain**.
  No previously passing affected control becomes a gap. RGB422 remains a gap;
  resident/stream restart420 and progressive live444 remain uncertain. Companion
  historical points establish no causal gain, nonregression or equivalence.
  Earlier adverse4:1:1 evidence remains historical after this source change. Exact frames,
  admission/ownership and private post-EOI interpretation remain unchanged.
  [Paired tile packing](../benchmarks/JPEG-PAIRED-TILE-PACKING.md) retains its scoped
  sparse-CMYK gain, inconclusive RGB comparison and adverse horizontal-AC evidence.
  [Narrow scan preparation](../benchmarks/JPEG-NARROW-SCAN-PREPARATION.md) retains
  progressive gains and source-valid costs. No new images/settings or repeated
  candidate confirmations. FFmpeg retains its contexts without an equivalent live token.
  [Grayscale tile preparation](../benchmarks/JPEG-GRAY-TILE-PREPARATION.md) shares
  row setup across up to eight MCUs, preserving exact normalization and gray samples.
  Complete RGBA→gray saves **299.16µs/20.49% raw**,
  19.96% reference-normalized, with separated
  filtered/all-actual confidence. Current 1,160.95/native1,403.16µs,
  upper0.840 passes the labeled SIMD libjpeg-turbo control;
  caller GRAY8 also passes upper0.969. Both retain exact58,910B
  packets; total GC is155,435/155,438B/op, without an allocation-gain claim.
  Extra768B stack storage fits the existing8KiB fixed admission; a conservative
  source bound including fixed heap/metadata and explicit arrays is4,804B.
  No new heap owner or retained state; full admitted/physical/JIT stack peaks
  are unmeasured. Only two costs refresh;15 unchanged color costs retain
  [Huffman replay evidence](../benchmarks/JPEG-PREPARED-HUFFMAN-BANK.md): combined
  16pass/0gaps/1uncertain. Existing coefficient pools and governed bypass stay unchanged.
  Strict JPEG metadata exceptions remain; passing cases stop. Restart/recomputed
  paths retain source-valid evidence. Older normalization/replay stages remain history.
- **Text/drawing:** [shared endpoint storage](../benchmarks/TEXT-ENDPOINT-STORAGE.md)
  omits repeated consecutive contour endpoints before event growth and sorting.
  Latin allocation falls67,368→59,000B/draw; admitted workspace falls48,544→42,336B,
  Arabic275,776→238,848B and fitting47,896→41,688B. Retained scratch stays zero;
  all eight frozen byte/word frames remain exact. Current98.44/102.57µs points
  improve13.74%/8.03% over contemporary originals, but raw/normalized intervals
  overlap: CPU gain remains inconclusive and both reference gaps remain.
  These complete prepared-layout draws include geometry/workspace/composition/
  disposal; resident font/layout and canvas resets are excluded equally. The
  labeled ImageSharp.Drawing2.1.7/Fonts2.1.3 integration retains public paths but
  rebuilds translated segments and scanner workspace per Fill. No managed work
  is removed from timing. Earlier [prepared pixels/order](../benchmarks/TEXT-PREPARED-PIXELS-AND-ORDER.md)
  retains history. No numerical-policy change or physical/native peak claim.
  [Shared sorting](../benchmarks/TEXT-RASTER-SHARED-SORT.md) retains the earlier allocation cut.
  [Layout/fitting and RGBA16 composition](../benchmarks/TEXT-DRAWING-CURRENT.md)
  pass/stop. These independent cases do not block codec or resize work.
  [Exact byte-vector composition](../benchmarks/BYTE-COMPOSITE-EXACT-VECTOR.md)
  saves492.39µs/88.24%raw (88.22%normalized):65.63/FFmpeg451.36µs, upper0.157
  passes the frozen RGBA8 control. Integer weights and a proven exact float
  division replace three scalar divisions per pixel. Allocation56B/op is refreshed
  on the actual package DLL, with no new owned workspace or retained scratch.
  Borrowed-canvas reset is excluded before every operation on both sides,
  invocation/unroll1, with exact managed and approved native gates. This is a
  scoped control result, not all-input parity; physical/native peaks are unmeasured.
  [Owned font storage](../benchmarks/FONT-OWNED-STORAGE.md) saves258.73µs/89.04%raw
  on complete loading:31.86µs versus labeledFreeType13.42µs, upper2.396 still a gap.
  Allocation falls757,597→176B/op; fullcopy/eager checks remain timed. The isolated
  load retains757,076B idle within the existing shared two-array/4MiB ceiling;
  governed calls bypass it. This is not a physical-peak reduction or parser-equivalence claim.

The current production build passes **5,907 full/live checks per OS**, affected
normal/scalar checks, and the **same actual Release package/all three examples
on Windows/Linux**, including exact mixed-alpha text at both depths, alpha and
opaque-word, progressive-strip, wide-column, shared byte/word enlargement and live VP8
sync/async consumers, plus bounded main validation and exact method2 residual storage/cancellation/recovery.
The package also verifies frozen vertical RGBA16 pixels at4/8KiB, aggregate
admission, terminalrefusal, cancellationcleanup and independent survivors.
Current resizing passes311 internal/729 public checks per normal/scalar/disabled-AVX2/AVX512 mode.
Current JPEG passes453 internal/375 public checks per those modes
on each OS, including actual live-window DC/EOB/following-read witnesses,
truncation/stuffing/partial failures, refusal/cancellation cleanup and survivors.
The current PNG package also verifies ordinary/live byte/word sync/async pixels,
terminal refusal, cancellation cleanup/recovery and independent survivors, plus
frozen Fastest8/16 sync/async packets,
shared admission and independent ownership. The current package also exercises
authored grouped-AC JPEG pixels in ordinary/live-token sync/async scans, terminal refusal and
independent survivors. Its live baseline witnesses also pass. Unchanged
alpha prefix/border/tail/palette, compressed horizontal-alpha package and other
coefficient/decoder/word/ISA evidence is reused.
Unchanged paths reuse valid evidence. This is correctness/package qualification,
not a full release refresh or whole-library performance pass. The retained
long-run cases are included in this full qualification. Original adverse and
uncertain points remain in the [green-prediction report](../benchmarks/WEBP-ISOLATED-GREEN-PREDICTION.md),
including the [earlier compact layouts](../benchmarks/WEBP-COMPACT-PREDICTOR-TRADEOFF.md).
Complete physical/admitted peaks are not inferred from allocations.

**References and boundaries.** FFmpeg is the reference except for explicitly
labeled **libwebp exact=1** lossless WebP (hidden RGB preserved), SIMD-enabled
**libjpeg-turbo** grayscale JPEG, and **zimg** resizing (FFmpeg's zscale backend).
The fifteen current one-shot PNG rows time fresh codec/zlib/packet state and independent
output on both sides, including native word-frame endian conversion/allocation/
disposal. Other native codec workers retain their documented wrappers/contexts.
Stream rows include declared acquisition, copies, EOF and release.

Text compares the complete **ImageSharp.Drawing 2.1.7/Fonts 2.1.3 public integration**,
including multiple layout calls and the full fitting search, not bare shaping.
Font load compares **FreeType 2.13.2 copy/open/retain/dispose**, with eager/lazy
validation differences; it does not establish general parser equivalence.
RGBA16 SrcOver compares **ImageSharp 3.1.12 Rgba64**, not FFmpeg performance.
[Scoped native/visual tolerances](../benchmarks/TEXT-DRAWING-CURRENT.md) retain
exact managed contracts and full 16-bit samples; visual gates are not sample
exactness. Complete packet claims, metadata exceptions and codec pixel contracts
retain their declared [case scope](codec-parity.md). Equal numerical quality
settings do not establish matched quality.

**Codecs**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Packet B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| WebP VP8 + alpha, fresh seekable Stream (parity confirmed) † | 5,868.76 | 5,999.20 | 0.978 / 0.995 | 238,960 | — |
| JPEG 4:4:4, forward-only / exact reference (parity confirmed) † | 3,492.70 | 3,776.95 | 0.925 / 0.931 | 820,120 | 133,039 / 133,041 |
| JPEG 4:2:0 from caller RGB samples (confidence open) † | 1,584.61 | 1,578.98 | 1.004 / 1.009 | 388,220 | 52,091 / 52,093 |
| JPEG RGBA16 4:2:0 / caller IMemoryBudget (parity confirmed) † | 1,706.53 | 1,836.80 | 0.929 / 0.934 | 819,475 | 52,115 / 52,117 |
| JPEG grayscale, standalone SIMD libjpeg-turbo (parity confirmed) † | 1,160.95 | 1,403.16 | 0.827 / 0.840 | 155,435 | 58,910 / 58,910 |
| JPEG from caller GRAY8, standalone SIMD libjpeg-turbo (parity confirmed) † | 1,059.14 | 1,112.88 | 0.952 / 0.969 | 155,438 | 58,910 / 58,910 |
| VP8L photograph / libwebp exact=1 † | 85,959.95 | 82,521.57 | 1.042 / 1.080 | 5,358,980 | 160,032 / 160,032 |
| VP8L text/diagram / libwebp exact=1 † | 7,441.59 | 9,575.22 | 0.777 / 0.791 | 3,721,041 | 7,234 / 7,234 |
| VP8 opaque RGBA8 † | 13,865.79 | 15,509.82 | 0.894 / 0.898 | 1,201,560 | 72,380 / 72,380 |
| VP8 opaque RGBA16, including normalization † | 13,882.59 | 15,579.01 | 0.891 / 0.894 | 1,201,560 | 72,380 / 72,380 |
| VP8 opaque photograph † | 9,630.85 | 9,657.98 | 0.997 / 1.001 | 809,233 | 17,688 / 17,688 |
| VP8 opaque text/diagram † | 5,476.34 | 5,803.20 | 0.944 / 0.952 | 741,196 | 7,100 / 7,100 |
| VP8 diagram, effort2 / distortion-only search † | 2,525.56 | 2,511.68 | 1.006 / 1.013 | 406,705 | 7,330 / 7,330 |
| VP8 diagram, effort6 / trellis † | 9,256.01 | 9,813.99 | 0.943 / 0.999 | 754,505 | 6,818 / 6,818 |
| VP8 RGBA8 + lossless alpha † | 54,544.68 | 53,351.23 | 1.022 / 1.065 | 6,242,608 | 98,590 / 98,590 |
| VP8 RGBA16 + alpha, including normalization † | 57,605.21 | 58,080.66 | 0.992 / 1.002 | 6,241,792 | 98,590 / 98,590 |
| VP8L RGBA8 / libwebp exact=1 † | 107,509.18 | 126,289.11 | 0.851 / 0.910 | 5,948,664 | 183,812 / 183,812 |
| VP8L opaque RGBA8 / libwebp exact=1 † | 74,977.27 | 75,349.65 | 0.995 / 1.013 | 4,823,168 | 121,896 / 121,896 |
| PNG RGBA8, one-shot SmallestSize9 / native9 † | 16,851.73 | 17,227.34 | 0.978 / 0.983 | 820,452 | 210,973 / 210,973 |
| PNG RGBA16, one-shot SmallestSize9 / native9 † | 20,549.38 | 20,868.88 | 0.985 / 0.991 | 2,437,660 | 963,202 / 963,202 |
| PNG RGBA8, one-shot Fastest1 / native1 † | 4,757.43 | 4,717.98 | 1.008 / 1.028 | 820,916 | 236,828 / 236,828 |
| PNG RGBA16, NoCompression0 / native0 † H | 203.45 | 406.60 | 0.500 / 0.539 | 1,741,468 | 1,066,552 / 1,066,552 |
| PNG grayscale8, one-shot Optimal6 / native6 † | 1,993.15 | 1,968.89 | 1.012 / 1.047 | 551,364 | 72,586 / 72,586 |
| PNG RGB8, one-shot Optimal6 / native6 † | 9,370.01 | 9,793.98 | 0.957 / 0.972 | 819,884 | 168,729 / 168,729 |
| PNG gray-alpha8, one-shot Optimal6 / native6 † | 4,162.99 | 4,444.97 | 0.937 / 0.955 | 552,332 | 120,366 / 120,366 |
| PNG RGBA8, one-shot Optimal6 / native6 † | 16,744.89 | 17,271.29 | 0.970 / 0.975 | 820,452 | 210,973 / 210,973 |
| PNG RGBA16, one-shot Optimal6 / native6 † | 20,261.52 | 20,732.37 | 0.977 / 0.983 | 2,437,660 | 963,202 / 963,202 |
| PNG RGB16, one-shot Optimal6 / native6 † | 15,330.07 | 16,071.79 | 0.954 / 0.963 | 2,431,492 | 739,472 / 739,472 |
| PNG grayscale16, one-shot Optimal6 / native6, red-plane reference † | 4,704.74 | 5,506.13 | 0.854 / 0.875 | 822,740 | 265,433 / 265,433 |
| PNG gray-alpha16, one-shot Optimal6 / native6 † | 9,874.42 | 10,487.03 | 0.942 / 0.950 | 1,360,900 | 506,887 / 506,887 |
| PNG RGBA8, stored0 / native0 † H | 253.82 | 189.65 | 1.338 / 1.471 | 1,203,724 | 533,444 / 533,444 |
| PNG RGBA8 decode, live token † | 1,391.93 | 1,393.91 | 0.999 / 1.004 | 4,472 | — |
| PNG RGBA8 decode, tight working limit † | 1,184.46 | 1,395.39 | 0.849 / 0.855 | 68,224 | — |
| PNG RGBA16 decode, tight working limit † | 2,826.77 | 2,894.02 | 0.977 / 0.981 | 70,272 | — |
| PNG predictable RGBA16 3000², Optimal6 / mixed6 † H | 352,397.34 | 407,457.02 | 0.865 / 0.870 | 3,402,572 | 573,041 / 573,041 |
| PNG RGBA16, one-shot Fastest1 / native1 † | 17,776.55 | 18,600.15 | 0.956 / 0.966 | 2,437,660 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, live cancelable token † | 18,197.48 | 19,178.63 | 0.949 / 0.954 | 2,437,660 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, forward-only output † | 17,856.45 | 18,591.70 | 0.960 / 0.966 | 2,437,692 | 968,216 / 968,216 |
| PNG RGBA8 async, one-shot Optimal6, nonseekable output † | 16,825.68 | 18,080.22 | 0.931 / 0.941 | 882,212 | 210,973 / 210,973 |
| JPEG baseline420, resident decode (confirmed pass) † | 833.31 | 1,008.40 | 0.826 / 0.834 | 258,450 | — |
| JPEG DC-only, caller CMYK decode † | 337.35 | 330.20 | 1.022 / 1.025 | 1,085,983 | — |
| VP8L RGBA8, resident decode (confidence open) † | 3,085.86 | 3,021.48 | 1.021 / 1.035 | 152,728 | — |
| VP8L RGBA8, fresh seekable Stream decode (confidence open) † | 3,290.70 | 3,268.51 | 1.007 / 1.021 | 152,824 | — |
| VP8L palette16 RGBA8, resident decode (confirmed pass) † | 461.31 | 590.92 | 0.781 / 0.789 | 288,462 | — |
| JPEG CMYK sparse-2D, resident decode † | 497.49 | 437.67 | 1.137 / 1.167 | 20,944 | — |
| JPEG YCCK420, resident decode (confirmed pass) † | 429.38 | 711.29 | 0.604 / 0.610 | 895,563 | — |
| JPEG 4:1:1, resident decode (confirmed pass) † | 390.09 | 447.08 | 0.873 / 0.885 | 219,551 | — |
| JPEG 4:1:0, resident decode (confirmed pass) † | 372.66 | 519.59 | 0.717 / 0.752 | 189,088 | — |
| VP8L RGBA16, including normalization / libwebp exact=1 † | 114,600.56 | 131,281.45 | 0.873 / 0.887 | 5,948,664 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 112,507.39 | 131,039.53 | 0.859 / 0.869 | 5,949,048 | 183,812 / 183,812 |
| JPEG caller RGB24 async nonseekable, 420 (parity confirmed) † | 1,581.70 | 1,629.27 | 0.971 / 0.979 | 309,524 | 52,091 / 52,093 |
| JPEG RGBA8 async nonseekable, 444 (parity confirmed) † | 3,517.84 | 3,856.40 | 0.912 / 0.918 | 820,531 | 133,039 / 133,041 |
| PNG8 decode, two retained images / shared caller root † | 2,610.86 | 2,847.04 | 0.917 / 0.920 | 1,638,214 | — |
| JPEG dense CMYK, resident decode (confirmed pass) † | 1,360.48 | 3,114.51 | 0.437 / 0.445 | 20,944 | — |
| JPEG horizontal-only CMYK, resident decode † | 433.42 | 392.27 | 1.105 / 1.142 | 20,944 | — |
| JPEG YCCK420, caller RGB24 decode (confirmed pass) † | 436.39 | 604.62 | 0.722 / 0.725 | 762,721 | — |
| JPEG progressive grayscale, resident decode (confirmed pass) † | 1,630.45 | 1,711.66 | 0.953 / 0.964 | 151,432 | — |
| JPEG progressive420, resident decode (confirmed pass) † | 1,559.16 | 1,948.18 | 0.800 / 0.809 | 243,664 | — |
| JPEG progressive422, resident decode (confirmed pass) † | 2,201.41 | 2,439.31 | 0.902 / 0.913 | 315,496 | — |
| JPEG ordinary progressive444, resident decode (confirmed pass) † | 3,172.92 | 3,355.21 | 0.946 / 0.949 | 169,664 | — |
| JPEG progressive444, live-token resident decode (confidence open) † | 3,338.76 | 3,355.58 | 0.995 / 1.012 | 169,664 | — |

**Editing**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 256×129 | 707.72 | 1,567.51 | 0.451 / 0.457 | 138,643 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 256×129 | 625.69 | 532.52 | 1.175 / 1.186 | 139 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 256×129 | 693.04 | 1,807.17 | 0.383 / 0.387 | 146,869 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 256×129 | 551.39 | 513.71 | 1.073 / 1.084 | 139 |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 1027×519 | 4,085.56 | 5,715.79 | 0.715 / 0.720 | 319,224 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 1027×519 | 3,492.08 | 4,067.38 | 0.859 / 0.865 | 148 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 1027×519 | 4,699.03 | 5,223.49 | 0.900 / 0.940 | 4,592,777 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 1027×519 | 4,021.02 | 3,903.37 | 1.030 / 1.043 | 4,265,332 |
| Bilinear resize RGBA8 † H → 256×129 | 504.78 | 387.47 | 1.303 / 1.315 | 157,971 |
| Bilinear upscale RGBA8 (parity confirmed) † → 1027×519 | 2,027.72 | 2,165.52 | 0.936 / 0.993 | 102,891 |
| Bilinear resize RGBA16 † H → 256×129 | 470.26 | 418.16 | 1.125 / 1.133 | 272,815 |
| Bicubic resize RGBA8 † → 256×129 | 460.84 | 418.20 | 1.102 / 1.106 | 199,981 |
| Bicubic upscale RGBA16 † → 1027×519 | 2,752.40 | 2,874.23 | 0.958 / 0.966 | 4,264,573 |
| Bicubic upscale RGBA8, default workspace † → 1027×519 | 3,108.21 | 3,037.83 | 1.023 / 1.026 | 116 |
| Bicubic RGBA16, width down / height up / input+output+4 KiB † → 256×519 | 862.45 | 825.16 | 1.045 / 1.049 | 114 |
| Bicubic RGBA8, 32×16 / input+output+4 KiB H → 32×16 | 464.68 | 375.02 | 1.239 / 1.370 | 2,185 |
| Bicubic RGBA8, 32×16 / default workspace † → 32×16 | 312.80 | 375.67 | 0.833 / 0.838 | 197,300 |
| Bicubic RGBA8 enlargement, input+output+4 KiB † → 1027×519 | 3,114.50 | 3,041.43 | 1.024 / 1.030 | 116 |
| Bicubic RGBA8 width enlargement, input+output+4 KiB † → 1027×259 | 1,546.70 | 1,524.36 | 1.015 / 1.066 | 114 |
| Bicubic RGBA8 height enlargement, input+output+4 KiB † → 513×519 | 1,811.37 | 1,745.56 | 1.038 / 1.061 | 115 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129 | 504.20 | 419.02 | 1.203 / 1.208 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 256×129 | 545.86 | 573.09 | 0.952 / 0.956 | 113 |
| Bicubic RGBA8 wider-support reduction † → 128×64 | 427.76 | 348.46 | 1.228 / 1.234 | 198,448 |
| Bicubic RGBA8, width up / height down, input+output+4 KiB † → 1027×129 | 888.37 | 866.88 | 1.025 / 1.030 | 114 |
| Bicubic RGBA16, width up / height down, input+output+4 KiB † → 1027×129 | 893.16 | 851.29 | 1.049 / 1.056 | 114 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 64×32 | 485.70 | 367.42 | 1.322 / 1.329 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 64×32 | 479.19 | 387.34 | 1.237 / 1.262 | 113 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 128×64 | 425.28 | 346.96 | 1.226 / 1.233 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 128×64 | 405.87 | 416.73 | 0.974 / 0.978 | 113 |
| Bicubic RGBA8, width down / height up, default workspace † → 256×519 | 782.07 | 840.35 | 0.931 / 0.936 | 199,943 |
| Bicubic RGBA16, width down / height up, default workspace † → 256×519 | 768.81 | 824.92 | 0.932 / 0.936 | 199,898 |
| Plain transpose RGBA16 † → 259×513 | 32.93 | 44.48 | 0.740 / 0.748 | 112 |
| Plain transpose RGBA8 † → 259×513 | 26.80 | 38.79 | 0.691 / 0.711 | 112 |
| Exact rectangle copy into borrowed RGBA8 canvas | 2.13 | 1.93 | 1.100 / 1.114 | 56 |
| Exact rectangle copy into borrowed RGBA16 canvas (confidence open) | 3.89 | 3.83 | 1.016 / 1.052 | 56 |
| Exact clipped fill into borrowed RGBA8 canvas (parity confirmed) | 1.19 | 1.57 | 0.759 / 0.775 | 56 |
| Exact clipped fill into borrowed RGBA16 canvas (parity confirmed) | 2.29 | 4.20 | 0.546 / 0.567 | 56 |
| Overlapping RGBA8 copy + reverse restoration, per pair (gap) | 4.18 | 3.78 | 1.106 / 1.152 | 112 |
| Font loading / FreeType 2.13.2 (gap) † | 31.86 | 13.42 | 2.375 / 2.396 | 176 |
| Wrapped logical layout / SixLabors public API (pass) | 6.07 | 96.16 | 0.063 / 0.097 | 3,153 |
| Latin OpenType layout / SixLabors public API (pass) | 20.40 | 57.62 | 0.354 / 0.717 | 6,409 |
| Arabic OpenType layout / SixLabors public API (pass) | 88.93 | 337.84 | 0.263 / 0.421 | 28,658 |
| Complete fitting search / SixLabors public API (pass) | 1,144.00 | 1,882.11 | 0.608 / 0.866 | 224,181 |
| Prepared text RGBA8 / ImageSharp.Drawing (gap) † | 98.44 | 50.45 | 1.951 / 3.155 | 59,000 |
| Prepared text RGBA16 / ImageSharp.Drawing (gap) † | 102.57 | 51.14 | 2.006 / 3.208 | 59,000 |
| Composite RGBA8 / FFmpeg (pass) † | 65.63 | 451.36 | 0.145 / 0.157 | 56 |
| Composite RGBA16 / ImageSharp 3.1.12 (pass) | 627.24 | 696.25 | 0.901 / 0.922 | 56 |
| Bicubic RGBA16 height reduction, input+output+4 KiB (pass) † → 513×129 | 486.02 | 593.16 | 0.819 / 0.824 | 113 |

**Inspection and pixel-coding validation**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Probe PNG dimensions, seekable | 0.158 | 0.123 | 1.284 / 1.328 | 744 | Confirmed gap |
| Probe JPEG dimensions, seekable | 0.326 | 0.194 | 1.681 / 1.736 | 744 | Confirmed gap |
| Probe WebP dimensions, seekable | 0.165 | 0.125 | 1.323 / 1.425 | 744 | Confirmed gap |
| Probe JPEG dimensions, async nonseekable | 0.404 | 0.201 | 2.007 / 2.052 | 744 | Confirmed gap |
| Validate PNG8, span † | 1,169.541 | 1,380.849 | 0.847 / 0.857 | 240 | Confirmed pass |
| Validate PNG16, span † | 2,768.710 | 2,962.790 | 0.934 / 0.938 | 240 | Confirmed pass |
| Validate indexed Adam7 19×17, span † | 1.760 | 3.939 | 0.447 / 0.463 | 792 | Confirmed pass |
| Validate JPEG baseline420, span † | 500.928 | 674.636 | 0.743 / 0.778 | 10,016 | Confirmed pass |
| Validate JPEG progressive420, span † | 288.749 | 297.280 | 0.971 / 0.984 | 4,088 | Confirmed pass |
| Validate VP8L+alpha, span † | 2,265.372 | 3,067.735 | 0.738 / 0.750 | 297,070 | Confirmed pass |
| Validate VP8+alpha, span † | 4,418.355 | 5,461.091 | 0.809 / 0.822 | 14,064 | Confirmed pass |
| Validate PNG16, async nonseekable † | 2,759.145 | 3,611.996 | 0.764 / 0.765 | 8,544 | Confirmed pass |
| Inspect PNG8 complete headers, seekable, adapter reference | 0.703 | 0.579 | 1.215 / 1.235 | 8,528 | Confirmed gap |
| Identify PNG8 complete container, resident, adapter reference | 13.079 | 13.049 | 1.002 / 1.005 | 96 | Confirmed gap |
| Identify JPEG420, resident, canonical adapter | 0.501 | 1.193 | 0.420 / 0.426 | 96 | Confirmed pass |
| Identify VP8L+alpha, resident, parser adapter | 0.022 | 4.985 | 0.004 / 0.005 | 96 | Confirmed pass |
| Identify PNG8, fresh seekable Stream, adapter reference | 19.537 | 15.929 | 1.226 / 1.232 | 192 | Confirmed gap |
| Identify PNG8, async nonseekable, adapter reference | 20.312 | 19.321 | 1.051 / 1.060 | 8,400 | Confirmed gap |

[Coverage and linked family reports](../benchmarks/WARM-COVERAGE.md) retain producer
identities, confidence decisions, ownership/correctness gates and reproducible raw
evidence. Historical experiment reports remain separate. Update with
`python benchmarks/render-benchmark.py`; verify with `--check`.
