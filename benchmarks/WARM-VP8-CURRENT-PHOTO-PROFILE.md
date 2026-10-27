# Current VP8 photograph attribution

This retained trace describes the two-state-trellis producer. Later accepted
prediction, source-analysis, histogram, rate-refresh and residual changes
supersede its cost attribution; it is historical, not a current hotspot ranking.

One bounded trace on the qualified two-state-trellis build reuses the frozen
effort-4 photograph, complete operation, harness, native worker and sampler.
Both idle guards pass; packets, reconstruction, borrowed source and independent
outputs remain exact. All instrumented timings are discarded.

The 706 managed workload samples include 110 exclusive samples in `Luma4`, 97 in
`ResidualVectors`, 43 in partition writing, 42 each in large-block prediction and
source histograms, 39 in reconstruction and 38 in small prediction. These are
sampling leads, with inlining and runtime coverage limitations, not phase timings.
The earlier profile predates shared small-row prediction and is historical.
[Raw evidence](warm-vp8-current-photo-profile-evidence.json) retains the trace,
full attribution, guarded packet validation and generated residual assembly.

A direct single-coefficient pricing shortcut was screened on this same photograph:
24,434.47 µs, 2.518× reference / upper 2.781×, 909,696 B/op and exact 17,688 B.
Its 0.825% elapsed difference from the preceding short is within uncertainty;
the candidate is rejected. Sources and selected binaries were restored byte
exactly. Both systems pass 21 focused checks normally and with intrinsics disabled.
Existing full/package proof is reused; no timing confirmation or package repeat.
[Screen evidence](warm-vp8-sparse-residual-screen-evidence.json) retains candidate
binaries, raw samples and restoration checks. Stop residual-pricing variants.

Generated `Luma4` code already inlines per-mode score arithmetic; its remaining
`Price`/`Add` calls occur after block selection. This does not justify a score-copy
trial. The identified large-block prediction lead subsequently became the accepted
[row dispatch change](WARM-VP8-LARGE-BLOCK-ROWS.md), preserving border snapshots,
missing-edge rules, true-motion clamping, tails and the portable fallback. That
round is closed; this old trace does not justify another prediction variant.
