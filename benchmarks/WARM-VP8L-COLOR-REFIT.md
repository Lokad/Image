# Lossless WebP color refit after predictor changes

The [color-code-cost round](WARM-VP8L-COLOR-COST.md) supersedes the photograph
result and closes its declared packet gate. Other five costs remain selected
after confirming their unchanged paths.

The fixed photograph falls **160,918 → 160,612 bytes**, saving **306 B** and
closing **34.5%** of its previous excess. It remains **580 B above** the frozen
**160,032-byte** reference. The actual packet is two bytes smaller than the single
qualified prototype without recipe retuning. Exact pixels, original inputs and
reference settings remain mandatory; this is not a broader compression claim.

Diagram **7,160/7,234 B**, opaque **41,088/121,896 B** and alpha8/16/async
**60,904/183,812 B** remain exact, passing and stopped. All seven lossy gates retain
identical primary packets/quality anchors and unchanged separate ALPH helpers.
Default JPEG and two PNG complete-packet gaps remain open.

Predictor refinements change the residuals used by the color fit. After a winning
predictor refinement, the encoder may repeat the existing signed 8×8 coefficient
recipe once: the same source-derived least-squares seed, fixed 16/8/4/2/1 coordinate
pass and stable ties. The predictor map, selected cache mode and run/prior-row parser
stay fixed; one existing group refit and complete-packet comparison decide adoption.
No additional predictor iteration, native map or settings grid is introduced.

Both alternatives borrow the same predictor/mode-code owners. New coefficient
capacity, auxiliary trees and overlapping old/new coding tables are admitted
before allocation; only losing color/coding owners release. Remaining local/work
guards precede shared attempts. Refusal, cancellation and actual work exhaustion
remain terminal, with cleanup and no fallback. The selected owner survives through
final output. Public operations pass the unchanged default work/memory limits.

Original 513×259 controls retain effort 4, **libwebp exact=1, FFmpeg's encoder backend**,
quality 75/method 4, frozen reference hashes, 16-to-8 normalization, hidden RGB and
complete independent output ownership. Managed and native normal/scalar pixels
are exact. Native allocations are unmeasured.

| Existing operation | Packet B managed/reference | Managed/reference µs | Ratio/upper | Managed allocated B/op | Evidence |
| --- | ---: | ---: | ---: | ---: | --- |
| EncodeOpaque8 | 41,088/121,896 | 481,885.87/69,570.25 | 6.927/8.486 | 53,104,872 | C110; path reconciled |
| DiagramLossless8 | 7,160/7,234 | 21,878.12/9,549.20 | 2.291/3.344 | 1,721,182 | C110; path reconciled |
| PhotographLossless8 | 160,612/160,032 | 548,773.21/82,692.19 | 6.636/7.566 | 59,631,048 | Current short |
| Encode8 | 60,904/183,812 | 445,941.10/121,336.20 | 3.675/5.157 | 55,238,624 | C110; path reconciled |
| Encode16 | 60,904/183,812 | 464,921.62/121,865.49 | 3.815/4.757 | 55,238,624 | C110; path reconciled |
| WebpLossless8AsyncNonSeekable | 60,904/183,812 | 441,626.63/121,213.42 | 3.643/4.686 | 55,170,368 | C110; path reconciled |

Only the photograph cost is refreshed: **three actuals/two passing idle-v2 guards**,
without independent CPU confirmation. Allocation rises from 51,847,984 to 59,631,048 B/op.
Extra CPU/allocation is accepted for compression; observations across rounds are
not a controlled speed-change estimate. The other five timings/allocations retain
their **C110 producer identities and raw** after proving the new analysis is skipped
and complete packets remain identical. Current async output/delivery checks pass.
No repeated timings or unrelated optimization are needed.

Both OSes pass **4,844 tests each**, including 936 live FFmpeg comparisons, plus
**109 internal/117 public scalar checks**. Ten new cases cover color-owner replacement
and rejection, preserving cache mode and exactly one predictor owner, the initial
replacement overlap, local memory/work guards, early/late refusal/cancellation/work
cleanup and recovery, and variable-alpha hidden RGB with independent output.
Existing 8/16-bit packet/output-limit tests pass with the stronger 160,612-byte bound.

One immutable production/internal-test build per OS is reused; public/differential
tests, harness, drivers, native workers and selectors need no build. The same actual
private 0.2.0 package/all three usage examples pass on both OSes; published 0.1.0 stays
immutable. Unrelated valid qualification remains valid. CPU parity is deferred.

[Numeric evidence](vp8l-color-refit-evidence.json) retains all actuals, allocations,
guards, current/reused producer identities, quality and qualification. Mirrored raw:

- `c114-qualified-windows.tar.gz`: `da344e7f4729fc39b25af17e2c78b89500c753babe4c1937e6d99d258c2e2de8`
- `c114-qualified-linux.tar.gz`: `8d29fbd5b3985ec3397e9c23c06eb58b90a28814fdeed688beaed5a458b0e716`
- Reused five costs/harness: `c110-qualified-linux.tar.gz`,
  `1752a1d87449c410e8f927bff5d38b80defc8c3d59f594761077c2f5106ef999`.

Actual private package SHA256: `37f1c48225045c56bbf98838925950cc4a8094a5a5cc5bfef6b6836d421bcc49`.
