# Current VP8 photograph attribution

**VPA989,actual upcoming-package DLL145cca00/packagec39ddab1.** One two-second
CPU0 software-clock trace uses the existing owned complete encode. Every frozen
packet/input/owner/reference gate passes; instrumented elapsed times are discarded.
No build or benchmark/helper/native/fixture change. Samples are leads, not component
timings, causal savings or projected speedups.

| Current sampled method | Samples |
| --- | ---: |
| Luma4 | 518 |
| WriteCoefficients | 156 |
| Luma16 | 142 |
| Transform | 83 |
| SmallCandidates | 78 |
| AddVector | 73 |
| Record | 71 |
| Chroma | 68 |
| Compare4 | 65 |
| DctFourPredictions | 50 |
| AddFourBlocks | 44 |
| ApplyBlocks | 43 |

Total1973samples. Luma4 code remains6630B;
uncancelable coefficient code is3515B, versus3967B
in the previous module. The smaller code and156coefficient samples do not prove
a phase-time reduction. Cursor arithmetic still repeatedly reads/writes stack fields.

[Accepted writer policy](VP8-WRITER-POLICY.md) records scoped incremental effects
and all nine actual standalone costs; photograph parity remains open. The previous
VPA984trace remains source-specific history in
[its evidence](current-vp8-photo-attribution-evidence.json), not current-module proof.
Three [candidate-preparation revisions](VP8-CANDIDATE-PREPARATION-TRIALS.md) are
fully restored and remain unaccepted.

The subsequent [word-publication trials](VP8-WORD-PUBLICATION-TRIALS.md) preserve
exact packets but establish no accepted gain; all three private layouts are removed.
The source/core/embedded baseline restores exactly, so this trace remains current.

The [luma execution trials](VP8-LUMA-EXECUTION-TRIALS.md) do not establish a
reproducible gain either; all source/modules restore exactly. The trace remains
current, and contradictory captures are retained rather than selecting a result.

The [two-candidate DCT tail](VP8-TWO-CANDIDATE-DCT-TAIL.md) also supplies no
useful elapsed lead despite exact coefficients and fewer logical transforms;
the original source/modules restore exactly.

**Next:** factor variable-price tree preparation:68levels share11leaf bands per
probability row. The accepted trace has29samples in VariableCost and8in table
filling, not phase times. Reuse each band's base cost while preserving fixed/EOB
prices,row cancellation and exact table/packet oracles. No new owner,cache or grid.

The [current evidence](vp8-writer-policy-evidence.json#currentAttribution) and its raw
archive retain perf data/maps, all instruction offsets,JITbytes,source/module pins,
guards and analysis scripts. Numerical and compression contracts are unchanged.
