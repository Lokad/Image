# Warm RGBA8 bicubic width reduction with unchanged height

Independent quiet confirmation measures **1.535× FFmpeg's scaler backend**,
conservative upper **1.558×**, at **858.24 µs** and **465,375 B/op**. All twelve
actuals are retained and selected; both guards pass. The 4,089.49-µs / 7.306×
direct baseline falls **79.0%**. Allocations rise 199,931 B/op from 265,444 B/op,
about 75%, for optional operation-local workspace of 199,840 bytes.

One admission change routes RGBA8 width reduction with unchanged height through
the existing sixteen-row/tap cache. The same original 513×259 raster becomes
256×259. Arithmetic, sample precision, conservative rounding guards, work charges,
cancellation and direct fallback are unchanged. Other single-axis directions and
depths remain open. No state or output is cached between calls.

The frozen qualified original output is checked **byte for byte**. Its six alpha
differences from the independent integer reference are each certified exact
rational halves by development-only `BigInteger` arithmetic. These retain the
binary64 rounding already permitted by SPEC.md; no managed tolerance or pixel
change is accepted. RGB and all other samples match the rational reference.
The pinned original output hash is checked in setup and receipt validation.

The native baseline is explicitly **zimg, FFmpeg's zscale backend**, through the
shared original development adapter. Complete source conversion, four-pixel
clamped halo, premultiplied float filtering, independent planar/packed allocation,
unrounded-alpha unpremultiplication, straight byte quantization, rounded-zero-alpha
RGB clearing and release are timed. Padding adds 4.697% input area. Both modes
satisfy the existing native maximum-1 / RMSE-0.25 bounds: normal RGB/alpha RMSE
0.002242 / 0.010275, scalar 0.003884 / 0.011651. Source, geometry, independent
owners, operation counters and zero-alpha RGB checks pass.

The screen and twelve-actual confirmation use the same immutable production and
harness. One shared downsample regression screen passes at **1.729×** before
acceptance qualification. Only two distinct workloads and the existing original
byte raster are used; the project's retained original raster count remains two.
Prepared children are reused. Unscored correctness setup refusals and the source
manifest refusal remain retained; the latter starts no benchmark child.

Windows and Linux each pass **2,854 Release tests**, including **896 live FFmpeg
comparisons**, plus the consumer and all three examples through the **same actual
dependency-free Release package**. Seven new public cases exercise horizontal
admission, odd widths, cache budgets, source edits, held owners, negative lobes
and tiny/zero alpha. All 188 focused resize checks pass in normal/scalar modes on
both platforms. Final qualification runs once per platform for the accepted change.

The [numeric evidence](warm-bicubichorizontal8-rows-evidence.json) retains the
baseline, immutable screen/confirmation, regression control and qualification.
Stop tuning this fixed pair. Required material coverage and uniform current-source
parity remain open.
