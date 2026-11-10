# Current live VP8 coefficient attribution

The subsequent [macroblock-window experiment](VP8-LIVE-MACROBLOCK-WINDOW-EXPERIMENT.md)
preserves correctness but establishes no useful gain; production is restored.
This capture remains attribution, rather than a removable-cost estimate.

AVP769 makes one two-second capture of the existing complete opaque live-token
VP8 decode on the qualified actual package DLL. Current source-valid cost remains
4,881.47/native 4,792.22 us, upper 1.028 gap, 229,344 B/op. This adds no timing or
allocation result, production change, build, fixture or qualification repeat.

Of 1,997 sampled locations, **1,441** fall in the live coefficient reader, 92 in
mode reading, 82 in the frame driver, 70 in pixel output and 60 in reconstruction.
The exact mapped reader body is 3,090 B with 248 B stack adjustment. Eligibility
adds 4 separate samples to its reader-family aggregate. Locations include inlining
and skid; they do not isolate lifecycle costs or predict removable time.
CPU0 executes/CPU1 samples; idle-v2, frozen 72,380 B input, exact 531,468 B managed/
native RGBA, source, active cancellation and independent-owner checks all pass.
Instrumented elapsed is discarded.

## Next bounded preparation/reuse design

Ordinary decoding already borrows one arithmetic window across 24/25 coefficient
blocks in a macroblock. Its 272 B/block partition guard is derived from RFC6386's
worst-case decisions and lookahead. The predicate excludes live cancellation.
Live decoding therefore creates/refills a window and normalizes/publishes it to
the scalar reader for each eligible coefficient block.

Extend operation-owned window reuse to live cancellation using the existing
polling coefficient core. Preserve every per-block/per-decision poll, successful
coefficients/context/cursor/following reads, strict partition bounds and current
short/per-block fallbacks. Reuse current window state/math; add no pool, session,
cache, heap capacity, root or owner. Keep the ordinary path protected. The current
live block already publishes only successful private state; repeating finally
removal would duplicate completed work.

Reuse the existing conservative bound and proofs. First inspect actual window/
caller lifetimes, then test a borrowed live window against scalar/per-block success,
cancellation, partition ends, tails, refusal/cleanup and surviving outputs. Add an
untimed applicability count only if a concrete eligibility uncertainty remains.
New ABI/ref-struct copies, branches or register pressure may offset saved lifecycle
work; judge complete existing live/ordinary controls before acceptance. No general
policy framework, weakened numerical/cancellation contract or new image/settings
grid is justified by this capture.

## Evidence and scope

The old validation capture omits reconstruction/filter/color; older complete
captures predate frame dequantizer preparation. This current full capture is
therefore useful attribution, without another baseline confirmation. Relevant
frame/reader/reconstruction/table sources match the qualified preparation stage.
Native codec/conversion contexts retain their documented boundary, and the
existing active-managed-cancellation asymmetry remains labeled. Necessary public
parse/preparation/normalization/output/disposal work stays inside complete timing.

[Evidence](vp8-live-current-attribution-evidence.json) retains source checks,
exact method maps, all sampled instructions and the proposed scope. Existing
6,182 full/live checks per OS, affected scalar/ISA and same actual Release-package/
all-three-example qualification are reused. There is no renderer dependency or
displayed-cost change. Mirrored/member-verified raw `vp8-live-macroblock-attribution-avp769.tar.xz`:
153,736 B /28 members, SHA256
`53fdfd0217c9d16f6afe7ec046949591cc5f2d53a69edd81fa6f44e8f7352831`. An older archive with a similar name is
preserved. All 283 material paths remain required; publication is postponed.
