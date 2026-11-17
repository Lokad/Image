# Certified crossing-order trials

**Neither implementation is retained.** The exact computed-X certificate is
valid for sorting, but these complete ordinary-RGBA8 operations establish no
useful performance/storage benefit. Qualified production and current costs stay
unchanged; no confirmation, standalone-reference refresh or broad/package repeat.

| Three alternating DLL pairs, 4,096 draws per subject | Baseline → prototype µs | Ratio interval | Public GC B/draw |
| --- | ---: | ---: | ---: |
| Per-band local descriptors | 47.047814 → 52.381420 | 1.064426–1.164568, slower | 83,216 → 83,216 |
| Reuse prepared ranges, sort crossings in place | 47.980224 → 49.176424 | .951638–1.103894, inconclusive | 83,216 → 83,488 |

Strict disjoint computed-X envelopes determine the original midpoint order
without division. Overlapping/touching/unbounded cases retain the exact scalar
coordinate, Double.CompareTo and ordinal tie. Larger active sets retain the
original sorter. Interpolation, endpoints, events, coverage and blending stay
unchanged, with the original 56-byte edge.

The first version reconstructs certificates and copies local descriptors in every
band. Its demonstrated slowdown motivates a coherent storage revision: keep the
already-required range workspace through drawing, size it for all unique shapes,
permute ranges alongside edges and sort the existing crossing array directly.
This removes newly repeated work but yields an adverse point with an inconclusive
interval and greater allocation/lifetime. No causal gain between the two capture
windows is claimed. Stop these implementations without replaying neighbors;
this does not ban a materially different algorithmic design.

Both versions pass **13,200 complete permutation cases per normal/scalar mode**,
including finite/extreme/nonfinite coordinates, signed zero, ties, shuffled input,
counts 0–16 and the larger fallback. Cancellation precedes mutation and allows
recovery. Each OS passes **285 internal/166 public focused checks per mode**,
original full-byte/full-word frames, admission, refusal, cancellation and cleanup.
The prepared-range revision also verifies joint edge/range permutations.

The local descriptor payload is bounded at 512 B; its actual JIT frame is
unmeasured and not production-qualified. Revised ranges are admitted before
allocation and remain charged through their actual users. The authored rectangle's
post-preparation charge rises 5,416 → 5,480 B; its 6,744 B admitted peak is unchanged.
These figures are distinct from total allocations and physical peaks. No
cross-operation retention, cache, pool, public API or numerical policy is added.

Both screens reuse the original public actor, frozen face/text/canvas, stock
reference correctness checks, per-operation reset/full-frame validation outside
two clocks, CPU0, natural nonconcurrent GC and idle-v2 guards. Every actual remains.
The existing timed adapter adds 24 B, separate from public allocation. These are
incremental DLL diagnostics, not ImageSharp or FFmpeg performance comparisons.

Two isolated Windows cores and three isolated embedded-test builds; zero VM,
canonical/public-test, actor/harness/reference/fixture/package builds. A rectangle
charge formula is corrected before tests; only its affected embedded assembly
rebuilds. No completed operation repeats. Reuse **6,790 full/live checks**, the
same actual package and all three examples each OS. Canonical sources/modules
never change. Independent copy work remains available.

[Evidence](text-certified-order-trials-evidence.json) pins stages, all actuals,
ownership and qualification reuse. Mirrored/member-verified raw
`text-certified-order-trials.tar.xz`: **4,358,404 B /155 members**, SHA256
`0f3405d902806845318785f970bc12ee2d0a86c91eabe82de41da92da260a04c`.
Archived `cer1179-*` / `crr1180-*` recipes reproduce both stages. This report
supplies no current displayed costs or renderer dependency.
