# Warm RGBA8 wider-support bicubic with a tight working limit

Independent quiet confirmation measures **1.680× FFmpeg's scaler backend**,
conservative upper **1.691×**, at **586.27 µs / 32,889 B/op**. All twelve actuals
are retained, eleven selected; both guards pass. The direct baseline was
9,849.31 µs / 28.316×: the accepted change reduces its mean **94.0%**.

The existing original 513×259 RGBA8 raster becomes 128×64 with a working limit
of **568,332 bytes: input + output + 4 KiB**, excluding the full row cache.
One bounded primitive reuses three-column strips and at most twenty source
rows/taps per axis. Its 4-KiB admission covers 4,056 bytes of fixed arrays; no
heap workspace, persistent state or output cache is added. Adjacent exact decoded
pixels use one checked wide-vector store. Fixed strip offsets, a common-tap
prefix and a captured immutable stride remove repeated index/count work while
retaining each accumulation's tap order. Baseline allocation is 32,896 B/op;
raw figures include harness overhead.

The `2^-40*20^2` absolute-error budget and original rounding guards preserve
every frozen managed sample, which also matches the independent rational
reference exactly. Uncertain rounding or tiny alpha releases the strip frame
and admission before complete original replay. Work, bounded cancellation,
source and owner contracts remain. Existing small byte and word kernels remain
unchanged. Larger supports, unsupported windows/hardware and tighter budgets
retain previous paths.

The baseline is **zimg, FFmpeg's zscale backend**, with timed eight-pixel clamped
padding (9.489% additional input area), complete source conversion, premultiplied
float filtering, independent output allocation, unrounded-alpha unpremultiplication,
straight byte quantization, rounded-zero-alpha RGB clearing and release.
Normal/scalar native maximum-one RGB/alpha and RMSE-0.25 bounds pass; managed
pixels remain exact. This is the labeled backend, not FFmpeg CLI. Native memory
is neither constrained nor measured.

The initial strip repeat reaches 1.987× but has upper **2.014×**, so remains
unaccepted. A coefficient-vector trial regresses to **2.024×** and is restored;
it receives no twelve/full qualification. One six-second profile identifies
decoding and horizontal filtering cost. The final screen (625.96 µs), repeat
and small-support regression reuse identical production/harness assemblies and
verified generated children. The regression passes its mean gate at **1.862×**
before qualification; it is a screen, not a new independent certification.
Preparation catches a stale selected native identity before timing; only that
reference worker rebuilds, and the managed child is reused. No original raster
is added. Superseded/failed evidence remains retained.

Windows and Linux each pass **2,975 Release tests**, including **896 live FFmpeg
comparisons**, plus **324 focused checks** in normal/scalar modes. Eight new
cases cover frozen pixels, admission/work, strip tails, support boundaries,
negative/tiny alpha, source edits, cancellation and retained owners. The consumer
and all three examples pass through the **same actual dependency-free Release
package** on both OSes. Full qualification runs once per platform for the accepted
production change.

The [numeric evidence](warm-bicubictightwide8-strips-evidence.json) retains baseline,
provisional/rejected trials, final confirmation/control, profile and qualification.
Stop this pair. Wider word timing, larger/unmatched supports, other tight-memory
paths and remaining material required/current-source uniform parity stay open.
