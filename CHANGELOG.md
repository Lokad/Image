# Changelog

## 0.2.1 — Unreleased

- Adaptive lossless PNG row prediction for predictable content, including
  full-precision 16-bit gradients after flips and rotations; existing declared
  reference packet controls remain unchanged.
- Faster PNG row scoring and prediction with safe SIMD, exact filter ties and
  residual bytes, portable fallback and unchanged admitted workspace.
- Faster bounded Deflate match scans for PNG Optimal/SmallestSize, preserving
  complete packets, dictionary order, ring boundaries and cancellation.
- Faster bicubic filtering and shared guarded word publication, preserving
  exact 8/16-bit pixels, arithmetic order and admitted workspace.
- Pixel-coding validation without a retained raster, with span/sync/async inputs,
  codec-dependent memory admission and the same required validity checks as decoding.
- Selective sync/async PNG/JPEG/WebP probing with explicit unknown facts, bounded
  acquisition and shared workspace admission, without a decoded raster.
- Explicit permissive EXIF handling across read APIs, preserving valid orientation,
  required container/pixel checks, checksums, limits and cancellation. Strict remains default.
- Faster stored PNG payload emission, preserving complete packet bytes, flush and
  cancellation boundaries, shared admission and output ownership.
- Constant-time Deflate distance-code selection for compressed PNG, preserving
  exact packets and portable execution.
- Faster lossless WebP color-transform scoring through trial reuse and safe SIMD,
  preserving exact packets, scratch capacity, admission and cancellation.
- Faster lossless WebP predictor scoring by sharing exact history terms across
  candidate modes, with one admitted 8 KiB buffer and unchanged packet bytes.
- Skip VP8 luma trellis searches whose candidate levels are all zero, preserving
  exact packets, validation, cancellation and portable fallback.
- Faster subsampled JPEG chroma filtering with safe SIMD, preserving exact
  packets, signed rounding, bounded scratch and portable fallback.
- Faster VP8 distortion-only mode selection through exact header-cost bounds,
  preserving packet bytes, strict ties and cancellation checkpoints.
- Faster VP8 interval coding with stack-local state and borrowed output storage,
  preserving exact packets, logical-byte admission and per-bit cancellation.
- Skip losing VP8 texture scores using reconstructed-pixel error bounds,
  preserving exact decisions, cancellation and memory ownership.
- SIMD alpha inspection and opaque luma preparation for 8-bit VP8 input,
  preserving exact packets, bounded workspace and portable fallback.

## 0.2.0 — 2026-10-27

Published on [NuGet](https://www.nuget.org/packages/Lokad.Image/0.2.0).

- Reference-compatible managed PNG/JPEG/WebP encoding on the declared packet
  controls; fixed-point JPEG and WebP decoding with exact reference sample gates.
- CMYK/YCCK and 4:1:1/4:1:0 JPEG decode; caller Gray/RGB/CMYK decode buffers,
  Gray/RGB encode samples, sampling/quality controls and asynchronous streams.
- Typed source color/alpha metadata and bounded header inspection.
- Supplied TrueType measurement/rendering, supported Latin/Hebrew/Arabic shaping,
  wrapping, alignment and fitting; drawing, compositing and Lanczos3 resizing.
- Caller-owned cooperative memory admission across nested work and retained results.
- Breaking API cleanup: removed `ImageBudget`/`BudgetedImage` and their overloads,
  `MaxWork` and cumulative CPU quotas. Use `ImageLimits.MemoryBudget`, explicit
  disposal and cooperative cancellation. See [migration](docs/migration.md).

[Supported scope](docs/spec.md), [codec parity](docs/codec-parity.md) and the
[benchmark snapshot](docs/benchmark.md) state verified boundaries. Full warm CPU
parity and general replacement readiness remain open.

## 0.1.0 — 2026-10-17

Initial release of the safe, dependency-free .NET 10 library, published on
[NuGet](https://www.nuget.org/packages/Lokad.Image/0.1.0).

- PNG encode/decode, baseline JPEG encode/decode, progressive JPEG decode,
  and lossy/lossless WebP with alpha and exact lossless hidden RGB.
- 8/16-bit RGBA buffers, EXIF orientation, crop, flips, quarter turns, transpose,
  and nearest/bilinear/bicubic resizing.
- Stream and span APIs, independent buffer ownership, finite resource limits
  and cancellation. See [SPEC.md](docs/spec.md) for supported profiles and exclusions.
- Named warm single-CPU workloads within the ≤2× FFmpeg-reference target, with
  exact-pixel/quality gates and documented [allocations and scope](docs/benchmark.md).
- Windows/Linux validation, including live FFmpeg comparisons and the same NuGet
  package exercised by an isolated consumer and all three usage examples.
