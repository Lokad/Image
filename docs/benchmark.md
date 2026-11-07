# Lokad.Image benchmarks

Updated 2026-11-07. These **125 selected comparisons** combine qualified
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
  Nine existing shared-decoder controls:3pass/6gaps. The contemporary palette
  control regresses **19.25µs/3.46%**, with separated raw/normalized intervals;
  retain that tradeoff, not a general decoder speedup claim.
  Validation passes/stop; source-valid coarse peaks235,152/11,121B and retained0
  are reused, not remeasured physical peaks. Full output retains531,468B until disposal.
  Those five displayed decoder costs reuse unchanged executed paths.
  [Successful-block publication](../benchmarks/VP8-COMPLETED-BLOCK-PUBLICATION.md)
  separately saves **82.55µs/1.60% raw**,1.79% normalized on complete live opaque VP8.
  Current 5,072.31/native4,798.30µs,upper1.065 remains a gap;
  GC stays228,864B/op. Only abandoned private failure-state work is removed;
  successful cursors, every decision poll and public cleanup stay exact.
- **WebP encoding:** [matching-to-population storage](../benchmarks/WEBP-MATCHING-POPULATION-STORAGE.md)
  saves134,736B/encode on photograph/diagram and70,560B on transparent alpha.
  Separate admitted peaks remain3,470,059/3,251,328/3,656,309B; retained scratch zero.
  Eight affected comparisons refresh:5pass/3uncertain (photograph/opaque/Alpha8).
  No causal CPU-gain claim. Seven unaffected lossy controls reuse
  [per-block preparation](../benchmarks/VP8-SMALL-BLOCK-PREPARATION.md), including
  its54.30µs raw gain and normalized uncertainty. Combined lossy scope:
  5pass/3gaps/1uncertain.
  Exact complete packets remain unchanged; passing controls stop.
- **PNG:** [prepared literal prefixes](../benchmarks/PNG-PREPARED-PREFIX.md) save
  422µs/12.8% on two complete retained-output decodes under one shared root.
  Current ratio 1.009/upper 1.014 remains a gap. Nominal arrays fit the same4KiB
  scratch; admitted peak1,766,641B and retained pair1,062,936B stay unchanged.
  Total GC measures1,638,214B/op, +694B; no allocation-reduction claim.
  [Current framing controls](../benchmarks/PNG-CURRENT-FRAMING-COSTS.md):
  51 independently confirmed controls, 45 pass/stop,
  6 gaps and 0 uncertain. All47 existing resident
  decode controls and four coding validators now have evidence; no resident
  confirmation remains missing. Passing cases stop. The report lists exact scopes,
  allocations and remaining gaps. Twenty-seven confirmations reuse the immutable
  prepared build, worker and harness; all125 displayed numbers reuse valid evidence.
  No new case, source change, rebuild or repeated package qualification. Header-only
  inspection/identification, stream/composition/shared-root boundaries and encoding
  keep their separate required coverage. No historical paired gain is inferred.
  [Lazy encoder preparation](../benchmarks/PNG-LAZY-VIEW-PREPARATION.md)
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
  [Shared word preparation](../benchmarks/BICUBIC-SHARED-WORD-PREPARATION.md) removes
  duplicated divisor/certificate setup. Independent mixed/cross gains41.73/83.80µs
  have separated raw/normalized confidence; tight vertical regresses11.96µs/1.66%,
  also separated. Existing division, guards, rounding, exact repair and pixels remain.
  Fifteen current word controls:6pass/9gaps; default/bounded full enlargement pass.
  Four displayed rows refresh;121 numerical rows reuse unchanged paths. No new
  owned arrays or retained state; admitted/physical peaks are not newly measured.
  [Shared enlargement](../benchmarks/BICUBIC-SHARED-ENLARGEMENT.md) retains752,064B/op
  workspace savings and source-valid byte costs. No new retained cache/pool or precision policy.
  [Bilinear boundary refresh](../benchmarks/WARM-COVERAGE.md) measures complete byte
  enlargement2,357µs/102,891B per operation, ratio1.093/upper1.134 but uncertain.
  Its older751µs gap is superseded; keep current uncertainty before further tuning.
  Exact Q30/rational/certified pixels remain required versus native float.
- **JPEG and reading:** [high-aligned entropy readers](../benchmarks/JPEG-HIGH-ALIGNED-READER.md)
  independently save37.62µs/2.13%raw on complete RGB decode and82.26µs/4.01%raw on
  live-token decode; both raw/normalized confidence views separate. Allocation stays
  10,088B/op. Shared signed-field arithmetic preserves canonical cursor/fallbacks,
  cancellation, exact pixels and packets. Current RGB/live uppers1.206/1.163 remain gaps.
  Sparse CMYK's first raw/normalized disagreement and reversed-order check remain
  recorded; no reliable normalized gain/loss is claimed. Twenty-three affected baseline
  costs refresh; six progressive confirmations reuse unchanged paths. Fourteen pass,
  fourteen gaps, one uncertain: baseline420 now passes; callerCMYK upper1.001 is uncertain.
  Ten displayed rows refresh;115 rows reuse unchanged paths. No blanket JPEG gain.
  [Full-grid ownership](../benchmarks/JPEG-FULL-GRID-OUTPUT.md) retains earlier paired
  gains and adverse stages. Its source-valid native444 governed peak541,253B and retained
  output531,468B are unchanged; default pool remains bounded, governed calls bypass it.
  These capacity facts are separate from total allocations and unmeasured physical/native peaks.
  [Grayscale encoding](../benchmarks/JPEG-GRAYSCALE-ROW-BATCH.md) against explicitly
  labeled SIMD libjpeg-turbo and other encoder costs remain source-valid.
- **Text/drawing:** [active edges](../benchmarks/TEXT-ACTIVE-EDGE-SCHEDULING.md)
  preserve exact frames,92,384B/draw and48,544B admitted Latin workspace;
  drawing remains a gap. [Layout/fitting and RGBA16 composition](../benchmarks/TEXT-DRAWING-CURRENT.md)
  pass/stop. These independent cases do not block codec or resize work.
  [Owned font storage](../benchmarks/FONT-OWNED-STORAGE.md) saves258.73µs/89.04%raw
  on complete loading:31.86µs versus labeledFreeType13.42µs, upper2.396 still a gap.
  Allocation falls757,597→176B/op; fullcopy/eager checks remain timed. The isolated
  load retains757,076B idle within the existing shared two-array/4MiB ceiling;
  governed calls bypass it. This is not a physical-peak reduction or parser-equivalence claim.

The current production build passes **5,867 full/live checks per OS**, affected
normal/scalar checks, and the **same actual Release package/all three examples
on Windows/Linux**, including exact mixed-alpha text at both depths, alpha and
opaque-word, progressive-strip, wide-column, shared byte/word enlargement and live VP8
sync/async consumers, plus bounded main validation and exact method2 residual storage/cancellation/recovery.
Affected JPEG paths pass398 internal/375 public checks per normal/scalar mode
on each OS, including signed fields, exact cursor/partial state, terminal refusal,
cancellation/recovery and surviving outputs. Unchanged word/WebP/other ISA evidence is reused.
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
| WebP VP8 + alpha, fresh seekable Stream † | 6,229.92 | 5,995.22 | 1.039 / 1.043 | 238,480 | — |
| JPEG 4:4:4, forward-only / exact reference (confidence open) † | 3,733.67 | 3,774.82 | 0.989 / 1.009 | 820,150 | 133,039 / 133,041 |
| JPEG 4:2:0 from caller RGB samples † | 1,655.50 | 1,580.59 | 1.047 / 1.061 | 388,220 | 52,091 / 52,093 |
| JPEG RGBA16 4:2:0 / caller IMemoryBudget (confidence open) † | 1,876.82 | 1,835.59 | 1.022 / 1.038 | 819,488 | 52,115 / 52,117 |
| JPEG grayscale, standalone SIMD libjpeg-turbo † | 1,660.52 | 1,522.24 | 1.091 / 1.103 | 155,436 | 58,910 / 58,910 |
| JPEG from caller GRAY8, standalone SIMD libjpeg-turbo † | 1,218.17 | 1,136.37 | 1.072 / 1.090 | 155,439 | 58,910 / 58,910 |
| VP8L photograph / libwebp exact=1 † | 81,999.54 | 82,030.95 | 1.000 / 1.008 | 6,382,422 | 160,032 / 160,032 |
| VP8L text/diagram / libwebp exact=1 † | 7,408.85 | 10,140.97 | 0.731 / 0.742 | 4,229,194 | 7,234 / 7,234 |
| VP8 opaque RGBA8 † | 13,891.65 | 15,503.56 | 0.896 / 0.898 | 1,201,560 | 72,380 / 72,380 |
| VP8 opaque RGBA16, including normalization † | 13,986.62 | 15,595.93 | 0.897 / 0.899 | 1,201,560 | 72,380 / 72,380 |
| VP8 opaque photograph † | 9,882.98 | 9,712.50 | 1.018 / 1.022 | 809,233 | 17,688 / 17,688 |
| VP8 opaque text/diagram † | 5,494.05 | 5,798.04 | 0.948 / 0.953 | 741,196 | 7,100 / 7,100 |
| VP8 diagram, effort2 / distortion-only search † | 2,745.35 | 2,501.11 | 1.098 / 1.104 | 406,705 | 7,330 / 7,330 |
| VP8 diagram, effort6 / trellis † | 9,452.61 | 9,810.83 | 0.963 / 0.993 | 754,505 | 6,818 / 6,818 |
| VP8 RGBA8 + lossless alpha † | 53,177.75 | 53,259.99 | 0.998 / 1.008 | 6,963,840 | 98,590 / 98,590 |
| VP8 RGBA16 + alpha, including normalization † | 53,145.13 | 53,609.96 | 0.991 / 0.996 | 6,963,024 | 98,590 / 98,590 |
| VP8L RGBA8 / libwebp exact=1 † | 105,483.31 | 122,008.61 | 0.865 / 0.875 | 7,341,808 | 183,812 / 183,812 |
| VP8L opaque RGBA8 / libwebp exact=1 † | 69,832.01 | 69,828.52 | 1.000 / 1.015 | 5,848,592 | 121,896 / 121,896 |
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
| PNG RGBA8 decode, live token † | 1,539.10 | 1,394.18 | 1.104 / 1.113 | 4,472 | — |
| PNG RGBA8 decode, tight working limit † | 1,297.38 | 1,395.09 | 0.930 / 0.935 | 68,224 | — |
| PNG RGBA16 decode, tight working limit † | 3,019.22 | 2,895.06 | 1.043 / 1.052 | 70,272 | — |
| PNG predictable RGBA16 3000², Optimal6 / mixed6 † H | 352,397.34 | 407,457.02 | 0.865 / 0.870 | 3,402,572 | 573,041 / 573,041 |
| PNG RGBA16, one-shot Fastest1 / native1 † | 18,449.19 | 18,610.40 | 0.991 / 1.019 | 2,437,660 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, live cancelable token † | 18,727.28 | 18,564.11 | 1.009 / 1.016 | 2,437,660 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, forward-only output † | 18,557.49 | 18,594.14 | 0.998 / 1.004 | 2,437,692 | 968,216 / 968,216 |
| PNG RGBA8 async, one-shot Optimal6, nonseekable output † | 16,996.93 | 17,633.09 | 0.964 / 0.968 | 882,212 | 210,973 / 210,973 |
| JPEG baseline420, resident decode (confirmed pass) † | 993.21 | 1,005.63 | 0.988 / 0.993 | 757,363 | — |
| JPEG DC-only, caller CMYK decode (confidence open) † | 331.02 | 331.70 | 0.998 / 1.001 | 1,085,963 | — |
| VP8L RGBA8, resident decode † | 3,093.70 | 3,008.39 | 1.028 / 1.034 | 152,728 | — |
| VP8L RGBA8, fresh seekable Stream decode † | 3,090.59 | 3,027.80 | 1.021 / 1.027 | 152,824 | — |
| JPEG CMYK sparse-2D, resident decode † | 526.35 | 437.63 | 1.203 / 1.221 | 4,520 | — |
| JPEG YCCK420, resident decode (confirmed pass) † | 426.02 | 707.58 | 0.602 / 0.603 | 895,547 | — |
| JPEG 4:1:1, resident decode † | 516.26 | 446.62 | 1.156 / 1.185 | 751,295 | — |
| JPEG 4:1:0, resident decode (confirmed pass) † | 355.57 | 507.41 | 0.701 / 0.708 | 720,838 | — |
| VP8L RGBA16, including normalization / libwebp exact=1 † | 107,919.76 | 121,030.61 | 0.892 / 0.901 | 7,341,808 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 104,422.47 | 120,958.63 | 0.863 / 0.876 | 7,342,192 | 183,812 / 183,812 |
| JPEG caller RGB24 async nonseekable, 420 (confidence open) † | 1,654.09 | 1,635.82 | 1.011 / 1.026 | 309,524 | 52,091 / 52,093 |
| JPEG RGBA8 async nonseekable, 444 (confirmed pass) † | 3,727.72 | 3,877.52 | 0.961 / 0.971 | 820,531 | 133,039 / 133,041 |
| PNG8 decode, two retained images / shared caller root † | 2,870.80 | 2,845.40 | 1.009 / 1.014 | 1,638,214 | — |
| JPEG dense CMYK, resident decode (confirmed pass) † | 2,221.81 | 3,111.90 | 0.714 / 0.717 | 4,520 | — |
| JPEG horizontal-only CMYK, resident decode † | 466.37 | 398.81 | 1.169 / 1.173 | 4,520 | — |
| JPEG YCCK420, caller RGB24 decode (confirmed pass) † | 428.13 | 604.06 | 0.709 / 0.710 | 762,704 | — |
| JPEG progressive grayscale, resident decode † | 1,897.32 | 1,710.22 | 1.109 / 1.115 | 683,136 | — |
| JPEG progressive420, resident decode (confirmed pass) † | 1,733.75 | 1,951.39 | 0.888 / 0.893 | 775,323 | — |
| JPEG progressive422, resident decode (confirmed pass) † | 2,213.98 | 2,251.17 | 0.983 / 0.991 | 847,116 | — |
| JPEG ordinary progressive444, resident decode † | 3,490.27 | 3,361.16 | 1.038 / 1.046 | 169,584 | — |
| JPEG progressive444, live-token resident decode † | 3,535.19 | 3,355.01 | 1.054 / 1.058 | 169,584 | — |

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
| Bicubic upscale RGBA16 † → 1027×519 | 2,752.40 | 2,874.23 | 0.958 / 0.966 | 4,264,573 |
| Bicubic upscale RGBA8, default workspace † → 1027×519 | 3,315.52 | 3,040.01 | 1.091 / 1.120 | 116 |
| Bicubic RGBA16, width down / height up / input+output+4 KiB † → 256×519 | 862.45 | 825.16 | 1.045 / 1.049 | 114 |
| Bicubic RGBA8, 32×16 / input+output+4 KiB H → 32×16 | 464.68 | 375.02 | 1.239 / 1.370 | 2,185 |
| Bicubic RGBA8, 32×16 / default workspace † → 32×16 | 384.00 | 403.93 | 0.951 / 0.962 | 197,298 |
| Bicubic RGBA8 enlargement, input+output+4 KiB † → 1027×519 | 3,212.58 | 3,052.03 | 1.053 / 1.072 | 116 |
| Bicubic RGBA8 width enlargement, input+output+4 KiB † → 1027×259 | 1,546.70 | 1,524.36 | 1.015 / 1.066 | 114 |
| Bicubic RGBA8 height enlargement, input+output+4 KiB † → 513×519 | 1,811.37 | 1,745.56 | 1.038 / 1.061 | 115 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129 | 617.43 | 456.16 | 1.354 / 1.363 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 256×129 | 751.02 | 619.10 | 1.213 / 1.223 | 113 |
| Bicubic RGBA8 wider-support reduction † H → 128×64 | 493.82 | 347.10 | 1.423 / 1.445 | 231,241 |
| Bicubic RGBA16, width up / height down, input+output+4 KiB † → 1027×129 | 1,017.38 | 847.82 | 1.200 / 1.204 | 114 |
| Bicubic RGBA8, width up / height down, input+output+4 KiB † H → 1027×129 | 1,167.23 | 864.52 | 1.350 / 1.367 | 530,400 |
| Bicubic RGBA8 reduction, input+output+4 KiB † H → 64×32 | 545.55 | 370.06 | 1.474 / 1.506 | 8,329 |
| Bicubic RGBA16 reduction, input+output+4 KiB † H → 64×32 | 518.01 | 389.68 | 1.329 / 1.355 | 16,521 |
| Bicubic RGBA8 reduction, input+output+4 KiB † H → 128×64 | 480.55 | 347.12 | 1.384 / 1.394 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † H → 128×64 | 532.73 | 404.07 | 1.318 / 1.329 | 113 |
| Bicubic RGBA8, width down / height up, default workspace † H → 256×519 | 912.31 | 841.39 | 1.084 / 1.107 | 731,768 |
| Bicubic RGBA16, width down / height up, default workspace † → 256×519 | 862.04 | 819.16 | 1.052 / 1.057 | 199,892 |
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
| Validate PNG8, span † | 1,287.369 | 1,377.047 | 0.935 / 0.938 | 240 | Confirmed pass |
| Validate PNG16, span † | 2,906.210 | 2,961.985 | 0.981 / 0.983 | 240 | Confirmed pass |
| Validate indexed Adam7 19×17, span † | 1.760 | 3.939 | 0.447 / 0.463 | 792 | Confirmed pass |
| Validate JPEG baseline420, span | 475.806 | 676.157 | 0.704 / 0.709 | 9,984 | Confirmed pass |
| Validate JPEG progressive420, span | 271.019 | 268.344 | 1.010 / 1.019 | 4,072 | Confirmed gap |
| Validate VP8L+alpha, span | 2,309.164 | 3,044.518 | 0.758 / 0.765 | 297,070 | Confirmed pass |
| Validate VP8+alpha, span | 4,646.035 | 5,440.023 | 0.854 / 0.860 | 13,584 | Confirmed pass |
| Validate PNG16, async nonseekable † | 2,956.807 | 3,613.242 | 0.818 / 0.826 | 8,544 | Confirmed pass |
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
