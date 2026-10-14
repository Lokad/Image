# Warm RGBA8 bicubic height enlargement with unchanged width

Independent quiet confirmation measures **1.279× FFmpeg's scaler backend**,
conservative upper **1.286×**, at **2,213.62 µs / 1,664,240 B/op**. All twelve
actuals are retained and selected; both guards pass. The 5,367.64-µs / 3.108×
direct baseline falls **58.8%**. Allocations rise 332,515 B/op from 1,331,725 B/op,
about 25%, for optional operation-local workspace of 383,852 bytes.

The same original 513×259 RGBA8 raster becomes 513×519. One admission extension
uses the existing sixteen-row cache and unchanged-width identity copy. Integer
horizontal centers have exact weights **0,0,1,0,0**; copying decoded products
preserves every unrounded intermediate. Cancellation checks every two copied
pixels, work charges, precision guards, memory admission and original/tiny-alpha
fallback stay unchanged. Larger supports, tight budgets and unsupported hardware
retain direct evaluation. No output or state is cached between calls.

Every managed byte matches the frozen qualified direct output. Its one difference
from the rational reference is blue byte 1,031,850: the exact rational value is
24.5, while original binary64 accumulation rounds to 24. Independent integer
premultiplied-color arithmetic certifies that exact half, preserving SPEC.md's
existing convention. There is no new managed pixel tolerance. The reference
certificate has four byte/word positive and negative controls. Seven new public
cases cover odd widths, cache budgets, source edits, held owners, negative lobes
and tiny/zero alpha; all 241 focused checks pass in normal/scalar modes on both
platforms before candidate timing.

The native baseline is explicitly **zimg, FFmpeg's zscale backend**, through the
existing shared original adapter. Complete conversion, timed four-pixel clamped
halo, premultiplied float filtering, independent output allocation, unrounded-alpha
unpremultiplication, straight byte quantization, rounded-zero-alpha RGB clearing
and release are timed. Padding adds 4.697% input area. Both modes satisfy the
unchanged native maximum-1 / RMSE-0.25 bounds: normal/scalar RGB RMSE
0.003165 / 0.003538, alpha RMSE 0.003357 in both. Source, geometry, independent
owners, operation counters and zero-alpha RGB checks pass. This is the labeled
backend, not FFmpeg CLI.

Screen and repeat use the same immutable production/harness. One existing
height-reduction regression screen passes on that build before full qualification.
Two workloads use one of the two existing original rasters. Prepared children and
harness are reused; only changed production inputs rebuild. No profile is needed.
Initial rational-reference and receipt-contract preparation failures remain
retained, unscored; dependent timing starts only after exact-half certification
and the scoped receipt checks pass.

Windows and Linux each pass **2,907 Release tests**, including **896 live FFmpeg
comparisons**, plus the consumer and all three examples through the **same actual
dependency-free Release package**. Final qualification runs once per platform.

The [numeric evidence](warm-bicubicverticalup8-rows-evidence.json) retains the
baseline, immutable accepted screen/confirmation, regression control, original
pixel certificate, preparation failures and qualification. Stop tuning this pair.
Word enlargement, larger supports, tight budgets and remaining material required/
current-source uniform parity remain open.
