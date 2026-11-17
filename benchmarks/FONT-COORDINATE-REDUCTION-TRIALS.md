# Font coordinate-reduction trials

**Neither prototype is retained.** Both preserve exact behavior but establish no
useful complete-operation gain. The qualified
[header-normalization implementation](FONT-HEADER-NORMALIZATION.md), its scoped
FreeType gap and passing complete text integrations remain unchanged.

| Three reversed pairs, 16,384 owned loads per subject | Baseline → prototype µs | Ratio interval / result | GC B/load |
| --- | ---: | --- | ---: |
| Combine two coordinate decisions per eight glyphs | 15.173186 → 15.063913 | .939245–1.049399 / inconclusive | 176 → 176 |
| Accumulate across proven batches; reduce before tail | 14.829864 → 14.958962 | .997655–1.019877 / inconclusive, adverse mean | 176 → 176 |

The first stage shares the exact four-header comparison and combines its two
private predicates before one masked extraction. The coherent revision removes
repeated coordinate extraction/branching across the entire eight-header loop,
retaining a vector accumulator and reducing before any tail can report an error.
Structural admission of every extent still precedes its bounded gather. Either
failure returns to the original ordered validator; no public state is exposed.
Empty/zero-contour sentinels, signed coordinates, scalar/cancelable paths, ownership
and admission remain. No storage, cache, pool or API is added.

One current-code two-second profile retains **1,991 leaves / zero lost**:
libc 1,173, glyph-header validation 700, mapping 58. Same-process codegen/map
reconciliation matches all 700 header leaves to instruction boundaries. Counts
identify locations, not phase times, stalls or predicted savings. The two coordinate
decisions are concrete leads; the measured implementations do not confirm a gain.
Code sizes are **1,731 / 1,712 / 1,735 B** for baseline/first/revised. The revision's
JIT folds the three-input OR into `VPTERNLOGD imm=FE`; removed branches do not imply
a faster complete operation. No physical/native/JIT peak or causal timing claim.

Each prototype passes **16,384 ordered oracle cases** per normal, AVX2-disabled
and scalar mode on both OSes, **166 public checks** per normal/scalar mode, and
the original complete actor's metrics, surviving owners, native correctness and
cleanup checks. Existing frozen inputs, full copy/admission/eager-validation/
retain/dispose boundaries, CPU0, natural nonconcurrent GC and idle-v2 guards remain.
All actuals are retained. No confirmation, reference retiming or favorable retry.

There are two isolated Windows core builds and zero VM, canonical, embedded-test,
public-test, actor, harness, reference, fixture or package builds. Reuse **6,790
full/live checks**, the same actual package and three examples each OS from the
unchanged accepted implementation. Per-load allocations and admitted/retained
capacity stay unchanged; loop register lifetime is separate from heap allocation.

Initial codegen assertions counted XMM tail sites or expected an unfused OR.
Explicit width/fused-instruction corrections use the already-captured code and
completed checks; no operation repeats. Recovery-helper quoting corrections occur
before remote execution. First recipes and recorded failures remain in the raw
archive. These are tooling corrections, not reference or correctness concessions.

[Evidence](font-coordinate-reduction-trials-evidence.json) pins prototypes, all
actuals, source proofs and qualification reuse. Mirrored/member-verified
`font-coordinate-reduction-trials.tar.xz`: **1,356,704 B / 109 members**, SHA256
`382147bab5f1e524bdd27b1e4752f814291ade12f67711da76b4890a155a9763`.
Archived `nfa1185-*`, `ehr1186-*` and `dhr1187-*` recipes plus the declared baseline
archive reproduce the round. No current displayed costs or renderer dependencies
come from these trials. Continue independent ordinary-text/copy work; a materially
different evidenced font design remains eligible.
