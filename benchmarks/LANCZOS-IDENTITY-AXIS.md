# Canonical Lanczos identity-axis preparation

An unchanged axis now retains one exact Q30 unit weight per integer sample.
The previous seven-tap support contained only zero neighbors. This removes
coefficient scratch, repeated normalization and unnecessary ring rows while
keeping shared exact/FMA execution, certified repair and explicit ownership.
No public API, pool, cache, governor, numerical policy or fixture dispatch is added.

The nonidentity builder, Support and Kernel reconstruct exactly by removing
only the identity guard/helper. Eight frozen nonidentity coefficient hashes
remain exact. New small8/16-bit one-axis reductions/enlargements match independent
decimal/BigInteger pixels, including zero/tiny alpha and edge behavior. Every
identity growth is checked for terminal refusal/cancellation, cleanup and recovery.

| Declared complete operation / labeled zimg3.0.6 FFmpeg backend | Managed / reference µs | Upper / status | Managed GC B/op | Evidence |
| --- | ---: | --- | ---: | --- |
| OneShot8 | 588.874 / 1568.564 | 0.378 / pass | 138,650 | immutable current DLL |
| Repeated8 | 462.403 / 488.521 | 0.953 / pass | 138 | reused unchanged execution |
| OneShot16 | 623.136 / 1806.503 | 0.350 / pass | 155,075 | immutable current DLL |
| Repeated16 | 485.357 / 512.069 | 0.958 / pass | 138 | reused unchanged execution |
| OneShotEnlarge8 | 3768.254 / 5225.863 | 0.726 / pass | 319,224 | immutable current DLL |
| RepeatedEnlarge8 | 3554.708 / 4074.816 | 0.882 / pass | 148 | reused unchanged execution |
| OneShotEnlarge16 | 4080.927 / 5074.412 | 0.808 / pass | 4,592,818 | immutable current DLL |
| RepeatedEnlarge16 | 4117.261 / 4212.386 | 0.990 / pass | 4,264,760 | reused unchanged execution |

All eight declared comparisons pass and stop. Four affected one-shot costs
refresh on the same immutable DLL. Four prepared nonidentity execution paths
reuse valid producer-specific evidence: coefficient bits, workspace, arithmetic,
output ownership and their timed boundaries are unchanged. One-shot includes
necessary fresh geometry/work preparation and complete owned output acquisition/
disposal on both sides. Prepared comparisons retain plans outside both clocks.
Native float maximum1/RMSE≤.25 and exact managed samples remain distinct.
No gain is inferred by subtracting historical one-shot measurements.

The two additional developer diagnostics use the existing frozen513×259RGBA16
input and compare prior qualified DLL with the candidate. They measure complete
public operations and output disposal, not external-reference performance.

| Scoped managed DLL comparison | Baseline → candidate µs | Paired interval | Retained plan B | One-shot admitted peak B |
| --- | ---: | ---: | ---: | ---: |
| Horizontal16 | 616.768 → 608.212 | 0.944319–1.030402 | 102,000 → 46,656 | 632,432 → 577,088 |
| Vertical16 | 1113.835 → 732.670 | 0.645790–0.669915 | 269,364 → 257,100 | 798,780 → 786,516 |

Independent vertical confirmation saves **381.164µs/34.22%**;
its paired and all-actual mean intervals separate. Horizontal CPU remains
inconclusive despite its favorable point: no verified gain or regression.
Horizontal allocation falls **100136–100143.75→44712–44719.75B/op**; vertical
falls **267015.5–267023.25→254671.5–254679.25B/op**. Observed ranges are retained
without asserting their cause. Allocations, retained plans and peak live
admission are separate; physical/native/JIT peaks remain unmeasured.

Each screen/independent confirmation retains12alternating32-operation pairs per
case,64warmups per case, natural nonconcurrent GC and oneCPU0operation at a time.
Source/hash/independent-survivor/root checks occur outside clocks. Every full-frame
hash matches the baseline; source bytes and caller roots survive. Governed probes
bypass default idle storage; the timed complete operations retain its unchanged
two-array/4MiBpolicy. No external one-axis parity claim or new material-path grid.

Windows/Linux each pass **6742full/live checks**, **163internal/15public** in
normal/scalar/AVX512-disabled modes, explicitly including PackedColumnStorageTests,
EnlargingStripBoundaryTests and EnlargementVerticalStorageTests. The SAME actual
Release-package consumer/allthreeexamples pass bothOS, with both new one-axis
directions checking fresh-source reuse, retained admission and asynchronous
surviving outputs. One core/embedded build perOS, zero public/reference/harness/
fixture builds, one portable helper after an initial public-API compile fix,
and oneNoBuild pack. The timed Linux DLL is never rebuilt.

All four reference captures completed before the VM's old parser failed on a
missing BenchmarkDotNet dependency. The already verified local parser processes
their saved logs; no timing job or production build is repeated. Raw observations
and that failure remain. The bounded review also identified duplicated bicubic
geometry preparation loops; it does not justify merging differently owned public
plan scaffolds or adding a common owner abstraction.

[Evidence](lanczos-identity-axis-evidence.json) pins proof, costs and actual package
`e58932e09d4a7c82d0a4bd4e5e4b0a56a6c1621b12f7d0379b5b1b3461d8e416`, containing timed DLL `843fa82f57827a4a5c4f0fe38061db9da92723207d9e0733a3f4d6c70613e045`.
Raw `lanczos-identity-axis-qualified.tar.xz` (5,179,204B/202members), SHA256
`6d967730048bee285c339bea9d7e5af8848927234c8d9020cb9cfb78f956ca08`, is mirrored/member-verified with original tooling, fixture,
reference and prior-producer dependencies. Publication is postponed; published
packages and the full289-path objective remain unchanged and open.
