# Narrow bicubic plan preparation attribution

CPA790 observes the existing 513×259→256×129 RGBA8 public-plan boundary on the
qualified immutable package. Four timestamp reads separate each complete
create/resize/checksum-and-dispose operation. Source and full output hashes stay
exact; CPU 0 and both idle-v2 guards pass.

| Diagnostic phase, µs/op | Batch 1 | Batch 2 | Batch 3 |
| --- | ---: | ---: | ---: |
| Plan creation, validation, admission and geometry/workspace preparation | 35.10 | 34.13 | 33.76 |
| Source reads, exact filtering/repair and independent output | 449.88 | 449.36 | 449.06 |
| Checksum and result/plan disposal | 0.07 | 0.07 | 0.07 |
| Instrumented total | 485.05 | 483.55 | 482.89 |

Each batch lasts 200 ms after warmup, with 413/414/415 complete operations.
The diagnostic allocates 200,056 B/op. It uses public plan creation plus resize,
whose ownership/caller boundary differs from default `ImageTransforms.Resize`.
Timestamp/caller/JIT effects can perturb execution. These are phase observations,
not new benchmark costs, kernel-only timings, native comparisons or additive
savings to subtract from the qualified one-shot/prepared costs.

Creation is material enough to inspect the common factory. It includes validation,
ownership/admission, array preparation, horizontal weights/normalizers and geometry.
Source inspection finds the same **unfactored** Catmull–Rom polynomial in the scalar
builders. A vector candidate must preserve each multiplication/subtraction/addition
and scalar normalizer order. Horner factoring or FMA is not equivalent bitwise.
Start with the cached factory; keep Q30/rational builders and unrelated execution
paths intact until a concrete shared design and complete-operation evidence exist.
No numerical policy, owner, pool or general caching framework is proposed.

One diagnostic tool build references the actual packaged DLL. There is no
production/embedded/benchmark-harness/reference rebuild, new image/settings grid, new profile
or repeated full qualification. Windows/Linux 6,248 checks, resize ISA and the SAME
actual-package consumer/all three examples from [CNP789](CACHED-NARROW-SHARED-PUBLICATION.md)
remain valid and reused. CNP789 commit ae636d61, CI42944904918 is green in all three jobs.
The selected benchmark summary is unchanged and gains no diagnostic dependency.

Actual DLL `9a1498d806db8e60808c6b90f2b58fb2f936262ca72c19e73059c00eae2b12bc`;
private package `565088e0b39ef05cb13b9d0ed7eed204fb0b1ee7050ea017f33ff56ec01ccccd`.
[Evidence](cached-narrow-plan-attribution-evidence.json) pins source/module/input
identities, phase batches, guards and reproduction. Mirrored/member-verified raw
`cached-narrow-plan-attribution.tar.xz`: 222,476 B / 21 members,
SHA256 `93102b4a9479b920d6d0781a90bacea2fcd414563fc69bc58cb39101065e04df`; its pinned CNP789 dependency supplies unchanged
package, fixture and qualification. Publication remains postponed; all 289 required
paths remain in scope.
