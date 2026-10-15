# VP8L decoding with a live token

Complete resident-input VP8L decoding with a live, never-signaled cancellation
token independently confirms **1.906× FFmpeg / upper 1.919×**, at **1,216,180
managed B/op**. Every decoded RGBA byte, including nonzero hidden RGB, is exact.
Production is unchanged; its existing qualification is reused.

| Same immutable build | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 5,760.17 | 3,008.20 | 1.915 / 1.982 | 1,216,180 |
| Independent twelve-actual confirmation | 5,757.19 | 3,020.47 | 1.906 / 1.919 | 1,216,180 |

The original 513×259 byte raster and existing frozen 183,812-byte native VP8L
packet are reused. It is the same adaptive-coding input as the ordinary
[native-input comparison](WARM-VP8L-CANONICAL.md); no input, image, profile,
quality or effort grid is generated. One new selector exercises the existing
public cancellation boundary.

Both subjects time complete identical-packet decoding, independent owned RGBA8
acquisition and release. Managed admission, entropy/transform work and token checks
are included. Native software WebP decoding, state reset, ARGB-to-RGBA conversion,
output allocation and release are included; codec/conversion contexts and packet/
frame wrappers are reused. Every operation decodes and converts all pixels, with
no output cache. Token/source creation and disposal, startup, I/O, IPC, fixtures
and validation are excluded. Native has no equivalent cancellation option.
Native allocations are **unmeasured**; managed figures include harness overhead.

Pre-canceled rejection preserves token identity; the timed token remains live and
unrequested. Exact normal/scalar native CLI samples, analytical RGBA, alpha/hidden
RGB, geometry, source preservation and two simultaneously live independent owners
pass outside timing. Sequential Release runs use one pinned CPU, one codec/
conversion thread, normal SIMD, disabled tiering and workstation nonconcurrent GC.
Every quiet guard passes. Screen and confirmation share the same immutable core/
harness; all twelve actuals are retained and selected. The upper includes selected
and all-actual intervals against the faster native bracket's lower bound.

Only one benchmark input changes. One harness build and cached generated jobs
suffice, with verified current DLLs preloaded into every measured child. Existing
**3,432 Release tests per OS**, including **898 live comparisons**, and the same
actual package's consumer plus all three examples on Windows/Linux are reused
from the [JPEG qualification](WARM-JPEG-BASELINE-AC-RUN.md). No production/native
worker rebuild, full suite, package proof, method audit or profile is repeated.

[Numeric evidence](warm-vp8l-cancelable-evidence.json) retains every actual,
allocation, guard, input/source/assembly/job/native identity, token check and
qualification reuse. Stop this confirmed pair; full required warm coverage stays open.
