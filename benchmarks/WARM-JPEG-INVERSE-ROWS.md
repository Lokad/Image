# Warm single-CPU JPEG shared inverse-DCT products

This round changes one production primitive: the AVX-512 inverse DCT.
Each exact dequantized horizontal product is shared across eight independent
output-row accumulators. Every pixel retains its original v/u term order,
separate multiplications, frozen owned binary64 basis and scalar final
rounding/clamping. AVX and scalar fallbacks, 4096 logical work units and
storage admission are unchanged; no allocation or cache is added.

It reuses the single [warm JPEG decode pair](WARM-JPEG-DECODE.md). The
[numeric evidence](warm-jpeg-inverse-rows-evidence.json) retains the accepted
sampling baseline and both candidate repeats, raw samples/statistics, native
brackets, allocations, identities, output hashes and quietness.
Regenerate with `python benchmarks/render-warm-jpeg-inverse-rows.py`.

| Phase | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | --- | ---: | ---: | ---: | ---: |
| baseline | 1 | 10905.00 | 1902.42 / 1899.21 | 5.74x | 7.76x |
| baseline | 2 | 9036.91 | 1912.48 / 1903.38 | 4.75x | 5.97x |
| sharedRows | 1 | 8122.17 | 1902.70 / 1901.60 | 4.27x | 4.40x |
| sharedRows | 2 | 7216.61 | 1901.97 / 1907.28 | 3.79x | 4.22x |

Measured 2026-10-10, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). CPU 0 only,
normal native SIMD, one codec/conversion thread, optimized managed JIT with
tiering disabled and non-concurrent workstation GC. Short repeats guide
priority; uncertainty and the differing baseline screens remain visible.
Uniform parity and stable speed certification are not established.

The candidate means have a 12.5% spread relative to the smaller mean.

Both subjects decode the identical resident 324,002-byte baseline 8-bit
4:4:4 JPEG to independent RGBA8, including full-range BT.601 conversion,
allocation and release. Native state reset is timed; contexts/wrappers are
reused while each call decodes/converts every pixel. Padded native rows need
no tight-row copy. Public managed Decode/disposal are timed. Startup, IPC,
I/O, generation, hashing, validation and packing writes are excluded.

No raster or quality enumeration is added. Input and both output hashes
remain unchanged; owned pixels equal the frozen preparation, native pixels
equal the pinned CLI. Maximum 4/RMSE 1.5 decoder bounds, exact opaque alpha
and independent live output storage are retained.

Managed BDN allocation: 2,596,143 before and
2,596,234 bytes/op after. Sampling/diagnostic variation
is retained; the production primitive introduces no heap allocation or
additional admission cost.

All 19 original inverse-transform controls pass, including frozen scalar
half ties, clipping, dense/extreme coefficients and ushort quantizers,
immutable inputs, guarded output, work/cancellation and complete lengths.
Exact owned pixels match the accepted Release package across all 30 existing
original JPEG fixtures; all 160 frozen encoder bitstreams remain unchanged.
Windows/Linux full Release/live suites pass 1,855 tests (275 internal,
686 public, 894 FFmpeg). Four SIMD/scalar Linux modes each pass 55 internal
and 70 public JPEG controls. The same actual dependency-free Release package
and all three usage examples pass both OS:
SHA256 `e1b19748ffb1b9c89f27bbb017c7dc674c87cdec59ba41077616d0c9c7f1d3e5`.

Source-byte and pre-run quietness checks pass. Failed post-run load samples
and later passing cooldowns remain without weakened thresholds. The separate
accepted-sampling profile attributed 45.30% to inverse DCT leaf samples;
profile timing does not enter parity evidence.

Reproduce sequentially on the dedicated quiet Linux VM using the
[worker instructions](../tools/WarmReference/README.md#jpeg-decode), frozen
preparation, output smoke and two independent warm JpegDecode repeats,
with a passing cooldown between them.
