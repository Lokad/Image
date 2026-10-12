# Warm VP8L decode: quiet confirmation

Complete public lossless WebP decode independently confirms at
1.693x FFmpeg, conservative upper
1.732x. Both idle guards pass. This fixed pair
clears 2x; full required warm coverage remains open. Production is unchanged.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Quiet screen | 3,706.59 | 1.72202x | 1.99801x | Pass |
| Independent quiet confirmation | 3,948.44 | 1.69254x | 1.73241x | Pass |

The existing original 513x259 RGBA8 raster and frozen owned VP8L input
(533,100 bytes) are reused. This declared literal/run
profile has no transforms, color cache or spatial entropy groups. All RGBA
samples, alpha, hidden RGB, input/output hashes, native CLI pixels, source and
independent owners remain exact. No new raster, profile, shape or quality sweep.
Other material VP8L decode profiles remain open; this is one fixed case.

Timing includes complete public Decode, admission, entropy reconstruction,
owned output acquisition and disposal. Native software WebP decode resets
state, converts ARGB to RGBA and acquires/releases independent output each time.
Codec/conversion contexts and packet/frame wrappers are reused; every pixel is
decoded and converted. Padded rows stay exposed. Startup, I/O, IPC, input
encoding/preparation, hashing and validation are excluded. CPU0, one codec/
conversion thread, thread type zero, normal SIMD, tiering off and nonconcurrent
workstation GC remain fixed.

Protocol `exclusive-vm-idle-v2` checks live jobs and five-second aggregate/CPU0
samples: busy <=5%, I/O wait <=1%, steal <=1%; historical load is diagnostic
only. The independent repeat retains 12 actual / 12 filtered measurements and
12 native samples per bracket. Upper = larger filtered/all-actual bound.
Managed allocation is **1,074,301 B/op**, raw GC bytes / operations rounded
up including harness overhead. Native allocation totals are unmeasured.

Both timing children match all 568 production method bodies/flags in the same
accepted Release package. The preceding 2,388 tests and 254 scalar JPEG controls
per OS, plus same-package/all-three-example checks on both OSes, remain applicable.
No production or benchmark implementation changed. The
[earlier reader evidence](WARM-VP8L-READ-INLINE.md) and its failures remain retained.
[Numeric evidence](warm-vp8ldecode-quiet-evidence.json) preserves measurements,
receipts, source/input/output/child/package identities, guards and immutable
archive digests. Regenerate with `python benchmarks/render-warm-vp8ldecode-quiet.py`;
`--check` verifies this report. See the [current summary](../docs/benchmark.md).
Stop tuning this passing pair and establish the missing required encode baseline.
