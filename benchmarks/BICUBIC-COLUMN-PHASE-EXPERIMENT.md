# Mixed-word column phase: unselected

BRM539/BRO540 separates paired-column decoding and vertical filtering from
horizontal accumulation and certified publication. Both coherent stages preserve
exact pixels but have unpromising complete-operation screens. Qualified sources,
main/embedded modules and public bindings are restored byte for byte on both
OSes. The actual package and all 125 displayed costs remain unchanged.

| Three-actual Mixed16 stage | Lokad µs | zimg reference µs | Main code / reported local frame B | Helper code / frame B |
| --- | ---: | ---: | ---: | ---: |
| A: helper-local decoded scratch | 1,058.36 | 848.05 | 7,498 / 984 | 804 / 40 |
| B: borrowed operation-owned scratch | 1,039.18 | 850.40 | 7,565 / 920 | 486 / 24 |

The source-valid current cost remains 1,016.78 µs, with an 8,262-byte main routine
and 872-byte reported local frame. GC remains 114 B/op. These short screens
establish no causal gain, regression or equivalence. No independent confirmation,
favorable repeat, workload expansion or broad/package qualification follows.
Smaller code and different scratch initialization alone do not establish speed.

A gives the helper a bounded 1,536-byte decoded array, with 32 bytes of caller
publication scratch. Actual generated code clears that array on every column
cache fill. This repeated work justifies B: the helper borrows the original
operation-owned decoded buffer, overwrites every active slot before filtering,
and returns before publication reuses its prefix. The next fill replaces those
slots before reading them. B removes per-fill allocation/clearing and the extra
caller buffer while retaining one actual column phase. Its call and metadata
costs still need complete-operation evidence; this implementation is unselected,
without a permanent ban on execution-phase designs.

Explicit overlapping arrays are 4,024 bytes for A and the original 3,992 bytes
for B, within the existing 4,096-byte workspace reservation. No heap capacity,
owner, pool, cache, session or API is added. Scratch retained after the operation
is zero. These source capacities and JIT frame sizes are not additive physical
peak measurements; admitted/physical/native peaks were not remeasured.

Full-source projections reconstruct the original outside the phase/scratch
handoff. Decode, vertical arithmetic, cache IDs/prefetch, geometry, tap order,
quotients, certificates, repair, packed output, every poll and admission/cleanup
remain exact. Both stages pass **58 internal / 688 public checks per normal/scalar
mode on each OS**. Existing coverage includes frozen hashes, width one, cache
wrap, odd bands/tails, tiny alpha, rational pixels, 4,095/4,096-byte admission,
terminal shared refusal, cancellation, recovery and independent survivors.

One immutable core/embedded build per stage/OS serves all checks. No tests,
public assemblies, harnesses, native workers, selectors or fixtures are built.
An A diagnostic's method-name filter selected 40 passing existing column checks;
its observer expected two and failed afterward. Completed logs/JIT are recovered
without rerunning. B's fully qualified filter selects the intended two checks.

The frozen complete 513×259 → 1027×129 RGBA16 public resize retains preparation,
filtering, independent output and disposal. Native zimg graphs/scratch remain
prepared as declared; source deinterleaving, halo, filtering and output packing/
ownership stay timed. Exact rational managed pixels, native float bounds and all
source/settings/ownership/idle-v2 gates pass. No numerical-policy change occurs.

[Evidence](bicubic-column-phase-experiment-evidence.json) pins both stages,
actuals, projections, generated code, observer recovery and exact restoration.
Raw `bicubic-column-phase-experiment.tar.xz`, SHA256
`c73f2a1ed83d4ed9abbe313b4f5bca4e0313458ab6add994a3574c18cf200623`,
13,783,172 bytes / 249 members, is mirrored/member-verified with qualified and
current-attribution providers. No renderer dependency is added. Exact restoration
reuses 5,875-check and same-actual-package qualification without rebuilding.
Next audit the independent WebP alpha path before more coefficient-loop variants.
Publication remains postponed; the full 283-path objective stays open.
