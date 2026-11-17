# Byte-copy reference reconciliation

Clipped-byte costs below are superseded by [partitioned actuals](COPY-PARTITIONED-REFERENCE.md),
which resolve its uncertainty as a gap. The separate-byte capture remains current;
all original samples and uncertainty stay retained.

Two existing byte controls use the same qualified immutable DLL and frozen
FFmpeg plane-copy reference, with explicitly interleaved reference groups.
Production is unchanged. These are current complete-operation costs, not a gain
over historical clocks or a private managed-DLL comparison.

| Existing public operation /FFmpeg public plane copy | Managed /reference µs | Conservative ratio interval | GC B/op |
| --- | ---: | --- | ---: |
| Separate RGBA8, 256×129 | 1.986809 /1.914611 | 1.030550–1.043571 /gap | 0 |
| Original clipped RGBA8, 254×129 effective | 2.097832 /1.998819 | 0.982591–1.121784 /uncertain | 0 |

Keep the demonstrated separate-byte gap and the clipped uncertainty distinct.
Neither a private baseline near 1.96 µs nor the old standalone measurements prove
parity, a regression or a historical speedup. No favorable replay, sample removal,
fixture change or confidence-gate relaxation is used. Passing self-copy paths stop;
the separate-word result remains [uncertain](WARM-CADENCE-AND-BRACKETS.md).

Reuse the [interleaved driver](WARM-CADENCE-AND-BRACKETS.md), including its explicit
archived scheduling patch. Each case retains twelve alternating pairs, three
groups of twelve native actuals, 65,536 operations per actual, four complete
warmups on both sides and the unchanged conservative all-actual, outlier-diagnostic
and paired-log statistics. One reference sample before and after each pair
distributes every group across the run. CPU0, one processor, Release optimized
JIT, natural nonconcurrent GC and all idle-v2 guards pass.

Both sides borrow the original resident 513×259 source/canvas; source preparation,
IPC, startup, checks and disposal remain outside. Idempotent copies require no
per-operation reset or output allocation. Managed public validation, clipping,
cancellation and cleanup remain timed; native public plane arguments are
prevalidated. Exact full source/canvas, hidden RGB, alpha, untouched pixels,
caller ownership and every original reference setting remain unchanged. There is
no new admitted or retained capacity; caller pixels and unmeasured native/JIT/
physical peaks remain separate from the zero managed allocations.

The timed DLL is `0707fa23…`, the actual qualified SRS package artifact
`d7cce226…`. Relevant production paths remain unchanged. Reuse its 6,805 full/live
checks per OS, focused normal/AVX-512-disabled/scalar checks, SAME actual package
and all three examples. The current E1 library/package is untouched. **Zero builds
and zero new broad/package checks** are needed for this measurement-only round.

[Evidence](byte-copy-reference-reconciliation-evidence.json) retains identities,
every actual, source proof, receipts and statistics. Mirrored/member-verified
`byte-copy-reference-reconciliation.tar.xz`: **44,932 B /25 members**, SHA256
`4f6e89097c2f9c68b7887bc75c26a771e94f0586e0d8e5ecc0cb87a3faa3b0f1`.
Archived `brr1225-*` recipes and declared dependencies reproduce the round.
No new workload is added; the full 289-path objective and final 165-comparison
immutable-package refresh remain open.
