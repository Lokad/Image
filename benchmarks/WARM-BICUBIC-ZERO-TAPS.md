# Bicubic byte zero taps

The existing HorizontalUp8 control falls from 2,134.423 to 1,788.297 µs in fresh
independent twelve-actual confirmations: 16.2% less filtered elapsed, 16.1% using
all actuals. Both confidence-interval forms separate; native bracket drift is
0.18%. Allocation stays 1,930,356 B/op. One short screen precedes confirmation,
without candidate variants. The resulting 1.171× / upper 1.193 remains a gap.

All six affected existing default-workspace byte controls have current costs.
Inputs remain the original 513×259 RGBA8 raster; complete warm single-CPU public
operations include admission, allocation and independently owned output. Frozen
zimg workers represent FFmpeg's zscale backend with unchanged settings/boundaries.

| Existing control | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| HorizontalUp8 → 1027×259 | 1,788.297 | 1.171 / 1.193 | 1,930,356 | Confirmed gap |
| Down8 → 256×129 | 572.970 | 1.374 / 1.441 | 332,188 | Screen gap |
| Up8 → 1027×519 | 4,062.521 | 1.331 / 1.498 | 3,244,842 | Screen gap |
| Horizontal8 → 256×259 | 573.117 | 1.002 / 1.008 | 465,390 | Confirmed gap |
| Vertical8 → 513×129 | 638.513 | 1.082 / 1.089 | 649,006 | Confirmed gap |
| VerticalUp8 → 513×519 | 1,860.063 | 1.066 / 1.075 | 1,664,183 | Confirmed gap |

Down8/Up8 screens differ from earlier unmatched runs by +2.3%/+3.7%; these do not
establish a causal gain or regression. Near-parity companions receive independent
confirmation. Horizontal8's upper exceeds one: it does not pass.

A six-second current-build profile captures 5,935 samples, including 2,982 in
WriteByteRow (50.2%). All writer instruction samples map to the same process's
1,292-byte JIT body. Its vertical tap loop accounts for 593 samples and processes
four exact zero weights when height is preserved. These are diagnostic shares.
The accepted seven-line change trims exact zero end taps once per row, retaining
nonzero accumulation order, normalization and full spans for Original replay.
Finite cached values and integer publication preserve pixels including zero
alpha and exact halves. No buffer, admission, cancellation or public API changes.
All Cached source outside WriteByteRow and BicubicMath remain byte-identical.

Windows/Linux each pass 5,074 checks plus 89 focused cases normal/scalar. Windows
exercises the portable fallback; Linux executes AVX512. The SAME actual private
0.2.1 package passes both platforms and three examples, including frozen byte/word
reductions/enlargements, refusal/refund/recovery and surviving-output ownership.
Published packages remain immutable. Existing six/25 alpha-half certifications
for Horizontal8/Vertical8 and one blue half for VerticalUp8 remain exact against
qualified original pixels; native-only bounds and hashes are unchanged.

Production, harness and native workers are reused during measurement. One original
Vertical8 selector is built because its historical archive omitted generated
source; twelve actuals/unroll16/generated16 are verified. Initial preparation
failure precedes any measurement. Other selectors are reused.

[Evidence](warm-bicubic-zero-taps-evidence.json) retains all samples, confidence,
allocation and qualification pins. Mirrored/member-verified production archive:
bicubic-byte-zero-taps-qualified.tar.xz, 281 members, SHA256
3e137d42ab773bdf87dd364d9956f76a39a7ae1bbe0328a5336a45a1d12fd841.
The diagnostic archive is linked there. Word publication remains unchanged:
VerticalUp16's independent upper 0.973 passes and stops. This round adds no paths
or parity passes and does not complete the full 268-path objective.
