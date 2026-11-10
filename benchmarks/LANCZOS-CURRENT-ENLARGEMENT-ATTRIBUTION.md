# Current prepared Lanczos enlargement

This capture predates [paired word windows](LANCZOS-PAIRED-WORD-WINDOWS.md).
That producer supersedes the horizontal-body attribution and owns current costs;
the original capture below remains historical evidence, not current phase costs.

The subsequent [fused traversal round](LANCZOS-FUSED-VERTICAL-EXPERIMENT.md)
restores both unselected implementations and retains cache-boundary coverage.

LPH690 records one two-second capture of the frozen prepared RGBA16 enlargement
on the qualified actual-package DLL. It reuses the worker, harness, selectors,
inputs and exact output checks. All original gates and both quiet guards pass;
CPU0 runs the operation and CPU1 the sampler. Profiled elapsed is discarded.
There is no new benchmark cost, production build or numerical-policy change.

Of 1,671 sampled locations, 563 are in final pixel writing/certification, 518 in
horizontal word filtering, 479 in vertical accumulation and 22 in the shared
driver. Locations are not phase times or projected savings. The earlier capture
predates the certified horizontal-row change and cannot stand in for this code.

The independently owned 4,264,104-byte output exceeds the existing 4 MiB default
pool ceiling, explaining most of the existing 4,265,332 B/op. This establishes
allocation volume, not allocator elapsed cost or a benefit from a larger pool.
No ceiling changes. The native adapter allocates float output planes and a packed
independent result during each apply, then frees them. Prepared execution retains
its graph/input/scratch; managed execution retains 333,396 bytes of plan/workspace.
Input normalization, filtering, output materialization and disposal remain timed
on their declared sides. Q30/rational managed pixels remain exact despite native
float intermediates.

The selected lead is shared vertical traversal. Current fast execution clears
the sum row and reads/writes it once per nonzero vertical tap. Preparing all
required horizontal rows first can allow one ordered vector accumulation and one
store per sample. Consecutive rows occupy distinct ring slots only when the
window fits the cache; preserve the streaming fallback otherwise. A bounded
common-support implementation can use existing rows/sums and a minor stack list,
with unchanged per-lane FMA order, certificates, rounding and exact repair.
Span checks, pointer reloads and loop pressure remain concrete risks; this is a
hypothesis, not an accepted speedup. Existing prepared word enlargement/reduction
are the first complete screens, without a new workload grid.

[Evidence](lanczos-current-enlargement-attribution-evidence.json) preserves the
source/ownership audit and proposed stage. Existing Windows/Linux/scalar/ISA and
same actual package qualification is reused. Current costs remain owned by
[certified word rows](LANCZOS-WORD-CERTIFIED-ROWS.md). Mirrored/member-verified raw
`lanczos-current-enlargement-attribution.tar.xz` has79,092B/24members, SHA256
`e5fc490280c52351dad16d56022f726cd6bb0e0dff409ecbce7bb9da487f1e77`.
No renderer dependency is added. Compression correctness and the full performance
objective remain; publication is postponed.
