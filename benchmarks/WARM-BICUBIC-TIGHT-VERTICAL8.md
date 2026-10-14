# Warm RGBA8 vertical bicubic with an unchanged width

Tight-memory 513×259→513×129 bicubic independently confirms **1.822× FFmpeg's
scaler backend**, conservative upper **1.838×**, at **1,063.64 µs / 264,995 B/op**.
All twelve actuals are retained and selected; both guards pass. The direct baseline
was 8,145.18 µs / 13.847×: the change reduces its mean **86.9%**.

The working limit is **800,272 bytes: input + output + 4 KiB**, excluding the
existing row cache. Horizontal/vertical admission bounds are 5/10 taps. The
vertical-first byte primitive now also admits an unchanged width when that cache
is unavailable. Four output rows reuse five source-column slots in **2,016 bytes
of fixed arrays** within the unchanged 4-KiB charge. Full coefficients, exact byte
products, unrounded intermediates, tap order, precision guards, original rounding
replay and tiny-alpha replay after workspace release are preserved. Work,
cancellation, checked loads, source and independent owner contracts remain.

The faster existing row cache remains preferred when its hardware, array and
memory requirements fit. It needs `(85 doubles + 17 ints) * width + 128 bytes`,
plus exact-alpha scratch. Boundary tests preserve pixels immediately below, at
and above admission. Default memory is independently reconfirmed at **1.287× /
upper 1.301×**, **756.93 µs / 648,992 B/op**. All twelve actuals are retained and
selected, and both guards pass. Both confirmations and the regression screen
reuse identical frozen production and benchmark assemblies. Tight baseline
allocation is **264,840 B/op**; figures include harness overhead.

The managed gate retains the frozen original binary64 output, including **25
independently certified exact rational alpha halves** already allowed by SPEC.
Every managed output byte remains exact; no managed tolerance is added. The native
comparison is **zimg, FFmpeg's zscale backend**, with the existing **four-pixel
timed clamped halo (4.697% additional input area)**. Complete packing, premultiplied
float filtering, independent allocation, unrounded-alpha unpremultiplication,
byte quantization, zero-alpha RGB clearing and release are timed. Native normal/
scalar maximum-one RGB/alpha and RMSE-0.25 gates and negative controls pass.
This is the labeled backend, not FFmpeg CLI; native memory is neither constrained
nor measured.

Windows and Linux each pass **3,214 Release tests**, including **896 live FFmpeg
comparisons**, plus **563 focused checks** in normal/scalar modes. Nine new cases
cover frozen pixels at 4095/4096-byte scratch, work/admission, source edits,
retained owners, cancellation, one-column and odd-width tails, low alpha, exact
scratch unwind and the row-cache admission boundary. Initial preparation stopped
because its receipt validator lacked the new case's existing 25-half mapping;
that mapping was added before scored timing, preserving the frozen hash and all
pixel/comparison gates. The failed preparation log is retained.

The consumer and all three examples pass through the **same actual dependency-free
Release package** on both platforms. Full qualification runs once per platform
after passing performance means. Only changed inputs build; verified prepared
children, cached restore and native identities are reused. No new profile or
original performance raster is needed.

The [numeric evidence](warm-bicubictightvertical8-cachepref-evidence.json) retains
baseline, tight/default confirmations, regression and qualification. Both pairs
are confirmed; other material coverage and final current-source uniform parity
remain open.
