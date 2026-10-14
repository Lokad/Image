# Warm RGBA8 bicubic support beyond sixty-four taps

Independent quiet confirmation measures **1.183× FFmpeg's scaler backend**,
conservative upper **1.192×**, at **465.61 µs / 199,354 B/op**. All twelve actuals
are retained, eleven selected, and both guards pass. The 8,705.05-µs / 22.139×
direct baseline falls **94.7%**. Allocations rise 197,178 B/op from 2,176 B/op for
optional operation-local workspace of 198,048 bytes.

The same original 513×259 RGBA8 raster becomes 32×16. A bounded 128-row/tap
extension removes repeated two-dimensional filtering. Its first fixed-margin
screen cuts the baseline 87% but remains **2.954×**, so it receives no twelve-
sample or full qualification. The accepted path applies the same
**2^-40 × 128²** per-unit absolute accumulation-error budget to each pixel's
actual alpha sum and normalizer. Color quotient uncertainty is bounded by
`(EC + abs(C/A)*EA)/(A-EA)`; alpha uses `(EA + abs(A/N)*EN)/(N-EN)`.
Both margins are doubled to cover their own rounding. These positive denominators
avoid the excessive fixed worst-case fallback. Uncertain rounding, nonpositive
lower denominators, normalizer/tiny-alpha guards and out-of-range handling retain
original evaluation independently for each pixel. Tap order, work charges,
cancellation and memory admission stay unchanged. Word support beyond sixty-four
taps, larger support, tight budgets and unsupported hardware retain the previous
path. No output or state is cached between calls.

Every managed sample matches the independent rational reference exactly. Seven
new public cases cover admission boundaries, odd widths, source changes, held
owners and negative/tiny/zero alpha. All 248 focused checks pass in normal/scalar
modes on Windows and Linux before candidate timing.

The native baseline is explicitly **zimg, FFmpeg's zscale backend**, with timed
thirty-two-pixel clamped padding, adding **40.269%** input area. Complete source
conversion, premultiplied float filtering, independent planar/packed output
allocation, unrounded-alpha unpremultiplication, straight byte quantization,
rounded-zero-alpha RGB clearing and release are timed. Both native modes are
pixel-exact on this raster, within the unchanged maximum-1 / RMSE-0.25 bounds.
Source, geometry, independent owners, operation counters and zero-alpha RGB
checks pass. This is the labeled backend, not FFmpeg CLI.

Screen and independent repeat use the same immutable production/harness. One
existing 64×32 byte regression screen passes at **1.783×** on that build before
full qualification. Prepared children and harness are reused; production rebuilds
occur only after changes. The native regression worker rebuilds once because
its shared source gains a new geometry. No additional profile is needed. The
initial dispatch preparation failure and above-two fixed-margin trial remain
retained, unscored or superseded. Two workloads use one of the two existing
original rasters.

Windows and Linux each pass **2,914 Release tests**, including **896 live FFmpeg
comparisons**, plus the consumer and all three examples through the **same actual
dependency-free Release package**. Final qualification runs once per platform.

The [numeric evidence](warm-bicubic32x16-8-rows-evidence.json) retains the baseline,
superseded trial, immutable accepted screen/confirmation, regression control and
qualification. Stop tuning this pair. Wider word support, tight budgets and
remaining material required/current-source uniform parity remain open.
