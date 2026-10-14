# Warm RGBA16 bicubic height reduction with unchanged width

Independent quiet confirmation measures **1.441× FFmpeg's scaler backend**,
conservative upper **1.480×**, at **832.27 µs** and **913,737 B/op**. All twelve
actuals are retained, eleven selected, and both guards pass. The 5,823.11-µs /
10.099× direct baseline falls **85.7%**. Allocations rise 332,635 B/op from
581,102 B/op, about 57%, for optional operation-local workspace of 383,852 bytes.

The same original 513×259 RGBA16 raster becomes 513×129. Word admission alone
measures 2.046× and receives no twelve-sample/full qualification. One substantive
cache optimization removes redundant horizontal filtering at unchanged width:
integer centers have exact weights **0,0,1,0,0**, so copying decoded products
preserves every unrounded intermediate. Copying checks cancellation every two
pixels; row work charges, unsigned products, precision guards, memory admission
and original/tiny-alpha fallback stay unchanged. Larger supports, tight budgets
and unsupported hardware retain direct evaluation. No output or state is cached
between calls.

Every managed sample matches the independent integer rational reference exactly.
The word raster has no half-rounding exception. Seven new public word cases cover
admission, odd widths, cache budgets, source edits, held owners, negative lobes
and tiny/zero alpha. All 223 focused resize checks pass in normal/scalar modes on
both platforms before timing.

The native baseline is explicitly **zimg, FFmpeg's zscale backend**, through the
shared original adapter's existing word macro. Complete source conversion,
four-pixel clamped halo, premultiplied float filtering, independent planar/packed
allocation, unrounded-alpha unpremultiplication, straight word quantization,
rounded-zero-alpha RGB clearing and release are timed. Padding adds 4.697% input
area. Both modes satisfy the existing native maximum-1 / RMSE-0.25 bounds:
normal RGB/alpha RMSE 0.046431 / 0.038482, scalar 0.049375 / 0.042405. Source,
geometry, independent owners, operation counters and zero-alpha RGB checks pass.
This is the labeled scaler backend, not FFmpeg CLI.

Accepted screen and confirmation use the same immutable production/harness.
The shared byte path independently confirms **1.331× / upper 1.345×**, at
**761.11 µs / 648,992 B/op**, on that same final build. Eliminating its redundant
horizontal filter reduces its previous confirmed mean by **31.3%**, with stable
allocations and all original pixels preserved, including the twenty-five
certified alpha halves permitted by SPEC.md. Byte screen and repeat use the
already prepared children; no new raster or production/harness rebuild is needed.
Both independent confirmations pass before acceptance qualification. Only the two
existing original rasters and two distinct workloads are used. Prepared children
and verified harness are reused; production rebuilds occur only after production
changes. The identity kernel requires no profile to identify its redundant work.

Windows and Linux each pass **2,889 Release tests**, including **896 live FFmpeg
comparisons**, plus the consumer and all three examples through the **same actual
dependency-free Release package**. Final qualification runs once per platform for
the accepted production change.

The [numeric evidence](warm-bicubicvertical16-rows-evidence.json) retains baseline,
the above-two admission screen, immutable accepted screen/confirmation, shared
byte controls and qualification. Stop tuning this pair. Other single-axis
directions, tight budgets, larger supports and required current-source parity stay
open.
