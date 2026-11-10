# Current byte bicubic enlargement attribution

BEP771 captures one two-second profile of complete `TightUp8` on the qualified
actual package DLL. It adds no production change, build or timing result. The
[source-valid comparison](BYTE-BICUBIC-ENLARGEMENT-CURRENT-PACKAGE.md) remains
3,114.50/zimg 3,041.43 µs, upper1.030 gap, **116 B/op**. The existing 513×259 to
1027×519 control retains its source+output+4 KiB boundary and independent output.

Of 1,994 sampled locations, 1,213 map to byte row publication, 319 to horizontal
filtering, 180 to tiny-alpha replay, 60 to original-order repair, 46 to vertical
preparation and 40 to source decoding. The actual publication body is 2,060 B
with 200 B explicit stack adjustment. Its combined division/certificate/packing
region has 614 locations; its vertical accumulation loop has 274. Inlining/skid
prevent treating these as phase costs, the cost of exactness or removable shares.
All instrumented elapsed time is discarded.

Full-width horizontal plans, row-cache reuse in unpublished output, prefix
retirement, 4 KiB tail strips and deferred tiny-alpha replay **already exist**.
The earlier strip capture predates that preparation and cannot attribute current
work. All three enlargement source hashes match the retained comparison. Exact
rational/frozen managed pixels, original native-only maximum1/RMSE≤.25, source,
ownership and idle-v2 guards pass. The labeled zimg backend retains graph/input/
scratch; managed preparation and both sides' complete conversion, independent
output and disposal boundaries remain explicit.

The next bounded design examines four-pixel row execution: share one vertical tap
traversal across two independent pixel pairs, then overlap the original divisions
before certification/publication. Inspect byte/word guards before deciding what
execution can be shared outside typed certificates. Preserve lane addition order,
separate multiply/add, full division, certificate margins, exact repair, tiny-alpha
behavior and pair/single tails. No reciprocal approximation, FMA, numerical-policy
change, owner, cache, session or extra buffer is proposed. Live-vector spills and
branching may offset the saved loop/control work; actual complete controls decide.

[Evidence](byte-bicubic-enlargement-current-attribution-evidence.json) retains
the proposal, method/instruction maps, source snapshots and reproducibility.
Raw `byte-bicubic-enlargement-attribution-bep771.tar.xz`: 105,472 B/22 members,
SHA256 `81d7890705c061cc35f342baed5dd680952680c0f7261a5d4bcd5572b4428661`,
mirrored/member-verified. The 6,182 full/live checks per OS, scalar/ISA and SAME
actual package/all-three-example evidence are reused. Physical/native peaks
remain unmeasured; this is no whole-release refresh or parity claim. All 283
required paths remain; publication is postponed.
