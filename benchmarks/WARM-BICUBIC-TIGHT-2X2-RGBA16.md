# Warm RGBA16 tight-memory bicubic with generated coefficients

Independent quiet confirmation measures **0.469× FFmpeg's scaler backend**,
conservative upper **0.493×**, at **1,039.52 µs / 154 B/op**. All twelve actuals
are retained, eleven selected; both guards pass. The direct baseline was
8,245.76 µs / 3.714×: the accepted change reduces its mean **87.4%**.

The existing original 513×259 RGBA16 raster becomes 2×2 with a working limit
of **1,067,064 bytes: input + output + 4 KiB**, excluding the full row cache.
Horizontal/vertical admission bounds are 1027/519 taps. One separate streaming
primitive generates eight coefficients at a time with the original binary64
Catmull-Rom operations. It keeps no coefficient table. A preparatory scan checks
every generated coefficient bit against the original scalar kernel before use,
while computing normalization and L1 sums; mismatch declines the path.
Eight output-row accumulators and metadata occupy **512 bytes** of fixed arrays
within the admitted 4-KiB workspace.

Each source row is filtered once per output column, retaining only the last
horizontal result and separate original clamped-edge contributions. Full unsigned
products, coefficients and totals remain binary64 with no storage loss or pixel
quantization. The unit bound is `Nx*Ny/2^40*max(1,Lx*Ly/1024)`, with admitted
tap product at most `2^40`; it conservatively covers original and separable
arithmetic. Quotient/quarter-margin guards replay original tap order for uncertainty;
tiny alpha unwinds before exact scratch. Cancellation is checked every 256
horizontal taps. Checked loads, absolute coordinates, work, source and independent
owner contracts remain. No heap/full-support/output cache is added.

The existing tight 4×2 word regression mean passes at **0.511×**, allocating
185 B/op. Its three-sample upper 1.246× is a screen, not another certification.

The baseline is **zimg, FFmpeg's zscale backend**, with **512-pixel clamped
padding (1384.169% additional input area)**. Padding is timed with complete
conversion, premultiplied float filtering, independent allocation, unrounded-alpha
unpremultiplication, word quantization, zero-alpha RGB clearing and release.
Normal/scalar maximum-one RGB/alpha and RMSE-0.25 gates and native negative
controls pass. This is the labeled backend, not FFmpeg CLI; native memory is
neither constrained nor measured.

Screen, independent repeat and regression reuse identical frozen production
and benchmark assemblies. Only changed inputs build; cached restore and verified
prepared children are reused. No profile or new original performance raster was
needed. Baseline allocation is 160 B/op; raw figures include harness overhead.

Windows and Linux each pass **3,137 Release tests**, including **896 live FFmpeg
comparisons**, plus **486 focused checks** in normal/scalar modes. Eleven new
cases cover frozen pixels, supports, admission/work, negative/tiny alpha,
scratch unwind, certified halves, cancellation, source edits, retained owners
and absolute source coordinates above UInt16. Consumer and all three examples
pass through the **same actual dependency-free Release package** on both OSes.
Qualification runs once per platform after passing means.

The [numeric evidence](warm-bicubictight2x2word-generated-evidence.json) retains
baseline, screen, confirmation, regression and qualification. Stop this pair;
byte support, unmatched heights, public boundaries and final current-source
uniform parity remain open.
