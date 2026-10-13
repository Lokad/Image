# Warm RGBA8 bicubic upscaling with row reuse

Independent quiet confirmation measures **1.553× FFmpeg's scaler backend**,
conservative upper **1.578×**, at **4,721.50 µs** and **3,244,622 B/op**. Both
guards pass; all twelve actuals are retained, with eleven selected results.
The 11,519.60-µs / 3.741× baseline falls **59.0%**. Stop tuning this fixed pair.

The same original 513×259 RGBA8 raster becomes 1027×519. Managed pixels remain
exact against the independent rational reference. Source, geometry and held
outputs pass. The native baseline is explicitly **zimg, FFmpeg's zscale
backend**, with timed four-pixel clamped padding, full source conversion,
filtering, independent float-planar/packed allocation and release. It returns
premultiplied float samples; timed packing unpremultiplies using unrounded alpha,
then quantizes straight RGBA and clears RGB when rounded alpha is zero. Native
RGB/alpha maximum errors are 1, RMSE 0.003537 / 0.002372. Scalar output is also
bounded. The same explicit maximum-1 / RMSE-0.25 gates remain in force.

Ordinary straight byte output fails the initial precheck: 56 pixels retain RGB
beneath zero alpha. Clearing those alone leaves 447 pixels above the RGB bound,
maximum error 20, because alpha overshoot is clamped before unpremultiplication.
That refusal is retained without scored measurements. The correct float boundary
first measures 8.71 ms native because of per-sample general-purpose math calls.
Equivalent bounded quantization preserves every output byte and cuts native time
to about 3.04 ms. The apparently passing earlier comparison is superseded; the
accepted result uses the optimized native adapter.

One production change admits RGBA8 enlargement on both axes to the existing
operation-local horizontal row cache. It removes repeated filtering across
overlapping output rows while keeping original-order/tiny-alpha fallback near
ambiguous thresholds. Optional workspace stays **716×output width + 32×source
width + 128 bytes**, 751,876 bytes here, admitted and released within the call.
Allocations rise 648,853 B/op, about 25.0%. Tight budgets and unsupported hardware
retain the previous path. No state or output is cached between calls.

Windows and Linux each pass **2,810 Release tests**, including **896 live FFmpeg
comparisons**, and all three examples through the **same actual dependency-free
Release package**. Three new public cases cover enlargement, edges, exact cache
admission, edits and retained owners. The 144 focused resize checks pass with
normal/scalar code on both platforms. The [numeric evidence](warm-bicubicup8-rows-evidence.json)
retains refusals, the superseded native comparison, optimized baseline,
immutable screen/confirmation and qualification. Other required material work
and current-source uniform parity remain open.
