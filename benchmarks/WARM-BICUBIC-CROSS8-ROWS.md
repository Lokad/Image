# Warm RGBA8 bicubic width reduction and height enlargement

Independent quiet confirmation measures **1.190× FFmpeg's scaler backend**,
conservative upper **1.206×**, at **984.18 µs** and **731,753 B/op**. Both guards
pass; all twelve actuals are retained, with eleven selected results. The
18,067.36-µs / 21.910× direct-evaluation baseline falls **94.6%**.

The existing original 513×259 RGBA8 raster becomes 256×519. One admission change
routes this direction through the existing checked, operation-local row cache
and byte writer. Horizontal eight/nine-tap filtering is reused across overlapping
vertical output rows. Arithmetic, accumulation order, precision guards,
original-order/tiny-alpha fallback, work charges and cancellation stay intact.
The four-tap shared-input optimization does not apply to this wider support.
No approximate reciprocal or managed pixel tolerance is added.

Optional workspace is **716×output width + 32×source width + 128 +
512×(output width & 1) bytes**, 199,840 bytes here. Allocations rise 200,154 B/op,
about 37.7%. Tight admission budgets retain direct evaluation with identical
pixels; odd widths remain padded for checked two-pixel indexing. Source edits
and retained independent owners pass. No state or output is cached between calls.

The explicitly labeled native baseline is **zimg, FFmpeg's zscale backend**.
The existing original adapter selects this geometry at compilation; no new
adapter implementation or original raster is introduced. The four-pixel clamped
halo, complete source conversion, premultiplied float filtering, independent
planar/packed output allocation and release are timed. Packing unpremultiplies
using unrounded alpha, quantizes straight RGBA and clears RGB under rounded-zero
alpha. Normal RGB/alpha maximum errors are 1, RMSE 0.002743 / 0.004752; scalar
RMSE is 0.003542 / 0.004752. The explicit native maximum-1 / RMSE-0.25 gates
preserve exact managed rational pixels, source and geometry.

The first candidate attempt fails its pre-run idle guard with 1.35% I/O wait;
no actual iteration starts. The failure is retained. Flushing preparation writes
outside timing permits the same immutable library/harness to complete its quiet
screen and confirmation without rebuilding or a historical-load cooldown.

Windows and Linux each pass **2,828 Release tests**, including **896 live FFmpeg
comparisons**, and the consumer plus all three examples through the **same actual
dependency-free Release package**. Five new public cases cover this direction,
exact admission, held owners, edits and tiny/zero alpha. All 162 focused resize
checks pass in normal/scalar modes on both platforms. Two quiet three-actual
controls check the previously confirmed RGBA8/16 mixed paths on the same build;
they are regression controls, not new independent confirmations.

The [numeric evidence](warm-bicubiccross8-rows-evidence.json) retains baseline,
failed guard, immutable screen/confirmation, regression controls and qualification.
Only three distinct workloads and the two existing original performance rasters
are used. Stop tuning this fixed pair. RGBA16 in this direction, wider support,
public boundaries and uniform current-source parity remain open.
