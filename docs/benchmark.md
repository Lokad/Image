# Lokad.Image benchmark state

Updated 2026-10-13. Latest retained warm measurements on the dedicated AMD EPYC
9V45 VM: Ubuntu 24.04.4, .NET 10.0.8 / SDK 10.0.204, FFmpeg n8.1
(`9047fa1b084f`). One pinned CPU, one codec/conversion/filter thread, normal SIMD,
tiering off, workstation nonconcurrent GC. Two original 513×259 RGBA8/16 rasters,
with documented projections for other pixel formats;
resize/crop outputs are 256×129, quarter turns/transposes 259×513.

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
| [PNG gray-alpha8, None](../benchmarks/WARM-PNG-GRAY-ALPHA8-ROW.md) | 1,574.21 | 1,147.07 | 1.372 / 1.409 | 661,014 | Confirmed |
| [PNG RGB8, None](../benchmarks/WARM-PNG-RGB8-ROW.md) | 2,139.12 | 1,510.62 | 1.416 / 1.440 | 727,305 | Confirmed |
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
| [JPEG 4:4:4, matched quality](../benchmarks/WARM-JPEG-ENCODE-QUIET.md) | 7,390.28 | 3,779.05 | 1.956 / 1.969 | 1,051,857 | Confirmed |
| [JPEG 4:2:2, matched quality](../benchmarks/WARM-JPEG422-ENCODE.md) | 4,632.95 | 2,359.67 | 1.963 / 1.990 | 527,307 | Confirmed |
| [JPEG 4:2:0, matched quality](../benchmarks/WARM-JPEG420-CHROMA-ROW.md) | 3,605.37 | 1,822.42 | 1.978 / 2.022 | 527,335 | Upper above 2x / open |
| [WebP VP8 + alpha‡](../benchmarks/WARM-VP8-ALPHAWEIGHTED.md) | 42,879.92 | 54,484.52 | 0.787 / 0.805 | 2,615,472 | Confirmed |
| [WebP VP8L, exact backend*](../benchmarks/WARM-VP8L-ENCODE-EXACT.md) | 7,363.30 | 121,587.95 | 0.061 / 0.063 | 2,100,170 | Confirmed |

**Editing**

| Operation / fixed profile | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [Nearest resize RGBA8](../benchmarks/WARM-COVERAGE-QUIET.md) | 58.67 | 252.66 | 0.232 / 0.241 | 132,294 | Confirmed |
| [Nearest resize RGBA16](../benchmarks/WARM-PREPARED-CONFIRM.md) | 105.69 | 170.58 | 0.620 / 0.629 | 264,466 | Confirmed |
| [Bilinear resize RGBA8](../benchmarks/WARM-BILINEAR8-QUIET.md) | 729.12 | 376.29 | 1.938 / 1.958 | 157,955 | Confirmed |
| [Bilinear resize RGBA16†](../benchmarks/WARM-BILINEAR16-REGISTER.md) | 784.03 | 418.41 | 1.874 / 1.908 | 290,118 | Confirmed |
| [Bicubic resize RGBA8](../benchmarks/bicubic8-boundary-evidence.json) | — | — | — | — | Unmatched boundary/alpha; no timing |
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

† RGBA16 bilinear allows native-only alpha maximum error 1 / RMSE <=0.25 in
full 16-bit sample units, alongside the same RGB bound. Managed pixels still
match the exact rational reference; RGBA8 native alpha stays exact. The latest
RGBA16 independent confirmation clears 2x with both quiet guards passing.
Earlier failed screens/prechecks and rejected prototypes remain recorded in
the linked reports.

‡ VP8 encode fixes native quality 75 / managed quality 100 at effort 4. Source
PSNR gap is 0.00365 dB within the unchanged 0.25 dB band; alpha is exact.
Managed output is 471,436 bytes versus 98,590 bytes. Quality-matched timing is
confirmed; compression efficiency parity is not established.

Allocations are managed bytes per timed operation, from BenchmarkDotNet memory
diagnostics or raw GC allocated bytes / operation count (rounded up, including
harness overhead). Decoder timing uses resident encoded spans. Separate fresh
public Stream controls measure Paeth8 **2,275,800**, Paeth16 **4,713,416**, and
Adam7-16 **5,058,912 B/op**, identically on both OSes with normal/scalar code.
These are distinct boundaries. FFmpeg allocation totals are **not measured**;
they must not be interpreted as zero. Retained owners and cold allocation patterns
need separate coverage.

The [material coverage audit](../benchmarks/WARM-COVERAGE.md) lists distinct
unmeasured primitives and public boundaries without expanding a profile matrix.
Other depths/directions/profiles remain open. Unmatched cases receive no parity
timing. GIF and
animation are deferred. No averages establish the uniform ≤2× target.
Historical horizontal-flip launches and their bounds remain in the earlier report.

The latest [PNG Average RGBA8 check](../benchmarks/WARM-PNG-AVERAGE8-BASELINE.md)
independently confirms 1.704x / upper 1.743x with 12,976 B/op using unchanged
production. All five filters have a confirmed RGBA8 case. The
[RGB8 encode check](../benchmarks/WARM-PNG-RGB8-ENCODE-BOUNDARY.md) is exact in
pixels/scanlines but unscored: IDAT sizes differ by 12.31%, exceeding the existing
5% gate. Row batching recovers only 120 bytes; the compression comparison remains
open. The [RGB16 encode check](../benchmarks/WARM-PNG-RGB16-ENCODE.md) independently
confirms 0.865x / upper 0.871x with 2,102,765 B/op and unchanged production.
The [gray8 encode check](../benchmarks/WARM-PNG-GRAY8-ENCODE-BOUNDARY.md) also has
exact pixels/scanlines but remains unscored: IDAT sizes differ by 16.65%, and
batching saves only 34 bytes. Its compression comparison remains open.
The [gray16 encode check](../benchmarks/WARM-PNG-GRAY16-ENCODE.md) independently
confirms 1.150x / upper 1.156x with 1,051,455 B/op. Its native baseline uses exact
red-plane extraction and includes conversion/intermediate ownership within each
operation. Production is unchanged.
The [gray-alpha8 encode check](../benchmarks/WARM-PNG-GRAY-ALPHA8-ENCODE-BOUNDARY.md)
preserves every sample with explicit timed lossless storage packing before the
native PNG encoder, but remains unscored: IDAT sizes differ by 5.17%; batching
saves only 61 bytes. Native colorspace conversion's alpha rounding is rejected.
The [gray-alpha16 encode check](../benchmarks/WARM-PNG-GRAY-ALPHA16-ENCODE.md)
independently confirms 0.903x / upper 0.915x with 1,052,368 B/op. Both complete
words and hidden RGB are exact; native storage packing/endian conversion occurs
inside every operation before FFmpeg PNG encoding. Production is unchanged.
Other material layouts and encoding projections remain open. The
[NoCompression RGBA8 check](../benchmarks/WARM-PNG-STORED8-ENCODE.md) drops from
11.526x to an independently confirmed 1.883x / upper 1.933x, with 2,102,472 B/op.
One bulk row-copy primitive preserves encoded bytes, admission and cancellation;
full tests and the same actual package pass on both OSes. The
[stored-Deflate RGBA8 decode check](../benchmarks/WARM-PNG-STORED8-DECODE.md)
independently confirms 1.159x / upper 1.168x with 12,976 B/op, unchanged production
and exact frozen input. The
[RGB16 check](../benchmarks/WARM-PNG-RGB16-CURRENT.md) is confirmed at 1.819x / upper
1.823x with 15,024 B/op. JPEG420's earlier upper-bound gap remains open.
Stop tuning confirmed pairs. Other profiles, ownership boundaries and the full
required scope remain open; earlier failures and rejected experiments stay recorded.

Maintain this summary whenever a performance round is accepted: update the
corresponding evidence selection in [the renderer](../benchmarks/render-benchmark.py),
regenerate with `python benchmarks/render-benchmark.py`, and run it with `--check`.
Preserve superseded raw evidence and failed runs in their linked reports. Add a
new row only for material coverage; prioritize the largest remaining gap one
primitive at a time. See the [measurement recipe](../tools/WarmReference/README.md).
