# PNG Stream, orientation, crop and fit

The complete RGBA8 composition independently confirms **1.891x FFmpeg /
conservative upper 1.906x**, with **1,453,517 allocated B/op**. Every final RGBA
sample matches an independent integer-coordinate reference. Production is
unchanged; this fixed public boundary clears the target. Uniform current-source
coverage remains open.

| Measurement | Lokad.Image us | FFmpeg us | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 2,792.37 | 1,480.50 | 1.886 / 2.464 | 1,453,517 |
| Independent twelve-actual confirmation | 2,795.71 | 1,478.63 | 1.891 / 1.906 | 1,453,517 |

Each operation creates a fresh read-only seekable MemoryStream with a non-exposed
backing array, decodes the frozen original PNG8, calls `AutoOrient` with its Normal
orientation, `CropCentered(256,129)` and
`ResizeToFit(128,64,false,ResizeFilter.Nearest)`, then releases every owner.
The crop begins at (128,65). Fit chooses ratio 64/129 and rounds width to127,
producing **127x64** pixels. The independent reference uses integer center-sample
coordinates into the original raster. Source preservation, hidden RGB, alpha,
caller-stream lifetime/EOF, final orientation and two-live-output independence
are checked outside timing. This covers normal orientation dispatch; it does not
claim composed coverage of other EXIF orientations.

All managed intermediate images are materialized and disposed. The native worker
performs the same fresh public AVIO acquisition and complete PNG decode as the
[Stream-only boundary](WARM-STREAM-PNG8.md), followed by a public FFmpeg graph:
Normal orientation, crop, exact point sampling of all four planes and packed RGBA
output. Plane extraction, merge, zscale sampling, packing and all release are
timed. FFmpeg may use intermediate views; its final result is independently owned.
No artificial intermediate copy is required. Codec/frame/graph wrappers are
reused, while every input packet is reacquired and every image is decoded anew.

The existing original 513x259 raster and identical frozen 218,775-byte PNG are
reused. No new original image, profile grid, tolerance or quality adjustment is
introduced. The final output SHA-256 is
`37e74deb6c0cbf5bb5fee349e1102b0a63371785201cdd22d2309b8f2fbd4dd2`.
The graph uses one codec/filter/conversion thread on one pinned CPU, with normal
SIMD, tiering off and workstation nonconcurrent GC. Startup, file/network I/O,
IPC, fixture construction, hashing and validation are excluded. Native allocation
totals are unmeasured; managed figures include harness overhead.

Both retained runs pass the versioned quiet guards. The first timing attempt is
skipped at 1.05% I/O wait against the 1% limit; its failed guard remains unscored.
An initial output-stride protocol refusal also precedes timing and is retained.
Confirmation retains all twelve actuals; BenchmarkDotNet selects eleven. The
published bound takes the larger filtered/all-actual interval against the faster
native bracket's lower bound. No failed guard or excluded actual is discarded.

Both runs use the same immutable qualified library and harness. Source inventories
show only four benchmark inputs changed. The existing **3,289 Release tests per
OS**, including **896 live FFmpeg comparisons**, and consumer plus three examples
through the same actual Release package are reused. No production rebuild,
full-suite repeat, package proof or whole-library method audit is needed.

The [numeric evidence](warm-png-composition8-evidence.json) retains every sample,
allocation, guard, generated job, assembly/source identity and qualification link.
Stop this confirmed pair; other material public boundaries and final current-source
coverage remain open.
