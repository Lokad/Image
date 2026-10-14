# Warm RGBA8 reverse mixed-axis bicubic with a bounded row cache

Independent quiet confirmation measures **1.504× FFmpeg's scaler backend**,
conservative upper **1.526×**, at **1,268.02 µs / 531,910 B/op**. All twelve
actuals are retained, eleven selected; both guards pass. The direct baseline was
18,083.73 µs / 21.743×: the accepted change reduces its mean **93.0%**.

The existing original 513×259 RGBA8 raster becomes 256×519 with a working
limit of **1,067,020 bytes: input + output + 4 KiB**, excluding the full row
cache. One separate horizontal-first primitive reuses five filtered source rows
for eight output columns while enlarging height. At most ten horizontal and
five vertical taps are supported; 32 decoded source-column slots and the other
fixed arrays occupy 3,440 bytes within the admitted workspace.

Exact products up to 65,025 and unrounded binary64 preserve every frozen sample
and match the independent rational reference exactly. Conservative
`2^-40*16^2` guards use alpha margin 1e-6 and color margin
`1e-4 / lower-power-of-two(alphaSum)`. Uncertainty replays original tap order.
Tiny alpha uses a private deferred marker, then a noninlined frame unwind and
timed original-color/exact-alpha replay with additional work and full scratch.
No marker escapes. Admission, cancellation, source and independent owner
contracts remain. No heap workspace, persistent state or output cache is added.
Existing word, enlargement and vertical-first kernels remain unchanged; other
geometry, supports, hardware and budgets retain prior paths.

The ordinary-memory control retains the existing full row cache and passes at
**1.165×**, allocating 731,753 B/op. Its three-sample upper is 1.226×; this is
a regression screen, not another independent certification.

The baseline is **zimg, FFmpeg's zscale backend**, with timed four-pixel clamped
padding (4.697% additional input area), complete source conversion, premultiplied
float filtering, independent output allocation, unrounded-alpha unpremultiplication,
quantization, rounded-zero-alpha RGB clearing and release. Normal/scalar native
RGB/alpha maximum-one and RMSE-0.25 checks pass. This is the labeled backend,
not FFmpeg CLI; native memory is neither constrained nor measured.

Screen, independent repeat and control reuse identical frozen production and
benchmark assemblies. Correct generated children are restored and checked
before timing. Only changed inputs build; cached restore and prepared children
are reused. No new profile or original raster was needed. Baseline allocation
is 531,599 B/op; raw figures include harness overhead.

Windows and Linux each pass **3,088 Release tests**, including **896 live FFmpeg
comparisons**, plus **437 focused checks** in normal/scalar modes. Nine new cases
cover frozen products, admission/work, tails, negative/tiny alpha, exact half
rounding, transparent RGB clearing, cancellation, source edits and retained
owners. The consumer and all three examples pass through the **same actual
dependency-free Release package** on both OSes. Full qualification runs once
per platform for the production change.

The [numeric evidence](warm-bicubictightcross8-rows-evidence.json) retains baseline,
screen, confirmation, control and qualification. Stop this fixed pair; larger
supports, other material boundaries and final current-source uniform parity
remain open.
