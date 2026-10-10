# Warm JPEG full-resolution color rows

One production primitive converts eight full-resolution YCbCr samples to
RGBA8 with safe AVX2/AVX-512F/VL intrinsics. Each original binary64 color
operation, rounding result and output pixel remains unchanged. The retained
gaps remain above 2x; short screens do not establish uniform parity.

The [original numeric evidence](warm-jpeg-color-row-evidence.json) retains
actual samples, statistics, allocation, native brackets, source and binary
identities, frozen input/output hashes, correctness controls and quietness.
Regenerate with `python benchmarks/render-warm-jpeg-color-row.py`.

| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: | ---: |
| Native / baseline | 1 | 6399.53 | 1723.29 / 1717.73 | 3.73x | 4.28x |
| Native / baseline | 2 | 6399.85 | 1724.41 / 1726.56 | 3.71x | 3.98x |
| Native / color rows | 1 | 5365.98 | 1717.89 / 1718.31 | 3.12x | 4.20x |
| Native / color rows | 2 | 5302.20 | 1723.40 / 1723.88 | 3.08x | 3.17x |
| Owned / baseline screen | 1 | 6777.20 | 1905.55 / 1921.61 | 3.56x | 4.58x |
| Owned / affected screen | 1 | 5783.39 | 1900.67 / 1898.88 | 3.05x | 3.29x |

Measured 2026-10-10, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). CPU 0 only,
one codec/conversion thread, normal SIMD, optimized JIT with tiering disabled
and non-concurrent workstation GC. Native-input managed mean spread is
1.203%. The owned affected input has one screen only.
Uncertainty and prior observations remain in the numeric evidence.

Both frozen JPEGs come from one existing original 513x259 RGBA8 raster,
flattened once on RGB 0x204060. No additional raster, upstream corpus,
preparation or quality enumeration is used. Each pair receives identical
resident JPEG bytes and includes complete public decode/conversion, owned
output allocation and disposal. Native context/conversion/input wrappers
are reused; state reset and every decoded/converted pixel remain timed.
Startup, IPC, I/O, generation, hashing, checks and file packing are excluded.

Equal component factors already map each source sample exactly to the
output coordinate. Complete eight-pixel rows widen each of three byte planes
to bounded integer lanes. Chroma subtraction by 128 is exact; the original
binary64 color matrix retains its separate multiply/add/subtract operations.
The existing sample-rounding primitive writes three eight-byte rows in
24 bytes of the existing inverse-block scratch. Safe widening and shifts
interleave R/G/B and opaque alpha into one 32-byte output. No buffer, cache,
table, allocation, working-byte admission or logical work is added.
Original row tails, subsampled interpolation, grayscale, explicit RGB and
non-SIMD production paths remain. Common 1x2 factors with equal sampling
qualify. Geometry/orientation, row work/cancellation, entropy/source/error
order and independent output ownership remain unchanged.

All 1,901 tests pass on Windows and Linux: 321 internal, 686 public and
894 live FFmpeg comparisons. Four VM SIMD/scalar modes each pass 101 internal
and 70 public JPEG controls. Twelve new cases check all 16,777,216 Y/Cb/Cr
byte triples: 50,331,648 original scalar-reference RGB samples and exact
opaque alpha. Eight unaligned mixed phases check input/output/scratch guards
and unchanged source samples. Ten invalid length cases reject before writing.
The 30 original JPEG fixtures keep exact owned pixels and 160 frozen encoder
bitstreams remain unchanged. Output smoke and normal-SIMD FFmpeg CLI pixel
controls pass on both frozen warm inputs.

The same actual dependency-free Release package passes verification and
all three usage-guide examples on both platforms. Package SHA256:
`ce1a7de4ac9284a68791ee24773021a532bccd764453d74a467722238fcdb57f`.

Package and benchmark builds have distinct DLL identities. On the VM,
their method-body fingerprints and all 30 original fixture pixels match.
These controls make no timing claim; both DLL hashes remain recorded.

Native-input managed allocation is 2,664,570 bytes/op.
Inclusive BDN changes against the prior native screens are [68, 34] bytes/op;
the owned affected screen changes by 34 bytes/op. Diagnostics remain visible.
Separate strict controls perform 16 complete public Stream decodes on each
frozen input through the baseline and measured DLLs. Candidate allocation
totals do not increase; owned output hashes remain identical. Fresh caller
streams and output disposal are included; argument storage is reused equally.
Native-input totals: 53,135,488 / 53,135,488 bytes (baseline / candidate).
Owned-input totals: 63,575,344 / 63,544,264 bytes (baseline / candidate).
These are
allocation controls, not timings; parity uses resident span inputs.

Failed quietness samples and later passing cooldowns remain recorded.
The separate current baseline profile attributes 26.14% of leaf work to
Render, 23.34% to ReadBlock, 14.83% to Huffman.Read and 12.53% to inverse DCT.
The coefficient/entropy group spans several methods and accounts for
54.47% inclusively. Method names are matched without parameter type names.
Profile workload includes warmup/calibration and is excluded from parity
timing. The profile does not attribute a separate percentage to color math.

Reproduce sequentially on the quiet dedicated Linux VM following the
[worker instructions](../tools/WarmReference/README.md#jpeg-decode).
