# Current Lanczos3 byte-reduction cost

The existing 513×259 RGBA8 →256×129 control independently confirms **735.824 µs,
1.500× / conservative upper1.511**, with **138,451 allocated B/op**. It remains
a CPU gap. Twelve actuals, ten filtered observations, both native brackets and
passing idle-v2 guards are retained. This is a current-cost confirmation; no
gain/regression or allocation reduction is inferred from the older release screen.

Complete warm single-CPU public resizing includes coefficient generation,
admission, allocation, filtering, independent output ownership and release.
The reference is explicitly zimg, FFmpeg's zscale backend, with unchanged
[equivalent complete boundaries](WARM-LANCZOS3-BASELINE.md). Current managed
pixels equal the frozen decimal/BigInteger frame exactly; this control also has
zero native pixel error. The existing native max1/RMSE.25 gate is unchanged.

Seven algorithm/dispatch/ownership source files match the qualified release.
Their independent exact and native normal/scalar proof is reused, with a scoped
producer header and current before/after frame/source/ownership checks. The
original shared quality header is restored afterward. One small confirmation
launcher is built against existing DLLs:12actuals,16declared/generated calls per
unroll. No production, harness or native build; no image/settings grid.
The current5,065-tests-per-OS/live and same actual Release-package/three-example
qualification carry forward without repetition.

[Evidence](current-lanczos3-reduce8-cost-evidence.json) pins source, binaries,
proof reuse and every sample. Mirrored/member-verified raw archive:
current-lanczos3-reduce8-confirmation.tar.xz,54members,SHA256
f68236f098dc24675d36b1353d62f22c9ed5962bff59bce33ff6055431ef6baf.
