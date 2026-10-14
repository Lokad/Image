# Warm RGBA16 tight-memory bicubic with packed row blocks

Independent quiet confirmation measures **1.028× FFmpeg's scaler backend**,
conservative upper **1.031×**, at **800.20 µs / 409 B/op**. All twelve actuals
are retained and selected; both guards pass. The direct baseline was
8,324.50 µs / 10.686×: the change reduces its mean **90.4%**.

The original 513×259 RGBA16 raster becomes 4×9 with a working limit of
**1,067,320 bytes: input + output + 4 KiB**, excluding the full row cache.
Horizontal/vertical admission bounds are 514/117 taps. The packed-coefficient
word primitive now processes any reduction height in blocks of at most eight
output rows, reusing its **3,776 bytes of fixed arrays**. Existing per-axis support,
hardware and resource bounds remain. There is no full-height accumulator array,
intermediate pixel quantization, heap workspace or output cache.

Each horizontal coefficient retains its sign, exponent and 36 fraction bits in
six bytes. Normalization, L1 magnitudes and measured discarded coefficient error
are prepared once per output column. Each row block preserves global output
coordinates, complete vertical tap order and separate clamped-edge contributions.
The original `544^2/2^40*max(1,Lx*Ly/1024)+2*Qx*Ly` bound, quotient/rounding
guards, full-coefficient original-order replay, tiny-alpha scratch unwind,
cancellation, complete work, checked loads and independent ownership remain.
Overlapping blocks may repeat horizontal filtering; no new precision loss is added.

The existing tight 4×2 word path passes its screen at **0.514× / upper 0.582×**,
allocating **185 B/op**. It uses the same frozen production and benchmark assemblies
before full qualification. This screen is not another independent certification.

The native comparison is **zimg, FFmpeg's zscale backend**, using a **256-pixel
timed clamped halo (494.787% additional input area)**. Complete conversion,
premultiplied float filtering, independent allocation, unrounded-alpha
unpremultiplication, word quantization, zero-alpha RGB clearing and release are
timed. Native normal/scalar maximum-one RGB/alpha and RMSE-0.25 gates and negative
controls pass. Managed pixels remain exact. This is the labeled backend, not
FFmpeg CLI; native memory is neither constrained nor measured.

Only changed inputs build. The screen, independent confirmation and regression
reuse immutable production and benchmark assemblies, verified prepared children
and the cached restore feed. No profile or additional original performance raster
is needed. Baseline allocation is **416 B/op**; figures include harness overhead.

Windows and Linux each pass **3,172 Release tests**, including **896 live FFmpeg
comparisons**, plus **521 focused checks** in normal/scalar modes. Eight new cases
cover frozen 4×9 pixels, work/admission, source edits, retained owners, cancellation,
packed-support block boundaries/tails at 9/17/33 rows, low alpha and exact scratch
unwind. The consumer and all three examples pass through the **same actual
dependency-free Release package** on both platforms. Qualification runs once
per platform after passing performance means.

The [numeric evidence](warm-bicubictight4x9word-generated-evidence.json) retains
the baseline, screen, confirmation, regression and qualification. This pair is
confirmed; other unmatched supports/heights, public boundaries and final
current-source uniform parity remain open.
