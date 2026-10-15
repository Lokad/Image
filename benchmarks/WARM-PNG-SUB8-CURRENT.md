# PNG8 Sub confirmation on the qualified build

The existing RGBA8 Sub decode independently confirms **1.686× FFmpeg / upper
1.697×**, with **4,456 managed B/op** and exact pixels. This closes the old short
confidence gap without changing production or benchmark inputs.

| Same immutable build | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 1,186.55 | 698.69 | 1.698 / 2.152 | 4,456 |
| Independent twelve-actual confirmation | 1,179.60 | 699.60 | 1.686 / 1.697 | 4,456 |

The original 513×259 RGBA8 raster and frozen Sub/Optimal level6 packet are reused.
The existing selector and PNG decoder worker suffice; no additional original
image, profile, filter, compression or geometry grid is introduced. The older
[Sub regression screen](WARM-PNG-SUB16.md), mean 1.973× / upper 2.211×, remains
retained at its original source. These are separate measurements.

Both subjects time complete identical-packet decoding into independent owned
RGBA8 and release. Managed admission, framing, inflation and Sub reconstruction
are included. Native codec/input packet/frame wrappers are reused; complete
decoding, owned output acquisition and release remain timed, with one codec
thread. No output is cached. Startup, I/O, IPC, fixture creation and validation
are excluded. Native allocations are **unmeasured**; managed figures include
harness overhead and released-storage reuse.

Analytical samples, normal/scalar native CLI output, alpha/hidden RGB, geometry,
source preservation and two simultaneously live output owners pass outside
timing. Sequential Release runs use one pinned CPU, normal SIMD, disabled tiering
and workstation nonconcurrent GC. Every quiet guard passes. Screen and independent
repeat share the same immutable core/harness; all twelve confirmation actuals
are selected and retained. The upper includes selected and all-actual intervals
against the faster native bracket's lower bound.

Production, harness, native worker and all owned inputs match the already accepted
snapshot. Existing **3,428 Release tests per OS**, including **898 live comparisons**,
and the same actual package's consumer plus all three examples on Windows/Linux
are reused from the [packed transparency qualification](WARM-PNG-PACKED-TRANSPARENCY.md).
Only cached generated jobs are prepared; no production/harness/native rebuild,
full suite, package proof, method audit or profile is repeated.

A private preparation flag typo caused one extra short execution. Its complete
log and native receipt are retained **unscored**, excluded from the separately
guarded screen and confirmation. The helper was corrected without repeating
either scored run or rebuilding inputs.

[Numeric evidence](warm-png-sub8-current-evidence.json) retains every scored actual,
allocation, guard, source/assembly/job identity, the unscored preparation execution
and qualification reuse. Stop this confirmed pair; full required warm coverage stays open.
