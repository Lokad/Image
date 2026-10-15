# PNG8 SmallestSize encoding

Complete RGBA8 SmallestSize encoding independently confirms **0.835× FFmpeg /
upper 0.841×**, at **527,564 managed B/op**. Production is unchanged.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Short screen | 14,545.55 | 17,311.35 | 0.840 / 0.943 | 527,564 |
| Independent confirmation | 14,469.30 | 17,319.86 | 0.835 / 0.841 | 527,564 |

One existing original 513×259 RGBA8 raster uses filter None, BCL SmallestSize
level9 and FFmpeg level9. Managed/native IDAT payloads are **209,314 / 210,283
bytes**, differing **0.461%**, within the unchanged **5% gate**. Complete packets
are 209,407 / 210,973 bytes. All RGBA samples, alpha and hidden RGB are exact.
No new image, depth/filter/level/quality grid or baseline adjustment is added.

Both subjects time complete encoding into independent owned output and release.
Managed fresh MemoryStream creation, growth, public encoding and disposal are
included, without a redundant ToArray. Native source/context/packet wrappers
are reused, processing every sample on every operation with real packet
acquisition and release. No output cache. Startup, IPC, I/O, fixture creation and
validation are excluded. Exact packets/pixels, normal/scalar CLI samples,
source and independent owners pass before timing. The native adapter adds one
scoped byte level9 operation; older operation levels remain fixed.

Both sequential Release runs pass versioned pre/post quiet guards on CPU0,
one codec thread, normal SIMD, disabled tiering and workstation nonconcurrent GC.
All twelve independent actuals are selected and retained. The upper includes
selected/all-actual intervals against the faster native bracket's lower bound.
Native process CPU/wall telemetry is retained. Managed allocations include
harness overhead; native total allocations are unmeasured.

One harness and one native-worker build serve both runs, reusing the qualified
immutable production core. Existing 3,483-tests-per-OS/live/same-actual-package
qualification is reused. No production rebuild, profile or repeated suite/package
proof is required. After measurement, inherited word-only receipt prose is
corrected to interpolate the actual depth; numeric depth8, input/output hashes,
levels and size checks were already correct. The original receipts remain
retained, with no rebuild or measurement repeat for this prose correction.

[Numeric evidence](warm-png-smallest8-evidence.json) retains every actual,
allocation, effort/size gate, guard, source/build/packet identity and raw archive
hash. Stop this fixed pair; remaining material/current-source coverage stays open.
