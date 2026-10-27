# Material warm coverage

Updated 2026-10-27. Full replacement readiness and independent warm upper ≤1×
remain open. Compression correctness takes priority over CPU gains.

## Finite required scope

The [fixed inventory](../docs/benchmark-pre-cleanup.md) has 257 named controls.
Retire these four boundaries and add the two omitted algorithms below: **255
required named cases**. Earlier required functional, font and memory scope remains.

- PNG RGBA8 resident / ImageBudget quota.
- JPEG RGBA8 4:2:0 / ImageBudget quota.
- VP8 alpha / caller work budget 32M.
- Optional JPEG caller RGB420 / CompactColor.

Their removed APIs/quotas/experimental encoder are not required operations.
Caller-root retained-result and async controls cover actual IMemoryBudget
admission. Keep existing images, dimensions, depths, filters, qualities and approved
references. Add a case only for an evidenced distinct omitted required algorithm
or public boundary; no replacement quota benchmark or settings grid.

`DiagramLossyTrellis8`: existing 513×259 opaque RGBA8 diagram, Q75/effort6.
Its [frozen packet](../tools/Shared/OriginalVp8Controls.cs) is 6,818 B, SHA256
`3c98407656841ff97b185e241e76f750f036c972cfe2a0b704ae2eb35cf0adee`.
[Method6](../src/Lokad.Image/Internal/Vp8MacroblockDecision.cs) runs coefficient
dynamic-programming trellis for each luma candidate; method4 never exercises it.
The current guarded short is 1.739× / upper 1.814, 754,328 B/op, exact packets and
pixels. It is not an independent confirmation or a broader effort/settings claim.

`DiagramLossyDistortion8`: same existing diagram, Q75/effort2. Frozen packet:
7,330 B, SHA256 `325a9d5b55b300db2554681735642edcdc8dcb866a88760501d3fe95fac1423b`.
`Vp8FramePlan.PrepareDistortion` / `Vp8DistortionDecision.Select` price original
prediction errors before selected-mode reconstruction. Method4 RDO and method6
trellis do not run this distinct algorithm. Its [current independent result](WARM-VP8-SOURCE-IDENTICAL-PLANES.md)
is 1.947× / upper 1.955, 741,342 B/op and an exact packet: the largest confirmed
gap. Other low-effort settings receive no broader throughput claim.

## Current qualification and costs

[Matching-plane analysis](WARM-VP8-SOURCE-IDENTICAL-PLANES.md) is the latest
qualified producer: 4,947 Release checks (1,965 internal, 2,035 public, 947 live)
and 626 scalar WebP checks per OS. The same actual private 0.2.0 package and all
three usage examples pass both systems. Published 0.1.0 stays unchanged.
Complete packets, decoded/editing pixels, cancellation and explicit shared
admission/ownership remain gates, separately from performance qualification.

The [three-table summary](../docs/benchmark.md) selects valid costs, allocations,
reference boundaries and confirmation status. Current named evidence:

- VP8 [six reconciled anchors](WARM-VP8-SOURCE-IDENTICAL-PLANES.md): effort2
  independent 1.947×; photo/diagram4/trellis6/alpha4/opaque16 shorts are
  1.819× / 1.651× / 1.739× / 1.432× / 1.726×. Packets and B/op are unchanged.
  Diagram4 upper 2.430 remains uncertain. The preceding histogram correction's
  [bounded baseline comparison](WARM-VP8-SOURCE-HISTOGRAM-ZERO-COUNTS.md) remains
  recorded; no repeated baseline or additional density variant.
- [VP8L empty spatial histograms](WARM-VP8L-EMPTY-HISTOGRAMS.md): diagram
  independent 1.362×; photograph/asymmetric shorts 1.605× / 1.566×.
- [VP8L RGBA16](WARM-VP8L-RGBA16-CURRENT.md): short 1.548× / upper 1.685,
  47,674,928 B/op, exact 183,812 B.
- [VP8L nonseekable async output](WARM-ASYNC-WEBP-CURRENT.md): short 1.663× /
  upper 1.889, 47,675,312 B/op, exact 183,812 B. Awaited delivery/staging/release
  are timed; callbacks complete immediately. Production proof is reused.
- [JPEG paired entropy](WARM-JPEG-PAIRED-ENTROPY.md): caller RGB420 independent
  1.543×; 444, RGBA grayscale and caller GRAY8 shorts 1.216× / 1.129× / 1.079×.
- [VP8 decoder StreamNative8](WARM-VP8-SHARED-SMALL-ROWS.md): short 1.309×,
  exact pixels. Earlier color-renderer timing is historical after predictor changes.
- [PNG SmallestSize8](WARM-CURRENT-ENCODER-ANCHORS.md) and
  [RGBA16 bilinear reduction](WARM-CURRENT-EDITING-ANCHORS.md): shorts
  1.417× / 1.339×, exact packets/managed words and approved native-only bounds.

These are gaps, not broad parity claims. Shorts are not independent confirmations.
Complete operations include normalization, equivalent delivery, fresh output
ownership and release. Do not revive matched-dB tuning on byte-exact controls.

## Remaining measurements and invalidation

Resolve uncertain measurements before assuming optimization is required. Every
remaining named inventory boundary stays required, including:

- Resident/stream/async Identify and InspectHeaders: typed source fields,
  structural validation, delivery/EOF and bounded workspace.
- PNG/JPEG/VP8/VP8L decoding and encoder policies: caller gray/RGB/CMYK,
  CMYK/YCCK/411/410, restart/normalization, stream/async and caller-root boundaries.
- Nearest/bilinear/bicubic/Lanczos3: existing reduction/enlargement, mixed/identity
  axes, tiny-alpha/half-rounding and 8/16-bit controls; crop/clone/eight orientations.
- Logical/OpenType text, bidi/Arabic, wrapping/alignment/fitting, prepared rendering,
  recorded supplied faces; fill/copy/composite and retained ownership.

Cleanup changed reader/poll, framing/entropy/MCU and nested callers; pre-cleanup
end-to-end CPU/allocation results do not qualify the cleaned build. Bicubic helper
extraction preserves exact math but changes code layout: pixel proofs carry
forward, old costs require measurement. Retired quota observations are never
required failures or performance passes.

Later invalidation is path-specific. JPEG entropy changes encoding; VP8L
clustering changes lossless/compressed-alpha encoding. Shared small-row prediction
changes VP8 encoder/decoder callers. Source histogram changes affect VP8 source
analysis across efforts0..6 (chroma-only histogram for0/1). Matching-plane analysis
changes those same callers; six current anchors replace their earlier costs,
while other affected observations remain historical.
Method2 pass reuse and methods0/1/2 selected-transform reuse did not affect RDO
or trellis. Unchanged decoder/codec/edit/text paths keep valid evidence.
Frozen inputs/settings/workers, approved native-only adjustments, pixel/packet
oracles, rounding certificates and unaffected font comparisons remain reusable.
Regression/package proof does not establish CPU parity.

## Next execution and retained history

Stop histogram/density and matching-plane variants. The retained
pre-change attribution has 752 samples: 111 input preparation, 82 histogram
updates, 73 small-block search, 66 interval writing and 54 coefficient emission.
Instrumented times are discarded; these shares are not post-change phase costs.
Use retained attribution and actual callers for one substantial unattempted
primitive before smaller gaps, without automatically profiling after each change.

Matching-plane analysis is accepted and qualified: exact zero residuals retain
the known unit forward bias, with every coefficient in magnitude bin0. Add the
complete population directly. Aliases retain sequential feedback and cancellation
is checked each row. No new cache, owner or allocated capacity.

The next untouched lead is opaque 8-bit chroma preprocessing: evaluate exact
parallel gamma lookup/sums/color matrices using safe managed intrinsics. Resolve
runtime capability and algorithm questions first. Preserve the current discrete
gamma/rounding policy, tails, alpha/16-bit and portable paths, plus explicit
workspace admission. Use the existing control; no luma variants, approximation,
new image/settings grid or speed-only relaxation of correctness.

Prior predictor/forward/quantizer/residual/trellis/pricing/emitter/flatness sweeps,
pass reuse and selected-transform variants remain closed. The luma SIMD screen
was marginal and its actual qualified source/binaries were restored. Useful
accepted/rejected history is retained in the
[preceding coverage checkpoint](https://github.com/lokad/image/blob/af7aed7747fcfff2bc5fce4c94e6bcb38feb681c/benchmarks/WARM-COVERAGE.md)
and individual reports. Retain cited raw archives, failed guards, source/native/
fixture identities and release packages; current summaries need not repeat them.

Reuse immutable builds through bounded rounds. Full Windows/Linux, live,
affected scalar and same-actual-package qualification is batched for accepted
production changes. No repeated proof for unchanged inputs or rejected trials.
Completion requires valid independent quiet-VM conservative upper ≤1× for every
material named required path, with precise tested scope. Stop confirmed passes.
Keep functional, memory, compression, CPU and release readiness distinct. GIF/
animation, startup and multithread concurrency remain outside this warm scope.
