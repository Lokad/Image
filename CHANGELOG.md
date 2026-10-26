# Changelog

## 0.2.0 — Unreleased

- Breaking unpublished API cleanup: removed `MaxWork`, cumulative work quotas,
  `ImageLimitKind.Work`, `ImageBudget`/`BudgetedImage` and their codec wrappers.
  Use direct cooperative cancellation and the existing `IMemoryBudget` root for
  explicit storage admission/ownership. Local storage/container bounds remain.
- Removed unused VP8 encoder prototypes and unreachable orientation mapping;
  consolidated identical bicubic arithmetic without changing pixels.

- Reference-compatible managed JPEG encoding for grayscale/4:4:4/4:2:2/4:2:0,
  including restarts, direct caller samples and async output. Removed superseded
  experimental encoders and switches. Ten frozen controls match complete reference
  packets, allowing only the documented color attribution replacement.
  Color encoding reuses bounded MCU normalization/chroma storage and plans,
  with exact SSE2/AVX2 forward transforms/quantization and scalar fallbacks.
  RGBA16 subsampling normalizes bounded rows with exact single rounding.
  Adaptive4:2:0/4:2:2 replays admitted, bounded quantized coefficients.
- Fixed-point JPEG decoding with exact samples on30 frozen grayscale/4:4:4/4:2:2/4:2:0
  controls, shared caller RGB conversion and opt-in orientation. Removed superseded
  inverse variants. CMYK/YCCK now share reference-precision RGB/grayscale
  conversion; 4:1:1/4:1:0 retains fractional chroma through final color rounding.
  Removed superseded floating renderers; named native sample controls are exact.
  Direct-RGB caller grayscale shares integer luma precision after component rounding.
- Bounded managed PNG stored/fast/optimal/smallest encoding, with complete raw
  packet equality on14 frozen8/16-bit controls and shared admission throughout.
- Managed VP8L prediction, palette, references/cache and entropy groups reproduce
  19 selected and two additional forced frozen libwebp packets.
  Entropy scoring reuses exact Q23 mathematical constants without runtime allocation.
  Bounded Int64 refinement retains full-range Int128 arithmetic when needed.
  Safe SIMD masks skip zero predictor bins with exact costs and scalar fallback.
  Matcher construction reuses result capacity for backward chains, removing a
  full-frame temporary array while preserving exact indices and packet bytes.
  Spatial clustering skips empty token-start tiles while retaining exact maps,
  merged populations, packet bytes and cooperative cancellation.
- Integer VP8 color normalization matches all164 existing frozen RGBA controls;
  removed the superseded floating/SIMD renderer and bounded RGB assertions.
- Managed VP8 encoding matches20 declared complete reference packets, with all
  effort policies, exact normalized alpha and direct async partition delivery.
  Forward4x4 transforms use exact integer butterfly projections.
  Exact rate/header bounds skip reconstruction of losing luma candidates.
  Uniform-boundary luma blocks share identical prediction work while preserving
  each mode's header, flat-level surcharge and exact tie decisions.
  Removed superseded encoder experiments; full CPU parity remains open.
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
