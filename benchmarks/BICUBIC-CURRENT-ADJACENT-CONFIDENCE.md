# Current bicubic confidence

Three existing bicubic comparisons **pass and stop** on the current immutable
package DLL; the 16×8 word control stays uncertain. No production change or speedup claim.

| Existing original 513×259 complete operation | Lokad / zimg µs | Conservative ratio interval | Status | Managed allocated B/op |
| --- | ---: | ---: | --- | ---: |
| Default mixed RGBA16 → 1027×129 | 789.05 / 837.28 | 0.935–0.946 | pass | 112 |
| Bounded wide RGBA8 → 128×64 | 340.70 / 347.49 | 0.972–0.984 | pass | 112 |
| Bounded wide RGBA16 → 128×64 | 398.16 / 405.63 | 0.968–0.991 | pass | 112 |
| Bounded RGBA16 → 16×8 | 496.74 / 495.21 | 0.985–1.011 | uncertain | 112 |

Complete public Resize includes geometry preparation, admission, filtering, exact
repair, independent output acquisition and disposal. The explicitly labeled zimg
3.0.6 FFmpeg scaler backend retains graph/input/scratch preparation; deinterleaving,
clamped halo, filtering, unpremultiplication, quantization, output allocation/packing
and release remain timed. These are **asymmetric diagnostics**, distinct from the
already-passing equivalent one-shot/prepared comparisons. Necessary managed work
stays timed. Exact managed rational/full-word pixels, native-only RGB/alpha error≤1
and RMSE≤.25, zero-alpha RGB, original hashes, normal/scalar native bounds, immutable
source and independent outputs pass unchanged.

Each gets one capture of 12 alternating pairs, 64 complete operations per subject,
with 12 native batches before/after. Every actual and the established conservative
paired/three-bracket confidence rule remain. Quiet CPU0, optimized JIT,
nonconcurrent GC and both idle-v2 guards pass. Historical clock changes and the
earlier scoped 3.29% bounded-wide DLL gain are not new production/native-parity gains.
Original uncertainty, adverse results and scoped gains remain linked separately.
The uncertain 16×8 word result is not retried. Default byte-wide already has valid
current adjacency and is explicitly reused **without resampling**.

**Zero builds:** all four reuse the same cached original harness, current qualified
DLL and verified adjacent driver. A mixed-word receipt-reader field mismatch is
recovered from completed logs/validation/guards without remeasurement; original
failure and recovery records remain. No production or tool input changes.

Allocation is **112 B/op**, distinct from old 113–114 B/op aggregate counters;
no intrinsic allocation saving is claimed. Owned/admitted capacity and retention
do not change. Mixed source/result are 1,062,936/1,059,864 B. Bounded byte source/
result are 531,468/32,768 B under a 568,332 B local ceiling. Bounded wide/small word
results are 65,536/1,024 B, with the same 1,062,936 B source and 4,096 B additional
workspace ceilings. Existing output loans, exact repair, cancellation and prompt
owner cleanup remain. Operation scratch retention is zero; the two-slot/4 MiB
pixel store and caller-root bypass are unchanged. These capacities are not
physical/native/JIT peaks, which remain unmeasured.

Reuse **6,624 Windows/Linux full/live checks**, focused scalar/ISA, three named
resize-storage suites and SAME actual-package consumers/all three examples.
No broad/package repeat. Current 73 resize views: **70 pass / three uncertain**;
the remaining names are default Wide8, Tight16x8_16 and bilinear Up16. Current
191 image views: **179 pass / one gap / 11 uncertain**. Full 289 material paths
remain required; publication is postponed and published releases stay immutable.

[Evidence](bicubic-current-adjacent-confidence-evidence.json) is a compact current
summary; full observations, identities, original gates and reproduction remain
in mirrored/member-verified raw archives:

- `bicubic-mixed-wide-current-adjacent.tar.xz`: 31,252 B / 29 members,
  SHA256 `11f177d4792321a07e5870a89d8636df967b6878635362e230f717a024808275`.
- `bicubic-word-current-adjacent.tar.xz`: 29,708 B / 22 members,
  SHA256 `d0f793e48d419c61de2a1c58d7f574e640bdbe029280e31777291c69a31a5a5e`; depends on the earlier archive and released-storage evidence.

Dependencies retain original fixtures, workers, harness, previous experiments and
qualified packages. No image, shape, depth, filter or settings grid is added.
