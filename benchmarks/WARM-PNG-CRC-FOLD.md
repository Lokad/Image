# Warm PNG polynomial CRC folding

Safe carryless multiplication reduces complete PNG16 decode screens to 1.9955x / 2.0021x
FFmpeg and PNG8 to 2.0370x / 2.0359x. PNG16 takes about 4.6% less managed time;
PNG8 about 1.5% less. PNG16 means straddle 2x; PNG8 remains above 2x.
Uniform warm parity remains open. These are narrow screens, not passing certificates.

The [numeric evidence](warm-png-crc-fold-evidence.json) retains actual samples,
confidence bounds, allocations, source/input/output identities, quietness
failures and immutable raw/source archive hashes. Regenerate this report with
`python benchmarks/render-warm-png-crc-fold.py`.

| Stage | Case | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | --- | ---: | ---: | ---: | ---: |
| Local accounting | Decode8 | 2930.14 | 1418.46 / 1419.15 | 2.0657x | 2.1770x |
| Local accounting | Decode16 | 6111.96 | 2919.04 / 2919.96 | 2.0938x | 2.2599x |
| Local accounting | Decode8 | 2932.68 | 1419.16 / 1417.72 | 2.0686x | 2.1239x |
| Local accounting | Decode16 | 6127.95 | 2921.08 / 2925.06 | 2.0978x | 2.2566x |
| Polynomial CRC | Decode16 | 5823.74 | 2940.46 / 2918.46 | 1.9955x | 2.2661x |
| Polynomial CRC | Encode8 | 13821.02 | 17159.50 / 17147.06 | 0.8060x | 0.8409x |
| Polynomial CRC | Decode8 | 2889.42 | 1420.09 / 1418.48 | 2.0370x | 2.1347x |
| Polynomial CRC | Decode16 | 5847.17 | 2924.02 / 2920.57 | 2.0021x | 2.1793x |
| Polynomial CRC | Encode8 | 13830.59 | 18105.68 / 17970.74 | 0.7696x | 0.7759x |
| Polynomial CRC | Decode8 | 2883.34 | 1416.25 / 1429.64 | 2.0359x | 3.0357x |

One current PNG16 EventPipe CPU profile selected this distinct primitive.
Inclusive strict framing accounts for 49.9%, BCL inflation 35.3%, and CRC 8.8%;
the visible framing leaf accounts for 45.5%. Inclusive methods overlap and
cannot be added. The profile includes workload warmup/calibration and supplies
CPU attribution, not a parity ratio. Its actual profiling-child library matches
the accepted package in all production method bodies and implementation flags.
Both correctness receipts bind that DLL and the same existing PNG16 input.

The CRC uses the [PNG section 5.5 polynomial](https://www.w3.org/TR/png/#5CRC-algorithm)
and safe [.NET carryless multiplication](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.x86.pclmulqdq.carrylessmultiply?view=net-10.0).
In normal polynomial order a 16-byte update is `(state * x^128 + input * x^32) mod P`.
A bounded 128-bit representative folds each new block using independently
generated remainders of x^128 and x^192. A final x^64 fold and polynomial Barrett
division recover the 32-bit state. The quotient and all remainders are derived
from P; no upstream implementation, table or constant set is copied.
SSE2 reverses bits within input bytes. Two bounded scalar span reads construct
each vector; no unsafe load, pointer, native binding or package dependency is added.
Unsupported hardware retains the original eight-byte/byte implementation.
Short and trailing bytes use the same scalar recurrence.

CRC cancellation remains before the first nonempty block and every 4096 bytes.
An empty checksum retains its original no-work cancellation boundary. Checksum
validation, cursor, work/memory/output admission, compression effort, encoded
bytes, precision, hidden RGB and independent ownership remain unchanged.
Static value vectors add no successful-path heap allocation or per-image cache.

The two existing original 513x259 RGBA8/16 rasters are reused. The first screen
contains only complete PNG16 decode; the second repeats it and checks affected
PNG8 decode/encode; the third repeats those affected cases. No new image, size
or quality enumeration. Lossless samples and all input/output hashes remain exact.
PNG8 encode keeps the historical bytes, sizes, level 6/filter None and IDAT checks.

PNG8 encode screens are 0.8060x / 0.7696x. Managed means stay near 13.82 ms;
the differing native brackets account for most of the ratio change. No encoder
speed gain is attributed to this round.

Complete public decode or encode and independent owned output acquisition/
release are timed, including required native endian conversion and reset.
Context/scratch reuse is declared. Startup, IPC, I/O, generation, hashing and
validation are excluded. Sequential Release jobs use CPU 0, one codec/conversion
thread, normal SIMD, .NET 10.0.8 / SDK 10.0.204, disabled tiering and non-concurrent
workstation GC on the dedicated AMD EPYC 9V45 VM. All statistical prechecks pass;
postchecks fail and remain recorded, along with failed cooldowns. No <=2x
certification follows from these screens.

BenchmarkDotNet allocation estimates remain in the numeric evidence: both
PNG16 screens report 2,032,898 B/op, 51 above the last baseline repeat;
PNG8 reports 755,294 B/op. A separate correctness control compares complete
public span decode and owned disposal through the actual old and new packages.
Three fixed repeats of sixteen operations have identical thread allocation
counts: 754,968 B per operation at 8 bits and 2,032,184 at 16 bits.
This control makes no timing claim. The BDN difference is preserved without
attributing it to a particular harness allocation.

Ten new controls pass against original and candidate code. They cover wide
boundaries, every byte value at every position of 16/32/48-byte unaligned inputs
and empty-input cancellation. All 114 focused CRC/framing controls pass with
hardware enabled and disabled on both platforms. All 2,044 tests pass on Windows
and Linux: 464 internal, 686 public and 894 live FFmpeg comparisons.
The same actual dependency-free Release package passes all three usage examples
on both platforms. Every actual statistical library matches that package in
all production method bodies and implementation flags. Package SHA256:
`3613a5ca167df6749f78fb0fdec680e6827c5aaef1d080250d27218ad8be95ed`.

Native-input JPEG decode at about 2.055x is now the largest retained measured gap,
ahead of the current PNG8 screens. Its single screen and the historical VP8L/
owned-JPEG results need current confirmation. JPEG encode remains at about 2.021x.
Other unknown, unmatched and uncovered required paths stay open; no blanket gain
is inferred.
