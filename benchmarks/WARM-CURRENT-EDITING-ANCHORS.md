# Current editing cost anchors

Existing controls are measured on the qualified immutable production build.
No shape/depth/filter grid or production/harness/native rebuild is introduced.

| Current quiet short | Lokad µs | Reference µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Bilinear Down16 | 547.29 | 408.78 | 1.339 / 1.426 | 290,124 |

Down16 resizes the original 513×259 RGBA16 raster to independently owned 256×129.
Complete public Resize, admission, geometry and output acquisition/release are
timed on one CPU. The pinned FFmpeg graph includes packed/planar conversion,
widened triangle filtering, encoded-space alpha premultiplication/unpremultiplication,
final RGBA64 and output ownership. Both idle-v2 guards pass.

Managed words stay exact against the rational reference; native RGB/alpha max1
and both RMSE≤0.25 retain their approved bounds. Frozen input/output identities,
normal/scalar native CLI, zero-alpha RGB, source immutability and independent
output ownership pass. The historical 1.277× result belongs to its pinned producer;
this current screen makes no cross-producer gain or independent parity claim.

The original selection driver is restored byte-for-byte from the cited raw archive;
production, benchmark, reference workers and runtime policy are reused. Existing
cross-platform regression/scalar/actual-package proofs remain valid and are reused.
[Evidence](warm-current-editing-anchors-evidence.json) retains raw samples, guards,
exact driver identity and mirrored archive. Full 253-path warm parity remains open.
