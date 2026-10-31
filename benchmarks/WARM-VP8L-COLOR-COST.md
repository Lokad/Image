# Lossless WebP color refinement by coding cost

Historical compression experiment: byte-exact encoding has superseded these size
search results. The [current async output cost](CURRENT-ASYNC-VP8L-ENCODE-COST.md)
uses exact 183,812-byte packets; retained measurements below describe their producer.

The fixed photograph falls **160,612 → 159,104 bytes**, saving **1,508 B** and
finishing **928 B below** its frozen **160,032-byte** reference. Actual managed
output exactly matches the fixed prototype with exact normalized RGBA. All
**six declared lossless WebP complete-packet gates now pass and stop**.

Diagram **7,160/7,234 B**, opaque **41,088/121,896 B** and alpha8/16/async
**60,904/183,812 B** pass unchanged. These are four distinct original raster controls
and the existing normalization/output boundaries at fixed settings, with no broader
content/profile claim. All seven lossy gates retain identical primary packets/quality
anchors and unchanged separate ALPH helpers. Default JPEG and two PNG packet gaps
remain open; full CPU parity and replacement readiness are separate open objectives.

One color refinement prices current red/blue residuals against selected group codes,
plus the actual coefficient-map codes. It starts from current own coefficients,
keeps old-value ties, prices absent symbols at fifteen bits, and uses the existing
fixed 16/8/4/2/1 coordinate pass. The predictor map, cache mode and run/prior-row parse
stay fixed; one group refit and complete-packet comparison choose the result.
Integer scoring changes 1,647 coefficient tiles. No extra convergence loop,
native map/table, input/reference/quality change or settings grid is introduced.

The tested color-candidate backend is shared with the entropy fitter. Current
scoring models stay borrowed and charged through their final user. New coefficient
capacity, auxiliary trees and old/new coding overlap are admitted before allocation;
both alternatives borrow one predictor/mode-code owner. Only losing color/coding
storage releases. Default/local/work guards, terminal refusal/cancellation/exhaustion,
cleanup and independent outputs remain intact. Public default limits pass.

Original 513×259 controls retain effort 4, **libwebp exact=1, FFmpeg's encoder backend**,
quality 75/method 4, frozen reference hashes, 16-to-8 normalization, hidden RGB and
complete independent output ownership. Managed and native normal/scalar pixels are
exact. Native allocations are unmeasured.

| Existing operation | Packet B managed/reference | Managed/reference µs | Ratio/upper | Managed allocated B/op | Evidence |
| --- | ---: | ---: | ---: | ---: | --- |
| EncodeOpaque8 | 41,088/121,896 | 481,885.87/69,570.25 | 6.927/8.486 | 53,104,872 | C110; path reconciled |
| DiagramLossless8 | 7,160/7,234 | 21,878.12/9,549.20 | 2.291/3.344 | 1,721,182 | C110; path reconciled |
| PhotographLossless8 | 159,104/160,032 | 612,338.64/83,533.61 | 7.330/8.250 | 67,414,112 | Current short |
| Encode8 | 60,904/183,812 | 445,941.10/121,336.20 | 3.675/5.157 | 55,238,624 | C110; path reconciled |
| Encode16 | 60,904/183,812 | 464,921.62/121,865.49 | 3.815/4.757 | 55,238,624 | C110; path reconciled |
| WebpLossless8AsyncNonSeekable | 60,904/183,812 | 441,626.63/121,213.42 | 3.643/4.686 | 55,170,368 | C110; path reconciled |

Only the photograph cost is refreshed: **three actuals/two passing idle-v2 guards**,
without independent CPU confirmation. Allocation rises from 59,631,048 to 67,414,112 B/op.
Extra CPU/allocation is accepted for compression; observations across rounds are
not a controlled speed-change estimate. Five C110 timings/allocations retain their
producer identities/raw after skipped-analysis/identical-packet reconciliation,
including current async output/delivery checks. No repeated timing or speed-only work.

Both OSes pass **4,856 tests each**, including 936 live FFmpeg comparisons, plus
**121 internal/117 public scalar checks**. Twelve added cases extend replacement
capacity/overlap, borrowed models/predictor ownership, local guards, early/late
terminal failures and recovery, hidden RGB and independent outputs; low effort and
ineligible plans make no work/shared attempt. The shared backend's rejected-candidate
coverage is reused. Existing 8/16-bit output-limit tests assert the reference ceiling.

One immutable production/internal-test build per OS is reused; public/differential
tests, harness, drivers, native workers and selectors need no build. The same actual
private 0.2.0 package/all three usage examples pass on both OSes; published 0.1.0 stays
immutable. After qualification/timing, SPEC's stale progress figures were corrected;
targets, tolerances, APIs and binaries are unchanged, so valid evidence is reused.
The exact measured SPEC is retained; release packaging must include current docs.

[Numeric evidence](vp8l-color-cost-evidence.json) contains the six-case packet ledger,
all actuals/allocations/guards, producer identities, quality and qualification.
Mirrored raw:

- `c119-qualified-windows.tar.gz`: `8627746284c410138047e3889f1386c998ab493c2937beb311c083add62cf883`
- `c119-qualified-linux.tar.gz`: `2e0a169f18f3b592c4e5e669e754b740d56c0bea737d82c001607da08b0c5190`
- Reused five costs/harness: `c110-qualified-linux.tar.gz`,
  `1752a1d87449c410e8f927bff5d38b80defc8c3d59f594761077c2f5106ef999`.

Actual private package SHA256: `185649820501a689b66ecc7ccb32b99e4ad0428db4e8e33511f8fe5f69ac08e5`.
