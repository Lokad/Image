# Warm PNG RGBA8 decode: quiet confirmation

Complete public PNG decode independently confirms at
1.953x FFmpeg, conservative upper
1.995x. Both idle guards pass. This fixed pair
clears 2x; full required warm coverage remains open. Production is unchanged.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Guard-failed screen | 2,755.86 | 1.94227x | 1.98843x | FAIL |
| Independent quiet confirmation | 2,961.21 | 1.95328x | 1.99490x | Pass |

One existing original 513x259 RGBA8 raster and its identical 218,775-byte PNG,
level 6/filter None/noninterlaced, are reused. All RGBA samples, alpha, hidden RGB,
source and independent owners match exactly. Normal/scalar native CLI checks
pass. No new image, filter, shape or quality enumeration.

Timing includes complete public Decode, framing/checksums, inflation, row
reconstruction, admission, allocation and disposal. Native decode resets state
and acquires/releases independently owned output each time. Codec/frame wrappers
are reused; every pixel is decoded. Padded rows stay exposed. Startup, I/O, IPC,
preparation, hashing and validation are excluded. CPU0, one codec/conversion
thread, thread type zero, normal SIMD, tiering off and nonconcurrent workstation GC.

The screen fails its post-run CPU0 I/O-wait guard (2.004%) and remains failed.
Its raw evidence is frozen before the independent confirmation. Protocol
`exclusive-vm-idle-v2` checks live jobs and five-second aggregate/CPU0 samples:
busy <=5%, I/O wait <=1%, steal <=1%; historical load is diagnostic only.
The repeat retains 12 actual / 11 filtered
measurements and 12 native samples per bracket. Upper = larger filtered/all-actual
bound. Managed allocation is **755,274 B/op**, raw GC bytes / operations
rounded up including harness overhead. Native allocation totals are unmeasured.

Both timing children match all 568 production method bodies/flags in the same
accepted Release package. The preceding 2,388 tests and 254 scalar JPEG controls
per OS, plus same-package/all-three-example checks on both OSes, remain applicable.
There is no production or benchmark implementation change.
[Numeric evidence](warm-png8decode-quiet-evidence.json) retains measurements,
receipts, source/input/output/child/package identities, guards and immutable
archive digests. Regenerate with `python benchmarks/render-warm-png8decode-quiet.py`;
`--check` verifies this report. See the [current summary](../docs/benchmark.md).
Stop tuning this passing pair and cover the largest remaining material gap.
