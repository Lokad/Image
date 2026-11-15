# Current JPEG probe attribution

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
between width/height/count stores. A **single checked six-byte span** is the next
bounded lead. Preserve identical reads, validation/error order, cancellation,
admission and ownership. Screen the existing complete async probe before any
confirmation or qualification; no field/shape/settings grid.

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
