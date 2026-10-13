# Warm PNG stored-Deflate RGBA8 decode

Stored-Deflate RGBA8 PNG decoding independently confirms **1.159× FFmpeg**,
conservative upper **1.168×**, with **12,976 managed bytes allocated per
operation**. The repeat averages 157.30 µs; all 12 actual results are retained and
both quietness guards pass. Production, native worker and package are unchanged.
Stop tuning this fixed case; other material cases and uniform warm parity remain
open.

| Run | Lokad µs | Ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Three-actual screen | 158.60 | 1.166 | 1.314 | Pass |
| Independent twelve-actual confirmation | 157.30 | 1.159 | 1.168 | Pass |

Both subjects decode the same frozen 513×259 RGBA8 PNG from the original raster.
It uses filter None, noninterlaced PNG and stored Deflate blocks from level-0
encoding. The 531,818-byte IDAT payload and full encoded bytes remain unchanged
from the earlier NoCompression encode case; no original image was added.
Preparation structurally verifies stored block types, lengths/complements and
exact scanlines. Every RGBA sample, alpha and hidden RGB, source integrity and
independent held output owners are exact, including normal/scalar FFmpeg CLI.

Managed timing includes public Decode, independent output acquisition and
disposal; released pixel storage may be reused. Native timing includes complete
decode, independent owned frame acquisition and release. Source packet, codec
context and frame wrappers are reused; no native color conversion is needed.
Startup, I/O, IPC, hashes and validation are excluded. Native allocations are
unmeasured. Sequential Release runs use one pinned CPU, one codec thread, thread
type zero, normal SIMD, tiering off and workstation nonconcurrent GC. Native
brackets target 50 ms and managed iterations 200 ms. The screen mean was parsed
before the single independent confirmation.

Only the original benchmark harness adds a new explicit decode selection. Forced
fresh builds and both actual generated children match all 595 production method
bodies and flags of the same verified actual Release package. Prior 2,689
Release/differential tests per OS, 98 focused PNG checks with intrinsics enabled
and disabled per OS and all three same-package examples on Windows and Linux
are reused; full verification is not repeated for unchanged production.

The [numeric evidence](warm-pngstore8decode-evidence.json) retains every actual
managed/native sample, allocation evidence and exact source/child/native
identities. All four actual child files, source, receipts and actual package are
frozen before further preparation or mutation. The
[benchmark summary](../docs/benchmark.md) reports this fixed pair at its measured
source; unmatched compression comparisons, JPEG420's upper-bound gap and the
complete required warm scope remain open.
