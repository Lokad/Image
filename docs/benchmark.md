# Lokad.Image benchmarks

Updated 2026-11-02. These **121 selected comparisons** combine qualified
**0.2.1 development** costs and the retained **0.2.0 baseline**. The
[282-path inventory](../benchmarks/WARM-COVERAGE.md) preserves material scope;
the [nine text/drawing controls](../benchmarks/TEXT-DRAWING-CURRENT.md) now have
warm measurements. Four layout/fitting controls pass their scoped integration
comparisons; loading, rasterization and compositing retain gaps. Full parity is open.

**Boundaries matter.** Bicubic and historical Lanczos enlargement rows include
managed preparation but reuse native graphs/scratch. Current Lanczos reduction
rows compare equivalent one-shot or prepared operations on both sides.
[Prepared Lanczos3](../benchmarks/LANCZOS3-PREPARED-EXECUTION.md) retains 139,800 B
per managed plan and allocates 139 B/call; prepared CPU parity remains open.
Rows marked **§** retain pre-consolidation enlargement costs pending refresh.
PNG encode rows
create managed Deflate state per call while FFmpeg retains encoder/zlib context
and resets it per frame. These measurements remain valid for those boundaries;
they are not equivalent one-shot or prepared-session comparisons.
[Architectural attribution](../benchmarks/PREPARATION-WORKSPACE-ATTRIBUTION.md)
separates these costs and identifies WebP workspace allocations. Its additional
screens are diagnostic, not qualified gains or replacements for the tables below.

[Current WebP workspace](../benchmarks/VP8L-TRACE-FORWARD-WINDOW.md) uses bounded
trace state and independent selected candidates. Photograph/diagram/alpha allocate
**16.8 / 8.0 / 14.2 MB/op**, with admitted peaks **12.2 / 5.1 / 4.1 MB**, excluding
borrowed inputs/caller streams. No trace workspace is retained between encodes.
The latest storage stage has mixed CPU results, including a slower raw diagram;
normalized before/after confidence overlaps. Three current costs are refreshed,
without an incremental CPU-gain claim. Diagram parity confidence is open;
photograph/alpha retain gaps. Rows marked **‡** retain historical affected costs.

Measurements run sequentially on the dedicated quiet Ubuntu x64 VM, AMD EPYC 9V45,
one pinned CPU, warm .NET 10, tiering off and nonconcurrent workstation GC. Timed
operations include declared normalization, output ownership and release; fixture
setup, I/O, IPC, startup and checks are excluded. Native brackets and versioned
idle/process guards pass. Times are µs; ratios use the faster native bracket.
**Upper** includes conservative filtered/all-sample confidence. **B/op** is managed
allocation, not peak/retained capacity; native allocation is unmeasured. Four
canvas rows reuse independently verified allocation diagnostics, explicitly
labeled in their evidence. Short screens do not establish independent parity.

FFmpeg is the reference, with labeled libwebp exact=1, SIMD libjpeg-turbo grayscale
and zimg backend exceptions. Text rows compare COMPLETE PUBLIC-API INTEGRATION
using ImageSharp.Drawing 2.1.7/Fonts 2.1.3, including multiple layout calls and the
entire fitting search, not bare shaping-engine speed. Font loading uses
FreeType 2.13.2 with eager/lazy validation differences; RGBA16 compositing uses
ImageSharp 3.1.12 Rgba64 SrcOver and is not FFmpeg performance. Scoped visual/native
tolerances preserve exact managed contracts and full 16-bit samples. See
[text/drawing scope](../benchmarks/TEXT-DRAWING-CURRENT.md) and
[codec parity](codec-parity.md); equal numeric quality settings are not equivalence.

Current production passes 5,411 full/live checks per OS, 234 affected checks in
normal/scalar modes and the same actual Release-package consumer/three examples
on Windows/Linux. Unchanged paths reuse valid performance evidence. Current
family evidence is linked below; individual experiments remain in
[coverage/history](../benchmarks/WARM-COVERAGE.md), with raw evidence retained.

**Codecs**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Packet B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| WebP VP8 + alpha, fresh seekable Stream † | 7,037.26 | 6,042.62 | 1.165 / 1.211 | 1,434,827 | — |
| JPEG 4:4:4, forward-only / exact reference (confidence open) † | 3,733.67 | 3,774.82 | 0.989 / 1.009 | 820,150 | 133,039 / 133,041 |
| JPEG 4:2:0 from caller RGB samples † | 1,655.50 | 1,580.59 | 1.047 / 1.061 | 388,220 | 52,091 / 52,093 |
| JPEG RGBA16 4:2:0 / caller IMemoryBudget (confidence open) † | 1,876.82 | 1,835.59 | 1.022 / 1.038 | 819,488 | 52,115 / 52,117 |
| JPEG grayscale, standalone SIMD libjpeg-turbo † | 1,684.29 | 1,496.43 | 1.126 / 1.137 | 155,436 | 58,910 / 58,910 |
| JPEG from caller GRAY8, standalone SIMD libjpeg-turbo † | 1,218.17 | 1,136.37 | 1.072 / 1.090 | 155,439 | 58,910 / 58,910 |
| VP8 diagram, effort2 / distortion-only search † | 3,353.14 | 2,517.95 | 1.332 / 1.346 | 741,375 | 7,330 / 7,330 |
| VP8 RGBA16 + alpha, including normalization † ‡ | 71,441.15 | 54,514.34 | 1.311 / 1.365 | 21,350,352 | 98,590 / 98,590 |
| VP8 opaque RGBA8 † | 19,848.95 | 15,654.15 | 1.268 / 1.308 | 1,201,552 | 72,380 / 72,380 |
| VP8 opaque photograph † | 13,899.64 | 9,701.62 | 1.433 / 1.438 | 809,072 | 17,688 / 17,688 |
| VP8 opaque text/diagram † | 7,791.31 | 5,845.88 | 1.333 / 1.351 | 741,137 | 7,100 / 7,100 |
| VP8 diagram, effort6 / trellis † | 15,835.32 | 11,306.07 | 1.401 / 1.415 | 754,344 | 6,818 / 6,818 |
| VP8 opaque RGBA16, including normalization † | 20,568.52 | 15,639.99 | 1.315 / 1.319 | 1,201,552 | 72,380 / 72,380 |
| VP8 RGBA8 + lossless alpha † | 68,339.72 | 54,643.25 | 1.251 / 1.301 | 14,238,680 | 98,590 / 98,590 |
| VP8L photograph / libwebp exact=1 † | 103,705.27 | 86,982.67 | 1.192 / 1.231 | 16,835,524 | 160,032 / 160,032 |
| VP8L text/diagram / libwebp exact=1 (confidence open) † | 10,392.20 | 10,057.04 | 1.033 / 1.075 | 7,988,537 | 7,234 / 7,234 |
| VP8L RGBA8 / libwebp exact=1 † ‡ | 158,892.27 | 121,442.06 | 1.308 / 1.335 | 29,696,928 | 183,812 / 183,812 |
| VP8L opaque RGBA8 / libwebp exact=1 † ‡ | 99,191.77 | 71,204.07 | 1.393 / 1.447 | 19,992,640 | 121,896 / 121,896 |
| VP8L RGBA16, including normalization / libwebp exact=1 † ‡ | 166,167.06 | 122,510.98 | 1.356 / 1.385 | 29,696,928 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † ‡ | 156,836.69 | 121,322.88 | 1.293 / 1.316 | 29,697,312 | 183,812 / 183,812 |
| PNG RGBA8, SmallestSize9 / native9 † | 22,600.32 | 17,310.44 | 1.306 / 1.331 | 1,325,732 | 210,973 / 210,973 |
| PNG RGBA16, SmallestSize9 / native9 † | 26,874.29 | 20,094.40 | 1.337 / 1.347 | 3,813,732 | 963,202 / 963,202 |
| PNG RGBA8, Fastest1 / native1 † | 6,083.65 | 4,351.80 | 1.398 / 1.420 | 1,344,428 | 236,828 / 236,828 |
| PNG RGBA16, NoCompression0 / native0 † | 203.45 | 406.60 | 0.500 / 0.539 | 1,741,468 | 1,066,552 / 1,066,552 |
| PNG RGBA16, Fastest1 / native1 † | 23,284.98 | 17,477.77 | 1.332 / 1.341 | 3,813,732 | 968,216 / 968,216 |
| PNG RGBA16 Fastest1, live cancelable token † | 24,004.55 | 17,494.31 | 1.372 / 1.383 | 3,813,732 | 968,216 / 968,216 |
| PNG RGBA16 Fastest1, forward-only output † | 23,265.92 | 17,478.35 | 1.331 / 1.345 | 3,813,764 | 968,216 / 968,216 |
| PNG grayscale8, Optimal6 / native6 † | 2,570.87 | 1,940.17 | 1.325 / 1.354 | 1,001,812 | 72,586 / 72,586 |
| PNG RGB8, Optimal6 / native6 † | 12,457.28 | 9,513.16 | 1.309 / 1.324 | 1,325,068 | 168,729 / 168,729 |
| PNG gray-alpha8, Optimal6 / native6 † | 5,626.99 | 4,312.43 | 1.305 / 1.350 | 1,021,036 | 120,366 / 120,366 |
| PNG RGBA8, Optimal6 / native6 † | 24,157.64 | 17,999.79 | 1.342 / 1.373 | 1,325,732 | 210,973 / 210,973 |
| PNG RGBA16, Optimal6 / native6 † | 26,842.58 | 20,081.34 | 1.337 / 1.359 | 3,813,732 | 963,202 / 963,202 |
| PNG RGB16, Optimal6 / native6 † | 20,239.97 | 15,329.96 | 1.320 / 1.338 | 3,571,252 | 739,472 / 739,472 |
| PNG grayscale16, Optimal6 / native6, red-plane reference † | 6,812.24 | 5,434.49 | 1.254 / 1.285 | 1,506,972 | 265,433 / 265,433 |
| PNG gray-alpha16, Optimal6 / native6 † | 13,361.83 | 10,171.22 | 1.314 / 1.333 | 2,264,564 | 506,887 / 506,887 |
| PNG RGBA8, stored0 / native0 † | 253.82 | 189.65 | 1.338 / 1.471 | 1,203,724 | 533,444 / 533,444 |
| PNG RGBA8 async, fresh nonseekable output, Optimal6 / native6 † | 24,513.62 | 18,812.43 | 1.303 / 1.323 | 1,387,492 | 210,973 / 210,973 |
| PNG RGBA8 decode, live token † | 1,607.11 | 1,397.16 | 1.150 / 1.182 | 4,472 | — |
| PNG RGBA8 decode, tight working limit † | 1,642.61 | 1,498.48 | 1.096 / 1.119 | 68,224 | — |
| PNG RGBA16 decode, tight working limit (confidence open) † | 3,120.34 | 3,128.99 | 0.997 / 1.016 | 70,272 | — |
| PNG predictable RGBA16 3000², Optimal6 / mixed6 † | 352,397.34 | 407,457.02 | 0.865 / 0.870 | 3,402,572 | 573,041 / 573,041 |
| JPEG baseline420, resident decode † | 1,062.27 | 1,017.29 | 1.044 / 1.078 | 757,362 | — |
| JPEG DC-only, caller CMYK decode (confidence open) † | 335.68 | 336.82 | 0.997 / 1.032 | 1,085,963 | — |
| VP8L RGBA8, resident decode † | 3,224.25 | 3,019.22 | 1.068 / 1.079 | 152,728 | — |
| VP8L RGBA8, fresh seekable Stream decode † | 3,271.53 | 3,048.58 | 1.073 / 1.102 | 152,824 | — |
| JPEG CMYK sparse-2D, resident decode † | 607.81 | 446.62 | 1.361 / 1.429 | 1,085,757 | — |
| JPEG YCCK420, resident decode (confirmed pass) † | 436.05 | 728.08 | 0.599 / 0.626 | 895,546 | — |
| JPEG 4:1:1, resident decode † | 529.90 | 450.86 | 1.175 / 1.213 | 751,290 | — |
| JPEG 4:1:0, resident decode (confirmed pass) † | 372.23 | 509.10 | 0.731 / 0.756 | 720,838 | — |
| JPEG caller RGB24 async nonseekable, 420 (confidence open) † | 1,654.09 | 1,635.82 | 1.011 / 1.026 | 309,524 | 52,091 / 52,093 |
| JPEG RGBA8 async nonseekable, 444 (confirmed pass) † | 3,727.72 | 3,877.52 | 0.961 / 0.971 | 820,531 | 133,039 / 133,041 |
| PNG8 decode, two retained images / shared caller root † | 3,292.45 | 2,846.74 | 1.157 / 1.168 | 1,637,520 | — |
| JPEG progressive grayscale, resident decode † | 2,210.71 | 1,714.85 | 1.289 / 1.319 | 683,126 | — |
| JPEG progressive420, resident decode † | 2,152.83 | 1,979.17 | 1.088 / 1.137 | 775,318 | — |
| JPEG progressive422, resident decode † | 2,706.44 | 2,246.52 | 1.205 / 1.214 | 847,116 | — |
| JPEG ordinary progressive444, resident decode † | 4,404.74 | 3,370.40 | 1.307 / 1.335 | 984,212 | — |
| JPEG progressive444, live-token resident decode † | 4,925.83 | 3,408.60 | 1.445 / 1.515 | 984,212 | — |
| JPEG dense CMYK, resident decode (confirmed pass) † | 2,575.62 | 3,161.31 | 0.815 / 0.860 | 1,085,661 | — |
| JPEG horizontal-only CMYK, resident decode † | 518.51 | 396.74 | 1.307 / 1.353 | 1,085,763 | — |
| JPEG YCCK420, caller RGB24 decode (confirmed pass) † | 441.76 | 624.43 | 0.707 / 0.742 | 762,700 | — |

**Editing**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Bilinear resize RGBA8 † → 256×129 | 504.78 | 387.47 | 1.303 / 1.315 | 157,971 |
| Bilinear upscale RGBA8 † → 1027×519 | 2,914.62 | 2,163.33 | 1.347 / 1.358 | 2,235,512 |
| Bilinear resize RGBA16 † → 256×129 | 470.26 | 418.16 | 1.125 / 1.133 | 272,815 |
| Bicubic resize RGBA8 † → 256×129 | 579.25 | 418.45 | 1.384 / 1.400 | 332,188 |
| Bicubic upscale RGBA16 † → 1027×519 | 3,541.70 | 2,879.12 | 1.230 / 1.244 | 5,016,602 |
| Bicubic RGBA16, width down / height up / input+output+4 KiB † → 256×519 | 1,040.15 | 844.30 | 1.232 / 1.396 | 1,063,419 |
| Bicubic RGBA8, 32×16 / input+output+4 KiB → 32×16 | 464.68 | 375.02 | 1.239 / 1.370 | 2,185 |
| Bicubic RGBA8, 32×16 / default workspace → 32×16 | 342.09 | 379.01 | 0.903 / 0.927 | 199,369 |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 256×129 | 773.75 | 1,680.57 | 0.460 / 0.473 | 138,619 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 256×129 | 641.49 | 517.81 | 1.239 / 1.268 | 139 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 256×129 | 787.27 | 1,919.47 | 0.410 / 0.418 | 138,611 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 256×129 | 654.48 | 559.68 | 1.169 / 1.222 | 139 |
| Lanczos3 enlargement RGBA8 † § → 1027×519 | 4,198.93 | 4,066.37 | 1.033 / 1.038 | 319,038 |
| Lanczos3 enlargement RGBA16 † § → 1027×519 | 5,399.64 | 4,260.84 | 1.267 / 1.453 | 4,583,983 |
| Bicubic RGBA8 enlargement, input+output+4 KiB † → 1027×519 | 4,473.00 | 3,036.63 | 1.473 / 1.481 | 2,132,668 |
| Bicubic RGBA8 width enlargement, input+output+4 KiB † → 1027×259 | 1,644.06 | 1,524.52 | 1.078 / 1.094 | 1,064,526 |
| Bicubic RGBA8 height enlargement, input+output+4 KiB † → 513×519 | 2,165.54 | 1,748.88 | 1.238 / 1.256 | 1,065,523 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129 | 622.83 | 422.49 | 1.474 / 1.540 | 132,309 |
| Bicubic RGBA8 wider-support reduction † → 128×64 | 493.82 | 347.10 | 1.423 / 1.445 | 231,241 |
| Bicubic RGBA16, width up / height down, input+output+4 KiB † → 1027×129 | 1,112.92 | 850.82 | 1.308 / 1.369 | 1,060,371 |
| Bicubic RGBA8, width up / height down, input+output+4 KiB † → 1027×129 | 1,167.23 | 864.52 | 1.350 / 1.367 | 530,400 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 128×64 | 505.70 | 347.78 | 1.454 / 1.475 | 32,905 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 64×32 | 545.55 | 370.06 | 1.474 / 1.506 | 8,329 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 128×64 | 611.34 | 417.61 | 1.464 / 1.524 | 65,674 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 64×32 | 518.01 | 389.68 | 1.329 / 1.355 | 16,521 |
| Bicubic RGBA8, width down / height up, default workspace † → 256×519 | 912.31 | 841.39 | 1.084 / 1.107 | 731,768 |
| Bicubic RGBA16, width down / height up, default workspace † → 256×519 | 974.30 | 826.85 | 1.178 / 1.196 | 1,263,193 |
| Plain transpose RGBA16 † → 259×513 | 32.93 | 44.48 | 0.740 / 0.748 | 112 |
| Plain transpose RGBA8 † → 259×513 | 26.80 | 38.79 | 0.691 / 0.711 | 112 |
| Exact rectangle copy into borrowed RGBA8 canvas | 2.13 | 1.93 | 1.100 / 1.114 | 56 |
| Exact rectangle copy into borrowed RGBA16 canvas (confidence open) | 3.89 | 3.83 | 1.016 / 1.052 | 56 |
| Exact clipped fill into borrowed RGBA8 canvas (parity confirmed) | 1.19 | 1.57 | 0.759 / 0.775 | 56 |
| Exact clipped fill into borrowed RGBA16 canvas (parity confirmed) | 2.29 | 4.20 | 0.546 / 0.567 | 56 |
| Overlapping RGBA8 copy + reverse restoration, per pair (gap) | 4.18 | 3.78 | 1.106 / 1.152 | 112 |
| Font loading / FreeType 2.13.2 (gap) | 339.68 | 14.88 | 22.824 / 23.618 | 757,598 |
| Wrapped logical layout / SixLabors public API (pass) | 6.07 | 96.16 | 0.063 / 0.097 | 3,153 |
| Latin OpenType layout / SixLabors public API (pass) | 20.40 | 57.62 | 0.354 / 0.717 | 6,409 |
| Arabic OpenType layout / SixLabors public API (pass) | 88.93 | 337.84 | 0.263 / 0.421 | 28,658 |
| Complete fitting search / SixLabors public API (pass) | 1,144.00 | 1,882.11 | 0.608 / 0.866 | 224,181 |
| Prepared text RGBA8 / ImageSharp.Drawing (gap) | 926.31 | 51.59 | 17.954 / 23.296 | 83,456 |
| Prepared text RGBA16 / ImageSharp.Drawing (gap) | 871.93 | 52.12 | 16.729 / 19.666 | 83,456 |
| Composite RGBA8 / FFmpeg (gap) | 5,086.39 | 492.69 | 10.324 / 10.470 | 56 |
| Composite RGBA16 / ImageSharp 3.1.12 (gap) | 5,247.40 | 696.04 | 7.539 / 7.708 | 56 |

**Inspection and pixel-coding validation**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Probe PNG dimensions, seekable | 0.158 | 0.123 | 1.284 / 1.328 | 744 | Confirmed gap |
| Probe JPEG dimensions, seekable | 0.326 | 0.194 | 1.681 / 1.736 | 744 | Confirmed gap |
| Probe WebP dimensions, seekable | 0.165 | 0.125 | 1.323 / 1.425 | 744 | Confirmed gap |
| Probe JPEG dimensions, async nonseekable | 0.404 | 0.201 | 2.007 / 2.052 | 744 | Confirmed gap |
| Validate PNG8, span | 1,417.189 | 1,384.730 | 1.023 / 1.056 | 240 | Confidence open |
| Validate PNG16, span | 2,805.515 | 2,974.590 | 0.943 / 0.972 | 240 | Confirmed pass |
| Validate indexed Adam7 19×17, span | 1.776 | 3.965 | 0.448 / 0.460 | 792 | Confirmed pass |
| Validate JPEG baseline420, span | 543.780 | 689.481 | 0.789 / 0.815 | 9,984 | Confirmed pass |
| Validate JPEG progressive420, span | 283.293 | 268.802 | 1.054 / 1.080 | 4,072 | Confirmed gap |
| Validate VP8L+alpha, span | 3,397.141 | 3,049.808 | 1.114 / 1.134 | 685,421 | Confirmed gap |
| Validate VP8+alpha, span | 6,696.676 | 5,469.412 | 1.224 / 1.250 | 554,478 | Confirmed gap |
| Validate PNG16, async nonseekable | 2,836.823 | 3,623.647 | 0.783 / 0.790 | 8,544 | Confirmed pass |
| Inspect PNG8 complete headers, seekable, adapter reference | 0.703 | 0.579 | 1.215 / 1.235 | 8,528 | Confirmed gap |
| Identify PNG8 complete container, resident, adapter reference | 13.079 | 13.049 | 1.002 / 1.005 | 96 | Confirmed gap |
| Identify JPEG420, resident, canonical adapter | 0.501 | 1.193 | 0.420 / 0.426 | 96 | Confirmed pass |
| Identify VP8L+alpha, resident, parser adapter | 0.022 | 4.985 | 0.004 / 0.005 | 96 | Confirmed pass |
| Identify PNG8, fresh seekable Stream, adapter reference | 19.537 | 15.929 | 1.226 / 1.232 | 192 | Confirmed gap |
| Identify PNG8, async nonseekable, adapter reference | 20.312 | 19.321 | 1.051 / 1.060 | 8,400 | Confirmed gap |

† identifies qualified development-path costs. PNG encodes use the cached
four-invocation selector; predictable PNG16 native samples also batch four.
Every value is per complete operation. JPEG baseline decode, caller-CMYK, all
displayed PNG CRC callers, VP8/VP8L stream decode, the indicated JPEG/WebP anchors, bicubic word anchors
and Lanczos3 have twelve measured iterations. Retained
[RGBA16 bilinear](../benchmarks/WARM-BILINEAR-WORD-GEOMETRY.md) and
[RGBA8 bicubic reduction](../benchmarks/CURRENT-BICUBIC-BYTE-REDUCTION-COST.md)
also have independent twelves and proven unchanged executed paths.
Remaining short screens retain three and do not establish independent parity.

FFmpeg is the reference, with approved backend boundaries: lossless WebP encode
uses **libwebp exact=1** to preserve hidden RGB; grayscale JPEG uses explicitly
labeled SIMD **libjpeg-turbo**. Bicubic/Lanczos3 use **zimg**, FFmpeg's zscale
backend, including required packing, halos and alpha normalization. Existing
case-scoped native-only pixel bounds remain; managed pixels stay exact.
Equal numeric quality settings establish no quality equivalence. Encoded sizes
and fixed packet claims retain their declared input/settings scope.

Probe/header inspection, complete Identify and pixel-coding validation have
distinct boundaries. Header inspection skips PNG CRCs; Identify validates the
complete container and nine source facts without decoding pixels. Validation
checks coding syntax without retaining an RGBA result. Stream rows include
actual acquisition, copies, EOF and release as declared.

[CRC evidence](../benchmarks/warm-png-gfni-crc-evidence.json),
[JPEG traversal](../benchmarks/WARM-JPEG-BASELINE-DC-AC-FUSION.md),
[codec parity](codec-parity.md) and
[coverage/history](../benchmarks/WARM-COVERAGE.md) retain scope, producer identities,
remaining gaps and raw evidence. Update with `python benchmarks/render-benchmark.py`;
verify with `--check`.
