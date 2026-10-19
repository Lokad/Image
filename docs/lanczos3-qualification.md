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
readiness**. The [subsequent warm baseline](../benchmarks/WARM-LANCZOS3-BASELINE.md)
qualifies transparent native controls and measures the four required paths,
all with material gaps. No timing was taken during this functional round. Existing filter kernels
and defaults are unchanged; published 0.1.0 remains immutable.
The subsequent [exact-quotient round](../benchmarks/WARM-LANCZOS3-QUOTIENT.md)
reduces complete time25.0–39.1%, qualifies4431tests/30scalar checks per OS and
preserves every original pixel; parity remains open.
The following [rounding-interval round](../benchmarks/WARM-LANCZOS3-INTERVAL.md)
saves another19.1–38.6%, qualifies4435tests/34scalar checks per OS and the same
actual private package on both systems. All four original frames remain exact;
the warm parity objective remains open.
The [exact SIMD accumulation round](../benchmarks/WARM-LANCZOS3-SPLIT.md) saves
another37.5–42.4%, qualifies4440tests/39scalar checks per OS and the same actual
private package on both systems. Scratch, work and cancellation boundaries are
unchanged; every original output remains exact. Warm parity remains open.
The [certified SIMD normalization round](../benchmarks/WARM-LANCZOS3-NORMALIZE.md)
saves another16.6–47.3%, qualifies4445tests/44scalar checks per OS and the same
actual private package on both systems. All four original frames remain exact;
admitted storage, work, cancellation and the parity objective are unchanged.
The [exact SIMD horizontal round](../benchmarks/WARM-LANCZOS3-HORIZONTAL.md)
saves another10.5–30.8%, qualifies4448tests/47scalar checks per OS and the same
actual private package on both systems. Original frames, working storage,
work and cancellation remain unchanged; warm parity stays open.
The [AVX2 byte-product round](../benchmarks/WARM-LANCZOS3-BYTE-PRODUCTS.md)
cuts Reduce8 another13.3%, qualifies4448tests/47scalar plus47AVX512F-off checks
per OS and the same actual package on both systems. All original frames stay
exact; storage, work and cancellation are unchanged. The paired trial is rejected.
The [eight-channel vertical round](../benchmarks/WARM-LANCZOS3-WIDE-VERTICAL.md)
saves another10.8–20.5%, with the same4448tests/47scalar/47AVX512F-off checks and
actual package on both OS. Original frames, storage and work remain unchanged.
The3.9% word-product trial is parked; parity stays open.
The [four-tap word round](../benchmarks/WARM-LANCZOS3-WORD-FOUR-TAP.md) saves
26.9% on RGBA16 reduction and6.4% on enlargement, adding no scratch. The same
4448tests/47scalar/47AVX512F-off checks and actual package pass on both OS, now
including1031-tap support in the independent row theory. Four original frames
remain exact; unchanged byte timing paths reuse their preceding build evidence.
The [four-tap byte round](../benchmarks/WARM-LANCZOS3-BYTE-FOUR-TAP.md) then saves
16.7% on RGBA8 reduction and2.7% on enlargement, with the same qualification,
exact frames and unchanged storage/work/owners. Only byte timing rows refresh;
word helper paths retain their identified build evidence. Parity remains open.

Retained SHA-256 identities:

- Qualified source: `f2807e5767fc507721ca9d26f522fccda023bf738492af7c5c09ff649ffdf069`.
- Windows producer: `165e6133987614cfffa77296cf0cdc2d46225d5454c1ab2b2c9632b4c24114f8`.
- Linux producer: `d2c25ee6dea30051dc20250901ff46dd1afe1f3fb17d7a2cbb5649c61e3f21cf`.
- Actual package: `4d41c84b4a6b34976bc8389b9d2b96e96a80e26a19dcd1769ef1407f79afd8b6`.
- `l3-qualified-windows.tar.gz`: `f9e58349d8c3c1231520c6a2bcf645e322ba0025c15eb6aa270012b6ede0f52a`.
- `l3-qualified-linux.tar.gz`: `a1b90b5f10378dff366a9afbb20e446ce619549f268df8094bfae186e768fd77`.
