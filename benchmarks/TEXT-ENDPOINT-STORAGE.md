# Shared text endpoint storage

TED698 omits an endpoint when it equals either endpoint of the immediately
preceding contour edge. The final distinct event coordinates, intersection
arithmetic, edge ordering, coverage, blending and public boundary remain unchanged.
Two local values replace redundant event insertion, growth copies and sorting;
there is no new array, field, cache, retained state or API.

| Complete prepared-layout drawing | Original µs | Current µs | ImageSharp.Drawing µs | Ratio / upper | Allocated B/draw |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8 | 114.12 | 98.44 | 50.45 | 1.951 / 3.155 | 59,000 |
| RGBA16 | 111.53 | 102.57 | 51.14 | 2.006 / 3.208 | 59,000 |

The independent confirmation has favorable filtered points of13.74%/8.03%,
and all-actual points of13.74%/8.15%. Raw and normalized intervals overlap:
**CPU gain remains inconclusive**, and both reference gaps remain. No timing
repeat follows. The stage is retained for simpler exact event storage and
reproducible memory savings, without inferring speed from fewer allocations.
Original/candidate three-actual screens and twelve-actual confirmations are kept.

Latin allocation falls67,368→59,000B/draw, or8,368B/12.42%. Separately measured
admitted drawing workspace falls48,544→42,336B for Latin,275,776→238,848B for
Arabic and47,896→41,688B for fitting; narrow wrapping stays7,424B. All eight
frozen RGBA8/RGBA16 frames remain exact. Borrowed font/layout/canvas storage
is outside this draw-only root; scratch retention is zero after disposal.
These coarse admitted peaks are not physical heap peaks. Native peaks and
allocations remain unmeasured.

The frozen DejaVu Sans18px “AV To”,384×160 canvas and origin8/8 are unchanged.
Complete drawing includes geometry/events, integration, blending and disposal;
resident font/layout and canvas reset are excluded equally. The intended Windows
Release-package DLL supplies timing on the quiet single-CPU Linux VM, with
contemporary originals and unchanged harness/reference binaries. Linux-native
production is separately qualified.

The reference is explicitly ImageSharp.Drawing2.1.7 / Fonts2.1.3 on ImageSharp
3.1.12. Its adapter retains public paths and touches bounds before timing, but
nonidentity translation reconstructs segments/tessellation and scanner storage
inside each Fill. The audit supplies no basis for excluding managed geometry
or adding a prepared API. The full text integration workload includes all public
layout calls and fitting trials; this prepared draw is its separate repeated-state
boundary, not a bare shaping comparison. Approved visual tolerances retain full
16-bit samples and do not relax exact managed pixels.

Six independent event-set/cancellation cases cover shared, touching, crossing,
disjoint and signed-zero endpoints against the original all-pair construction.
Only the internal representative sign of a zero coordinate may differ; it does
not change a numerical event coordinate. Actual frozen frames and existing exact
arithmetic, layout, ownership, refusal and cancellation tests remain mandatory.
Windows/Linux each pass **6,064 full/live checks**, plus170internal/185public
affected checks per normal/scalar mode. The SAME actual private Release package
and all three usage examples pass both OSes. No ISA branch changes; unaffected
ISA qualification is reused.

One main/embedded build per OS serves the entire round. Font loading, layout and
fitting without rendering, composition, codecs and resizing retain unchanged-path
evidence. This is a scoped development checkpoint, not a full release refresh.
[Evidence](text-endpoint-storage-evidence.json) identifies modules, observations,
storage, package and mirrored/member-verified raw evidence. Raw
`text-endpoint-storage-qualified.tar.xz` contains16,922,396B /251members,
SHA256 `2b0939e933183cd40aa9e9d847f1de199c1d85d065801876c67449ba46b67a90`.
The actual private package is438,323B, SHA256
`a02bfd52430b3cf01b74698ced949b5838e48b7220d856bc687ad85d6ad3a51f`.
Reporting recovery
reuses completed short jobs; no actual measurement is repeated.

[Prepared pixels/order](TEXT-PREPARED-PIXELS-AND-ORDER.md) retains the preceding
costs, work-reuse rationale and worker providers. The full283-path performance
objective remains open; independent codec/resizing work continues. Publication
is postponed.
