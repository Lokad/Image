# VP8 luma4 preparation and delayed publication

The widened Q17 quantizer now returns one detached register/stack value. Luma4
reuses its nonzero/last/Flat facts and the existing checked residual-cost body.
Raster writes follow the existing rate/header bound; selected levels are written
only for the strict winner. Flat means at most three nonzero AC coefficients.
Mode order, ties, bounds, uniform-score reuse, cancellation and exact packets stay
unchanged. AVX2/scalar and trellis retain their actual production paths.

One counterbalanced comparison confirms **286.80 µs / 2.32% elapsed improvement**
on the frozen 513×259 opaque photograph, Q75/effort4: 12,335.69→12,048.89 µs.
Twelve alternating pairs × sixteen complete operations per subject, all retained;
ratio confidence **0.97450–0.97901**. Fresh output creation/growth, public encoding,
independent ownership and disposal remain timed. Equal setup/JIT/validation are
excluded. Exact 17,688-byte packets, unchanged source, producer and quiet guards
pass. This is a scoped managed-build gain, not full FFmpeg parity or an alpha gain.

| Existing complete 513×259 control | Lokad µs | FFmpeg/libwebp µs | Ratio / upper | Managed B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Opaque photograph, effort4 | 11,783.72 | 9,695.15 | 1.215 / 1.221 | 809,225 | 17,688 |
| RGBA8 + ALPH | 63,660.45 | 56,632.53 | 1.124 / 1.220 | 7,165,880 | 98,590 |
| RGBA16 + ALPH / normalization | 67,613.09 | 57,748.29 | 1.171 / 1.183 | 7,165,472 | 98,590 |
| Opaque RGBA8 | 16,831.51 | 15,535.80 | 1.083 / 1.087 | 1,201,552 | 72,380 |
| Opaque text/diagram, effort4 | 6,730.70 | 5,824.45 | 1.156 / 1.161 | 741,137 | 7,100 |
| Diagram, effort2 / distortion | 3,072.12 | 2,507.01 | 1.225 / 1.233 | 741,386 | 7,330 |
| Diagram, effort6 / trellis | 12,878.10 | 9,921.23 | 1.298 / 1.307 | 754,344 | 6,818 |
| Opaque RGBA16 / normalization | 17,971.95 | 15,613.53 | 1.151 / 1.157 | 1,201,552 | 72,380 |

All eight costs remain gaps. Alpha standalone raw points increase from
61,821.92→63,660.45 µs and 62,712.50→67,613.09 µs at byte/word depth; native clock
movement is +6.08%/+7.41%. These adverse observations remain recorded without a
causal alpha gain/regression claim or favorable resampling. Earlier
[shared residual preparation](VP8-RESIDUAL-PREPARATION.md) retains its own scoped
gain; its costs are superseded here. Unchanged VP8L and other families reuse valid
evidence. Other affected VP8 boundaries marked H remain historical.

Every paired subject allocates 809,072 B/op. The standalone photograph diagnostic
records 809,225 B/op at 32 operations/three GCs versus the prior 16-operation/
zero-GC diagnostic; no causal allocation claim follows that unequal boundary.
No substantial capacity, owner, pool or cache is added. The small fixed value
fits the existing coarse 16 KiB macroblock allowance. Retained operation state is
unchanged; admitted/physical/stack peaks and native allocations are unmeasured.

Windows/Linux each pass **5,639 full/live checks** (2,404 internal, 2,287 public,
948 differential), 852 internal/357 public focused checks per normal/scalar mode,
and 26 actual AVX512-off quantization/rate/decision checks. Review then caught a
precedence error in the new test generator. Explicit grouping, all sparse
positions and zero/saturation assertions strengthen its intended families;
the three corrected theories pass normal/scalar on both OSes. Only internal test
assemblies rebuild; all other 5,636 tests and production bytes remain unchanged,
so their proof is reused. Both test epochs are retained. Theories independently
check scalar-derived quantization, prices/Flat, detached lifetime, tails, invalid
range/context and zero allocation/admission change. Scalar public/native corpus
exercises the actual fallback; the new hardware-specific theories return there.

One core and two internal test builds per OS; no new harness, native worker,
selector or paired driver. The **same actual private 0.2.1 Release package**
passes its consumer and all three examples on Windows/Linux. Published packages
are unchanged and publication is postponed.

Reproduce with retained `qdp300-*.py`, Release/differential, affected normal/scalar/
AVX512-off checks and the actual package consumer. Existing frozen inputs,
workers, every actual/bracket/guard and the unchanged paired driver are pinned.
Use the dedicated quiet single-CPU VM. [Evidence](vp8-luma4-preparation-evidence.json)
contains identities, costs, adverse observations, qualification and raw manifests.

Raw `vp8-luma4-preparation-qualified.tar.xz`: 6,063,440 B / 339 members,
SHA256 `64155fe69673aa19b0e6ed909e5bb82218c5d07b16db77450ca51d8cfb861d4b`. Member-verified locally and on the VM; includes
original/selected source and modules, both test epochs, all checks/actuals,
paired driver and the actual package. Dependencies retain unchanged framework,
controls, workers and qualified history.
