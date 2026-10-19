# Fused caller-RGB JPEG MCU preparation

Complete RGB420 sample encoding improves **33.7%**, from
**3,162.12→2,097.34µs**. The existing74,040-byte packet,
decoded pixels and matched source quality remain exact. The current short ratio
is **1.328× / upper1.408×** FFmpeg, so parity remains open. No long confirmation
is run on this clearly above-one result. The gray dispatch control remains near
its prior time with unchanged bytes/allocations.

| Complete caller-sample encode | Managed / reference µs | Ratio / upper | Managed allocated B/op | Compressed bytes, managed / reference |
| --- | ---: | ---: | ---: | ---: |
| RGB8 → JPEG420 | 2,097.34 / 1,579.06 | 1.328 / 1.408 | 297,894 | 74,040 / 52,093 |
| Gray8 → one-component JPEG | 1,252.22 / 1,077.49 | 1.162 / 1.422 | 155,007 | 58,823 / 58,910 |

The independently implemented primitive prepares four Y and two chroma blocks
in one16×16MCU traversal. Packed RGB normalization needs16row copies and32
eight-pixel channel extractions instead of64/96; channel vectors serve Y,Cb,Cr.
The384-byte result stays within the original admitted8-KiB workspace. It retains
the binary64 matrix expressions, dy/dx chroma summation order, final-only rounding,
clamped edges, component order and original logical work charges. No RGBA frame,
input copy, extra heap allocation, quantizer or entropy change is introduced.

Only already-admitted noncancelable adaptive RGB420 selects this path on supported
safe AVX-512 hardware. Whole-operation work was admitted before preparation;
original block-level charges remain. Other sampling, scalar, small/tight-budget,
callback and cancelable paths retain their row sampler and failure behavior.
Caller samples/padding are unchanged and output owners stay independent.

An independent scalar sample reference checks all384bytes at full, padded,
right/bottom-clamped MCUs. A full513×259padded RGB test matches both an opaque
adaptive RGBA encode and the exact pre-change qualified RGB packet hash. Its
background is explicit so both routes use adaptive coding: the initial null-
background RGBA comparison selected fixed entropy and was an invalid test
assumption. The archived pre-change encoder reproduces the same differing route
packets, proving no candidate regression. Normal/scalar native and frozen source
quality/alpha gates also pass before/after the timed operation.

Two three-actual screens retain all six actuals, native pre/post50ms brackets and
both idle-v2 guards. The verified immutable producer, existing bc1d0e6a... harness,
selectors, native workers and fixtures are reused. There is no quality retuning,
new benchmark input/profile grid, profiling run or native/harness/selector build.
The two completed logs/guards are reused after correcting raw-receipt camelCase
sample fields and nested quality hashes in postprocessing; no timed run repeats.
Native workloads keep complete normalization, encode and owned packet release.
Gray uses the approved SIMD libjpeg-turbo3.1.4.1 baseline; RGB uses FFmpeg. Quality
numbers69/69gray and69/nativeqscale5RGB keep their frozen source-error matches;
equal numeric quality is not the matching rule.

All **4,169tests/OS** (1,415internal,1,826public,928liveFFmpeg), **109 affected
intrinsics-disabled checks/OS** (82public,27internal), and the **same actual Windows
private package** consumer/all3examples pass on Windows/Linux. Production remains
safe managed dependency-free .NET10. Precisely the two caller-sample rows are
replaced; other current paths and69passing proofs are reused. No new material
performance case is added. The216numeric/27pending checklist and full objective
remain open; passing cases stop until affected.

Linux producer6e2066054e6924dbea99782644a02a4e8148095ba73aa91598276d15cdc7de7b;
Windowsb9c42cd9c4cfc6813189433acd711787bbe0d1d55a05cc4cce7a944bb404b455.
Private package df4defff2a75a22c5cc7a604a8bf0c03c57127a00fd847563e71897086a282b8.
Timed sourceb3b655a81bcb68669667f48b9fe945ece38dec99dfa88cb23577a7160e895c3f,
qualifiedsource4b642f834866681857d14166512cf93aef8b6f709f737ba8ed3294e8f7c24c56,
2424files. Only SPEC clarification changed after timing; exact initial ordinal
inventory is reconstructed and hash-verified in raw data. Core/harness unchanged.
[Numeric evidence](warm-jpeg-caller-mcu-evidence.json) retains all actuals, allocation,
guards, packets, quality, owners and exact identities. Raw on both hosts:
`h10d-measurements.tar.gz`
72ca3a4f72fbad1e3a4a365d32a5a3ab14f2a7c515f801780d9c94a1e1980b9d;
`h10d-qualified-windows.tar.gz`
99ed60418963d72db3e571e10ee406f6b0637e6b2bd02bf331e019412ccbd7fd;
`h10d-qualified-linux.tar.gz`
2285d5caedcfc38f0e7b722e621e057051fd6966fb2bab1bc33fd3ce53457e72.
The published0.1.0 package/tag remains unchanged.
