# Prepared VP8 model-row stride

The decoder prepares one 96-row coefficient model with a 16-byte storage stride.
Each row still contains exactly eleven encoded probabilities; five unused bytes
replace repeated 11-byte-stride address calculations. Defaults and header updates
retain their original syntax order. Ordinary, live, window and tail readers borrow
the same owner. Boolean arithmetic, refill, coefficient traversal, quantization,
pixel reconstruction and every cancellation checkpoint are unchanged.

| Complete operation | Managed / FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: |
| Resident RGBA8 + alpha | 5,844.19 / 5,965.61 | 0.980 / 0.983 | 238,864 |
| Fresh seekable stream RGBA8 + alpha | 5,875.90 / 5,992.57 | 0.981 / 0.984 | 238,960 |
| Opaque RGBA8 | 4,849.44 / 4,786.26 | 1.013 / 1.017 | 229,344 |
| Opaque RGBA8, simple filter | 4,944.72 / 4,891.85 | 1.011 / 1.017 | 229,344 |
| Opaque RGBA8, live token | 4,920.12 / 4,789.06 | 1.027 / 1.032 | 229,344 |
| Complete alpha VP8 pixel-coding validation | 4,392.35 / 5,447.24 | 0.806 / 0.814 | 14,064 |

All six controls improve **49.93–137.23µs** against their source-valid original
confirmations, with separated filtered/all-actual raw and native-normalized
intervals. Live decode saves **59.31µs/1.19% raw/1.18% normalized**; its native
clock moves −0.012%. Resident alpha saves 75.78µs and now passes parity. Three
cases pass and stop; the three opaque cases remain demonstrated gaps. This is
scoped evidence on frozen controls, not a general decoder or whole-library claim.

The one model payload grows **1,056→1,536B**. Every complete operation allocates
exactly **480B more**. It fits the existing 8KiB model/state/stack allowance,
reserved before allocation; no second model, pool, cache, independent allowance
or escaping state is added. Physical/native peaks were not remeasured. Decoded
frames retain 531,468B independently until disposal; validation instead owns its
36B descriptor. Admission refusal, cleanup and surviving outputs remain exact.

One three-actual live screen precedes an independent twelve-actual confirmation;
the five other affected controls each refresh once. Native context retention,
complete header preparation, input delivery, normalization, filtering, alpha/color,
output ownership and disposal retain their original boundaries. No preparation
is silently excluded. One CPU, sequential quiet-VM jobs and idle-v2 guards remain.
No new fixture/settings grid, harness/reference build or favorable repeat is used.

Windows/Linux each pass **5,909 full/live checks**, plus 14 internal and 239 public
affected checks per normal/scalar mode. Dense independent coefficient oracles pack
the new rows with poisoned padding; all twelve coefficient classes, windows,
following reads and 305 cancellation ordinals remain exact. The SAME actual Release
package and all three examples pass both OSes. Main/embedded modules build once
per OS throughout the round. Encoder packets and unrelated editing paths retain
their contracts and source-valid evidence.

[Evidence](vp8-model-row-stride-evidence.json) records current costs, confidence,
source scope, storage and qualification. Raw `vp8-model-row-stride-qualified.tar.xz`,
SHA256 `6ba2ce0bd7569e29a86521f20c5721df558492e1973b5d8b2a1d1803b1b4d607`,
6,012,200B/147 members, is mirrored and member-verified. Retained vps588 scripts
and dependencies reproduce the stage. [Earlier model preparation](VP8-PREPARED-COEFFICIENT-MODEL.md)
and [alpha publication](WEBP-HORIZONTAL-ALPHA-PUBLICATION.md) retain history.
The full 283-path objective remains open; publication is postponed.
