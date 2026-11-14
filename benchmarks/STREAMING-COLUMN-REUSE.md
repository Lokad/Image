# Streaming column reuse

Bounded 513×259 → 16×8 RGBA16 bicubic reduction passes the existing complete-operation
comparison: **336.93 / zimg 495.11 µs**, conservative interval
**0.675076–0.685485**, **112 B/op**. Managed pixels remain exact.
Adjacent columns share overlapping pixel unpack/premultiplication/widening and
vertical preparation. One typed driver preserves the single-column path.

| Existing bounded output | Lokad.Image µs | Prepared zimg µs | Upper ratio | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| 16×8 RGBA16 | 336.93 | 495.11 | 0.685485 | 112 |
| 16×8 RGBA8 | 405.41 | 452.74 | 0.902217 | 112 |
| 8×4 RGBA16 | 481.19 | 692.89 | 0.696535 | 112 |
| 8×4 RGBA8 | 374.51 | 653.44 | 0.578342 | 112 |
| 8×17 RGBA16 | 492.98 | 536.35 | 0.922823 | 112 |
| 8×17 RGBA8 | 392.57 | 477.85 | 0.830831 | 112 |

All six controls use the frozen 513×259 input. Each retains twelve alternating
managed/native pairs of 256 complete operations,
twelve native batches before and after, natural GC and versioned idle guards.
The target has an independent confirmation after its short screen; the other
five qualify the affected single-column byte/word paths. All six pass and stop.
Managed preparation, admission, filtering/repair, independent output and disposal
remain timed. Native retains its zimg graph/input/scratch; this asymmetric
diagnostic is distinct from the already-passing equivalent one-shot/prepared
controls. Exact rational managed samples and approved native-only maximum 1 / RMSE .25
gates, input immutability and surviving-output ownership all pass.

The separately measured prior producer is 494.87 µs; the descriptive
reduction is 31.91%, not a same-process paired causal
confidence interval. Its native parity remains uncertain and is not promoted.
The first implementation was adverse at 676.52 µs. Generated code showed each
intermediate add stored/reloaded through potentially aliased output references.
Local accumulators remove that traffic without changing addition order. The raw
archive retains both implementations, every sample and a failed manifest preflight
that produced no timed samples. The current source is the revised implementation.

Workspace admission remains 4,096 B; explicit single/pair arrays are 4,048/4,096 B.
The target owns 1,024 B output while live; source 1,062,936 B is borrowed. Per-operation
owned capacity is 5,120 B, separate from 112 B GC allocation. Operation workspace has
zero retention after return. The existing two-array / 4 MiB idle
ceiling is unchanged and caller-root storage bypasses it. Physical/native/JIT
peaks are unmeasured. Arithmetic, certificates, repair, cancellation and terminal
admission refusal remain; no numerical-policy change.

An authored 521×263 → 17×9 RGBA16 control checks odd column tails, edges, exact rational
pixels, borrowed source, two independent survivors, refusal/cancellation/recovery.
The primitive compares ordered full-word sums bitwise, including overlap parity,
negative weights, clamped samples and high unsigned premultiplied products.
Both OSes pass 6,702 full/live checks (3,264 internal / 2,490 public / 948 live), plus
45 internal / 119 public normal/scalar/AVX512-disabled checks, explicitly including
all three required resize-storage suites. The SAME actual Release package and
all three usage examples pass both OSes, including the frozen 16×8 RGBA16 paired
path with shared refusal, cancellation and independent survivors.

Two Linux core/three embedded/two public builds include the adverse candidate
and a test-only scalar-peak assertion correction. Windows builds each once.
No harness/native/helper/fixture builds, one NoBuild pack, zero producer rebuilds
after accepted timing. Unchanged codecs, text, Lanczos and other resize paths
reuse source-valid evidence. No broader workload grid or release-wide refresh.

Current resize scope: 72 passes / one uncertainty across 73 views. Across 191 image
views: 188 passes / one gap / two uncertainties. Borrowed drawing and inspection
gaps remain separate; all 289 material paths remain required. Publication is postponed.

[Evidence](streaming-column-reuse-evidence.json) retains identities and observations.
Timed DLL 2fe8b8482857de6984d4b576f329fa430bf0d7a490460c9608bea433f2b56b8a; actual private package 9d2963881665724768d7d1a37375649bcd3cd7f0efdf397756095ad313f30a5b.
Raw streaming-column-reuse-qualified.tar.xz, 8,236,948 B / 253 members,
SHA256 9ef2ee24cf0c5324d0a60eea89c9d7de4dc18a421220a5a6e027335b19301e37, mirrored and member-verified.
Earlier [bounded-word uncertainty](BICUBIC-CURRENT-ADJACENT-CONFIDENCE.md)
is superseded only by the affected new production path; its raw evidence remains.
