# Current asynchronous caller-RGB420 JPEG encoding

The existing 513×259 RGB24 nonseekable-output control independently confirms a
**1.143× / conservative upper 1.154** CPU gap. Reconstruction matches exactly;
the complete 52,091/52,093-byte packets differ only by approved COM attribution.
This replaces stale cost evidence, without a causal comparison to its old producer.

| Complete warm single-CPU operation | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Packet B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| Caller RGB24 EncodeJpegAsync, 420 | 1,862.241 | 1,628.855 | 1.143 / 1.154 | 309,526 | 52,091 / 52,093 |

Both borrow the same original RGB24 projection and include conversion/sampling,
encoding, admission/staging, fresh growing nonseekable output, actual immediate
delivery copies and release. Managed awaits WriteAsync users; native includes
FFmpeg AVIO buffering and internal flush. Neither constructs an RGBA intermediate
or flattens alpha. Projection, real I/O, forced scheduling and final snapshots are
excluded on both sides. Settings stay Q85/native qscale5 and 420; numerical quality
values are not treated as equivalent. Native allocation is unmeasured.

The current public async probe verifies exact packets, decoded pixels, caller
ownership, surviving outputs, terminal refusal, cancellation and sink-failure
cleanup/recovery. Setup and cleanup repeat packet/source/owner checks. Frozen
native bytes permit reuse of normal/scalar oracle proof; the scoped producer
header is restored exactly. All twelve actuals, both confidence views, native
brackets and passing idle-v2 guards remain available.

Zero measurement builds or profiles: immutable core, harness, worker, fixture and
the verified cached selector are reused. Current Windows/Linux full/live and the
same actual Release-package/three-example qualification carry forward. One
reporting-only follow-up removes an old calibration record from future async
JPEG receipts; current quality comes from the actual probe and exact packets.
The original timed receipt/source/harness remains archived, without retiming.

[Evidence](current-async-jpeg-rgb420-cost-evidence.json) pins all samples, current
quality, exact invalidation and qualification reuse. Mirrored/member-verified raw:
`current-async-jpeg-rgb420-cost.tar.xz`, SHA256 `b6839da592542da50aef106f95d4b96cb1afb494d6d2435890e4851778d989f5`;
its immutable-runtime and qualification dependencies are separately pinned.
No case or workload grid is added. Full upper≤1× parity across 268 paths stays open.
