# Warm single-CPU JPEG block-coordinate round

This round changes one production primitive: encoder block geometry.
Derive each block origin and component sampling factors once outside the
pixel/sample traversal. Sample order, edge clamping, flattening, color math,
rounding, DCT and entropy coding remain unchanged, as do work/admission checks
and failure behavior. No allocation, storage or cache is added.

It reuses the single matched [warm JPEG encode pair](WARM-JPEG-BYTE.md).
The [numeric evidence](warm-jpeg-coordinates-evidence.json) retains both
phases, actual managed samples/statistics, native elapsed/count brackets,
source/tool/DLL identities, output bytes, quality, allocations and quietness.
Regenerate with `python benchmarks/render-warm-jpeg-coordinates.py`.

| Phase | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | --- | ---: | ---: | ---: | ---: |
| baseline | 1 | 16349.22 | 3784.41 / 3783.31 | 4.32x | 4.91x |
| baseline | 2 | 16281.13 | 3794.93 / 3783.91 | 4.30x | 4.71x |
| coordinates | 1 | 15723.71 | 3775.40 / 3782.03 | 4.16x | 5.31x |
| coordinates | 2 | 15483.87 | 3786.85 / 3785.90 | 4.09x | 5.17x |

Measured 2026-10-10, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). CPU 0 only,
normal native SIMD, one codec/conversion thread, optimized managed JIT with
tiering disabled and non-concurrent workstation GC. Short independent
screens guide priority and retain uncertainty; uniform parity is not met.
Candidate managed means have a 1.5% spread relative to the smaller mean.

Both intervals include resident original RGBA8, exact once-rounded alpha
flattening on RGB 0x204060, full-range BT.601 conversion, 4:4:4 encoding and
complete independent output acquisition/release. Native contexts, scratch
and wrappers are reused; every pixel is reprocessed. Fresh managed
MemoryStream creation, growth and disposal are timed. Startup, IPC, I/O,
generation, hashing, validation and quality enumeration are excluded.

Frozen owned quality 69 / native qscale 5 retain the
0.082440 dB source-error gap within the fixed 0.25 dB band.
Encoded sizes remain 324,002 / 133,041 bytes. Managed fixed
and native optimal Huffman policies remain explicit. The original preparation
used seven owned settings and one source; this round adds neither settings
nor images. Encoded hashes and all quality/decoder/alpha checks are unchanged.

BDN managed allocation: 1,051,673 before and
1,051,660 bytes/op after. Sampling/diagnostic variation
is retained; this primitive adds no heap allocation or admission cost.

Windows/Linux full Release/live suites pass 1,855 tests (275 internal,
686 public, 894 FFmpeg). Four SIMD/scalar Linux modes each pass 55 internal
and 70 public JPEG controls. All 160 original frozen encoder bitstreams pass,
including both input depths and all sampling modes. Exact memory/output
boundaries, cancellation, caller stream ownership and immutable input pass.
The same actual dependency-free Release package and all three usage examples
pass both OS: SHA256 `d1ee1ae93acba5e0878de2799183173ae64bb78fb156d073c1725c481bb75daa`.

Source-byte and pre-run quietness checks pass; failed post-run load samples
and later passing cooldowns remain without weakened thresholds. The separate
accepted encoder profile attributed 55.90% to Encode leaf samples and 13.99%
to forward DCT; profile timing does not enter parity evidence.

Reproduce sequentially on the quiet dedicated Linux VM using the
[JPEG worker instructions](../tools/WarmReference/README.md#jpeg), frozen
preparation, output/quality smoke and two independent warm Jpeg repeats,
with a passing cooldown between them.
