# Warm single-CPU JPEG decode screen

The existing prepared 513x259 baseline 8-bit 4:4:4 JPEG is decoded from
identical resident bytes to independent owned RGBA8 on both subjects.
This round adds one benchmark direction and changes no production primitive.
The selected decoder is above 2x and becomes the largest measured gap.

The [original numeric evidence](warm-jpeg-decode-evidence.json) retains managed
samples/statistics, native elapsed/count brackets, source/tool/DLL identities,
output hashes, decoder agreement, allocations and quietness. Regenerate with
`python benchmarks/render-warm-jpeg-decode.py`.

| Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
| 1 | 30141.96 | 1900.53 / 1902.38 | 15.86x | 21.96x |
| 2 | 29637.68 | 1908.19 / 1897.88 | 15.62x | 18.66x |

Measured 2026-10-10, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). CPU 0 only,
one codec/conversion thread, normal native SIMD, optimized managed JIT with
tiering disabled and non-concurrent workstation GC. Short repeats guide
priority and retain uncertainty; they do not establish uniform parity.

Both intervals include full decode, full-range BT.601 to RGBA8, owned output
allocation and release. Native decoder/conversion/input packet/frame wrappers
are reused; decoder state resets and all pixels are decoded/converted every
operation. Padded native RGBA rows need no tight-row copy. Managed public
Decode and disposal are timed. Startup, IPC, I/O, generation, hashing,
validation and tight-row file writes are excluded from both intervals.

The already prepared 324,002-byte owned quality-69 JPEG is reused; input SHA256
`998c0ef54da55a0fdfb597927f3d485f19c2ef9f2b97dd8726b512e9210e849d`. No encoder quality search or additional
source raster is added. Since encoded input is identical, the decoder pair
needs decoder agreement rather than an encoder source-quality match.
RGB agreement is maximum 3, RMSE 0.227803, within
the unchanged maximum 4/RMSE 1.5 bounds. Opaque alpha is exact. Native pixels
match the pinned public FFmpeg CLI exactly with the same conversion flags.
Setup/cleanup check repeated pixels, independent live owners and unchanged
encoded source. Fourteen native protocol/argument/affinity rejection controls
and BenchmarkDotNet output smoke pass.

Each output has 531,468 logical bytes; native padded rows are recorded.
Managed BDN allocation is 2,594,723 bytes per operation.
Owned source-byte and pre-run quietness checks pass. Post-run rolling-load
failures and later passing cooldowns remain recorded without weakened limits.
JPEG encode, PNG, WebP and other editing paths retain their separate evidence.

A separate workload-only CPU profile attributes 54.05%
to inverse DCT, the first production primitive selected for optimization.
Entropy reading and raster rendering remain separate substantial costs.
The original trace hash and weighted attribution are retained in the evidence;
profile samples are excluded from the parity timing rows.

Reproduce sequentially on the quiet dedicated Linux VM following the
[worker instructions](../tools/WarmReference/README.md#jpeg-decode).
