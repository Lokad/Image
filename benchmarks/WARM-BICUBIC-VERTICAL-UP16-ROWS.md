# Warm RGBA16 bicubic height enlargement with unchanged width

Independent quiet confirmation measures **1.288× FFmpeg's scaler backend**,
conservative upper **1.297×**, at **2,131.66 µs / 2,516,605 B/op**. All twelve
actuals are retained, eleven selected, and both guards pass. The 4,561.52-µs /
2.743× direct baseline falls **53.3%**. Allocations rise 332,201 B/op from
2,184,404 B/op, about 15%, for optional operation-local workspace of 383,852 bytes.

The same original 513×259 RGBA16 raster becomes 513×519. One admission change
uses the existing checked word-row cache and unchanged-width identity copy.
Integer horizontal centers have exact weights **0,0,1,0,0**; copying exact
unsigned products preserves every unrounded intermediate. Tap order, precision
guards, work charges, cancellation, memory admission and original/tiny-alpha
fallback remain unchanged. Larger supports, tight budgets and unsupported
hardware retain the previous path. No state is cached between calls.

Every managed sample matches the independent rational reference exactly. Seven
new word cases cover odd widths, admission boundaries, source edits, held owners
and negative/tiny/zero alpha. All 255 focused checks pass in normal/scalar modes
on Windows and Linux before candidate timing.

The native baseline is explicitly **zimg, FFmpeg's zscale backend**, with complete
source conversion, timed four-pixel clamped halo, premultiplied float filtering,
independent output allocation, unrounded-alpha unpremultiplication, straight word
quantization, rounded-zero-alpha RGB clearing and release. Padding adds 4.697%
input area. This case has an explicitly approved native-only adjustment:
**RGB error ≤2 only where reference alpha ≤16; RGB error ≤1 everywhere else**.
RGB/alpha RMSE remain ≤0.25 and alpha error remains ≤1. Both native modes differ
by two samples in two RGB channels at one alpha-12 pixel; normal RGB/alpha RMSE
are 0.043364 / 0.032775, scalar 0.046405 / 0.035152. Source, geometry, independent
owners, operation counters and zero-alpha RGB checks pass. Negative controls
reject RGB error 3 at low alpha and error 2 above alpha 16. Managed pixels have
no tolerance. This is the labeled backend, not FFmpeg CLI.

The initial maximum-1 native refusal remains retained and unscored. A diagnostic
with more precise premultiplied float input still exceeds that bound and is
rejected; its altered conversion is not the measured baseline. The approved
exception changes only comparison validation for this fixed word geometry,
preserving complete timed work and the original native pixels.

Screen and repeat reuse the same immutable production/harness. One existing byte
vertical-enlargement regression screen passes on that build before qualification;
its original pixels and strict native maximum-1 bound remain unchanged. Prepared
children are reused, and the byte worker rebuilds once because shared validation
source changes. Two workloads use the two existing original rasters. No profile
is needed; production rebuilds occur only after its admission change.

Windows and Linux each pass **2,921 Release tests**, including **896 live FFmpeg
comparisons**, plus the consumer and all three examples through the **same actual
dependency-free Release package**. Final qualification runs once per platform.

The [numeric evidence](warm-bicubicverticalup16-rows-evidence.json) retains the
refusal, rejected diagnostic, qualified baseline, immutable screen/confirmation,
regression control and qualification. Stop tuning this pair. Wider word support,
tight budgets and remaining material required/current-source uniform parity stay
open.
