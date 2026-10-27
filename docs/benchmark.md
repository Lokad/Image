# Lokad.Image benchmarks

Updated 2026-10-27. **† Compressed PNG and VP8 effort2 rows** use the qualified 0.2.1
development producer; **‡ stored PNG** retains its unchanged 0.2.1 path evidence.
The other 26 controls retain published 0.2.0 measurements on unchanged paths.
The original 31-control release evidence remains immutable. Full CPU parity and
general replacement readiness remain open.

Measurements use the dedicated quiet Ubuntu 24.04 x64 VM
(AMD EPYC 9V45 96-Core Processor): one pinned CPU per operation, warm .NET 10,
sequential Release execution, no concurrency. This representative scope is not
the full performance objective.

Stored PNG16 and Fastest PNG8 independently confirm 12 measured iterations each;
other rows are short BenchmarkDotNet screens with three. Native brackets and
idle/process guards pass. Stored PNG16 improved from 1,112.30 to 426.14 µs;
Fastest PNG8 from 7,763.44 to 6,450.06 µs. Their ratios are 1.052× and 1.471×;
full parity remains open.
Ratio uses the faster native bracket; upper is the conservative confidence bound
including unfiltered managed samples. Short screens are not new independent
parity confirmations. B/op measures managed allocation, not native
allocation, peak live memory or cooperative-admission charges.

Operations include normalization, input delivery and independently owned output;
async encoding includes staging/delivery/release. Fixture preparation is excluded.
Source images are 513×259; resize rows state output dimensions. The
[release evidence](../benchmarks/release-0.2.0-benchmark-evidence.json) and
[stored publication](../benchmarks/WARM-PNG-BULK-STORED.md) and
[distance mapping](../benchmarks/WARM-PNG-DISTANCE-SYMBOL.md) record exact dimensions,
settings, boundaries, binaries, samples, package checks and retained raw archives.
The [effort2 baseline refresh](../benchmarks/VP8-RESIDUAL-SUMMARY-DEFERRED.md) retains
a new short on unchanged production; its prototype was deferred and removed.

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Packet B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| WebP VP8 + alpha, fresh seekable Stream | 7,831.60 | 5,985.32 | 1.308 / 1.445 | 1,966,583 | — |
| JPEG 4:4:4, forward-only / exact reference | 4,597.26 | 3,766.58 | 1.221 / 1.407 | 820,123 | 133,039 / 133,041 |
| JPEG 4:2:0 from caller RGB samples | 2,594.66 | 1,662.67 | 1.561 / 2.434 | 373,413 | 52,091 / 52,093 |
| JPEG RGBA16 4:2:0 / caller IMemoryBudget | 2,697.19 | 1,835.47 | 1.469 / 1.815 | 804,637 | 52,115 / 52,117 |
| JPEG grayscale, standalone SIMD libjpeg-turbo | 1,594.51 | 1,401.06 | 1.138 / 1.298 | 155,436 | 58,910 / 58,910 |
| JPEG from caller GRAY8, standalone SIMD libjpeg-turbo | 1,155.59 | 1,075.36 | 1.075 / 1.561 | 155,438 | 58,910 / 58,910 |
| VP8 diagram, effort2 / distortion-only search † | 3,993.94 | 2,523.55 | 1.583 / 1.709 | 741,401 | 7,330 / 7,330 |
| VP8 RGBA8 | 73,112.14 | 53,921.86 | 1.356 / 1.981 | 21,353,192 | 98,590 / 98,590 |
| VP8 opaque photograph | 15,516.03 | 9,706.04 | 1.599 / 1.677 | 809,072 | 17,688 / 17,688 |
| VP8 opaque text/diagram | 8,407.67 | 5,833.42 | 1.441 / 1.561 | 741,137 | 7,100 / 7,100 |
| VP8 diagram, effort6 / trellis | 16,076.88 | 9,769.43 | 1.646 / 1.961 | 754,344 | 6,818 / 6,818 |
| VP8 opaque RGBA16, including normalization | 23,851.31 | 15,554.15 | 1.533 / 1.565 | 1,201,552 | 72,380 / 72,380 |
| VP8L RGBA8 / libwebp exact=1 | 192,184.73 | 120,445.40 | 1.596 / 2.147 | 29,688,696 | 183,812 / 183,812 |
| VP8L RGBA16, including normalization / libwebp exact=1 | 193,006.68 | 122,663.07 | 1.573 / 2.125 | 29,688,696 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 | 205,529.71 | 127,025.13 | 1.618 / 2.028 | 29,689,080 | 183,812 / 183,812 |
| VP8L photograph / libwebp exact=1 | 132,270.49 | 83,865.69 | 1.577 / 2.343 | 20,409,392 | 160,032 / 160,032 |
| VP8L text/diagram / libwebp exact=1 | 11,087.40 | 9,649.39 | 1.149 / 1.709 | 27,419,167 | 7,234 / 7,234 |
| PNG RGBA8, SmallestSize9 / native9 † | 22,507.99 | 17,297.84 | 1.301 / 1.350 | 1,325,664 | 210,973 / 210,973 |
| PNG RGBA16, SmallestSize9 / native9 † | 26,234.53 | 20,109.52 | 1.305 / 1.565 | 3,813,670 | 963,202 / 963,202 |
| PNG RGBA8, Fastest1 / native1 † | 6,450.06 | 4,385.76 | 1.471 / 1.504 | 1,344,586 | 236,828 / 236,828 |
| PNG RGBA16, NoCompression0 / native0 ‡ | 426.14 | 405.06 | 1.052 / 1.067 | 1,741,709 | 1,066,552 / 1,066,552 |

Packets pass existing complete-byte gates; color JPEG differs only by the approved
two-byte-smaller attribution COM. Lossless pixels, lossy reconstruction and alpha
gates remain enforced. [Codec parity](codec-parity.md) gives the broader frozen
control scope. These measurements make no arbitrary-content compression claim.

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Bilinear resize RGBA16 → 256×129 | 543.06 | 417.56 | 1.301 / 1.411 | 290,128 |
| Bicubic resize RGBA8 → 256×129 | 668.47 | 432.43 | 1.546 / 2.457 | 332,183 |
| Bicubic upscale RGBA16 → 1027×519 | 4,140.75 | 2,874.80 | 1.440 / 1.709 | 5,016,594 |
| Bicubic RGBA16, width down / height up / input+output+4 KiB → 256×519 | 1,275.98 | 819.97 | 1.556 / 1.640 | 1,063,410 |
| Bicubic RGBA8, 32×16 / input+output+4 KiB → 32×16 | 464.68 | 375.02 | 1.239 / 1.370 | 2,185 |
| Bicubic RGBA8, 32×16 / default workspace → 32×16 | 363.67 | 375.44 | 0.969 / 1.174 | 199,372 |
| Lanczos3 reduction RGBA8 → 256×129 | 726.29 | 491.22 | 1.479 / 2.222 | 138,455 |
| Lanczos3 reduction RGBA16 → 256×129 | 729.03 | 505.81 | 1.441 / 1.745 | 138,435 |
| Lanczos3 enlargement RGBA8 → 1027×519 | 4,217.30 | 4,129.35 | 1.021 / 1.137 | 319,038 |
| Lanczos3 enlargement RGBA16 → 1027×519 | 5,395.65 | 4,106.76 | 1.314 / 1.571 | 4,583,983 |

Resize references use FFmpeg or its explicitly labeled zimg/zscale backend with
the approved halo, alpha and packing boundary. Managed pixels stay exact; native
rounding tolerances remain the frozen per-case bounds in the evidence. Tight
bicubic rows allow only input + output + 4 KiB local workspace.

References are pinned FFmpeg n8.1 and its libwebp 1.6.0 backend. Grayscale JPEG
uses the approved standalone **SIMD libjpeg-turbo 3.1.4.1** exception; lossless
WebP uses **libwebp exact=1** to preserve hidden RGB. Defaults/quality numbers
across encoders are not assumed equivalent. Startup, concurrency, GIF/animation
and unlisted content/settings are excluded.

[Measurement instructions](../benchmarks/README.md) and the deferred
[finite coverage objective](../benchmarks/WARM-COVERAGE.md) retain reproducibility
and outstanding work. Rebuild this summary with
`python benchmarks/render-benchmark.py`; verify it with `--check`.
