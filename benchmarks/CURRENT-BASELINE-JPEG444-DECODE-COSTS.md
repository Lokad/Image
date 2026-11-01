# Current baseline JPEG 4:4:4 decoding costs

Historical costs: affected JPEG boundaries are superseded by the
[current retained-result round](WARM-JPEG-DIRECT-RGB-RESULT.md).
Original observations and raw evidence remain below.

Three existing 513×259 resident-input controls now have current independent
single-CPU warm costs. All remain demonstrated gaps; these are fresh costs,
with no paired speedup or broad JPEG performance claim.

| Complete owned RGBA8 decode | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op | Encoded input B |
|---|---:|---:|---:|---:|---:|
| Owned packet | 2,180.666 | 1,900.363 | 1.147 / 1.158 | 948,069 | 324,002 |
| Native packet | 2,115.423 | 1,717.672 | 1.232 / 1.247 | 966,367 | 133,041 |
| Native packet, live token | 2,295.790 | 1,725.930 | 1.330 / 1.348 | 966,367 | 133,041 |

Both original packets/settings are unchanged. Full public decode, output
allocation and release remain timed; the native operation decodes/converts every
pixel and acquires/releases its output. Every current managed/native pixel is
exact. The live token remains cancelable, never signaled during timing, with
pre-cancellation rejection checked. Startup/IPC/fixtures/checks are excluded.
Native allocation is unmeasured; managed B/op includes harness costs.

Old independent costs predate fuel/ownership cleanup and the documented integer
inverse/color policy, baseline sample planes and entropy changes. Ordinary
baseline scans use JBF1 fusion; the live path bypasses it and cannot inherit its
gain. The live case also reuses the same current frozen native worker as the
ordinary native control; its very old worker predated full-resolution packing.

The first setup rejected stale expected decoded hashes before any measurement.
An independent audit then proved current managed normal==scalar==pinned normal
FFmpeg CLI for every RGBA sample in both unchanged packets. Only those two
expected metadata hashes were projected for the jobs; original anchor bytes were
restored afterward. No input, reference setting or pixel tolerance changed.

Zero builds, new controls/settings/profiles or repeated full/package checks.
The same immutable J4N31 core/harness, recovered cached selectors and workers are
reused. Its 5,342 checks/OS, same actual Release-package consumers/three examples
and all three green CI jobs remain applicable. Each job retains all twelve
actuals, both native brackets and idle-v2 guards. Stop retiming these gaps without
a distinct change or concrete invalidation; the full 268-path objective stays open.

The [evidence](current-baseline-jpeg444-decode-costs-evidence.json) records all
samples, allocations, exact inputs/pixels, hashes, scope and reuse. Raw evidence
is mirrored/member-verified: `current-baseline-jpeg444-decode-costs.tar.xz`,
3,460,972 B / 91 members, SHA256
`2389e8f997929ace43b4db79df071bbc4124ee55175c7548eed23294d4cc8646`.
The retained J4N31 archive supplies unchanged producer/package provenance.
