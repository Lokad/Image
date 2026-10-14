# Warm RGBA16 bicubic upscaling with row reuse

Independent quiet confirmation measures **1.505× FFmpeg's scaler backend**,
conservative upper **1.516×**, at **4,317.42 µs** and **5,016,578 B/op**. Both
guards pass; all twelve actuals are retained, with eleven selected results.
The 12,026.72-µs / 4.154× baseline falls **64.1%**. Stop tuning this fixed pair.

The same original 513×259 RGBA16 raster becomes 1027×519. Managed pixels remain
exact against the independent rational reference. Source, geometry and held
outputs pass. The native baseline is explicitly **zimg, FFmpeg's zscale
backend**, with timed four-pixel clamped padding, full source conversion,
filtering, independent float-planar/packed allocation and release. It returns
premultiplied float samples; timed packing unpremultiplies using unrounded alpha,
then quantizes straight full-word RGBA and clears RGB when rounded alpha is zero.
Native RGB/alpha maximum errors are 1, RMSE 0.048077 / 0.039863. Scalar output is
also bounded, at RMSE 0.052253 / 0.042284. The explicit maximum-1 / RMSE-0.25
native gates remain in force. No managed pixel tolerance is introduced.

One production change admits RGBA16 enlargement on both axes to the existing
operation-local horizontal row cache. Exact unsigned source products, binary64
intermediates, original-order/tiny-alpha fallback and unsigned full-word packing
are reused. Optional workspace stays **716×output width + 32×source width + 128
bytes**, 751,876 bytes here, admitted and released within the call. Allocations
rise 649,217 B/op, about 14.9%. Tight budgets and unsupported hardware retain
the previous path. No state or output is cached between calls.

Windows and Linux each pass **2,813 Release tests**, including **896 live FFmpeg
comparisons**, and all three examples through the **same actual dependency-free
Release package**. Three new public cases cover word enlargement, edges, exact
cache admission, edits and retained owners. The 147 focused resize checks pass
with normal/scalar code on both platforms. The [numeric evidence](warm-bicubicup16-rows-evidence.json)
retains the baseline, immutable screen/confirmation and qualification. Only the
two existing original performance rasters are used. Other required material work
and current-source uniform parity remain open.
