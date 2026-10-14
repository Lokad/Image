# Warm RGBA8 mixed-axis bicubic resize

Independent quiet confirmation measures **1.945× FFmpeg's scaler backend**,
conservative upper **1.978×**, at **1,692.10 µs** and **1,283,063 B/op**. Both
guards pass; all twelve actuals are retained and selected. The 17,374.03-µs /
19.938× baseline falls **90.3%**. Stop tuning this fixed pair.

The existing original 513×259 RGBA8 raster becomes 1027×129: width grows while
height shrinks. The previous cache guard routed this geometry to direct
evaluation. Admitting horizontal row reuse first reduces time to 2,251.04 µs /
2.623×, which receives no twelve-actual repetition. One six-second profile shows
95.8% in the cache kernel, with repeated span checks and temporary vector loads
prominent. Padding odd cache-row widths and directly indexing two-pixel vectors
cuts another 24.6% from that screen. Arithmetic, division, precision margins,
original-order/tiny-alpha fallback, cancellation and work charges are preserved.
All accesses remain checked managed operations.

Optional workspace is **716×output width + 32×source width + 128 +
512×(output width & 1) bytes** for this direction, 752,388 bytes here. The padded
pixel costs 512 bytes across sixteen rows. Allocations rise 650,208 B/op, about
102.7%, against direct evaluation. Tight admission budgets retain direct
evaluation with identical pixels. No state or output is cached between calls.

Managed output stays exact against the independent rational reference. The
native baseline is explicitly **zimg, FFmpeg's zscale backend**, with the same
timed four-pixel clamped halo, full source conversion, premultiplied float
filtering, independent planar/packed output allocation and release. Packing
unpremultiplies using unrounded alpha, then quantizes straight RGBA and clears
RGB under rounded-zero alpha. Normal RGB/alpha maximum errors are 1, RMSE
0.003547 / 0.002747; scalar is bounded too. The explicit native maximum-1 /
RMSE-0.25 gates remain unchanged. Source, geometry and independent owners pass.

Windows and Linux each pass **2,818 Release tests**, including **896 live FFmpeg
comparisons**, and the consumer plus all three examples through the **same actual
dependency-free Release package**. Five new public cases cover mixed geometry,
exact admission, held owners, edits and tiny/zero alpha. All 152 focused resize
checks pass in normal/scalar modes on both platforms. Quiet three-actual
regression screens retain RGBA16 upscale at 1.490× and RGBA8 downsample at
1.736× on the same immutable production/harness build; they are regression
controls, not new independent confirmations.

The [numeric evidence](warm-bicubicmixed8-rows-evidence.json) retains the baseline,
superseded row-only candidate, accepted screen/confirmation, profile, regression
controls and qualification. Two original performance rasters remain sufficient.
Other mixed directions/depths, wider support, public boundaries and uniform
current-source parity remain open.
