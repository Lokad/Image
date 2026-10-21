# Changelog

## 0.2.0 â€” Unreleased

- Smaller lossy WebP output through admitted 16x16 luma prediction, grouped DC
  coding, full-direction reconstruction-based luma/chroma decisions and whole-frame entropy
  modeling at higher efforts, plus exact compressed alpha with bounded backward
  references and adaptive prefix codes; broader compression parity remains open.
- Smaller constrained-work WebP packets through admitted coefficient statistics
  and reusable alpha tokens with four-sample keys and bounded older-match anchors, preserving exact
  alpha, ownership and resource caps.
- Source-derived WebP spatial quantization at higher efforts, with bounded shared
  admission, DC/AC-aware centered chroma noise propagation, unchanged base quality mapping
  and admitted intra16/Y2 search for its segments; lossy quality is verified separately.
- Admitted coefficient-level/EOB refinement for selected high-precision WebP
  predictions, reducing complete packets within verified reconstruction quality.
- Smaller constrained-work WebP packets through exact prediction pruning and
  admitted coefficient samples with token histograms, preserving reconstructed pixels.
- Admitted small-coefficient decisions further reduce constrained-work WebP
  packets within verified reconstruction quality and the existing resource caps.

- Coverage-based OpenType contextual substitution and validated zero default-instance
  positioning corrections for supplied TrueType text.

- Typed source color, alpha and component metadata across synchronous/asynchronous
  identification and bounded header inspection, preserving source sample/index depth.

- Faster VP8 forward DCT with safe SIMD, identical coefficients and packets,
  and the original scalar fallback.

- Optional `CompactColor` JPEG quantization, separate from the default formula.
- Smaller default subsampled color JPEG packets through bounded source-DCT AC decisions;
  original quantization tables, grayscale, shared admission and ownership remain unchanged.
- Source-relative block-error caps extend bounded AC decisions to default 4:4:4
  JPEG, preserving quantizers/DC and the separately verified reconstruction-quality gates.
- Caller-budget JPEG encoding keeps adaptive event storage within its operation owner,
  without borrowing or retaining an uncharged thread cache.
- Adaptive JPEG tables also reduce forward-only and cancellable packets while
  preserving checked writes, quotas and cancellation; reconstructed pixels are unchanged.

- Smaller lossless WebP output through bounded color-cache and adaptive prefix
  coding, adaptive prediction, bounded predictor/color code-cost refinements,
  residual color fits after predictor changes, uncached spatial groups and bounded
  references with seeded copy probabilities. Exact hidden RGBA and shared admission
  are preserved; the declared lossless controls fit their frozen packet-size references.

- Faster baseline JPEG decoding with unchanged pixels, work limits and cancellation.

- Certified integer SIMD for sparse and horizontal-only four-component JPEG decoding, with the
  original calculation for uncertain blocks and no new heap workspace.

- Faster exact SIMD Lanczos3 filtering at both depths, including grouped
  horizontal products, packed coefficient loads and certified FMA vertical sums
  with exact fallback. Lower scratch allocation preserves cooperative admission
  and cancellation.

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
