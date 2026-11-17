# Prepared copy-vector tail trials

**Rejected; qualified sources, DLLs and package restored.** The fixed 1–4-vector
tail revision improves a private DLL but regresses in the final build. No current
cost, passing status or production implementation changes.

| Bounded comparison | Baseline / candidate µs | Paired ratio interval | Decision |
| --- | --- | --- | --- |
| CVT1236 equality-guard screen, 3 pairs | 1.961394 /1.987923 | 1.010347–1.016715 | Adverse; no confirmation |
| CVU1237 unsigned-guard screen, 3 pairs | 1.985922 /1.954644 | .952479–1.017082 | Promising point; uncertain |
| Independent private confirmation, 12 pairs | 2.103618 /2.061848 | .961811–.998421 | Scoped private gain |
| Final-DLL transfer, 12 pairs | 2.117935 /2.154096 | 1.009814–1.024389 | Adverse; restore |

The revision replaces two prepared-row tail loops with one bounded helper.
Bulk accesses, exact tail addresses/order, row scheduling, checkpoints, portable
kernels, ownership and admission remain. Unsigned guards remove two remaining
bounds checks; selected code shrinks from507 to495bytes. The final and private
495-byte instruction text is identical, as are source and non-core runtime files.
No causal explanation for the contradictory elapsed results is established.
Code identity and private gains cannot override the final complete operation.

Every sample uses the original complete public separate-byte CopyRegion,
original source/canvas and full exact pixel/alpha/hidden-RGB/untouched/source/owner
checks. Each actual and warmup has65,536 operations; reversed alternating pairs,
CPU0, optimized JIT, natural nonconcurrent GC and idle-v2 guards remain.
All copies allocate0B; no new workspace or retention. Physical/native/JIT peaks
are unmeasured. Instrumented codegen clocks are discarded, with no sample commands.

Both private revisions and the final build pass10internal/190public checks in
normal, AVX512-off and scalar modes on each OS, including existing775-length
two-direction snapshots. Two isolated Windows core/embedded builds plus one
final core/embedded build per OS; no public/harness/native/fixture/package builds.
The adverse final comparison stops standalone references and broad/package work.
Restore exact qualified core/embedded DLLs, source and source manifest, retaining
rejected debug symbols separately. Reuse6,824full/live checks per OS, the SAME
actual package/all3examples and greenCI44161514834. Package539cbe19 is unchanged.

Setup corrections remain explicit: an overbroad core-file exclusion initially
omits benchmark files and is narrowed to four exact names before workers start;
a quoting error stops codegen before remote execution. Original failures, patches
and logs remain. No fixture, reference, correctness gate or measured body changes.

[Evidence](copy-vector-tail-trials-evidence.json) retains every actual and
restoration receipt. Mirrored/member-verified `copy-vector-tail-trials.tar.xz`:
**4,166,080 B /258 members**, SHA256 `51fda2970cc3b28358b57c73af6821e93712ff3b022dae1b65f6e7b38bfa1020`.
Archived cvt1236/cvu1237 scripts, candidate sources/DLLs, codegen, rejected final
artifacts and identified baseline/actor archives reproduce this bounded round.
No renderer dependency or favorable replay follows; current copy uncertainties
and the separate-byte gap remain open.
