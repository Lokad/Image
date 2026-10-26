# Material warm coverage

Updated 2026-10-26. Full replacement readiness and independent warm upper ≤1×
remain open. Compression correctness takes priority over CPU gains.

The finite checklist is the [fixed inventory](../docs/benchmark-pre-cleanup.md):
257 named controls, minus these four retired boundaries, plus the one distinct
trellis path below: 254 required named cases. The earlier 253 remain required.

- PNG RGBA8 resident / ImageBudget quota.
- JPEG RGBA8 4:2:0 / ImageBudget quota.
- VP8 alpha / caller work budget 32M.
- Optional JPEG caller RGB420 / CompactColor.

Their removed APIs/quotas/experimental encoder are not required operations.
Caller-root retained-result and async controls cover actual IMemoryBudget
admission. Keep existing images, dimensions, depths, filters, quality settings and
approved references; add a case only for a demonstrated distinct omitted required
algorithm or public boundary. No replacement quota benchmark or settings grid.

## One required algorithm omitted from the earlier inventory

`DiagramLossyTrellis8`: the existing 513×259 opaque RGBA8 diagram, quality75,
effort6. Its [frozen complete packet](../tools/Shared/OriginalVp8Controls.cs) is
6,818 B, SHA256 `3c98407656841ff97b185e241e76f750f036c972cfe2a0b704ae2eb35cf0adee`.
[Method6 search](../src/Lokad.Image/Internal/Vp8MacroblockDecision.cs) runs
coefficient-domain dynamic-programming trellis for every luma candidate; the
current method4 warm controls never execute this required algorithm. Functional
packet/ownership proof already exists. Its [current complete-operation short](WARM-VP8-TRELLIS-BASELINE.md)
measures 2.813× / upper2.948, 854,968 B/op, with exact packets and reconstructed
pixels. This is now the largest demonstrated gap. Source, depth and quality stay
frozen: no grid or new image. Other effort/settings receive no broader performance
claim from it.

## Current qualification and demonstrated gaps

[Shared VP8 small-prediction rows](WARM-VP8-SHARED-SMALL-ROWS.md) is the latest
qualified producer: 4,933 Release checks and 612 affected scalar WebP checks per
OS, plus the same actual private 0.2.0 package and all three usage examples.
Complete encoder packets, decoded pixels, editing precision, cancellation and
explicit admission/ownership remain acceptance gates. Published 0.1.0 is unchanged.

The [current three-table summary](../docs/benchmark.md) selects valid costs,
allocations, reference boundaries and confirmation status. Current evidence:

- [Effort6 trellis](WARM-VP8-TRELLIS-BASELINE.md): diagram short 2.813×, exact
  6,818 B and 854,968 B/op. The selected production core and earlier proof are reused.
- [Shared VP8 rows](WARM-VP8-SHARED-SMALL-ROWS.md): photograph independent
  2.524× / upper 2.538; diagram/asymmetric-alpha shorts 2.462× / 1.563×.
  Older affected VP8 costs are historical; these remain current effort4 anchors.
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

The [combined-price screen](WARM-VP8-COMBINED-PRICES-SCREEN.md) shows no useful
gain; its implementation and private table format are removed, actual qualified
binaries/sources restored exactly. Two extreme-probability admission tests remain,
passing normally/scalar on both OS. Existing full/package proof is reused.

The trellis measurement resolves that uncertainty. Next try one bounded reuse of
the existing four-prediction forward transform for uncancelable trellis candidates.
Keep trellis/score traversal ordered and uniform-score reuse disabled for trellis;
no new kernel, cache or workspace. Preserve scalar/live-token behavior. Stop old
combined-price, row-prediction, uniform/cache and publication variants. No profile,
grid, unrelated audit, production rebuild or package repeat for unchanged/rejected work. Screen complete
operations, independently confirm promising changes, and batch affected
cross-platform/scalar/actual-package qualification only for accepted production.

Useful rejected screens and earlier accepted rounds remain linked in the
[prior coverage checkpoint](https://github.com/lokad/image/blob/0bd4a507cd87dcf85bfec4f9ac0a38c49174b86b/benchmarks/WARM-COVERAGE.md)
and individual reports. Retain cited raw archives, failures, source/native/fixture
identities and release packages; current summaries need not repeat that chronology.

Completion requires valid independent quiet-VM conservative upper ≤1× evidence for
every remaining required named path, with exact tested scope. Stop confirmed
passing paths. Keep functional compatibility, cooperative memory, compression,
CPU and release readiness distinct. GIF/animation, startup and multithread
concurrency remain outside this fixed still-image warm scope.
