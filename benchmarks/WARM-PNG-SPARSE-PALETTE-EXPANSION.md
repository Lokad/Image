# Packed sparse PNG palette expansion

Complete Adam7 palette8/4 decoding improves **54.9% / 64.9%**, with unchanged
allocations and exact pixels. Both independently pass warm one-CPU parity and stop.

| Complete 513×259 decode | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Prior Adam7 palette8 | 1,703.925 | 1.274 / 1.283 | 4,976 | Prior |
| Current Adam7 palette8 | 768.983 | 0.570 / 0.587 | 4,976 | Pass; stop |
| Current Adam7 palette4 | 495.940 | 0.437 / 0.440 | 2,608 | Pass; stop |
| Current Adam7 gray1 | 1,282.717 | 1.129 / 1.155 | 1,440 | Gap |

Sparse palette passes pack exact RGB/tRNS bytes into one little-endian pixel
store, replacing generic mapping and four per-channel stores. Only pass pixels
are touched; hidden RGB, partial alpha, unused input bits, invalid-index prefix
and cancellation checkpoints remain. No LUT, buffer, owner, admitted capacity
or API is added. Dense SIMD paths remain. The unreachable generic palette arm
is removed; gray1 is refreshed because the shared fallback body changed.

A fresh three-actual primary pair clears the 5% gate. Independent twelve-actual
palette confirmations reuse valid original twelves; filtered/unfiltered raw
99.9% CIs and conservative normalized envelopes separately separate. Elapsed
reductions are 54.870% filtered/54.701% unfiltered for palette8 and 64.948% for
palette4. Gray1 is a current cost, without a causal comparison against older
native rates. All raw samples/native brackets and idle-v2 guards are retained.
Frozen inputs/settings, full decode/normalization/independent output/disposal,
exact RGBA/alpha/hidden RGB and source/owner gates pass. Native allocations are
unmeasured. No new images, settings, performance grid or original reruns.

All 5,153 checks/OS pass (2,070 internal, 2,135 public, 948 live), plus 140 internal
and 258 public checks normal/scalar per OS. The 21 new cases cover packed depths,
sparse pass positions, guards/tails, palette/alpha, first-invalid prefix and
cancellation. The same actual Release package and three examples pass both OSes.
One core build/OS, zero harness/native/selector builds; immutable builds reused.
Published releases remain unchanged. The full 268-path parity objective stays open.

[Evidence](warm-png-sparse-palette-expansion-evidence.json) and
[prior palette boundaries](CURRENT-PNG-PALETTE-BOUNDARY-COSTS.md).
Mirrored and verified archive png-sparse-palette-expansion-qualified.tar.xz, 160 members,
SHA256 59696b6020c6a3db93ab7b2d5946b44394c77e54a541eeac47ce7b077e91e116; reproduction dependencies pinned by hash.
