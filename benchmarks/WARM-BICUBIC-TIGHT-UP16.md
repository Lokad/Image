# Warm RGBA16 bicubic enlargement with a tight working limit

Independent quiet confirmation measures **1.760× FFmpeg's scaler backend**,
conservative upper **1.772×**, at **5,088.24 µs / 4,264,616 B/op**. All twelve
actuals are retained, eleven selected; both guards pass. The direct baseline
was 47,521.59 µs / 16.447×: the accepted change reduces its mean **89.3%**.

The existing original 513×259 RGBA16 raster becomes 1027×519 with a working
limit of **5,331,136 bytes: input + output + 4 KiB**, excluding the full row
cache. One separate word primitive reuses eight-column strips and five source
rows/taps per axis. Its 4-KiB admission covers 2,416 bytes of fixed arrays.
A bounded source window is decoded once per reused row. Full-range UInt32
premultiplication products widen unsigned to binary64 exactly, preserving each
horizontal and vertical tap order. Paired and single word outputs retain
depth-scaled quotient bounds with the conservative `2^-40*16^2` error budget.
Every frozen sample stays exact and matches the independent rational reference.

Uncertain ordinary or negative-alpha samples replay their original order locally;
tiny alpha unwinds the complete strip operation before exact scratch. Complete
work, bounded cancellation, source and owner contracts remain. There is no heap
workspace, persistent state or output cache. Existing byte and reduction kernels
stay unchanged; other directions, hardware and smaller budgets retain prior paths.
Baseline allocation is 4,264,286 B/op; raw figures include harness overhead.
The operation still acquires and releases a complete independently owned output.

The baseline is **zimg, FFmpeg's zscale backend**, with timed four-pixel clamped
padding (4.697% additional input area), complete source conversion, premultiplied
float filtering, independent output allocation, unrounded-alpha unpremultiplication,
word quantization, rounded-zero-alpha RGB clearing and release. Normal/scalar
native maximum-one RGB/alpha and RMSE-0.25 bounds pass. This is the labeled
backend, not FFmpeg CLI; native memory is neither constrained nor measured.

Screen, repeat and the existing default-memory enlargement regression reuse
identical verified production and benchmark assemblies. The regression passes
its mean gate at **1.523×** before qualification; it is a screen, not a new
independent certification. Declared budget, geometry and depth are asserted
before scoring. Only changed inputs build; prepared children and cached restore
are reused. No additional profile or original performance raster is needed.

Windows and Linux each pass **3,045 Release tests**, including **896 live FFmpeg
comparisons**, plus **394 focused checks** in normal/scalar modes. Eight new
cases cover frozen pixels, products, admission/work, strip tails, negative/tiny
alpha, cancellation, source edits and retained owners. Linux intrinsic checks
caught indexing errors before any candidate timing; those preparation failures
remain retained and unscored. The corrected build passes all checks. The consumer
and all three examples pass through the **same actual dependency-free Release
package** on both OSes. Full qualification runs once per platform for this change.

The [numeric evidence](warm-bicubictightup16-columns-evidence.json) retains the
baseline, immutable confirmation/control and qualification. Stop this pair.
Other depths/directions, public boundaries and remaining material required/
current-source uniform parity stay open.
