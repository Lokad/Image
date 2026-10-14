# Warm RGBA8 bicubic width enlargement with unchanged height

Independent quiet confirmation measures **1.727× FFmpeg's scaler backend**,
conservative upper **1.742×**, at **2,594.78 µs / 1,930,602 B/op**. All twelve
actuals are retained, eleven selected, and both guards pass. The 3,734.66-µs /
2.490× direct baseline falls **30.5%**. Allocations rise 649,433 B/op from
1,281,169 B/op, about 51%, for optional operation-local workspace of 751,876 bytes.

The same original 513×259 RGBA8 raster becomes 1027×259. Admission alone screens
below two, but its independent repeat has an upper of **2.042×**, so that build
receives no full qualification. One substantive extension then reuses the
existing adjacent four-tap kernel: interior output pairs load five source vectors
instead of eight. Each output keeps its original tap and multiply/add order.
Precision guards, work charges, cancellation, memory admission and original/
tiny-alpha fallback remain unchanged. Larger supports, tight budgets and
unsupported hardware retain direct evaluation. No state is cached between calls.

Every managed sample matches the independent integer rational reference exactly;
there is no half-rounding exception for this raster. Seven new public cases cover
odd widths, admission budgets, source edits, held owners, negative lobes and
tiny/zero alpha. All 230 focused checks pass in normal/scalar modes on both OSes.

The native baseline is explicitly **zimg, FFmpeg's zscale backend**, through the
shared original adapter. Complete conversion, four-pixel clamped halo,
premultiplied float filtering, independent output allocation, unrounded-alpha
unpremultiplication, straight byte quantization, rounded-zero-alpha RGB clearing
and release are timed. Padding adds 4.697% input area. Both native modes satisfy
the existing maximum-1 / RMSE-0.25 bounds: RGB RMSE 0.002962 normal and 0.003878
scalar; alpha is exact. Source, geometry, independent owners, operation counters
and zero-alpha RGB controls pass. This is the labeled backend, not FFmpeg CLI.

Accepted screen and confirmation reuse the same immutable production/harness.
One existing horizontal-reduction regression screen passes on that build before
acceptance qualification. Only two workloads and one of the two existing original
rasters are used. No additional profile is needed. Three pre-run I/O guard
refusals start no benchmark child and remain retained, unscored; guard thresholds
are unchanged. The earlier admission-only repeat remains superseded evidence.

Windows and Linux each pass **2,896 Release tests**, including **896 live FFmpeg
comparisons**, plus the consumer and all three examples through the **same actual
dependency-free Release package**. Final qualification runs once per platform.

The [numeric evidence](warm-bicubichorizontalup8-rows-evidence.json) retains the
baseline, superseded admission trial, immutable accepted screen/confirmation,
regression control, guard refusals and qualification. Stop tuning this pair.
Other single-axis directions/depths, tight budgets, larger supports and required
current-source uniform parity remain open.
