# Lokad.Image benchmark state

Updated 2026-10-12. Latest retained warm measurements on the dedicated AMD EPYC
9V45 VM: Ubuntu 24.04.4, .NET 10.0.8 / SDK 10.0.204, FFmpeg n8.1
(`9047fa1b084f`). One pinned CPU, one codec/conversion/filter thread, normal SIMD,
tiering off, workstation nonconcurrent GC. Two original 513×259 RGBA8/16 rasters;
resize/crop outputs are 256×129, quarter turns/transposes 259×513.

Times cover complete public operations and independent output acquisition/release,
including admission, conversion and disposal. Native wrappers/contexts are reused;
managed editing may reuse released pixel storage. No output is cached. Startup,
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
| [PNG RGBA16, None](../benchmarks/WARM-PNG-LITERAL-RUN.md) | 5,740.60 | 2,931.33 | 1.958 / 1.968 | 2,032,864 | Confirmed |
| [PNG RGBA8, Paeth](../benchmarks/WARM-PAETH-FOUR-CHANNEL.md) | 5,099.55 | 2,925.24 | 1.743 / 1.842 | 881,892 | Confirmed |
| [PNG RGBA16, Paeth](../benchmarks/WARM-PAETH-VECTOR.md) | 8,778.54 | 5,671.15 | 1.548 / 1.607 | 1,844,468 | Confirmed |
| [PNG RGBA16, Adam7](../benchmarks/WARM-PNG-SCATTER.md) | 6,567.82 | 3,984.54 | 1.648 / 1.700 | 2,024,532 | Confirmed |
| [JPEG baseline gray](../benchmarks/WARM-JPEG-AC-FUSED.md) | 1,267.57 | 682.73 | 1.857 / 1.880 | 1,221,545 | Confirmed |
| [JPEG baseline 4:2:0](../benchmarks/WARM-JPEG420-BULK.md) | 1,721.81 | 1,007.73 | 1.709 / 1.719 | 1,619,530 | Confirmed |
| [JPEG baseline 4:2:2](../benchmarks/WARM-JPEG422-ROWS.md) | 2,130.87 | 1,333.83 | 1.598 / 1.614 | 1,978,646 | Quiet guard failed / open |
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
| [PNG RGBA16, level 6 / None](../benchmarks/WARM-PNG16-ENCODE-CRC.md) | 16,470.31 | 19,951.52 | 0.826 / 0.838 | 2,103,836 | Confirmed |
| [JPEG 4:4:4, matched quality](../benchmarks/WARM-JPEG-ENCODE-QUIET.md) | 7,390.28 | 3,779.05 | 1.956 / 1.969 | 1,051,857 | Confirmed |
| [WebP VP8 + alpha](../benchmarks/WARM-VP8-CHROMA-REJECTED.md) | — | — | — | — | Unmatched quality; no timing |
| [WebP VP8L, exact backend*](../benchmarks/WARM-VP8L-ENCODE-EXACT.md) | 7,363.30 | 121,587.95 | 0.061 / 0.063 | 2,100,170 | Confirmed |

**Editing**

| Operation / fixed profile | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [Nearest resize RGBA8](../benchmarks/WARM-NEAREST-BASELINE.md) | 313.08 | 238.62 | 1.312 / 1.497 | 132,434 | Quiet guard failed / open |
| [Nearest resize RGBA16](../benchmarks/WARM-PREPARED-CONFIRM.md) | 105.69 | 170.58 | 0.620 / 0.629 | 264,466 | Confirmed |
| [Bilinear resize RGBA8](../benchmarks/WARM-BILINEAR8-QUIET.md) | 729.12 | 376.29 | 1.938 / 1.958 | 157,955 | Confirmed |
| [Bilinear resize RGBA16](../benchmarks/bilinear16-precision-evidence.json) | — | — | — | — | Unmatched alpha; no timing |
| [Bicubic resize RGBA8](../benchmarks/bicubic8-boundary-evidence.json) | — | — | — | — | Unmatched boundary/alpha; no timing |
| [Crop RGBA16](../benchmarks/WARM-CROP-COPY.md) | 4.11 | 4.15 | 0.991 / 1.003 | 96 | Confirmed |
| [Clone RGBA16](../benchmarks/WARM-CLONE-REUSE.md) | 17.86 | 16.60 | 1.076 / 1.089 | 96 | Confirmed |
| [Normal orientation RGBA16](../benchmarks/WARM-NORMAL-ROWS.md) | 17.92 | 16.87 | 1.062 / 1.087 | 128 | Quiet guard failed / open |
| [Vertical flip RGBA16](../benchmarks/WARM-VFLIP-ROWS.md) | 17.78 | 17.75 | 1.002 / 1.032 | 128 | Quiet guard failed / open |
| [Horizontal flip RGBA8](../benchmarks/WARM-HORIZONTAL-FLIP.md) | 12.05 | 9.62 | 1.253 / 1.283 | 128 | Quiet certification open |
| [Horizontal flip RGBA16](../benchmarks/WARM-HORIZONTAL-FLIP.md) | 22.00 | 22.86 | 0.963 / 1.057 | 128 | Quiet certification open |
| [Half turn RGBA16](../benchmarks/WARM-HALFTURN-PIXELS.md) | 20.78 | 22.28 | 0.933 / 0.967 | 128 | Confirmed |
| [Clockwise quarter turn RGBA16](../benchmarks/WARM-CLOCKWISE-TILES.md) | 43.69 | 44.12 | 0.990 / 1.013 | 128 | Confirmed |
| [Counterclockwise quarter turn RGBA16](../benchmarks/WARM-COUNTERCLOCKWISE-TILES.md) | 49.14 | 47.12 | 1.043 / 1.062 | 128 | Quiet guard failed / open |
| [Transpose RGBA8](../benchmarks/WARM-TRANSPOSE8-PARITY.md) | 38.00 | 38.36 | 0.991 / 1.002 | 128 | Confirmed |
| [Transpose RGBA16](../benchmarks/WARM-TRANSPOSE-TILES.md) | 48.90 | 44.95 | 1.088 / 1.093 | 128 | Quiet guard failed / open |
| [Mirrored transpose RGBA16](../benchmarks/WARM-TRANSVERSE-TILES.md) | 45.49 | 44.57 | 1.021 / 1.033 | 128 | Quiet guard failed / open |

Allocations are managed bytes per timed operation, from BenchmarkDotNet memory
diagnostics or raw GC allocated bytes / operation count (rounded up, including
harness overhead). Decoder timing uses resident encoded spans. Separate fresh
public Stream controls measure Paeth8 **2,275,800**, Paeth16 **4,713,416**, and
Adam7-16 **5,058,912 B/op**, identically on both OSes with normal/scalar code.
These are distinct boundaries. FFmpeg allocation totals are **not measured**;
they must not be interpreted as zero. Retained owners and cold allocation patterns
need separate coverage.

Other depths/directions/profiles and horizontal-flip quiet
certification remain open. Unmatched cases receive no parity timing. GIF and
animation are deferred. No averages establish the uniform ≤2× target.
Horizontal flips retain the worst upper bound of their two historical launches.

Maintain this summary whenever a performance round is accepted: update the
corresponding evidence selection in [the renderer](../benchmarks/render-benchmark.py),
regenerate with `python benchmarks/render-benchmark.py`, and run it with `--check`.
Preserve superseded raw evidence and failed runs in their linked reports. Add a
new row only for material coverage; prioritize the largest remaining gap one
primitive at a time. See the [measurement recipe](../tools/WarmReference/README.md).
