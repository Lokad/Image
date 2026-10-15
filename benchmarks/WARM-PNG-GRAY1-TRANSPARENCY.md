# One-bit grayscale PNG transparency

The qualified packed-row decoder independently confirms **0.094× FFmpeg plus
the exact timed reference correction / upper 0.098×**, with **520 managed B/op**.
Production is unchanged; its full package qualification is reused.

| Same immutable build | Lokad.Image µs | FFmpeg + color-key µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 62.78 | 684.51 | 0.092 / 0.103 | 520 |
| Independent twelve-actual confirmation | 64.68 | 684.86 | 0.094 / 0.098 | 520 |

One fixture reuses the original byte raster's black/white projection:
`gray=(red>>7)*255`, equal RGB, alpha zero for the one-bit tRNS key1 and opaque
otherwise. White RGB under zero alpha is retained. Original PNG authoring uses
MSB packing, zero unused padding, filter None and BCL Optimal level6. The packet
and projected raster match the previous retained correctness diagnostic exactly.
No additional original image, depth/key/filter grid or production tuning is needed.

The pinned FFmpeg decoder ignores one-bit transparency. The existing
[SPEC reference policy](../SPEC.md) supplies exact alpha through its public
`colorkey=white:similarity=0.01:blend=0` filter after full MONOBLACK-to-RGBA
conversion. This is an explicitly corrected reference pipeline. The raw decoder
failure remains [unscored](WARM-PNG-PACKED-TRANSPARENCY.md).

Both subjects time complete identical-packet decoding into independent owned
RGBA8 and release. Native codec/input packet/frame wrappers and the filter graph
are reused; complete decoding, output allocation, conversion, buffer transfer,
filtering and release are timed. Transferring the owned conversion buffer lets
FFmpeg filter in place without an unnecessary extra reference or copy. All codec,
conversion and filter work uses one thread. Managed admission, framing, inflation
and packed expansion are included. Startup, I/O, IPC, fixture creation and
validation are excluded. Native allocations are **unmeasured**; managed figures
include harness overhead and released-storage reuse.

The actual native adapter produces exact analytical RGBA with normal and disabled
CPU flags before timing; its receipts verify one thread and two simultaneously
live independent outputs. Normal/scalar CLI pixels also match exactly. Managed
full samples, alpha/hidden RGB, geometry, source preservation and independent
owners pass outside timing. The harness rejects scalar native timing.

Sequential Release measurements use one pinned CPU, normal SIMD, disabled tiering
and workstation nonconcurrent GC. Both guards pass for each run. Screen and repeat
share the same immutable production/harness; all twelve confirmation actuals are
selected and retained. The upper includes selected/all-actual intervals against
the faster native bracket's lower bound.

Only two benchmark inputs and the original native adapter change. Existing
**3,428 Release tests per OS**, including **898 live comparisons**, and the same
actual package's consumer plus all three examples on Windows/Linux are reused
from the [packed transparency round](WARM-PNG-PACKED-TRANSPARENCY.md). No production
rebuild, full suite, package proof, method audit or profile is repeated.

[Numeric evidence](warm-png-gray1-transparency-evidence.json) retains every actual,
allocation, guard, source/assembly/job identity, exact adapter proof and
qualification reuse. Stop this confirmed pair; full required warm coverage stays open.
