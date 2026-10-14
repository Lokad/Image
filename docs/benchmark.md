# Lokad.Image benchmark state

Updated 2026-10-14. Latest retained warm measurements on the dedicated AMD EPYC
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

Ratio = Lokad.Image / the faster FFmpeg pre/post mean; lower is faster. Upper is
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

**Decode**

| Operation / fixed profile | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [PNG RGBA8, None](../benchmarks/WARM-PNG8-DECODE-QUIET.md) | 2,961.21 | 1,516.02 | 1.953 / 1.995 | 755,274 | Confirmed |
| [PNG RGBA8, stored Deflate / None](../benchmarks/WARM-PNG-STORED8-DECODE.md) | 157.30 | 135.76 | 1.159 / 1.168 | 12,976 | Confirmed |
| [PNG RGBA16, None](../benchmarks/WARM-PNG-LITERAL-RUN.md) | 5,740.60 | 2,931.33 | 1.958 / 1.968 | 2,032,864 | Confirmed |
| [PNG RGBA8, Paeth](../benchmarks/WARM-PAETH-FOUR-CHANNEL.md) | 5,099.55 | 2,925.24 | 1.743 / 1.842 | 881,892 | Confirmed |
| [PNG RGBA8, Up](../benchmarks/WARM-PNG-UP8-BASELINE.md) | 2,644.01 | 1,505.93 | 1.756 / 1.764 | 12,976 | Confirmed |
| [PNG RGBA8, Sub](../benchmarks/WARM-PNG-SUB8-BASELINE.md) | 1,209.60 | 682.52 | 1.772 / 1.780 | 12,976 | Confirmed |
| [PNG RGBA8, Average](../benchmarks/WARM-PNG-AVERAGE8-BASELINE.md) | 3,137.74 | 1,841.23 | 1.704 / 1.743 | 12,976 | Confirmed |
| [PNG RGBA16, Paeth](../benchmarks/WARM-PAETH-VECTOR.md) | 8,778.54 | 5,671.15 | 1.548 / 1.607 | 1,844,468 | Confirmed |
| [PNG RGBA16, Adam7](../benchmarks/WARM-PNG-SCATTER.md) | 6,567.82 | 3,984.54 | 1.648 / 1.700 | 2,024,532 | Confirmed |
| [PNG RGB16, None](../benchmarks/WARM-PNG-RGB16-CURRENT.md) | 4,020.98 | 2,210.00 | 1.819 / 1.823 | 15,024 | Confirmed |
| [PNG gray8, None](../benchmarks/WARM-PNG-GRAY8-CURRENT.md) | 564.59 | 361.22 | 1.563 / 1.580 | 9,904 | Confirmed |
| [PNG gray8 + tRNS key127, None](../benchmarks/WARM-PNG-GRAY8-TRANSPARENCY.md) | 563.19 | 762.11 | 0.739 / 0.761 | 9,936 | Confirmed |
| [PNG gray16 + tRNS key32639, None](../benchmarks/WARM-PNG-GRAY16-TRANSPARENCY.md) | 3,501.16 | 2,033.36 | 1.722 / 1.755 | 10,960 | Confirmed |
| [PNG gray-alpha8, None](../benchmarks/WARM-PNG-GRAY-ALPHA8-ROW.md) | 1,574.21 | 1,147.07 | 1.372 / 1.409 | 661,014 | Confirmed |
| [PNG RGB8, None](../benchmarks/WARM-PNG-RGB8-ROW.md) | 2,139.12 | 1,510.62 | 1.416 / 1.440 | 727,305 | Confirmed |
| [PNG RGB8 + tRNS key(48,139,231), None](../benchmarks/WARM-PNG-RGB8-TRANSPARENCY.md) | 1,779.35 | 1,255.63 | 1.417 / 1.439 | 11,984 | Confirmed |
| [PNG RGB16 + tRNS key(12347,35806,59265), None](../benchmarks/WARM-PNG-RGB16-TRANSPARENCY.md) | 4,033.48 | 3,170.09 | 1.272 / 1.302 | 15,056 | Confirmed |
| [PNG gray16, None](../benchmarks/WARM-PNG-GRAY16-ROW.md) | 1,743.26 | 960.42 | 1.815 / 1.837 | 1,331,046 | Confirmed |
| [PNG gray-alpha16, None](../benchmarks/WARM-PNG-GRAY-ALPHA16-ROW.md) | 3,195.37 | 1,798.83 | 1.776 / 1.785 | 1,573,649 | Confirmed |
| [PNG palette8 + tRNS, None](../benchmarks/WARM-PNG-PALETTE-SIMD.md) | 553.32 | 362.52 | 1.526 / 1.543 | 10,976 | Confirmed |
| [PNG palette8 + two-entry tRNS, None](../benchmarks/WARM-PNG-PALETTE8-PARTIAL-ALPHA.md) | 709.94 | 363.54 | 1.953 / 1.967 | 10,728 | Confirmed |
| [PNG palette4 + tRNS → RGBA8, None](../benchmarks/WARM-PNG-DECODE-REUSE.md) | 214.75 | 136.64 | 1.572 / 1.617 | 13,656 | Confirmed |
| [PNG palette2 + tRNS → RGBA8, None](../benchmarks/WARM-PNG-PALETTE2-ROW.md) | 76.33 | 98.66 | 0.774 / 0.789 | 9,208 | Confirmed |
| [PNG palette1 + tRNS → RGBA8, None](../benchmarks/WARM-PNG-PALETTE1-ROW.md) | 61.91 | 86.80 | 0.713 / 0.723 | 9,072 | Confirmed |
| [PNG opaque gray1 → RGBA8, None](../benchmarks/WARM-PNG-GRAY1-ROW.md) | 299.69 | 305.79 | 0.980 / 1.001 | 537,700 | Confirmed |
| [PNG opaque gray2 → RGBA8, None](../benchmarks/WARM-PNG-GRAY2-CURRENT.md) | 83.19 | 110.11 | 0.756 / 0.783 | 9,136 | Confirmed |
| [PNG opaque gray4 → RGBA8, None](../benchmarks/WARM-PNG-GRAY4-REUSE.md) | 220.61 | 146.67 | 1.504 / 1.532 | 13,544 | Confirmed |
| [JPEG baseline gray](../benchmarks/WARM-JPEG-AC-FUSED.md) | 1,267.57 | 682.73 | 1.857 / 1.880 | 1,221,545 | Confirmed |
| [JPEG baseline 4:2:0](../benchmarks/WARM-JPEG420-BULK.md) | 1,721.81 | 1,007.73 | 1.709 / 1.719 | 1,619,530 | Confirmed |
| [JPEG baseline 4:2:2](../benchmarks/WARM-COVERAGE-QUIET.md) | 2,087.01 | 1,336.37 | 1.562 / 1.567 | 1,978,646 | Confirmed |
| [JPEG baseline 4:4:4, owned input](../benchmarks/WARM-JPEG444-OWNED-QUIET.md) | 3,564.61 | 2,039.57 | 1.748 / 1.793 | 2,596,174 | Confirmed |
| [JPEG baseline 4:4:4, native input](../benchmarks/WARM-JPEG-AC-FUSED.md) | 3,339.18 | 1,719.98 | 1.941 / 1.961 | 2,664,358 | Confirmed |
| [JPEG progressive gray](../benchmarks/WARM-JPEG-REFINEMENT.md) | 3,336.90 | 1,743.36 | 1.914 / 1.942 | 1,232,501 | Confirmed |
| [JPEG progressive 4:2:0](../benchmarks/WARM-JPEG-PROGRESSIVE420.md) | 3,646.81 | 1,938.47 | 1.881 / 1.894 | 1,637,521 | Confirmed |
| [JPEG progressive 4:2:2](../benchmarks/WARM-JPEG-PROGRESSIVE422.md) | 3,882.57 | 2,210.07 | 1.757 / 1.781 | 1,996,661 | Confirmed |
| [JPEG progressive 4:4:4](../benchmarks/WARM-JPEG-MASK.md) | 6,217.13 | 3,342.33 | 1.860 / 1.878 | 2,682,364 | Confirmed |
| [WebP VP8 + alpha](../benchmarks/WARM-VP8-DECODE-QUIET.md) | 11,623.12 | 6,074.58 | 1.913 / 1.948 | 1,966,616 | Confirmed |
| [WebP VP8, opaque](../benchmarks/WARM-VP8-OPAQUE-DECODE.md) | 9,504.58 | 4,792.69 | 1.983 / 2.004 | 760,548 | Upper above 2x / open |
| [WebP VP8L, literal/run input](../benchmarks/WARM-VP8L-DECODE-QUIET.md) | 3,948.44 | 2,332.84 | 1.693 / 1.732 | 1,074,301 | Confirmed |
| [WebP VP8L, native input](../benchmarks/WARM-VP8L-CANONICAL.md) | 5,703.55 | 3,023.03 | 1.887 / 1.904 | 1,216,180 | Confirmed |

**Encode**

| Operation / fixed profile | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [PNG RGBA8, level 6 / None](../benchmarks/WARM-PNG8-ENCODE.md) | 7,241.99 | 17,219.43 | 0.421 / 0.430 | 527,629 | Confirmed |
| [PNG RGBA8, NoCompression / None](../benchmarks/WARM-PNG-STORED8-ENCODE.md) | 357.17 | 189.72 | 1.883 / 1.933 | 2,102,472 | Confirmed |
| [PNG RGBA16, level 6 / None](../benchmarks/WARM-PNG16-ENCODE-CRC.md) | 16,470.31 | 19,951.52 | 0.826 / 0.838 | 2,103,836 | Confirmed |
| [PNG RGB8, level 6 / None](../benchmarks/WARM-PNG-RGB8-ENCODE-BOUNDARY.md) | — | — | — | — | Compression-size gate failed; no timing |
| [PNG RGB16, level 6 / None](../benchmarks/WARM-PNG-RGB16-ENCODE.md) | 13,121.10 | 15,174.40 | 0.865 / 0.871 | 2,102,765 | Confirmed |
| [PNG gray8, level 6 / None](../benchmarks/WARM-PNG-GRAY8-ENCODE-BOUNDARY.md) | — | — | — | — | Compression-size gate failed; no timing |
| [PNG gray16, level 6 / None](../benchmarks/WARM-PNG-GRAY16-ENCODE.md) | 5,667.73 | 4,927.46 | 1.150 / 1.156 | 1,051,455 | Confirmed |
| [PNG gray-alpha8, level 6 / None](../benchmarks/WARM-PNG-GRAY-ALPHA8-ENCODE-BOUNDARY.md) | — | — | — | — | Compression-size gate failed; no timing |
| [PNG gray-alpha16, level 6 / None](../benchmarks/WARM-PNG-GRAY-ALPHA16-ENCODE.md) | 9,316.78 | 10,318.59 | 0.903 / 0.915 | 1,052,368 | Confirmed |
| [JPEG 4:4:4, adaptive Huffman / matched quality](../benchmarks/WARM-JPEG444-RGBA8-ADAPTIVE.md) | 6,705.88 | 3,783.34 | 1.772 / 1.818 | 559,384 | Confirmed |
| [JPEG 4:4:4 from RGBA16, adaptive Huffman / full normalization](../benchmarks/WARM-JPEG444-RGBA16-ADAPTIVE.md) | 6,767.17 | 3,851.77 | 1.757 / 1.790 | 556,280 | Confirmed |
| [JPEG 4:2:2, adaptive Huffman / matched quality](../benchmarks/WARM-JPEG422-RGBA8-ADAPTIVE.md) | 4,277.49 | 2,349.58 | 1.821 / 1.862 | 294,110 | Confirmed |
| [JPEG 4:2:0, adaptive Huffman / matched quality](../benchmarks/WARM-JPEG420-RGBA8-ADAPTIVE.md) | 3,501.53 | 1,771.06 | 1.977 / 2.014 | 297,772 | Upper above 2x / open |
| [JPEG 4:2:0 from RGBA16, adaptive Huffman](../benchmarks/WARM-JPEG-ADAPTIVE-HUFFMAN.md) | 3,481.14 | 1,843.98 | 1.888 / 1.931 | 297,772 | Confirmed |
| [WebP VP8 + alpha‡](../benchmarks/WARM-VP8-ALPHAWEIGHTED.md) | 42,879.92 | 54,484.52 | 0.787 / 0.805 | 2,615,472 | Confirmed |
| [WebP VP8 + alpha from RGBA16‡](../benchmarks/WARM-VP8-RGBA16-ENCODE.md) | 43,362.07 | 53,579.62 | 0.809 / 0.819 | 2,615,472 | Confirmed |
| [WebP VP8L, exact backend*](../benchmarks/WARM-VP8L-ENCODE-EXACT.md) | 7,363.30 | 121,587.95 | 0.061 / 0.063 | 2,100,170 | Confirmed |
| [WebP VP8L from RGBA16, exact backend*](../benchmarks/WARM-VP8L-RGBA16-ENCODE.md) | 7,928.11 | 121,282.99 | 0.065 / 0.066 | 2,100,119 | Confirmed |

**Editing**

| Operation / fixed profile | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [Nearest resize RGBA8](../benchmarks/WARM-COVERAGE-QUIET.md) | 58.67 | 252.66 | 0.232 / 0.241 | 132,294 | Confirmed |
| [Nearest upscale RGBA8](../benchmarks/WARM-NEAREST-UP8.md) | 935.05 | 1,006.00 | 0.929 / 0.955 | 2,133,091 | Confirmed |
| [Nearest upscale RGBA16](../benchmarks/WARM-NEAREST-UP16.md) | 1,071.13 | 1,019.06 | 1.051 / 1.095 | 4,265,431 | Confirmed |
| [Nearest resize RGBA16](../benchmarks/WARM-PREPARED-CONFIRM.md) | 105.69 | 170.58 | 0.620 / 0.629 | 264,466 | Confirmed |
| [Bilinear resize RGBA8](../benchmarks/WARM-BILINEAR8-QUIET.md) | 729.12 | 376.29 | 1.938 / 1.958 | 157,955 | Confirmed |
| [Bilinear resize RGBA16†](../benchmarks/WARM-BILINEAR16-REGISTER.md) | 784.03 | 418.41 | 1.874 / 1.908 | 290,118 | Confirmed |
| [Bilinear upscale RGBA8†](../benchmarks/WARM-BILINEAR-UP8-BATCH.md) | 3,797.55 | 2,152.25 | 1.764 / 1.787 | 2,235,599 | Confirmed |
| [Bilinear upscale RGBA16†](../benchmarks/WARM-BILINEAR-UP16-ROWS.md) | 2,550.27 | 1,916.08 | 1.331 / 1.416 | 4,498,974 | Confirmed |
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
| [Bicubic width enlargement, unchanged height RGBA16§](../benchmarks/WARM-BICUBIC-HORIZONTAL-UP16.md) | 2,801.88 | 1,429.24 | 1.960 / 1.973 | 2,232,080 | Confirmed |
| [Bicubic RGBA8, 64×32 / support beyond 32§](../benchmarks/WARM-BICUBIC-64X32-RGBA8.md) | 637.23 | 351.87 | 1.811 / 1.820 | 205,880 | Confirmed |
| [Bicubic RGBA16, 64×32 / support beyond 32§](../benchmarks/WARM-BICUBIC-64X32-RGBA16.md) | 523.91 | 377.79 | 1.387 / 1.414 | 214,071 | Confirmed |
| [Bicubic RGBA8, 32×16 / support beyond 64§](../benchmarks/WARM-BICUBIC-32X16-RGBA8.md) | 465.61 | 393.50 | 1.183 / 1.192 | 199,354 | Confirmed |
| [Bicubic RGBA16, 32×16 / support beyond 64§](../benchmarks/WARM-BICUBIC-32X16-RGBA16.md) | 463.30 | 411.83 | 1.125 / 1.131 | 201,397 | Confirmed |
| [Bicubic RGBA8, 16×8 / adaptive support beyond 128§](../benchmarks/WARM-BICUBIC-16X8-ADAPTIVE.md) | 614.50 | 452.97 | 1.357 / 1.368 | 201,721 | Confirmed |
| [Bicubic RGBA16, 16×8 / adaptive support beyond 128§](../benchmarks/WARM-BICUBIC-16X8-RGBA16.md) | 465.90 | 489.16 | 0.952 / 0.958 | 202,229 | Confirmed |
| [Bicubic height reduction, unchanged width RGBA8§](../benchmarks/WARM-BICUBIC-VERTICAL16-IDENTITY.md) | 761.11 | 571.64 | 1.331 / 1.345 | 648,992 | Confirmed |
| [Bicubic height reduction, unchanged width RGBA16§](../benchmarks/WARM-BICUBIC-VERTICAL16-IDENTITY.md) | 832.27 | 577.53 | 1.441 / 1.480 | 913,737 | Confirmed |
| [Bicubic height enlargement, unchanged width RGBA8§](../benchmarks/WARM-BICUBIC-VERTICAL-UP8-ROWS.md) | 2,213.62 | 1,730.56 | 1.279 / 1.286 | 1,664,240 | Confirmed |
| [Bicubic height enlargement, unchanged width RGBA16§](../benchmarks/WARM-BICUBIC-VERTICAL-UP16-ROWS.md) | 2,131.66 | 1,655.60 | 1.288 / 1.297 | 2,516,605 | Confirmed |
| [Crop RGBA8](../benchmarks/WARM-CROPCLONE8-FLIP16.md) | 2.34 | 2.27 | 1.033 / 1.039 | 96 | Confirmed |
| [Crop RGBA16](../benchmarks/WARM-CROP-COPY.md) | 4.11 | 4.15 | 0.991 / 1.003 | 96 | Confirmed |
| [Clone RGBA8](../benchmarks/WARM-CROPCLONE8-FLIP16.md) | 9.82 | 7.19 | 1.366 / 1.390 | 96 | Confirmed |
| [Clone RGBA16](../benchmarks/WARM-CLONE-REUSE.md) | 17.86 | 16.60 | 1.076 / 1.089 | 96 | Confirmed |
| [Normal orientation RGBA8](../benchmarks/WARM-COPIES8-QUIET.md) | 9.92 | 7.98 | 1.243 / 1.296 | 128 | Confirmed |
| [Normal orientation RGBA16](../benchmarks/WARM-FLIP-ORIENTATION-QUIET.md) | 18.01 | 16.59 | 1.086 / 1.102 | 128 | Confirmed |
| [Vertical flip RGBA8](../benchmarks/WARM-FLIPTURN8-FRESH.md) | 9.02 | 8.28 | 1.090 / 1.110 | 128 | Confirmed |
| [Vertical flip RGBA16](../benchmarks/WARM-DIRECTIONS16-QUIET.md) | 17.16 | 17.57 | 0.977 / 0.983 | 128 | Confirmed |
| [Horizontal flip RGBA8](../benchmarks/WARM-FLIP-ORIENTATION-QUIET.md) | 11.60 | 9.32 | 1.244 / 1.261 | 128 | Confirmed |
| [Horizontal flip RGBA16](../benchmarks/WARM-CROPCLONE8-FLIP16.md) | 21.72 | 21.94 | 0.990 / 1.007 | 128 | Confirmed |
| [Half turn RGBA8](../benchmarks/WARM-FLIPTURN8-FRESH.md) | 12.47 | 11.34 | 1.100 / 1.118 | 128 | Confirmed |
| [Half turn RGBA16](../benchmarks/WARM-HALFTURN-PIXELS.md) | 20.78 | 22.28 | 0.933 / 0.967 | 128 | Confirmed |
| [Clockwise quarter turn RGBA8](../benchmarks/WARM-DIRECTIONS8-QUIET.md) | 36.86 | 37.89 | 0.973 / 0.990 | 128 | Confirmed |
| [Clockwise quarter turn RGBA16](../benchmarks/WARM-CLOCKWISE-TILES.md) | 43.69 | 44.12 | 0.990 / 1.013 | 128 | Confirmed |
| [Counterclockwise quarter turn RGBA8](../benchmarks/WARM-DIRECTIONS8-QUIET.md) | 37.40 | 37.71 | 0.992 / 1.003 | 128 | Confirmed |
| [Counterclockwise quarter turn RGBA16](../benchmarks/WARM-DIRECTIONS16-QUIET.md) | 49.20 | 44.42 | 1.108 / 1.133 | 128 | Confirmed |
| [Transpose RGBA8](../benchmarks/WARM-TRANSPOSE8-PARITY.md) | 38.00 | 38.36 | 0.991 / 1.002 | 128 | Confirmed |
| [Transpose RGBA16](../benchmarks/WARM-COVERAGE-QUIET.md) | 48.75 | 44.49 | 1.096 / 1.103 | 128 | Confirmed |
| [Mirrored transpose RGBA16](../benchmarks/WARM-DIRECTIONS16-QUIET.md) | 44.93 | 42.63 | 1.054 / 1.085 | 128 | Confirmed |
| [Mirrored transpose RGBA8](../benchmarks/WARM-DIRECTIONS8-QUIET.md) | 36.59 | 37.98 | 0.964 / 0.991 | 128 | Confirmed |

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
harness overhead). Decoder timing uses resident encoded spans. Separate fresh
public Stream controls measure Paeth8 **2,275,800**, Paeth16 **4,713,416**, and
Adam7-16 **5,058,912 B/op**, identically on both OSes with normal/scalar code.
These are distinct boundaries. FFmpeg allocation totals are **not measured**;
they must not be interpreted as zero. Retained owners and cold allocation patterns
need separate coverage.

Full required warm coverage is **open**. The
[material coverage audit](../benchmarks/WARM-COVERAGE.md) tracks remaining depths,
layouts, resize directions/fallbacks, fresh Streams, retained owners and composed
public operations. RGBA8 bilinear upscaling now confirms **1.764x / upper 1.787x**
after a four-pixel kernel cuts its baseline mean 41.8%. Stop tuning that pair;
RGBA16 upscaling also confirms **1.331x / upper 1.416x** with horizontal row reuse,
cutting its baseline mean 52.4%. Bicubic RGBA16 now confirms **1.313x / upper 1.320x**
against the explicitly adjusted scaler backend, cutting its baseline mean 91.1%.
RGBA8 on the same new build confirms **1.713x / upper 1.728x**. Other material
paths remain open. Bicubic RGBA16 enlargement confirms **1.505x / upper 1.516x**,
cutting its baseline mean 64.1% while retaining exact managed pixels.
For bicubic width enlargement with height reduction, the same final build
confirms RGBA16 **1.924x / upper 1.975x**, cutting a 20.419x baseline by 90.6%,
and RGBA8 **1.848x / upper 1.867x** with unchanged allocations.
RGBA8 bicubic width reduction with height enlargement confirms **1.190x / upper
1.206x**, cutting a 21.910x baseline by 94.6%.
RGBA16 in that direction confirms **1.285x / upper 1.297x**, cutting a 23.838x
baseline by 94.7%.
Wider-support RGBA8 downsampling to 128×64 confirms **1.631x / upper 1.642x**,
cutting a 26.954x baseline by 94.0% with bounded thirty-two-row reuse.
RGBA16 at that shape confirms **1.432x / upper 1.446x**, cutting a 25.058x baseline
by 94.2% with exact unsigned products and conservative word precision guards.
PNG RGB8, gray8 and gray-alpha8
encoding preserves exact samples but exceeds the 5% IDAT-size gate by differences
of 12.31%, 16.65% and 5.17%; these cases receive no timing score. GIF and animation
are deferred.

JPEG420 RGBA8's independent upper is **2.014x**, so that pair remains open.
Opaque VP8 decoding also remains open at **1.983x / upper 2.004x**.
Its [bulk normalization trial](../benchmarks/VP8-NORMALIZATION-REJECTED.md) is rejected;
the tested production is restored.
Its [correctness prototype](../benchmarks/VP8-WINDOW-PROTOTYPE.md) passes 5.42 million
reference checks per OS/mode and preserves both frozen pixel profiles. The
[wider-window integration](../benchmarks/VP8-WINDOW-REJECTED.md) is also rejected:
2.052x / upper 2.115x, unchanged 760,548 B/op. Production is restored;
no arithmetic-reader speed gain is accepted.
JPEG444/422 RGBA8 and JPEG444/420 RGBA16 are confirmed with adaptive Huffman
coding; remaining material codec layouts and public boundaries still need coverage.
Stop tuning confirmed pairs; no averages or rolling older-source results prove
the uniform target. Linked reports retain superseded raw evidence, failed guards,
rejected experiments, correctness controls and exact package/source identities.

Maintain this summary whenever a performance round is accepted: update the
corresponding evidence selection in [the renderer](../benchmarks/render-benchmark.py),
regenerate with `python benchmarks/render-benchmark.py`, and run it with `--check`.
Preserve superseded raw evidence and failed runs in their linked reports. Add a
new row only for material coverage; prioritize the largest remaining gap one
primitive at a time. See the [measurement recipe](../tools/WarmReference/README.md).
