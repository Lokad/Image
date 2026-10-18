# Warm RGBA16 vertical bicubic with an unchanged width

Historical timing note: [byte-row output specialization](WARM-BICUBIC-BYTE-ROW.md)
replaces thirteen default full-row-cache comparisons with current qualified short
gaps. Only the affected default-cache timings here are superseded; other selected
paths retain their applicable evidence in the [current summary](../docs/benchmark.md).

Tight-memory 513×259→513×129 bicubic independently confirms **1.813× FFmpeg's
scaler backend**, conservative upper **1.823×**, at **1,059.29 µs / 529,838 B/op**.
All twelve actuals are retained, eleven selected; both guards pass. The direct
baseline was 7,996.03 µs / 13.693×: the change reduces its mean **86.8%**.

The working limit is **1,596,448 bytes: input + output + 4 KiB**, excluding the
existing row cache. Horizontal/vertical admission bounds are 5/10 taps. The
vertical-first word primitive now also admits an unchanged width when the row
cache cannot fit. Four output rows reuse five source-column slots in **2,016 bytes
of fixed arrays** within the unchanged 4-KiB charge. Original full coefficients,
unrounded intermediates, unsigned products, tap order, precision guards, original
rounding replay and exact tiny-alpha scratch unwind are preserved. Work,
cancellation, checked loads, source and independent owner contracts remain.

The initial unconditional admission regressed default-memory timing to **1.847×**.
That prototype was not accepted or fully qualified. The final primitive defers
to the existing row cache when its hardware, array and memory requirements fit.
The cache needs `(85 doubles + 17 ints) * width + 128 bytes`, plus exact-alpha
scratch. Boundary tests preserve pixels immediately below, at and above admission.

Default memory is independently reconfirmed at **1.423× / upper 1.428×**,
**832.49 µs / 913,737 B/op**. All twelve actuals are retained and selected;
both guards pass. This retains the faster existing cache strategy. Both final
confirmations and the regression screen reuse identical frozen production and
benchmark assemblies. Tight baseline allocation is **529,613 B/op**; allocation
figures include harness overhead.

The native comparison is **zimg, FFmpeg's zscale backend**, with the existing
**four-pixel timed clamped halo (4.697% additional input area)**. Complete packing,
premultiplied float filtering, independent allocation, unrounded-alpha
unpremultiplication, word quantization, zero-alpha RGB clearing and release are
timed. Native normal/scalar maximum-one RGB/alpha and RMSE-0.25 gates and negative
controls pass. Managed pixels remain exact. This is the labeled backend, not
FFmpeg CLI; native memory is neither constrained nor measured.

Windows and Linux each pass **3,205 Release tests**, including **896 live FFmpeg
comparisons**, plus **554 focused checks** in normal/scalar modes. Nine new cases
cover frozen pixels at 4095/4096-byte scratch, work/admission, source edits,
retained owners, cancellation, one-column and odd-width tails, low alpha, exact
scratch unwind and the row-cache admission boundary. Two initial reference tests
incorrectly gave exact alpha rounding only 4095 bytes; that setup was corrected
before timing and the failed log is retained. No production pixel gate changed.
The consumer and all three examples pass through the **same actual dependency-free
Release package** on both platforms. Full qualification runs once per platform
for the accepted production, after passing performance means.

Only changed inputs build; verified prepared children, cached restore and native
worker identities are reused. No new profile or original performance raster is
needed. The [numeric evidence](warm-bicubictightvertical16-cachepref-evidence.json)
retains baseline, rejected prototype, final tight/default confirmations, regression
and qualification. Both pairs are confirmed; other material coverage and final
current-source uniform parity remain open.
