# Warm RGBA16 tight-memory bicubic beyond 256 taps

Independent quiet confirmation measures **0.635× FFmpeg's scaler backend**,
conservative upper **0.642×**, at **439.90 µs / 377 B/op**. All twelve actuals
are retained, ten selected; both guards pass. The direct baseline was
8,572.92 µs / 12.360×: the accepted change reduces its mean **94.9%**.

The existing original 513×259 RGBA16 raster becomes 8×4 with a working
limit of **1,067,288 bytes: input + output + 4 KiB**, excluding the full row
cache. Horizontal/vertical admission bounds are 258/260 taps. One production
primitive extends the existing word streaming capacity from 256 to **304 taps**;
its fixed arrays grow from 3,456 to **3,936 bytes**, within the same 4-KiB charge.
Each source row is horizontally filtered once per output column and feeds at
most sixteen output accumulators. Separate clamped-edge contributions retain
original order; only the last horizontal result is stored.

Full unsigned products widen exactly to unrounded binary64. The error bound
increases to `2^-40*304^2`; quotient/rounding guards still replay original tap
order for uncertainty. Tiny alpha unwinds before exact scratch. Complete work,
cancellation, checked relative offsets, full absolute coordinates and independent
owner contracts remain. No heap workspace, full support cache or output cache
is added. Other heights, supports, directions, hardware and smaller budgets
retain prior paths. The existing tight 16×8 word regression mean passes at
**0.976×**, with 1,145 B/op; its three-sample upper 1.050× is a screen only.

The baseline is **zimg, FFmpeg's zscale backend**. Its declared **128-pixel
clamped halo adds 198.069% input area** and is timed with complete source
conversion, premultiplied float filtering, independent output allocation,
unrounded-alpha unpremultiplication, word quantization, zero-alpha RGB clearing
and release. Normal/scalar maximum-one RGB/alpha and RMSE-0.25 checks and native
negative controls pass. This is the labeled backend, not FFmpeg CLI; native
memory is neither constrained nor measured. The archived percentage label was
corrected; verified padding dimensions and timed work were unchanged.

Screen, independent repeat and regression reuse the same frozen production
and benchmark assemblies. Only changed inputs build; verified prepared children
and cached restore are reused. A missing case registration failed correctness-only
preparation and remains retained unscored. No new profile or original raster was
needed. Baseline allocation is 384 B/op; raw figures include harness overhead.

Windows and Linux each pass **3,097 Release tests**, including **896 live FFmpeg
comparisons**, plus **446 focused checks** in normal/scalar modes. Nine new cases
cover frozen pixels, support admission boundaries, work, negative/tiny alpha,
scratch unwind, cancellation, source edits and retained owners. The consumer
and all three examples pass through the **same actual dependency-free Release
package** on both OSes. Qualification runs once per platform after passing means.

The [numeric evidence](warm-bicubictight8x4word-stream-evidence.json) retains
baseline, screen, confirmation, regression and qualification. Stop this fixed
pair; byte support, other material boundaries and final current-source uniform
parity remain open.
