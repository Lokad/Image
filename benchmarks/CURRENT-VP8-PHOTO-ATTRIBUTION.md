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

**Next:** stream a prepared block through the complete ordered Luma4 mode search,
retaining quantizer/rate vectors without materializing ten candidate snapshots.
Check actual register retention and call/storage overhead before complete timing.
Preserve all strict prunes,ties,winner publication and portable/live/trellis/uniform
paths. The larger518sample location supplies a lead, not projected savings.

The [current evidence](vp8-writer-policy-evidence.json#currentAttribution) and its raw
archive retain perf data/maps, all instruction offsets,JITbytes,source/module pins,
guards and analysis scripts. Numerical and compression contracts are unchanged.
