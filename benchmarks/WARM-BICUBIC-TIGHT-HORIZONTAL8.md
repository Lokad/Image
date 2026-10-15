# Warm RGBA8 horizontal bicubic with an unchanged height

Tight-memory 513×259→256×259 bicubic independently confirms **1.414× FFmpeg's
scaler backend**, conservative upper **1.425×**, at **811.19 µs / 265,502 B/op**.
All twelve actuals are retained, eleven selected; both guards pass. The direct
baseline was 4,100.16 µs / 7.182×: the change reduces its mean **80.2%**.

The working limit is **800,780 bytes: input + output + 4 KiB**, excluding the
existing row cache. Horizontal/vertical admission bounds are 10/5 taps. The
horizontal-first byte primitive now also admits an unchanged height. Eight output
columns reuse five filtered source-row slots; 32 decoded source-column slots and
the other fixed arrays occupy **3,440 bytes** within the unchanged 4-KiB charge.
Full coefficients, exact byte products, unrounded intermediates, tap order,
precision guards, original rounding replay and tiny-alpha replay after workspace
release are preserved. Work, cancellation, checked loads, source and independent
owner contracts remain. No heap workspace, persistent state or output cache is added.

The existing cache executes before this fallback, preserving the default-memory
strategy. Its regression screen passes at **1.471× / upper 1.572×**,
**841.65 µs / 465,375 B/op**, on the same frozen production and benchmark assemblies.
That three-sample screen is not another independent certification. The default
path and its production inputs are unchanged, so its independent confirmation
is not repeated. Tight baseline allocation is **265,444 B/op**; figures include
harness overhead.

The managed gate retains the frozen original binary64 output, including **six
independently certified exact rational alpha halves** already allowed by SPEC.
Every managed byte remains exact; no managed tolerance is added. The native
comparison is **zimg, FFmpeg's zscale backend**, with the existing **four-pixel
timed clamped halo (4.697% additional input area)**. Complete packing, premultiplied
float filtering, independent allocation, unrounded-alpha unpremultiplication,
byte quantization, zero-alpha RGB clearing and release are timed. Native normal/
scalar maximum-one RGB/alpha and RMSE-0.25 gates and negative controls pass.
This is the labeled backend, not FFmpeg CLI; native memory is neither constrained
nor measured.

Windows and Linux each pass **3,230 Release tests**, including **896 live FFmpeg
comparisons**, plus **579 focused checks** in normal/scalar modes. Eight new cases
cover frozen pixels at 4095/4096-byte scratch, work/admission, source edits,
retained owners, cancellation, one-row and odd-width tails, low alpha and exact
scratch unwind. The consumer and all three examples pass through the **same actual
dependency-free Release package** on both platforms. Full qualification runs
once per platform after passing performance means.

Only changed inputs build; verified prepared children, cached restore and native
identities are reused. The new case uses the existing six-half receipt contract;
no comparison gate changes. No new profile or original performance raster is needed.
The [numeric evidence](warm-bicubictighthorizontal8-cross-evidence.json) retains
baseline, screen, independent confirmation, regression and qualification. This
pair is confirmed; other material coverage and final current-source uniform
parity remain open.
