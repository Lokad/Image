# Current adjacent-operation confidence

Current [RGBA16 bilinear confidence](BILINEAR-WORD-CURRENT-CONFIDENCE.md) supersedes
the enlargement cost below. All nine lossy encodes now pass on
[variable-price preparation](VP8-VARIABLE-PRICE-PREPARATION.md). This report keeps
its earlier captures and qualification as scoped history.

The word-alpha longer-batch comparison now passes/stops on the actual current
package DLL. Transparent byte alpha remains passing; bilinear enlargement
remains uncertain on its source-valid unchanged path. These are measurements,
not production speedup claims. All eight declared lossless/alpha encode paths
now pass; the broader nine-control lossy family retains two opaque uncertainties.

| Existing complete operation | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Transparent VP8 RGBA8 | 52,994.15 | 53,664.11 | 0.988 / 0.998 | 6,241,792 | pass |
| Transparent VP8 RGBA16, normalized | 52,408.00 | 54,009.89 | 0.970 / 0.979 | 6,241,792 | pass |
| Bilinear RGBA16 enlargement | 1,830.73 | 1,884.92 | 0.971 / 1.013 | 4,498,558 | uncertain |

**Word alpha:** one prespecified four→sixteen operation batch addresses uncertainty
from the earlier213ms blocks, .991–1.008 interval and53.417/53.475/53.305ms native
brackets. The new conservative range is **0.96042–0.97894**;
native brackets are54.097, 54.081, 54.010ms. No cause for old drift or gain from
different clocks is inferred. Keep every previous point. No unchanged repeat follows.

The same case-specific driver/harness,12alternating pairs,12native batches on
each side, four untimed alternating warmup cycles, natural GC and worse
paired/all-actual/diagnostic confidence remain. Only complete batch duration
changes. Byte alpha retains its four-operation batches; enlargement retains64.
No grid expansion or acceptance-rule change.

Full16bit normalization/repacking, complete encode, fresh MemoryStream growth,
independent packet ownership and disposal remain timed on both declared sides.
Native FFmpeg/libwebp retains its codec/input/scratch wrappers. This is not a
fresh-context comparison. Both alpha packets remain exactly98,590B, SHA256
8fcc1c935850d7813ebfce329a8b00c41fe9b02683d2be40b91dedfd74192a10.
Source/settings, full channel/alpha/hiddenRGB checks, syntax, normal/scalar CLI,
source-quality sanity and surviving output owners pass. Startup,JIT,IPC,I/O,
fixture generation and validation stay outside clocks. CPU0,one thread,Release,
optimized JIT,nonconcurrent workstation GC and idle-v2 guards pass.

Word-alpha allocation remains **6,241,792B/op** and retained operation scratch
zero. No new production capacity/retention is introduced; its governed peak,
physical/native/JIT peaks and default idle retention are not remeasured.
Unchanged byte-alpha3,656,309Badmitted peak and enlargement geometry/row-cache/
output capacities stay scoped evidence, separate from allocation.

WAC982 uses the actualee4bc932private0.2.1package/b26c35e1DLL. Reuse its
**6,683full/live checks perOS**, focused normal/scalar and SAME actual-package
consumer/all three examples. **Zero builds or broad/package qualification
repeats**. Unchanged byte-alpha/enlargement costs retain their earlier6ef75b81
package producer and qualification. Published releases remain immutable.

[Evidence](warm-adjacent-confidence-evidence.json) pins per-case producers,
qualification, both word-alpha captures and every sample. New mirrored/member-
verified raw: webp-alpha16-long-batch-confidence.tar.xz,
**28924B/14members**, SHA256
016bf4c64d1a72eabe61508f567f4f56572fe6f4ba61532b2c25dcc16b37178d.
Dependencies retain the prior word-alpha capture/driver/harness/reference/fixtures
and current actual package. Original WAC805/BCC806/807 raw580bfd35/a4385a31 remains;
the accepted bilinear18.12%normalized redesign gain is separate from confidence.
Publication and full289-path parity remain open.
