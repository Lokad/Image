# Warm RGBA16 bicubic width enlargement with unchanged height

Independent quiet confirmation measures **1.960× FFmpeg's scaler backend**,
conservative upper **1.973×**, at **2,801.88 µs / 2,232,080 B/op**. All twelve
actuals are retained, eleven selected, and both guards pass. The three-actual
screen measures 1.985×; its wider confidence bound motivates the independent
repeat. Production is unchanged. No optimization, new profile, repeated full
suite or package proof is needed for this fixed pair.

The same original 513×259 RGBA16 raster becomes 1027×259. The benchmark extends
the existing original adapter's word macro and geometry selection; its C source
is unchanged. Every managed sample matches the independent rational reference
exactly. Source, geometry, independent held outputs, operation counters and
zero-alpha RGB controls pass. No half-rounding exception is needed.

The native baseline is explicitly **zimg, FFmpeg's zscale backend**, with complete
conversion, timed four-pixel clamped halo, premultiplied float filtering,
independent output allocation, unrounded-alpha unpremultiplication, straight word
quantization, rounded-zero-alpha RGB clearing and release. Padding adds 4.697%
input area. Both modes satisfy the unchanged native maximum-1 / RMSE-0.25 bounds:
normal RGB/alpha RMSE 0.043615 / 0.037497, scalar 0.046736 / 0.039308. This is
the labeled backend, not FFmpeg CLI. Managed pixels remain exact.

Screen and repeat use the same immutable qualified production/harness. Only
changed benchmark inputs and the selected generated children are built. One
workload uses one of the two existing original rasters. The [preceding production
qualification](WARM-BICUBIC-HORIZONTAL-UP8-ROWS.md) is reused: Windows/Linux each
pass 2,896 Release tests including 896 live FFmpeg comparisons, plus the consumer
and all three examples through the same actual Release package.

The [numeric evidence](warm-bicubichorizontalup16-evidence.json) retains both runs,
native controls, exact identities and the reused qualification. Stop tuning this
pair. Other directions, larger supports, tight budgets and remaining material
required/current-source uniform parity stay open.
