# Warm RGBA8 tight-memory bicubic with packed coefficients

Independent quiet confirmation measures **0.606× FFmpeg's scaler backend**,
conservative upper **0.618×**, at **665.48 µs / 153 B/op**. All twelve actuals
are retained and selected; both guards pass. The direct baseline was
8,165.10 µs / 7.463×: the accepted change reduces its mean **91.8%**.

The existing original 513×259 RGBA8 raster becomes 4×2 with a working limit
of **535,596 bytes: input + output + 4 KiB**, excluding the full row cache.
Horizontal/vertical admission bounds are 514/519 taps. One separate byte
streaming primitive supports up to 544 taps per axis and eight output rows.
Six-byte coefficients and the other fixed arrays occupy **3,776 bytes** within
the admitted workspace. Each source row is filtered once per output column;
only the last horizontal result is stored, preserving separate original
clamped-edge contributions.

Each coefficient retains the upper 48 binary64 bits, including 36 fraction bits;
discarded absolute error is measured. Exact byte products and accumulators
remain binary64. The word strategy's conservative unit bound
`2^-40*544^2*max(1,Lx*Ly/1024)+2*Qx*Ly` covers both arithmetic orders and
coefficient storage loss, with byte-scaled quotient and quarter-margin guards.
Uncertainty recomputes original full coefficients and tap order; tiny alpha
unwinds before exact scratch. Frozen/rational pixels remain exact, including
original rounding at independently certified halves. Complete work, cancellation,
checked loads, absolute coordinates, source and owner contracts remain.
No heap/full-support/output cache is added. Other geometry, hardware and budgets
retain prior paths; the word and smaller-support kernels remain unchanged.

The existing tight 8×4 byte regression mean passes at **0.795×**, allocating
249 B/op. Its three-sample upper 0.933× is a screen, not another certification.

The baseline is **zimg, FFmpeg's zscale backend**, using the existing native
macro and **256-pixel clamped padding (494.787% additional input area)**. Padding
is timed with complete conversion, premultiplied float filtering, independent
allocation, unrounded-alpha unpremultiplication, quantization, zero-alpha RGB
clearing and release. Normal/scalar maximum-one RGB/alpha and RMSE-0.25 gates
and native negative controls pass. This is the labeled backend, not FFmpeg CLI;
native memory is neither constrained nor measured.

Screen, independent repeat and regression reuse identical frozen production and
benchmark assemblies. Only changed inputs build; cached restore and verified
prepared children are reused. No new profile or original performance raster
was needed. Baseline allocation is 160 B/op; raw figures include harness overhead.

Windows and Linux each pass **3,126 Release tests**, including **896 live FFmpeg
comparisons**, plus **475 focused checks** in normal/scalar modes. Ten new cases
cover frozen pixels, support boundaries, admission/work, negative/tiny alpha,
scratch unwind, certified halves, cancellation, source edits and retained owners.
Consumer and all three examples pass through the **same actual dependency-free
Release package** on both OSes. Qualification runs once per platform after
passing means.

The [numeric evidence](warm-bicubictight4x2byte-packed-evidence.json) retains
baseline, screen, confirmation, regression and qualification. Stop this pair;
larger/unmatched work, public boundaries and final current-source uniform parity
remain open.
