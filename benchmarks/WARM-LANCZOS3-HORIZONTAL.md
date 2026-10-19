# Exact SIMD Lanczos3 horizontal products

Four-channel horizontal products cut complete Lanczos3 time **10.5–30.8%**
from the preceding SIMD normalization. All four existing cases remain measured
gaps against independently confirmed warm single-CPU parity.

| Complete operation | Lokad µs | Reference µs | Ratio / conservative upper | Lokad allocated B/op | Time saved |
| --- | ---: | ---: | ---: | ---: | ---: |
| Reduce8 | 1,576.59 | 490.31 | 3.215 / 3.831 | 146,640 | 29.5% |
| Reduce16 | 1,565.63 | 505.77 | 3.096 / 3.554 | 146,625 | 30.8% |
| Enlarge8 | 8,018.68 | 4,087.69 | 1.962 / 2.107 | 351,912 | 10.5% |
| Enlarge16 | 8,903.02 | 3,941.51 | 2.259 / 2.559 | 4,617,090 | 11.2% |

Premultiplied RGBA16 samples reach `65535^2=4294836225`, fitting UInt32 and
exceeding Int32. SIMD first multiplies samples by original alpha, using one in
the alpha lane. **Unsigned** widening to Int64 preserves the full products
before signed weighting. Existing axis magnitude bounds certify every product
and partial sum below `65535^2*2^31 < 2^63`. Four Int64 lanes therefore reproduce
the original exact arithmetic, including negative weights. Each read covers
exactly one pixel (4/8 bytes); unsupported hardware retains the scalar path.

Three new checks compare unrounded rows against independent decimal axis
coefficients and BigInteger sums. They cover full unsigned precision, negative
weights, tiny alpha, row ends, source immutability, output sentinels, unchanged
logical work/storage and canceled publication. All four protected full frames
remain exact on both systems. Coefficients, vertical accumulation, normalization,
scratch, admission, work and cancellation boundaries are unchanged. No array,
owner or cache was added; small allocation differences are harness amortization.

One immutable producer and the prepared harness, selectors, original fixtures
and native quality proofs were reused throughout. Reference remains explicitly
**zimg, FFmpeg's zscale backend**, with complete equivalent boundaries and gates
from the [baseline](WARM-LANCZOS3-BASELINE.md). All eight scored quiet guards pass.
Both native brackets and selected/all-actual confidence views are retained.
No long confirmation was run above one; no profile or workload grid was added.

Qualification: **4,448 Release tests per OS** (1,635 internal, 1,881 public and
932 live FFmpeg), **47 intrinsics-disabled Lanczos checks per OS**, the same
actual private 0.2.0 package consumer and all three packaged examples on both
systems. Twelve font comparisons retain unchanged paths. Published 0.1.0 is
immutable. Only these four timings refresh; the other 227 numeric rows retain
their identified historical builds. Parity and replacement readiness stay open.

[Numeric evidence](warm-lanczos3-horizontal-evidence.json) includes samples,
allocations, product bounds, identities and reuse. Qualified source:
`f60c4f38e88af86169d109c094fef696326c197c7f4add987a6b9b3b11c3eafd`.
Raw `l3h-qualified-windows.tar.gz`:
`0f375c4e53a623a16ca228ad94662b7a520c0a63c301fd1f3a5663cc1cf9e981`;
`l3h-qualified-linux.tar.gz`:
`f9aeef5d815cb824cfb16c8a07edd2bbf5e93813604f4dfcfa6260b4e20c15b5`.
Unchanged dependencies, fixtures and native worker remain in the baseline archive.
Next: paired horizontal taps in wider vectors, preserving exact subset sums
and the single-tap tail, with Reduce8 screened before extending the experiment.
