# Lokad.Image benchmark state

Updated 2026-10-15. Latest retained warm measurements on the dedicated AMD EPYC
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
| [PNG8 Stream, orient/crop/nearest fit](../benchmarks/WARM-PNG-COMPOSITION8.md) | 2,795.71 | 1,478.63 | 1.891 / 1.906 | 1,453,517 | Confirmed |
| [PNG16 Stream, orient/crop/nearest fit](../benchmarks/WARM-PNG-FUSED-INFLATION.md) | 3,572.35 | 2,936.91 | 1.216 / 1.231 | 4,460,393 | Confirmed |
| [PNG RGBA8, fresh seekable Stream](../benchmarks/WARM-STREAM-PNG8.md) | 2,655.37 | 1,392.55 | 1.907 / 1.916 | 756,605 | Confirmed |
| [PNG RGBA16, fresh seekable Stream](../benchmarks/WARM-STREAM-PNG16.md) | 5,626.97 | 3,093.68 | 1.819 / 1.830 | 3,075,772 | Confirmed |
| [JPEG420 restart, fresh seekable Stream](../benchmarks/WARM-JPEG-STREAM.md) | 1,886.05 | 997.23 | 1.891 / 1.902 | 1,813,266 | Confirmed |
| [WebP VP8 + alpha, fresh seekable Stream](../benchmarks/WARM-VP8-STREAM.md) | 10,914.60 | 5,979.52 | 1.825 / 1.833 | 2,327,699 | Confirmed |
| [PNG RGBA8, None](../benchmarks/WARM-PNG-FUSED-INFLATION.md) | 1,991.21 | 1,368.86 | 1.455 / 2.779 | 4,456 | Regression screen; prior confirmed |
| [PNG RGBA8, stored Deflate / None](../benchmarks/WARM-PNG-STORED8-DECODE.md) | 157.30 | 135.76 | 1.159 / 1.168 | 12,976 | Confirmed |
| [PNG RGBA16, stored Deflate / None](../benchmarks/WARM-PNG-STORED16-DECODE.md) | 472.57 | 309.99 | 1.524 / 1.556 | 1,080,706 | Confirmed |
| [PNG RGBA16, None](../benchmarks/WARM-PNG-LITERAL-RUN.md) | 5,740.60 | 2,931.33 | 1.958 / 1.968 | 2,032,864 | Confirmed |
| [PNG RGBA8, Paeth](../benchmarks/WARM-PAETH-FOUR-CHANNEL.md) | 5,099.55 | 2,925.24 | 1.743 / 1.842 | 881,892 | Confirmed |
| [PNG RGBA8, Up](../benchmarks/WARM-PNG-UP8-BASELINE.md) | 2,644.01 | 1,505.93 | 1.756 / 1.764 | 12,976 | Confirmed |
| [PNG RGBA8, Sub](../benchmarks/WARM-PNG-SUB8-BASELINE.md) | 1,209.60 | 682.52 | 1.772 / 1.780 | 12,976 | Confirmed |
| [PNG RGBA8, Average](../benchmarks/WARM-PNG-AVERAGE8-BASELINE.md) | 3,137.74 | 1,841.23 | 1.704 / 1.743 | 12,976 | Confirmed |
| [PNG RGBA16, Paeth](../benchmarks/WARM-PNG-FUSED-INFLATION.md) | 5,747.13 | 5,462.45 | 1.052 / 1.327 | 8,552 | Regression screen; prior confirmed |
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
| [JPEG baseline 4:2:0, row restart markers](../benchmarks/WARM-JPEG-RESTART-DECODE.md) | 1,852.40 | 1,016.13 | 1.823 / 1.837 | 1,623,906 | Confirmed |
| [JPEG baseline 4:2:2](../benchmarks/WARM-COVERAGE-QUIET.md) | 2,087.01 | 1,336.37 | 1.562 / 1.567 | 1,978,646 | Confirmed |
| [JPEG baseline 4:4:4, owned input](../benchmarks/WARM-JPEG444-OWNED-QUIET.md) | 3,564.61 | 2,039.57 | 1.748 / 1.793 | 2,596,174 | Confirmed |
| [JPEG baseline 4:4:4, native input](../benchmarks/WARM-JPEG-AC-FUSED.md) | 3,339.18 | 1,719.98 | 1.941 / 1.961 | 2,664,358 | Confirmed |
| [JPEG progressive gray](../benchmarks/WARM-JPEG-REFINEMENT.md) | 3,336.90 | 1,743.36 | 1.914 / 1.942 | 1,232,501 | Confirmed |
| [JPEG progressive 4:2:0](../benchmarks/WARM-JPEG-PROGRESSIVE420.md) | 3,646.81 | 1,938.47 | 1.881 / 1.894 | 1,637,521 | Confirmed |
| [JPEG progressive 4:2:2](../benchmarks/WARM-JPEG-PROGRESSIVE422.md) | 3,882.57 | 2,210.07 | 1.757 / 1.781 | 1,996,661 | Confirmed |
| [JPEG progressive 4:4:4](../benchmarks/WARM-JPEG-MASK.md) | 6,217.13 | 3,342.33 | 1.860 / 1.878 | 2,682,364 | Confirmed |
| [WebP VP8 + alpha](../benchmarks/WARM-VP8-FILTER8.md) | 10,777.02 | 5,977.48 | 1.803 / 1.819 | 1,966,616 | Confirmed |
| [WebP VP8, opaque](../benchmarks/WARM-VP8-FILTER8.md) | 8,649.43 | 4,797.64 | 1.803 / 1.808 | 760,548 | Confirmed |
| [WebP VP8L, literal/run input](../benchmarks/WARM-VP8L-DECODE-QUIET.md) | 3,948.44 | 2,332.84 | 1.693 / 1.732 | 1,074,301 | Confirmed |
| [WebP VP8L, native input](../benchmarks/WARM-VP8L-CANONICAL.md) | 5,703.55 | 3,023.03 | 1.887 / 1.904 | 1,216,180 | Confirmed |

**Encode**

| Operation / fixed profile | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [PNG RGBA8, level 6 / None](../benchmarks/WARM-PNG8-ENCODE.md) | 7,241.99 | 17,219.43 | 0.421 / 0.430 | 527,629 | Confirmed |
| [PNG RGBA8, NoCompression / None](../benchmarks/WARM-PNG-STORED8-ENCODE.md) | 357.17 | 189.72 | 1.883 / 1.933 | 2,102,472 | Confirmed |
| [PNG RGBA16, NoCompression / None](../benchmarks/WARM-PNG-STORED16-ENCODE.md) | 504.25 | 405.59 | 1.243 / 1.289 | 4,203,541 | Confirmed |
| [PNG RGBA16, Fastest / level1 / None](../benchmarks/WARM-PNG-FAST16-REFUSAL.md) | — | — | — | — | Unscored: IDAT-size gate failed |
| [PNG RGBA16, level 6 / None](../benchmarks/WARM-PNG-STORED16-ENCODE.md) | 14,544.25 | 20,072.37 | 0.725 / 0.756 | 2,103,776 | Regression screen; prior confirmed |
| [PNG RGB8, Optimal managed9/native6 / None](../benchmarks/WARM-PNG-BYTE-OPTIMAL.md) | 10,402.09 | 9,514.15 | 1.093 / 1.107 | 527,098 | Confirmed |
| [PNG RGB16, level 6 / None](../benchmarks/WARM-PNG-RGB16-ENCODE.md) | 13,121.10 | 15,174.40 | 0.865 / 0.871 | 2,102,765 | Confirmed |
| [PNG gray8, Optimal managed9/native6 / None](../benchmarks/WARM-PNG-BYTE-OPTIMAL.md) | 3,540.72 | 1,964.99 | 1.802 / 1.818 | 263,664 | Confirmed |
| [PNG gray16, level 6 / None](../benchmarks/WARM-PNG-GRAY16-ENCODE.md) | 5,667.73 | 4,927.46 | 1.150 / 1.156 | 1,051,455 | Confirmed |
| [PNG gray-alpha8, Optimal managed9/native6 / None](../benchmarks/WARM-PNG-BYTE-OPTIMAL.md) | 5,924.76 | 4,311.86 | 1.374 / 1.382 | 264,179 | Confirmed |
| [PNG gray-alpha16, level 6 / None](../benchmarks/WARM-PNG-GRAY-ALPHA16-ENCODE.md) | 9,316.78 | 10,318.59 | 0.903 / 0.915 | 1,052,368 | Confirmed |
| [JPEG 4:4:4, adaptive Huffman / matched quality](../benchmarks/WARM-JPEG444-RGBA8-ADAPTIVE.md) | 6,705.88 | 3,783.34 | 1.772 / 1.818 | 559,384 | Confirmed |
| [JPEG 4:4:4, forward-only output / geometric AC Huffman](../benchmarks/WARM-JPEG444-FORWARD.md) | 6,613.11 | 3,770.32 | 1.754 / 1.765 | 526,051 | Confirmed |
| [JPEG 4:4:4 from RGBA16, adaptive Huffman / full normalization](../benchmarks/WARM-JPEG444-RGBA16-ADAPTIVE.md) | 6,767.17 | 3,851.77 | 1.757 / 1.790 | 556,280 | Confirmed |
| [JPEG 4:2:2, adaptive Huffman / matched quality](../benchmarks/WARM-JPEG422-RGBA8-ADAPTIVE.md) | 4,277.49 | 2,349.58 | 1.821 / 1.862 | 294,110 | Confirmed |
| [JPEG 4:2:2 from RGBA16, adaptive Huffman / full normalization](../benchmarks/WARM-JPEG422-RGBA16.md) | 4,538.42 | 2,422.35 | 1.874 / 1.884 | 294,110 | Confirmed |
| [JPEG 4:2:0, adaptive Huffman / matched quality](../benchmarks/WARM-JPEG420-CHROMA-PAIR.md) | 3,434.93 | 1,767.46 | 1.943 / 1.988 | 297,772 | Confirmed |
| [JPEG 4:2:0, adaptive / row restart markers](../benchmarks/WARM-JPEG-ADMITTED.md) | 3,157.50 | 1,612.58 | 1.958 / 1.973 | 297,780 | Confirmed |
| [JPEG 4:2:0, forward-only output / geometric AC Huffman](../benchmarks/WARM-JPEG-FIXED-GEOMETRIC.md) | 3,382.57 | 1,765.48 | 1.916 / 1.936 | 263,812 | Confirmed |
| [JPEG 4:2:0 from RGBA16, forward-only output](../benchmarks/WARM-JPEG-FORWARD-RGBA16.md) | 3,572.45 | 1,833.91 | 1.948 / 1.954 | 263,812 | Confirmed |
| [JPEG 4:2:0 from RGBA16, adaptive Huffman](../benchmarks/WARM-JPEG-ADAPTIVE-HUFFMAN.md) | 3,481.14 | 1,843.98 | 1.888 / 1.931 | 297,772 | Confirmed |
| [JPEG grayscale, single component](../benchmarks/WARM-COVERAGE.md) | — | — | — | — | Unscored: native baseline unavailable |
| [WebP VP8, opaque‡](../benchmarks/WARM-VP8-OPAQUE-ENCODE.md) | 29,381.20 | 15,527.63 | 1.892 / 1.918 | 2,536,203 | Confirmed |
| [WebP VP8, opaque from RGBA16‡](../benchmarks/WARM-VP8-OPAQUE-RGBA16.md) | 29,778.24 | 15,604.01 | 1.908 / 1.922 | 2,535,736 | Confirmed |
| [WebP VP8 + alpha‡](../benchmarks/WARM-VP8-OPAQUE-ENCODE.md) | 30,157.65 | 53,549.54 | 0.563 / 0.655 | 2,615,939 | Regression screen; prior confirmed |
| [WebP VP8 + alpha from RGBA16‡](../benchmarks/WARM-VP8-OPAQUE-ENCODE.md) | 30,644.46 | 53,908.23 | 0.568 / 0.598 | 2,615,472 | Regression screen; prior confirmed |
| [WebP VP8L, exact backend*](../benchmarks/WARM-VP8L-ENCODE-EXACT.md) | 7,363.30 | 121,587.95 | 0.061 / 0.063 | 2,100,170 | Confirmed |
| [WebP VP8L from RGBA16, exact backend*](../benchmarks/WARM-VP8L-RGBA16-ENCODE.md) | 7,928.11 | 121,282.99 | 0.065 / 0.066 | 2,100,119 | Confirmed |

**Editing**

| Operation / fixed profile | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [Horizontal flip RGBA8, two live outputs / per image](../benchmarks/WARM-RETAINED-FLIP.md) | 13.26 | 9.16 | 1.447 / 1.484 | 128 | Confirmed |
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
PNG RGB8, gray8 and gray-alpha8 encoding confirms below 2x with exact samples
and the unchanged 5% IDAT-size gate, using managed level 9 / FFmpeg level 6.
Their earlier level6/level6 refusals remain unscored. GIF and animation are deferred.

JPEG420 RGBA8 confirms **1.943x / upper 1.988x** after sharing chroma sampling,
with unchanged exact packets and allocations.
Its forward-only output fallback confirms **1.916x / upper 1.936x** with
geometric AC coding, exact coefficients/pixels and half the allocation.
RGBA16 forward-only output confirms **1.948x / upper 1.954x**, including full
word normalization and flattening, on the same qualified production.
Two-live-output RGBA8 flips confirm **1.447x / upper 1.484x**, at **128 B/image**,
after bounded two-array reuse halves managed elapsed time. Both subjects retain
two complete outputs before release; counters consistently normalize per image.
Opaque VP8 decoding now confirms **1.803x / upper 1.808x**, closing its previous
upper-2.004x gap. An eight-pixel integer loop-filter kernel reduces the retained
managed mean **8.8%**, preserving exact pixels and allocations. Alpha-bearing
VP8 independently confirms **1.803x / upper 1.819x** on the same qualified build.
Earlier [bulk normalization](../benchmarks/VP8-NORMALIZATION-REJECTED.md),
[wider-window](../benchmarks/VP8-WINDOW-REJECTED.md) and
[token-local](../benchmarks/VP8-TOKEN-LOCAL-REJECTED.md) reader trials remain rejected.
JPEG444/422 RGBA8 and JPEG444/422/420 RGBA16 are confirmed with adaptive Huffman
coding; RGBA16 4:2:2 confirms **1.874x / upper 1.884x** with full normalization.
JPEG420 with row restart markers confirms **1.958x / upper 1.973x**, at
**297,780 B/op**, after whole-packet admission removes repeated entropy byte
checks. Its retained mean falls **8.0%** with identical packets; nonrestart
adaptive and fixed streaming regression means pass. Both OSes pass **3,298**
Release tests, including **896** live comparisons, and the same actual package's
consumer and three examples. Short regression screens retain their own bounds
in the linked report and do not replace earlier independent certificates.
Identical-input JPEG420 restart decoding confirms **1.823x / upper 1.837x**,
at **1,623,906 B/op**, reusing the validated packet and qualified production.
Opaque VP8 encoding closes a **2.718x** gap at **1.892x / upper 1.918x**,
with identical packets and **2,536,203 B/op**. Packing arithmetic output,
compiling the existing tree paths and inlining emission reduce elapsed time
**30.1%**. Current alpha8/16 regression screens measure **0.563x / 0.568x**;
their rows explicitly distinguish short screens from the earlier independent
confirmations. Both OSes pass **3,308** Release tests and the same actual package.
Opaque RGBA16 VP8 encoding independently confirms **1.908x / upper 1.922x**,
at **2,535,736 B/op**, including complete word normalization/repacking. Its
normalized raster, packets and quality samples equal the opaque byte anchor;
unchanged qualified production and package proofs are reused.
Fresh seekable JPEG420 Stream decoding confirms **1.891x / upper 1.902x**,
at **1,813,266 B/op**. Complete acquisition, all buffering/copies, EOF checks,
decoding/conversion and owned output release are timed on both subjects;
the existing restart packet and qualified production are reused.
Fresh VP8/ALPH Stream decoding confirms **1.825x / upper 1.833x**, at
**2,327,699 B/op**, with complete acquisition/copies, alpha/chroma/color passes
and input/output release timed. Its input and per-decoder pixels equal the
resident anchor; unchanged qualified production and package proofs are reused.
Forward-only JPEG444 encoding confirms **1.754x / upper 1.765x**, at
**526,051 B/op**, using the existing geometric Huffman fallback. Full independent
coefficient dumps, per-decoder pixels and source errors equal the qualified
adaptive anchor; unchanged production and native worker proofs are reused.
Remaining material codec layouts and public boundaries still need coverage.
Stop tuning confirmed pairs; no averages or rolling older-source results prove
the uniform target. Linked reports retain superseded raw evidence, failed guards,
rejected experiments, correctness controls and exact package/source identities.

Maintain this summary whenever a performance round is accepted: update the
corresponding evidence selection in [the renderer](../benchmarks/render-benchmark.py),
regenerate with `python benchmarks/render-benchmark.py`, and run it with `--check`.
Preserve superseded raw evidence and failed runs in their linked reports. Add a
new row only for material coverage; prioritize the largest remaining gap one
primitive at a time. See the [measurement recipe](../tools/WarmReference/README.md).
