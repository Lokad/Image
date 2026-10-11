# Warm PNG local work accounting

Exact local bit-work accounting improves complete PNG16 decode screens to
2.09x / 2.10x FFmpeg and PNG8 to 2.07x / 2.07x. Managed time falls about
5% and 2%, respectively. All six actual samples at each depth are below
the latest retained baseline samples. Every case remains above 2x;
uniform warm parity remains open.

The [numeric evidence](warm-png-local-work-evidence.json) retains samples,
confidence bounds, allocations, source/input/output identities, quietness
failures, job receipts and immutable raw/source archive hashes. Regenerate
with `python benchmarks/render-warm-png-local-work.py`.

| Stage | Case | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | --- | ---: | ---: | ---: | ---: |
| Prior Work | Decode16 | 6482.70 | 2919.78 / 2935.67 | 2.22x | 2.44x |
| Prior Work | Decode8 | 2994.19 | 1418.29 / 1422.28 | 2.11x | 2.32x |
| Prior Work | Decode16 | 6469.95 | 2919.63 / 2916.54 | 2.22x | 2.39x |
| Local accounting | Decode8 | 2930.14 | 1418.46 / 1419.15 | 2.07x | 2.18x |
| Local accounting | Decode16 | 6111.96 | 2919.04 / 2919.96 | 2.09x | 2.26x |
| Local accounting | Decode8 | 2932.68 | 1419.16 / 1417.72 | 2.07x | 2.12x |
| Local accounting | Decode16 | 6127.95 | 2921.08 / 2925.06 | 2.10x | 2.26x |

For noncancelable tokens, the strict Deflate token loop snapshots remaining
work after tree/lookup construction. Each Read/Consume checks its original
atomic bit charge and subtracts it locally. A finally block publishes exactly
the successful charges on every exit, before further parent-context work.
No work is charged ahead. Cancellable operations and stored blocks retain
their original work and cancellation checks. Two local stack integers replace
repeated shared tally writes; no buffer, cache or successful-path heap
allocation is added. Prefix decoding, word refill, output and memory limits,
checksum cursor, precision, hidden RGB and independent ownership are unchanged.

One optimized JIT inspection confirms that the default short-prefix path
updates the local budget and skips the shared tally write. Cancellable fallback
and block exit retain shared accounting. Validate grows from 9,380 to 11,077
bytes, with its fixed frame growing from 584 to 600 bytes. Actual statistical
and JIT libraries match the same package in every production method body and
implementation flag. The JIT check passes both quiet checks and supplies no
new CPU-cost attribution.

The repeat matches prior BenchmarkDotNet allocations exactly: 755,294 B/op
at 8 bits and 2,032,847 B/op at 16 bits. The first screen reports an additional
21 / 51 B/op. A separate fixed-count control performs complete public span
decode and owned disposal through both actual packages, using the same two
input hashes. Three repeats of sixteen operations produce identical thread
allocation counts: 754,968 / 2,032,184 B per operation. This control passes
both quiet checks and reports no timing. No particular harness allocation
is attributed to the small first-screen variation.

Both screens reuse the two existing original 513x259 RGBA8/16 rasters;
there is no new image or quality enumeration. All decoded samples and input/
output hashes remain exact, including zero-alpha hidden RGB. Complete public
decode and independent output acquisition/release are timed, including
necessary native conversion and reset. Context/scratch reuse is declared.
Startup, IPC, I/O, generation, hashing and validation are excluded. Sequential
Release jobs use CPU 0, one codec/conversion thread, normal SIMD, .NET 10.0.8 /
SDK 10.0.204, disabled tiering and non-concurrent workstation GC on the
dedicated AMD EPYC 9V45 VM. Statistical prechecks pass; postchecks fail and
remain recorded. One failed precheck collects no samples. These screens
support retaining the reduction, without certifying <=2x.

Two new truncated-distance cases pass against original and candidate code.
They pin the successful work charge before a failing refill, the atomic
failed charge under a tight budget, parent work already consumed, exact
format/limit error precedence and scratch release, with both default and
cancellable tokens. All 2,034 tests pass on Windows and Linux: 454 internal,
686 public and 894 live FFmpeg comparisons. The same actual dependency-free
Release package passes all three usage examples on both platforms. SHA256:
`02d84f78e5bbad2539fe2461905157c00504a4d235bb8166c3099328e0da2464`.

A subsequent single frozen matched JPEG encode recheck screens at 2.02x
FFmpeg (conservative upper 2.20x), with unchanged encoded bytes, quality
gap 0.08244 dB. Allocation estimates are 1,051,958 versus the pre-Work
1,051,907 B/op; retain the +51 difference without attributing it. Its managed
mean is 7,638.30 µs;
native brackets are 3,784.38 / 3,778.61 µs. The seven historical owned quality
settings are reused; no new quality enumeration or benchmark image. Its actual
statistical DLL also matches the same package in all production IL and flags.
The new PNG-only accounting is not used by this path, so no JPEG gain is
attributed to it. Prequiet passes, postquiet fails; confirmation remains open.

PNG16 remains the largest current measured gap. Profile that current code
once on the same input before selecting the next primitive; earlier framing
percentages predate Work and local accounting. Native JPEG decode at 2.06x
has one affected screen. Other provisional, unknown and unmatched paths stay
open; no blanket gain is inferred.
