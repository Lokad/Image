# Historical JPEG probe attribution

The advancement/suspension proposal below was completed by [JPEG inspection advancement](JPEG-INSPECTION-DRIVER.md). [Current-package profiling and the completed-read trial](JPEG-COMPLETED-READ-TRIAL.md) supersede this profile as an active lead; original source-specific evidence remains below.

Read-only profile of the [qualified refill-inlining DLL](REFILL-CALL-INLINING.md):
one bounded **two-second** CPU sample, operation on CPU0, observer on CPU1.
**1,993 locations /1,699 resolved managed**; same-process map/JIT method sizes
match. All instrumented elapsed results are discarded. No build, source/package
change, new workload or qualification repeat.

The JPEG parser has **319 locations /7,451 B code**. Reader ReadAsync,
JpegHeaderMarkerAsync and SkipAsync have **225/158/96 locations**, with
**1,977/1,706/2,247 B code**. Refill work is now inlined there; comparing those
counts with the old separate FillAsync body would not measure saved time.
Sampling skid, inlining, runtime/GC and native-reference orchestration limit
attribution. These counts are not elapsed fractions or gain forecasts.

Current frame-field code repeatedly reloads the header array and checks bounds
between width/height/count stores. A single checked six-byte span preserves the
same bytes, reads and checks, but **is not retained**: its favorable three-pair
screen (0.244228→0.236322 µs) is uncertain;
independent twelve-pair confirmation is adverse **0.235529→0.237398 µs**,
interval **1.003734–1.012150**, still **568 B/op**.
Keep both observations, without a causal codegen explanation or favorable replay.
Linux10internal/148public normal/scalar pass. Exact source/core/embedded/public
modules and source inventory are restored without rebuilding. Windows modules
and actual package never change; qualified6,721checks per OS and SAME actual
package/three examples remain valid. One Linux core/embedded build, zero others;
no native/codegen capture, broader controls or package qualification follows.

The storage audit reuses [initial-workspace evidence](PROBE-INITIAL-WORKSPACE-TRIAL.md):
the same32→256B design was already tested. It saved224B for early seekable inputs
but added56B and promotion work for nonseekable JPEG. Both original screens were
inconclusive; no new build or timing replays that design on a newer clock.

Next architectural lead: separate synchronous JPEG grammar advancement from
genuine I/O suspension, using operation-private value state and one grammar.
Preserve original reads, checks, polls, failure order, public owner wrappers and
exactly-once pending consumption. No new immediate-path heap owner or parser cache.
This requires a concrete design and bounded complete-operation screen; it differs
from the failed outer-selector shortcut and fixed-view implementation.

Benchmark Input.Copy/ReadAsync account for **389 locations**. Keep frozen delivery
and callbacks unchanged; they are not production kernels. Builder/selector samples
also remain, but the [direct-selector trial](JPEG-SELECTOR-DIRECT-TRIAL.md) had no
useful screen, so it is not replayed unchanged.

All original input/reference/descriptor/independent-owner and quiet gates pass.
The shared proof is restored exactly. Current costs, allocations, retained capacity
and actual-package qualification stay unchanged; physical/native/JIT peaks are
unmeasured. Font/text controls and all passing codec paths remain closed.

[Evidence](jpeg-probe-current-attribution-evidence.json) pins source and analysis.
Raw `jpeg-probe-current-attribution.tar.xz` (214,420 B/41 members), SHA256
`432cd62190bec1736584bcf2fd7ddf71415d4b6140f302bff1cf5c2796d754b2`, is mirrored/member-verified with the qualified baseline dependency.
No current-cost renderer dependency. Full 289-path completion remains open;
publication stays postponed.

Fixed-view raw `jpeg-frame-view-trial.tar.xz` (1,787,292 B/81 members), SHA256
`20b7430c40d9f086e319b3bd6972f2bb0c1768a106c966fa1e021ce35bd64077`, is mirrored/member-verified with this profile and qualified
baseline dependencies. Both archives remain; current costs do not change.
