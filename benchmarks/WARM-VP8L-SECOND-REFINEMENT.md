# Lossless WebP bounded second predictor refinement

The fixed photograph falls **161,410 → 160,918 bytes**, saving **492 B** and
closing **35.7%** of its previous excess. It remains **886 B above** the frozen
**160,032-byte** reference. Actual managed output exactly matches the single
qualified prototype; no recipe retuning, source/reference/settings change or
broader compression claim is made.

Diagram **7,160/7,234 B**, opaque **41,088/121,896 B** and alpha8/16/async
**60,904/183,812 B** remain exact, passing and stopped. All seven lossy gates retain
identical primary packets/quality anchors and unchanged separate ALPH helpers.
Default JPEG and two PNG complete-packet gaps remain open.

A winning first code-cost predictor refinement changes the coding probabilities.
The encoder may reprice once against those new tables, for at most **two passes**;
an unchanged or rejected first map stops immediately. Each pass retains stable
old-mode ties, missing-code cost 15, fourteen format modes per8×8tile, fixed own color
coefficients, the uncached run/prior-row parser, one group refit and complete-packet
selection. The second map changes 219 tiles; this is not a convergence loop.

The same admitted color owner survives both predictor/table replacements. Each
pass checks remaining local memory/work before shared attempts; refusal,
cancellation and actual exhaustion remain terminal. New capacity is admitted
before allocation, old/new owners overlap and unused capacity releases promptly.
The immutable-method screen fits default `MaxWork = 2_000_000_000`; full public operations with
that unchanged default also pass.

Original 513×259 controls retain effort4, **libwebp exact=1, FFmpeg's encoder backend**,
quality 75/method 4, frozen reference hashes,16-to-8 normalization, hidden RGB and
complete independent output ownership. Managed and native normal/scalar pixels
are exact. Native allocations are unmeasured.

| Existing operation | Packet B managed/reference | Managed/reference µs | Ratio/upper | Managed allocated B/op | Evidence |
| --- | ---: | ---: | ---: | ---: | --- |
| EncodeOpaque8 | 41,088/121,896 | 481,885.87/69,570.25 | 6.927/8.486 | 53,104,872 | C110; path reconciled |
| DiagramLossless8 | 7,160/7,234 | 21,878.12/9,549.20 | 2.291/3.344 | 1,721,182 | C110; path reconciled |
| PhotographLossless8 | 160,918/160,032 | 474,740.90/82,980.13 | 5.721/7.099 | 51,847,984 | Current short |
| Encode8 | 60,904/183,812 | 445,941.10/121,336.20 | 3.675/5.157 | 55,238,624 | C110; path reconciled |
| Encode16 | 60,904/183,812 | 464,921.62/121,865.49 | 3.815/4.757 | 55,238,624 | C110; path reconciled |
| WebpLossless8AsyncNonSeekable | 60,904/183,812 | 441,626.63/121,213.42 | 3.643/4.686 | 55,170,368 | C110; path reconciled |

Only the photograph cost is refreshed: **three actuals/two passing idle-v2 guards**,
without independent CPU confirmation. Allocation rises from 44,090,320 to 51,847,984 B/op.
Extra CPU/allocation is accepted for compression; observations across rounds are
not a controlled speed-change estimate. The other five timings/allocations retain
their **C110 producer identities and raw**: their code-cost selection returns before
work, the new second pass is skipped, and complete packets remain identical. Current
async output and delivery checks also pass. No repeated timings for unchanged paths.
A setup-only launcher-resolution failure produced no measurements; its log is
retained alongside the uniquely named retry. No production/harness rebuild resulted.

Both OSes pass **4,834 tests each**, including 936 live FFmpeg comparisons, plus
**99 internal/117 public scalar checks**. Six added cases start from the first refinement’s result to cover second ownership transfer, local memory/work guards, refusal,
cancellation/work failure and successful recovery. Existing 8/16-bit exact output
limits and independent lifetime checks pass with the stronger packet bound.

One immutable production/internal-test build per OS is reused; public/differential
tests, harness, drivers, native workers and selectors need no build. The same actual
private 0.2.0 package/all three usage examples pass on both OSes; published 0.1.0 stays
immutable. Unrelated valid qualification remains valid. CPU parity is deferred.

[Numeric evidence](vp8l-second-refinement-evidence.json) retains all actuals,
allocations, guards, current/reused producer identities, quality and qualification.
Mirrored raw:

- `c112-qualified-windows.tar.gz`: `63ea35aab9440b29e224b1d77e30bd5face21a0e45d37d51ce9b12717800c7bb`
- `c112-qualified-linux.tar.gz`: `59e747cdb815aef56abf01ef06db1e35f1ffaa2b6fc3bd8ecfbb4c097985769a`
- Reused five costs/harness: `c110-qualified-linux.tar.gz`,
  `1752a1d87449c410e8f927bff5d38b80defc8c3d59f594761077c2f5106ef999`.

Actual private package SHA256: `e5bebe83ebe8db18c3663fd833d485255a1713abb08ad538b9fe4a099efc7733`.
