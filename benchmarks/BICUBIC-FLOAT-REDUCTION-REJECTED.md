# FP32 bicubic reduction — rejected

One candidate reduced allocated storage but failed the elapsed-time screen.
It is removed without variants, profiling or an independent candidate run.

| Three-actual quiet screen | Lokad µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Original | 571.939 | 1.367 / 1.716 | 332,188 |
| Rejected FP32 | 958.044 | 2.283 / 3.197 | 276,834 |

The candidate used a smaller FP32 row cache and FMA filtering, binary64
publication and conservative rounding certificates, retaining original-order
full-precision replay for uncertain pixels. A bounded range proof preceded code.
Original rational/qualified managed pixels, native bounds/hashes/settings,
source/owners and both quiet guards pass. There is no accepted gain or broader
parity claim. All actuals and filtered/unfiltered native-bracket bounds remain.

Production source and exact DLL/PDB/XML are restored on both OSes. Candidate
and restored builds pass 661 affected public checks normal/scalar per OS.
Fourteen useful geometry, transparency, independent-owner, aggregate-refusal,
cancellation and recovery checks remain against actual production; no obsolete
kernel or hook remains. A portable test assumption and test-sync failures were
corrected without rebuilding production or rescoring successful jobs.

One candidate core per OS, zero harness/native/selector builds and zero rebuilds
for restoration. Applicable 5,220 full checks per OS and the unchanged actual
Release-package/governed consumers/three examples are reused. Current Down8
remains [its independent 1.384× / upper1.400 cost](CURRENT-BICUBIC-BYTE-REDUCTION-COST.md).
The full 268-path objective is unchanged.

[Evidence](bicubic-float-reduction-rejected-evidence.json) pins the candidate,
proof, rejection, restoration and mirrored/member-verified bicubic-float-reduction-rejected.tar.xz,
SHA256 04441aa14f2a56aee1d8394d2680c6b9aa98f2f116f6f1ce2579e5645b21885b. Close this FP32/certificate family.
