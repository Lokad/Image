# Shared stream-reader execution

[Refill completion inlining](REFILL-COMPLETION-INLINING.md) now owns the five
current probe/header costs. The consolidation, profile and earlier evidence remain.

The forward-skip loop now lives in SkipAsync,removing its private immediate
wrapper. Synchronous and completed-async refills share one completion block;
pending helpers remain. Restoring just these two control-flow sections rebuilds
the entire original source. The same loops,read sizes,validation/error order,
cancellation,counters,admission and actual asynchronous lifetimes remain.
No new storage,owner,cache,API,hint or dependency.

Independent complete async JPEG pairs save **2.32% /6.25 ns**:
**.269726 →.263475 µs**,interval **.954496–.999696**,still **568 B/op**.
This is a scoped DLL gain,not FFmpeg parity. The skip-only stage's earlier
independent point is favorable but uncertain (interval.960166–1.000458);
it establishes no separate gain. Percentages are not added across stages.

| Original complete boundary / labeled FFmpeg adapter | Lokad / reference µs | Conservative upper / status | GC B/op |
| --- | ---: | ---: | ---: |
| ProbeDimensionsPng | 0.119 / 0.123 | 0.994006 / pass | 568 |
| ProbeDimensionsWebp | 0.122 / 0.122 | 1.034304 / uncertain | 568 |
| ProbeDimensionsJpeg | 0.234 / 0.191 | 1.264532 / gap | 568 |
| ProbeDimensionsJpegAsync | 0.266 / 0.203 | 1.337674 / gap | 568 |
| PngHeaders | 0.296 / 0.568 | 0.528563 / pass | 568 |

All five current views use the same actual immutable timed/package DLL,twelve
actuals,both native brackets and quiet guards. Frozen input,field acquisition,
exact independent descriptors and disposal remain. Async callbacks complete
immediately;suspended-user behavior is tested separately. PNG dimensions now
passes/stops;PNG headers pass. JPEG probes remain gaps and WebP uncertain.
Sync/JPEG-header paired guards span parity;no gains are claimed for those paths.
No favorable retries,new fixtures or complete-workload boundary changes.

One bounded2second current-process profile supplies1,991locations,including
141in SkipAsync and293in FillAsync. Same-process map/JIT offsets locate leads,
including the skip return-copy blocks;they are not elapsed-time fractions or a
forecast of savings. Instrumented times are discarded. The B refill revision
also affects PNG/WebP,so their existing controls refresh once rather than reuse
the A-only narrower invalidation claim. Identify/codec/edit/resize/font/text paths
remain source-identical and retain their identified evidence.

**Windows/Linux each pass6,721full/live checks**,10internal/148public focused
normal/scalar and the **SAME actual Release-package consumer/all three examples**.
Two Linux core/embedded builds cover A+B;Windows builds B once. No public,
harness,native or fixture build. Performance reconciliation precedes the single
broad/package milestone. One pack,no accepted-module rebuild after timing.
The original A sources/modules are restored before B without rebuilding.

Admission stays **512B dimension /8448B bulk maximum**. Total allocation,logical
capacity and retained state remain separate;zero added capacity/retention and
no new physical/native/JIT peak claim. Published packages remain immutable;
publication and the full289-path objective remain open.

[Evidence](stream-reader-consolidation-evidence.json) retains the profile,both
stages,all samples,source reconstruction and package qualification. Actual package
`dbe677a1271cae2f96388532abc0c89e904aed97cf4d8f2d5279bdb8420d478d`,465,593B,contains timed DLL
`5583111e5c98959de23b8c5418ca05eb27dfea04b8eb41005fe81ca2b0874927`. Raw `stream-reader-consolidation-qualified.tar.xz`:
5,500,116B/231members,SHA256
`d2aed94572b6347546acc1ec9629c689ca90e8886ac036604850734537ebf5de`,mirrored/member-verified. [Earlier bounded requests](BOUNDED-PROBE-REQUESTS.md)
retain the preceding change;[direct publication](PROBE-RESULT-PUBLICATION.md)
still owns unchanged complete Identify costs.
