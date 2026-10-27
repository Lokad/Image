# Benchmark state

Updated 2026-10-27. Development0.2.0 remains under qualification; warm single-CPU
parity and replacement readiness are open. Compression correctness precedes CPU
gains. Published0.1.0 remains unchanged.

Pre-cleanup costs describe their frozen producers, **not the cleaned build**.
Current independent confirmations and short screens are labeled below; other
required paths remain open. Rate-plan refresh independently measures 2.214× reference time on the existing
photograph, with exact packets. Allocations fall 11.1% to 809,056 B/op by reusing
operation-owned tables. Its 21.50 ms is 1.91% below the preceding independent mean;
no paired baseline CPU gain is claimed. Other affected encoders have current
guarded shorts; remaining required confirmations stay open.

The [complete historical inventory](benchmark-pre-cleanup.md) retains all257cases,
raw links and baseline exceptions; [coverage](../benchmarks/WARM-COVERAGE.md)
defines the remaining finite scope and evidence invalidation.

| Current qualification | Evidence | Status |
| --- | --- | --- |
| Packet/pixel/memory/cancellation regression | [Current qualification](../benchmarks/WARM-VP8-RATE-PLAN-REFRESH.md) | 4,943 checks/OS; 622 affected scalar WebP/OS and same actual package passed |
| CPU and allocation measurements after cleanup | [rate-plan refresh](../benchmarks/WARM-VP8-RATE-PLAN-REFRESH.md), [JPEG append](../benchmarks/WARM-JPEG-PAIRED-ENTROPY.md), [empty spatial histograms](../benchmarks/WARM-VP8L-EMPTY-HISTOGRAMS.md), [shared prediction rows](../benchmarks/WARM-VP8-SHARED-SMALL-ROWS.md) | Exact packets; other required paths and full parity open |
| Full warm target | Independent quiet upper≤1× for each required path, one CPU/operation | Open |

Current screens are labeled; other cost anchors remain **pre-cleanup**. B/op is managed allocation, distinct
from peak cooperative admission and native allocation. Ratios use the faster native
bracket; upper is the recorded conservative confidence bound. Historical shorts
are leads for measurement, not current demonstrated gaps.

| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [WebP VP8 + alpha, fresh seekable Stream](../benchmarks/WARM-VP8-SHARED-SMALL-ROWS.md) | 7,838.48 | 5,987.65 | 1.309 / 1.337 | 1,966,583 | Current gap (short; qualified) |
| [JPEG 4:4:4, forward-only / exact reference](../benchmarks/WARM-JPEG-PAIRED-ENTROPY.md) | 4,599.19 | 3,783.31 | 1.216 / 1.414 | 820,123 | Current gap (short; qualified) |
| [JPEG 4:2:0 from caller RGB samples](../benchmarks/WARM-JPEG-PAIRED-ENTROPY.md) | 2,432.58 | 1,576.20 | 1.543 / 1.556 | 373,413 | Current gap (independent) |
| [JPEG grayscale, standalone SIMD libjpeg-turbo](../benchmarks/WARM-JPEG-PAIRED-ENTROPY.md) | 1,585.89 | 1,405.11 | 1.129 / 1.256 | 155,436 | Current gap (short; qualified) |
| [JPEG from caller GRAY8, standalone SIMD libjpeg-turbo](../benchmarks/WARM-JPEG-PAIRED-ENTROPY.md) | 1,162.42 | 1,077.77 | 1.079 / 1.416 | 155,438 | Current gap (short; qualified) |
| [VP8 RGBA8](../benchmarks/WARM-VP8-RATE-PLAN-REFRESH.md) | 80,109.96 | 53,494.76 | 1.498 / 1.935 | 23,257,760 | Current gap (short; qualified) |
| [VP8 opaque photograph](../benchmarks/WARM-VP8-RATE-PLAN-REFRESH.md) | 21,504.39 | 9,711.21 | 2.214 / 2.222 | 809,056 | Current gap (independent) |
| [VP8 opaque text/diagram](../benchmarks/WARM-VP8-RATE-PLAN-REFRESH.md) | 12,203.50 | 5,833.47 | 2.092 / 2.276 | 740,968 | Current gap (short; qualified) |
| [VP8 diagram, effort6 / trellis](../benchmarks/WARM-VP8-RATE-PLAN-REFRESH.md) | 21,201.56 | 9,795.35 | 2.164 / 2.255 | 754,328 | Current gap (short; qualified) |
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
