# Deferred VP8L population windows

The end-to-end baseline below is superseded by
[exact color-trial reuse](WARM-VP8L-COLOR-TRIAL-REUSE.md); its proofs and verdict remain.

The existing RGBA8 control has an independent qualified baseline of **193,847 µs,
1.590× reference time / upper1.609**, with 29,688,696 B/op and exact 183,812-byte
packets. This uses the unchanged accepted producer and all twelve actual samples.

A SIMD population-cost rewrite replaces per-run work with exact four/five-sample
window statistics and small-count entropy products. Its short screen suggested
22% improvement (159,299 versus 204,700 µs), but independent confirmation did not
reproduce it: **192,988 versus 193,847 µs** on the same confirmation host. The
0.44% difference and overlapping confidence bounds establish no material gain.
The short/confirmation discrepancy's cause is unproven. Defer and remove the trial.

Input/settings, complete-operation boundaries, native workers, packet/pixel gates,
allocations and quiet guards agree. Both runtimes support Vector512. Nine focused
tests pass normal/scalar on each OS, including 10,080 existing frozen paired costs
and 18,424 new vector/tail boundary comparisons. No full-suite repeat, package
regeneration or companion timing is warranted for the removed candidate.

Two bounded follow-up diagnostics also rule out extra maintained state: empty
64-bin regions cover only 12.5% of scanned bins; repeated unchanged histogram-pair
results could avoid only 1.25%. Neither justifies a production candidate now.
All diagnostics use the same frozen control and preserve its complete packet.

The qualified source, tests and immutable producer are restored on both systems.
[The evidence](vp8l-window-populations-deferred-evidence.json) retains every actual,
producer/worker identity, proof and mirrored member-verified raw archive. Keep
population-window, occupancy and pair-cache variants closed without materially
different evidence. Full warm CPU parity remains open.
