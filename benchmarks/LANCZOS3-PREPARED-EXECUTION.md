# Lanczos3 preparation and execution

`Lanczos3ResizePlan` explicitly owns axes and reusable row/sum storage. One-shot
and prepared calls share allocation and execution code; specialized exact/FMA
kernels and certified repair remain unchanged. Each call resets row identities,
reads current source pixels and independently owns its result. No image cache or
cross-operation pool is added. Creation limits and the borrowed root apply to
every call; refusal is terminal. Results survive plan disposal.

The quiet-VM comparison now uses equivalent boundaries on both sides:

| 513×259 → 256×129 | Lokad µs | zimg µs | Ratio / upper | Allocated B/op | Managed retained B |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8, complete one-shot | 773.75 | 1,680.57 | 0.460 / 0.473 | 138,619 | 0 |
| RGBA8, prepared execution | 641.49 | 517.81 | 1.239 / 1.268 | 139 | 139,800 |
| RGBA16, complete one-shot | 787.27 | 1,919.47 | 0.410 / 0.418 | 138,611 | 0 |
| RGBA16, prepared execution | 654.48 | 559.68 | 1.169 / 1.222 | 139 | 139,800 |

Each has twelve BenchmarkDotNet actuals, twelve native samples on either side,
source/producer identities, exact managed output checks and passing quiet guards.
Ratios use the faster bracket; upper bounds retain filtered/all-sample confidence.
These are current development measurements, not a full release refresh or a
claim that a changed reference boundary accelerated the managed kernel.

One-shot includes geometry, working storage, independent output and disposal on
both sides. Repeated use excludes explicit preparation on both sides. Native
zimg retains its graph, input planes and 367,168-byte scratch; graph/input-plane
retention and native allocations are unmeasured. Its one-shot adapter creates
and frees those resources inside each operation. Both modes time input/halo
packing, normalization, filtering and output ownership. No startup/IPC/I/O is timed.
Managed allocation is distinct from retained capacity and admitted live storage.
The new plan reports its coarse capacity through `StorageBytes` and keeps it
charged until disposal; independent images remain charged to their own owners.

Managed pixels preserve exact Q30/rational results and certified rounding.
The native float path retains the existing case-specific max-error/RMSE gates.
All four frozen reduction/enlargement controls at both depths pass in normal
and scalar modes. Focused tests also cover identity/hidden RGB, changed source
pixels, shape/depth mismatch, orientation, preparation/refusal/cancellation cleanup,
reentrant execution/active disposal and independent surviving outputs.

Windows/Linux each pass 5,400 full/live checks and 83 affected checks in both
normal and scalar modes. The same actual Release package and all three usage
examples pass on both. Its consumer additionally encodes surviving plan outputs
asynchronously after source/plan disposal. Published packages stay unchanged.

Prepared reductions still have confirmed CPU gaps; earlier enlargement costs
remain historical until refreshed. Bicubic scheduling/preparation consolidation
is separate work. [Initial attribution](PREPARATION-WORKSPACE-ATTRIBUTION.md)
retains the former asymmetric boundaries; no numerical contract was relaxed.

[Evidence](lanczos3-prepared-execution-evidence.json) records actuals, scopes,
identities and qualification. Raw lanczos3-prepared-execution.tar.xz:
8,621,576 B / 175 members,
SHA256 72918e61a1584f3494a384cb7cef8effe8cb001d5f9c35aa019c1b3627f529d6,
mirrored/member-verified. It preserves source/producers, the reused native adapter,
replay scripts, complete measurements/guards and old/new package artifacts.
