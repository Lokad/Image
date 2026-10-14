# Warm RGBA16 bicubic 16×8

The unchanged adaptive kernel independently confirms **0.952× FFmpeg's scaler
backend**, conservative upper **0.958×**, at **465.90 µs / 202,229 B/op**.
All twelve actuals are retained, eleven selected; both quietness guards pass.
This establishes this fixed pair only; uniform required warm parity remains open.

The existing original 513×259 RGBA16 raster becomes 16×8, using the bounded
256-row adaptive cache. Every managed sample matches the independent rational
reference exactly. Source, geometry, independent owners, counters and zero-alpha
RGB controls pass. No additional raster or production optimization is introduced.

The native baseline is **zimg, FFmpeg's zscale backend**, with timed sixty-four-pixel
clamped padding (86.703% additional input area). Source conversion, premultiplied
float filtering, independent output allocation, unrounded-alpha unpremultiplication,
straight word quantization, rounded-zero-alpha RGB clearing and release are timed.
Normal/scalar native RGB maximum error is one, RMSE 0.0722/0.1021; alpha is exact.
The unchanged maximum-one / RMSE-0.25 comparison bounds pass. The separate scoped
vertical-enlargement allowance does not apply here. This is the labeled backend,
not FFmpeg CLI.

The three-actual screen and independent twelve reuse identical verified production
and benchmark assemblies. Only changed benchmark inputs build; generated children
and cached restore are reused. Production is unchanged, so the accepted adaptive
round's **2,942 Release tests per OS**, including **896 live FFmpeg comparisons**,
and the consumer plus three examples through the same actual Release package are
reused. No repeated full suite, package proof or profile is needed.

The [numeric evidence](warm-bicubic16x8-16-evidence.json) retains all actuals,
native brackets, controls, immutable identities, raw archive hashes and reused
qualification. Stop tuning this confirmed pair; tight budgets, larger/unmatched
supports and remaining material required/current-source coverage stay open.
