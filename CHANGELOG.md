# Changelog

## Unreleased

- Faster JPEG metadata inspection through bounded entropy scanning, preserving
  complete validation, resource charges and cancellation.
- Faster progressive JPEG refinement, including live cancelable decoding,
  with unchanged pixels, resource limits and cancellation behavior;
  see the [benchmark state](docs/benchmark.md).
- Faster RGBA16 bilinear reduction through bounded row reuse, preserving exact
  pixels, resource admission and cancellation.
- Faster RGBA8/16 bicubic filtering through bounded row and column reuse,
  preserving exact pixels and output ownership.
- Faster fixed-Huffman JPEG emission with unchanged packets, work and stream
  callback/failure behavior.
- Faster VP8 source conversion with unchanged packets, alpha and resource charges.
- Faster PNG chunk checksums, byte packing, Up filtering and RGB16 Paeth decoding,
  preserving exact packets, samples and limits.

## 0.1.0 — 2026-10-17

Initial release of the safe, dependency-free .NET 10 library, published on
[NuGet](https://www.nuget.org/packages/Lokad.Image/0.1.0).

- PNG encode/decode, baseline JPEG encode/decode, progressive JPEG decode,
  and lossy/lossless WebP with alpha and exact lossless hidden RGB.
- 8/16-bit RGBA buffers, EXIF orientation, crop, flips, quarter turns, transpose,
  and nearest/bilinear/bicubic resizing.
- Stream and span APIs, independent buffer ownership, finite resource limits
  and cancellation. See [SPEC.md](SPEC.md) for supported profiles and exclusions.
- Named warm single-CPU workloads within the ≤2× FFmpeg-reference target, with
  exact-pixel/quality gates and documented [allocations and scope](docs/benchmark.md).
- Windows/Linux validation, including live FFmpeg comparisons and the same NuGet
  package exercised by an isolated consumer and all three usage examples.
