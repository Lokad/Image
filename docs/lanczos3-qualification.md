# Lanczos3 functional qualification

Development 0.2.0 adds `ResizeFilter.Lanczos3` to `Resize` and `ResizeToFit`.
Both pixel depths use radius-three windowed sinc, reduction antialiasing,
clamped edges and premultiplied alpha. The [pixel contract](../SPEC.md) defines
centers, cumulative Q30 coefficients, exact accumulation, rounding and saturation.
Identity preserves all samples; other zero-rounded-alpha pixels have zero RGB.

**4,426 tests pass per OS:** 1,613 internal, 1,881 public API and 932 live FFmpeg
comparisons on Windows/Linux. All **25 new Lanczos3 checks** also pass with
intrinsics disabled. The original decimal/Taylor-sine and BigInteger oracle
checks exact pixels for enlargement/reduction, transparency at both depths,
mixed axes, single rows/columns, edge impulses, constants and row-cache eviction.
Other controls cover aspect-fit rounding, orientation, source independence,
retained-result and replacement-peak admission, every growth refusal, cancellation
cleanup, subsequent success, local storage/work limits and extreme coordinates.

The same actual private 0.2.0 Release package passes both consumers, including
Lanczos3 at both depths with shared admission and independent result lifetimes.
All three packaged usage examples pass; the thumbnail example uses Lanczos3.
The previous twelve fixed text comparisons remain applicable: all production
text, font, drawing, memory-context and shaping sources are unchanged.

Four opaque native controls reuse the original 129×67 samples, reducing to 64×33
or enlarging to 257×135 at both depths. Explicit radius-three and edge extension
give RGB maximum error ≤1, RMSE ≤0.25 and exact alpha. Authored input padding
keeps these controls compatible with the pinned FFmpeg filter set. This graph
computes surplus border output and **must not be used for performance timing**.
Plain zscale instead mirrors edges and clamps alpha before unpremultiplication;
transparent overshoot requires the unclipped float-output adapter strategy.
No quality bounds or exact managed pixels were relaxed.

This is functional/memory evidence, **not performance parity or replacement
readiness**. Four new warm cases remain unmeasured: 8/16-bit reduction/enlargement
on the existing 513×259 rasters. No timing was taken. Existing filter kernels
and defaults are unchanged; published 0.1.0 remains immutable.

Retained SHA-256 identities:

- Qualified source: `f2807e5767fc507721ca9d26f522fccda023bf738492af7c5c09ff649ffdf069`.
- Windows producer: `165e6133987614cfffa77296cf0cdc2d46225d5454c1ab2b2c9632b4c24114f8`.
- Linux producer: `d2c25ee6dea30051dc20250901ff46dd1afe1f3fb17d7a2cbb5649c61e3f21cf`.
- Actual package: `4d41c84b4a6b34976bc8389b9d2b96e96a80e26a19dcd1769ef1407f79afd8b6`.
- `l3-qualified-windows.tar.gz`: `f9e58349d8c3c1231520c6a2bcf645e322ba0025c15eb6aa270012b6ede0f52a`.
- `l3-qualified-linux.tar.gz`: `a1b90b5f10378dff366a9afbb20e446ce619549f268df8094bfae186e768fd77`.
