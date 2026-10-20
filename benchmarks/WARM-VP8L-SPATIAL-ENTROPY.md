# Lossless WebP spatial coding groups

The accepted encoder reduces the fixed photograph from **184,518 to174,748 B**
(5.29%, closing39.9% of its remaining byte excess). The diagram retains7,636 B.
The required ceilings,160,032/7,234 B, remain open. These are the same opaque
513×259 controls, libwebp `exact=1` (FFmpeg's encoder backend), method4/quality75.
Inputs, settings and exact alpha/hidden-RGB requirements are unchanged.

All six existing complete encodes independently pass warm parity on one CPU.
Bounds retain selected/all actuals and native brackets, with the versioned quiet
VM guard. Times are microseconds; allocations are managed bytes per operation.

| Existing case | Managed / reference µs | Ratio / upper | Allocated B/op | Managed / reference packet B |
| --- | ---: | ---: | ---: | ---: |
| DiagramLossless8 | 7,820.91 / 10,860.00 | 0.720 / 0.729 | 107,568 | 7,636 / 7,234 |
| PhotographLossless8 | 76,320.26 / 96,413.88 | 0.792 / 0.801 | 9,912,952 | 174,748 / 160,032 |
| Encode8 | 66,545.73 / 141,275.61 | 0.471 / 0.478 | 10,286,728 | 145,648 / 183,812 |
| Encode16 | 76,499.50 / 140,691.44 | 0.544 / 0.549 | 10,286,320 | 145,648 / 183,812 |
| EncodeOpaque8 | 14,963.90 / 82,930.81 | 0.180 / 0.182 | 614,408 | 170,156 / 121,896 |
| WebpLossless8AsyncNonSeekable | 67,963.11 / 140,030.24 | 0.485 / 0.492 | 10,286,704 | 145,648 / 183,812 |

One fixed8×8 map selects up to nine prefix-code groups after four deterministic
coding-cost assignments. Residuals, cache behavior and copy decisions stay intact.
Complete map/header/body costing retains the original smaller packet. Eligibility
requires the existing LEFT-selected, literal-heavy path and sufficient configured
local memory/work. No general-image size claim follows.

This spends extra work and allocation for compression: photograph9,912,952 B/op,
versus historical702,616 B/op. Tile counts/indices, map/ranking and overlapping
tables are admitted before allocation through the same borrowed root. Temporary
histograms end before output. Shared refusal is terminal; cancellation, cleanup,
recovery and independent owners pass. Tight local limits can choose the earlier
algorithm before shared admission, without inspecting root availability.

Both platforms pass4,575 tests each (including932 live FFmpeg), plus24 internal
and116 public affected scalar checks. One production build per OS is reused for
12 scores/90 actuals/24 passing guards; no harness, selector, native-worker or
differential-test rebuild. The same actual private0.2.0 package and all three
usage examples pass on both OSes. Published0.1.0 is unchanged.

Earlier cache/LEFT admission fixtures explicitly select their original64-KiB
local workspace and derive output limits at that allowance. New public controls
cover larger analysis. An initial overstrong clustering assertion was corrected
without rebuilding production; grouping need not find a global optimum.

An earlier untimed cross-color probe saves only2,812 B beyond its independent
adaptive-predictor basis and grows the diagram; it is not adopted. The independent
spatial-group prototype saves9,972 B on current photograph residuals/tokens,
motivating this primitive. Managed and normal/scalar FFmpeg pixels are exact;
no prototype timing/allocation claim is made.

Exactly six encoder observations refresh. Other paths retain their identified
evidence;31 C43 metadata timings remain open. The finite warm checklist, required
additional-font inventory, remaining size gates and replacement readiness remain
open. Next audit: existing Opaque8 cached/raw size170,156/121,896 B, with frozen
inputs/settings and no matrix expansion.

[Numeric evidence](warm-vp8l-spatial-entropy-evidence.json) includes every actual,
allocation, guard, quality check and producer identity. Mirrored raw archives:

- `c47-qualified-windows.tar.gz`: `36414e34d574461a5e5112d0fa3f54b0f0872ffe665b6e1fb65dae256f6a3e00`

- `c47-qualified-linux.tar.gz`: `ba7febb5dd0b03c9f9ada5224e855656d3fc2cafc281a1f7af8ea70ceb5a1a55`

- `c47-measurements.tar.gz`: `d4c8514f3cc1c3cfd622f33358e077e4e569a6f793f60237ae6765563ddead04`

- `c45-c46-lossless-feasibility.tar.gz`: `c3404f30aece8eaffa065977423d555f6732d31bdbcd7ef9603b0cf2c751953e`

Actual private package SHA256: `bab07ef06cffeef946ecf16006cf1a5c4ed4f54f4ffdaa0eaa0cdd2c7b4290b9`.
