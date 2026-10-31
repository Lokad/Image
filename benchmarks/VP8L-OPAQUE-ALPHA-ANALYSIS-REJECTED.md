# Opaque predictor alpha reuse: rejected

One fresh short pair saves only **1.58% raw elapsed** and is **0.35% worse
native-normalized**, below the 4.1% continuation scale. The candidate is removed
without an independent confirmation, loop variant or further profile.

| Existing complete opaque RGBA8 encode | Managed µs | Ratio / upper | Allocated B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: |
| Original, three actuals | 104,845.213 | 1.368 / 1.920 | 19,992,640 | 121,896 |
| Rejected candidate, three actuals | 103,186.557 | 1.373 / 1.871 | 19,992,640 | 121,896 |

The original 513×259 RGB samples with alpha 255, effort 4/quality 75 and labeled
libwebp exact=1 reference stay frozen. Both subjects normalize, encode, acquire
independent complete output and release it. Every packet, RGBA/alpha/hidden-RGB
pixel, source/ownership/native and idle-v2 check passes. No admission, capacity
or allocation changes. These short observations establish no accepted gain,
parity or causal regression against unmatched older costs; accepted costs remain.

One current profile on the immutable qualified core yields 762 workload samples:
PopulationSum 14.17%, color histogram 9.97%, predictor Visit 8.01%, prepared
prediction entropy 5.77%. These are diagnostic fractions, not component timings;
instrumented timings are discarded. Cached sampler/selector/worker are reused.
The old profile differs in two executed color files and an unexecuted decoder
file, precisely recorded by the scoped 29-file comparison.

Every predictor preserves opaque alpha. The candidate fills alpha-zero residual
counts once per leaf and omits a common integer alpha cost from mode ranking.
Complete parent/history/entropy histograms and strict ties remain exact. Four
prototype controls match full histograms, maps, residuals and root lifetimes;
all native complete packets remain exact. This structural reduction fails to
produce a material complete-operation gain and is fully removed.

Candidate passes 45 internal/229 public focused checks normal/scalar on both OSes
and with AVX512 disabled on Linux. After byte-exact source/core/PDB/XML restoration,
41 internal/229 public checks pass in the same modes. Four prototype-only tests
are removed; one useful fact retains 57,344 opaque predictor outputs. One core
and two internal assembly builds per OS; zero public/differential/harness/native/
selector builds. No repeated full or package run: unchanged 5,331 checks/OS and
the same actual package/three-example proof carry forward with the new fact,
5,332 applicable per OS. No fresh full 5,332 claim. Package stays
`228818d7e0026933b6cc800ee58dcece0603e945f7f46bcb02397a63184203f9`.

[Numeric evidence](vp8l-opaque-alpha-analysis-rejected-evidence.json) retains all
actuals, native brackets, confidence, original decisions and profile attribution.
Mirrored/member-verified raw `vp8l-opaque-alpha-analysis-rejected.tar.xz` SHA256
`b0604236bd608fce7cefddc5d221dd86e786a5fa153a44948af6a42909180dad` (1,520,092 B / 149 members)
preserves both producers, candidate/restored sources and checks, trace, cached
tools and reproducing scripts, with JFN1/CBA2 dependencies retained. Close opacity,
ranking, loop-width and inlining variants; the full 268-path objective remains.
