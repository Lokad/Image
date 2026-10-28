# Bicubic vertical coefficient screen: rejected

The existing TightCross16 control screens at1,372.986µs,1.676×/upper1.891,
1,063,410B/op. It fails the5% gate against the applicable fresh1,263.401µs
baseline. No speedup/regression claim follows from unmatched short screens.
Exact managed pixels, native maximum1/RMSE≤0.25 bounds, frozen source, owners
and both quiet guards pass. No variants or independent timing confirmation.

The candidate reuses the existing SIMD coefficient generator for vertical taps
in the constrained RGBA16 strip. Eligible height preservation/enlargement makes
the vertical scale1. First four/fifth coefficients use existing five-double
scratch, then the original order. Workspace, guards, fallback and source remain.
Despite bit-identical coefficients, the complete operation has no useful gain.
Production source and qualified actual DLLs/package are restored exactly.

Retained independent tests compare active SIMD coefficient bits with the scalar
kernel on the frozen enlargement, identity-height and one-row edge controls.
They pass before edits and after restoration on Linux; Windows lacks AVX512
and provides compilation/portable-path proof. Candidate/restored16 affected
public cases pass normal/scalar per platform. Unchanged full and same-package
qualification is reused. The full268-path objective stays open.

[Evidence](bicubic-vertical-coefficients-rejected-evidence.json) retains samples,
uncertainty, pins, checks and restoration. Mirrored/member-verified raw
bicubic-vertical-coefficients-rejected.tar.xz, SHA256
1fb9ed31bc26486ffef41d8c9dad01fd8e8bc6ac1242c2dbaea9a26347ec838e (142members),
retains source/producers/selectors/guards/receipts and links the reused
[current attribution](BICUBIC-WORD-GUARD-VECTOR-REJECTED.md).
Reproduce with existing WarmShort/TightCross16 and unchanged reference tooling.
