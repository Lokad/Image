# Warm PNG palette8 decode with partial transparency

Palette8 PNG decoding with a two-entry tRNS independently confirms **1.953×
FFmpeg**, conservative upper **1.967×**, with **10,728 managed bytes allocated
per operation**. The repeat averages 709.94 µs; all 12 actual results are retained
and both quietness guards pass. Production, native worker and package are
unchanged. Stop tuning this fixed case; uniform warm parity remains open.

| Run | Lokad µs | Ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Three-actual screen | 711.40 | 1.938 | 2.311 | Pass |
| Independent twelve-actual confirmation | 709.94 | 1.953 | 1.967 | Pass |

Both subjects decode the same original 513×259 indexed PNG, filter None and
noninterlaced. Indices come from the existing RGBA8 raster's red channel; its
256-entry RGB palette remains unchanged. tRNS contains alpha 0 and 1 for indices
0 and 1; the remaining 254 entries default to 255. This exercises 272 transparent,
556 alpha-one and 132,039 default-opaque pixels. The existing full-tRNS fixture
remains byte-identical; no original image was added. Every RGBA sample, alpha and
hidden RGB, source integrity and independent held output owners are exact,
including normal/scalar FFmpeg CLI comparisons.

Managed timing includes public Decode, independent output acquisition and
disposal; released pixel storage may be reused. Native timing includes complete
PNG decode, PAL8-to-owned-RGBA conversion and frame release. Source packet,
context and frame wrappers are reused; palette expansion is inside each timed
operation. Startup, I/O, IPC, hashes and validation are excluded. Native
allocations are unmeasured. Sequential Release runs use one pinned CPU, one codec
and conversion thread, normal SIMD, tiering off and workstation nonconcurrent GC.
The three-actual mean was parsed before the single independent confirmation.

Only the benchmark harness changes. Forced fresh builds and both actual generated
children match all 595 production method bodies and flags of the same verified
actual Release package. Prior 2,689 Release/differential tests per OS, 98 focused
PNG normal/scalar checks per OS and all three same-package examples on Windows
and Linux are reused for unchanged production.

The [numeric evidence](warm-pngpal8a2-evidence.json) retains every actual sample,
allocation evidence and source/child/native identities. All four actual child
files, source, receipts and actual package were frozen before further preparation
or mutation. The [benchmark summary](../docs/benchmark.md) reports this fixed
pair at its measured source; other material cases, unmatched compression
comparisons and JPEG420's upper-bound gap remain open.
