# Lokad.Image benchmarks

Updated 2026-11-04. These **124 selected comparisons** combine qualified
**0.2.1 development** costs and the retained **0.2.0 baseline**. The
[282-path inventory](../benchmarks/WARM-COVERAGE.md) preserves material scope;
the [nine text/drawing controls](../benchmarks/TEXT-DRAWING-CURRENT.md) now have
warm measurements. Four layout/fitting controls and RGBA16 compositing pass their
scoped comparisons; loading, rasterization and RGBA8 compositing retain gaps.
Full parity is open.

[Interval-ordered text preparation](../benchmarks/TEXT-EDGE-INTERVAL-PREPARATION.md)
saves **0.50 / 0.40 ms** on the two prepared drawing controls, **58.24% / 50.35% raw**
(55.95% / 54.54% reference-normalized), with separated confidence and exact pixels.
Both retain gaps against ImageSharp.Drawing. Allocation is 83,712 B/draw (+256 B);
admitted drawing workspace peaks at 48,544 B (previously 44,416), with zero retained
scratch. These measurements include preparation and cleanup, with borrowed
font/layout setup and per-operation canvas reset excluded equally.

**Boundaries matter.** Bicubic rows include managed preparation but reuse native
graphs/scratch. All eight current Lanczos rows compare equivalent one-shot or
prepared operations. [Paired row output](../benchmarks/LANCZOS-PAIRED-ROW-OUTPUT.md)
saves **598 µs / 12.50% raw / 12.43% normalized** on prepared word enlargement,
and **51 µs / 8.35% / 8.41%** on prepared word reduction, with separated independent
confidence and unchanged word allocations/retention. Five boundaries pass and
stop, including word one-shot enlargement. Prepared byte reduction and word
reduction/enlargement retain gaps (uppers **1.240 / 1.115 / 1.080**).
Plans retain **139,800 / 148,040 B** for byte/word reduction and **325,156 /
333,396 B** for enlargement. Admission capacity is unchanged; complete admitted/
physical peaks and native allocation/graph/input-plane retention are not newly
measured. [Earlier preparation](../benchmarks/LANCZOS-WORD-ROW-PREPARATION.md) and
[boundary reconciliation](../benchmarks/LANCZOS-CURRENT-ENLARGEMENT.md) retain history.
Rows marked **H** retain historical affected costs pending refresh.
All six current PNG Optimal/Fastest/Smallest rows compare fresh encoder state, complete
encoding and independent output/disposal on both sides. Word native endian-frame
allocation/conversion/disposal stays timed.
[Tree workspace reuse](../benchmarks/PNG-TREE-WORKSPACE.md) saves **834,544 B/op
(24.4%) word / 94,960 B/op (10.2%) byte** on SmallestSize9 at identical batching.
CPU intervals overlap; no incremental speedup, equivalence or causal regression
is claimed. Byte raw elapsed rises8.77% while its native clock rises8.46%.
Five current PNG controls have confirmed gaps: Smallest uppers **1.104 word /
1.061 byte**, word Optimal **1.064**, Fastest **1.124 / 1.282**. Byte Optimal
remains uncertain (**upper1.055**); resolve that measurement before tuning it.
Word Optimal/Fastest allocate **2,585,972 B/op**, Smallest **2,585,904 B/op**;
byte Optimal/Smallest **837,556 B/op**, Fastest **840,844 B/op**.
The operation owns **12,648 B** tree-array payload, released at disposal.
Workspace admission stays **384 KiB**, scratch retained after disposal zero;
complete admitted/physical peaks and native allocation are not newly measured.
[Earlier token output](../benchmarks/PNG-TOKEN-OUTPUT.md),
[Dictionary preparation](../benchmarks/PNG-SEQUENTIAL-DICTIONARY-HASH.md),
[greedy state](../benchmarks/PNG-FAST-LOCAL-EXECUTION-STATE.md),
[one-shot reconciliation](../benchmarks/PNG-SMALLEST-ONE-SHOT-CURRENT.md) and
[restored search stages](../benchmarks/PNG-SEARCH-EXECUTION-INCONCLUSIVE.md)
retain history. Other affected encoding costs stay historical (H).
[Prepared probes](../benchmarks/PNG-PREPARED-PROBES.md) retain earlier costs/history.
Other PNG encode rows create managed Deflate state per call while
FFmpeg retains its encoder/zlib context and resets it per frame. Those historical
measurements retain their labeled boundaries, not current-cost proof or equivalent
one-shot/session comparisons.
[Architectural attribution](../benchmarks/PREPARATION-WORKSPACE-ATTRIBUTION.md)
separates these costs and identifies WebP workspace allocations. Its additional
screens are diagnostic, not qualified gains or replacements for the tables below.

[VP8 source texture preparation](../benchmarks/VP8-SOURCE-TEXTURE-PREPARATION.md)
shares fixed source scores across luma candidates. The opaque photograph saves
**0.214 ms / 1.54% raw** (1.29% normalized), with separated confidence and unchanged
809,072 B/op. Five unchanged opaque VP8 costs retain valid evidence; the two transparent
controls below use the current workspace build. All retain gaps. Only the photograph has
an incremental speedup claim. The source-texture stage leaves allocation,
admitted peaks and retained scratch unchanged. Preparation stays timed.
The [shared magnitude grammar](../benchmarks/VP8-MAGNITUDE-GRAMMAR-INCONCLUSIVE.md)
investigation restores production after two stages show no useful complete-encode
lead; allocations and current costs stay unchanged. Two exhaustive production
statistics tests remain. Its adverse/flat screens are not causal regression proof.

[Color histogram construction](../benchmarks/WEBP-COLOR-HISTOGRAM-SEPARATION.md)
reduces complete photograph elapsed **11.32% / 7.65% reference-normalized**,
saving **10.75 ms**, with separated independent confidence and unchanged packets/
allocations. [Current reconciliation](../benchmarks/VP8L-INVARIANT-ALPHA-PREPARATION-INCONCLUSIVE.md)
retains the qualified implementation after an inconclusive alpha-preparation trial.
The current photograph global upper is **0.997**, opaque **1.037**; stricter paired
uppers are **1.026 / 1.028** respectively. **Strict parity remains open for both**;
the photograph's global pass alone does not close it. The table displays the global
estimator, distinct from the fastest-adjacent-sample paired check. Other unchanged
original/diagram/depth/async lossless controls pass and stop. The two transparent
controls reuse unchanged CTS197 evidence.

[Token-tail history reuse](../benchmarks/WEBP-TOKEN-TAIL-HISTORY.md) retains its
**475,928 B/op (5.52%)** alpha8 saving, now **8,146,632 B/op**. Its admitted peak
remains **3,656,309 B**, retained operation scratch zero. The histogram redesign
adds **32 B** to the admitted search workspace; measured complete peaks/allocations
are unchanged. Native allocation and physical peaks are unmeasured. Other affected
**H** costs remain historical. The current build passes **5,569 full/live checks
per OS** and affected normal/scalar checks. The same actual Release package and
all three examples pass both OSes, including retained resize admission and async
surviving outputs. Unchanged paths reuse valid proof; this is not a full release
refresh. [Earlier shared scoring](../benchmarks/WEBP-SCORING-PREPARATION.md)
retains its scoped history. Allocation, admission and retention are separate.

[Shared resize output ownership](../benchmarks/RESIZE-OVERWRITE-OUTPUT-STORAGE.md)
removes redundant output initialization using the existing complete-overwrite
allocator. Byte enlargement saves **0.385 ms / 10.08% raw / 8.94% normalized**, with
separated confidence; allocations fall **2.885 → 0.752 MB/op**. Word enlargement
saves 0.048 ms with separated raw/normalized confidence. Four existing costs are
refreshed; three retain gaps and wide reduction still passes. Its adverse raw
9.57% point accompanies a 7.39% slower native clock; normalized intervals overlap.
Unbudgeted byte enlargement retains one 2,132,052 B idle output array within the
existing shared 4 MiB ceiling. Governed calls never use that pool; admitted peaks
are unchanged and ownership stays explicit. Allocation, live capacity and idle
retention are separate. Other affected bilinear/bicubic rows marked **H** remain
historical. The [shared cached driver](../benchmarks/BICUBIC-SHARED-CACHED-DRIVER.md)
retains its geometry, scheduling, specialized kernels and exact repair.

[Shared compositing rows](../benchmarks/COMPOSITING-SHARED-ROWS.md) remove repeated
per-channel preparation across all depth pairs. Both measured controls now take
about 0.63 ms, saving 4.46 / 4.62 ms (87.7% / 88.0%) with exact rational samples
and unchanged 56 B/op. RGBA16 passes against labeled ImageSharp 3.1.12; RGBA8
retains a 1.254× FFmpeg gap. Mixed-depth/overlap correctness passes; their CPU cost
is unmeasured. No compositing workspace is allocated or retained.

Measurements run sequentially on the dedicated quiet Ubuntu x64 VM, AMD EPYC 9V45,
one pinned CPU, warm .NET 10, tiering off and nonconcurrent workstation GC. Timed
operations include declared normalization, output ownership and release; fixture
setup, I/O, IPC, startup and checks are excluded. Native brackets and versioned
idle/process guards pass. Times are µs; ratios use the faster native bracket.
**Upper** includes conservative filtered/all-sample confidence. **B/op** is managed
allocation, not peak/retained capacity; native allocation is unmeasured. Canvas
rows use separate verified allocation diagnostics, including current-core
compositing and drawing observations. Short screens do not establish independent parity.

FFmpeg is the reference, with labeled libwebp exact=1, SIMD libjpeg-turbo grayscale
and zimg backend exceptions. Text rows compare COMPLETE PUBLIC-API INTEGRATION
using ImageSharp.Drawing 2.1.7/Fonts 2.1.3, including multiple layout calls and the
entire fitting search, not bare shaping-engine speed. Font loading uses
FreeType 2.13.2 with eager/lazy validation differences; RGBA16 compositing uses
ImageSharp 3.1.12 Rgba64 SrcOver and is not FFmpeg performance. Scoped visual/native
tolerances preserve exact managed contracts and full 16-bit samples. See
[text/drawing scope](../benchmarks/TEXT-DRAWING-CURRENT.md) and
[codec parity](codec-parity.md); equal numeric quality settings are not equivalence.

Unchanged resize paths reuse their normal/scalar and package qualification;
current build checks are stated above. Family evidence is linked below;
individual experiments remain in
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
| VP8 RGBA16 + alpha, including normalization † | 67,047.81 | 53,839.15 | 1.245 / 1.274 | 8,146,224 | 98,590 / 98,590 |
| VP8 opaque RGBA8 † | 21,160.69 | 16,885.52 | 1.253 / 1.265 | 1,201,552 | 72,380 / 72,380 |
| VP8 opaque photograph † | 13,670.07 | 9,706.88 | 1.408 / 1.418 | 809,072 | 17,688 / 17,688 |
| VP8 opaque text/diagram † | 8,091.60 | 6,264.06 | 1.292 / 1.319 | 741,137 | 7,100 / 7,100 |
| VP8 diagram, effort6 / trellis † | 13,518.03 | 9,816.75 | 1.377 / 1.394 | 754,344 | 6,818 / 6,818 |
| VP8 opaque RGBA16, including normalization † | 20,367.41 | 15,593.58 | 1.306 / 1.331 | 1,201,552 | 72,380 / 72,380 |
| VP8 RGBA8 + lossless alpha † | 71,184.35 | 58,346.34 | 1.220 / 1.230 | 8,146,632 | 98,590 / 98,590 |
| VP8L photograph / libwebp exact=1 † | 82,037.88 | 83,327.52 | 0.985 / 0.997 | 6,517,254 | 160,032 / 160,032 |
| VP8L text/diagram / libwebp exact=1 † | 8,036.29 | 9,483.21 | 0.847 / 0.867 | 4,363,930 | 7,234 / 7,234 |
| VP8L RGBA8 / libwebp exact=1 † | 115,497.65 | 130,268.92 | 0.887 / 0.895 | 7,476,640 | 183,812 / 183,812 |
| VP8L opaque RGBA8 / libwebp exact=1 † | 70,681.22 | 70,344.26 | 1.005 / 1.037 | 5,983,424 | 121,896 / 121,896 |
| PNG RGBA8, one-shot SmallestSize9 / native9 † | 19,603.10 | 18,690.15 | 1.049 / 1.061 | 837,556 | 210,973 / 210,973 |
| PNG RGBA16, one-shot SmallestSize9 / native9 † | 22,770.12 | 21,242.05 | 1.072 / 1.104 | 2,585,904 | 963,202 / 963,202 |
| PNG RGBA8, one-shot Fastest1 / native1 † | 5,956.95 | 4,745.88 | 1.255 / 1.282 | 840,844 | 236,828 / 236,828 |
| PNG RGBA16, NoCompression0 / native0 † H | 203.45 | 406.60 | 0.500 / 0.539 | 1,741,468 | 1,066,552 / 1,066,552 |
| PNG RGBA16, one-shot Fastest1 / native1 † | 22,154.97 | 20,225.47 | 1.095 / 1.124 | 2,585,972 | 968,216 / 968,216 |
| PNG RGBA16 Fastest1, live cancelable token † H | 24,004.55 | 17,494.31 | 1.372 / 1.383 | 3,813,732 | 968,216 / 968,216 |
| PNG RGBA16 Fastest1, forward-only output † H | 23,265.92 | 17,478.35 | 1.331 / 1.345 | 3,813,764 | 968,216 / 968,216 |
| PNG grayscale8, Optimal6 / native6 † H | 2,570.87 | 1,940.17 | 1.325 / 1.354 | 1,001,812 | 72,586 / 72,586 |
| PNG RGB8, Optimal6 / native6 † H | 12,457.28 | 9,513.16 | 1.309 / 1.324 | 1,325,068 | 168,729 / 168,729 |
| PNG gray-alpha8, Optimal6 / native6 † H | 5,626.99 | 4,312.43 | 1.305 / 1.350 | 1,021,036 | 120,366 / 120,366 |
| PNG RGBA8, one-shot Optimal6 / native6 † | 17,820.47 | 17,246.94 | 1.033 / 1.055 | 837,556 | 210,973 / 210,973 |
| PNG RGBA16, one-shot Optimal6 / native6 † | 21,775.28 | 20,741.61 | 1.050 / 1.064 | 2,585,972 | 963,202 / 963,202 |
| PNG RGB16, Optimal6 / native6 † H | 20,239.97 | 15,329.96 | 1.320 / 1.338 | 3,571,252 | 739,472 / 739,472 |
| PNG grayscale16, Optimal6 / native6, red-plane reference † H | 6,812.24 | 5,434.49 | 1.254 / 1.285 | 1,506,972 | 265,433 / 265,433 |
| PNG gray-alpha16, Optimal6 / native6 † H | 13,361.83 | 10,171.22 | 1.314 / 1.333 | 2,264,564 | 506,887 / 506,887 |
| PNG RGBA8, stored0 / native0 † H | 253.82 | 189.65 | 1.338 / 1.471 | 1,203,724 | 533,444 / 533,444 |
| PNG RGBA8 async, fresh nonseekable output, Optimal6 / native6 † H | 24,513.62 | 18,812.43 | 1.303 / 1.323 | 1,387,492 | 210,973 / 210,973 |
| PNG RGBA8 decode, live token † | 1,607.11 | 1,397.16 | 1.150 / 1.182 | 4,472 | — |
| PNG RGBA8 decode, tight working limit † | 1,642.61 | 1,498.48 | 1.096 / 1.119 | 68,224 | — |
| PNG RGBA16 decode, tight working limit (confidence open) † | 3,120.34 | 3,128.99 | 0.997 / 1.016 | 70,272 | — |
| PNG predictable RGBA16 3000², Optimal6 / mixed6 † H | 352,397.34 | 407,457.02 | 0.865 / 0.870 | 3,402,572 | 573,041 / 573,041 |
| JPEG baseline420, resident decode † | 1,062.27 | 1,017.29 | 1.044 / 1.078 | 757,362 | — |
| JPEG DC-only, caller CMYK decode (confidence open) † | 335.68 | 336.82 | 0.997 / 1.032 | 1,085,963 | — |
| VP8L RGBA8, resident decode † | 3,224.25 | 3,019.22 | 1.068 / 1.079 | 152,728 | — |
| VP8L RGBA8, fresh seekable Stream decode † | 3,271.53 | 3,048.58 | 1.073 / 1.102 | 152,824 | — |
| JPEG CMYK sparse-2D, resident decode † | 607.81 | 446.62 | 1.361 / 1.429 | 1,085,757 | — |
| JPEG YCCK420, resident decode (confirmed pass) † | 436.05 | 728.08 | 0.599 / 0.626 | 895,546 | — |
| JPEG 4:1:1, resident decode † | 529.90 | 450.86 | 1.175 / 1.213 | 751,290 | — |
| JPEG 4:1:0, resident decode (confirmed pass) † | 372.23 | 509.10 | 0.731 / 0.756 | 720,838 | — |
| VP8L RGBA16, including normalization / libwebp exact=1 † | 108,444.64 | 120,791.93 | 0.898 / 0.911 | 7,476,640 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 106,786.87 | 120,901.44 | 0.883 / 0.896 | 7,477,024 | 183,812 / 183,812 |
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
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 256×129 | 725.70 | 1,575.62 | 0.461 / 0.463 | 138,643 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 256×129 | 603.45 | 488.73 | 1.235 / 1.240 | 139 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 256×129 | 718.06 | 1,819.13 | 0.395 / 0.397 | 146,869 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 256×129 | 563.58 | 511.13 | 1.103 / 1.115 | 139 |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 1027×519 | 3,939.25 | 5,246.65 | 0.751 / 0.755 | 319,224 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 1027×519 | 3,666.99 | 4,054.17 | 0.904 / 0.907 | 148 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 1027×519 | 4,519.20 | 5,067.93 | 0.892 / 0.896 | 4,592,777 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 1027×519 | 4,184.41 | 3,897.97 | 1.073 / 1.080 | 4,265,302 |
| Bilinear resize RGBA8 † H → 256×129 | 504.78 | 387.47 | 1.303 / 1.315 | 157,971 |
| Bilinear upscale RGBA8 † H → 1027×519 | 2,914.62 | 2,163.33 | 1.347 / 1.358 | 2,235,512 |
| Bilinear resize RGBA16 † H → 256×129 | 470.26 | 418.16 | 1.125 / 1.133 | 272,815 |
| Bicubic resize RGBA8 † → 256×129 | 549.84 | 418.61 | 1.313 / 1.321 | 199,981 |
| Bicubic upscale RGBA16 † → 1027×519 | 3,336.56 | 2,872.72 | 1.161 / 1.167 | 5,016,602 |
| Bicubic upscale RGBA8, default workspace † → 1027×519 | 3,434.17 | 3,198.46 | 1.074 / 1.082 | 751,975 |
| Bicubic RGBA16, width down / height up / input+output+4 KiB † H → 256×519 | 1,040.15 | 844.30 | 1.232 / 1.396 | 1,063,419 |
| Bicubic RGBA8, 32×16 / input+output+4 KiB H → 32×16 | 464.68 | 375.02 | 1.239 / 1.370 | 2,185 |
| Bicubic RGBA8, 32×16 / default workspace † → 32×16 | 384.00 | 403.93 | 0.951 / 0.962 | 197,298 |
| Bicubic RGBA8 enlargement, input+output+4 KiB † H → 1027×519 | 4,473.00 | 3,036.63 | 1.473 / 1.481 | 2,132,668 |
| Bicubic RGBA8 width enlargement, input+output+4 KiB † H → 1027×259 | 1,644.06 | 1,524.52 | 1.078 / 1.094 | 1,064,526 |
| Bicubic RGBA8 height enlargement, input+output+4 KiB † H → 513×519 | 2,165.54 | 1,748.88 | 1.238 / 1.256 | 1,065,523 |
| Bicubic RGBA8 reduction, input+output+4 KiB † H → 256×129 | 622.83 | 422.49 | 1.474 / 1.540 | 132,309 |
| Bicubic RGBA8 wider-support reduction † H → 128×64 | 493.82 | 347.10 | 1.423 / 1.445 | 231,241 |
| Bicubic RGBA16, width up / height down, input+output+4 KiB † H → 1027×129 | 1,112.92 | 850.82 | 1.308 / 1.369 | 1,060,371 |
| Bicubic RGBA8, width up / height down, input+output+4 KiB † H → 1027×129 | 1,167.23 | 864.52 | 1.350 / 1.367 | 530,400 |
| Bicubic RGBA8 reduction, input+output+4 KiB † H → 128×64 | 505.70 | 347.78 | 1.454 / 1.475 | 32,905 |
| Bicubic RGBA8 reduction, input+output+4 KiB † H → 64×32 | 545.55 | 370.06 | 1.474 / 1.506 | 8,329 |
| Bicubic RGBA16 reduction, input+output+4 KiB † H → 128×64 | 611.34 | 417.61 | 1.464 / 1.524 | 65,674 |
| Bicubic RGBA16 reduction, input+output+4 KiB † H → 64×32 | 518.01 | 389.68 | 1.329 / 1.355 | 16,521 |
| Bicubic RGBA8, width down / height up, default workspace † H → 256×519 | 912.31 | 841.39 | 1.084 / 1.107 | 731,768 |
| Bicubic RGBA16, width down / height up, default workspace † H → 256×519 | 974.30 | 826.85 | 1.178 / 1.196 | 1,263,193 |
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
| Prepared text RGBA8 / ImageSharp.Drawing (gap) | 360.59 | 45.27 | 7.966 / 9.721 | 83,712 |
| Prepared text RGBA16 / ImageSharp.Drawing (gap) | 392.22 | 50.08 | 7.832 / 10.030 | 83,712 |
| Composite RGBA8 / FFmpeg (gap) | 625.41 | 498.73 | 1.254 / 1.270 | 56 |
| Composite RGBA16 / ImageSharp 3.1.12 (pass) | 627.24 | 696.25 | 0.901 / 0.922 | 56 |

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
[current bicubic controls](../benchmarks/BICUBIC-SHARED-CACHED-DRIVER.md)
also have independent twelves. H marks historical costs after the documented
storage/execution changes; these are not current performance proof.
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
