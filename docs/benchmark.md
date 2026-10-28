# Lokad.Image benchmarks

Updated 2026-10-28. **† Rows** use qualified 0.2.1 development-path evidence;
**‡ stored PNG** and Fastest PNG retain their applicable earlier evidence;
SmallestSize and predictive PNG are refreshed after bounded match scanning.
Eleven other controls retain
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

Predictive/stored PNG16, Fastest PNG8, caller-RGB420 JPEG, JPEG420/VP8L resident decode, VP8 photograph, VP8L RGBA8 encode and bicubic Up16/TightCross16 have 12 measured iterations each;
other codec/editing rows are short BenchmarkDotNet screens with three. Native brackets and
idle/process guards pass. Predictive PNG16 uses four complete sequential operations
per iteration/native sample to resolve execution-rate shifts; costs are per operation.
Shared-history predictor pricing reduces VP8L RGBA8
elapsed by 7.4% against a fresh independent baseline, preserving exact packets.
It remains a1.332× / upper1.360 gap. Allocation rises by8,224B/op for one reusable
8KiB analysis buffer; RGBA16/async/photograph have current screens.
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
The current producer passes 5,065 checks on each platform, plus 41 focused
normal/scalar and 27 public scalar cases, and the same actual package/three examples.
Nine tests solely for unreachable private invalid inputs were removed; public/live
coverage, exact bicubic pixels and shared ownership remain.
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
current costs. TightCross16 has an
[independent current confirmation](../benchmarks/CURRENT-BICUBIC-CROSS16-COST.md):
1.570× / upper1.580,1,063,410B/op. This unchanged-path follow-up reuses qualification
and makes no gain/regression claim against earlier short screens.
The [cached-path reconciliation](../benchmarks/CURRENT-BICUBIC-CACHED-COSTS.md)
retains mixed-axis costs and exact-half qualifications; byte zero-tap trimming
and shared word publication supersede its other rows with current results.
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
retains VP8 Stream decode. The
[current reconciliation](../benchmarks/current-png-vp8-reconciliation-evidence.json)
refreshes four tight PNG decoder costs after the shared cursor change below:
PNG8 ordinary/live-token screen at 1.090/1.405×, 68,224 B/op; PNG16 independently
confirms gaps at 1.010× / upper 1.015 and 1.040× / upper 1.053, 70,272 B/op.
Each preserves its frozen local ceiling, one-byte-below refusal and exact pixels.
The unchanged qualified build, harness, native workers, selectors and package
proof are reused, without builds or repeated qualification. Other PNG decoder
costs remain prior-production until reconciled.
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
| WebP VP8 + alpha, fresh seekable Stream † | 7,754.22 | 6,004.85 | 1.291 / 1.334 | 1,966,583 | — |
| JPEG 4:4:4, forward-only / exact reference | 4,597.26 | 3,766.58 | 1.221 / 1.407 | 820,123 | 133,039 / 133,041 |
| JPEG 4:2:0 from caller RGB samples † | 2,030.36 | 1,582.17 | 1.283 / 1.294 | 373,416 | 52,091 / 52,093 |
| JPEG RGBA16 4:2:0 / caller IMemoryBudget † | 2,251.99 | 1,839.51 | 1.224 / 1.446 | 804,644 | 52,115 / 52,117 |
| JPEG grayscale, standalone SIMD libjpeg-turbo | 1,594.51 | 1,401.06 | 1.138 / 1.298 | 155,436 | 58,910 / 58,910 |
| JPEG from caller GRAY8, standalone SIMD libjpeg-turbo | 1,155.59 | 1,075.36 | 1.075 / 1.561 | 155,438 | 58,910 / 58,910 |
| VP8 diagram, effort2 / distortion-only search † | 3,331.69 | 2,509.72 | 1.328 / 1.463 | 741,375 | 7,330 / 7,330 |
| VP8 RGBA8 † | 70,231.82 | 53,675.16 | 1.308 / 1.382 | 21,353,208 | 98,590 / 98,590 |
| VP8 opaque photograph † | 13,899.64 | 9,701.62 | 1.433 / 1.438 | 809,072 | 17,688 / 17,688 |
| VP8 opaque text/diagram † | 7,750.98 | 5,823.99 | 1.331 / 1.478 | 741,137 | 7,100 / 7,100 |
| VP8 diagram, effort6 / trellis † | 13,710.68 | 9,832.62 | 1.394 / 1.740 | 754,344 | 6,818 / 6,818 |
| VP8 opaque RGBA16, including normalization † | 20,535.94 | 15,580.45 | 1.318 / 1.375 | 1,201,552 | 72,380 / 72,380 |
| VP8L RGBA8 / libwebp exact=1 † | 161,623.34 | 121,357.51 | 1.332 / 1.360 | 29,696,928 | 183,812 / 183,812 |
| VP8L RGBA16, including normalization / libwebp exact=1 † | 161,904.66 | 120,988.24 | 1.338 / 1.923 | 29,696,928 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 162,750.37 | 120,441.78 | 1.351 / 1.431 | 29,697,312 | 183,812 / 183,812 |
| VP8L photograph / libwebp exact=1 † | 109,137.75 | 82,564.36 | 1.322 / 1.411 | 20,417,624 | 160,032 / 160,032 |
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
| Bicubic RGBA16, width down / height up / input+output+4 KiB † → 256×519 | 1,285.38 | 818.88 | 1.570 / 1.580 | 1,063,410 |
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
| Validate PNG8, span | 1,436.810 | 1,378.564 | 1.042 / 1.052 | 240 | Confirmed gap |
| Validate PNG16, span | 2,878.686 | 2,960.749 | 0.972 / 0.974 | 240 | Confirmed pass |
| Validate indexed Adam7 19×17, span | 5.286 | 3.930 | 1.345 / 1.570 | 1,168 | Screen gap |
| Validate JPEG baseline420, span | 620.209 | 676.122 | 0.917 / 0.935 | 9,984 | Confirmed pass |
| Validate JPEG progressive420, span | 277.509 | 263.457 | 1.053 / 1.063 | 4,072 | Confirmed gap |
| Validate VP8L+alpha, span | 4,423.742 | 3,218.514 | 1.374 / 1.648 | 685,390 | Screen gap |
| Validate VP8+alpha, span | 7,923.113 | 5,448.818 | 1.454 / 1.595 | 770,092 | Screen gap |
| Validate PNG16, async nonseekable | 2,942.287 | 3,614.992 | 0.814 / 0.817 | 8,544 | Confirmed pass |
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
validation pass independently; PNG8 and progressive420 remain confirmed gaps. Other
validation rows are short screens, without independent parity claims. The
[shared PNG cursor](../benchmarks/WARM-PNG-STREAMING-CURSOR.md) reduces PNG8
validation elapsed by 22.85% filtered / 22.87% all actuals against a fresh baseline,
with unchanged 240 B/op. Its 1.042× / upper 1.052 gap remains. PNG16 span and async
are refreshed on this producer and pass with upper 0.974/0.817, 240/8,544 B/op.
Both platforms pass 5,057 full checks, affected scalar tests and the same actual
package/three examples. No storage or admission boundary is added. Other affected
PNG decoder costs need current evidence before pass claims. The
[preceding streaming validator](../benchmarks/WARM-PNG-VALIDATION.md) retains
the earlier workspace and allocation improvements; its timing is historical.
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
