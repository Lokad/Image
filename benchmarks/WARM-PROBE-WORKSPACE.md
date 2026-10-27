# Dimension-probe workspace

The [completed-read round](WARM-PROBE-COMPLETED-READS.md) supersedes CPU costs
below; the allocation/admission reduction and its qualification remain valid.

Dimension-only probing now uses 256 bytes of scratch instead of 8 KiB in the
existing header reader. Its admission is 512 bytes instead of 8,448. Other fact
selections and complete inspection retain the larger buffer. The format parser,
prefix/position semantics, limits, cooperative cancellation and async lifetimes
are unchanged; refusal remains terminal before allocation or input reads.

All four existing PNG/JPEG/WebP controls allocate **744 B/op instead of 8,680**,
a 91.4% reduction. Current JPEG/native confirmations measure 3.714× sync and
4.098× async; PNG/WebP companion screens measure 1.783×/1.812×. These remain gaps.
The [summary](../docs/benchmark.md) includes confidence bounds and current costs;
[evidence](warm-probe-workspace-evidence.json) retains every sample. Prior CPU
baselines are short, noisy screens; these comparisons do not establish a precise
independent before/after speedup. The allocation reduction is consistent across
all six new runs. Do not further tune buffer size from these observations.

The same original packets, native adapter and cached benchmark/selectors are
reused from [read-boundary qualification](WARM-IMAGE-READ.md), without rebuilding
tooling or expanding the matrix. All twelve preparation controls, exact requested
descriptors, source/independent-result ownership, truncated-prefix and coding-scope
gates pass. Both idle/process guards pass; all three/twelve actuals are retained.

Both platforms pass 5,037 Release checks each, including 947 live comparisons,
plus 95 focused checks each in normal/scalar modes. The SAME actual private
0.2.1 package and three usage examples pass both platforms; published packages
are unchanged. One production core build per OS serves the bounded round.
Admission/refusal/cancellation tests verify the smaller workspace, cleanup and
recovery; requesting later facts still refuses the insufficient small allowance.

Only dimension-probe CPU/allocation/admission evidence is superseded. Pixel
validation, packet/pixel algorithms, editing and other applicable confirmations
remain unchanged. Full proofs, actual package, matching measured Linux binaries,
sources and scripts are retained in the mirrored, member-verified raw archive
identified in the evidence; unchanged tooling remains in the prior raw master.
