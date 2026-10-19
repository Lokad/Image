# JPEG 4:1:1 owned decode

The first screen finds a **7.771x gap**, with exact managed and native pixels.
The finite checklist already required this quarter-width chroma path; no image,
shape, depth, quality or sampling grid was added.

| Complete resident operation | Lokad / FFmpeg µs | Ratio / upper | Lokad allocated B/op | JPEG / RGBA bytes |
| --- | ---: | ---: | ---: | ---: |
| JPEG H4V1 → owned RGBA8 | 3,527.46 / 453.95 | 7.771 / 8.211 | 751,223 | 3,904 / 531,468 |

The existing original 513×259 raster is flattened over `0x204060` and projected
to YCbCr. An original T.81 DC-only, unit-quantization packet uses full-resolution
8×8 Y means and quarter-width 32×8 Cb/Cr means, with clamped edges. Independent
integer-eighth centered interpolation and the specified binary64 matrix produce
the exact RGBA oracle. JPEG 4:1:0 (H4V2) remains a separate required case.

Public FFmpeg software MJPEG planes match the analytical component samples
exactly. Its default odd-width conversion misses the unchanged maximum4/RMSE1.5
gate (maximum26); an initial CLI pad filter also fails. Neither was timed.
The reference instead preserves all 129 real chroma samples on their exact
516-column grid: extend only owned luma padding, use explicit centered
full-range BT.601 bilinear/accurate/full-chroma/bitexact conversion, then compact
RGBA rows in place to513. Normal and scalar public CLI normalization and the
worker match every reference pixel. The packet and valid decoded samples stay
unchanged; there is no second RGBA raster or eased comparison bound.

Both sides time complete resident packet decoding, all components, chroma/color
normalization, fresh zeroed independently owned output and release. Native reset,
luma padding, conversion, compaction and decoded-plane release are included.
Only codec/conversion contexts, packet and empty frame wrapper are reused.
ICC conversion and EXIF reorientation are off. Source and two surviving output
owners are checked. One CPU/thread, normal SIMD; startup, I/O, IPC and checks
are excluded. The short has three 200ms actuals, 50ms native pre/post brackets,
all actuals/outliers and both idle-v2 guards. No long repeat above one.

One six-second profile (5,663 samples) locates **90.0%** in scalar interpolation
(43.6%), rounding (29.4%) and rendering (17.0%). Next: one exact quarter-width
SIMD row primitive, keeping scalar borders/fallback, matrix order and work limits.

Production is unchanged: reuse4,209 tests/OS,96 hardware-off checks/OS and the
same actual private package consumer/all three examples on both OS. One harness
build/OS, one original native worker build and reused compiled selectors. No
production/package/full-suite rebuild. All225 earlier rows and70 passes remain
valid. Scope247=226numeric+21unmeasured:70passes/155gaps/one confidence-open.

[Numeric evidence](warm-jpeg-411-evidence.json). Raw retained on both hosts:

- `h10w-411-preflight.tar.gz`: SHA `8bc35a0921d3ff29f724caf5e87abb9803a3b9cc88b075f20fc76b2fa67eb9e6`.
- `h10w-measurements.tar.gz`: SHA `760bf25b855b8041b25454f6bdc58e1a0dececd2a42e4ed5be5627c1eeff1f88`.
- `h10w-profile.tar.gz`: SHA `9c6a00f1fa8a64f229862530a8cf9fb3264d19a798ee49f5279232ddad4583a4`.
