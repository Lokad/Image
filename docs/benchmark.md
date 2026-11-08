# Lokad.Image benchmarks

Updated 2026-11-08. These **126 selected comparisons** combine qualified
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

- **WebP decoding:** [fused final inverse/publication](../benchmarks/VP8L-FINAL-PUBLICATION.md)
  saves **155.66µs/4.79% raw**,4.53% native-normalized, on complete resident VP8L.
  Both confidence views separate. GC stays152,728B/op; no new owned capacity/cache.
  Current resident/live/stream uppers1.034/
  1.043/1.027 remain gaps.
  Six unchanged lossless controls:2pass/4gaps. The contemporary palette
  control regresses **19.25µs/3.46%**, with separated raw/normalized intervals;
  retain that tradeoff, not a general decoder speedup claim.
  Validation passes/stop; source-valid coarse peaks235,152/11,121B and retained0
  are reused, not remeasured physical peaks. Full output retains531,468B until disposal.
  [Frame-private quantizers](../benchmarks/VP8-FRAME-QUANTIZER-PREPARATION.md)
  prepare six scalar dequantizers per segment once inside complete decoding,
  using96B within the existing8KiB allowance. Live opaque saves45.36µs/0.92%raw;
  raw confidence separates in both views, while0.86% normalized remains a point
  gain with all-actual uncertainty. Resident alpha saves110.85µs/1.83%raw/
  1.82%normalized with separated confidence. Contemporary originals resolve the
  resident historical-clock contradiction; no favorable repeats. Current resident
  5,961.70/native5,968.47µs,
  upper1.003 is uncertain. Stream upper0.980
  and validation upper0.811 pass/stop. Six current
  costs:2pass/3gaps/1uncertain.
  Live opaque remains4,881.47/native4,792.22µs,
  upper1.028gap,229,344B/op. All allocation counts
  are unchanged; no new heap/retention. Existing1,536B probability model and
  independently retained531,468B frames remain. Physical/native peaks unmeasured;
  logical fixed payload/coarse allowance5,108B fits8,192B. Syntax, every original
  poll, cursor/pixels, admission and actual async lifetimes remain. Both OSes pass
  5,955full/live, normal/scalar/disabled-ISA and SAME package/three examples.
  [Model rows](../benchmarks/VP8-MODEL-ROW-STRIDE.md),
  [earlier preparation](../benchmarks/VP8-PREPARED-COEFFICIENT-MODEL.md) and
  [alpha publication](../benchmarks/WEBP-HORIZONTAL-ALPHA-PUBLICATION.md)
  retain their historical gains and raw evidence separately.
- **WebP encoding:** [matching-to-population storage](../benchmarks/WEBP-MATCHING-POPULATION-STORAGE.md)
  saves134,736B/encode on photograph/diagram and70,560B on transparent alpha.
  Separate admitted peaks remain3,470,059/3,251,328/3,656,309B; retained scratch zero.
  Six unchanged lossless comparisons reuse that evidence; photograph/opaque
  remain uncertain. No causal CPU-gain claim for that storage change.
  [Rounded-border prediction](../benchmarks/VP8-ROUNDED-BORDER-PREDICTION.md)
  saves **112.73µs/4.27% raw**,
  4.55% reference-normalized, on complete effort2.
  Both filtered/all-actual confidence views separate. GC remains406,705B/op;
  no new owned heap capacity or retained state. Current
  2,525.56/native2,511.68µs,
  upper1.013 is uncertain, not a parity pass or demonstrated gap.
  Eight unchanged encode costs reuse [histogram-stage evidence](../benchmarks/VP8-FOUR-BLOCK-HISTOGRAM.md);
  shared RDO/decoder prediction code is untouched. Combined lossy scope:
  6pass/0gaps/3uncertain.
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
  upper1.017 and Gray8 upper1.047 remain uncertain; resolve before assuming tuning.
  WordFastest saves180.09µs/1.00% raw with separated filtered/allactual intervals;
  normalized intervals overlap. Byte's84.29µs point gain has allactual uncertainty.
  No robust normalized or blanket gain claim. Exact packets,384KiB admission,
  retained0scratch and primaryGC820916/2437660Bop remain. Earlier
  [streaming census](../benchmarks/PNG-STREAMING-TOKEN-FREQUENCIES.md),
  [lazy](../benchmarks/PNG-LAZY-VIEW-PREPARATION.md) and
  [greedy preparation](../benchmarks/PNG-GREEDY-VIEW-PREPARATION.md) retain history.
- **Resizing:** [Current tight mixed-byte](../benchmarks/BICUBIC-MIXED-BYTE-CURRENT.md)
  refreshes the historical point using the same qualified actual package DLL:
  1319.86/zimg860.30µs,
  upper1.545,114B/op; about459.57µs gap.
  Exact rational pixels and original complete boundaries remain. No production,
  build, test, package or fixture changes; previous qualification is reused.
  [Direct identity-axis cache rows](../benchmarks/BICUBIC-IDENTITY-CACHE-ROWS.md)
  close byte/word vertical reduction: **636.49→530.97µs / 16.58% raw / 16.50%
  normalized** and **666.59→511.76µs / 23.23% raw / 21.26% normalized**.
  Filtered/all-actual confidence separates; parity uppers0.908/0.904.
  The previous vertical-word regression is superseded. Nine refreshed cached
  controls:7pass/1gap/1uncertain. Byte reduction remains485.10/native416.46µs,
  upper1.169; horizontal word is uncertain (upper1.010). Passing cases stop.
  Identity workspace383,852→367,436B; total allocation367,675/367,595Bop,
  retainedscratch0/result264,708/529,416B until disposal. Exact repair's4KiB
  overlap stays separately admitted. Physical/native peaks are unmeasured;
  existing output-pool retention remains separate. No new API/owner/cache/pool.
  Native zimg retains graph/input/scratch; managed one-shot preparation is timed.
  Both OSes pass5,938full/live, affected normal/scalar/disabled-ISA checks and
  the SAME actual package/three examples. Four displayed rows refresh.
  [Result-backed bands](../benchmarks/BICUBIC-RESULT-BACKED-BANDS.md) save
  86.35/182.46µs
  on default/4KiB mixed word: 841.53/
  862.21µs, uppers
  0.997/1.025.
  Default passes/stops; tight remains a small gap. GC114B/op remains;28,136B is
  borrowed from already-admitted output and retired before suffix stores.
  No added heap or retained scratch. The shared vertical path regresses
  61.98µs/14.62%
  against its contemporary original, but still passes upper
  0.824,113B/op. This tradeoff is
  retained with substantial mixed savings; no uniform gain claim. Both OSes pass
  5,948full/live, affected normal/scalar/disabled-ISA checks and SAME actual
  package/three examples. Fifteen word controls now
  10pass/4gap/1uncertain;
  twelve unchanged paths are reused. Historical bands/geometry stay in linked reports.
  [Lanczos](../benchmarks/LANCZOS-PAIRED-CERTIFICATE-PREPARATION.md) has five
  passes/three prepared gaps; plans retain139,800–333,396B and equivalent
  one-shot setup stays timed. [Streaming](../benchmarks/BICUBIC-SHARED-STREAMING.md)
  passes six of eight; small/wide-column gaps remain linked in coverage.
  [Byte enlargement](../benchmarks/BICUBIC-SHARED-ENLARGEMENT.md) remains
  3,108.21/native3,037.83µs,upper1.026/116Bop. Bilinear byte enlargement is
  uncertain at2,357µs/102,891Bop,upper1.134. Preserve exact Q30/rational/
  certified pixels versus native float; historical gains stay in linked reports.
- **JPEG and reading:** [completed output storage](../benchmarks/JPEG-COMPLETED-OUTPUT.md)
  saves **98.02µs/5.17% raw/5.11% normalized** on progressive grayscale; filtered/
  allactual raw/normalized intervals separate. Current1797.74/native1711.60µs,
  upper1.054 remains a gap. GC683,176→151,432B/op; governed gray peak699,834B and
  retained output531,468B stay unchanged. Sequential default idle pixels rise
  0→531,468B within the existing two-slot/4MiB pool; gray idlecoefficients stay
  549,120B. Governed calls bypass both pools; physical/native peaks are unmeasured.
  Twelve affected costs refresh and seventeen unchanged paths reuse evidence:
  16pass/12gaps/1uncertain. Owned gray now passes (602.47/native681.27µs,upper.891);
  stream420restart is uncertain (upper1.002); stop passing cases and resolve
  uncertainty before tuning. Live444 remains an independent201.57µs gap.
  [Paired AC windows](../benchmarks/JPEG-PAIRED-AC-WINDOW.md) retain earlier causal
  RGB/CMYK gains, unchanged RGB1530.34/native1450.52µs upper1.065 and its574,069B
  governed peak. Current scan tables/owner/cancellation contracts remain intact.
  Earlier [live windows](../benchmarks/JPEG-LIVE-ENTROPY-WINDOW.md),
  [publication](../benchmarks/JPEG-LIVE-NORMAL-PUBLICATION.md) and
  [long lookup](../benchmarks/JPEG-BUFFERED-LONG-LOOKUP.md) retain historical gains.
  [Prepared Huffman replay](../benchmarks/JPEG-PREPARED-HUFFMAN-BANK.md) saves
  **151.45µs/9.43% raw**,
  10.09% reference-normalized, on complete RGBA→gray encoding
  versus explicitly labeled SIMD libjpeg-turbo. Filtered/all-actual intervals separate;
  exact58,910B packets and155,436B/op remain. Current
  1,455.31/native1,409.83µs,
  upper1.042 remains a gap. All17 affected encoder
  boundaries refresh: 15pass/1gaps/1uncertain.
  A4KiB stack lookup fits the existing16KiB table allowance, with no new heap or
  retained state. GovernedRGBA16 peak remains1,704,856B. Existing coefficient pools
  and their governed bypass are unchanged; physical/native peaks are unmeasured.
  Strict JPEG metadata exceptions remain; passing cases stop. Restart/recomputed
  paths retain source-valid evidence. Older normalization/replay stages remain history.
- **Text/drawing:** [exact interior coverage](../benchmarks/TEXT-INTERIOR-COVERAGE.md)
  reuses one exact area across fully covered pixels. Current130.55/132.38µs means are5.94%/6.49%
  lower than the contemporaneous original; intervals overlap, so a causal CPUgain
  remains inconclusive. Both reference gaps remain. Allocation67,368B/draw,
  exactframes,48,544B admitted Latin workspace and zero retainedscratch are unchanged.
  [Shared sorting](../benchmarks/TEXT-RASTER-SHARED-SORT.md) retains the earlier allocation cut.
  [Layout/fitting and RGBA16 composition](../benchmarks/TEXT-DRAWING-CURRENT.md)
  pass/stop. These independent cases do not block codec or resize work.
  [Owned font storage](../benchmarks/FONT-OWNED-STORAGE.md) saves258.73µs/89.04%raw
  on complete loading:31.86µs versus labeledFreeType13.42µs, upper2.396 still a gap.
  Allocation falls757,597→176B/op; fullcopy/eager checks remain timed. The isolated
  load retains757,076B idle within the existing shared two-array/4MiB ceiling;
  governed calls bypass it. This is not a physical-peak reduction or parser-equivalence claim.

The current production build passes **5,943 full/live checks per OS**, affected
normal/scalar checks, and the **same actual Release package/all three examples
on Windows/Linux**, including exact mixed-alpha text at both depths, alpha and
opaque-word, progressive-strip, wide-column, shared byte/word enlargement and live VP8
sync/async consumers, plus bounded main validation and exact method2 residual storage/cancellation/recovery.
The package also verifies frozen vertical RGBA16 pixels at4/8KiB, aggregate
admission, terminalrefusal, cancellationcleanup and independent survivors.
JPEG paths pass407 internal/375 public checks per normal/scalar mode
on each OS, including actual live-window DC/EOB/following-read witnesses,
truncation/stuffing/partial failures, refusal/cancellation cleanup and survivors.
The current PNG package also verifies ordinary/live byte/word sync/async pixels,
terminal refusal, cancellation cleanup/recovery and independent survivors, plus
frozen Fastest8/16 sync/async packets,
shared admission and independent ownership. The current package also exercises
authored grouped-AC JPEG pixels, sync/async scan storage, terminal refusal and
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
| WebP VP8 + alpha, fresh seekable Stream (parity confirmed) † | 5,847.99 | 5,996.63 | 0.975 / 0.980 | 238,960 | — |
| JPEG 4:4:4, forward-only / exact reference (parity confirmed) † | 3,492.70 | 3,776.95 | 0.925 / 0.931 | 820,120 | 133,039 / 133,041 |
| JPEG 4:2:0 from caller RGB samples (confidence open) † | 1,584.61 | 1,578.98 | 1.004 / 1.009 | 388,220 | 52,091 / 52,093 |
| JPEG RGBA16 4:2:0 / caller IMemoryBudget (parity confirmed) † | 1,706.53 | 1,836.80 | 0.929 / 0.934 | 819,475 | 52,115 / 52,117 |
| JPEG grayscale, standalone SIMD libjpeg-turbo (confirmed gap) † | 1,455.31 | 1,409.83 | 1.032 / 1.042 | 155,436 | 58,910 / 58,910 |
| JPEG from caller GRAY8, standalone SIMD libjpeg-turbo (parity confirmed) † | 1,061.94 | 1,076.74 | 0.986 / 0.994 | 155,438 | 58,910 / 58,910 |
| VP8L photograph / libwebp exact=1 † | 81,999.54 | 82,030.95 | 1.000 / 1.008 | 6,382,422 | 160,032 / 160,032 |
| VP8L text/diagram / libwebp exact=1 † | 7,408.85 | 10,140.97 | 0.731 / 0.742 | 4,229,194 | 7,234 / 7,234 |
| VP8 opaque RGBA8 † | 13,865.79 | 15,509.82 | 0.894 / 0.898 | 1,201,560 | 72,380 / 72,380 |
| VP8 opaque RGBA16, including normalization † | 13,882.59 | 15,579.01 | 0.891 / 0.894 | 1,201,560 | 72,380 / 72,380 |
| VP8 opaque photograph † | 9,630.85 | 9,657.98 | 0.997 / 1.001 | 809,233 | 17,688 / 17,688 |
| VP8 opaque text/diagram † | 5,476.34 | 5,803.20 | 0.944 / 0.952 | 741,196 | 7,100 / 7,100 |
| VP8 diagram, effort2 / distortion-only search † | 2,525.56 | 2,511.68 | 1.006 / 1.013 | 406,705 | 7,330 / 7,330 |
| VP8 diagram, effort6 / trellis † | 9,256.01 | 9,813.99 | 0.943 / 0.999 | 754,505 | 6,818 / 6,818 |
| VP8 RGBA8 + lossless alpha † | 53,302.41 | 53,807.33 | 0.991 / 1.002 | 6,963,840 | 98,590 / 98,590 |
| VP8 RGBA16 + alpha, including normalization † | 53,129.08 | 53,549.12 | 0.992 / 0.996 | 6,963,024 | 98,590 / 98,590 |
| VP8L RGBA8 / libwebp exact=1 † | 105,483.31 | 122,008.61 | 0.865 / 0.875 | 7,341,808 | 183,812 / 183,812 |
| VP8L opaque RGBA8 / libwebp exact=1 † | 69,832.01 | 69,828.52 | 1.000 / 1.015 | 5,848,592 | 121,896 / 121,896 |
| PNG RGBA8, one-shot SmallestSize9 / native9 † | 16,851.73 | 17,227.34 | 0.978 / 0.983 | 820,452 | 210,973 / 210,973 |
| PNG RGBA16, one-shot SmallestSize9 / native9 † | 20,549.38 | 20,868.88 | 0.985 / 0.991 | 2,437,660 | 963,202 / 963,202 |
| PNG RGBA8, one-shot Fastest1 / native1 † | 4,343.15 | 4,342.02 | 1.000 / 1.017 | 820,916 | 236,828 / 236,828 |
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
| JPEG baseline420, resident decode (confirmed pass) † | 829.94 | 1,009.76 | 0.822 / 0.829 | 258,450 | — |
| JPEG DC-only, caller CMYK decode † | 337.97 | 332.25 | 1.017 / 1.019 | 1,085,977 | — |
| VP8L RGBA8, resident decode † | 3,093.70 | 3,008.39 | 1.028 / 1.034 | 152,728 | — |
| VP8L RGBA8, fresh seekable Stream decode † | 3,090.59 | 3,027.80 | 1.021 / 1.027 | 152,824 | — |
| JPEG CMYK sparse-2D, resident decode † | 511.72 | 436.56 | 1.172 / 1.178 | 20,944 | — |
| JPEG YCCK420, resident decode (confirmed pass) † | 428.14 | 706.85 | 0.606 / 0.608 | 895,563 | — |
| JPEG 4:1:1, resident decode † | 493.78 | 448.75 | 1.100 / 1.110 | 219,548 | — |
| JPEG 4:1:0, resident decode (confirmed pass) † | 339.51 | 505.41 | 0.672 / 0.678 | 189,090 | — |
| VP8L RGBA16, including normalization / libwebp exact=1 † | 107,919.76 | 121,030.61 | 0.892 / 0.901 | 7,341,808 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 104,422.47 | 120,958.63 | 0.863 / 0.876 | 7,342,192 | 183,812 / 183,812 |
| JPEG caller RGB24 async nonseekable, 420 (parity confirmed) † | 1,581.70 | 1,629.27 | 0.971 / 0.979 | 309,524 | 52,091 / 52,093 |
| JPEG RGBA8 async nonseekable, 444 (parity confirmed) † | 3,517.84 | 3,856.40 | 0.912 / 0.918 | 820,531 | 133,039 / 133,041 |
| PNG8 decode, two retained images / shared caller root † | 2,610.86 | 2,847.04 | 0.917 / 0.920 | 1,638,214 | — |
| JPEG dense CMYK, resident decode (confirmed pass) † | 1,390.69 | 3,111.67 | 0.447 / 0.450 | 20,944 | — |
| JPEG horizontal-only CMYK, resident decode † | 453.31 | 392.03 | 1.156 / 1.162 | 20,944 | — |
| JPEG YCCK420, caller RGB24 decode (confirmed pass) † | 435.42 | 604.87 | 0.720 / 0.722 | 762,721 | — |
| JPEG progressive grayscale, resident decode † | 1,797.74 | 1,711.60 | 1.050 / 1.054 | 151,432 | — |
| JPEG progressive420, resident decode (confirmed pass) † | 1,600.87 | 1,949.71 | 0.821 / 0.824 | 243,664 | — |
| JPEG progressive422, resident decode (confirmed pass) † | 2,089.26 | 2,248.06 | 0.929 / 0.934 | 315,496 | — |
| JPEG ordinary progressive444, resident decode † | 3,490.94 | 3,355.24 | 1.040 / 1.049 | 169,664 | — |
| JPEG progressive444, live-token resident decode † | 3,553.06 | 3,351.49 | 1.060 / 1.066 | 169,664 | — |

**Editing**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 256×129 | 711.35 | 1,588.55 | 0.448 / 0.457 | 138,647 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 256×129 | 563.84 | 489.93 | 1.151 / 1.158 | 139 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 256×129 | 752.13 | 1,946.83 | 0.386 / 0.391 | 146,864 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 256×129 | 558.34 | 512.94 | 1.089 / 1.097 | 139 |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 1027×519 | 4,016.44 | 5,281.52 | 0.760 / 0.783 | 319,224 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 1027×519 | 3,775.71 | 4,085.82 | 0.924 / 0.946 | 148 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 1027×519 | 4,337.24 | 5,079.02 | 0.854 / 0.860 | 4,592,777 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 1027×519 | 4,375.20 | 4,239.10 | 1.032 / 1.052 | 4,265,302 |
| Bilinear resize RGBA8 † H → 256×129 | 504.78 | 387.47 | 1.303 / 1.315 | 157,971 |
| Bilinear upscale RGBA8 † → 1027×519 | 2,357.04 | 2,155.92 | 1.093 / 1.134 | 102,891 |
| Bilinear resize RGBA16 † H → 256×129 | 470.26 | 418.16 | 1.125 / 1.133 | 272,815 |
| Bicubic resize RGBA8 † → 256×129 | 485.10 | 416.46 | 1.165 / 1.169 | 199,980 |
| Bicubic upscale RGBA16 † → 1027×519 | 2,752.40 | 2,874.23 | 0.958 / 0.966 | 4,264,573 |
| Bicubic upscale RGBA8, default workspace † → 1027×519 | 3,108.21 | 3,037.83 | 1.023 / 1.026 | 116 |
| Bicubic RGBA16, width down / height up / input+output+4 KiB † → 256×519 | 862.45 | 825.16 | 1.045 / 1.049 | 114 |
| Bicubic RGBA8, 32×16 / input+output+4 KiB H → 32×16 | 464.68 | 375.02 | 1.239 / 1.370 | 2,185 |
| Bicubic RGBA8, 32×16 / default workspace † → 32×16 | 356.88 | 375.09 | 0.951 / 0.956 | 197,300 |
| Bicubic RGBA8 enlargement, input+output+4 KiB † → 1027×519 | 3,212.58 | 3,052.03 | 1.053 / 1.072 | 116 |
| Bicubic RGBA8 width enlargement, input+output+4 KiB † → 1027×259 | 1,546.70 | 1,524.36 | 1.015 / 1.066 | 114 |
| Bicubic RGBA8 height enlargement, input+output+4 KiB † → 513×519 | 1,811.37 | 1,745.56 | 1.038 / 1.061 | 115 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129 | 617.43 | 456.16 | 1.354 / 1.363 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 256×129 | 751.02 | 619.10 | 1.213 / 1.223 | 113 |
| Bicubic RGBA8 wider-support reduction † H → 128×64 | 493.82 | 347.10 | 1.423 / 1.445 | 231,241 |
| Bicubic RGBA8, width up / height down, input+output+4 KiB † H → 1027×129 | 1,319.86 | 860.30 | 1.534 / 1.545 | 114 |
| Bicubic RGBA16, width up / height down, input+output+4 KiB † → 1027×129 | 862.21 | 844.22 | 1.021 / 1.025 | 114 |
| Bicubic RGBA8 reduction, input+output+4 KiB † H → 64×32 | 545.55 | 370.06 | 1.474 / 1.506 | 8,329 |
| Bicubic RGBA16 reduction, input+output+4 KiB † H → 64×32 | 518.01 | 389.68 | 1.329 / 1.355 | 16,521 |
| Bicubic RGBA8 reduction, input+output+4 KiB † H → 128×64 | 480.55 | 347.12 | 1.384 / 1.394 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † H → 128×64 | 532.73 | 404.07 | 1.318 / 1.329 | 113 |
| Bicubic RGBA8, width down / height up, default workspace † → 256×519 | 816.23 | 840.52 | 0.971 / 0.979 | 199,943 |
| Bicubic RGBA16, width down / height up, default workspace † → 256×519 | 790.79 | 820.20 | 0.964 / 0.983 | 199,891 |
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
| Prepared text RGBA8 / ImageSharp.Drawing (gap) † | 130.55 | 45.72 | 2.856 / 4.456 | 67,368 |
| Prepared text RGBA16 / ImageSharp.Drawing (gap) † | 132.38 | 45.83 | 2.889 / 4.174 | 67,368 |
| Composite RGBA8 / FFmpeg (gap) | 625.41 | 498.73 | 1.254 / 1.270 | 56 |
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
| Validate JPEG baseline420, span | 477.351 | 674.564 | 0.708 / 0.711 | 10,016 | Confirmed pass |
| Validate JPEG progressive420, span | 330.131 | 268.515 | 1.229 / 1.237 | 4,088 | Confirmed gap |
| Validate VP8L+alpha, span | 2,309.164 | 3,044.518 | 0.758 / 0.765 | 297,070 | Confirmed pass |
| Validate VP8+alpha, span † | 4,409.048 | 5,448.587 | 0.809 / 0.811 | 14,064 | Confirmed pass |
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
