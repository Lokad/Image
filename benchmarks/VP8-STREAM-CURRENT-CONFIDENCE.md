# Current VP8 stream confidence

The existing transparent VP8/ALPH stream decode **passes and stops** on the current
immutable package DLL. This resolves uncertainty without a production speedup claim.

| Existing 513×259 RGBA8 complete operation | Lokad / FFmpeg µs | Conservative ratio interval | Managed allocated B/op |
| --- | ---: | ---: | ---: |
| Fresh non-exposed seekable stream, decode/dispose | 5,841.97 / 5,986.04 | 0.963–0.988 | 240,496 |

Both sides reacquire fresh owned encoded input through complete reads/EOF, decode,
normalize, acquire independent RGBA output and release operation owners. Managed
creates a fresh non-exposed MemoryStream. Native creates AVIO/scratch/packet payload,
resets/decodes/converts all pixels and merges exact alpha. Its decoder/conversion/
frame/packet wrappers remain retained, as before. Frozen 98,590 B input, exact full
managed/native output anchors, alpha, scalar CLI, caller stream lifetime and
independent surviving outputs pass unchanged. No quality-number matching is involved.

Twelve fixed alternating pairs of 16 complete operations replace the older global
brackets. Twelve native batches precede/follow; every actual and the same conservative
paired/three-bracket confidence rule remain. Original 6,147.32/6,261.60 µs, upper
1.004 uncertainty stays history. Changed clocks do not establish a speedup; no
unchanged favorable repeat follows. Quiet sequential CPU0, Release optimized JIT,
nonconcurrent workstation GC and both idle-v2 guards pass.

Allocation stays **240,496 B/op**. No capacity/retention change. Borrowed encoded
input is 98,590 B, its owned bucket 131,072 B and independent result 531,468 B. Their
662,540 B owned-capacity sum excludes other decode workspace and replacement overlap;
it is not measured peak admission or physical memory. Operation scratch retention
is zero; the two-slot/4 MiB default pixel store and governed bypass are unchanged.
Native/JIT/physical peaks and idle payload are unmeasured.

**Zero builds:** reuse the verified VDC936 driver with the original frozen stream
harness/worker and current qualified DLL. Reuse 6,624 Windows/Linux full/live checks,
focused scalar/ISA and SAME actual-package consumers/all three examples. No broad
or package repeat. All eight synchronous input views, including Identify, now pass.
WebP reading has nine passes/three opaque uncertainties; current 191 image views have
176 passes/one gap/14 uncertainties. Full 289-material-path completion remains open.
Publication is postponed; published packages remain immutable.

[Evidence](vp8-stream-current-confidence-evidence.json) retains identities, original
gates, every actual and reproduction. Raw `vp8-stream-current-adjacent.tar.xz`:
25,268 B / 17 members, SHA256 `4b56c3f7e91ce36f620883e9fc009c68a90253436fdb185493ff405871fc1128`,
mirrored/member-verified. Dependencies preserve the earlier uncertain capture,
cached driver, original fixtures/native workers and qualified packages.
