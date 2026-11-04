# Lokad.Image benchmarks

Updated 2026-11-04. These **125 selected comparisons** combine qualified
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

Current priorities and scope:

- **WebP:** [exact packed reconstruction](../benchmarks/VP8-PACKED-RECONSTRUCTION.md)
  saves 337.10 µs / 2.27% raw on the photograph (2.05% normalized, native clock
  0.23% faster). Alpha's normalized confidence overlaps; its adverse short remains.
  All eight affected VP8 costs are current and retain gaps, with unchanged allocations.
  [ALPH-to-frame storage reuse](../benchmarks/WEBP-ALPHA-COEFFICIENT-STORAGE.md)
  retains its 449 KB/op saving and 7,165,880/7,165,472 B/op at byte/word depths.
  Its measured alpha8 admission peak is 3,656,309 B, retained scratch zero;
  the coefficient phase carries 82,668 B extra. Physical peak is unmeasured.
  [Unchanged VP8L paths](../benchmarks/WEBP-ALPHA-MATCH-INDEX-REUSE.md) reuse valid
  proof: original/diagram/depth/async pass and stop; opaque retains a gap.
  Photograph remains uncertain, including its adjacent-sample interval
  0.995–1.044; no unchanged resampling follows.
- **PNG:** [operation-owned block preparation](../benchmarks/PNG-BLOCK-PREPARATION.md)
  saves 148,320 B on word Smallest9 (5.7%) and 19,936 B on byte Fastest1 (2.4%)
  at equal batching, without a CPU gain/equivalence/regression claim. Nine current
  one-shot costs retain eight gaps; byte Optimal6 remains uncertain (upper 1.045).
  [Delivery/cancellation comparisons](../benchmarks/PNG-BOUNDARY-ONE-SHOT.md)
  include native codec preparation: word token/forward uppers 1.116/1.140;
  byte async upper 1.029. Word-workspace and complete async delivery stay timed.
  Existing 384 KiB admission is unchanged; two reused arrays add 2,620 B capacity,
  zero retained scratch after disposal. Complete physical/admitted peaks are
  unmeasured. [Earlier tree reuse](../benchmarks/PNG-TREE-WORKSPACE.md) is history.
- **Resizing:** [shared Lanczos driver](../benchmarks/LANCZOS-SHARED-ROW-DRIVER.md)
  saves 24 µs / 3.98% raw on prepared byte reduction, with unchanged allocations/
  retention and exact pixels. Word point increases have overlapping confidence;
  refreshed native-clock movements are disclosed in the report. Current coverage
  has five passing boundaries; prepared byte reduction and word reduction/
  enlargement retain gaps (uppers 1.201/1.121/1.100). One-shot and prepared calls
  compare equivalent setup/ownership. Plans retain 139,800/148,040 B for byte/word
  reduction and 325,156/333,396 B for enlargement. Managed Q30/rational/certified
  pixels stay exact; zimg uses float. [Shared bicubic streaming](../benchmarks/BICUBIC-SHARED-STREAMING.md)
  saves 66.62/51.17 µs (11.24%/9.95%) on byte/word 8×17, with unchanged allocations.
  Two schedulers serve four exact kernels within 4 KiB. Eight existing tight
  controls have current costs: six pass and stop; RGBA8 8×17/4×9 retain gaps
  (uppers 1.127/1.269). Managed preparation stays timed; native graphs/scratch are
  retained. [Column geometry preparation](../benchmarks/BICUBIC-COLUMN-PREPARATION.md)
  saves 8.84 µs / 1.41% raw on byte 256×129 with the same 4 KiB allowance and
  113 B/op; its remaining upper is 1.363. Native clock movement is +9.05%, disclosed
  separately from the raw gain. [Shared word preparation](../benchmarks/BICUBIC-WORD-COLUMN-PREPARATION.md)
  saves 126.92 µs / 14.46% raw on the existing word 256×129 control, with unchanged
  4 KiB admission (upper 1.223, 113 B/op; no allocation-saving claim). Its new row
  surfaces an already-required boundary. Other affected H costs still need reconciliation.
- **JPEG and reading:** [exact grayscale row batches](../benchmarks/JPEG-GRAYSCALE-ROW-BATCH.md)
  save 32.71 µs / 1.93% raw (4.04% reference-normalized) on the existing RGBA8
  grayscale encode, with unchanged 155,436 B/op and exact complete packets. The
  labeled SIMD libjpeg-turbo gap remains (upper 1.103); its native clock is 2.19%
  slower in confirmation. Caller-GRAY/RGB paths keep unchanged evidence. Other table statuses and
  [coverage](../benchmarks/WARM-COVERAGE.md) retain their named color/sampling/
  delivery scope. Sub-microsecond probe/copy gaps remain visible, with expensive
  codec/resize paths prioritized. Inspection, complete Identify and coding
  validation are distinct: PNG header inspection skips CRCs; Identify validates
  the container and nine source facts without pixel decoding; validation checks
  coding syntax without retaining an RGBA result.
- **Text/drawing:** [layout/fitting and RGBA16 compositing](../benchmarks/TEXT-DRAWING-CURRENT.md)
  pass their scoped comparisons. Loading, RGBA8 compositing and
  [rasterization](../benchmarks/TEXT-EDGE-INTERVAL-PREPARATION.md) retain gaps.
  These independent cases do not block codecs or resizing.

The current build passes **5,628 full/live checks per OS**, affected normal/scalar
checks, and the **same actual Release package/all three examples on Windows/Linux**.
Unchanged paths reuse valid evidence. This is correctness/package qualification,
not a full release refresh or whole-library performance pass. Complete physical/
admitted peaks are not inferred from allocation reductions.

**References and boundaries.** FFmpeg is the reference except for explicitly
labeled **libwebp exact=1** lossless WebP (hidden RGB preserved), SIMD-enabled
**libjpeg-turbo** grayscale JPEG, and **zimg** resizing (FFmpeg's zscale backend).
The nine current one-shot PNG rows time fresh codec/zlib/packet state and independent
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
| WebP VP8 + alpha, fresh seekable Stream † | 7,037.26 | 6,042.62 | 1.165 / 1.211 | 1,434,827 | — |
| JPEG 4:4:4, forward-only / exact reference (confidence open) † | 3,733.67 | 3,774.82 | 0.989 / 1.009 | 820,150 | 133,039 / 133,041 |
| JPEG 4:2:0 from caller RGB samples † | 1,655.50 | 1,580.59 | 1.047 / 1.061 | 388,220 | 52,091 / 52,093 |
| JPEG RGBA16 4:2:0 / caller IMemoryBudget (confidence open) † | 1,876.82 | 1,835.59 | 1.022 / 1.038 | 819,488 | 52,115 / 52,117 |
| JPEG grayscale, standalone SIMD libjpeg-turbo † | 1,660.52 | 1,522.24 | 1.091 / 1.103 | 155,436 | 58,910 / 58,910 |
| JPEG from caller GRAY8, standalone SIMD libjpeg-turbo † | 1,218.17 | 1,136.37 | 1.072 / 1.090 | 155,439 | 58,910 / 58,910 |
| VP8 diagram, effort2 / distortion-only search † | 3,522.60 | 2,709.10 | 1.300 / 1.311 | 741,375 | 7,330 / 7,330 |
| VP8 RGBA16 + alpha, including normalization † | 70,165.60 | 57,652.08 | 1.217 / 1.229 | 7,165,472 | 98,590 / 98,590 |
| VP8 opaque RGBA8 † | 20,366.51 | 16,737.93 | 1.217 / 1.225 | 1,201,552 | 72,380 / 72,380 |
| VP8 opaque photograph † | 14,482.28 | 10,521.42 | 1.376 / 1.391 | 809,072 | 17,688 / 17,688 |
| VP8 opaque text/diagram † | 8,123.43 | 6,270.92 | 1.295 / 1.304 | 741,137 | 7,100 / 7,100 |
| VP8 diagram, effort6 / trellis † | 14,550.71 | 10,511.03 | 1.384 / 1.402 | 754,344 | 6,818 / 6,818 |
| VP8 opaque RGBA16, including normalization † | 21,738.20 | 16,813.65 | 1.293 / 1.304 | 1,201,552 | 72,380 / 72,380 |
| VP8 RGBA8 + lossless alpha † | 69,657.13 | 57,733.16 | 1.207 / 1.217 | 7,165,880 | 98,590 / 98,590 |
| VP8L photograph / libwebp exact=1 † | 86,323.75 | 83,862.10 | 1.029 / 1.090 | 6,517,254 | 160,032 / 160,032 |
| VP8L text/diagram / libwebp exact=1 † | 8,159.66 | 9,566.19 | 0.853 / 0.871 | 4,363,930 | 7,234 / 7,234 |
| VP8L RGBA8 / libwebp exact=1 † | 115,815.96 | 130,140.89 | 0.890 / 0.906 | 7,476,640 | 183,812 / 183,812 |
| VP8L opaque RGBA8 / libwebp exact=1 † | 77,597.76 | 75,729.69 | 1.025 / 1.036 | 5,983,424 | 121,896 / 121,896 |
| PNG RGBA8, one-shot SmallestSize9 / native9 † | 19,417.24 | 18,696.87 | 1.039 / 1.049 | 820,444 | 210,973 / 210,973 |
| PNG RGBA16, one-shot SmallestSize9 / native9 † | 23,814.57 | 22,718.51 | 1.048 / 1.065 | 2,437,652 | 963,202 / 963,202 |
| PNG RGBA8, one-shot Fastest1 / native1 † | 5,940.17 | 4,734.88 | 1.255 / 1.276 | 820,908 | 236,828 / 236,828 |
| PNG RGBA16, NoCompression0 / native0 † H | 203.45 | 406.60 | 0.500 / 0.539 | 1,741,468 | 1,066,552 / 1,066,552 |
| PNG RGBA16, one-shot Fastest1 / native1 † | 21,689.76 | 20,151.55 | 1.076 / 1.085 | 2,437,652 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, live cancelable token † | 22,117.18 | 20,058.57 | 1.103 / 1.116 | 2,437,652 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, forward-only output † | 21,681.81 | 19,863.09 | 1.092 / 1.140 | 2,437,684 | 968,216 / 968,216 |
| PNG grayscale8, Optimal6 / native6 † H | 2,570.87 | 1,940.17 | 1.325 / 1.354 | 1,001,812 | 72,586 / 72,586 |
| PNG RGB8, Optimal6 / native6 † H | 12,457.28 | 9,513.16 | 1.309 / 1.324 | 1,325,068 | 168,729 / 168,729 |
| PNG gray-alpha8, Optimal6 / native6 † H | 5,626.99 | 4,312.43 | 1.305 / 1.350 | 1,021,036 | 120,366 / 120,366 |
| PNG RGBA8, one-shot Optimal6 / native6 † | 17,723.94 | 17,294.12 | 1.025 / 1.045 | 820,444 | 210,973 / 210,973 |
| PNG RGBA16, one-shot Optimal6 / native6 † | 23,400.62 | 22,554.07 | 1.038 / 1.047 | 2,437,652 | 963,202 / 963,202 |
| PNG RGB16, one-shot Optimal6 / native6 † | 18,413.80 | 17,724.93 | 1.039 / 1.048 | 2,431,484 | 739,472 / 739,472 |
| PNG grayscale16, Optimal6 / native6, red-plane reference † H | 6,812.24 | 5,434.49 | 1.254 / 1.285 | 1,506,972 | 265,433 / 265,433 |
| PNG gray-alpha16, Optimal6 / native6 † H | 13,361.83 | 10,171.22 | 1.314 / 1.333 | 2,264,564 | 506,887 / 506,887 |
| PNG RGBA8, stored0 / native0 † H | 253.82 | 189.65 | 1.338 / 1.471 | 1,203,724 | 533,444 / 533,444 |
| PNG RGBA8 async, one-shot Optimal6, nonseekable output † | 19,366.24 | 18,983.03 | 1.020 / 1.029 | 882,204 | 210,973 / 210,973 |
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
| VP8L RGBA16, including normalization / libwebp exact=1 † | 108,771.66 | 121,544.11 | 0.895 / 0.924 | 7,476,640 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 109,097.10 | 121,293.70 | 0.899 / 0.950 | 7,477,024 | 183,812 / 183,812 |
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
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 256×129 | 711.35 | 1,588.55 | 0.448 / 0.457 | 138,647 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 256×129 | 563.84 | 489.93 | 1.151 / 1.158 | 139 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 256×129 | 752.13 | 1,946.83 | 0.386 / 0.391 | 146,864 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 256×129 | 558.34 | 512.94 | 1.089 / 1.097 | 139 |
| Lanczos3 one-shot RGBA8 / zimg one-shot † → 1027×519 | 4,016.44 | 5,281.52 | 0.760 / 0.783 | 319,224 |
| Lanczos3 prepared RGBA8 / zimg prepared † → 1027×519 | 3,775.71 | 4,085.82 | 0.924 / 0.946 | 148 |
| Lanczos3 one-shot RGBA16 / zimg one-shot † → 1027×519 | 4,337.24 | 5,079.02 | 0.854 / 0.860 | 4,592,777 |
| Lanczos3 prepared RGBA16 / zimg prepared † → 1027×519 | 4,375.20 | 4,239.10 | 1.032 / 1.052 | 4,265,302 |
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
| Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129 | 617.43 | 456.16 | 1.354 / 1.363 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 256×129 | 751.02 | 619.10 | 1.213 / 1.223 | 113 |
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

[Coverage and linked family reports](../benchmarks/WARM-COVERAGE.md) retain producer
identities, confidence decisions, ownership/correctness gates and reproducible raw
evidence. Historical experiment reports remain separate. Update with
`python benchmarks/render-benchmark.py`; verify with `--check`.
