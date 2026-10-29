# VP8-plus-alpha validation cost before direct token decisions

Superseded by [direct token decisions](WARM-VP8-DIRECT-TOKEN-DECISIONS.md).
Retained historical cost and raw evidence; this does not qualify the new reader.

One existing 513×259 frozen VP8/ALPH packet, borrowed encoded span. Both subjects
validate complete coding and return independently owned nine-field metadata.
FFmpeg uses natural-frame software decoding plus the original metadata adapter;
no RGBA normalization or caller raster copy is added. One warm pinned CPU.

| Independent confirmation | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| ValidateVp8Alpha | 8,716.160 | 5,721.858 | 1.523 / 1.697 | 770,092 |

All 12 actual/filtered iterations, both native brackets and idle-v2 guards pass.
The original packet, descriptor, native build/settings and boundary match.
Source integrity, complete coding scope and independent surviving output pass.
Allocation covers managed temporaries/results, not native or peak live storage.
This resolves an older screen; it establishes no causal gain or regression.

Zero production/harness/native/selector builds, new inputs or repeated qualification.
All 222 production files match the qualified build: existing Windows/Linux full,
scalar and SAME actual Release-package/three-example proof are reused. The scoped
producer header is restored byte-for-byte. This control remains a CPU gap;
the full 268-path objective and decoder correctness requirements are unchanged.

[Evidence](current-vp8-alpha-validation-evidence.json). Mirrored/member-verified
raw archive: current-vp8-alpha-validation.tar.xz, 69 members, SHA256
8bc0b8e9db26db2d3013e740089729e89eca3362178d8e76097382e25dd7a2a7.
