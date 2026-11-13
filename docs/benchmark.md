# Lokad.Image benchmarks

Current development version **0.2.1**. Warm single-CPU parity remains open across
**289 named material paths**; these **139 comparisons** summarize that scope.
This is an incremental update: affected costs use the qualified immutable package
DLL; unchanged paths reuse their identified evidence. It is not a full release refresh.

[Shared word matrices](../benchmarks/COUNTERCLOCKWISE-WORD-MATRIX.md) closes the
RGBA16 counterclockwise turn: **39.79 / FFmpeg 48.27 µs, upper 0.884**, **112 B/op**.
Independent saving is **10.90 µs / 21.51% raw**, **27.18% reference-normalized**;
the native clock moves **+7.79%**. Shared word transpose and byte transpose pass;
the existing byte counterclockwise gap remains. No extra owned/retained capacity.

[Word product reuse](../benchmarks/LANCZOS-WORD-PRODUCT-REUSE.md) closes prepared
RGBA16 Lanczos reduction: **485.36 / zimg 512.07 µs, upper 0.958**, with **138 B/op**
and **156,248 B** retained plan. Independent gain is **6.18% reference-normalized**;
the **13.33% raw** view includes a **−7.62%** reference clock change. All four
affected one-shot/prepared byte/word controls pass and stop. Exact pixels, admission
and owned capacity stay unchanged; temporary product/sum views reuse existing owners.

[Packed mixed-word storage](../benchmarks/MIXED-WORD-PACKED-CACHE.md) passes the
bounded bicubic control: **820.00 / zimg 846.10 µs, upper 0.978**, **113 B/op**.
Independent saving is **35.58 µs / 4.16% raw**, **3.38% reference-normalized**;
both are confirmed. Tight vertical independently saves **23.04 µs / 4.79% raw**;
default workspace remains uncertain. The earlier candidate's vertical regression
is retained. The borrowed view is 1 KiB smaller, with no owned/admitted/retained
capacity reduction. Stop passes.

The [released-storage view](../benchmarks/RELEASED-IMAGE-STORAGE.md), with these twelve
updated costs, has **165 pass, 10 gaps, 16 uncertain** across 191 existing comparisons.
Unchanged paths reuse their identified source-valid evidence. The complete
RGBA16 PNG decode/orient/crop/fit operation independently saves **317.42 µs / 9.74%**
and **1,063,078 allocated B/op**. Its parity remains uncertain. The bounded byte
128×64 control has a recorded **1.59 µs raw adverse effect**; its normalized effect
is inconclusive and both contemporary builds miss parity. No adverse or uncertain
capture is hidden. Remaining demonstrated gaps include bounded byte wide
bicubic and byte quarter turns; the retained horizontal-CMYK JPEG view
has an equivalent fresh-context pass. Tiny copy/turn gaps remain in the report.

Every operation runs sequentially on the dedicated quiet VM, pinned to one CPU,
with .NET 10, optimized JIT and nonconcurrent workstation GC. Startup, transport,
fixture preparation and validation are outside the clocks. Timed calls retain
their declared normalization, input delivery, output ownership and disposal.
Ratios are Lokad/reference; **upper** is the conservative confidence bound, with
all actuals retained. Upper ≤1 passes, lower >1 is a gap; otherwise uncertainty stays open.
**GC B/op** reports managed allocation, including result ownership where applicable,
not peak live capacity or native allocation.

The default store still has **two slots / 4 MiB**. Source-derived word-composition
idle payload rises **1,327,128 → 2,125,872 B**; four simultaneous image payloads
remain **2,455,088 B**. Caller-governed storage bypasses that cache. Logical admitted
capacity, allocation and retained storage are separate; physical/native/JIT peaks
are unmeasured. Each OS passes **6,580 full/live checks**, focused normal/scalar
ownership checks and the **same actual Release-package consumer / three examples**.
Unchanged arithmetic/ISA qualification is reused; publication is postponed.

† Reference boundaries require care. Codec contexts or filter graphs may remain
prepared; packed conversion, independent payload acquisition and release are timed
as declared. **zimg** is FFmpeg's labeled scaler backend, using float intermediates;
managed Q30/rational pixel guarantees remain exact. One-shot and prepared results
are distinct. Grayscale JPEG uses approved **SIMD libjpeg-turbo**; lossless WebP uses
**libwebp exact=1** where labeled. These are benchmark tools, never production dependencies.

Text comparisons use **ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12**.
The complete one-shot and preparation-plus-eight-draw public integrations pass,
including every layout/fitting call and fresh canvas ownership. Preparation is
reported separately and the complete repeated batch amortized across eight draws.
Loading and prepared-draw rows remain scoped diagnostics: eager/lazy validation
and retained raster state differ; historical stage times are never added to infer
overall text cost. Font loading uses **FreeType 2.13.2 copy/open/retain/dispose**.
RGBA16 compositing uses **ImageSharp Rgba64 SrcOver**, not FFmpeg. Keep the approved
[visual/native bounds](../benchmarks/TEXT-DRAWING-CURRENT.md) distinct from exact
managed pixels. Complete encoder packets and narrow metadata exceptions retain
their [declared scope](codec-parity.md); equal quality numbers are not quality matching.

**Codecs**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Packet B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| WebP VP8 + alpha, fresh seekable Stream † | 5,815.99 | 6,005.64 | 0.968 / 0.974 | 240,496 | — |
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
| PNG RGBA8, one-shot SmallestSize9 / native9 † | 16,887.12 | 17,258.24 | 0.978 / 0.992 | 815,716 | 210,973 / 210,973 |
| PNG RGBA16, one-shot SmallestSize9 / native9 † | 19,824.87 | 20,956.16 | 0.946 / 0.958 | 2,432,924 | 963,202 / 963,202 |
| PNG RGBA8, one-shot Fastest1 / native1 † | 4,111.01 | 4,361.68 | 0.943 / 0.963 | 816,180 | 236,828 / 236,828 |
| PNG RGBA16, one-shot stored0 / native0 † | 389.62 | 1,387.71 | 0.281 / 0.288 | 1,143,324 | 1,066,552 / 1,066,552 |
| PNG grayscale8, one-shot Optimal6 / native6 † | 1,986.46 | 1,993.40 | 0.997 / 1.038 | 546,628 | 72,586 / 72,586 |
| PNG RGB8, one-shot Optimal6 / native6 † | 9,398.61 | 9,773.57 | 0.962 / 0.978 | 815,148 | 168,729 / 168,729 |
| PNG gray-alpha8, one-shot Optimal6 / native6 † | 4,197.60 | 4,477.89 | 0.937 / 0.953 | 547,596 | 120,366 / 120,366 |
| PNG RGBA8, one-shot Optimal6 / native6 † | 16,718.11 | 17,275.89 | 0.968 / 0.979 | 815,716 | 210,973 / 210,973 |
| PNG RGBA16, one-shot Optimal6 / native6 † | 19,971.87 | 20,773.12 | 0.961 / 0.968 | 2,432,924 | 963,202 / 963,202 |
| PNG RGB16, one-shot Optimal6 / native6 † | 15,326.44 | 16,232.30 | 0.944 / 0.955 | 2,426,756 | 739,472 / 739,472 |
| PNG grayscale16, one-shot Optimal6 / native6, red-plane reference † | 4,602.49 | 5,408.55 | 0.851 / 0.867 | 818,004 | 265,433 / 265,433 |
| PNG gray-alpha16, one-shot Optimal6 / native6 † | 9,892.73 | 10,500.84 | 0.942 / 0.952 | 1,356,164 | 506,887 / 506,887 |
| PNG RGBA8, one-shot stored0 / native0 † | 53.27 | 541.40 | 0.098 / 0.114 | 607,628 | 533,444 / 533,444 |
| PNG RGBA8 decode, live token † | 1,358.23 | 1,393.80 | 0.974 / 0.980 | 4,472 | — |
| PNG RGBA8 decode, tight working limit † | 1,178.41 | 1,396.38 | 0.844 / 0.853 | 68,224 | — |
| PNG RGBA16 decode, tight working limit † | 2,821.89 | 2,902.08 | 0.972 / 0.978 | 70,272 | — |
| PNG predictable RGBA16 3000², Optimal6 / mixed6 † | 259,795.57 | 388,008.23 | 0.670 / 0.685 | 2,491,252 | 573,041 / 573,041 |
| PNG RGBA16, one-shot Fastest1 / native1 † | 17,266.79 | 18,600.99 | 0.928 / 0.934 | 2,432,924 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, live cancelable token † | 17,573.86 | 18,607.98 | 0.944 / 0.954 | 2,432,924 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, forward-only output † | 17,196.47 | 18,607.32 | 0.924 / 0.929 | 2,432,956 | 968,216 / 968,216 |
| PNG RGBA8 async, one-shot Optimal6, nonseekable output † | 16,720.65 | 17,676.67 | 0.946 / 0.961 | 877,476 | 210,973 / 210,973 |
| JPEG baseline420, resident decode † | 677.27 | 1,007.87 | 0.672 / 0.678 | 258,456 | — |
| JPEG DC-only, caller CMYK decode (confirmed pass) † | 318.93 | 330.45 | 0.965 / 0.972 | 1,085,981 | — |
| VP8L RGBA8, resident decode † | 2,709.07 | 3,019.90 | 0.897 / 0.902 | 152,728 | — |
| VP8L RGBA8, fresh seekable Stream decode † | 2,730.45 | 3,029.07 | 0.901 / 0.905 | 152,824 | — |
| VP8L palette16 RGBA8, resident decode † | 408.48 | 545.38 | 0.749 / 0.759 | 288,464 | — |
| JPEG CMYK sparse-2D, resident decode † | 417.17 | 434.69 | 0.960 / 0.979 | 20,944 | — |
| JPEG YCCK420, resident decode † | 419.05 | 709.44 | 0.591 / 0.592 | 895,563 | — |
| JPEG 4:1:1, resident decode † | 384.80 | 447.28 | 0.860 / 0.865 | 219,551 | — |
| JPEG 4:1:0, resident decode † | 335.71 | 508.89 | 0.660 / 0.670 | 189,091 | — |
| VP8L RGBA16, including normalization / libwebp exact=1 † | 114,600.56 | 131,281.45 | 0.873 / 0.887 | 5,948,664 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 112,507.39 | 131,039.53 | 0.859 / 0.869 | 5,949,048 | 183,812 / 183,812 |
| JPEG caller RGB24 async nonseekable, 420 (parity confirmed) † | 1,581.70 | 1,629.27 | 0.971 / 0.979 | 309,524 | 52,091 / 52,093 |
| JPEG RGBA8 async nonseekable, 444 (parity confirmed) † | 3,517.84 | 3,856.40 | 0.912 / 0.918 | 820,531 | 133,039 / 133,041 |
| PNG8 decode, two retained images / shared caller root † | 2,611.56 | 2,853.33 | 0.915 / 0.919 | 1,638,214 | — |
| JPEG dense CMYK, resident decode † | 1,269.91 | 3,112.28 | 0.408 / 0.413 | 20,944 | — |
| JPEG horizontal-only CMYK, resident decode (gap) † | 400.21 | 386.59 | 1.035 / 1.040 | 20,944 | — |
| JPEG YCCK420, caller RGB24 decode (confirmed pass) † | 429.43 | 605.39 | 0.709 / 0.715 | 762,717 | — |
| JPEG progressive grayscale, resident decode † | 1,590.44 | 1,713.68 | 0.928 / 0.934 | 151,432 | — |
| JPEG progressive420, resident decode † | 1,418.37 | 1,954.92 | 0.726 / 0.731 | 243,676 | — |
| JPEG progressive422, resident decode † | 1,784.86 | 2,245.97 | 0.795 / 0.800 | 315,496 | — |
| JPEG ordinary progressive444, resident decode † | 2,849.49 | 3,361.93 | 0.848 / 0.854 | 169,664 | — |
| JPEG progressive444, live-token resident decode † | 3,035.18 | 3,362.21 | 0.903 / 0.907 | 169,664 | — |

**Editing**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 256×129 | 596.90 | 1,576.30 | 0.379 / 0.382 | 138,647 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 256×129 | 462.40 | 488.52 | 0.947 / 0.953 | 138 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 256×129 | 661.63 | 1,809.24 | 0.366 / 0.382 | 155,078 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 256×129 | 485.36 | 512.07 | 0.948 / 0.958 | 138 |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 1027×519 | 3,770.44 | 5,255.86 | 0.717 / 0.727 | 319,224 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 1027×519 | 3,554.71 | 4,074.82 | 0.872 / 0.882 | 148 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 1027×519 | 4,117.46 | 5,096.28 | 0.808 / 0.821 | 4,592,777 |
| Lanczos3 prepared RGBA16 / zimg prepared (uncertain) † → 1027×519 | 3,989.78 | 3,911.18 | 1.020 / 1.035 | 4,265,332 |
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
| Bicubic RGBA8, fresh managed / prepared zimg (uncertain) † → 128×64 | 342.20 | 350.44 | 0.976 / 1.004 | 198,350 |
| Bicubic RGBA8, width up / height down, input+output+4 KiB † → 1027×129 | 830.84 | 864.78 | 0.961 / 0.967 | 113 |
| Bicubic RGBA16, width up / height down, input+output+4 KiB † → 1027×129 | 820.00 | 846.10 | 0.969 / 0.978 | 113 |
| Bicubic RGBA16 extreme reduction, input+output+4 KiB † → 2×9 | 428.16 | 1,261.83 | 0.339 / 0.353 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 64×32 | 353.91 | 388.26 | 0.912 / 0.921 | 113 |
| Bicubic RGBA8 reduction, input+output+4 KiB (gap) † → 128×64 | 357.38 | 347.95 | 1.027 / 1.031 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB (uncertain) † → 128×64 | 416.84 | 415.03 | 1.004 / 1.020 | 113 |
| Bicubic RGBA8, width down / height up, default workspace † → 256×519 | 771.67 | 844.28 | 0.914 / 0.926 | 199,947 |
| Bicubic RGBA16, width down / height up, default workspace † → 256×519 | 752.15 | 827.29 | 0.909 / 0.926 | 199,898 |
| Plain transpose RGBA16 † → 259×513 | 39.70 | 48.13 | 0.825 / 0.925 | 112 |
| Plain transpose RGBA8 † → 259×513 | 30.26 | 40.46 | 0.748 / 0.770 | 112 |
| Counterclockwise quarter turn RGBA16 † → 259×513 | 39.79 | 48.27 | 0.824 / 0.884 | 112 |
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
| Bilinear upscale RGBA8 † → 1027×519 | 1,964.35 | 1,990.85 | 0.987 / 0.992 | 102,891 |
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

**Inspection and pixel-coding validation**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Probe PNG dimensions, seekable | 0.158 | 0.123 | 1.284 / 1.328 | 744 | Confirmed gap |
| Probe JPEG dimensions, seekable | 0.326 | 0.194 | 1.681 / 1.736 | 744 | Confirmed gap |
| Probe WebP dimensions, seekable | 0.165 | 0.125 | 1.323 / 1.425 | 744 | Confirmed gap |
| Probe JPEG dimensions, async nonseekable | 0.404 | 0.201 | 2.007 / 2.052 | 744 | Confirmed gap |
| Validate PNG8, span † | 1,276.826 | 1,437.380 | 0.888 / 0.935 | 240 | Confirmed pass |
| Validate PNG16, span † | 2,972.985 | 3,204.750 | 0.928 / 0.932 | 240 | Confirmed pass |
| Validate indexed Adam7 19×17, span † | 1.801 | 3.966 | 0.454 / 0.472 | 792 | Confirmed pass |
| Validate JPEG baseline420, span † | 483.057 | 674.268 | 0.716 / 0.720 | 10,016 | Confirmed pass |
| Validate JPEG progressive420, span † | 288.749 | 297.280 | 0.971 / 0.984 | 4,088 | Confirmed pass |
| Validate VP8L+alpha, span † | 2,314.233 | 3,046.809 | 0.760 / 0.767 | 297,070 | Confirmed pass |
| Validate VP8+alpha, span † | 4,344.720 | 5,438.871 | 0.799 / 0.801 | 15,600 | Confirmed pass |
| Validate PNG16, async nonseekable † | 2,785.658 | 3,607.668 | 0.772 / 0.775 | 360 | Confirmed pass |
| Inspect PNG8 complete headers, seekable, adapter reference | 0.703 | 0.579 | 1.215 / 1.235 | 8,528 | Confirmed gap |
| Identify PNG8 complete container, resident, adapter reference | 13.079 | 13.049 | 1.002 / 1.005 | 96 | Confirmed gap |
| Identify JPEG420, resident, canonical adapter | 0.501 | 1.193 | 0.420 / 0.426 | 96 | Confirmed pass |
| Identify VP8L+alpha, resident, parser adapter | 0.022 | 4.985 | 0.004 / 0.005 | 96 | Confirmed pass |
| Identify PNG8, fresh seekable Stream, adapter reference | 17.492 | 15.921 | 1.099 / 1.103 | 192 | Confirmed gap |
| Identify PNG8, async nonseekable, adapter reference | 18.367 | 19.282 | 0.953 / 0.956 | 216 | Confirmed pass |
| JPEG Adobe CMYK horizontal AC / FFmpeg fresh contexts † | 408.406 | 750.674 | 0.544 / 0.547 | 20,944 | Confirmed pass |

[Coverage and current family reports](../benchmarks/WARM-COVERAGE.md) identify
remaining paths, boundaries, allocations, references and raw evidence. Superseded
experiments remain linked separately. Update with `python benchmarks/render-benchmark.py`;
verify with `--check`.
