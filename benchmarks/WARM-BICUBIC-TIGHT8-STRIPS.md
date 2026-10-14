# Warm RGBA8 bicubic with a tight working limit

Independent quiet confirmation measures **1.878× FFmpeg's scaler backend**,
conservative upper **1.895×**, at **783.84 µs / 132,295 B/op**. All twelve actuals
are retained, ten selected; both guards pass. The direct baseline was
8,763.86 µs / 20.990×: the accepted change reduces its mean **91.1%**.

The existing original 513×259 RGBA8 raster becomes 256×129 with a managed working
limit of **667,660 bytes: input + output + 4 KiB**. This excludes the full row
cache. One bounded primitive reuses six-column strips and ten source rows within
an admitted 4-KiB stack workspace (3,640 bytes of arrays). A compact source window
is decoded once per reused row; independent paired accumulators and quotient
guards avoid repeated decoding and scalar normalization chains. No heap workspace,
persistent state or output cache is added. Raw allocation figures include harness
overhead: baseline 132,224 B/op, confirmation 132,295 B/op.

The original absolute-error budget and rounding guards preserve every frozen
managed sample, which also matches the independent rational reference exactly.
An uncertain strip returns to the complete original operation after its frame and
memory admission unwind. Success charges the complete original tap cost; failed
speculation leaves that counter untouched. Cancellation remains bounded. The
path requires byte reduction on both axes, width at least two, at most ten taps
per axis and the declared SIMD support. Other cases retain existing paths.

The native baseline remains **zimg, FFmpeg's zscale backend**, with the existing
complete byte-output operation: timed source deinterleaving, four-pixel clamped
halo (4.697% additional input area), premultiplied filtering, independent
planar/packed output allocation, repacking and release. Normal/scalar maximum-one
RGB/alpha and RMSE-0.25 bounds pass; managed pixels remain exact. This is the labeled
backend, not FFmpeg CLI. The native memory budget is not constrained or measured.

Short intermediate screens remain retained at 2,393.53, 1,550.53, 1,585.46,
1,151.48 and 871.24 µs. A global coefficient-inlining trial regresses to 907.47 µs
and is restored. None receives a twelve-sample run or full qualification. One
six-second profile of the first candidate identifies filtering/geometry cost;
its raw evidence is preserved. The final six-column screen, confirmation and
default-memory control reuse identical production/harness assemblies. The control
passes its mean gate at **1.697×** before qualification; it is a screen, not a new
independent certification of that pair. No new performance raster is added.

Windows and Linux each pass **2,958 Release tests**, including **896 live FFmpeg
comparisons**, and **307 focused checks** in normal/scalar modes. Sixteen new cases
cover budget boundaries, tap/strip tails, negative/tiny/hidden alpha, exact work,
cancellation, source edits and retained independent owners. The consumer and all
three examples pass through the **same actual dependency-free Release package**
on both OSes. Full qualification runs once per platform for the accepted change.

The [numeric evidence](warm-bicubictight8-strips-evidence.json) retains the baseline,
all trials, immutable confirmation/control, profile and qualification identities.
Stop tuning this pair. Other tight-memory depths/supports, larger/unmatched
supports and remaining material required/current-source uniform parity stay open.
