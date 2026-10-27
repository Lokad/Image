# Bulk stored PNG publication

The existing 513×259 RGBA16 NoCompression control independently measures
**426.14 µs, 1.052× FFmpeg, conservative upper 1.067×**, with the exact complete
1,066,552-byte packet. The 0.2.0 release snapshot measured 1,112.30 µs / 2.752×.
These separate jobs establish a substantial observed improvement, not a paired
speedup confidence interval. Full ≤1× warm parity remains open.

| Existing complete operation | Lokad µs | Ratio / upper | Allocated B/op | Packet bytes, both |
| --- | ---: | ---: | ---: | ---: |
| Stored RGBA16, short | 450.34 | 1.066 / 1.185 | 1,741,710 | 1,066,552 |
| Stored RGBA16, independent | 426.14 | 1.052 / 1.067 | 1,741,709 | 1,066,552 |
| Fastest RGBA8, short | 7,763.44 | 1.781 / 1.979 | 1,344,453 | 236,828 |
| SmallestSize RGBA8, short | 23,674.33 | 1.370 / 1.558 | 1,325,670 | 210,973 |
| SmallestSize RGBA16, short | 26,792.89 | 1.334 / 1.441 | 3,813,679 | 963,202 |

One bounded profile of the unchanged published producer found 451/712 workload
samples in `StoredBlock` itself, versus two in constructor code. Its instrumented
times are discarded. This favored removing per-byte publication over specializing
allocation. The constructor, workspace allowance and all owned capacities remain.
Stored allocation is approximately 1.74 MB/op; the release snapshot recorded
1,741,680 B/op, 29 B/op below the independent candidate observation.

Managed span copies fill the existing 4 KiB encoded buffer, preserving every
flush, cancellation check and writer-failure prefix. Stored fallback reads at most
two spans across the history ring. Block decisions, alignment, checksums, policies
and packet bytes are unchanged. No new owner, cache or admission mechanism.

A bounded comparison against Lokad.Image's own published source passes 176
byte/callback cases and 32 failure/cancellation pairs on each platform, including
ring/flush edges; independent inflation and shared cleanup pass. Existing tests
also cover complete-row stored blocks. Both platforms pass all 4,959 Release
checks and 499 internal / 180 public scalar PNG checks, including all fourteen
frozen PNG packets. The same actual private 0.2.1 package and three usage examples
pass Windows/Linux. Published packages remain immutable.

Only one production build per OS was used. Native workers, fixtures, reference
settings, benchmark assembly and cached selectors were reused. Normalization,
compression, fresh output ownership and release remain timed, on one pinned CPU,
with passing idle-v2 guards and all actual/unfiltered samples retained.

[Evidence](warm-png-bulk-stored-evidence.json) identifies producers, package,
samples and the mirrored, member-verified raw archive, which also retains the
actual private package. All PNG encoding callers have changed code; the four
displayed controls above replace their older costs. Other PNG policies/stream
boundaries still need applicable evidence. Unchanged decoders, JPEG/WebP,
editing/text and native reference proofs remain reusable. PNG Fastest is now the
largest displayed gap; do not prioritize the remaining 5.2% stored gap over it.
