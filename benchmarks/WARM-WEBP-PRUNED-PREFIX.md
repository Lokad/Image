# Prediction pruning enables a larger coefficient sample

The existing32M-work WebP packet shrinks **102,006 → 101,478 B** with identical
decoded RGBA, source quality and alpha. This closes15.5% of its former3,416 B gap;
the complete **98,590 B** ceiling remains open by **2,888 B**. Six of seven declared
lossy controls pass. This is a scoped incremental gain, not whole-codec parity.

| Existing control | Managed / reference packet B | Managed / reference µs | Managed allocated B/op | Short ratio / upper |
| --- | ---: | ---: | ---: | ---: |
| EncodeBudget8 | 101,478 / 98,590 | 15,608.22 / 53,578.57 | 2,005,004 | 0.291 / 0.346 |

Nonnegative partial squared error cannot improve after reaching the best complete
score. Losing prediction candidates therefore stop evaluating error rows, retaining
mode order, ties and the winning prediction exactly. Every full candidate still
charges192 units:96prediction/preparation and24per evaluated four-pixel row.
Only skipped rows end early; transform/coefficient/writer charges are unchanged.
The dependency alone reproduces the former packet and saves999,336 actual units.
It is used to admit exactly one larger coefficient sample, **64 → 128 macroblocks**,
adding393,216 statistics/cache units and changing probabilities alone. Combined
actual work is **31,343,084**, versus31,947,691 before, within the unchanged32M cap.

The same borrowed root admits **31,744 B** retained cache before allocation,
versus24,064 B. Independent tighter work/memory keeps the smaller sample before
shared admission. Refusal, actual exhaustion and cancellation terminate without
retry; retained outputs, caller inputs/root and cache lifetimes are preserved.
There is no extra frame, transform or input pass. ALPH remains exactly19,246 B.
Original managed100/native75,effort4, inputs/reference packets and all quality
gates stay fixed. The source-PSNR gap remains0.038933 dB within0.25dB; matching
is demonstrated by quality, never numerical quality equality.

Both OSes pass **4,709 tests**,233internal+12public+4differential scalar checks,
and the same actual private0.2.0 package/all three examples. Ten new internal
cases cover exact selectors, edge/tie behavior, row-work refusal,128capacity,
local declines, terminal shared refusal, idempotent disposal and subsequent success.
Existing public8/16-bit independent outputs and resource/cancellation checks pass.
Three old injection callbacks initially missed the larger cache; their capacity
constant was updated without weakening assertions. Fullinternal passes were
reused, only the changed public suite was rebuilt/rerun, then the differential
suite ran for the first time. Production stayed frozen; no whole-suite repeat.

One accepted core perOS, one dependency and two rejected Linux-only producers
were built for distinct input changes. No suites/package/timing were spent on the
preliminary screens. One short guarded three-actual job records the costs above;
CPU parity and independent repeats remain deferred. Allocation is2,005,004 B/op
versus1,997,180 previously; native allocations are unmeasured. Four unrestricted
full records are exactly C76; primary positive-base paths are excluded. Their six
cost observations retain C76/C70 producer identities. No workload grid expands.

[Structured evidence](webp-pruned-prefix-evidence.json) retains immutable raw,
source/producer/package, exact quality/work and allocation proofs. Earlier local
zero/one and header-rate trials passed quality but displaced stronger alpha
matching under the work cap, worsening complete packets; both were rejected.
The accepted dependency is not a standalone speed optimization.
