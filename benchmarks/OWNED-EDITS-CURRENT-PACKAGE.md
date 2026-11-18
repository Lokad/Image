# Owned edits on the current package

Sixteen of the eighteen original owned edits affected by the shared idle pool
passed on the first tested 0.2.1 package. Clone, normal orientation, horizontal flip,
half turn, both quarter turns and both transpose operations pass at both depths.
The two [vertical flip passes](VERTICAL-PREPARED-ROWS.md) follow the pool change
and remain valid. The RGBA8 crop now passes on the qualified build below.

| Complete crop / FFmpeg owned output | Lokad.Image µs | FFmpeg µs | Conservative interval | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 | 2.374 | 2.287 | 0.996633–1.064256 | 56 |
| RGBA16 | 4.135 | 4.151 | 0.964557–1.005834 | 56 |

All eighteen checks use full public admission, independent output materialization
and disposal. FFmpeg retains graph and frame wrappers, then materializes,
owns and releases the output on every operation. The original native padding
and view boundaries remain. Full samples, alpha, hidden RGB, immutable source
and surviving-output tests pass. Every case allocates 56 managed B/op.
This counter is separate from live output capacity, idle retention and physical
peaks.

The original runs retain twelve alternating pairs, equal native brackets
and the original per-case counts of 64, 256 or 4,096. The crop batches at
64 calls took under 0.3 ms. Their uppers were 1.114241 and 1.081318.
The longer crop confirmation uses the existing 4,096-call driver, whose
source differs only in the batch-size allowance. Original inputs, work,
confidence rule and native reference remain. It narrows both intervals,
and both still cross the parity threshold. No further identical sampling
is planned. Both guard checks pass for every capture.

Three runtime groups preserve the verified original library and driver
combinations while loading the same actual package DLL. Preflight initially
assumed every historical receipt passed and carried explicit output dimensions.
The two crop receipts were uncertain, and the horizontal flip schema uses
the input dimensions for its full-size output. Both corrections happened
before timing. The existing package and qualification evidence are reused.

The mirrored `owned-edits-current-package-confirmations.tar.xz` contains 134 verified members
and 111,384 B, SHA-256 `1188a31f97b54404694ad51d88bbbf3a447625b566cf493c2d9500384948854e`.
It retains all measured samples, guards, validation records and source recipes.
[Evidence](owned-edits-current-package-evidence.json)

[Crop row reuse](CROP-ROW-REUSE.md) qualifies the new RGBA8 crop at
**2.165 / FFmpeg 2.521 µs**, conservative upper **0.877018**, and **56 B/op**
on the actual package. The associated complete PNG8 stream pipeline also
passes. Seventeen of the eighteen original pool-affected cases now have valid
passing evidence. The RGBA16 crop stays open at upper 1.005834, using the
source-valid word route measurement above.
