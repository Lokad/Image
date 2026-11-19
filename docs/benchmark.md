# Lokad.Image benchmarks

All **165 comparisons** pass on Lokad.Image **0.2.1**
from package `28fb64ab4529`. The [coverage inventory](../benchmarks/WARM-COVERAGE.md)
covers **289 required paths**, with scoped evidence for the remaining operations.

Measurements ran sequentially on one pinned CPU of the dedicated quiet VM under .NET 10
with optimized JIT and nonconcurrent workstation GC. Each operation includes its
declared input delivery, normalization, output ownership and disposal. Pass requires a
conservative ratio upper bound of 1 or less. The tables use Lokad/reference ratios.

† The reference can retain prepared codec contexts or filter graphs according to the
recorded boundary. One-shot rows include the named setup. zimg is the FFmpeg zscale
backend and uses float intermediates. Lokad resizing preserves its exact managed
pixel contract. Startup, fixture preparation and validation are outside the clocks.

Allocation is GC bytes per complete operation. A dash denotes an unmeasured value.
Governed peak capacity and retained state are recorded separately in the evidence.
Packet sizes cover the declared [codec-parity controls](codec-parity.md).

Text rows use ImageSharp.Drawing 2.1.7, Fonts 2.1.3 and ImageSharp 3.1.12. The
five authored controls include the [reproducible reference corrections](../benchmarks/AUTHORED-FONT-COMPARISONS.md). The complete integrations include all
30 fitting trials. The repeated row measures preparation, eight draws and cleanup
together. FreeType loading retains its scoped copy/open/retain/dispose boundary.
Font validation eagerness and prepared-state retention differ between references.

Preparation alone took **1,122.30 / 6,141.30 µs**. The measured
eight-draw batch amortizes to **157.73 / 859.79 µs/draw**.
Managed preparation retained **4,160 B** of admitted capacity.
The approximate live-heap observations and the scoped preparation and steady-draw
diagnostics are recorded in the current evidence.

The test suites passed on Windows (6,870 normal and 6,870 scalar) and Linux (6,870 normal and 6,870 scalar), including live FFmpeg differential tests.
Both platforms also passed the consumer smoke test and all three usage examples
with the same measured package. [Current benchmark evidence](../benchmarks/final-package-benchmarks.json)

## Codecs

| Operation | Reference | Lokad µs | Ref. µs | Ratio / upper | GC B/op Lokad / ref. | Status | Packet B Lokad / ref. |
| --- | --- | ---: | ---: | ---: | ---: | --- | ---: |
| WebP VP8 + alpha, fresh seekable Stream † | FFmpeg | 5,644.048 | 5,989.030 | 0.942 / 0.943645 | 240,496 / — | Pass | — |
| JPEG 4:4:4, forward-only / exact reference † | FFmpeg | 3,151.520 | 3,773.163 | 0.835 / 0.844819 | 820,136 / — | Pass | 133,039 / 133,041 |
| JPEG 4:2:0 from caller RGB samples † | FFmpeg | 1,429.966 | 1,576.368 | 0.907 / 0.911691 | 388,220 / — | Pass | 52,091 / 52,093 |
| JPEG RGBA16 4:2:0 / caller IMemoryBudget † | FFmpeg | 1,640.861 | 1,837.860 | 0.893 / 0.900911 | 819,399 / — | Pass | 52,115 / 52,117 |
| JPEG grayscale, standalone SIMD libjpeg-turbo † | SIMD libjpeg-turbo | 970.185 | 1,404.965 | 0.691 / 0.694227 | 155,435 / — | Pass | 58,910 / 58,910 |
| JPEG from caller GRAY8, standalone SIMD libjpeg-turbo † | SIMD libjpeg-turbo | 914.511 | 1,078.636 | 0.848 / 0.858436 | 155,437 / — | Pass | 58,910 / 58,910 |
| VP8L photograph / libwebp exact=1 † | libwebp exact=1 | 82,466.371 | 84,207.946 | 0.979 / 0.981406 | 5,358,368 / — | Pass | 160,032 / 160,032 |
| VP8L text/diagram / libwebp exact=1 † | libwebp exact=1 | 7,775.280 | 9,569.517 | 0.813 / 0.861736 | 3,720,072 / — | Pass | 7,234 / 7,234 |
| VP8 opaque RGBA8 † | FFmpeg / libwebp | 14,429.514 | 15,618.227 | 0.924 / 0.972006 | 1,201,560 / — | Pass | 72,380 / 72,380 |
| VP8 opaque RGBA16, including normalization † | FFmpeg / libwebp | 13,530.498 | 15,529.715 | 0.871 / 0.877259 | 1,201,560 / — | Pass | 72,380 / 72,380 |
| VP8 opaque photograph † | FFmpeg / libwebp | 9,535.927 | 9,659.098 | 0.987 / 0.988422 | 809,080 / — | Pass | 17,688 / 17,688 |
| VP8 opaque text/diagram † | FFmpeg / libwebp | 5,344.806 | 5,820.659 | 0.918 / 0.929941 | 740,992 / — | Pass | 7,100 / 7,100 |
| VP8 diagram, effort2 / distortion-only search † | FFmpeg / libwebp | 2,474.509 | 2,515.602 | 0.984 / 0.984873 | 406,664 / — | Pass | 7,330 / 7,330 |
| VP8 diagram, effort6 / trellis † | FFmpeg / libwebp | 9,191.231 | 9,794.737 | 0.938 / 0.953162 | 754,352 / — | Pass | 6,818 / 6,818 |
| VP8 RGBA8 + lossless alpha † | FFmpeg / libwebp | 50,116.231 | 53,714.367 | 0.933 / 0.937725 | 6,241,792 / — | Pass | 98,590 / 98,590 |
| VP8 RGBA16 + alpha, including normalization † | FFmpeg / libwebp | 51,698.989 | 53,755.178 | 0.962 / 0.964409 | 6,241,792 / — | Pass | 98,590 / 98,590 |
| VP8L RGBA8 / libwebp exact=1 † | libwebp exact=1 | 107,126.733 | 121,093.328 | 0.885 / 0.896809 | 5,948,664 / — | Pass | 183,812 / 183,812 |
| VP8L opaque RGBA8 / libwebp exact=1 † | libwebp exact=1 | 68,513.063 | 71,301.085 | 0.961 / 0.965527 | 4,822,760 / — | Pass | 121,896 / 121,896 |
| PNG RGBA8, one-shot SmallestSize9 / native9 † | FFmpeg | 16,493.419 | 17,260.143 | 0.956 / 0.961978 | 815,654 / — | Pass | 210,973 / 210,973 |
| PNG RGBA16, one-shot SmallestSize9 / native9 † | FFmpeg | 19,935.272 | 20,968.768 | 0.951 / 0.960604 | 2,432,976 / — | Pass | 963,202 / 963,202 |
| PNG RGBA8, one-shot Fastest1 / native1 † | FFmpeg | 3,876.522 | 4,373.318 | 0.886 / 0.898906 | 816,118 / — | Pass | 236,828 / 236,828 |
| PNG RGBA16, one-shot stored0 / native0 † | FFmpeg | 209.104 | 1,389.857 | 0.150 / 0.244880 | 1,143,304 / — | Pass | 1,066,552 / 1,066,552 |
| PNG grayscale8, one-shot Optimal6 / native6 † | FFmpeg | 1,921.094 | 1,976.219 | 0.972 / 0.979306 | 546,525 / — | Pass | 72,586 / 72,586 |
| PNG RGB8, one-shot Optimal6 / native6 † | FFmpeg | 8,979.020 | 9,861.364 | 0.911 / 0.920887 | 815,086 / — | Pass | 168,729 / 168,729 |
| PNG gray-alpha8, one-shot Optimal6 / native6 † | FFmpeg | 4,027.909 | 4,484.899 | 0.898 / 0.929408 | 547,493 / — | Pass | 120,366 / 120,366 |
| PNG RGBA8, one-shot Optimal6 / native6 † | FFmpeg | 16,434.541 | 17,263.853 | 0.952 / 0.965836 | 815,654 / — | Pass | 210,973 / 210,973 |
| PNG RGBA16, one-shot Optimal6 / native6 † | FFmpeg | 19,833.494 | 20,773.143 | 0.955 / 0.966335 | 2,432,986 / — | Pass | 963,202 / 963,202 |
| PNG RGB16, one-shot Optimal6 / native6 † | FFmpeg | 14,862.371 | 16,142.598 | 0.921 / 0.930927 | 2,426,839 / — | Pass | 739,472 / 739,472 |
| PNG grayscale16, one-shot Optimal6 / native6, red-plane reference † | FFmpeg | 4,753.074 | 5,449.260 | 0.872 / 0.902296 | 817,942 / — | Pass | 265,433 / 265,433 |
| PNG gray-alpha16, one-shot Optimal6 / native6 † | FFmpeg | 9,733.664 | 10,496.227 | 0.927 / 0.935654 | 1,356,154 / — | Pass | 506,887 / 506,887 |
| PNG RGBA8, one-shot stored0 / native0 † | FFmpeg | 263.195 | 540.550 | 0.487 / 0.594169 | 607,587 / — | Pass | 533,444 / 533,444 |
| PNG RGBA8 decode, live token † | FFmpeg | 1,352.029 | 1,398.389 | 0.967 / 0.968199 | 4,472 / — | Pass | — |
| PNG RGBA8 decode, tight working limit † | FFmpeg | 1,242.266 | 1,509.768 | 0.823 / 0.827495 | 68,224 / — | Pass | — |
| PNG RGBA16 decode, tight working limit † | FFmpeg | 2,801.156 | 2,932.035 | 0.955 / 0.977442 | 70,272 / — | Pass | — |
| PNG predictable RGBA16 3000², Optimal6 / mixed6 † | FFmpeg | 255,601.002 | 404,494.848 | 0.632 / 0.677015 | 2,491,149 / — | Pass | 573,041 / 573,041 |
| PNG RGBA16, one-shot Fastest1 / native1 † | FFmpeg | 16,854.505 | 18,585.951 | 0.907 / 0.916577 | 2,432,976 / — | Pass | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, live cancelable token † | FFmpeg | 17,804.243 | 18,614.580 | 0.956 / 0.962617 | 2,432,976 / — | Pass | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, forward-only output † | FFmpeg | 16,814.165 | 18,645.160 | 0.902 / 0.913129 | 2,433,008 / — | Pass | 968,216 / 968,216 |
| PNG RGBA8 async, one-shot Optimal6, nonseekable output † | FFmpeg | 16,285.810 | 17,520.164 | 0.930 / 0.950289 | 877,476 / — | Pass | 210,973 / 210,973 |
| JPEG baseline420, resident decode † | FFmpeg | 718.725 | 1,010.104 | 0.712 / 0.719096 | 258,479 / — | Pass | — |
| JPEG DC-only, caller CMYK decode † | FFmpeg | 281.035 | 332.853 | 0.844 / 0.850230 | 536,380 / — | Pass | — |
| VP8L RGBA8, resident decode † | FFmpeg | 2,723.542 | 3,019.170 | 0.902 / 0.909394 | 152,728 / — | Pass | — |
| VP8L RGBA8, fresh seekable Stream decode † | FFmpeg | 2,851.508 | 3,033.267 | 0.940 / 0.943220 | 152,824 / — | Pass | — |
| VP8L palette16 RGBA8, resident decode † | FFmpeg | 477.271 | 544.408 | 0.877 / 0.886791 | 288,485 / — | Pass | — |
| JPEG CMYK sparse-2D, resident decode † | FFmpeg | 413.888 | 434.442 | 0.953 / 0.962244 | 20,948 / — | Pass | — |
| JPEG YCCK420, resident decode † | FFmpeg | 414.403 | 710.308 | 0.583 / 0.586958 | 895,570 / — | Pass | — |
| JPEG 4:1:1, resident decode † | FFmpeg | 373.322 | 447.645 | 0.834 / 0.842614 | 219,573 / — | Pass | — |
| JPEG 4:1:0, resident decode † | FFmpeg | 339.698 | 507.838 | 0.669 / 0.673099 | 189,109 / — | Pass | — |
| VP8L RGBA16, including normalization / libwebp exact=1 † | libwebp exact=1 | 107,199.706 | 121,298.967 | 0.884 / 0.893248 | 5,948,664 / — | Pass | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | libwebp exact=1 | 106,555.051 | 121,272.569 | 0.879 / 0.891845 | 5,949,048 / — | Pass | 183,812 / 183,812 |
| JPEG caller RGB24 async nonseekable, 420 † | FFmpeg | 1,507.497 | 1,687.476 | 0.893 / 0.972623 | 309,525 / — | Pass | 52,091 / 52,093 |
| JPEG RGBA8 async nonseekable, 444 † | FFmpeg | 3,160.574 | 3,845.049 | 0.822 / 0.832270 | 820,488 / — | Pass | 133,039 / 133,041 |
| PNG8 decode, two retained images / shared caller root † | FFmpeg | 2,582.758 | 2,853.541 | 0.905 / 0.914338 | 1,638,234 / — | Pass | — |
| JPEG RGBA8 4:2:0, row restart33 † | FFmpeg | 1,347.225 | 1,607.385 | 0.838 / 0.842969 | 151,179 / — | Pass | 58,010 / 58,012 |
| JPEG dense CMYK, resident decode † | FFmpeg | 1,264.768 | 3,120.186 | 0.405 / 0.410533 | 20,944 / — | Pass | — |
| JPEG horizontal-only CMYK, resident decode † | FFmpeg | 379.432 | 387.649 | 0.979 / 0.983404 | 20,944 / — | Pass | — |
| JPEG YCCK420, caller RGB24 decode † | FFmpeg | 438.637 | 607.368 | 0.722 / 0.732637 | 762,727 / — | Pass | — |
| JPEG progressive grayscale, resident decode † | FFmpeg | 1,587.342 | 1,710.602 | 0.928 / 0.944530 | 151,471 / — | Pass | — |
| JPEG progressive420, resident decode † | FFmpeg | 1,427.015 | 1,954.867 | 0.730 / 0.735124 | 243,710 / — | Pass | — |
| JPEG progressive422, resident decode † | FFmpeg | 1,807.082 | 2,258.805 | 0.800 / 0.809941 | 315,526 / — | Pass | — |
| JPEG ordinary progressive444, resident decode † | FFmpeg | 2,898.086 | 3,363.268 | 0.862 / 0.869834 | 169,664 / — | Pass | — |
| JPEG progressive444, live-token resident decode † | FFmpeg | 2,987.405 | 3,360.340 | 0.889 / 0.895994 | 169,664 / — | Pass | — |

## Editing and text

| Operation | Reference | Lokad µs | Ref. µs | Ratio / upper | GC B/op Lokad / ref. | Status |
| --- | --- | ---: | ---: | ---: | ---: | --- |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 256×129 | zimg | 576.135 | 1,572.633 | 0.366 / 0.371436 | 138,650 / — | Pass |
| Lanczos3 prepared RGBA8 / zimg prepared † → 256×129 | zimg | 459.220 | 490.114 | 0.937 / 0.953152 | 138 / — | Pass |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 256×129 | zimg | 628.551 | 1,811.122 | 0.347 / 0.351309 | 155,078 / — | Pass |
| Lanczos3 prepared RGBA16 / zimg prepared † → 256×129 | zimg | 492.370 | 508.874 | 0.968 / 0.977637 | 138 / — | Pass |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 1027×519 | zimg | 3,742.925 | 5,249.112 | 0.713 / 0.725694 | 319,224 / — | Pass |
| Lanczos3 prepared RGBA8 / zimg prepared † → 1027×519 | zimg | 3,362.130 | 4,047.127 | 0.831 / 0.834619 | 148 / — | Pass |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 1027×519 | zimg | 3,899.567 | 5,090.134 | 0.766 / 0.774796 | 4,592,818 / — | Pass |
| Lanczos3 prepared RGBA16 / zimg prepared † → 1027×519 | zimg | 3,775.495 | 3,898.332 | 0.968 / 0.982796 | 4,264,760 / — | Pass |
| Bicubic resize RGBA8 † → 256×129 | zimg | 393.236 | 420.990 | 0.934 / 0.939054 | 112 / — | Pass |
| Bicubic upscale RGBA16 † → 1027×519 | zimg | 2,418.737 | 2,875.022 | 0.841 / 0.851390 | 4,264,752 / — | Pass |
| Bicubic RGBA8, width reduction / height enlargement, input+output+4 KiB † → 256×519 | zimg | 754.916 | 844.065 | 0.894 / 0.898207 | 113 / — | Pass |
| Bicubic RGBA16, width down / height up / input+output+4 KiB † → 256×519 | zimg | 788.323 | 826.911 | 0.953 / 0.957213 | 113 / — | Pass |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 32×16 | zimg | 405.804 | 415.284 | 0.977 / 0.979981 | 113 / — | Pass |
| Bicubic RGBA8 extreme reduction, input+output+4 KiB † → 2×9 | zimg | 438.657 | 1,059.738 | 0.414 / 0.425773 | 113 / — | Pass |
| Bicubic RGBA8 enlargement, input+output+4 KiB † → 1027×519 | zimg | 2,827.389 | 3,048.305 | 0.928 / 0.938214 | 116 / — | Pass |
| Bicubic RGBA8 width enlargement, input+output+4 KiB † → 1027×259 | zimg | 1,416.008 | 1,522.985 | 0.930 / 0.935465 | 114 / — | Pass |
| Bicubic RGBA8 height enlargement, input+output+4 KiB † → 513×519 | zimg | 1,397.127 | 1,748.627 | 0.799 / 0.803475 | 114 / — | Pass |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129 | zimg | 385.641 | 422.644 | 0.912 / 0.920920 | 112 / — | Pass |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 256×129 | zimg | 488.826 | 577.364 | 0.847 / 0.861267 | 113 / — | Pass |
| Bicubic RGBA8, fresh managed / prepared zimg † → 128×64 | zimg | 334.285 | 349.479 | 0.957 / 0.969803 | 198,350 / — | Pass |
| Bicubic RGBA8, width up / height down, input+output+4 KiB † → 1027×129 | zimg | 828.835 | 869.065 | 0.954 / 0.967183 | 114 / — | Pass |
| Bicubic RGBA16, width up / height down, input+output+4 KiB † → 1027×129 | zimg | 811.836 | 844.003 | 0.962 / 0.970763 | 113 / — | Pass |
| Bicubic RGBA16 extreme reduction, input+output+4 KiB † → 2×9 | zimg | 446.331 | 1,305.111 | 0.342 / 0.353475 | 113 / — | Pass |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 64×32 | zimg | 362.716 | 391.137 | 0.927 / 0.936207 | 113 / — | Pass |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 128×64 | zimg | 342.078 | 349.994 | 0.977 / 0.997524 | 112 / — | Pass |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 128×64 | zimg | 374.981 | 407.649 | 0.920 / 0.933034 | 112 / — | Pass |
| Bicubic RGBA8, width down / height up, default workspace † → 256×519 | zimg | 760.645 | 839.367 | 0.906 / 0.928892 | 199,969 / — | Pass |
| Bicubic RGBA16, width down / height up, default workspace † → 256×519 | zimg | 725.785 | 827.902 | 0.877 / 0.884826 | 199,914 / — | Pass |
| Exact rectangle copy into borrowed RGBA8 canvas | FFmpeg adapter | 1.584 | 1.916 | 0.827 / 0.835105 | 0 / — | Pass |
| Exact rectangle copy into borrowed RGBA16 canvas (confidence open) | FFmpeg adapter | 3.323 | 3.782 | 0.879 / 0.885121 | 0 / — | Pass |
| Exact clipped fill into borrowed RGBA8 canvas | FFmpeg adapter | 1.188 | 1.470 | 0.808 / 0.818121 | 0 / — | Pass |
| Exact clipped fill into borrowed RGBA16 canvas | FFmpeg adapter | 2.032 | 4.085 | 0.497 / 0.504838 | 0 / — | Pass |
| Overlapping RGBA8 copy + reverse restoration, per pair | FFmpeg adapter | 3.742 | 3.803 | 0.984 / 0.987203 | 0 / — | Pass |
| Original overlapping RGBA16 copy, 257×129, shift (1,1), per pair | FFmpeg adapter | 4.025 | 4.435 | 0.908 / 0.933324 | 0 / — | Pass |
| RGBA8 source-over / FFmpeg overlay † | FFmpeg | 66.496 | 450.779 | 0.148 / 0.173671 | 0 / — | Pass |
| RGBA16 source-over / ImageSharp 3.1.12 † | ImageSharp 3.1.12 Rgba64 | 566.348 | 629.206 | 0.900 / 0.914659 | 0 / — | Pass |
| Original self-composite RGBA16 / ImageSharp snapshot + SrcOver † | ImageSharp 3.1.12 Rgba64 | 620.612 | 709.122 | 0.875 / 0.878911 | 0 / 26,800 | Pass |
| Complete text one-shot / ImageSharp public integration † | ImageSharp public API | 1,115.511 | 6,253.352 | 0.178 / 0.200012 | 304,933 / 9,256,953 | Pass |
| Complete text preparation + eight draws / ImageSharp public integration † | ImageSharp public API | 1,261.820 | 6,878.318 | 0.183 / 0.204212 | 314,664 / 9,494,633 | Pass |
| Text ordinary draw RGBA8 / ImageSharp.Drawing † | ImageSharp public API | 45.225 | 45.672 | 0.990 / 0.999263 | 83,008 / 48,576 | Pass |
| Text ordinary draw RGBA16 / ImageSharp.Drawing † | ImageSharp public API | 45.095 | 46.856 | 0.962 / 0.973094 | 83,008 / 48,576 | Pass |
| Text prepared coverage RGBA8 / ImageSharp.Drawing † | ImageSharp public API | 2.870 | 36.562 | 0.078 / 0.084946 | 56 / — | Pass |
| Text prepared coverage RGBA16 / ImageSharp.Drawing † | ImageSharp public API | 5.299 | 37.532 | 0.141 / 0.150120 | 56 / — | Pass |
| Font loading / FreeType 2.13.2 † | FreeType 2.13.2 | 12.952 | 13.377 | 0.968 / 0.975453 | 176 / — | Pass |
| Wrapped logical layout / SixLabors public API | ImageSharp public API | 5.272 | 85.919 | 0.061 / 0.089167 | 3,153 / — | Pass |
| Latin OpenType layout / SixLabors public API | ImageSharp public API | 17.999 | 50.345 | 0.358 / 0.580138 | 6,409 / — | Pass |
| Arabic OpenType layout / SixLabors public API | ImageSharp public API | 78.042 | 295.135 | 0.264 / 0.404250 | 28,658 / — | Pass |
| Complete fitting search / SixLabors public API | ImageSharp public API | 1,013.871 | 1,621.343 | 0.625 / 0.703525 | 224,179 / — | Pass |
| Bilinear upscale RGBA8 † → 1027×519 | FFmpeg | 1,976.586 | 2,164.788 | 0.913 / 0.926187 | 102,890 / — | Pass |
| Bilinear RGBA16 enlargement, fresh managed / prepared FFmpeg graph † → 1027×519 | FFmpeg | 1,893.769 | 2,019.088 | 0.938 / 0.949597 | 4,498,579 / — | Pass |
| Bilinear one-shot RGBA16 / FFmpeg one-shot † → 1027×519 | FFmpeg | 1,863.749 | 4,835.298 | 0.385 / 0.506960 | 4,498,579 / — | Pass |
| Bilinear resize RGBA8 † → 256×129 | FFmpeg | 354.110 | 389.518 | 0.909 / 0.916496 | 8,473 / — | Pass |
| Bilinear resize RGBA16 † → 256×129 | FFmpeg | 381.138 | 421.815 | 0.904 / 0.918475 | 8,473 / — | Pass |
| Bicubic RGBA8, width reduction / same height, input+output+4 KiB † → 256×259 | zimg | 487.983 | 571.411 | 0.854 / 0.866162 | 113 / — | Pass |
| Bicubic one-shot RGBA8 / zimg one-shot † → 128×64 | zimg | 313.392 | 992.548 | 0.316 / 0.319096 | 198,456 / — | Pass |
| Bicubic prepared RGBA8 / zimg prepared † → 128×64 | zimg | 280.290 | 347.317 | 0.807 / 0.815465 | 139 / — | Pass |
| Bicubic one-shot RGBA16 / zimg one-shot † → 128×64 | zimg | 377.456 | 594.880 | 0.635 / 0.638963 | 198,451 / — | Pass |
| Bicubic prepared RGBA16 / zimg prepared † → 128×64 | zimg | 301.122 | 410.982 | 0.733 / 0.740276 | 139 / — | Pass |
| Bicubic one-shot narrow RGBA8 / zimg one-shot † → 256×129 | zimg | 385.388 | 1,043.639 | 0.369 / 0.372509 | 138 / — | Pass |
| Bicubic prepared narrow RGBA8 / zimg prepared † → 256×129 | zimg | 404.341 | 417.118 | 0.969 / 0.971772 | 136 / — | Pass |
| Bicubic one-shot narrow RGBA16 / zimg one-shot † → 256×129 | zimg | 443.048 | 706.428 | 0.627 / 0.633479 | 199,984 / — | Pass |
| Bicubic prepared narrow RGBA16 / zimg prepared † → 256×129 | zimg | 394.489 | 571.120 | 0.691 / 0.697543 | 137 / — | Pass |
| Lanczos3 one-axis one-shot RGBA16 / zimg one-shot † → 256×259 | zimg | 608.205 | 2,090.821 | 0.291 / 0.298338 | 44,720 / — | Pass |
| Lanczos3 one-axis one-shot RGBA16 / zimg one-shot † → 513×129 | zimg | 730.901 | 2,051.381 | 0.356 / 0.360264 | 254,680 / — | Pass |
| PNG RGBA16 one-shot stream decode/orient/crop/fit / FFmpeg one-shot † → 127×64 | FFmpeg | 2,787.424 | 3,298.544 | 0.845 / 0.857913 | 8,888 / — | Pass |
| Plain transpose RGBA16 † → 259×513 | FFmpeg | 33.567 | 44.207 | 0.759 / 0.779960 | 56 / — | Pass |
| Plain transpose RGBA8 † → 259×513 | FFmpeg | 27.234 | 37.911 | 0.718 / 0.726287 | 56 / — | Pass |
| Counterclockwise quarter turn RGBA8 † → 259×513 | FFmpeg | 31.256 | 40.931 | 0.764 / 0.777686 | 56 / — | Pass |
| Counterclockwise quarter turn RGBA16 † → 259×513 | FFmpeg | 38.195 | 48.231 | 0.792 / 0.815124 | 56 / — | Pass |
| Mirrored transpose RGBA8 † → 259×513 | FFmpeg | 33.179 | 42.417 | 0.782 / 0.795398 | 56 / — | Pass |
| Half turn RGBA8 † | FFmpeg | 8.037 | 9.993 | 0.804 / 0.838141 | 56 / — | Pass |
| Vertical flip RGBA8 † | FFmpeg | 8.172 | 8.482 | 0.963 / 0.972638 | 56 / — | Pass |
| Original clipped RGBA8 fill, 254×129 | FFmpeg adapter | 1.169 | 1.510 | 0.774 / 0.784836 | 0 / — | Pass |
| Original clipped RGBA16 fill, 254×129 | FFmpeg adapter | 2.013 | 4.670 | 0.431 / 0.435064 | 0 / — | Pass |
| Original clipped RGBA8 copy, 254×129 | FFmpeg adapter | 1.575 | 1.862 | 0.846 / 0.851870 | 0 / — | Pass |
| Original clipped RGBA8 composite / retained FFmpeg | FFmpeg | 73.279 | 491.958 | 0.149 / 0.160096 | 0 / — | Pass |
| Authored default-variation layout / ImageSharp public integration | ImageSharp public API | 3.501 | 19.487 | 0.180 / 0.182285 | 3,608 / 58,496 | Pass |
| Authored Unicode layout / corrected ImageSharp integration | Source-corrected ImageSharp public API | 2.364 | 112.445 | 0.021 / 0.021422 | 5,592 / 367,584 | Pass |
| Authored cmap4 layout / corrected ImageSharp integration | Source-corrected ImageSharp public API | 2.302 | 118.035 | 0.020 / 0.019870 | 5,088 / 385,016 | Pass |
| Authored compound layout / corrected ImageSharp integration | Source-corrected ImageSharp public API | 2.534 | 112.400 | 0.023 / 0.022958 | 6,152 / 366,960 | Pass |
| Authored RGBA8 layout + draw / corrected ImageSharp integration | Source-corrected ImageSharp public API | 36.343 | 223.800 | 0.162 / 0.273133 | 44,136 / 417,912 | Pass |
| Authored RGBA16 layout + draw / corrected ImageSharp integration | Source-corrected ImageSharp public API | 35.592 | 223.174 | 0.159 / 0.265136 | 44,136 / 417,912 | Pass |

## Inspection and pixel validation

| Operation | Reference | Lokad µs | Ref. µs | Ratio / upper | GC B/op Lokad / ref. | Status |
| --- | --- | ---: | ---: | ---: | ---: | --- |
| Probe PNG dimensions, seekable | FFmpeg adapter | 0.106 | 0.124 | 0.860 / 0.885324 | 568 / — | Pass |
| Probe JPEG dimensions, seekable | FFmpeg adapter | 0.184 | 0.194 | 0.951 / 0.953256 | 568 / — | Pass |
| Probe WebP dimensions, seekable | FFmpeg adapter | 0.109 | 0.121 | 0.895 / 0.921664 | 568 / — | Pass |
| Probe JPEG dimensions, async nonseekable | FFmpeg adapter | 0.193 | 0.206 | 0.933 / 0.936376 | 568 / — | Pass |
| Validate PNG8, span † | FFmpeg adapter | 1,153.141 | 1,377.335 | 0.837 / 0.840825 | 252 / — | Pass |
| Validate PNG16, span † | FFmpeg adapter | 2,677.717 | 2,964.183 | 0.903 / 0.908011 | 240 / — | Pass |
| Validate indexed Adam7 19×17, span † | FFmpeg adapter | 1.789 | 3.931 | 0.455 / 0.460934 | 792 / — | Pass |
| Validate JPEG baseline420, span † | FFmpeg adapter | 482.243 | 674.136 | 0.715 / 0.718821 | 10,016 / — | Pass |
| Validate JPEG progressive420, span † | FFmpeg adapter | 260.459 | 267.894 | 0.972 / 0.985447 | 4,088 / — | Pass |
| Validate VP8L+alpha, span † | FFmpeg adapter | 2,237.604 | 3,050.713 | 0.733 / 0.741790 | 297,090 / — | Pass |
| Validate VP8+alpha, span † | FFmpeg adapter | 4,222.234 | 5,447.337 | 0.775 / 0.778454 | 15,641 / — | Pass |
| Validate PNG16, async nonseekable † | FFmpeg adapter | 2,748.503 | 3,611.071 | 0.761 / 0.767620 | 360 / — | Pass |
| Inspect PNG8 complete headers, seekable, adapter reference | FFmpeg adapter | 0.306 | 0.575 | 0.533 / 0.558615 | 568 / — | Pass |
| Identify PNG8 complete container, resident, adapter reference | FFmpeg adapter | 6.736 | 13.058 | 0.516 / 0.517932 | 96 / — | Pass |
| Identify JPEG420, resident, canonical adapter | FFmpeg adapter | 0.503 | 1.179 | 0.427 / 0.438642 | 96 / — | Pass |
| Identify VP8L+alpha, resident, parser adapter | FFmpeg adapter | 0.022 | 5.002 | 0.004 / 0.004832 | 96 / — | Pass |
| Identify PNG8, fresh seekable Stream, adapter reference | FFmpeg adapter | 8.646 | 15.944 | 0.542 / 0.543166 | 192 / — | Pass |
| Identify PNG8, async nonseekable, adapter reference | FFmpeg adapter | 11.917 | 19.280 | 0.618 / 0.620655 | 216 / — | Pass |
| JPEG Adobe CMYK horizontal AC / FFmpeg fresh contexts † | FFmpeg | 376.538 | 748.053 | 0.503 / 0.507183 | 20,944 / — | Pass |
| Complete JPEG headers, fresh source / FFmpeg AVIO adapter | FFmpeg adapter | 1.980 | 2.819 | 0.702 / 0.715019 | 8,768 / — | Pass |
| Complete WebP headers, fresh seekable source / FFmpeg AVIO adapter | FFmpeg adapter | 0.133 | 0.293 | 0.453 / 0.466799 | 552 / — | Pass |
