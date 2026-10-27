# Benchmark state

Updated 2026-10-27. Development0.2.0 remains under qualification; warm single-CPU
parity and replacement readiness are open. Compression correctness precedes CPU
gains. Published0.1.0 remains unchanged.

Pre-cleanup costs describe their frozen producers, **not the cleaned build**.
Current independent confirmations and short screens are labeled below; other
required paths remain open. Indexed VP8L tile reconstruction removes a full
histogram slab and copy while preserving exact packets. Async allocation falls
37.73% to 29,689,080 B/op; independent cost is 1.622× (upper 1.641×), exact 183,812 B.
Diagram independently measures 11.12 ms, 1.166× (upper 1.204×), 27,419,167 B/op and
exact 7,234 B: 15.68% less elapsed than its preceding independent mean. Separate
runs are not paired baseline gains. Six affected anchors are reconciled. The
unchanged photograph RDO point 1.649× remains the largest selected independent gap.
Required upper ≤1× stays open.

The [complete historical inventory](benchmark-pre-cleanup.md) retains all257cases,
raw links and baseline exceptions; [coverage](../benchmarks/WARM-COVERAGE.md)
defines the remaining finite scope and evidence invalidation.

| Current qualification | Evidence | Status |
| --- | --- | --- |
| Packet/pixel/memory/cancellation regression | [Current qualification](../benchmarks/WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md) | 4,957 checks/OS; 636 affected scalar WebP/OS and same actual package passed |
| Benchmark delivery/inspection validation | [Cleanup follow-up](../benchmarks/WARM-ASYNC-WEBP-CURRENT.md) | 9 Windows / 11 Linux groups; production qualification reused |
| Additional required search algorithm | [Distortion-only VP8 scope](../benchmarks/WARM-COVERAGE.md) | Existing diagram, Q75/effort2; current independent gap |
| CPU and allocation measurements after cleanup | [word interval coder](../benchmarks/WARM-VP8-WORD-ARITHMETIC.md), [JPEG append](../benchmarks/WARM-JPEG-PAIRED-ENTROPY.md), [indexed histogram replay](../benchmarks/WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md), [shared prediction rows](../benchmarks/WARM-VP8-SHARED-SMALL-ROWS.md) | Exact packets; async VP8L allocation falls 37.73%; actual capacity admission and retained ownership remain |
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
| [VP8 RGBA8](../benchmarks/WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md) | 73,450.84 | 53,323.00 | 1.377 / 1.977 | 21,353,192 | Current gap (short; qualified) |
| [VP8 opaque photograph](../benchmarks/WARM-VP8-WORD-ARITHMETIC.md) | 16,027.22 | 9,721.50 | 1.649 / 1.655 | 809,072 | Current gap (independent) |
| [VP8 opaque text/diagram](../benchmarks/WARM-VP8-WORD-ARITHMETIC.md) | 8,698.88 | 5,832.06 | 1.492 / 1.574 | 741,137 | Current gap (short; qualified) |
| [VP8 diagram, effort6 / trellis](../benchmarks/WARM-VP8-WORD-ARITHMETIC.md) | 16,011.45 | 9,794.12 | 1.635 / 1.968 | 754,344 | Current gap (short; qualified) |
| [VP8 opaque RGBA16, including normalization](../benchmarks/WARM-VP8-WORD-ARITHMETIC.md) | 24,231.22 | 15,598.42 | 1.553 / 1.590 | 1,201,552 | Current gap (short; qualified) |
| [VP8L RGBA8 / libwebp exact=1](../benchmarks/WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md) | 191,999.87 | 121,216.12 | 1.584 / 2.263 | 29,688,696 | Current gap (short; qualified) |
| [VP8L RGBA16, including normalization / libwebp exact=1](../benchmarks/WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md) | 193,837.99 | 121,180.27 | 1.600 / 1.847 | 29,688,696 | Current gap (short; qualified) |
| [VP8L async, fresh nonseekable output / libwebp exact=1](../benchmarks/WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md) | 197,779.35 | 121,909.14 | 1.622 / 1.641 | 29,689,080 | Current gap (independent) |
| [VP8L photograph / libwebp exact=1](../benchmarks/WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md) | 134,828.31 | 82,982.42 | 1.625 / 1.945 | 20,410,208 | Current gap (short; qualified) |
| [VP8L text/diagram / libwebp exact=1](../benchmarks/WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md) | 11,123.58 | 9,542.18 | 1.166 / 1.204 | 27,419,167 | Current gap (independent) |
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
