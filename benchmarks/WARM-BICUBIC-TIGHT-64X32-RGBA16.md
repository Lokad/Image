# Warm RGBA16 bicubic beyond twenty taps with a tight working limit

Independent quiet confirmation measures **1.600× FFmpeg's scaler backend**,
conservative upper **1.624×**, at **622.50 µs / 16,505 B/op**. All twelve actuals
are retained, eleven selected; both guards pass. The direct baseline was
8,932.88 µs / 23.511×: the accepted change reduces its mean **93.0%**.

The existing original 513×259 RGBA16 raster becomes 64×32 with a working limit
of **1,083,416 bytes: input + output + 4 KiB**, excluding the full row cache.
One separate bounded word primitive reuses two-column strips and up to
thirty-four source rows/taps per axis. Its 4-KiB admission covers 4,044 bytes
of fixed arrays. Full-range premultiplication products reach 4294836225, remain
UInt32 and widen unsigned to binary64 exactly. Relative source-window offsets
fit UInt16 after checking; absolute coordinates remain Int32. There is no
intermediate pixel quantization, heap workspace, persistent state or output
cache. Baseline allocation is 16,512 B/op; raw figures include harness overhead.

Original scalar coefficients, each accumulation's tap order and the conservative
`2^-40*34^2` error budget preserve every frozen sample, also matching the independent
rational reference exactly. Quotient bounds scale with 65535 and its square.
Uncertain ordinary-alpha outputs replay the original tap order under normalization
and alpha guards. Tiny alpha unwinds the strip frame/admission before complete
original replay can need exact scratch. Work, bounded cancellation, source and
owner contracts remain. Existing byte and smaller-support word kernels stay
unchanged; other supports/windows/hardware retain prior paths.

The baseline is **zimg, FFmpeg's zscale backend**, with timed sixteen-pixel clamped
padding (19.364% additional input area), complete source conversion, premultiplied
float filtering, independent output allocation, unrounded-alpha unpremultiplication,
word quantization, rounded-zero-alpha RGB clearing and release. Normal/scalar native
maximum-one RGB/alpha and RMSE-0.25 bounds pass. This is the labeled backend, not
FFmpeg CLI; native memory is neither constrained nor measured.

Screen, repeat and the existing byte 64×32 regression reuse identical verified
production and benchmark assemblies. The byte regression passes its mean gate
at **1.836×** before qualification; it is a screen, not a new independent
certification. Declared budget, geometry and depth are asserted before scoring.
Only changed inputs build; prepared children and cached restore are reused.
No additional profile or original performance raster is needed.

Windows and Linux each pass **3,001 Release tests**, including **896 live FFmpeg
comparisons**, plus **350 focused checks** in normal/scalar modes. Nine new cases
cover frozen pixels, unsigned products, admission/work, support/strip tails,
negative/tiny alpha, cancellation, source edits and retained owners. A source
wider than 65535 columns verifies that relative offsets preserve full absolute
coordinates. The consumer and all three examples pass through the **same actual
dependency-free Release package** on both OSes. Full qualification runs once per
platform for the accepted production change.

The [numeric evidence](warm-bicubictight64x32-16-packed-evidence.json) retains the
baseline, immutable confirmation/control and qualification. Stop this pair.
Larger/unmatched supports, other tight-memory directions and remaining material
required/current-source uniform parity stay open.
