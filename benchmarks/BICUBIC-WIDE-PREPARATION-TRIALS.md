# Tight RGBA8 bicubic preparation trials

Both trials use the original 513×259 to 128×64 tight RGBA8 control and the
actual 0.2.1 package `1699cfb1` with Linux DLL `bb3d063d` as the managed baseline.
The complete public resize includes geometry, filtering, exact repair,
admission, independent output and disposal. The prepared zimg reference retains
its original graph and scratch. Every run preserves exact managed pixels,
approved native bounds, source data and output ownership. Both idle-v2 guards
pass for each run on the dedicated VM at one CPU.

| Trial | Baseline or zimg µs | Private candidate µs | Candidate ratio interval | GC B/op, managed |
| --- | ---: | ---: | ---: | ---: |
| Deferred fallback views, three managed pairs | 348.32 | 351.48 | 0.984936–1.033795 | 112 on each |
| Quartet eligibility per tile, three managed pairs | 389.21 | 375.25 | 0.907512–1.024281 | 112 on each |
| Quartet eligibility per tile, twelve new managed pairs | 349.10 | 340.51 | 0.967564–0.983278 | 112 on each |
| Quartet eligibility per tile, standalone versus prepared zimg | 377.14 | 377.69 | 0.987161–1.009544 | 112 |

Deferring checked views to their consumers gave no useful complete-operation
gain in the wide implementation. That trial stops after its short screen.

The second candidate evaluates quartet eligibility once per horizontal tile.
The existing pair markers, counts and offsets stay constant while rows use
the tile. A local mask records the original predicate for each eligible
four-column start in the at-most-16-column byte row-loan tile. It preserves
pair and scalar fallback selection, arithmetic, repair and cancellation. The
candidate adds no owned or retained buffer capacity. The timings measure GC
allocations separately. Physical peak and native retained state were unmeasured.

The twelve new pairs confirm a scoped 2.46% managed gain. The same immutable
private DLL remains uncertain against zimg in the standalone schedule.
That run reuses the original 64-call adjacent driver and its 128 pinned
non-core files, with twelve managed units and all three native groups.
Native group means are 378.58, 377.14 and 378.56 µs. The earlier and later
session clocks vary across both subjects. Their difference has no verified
cause. The candidate remains private and the original TightWide8 case stays
open under the conservative upper-ratio gate. Retain its existing standalone
comparison and defer identical sampling without a concrete diagnosis.

Two private core builds and one actor build cover both trials. The second
candidate reuses the actor and its own DLL across all three captures.
The release candidate package, displayed timings and prior Windows/Linux,
scalar and actual-package qualification remain unchanged.

[Evidence and archive manifests](bicubic-wide-preparation-trials-evidence.json)
pin the four mirrored archives and all 68 members. They retain both candidates,
exact replay commands, all actuals, guards and validation reports. The
[current package comparison](BICUBIC-CURRENT-PACKAGE.md) retains the three
original bicubic uncertainties.
