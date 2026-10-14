# Warm RGBA8 mixed-axis bicubic with bounded vertical-first filtering

Independent quiet confirmation measures **1.897× FFmpeg's scaler backend**,
conservative upper **1.907×**, at **1,636.25 µs / 530,370 B/op**. All twelve
actuals are retained and selected; both guards pass. The direct baseline was
20,053.40 µs / 23.071×: the accepted change reduces its mean **91.8%**.

The existing original 513×259 RGBA8 raster becomes 1027×129 with a working
limit of **1,065,496 bytes: input + output + 4 KiB**, excluding the full row
cache. One separate byte primitive vertically filters source columns first,
reusing five columns for four output rows while enlarging width. Its 2,016
bytes of fixed arrays fit the admitted workspace. The path is preferred for
supported geometry even when the full row cache would fit; the ordinary-memory
regression screen passes at **1.867×**, upper 1.902×, with the same allocation.
That three-sample screen is not another independent certification.

Exact products up to 65,025 and unrounded binary64 intermediates preserve every
frozen sample and match the independent rational reference exactly. Conservative
`2^-40*16^2` bounds use alpha margin 1e-6 and color margin
`1e-4 / lower-power-of-two(alphaSum)`, with normalizer/alpha/quarter-margin
guards. Uncertainty replays the original horizontal-within-vertical tap order.
Tiny-alpha pixels are marked only in private unfinished output; the noninlined
frame releases its workspace before a timed final scan replays original color
and exact rational alpha, including additional work. No marker reaches callers.
Admission, cancellation, source and independent owner contracts remain. No heap
workspace, persistent state or output cache is introduced. Other directions,
supports, hardware and smaller budgets retain prior paths.

The baseline is **zimg, FFmpeg's zscale backend**, with timed four-pixel clamped
padding (4.697% additional input area), complete source conversion, premultiplied
float filtering, independent output allocation, unrounded-alpha unpremultiplication,
quantization, rounded-zero-alpha RGB clearing and release. Normal/scalar native
RGB/alpha maximum-one and RMSE-0.25 checks pass. This is the labeled backend,
not FFmpeg CLI; native memory is neither constrained nor measured.

The screen, independent repeat and control reuse identical frozen production
and benchmark assemblies. Correct generated children are restored and checked
before timing. Only changed inputs build; cached restore and prepared children
are reused. No new profile or original raster was needed. Baseline allocation
is 530,081 B/op; raw figures include harness overhead.

Windows and Linux each pass **3,071 Release tests**, including **896 live FFmpeg
comparisons**. Focused normal/scalar checks cover 420 cases per OS: Windows
retains its original 419-case run plus nine targeted checks after adding the
tiny-alpha case. Nine new public cases cover frozen pixels, products, admission,
work, tails, negative/tiny alpha, exact half rounding, transparent RGB clearing,
cancellation, source edits and retained owners. The consumer and all three
examples pass through the **same actual dependency-free Release package** on
both OSes. Full qualification runs once per platform for the production change.

The [numeric evidence](warm-bicubictightmixed8-vertical-first-evidence.json)
retains the baseline, screen, confirmation, control and qualification. Stop this
fixed pair; other directions, public boundaries and final current-source uniform
parity remain open.
