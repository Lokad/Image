# Live-token opaque VP8 decoding

Complete opaque VP8 decode with a live, never-signaled cancellation token
independently confirms **1.838× FFmpeg / upper 1.843×**, at **760,548 managed
B/op**. Production is unchanged.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Short screen | 8,767.48 | 4,789.51 | 1.831 / 1.888 | 760,548 |
| Independent confirmation | 8,789.89 | 4,783.31 | 1.838 / 1.843 | 760,548 |

The existing 72,380-byte frozen native opaque keyframe and original 513×259
byte raster's opacity projection are reused. No new raster, shape, quality
enumeration or packet regeneration is added. Managed RGBA stays byte-for-byte
equal to its qualified anchor. Native normal/scalar CLI pixels are exact to
their frozen anchor; RGB agreement is max1/RMSE0.106900, within the unchanged
max4/RMSE1.5 contract. Opaque alpha255 is exact.

One CancellationTokenSource is created outside timing. Every timed public
Decode receives its live token; validation rejects a pre-canceled token with
the same exception-token identity and checks the measured token remains live
and unrequested. Token disposal is outside timing. Source and simultaneous
independent output ownership checks remain. Every original computational work
charge and cancellation/error boundary is preserved.

Complete public decode and owned-output disposal are timed. Native software
decode/state reset, centered chroma expansion, limited BT601 conversion through
RGBA16, nondithered RGBA8 reduction and real scratch/output acquisition/release
are included. Source/context/frame/packet/conversion wrappers are reused, while
every call decodes/converts all selected pixels. No alpha-plane merge or output
cache. Startup, IPC, I/O, hashing, fixture creation and validation are excluded.

Sequential Release measurements use CPU0, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Both pre/post versioned
quiet guards pass on both runs. All twelve independent actuals are selected and
retained; the upper includes selected/all-actual intervals against the faster
native bracket's lower bound. Allocations include harness overhead; native
total allocations are unmeasured.

Only one benchmark source/build changes. Both runs reuse the same verified
immutable production core/harness/native worker. Existing 3,483-tests-per-OS/
live/same-actual-package qualification is reused without a production/native
rebuild, profile or repeated suite/package proof. The harness build also includes
the previous SmallestSize receipt-prose correction, with no dedicated rebuild
or timing repeat for that wording.

[Numeric evidence](warm-vp8-cancelable-evidence.json) retains every actual,
allocation, guard, frozen input/output/source/build identity and raw archive
hash. Stop this fixed pair; remaining material/current-source coverage stays open.
