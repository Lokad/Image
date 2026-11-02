# WebP tile preparation: two inconclusive stages

Both stages preserve exact packets and pass focused checks, but neither shows a
useful complete-encode gain. Restore the qualified implementation. The stages
and adverse points remain evidence; this is not a permanent family ban.

| Existing direct control | Before µs | Channel planes µs | Coefficient contributions µs | Added B/op, stage1 / stage2 |
| --- | ---: | ---: | ---: | ---: |
| photograph | 92,457.70 | 93,047.17 | 92,729.38 | 784 / 6,232 |
| diagram | 8,725.87 | 8,973.04 | 8,619.48 | 0 / 0 |
| alpha | 63,617.69 | 64,579.39 | 63,762.28 | 0 / 0 |
| authored-bars | 792.54 | 824.64 | 794.48 | 0 / 0 |

Stage1 prepares signed-green/raw-red/raw-blue planes once per small tile in the
existing admitted Search array. Stage2 revises the design to reuse coefficient-
dependent blue contributions in two bounded eight-slot tables. Candidate order,
ties, maps, active-bin order and packets stay exact. The extra preparation,
lookups, loads and storage do not establish useful elapsed-time benefit. The
photograph points are0.64%/0.29% adverse, not causal-regression claims. No further
variant, independent confirmation or favorable resampling follows. All four
admitted peaks are unchanged; physical peak capacity is unmeasured. No encoder
scratch escapes the operation. Current qualified benchmark rows remain unchanged.

Each stage uses one immutable core build perOS and the cached four-control
complete-operation screen. Three direct batches retain source, exact full packets,
ownership, root cleanup and quiet guards. This is diagnostic evidence without
a native bracket or statistical CPU comparison. Unchanged relevant WebP before
evidence is reused; intervening PNG changes only affect untimed fixture setup.

Each stage passes275affected normal/scalar checks on Windows/Linux. One initial
cancellation-test setup is corrected with test-only builds; production remains
immutable and the failed logs stay. Exact qualified source/DLLs and their copies
are restored, with zero production rebuilds. The retained274normal/scalar checks
perOS include six new cases against actual production: signed-floor histograms
and first-nonzero bin order, guarded partial tiles, tiny/large admission refusal,
cleanup and subsequent success. Candidate-only helpers/tests are removed.

Reuse [current production qualification](PNG-PREPARED-PROBES.md): full5,456/live
checks perOS and the SAMEactual0.2.1Release package/three examples bothOS. The
six additions are focused evidence, not a claim that a full5,462suite ran.
No package regeneration or broad repeat. Publication remains postponed.

Next, reuse the existing exact entropy-count audit before another design:
current color-transform and SLog files are byte-identical to its producers.
Dense preparation in future token storage is a distinct ownership/algorithm
hypothesis; old sparse term caches and tile-plane screens do not prove a gain.

[Evidence](webp-tile-preparation-inconclusive-evidence.json) retains source/runtime
identities, both stages, failures, screens, restoration and preserved coverage.
Raw webp-tile-preparation-inconclusive.tar.xz: 2,086,944B /156members, SHA256
148152e216d2d72a42ecea796f66bfb151d6f2b6c75a279135cea9ff3f8bccd9, mirrored/member-verified. Dependencies: png-prepared-probes.tar.xz
(SHA256 a99e642670a8d9462e89e40663898289f1abdb03a6bffbd4bbd995eab90b4263)
and webp-color-preparation-audit.tar.xz
(SHA256 c23ea83428c27670a44473ce1012f536a4bffce92b95e49652e8a8b0db1d8fa7).
