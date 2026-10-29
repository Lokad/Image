# Current Lanczos3 word-reduction cost

The existing513×259 RGBA16→256×129 control independently confirms **802.461 µs,
1.585× / conservative upper1.666**, with **138,439 allocated B/op**. It remains
a CPU gap. All12 actuals are retained/filtered, with both passing idle-v2 guards.
Native brackets522.433/506.275µs remain; the upper uses the faster bracket and
both actual confidence views. No causal comparison against the old release screen.

Complete warm single-CPU public resizing includes plans, admission, allocation,
filtering, independent ownership and release. The reference is zimg, explicitly
FFmpeg's zscale backend, with the unchanged
[complete boundary](WARM-LANCZOS3-BASELINE.md). Managed decimal/BigInteger pixels
are exact. Native RGB/alpha maxima1/1, RMSE.050134/.040062 satisfy the unchanged
max1/RMSE.25 gate; inputs/settings/worker and native normal/scalar proof are fixed.

Seven unchanged algorithm/dispatch/ownership sources permit proof reuse. A scoped
producer header and current before/after frame/source/ownership checks cover
Reduce16; the original header is restored. One small confirmation launcher is
built against existing DLLs,12actuals/16declared-generated calls per unroll.
No production/harness/native build or new workload. Current5,065 checks per OS,
live and same actual package/three-example qualification carry forward.

[Evidence](current-lanczos3-reduce16-cost-evidence.json) pins every sample,
identity and proof reuse. Mirrored/member-verified raw archive:
current-lanczos3-reduce16-confirmation.tar.xz,54members,SHA256
8e88f6fef1e922fe68adef597f23dd6ae0dea8f2035370dbfc2c447b59e1530e.
