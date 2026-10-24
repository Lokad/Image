# Exact compressed-alpha predictor histograms

The existing 513×259 RGBA8 VP8 control retains Q75, method4, exact alpha and
raw-exact 98,590-byte packets. Complete operations include encoding and fresh
independent owned output acquisition/release. The lossless control retains
raw-exact 183,812-byte packets and uses explicitly labeled libwebp exact=1,
FFmpeg's encoder backend. Inputs, reference settings and gates are unchanged.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| VP8 short | 154,647.40 | 53,398.27 | 2.896/3.311 | 24,080,952 |
| VP8 independent | 154,448.09 | 53,667.00 | 2.878/2.901 | 24,080,952 |
| VP8L current short | 376,354.64 | 121,612.72 | 3.095/3.544 | 48,056,112 |

VP8 independent confirmation uses 7.3% less elapsed than the
[previous confirmation](WARM-VP8-AVX2-LEVELS.md), with unchanged allocations.
All 12 actual iterations are retained and selected; both idle-v2 guards pass.
VP8L is a current short reconciliation, without an independent speedup claim.

ALPH normalization puts filtered alpha in green and zeroes the other channels.
Their predictor histogram bins can be calculated directly. Green residuals,
edge rules, mode costs/ties, parent merging and packet emission remain exact.
Palette, subtract-green and all-family trials keep generic analysis. The same
borrowed storage, admission, work/cancellation cadence and ownership remain.
No heap storage, cache, extra scan or reservation is added.

Both platforms pass 5,096 checks each, including 947 live comparisons, 50 scalar
predictor/alpha checks and 29 scalar public VP8 checks. Frozen full-histogram,
edge, map/residual/cost, work, refusal/cancellation cleanup and recovery checks
pass. The same actual private 0.2.0 package and all three examples pass both OS.
An initial Windows grayscale allocation assertion passed three fresh focused
runs and the unchanged full rerun; the cause remains unproven and raw failure
is retained. No gate changed.

[Evidence](warm-vp8-alpha-histogram-evidence.json) records samples, identities,
qualification and reuse. JPEG/PNG/decoders/editing/text are unchanged. VP8L is now
the largest measured encoder gap; the full 257-case CPU-parity objective is open.
