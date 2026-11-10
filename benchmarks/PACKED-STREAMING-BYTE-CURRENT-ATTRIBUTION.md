# Packed streaming byte attribution

PSA751 confirms the existing complete513×259→4×9 RGBA8 operation on the qualified
PIF750 package: **797.37/zimg717.48µs**, ratio1.111/upper1.150, GC114B/op,
12actuals/11filtered with all actuals retained. Exact managed pixels, original
native bounds, source/output ownership and quiet single-CPU/idle-v2 gates pass.
The roughly79.88µs gap remains demonstrated. Earlier815.17/native684.23µs is
superseded without a causal speed or allocation claim; code did not change.

The bounded256×129 byte control has the roughly85µs gap;8×17 is about49µs.
The earlier plan shorthand confused those cases. Existing4×9 had the larger
recorded gap and now belongs among the roughly80µs leads. No new workload,
image, quality setting, production build or qualification repeat is added.

All four full/packed streaming source files remain identical to
[shared streaming](BICUBIC-SHARED-STREAMING.md). BicubicMath/ImageTransforms changed
elsewhere; the relevant Coefficient, PrepareStreamingRows, RoundByte and Kernel
bodies remain identical. A current profile verifies the actual packed path.
Managed geometry/4KiB workspace/materialization/disposal remain timed; labeled
zimg retains graph/input/scratch and includes its established halo, packing,
filtering, independent output/release boundary. No fresh-native-graph claim.

One two-second CPU0 capture/CPU1 sampler collects1998samples:1899 in the655B
packed byte row filter,66 in its driver,26 in Kernel. Instrumented elapsed is
discarded. Hot instructions include source clamp/bounds checks, integer
premultiplication, coefficient bit reconstruction and the ordered dot loop.
Sample locations/skid do not quantify exact phase costs or removable percentages.

The next architectural lead pairs two actual adjacent required source rows,
sharing clamped indices and six-byte coefficient reconstruction while keeping
each row's original lower-then-upper product/add order. Two independent sum
chains can interleave without reassociation, FMA or changed coefficients/repair.
One next-row numeric vector would remain private until the immediately following
coordinate. Repeated clamped-edge contributions, block reset, cancellation,
existing4KiB admission and all row/input/output ownership must remain.

A64-check scheduler model preserves computed-source and vertical accumulation
traces, including clipped edges and block boundaries. On the frozen control,
259rows become129pairs/1single per column: calls1036→520 and logical coefficient
decodes531468→266760. This is no actual SIMD, memory-peak or CPU-gain proof.
The intended end-state keeps one shared packed scheduler source with row/depth
modes chosen once, batching state erased from word execution, and useful single
kernels retained. No source/product array, session/cache, pool, owner or quota.
Inspect actual row-pair arithmetic/lifetimes before building; then focused bits/
guards/root/cancellation, existing short, useful confirmation/affected packed
costs and broad Windows/Linux/scalar/ISA/SAME-package proof at acceptance.

[Evidence](packed-streaming-byte-current-attribution-evidence.json) pins the
current actual package/DLL, confirmation, source audit, distinct samples,
instruction annotations and model. Unchanged6154full/live perOS,328/729 scalar/
ISA and SAME-package/allthree-example qualification are reused. Mirrored and
member-verified raw `tmp/packed-streaming-byte-current-attribution.tar.xz`, SHA256
`b69212c86862b33fabc638e9daf6e74e30aedef9d9e3217e37b74f6342addc79`,
90,944bytes/34members, retains reproducibility. Publication remains postponed.
