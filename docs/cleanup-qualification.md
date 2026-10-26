# 0.2.0 cleanup qualification

This cleanup deliberately breaks unpublished APIs. Published 0.1.0 artifacts stay
unchanged. `MaxWork`, cumulative quotas/counters/reservations, `ImageLimitKind.Work`,
`ImageBudget`, `BudgetedImage` and their wrappers are removed. Direct cancellation
checkpoints replace fuel-coupled polls. Input/output, dimensions, metadata/container
and local memory bounds remain. [Migration](migration.md) describes caller changes.

One admission model remains: the borrowed `ImageLimits.MemoryBudget` root and
explicit owned scopes/reservations. Actual capacity is reserved before allocation,
including replacement overlap; images/fonts/layouts retain charges until disposal.
Async storage remains charged until its actual users finish. Refusal is terminal.
Whole-operation ceiling reservations, weak-table tracking and finalizer leases
are removed without a replacement governor or compatibility layer.

Call-site review established that `Vp8BooleanWriter` and `Vp8ForwardDct` were used
only by tests. Both and their dedicated tests are deleted. Production packet and
primitive tests exercise `Vp8PartitionWriter` and `Vp8ForwardTransform`; frozen
complete packets remain required. All eight orientations use the existing exact
specialized permutations; unreachable general mapping and its helper are removed.

The bounded bicubic review covered 22 files, previously 5,516 lines. Geometry
distinguishes reduction/enlargement, mixed or identity axes and deep support;
depth distinguishes byte and full unsigned word arithmetic. Packed, column,
streaming and row-cache variants cover different support/precision/workspace
conditions; certified SIMD stores retain portable and uncertain-rounding fallbacks.
Earlier measured rounds are linked in the [historical inventory](benchmark-pre-cleanup.md).
Those differences justify keeping the kernels rather than merging their loops.

Ten closed, token-identical rounding/coefficient/store helpers were extracted
into `BicubicMath`, removing 25 duplicate copies. Attributes, arithmetic order,
guards, tap order and rounding are preserved. Dispatch/setup is already shared
where the predicates permit it; this round adds no marginal geometry/ISA variant,
new workspace or performance experiment. Exact frozen and analytical pixels at
both depths, half/tiny-alpha cases, scalar fallbacks and admission/cancellation
tests remain acceptance gates.

Maintenance now separates [current benchmark state](benchmark.md) from the full
historical inventory. Raw observations, reference settings, failed guards, cited
archives and published packages remain retained. Retired quota-only benchmark
entry points and fixture thresholds are removed. The old floating JPEG certificate
is accessible at its historical commit rather than described as current code.
There is no arbitrary checkout-size ceiling; substantial growth uses actual disk
headroom and routine package verification can proceed.

On 2026-10-26, Windows and Linux each pass 4,903 Release checks: 1,921 internal,
2,035 public API and 947 live FFmpeg comparisons. Both systems also pass all
3,956 internal/public checks with hardware intrinsics disabled. Encoder packet
identities, decoded pixels, editing precision and actual memory ownership remain
the gates; obsolete fuel thresholds and prototype-only tests are retired.

Release-package acceptance requires the same actual private 0.2.0 package on
both systems, dependency/API checks, its isolated consumer and all three compiled
usage examples. [Qualification evidence](../benchmarks/cleanup-20261026-evidence.json)
records the actual package/core/archive hashes and verification results; no package
is published by this round. Cited raw logs and the helper-extraction proof are
retained with the source manifest.

A final bounded check removed the copied-pixel accumulator/pass used solely by
the former VP8L cache fuel charge, plus its duplicate cancellation poll. Encoding
decisions, owners and bytes are unchanged. The affected paths are rechecked;
unaffected scalar evidence is reused. The
[final qualification](../benchmarks/cleanup-final-residue-evidence.json) records
the resulting cores/package and supersedes the initial binary identities above.

Timing and allocation confirmations from changed callers remain historical; this
cleanup makes no CPU-speed claim. Full warm parity and replacement readiness remain
open, with the finite required paths in [coverage](../benchmarks/WARM-COVERAGE.md).
