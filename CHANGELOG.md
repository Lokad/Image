# Changelog

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
