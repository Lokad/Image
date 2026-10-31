# Lokad.Image benchmarks

Updated 2026-11-01. These **98 selected comparisons** combine qualified
**0.2.1 development** costs with the retained **0.2.0 baseline**. The
[268-path coverage checklist](../benchmarks/WARM-COVERAGE.md) records current-path
validity, missing confirmations and remaining gaps. Full warm parity is open.

Sequential measurements use the dedicated quiet Ubuntu x64 VM, AMD EPYC 9V45,
one pinned CPU, warm .NET 10, tiering off and nonconcurrent workstation GC.
Complete operations include admission, labeled input delivery/normalization,
fresh output ownership and release. Startup, benchmark IPC, preparation and
checks are excluded. Native brackets and versioned idle/process guards pass.
Times are µs; ratios use the faster native bracket. **Upper** includes conservative
filtered/unfiltered confidence bounds. **B/op** is measured managed allocation,
including harness costs; native allocation is unmeasured. Short screens do not
establish independent parity, even when their upper bound is below one.

FFmpeg is the reference. Approved exceptions are **libwebp exact=1** for lossless
encoding, **SIMD libjpeg-turbo** for single-component JPEG encoding, and **zimg**
(FFmpeg zscale backend) for bicubic/Lanczos3. Required packing, halos, alpha work
and output ownership stay inside the declared boundaries. Exact packets/pixels,
case-scoped native bounds and frozen settings remain; equal numerical quality
settings do not establish equivalence. See [codec parity](codec-parity.md).

Current production passes **5,342 checks per OS**, including 948 live comparisons.
The same actual Release package, governed/retained/async consumers and three
examples pass both OSes. Applicable unchanged performance evidence is reused.
The [accepted color-bin gain](../benchmarks/WARM-VP8L-COLOR-BIN-REUSE.md) retains
its explicit normalized-confidence-overlap exception, without a parity claim.

[Completed header skips](../benchmarks/WARM-HEADER-SKIP-FAST-PATH.md) retain an
8.25% seekable JPEG probe gain with unchanged 744 B/op. Its async companion's
small 1.7% point cost increase is disclosed; all three refreshed read controls
remain confirmed gaps.

The three existing [RGBA16 Fastest PNG boundaries](../benchmarks/CURRENT-FASTEST-WORD-PNG-COSTS.md)
now confirm 1.331–1.372× gaps with exact complete 968,216-byte packets. Their
older BCL-encoder timing passes are historical; this adds current costs for
existing required paths, with unchanged production and reused qualification.
The [existing byte-color PNG controls](../benchmarks/CURRENT-BYTE-COLOR-PNG-COSTS.md)
likewise confirm 1.305–1.325× with exact packets; their old BCL-era costs are
superseded. Reference inputs/settings and the finite required scope are unchanged.
The [six named default/stored PNG controls](../benchmarks/CURRENT-PNG-ENCODE-COVERAGE.md)
have independent confirmations. Qualified [word-color packing](../benchmarks/WARM-PNG-WORD-COLOR-PACKING.md)
then improves Gray16 by **21.14%**, with unchanged allocation and exact packets.
Gray16 is now **1.254 / upper 1.285**; RGB16/gray-alpha16 confirm **1.320/1.314×**.
All three remain gaps; byte/RGBA/stored paths retain applicable unchanged costs.
The [existing async PNG output boundary](../benchmarks/CURRENT-ASYNC-PNG-ENCODE-COST.md)
now confirms **1.303 / upper 1.323**, 1,387,492 B/op and exact 210,973-byte packets.
Its earlier BCL-encoder CPU pass is superseded; production and qualification remain.
The [existing live-token PNG decode](../benchmarks/PNG-CANCELABLE-CURSOR-REASSESSMENT.md)
confirms **1.150 / upper 1.182**, 4,472 B/op. The archived cursor reassessment
misses 4.1% triage and is removed; accepted production and exact pixels remain.
Two [tight PNG decoding costs](../benchmarks/CURRENT-TIGHT-PNG-DECODE-COSTS.md)
are current: byte confirms **1.096 / upper 1.119**, 68,224 B/op; word is
**confidence-open at 0.997 / upper 1.016**, 70,272 B/op, with no pass or gap claim.

Two [grayscale JPEG encoder boundaries](../benchmarks/CURRENT-GRAYSCALE-JPEG-COSTS.md)
now have independent current costs. The additional named
[baseline grayscale decode](../benchmarks/CURRENT-BASELINE-GRAYSCALE-JPEG-DECODE-COST.md)
confirms **829.713 µs, 1.218 / upper 1.226, 672,196 B/op** with exact six-way
managed/native pixels. It remains a gap. Earlier experiments and individual
acceptance histories are retained in the coverage links and raw evidence.

**Codecs**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Packet B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| WebP VP8 + alpha, fresh seekable Stream † | 7,037.26 | 6,042.62 | 1.165 / 1.211 | 1,434,827 | — |
| JPEG 4:4:4, forward-only / exact reference | 4,597.26 | 3,766.58 | 1.221 / 1.407 | 820,123 | 133,039 / 133,041 |
| JPEG 4:2:0 from caller RGB samples † | 1,877.03 | 1,577.94 | 1.190 / 1.197 | 388,222 | 52,091 / 52,093 |
| JPEG RGBA16 4:2:0 / caller IMemoryBudget † | 2,071.31 | 1,837.31 | 1.127 / 1.135 | 819,489 | 52,115 / 52,117 |
| JPEG grayscale, standalone SIMD libjpeg-turbo † | 1,684.29 | 1,496.43 | 1.126 / 1.137 | 155,436 | 58,910 / 58,910 |
| JPEG from caller GRAY8, standalone SIMD libjpeg-turbo † | 1,218.17 | 1,136.37 | 1.072 / 1.090 | 155,439 | 58,910 / 58,910 |
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
| PNG RGBA16, Fastest1 / native1 † | 23,284.98 | 17,477.77 | 1.332 / 1.341 | 3,813,732 | 968,216 / 968,216 |
| PNG RGBA16 Fastest1, live cancelable token † | 24,004.55 | 17,494.31 | 1.372 / 1.383 | 3,813,732 | 968,216 / 968,216 |
| PNG RGBA16 Fastest1, forward-only output † | 23,265.92 | 17,478.35 | 1.331 / 1.345 | 3,813,764 | 968,216 / 968,216 |
| PNG grayscale8, Optimal6 / native6 † | 2,570.87 | 1,940.17 | 1.325 / 1.354 | 1,001,812 | 72,586 / 72,586 |
| PNG RGB8, Optimal6 / native6 † | 12,457.28 | 9,513.16 | 1.309 / 1.324 | 1,325,068 | 168,729 / 168,729 |
| PNG gray-alpha8, Optimal6 / native6 † | 5,626.99 | 4,312.43 | 1.305 / 1.350 | 1,021,036 | 120,366 / 120,366 |
| PNG RGBA8, Optimal6 / native6 † | 24,157.64 | 17,999.79 | 1.342 / 1.373 | 1,325,732 | 210,973 / 210,973 |
| PNG RGBA16, Optimal6 / native6 † | 26,842.58 | 20,081.34 | 1.337 / 1.359 | 3,813,732 | 963,202 / 963,202 |
| PNG RGB16, Optimal6 / native6 † | 20,239.97 | 15,329.96 | 1.320 / 1.338 | 3,571,252 | 739,472 / 739,472 |
| PNG grayscale16, Optimal6 / native6, red-plane reference † | 6,812.24 | 5,434.49 | 1.254 / 1.285 | 1,506,972 | 265,433 / 265,433 |
| PNG gray-alpha16, Optimal6 / native6 † | 13,361.83 | 10,171.22 | 1.314 / 1.333 | 2,264,564 | 506,887 / 506,887 |
| PNG RGBA8, stored0 / native0 † | 253.82 | 189.65 | 1.338 / 1.471 | 1,203,724 | 533,444 / 533,444 |
| PNG RGBA8 async, fresh nonseekable output, Optimal6 / native6 † | 24,513.62 | 18,812.43 | 1.303 / 1.323 | 1,387,492 | 210,973 / 210,973 |
| PNG RGBA8 decode, live token † | 1,607.11 | 1,397.16 | 1.150 / 1.182 | 4,472 | — |
| PNG RGBA8 decode, tight working limit † | 1,642.61 | 1,498.48 | 1.096 / 1.119 | 68,224 | — |
| PNG RGBA16 decode, tight working limit (confidence open) † | 3,120.34 | 3,128.99 | 0.997 / 1.016 | 70,272 | — |
| PNG predictable RGBA16 3000², Optimal6 / mixed6 † | 352,397.34 | 407,457.02 | 0.865 / 0.870 | 3,402,572 | 573,041 / 573,041 |
| JPEG baseline420, resident decode † | 1,111.27 | 1,013.36 | 1.097 / 1.136 | 757,362 | — |
| JPEG DC-only, caller CMYK decode † | 348.53 | 331.27 | 1.052 / 1.058 | 1,085,961 | — |
| VP8L RGBA8, resident decode † | 3,224.25 | 3,019.22 | 1.068 / 1.079 | 152,728 | — |
| VP8L RGBA8, fresh seekable Stream decode † | 3,271.53 | 3,048.58 | 1.073 / 1.102 | 152,824 | — |
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
| Bicubic RGBA16, width up / height down, input+output+4 KiB † → 1027×129 | 1,112.92 | 850.82 | 1.308 / 1.369 | 1,060,371 |
| Bicubic RGBA8, width up / height down, input+output+4 KiB † → 1027×129 | 1,167.23 | 864.52 | 1.350 / 1.367 | 530,400 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 128×64 | 505.70 | 347.78 | 1.454 / 1.475 | 32,905 |
| Bicubic RGBA8 reduction, input+output+4 KiB † → 64×32 | 545.55 | 370.06 | 1.474 / 1.506 | 8,329 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 128×64 | 611.34 | 417.61 | 1.464 / 1.524 | 65,674 |
| Bicubic RGBA16 reduction, input+output+4 KiB † → 64×32 | 518.01 | 389.68 | 1.329 / 1.355 | 16,521 |

**Inspection and pixel-coding validation**

| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Probe PNG dimensions, seekable | 0.156 | 0.123 | 1.263 / 1.956 | 744 | Screen gap |
| Probe JPEG dimensions, seekable | 0.326 | 0.194 | 1.681 / 1.736 | 744 | Confirmed gap |
| Probe WebP dimensions, seekable | 0.165 | 0.125 | 1.323 / 1.425 | 744 | Confirmed gap |
| Probe JPEG dimensions, async nonseekable | 0.404 | 0.201 | 2.007 / 2.052 | 744 | Confirmed gap |
| Validate PNG8, span | 1,417.189 | 1,384.730 | 1.023 / 1.056 | 240 | Confidence open |
| Validate PNG16, span | 2,805.515 | 2,974.590 | 0.943 / 0.972 | 240 | Confirmed pass |
| Validate indexed Adam7 19×17, span | 1.776 | 3.965 | 0.448 / 0.460 | 792 | Confirmed pass |
| Validate JPEG baseline420, span | 589.348 | 673.562 | 0.875 / 0.880 | 9,984 | Confirmed pass |
| Validate JPEG progressive420, span | 291.290 | 267.973 | 1.087 / 1.094 | 4,072 | Confirmed gap |
| Validate VP8L+alpha, span | 3,397.141 | 3,049.808 | 1.114 / 1.134 | 685,421 | Confirmed gap |
| Validate VP8+alpha, span | 6,696.676 | 5,469.412 | 1.224 / 1.250 | 554,478 | Confirmed gap |
| Validate PNG16, async nonseekable | 2,836.823 | 3,623.647 | 0.783 / 0.790 | 8,544 | Confirmed pass |
| Inspect PNG8 complete headers, seekable, adapter reference | 0.703 | 0.579 | 1.215 / 1.235 | 8,528 | Confirmed gap |
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
