# JPEG row-product study

**Not adopted: the controlled complete-operation result is inconclusive.**
Production and the actual package are restored byte-exact. Two independent matrix
test cases remain against the real production transform.

| Original horizontal-CMYK decode, 12 alternating immutable-DLL pairs | Original µs | Candidate µs | Mean-pair ratio interval |
| --- | ---: | ---: | ---: |
| Complete public Decode, normalization, independent output and disposal | 395.51 | 399.34 | 0.997–1.023 |

The point is approximately1% adverse, with no demonstrated gain or regression.
Every actual pair remains. The earlier three-actual screen398.17/native388.29 µs
is wide and cannot qualify parity or an improvement; sparse-2D419.94/434.09 µs is
also screen-only. No standalone confirmation or favorable repeat follows this
inconclusive increment. Current accepted costs remain unchanged.

The candidate reuses exact post-dequantization support in the shared row loop to
omit three trailing dot products only when their words are all zero. Other rows
keep the original ordered sums. This applies independently at any vertical
frequency, preserving bias, modulo arithmetic, saturation, stores and column
publication. Unlike earlier top-left helper or entropy-output designs, every
other production body remains verbatim. The source hypothesis removed algebraic
zero work, but that does not establish useful elapsed savings. No allocation,
capacity, owner, pool, retained scratch, dependency or numerical-policy change
occurs; main allocation remains20,944 B/op and physical/native/JIT peaks are
unmeasured.

Each candidate passes466internal/342public JPEG-focused checks per normal, scalar,
AVX512-off and AVX2-off mode on Windows/Linux. New tests add4,096 blocks/262,144
independent scalar-matrix samples across mixed spectra, full-width products,
modular zeros, natural/scan order, every vertical row, canaries and borrowed inputs.
After restoring production, all five matrix-class cases pass in all four modes on
both OSes. One necessary embedded-only build per OS checks those retained tests;
the production DLL is copied back without rebuilding.

One immutable core/embedded candidate build per OS serves the round; original
harnesses, workers and fixtures are reused. The actual package is untouched.
Reuse the prior6,600 full/live checks per OS, scalar/ISA and SAME-package/all-three-
example proof for unchanged production. No broad/package campaign occurs.
Exact encoder packets, managed pixels, ownership and all289material paths remain.

This is an inconclusive implementation, not a permanent row-family prohibition.
The next justified lead is actual repeated raw-row normalization: compare before
mutation, normalize a run once, then copy completed rows with cancellation. Check
companions and exact output before any acceptance; no fixture-specific reuse.
Publication remains postponed and PLAN's authorized edit scope stays unchanged.

[Evidence](jpeg-row-product-study-evidence.json) retains the complete pair, both
screens, source/module identities, candidate qualification and restoration.
Raw `jpeg-row-product-study.tar.xz`: 1,383,788 B / 85 members,
SHA256 `c97715580fc3680207a7cd082f126c0fc9873d06ca1eb9dea8b03a34da1a4894`, mirrored and member-verified.
Dependencies retain the motivating current-build profile, original tools/inputs
and qualified actual package. No current renderer cost depends on this study.
