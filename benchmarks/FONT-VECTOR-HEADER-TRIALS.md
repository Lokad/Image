# Bounded vector font-header trials

**Neither prototype is retained.** Both preserve exact behavior but establish no
useful complete-operation gain. Qualified production, the scoped FreeType loading
gap and passing complete public text integrations remain unchanged.

| Three reversed pairs, 16,384 owned loads per subject | Baseline → prototype µs | Ratio interval / result | GC B/load |
| --- | ---: | --- | ---: |
| Four bounded 16-byte headers; vector contour/bounds predicates | 14.981469 → 15.248651 | .991236–1.045146 / inconclusive, adverse mean | 176 → 176 |
| Direct vector constant and one upper-half insertion | 14.984657 → 15.252536 | .951195–1.089221 / inconclusive, adverse mean | 176 → 176 |

An untimed scan of the frozen DejaVu Sans face finds 765/781 eight-record blocks
eligible for 16-byte loads. Each record extent is proven before its load; the six
extra outline bytes stay inside that record. Vector predicates preserve signed
bounds and zero-contour handling. Empty/short records, portable and cancelable
paths, ordered failure fallback, copied input and ownership remain unchanged.
No storage, cache, pool or public API is added.

The first generated kernel constructs vector operands and constants repeatedly.
The coherent revision removes that overhead; its captured code verifies the
change. The retained qualified kernel is 1,731 B on its identified earlier DLL;
the current candidates are 2,281 / 2,215 B. The earlier size is not a new
machine-code capture of the current baseline DLL.
Coverage, fewer scalar branches and smaller code do not establish a CPU gain.
Both completed screens remain, without confirmation or favorable replay.

Each version passes **292 internal checks** per normal, AVX-512-disabled and
scalar mode, **166 public checks** per normal/scalar mode on both OSes, and
**16,384 ordered oracle cases** in each of the three modes. Nine added controls
cover exact record ends, lengths 10–20, zero contours, signed extremes, first-error
order, independent owners, refusal, borrowed-root release and recovery. The
feature-gated direct predicate checks 269,347 cases; Linux codegen confirms the
AVX-512 kernel executes. Original complete-actor metrics, retained owners, native
correctness and cleanup checks pass. No lossless or editing contract changes.

Full public copy/admission/eager-validation/glyph-count/dispose boundaries, frozen
inputs, CPU0, natural nonconcurrent GC and idle-v2 guards remain. These are paired
DLL diagnostics, not new FreeType measurements or overall text-processing costs.
Allocated, admitted and retained capacities are unchanged; physical/native/JIT
peaks remain unmeasured. Two isolated Windows core/test builds, zero VM,
canonical, public-test, harness, reference, fixture or package builds. Reuse the
unchanged **6,794 full/live checks**, same actual package and three examples each OS.

[Evidence](font-vector-header-trials-evidence.json) retains all actuals and pins
sources, artifacts, qualification and dependencies. Mirrored/member-verified raw
`font-vector-header-trials.tar.xz`: **3,593,928 B / 109 members**, SHA256
`710f15b7d28fb50ccaca5427b96e63e2f8a25d6c6cba4ff08b34d8c20f3cd396`.
Archived `fvg1194-*`, `fhg1195-*` and `fhc1196-*` recipes reproduce the stages.
No current displayed costs or renderer dependencies come from these trials.
