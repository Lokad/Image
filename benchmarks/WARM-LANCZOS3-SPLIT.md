# Exact SIMD Lanczos3 vertical accumulation

Splitting vertical accumulation into signed limbs cuts complete Lanczos3 time
**37.5–42.4%** from the preceding rounding-interval implementation. The four
existing cases remain gaps against independently confirmed single-CPU parity.

| Complete operation | Lokad µs | Reference µs | Ratio / conservative upper | Lokad allocated B/op | Time saved |
| --- | ---: | ---: | ---: | ---: | ---: |
| Reduce8 | 2,690.16 | 492.00 | 5.468 / 6.536 | 146,635 | 37.5% |
| Reduce16 | 2,713.33 | 507.50 | 5.346 / 5.726 | 146,612 | 37.6% |
| Enlarge8 | 16,998.78 | 4,066.65 | 4.180 / 4.470 | 351,925 | 42.4% |
| Enlarge16 | 17,909.37 | 3,942.46 | 4.543 / 4.705 | 4,616,716 | 41.1% |

Existing axis plans already certify `sum(abs(weights))<=2^31`, hence horizontal
RGBA16 values have absolute magnitude at most `65535^2*2^31`. Write each value
as `hi*2^32+lo`, with signed Int32 `lo` and `hi`; the latter's magnitude is at
most 2,147,418,113. Every prefix of either weighted limb sum stays within
`2^31*2^31=2^62`, safely inside Int64. AVX2 performs four signed products and
accumulations per vector. Recombination into Int128 at output is exact.

Two contiguous Int64 planes reuse the original admitted Int128 array. Other
processors keep the original Int128 accumulation. There are no new arrays,
owners, allowances or caches. Work charges and 4,096-channel cancellation
boundaries are unchanged. Normalization, coefficients and pixel rules remain
unchanged; all four protected complete outputs match on Windows/Linux. Small
allocation differences reflect amortized diagnostic overhead.

Five new checks compare independent BigInteger sums at signed limb extrema,
every prefix, mixed deterministic rows, cancellation and the vector/poll boundary.
All run through both SIMD and intrinsics-disabled production paths. The same
frozen producer, harness, compiled selectors, original inputs and native worker
were reused for the four shorts. Native reference remains explicitly **zimg,
FFmpeg's zscale backend**, with the equivalent complete boundaries and quality
gates from the [baseline](WARM-LANCZOS3-BASELINE.md).

Eight scored quiet guards pass; one Reduce16 preguard refused I/O wait and
launched nothing before a fresh passing retry. Both native brackets and both
selected/all-actual confidence views are retained. No long confirmation was run
above one. Two optional Reduce8 diagnostic preguards also refused I/O wait;
no profiler or benchmark was launched and no profile conclusions follow.

Qualification: **4,440 Release tests per OS** (1,627 internal, 1,881 public and
932 live FFmpeg), **39 intrinsics-disabled Lanczos checks per OS**, the same
actual private 0.2.0 package consumer and all three packaged examples on both
systems. Twelve font comparisons retain valid unchanged paths. Published 0.1.0
is immutable. The other 227 numeric rows retain their identified historical
builds; there is no broad current parity or replacement-readiness claim.

[Numeric evidence](warm-lanczos3-split-evidence.json) retains samples, allocations,
arithmetic bounds, identities, failed guards and reuse. Qualified source:
`0390d7a3f11a2bc069f08374c53c22f31289ef603f6e7aaf73221d5676c1b74e`.
Raw `l3s-qualified-windows.tar.gz`:
`4de80b01b7bd9e9c86c0f3951f3071843e0c062ed9e14c301d7e70c11fdcbe90`;
`l3s-qualified-linux.tar.gz`:
`27853699c8fbf6f2c7d8429f02c443d21875fe034280f61ebce9a77057b6b47c`.
Unchanged runtime dependencies, fixtures and worker remain in the baseline archive.
Next: a bounded Reduce8 diagnostic, then its dominant arithmetic; investigate
shared-denominator SIMD normalization only with rigorous exact-pixel certificates.
