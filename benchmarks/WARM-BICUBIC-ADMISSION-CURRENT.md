# Current tight-memory bicubic reduction

The accepted current producer still has a gap on the existing TightWide8 operation:
**1.704x/upper1.949x**. This resolves its pre-0.2
ownership-path uncertainty before optimization; the old independent result is historical.

| Existing complete operation | Lokad µs | Native µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 513×259 → 128×64, input+output+4 KiB local limit | 594.66 | 349.08 | 1.704 / 1.949 | 32,913 |

Three actuals, native pre/post brackets and both official idle-v2 guards pass.
The original exact rational managed frame, native normal/scalar maximum1/RMSE0.25,
input/native hashes, 568,332B local limit, two independent outputs and unchanged
source pass. Native allocation is unmeasured. A supplied shared root is a separate
untimed boundary; this result establishes normal-operation performance only.

The labeled reference is zimg, FFmpeg's zscale backend, with the same packed-input
deinterleaving, eight-pixel clamped halo, premultiplied float filtering, owned
planar/packed output, unpremultiplication, quantization and release. Both operations
run on one CPU. Startup, IPC, I/O and validation remain outside timing.

The frozen accepted C12 producer, original selector/harness, fixture and native
worker are reused without compilation, new settings or a long job above parity.
The accepted 4,469-test/OS and same-actual-private-package consumer/three-example
qualification remains valid. No production input changes in this round.

One current one-second profile retains 983 samples with both official guards:
86.8% in WideColumnBicubicResize and9.5% in the coefficient kernel. These are
profile shares, not comparative timings. Four columns can replace three within
the same 4KiB workspace:17row/coefficient slots, byte relative offsets and30decoded
pixels total4,068B. At scale<=4.125, support is at most17samples; actual count/offset
guards must precede writes. The fixed case needs32strips rather than43. This is
feasibility, not an implemented or measured speedup.

Next: implement and check that single strip-width primitive, preserving per-pixel
arithmetic order, existing exactness certificates/replay, work, cancellation and
shared-root ownership. Screen only the existing TightWide8 operation first.
The finite254-case objective, full warm parity and release readiness remain open.
[Evidence](warm-bicubic-admission-current-evidence.json) retains all statistics,
allocations, producer/native identities, profile and mirrored raw archive hashes.
