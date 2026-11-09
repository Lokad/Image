# Current text geometry attribution

TRP704 takes one two-second capture of complete prepared-layout RGBA8 drawing
using the qualified JGT702 actual-package DLL. It reuses the frozen DejaVu Sans
face, original harness and existing diagnostic runner, with zero builds or new
timing confirmations. CPU0 executes and CPU1 samples; both idle-v2 guards and
every exact frozen frame/visual/ownership check pass. Instrumented elapsed is
discarded. Selected benchmark costs and package qualification remain unchanged.

Of 1,993 sampled locations, Band has 377, generic sorting 319, Area 210, edge-X
evaluation 170, BuildEvents 136, clipping 129 and Draw 75. Same-capture JIT sizes
resolve the erased generic sort names: crossing sort has 238 locations and event
sort 81. Stack attribution assigns 261 sort locations to event preparation, 55 to
Band and three to Draw. These are exclusive sampled locations with skid/inlining,
not phase times, call counts, stall proof or predicted speedups.

Draw clears and quantizes the full canvas-width row per glyph row, but this
capture supplies no basis for prioritizing another range micro-optimization.
The earlier [row-range study](TEXT-ROW-COMPOSITION-INCONCLUSIVE.md) and latest
[transparent-destination experiment](TEXT-TRANSPARENT-DESTINATION-EXPERIMENT.md)
retain their inconclusive/adverse evidence. Each unique shape is already prepared
once per draw; no duplicate-shape preparation is found. Geometry/event preparation
remains a distinct lead for a materially revised design. Independent codec and
resizing work continues without a text reference block.

The reference remains the approved ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 complete
public integration on ImageSharp 3.1.12. Prepared drawing includes geometry,
events, coverage, blending and disposal; resident font/layout and canvas reset
are excluded equally. The full integration separately includes every layout call
and fitting trial. Neither comparison is bare shaping or FFmpeg performance.
Full 16-bit samples and exact managed pixels remain required.

[Evidence](text-current-geometry-attribution-evidence.json) pins sources, module,
JIT/map/trace, resolved types/callers, original gates and borrowed runtime providers.
Mirrored/member-verified raw `text-current-geometry-attribution.tar.xz` contains
109,536 B / 25 members, SHA256
`c68c5997dd9e7c0e0ffd778ab6768b0c8107d643c7da1788979e83283d5501e6`.
Unchanged current Windows/Linux 6,073-check, scalar/affected-ISA and SAME actual
package/three-example qualification is reused. No renderer dependency is added;
the 283-path objective remains open and publication is postponed.
