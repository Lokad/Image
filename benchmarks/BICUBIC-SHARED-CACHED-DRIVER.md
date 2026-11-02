# Shared cached bicubic execution

One geometry, allocation and row-scheduling driver now serves the small and wide
cached kernels. Original-order repair is shared; distinct numerical certificates,
adjacent-tap reuse, unchanged-width copying, fixed 4 KiB kernels and portable
fallbacks remain. This removes 87 of 653 lines without adding a cache, owner or
public API. Exact managed pixels and admission charges are unchanged.

| Complete resize, 513×259 source | Before → after µs | Current zimg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 → 256×129 | 609.43 → 595.64 | 447.21 | 1.332 / 1.366 | 332,188 |
| RGBA16 → 1027×519 | 3,813.64 → 3,591.82 | 3,026.52 | 1.187 / 1.230 | 5,016,602 |
| RGBA8 → 32×16 | 367.20 → 371.49 | 393.29 | 0.945 / 0.970 | 199,371 |

Enlargement saves 222 µs / 5.82% in raw elapsed, with separated filtered and
all-sample confidence. Normalized intervals overlap; reference elapsed shifts
−1.62%. Small reduction's 2.26% point gain and wide reduction's 1.17% point loss
are inconclusive. Wide parity remains confirmed; the two other gaps remain.
Acceptance combines this measured behavior with simpler shared execution, without
claiming an independently confirmed normalized speedup.

Each baseline and three coherent implementations uses twelve actuals per control,
the existing immutable harness/selectors/references and quiet single-CPU guards.
Before-runs are reused. The first shared vector-view kernel regressed wide elapsed
8.11%; retaining the original tap expressions alone regressed 18.37%.
Both are preserved, unmerged. A one-second diagnostic profile placed 505/693
samples in the extracted horizontal loop and 24/693 in row writing/repair.
Requesting inlining recovered the final measured behavior. Attribution and this
association do not prove a causal cycle count. Instrumented timings are discarded;
there was no repeated sampling of an unchanged candidate to seek a favorable result.

Managed operations include preparation, storage, independent result ownership and
disposal. Native zimg reuses its graph/scratch: these are the existing labeled
boundaries, not equivalent one-shot setup. Managed rational/certified rounding
remains exact; native float intermediates retain the original scoped pixel gates.
No numerical policy or codec packet changes.

Total allocation and admitted capacity remain unchanged; differences of a few
bytes per operation are harness accounting. Operation scratch retained afterward
is zero. Five pre-change frozen admission controls cover small byte/word, odd
mixed padding and wide stack/heap metadata, with charges 12,640 / 25,732 / 26,244 /
13,368 / 84,036 B. They check exact fallback pixels, terminal refusal, cancellation,
untouched canceled output and subsequent use of the borrowed root. These are
admission charges, not physical heap peaks.

Windows/Linux each pass 5,416 full/live checks, affected scalar checks, and the
same actual Release package with three examples. Linux exercises the SIMD-specific
admission controls; Windows lacks AVX512 and exercises qualified portable paths.
Published packages remain immutable; publication is postponed.

The [self-contained evidence](bicubic-shared-cached-driver-evidence.json) retains
all implementations, source/producer pins, comparisons and package checks.
Other displayed bicubic timings are conservatively historical after the changed
dispatch; unchanged non-bicubic paths reuse valid proof. No workload grid was added.
Raw bicubic-shared-cached-driver.tar.xz: 19,321,412 B / 1,639 members, SHA256
193df11fcd7d104e6226e38b1f001577162fef6f442d64448fa5339665b4bba3,
mirrored/member-verified, including original/revised sources, runtimes, adverse
measurements, traces, guards and actual packages.
