# Warm JPEG sample rounding and conversion

The final inverse-DCT conversion rounds, clamps and writes eight independent
pixels with safe AVX-512F/VL intrinsics. Transform accumulation and exact
owned pixels remain unchanged. The retained gaps are still above 2x;
these short screens do not establish uniform parity.

The [original numeric evidence](warm-jpeg-round-samples-evidence.json) retains
actual samples, statistics, allocation, native brackets, source and binary
identities, frozen input/output hashes, correctness controls and quietness.
Regenerate with `python benchmarks/render-warm-jpeg-round-samples.py`.

| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: | ---: |
| Native / baseline | 1 | 6979.76 | 1719.74 / 1725.72 | 4.06x | 4.52x |
| Native / baseline | 2 | 7043.52 | 1722.65 / 1720.48 | 4.09x | 4.82x |
| Native / SIMD conversion | 1 | 6399.53 | 1723.29 / 1717.73 | 3.73x | 4.28x |
| Native / SIMD conversion | 2 | 6399.85 | 1724.41 / 1726.56 | 3.71x | 3.98x |
| Owned / baseline screen | 1 | 7326.89 | 1903.70 / 1906.35 | 3.85x | 4.43x |
| Owned / affected screen | 1 | 6777.20 | 1905.55 / 1921.61 | 3.56x | 4.58x |

Measured 2026-10-10, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). CPU 0 only,
one codec/conversion thread, normal SIMD, optimized JIT with tiering disabled
and non-concurrent workstation GC. Native-input managed mean spread is
0.005%. The owned affected input has one screen only.
Uncertainty and the earlier observations remain in the numeric evidence.
The initial conversion variant screened at 4.18x and did not improve the baseline.
That variant used explicit vector division without the inlining request.
Its source, samples and quietness remain recorded separately.

Both frozen JPEGs come from one existing original 513x259 RGBA8 raster,
flattened once on RGB 0x204060. No additional raster, upstream corpus,
preparation or quality enumeration is used. Each pair receives identical
resident JPEG bytes and includes complete public decode/conversion, owned
output allocation and disposal. Native context/conversion/input wrappers
are reused; state reset and every decoded/converted pixel remain timed.
Startup, IPC, I/O, generation, hashing, checks and file packing are excluded.

One production primitive changes. The eight row sums use separate
multiplication by the exact 0.25 and addition of 128. This equals division
by four; the helper is inlined. Finite samples are clipped to the
byte range, floored and incremented only when their exact fractional part
is at least one half. Adding 0.5 before flooring could incorrectly round
values immediately below a tie. The resulting integers are safely narrowed
and copied to eight output bytes. The original basis, v/u accumulation
order, separate products, 4,096 work units and admission/cancellation stay
unchanged. AVX/scalar fallbacks remain available; there is no FMA,
reassociation, new table, buffer, heap allocation or cache.

All 1,868 tests pass on Windows and Linux: 288 internal, 686 public and
894 live FFmpeg comparisons. The VM also passes 68 internal and 70 public
JPEG controls in each of four modes: normal SIMD, AVX-512 disabled, AVX
disabled and all intrinsics disabled. Five new cases cover 1,048 original
scalar-reference boundary values, all eight rotations of distinct lanes,
guard bytes and rejection of incomplete rows before writing.
The 30 existing original JPEG fixtures retain exact pixels against the
previous library; 160 frozen encoder bitstreams also pass. BDN output
smoke and frozen warm outputs pass, with exact normal-SIMD FFmpeg CLI pixels.

The same actual dependency-free Release package passes verification and
all three usage-guide examples on both platforms. Package SHA256:
`1127782175cf639f80e1afd95307dd6e5500e24f38322d4510a607ed6a323307`.

Package and benchmark builds have distinct DLL identities. On the VM,
their method-body fingerprints and all 30 original fixture pixels match.
These controls make no timing claim; both DLL hashes remain recorded.

Native-input managed allocation is 2,664,536 bytes/op.
Observed changes against the prior native screens are [0, 0] bytes/op;
the owned affected screen changes by 0 bytes/op. No storage is added.
Failed quietness samples and later passing cooldowns remain recorded.
A separate baseline workload profile attributed 34.41% of leaf work to
inverse DCT, the largest individual primitive, and 41.45% inclusively to
entropy/Huffman across several methods. Profile warmup/calibration samples
are excluded from parity timing.

Reproduce sequentially on the quiet dedicated Linux VM following the
[worker instructions](../tools/WarmReference/README.md#jpeg-decode).
