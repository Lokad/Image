# Exact JPEG grayscale row batches

RGBA grayscale preparation batches eight already-normalized packed pixels using
safe AVX2 binary64 operations. Separate multiplies/additions, then +0.5/floor,
preserve the original scalar arithmetic; no FMA or intermediate rounding. Partial
rows retain the scalar path and replicate their last edge sample once. CallerGRAY
bypasses this preparation; the callerRGB branch is unchanged.

| Existing complete513×259RGBA8→grayJPEG, Q69 | Before µs | Selected µs | SIMDlibjpeg-turbo µs | Ratio / upper | Allocated B/op | Complete packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Independent confirmation | 1,693.24 | 1,660.52 | 1,522.24 | 1.091 / 1.103 | 155,436 | 58,910 |

Independent confirmation saves **32.71µs / 1.93% raw /
4.04% reference-normalized**. Filtered and all-actual raw/normalized
confidence separate. The native clock is **2.19% slower**, disclosed with
the raw gain. All12actuals, estimator exclusions, native brackets and quiet guards
remain. Two three-actual complete shorts precede the independent pair; no retune
or favorable resampling. The scoped CPU gap remains open.

Reference is explicitly labeled SIMD-enabled standalone libjpeg-turbo3.1.4.1,
approved only for single-component grayscale benchmark tooling. Both sides time
normalization/flattening on0x204060, complete encoding, fresh output acquisition/
growth and release on one CPU. Native wrapper/options may be retained. Frozen
inputs, quantizers and raw-exact58,910B packets establish equivalence; equal
numeric quality is not the proof. Claims cover this control, without a broader
content/settings or decoder claim.

All16,777,216RGBtriples match the separate binary64 scalar oracle. The existing
10,766block corpus retains689,024frozen gray bytes across depths, opacity, padding
and edges. Unaligned rows, tails/guards and borrowed inputs remain exact. Each OS
passes **5,615 full/live checks** (2,380internal +2,287public +948differential),
plus371internal/331public JPEG checks in normal/scalar modes. Existing cancellation,
terminal refusal, tight/recomputed paths, owner cleanup and later root reuse pass.

The SAME actual Release package/all three examples pass bothOS, including frozen
grayscale packets at8/16bit depths and shared/async ownership. Published releases
are unchanged. One core/internal build perOS serves development/qualification;
no public/harness/native/selector/audit rebuild, one package milestone.

Allocation stays155,436B/op; no new heap arrays/reservations/retained state. Packed
row scratch stays within the existing8KiB fixed workspace. Admitted/physical/native
peaks are not newly measured. Only ImageBuffer grayscale encoding costs are
affected; callerGRAY/RGB, colorJPEG and other families reuse valid proof. Other
affected image-grayscale timing remains historical until refreshed when needed.

[Evidence](jpeg-grayscale-row-batch-evidence.json) pins sources, producers,
allactuals/confidence, exact gates and actual-package qualification. Mirrored/
member-verified raw `jpeg-grayscale-row-batch-qualified.tar.xz`: 4,711,412B /
199members, SHA256 `6468a48369bf6b1037e31547d99e0e78353cb760918ed41b44e0694acacfb204`.
Original/selected modules, tools, checks, consumers and reproducibility remain;
qualified baseline and unchanged reference/runtime dependencies are pinned.
Publication stays postponed; the full282-path objective remains open.
