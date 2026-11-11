# Changelog

## 0.2.1 — Unreleased

- Share byte/word bilinear reduction preparation and remove unused horizontal
  storage, preserving exact pixels, cancellation and shared memory ownership.

- Share certified bicubic byte publication and ordered vertical quartets across
  cached, bounded and enlargement execution, preserving exact rounding, repair
  and memory ownership.

- Prepare VP8 coefficient positions once per frame for shared ordinary, cancelable
  and validation decoding, preserving exact samples and memory admission.

- Prepare exact JPEG block support alongside dequantization and reuse it across
  inverse-transform decisions, preserving decoded samples and encoder packets.

- Add caller-owned text render plans with independently owned geometry and
  admitted reusable workspace, preserving exact drawing and per-call cancellation.

- Add caller-owned bicubic plans with shared exact preparation/execution,
  admitted retained storage and independently owned results.

- Prepare VP8 dequantizers once per frame segment, preserving exact decoded
  pixels, bounded scratch admission and independent output ownership.

- Reuse unpublished resize output for larger bicubic row bands, preserving exact
  pixels, bounded scratch admission and independently owned results.

- Share VP8 quantizer/rate preparation across luma/chroma searches, using bounded
  price views and deferred scratch writes with exact packets, unchanged admitted
  capacity and portable fallbacks.

- Apply live progressive JPEG correction bits through exact integer selection,
  preserving pixels, cancellation prefixes and overflow behavior.

- Buffer live VP8 coefficient decoding without repeated code-value shifts,
  preserving exact pixels, cancellation prefixes and output ownership.

- Share byte/word bicubic enlargement preparation and output-backed scheduling,
  reducing temporary storage while preserving exact pixels and memory ownership.

- Prepare complete VP8 prediction candidates from shared borders and interpolation,
  preserving exact packets, cancellation and output ownership.

- Progressive full-resolution JPEG decoding avoids two complete sample planes,
  preserving exact pixels, cancellation and shared memory ownership.

- Build fresh WebP color histograms without redundant prior-support scans,
  preserving exact packets and allocation/ownership contracts.

- Share WebP input preparation through bounded byte/word tiles and prepared alpha
  weights, preserving exact packets, portable fallbacks and workspace admission.

- Prepare fixed VP8 trellis importance without repeated exponential evaluation,
  preserving checked custom models, exact packets and memory ownership.

- Share exact VP8 residual preparation between statistics and partition encoding,
  with vectorized validation/termination and a portable scalar path.

- Quantize complete VP8 coefficient blocks with AVX512F, preserving exact packets,
  zero per-block allocation and the AVX2/scalar fallbacks.

- Prepare WebP matching for uncancelable tokens and read owned chain links ahead
  of comparisons, preserving live cancellation checks, exact packets and ownership.

- Batch alpha predictor residuals by tile-row runs with shared row storage,
  preserving exact packets, scalar fallbacks, cancellation and workspace ownership.

- Share cache-hit discovery and missed-prefix updates for normalized WebP alpha,
  preserving exact packets, existing workspace and cancellation behavior.

- Share Lanczos RGB and certificate preparation in SIMD lanes, preserving exact
  pixels, portable fallbacks, cancellation and memory ownership.

- Share word bicubic geometry preparation and widen eligible row strips within
  the same 4 KiB admission, preserving exact pixels and output ownership.

- Reuse that preparation for wider byte/word bicubic strips without changing
  their filtering kernels, exact pixels or 4 KiB workspace admission.

- Keep certified VP8 inverse reconstruction in packed SIMD arithmetic, preserving
  exact encoder packets, full-range/scalar fallbacks and output ownership.

- Prepare tight-memory bicubic column geometry in owned output storage, preserving
  exact pixels, the 4 KiB workspace and independent output lifetimes.

- Batch JPEG grayscale conversion with exact binary64 SIMD arithmetic, preserving
  encoder packets, scalar execution and memory ownership.

- Share bicubic streaming schedulers and avoid repeated row filtering within the
  existing 4 KiB workspace, preserving exact pixels and independent output ownership.

- PNG encoding shares block/tree workspace, prepares canonical codes in one pass
  and batches literal output with compact dictionary history, preserving exact
  packets, cancellation and memory ownership.

- Transparent WebP encoding reuses match/token storage across predictor analysis,
  residual rows and sequential alpha filters, then hands completed alpha workspace
  to frame coefficients, preserving exact packets, admission and independent outputs.

- Text raster preparation skips disjoint edge pairs and carries active crossings
  between bands, preserving exact pixels, cancellation and shared admission.

- Lanczos3 shares row scheduling and result ownership across exact/SIMD execution.
  Reusable caller-owned plans retain admitted geometry and working rows,
  preserving exact pixels, cancellation and independent result lifetimes.
- Lossless WebP compacts sparse histograms, reuses rejected token storage and
  borrows idle output capacity for trace working state, with shared predictor
  preparation, invariant alpha scoring and prepared-body counting preserving
  exact packets.
- WebP clustering retains immutable sparse leaves and recycles merged populations
  within each encode, reducing allocations and admitted peak storage while
  preserving exact packets and independent output ownership. Prepared green
  population models and shared fixed-weight bias pricing reduce recurring
  candidate-cost computation; compact banks avoid unused prepared storage.

- Vectorized complete-pixel rectangle filling preserves full sample precision,
  hidden RGB, clipping, cancellation and existing memory ownership.

- Faster exact rectangle copies at both pixel depths, preserving overlap,
  clipping, cancellation and caller-owned storage.
- Wider bicubic resizing repairs uncertain pixels independently, preserving exact
  pixels, cancellation and existing memory ownership.
- Speed JPEG decoder preflight with bounded marker searches, preserving exact
  pixels, container limits and cooperative cancellation.
- Decode half-grid YCCK directly into caller RGB rows through the existing exact
  color kernel, preserving padding, ownership and cancellation.
- Equal-grid baseline RGB JPEG decoding writes into the retained result,
  avoiding temporary component planes while preserving exact pixels and admission.
- Faster 4:2:0 and 4:2:2 JPEG encoding through exact SIMD vertical chroma rows,
  preserving packets, scalar fallbacks and existing memory ownership.
- Faster RGBA8/16 transpose through safe SSE2/AVX2, preserving exact pixels, scalar
  fallbacks, cancellation and shared-memory ownership.
- Exact low-alpha resizing uses Int128 where overflow bounds permit, reducing
  temporary allocations while retaining arbitrary-precision fallback and admission.
- Tight-memory RGBA8 bicubic enlargement avoids a redundant error-bound division,
  preserving exact pixels, admitted storage and portable fallbacks.
- Mixed bicubic resizing reuses exact horizontal geometry in private result
  storage, preserving pixels, working limits and output ownership.
- RGBA8 bilinear enlargement reuses exact horizontal rows in existing geometry
  storage, reducing CPU while preserving pixels, admission and output ownership.
- Lossless WebP color search scores only bins recorded during histogram
  construction, preserving exact packets and existing scratch capacity.
- Bounded JPEG probes combine marker reads while preserving input limits,
  cancellation, prefix boundaries and caller-owned admission.
- Buffered and seekable header skips complete without an async state machine,
  preserving bounded reads, cancellation and pending-I/O storage lifetimes.
- Non-indexed lossless WebP decoding owns decoded storage directly, avoiding a
  temporary full frame while preserving exact pixels and caller-budget lifetimes.
- RGBA16 bilinear reduction reuses vertical geometry and avoids unused horizontal
  preparation, reducing CPU and allocation while preserving exact pixels.
- VP8 pixel-coding validation shares the complete parser while omitting discarded
  reconstruction planes, reducing CPU, allocation and shared-memory peaks.
- Faster lossless WebP decoding through bounded word refills and entropy-group
  reuse, preserving exact pixels, cancellation and admitted storage ownership.
- Lossless WebP decoding reuses complete-overwrite result storage for warm,
  ungoverned calls, preserving exact pixels and caller-budget ownership.
- Compressed WebP alpha decoding avoids an intermediate RGBA image, reducing
  allocations and shared-memory peaks while preserving exact pixels.
- Faster PNG CRC processing through safe GFNI, with portable fallbacks and
  unchanged packets, pixels and resource ownership.
- Faster sparse-CMYK JPEG decoding through a fused baseline DC/AC traversal,
  preserving exact pixels, cancellation and admitted storage ownership.
- Faster JPEG 4:1:1 and 4:1:0 decoding with separable chroma strips and safe SIMD,
  preserving exact pixels and explicitly admitted bounded workspace.
- Faster YCCK 4:2:0 decoding through SIMD over rows, preserving exact pixels,
  portable execution, cancellation and shared admission.
- Baseline JPEG decoding reconstructs component planes directly, and pixel
  validation uses block scratch, reducing admitted workspace with exact pixels.
- Adaptive lossless PNG row prediction for predictable content, including
  full-precision 16-bit gradients after flips and rotations; existing declared
  reference packet controls remain unchanged.
- Faster PNG full-word color packing, row scoring, prediction, reconstruction
  and bounded cancelable inflation, including reuse of certified indexed
  validation bytes, preserving exact bytes, portable fallback and admitted workspace.
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
