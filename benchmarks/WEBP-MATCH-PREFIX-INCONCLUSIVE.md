# WebP match-prefix screen

The native matcher uses an exact mismatch operation after its preliminary best-
length probe. A candidate independently uses the safe BCL uint common-prefix
operation in bounded 256-sample chunks, retaining that probe, whole-sample
comparisons, match/candidate order, ties and packets. No owned storage is added.

| Existing complete encode, three direct batches | Current µs | Candidate µs | Raw gain |
| --- | ---: | ---: | ---: |
| Lossless photograph | 104,792.53 | 105,470.74 | −0.65% |
| Lossless text/diagram | 9,539.85 | 9,647.03 | −1.12% |
| VP8 with lossless alpha | 70,992.92 | 71,556.00 | −0.79% |
| Existing authored bars | 907.53 | 892.67 | +1.64% |

This reuses the unchanged four-control screen and one fresh qualified current
before/candidate pair. Both quiet guards, exact complete packets, source/output
ownership and root cleanup pass. Allocation and admitted peaks are identical.
There is no native timing or statistical confirmation here. The points establish
no material gain or causal regression; restore without independent confirmation
or favorable resampling. Current qualified comparison tables remain unchanged.

The original profile's 15.5% inclusive matching attribution was too broad to
establish prefix-loop headroom. Its existing detail assigns 9/779 exclusive
samples to Length and 102/779 to Build; these are attribution, not method timings.
Use that detail and substantial storage lifetimes before a different redesign.

Candidate checks pass 236 normal/scalar per OS, including frozen complete match
indices, packets, overlap/tail/chunk/probe comparisons, cancellation and shared
admission. An initial Windows cancellation-test setup was corrected without a
production rebuild; failed logs remain. Complete source/tests/DLLs are restored;
235 normal/scalar checks per OS pass again. Candidate-only helper exposure and
its direct test are removed. Existing production index/packet coverage remains.

The unchanged qualified core/package reuses 5,423 full/live checks per OS and the
same actual Release-package consumer/three examples on both OSes. No package
regeneration or broad repeat. [Evidence](webp-match-prefix-inconclusive-evidence.json)
pins the complete screen, exact producers, failed/corrected checks and restoration.
Raw webp-match-prefix-inconclusive.tar.xz: 932,508 B / 84 members, SHA256
38238665746647993609c235dd14715a747cbf6e50ccd614ab3295813ad389a1,
mirrored/member-verified; baseline qualification/source depends on the retained
shared-compositing archive. The next bounded audit traces major admission
lifetimes on two existing encodes using the qualified DLL, without CPU claims
or production instrumentation.
