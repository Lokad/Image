# Matched-stride row preparation trial

The tested implementation is **not promoted**. Neither original complete
byte-copy control establishes a useful gain; no confirmation or broad/package
qualification follows.

| Existing complete operation /E1 baseline →private candidate | Baseline →candidate µs | Paired ratio interval | GC B/op |
| --- | ---: | ---: | ---: |
| Separate RGBA8, 256×129 | 1.967859 →1.990797 | 0.856013–1.195522 /inconclusive | 0 →0 |
| Original clipped RGBA8, 254×129 effective | 1.956969 →1.958388 | 0.964761–1.038033 /inconclusive | 0 →0 |

These are paired managed-DLL screens, **not FFmpeg parity measurements**. Each
retains all three alternating 65,536-operation pairs, complete fixed warmups,
CPU0, natural nonconcurrent GC and idle-v2 guards. The original compiled public
harness, input/output hashes, native readiness, exact full source/canvas, hidden
RGB, alpha, untouched pixels and owner checks remain. Setup, native smoke checks,
I/O, IPC and final validation stay outside the two batch timestamps. No clocks
from instrumented code-generation probes contribute to these results.

The hypothesis applies the measured self-row state's principle to two separate
backings with equal positive packed strides. Two rectangle views are bounded once;
one row cursor/step replaces two offsets/steps, removing three scalar arguments.
The extent `(height-1)*stride+rowBytes` fits both validated frames and Int32.
Existing four-load-before-store groups, tail bounds, row cancellation and final
cursor rule remain verbatim under substitution. The general, separate-word,
portable and self-row kernels remain untouched. No API, pin, cache, owned capacity,
shared-budget allowance or retained workspace is added.

Both OSes pass **10 internal/190 public** checks per normal, AVX-512-disabled and
scalar mode. New direct checks cover 775 lengths, three row counts, exact bytes,
padding/canaries, cancellation and a last row with an Int32-sized step. The actual
optimized matched-stride method runs: its body is **371 B**, versus the original
438 B; the public caller grows **1,531→1,646 B**. Code size and source-state removal
do not assign causal time savings. The complete screens do not justify the extra
path or neighboring kernel/parameter trials.

Preparation initially lacks a reference-metadata file, then excludes benchmark
assemblies along with core files. Both failures precede workers/samples. Restore
only original hash-verified modules and minimal fixture pins; no core, test or
actor rebuild or completed measurement repeat. The initial guard/empty wire,
failures and corrections remain. Archive postprocessing then needs the unchanged
private project file; supplying it repeats only archival work.

Keep the qualified E1 library/private package and its 6,819 checks per OS/SAME
package/all three examples unchanged. Focused prototype checks are not full
qualification. The new actor builds once, reusing the existing public benchmark
assembly. Review matched standalone byte/reference boundaries before further
kernel work: earlier standalone and current private-baseline clocks differ, which
does not establish a historical speedup. Use the prepared interleaved driver and
original qualified artifact for a bounded reconciliation, preserving every gate.

[Evidence](matched-stride-row-trial-evidence.json) pins sources, bounds proof,
actuals, code generation and qualification scope. Mirrored/member-verified
`matched-stride-row-trial.tar.xz`: **1,928,232 B /80 members**, SHA256
`0df094564818f4af1fa704941f8e2e41a7f20f7be72162564948aa739a3b0722`.
Archived `msr1224-*` recipes and declared dependencies reproduce it. No current-cost
renderer dependency or new required workload is added; all 289 paths remain in scope.
