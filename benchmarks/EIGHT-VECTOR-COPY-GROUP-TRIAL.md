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
