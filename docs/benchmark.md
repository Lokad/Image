# Benchmark state

Updated 2026-10-27. Development0.2.0 remains under qualification; warm single-CPU
parity and replacement readiness are open. Compression correctness precedes CPU
gains. Published0.1.0 remains unchanged.

Pre-cleanup costs describe their frozen producers, **not the cleaned build**.
Current independent confirmations and short screens are labeled below; other
required paths remain open. The previously unmeasured distortion-only VP8 search
exposed a 3.615× gap. After exact metrics, caller reuse and opaque chroma SIMD,
it independently measures 1.720× (upper 1.734×), 741,342 B/op and an exact
7,330-byte packet. Its 4.339 ms is 11.43% below the preceding independent mean;
these separate runs are not a paired baseline gain. Six affected VP8 anchors are
reconciled. Photograph confirmation resolves its wide short: 1.755× (upper 1.774×),
now the largest confirmed current anchor. Required upper ≤1× stays open.

The [complete historical inventory](benchmark-pre-cleanup.md) retains all257cases,
raw links and baseline exceptions; [coverage](../benchmarks/WARM-COVERAGE.md)
defines the remaining finite scope and evidence invalidation.

| Current qualification | Evidence | Status |
| --- | --- | --- |
| Packet/pixel/memory/cancellation regression | [Current qualification](../benchmarks/WARM-VP8-OPAQUE-CHROMA-GAMMA.md) | 4,948 checks/OS; 627 affected scalar WebP/OS and same actual package passed |
| Benchmark delivery/inspection validation | [Cleanup follow-up](../benchmarks/WARM-ASYNC-WEBP-CURRENT.md) | 9 Windows / 11 Linux groups; production qualification reused |
| Additional required search algorithm | [Distortion-only VP8 scope](../benchmarks/WARM-COVERAGE.md) | Existing diagram, Q75/effort2; current independent gap |
| CPU and allocation measurements after cleanup | [opaque chroma](../benchmarks/WARM-VP8-OPAQUE-CHROMA-GAMMA.md), [JPEG append](../benchmarks/WARM-JPEG-PAIRED-ENTROPY.md), [empty spatial histograms](../benchmarks/WARM-VP8L-EMPTY-HISTOGRAMS.md), [shared prediction rows](../benchmarks/WARM-VP8-SHARED-SMALL-ROWS.md) | Exact packets; eligible opaque8 preparation admits 4 KiB scratch (formerly 1 KiB); heap B/op unchanged |
| Full warm target | Independent quiet upper≤1× for all 255 named required paths, one CPU/operation | Open |

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
| [VP8 diagram, effort2 / distortion-only search](../benchmarks/WARM-VP8-OPAQUE-CHROMA-GAMMA.md) | 4,338.57 | 2,522.55 | 1.720 / 1.734 | 741,342 | Current gap (independent) |
| [VP8 RGBA8](../benchmarks/WARM-VP8-OPAQUE-CHROMA-GAMMA.md) | 77,283.89 | 53,477.53 | 1.445 / 1.625 | 23,258,168 | Current gap (short; qualified) |
| [VP8 opaque photograph](../benchmarks/WARM-VP8-OPAQUE-CHROMA-GAMMA.md) | 17,076.96 | 9,728.47 | 1.755 / 1.774 | 809,056 | Current gap (independent) |
| [VP8 opaque text/diagram](../benchmarks/WARM-VP8-OPAQUE-CHROMA-GAMMA.md) | 9,166.64 | 5,821.97 | 1.574 / 1.731 | 741,121 | Current gap (short; qualified) |
| [VP8 diagram, effort6 / trellis](../benchmarks/WARM-VP8-OPAQUE-CHROMA-GAMMA.md) | 16,609.72 | 9,835.02 | 1.689 / 1.748 | 754,328 | Current gap (short; qualified) |
| [VP8 opaque RGBA16, including normalization](../benchmarks/WARM-VP8-OPAQUE-CHROMA-GAMMA.md) | 26,973.16 | 15,619.34 | 1.727 / 1.832 | 1,201,536 | Current gap (short; qualified) |
| [VP8L RGBA8 / libwebp exact=1](../benchmarks/WARM-VP8L-EMPTY-HISTOGRAMS.md) | 191,649.39 | 122,403.16 | 1.566 / 1.827 | 47,674,928 | Current gap (short; qualified) |
| [VP8L RGBA16, including normalization / libwebp exact=1](../benchmarks/WARM-VP8L-RGBA16-CURRENT.md) | 188,991.14 | 122,121.60 | 1.548 / 1.685 | 47,674,928 | Current gap (short; reused qualification) |
| [VP8L async, fresh nonseekable output / libwebp exact=1](../benchmarks/WARM-ASYNC-WEBP-CURRENT.md) | 201,469.36 | 121,148.65 | 1.663 / 1.889 | 47,675,312 | Current gap (short; reused qualification) |
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
