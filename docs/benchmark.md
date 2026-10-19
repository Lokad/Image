# Lokad.Image benchmark state

Updated 2026-10-19. Target: **warm single-CPU parity, independently confirmed
conservative upper ≤1x**. The 225 current numeric rows contain **70 passes,
154 gaps and 1 confidence-open case**. The finite
[material checklist](../benchmarks/WARM-COVERAGE.md) has 247 named cases, 22 unmeasured.
The completed ≤2x phase belongs to 0.1.0; current work targets parity. Results apply
to the named fixed inputs/settings/hardware, without extrapolating to other profiles.

[Exact SIMD caller YCCK RGB](../benchmarks/WARM-JPEG-YCCK-CALLER-SIMD.md) cuts
complete time **80.2%**, 4,532.59→898.47µs: **1.488x / upper 1.550x**,
**1,337,490 B/op**. Shared owned YCCK improves20.3% to1.163x/1,470,256B/op;
caller CMYK control remains1.592x. Exact managed pixels, approved native bounds,
resource limits and complete independent output ownership stay unchanged.
Three rows refresh;222other numeric rows and all70passing proofs are reused.

Accepted qualification:4,209tests and96hardware-off checks per OS, plus the same
actual private package consumer/all three examples on Windows/Linux. The released
0.1.0 package/tag remain immutable. Current largest gap:CMYK2D sparse1.812x.
The22unmeasured representatives include JPEG411/410, async, header, drawing and
text boundaries; functional qualification does not supply warm timing evidence.
Stop unchanged independently passing paths; no image/settings grid was added.

JPEG caller-sample matched-quality packets are **58,823/58,910 B** grayscale
(managed/SIMD libjpeg-turbo) and **74,040/52,093 B** RGB420 (managed/FFmpeg), with
source-PSNR gaps 0.000033/0.106110 dB inside 0.25 dB. Equal quality numbers are not the
matching rule; [frozen comparisons](../benchmarks/WARM-JPEG-CALLER-MCU.md) include
complete time/allocations. WebP photograph/diagram lossless sizes are 383,154/160,032 B
and 15,120/7,234 B; [lossy sizes](../benchmarks/WARM-VP8-ADAPTIVE-PROBABILITIES.md)
are 21,432/17,688 B and 8,288/7,100 B at the fixed qualified source quality. Compression-
efficiency parity is not established. Current operation timings/allocations follow.

Dedicated AMD EPYC9V45 VM; Ubuntu24.04.4; .NET10.0.8/SDK10.0.204;
FFmpeg n8.1 (`9047fa1b084f`). One pinned CPU, one codec/conversion/filter thread,
normal SIMD, tiering off, nonconcurrent workstation GC. Inputs are the two original
513×259 RGBA8/16 rasters, the fixed photograph/text diagram pair and documented projections; exact case dimensions and
policies are in linked reports. Only the two required content controls were added; no settings grid.

Times include complete public operations, admission, conversion, fresh independent
output ownership and disposal. Native contexts may be reused; managed private
storage may be reused after release. No decoded output or descriptor is cached.
Fresh Stream rows include source-wrapper/input acquisition, reads/copies/EOF and
release. Startup, I/O, IPC, preparation/hashing and validation are excluded.

Ratio = Lokad.Image / faster native pre/post mean; lower is faster. Upper retains
the larger selected/all-actual confidence bound against the faster native bracket's
lower bound. Parity confirmed requires an independent quiet repeat with upper≤1;
a mean≤1 with upper>1 stays confidence-open. Gap rows exceed the new target.
Independent jobs retain all 12 actuals; explicitly labeled short screens retain
all three. Both retain 5s `exclusive-vm-idle-v2` guards. Earlier records
retain their original protocol and failures. **B/op** is managed allocation from
BenchmarkDotNet/raw GC bytes per operation, rounded up. Native allocations are
**unmeasured**, not zero. Timings were collected across rounds; unchanged-path
reconciliation now covers every numeric row.

**Decode and metadata**

| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [PNG Identify, fresh Stream¶](../benchmarks/WARM-PNG-STREAM-IDENTIFY.md) | 22.94 | 15.87 | 1.445 / 1.459 | 184 | Gap (independent) |
| [WebP Identify, resident span (parser)](../benchmarks/WARM-WEBP-IDENTIFY.md) | 0.02 | 4.54 | 0.004 / 0.004 | 88 | Parity confirmed |
| [JPEG Identify, resident span‖](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 0.59 | 1.21 | 0.488 / 0.494 | 88 | Parity confirmed |
| [PNG Identify, resident span¶](../benchmarks/WARM-PNG-IDENTIFY.md) | 16.09 | 13.04 | 1.234 / 1.238 | 88 | Gap (independent) |
| [PNG palette4/full tRNS Adam7](../benchmarks/WARM-PNG-PALETTE4-ADAM7.md) | 1,480.35 | 1,135.35 | 1.304 / 1.387 | 2,592 | Gap (independent) |
| [PNG8 Stream, orient/crop/nearest fit](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 2,044.52 | 1,476.97 | 1.384 / 1.613 | 701,312 | Gap (short; qualified) |
| [PNG16 Stream, orient/crop/nearest fit](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 3,355.49 | 2,946.76 | 1.139 / 1.213 | 1,401,314 | Gap (short; qualified) |
| [PNG RGBA8, fresh seekable Stream](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 1,918.97 | 1,393.95 | 1.377 / 1.583 | 4,552 | Gap (short; qualified) |
| [PNG RGBA16, fresh seekable Stream](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 3,054.27 | 3,091.94 | 0.988 / 0.991 | 8,648 | Parity confirmed |
| [JPEG420 restart, fresh seekable Stream](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 1,377.94 | 991.96 | 1.389 / 1.473 | 761,816 | Gap (short; qualified) |
| [WebP VP8 + alpha, fresh seekable Stream](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 9,381.96 | 6,007.84 | 1.562 / 1.793 | 1,966,712 | Gap (short; qualified) |
| [WebP VP8L, fresh seekable Stream](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 4,320.01 | 3,018.37 | 1.431 / 1.494 | 1,216,276 | Gap (short; qualified) |
| [PNG RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 1,904.89 | 1,392.72 | 1.368 / 1.371 | 4,456 | Gap (independent) |
| [PNG RGBA8, resident / shared budget](../benchmarks/WARM-SHARED-BUDGET.md) | 2,004.67 | 1,392.79 | 1.439 / 1.482 | 4,561 | Gap (short; qualified) |
| [PNG RGBA8, None / live cancelable token](../benchmarks/WARM-PNG-CANCEL-BLOCKS.md) | 1,732.37 | 1,390.42 | 1.246 / 1.320 | 4,456 | Gap (short; qualified) |
| [PNG RGBA8, None / exact working limit](../benchmarks/WARM-PNG-STREAMING-ROWS.md) | 2,028.73 | 1,395.91 | 1.453 / 1.597 | 68,200 | Gap (short; qualified) |
| [PNG RGBA8, None / exact limit + live cancelable token](../benchmarks/WARM-PNG-STREAMING-ROWS.md) | 2,067.97 | 1,397.05 | 1.480 / 1.826 | 68,200 | Gap (short; qualified) |
| [PNG RGBA8, stored Deflate / None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 150.38 | 136.49 | 1.102 / 1.114 | 4,456 | Gap (independent) |
| [PNG RGBA16, stored Deflate / None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 322.73 | 296.18 | 1.090 / 1.096 | 8,552 | Gap (independent) |
| [PNG RGBA16, None](../benchmarks/WARM-CURRENT-PNG16-BICUBIC16.md) | 3,034.16 | 2,903.68 | 1.045 / 1.059 | 8,552 | Gap (independent) |
| [PNG RGBA16, None / exact working limit](../benchmarks/WARM-PNG-STREAMING-ROWS.md) | 3,103.58 | 2,902.28 | 1.069 / 1.242 | 70,248 | Gap (short; qualified) |
| [PNG RGBA16, None / live cancelable token](../benchmarks/WARM-PNG-CANCEL-BLOCKS.md) | 3,068.88 | 2,897.86 | 1.059 / 1.108 | 8,552 | Gap (short; qualified) |
| [PNG RGBA16, None / exact limit + live cancelable token](../benchmarks/WARM-PNG-STREAMING-ROWS.md) | 3,120.43 | 2,904.38 | 1.074 / 1.145 | 70,248 | Gap (short; qualified) |
| [PNG RGBA8, Paeth](../benchmarks/WARM-PAETH-SIX-CHANNEL.md) | 3,149.31 | 2,847.74 | 1.106 / 1.128 | 4,456 | Gap (short; qualified) |
| [PNG RGBA8, Up](../benchmarks/WARM-PNG-UP-SIMD.md) | 1,484.65 | 1,509.16 | 0.984 / 0.989 | 4,456 | Parity confirmed |
| [PNG RGBA16, Up](../benchmarks/WARM-PNG-UP-SIMD.md) | 1,902.38 | 1,911.94 | 0.995 / 0.997 | 8,552 | Parity confirmed |
| [PNG RGBA8, Sub](../benchmarks/WARM-PNG-SUB-PREFIX.md) | 584.20 | 686.53 | 0.851 / 0.855 | 4,456 | Parity confirmed |
| [PNG RGBA16, Sub](../benchmarks/WARM-PNG-SUB16.md) | 357.99 | 743.02 | 0.482 / 0.493 | 8,552 | Parity confirmed |
| [PNG RGBA8, Average](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,380.32 | 1,850.31 | 1.286 / 1.298 | 4,456 | Gap (independent) |
| [PNG RGBA16, Average](../benchmarks/WARM-PNG-AVERAGE16.md) | 5,894.17 | 4,384.37 | 1.344 / 1.357 | 8,552 | Gap (independent) |
| [PNG RGBA16, Paeth](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 5,401.14 | 5,511.74 | 0.980 / 0.990 | 8,552 | Parity confirmed |
| [PNG RGB8, Paeth](../benchmarks/WARM-PAETH-SIX-CHANNEL.md) | 3,140.69 | 2,632.59 | 1.193 / 1.241 | 3,432 | Gap (short; qualified) |
| [PNG RGB16, Paeth](../benchmarks/WARM-PAETH-SIX-CHANNEL.md) | 4,290.59 | 4,472.22 | 0.959 / 0.969 | 6,504 | Parity confirmed |
| [PNG gray8, Paeth](../benchmarks/WARM-PNG-GRAY8-PAETH.md) | 1,038.34 | 1,029.60 | 1.008 / 1.014 | 1,384 | Gap (independent) |
| [PNG gray-alpha8, Paeth](../benchmarks/WARM-PNG-GRAY-ALPHA8-PAETH.md) | 2,051.11 | 2,016.66 | 1.017 / 1.068 | 2,408 | Gap (independent) |
| [PNG RGBA16, Adam7](../benchmarks/WARM-CURRENT-PNG-ADAM7.md) | 5,393.13 | 3,917.00 | 1.377 / 1.381 | 23,600 | Gap (independent) |
| [PNG gray1 → RGBA8, Adam7](../benchmarks/WARM-CURRENT-PNG-ADAM7.md) | 1,512.17 | 1,150.81 | 1.314 / 1.320 | 1,424 | Gap (independent) |
| [PNG palette8 + tRNS, Adam7](../benchmarks/WARM-PNG-PALETTE8-ADAM7.md) | 1,710.03 | 1,465.40 | 1.167 / 1.177 | 4,960 | Gap (independent) |
| [PNG RGB16, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,297.09 | 2,227.02 | 1.031 / 1.034 | 6,504 | Gap (independent) |
| [PNG gray8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 357.47 | 363.27 | 0.984 / 0.993 | 1,384 | Parity confirmed |
| [PNG gray8 + tRNS key127, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 357.50 | 758.36 | 0.471 / 0.474 | 1,416 | Parity confirmed |
| [PNG gray16 + tRNS key32639, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,970.49 | 2,055.01 | 1.445 / 1.450 | 2,440 | Gap (independent) |
| [PNG gray-alpha8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 845.98 | 1,156.04 | 0.732 / 0.733 | 2,408 | Parity confirmed |
| [PNG RGB8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 1,293.31 | 1,523.89 | 0.849 / 0.853 | 3,432 | Parity confirmed |
| [PNG RGB8 + tRNS key(48,139,231), None](../benchmarks/WARM-PNG-RGB-TRANSPARENCY-CONFIRM.md) | 1,302.04 | 1,286.75 | 1.012 / 1.014 | 3,464 | Gap (independent) |
| [PNG RGB16 + tRNS key(12347,35806,59265), None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,316.17 | 3,182.24 | 0.728 / 0.732 | 6,536 | Parity confirmed |
| [PNG gray16, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 714.81 | 957.95 | 0.746 / 0.751 | 2,408 | Parity confirmed |
| [PNG gray-alpha16, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 1,563.09 | 1,795.59 | 0.871 / 0.874 | 4,456 | Parity confirmed |
| [PNG byte indices / sixteen-entry palette + tRNS, None](../benchmarks/WARM-PNG-SHORT-PALETTE.md) | 144.22 | 109.47 | 1.317 / 1.327 | 1,496 | Gap (independent) |
| [PNG palette8 + tRNS, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 349.92 | 364.06 | 0.961 / 0.970 | 2,456 | Parity confirmed |
| [PNG palette8 + two-entry tRNS, None](../benchmarks/WARM-PNG-PARTIAL-ALPHA.md) | 360.10 | 365.80 | 0.984 / 0.992 | 2,208 | Parity confirmed |
| [PNG palette4 + tRNS → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 109.81 | 140.53 | 0.781 / 0.799 | 984 | Parity confirmed |
| [PNG palette2 + tRNS → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 66.42 | 97.53 | 0.681 / 0.686 | 688 | Parity confirmed |
| [PNG palette1 + tRNS → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 54.06 | 88.92 | 0.608 / 0.611 | 552 | Parity confirmed |
| [PNG opaque gray1 → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 59.11 | 303.42 | 0.195 / 0.199 | 488 | Parity confirmed |
| [PNG gray1 + tRNS key1 → RGBA8, exact timed FFmpeg color-key](../benchmarks/WARM-PNG-GRAY1-TRANSPARENCY.md) | 64.68 | 684.86 | 0.094 / 0.098 | 520 | Parity confirmed |
| [PNG gray2 + tRNS key0 → RGBA8, None](../benchmarks/WARM-PNG-PACKED-TRANSPARENCY.md) | 71.49 | 497.90 | 0.144 / 0.145 | 648 | Parity confirmed |
| [PNG gray1/key1 and gray2/key1, unfiltered native](../benchmarks/WARM-PNG-PACKED-TRANSPARENCY.md) | — | — | — | — | Unscored: native alpha differs |
| [PNG opaque gray2 → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 71.73 | 106.38 | 0.674 / 0.679 | 616 | Parity confirmed |
| [PNG opaque gray4 → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 116.09 | 169.54 | 0.685 / 0.698 | 872 | Parity confirmed |
| [JPEG baseline gray](../benchmarks/WARM-JPEG-GRAY-CURRENT.md) | 973.44 | 682.79 | 1.426 / 1.459 | 672,186 | Gap (short; qualified) |
| [JPEG gray → caller GRAY8](../benchmarks/WARM-JPEG-FOUR-COMPONENT-PLANES.md) | 868.77 | 646.09 | 1.345 / 1.411 | 273,422 | Gap (short; qualified) |
| [JPEG untagged → caller CMYK8](../benchmarks/WARM-JPEG-YCCK-CALLER-SIMD.md) | 526.74 | 330.84 | 1.592 / 1.669 | 1,635,479 | Gap (short; qualified) |
| [JPEG 4:2:0 → caller RGB24](../benchmarks/WARM-JPEG-FOUR-COMPONENT-PLANES.md) | 1,121.94 | 1,166.50 | 0.962 / 0.966 | 624,508 | Parity confirmed |
| [JPEG Adobe CMYK, four full-resolution planes](../benchmarks/WARM-JPEG-YCCK-SIMD.md) | 525.44 | 377.75 | 1.391 / 1.473 | 1,085,667 | Gap (short; qualified) |
| [JPEG Adobe CMYK, one AC control](../benchmarks/WARM-JPEG-YCCK-SIMD.md) | 674.71 | 389.55 | 1.732 / 1.783 | 1,085,662 | Gap (short; qualified) |
| [JPEG Adobe CMYK, general transform control](../benchmarks/WARM-JPEG-YCCK-SIMD.md) | 3,463.08 | 3,110.84 | 1.113 / 1.197 | 1,085,522 | Gap (short; qualified) |
| [JPEG Adobe CMYK, two-dimensional sparse control](../benchmarks/WARM-JPEG-YCCK-SIMD.md) | 792.76 | 437.47 | 1.812 / 1.869 | 1,085,656 | Gap (short; qualified) |
| [JPEG YCCK420, full-resolution black](../benchmarks/WARM-JPEG-YCCK-CALLER-SIMD.md) | 824.27 | 708.72 | 1.163 / 1.175 | 1,470,256 | Gap (short; qualified) |
| [JPEG YCCK420 → caller RGB24](../benchmarks/WARM-JPEG-YCCK-CALLER-SIMD.md) | 898.47 | 603.97 | 1.488 / 1.550 | 1,337,490 | Gap (short; qualified) |
| [JPEG baseline 4:2:0](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 1,202.77 | 1,008.96 | 1.192 / 1.315 | 757,345 | Gap (short; qualified) |
| [JPEG baseline 4:2:0, row restart markers](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 1,378.07 | 1,014.38 | 1.359 / 1.446 | 761,720 | Gap (short; qualified) |
| [JPEG baseline 4:2:2](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 1,510.49 | 1,351.29 | 1.118 / 1.263 | 829,148 | Gap (short; qualified) |
| [JPEG baseline 4:4:4, owned input](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 2,522.16 | 1,902.72 | 1.326 / 1.431 | 948,023 | Gap (short; qualified) |
| [JPEG baseline 4:4:4, native input](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 2,300.39 | 1,719.44 | 1.338 / 1.418 | 966,317 | Gap (short; qualified) |
| [JPEG baseline direct RGB, full resolution](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 2,193.44 | 1,452.84 | 1.510 / 1.656 | 966,317 | Gap (short; qualified) |
| [JPEG direct RGB 4:2:2, timed centered correction](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 1,472.21 | 1,094.02 | 1.346 / 1.509 | 829,148 | Gap (short; qualified) |
| [JPEG direct RGB 4:2:0, timed component-plane interpretation](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 1,073.37 | 743.41 | 1.444 / 1.518 | 757,346 | Gap (short; qualified) |
| [JPEG baseline 4:4:4, native input / live cancelable token](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 2,392.14 | 1,720.11 | 1.391 / 1.482 | 966,317 | Gap (short; qualified) |
| [JPEG progressive gray](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 2,647.80 | 1,712.34 | 1.546 / 1.634 | 683,138 | Gap (short; qualified) |
| [JPEG progressive 4:2:0](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 2,438.11 | 1,947.56 | 1.252 / 1.446 | 775,336 | Gap (short; qualified) |
| [JPEG progressive 4:4:4 / live cancelable token](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 4,960.36 | 3,350.73 | 1.480 / 1.614 | 984,298 | Gap (short; qualified) |
| [JPEG progressive 4:2:2](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 3,071.46 | 2,238.43 | 1.372 / 1.508 | 847,162 | Gap (short; qualified) |
| [JPEG progressive 4:4:4](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 5,037.72 | 3,361.01 | 1.499 / 1.715 | 984,298 | Gap (short; qualified) |
| [WebP VP8 + alpha](../benchmarks/WARM-VP8L-SIMD-PREDICTION.md) | 9,272.91 | 5,970.23 | 1.553 / 1.648 | 1,966,616 | Gap (short; qualified) |
| [WebP VP8 opaque, simple luma filter](../benchmarks/WARM-VP8-BLOCK-PREDICTION.md) | 7,807.89 | 4,893.93 | 1.595 / 1.621 | 760,548 | Gap (short; qualified) |
| [WebP VP8 opaque, live cancelable token](../benchmarks/WARM-VP8-BLOCK-PREDICTION.md) | 7,613.64 | 4,801.66 | 1.586 / 1.664 | 760,548 | Gap (short; qualified) |
| [WebP VP8, opaque](../benchmarks/WARM-VP8-BLOCK-PREDICTION.md) | 7,568.84 | 4,798.45 | 1.577 / 1.672 | 760,548 | Gap (short; qualified) |
| [WebP VP8L, sixteen-color indexed input](../benchmarks/WARM-VP8L-SIMD-PREDICTION.md) | 785.58 | 544.96 | 1.442 / 1.495 | 1,351,972 | Gap (short; qualified) |
| [WebP VP8L, literal/run input](../benchmarks/WARM-VP8L-SIMD-PREDICTION.md) | 2,269.12 | 2,157.09 | 1.052 / 1.074 | 1,074,326 | Gap (short; qualified) |
| [WebP VP8L, native input](../benchmarks/WARM-VP8L-SIMD-PREDICTION.md) | 4,576.04 | 3,022.99 | 1.514 / 1.615 | 1,216,180 | Gap (short; qualified) |
| [WebP VP8L, native input / live cancelable token](../benchmarks/WARM-VP8L-SIMD-PREDICTION.md) | 4,573.29 | 3,016.50 | 1.516 / 1.579 | 1,216,180 | Gap (short; qualified) |

**Encode**

| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [PNG RGBA8, level 6 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 5,406.30 | 17,266.72 | 0.313 / 0.315 | 527,627 | Parity confirmed |
| [PNG RGBA8, NoCompression / None](../benchmarks/WARM-PNG-STORED-CAPACITY.md) | 245.37 | 189.47 | 1.295 / 1.430 | 604,319 | Gap (short; qualified) |
| [PNG RGBA16, NoCompression / None](../benchmarks/WARM-PNG-STORED-CAPACITY.md) | 349.92 | 406.43 | 0.861 / 0.870 | 1,140,504 | Parity confirmed |
| [PNG RGBA8, Fastest managed2/native1 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 3,053.16 | 4,355.98 | 0.701 / 0.704 | 527,694 | Parity confirmed |
| [PNG RGBA16, Fastest managed2/native1 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 13,289.83 | 17,822.40 | 0.746 / 0.776 | 2,103,738 | Parity confirmed |
| [PNG RGBA16, Fastest managed2/native1 / live cancelable token](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 12,846.40 | 17,513.32 | 0.734 / 0.743 | 2,103,738 | Parity confirmed |
| [PNG RGBA16, Fastest managed2/native1 / forward-only output](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 12,790.89 | 17,465.07 | 0.732 / 0.740 | 2,103,770 | Parity confirmed |
| [PNG RGBA16, level 6 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 14,604.45 | 20,095.18 | 0.727 / 0.731 | 2,103,624 | Parity confirmed |
| [PNG RGBA8, SmallestSize9 / native9](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 14,486.25 | 17,324.40 | 0.836 / 0.843 | 527,564 | Parity confirmed |
| [PNG RGBA16, SmallestSize managed9/native9 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 19,610.01 | 20,110.26 | 0.975 / 0.980 | 2,103,522 | Parity confirmed |
| [PNG RGB8, Optimal managed9/native6 / None](../benchmarks/WARM-PNG-BYTE-PACKING.md) | 8,712.51 | 9,507.55 | 0.916 / 0.922 | 527,032 | Parity confirmed |
| [PNG RGB16, level 6 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 13,163.23 | 15,313.53 | 0.860 / 0.863 | 2,102,732 | Parity confirmed |
| [PNG gray8, Optimal managed9/native6 / None](../benchmarks/WARM-PNG-BYTE-PACKING.md) | 1,871.37 | 1,934.46 | 0.967 / 0.974 | 263,699 | Parity confirmed |
| [PNG gray16, level 6 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 5,762.37 | 4,913.26 | 1.173 / 1.246 | 1,051,421 | Gap (short; qualified) |
| [PNG gray-alpha8, Optimal managed9/native6 / None](../benchmarks/WARM-PNG-BYTE-PACKING.md) | 4,174.10 | 4,289.79 | 0.973 / 0.980 | 264,179 | Parity confirmed |
| [PNG gray-alpha16, level 6 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 9,426.43 | 10,150.04 | 0.929 / 0.933 | 1,052,317 | Parity confirmed |
| [JPEG 4:4:4, adaptive Huffman / matched quality](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 3,894.69 | 3,771.76 | 1.033 / 1.115 | 559,418 | Gap (short; qualified) |
| [JPEG 4:4:4, forward-only output / geometric AC Huffman](../benchmarks/WARM-JPEG-FIXED-FIELDS.md) | 4,379.70 | 3,778.08 | 1.159 / 1.353 | 526,029 | Gap (short; qualified) |
| [JPEG 4:4:4, live cancelable token / geometric AC Huffman](../benchmarks/WARM-JPEG-FIXED-FIELDS.md) | 4,465.15 | 3,775.40 | 1.183 / 1.364 | 525,997 | Gap (short; qualified) |
| [JPEG 4:4:4 from RGBA16, adaptive Huffman / full normalization](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 3,980.90 | 3,855.74 | 1.032 / 1.135 | 556,284 | Gap (short; qualified) |
| [JPEG 4:2:2, adaptive Huffman / matched quality](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 2,579.93 | 2,354.66 | 1.096 / 1.227 | 294,116 | Gap (short; qualified) |
| [JPEG 4:2:2 from RGBA16, adaptive Huffman / full normalization](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 2,633.91 | 2,421.91 | 1.088 / 1.220 | 294,116 | Gap (short; qualified) |
| [JPEG 4:2:0, adaptive Huffman / matched quality](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 2,022.21 | 1,766.91 | 1.144 / 1.247 | 297,781 | Gap (short; qualified) |
| [JPEG 4:2:0, shared budget / matched quality](../benchmarks/WARM-SHARED-BUDGET.md) | 2,026.04 | 1,769.43 | 1.145 / 1.272 | 297,781 | Gap (short; qualified) |
| [JPEG 4:2:0, adaptive / row restart markers](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 2,019.53 | 1,611.25 | 1.253 / 1.455 | 297,781 | Gap (short; qualified) |
| [JPEG 4:2:0, forward-only output / geometric AC Huffman](../benchmarks/WARM-JPEG-FIXED-FIELDS.md) | 2,310.38 | 1,769.17 | 1.306 / 1.488 | 263,819 | Gap (short; qualified) |
| [JPEG 4:2:0 from RGBA16, forward-only output](../benchmarks/WARM-JPEG-FIXED-FIELDS.md) | 2,393.98 | 1,839.60 | 1.301 / 1.415 | 263,819 | Gap (short; qualified) |
| [JPEG 4:2:0 from RGBA16, live cancelable token](../benchmarks/WARM-JPEG-FIXED-FIELDS.md) | 2,444.30 | 1,841.37 | 1.327 / 1.443 | 263,787 | Gap (short; qualified) |
| [JPEG 4:2:0 from RGBA16, adaptive Huffman](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 2,173.56 | 1,841.00 | 1.181 / 1.300 | 297,779 | Gap (short; qualified) |
| [JPEG grayscale, standalone SIMD libjpeg-turbo](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 1,350.95 | 1,404.64 | 0.962 / 0.970 | 154,892 | Parity confirmed |
| [JPEG from caller gray samples, standalone SIMD libjpeg-turbo](../benchmarks/WARM-JPEG-CALLER-MCU.md) | 1,252.22 | 1,077.49 | 1.162 / 1.422 | 155,007 | Gap (short; qualified) |
| [JPEG 4:2:0 from caller RGB samples](../benchmarks/WARM-JPEG-CALLER-MCU.md) | 2,097.34 | 1,579.06 | 1.328 / 1.408 | 297,894 | Gap (short; qualified) |
| [WebP VP8, opaque‡](../benchmarks/WARM-VP8-ADAPTIVE-PROBABILITIES.md) | 20,420.05 | 15,471.74 | 1.320 / 1.446 | 1,482,092 | Gap (short; qualified) |
| [WebP VP8, opaque from RGBA16‡](../benchmarks/WARM-VP8-ADAPTIVE-PROBABILITIES.md) | 20,641.94 | 15,607.33 | 1.323 / 1.375 | 1,481,928 | Gap (short; qualified) |
| [WebP VP8 + alpha‡](../benchmarks/WARM-VP8-ADAPTIVE-PROBABILITIES.md) | 20,670.47 | 53,317.03 | 0.388 / 0.391 | 2,109,290 | Parity confirmed |
| [WebP VP8 + alpha from RGBA16‡](../benchmarks/WARM-VP8-ADAPTIVE-PROBABILITIES.md) | 21,352.42 | 53,437.67 | 0.400 / 0.403 | 2,108,982 | Parity confirmed |
| [WebP VP8 + alpha‡, caller work budget32M](../benchmarks/WARM-VP8-ADAPTIVE-PROBABILITIES.md) | 22,866.92 | 53,563.20 | 0.427 / 0.430 | 2,616,016 | Parity confirmed |
| [WebP VP8L, exact backend*](../benchmarks/WARM-VP8L-PREVIOUS-ROW.md) | 9,878.57 | 122,155.83 | 0.081 / 0.082 | 2,100,170 | Parity confirmed |
| [WebP VP8L, opaque RGBA8 / exact backend*](../benchmarks/WARM-VP8L-PREVIOUS-ROW.md) | 9,198.47 | 70,847.86 | 0.130 / 0.132 | 1,051,213 | Parity confirmed |
| [WebP VP8L from RGBA16, exact backend*](../benchmarks/WARM-VP8L-PREVIOUS-ROW.md) | 11,972.58 | 121,563.39 | 0.098 / 0.099 | 2,100,119 | Parity confirmed |
| [WebP VP8L photograph RGBA8](../benchmarks/WARM-VP8L-PREVIOUS-ROW.md) | 9,279.32 | 82,847.01 | 0.112 / 0.113 | 1,051,162 | Parity confirmed |
| [WebP VP8L text/diagram RGBA8](../benchmarks/WARM-VP8L-PREVIOUS-ROW.md) | 2,713.60 | 9,769.91 | 0.278 / 0.284 | 34,928 | Parity confirmed |
| [WebP VP8 photograph RGBA8 / matched quality](../benchmarks/WARM-VP8-ADAPTIVE-PROBABILITIES.md) | 7,643.09 | 9,722.18 | 0.786 / 0.790 | 558,520 | Parity confirmed |
| [WebP VP8 text/diagram RGBA8 / matched quality](../benchmarks/WARM-VP8-ADAPTIVE-PROBABILITIES.md) | 5,842.06 | 5,840.84 | 1.000 / 1.009 | 489,932 | Gap (independent) |

**Editing**

| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [Horizontal flip RGBA8, two live outputs / per image](../benchmarks/WARM-HORIZONTAL-FLIP512.md) | 9.56 | 9.08 | 1.053 / 1.142 | 128 | Gap (short; qualified) |
| [Nearest resize RGBA8](../benchmarks/WARM-COVERAGE-QUIET.md) | 58.67 | 252.66 | 0.232 / 0.241 | 132,294 | Parity confirmed |
| [Nearest upscale RGBA8](../benchmarks/WARM-NEAREST-UP8.md) | 935.05 | 1,006.00 | 0.929 / 0.955 | 2,133,091 | Parity confirmed |
| [Nearest upscale RGBA16](../benchmarks/WARM-NEAREST-UP16.md) | 1,071.13 | 1,019.06 | 1.051 / 1.095 | 4,265,431 | Gap (independent) |
| [Nearest resize RGBA16](../benchmarks/WARM-PREPARED-CONFIRM.md) | 105.69 | 170.58 | 0.620 / 0.629 | 264,466 | Parity confirmed |
| [Bilinear resize RGBA8](../benchmarks/WARM-BILINEAR-BYTE-ROW-CACHE.md) | 500.71 | 387.70 | 1.291 / 1.406 | 157,960 | Gap (short; qualified) |
| [Bilinear resize RGBA16†](../benchmarks/WARM-BILINEAR-WORD-CACHE.md) | 534.35 | 418.51 | 1.277 / 1.336 | 290,127 | Gap (short; qualified) |
| [Bilinear upscale RGBA8†](../benchmarks/WARM-BILINEAR-BYTE-FACTORIZATION.md) | 3,138.47 | 2,155.65 | 1.456 / 1.549 | 2,235,599 | Gap (short; qualified) |
| [Bilinear upscale RGBA16†](../benchmarks/WARM-CURRENT-BILINEAR.md) | 2,563.23 | 1,869.40 | 1.371 / 1.382 | 4,498,974 | Gap (independent) |
| [RGBA8 bicubic, single-pixel reduction](../benchmarks/WARM-BICUBIC-SINGLE-PIXEL.md) | 1,045.31 | 4,897.54 | 0.213 / 0.227 | 130 | Parity confirmed |
| [Bicubic resize RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 635.08 | 416.65 | 1.524 / 1.610 | 332,178 | Gap (short; qualified) |
| [Bicubic resize RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 729.05 | 571.10 | 1.277 / 1.715 | 464,343 | Gap (short; qualified) |
| [Bicubic upscale RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 3,919.91 | 3,036.45 | 1.291 / 1.357 | 3,244,646 | Gap (short; qualified) |
| [Bicubic upscale RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 4,258.22 | 2,871.12 | 1.483 / 1.690 | 5,016,578 | Gap (short; qualified) |
| [Bicubic mixed-axis RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-PAIRED.md) | 1,135.34 | 868.67 | 1.307 / 1.473 | 530,385 | Gap (short; qualified) |
| [Bicubic mixed-axis RGBA16§](../benchmarks/WARM-BICUBIC-PAIRED-COLUMNS.md) | 1,237.22 | 846.01 | 1.462 / 1.883 | 1,060,429 | Gap (short; qualified) |
| [Bicubic width reduction / height enlargement RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 994.41 | 840.45 | 1.183 / 1.259 | 731,753 | Gap (short; qualified) |
| [Bicubic width reduction / height enlargement RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 1,056.31 | 819.48 | 1.289 / 1.388 | 1,263,255 | Gap (short; qualified) |
| [Bicubic RGBA8, 128×64 / wider support§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 531.73 | 346.74 | 1.534 / 1.540 | 231,229 | Gap (independent) |
| [Bicubic RGBA16, 128×64 / wider support§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 504.51 | 416.18 | 1.212 / 1.218 | 263,991 | Gap (independent) |
| [Bicubic width reduction, unchanged height RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 734.08 | 574.67 | 1.277 / 1.365 | 465,374 | Gap (short; qualified) |
| [Bicubic width reduction, unchanged height RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 866.64 | 579.18 | 1.496 / 1.646 | 730,691 | Gap (short; qualified) |
| [Bicubic width enlargement, unchanged height RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 2,211.79 | 1,516.35 | 1.459 / 1.596 | 1,930,617 | Gap (short; qualified) |
| [Bicubic width enlargement, unchanged height RGBA16§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 1,965.91 | 1,460.21 | 1.346 / 1.467 | 2,129,333 | Gap (short; qualified) |
| [Bicubic RGBA8, 64×32 / support beyond 32§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 418.52 | 369.03 | 1.134 / 1.215 | 205,884 | Gap (short; qualified) |
| [Bicubic RGBA16, 64×32 / support beyond 32§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 522.57 | 390.56 | 1.338 / 1.351 | 214,071 | Gap (independent) |
| [Bicubic RGBA8, 32×16 / support beyond 64§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 452.57 | 375.86 | 1.204 / 1.208 | 199,355 | Gap (independent) |
| [Bicubic RGBA16, 32×16 / support beyond 64§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 455.52 | 412.28 | 1.105 / 1.110 | 201,398 | Gap (independent) |
| [Bicubic RGBA8, 16×8 / adaptive support beyond 128§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 363.20 | 452.30 | 0.803 / 0.809 | 201,728 | Parity confirmed |
| [Bicubic RGBA16, 16×8 / adaptive support beyond 128§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 441.93 | 496.89 | 0.889 / 0.893 | 202,231 | Parity confirmed |
| [Bicubic RGBA8, 256×129 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-BYTE-STRIPS.md) | 654.01 | 416.59 | 1.570 / 1.660 | 132,293 | Gap (short; qualified) |
| [Bicubic RGBA16, 256×129 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 818.75 | 570.81 | 1.434 / 1.445 | 264,469 | Gap (independent) |
| [Bicubic RGBA8, 128×64 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 588.84 | 349.09 | 1.687 / 1.701 | 32,889 | Gap (independent) |
| [Bicubic RGBA16, 128×64 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 606.16 | 414.59 | 1.462 / 1.479 | 65,658 | Gap (independent) |
| [Bicubic RGBA8, 64×32 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-WEIGHT-BROADCAST.md) | 560.51 | 367.05 | 1.527 / 1.797 | 8,313 | Gap (short; qualified) |
| [Bicubic RGBA16, 64×32 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-WORD-BROADCAST.md) | 527.50 | 383.98 | 1.374 / 2.313 | 16,505 | Gap (short; qualified) |
| [Bicubic RGBA16, 32×16 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 570.51 | 412.45 | 1.383 / 1.394 | 4,217 | Gap (independent) |
| [Bicubic RGBA8, 32×16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-PACKED-REDUCTION.md) | 477.99 | 377.49 | 1.266 / 2.275 | 2,169 | Gap (short; qualified) |
| [Bicubic RGBA16, 16×8 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 487.53 | 495.21 | 0.984 / 0.995 | 1,145 | Parity confirmed |
| [Bicubic RGBA8, 16×8 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 489.92 | 451.81 | 1.084 / 1.090 | 633 | Gap (independent) |
| [Bicubic RGBA16 enlargement / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 4,188.86 | 2,870.82 | 1.459 / 1.637 | 4,264,616 | Gap (short; qualified) |
| [Bicubic RGBA8 enlargement / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 4,766.98 | 3,042.21 | 1.567 / 1.693 | 2,493,057 | Gap (short; qualified) |
| [Bicubic RGBA16, width up / height down / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-PAIRED-COLUMNS.md) | 1,197.19 | 853.79 | 1.402 / 1.555 | 1,060,439 | Gap (short; qualified) |
| [Bicubic RGBA8, width up / height down / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-BYTE-PAIRED.md) | 1,192.42 | 868.12 | 1.374 / 1.571 | 530,385 | Gap (short; qualified) |
| [Bicubic RGBA16, width down / height up / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,298.90 | 825.27 | 1.574 / 1.579 | 1,063,477 | Gap (independent) |
| [Bicubic RGBA8, width down / height up / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,249.21 | 831.82 | 1.502 / 1.513 | 531,910 | Gap (independent) |
| [Bicubic RGBA16, 8×4 / support beyond 256 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 445.77 | 693.80 | 0.643 / 0.651 | 377 | Parity confirmed |
| [Bicubic RGBA8, 8×4 / support beyond 256 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 508.41 | 664.36 | 0.765 / 0.772 | 249 | Parity confirmed |
| [Bicubic RGBA16, 4×2 / packed support beyond 304 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 624.81 | 1,235.83 | 0.506 / 0.509 | 185 | Parity confirmed |
| [Bicubic RGBA8, 4×2 / packed support beyond 304 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 786.38 | 1,138.73 | 0.691 / 0.695 | 153 | Parity confirmed |
| [Bicubic RGBA16, 2×2 / generated support beyond 544 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,069.82 | 2,336.26 | 0.458 / 0.487 | 154 | Parity confirmed |
| [Bicubic RGBA16, 2×9 / bounded row blocks / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,474.00 | 1,286.00 | 1.146 / 1.180 | 266 | Gap (independent) |
| [Bicubic RGBA16, 4×9 / packed row blocks / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 808.81 | 789.44 | 1.025 / 1.030 | 409 | Gap (independent) |
| [Bicubic RGBA16, 8×17 / full-coefficient row blocks / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-8X17-RGBA16.md) | 516.33 | 530.91 | 0.973 / 0.981 | 1,209 | Parity confirmed |
| [Bicubic RGBA8, 8×17 / full-coefficient row blocks / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-8X17-RGBA8.md) | 538.78 | 494.65 | 1.089 / 1.100 | 665 | Gap (independent) |
| [Bicubic RGBA8, 4×9 / packed row blocks / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,002.56 | 675.80 | 1.484 / 1.488 | 266 | Gap (independent) |
| [Bicubic RGBA8, 2×9 / bounded row blocks / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,337.80 | 1,050.39 | 1.274 / 1.286 | 194 | Gap (independent) |
| [Bicubic RGBA8, 2×2 / generated support beyond 544 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,173.66 | 1,633.74 | 0.718 / 0.745 | 138 | Parity confirmed |
| [Bicubic height reduction, unchanged width RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 624.02 | 583.77 | 1.069 / 1.273 | 648,997 | Gap (short; qualified) |
| [Bicubic height reduction, unchanged width RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-BYTE-PAIRED.md) | 725.00 | 589.64 | 1.230 / 1.378 | 265,003 | Gap (short; qualified) |
| [Bicubic height reduction, unchanged width RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 782.41 | 592.55 | 1.320 / 1.672 | 913,737 | Gap (short; qualified) |
| [Bicubic height reduction, unchanged width RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-PAIRED-COLUMNS.md) | 770.52 | 589.55 | 1.307 / 1.361 | 529,847 | Gap (short; qualified) |
| [Bicubic width reduction, unchanged height RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 838.73 | 579.56 | 1.447 / 1.459 | 530,858 | Gap (independent) |
| [Bicubic width reduction, unchanged height RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-HORIZONTAL8.md) | 811.19 | 573.66 | 1.414 / 1.425 | 265,502 | Gap (independent) |
| [Bicubic height enlargement, unchanged width RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 1,776.78 | 1,742.77 | 1.020 / 1.024 | 1,664,251 | Gap (independent) |
| [Bicubic height enlargement, unchanged width RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 2,614.86 | 1,744.07 | 1.499 / 1.722 | 1,280,402 | Gap (short; qualified) |
| [Bicubic height enlargement, unchanged width RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 1,998.15 | 1,670.35 | 1.196 / 1.482 | 2,516,599 | Gap (short; qualified) |
| [Bicubic height enlargement, unchanged width RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 2,286.62 | 1,672.40 | 1.367 / 1.438 | 2,133,103 | Gap (short; qualified) |
| [Bicubic width enlargement, unchanged height RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 1,920.59 | 1,453.02 | 1.322 / 1.373 | 2,129,333 | Gap (short; qualified) |
| [Bicubic width enlargement, unchanged height RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 2,125.43 | 1,509.12 | 1.408 / 1.529 | 1,178,425 | Gap (short; qualified) |
| [Crop RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 2.37 | 2.27 | 1.042 / 1.053 | 96 | Gap (independent) |
| [Crop RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 4.11 | 4.30 | 0.955 / 0.963 | 96 | Parity confirmed |
| [Clone RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 9.50 | 7.95 | 1.195 / 1.209 | 96 | Gap (independent) |
| [Clone RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 19.06 | 17.02 | 1.120 / 1.141 | 96 | Gap (independent) |
| [Normal orientation RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 9.29 | 7.63 | 1.217 / 1.254 | 128 | Gap (independent) |
| [Normal orientation RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 18.58 | 16.70 | 1.112 / 1.132 | 128 | Gap (independent) |
| [Vertical flip RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 9.28 | 8.37 | 1.108 / 1.124 | 128 | Gap (independent) |
| [Vertical flip RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 17.68 | 17.39 | 1.017 / 1.036 | 128 | Gap (independent) |
| [Horizontal flip RGBA8](../benchmarks/WARM-HORIZONTAL-FLIP512.md) | 9.08 | 9.55 | 0.950 / 0.965 | 128 | Parity confirmed |
| [Horizontal flip RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 21.83 | 27.03 | 0.807 / 0.811 | 128 | Parity confirmed |
| [Half turn RGBA8](../benchmarks/WARM-EDIT-CONFIDENCE.md) | 11.47 | 11.42 | 1.004 / 1.024 | 128 | Gap (independent) |
| [Half turn RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 18.59 | 21.86 | 0.850 / 0.874 | 128 | Parity confirmed |
| [Clockwise quarter turn RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 40.77 | 37.31 | 1.093 / 1.126 | 128 | Gap (independent) |
| [Clockwise quarter turn RGBA16](../benchmarks/WARM-EDIT-CONFIDENCE.md) | 43.45 | 44.08 | 0.986 / 1.010 | 128 | Confidence open |
| [Counterclockwise quarter turn RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 38.11 | 37.49 | 1.016 / 1.030 | 128 | Gap (independent) |
| [Counterclockwise quarter turn RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 49.42 | 45.49 | 1.086 / 1.103 | 128 | Gap (independent) |
| [Transpose RGBA8](../benchmarks/WARM-EDIT-CONFIDENCE.md) | 38.15 | 38.42 | 0.993 / 0.995 | 128 | Parity confirmed |
| [Transpose RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 48.17 | 44.86 | 1.074 / 1.114 | 128 | Gap (independent) |
| [Mirrored transpose RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 44.34 | 43.25 | 1.025 / 1.064 | 128 | Gap (independent) |
| [Mirrored transpose RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 39.10 | 38.54 | 1.015 / 1.041 | 128 | Gap (independent) |

* VP8L encode uses **libwebp exact=1, FFmpeg's encoder backend**, through its
public API because FFmpeg does not expose hidden-RGB preservation. All decoded
RGBA samples remain exact. Single-component grayscale JPEG uses the approved
benchmark-only **SIMD libjpeg-turbo3.1.4.1** reference; FFmpeg emits three components.
Production stays safe managed .NET 10 without dependencies or native bindings.

‡ Default VP8 comparisons freeze managed/native quality 100/75 and effort 4, retaining
the 0.25 dB source-quality gate and exact alpha. Matched-quality content controls use
their linked frozen settings. [Current packet sizes](../benchmarks/WARM-VP8-ADAPTIVE-PROBABILITIES.md)
and per-codec reports accompany time/allocation evidence; size parity remains open.

¶ PNG Identify references use the public FFmpeg PNG parser/CRC plus original
complete structural/EXIF validation and owned descriptors, without inflation;
the Stream reference also times fresh AVIO/scratch/input/EOF and release.
‖ JPEG Identify uses FFmpeg MJPEG discard-all, original complete structural/EXIF
validation and a timed17-byte canonical SOF2 metadata projection, fresh packet/
descriptor ownership and reset, with zero pixel-buffer requests. WebP Identify
uses the public FFmpeg WebP parser plus original complete RIFF/EXIF validation,
including mandatory parser buffering/EOF flush and descriptor ownership. Its
0.0040x/upper0.0045x is specific to this metadata boundary and parser copy cost.
All six managed metadata fields remain exact.

† RGBA16 bilinear and explicit RGBA8 upscale allow native-only alpha max1/RMSE≤0.25;
Up8 times zero-alpha RGB clearing. Managed pixels remain exact; Down8 alpha is exact.
§ Bicubic uses **zimg, FFmpeg's zscale backend**, with all required clamped halo,
packing, premultiplied float filtering, unpremultiplication/quantization, zero-alpha
RGB clearing and independent owners timed. Native RGB/alpha max1 and both RMSE≤0.25;
only RGBA16 513×259→513×519 permits native RGB≤2 where reference alpha≤16,≤1
elsewhere, retaining alpha≤1/both RMSE≤0.25. Managed frozen pixels and independently
certified rational-half cases remain exact. Linked reports specify every halo
and scope control; no managed tolerance is introduced.

PNG byte RGB/gray/gray-alpha Optimal uses managed9/native6; RGBA Fastest uses2/1,
SmallestSize9/9, with exact samples and the unchanged5% IDAT-size gate. Packed-gray
transparency uses the approved exact timed native color-key correction. Raw
incompatible baselines/refusals remain unscored; approved equivalent replacements
are confirmed. GIF/animation, startup, concurrency and arbitrary profiles are
outside this still-image fixed warm scope.

Maintain the selected evidence in [the renderer](../benchmarks/render-benchmark.py),
regenerate with `python benchmarks/render-benchmark.py`, then run `--check`.
Preserve raw evidence, failures, controls and package/source qualification.
Stop independently confirmed ≤1x cases. Add a later case only when evidence identifies a distinct
required algorithm or public boundary omitted from the finite checklist.
