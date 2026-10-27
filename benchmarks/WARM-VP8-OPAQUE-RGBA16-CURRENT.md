# Current opaque RGBA16 VP8 screen

The existing 513×259 asymmetric RGBA16 control with opaque alpha measures
28.83 ms: 1.850× the reference (upper 1.941×), allocating 1,201,536 B/op. Both
complete packets are exactly 72,380 bytes at frozen quality75/effort4. This is
one guarded short, with all three actual/filtered samples; independent parity
and a measured speedup are not claimed.

Both operations include full `(sample+128)/257` normalization, repacking, complete
encoding, fresh packet acquisition/growth and release. No alpha plane is encoded.
Complete packets, matched-packet reconstruction sanity, exact normalized alpha,
source preservation and independent ownership pass before/after timing. Both
idle-v2 guards pass. Startup, transport, ToArray and validation are untimed.

Production, the shared harness and native reference remain unchanged. Only the
original generated driver/config were recovered from their verified retained
master; no rebuild, independent timing or full/package qualification repeat was
needed. [Current qualification](WARM-VP8-TRELLIS-SCAN-DATA.md) carries forward.
[Evidence](warm-vp8-opaque-rgba16-current-evidence.json) retains identities,
samples and a mirrored, member-verified raw archive. No broader 16-bit/content/
settings claim follows. The photograph remains the largest current confirmed gap.
