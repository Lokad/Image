# Warm RGBA16 bicubic wider support with a tight working limit

Independent quiet confirmation measures **1.381× FFmpeg's scaler backend**,
conservative upper **1.403×**, at **571.07 µs / 4,217 B/op**. All twelve actuals
are retained, ten selected; both guards pass. The direct baseline was
8,998.23 µs / 21.311×: the accepted change reduces its mean **93.7%**.

The existing original 513×259 RGBA16 raster becomes 32×16 with a working limit
of **1,071,128 bytes: input + output + 4 KiB**, excluding the full row cache.
One separate bounded word primitive reuses a single output column and up to
seventy-two source rows/taps per axis. Its 4-KiB admission covers 4,032 bytes of
fixed arrays. Each selected source row is horizontally filtered once per column;
there is no decoded-source table. Paired full-range UInt32 premultiplication
products widen unsigned to binary64 exactly, with separate horizontal additions
preserving tap order. Relative offsets are checked before UInt16 narrowing;
absolute coordinates remain Int32. There is no intermediate pixel quantization,
heap workspace, persistent state or output cache. Baseline allocation is
4,224 B/op; raw figures include harness overhead.

The conservative `2^-40*72^2` error budget and depth-scaled quotient bounds preserve
every frozen sample, also matching the independent rational reference exactly.
Uncertain ordinary-alpha outputs replay the original tap order under normalization
and alpha guards. Tiny alpha unwinds the frame/admission before complete original
replay can need exact scratch. Complete work, bounded cancellation, source and
owner contracts remain. Existing byte and smaller-support word kernels stay
unchanged; other supports, directions, hardware and budgets retain prior paths.

The baseline is **zimg, FFmpeg's zscale backend**, with timed thirty-two-pixel
clamped padding (40.269% additional input area), complete source conversion,
premultiplied float filtering, independent output allocation, unrounded-alpha
unpremultiplication, word quantization, rounded-zero-alpha RGB clearing and
release. Normal/scalar native maximum-one RGB/alpha and RMSE-0.25 bounds pass.
This is the labeled backend, not FFmpeg CLI; native memory is neither constrained
nor measured.

Screen, repeat and the existing 64×32 word regression reuse identical verified
production and benchmark assemblies. The regression passes its mean gate at
**1.598×** before qualification; it is a screen, not a new independent certification.
Declared budget, geometry and depth are asserted before scoring. Only changed
inputs build; prepared children and cached restore are reused. No additional
profile or original performance raster is needed.

Windows and Linux each pass **3,010 Release tests**, including **896 live FFmpeg
comparisons**, plus **359 focused checks** in normal/scalar modes. Nine new cases
cover frozen pixels, full unsigned products, admission/work, support/tails,
negative/tiny alpha, cancellation, source edits and retained owners. A source
wider than 65535 columns verifies full absolute coordinates. The consumer and
all three examples pass through the **same actual dependency-free Release
package** on both OSes. Full qualification runs once per platform for this change.

The [numeric evidence](warm-bicubictight32x16-16-columns-evidence.json) retains the
baseline, immutable confirmation/control and qualification. Stop this pair.
Other depths/supports, tight-memory directions and remaining material required/
current-source uniform parity stay open.
