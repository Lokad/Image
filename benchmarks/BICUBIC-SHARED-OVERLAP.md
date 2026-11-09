# Shared ordered bicubic overlap kernel

The scoped gains and adverse evidence below remain historical. Current ten-case
costs, large-mode screening and whole-build qualification belong to
[wide overlap execution](BICUBIC-WIDE-OVERLAP.md).

BOV716 shares one eight-tap overlap kernel between cached and bounded-column
execution. Complete bounded byte reduction saves **33.24 µs / 5.80% raw**,
573.55→540.31 µs, with separated filtered/all-actual raw intervals. The normalized
3.29% point has filtered separation but all-actual overlap; no robust normalized
gain is claimed. Exact managed pixels and owned capacity remain unchanged.

| Existing complete 513×259 resize | Lokad µs | zimg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Byte →256×129, 4 KiB working | 540.31 | 435.80 | 1.240 / 1.259 | 113 | gap |
| Word →256×129, 4 KiB working | 542.87 | 570.29 | 0.952 / 0.955 | 113 | pass |
| Byte →256×129, cached | 497.11 | 453.01 | 1.097 / 1.105 | 199,980 | gap |
| Word →256×129, cached | 505.13 | 620.33 | 0.814 / 0.818 | 199,951 | pass |
| Byte →256×259, cached | 487.75 | 611.61 | 0.797 / 0.830 | 199,980 | pass |
| Word →256×259, cached | 558.03 | 581.75 | 0.959 / 0.965 | 199,933 | pass |
| Byte →256×519, cached | 852.03 | 896.30 | 0.951 / 0.964 | 199,938 | pass |
| Word →256×519, cached | 766.98 | 827.47 | 0.927 / 0.948 | 199,898 | pass |

All eight affected costs refresh on the actual-package DLL: **six pass / two
gaps / zero uncertain**. No previously passing control becomes a gap. Bounded
word reduction and cached horizontal word reduction now pass; passing cases stop.
No new benchmark image, shape or settings grid is added.

The cached byte comparison is adverse in raw time: **484.57→497.11 µs**, +12.55 µs /
2.59%, while native moves +8.97%. Normalized ratios improve 5.86% with separated
intervals in both views. One targeted original-only check gives484.27/native418.38
µs and reproduces the original regime; the completed candidate is reused.
The raw loss remains. No causal absolute gain, nonregression, equivalence or clock
explanation is established, and no further repeat follows. Acceptance combines
the confirmed bounded-byte raw saving, passing word boundaries, shared exact
source kernel and unchanged storage; no uniform elapsed-time or parity claim.

## Coherent implementation

The [audit](BICUBIC-BOUNDED-CURRENT-ATTRIBUTION.md) identifies repeated indexed
horizontal loads. The cached eight-tap/+2 overlap arithmetic now has one shared
source body; specialized schedulers keep their natural geometry and ownership.
Five bounded source windows provide ten pixels instead of sixteen independent
pixel loads. Each lane retains tap order0..7 and separate Multiply/Add. Column
preparation proves eligibility once per strip, resets dead padding each time and
preserves live taps. Clamped/unequal/full-capacity/odd and portable paths remain.

The first stage screens adversely at816.84 versus original588.48 µs. Its inlined
weight-vector construction emits two64-byte stack load/store cycles per tap-pair
iteration. The revision uses fully initialized scalar broadcasts and upper-half
inserts, preserving every weight bit. Generated code removes those cycles;
large/small byte drivers are5,898/6,022 B versus first-stage5,963/6,103 B.
Code structure alone establishes no CPU gain. Both stages, every sample and the
unchanged-build original-only check remain in raw evidence.

No owned array, admission, owner, session or pool is added. Bounded workspace
remains4,096 B, explicit payload4,080/3,232 B; cached byte workspace remains199,840 B.
Retained results are132,096/264,192 B for byte/word reduction until disposal,
with zero retained operation scratch. Default output-pool retention is separate;
governed calls bypass it. Physical/native/total-stack peaks are unmeasured.

Nine new focused controls cover bitwise ordered byte/word sums, low alpha,
borrowed-input canaries, stale/full/odd metadata, incomplete windows and canceled
preparation. Existing actual-column tests retain frozen/direct pixels, result
footers, terminal refusal, tiny-alpha unwind, cleanup/recovery and survivors.
Windows/Linux each pass **6,109 full/live checks**,274internal/729public per
normal/scalar/AVX2-disabled/AVX512-disabled mode, and the SAME actual Release
package/all three examples. Two immutable production/embedded stages per OS
reuse existing harnesses/selectors/native workers/fixtures. A Linux sync preflight
is corrected before mutation/build; the completed Windows stage is reused.

Managed preparation/workspace, computation, independent output and disposal stay
timed on the exclusive quiet VM, sequential Release/one CPU. Native zimg retains
its labeled graph/scratch. Exact binary64/certified/rational managed pixels remain
distinct from native float; encoder packets and metadata contracts are unchanged.
The byte horizontal identity boundary retains its declared binary64 sample-half
decisions; its frozen managed pixels remain exact, without claiming rational
equivalence at those ties.

[Evidence](bicubic-shared-overlap-evidence.json) owns current costs, comparisons,
source/JIT/lifetime proof and qualification. Mirrored/member-verified raw
`bicubic-shared-overlap-qualified.tar.xz`:8,727,972 B /308members, SHA256
`a13762d3587a04bf0e0cea73e7f9222ebc28b283b951adbc3d9d514c7822e1b5`.
Private package439,796 B, SHA256
`ad519c5f1d8abb304b74fc1457005173d20fd2084c1b6b9964aba81d3a9d67f8`.
Published packages remain immutable; publication is postponed. All283paths remain
required. Next investigate shared window preparation for the larger unchanged
wide-column byte gap; no numerical relaxation or automatic merge is proposed.
