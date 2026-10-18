# Cancelable PNG materialization

The two existing complete PNG decodes improve **32.1% for RGBA16** and **10.2%
for RGBA8**, with exact pixels and unchanged allocation strategy. Both means
remain above parity; these are qualified short results.

| Existing decode | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | PNG input B |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA16 / live token | 3,068.88 / 2,897.86 | 1.059 / 1.108 | 8,552 | 960,565 |
| RGBA8 / live token | 1,732.37 / 1,390.42 | 1.246 / 1.320 | 4,456 | 218,775 |

A separate materialized-block loop decodes each symbol, admits and writes a
first literal, then batches additional literals with the existing bounded helper.
Match symbols avoid an extra literal-helper call. This retains both per-literal
cancellation polls, exact atomic bit charges, length/distance/output validation,
overlapping copy order and work publication on every exit. Only complete-tree,
materialized blocks with a live token use the new loop. Counting, tight streaming,
noncancelable computation and bit-reader bodies remain unchanged. No new heap
array, field, table, workspace reservation or logical work charge is added.

Both sides time complete decoding, validation, normalization, independent output
acquisition and release. The original 513×259 authored RGBA8/16 rasters and frozen
None-filter PNG bytes remain. Native RGBA16 includes BE-to-LE conversion and a
second owned output. All samples, alpha, hidden RGB, source and ownership checks
are exact. Compressed output is inapplicable to decoding; input sizes appear above.
The already prepared FFmpeg reference, one codec/conversion thread and pixel gates
are reused. Native allocation is unmeasured; managed B/op includes amortized harness
overhead. No additional image or settings grid was introduced.

One CPU, normal SIMD, tiering off, nonconcurrent workstation GC. Two screens retain
all six 200ms actuals and both 5s idle-v2 guards, with 50ms native pre/post samples.
Upper uses the larger selected/all-actual bound and conservative native lower
bracket. Both means exceed 1, so no long confirmation was warranted.

All **3,725 Release tests per OS** pass: 1,391 internal, 1,436 public and 898 live
FFmpeg comparisons. Focused checks per OS comprise 119 internal Zlib tests and
141 public PNG tests; the 119 Zlib tests also pass with intrinsics disabled.
The independent fixed-code oracle covers 349 bit budgets and written prefixes;
existing tests extend live materialization coverage to overlap, maximum distance,
truncated input, output bounds and match failures. The **same actual private
Release package** passes its consumer and all three examples on Windows and Linux.
Measured producer bytes remain unchanged through qualification.

The preceding unscoped literal-helper routing was rejected after an 8.7% paired
RGBA8 regression. Its raw evidence is retained and unselected. The new loop instead
improves RGBA8, including against that recent qualified baseline. No tuning grid
follows this substantial gain.

[Numeric evidence](warm-png-cancel-blocks-evidence.json) retains exact source
reconciliation, all actuals, guards, references and package hashes. Two selected
independent gaps become qualified shorts: 125 independent observations and 82
qualified shorts remain across 207 rows. Other 205 comparisons and all 62 passing
proofs retain their paths. Full parity remains open: 62 passes, 144 measured gaps
and one confidence case.
