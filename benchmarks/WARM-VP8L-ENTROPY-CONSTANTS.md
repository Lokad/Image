# Exact VP8L entropy constants

The existing513×259 RGBA8 lossless control retains the frozen libwebp reference
(FFmpeg's encoder backend), method4/Q75/exact1 and complete183,812-byte packets.
The existing VP8 alpha control retains its98,590-byte packet and reference settings.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| VP8L short | 494,142.95 | 120,466.29 | 4.102/4.893 | 48,064,592 |
| VP8L confirm | 497,273.01 | 121,131.97 | 4.105/4.153 | 48,064,592 |
| VP8 current short | 245,441.85 | 53,516.53 | 4.586/5.114 | 24,093,944 |

Independent VP8L confirmation saves17.8% against the
[previous605,260.92-µs screen](WARM-CURRENT-ENCODER-COSTS.md), with unchanged
allocations. Both versioned idle guards pass. Operations include normalization,
encoding, fresh owned output and release. VP8 has one current-cost screen because
its alpha encoder shares the primitive. CPU parity remains open; claims cover
these fixed controls/settings.

Small counts and reduced heads reuse3KiB of independently derived Q23 constants.
Logarithms and count-times-logarithms keep their distinct original rounding;
larger formulas are unchanged. Compiled spans allocate no managed cache/arrays.
Work, local/shared admission, cancellation and ownership remain unchanged.

Both platforms pass5,055 checks each, including947 live comparisons, three scalar
arithmetic checks and56 affected scalar packet/public-wrapper checks. Counts
through65,535 are exhaustive; large thresholds/extremes and65,536 deterministic
Int32 samples match original formulas. Existing VP8L/VP8 frozen packets remain
exact. The same actual private0.2.0 package and all three examples pass on both.

[Evidence](warm-vp8l-entropy-constants-evidence.json) records samples, identities,
qualification and unchanged-path reuse. The benchmark/native workers are reused;
one archived confirmation body changes only its existing enum case before an
isolated compile. The next largest measured encoder gap is VP8.
