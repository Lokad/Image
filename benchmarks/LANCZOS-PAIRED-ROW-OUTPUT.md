# Certified Lanczos row output

The shared accelerated row writer certifies two adjacent RGBA pixels together,
keeps an independent mask for each pixel and repairs only uncertain pixels with
the existing exact arithmetic. Packed stores publish complete samples; odd tails
and low-alpha groups keep the original certificate. Q30 weights, FMA accumulation
order, error bounds, full16-bit precision and exact managed pixels are unchanged.
One-shot and prepared execution share the same row boundary and retained owners;
no new file hierarchy, cache, session or storage is introduced.

Independent prepared word enlargement improves **4,782.06 → 4,184.41 µs**,
**12.50% raw / 12.43% normalized**, saving **597.65 µs**. Prepared word reduction
improves **614.96 → 563.58 µs**, **8.35% / 8.41%**, saving **51.38 µs**. Filtered
and all-actual raw confidence and conservative normalized intervals separate.
The native reference runs 0.076% faster for enlargement and 0.066% slower for
reduction. Both prepared word controls retain parity gaps.

| Existing 513×259 operation / zimg, reduction256×129 or enlargement1027×519 | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Plan retained B |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8 reduction, one-shot | 725.70 | 1,575.62 | 0.461 / 0.463 | 138,643 | 0 |
| RGBA8 reduction, prepared | 603.45 | 488.73 | 1.235 / 1.240 | 139 | 139,800 |
| RGBA16 reduction, one-shot | 718.06 | 1,819.13 | 0.395 / 0.397 | 146,869 | 0 |
| RGBA16 reduction, prepared | 563.58 | 511.13 | 1.103 / 1.115 | 139 | 148,040 |
| RGBA8 enlargement, one-shot | 3,939.25 | 5,246.65 | 0.751 / 0.755 | 319,224 | 0 |
| RGBA8 enlargement, prepared | 3,666.99 | 4,054.17 | 0.904 / 0.907 | 148 | 325,156 |
| RGBA16 enlargement, one-shot | 4,519.20 | 5,067.93 | 0.892 / 0.896 | 4,592,777 | 0 |
| RGBA16 enlargement, prepared | 4,184.41 | 3,897.97 | 1.073 / 1.080 | 4,265,302 | 333,396 |

All eight affected comparisons receive independent twelve-actual confirmation on
one immutable selected core. Five pass and stop, including the previously uncertain
word one-shot enlargement. Prepared byte reduction and both prepared word controls
retain gaps. The six other refreshed costs establish current scope; no incremental
gain is claimed for them. No workload grid expands and the 282-path goal stays open.

Both sides include necessary geometry/workspace setup and independent output/
disposal for one-shot operations, and exclude explicit retained preparation for
prepared calls. Source processing, normalization, output ownership and disposal stay
timed. Original source/settings, exact managed pixels, full samples, native RGB/
alpha maximum≤1/RMSE≤0.25 and versioned quiet guards pass on one CPU. Native float
arithmetic does not relax managed Q30/rational/certified rounding.

An initial packed single-pixel writer has adverse enlargement/reduction screens
(5.160/0.623 ms versus 4.657/0.615 ms). The paired stage leads. A coherent final-tap
fusion/initialization stage preserves pixels but has no useful lead (4.678/0.637 ms)
and is removed. Select the simpler paired stage from saved source/core/tests without
rebuilding or repeating the adverse stages. Wide short intervals establish neither
causal regression nor parity. Their actuals remain; no permanent design-family ban.

There are no new arrays, admission requests or retained capacity. Independently
compared word allocations and retention are unchanged; the initial +30 B/op short
counter point does not recur in confirmation. Small allocation-reporting differences
in other refreshes are retained without capacity-saving claims. Workspace/result
ownership, terminal refusal and cleanup are unchanged. Complete admitted/physical
peaks and native allocation/graph/input-plane retention are not newly measured.

BigInteger envelope checks now validate certified pairs, and two additional cases
check independent RGB/alpha uncertainty and transparent output. Both OSes pass
**5,545 full/live checks** (2,322 internal + 2,275 public + 948 differential), plus
74 internal/15 public Lanczos checks per normal/scalar mode. All eight frozen
boundaries pass normal/scalar checks on Linux. The same actual Release package and
all three examples pass Windows/Linux, including retained plans and async survivors.

Three Linux core/internal builds cover distinct stages; Windows builds only the
selected stage once. Existing workers/harnesses/selectors/public tests are reused.
No core rebuild after timing; one package build. A preparation metadata stage-label
typo is corrected without rebuilding or repeating checks; original logs remain.
Published packages are unchanged. This is a development update, not a full release
refresh; publication remains postponed.

[Evidence](lanczos-paired-row-output-evidence.json) pins all current costs, independent
comparisons, sources, qualified producers and actual package. Raw
`lanczos-paired-row-output-qualified.tar.xz`: 31,341,032 B / 430 members, SHA256
`1ddefb20959ba356f501e335f90e588082b9bd1037242ca42747c1a88bf8c9cf`,
mirrored/member-verified. It retains every stage, source/actual binary, exact gates,
guards, actuals, package/consumer proof and reproduction. The qualified baseline,
equivalent-reference and pre-change word-attribution archives are pinned dependencies.
