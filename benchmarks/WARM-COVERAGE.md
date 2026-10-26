# Material warm coverage

Updated 2026-10-26. Full replacement readiness and independent warm upper ≤1×
remain open. Compression correctness takes priority over CPU gains.

The finite checklist is the [fixed inventory](../docs/benchmark-pre-cleanup.md):
257 named controls, minus these four retired boundaries, leaving 253 required
cases (236 previously numeric, 17 unmeasured):

- PNG RGBA8 resident / ImageBudget quota.
- JPEG RGBA8 4:2:0 / ImageBudget quota.
- VP8 alpha / caller work budget 32M.
- Optional JPEG caller RGB420 / CompactColor.

Their removed APIs/quotas/experimental encoder are not required operations.
Caller-root retained-result and async controls cover actual IMemoryBudget
admission. Keep existing images, dimensions, depths, filters, quality settings and
approved references; add a case only for a demonstrated distinct omitted required
algorithm or public boundary. No replacement quota benchmark or settings grid.

## Current qualification and demonstrated gaps

[Adjacent VP8 forward batching](WARM-VP8-ADJACENT-FORWARD.md) is the latest
qualified producer: 4,932 Release checks and 611 affected scalar WebP checks per
OS, plus the same actual private 0.2.0 package and all three usage examples.
Complete encoder packets, decoded pixels, editing precision, cancellation and
explicit admission/ownership remain acceptance gates. Published 0.1.0 is unchanged.

The [current three-table summary](../docs/benchmark.md) selects valid costs,
allocations, reference boundaries and confirmation status. Current evidence:

- [VP8 forward batching](WARM-VP8-ADJACENT-FORWARD.md): photograph independent
  2.879× / upper 2.900; diagram/asymmetric-alpha shorts 2.733× / 1.626×.
  Older affected VP8 costs are historical. These largest demonstrated gaps lead.
- [VP8L empty spatial histograms](WARM-VP8L-EMPTY-HISTOGRAMS.md): diagram
  independent 1.362×; photograph/asymmetric VP8L shorts 1.605× / 1.566×.
- [JPEG paired entropy](WARM-JPEG-PAIRED-ENTROPY.md): caller RGB420 independent
  1.543×; 444, RGBA grayscale and caller GRAY8 shorts 1.216× / 1.129× / 1.079×.
- [VP8 decoder color batching](WARM-VP8-VECTOR-COLOR.md): StreamNative8
  independent 1.301× with exact pixels.
- [PNG SmallestSize8](WARM-CURRENT-ENCODER-ANCHORS.md) and
  [RGBA16 bilinear reduction](WARM-CURRENT-EDITING-ANCHORS.md): current shorts
  1.417× / 1.339×; exact packets/managed words and approved native-only bounds.

These are gaps, not broad parity claims. Shorts are not independent confirmations.
Complete operations include normalization, equivalent input delivery, fresh output
ownership and release. Do not revive matched-dB tuning on byte-exact controls.

## Remaining measurements and evidence validity

Resolve uncertainty before assuming optimization is needed. Every remaining named
inventory boundary stays required, including:

- Resident/stream/async Identify and InspectHeaders: matching typed source fields,
  structural validation, delivery/EOF and bounded workspace.
- PNG/JPEG/VP8/VP8L decoding and encoder policies: current caller gray/RGB/CMYK,
  CMYK/YCCK/411/410, restart/normalization, stream/async and caller-root boundaries.
- Nearest/bilinear/bicubic/Lanczos3: existing reduction/enlargement, mixed/identity
  axes, tiny-alpha/half-rounding and 8/16-bit controls; crop/clone/eight orientations.
- Logical/OpenType text, bidi/Arabic, wrapping/alignment/fitting, prepared rendering
  and recorded supplied faces; fill/copy/composite and retained ownership.

Cleanup changed work-poll/reader, framing/entropy/MCU and nested callers; prior
end-to-end CPU/allocation confirmations do not qualify the cleaned build. Bicubic
helper extraction preserves token-identical math but changes code layout: exact
pixel proofs carry forward, while old costs need measurement. Retired quota/API
observations never become required failures or performance passes.

Later invalidation is path-specific: paired entropy changes JPEG encoding;
clustering changes VP8L/compressed-alpha encoding; prediction/quantization/forward
batching changes VP8 encoding; decoder color batching changes VP8 decode rendering.
Current anchors above reconcile only their named boundaries. Unchanged unrelated
paths keep valid evidence. Frozen inputs/settings/native workers, approved baseline
adjustments, packet/pixel oracles, rounding certificates and unaffected font
comparisons remain reusable. Regression/package proof is not CPU parity proof.

## Next execution and retained history

Stop completed primitive variants. The latest bounded photograph profile reuses
the qualified core and prepared longer selector: 706 samples, 101 exclusive in
SmallBlock. Source inspection identifies per-pixel encoder dispatch where the
decoder already has exact whole-row formulas. Sharing those formulas is the next
bounded primitive, preserving mapping/aliasing/rounding/errors. This reopens no
uniform/cache variant. Instrumented timings are discarded; the failed first
short-target attempt is retained without an attribution claim. Screen one equivalent
complete operation, independently confirm promising changes, and batch affected
cross-platform/scalar/actual-package qualification when accepted. Reconcile decoder
caller costs if shared code changes them. No unrelated audit, rebuild or package
proof for unchanged/rejected trials.

Useful rejected screens and earlier accepted rounds remain linked in the
[prior coverage checkpoint](https://github.com/lokad/image/blob/0bd4a507cd87dcf85bfec4f9ac0a38c49174b86b/benchmarks/WARM-COVERAGE.md)
and individual reports. Retain cited raw archives, failures, source/native/fixture
identities and release packages; current summaries need not repeat that chronology.

Completion requires valid independent quiet-VM conservative upper ≤1× evidence for
every remaining required named path, with exact tested scope. Stop confirmed
passing paths. Keep functional compatibility, cooperative memory, compression,
CPU and release readiness distinct. GIF/animation, startup and multithread
concurrency remain outside this fixed still-image warm scope.
