# PNG complete-operation boundary refresh

Two existing RGBA16 Fastest1 controls now include fresh codec/options/zlib and
packet preparation on both sides. Native word-frame allocation, endian conversion
and disposal stay timed. This replaces retained native setup, with unchanged
source, settings, exact packet and complete managed boundaries. It is a current
cost refresh, without a speedup or causal regression claim.

| Complete 513×259 RGBA16 operation | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Live cancelable token | 22,117.18 | 20,058.57 | 1.103 / 1.116 | 2,437,652 |
| Forward-only output | 21,681.81 | 19,863.09 | 1.092 / 1.140 | 2,437,684 |

Both remain confirmed gaps. Each has twelve actuals and eleven filtered results;
all twelve, filtered/all-actual confidence, both native brackets and passing quiet
guards remain. Do not compare their raw points with older retained-state costs to
claim a new optimization. No input, quality, shape or workload grid expands.

The live token is never signaled during timing; source/token setup is excluded.
Cancellation rejection and behavior remain checked outside timing. The reference
has no caller cancellation token. Forward-only wrapper creation/disposal and
callbacks are timed; seek/position/length remain unavailable and caller output
stays open. Native independent packet ownership needs no equivalent wrapper.

Complete packets are exactly 968,216 B on both sides, SHA256
`adbcc5115ee9acce294ab70f65b6f24862b7451d90f870a082269a8008c08e25`.
Full samples, alpha, hidden RGB, source and independent outputs pass existing
checks. Allocation is distinct from physical/admitted peak and retained storage;
those capacities and native allocations are not newly measured.

Only the benchmark setup whitelist and boundary descriptions change. One portable
harness build reuses the existing native worker and selector. Production, tests,
package and their Windows/Linux/full/live/actual-consumer qualification remain
unchanged; no broad rebuild or package repeat. Other timings reuse valid evidence.
The separate async PNG output boundary is still historical and needs its own
equivalent native preparation before tuning.

[Evidence](png-boundary-one-shot-evidence.json) pins producers, all actuals,
settings and output checks. Mirrored/member-verified raw `png-boundary-one-shot-current.tar.xz`:
1,352,420 B / 68 members, SHA256
`783b4dc3f79538dcd42633dd97f75856bf1c5047f8ba5028a5eb43dad4fbabad`.

Qualified source/package and unchanged native/runtime dependencies are pinned.
Publication is postponed; the full 282-path objective remains open.
