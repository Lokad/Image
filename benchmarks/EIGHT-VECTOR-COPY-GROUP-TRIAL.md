# Eight-vector copy-group trial

**Not retained.** Eight-vector groups halve copy-loop iterations and independently
improve complete byte rectangle copying by **1.21% /24.16 ns**, but the candidate's
complete word-flip reference fails the required parity gate. The qualified
four-vector implementation is restored.

| Existing complete comparison | Baseline / candidate µs | Ratio interval | GC B/op |
| --- | ---: | ---: | ---: |
| Byte copy, independent managed-DLL confirmation | 1.995336 /1.971180 | 0.978486–0.997290 | 0 /0 |
| Word flip, managed-DLL guard | 17.594937 /17.655469 | 0.998090–1.008835 | 56 /56 |

The candidate's actual word-flip cost is **17.892884 / FFmpeg
17.302762 µs**, interval **1.026296–1.039072**,
**56 B/op**: a gap. It retains the latest 16,384-call complete boundary, twelve
alternating pairs and three native brackets. The matched managed guard spans
parity, so these observations establish no causal word regression or general
alignment/register-pressure explanation. The byte gain is scoped; no workload
frequency assumption justifies the shared change.

The candidate uses eight explicit 64-byte loads before eight stores, checked
512-byte blocks and the identical BCL remainder. Smaller/non-Vector512 rows keep
BCL copying. Callers, clipping, snapshot traversal, cancellation chunks, admission
and ownership remain verbatim. Exact byte/word samples, hidden RGB and independent
outputs pass. No new pixel kernel family, workspace, cache, owner or API.

All original screen/confirmation/guard actuals, adverse data and quiet checks remain.
Other four reference configurations were prepared but not timed after the word
result made this version unpromising. No baseline-reference control, codegen,
Windows candidate, broad/package qualification or neighboring unroll sweep follows.
No candidate/native capture is repeated for a favorable score.

Linux passes **25 internal/190 public** normal/scalar checks, including tails,
clipped/overlapping/multi-chunk copies, cancellation, refusal and recovery. Exact
qualified sources/core/embedded/public modules and source inventory are restored
without rebuilding. Windows modules and actual package never change. Reuse
**6,721 full/live checks per OS** and SAME actual-package/all-three-example proof.
One Linux core/embedded build, zero other builds. Added owned/admitted/retained
capacity is zero; physical/native/JIT peaks remain unmeasured.

[Current copy](DRAWING-ROW-PREPARATION.md), [word-copy uncertainty](RECTANGLE-COPY-ADJACENT.md)
and [flip confidence](OWNED-FLIP-CURRENT-CONFIDENCE.md) remain unchanged.
[Evidence](eight-vector-copy-group-trial-evidence.json) pins all data and restoration.
Raw `eight-vector-copy-group-trial.tar.xz` (1,809,288 B/91 members), SHA256
`12cf243d8672565f4ed79cf4fd63b08e39cdf8071ec23a56bdbb3fc6b576e3e8`, is mirrored/member-verified with original tools/reference
and qualified-package dependencies retained. This rejected trial stays outside
renderer dependencies. Full289-path completion remains open; publication postponed.

## Prepared-row follow-up

A materially revised current prepared-row implementation is also **unpromoted**.
Complete separate-byte copying screens **1.957229→1.963354µs**, ratio interval
**.983008–1.023663**,0B/op, across three reversed65,536-operation pairs. This is
near-neutral/inconclusive, with no useful gain. No confirmation, canonical change,
standalone/affected-passing retiming or broad/package repetition follows.

The earlier eight-vector lead is reconciled before prototyping: it changed the
single-row helper/BCL tail, whereas current prepared execution shares row geometry
and vector ends. The new trial halves only prepared bulk iterations, copying an
odd four-vector group first within its disjoint row so narrow views expire before
the eight-vector loop. Existing tail geometry, row traversal/checkpoints, public
selection, shared single-row copying, self-word helper and portable fallback stay.
No buffer, cache, API, pin or owner is added. Actual helper code grows438→637B,
keeps24B local stack and emits eight loads before eight stores; the1531B public
caller is unchanged. Code observations do not prove elapsed-time savings.

Each OS passes12internal/190public checks in normal, AVX-512-disabled and scalar
modes. The exact retained helper oracle covers775lengths, both row directions,
separate and aliased backings, canaries and cancellation. The initially copied
public filter selects91cases; only the disjoint99-case ownership/transform
complement runs afterward, without repeated tests or builds. Reuse the full
filter in subsequent work. Original full source/canvas/alpha/hidden-RGB/untouched/
owner/native-readiness gates, every actual and idle-v2 quiet guards remain.
One private Windows core/embedded build, zero other builds or new fixtures.

Qualified Windows487a0f7f/Linux801f7d09and packagea54dda1c remain byte-identical;
reuse6,839full/live checks each OS and SAMEactualpackage/allthreeexamples.
Added admitted/retained capacity and allocations stay zero; caller payload and
unmeasured physical/native/JIT peaks remain separate. The tests/history commit
98f06e59passes allthreeCIjobs ([run44210352000](https://github.com/Lokad/Image/actions/runs/44210352000)).

Mirrored/member-verified `prepared-copy-eight-vector-trial.tar.xz`:2,210,892B,
62members, SHA256 `2fc6d760bed43f016a0cc794eac78bdb9e695464b07c96466970e395ff7b22fd`.
The archived `pbg1251-*` recipes preserve exact source/modules, actuals, codegen,
checks, scope completion and unchanged-source pins. The mapping-validation and
original prepared-copy reports retain reused qualified/runtime dependencies.
No displayed result or renderer dependency is added. Three copy gaps remain;
no neighboring group-size or code-placement sweep follows.
