# JPEG 4:1:0 owned decode

The first screen finds a **7.144x gap** on the already-named H4V2 JPEG path.
Managed pixels are exact; native maximum RGB error is1, RMSE0.136, with exact
alpha. The unchanged comparison gate remains maximum4/RMSE1.5.

| Complete resident operation | Lokad / FFmpeg µs | Ratio / upper | Lokad allocated B/op | JPEG / RGBA bytes |
| --- | ---: | ---: | ---: | ---: |
| JPEG H4V2 / 4:1:0 → owned RGBA8 | 3,427.72 / 479.78 | 7.144 / 7.585 | 720,759 | 3,399 / 531,468 |

The existing original513×259 image, flattened over `0x204060`, reuses its frozen
YCbCr projection. An original T.81 DC-only/unit-quantization packet uses full Y
8×8 means and quarter-width/half-height Cb/Cr32×16 means with clamped edges.
Independent integer-eighth horizontal/quarter vertical interpolation and the
specified binary64 matrix define exact RGBA samples. JPEG H4V2 is distinct from
the video YUV410p format, which has quarter-height chroma.

FFmpeg software MJPEG returns YUVJ420P after horizontal expansion. Every original
quarter-width sample survives at an even position; all returned component bytes,
including inserted averages, are checked analytically. The worker recovers these
samples and performs centered vertical expansion in one pass, rounding vertical
fractions to native8bit. It then uses full-range BT.601 centered516-column
bilinear/accurate/full-chroma/bitexact conversion and compacts RGBA rows in place
to513. Normal/scalar public CLI normalization and the worker agree byte-for-byte.
The packet and valid decoded samples stay unchanged. No eased gate, separate
recovered-plane copy or second RGBA raster.

Both sides time complete resident decoding, all component transforms, required
normalization, fresh zeroed independent owned output and release. Native reset,
sample recovery/interpolation, chroma staging acquisition/release, conversion,
compaction and decoded-plane release are included. Only codec/conversion/packet
and empty frame wrappers are reused. ICC/EXIF transforms are off. One CPU/thread;
startup, I/O, IPC and checks are excluded. Source and surviving owners are checked.

The short retains all three200ms actuals/outliers,50ms native pre/post brackets
and both idle-v2 guards. No long repeat above one. An initial fixture gate looked
for a duplicate projection instead of the shared411 file; it failed before any
actual measurement. Correcting that path required a second harness build per OS.
One native worker build, no production/selector build or package/full-suite repeat.

Reuse the unchanged production qualification:4,210 tests/OS,97 hardware-off checks
per OS and the same actual private package consumer/all three examples on both OS.
All226 earlier numeric rows and70passes retain evidence. Scope247=227numeric+
20unmeasured now has70passes/156gaps/one confidence-open case.

No new profile: this layout uses the unchanged general Render/Interpolate/Round
methods covered by the retained [411 profile](WARM-JPEG-411.md). That profile
guides the next quarter-width/half-height SIMD primitive; its percentages are
not claimed as a410 measurement. No image/settings grid or retuning of passes.

[Numeric evidence](warm-jpeg-410-evidence.json). Raw retained on both hosts:

- `h10y-410-preflight.tar.gz`: SHA `78a377ad05ead997530d48375b9181ee3c4f0bdd7359e8bc91e7264efee89cb6`.
- `h10y-initial-gate.tar.gz`: SHA `ece9dcaee964ba164e31d6156eda5f4068ca4b03be9770f40e40ee84883fda8b`.
- `h10y-measurements.tar.gz`: SHA `3058e7957ff74d0114684f8acefd81025880d1d3b0dc76c0e065029b54617f1b`.
