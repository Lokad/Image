# Current RGBA16 lossless-WebP screen

The existing 513×259 asymmetric RGBA16 control measures 188.99 ms, 1.548× the
reference (conservative upper 1.685×), allocating 47,674,928 B/op. This is one
guarded short, with all three actual and filtered samples retained. It is a
current gap screen, not independent parity evidence or a measured speedup.

Both complete packets are exactly 183,812 bytes. Every normalized RGBA, alpha and
hidden-RGB sample agrees with the managed and normal/scalar native decoders.
The fixed input, effort4 and libwebp exact=1/quality75 reference remain unchanged.
Both operations include all sample normalization `(sample+128)/257`, native
RGBA-to-BGRA repacking, fresh independent packet acquisition and release. Managed
stream growth is timed; redundant ToArray, startup, IPC and validation are not.
Both idle-v2 guards pass; the operation uses one CPU.

No production change or rebuild was required. The original generated driver and
runtime configuration were recovered from their verified raw master; the current
qualified core, harness and native reference were reused. Existing Windows/Linux
regression, scalar WebP and same-actual-package consumer proof therefore carry
forward from [current qualification](WARM-VP8-TRELLIS-SCAN-DATA.md). No full-suite,
package or independent timing repeat accompanies this failing short.

[Evidence](warm-vp8l-rgba16-current-evidence.json) retains source/core/worker/driver
identities, exact packet/pixel receipts, samples and a mirrored, member-verified
raw archive. Only this named normalization boundary is measured; arbitrary
16-bit sources, asynchronous delivery and other settings receive no new claim.
