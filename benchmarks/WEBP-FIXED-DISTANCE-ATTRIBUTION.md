# Current transparent WebP matching attribution

One two-second capture on the immutable QDP300 producer reuses the existing
complete 513×259 transparent Encode8 + ALPH harness, selector and native worker.
Exact 98,590-byte packet, source/output ownership, producer and quiet guards pass.
CPU0 executes, CPU1 samples. No build or new control. Profiled elapsed is discarded;
samples and instruction skid are attribution, not phase times or available gains.

| Exclusive location | Samples, out of 1,952 |
| --- | ---: |
| VP8L matcher | 299 |
| VP8 coefficient coding | 115 |
| VP8 intra4 decisions | 93 |
| Cache history | 87 |
| Green predictor histogram | 86 |
| Prepared population sums | 67 |
| Token statistics | 60 |

The matcher remains a material lead after full-block quantization, residual
preparation and delayed luma4 publication. Its 2,362-byte JIT body is unchanged.
Earlier [matching](WEBP-MATCHING-PREPARATION-INCONCLUSIVE.md),
[compact representation](WEBP-COMPACT-MATCHING-INCONCLUSIVE.md) and
[scalar-window](WEBP-SCALAR-WINDOW-PREPARATION.md) trials retain adverse/inconclusive
evidence; this capture does not authorize unchanged replay.

A distinct proposal would replace repeated previous-pixel/previous-row prefix
scans with two reverse LCP recurrences. A count-only projection of current owned
source runs one warmup and one observed complete frozen encode, preserving the
actual algorithm and exact packet. Its two full-size matcher passes show:

| Full-size alpha matching | First filter | Second filter | Total |
| --- | ---: | ---: | ---: |
| Outer queries | 84,215 | 48,463 | 132,678 |
| Visited reverse positions | 132,865 | 132,865 | 265,730 |
| Fixed-distance probe comparisons | 168,327 | 96,817 | 265,144 |
| Fixed-distance prefix comparisons | 56,424 | 75,520 | 131,944 |
| Chain prefix comparisons | 643,710 | 1,073,325 | 1,717,035 |

Existing fixed probes plus prefix scans total **397,088 comparisons**. Updating
both straightforward recurrences at every visited position would use **530,436**,
including positions skipped by backward extension. This does not justify that
implementation; no production prototype follows. Counts are not CPU costs or a
regression prediction. A more substantially fused design would need its own
evidence; the family is not closed. Chain queries dominate prefix comparisons.

One isolated diagnostic build, zero production/harness/native/selector/package
builds. All source pixels and borrowed roots survive; scratch charges return to
zero. Diagnostic telemetry is excluded from production accounting and supports
no timing/allocation/peak claim. Current sources, selected DLLs and actual package
remain the qualified [luma4 checkpoint](VP8-LUMA4-PREPARATION.md); its 5,639
full/live checks per OS, affected fallback and same-package/all-three-example
proof are reused. Every current comparison row stays unchanged. The full
282-path objective remains open and publication is postponed.

[Evidence](webp-fixed-distance-attribution-evidence.json) pins the profile,
independent count-only producer, all observations, ownership and raw manifests.
Reproduce with retained `waa302-profile*.py` and `wlc303-*.py` on the quiet
single-CPU VM. Current full owned source and instrumented replacement/driver,
compiler response, assembly, original recipe/golden packets, trace/maps/JIT and
guards are retained. The qualified dependency supplies unchanged runtime tools.

Raw `webp-fixed-distance-attribution.tar.xz`: 1,210,380 B / 276 members,
SHA256 `d5d6b5a1e978d3e726cd8c529796ae517ad5e58d23287ab86dbf525db24b3796`, mirrored/member-verified locally and on the VM.

## Prepared-query follow-up

A count-only projection on the unchanged matching path confirms 132,678 full-size
outer queries and 1,717,035 chain-prefix comparisons. About 65.5% of chain scans
match two or three pixels. Query reuse motivates one bounded implementation:
prepare four full uint target pixels once, borrow that value across candidates,
compare the first vector safely, then retain the scalar long/tail loop. Search
order, pruning, ties, complete indexes/packets and all ownership remain exact.

The unchanged complete four-control screen has no favorable point: photograph
88,068.03→89,679.02 µs, diagram 7,800.28→7,950.46 µs, transparent VP8/ALPH
65,599.59→67,925.57 µs and authored bars 834.81→842.56 µs. Each subject has three
direct batches with quiet guards, exact packets/source/root cleanup and identical
allocations/admitted peaks. These points establish no statistical regression;
they justify no independent confirmation. Counts imply no CPU savings.

Both OSes pass 394 internal/195 public checks per normal/scalar mode, including
actual primitive overlap/tail/full-bit/probe/cancellation checks and frozen
complete indexes/packets. Original source/main/internal modules restore exactly
without rebuilding; 15 restored primitive checks pass per mode/OS. Reuse WSP312's
5,656 full/live checks per OS and SAME actual package/all-three-example proof.
Current costs stay unchanged. One vector-overload compile mistake per OS and an
observation filename error before execution are retained; no completed timing
repeats, broader suite or package rebuild follows. This implementation is
inconclusive, not a permanent exclusion of prepared matching.

The existing [evidence](webp-fixed-distance-attribution-evidence.json) now includes
this follow-up. Raw `webp-prepared-query-inconclusive.tar.xz`: 3,022,544 B /
358 members, SHA256
`eded950b6ef422da86cd1dbc4643ee9c5c3181ae65340392f3a5b94dc012342b`,
mirrored/member-verified. Reproduce with retained `wmp313-*.py`; the pinned WSP312
dependency supplies unchanged tooling and qualification. Publication stays
postponed and the full 282-path objective remains open.
