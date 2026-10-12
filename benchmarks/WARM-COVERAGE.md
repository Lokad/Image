# Material warm coverage

The [benchmark summary](../docs/benchmark.md) reports confirmed fixed workloads,
not complete coverage of the library contract. Keep unknown cases open and add
one representative only when it exercises distinct substantial work. Reuse the
two existing original 513x259 RGBA8/16 rasters; documented projections for another
pixel format do not introduce another original image. Normally run one pair per
round, at most three, with correctness established before timing.

| Primitive group | Retained coverage | Remaining material work |
| --- | --- | --- |
| PNG decode | RGBA8/16, None/Paeth; RGBA16 Adam7; RGB8, gray16 and gray-alpha8/16 None confirmed; RGB16 and gray8 None screens above 2x | RGB16/gray8 parity; packed/palette expansion; other filter implementations |
| PNG encode | RGBA8/16, level 6/None | RGB and gray projections; compression policies with materially different work |
| JPEG decode | Baseline/progressive gray, 4:2:0, 4:2:2, 4:4:4 | Distinct restart handling or unusual supported layouts if they dominate work |
| JPEG encode | RGBA8 to 4:4:4, matched quality; 4:2:0 repeat mean below 2x, upper remains above | 4:2:0 upper-bound gap; 4:2:2, grayscale, full 16-bit source normalization |
| WebP | VP8 with alpha decode/encode; VP8L native and managed inputs; exact VP8L backend encode | Material opaque/alpha alternatives and full 16-bit source normalization |
| Resize | Nearest/bilinear at both depths, fixed downsample | Bicubic at both depths; upsample/wide-support paths; tight-memory fallback |
| Exact edits | Both depths and every orientation/permutation, fixed shapes | Retained live output owners versus released-storage reuse |
| Public boundaries | Resident encoded span to owned output; complete single operations | Fresh Stream decoding and decode/orient/crop/fit compositions |

Centered/clamped crop, AutoOrient and ResizeToFit route to the measured copy or
resize primitives after small geometry/metadata work. They need correctness
controls and one relevant composed boundary, not a duplicate matrix of timings.
Fresh Stream and retained-owner boundaries remain distinct from the reported
resident-span/released-storage results.

The first additional codec screen uses noninterlaced RGB16 PNG, filter None, preserving
the original RGBA16 RGB samples and setting alpha to 65535. This exercises the
generic sample-expansion path absent from the existing RGBA rows. Native timing
includes RGB48BE-to-owned-RGBA64LE conversion on one thread. The
[row-expansion round](WARM-PNG-RGB16-ROW.md) reduces its screened mean 36.5%,
from 3.167x to 2.014x; upper 2.187x remains open. Rank this gap against the next
eligible bicubic pair. The [4:2:0 luma round](WARM-JPEG420-LUMA-ROW.md) reduces
its screen from 4.206x to 3.462x, upper 3.731x. This larger measured gap now
was prioritized. The [chroma round](WARM-JPEG420-CHROMA-ROW.md) lowers its
independent mean to 1.978x; upper 2.022x remains open. Return to material unknown
paths before further small tuning. Two [framing experiments](WARM-PNG-LITERAL-REJECTED.md) were
reverted without a useful parity gain. No averages, easy-case
selection or tolerance changes establish the uniform warm target.

The [gray8 round](WARM-PNG-GRAY8-ROW.md) exposes a 7.921x gap and reduces it to
2.245x by direct expansion, with the same existing raster projection. The updated
profile places the remaining substantial cost in inflation/framing. Keep this
gap and the JPEG/RGB16 upper-bound gaps open; use one primitive per round.

A [compact literal classifier](WARM-PNG-LITERAL-CLASSIFIER-REJECTED.md) screen
lowered gray8 mean only 3.1%, to 2.181x (upper 2.522x), and was reverted.
The summary retains the accepted gray8 result. The
[gray-alpha8 row](WARM-PNG-GRAY-ALPHA8-ROW.md) then lowers its newly covered
3.358x screen by 59.0%, confirming 1.372x / upper 1.409x. Stop tuning that fixed
pair. The [RGB8 row](WARM-PNG-RGB8-ROW.md) then lowers a new 2.956x screen
by 51.9%, confirming 1.416x / upper 1.440x. Stop tuning this fixed pair. The
[gray16 row](WARM-PNG-GRAY16-ROW.md) lowers a newly covered 4.199x screen
by 56.8%, confirming 1.815x / upper 1.837x. Its failed first post-confirmation
guard remains unscored. Stop tuning this fixed pair. The
[gray-alpha16 row](WARM-PNG-GRAY-ALPHA16-ROW.md) reduces a new 3.114x screen
by 38.3%, confirming 1.776x / upper 1.785x. Stop tuning that fixed pair. Next
screen one indexed-palette8 projection; preserve framing checks and prioritize
substantial expansion/data-flow gaps.
