# Material warm coverage

The [benchmark summary](../docs/benchmark.md) reports confirmed fixed workloads,
not complete coverage of the library contract. Keep unknown cases open and add
one representative only when it exercises distinct substantial work. Reuse the
two existing original 513x259 RGBA8/16 rasters; documented projections for another
pixel format do not introduce another original image. Normally run one pair per
round, at most three, with correctness established before timing.

| Primitive group | Retained coverage | Remaining material work |
| --- | --- | --- |
| PNG decode | RGBA8/16, None/Paeth; RGBA16 Adam7; RGB16 None screen remains above 2x | Gray/gray-alpha sample expansion; packed/palette expansion; other filter implementations |
| PNG encode | RGBA8/16, level 6/None | RGB and gray projections; compression policies with materially different work |
| JPEG decode | Baseline/progressive gray, 4:2:0, 4:2:2, 4:4:4 | Distinct restart handling or unusual supported layouts if they dominate work |
| JPEG encode | RGBA8 to 4:4:4, matched quality | Default 4:2:0, 4:2:2, grayscale, full 16-bit source normalization |
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
eligible bicubic or default JPEG 4:2:0 pair. No averages, easy-case
selection or tolerance changes establish the uniform warm target.
