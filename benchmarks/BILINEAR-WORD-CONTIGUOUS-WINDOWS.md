# Contiguous source windows for RGBA16 bilinear enlargement

RGBA16 enlargement from 513×259 to 1027×519 saves **227.05 µs per call** in
twelve alternating pairs against the preceding package. The candidate/current
ratio is **0.890297**, with interval **0.882381–0.898284**. Both sides time
the complete public resize, including fresh geometry, workspace, allocation,
result publication and disposal. Allocation stays at about 4.50 MB per call.

| Reference boundary | Lokad.Image / FFmpeg µs | Conservative upper | Managed GC B/op |
| --- | ---: | ---: | ---: |
| Prepared FFmpeg graph, fresh managed resize | 1,894.77 / 1,994.87 | 0.973427 | 4,498,579 |
| Fresh graph and geometry on both sides | 1,865.92 / 4,801.28 | 0.513600 | 4,498,579 |

Each reference comparison retains twelve 256-operation units and its original
quiet-VM schedule. The prepared-graph comparison interleaves the reference
groups within each unit. The fresh-graph worker includes graph, geometry and
output preparation. Both use the original pinned FFmpeg workers and inputs.
Managed frames match the exact rational oracle. Native RGB and alpha differ by
at most one sample, with RMSE below 0.05 in both cases. Independent outputs,
unchanged inputs and precision, cancellation and memory ownership are covered.

Geometry preparation identifies which groups of four destination pixels can
read all eight taps from four contiguous source pixels. The row filter loads
that window once and permutes its lanes for the left and right taps. The source
window stays within its row even at clamped edges. Geometry reuses two existing
slots. Wider groups and CPUs without the required SIMD feature keep the original
gather. Accumulation, rounding certificates and exact repair stay the same.
There is no added admitted capacity or retained workspace. The near-unity test
exercises both window and gather groups within the same operation.

Windows and Linux each pass 6,863 checks, including 948 live FFmpeg checks.
Focused tests pass in normal and scalar modes on both platforms. Both consumers
and all three usage examples use the same actual Release package. Its DLL is
byte-identical to the one measured on the VM. This result covers the two named
enlargement comparisons. The other
bilinear cases retain their earlier evidence, and the complete final-package
refresh remains open.

The [evidence](bilinear-word-contiguous-windows-evidence.json) pins the immutable
artifacts, all actuals, exact output gates and platform checks. Earlier
[prepared-graph timing](BILINEAR-PARTITIONED-REFERENCE.md),
[fresh-graph timing](BILINEAR-EQUIVALENT-ONE-SHOT.md) and
[row storage experiments](BILINEAR-WORD-ROW-STORAGE-TRIALS.md) retain their
original producer and boundaries.
