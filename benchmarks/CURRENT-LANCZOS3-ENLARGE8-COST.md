# Current Lanczos3 byte-enlargement cost

The existing513×259 RGBA8→1027×519 control independently confirms **4,198.931 µs,
1.033× / conservative upper1.038**, with **319,038 allocated B/op**. It remains
a small CPU gap. Twelve actuals/eleven filtered, both native brackets and passing
idle-v2 guards are retained. No causal comparison against the unmatched release
screen or allocation reduction is claimed.

Complete warm single-CPU public resizing includes plans, admission, allocation,
filtering, independent ownership and release. The reference is explicitly zimg,
FFmpeg's zscale backend, with the unchanged
[complete boundary](WARM-LANCZOS3-BASELINE.md). Managed independent decimal/
BigInteger pixels are exact. Native RGB/alpha max1 and RMSE.002623/.003063 retain
the unchanged max1/RMSE.25 gate, worker/settings and normal/scalar proof.

Seven unchanged algorithm/dispatch/ownership sources permit proof reuse. The
scoped producer header and before/after exact-frame/source/ownership checks cover
Enlarge8; the original header is restored. Cached Runnable0 selects Case2 with
12actuals/16declared-generated unroll calls. **No builds**, workload expansion
or repeated full/package proof: current5,065 checks per OS/live and the same
actual package/three examples remain applicable.

[Evidence](current-lanczos3-enlarge8-cost-evidence.json) pins samples and reuse.
Mirrored/member-verified raw archive:
current-lanczos3-enlarge8-confirmation.tar.xz,52members,SHA256
b2152b8dca784a12d5128c8bd43a0765f662dfd95ce9bff47358371ff90f689e.
