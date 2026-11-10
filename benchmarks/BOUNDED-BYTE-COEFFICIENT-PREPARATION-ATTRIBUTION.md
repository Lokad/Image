# Bounded-byte coefficient preparation attribution

The qualified current package supplies one two-second CPU0 capture, sampled on
CPU1. No production/test change, build, timing confirmation or new workload.
Instrumented elapsed is discarded. The source-valid complete513x259->256x129
RGBA8/4KiB cost remains **504.61/zimg417.18us**, upper1.216,113Bop: approximately
87.44us remains. The three relevant source files match the qualified shared-byte
stage; current6158full/live, scalar/ISA and SAME-package evidence are reused.
CSP757 commit64a5c448 CI42791320323 is green in all three jobs.

| Current sampled body | Code bytes | Locations out of1993 |
| --- | ---: | ---: |
| Large byte strip | 5891 | 639 |
| Small byte strip | 6015 | 1186 |
| Geometry Build/Read/Prepare | — | 55 |
| Exact original-order byte repair | — | 4 |

Generated code still repeats scalar coefficient broadcasts and lane inserts while
filtering each source row. Vertical row lookups and publication are also hot.
Inlining/skid prevent treating counts as phase times, removable percentages or
speedup forecasts. Geometry setup and exact replay are not dominant sampled
locations. Both idle-v2 guards and frozen rational/native/source/ownership gates pass.

## Next architectural hypothesis

Prepare eligible eight-tap/+2 byte pairs as interleaved exact doubles once per
strip, inside the existing coefficient capacity. Each two-tap step could then
load four doubles once and permute their lanes twice, replacing four scalar
broadcasts and two inserts. Coefficient bytes loaded and both independent ordered
Multiply/Add traces stay unchanged. This is a preparation/layout hypothesis,
distinct from the discarded vertical-quad implementation.

The bounded model identifies124primary pairs and32pairs on the existing odd/
clipped151x83->75x41 functional control. It checks12480coefficient-layout/lane
bit comparisons plus16signed-zero/near-half pattern comparisons. Logical scalar
coefficient loads513856 become128464packed loads on the primary control, with
the same loaded bytes and no actual SIMD or CPU gain yet established. Four/five
generic pairs and the odd tail keep their original layouts. No new perf matrix.

Before initial decoding, the existing decoded-row workspace can stage16doubles
for the transpose. After horizontal filtering has finished, that workspace has
no remaining user until a subsequent source-row miss rebuilds it. Exact scalar
repair can borrow two decoded vectors to restore eight coefficients for one
uncertain pixel. The existing repair reads original source and coefficients,
not the dead decode workspace. No added array, capacity, owner, root, pool, session
or public API is proposed. Word, clamped/unequal/generic/odd paths remain original.

Next implement and test the actual layout/kernel/replay boundary: exact bits,
canaries, phase lifetime, no allocation, cancellation and borrowed admission;
inspect emitted code and word-mode erasure before the existing short screen.
Use independent complete-operation confirmation only if useful, reconcile affected
costs, and qualify coherent production stages on both OS/scalar/ISA/SAME package.
Preserve numerical policy, tiny-alpha unwind, terminal refusal and result loans.
Failed implementations remain evidence, without permanent family bans. Independent
codec/text work remains eligible; publication stays postponed.

[Evidence](bounded-byte-coefficient-preparation-attribution-evidence.json) pins
source identity, code annotations, bounded model and reused qualification.
Mirrored/member-verified raw `bounded-byte-coefficient-preparation-attribution.tar.xz`:
127812B/25members, SHA256
`a2607c69e5bb36e802078c7c6c333a5132fcfe570894da36e713392e00caf583`. Earlier
[attribution](BOUNDED-COLUMN-CURRENT-ATTRIBUTION.md),
[vertical study](COLUMN-VERTICAL-QUAD-STUDY.md) and
[shared publication](BICUBIC-SHARED-BYTE-PUBLICATION.md) retain their raw history.
No renderer dependency or current-cost change is added. All283paths remain required.
