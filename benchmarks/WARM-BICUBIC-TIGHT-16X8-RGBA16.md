# Warm RGBA16 bicubic streaming with a tight working limit

Independent quiet confirmation measures **0.964× FFmpeg's scaler backend**,
conservative upper **0.984×**, at **473.41 µs / 1,145 B/op**. All twelve actuals
are retained, eleven selected; both guards pass. The direct baseline was
8,869.01 µs / 18.103×: the accepted change reduces its mean **94.7%**.

The existing original 513×259 RGBA16 raster becomes 16×8 with a working limit
of **1,068,056 bytes: input + output + 4 KiB**, excluding the full row cache.
One separate streaming word primitive supports at most 256 taps per axis and
sixteen output rows. Its 4-KiB admission covers 3,456 bytes of fixed arrays.
Each selected source row is horizontally filtered once per output column, then
feeds the bounded output accumulators. Increasing unclamped vertical coordinates
preserve each output's original tap order and separate contributions from duplicated
clamped edge rows; only the last horizontal result is retained.

Full-range UInt32 premultiplication products widen unsigned to binary64 exactly.
Paired products retain separate horizontal additions. Checked relative offsets
fit UInt16; absolute coordinates remain Int32. The conservative `2^-40*256^2`
error budget and depth-scaled quotient bounds preserve every frozen sample,
also matching the independent rational reference exactly. Guarded ordinary-alpha
uncertainty replays the original order; tiny alpha unwinds before exact scratch.
Complete work, bounded cancellation, source and owner contracts remain. There is
no heap workspace, full support cache, persistent state or output cache. Existing
byte and smaller-support word kernels stay unchanged. Other supports, heights,
directions, hardware and budgets retain prior paths. Baseline allocation is
1,152 B/op; raw figures include harness overhead.

The baseline is **zimg, FFmpeg's zscale backend**, with timed sixty-four-pixel
clamped padding (86.703% additional input area), complete source conversion,
premultiplied float filtering, independent output allocation, unrounded-alpha
unpremultiplication, word quantization, rounded-zero-alpha RGB clearing and
release. Normal/scalar native maximum-one RGB/alpha and RMSE-0.25 bounds pass.
This is the labeled backend, not FFmpeg CLI; native memory is neither constrained
nor measured.

Screen, repeat and the existing 32×16 word regression reuse identical verified
production and benchmark assemblies. The regression passes its mean gate at
**1.380×** before qualification; it is a screen, not a new independent certification.
Declared budget, geometry and depth are asserted before scoring. Only changed
inputs build; prepared children and cached restore are reused. No additional
profile or original performance raster is needed.

Windows and Linux each pass **3,028 Release tests**, including **896 live FFmpeg
comparisons**, plus **377 focused checks** in normal/scalar modes. Nine new cases
cover frozen pixels, full unsigned products, admission/work, wide support/tails,
negative/tiny alpha, cancellation, source edits and retained owners. A source
wider than 65535 columns verifies full absolute coordinates. The consumer and
all three examples pass through the **same actual dependency-free Release
package** on both OSes. Full qualification runs once per platform for this change.

The [numeric evidence](warm-bicubictight16x8-16-streaming-evidence.json) retains the
baseline, immutable confirmation/control and qualification. Stop this pair.
Other depths, supports, tight-memory directions and remaining material required/
current-source uniform parity stay open.
