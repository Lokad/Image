# Prepared small-palette PNG lookup

Complete decoding of the existing 513×259, 16-entry palette8 control uses
**40.5% less elapsed time**, with unchanged **1,512 B/op** and exact pixels.
Its independent warm one-CPU conservative upper is **0.706×**: pass and stop.

| Complete decode | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Prior per-row preparation | 128.064 | 1.173 / 1.180 | 1,512 | Superseded |
| Preparation once per dense pass | 76.147 | 0.698 / 0.706 | 1,512 | Pass; stop |

The same four SIMD lookup vectors become one fixed-size 64-byte value prepared
once per dense pass. No heap table, cache, buffer, owner or admission is added.
Scalar fallback, first-invalid-index written prefix, tails, hidden RGB, omitted
alpha and cancellation checkpoints remain. Other palette/filter/inflate kernels
are unchanged. This claim covers the named control, without a new workload grid.

A fresh three-actual original/candidate pair clears the predeclared 5% gate.
One independent twelve-actual confirmation reuses the valid original twelve:
40.540% filtered/40.539% unfiltered elapsed reduction and 40.493% normalized
point improvement. Both raw 99.9% CIs and conservative normalized envelopes
separately separate. All actuals, native brackets and idle-v2 guards are retained.
Frozen packets, rasters and settings, complete public decode/normalization/owned
output/disposal, exact RGBA/alpha/hidden RGB, native normal/scalar CLI, borrowed
source and independent output lifetimes pass. Native allocations are unmeasured.

All 5,158 checks/OS pass (2,075 internal, 2,135 public, 948 live), plus 72 internal
and 258 public focused checks normal/scalar per OS. Existing guard/error/cancel
tests exercise the prepared production entry; one additional multirow test
checks reuse and surviving independent outputs. The same actual Release package,
governed async consumer and three usage examples pass Windows/Linux. One core
build/OS and zero harness/native/selector builds; immutable builds reused.
Published packages stay unchanged; the full 268-path parity objective stays open.

[Evidence](warm-png-small-palette-lookup-evidence.json) and
[prior control](CURRENT-PNG-PALETTE-BOUNDARY-COSTS.md).
Mirrored/member-verified png-small-palette-lookup-qualified.tar.xz, 141 members,
SHA256 d762cb184bda4ea642161b68cb1535d55c81988b15327cb02c0a7c0fd5137502; reproduction dependencies are pinned by hash.
