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

- **WebP:** [current lossless/alpha storage](../benchmarks/WEBP-HUFFMAN-WORKSPACE.md)
  has 4 passes / 4 uncertain encode controls; the [lossy scope](../benchmarks/VP8-ROUNDED-BORDER-PREDICTION.md)
  has 5 passes / 4 uncertain. Resolve uncertainty before tuning. Complete packets remain
  exact. [Lossless decoding](../benchmarks/WEBP-PALETTE-OUTPUT.md) has 3 passes / 3 uncertain;
  VP8 has 2 passes / 3 gaps / 1 uncertain. Live opaque decode remains 4,881.47/native 4,792.22µs,
  upper1.028. Selected codes/results retain independent ownership; scratch0.
- **PNG:** [current decoding](../benchmarks/PNG-CURRENT-FRAMING-COSTS.md)
  has 49 passes / 1 gap / 1 uncertain across 51 controls. Live materialized RGBA16 remains
  2,919.94/native2,894.87µs, upper1.012. [One-shot encoding](../benchmarks/PNG-LENGTH-CLASSIFICATION.md)
  has 13 passes / 2 uncertain across 15 controls; ByteFastest/Gray8 need confidence resolution.
  [The current Fastest check](../benchmarks/PNG-FASTEST-CURRENT-CONFIDENCE.md) stays
  uncertain. Fresh native codec/zlib state, normalization and independent packets
  are timed. Exact packets and existing admission remain; no global parity claim.
- **JPEG:** [compiled token descriptors](../benchmarks/JPEG-PREPARED-TOKEN-DESCRIPTORS.md)
  save **35.00 µs/2.18% raw/2.87% normalized** on complete direct RGB444,
  with separated filtered/all-actual raw/normalized intervals. Current
  1571.03/native1563.64 µs,
  upper1.013 stays uncertain near parity; one targeted check resolves
  initial managed variation, with every actual retained. The existing 16,408 B
  table per prepared alphabet and scratch retention 0 remain; primary GC is
  42,936 B/op, without new owned capacity. All 23 baseline costs refresh; six
  unchanged progressive costs are reused. Across 29 reads: **22 pass / 4 gaps /
  3 uncertain**. RGB420/RGB422, sparse CMYK and caller CMYK remain gaps;
  RGB444, horizontal CMYK and progressive live444 need confidence resolution.
  [Encoding](../benchmarks/JPEG-GRAY-TILE-PREPARATION.md) retains scoped grayscale
  gains and 15 unchanged color costs. Default/CompactColor and content/sampling
  claims remain distinct in the packet table. Frozen packets/metadata gates apply.
- **Resizing:** [wide result-row reuse](../benchmarks/BICUBIC-RESULT-ROW-LOAN.md)
  saves **58.90µs/12.66% raw/8.94% normalized**,
  with separated filtered/all-actual raw/normalized intervals. Wide byte is
  406.16/zimg367.01µs,
  upper1.119 gap; refreshed word upper1.003 is uncertain.
  Its changed driver prevents substituting the older passing word cost; current
  intervals do not prove a regression. No unchanged favorable repeat.
  Cache/weights borrow 10,880 B from unpublished output, with no added owned capacity;
  same 4 KiB admission, 113 B/op, 32,768 B result and scratch 0. The existing large
  default/compact pair screens 20.53/21.64 ms, three actuals each, without a native
  or historical-gain claim. [Narrow preparation](../benchmarks/BICUBIC-PAIRED-COEFFICIENT-PREPARATION.md)
  retains 33.29 µs savings/upper 1.167 gap. [Cached-wide](../benchmarks/CACHED-WIDE-CERTIFIED-PUBLICATION.md)
  has 7 passes / 1 gap; [packed reduction](../benchmarks/PACKED-BICUBIC-INDEXED-REDUCTION.md)
  has 2 gaps. [Lanczos](../benchmarks/LANCZOS-PAIRED-BYTE-ROWS.md) has 6 passes / 1 gap / 1 uncertain;
  resolve the byte uncertainty before tuning. Exact managed Q30/rational results
  remain distinct from native float. Useful specialized kernels/fallbacks remain.
- **Text/drawing:** [complete layout/fitting and RGBA16 composition](../benchmarks/TEXT-DRAWING-CURRENT.md)
  pass/stop, as does [exact RGBA8 composition](../benchmarks/BYTE-COMPOSITE-EXACT-VECTOR.md).
  [Prepared drawing](../benchmarks/TEXT-ENDPOINT-STORAGE.md) retains 59,000 B/op and
  current 98.44/102.57 µs points, with reference gaps and inconclusive CPU gains.
  [Font loading](../benchmarks/FONT-OWNED-STORAGE.md) remains 31.86/FreeType 13.42 µs,
  upper 2.396 gap, 176 B/op; 757,076 B idle pooled capacity is separate. These cases do
  not block codec/resizing work. Existing pools and governed bypass remain.

The current immutable build passes **6,172 full/live checks per OS**
and the **same actual Release-package consumer/all three usage examples on
Windows/Linux**. JPEG checks pass 458 internal/375 public per
normal/scalar/disabled-AVX2/AVX512 mode; unchanged resize evidence (341 internal/729 public)
is reused. Exact packets/pixels, shared admission,
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
| JPEG baseline420, resident decode (confirmed pass) † | 906.60 | 1,085.68 | 0.835 / 0.843 | 258,446 | — |
| JPEG DC-only, caller CMYK decode † | 365.86 | 353.37 | 1.035 / 1.047 | 1,085,975 | — |
| VP8L RGBA8, resident decode (confidence open) † | 3,085.86 | 3,021.48 | 1.021 / 1.035 | 152,728 | — |
| VP8L RGBA8, fresh seekable Stream decode (confidence open) † | 3,290.70 | 3,268.51 | 1.007 / 1.021 | 152,824 | — |
| VP8L palette16 RGBA8, resident decode (confirmed pass) † | 461.31 | 590.92 | 0.781 / 0.789 | 288,462 | — |
| JPEG CMYK sparse-2D, resident decode † | 484.96 | 437.68 | 1.108 / 1.121 | 20,944 | — |
| JPEG YCCK420, resident decode (confirmed pass) † | 458.05 | 756.80 | 0.605 / 0.612 | 895,562 | — |
| JPEG 4:1:1, resident decode (confirmed pass) † | 388.42 | 477.04 | 0.814 / 0.884 | 219,552 | — |
| JPEG 4:1:0, resident decode (confirmed pass) † | 346.77 | 508.74 | 0.682 / 0.703 | 189,091 | — |
| VP8L RGBA16, including normalization / libwebp exact=1 † | 114,600.56 | 131,281.45 | 0.873 / 0.887 | 5,948,664 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 112,507.39 | 131,039.53 | 0.859 / 0.869 | 5,949,048 | 183,812 / 183,812 |
| JPEG caller RGB24 async nonseekable, 420 (parity confirmed) † | 1,581.70 | 1,629.27 | 0.971 / 0.979 | 309,524 | 52,091 / 52,093 |
| JPEG RGBA8 async nonseekable, 444 (parity confirmed) † | 3,517.84 | 3,856.40 | 0.912 / 0.918 | 820,531 | 133,039 / 133,041 |
| PNG8 decode, two retained images / shared caller root † | 2,610.86 | 2,847.04 | 0.917 / 0.920 | 1,638,214 | — |
| JPEG dense CMYK, resident decode (confirmed pass) † | 1,387.46 | 3,110.83 | 0.446 / 0.449 | 20,944 | — |
| JPEG horizontal-only CMYK, resident decode (confidence open) † | 428.38 | 397.03 | 1.079 / 1.098 | 20,944 | — |
| JPEG YCCK420, caller RGB24 decode (confirmed pass) † | 489.76 | 646.09 | 0.758 / 0.764 | 762,717 | — |
| JPEG progressive grayscale, resident decode (confirmed pass) † | 1,630.45 | 1,711.66 | 0.953 / 0.964 | 151,432 | — |
| JPEG progressive420, resident decode (confirmed pass) † | 1,559.16 | 1,948.18 | 0.800 / 0.809 | 243,664 | — |
| JPEG progressive422, resident decode (confirmed pass) † | 2,201.41 | 2,439.31 | 0.902 / 0.913 | 315,496 | — |
| JPEG ordinary progressive444, resident decode (confirmed pass) † | 3,172.92 | 3,355.21 | 0.946 / 0.949 | 169,664 | — |
| JPEG progressive444, live-token resident decode (confidence open) † | 3,338.76 | 3,355.58 | 0.995 / 1.012 | 169,664 | — |

**Editing**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 256×129 | 672.38 | 1,696.51 | 0.396 / 0.400 | 138,647 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 256×129 | 491.59 | 489.18 | 1.005 / 1.011 | 138 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 256×129 | 684.08 | 1,812.27 | 0.377 / 0.380 | 146,866 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 256×129 | 587.91 | 548.90 | 1.071 / 1.095 | 139 |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 1027×519 | 3,765.94 | 5,244.39 | 0.718 / 0.723 | 319,224 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 1027×519 | 3,780.94 | 4,376.66 | 0.864 / 0.877 | 148 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 1027×519 | 4,393.34 | 5,486.14 | 0.801 / 0.808 | 4,592,777 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 1027×519 | 3,784.95 | 3,898.01 | 0.971 / 0.979 | 4,265,332 |
| Bilinear resize RGBA8 † H → 256×129 | 504.78 | 387.47 | 1.303 / 1.315 | 157,971 |
| Bilinear upscale RGBA8 (parity confirmed) † → 1027×519 | 2,027.72 | 2,165.52 | 0.936 / 0.993 | 102,891 |
| Bilinear resize RGBA16 † H → 256×129 | 470.26 | 418.16 | 1.125 / 1.133 | 272,815 |
| Bicubic resize RGBA8 † → 256×129 | 460.84 | 418.20 | 1.102 / 1.106 | 199,981 |
| Bicubic upscale RGBA16 † → 1027×519 | 2,752.40 | 2,874.23 | 0.958 / 0.966 | 4,264,573 |
| Bicubic upscale RGBA8, default workspace † → 1027×519 | 3,108.21 | 3,037.83 | 1.023 / 1.026 | 116 |
| Bicubic RGBA16, width down / height up / input+output+4 KiB † → 256×519 | 862.45 | 825.16 | 1.045 / 1.049 | 114 |
| Bicubic RGBA8, 32×16 / input+output+4 KiB H → 32×16 | 464.68 | 375.02 | 1.239 / 1.370 | 2,185 |
| Bicubic RGBA8, 32×16 / default workspace † → 32×16 | 329.13 | 376.50 | 0.874 / 0.898 | 197,298 |
| Bicubic RGBA8 enlargement, input+output+4 KiB † → 1027×519 | 3,114.50 | 3,041.43 | 1.024 / 1.030 | 116 |
| Bicubic RGBA8 width enlargement, input+output+4 KiB † → 1027×259 | 1,546.70 | 1,524.36 | 1.015 / 1.066 | 114 |
| Bicubic RGBA8 height enlargement, input+output+4 KiB † → 513×519 | 1,811.37 | 1,745.56 | 1.038 / 1.061 | 115 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129 | 484.90 | 417.35 | 1.162 / 1.167 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 256×129 | 560.37 | 572.84 | 0.978 / 0.985 | 113 |
| Bicubic RGBA8 wider-support reduction † → 128×64 | 395.96 | 345.54 | 1.146 / 1.206 | 198,450 |
| Bicubic RGBA8, width up / height down, input+output+4 KiB † → 1027×129 | 888.37 | 866.88 | 1.025 / 1.030 | 114 |
| Bicubic RGBA16, width up / height down, input+output+4 KiB † → 1027×129 | 893.16 | 851.29 | 1.049 / 1.056 | 114 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 64×32 | 428.98 | 366.56 | 1.170 / 1.177 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 64×32 | 416.24 | 387.97 | 1.073 / 1.086 | 113 |
| Bicubic RGBA8 reduction, input+output+4 KiB (gap) † → 128×64 | 406.16 | 367.01 | 1.107 / 1.119 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB (confidence open) † → 128×64 | 433.70 | 436.46 | 0.994 / 1.003 | 113 |
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
| Validate JPEG baseline420, span † | 483.057 | 674.268 | 0.716 / 0.720 | 10,016 | Confirmed pass |
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
