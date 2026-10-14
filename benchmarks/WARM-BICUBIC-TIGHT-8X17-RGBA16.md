# Warm RGBA16 tight-memory bicubic with full-coefficient row blocks

Independent quiet confirmation measures **0.973× FFmpeg's scaler backend**,
conservative upper **0.981×**, at **516.33 µs / 1,209 B/op**. All twelve actuals
are retained, eleven selected; both guards pass. The direct baseline was
8,515.53 µs / 16.036×: the change reduces its mean **93.9%**.

The original 513×259 RGBA16 raster becomes 8×17 with a working limit of
**1,068,120 bytes: input + output + 4 KiB**, excluding the full row cache.
Horizontal/vertical admission bounds are 258/62 taps. The full-coefficient word
streaming primitive now handles any reduction height in blocks of at most sixteen
output rows, reusing its **3,936 bytes of fixed arrays**. Existing support, hardware
and resource bounds remain. There is no full-height accumulator array, intermediate
pixel quantization, heap workspace or output cache.

Full binary64 horizontal coefficients, normalization and checked relative offsets
are prepared once per output column. Each block preserves global output coordinates,
complete vertical tap order and separate clamped-edge contributions. Source rows
are filtered once per column within a block; overlapping blocks may repeat that
work. The existing `304^2/2^40` arithmetic budget, depth-scaled quotient/rounding
guards, original-order replay, tiny-alpha scratch unwind, exact unsigned products,
work, cancellation, checked loads and independent ownership remain unchanged.

The existing tight 8×4 word path passes its screen at **0.649× / upper 0.713×**,
allocating **377 B/op**. It uses the same frozen production and benchmark assemblies
before full qualification. This screen is not another independent certification.

The native comparison is **zimg, FFmpeg's zscale backend**, using a **128-pixel
timed clamped halo (198.069% additional input area)**. Complete conversion,
premultiplied float filtering, independent allocation, unrounded-alpha
unpremultiplication, word quantization, zero-alpha RGB clearing and release are
timed. Native normal/scalar maximum-one RGB/alpha and RMSE-0.25 gates and negative
controls pass. Managed pixels remain exact. This is the labeled backend, not
FFmpeg CLI; native memory is neither constrained nor measured.

Only changed inputs build. The screen, independent confirmation and regression
reuse immutable production and benchmark assemblies, verified prepared children
and the cached restore feed. No profile or additional original performance raster
is needed. Baseline allocation is **1,216 B/op**; figures include harness overhead.

Windows and Linux each pass **3,188 Release tests**, including **896 live FFmpeg
comparisons**, plus **537 focused checks** in normal/scalar modes. Eight new cases
cover frozen 8×17 pixels, work/admission, source edits, retained owners, cancellation,
block boundaries/tails at 17/33/65 rows, low alpha and exact scratch unwind.
The consumer and all three examples pass through the **same actual dependency-free
Release package** on both platforms. Qualification runs once per platform after
passing performance means.

The [numeric evidence](warm-bicubictight8x17word-generated-evidence.json) retains
the baseline, screen, confirmation, regression and qualification. This pair is
confirmed; other unmatched supports/heights, public boundaries and final
current-source uniform parity remain open.
