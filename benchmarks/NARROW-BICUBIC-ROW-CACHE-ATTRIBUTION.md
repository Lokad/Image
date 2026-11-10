# Narrow bicubic row/cache attribution

NRA764 is read-only attribution on the qualified package. Existing complete
513×259→256×129 RGBA8/4 KiB costs remain484.90/native417.35 µs, upper1.167 gap,
113 B/op. Both actual byte kernels match the accepted coefficient stage after
process-address/security-cookie normalization:5,934/5,999 B. No new statistical
cost, build, fixture, full suite or package proof; current6,172 qualification is reused.

One two-second CPU0 capture has1,996 locations: large-strip656, small-strip1,182,
geometry Build/Read32/30, stride40, Kernel23, paired preparation6. CPU1 samples;
idle-v2/frozen source/exact managed/native-bound/ownership gates pass. Instrumented
elapsed is discarded. Inlining/skid counts are not phase costs or removable shares.

## Storage and schedule hypothesis

The current executor already uses sixteen columns for a96-column prefix, then
eight columns for the160-column suffix. Its cache occupies still-unwritten right
row tails. Borrowing cache/weights from final output rows instead could permit
32-column strips across the top117rows, then original eight-column strips over
the final12rows after the first frame returns and only tail metadata is restored.
Keep the shared executor, coefficient groups/ordered arithmetic/certificates/repair,
old large/small/word/scalar fallbacks and existing geometry tail preparation.

| Logical complete-primary work | Existing | Proposed |
| --- | ---: | ---: |
| Scheduled strip rows | 3,354 | 1,320 |
| Source-row conversions | 6,734 | 2,768 |
| Decoded source pixels | 171,976 | 151,228 |
| Geometry builds, including preparation | 516 | 177 |
| Horizontal column-rows | 66,304 | 67,840 |
| Horizontal weight evaluations | 2,052 | 4,104 |
| Total horizontal/vertical Kernel evaluations | 6,200 | 5,525 |

Extra horizontal filtering and repeated coefficient preparation are explicit
adverse work. These are logical counts/address checks, not CPU forecasts or
actual SIMD/repair proofs. The model retains the existing75×41 fallback, a
rowSlots10 arithmetic boundary and odd-stride generalization; no new timed image
or settings grid. Word remains outside the proposed mode.

The primary loan is9,216 B cache+2,560 B weights=11,776 B inside12,288 B from
12 unpublished result rows. Top writes are disjoint. Up to80 decoded vectors
plus metadata occupy3,424 B explicit stack payload under existing4 KiB admission.
No additional owned capacity/root/pool/session/API. The independent132,096 B result,
retained scratch0, existing output pool/governed bypass and owner lifetime remain.
Complete physical/native/JIT-stack peaks are unknown. Loan users must return
before restoring tail metadata or writing tail pixels; all preparation/restoration
and repeated computation remain inside complete timing.

Next implement closed row-loan/tail modes in the existing executor, with actual
layout/canary/metadata/packed-coefficient/kernel-bit/root/refusal/cancellation/
tiny-alpha/recovery/survivor checks before the existing primary short. Identify
which driver/word/helper costs change; do not retime unchanged passing paths.
Shared loan preparation is useful only if it simplifies the completed design;
no general caching framework or extra generic layer in every pixel loop.

[Evidence](narrow-bicubic-row-cache-attribution-evidence.json) retains current
source/code identity, capture and bounded model. Mirrored/member-verified raw
`narrow-bicubic-row-cache-attribution.tar.xz`:135,916 B/29members, SHA256
`b21184928ce44b36f50517cd9a7fa5445e8020e86ec126d323440a19691cb976`. [Paired coefficient preparation](BICUBIC-PAIRED-COEFFICIENT-PREPARATION.md)
owns reused costs; [wide row reuse](BICUBIC-RESULT-ROW-LOAN.md) supplies the lifetime
pattern. This audit adds no renderer dependency or performance claim.
All283 required paths remain; publication is postponed.
