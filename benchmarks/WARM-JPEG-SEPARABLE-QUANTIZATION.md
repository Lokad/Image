# JPEG separable MCU conversion and vector quantization

One existing513×259 RGBA8 JPEG420 control, unchanged quality85/native qscale5,
adaptive Huffman, background0x204060 and reference inputs/settings. Complete
packets remain52,091/52,093 B, allowing only the approved attribution COM.

| Quiet one-CPU run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| RGBA8 short | 8,799.88 | 1,767.22 | 4.980/5.531 | 372,808 |
| RGBA8 confirm | 8,766.00 | 1,763.93 | 4.970/4.990 | 372,808 |
| RGBA16 current short | 11,434.83 | 1,840.77 | 6.212/6.758 | 372,808 |

Independent confirmation reduces elapsed time by 23.0% against the
[previous11,388.76-µs result](WARM-JPEG-ROW-NORMALIZATION.md). Full CPU parity
remains open. The same immutable benchmark/native/archived workers serve every
screen/confirmation; both versioned idle guards pass. No new timing grid.

The bounded MCU tile is reused through RGB, Q6 matrix conversion and signed
horizontal Q7 filtering. Vertical samples reuse these rows. Exact SSE2/AVX2
quantization processes8/16 coefficients together; its stack plan halves to256 B.
The8-KiB owner/work limits and heap allocation are unchanged.

Both platforms pass5,047 checks each, including947 live comparisons, affected
scalar checks and SSE2 quantization. Every signed coefficient/quantization step
and every byte/alpha/background combination are tested. Two tiny original
yellow/cyan controls guard negative Q7 intermediates against full native packets;
they add correctness coverage, with no new timing workload. The same actual
private0.2.0 package and all three examples pass on both platforms.

[Evidence](warm-jpeg-separable-quantization-evidence.json) retains samples,
identities, signed-corner provenance, ownership/qualification scope, unchanged
evidence and the mirrored retirement of two completed consumer extractions.

The existing RGBA16 control now has one same-build current-cost screen. Its
52,115/52,117-B packets and frozen settings remain exact, with full16-bit alpha
normalization included. This is a measured gap, without an independent repeat
or speedup claim. It takes priority over the smaller VP8L encoder anchor gap.
The archived host, prepared inputs and E129 qualification are reused. A stale
documentation-source guard rejection ran no timing; production inputs/binaries
were verified unchanged before refreshing source metadata. No rebuild was needed.
