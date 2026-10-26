# Exact VP8 chroma and color batching

The existing 513×259 VP8/ALPH control decodes its frozen 98,590-byte packet from
a fresh seekable Stream to independently owned RGBA8. Complete input buffering,
copies, EOF, decode, chroma/color, exact alpha and output release run on one CPU.

| Quiet run | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| short | 7,793.88 | 5,992.70 | 1.301 / 1.353 | 1,966,583 |
| confirm | 7,784.47 | 5,983.28 | 1.301 / 1.308 | 1,966,583 |

The independent run uses 21.5% less elapsed than the preceding quiet
[short screen](WARM-CURRENT-VP8-STREAM-DECODER.md), without a paired independent
baseline gain claim. Both idle-v2 guards pass. All12actual samples, including the
filtered outlier, remain retained; conservative bounds cover filtered/unfiltered
managed samples and both native brackets. Allocations stay1,966,583B/op. Managed,
native and scalar CLI RGBA pixels remain exact, SHA256
`55cdb9d8ce5e4dbaf16967129b299f6f900aa1c2fa8dad554d26d91b02295dcd`.

One bounded profile placed24.8% of758workload samples in scalar rendering/color.
Eight-pixel AVX2 groups now share five centered chroma columns for four odd/even
pairs. Every integer rounding/clipping step remains: /16chroma, saturatedRGB16,
Q15studio roundtrip,15bit truncation and finalQ13byte conversion. A range proof
over the complete RGB16cube bounds every product/partial sum/bias to signed32;
the largest forward sum is2,016,937,928. No precision/tolerance change is introduced.

The actual vector helper matches the retained scalar helper over all16,777,216
byte Y/U/V combinations with mixed alpha lanes.256render controls cover odd sizes,
edges/tails, differing lanes, paddedrows, alpha/opaque and source immutability.
Native whole-decoder controls and the independent chroma oracle remain exact.
Scalar firstpixel/tails/narrowrows and unsupported-hardware fallbacks remain.
Only registers/local span views are added: same output allocation, refusal,
per-row cancellation, input release, failure cleanup and borrowed memory root.

Both OS pass4,922Release checks each(947live comparisons), plus601affected scalar
WebP checks each. The SAME actual private0.2.0package and allthree usage examples
pass both platforms. One immutable core serves the round; benchmark/native/
generated workers are reused. Published0.1.0 stays unchanged.

[Evidence](warm-vp8-vector-color-evidence.json) retains samples, range proof,
profile/raw identities, qualification and actual package. Other affected VP8decode
cost evidence becomes historical; unrelated codec/editing/text paths are unchanged.
Stop color tuning; full253-path warm parity and replacement readiness remain open.
