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
  [Prepared coefficient models](../benchmarks/VP8-PREPARED-COEFFICIENT-MODEL.md)
  separately save **155.50µs/3.08% raw**,3.16% normalized on complete opaque VP8;
  both confidence views separate. That model-only stage established no alpha gain.
  [Fused horizontal alpha publication](../benchmarks/WEBP-HORIZONTAL-ALPHA-PUBLICATION.md)
  separately saves **196.04µs/3.21% raw**,3.25% normalized, on complete resident
  decoding, with separated filtered/all-actual raw/normalized confidence.
  Current resident5,919.97/native5,962.52µs,
  upper1.002 is uncertain; stream upper0.992 passes/stop.
  GC stays238,384/238,480B/op, retained output531,468B; no new owned capacity.
  Two alpha costs refresh, four unchanged VP8 costs reuse their model evidence:
  two pass, three gaps, one uncertain. Live opaque is now
  4,979.43/native4,789.64µs,
  upper1.044 remains a gap. GC stays228,864B/op.
  One 1,056 B typed owner serves header and readers; per-block casting and ordinary
  numeric token dispatch retire. Actual complete-kernel coverage witnesses all12
  symbols. Probability/category/sign order, every poll, exact cursor/pixels and
  ownership remain. No extra model, capacity, cache or retained state.
  Earlier [live-class gains](../benchmarks/VP8-DIRECT-COEFFICIENT-CLASSES.md) remain history.
- **WebP encoding:** [matching-to-population storage](../benchmarks/WEBP-MATCHING-POPULATION-STORAGE.md)
  saves134,736B/encode on photograph/diagram and70,560B on transparent alpha.
  Separate admitted peaks remain3,470,059/3,251,328/3,656,309B; retained scratch zero.
  Six unchanged lossless comparisons reuse that evidence; photograph/opaque
  remain uncertain. No causal CPU-gain claim for that storage change.
  [Four-block histograms](../benchmarks/VP8-FOUR-BLOCK-HISTOGRAM.md) separately
  save **101.15µs/3.69% raw**,3.47% reference-normalized, on complete effort2.
  Both confidence views separate. The same128B dead coefficient workspace holds
  bins; no new owned capacity or retained state. GC stays406,705B/op. Current
  2,638.29/native2,504.45µs,upper1.058 remains a134µs gap. All nine affected lossy
  controls refresh, including alpha; causal gain is established only for effort2.
  Photograph and Alpha8 remain uncertain; effort6 passes its fixed four-launch
  envelope. Combined lossy scope:
  6pass/1gaps/2uncertain.
  Exact complete packets remain unchanged; passing controls stop.
- **PNG:** [prepared literal prefixes](../benchmarks/PNG-PREPARED-PREFIX.md) save
  422µs/12.8% on two complete retained-output decodes under one shared root.
  Current ratio 1.009/upper 1.014 remains a gap. Nominal arrays fit the same4KiB
  scratch; admitted peak1,766,641B and retained pair1,062,936B stay unchanged.
  Total GC measures1,638,214B/op, +694B; no allocation-reduction claim.
  [Current framing controls](../benchmarks/PNG-CURRENT-FRAMING-COSTS.md):
  51 independently confirmed controls, 45 pass/stop,
  5 gaps and 1 uncertain. All47 existing resident
  decode controls and four coding validators now have evidence; no resident
  confirmation remains missing. Passing cases stop. The report lists exact scopes,
  allocations and remaining gaps.
  [Live local cursors](../benchmarks/PNG-LIVE-PREPARED-CURSOR.md) save
  **147.18µs/9.56% raw/9.54% normalized** on RGBA8 and **31.01µs/1.05% raw/1.04%
  normalized** on RGBA16; filtered/allactual raw/normalized intervals separate.
  Current1391.93/native1393.91µs upper1.004 remains uncertain; word2919.94/
  native2894.87µs upper1.012 remains a gap. No favorable resampling or parity claim.
  GC4472/8568Bop and4KiB framing admission unchanged; no new heap capacity or
  retained scratch. Exact pixels, full lookahead, per-field polls, public errors
  and completed cursor remain. Failed private readers are abandoned; outer owner
  cleanup stays. Two costs refresh and49 unchanged controls reuse prior evidence.
  One displayed row refreshes;124 numerical rows reuse unchanged paths.
  Header-only inspection/identification, stream/composition/shared-root boundaries
  and encoding keep their separate required coverage.
  [Streaming token frequencies](../benchmarks/PNG-STREAMING-TOKEN-FREQUENCIES.md)
  remove the greedy block's second token census using its existing storage.
  Independent live/plain word gains are516–550/557µs; filtered/allactual raw and
  normalized intervals separate. All three word Fastest delivery controls pass/stop.
  Byte's filtered113µs gain has overlapping allactual intervals. Fifteen fresh
  complete one-shot controls:12pass/stop,2gaps,1uncertain. ByteFastest(~87µs) and
  Gray8(~60µs) remain gaps; wordSmallest remains uncertain. Exact packets and
  384KiB admission/zero retained scratch remain. Primary allocations are unchanged;
  Gray8 measures−408B/op without a capacity-saving claim. Earlier
  [lazy](../benchmarks/PNG-LAZY-VIEW-PREPARATION.md) and
  [greedy preparation](../benchmarks/PNG-GREEDY-VIEW-PREPARATION.md) retain their
  independent gains as linked history.
- **Resizing:** [Lanczos](../benchmarks/LANCZOS-PAIRED-CERTIFICATE-PREPARATION.md)
  has five passing cases and three prepared gaps (uppers1.158/1.097/1.052).
  Prepared plans retain139,800–333,396B; one-shot setup stays timed on both sides.
  [Shared streaming](../benchmarks/BICUBIC-SHARED-STREAMING.md) passes six of eight;
  [small](../benchmarks/BICUBIC-WORD-COLUMN-PREPARATION.md) and
  [wide columns](../benchmarks/BICUBIC-WIDE-PREPARATION.md) retain named gaps.
  [Shared band execution](../benchmarks/BICUBIC-BOUNDED-BAND-EXECUTION.md) saves
  **159.23µs/14.79% raw/15.40% normalized** on default mixed-word resizing and
  **86.23µs/8.03% raw/8.71% normalized** at4KiB, with separated raw/normalized
  confidence. Current917.38/native851.87µs upper1.079 and987.47/native851.08µs
  upper1.164 remain gaps.
  GC stays114/113Bop. Sixteen-row scratch7596B is admitted as8192B; bounded
  mode stays3992/4096B. Scratch retention0, mixed output1059864B until disposal;
  existing pixel-pool retention stays separate, physical/native peaks unmeasured.
  [Identity-axis geometry](../benchmarks/BICUBIC-IDENTITY-WORD-GEOMETRY.md) saves
  **271.81µs/37.22% raw/36.98% normalized** on bounded word height reduction:
  458.51/native589.46µs,upper0.779 passes/stop,113B/op unchanged. It removes four
  exactzero horizontal taps in the shared loop; exactpixels, repair and ownership
  remain. Its result retains529,416B, scratch0; existingidlepool staysseparate.
  Fourteen other word costs reuse valid paths. Fifteen now7pass/8gaps;
  passing enlargement stays stopped. One existing materialcase is newly displayed,
  with125 numerical rows reused. Earlier
  dynamic-capacity regression remains in the report, without renderer dependencies.
  One source loop/static capacities, no new heap/pool/session or pixel tolerance.
  [Shared enlargement](../benchmarks/BICUBIC-SHARED-ENLARGEMENT.md) retains752,064B/op
  workspace savings and source-valid byte costs. Its unchanged-build defaultRGBA8
  confirmation measures3,108.21/native3,037.83µs,upper1.026/116Bop: a70.38µs gap.
  This resolves earlier uncertainty without a causal historical-gain claim.
  No new retained cache/pool or precision policy.
  [Bilinear boundary refresh](../benchmarks/WARM-COVERAGE.md) measures complete byte
  enlargement2,357µs/102,891B per operation, ratio1.093/upper1.134 but uncertain.
  Its older751µs gap is superseded; keep current uncertainty before further tuning.
  Exact Q30/rational/certified pixels remain required versus native float.
- **JPEG and reading:** [paired AC windows](../benchmarks/JPEG-PAIRED-AC-WINDOW.md)
  save **113.83µs/6.92% raw/6.75% normalized** on the frozen RGB444 control and
  15.91µs/3.02% raw on sparse CMYK; filtered/allactual raw/normalized intervals
  separate. RGB1530.34/native1450.52µs,upper1.065 remains an80µs gap. New scan-owned
  tables cost32,848/16,424 extra GC B/op forRGB/CMYK; no retained pair scratch.
  Governed RGB peak541,253→574,069B, output531,468B unchanged; live peak stays
  541,253B. Default idle pixel pool531,468B is unchanged, governed calls bypass it.
  Physical/native peaks are unmeasured. Header preparation is timed and admission
  precedes allocation; refusal remains terminal. Empty plans skip allocation.
  All29JPEG controls refresh, including constructor layout on progressive paths:
  14pass/14gaps/1uncertain. Owned gray moved from passing to uncertain
  (679.83/native679.12µs,upper1.009); the adverse result remains, with no broader
  nonregression claim. Native444 now passes; callerCMYK is a demonstrated gap.
  Earlier [live windows](../benchmarks/JPEG-LIVE-ENTROPY-WINDOW.md),
  [publication](../benchmarks/JPEG-LIVE-NORMAL-PUBLICATION.md) and
  [long lookup](../benchmarks/JPEG-BUFFERED-LONG-LOOKUP.md) retain historical gains.
  [Grayscale encoding](../benchmarks/JPEG-GRAYSCALE-ROW-BATCH.md) against explicitly
  labeled SIMD libjpeg-turbo and other encoder costs remain source-valid.
- **Text/drawing:** [shared raster sorting](../benchmarks/TEXT-RASTER-SHARED-SORT.md)
  cuts allocation by25,016B/draw (27.08%) to67,368B. Point costs153.86/147.75µs
  have inconclusive causal CPUgains; both reference gaps remain. Exactframes,
 48,544B admitted Latin workspace and zero retainedscratch remain.
  [Layout/fitting and RGBA16 composition](../benchmarks/TEXT-DRAWING-CURRENT.md)
  pass/stop. These independent cases do not block codec or resize work.
  [Owned font storage](../benchmarks/FONT-OWNED-STORAGE.md) saves258.73µs/89.04%raw
  on complete loading:31.86µs versus labeledFreeType13.42µs, upper2.396 still a gap.
  Allocation falls757,597→176B/op; fullcopy/eager checks remain timed. The isolated
  load retains757,076B idle within the existing shared two-array/4MiB ceiling;
  governed calls bypass it. This is not a physical-peak reduction or parser-equivalence claim.

The current production build passes **5,905 full/live checks per OS**, affected
normal/scalar checks, and the **same actual Release package/all three examples
on Windows/Linux**, including exact mixed-alpha text at both depths, alpha and
opaque-word, progressive-strip, wide-column, shared byte/word enlargement and live VP8
sync/async consumers, plus bounded main validation and exact method2 residual storage/cancellation/recovery.
The package also verifies frozen vertical RGBA16 pixels at4/8KiB, aggregate
admission, terminalrefusal, cancellationcleanup and independent survivors.
JPEG paths pass407 internal/375 public checks per normal/scalar mode
on each OS, including actual live-window DC/EOB/following-read witnesses,
truncation/stuffing/partial failures, refusal/cancellation cleanup and survivors.
The current PNG package also exercises frozen Fastest8/16 sync/async packets,
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
| WebP VP8 + alpha, fresh seekable Stream (parity confirmed) † | 5,939.22 | 6,011.78 | 0.988 / 0.992 | 238,480 | — |
| JPEG 4:4:4, forward-only / exact reference (confidence open) † | 3,733.67 | 3,774.82 | 0.989 / 1.009 | 820,150 | 133,039 / 133,041 |
| JPEG 4:2:0 from caller RGB samples † | 1,655.50 | 1,580.59 | 1.047 / 1.061 | 388,220 | 52,091 / 52,093 |
| JPEG RGBA16 4:2:0 / caller IMemoryBudget (confidence open) † | 1,876.82 | 1,835.59 | 1.022 / 1.038 | 819,488 | 52,115 / 52,117 |
| JPEG grayscale, standalone SIMD libjpeg-turbo † | 1,660.52 | 1,522.24 | 1.091 / 1.103 | 155,436 | 58,910 / 58,910 |
| JPEG from caller GRAY8, standalone SIMD libjpeg-turbo † | 1,218.17 | 1,136.37 | 1.072 / 1.090 | 155,439 | 58,910 / 58,910 |
| VP8L photograph / libwebp exact=1 † | 81,999.54 | 82,030.95 | 1.000 / 1.008 | 6,382,422 | 160,032 / 160,032 |
| VP8L text/diagram / libwebp exact=1 † | 7,408.85 | 10,140.97 | 0.731 / 0.742 | 4,229,194 | 7,234 / 7,234 |
| VP8 opaque RGBA8 † | 13,865.79 | 15,509.82 | 0.894 / 0.898 | 1,201,560 | 72,380 / 72,380 |
| VP8 opaque RGBA16, including normalization † | 13,882.59 | 15,579.01 | 0.891 / 0.894 | 1,201,560 | 72,380 / 72,380 |
| VP8 opaque photograph † | 9,630.85 | 9,657.98 | 0.997 / 1.001 | 809,233 | 17,688 / 17,688 |
| VP8 opaque text/diagram † | 5,476.34 | 5,803.20 | 0.944 / 0.952 | 741,196 | 7,100 / 7,100 |
| VP8 diagram, effort2 / distortion-only search † | 2,638.29 | 2,504.45 | 1.053 / 1.058 | 406,705 | 7,330 / 7,330 |
| VP8 diagram, effort6 / trellis † | 9,256.01 | 9,813.99 | 0.943 / 0.999 | 754,505 | 6,818 / 6,818 |
| VP8 RGBA8 + lossless alpha † | 53,302.41 | 53,807.33 | 0.991 / 1.002 | 6,963,840 | 98,590 / 98,590 |
| VP8 RGBA16 + alpha, including normalization † | 53,129.08 | 53,549.12 | 0.992 / 0.996 | 6,963,024 | 98,590 / 98,590 |
| VP8L RGBA8 / libwebp exact=1 † | 105,483.31 | 122,008.61 | 0.865 / 0.875 | 7,341,808 | 183,812 / 183,812 |
| VP8L opaque RGBA8 / libwebp exact=1 † | 69,832.01 | 69,828.52 | 1.000 / 1.015 | 5,848,592 | 121,896 / 121,896 |
| PNG RGBA8, one-shot SmallestSize9 / native9 † | 17,026.17 | 17,248.19 | 0.987 / 0.992 | 820,452 | 210,973 / 210,973 |
| PNG RGBA16, one-shot SmallestSize9 / native9 † | 20,847.84 | 20,894.28 | 0.998 / 1.004 | 2,437,660 | 963,202 / 963,202 |
| PNG RGBA8, one-shot Fastest1 / native1 † | 4,427.44 | 4,340.50 | 1.020 / 1.040 | 820,916 | 236,828 / 236,828 |
| PNG RGBA16, NoCompression0 / native0 † H | 203.45 | 406.60 | 0.500 / 0.539 | 1,741,468 | 1,066,552 / 1,066,552 |
| PNG grayscale8, one-shot Optimal6 / native6 † | 2,043.52 | 1,983.53 | 1.030 / 1.065 | 551,364 | 72,586 / 72,586 |
| PNG RGB8, one-shot Optimal6 / native6 † | 9,481.32 | 9,777.65 | 0.970 / 0.982 | 819,884 | 168,729 / 168,729 |
| PNG gray-alpha8, one-shot Optimal6 / native6 † | 4,229.81 | 4,423.13 | 0.956 / 0.972 | 552,332 | 120,366 / 120,366 |
| PNG RGBA8, one-shot Optimal6 / native6 † | 16,857.24 | 17,278.11 | 0.976 / 0.982 | 820,452 | 210,973 / 210,973 |
| PNG RGBA16, one-shot Optimal6 / native6 † | 20,491.90 | 20,771.24 | 0.987 / 0.992 | 2,437,660 | 963,202 / 963,202 |
| PNG RGB16, one-shot Optimal6 / native6 † | 15,546.19 | 16,190.43 | 0.960 / 0.967 | 2,431,492 | 739,472 / 739,472 |
| PNG grayscale16, one-shot Optimal6 / native6, red-plane reference † | 4,826.60 | 5,417.93 | 0.891 / 0.911 | 822,740 | 265,433 / 265,433 |
| PNG gray-alpha16, one-shot Optimal6 / native6 † | 10,023.86 | 10,524.89 | 0.952 / 0.963 | 1,360,900 | 506,887 / 506,887 |
| PNG RGBA8, stored0 / native0 † H | 253.82 | 189.65 | 1.338 / 1.471 | 1,203,724 | 533,444 / 533,444 |
| PNG RGBA8 decode, live token † | 1,391.93 | 1,393.91 | 0.999 / 1.004 | 4,472 | — |
| PNG RGBA8 decode, tight working limit † | 1,297.38 | 1,395.09 | 0.930 / 0.935 | 68,224 | — |
| PNG RGBA16 decode, tight working limit † | 3,019.22 | 2,895.06 | 1.043 / 1.052 | 70,272 | — |
| PNG predictable RGBA16 3000², Optimal6 / mixed6 † H | 352,397.34 | 407,457.02 | 0.865 / 0.870 | 3,402,572 | 573,041 / 573,041 |
| PNG RGBA16, one-shot Fastest1 / native1 † | 17,956.64 | 18,627.96 | 0.964 / 0.970 | 2,437,660 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, live cancelable token † | 18,279.79 | 18,615.61 | 0.982 / 0.987 | 2,437,660 | 968,216 / 968,216 |
| PNG RGBA16, one-shot Fastest1, forward-only output † | 18,019.73 | 18,642.40 | 0.967 / 0.982 | 2,437,692 | 968,216 / 968,216 |
| PNG RGBA8 async, one-shot Optimal6, nonseekable output † | 16,950.89 | 17,518.41 | 0.968 / 0.973 | 882,212 | 210,973 / 210,973 |
| JPEG baseline420, resident decode (confirmed pass) † | 929.80 | 1,012.30 | 0.918 / 0.924 | 790,211 | — |
| JPEG DC-only, caller CMYK decode † | 337.97 | 332.25 | 1.017 / 1.019 | 1,085,977 | — |
| VP8L RGBA8, resident decode † | 3,093.70 | 3,008.39 | 1.028 / 1.034 | 152,728 | — |
| VP8L RGBA8, fresh seekable Stream decode † | 3,090.59 | 3,027.80 | 1.021 / 1.027 | 152,824 | — |
| JPEG CMYK sparse-2D, resident decode † | 511.72 | 436.56 | 1.172 / 1.178 | 20,944 | — |
| JPEG YCCK420, resident decode (confirmed pass) † | 428.14 | 706.85 | 0.606 / 0.608 | 895,563 | — |
| JPEG 4:1:1, resident decode † | 521.14 | 448.78 | 1.161 / 1.180 | 751,315 | — |
| JPEG 4:1:0, resident decode (confirmed pass) † | 363.99 | 506.96 | 0.718 / 0.731 | 720,854 | — |
| VP8L RGBA16, including normalization / libwebp exact=1 † | 107,919.76 | 121,030.61 | 0.892 / 0.901 | 7,341,808 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 104,422.47 | 120,958.63 | 0.863 / 0.876 | 7,342,192 | 183,812 / 183,812 |
| JPEG caller RGB24 async nonseekable, 420 (confidence open) † | 1,654.09 | 1,635.82 | 1.011 / 1.026 | 309,524 | 52,091 / 52,093 |
| JPEG RGBA8 async nonseekable, 444 (confirmed pass) † | 3,727.72 | 3,877.52 | 0.961 / 0.971 | 820,531 | 133,039 / 133,041 |
| PNG8 decode, two retained images / shared caller root † | 2,870.80 | 2,845.40 | 1.009 / 1.014 | 1,638,214 | — |
| JPEG dense CMYK, resident decode (confirmed pass) † | 1,390.69 | 3,111.67 | 0.447 / 0.450 | 20,944 | — |
| JPEG horizontal-only CMYK, resident decode † | 453.31 | 392.03 | 1.156 / 1.162 | 20,944 | — |
| JPEG YCCK420, caller RGB24 decode (confirmed pass) † | 435.42 | 604.87 | 0.720 / 0.722 | 762,721 | — |
| JPEG progressive grayscale, resident decode † | 1,898.18 | 1,710.59 | 1.110 / 1.113 | 683,176 | — |
| JPEG progressive420, resident decode (confirmed pass) † | 1,717.43 | 1,950.80 | 0.880 / 0.884 | 775,403 | — |
| JPEG progressive422, resident decode (confirmed pass) † | 2,205.54 | 2,240.65 | 0.984 / 0.991 | 847,196 | — |
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
| Bicubic resize RGBA8 † → 256×129 | 549.84 | 418.61 | 1.313 / 1.321 | 199,981 |
| Bicubic upscale RGBA16 † → 1027×519 | 2,752.40 | 2,874.23 | 0.958 / 0.966 | 4,264,573 |
| Bicubic upscale RGBA8, default workspace † → 1027×519 | 3,108.21 | 3,037.83 | 1.023 / 1.026 | 116 |
| Bicubic RGBA16, width down / height up / input+output+4 KiB † → 256×519 | 862.45 | 825.16 | 1.045 / 1.049 | 114 |
| Bicubic RGBA8, 32×16 / input+output+4 KiB H → 32×16 | 464.68 | 375.02 | 1.239 / 1.370 | 2,185 |
| Bicubic RGBA8, 32×16 / default workspace † → 32×16 | 384.00 | 403.93 | 0.951 / 0.962 | 197,298 |
| Bicubic RGBA8 enlargement, input+output+4 KiB † → 1027×519 | 3,212.58 | 3,052.03 | 1.053 / 1.072 | 116 |
| Bicubic RGBA8 width enlargement, input+output+4 KiB † → 1027×259 | 1,546.70 | 1,524.36 | 1.015 / 1.066 | 114 |
| Bicubic RGBA8 height enlargement, input+output+4 KiB † → 513×519 | 1,811.37 | 1,745.56 | 1.038 / 1.061 | 115 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129 | 617.43 | 456.16 | 1.354 / 1.363 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 256×129 | 751.02 | 619.10 | 1.213 / 1.223 | 113 |
| Bicubic RGBA8 wider-support reduction † H → 128×64 | 493.82 | 347.10 | 1.423 / 1.445 | 231,241 |
| Bicubic RGBA16, width up / height down, input+output+4 KiB † → 1027×129 | 987.47 | 851.08 | 1.160 / 1.164 | 114 |
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
| Prepared text RGBA8 / ImageSharp.Drawing (gap) | 153.86 | 45.58 | 3.376 / 4.360 | 67,368 |
| Prepared text RGBA16 / ImageSharp.Drawing (gap) | 147.75 | 45.90 | 3.219 / 4.522 | 67,368 |
| Composite RGBA8 / FFmpeg (gap) | 625.41 | 498.73 | 1.254 / 1.270 | 56 |
| Composite RGBA16 / ImageSharp 3.1.12 (pass) | 627.24 | 696.25 | 0.901 / 0.922 | 56 |
| Bicubic RGBA16 height reduction, input+output+4 KiB (pass) † → 513×129 | 458.51 | 589.46 | 0.778 / 0.779 | 113 |

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
| Validate JPEG baseline420, span | 477.351 | 674.564 | 0.708 / 0.711 | 10,016 | Confirmed pass |
| Validate JPEG progressive420, span | 330.131 | 268.515 | 1.229 / 1.237 | 4,088 | Confirmed gap |
| Validate VP8L+alpha, span | 2,309.164 | 3,044.518 | 0.758 / 0.765 | 297,070 | Confirmed pass |
| Validate VP8+alpha, span | 4,529.581 | 5,446.763 | 0.832 / 0.836 | 13,584 | Confirmed pass |
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
