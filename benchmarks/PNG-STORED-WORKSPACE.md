# Stored PNG workspace

Historical stage: [current IDAT publication](PNG-IDAT-FORWARDING.md) supersedes
these affected encoder costs and transport ownership. Its deflater algorithms and
the reference boundaries qualified here remain; raw evidence is retained.

Uncompressed PNG now omits unused matching heads, links and tokens. This removes
three arrays/196,604B payload per encode and reduces the deflater reservation
from384KiB to72KiB. History64KiB and output4KiB remain operation-owned.
Compressed modes preserve their original allocation order/384KiB reservation.
All packet generation, cancellation, instance fields, checksums, filter and
compression decisions remain verbatim. No cache, pool, new owner or API is added.

| Existing complete encode,513×259 | Lokad / FFmpeg µs | Ratio / conservative upper | Managed B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: |
| StoredRGBA8,post-GC4op actuals | 92.72 / 190.15 | 0.488 / 0.535 | 613,844 | 533,444 |
| StoredRGBA16,post-GC4op actuals | 501.70 / 405.21 | 1.238 / 1.255 | 1,151,996 | 1,066,552 |
| SameRGBA16,natural-GC64op adjacent pairs | 350.36 / 405.10 | 0.865 / 0.904 | 1,151,529.875 direct | 1,066,552 |

Byte passes/stops. Word passes only the scoped repeated-use view; its post-GC
diagnostic remains a gap and general warm parity stays open. The current baseline
word confirmation is507.12/405.75µs,upper1.336. Candidate501.70µs is not a proven
CPU gain: filtered/all-actual raw and normalized confidence overlaps, with no
separated adverse effect. Preserve every actual, including the three-actual
candidate screen515.42/405.56µs. Do not attribute the conditioning/batching
difference solely to GC, sum historical stages or repeat unchanged for a pass.

Complete public EncodePng times row packing, stored Deflate, fresh caller-owned
MemoryStream creation/growth and disposal, without a final snapshot copy. The
frozen FFmpegn8.1 worker retains codec/frame/wrappers but includes independent
packet acquisition/encode/release and word endian conversion. No preparation is
silently removed. Original source/settings/full packets, all pixels/alpha/hidden
RGB, borrowed source and independent output checks pass. CPU0,one thread, optimized
JIT/nonconcurrent workstation GC and idle-v2 guards are unchanged.

Global confirmations retain12actuals of4managed operations, with BenchmarkDotNet's
verified forced GC outside each actual. Adaptive50ms native samples use512byte/
128word operations. The natural-word driver reuses12alternating pairs of64complete
operations per subject and12native samples before/after. No harness, selector,
native worker or fixture build and no added workload/image/settings grid.

Total allocation falls196,680B per byte encode and natural-word encode; post-GC
word counters show196,272B less. These boundaries/counter differences are retained,
not collapsed into an exact universal saving. Three fixed-array payloads fall
266,236→69,632B, while coarse reserved capacity falls393,216→73,728B.
Operation scratch retention after disposal is zero. Output growth/capacity and
borrowed resident source payloads531,468/1,062,936B are unchanged. Governed capacity,
total allocation and physical/native/JIT/pool peaks are distinct; the latter are
not remeasured. No physical-peak or causal CPU saving is claimed.

Ten focused cases cover preallocation admission, stored/nonzero ownership,
aggregate refusal/recovery, local threshold, cancellation/writer-failure cleanup
and independently surviving exact byte/word packets. Existing all-depth/color/
compression public resource tests now use the mode's actual reservation.
The new actual-package consumer covers byte/word exact sync/async encoding,
smaller admission, source/root/stream borrowing and surviving output ownership.

Windows/Linux each pass6,449full/live checks (3,120internal+2,381public+948live),
plus690internal/246public PNG checks in normal/scalar/AVX512-off/AVX2-off modes.
The SAMEactual Release package and all three usage examples pass bothOSes.
One immutable production build perOS was reused throughout. Initial test analyzer,
writer-double and stale resource-threshold failures are retained with fixes:
three embedded build attempts/two successful versions and two public versions
perOS; no additional production build. One full/package milestone follows
performance reconciliation. Published packages are unchanged; no publication.

Unchanged compressed PNG, other codecs/resizing/text and ISA evidence is reused.
[Predictable16](PNG-HISTORICAL-CURRENT-CONFIRMATION.md) remains a valid passing
compressed control. That report retains earlier stored-mode costs and verified
GC/selector inspection. The allocation/admission improvement is accepted;
broader word sampling remains unresolved. Continue independent demonstrated work.

[Evidence](png-stored-workspace-evidence.json) pins source proofs, full receipts,
complete packet/input identities, immutable builds, tests, package and dependencies.
Raw archive `png-stored-workspace-qualified.tar.xz`, SHA256`df812f63165dc437aa54b333c60993c398a15a34005626d547f19e62b5aaf6cc` (2586244B/162members), is mirrored
and every member verified. It includes failed diagnostics as well as final checks.
All289required paths remain in scope; publication is postponed.
