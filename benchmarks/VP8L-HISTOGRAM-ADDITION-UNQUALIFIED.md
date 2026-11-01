# Unqualified VP8L histogram addition

SIMD histogram addition independently saves **4.66% filtered / 4.78% all-actual
elapsed**, with exact packets and unchanged allocation. Native-normalized point
gain is **4.00%**; its conservative confidence overlaps the original. Restore
production byte-exactly; this candidate is not accepted. The previously approved
color-bin gain and its specific confidence caveat remain intact.

| Existing opaque complete encode | Lokad µs | Ratio / upper | Managed B/op |
|---|---:|---:|---:|
| Qualified original, independent12 | 99,191.765 | 1.393 / 1.447 | 19,992,640 |
| Candidate short3 | 93,381.408 | 1.337 / 1.536 | 19,992,640 |
| Candidate independent12 | 94,573.228 | 1.337 / 1.369 | 19,992,640 |

Raw confidence separates in both views. Normalized candidate upper **1.369**
exceeds original lower **1.349**. No second pair or SIMD-width/inlining variant.
The short's 5.86% elapsed / 4.05% normalized gain earned one confirmation by
explicit bounded judgment: it narrowly missed literal 4.1% screen triage by
0.049 percentage point. That decision granted no final confidence waiver.

Two scalar clustering bank-add loops share one safe Vector512/256 helper with
scalar tails/fallback. Modular integer sums, merge order/ties, entropy decisions,
cancellation checkpoints, capacity/admission and output ownership are unchanged.
The original 513×259 opaque method4/Q75/exact1 input/settings remain frozen.
Both complete packets are exactly **121,896 bytes**; all pixels/alpha agree.
Complete normalization/repacking, fresh output growth/ownership and release stay
inside each timed operation. Source, independent owners and quiet-v2 gates pass.

The clustering source matches retained VPR1 attribution byte-exactly. Its old
26 Combine / 25 Finish samples out of 762 support one bounded trial; historical
fractions are not current phase costs or predicted gains. No new profile.

Candidate **29 internal / 153 public** controls pass normal/scalar Linux, including
exact cluster/packet checks, arithmetic guards/offsets/full integer patterns/tails,
zero allocation and shared-memory refusal/cancellation cleanup. Restored original
**28 / 153** pass both modes; the helper-only test is removed with the candidate.
Source/core/PDB/XML/all saved linked binaries restore byte-exactly, without builds.

One Linux core/internal build, zero public/harness/native/selector/Windows builds.
Unchanged 5,342 checks per OS, three additional probe contracts and SAME actual
Release package/three-example proof are reused. CI7e75645 is green in all three
jobs. Published releases and current benchmark costs are unchanged. All **268
required paths** remain; close histogram-add width/packing/loop/inlining variants.

[Numeric evidence](vp8l-histogram-addition-unqualified-evidence.json) records the
decision and exact raw references. `vp8l-histogram-addition-unqualified.tar.xz`, SHA256
da33e7c434a8515b71d4677b574a893ec6b387aa370b9176499b37abcaf0b70f (1,666,880 B / 77 members),
is mirrored and member-verified, including original/candidate source and binaries,
both scores, actuals/brackets/guards and the reused profile. J4N31 and retained
CBA2 archives supply unchanged qualification and original-cost dependencies.
