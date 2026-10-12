# Warm VP8 decode: independent quiet confirmation

Complete public VP8/ALPH decode independently confirms at
1.913x FFmpeg, conservative upper
1.948x. Both idle guards pass. This fixed pair
clears 2x; full required warm coverage remains open. Production is unchanged.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Coverage screen | 11,568.81 | 1.90051x | 2.01349x | Pass |
| Independent quiet coverage confirmation | 11,623.12 | 1.91340x | 1.94804x | Pass |

The existing original 513x259 RGBA8 raster and identical frozen 98,590-byte
native-quality-75 VP8/ALPH keyframe are reused. No new images, profiles, effort
or quality enumeration. Both subjects decode the same encoded input; encoder
quality matching is irrelevant here. Fixed managed/native output hashes,
RGB maximum <=4/RMSE <=1.5, exact alpha, source and independent owners pass.
Native CLI pixels match the registered reference pipeline exactly.

Both time complete decode to independently owned RGBA8 and disposal. Native
decode/reset, centered bilinear chroma expansion, limited BT.601 conversion
through RGBA16, reduction, alpha merge and scratch/output allocation are included.
Codec, packet/frame wrappers and conversion contexts are reused; every pixel is
reprocessed. Padded rows stay exposed. Startup, I/O, IPC, hashing, validation
and encoding are excluded. CPU0, one codec/conversion thread, thread type zero,
normal SIMD, disabled tiering and nonconcurrent workstation GC are fixed.

Protocol `exclusive-vm-idle-v2` checks live jobs and five-second aggregate/CPU0
samples: busy <=5%, I/O wait <=1%, steal <=1%. Exclusive VM use requires no
historical-load cooldown. Load averages are diagnostic only. The earlier
[color-conversion round](WARM-VP8-COLOR-BLOCK.md) retains its legacy failed
guards; these are not reclassified. Preparation/smoke checks occur outside timing.

The independent repeat retains 12 actual / 10
filtered measurements and 12 native samples per bracket. The upper uses the
larger filtered/all-actual bound. Managed allocation is **1,966,616 B/op**,
raw GC bytes / operations rounded up, including harness overhead. Native
allocation totals are unmeasured.

Both actual timing children match all 566 production method bodies/flags in
the same accepted Release package. The preceding 2,387 tests and 90 scalar
framing controls per OS, plus same-package/all-three-example checks on both
OSes, remain applicable: no production or benchmark implementation changed.
[Numeric evidence](warm-vp8decode-quiet-evidence.json) retains measurements,
receipts, source/input/output/child/package identities, guards and immutable
raw archive digests. Regenerate with `python benchmarks/render-warm-vp8decode-quiet.py`;
`--check` verifies this report. See the [current summary](../docs/benchmark.md).
Stop tuning this passing pair; confirm the largest remaining matched gap.
