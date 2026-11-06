# Lokad.Image benchmarks

Updated 2026-11-06. These **125 selected comparisons** combine qualified
**0.2.1 development** costs with retained **0.2.0** evidence. Full parity remains
open across the [282 required paths](../benchmarks/WARM-COVERAGE.md). **†** marks
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

- **WebP:** [complete prediction preparation](../benchmarks/VP8-COMPLETE-PREDICTION-PREPARATION.md)
  shares immutable borders/DC/interpolation across ten ordered candidates.
  Photograph independently saves682µs/6.23%, raw/normalized confidence separated,
  exact packets and809,225B/op unchanged. Seven affected costs refresh;
  distortion-only reuses unchanged code. Eight controls:2pass/stop,4gaps,2uncertain.
  Transparent word's adverse994µs point stays inconclusive against a contemporary
  original control; no favorable repeat. Standalone opaque8/diagram allocations
  differ+117/+85B/op; no new heap capacity. [Rate/score preparation](../benchmarks/VP8-CANDIDATE-SCORE-PUBLICATION.md)
  retains prior independent gains and adverse/uncertain history.
  [Prepared token state](../benchmarks/VP8-PREPARED-TOKEN-STATE.md) independently
  saves440/304µs (6.76%/4.43%) on complete validation/stream decoding, unchanged
  paired allocation. [Unshifted live reservoir](../benchmarks/VP8-UNSHIFTED-LIVE-RESERVOIR.md)
  avoids repeated code-value shifts and independently saves136µs/2.25%raw on complete
  live opaque decoding; normalized confidence overlaps,760,564B/op unchanged. All six
  reader controls remain gaps; five unchanged paths reuse evidence. Token order,
  every per-bit cancellation check and the published cursor remain exact. Earlier/adverse stages
  stay linked history.
  Six lossless controls reuse [unchanged green-predictor evidence](../benchmarks/WEBP-ISOLATED-GREEN-PREDICTION.md):
  four pass/stop; opaque/photo remain uncertain, uppers1.008/1.022.
- **PNG:** [lazy view preparation](../benchmarks/PNG-LAZY-VIEW-PREPARATION.md)
  independently saves390/375µs (1.83%/2.16%) on complete Optimal16/Smallest8.
  Eleven affected fresh one-shot comparisons refresh; four Fastest costs reuse
  unchanged bodies. Fifteen controls:9pass/stop,2gaps,4uncertain. Exact packets and
  384KiB admission remain unchanged; retained scratch is zero. Grayscale8 measures
  +408B/op (551,772); ten other fresh allocations are unchanged. Earlier
  [greedy preparation](../benchmarks/PNG-GREEDY-VIEW-PREPARATION.md)
  retains independent269/836µs gains; output/preparation histories stay linked.
- **Resizing:** [Lanczos](../benchmarks/LANCZOS-PAIRED-CERTIFICATE-PREPARATION.md)
  has five passing cases and three prepared gaps (uppers1.158/1.097/1.052).
  Prepared plans retain139,800–333,396B; one-shot setup stays timed on both sides.
  [Shared streaming](../benchmarks/BICUBIC-SHARED-STREAMING.md) passes six of eight;
  [small](../benchmarks/BICUBIC-WORD-COLUMN-PREPARATION.md) and
  [wide columns](../benchmarks/BICUBIC-WIDE-PREPARATION.md) retain named gaps.
  [Shared enlargement](../benchmarks/BICUBIC-SHARED-ENLARGEMENT.md) uses one geometry/
  filter source and two schedules across depths, preserving typed publication/repair.
  Complete default word enlargement independently saves179µs/4.97% and752,064B/op;
  its4,264,538B/op includes the mandatory4,264,104Boutput. Twelve existing full/axial/
  default/tight costs refresh:8gaps/4uncertain. Byte incremental timing is inconclusive;
  adverse points/native clock movement stay explicit. No new retained cache or pool.
  [Bilinear boundary refresh](../benchmarks/WARM-COVERAGE.md) measures complete byte
  enlargement2,357µs/102,891B per operation, ratio1.093/upper1.134 but uncertain.
  Its older751µs gap is superseded; keep current uncertainty before further tuning.
  Exact Q30/rational/certified pixels remain required versus native float.
- **JPEG and reading:** [correction-bit selection](../benchmarks/JPEG-CORRECTION-BIT.md)
  independently saves108µs/2.53%raw and2.07%normalized on complete live progressive444,
  with exact pixels and701,182B/op unchanged. Ratio1.140/upper1.151 remains a gap.
  Five unchanged controls reuse evidence: six total,1pass/3gaps/2uncertain.
  [Plane scheduling](../benchmarks/JPEG-PROGRESSIVE-PLANE-SCHEDULING.md) retains
  ordinary/live444 allocation984KB→701KB/op and its stated peak/retention boundaries.
  [Signed preparation](../benchmarks/JPEG-SIGNED-PREPARED-INSTRUCTIONS.md)
  retains prior gains. Other JPEG paths, including [grayscale encoding](../benchmarks/JPEG-GRAYSCALE-ROW-BATCH.md)
  against labeled SIMD libjpeg-turbo, reuse unchanged evidence.
- **Text/drawing:** [active edges](../benchmarks/TEXT-ACTIVE-EDGE-SCHEDULING.md)
  preserve exact frames,92,384B/draw and48,544B admitted Latin workspace;
  drawing remains a gap. [Layout/fitting and RGBA16 composition](../benchmarks/TEXT-DRAWING-CURRENT.md)
  pass/stop. These independent cases do not block codec or resize work.

The current production build passes **5,742 full/live checks per OS**, affected
normal/scalar checks, and the **same actual Release package/all three examples
on Windows/Linux**, including exact mixed-alpha text at both depths, alpha and
opaque-word, progressive-strip, wide-column, shared byte/word enlargement and live VP8
sync/async consumers. Normal/scalar and affected AVX512-disabled checks pass;
unchanged vector-path AVX2-disabled proof is reused.
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
| WebP VP8 + alpha, fresh seekable Stream † | 6,600.44 | 5,986.72 | 1.103 / 1.115 | 1,434,827 | — |
| JPEG 4:4:4, forward-only / exact reference (confidence open) † | 3,733.67 | 3,774.82 | 0.989 / 1.009 | 820,150 | 133,039 / 133,041 |
| JPEG 4:2:0 from caller RGB samples † | 1,655.50 | 1,580.59 | 1.047 / 1.061 | 388,220 | 52,091 / 52,093 |
| JPEG RGBA16 4:2:0 / caller IMemoryBudget (confidence open) † | 1,876.82 | 1,835.59 | 1.022 / 1.038 | 819,488 | 52,115 / 52,117 |
| JPEG grayscale, standalone SIMD libjpeg-turbo † | 1,660.52 | 1,522.24 | 1.091 / 1.103 | 155,436 | 58,910 / 58,910 |
| JPEG from caller GRAY8, standalone SIMD libjpeg-turbo † | 1,218.17 | 1,136.37 | 1.072 / 1.090 | 155,439 | 58,910 / 58,910 |
| VP8 diagram, effort2 / distortion-only search † | 2,987.26 | 2,503.72 | 1.193 / 1.197 | 741,386 | 7,330 / 7,330 |
| VP8 RGBA16 + alpha, including normalization † | 57,673.00 | 53,478.66 | 1.078 / 1.103 | 7,033,984 | 98,590 / 98,590 |
| VP8 opaque RGBA8 † | 14,427.73 | 15,489.57 | 0.931 / 0.938 | 1,201,669 | 72,380 / 72,380 |
| VP8 opaque photograph † | 10,269.81 | 9,692.49 | 1.060 / 1.068 | 809,225 | 17,688 / 17,688 |
| VP8 opaque text/diagram † | 6,118.83 | 5,820.81 | 1.051 / 1.066 | 741,222 | 7,100 / 7,100 |
| VP8 diagram, effort6 / trellis † | 10,708.53 | 9,838.60 | 1.088 / 1.120 | 754,497 | 6,818 / 6,818 |
| VP8 opaque RGBA16, including normalization † | 14,502.89 | 15,678.51 | 0.925 / 0.945 | 1,201,552 | 72,380 / 72,380 |
| VP8 RGBA8 + lossless alpha † | 56,148.06 | 54,903.24 | 1.023 / 1.053 | 7,034,392 | 98,590 / 98,590 |
| VP8L photograph / libwebp exact=1 † | 81,361.78 | 83,323.50 | 0.976 / 1.022 | 6,517,158 | 160,032 / 160,032 |
| VP8L text/diagram / libwebp exact=1 † | 7,550.15 | 9,576.69 | 0.788 / 0.823 | 4,363,930 | 7,234 / 7,234 |
| VP8L RGBA8 / libwebp exact=1 † | 109,807.33 | 127,005.55 | 0.865 / 0.940 | 7,476,544 | 183,812 / 183,812 |
| VP8L opaque RGBA8 / libwebp exact=1 † | 75,840.35 | 76,643.85 | 0.990 / 1.008 | 5,983,328 | 121,896 / 121,896 |
| PNG RGBA8, one-shot SmallestSize9 / native9 † | 16,972.34 | 17,224.29 | 0.985 / 0.993 | 820,452 | 210,973 / 210,973 |
| PNG RGBA16, one-shot SmallestSize9 / native9 † | 20,800.66 | 20,840.32 | 0.998 / 1.004 | 2,437,660 | 963,202 / 963,202 |
| PNG RGBA8, one-shot Fastest1 / native1 † | 4,543.94 | 4,356.87 | 1.043 / 1.059 | 820,916 | 236,828 / 236,828 |
| PNG RGBA16, NoCompression0 / native0 † H | 203.45 | 406.60 | 0.500 / 0.539 | 1,741,468 | 1,066,552 / 1,066,552 |
| PNG grayscale8, one-shot Optimal6 / native6 † | 2,053.51 | 1,974.20 | 1.040 / 1.075 | 551,772 | 72,586 / 72,586 |
| PNG RGB8, one-shot Optimal6 / native6 † | 9,471.66 | 9,769.77 | 0.969 / 0.984 | 819,884 | 168,729 / 168,729 |
| PNG gray-alpha8, one-shot Optimal6 / native6 † | 4,276.85 | 4,484.44 | 0.954 / 0.971 | 552,332 | 120,366 / 120,366 |
| PNG RGBA8, one-shot Optimal6 / native6 † | 16,932.50 | 17,259.94 | 0.981 / 0.987 | 820,452 | 210,973 / 210,973 |
| PNG RGBA16, one-shot Optimal6 / native6 † | 20,534.92 | 20,745.47 | 0.990 / 0.995 | 2,437,660 | 963,202 / 963,202 |
| PNG RGB16, one-shot Optimal6 / native6 † | 15,576.65 | 16,093.65 | 0.968 / 0.975 | 2,431,492 | 739,472 / 739,472 |
| PNG grayscale16, one-shot Optimal6 / native6, red-plane reference † | 4,773.10 | 5,394.24 | 0.885 / 0.902 | 822,740 | 265,433 / 265,433 |
| PNG gray-alpha16, one-shot Optimal6 / native6 † | 10,050.55 | 10,479.84 | 0.959 / 0.967 | 1,360,900 | 506,887 / 506,887 |
| PNG RGBA8, stored0 / native0 † H | 253.82 | 189.65 | 1.338 / 1.471 | 1,203,724 | 533,444 / 533,444 |
| PNG RGBA8 decode, live token † | 1,607.11 | 1,397.16 | 1.150 / 1.182 | 4,472 | — |
| PNG RGBA8 decode, tight working limit † | 1,642.61 | 1,498.48 | 1.096 / 1.119 | 68,224 | — |
| PNG RGBA16 decode, tight working limit (confidence open) † | 3,120.34 | 3,128.99 | 0.997 / 1.016 | 70,272 | — |
| PNG predictable RGBA16 3000², Optimal6 / mixed6 † H | 352,397.34 | 407,457.02 | 0.865 / 0.870 | 3,402,572 | 573,041 / 573,041 |
| PNG RGBA16, one-shot Fastest1 / native1 † | 18,449.19 | 18,610.40 | 0.991 / 1.019 | 2,437,660 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, live cancelable token † | 18,727.28 | 18,564.11 | 1.009 / 1.016 | 2,437,660 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, forward-only output † | 18,557.49 | 18,594.14 | 0.998 / 1.004 | 2,437,692 | 968,216 / 968,216 |
| PNG RGBA8 async, one-shot Optimal6, nonseekable output † | 16,996.93 | 17,633.09 | 0.964 / 0.968 | 882,212 | 210,973 / 210,973 |
| JPEG baseline420, resident decode † | 1,062.27 | 1,017.29 | 1.044 / 1.078 | 757,362 | — |
| JPEG DC-only, caller CMYK decode (confidence open) † | 335.68 | 336.82 | 0.997 / 1.032 | 1,085,963 | — |
| VP8L RGBA8, resident decode † | 3,224.25 | 3,019.22 | 1.068 / 1.079 | 152,728 | — |
| VP8L RGBA8, fresh seekable Stream decode † | 3,271.53 | 3,048.58 | 1.073 / 1.102 | 152,824 | — |
| JPEG CMYK sparse-2D, resident decode † | 607.81 | 446.62 | 1.361 / 1.429 | 1,085,757 | — |
| JPEG YCCK420, resident decode (confirmed pass) † | 436.05 | 728.08 | 0.599 / 0.626 | 895,546 | — |
| JPEG 4:1:1, resident decode † | 529.90 | 450.86 | 1.175 / 1.213 | 751,290 | — |
| JPEG 4:1:0, resident decode (confirmed pass) † | 372.23 | 509.10 | 0.731 / 0.756 | 720,838 | — |
| VP8L RGBA16, including normalization / libwebp exact=1 † | 116,271.20 | 123,756.44 | 0.940 / 0.977 | 7,476,544 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 108,317.01 | 123,053.03 | 0.880 / 0.953 | 7,476,928 | 183,812 / 183,812 |
| JPEG caller RGB24 async nonseekable, 420 (confidence open) † | 1,654.09 | 1,635.82 | 1.011 / 1.026 | 309,524 | 52,091 / 52,093 |
| JPEG RGBA8 async nonseekable, 444 (confirmed pass) † | 3,727.72 | 3,877.52 | 0.961 / 0.971 | 820,531 | 133,039 / 133,041 |
| PNG8 decode, two retained images / shared caller root † | 3,292.45 | 2,846.74 | 1.157 / 1.168 | 1,637,520 | — |
| JPEG progressive grayscale, resident decode † | 2,023.81 | 1,829.96 | 1.106 / 1.111 | 683,136 | — |
| JPEG progressive420, resident decode † | 1,831.21 | 2,088.88 | 0.877 / 0.882 | 775,314 | — |
| JPEG progressive422, resident decode † | 2,292.77 | 2,245.05 | 1.021 / 1.074 | 847,116 | — |
| JPEG ordinary progressive444, resident decode † | 3,657.90 | 3,353.21 | 1.091 / 1.102 | 701,233 | — |
| JPEG progressive444, live-token resident decode † | 4,180.69 | 3,666.00 | 1.140 / 1.151 | 701,182 | — |
| JPEG dense CMYK, resident decode (confirmed pass) † | 2,575.62 | 3,161.31 | 0.815 / 0.860 | 1,085,661 | — |
| JPEG horizontal-only CMYK, resident decode † | 518.51 | 396.74 | 1.307 / 1.353 | 1,085,763 | — |
| JPEG YCCK420, caller RGB24 decode (confirmed pass) † | 441.76 | 624.43 | 0.707 / 0.742 | 762,700 | — |

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
| Bicubic resize RGBA8 † → 256×129 | 549.84 | 418.61 | 1.313 / 1.321 | 199,981 |
| Bicubic upscale RGBA16 † → 1027×519 | 3,429.17 | 3,125.34 | 1.097 / 1.113 | 4,264,538 |
| Bicubic upscale RGBA8, default workspace † → 1027×519 | 3,315.52 | 3,040.01 | 1.091 / 1.120 | 116 |
| Bicubic RGBA16, width down / height up / input+output+4 KiB † H → 256×519 | 1,040.15 | 844.30 | 1.232 / 1.396 | 1,063,419 |
| Bicubic RGBA8, 32×16 / input+output+4 KiB H → 32×16 | 464.68 | 375.02 | 1.239 / 1.370 | 2,185 |
| Bicubic RGBA8, 32×16 / default workspace † → 32×16 | 384.00 | 403.93 | 0.951 / 0.962 | 197,298 |
| Bicubic RGBA8 enlargement, input+output+4 KiB † → 1027×519 | 3,212.58 | 3,052.03 | 1.053 / 1.072 | 116 |
| Bicubic RGBA8 width enlargement, input+output+4 KiB † → 1027×259 | 1,546.70 | 1,524.36 | 1.015 / 1.066 | 114 |
| Bicubic RGBA8 height enlargement, input+output+4 KiB † → 513×519 | 1,811.37 | 1,745.56 | 1.038 / 1.061 | 115 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129 | 617.43 | 456.16 | 1.354 / 1.363 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 256×129 | 751.02 | 619.10 | 1.213 / 1.223 | 113 |
| Bicubic RGBA8 wider-support reduction † H → 128×64 | 493.82 | 347.10 | 1.423 / 1.445 | 231,241 |
| Bicubic RGBA16, width up / height down, input+output+4 KiB † H → 1027×129 | 1,112.92 | 850.82 | 1.308 / 1.369 | 1,060,371 |
| Bicubic RGBA8, width up / height down, input+output+4 KiB † H → 1027×129 | 1,167.23 | 864.52 | 1.350 / 1.367 | 530,400 |
| Bicubic RGBA8 reduction, input+output+4 KiB † H → 64×32 | 545.55 | 370.06 | 1.474 / 1.506 | 8,329 |
| Bicubic RGBA16 reduction, input+output+4 KiB † H → 64×32 | 518.01 | 389.68 | 1.329 / 1.355 | 16,521 |
| Bicubic RGBA8 reduction, input+output+4 KiB † H → 128×64 | 480.55 | 347.12 | 1.384 / 1.394 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † H → 128×64 | 532.73 | 404.07 | 1.318 / 1.329 | 113 |
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
| Prepared text RGBA8 / ImageSharp.Drawing (gap) | 189.86 | 45.43 | 4.179 / 5.230 | 92,384 |
| Prepared text RGBA16 / ImageSharp.Drawing (gap) | 189.35 | 45.98 | 4.118 / 5.226 | 92,384 |
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
| Validate JPEG progressive420, span | 286.239 | 289.052 | 0.990 / 1.001 | 4,072 | Confidence open |
| Validate VP8L+alpha, span | 3,397.141 | 3,049.808 | 1.114 / 1.134 | 685,421 | Confirmed gap |
| Validate VP8+alpha, span | 6,174.289 | 5,443.414 | 1.134 / 1.139 | 554,478 | Confirmed gap |
| Validate PNG16, async nonseekable | 2,836.823 | 3,623.647 | 0.783 / 0.790 | 8,544 | Confirmed pass |
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
