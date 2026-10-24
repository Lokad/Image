# Exact VP8 residual lookup preparation

The existing 513×259 RGBA8 VP8 control keeps Q75/method4 and exact compressed
alpha. Complete operations include normalization, encoding and fresh independent
owned output acquisition/release. Raw-exact packets remain 98,590 B, with
unchanged inputs, reference settings and correctness gates. One CPU per operation.

| Quiet VM run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| Short | 133,494.94 | 53,507.82 | 2.495/2.703 | 24,080,952 |
| Independent | 134,090.08 | 53,649.34 | 2.499/2.518 | 24,080,952 |

Independent confirmation uses 7.6% less elapsed than the
[preceding confirmation](WARM-VP8-GREEN-PREDICTION-COST.md), with unchanged
allocations. All 12 actual iterations are retained, 11 selected; the conservative
upper includes both. Both idle-v2 guards pass.

Safe AVX2 prepares magnitudes, contexts and complete lookup addresses in sixteen
lanes. The same generated tables and scalar final sum preserve exact costs and
decisions. Portable scalar execution remains supported. A 64 B stack temporary
fits the existing admitted workspace; no heap owner, scope or copy is added.
Work, cancellation, shared admission, ownership and terminal refusal are unchanged.

Both platforms pass 5,108 checks each, including 947 live comparisons, 16 scalar
rate/macroblock checks and 29 scalar public VP8 checks. Frozen arithmetic, native
costs/macroblocks, packets and zero-allocation stage checks pass. The SAME actual
private 0.2.0 Release package and all three consumer examples pass both platforms.

[Evidence](warm-vp8-vector-residual-evidence.json) records samples, identities and
qualification. VP8L, JPEG, PNG, decoders, editing and text reuse unchanged evidence.
No images/settings were added. Grayscale JPEG now has the largest measured gaps;
full 257-case warm parity remains open.
