# Opaque VP8 encoding

Current production measurements are superseded by [VP8 source conversion](WARM-VP8-SOURCE-VIEWS.md). The retained results below are historical; the source-view change invalidates the four VP8 encode timings, with both alpha paths independently reconfirmed.

Complete opaque VP8 encoding now independently confirms **1.892x FFmpeg /
conservative upper 1.918x**, down from **2.718x**, with identical encoded bytes.
Managed elapsed time falls **30.1%**; measured allocation is **2,536,203 B/op**.
Uniform current-source parity remains open.

| Measurement | Lokad.Image ms | FFmpeg ms | Ratio / upper | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Original screen | 42.019 | 15.461 | 2.718 / 3.062 | 2,535,736 |
| Packed arithmetic normalization | 36.487 | 15.478 | 2.357 / 2.496 | 2,535,736 |
| Compiled format-tree paths | 31.874 | 15.488 | 2.058 / 2.161 | 2,536,008 |
| Inlined arithmetic writer screen | 29.290 | 15.527 | 1.886 / 2.452 | 2,536,203 |
| Independent twelve-actual confirmation | 29.381 | 15.528 | 1.892 / 1.918 | 2,536,203 |
| Alpha RGBA8 regression screen | 30.158 | 53.550 | 0.563 / 0.655 | 2,615,939 |
| Alpha RGBA16 regression screen | 30.644 | 53.908 | 0.568 / 0.598 | 2,615,472 |

All screens have three actuals. Above-two intermediates receive no twelve-actual
confirmation or package qualification. Regression screens check the actual
shared paths before acceptance; their short intervals are not new independent
certificates. Allocation includes harness/GC bookkeeping, which varies slightly
with iteration operation counts; the change adds no per-operation workspace.
Native allocation is unmeasured.
Earlier independent alpha confirmations remain available for
[RGBA8](WARM-VP8-ALPHAWEIGHTED.md) and [RGBA16](WARM-VP8-RGBA16-ENCODE.md).

One opacity projection of the existing original 513x259 RGBA8 raster sets alpha
to255 and preserves every RGB byte. Managed quality100 and native quality75,
both effort4, are fixed before measurement. Whole-raster RGB source PSNR is
10.938710 and10.913072 dB respectively: **0.025638 dB** clears the unchanged
**0.25 dB** band. The registered decoders agree within maximum1/RMSE0.106900,
inside unchanged maximum4/RMSE1.5. Opaque alpha is exact; source preservation,
independent outputs and deterministic packets pass before/after timing.

Managed output remains **333,580 bytes**, SHA-256
`31160884d764be8c90eabbce9d8f845b5a41cccf5b4bd3b99c002072f8b2334b`;
native output is **72,380 bytes**, identical to the existing opaque decode input.
Managed fixed entropy and native adaptive entropy are declared. Compression
efficiency parity is not established; these numbers compare source-quality
matched complete-operation time.

A six-second single-CPU profile attributes27.10% to arithmetic writing,13.24%
to bit appending and3.18% to carry propagation; recursive leaf discovery and
tree emission add10.06% and8.03%. Three narrow steps address these costs:

1. Pack at most seven normalization bits within already admitted partition
   capacity. Growth/output/work failures keep the original bit-at-a-time path.
2. Derive immutable codewords once from the encoder's own RFC6386 format trees,
   preserving each ordered probability. The post-zero root skips exactly its
   original first step; no upstream implementation table is copied.
3. Inline arithmetic writing into hot emission callers, preserving its arithmetic.

Ten new cases compare every interval, partial byte, buffer, work/memory counter
and failure against the original scalar writer. They cover all probabilities,
both trees, every token, both coefficient roots, cancellation, constrained work,
clipped capacities and growth boundaries. Existing independent BigInteger
interval, codec fixture and admission controls also pass. Prediction choices,
coefficients, quality, bytes, work charges and failure sequencing are unchanged.

The complete public encode, fresh MemoryStream growth/acquisition and disposal
are timed. Native RGBA-to-BGRA repacking and complete encoding run every time;
codec/scratch/frame/packet wrappers may be reused. No encoded result is cached.
Sequential Release runs use one pinned CPU, one codec thread, normal SIMD,
disabled tiering and workstation nonconcurrent GC. Startup, I/O, IPC, generation,
quality preparation, hashing and validation are excluded. Both versioned quiet
guards pass for every statistical run. All twelve confirmation actuals remain
included; the upper takes the larger filtered/all-actual interval against the
faster native bracket's lower bound.

The original alpha-only native adapter is preserved. A separate original opaque
adapter enforces one still VP8 chunk without alpha/animation/metadata. Initial
adapter/identity preparation refusals remain unscored. An older alpha8 quality
receipt predates the existing word-capable native worker: a fixed-setting
correctness refresh reproduces its entire quality match, packets and pixels
before rebinding the identity. No quality sweep is performed.

The same frozen harness and actual drivers are reused as production primitives
change. Accepted production passes **3,308 Release tests per OS**, including
**896 live FFmpeg comparisons**, plus27 final scalar checks per OS. The consumer
and all three usage examples pass through the same actual dependency-free
Release package on Windows and Linux. No whole-library method audit repeats.

The [numeric evidence](warm-vp8-encode-evidence.json) retains all samples,
allocations, guards, profile attribution, packets, actual jobs/builds and package
proof. Stop this confirmed pair; material public/codec/current-source coverage
and the single-component grayscale reference remain open.
