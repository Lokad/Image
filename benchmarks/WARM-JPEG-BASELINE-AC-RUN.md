# JPEG baseline AC runs

Historical ≤2x checkpoint. Its live-token baseline444 timing is superseded by
[bounded AC cache reuse](WARM-JPEG-CANCELABLE-AC-CACHE.md), a current qualified
short gap under the ≤1x objective. Other selected paths retain their applicable
evidence in the [current summary](../docs/benchmark.md).

Ordinary native-input JPEG444 decoding now independently confirms **1.715× FFmpeg /
upper 1.733×**, reducing managed time **18.6%** from the prior independent run.
The same qualified build also confirms the existing live-token and progressive
boundaries. Registered managed pixels and native agreement bounds are unchanged.

| Existing fixed workload / stage | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Ordinary, original short | 3,444.46 | 1,723.94 | 1.998 / 3.064 | 2,664,358 |
| Ordinary, original independent twelve | 3,626.92 | 1,824.30 | 1.988 / 2.041 | 2,664,358 |
| Ordinary, new short | 3,136.05 | 1,831.37 | 1.712 / 2.081 | 2,664,358 |
| Ordinary, new independent twelve | 2,953.89 | 1,722.64 | 1.715 / 1.733 | 2,664,373 |
| Live token, regression screen | 3,034.26 | 1,722.52 | 1.762 / 2.755 | 2,664,373 |
| Live token, independent twelve | 3,048.96 | 1,722.47 | 1.770 / 1.797 | 2,664,373 |
| Progressive, regression screen | 6,415.27 | 3,325.14 | 1.929 / 2.065 | 2,682,364 |
| Progressive, independent twelve | 6,515.45 | 3,334.14 | 1.954 / 1.960 | 2,682,364 |

One six-second profile records 5,580 samples: **50.9%** in fused AC symbol reading
and **13.7%** in baseline block traversal. Noncancelable blocks now decode runs of
ordinary short AC symbols with local reader state, coefficient position and block
budget, using safe two-byte lookahead. Unused whole bytes are returned before
publishing the logical position and pending bits. Completed state is published
on every exit. Stuffing, markers, long/incomplete codes, invalid syntax and tight
budgets retain the original fallback. The existing atomic fused prefix/amplitude
charge, partial blocks and failure precedence are preserved. Cancelable and
progressive traversal keep their existing policies. No table, heap workspace,
unsafe code, dependency or upstream implementation is added.

The original 513×259 byte raster, frozen 133,041-byte native JPEG444 packet and
its existing lossless progressive rewrite are reused. Only three existing workload
types are measured; no image, sampling, quality or scan grid is introduced.
Both subjects time complete resident-input decoding, color conversion, independent
owned output acquisition and release. Native codec/conversion contexts and packet/
frame wrappers are reused, including per-operation state reset and all pixel work.
No output is cached. Live-token setup is excluded and the token is never signaled.
Startup, I/O, IPC, fixture creation and validation are excluded. Native allocations
are **unmeasured**; managed figures include diagnostic/harness overhead.

Every registered managed output byte remains frozen. Native normal/scalar RGB
maximum is **3**, RMSE **0.227**, within the unchanged maximum4/RMSE1.5 gate;
opaque alpha, geometry, source and independent owners pass. Four additional
controls extend every-byte-pair and long-code checks to noncancelable scopes,
and compare a consecutive run against independent scalar traversal at every
budget, truncation and cached phase. All **34 focused checks** pass with normal/
scalar code on each OS, covering work, error precedence, reader and partial blocks.

Sequential Release runs use one pinned CPU, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Every quiet guard passes.
All twelve actuals are retained for every confirmation; BDN selects ten ordinary,
eleven live-token and twelve progressive actuals. The upper includes selected and
all-actual intervals against the faster native bracket's lower bound. Original
above-two bounds remain retained at their original source.

Both regression means pass before full **3,432 Release tests per OS**, including
**898 live comparisons**. The same actual Release package passes its consumer and
all three examples on Windows and Linux. Production/harness binaries remain frozen
through accepted measurements; the ordinary confirmation driver is reused without
rebuilding. The only source change between measurement and qualification is the
SPEC description, updated before packaging. Production inputs are identical.
Full qualification is performed once; the measured harness is restored from a
verified child afterward without rebuilding.

[Numeric evidence](warm-jpeg-ac-run-evidence.json) retains every actual, allocation,
guard, profile, source/assembly/job/native identity, rejected bound and full package
qualification. Stop all three confirmed pairs; full required warm coverage stays open.
