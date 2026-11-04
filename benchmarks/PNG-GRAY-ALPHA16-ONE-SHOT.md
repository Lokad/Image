# Current one-shot gray-alpha16 PNG cost

The existing 513×259 gray-alpha16 Optimal6 control now has equivalent complete
one-shot evidence. No production change or additional workload.

| Complete encode | Lokad µs | FFmpeg µs | Ratio / upper | Managed allocated B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Gray-alpha16 Optimal6, one-shot | 10,935.23 | 10,483.28 | 1.043 / 1.052 | 1,360,892 | 506,887 |

All twelve actuals and both filtered/all-actual confidence views remain; ten
filtered observations, conservative lower 1.036. This remains a gap.
The historical 13.36/10.17 ms comparison retained native preparation. Replacing
it establishes current costs, without an incremental CPU or allocation claim.

Both subjects borrow the frozen resident RGBA64LE source and own independent
complete packets. Managed fresh MemoryStream/public EncodePng/growth/disposal stays
timed. Native fresh codec/options/zlib, packet wrapper and YA16BE working frame
creation/disposal are included. The same direct red/alpha packing and endian
conversion execute every operation; no SWS conversion/context. Native Ready and
pinned generated-source/build identities enforce these lifetimes. Existing
adapters and native workers are unchanged.

The full 506,887-byte packet (addd3f23…) and input (9d0ed2b9…), level6/filterNone/
color4, all 16-bit samples/alpha/hidden RGB, normal/scalar CLI roundtrips, source,
independent outputs, caller-stream and quiet CPU0 gates pass. B/op describes the
complete managed operation at four-operation batching. Native allocations,
physical/admitted peaks and retained-state costs are not newly measured.

One isolated successful harness build (two compile attempts), one smoke driver,
one native prototype and one promoted worker build. The latter adds explicit
workspace-lifetime diagnostics, without changing encode work. Reuse the existing
confirmation selector and all completed immutable builds. No production, test,
package or measurement builds; no timing repeat. Unchanged production reuses
5,634 full/live checks per OS, affected scalar and SAME actual Release-package/
all-three-example verification. Publication stays postponed.

[Evidence](png-gray-alpha16-one-shot-evidence.json) pins boundaries, producers,
all gates, corrections and reproduction. Mirrored/member-verified
`png-gray-alpha16-one-shot-current.tar.xz`: 3,819,304 B / 213 members,
SHA256 `03444bfd9d910b101a3c0432bc9ba764a945dbe02572c9edaa9ba9e5d9ec0ba0`. Sources, native/harness/selector modules, original
prototype, failed logs and every actual/bracket/guard are retained with frozen
qualified-source/framework dependencies. The full 282-path objective remains open.
