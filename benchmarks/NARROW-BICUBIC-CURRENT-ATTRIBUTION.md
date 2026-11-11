# Current bounded narrow bicubic attribution

NCA792 observes the accepted immutable package on the existing complete
513×259→256×129 RGBA8/4KiB Resize. It reuses the declared472.44/native417.11µs
cost (~55µs point gap); this is not a new timing confirmation. Column/geometry
sources match the [row-loan stage](NARROW-BICUBIC-ROW-LOAN.md), and current shared
math is pinned to accepted CNP789 qualification.

One two-second CPU0 capture, sampled from CPU1, has1,996 locations. Row-loan
execution has1,706 and tail execution195; geometry Build/Read11 each, Kernel19.
Hot locations include horizontal window shuffles, vertical row/bounds/weight
traversal and ordinary certified publication. Separate original repair has one
sample; do not infer its total cost from this. Inlining, skid and sample counts
are not phase timings, removable percentages or gain forecasts. Instrumented
elapsed time is discarded. Frozen pixels/native bounds/ownership and idle-v2
checks pass. No production/tool/native build or new image/settings grid.

The bounded executor still accumulates pairs independently. Enlargement already
has a four-pixel accumulator that preserves every channel's ordered multiply/add.
Moving that helper into shared math and using one pending second-pair vector could
share vertical row/weight traversal while keeping the single existing publication/
repair loop and original pair/scalar tails. It adds no cache, owner or capacity.
The byte/word generic schedule must both qualify. Existing scalar/portable paths,
normalizer order, certificates, exact repair and result-row loan lifetimes remain.

The primary has vertical tap counts[8, 9]; vertical loop iterations
would fall132,736→66,368. Pixel loads, arithmetic and outputs
stay unchanged. This is a logical work model, not elapsed-time evidence. A pending
64B register value, extra branch, code size and register pressure are explicit
risks. Actual ordered-lane/canary checks precede existing paired complete short
screens. No numerical-policy relaxation or automatic acceptance is proposed.

[Evidence](narrow-bicubic-current-attribution-evidence.json) pins source, modules,
capture, code locations and hypothesis. Mirrored/member-verified raw
`narrow-bicubic-current-attribution.tar.xz`: 119,260B/24members, SHA256
`c9fbd51acf95b16e36ff7fd4613d861d24fc360b80d6bbb1b121b5e7a2d67757`. Its CNP789 dependency supplies the actual
DLL/package, unchanged fixtures/tools and6,248full/live/ISA/SAME-package proof.
Current benchmark costs remain unchanged and gain no diagnostic dependency.
All289 required paths remain; publication is postponed.
