# Skip transforms for exactly matching source planes

The existing effort2 diagram independently measures 4.898 ms: 1.947× reference
time (upper 1.955×), an exact 7,330-byte packet and unchanged 741,342 B/op.
Elapsed time is 3.87% below the preceding independent mean; separate runs
are not a paired baseline gain. All 12 actual/filtered samples remain. Initial
main-short uncertainty is resolved by that confirmation. RDO diagram remains
uncertain; other rows are guarded shorts, without independent gain/parity claims.

| Existing quiet one-CPU control | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| DiagramLossyDistortion8 short | 4,845.94 | 2,522.85 | 1.921 / 3.033 | 741,342 | 7,330 |
| DiagramLossyDistortion8 confirm | 4,898.25 | 2,515.91 | 1.947 / 1.955 | 741,342 | 7,330 |
| PhotographLossy8 short | 17,613.12 | 9,680.61 | 1.819 / 1.878 | 809,056 | 17,688 |
| DiagramLossy8 short | 9,641.21 | 5,840.99 | 1.651 / 2.430 | 741,121 | 7,100 |
| DiagramLossyTrellis8 short | 17,045.22 | 9,804.30 | 1.739 / 1.814 | 754,328 | 6,818 |
| vp8 short | 77,052.81 | 53,810.44 | 1.432 / 1.557 | 23,258,168 | 98,590 |
| EncodeOpaque16 short | 26,955.09 | 15,617.77 | 1.726 / 1.930 | 1,201,536 | 72,380 |

After DC/TM prediction, exact source equality lets analysis skip transforms and
histogram calls for a complete 8×8 or 16×16 plane. Zero residuals retain a unit
forward-transform bias at coefficient 1; every magnitude still belongs to bin0.
Add exactly size² to that population. The predicate uses byte equality; rounding,
source scores, segmentation and complete packets remain exact. Aliased or short
population buffers retain the original path, with direct row cancellation checks.
Same admitted scratch, capacities and lifetimes; no cache, heap storage or owner.

Both systems pass 4,947 Release checks (1,965 internal, 2,035 public, 947 live), 626 scalar
WebP checks and 50 focused checks. The new literal bias/flat-source test exercises
actual DC/TM Y/UV summaries, source immutability and retained root charges.
Existing native source/decision, packet, ownership and cancellation gates remain.
The same actual private 0.2.0 package and all three usage examples pass both systems.
One immutable production/linked-test build per OS, existing workers/harnesses/
selectors and unaffected qualification were reused; no additional profile.

[Evidence](warm-vp8-source-identical-planes-evidence.json) retains identities,
samples, qualification and a mirrored/member-verified raw archive. Complete
normalization, fresh output ownership and release remain timed. Six named VP8
anchors replace earlier costs; unrelated decoder/codec/edit/text evidence remains
valid. Stop shortcut variants. Full 255-path upper ≤1× and replacement readiness
remain open; the effort2 case is still the largest confirmed gap.
