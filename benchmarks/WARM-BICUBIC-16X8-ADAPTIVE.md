# Warm RGBA8 bicubic adaptive support beyond 128 taps

Independent quiet confirmation measures **1.357× FFmpeg's scaler backend**,
conservative upper **1.368×**, at **614.50 µs / 201,721 B/op**. All twelve actuals
are retained and selected; both guards pass. The 8,704.20-µs / 19.318× direct
baseline falls **92.9%**. Allocations rise 201,081 B/op from 640 B/op for optional
operation-local workspace of 200,928 bytes.

The same original 513×259 RGBA8 raster becomes 16×8. One general bounded extension
selects the next power-of-two cache capacity for support above 128, up to **2048**
rows/taps; this measured shape selects 256. Larger row identifiers, offsets and
vertical weights occupy admitted portions of the existing managed arrays, with
no enlarged stack tables or persistent state. The same **2^-40 × capacity²**
absolute-error budget scales the exact premultiplied byte/word products and
per-pixel quotient guards. Uncertain rounding and original normalizer/tiny-alpha
handling retain direct evaluation. Geometry, tap order, work, cancellation and
memory admission stay unchanged. Support above 2048, tight budgets and unsupported
hardware retain the previous path.

Every managed sample matches the independent rational reference exactly. Fourteen
new byte/word cases cover capacities 256, 512, 1024 and 2048, admission boundaries,
odd widths, source edits, held owners and negative/tiny/zero alpha. All 276 focused
checks pass in normal/scalar modes on Windows and Linux before candidate timing.
These establish correctness across the bounded capacities; timing confirms only
the declared fixed workload.

The native baseline is explicitly **zimg, FFmpeg's zscale backend**, with timed
sixty-four-pixel clamped padding, adding **86.703%** input area. Complete source
conversion, premultiplied float filtering, independent output allocation,
unrounded-alpha unpremultiplication, straight byte quantization, rounded-zero-alpha
RGB clearing and release are timed. Both native modes are pixel-exact on this
raster, within the unchanged maximum-1 / RMSE-0.25 bounds. Source, geometry,
independent owners, counters and zero-alpha RGB controls pass. This is the labeled
backend, not FFmpeg CLI.

Screen and repeat reuse the same immutable production/harness. One existing
32×16 word regression screen passes on that build before qualification. Prepared
children and harness are reused; only changed production rebuilds. The native
regression worker rebuilds once because shared source gains the new geometry.
Two workloads use the two existing original rasters. No profile is needed.

Windows and Linux each pass **2,942 Release tests**, including **896 live FFmpeg
comparisons**, plus the consumer and all three examples through the **same actual
dependency-free Release package**. Final qualification runs once per platform.

The [numeric evidence](warm-bicubic16x8-8-rows-evidence.json) retains the baseline,
immutable screen/confirmation, word regression control and qualification. Stop
tuning this pair. Word timing at this shape, larger/unmatched supports, tight
budgets and remaining material required/current-source uniform parity stay open.
