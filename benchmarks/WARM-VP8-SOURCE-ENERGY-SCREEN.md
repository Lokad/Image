# Rejected fixed-source texture-energy screen

One existing 513×259 RGBA8 VP8 Q75/method4 control tests reusing a full scalar
source energy within each Luma4 block. Source pixels and weights are invariant
snapshots; final absolute-difference/shift order and geometry/weight checks remain.
No owner, cache object or admission change. The complete one-CPU operation still
includes normalization, encoding and fresh independent output acquisition/release.

| Rejected quiet short | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Exact packet bytes, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| VP8 + alpha | 92,817.76 | 53,575.70 | 1.732 / 1.913 | 23,358,400 | 98,590 |

The result is flat against the selected 92,818.18-µs independent run, with no
allocation improvement. Both idle-v2 guards pass; this is an unconfirmed screen.
The candidate is discarded. No validation relaxation, second representation,
independent timing or full/package qualification follows it.

The candidate passes 55 focused native/metric/macroblock/alpha checks on each OS,
including 102,656 frozen mathematical byte-domain blocks. One new independent
dense-energy test is retained against the actual restored production helper:
64 original seeded sources, 21 overlapping/unaligned candidate layouts each,
low bits before final rounding and borrowed input preservation. It passes normal
and scalar on both OS. No test-only production helper or forwarding API remains.

Both production files and qualified binaries are restored exactly. Existing
4,911 full checks/OS, 597 affected scalar checks/OS and the same actual private
0.2.0 package/all three examples are reused. Current summary rows stay unchanged;
[selected qualification](WARM-VP8-LUMA4-RATE-BOUND.md) remains authoritative.
[Evidence](warm-vp8-source-energy-screen-evidence.json) preserves the rejected
producer, raw screen, restoration and test identities. Stop source-energy tuning;
next profile the existing caller-RGB420 JPEG boundary. Full warm 1× remains open.
