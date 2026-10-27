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
The current guarded short is 1.606× / upper 1.856, 754,344 B/op, exact packets and
pixels. It is not an independent confirmation or a broader effort/settings claim.

`DiagramLossyDistortion8`: same existing diagram, Q75/effort2. Frozen packet:
7,330 B, SHA256 `325a9d5b55b300db2554681735642edcdc8dcb866a88760501d3fe95fac1423b`.
`Vp8FramePlan.PrepareDistortion` / `Vp8DistortionDecision.Select` price original
prediction errors before selected-mode reconstruction. Method4 RDO and method6
trellis do not run this distinct algorithm. Its [current qualified short](WARM-VP8-SIGNED-DOMAIN-GUARD.md)
is 1.589× / upper 1.725, 741,375 B/op and an exact packet. Photograph effort4
independently measures 1.590× / upper 1.610. Other settings receive no broader claim.

## Current qualification and costs

[Shared PNG checksum](WARM-PNG-SHARED-ADLER.md) is the latest
qualified producer: 4,959 Release checks (1,977 internal, 2,035 public, 947 live)
and 499 internal plus 180 public scalar PNG checks per OS. The same actual private 0.2.0 package and all
three usage examples pass both systems. Published 0.1.0 stays unchanged.
Complete packets, decoded/editing pixels, cancellation and explicit shared
admission/ownership remain gates, separately from performance qualification.

The [three-table summary](../docs/benchmark.md) selects valid costs, allocations,
reference boundaries and confirmation status. Current named evidence:

- VP8 [six reconciled anchors](WARM-VP8-SIGNED-DOMAIN-GUARD.md): photograph effort4
  independent 1.590× / upper 1.610, 809,072 B/op, exact 17,688 B. Effort2 / diagram4 /
  trellis6 / opaque16 / alpha shorts are 1.589× / 1.437× / 1.606× / 1.544× / 1.363×.
  All retain exact packets and unchanged allocation. Other affected costs are historical.
- [Indexed VP8L and alpha](WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md): diagram
  independent 1.166× / upper 1.204, 27,419,167 B/op, exact 7,234 B. Asymmetric8 /
  asymmetric16 shorts are 1.584× / 1.600×, both 29,688,696 B/op.
- [Lossless photograph confirmation](WARM-VP8L-PHOTOGRAPH-CONFIRMATION.md):
  independent 1.599× / upper 1.629, 20,410,208 B/op, exact 160,032 B. Same
  qualified immutable producer and cached worker/selector; no production changes.
- VP8L nonseekable async output independently confirms 1.622× / upper 1.641,
  29,689,080 B/op, exact 183,812 B. Allocation falls 37.73%; its 12 actual/filtered
  samples and both brackets remain. Awaited staging/delivery/release is timed;
  callbacks complete immediately. No new input/settings grid or source pixel change.
- [JPEG paired entropy](WARM-JPEG-PAIRED-ENTROPY.md): caller RGB420 independent
  1.543×; 444, RGBA grayscale and caller GRAY8 shorts 1.216× / 1.129× / 1.079×.
- [JPEG shared-root RGBA16 and Lanczos](WARM-CURRENT-JPEG-ROOT-LANCZOS.md): JPEG
  4:2:0 short 1.462× / upper 1.654, 804,637 B/op, 1,704,856 B root peak, exact
  approved packet. Lanczos reduction8/16 shorts 1.478× / 1.438×; enlargement8/16
  1.039× / 1.310×. All managed frames exact; existing native bounds pass. Same
  qualified producer and cached workers/selectors; no production changes.
- [Bicubic cross-axis/streaming](WARM-CURRENT-BICUBIC-CROSS-STREAMING.md): default
  RGBA8 32×16 independently passes at 0.927× / upper 0.934, exact pixels and
  199,372 B/op. Stop this named path while unchanged. Constrained RGBA8 32×16
  and RGBA16 256×519 shorts are gaps at 1.235× / 1.549×, with exact pixels,
  native bounds and local working limits preserved. No production changes.
- [VP8 decoder StreamNative8](WARM-VP8-SHARED-SMALL-ROWS.md): short 1.309×,
  exact pixels. Earlier color-renderer timing is historical after predictor changes.
- [PNG checksum callers](WARM-PNG-SHARED-ADLER.md): RGBA16 SmallestSize independently
  confirms 1.336× / upper 1.360, 3,813,679 B/op and exact 963,202 B. RGBA8
  SmallestSize / Fastest and RGBA16 NoCompression shorts are 1.396× / 1.760× /
  2.754×, all complete canonical packets exact. NoCompression is now the largest
  observed current multiple; unconfirmed shorts remain separately labeled.
- [RGBA16 bilinear reduction](WARM-CURRENT-EDITING-ANCHORS.md): short 1.339×,
  exact managed words and approved native-only bounds.

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
unaffected by that change. Signed coefficient validation subsequently changes
VP8 quantization callers across efforts/depths/alpha and stream/async output.
Six affected anchors replace preceding costs; other affected costs are historical.
VP8L-only encoding, decoders, other codecs and editing/text remain unchanged.
Regression/package proof and unchanged reference oracles carry forward.
PNG encoder checksum callers now reuse the unchanged shared Adler32 kernel.
All PNG encoding policies/colors/depths and stream/async callers are affected;
four named anchors replace older costs. Other affected observations remain
historical. PNG decoders, other codecs and editing/text keep applicable evidence.
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

The bounded candidate-ordering probe is closed without integration: additional
reconstruction pruning did not establish a substantial complete-operation
opportunity. Signed coefficient validation is accepted; its exhaustive signed16
proof, native oracles and six anchors pass. Stop guard variants and closed math
kernel retuning.

Async VP8L 1.622× / upper 1.641 remains the largest selected independent point.
The exact vector color-histogram trial had no meaningful complete-operation
async gain and is rejected; actual qualified sources/cores are restored, with
49 focused normal/scalar checks per OS. Stop these variants. The new JPEG root
and four Lanczos screens resolve missing current costs without production tuning;
shorts are not ranked as independent confirmations.

The three cached bicubic controls resolve the cross-axis and two deep-support
workspace paths after cleanup. The PNG16 measurement and shared checksum round
expose the larger NoCompression gap at 2.754×. Prioritize one stored Deflate
payload-emission primitive: copy contiguous/ring segments into the existing
encoded buffer in bulk, retaining exact row/block selection, bit alignment,
4 KiB flush/write-failure prefixes and cancellation. No new storage or owner.
Prove segmentation, then one existing stored16 short; confirm only a meaningful
result. No new grid, broad audit or automatic profile. Keep closed
PopulationSum/predictor/cost/slab/math kernels closed and stop confirmed passes.

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
