# Lokad.Image benchmarks

Updated 2026-11-12. These **138 tabulated comparisons** combine qualified
**0.2.1 development** costs with retained **0.2.0** evidence. Full parity remains
open across the [289 required paths](../benchmarks/WARM-COVERAGE.md). **†** marks
development costs. Retained release costs apply only to unchanged production
paths. Publication is postponed.

All measurements use the dedicated quiet Ubuntu x64/AMD EPYC 9V45 VM, sequential
warm operations on one pinned CPU, .NET 10 with tiering off and nonconcurrent
workstation GC. Times are **µs per complete operation**. Ratios use the faster
reference bracket; **upper** includes conservative all-actual and relevant filtered/paired confidence.
Short screens do not establish independent parity. **B/op** is total managed
allocation, separate from peak live capacity and retained storage. Native
allocations are unmeasured. Normalization, output ownership and disposal stay
timed; setup, delivery and retained state follow the stated case boundaries.

Current scope and remaining work:

- **WebP:** [current lossless/alpha storage](../benchmarks/WEBP-HUFFMAN-WORKSPACE.md)
  has 6 passes / 2 uncertain encode controls;
  [current photograph](../benchmarks/WEBP-LOSSLESS-PHOTOGRAPH-CONFIDENCE.md) and
  [transparent RGBA8](../benchmarks/WARM-ADJACENT-CONFIDENCE.md) pass on the actual
  package DLL. Alpha costs 52.99/FFmpeg 53.66 ms,
  upper 0.998, exact 98,590 B and 6,241,792 B/op; this resolves uncertainty, with no production speedup claim.
  [Opaque lossless](../benchmarks/WEBP-OPAQUE-CURRENT-CONFIDENCE.md) remains uncertain:
  73.41/libwebp73.50ms, range0.915–1.070,
  4,822,760B/op and exact121,896B packet. All adjacent pairs/brackets remain;
  no codec change, presumed gap or favorable repeat follows the first adjacent capture.
  [Per-tile inverse color](../benchmarks/WEBP-INVERSE-COLOR-TILES.md) saves191.79µs/
  6.14%raw/7.18%normalized on live-token VP8L decoding. Current2.933/FFmpeg3.059ms,
  range0.907–0.989 passes/stops; allocation stays152,728B/op.
  All exact pixels/owners and12adjacent pairs/brackets pass; native retains codec/
  conversion contexts and has no equivalent cancellation token. Across six lossless
  reads/validators:6pass/0gap/0uncertain.
  All six pass/stop. Two validators bypass inverse arithmetic and reuse source-valid evidence.
  The [lossy scope](../benchmarks/VP8-ROUNDED-BORDER-PREDICTION.md) now has
  6 passes / 3 uncertain. The current word-alpha view is
  53.40/FFmpeg 53.30 ms, upper 1.008: uncertain,
  exact normalized alpha/packet and 6,241,792 B/op. Resolve uncertainty before tuning. Complete packets remain
  exact. The same [color-tile round](../benchmarks/WEBP-INVERSE-COLOR-TILES.md)
  refreshes six affected companion decodes without a uniform causal gain claim.
  VP8/ALPH plus unchanged opaque [position preparation](../benchmarks/VP8-POSITION-MODEL-PREPARATION.md) has
  5 passes / 0 gaps / 1 uncertain.
  Ordinary opaque decoding improves 2.02% after reference normalization, with separated
  raw/all-actual/normalized intervals. Live 4,803.55/native4,786.85µs,
  upper1.009, remains uncertain. Resident and stream ALPH now pass.
  All six allocation totals include the earlier increase of
  1,536 B/op for frame preparation, within the unchanged 8 KiB admission; scratch0.
  Selected codes/results retain independent ownership.
- **PNG:** [current decoding](../benchmarks/PNG-CURRENT-FRAMING-COSTS.md)
  has 49 passes / 0 gaps / 2 uncertain across 51 controls. [Prepared live-run invariants](../benchmarks/PNG-LIVE-RUN-PREPARATION.md)
  save93.08µs/2.89% raw/1.86% normalized on RGBA16, with separated filtered/all-actual
  and normalized confidence; native moves−1.05%. Current3,126.33/native3,153.78µs,
  upper1.001253 remains uncertain. Byte1,529.69/native1,535.97µs, upper1.012 also
  remains uncertain; raw+9.90% tracks native+10.19%, with overlapping normalized
  intervals, not a byte speedup or uniform raw-gain claim. GC4,472/8,568B/op and
  4KiB framing admission are unchanged; both per-symbol cancellation polls remain.
  [One-shot encoding](../benchmarks/PNG-LENGTH-CLASSIFICATION.md)
  has 13 passes / 2 uncertain across 15 controls; ByteFastest/Gray8 need confidence resolution.
  [The first adjacent Fastest check](../benchmarks/PNG-FASTEST-ADJACENT-CONFIDENCE.md) stays
  uncertain:4.398/4.375ms,range0.944–1.044,820,843.67directB/op,exact236,828B packet.
  Its natural-GC4op pairs remain distinct from the earlier global-bracket view;
  no causal gain, allocation saving or presumed gap.
  [Grayscale8's first adjacent view](../benchmarks/PNG-GRAY-ADJACENT-CONFIDENCE.md)
  also stays uncertain:2.109/1.997ms,range0.963–1.143,551,260.67directB/op,exact72,586B packet.
  Fresh native codec/conversion/zlib state, normalization and independent packets
  are timed. Exact packets and existing admission remain; no global parity claim.
  [Predictable16](../benchmarks/PNG-HISTORICAL-CURRENT-CONFIRMATION.md) retains its
  unchanged compressed-path pass:277.00/407.42ms upper0.706,2,495,988B/op,573,041B packet.
  [Stored-mode workspace](../benchmarks/PNG-STORED-WORKSPACE.md) removes three unused
  arrays:196,604B payload and384→72KiB admission, preserving complete packets.
  Stored8 now92.72/190.15µs upper0.535,613,844B/op. Stored16 post-GC4op
  501.70/405.21µs upper1.255 remains a gap; natural-GC64op350.36/405.10µs upper0.904
  passes that scope,1,151,529.875 direct B/op. Its post-GC allocation is1,151,996B/op.
  Both sampling views remain; general word parity stays open. No CPU gain or
  physical-peak claim. Existing output capacity is unchanged; scratch retention is zero.
- **JPEG:** [shared block support](../benchmarks/JPEG-SHARED-BLOCK-SUPPORT.md)
  forms exact support alongside dequantization and reuses inverse decisions.
  Sparse CMYK saves **16.85 µs/3.86% raw**, dense RGB444 **45.35 µs/3.87%**,
  with separated filtered/all-actual raw and normalized intervals. All 27 pixel
  decode costs refresh; two unchanged coding-only validations are reused.
  Across 29 reads: **28 pass / 0 gaps /
  1 uncertain**. Sparse and caller CMYK pass/stop;
  retained-context horizontal CMYK stays uncertain and its equivalent fresh view passes.
  No added owned capacity or retention; sparse CMYK/RGB allocations remain 20,944/42,936 B/op.
  [Encoding](../benchmarks/JPEG-GRAY-TILE-PREPARATION.md) retains scoped grayscale
  gains and 15 unchanged color costs. Default/CompactColor and content/sampling
  claims remain distinct in the packet table. Frozen packets/metadata gates apply.
- **Resizing:** [Shared filter weights](../benchmarks/SHARED-RESIZE-FILTER-WEIGHTS.md)
  reconcile all61existing bicubic, eight prepared/one-shot and four bilinear controls:
  **58pass / 9gaps / 6uncertain** in the standalone protocol. [Shared byte geometry](../benchmarks/BYTE-CROSS-SHARED-GEOMETRY.md)
  refreshes the two affected byte cross/identity costs on the new actual package DLL;
  its two costs remain source-valid. It saves149.01/114.82µs,12.29%/14.26%raw and
  12.83%/14.79%normalized, with separated confidence and native+0.614%/+0.627%.
  Both remain gaps, upper1.270/1.212; numerical loops,4KiB and ownership are unchanged.
  The table replaces two comfortable passing summaries with these material gaps;
  all73controls remain in the linked evidence. Bounded wide byte343.67/zimg346.22µs,
  upper.995 passes after13.66µs/3.82%raw/2.57%normalized saved; narrow/packed word
  gains generalize, with separate raw/native views and disclosed slower passing controls.
  Exact pixels/repair,4KiB bounds and ownership remain; no new owned capacity or retention.
  Prepared plans retain198432B wide/199840B narrow; all eight equivalent boundaries pass.
  Mixed byte's scoped64-operation adjacent854.87/866.15µs upper.998 passes; its
  original standalone view remains uncertain. Bilinear word enlargement's adjacent
 1836.05/1967.62µs upper1.001 remains uncertain, directGC4498558B/op. No historical
  stage sums, favorable repeats or full-parity claim.
  [Generated byte row reuse](../benchmarks/GENERATED-BICUBIC-ROW-REUSE.md) removes
  repeated coefficient/index preparation across16source rows and overlapping work
  across16output rows. The2×9control is now460.45/zimg1049.03µs,
  upper0.442,113B/op; all three affected2×9/2×2/1×1controls pass/stop.
  Explicit arrays grow512→1824B inside unchanged4KiB admission, with zero retained
  scratch; physical/JIT peaks remain unmeasured. Other70resize costs are reused.
  The table replaces a comfortable passing32×16summary with the current2×9cost;
  all73remain in evidence. Largest remaining resize gap is byte reverse-mixed,
  about222µs. Establish current attribution before further kernel tuning.
  [Lanczos3](../benchmarks/LANCZOS-BYTE-PRODUCT-PREPARATION.md) retains source-valid
  six passes/two uncertainties, exact Q30 and separate complete preparation boundaries.
  Its byte489.94/zimg491.00µs upper1.015 is uncertain; no presumed gap or retuning.
- **Text/drawing:** [Cleared canvas storage and complete public pipelines](../benchmarks/CLEARED-CANVAS-STORAGE.md)
  pass on the existing constrained-fit RGBA16 control: one-shot font→fit→layout→draw→dispose
  1,313.88/ImageSharp 6,739.69 µs, upper 0.218;
  preparation plus eight fresh-canvas draws/disposal 2,055.00/7,634.49 µs,
  upper 0.302, measured amortization 256.87/954.31 µs/draw.
  GC totals are 283,275/9,256,953 B one-shot and
  284,623/9,494,633 B per eight-draw batch.
  The eight-draw batch saves665.54 µs/24.46% raw/29.86% normalized with separated
  confidence; reference time moves+7.69%. Managed active preparation owns39,904 B;
  approximate live-heap deltas34,256/22,504 B are separate from allocations.
  Default disposed font/canvas storage may retain1,248,596 B in this sequence,
  within the existing two-array/4MiB ceiling; governed calls still bypass it.
  Steady fresh-canvas drawing is uncertain (105.65/81.93 µs, upper 2.006);
  no general-font or arbitrary-repeat-count claim. Parser validation, retained paths/workspace,
  all fitting/layout calls and full16-bit visual gates remain explicit. Earlier
  [font-load](../benchmarks/FONT-OWNED-STORAGE.md) and [drawing](../benchmarks/TEXT-RENDER-PREPARATION.md)
  stage costs below are scoped diagnostics, not proof of overall text slowdown; never add
  them into an end-to-end estimate. Separate [RGBA16](../benchmarks/TEXT-DRAWING-CURRENT.md)
  and [RGBA8 composition](../benchmarks/BYTE-COMPOSITE-EXACT-VECTOR.md) pass/stop. Independent codec/resizing work continues.

The current immutable build passes **6,470 full/live checks per OS**
and the **same actual Release-package consumer/all three usage examples on
Windows/Linux**. Current WebP checks pass1,033internal/239public per
normal/scalar/AVX512-off/AVX2-off mode; unchanged Huffman BMI2, broader shared-memory and numerical ISA
evidence is reused. Current text/canvas/ownership
checks pass183internal/162public per normal/scalar mode on each OS.
Current PNG/admission checks pass690internal/246public per
normal/scalar/disabled-AVX2/AVX512 mode.
Current resize qualification passes542 internal/791 public per normal/scalar mode;
unchanged disabled-AVX2/AVX512 evidence (528internal/791public) is reused.
Unchanged JPEG checks pass
464 internal/341 public per normal/scalar mode; Linux also disables
AVX512 and AVX512+BMI2, with 464 Windows internal checks for the latter.
The exact JPEG filter is `Jpeg`; older wider counts included ownership tests.
Exact packets/pixels, shared admission,
refusal/cancellation cleanup and surviving owners remain qualified. Unchanged
paths and family-specific ISA evidence are reused; this is not a whole-release
performance refresh. Total GC, admitted capacity and retained state are separate;
physical/native/JIT-stack peaks are unmeasured unless a linked report says otherwise.
Historical/adverse experiments remain in the linked family reports and raw archives.

**References and boundaries.** FFmpeg is the reference except for explicitly
labeled **libwebp exact=1** lossless WebP (hidden RGB preserved), SIMD-enabled
**libjpeg-turbo** grayscale JPEG, and **zimg** resizing (FFmpeg's zscale backend).
The fifteen current one-shot PNG rows time fresh codec/zlib/packet state and independent
output on both sides, including native word-frame endian conversion/allocation/
disposal. The [Adobe CMYK fresh-context row](../benchmarks/JPEG-SHARED-BLOCK-SUPPORT.md)
also times decoder/frame/packing-context setup and disposal; its original
retained-context comparison remains separate. Other native codec workers retain
their documented wrappers/contexts.
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
| WebP VP8 + alpha, fresh seekable Stream (parity confirmed) † | 6,187.90 | 6,465.82 | 0.957 / 0.964 | 240,496 | — |
| JPEG 4:4:4, forward-only / exact reference (parity confirmed) † | 3,492.70 | 3,776.95 | 0.925 / 0.931 | 820,120 | 133,039 / 133,041 |
| JPEG 4:2:0 from caller RGB samples (confidence open) † | 1,584.61 | 1,578.98 | 1.004 / 1.009 | 388,220 | 52,091 / 52,093 |
| JPEG RGBA16 4:2:0 / caller IMemoryBudget (parity confirmed) † | 1,706.53 | 1,836.80 | 0.929 / 0.934 | 819,475 | 52,115 / 52,117 |
| JPEG grayscale, standalone SIMD libjpeg-turbo (parity confirmed) † | 1,160.95 | 1,403.16 | 0.827 / 0.840 | 155,435 | 58,910 / 58,910 |
| JPEG from caller GRAY8, standalone SIMD libjpeg-turbo (parity confirmed) † | 1,059.14 | 1,112.88 | 0.952 / 0.969 | 155,438 | 58,910 / 58,910 |
| VP8L photograph / libwebp exact=1 † | 81,000.31 | 83,387.76 | 0.971 / 0.981 | 5,358,368 | 160,032 / 160,032 |
| VP8L text/diagram / libwebp exact=1 † | 7,441.59 | 9,575.22 | 0.777 / 0.791 | 3,721,041 | 7,234 / 7,234 |
| VP8 opaque RGBA8 † | 13,865.79 | 15,509.82 | 0.894 / 0.898 | 1,201,560 | 72,380 / 72,380 |
| VP8 opaque RGBA16, including normalization † | 13,882.59 | 15,579.01 | 0.891 / 0.894 | 1,201,560 | 72,380 / 72,380 |
| VP8 opaque photograph † | 9,630.85 | 9,657.98 | 0.997 / 1.001 | 809,233 | 17,688 / 17,688 |
| VP8 opaque text/diagram † | 5,476.34 | 5,803.20 | 0.944 / 0.952 | 741,196 | 7,100 / 7,100 |
| VP8 diagram, effort2 / distortion-only search † | 2,525.56 | 2,511.68 | 1.006 / 1.013 | 406,705 | 7,330 / 7,330 |
| VP8 diagram, effort6 / trellis † | 9,256.01 | 9,813.99 | 0.943 / 0.999 | 754,505 | 6,818 / 6,818 |
| VP8 RGBA8 + lossless alpha † | 52,994.15 | 53,664.11 | 0.988 / 0.998 | 6,241,792 | 98,590 / 98,590 |
| VP8 RGBA16 + alpha, including normalization † | 53,395.51 | 53,304.90 | 1.002 / 1.008 | 6,241,792 | 98,590 / 98,590 |
| VP8L RGBA8 / libwebp exact=1 † | 107,509.18 | 126,289.11 | 0.851 / 0.910 | 5,948,664 | 183,812 / 183,812 |
| VP8L opaque RGBA8 / libwebp exact=1 † | 73,406.90 | 73,496.23 | 0.999 / 1.070 | 4,822,760 | 121,896 / 121,896 |
| PNG RGBA8, one-shot SmallestSize9 / native9 † | 16,851.73 | 17,227.34 | 0.978 / 0.983 | 820,452 | 210,973 / 210,973 |
| PNG RGBA16, one-shot SmallestSize9 / native9 † | 20,549.38 | 20,868.88 | 0.985 / 0.991 | 2,437,660 | 963,202 / 963,202 |
| PNG RGBA8, one-shot Fastest1 / native1 † | 4,397.62 | 4,374.65 | 1.005 / 1.044 | 820,844 | 236,828 / 236,828 |
| PNG RGBA16, stored0 / native0 (post-GC 4-op gap) † | 501.70 | 405.21 | 1.238 / 1.255 | 1,151,996 | 1,066,552 / 1,066,552 |
| PNG grayscale8, one-shot Optimal6 / native6 † | 2,109.25 | 1,997.37 | 1.056 / 1.143 | 551,261 | 72,586 / 72,586 |
| PNG RGB8, one-shot Optimal6 / native6 † | 9,370.01 | 9,793.98 | 0.957 / 0.972 | 819,884 | 168,729 / 168,729 |
| PNG gray-alpha8, one-shot Optimal6 / native6 † | 4,162.99 | 4,444.97 | 0.937 / 0.955 | 552,332 | 120,366 / 120,366 |
| PNG RGBA8, one-shot Optimal6 / native6 † | 16,744.89 | 17,271.29 | 0.970 / 0.975 | 820,452 | 210,973 / 210,973 |
| PNG RGBA16, one-shot Optimal6 / native6 † | 20,261.52 | 20,732.37 | 0.977 / 0.983 | 2,437,660 | 963,202 / 963,202 |
| PNG RGB16, one-shot Optimal6 / native6 † | 15,330.07 | 16,071.79 | 0.954 / 0.963 | 2,431,492 | 739,472 / 739,472 |
| PNG grayscale16, one-shot Optimal6 / native6, red-plane reference † | 4,704.74 | 5,506.13 | 0.854 / 0.875 | 822,740 | 265,433 / 265,433 |
| PNG gray-alpha16, one-shot Optimal6 / native6 † | 9,874.42 | 10,487.03 | 0.942 / 0.950 | 1,360,900 | 506,887 / 506,887 |
| PNG RGBA8, stored0 / native0 † | 92.72 | 190.15 | 0.488 / 0.535 | 613,844 | 533,444 / 533,444 |
| PNG RGBA8 decode, live token † | 1,529.69 | 1,535.97 | 0.996 / 1.012 | 4,472 | — |
| PNG RGBA8 decode, tight working limit † | 1,184.46 | 1,395.39 | 0.849 / 0.855 | 68,224 | — |
| PNG RGBA16 decode, tight working limit † | 2,826.77 | 2,894.02 | 0.977 / 0.981 | 70,272 | — |
| PNG predictable RGBA16 3000², Optimal6 / mixed6 † | 276,996.48 | 407,420.77 | 0.680 / 0.706 | 2,495,988 | 573,041 / 573,041 |
| PNG RGBA16, one-shot Fastest1 / native1 † | 17,776.55 | 18,600.15 | 0.956 / 0.966 | 2,437,660 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, live cancelable token † | 18,197.48 | 19,178.63 | 0.949 / 0.954 | 2,437,660 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, forward-only output † | 17,856.45 | 18,591.70 | 0.960 / 0.966 | 2,437,692 | 968,216 / 968,216 |
| PNG RGBA8 async, one-shot Optimal6, nonseekable output † | 16,825.68 | 18,080.22 | 0.931 / 0.941 | 882,212 | 210,973 / 210,973 |
| JPEG baseline420, resident decode (confirmed pass) † | 668.74 | 1,013.98 | 0.660 / 0.670 | 258,454 | — |
| JPEG DC-only, caller CMYK decode (confirmed pass) † | 318.93 | 330.45 | 0.965 / 0.972 | 1,085,981 | — |
| VP8L RGBA8, resident decode † | 2,719.46 | 3,020.83 | 0.900 / 0.907 | 152,728 | — |
| VP8L RGBA8, fresh seekable Stream decode † | 2,906.33 | 3,151.62 | 0.922 / 0.983 | 152,824 | — |
| VP8L palette16 RGBA8, resident decode (confirmed pass) † | 420.09 | 544.27 | 0.772 / 0.778 | 288,465 | — |
| JPEG CMYK sparse-2D, resident decode (confirmed pass) † | 419.25 | 433.52 | 0.967 / 0.978 | 20,944 | — |
| JPEG YCCK420, resident decode (confirmed pass) † | 420.81 | 709.56 | 0.593 / 0.598 | 895,563 | — |
| JPEG 4:1:1, resident decode (confirmed pass) † | 382.03 | 448.23 | 0.852 / 0.863 | 219,551 | — |
| JPEG 4:1:0, resident decode (confirmed pass) † | 337.85 | 507.32 | 0.666 / 0.671 | 189,090 | — |
| VP8L RGBA16, including normalization / libwebp exact=1 † | 114,600.56 | 131,281.45 | 0.873 / 0.887 | 5,948,664 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 112,507.39 | 131,039.53 | 0.859 / 0.869 | 5,949,048 | 183,812 / 183,812 |
| JPEG caller RGB24 async nonseekable, 420 (parity confirmed) † | 1,581.70 | 1,629.27 | 0.971 / 0.979 | 309,524 | 52,091 / 52,093 |
| JPEG RGBA8 async nonseekable, 444 (parity confirmed) † | 3,517.84 | 3,856.40 | 0.912 / 0.918 | 820,531 | 133,039 / 133,041 |
| PNG8 decode, two retained images / shared caller root † | 2,610.86 | 2,847.04 | 0.917 / 0.920 | 1,638,214 | — |
| JPEG dense CMYK, resident decode (confirmed pass) † | 1,274.91 | 3,112.44 | 0.410 / 0.413 | 20,944 | — |
| JPEG horizontal-only CMYK, resident decode (confidence open) † | 397.12 | 402.99 | 0.985 / 1.008 | 20,944 | — |
| JPEG YCCK420, caller RGB24 decode (confirmed pass) † | 429.43 | 605.39 | 0.709 / 0.715 | 762,717 | — |
| JPEG progressive grayscale, resident decode (confirmed pass) † | 1,587.42 | 1,712.14 | 0.927 / 0.937 | 151,432 | — |
| JPEG progressive420, resident decode (confirmed pass) † | 1,443.79 | 1,950.04 | 0.740 / 0.745 | 243,676 | — |
| JPEG progressive422, resident decode (confirmed pass) † | 1,788.29 | 2,241.06 | 0.798 / 0.804 | 315,496 | — |
| JPEG ordinary progressive444, resident decode (confirmed pass) † | 2,852.57 | 3,359.77 | 0.849 / 0.852 | 169,664 | — |
| JPEG progressive444, live-token resident decode (confirmed pass) † | 3,036.83 | 3,360.43 | 0.904 / 0.907 | 169,664 | — |

**Editing**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 256×129 | 667.33 | 1,629.83 | 0.409 / 0.436 | 138,647 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 256×129 | 489.94 | 491.00 | 0.998 / 1.015 | 138 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 256×129 | 698.00 | 1,963.30 | 0.356 / 0.360 | 155,077 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 256×129 | 550.80 | 552.29 | 0.997 / 1.012 | 139 |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 1027×519 | 3,769.63 | 5,231.96 | 0.720 / 0.726 | 319,224 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 1027×519 | 3,496.35 | 4,060.63 | 0.861 / 0.865 | 148 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 1027×519 | 4,084.94 | 5,070.48 | 0.806 / 0.811 | 4,592,818 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 1027×519 | 3,797.13 | 3,893.13 | 0.975 / 0.979 | 4,265,332 |
| Bicubic resize RGBA8 (uncertain) † → 256×129 | 416.53 | 418.55 | 0.995 / 1.004 | 199,985 |
| Bicubic upscale RGBA16 † → 1027×519 | 2,823.87 | 3,104.13 | 0.910 / 0.920 | 4,264,573 |
| Bicubic RGBA8, width reduction / height enlargement, input+output+4 KiB (gap) † → 256×519 | 1,063.49 | 841.66 | 1.264 / 1.270 | 114 |
| Bicubic RGBA16, width down / height up / input+output+4 KiB † → 256×519 | 811.25 | 819.63 | 0.990 / 0.993 | 113 |
| Bicubic RGBA8, 32×16 / input+output+4 KiB † → 32×16 | 289.79 | 375.72 | 0.771 / 0.773 | 113 |
| Bicubic RGBA8 extreme reduction, input+output+4 KiB † → 2×9 | 460.45 | 1,049.03 | 0.439 / 0.442 | 113 |
| Bicubic RGBA8 enlargement, input+output+4 KiB † → 1027×519 | 3,004.30 | 3,272.75 | 0.918 / 0.938 | 116 |
| Bicubic RGBA8 width enlargement, input+output+4 KiB † → 1027×259 | 1,424.98 | 1,521.11 | 0.937 / 0.942 | 114 |
| Bicubic RGBA8 height enlargement, input+output+4 KiB † → 513×519 | 1,399.53 | 1,736.72 | 0.806 / 0.811 | 114 |
| Bicubic RGBA8 reduction, input+output+4 KiB (gap) † → 256×129 | 426.33 | 418.60 | 1.018 / 1.024 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 256×129 | 488.42 | 571.80 | 0.854 / 0.859 | 113 |
| Bicubic RGBA8, fresh managed / prepared zimg (gap) † → 128×64 | 349.08 | 346.62 | 1.007 / 1.011 | 198,452 |
| Bicubic RGBA8, width up / height down, input+output+4 KiB (uncertain) † → 1027×129 | 900.25 | 870.63 | 1.034 / 1.091 | 114 |
| Bicubic RGBA16, width up / height down, input+output+4 KiB (uncertain) † → 1027×129 | 857.10 | 848.80 | 1.010 / 1.023 | 114 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 64×32 | 361.90 | 368.76 | 0.981 / 0.991 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 64×32 | 364.87 | 387.63 | 0.941 / 0.945 | 113 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 128×64 | 343.67 | 346.22 | 0.993 / 0.995 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 128×64 | 405.22 | 413.66 | 0.980 / 0.990 | 113 |
| Bicubic RGBA8, width down / height up, default workspace † → 256×519 | 783.44 | 838.43 | 0.934 / 0.981 | 199,947 |
| Bicubic RGBA16, width down / height up, default workspace † → 256×519 | 749.70 | 815.22 | 0.920 / 0.926 | 199,898 |
| Plain transpose RGBA16 † → 259×513 | 32.93 | 44.48 | 0.740 / 0.748 | 112 |
| Plain transpose RGBA8 † → 259×513 | 26.80 | 38.79 | 0.691 / 0.711 | 112 |
| Exact rectangle copy into borrowed RGBA8 canvas | 2.13 | 1.93 | 1.100 / 1.114 | 56 |
| Exact rectangle copy into borrowed RGBA16 canvas (confidence open) | 3.89 | 3.83 | 1.016 / 1.052 | 56 |
| Exact clipped fill into borrowed RGBA8 canvas (parity confirmed) | 1.19 | 1.57 | 0.759 / 0.775 | 56 |
| Exact clipped fill into borrowed RGBA16 canvas (parity confirmed) | 2.29 | 4.20 | 0.546 / 0.567 | 56 |
| Overlapping RGBA8 copy + reverse restoration, per pair (gap) | 4.18 | 3.78 | 1.106 / 1.152 | 112 |
| Text ordinary draw RGBA8 / ImageSharp.Drawing (gap) † | 87.12 | 45.40 | 1.919 / 2.945 | 59,304 |
| Text ordinary draw RGBA16 / ImageSharp.Drawing (gap) † | 92.57 | 46.48 | 1.992 / 2.975 | 59,304 |
| Text prepared raster RGBA8 / ImageSharp.Drawing (confidence open) † | 53.81 | 37.80 | 1.424 / 1.622 | 56 |
| Text prepared raster RGBA16 / ImageSharp.Drawing (gap) † | 56.80 | 37.61 | 1.510 / 2.015 | 56 |
| Font loading / FreeType 2.13.2 (gap) † | 31.86 | 13.42 | 2.375 / 2.396 | 176 |
| Wrapped logical layout / SixLabors public API (pass) | 6.07 | 96.16 | 0.063 / 0.097 | 3,153 |
| Latin OpenType layout / SixLabors public API (pass) | 20.40 | 57.62 | 0.354 / 0.717 | 6,409 |
| Arabic OpenType layout / SixLabors public API (pass) | 88.93 | 337.84 | 0.263 / 0.421 | 28,658 |
| Complete fitting search / SixLabors public API (pass) | 1,144.00 | 1,882.11 | 0.608 / 0.866 | 224,181 |
| Composite RGBA8 / FFmpeg (pass) † | 65.63 | 451.36 | 0.145 / 0.157 | 56 |
| Composite RGBA16 / ImageSharp 3.1.12 (pass) | 627.24 | 696.25 | 0.901 / 0.922 | 56 |
| Bilinear upscale RGBA8 † → 1027×519 | 1,959.44 | 1,981.62 | 0.989 / 0.993 | 102,891 |
| Bilinear resize RGBA8 (gap) † → 256×129 | 396.27 | 379.65 | 1.044 / 1.056 | 8,473 |
| Bilinear resize RGBA16 † → 256×129 | 398.05 | 417.42 | 0.954 / 0.962 | 8,473 |
| Bicubic RGBA8, width reduction / same height, input+output+4 KiB (gap) † → 256×259 | 690.55 | 576.22 | 1.198 / 1.212 | 113 |
| Bicubic one-shot RGBA8 / zimg one-shot † → 128×64 | 348.81 | 988.82 | 0.353 / 0.355 | 198,455 |
| Bicubic prepared RGBA8 / zimg prepared † → 128×64 | 259.80 | 347.32 | 0.748 / 0.750 | 137 |
| Bicubic one-shot RGBA16 / zimg one-shot † → 128×64 | 396.85 | 594.42 | 0.668 / 0.674 | 198,453 |
| Bicubic prepared RGBA16 / zimg prepared † → 128×64 | 328.08 | 412.01 | 0.796 / 0.805 | 137 |
| Bicubic one-shot narrow RGBA8 / zimg one-shot † → 256×129 | 448.47 | 1,034.17 | 0.434 / 0.436 | 199,991 |
| Bicubic prepared narrow RGBA8 / zimg prepared † → 256×129 | 390.39 | 416.20 | 0.938 / 0.942 | 137 |
| Bicubic one-shot narrow RGBA16 / zimg one-shot † → 256×129 | 446.69 | 717.02 | 0.623 / 0.626 | 199,983 |
| Bicubic prepared narrow RGBA16 / zimg prepared † → 256×129 | 372.59 | 569.92 | 0.654 / 0.658 | 137 |

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
| Validate JPEG baseline420, span † | 483.057 | 674.268 | 0.716 / 0.720 | 10,016 | Confirmed pass |
| Validate JPEG progressive420, span † | 288.749 | 297.280 | 0.971 / 0.984 | 4,088 | Confirmed pass |
| Validate VP8L+alpha, span † | 2,225.118 | 3,204.194 | 0.694 / 0.730 | 297,070 | Confirmed pass |
| Validate VP8+alpha, span † | 4,350.108 | 5,453.134 | 0.798 / 0.801 | 15,600 | Confirmed pass |
| Validate PNG16, async nonseekable † | 2,759.145 | 3,611.996 | 0.764 / 0.765 | 8,544 | Confirmed pass |
| Inspect PNG8 complete headers, seekable, adapter reference | 0.703 | 0.579 | 1.215 / 1.235 | 8,528 | Confirmed gap |
| Identify PNG8 complete container, resident, adapter reference | 13.079 | 13.049 | 1.002 / 1.005 | 96 | Confirmed gap |
| Identify JPEG420, resident, canonical adapter | 0.501 | 1.193 | 0.420 / 0.426 | 96 | Confirmed pass |
| Identify VP8L+alpha, resident, parser adapter | 0.022 | 4.985 | 0.004 / 0.005 | 96 | Confirmed pass |
| Identify PNG8, fresh seekable Stream, adapter reference | 19.537 | 15.929 | 1.226 / 1.232 | 192 | Confirmed gap |
| Identify PNG8, async nonseekable, adapter reference | 20.312 | 19.321 | 1.051 / 1.060 | 8,400 | Confirmed gap |
| JPEG Adobe CMYK horizontal AC / FFmpeg fresh contexts † | 398.401 | 745.735 | 0.534 / 0.537 | 20,944 | Confirmed pass |

[Coverage and linked family reports](../benchmarks/WARM-COVERAGE.md) retain producer
identities, confidence decisions, ownership/correctness gates and reproducible raw
evidence. Historical experiment reports remain separate. Update with
`python benchmarks/render-benchmark.py`; verify with `--check`.
