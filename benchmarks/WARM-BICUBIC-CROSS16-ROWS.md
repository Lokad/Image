# Warm RGBA16 bicubic width reduction and height enlargement

Independent quiet confirmation measures **1.285× FFmpeg's scaler backend**,
conservative upper **1.297×**, at **1,044.98 µs** and **1,263,255 B/op**. Both
guards pass; all twelve actuals are retained, with eleven selected results. The
19,600.92-µs / 23.838× direct-evaluation baseline falls **94.7%**.

The existing original 513×259 RGBA16 raster becomes 256×519. One admission
change routes this direction through the existing checked, operation-local
row cache and word writer. Horizontal eight/nine-tap filtering is reused across
overlapping vertical output rows. Exact unsigned source products, binary64
intermediates, unsigned word packing, precision guards and original-order/tiny-
alpha fallback are unchanged. Work charges, cancellation and source/owner
contracts are preserved. No approximate reciprocal or pixel tolerance is added.

Optional workspace is **716×output width + 32×source width + 128 +
512×(output width & 1) bytes**, 199,840 bytes here. Allocations rise 200,198 B/op,
about 18.8%. Tight budgets retain direct evaluation with identical pixels;
odd widths remain padded for checked two-pixel indexing. No state or output
is cached between calls. The same immutable library/harness is reused for the
screen, independent confirmation and two regression screens without rebuilding.
The admission gap requires no additional profile.

The explicitly labeled native baseline is **zimg, FFmpeg's zscale backend**.
The same original adapter selects direction and depth at compilation. Its
four-pixel clamped halo, complete source conversion, premultiplied float
filtering, independent planar/packed output allocation and release are timed.
Packing unpremultiplies using unrounded alpha, quantizes straight full-word RGBA
and clears RGB under rounded-zero alpha. Normal RGB/alpha maximum errors are 1,
RMSE 0.046746 / 0.035665; scalar RMSE is 0.052318 / 0.044066. The explicit native
maximum-1 / RMSE-0.25 bounds preserve exact managed rational pixels, source and
geometry. Only the two existing original performance rasters are used.

Windows and Linux each pass **2,833 Release tests**, including **896 live FFmpeg
comparisons**, and the consumer plus all three examples through the **same actual
dependency-free Release package**. Five new word cases cover this direction,
exact admission, held owners, edits and tiny/zero alpha. All 167 focused resize
checks pass in normal/scalar modes on both platforms. Two quiet three-actual
controls check RGBA8 in this direction and the other RGBA16 mixed direction on
the same build; these are regression controls, not new confirmations.

The [numeric evidence](warm-bicubiccross16-rows-evidence.json) retains baseline,
immutable screen/confirmation, regression controls and qualification. Only three
distinct workloads are used. Stop tuning this fixed pair. Wider support, tight
budgets, public boundaries and uniform current-source parity remain open.
