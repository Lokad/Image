# Constant-time PNG distance symbols

The existing 513×259 RGBA8 Fastest control independently measures **6,450.06 µs,
1.471× FFmpeg, conservative upper 1.504×**, with its exact 236,828-byte packet.
The preceding source-qualified short was 7,763.44 µs / 1.781×. These separate jobs
are an observed improvement, not a paired speedup interval. Full warm parity is open.

| Existing complete operation | Lokad µs | Ratio / upper | Allocated B/op | Packet bytes, both |
| --- | ---: | ---: | ---: | ---: |
| Fastest RGBA8, short | 6,415.15 | 1.469 / 1.695 | 1,344,586 | 236,828 |
| Fastest RGBA8, independent | 6,450.06 | 1.471 / 1.504 | 1,344,586 | 236,828 |
| SmallestSize RGBA8, short | 22,507.99 | 1.301 / 1.350 | 1,325,664 | 210,973 |
| SmallestSize RGBA16, short | 26,234.53 | 1.305 / 1.565 | 3,813,670 | 963,202 |

One bounded profile found greedy matching in 388/794 self samples and distance
mapping in 82/794. An untimed traversal retained the exact packet and found 76.46%
of accepted prefixes were only three/four bytes, with 4.26% at least eight. This
discouraged a wide-match-loop trial. Instrumented times are discarded.

The distance-range loop now uses `BitOperations.Log2(distance - 1) - 1`, following
the doubling intervals in RFC1951 §3.2.5. All 32,768 legal distances match independent
interval expansion and the prior accepted implementation, on both platforms with
intrinsics enabled/disabled. Existing 176 byte/callback and 32 failure/cancellation
comparisons also pass in both modes. No match selection, entropy policy, table,
buffer, owner or cancellation boundary changed. Owned capacities stay fixed;
the measured Fastest allocation is 133 B/op above the preceding short (about 0.01%).

Both platforms pass all 4,959 Release checks and 499 internal / 180 public scalar
PNG checks, including all fourteen complete frozen PNG packets. The same actual
private 0.2.1 package and all three usage examples pass Windows/Linux. One producer
build per OS serves the round; native workers, fixtures, settings, benchmark and
cached selectors are reused. Complete operation boundaries and single-CPU quiet
guards remain; all twelve actual and eleven filtered primary results are retained.

[Evidence](warm-png-distance-symbol-evidence.json) records source, binary, package,
sample and mirrored raw-archive identities, including the actual private package.
Compressed PNG callers need applicable new timing evidence; the three displayed
controls above replace older costs. Level0 does not call distance mapping and
retains its independent [stored-publication result](WARM-PNG-BULK-STORED.md).
Other codecs, editing/text and native oracles are unchanged. The largest displayed
gap is now VP8 diagram effort2; prioritize it before smaller PNG refinements.
