# Current row-block copy screen

Current vertical-flip and borrowed-copy costs are in [grouped block copying](SNAPSHOT-BLOCK-COPY.md). This report retains its original evidence/history.

RBC1056 is **not retained**. It re-evaluated the checked 256-byte value-copy loop
after accepted BCL cloning removed the old PNG pipeline's helper call. Current
callers are vertical flips and disjoint byte drawing rows; no new path/cache/owner.
All exact samples, tails, clipping, alias directions, cancellation and ownership
checks pass. The changed caller context justified this study, not automatic reuse.

| Complete existing workload | Current baseline / candidate µs | Interpretation |
| --- | ---: | --- |
| Byte vertical flip, isolated pair | 8.535 /8.376 | 1.86% gain; ratio interval 0.977964–0.984907 |
| Overlapping copy pair, isolated pair | 3.811 /3.732 | 2.08% gain; interval 0.974345–0.984171 |
| Overlapping copy pair, standalone | 3.888 /4.593 | Adverse standalone cost; raw intervals separate |

The paired results retain twelve alternating 4,096/8,192-operation batches and
immutable DLLs. Actual standalone reference costs were reconciled for all four
affected controls before deciding. Both flips remain uncertain; separate/overlap
byte drawing remain gaps. The qualified baseline control uses the SAME standalone
harness and resolves the apparent gain as insufficient acceptance evidence.
Execution-context dependence remains unexplained; no alignment/JIT or uniform
causal-regression claim is made. No candidate capture is repeated for a pass.

Current qualified overlapping-copy cost is **3.888 / FFmpeg
3.766 µs**, conservative upper **1.037960**, **0 B/op**:
still a small gap. Two public moves, reverse restoration, six directional native
plane copies, full forward/final canvas checks and every original identity/boundary
remain. The earlier3.850/3.776µs observation remains in Git/raw history. This update
is no production speedup; other source-valid costs are unchanged.

One Linux core/embedded build passes25internal/190public normal/scalar checks.
Exact qualified source/core/embedded snapshots are restored; Windows was unchanged.
Reuse6,721full/live checks perOS and the SAME actual package/all three examples.
No new worker/helper/harness/fixture, Windows, broad or package build. No tests,
private block type or implementation survive the trial. Owned flips retain56B/op,
borrowed copies0B/op and no extra admitted/retained storage. Physical peaks unmeasured.

[Evidence](row-block-copy-current-screen-evidence.json) retains all costs and the
[current overlap owner](borrowed-overlap-current-evidence.json) supplies only the
verified baseline result to the renderer. Raw `tmp/row-block-copy-current-screen.tar.xz`: 2,041,508 B /
277 members, SHA256 `65cee3e691a234f4180ef26ba531a67df3ef9c16c38e75c9866ab0fabdaa2f45`, mirrored/member-verified, with prior dependencies.
The [earlier block trial](VECTOR-BLOCK-VALUE-COPY-SCREEN.md) keeps its PNG adversity.
No permanent block-representation ban; further work needs a concrete revised
design/attribution. All289material paths remain required;publication postponed.


## Compiler-context audit

CGA1059 reuses the exact retained baseline/candidate DLLs and original complete
standalone/isolated harnesses, with zero builds. Each producer's normalized
`CopyRegion` instruction text is identical across those contexts: baseline
**1,655 B / 410 instructions**, candidate **1,420 B / 350 instructions**, FullOpts
without PGO. Normalization removes encoded relocation bytes and process-specific
absolute handles. The complete source/canvas/native/borrowed-owner checks pass.
All instrumented elapsed values are discarded; this supplies no refreshed timing.

The rejected block assignment emits four interleaved load/store pairs using one
vector register and fixed offsets. The current kernel loads four vectors before
storing them. This identifies a concrete revised design: checked block refs with
four explicit vector locals, retaining the current group load/store order while
simplifying index arithmetic. Generated code and complete workloads must establish
whether it helps. No CPU gain, alignment/aliasing cause or automatic adoption follows.
The different timing contexts do **not** contain different generated method bodies;
the earlier contradiction otherwise remains unresolved and the candidate rejected.

Completed standalone captures are reused after a UUID-filename postprocessor fix.
An unsupported isolated-helper screen argument fails before operations; its existing
confirm entry is then used once for code capture. No completed workload is repeated.
Both idle-v2 guards pass. Production, package, allocation and retention are unchanged;
reuse current qualification without broad/package checks. No renderer dependency.

[Evidence](copy-context-codegen-audit-evidence.json) retains disassembly hashes,
instruction comparisons, commands, exact output gates and the failed preflight.
Raw `copy-context-codegen-audit.tar.xz`: 28,404 B/32 members, SHA256
`da07bf9bc59795a22f4fbf5010eaafd2d44678ca279974e96a8b798c8235d91f`, mirrored/member-verified; the dependency retains both immutable subjects.
