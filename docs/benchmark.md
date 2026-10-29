# Lokad.Image benchmarks

Updated 2026-10-29. **† Rows** use qualified 0.2.1 development-path evidence;
**‡ stored PNG** and Fastest PNG retain their applicable earlier evidence;
SmallestSize and predictive PNG are refreshed after bounded match scanning.
Seven other controls retain
published 0.2.0 measurements on unchanged paths. VP8 Stream decode now has a
current-cost screen. The release snapshot remains immutable. Eighteen read
controls below use qualified 0.2.1 builds. Default JPEG420 decode has independent
current evidence; VP8L resident decode is independently confirmed and Stream retains a current screen. These 53 displayed controls do not
complete the full CPU-parity objective
or establish general replacement readiness.

Measurements use the dedicated quiet Ubuntu 24.04 x64 VM
(AMD EPYC 9V45 96-Core Processor): one pinned CPU per operation, warm .NET 10,
sequential Release execution, no concurrency. This representative scope is not
the full performance objective.

Predictive/stored PNG16, Fastest PNG8, caller-RGB420 JPEG, JPEG420/VP8L resident decode, VP8 photograph/effort6 diagram, VP8L RGBA8/16/photograph encode and bicubic Up16/TightCross16 and all four Lanczos3 controls have 12 measured iterations each;
other codec/editing rows are short BenchmarkDotNet screens with three. Native brackets and
idle/process guards pass. Predictive PNG16 uses four complete sequential operations
per iteration/native sample to resolve execution-rate shifts; costs are per operation.
Shared-history predictor pricing reduces VP8L RGBA8
elapsed by 7.4% against a fresh independent baseline, preserving exact packets.
It remains a1.332× / upper1.360 gap. Allocation rises by8,224B/op for one reusable
8KiB analysis buffer. [RGBA16 confirmation](../benchmarks/CURRENT-VP8L-RGBA16-COST.md)
resolves the wide screen at1.341× / upper1.431,29,696,928B/op and exact183,812B
packets; this makes no causal comparison against older screens.
[Photograph confirmation](../benchmarks/VP8L-SHARED-COLOR-MASK-REJECTED.md) gives
the current unchanged-path cost; its mask-sharing trial is rejected. Async
retains a current screen.
Horizontal JPEG chroma filtering independently reduces caller-RGB420 elapsed by
16.1%, retaining373,416B/op and the approved exact packet boundary. Its1.283× /
upper1.294 gap remains; governed RGBA16 has a current1.224× screen.
[Quantization caller invariants](../benchmarks/WARM-VP8-QUANTIZATION-CALLER-INVARIANTS.md)
reduce VP8 photograph elapsed by 6.93% filtered / 6.90% all actuals against a fresh
independent baseline: 1.433× / upper 1.438, unchanged 809,072 B/op and exact
17,688-byte packet. All six affected displayed encodes have current costs;
the five companions are short screens and remain gaps, with unchanged allocations
and packets. Unmatched screens establish no causal gain or regression.
Other affected method0–6 boundaries need applicable current costs before CPU-pass claims.
Earlier input preparation and trellis gains remain in the linked history.
[Current effort-6 diagram](../benchmarks/CURRENT-VP8-EFFORT6-COST.md) independently
confirms1.401× / upper1.415,754,344B/op and an unchanged exact6,818Bpacket.
It remains a gap; this unchanged-path confirmation makes no causal comparison
against older short screens. Zero builds or repeated qualification.
The current producer passes 5,132 checks on each platform, plus 122 affected kernel
and 258 public checks normal/scalar per OS, and the same actual package/three examples.
Prior scalar qualification remains applicable to unchanged kernels; exact pixels,
encoder packets and shared ownership remain.
Ratio uses the faster native bracket; upper is the conservative confidence bound
including unfiltered managed samples. Short screens are not new independent
parity confirmations. [Byte zero-tap trimming](../benchmarks/WARM-BICUBIC-ZERO-TAPS.md)
reduces HorizontalUp8 elapsed by16.2% filtered/16.1% all actuals in an independent
pair:1.171× / upper1.193, unchanged1,930,356B/op. All six changed byte paths have
current costs; Down8 now has a1.374× / upper1.441 screen,332,188B/op. This screen
makes no gain/regression claim. Horizontal8 confirms upper1.008, still a gap.
[Shared word publication](../benchmarks/WARM-BICUBIC-SHARED-WORD-PUBLICATION.md)
reduces Up16 elapsed by 15.1% in a fresh independent pair, retaining exact pixels:
1.221× / upper 1.234, 5,016,602 B/op (measured +8 B/op; no new owned buffer).
Its VerticalUp16 companion independently passes at 0.964× / upper 0.973,
2,516,728 B/op; stop this unchanged case. All five affected word controls have
current costs. [Output-resident rows](../benchmarks/WARM-BICUBIC-OUTPUT-ROW-CACHE.md)
reduce TightCross16 elapsed26.2–29.3% in both counterbalanced time orders; all raw
and reference-normalized confidence gates pass. Current conservative cost is
1.232× / upper1.396,1,063,419B/op. Existing private output supplies workspace,
with no added owned capacity; full5,080checks/OS and SAME actual package/three
examples pass. The identity-height companion has a current short gap.
The [cached-path reconciliation](../benchmarks/CURRENT-BICUBIC-CACHED-COSTS.md)
retains mixed-axis costs and exact-half qualifications; byte zero-tap trimming
and shared word publication supersede its other rows with current results.
[Lanczos3 byte reduction](../benchmarks/CURRENT-LANCZOS3-REDUCE8-COST.md)
now independently confirms1.500× / upper1.511,138,451B/op on unchanged production.
Exact pixels and the same native quality/ownership gates pass; one small launcher
build reuses qualified library/harness/worker and package proof.
[Word reduction](../benchmarks/CURRENT-LANCZOS3-REDUCE16-COST.md) independently
confirms1.585× / upper1.666,138,439B/op, with the same exact managed/native-bound
gates and unchanged-path qualification.
[Byte enlargement](../benchmarks/CURRENT-LANCZOS3-ENLARGE8-COST.md) independently
confirms1.033× / upper1.038,319,038B/op.
[Word enlargement](../benchmarks/CURRENT-LANCZOS3-ENLARGE16-COST.md) confirms
1.267× / upper1.453,4,583,983B/op. Both reuse cached builds/qualification. All
four Lanczos controls have current independent confirmations; none implies a
causal comparison against older screens or a general parity claim. Exact/native-
bound gates and operation boundaries remain fixed.
B/op measures managed allocation, not native
allocation, peak live memory or cooperative-admission charges.

Operations include normalization, input delivery and independently owned output;
async encoding includes staging/delivery/release. Fixture preparation is excluded.
Source images are 513×259 except the named 3000² predictive PNG; resize rows state output dimensions. The
[release evidence](../benchmarks/release-0.2.0-benchmark-evidence.json) and
[stored publication](../benchmarks/WARM-PNG-BULK-STORED.md) and
[distance mapping](../benchmarks/WARM-PNG-DISTANCE-SYMBOL.md) record exact dimensions,
settings, boundaries, binaries, samples, package checks and retained raw archives.
[SIMD VP8 input preparation](../benchmarks/WARM-VP8-SIMD-INPUT.md) supplies all six
VP8 encoding rows, retaining earlier observations and uncertainty; the
[trellis follow-up](../benchmarks/current-vp8-trellis-encoding-evidence.json)
and [photograph follow-up](../benchmarks/current-vp8-photograph-encoding-evidence.json)
and [alpha follow-up](../benchmarks/current-vp8-alpha8-encoding-evidence.json)
resolve their wide screens without rebuilding or repeating qualification.
[shared-history predictor pricing](../benchmarks/WARM-VP8L-PREPARED-PREDICTION-COST.md) supplies the
four affected VP8L rows. Other paths retain applicable prior proof.
[JPEG area filtering](../benchmarks/WARM-JPEG-HORIZONTAL-AREA-FILTER.md) refreshes
caller RGB420 and governed RGBA16; other subsampled encoder costs need current
evidence before a pass claim. Grayscale/4:4:4 encoding paths are unchanged.
The [earlier read reconciliation](../benchmarks/WARM-READ-RECONCILIATION.md)
retains prior VP8 Stream decode. The four tight PNG decode controls (byte/word,
ordinary/live token) need current CPU evidence after shared scan-CRC reuse;
their frozen ceilings, one-byte-below refusals and exact pixels remain qualified.
Earlier CPU costs remain in [retained evidence](../benchmarks/current-png-vp8-reconciliation-evidence.json).
The [default decoder reconciliation](../benchmarks/CURRENT-DEFAULT-DECODERS.md)
retains the VP8L Stream screen at1.304×,1,216,190B/op. A
[resident confirmation](../benchmarks/current-vp8l-resident-decode-evidence.json)
resolves its wide screen at1.333× / upper1.404,1,216,162B/op,with the same
immutable build and exact pixels;no gain/regression or general decode claim follows.
[Paired JPEG inverse columns](../benchmarks/WARM-JPEG-PAIRED-COLUMNS.md) reduce
JPEG420 decode from 1.331 ms to 1.298 ms: 2.5% less filtered elapsed and 2.4% using
all actuals, with separated intervals and unchanged 757,319 B/op. Exact pixels
remain; the 1.285× / upper 1.304 gap stays open. Both platforms pass 5,056 full
checks, affected scalar tests and the same actual package/three examples. Other
JPEG inverse-render costs need current evidence before pass claims; validation,
encoding and unrelated paths retain applicable proof. Packet/settings and owned
output boundaries stay fixed. VP8 companions are now independently confirmed;
PNG Fastest retains its applicable independent gap.

[Predictable-content PNG](../benchmarks/PNG-PREDICTABLE-COMPRESSION.md) reduces the
3000² RGBA16 gradient from29,375,876 to511,771 bytes; all six tested orientations
remain below574KB with every sample exact. BothOS5,063 checks, affected scalar
checks and the same actual package/three examples pass. Four displayed encoder
costs are refreshed below with unchanged exact packets; each remains a CPU gap.
[Bounded word matching](../benchmarks/WARM-PNG-WORD-MATCH.md) halves predictive PNG
elapsed in a fresh matched pair, with unchanged allocation and exact573,041-byte
packets. A variance-controlled confirmation establishes0.941× / upper0.979; stop
this control unchanged. Batched3,402,572B/op amortizes measurement overhead and
does not establish an allocation reduction. BothOS5,070 checks, affected scalar
and the same actual package/three examples pass. SmallestSize8/16 have current
1.330×/1.324× screens; no paired gain/regression claim follows. Other affected
level6/9 PNG encoder costs need current evidence. Fastest/stored, decoding,
other-codec and editing paths retain applicable proofs.
[Closed coefficient trials](../benchmarks/WARM-VP8-COEFFICIENT-TRIALS.md) retain
rejected VP8 experiments; their costs do not replace accepted-production evidence.


| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Packet B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| WebP VP8 + alpha, fresh seekable Stream † | 7,638.57 | 5,995.99 | 1.274 / 2.005 | 1,966,583 | — |
| JPEG 4:4:4, forward-only / exact reference | 4,597.26 | 3,766.58 | 1.221 / 1.407 | 820,123 | 133,039 / 133,041 |
| JPEG 4:2:0 from caller RGB samples † | 2,030.36 | 1,582.17 | 1.283 / 1.294 | 373,416 | 52,091 / 52,093 |
| JPEG RGBA16 4:2:0 / caller IMemoryBudget † | 2,251.99 | 1,839.51 | 1.224 / 1.446 | 804,644 | 52,115 / 52,117 |
| JPEG grayscale, standalone SIMD libjpeg-turbo | 1,594.51 | 1,401.06 | 1.138 / 1.298 | 155,436 | 58,910 / 58,910 |
| JPEG from caller GRAY8, standalone SIMD libjpeg-turbo | 1,155.59 | 1,075.36 | 1.075 / 1.561 | 155,438 | 58,910 / 58,910 |
| VP8 diagram, effort2 / distortion-only search † | 3,331.69 | 2,509.72 | 1.328 / 1.463 | 741,375 | 7,330 / 7,330 |
| VP8 RGBA8 † | 70,231.82 | 53,675.16 | 1.308 / 1.382 | 21,353,208 | 98,590 / 98,590 |
| VP8 opaque photograph † | 13,899.64 | 9,701.62 | 1.433 / 1.438 | 809,072 | 17,688 / 17,688 |
| VP8 opaque text/diagram † | 7,750.98 | 5,823.99 | 1.331 / 1.478 | 741,137 | 7,100 / 7,100 |
| VP8 diagram, effort6 / trellis † | 15,835.32 | 11,306.07 | 1.401 / 1.415 | 754,344 | 6,818 / 6,818 |
| VP8 opaque RGBA16, including normalization † | 20,535.94 | 15,580.45 | 1.318 / 1.375 | 1,201,552 | 72,380 / 72,380 |
| VP8L RGBA8 / libwebp exact=1 † | 161,623.34 | 121,357.51 | 1.332 / 1.360 | 29,696,928 | 183,812 / 183,812 |
| VP8L RGBA16, including normalization / libwebp exact=1 † | 185,381.86 | 138,254.62 | 1.341 / 1.431 | 29,696,928 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 162,750.37 | 120,441.78 | 1.351 / 1.431 | 29,697,312 | 183,812 / 183,812 |
| VP8L photograph / libwebp exact=1 † | 121,047.71 | 89,021.62 | 1.360 / 1.397 | 20,418,440 | 160,032 / 160,032 |
| VP8L text/diagram / libwebp exact=1 | 11,087.40 | 9,649.39 | 1.149 / 1.709 | 27,419,167 | 7,234 / 7,234 |
| PNG RGBA8, SmallestSize9 / native9 † | 25,562.15 | 19,219.49 | 1.330 / 1.966 | 1,325,670 | 210,973 / 210,973 |
| PNG RGBA16, SmallestSize9 / native9 † | 29,650.63 | 22,398.19 | 1.324 / 1.733 | 3,813,679 | 963,202 / 963,202 |
| PNG RGBA8, Fastest1 / native1 † | 6,321.82 | 4,366.98 | 1.448 / 1.460 | 1,344,453 | 236,828 / 236,828 |
| PNG RGBA16, NoCompression0 / native0 ‡ | 443.91 | 421.09 | 1.054 / 1.090 | 1,741,712 | 1,066,552 / 1,066,552 |
| PNG predictable RGBA16 3000², Optimal6 / mixed6 † | 383,214.38 | 407,274.63 | 0.941 / 0.979 | 3,402,572 | 573,041 / 573,041 |
| JPEG baseline420, resident decode † | 1,297.81 | 1,009.85 | 1.285 / 1.304 | 757,319 | — |
| VP8L RGBA8, resident decode † | 4,583.44 | 3,438.40 | 1.333 / 1.404 | 1,216,162 | — |
| VP8L RGBA8, fresh seekable Stream decode † | 4,462.61 | 3,422.65 | 1.304 / 1.578 | 1,216,190 | — |

Packets pass existing complete-byte gates; color JPEG differs only by the approved
two-byte-smaller attribution COM. Lossless pixels, lossy reconstruction and alpha
gates remain enforced. [Codec parity](codec-parity.md) gives the broader frozen
control scope. These measurements make no arbitrary-content compression claim.

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Bilinear resize RGBA16 → 256×129 | 543.06 | 417.56 | 1.301 / 1.411 | 290,128 |
| Bicubic resize RGBA8 † → 256×129 | 572.97 | 417.08 | 1.374 / 1.441 | 332,188 |
| Bicubic upscale RGBA16 † → 1027×519 | 3,513.80 | 2,878.92 | 1.221 / 1.234 | 5,016,602 |
| Bicubic RGBA16, width down / height up / input+output+4 KiB † → 256×519 | 1,040.15 | 844.30 | 1.232 / 1.396 | 1,063,419 |
| Bicubic RGBA8, 32×16 / input+output+4 KiB → 32×16 | 464.68 | 375.02 | 1.239 / 1.370 | 2,185 |
| Bicubic RGBA8, 32×16 / default workspace → 32×16 | 363.67 | 375.44 | 0.969 / 1.174 | 199,372 |
| Lanczos3 reduction RGBA8 † → 256×129 | 735.82 | 490.45 | 1.500 / 1.511 | 138,451 |
| Lanczos3 reduction RGBA16 † → 256×129 | 802.46 | 506.27 | 1.585 / 1.666 | 138,439 |
| Lanczos3 enlargement RGBA8 † → 1027×519 | 4,198.93 | 4,066.37 | 1.033 / 1.038 | 319,038 |
| Lanczos3 enlargement RGBA16 † → 1027×519 | 5,399.64 | 4,260.84 | 1.267 / 1.453 | 4,583,983 |

Resize references use FFmpeg or its explicitly labeled zimg/zscale backend with
the approved halo, alpha and packing boundary. Managed pixels stay exact; native
rounding tolerances remain the frozen per-case bounds in the evidence. Tight
bicubic rows allow only input + output + 4 KiB local workspace.

References are pinned FFmpeg n8.1 and its libwebp 1.6.0 backend. Grayscale JPEG
uses the approved standalone **SIMD libjpeg-turbo 3.1.4.1** exception; lossless
WebP uses **libwebp exact=1** to preserve hidden RGB. Defaults/quality numbers
across encoders are not assumed equivalent. Startup, concurrency, GIF/animation
and unlisted content/settings are excluded.

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Probe PNG dimensions, seekable | 0.156 | 0.123 | 1.263 / 1.956 | 744 | Screen gap |
| Probe JPEG dimensions, seekable | 0.381 | 0.192 | 1.988 / 2.031 | 744 | Confirmed gap |
| Probe WebP dimensions, seekable | 0.159 | 0.123 | 1.289 / 1.550 | 744 | Screen gap |
| Probe JPEG dimensions, async nonseekable | 0.425 | 0.201 | 2.119 / 2.154 | 744 | Confirmed gap |
| Validate PNG8, span | 1,425.169 | 1,446.042 | 0.986 / 1.020 | 240 | Confidence open |
| Validate PNG16, span | 2,943.210 | 3,119.778 | 0.943 / 0.955 | 240 | Confirmed pass |
| Validate indexed Adam7 19×17, span | 5.320 | 3.915 | 1.359 / 1.369 | 1,168 | Confirmed gap |
| Validate JPEG baseline420, span | 620.209 | 676.122 | 0.917 / 0.935 | 9,984 | Confirmed pass |
| Validate JPEG progressive420, span | 277.509 | 263.457 | 1.053 / 1.063 | 4,072 | Confirmed gap |
| Validate VP8L+alpha, span | 4,423.742 | 3,218.514 | 1.374 / 1.648 | 685,390 | Screen gap |
| Validate VP8+alpha, span | 7,367.434 | 5,452.572 | 1.351 / 1.382 | 770,092 | Confirmed gap |
| Validate PNG16, async nonseekable | 2,984.361 | 3,678.840 | 0.811 / 0.843 | 8,544 | Confirmed pass |
| Inspect PNG8 complete headers, seekable, adapter reference | 0.805 | 0.590 | 1.365 / 1.603 | 8,528 | Screen gap |
| Identify PNG8 complete container, resident, adapter reference | 16.133 | 13.073 | 1.234 / 1.312 | 96 | Screen gap |
| Identify JPEG420, resident, canonical adapter | 0.501 | 1.193 | 0.420 / 0.426 | 96 | Confirmed pass |
| Identify VP8L+alpha, resident, parser adapter | 0.022 | 4.985 | 0.004 / 0.005 | 96 | Confirmed pass |
| Identify PNG8, fresh seekable Stream, adapter reference | 22.674 | 15.915 | 1.425 / 1.535 | 192 | Screen gap |
| Identify PNG8, async nonseekable, adapter reference | 26.586 | 21.477 | 1.238 / 1.271 | 8,400 | Confirmed gap |

Probes compare requested dimensions using FFmpeg AVIO/owned descriptors plus an
original bounded header adapter; they stop before pixel payloads. Validation
compares complete coding and source metadata against FFmpeg natural-frame decode
plus an original metadata adapter, without RGBA normalization or caller raster
copies. Async uses resident immediate callbacks and includes acquisition/EOF and
cleanup. Inputs are 513×259 except indexed Adam7. Baseline420, PNG16 span and async
validation pass independently; PNG8 and progressive420 remain confirmed gaps. VP8L and indexed
validation rows are short screens, without independent parity claims.
[Resident VP8 confirmations](../benchmarks/CURRENT-VP8-DECODER-COSTS.md) refresh
the four existing opaque ordinary/live-token, simple-filter and VP8+ALPH controls:
1.170/1.186/1.253/1.246×, conservative uppers1.210/1.193/1.280/1.273.
Allocations are760,564/760,564/760,513/1,966,487B/op, with exact frozen pixels.
All remain gaps. Zero builds; current qualification is reused. No causal
comparison against older costs or new workload follows.
[Direct VP8 token decisions](../benchmarks/WARM-VP8-DIRECT-TOKEN-DECISIONS.md)
reduce validation elapsed by 7.20–8.12% in both counterbalanced time directions,
with separate raw and reference-normalized confidence envelopes. Current cost
is 1.351× / upper 1.382, unchanged 770,092 B/op. The stream companion is a current
1.274× / upper 2.005 short gap, unchanged 1,966,583 B/op. Exact descriptor/pixels,
source/owners/quiet gates, 5,082 full checks per OS, affected scalar checks and
the SAME actual Release package/three examples pass. Other affected VP8 decoder
costs need current evidence before pass claims; metadata-only reads, other codecs
and editing retain applicable unchanged proof.

[Paeth source layouts](../benchmarks/CURRENT-PNG-PAETH-SOURCE-COSTS.md) pass for
gray8/gray-alpha8/RGB16 at uppers0.960/0.941/0.877.
[RGB8 SIMD reconstruction](../benchmarks/WARM-PNG-RGB8-PAETH-SIMD.md) supersedes
its gap:19.2%less elapsed,0.880× / upper0.882,unchanged3,448B/op;passandstop.
All12actuals/pixels/owners/quiet pass; fullbothOS/live/scalar/SAMEpackage qualify.

[Color-key transparency](../benchmarks/CURRENT-PNG-TRANSPARENCY-COSTS.md):
Gray8tRNS 0.443/upper0.446 pass; Gray16tRNS priorgap supersededbydirectrow0.341/upper0.342 pass; Rgb8tRNS 0.845/upper0.852 pass; Rgb16tRNS 0.700/upper0.705 pass.
Exactpixels/owners/native/quiet pass;zero builds or repeatedqualification.

[Direct gray16 color-key expansion](../benchmarks/WARM-PNG-GRAY16-KEY-ROW.md)
removes generic pixelmapping:76.2%less elapsed,0.341× / upper0.342,unchanged
2,456B/op;passandstop. FullbothOS/live/normal+scalar/SAMEpackage qualify.

[Palette boundaries](../benchmarks/CURRENT-PNG-PALETTE-BOUNDARY-COSTS.md):
Palette8, partial alpha 0.902/upper 0.911 pass; Palette8, 16 entries 1.173/upper 1.180 gap; Adam7 palette8 1.274/upper 1.283 gap; Adam7 palette4 1.250/upper 1.251 gap.
Exact pixels, owners, native and quiet gates pass; zero builds or repeated qualification.

[Policies and packed keys](../benchmarks/CURRENT-PNG-POLICY-PACKED-KEY-COSTS.md)
resolve eight more costs. Pass and stop: Gray2, key0, Gray1, key1, RGBA16, cancelable. Remaining: RGBA16, tight 0.991 / upper 1.002 confidence open; RGBA16, tight + cancelable now passes upper 0.984; RGBA8, tight 1.071 / upper 1.076 gap; RGBA8, tight + cancelable now 1.094 / upper 1.107 gap; RGBA8, cancelable 1.140 / upper 1.143 gap.
All actuals, pixels, owners, policies, native and quiet gates pass; zero builds.

[Bounded cancelable row cursors](../benchmarks/WARM-PNG-CANCELABLE-ROW-CURSOR.md)
reduce tight+cancelable PNG8 elapsed by 21.0%, with unchanged 68,224 B/op;
1.094× / upper 1.107 remains a gap. The word companion passes upper 0.984,
70,272 B/op. Exact pixels, cancellation, admission, owners and quiet gates pass;
full 5,132 checks/OS, affected normal/scalar and same actual package qualify.

[Stream and composition costs](../benchmarks/CURRENT-PNG-STREAM-COMPOSITION-COSTS.md)
finish the planned PNG refresh: seekable PNG16 passes at upper 0.958, 8,664 B/op.
Byte seekable/composed, word composed and async nonseekable remain gaps at
1.112/1.147/1.124/1.075×; allocations 4,568/701,264/1,401,346/12,776 B/op.
Exact pixels, input delivery, owners, native and quiet gates pass. One untimed
word-input harness fix; no production rebuild or repeated completed timings.
[PNG coverage](../benchmarks/WARM-COVERAGE.md) has no unrefreshed named costs;
gaps and confidence-open cases remain. The full 268-path objective stays active.

[Adam7 framed-output reuse](../benchmarks/WARM-PNG-ADAM7-FRAMED-OUTPUT.md) removes
duplicate inflation: 43.8% less elapsed, 0.758× / upper 0.761, 23,240 B/op; pass.
It admits 1,063,423 extra bytes of filtered capacity in the existing bounded pool;
shared-root and tight-ceiling paths retain their prior storage requirements.
[Average SIMD](../benchmarks/WARM-PNG-AVERAGE-SIMD.md) improves RGBA8/16 by
20.3%/22.5%, with uppers 0.916/0.918 and unchanged 4,472/8,568 B/op; both pass.

[Word expansion](../benchmarks/CURRENT-PNG-WORD-DECODE-COSTS.md) RGB16/gray-alpha16/
gray16 passes at uppers 0.996/0.851/0.739, allocated 6,520/4,472/2,424 B/op.
[Byte expansion and stored RGBA8](../benchmarks/CURRENT-PNG-BYTE-DECODE-COSTS.md)
pass at uppers 0.721/0.655/0.916/0.748, allocated 3,448/2,424/1,400/4,472 B/op.
[Sub/Up](../benchmarks/CURRENT-PNG-SUB-UP-COSTS.md) also pass at both depths;
their allocated costs are 4,472/8,568 B/op. Each named control has 12 actuals,
frozen inputs/settings, exact pixels, independent outputs and quiet guards.
[Indexed and packed controls](../benchmarks/CURRENT-PNG-INDEXED-PACKED-COSTS.md)
add seven passes: palette8/4/2/1 and gray1/2/4, allocated 504–2,472 B/op. Adam7
Gray1 remains a 1.083× / upper 1.094 gap, 1,440 B/op. All eight reuse the same
build, selector and qualification; no new images, cases or retiming.

[Two retained PNG outputs under one caller root](../benchmarks/WARM-PNG-CALLER-ROOT-ROWS.md)
remain a 1.201× / upper 1.204 gap, 1,638,183 B/op and 1,766,641-byte admitted peak.
[Resident None/Paeth costs](../benchmarks/CURRENT-PNG-RGBA-DECODE-COSTS.md) retain
a None RGBA8 gap at 1.131× / upper 1.166, 4,472 B/op; None RGBA16 and Paeth8
remain confidence-open at uppers 1.006/1.031, 8,568/4,472 B/op. Paeth16 passes.
The validation table retains unchanged PNG16 span/async passes, PNG8 uncertainty
and the refreshed indexed Adam7 gap. No retiming or pass claims from means alone.

Current production passes 5,094 tests per OS, 11 kernel and 231 public checks
normal/scalar, and the SAME actual package/three examples. Each accepted change
reuses one frozen core build per OS; these PNG rounds add no harness/native/
selector builds. Subsequent current-cost checks reuse qualification, with no new
benchmark images, cases or settings. All raw observations and confidence views remain.
JPEG probing has twelve-actual gap
confirmations; PNG/WebP probing remains screened. The
[workspace reduction](../benchmarks/WARM-PROBE-WORKSPACE.md) lowers all four probes
from 8,680 to 744 B/op; later-fact/header scans retain their larger workspace.
[Completed acquisition](../benchmarks/WARM-PROBE-COMPLETED-READS.md) independently
reduces JPEG probing elapsed by 46.5% sync / 50.6% async, with unchanged allocation.
See [read boundaries and raw evidence](../benchmarks/WARM-IMAGE-READ.md).
Complete PNG header inspection uses the explicitly labeled bounded adapter,
checking structure/Strict EXIF and all nine source fields through EOF while
skipping IDAT. CRC and pixels are opaque on both sides. Its 21 frozen RGBA8
controls pass; the timing remains a screen gap, distinct from prefix probing.
[Resident PNG Identify](../benchmarks/CURRENT-PNG-RESIDENT-IDENTIFY.md) checks the
complete 218,775B container, including CRC and all nine metadata fields, without
pixel decoding. Its 21 existing controls pass normal/scalar; the timing is a
current screen gap.
[Resident JPEG/WebP Identify](../benchmarks/CURRENT-JPEG-WEBP-RESIDENT-IDENTIFY.md)
passes independently with all nine metadata facts,96B/op each. JPEG retains the
explicit canonical-header adapter; WebP includes public-parser packet buffering
and EOF flush. These frozen packet results are not general codec speed claims.
[PNG stream Identify](../benchmarks/CURRENT-PNG-STREAM-IDENTIFY.md) retains complete
acquisition/EOF/ownership and all nine facts. Seekable is a screen gap; async has
one independent gap confirmation resolving its wide short interval. Existing
same-packet/delivery controls pass normal/scalar. Other default decoder boundaries
remain pending; these results do not imply a general read-API pass.

Read allocations use the default null shared budget and warm permitted private
pools. [Governed pixel-validation storage](../benchmarks/PIXEL-VALIDATION-STORAGE.md)
separately compares allocations/admitted peaks with decode-and-dispose on five
declared controls; these figures measure different ownership/accounting settings.

[Measurement instructions](../benchmarks/README.md) and the active
[finite coverage objective](../benchmarks/WARM-COVERAGE.md) retain reproducibility
and outstanding work. Rebuild this summary with
`python benchmarks/render-benchmark.py`; verify it with `--check`.
