# Exact shared-history predictor pricing

The existing RGBA8 control independently uses **7.4% less elapsed**:161.623ms
versus a fresh174.600ms baseline. Its **1.332× / upper1.360** CPU gap remains.
Packets/pixels stay exact at183812B; allocation rises by8224B/op.

| Quiet one-CPU run | Lokad µs | libwebp µs | Ratio / upper | Managed B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Baseline RGBA8, short | 173,688.04 | 122,541.73 | 1.417 / 1.741 | 29,688,704 | 183,812 |
| Baseline RGBA8, confirm | 174,600.18 | 121,606.73 | 1.436 / 1.459 | 29,688,704 | 183,812 |
| Prepared RGBA8, short | 163,514.55 | 120,763.66 | 1.354 / 1.797 | 29,696,928 | 183,812 |
| Prepared RGBA8, confirm | 161,623.34 | 121,357.51 | 1.332 / 1.360 | 29,696,928 | 183,812 |
| Encode16, short | 161,904.66 | 120,988.24 | 1.338 / 1.923 | 29,696,928 | 183,812 |
| WebpLossless8AsyncNonSeekable, short | 162,750.37 | 120,441.78 | 1.351 / 1.431 | 29,697,312 | 183,812 |
| PhotographLossless8, short | 109,137.75 | 82,564.36 | 1.322 / 1.411 | 20,417,624 | 160,032 |

Reference is explicitly libwebp exact=1, FFmpeg's encoder backend. Existing513×259
inputs/method4/quality75 remain frozen; equal numerical quality is not a matching
criterion. Complete operations include normalization, fresh independent packet
ownership and actual async staging/delivery/release where applicable. Both main
confirmations retain all12actual/filtered samples. All native brackets/idle-v2
guards pass. This compares separate sequential means, without paired-sample or
general-content claims. The three companions are current short screens, not
independent speedup/parity confirmations. Native allocation is unmeasured.

A bounded count-only helper finds that85.28% of active union bins in30030prices
are prior-only. Every14mode group shares one unchanged history and plane
population, with zero mismatches. Prepare exact prior SLog products and the
population baseline once, then visit only nonzero current bins for each mode.
Existing color-cost preparation is reused; logarithms, Q23 rounding, bias, strict
ties, maps, residuals and packets stay exact. Safe512/256-bit masks preserve
ascending-bin arithmetic with a portable scalar fallback.

One8KiB typed array belongs to the existing analysis owner, admitted before
allocation and reused only after child visits finish. It is released before row
application. Allocation rises by8224B/op on all four controls, including array/
owner overhead; the existing512B metadata allowance covers minor objects. No
new scope/cache/allowance or full-frame copy. Green-only pricing allocates none.
Retained results, refusal/cancellation cleanup and actual async lifetimes remain.

Both systems pass5052Release checks each, including947live comparisons,15internal/
27public scalar checks, the SAME actual private0.2.1 package and three examples.
New proofs cover14different modes against one prepared history at six populations,
borrowed-input/product guards, cancellation before access and64KiB admission
refusal before shared growth, followed by successful release. Existing independent
dense-cost/native-map/residual/packet and ownership controls remain exact.

One immutable successful core build perOS was reused; harness/native/selectors
were cached. The completed8bit baseline was retained after correcting an inherited
16bit receipt assertion. Superseded companion assembly detection stopped before
timing; the verified current harness was reused. A zero-match scalar test filter
was corrected without repeating valid full/internal checks. All failures and raw
observations are retained. Published releases are unchanged.

[Numeric evidence](warm-vp8l-prepared-prediction-cost-evidence.json) retains producer/
package identities, samples, guards and the mirrored/member-verified raw master.
Generic predictor callers change, including method4/5/6, depth normalization and
stream/async boundaries. Four displayed costs are refreshed; other affected named
cases need current costs before a pass claim. Diagram4 skips prediction, and
compressed VP8 alpha uses the unchanged green-only path. Decoding, other VP8
encoding, JPEG/PNG, editing/text and applicable unchanged proofs remain valid.
Stop this primitive; the full267-path warm single-CPU ≤1× objective stays open.
