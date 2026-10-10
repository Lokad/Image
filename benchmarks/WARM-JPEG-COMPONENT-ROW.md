# Warm single-CPU JPEG component-row construction

One production primitive constructs eight full-resolution component samples
from a complete RGBA8 row with safe AVX2/AVX-512 intrinsics. Exact alpha
flattening, separate binary64 color operations, rounding and encoded bytes
remain unchanged. The original generic production paths remain available.
The retained screens remain above 2x; uniform parity is not established.

The [original numeric evidence](warm-jpeg-component-row-evidence.json) retains
actual samples, statistics, native brackets, quality, exact output hashes,
source/tool/DLL identities, allocation, correctness controls and quietness.
Regenerate with `python benchmarks/render-warm-jpeg-component-row.py`.

| Phase | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: | ---: |
| baseline | 1 | 13997.60 | 3791.08 / 3780.30 | 3.70x | 4.46x |
| baseline | 2 | 14172.51 | 3780.59 / 3799.85 | 3.75x | 4.06x |
| componentRows | 1 | 9485.54 | 3794.72 / 3773.77 | 2.51x | 3.33x |
| componentRows | 2 | 9431.87 | 3775.93 / 3787.21 | 2.50x | 3.03x |

Measured 2026-10-10, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). CPU 0 only,
one codec/conversion thread, normal SIMD, optimized JIT with tiering disabled
and non-concurrent workstation GC. Short independent screens guide priority
and retain uncertainty.
Managed mean spread is 0.569% relative to the smaller candidate mean.

The same original 513x259 RGBA8 raster is flattened once on RGB 0x204060.
Both intervals include full-range BT.601 conversion, 4:4:4 encode, independent
output acquisition and release. Native contexts/scratch/wrappers are reused;
every pixel is reprocessed. Managed MemoryStream creation, growth and
disposal are timed. Startup, IPC, I/O, generation, hashing, checks and
quality enumeration are excluded.

Frozen owned quality 69 / native qscale 5 retain the
0.082440 dB source-error gap within the fixed 0.25 dB band.
Encoded sizes remain 324,002 / 133,041 bytes.
Exact hashes, decoder/alpha checks and the original seven owned settings
remain unchanged. Managed fixed/native optimal Huffman policies are explicit.
There is one original warm image and one encode case; no new preparation.

Eight complete RGBA pixels occupy one safe 32-byte load. Integer lanes keep
the original nearest encoded-space alpha blend. For its bounded numerator
`0 <= n <= 65152`, division by 255 is exactly `(n + 1 + (n >> 8)) >> 8`.
Each channel widens to binary64 before the existing color matrix; every
multiply/add/subtract remains separate, followed by the existing exact byte
rounding primitive. There is no FMA or reassociation. Only full-resolution
8-bit complete rows use this dispatch. Padded right edges, 16-bit input,
subsampled encoding and CPUs without the required intrinsics use the original
path. Bottom-edge replication and component/block order remain unchanged.
No buffer, retained row/pixel cache, table, dependency, working-byte admission
or logical work is added.

Managed allocation was 1,051,765 bytes/op and is
1,051,907 bytes/op in the last screen.
Observed changes are [142, 142] bytes/op.
Automatic calibration uses [14] baseline versus [32] candidate operations per actual sample.
Direct controls through the actual baseline and measured DLLs retain equal
allocation totals and identical sorted per-operation allocation values over
16 complete encodes, with the same frozen output. The inclusive BDN diagnostic
variation remains visible; no added library heap storage is observed.

All 1,889 tests pass on Windows and Linux: 309 internal, 686 public and
894 live FFmpeg comparisons. Four VM SIMD/scalar modes each pass 89 internal
and 70 public JPEG controls. All 160 frozen encoder bitstreams pass across
both source depths and all sampling modes. Thirteen new cases check every
opaque RGB byte color for each component (50,331,648 reference samples),
1,572,864 alpha/background reference samples, unaligned input/output guards,
unchanged source rows, every transparency lane and invalid row/component
rejection. BDN output smoke passes with the same frozen matched-quality bytes.

A separate original 17x9 correctness input checks 28 complete operations
through the baseline and candidate binaries on both platforms. Transparency
in every lane and at block/right/bottom edges, work/memory/output limits
and cancellation retain the exact error type/message/parameter, output prefix
hash, unchanged source and writable caller stream. These controls make no
timing claim and add no benchmark image.

The same actual dependency-free Release package and all three usage examples
pass both platforms. Package SHA256:
`81f41e52f39f18d6a84b37ab96f4ec2adf534d1589e9cf17946d3b12c1cd4894`.

Package and measured benchmark DLL hashes differ by build identity. Their
method-body fingerprints and all 30 existing JPEG fixture outputs match
through the actual binaries on the VM. These controls make no timing claim.

Failed rolling-load samples and subsequent passing cooldowns remain recorded
without relaxing thresholds. The earlier profile identifies unchanged scalar
color/row work; it is not a post-quantization attribution. Profile warmup and
calibration work is excluded from the parity rows.

Reproduce sequentially on the quiet dedicated Linux VM following the
[JPEG worker instructions](../tools/WarmReference/README.md#jpeg), using
the frozen preparation, output smoke and two independent warm Jpeg repeats.
