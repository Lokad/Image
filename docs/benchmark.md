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
  [Inline Huffman reads](../benchmarks/WEBP-HUFFMAN-READ-FRONTEND.md) save82.02µs/
  2.56%raw/2.54%normalized on live-token VP8L decoding. Current3.125/FFmpeg3.025ms,
  range1.018–1.043 remains a gap; allocation stays152,728B/op.
  All exact pixels/owners and12adjacent pairs/brackets pass; native retains codec/
  conversion contexts and has no equivalent cancellation token. Across six lossless
  reads/validators:3pass/1gap/2uncertain.
  Attribute the remaining approximately100µs gap before another substantial decoder change.
  The [lossy scope](../benchmarks/VP8-ROUNDED-BORDER-PREDICTION.md) now has
  6 passes / 3 uncertain. The current word-alpha view is
  53.40/FFmpeg 53.30 ms, upper 1.008: uncertain,
  exact normalized alpha/packet and 6,241,792 B/op. Resolve uncertainty before tuning. Complete packets remain
  exact. The same [Huffman round](../benchmarks/WEBP-HUFFMAN-READ-FRONTEND.md)
  refreshes all eight affected companion reads/validators without a uniform causal
  gain claim. VP8/ALPH plus unchanged opaque [position preparation](../benchmarks/VP8-POSITION-MODEL-PREPARATION.md) has
  4 passes / 0 gaps / 2 uncertain.
  Ordinary opaque decoding improves 2.02% after reference normalization, with separated
  raw/all-actual/normalized intervals. Live 4,803.55/native4,786.85µs,
  upper1.009, and resident ALPH remain uncertain; stream ALPH now passes.
  The current resident interval supersedes its older pass without establishing a regression.
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
- **Resizing:** [Shared horizontal preparation/execution](../benchmarks/CACHED-BICUBIC-SHARED-HORIZONTAL-SCHEDULE.md)
  saves 22.70 µs/4.78% raw/5.11% reference-normalized on narrow Down8,
  with separated filtered/all-actual and normalized intervals. Native time moves
  +0.35%. Current 452.28/zimg 454.42 µs, upper 1.005, remains uncertain;
  Wide8 378.51/375.02 µs upper 1.020 is also uncertain. Neither is a presumed tuning need.
  All 24 existing shared cached controls refresh: 22 pass / 0 gaps / 2 uncertain.
  All eight equivalent one-shot/prepared boundaries pass; stop passing cases.
  One shared prepared-layout body and one closed numeric source retain fixed wide/narrow
  schedules and original exact tap order, certificates and repair. Existing padding
  holds coefficients; no new owner, array or capacity. Down8 GC 199,982 B/op;
  prepared GC 137–138 B/op, wide/narrow retained plans 198,432/199,840 B.
  The same actual package verifies all four frozen byte/word wide/narrow plans,
  cancellation, prompt admission release and independent surviving outputs on both OSes.
  Its coarse governed peaks and portable/cached differences are reported separately;
  physical/native/JIT peaks remain unmeasured. Other raw/native clock movements stay
  visible; no uniform gain claim. The native-prepared asymmetry remains explicit
  alongside equivalent boundaries. Earlier quartet results retain linked history.
  [Shared narrow](../benchmarks/BOUNDED-BICUBIC-SHARED-QUARTETS.md) and
  [contiguous wide source views](../benchmarks/CONTIGUOUS-WIDE-SOURCE-VIEWS.md) keep bounded
  allocation 113 B/op and admission 4 KiB. Narrow byte upper1.027 remains a gap,
  word upper.888 passes. Wide byte 357.33/zimg350.73 µs,
  upper1.033 remains a gap after14.15µs/3.81%raw/4.01%normalized saved;
  raw/all-actual/normalized confidence separate, native+0.21%. Current word
  404.77/zimg407.07µs,
  upper1.012 is uncertain, superseding its older pass.
  Raw cost+2.35µs/.58% and normalized cost+2.51% are recorded; all-actual raw and normalized
  confidence overlap, native−1.88%. No word gain or established normalized regression.
  Same113B/op/4KiB/zero retained scratch, no new capacity or owner. Word JIT layout
  changes despite the folded guard; its complete companion was refreshed before qualification.
  [Compact mixed geometry](../benchmarks/COMPACT-MIXED-BICUBIC-GEOMETRY.md)
  uses36 rather than84 B/column. Default/bounded byte save35.35/13.27 µs,
  4.01%/1.54% raw and4.07%/4.10% normalized, with separated confidence; both pass/stop.
  Native clocks move+0.063%/+2.673%. Both word controls remain uncertain; raw/native
  clock movement and contemporary predecessor checks are explicit. GC stays114 B/op,
  scratch4/8KiB; geometry borrows36,972 rather than86,268 B of existing output,
  with no extra heap capacity or retention. Original exact arithmetic/repair remain.
  [Bounded horizontal window reuse](../benchmarks/PACKED-HORIZONTAL-WINDOW-REUSE.md)
  saves 14.21 µs/3.76% raw/3.68% normalized on byte 64×32, with separated
  confidence and native -0.08%. Byte 364.04/zimg 372.99 µs upper 0.982 passes/stops.
  Word 386.57/378.04 µs upper 1.027 remains uncertain. A matched 64-operation
  adjacent view 397.04/410.52 µs upper 0.979 passes its scope; both views remain
  visible and broader word parity stays open. No causal word gain or favorable
  repeat is claimed. Same 113 B/op (112 direct in the adjacent tool), 4 KiB
  admission and 8,192/16,384 B independently owned results; no added capacity or retention.
  [Current Lanczos3 byte confidence](../benchmarks/LANCZOS-BYTE-PRODUCT-PREPARATION.md)
  is 489.94/zimg 491.00 µs, upper 1.015: uncertain on the unchanged actual package,
  superseding the former byte gap. The eight controls now have 6 passes / 2 uncertain;
  word plans retain 156,248 B. Row-product experiments are unselected; no CPU gain
  or candidate capacity is claimed. Current byte GC 138 B/op/plan 139,800 B is unchanged.
  [Ordered word bicubic windows](../benchmarks/BICUBIC-ORDERED-HORIZONTAL-WINDOWS.md)
  saves49.41/34.44 µs on reverse-mixed/horizontal RGBA16:5.25%/6.53% raw and
  6.36%/15.36% normalized, with separated raw/all-actual/normalized confidence.
  Native clocks move+1.19%/+10.44%; both independently pass/stop, upper
  0.999/0.774,
  GC114/113 B/op. Both keep4KiB admission and existing output-backed workspace;
  no added array capacity or retention. [Identity-height history](../benchmarks/BICUBIC-IDENTITY-HEIGHT.md)
  retains the one-row design and adverse/inconclusive experiments separately.
  Other kernels reuse valid evidence; no repeated passing-case tuning.
  [Ordered bilinear horizontal windows](../benchmarks/BILINEAR-ORDERED-HORIZONTAL-WINDOWS.md)
  save28.77/32.94 µs on byte/word reduction:6.42%/7.08%raw and6.35%/6.88%normalized,
  with separated raw/all-actual and normalized confidence. Native clocks move−.072%/−.208%.
  Down16 passes/stops; Down8 is uncertain (upper1.013).
  GC stays8,473 B/op and admitted scratch12,608 B, with no added capacity or retention.
  [Earlier shared preparation](../benchmarks/BILINEAR-SHARED-PREPARATION.md) retains
  the unchanged Up8 pass. [Vector normalization/certification](../benchmarks/BILINEAR-VECTOR-CERTIFICATION.md)
  independently saves 769.82 µs/27.67% raw/18.12% normalized on existing Up16.
  Its [current adjacent complete cost](../benchmarks/WARM-ADJACENT-CONFIDENCE.md) is
  1,830.73/FFmpeg 1,884.92 µs, conservative
  0.884–1.013: **uncertain**, not a parity pass or demonstrated remaining gap.
  Measured direct GC 4,498,558 B/op includes the 4,264,104 B result; admitted capacity
  and retention are unchanged. Native-clock movement remains explicit; no favorable repeat.
  Exact managed Q30/rational/certified results remain distinct from native float.
  [Enlargement horizontal preparation/execution](../benchmarks/BICUBIC-ENLARGEMENT-HORIZONTAL-MODES.md)
  refreshes all twelve existing default/bounded byte/word full/horizontal/vertical
  controls: **11 pass / 1 uncertain**. Exact identity-width cache decoding removes
  redundant horizontal filtering; common/adjacent four-tap windows share input
  preparation without changing multiply/add order. Tight vertical byte/word save
  286.55/232.95 µs,16.42%/15.15% raw and15.98%/19.18% normalized;
  tight full word saves78.84 µs,2.91% raw/2.97% normalized. All three confidence
  views separate; native clocks move−.52%/+4.98%/+.063%. No uniform raw-gain claim.
  Tight horizontal byte 1,499.15/zimg1,521.64 µs,
  upper1.048 remains uncertain; an earlier implementation's pass is
  not current proof. GC114–116 B/op except full word4,264,573 B/op; same4KiB
  admission, no new owned capacity or retention. Prepared zimg graph/scratch remains
  explicitly distinct from complete managed Resize. Stop the eleven confirmed cases.
  [Earlier quartet costs](../benchmarks/BICUBIC-SHARED-QUARTET-EXECUTION.md) and
  unaccepted capacity trials remain history; original exact arithmetic/repair remain.
- **Deep bounded bicubic:** [shared prepared conversions](../benchmarks/PREPARED-DEEP-PAIR-CONVERSIONS.md)
  save97.90µs/23.73%raw/23.80%normalized, with separated raw/all-actual/normalized
  confidence and native+0.085%. Current32×16 byte314.66/zimg376.79µs,
  upper0.846 passes/stops. Same113B/op/4KiB, zero retained scratch;
  3,536Brow and4,032Bcolumn frames remain separate. Exact double streams,
  certificates/repair and ownership remain. Code/prologue growth is disclosed;
  peaks and pool retention remain unmeasured. Other source-valid costs are reused.
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

The current immutable build passes **6,449 full/live checks per OS**
and the **same actual Release-package consumer/all three usage examples on
Windows/Linux**. Current Huffman/VP8 checks pass1,026internal/239public per
normal/scalar/BMI2-off mode; unchanged broader VP8/shared-memory and numerical ISA
evidence is reused. Current text/canvas/ownership
checks pass183internal/162public per normal/scalar mode on each OS.
Current PNG/admission checks pass690internal/246public per
normal/scalar/disabled-AVX2/AVX512 mode.
Unchanged resize qualification retains 528 internal/791 public per
normal/scalar/disabled-AVX2/AVX512 mode. Unchanged JPEG checks pass
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
| WebP VP8 + alpha, fresh seekable Stream (parity confirmed) † | 6,196.90 | 6,487.75 | 0.955 / 0.961 | 240,496 | — |
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
| VP8L RGBA8, resident decode (confidence open) † | 3,044.22 | 3,025.72 | 1.006 / 1.035 | 152,728 | — |
| VP8L RGBA8, fresh seekable Stream decode (confidence open) † | 3,317.83 | 3,219.68 | 1.030 / 1.068 | 152,824 | — |
| VP8L palette16 RGBA8, resident decode (confirmed pass) † | 417.53 | 544.48 | 0.767 / 0.780 | 288,465 | — |
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
| Bicubic resize RGBA8 † → 256×129 | 452.28 | 454.42 | 0.995 / 1.005 | 199,982 |
| Bicubic upscale RGBA16 † → 1027×519 | 2,893.73 | 3,105.31 | 0.932 / 0.940 | 4,264,573 |
| Bicubic upscale RGBA8, default workspace † → 1027×519 | 3,113.60 | 3,291.86 | 0.946 / 0.955 | 116 |
| Bicubic RGBA16, width down / height up / input+output+4 KiB † → 256×519 | 892.36 | 900.40 | 0.991 / 0.999 | 114 |
| Bicubic RGBA8, 32×16 / input+output+4 KiB (parity confirmed) † → 32×16 | 314.66 | 376.79 | 0.835 / 0.846 | 113 |
| Bicubic RGBA8, 32×16 / default workspace † → 32×16 | 311.98 | 378.45 | 0.824 / 0.840 | 197,299 |
| Bicubic RGBA8 enlargement, input+output+4 KiB † → 1027×519 | 3,064.21 | 3,289.31 | 0.932 / 0.939 | 116 |
| Bicubic RGBA8 width enlargement, input+output+4 KiB † → 1027×259 | 1,499.15 | 1,521.64 | 0.985 / 1.048 | 114 |
| Bicubic RGBA8 height enlargement, input+output+4 KiB † → 513×519 | 1,458.80 | 1,744.79 | 0.836 / 0.845 | 114 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129 | 459.65 | 451.35 | 1.018 / 1.027 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 256×129 | 506.31 | 574.04 | 0.882 / 0.888 | 113 |
| Bicubic RGBA8, fresh managed / prepared zimg † → 128×64 | 378.51 | 375.02 | 1.009 / 1.020 | 198,450 |
| Bicubic RGBA8, width up / height down, input+output+4 KiB † → 1027×129 | 850.67 | 893.27 | 0.952 / 0.978 | 114 |
| Bicubic RGBA16, width up / height down, input+output+4 KiB (confidence open) † → 1027×129 | 923.05 | 914.24 | 1.010 / 1.016 | 114 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 64×32 | 364.04 | 372.99 | 0.976 / 0.982 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB (confidence open) † → 64×32 | 386.57 | 378.04 | 1.023 / 1.027 | 113 |
| Bicubic RGBA8 reduction, input+output+4 KiB (gap) † → 128×64 | 357.33 | 350.73 | 1.019 / 1.033 | 113 |
| Bicubic RGBA16 reduction, input+output+4 KiB (confidence open) † → 128×64 | 404.77 | 407.07 | 0.994 / 1.012 | 113 |
| Bicubic RGBA8, width down / height up, default workspace † → 256×519 | 768.30 | 840.72 | 0.914 / 0.918 | 199,947 |
| Bicubic RGBA16, width down / height up, default workspace † → 256×519 | 821.70 | 889.98 | 0.923 / 0.936 | 199,891 |
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
| Bilinear upscale RGBA8 (parity confirmed) † → 1027×519 | 2,091.11 | 2,296.62 | 0.911 / 0.916 | 102,891 |
| Bilinear resize RGBA8 (confidence open) † → 256×129 | 419.27 | 417.88 | 1.003 / 1.013 | 8,473 |
| Bilinear resize RGBA16 (parity confirmed) † → 256×129 | 432.57 | 449.05 | 0.963 / 0.974 | 8,473 |
| Bicubic RGBA16 height reduction, input+output+4 KiB (pass) † → 513×129 | 486.02 | 593.16 | 0.819 / 0.824 | 113 |
| Bicubic one-shot RGBA8 / zimg one-shot † → 128×64 | 349.70 | 989.97 | 0.353 / 0.358 | 198,455 |
| Bicubic prepared RGBA8 / zimg prepared † → 128×64 | 258.97 | 347.40 | 0.745 / 0.752 | 137 |
| Bicubic one-shot RGBA16 / zimg one-shot † → 128×64 | 397.92 | 602.90 | 0.660 / 0.694 | 198,453 |
| Bicubic prepared RGBA16 / zimg prepared † → 128×64 | 331.70 | 415.65 | 0.798 / 0.803 | 137 |
| Bicubic one-shot narrow RGBA8 / zimg one-shot † → 256×129 | 494.53 | 1,057.83 | 0.467 / 0.490 | 199,991 |
| Bicubic prepared narrow RGBA8 / zimg prepared † → 256×129 | 436.53 | 455.85 | 0.958 / 0.968 | 138 |
| Bicubic one-shot narrow RGBA16 / zimg one-shot † → 256×129 | 452.32 | 713.95 | 0.634 / 0.644 | 199,983 |
| Bicubic prepared narrow RGBA16 / zimg prepared † → 256×129 | 396.33 | 572.47 | 0.692 / 0.726 | 137 |

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
