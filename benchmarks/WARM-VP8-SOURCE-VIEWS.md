# VP8 source conversion

Updated 2026-10-17. Reusing bounded views of source storage reduces complete
opaque encoding times by about12% and alpha-bearing times by about13–14%.
**Opaque cases remain parity gaps; both affected alpha cases are independently
reconfirmed below parity.** No image, setting or quality search was added.

| Existing selector | Prior / current Lokad µs | FFmpeg µs | Ratio / upper | Lokad B/op | Packet B, Lokad / FFmpeg |
| --- | ---: | ---: | ---: | ---: | ---: |
| EncodeOpaque8 | 29,381.20 / 25,741.07 | 15,496.34 | 1.661 / 1.744 | 2,536,348 | 333,580 / 72,380 |
| EncodeOpaque16 | 29,778.24 / 26,263.39 | 15,587.43 | 1.685 / 1.768 | 2,535,940 | 333,580 / 72,380 |
| Encode8 | 30,390.89 / 26,291.83 | 53,737.54 | 0.489 / 0.492 | 2,616,084 | 471,436 / 98,590 |
| Encode16 | 30,755.98 / 26,552.66 | 53,811.05 | 0.493 / 0.495 | 2,615,472 | 471,436 / 98,590 |

Opaque rows are qualified three-actual screens. Alpha rows are independent
twelve-actual confirmations; their preceding screens are also retained.
All actuals and both versioned quiet guards pass. Native allocation is unmeasured.
Managed B/op includes harness amortization; source views add no heap storage.

The same original513×259 RGBA8/16 rasters and opacity projections, managed/native
quality100/75, effort4 and ≤0.25dB source-quality gate are preserved. Packets,
decoded samples and exact normalized alpha are unchanged. Packet sizes above do
not establish compression-efficiency parity. Complete operations still include
normalization, color conversion, prediction/transform/entropy, fresh MemoryStream
growth, independent output ownership and disposal. No normalized raster is cached.

Only source conversion and its sample helper change: source storage is viewed
once, and clamped coordinates are shared across channels. Floating evaluation,
rounding, alpha weights, edge extension and every row work charge remain exact.
The first work check still precedes source access. Prediction, transform, entropy,
container output and resource admission are unchanged. Ten new scalar-reference
cases cover nonreplicated words, transparent/opaque rounding thresholds, thin/odd
padded geometry, tight work budgets and disposed-source failure precedence.

All **3,606 Release tests per OS** pass (1,318 internal,1,390 public,898 live
FFmpeg). The same actual package passes its consumer and all three examples on
Windows/Linux; packaged DLL, symbols and SPEC match. The screened production
binary stays byte-identical through qualification; the frozen measured harness
is restored after its metadata build. Unchanged scalar JPEG proofs are reused.

The other203 comparisons retain their executed paths, including49 passing proofs.
Two affected passing proofs are replaced by the confirmations above. Two original
rasters and the fixed207-case scope remain unchanged. Earlier entropy candidates
were rejected; a profiling-only I/O-guard failure is retained and never used as
performance evidence.

[Numeric evidence](warm-vp8-source-views-evidence.json) contains statistics,
quality/packet records and the precise source reconciliation. Raw archive:
`parity-p13-qualified-evidence.tar.gz`, SHA256
`d7bf41fd2d9f0d3e401d87a42ad9156cd978b027db78555e68622e5f5c5d8e6c`.
