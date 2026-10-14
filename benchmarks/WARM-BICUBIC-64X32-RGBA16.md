# Warm RGBA16 bicubic support beyond thirty-two taps

Independent quiet confirmation measures **1.387× FFmpeg's scaler backend**,
conservative upper **1.414×**, at **523.91 µs** and **214,071 B/op**. All twelve
actuals are retained, ten selected, and both guards pass. The 8,923.42-µs /
23.466× direct baseline falls **94.1%**. Allocations rise from 16,512 B/op by
197,559 B/op for optional operation-local workspace of 197,920 bytes.

The same original 513×259 RGBA16 raster becomes 64×32. One primitive admits words
to the existing sixty-four-row/tap cache and adds unsigned certain-neighbor
packing. Source products remain exact across the full UInt16 range. Workspace is
**2828×output width + 32×source width + 512 bytes**, retaining 4 KiB for exact
low-alpha handling. Supports beyond sixty-four taps, tight budgets and unsupported
hardware keep direct evaluation. No state or output is cached between calls.

Word alpha uses a **0.004** margin, and its conservative color quotient bound is
**32.04/alphaSum**, evaluated with the reciprocal of the lower power of two.
The per-unit absolute-error bound is 2^-40×4096, with normalization at least 0.5
and fast alpha at least one. Margins at least 0.25 retain original evaluation,
including clamped out-of-range samples. The same guards decide uncertainty per
pixel; certain neighbors use word packing. Original order and exact tiny-alpha
handling remain active where required. Every managed sample matches the independent
rational reference. No pixel tolerance or approximate reciprocal is added.
Existing thirty-two-tap word margins and byte margins stay unchanged.

The native baseline is explicitly **zimg, FFmpeg's zscale backend**, through the
same original adapter's existing depth macro. Only the newly selected word worker
and harness require preparation; the verified byte worker is reused. Its timed
sixteen-pixel clamped halo adds **19.364% input area**. Complete source conversion,
premultiplied float filtering, independent planar/packed allocation, unrounded-alpha
unpremultiplication, straight word quantization, rounded-zero-alpha RGB clearing
and release are timed. Both native modes satisfy the unchanged maximum-1 /
RMSE-0.25 bounds: normal RGB/alpha RMSE 0.054127 / 0.049411, scalar
0.073288 / 0.038273. Source, geometry, independent owners and counters pass.
This is the labeled scaler backend, not FFmpeg CLI.

Screen and independent confirmation use the same immutable production/harness.
Two quiet three-actual controls check the byte sixty-four-tap and word thirty-two-
tap paths at **1.753× / 1.412×** before acceptance qualification. Only three distinct workloads and the
two existing original rasters are used. Prepared children and the verified harness
are reused; only changed production inputs are rebuilt. The already identified
admission gap needs no additional profile.

Windows and Linux each pass **2,875 Release tests**, including **896 live FFmpeg
comparisons**, plus the consumer and all three examples through the **same actual
dependency-free Release package**. Seven new word cases exercise larger support,
odd widths, memory admission, held owners, edits, full-range products/packing,
negative lobes, tiny/zero alpha and support beyond sixty-four taps. All 209 focused
resize checks pass in normal/scalar modes on both platforms. Final qualification
runs once per platform for the accepted production change.

The [numeric evidence](warm-bicubic64x32-16-rows-evidence.json) retains baseline,
screen, confirmation, regression controls and qualification. Stop tuning this fixed
pair. Other single-axis directions, tight budgets, larger supports and remaining
required/current-source parity stay open.
