# YCCK420 complete decoding

The first screen demonstrates a **6.733x warm single-CPU gap** against
FFmpeg. No optimization need was assumed before this measurement. Production,
all222previous rows and their qualification remain unchanged.

| Complete resident YCCK420 → owned RGBA8 | Lokad / FFmpeg µs | Ratio / upper | Lokad allocated B/op | Input JPEG / RGBA bytes |
| --- | ---: | ---: | ---: | ---: |
| Original varying CMY/K projection | 4,761.24 / 707.13 | 6.733 / 6.911 | 1,470,168 | 6,704 / 531,468 |

One fixed fixture uses the original513x259 RGBA8 raster flattened over0x204060,
with its canonical device-CMYK projection transformed to YCCK. All four channels
vary. Full Y/K8x8 and half-width/height Cb/Cr16x16 means use clamped edges and
ties-up rounding; an original T.81 DC packet uses unit quantization, Adobe2 and
H2V2 with full-resolution black. Independent exact-quarter interpolation and the
specified binary64 matrix/inversion/black order produce exact managed pixels.
No8-bit canonical-CMYK intermediate, simpler chroma substitute, input search,
new image, shape/depth/filter/quality/sampling grid or checklist case.

Both sides decode the SAME resident packet into fresh independently owned RGBA8,
including component decode, matrix/black/color normalization and release. The
original FFmpeg adapter resets software MJPEG, receives YUVA444P with black
applied, then converts full-range BT.601 to fresh zeroed owned RGBA. Decoder
planes/output release stay timed. Only packet/context/empty wrappers are reused;
no decoded-content cache. Native output matches the public CLI exactly. Source,
two surviving independent outputs, one CPU/thread and opaque alpha pass. ICC
conversion remains off and EXIF reorientation opt-in.

FFmpeg internally expands chroma at a different phase and quantizes intermediate
samples. An independent cosited/integer model reproduces every native YUVA sample
exactly. Normal/scalar outputs are identical: max16/RMSE1.3489149162. All258RGB
samples above4 occur at authored chroma transitions; other pixels have max3.
The owner approved a **native-only** bound: RGB<=16 only at x/y mod16=0or15,
<=4 elsewhere, unchanged RGB RMSE<=1.5, exact alpha255 and exact managed pixels.
The benchmark binds this exception to the fixed packet SHA. Before approval the
strict smoke failed before any timed workload; its evidence is retained.

Three200ms actuals, all outliers, both idle-v2 guards and50ms native pre/post
brackets are retained. The above-one mean needs no long confirmation. One
native build and initial strict/approved narrow harness builds per OS; reuse
the frozen producer, compiled selector and CLI samples. A shell newline failure
occurred before native compilation and is retained. No production build,
repeated full suite or package proof: reuse4,197tests/OS,61affected hardware-off
checks/OS and same actual private package consumer/examples on both OS from
[accepted qualification](WARM-JPEG-HORIZONTAL-ROW.md). Released0.1.0 is immutable.

The finite scope stays246cases:223numeric+23unmeasured,70passes/152mean gaps/one
confidence-open. No prior row changes. This6.733x case is now the largest gap;
select one substantial primitive from a narrow profile of this newly measured
path before returning to smaller gaps. This scope does not imply other spectra,
photographs, profiles or sampling layouts have the same speed.

[Numeric evidence](warm-jpeg-ycck420-evidence.json). Raw on both hosts:

- `h10n-untimed.tar.gz` SHA`5d6bcac48eb5e0d90130c83fcab95d48f39f8e60cf42c01567b4ffca3ecc43a5`.

- `h10n-gate-smoke.tar.gz` SHA`c63cacb6f6dc11dfdeee0f3702cec6684613d31dd0638bbcde395fb2b2750124`.

- `h10n-measurements.tar.gz` SHA`91ce535d0c6e6db8d9649717f775166ef0d02ea8d7eaee286a5c355c2e1f85b6`.
