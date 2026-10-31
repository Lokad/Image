# CMYK byte-plane packing: rejected

One complete sparse-CMYK decode short improves only **0.45%** raw / **0.23%**
native-normalized, missing the retention gate: **642.344 → 639.440 µs**.
Allocation stays **1,085,758 B/op**. Short confidence overlaps; no causal gain
or regression is claimed. No independent candidate confirmation or variant.
The accepted sparse-CMYK cost remains 638.035 µs, 1.442× / upper 1.481.

The trial keeps all 16-pixel loads, inversion and exact Q16 products, replacing
the final two 8-pixel expansion/shift/OR chains with byte-channel packing and
one 64-byte AVX512VBMI permutation. Cold JIT evidence contains VPERMB and the actual
helper's exact unaligned/tail tests pass. Fewer packing operations do not establish
a material complete-operation gain. No new buffer, owner or admission charge.

Candidate 353 internal/346 public JPEG/root controls pass normal/scalar on both
OSes and Linux without AVX512. The four new behavioral controls exercise the
actual production row helper: all 65,536 color/black pairs, both inversion modes,
independent channels/opaque alpha, unaligned rows/tails, canaries, input
immutability and zero warm allocation. Exact frozen managed/native frames,
scalar checks, source/output ownership and both quiet-v2 guards pass.

The original source and core/public-core copies are restored byte-exactly.
Keep the four useful behavioral controls; they cover existing production,
with no dependency on the removed optimization. Both restored internal
assemblies pass **2,162 checks per OS**, including the new controls; four new
scalar controls per OS and Linux AVX512-off also pass. Unchanged 2,185 public +
948 live checks and the same actual Release-package/three-example proof are
reused, giving 5,295 applicable checks per OS. Packages remain unchanged.

One candidate core build and two internal-assembly builds per OS (candidate,
then restored production with retained tests); zero public/differential/harness/
native/selector builds, profiles, new inputs or package-proof repeats.
A completed original short is recovered after an optional descriptor-field
postcheck mismatch; exact input/output/native gates remain and no score repeats.

[Evidence](cmyk-vbmi-packing-rejected-evidence.json) retains actuals, brackets,
JIT proof, decisions, restoration and qualification reuse. Mirrored/member-
verified candidate raw cmyk-vbmi-packing-rejected.tar.xz SHA256
5b871484cd860f3abd1551c8721af7401a0531906a440a88d1d0ebe1924aa109
(2,081,272 bytes, 107 members); restored regression qualification raw
cmyk-row-regression-qualification.tar.xz SHA256
ee5aa868d597d9c886002948116038ca2e5391fb052b682637f5197d2e28d577
(928,156 bytes, 21 members). The [qualified current package](WARM-JPEG-COMPLETED-ROW-REUSE.md)
retains unchanged producer/package evidence. This primitive is closed without
width/inlining variants. All 268 required paths and the ≤1× objective remain.
