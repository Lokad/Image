# Lokad.Image benchmarks

Updated 2026-10-31. Rows combine qualified **0.2.1 development** evidence with
retained unchanged-path measurements. Seven affected JPEG decode costs are current;
eight named paths retain proven unchanged-path evidence.
These **76 tabulated comparisons** rank the remaining work; completion requires every
material path in the [268-case coverage checklist](../benchmarks/WARM-COVERAGE.md).

Measurements run sequentially on the dedicated quiet Ubuntu x64 VM
(AMD EPYC 9V45 96-Core Processor), with one pinned CPU, warm .NET 10, tiering
and concurrent GC disabled. Each operation includes admission, normalization,
input delivery as labeled, fresh output ownership and release. Startup,
preparation, transport and validation are excluded. Native brackets and the
versioned idle/process guards pass. Ratios use the faster native bracket;
**upper** includes conservative filtered/unfiltered confidence bounds.
Managed **B/op** is measured allocation; native allocation is unmeasured.

All twelve displayed PNG comparisons affected by
[GFNI CRC reversal](../benchmarks/WARM-PNG-GFNI-CRC.md) have fresh independent
confirmations. Complete seekable Identify improves **14.16%**, with unchanged
192 B/op, but remains 1.226× / upper 1.232. Resident Identify remains a small
confirmed gap; PNG8 validation is confidence-open and is retained without
retiming. Four PNG controls pass at this tested scope. Header/probe paths skip
CRC and retain applicable evidence. Other affected PNG paths need current costs
before CPU-pass claims.
Two resident PNG decode controls also remain confidence-open on this build;
[None/Paeth evidence](../benchmarks/CURRENT-PNG-NONE-PAETH-CRC-COSTS.md) retains
one confirmation each without retiming or assuming an optimization need.
The existing [RGBA8/None decode](../benchmarks/CURRENT-PNG-RGBA8-CRC-COST.md)
separately confirms a 1.092× gap (upper 1.098), with 4,472 B/op.

Non-indexed VP8L decoding now owns its decoded storage directly: **2.73%** less
elapsed time, **531,649 fewer allocated B/op** and **41.4% lower governed peak**.
Eight affected companions have current costs. Literal/run passes at **0.692× /
upper 0.696**, 10,840 B/op; indexed and live-token controls remain gaps.
[Direct-output evidence](../benchmarks/WARM-VP8L-DIRECT-OUTPUT.md).

JPEG dimension probing now combines marker reads: **6.73%** less filtered elapsed,
unchanged **744 B/op / 512 admitted bytes**. Both named probes remain CPU gaps.
[Marker-pair evidence](../benchmarks/WARM-JPEG-PROBE-MARKER-PAIRS.md).

Lossless WebP color-bin reuse retains **4.02% less elapsed time** on the opaque
control, with unchanged **19,992,640 B/op**. Raw confidence intervals separate;
normalized intervals overlap. This specific gain is explicitly accepted with
that caveat. All four displayed color-search controls have current independent
costs; their complete packets remain exact and all remain CPU gaps.
[Color-bin evidence](../benchmarks/WARM-VP8L-COLOR-BIN-REUSE.md).

The existing async lossless WebP output boundary now has current independent
evidence: **1.293× / upper 1.316**, **29,697,312 B/op**, exact **183,812-byte** packets.
Staging, actual WriteAsync delivery, growth and output release are timed on both
subjects. [Async evidence](../benchmarks/CURRENT-ASYNC-VP8L-ENCODE-COST.md).

Async JPEG confirms **1.143× / upper 1.154**, **309,526 B/op** for caller RGB420,
and **1.199× / upper 1.210**, **820,475 B/op** for RGBA8/444 including flattening.
Reconstruction is exact; complete packets differ only by approved attribution.
[Caller RGB420](../benchmarks/CURRENT-ASYNC-JPEG-RGB420-COST.md) and
[RGBA8/444](../benchmarks/CURRENT-ASYNC-JPEG-RGBA8-COST.md) retain current evidence.

Byte-bilinear enlargement reuses exact horizontal rows in existing geometry:
**21.66% less elapsed**, **1.356× / upper 1.363**, **2,235,512 B/op**, with unchanged
admitted capacity. Reduction retains its current independent cost; both remain
gaps. [Row-reuse evidence](../benchmarks/WARM-BILINEAR-BYTE-ROW-REUSE.md).

Lanczos3 word reduction has a fresh current cost: **1.432× / upper 1.446**,
**138,437 B/op**. A centered-product candidate fails independent confirmation;
the qualified implementation is restored and new exact boundary tests retained.
[Current cost and rejected trial](../benchmarks/LANCZOS3-CENTERED-WORD-PRODUCTS-REJECTED.md).

Exact Int128 alpha accumulation improves tight RGBA8 bicubic enlargement
**7.26%**, removing **360,288 allocated B/op (14.45%)**: **1.473× / upper 1.481**,
**2,132,668 B/op**, exact pixels and the same **4 KiB** admission.
Five shared-caller costs are refreshed on the immutable build; all six remain gaps.
[Exact-alpha evidence](../benchmarks/WARM-EXACT-ALPHA-INT128.md).

Local original-order repair improves tight RGBA8 bicubic reduction **5.61%**
(all actuals **5.12%**), preserving exact pixels and the same **4 KiB** admission:
**1.474× / upper 1.540**, **132,309 B/op** (+3 B/op noise). Uncertain pairs
repair locally; tiny alpha still unwinds before exact scratch.
[Local-repair evidence](../benchmarks/WARM-BICUBIC-LOCAL-REPAIR.md).

The remaining shared-alpha caller audit reuses six proven unchanged paths and
confirms four current costs. Word vertical enlargement passes at **0.973× /
upper 0.985**, **2,514,272 B/op** and stops. Word bicubic enlargement remains
**1.230× / upper 1.244**, **5,016,602 B/op**. Existing word-bilinear/default-byte
enlargements remain gaps; no new workload or production change.
[Caller audit](../benchmarks/CURRENT-SHARED-ALPHA-CALLER-COSTS.md).
Exact completed-row reuse improves horizontal CMYK decoding **10.48%** independently
(all actuals **10.26%**, normalized **8.00%**) with separated confidence:
**1.366× / upper 1.396**, **1,085,755 B/op** (−2 B/op noise), unchanged owned capacity.
All seven affected displayed JPEG costs are reconciled on the immutable build;
progressive420 is now a confirmed gap. Eight named no-call proofs retain valid
costs, including grayscale, dense CMYK and both coding validations.
[Completed-row evidence](../benchmarks/WARM-JPEG-COMPLETED-ROW-REUSE.md).

Dense/general CMYK JPEG decoding independently passes at **0.894× / upper 0.916**,
**1,085,661 B/op**, with exact independent-matrix/native normal/scalar pixels.
The stale derived reference is reconciled without changing production or inputs;
this already-required case stops. [Current cost](../benchmarks/CURRENT-DENSE-CMYK-JPEG-COST.md).

Two retained PNG8 decodes under one borrowed caller root confirm **1.157× /
upper 1.168**, **1,637,520 B/op**, with **1,766,641 governed peak bytes**. Each
operation includes both complete decodes, retained overlap and releases on both
subjects; native has no shared admission API. Exact pixels and root cleanup pass.
[Retained-output cost](../benchmarks/CURRENT-RETAINED-PNG-DECODE-COST.md).

The already-required tight-workspace RGBA16 horizontal resize now has an independent
current cost: **1.267× / upper 1.326**, **530,838 B/op** (lower 1.201 confirms a gap).
Exact managed pixels and frozen native bounds pass; no build or optimization.
[Current horizontal cost](../benchmarks/CURRENT-TIGHT-HORIZONTAL-WORD-BICUBIC.md).

A bounded wider-support audit resolves eight existing default bicubic paths.
Both depths at **32×16 and 16×8 pass**, with conservative uppers **0.670–0.948×**,
**199,372–202,248 B/op**, and stop unchanged. Seven current confirmations reuse
the immutable build; Wide8 retains its proven unchanged-path confirmation.
Four larger-output controls remain gaps. No build, profile, new input or grid.
[Current wider-support costs](../benchmarks/CURRENT-WIDE-BICUBIC-COSTS.md).

The current producer passes **5,291 checks per OS**, plus 349 focused internal and
346 public focused checks normal/scalar and Linux with AVX512 disabled.
Header/probe proofs remain applicable.
The same actual Release package, governed
async/one-frame lossless/alpha consumers and three examples pass both OSes.
Packets, pixels, source ownership, admission and cancellation remain qualified.
Every affected named WebP decode/validation control has current costs; the three
opaque VP8 controls retain unchanged-path evidence. The 268-case objective stays open.

**Codecs**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Packet B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| WebP VP8 + alpha, fresh seekable Stream † | 6,986.26 | 6,014.54 | 1.162 / 1.176 | 1,434,827 | — |
| JPEG 4:4:4, forward-only / exact reference | 4,597.26 | 3,766.58 | 1.221 / 1.407 | 820,123 | 133,039 / 133,041 |
| JPEG 4:2:0 from caller RGB samples † | 1,877.03 | 1,577.94 | 1.190 / 1.197 | 388,222 | 52,091 / 52,093 |
| JPEG RGBA16 4:2:0 / caller IMemoryBudget † | 2,092.66 | 1,833.49 | 1.141 / 1.292 | 819,452 | 52,115 / 52,117 |
| JPEG grayscale, standalone SIMD libjpeg-turbo | 1,594.51 | 1,401.06 | 1.138 / 1.298 | 155,436 | 58,910 / 58,910 |
| JPEG from caller GRAY8, standalone SIMD libjpeg-turbo | 1,155.59 | 1,075.36 | 1.075 / 1.561 | 155,438 | 58,910 / 58,910 |
| VP8 diagram, effort2 / distortion-only search † | 3,331.69 | 2,509.72 | 1.328 / 1.463 | 741,375 | 7,330 / 7,330 |
| VP8 RGBA8 † | 70,231.82 | 53,675.16 | 1.308 / 1.382 | 21,353,208 | 98,590 / 98,590 |
| VP8 opaque photograph † | 13,899.64 | 9,701.62 | 1.433 / 1.438 | 809,072 | 17,688 / 17,688 |
| VP8 opaque text/diagram † | 7,750.98 | 5,823.99 | 1.331 / 1.478 | 741,137 | 7,100 / 7,100 |
| VP8 diagram, effort6 / trellis † | 15,835.32 | 11,306.07 | 1.401 / 1.415 | 754,344 | 6,818 / 6,818 |
| VP8 opaque RGBA16, including normalization † | 20,535.94 | 15,580.45 | 1.318 / 1.375 | 1,201,552 | 72,380 / 72,380 |
| VP8L RGBA8 / libwebp exact=1 † | 158,892.27 | 121,442.06 | 1.308 / 1.335 | 29,696,928 | 183,812 / 183,812 |
| VP8L opaque RGBA8 / libwebp exact=1 † | 99,191.77 | 71,204.07 | 1.393 / 1.447 | 19,992,640 | 121,896 / 121,896 |
| VP8L RGBA16, including normalization / libwebp exact=1 † | 166,167.06 | 122,510.98 | 1.356 / 1.385 | 29,696,928 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 156,836.69 | 121,322.88 | 1.293 / 1.316 | 29,697,312 | 183,812 / 183,812 |
| VP8L photograph / libwebp exact=1 † | 110,839.47 | 84,688.04 | 1.309 / 1.326 | 20,418,338 | 160,032 / 160,032 |
| VP8L text/diagram / libwebp exact=1 | 11,087.40 | 9,649.39 | 1.149 / 1.709 | 27,419,167 | 7,234 / 7,234 |
| PNG RGBA8, SmallestSize9 / native9 † | 22,600.32 | 17,310.44 | 1.306 / 1.331 | 1,325,732 | 210,973 / 210,973 |
| PNG RGBA16, SmallestSize9 / native9 † | 26,874.29 | 20,094.40 | 1.337 / 1.347 | 3,813,732 | 963,202 / 963,202 |
| PNG RGBA8, Fastest1 / native1 † | 6,083.65 | 4,351.80 | 1.398 / 1.420 | 1,344,428 | 236,828 / 236,828 |
| PNG RGBA16, NoCompression0 / native0 † | 203.45 | 406.60 | 0.500 / 0.539 | 1,741,468 | 1,066,552 / 1,066,552 |
| PNG predictable RGBA16 3000², Optimal6 / mixed6 † | 352,397.34 | 407,457.02 | 0.865 / 0.870 | 3,402,572 | 573,041 / 573,041 |
| JPEG baseline420, resident decode † | 1,111.27 | 1,013.36 | 1.097 / 1.136 | 757,362 | — |
| JPEG DC-only, caller CMYK decode † | 348.53 | 331.27 | 1.052 / 1.058 | 1,085,961 | — |
| VP8L RGBA8, resident decode † | 3,427.41 | 3,023.55 | 1.134 / 1.142 | 152,728 | — |
| VP8L RGBA8, fresh seekable Stream decode † | 3,400.72 | 3,032.93 | 1.121 / 1.134 | 152,824 | — |
| JPEG CMYK sparse-2D, resident decode † | 638.03 | 442.45 | 1.442 / 1.481 | 1,085,758 | — |
| JPEG YCCK420, resident decode † | 421.45 | 707.63 | 0.596 / 0.607 | 895,547 | — |
| JPEG 4:1:1, resident decode † | 527.41 | 450.16 | 1.172 / 1.192 | 751,295 | — |
| JPEG 4:1:0, resident decode † | 367.40 | 507.52 | 0.724 / 0.735 | 720,838 | — |
| JPEG caller RGB24 async nonseekable, 420 † | 1,862.24 | 1,628.86 | 1.143 / 1.154 | 309,526 | 52,091 / 52,093 |
| JPEG RGBA8 async nonseekable, 444 † | 4,626.46 | 3,857.58 | 1.199 / 1.210 | 820,475 | 133,039 / 133,041 |
| PNG8 decode, two retained images / shared caller root † | 3,292.45 | 2,846.74 | 1.157 / 1.168 | 1,637,520 | — |
| JPEG progressive grayscale, resident decode † | 2,270.11 | 1,712.36 | 1.326 / 1.332 | 683,126 | — |
| JPEG progressive420, resident decode † | 2,138.63 | 1,951.98 | 1.096 / 1.108 | 775,318 | — |
| JPEG progressive422, resident decode † | 2,728.90 | 2,246.56 | 1.215 / 1.258 | 847,116 | — |
| JPEG ordinary progressive444, resident decode † | 4,557.05 | 3,374.33 | 1.351 / 1.392 | 984,212 | — |
| JPEG progressive444, live-token resident decode † | 4,788.19 | 3,369.25 | 1.421 / 1.451 | 984,212 | — |
| JPEG dense CMYK, resident decode (confirmed pass) † | 2,786.90 | 3,115.72 | 0.894 / 0.916 | 1,085,661 | — |
| JPEG horizontal-only CMYK, resident decode † | 535.55 | 392.07 | 1.366 / 1.396 | 1,085,755 | — |

**Editing**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Bilinear resize RGBA8 † → 256×129 | 504.78 | 387.47 | 1.303 / 1.315 | 157,971 |
| Bilinear upscale RGBA8 † → 1027×519 | 2,914.62 | 2,163.33 | 1.347 / 1.358 | 2,235,512 |
| Bilinear resize RGBA16 † → 256×129 | 470.26 | 418.16 | 1.125 / 1.133 | 272,815 |
| Bicubic resize RGBA8 † → 256×129 | 579.25 | 418.45 | 1.384 / 1.400 | 332,188 |
| Bicubic upscale RGBA16 † → 1027×519 | 3,541.70 | 2,879.12 | 1.230 / 1.244 | 5,016,602 |
| Bicubic RGBA16, width down / height up / input+output+4 KiB † → 256×519 | 1,040.15 | 844.30 | 1.232 / 1.396 | 1,063,419 |
| Bicubic RGBA8, 32×16 / input+output+4 KiB → 32×16 | 464.68 | 375.02 | 1.239 / 1.370 | 2,185 |
| Bicubic RGBA8, 32×16 / default workspace (confirmed pass) † → 32×16 | 349.19 | 377.36 | 0.925 / 0.948 | 199,372 |
| Lanczos3 reduction RGBA8 † → 256×129 | 735.82 | 490.45 | 1.500 / 1.511 | 138,451 |
| Lanczos3 reduction RGBA16 † → 256×129 | 726.96 | 507.57 | 1.432 / 1.446 | 138,437 |
| Lanczos3 enlargement RGBA8 † → 1027×519 | 4,198.93 | 4,066.37 | 1.033 / 1.038 | 319,038 |
| Lanczos3 enlargement RGBA16 † → 1027×519 | 5,399.64 | 4,260.84 | 1.267 / 1.453 | 4,583,983 |
| Bicubic RGBA8 enlargement, input+output+4 KiB † → 1027×519 | 4,473.00 | 3,036.63 | 1.473 / 1.481 | 2,132,668 |
| Bicubic RGBA8 width enlargement, input+output+4 KiB † → 1027×259 | 1,644.06 | 1,524.52 | 1.078 / 1.094 | 1,064,526 |
| Bicubic RGBA8 height enlargement, input+output+4 KiB † → 513×519 | 2,165.54 | 1,748.88 | 1.238 / 1.256 | 1,065,523 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129 | 622.83 | 422.49 | 1.474 / 1.540 | 132,309 |
| Bicubic RGBA8 wider-support reduction † → 128×64 | 529.18 | 348.67 | 1.518 / 1.534 | 231,239 |

**Inspection and pixel-coding validation**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Probe PNG dimensions, seekable | 0.156 | 0.123 | 1.263 / 1.956 | 744 | Screen gap |
| Probe JPEG dimensions, seekable | 0.355 | 0.193 | 1.842 / 1.895 | 744 | Confirmed gap |
| Probe WebP dimensions, seekable | 0.165 | 0.125 | 1.323 / 1.425 | 744 | Confirmed gap |
| Probe JPEG dimensions, async nonseekable | 0.397 | 0.201 | 1.976 / 2.017 | 744 | Confirmed gap |
| Validate PNG8, span | 1,382.593 | 1,378.823 | 1.003 / 1.010 | 240 | Confidence open |
| Validate PNG16, span | 2,799.737 | 2,958.844 | 0.946 / 0.956 | 240 | Confirmed pass |
| Validate indexed Adam7 19×17, span | 5.352 | 3.926 | 1.363 / 1.386 | 1,168 | Confirmed gap |
| Validate JPEG baseline420, span | 589.348 | 673.562 | 0.875 / 0.880 | 9,984 | Confirmed pass |
| Validate JPEG progressive420, span | 291.290 | 267.973 | 1.087 / 1.094 | 4,072 | Confirmed gap |
| Validate VP8L+alpha, span | 3,537.096 | 3,050.177 | 1.160 / 1.165 | 685,421 | Confirmed gap |
| Validate VP8+alpha, span | 6,795.415 | 5,445.227 | 1.248 / 1.268 | 554,478 | Confirmed gap |
| Validate PNG16, async nonseekable | 2,850.602 | 3,605.292 | 0.791 / 0.802 | 8,544 | Confirmed pass |
| Inspect PNG8 complete headers, seekable, adapter reference | 0.805 | 0.590 | 1.365 / 1.603 | 8,528 | Screen gap |
| Identify PNG8 complete container, resident, adapter reference | 13.079 | 13.049 | 1.002 / 1.005 | 96 | Confirmed gap |
| Identify JPEG420, resident, canonical adapter | 0.501 | 1.193 | 0.420 / 0.426 | 96 | Confirmed pass |
| Identify VP8L+alpha, resident, parser adapter | 0.022 | 4.985 | 0.004 / 0.005 | 96 | Confirmed pass |
| Identify PNG8, fresh seekable Stream, adapter reference | 19.537 | 15.929 | 1.226 / 1.232 | 192 | Confirmed gap |
| Identify PNG8, async nonseekable, adapter reference | 20.312 | 19.321 | 1.051 / 1.060 | 8,400 | Confirmed gap |

† identifies qualified development-path costs. PNG encodes use the cached
four-invocation selector; predictable PNG16 native samples also batch four.
Every value is per complete operation. JPEG baseline decode, caller-CMYK, all
displayed PNG CRC callers, VP8/VP8L stream decode, the indicated JPEG/WebP anchors, bicubic word anchors
and Lanczos3 have twelve measured iterations. Retained
[RGBA16 bilinear](../benchmarks/WARM-BILINEAR-WORD-GEOMETRY.md) and
[RGBA8 bicubic reduction](../benchmarks/CURRENT-BICUBIC-BYTE-REDUCTION-COST.md)
also have independent twelves and proven unchanged executed paths.
Remaining short screens retain three and do not establish independent parity.

FFmpeg is the reference, with approved backend boundaries: lossless WebP encode
uses **libwebp exact=1** to preserve hidden RGB; grayscale JPEG uses explicitly
labeled SIMD **libjpeg-turbo**. Bicubic/Lanczos3 use **zimg**, FFmpeg's zscale
backend, including required packing, halos and alpha normalization. Existing
case-scoped native-only pixel bounds remain; managed pixels stay exact.
Equal numeric quality settings establish no quality equivalence. Encoded sizes
and fixed packet claims retain their declared input/settings scope.

Probe/header inspection, complete Identify and pixel-coding validation have
distinct boundaries. Header inspection skips PNG CRCs; Identify validates the
complete container and nine source facts without decoding pixels. Validation
checks coding syntax without retaining an RGBA result. Stream rows include
actual acquisition, copies, EOF and release as declared.

[CRC evidence](../benchmarks/warm-png-gfni-crc-evidence.json),
[JPEG traversal](../benchmarks/WARM-JPEG-BASELINE-DC-AC-FUSION.md),
[codec parity](codec-parity.md) and
[coverage/history](../benchmarks/WARM-COVERAGE.md) retain scope, producer identities,
remaining gaps and raw evidence. Update with `python benchmarks/render-benchmark.py`;
verify with `--check`.
