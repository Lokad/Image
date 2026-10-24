# Exact grayscale JPEG entropy replay

Existing 513×259 Q69 grayscale controls retain owned quantizers and raw-exact
58,910 B packets. Complete operations include normalization where required,
encoding and fresh independent owned output/release. Caller-gray borrows resident
samples directly. One CPU; the approved reference is explicitly labeled standalone
SIMD-enabled libjpeg-turbo3.1.4.1, for benchmark tooling only.

| Quiet VM run | Lokad µs | libjpeg-turbo µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| RGBA-gray short | 2,442.09 | 1,411.34 | 1.730/1.909 | 154,884 |
| RGBA-gray independent | 2,432.29 | 1,409.20 | 1.726/1.739 | 154,883 |
| Caller-gray current short | 2,038.89 | 1,080.83 | 1.886/2.154 | 154,886 |

Independent RGBA-gray confirmation uses 35.3% less elapsed than the
[preceding confirmation](WARM-JPEG-GRAY-AVX2-TRANSFORM.md). All 12 actual
iterations are retained, 11 selected; both contribute to the conservative upper.
Both idle-v2 guards pass. Caller-gray is a current short reconciliation.

The first pass retains exact entropy events. Replay avoids repeating complete
normalization, DCT, quantization and coefficient traversal. Huffman construction,
writer calls, work and pixels remain exact. Restart grayscale and tight local
allowances keep necessary recomputation. A 48 B owner accounts for the small warm
allocation increase; raw GC rounding differs by one byte. Initial record capacity
is 274,560 B on these controls, at most doubling once. Shared admission charges
full live capacity and old/new overlap before allocation; refusal is terminal.

Both platforms pass 5,111 checks each, including 947 live comparisons, 11 scalar
gray/record checks and 148 scalar public JPEG checks. Exact packets, logical work,
source ownership, aggregate admission, replacement peaks, refusal/cancellation
cleanup and recovery pass. The SAME actual private 0.2.0 package and all three
examples pass both platforms.

[Evidence](warm-jpeg-gray-entropy-records-evidence.json) records identities,
samples, memory bounds and qualification. Other affected gray timings are
historical; color JPEG, other codecs, decoders, editing and text reuse unchanged
evidence. No new controls. Full 257-case warm parity remains open; larger encoder
gaps take priority next.
