# Exact green-plane predictor scores

The existing 513×259 RGBA8 VP8 control retains Q75, method4, exact alpha and
raw-exact 98,590-byte packets. Complete operations include encoding and fresh
independent owned output acquisition/release. The lossless control retains exact
183,812-byte packets and uses explicitly labeled libwebp exact=1, FFmpeg's encoder
backend. Inputs, reference settings and all correctness gates are unchanged.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| VP8 short | 144,730.09 | 53,875.16 | 2.686/2.891 | 24,080,952 |
| VP8 independent | 145,179.99 | 53,491.71 | 2.714/2.729 | 24,080,952 |
| VP8L current short | 297,269.17 | 121,972.43 | 2.437/2.755 | 48,060,256 |

VP8 independent confirmation uses 6.0% less elapsed than the
[preceding confirmation](WARM-VP8-ALPHA-HISTOGRAM.md), with unchanged allocations.
All 12 actual iterations are retained, 11 selected; both sets contribute to the
conservative upper. Both idle-v2 guards pass. VP8L is a current short reconciliation
without an independently confirmed speedup claim.

Known ALPH predictor histograms have singleton red/blue and two alpha bins.
Their entropy terms reduce exactly, retaining all original integer logarithms,
rounding and weighted residual biases. Green scores, mode decisions, recursive
history, residuals and packet emission remain unchanged. The same 1,024-unit work
charge/cancellation check remains; no storage, cache or reservation is added.
Generic scores remain required for other representations.

Both platforms pass 5,101 checks each, including 947 live comparisons, 53 scalar
predictor/alpha checks and 29 scalar public VP8 checks. 16,384 bounded controls
retain frozen full-plane scores and work. Existing maps/residuals/ALPH packets,
shared admission, refusal/cancellation cleanup and recovery remain exact. The
same actual private 0.2.0 package and all three examples pass both platforms.

[Evidence](warm-vp8-green-prediction-cost-evidence.json) records samples, identities,
qualification and reuse. JPEG/PNG/decoders/editing/text are unchanged. The existing
governed color-JPEG control is now the largest measured encoder gap. Claims stay
limited to the tested controls; the full 257-case CPU-parity objective remains open.
