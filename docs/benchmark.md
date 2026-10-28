# Lokad.Image benchmarks

Updated 2026-10-28. **† Rows** use qualified 0.2.1 development-path evidence;
**‡ stored PNG** retains its unchanged 0.2.1 proof. Fourteen other controls retain
published 0.2.0 measurements on unchanged paths. VP8 Stream decode now has a
current-cost screen. The release snapshot remains immutable. Eighteen read
controls below use qualified 0.2.1 builds. Default JPEG420 decode has independent
current evidence; both VP8L decoder controls retain current screens. These 52 displayed controls do not
complete the full CPU-parity objective
or establish general replacement readiness.

Measurements use the dedicated quiet Ubuntu 24.04 x64 VM
(AMD EPYC 9V45 96-Core Processor): one pinned CPU per operation, warm .NET 10,
sequential Release execution, no concurrency. This representative scope is not
the full performance objective.

Stored PNG16, Fastest PNG8, caller-RGB420 JPEG, JPEG420 decode, VP8 photograph and VP8L RGBA8 have 12 measured iterations each;
other codec/editing rows are short BenchmarkDotNet screens with three. Native brackets and
idle/process guards pass. Shared-history predictor pricing reduces VP8L RGBA8
elapsed by 7.4% against a fresh independent baseline, preserving exact packets.
It remains a1.332× / upper1.360 gap. Allocation rises by8,224B/op for one reusable
8KiB analysis buffer; RGBA16/async/photograph have current screens.
Horizontal JPEG chroma filtering independently reduces caller-RGB420 elapsed by
16.1%, retaining373,416B/op and the approved exact packet boundary. Its1.283× /
upper1.294 gap remains; governed RGBA16 has a current1.224× screen.
Reconstructed-error bounds independently reduce VP8 photograph elapsed by4.3%,
retaining809,072B/op and exact packets; its1.479× / upper1.483 gap remains open.
Four other affected VP8 rows have current screens, without independent gain/pass
claims. Effort2 keeps its unchanged borrowed-cursor evidence. Current alpha
allocation includes the prior predictor owner's16B layout overhead increase.
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
[VP8 texture bounds](../benchmarks/WARM-VP8-TEXTURE-DISTORTION-BOUND.md) supply five
current VP8 encoding rows; unchanged effort2 retains
[borrowed-cursor evidence](../benchmarks/WARM-VP8-BORROWED-CURSOR.md).
[shared-history predictor pricing](../benchmarks/WARM-VP8L-PREPARED-PREDICTION-COST.md) supplies the
four affected VP8L rows. Other paths retain applicable prior proof.
[JPEG area filtering](../benchmarks/WARM-JPEG-HORIZONTAL-AREA-FILTER.md) refreshes
caller RGB420 and governed RGBA16; other subsampled encoder costs need current
evidence before a pass claim. Grayscale/4:4:4 encoding paths are unchanged.
The [read reconciliation](../benchmarks/WARM-READ-RECONCILIATION.md) refreshes VP8
Stream decode and four existing tight PNG companions without a speedup claim.
The [default decoder reconciliation](../benchmarks/CURRENT-DEFAULT-DECODERS.md)
supplies VP8L resident/Stream screens at1.425×/1.304×,1,216,162/1,216,190B/op.
[Integer quarter-phase chroma](../benchmarks/WARM-JPEG-QUARTER-CHROMA.md)
independently reduces JPEG420 decode from3.965ms to2.095ms (47.2% less), retaining
exact pixels. Its2.079×/upper2.241 gap remains. Reported allocation rises30B/op to
757,298;64B stack tiles and owned capacity stay unchanged. BothOS pass5054full
checks and SAME actual package/three consumer examples; existing293public JPEG
scalar checks perOS and unchanged inverse proof are reused. Other affected420/422
decoder costs need current evidence before pass claims; validation,encoding and
other codecs/editing/text retain applicable unchanged proof. Frozen packet/
settings and owned-output boundaries stay fixed. One current diagnostic profile
prioritizes SIMD chroma tiles; it introduces no extra performance control.

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Packet B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| WebP VP8 + alpha, fresh seekable Stream † | 7,754.22 | 6,004.85 | 1.291 / 1.334 | 1,966,583 | — |
| JPEG 4:4:4, forward-only / exact reference | 4,597.26 | 3,766.58 | 1.221 / 1.407 | 820,123 | 133,039 / 133,041 |
| JPEG 4:2:0 from caller RGB samples † | 2,030.36 | 1,582.17 | 1.283 / 1.294 | 373,416 | 52,091 / 52,093 |
| JPEG RGBA16 4:2:0 / caller IMemoryBudget † | 2,251.99 | 1,839.51 | 1.224 / 1.446 | 804,644 | 52,115 / 52,117 |
| JPEG grayscale, standalone SIMD libjpeg-turbo | 1,594.51 | 1,401.06 | 1.138 / 1.298 | 155,436 | 58,910 / 58,910 |
| JPEG from caller GRAY8, standalone SIMD libjpeg-turbo | 1,155.59 | 1,075.36 | 1.075 / 1.561 | 155,438 | 58,910 / 58,910 |
| VP8 diagram, effort2 / distortion-only search † | 3,598.28 | 2,509.90 | 1.434 / 1.484 | 741,375 | 7,330 / 7,330 |
| VP8 RGBA8 † | 70,132.45 | 53,670.66 | 1.307 / 1.393 | 21,353,208 | 98,590 / 98,590 |
| VP8 opaque photograph † | 14,372.74 | 9,719.09 | 1.479 / 1.483 | 809,072 | 17,688 / 17,688 |
| VP8 opaque text/diagram † | 8,186.76 | 5,828.66 | 1.405 / 1.440 | 741,137 | 7,100 / 7,100 |
| VP8 diagram, effort6 / trellis † | 14,921.39 | 9,825.29 | 1.519 / 1.614 | 754,344 | 6,818 / 6,818 |
| VP8 opaque RGBA16, including normalization † | 20,736.03 | 15,578.69 | 1.331 / 1.368 | 1,201,552 | 72,380 / 72,380 |
| VP8L RGBA8 / libwebp exact=1 † | 161,623.34 | 121,357.51 | 1.332 / 1.360 | 29,696,928 | 183,812 / 183,812 |
| VP8L RGBA16, including normalization / libwebp exact=1 † | 161,904.66 | 120,988.24 | 1.338 / 1.923 | 29,696,928 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 162,750.37 | 120,441.78 | 1.351 / 1.431 | 29,697,312 | 183,812 / 183,812 |
| VP8L photograph / libwebp exact=1 † | 109,137.75 | 82,564.36 | 1.322 / 1.411 | 20,417,624 | 160,032 / 160,032 |
| VP8L text/diagram / libwebp exact=1 | 11,087.40 | 9,649.39 | 1.149 / 1.709 | 27,419,167 | 7,234 / 7,234 |
| PNG RGBA8, SmallestSize9 / native9 † | 22,507.99 | 17,297.84 | 1.301 / 1.350 | 1,325,664 | 210,973 / 210,973 |
| PNG RGBA16, SmallestSize9 / native9 † | 26,234.53 | 20,109.52 | 1.305 / 1.565 | 3,813,670 | 963,202 / 963,202 |
| PNG RGBA8, Fastest1 / native1 † | 6,450.06 | 4,385.76 | 1.471 / 1.504 | 1,344,586 | 236,828 / 236,828 |
| PNG RGBA16, NoCompression0 / native0 ‡ | 426.14 | 405.06 | 1.052 / 1.067 | 1,741,709 | 1,066,552 / 1,066,552 |
| JPEG baseline420, resident decode † | 2,095.45 | 1,007.97 | 2.079 / 2.241 | 757,298 | — |
| VP8L RGBA8, resident decode † | 4,314.08 | 3,028.30 | 1.425 / 3.692 | 1,216,162 | — |
| VP8L RGBA8, fresh seekable Stream decode † | 4,462.61 | 3,422.65 | 1.304 / 1.578 | 1,216,190 | — |

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

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Probe PNG dimensions, seekable | 0.156 | 0.123 | 1.263 / 1.956 | 744 | Screen gap |
| Probe JPEG dimensions, seekable | 0.381 | 0.192 | 1.988 / 2.031 | 744 | Confirmed gap |
| Probe WebP dimensions, seekable | 0.159 | 0.123 | 1.289 / 1.550 | 744 | Screen gap |
| Probe JPEG dimensions, async nonseekable | 0.425 | 0.201 | 2.119 / 2.154 | 744 | Confirmed gap |
| Validate PNG8, span | 1,868.603 | 1,379.073 | 1.355 / 1.399 | 240 | Screen gap |
| Validate PNG16, span | 2,933.196 | 2,962.169 | 0.990 / 1.000 | 240 | Confirmed pass |
| Validate indexed Adam7 19×17, span | 5.286 | 3.930 | 1.345 / 1.570 | 1,168 | Screen gap |
| Validate JPEG baseline420, span | 620.209 | 676.122 | 0.917 / 0.935 | 9,984 | Confirmed pass |
| Validate JPEG progressive420, span | 277.509 | 263.457 | 1.053 / 1.063 | 4,072 | Confirmed gap |
| Validate VP8L+alpha, span | 4,423.742 | 3,218.514 | 1.374 / 1.648 | 685,390 | Screen gap |
| Validate VP8+alpha, span | 7,923.113 | 5,448.818 | 1.454 / 1.595 | 770,092 | Screen gap |
| Validate PNG16, async nonseekable | 3,001.887 | 3,602.437 | 0.833 / 0.847 | 8,544 | Confirmed pass |
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
validation pass independently; progressive420 remains a confirmed gap. Other
validation rows are short screens, without independent parity claims. The
[streaming PNG validator](../benchmarks/WARM-PNG-VALIDATION.md) reduces PNG16 elapsed
by42.4% and warm span allocation from17,008 to240B/op. Its repeat passes with a
narrow upper0.99966; the preceding upper1.00108 is retained in the evidence.
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
