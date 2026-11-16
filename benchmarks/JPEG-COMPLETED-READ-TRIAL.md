# JPEG completed-read trial

**Not retained.** Separating the private completed sentinel from general
Task/source completion handling produces an inconclusive independent result:
**0.215744→0.212572 µs**, point gain **1.46%**, interval **0.964723–1.006554**.
Allocation stays **568 B/op**, with no admission or retention change. The earlier
three-pair screen's 6.00% point is also uncertain; both captures and every sample
remain. No speedup, regression, FFmpeg parity or causal code-generation claim follows.

The candidate keeps the original nondefault completion body out of line and handles
`ValueTask.CompletedTask` directly. The private reader returns that sentinel after
consuming actual users; it has no Task/source or GetResult side effect. Acquisition,
reads, checks, polls, grammar, failure order and asynchronous owners are otherwise
source-identical. No new array, cache, pool, owner, API or numerical policy.

One two-second current-package profile motivated the trial. CPU0 runs the original
complete async-JPEG probe; CPU1 samples its parent. The trace has **1,990 locations /
1,729 resolved managed locations**, with same-process map/JIT size matches:

| Method | Native code B | Sample locations |
| --- | ---: | ---: |
| ReadAsync | 1,977 | 270 |
| SkipAsync | 2,247 | 109 |
| JpegHeaderMarkerAsync | 1,706 | 191 |
| AdvanceJpeg | 8,264 | 284 |

Input delivery callbacks account for another 354 locations and remain frozen.
Skid, inlining, runtime/GC and reference orchestration prevent interpreting these
counts as phase costs or removable fractions. All instrumented times are discarded.
The reader already owns admission and the grammar already separates advancement
from suspension; older reports' proposals for those changes are completed history.
No new generated-code comparison attributes this trial's null effect to a cause.

The first pre-run guard rejected aggregate I/O wait (1.75%, CPU0 0%), with no
concurrent job or measurement output. One distinct idle-ready screen then passed
both guards, followed by independent twelve-pair confirmation. No completed capture
was restarted, and no favorable retry, neighboring variant or native retiming follows.

Linux passes 10 internal/154 public focused checks in normal/scalar modes, including
fragmentation, failure/cancellation, held-user admission, refusal, cleanup and recovery.
Exact original sources, 19 affected build-output files and the source inventory are
restored without rebuilding. Windows DLLs and the actual package never change.
Reuse 6,745 full/live checks per OS and the same Release-package consumer/all three
examples. One Linux core/embedded build; zero other builds, broad or package repeats.
Current benchmark costs stay unchanged; physical/native/JIT peaks are unmeasured.

[Evidence](jpeg-completed-read-trial-evidence.json) pins the current profile, both captures, immutable producers, source proof and restoration. Raw `jpeg-completed-read-trial.tar.xz` (9,148,072 B/119 members), SHA256 `1436dc73e1226bf45f7770fb35e78476fd511054ffb2e5d7d82eecd5ce457eba`, is mirrored/member-verified with qualified package and original-harness dependencies. Replay uses archived jcp1121 profile and jcs1122 prepare/pair/restore recipes. This inconclusive implementation supplies no automatic authorization or permanent family ban. A revised design needs concrete attributable work and equivalent complete boundaries. [Current inspection costs](PROBE-OWNER-COMPLETION.md) and the full 289-path objective remain open; publication is postponed.
