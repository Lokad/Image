# RGBA16 Lanczos3 product trial

Replacing signed64 SIMD products with unsigned32 multiplication plus exact sign
correction was **4.9% slower** on the existing complete Reduce16 operation.
The candidate is rejected and the qualified production code is restored.

| Existing Reduce16 operation | Lokad µs | Native µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Qualified old producer baseline | 807.39 | 509.66 | 1.584 / 1.674 | 138,454 |
| Rejected unsigned-product candidate | 847.22 | 506.16 | 1.674 / 1.986 | 138,449 |

Each row retains three actuals, native pre/post brackets and both unchanged
idle-v2 guards. The first runtime retained the old producer despite the candidate
build; it is labeled as a baseline. A second launch failed identity setup before
candidate exercise. Both are retained. The actual candidate passed the original
full-frame pixel, native-quality and independent-owner gates before timing.
There was no long repeat, enlargement run or new fixture/settings grid.

The identity is exact over the full UInt32-by-Int32 domain. Fifty-six focused
controls pass normally, scalar and with AVX512 disabled per OS, including original
pixel certificates, work/cancellation and admission behavior. No new storage or
owner. Rejected code does not trigger full suites or package qualification: the
restored accepted build retains its 4,469-test/OS and actual-package proofs.

One current accepted-code profile retains 944 samples with both official guards:
RGBA16 horizontal filtering 50.4%, final FMA pass/normalization 17.7%, vertical
accumulation 15.0%, and native trigonometry 7.7%. These are profile shares, not
comparative timings. No safe .NET10 IFMA52 intrinsic is available for the observed
hardware. The current normalization already uses one packed division; replacing
it with a reciprocal is a smaller opportunity.

Next: assess vertical-first RGBA16 reduction, putting integer weighting on
contiguous rows with one coefficient broadcast. Preserve Q30 plans, exact pixels,
the certified error bound with axes exchanged, original exact fallback and
admission/cancellation/work gates. Screen only the existing Reduce16 operation.

The accepted four Lanczos rows and finite 254-case checklist remain unchanged.
Full warm parity and release readiness remain open. [Evidence](warm-lanczos3-product-trial-evidence.json)
retains every statistic, allocation, producer identity and mirrored raw archive.
