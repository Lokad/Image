# Warm RGBA16 tight-memory bicubic with packed coefficients

Independent quiet confirmation measures **0.511× FFmpeg's scaler backend**,
conservative upper **0.518×**, at **637.19 µs / 185 B/op**. All twelve actuals
are retained and selected; both guards pass. The direct baseline was
8,357.28 µs / 6.737×: the accepted change reduces its mean **92.4%**.

The existing original 513×259 RGBA16 raster becomes 4×2 with a working limit
of **1,067,096 bytes: input + output + 4 KiB**, excluding the full row cache.
Horizontal/vertical admission bounds are 514/519 taps. One separate streaming
primitive supports up to 544 taps per axis and eight output rows. Coefficients
use six bytes each; all fixed arrays occupy **3,776 bytes** within the admitted
workspace. Each source row is filtered once per output column; only its last
horizontal result is stored, with separate original clamped-edge contributions.

Each binary64 coefficient retains its sign, exponent and 36 fraction bits.
The discarded absolute error is measured against the original value. Full
unsigned products and accumulators remain unrounded binary64. With original
coefficient L1 sums `Lx/Ly` and measured horizontal loss `Qx`, the unit bound is
`2^-40*544^2*max(1,Lx*Ly/1024)+2*Qx*Ly`. Its arithmetic term provides at least
`8*2^-53*544^2*Lx*Ly`, conservatively covering both evaluation orders and
normalizers; measured storage loss is doubled. Quotient/quarter-margin guards
force original full-coefficient tap-order replay whenever rounding is uncertain.
Tiny alpha unwinds before exact scratch. Frozen and independent rational pixels
remain exact; a separate test certifies rational halves and preserves original
rounding there. Work, cancellation, checked loads, absolute coordinates, source
and owner contracts remain. No heap/full-support/output cache is added.
Other supports, heights, directions, hardware and budgets retain prior paths.

The existing tight 8×4 word regression mean passes at **0.638×**, allocating
377 B/op. Its three-sample upper 0.801× is a screen, not another certification.

The baseline is **zimg, FFmpeg's zscale backend**, with **256-pixel clamped
padding (494.787% additional input area)**. Padding is timed with complete
conversion, premultiplied float filtering, independent allocation, unrounded-alpha
unpremultiplication, word quantization, zero-alpha RGB clearing and release.
Normal/scalar maximum-one RGB/alpha and RMSE-0.25 gates and native negative
controls pass. This is the labeled backend, not FFmpeg CLI; native memory
is neither constrained nor measured.

Screen, independent repeat and regression reuse identical frozen production
and benchmark assemblies. The added half-rounding test builds without rebuilding
production. Only changed inputs build; cached restore and verified prepared
children are reused. No new profile or original performance raster was needed.
Baseline allocation is 192 B/op; raw figures include harness overhead.

Windows and Linux each pass **3,116 Release tests**, including **896 live FFmpeg
comparisons**. Focused normal/scalar checks cover 465 unique cases per OS: the
original 464-case run plus the added half test. Ten new cases cover frozen pixels,
support boundaries, admission/work, negative/tiny alpha, scratch unwind,
certified halves, cancellation, source edits and retained owners. Consumer and
all three examples pass through the **same actual dependency-free Release
package** on both OSes. Qualification runs once per platform after passing means.

The [numeric evidence](warm-bicubictight4x2word-packed-evidence.json) retains
baseline, screen, confirmation, regression and qualification. Stop this pair;
byte support, larger/unmatched work and final current-source uniform parity
remain open.
