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
The current guarded short is 1.635× / upper 1.968, 754,344 B/op, exact packets and
pixels. It is not an independent confirmation or a broader effort/settings claim.

`DiagramLossyDistortion8`: same existing diagram, Q75/effort2. Frozen packet:
7,330 B, SHA256 `325a9d5b55b300db2554681735642edcdc8dcb866a88760501d3fe95fac1423b`.
`Vp8FramePlan.PrepareDistortion` / `Vp8DistortionDecision.Select` price original
prediction errors before selected-mode reconstruction. Method4 RDO and method6
trellis do not run this distinct algorithm. Its [current independent result](WARM-VP8-WORD-ARITHMETIC.md)
is 1.603× / upper 1.614, 741,375 B/op and an exact packet. Photograph effort4
independently measures 1.649× / upper 1.655. Other settings receive no broader claim.

## Current qualification and costs

[Indexed histogram replay](WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md) is the latest
qualified producer: 4,957 Release checks (1,975 internal, 2,035 public, 947 live)
and 636 scalar WebP checks per OS. The same actual private 0.2.0 package and all
three usage examples pass both systems. Published 0.1.0 stays unchanged.
Complete packets, decoded/editing pixels, cancellation and explicit shared
admission/ownership remain gates, separately from performance qualification.

The [three-table summary](../docs/benchmark.md) selects valid costs, allocations,
reference boundaries and confirmation status. Current named evidence:

- VP8 [five unchanged opaque anchors](WARM-VP8-WORD-ARITHMETIC.md): effort2 diagram
  independent 1.603× / upper 1.614; photograph effort4 independent 1.649× / upper
  1.655. Diagram4/trellis6/opaque16 shorts are 1.492× / 1.635× / 1.553×.
  Alpha costs are superseded below; unchanged opaque paths keep valid evidence.
- [Indexed VP8L and alpha](WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md): diagram
  independent 1.166× / upper 1.204, 27,419,167 B/op, exact 7,234 B. Photograph /
  asymmetric8 / asymmetric16 / VP8 alpha shorts are 1.625× / 1.584× / 1.600× /
  1.377×; their allocations are 20,410,208 / 29,688,696 / 29,688,696 / 21,353,192 B/op.
- VP8L nonseekable async output independently confirms 1.622× / upper 1.641,
  29,689,080 B/op, exact 183,812 B. Allocation falls 37.73%; its 12 actual/filtered
  samples and both brackets remain. Awaited staging/delivery/release is timed;
  callbacks complete immediately. No new input/settings grid or source pixel change.
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
changes those same callers. Opaque chroma changes preparation; its main caller
layout also changes, so six8/16-bit/alpha anchors are reconciled. Other affected
observations remain historical.
Method2 pass reuse and methods0/1/2 selected-transform reuse did not affect RDO
or trellis. Single-block inverse reconstruction changes encoder searches/selected
reconstruction across required efforts; its six anchors replace preceding costs.
Other affected observations stay historical. The word interval coder changes
VP8 partition encoding across efforts/depths/alpha and sync/async output; six
existing anchors replace preceding costs. Decision, transform and pixel math
stay exact. Decoder, other codecs and edit/text paths keep valid evidence.
Indexed histogram replay changes VP8L and compressed VP8 alpha callers, including
stream/async and normalization boundaries. Six affected anchors replace their
preceding costs; other affected observations remain historical. Opaque VP8 is
unaffected. Regression/package proof and unchanged reference oracles carry forward.
Frozen inputs/settings/workers, approved native-only adjustments, pixel/packet
oracles and unaffected font comparisons remain reusable. Regression/package
proof does not establish CPU parity.

## Next execution and retained history

Stop source-energy, reconstruction/copy/store, chroma, histogram/density,
matching-plane and other closed predictor/forward/quantizer/residual/trellis/
pricing/emitter sweeps. The source-energy trial was marginal and the qualified
source/core was restored before this accepted reconstruction round.

A bounded current photograph attribution (730 samples) has Luma4 self216,
reconstruction49 and partition writing54 samples. Instrumented times are discarded;
these shares precede the accepted reconstruction change. Its proof/raw trace is
retained with [the reconstruction round](WARM-VP8-SINGLE-BLOCK-RECONSTRUCTION.md).

Word interval publication is accepted and qualified. Fifteen native controls,
1,048,576 private state comparisons and8,192 frozen complete seeded partitions
preserve canonical bytes, delayed carry, limits, cancellation and ownership.
Stop word/fragment variants. The earlier per-symbol fragment screen was marginal
and its qualified producer was restored before this independent wider-state design.

Indexed histogram replay is accepted and qualified. The original full slab/copy
is removed; compact row-cell indexes replay into a non-live working slot, or use
unchanged working originals when no merge happened. Exact native maps/populations,
packets, clipped/copy-crossing geometry, retained results and tight shared
admission/refusal/cancellation cleanup pass. Stop slab/index/replay variants.

The unchanged VP8 photograph RDO point 1.649× is now the largest selected
independent point, ahead of async VP8L 1.622×. Investigate one substantial exact
pruning/candidate-ordering lead in actual Luma4 callers. Every candidate already
computes pure residual/header lower bounds; an equivalent reconstruction order
must preserve original mode-index ties, uniform/cancellable/trellis behavior and
all native decisions. Establish bounded equivalence and a concrete pruning
opportunity before integration. No automatic profile or content/settings grid.
Do not infer a ranking of unconfirmed paths from short screens.

The [preceding checkpoint](https://github.com/lokad/image/blob/0defdc0e7e0e984497c0815f5dc1e759ba6bacf7/benchmarks/WARM-COVERAGE.md)
and individual reports retain accepted/rejected history. Preserve cited archives,
source/native/fixture identities, failed guards and release packages; current
summaries need not repeat the historical narrative.

Reuse immutable builds through bounded rounds. Full Windows/Linux, live,
affected scalar and same-actual-package qualification is batched for accepted
production changes. No repeated proof for unchanged inputs or rejected trials.
Completion requires valid independent quiet-VM conservative upper ≤1× for every
material named required path, with precise tested scope. Stop confirmed passes.
Keep functional, memory, compression, CPU and release readiness distinct. GIF/
animation, startup and multithread concurrency remain outside this warm scope.
