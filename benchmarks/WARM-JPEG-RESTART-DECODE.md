# JPEG420 decoding with row restart markers

The existing row-restart packet independently confirms **1.823x FFmpeg /
conservative upper 1.837x**, at **1,623,906 managed allocated B/op**. Production
is unchanged; uniform current-source coverage remains open.

| Measurement | Lokad.Image us | FFmpeg us | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 1,857.61 | 1,014.46 | 1.831 / 2.275 | 1,623,906 |
| Independent twelve-actual confirmation | 1,852.40 | 1,016.13 | 1.823 / 1.837 | 1,623,906 |

Both decoders receive the same **58,012-byte** native JPEG420 packet from the
[restart encode round](WARM-JPEG-ADMITTED.md), SHA-256
`bd0060bf5bbf48a9f55120c5e71b22ac2763751bb370e9c5c2d93ebba38ed622`.
It encodes the existing original 513x259 RGBA8 raster with DRI33 and sixteen
sequenced RST markers. No image, packet generation or quality search is added.
Strict framing and corrupted-DRI/RST controls pass before timing. Each decoder's
pixels equal its own nonrestart anchor exactly; the frozen managed pixel hash is
`af11bb4d45ff54b87749d5d0e5887c6f706c8ea4dba263b3e6459e3b7cbd7aef`.
Native conversion matches the public CLI byte for byte. Scalar/normal decoder
RGB agreement is maximum3/RMSE0.579363, within unchanged maximum4/RMSE1.5.
Opaque alpha, source preservation and two-live-output independence pass before
and after measurement.

The complete public managed decode, output allocation and disposal are timed.
The unchanged native worker resets software MJPEG state for every operation,
fully decodes, expands/crops ceil-sized chroma, converts to full-range BT.601
RGBA8, and acquires/releases independent output. Decoder/input packet/frame
wrappers may be reused, with no decoded output cache. Sequential Release runs
use one pinned CPU, one codec/conversion thread, normal SIMD, disabled tiering
and workstation nonconcurrent GC. Startup, I/O, IPC, fixture preparation,
hashing and validation are excluded. Native allocations are unmeasured; managed
figures include harness overhead.

All versioned pre/post quiet guards pass. All twelve confirmation actuals are
retained; BenchmarkDotNet selects eleven. The conservative upper uses the
larger filtered/all-actual interval against the faster native bracket's lower
bound. The screen supervisor initially expected the display label `4:2:0`
where the receipt correctly stores SOF factors `2x2,1x1,1x1`. Its original failed
receipt is retained alongside an explicit correction and revalidation of all
byte, pixel, job and guard checks; timing was not repeated. Confirmation checks
the correct representation directly.

Only two benchmark inputs change. Screen and confirmation reuse the same
verified production DLL/harness and native worker. Existing **3,298 Release
tests per OS**, including **896 live FFmpeg comparisons**, and consumer plus
three examples through the same actual Release package are reused from the
[production qualification](warm-jpeg-admitted-evidence.json). No production or
native rebuild, full suite, package proof or whole-library method audit repeats.

The [numeric evidence](warm-jpeg-restartdecode-evidence.json) retains all samples,
allocations, guards, actual jobs/assemblies and qualification links. Stop this
confirmed pair; remaining material public/codec/current-source coverage stays open.
