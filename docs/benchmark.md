# Benchmark state

Updated2026-10-26. Development0.2.0 remains under qualification; warm single-CPU
parity and replacement readiness are open. Compression correctness precedes CPU
gains. Published0.1.0 remains unchanged.

The cleanup removes fuel, ceiling reservations and unpublished wrappers.
Pre-cleanup timings describe their frozen producers, **not the cleaned build**.
VP8 stream decoding, caller-RGB420 JPEG, VP8 opaque photograph and VP8L text/diagram have current
independent cost confirmations. Other VP8/VP8L encoding anchors have current
short screens after empty spatial histogram pruning.
Paired JPEG entropy appends use 6.7% less elapsed than the preceding quiet short,
with unchanged allocations and exact packets. Other shared-writer JPEG encoding
costs become historical; JPEG444 and the two grayscale boundaries now have current
short screens, without an independent parity claim. VP8 rate pruning and matcher
storage remain qualified;
PNG SmallestSize retains unchanged current short evidence. Exact eight-pixel
VP8 color/chroma batching reduces complete decode elapsed by21.5% versus its
preceding short, confirming a1.301× gap with unchanged1,966,583B/op; RGBA16 bilinear reduction has
a current 1.339× short gap with exact managed words. Existing VP8L photograph and
diagram controls now have a current 1.605× short and 1.362× independent gap, with
29,610,520/45,404,838 B/op. Skipping empty token-start tiles uses40.0% less diagram
elapsed than the preceding quiet short; packets remain byte exact. Older affected
encoding timings are historical. Parity remains open.
The two existing opaque VP8 photograph/diagram controls have current2.879×/2.733×
gaps with909,696/841,608 B/op and exact17,688/7,100-byte packets. These larger gaps
now take priority over smaller gains. Adjacent forward-block batching confirms
8.6% less photograph elapsed than its preceding quiet short, with unchanged
allocation and exact bytes. Packed quantization and uniform prediction reuse remain
qualified; no new input/settings matrix.
The [complete historical inventory](benchmark-pre-cleanup.md) retains all257cases,
raw links and baseline exceptions; [coverage](../benchmarks/WARM-COVERAGE.md)
defines the remaining finite scope and evidence invalidation.

| Current qualification | Evidence | Status |
| --- | --- | --- |
| Packet/pixel/memory/cancellation regression | [Current qualification](../benchmarks/WARM-VP8-ADJACENT-FORWARD.md) | 4,932 checks/OS; 611 affected scalar WebP/OS and same actual package passed |
| CPU and allocation measurements after cleanup | [VP8 decode color](../benchmarks/WARM-VP8-VECTOR-COLOR.md), [JPEG append](../benchmarks/WARM-JPEG-PAIRED-ENTROPY.md), [empty spatial histograms](../benchmarks/WARM-VP8L-EMPTY-HISTOGRAMS.md), [adjacent forward blocks](../benchmarks/WARM-VP8-ADJACENT-FORWARD.md) | Exact packets; other required paths and full parity open |
| Full warm target | Independent quiet upper≤1× for each required path, one CPU/operation | Open |

Current screens are labeled; other cost anchors remain **pre-cleanup**. B/op is managed allocation, distinct
from peak cooperative admission and native allocation. Ratios use the faster native
bracket; upper is the recorded conservative confidence bound. Historical shorts
are leads for measurement, not current demonstrated gaps.

| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [WebP VP8 + alpha, fresh seekable Stream](../benchmarks/WARM-VP8-VECTOR-COLOR.md) | 7,784.47 | 5,983.28 | 1.301 / 1.308 | 1,966,583 | Current gap (independent) |
| [JPEG 4:4:4, forward-only / exact reference](../benchmarks/WARM-JPEG-PAIRED-ENTROPY.md) | 4,599.19 | 3,783.31 | 1.216 / 1.414 | 820,123 | Current gap (short; qualified) |
| [JPEG 4:2:0 from caller RGB samples](../benchmarks/WARM-JPEG-PAIRED-ENTROPY.md) | 2,432.58 | 1,576.20 | 1.543 / 1.556 | 373,413 | Current gap (independent) |
| [JPEG grayscale, standalone SIMD libjpeg-turbo](../benchmarks/WARM-JPEG-PAIRED-ENTROPY.md) | 1,585.89 | 1,405.11 | 1.129 / 1.256 | 155,436 | Current gap (short; qualified) |
| [JPEG from caller GRAY8, standalone SIMD libjpeg-turbo](../benchmarks/WARM-JPEG-PAIRED-ENTROPY.md) | 1,162.42 | 1,077.77 | 1.079 / 1.416 | 155,438 | Current gap (short; qualified) |
| [VP8 RGBA8](../benchmarks/WARM-VP8-ADJACENT-FORWARD.md) | 86,847.48 | 53,412.61 | 1.626 / 1.733 | 23,358,400 | Current gap (short; qualified) |
| [VP8 opaque photograph](../benchmarks/WARM-VP8-ADJACENT-FORWARD.md) | 27,777.10 | 9,647.05 | 2.879 / 2.900 | 909,696 | Current gap (independent) |
| [VP8 opaque text/diagram](../benchmarks/WARM-VP8-ADJACENT-FORWARD.md) | 15,927.54 | 5,826.94 | 2.733 / 2.762 | 841,608 | Current gap (short; qualified) |
| [VP8L RGBA8 / libwebp exact=1](../benchmarks/WARM-VP8L-EMPTY-HISTOGRAMS.md) | 191,649.39 | 122,403.16 | 1.566 / 1.827 | 47,674,928 | Current gap (short; qualified) |
| [VP8L photograph / libwebp exact=1](../benchmarks/WARM-VP8L-EMPTY-HISTOGRAMS.md) | 133,800.97 | 83,340.00 | 1.605 / 2.159 | 29,610,520 | Current gap (short; qualified) |
| [VP8L text/diagram / libwebp exact=1](../benchmarks/WARM-VP8L-EMPTY-HISTOGRAMS.md) | 13,192.85 | 9,685.69 | 1.362 / 1.408 | 45,404,838 | Current gap (independent) |
| [PNG RGBA8, SmallestSize9 / native9](../benchmarks/WARM-CURRENT-ENCODER-ANCHORS.md) | 24,517.23 | 17,302.12 | 1.417 / 1.498 | 1,325,670 | Current gap (short; qualified) |
| [Bilinear resize RGBA16](../benchmarks/WARM-CURRENT-EDITING-ANCHORS.md) | 547.29 | 408.78 | 1.339 / 1.426 | 290,124 | Current gap (short; qualified) |
| [Bicubic resize RGBA8](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 635.08 | 416.65 | 1.524 / 1.610 | 332,178 | Historical short; remeasure |
| [Bicubic upscale RGBA16](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 4,258.22 | 2,871.12 | 1.483 / 1.690 | 5,016,578 | Historical short; remeasure |
| [Lanczos3 reduction RGBA8](../benchmarks/WARM-LANCZOS3-CERTIFIED-FMA.md) | 758.75 | 490.07 | 1.548 / 1.646 | 138,466 | Historical short; remeasure |
| [Lanczos3 enlargement RGBA16](../benchmarks/WARM-LANCZOS3-CERTIFIED-FMA.md) | 5,248.30 | 3,935.96 | 1.333 / 1.459 | 4,584,270 | Historical short; remeasure |

Complete encoder packets remain the acceptance boundary; metadata differences
are limited to the approved JPEG attribution COM. These are the named controls,
not broad arbitrary-content/settings compression claims.

| Frozen packet scope | Lokad.Image bytes | Reference bytes | Verification |
| --- | ---: | ---: | --- |
| PNG,14depth/color/policy controls | Identical | Identical | Complete raw packets |
| JPEG RGBA8,444 /422 /420 | 133039 /71166 /52091 | 133041 /71168 /52093 | Exact except approved COM |
| JPEG gray8, Q69 | 58910 | 58910 | Complete raw packet; SIMD libjpeg-turbo |
| VP8L photograph / diagram | 160032 /7234 | 160032 /7234 | Exact=1; complete raw packets |
| VP8 photograph / diagram, Q75/method4 | 17688 /7100 | 17688 /7100 | Complete raw packets |

[Codec parity](codec-parity.md) records the full packet identities and producer
scope: JPEG10controls, PNG14, VP8L19selected+2forced, VP8 twenty513×259identities
plus the large retry control. Decoder exactness and editing oracle requirements
remain separate. A timing pass,5%IDAT tolerance or equal numeric qualities cannot
replace packet equality.

Reference operations include normalization, input delivery and independent output
ownership. FFmpeg is pinned; grayscale encoding uses the approved explicitly
labeled SIMD libjpeg-turbo3.1.4.1 reference; VP8L uses libwebp exact=1; bicubic uses
zimg, FFmpeg's zscale backend, with the approved full halo/alpha/packing boundary.
Native-only resize tolerances remain in the retained reports; managed pixels
stay exact. GIF/animation, startup, concurrency and arbitrary profiles are excluded.

Use the exclusive quiet VM, versioned idle/process guard and sequential Release
jobs. Screen narrowly; independently confirm promising cases and stop passing
ones. Reuse immutable builds and unchanged qualification. Historical experiments,
failed guards and rejected candidates stay retained; none silently becomes current
evidence. Regenerate with `python benchmarks/render-benchmark.py`, then `--check`.
