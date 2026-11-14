# Prior VP8 photograph attribution

**Historical after VTF997.** All nine current lossy controls now pass/stop in
[variable-price preparation](VP8-VARIABLE-PRICE-PREPARATION.md). The trace below
predates that production change; VariableCost no longer exists in production.
It is retained source-specific evidence, not a current profile or next task.

**VPA989,DLL145cca00/packagec39ddab1.** One two-second
CPU0 software-clock trace uses the existing owned complete encode. Every frozen
packet/input/owner/reference gate passes; instrumented elapsed times are discarded.
No build or benchmark/helper/native/fixture change. Samples are leads, not component
timings, causal savings or projected speedups.

| Prior sampled method | Samples |
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

Total 1,973 samples. Luma4 code was 6,630 B;
uncancelable coefficient code was 3,515 B, versus 3,967 B
in the previous module. The smaller code and156coefficient samples do not prove
a phase-time reduction. Cursor arithmetic still repeatedly reads/writes stack fields.

[Accepted writer policy](VP8-WRITER-POLICY.md) records scoped incremental effects
and its nine source-specific standalone costs; photograph parity was open then. The previous
VPA984trace remains source-specific history in
[its evidence](current-vp8-photo-attribution-evidence.json), not current-module proof.
Three [candidate-preparation revisions](VP8-CANDIDATE-PREPARATION-TRIALS.md) are
fully restored and remain unaccepted.

The subsequent [word-publication trials](VP8-WORD-PUBLICATION-TRIALS.md) preserve
exact packets but establish no accepted gain; all three private layouts are removed.
The source/core/embedded baseline restored exactly, so this trace was reused at that checkpoint.

The [luma execution trials](VP8-LUMA-EXECUTION-TRIALS.md) do not establish a
reproducible gain either; all source/modules restored exactly. Contradictory
captures are retained rather than selecting a result.

The [two-candidate DCT tail](VP8-TWO-CANDIDATE-DCT-TAIL.md) also supplies no
useful elapsed lead despite exact coefficients and fewer logical transforms;
the original source/modules restore exactly.

**Stop the confirmed encoder paths.** The full289-path objective still has
independent PNG/resize/edit/inspection work. Begin with the named retained PNG
RGBA16uncertainty,inspecting current integration and reusable evidence before
assuming optimization is needed. No new image/settings grid or publication.

The [historical evidence](vp8-writer-policy-evidence.json#currentAttribution) and its raw
archive retain perf data/maps, all instruction offsets,JITbytes,source/module pins,
guards and analysis scripts. Numerical and compression contracts are unchanged.
