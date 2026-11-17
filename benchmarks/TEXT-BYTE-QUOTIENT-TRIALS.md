# Byte text-channel quotient trials

Neither implementation is promoted. Both preserve exact integer results, but
neither complete ordinary RGBA8 screen establishes a useful gain. Production,
the qualified package and current comparative costs remain unchanged.

| Complete ordinary RGBA8 draw /current immutable baseline →private DLL | Baseline →candidate µs | Paired ratio interval | Measured GC B/op |
| --- | ---: | --- | ---: |
| Shared scalar reciprocal with integer correction | 46.537900 →47.536064 | 0.924295–1.128807 /unpromising | 75,864 →75,864 |
| Shared SIMD RGB arithmetic and correction | 46.470450 →46.738975 | 0.842637–1.200601 /inconclusive, unpromising | 75,864 →75,864 |

Three RGB channels divide by one denominator. The hypothesis prepares one
binary64 reciprocal, estimates each quotient, then uses exact UInt64 product/
remainder comparisons to correct an estimate at most one integer away. For
`M=65535`, `D=M*(255*sa+da*(M²-sa))`; valid rounded numerators satisfy
`N<=255D+floor(D/2)<2^64`. The conservative quotient-error bound is below 2^-40,
and candidate products fit UInt64. The floating estimate is never a published
sample. Full sixteen-bit color/coverage, alpha rounding and all word paths remain.

The finite 251,576-case arithmetic model includes estimates needing correction
in both directions. Compiled boundary tests exercise actual helpers, while the
existing independent BigInteger composition oracle covers full source precision.
Each prototype passes **252 internal** checks per normal/AVX2-disabled/scalar mode
and **177 public** checks per normal/scalar mode on **both OSes**, plus four frozen
drawing preflights. The direct SIMD-helper test runs only when its required ISA
is enabled; the original portable division body and public rational/rendering
checks run in all modes. Ownership, refusal, cancellation and cleanup remain exact.

Actual scalar code removes three integer divides but grows **223→410 B**, with
seven scalar floating conversions and correction branches. This motivates the
coherent SIMD revision, which shares numerator arithmetic, conversions and exact
mask corrections across RGB. Its actual selected helper is **388 B**, with three
floating conversions and no variable integer divide. The raster caller remains
2,596 B. These are generated instructions, not phase costs or causal CPU savings.
Both unpromising screens remain; no third neighboring variant follows.

Each screen retains all three alternating 4,096-operation pairs, complete fixed
warmups, natural nonconcurrent GC, CPU0 and idle-v2 guards. Every operation keeps
its original untimed reset and exact full-frame check, with two timestamp reads.
Original font/text/options, ImageSharp correctness-only observations and owner
checks remain. No reference timing or historical-clock subtraction establishes
parity. Public allocations are **75,840 B plus the unchanged 24 B adapter**;
admitted capacity, retained storage and cleanup do not change. Physical/native/
JIT peaks remain unmeasured.

Two private core/embedded builds; zero canonical, VM, public, harness, reference,
fixture or package builds. Reuse 6,823 checks per OS/SAME actual package/all three
examples for unchanged production. No independent confirmation, passing-path
retiming or broad/package campaign. Code-generation captures issue no sample
commands and discard every instrumented clock. One transfer list wrongly expects
the intentionally omitted second baseline capture; both actual files already
transferred. Correct only transport metadata, with zero work replay.

[Evidence](text-byte-quotient-trials-evidence.json) retains proofs, sources, exact
artifacts, every actual, code generation and qualification scope. Mirrored/member-
verified `text-byte-quotient-trials.tar.xz`: **3,712,388 B /124 members**, SHA256
`bcc859c3f834c21094011ab7fac2d19eb2d9fd112a82ea7d459dda33d9073140`.
Archived `tqr1229-*` /`sqr1230-*` recipes and declared dependencies reproduce the
round. No renderer dependency or new required workload is added. Passing paths
stop; the full 289-path objective and final immutable-package refresh stay open.
