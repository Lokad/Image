# Owned-flip reference reconciliation

Superseded by [prepared vertical-row execution](VERTICAL-PREPARED-ROWS.md),
which closes both controls on its final timed/package artifact. The scheduling
correction and original observations below remain reproducible history.

Both original owned flips remain **uncertain**. Their means are close; the full
conservative confidence gate does not establish parity. Production is unchanged,
and no identical favorable retry follows.

| Complete original 513×259 operation /FFmpeg owned output | Managed /reference µs | Conservative ratio interval | GC B/op |
| --- | ---: | --- | ---: |
| RGBA8 vertical flip | 9.106841 /9.136850 | 0.983016–1.006960 /uncertain | 56 |
| RGBA16 vertical flip | 18.645503 /18.650090 | 0.994222–1.004353 /uncertain | 56 |

The preceding shared-storage byte capture has native group means of
9.033 /8.616 /8.330 µs, while managed samples also change during the paired phase.
Both sides already have four complete warmups. Do not assign a host cause, call
this cold startup, subtract historical clocks or assume a kernel needs tuning.
The explicit tooling correction distributes the same 36 native actuals around
twelve alternating pairs: one before and after each pair. All groups span the run.
The 16,384-operation cadence, warmups, timed bodies and conservative all-actual,
upper-outlier-diagnostic and paired-log statistics remain unchanged.

Managed public validation, admission, cancellation, independent output
materialization and disposal stay timed. FFmpeg retains its graph/source/frame
wrappers and materializes the negative-stride view into separate owned output,
including allocation/copy/release. Source preparation, checks, IPC and JIT remain
outside. Original input/output hashes, full samples, alpha, hidden RGB, unchanged
source, native CLI/scalar pixels and independently surviving outputs pass.
CPU0, one processor, optimized Release JIT, natural nonconcurrent GC and idle-v2
guards remain. Only untimed temporary image files use bounded tmpfs.

The timed DLL is the current immutable E1 actual-package artifact. Source proof
reuses the shared pool from the preceding flip producer and verifies the called
single-row copy body unchanged; later rectangle-only helpers are irrelevant.
Reuse 6,819 full/live checks per OS/SAME package/all three examples. One small
driver build changes its harness guard/description; no production, embedded,
public, harness, native, fixture or package rebuild or broad qualification repeat.
The exact driver patch, earlier samples and every current actual remain.

Allocation is 56 B/op; source/result payload is 531,468 B per RGBA8 frame or
1,062,936 B per RGBA16 frame. No new workspace or retained operation scratch.
The four-array/4 MiB shared idle pool and caller-root bypass remain unchanged.
Allocation, live ownership/admission and idle retention are distinct; physical,
native and JIT peaks are unmeasured. No new required workload or wider copy claim.

[Evidence](owned-flip-interleaved-reference-evidence.json) pins current costs,
qualified package, source proof, exact patch, checks and reproducibility.
Mirrored/member-verified `owned-flip-interleaved-reference.tar.xz`:
**62,548 B /38 members**, SHA256
`18a32f758b5193b8bcb37802436cdda3aa073d3051eed34a70ca9d1e627db66f`.
Archived `ofr1227-*` recipes and declared dependencies reproduce the round.
