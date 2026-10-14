# Warm RGBA8 bicubic height reduction with unchanged width

The later [shared identity-filter round](WARM-BICUBIC-VERTICAL16-IDENTITY.md)
supersedes this timing with **1.331× / upper 1.345×**, **761.11 µs / 648,992 B/op**
on the same final build as the word confirmation. It cuts this confirmed mean
31.3% while preserving every original byte. The original admission evidence below
is retained.

Independent quiet confirmation measures **1.941× FFmpeg's scaler backend**,
conservative upper **1.956×**, at **1,107.44 µs** and **648,991 B/op**. All twelve
actuals are retained, eleven selected, and both guards pass. The 5,705.04-µs /
10.018× direct baseline falls **80.6%**. Allocations rise 332,685 B/op from
316,306 B/op, about 105%, for optional operation-local workspace of 383,852 bytes.

One admission change routes RGBA8 height reduction with unchanged width through
the existing sixteen-row/tap cache. The same original 513×259 raster becomes
513×129. Arithmetic, conservative rounding guards, work charges, cancellation,
memory admission and direct fallback are unchanged. Other directions and depths
remain open. No state or output is cached between calls.

The frozen qualified original output is checked **byte for byte**. Its twenty-five
alpha differences from the independent integer reference are each certified exact
rational halves by development-only `BigInteger` arithmetic. These retain the
binary64 rounding already permitted by SPEC.md; no managed tolerance or pixel
change is accepted. RGB and all other samples match the rational reference.
The pinned original output hash is checked in setup and receipt validation.

The native baseline is explicitly **zimg, FFmpeg's zscale backend**, through the
shared original development adapter. Complete source conversion, four-pixel
clamped halo, premultiplied float filtering, independent planar/packed allocation,
unrounded-alpha unpremultiplication, straight byte quantization, rounded-zero-alpha
RGB clearing and release are timed. Padding adds 4.697% input area. Both modes
satisfy the existing native maximum-1 / RMSE-0.25 bounds: normal RGB/alpha RMSE
0.002244 / 0.019044, scalar 0 / 0.019821. Source, geometry, independent owners,
operation counters and zero-alpha RGB checks pass. This is the labeled scaler
backend, not FFmpeg CLI.

Screen and independent confirmation use the same immutable production/harness.
One relevant quiet three-actual control checks the shared horizontal path at **1.512×** before
acceptance qualification. Only two distinct workloads and the existing original
byte raster are used; the retained original raster count remains two. Prepared
children and the verified harness are reused; only changed production inputs are
rebuilt. The admission gap requires no additional profile.

Windows and Linux each pass **2,882 Release tests**, including **896 live FFmpeg
comparisons**, plus the consumer and all three examples through the **same actual
dependency-free Release package**. Seven new public cases exercise admission,
odd widths, cache budgets, source edits, held owners, negative lobes and tiny/zero
alpha. All 216 focused resize checks pass in normal/scalar modes on both platforms.
Final qualification runs once per platform for the accepted production change.

The [numeric evidence](warm-bicubicvertical8-rows-evidence.json) retains baseline,
screen, confirmation, regression control and qualification. Stop tuning this fixed
pair. Other single-axis directions, tight budgets, larger supports and remaining
required/current-source parity stay open.
