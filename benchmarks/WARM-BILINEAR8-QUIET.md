# Warm bilinear RGBA8: independent quiet confirmation

Complete public bilinear resize independently confirms at
1.698x FFmpeg, conservative upper
1.704x. Both idle guards pass. This fixed pair
clears 2x on the current qualified production DLL; full required warm coverage
remains open. The 2026-10-16 round resolves uncertainty after shared Resize
dispatch changes. No production optimization is needed or introduced.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Coverage screen | 731.43 | 1.94663x | 3.40636x | Pass |
| Independent quiet coverage confirmation | 729.12 | 1.93765x | 1.95828x | Pass |
| current-bilinear8-short | 660.95 | 1.70712x | 1.87117x | Pass |
| current-bilinear8-confirm | 656.29 | 1.69774x | 1.70408x | Pass |

One existing original 513x259 RGBA8 raster is resized to 256x129. Managed
samples match the independent exact rational reference. Native alpha matches
exactly, zero-alpha RGB is zero, and the unchanged RGB maximum <=1/RMSE <=0.25
bound passes. Input, managed/native output hashes, native CLI/deterministic
pixels, source and independent owners are unchanged. No image, geometry,
filter or strategy enumeration. The other three existing bilinear cases are
reconciled separately; this result certifies only the stated byte downsample.

Managed timing includes public Resize, geometry preparation, admission,
allocation/materialization and disposal. The native public FFmpeg graph includes
packed/planar conversion, zscale widened triangle filtering, encoded-space
premultiplication/unpremultiplication, final RGBA conversion and independent
output acquisition/release. Graph/source/frame wrappers and geometry contexts
are reused; every call filters the full input. Native padded output rows stay
exposed. No output cache. Startup, I/O, IPC, hashing and validation are excluded.
CPU0, one filter thread, thread type zero, normal SIMD, disabled managed tiering
and nonconcurrent workstation GC remain fixed.

Protocol `exclusive-vm-idle-v2` checks live jobs and five-second aggregate/CPU0
samples: busy <=5%, I/O wait <=1%, steal <=1%. Historical load is diagnostic
only. Preparation/smoke checks occur outside timing. The earlier
[packed-alpha round](WARM-RESIZE-QUAD-ALPHA.md) retains its original failed guards.

The independent repeat retains 12 actual / 11
filtered measurements and 12 native samples per bracket. Its upper uses the
larger filtered/all-actual bound. Managed allocation is **157,957 B/op**,
raw GC bytes / operations rounded up, including harness overhead. Native
allocation totals are unmeasured. This is a single-result warm operation.

Both current timing children use the same verified production, benchmark and
native worker binaries; only cached selection drivers change. Production,
harness and native rebuild counts are zero. The existing
[qualification](WARM-JPEG-AC-PLANNING.md), 3,483 Release tests per OS including
live FFmpeg comparisons and the same actual package's consumer plus all three
examples on Windows/Linux, remains valid because production/package inputs
are unchanged. No full suite, package proof or whole-library method audit is
repeated. Older rows and their original package/method proofs remain historical
evidence at their recorded builds. The current source/core/child identities,
unchanged pixel hashes and reused qualification are recorded separately.
[Numeric evidence](warm-bilinear8-quiet-evidence.json) retains measurements,
receipts, source/input/output/child/package identities, guards and immutable
raw archive digests. Regenerate with `python benchmarks/render-warm-bilinear8-quiet.py`;
`--check` verifies this report. See the [current summary](../docs/benchmark.md).
Stop tuning this passing pair and confirm the largest remaining gap.
