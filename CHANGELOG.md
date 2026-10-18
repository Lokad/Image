# Changelog

## Unreleased

- Faster cache-coded lossless WebP decoding with unchanged pixels, working
  memory, work limits and cancellation.
- Faster JPEG metadata inspection through bounded entropy scanning, preserving
  complete validation, resource charges and cancellation.
- Faster cancelable baseline JPEG decoding through bounded AC lookahead reuse,
  preserving pixels, work charges and partial-failure state.
- Faster progressive JPEG refinement, including live cancelable decoding,
  through bounded lookahead reuse with unchanged pixels, resource limits,
  logical failure state and cancellation behavior;
  see the [benchmark state](docs/benchmark.md).
- Faster RGBA8/16 bilinear reduction through bounded row reuse, preserving exact
  pixels, resource admission and cancellation.
- Faster RGBA8 bilinear enlargement through exact integer factorization,
  preserving pixels, working memory and rounding.
- Faster RGBA8/16 bicubic filtering through bounded row/column reuse, wider
  enlargement strips, packed weight broadcasts and specialized byte output, preserving exact
  pixels and output ownership.
- Faster adaptive JPEG event planning and fixed-Huffman emission with unchanged
  packets, work and stream callback/failure behavior.
- Faster VP8 source conversion with unchanged packets, alpha and resource charges.
- Faster PNG chunk checksums, byte packing, Up/Sub filtering and RGB16 Paeth decoding,
  preserving exact packets, samples and limits.
- Faster PNG RGBA decoding at tight working limits through bounded streaming
  inflation, preserving pixels, admission, work and failure ordering.

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
