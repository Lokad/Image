# Changelog

## 0.2.0 — Unreleased

- Faster exact Lanczos3 normalization through certified rounding intervals and integer remainders,
  preserving pixels, memory admission and cancellation.

- Alpha-aware Lanczos3 resizing at 8/16 bits, with reduction antialiasing,
  clamped edges, aspect-preserving fit and cooperative-memory admission.

- Explicit OpenType text layout for supported Latin/Hebrew/Arabic font tables,
  with bidirectional order, joining, ligatures, marks, alignment and finite-grid
  fitting with shared admission and cumulative work across trials.

- Caller-owned `IMemoryBudget` hooks across codecs, inspection, editing and text;
  live capacities and retained results stay charged until explicit disposal.
  Fonts and prepared layouts now implement `IDisposable`.

- Asynchronous PNG/JPEG/WebP output, including caller JPEG samples and shared
  budgets, with bounded staging and JPEG adaptive entropy when admitted.
- Asynchronous stream input for identification and decoding, including shared
  budgets, cancellation and independent outputs without a final input copy.
- CMYK/YCCK JPEG decoding, including caller CMYK samples and explicit inversion,
  plus JPEG 4:1:1/4:1:0 sampling layouts.
- JPEG decoding/encoding with caller-owned grayscale/RGB samples and strides,
  explicit decode color interpretation/orientation and encode quality/sampling,
  without RGBA intermediates.
- Shared codec memory/work budgets, caller reservations and independently owned
  budgeted decode results, with typed shared-budget exhaustion.
- Typed limit and unsupported-feature details on codec exceptions, preserving
  existing exception types, messages and cancellation behavior.
- Faster subsampled JPEG color reconstruction with certified exact rounding
  and the original binary64 fallback, preserving pixels and resource limits.
- Faster VP8 encoding through prepared block prediction, preserving exact
  packets, mode selection, resource limits and cancellation.
- Faster VP8 decoding through bounded vertical SIMD filtering and whole-block
  prediction, preserving exact pixels, edge order, resource admission and cancellation.
- Faster lossless WebP decoding through fused RGBA inverse prediction and
  complete-byte literal reads, preserving exact pixels, hidden RGB, resource
  limits and cancellation behavior.
- Lower JPEG decode allocations through bounded private coefficient reuse,
  preserving exact pixels, resource admission and failure behavior.
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
- Faster RGBA8 horizontal flips with unchanged pixels, owners and resource limits.
- Faster RGBA8/16 bilinear reduction through bounded row reuse, preserving exact
  pixels, resource admission and cancellation.
- Faster RGBA8 bilinear enlargement through exact integer factorization,
  preserving pixels, working memory and rounding.
- Faster RGBA8/16 bicubic filtering through bounded row/column reuse, wider
  enlargement strips, packed coefficient broadcasts, parallel byte reductions
  and certified quotient bounds, preserving exact pixels and output ownership.
- Faster adaptive JPEG event planning through parallel integer category/amplitude
  preparation, and faster fixed-Huffman emission, preserving packets, work, limits
  and stream callback/failure behavior.
- Faster VP8 source conversion with unchanged packets, alpha and resource charges.
- Faster PNG checksums, packing, filters and palette expansion with partial
  transparency, with fewer stored-PNG allocations and unchanged samples and limits.
- Faster cancelable PNG materialization through separate block dispatch and
  bounded literal batching, alongside tight RGBA streaming inflation, preserving
  pixels, admission, work and failure ordering.

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
