# Warm RGBA8 tight-memory bicubic beyond 256 taps

Independent quiet confirmation measures **0.792× FFmpeg's scaler backend**,
conservative upper **0.803×**, at **505.31 µs / 249 B/op**. All twelve actuals
are retained and selected; both guards pass. The direct baseline was
8,347.12 µs / 13.085×: the accepted change reduces its mean **93.9%**.

The existing original 513×259 RGBA8 raster becomes 8×4 with a working limit
of **535,692 bytes: input + output + 4 KiB**, excluding the full row cache.
Horizontal/vertical admission bounds are 258/260 taps. One production primitive
extends byte streaming capacity from 256 to **304 taps**; fixed arrays grow
from 3,456 to **3,936 bytes** within the unchanged 4-KiB charge. A source row
is filtered once per output column and feeds at most sixteen accumulators.
Separate original clamped-edge contributions retain order; only the last
horizontal result is stored.

Exact byte products and unrounded binary64 preserve every frozen sample and
match the independent rational reference exactly. The error bound increases
to `2^-40*304^2`; uncertainty still replays original tap order, and tiny alpha
unwinds before exact scratch. Work, cancellation, checked relative offsets,
full absolute coordinates, source and independent owner contracts remain.
No heap workspace, full support cache or output cache is added. Other heights,
supports, directions, hardware and smaller budgets retain prior paths.
The existing tight 16×8 byte regression mean passes at **1.088×**, with 633 B/op;
its three-sample upper 1.262× is a screen only.

The baseline is **zimg, FFmpeg's zscale backend**, using the existing native
macro and **128-pixel clamped halo (198.069% additional input area)**. Padding
is timed with complete conversion, premultiplied float filtering, independent
output allocation, unrounded-alpha unpremultiplication, quantization, zero-alpha
RGB clearing and release. Normal/scalar RGB/alpha maximum-one and RMSE-0.25
checks and native negative controls pass. This is the labeled backend, not
FFmpeg CLI; native memory is neither constrained nor measured.

Screen, independent repeat and regression reuse the same frozen production and
benchmark assemblies. Only changed inputs build; verified prepared children
and cached restore are reused. No new profile or original raster was needed.
Baseline allocation is 256 B/op; raw figures include harness overhead.

Windows and Linux each pass **3,106 Release tests**, including **896 live FFmpeg
comparisons**, plus **455 focused checks** in normal/scalar modes. Nine new
cases cover frozen pixels, support boundaries, admission/work, negative/tiny
alpha, scratch unwind, cancellation, source edits and retained owners. The
consumer and all three examples pass through the **same actual dependency-free
Release package** on both OSes. Qualification runs once per platform after
passing means.

The [numeric evidence](warm-bicubictight8x4byte-stream-evidence.json) retains
baseline, screen, confirmation, regression and qualification. Stop this fixed
pair; larger supports, other material boundaries and final current-source uniform
parity remain open.
