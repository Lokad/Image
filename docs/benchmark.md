# Lokad.Image benchmark state

Updated 2026-10-16. The finite [material checklist](../benchmarks/WARM-COVERAGE.md)
is **closed**: every named required path has an independent quiet upper ≤2x,
or explicitly reconciled reuse of unchanged executed paths. All61 bicubic cases,
20 exact edits, four bilinear cases and C01-C04 metadata boundaries are closed.
This states the tested fixed scope; it does not extrapolate to arbitrary images,
settings, sizes or hardware.

Dedicated AMD EPYC9V45 VM; Ubuntu24.04.4; .NET10.0.8/SDK10.0.204;
FFmpeg n8.1 (`9047fa1b084f`). One pinned CPU, one codec/conversion/filter thread,
normal SIMD, tiering off, nonconcurrent workstation GC. Inputs are the two original
513×259 RGBA8/16 rasters and documented projections; exact case dimensions and
policies are in linked reports. No image or settings grid was added.

Times include complete public operations, admission, conversion, fresh independent
output ownership and disposal. Native contexts may be reused; managed private
storage may be reused after release. No decoded output or descriptor is cached.
Fresh Stream rows include source-wrapper/input acquisition, reads/copies/EOF and
release. Startup, I/O, IPC, preparation/hashing and validation are excluded.

Ratio = Lokad.Image / faster native pre/post mean; lower is faster. Upper retains
the larger selected/all-actual confidence bound against the faster native bracket's
lower bound. Confirmed requires an independent passing quiet repeat. Recent jobs
retain all12 actuals and both5s `exclusive-vm-idle-v2` guards. Earlier records
retain their original protocol and failures. **B/op** is managed allocation from
BenchmarkDotNet/raw GC bytes per operation, rounded up. Native allocations are
**unmeasured**, not zero. Timings were collected across rounds; precise unchanged-
path reconciliation makes the retained evidence applicable to current production.

Latest production round: [fresh PNG Identify Stream](../benchmarks/WARM-PNG-STREAM-IDENTIFY.md)
**7.863x →1.445x / upper1.459x**, **184 B/op**,81.6% less managed time.
Private input reuse removes large-array churn while preserving admission/read/work/
failure semantics. Only Identify(Stream) acquisition changes; decoder input,
resident metadata, pixel codecs, editing and resize paths retain their proofs.
Fresh **3,546 Release tests per OS** (1,262 internal+1,386 public+898 live FFmpeg),
and the consumer/all three examples through the **same actual Release package on
Windows/Linux**, pass. Production remains safe managed dependency-free C#/.NET10.

**Decode and metadata**

| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [PNG Identify, fresh Stream¶](../benchmarks/WARM-PNG-STREAM-IDENTIFY.md) | 22.94 | 15.87 | 1.445 / 1.459 | 184 | Confirmed |
| [WebP Identify, resident span (parser)](../benchmarks/WARM-WEBP-IDENTIFY.md) | 0.02 | 4.54 | 0.004 / 0.004 | 88 | Confirmed |
| [JPEG Identify, resident span‖](../benchmarks/WARM-JPEG-IDENTIFY.md) | 2.23 | 1.21 | 1.840 / 1.860 | 88 | Confirmed |
| [PNG Identify, resident span¶](../benchmarks/WARM-PNG-IDENTIFY.md) | 16.09 | 13.04 | 1.234 / 1.238 | 88 | Confirmed |
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
| [Bicubic resize RGBA8§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 707.44 | 418.58 | 1.690 / 1.697 | 332,177 | Confirmed |
| [Bicubic resize RGBA16§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 747.22 | 571.05 | 1.309 / 1.319 | 464,342 | Confirmed |
| [Bicubic upscale RGBA8§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 4,738.18 | 3,047.46 | 1.555 / 1.569 | 3,244,622 | Confirmed |
| [Bicubic upscale RGBA16§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 4,386.11 | 2,867.06 | 1.530 / 1.542 | 5,016,578 | Confirmed |
| [Bicubic mixed-axis RGBA8§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,600.47 | 864.39 | 1.852 / 1.872 | 530,370 | Confirmed |
| [Bicubic mixed-axis RGBA16§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,627.26 | 851.20 | 1.912 / 1.964 | 1,060,419 | Confirmed |
| [Bicubic width reduction / height enlargement RGBA8§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 984.99 | 834.48 | 1.180 / 1.184 | 731,753 | Confirmed |
| [Bicubic width reduction / height enlargement RGBA16§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,045.67 | 824.89 | 1.268 / 1.275 | 1,263,255 | Confirmed |
| [Bicubic RGBA8, 128×64 / wider support§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 531.73 | 346.74 | 1.534 / 1.540 | 231,229 | Confirmed |
| [Bicubic RGBA16, 128×64 / wider support§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 504.51 | 416.18 | 1.212 / 1.218 | 263,991 | Confirmed |
| [Bicubic width reduction, unchanged height RGBA8§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 838.04 | 572.02 | 1.465 / 1.475 | 465,375 | Confirmed |
| [Bicubic width reduction, unchanged height RGBA16§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 895.59 | 581.76 | 1.539 / 1.552 | 730,690 | Confirmed |
| [Bicubic width enlargement, unchanged height RGBA8§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 2,642.83 | 1,520.09 | 1.739 / 1.751 | 1,930,602 | Confirmed |
| [Bicubic width enlargement, unchanged height RGBA16§](../benchmarks/WARM-CURRENT-PNG16-BICUBIC16.md) | 2,144.05 | 1,455.48 | 1.473 / 1.499 | 2,129,311 | Confirmed |
| [Bicubic RGBA8, 64×32 / support beyond 32§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 611.55 | 367.91 | 1.662 / 1.668 | 205,881 | Confirmed |
| [Bicubic RGBA16, 64×32 / support beyond 32§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 522.57 | 390.56 | 1.338 / 1.351 | 214,071 | Confirmed |
| [Bicubic RGBA8, 32×16 / support beyond 64§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 452.57 | 375.86 | 1.204 / 1.208 | 199,355 | Confirmed |
| [Bicubic RGBA16, 32×16 / support beyond 64§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 455.52 | 412.28 | 1.105 / 1.110 | 201,398 | Confirmed |
| [Bicubic RGBA8, 16×8 / adaptive support beyond 128§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 363.20 | 452.30 | 0.803 / 0.809 | 201,728 | Confirmed |
| [Bicubic RGBA16, 16×8 / adaptive support beyond 128§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 441.93 | 496.89 | 0.889 / 0.893 | 202,231 | Confirmed |
| [Bicubic RGBA8, 256×129 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 783.42 | 416.72 | 1.880 / 1.904 | 132,295 | Confirmed |
| [Bicubic RGBA16, 256×129 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 818.75 | 570.81 | 1.434 / 1.445 | 264,469 | Confirmed |
| [Bicubic RGBA8, 128×64 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 588.84 | 349.09 | 1.687 / 1.701 | 32,889 | Confirmed |
| [Bicubic RGBA16, 128×64 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 606.16 | 414.59 | 1.462 / 1.479 | 65,658 | Confirmed |
| [Bicubic RGBA8, 64×32 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 640.57 | 372.93 | 1.718 / 1.743 | 8,313 | Confirmed |
| [Bicubic RGBA16, 64×32 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 611.50 | 383.62 | 1.594 / 1.608 | 16,505 | Confirmed |
| [Bicubic RGBA16, 32×16 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 570.51 | 412.45 | 1.383 / 1.394 | 4,217 | Confirmed |
| [Bicubic RGBA8, 32×16 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 591.45 | 375.61 | 1.575 / 1.580 | 2,169 | Confirmed |
| [Bicubic RGBA16, 16×8 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 487.53 | 495.21 | 0.984 / 0.995 | 1,145 | Confirmed |
| [Bicubic RGBA8, 16×8 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 489.92 | 451.81 | 1.084 / 1.090 | 633 | Confirmed |
| [Bicubic RGBA16 enlargement / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-HORIZONTAL-UP8.md) | 5,097.69 | 2,874.50 | 1.773 / 1.783 | 4,264,616 | Confirmed |
| [Bicubic RGBA8 enlargement / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-HORIZONTAL-UP8.md) | 5,412.68 | 3,035.41 | 1.783 / 1.795 | 2,493,057 | Confirmed |
| [Bicubic RGBA16, width up / height down / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,644.19 | 848.47 | 1.938 / 1.957 | 1,060,419 | Confirmed |
| [Bicubic RGBA8, width up / height down / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,617.87 | 860.30 | 1.881 / 1.901 | 530,370 | Confirmed |
| [Bicubic RGBA16, width down / height up / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,298.90 | 825.27 | 1.574 / 1.579 | 1,063,477 | Confirmed |
| [Bicubic RGBA8, width down / height up / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,249.21 | 831.82 | 1.502 / 1.513 | 531,910 | Confirmed |
| [Bicubic RGBA16, 8×4 / support beyond 256 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 445.77 | 693.80 | 0.643 / 0.651 | 377 | Confirmed |
| [Bicubic RGBA8, 8×4 / support beyond 256 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 508.41 | 664.36 | 0.765 / 0.772 | 249 | Confirmed |
| [Bicubic RGBA16, 4×2 / packed support beyond 304 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 624.81 | 1,235.83 | 0.506 / 0.509 | 185 | Confirmed |
| [Bicubic RGBA8, 4×2 / packed support beyond 304 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 786.38 | 1,138.73 | 0.691 / 0.695 | 153 | Confirmed |
| [Bicubic RGBA16, 2×2 / generated support beyond 544 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,069.82 | 2,336.26 | 0.458 / 0.487 | 154 | Confirmed |
| [Bicubic RGBA16, 2×9 / bounded row blocks / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,474.00 | 1,286.00 | 1.146 / 1.180 | 266 | Confirmed |
| [Bicubic RGBA16, 4×9 / packed row blocks / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 808.81 | 789.44 | 1.025 / 1.030 | 409 | Confirmed |
| [Bicubic RGBA16, 8×17 / full-coefficient row blocks / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-8X17-RGBA16.md) | 516.33 | 530.91 | 0.973 / 0.981 | 1,209 | Confirmed |
| [Bicubic RGBA8, 8×17 / full-coefficient row blocks / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-8X17-RGBA8.md) | 538.78 | 494.65 | 1.089 / 1.100 | 665 | Confirmed |
| [Bicubic RGBA8, 4×9 / packed row blocks / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,002.56 | 675.80 | 1.484 / 1.488 | 266 | Confirmed |
| [Bicubic RGBA8, 2×9 / bounded row blocks / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,337.80 | 1,050.39 | 1.274 / 1.286 | 194 | Confirmed |
| [Bicubic RGBA8, 2×2 / generated support beyond 544 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,173.66 | 1,633.74 | 0.718 / 0.745 | 138 | Confirmed |
| [Bicubic height reduction, unchanged width RGBA8§](../benchmarks/WARM-BICUBIC-TIGHT-VERTICAL8.md) | 756.93 | 588.11 | 1.287 / 1.301 | 648,992 | Confirmed |
| [Bicubic height reduction, unchanged width RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-VERTICAL8.md) | 1,063.64 | 583.82 | 1.822 / 1.838 | 264,995 | Confirmed |
| [Bicubic height reduction, unchanged width RGBA16§](../benchmarks/WARM-BICUBIC-TIGHT-VERTICAL16.md) | 832.49 | 585.00 | 1.423 / 1.428 | 913,737 | Confirmed |
| [Bicubic height reduction, unchanged width RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-VERTICAL16.md) | 1,059.29 | 584.28 | 1.813 / 1.823 | 529,838 | Confirmed |
| [Bicubic width reduction, unchanged height RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 838.73 | 579.56 | 1.447 / 1.459 | 530,858 | Confirmed |
| [Bicubic width reduction, unchanged height RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-HORIZONTAL8.md) | 811.19 | 573.66 | 1.414 / 1.425 | 265,502 | Confirmed |
| [Bicubic height enlargement, unchanged width RGBA8§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 2,206.63 | 1,732.64 | 1.274 / 1.282 | 1,664,240 | Confirmed |
| [Bicubic height enlargement, unchanged width RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 3,003.25 | 1,739.69 | 1.726 / 1.737 | 1,280,402 | Confirmed |
| [Bicubic height enlargement, unchanged width RGBA16§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 2,108.79 | 1,674.17 | 1.260 / 1.270 | 2,516,605 | Confirmed |
| [Bicubic height enlargement, unchanged width RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 2,719.36 | 1,667.94 | 1.630 / 1.647 | 2,133,096 | Confirmed |
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

* VP8L encode uses **libwebp exact=1, FFmpeg's encoder backend**, through its public
API: the FFmpeg encoder does not expose hidden-RGB preservation. Decoded RGBA is
exact; managed/native packets are533,100/183,812 B. Compression-efficiency parity
is not established. Single-component grayscale JPEG uses approved benchmark-only
**SIMD libjpeg-turbo3.1.4.1**, since FFmpeg emits three components:1.388x/upper1.398x,
154,893 B/op,58,823/58,910 B compressed; source-PSNR gap0.000033dB. Complete
normalization and output ownership are timed.

‡ VP8 encode freezes managed/native quality100/75, effort4; its PSNR gap0.00365dB
passes the unchanged0.25dB gate, with exact alpha. Packets471,436/98,590 B do not
establish compression-efficiency parity. JPEG lossy comparisons preserve their
frozen source-quality-matched settings and0.25dB band; sizes remain in linked reports.

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
Stop passing cases. Add a later case only when evidence identifies a distinct
required algorithm or public boundary omitted from the finite checklist.
