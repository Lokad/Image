# Warm RGBA16 bicubic downsampling with wider support

Independent quiet confirmation measures **1.432× FFmpeg's scaler backend**,
conservative upper **1.446×**, at **575.04 µs** and **263,988 B/op**. Both guards
pass; all twelve actuals are retained and selected. The 9,993.39-µs / 25.058×
direct-evaluation baseline falls **94.2%**.

The existing original 513×259 RGBA16 raster becomes 128×64, with support reaching
seventeen taps. One primitive extends the bounded thirty-two-row/tap kernel to
words. Exact unsigned source products, binary64 unrounded horizontal
intermediates, unsigned word packing, checked accesses, work charges and
cancellation are preserved. The screen and confirmation use the same immutable
library/harness, with the generated child reused after changing only the library.
The identified admission gap requires no additional profile.

Wider word precision uses the conservative 2^-40×1024 absolute-error bound per
unit sample. At normalization at least 0.5 and alpha at least one, the alpha margin
is 1e-3; the color quotient bound is 8.01/alphaSum, conservatively evaluated using
the lower power of two. Margins at least 0.25 retain original evaluation, which
also protects clamped out-of-range samples at small alpha sums. Ambiguous and
tiny-alpha pixels use the original order and exact low-alpha handling. Byte
margins stay 1e-5 / 1e-3. No pixel tolerance or approximate reciprocal is added.

Optional workspace stays **1420×output width + 32×source width + 256 bytes**,
198,432 bytes here, leaving the existing 4 KiB exact-low-alpha allowance.
Allocations rise 198,324 B/op from the 65,664-B/op direct baseline, about 302%.
Supports beyond thirty-two taps, tight budgets and unsupported hardware retain
direct evaluation. Existing sixteen-tap kernels retain their previous path.
No state or output is cached between calls.

The native baseline is explicitly **zimg, FFmpeg's zscale backend**, through the
same original development adapter with compile-time depth selection. Its timed
eight-pixel clamped halo adds **9.489% input area**. Complete source conversion,
premultiplied float filtering, independent planar/packed allocation, unrounded-
alpha unpremultiplication, straight word quantization, rounded-zero-alpha RGB
clearing and release are timed. Normal RGB/alpha maximum errors are 1, RMSE
0.043731 / 0.044194; scalar RMSE is 0.049411 / 0.049411. The explicit native
maximum-1 / RMSE-0.25 bounds preserve exact managed rational pixels, source,
geometry and independent owners. Only the two existing original rasters are used.

Windows and Linux each pass **2,847 Release tests**, including **896 live FFmpeg
comparisons**, and the consumer plus all three examples through the **same actual
dependency-free Release package**. Seven new word cases cover wider support,
admission, odd widths, held owners, edits, full-range unsigned products/packing,
negative lobes and tiny/zero alpha. All 181 focused resize checks pass in
normal/scalar modes on both platforms. Two quiet three-actual controls check the
wider byte kernel and an existing word mixed path on the same build; these are
regression controls, not new independent confirmations.

The [numeric evidence](warm-bicubicwide16-rows-evidence.json) retains baseline,
immutable screen/confirmation, regression controls and qualification. Only three
distinct workloads are used. Stop tuning this fixed pair. Larger kernels,
single-axis resizing, tight budgets, public boundaries and uniform current-source
parity remain open.
