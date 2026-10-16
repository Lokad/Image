# Lokad.Image benchmark state

Updated 2026-10-16. Latest retained warm measurements on the dedicated AMD EPYC
9V45 VM: Ubuntu 24.04.4, .NET 10.0.8 / SDK 10.0.204, FFmpeg n8.1
(`9047fa1b084f`). One pinned CPU, one codec/conversion/filter thread, normal SIMD,
tiering off, workstation nonconcurrent GC. Two original 513×259 RGBA8/16 rasters,
with documented projections for other pixel formats;
downsample/crop outputs are 256×129, upscale outputs are 1027×519,
and quarter turns/transposes are 259×513.

Times cover complete public operations and independent output acquisition/release,
including admission, conversion and disposal. Native wrappers/contexts are reused;
managed editing and PNG decode may reuse released pixel storage. No output is cached. Startup,
I/O, IPC, fixture preparation and validation are excluded. Each linked report
declares the exact workload, input and measured source; these are rolling results
at those sources, not a simultaneous rerun of the current HEAD.

Ratio = Lokad.Image / the faster reference pre/post mean; lower is faster. Upper is
the retained conservative confidence bound, taking the larger filtered/all-actual
bound where available. **Confirmed** requires an independent quiet repeat and
upper ≤2. Failed guards remain failures. Full required warm coverage is **open**.
Recent confirmations use `exclusive-vm-idle-v2`: live job checks and
five-second aggregate/CPU0 idle samples; historical load is diagnostic only.
Earlier receipts retain their original load guard and its failures.

*VP8L encode uses **libwebp exact=1 (FFmpeg encoder backend)** through its public
API. The public FFmpeg encoder changes hidden RGB and exposes no exact option.
Both subjects preserve every decoded RGBA sample, but compression differs:
Lokad.Image emits 533,100 bytes versus 183,812 bytes. This is an exact-pixel
timing comparison; compression efficiency parity is not established.

Single-component grayscale JPEG encoding uses **SIMD-enabled standalone
libjpeg-turbo3.1.4.1** as an approved tooling-only reference exception; FFmpeg's
MJPEG encoder emits three components. It confirms **1.388x / upper1.398x** at
**154,893 B/op**, including normalization and fresh output ownership/release.
Compressed sizes are **58,823 / 58,910 bytes**, managed/reference; the source-
quality gap is **0.000033dB**, within the existing0.25dB gate. Prepared fixtures,
worker and qualified production DLL are reused. The production library remains
safe managed code with no dependency or native binding.

Remaining current-production work is a finite set of named changed-path
confirmations and distinct unmeasured public boundaries. Historical confirmations
remain valid at their recorded sources; reuse requires unchanged relevant paths.
Warm identification-only coverage is missing because the existing inspection
benchmark includes startup and I/O. See [material coverage](../benchmarks/WARM-COVERAGE.md).

All **four existing bilinear cases** now have independent confirmation on the
current qualified core. Their conservative uppers range from 1.382x to 1.892x;
the tables retain exact ratios and allocations. Exact managed rational pixels,
native RGB bounds and the approved scoped alpha policies pass. No production
optimization, rebuild or repeated qualification was needed. Stop these cases;
the finite remaining coverage checklist stays open.

Current adaptive JPEG reconciliation: **4 affected 444/422 byte/word pairs**
independently confirm on the same qualified core, worst upper **1.639x**.
Exact frozen packets and source-quality gates pass; full normalization and fresh
output ownership are timed. [Current report](../benchmarks/WARM-CURRENT-JPEG-ADAPTIVE.md)
also records compressed sizes. No production optimization or rebuild is needed.

Current ordinary JPEG decode reconciliation: **8/8 named affected cases**
independently confirm on the qualified core, worst upper **1.626x**.
Exact frozen pixels, existing native agreement bounds and complete output
ownership pass. [Current report](../benchmarks/WARM-CURRENT-JPEG-DECODE.md)
states the fixed inputs and remaining cases. No production rebuild or retuning.

Current progressive JPEG reconciliation confirms the three named gray/420/422
pairs, worst upper **1.960x**. Exact frozen pixels and unchanged native bounds
pass. [Current report](../benchmarks/WARM-CURRENT-JPEG-PROGRESSIVE.md) includes
allocations and unchanged-build/qualification reuse. Stop these cases and the
already confirmed444/live-token444 pairs; remaining material coverage is open.

Current PNG decode reconciliation: **25/25 named affected cases** independently
confirm for current production paths, worst upper **1.494x**. Exact RGBA/alpha/hidden
RGB and complete output ownership pass. [Current report](../benchmarks/WARM-CURRENT-PNG-DECODE.md)
includes allocations and remaining cases. Shared three-case drivers preserve
sequential short/independent confirmation jobs without production rebuilds.

The demonstrated stored RGBA16 gap is resolved: **2.219x → 1.090x**, upper
**1.096x**; allocation falls **1,072,108 → 8,552B/op**. Bounded private IDAT
storage reuse halves decode time. Exact pixels, both quiet guards, all twelve
actuals and fresh Windows/Linux differential/same-package qualification pass.
Unchanged smaller-input confirmations remain valid; other named coverage is open.

The named Adam7 follow-up is closed: **RGBA16 1.377x / upper1.381x** and
**packed gray1 1.314x / upper1.320x** independently confirm on unchanged production
inputs. [Current evidence](../benchmarks/WARM-CURRENT-PNG-ADAM7.md) records exact
pixels, allocations, all twelve actuals and both guards. Later palette4/8 proofs
are reused; no production rebuild or repeated qualification.

The named PNG Stream follow-up is closed: **Png8 1.428x / upper1.441x**, **PngCropFit8 1.445x / upper1.454x**, **Png16 1.136x / upper1.146x**. Full input copying,
decode/conversion/composition and ownership boundaries remain timed.
[Current report](../benchmarks/WARM-CURRENT-PNG-STREAM.md) includes allocations;
word-composition proof and production/package qualification are reused.

**Decode**

| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [PNG palette4/full tRNS Adam7](../benchmarks/WARM-PNG-PALETTE4-ADAM7.md) | 1,480.35 | 1,135.35 | 1.304 / 1.387 | 2,592 | Confirmed |
| [PNG8 Stream, orient/crop/nearest fit](../benchmarks/WARM-CURRENT-PNG-STREAM.md) | 2,129.50 | 1,473.35 | 1.445 / 1.454 | 1,445,000 | Confirmed |
| [PNG16 Stream, orient/crop/nearest fit](../benchmarks/WARM-PNG-FUSED-INFLATION.md) | 3,572.35 | 2,936.91 | 1.216 / 1.231 | 4,460,393 | Confirmed |
| [PNG RGBA8, fresh seekable Stream](../benchmarks/WARM-CURRENT-PNG-STREAM.md) | 1,982.59 | 1,388.00 | 1.428 / 1.441 | 748,088 | Confirmed |
| [PNG RGBA16, fresh seekable Stream](../benchmarks/WARM-CURRENT-PNG-STREAM.md) | 3,499.10 | 3,080.71 | 1.136 / 1.146 | 3,067,227 | Confirmed |
| [JPEG420 restart, fresh seekable Stream](../benchmarks/WARM-JPEG-AC-CACHE.md) | 1,627.42 | 997.29 | 1.632 / 1.673 | 1,813,266 | Confirmed |
| [WebP VP8 + alpha, fresh seekable Stream](../benchmarks/WARM-VP8-STREAM.md) | 10,914.60 | 5,979.52 | 1.825 / 1.833 | 2,327,699 | Confirmed |
| [WebP VP8L, fresh seekable Stream](../benchmarks/WARM-VP8L-STREAM.md) | 5,549.36 | 3,026.08 | 1.834 / 1.851 | 1,924,876 | Confirmed |
| [PNG RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 1,904.89 | 1,392.72 | 1.368 / 1.371 | 4,456 | Confirmed |
| [PNG RGBA8, None / live cancelable token](../benchmarks/WARM-PNG-DECODE-BOUNDARIES.md) | 1,929.15 | 1,370.88 | 1.407 / 1.469 | 4,456 | Confirmed |
| [PNG RGBA8, None / exact working limit](../benchmarks/WARM-PNG-COUNTING.md) | 2,605.93 | 1,430.84 | 1.821 / 1.950 | 4,800 | Confirmed |
| [PNG RGBA8, None / exact limit + live cancelable token](../benchmarks/WARM-PNG-TIGHT-CANCELABLE8.md) | 2,559.30 | 1,371.11 | 1.867 / 1.873 | 4,800 | Confirmed |
| [PNG RGBA8, stored Deflate / None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 150.38 | 136.49 | 1.102 / 1.114 | 4,456 | Confirmed |
| [PNG RGBA16, stored Deflate / None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 322.73 | 296.18 | 1.090 / 1.096 | 8,552 | Confirmed |
| [PNG RGBA16, None](../benchmarks/WARM-CURRENT-PNG16-BICUBIC16.md) | 3,034.16 | 2,903.68 | 1.045 / 1.059 | 8,552 | Confirmed |
| [PNG RGBA16, None / exact working limit](../benchmarks/WARM-PNG-DECODE-BOUNDARIES.md) | 5,214.71 | 2,870.48 | 1.817 / 1.821 | 8,896 | Confirmed |
| [PNG RGBA16, None / live cancelable token](../benchmarks/WARM-PNG-COUNTING.md) | 4,518.51 | 2,873.34 | 1.573 / 1.583 | 8,552 | Confirmed |
| [PNG RGBA16, None / exact limit + live cancelable token](../benchmarks/WARM-PNG-COUNTING.md) | 5,265.78 | 2,871.10 | 1.834 / 1.852 | 8,896 | Confirmed |
| [PNG RGBA8, Paeth](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 3,230.00 | 2,843.71 | 1.136 / 1.140 | 4,456 | Confirmed |
| [PNG RGBA8, Up](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,209.03 | 1,518.35 | 1.455 / 1.458 | 4,456 | Confirmed |
| [PNG RGBA16, Up](../benchmarks/WARM-PNG-UP16.md) | 3,473.99 | 1,883.52 | 1.844 / 1.864 | 8,552 | Confirmed |
| [PNG RGBA8, Sub](../benchmarks/WARM-PNG-SUB8-CURRENT.md) | 1,179.60 | 699.60 | 1.686 / 1.697 | 4,456 | Confirmed |
| [PNG RGBA16, Sub](../benchmarks/WARM-PNG-SUB16.md) | 357.99 | 743.02 | 0.482 / 0.493 | 8,552 | Confirmed |
| [PNG RGBA8, Average](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,380.32 | 1,850.31 | 1.286 / 1.298 | 4,456 | Confirmed |
| [PNG RGBA16, Average](../benchmarks/WARM-PNG-AVERAGE16.md) | 5,894.17 | 4,384.37 | 1.344 / 1.357 | 8,552 | Confirmed |
| [PNG RGBA16, Paeth](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 5,401.14 | 5,511.74 | 0.980 / 0.990 | 8,552 | Confirmed |
| [PNG RGB8, Paeth](../benchmarks/WARM-PNG-RGB8-PAETH.md) | 3,140.91 | 2,608.88 | 1.204 / 1.214 | 3,432 | Confirmed |
| [PNG RGB16, Paeth](../benchmarks/WARM-PNG-RGB16-PAETH.md) | 8,510.46 | 4,469.96 | 1.904 / 1.928 | 6,504 | Confirmed |
| [PNG gray8, Paeth](../benchmarks/WARM-PNG-GRAY8-PAETH.md) | 1,038.34 | 1,029.60 | 1.008 / 1.014 | 1,384 | Confirmed |
| [PNG gray-alpha8, Paeth](../benchmarks/WARM-PNG-GRAY-ALPHA8-PAETH.md) | 2,051.11 | 2,016.66 | 1.017 / 1.068 | 2,408 | Confirmed |
| [PNG RGBA16, Adam7](../benchmarks/WARM-CURRENT-PNG-ADAM7.md) | 5,393.13 | 3,917.00 | 1.377 / 1.381 | 23,600 | Confirmed |
| [PNG gray1 → RGBA8, Adam7](../benchmarks/WARM-CURRENT-PNG-ADAM7.md) | 1,512.17 | 1,150.81 | 1.314 / 1.320 | 1,424 | Confirmed |
| [PNG palette8 + tRNS, Adam7](../benchmarks/WARM-PNG-PALETTE8-ADAM7.md) | 1,710.03 | 1,465.40 | 1.167 / 1.177 | 4,960 | Confirmed |
| [PNG RGB16, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,297.09 | 2,227.02 | 1.031 / 1.034 | 6,504 | Confirmed |
| [PNG gray8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 357.47 | 363.27 | 0.984 / 0.993 | 1,384 | Confirmed |
| [PNG gray8 + tRNS key127, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 357.50 | 758.36 | 0.471 / 0.474 | 1,416 | Confirmed |
| [PNG gray16 + tRNS key32639, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,970.49 | 2,055.01 | 1.445 / 1.450 | 2,440 | Confirmed |
| [PNG gray-alpha8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 845.98 | 1,156.04 | 0.732 / 0.733 | 2,408 | Confirmed |
| [PNG RGB8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 1,293.31 | 1,523.89 | 0.849 / 0.853 | 3,432 | Confirmed |
| [PNG RGB8 + tRNS key(48,139,231), None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 1,281.05 | 1,282.56 | 0.999 / 1.001 | 3,464 | Confirmed |
| [PNG RGB16 + tRNS key(12347,35806,59265), None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,316.17 | 3,182.24 | 0.728 / 0.732 | 6,536 | Confirmed |
| [PNG gray16, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 714.81 | 957.95 | 0.746 / 0.751 | 2,408 | Confirmed |
| [PNG gray-alpha16, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 1,563.09 | 1,795.59 | 0.871 / 0.874 | 4,456 | Confirmed |
| [PNG byte indices / sixteen-entry palette + tRNS, None](../benchmarks/WARM-PNG-SHORT-PALETTE.md) | 144.22 | 109.47 | 1.317 / 1.327 | 1,496 | Confirmed |
| [PNG palette8 + tRNS, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 349.92 | 364.06 | 0.961 / 0.970 | 2,456 | Confirmed |
| [PNG palette8 + two-entry tRNS, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 540.03 | 363.49 | 1.486 / 1.494 | 2,208 | Confirmed |
| [PNG palette4 + tRNS → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 109.81 | 140.53 | 0.781 / 0.799 | 984 | Confirmed |
| [PNG palette2 + tRNS → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 66.42 | 97.53 | 0.681 / 0.686 | 688 | Confirmed |
| [PNG palette1 + tRNS → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 54.06 | 88.92 | 0.608 / 0.611 | 552 | Confirmed |
| [PNG opaque gray1 → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 59.11 | 303.42 | 0.195 / 0.199 | 488 | Confirmed |
| [PNG gray1 + tRNS key1 → RGBA8, exact timed FFmpeg color-key](../benchmarks/WARM-PNG-GRAY1-TRANSPARENCY.md) | 64.68 | 684.86 | 0.094 / 0.098 | 520 | Confirmed |
| [PNG gray2 + tRNS key0 → RGBA8, None](../benchmarks/WARM-PNG-PACKED-TRANSPARENCY.md) | 71.49 | 497.90 | 0.144 / 0.145 | 648 | Confirmed |
| [PNG gray1/key1 and gray2/key1, unfiltered native](../benchmarks/WARM-PNG-PACKED-TRANSPARENCY.md) | — | — | — | — | Unscored: native alpha differs |
| [PNG opaque gray2 → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 71.73 | 106.38 | 0.674 / 0.679 | 616 | Confirmed |
| [PNG opaque gray4 → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 116.09 | 169.54 | 0.685 / 0.698 | 872 | Confirmed |
| [JPEG baseline gray](../benchmarks/WARM-CURRENT-JPEG-DECODE.md) | 937.68 | 681.99 | 1.375 / 1.391 | 1,221,556 | Confirmed |
| [JPEG baseline 4:2:0](../benchmarks/WARM-CURRENT-JPEG-DECODE.md) | 1,455.95 | 1,009.28 | 1.443 / 1.449 | 1,619,530 | Confirmed |
| [JPEG baseline 4:2:0, row restart markers](../benchmarks/WARM-CURRENT-JPEG-DECODE.md) | 1,640.44 | 1,013.53 | 1.619 / 1.626 | 1,623,906 | Confirmed |
| [JPEG baseline 4:2:2](../benchmarks/WARM-CURRENT-JPEG-DECODE.md) | 1,730.66 | 1,347.06 | 1.285 / 1.291 | 1,978,655 | Confirmed |
| [JPEG baseline 4:4:4, owned input](../benchmarks/WARM-CURRENT-JPEG-DECODE.md) | 2,622.85 | 1,896.19 | 1.383 / 1.390 | 2,596,169 | Confirmed |
| [JPEG baseline 4:4:4, native input](../benchmarks/WARM-CURRENT-JPEG-DECODE.md) | 2,648.65 | 1,718.17 | 1.542 / 1.549 | 2,664,373 | Confirmed |
| [JPEG baseline direct RGB, full resolution](../benchmarks/WARM-JPEG-AC-CACHE.md) | 2,460.96 | 1,451.53 | 1.695 / 1.709 | 2,664,366 | Confirmed |
| [JPEG direct RGB 4:2:2, timed centered correction](../benchmarks/WARM-CURRENT-JPEG-DECODE.md) | 1,495.61 | 1,092.79 | 1.369 / 1.375 | 1,978,658 | Confirmed |
| [JPEG direct RGB 4:2:0, timed component-plane interpretation](../benchmarks/WARM-CURRENT-JPEG-DECODE.md) | 1,131.19 | 742.22 | 1.524 / 1.532 | 1,619,540 | Confirmed |
| [JPEG baseline 4:4:4, native input / live cancelable token](../benchmarks/WARM-JPEG-BASELINE-AC-RUN.md) | 3,048.96 | 1,722.47 | 1.770 / 1.797 | 2,664,373 | Confirmed |
| [JPEG progressive gray](../benchmarks/WARM-CURRENT-JPEG-PROGRESSIVE.md) | 3,343.59 | 1,709.58 | 1.956 / 1.960 | 1,232,501 | Confirmed |
| [JPEG progressive 4:2:0](../benchmarks/WARM-CURRENT-JPEG-PROGRESSIVE.md) | 3,187.59 | 1,952.50 | 1.633 / 1.650 | 1,637,521 | Confirmed |
| [JPEG progressive 4:4:4 / live cancelable token](../benchmarks/WARM-JPEG-PROGRESSIVE-CANCELABLE444.md) | 6,453.37 | 3,335.92 | 1.935 / 1.940 | 2,682,364 | Confirmed |
| [JPEG progressive 4:2:2](../benchmarks/WARM-CURRENT-JPEG-PROGRESSIVE.md) | 3,913.19 | 2,244.21 | 1.744 / 1.752 | 1,996,661 | Confirmed |
| [JPEG progressive 4:4:4](../benchmarks/WARM-JPEG-AC-CACHE.md) | 6,282.99 | 3,348.11 | 1.877 / 1.896 | 2,682,364 | Confirmed |
| [WebP VP8 + alpha](../benchmarks/WARM-VP8-FILTER8.md) | 10,777.02 | 5,977.48 | 1.803 / 1.819 | 1,966,616 | Confirmed |
| [WebP VP8 opaque, simple luma filter](../benchmarks/WARM-VP8-SIMPLE-FILTER.md) | 8,538.31 | 4,889.76 | 1.746 / 1.765 | 760,548 | Confirmed |
| [WebP VP8 opaque, live cancelable token](../benchmarks/WARM-VP8-CANCELABLE.md) | 8,789.89 | 4,783.31 | 1.838 / 1.843 | 760,548 | Confirmed |
| [WebP VP8, opaque](../benchmarks/WARM-VP8-FILTER8.md) | 8,649.43 | 4,797.64 | 1.803 / 1.808 | 760,548 | Confirmed |
| [WebP VP8L, sixteen-color indexed input](../benchmarks/WARM-VP8L-INDEXED16.md) | 825.07 | 543.26 | 1.519 / 1.550 | 1,351,961 | Confirmed |
| [WebP VP8L, literal/run input](../benchmarks/WARM-CURRENT-VP8L-LITERAL.md) | 3,479.66 | 2,150.57 | 1.618 / 1.625 | 1,074,301 | Confirmed |
| [WebP VP8L, native input](../benchmarks/WARM-VP8L-CANONICAL.md) | 5,703.55 | 3,023.03 | 1.887 / 1.904 | 1,216,180 | Confirmed |
| [WebP VP8L, native input / live cancelable token](../benchmarks/WARM-VP8L-CANCELABLE.md) | 5,757.19 | 3,020.47 | 1.906 / 1.919 | 1,216,180 | Confirmed |

The named RGBA PNG encode follow-up is closed: **Encode16 0.745x / upper0.763x**, **Encode8 0.314x / upper0.316x**. Exact samples,
full packet ownership and the unchanged5% IDAT-size gate pass.
[Current report](../benchmarks/WARM-CURRENT-PNG-ENCODE.md) includes compressed
sizes and allocations. Production/harness/native builds and qualification repeat
counts are zero; stop these cases and retain other valid confirmed settings.

The named alpha-bearing VP8 follow-up is closed: **Encode16 0.576x / upper0.581x**, **Encode8 0.569x / upper0.573x**. The unchanged fixed quality pair,
exact alpha, full normalization/ownership and decoder bounds pass.
[Current report](../benchmarks/WARM-CURRENT-VP8-ENCODE.md) includes allocations and
471,436/98,590-byte packets; no compression-efficiency parity is claimed.
Prepared quality records/packets and production qualification are reused.

**Encode**

| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [PNG RGBA8, level 6 / None](../benchmarks/WARM-CURRENT-PNG-ENCODE.md) | 5,425.94 | 17,261.40 | 0.314 / 0.316 | 527,627 | Confirmed |
| [PNG RGBA8, NoCompression / None](../benchmarks/WARM-PNG-STORED8-ENCODE.md) | 357.17 | 189.72 | 1.883 / 1.933 | 2,102,472 | Confirmed |
| [PNG RGBA16, NoCompression / None](../benchmarks/WARM-PNG-STORED16-ENCODE.md) | 504.25 | 405.59 | 1.243 / 1.289 | 4,203,541 | Confirmed |
| [PNG RGBA8, Fastest managed2/native1 / None](../benchmarks/WARM-PNG-FAST8.md) | 3,077.83 | 4,351.79 | 0.707 / 0.712 | 527,669 | Confirmed |
| [PNG RGBA16, Fastest managed2/native1 / None](../benchmarks/WARM-PNG-FAST2.md) | 12,927.95 | 17,660.77 | 0.732 / 0.735 | 2,103,738 | Confirmed |
| [PNG RGBA16, Fastest managed2/native1 / live cancelable token](../benchmarks/WARM-PNG-FAST2.md) | 12,932.71 | 17,700.33 | 0.731 / 0.747 | 2,103,647 | Confirmed |
| [PNG RGBA16, Fastest managed2/native1 / forward-only output](../benchmarks/WARM-PNG-FAST-FORWARD16.md) | 13,628.70 | 17,677.29 | 0.771 / 0.805 | 2,103,770 | Confirmed |
| [PNG RGBA16, level 6 / None](../benchmarks/WARM-CURRENT-PNG-ENCODE.md) | 14,974.15 | 20,089.84 | 0.745 / 0.763 | 2,103,624 | Confirmed |
| [PNG RGBA8, SmallestSize9 / native9](../benchmarks/WARM-PNG-SMALLEST8.md) | 14,469.30 | 17,319.86 | 0.835 / 0.841 | 527,564 | Confirmed |
| [PNG RGBA16, SmallestSize managed9/native9 / None](../benchmarks/WARM-PNG-SMALLEST16.md) | 19,689.04 | 20,296.44 | 0.970 / 0.985 | 2,103,522 | Confirmed |
| [PNG RGB8, Optimal managed9/native6 / None](../benchmarks/WARM-PNG-BYTE-OPTIMAL.md) | 10,402.09 | 9,514.15 | 1.093 / 1.107 | 527,098 | Confirmed |
| [PNG RGB16, level 6 / None](../benchmarks/WARM-PNG-RGB16-ENCODE.md) | 13,121.10 | 15,174.40 | 0.865 / 0.871 | 2,102,765 | Confirmed |
| [PNG gray8, Optimal managed9/native6 / None](../benchmarks/WARM-PNG-BYTE-OPTIMAL.md) | 3,540.72 | 1,964.99 | 1.802 / 1.818 | 263,664 | Confirmed |
| [PNG gray16, level 6 / None](../benchmarks/WARM-PNG-GRAY16-ENCODE.md) | 5,667.73 | 4,927.46 | 1.150 / 1.156 | 1,051,455 | Confirmed |
| [PNG gray-alpha8, Optimal managed9/native6 / None](../benchmarks/WARM-PNG-BYTE-OPTIMAL.md) | 5,924.76 | 4,311.86 | 1.374 / 1.382 | 264,179 | Confirmed |
| [PNG gray-alpha16, level 6 / None](../benchmarks/WARM-PNG-GRAY-ALPHA16-ENCODE.md) | 9,316.78 | 10,318.59 | 0.903 / 0.915 | 1,052,368 | Confirmed |
| [JPEG 4:4:4, adaptive Huffman / matched quality](../benchmarks/WARM-CURRENT-JPEG-ADAPTIVE.md) | 5,957.29 | 3,776.02 | 1.578 / 1.585 | 559,429 | Confirmed |
| [JPEG 4:4:4, forward-only output / geometric AC Huffman](../benchmarks/WARM-JPEG444-FORWARD.md) | 6,613.11 | 3,770.32 | 1.754 / 1.765 | 526,051 | Confirmed |
| [JPEG 4:4:4, live cancelable token / geometric AC Huffman](../benchmarks/WARM-JPEG-FIXED-OCCUPANCY.md) | 5,635.05 | 3,766.56 | 1.496 / 1.700 | 526,075 | Regression screen; prior confirmed |
| [JPEG 4:4:4 from RGBA16, adaptive Huffman / full normalization](../benchmarks/WARM-CURRENT-JPEG-ADAPTIVE.md) | 5,955.85 | 3,852.95 | 1.546 / 1.552 | 556,286 | Confirmed |
| [JPEG 4:2:2, adaptive Huffman / matched quality](../benchmarks/WARM-CURRENT-JPEG-ADAPTIVE.md) | 3,824.38 | 2,344.76 | 1.631 / 1.639 | 294,116 | Confirmed |
| [JPEG 4:2:2 from RGBA16, adaptive Huffman / full normalization](../benchmarks/WARM-CURRENT-JPEG-ADAPTIVE.md) | 3,816.67 | 2,425.80 | 1.573 / 1.588 | 294,116 | Confirmed |
| [JPEG 4:2:0, adaptive Huffman / matched quality](../benchmarks/WARM-JPEG-AC-PLANNING.md) | 2,881.90 | 1,763.43 | 1.634 / 1.641 | 297,780 | Confirmed |
| [JPEG 4:2:0, adaptive / row restart markers](../benchmarks/WARM-JPEG-AC-PLANNING.md) | 2,886.13 | 1,604.33 | 1.799 / 1.809 | 297,780 | Confirmed |
| [JPEG 4:2:0, forward-only output / geometric AC Huffman](../benchmarks/WARM-JPEG-FIXED-GEOMETRIC.md) | 3,382.57 | 1,765.48 | 1.916 / 1.936 | 263,812 | Confirmed |
| [JPEG 4:2:0 from RGBA16, forward-only output](../benchmarks/WARM-JPEG-FIXED-OCCUPANCY.md) | 3,057.46 | 1,835.63 | 1.666 / 1.973 | 263,820 | Regression screen; prior confirmed |
| [JPEG 4:2:0 from RGBA16, live cancelable token](../benchmarks/WARM-JPEG-FIXED-OCCUPANCY.md) | 3,031.41 | 1,835.85 | 1.651 / 1.663 | 263,788 | Confirmed |
| [JPEG 4:2:0 from RGBA16, adaptive Huffman](../benchmarks/WARM-JPEG-AC-PLANNING.md) | 3,098.92 | 1,842.05 | 1.682 / 1.697 | 297,780 | Confirmed |
| [JPEG grayscale, standalone SIMD libjpeg-turbo](../benchmarks/WARM-JPEG-GRAYSCALE.md) | 1,958.56 | 1,411.27 | 1.388 / 1.398 | 154,893 | Confirmed |
| [WebP VP8, opaque‡](../benchmarks/WARM-VP8-OPAQUE-ENCODE.md) | 29,381.20 | 15,527.63 | 1.892 / 1.918 | 2,536,203 | Confirmed |
| [WebP VP8, opaque from RGBA16‡](../benchmarks/WARM-VP8-OPAQUE-RGBA16.md) | 29,778.24 | 15,604.01 | 1.908 / 1.922 | 2,535,736 | Confirmed |
| [WebP VP8 + alpha‡](../benchmarks/WARM-CURRENT-VP8-ENCODE.md) | 30,390.89 | 53,447.22 | 0.569 / 0.573 | 2,615,939 | Confirmed |
| [WebP VP8 + alpha from RGBA16‡](../benchmarks/WARM-CURRENT-VP8-ENCODE.md) | 30,755.98 | 53,436.98 | 0.576 / 0.581 | 2,615,472 | Confirmed |
| [WebP VP8L, exact backend*](../benchmarks/WARM-VP8L-ENCODE-EXACT.md) | 7,363.30 | 121,587.95 | 0.061 / 0.063 | 2,100,170 | Confirmed |
| [WebP VP8L, opaque RGBA8 / exact backend*](../benchmarks/WARM-VP8L-OPAQUE8.md) | 7,195.24 | 70,637.23 | 0.102 / 0.103 | 2,100,170 | Confirmed |
| [WebP VP8L from RGBA16, exact backend*](../benchmarks/WARM-VP8L-RGBA16-ENCODE.md) | 7,928.11 | 121,282.99 | 0.065 / 0.066 | 2,100,119 | Confirmed |

**Editing**

| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [Horizontal flip RGBA8, two live outputs / per image](../benchmarks/WARM-RETAINED-FLIP.md) | 13.26 | 9.16 | 1.447 / 1.484 | 128 | Confirmed |
| [Nearest resize RGBA8](../benchmarks/WARM-COVERAGE-QUIET.md) | 58.67 | 252.66 | 0.232 / 0.241 | 132,294 | Confirmed |
| [Nearest upscale RGBA8](../benchmarks/WARM-NEAREST-UP8.md) | 935.05 | 1,006.00 | 0.929 / 0.955 | 2,133,091 | Confirmed |
| [Nearest upscale RGBA16](../benchmarks/WARM-NEAREST-UP16.md) | 1,071.13 | 1,019.06 | 1.051 / 1.095 | 4,265,431 | Confirmed |
| [Nearest resize RGBA16](../benchmarks/WARM-PREPARED-CONFIRM.md) | 105.69 | 170.58 | 0.620 / 0.629 | 264,466 | Confirmed |
| [Bilinear resize RGBA8](../benchmarks/WARM-BILINEAR8-QUIET.md) | 656.29 | 386.57 | 1.698 / 1.704 | 157,957 | Confirmed |
| [Bilinear resize RGBA16†](../benchmarks/WARM-CURRENT-BILINEAR.md) | 783.89 | 416.64 | 1.881 / 1.892 | 290,118 | Confirmed |
| [Bilinear upscale RGBA8†](../benchmarks/WARM-CURRENT-BILINEAR.md) | 3,774.00 | 2,150.06 | 1.755 / 1.762 | 2,235,599 | Confirmed |
| [Bilinear upscale RGBA16†](../benchmarks/WARM-CURRENT-BILINEAR.md) | 2,563.23 | 1,869.40 | 1.371 / 1.382 | 4,498,974 | Confirmed |
| [RGBA8 bicubic, single-pixel reduction](../benchmarks/WARM-BICUBIC-SINGLE-PIXEL.md) | 1,045.31 | 4,897.54 | 0.213 / 0.227 | 130 | Confirmed |
| [Bicubic resize RGBA8§](../benchmarks/WARM-BICUBIC-RGBA16-ROWS.md) | 711.85 | 415.44 | 1.713 / 1.728 | 332,177 | Confirmed |
| [Bicubic resize RGBA16§](../benchmarks/WARM-BICUBIC-RGBA16-ROWS.md) | 745.92 | 567.95 | 1.313 / 1.320 | 464,342 | Confirmed |
| [Bicubic upscale RGBA8§](../benchmarks/WARM-BICUBIC-UP8-ROWS.md) | 4,721.50 | 3,041.03 | 1.553 / 1.578 | 3,244,622 | Confirmed |
| [Bicubic upscale RGBA16§](../benchmarks/WARM-BICUBIC-UP16-ROWS.md) | 4,317.42 | 2,869.21 | 1.505 / 1.516 | 5,016,578 | Confirmed |
| [Bicubic mixed-axis RGBA8§](../benchmarks/WARM-BICUBIC-MIXED16-ROWS.md) | 1,570.03 | 849.57 | 1.848 / 1.867 | 1,283,063 | Confirmed |
| [Bicubic mixed-axis RGBA16§](../benchmarks/WARM-BICUBIC-MIXED16-ROWS.md) | 1,636.58 | 850.53 | 1.924 / 1.975 | 1,813,110 | Confirmed |
| [Bicubic width reduction / height enlargement RGBA8§](../benchmarks/WARM-BICUBIC-CROSS8-ROWS.md) | 984.18 | 827.04 | 1.190 / 1.206 | 731,753 | Confirmed |
| [Bicubic width reduction / height enlargement RGBA16§](../benchmarks/WARM-BICUBIC-CROSS16-ROWS.md) | 1,044.98 | 813.15 | 1.285 / 1.297 | 1,263,255 | Confirmed |
| [Bicubic RGBA8, 128×64 / wider support§](../benchmarks/WARM-BICUBIC-WIDE8-ROWS.md) | 588.69 | 360.96 | 1.631 / 1.642 | 231,227 | Confirmed |
| [Bicubic RGBA16, 128×64 / wider support§](../benchmarks/WARM-BICUBIC-WIDE16-ROWS.md) | 575.04 | 401.68 | 1.432 / 1.446 | 263,988 | Confirmed |
| [Bicubic width reduction, unchanged height RGBA8§](../benchmarks/WARM-BICUBIC-HORIZONTAL8-ROWS.md) | 858.24 | 559.04 | 1.535 / 1.558 | 465,375 | Confirmed |
| [Bicubic width reduction, unchanged height RGBA16§](../benchmarks/WARM-BICUBIC-HORIZONTAL16-ROWS.md) | 1,040.82 | 566.53 | 1.837 / 1.853 | 730,683 | Confirmed |
| [Bicubic width enlargement, unchanged height RGBA8§](../benchmarks/WARM-BICUBIC-HORIZONTAL-UP8-ROWS.md) | 2,594.78 | 1,502.85 | 1.727 / 1.742 | 1,930,602 | Confirmed |
| [Bicubic width enlargement, unchanged height RGBA16§](../benchmarks/WARM-CURRENT-PNG16-BICUBIC16.md) | 2,144.05 | 1,455.48 | 1.473 / 1.499 | 2,129,311 | Confirmed |
| [Bicubic RGBA8, 64×32 / support beyond 32§](../benchmarks/WARM-BICUBIC-64X32-RGBA8.md) | 637.23 | 351.87 | 1.811 / 1.820 | 205,880 | Confirmed |
| [Bicubic RGBA16, 64×32 / support beyond 32§](../benchmarks/WARM-BICUBIC-64X32-RGBA16.md) | 523.91 | 377.79 | 1.387 / 1.414 | 214,071 | Confirmed |
| [Bicubic RGBA8, 32×16 / support beyond 64§](../benchmarks/WARM-BICUBIC-32X16-RGBA8.md) | 465.61 | 393.50 | 1.183 / 1.192 | 199,354 | Confirmed |
| [Bicubic RGBA16, 32×16 / support beyond 64§](../benchmarks/WARM-BICUBIC-32X16-RGBA16.md) | 463.30 | 411.83 | 1.125 / 1.131 | 201,397 | Confirmed |
| [Bicubic RGBA8, 16×8 / adaptive support beyond 128§](../benchmarks/WARM-BICUBIC-16X8-ADAPTIVE.md) | 614.50 | 452.97 | 1.357 / 1.368 | 201,721 | Confirmed |
| [Bicubic RGBA16, 16×8 / adaptive support beyond 128§](../benchmarks/WARM-BICUBIC-16X8-RGBA16.md) | 465.90 | 489.16 | 0.952 / 0.958 | 202,229 | Confirmed |
| [Bicubic RGBA8, 256×129 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT16-STRIPS.md) | 775.79 | 418.23 | 1.855 / 1.876 | 132,295 | Confirmed |
| [Bicubic RGBA16, 256×129 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT16-STRIPS.md) | 828.06 | 567.99 | 1.458 / 1.467 | 264,469 | Confirmed |
| [Bicubic RGBA8, 128×64 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-WIDE8-STRIPS.md) | 586.27 | 348.88 | 1.680 / 1.691 | 32,889 | Confirmed |
| [Bicubic RGBA16, 128×64 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-WIDE16-STRIPS.md) | 602.40 | 414.67 | 1.453 / 1.470 | 65,658 | Confirmed |
| [Bicubic RGBA8, 64×32 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-64X32-RGBA8.md) | 635.44 | 353.77 | 1.796 / 1.818 | 8,313 | Confirmed |
| [Bicubic RGBA16, 64×32 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-64X32-RGBA16.md) | 622.50 | 389.09 | 1.600 / 1.624 | 16,505 | Confirmed |
| [Bicubic RGBA16, 32×16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-32X16-RGBA16.md) | 571.07 | 413.56 | 1.381 / 1.403 | 4,217 | Confirmed |
| [Bicubic RGBA8, 32×16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-32X16-RGBA8.md) | 591.75 | 376.53 | 1.572 / 1.578 | 2,169 | Confirmed |
| [Bicubic RGBA16, 16×8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-16X8-RGBA16.md) | 473.41 | 491.02 | 0.964 / 0.984 | 1,145 | Confirmed |
| [Bicubic RGBA8, 16×8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-16X8-RGBA8.md) | 493.22 | 455.04 | 1.084 / 1.103 | 633 | Confirmed |
| [Bicubic RGBA16 enlargement / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-HORIZONTAL-UP8.md) | 5,097.69 | 2,874.50 | 1.773 / 1.783 | 4,264,616 | Confirmed |
| [Bicubic RGBA8 enlargement / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-HORIZONTAL-UP8.md) | 5,412.68 | 3,035.41 | 1.783 / 1.795 | 2,493,057 | Confirmed |
| [Bicubic RGBA16, width up / height down / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-MIXED16.md) | 1,642.70 | 840.48 | 1.954 / 1.980 | 1,060,419 | Confirmed |
| [Bicubic RGBA8, width up / height down / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-MIXED8.md) | 1,636.25 | 862.74 | 1.897 / 1.907 | 530,370 | Confirmed |
| [Bicubic RGBA16, width down / height up / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-CROSS16.md) | 1,285.60 | 812.91 | 1.581 / 1.587 | 1,063,477 | Confirmed |
| [Bicubic RGBA8, width down / height up / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-CROSS8.md) | 1,268.02 | 842.90 | 1.504 / 1.526 | 531,910 | Confirmed |
| [Bicubic RGBA16, 8×4 / support beyond 256 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-8X4-RGBA16.md) | 439.90 | 692.95 | 0.635 / 0.642 | 377 | Confirmed |
| [Bicubic RGBA8, 8×4 / support beyond 256 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-8X4-RGBA8.md) | 505.31 | 637.83 | 0.792 / 0.803 | 249 | Confirmed |
| [Bicubic RGBA16, 4×2 / packed support beyond 304 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-4X2-RGBA16.md) | 637.19 | 1,247.45 | 0.511 / 0.518 | 185 | Confirmed |
| [Bicubic RGBA8, 4×2 / packed support beyond 304 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-4X2-RGBA8.md) | 665.48 | 1,098.32 | 0.606 / 0.618 | 153 | Confirmed |
| [Bicubic RGBA16, 2×2 / generated support beyond 544 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-2X2-RGBA16.md) | 1,039.52 | 2,217.15 | 0.469 / 0.493 | 154 | Confirmed |
| [Bicubic RGBA16, 2×9 / bounded row blocks / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-2X9-RGBA16.md) | 1,535.71 | 1,255.51 | 1.223 / 1.275 | 266 | Confirmed |
| [Bicubic RGBA16, 4×9 / packed row blocks / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-4X9-RGBA16.md) | 800.20 | 778.15 | 1.028 / 1.031 | 409 | Confirmed |
| [Bicubic RGBA16, 8×17 / full-coefficient row blocks / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-8X17-RGBA16.md) | 516.33 | 530.91 | 0.973 / 0.981 | 1,209 | Confirmed |
| [Bicubic RGBA8, 8×17 / full-coefficient row blocks / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-8X17-RGBA8.md) | 538.78 | 494.65 | 1.089 / 1.100 | 665 | Confirmed |
| [Bicubic RGBA8, 4×9 / packed row blocks / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-4X9-RGBA8.md) | 814.23 | 674.14 | 1.208 / 1.212 | 265 | Confirmed |
| [Bicubic RGBA8, 2×9 / bounded row blocks / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-2X9-RGBA8.md) | 1,355.20 | 1,017.22 | 1.332 / 1.341 | 194 | Confirmed |
| [Bicubic RGBA8, 2×2 / generated support beyond 544 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-2X2-RGBA8.md) | 1,040.64 | 1,690.30 | 0.616 / 0.634 | 138 | Confirmed |
| [Bicubic height reduction, unchanged width RGBA8§](../benchmarks/WARM-BICUBIC-TIGHT-VERTICAL8.md) | 756.93 | 588.11 | 1.287 / 1.301 | 648,992 | Confirmed |
| [Bicubic height reduction, unchanged width RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-VERTICAL8.md) | 1,063.64 | 583.82 | 1.822 / 1.838 | 264,995 | Confirmed |
| [Bicubic height reduction, unchanged width RGBA16§](../benchmarks/WARM-BICUBIC-TIGHT-VERTICAL16.md) | 832.49 | 585.00 | 1.423 / 1.428 | 913,737 | Confirmed |
| [Bicubic height reduction, unchanged width RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-VERTICAL16.md) | 1,059.29 | 584.28 | 1.813 / 1.823 | 529,838 | Confirmed |
| [Bicubic width reduction, unchanged height RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-HORIZONTAL16.md) | 848.00 | 564.36 | 1.503 / 1.510 | 530,858 | Confirmed |
| [Bicubic width reduction, unchanged height RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-HORIZONTAL8.md) | 811.19 | 573.66 | 1.414 / 1.425 | 265,502 | Confirmed |
| [Bicubic height enlargement, unchanged width RGBA8§](../benchmarks/WARM-BICUBIC-VERTICAL-UP8-ROWS.md) | 2,213.62 | 1,730.56 | 1.279 / 1.286 | 1,664,240 | Confirmed |
| [Bicubic height enlargement, unchanged width RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-VERTICAL-UP8.md) | 3,034.06 | 1,735.79 | 1.748 / 1.764 | 1,280,402 | Confirmed |
| [Bicubic height enlargement, unchanged width RGBA16§](../benchmarks/WARM-BICUBIC-VERTICAL-UP16-ROWS.md) | 2,131.66 | 1,655.60 | 1.288 / 1.297 | 2,516,605 | Confirmed |
| [Bicubic height enlargement, unchanged width RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-VERTICAL-UP16.md) | 2,688.28 | 1,661.27 | 1.618 / 1.636 | 2,133,096 | Confirmed |
| [Bicubic width enlargement, unchanged height RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-HORIZONTAL-UP16.md) | 2,313.25 | 1,440.17 | 1.606 / 1.614 | 2,129,311 | Confirmed |
| [Bicubic width enlargement, unchanged height RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-HORIZONTAL-UP8.md) | 2,387.65 | 1,508.44 | 1.583 / 1.591 | 1,178,425 | Confirmed |
| [Crop RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 2.37 | 2.27 | 1.042 / 1.053 | 96 | Confirmed |
| [Crop RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 4.11 | 4.30 | 0.955 / 0.963 | 96 | Confirmed |
| [Clone RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 9.50 | 7.95 | 1.195 / 1.209 | 96 | Confirmed |
| [Clone RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 19.06 | 17.02 | 1.120 / 1.141 | 96 | Confirmed |
| [Normal orientation RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 9.29 | 7.63 | 1.217 / 1.254 | 128 | Confirmed |
| [Normal orientation RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 18.58 | 16.70 | 1.112 / 1.132 | 128 | Confirmed |
| [Vertical flip RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 9.28 | 8.37 | 1.108 / 1.124 | 128 | Confirmed |
| [Vertical flip RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 17.68 | 17.39 | 1.017 / 1.036 | 128 | Confirmed |
| [Horizontal flip RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 11.64 | 9.52 | 1.223 / 1.235 | 128 | Confirmed |
| [Horizontal flip RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 21.83 | 27.03 | 0.807 / 0.811 | 128 | Confirmed |
| [Half turn RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 11.25 | 11.32 | 0.994 / 1.021 | 128 | Confirmed |
| [Half turn RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 18.59 | 21.86 | 0.850 / 0.874 | 128 | Confirmed |
| [Clockwise quarter turn RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 40.77 | 37.31 | 1.093 / 1.126 | 128 | Confirmed |
| [Clockwise quarter turn RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 43.89 | 43.93 | 0.999 / 1.045 | 128 | Confirmed |
| [Counterclockwise quarter turn RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 38.11 | 37.49 | 1.016 / 1.030 | 128 | Confirmed |
| [Counterclockwise quarter turn RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 49.42 | 45.49 | 1.086 / 1.103 | 128 | Confirmed |
| [Transpose RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 38.05 | 38.28 | 0.994 / 1.004 | 128 | Confirmed |
| [Transpose RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 48.17 | 44.86 | 1.074 / 1.114 | 128 | Confirmed |
| [Mirrored transpose RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 44.34 | 43.25 | 1.025 / 1.064 | 128 | Confirmed |
| [Mirrored transpose RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 39.10 | 38.54 | 1.015 / 1.041 | 128 | Confirmed |

† RGBA16 bilinear and the explicit RGBA8 upscale allow native-only alpha maximum
error 1 / RMSE <=0.25, alongside the same RGB bound. Up8 also includes writable
output acquisition and zero-alpha RGB clearing inside native timing. Managed
pixels remain exact against the rational reference; Down8 native alpha stays
exact. The RGBA16 independent confirmation clears 2x with both guards passing.
Earlier failed screens/prechecks and rejected prototypes remain recorded in
the linked reports.

‡ VP8 encode fixes native quality 75 / managed quality 100 at effort 4. Source
PSNR gap is 0.00365 dB within the unchanged 0.25 dB band; alpha is exact.
Managed output is 471,436 bytes versus 98,590 bytes. Quality-matched timing is
confirmed; compression efficiency parity is not established.

§ Bicubic RGBA8/16 uses **zimg, FFmpeg's zscale backend**, with timed four-pixel
clamped edge padding (4.697% additional input area); wider-support RGBA8/16 uses
eight pixels (9.489%); the 64×32 byte/word cases use sixteen (19.364%);
the 2×2 and 2×9 byte/word cases use 512 pixels (1384.169%);
the 4×2 and 4×9 byte/word cases use 256 pixels (494.787%);
the 8×4 and 8×17 byte/word cases use 128 pixels (198.069%);
the 32×16 byte/word cases use thirty-two (40.269%); the 16×8 byte/word cases use
sixty-four (86.703%). All include complete source/output
packing, filtering and independent output allocation/release. The explicit
native-only RGB/alpha maximum-1 / RMSE-0.25 bounds preserve exact managed pixels.
RGBA8 256×129 downsample scalar pixels are exact; normal SIMD differs by one RGB and one
alpha sample. Enlargement and mixed-axis scalar results are bounded too. RGBA16 normal/scalar results both stay within those bounds. This
comparison is distinct from the unmatched ordinary FFmpeg CLI.
For RGBA16 height enlargement with unchanged width only, the approved native RGB
bound is two samples where reference alpha is at most sixteen, one elsewhere.
RGB/alpha RMSE ≤0.25 and alpha maximum one remain unchanged; managed pixels are
exact. Negative controls reject errors outside this scope. The original strict
refusal and rejected conversion diagnostic remain unscored.
Bicubic enlargement, mixed-axis resize, wider-support downsampling and single-axis reduction use premultiplied float output and timed straight-RGBA
unpremultiplication/quantization, preserving unclamped alpha for the color ratio
and clearing RGB when rounded alpha is zero. Its packing uses equivalent bounded
quantization; the slower general-purpose-math comparison remains superseded.
The horizontal/vertical RGBA8 cases preserve frozen original binary64 pixels exactly;
six/twenty-five independently certified exact rational alpha halves retain the rounding
already permitted by SPEC.md. Vertical RGBA8 enlargement likewise preserves its
one independently certified exact rational blue half. No managed pixel tolerance is added.

Allocations are managed bytes per timed operation, from BenchmarkDotNet memory
diagnostics or raw GC allocated bytes / operation count (rounded up, including
harness overhead). Decoder rows use resident encoded spans unless labeled fresh Stream. The fresh
seekable PNG rows include input acquisition and copies for both subjects; their
linked reports define the full boundaries, including timed native PNG16 endian
conversion and fresh owned native-endian output. Separate fresh
public Stream controls measure Paeth8 **2,275,800**, Paeth16 **4,713,416**, and
Adam7-16 **5,058,912 B/op**, identically on both OSes with normal/scalar code.
These are distinct boundaries. FFmpeg allocation totals are **not measured**;
they must not be interpreted as zero. The composed PNG8 Stream/orient/crop/fit
row includes every managed intermediate and the complete native filter graph;
its output is 127x64. Other retained-owner and cold allocation patterns
need separate coverage.

The latest accepted production round isolates adaptive JPEG AC event planning
from sampling/transforms. Row-restart encoding improves from **2.001x** to
independently confirmed **1.799x / upper1.809x**, **10.4% less managed time**,
with unchanged **297,780 B/op** and exact whole packets. The nonrestart
regression mean is1.631x. One six-second profile, one changed core build and
unchanged harness/native workers suffice. Twenty-one focused normal/scalar
checks per OS, **3,483 Release tests per OS** including **898 live comparisons**,
and the consumer/all three examples through the same actual Release package
on Windows/Linux pass. No image, selector or settings grid is added.

Three existing cases subsequently confirm on that same qualified immutable
build: adaptive nonrestart JPEG420 at **1.634x / upper1.641x**, **297,780 B/op**;
PNG16 None decode at **1.045x / upper1.059x**, **8,552 B/op**; and RGBA16 bicubic
width enlargement at **1.473x / upper1.499x**, **2,129,311 B/op**. No production
or harness rebuild, profile, new fixture or repeated qualification is needed.
All twelve actuals and both quiet guards are retained for each. Earlier
near-2x results and the bicubic pre-guard refusal remain in linked evidence.
The existing adaptive JPEG420 RGBA16 source pair subsequently confirms at
**1.682x / upper1.697x**, **297,780 B/op**, including complete full-word alpha
flattening/normalization. It reuses the same qualified core/harness/native worker
and frozen packet, with no new selector, fixture, profile or repeated proofs.

The preceding production round widens the local baseline JPEG AC bit
cache, reducing a current-build **2.040x** RGB gap by **16.8%**, independently
confirming **1.695x / upper 1.709x** at **2,664,366 B/op**. Four checked ordinary
bytes supply several short coefficients per refill. Logical byte/bit state,
partial coefficients, work/error ordering and the original fallback remain
exact; no heap workspace is added. The same qualified build independently
confirms progressive 4:4:4 at **1.877x / upper 1.896x**, **2,682,364 B/op**, and fresh
JPEG Stream at **1.632x / upper 1.673x**, **1,813,266 B/op**.
The Stream worker now performs source-preservation validation outside timing.
Both regression means precede full **3,483-test-per-OS** qualification, including
**898 live comparisons**, and the same actual Release package's consumer plus
all three examples on Windows/Linux. Additional confirmations reuse those
proofs. No input variants or repeated production/harness builds are added.

The preceding RGB420/RGB422 rows retain their independent 1.638x/upper1.646x and
1.511x/upper1.520x results with 70.6% and 62.9% less managed time respectively.
Those measurements precede the current shared entropy primitive.

The [single-pixel bicubic comparison](../benchmarks/WARM-BICUBIC-SINGLE-PIXEL.md)
adds one unchanged-production RGBA8 reduction, independently confirmed at
**0.213x / upper 0.227x**, **130 B/op**, against the labeled zimg/FFmpeg zscale
backend. Managed/normal/scalar native samples are exact. Required rectangular
halo/packing/filtering/quantization/ownership work is timed. Existing full
qualification is reused; no production rebuild or profile is needed.

The [packed palette4 Adam7 comparison](../benchmarks/WARM-PNG-PALETTE4-ADAM7.md)
independently confirms **1.304x / upper 1.387x**, **2,592 B/op**, using the existing
original sixteen-entry palette projection. Managed/normal/scalar native pixels,
alpha and hidden RGB are exact. Complete packed decode, palette expansion,
interlaced placement and output ownership are timed. Production qualification
is reused; no production rebuild, profile or repeated package proof is needed.

The [RGBA8 SmallestSize comparison](../benchmarks/WARM-PNG-SMALLEST8.md)
independently confirms **0.835x / upper 0.841x**, **527,564 B/op**, with numeric
levels9/9, exact RGBA and the unchanged5% IDAT-size gate (209,314/210,283 bytes).
Production and package qualification are reused without a production rebuild,
profile or repeated proofs. The measured binaries remain immutable; a word-only
receipt-prose correction after measurement needs no repeat.

The [live-token opaque VP8 comparison](../benchmarks/WARM-VP8-CANCELABLE.md)
independently confirms **1.838x / upper 1.843x**, **760,548 B/op**, with exact
qualified managed pixels, opaque alpha and unchanged decoder bounds. Existing
frozen input, pre-cancellation/token identity, normal/scalar CLI and independent
owners are checked; token setup/disposal is outside timing. One harness build,
no production/native rebuild/profile or repeated qualification is required.

The [VP8 simple-filter comparison](../benchmarks/WARM-VP8-SIMPLE-FILTER.md)
independently confirms **1.746x / upper 1.765x**, **760,548 B/op**. One original
packet on the existing opacity projection independently verifies coded/effective
simple filtering at level8. Managed pixels/alpha, normal/scalar CLI anchors and
unchanged native agreement bounds pass. FFmpeg's decoder remains the baseline;
libwebp only authors the untimed fixture. Existing production qualification is
reused without a production/native decoder rebuild, profile or repeated proof.

The [indexed VP8L comparison](../benchmarks/WARM-VP8L-INDEXED16.md) independently
confirms **1.519x / upper 1.550x**, **1,351,961 B/op**, with exact RGBA/hidden RGB
through managed and normal/scalar native decoders. One 11,810-byte type3/sixteen-
entry packet reuses the existing PNG palette projection and exact=1 author.
FFmpeg's decoder remains the baseline; all inverse expansion/packing/ownership
is timed. Existing production/native builds and qualification are reused, with
one harness build and no profile or repeated proofs.

Recent unchanged-production coverage confirms RGB16 Paeth at1.904x /
upper1.928x,6,504 B/op, and gray-alpha8 Paeth at1.017x / upper1.068x,2,408 B/op.
Those rounds reuse the preceding immutable production qualification.

The palette8/tRNS Adam7 pair now independently confirms **1.167x / upper1.177x**,
with **4,960 B/op**, exact alpha/hidden RGB and both quiet guards passing.
Optional native process-CPU telemetry accompanies unchanged wall timing; CPU/wall
ratios0.99727–0.99915 show no substantial descheduling in the new observations.
The historical sixfold native slowdown remains unexplained and unscored.
One worker/harness build suffices; current production qualification is reused.

PNG RGB8/gray8/gray-alpha8 encoding uses managed level9/native6 with exact samples
and the unchanged 5% IDAT-size gate; earlier level6/level6 refusals remain unscored.
GIF and animation are deferred. Full required warm coverage remains **open**;
the [material coverage audit](../benchmarks/WARM-COVERAGE.md) tracks remaining
layouts and public boundaries. Stop confirmed pairs; rolling older-source results
do not prove uniform current-source parity. Linked reports preserve superseded
evidence, failed guards, rejected trials and exact package/source qualification.

Maintain this summary whenever a performance round is accepted: update the
corresponding evidence selection in [the renderer](../benchmarks/render-benchmark.py),
regenerate with `python benchmarks/render-benchmark.py`, and run it with `--check`.
Preserve superseded raw evidence and failed runs in their linked reports. Add a
new row only for material coverage; prioritize the largest remaining gap one
primitive at a time. See the [measurement recipe](../tools/WarmReference/README.md).
