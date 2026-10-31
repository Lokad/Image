# Current tight-memory byte bicubic enlargement

The existing 513×259 RGBA8 →1027×519 control independently confirms **1.758× /
conservative upper 1.891**, making it the largest current gap involving substantial
per-image work. Its **4 KiB extra workspace** stays unchanged. This refreshes an
older qualified short without a causal gain/regression claim.

| Complete warm single-CPU resize | Lokad µs | FFmpeg backend µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| TightUp8, input+output+4 KiB | 5,351.266 | 3,043.227 | 1.758 / 1.891 | 2,492,956 |

The local limit is exactly **2,667,616 bytes**: 531,468 source bytes, 2,132,052
output bytes and 4,096 workspace bytes. This tests the existing strip algorithm,
distinct from regular prepared enlargement. Complete public admission, geometry,
filtering, fresh output ownership and disposal are timed. The fixed zimg backend
used by FFmpeg zscale includes halo/packing/premultiplication, filtering, independent
planar/packed output, unrounded-alpha division/quantization and release. Native
allocation is unmeasured; startup/preparation/I/O/validation are excluded.

All twelve actuals, both confidence views, native brackets and passing idle-v2
guards are retained. Managed rational pixels remain exact and all frozen hashes
match. Native RGB/alpha maximum1, RMSE0.003537/0.002373 satisfy the unchanged
maximum1/RMSE≤.25 bounds. Normal/scalar, zero-alpha RGB, source and independent
output checks pass before/after. No image, shape, setting or required case is added.

Zero builds/profiles: reuse the qualified immutable core, cached harness/native
worker and verified prior twelve-actual selector. Unchanged Windows/Linux full/live
and same actual Release-package/three-example qualification carry forward.

[Evidence](current-tight-byte-bicubic-enlargement-cost-evidence.json) pins every
sample, identity, boundary and qualification reuse. Mirrored/member-verified raw:
`current-tight-byte-bicubic-enlargement-cost.tar.xz`, SHA256 `31f6b5ae6ebf68be3ebbc14e10a1a20e37e2f44f7cd34fb87e3358c1f2de6138`;
runtime/qualification dependencies are separately pinned. Current attribution
comes next; retain the exact-pixel, memory and ownership contracts and the full
268-path upper≤1× objective.
