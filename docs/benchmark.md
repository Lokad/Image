# Benchmark state

Updated 2026-10-27. Development0.2.0 remains under qualification; warm single-CPU
parity and replacement readiness are open. Compression correctness precedes CPU
gains. Published0.1.0 remains unchanged.

Pre-cleanup costs describe their frozen producers, **not the cleaned build**.
Current independent confirmations and short screens are labeled below; other
required paths remain open. A bounded integer VP8 coder publishes four bytes at
a time while preserving canonical packets. Effort2 diagram independently measures
4.028 ms, 1.603× (upper 1.614×), 741,375 B/op and exact 7,330 B. Elapsed is 6.53%
below the preceding independent mean; these separate runs are not a paired
baseline gain. Photograph independently measures 16.027 ms, 1.649× (upper 1.655×),
809,072 B/op and exact 17,688 B. Six existing anchors are reconciled. Allocation
rises 16–33 B/op; no new buffers or admission model. The existing async VP8L
control now independently confirms 1.669× (upper 1.731×), 47,675,312 B/op and exact
183,812 B, resolving its short. Required upper ≤1× stays open.

The [complete historical inventory](benchmark-pre-cleanup.md) retains all257cases,
raw links and baseline exceptions; [coverage](../benchmarks/WARM-COVERAGE.md)
defines the remaining finite scope and evidence invalidation.

| Current qualification | Evidence | Status |
| --- | --- | --- |
| Packet/pixel/memory/cancellation regression | [Current qualification](../benchmarks/WARM-VP8-WORD-ARITHMETIC.md) | 4,950 checks/OS; 629 affected scalar WebP/OS and same actual package passed |
| Benchmark delivery/inspection validation | [Cleanup follow-up](../benchmarks/WARM-ASYNC-WEBP-CURRENT.md) | 9 Windows / 11 Linux groups; production qualification reused |
| Additional required search algorithm | [Distortion-only VP8 scope](../benchmarks/WARM-COVERAGE.md) | Existing diagram, Q75/effort2; current independent gap |
| CPU and allocation measurements after cleanup | [word interval coder](../benchmarks/WARM-VP8-WORD-ARITHMETIC.md), [JPEG append](../benchmarks/WARM-JPEG-PAIRED-ENTROPY.md), [empty spatial histograms](../benchmarks/WARM-VP8L-EMPTY-HISTOGRAMS.md), [shared prediction rows](../benchmarks/WARM-VP8-SHARED-SMALL-ROWS.md) | Exact packets; VP8 fixed state adds 16–33 B/op; output admission and prior 4 KiB opaque8 preparation remain |
| Full warm target | Independent quiet upper≤1× for all 255 named required paths, one CPU/operation | Open |

This table selects current paths; unlisted inventory costs still need applicable evidence.
B/op is managed allocation, distinct
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
| [VP8 diagram, effort2 / distortion-only search](../benchmarks/WARM-VP8-WORD-ARITHMETIC.md) | 4,028.03 | 2,512.42 | 1.603 / 1.614 | 741,375 | Current gap (independent) |
| [VP8 RGBA8](../benchmarks/WARM-VP8-WORD-ARITHMETIC.md) | 74,746.58 | 53,985.33 | 1.385 / 1.395 | 23,258,184 | Current gap (independent) |
| [VP8 opaque photograph](../benchmarks/WARM-VP8-WORD-ARITHMETIC.md) | 16,027.22 | 9,721.50 | 1.649 / 1.655 | 809,072 | Current gap (independent) |
| [VP8 opaque text/diagram](../benchmarks/WARM-VP8-WORD-ARITHMETIC.md) | 8,698.88 | 5,832.06 | 1.492 / 1.574 | 741,137 | Current gap (short; qualified) |
| [VP8 diagram, effort6 / trellis](../benchmarks/WARM-VP8-WORD-ARITHMETIC.md) | 16,011.45 | 9,794.12 | 1.635 / 1.968 | 754,344 | Current gap (short; qualified) |
| [VP8 opaque RGBA16, including normalization](../benchmarks/WARM-VP8-WORD-ARITHMETIC.md) | 24,231.22 | 15,598.42 | 1.553 / 1.590 | 1,201,552 | Current gap (short; qualified) |
| [VP8L RGBA8 / libwebp exact=1](../benchmarks/WARM-VP8L-EMPTY-HISTOGRAMS.md) | 191,649.39 | 122,403.16 | 1.566 / 1.827 | 47,674,928 | Current gap (short; qualified) |
| [VP8L RGBA16, including normalization / libwebp exact=1](../benchmarks/WARM-VP8L-RGBA16-CURRENT.md) | 188,991.14 | 122,121.60 | 1.548 / 1.685 | 47,674,928 | Current gap (short; reused qualification) |
| [VP8L async, fresh nonseekable output / libwebp exact=1](../benchmarks/WARM-ASYNC-WEBP-CONFIRMATION.md) | 202,094.02 | 121,066.36 | 1.669 / 1.731 | 47,675,312 | Current gap (independent) |
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
