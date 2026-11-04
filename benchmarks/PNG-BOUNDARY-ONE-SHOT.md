# PNG complete-operation boundary refresh

Three existing controls now include fresh codec/options/zlib and packet-wrapper
preparation/disposal on both sides. Word references also include endian-frame
allocation, conversion and disposal. Async output retains complete staging,
fresh AVIO/scratch, growing output, copies and release. Sources, settings and
complete managed boundaries stay frozen. This is a current cost refresh, without
a speedup or causal regression claim from replacing retained native preparation.

| Complete 513×259 operation | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA16 Fastest1, live token | 22,117.18 | 20,058.57 | 1.103 / 1.116 | 2,437,652 | 968,216 |
| RGBA16 Fastest1, forward-only | 21,681.81 | 19,863.09 | 1.092 / 1.140 | 2,437,684 | 968,216 |
| RGBA8 Optimal6, async nonseekable | 19,366.24 | 18,983.03 | 1.020 / 1.029 | 882,204 | 210,973 |

All remain confirmed gaps. Each has twelve actuals: eleven filtered word results
and ten async results. Every actual, both confidence views/native brackets and
passing quiet guards remain. No input, quality, shape or workload grid expands.

The live token is never signaled during timing; token-source setup is excluded.
Cancellation behavior is checked outside timing. Native has no caller token.
Forward-only wrapper creation/disposal and callbacks are timed; seek/position/
length remain unavailable and caller output stays open. Native independent packet
ownership needs no equivalent wrapper. Async callbacks complete immediately;
actual awaited delivery, segmented staging, output growth/copy/disposal stay timed.
Native AVIO's internal flush completes delivery without requiring caller flush.
No final snapshot, forced scheduling or real I/O is timed.

Frozen full packets, samples/alpha/hidden RGB, source and independent outputs pass.
The new async worker's normal/scalar packets match the frozen 210,973-byte packet,
preserving the earlier full pixel/CLI proof. Current managed normal/scalar probes
also check refusal, cancellation, sink failure/recovery and surviving outputs.
Original retained worker/source/identity/proof remain unchanged. An initial source-
manifest mismatch stops before controls; updating measured-tool identities reuses
the same builds, without relaxing the guard or repeating a completed measurement.

Across these two tooling stages: two portable harness builds, one native adapter
projection build, cached selectors and zero production/test/package builds.
Qualified Windows/Linux/full/live/actual-package proof remains applicable to the
unchanged production. Allocation is distinct from physical/admitted peak and
retained storage; those capacities and native allocation are not newly measured.
Other affected PNG costs remain historical until their named boundary is reconciled.

[Evidence](png-boundary-one-shot-evidence.json) pins producers, all actuals,
settings and gates. Mirrored/member-verified raw:

- `png-async-one-shot-current.tar.xz`: 2,470,348 B / 108 members, SHA256
  `597b761b0950956c2ea365ea273743129640c20927764c599c882c6ea0163890`.
- `png-boundary-one-shot-current.tar.xz`: 1,352,420 B / 68 members, SHA256
  `783b4dc3f79538dcd42633dd97f75856bf1c5047f8ba5028a5eb43dad4fbabad`.

Qualified source/package and unchanged runtime/reference dependencies are pinned.
Publication is postponed; the full 282-path objective remains open.
