# Warm RGBA16 bicubic support beyond sixty-four taps

Independent quiet confirmation measures **1.125× FFmpeg's scaler backend**,
conservative upper **1.131×**, at **463.30 µs / 201,397 B/op**. All twelve actuals
are retained and selected; both guards pass. The 8,888.25-µs / 21.564× direct
baseline falls **94.8%**. Allocations rise 197,173 B/op from 4,224 B/op for optional
operation-local workspace of 198,048 bytes.

The same original 513×259 RGBA16 raster becomes 32×16. One extension admits words
to the existing bounded 128-row/tap kernel and generalizes its quotient-error
bounds to the full sample maximum. Exact unsigned UInt16 premultiplication uses
the unchanged packing and accumulation order. The **2^-40 × 128²** per-unit
budget bounds alpha by `unitError * 65535` and premultiplied color by
`unitError * 65535²`. Each pixel's actual positive alpha sum and normalizer bound
the color/alpha quotient error; doubled margins cover their own rounding.
Original threshold/tiny-alpha handling, uncertain-pixel fallback, work charges,
cancellation and memory admission remain unchanged. Larger supports, tight
budgets and unsupported hardware retain the previous path. No state is cached
between operations.

Every managed sample matches the independent rational reference exactly. Seven
new word cases cover admission boundaries, odd widths, source edits, held owners
and negative/tiny/zero alpha. All 262 focused checks pass in normal/scalar modes
on Windows and Linux before candidate timing.

The native baseline is explicitly **zimg, FFmpeg's zscale backend**, using the
existing original adapter's word macro; its shared C source is unchanged.
Thirty-two-pixel clamped padding is timed, adding **40.269%** input area. Complete
source conversion, premultiplied float filtering, independent planar/packed output
allocation, unrounded-alpha unpremultiplication, straight word quantization,
rounded-zero-alpha RGB clearing and release are timed. Both modes pass the
unchanged native maximum-1 / RMSE-0.25 bounds: normal RGB/alpha RMSE
0.062500 / 0.044194, scalar 0.091998 / 0.062500. The vertical-enlargement low-alpha
RGB exception does not apply. Source, geometry, independent owners, operation
counters and zero-alpha RGB controls pass. This is the labeled backend, not
FFmpeg CLI.

Screen and repeat use the same immutable production/harness. One existing 32×16
byte regression screen passes on that build before full qualification. Prepared
children and harness are reused; only changed production rebuilds. The byte
worker refreshes its previously stale shared-source identity once. No profile is
needed. Two workloads use the two existing original rasters.

Windows and Linux each pass **2,928 Release tests**, including **896 live FFmpeg
comparisons**, plus the consumer and all three examples through the **same actual
dependency-free Release package**. Final qualification runs once per platform.

The [numeric evidence](warm-bicubic32x16-16-rows-evidence.json) retains the baseline,
immutable accepted screen/confirmation, byte regression control and qualification.
Stop tuning this pair. Support beyond 128 taps, tight budgets and remaining
material required/current-source uniform parity stay open.
