# Byte bicubic enlargement: fixed error certificate

The existing tight-memory RGBA8 enlargement is **9.87% faster** in independent
confirmation, with **unchanged pixels and allocations**. A fixed conservative
clipped-quotient error bound removes the second vector division used for rounding
certification. Ambiguous bins, low alpha and portable execution keep their existing
fallbacks. This is a measured improvement; the **≤1× target remains open**.

| Complete warm single-CPU resize | Lokad µs | FFmpeg backend µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Verified original | 5,351.266 | 3,043.227 | 1.758 / 1.891 | 2,492,956 |
| Fixed certificate | 4,823.165 | 3,081.490 | 1.565 / 1.591 | 2,492,956 |

Scope is **513×259 RGBA8 →1027×519**, with the same **2,667,616-byte** local
working limit: source531,468 + owned output2,132,052 + **4,096 scratch bytes**.
Complete public Resize, admission, geometry, filtering, independent output and
disposal are timed. The unchanged **zimg backend used by FFmpeg zscale** includes
halo/packing/premultiplication, filtering, independent planar/packed outputs,
unrounded-alpha division, quantization and release. Native allocation is unmeasured.
No new input, shape, depth, filter or settings are introduced.

The short screen improves11.11% raw/11.41% normalized. Independent confirmation
improves9.87% filtered,11.20% across all actuals and10.99% normalized. Raw confidence
envelopes separate; normalized candidate upper**1.591** is below original
lower**1.662**. The recent original twelve is reused after a stable original short
(+3.20% filtered,+1.46% all; native−0.13%). Every score retains all actuals, filtered/
all statistics, reference brackets and passing idle-v2 guards.

Exact rational and frozen managed frames, source/output/native hashes and approved
native RGB/alpha maximum1/RMSE≤0.25 are unchanged. Three new independent rational
controls cover clipped colors and alpha0/1/2/255 through two-/one-axis enlargement
and strip tails. The rational interval proof also checks adversarial half-bin and
clipping endpoints. Existing original-order/tiny-alpha replay remains intact.
No new allocation, shared-budget charge, ownership or cancellation path is added.

**5,245 checks per OS** pass:2,112 internal,2,185 public and948 live differential.
Focused public179 pass normal/scalar on both OSes and with AVX512 disabled on
Linux. Linux normal executes the changed AVX512 path; Windows does not claim it.
The **same actual Windows Release package/DLL** passes both OS consumers,
three usage examples and the new exact byte-bicubic scratch/retained-owner check.
Published releases remain unchanged.

One two-second diagnostic attributes1,367/1,787 samples to the actual strip kernel;
all map to its5,089-byte same-process JIT. Two division sites justify this distinct
certificate change. Sampling shares are not component elapsed. The earlier word
guard experiment retained both divisions and did not test these clipped bounds.

One staging result loaded the original DLL through symlinked dependencies; the
actual loaded-DLL gate rejected it. It is retained and excluded from candidate
comparisons. Regular runtime files and a cached-driver hash check fix the staging.
The first package consumer compile used an internal accessor; public row access
fixes the check against the identical already-created package. No successful
timing, full suite or production build is repeated for these tooling corrections.

Only TightUp8 has a new cost. **TightHorizontalUp8/TightVerticalUp8** also call the
changed guard: functional qualification passes, but their older timings are
historical pending current confirmation. Other kernels and codec production
paths are unchanged. Stop variants of this primitive and resolve those named
uncertainties before choosing further optimization; all268 required paths remain.

[Numeric evidence](warm-bicubic-byte-fixed-certificate-evidence.json) includes
both independent producers, samples, bounds, proof and qualification. Raw
**bicubic-byte-fixed-certificate-qualified.tar.xz**, SHA256**cfd0ba5aa362f283b33b9a6b50061cb1dd7bb574aa58902128682b1d1b90668e**,
is mirrored and member-verified; it retains sources, actual builds/package,
profile/maps/JIT, fixtures/selectors by pinned dependencies, scores, guards and
failures. Reproduce the existing WarmBicubicResize/TightUp8 boundary with the
[reference tooling](../tools/WarmReference/README.md).
