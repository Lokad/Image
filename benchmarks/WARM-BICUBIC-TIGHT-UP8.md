# Warm RGBA8 bicubic enlargement with a tight working limit

Independent quiet confirmation measures **1.773× FFmpeg's scaler backend**,
conservative upper **1.908×**, at **5,395.57 µs / 2,493,057 B/op**. All twelve
actuals are retained, nine selected; both guards pass. The direct baseline was
48,511.74 µs / 15.961×: the accepted change reduces its mean **88.9%**.

The existing original 513×259 RGBA8 raster becomes 1027×519 with a working
limit of **2,667,616 bytes: input + output + 4 KiB**, excluding the full row
cache. One separate byte primitive reuses eight-column strips and five source
rows/taps per axis. Its 4-KiB admission covers 2,416 bytes of fixed arrays.
Exact byte products widen to binary64, preserving each accumulation's tap order.
Paired and single byte outputs retain depth-scaled quotient bounds with the
conservative `2^-40*16^2` budget. Every frozen sample stays exact and matches
the independent rational reference.

Ordinary/negative-alpha uncertainty replays locally. Tiny-alpha pixels are
temporarily marked in the private unfinished output, then resolved after the
non-inlined core frame returns and releases its admission. The marker cannot
occur in a completed resize: zero alpha requires zero RGB. The complete timed
final scan replays original color arithmetic and exact rational alpha rounding,
including its extra work charge, with the full 4-KiB scratch available. Every
marker is resolved before return. This adds no heap workspace, persistent state
or output cache; source, memory/work limits, cancellation and owner contracts
remain. Existing word and smaller-support byte kernels stay unchanged.
Baseline allocation is 2,492,526 B/op; raw figures include harness overhead
and the existing exact-alpha arithmetic. Independent output acquisition/release
and the deferred compatibility pass are included in managed timing.

The baseline is **zimg, FFmpeg's zscale backend**, with timed four-pixel clamped
padding (4.697% additional input area), complete source conversion, premultiplied
float filtering, independent output allocation, unrounded-alpha unpremultiplication,
byte quantization, rounded-zero-alpha RGB clearing and release. Normal/scalar
native maximum-one RGB/alpha and RMSE-0.25 bounds pass. This is the labeled
backend, not FFmpeg CLI; native memory is neither constrained nor measured.

Screen, repeat and the existing default-memory enlargement regression reuse
identical verified production and benchmark assemblies. The regression passes
its mean gate at **1.554×** before qualification; it is a screen, not a new
independent certification. Declared budget, geometry and depth are asserted
before scoring. Only changed inputs build; prepared children and cached restore
are reused. No additional profile or original performance raster is needed.

Windows and Linux each pass **3,054 Release tests**, including **896 live FFmpeg
comparisons**, plus **403 focused checks** in normal/scalar modes. Nine new cases
cover frozen pixels, products, admission/work, strip tails, negative/tiny alpha,
cancellation, source edits and retained owners. The explicit deferred-alpha case
checks exact half rounding, transparent RGB clearing, scratch admission and extra
work charges. A preparation test initially assumed that 4095 scratch bytes could
handle tiny alpha; the existing contract requires 4096. That failure remains
retained and unscored; the corrected assertion preserves admission semantics.
The consumer and all three examples pass through the **same actual dependency-free
Release package** on both OSes. Full qualification runs once per platform.

The [numeric evidence](warm-bicubictightup8-columns-evidence.json) retains the
baseline, immutable confirmation/control and qualification. Stop this pair.
Other directions, public boundaries and remaining material required/current-source
uniform parity stay open.
