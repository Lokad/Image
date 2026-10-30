# Current CMYK sparse-2D decode

Current costs are superseded by [SIMD dequantization](WARM-JPEG-DEQUANTIZATION.md).
Original evidence and accepted behavior remain retained.

The existing 513×259 CMYK sparse-2D control independently confirms a
**2.850× / upper2.858 gap**, the largest current material gap.
This is a current cost, without a speedup/regression claim against the superseded
floating-inverse/nearest-device prototype.

| Complete resident JPEG → owned RGBA8 | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | JPEG / RGBA bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| CMYK sparse-2D, current fixed policy | 1,247.213 | 437.652 | 2.850 / 2.858 | 1,085,508 | 22,708 / 531,468 |

Managed normal/scalar RGBA and the original frozen native output are exact.
An independent oracle uses the original authored DC block means, two AC=4
coefficients, T.81 cosine weights and the documented Q14/Q16 rounding boundaries.
It also exactly matches caller-CMYK normal/scalar output. The old ideal-cosine
oracle differs at one canonical sample; nearest-device RGB differs at 200,427
samples, all by one. The packet, raster, projection, source blocks, native output
and settings stay unchanged. Only derived expected pixels and their hashes change.
No tolerance is relaxed: native max4/RMSE1.5/alpha255 remains, with observed0/0.

The complete owned decode, component materialization, inversion/device color,
output allocation and disposal boundaries stay equivalent. ICC/reorientation
are off. All twelve actuals, both native brackets, filtered/unfiltered confidence
envelopes and idle-v2 guards are retained. Native allocations are unmeasured.

Production and package inputs stay unchanged; reuse 5,163 checks/OS and the same
actual package/governed consumer/three examples. The new case-specific pixel
probe covers Linux normal/scalar. One small selector, two generator-correction
harness builds and one diagnostic tool build were needed; zero production/native
builds or repeated package/full suites. Earlier attempts stopped before scoring.
There is one completed confirmation, with no rerun. Temporary fixture overlays
were restored byte-exact, then the accepted derived oracles were installed after
preserving originals in the raw archives. The full generator reproduces them.

Prioritize a substantial distinct decoder primitive; rejected floating sparse
axis/cross-layout families stay closed. The full 268-path objective stays open.
[Evidence](current-cmyk-fixed-inverse-cost-evidence.json) and
[contract diagnosis](CMYK-FIXED-DEVICE-BENCHMARK-CONTRACT.md).
Mirrored/member-verified cmyk-fixed-oracle-current-cost-qualified.tar.xz, 122 members,
SHA256 5cc2a40231fbf315432baf5df6a9dcb8fc2a1538e48739c2f54ef6d7f3a78645; earlier failures/oracles and dependencies are retained.
