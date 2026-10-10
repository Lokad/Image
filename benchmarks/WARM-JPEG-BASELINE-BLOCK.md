# Warm JPEG baseline coefficient blocks

One production primitive separates admitted baseline JPEG coefficient blocks
from progressive/refinement decoding. Decoded pixels and failure boundaries
remain unchanged. Both retained gaps exceed 2x; these short screens do not
establish uniform parity.

The [original numeric evidence](warm-jpeg-baseline-block-evidence.json)
retains samples, statistics, allocation diagnostics, native brackets, source
and binary identities, frozen input/output hashes, controls and quietness.
Regenerate with `python benchmarks/render-warm-jpeg-baseline-block.py`.

| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: | ---: |
| Native / baseline | 1 | 5365.98 | 1717.89 / 1718.31 | 3.12x | 4.20x |
| Native / baseline | 2 | 5302.20 | 1723.40 / 1723.88 | 3.08x | 3.17x |
| Native / baseline blocks | 1 | 5112.00 | 1726.01 / 1716.48 | 2.98x | 4.61x |
| Native / baseline blocks | 2 | 5134.22 | 1723.64 / 1731.19 | 2.98x | 3.54x |
| Owned / baseline screen | 1 | 5783.39 | 1900.67 / 1898.88 | 3.05x | 3.29x |
| Owned / affected screen | 1 | 5477.23 | 1925.25 / 1898.57 | 2.88x | 5.37x |

Measured 2026-10-10, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). CPU 0 only,
one codec/conversion thread, normal SIMD, optimized JIT with tiering disabled
and non-concurrent workstation GC. Native-input managed mean spread is
0.435%. The owned input has one affected screen only.

Two frozen JPEGs come from one existing original 513x259 RGBA8 raster.
No additional image, preparation or quality enumeration is used. Each pair
receives identical resident JPEG bytes and includes complete public decode,
conversion, independent owned output allocation and disposal. Native context,
conversion and input wrappers are reused; reset and every decoded/converted
pixel remain timed. Startup, IPC, I/O, generation, hashing, correctness checks
and packing are excluded.

Existing scan admission establishes SS=0, SE=63 and Ah=Al=0. The separate
baseline loop uses a constant AC band and removes variable amplitude shifts
and progressive/refinement state, following [T.81 F.2.2](https://www.w3.org/Graphics/JPEG/itu-t81.pdf).
DC categories/predictor bounds, AC categories, zero runs, end-of-block,
coefficient positions and symbol/amplitude/error order remain identical.
The original progressive loop, Huffman/reader primitives, MCU/restart handling,
work/cancellation, geometry/orientation, rendering and independent ownership
remain. No storage, cache, table, allocation or dependency is added.

All 1,915 tests pass on Windows and Linux: 335 internal, 686 public and
894 live FFmpeg comparisons. Four VM SIMD/scalar modes each pass 115 internal
and 70 public JPEG controls. Fourteen new cases cover 45 DC signed-endpoint
subcases, 2,520 AC position/category/signed-endpoint subcases, dense mixed
coefficients, zero runs, exact marker/source positions, untouched coefficients
and guards. Seven malformed cases preserve failure messages, work and partial
writes; two tight work budgets, cancellation and two invalid block lengths
check admission before input/mutation. Thirty original fixtures retain exact
pixels; all 160 frozen encoder bitstreams remain unchanged. Output smoke and
FFmpeg CLI pixel controls pass.

The same actual dependency-free Release package passes verification and all
three usage-guide examples on both platforms. Package SHA256:
`dc3343d20e1c4a7e067d01546297474937d79eea58583e3e923c40d2b08edc22`.
Package and measured DLL identities differ; their method-body fingerprints
and all 30 original fixture pixels match on the VM.

Inclusive BDN allocation changes are [-34, -34] bytes/op for the native-input repeats
and 0 bytes/op for the owned screen. Raw diagnostics remain visible.
Separate strict controls perform 16 complete public Stream decodes per frozen
input through baseline and measured DLLs. Candidate totals do not increase;
owned pixel hashes match. Fresh caller streams and disposal are included.
Native-input totals: 53,135,488 / 53,135,488 bytes (baseline / candidate).
Owned-input totals: 63,575,344 / 63,544,264 bytes (baseline / candidate).
These controls make no timing claim; warm parity uses resident span inputs.

Failed quietness samples and later passing cooldowns are retained. The earlier
profile predates the color-row and baseline-block changes. Its unchanged
coefficient loop guided this candidate; its percentages are not candidate
measurements. Profile warmup/calibration is excluded from parity timing.

Reproduce sequentially on the quiet dedicated VM using the
[worker instructions](../tools/WarmReference/README.md#jpeg-decode).
