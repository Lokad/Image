# Lokad.Image benchmarks

Updated 2026-10-30. Rows combine qualified **0.2.1 development** evidence with
retained 0.2.0 measurements whose relevant production paths remain unchanged.
These **58 comparisons** rank the remaining work; completion requires every
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

Compressed-alpha decoding now avoids a full RGBA intermediate. Complete stream
decode improves **2.89%**, with **531,756 fewer allocated B/op**; the existing
governed control's peak falls **37.34%**. Both stream and resident costs have
independent confirmations. [Direct-green evidence](../benchmarks/WARM-WEBP-ALPHA-DIRECT-GREEN.md).

The current producer passes **5,197 checks per OS**, 35 internal/270 public
focused checks in normal/scalar modes, and 54 additional scalar admission checks.
The same actual Release package, governed async consumer, two-frame compressed-
alpha consumer and three examples pass on both OSes. Packets, pixels, source
ownership and native-only bounds remain qualified. Unchanged CRC code retains
its earlier GFNI-only-disabled proof. Companion costs make no paired gain claim.

**Codecs**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Packet B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| WebP VP8 + alpha, fresh seekable Stream † | 7,068.76 | 5,985.38 | 1.181 / 1.184 | 1,434,827 | — |
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
| VP8L RGBA8 / libwebp exact=1 † | 161,623.34 | 121,357.51 | 1.332 / 1.360 | 29,696,928 | 183,812 / 183,812 |
| VP8L RGBA16, including normalization / libwebp exact=1 † | 185,381.86 | 138,254.62 | 1.341 / 1.431 | 29,696,928 | 183,812 / 183,812 |
| VP8L async, fresh nonseekable output / libwebp exact=1 † | 162,750.37 | 120,441.78 | 1.351 / 1.431 | 29,697,312 | 183,812 / 183,812 |
| VP8L photograph / libwebp exact=1 † | 121,047.71 | 89,021.62 | 1.360 / 1.397 | 20,418,440 | 160,032 / 160,032 |
| VP8L text/diagram / libwebp exact=1 | 11,087.40 | 9,649.39 | 1.149 / 1.709 | 27,419,167 | 7,234 / 7,234 |
| PNG RGBA8, SmallestSize9 / native9 † | 22,600.32 | 17,310.44 | 1.306 / 1.331 | 1,325,732 | 210,973 / 210,973 |
| PNG RGBA16, SmallestSize9 / native9 † | 26,874.29 | 20,094.40 | 1.337 / 1.347 | 3,813,732 | 963,202 / 963,202 |
| PNG RGBA8, Fastest1 / native1 † | 6,083.65 | 4,351.80 | 1.398 / 1.420 | 1,344,428 | 236,828 / 236,828 |
| PNG RGBA16, NoCompression0 / native0 † | 203.45 | 406.60 | 0.500 / 0.539 | 1,741,468 | 1,066,552 / 1,066,552 |
| PNG predictable RGBA16 3000², Optimal6 / mixed6 † | 352,397.34 | 407,457.02 | 0.865 / 0.870 | 3,402,572 | 573,041 / 573,041 |
| JPEG baseline420, resident decode † | 1,085.36 | 1,007.70 | 1.077 / 1.082 | 757,362 | — |
| JPEG DC-only, caller CMYK decode † | 348.53 | 331.27 | 1.052 / 1.058 | 1,085,961 | — |
| VP8L RGBA8, resident decode † | 4,583.44 | 3,438.40 | 1.333 / 1.404 | 1,216,162 | — |
| VP8L RGBA8, fresh seekable Stream decode † | 4,462.61 | 3,422.65 | 1.304 / 1.578 | 1,216,190 | — |
| JPEG CMYK sparse-2D, resident decode † | 629.11 | 447.48 | 1.406 / 1.413 | 1,085,758 | — |
| JPEG YCCK420, resident decode † | 421.45 | 707.63 | 0.596 / 0.607 | 895,547 | — |
| JPEG 4:1:1, resident decode † | 527.41 | 450.16 | 1.172 / 1.192 | 751,295 | — |
| JPEG 4:1:0, resident decode † | 367.40 | 507.52 | 0.724 / 0.735 | 720,838 | — |

**Editing**

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

**Inspection and pixel-coding validation**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Probe PNG dimensions, seekable | 0.156 | 0.123 | 1.263 / 1.956 | 744 | Screen gap |
| Probe JPEG dimensions, seekable | 0.381 | 0.192 | 1.988 / 2.031 | 744 | Confirmed gap |
| Probe WebP dimensions, seekable | 0.159 | 0.123 | 1.289 / 1.550 | 744 | Screen gap |
| Probe JPEG dimensions, async nonseekable | 0.425 | 0.201 | 2.119 / 2.154 | 744 | Confirmed gap |
| Validate PNG8, span | 1,382.593 | 1,378.823 | 1.003 / 1.010 | 240 | Confidence open |
| Validate PNG16, span | 2,799.737 | 2,958.844 | 0.946 / 0.956 | 240 | Confirmed pass |
| Validate indexed Adam7 19×17, span | 5.352 | 3.926 | 1.363 / 1.386 | 1,168 | Confirmed gap |
| Validate JPEG baseline420, span | 589.348 | 673.562 | 0.875 / 0.880 | 9,984 | Confirmed pass |
| Validate JPEG progressive420, span | 277.509 | 263.457 | 1.053 / 1.063 | 4,072 | Confirmed gap |
| Validate VP8L+alpha, span | 4,423.742 | 3,218.514 | 1.374 / 1.648 | 685,390 | Screen gap |
| Validate VP8+alpha, span | 7,367.434 | 5,452.572 | 1.351 / 1.382 | 770,092 | Confirmed gap |
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
displayed PNG CRC callers, VP8 stream decode, the indicated JPEG/WebP anchors, bicubic word anchors
and Lanczos3 have twelve measured iterations; remaining short screens retain
three. Short screens do not establish independent parity.

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
