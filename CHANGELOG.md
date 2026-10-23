# Changelog

## 0.2.0 — Unreleased

- Reference-compatible managed JPEG encoding for grayscale/4:4:4/4:2:2/4:2:0,
  including restarts, direct caller samples and async output. Removed superseded
  experimental encoders and switches. Ten frozen controls match complete reference
  packets, allowing only the documented color attribution replacement.
- Fixed-point JPEG decoding with exact samples on14 frozen grayscale/full-resolution
  controls, shared caller RGB conversion and opt-in orientation. Removed superseded
  inverse variants; other color layouts remain under exactness qualification.
- Bounded managed PNG stored/fast/optimal/smallest encoding, with complete raw
  packet equality on14 frozen8/16-bit controls and shared admission throughout.
- Managed VP8L prediction, palette, references/cache and entropy groups reproduce
  19 selected and two additional forced frozen libwebp packets.
- Managed VP8 encoding matches20 declared complete reference packets, with all
  effort policies, exact normalized alpha and direct async partition delivery.
  Removed superseded encoder experiments; new CPU/allocation qualification is open.
- Caller-owned cooperative memory admission across nested work and retained
  images, fonts, layouts and caches, with structured limit/unsupported failures.
- Typed source color/alpha metadata, bounded header inspection and async stream
  operations that retain caller ownership.
- CMYK/YCCK and 4:1:1/4:1:0 JPEG decode, explicit color/inversion options and
  direct caller grayscale/RGB/CMYK buffers.
- Supplied TrueType measurement/rendering, wrapping, alignment and fitting,
  supported OpenType Latin/Hebrew/Arabic shaping, drawing and alpha compositing.
- Lanczos3 resizing and bounded SIMD improvements across codecs and transforms.

See [SPEC.md](SPEC.md) for supported boundaries and [benchmark state](docs/benchmark.md)
for measured scope. Exact codec replication, full performance parity and replacement
release qualification remain in progress.

## 0.1.0 â€” 2026-10-17

Initial release of the safe, dependency-free .NET 10 library, published on
[NuGet](https://www.nuget.org/packages/Lokad.Image/0.1.0).

- PNG encode/decode, baseline JPEG encode/decode, progressive JPEG decode,
  and lossy/lossless WebP with alpha and exact lossless hidden RGB.
- 8/16-bit RGBA buffers, EXIF orientation, crop, flips, quarter turns, transpose,
  and nearest/bilinear/bicubic resizing.
- Stream and span APIs, independent buffer ownership, finite resource limits
  and cancellation. See [SPEC.md](SPEC.md) for supported profiles and exclusions.
- Named warm single-CPU workloads within the â‰¤2Ã— FFmpeg-reference target, with
  exact-pixel/quality gates and documented [allocations and scope](docs/benchmark.md).
- Windows/Linux validation, including live FFmpeg comparisons and the same NuGet
  package exercised by an isolated consumer and all three usage examples.
