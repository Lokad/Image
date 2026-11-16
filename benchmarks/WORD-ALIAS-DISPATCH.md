# Reuse the row kernel for word self-copy

The existing safe grouped row-copy kernel now handles RGBA16 self-copies between
different packed rows. Those slices are disjoint; existing reverse traversal
preserves snapshot semantics. Byte decisions, separate-word runtime copying,
same-row memmove, clipping, validation, cancellation chunks and ownership are
unchanged. No new implementation, storage, cache or public API.

Twelve controlled alternating DLL pairs on the original required word-overlap
control save **16.697% / 1.234933 µs**: **7.396000→6.161068 µs**, ratio interval
**.830907–.835152**, **0 B/op**. This scopes the incremental gain. Do not subtract
historical standalone clocks or add it to earlier gains.

| Current complete operation | Lokad / reference µs | Ratio interval / status | Managed B/op |
| --- | ---: | --- | ---: |
| Original RGBA16 overlap, 257×129, (1,1), forward/reverse pair | 5.576 / FFmpeg 5.077 | 1.089617–1.104081 / gap | 0 |
| Separate RGBA8 copy, 256×129 | 2.056 / FFmpeg 1.883 | 1.040226–1.141546 / gap | 0 |
| Separate RGBA16 copy, 256×129 | 4.103 / FFmpeg 4.124 | .985731–1.002100 / uncertain | 0 |
| Existing RGBA8 overlap and reverse pair | 3.624 / FFmpeg 3.867 | .933971–.940570 / pass-stop | 0 |

The original word short screen is **5.568/5.020 µs**, upper1.142394; independent
twelve-actual confirmation supplies the current row above. Its original
[7.010/5.092 µs baseline](ORIGINAL-WORD-OVERLAP.md) is retained as history.
The separate-byte standalone point is adverse versus historical costs. One
justified controlled comparison resolves that suspected regression:
**1.983042→1.984556 µs**, ratio **.998832–1.002701**, zero allocation change.
This is inconclusive for a causal difference, not evidence of a slowdown or gain.
No favorable standalone retry or neighboring production variant follows.

Every affected complete reference boundary reconciled before broad qualification.
Original inputs, geometry and references stay frozen. The original word control
uses 16,384 complete pairs per actual, all twelve actuals and three native
brackets; native FFmpeg n8.1 uses 258 disjoint plane calls per pair. Independent
word uses 65,536 matched adjacent operations; the byte controls reuse their
original BDN harnesses and conservative all-actual/outlier-diagnostic bounds.
Native copy arguments are prevalidated; managed public validation remains timed.
Neither side resets or allocates canvas/output per operation. CPU0, optimized JIT,
natural nonconcurrent workstation GC and idle-v2 guard remain. Direct DLL pairing
is an incremental comparison, not reference parity; independent reference clocks
and their remaining gaps are reported separately.

Windows and Linux each pass **6,745 full/live checks**, **25 internal/190 public**
normal/scalar focused checks, wide cross-chunk aliases/tails and original full-word
snapshot/untouched/hidden-RGB/cancellation/refusal/recovery checks. The SAME actual
Release package and all three examples pass on both platforms. Each OS builds
core/embedded tests once; public tests, harnesses, tools, native references and
fixtures reuse their builds. One NoBuild pack; the accepted timed Linux DLL is
never rebuilt. Published packages remain immutable; publication is postponed.

Allocated, newly admitted and retained library capacity stay **zero** for these
borrowed mutations. The original word canvas is **1,062,936 B**, owned outside
the operation. Physical/native/JIT peaks remain unmeasured. Other production
methods are verbatim and retain their identified evidence; this refresh does not
claim whole-library parity or a full release measurement.

[Evidence](word-alias-dispatch-evidence.json) pins source, all four current cases,
both controlled comparisons, exact snapshots, package and build counts. Raw
`tmp/word-alias-dispatch-qualified.tar.xz`, SHA-256
`5ae813b07fcaf5a88464400712d560efcbc41034a517944854649c174c1b5b51`,
4,589,296 B / 148 members, is mirrored/member-verified with pinned prior-worker
archives. All adverse actuals and pre-sample tooling failures remain. Reproduce
the original word control with the [worker commands](ORIGINAL-WORD-OVERLAP.md);
`--production-changed` only labels the new result. The retained controllers
rebind unchanged neighboring/paired workers to the two immutable DLLs.

Stop the passing byte overlap. Keep the remaining gaps and word uncertainty open;
the [finite original coverage](ORIGINAL-PLANNED-COVERAGE-RECONCILIATION.md) also
contains unmeasured distinct boundaries. No shape/depth/filter/quality grid is added.
