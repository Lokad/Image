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

[Shared VP8 small-prediction rows](WARM-VP8-SHARED-SMALL-ROWS.md) is the latest
qualified producer: 4,933 Release checks and 612 affected scalar WebP checks per
OS, plus the same actual private 0.2.0 package and all three usage examples.
Complete encoder packets, decoded pixels, editing precision, cancellation and
explicit admission/ownership remain acceptance gates. Published 0.1.0 is unchanged.

The [current three-table summary](../docs/benchmark.md) selects valid costs,
allocations, reference boundaries and confirmation status. Current evidence:

- [Shared VP8 rows](WARM-VP8-SHARED-SMALL-ROWS.md): photograph independent
  2.524× / upper 2.538; diagram/asymmetric-alpha shorts 2.462× / 1.563×.
  Older affected VP8 costs are historical. These largest demonstrated gaps lead.
- [VP8L empty spatial histograms](WARM-VP8L-EMPTY-HISTOGRAMS.md): diagram
  independent 1.362×; photograph/asymmetric VP8L shorts 1.605× / 1.566×.
- [JPEG paired entropy](WARM-JPEG-PAIRED-ENTROPY.md): caller RGB420 independent
  1.543×; 444, RGBA grayscale and caller GRAY8 shorts 1.216× / 1.129× / 1.079×.
- [VP8 decoder calling-cost reconciliation](WARM-VP8-SHARED-SMALL-ROWS.md):
  StreamNative8 short 1.309× with exact pixels. Color arithmetic proof is unchanged;
  its earlier end-to-end timing is historical after shared-predictor calling changes.
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
Shared small-row prediction changes both VP8 encoder and decoder calling code.
Current anchors above reconcile only their named boundaries. Unchanged unrelated
paths keep valid evidence. Frozen inputs/settings/native workers, approved baseline
adjustments, packet/pixel oracles, rounding certificates and unaffected font
comparisons remain reusable. Regression/package proof is not CPU parity proof.

## Next execution and retained history

Stop completed primitive variants, including shared prediction rows. The earlier
bounded photograph sample recorded 76 exclusive residual-vector samples and
31 exclusive residual-wrapper samples among 706 workload samples. Their code is
unchanged; this is a code lead, not current phase attribution or a speed forecast.
Inspect address/table lookup work before one bounded residual-pricing candidate,
preserving exact costs/errors and safe managed fallback. No fresh profile or new
workload/settings grid without concrete uncertainty. Screen one complete photo
operation, independently confirm promising changes, and batch affected
cross-platform/scalar/actual-package qualification when accepted. Reuse unaffected
proof; do not rerun unrelated audits/builds/package checks for rejected trials.

Useful rejected screens and earlier accepted rounds remain linked in the
[prior coverage checkpoint](https://github.com/lokad/image/blob/0bd4a507cd87dcf85bfec4f9ac0a38c49174b86b/benchmarks/WARM-COVERAGE.md)
and individual reports. Retain cited raw archives, failures, source/native/fixture
identities and release packages; current summaries need not repeat that chronology.

Completion requires valid independent quiet-VM conservative upper ≤1× evidence for
every remaining required named path, with exact tested scope. Stop confirmed
passing paths. Keep functional compatibility, cooperative memory, compression,
CPU and release readiness distinct. GIF/animation, startup and multithread
concurrency remain outside this fixed still-image warm scope.
