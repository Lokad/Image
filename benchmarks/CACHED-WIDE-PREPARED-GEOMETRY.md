# Prepared wider-cache geometry

CGP706 proves contiguous horizontal windows once while preparing existing
operation-owned geometry. A negative marker occupies dead padding only for equal,
positive pair counts below capacity. Actual clamped indexes remain nonnegative;
full-capacity, unequal, irregular and odd-tail cases keep the original indexed
path. Weights, live offsets, counts, normalization, ordered arithmetic and exact
repair remain unchanged. No array, owner, reservation, pool, session or API is added.

Complete RGBA8 513×259→128×64 saves **19.33 µs / 4.15% raw / 3.32%
reference-normalized**, 465.40→446.07 µs. Filtered/all-actual raw and conservative
normalized confidence separate; all-actual saving is19.57µs/4.20%.

| Existing complete bicubic control | Lokad µs | zimg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| RGBA8 → 128×64 | 446.07 | 346.46 | 1.288 / 1.292 | 198,450 | gap |
| RGBA16 → 128×64 | 434.58 | 447.09 | 0.972 / 0.987 | 198,442 | pass |
| RGBA8 → 64×32 | 371.95 | 402.88 | 0.923 / 0.930 | 197,683 | pass |
| RGBA16 → 64×32 | 396.57 | 434.82 | 0.912 / 0.921 | 197,677 | pass |
| RGBA8 → 32×16 | 332.79 | 412.11 | 0.808 / 0.817 | 197,298 | pass |
| RGBA16 → 32×16 | 329.91 | 455.65 | 0.724 / 0.729 | 197,294 | pass |
| RGBA8 → 16×8 | 319.68 | 498.61 | 0.641 / 0.646 | 201,203 | pass |
| RGBA16 → 16×8 | 314.95 | 545.98 | 0.577 / 0.580 | 201,200 | pass |

All eight affected controls refresh on the actual-package DLL: **seven pass /
one gap / zero uncertain**. Both 64×32 uncertainties now pass and stop. The byte
128×64 control remains a gap; no complete parity or causal companion gains are
claimed. These are the existing support32/64/128/256 byte/word controls, without
new images or a settings grid.

The reversed independent pair first gives464.10→446.07µs, raw3.89% with separated
confidence but normalized2.14% with overlapping bounds. Its original first native
bracket has363.75/366.61/370.59µs samples before settling near352µs; after is stable.
One targeted original-only twelve-actual recheck resolves that observed transient,
using the unchanged build, settings, cadence and both brackets. The completed
candidate is reused. Every earlier sample, bracket and adverse point is retained;
there is no exclusion, preconditioning change, candidate repeat or further recheck.

The [earlier window study](CACHED-WIDE-WINDOW-SCHEDULING-EXPERIMENT.md) checked
eligibility inside every row and supplied no useful robust lead. This revision
prepares that proof once, seeking its indexing benefit with less repeated setup.
Current production previously used indexed offsets, not repeated eligibility.
Actual generated code has no per-tap offset loads/range branches in the prepared
loop, while the original fallback keeps both checks. Required window bounds stay
outside the loop. Parent code is8,349B plus a431B preparation helper; fewer checks
or code-size differences alone establish no CPU gain.

Nineteen actual-kernel/preparation controls cover ordered double results across
clamped/odd/full/unequal/zero layouts, signed-marker collision, live-padding/count
aliasing, invalid source windows, zero allocation/admission and cancellation.
Test helpers live only in the source-embedding test assembly. Windows/Linux each
pass **6,096 full/live checks**,265internal/729public per normal/scalar/AVX2-disabled/
AVX512-disabled mode, and the SAME actual private Release package/all three examples.
Existing package consumers exercise128×64 byte/word exact frames, aggregate shared
admission, disposal and independent survivors; no new consumer inputs are needed.

Primary total allocation remains198,450B/op; source-valid admitted workspace is
198,432B and retained result32,768B until disposal. Retained operation scratch is
zero. Existing reservations, array capacities, ownership and release boundaries
are unchanged; no new physical/native peak is measured. Shared refusal remains
terminal, without retry or lower-memory fallback.

Managed one-shot geometry, workspace, computation, independent output and disposal
remain timed on the quiet VM, sequential Release/one CPU. Native zimg graph/scratch
remain prepared and labeled; no fresh-context parity is claimed. Exact managed
pixels and original native bounds remain distinct. No Q30/rational/certified-rounding
policy, encoder packet, codec, metadata or unrelated editing contract changes.

One main/embedded build per OS serves the round; no harness, selector, native or
fixture rebuild. [Shared pixel preparation](CACHED-WIDE-PIXEL-PREPARATION.md)
retains preceding gains/history, not current costs. Its old execution profile
predates this loop change and is a historical lead, not current phase-time proof.

[Evidence](cached-wide-prepared-geometry-evidence.json) pins all costs, proofs,
both original comparisons, runtime providers, raw jobs and same-package checks.
Mirrored/member-verified raw `cached-wide-prepared-geometry-qualified.tar.xz`:
6,336,552B /204members, SHA256
`94d022a05f3bc8cd604b85c87022840ced7c09b79de2c8651a76bd08cd9d6fbf`.
Private package439,149B, SHA256
`34023b9147fd5cf8537426d433d6f7a37d65d92854de5b417dc9aee882d8b8c3`.
Other unchanged qualification is reused. Publication is postponed; the full283-path
objective remains open, and this is not a full release-performance refresh.
