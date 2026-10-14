# Warm RGBA16 horizontal bicubic with an unchanged height

Tight-memory 513×259→256×259 bicubic independently confirms **1.503× FFmpeg's
scaler backend**, conservative upper **1.510×**, at **848.00 µs / 530,858 B/op**.
All twelve actuals are retained, ten selected; both guards pass. The direct baseline
was 4,151.02 µs / 7.326×: the change reduces its mean **79.6%**.

The working limit is **1,597,464 bytes: input + output + 4 KiB**, excluding the
existing row cache. Horizontal/vertical admission bounds are 10/5 taps. The
horizontal-first word primitive now also admits an unchanged height. Eight output
columns reuse five filtered source-row slots; 32 decoded source-column slots and
the other fixed arrays occupy **3,440 bytes** within the unchanged 4-KiB charge.
Full coefficients, exact unsigned products, unrounded intermediates, tap order,
precision guards, original rounding replay and exact tiny-alpha scratch unwind
are preserved. Work, cancellation, checked loads, source and independent owner
contracts remain. No heap workspace, persistent state or output cache is added.

The existing cache executes before this fallback, preserving the default-memory
strategy. Its regression screen passes at **1.560× / upper 1.660×**,
**884.50 µs / 730,690 B/op**, on the same frozen production and benchmark assemblies.
That three-sample screen is not another independent certification. The default
path and its production inputs are unchanged, so its independent confirmation
is not repeated in this round. Tight baseline allocation is **530,742 B/op**;
figures include harness overhead.

The native comparison is **zimg, FFmpeg's zscale backend**, with the existing
**four-pixel timed clamped halo (4.697% additional input area)**. Complete packing,
premultiplied float filtering, independent allocation, unrounded-alpha
unpremultiplication, word quantization, zero-alpha RGB clearing and release are
timed. Native normal/scalar maximum-one RGB/alpha and RMSE-0.25 gates and negative
controls pass. Managed pixels remain exact, including the rational reference.
This is the labeled backend, not FFmpeg CLI; native memory is neither constrained
nor measured.

Windows and Linux each pass **3,222 Release tests**, including **896 live FFmpeg
comparisons**, plus **571 focused checks** in normal/scalar modes. Eight new cases
cover frozen pixels at 4095/4096-byte scratch, work/admission, source edits,
retained owners, cancellation, one-row and odd-width tails, low alpha and exact
scratch unwind. The consumer and all three examples pass through the **same actual
dependency-free Release package** on both platforms. Full qualification runs
once per platform after passing performance means.

Only changed inputs build; verified prepared children, cached restore and native
identities are reused. No new profile or original performance raster is needed.
The [numeric evidence](warm-bicubictighthorizontal16-cross-evidence.json) retains
baseline, screen, independent confirmation, regression and qualification. This
pair is confirmed; other material coverage and final current-source uniform
parity remain open.
