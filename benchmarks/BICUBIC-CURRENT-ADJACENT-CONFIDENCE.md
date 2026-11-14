# Current bicubic confidence

[Later default Wide8 confidence](BICUBIC-WIDE-CURRENT-CONFIDENCE.md) now passes
with one longer integration. Current resize scope is 71 passing/two uncertain;
the four controls below retain their valid measurements and original history.


Three existing bicubic comparisons **pass and stop** on the current immutable
package DLL; the 16×8 word control stays uncertain. No production change or speedup claim.

| Existing original 513×259 complete operation | Lokad / zimg µs | Conservative ratio interval | Status | Managed allocated B/op |
| --- | ---: | ---: | --- | ---: |
| Default mixed RGBA16 → 1027×129 | 789.05 / 837.28 | 0.935–0.946 | pass | 112 |
| Bounded wide RGBA8 → 128×64 | 340.70 / 347.49 | 0.972–0.984 | pass | 112 |
| Bounded wide RGBA16 → 128×64 | 398.16 / 405.63 | 0.968–0.991 | pass | 112 |
| Bounded RGBA16 → 16×8 | 494.73 / 494.78 | 0.993267–1.003069 | uncertain | 112 |

Complete public Resize includes geometry preparation, admission, filtering, exact
repair, independent output acquisition and disposal. The explicitly labeled zimg
3.0.6 FFmpeg scaler backend retains graph/input/scratch preparation; deinterleaving,
clamped halo, filtering, unpremultiplication, quantization, output allocation/packing
and release remain timed. These are **asymmetric diagnostics**, distinct from the
already-passing equivalent one-shot/prepared comparisons. Necessary managed work
stays timed. Exact managed rational/full-word pixels, native-only RGB/alpha error≤1
and RMSE≤.25, zero-alpha RGB, original hashes, normal/scalar native bounds, immutable
source and independent outputs pass unchanged.

The three passing rows use 12 alternating pairs of 64 operations. A later
prespecified 64→256 integration of bounded word 16×8 (~32→128 ms) remains uncertain:
494.73/494.78 µs, interval .993267–1.003069, 112 B/op. It uses the current immutable
actual package (DLL b1e968e8, package 03f0db38) and reuses its 6,690 full/live checks
per OS, existing scalar/resize-storage and SAME-package/all-three-example proof.
No builds or broad/package repeats. Original 496.74/495.21 µs and .984873–1.011176
remain history. No presumed gap, production gain or further identical retry.
All twelve actuals and twelve samples in each native bracket are retained. Every actual and the established conservative
paired/three-bracket confidence rule remain. Quiet CPU0, optimized JIT,
nonconcurrent GC and both idle-v2 guards pass. Historical clock changes and the
earlier scoped 3.29% bounded-wide DLL gain are not new production/native-parity gains.
Original uncertainty, adverse results and scoped gains remain linked separately.
Default byte-wide was reused in the original round; its later longer integration
is linked above. Both longer integrations preserve the original confidence rule.

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
No broad/package repeat. At this milestone 70 resize views passed and three were
uncertain. The [current family status](WARM-COVERAGE.md) now has 71 passing/two
uncertain resize views after default Wide8 resolution. Tight16x8_16 and bilinear
Up16 remain uncertain. Full 289 material paths remain required; publication is
postponed and published releases stay immutable.

[Evidence](bicubic-current-adjacent-confidence-evidence.json) is a compact current
summary; full observations, identities, original gates and reproduction remain
in mirrored/member-verified raw archives:

- `bicubic-mixed-wide-current-adjacent.tar.xz`: 31,252 B / 29 members,
  SHA256 `11f177d4792321a07e5870a89d8636df967b6878635362e230f717a024808275`.
- `bicubic-word-current-adjacent.tar.xz`: 29,708 B / 22 members,
  SHA256 `d0f793e48d419c61de2a1c58d7f574e640bdbe029280e31777291c69a31a5a5e`; depends on the earlier archive and released-storage evidence.

Dependencies retain original fixtures, workers, harness, previous experiments and
qualified packages. No image, shape, depth, filter or settings grid is added.

[Current bounded-word evidence](bicubic-bounded-word-current-confidence-evidence.json)
pins every original identity, sample, exact/native gate and ownership boundary.
Raw `bicubic-bounded-word-long-integration.tar.xz`, 24532 B / 19 members,
SHA256 `a1db6c3f2d8136e083ae9773b878a20ff18e5999aa7574f687bf7baff3fdd5c9`, is mirrored/member-verified.
The 4,096 B additional-workspace allowance is a ceiling, not an actual physical
peak; independent output owns 1,024 B and operation scratch retention remains zero.
