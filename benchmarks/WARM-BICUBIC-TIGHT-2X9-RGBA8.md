# Warm RGBA8 tight-memory bicubic with bounded row blocks

Independent quiet confirmation measures **1.332× FFmpeg's scaler backend**,
conservative upper **1.341×**, at **1,355.20 µs / 194 B/op**. All twelve actuals
are retained and selected; both guards pass. The direct baseline was
8,058.12 µs / 7.963×: the change reduces its mean **83.2%**.

The original 513×259 RGBA8 raster becomes 2×9 with a working limit of
**535,636 bytes: input + output + 4 KiB**, excluding the full row cache.
Horizontal/vertical admission bounds are 1027/117 taps. The byte primitive now
processes any reduction height in blocks of at most eight output rows, reusing
the same **512 bytes of fixed arrays**. Existing tap-product, hardware and resource
bounds apply. There is no full-height accumulator array, intermediate pixel
quantization or coefficient table.

Horizontal coefficient-bit verification and normalization occur once per output
column. Each block retains original global output coordinates, complete vertical
tap order, normalization and separate clamped-edge contributions. Source rows
are filtered once per column within each block; overlapping blocks may repeat
that work. Exact byte products, generated coefficient checks, L1-scaled arithmetic
guards, original rounding replay, tiny-alpha scratch unwind, cancellation, work,
checked loads, source and independent ownership contracts are preserved.

The existing tight 2×2 byte path passes its screen at **0.641× / upper 1.833×**,
allocating **138 B/op**. It uses the same frozen production and benchmark assemblies
before full qualification. This screen is not another independent certification.

The native comparison is **zimg, FFmpeg's zscale backend**, using a **512-pixel
timed clamped halo (1384.169% additional input area)**. Complete conversion,
premultiplied float filtering, independent allocation, unrounded-alpha
unpremultiplication, byte quantization, zero-alpha RGB clearing and release are
timed. Native normal/scalar maximum-one RGB/alpha and RMSE-0.25 gates and negative
controls pass. Managed pixels remain exact. This is the labeled backend, not
FFmpeg CLI; native memory is neither constrained nor measured.

Only changed inputs build. The screen, independent confirmation and regression
reuse immutable production and benchmark assemblies, verified prepared children
and the cached restore feed. No profile or additional original performance raster
is needed. Baseline allocation is **200 B/op**; figures include harness overhead.

Windows and Linux each pass **3,164 Release tests**, including **896 live FFmpeg
comparisons**, plus **513 focused checks** in normal/scalar modes. Eight new cases
cover the frozen nine-row output, work/admission, source edits, retained owners,
cancellation, block boundaries and tails at 9/17/33 rows, low alpha and exact
scratch unwind. The consumer and all three examples pass through the **same
actual dependency-free Release package** on both platforms. Qualification runs
once per platform after passing performance means.

The [numeric evidence](warm-bicubictight2x9byte-generated-evidence.json) retains
the baseline, screen, confirmation, regression and qualification. This pair is
confirmed; other unmatched supports/heights, public boundaries and final
current-source uniform parity remain open.
