# Current read-boundary reconciliation

Six existing controls use the qualified 0.2.1 producer. All remain warm CPU gaps;
these are three-actual quiet-VM screens, with no independent pass or speedup claim.
Complete input delivery, independent output acquisition and release are timed.
No production build, new input/settings grid or repeated package proof is needed.

| Fixed control | Lokad µs | Reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| PNG8 complete headers, seekable | 0.805 | 0.590 | 1.365 / 1.603 | 8,528 |
| VP8+alpha decode, seekable | 7,754.220 | 6,004.849 | 1.291 / 1.334 | 1,966,583 |
| PNG8 tight decode | 1,916.027 | 1,398.500 | 1.370 / 1.563 | 68,224 |
| PNG8 tight decode, live token | 1,935.624 | 1,395.551 | 1.387 / 1.585 | 68,224 |
| PNG16 tight decode | 3,015.567 | 2,896.668 | 1.041 / 1.134 | 70,272 |
| PNG16 tight decode, live token | 3,156.935 | 3,068.197 | 1.029 / 1.144 | 70,272 |

The header reference is explicitly an **original bounded header/EXIF adapter
using FFmpeg AVIO and owned descriptor APIs**, not a public FFmpeg inspection API.
Its descriptor now includes all nine fields, including typed source color/alpha
and component count. Both subjects read a fresh unexposed seekable source, skip
opaque IDAT, check structure/Strict EXIF and EOF, and retain independent metadata.
CRC and compressed pixels are opaque. Native scratch is 8,192 B plus fixed header/
descriptor storage; managed local admission is 8,448 B. The native worker already
has no fuel accounting. All 21 original RGBA8 structural/EXIF/CRC controls agree
on managed normal/intrinsics-off and native normal/scalar. This scope does not
establish every source representation or IgnoreInvalid-policy performance.

VP8 retains the exact frozen input, normal/scalar native anchors, exact RGB/alpha
and ownership. Tight PNG retains the two original packets, exact memory ceilings,
one-below rejection, live-token behavior and all RGBA samples. The four tight
screens refresh costs affected by the shared row-engine change; they do not
invalidate the unchanged default materialized PNG path. Native allocations are
unmeasured. Governed storage is separate from default warm GC allocation.

Production qualification is [reused unchanged](WARM-PNG-VALIDATION.md): 5,041 full
checks per OS, 137 internal/88 public scalar checks, the same actual private
Release package and three consumer examples on both OS. Commit `d1f5a92` also
passes all three CI jobs. One Linux header-worker/harness build and archived
selectors serve this round; Windows tooling compiles. No independent timing
confirmation is justified for these gaps. Defer the near-parity word PNG and
submicrosecond wrapper costs while larger encoder opportunities remain.

[Evidence](warm-read-reconciliation-evidence.json) retains every actual, native
bracket, guard, producer, boundary and exact control. The mirrored/member-verified
raw master is `current-read-boundaries-qualified.tar.xz`, SHA256 `db1c1c02210b3b9e232c4545e26dabd5fc6a0e32f17752593fbc3908a2358412`.
The full 267-path ≤1× objective remains open. Other read-policy-affected costs
still require applicable current evidence before any pass claim.
