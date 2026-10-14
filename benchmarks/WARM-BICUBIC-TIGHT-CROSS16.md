# Warm RGBA16 reverse mixed-axis bicubic with a bounded row cache

Independent quiet confirmation measures **1.581× FFmpeg's scaler backend**,
conservative upper **1.587×**, at **1,285.60 µs / 1,063,477 B/op**. All twelve
actuals are retained and selected; both guards pass. The direct baseline was
18,280.83 µs / 22.412×: the accepted change reduces its mean **93.0%**.

The existing original 513×259 RGBA16 raster becomes 256×519 with a working
limit of **2,129,944 bytes: input + output + 4 KiB**, excluding the full row
cache. One separate horizontal-first primitive reuses five filtered source rows
for eight output columns while enlarging height. It supports at most ten
horizontal and five vertical taps; 32 decoded source-column slots and other
fixed arrays occupy 3,440 bytes in the admitted workspace.

Exact full-range unsigned products and unrounded binary64 intermediates preserve
every frozen sample and match the independent rational reference exactly.
Conservative `2^-40*16^2` bounds use alpha margin 1e-4 and color margin
`2.0001 / lower-power-of-two(alphaSum)`, retaining normalizer/alpha/quarter-margin
guards. Uncertainty replays the original horizontal-within-vertical tap order;
tiny alpha unwinds before exact scratch. Complete work, bounded cancellation,
source and independent owner contracts remain. No heap workspace, persistent
state or output cache is introduced. Existing enlargement and vertical-first
kernels remain unchanged; other directions, supports, hardware and smaller
budgets retain prior paths.

The ordinary-memory control retains the existing full row cache and passes at
**1.287×**, allocating 1,263,255 B/op. Its three-sample upper is 1.564×; this is
a regression screen, not another independent certification.

The baseline is **zimg, FFmpeg's zscale backend**, with timed four-pixel clamped
padding (4.697% additional input area), complete source conversion, premultiplied
float filtering, independent output allocation, unrounded-alpha unpremultiplication,
word quantization, rounded-zero-alpha RGB clearing and release. Normal/scalar
native maximum-one RGB/alpha and RMSE-0.25 checks pass. This is the labeled
backend, not FFmpeg CLI; native memory is neither constrained nor measured.

Screen, independent repeat and control reuse identical frozen production and
benchmark assemblies. Correct generated children are restored and checked
before timing. Only changed inputs build; cached restore and prepared children
are reused. No new profile or original raster was needed. Baseline allocation
is 1,063,055 B/op; raw figures include harness overhead.

Windows and Linux each pass **3,079 Release tests**, including **896 live FFmpeg
comparisons**, plus **428 focused checks** in normal/scalar modes. Eight new
cases cover frozen pixels, unsigned products, admission/work, tails, negative
and tiny alpha, cancellation, source edits and retained owners. The consumer
and all three examples pass through the **same actual dependency-free Release
package** on both OSes. Full qualification runs once per platform for the change.

The [numeric evidence](warm-bicubictightcross16-rows-evidence.json) retains
baseline, screen, confirmation, control and qualification. Stop this fixed pair;
the byte direction, other material boundaries and final current-source uniform
parity remain open.
