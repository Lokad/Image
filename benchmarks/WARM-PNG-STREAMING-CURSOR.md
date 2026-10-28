# Shared PNG Deflate cursor

PNG8 span validation takes **22.85% less filtered elapsed** in a fresh independent
comparison, with unchanged **240 B/op**. The remaining **1.042× / upper 1.052×**
is still a gap to warm single-CPU parity. The two existing PNG16 validation cases
retain independent passing evidence on the new production build.

| Quiet confirmation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| PNG8 span, baseline | 1,862.371 | 1,379.830 | 1.350 / 1.354 | 240 |
| PNG8 span, candidate | 1,436.810 | 1,378.564 | 1.042 / 1.052 | 240 |
| PNG16 span, candidate | 2,878.686 | 2,960.749 | 0.972 / 0.974 | 240 |
| PNG16 async nonseekable, candidate | 2,942.287 | 3,614.992 | 0.814 / 0.817 | 8,544 |

Each confirmation retains twelve actual samples, filtered and all-actual
confidence, native before/after brackets and passing idle-v2 guards. The PNG8
reduction is 22.87% using all actuals; both interval forms separate. The PNG8
short screen was 22.7% below the preceding applicable confirmation; it is retained
separately. Inputs, reference settings and complete-operation boundaries are
unchanged. These are the existing OriginalExif 513×259 packets: complete coding
and all nine source metadata fields, independent descriptor ownership and cleanup,
without RGBA normalization or caller raster output. Async includes acquisition,
growth, EOF and release using resident immediate callbacks.

The noncancellable row inflater now borrows the existing 64 KiB history slab and
uses the same block-local bit cursor as full materialization. It leaves room for
a 258-byte token, publishes the produced range, retains the last 32 KiB and
resumes without reading a token at a slab boundary. Expected-output limits and
end-of-block are separate from the slab stop. No array, table, owner or admission
boundary is added; the cancellable decoder retains its direct checkpoints.

One original regression first passes on the previous production implementation:
stored history plus maximum-distance matches reach exactly 65,536 bytes before
an empty final block. BCL expansion, near-tail truncations, expected-size errors,
live-token behavior and shared-root cleanup agree. Existing tests cover multiple
slides, overlapping copies, canonical fallback, guarded partial materialization,
all filters/depths, malformed/checksum precedence, admission and cancellation.

Both platforms pass 5,057 full checks, including 947 live comparisons, 119
focused internal and 40 public checks with SIMD enabled and disabled, and the
212 affected scalar public PNG checks. The same actual private Release package and
all three consumer examples pass both platforms. One immutable producer per
platform is reused; benchmark/native workers and selectors are cached.

[Evidence](warm-png-streaming-cursor-evidence.json) records all samples, exact
identities, scalar counts, package checks and the mirrored member-verified raw
archive. Noncancellable streaming PNG validation and fused RGBA decode, and the
shared materialized PNG cursor, have changed CPU paths. Only these three
validation controls are refreshed here; other affected decoder costs are
prior-production until reconciled. The cancellable token decoder itself is
unchanged, with an added dispatch check. Indexed/interlaced counting, encoding,
inspection, other codecs, editing and text keep applicable evidence.
Exact packets/pixels and all declared contracts remain required.
The full 267-path parity objective remains open.
