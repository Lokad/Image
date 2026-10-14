# Warm RGBA8 bicubic downsampling with wider support

Independent quiet confirmation measures **1.631× FFmpeg's scaler backend**,
conservative upper **1.642×**, at **588.69 µs** and **231,227 B/op**. Both guards
pass; all twelve actuals are retained, with eleven selected results. The
9,838.07-µs / 26.954× direct-evaluation baseline falls **94.0%**.

The existing original 513×259 RGBA8 raster becomes 128×64. Actual horizontal
and vertical support reaches seventeen taps, exceeding the previous sixteen-tap
cache admission. One bounded primitive uses thirty-two operation-local rows,
with at most thirty-two taps per axis, to reuse unrounded horizontal filtering
across overlapping output rows. Exact integer premultiplication, checked accesses,
tap order within each accumulator, work charges and cancellation are preserved.

The wider rounding guards use the same original-order/tiny-alpha fallback.
The conservative 2^-40×1024 absolute-error bound per unit sample covers both
accumulation orders. With normalization at least 0.5 and alpha at least one,
alpha/color margins of 1e-5 / 1e-3 select direct evaluation near quantization
thresholds. Managed pixels remain exact against the independent rational
reference; no approximate reciprocal or pixel tolerance is introduced.

Optional workspace is **1420×output width + 32×source width + 256 bytes**,
198,432 bytes here, leaving the existing 4 KiB exact-low-alpha allowance.
Allocations rise 198,331 B/op from the 32,896-B/op direct baseline, about 603%.
This pays for row reuse on a small output. Tight budgets, supports beyond thirty-two
taps and unsupported hardware retain direct evaluation. The previous sixteen-tap
paths retain their existing kernel. No state or output is cached between calls.

The native baseline is explicitly **zimg, FFmpeg's zscale backend**, using the
same original development adapter. This wider kernel requires a timed eight-pixel
clamped halo, **9.489% additional input area**. Complete source conversion,
premultiplied float filtering, independent planar/packed output allocation,
unrounded-alpha unpremultiplication, straight RGBA quantization, rounded-zero-alpha
RGB clearing and release are timed. Normal/scalar RGB maximum error is 1, RMSE
0.006379; alpha is exact for this raster. The explicit maximum-1 / RMSE-0.25
native bounds remain unchanged. Source, geometry and independent owners pass.

Initial setup attempts reject the smaller geometry and then the old minimum
output-stride assumption, without measured iterations. The controller now
validates the selected geometry and its actual output width. These refusals remain
recorded. The generated job is reused with the corrected harness; the qualified
production baseline is unchanged. No additional profile is needed for the
identified admission gap.

Windows and Linux each pass **2,840 Release tests**, including **896 live FFmpeg
comparisons**, and the consumer plus all three examples through the **same actual
dependency-free Release package**. Seven new public cases cover wider support,
odd widths, admission boundaries, held owners, edits, negative lobes and tiny/zero
alpha, including fallback beyond the new support cap. All 174 focused resize
checks pass in normal/scalar modes on both platforms. Two quiet three-actual
controls check the existing sixteen-tap byte path and a word mixed path on the
same build; these are regression controls, not new confirmations.

The [numeric evidence](warm-bicubicwide8-rows-evidence.json) retains baseline,
setup refusals, immutable screen/confirmation, regression controls and
qualification. Only three distinct workloads and the two existing original
performance rasters are used. Stop tuning this fixed pair. Wider word support,
larger kernels, tight budgets, public boundaries and uniform current-source parity
remain open.
