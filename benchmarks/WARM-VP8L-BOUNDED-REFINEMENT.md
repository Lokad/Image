# Exact bounded WebP entropy refinement

The existing 513×259 RGBA8 VP8L control retains libwebp exact=1, Q75/method4,
one CPU and complete independent output ownership. Normalization, encoding,
fresh output acquisition and release are timed. Full packets stay183,812B.
The shared compressed-VP8-alpha boundary is refreshed at98,590B. No image,
quality, effort or shape grid was added.

| Quiet run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Packet bytes, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| VP8L short | 217,165.21 | 122,245.20 | 1.776 / 1.873 | 48,232,248 | 183,812 |
| VP8L independent | 218,359.14 | 121,829.83 | 1.792 / 1.820 | 48,232,248 | 183,812 |
| VP8 alpha current short | 95,456.35 | 53,515.81 | 1.784 / 2.031 | 24,424,944 | 98,590 |

Independent VP8L cost is **1.792× / upper1.820**.
Elapsed time is3.5% lower than the prior quiet short screen, with unchanged
allocation. This is not an independently paired baseline speedup claim. All12
actual iterations and both versioned idle guards pass. Parity remains open.
The alpha row is a current cost screen, without a speedup/regression claim.

PopulationSum uses Int64 only when sum≤2²⁸, maximum≤sum and0≤entropy≤2⁵³.
Maximum starts at zero; mix is950,700 or627. Its nonnegative numerator is at most
(950+2×373)×2⁵²+500, strictly below Int64.MaxValue. Division by1000 therefore has
the same integer rounding as Int128. Other values keep the original arithmetic.
Run statistics, Q23 costs, Huffman decisions and tie order remain unchanged.
No buffer, heap allocation, cache or memory-admission operation is added.

All10,080 frozen paired costs/2,120,160 bins remain exact. Twenty-four added
comparisons cover the certificate's sum bounds and full-range fallbacks against
the independent Population(a+b) implementation. Cancellation precedes inputs;
zero heap allocation and unchanged source spans remain tested. A streaming
accumulator and long-zero SIMD variant gave no useful full-operation gain and
are discarded, including their prototype code. Their raw screens are retained.

Both systems pass4,904 Release checks each, including947 live FFmpeg comparisons,
plus225 affected scalar checks each. The same actual private0.2.0 package and all
three usage examples pass Windows/Linux. Published0.1.0 remains unchanged.
One selected immutable timing build served short, independent and affected-alpha
runs; native/generated/benchmark workers were reused. Full qualification followed
selection; unchanged scalar evidence was reused. No package was published.

[Evidence](warm-vp8l-bounded-refinement-evidence.json) identifies raw results,
guard/producer/package/archive hashes and precise invalidation. Historical affected
timings remain historical. Full253-path warm1× and replacement readiness stay open.
Next prioritize substantial predictor-scoring work; avoid further small tuning
of this primitive while other material opportunities remain.
