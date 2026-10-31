# Lanczos3 centered word products

The independent confirmation rejects this candidate: **4.80% slower filtered**,
**4.69% across all actuals** and **4.79% normalized**. The preceding short screen
suggested 2.38%, 2.38% and 2.06% reductions under a predeclared substantial-arithmetic
exception. No speedup is accepted, no variant is attempted and no job is retimed.

| Existing RGBA16 reduction | Lokad µs | FFmpeg backend µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Qualified implementation | 726.956 | 507.572 | 1.432 / 1.446 | 138,437 |
| Rejected centered products | 761.822 | 507.621 | 1.501 / 1.518 | 138,437 |

This also refreshes the restored implementation's 513×259→256×129 cost:
**1.432× / upper 1.446**, 138,437 B/op. It remains a CPU gap. This is current cost,
not a causal improvement over the older unmatched measurement. All12 actuals,
both confidence views and both native brackets are retained.

The candidate centers unsigned premultiplied word samples at2³¹. Normalized
Q30 coefficients sum exactly to 2³⁰, allowing one 2⁶¹ correction per completed
channel and signed32×32→64 SIMD products. The L1 coefficient bound proves signed
prefix safety. Original integer rows are restored before double conversion.
There is no new storage, admission, owner, coefficient split or approximation.
Correctness passes; complete-operation performance determines rejection.

Both source variants pass 66 internal and 159 public focused checks normal/scalar
on Windows/Linux, plus Linux AVX512-off. New meaningful BigInteger coverage spans
premultiplication sign boundaries, negative weights, zero/full alpha, tails and
long support. It is retained after independently passing the restored production
implementation. Complete frozen managed frames remain exact; unchanged native
RGB/alpha max 1 / RMSE ≤.25, source, independent owners and idle-v2 gates pass.
The FFmpeg/zscale zimg reference retains equivalent normalization, filtering,
fresh output acquisition/release and one CPU; native allocation is unmeasured.

One candidate core build per OS uses cached harness, worker and selectors, with
three original/candidate actuals followed by twelve independent actuals each.
Exact qualified source/DLL/PDB/XML
restoration needs no core rebuild; only linked internal tests rebuild to validate
the retained fact. The unchanged 5,241-check/live and same actual Release-package/
three-example qualification is reused. No enlargement job or repeated full proof.

[Evidence](lanczos3-centered-word-products-rejected-evidence.json) pins all
observations, producers, ownership/correctness checks and restoration. Mirrored,
member-verified raw: `lanczos3-centered-word-products-rejected.tar.xz`, SHA256 ca9094fe3c6fcbaec1a5438e4b041346155c0664e1a9c2d15fbb69b7bcc7f718.
Close this arithmetic family. Full performance parity across 268 paths stays open.
