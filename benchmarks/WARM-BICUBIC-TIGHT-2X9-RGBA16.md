# Warm RGBA16 tight-memory bicubic with bounded row blocks

Independent quiet confirmation measures **1.223× FFmpeg's scaler backend**,
conservative upper **1.275×**, at **1,535.71 µs / 266 B/op**. All twelve actuals
are retained and selected; both guards pass. The direct baseline was
8,184.51 µs / 6.603×: the change reduces its mean **81.2%**.

The original 513×259 RGBA16 raster becomes 2×9 with a working limit of
**1,067,176 bytes: input + output + 4 KiB**, excluding the full row cache.
Horizontal/vertical admission bounds are 1027/117 taps. The generated-coefficient
word primitive now processes output rows in blocks of at most eight, reusing
the same **512 bytes of fixed arrays**. It admits arbitrary reduction heights
within the existing tap-product, hardware and resource bounds. It stores no
full-height accumulator array, intermediate pixels or coefficient table.

Horizontal coefficient verification and normalization occur once per output
column. Each block retains original global output coordinates, complete vertical
tap order, normalization and separate clamped-edge contributions. Source rows
are filtered once per column within each block; overlapping blocks may repeat
that work. Exact unsigned products and generated coefficient checks, L1-scaled
arithmetic guards, original rounding replay, tiny-alpha scratch unwind, cancellation,
complete work, checked loads and independent ownership remain unchanged.

The existing tight 2×2 word path passes its screen at **0.451× / upper 0.759×**,
allocating **154 B/op**. It uses the same frozen production and benchmark assemblies
before full qualification. This screen is not another independent certification.

The native comparison is **zimg, FFmpeg's zscale backend**, using a **512-pixel
timed clamped halo (1384.169% additional input area)**. Complete conversion,
premultiplied float filtering, independent allocation, unrounded-alpha
unpremultiplication, word quantization, zero-alpha RGB clearing and release are
timed. Native normal/scalar maximum-one RGB/alpha and RMSE-0.25 gates and negative
controls pass. Managed pixels remain exact. This is the labeled backend, not
FFmpeg CLI; native memory is neither constrained nor measured.

Only changed inputs build. The screen, independent confirmation and regression
reuse immutable production and benchmark assemblies, verified prepared children
and the cached restore feed. No profile or additional original performance raster
is needed. Baseline allocation is **272 B/op**; figures include harness overhead.

Windows and Linux each pass **3,156 Release tests**, including **896 live FFmpeg
comparisons**, plus **505 focused checks** in normal/scalar modes. Eight new cases
cover the frozen nine-row output, work/admission, source edits, retained owners,
cancellation, block boundaries and tails at 9/17/33 rows, low alpha and exact
scratch unwind. The consumer and all three examples pass through the **same
actual dependency-free Release package** on both platforms. Qualification runs
once per platform after passing performance means.

The [numeric evidence](warm-bicubictight2x9word-generated-evidence.json) retains
the baseline, screen, confirmation, regression and qualification. This pair is
confirmed; other unmatched supports/heights, public boundaries and final
current-source uniform parity remain open.
