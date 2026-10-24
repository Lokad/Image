# Exact VP8L paired-population costs

The existing513×259 RGBA8 control retains libwebp (FFmpeg's encoder backend),
method4/Q75/exact1 and its complete183,812-byte packet. The existing VP8 alpha
control retains its98,590-byte packet and frozen reference settings.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| VP8L short | 374,208.48 | 121,668.09 | 3.076/3.722 | 48,056,112 |
| VP8L confirm | 375,223.15 | 121,375.26 | 3.091/3.127 | 48,056,112 |
| VP8 short | 197,551.76 | 53,631.60 | 3.683/4.104 | 24,080,952 |

Independent VP8L confirmation saves24.5% against the
[previous497,273.01-µs confirmation](WARM-VP8L-ENTROPY-CONSTANTS.md), allocating
8,480 fewer bytes per operation. Both versioned idle guards pass. Complete
operations include normalization, encoding, fresh owned output and release.
VP8 has one affected-path current-cost screen, without an independent speedup
claim. CPU parity remains open; these results cover the fixed controls/settings.

Paired costs scan borrowed count banks directly and aggregate equal summed runs,
removing a temporary sum array and repeated entropy scans. Exact Q23/Int128 costs,
bank order, early comparisons, ties and logical work charges stay unchanged.
Local/shared admission, cancellation and output ownership remain enforced.
No upstream implementation data, cache or new encoding mode is introduced.

Both platforms pass5,062 checks each, including947 live comparisons,22 scalar
arithmetic/clustering checks and56 scalar reference-packet/public-wrapper checks.
10,080 original mathematical cases retain frozen exact costs; existing native
cluster maps/populations and complete packets remain exact. Focused tests cover
input ownership, zero per-call allocation, work limits and cancellation. The same
actual private0.2.0 package and all three usage examples pass on both platforms.

[Evidence](warm-vp8l-paired-population-evidence.json) records samples, identities,
qualification and unchanged-path reuse. Immutable benchmark/native/confirmation
workers are reused. JPEG/PNG, decoders, editing and text remain unchanged.
