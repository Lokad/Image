# Warm single-CPU JPEG magnitude-category round

One production primitive replaces the coefficient magnitude-category shift
loop with the BCL leading-zero primitive. Hardware lowering is used where
available, with a safe software fallback. Math.Abs retains the original
Int32 minimum overflow; zero remains category zero. Color/quantization math,
sample/entropy order, work/admission/cancellation and failure behavior remain
unchanged. No table, allocation, storage, cache or dependency is added.

The [original numeric evidence](warm-jpeg-category-evidence.json) retains
actual samples/statistics, native brackets, quality, exact encoded outputs,
source/tool/DLL identities, allocations, validation and quietness. Regenerate
with `python benchmarks/render-warm-jpeg-category.py`.

| Phase | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: | ---: |
| baseline | 1 | 15723.71 | 3775.40 / 3782.03 | 4.16x | 5.31x |
| baseline | 2 | 15483.87 | 3786.85 / 3785.90 | 4.09x | 5.17x |
| categories | 1 | 14400.22 | 3820.19 / 3774.41 | 3.82x | 5.54x |
| categories | 2 | 14266.48 | 3779.26 / 3775.75 | 3.78x | 3.95x |

Measured 2026-10-10, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). CPU 0 only,
one codec/conversion thread, normal SIMD, optimized JIT with tiering disabled
and non-concurrent workstation GC. These short independent screens retain
uncertainty and guide priority; uniform parity is not established.
Managed mean spread is 0.9% relative to the smaller candidate mean.

The same original 513x259 RGBA8 raster is flattened once on RGB 0x204060.
Both intervals include complete full-range BT.601 conversion, 4:4:4 encode,
independent output acquisition and release. Native contexts/scratch/wrappers
are reused, with every pixel reprocessed. Managed MemoryStream creation,
growth and disposal are timed. Startup, IPC, I/O, generation, hashing,
checks and quality enumeration are excluded.

Frozen owned quality 69 / native qscale 5 retain the
0.082440 dB source-error gap within the fixed 0.25 dB band.
Encoded sizes remain 324,002 / 133,041 bytes, with identical
encoded hashes and decoder/alpha checks. Managed fixed/native optimal Huffman
policies remain explicit. Preparation uses its original seven owned settings
and one source; this round adds neither settings nor images.

Managed allocation is 1,051,660 before and
1,051,765 bytes per operation after. Diagnostic variation
is retained; this primitive adds no allocation or admission cost.

All 1,863 tests pass on Windows/Linux: 283 internal, 686 public and 894 live
FFmpeg comparisons. Four SIMD/scalar VM modes each pass 63 internal and 70
public JPEG controls. All 160 frozen encoder bitstreams pass across both
input depths and all sampling modes. Two additional controls cover all
131,071 signed JPEG magnitude values, all 31 integer category edges and
the original Int32 minimum overflow. Output/memory limits, cancellation,
caller stream ownership and unchanged input pass. BDN output smoke passes.

The same actual dependency-free Release package and all three usage examples
pass both platforms. Package SHA256:
`58cb5f0595838a25fb5038598f38558cfd6e013330690635bf860b47cb2438a9`.

The package and measured benchmark DLL have separate binary hashes. Their
method-body fingerprints match, and the same 30 existing JPEG fixtures yield
identical pixels through both actual binaries on the VM.

Failed post-run rolling-load samples and later passing cooldowns remain
without relaxed thresholds. A separate workload profile attributed 6.79%
to the category loop, 50.23% to Encode leaf samples and 16.77% to forward DCT.
Its trace and source identity are retained; profile timing is excluded from
the parity rows.

Reproduce sequentially on the quiet dedicated Linux VM following the
[JPEG worker instructions](../tools/WarmReference/README.md#jpeg), using
the frozen preparation, smoke and two independent warm Jpeg repeats.
