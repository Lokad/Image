# Warm RGBA16 mixed-axis bicubic with bounded vertical-first filtering

Independent quiet confirmation measures **1.954× FFmpeg's scaler backend**,
conservative upper **1.980×**, at **1,642.70 µs / 1,060,419 B/op**. All twelve
actuals are retained, eleven selected; both guards pass. The direct baseline was
19,953.04 µs / 23.799×: the accepted change reduces its mean **91.8%**.

The existing original 513×259 RGBA16 raster becomes 1027×129 with a working
limit of **2,126,896 bytes: input + output + 4 KiB**, excluding the full row
cache. One bounded primitive vertically filters source columns first, then
reuses them while enlarging width. Four output rows reuse five source-column
slots; 2,016 bytes of fixed arrays fit the admitted 4-KiB workspace. This lowers
the substantial filtering work and avoids repeating vertical geometry per strip.
It is preferred for the supported geometry even when the full row cache would fit.

Exact full-range UInt32 products and unrounded binary64 intermediates retain
every frozen sample, also matching the independent rational reference exactly.
The conservative `2^-40*16^2` word bounds use alpha margin 1e-4 and color margin
`2.0001 / lower-power-of-two(alphaSum)`, removing a second vector division.
Normalizer/alpha/quarter-margin guards remain. Uncertainty replays the original
horizontal-within-vertical tap order; tiny alpha unwinds before exact scratch.
Paired packing reuses decoded storage before separate checked stores to two rows.
Complete work, bounded cancellation, source and owner contracts remain. No heap
workspace, persistent state or output cache is introduced. Other directions,
supports, hardware and smaller budgets retain prior paths.

The baseline is **zimg, FFmpeg's zscale backend**, with timed four-pixel clamped
padding (4.697% additional input area), complete source conversion, premultiplied
float filtering, independent output allocation, unrounded-alpha unpremultiplication,
word quantization, rounded-zero-alpha RGB clearing and release. Normal/scalar
native maximum-one RGB/alpha and RMSE-0.25 bounds pass. This is the labeled
backend, not FFmpeg CLI; native memory is neither constrained nor measured.

The first six-column screen remained at 3.037×. One six-second CPU profile
identified filtering, repeated geometry and quotient checks. A wider horizontal
pair trial regressed to 3.799× and was rejected without confirmation or full
qualification. Reversing the passes made the substantial next improvement.
Intermediate default-memory controls above two held qualification; the final
preferred path and conservative-bound check restore its mean gate at **1.934×**.
That control allocates 1,060,419 B/op versus the earlier 1,813,110 B/op and has
three-sample upper 2.143×: it is a screen, not a new independent certification.
All intermediate actuals and failed selected-child receipts remain retained.

Screen, independent repeat and final control reuse identical frozen production
and benchmark assemblies. Declared geometry/depth/budget and the actual selected
child are checked before promotion. Only changed inputs build; prepared children
and cached restore are reused. There is no additional original performance raster.
Baseline allocation is 1,060,009 B/op; raw figures include harness overhead.

Windows and Linux each pass **3,062 Release tests**, including **896 live FFmpeg
comparisons**, plus **411 focused checks** in normal/scalar modes. Eight new
cases cover frozen pixels, full unsigned products, admission/work, direction
guards, tails, negative/tiny alpha, cancellation, source edits and retained owners.
The consumer and all three examples pass through the **same actual dependency-free
Release package** on both OSes. Full qualification runs once per platform for
the final production change.

The [numeric evidence](warm-bicubictightmixed16-vertical-first-evidence.json)
retains the baseline, rejected/intermediate trials, final confirmation/control,
profile and qualification. Stop this pair. Other depths/directions, public
boundaries and final current-source uniform parity stay open.
