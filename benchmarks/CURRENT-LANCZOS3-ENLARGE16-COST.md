# Current Lanczos3 word-enlargement cost

The existing513×259 RGBA16→1027×519 control independently confirms **5,399.640 µs,
1.267× / conservative upper1.453**, with **4,583,983 allocated B/op**. It remains
a CPU gap. All12actuals/12filtered, both native brackets and passing idle-v2 guards
are retained. No causal comparison against the unmatched release screen or
allocation reduction is claimed.

Complete warm single-CPU public resizing includes plans, admission, allocation,
filtering, independent ownership and release. The reference is explicitly zimg,
FFmpeg's zscale backend, with the unchanged
[complete boundary](WARM-LANCZOS3-BASELINE.md). Independent decimal/BigInteger
managed pixels are exact. Native RGB/alpha max1, RMSE.049884/.041591 retain the
unchanged max1/RMSE.25 gate, worker/settings and native normal/scalar proof.

Seven unchanged algorithm/dispatch/ownership sources permit proof reuse. The
scoped producer header and before/after exact-frame/source/ownership checks cover
Enlarge16; the original header is restored. Cached Runnable1 selects Case3 with
12actuals/16declared-generated unroll calls. **No builds**, workload expansion
or repeated full/package proof. Current5,065 checks per OS/live and same actual
package/three examples remain applicable. All four existing Lanczos controls
now have independent current confirmations; none establishes full CPU parity.

[Evidence](current-lanczos3-enlarge16-cost-evidence.json) pins samples and reuse.
Mirrored/member-verified raw archive:
current-lanczos3-enlarge16-confirmation.tar.xz,52members,SHA256
72e5b1bce2420adb7861a5f23d19ad6fec53b0c5ddb3afa73fd883cd4e130108.
