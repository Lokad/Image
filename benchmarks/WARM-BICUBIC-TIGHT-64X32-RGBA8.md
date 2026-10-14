# Warm RGBA8 bicubic beyond twenty taps with a tight working limit

Independent quiet confirmation measures **1.796× FFmpeg's scaler backend**,
conservative upper **1.818×**, at **635.44 µs / 8,313 B/op**. All twelve actuals
are retained, ten selected; both guards pass. The direct baseline was
8,761.39 µs / 24.951×: the accepted change reduces its mean **92.7%**.

The existing original 513×259 RGBA8 raster becomes 64×32 with a working limit
of **543,756 bytes: input + output + 4 KiB**, excluding the full row cache.
One bounded primitive reuses two-column strips and up to thirty-six source
rows/taps per axis. Its 4-KiB admission covers 3,992 bytes of fixed arrays.
Exact byte premultiplication products are at most 65025 and fit UInt16, alongside
alpha; compact storage introduces no intermediate pixel quantization. Relative
source-window offsets also fit UInt16, while absolute coordinates remain Int32
and are checked before narrowing. Paired independent products widen exactly to
binary64 for filtering. No heap workspace, persistent state or output cache is
added. Baseline allocation is 8,320 B/op; raw figures include harness overhead.

Original scalar coefficients, each accumulation's tap order and the conservative
`2^-40*36^2` error budget preserve every frozen sample, which also matches the
independent rational reference exactly. Uncertainty or tiny alpha unwinds the
strip frame/admission before complete original replay. Work, bounded cancellation,
source and owner contracts remain. Existing smaller-support byte and word
kernels stay unchanged; other supports/windows/hardware retain prior paths.

The baseline is **zimg, FFmpeg's zscale backend**, with timed sixteen-pixel clamped
padding (19.364% additional input area), complete source conversion, premultiplied
float filtering, independent output allocation, unrounded-alpha unpremultiplication,
byte quantization, rounded-zero-alpha RGB clearing and release. Normal/scalar native
maximum-one RGB/alpha and RMSE-0.25 bounds pass. This is the labeled backend, not
FFmpeg CLI; native memory is neither constrained nor measured.

Screen, repeat and existing 128×64 regression reuse identical verified production
and benchmark assemblies. The regression passes its mean gate at **1.708×**
before qualification; it is a screen, not a new independent certification.
Declared budget, geometry and depth are asserted before scoring. Only changed
production/harness inputs build; generated children and cached restore are reused.
No additional profile or original performance raster is needed.

Windows and Linux each pass **2,992 Release tests**, including **896 live FFmpeg
comparisons**, plus **341 focused checks** in normal/scalar modes. Nine new cases
cover frozen pixels, products, admission/work, support/strip tails, negative/tiny
alpha, cancellation, source edits and retained owners. A source wider than 65535
columns verifies that relative compact offsets preserve full absolute coordinates.
The consumer and all three examples pass through the **same actual dependency-free
Release package** on both OSes. Full qualification runs once per platform for the
accepted production change.

The [numeric evidence](warm-bicubictight64x32-8-packed-evidence.json) retains the
baseline, immutable confirmation/control and qualification. Stop this pair.
Word timing, larger/unmatched supports, other tight-memory directions and remaining
material required/current-source uniform parity stay open.
