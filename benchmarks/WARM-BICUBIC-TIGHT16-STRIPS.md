# Warm RGBA16 bicubic with a tight working limit

Independent quiet confirmation measures **1.458× FFmpeg's scaler backend**,
conservative upper **1.467×**, at **828.06 µs / 264,469 B/op**. All twelve actuals
are retained, eleven selected; both guards pass. The direct baseline was
8,009.80 µs / 13.961×: the accepted change reduces its mean **89.7%**.

The existing original 513×259 RGBA16 raster becomes 256×129 with a working limit
of **1,331,224 bytes: input + output + 4 KiB**, excluding the full row cache.
One word specialization reuses six-column strips and ten source rows within the
same admitted 4-KiB stack workspace (3,640 bytes of arrays). Full UInt16 products
are unsigned 32-bit integers converted exactly to binary64. A compact source
window is decoded once per reused row; filtering and quotient guards operate on
paired independent outputs. No heap workspace, persistent state or output cache
is added. Allocation figures include harness overhead; the baseline is 264,320 B/op.

The error budget scales with the full word maximum, including squared color
products. Ordinary-alpha uncertainty evaluates the affected pixels in the original
tap order. Pair replay requires normalization at least 0.5 and computed normalized
alpha at least two. Exact tiny-alpha work declines the strip operation and unwinds
its frame/admission before complete original replay needs scratch. Odd word tails
also use original evaluation. Every frozen managed sample matches the independent
rational reference exactly; original work, bounded cancellation and owner contracts
remain. Other directions, supports and smaller budgets retain existing paths.

The baseline remains **zimg, FFmpeg's zscale backend**, with timed full-word source
deinterleaving, four-pixel clamped halo (4.697% extra input area), premultiplied
filtering, independent planar/packed output allocation, repacking and release.
Normal/scalar native maximum-one RGB/alpha and RMSE-0.25 bounds pass. The separate
vertical-enlargement allowance does not apply. This is the labeled backend, not
FFmpeg CLI; native memory is neither constrained nor measured.

The initial shared-depth trial confirms the word pair at 1.466×, but its byte
regression mean is **2.129×**; that production candidate is rejected without full
qualification. Restoring the exact byte kernel and adding the separate word
specialization resolves the regression. On the same final immutable build, byte
tight-memory resize independently confirms **1.855× / upper 1.876×**, at
**775.79 µs / 132,295 B/op**, with all twelve actuals selected and both guards
passing. A stale generated-child selection is retained unscored; exact case checks
reject its word label. Verified generated children are then reused directly.
No additional profile or original performance raster is needed.

Windows and Linux each pass **2,967 Release tests**, including **896 live FFmpeg
comparisons**, plus **316 focused checks** in normal/scalar modes. Nine new word
cases cover frozen pixels, unsigned products, admission, exact work, cancellation,
odd tails, tiny/hidden alpha, source edits and independent retained owners. The
consumer and all three examples pass through the **same actual dependency-free
Release package** on both OSes. Full qualification runs once per platform for the
accepted change.

The [numeric evidence](warm-bicubictight16-strips-evidence.json) retains baseline,
rejected trial/control, unscored selection refusal, final confirmations and
qualification. Stop both pairs. Larger/unmatched supports, other tight-memory
paths and remaining material required/current-source uniform parity stay open.
