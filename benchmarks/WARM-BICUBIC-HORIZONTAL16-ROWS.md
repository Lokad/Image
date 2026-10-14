# Warm RGBA16 bicubic width reduction with unchanged height

Independent quiet confirmation measures **1.837× FFmpeg's scaler backend**,
conservative upper **1.853×**, at **1,040.82 µs** and **730,683 B/op**. All twelve
actuals are retained, ten are selected, and both guards pass. The 4,130.23-µs /
7.286× direct baseline falls **74.8%**. Allocations rise 199,941 B/op from
530,742 B/op, about 38%, for optional operation-local workspace of 199,840 bytes.

One admission change extends the existing sixteen-row/tap cache's unchanged-height
width reduction to words. The same original 513×259 RGBA16 raster becomes 256×259.
Unsigned source products, word packing, binary64 filtering, precision guards,
work charges, cancellation and direct fallback are unchanged. The screen measures
889.25 µs / 1.570×; the independent repeat is slower and supplies the published
result and conservative bound. Both use the same immutable production/harness.
No state or output is cached between calls.

Every managed sample matches the independent integer rational reference exactly.
The measured word raster has no half-rounding exception. Seven new public cases
cover admission, odd widths, tight workspace boundaries, source edits, held owners,
negative lobes and tiny/zero alpha. All 195 focused resize checks pass in normal
and scalar modes on both platforms before timing.

The native baseline is explicitly **zimg, FFmpeg's zscale backend**, through the
shared original development adapter with depth selected at compile time. Complete
source conversion, four-pixel clamped halo, premultiplied float filtering,
independent planar/packed allocation, unrounded-alpha unpremultiplication, straight
word quantization, rounded-zero-alpha RGB clearing and release are timed. Padding
adds 4.697% input area. Both modes satisfy the existing native maximum-1 /
RMSE-0.25 bounds: normal RGB/alpha RMSE 0.040484 / 0.033633, scalar
0.044731 / 0.038836. Source, geometry, independent owners, operation counters and
zero-alpha RGB checks pass. This is the labeled scaler backend, not FFmpeg CLI.

One relevant quiet three-actual control checks the existing RGBA8 horizontal path
at **1.520×** on the same build before acceptance qualification. Only two distinct workloads
and the two existing original rasters are used. The generated short child and
verified harness are reused; only the changed production DLL is replaced.

Windows and Linux each pass **2,861 Release tests**, including **896 live FFmpeg
comparisons**, plus the consumer and all three examples through the **same actual
dependency-free Release package**. Final qualification runs once per platform for
the accepted production change.

The [numeric evidence](warm-bicubichorizontal16-rows-evidence.json) retains the
baseline, immutable screen/confirmation, regression control and qualification.
Stop tuning this fixed pair. Other single-axis directions, tight budgets and
remaining material required/current-source parity stay open.
