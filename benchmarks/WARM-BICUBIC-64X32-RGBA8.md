# Warm RGBA8 bicubic support beyond thirty-two taps

Independent quiet confirmation measures **1.811× FFmpeg's scaler backend**,
conservative upper **1.820×**, at **637.23 µs** and **205,880 B/op**. All twelve
actuals are retained, eleven selected, and both guards pass. The 8,673.94-µs /
24.783× direct baseline falls **92.7%**. Allocations rise from 8,320 B/op by
197,560 B/op for optional operation-local workspace of 197,920 bytes.

The same original 513×259 RGBA8 raster becomes 64×32. Support exceeds the old
thirty-two-tap admission. One primitive extends the byte cache to sixty-four
operation-local rows/taps; word support remains thirty-two. Workspace is
**2828×output width + 32×source width + 512 bytes**, retaining 4 KiB for exact
low-alpha handling. Larger support, tight budgets and unsupported hardware keep
direct evaluation. No state or output is cached between calls.

The first cache screen is 2.139×. One six-second quiet profile, 5,617 samples,
places 36.8% of CPU time in original-order fallback. Vectorizing that fallback
gives only a small gain, remains 2.080×, and is rejected. Neither above-two
candidate receives twelve samples or full qualification.

The accepted cache preserves every guard and decides uncertainty per pixel,
so its certain neighbor avoids redundant two-dimensional evaluation. Byte
alpha/color margins are **1e-4 / 4e-3**, based on the conservative
2^-40×4096 per-unit absolute-error bound, normalization at least 0.5 and fast
alpha at least one. Original order and exact tiny-alpha handling remain active
where required. Every managed sample matches the independent rational reference;
no pixel tolerance or approximate reciprocal is added. Existing thirty-two-tap
byte/word margins and sixteen-tap kernels stay unchanged.

The native baseline is explicitly **zimg, FFmpeg's zscale backend**, through the
shared original development adapter. Its timed sixteen-pixel clamped halo adds
**19.364% input area**. Complete source conversion, premultiplied float filtering,
independent planar/packed allocation, unrounded-alpha unpremultiplication,
straight byte quantization, rounded-zero-alpha RGB clearing and release are timed.
Both native modes match all rational pixels exactly under the existing unchanged
maximum-1 / RMSE-0.25 bounds. Source, geometry, independent owners and counters
pass. This is the labeled scaler backend, not FFmpeg CLI.

Screen and independent confirmation use the same immutable production/harness.
Two quiet three-actual controls check the existing wider byte and word paths
at **1.695× / 1.487×** before acceptance qualification. Only three distinct workloads and the two
existing original rasters are used. Prepared children and the verified harness
are reused; production rebuilds occur only after production changes.

Windows and Linux each pass **2,868 Release tests**, including **896 live FFmpeg
comparisons**, plus the consumer and all three examples through the **same actual
dependency-free Release package**. Seven new public cases exercise larger
support, odd widths, memory admission, held owners, edits, negative lobes,
tiny/zero alpha and support beyond sixty-four taps. All 202 focused resize checks
pass in normal/scalar modes on both platforms. Final qualification runs once per
platform for the accepted production change.

The [numeric evidence](warm-bicubic64x32-8-rows-evidence.json) retains all screens,
the rejected fallback, profile, confirmation, regression controls and qualification.
Stop tuning this fixed pair. Wider word support, other single-axis directions,
tight budgets and remaining required/current-source parity stay open.
