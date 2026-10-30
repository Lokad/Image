# Packed sparse grayscale PNG expansion

Complete Adam7 gray1 decoding uses **80.0% less elapsed time**, with unchanged
**1,440 B/op** and exact pixels. This existing control independently passes
warm one-CPU parity at a conservative upper of **0.230×** and stops.

| Complete 513×259 decode | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Prior Adam7 gray1 | 1,282.717 | 1.129 / 1.155 | 1,440 | Prior |
| Packed sparse gray1 | 256.299 | 0.223 / 0.230 | 1,440 | Pass; stop |

Sparse 1/2/4-bit grayscale rows compute their exact scale and complete tRNS key
once, then store one packed RGBA pixel per selected column. Hidden RGB, sparse
holes, input padding and 4,096-pixel cancellation checkpoints remain. No buffer,
LUT, owner, capacity or admission mechanism is added. Dense grayscale, RGBA and
palette handlers remain earlier branches; the generic pixel writer is unchanged.

A fresh three-actual pair clears the 5% gate. One independent twelve-actual
confirmation reuses the valid prior twelve: 80.019% filtered/79.990% unfiltered
elapsed reduction and 80.265% normalized point improvement. Both raw 99.9% CIs
and conservative normalized envelopes separately separate. All actuals/native
brackets and idle-v2 guards are retained. Frozen inputs/settings, complete public
decode/normalization/independent output/disposal, exact RGBA/alpha/hidden RGB,
native normal/scalar CLI, source integrity and ownership pass. Native allocations
are unmeasured. The speed claim covers this gray1 control; other packed depths
receive correctness coverage without a new benchmark grid.

All 5,157 checks/OS pass (2,074 internal, 2,135 public, 948 live), plus 131 internal
and 258 public focused checks normal/scalar per OS. Four new controls cover
packed depths, full-word keys, hidden RGB, tails, padding, holes, unaligned guards,
borrowed-input integrity and cancellation. The same actual Release package and
three examples pass both OSes. One core build/OS, zero harness/native/selector
builds; immutable builds reused throughout. Published packages remain unchanged.
The full 268-path parity objective remains open.

[Evidence](warm-png-sparse-gray-expansion-evidence.json) and
[prior shared-fallback cost](WARM-PNG-SPARSE-PALETTE-EXPANSION.md).
Mirrored/member-verified archive png-sparse-gray-expansion-qualified.tar.xz, 134 members,
SHA256 d9624d06a8a678f43cf1dcc0dee10f605a28b720dce27d7a92e5af1d9de72158; reproduction dependency is pinned by hash.
