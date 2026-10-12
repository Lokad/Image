# Warm JPEG: combined baseline AC prefix and amplitude

Complete public baseline JPEG 4:4:4 decode independently confirms at
1.941x FFmpeg, conservative upper
1.961x. Complete managed mean falls
4.18% versus the valid baseline repeat.
Both current idle guards pass. This fixed pair clears 2x; full required warm scope remains open.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Baseline screen | 3,708.38 | 2.02480x | 2.55191x | Pass |
| Independent baseline above 2x | 3,484.80 | 2.02548x | 2.09261x | Pass |
| Rejected inlining / failed guard | 3,444.52 | 1.99675x | 2.11549x | FAIL |
| Combined AC screen | 3,361.69 | 1.94909x | 2.29916x | Pass |
| Independent combined AC confirmation | 3,339.18 | 1.94140x | 1.96113x | Pass |

The same existing original 513x259 RGBA8 raster and frozen 133,041-byte native
baseline JPEG are reused. Equal component factors `1x2,1x2,1x2` give 4:4:4.
Input and managed/native output hashes are exact and unchanged. Normal/scalar
native CLI agreement remains RGB maximum <=4/RMSE <=1.5, with exact opaque
alpha, unchanged source and independent owners. No image, shape, quality,
scan-strategy or effort enumeration.

One six-second baseline profile records 5,407 CPU samples: 32.2% in short
Huffman reads, 19.6% in bit reads and 16.7% in baseline block dispatch. The new
independent T.81 F.2.2 primitive reads a valid short AC prefix and amplitude
together, publishing reader state once. It admits exactly prefix-length+1 work
units. Speculation stops before invalid size/run syntax. Long/unassigned codes,
EOB/zero runs, markers, truncation, tight budgets and cancelable operations retain
the original path and partial-state/failure precedence. Progressive traversal,
table construction, memory admission and ownership are unchanged. No new heap
allocation, dependency, unsafe or native production code.

The single-method inlining trial is discarded: its screen lacked sufficient
margin and fails the post-run aggregate I/O-wait guard. The original method flag
is restored before the combined-AC candidate. A candidate confirmation preguard
also fails I/O wait and launches no benchmark; it is frozen before one fresh
attempt using the same four prepared child bytes. All limits/code/inputs remain
unchanged. Protocol `exclusive-vm-idle-v2` checks live jobs and five-second
aggregate/CPU0 samples: busy <=5%, I/O wait <=1%, steal <=1%. Historical load is
diagnostic only. All failures and actuals remain retained.

A regression control reuses the existing frozen grayscale JPEG, whose earlier
upper was close to 2x. It independently confirms under the new primitive:

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Coverage screen | 1,270.49 | 1.86056x | 2.10572x | Pass |
| Independent quiet coverage confirmation | 1,267.57 | 1.85662x | 1.87975x | Pass |

Both confirmations retain 12 actual / 12 filtered managed measurements and
12 native samples per bracket. Upper = larger filtered/all-actual bound.
Managed allocation is **2,664,358 B/op** for 4:4:4 and
**1,221,545 B/op** for grayscale, raw GC bytes / operations rounded
up including harness overhead. Native allocation totals are unmeasured.

Timing includes public Decode, framing/admission, entropy reconstruction,
inverse transform, color conversion, independent output allocation and disposal.
Native software decode/state reset, full-range BT.601 conversion and owned
output allocation/release are included; grayscale expands decoded GRAY8 into an
independent RGBA8 buffer each time. Native codec/conversion/packet/frame wrappers
are reused; every pixel is reprocessed. Padded rows stay exposed. Startup, I/O,
IPC, preparation, hashes and validation are excluded. CPU0, one codec/conversion
thread, thread type zero, normal SIMD, tiering off and nonconcurrent workstation GC.

Both OSes pass **2,388 tests** each (644 internal, 848 public, 896 live FFmpeg),
plus 254 scalar JPEG controls each. New controls compare every tight budget,
truncation, marker/invalid syntax, predictor, partial coefficient writes and
cursor against the cancelable per-symbol path. The same actual Release package
and all three usage examples pass on both OSes. Fresh builds, timing children
and that package match all 568 production method bodies/flags. Contract/support unchanged.

[Numeric evidence](warm-jpegac-fused-evidence.json) retains all samples,
receipts, guards, source/input/output/child/package identities, correctness
proofs and immutable archive digests. Regenerate with
`python benchmarks/render-warm-jpegac-fused.py`; `--check` verifies this report.
See the [current summary](../docs/benchmark.md) and
[reference recipe](../tools/WarmReference/README.md). Stop tuning these passing
pairs and prioritize the largest remaining material gap.
