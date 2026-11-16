# Current ordinary text clipping attribution

One two-second profile uses the current immutable package on the existing
DejaVu Sans 2.37 “AV To”, 18 px, 384×160 RGBA16 control. Font/layout are retained;
each public DrawText includes geometry/events/workspace preparation, scan,
composition and cleanup. The existing runner performs **27,016 calls**, resetting
and checking the complete frozen canvas between calls. Application CPU0,
collector CPU1, both idle-v2 guards and original correctness/owner gates pass.
There are no builds, new fixtures, elapsed comparisons or qualification repeats.

Of **1,994 sampled locations**, 1,478 map to managed code. Band has 345, Area 250,
BuildEdges 122, BuildEvents 115 and Rasterize 102. Sorting leaves include
MergeBuffered 94, Partition 65 and TryNatural 40. Actual same-capture JIT sizes
resolve Crossing/double specializations; the raw audit retains them. Area is
938 B and Band 2,401 B. These counts include skid, inlining/unwind limits, resets,
validation and GC; they are not phase durations or removable percentages.

Two Area blocks with 112/22 sampled locations contain two floating divisions each:
horizontal clipping intersections whose ratios are multiplied by zero endpoint
Y differences. This supplies a concrete lead, not proof that those samples are
division stalls or a prediction of gain.

The next bounded candidate should certify the finite horizontal-intersection
identity, preserving signed-zero cases, original polygon/shoelace order, coverage
and blending. Nonfinite inputs retain the original arithmetic. It adds no owner,
array, cache, pool, public API or numerical policy. Guard/code-size cost may
outweigh removed divisions. Require the independent always-clipped bit oracle,
extreme/subnormal/signed-zero/nonfinite cases and all eight frozen full-word
frames before a complete ordinary16 paired screen. Confirm independently only
when useful, then reconcile affected complete public costs before one coherent
Windows/Linux/scalar/actual-package milestone. Stop unchanged prepared draw paths.

Current [complete one-shot/eight-draw comparisons](FONT-PACKED-BOUNDS.md) remain
passing; [ordinary/prepared drawing costs](TEXT-PREPARED-COVERAGE.md) stay unchanged.
The ordinary16 gap is scoped. This profile does not establish an overall text
slowdown, parser equivalence or an end-to-end gain by adding historical stages.
Reference identities remain ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12,
with zero elapsed reference samples here. Existing eager/lazy validation and
retained coverage/positioned-path differences remain explicit. GC allocations,
retained capacity and peaks are not remeasured.

The actual package/DLL and 6,742 full/live checks per OS, affected normal/scalar/ISA
and same-package consumer/all three examples are reused unchanged.
[Evidence](text-ordinary-current-clipping-attribution-evidence.json) pins sources,
code locations, current producer and exact gates. Raw `text-ordinary-current-clipping-attribution.tar.xz`
(382,548 B/35 members), SHA256
`8ce71444ed18859f8c064df0383b1d7e32bafd41b0c517018f7bee273f50017e`, is mirrored/member-verified with the package and prior-runner
dependencies. It retains perf trace/map/JIT, full stacks, original runner/harness,
scripts, guards and frame checks. Reproduce with archived `tmp/toc1118-profile.py`
and `tmp/toc1118-audit.py` in a fresh ignored workspace with pinned dependencies;
never restart a completed capture. Full 289-path parity remains open; publication
is postponed.
