# Warm RGBA8 tight-memory bicubic with generated coefficients

Independent quiet confirmation measures **0.616× FFmpeg's scaler backend**,
conservative upper **0.634×**, at **1,040.64 µs / 138 B/op**. All twelve actuals
are retained and selected; both guards pass. The direct baseline was
8,025.32 µs / 4.837×: the accepted change reduces its mean **87.0%**.

The original 513×259 RGBA8 raster becomes 2×2 with a working limit of
**535,580 bytes: input + output + 4 KiB**, excluding the full row cache.
Horizontal/vertical admission bounds are 1027/519 taps. A separate byte primitive
generates eight coefficients at a time using the original binary64 Catmull-Rom
operations. A preparatory scan checks every generated coefficient bit against
the original scalar kernel and computes normalization and L1 sums. A mismatch
declines the path. No coefficient table is stored; fixed arrays occupy **512 bytes**
within the admitted 4-KiB workspace.

Each source row is filtered once per output column. The primitive retains only
the last horizontal result and separate original clamped-edge contributions.
Exact byte products, coefficients and totals remain binary64 with no intermediate
pixel quantization. The unit bound is `Nx*Ny/2^40*max(1,Lx*Ly/1024)`, with admitted
tap product at most `2^40`. Quotient and rounding guards replay original tap order
when uncertain; tiny alpha unwinds before exact scratch. Cancellation, checked
loads, absolute coordinates, complete work, source and independent owner contracts
remain. There is no heap, full-support or output cache.

The existing tight 4×2 byte regression screen passes at **0.717× / upper 0.825×**,
allocating **153 B/op**. This three-sample screen is not another certification.

The baseline is **zimg, FFmpeg's zscale backend**, with **512-pixel clamped
padding (1384.169% additional input area)**. Padding is timed with complete
conversion, premultiplied float filtering, independent allocation, unrounded-alpha
unpremultiplication, byte quantization, zero-alpha RGB clearing and release.
Native normal/scalar maximum-one RGB/alpha and RMSE-0.25 gates and negative
controls pass. Managed pixels remain exact. This is the labeled backend, not
FFmpeg CLI; native memory is neither constrained nor measured.

The screen, independent confirmation and regression reuse identical frozen
production and benchmark assemblies. Only changed inputs build; verified prepared
children and cached restore are reused. No profile or new original performance
raster was needed. Baseline allocation is **144 B/op**; figures include harness
overhead.

Windows and Linux each pass **3,148 Release tests**, including **896 live FFmpeg
comparisons**, plus **497 focused checks** in normal/scalar modes. Eleven new
cases cover frozen pixels, supports, admission/work, negative and tiny alpha,
scratch unwind, certified halves, cancellation, source edits, retained owners
and absolute coordinates above UInt16. The consumer and all three examples pass
through the **same actual dependency-free Release package** on both OSes.
Full qualification runs once per platform after passing means.

The [numeric evidence](warm-bicubictight2x2byte-generated-evidence.json) retains
the baseline, screen, confirmation, regression and qualification. This pair is
confirmed; unmatched heights, public boundaries and final current-source uniform
parity remain open.
