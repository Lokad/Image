# RGBA16 PNG: equivalent one-shot operations

Costs below precede the accepted
[lazy execution state](PNG-LOCAL-EXECUTION-STATE.md) and
[greedy execution state](PNG-FAST-LOCAL-EXECUTION-STATE.md); use those reports for
current Optimal/Fastest results. The equivalent reference boundary remains valid.

The existing Optimal/Fastest word controls now compare complete one-shot encoding
on both sides. FFmpeg creates and releases its codec/options/zlib context, packet
wrapper and independently owned payload inside each operation. Its endian-conversion
frame is also allocated, filled and released inside timing, after actual codec
users finish. Both sides borrow only their resident source between calls.

| Existing 513×259 RGBA16 control | Lokad µs | FFmpeg µs | Ratio / upper | Managed allocated B/op | Complete packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Optimal6 / native6, filter None | 26,230.66 | 22,267.87 | 1.178 / 1.201 | 3,420,516 | 963,202 |
| Fastest1 / native1, filter None | 24,076.62 | 20,160.55 | 1.194 / 1.204 | 3,420,516 | 968,216 |

Independent twelve-actual confirmation follows each three-actual screen on the
same immutable DLL/harness/worker. Each actual includes four complete managed
operations. Native brackets, versioned quiet guards, one CPU, source/settings,
full 16-bit samples, hidden RGB, exact packets and independent output all pass.
Both conservative lower bounds exceed one: these are demonstrated gaps.
No production change or speedup is claimed from correcting the reference boundary.

A separate word adapter projects this repository's existing reference wrapper
into ignored storage. Its input source, projection, build script, native archives
and resulting binary are pinned. The shared wrapper and older byte/retained-context
workers remain byte-verified and unchanged. No upstream source is added.

Four untimed native checks cover both controls in normal/scalar modes. An initial
managed setup catches a second byte-only constructor guard before any timed
actual; the corrected harness reuses that native proof without rebuilding or
rechecking the worker. Both complete managed setups additionally preserve the
existing resource, caller-stream, source/output and native CLI normal/scalar gates.
Two isolated portable harness builds, one native build; zero production, selector
or package builds. All failed setup and valid runs are retained.

The unchanged production DLL/package reuses HSP200's 5,541 full/live checks per
OS and same actual Release-package/three-example proof, with two additional
actual-production histogram Facts passing normal/scalar checks on both OSes.
This is a two-row development comparison update; other affected H costs remain
historical and this is not a full release refresh. Native allocations/physical
peaks are unmeasured. Deflate's 384 KiB admission and zero retained operation
scratch remain unchanged; total B/op is a different quantity.

[Evidence](png-word-one-shot-boundary-evidence.json) pins current costs, gates,
source/producer identities and unchanged qualification. Raw
`png-word-one-shot-boundary.tar.xz`: 21,019,160 B / 202 members, SHA256
`05ee89429234e33939e2bb2efd4979fca998a233ee461a17ddd6123205b4e13e`,
mirrored/member-verified, with the qualified-core and existing runtime archives
pinned as dependencies. The subsequent attribution and parser stage reuse this
reference without worker or harness rebuilds.
